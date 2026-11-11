# COD-02 – Gateway Connector (`DCB.Gateway`)

**Data:** 11/11/2026
**Fonti:** ANA-03 §4.3–4.6 e §8, ANA-04 §3, sessione di coding dell'11/11/2026
**Stato:** organizzazione e scelte tecniche confermate; restano i punti aperti del §4

---

## 1. Ruolo

`DCB.Gateway` (`DCB-GTW`) è il **client di deCONZ**: chiama le REST API e riceve gli eventi del WebSocket.
È una libreria autonoma (ANA-03 §4.6): non espone endpoint propri (il canale verso i client è `DCB-API` + `DCB-EVS`
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
│   └── Config/   IConfigClient, ConfigClient            DCB-GTW-CFG
├── Authentication/                     DCB-GTW-AUT
│   ├── IDeconzAuthenticator.cs
│   └── DeconzAuthenticator.cs
├── WebSocket/                          DCB-GTW-WSS
│   ├── IDeconzEventStream.cs
│   ├── DeconzEventStream.cs
│   └── IWebSocketConnection.cs        involucro di ClientWebSocket, sostituibile nei test
├── Dto/{Shared, Lights, Sensors, Groups, Rules, Config, WebSocket}   (ANA-03 §4.5)
└── DependencyInjection/
    └── ServiceCollectionExtensions.cs  services.AddDeconzGateway(...)
```

I client degli altri sottomoduli (`DCB-GTW-RUL`, `-SCH`, `-DEV`, `-ALM`, `-TLK`, `-RLK`, `-INF`) seguono lo stesso
schema; la loro inclusione nella prima versione è il punto PA-16.

## 3. Scelte tecniche (confermate l'11/11/2026)

| Tema | Scelta | Motivo |
| --- | --- | --- |
| Accesso HTTP | Solo `DeconzRestClient` usa `HttpClient`; i client per area indicano risorsa, metodo e corpo | La gestione degli errori deCONZ sta in un solo posto (stessa logica degli script Postman `DCB-PMN`) |
| Interfacce | Ogni client ha un'interfaccia; `DCB.Server` dipende solo dalle interfacce | Sostituibilità nei test e in `DCB.Server` |
| Errori deCONZ | `DeconzApiException` con tipo, indirizzo e descrizione; per i comandi `PUT` con esito misto (`success` + `error`) un risultato che riporta entrambi | deCONZ può segnalare errori con HTTP 2xx e applicare solo in parte un comando |
| Notifiche di `DCB-GTW-AUT` | Eventi .NET su `IDeconzAuthenticator` (`WaitingForUnlock`, `ApiKeyObtained`) | Il meccanismo più semplice da leggere; `DCB.Server` li trasforma in condizioni dello stato attuale |
| Flusso di `DCB-GTW-WSS` | `IAsyncEnumerable<DeconzWsEventDto>` più una notifica "riconnesso" | Lettura sequenziale naturale; la notifica serve alla rilettura dello stato (PA-13) |
| Sensori con campi variabili | Proprietà comuni tipizzate, il resto in `Dictionary<string, JsonElement>` | I campi di `state` e `config` cambiano con il tipo di sensore |
| Serializzazione | `System.Text.Json` con nomi deCONZ espliciti (`[JsonPropertyName]`) | Nessuna dipendenza da convenzioni di nomi implicite |
| Sicurezza nei log | L'API key nell'URL (`/api/{key}/…`) viene **mascherata** nei log | La chiave dà pieno controllo del gateway |
| Tempo | `TimeProvider` per attese, tentativi e riconnessioni | Test dei 60 s di sblocco e delle attese crescenti in millisecondi |
| Test (`DCB-TST`) | `HttpMessageHandler` finto, `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`), WebSocket finto | Verifica della logica senza deCONZ (ANA-03 §8) |

**Pacchetti previsti** in `DCB.Gateway`: `Microsoft.Extensions.Http`, `Microsoft.Extensions.Options`,
`Microsoft.Extensions.Logging.Abstractions`.

## 4. Punti aperti collegati

| ID | Punto | Documento |
| --- | --- | --- |
| PA-01 | Esempi JSON reali di deCONZ per i test dei DTO | `PuntiApertiCoding.md` |
| PA-04 | Passi di bootstrap, compreso il comportamento con deCONZ non raggiungibile | `PuntiApertiAnalisi.md` |
| PA-13 | Chi rilegge lo stato completo dopo una riconnessione del WebSocket | `PuntiApertiAnalisi.md` |
| PA-14 | Sottomodulo dello stato completo (`GET /api/{key}`) | `PuntiApertiAnalisi.md` |
| PA-15 | DTO delle richieste (comandi) | `PuntiApertiAnalisi.md` |
| PA-16 | Ampiezza della prima versione di GTW | `PuntiApertiAnalisi.md` |
| PA-17 | Dove stanno i parametri: `appsettings.json` o `cfg.json` | `PuntiApertiAnalisi.md` |
