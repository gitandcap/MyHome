# ANA-07 – File JSON di configurazione e persistenza del DCB

**Data:** 11/11/2026
**Fonti:** storico\ANA-04-FileJsonDCB.md, ANA-06-SintesiUnificata.md (§4.3, §4.7, §4.8, §5, §6), sessione di analisi dell'11/11/2026
**Progetto / sottomodulo:** `DCB.Infrastructure` / `DCB-STO` *(già previsto)*
**Stato:** bozza – le strutture indicate come *proposta* sono da confermare (PA-03); sostituisce ANA-04 (spostato in `storico\`)
**Novità rispetto ad ANA-04** (decisione dell'11/11/2026, PA-17): i parametri passano ad `appsettings.json` letto con le
classi Options; `CFG` contiene solo le differenze locali e la chiave delle TCL; l'API key deCONZ passa nel nuovo file
`KEY` (`apikey.json`); `DBZ` è aggiornato da `DCB-SYN` (ANA-06 §4.7, PA-13).

---

## 1. Scopo

Il DCB legge i parametri dalla configurazione .NET (`appsettings.json` + `CFG`) e usa tre file JSON di persistenza:

| Acronimo | File | Contenuto | Chi scrive | Chi legge |
| --- | --- | --- | --- | --- |
| – | `appsettings.json` (cartella di installazione) | **Tutti** i parametri, con valori predefiniti e commenti | Sviluppatore (versionato) | DCB all'avvio, tramite le classi Options |
| **`CFG`** | `config\cfg.json` | Differenze locali rispetto ad `appsettings.json` e chiave condivisa delle TCL | Utente (a DCB fermo); il DCB **non lo riscrive mai** | DCB all'avvio, tramite le classi Options |
| **`KEY`** | `config\apikey.json` | API key deCONZ e data di rilascio | Solo il DCB (`DCB-STO`, su richiesta di `DCB-GTW-AUT` tramite `IApiKeyStore`) | DCB all'avvio (`DCB-GTW-AUT`) |
| **`DBZ`** | `data\dbz.json` | Specchio grezzo dello stato dei `ZDEV` letto da deCONZ | Solo il DCB (`DCB-SYN`, ANA-06 §4.7) | DCB, verso TCL/HSPA |
| **`MDZ`** | `user\mdz.json` | Metadati utente: dispositivi mappati e auto-discovery | Utente (anche a DCB in esecuzione), auto-discovery del DCB (`DCB-SYN`) | DCB all'avvio e a ogni salvataggio dell'utente |

`KEY`, `DBZ` e `MDZ` sono gestiti da `DCB-STO`; gli altri sottomoduli non accedono mai direttamente ai file.
`appsettings.json` e `CFG` sono letti dal sistema di configurazione di .NET e arrivano ai sottomoduli solo come classi Options.

---

## 2. Regole comuni

### 2.1 Posizione e nomi

```text
C:\ProgramData\MyHome\DCB\        (%ProgramData%\MyHome\DCB\)
├── config\
│   ├── cfg.json        CFG  differenze locali, letto solo all'avvio, mai riscritto dal DCB
│   └── apikey.json     KEY  scritto solo dal DCB
├── data\
│   └── dbz.json        DBZ  scritto solo dal DCB
├── user\
│   └── mdz.json        MDZ  modificato dall'utente in corsa, riletto al salvataggio
└── backup\             copie automatiche prima di ogni sovrascrittura (*.bak)
```

- **`%ProgramData%`**: il DCB gira come servizio Windows (`DCB-SVC`), quindi i file non dipendono dall'utente connesso.
  Il percorso base è sovrascrivibile da riga di comando o variabile d'ambiente (utile in debug).
- **Sottocartelle per ruolo**: separano i file dell'utente (`config\cfg.json`, `user\`) da quelli riscritti spesso dal DCB (`data\`);
  il DCB osserva solo `user\`. `apikey.json` sta in `config\` perché, come `cfg.json`, contiene un segreto.
- **Permessi**: al primo avvio il DCB crea le cartelle e concede all'utente il permesso di modifica su `config\` e `user\`
  (in `%ProgramData%` i file creati dal servizio non sono modificabili da un utente standard).
- **Nomi**: acronimi in minuscolo (`cfg.json`, `dbz.json`, `mdz.json`), corrispondenti a quelli dei documenti;
  eccezione `apikey.json`, più chiaro per chi apre la cartella (acronimo `KEY`).
  Escluso `DCB-CFG.json` per non confonderlo con il sottomodulo `DCB-GTW-CFG` (client della configurazione deCONZ).
- **In sviluppo** la cartella dati locale (`config\` compresa) è esclusa da git (`.gitignore`): non contiene nulla da versionare.

### 2.2 Gestione dei file
- **Formato:** UTF-8 senza BOM, JSON indentato (leggibile e modificabile a mano).
  Nomi delle proprietà: in PascalCase per `appsettings.json` e `cfg.json` (convenzione della configurazione .NET,
  che comunque non distingue maiuscole e minuscole), in camelCase per `apikey.json`, `dbz.json` e `mdz.json`.
- **Commenti:** ammessi in `appsettings.json` e `cfg.json` (`//`, come da `CLAUDE.md`); i file scritti dal DCB non ne hanno.
- **Versione dello schema:** `apikey.json`, `dbz.json` e `mdz.json` hanno `schemaVersion` (intero) per gestire future migrazioni.
- **Thread-safety:** un servizio per file scritto dal DCB (`KeyStorageService`, `DbzStorageService`, `MdzStorageService`)
  con lock interno; lettura in memoria all'avvio, scrittura su richiesta.
- **Scrittura atomica:** scrittura su file temporaneo e sostituzione (`File.Replace` / move), così un'interruzione
  non lascia il file corrotto; la versione precedente viene copiata in `backup\`.

### 2.3 Comportamento per file

| File | Lettura | Scrittura | Note |
| --- | --- | --- | --- |
| `appsettings.json` | Solo all'avvio | Sviluppatore; sovrascritto a ogni pubblicazione | Nessun segreto, nessun valore scritto dal programma |
| `cfg.json` | Solo all'avvio | Utente a DCB fermo | Le modifiche manuali valgono dal riavvio; il DCB non lo riscrive, così i commenti dell'utente si conservano |
| `apikey.json` | All'avvio | Solo `DCB-STO`, quando `DCB-GTW-AUT` ottiene una nuova chiave | Non va modificato a mano |
| `dbz.json` | All'avvio, come cache | Solo il DCB | Non va modificato a mano: verrebbe riscritto |
| `mdz.json` | All'avvio e a ogni salvataggio dell'utente | Utente; il DCB per aggiungere nuovi `devices-unknown` | Rilettura in corsa, vedi §5.3 |

### 2.4 File mancante o non valido
- `cfg.json` mancante → si usano i soli valori di `appsettings.json`; `cfg.json` non leggibile → condizione aperta nello
  stato attuale (ANA-06 §6) ed evento persistente, il bootstrap si ferma al passo `config` (ANA-06 §4.2).
- `apikey.json` mancante o non valido → nessuna chiave disponibile: `DCB-GTW-AUT` entra in attesa dello sblocco
  del gateway (ANA-06 §4.3); un file non valido viene copiato in `backup\` prima di essere sostituito.
- `DBZ` mancante o non valido → viene ricostruito da deCONZ via REST (passo `sync`).
- `MDZ` mancante → viene creato vuoto; non valido → condizione aperta, non viene sovrascritto (contiene dati dell'utente).
  In corsa resta valida la versione precedente in memoria (§5.3).

---

## 3. Parametri (`appsettings.json` e `CFG`) e API key (`KEY`)

### 3.1 Parametri

Ogni sezione corrisponde a una classe Options nella cartella `Configuration` del progetto che la usa; ogni parametro
è commentato sia nella classe (`///`) sia in `appsettings.json` (significato, unità, valore predefinito, valori ammessi).

| Sezione / parametro | Descrizione | Note |
| --- | --- | --- |
| `Deconz:Host` | Indirizzo di deCONZ (`DEZ`) | Predefinito `127.0.0.1`: deCONZ gira sulla stessa macchina come applicazione Windows; in futuro da container |
| `Deconz:HttpPort` | Porta REST di deCONZ | 80 (verificato il 07/11/2026 su deCONZ 2.33.2) |
| `Deconz:WsPort` | Porta WebSocket *(facoltativa)* | Se vuota si usa `websocketport` letto da deCONZ; se valorizzata ha la precedenza (vedi sotto) |
| `Deconz:DeviceType` | Nome con cui il DCB si registra su deCONZ (`devicetype` di `POST /api`) | Es. `MyHome#DCB` |
| `Deconz:UnlockRetrySeconds` | Intervallo tra i tentativi di `POST /api` durante l'attesa dello sblocco | |
| `Bootstrap:DefaultMode` | Modalità di avvio predefinita: `auto` / `sbs` | Sovrascrivibile con `--mode` |
| `Kestrel:…` | Porta di ascolto di `DCB.Server` | Configurazione standard di ASP.NET Core; valore da scegliere (PA-07) |
| `Tcl:SharedKey` | Chiave condivisa per la connessione delle TCL | ANA-06 §6. In `appsettings.json` **vuota**, con il commento che rimanda a `cfg.json`: è un segreto |

I parametri tecnici (timeout, attese crescenti del bootstrap, keep-alive SSE, raggruppamento delle scritture di `DBZ`…)
si aggiungono alle rispettive sezioni quando vengono sviluppati i sottomoduli che li usano.

La sezione `Deconz` corrisponde a `DeconzGatewayOptions` di `DCB.Gateway` (ANA-06 §4.6): `GTW` non sa da quale file
arrivano i valori.

**Porta WebSocket** (`DCB-GTW-WSS`):
- **`Deconz:WsPort` vuota** (caso normale, deCONZ in locale): la porta viene letta a ogni avvio da `GET /api/{key}/config`
  (campo `websocketport`), così `DCB-GTW-WSS` resta corretto anche se deCONZ la cambia.
- **`Deconz:WsPort` valorizzata**: ha la precedenza su `/config`. Serve quando deCONZ girerà in un container e Docker
  pubblicherà la porta WebSocket su un numero diverso da quello interno (es. `8443 → 443`): deCONZ conosce solo
  la porta interna, quindi il valore letto da `/config` sarebbe sbagliato.

### 3.2 `CFG` – esempio (proposta)

Contiene solo ciò che differisce da `appsettings.json`, più la chiave delle TCL:

```json
{
  // Chiave condivisa delle TCL (header X-Api-Key). Obbligatoria: in appsettings.json è vuota.
  "Tcl": {
    "SharedKey": "cambiami"
  },
  // Esempio di differenza locale: deCONZ su un'altra macchina.
  "Deconz": {
    "Host": "192.168.1.20"
  }
}
```

### 3.3 `KEY` – API key deCONZ (nuovo)

| Proprietà | Descrizione | Note |
| --- | --- | --- |
| `schemaVersion` | Versione dello schema | |
| `apiKey` | Ultima API key utilizzata | Scritta da `DCB-STO` su richiesta di `DCB-GTW-AUT` (ANA-06 §4.3) |
| `obtainedAt` | Data/ora di rilascio della chiave | Informativa |

Esempio (proposta):

```json
{
  "schemaVersion": 1,
  "apiKey": "1234567890ABCDEF",
  "obtainedAt": "2026-11-07T10:15:00+01:00"
}
```

---

## 4. `DBZ` – specchio dello stato dei dispositivi

### 4.1 Contenuto

- Copia **grezza** di quanto restituito da deCONZ, senza interpretazione: serve come cache locale dello stato
  dei `ZDEV` e come base per l'auto-discovery.
- Sezioni: `lights`, `sensors`, `groups`, indicizzate per id deCONZ, nel formato dei `Deconz*Dto` (ANA-06 §4.5).
- `lastFullSync`: data/ora dell'ultima lettura completa via REST.

### 4.2 Aggiornamento

- Aggiornato da `DCB-SYN` (ANA-06 §4.7), che applica lettura completa ed eventi uno alla volta.
- **Lettura completa via REST** (`DCB-GTW-FUL`) al passo `sync` del bootstrap e dopo ogni riconnessione del WebSocket
  (recupero eventi persi, ANA-06 §4.4 e §4.7).
- **Aggiornamento puntuale** da ogni evento ricevuto da `DCB-GTW-WSS` (`changed`, `added`, `deleted`).
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

Due sezioni (ANA-06 §5):

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
   viene aperta una condizione nello stato attuale (ANA-06 §6) con evento persistente, il file non viene toccato.
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
2. Protezione dei dati sensibili (`apiKey` in `KEY`, `Tcl:SharedKey` in `CFG`): in chiaro o cifrati (es. DPAPI).
3. Convenzione dei codici dispositivo in `MDZ` (tipi, stanze, numerazione) – punto aperto PA-08.
4. Frequenza di scrittura su disco di `DBZ`.
5. Ricaricamento dei file modificati a mano: **definito il 07/11/2026** – `CFG` al riavvio, `MDZ` in corsa (§5.3).
6. Collocazione dei parametri: **decisa l'11/11/2026** (PA-17) – `appsettings.json` + `CFG` + `KEY` (§1, §3).
