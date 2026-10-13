# Registro della migrazione a Performance2027

Stime ed effettivi per fase, per ricalibrare le stime successive. Piano: `docs/PianoMigrazioneCSharp.md`.

- **Mio lavoro:** tempo attivo delle sessioni (scrittura, compilazione, test, correzioni).
- **Tuo tempo:** prove sull'interfaccia, decisioni, confronti.
- **Calendario:** dall'inizio alla chiusura della fase, pause comprese.

## Riepilogo

| Fase | Modello / effort | Stima mio lavoro | Stima tuo tempo | Effettivo mio lavoro | Effettivo tuo tempo | Calendario | Stato |
|---|---|---|---|---|---|---|---|
| 0 Preparazione (VB) | Opus 5.5 / high | 3-5 h | 1-2 h | circa 1,5 h (in corso) | - | dal 2026-10-13 | in corso |
| 1 Soluzione e motore | Opus high + Sonnet medium | da stimare a fine fase 0 | | | | | |
| 2 Dati e profili | Opus 5.5 / high | | | | | | |
| 3 Scheletro dell'app | Sonnet 5.5 / medium | | | | | | |
| 4 Grafici ScottPlot | Opus 5.5 / high | | | | | | |
| 5-12 Tab | Sonnet 5.5 / medium (high per i file complessi) | | | | | | |
| 13 Distribuzione | Sonnet 5.5 / medium | | | | | | |

## Fase 0: preparazione nella versione VB

**Stima (2026-10-13):** mio lavoro 3-5 h in 1-2 sessioni; tuo tempo 1-2 h. Incertezza principale: quanti canali
matematici dipendono da altri dati (polari, target) e quanto pesano gli export.

**Fatto:**
- `docs/PianoMigrazioneCSharp.md`: piano con il nome Performance2027 e le regole di lavoro.
- Strumento di esportazione dei risultati di riferimento nella versione VB (`EsportaRiferimenti.vb`, menu File
  Utilities → "Export reference data"): tutti i canali grezzi e matematici, gli istanti, i periodi e i risultati
  del Feed Forward e della sensitività, in un formato binario compresso più JSON leggibile da C#.
- `AKCurrentLib.dll`: è una libreria .NET Standard 2.0, si carica e funziona su .NET 10 (prova con un'app console
  .NET 10.0.12). Calcola correnti di marea da un file di maree; oggi **nessuna parte dell'app la usa**. Rischio chiuso:
  nella nuova versione la si porta solo se serve.
- `docs/InventarioFunzionalita.md`: 490 voci da spuntare (tab, menu, pulsanti), per controllo e per fase.

**Da fare (tuo):**
- Scegliere 3-4 dataset di riferimento (uno a 10 Hz, uno a 1 Hz, uno con buchi o più file, uno con molte manovre) e
  per ognuno: caricarlo con il profilo giusto (Alegre52 compreso), lanciare Auto Periods se servono i periodi, poi
  File Utilities → "Export reference data" (No = tutto il dataset).
- Indicare le funzioni che usi poco o mai nell'inventario.

**Da fare (mio):** controllare gli export (canali in errore o vuoti), copiare un profilo di prova, tag `vb-final-baseline`,
stima della Fase 1.

**Scarto e motivi:** da compilare a fine fase.
