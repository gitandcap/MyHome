# Acronimi di MyHome

**Aggiornato al:** 09/11/2026
**Stato:** registro permanente degli acronimi di progetti e sottomoduli (scorporato da ANA-03 §1)

---

**MyHome** è il sistema domotico. Gli acronimi servono per indicare velocemente un componente in documenti e conversazioni.

**Convenzione**
- Le tabelle riportano gli acronimi dei progetti e, per ciascun progetto, gli acronimi dei sottomoduli.
- **Ogni sottomodulo corrisponde a un componente da sviluppare.** La fase di analisi serve anche a individuarli.
- Ogni proposta deve indicare il progetto e il sottomodulo, specificando se è **già previsto** o **nuovo**.
- Ogni nuovo componente o sottomodulo riceve un acronimo e una riga nelle tabelle.

## 1. Componenti di sistema

| Acronimo | Nome esteso | Descrizione e ruolo nel sistema |
| --- | --- | --- |
| **`MPC`** | **M**ini **P**C | Il server fisico: mini PC Windows 11 con Docker Desktop (WSL2) che ospita l'infrastruttura software. |
| **`DEZ`** | **d**e**C**ON**Z** (con ConBee II) | Il gateway/coordinatore Zigbee (servizio deCONZ in container Docker + antenna USB ConBee II). |
| **`DCB`** | **D**e**c**onz **B**ridge | Il componente che dialoga con REST API e WebSocket di deCONZ e fa da ponte verso TCL e HSPA, pubblicando verso di loro un unico canale REST + SSE (ANA-05). Progetti `DCB.Gateway` (con i test `DCB.Gateway.Tests`), `DCB.Infrastructure` e `DCB.Server`, più la raccolta Postman in `Tools\Postman\`; gira come servizio Windows, .NET 10. |
| **`TCL`** | **T**ext **C**onsole **L**ine | Console per test, debug e invio comandi; può girare su postazioni remote. Progetto `TCL.Client`. |
| **`HSPA`** | **H**ouse **S**ingle **P**age **A**pplication | Dashboard web interattiva per il controllo della casa (REST + SSE verso il DCB, ANA-05). Prevista nell'architettura, sviluppata in fase successiva. |
| **`SHR`** | **SH**a**R**ed | Codice condiviso tra DCB, TCL e HSPA. Progetto `MyHome.Shared`. *(confermato il 07/11/2026)* |
| **`ZDEV`** | **Z**igbee **DEV**ices | L'insieme dei dispositivi fisici sul campo (sensori, relè, valvole TRV, lampade). |
| **`CFG`** | **C**on**F**i**G**uration JSON | File JSON con i parametri di sistema del DCB (IP e porte deCONZ, API key, chiave TCL, modalità di avvio). |
| **`DBZ`** | **D**ata**B**ase **Z**igbee JSON | File JSON con lo specchio grezzo dello stato dei `ZDEV` letto da deCONZ. |
| **`MDZ`** | **M**eta**D**ata **Z**igbee JSON | File JSON con i metadati utente: dispositivi mappati e auto-discovery dei nuovi. |

## 2. Gateway Connector (`GTW`) – progetto `DCB.Gateway`

Il sottomodulo `DCB-GTW` raccoglie tutta la comunicazione del DCB con deCONZ (`DEZ`): la base REST comune,
i client per endpoint e lo stream WebSocket. *(nuovo, registrato il 07/11/2026)*
È una libreria autonoma, che non dipende dal resto di MyHome e si verifica da sola con i test `DCB-TST` (ANA-03 §4.6).

| Acronimo | Sottomodulo C# | Endpoint deCONZ / ambito operativo | Stato specifica |
| --- | --- | --- | --- |
| **`DCB-GTW`** | **G**a**t**e**w**ay Connector *(base)* | Base comune verso deCONZ: connessione HTTP a host e porta da `CFG`, inserimento dell'API key, lettura delle risposte e degli errori deCONZ (`DeconzApiResponseDto`, errore 1 *unauthorized user*). | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-LIG`** | **Li**ghts API Client | Attuatori, relè e luci (`/api/{key}/lights`, `/lights/{id}`, `/lights/{id}/state`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-SEN`** | **Sen**sors API Client | Sensori di movimento, temperatura, contatti, pulsanti (`/api/{key}/sensors`, `/sensors/{id}`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-GRP`** | **Gr**ou**p**s API Client | Gruppi di dispositivi e comandi broadcast (`/api/{key}/groups`, `/groups/{id}/action`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-SCN`** | **Sc**e**n**es API Client | Scene preimpostate per i gruppi (`/api/{key}/groups/{id}/scenes`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-RUL`** | **Rul**es API Client | Regole e automazioni interne del gateway (`/api/{key}/rules`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-CFG`** | **C**on**f**i**g** API Client | Configurazione globale del gateway (`/api/{key}/config`). | *Parziale* |
| &nbsp;&nbsp;↳ **`DCB-GTW-AUT`** | **Aut**hentication Client | Riuso dell'ultima API key salvata in `CFG`, verificata all'avvio; se non valida, attesa dello sblocco del gateway e richiesta di una nuova chiave (`/api`, `/config/whitelist`). Vedi ANA-03 §4.3. | Definita |
| &nbsp;&nbsp;↳ **`DCB-GTW-SCH`** | **Sch**edules API Client | Azioni programmate a orario (`/api/{key}/schedules`). *(nuovo)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-DEV`** | **Dev**ices API Client | Vista per dispositivo fisico, che raggruppa le risorse light/sensor dello stesso apparecchio (`/api/{key}/devices`). *(nuovo)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-ALM`** | **Al**ar**m** Systems API Client | Sistemi di allarme del gateway (`/api/{key}/alarmsystems`). *(nuovo)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-TLK`** | **T**ouch**l**in**k** API Client | Ricerca, identificazione e reset delle lampade via Touchlink (`/api/{key}/touchlink`). *(nuovo)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-RLK`** | **R**esource**l**in**k**s API Client | Collegamenti tra risorse del gateway (`/api/{key}/resourcelinks`). *(nuovo, uso marginale)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-INF`** | **Inf**o API Client | Informazioni di servizio del gateway, es. fusi orari (`/api/{key}/info`). *(nuovo, uso marginale)* | *Da definire* |
| &nbsp;&nbsp;↳ **`DCB-GTW-WSS`** | **W**eb**S**ocket **S**tream | Stream degli eventi in tempo reale di deCONZ (porta WS dedicata). Vedi ANA-03 §4. | Definita |

## 3. Altri sottomoduli per progetto *(confermati il 07/11/2026)*

| Progetto | Acronimo | Sottomodulo | Ruolo | Stato |
| --- | --- | --- | --- | --- |
| `MyHome.Shared` | **`SHR-CTR`** | Contratti API | DTO di richieste e risposte REST condivisi (ANA-05) | Confermato (aggiornato 09/11/2026, prima contratti SignalR) |
| | **`SHR-EVT`** | Modelli eventi | `DcbEventDto` e modelli degli eventi | Confermato |
| | **`SHR-MOD`** | Modelli dispositivi | Modelli condivisi con TCL e HSPA | Confermato |
| `DCB.Infrastructure` | **`DCB-STO`** | Storage | Servizi di lettura/scrittura CFG, DBZ, MDZ | Confermato |
| `DCB.Server` | **`DCB-SVC`** | Hosting | Avvio come servizio Windows | Confermato |
| | **`DCB-BOT`** | Bootstrap | Avvio `auto` / `sbs` | Confermato |
| | ~~**`DCB-HUB`**~~ | ~~Hub SignalR~~ | Sostituito da `DCB-API` + `DCB-EVS` (ANA-05) | Sostituito (09/11/2026) |
| | **`DCB-API`** | API REST | Letture e comandi per TCL/HSPA, OpenAPI, chiave condivisa in `X-Api-Key` (ANA-05 §4) | Nuovo (09/11/2026) |
| | **`DCB-EVS`** | Flusso eventi SSE | Sessioni dei client, invio eventi e stato attuale (ANA-05 §5) | Nuovo (09/11/2026) |
| | **`DCB-EVD`** | Dispatcher eventi | Lazy event emission | Confermato |
| | **`DCB-ELS`** | EventLogStore | Registro degli eventi persistenti | Confermato |
| `DCB.Gateway.Tests` | **`DCB-TST`** | Test di GTW | Test automatici (xUnit) di `DCB-GTW` senza deCONZ: DTO, client REST, `DCB-GTW-AUT`, `DCB-GTW-WSS` (ANA-03 §8) | Confermato (nuovo, 07/11/2026) |
| ~~`DCB.Gateway.Console`~~ | ~~**`DCB-CLI`**~~ | ~~Console di verifica~~ | Sostituita da `DCB-PMN` + `DCB-TST` (ANA-03 §4.6) | Eliminato (09/11/2026) |
| `Tools\Postman` *(fuori dalla solution)* | **`DCB-PMN`** | Raccolta Postman | Richieste salvate verso deCONZ e verso `DCB-API` / `DCB-EVS`, con ambienti e script di controllo (ANA-03 §8) | Nuovo (09/11/2026) |
| `TCL.Client` | **`TCL-CON`** | Connessione | Client HTTP (REST) e lettura del flusso SSE verso DCB (ANA-05) | Confermato (aggiornato 09/11/2026) |
| | **`TCL-UI`** | Interfaccia | Layout a 3 zone con Terminal.Gui | Confermato |
| | **`TCL-CMD`** | Comandi | Interprete di `subscribe`, `step`, `status`, `history` (tradotti in chiamate REST, ANA-05 §4) | Confermato |
| `HSPA.*` | — | — | Da individuare in fase successiva | — |
