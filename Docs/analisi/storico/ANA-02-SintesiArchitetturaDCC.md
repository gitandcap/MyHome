# 📌 Documento di Sintesi Architetturale e Stato del Progetto DCC

**Progetto:** DCC (Domotic Control Center)  
**Target Execution Environment:** Mini PC (**MPC**) con container Docker deCONZ (**DEZ**)  
**Linguaggio & Framework:** C# / .NET 8/9 (Clean Architecture)  
**Client Interface:** Multiple Interactive Remote CLI Clients (**TCL**)  

---

## 1. 🏛️ Architettura Generale della Solution

La soluzione `.sln` è suddivisa in progetti C# distinti per garantire **indipendenza di pubblicazione (deploy)** tra il backend headless che eseguirà sul Mini PC e la console di gestione che potrà essere eseguita su postazioni remote.

```text
DCC.sln
│
├── src/
│   ├── DCC.Core/             <-- 🔗 CONDIVISO: DTO (DccEventDto), contratti SignalR, costanti
│   ├── DCC.Infrastructure/   <-- Storage (CFG, DBZ, MDZ) e Client REST/Auth deCONZ
│   ├── DCC.Server/           <-- Backend Headless (gira su MPC, gestisce SignalR e Bootstrap)
│   └── DCC.TclClient/        <-- Console CLI Remota (Indipendente e portabile)
```

### Principali Vantaggi:
* **`DCC.TclClient`**: Compilabile e distribuibile in autonomia (es. `win-x64`), si connette via SignalR WebSocket.
* **`DCC.Server`**: Compilabile come servizio Linux (`linux-x64`) per l'esecuzione continua su Mini PC (**MPC**).

---

## 2. 💾 Modello di Storage (Fase 1)

Il backend gestisce tre store persistenti thread-safe basati su file JSON:

1. **`CFG` (`DccConfig`)**: Parametri di sistema, indirizzo IP deCONZ, porte, token API e modalità di avvio predefinita.
2. **`DBZ` (`DbzStorageService`)**: Specchio grezzo dei dispositivi Zigbee letto da deCONZ.
3. **`MDZ` (`MdzStorageService`)**: Metadati utente strutturati in due sezioni:
   * **`devices`**: Dispositivi mappati e riconosciuti (es. `LIG-SAL-01`, `SEN-ING-02`).
   * **`devices-unknown`**: Modulo di auto-discovery per i nuovi dispositivi Zigbee con acronimi proposti in automatico (`PROPOSED-LIG-XX`, `PROPOSED-SEN-XX`).

---

## 3. 🚦 Modalità di Avvio e Bootstrap (SBS & Auto)

L'eseguibile del backend (`DCC.Server`) accetta la modalità di avvio direttamente tramite parametro a riga di comando:

* **Esecuzione Standard (Default / Produzione):**
  ```bash
  ./DCC.Server --mode auto
  ```
  Se il parametro `--mode` viene omesso, il sistema parte di default in modalità **`auto`**, eseguendo la sequenza completa dei passaggi di avvio senza intervento umano.

* **Esecuzione Step-by-Step (Debug / Test):**
  ```bash
  ./DCC.Server --mode sbs
  ```
  Il sistema si avvia e si mette in attesa. I client `TCL` connessi possono interrogare lo stato (`status`) ed eseguire i singoli step di bootstrap (`step <stepName>`), garantendo il supporto all'esecuzione **fuori sequenza / non sequenziale** per finalità di diagnostica.

---

## 4. ⚡ Gestione Eventi, Sessioni Multiple e Strategia di Rete

Per prevenire l'intasamento della linea di rete e ottimizzare le risorse sul Mini PC, l'architettura implementa una duplice strategia:

### A. Sessioni Multiple TCL
* Ogni istanza di `DCC.TclClient` connessa possiede una propria **Context Connection ID** SignalR e un proprio stato di sottoscrizione isolato.

### B. Pattern *Lazy Event Emission* (Eventi Volatili / Real-Time)
* Gli eventi ad alta frequenza o di monitoraggio (API deCONZ, cambi di stato Zigbee) **non vengono creati né allocati in memoria se non ci sono TCL sottoscritti**.
* La creazione dell'oggetto viene delegata a una lambda factory (`Func<DccEventDto>`) che si attiva **solo se** la verifica `HasSubscribers(topic)` ha esito positivo.

### C. Storicizzazione & Audit (Eventi Persistenti)
* Gli eventi di sistema rilevanti (avvio/arresto, cambi configurazione, errori di rete, eventi critici) vengono identificati con il flag `isPersistent = true`.
* Tali eventi vengono **sempre salvati nel registro di memoria/disco (`EventLogStore`)** del backend, indipendentemente dalla presenza di `TCL` connessi.
* I client `TCL` possono richiedere in qualsiasi momento la visualizzazione di tali eventi tramite i comandi di cronologia (es. `history system 20`).

---

## 5. 🖥️ Layout e Funzionalità del Client `TCL`

Il client console `DCC.TclClient` è strutturato in tre zone operative:

* **Zona 1 (Streaming Eventi):** Visualizzazione in tempo reale degli eventi ricevuti dal server (con filtri locali ed eliminazione del rumore).
* **Zona 2 (Input Comandi):** Prompt di comando interattivo per inviare direttive al server.
  * `subscribe <topic>` / `unsubscribe <topic>`
  * `step <stepName>` (Esecuzione guidata SBS)
  * `status` (Verifica dello stato del bootstrap)
  * `history <category> [limit]` (Interrogazione del registro eventi storicizzati)
* **Zona 3 (Output Comandi):** Area riservata all'esito dettagliato e all'ispezione dei comandi inviati da Zona 2.

---

## 📊 Matrice dello Stato Attuale del Progetto

| Componente / Funzionalità | Stato Architetturale | Stato Prototipo / Codice |
| :--- | :---: | :---: |
| Soluzione Poliprogetto (`DCC.Core`, `DCC.Infrastructure`, `DCC.Server`, `DCC.TclClient`) | **Completato** | Pronto per la stesura |
| File Storage (`CFG`, `DBZ`, `MDZ` + Auto-discovery) | **Completato** | Servizi base definiti |
| Modulo Autenticazione deCONZ (`DCB-AUT`) con Exponential Backoff | **Completato** | Codice implementato |
| Orchestratore Avvio (Modalità `Auto` vs `SBS` fuori sequenza) | **Completato** | Logica prototipo definita |
| Dispatcher Eventi con *Lazy Emission* e Persistenza Log | **Completato** | Logica prototipo definita |
| Gestione Sessioni Multiple SignalR | **Completato** | Hub SignalR progettato |
| Interfaccia CLI a 3 Zone (`DCC.TclClient`) | **Completato** | Prototipo base impostato |
| Client REST concreti (`DCB-LIG`, `DCB-SEN`, `DCB-GRP`) | In attesa (Fase 2) | Prossimo step post-prototipo |

---