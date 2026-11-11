# ANA-06 – Sintesi unificata del sistema MyHome

**Data:** 11/11/2026
**Fonti:** storico\ANA-03-SintesiUnificata.md (che a sua volta fondeva ANA-01 e ANA-02), sessione di analisi dell'11/11/2026
**Stato:** documento di riferimento per la fase di coding; sostituisce ANA-03 (spostato in `storico\`)
**Novità rispetto ad ANA-03** (decisioni dell'11/11/2026, punti PA-04, PA-13…PA-17):
- sequenza dei passi di bootstrap (§4.2);
- nuovo sottomodulo `DCB-SYN` per la sincronizzazione dello stato (§4.7) e notifica `ConnectionStateChanged` di `WSS` (§4.4);
- nuovo sottomodulo `DCB-GTW-FUL` per lo stato completo, DTO delle richieste (§4.5);
- ampiezza della prima versione di `GTW` (§4.6) e prossimi passi riallineati (§10);
- parametri: `appsettings.json` + `cfg.json` + nuovo `apikey.json` (`KEY`) (§4.8, dettagli in ANA-07).

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
├── DCB.Infrastructure  (DCB) storage CFG/KEY/DBZ/MDZ (DCB-STO), implementazioni delle interfacce di GTW
├── DCB.Server          (DCB) API REST + flusso SSE (ANA-05), bootstrap auto/sbs, sincronizzazione stato (DCB-SYN),
│                             dispatcher eventi, EventLogStore
├── TCL.Client          (TCL) console a 3 zone (Terminal.Gui)
└── HSPA.*              (HSPA) fase successiva
```

**Riferimenti tra progetti**
- `DCB.Server` → `DCB.Infrastructure` → `DCB.Gateway`; `DCB.Infrastructure` → `MyHome.Shared`.
- `DCB.Gateway` → nessun progetto MyHome (solo .NET e astrazioni standard).
- `DCB.Gateway.Tests` → solo `DCB.Gateway`.
- `TCL.Client` → solo `MyHome.Shared`, così è distribuibile da solo su altre postazioni.

**Fuori dalla solution**: `Tools\Postman\` contiene la raccolta Postman `DCB-PMN` (§8).

> Solution e progetti sono stati creati il 09/11/2026 (comandi in `Docs\coding\RegistroComandi.md`).

---

## 4. DCB (Deconz Bridge)

### 4.1 Hosting (`DCB-SVC`)
- Applicazione ASP.NET Core ospitata come **servizio Windows** (`UseWindowsService()`): parte all'avvio del PC senza login.
- In debug si avvia come normale applicazione console.

### 4.2 Modalità di avvio (`DCB-BOT`)
- `DCB.Server --mode auto` (predefinita, produzione): esegue tutta la sequenza di avvio senza intervento umano.
- `DCB.Server --mode sbs` (debug/test): il sistema si mette in attesa; le TCL connesse interrogano lo stato (`status`)
  ed eseguono i singoli passi (`step <stepName>`), anche fuori sequenza.

**Sequenza dei passi** *(approvata l'11/11/2026, PA-04)*

| # | Passo (`stepName`) | Cosa fa | Sottomodulo | Esito in caso di problema |
| --- | --- | --- | --- | --- |
| 0 | *(prima del bootstrap)* | Avvio dell'host, di `DCB-API` e di `DCB-EVS`: le TCL possono collegarsi subito, vedere le condizioni aperte e, in `sbs`, guidare il bootstrap | `DCB-SVC` | – |
| 1 | `config` | Lettura della configurazione (§4.8) e impostazione di `GTW` (`DeconzGatewayOptions`) | `DCB-STO` | **Bloccante**: condizione aperta, stop, serve il riavvio |
| 2 | `storage` | Caricamento di `MDZ` e della cache `DBZ` | `DCB-STO` | **Degradato**: `MDZ` non valido → condizione, si prosegue senza mappature; `DBZ` non valido → ignorato e ricostruito al passo 6 |
| 3 | `gateway` | Verifica che deCONZ risponda (`GET /api/config`, senza chiave) | `DCB-GTW` | **Attesa**: condizione "deCONZ non raggiungibile", nuovi tentativi, chiusura automatica |
| 4 | `auth` | Verifica della chiave ed eventuale attesa dello sblocco (§4.3); la verifica (`GET /api/{key}/config`) fornisce anche `websocketport` e versione | `DCB-GTW-AUT` | **Attesa**: condizione "attesa sblocco gateway" |
| 5 | `events` | Avvio del WebSocket (§4.4) | `DCB-GTW-WSS` | Attesa e riconnessione |
| 6 | `sync` | Lettura completa via REST → `DBZ`, auto-discovery verso `MDZ` (§4.7) | `DCB-SYN`, `DCB-GTW-FUL` | Errori di rete → come il passo 3 |
| 7 | `ready` | Stato "operativo", evento persistente "DCB avviato" | `DCB-BOT` | – |

- **`events` prima di `sync`**: collegando il WebSocket prima della lettura completa non si perdono gli eventi arrivati
  nel frattempo; è lo stesso schema usato dopo ogni riconnessione (§4.7).
- **Categorie di errore**: *bloccanti* (stop, serve il riavvio), *attese recuperabili* (condizione aperta, nuovi tentativi,
  chiusura automatica), *degradati* (condizione o avviso, si prosegue).
- Ogni passo ha uno stato (*non eseguito*, *in corso*, *in attesa*, *completato*, *fallito*), restituito da
  `GET /api/bootstrap` (ANA-05 §4).
- Ancora da decidere (PA-04): chi ripete i tentativi quando deCONZ non è raggiungibile (proposta: `GTW` fa un solo
  tentativo, `DCB-BOT` ripete), regole della modalità `sbs` (rilancio dei passi, prerequisiti, nessun tentativo
  automatico), valori delle attese crescenti.

### 4.3 Autenticazione deCONZ (`DCB-GTW-AUT`)
- **All'avvio non crea una nuova API key**: usa l'ultima utilizzata, letta tramite `IApiKeyStore`
  (in MyHome implementata da `DCB-STO` sul file `KEY`, `apikey.json`, vedi §4.6 e §4.8).
- **Verifica**: la chiave viene controllata con una chiamata di prova (es. `GET /api/{key}/config`).
  - Chiave valida → il bootstrap prosegue (avvio di `DCB-GTW-WSS`).
  - Chiave non valida (errore deCONZ 1 *unauthorized user*: rimossa dalla whitelist, gateway cambiato o resettato)
    → il modulo entra in **attesa dello sblocco del gateway**.
- **Attesa dello sblocco**: serve l'intervento umano su Phoscon ("Authenticate app", finestra di ~60 secondi).
  Durante l'attesa il modulo riprova periodicamente `POST /api` finché deCONZ non rilascia una nuova chiave.
  - All'ingresso in attesa `GTW` notifica "attesa sblocco gateway"; in MyHome `DCB.Server` apre la condizione
    nello **stato attuale** (§6), con evento persistente.
- **Nuova chiave ottenuta**: viene salvata tramite `IApiKeyStore` al posto della precedente (in MyHome in `KEY`;
  diventa "l'ultima utilizzata"), `GTW` notifica "API key ottenuta", la condizione viene chiusa con evento di chiusura
  e il bootstrap prosegue.
- Una chiave deCONZ non scade per tempo: diventa non valida solo nei casi sopra.

### 4.4 WebSocket deCONZ (`DCB-GTW-WSS`)
- Servizio in background eseguito da `DCB.Server`, avviato come passo di bootstrap **dopo `DCB-GTW-AUT`**.
- **Porta**: se il parametro `Deconz:WsPort` è vuoto, viene letta da `GET /api/{key}/config` (`websocketport`);
  se è valorizzato ha la precedenza. Serve per deCONZ in container con porta pubblicata diversa da quella interna
  (ANA-07 §3).
- Gli eventi ricevuti sono esposti come flusso (`IAsyncEnumerable<DeconzWsEventDto>`); `WSS` non aggiorna `DBZ`:
  lo fa `DCB-SYN` (§4.7), che li passa anche al dispatcher (`DCB-EVD`).
- Dopo una disconnessione si riconnette con attese crescenti.
- **Notifica `ConnectionStateChanged`** *(decisa l'11/11/2026, PA-13)* con tre stati: *collegato*, *scollegato*,
  *in riconnessione*. Serve a:
  - avviare la rilettura dello stato completo da parte di `DCB-SYN` al ritorno a *collegato* (esclusa la prima
    connessione, coperta dal passo `sync` del bootstrap);
  - aprire e chiudere la condizione "riconnessione WebSocket" nello stato attuale (§6).
- `WSS` **non rilegge** lo stato via REST: la rilettura è compito di `DCB-SYN` (§4.7).

### 4.5 DTO deCONZ
- I `Deconz*Dto` descrivono l'API di deCONZ e stanno in
  `DCB.Gateway/Dto/{Shared, Lights, Sensors, Groups, Rules, Config, FullState, WebSocket}`
  (le cartelle per i sottomoduli rinviati `DCB-GTW-SCH`, `-DEV`, `-ALM`, `-TLK`, `-RLK`, `-INF` sono da definire):
  - Shared: `DeconzBaseDeviceDto`, `DeconzLightStateDto`, `DeconzApiResponseDto`, `DeconzLightStateRequestDto`
  - Lights: `DeconzLightDto`
  - Sensors: `DeconzSensorDto` (dizionari dinamici per stati e configurazioni), `DeconzSensorConfigRequestDto`
  - Groups: `DeconzGroupDto`, `DeconzSceneDto`, `DeconzGroupActionRequestDto`
  - Rules: `DeconzRuleDto`
  - Config: `DeconzConfigDto`, `DeconzAuthRequestDto`, `DeconzAuthResponseDto`
  - FullState: `DeconzFullStateDto` *(nuovo, PA-14)*
  - WebSocket: `DeconzWsEventDto`
- Principio DRY: classi base condivise ed ereditarietà.
- **Stato completo** (`GET /api/{key}`, sottomodulo nuovo `DCB-GTW-FUL`, *deciso l'11/11/2026, PA-14*):
  `DeconzFullStateDto` è composto dai DTO esistenti (`DeconzConfigDto` e dizionari di `DeconzLightDto`,
  `DeconzSensorDto`, `DeconzGroupDto` indicizzati per id deCONZ). Le sezioni `rules` e `schedules` per ora non sono
  mappate (si aggiungono con `DCB-GTW-RUL` / `-SCH`). La `whitelist` contenuta in `config` (tutte le API key
  registrate) **non va mai nei log né in `DBZ`**.
- **DTO delle richieste** *(decisi l'11/11/2026, PA-15)*:
  - deCONZ applica ogni campo presente nel corpo: tutte le proprietà sono *nullable* e i campi `null` non vengono
    serializzati (`JsonIgnoreCondition.WhenWritingNull`), così si inviano solo i campi valorizzati;
  - `DeconzLightStateRequestDto` (campi comuni a luci e gruppi: `on`, `bri`, `hue`, `sat`, `ct`, `xy`, `alert`,
    `effect`, `transitiontime`…) → `DeconzGroupActionRequestDto` eredita e aggiunge `toggle`;
  - `DeconzSensorConfigRequestDto` basato su un dizionario di campi (variano con il tipo di sensore); un DTO
    tipizzato per le valvole TRV si aggiungerà con la loro mappatura in `MDZ`;
  - comandi senza corpo (es. richiamo di una scena): solo un metodo del client, senza DTO;
  - nomi: suffisso `RequestDto` per le richieste, `Dto` per le risposte, nella cartella dell'area;
  - **unità deCONZ** in `GTW` (`bri` 0–255, `ct` in mired, `transitiontime` in decimi di secondo, indicate nei
    commenti `///`); conversione dalle unità MyHome e validazione dei valori inseriti in `DCB.Server` (`DCB-API`).
    `GTW` non valida gli intervalli: gli errori deCONZ (es. tipo 7 *invalid value*) arrivano tramite
    `DeconzApiException` o `DeconzCommandResult`;
  - esito dei comandi: `DeconzCommandResult`, con l'elenco dei campi applicati (indirizzo e valore) e degli errori.
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
| Host, porta, opzioni | `DeconzGatewayOptions` | Sezione `Deconz` della configurazione (`appsettings.json` + `cfg.json`, §4.8) | Valori di prova |
| API key (`DCB-GTW-AUT`) | Interfaccia `IApiKeyStore` | Implementata da `DCB-STO` su `KEY` (`apikey.json`) | `IApiKeyStore` finto in memoria |
| Attesa sblocco gateway | Eventi .NET su `IDeconzAuthenticator` (`WaitingForUnlock`, `ApiKeyObtained`) | `DCB.Server` li trasforma in condizioni dello stato attuale ed eventi verso le TCL (§6) | Verificati dal test |
| Eventi WebSocket (`DCB-GTW-WSS`) | `IAsyncEnumerable<DeconzWsEventDto>` + `ConnectionStateChanged` (§4.4) | `DCB-SYN` → `DBZ` e `DCB-EVD` (§4.7) | WebSocket finto |
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

**Prima versione di `GTW`** *(decisa l'11/11/2026, PA-16)*. Criterio: entra ciò che serve al bootstrap, a `DCB-SYN`
o ai comandi di `DCB-API`; le letture di `DCB-API` arrivano da `DBZ`, quindi dei client per area servono soprattutto
i comandi (elenchi e dettagli restano utili per prove e debug).

| Sottomodulo | Prima versione | Note |
| --- | --- | --- |
| `DCB-GTW` (base), `-CFG`, `-AUT`, `-FUL`, `-WSS` | sì | bootstrap e sincronizzazione |
| `DCB-GTW-LIG`, `-SEN`, `-GRP` | sì | comandi; per `SEN` configurazione a dizionario (§4.5) |
| `DCB-GTW-SCN` | sì, solo il richiamo | creazione e salvataggio delle scene rinviati |
| `DCB-GTW-DEV` | rinviato | da rivedere con PA-08: se `MDZ` mappa gli apparecchi fisici, le risorse dello stesso apparecchio si possono raggruppare anche con la parte MAC dello `uniqueid` |
| `DCB-GTW-RUL`, `-SCH` | rinviati | la collocazione della logica delle automazioni di MyHome non è ancora analizzata |
| `DCB-GTW-ALM`, `-TLK`, `-RLK`, `-INF` | rinviati | Phoscon basta / uso marginale |

Organizzazione del progetto e scelte tecniche: `Docs\coding\COD-02-GatewayConnector.md`.

### 4.7 Sincronizzazione dello stato (`DCB-SYN`, nuovo)

*Deciso l'11/11/2026 (PA-13).* Sottomodulo di `DCB.Server` che tiene `DBZ` allineato a deCONZ. `GTW` resta un client
senza logica di allineamento: avvisa con `ConnectionStateChanged` (§4.4), `DCB-SYN` rilegge.

- **Lettura completa** con `DCB-GTW-FUL` (§4.5), usata sia dal passo `sync` del bootstrap sia dopo ogni
  riconnessione del WebSocket, **sempre**, qualunque sia la durata della disconnessione (deCONZ non ripete gli eventi persi).
- **Eventi WebSocket** applicati a `DBZ` e passati al dispatcher `DCB-EVD`.
- **Auto-discovery** dei nuovi dispositivi verso `MDZ` (ANA-07 §5.2).
- Lettura completa ed eventi scrivono sullo stesso `DBZ` in memoria: `DCB-SYN` li applica **uno alla volta**
  (coda o blocco unico).
- **Rilettura fallita** (REST non risponde con WebSocket collegato): condizione "stato non allineato" nello stato
  attuale (§6), nuovi tentativi con le attese crescenti del bootstrap (§4.2); la condizione si chiude alla prima
  lettura riuscita.
- **Avviso ai client**: dopo una rilettura un unico evento "stato riallineato"; i client rileggono via `DCB-API`.
  Un avviso per singolo dispositivo cambiato (confronto vecchio/nuovo) è rinviato a PA-05 (topic degli eventi).

### 4.8 Parametri (`appsettings.json`, `CFG`, `KEY`)

*Deciso l'11/11/2026 (PA-17).* Un solo meccanismo di lettura, le classi Options (`IOptions<T>`) come da `CLAUDE.md`,
alimentate da file in cascata. Dettagli in ANA-07.

| File | Dove | Versionato | Contenuto | Chi scrive |
| --- | --- | --- | --- | --- |
| `appsettings.json` | cartella di installazione | sì | **Tutti** i parametri, tecnici e d'impianto, con valori predefiniti e commenti | sviluppatore |
| `cfg.json` (`CFG`) | `config\` della cartella dati | no | Solo le **differenze locali** (stesse sezioni e chiavi di `appsettings.json`, letto dopo e quindi prevale) e la **chiave condivisa delle TCL** | utente, a DCB fermo |
| `apikey.json` (`KEY`, nuovo) | `config\` della cartella dati | no | API key deCONZ e data di rilascio | solo `DCB-STO`, tramite `IApiKeyStore` |

- `appsettings.json` non contiene segreti né valori scritti dal programma: è versionato e viene sovrascritto
  a ogni pubblicazione.
- Il DCB non riscrive mai `cfg.json`, così i commenti dell'utente si conservano.
- La porta di ascolto di `DCB.Server` è la configurazione standard di Kestrel in `appsettings.json`, sovrascrivibile
  in `cfg.json` (PA-07).
- `TCL.Client` segue lo stesso schema: indirizzo del DCB in `appsettings.json`, chiave condivisa in un file locale
  non versionato.
- In sviluppo la cartella `config\` locale è esclusa da git (`.gitignore`).

---

## 5. Storage (`DCB-STO`)

> Descrizione dettagliata dei file (posizione, nomi, struttura, rilettura in corsa di `MDZ`) in
> [`ANA-07-FileJsonDCB.md`](ANA-07-FileJsonDCB.md).

Quattro file JSON, gestiti in modo thread-safe:

1. **`CFG`** – differenze locali rispetto ad `appsettings.json` e chiave condivisa per le TCL (§4.8).
2. **`KEY`** – API key deCONZ e data di rilascio, scritta solo da `DCB-STO` (§4.8).
3. **`DBZ`** – specchio grezzo dei dispositivi Zigbee letto da deCONZ.
4. **`MDZ`** – metadati utente, in due sezioni:
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
    riconnessione WebSocket, stato non allineato dopo una rilettura fallita, §4.7).
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

- Repository **git** con `.gitignore` per .NET / Visual Studio (creato il 09/11/2026); la cartella `config\` locale è esclusa (§4.8).
- **Progetto di test `DCB.Gateway.Tests` (`DCB-TST`)**, xUnit, dipende solo da `DCB.Gateway`. Verifica la logica di `GTW`
  **senza deCONZ**, in pochi secondi:
  1. **DTO**: lettura di esempi JSON della documentazione deCONZ (salvati come file nel progetto), compresi i sensori
     con campi variabili, gli eventi WebSocket, lo stato completo (`DeconzFullStateDto`) e le risposte con campi mancanti
     (es. `/config` senza API key); serializzazione dei DTO delle richieste senza i campi `null`;
  2. **client REST** (`DCB-GTW-*`): URL, metodo e corpo delle chiamate, interpretazione di risposte ed errori deCONZ,
     con un `HttpMessageHandler` finto al posto della rete;
  3. **`DCB-GTW-AUT`**: chiave valida → si prosegue; errore 1 → attesa dello sblocco con notifica; nuova chiave → salvata
     tramite un `IApiKeyStore` finto; tempo simulato (`TimeProvider`);
  4. **`DCB-GTW-WSS`**: lettura dei messaggi, scelta della porta (`Deconz:WsPort` / `websocketport`), riconnessione, `ConnectionStateChanged`,
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
     - porte in ascolto su `0.0.0.0`: **80** (REST) e **443**. *Correzione dell'11/11/2026*: lo stato completo reale riporta
       `websocketport: 80` (WebSocket `ws://`, stessa porta della REST) e `websocketport_wss: 443` (WebSocket cifrato `wss://`);
       il 09/11/2026 la 443 era stata attribuita per errore a `websocketport`. Quale usare: PA-19;
     - ConBee II collegato su **COM3** (`USB\VID_1CF1&PID_0030`), riconosciuto da deCONZ (`devicename`: "ConBee II");
     - `GET http://127.0.0.1/api/config` senza API key: 200 OK, `bridgeid` `00212EFFFF051A5C`;
     - endpoint `/devices` e `/alarmsystems` disponibili in questa versione (`DCB-GTW-DEV`, `DCB-GTW-ALM`).
   - **Note sul locale**: deCONZ è un'applicazione e parte solo dopo il login, mentre il DCB (servizio) parte prima:
     caso "deCONZ non raggiungibile" gestito dallo stato attuale (§6). deCONZ è raggiungibile da tutta la rete locale.
   - **Futuro: deCONZ in container.** Il DCB è già predisposto: indirizzo e porte nei parametri (§4.8), `Deconz:WsPort` per la porta
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
   - ~~elenco e ordine dei passi di bootstrap~~ → sequenza approvata l'11/11/2026 (§4.2); restano le regole di PA-04;
   - topic e categorie degli eventi, quali eventi sono persistenti;
   - ~~formato e posizione su disco dei file `CFG`, `DBZ`, `MDZ`~~ → descritti in ANA-07 (con il nuovo `KEY`).
5. **`GTW` autonomo (§4.6)**:
   - progetto `DCB.Gateway` registrato in `Acronimi.md`;
   - progetto di test: **previsto** `DCB.Gateway.Tests` (`DCB-TST`), deciso il 07/11/2026 (§8);
   - ~~console di verifica `DCB.Gateway.Console` (`DCB-CLI`)~~ → eliminata il 09/11/2026, sostituita dalla raccolta
     Postman `DCB-PMN` (§8).

---

## 10. Prossimi passi (fase coding)

*Riallineati l'11/11/2026 (PA-16) con i passi di bootstrap (§4.2), `DCB-SYN` (§4.7) e la prima versione di `GTW` (§4.6).*

1. ~~`git init` + `.gitignore`; creazione di `MyHome.sln` e dei progetti .NET 10 con i riferimenti (§3);
   prima versione della raccolta Postman su deCONZ (`DCB-PMN`, §8)~~ → fatto il 09/11/2026.
2. Parametri (`appsettings.json`, §4.8) e storage `CFG` / `KEY` / `DBZ` / `MDZ` (`DCB-STO`).
3. `GTW` base, `DCB-GTW-CFG`, `DCB-GTW-AUT` (incremento 1 di COD-02) e `DCB-GTW-FUL` (incremento 2);
   bootstrap `auto` / `sbs` (`DCB-BOT`) fino al passo `auth`.
4. API REST (`DCB-API`) e flusso SSE (`DCB-EVS`) con chiave condivisa + `TCL.Client` a 3 zone (ANA-05);
   comandi di `DCB-GTW-LIG`, `-SEN`, `-GRP`, `-SCN` (incremento 3); `DCB-SYN` con il passo `sync`;
   raccolta Postman su `DCB.Server` (`DCB-PMN`).
5. `DCB-GTW-WSS` con `ConnectionStateChanged` (incremento 4), passo `events` e dispatcher eventi (`DCB-EVD`);
   rilettura dopo le riconnessioni (`DCB-SYN`).

Fino al loro passo, i passi di bootstrap non ancora sviluppati risultano "non disponibili" in modalità `sbs`.
