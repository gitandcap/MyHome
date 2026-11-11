# COD-02 – Gateway Connector (`DCB.Gateway`)

**Data:** 11/11/2026
**Fonti:** ANA-06 §4.3–4.8 e §8, ANA-07 §3, sessioni di coding e di analisi dell'11/11/2026
**Stato:** organizzazione e scelte tecniche confermate; aggiornato l'11/11/2026 con le decisioni su PA-13…PA-17 (resta aperto PA-04, §4)

---

## 1. Ruolo

`DCB.Gateway` (`DCB-GTW`) è il **client di deCONZ**: chiama le REST API e riceve gli eventi del WebSocket.
È una libreria autonoma (ANA-06 §4.6): non espone endpoint propri (il canale verso i client è `DCB-API` + `DCB-EVS`
di `DCB.Server`, ANA-05), non dipende da altri progetti MyHome e usa solo .NET e le astrazioni standard
(`HttpClient`, `ILogger`, `IOptions`, `TimeProvider`).

## 2. Organizzazione del progetto

```text
DCB.Gateway/
├── Configuration/
│   └── DeconzGatewayOptions.cs        host, porta REST, porta WS facoltativa, timeout, intervallo tentativi sblocco
├── Abstractions/
│   └── IApiKeyStore.cs                lettura/salvataggio dell'ultima API key (in MyHome implementata da DCB-STO)
├── Core/                               DCB-GTW (base)
│   ├── DeconzRestClient.cs            unico punto che usa HttpClient: URL /api/{key}/…, invio, deserializzazione
│   ├── DeconzErrorReader.cs           riconosce gli errori deCONZ anche nelle risposte 2xx
│   └── DeconzApiException.cs          tipo, indirizzo e descrizione dell'errore deCONZ
├── Clients/                            un client per area, ognuno con la sua interfaccia
│   ├── Lights/   ILightsClient, LightsClient            DCB-GTW-LIG
│   ├── Sensors/  ISensorsClient, SensorsClient          DCB-GTW-SEN
│   ├── Groups/   IGroupsClient, GroupsClient            DCB-GTW-GRP
│   ├── Scenes/   IScenesClient, ScenesClient            DCB-GTW-SCN
│   ├── Config/   IConfigClient, ConfigClient            DCB-GTW-CFG
│   └── FullState/ IFullStateClient, FullStateClient     DCB-GTW-FUL (GET /api/{key})
├── Authentication/                     DCB-GTW-AUT
│   ├── IDeconzAuthenticator.cs
│   └── DeconzAuthenticator.cs
├── WebSocket/                          DCB-GTW-WSS
│   ├── IDeconzEventStream.cs          flusso degli eventi + ConnectionStateChanged
│   ├── DeconzEventStream.cs
│   └── IWebSocketConnection.cs        involucro di ClientWebSocket, sostituibile nei test
├── Dto/{Shared, Lights, Sensors, Groups, Rules, Config, FullState, WebSocket}   (ANA-06 §4.5)
└── DependencyInjection/
    └── ServiceCollectionExtensions.cs  services.AddDeconzGateway(...)
```

I client degli altri sottomoduli (`DCB-GTW-RUL`, `-SCH`, `-DEV`, `-ALM`, `-TLK`, `-RLK`, `-INF`) seguono lo stesso
schema e sono rinviati (PA-16, ANA-06 §4.6). Ordine di sviluppo della prima versione:

| Incremento | Contenuto | Passo di ANA-06 §10 |
| --- | --- | --- |
| 1 | base, `Configuration`, `AddDeconzGateway`, `CFG`, `AUT` + test | 3 |
| 2 | `FUL` + test (esempio reale, PA-01) | 3 |
| 3 | `LIG`, `SEN`, `GRP`, `SCN` (comandi, `DeconzCommandResult`) + test | 4 |
| 4 | `WSS` con `ConnectionStateChanged` + test | 5 |

## 3. Scelte tecniche (confermate l'11/11/2026)

| Tema | Scelta | Motivo |
| --- | --- | --- |
| Accesso HTTP | Solo `DeconzRestClient` usa `HttpClient`; i client per area indicano risorsa, metodo e corpo | La gestione degli errori deCONZ sta in un solo posto (stessa logica degli script Postman `DCB-PMN`) |
| Interfacce | Ogni client ha un'interfaccia; `DCB.Server` dipende solo dalle interfacce | Sostituibilità nei test e in `DCB.Server` |
| Errori deCONZ | `DeconzApiException` con tipo, indirizzo e descrizione; per i comandi `PUT` con esito misto (`success` + `error`) un `DeconzCommandResult` che riporta entrambi | deCONZ può segnalare errori con HTTP 2xx e applicare solo in parte un comando |
| Notifiche di `DCB-GTW-AUT` | Eventi .NET su `IDeconzAuthenticator` (`WaitingForUnlock`, `ApiKeyObtained`) | Il meccanismo più semplice da leggere; `DCB.Server` li trasforma in condizioni dello stato attuale |
| Flusso di `DCB-GTW-WSS` | `IAsyncEnumerable<DeconzWsEventDto>` più l'evento `ConnectionStateChanged` (*collegato*, *scollegato*, *in riconnessione*) | Lettura sequenziale naturale; la notifica avvia la rilettura di `DCB-SYN` e la condizione "riconnessione WebSocket" (PA-13) |
| DTO delle richieste | Proprietà *nullable*, `null` esclusi dal JSON (`JsonIgnoreCondition.WhenWritingNull`); suffisso `RequestDto`; `DeconzLightStateRequestDto` → `DeconzGroupActionRequestDto`; `DeconzSensorConfigRequestDto` a dizionario | deCONZ applica ogni campo presente nel corpo (PA-15) |
| Unità e validazione | Unità deCONZ (`bri` 0–255, `ct` in mired, `transitiontime` in decimi di secondo) documentate nei `///`; nessuna validazione degli intervalli in `GTW` | Conversione e validazione spettano a `DCB.Server` (PA-15) |
| Stato completo | `DeconzFullStateDto` composto dai DTO esistenti; `rules` e `schedules` non mappate; la `whitelist` non va mai nei log | PA-14 |
| Sensori con campi variabili | Proprietà comuni tipizzate, il resto in `Dictionary<string, JsonElement>` | I campi di `state` e `config` cambiano con il tipo di sensore |
| Serializzazione | `System.Text.Json` con nomi deCONZ espliciti (`[JsonPropertyName]`) | Nessuna dipendenza da convenzioni di nomi implicite |
| Sicurezza nei log | L'API key nell'URL (`/api/{key}/…`) viene **mascherata** nei log | La chiave dà pieno controllo del gateway |
| Tempo | `TimeProvider` per attese, tentativi e riconnessioni | Test dei 60 s di sblocco e delle attese crescenti in millisecondi |
| Test (`DCB-TST`) | `HttpMessageHandler` finto, `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`), WebSocket finto | Verifica della logica senza deCONZ (ANA-06 §8) |

**Pacchetti previsti** in `DCB.Gateway`: `Microsoft.Extensions.Http`, `Microsoft.Extensions.Options`,
`Microsoft.Extensions.Logging.Abstractions`.

## 4. Punti aperti e decisioni collegate

| ID | Punto | Stato | Documento |
| --- | --- | --- | --- |
| PA-01 | Esempi JSON reali di deCONZ per i test dei DTO | aperto | `PuntiApertiCoding.md` |
| PA-04 | Passi di bootstrap: sequenza approvata; restano deCONZ non raggiungibile, regole `sbs`, attese crescenti | aperto | `PuntiApertiAnalisi.md`, ANA-06 §4.2 |
| PA-13 | Rilettura dello stato dopo una riconnessione → `ConnectionStateChanged` + `DCB-SYN` | chiuso 11/11/2026 | ANA-06 §4.4, §4.7 |
| PA-14 | Stato completo → nuovo `DCB-GTW-FUL` | chiuso 11/11/2026 | ANA-06 §4.5 |
| PA-15 | DTO delle richieste | chiuso 11/11/2026 | ANA-06 §4.5 |
| PA-16 | Ampiezza della prima versione e incrementi | chiuso 11/11/2026 | ANA-06 §4.6, §10 |
| PA-17 | Parametri: `appsettings.json` + `cfg.json` + `apikey.json`; `IApiKeyStore` su `KEY` | chiuso 11/11/2026 | ANA-06 §4.8, ANA-07 |

## 5. Stato di avanzamento

| Incremento | Stato | Note |
| --- | --- | --- |
| 1 – base, `CFG`, `AUT` | **completato l'11/11/2026** | 21 test superati, build senza avvisi |
| 2 – `FUL` | **completato l'11/11/2026** | 27 test superati (totale), con lo stato completo reale dell'impianto |
| 3 – comandi `LIG`, `SEN`, `GRP`, `SCN` | **completato l'11/11/2026** | 37 test superati (totale) |
| 4 – `WSS` | **completato l'11/11/2026** | 44 test superati (totale); prova dal vivo sull'impianto riuscita |

**Dettagli dell'incremento 1** (oltre a quanto previsto al §2):
- `DeconzGatewayOptions`: parametri `Host`, `HttpPort`, `WsPort`, `DeviceType`, `UnlockRetrySeconds` e il nuovo
  `RequestTimeoutSeconds` (tempo massimo di risposta REST, predefinito 10 s); validati alla registrazione.
- `Core`: oltre a `DeconzRestClient` e `DeconzErrorReader`, `DeconzApiKeyHolder` (chiave in uso, impostata da `AUT`),
  `DeconzJson` (opzioni JSON comuni, `null` esclusi), `DeconzErrorTypes`, `DeconzApiException`,
  `DeconzUnreachableException` (applicazione provvisoria di PA-04 (a): un solo tentativo, nessuna ripetizione in GTW).
- `DeconzRestClient` crea il client HTTP a ogni richiesta da `IHttpClientFactory` (client con nome `Deconz`), così i servizi
  possono essere singleton; il tempo massimo usa `TimeProvider`; l'API key è mascherata (`***`) nei log.
- `DeconzConfigDto` non mappa la `whitelist`. `ConfigClient.GetConfigAsync` tratta come chiave non valida una risposta
  senza `websocketport` (verifica del comportamento reale: PA-18).
- `IDeconzAuthenticator.EnsureAuthenticatedAsync` restituisce la configurazione completa letta con la chiave valida
  (porta WebSocket e versione per il passo `auth`); l'evento `ApiKeyObtained` non contiene la chiave.

**Dettagli dell'incremento 2**:
- DTO: `DeconzBaseDeviceDto` (campi comuni di luci e sensori), `DeconzLightDto`, `DeconzLightStateDto`
  (comprende `lift` e `open` delle tapparelle), `DeconzSensorDto` (`state` e `config` a dizionario), `DeconzGroupDto`,
  `DeconzGroupStateDto`, `DeconzSceneDto`, `DeconzFullStateDto`; `DeconzConfigDto` legge anche `websocketport_wss`.
- **Campi non mappati conservati** (`[JsonExtensionData]`) nei DTO di dispositivi, stati, gruppi e scene: `DBZ` è una
  copia grezza e una rilettura seguita da una scrittura non deve perdere informazioni. Non su `DeconzConfigDto` e
  `DeconzFullStateDto`, dove conserverebbe la `whitelist`.
- **Date come stringhe grezze** (`lastseen`, `lastupdated`…): deCONZ usa formati diversi (`2026-11-11T18:27Z` senza
  secondi, `2026-11-11T18:21:27.822` senza fuso) e anche il valore `none`.
- Dati reali in `DCB.Gateway.Tests\TestData\Deconz\full-state.json` (senza `whitelist`, PA-01), copiato nella cartella
  di output dei test.
- Scoperte dai dati reali: le tapparelle Vimar sono risorse `lights` di tipo `Window covering device` (comandi da
  prevedere nell'incremento 3: `lift`, `open`, `stop`); la porta WebSocket non cifrata è 80, non 443 (PA-19).

**Dettagli dell'incremento 3**:
- Client `ILightsClient`, `ISensorsClient`, `IGroupsClient` (elenco, dettaglio, comando) e `IScenesClient`
  (solo richiamo, `PUT` senza corpo), registrati in `AddDeconzGateway`.
- `DeconzLightStateRequestDto` comprende anche i comandi delle tapparelle `lift`, `open` e `stop`
  (approvati l'11/11/2026); `DeconzGroupActionRequestDto` eredita e aggiunge `toggle`; `DeconzSensorConfigRequestDto`
  è un dizionario con `Set(nome, valore)`, i cui campi compaiono al primo livello del JSON.
- `DeconzCommandResult` (`Applied`: indirizzo → valore, `Errors`, `IsFullySuccessful`): un comando applicato in parte
  restituisce il risultato misto; un comando rifiutato del tutto solleva `DeconzApiException`.
- `DeconzResourcePath` codifica gli id nei percorsi: un id come `1/../config` non può indirizzare un'altra risorsa.

**Dettagli dell'incremento 4**:
- `IDeconzEventStream`: `ReadEventsAsync` (flusso `IAsyncEnumerable<DeconzWsEventDto>`, un solo consumatore), proprietà
  `State` ed evento `ConnectionStateChanged` (`Connected`, `Disconnected`, `Reconnecting`) con `IsReconnection`, vero
  quando la connessione torna dopo essere stata persa: è il segnale per la rilettura di `DCB-SYN`.
- Connessione `ws://` sulla porta `Deconz:WsPort` o, se vuota, su `websocketport` riletta da `/config` a ogni connessione
  (PA-19). Riconnessione con attese crescenti (2 s, 4 s, 8 s… fino a 60 s); messaggi non JSON o diversi da
  `"t":"event"` ignorati.
- `IWebSocketConnection` / `IWebSocketConnectionFactory` (interni) isolano `ClientWebSocket`: nei test una connessione
  finta programmabile. `ClientWebSocketConnection` ricompone i messaggi frammentati e usa i ping
  (`KeepAliveInterval` + `KeepAliveTimeout`) per accorgersi delle connessioni "morte".
- Nuovi parametri: `WsReconnectInitialDelaySeconds`, `WsReconnectMaxDelaySeconds`, `WsKeepAliveSeconds` (ANA-07 §3.1).
- `DeconzWsEventDto`: `t`, `e`, `r`, `id`, `uniqueid`, `state` / `config` / `attr` a dizionario, risorsa intera per
  `added` (`light`, `sensor`, `group`), `gid` / `scid` per `scene-called`.
- **Prova dal vivo** (11/11/2026): programma temporaneo con `AddDeconzGateway` + `IDeconzEventStream` collegato a deCONZ
  sulla porta 80: connessione riuscita, eventi reali ricevuti (`attr.lastseen` delle tapparelle).

**Prima versione di GTW completata** (incrementi 1–4, PA-16). Restano aperti PA-04 (regole del bootstrap) e PA-18
(risposta di deCONZ a una chiave non valida).
