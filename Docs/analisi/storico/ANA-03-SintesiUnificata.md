# ANA-03 – Sintesi unificata del sistema MyHome

**Data:** 07/11/2026
**Fonti:** storico\ANA-01-AnalisiGemini.md, storico\ANA-02-SintesiArchitetturaDCC.md, sessione di analisi del 07/11/2026
**Stato:** documento di riferimento per la fase di coding (ANA-01 e ANA-02 spostati in `storico\`)
**Aggiornato il 09/11/2026:** il canale verso i client passa da SignalR a REST + SSE (ANA-05); la console `DCB-CLI`
è eliminata e sostituita dalla raccolta Postman `DCB-PMN` (§8)

---

## 1. Sistema e componenti

**MyHome** è il sistema domotico. Acronimi, convenzione e tabelle di progetti e sottomoduli sono in [`Acronimi.md`](Acronimi.md).

---

## 2. Stack tecnologico

- Linguaggio e framework: **C# / .NET 10 (LTS)**.
- IDE: Visual Studio Community.
- Ambiente di esecuzione: mini PC (`MPC`) **Windows 11**, Docker Desktop su WSL2 per il container deCONZ (`DEZ`).

---

## 3. Struttura della solution

```text
MyHome.sln
├── MyHome.Shared       (SHR) DTO dell'API REST, DcbEventDto, modelli condivisi DCB/TCL/HSPA
├── DCB.Gateway         (DCB) DCB-GTW e DCB-GTW-*: client REST/WS deCONZ, Deconz*Dto – libreria autonoma (§4.6)
├── DCB.Gateway.Tests   (DCB) DCB-TST: test automatici di GTW, senza deCONZ (§8)
├── DCB.Infrastructure  (DCB) storage CFG/DBZ/MDZ (DCB-STO), implementazioni delle interfacce di GTW
├── DCB.Server          (DCB) API REST + flusso SSE (ANA-05), bootstrap auto/sbs, dispatcher eventi, EventLogStore
├── TCL.Client          (TCL) console a 3 zone (Terminal.Gui)
└── HSPA.*              (HSPA) fase successiva
```

**Riferimenti tra progetti**
- `DCB.Server` → `DCB.Infrastructure` → `DCB.Gateway`; `DCB.Infrastructure` → `MyHome.Shared`.
- `DCB.Gateway` → nessun progetto MyHome (solo .NET e astrazioni standard).
- `DCB.Gateway.Tests` → solo `DCB.Gateway`.
- `TCL.Client` → solo `MyHome.Shared`, così è distribuibile da solo su altre postazioni.

**Fuori dalla solution**: `Tools\Postman\` contiene la raccolta Postman `DCB-PMN` (§8).

> Solution e progetti **non sono ancora stati creati**: lo scaffolding è il primo passo della fase di coding.

---

## 4. DCB (Deconz Bridge)

### 4.1 Hosting (`DCB-SVC`)
- Applicazione ASP.NET Core ospitata come **servizio Windows** (`UseWindowsService()`): parte all'avvio del PC senza login.
- In debug si avvia come normale applicazione console.

### 4.2 Modalità di avvio (`DCB-BOT`)
- `DCB.Server --mode auto` (predefinita, produzione): esegue tutta la sequenza di avvio senza intervento umano.
- `DCB.Server --mode sbs` (debug/test): il sistema si mette in attesa; le TCL connesse interrogano lo stato (`status`)
  ed eseguono i singoli passi (`step <stepName>`), anche fuori sequenza.

### 4.3 Autenticazione deCONZ (`DCB-GTW-AUT`)
- **All'avvio non crea una nuova API key**: usa l'ultima utilizzata, letta tramite `IApiKeyStore`
  (in MyHome implementata da `DCB-STO` su `CFG`, vedi §4.6).
- **Verifica**: la chiave viene controllata con una chiamata di prova (es. `GET /api/{key}/config`).
  - Chiave valida → il bootstrap prosegue (avvio di `DCB-GTW-WSS`).
  - Chiave non valida (errore deCONZ 1 *unauthorized user*: rimossa dalla whitelist, gateway cambiato o resettato)
    → il modulo entra in **attesa dello sblocco del gateway**.
- **Attesa dello sblocco**: serve l'intervento umano su Phoscon ("Authenticate app", finestra di ~60 secondi).
  Durante l'attesa il modulo riprova periodicamente `POST /api` finché deCONZ non rilascia una nuova chiave.
  - All'ingresso in attesa `GTW` notifica "attesa sblocco gateway"; in MyHome `DCB.Server` apre la condizione
    nello **stato attuale** (§6), con evento persistente.
- **Nuova chiave ottenuta**: viene salvata tramite `IApiKeyStore` al posto della precedente (in MyHome in `CFG`;
  diventa "l'ultima utilizzata"), `GTW` notifica "API key ottenuta", la condizione viene chiusa con evento di chiusura
  e il bootstrap prosegue.
- Una chiave deCONZ non scade per tempo: diventa non valida solo nei casi sopra.

### 4.4 WebSocket deCONZ (`DCB-GTW-WSS`)
- Servizio in background eseguito da `DCB.Server`, avviato come passo di bootstrap **dopo `DCB-GTW-AUT`**.
- **Porta**: se `deconz.wsPort` in `CFG` è vuota, viene letta da `GET /api/{key}/config` (`websocketport`);
  se è valorizzata ha la precedenza. Serve per deCONZ in container con porta pubblicata diversa da quella interna
  (ANA-04 §3).
- Ogni evento ricevuto aggiorna `DBZ` e passa al dispatcher (`DCB-EVD`).
- Dopo una disconnessione si riconnette con attese crescenti, poi **rilegge via REST lo stato completo**
  per recuperare gli eventi persi.

### 4.5 DTO deCONZ
- I `Deconz*Dto` descrivono l'API di deCONZ e stanno in
  `DCB.Gateway/Dto/{Shared, Lights, Sensors, Groups, Rules, Config, WebSocket}`
  (le cartelle per i nuovi sottomoduli `DCB-GTW-SCH`, `-DEV`, `-ALM`, `-TLK`, `-RLK`, `-INF` sono da definire):
  - Shared: `DeconzBaseDeviceDto`, `DeconzLightStateDto`, `DeconzApiResponseDto`
  - Lights: `DeconzLightDto`
  - Sensors: `DeconzSensorDto` (dizionari dinamici per stati e configurazioni)
  - Groups: `DeconzGroupDto`, `DeconzSceneDto`
  - Rules: `DeconzRuleDto`
  - Config: `DeconzConfigDto`, `DeconzAuthRequestDto`, `DeconzAuthResponseDto`
  - WebSocket: `DeconzWsEventDto`
- Principio DRY: classi base condivise ed ereditarietà.
- In `MyHome.Shared` vanno solo `DcbEventDto`, DTO dell'API REST e modelli dei dispositivi: i client remoti
  non dipendono dal formato dei dati di deCONZ.

### 4.6 Gateway Connector autonomo (`DCB-GTW`)

`GTW` è una **libreria autonoma** (`DCB.Gateway`): in MyHome è usata da `DCB.Server`, ma non dipende dal resto
del sistema. Questo la rende verificabile da sola con i test automatici (`DCB-TST`, §8).

**Dipendenze**: `DCB.Gateway` non dipende da `MyHome.Shared`, dal canale client di `DCB.Server`, da `DCB-STO` o da `CFG`; usa solo .NET e le astrazioni
standard (`HttpClient`, `ILogger`, `IOptions`).

**Punti di contatto sostituibili**

| Esigenza | In `DCB.Gateway` | In MyHome | Nei test (`DCB-TST`) |
| --- | --- | --- | --- |
| Host, porta, opzioni | Classe di opzioni (es. `DeconzGatewayOptions`) | Valori da `cfg.json` tramite `DCB-STO` | Valori di prova |
| API key (`DCB-GTW-AUT`) | Interfaccia `IApiKeyStore` | Implementata da `DCB-STO` su `CFG` | `IApiKeyStore` finto in memoria |
| Attesa sblocco gateway | Eventi/callback ("attesa sblocco", "chiave ottenuta") | `DCB.Server` li trasforma in condizioni dello stato attuale ed eventi verso le TCL (§6) | Verificati dal test |
| Eventi WebSocket (`DCB-GTW-WSS`) | Eventi .NET o `IAsyncEnumerable` di `DeconzWsEventDto` | Verso `DBZ` e `DCB-EVD` | WebSocket finto |
| Log | `ILogger` | Log del servizio | Log dei test |
| Registrazione | `services.AddDeconzGateway(...)` | Chiamata da `DCB.Server` | Chiamata dai test |

```text
In MyHome:  TCL / HSPA / Postman ──(REST + SSE)──▶ DCB.Server ──(metodi, stesso processo)──▶ DCB.Gateway ──(REST/WS)──▶ deCONZ
Prove:      Postman ──(REST/WS)──▶ deCONZ   (comportamento reale del gateway, senza codice MyHome)
```

**Console di verifica `DCB-CLI` eliminata (09/11/2026).** Le sue funzioni sono coperte da (§8):
- `DCB-TST` per la logica di `GTW`, isolata e senza deCONZ;
- la raccolta Postman `DCB-PMN` per le prove reali, sia direttamente su deCONZ sia su `DCB.Server` (`DCB-API`),
  usando la modalità `sbs` per eseguire i passi di bootstrap uno alla volta (§4.2).

---

## 5. Storage (`DCB-STO`)

> Descrizione dettagliata dei file (posizione, nomi, struttura, rilettura in corsa di `MDZ`) in
> [`ANA-04-FileJsonDCB.md`](ANA-04-FileJsonDCB.md).

Tre file JSON, gestiti in modo thread-safe:

1. **`CFG`** – parametri di sistema: IP e porte deCONZ, API key, chiave condivisa per le TCL, modalità di avvio predefinita.
2. **`DBZ`** – specchio grezzo dei dispositivi Zigbee letto da deCONZ.
3. **`MDZ`** – metadati utente, in due sezioni:
   - `devices`: dispositivi mappati e riconosciuti (es. `LIG-SAL-01`, `SEN-ING-02`);
   - `devices-unknown`: auto-discovery dei nuovi dispositivi con codici proposti (`PROPOSED-LIG-XX`, `PROPOSED-SEN-XX`).

---

## 6. Eventi e sessioni (`DCB-API`, `DCB-EVS`, `DCB-EVD`, `DCB-ELS`)

> Il canale verso i client (API REST + flusso SSE) è descritto in [`ANA-05-CanaleClientDCB.md`](ANA-05-CanaleClientDCB.md).

- **Sessioni multiple**: ogni client connesso al flusso SSE è una sessione con un proprio ID e sottoscrizioni isolate (ANA-05 §5).
- **Lazy event emission**: gli eventi ad alta frequenza non vengono creati se nessuno è sottoscritto;
  la creazione è delegata a una factory (`Func<DcbEventDto>`) eseguita solo se `HasSubscribers(topic)` è vero.
- **Eventi persistenti**: gli eventi di sistema rilevanti (avvio/arresto, cambi di configurazione, errori di rete,
  eventi critici) hanno `isPersistent = true` e vengono **sempre** salvati in `EventLogStore`,
  consultabili con `history`.
- **Stato attuale** (condizioni in corso): il DCB mantiene l'elenco delle condizioni aperte che richiedono attenzione
  (es. "attesa sblocco gateway" da `DCB-GTW-AUT`).
  - All'apertura di una condizione: evento persistente inviato alle TCL connesse (`DCB-EVD`) e salvato in `EventLogStore` (`DCB-ELS`).
  - **Alla connessione di una nuova TCL** (apertura del flusso `DCB-EVS`): le viene inviato subito lo stato attuale, così è informata
    anche se si connette dopo l'evento.
  - Alla chiusura: la condizione viene rimossa dallo stato attuale e viene inviato l'evento di chiusura.
  - Gli eventi persistenti sono sempre creati, indipendentemente dalla lazy event emission.
  - Nella TCL le condizioni aperte sono mostrate in evidenza (`TCL-UI`); il messaggio è un `DcbEventDto` (`SHR-EVT`).
  - Il meccanismo è riutilizzabile per altre attese (deCONZ non raggiungibile, ConBee II scollegato,
    riconnessione WebSocket).
- **Sicurezza**: le connessioni TCL sono protette da una **chiave condivisa** (in `CFG` e nella configurazione della TCL),
  inviata nell'header `X-Api-Key` di ogni richiesta REST e SSE (ANA-05 §6).

---

## 7. TCL (`TCL-CON`, `TCL-UI`, `TCL-CMD`)

Console realizzata con **Terminal.Gui**, divisa in tre zone:

- **Zona 1 – Streaming eventi**: eventi in tempo reale dal DCB, con filtri locali.
- **Zona 2 – Input comandi**:
  - `subscribe <topic>` / `unsubscribe <topic>`
  - `step <stepName>` (esecuzione guidata in modalità `sbs`)
  - `status` (stato del bootstrap)
  - `history <categoria> [limite]` (registro degli eventi persistenti)
- **Zona 3 – Output comandi**: esito dettagliato dei comandi inviati.

---

## 8. Gestione del progetto

- Repository **git** con `.gitignore` per .NET / Visual Studio (da creare all'inizio della fase di coding).
- **Progetto di test `DCB.Gateway.Tests` (`DCB-TST`)**, xUnit, dipende solo da `DCB.Gateway`. Verifica la logica di `GTW`
  **senza deCONZ**, in pochi secondi:
  1. **DTO**: lettura di esempi JSON della documentazione deCONZ (salvati come file nel progetto), compresi i sensori
     con campi variabili, gli eventi WebSocket e le risposte con campi mancanti (es. `/config` senza API key);
  2. **client REST** (`DCB-GTW-*`): URL, metodo e corpo delle chiamate, interpretazione di risposte ed errori deCONZ,
     con un `HttpMessageHandler` finto al posto della rete;
  3. **`DCB-GTW-AUT`**: chiave valida → si prosegue; errore 1 → attesa dello sblocco con notifica; nuova chiave → salvata
     tramite un `IApiKeyStore` finto; tempo simulato (`TimeProvider`);
  4. **`DCB-GTW-WSS`**: lettura dei messaggi, scelta della porta (`deconz.wsPort` / `websocketport`), riconnessione,
     con un WebSocket finto.
- **Raccolta Postman (`DCB-PMN`)**, in `Tools\Postman\`, salvata nel repository:
  - `MyHome-deCONZ.postman_collection.json`: richieste dirette a deCONZ (autenticazione, `/config`, lights, sensors,
    groups, comandi semplici, connessione WebSocket), per conoscere il comportamento reale del gateway;
  - `MyHome-DCB.postman_collection.json`: richieste a `DCB-API` e flusso `DCB-EVS` (ANA-05), comprese le chiamate
    di bootstrap per la modalità `sbs`;
  - ambienti (`*.postman_environment.json`, es. "deCONZ locale", "deCONZ container") con le variabili `deconzUrl`,
    `deconzKey`, `dcbUrl`, `dcbKey`. **Le chiavi reali non si salvano nel repository**: negli ambienti restano vuote
    e si impostano solo come valore corrente in Postman;
  - script di controllo sulle risposte (codice, campi attesi) ed esecuzione completa con Collection Runner o Newman.
- **Divisione dei ruoli**: `DCB-TST` verifica la logica di `GTW`; `DCB-PMN` verifica la comunicazione reale con deCONZ
  e il comportamento di `DCB.Server`. La console `DCB-CLI` prevista in precedenza è stata eliminata il 09/11/2026 (§4.6).
- Test per `DCB.Infrastructure` e `DCB.Server`: da aggiungere quando verranno sviluppati.

---

## 9. Questioni in sospeso

> L'elenco aggiornato dei punti aperti, con il loro stato, è in [`PuntiApertiAnalisi.md`](PuntiApertiAnalisi.md)
> (decisioni e analisi) e in [`PuntiApertiCoding.md`](../coding/PuntiApertiCoding.md) (attività di coding).

1. **deCONZ (`DEZ`)**
   - **Attuale: deCONZ in locale** come applicazione Windows sul mini PC (`MPC`), stessa macchina del DCB.
     Verifica del 07/11/2026:
     - `deCONZ.exe` 2.33.2 (API 1.16.0), installato per utente in `C:\Users\Andrea\AppData\Local\Programs\deCONZ\`;
     - porte in ascolto su `0.0.0.0`: **80** (REST) e **443** (WebSocket, confermata il 09/11/2026 con `websocketport` di `/api/{key}/config`);
     - ConBee II collegato su **COM3** (`USB\VID_1CF1&PID_0030`), riconosciuto da deCONZ (`devicename`: "ConBee II");
     - `GET http://127.0.0.1/api/config` senza API key: 200 OK, `bridgeid` `00212EFFFF051A5C`;
     - endpoint `/devices` e `/alarmsystems` disponibili in questa versione (`DCB-GTW-DEV`, `DCB-GTW-ALM`).
   - **Note sul locale**: deCONZ è un'applicazione e parte solo dopo il login, mentre il DCB (servizio) parte prima:
     caso "deCONZ non raggiungibile" gestito dallo stato attuale (§6). deCONZ è raggiungibile da tutta la rete locale.
   - **Futuro: deCONZ in container.** Il DCB è già predisposto: indirizzo e porte in `CFG`, `deconz.wsPort` per la porta
     WebSocket pubblicata (§4.4), API key rigenerabile (§4.3), verifica preventiva con la raccolta Postman `DCB-PMN` (§8). Da progettare:
     - compose con `deconzcommunity/deconz`, dispositivo `/dev/ttyACM0`, porte REST/WS pubblicate;
     - ConBee II verso WSL con `usbipd attach --wsl --auto-attach` all'avvio del PC (usbipd-win già installato);
     - **volume persistente** per i dati di deCONZ (database, rete Zigbee, chiavi);
     - migrazione della rete Zigbee dal deCONZ locale (backup/ripristino da Phoscon).
   - Docker Desktop attivo su WSL2, nessun container deCONZ presente; l'immagine `zigbeeservice:dev` è solo un'immagine
     di debug di Visual Studio (progetto `ZigBeeService`) e non contiene deCONZ.
2. **Progetto `ZigBeeService`**: eventuale recupero del codice (non presente nella cartella MyHome).
3. **Acronimi dei sottomoduli** in `Acronimi.md` §3 (`SHR-*`, `DCB-*`, `TCL-*`): **confermati il 07/11/2026**.
4. **Argomenti non ancora analizzati**:
   - convenzione dei codici dispositivo in `MDZ` (tipi, stanze, numerazione);
   - elenco e ordine dei passi di bootstrap e gestione degli errori;
   - topic e categorie degli eventi, quali eventi sono persistenti;
   - ~~formato e posizione su disco dei file `CFG`, `DBZ`, `MDZ`~~ → descritti in ANA-04.
5. **`GTW` autonomo (§4.6)**:
   - progetto `DCB.Gateway` registrato in `Acronimi.md`;
   - progetto di test: **previsto** `DCB.Gateway.Tests` (`DCB-TST`), deciso il 07/11/2026 (§8);
   - ~~console di verifica `DCB.Gateway.Console` (`DCB-CLI`)~~ → eliminata il 09/11/2026, sostituita dalla raccolta
     Postman `DCB-PMN` (§8).

---

## 10. Prossimi passi (fase coding)

1. `git init` + `.gitignore`; creazione di `MyHome.sln` e dei progetti .NET 10 con i riferimenti (§3);
   prima versione della raccolta Postman su deCONZ (`DCB-PMN`, §8), utile già durante lo sviluppo di `GTW`.
2. Storage `CFG` / `DBZ` / `MDZ` (`DCB-STO`).
3. `DCB-GTW-AUT` e bootstrap `auto` / `sbs` (`DCB-BOT`).
4. API REST (`DCB-API`) e flusso SSE (`DCB-EVS`) con chiave condivisa + `TCL.Client` a 3 zone (ANA-05);
   raccolta Postman su `DCB.Server` (`DCB-PMN`).
5. `DCB-GTW-WSS` e dispatcher eventi (`DCB-EVD`).
