# Piano: da Performance2021 (VB.NET) a Performance2027 (C#, .NET 10), senza SciChart

## Contesto

Performance2021 è un'app WPF in VB.NET su .NET Framework 4.8: circa 82.000 righe in 86 file `.vb` e 46 pagine XAML.
L'obiettivo è passare a **C# su .NET 10 (LTS, supportato fino a novembre 2028)** in un **nuovo repository**, restando su
**WPF**, e ripristinare a tappe tutte le funzionalità.

Il problema principale è **SciChart 6**: è usato in circa 20 controlli attivi e una nuova licenza non vale il costo. Va
sostituito con **ScottPlot 5**, gratuito (licenza MIT) e già presente nel progetto: GestionePdf, XY Plots, Highlight.
La versione VB resta intatta e utilizzabile finché la nuova non la sostituisce.

Decisioni prese con l'utente:
- nuovo repository;
- WPF su .NET 10;
- la vista 3D (Ab3d) va in fondo al piano e può essere abbandonata.

## Principi

1. **I dati non devono cambiare.** Prima di toccare il codice si producono dalla versione VB i "risultati di
   riferimento" (canali matematici, tabelle, Feed Forward) su alcuni dataset reali. Ogni parte portata deve dare gli
   stessi numeri, entro una tolleranza.
2. **Motore separato dall'interfaccia.** I calcoli vanno in una libreria senza WPF, testata in automatico. L'interfaccia
   la usa soltanto.
3. **Grafici dietro un'interfaccia propria.** I controlli dell'app non chiamano ScottPlot direttamente, ma un livello
   intermedio. Se un giorno si cambia libreria grafica, si tocca solo quel livello.
4. **Compatibilità dei profili esistenti.** I profili JSON usano `TypeNameHandling.Auto`
   (`cls2021.vb:3086`, `mdlCommon.vb:284/304`) e salvano tipi come `SPwpf.clsXxx, Performance2021`. Serve un
   `SerializationBinder` che traduca i vecchi nomi nei nuovi tipi C#, così la nuova app legge i profili della vecchia.
5. **Una funzionalità alla volta, sempre funzionante.** Ogni fase finisce con l'app compilabile, i test verdi e la tua
   prova sull'interfaccia.

## Struttura della nuova soluzione

Repository e soluzione **`Performance2027`**:

| Progetto | Tipo | Contenuto (origine VB) |
|---|---|---|
| `Performance2027.Core` | libreria .NET 10 | Canali e data provider (`Classi2020.vb`), calcoli comuni, vento e correnti (`mdlCommon.vb`), profili e impostazioni (`cls2021.vb`), polari (`ClassiGestionePolari.vb`), straight line, Pavarot, filtri (`ClassiGestioneFiltri.vb`), Feed Forward (`ClassiFeedForward.vb`), inerzia (`ClassiInerzia.vb`), leeway model, sea state |
| `Performance2027.IO` | libreria | Parquet (`GestioneFileSources.vb`, con Parquet.Net 5), import FaRo, Expedition e Deckman, Excel, KML/GPX, CSV |
| `Performance2027.Reports` | libreria | PDF con PdfSharp 6 (`GestionePdf.vb`, `GestionePdfFeedForward.vb`, report straight line), export HTML |
| `Performance2027.Charts` | libreria WPF | Livello grafico su ScottPlot 5: grafico temporale multicanale, XY, sincronizzazione, periodi, annotazioni, export in immagine per i PDF |
| `Performance2027.App` | WPF .NET 10 | Finestre, tab, UserControl, viewmodel |
| `Performance2027.Tests` | xUnit | Test dei motori e confronto con i risultati di riferimento |

Progetti in formato SDK-style, quindi i file vengono inclusi da soli. Pacchetti gestiti a livello di soluzione e
`CLAUDE.md` del nuovo repo con le regole: UI in inglese, commenti in italiano, versione aggiornata a ogni modifica,
build dopo ogni modifica.

## Librerie: cosa si tiene, cosa si sostituisce

| Oggi | Domani | Nota |
|---|---|---|
| SciChart 6 | **ScottPlot 5** (MIT) | Il lavoro più grande, vedi la Fase 4 |
| Parquet.Net 3.9 | Parquet.Net 5 | Più veloce; va riscritta la lettura e scrittura |
| PdfSharp 1.51 beta | PdfSharp 6 (MIT) | Va gestita la risoluzione dei font |
| Mapsui 2.0 | Mapsui 5 | API cambiata; cache delle tile da riscrivere |
| Accord.Statistics, alglib | MathNet.Numerics (già presente) | Accord è abbandonata dal 2020 |
| EPPlus 5.8 | **ClosedXML** (MIT) | Da EPPlus 5 serve una licenza per uso commerciale: da verificare il tuo caso |
| Interop Office (`Libs\`) | ClosedXML / OpenXML | Niente più dipendenza da Office installato |
| Extended.Wpf.Toolkit 4.x | Controlli propri o toolkit MIT | Dalla 4.x la licenza è solo per uso non commerciale |
| System.Data.SQLite | Microsoft.Data.Sqlite | |
| PropertyChanged.Fody | CommunityToolkit.Mvvm | Generatori di codice standard di C# |
| Unity (DI) | Microsoft.Extensions.DependencyInjection | |
| FontAwesome.WPF 4 | Icone in un pacchetto MIT moderno | |
| Newtonsoft.Json | Newtonsoft.Json | Si tiene, per la compatibilità con i profili |
| Ab3d.PowerToys | Ultima fase, oppure si abbandona | A pagamento: da decidere più avanti |
| AKCurrentLib.dll | Da provare su .NET 10; se non va, chiedere il sorgente o riscrivere | Rischio da verificare nella Fase 0 |
| Eziriz Reactor | Versione per .NET moderno, se la protezione serve ancora | |

## Fasi

### Fase 0: preparazione nella versione VB (2-3 giorni)
- Tag `vb-final-baseline` sull'ultima versione VB.
- **Risultati di riferimento:**
  - un piccolo strumento nella soluzione VB carica 3-4 dataset reali (10 Hz, 1 Hz, con e senza buchi, più file);
  - esporta in CSV i canali grezzi e tutti i canali matematici (VMG, Vmg%, Inertial, RecLeeway, correnti, leeway,
    qualità, ecc.);
  - esporta le tabelle straight line, Pavarot e Feed Forward e i PDF.

  Sono i criteri di accettazione di tutte le fasi successive.
- Copia di uno o due profili reali (Alegre52 compreso) come dati di test.
- Inventario delle funzionalità: un elenco di controllo tab per tab, preso dal VB (menu, pulsanti, report).
- Prova rapida: `AKCurrentLib.dll` caricata da un'app di prova .NET 10.

### Fase 1: soluzione nuova e conversione del motore (1-2 settimane)
- Creare la soluzione con i 6 progetti.
- Convertire i file non UI con il **convertitore VB → C# di ICSharpCode** (estensione di Visual Studio), poi sistemarli
  a mano. Nodi noti:
  - `Option Strict Off`: conversioni implicite e chiamate in late binding diventano conversioni esplicite;
  - i nomi VB non distinguono maiuscole e minuscole, il C# sì: alcune variabili vanno rinominate;
  - funzioni `My.*` ed eventi `Handles`.
- Le variabili globali di `mdlCommon` (`DataProvider2020`, `AppConfig`, `PeriodsManager`, ecc.) restano inizialmente
  in un contenitore statico unico. Si riorganizzano più avanti, senza bloccare il porting.
- Test: i motori già isolati (Feed Forward, sensitività, inerzia, filtri) contro i risultati di riferimento.

### Fase 2: dati e profili (1 settimana)
- Lettura dei Parquet con Parquet.Net 5: stessi array dei riferimenti VB su tutti i dataset.
- Mantenere la regola "lettura sul thread UI". La si rilassa solo se la nuova libreria la rende sicura.
- Profili: caricamento e salvataggio con il `SerializationBinder` dei vecchi nomi di tipo. Test: si carica il profilo
  Alegre52 della versione VB, lo si salva e lo si ricarica senza perdite.
- Tutti i canali matematici calcolati e confrontati con i riferimenti (tolleranza per esempio 1e-9).

### Fase 3: scheletro dell'app (1 settimana)
- `MainWindow` con la barra dei tab (una riga, frecce, menu), `LoadingForm`, `SelectProfileForm`, File Load, menu File
  Utilities (Boat Inertia, metodo Inertial), Quality Settings, Leeway Model, Sea State Norm, finestra dei filtri.
- Nessun grafico ancora. L'app carica dataset e profili, e si possono ispezionare i canali.

### Fase 4: livello grafico su ScottPlot 5 (2 settimane, la parte critica)
- **Prima un prototipo di prestazioni:** 8 ore a 10 Hz, circa 288.000 punti per canale, 6-10 canali. Si verificano
  zoom, pan e cursore fluidi. Riferimenti ScottPlot: grafici `SignalXY`/`Scatter`, `AxisSpan` per i periodi,
  `Crosshair`, plot multipli con asse X condiviso. Se il prototipo non regge, si decide subito come ridurre i punti
  prima di andare avanti.
- **Componenti da costruire:**
  - grafico temporale multicanale con assi sincronizzati: oggi `clsChartsSyncManager.vb`, `DataPlotSync`,
    `UserControlTimePlotView`;
  - grafico XY con colore per canale, nuvole, medie, curve, punti e legenda: oggi `UserControlSciChartXYplot` e il
    grafico del Feed Forward;
  - evidenziazione dei periodi e degli eventi: oggi `clsChartEventiViewModel.vb`;
  - selezione con il mouse (rubber band, oggi `SciChart\RubberBandModifier.vb`) e annotazioni di testo;
  - nomi degli assi grandi sullo sfondo;
  - export in immagine per i PDF, che con ScottPlot è più semplice di oggi.

### Fase 5 e seguenti: ripristino dei tab, in ordine di valore
Ogni tab chiude con il controllo sull'inventario della Fase 0, il confronto dei numeri e dei PDF con i riferimenti e la
tua prova sull'interfaccia.

| # | Funzionalità | Stima |
|---|---|---|
| 5 | Data Plots (grafico temporale), Summary / Highlight | 1-2 settimane |
| 6 | XY Plots, report XY e filtri nominati | 1-2 settimane |
| 7 | Straight line: Upwind, Downwind, Reaching, StraightLine, report PDF/HTML | 2 settimane |
| 8 | Feed Forward completo, con PDF e sensitività | 1 settimana |
| 9 | Pavarot (virate e strambate), stima dell'inerzia dalle manovre | 2 settimane |
| 10 | Polari, Benchmarks, 360 checks, import ed export Excel/Expedition/FaRo | 1-2 settimane |
| 11 | Starts / Expedition, Crossover, utilità delle correnti, Sail usage / Sail shape | 1-2 settimane |
| 12 | Mappe (Mapsui 5, tile offline) | 1 settimana |
| 13 | Distribuzione: installer (ClickOnce per .NET 10 o MSIX autonomo), licenze, passaggio definitivo | 3-5 giorni |
| 14 | 3D (Ab3d): solo se ancora utile e conveniente | da decidere |

**Totale indicativo:** 4-6 mesi di lavoro a tempo parziale. I tempi dipendono soprattutto dalla Fase 4 e da Pavarot,
che è il file più legato a SciChart.

## Verifica (vale per ogni fase)
- `dotnet build` senza errori e `dotnet test` verde. I test confrontano canali e tabelle con i risultati di riferimento
  VB della Fase 0.
- Per i PDF: si generano con lo stesso dataset e si confrontano a vista con quelli VB, rendendoli in immagine come
  abbiamo già fatto per il Feed Forward.
- Prova manuale dell'utente sull'interfaccia, con l'elenco di controllo del tab.
- Il repository VB resta la versione in produzione fino al passaggio definitivo della Fase 13.

## Quale LLM e con quale impegno

| Lavoro | Modello | Livello di effort |
|---|---|---|
| Architettura, livello grafico ScottPlot, binder dei profili, motore dati (Fasi 0, 1 parte, 2, 4) | **Claude Opus 5.5** | **high** |
| Conversione in blocco dei file, porting dei tab e dei controlli, scrittura dei test (Fasi 1, 3, 5-12) | **Claude Sonnet 5.5** | **medium**, alzato a high per i file più complessi (Pavarot, XY Plots, Straight line) |
| Bug difficili sulle differenze numeriche tra VB e C# o sulle prestazioni dei grafici | Claude Opus 5.5 | **xhigh** solo per il problema specifico, poi si torna al livello normale |
| Revisione prima di ogni commit importante | Claude Opus 5.5 | high (comando `/code-review`) |

**Consigli pratici di lavoro:**
- Una sessione per fase o per tab, con il `CLAUDE.md` del nuovo repo sempre aggiornato.
- I risultati di riferimento sono il criterio di "fatto": l'IA non dichiara finita una parte finché i test non sono
  verdi.
- Commit piccoli e frequenti, uno per file o per tab portato.
- Non usare modelli piccoli (Haiku) per la conversione: con `Option Strict Off` gli errori sottili di tipo costano più
  del risparmio.

## Rischi principali
1. **Prestazioni di ScottPlot sui grandi dataset.** Mitigazione: il prototipo della Fase 4 viene prima di tutto il
   resto dell'interfaccia.
2. **Semantica di VB con `Option Strict Off`** (conversioni, arrotondamenti, `\` intero, confronti di stringhe).
   Mitigazione: i test sui risultati di riferimento.
3. **Compatibilità dei profili JSON.** Mitigazione: il binder dei tipi e un test di andata e ritorno sul profilo reale.
4. **`AKCurrentLib.dll` e licenze** (EPPlus, Extended Toolkit, Eziriz). Si verificano nella Fase 0 e si sostituiscono
   per tempo.
5. **Durata del progetto.** Mitigazione: ogni fase lascia un'app funzionante e la versione VB resta in uso fino alla
   fine.

## Regole di lavoro concordate

- **Modello ed effort per fase:** come nella tabella "Quale LLM e con quale impegno". La sessione principale coordina;
  i blocchi grandi e meccanici (conversione dei file, porting di un tab) si affidano a sotto-agenti con il modello e
  l'effort indicati. In alternativa si cambia con `/model` e `/effort` all'inizio della fase.
- **Commit frequenti** (uno per passo completato) e push su GitHub: spegnendo il PC si perde al massimo il passo in
  corso; si riprende con `claude --continue` o `claude --resume`.
- **Stima prima di ogni fase** (mio lavoro, tuo tempo, calendario) e **registro** di stime ed effettivi in
  `docs/RegistroMigrazione.md`, per ricalibrare le stime successive.
- **Inventario delle funzionalità** in `docs/InventarioFunzionalita.md`: elenco di controllo, da spuntare tab per tab.
