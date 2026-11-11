# Punti aperti di analisi

**Aggiornato al:** 11/11/2026
**Stato:** documento permanente (nome fisso, senza progressivo), aggiornato ogni volta che un punto di analisi si apre,
cambia o si chiude

---

Elenco dei punti che richiedono ancora una **decisione o un'analisi**. I punti operativi della fase di coding sono in
[`PuntiApertiCoding.md`](../coding/PuntiApertiCoding.md).

**Identificativi**: la numerazione `PA-nn` è **unica per i due documenti** (un ID non si riutilizza e non si duplica):
per un nuovo punto si usa il numero successivo al più alto presente in uno dei due.
Quando un punto è deciso passa nella sezione 2, con data, esito e documento in cui la decisione è riportata.

**Colonna "Blocca"**: indica il passo di coding (ANA-06 §10) o il componente che non può partire finché il punto è aperto.

## 1. Punti aperti

| ID | Punto | Origine | Progetto / sottomodulo | Blocca | Aperto il |
| --- | --- | --- | --- | --- | --- |
| PA-03 | **Strutture dei file `CFG`, `KEY`, `DBZ`, `MDZ`**: ANA-07 è una bozza con parti *proposta* da confermare (riveduta l'11/11/2026 dopo la decisione su PA-17) | ANA-04 → ANA-07 | `DCB-STO` *(già previsto)* | passo 2 (storage) | 07/11/2026 |
| PA-04 | **Passi di bootstrap** – sequenza **approvata l'11/11/2026** (ANA-06 §4.2). Restano da decidere: (a) deCONZ non raggiungibile, *proposta*: `GTW` fa un solo tentativo con un'eccezione distinta (`DeconzUnreachableException`), `DCB-BOT` ripete; (b) regole della modalità `sbs`, *proposta*: passi rilanciabili, fuori sequenza con errore esplicito se mancano prerequisiti, nessun tentativo automatico; (c) valori delle attese crescenti (es. 5 s, 10 s, 20 s… fino a 60 s). *Nel codice di GTW (incremento 1) il punto (a) è applicato in modo provvisorio: `DeconzUnreachableException`, nessun nuovo tentativo dentro GTW* | ANA-03 §9, COD-02 | `DCB-BOT`, `DCB-GTW-AUT` *(già previsti)* | passo 3 (`DCB-GTW-AUT` e bootstrap) | 07/11/2026 |
| PA-05 | **Topic e categorie degli eventi**, quali sono persistenti; comprende l'eventuale avviso per singolo dispositivo dopo una rilettura (ANA-06 §4.7) | ANA-03 §9 | `DCB-EVD`, `DCB-ELS`, `SHR-EVT`, `DCB-SYN` | passo 5 (dispatcher eventi) | 07/11/2026 |
| PA-06 | **Elenco dettagliato degli indirizzi REST** di `DCB-API` | ANA-05 §8 | `DCB-API` *(già previsto)* | passo 4 (API + TCL) | 09/11/2026 |
| PA-07 | **Porta di ascolto di `DCB.Server`**: collocazione decisa l'11/11/2026 con PA-17 (configurazione Kestrel in `appsettings.json`, sovrascrivibile in `cfg.json`, ANA-07 §3.1); resta da scegliere il valore | ANA-05 §8 | `DCB-API` *(già previsto)* | passo 4 | 09/11/2026 |
| PA-08 | **Convenzione dei codici dispositivo** in `MDZ` (tipi, stanze, numerazione); alla chiusura rivedere il rinvio di `DCB-GTW-DEV` (ANA-06 §4.6) | ANA-03 §9 | `DCB-STO`, `DCB-GTW-DEV` *(già previsti)* | – | 07/11/2026 |
| PA-09 | **SSE dalla HSPA**: nel browser il modo standard di leggere SSE non permette l'intestazione `X-Api-Key`; scegliere tra `fetch`, token nella query string o cookie | ANA-05 §8 | `DCB-EVS`, `HSPA` *(già previsti)* | HSPA | 09/11/2026 |
| PA-10 | **HTTPS** per `DCB.Server`, da valutare prima dell'uso fuori casa | ANA-05 §8 | `DCB-API`, `DCB-EVS` *(già previsti)* | uso remoto | 09/11/2026 |
| PA-11 | **deCONZ in container**: compose, ConBee II verso WSL con `usbipd`, volume persistente, migrazione della rete Zigbee | ANA-03 §9 | `DEZ` *(già previsto)* | – (oggi deCONZ in locale) | 07/11/2026 |
| PA-12 | **Progetto `ZigBeeService`**: decidere se recuperare il codice | ANA-03 §9 | – | – | 07/11/2026 |

## 2. Punti chiusi

| ID | Punto | Chiuso il | Esito | Riportato in |
| --- | --- | --- | --- | --- |
| – | Porta WebSocket di deCONZ locale | 09/11/2026 | 443, confermata con `websocketport` di `/api/{key}/config`. *Chiuso prima della creazione di questo documento, senza ID.* | ANA-06 §9 |
| PA-13 | Rilettura dello stato dopo una riconnessione del WebSocket | 11/11/2026 | `GTW` notifica `ConnectionStateChanged` (*collegato*, *scollegato*, *in riconnessione*); rilegge il nuovo sottomodulo `DCB-SYN` di `DCB.Server`, sempre, applicando lettura ed eventi uno alla volta; rilettura fallita → condizione "stato non allineato"; ai client un unico evento "stato riallineato" | ANA-06 §4.4, §4.7; ANA-07 §4.2; `Acronimi.md` |
| PA-14 | Sottomodulo dello stato completo (`GET /api/{key}`) | 11/11/2026 | Nuovo sottomodulo `DCB-GTW-FUL`; `DeconzFullStateDto` composto dai DTO esistenti, `rules` e `schedules` rinviate; la `whitelist` non va mai nei log né in `DBZ` | ANA-06 §4.5; `Acronimi.md` |
| PA-15 | DTO delle richieste (comandi) | 11/11/2026 | Proprietà *nullable* con i `null` esclusi dal JSON; `DeconzLightStateRequestDto` (con `lift`, `open`, `stop` per le tapparelle, aggiunti l'11/11/2026) → `DeconzGroupActionRequestDto`; `DeconzSensorConfigRequestDto` a dizionario (TRV tipizzato rinviato); suffisso `RequestDto` nella cartella dell'area; unità deCONZ in `GTW`, conversione e validazione in `DCB.Server`; esito `DeconzCommandResult` | ANA-06 §4.5; COD-02 |
| PA-16 | Ampiezza della prima versione di `GTW` | 11/11/2026 | Base, `CFG`, `AUT`, `FUL`, `WSS`, `LIG`, `SEN`, `GRP`, `SCN` (solo richiamo); rinviati `DEV` (da rivedere con PA-08), `RUL`, `SCH`, `ALM`, `TLK`, `RLK`, `INF`; quattro incrementi di sviluppo e prossimi passi riallineati | ANA-06 §4.6, §10; COD-02 |
| PA-17 | Dove stanno i parametri | 11/11/2026 | Un solo meccanismo (classi Options) e tre file: `appsettings.json` (tutti i parametri, versionato), `cfg.json` (`CFG`: differenze locali e chiave delle TCL, mai riscritto dal DCB), `apikey.json` (nuovo `KEY`: API key deCONZ, scritto da `DCB-STO`); stesso schema per `TCL.Client` | ANA-06 §4.8; ANA-07 §1–3; `Acronimi.md` |
| PA-19 | Porta del WebSocket deCONZ | 11/11/2026 | Prova sull'impianto: `ws://127.0.0.1:80` accetta la connessione, `ws://` e `wss://` sulla 443 la rifiutano (deCONZ tiene la porta aperta ma non risponde). Decisione: `DCB-GTW-WSS` usa `ws://` sulla porta `websocketport` letta da `/config` (oggi 80), sovrascrivibile con `Deconz:WsPort`; niente `wss://` (stessa macchina, certificato autofirmato). Ambiente Postman "deCONZ locale" corretto in `ws://127.0.0.1:80`. Verificato anche che deCONZ risponde ai ping WebSocket | ANA-06 §4.4, §9; COD-02 |
