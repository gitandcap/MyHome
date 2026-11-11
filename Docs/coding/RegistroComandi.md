# Registro dei comandi del progetto

**Aggiornato al:** 09/11/2026
**Stato:** documento permanente (nome fisso, senza progressivo; era `COD-01-RegistroComandi.md`), aggiornato ogni volta
che si esegue un comando per il progetto

---

Raccoglie in ordine i comandi eseguiti per costruire il progetto (solution, progetti, git…), così il percorso è
ripetibile. Non contiene i comandi di sola consultazione (versioni, stato, elenchi).
Tutti i comandi si eseguono da PowerShell nella cartella radice `C:\Users\Andrea\source\repos\MyHome`.

## 1. Prerequisiti

| Strumento | Versione | Note |
| --- | --- | --- |
| .NET SDK | 10.0.401 | già installato |
| Git per Windows | 2.56.0 | installato da Andrea il 09/11/2026 (es. `winget install Git.Git`) |

## 2. Repository e solution (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| 1 | `git init -b main` | Crea il repository git locale con ramo principale `main`. |
| 2 | `dotnet new gitignore` | Crea il `.gitignore` standard per .NET e Visual Studio (esclude `bin`, `obj`, file utente…). |
| 3 | `dotnet new sln -n MyHome --format sln` | Crea la solution `MyHome.sln` nel formato `.sln` classico (in .NET 10 il predefinito sarebbe `.slnx`). |
| 4 | `dotnet new classlib -n MyHome.Shared -f net10.0` | Crea la libreria del codice condiviso (`SHR`). |
| 5 | `dotnet new classlib -n DCB.Gateway -f net10.0` | Crea la libreria del Gateway Connector verso deCONZ (`DCB-GTW`). |
| 6 | `dotnet new xunit -n DCB.Gateway.Tests -f net10.0` | Crea il progetto di test xUnit di GTW (`DCB-TST`). |
| 7 | `dotnet new classlib -n DCB.Infrastructure -f net10.0` | Crea la libreria dello storage e delle implementazioni (`DCB-STO`). |
| 8 | `dotnet new web -n DCB.Server -f net10.0` | Crea il server ASP.NET Core vuoto (API REST, SSE, bootstrap). |
| 9 | `dotnet new console -n TCL.Client -f net10.0` | Crea l'applicazione console TCL. |
| 10 | `dotnet sln MyHome.sln add MyHome.Shared DCB.Gateway DCB.Gateway.Tests DCB.Infrastructure DCB.Server TCL.Client` | Aggiunge i sei progetti alla solution. |
| 11 | `dotnet add DCB.Infrastructure reference DCB.Gateway MyHome.Shared` | Riferimenti di `DCB.Infrastructure` (ANA-03 §3). |
| 12 | `dotnet add DCB.Server reference DCB.Infrastructure MyHome.Shared` | Riferimenti di `DCB.Server`. |
| 13 | `dotnet add DCB.Gateway.Tests reference DCB.Gateway` | Il progetto di test vede solo GTW. |
| 14 | `dotnet add TCL.Client reference MyHome.Shared` | La TCL vede solo il codice condiviso, così è distribuibile da sola. |
| 15 | `Remove-Item MyHome.Shared\Class1.cs, DCB.Gateway\Class1.cs, DCB.Infrastructure\Class1.cs, DCB.Gateway.Tests\UnitTest1.cs` | Elimina i file d'esempio creati dai modelli. |
| – | *(file creato con l'editor)* `Directory.Build.props` | Impostazioni di build comuni a tutti i progetti (documentazione XML, versione C#). |
| 16 | `dotnet build MyHome.sln` | Compila la solution per verificare progetti e riferimenti (0 avvisi, 0 errori). |
| 17 | `git add -A` | Prepara per il commit tutti i file del progetto. |
| 18 | `git commit -m "Struttura iniziale della solution MyHome"` | Primo commit locale. |

## 3. Collegamento a GitHub (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| 19 | `git remote add origin https://github.com/gitandcap/MyHome.git` | Collega il repository remoto GitHub con il nome `origin`. |
| 20 | `git remote set-url origin https://github.com/gitandcap/MyHome.git` | Corregge l'indirizzo del remoto (la prima volta era stato scritto `githun.com`). |
| 21 | `git push -u origin main` | Invia `main` a GitHub e lo imposta come ramo remoto predefinito. Le credenziali erano già salvate da Git Credential Manager. |

## 4. Raccolta Postman su deCONZ (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati con l'editor)* `Tools\Postman\*.json` | Raccolta `MyHome-deCONZ` e ambienti `deCONZ locale` / `deCONZ container` (`DCB-PMN`). |
| 22 | `git add Tools` | Prepara per il commit la cartella della raccolta Postman. |
| 23 | `git commit -m "Raccolta Postman su deCONZ (DCB-PMN)"` | Commit locale della raccolta (non ancora inviato a GitHub). |

## 5. Registro dei comandi (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati/modificati con l'editor)* `Docs\coding\COD-01-RegistroComandi.md`, `CLAUDE.md`, `Docs\analisi\RegistroDocumenti.md` | Questo registro, la regola che lo rende obbligatorio e la sua voce nel registro dei documenti. |
| 24 | `git add Docs CLAUDE.md` | Prepara per il commit la documentazione aggiornata. |
| 25 | `git commit -m "Registro dei comandi (COD-01)"` | Commit locale del registro dei comandi. |
| 26 | `git push` | Invia a GitHub i commit della raccolta Postman e del registro dei comandi (eseguito da Andrea). |

## 6. Verifica di deCONZ con Postman (09/11/2026)

API key ottenuta da Postman, porta WebSocket 443 confermata, cartelle di sola lettura eseguite senza errori.

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file modificati con l'editor)* `Docs\analisi\ANA-03-SintesiUnificata.md`, `Docs\coding\COD-01-RegistroComandi.md` | Porta WebSocket 443 confermata (ANA-03 §9) e aggiornamento di questo registro. |
| 27 | `git add Docs` | Prepara per il commit la documentazione aggiornata. |
| 28 | `git commit -m "Porta WebSocket deCONZ confermata, registro comandi aggiornato"` | Commit locale. |
| 29 | `git push` | Invia a GitHub il commit precedente (eseguito da Andrea). |

## 7. Punti aperti e documenti permanenti (09/11/2026)

| # | Comando | Descrizione |
| --- | --- | --- |
| – | *(file creati/modificati con l'editor)* `Docs\analisi\PuntiApertiAnalisi.md`, `Docs\coding\PuntiApertiCoding.md`, `CLAUDE.md`, `Docs\analisi\RegistroDocumenti.md`, `Docs\analisi\ANA-03-SintesiUnificata.md` | Documenti permanenti dei punti aperti (analisi e coding), regole in `CLAUDE.md`, voci nel registro dei documenti, rimando in ANA-03 §9. |
| 30 | `git mv Docs/coding/COD-01-RegistroComandi.md Docs/coding/RegistroComandi.md` | Rinomina questo registro a nome fisso: è un documento permanente. `git mv` conserva la storia del file. |
| 31 | `git add -A` | Prepara per il commit tutte le modifiche (compresi i nuovi documenti dei punti aperti). |
| 32 | `git commit -m "Documenti permanenti: punti aperti e registro comandi"` | Commit locale. |
| 33 | `git push` | Invia a GitHub il commit precedente (eseguito da Andrea). |
| – | *(file creati/modificati con l'editor)* `Docs\coding\COD-02-GatewayConnector.md`, `Docs\analisi\PuntiApertiAnalisi.md`, `Docs\analisi\RegistroDocumenti.md` | Organizzazione e scelte tecniche di `DCB.Gateway` (COD-02), nuovi punti aperti PA-13…PA-17 e PA-04 ampliato, registrazione di COD-02. |
| 34 | `git add -A` | Prepara per il commit le modifiche ai documenti. |
| 35 | `git commit -m "COD-02 Gateway Connector: organizzazione, scelte tecniche e punti aperti"` | Commit locale. |
| 36 | `git push` | Invia a GitHub il commit precedente (eseguito da Andrea). |
| 37 | `git mv Docs/analisi/ANA-03-SintesiUnificata.md Docs/analisi/storico/` | Sposta ANA-03 nello storico: sostituito da ANA-06 (modifiche importanti dell'11/11/2026). |
| 38 | `git mv Docs/analisi/ANA-04-FileJsonDCB.md Docs/analisi/storico/` | Sposta ANA-04 nello storico: sostituito da ANA-07. |
| 39 | `cp storico/ANA-03-SintesiUnificata.md ANA-06-SintesiUnificata.md` | Crea ANA-06 partendo dal testo di ANA-03 (eseguito in `Docs\analisi`). |
| 40 | `cp storico/ANA-04-FileJsonDCB.md ANA-07-FileJsonDCB.md` | Crea ANA-07 partendo dal testo di ANA-04 (eseguito in `Docs\analisi`). |
| – | *(file creati/modificati con l'editor)* `ANA-06-SintesiUnificata.md`, `ANA-07-FileJsonDCB.md`, `ANA-05-CanaleClientDCB.md`, `Acronimi.md`, `PuntiApertiAnalisi.md`, `RegistroDocumenti.md`, `Docs\coding\COD-02-GatewayConnector.md`, `CLAUDE.md` | Decisioni su PA-04 (sequenza), PA-13…PA-17: nuovi `DCB-SYN`, `DCB-GTW-FUL`, `KEY`; punti chiusi; rimandi da ANA-03/ANA-04 ad ANA-06/ANA-07. |
| 41 | `dotnet add DCB.Gateway package Microsoft.Extensions.Http` (e `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions`); `dotnet add DCB.Gateway.Tests package Microsoft.Extensions.TimeProvider.Testing` | Primo inserimento dei pacchetti di GTW e dei test: senza versione NuGet ha scelto la 11.0.0 (.NET 11), sostituita al comando successivo. |
| 42 | `dotnet add DCB.Gateway package <pacchetto> --version 10.0.15` (per i tre pacchetti di GTW) | Allinea i pacchetti `Microsoft.Extensions.*` di GTW a .NET 10 LTS, come il resto della solution. |
| 43 | `dotnet add DCB.Gateway.Tests package Microsoft.Extensions.TimeProvider.Testing --version 10.11.0` | `FakeTimeProvider` per i test; il pacchetto ha una numerazione propria (repository dotnet/extensions), la 10.0.15 non esiste. |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\**` (Configuration, Abstractions, Core, Clients\Config, Authentication, Dto, DependencyInjection), `DCB.Gateway.csproj` (`InternalsVisibleTo`), `DCB.Gateway.Tests\**` | Incremento 1 di GTW (COD-02): base REST, `DCB-GTW-CFG`, `DCB-GTW-AUT`, registrazione e test. |
| 44 | `dotnet build DCB.Gateway` | Compila la libreria (0 avvisi, 0 errori). |
| 45 | `dotnet test` | Esegue i test della solution: 21 superati. |
| 46 | *(PowerShell)* lettura di `%USERPROFILE%\Downloads\deconz-full-state.json` con `System.Text.Json.Nodes`, rimozione di `config.whitelist`, scrittura in `DCB.Gateway.Tests\TestData\Deconz\full-state.json` | Copia nel repository dello stato completo reale salvato da Postman, senza le API key della whitelist (PA-01). |
| – | *(file creati/modificati con l'editor)* `DCB.Gateway\Dto\**`, `DCB.Gateway\Clients\FullState\**`, `ServiceCollectionExtensions.cs`, `DCB.Gateway.Tests\**`, `DCB.Gateway.Tests.csproj` (copia dei file JSON di prova) | Incremento 2 di GTW: `DCB-GTW-FUL`, DTO di luci, sensori e gruppi, test sui dati reali. |
| 47 | `dotnet test` | Esegue i test: 27 superati. |
| 48 | `git add -A` | Prepara per il commit documenti di analisi (ANA-06, ANA-07) e incrementi 1-2 di GTW. |
| 49 | `git commit -m "GTW incrementi 1-2 (base, CFG, AUT, FUL) e analisi ANA-06/ANA-07"` | Commit locale. |
