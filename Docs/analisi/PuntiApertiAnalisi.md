# Punti aperti di analisi

**Aggiornato al:** 09/11/2026
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
| PA-04 | **Passi di bootstrap**: elenco, ordine e gestione degli errori | ANA-03 §9 | `DCB-BOT` *(già previsto)* | passo 3 (`DCB-GTW-AUT` e bootstrap) | 07/11/2026 |
| PA-05 | **Topic e categorie degli eventi**, quali sono persistenti | ANA-03 §9 | `DCB-EVD`, `DCB-ELS`, `SHR-EVT` *(già previsti)* | passo 5 (dispatcher eventi) | 07/11/2026 |
| PA-06 | **Elenco dettagliato degli indirizzi REST** di `DCB-API` | ANA-05 §8 | `DCB-API` *(già previsto)* | passo 4 (API + TCL) | 09/11/2026 |
| PA-07 | **Porta di ascolto di `DCB.Server`** da aggiungere ai parametri di `CFG` | ANA-05 §8 | `DCB-STO`, `DCB-API` *(già previsti)* | passo 4 | 09/11/2026 |
| PA-08 | **Convenzione dei codici dispositivo** in `MDZ` (tipi, stanze, numerazione) | ANA-03 §9 | `DCB-STO` *(già previsto)* | – | 07/11/2026 |
| PA-09 | **SSE dalla HSPA**: nel browser il modo standard di leggere SSE non permette l'intestazione `X-Api-Key`; scegliere tra `fetch`, token nella query string o cookie | ANA-05 §8 | `DCB-EVS`, `HSPA` *(già previsti)* | HSPA | 09/11/2026 |
| PA-10 | **HTTPS** per `DCB.Server`, da valutare prima dell'uso fuori casa | ANA-05 §8 | `DCB-API`, `DCB-EVS` *(già previsti)* | uso remoto | 09/11/2026 |
| PA-11 | **deCONZ in container**: compose, ConBee II verso WSL con `usbipd`, volume persistente, migrazione della rete Zigbee | ANA-03 §9 | `DEZ` *(già previsto)* | – (oggi deCONZ in locale) | 07/11/2026 |
| PA-12 | **Progetto `ZigBeeService`**: decidere se recuperare il codice | ANA-03 §9 | – | – | 07/11/2026 |

## 2. Punti chiusi

| ID | Punto | Chiuso il | Esito | Riportato in |
| --- | --- | --- | --- | --- |
| – | Porta WebSocket di deCONZ locale | 09/11/2026 | 443, confermata con `websocketport` di `/api/{key}/config`. *Chiuso prima della creazione di questo documento, senza ID.* | ANA-03 §9 |
