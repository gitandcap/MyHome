# COD-01 – Registro dei comandi del progetto

**Aggiornato al:** 09/11/2026
**Stato:** documento vivo, aggiornato ogni volta che si esegue un comando per il progetto

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
| 25 | `git commit -m "Registro dei comandi (COD-01)"` | Commit locale del registro dei comandi (non ancora inviato a GitHub). |
