# ANA-05 – Canale client di DCB.Server (REST + SSE)

**Data:** 09/11/2026
**Fonti:** sessione di analisi del 09/11/2026, ANA-03 §4.6 e §6
**Stato:** decisione presa; sostituisce l'hub SignalR (`DCB-HUB`) previsto in ANA-03

---

## 1. Decisione

`DCB.Server` pubblica **un unico canale** verso tutte le applicazioni client (TCL, HSPA e quelle future).
Chi vuole comunicare con il server si adegua a quel canale.

Il canale è composto da:
- **API REST** (HTTP/JSON, descritta con OpenAPI) per letture e comandi → `DCB-API` *(nuovo)*;
- **flusso eventi SSE** (Server-Sent Events) per le notifiche dal server ai client → `DCB-EVS` *(nuovo)*.

Sostituisce l'hub SignalR `DCB-HUB` *(già previsto, ora sostituito)*.

Il lato deCONZ non cambia: il protocollo lo stabilisce deCONZ (REST + WebSocket) e lo gestiscono i client di `DCB-GTW-*`.

## 2. Motivazione

- **SignalR** usa un protocollo pubblico, ma ha regole proprie (negoziazione, messaggi incorniciati, invocazione dei
  metodi dell'hub). Esistono client ufficiali solo per .NET, JavaScript e Java. Per gli altri linguaggi servono
  librerie di terze parti.
- **REST** è utilizzabile da qualsiasi tecnologia, perfino da `curl` o da un browser, e si documenta con OpenAPI/Swagger.
- REST da solo non permette al server di avvisare i client di sua iniziativa: serve un canale "push" altrettanto
  standard → **SSE**, che è HTTP puro, gestito nativamente dai browser e facile da leggere da qualsiasi linguaggio.
- Lo schema è lo stesso di deCONZ (REST + flusso eventi): un solo modello mentale per tutto il sistema.

## 3. Architettura

```text
TCL, HSPA, altri client
   │  REST  (HTTP/JSON, OpenAPI)        → letture e comandi        DCB-API
   │  SSE   (text/event-stream)         ← eventi dal server         DCB-EVS
   ▼
DCB.Server ──(metodi, stesso processo)──▶ DCB.Gateway ──(REST/WS)──▶ deCONZ
```

## 4. API REST (`DCB-API`, nuovo)

- Applicazione ASP.NET Core già ospitata da `DCB-SVC`, con *minimal API* o controller (scelta in fase di coding).
- Documentazione OpenAPI generata automaticamente, con Swagger UI disponibile in debug. Il file OpenAPI si può
  importare in Postman come base della raccolta `MyHome-DCB` (`DCB-PMN`, ANA-03 §8).
- Risorse indicative (elenco dettagliato da definire):

| Ambito | Esempio | Sostituisce (TCL) |
| --- | --- | --- |
| Stato del bootstrap | `GET /api/bootstrap` | `status` |
| Passo di bootstrap (`sbs`) | `POST /api/bootstrap/steps/{stepName}` | `step <stepName>` |
| Stato attuale (condizioni aperte) | `GET /api/conditions` | – |
| Registro eventi persistenti | `GET /api/history?categoria=…&limite=…` | `history` |
| Sottoscrizioni della sessione | `PUT` / `DELETE /api/sessions/{id}/topics/{topic}` | `subscribe` / `unsubscribe` |
| Dispositivi e comandi | `GET /api/devices`, `POST /api/devices/{code}/commands` | – (uso HSPA) |

- I DTO di richieste e risposte stanno in `MyHome.Shared` (`SHR-CTR`, `SHR-MOD`): i client non dipendono dal formato
  dei dati di deCONZ (come già previsto in ANA-03 §4.5).
- Errori in formato standard *Problem Details* (RFC 9457).

## 5. Flusso eventi (`DCB-EVS`, nuovo)

- Endpoint `GET /api/events` con risposta `text/event-stream`, che resta aperto.
- **Una connessione SSE = una sessione.** All'apertura il server assegna un ID di sessione e lo invia come primo
  evento. Le sottoscrizioni (§4) sono legate a quell'ID. Sostituisce il Connection ID di SignalR (ANA-03 §6).
- Formato di ogni evento: campo `event` = topic, campo `data` = `DcbEventDto` in JSON (`SHR-EVT`), campo `id` = progressivo.
- **Stato attuale all'apertura**: subito dopo l'ID di sessione vengono inviate le condizioni aperte, come previsto per la
  connessione di una nuova TCL (ANA-03 §6).
- **Lazy event emission** invariata: `DCB-EVD` crea un evento solo se almeno una sessione è sottoscritta al topic.
  Gli eventi persistenti vengono creati sempre.
- **Riconnessione**: il client si riconnette in automatico (comportamento standard SSE) inviando `Last-Event-ID`.
  Il server può inviare gli eventi persistenti persi, presi da `DCB-ELS`.
- Un messaggio di *keep-alive* periodico (commento SSE) tiene aperta la connessione; l'intervallo è un parametro di configurazione.
- **Evoluzione possibile**: un WebSocket semplice (non SignalR) solo se in futuro servirà un dialogo continuo nei due sensi.

## 6. Sicurezza

- La **chiave condivisa** in `CFG` resta: viaggia nell'header `X-Api-Key`, sia per REST sia per SSE.
- Una richiesta senza chiave o con chiave errata riceve `401`.

## 7. Impatti sugli acronimi

| Acronimo | Stato | Effetto |
| --- | --- | --- |
| `DCB-HUB` | già previsto | **sostituito** da `DCB-API` + `DCB-EVS` |
| `DCB-API` | **nuovo** | API REST di `DCB.Server` |
| `DCB-EVS` | **nuovo** | flusso eventi SSE e sessioni |
| `SHR-CTR` | già previsto | da contratti SignalR a DTO di richieste e risposte REST |
| `SHR-EVT` | già previsto | invariato (`DcbEventDto` = `data` dell'evento SSE) |
| `TCL-CON` | già previsto | da connessione SignalR a client HTTP + lettura del flusso SSE |
| `TCL-CMD` | già previsto | i comandi diventano chiamate REST (tabella §4) |
| `DCB-EVD` | già previsto | logica invariata, pubblica su `DCB-EVS` |
| `HSPA` | già previsto | usa REST + SSE (non più SignalR) |

## 8. Questioni aperte

1. **SSE da browser (HSPA)**: l'oggetto standard `EventSource` non permette di aggiungere header come `X-Api-Key`.
   Possibili soluzioni: leggere SSE tramite `fetch`, usare un token temporaneo nella query string ottenuto via REST,
   oppure un cookie. Da decidere con la HSPA.
2. **HTTPS**: oggi solo rete locale. Da valutare prima di esporre il servizio fuori casa o usare HSPA da remoto.
3. **Elenco dettagliato degli endpoint** e dei topic degli eventi (collegato ad ANA-03 §9, "topic e categorie degli eventi").
4. Porta di ascolto di `DCB.Server`: da aggiungere ai parametri di `CFG` (ANA-04).
