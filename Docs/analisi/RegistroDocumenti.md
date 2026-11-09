# Registro dei documenti di MyHome

**Aggiornato al:** 09/11/2026
**Stato:** registro permanente (come `Acronimi.md`, non segue la numerazione)

---

Il registro descrive ogni documento creato e, quando un documento ne sostituisce un altro, spiega perché si è scelto
di creare una nuova versione invece di sovrascrivere la precedente.

**Regola di salvataggio (dal 09/11/2026)**
- Modifiche **non importanti** → si sovrascrive il documento di partenza.
- Modifiche **importanti** → Claude lo segnala. Se Andrea conferma, il documento riceve il progressivo successivo
  della categoria, il precedente va in `storico` e qui si annota il motivo.

## 1. Analisi (`Docs\analisi`)

| Documento | Data | Stato | Contenuto | Sostituisce | Perché non si è sovrascritto il precedente |
| --- | --- | --- | --- | --- | --- |
| `storico\ANA-01-AnalisiGemini.md` | prima del 07/11/2026 | storico | Prima analisi fatta con Gemini (ex `AnalisiMyHomeConGemini.md`): progetto ancora chiamato DCC, .NET 8/9, prima tabella degli acronimi | – | – (documento iniziale) |
| `storico\ANA-02-SintesiArchitetturaDCC.md` | prima del 07/11/2026 | storico | Sintesi architetturale DCC fatta con Gemini: solution, TCL remote multiple, deCONZ in container | – | – (documento iniziale, parallelo ad ANA-01) |
| `ANA-03-SintesiUnificata.md` | 07/11/2026 | **riferimento attuale** | Sintesi unificata di MyHome: componenti, solution, DCB, storage, eventi, TCL, test, questioni aperte | ANA-01, ANA-02 | Le due fonti erano distinte e in parte in conflitto (nome DCC, .NET 8/9, struttura diversa): serviva un unico documento nuovo che le fondesse e le allineasse a MyHome e .NET 10 |
| `ANA-04-FileJsonDCB.md` | 07/11/2026 | attivo | Approfondimento sui file JSON `CFG`, `DBZ`, `MDZ` di `DCB-STO` | – (approfondisce ANA-03 §5) | Non sostituisce nulla: è un argomento nuovo |
| `ANA-05-CanaleClientDCB.md` | 09/11/2026 | attivo | Canale verso i client di `DCB.Server`: REST (`DCB-API`) + SSE (`DCB-EVS`) al posto di SignalR | – (approfondisce ANA-03 §6) | Non sostituisce nulla: è una decisione nuova |
| `Acronimi.md` | 07/11/2026 | registro permanente | Acronimi di componenti e sottomoduli | – (scorporato da ANA-03 §1) | – |
| `RegistroDocumenti.md` | 09/11/2026 | registro permanente | Questo registro | – | – |
| `PuntiApertiAnalisi.md` | 09/11/2026 | documento permanente | Punti di analisi aperti e chiusi, con ID `PA-nn` condivisi con `PuntiApertiCoding.md` | – | – |

**Nota sul 09/11/2026:** ANA-03 è stato aggiornato sovrascrivendolo, prima che esistesse la regola di salvataggio:
passaggio da SignalR a REST + SSE ed eliminazione di `DCB-CLI` a favore della raccolta Postman `DCB-PMN`.
Con la regola attuale queste modifiche sarebbero state classificate come **importanti**. Il testo precedente non è
recuperabile, perché la cartella non è ancora un repository git.

## 2. Base (`Docs\base`)

| Documento | Contenuto |
| --- | --- |
| `BASE-01-ImpostazioniDiBase.txt` | Impostazioni di base e modalità di lavoro con l'AI (riportate in `CLAUDE.md`) |
| `BASE-02-IdeaDiPartenza.txt` | Idea di partenza del progetto MyHome |

## 3. Coding (`Docs\coding`)

| Documento | Data | Stato | Contenuto |
| --- | --- | --- | --- |
| `RegistroComandi.md` | 09/11/2026 | documento permanente | Sequenza dei comandi eseguiti per costruire il progetto, con descrizione (era `COD-01-RegistroComandi.md`; il progressivo COD-01 non si riutilizza) |
| `PuntiApertiCoding.md` | 09/11/2026 | documento permanente | Punti operativi di coding aperti e chiusi, con ID `PA-nn` condivisi con `PuntiApertiAnalisi.md` |
