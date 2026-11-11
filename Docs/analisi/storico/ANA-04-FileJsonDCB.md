# ANA-04 – File JSON di configurazione e persistenza del DCB

**Data:** 07/11/2026
**Fonti:** ANA-03-SintesiUnificata.md (§4.3, §5, §6), storico\ANA-02-SintesiArchitetturaDCC.md (§storage), sessione di analisi del 07/11/2026
**Progetto / sottomodulo:** `DCB.Infrastructure` / `DCB-STO` *(già previsto)*
**Stato:** bozza – le strutture indicate come *proposta* sono da confermare

---

## 1. Scopo

Il DCB usa tre file JSON per i parametri di configurazione e per la persistenza:

| Acronimo | File | Contenuto | Chi scrive | Chi legge |
| --- | --- | --- | --- | --- |
| **`CFG`** | `config\cfg.json` | Parametri di sistema | Utente (a DCB fermo), `DCB-GTW-AUT` (solo nuova API key) | DCB, solo all'avvio |
| **`DBZ`** | `data\dbz.json` | Specchio grezzo dello stato dei `ZDEV` letto da deCONZ | Solo il DCB (`DCB-GTW-WSS`, client REST `DCB-GTW-LIG` / `DCB-GTW-SEN` / `DCB-GTW-GRP`) | DCB, verso TCL/HSPA |
| **`MDZ`** | `user\mdz.json` | Metadati utente: dispositivi mappati e auto-discovery | Utente (anche a DCB in esecuzione), auto-discovery del DCB | DCB all'avvio e a ogni salvataggio dell'utente |

Tutti i file sono gestiti da `DCB-STO`; gli altri sottomoduli non accedono mai direttamente ai file.

---

## 2. Regole comuni

### 2.1 Posizione e nomi

```text
C:\ProgramData\MyHome\DCB\        (%ProgramData%\MyHome\DCB\)
├── config\
│   └── cfg.json        CFG  letto solo all'avvio
├── data\
│   └── dbz.json        DBZ  scritto solo dal DCB
├── user\
│   └── mdz.json        MDZ  modificato dall'utente in corsa, riletto al salvataggio
└── backup\             copie automatiche prima di ogni sovrascrittura (*.bak)
```

- **`%ProgramData%`**: il DCB gira come servizio Windows (`DCB-SVC`), quindi i file non dipendono dall'utente connesso.
  Il percorso base è sovrascrivibile da riga di comando o variabile d'ambiente (utile in debug).
- **Sottocartelle per ruolo**: separano i file dell'utente (`config\`, `user\`) da quelli riscritti spesso dal DCB (`data\`);
  il DCB osserva solo `user\`.
- **Permessi**: al primo avvio il DCB crea le cartelle e concede all'utente il permesso di modifica su `config\` e `user\`
  (in `%ProgramData%` i file creati dal servizio non sono modificabili da un utente standard).
- **Nomi**: acronimi in minuscolo (`cfg.json`, `dbz.json`, `mdz.json`), corrispondenti a quelli dei documenti.
  Escluso `DCB-CFG.json` per non confonderlo con il sottomodulo `DCB-GTW-CFG` (client della configurazione deCONZ).

### 2.2 Gestione dei file
- **Formato:** UTF-8 senza BOM, JSON indentato (leggibile e modificabile a mano), nomi delle proprietà in camelCase.
- **Versione dello schema:** ogni file ha `schemaVersion` (intero) per gestire future migrazioni.
- **Thread-safety:** un servizio per file (`CfgStorageService`, `DbzStorageService`, `MdzStorageService`) con lock interno;
  lettura in memoria all'avvio, scrittura su richiesta.
- **Scrittura atomica:** scrittura su file temporaneo e sostituzione (`File.Replace` / move), così un'interruzione
  non lascia il file corrotto; la versione precedente viene copiata in `backup\`.

### 2.3 Comportamento per file

| File | Lettura | Scrittura | Note |
| --- | --- | --- | --- |
| `cfg.json` | Solo all'avvio | Utente a DCB fermo; `DCB-GTW-AUT` solo per salvare una nuova API key | Le modifiche manuali valgono dal riavvio |
| `dbz.json` | All'avvio, come cache | Solo il DCB | Non va modificato a mano: verrebbe riscritto |
| `mdz.json` | All'avvio e a ogni salvataggio dell'utente | Utente; il DCB per aggiungere nuovi `devices-unknown` | Rilettura in corsa, vedi §5.3 |

### 2.4 File mancante o non valido
- `CFG` mancante → viene creato con i valori predefiniti; `CFG` non leggibile → condizione aperta nello stato attuale
  (ANA-03 §6) ed evento persistente, il bootstrap si ferma.
- `DBZ` mancante o non valido → viene ricostruito da deCONZ via REST.
- `MDZ` mancante → viene creato vuoto; non valido → condizione aperta, non viene sovrascritto (contiene dati dell'utente).
  In corsa resta valida la versione precedente in memoria (§5.3).

---

## 3. `CFG` – parametri di sistema

### 3.1 Contenuto

| Proprietà | Descrizione | Note |
| --- | --- | --- |
| `schemaVersion` | Versione dello schema | |
| `deconz.host` | Indirizzo di deCONZ (`DEZ`) | Per ora `127.0.0.1`: deCONZ gira sulla stessa macchina come applicazione Windows; in futuro da container |
| `deconz.httpPort` | Porta REST di deCONZ | 80 (verificato il 07/11/2026 su deCONZ 2.33.2) |
| `deconz.wsPort` | Porta WebSocket *(facoltativa)* | Se vuota si usa `websocketport` letto da deCONZ; se valorizzata ha la precedenza (vedi sotto) |
| `deconz.apiKey` | Ultima API key utilizzata | Scritta da `DCB-GTW-AUT` (ANA-03 §4.3) |
| `deconz.apiKeyObtainedAt` | Data/ora di rilascio della chiave | Informativa |
| `deconz.deviceType` | Nome con cui il DCB si registra su deCONZ (`devicetype` di `POST /api`) | Es. `MyHome#DCB` |
| `deconz.unlockRetrySeconds` | Intervallo tra i tentativi di `POST /api` durante l'attesa dello sblocco | |
| `tcl.sharedKey` | Chiave condivisa per la connessione delle TCL | ANA-03 §6, sicurezza |
| `bootstrap.defaultMode` | Modalità di avvio predefinita: `auto` / `sbs` | Sovrascrivibile con `--mode` |

**Porta WebSocket** (`DCB-GTW-WSS`, funzione nuova):
- **`deconz.wsPort` vuota** (caso normale, deCONZ in locale): la porta viene letta a ogni avvio da `GET /api/{key}/config`
  (campo `websocketport`), così `DCB-GTW-WSS` resta corretto anche se deCONZ la cambia.
- **`deconz.wsPort` valorizzata**: ha la precedenza su `/config`. Serve quando deCONZ girerà in un container e Docker
  pubblicherà la porta WebSocket su un numero diverso da quello interno (es. `8443 → 443`): deCONZ conosce solo
  la porta interna, quindi il valore letto da `/config` sarebbe sbagliato.
- In `DCB.Gateway` corrisponde a un'opzione facoltativa di `DeconzGatewayOptions` (ANA-03 §4.6).

### 3.2 Esempio (proposta)

```json
{
  "schemaVersion": 1,
  "deconz": {
    "host": "127.0.0.1",
    "httpPort": 80,
    "wsPort": null,
    "apiKey": "1234567890ABCDEF",
    "apiKeyObtainedAt": "2026-11-07T10:15:00+01:00",
    "deviceType": "MyHome#DCB",
    "unlockRetrySeconds": 5
  },
  "tcl": {
    "sharedKey": "cambiami"
  },
  "bootstrap": {
    "defaultMode": "auto"
  }
}
```

---

## 4. `DBZ` – specchio dello stato dei dispositivi

### 4.1 Contenuto

- Copia **grezza** di quanto restituito da deCONZ, senza interpretazione: serve come cache locale dello stato
  dei `ZDEV` e come base per l'auto-discovery.
- Sezioni: `lights`, `sensors`, `groups`, indicizzate per id deCONZ, nel formato dei `Deconz*Dto` (ANA-03 §4.5).
- `lastFullSync`: data/ora dell'ultima lettura completa via REST.

### 4.2 Aggiornamento

- **Lettura completa via REST** all'avvio e dopo ogni riconnessione del WebSocket (recupero eventi persi, ANA-03 §4.4).
- **Aggiornamento puntuale** da ogni evento `DCB-GTW-WSS` (`changed`, `added`, `deleted`).
- Gli aggiornamenti da WebSocket possono essere molto frequenti: in memoria sono immediati, la scrittura su disco
  è raggruppata (proposta: al più una scrittura ogni pochi secondi) per non scrivere il file a ogni evento.

### 4.3 Esempio (proposta, ridotto)

```json
{
  "schemaVersion": 1,
  "lastFullSync": "2026-11-07T10:16:02+01:00",
  "lights": {
    "3": { "name": "Lampada salotto", "type": "Extended color light", "uniqueid": "00:17:88:01:...-0b", "state": { "on": true, "bri": 180, "reachable": true } }
  },
  "sensors": {
    "7": { "name": "Movimento ingresso", "type": "ZHAPresence", "uniqueid": "00:15:8d:00:...-01-0406", "state": { "presence": false, "lastupdated": "2026-11-07T09:14:55" } }
  },
  "groups": {}
}
```

---

## 5. `MDZ` – metadati utente

### 5.1 Contenuto

Due sezioni (ANA-03 §5):

- **`devices`** – dispositivi mappati e riconosciuti. Per ciascuno:
  - `code`: codice MyHome (es. `LIG-SAL-01`, `SEN-ING-02`);
  - `uniqueId`: `uniqueid` deCONZ, **chiave di collegamento** con `DBZ` (l'id numerico deCONZ può cambiare,
    l'`uniqueid` no);
  - `description`, `room` ed eventuali altri attributi utente.
- **`devices-unknown`** – dispositivi presenti in `DBZ` ma non ancora mappati, con codice proposto
  (`PROPOSED-LIG-XX`, `PROPOSED-SEN-XX`) e data di prima rilevazione.

### 5.2 Auto-discovery

- Dopo ogni lettura completa e a ogni evento `added`, il DCB confronta `DBZ` con `MDZ` tramite `uniqueId`.
- Un dispositivo non presente in nessuna delle due sezioni viene aggiunto a `devices-unknown` con codice proposto.
- Lo spostamento da `devices-unknown` a `devices` è un'operazione dell'utente (a mano o, in futuro, da TCL/HSPA).
- Il DCB non cancella mai voci da `devices`; un dispositivo mappato che sparisce da deCONZ resta in `MDZ`.

### 5.3 Rilettura in corsa (`DCB-STO`, funzione nuova)

L'utente modifica `mdz.json` mentre il DCB è in esecuzione (es. assegna codice e descrizione a un dispositivo
di `devices-unknown` e lo sposta in `devices`); al salvataggio il DCB lo rilegge:

1. **Osservazione**: `FileSystemWatcher` sulla cartella `user\`, con breve attesa dopo l'ultima modifica
   (gli editor salvano in più passaggi); le scritture fatte dal DCB stesso vengono ignorate.
2. **File valido** → i metadati in memoria vengono sostituiti; evento "MDZ ricaricato" verso le TCL.
3. **File non valido** (JSON errato, codici duplicati, `uniqueId` ripetuti) → resta in memoria la versione precedente,
   viene aperta una condizione nello stato attuale (ANA-03 §6) con evento persistente, il file non viene toccato.
   La condizione si chiude al successivo salvataggio corretto.
4. **Dopo ogni rilettura** il DCB ripete l'auto-discovery (`DBZ` ↔ `MDZ` tramite `uniqueId`) e aggiorna `devices-unknown`.

**Conflitto di scrittura**: se il DCB aggiunge un nuovo dispositivo mentre l'utente ha il file aperto, il salvataggio
dell'utente può sovrascriverlo. Non è un problema: `devices-unknown` è sempre ricalcolabile da `DBZ` e il punto 4
lo ripristina. Le voci di `devices` sono modificate solo dall'utente, mai dal DCB.

### 5.4 Esempio (proposta)

```json
{
  "schemaVersion": 1,
  "devices": [
    { "code": "LIG-SAL-01", "uniqueId": "00:17:88:01:...-0b", "description": "Lampada salotto", "room": "SAL" }
  ],
  "devices-unknown": [
    { "proposedCode": "PROPOSED-SEN-01", "uniqueId": "00:15:8d:00:...-01-0406", "deconzType": "ZHAPresence", "firstSeen": "2026-11-07T10:16:02+01:00" }
  ]
}
```

---

## 6. Fuori ambito

- **`EventLogStore`** (`DCB-ELS`): il registro degli eventi persistenti è anch'esso persistenza del DCB, ma il formato
  (JSON, JSON Lines, database) non è ancora deciso e non è descritto qui.

---

## 7. Questioni in sospeso

1. Posizione e nomi dei file: **proposta registrata il 07/11/2026** (§2.1).
2. Protezione dei dati sensibili in `CFG` (`deconz.apiKey`, `tcl.sharedKey`): in chiaro o cifrati (es. DPAPI).
3. Convenzione dei codici dispositivo in `MDZ` (tipi, stanze, numerazione) – già tra le questioni aperte di ANA-03 §9.
4. Frequenza di scrittura su disco di `DBZ`.
5. Ricaricamento dei file modificati a mano: **definito il 07/11/2026** – `CFG` al riavvio, `MDZ` in corsa (§5.3).
