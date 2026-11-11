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

**Colonna "Blocca"**: indica il passo di coding (ANA-03 §10) o il componente che non può partire finché il punto è aperto.

## 1. Punti aperti

| ID | Punto | Origine | Progetto / sottomodulo | Blocca | Aperto il |
| --- | --- | --- | --- | --- | --- |
| PA-03 | **Strutture dei file `CFG`, `DBZ`, `MDZ`**: ANA-04 è una bozza con parti *proposta* da confermare | ANA-04 | `DCB-STO` *(già previsto)* | passo 2 (storage) | 07/11/2026 |
| PA-04 | **Passi di bootstrap**: elenco, ordine e gestione degli errori. Comprende il caso **deCONZ non raggiungibile** (in locale parte solo dopo il login, il DCB prima): GTW riprova da solo o restituisce l'errore al bootstrap? *(aggiornato l'11/11/2026)* | ANA-03 §9, COD-02 | `DCB-BOT`, `DCB-GTW-AUT` *(già previsti)* | passo 3 (`DCB-GTW-AUT` e bootstrap) | 07/11/2026 |
| PA-05 | **Topic e categorie degli eventi**, quali sono persistenti | ANA-03 §9 | `DCB-EVD`, `DCB-ELS`, `SHR-EVT` *(già previsti)* | passo 5 (dispatcher eventi) | 07/11/2026 |
| PA-06 | **Elenco dettagliato degli indirizzi REST** di `DCB-API` | ANA-05 §8 | `DCB-API` *(già previsto)* | passo 4 (API + TCL) | 09/11/2026 |
| PA-07 | **Porta di ascolto di `DCB.Server`** da aggiungere ai parametri di `CFG` | ANA-05 §8 | `DCB-STO`, `DCB-API` *(già previsti)* | passo 4 | 09/11/2026 |
| PA-08 | **Convenzione dei codici dispositivo** in `MDZ` (tipi, stanze, numerazione) | ANA-03 §9 | `DCB-STO` *(già previsto)* | – | 07/11/2026 |
| PA-09 | **SSE dalla HSPA**: nel browser il modo standard di leggere SSE non permette l'intestazione `X-Api-Key`; scegliere tra `fetch`, token nella query string o cookie | ANA-05 §8 | `DCB-EVS`, `HSPA` *(già previsti)* | HSPA | 09/11/2026 |
| PA-10 | **HTTPS** per `DCB.Server`, da valutare prima dell'uso fuori casa | ANA-05 §8 | `DCB-API`, `DCB-EVS` *(già previsti)* | uso remoto | 09/11/2026 |
| PA-11 | **deCONZ in container**: compose, ConBee II verso WSL con `usbipd`, volume persistente, migrazione della rete Zigbee | ANA-03 §9 | `DEZ` *(già previsto)* | – (oggi deCONZ in locale) | 07/11/2026 |
| PA-12 | **Progetto `ZigBeeService`**: decidere se recuperare il codice | ANA-03 §9 | – | – | 07/11/2026 |
| PA-13 | **Rilettura dello stato dopo una riconnessione del WebSocket**: ANA-03 §4.4 la attribuisce a `WSS`, ma il risultato va in `DBZ`, fuori da GTW. *Proposta*: GTW notifica solo "riconnesso", `DCB.Server` rilegge lo stato tramite i client | COD-02 | `DCB-GTW-WSS`, `DCB-STO` *(già previsti)* | passo 5 (`WSS`) | 11/11/2026 |
| PA-14 | **Stato completo del gateway** (`GET /api/{key}`): non ha un sottomodulo. *Proposta*: assegnarlo a `DCB-GTW-CFG` oppure a un nuovo sottomodulo (es. `DCB-GTW-FUL`) | COD-02 | `DCB-GTW-CFG` *(già previsto)* o nuovo | client REST di GTW | 11/11/2026 |
| PA-15 | **DTO delle richieste**: ANA-03 §4.5 elenca solo i DTO di risposta; per i comandi servono DTO parziali con campi facoltativi (es. `DeconzLightStateRequestDto`). Da definire nomi e cartelle | COD-02 | `DCB-GTW-LIG`, `-GRP`, `-SCN` *(già previsti)* | comandi di GTW | 11/11/2026 |
| PA-16 | **Ampiezza della prima versione di GTW**. *Proposta*: base, `CFG`, `AUT`, `LIG`, `SEN`, `GRP`, `SCN`, `WSS`; rinviati `RUL`, `SCH`, `DEV`, `ALM`, `TLK`, `RLK`, `INF` | COD-02 | `DCB-GTW-*` *(già previsti)* | piano di sviluppo di GTW | 11/11/2026 |
| PA-17 | **Dove stanno i parametri**: CLAUDE.md prevede `appsettings.json` con classi Options per ogni progetto, ANA-03/ANA-04 mettono host, porte e chiavi deCONZ in `cfg.json` letto da `DCB-STO`. Non riguarda GTW (riceve `DeconzGatewayOptions`), ma decide come `DCB.Server` lo configura | COD-02 | `DCB-STO`, `DCB-SVC` *(già previsti)* | passo 2 (storage) | 11/11/2026 |

## 2. Punti chiusi

| ID | Punto | Chiuso il | Esito | Riportato in |
| --- | --- | --- | --- | --- |
| – | Porta WebSocket di deCONZ locale | 09/11/2026 | 443, confermata con `websocketport` di `/api/{key}/config`. *Chiuso prima della creazione di questo documento, senza ID.* | ANA-03 §9 |
