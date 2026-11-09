# DCC (Domotic Control Center) - Punto della Situazione e Sintesi

## 1. Obiettivo del Progetto e Ambito

* **Progetto**: Domotic Control Center (**`DCC`**) personalizzato.
* **Stack Tecnologico**: C# / .NET 8/9, Visual Studio 2022 Community Edition.
* **Hardware Target**: Mini PC (**`MPC`**) con deCONZ (**`DEZ`**) e antenna USB Zigbee ConBee II.
* **Interfacce**:
  * Text Console Line (**`TCL`**).
  * House Single Page Application (**`HSPA`** dashboard web, utilizzo previsto di SignalR).

---

## 2. Tabella Generale degli Acronimi e Moduli

### Componenti Principali di Sistema

| Acronimo | Nome Esteso del Componente | Descrizione e Ruolo nel Sistema |
| --- | --- | --- |
| **`MPC`** | **M**ini **P**C | Il server fisico/hardware che ospita l'infrastruttura software. |
| **`DEZ`** | **d**e**C**ON**Z** (con ConBee II) | Il gateway/coordinatore Zigbee (servizio deCONZ + antenna USB). |
| **`DCC`** | **D**omotic **C**ontrol **C**enter | Il middleware/servizio backend centrale sviluppato in **C# / .NET 8/9**. |
| **`TCL`** | **T**ext **C**onsole **L**ine | La console a riga di comando per test, debug e invio comandi rapidi. |
| **`HSPA`** | **H**ouse **S**ingle **P**age **A**pplication | Il cruscotto grafico web vettoriale/interattivo per il controllo della casa. |
| **`ZDEV`** | **Z**igbee **DEV**ices | L'insieme di tutti i dispositivi fisici sul campo (sensori, relè, valvole TRV, lampade). |
| **`CFG`** | **C**on**F**i**G**uration JSON | File JSON contenente i parametri di sistema del DCC (IP deCONZ, porte, API Key). |
| **`DBZ`** | **D**ata**B**ase **Z**igbee JSON | File JSON per la persistenza e il salvataggio dello stato locale di tutti i **`ZDEV`**. |

### Sottomoduli del `DeconzBridge` (`DCB`) ed Endpoint

| Acronimo | Sottomodulo C# (`DCB`) | Endpoint deCONZ / Ambito Operativo | Stato Specifica |
| --- | --- | --- | --- |
| **`DCB`** | **D**econz**B**ridge *(Core)* | Modulo principale C# per la gestione delle comunicazioni REST e WebSocket con deCONZ. | Definita |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-LIG`** | **L**ights **A**PI Client | Gestione attuatori, relè e luci (`/api/{key}/lights`, `/lights/{id}`, `/lights/{id}/state`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-SEN`** | **S**ensors **A**PI Client | Gestione sensori di movimento, temperatura, contatti, pulsanti (`/api/{key}/sensors`, `/sensors/{id}`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-GRP`** | **G**roups **A**PI Client | Gestione dei gruppi di dispositivi e comandi broadcast (`/api/{key}/groups`, `/groups/{id}/action`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-SCN`** | **Sc**e**n**es **A**PI Client | Gestione delle scene preimpostate per i gruppi (`/api/{key}/groups/{id}/scenes`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-RUL`** | **R**ules **A**PI Client | Gestione delle regole e automazioni interne del gateway (`/api/{key}/rules`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-CFG`** | **C**onfig **A**PI Client | Gestione configurazione globale del gateway (`/api/{key}/config`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-AUT`** | **Aut**hentication Client | Gestione della creazione API Key e della whitelist (`/api`, `/config/whitelist`). | *Parziale* |
| &nbsp;&nbsp;&nbsp;&nbsp;↳ **`DCB-WSS`** | **W**eb**S**ocket **S**tream | Stream asincrono degli eventi in tempo reale inviati da deCONZ (porta WS dedicata). | Definita |

---

## 3. File di Configurazione e Persistenza

* **`CFG`**: File dei parametri di sistema (IP, porte, API Key, percorsi).
* **`DBZ`**: File JSON per la persistenza dello stato e la mappatura dei dispositivi Zigbee.

---

## 4. Strutture Dati e Architettura dei DTO

* Progettazione basata sul principio **DRY (Don't Repeat Yourself)** con classi base condivise ed ereditarietà.
* **Condivisi (`Shared/`)**: `DeconzBaseDeviceDto`, `DeconzLightStateDto`, `DeconzApiResponseDto`.
* **Luci (`Lights/`)**: `DeconzLightDto`.
* **Sensori (`Sensors/`)**: `DeconzSensorDto` (con dizionari dinamici per stati e configurazioni).
* **Gruppi e Scene (`Groups/`)**: `DeconzGroupDto`, `DeconzSceneDto`.
* **Regole (`Rules/`)**: `DeconzRuleDto`.
* **Config e Auth (`Config/`)**: `DeconzConfigDto`, `DeconzAuthRequestDto`, `DeconzAuthResponseDto`.
* **WebSocket (`WebSocket/`)**: `DeconzWsEventDto`.

---

## 5. Prossimi Passi per le Prossime Sessioni

1. Definizione della struttura e della logica di salvataggio/caricamento del file **`DBZ`** (State Manager locale).
2. Registrazione e configurazione della Dependency Injection per i sottomoduli del **`DCB`**.
3. Implementazione dell'ascolto in tempo reale via WebSocket (**`DCB-WSS`**) e notifica verso **`HSPA`**.