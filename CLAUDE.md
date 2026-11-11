# MyHome – impostazioni generali di lavoro

MyHome è un sistema domotico: C# / .NET 10, Visual Studio Community, mini PC Windows 11 con Docker Desktop (WSL2),
dispositivi Zigbee gestiti da deCONZ + ConBee II in container. Dettagli in `Docs\base` e `Docs\analisi`
(riferimento attuale: `Docs\analisi\ANA-06-SintesiUnificata.md`).

## Avvio di ogni sessione
- Salutami con una massima del giorno divertente.
- Poi chiedimi in quale modalità voglio lavorare: **analisi**, **coding** o **altro**.

## Lingua
- Usa l'italiano dove possibile.

## Modalità di lavoro
1. **Analisi**: raccogliamo idee e produciamo un documento di sintesi.
   - Si lavora in plan mode; agisci solo quando te lo chiedo.
   - **Sono io a guidare il dialogo**: rispondi a quello che chiedo, senza propormi domande, argomenti o approvazioni non richieste.
2. **Coding**: dalle analisi produciamo i componenti software. Sei più operativo (accept edits on).
3. **Altro**: da valutare di volta in volta.

## Registro dei comandi (modalità coding)
- Ogni comando eseguito per il progetto (solution, progetti, pacchetti, strumenti…) va aggiunto in ordine
  in `Docs\coding\RegistroComandi.md`, con una breve descrizione.
- Non vanno registrati i comandi di sola consultazione (versioni, stato, elenchi) né quelli di versionamento
  (`git add`, `commit`, `push`, `mv`…): la loro storia è già in git.

## Punti aperti (modalità analisi e coding)
- Due documenti vivi, da aggiornare ogni volta che un punto si apre, cambia o si chiude:
  - `Docs\analisi\PuntiApertiAnalisi.md`: punti che richiedono una decisione o un'analisi;
  - `Docs\coding\PuntiApertiCoding.md`: punti operativi della fase di coding.
- Gli ID `PA-nn` sono unici per i due documenti (un nuovo punto prende il numero successivo al più alto dei due)
  e non si riutilizzano. Ogni punto indica progetto/sottomodulo e cosa blocca.
- I punti chiusi passano nella sezione "Punti chiusi" con data ed esito.

## Linee guida per il codice (modalità coding)
- **Leggibilità prima di tutto**: il codice deve essere facile da leggere e da valutare. Nomi espliciti,
  metodi brevi con una sola responsabilità, niente soluzioni "furbe" quando esiste un'alternativa chiara.
- **Commenti nei punti strategici**: spiegano il *perché* (scelte progettuali, vincoli, interazioni con
  deCONZ e i dispositivi, casi limite, workaround), non ripetono il *cosa*. Commenti XML `///` su classi
  e membri pubblici.
- **Configurazione centralizzata**: nessun valore cablato nel codice (URL, porte, timeout, intervalli,
  soglie, percorsi, chiavi…). Ogni progetto tiene i parametri in `appsettings.json`, letti tramite classi
  Options tipizzate (pattern `IOptions<T>`), una per area funzionale e raccolte in una cartella `Configuration`.
- **Parametri documentati**: ogni parametro va commentato sia nella classe Options (`///`) sia in
  `appsettings.json` (sono ammessi i commenti `//`). Il commento indica significato, unità di misura,
  valore predefinito e valori ammessi.

## Documenti
- Tutti i documenti prodotti vanno nella cartella `Docs`, suddivisa per tipologia: `base`, `analisi`, `coding`.
- Nome del documento: `ACRONIMO-progressivo-BreveDescrizione` (es. `ANA-03-SintesiUnificata.md`),
  con acronimo `BASE`, `ANA` o `COD` e progressivo a due cifre.
- Eccezioni: i documenti permanenti hanno un nome fisso, non seguono la numerazione e si aggiornano sovrascrivendoli:
  `Docs\analisi\Acronimi.md` (registro degli acronimi), `Docs\analisi\RegistroDocumenti.md` (registro dei documenti),
  `Docs\analisi\PuntiApertiAnalisi.md` e `Docs\coding\PuntiApertiCoding.md` (punti aperti),
  `Docs\coding\RegistroComandi.md` (registro dei comandi).
- **Salvataggio delle modifiche in analisi**:
  - modifiche **non importanti** → sovrascrivi il documento di partenza;
  - modifiche **importanti** (es. cambi di architettura, componenti aggiunti o eliminati) → non salvarle subito: dimmi
    che le ritieni importanti. Solo se confermo, assegna al documento il progressivo successivo della categoria,
    sposta il precedente in `storico` e aggiorna i rimandi;
  - in `RegistroDocumenti.md` va registrato ogni nuovo documento, e per ogni nuova versione il motivo per cui
    non si è sovrascritta la precedente.
- Ogni sottocartella di `Docs` ha una cartella `storico`: vi si spostano i documenti le cui informazioni sono state
  riportate in documenti più recenti della stessa categoria. Il progressivo resta unico per categoria (i numeri
  dei documenti in `storico` non si riutilizzano); i rimandi ai documenti spostati vanno aggiornati.

## Acronimi di progetti e sottomoduli
- Le tabelle degli acronimi (in `Docs\analisi\Acronimi.md`) riportano gli acronimi dei progetti e, per ciascun progetto, dei sottomoduli.
- Ogni sottomodulo corrisponde a un componente da sviluppare; l'analisi serve anche a individuarli.
- Quando proponi qualcosa, indica sempre l'**acronimo del progetto** e del **sottomodulo**, specificando se è **già previsto** o **nuovo**.
- Le tabelle vanno mantenute: ogni nuovo componente o sottomodulo riceve un acronimo e una riga.
- Formato degli acronimi: `COMPONENTE-SOTTOMODULO` (es. `DCB-STO`); i sottomoduli annidati aggiungono un livello
  (es. `DCB-GTW-LIG`, sottomodulo di `DCB-GTW`).

## Nomi della solution
- **MyHome** è il sistema e dà il nome alla solution (`MyHome.sln`).
- I progetti portano l'acronimo del componente a cui appartengono (`DCB.Server`, `DCB.Infrastructure`, `TCL.Client`, `HSPA.*`);
  il codice condiviso tra componenti porta il nome del sistema (`MyHome.Shared`).
