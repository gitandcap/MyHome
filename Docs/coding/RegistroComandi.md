# Registro dei comandi del progetto

**Aggiornato al:** 11/11/2026
**Stato:** documento permanente (nome fisso, senza progressivo; era `COD-01-RegistroComandi.md`), aggiornato ogni volta
che si esegue un comando per il progetto

---

Raccoglie in ordine i comandi eseguiti per costruire il progetto (solution, progetti, pacchetti, strumenti…), così il
percorso è ripetibile. Non contiene i comandi di sola consultazione (versioni, stato, elenchi) né quelli di
versionamento (`git add`, `commit`, `push`, `mv`…), la cui storia è già in git: tolti l'11/11/2026, anche dalle righe
precedenti, con la conseguente rinumerazione dei comandi.
Tutti i comandi si eseguono da PowerShell nella cartella radice `C:\Users\Andrea\source\repos\MyHome`.

## 1. Prerequisiti

| Strumento | Versione | Note |
| --- | --- | --- |
| .NET SDK | 10.0.401 | già installato |
| Git per Windows | 2.56.0 | installato da Andrea il 09/11/2026 (es. `winget install Git.Git`) |
| Repository GitHub | – | `https://github.com/gitandcap/MyHome` (remoto `origin`, ramo `main`) |

## 2. Repository e solution (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| 1 | `dotnet new gitignore` | Crea il `.gitignore` standard per .NET e Visual Studio (esclude `bin`, `obj`, file utente…). |
| 2 | `dotnet new sln -n MyHome --format sln` | Crea la solution `MyHome.sln` nel formato `.sln` classico (in .NET 10 il predefinito sarebbe `.slnx`). |
| 3 | `dotnet new classlib -n MyHome.Shared -f net10.0` | Crea la libreria del codice condiviso (`SHR`). |
| 4 | `dotnet new classlib -n DCB.Gateway -f net10.0` | Crea la libreria del Gateway Connector verso deCONZ (`DCB-GTW`). |
| 5 | `dotnet new xunit -n DCB.Gateway.Tests -f net10.0` | Crea il progetto di test xUnit di GTW (`DCB-TST`). |
| 6 | `dotnet new classlib -n DCB.Infrastructure -f net10.0` | Crea la libreria dello storage e delle implementazioni (`DCB-STO`). |
| 7 | `dotnet new web -n DCB.Server -f net10.0` | Crea il server ASP.NET Core vuoto (API REST, SSE, bootstrap). |
| 8 | `dotnet new console -n TCL.Client -f net10.0` | Crea l'applicazione console TCL. |
| 9 | `dotnet sln MyHome.sln add MyHome.Shared DCB.Gateway DCB.Gateway.Tests DCB.Infrastructure DCB.Server TCL.Client` | Aggiunge i sei progetti alla solution. |
| 10 | `dotnet add DCB.Infrastructure reference DCB.Gateway MyHome.Shared` | Riferimenti di `DCB.Infrastructure` (ANA-03 §3). |
| 11 | `dotnet add DCB.Server reference DCB.Infrastructure MyHome.Shared` | Riferimenti di `DCB.Server`. |
| 12 | `dotnet add DCB.Gateway.Tests reference DCB.Gateway` | Il progetto di test vede solo GTW. |
| 13 | `dotnet add TCL.Client reference MyHome.Shared` | La TCL vede solo il codice condiviso, così è distribuibile da sola. |
| 14 | `Remove-Item MyHome.Shared\Class1.cs, DCB.Gateway\Class1.cs, DCB.Infrastructure\Class1.cs, DCB.Gateway.Tests\UnitTest1.cs` | Elimina i file d'esempio creati dai modelli. |
| – | *(file creato con l'editor)* `Directory.Build.props` | Impostazioni di build comuni a tutti i progetti (documentazione XML, versione C#). |
| 15 | `dotnet build MyHome.sln` | Compila la solution per verificare progetti e riferimenti (0 avvisi, 0 errori). |

## 3. Raccolta Postman su deCONZ (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati con l'editor)* `Tools\Postman\*.json` | Raccolta `MyHome-deCONZ` e ambienti `deCONZ locale` / `deCONZ container` (`DCB-PMN`). |

## 4. Registro dei comandi (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati/modificati con l'editor)* `Docs\coding\COD-01-RegistroComandi.md`, `CLAUDE.md`, `Docs\analisi\RegistroDocumenti.md` | Questo registro, la regola che lo rende obbligatorio e la sua voce nel registro dei documenti. |

## 5. Verifica di deCONZ con Postman (09/11/2026)

API key ottenuta da Postman, porta WebSocket 443 confermata (dato poi corretto l'11/11/2026: la 443 è `websocketport_wss`,
il WebSocket usato è `ws://` sulla 80, PA-19), cartelle di sola lettura eseguite senza errori.

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file modificati con l'editor)* `Docs\analisi\ANA-03-SintesiUnificata.md`, `Docs\coding\COD-01-RegistroComandi.md` | Porta WebSocket 443 confermata (ANA-03 §9) e aggiornamento di questo registro. |

## 6. Punti aperti e documenti permanenti (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati/modificati con l'editor)* `Docs\analisi\PuntiApertiAnalisi.md`, `Docs\coding\PuntiApertiCoding.md`, `CLAUDE.md`, `Docs\analisi\RegistroDocumenti.md`, `Docs\analisi\ANA-03-SintesiUnificata.md` | Documenti permanenti dei punti aperti (analisi e coding), regole in `CLAUDE.md`, voci nel registro dei documenti, rimando in ANA-03 §9. |

## 7. Gateway Connector e decisioni di analisi (11/11/2026)

ANA-03 e ANA-04 sono stati spostati in `Docs\analisi\storico\` (con `git mv`) prima di creare ANA-06 e ANA-07.

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati/modificati con l'editor)* `Docs\coding\COD-02-GatewayConnector.md`, `Docs\analisi\PuntiApertiAnalisi.md`, `Docs\analisi\RegistroDocumenti.md` | Organizzazione e scelte tecniche di `DCB.Gateway` (COD-02), nuovi punti aperti PA-13…PA-17 e PA-04 ampliato, registrazione di COD-02. |
| 16 | `cp storico/ANA-03-SintesiUnificata.md ANA-06-SintesiUnificata.md` | Crea ANA-06 partendo dal testo di ANA-03 (eseguito in `Docs\analisi`). |
| 17 | `cp storico/ANA-04-FileJsonDCB.md ANA-07-FileJsonDCB.md` | Crea ANA-07 partendo dal testo di ANA-04 (eseguito in `Docs\analisi`). |
| – | *(file creati/modificati con l'editor)* `ANA-06-SintesiUnificata.md`, `ANA-07-FileJsonDCB.md`, `ANA-05-CanaleClientDCB.md`, `Acronimi.md`, `PuntiApertiAnalisi.md`, `RegistroDocumenti.md`, `Docs\coding\COD-02-GatewayConnector.md`, `CLAUDE.md` | Decisioni su PA-04 (sequenza), PA-13…PA-17: nuovi `DCB-SYN`, `DCB-GTW-FUL`, `KEY`; punti chiusi; rimandi da ANA-03/ANA-04 ad ANA-06/ANA-07. |
| 18 | `dotnet add DCB.Gateway package Microsoft.Extensions.Http` (e `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions`); `dotnet add DCB.Gateway.Tests package Microsoft.Extensions.TimeProvider.Testing` | Primo inserimento dei pacchetti di GTW e dei test: senza versione NuGet ha scelto la 11.0.0 (.NET 11), sostituita al comando successivo. |
| 19 | `dotnet add DCB.Gateway package <pacchetto> --version 10.0.15` (per i tre pacchetti di GTW) | Allinea i pacchetti `Microsoft.Extensions.*` di GTW a .NET 10 LTS, come il resto della solution. |
| 20 | `dotnet add DCB.Gateway.Tests package Microsoft.Extensions.TimeProvider.Testing --version 10.11.0` | `FakeTimeProvider` per i test; il pacchetto ha una numerazione propria (repository dotnet/extensions), la 10.0.15 non esiste. |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\**` (Configuration, Abstractions, Core, Clients\Config, Authentication, Dto, DependencyInjection), `DCB.Gateway.csproj` (`InternalsVisibleTo`), `DCB.Gateway.Tests\**` | Incremento 1 di GTW (COD-02): base REST, `DCB-GTW-CFG`, `DCB-GTW-AUT`, registrazione e test. |
| 21 | `dotnet build DCB.Gateway` | Compila la libreria (0 avvisi, 0 errori). |
| 22 | `dotnet test` | Esegue i test della solution: 21 superati. |
| 23 | *(PowerShell)* lettura di `%USERPROFILE%\Downloads\deconz-full-state.json` con `System.Text.Json.Nodes`, rimozione di `config.whitelist`, scrittura in `DCB.Gateway.Tests\TestData\Deconz\full-state.json` | Copia nel repository dello stato completo reale salvato da Postman, senza le API key della whitelist (PA-01). |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\Dto\**`, `DCB.Gateway\Clients\FullState\**`, `ServiceCollectionExtensions.cs`, `DCB.Gateway.Tests\**`, `DCB.Gateway.Tests.csproj` (copia dei file JSON di prova) | Incremento 2 di GTW: `DCB-GTW-FUL`, DTO di luci, sensori e gruppi, test sui dati reali. |
| 24 | `dotnet test` | Esegue i test: 27 superati. |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\Clients\{Lights,Sensors,Groups,Scenes}\**`, `DCB.Gateway\Core\{DeconzCommandResult,DeconzResourcePath,DeconzRestClient}.cs`, `DCB.Gateway\Dto\**\*RequestDto.cs`, `ServiceCollectionExtensions.cs`, `DCB.Gateway.Tests\**` | Incremento 3 di GTW: comandi di `LIG`, `SEN`, `GRP`, `SCN`, esito `DeconzCommandResult`, test. |
| 25 | `dotnet build DCB.Gateway` | Compila la libreria (0 avvisi, 0 errori). |
| 26 | `dotnet test` | Esegue i test: 37 superati. |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\WebSocket\**`, `DCB.Gateway\Dto\WebSocket\DeconzWsEventDto.cs`, `DeconzGatewayOptions.cs`, `ServiceCollectionExtensions.cs`, `DCB.Gateway.Tests\**`, `Tools\Postman\deCONZ-locale.postman_environment.json` (`deconzWsUrl` → `ws://127.0.0.1:80`) | Incremento 4 di GTW (`DCB-GTW-WSS`) e correzione dell'ambiente Postman (PA-19). |
| 27 | `dotnet build DCB.Gateway` | Compila la libreria (0 avvisi, 0 errori). |
| 28 | `dotnet test` (ripetuto 5 volte con `--no-build`) | Esegue i test: 44 superati a ogni esecuzione (verifica di stabilità dei test con tempo simulato). |
| 29 | `dotnet run WsProva.cs` *(programma temporaneo nella scratchpad, fuori dal repository)* | Prova dal vivo di `IDeconzEventStream` sul deCONZ dell'impianto: connessione `ws://127.0.0.1:80` riuscita, eventi ricevuti. |
| – | *(file modificati con l'editor)* `ANA-06-SintesiUnificata.md` (§4.2, §9), `ANA-07-FileJsonDCB.md` (§3.1), `PuntiApertiAnalisi.md`, `COD-02-GatewayConnector.md` | Decisioni su PA-04: ripetizioni in `DCB-BOT`, regole `sbs`, parametri `Bootstrap:*` delle attese; PA-04 chiuso. |
| – | *(file modificato con l'editor)* `DCB.Gateway\Core\DeconzUnreachableException.cs` | Commento aggiornato dopo la chiusura di PA-04 (non più scelta provvisoria). |
| 30 | `dotnet build DCB.Gateway` | Compila la libreria dopo la modifica del commento (0 avvisi, 0 errori). |
| – | *(file modificati con l'editor)* `CLAUDE.md`, `Docs\coding\RegistroComandi.md` | Nuova regola: i comandi di versionamento non si registrano; tolte dal registro le righe git già presenti e rinumerati i comandi. |
