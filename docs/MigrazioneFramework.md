# Passaggio a un framework .NET più moderno

Valutazione del passaggio di Performance2021 da .NET Framework 4.8 a .NET moderno (ottobre 2026).

**In breve:** i vantaggi reali per questa applicazione sono velocità, gestione della memoria, un progetto più
semplice da mantenere e librerie aggiornate. Il costo è un porting non banale per alcune dipendenze, da fare a tappe.

**Destinazione consigliata: .NET 10.** È la versione a lungo supporto (LTS) attuale, supportata fino a novembre 2028.
Il .NET 8 esce dal supporto a novembre 2026.

## Benefici che ci si può aspettare

| Area | Cosa cambia per Performance2021 |
|---|---|
| **Velocità di calcolo** | I canali matematici, i filtri e il Feed Forward sono cicli su grandi array di numeri. Il compilatore del .NET moderno li ottimizza molto meglio: in genere dal 20% al doppio di velocità su questo tipo di codice, senza toccarlo. I cicli più pesanti si possono poi accelerare con le istruzioni vettoriali del processore |
| **Lettura dei Parquet** | Oggi c'è Parquet.Net 3.9.1, una versione vecchia. Le versioni 4 e 5 sono molto più veloci, consumano meno memoria e rendono al meglio sul .NET moderno. È probabilmente il guadagno più visibile all'apertura dei dataset |
| **Memoria** | Gestione migliore dei grandi array: meno frammentazione e meno pause quando si caricano molte sessioni |
| **Avvio** | Con la compilazione anticipata l'app si apre più in fretta |
| **Progetto moderno ("SDK-style")** | I file `.vb` e `.xaml` vengono inclusi da soli: sparisce il rischio di dimenticare le voci `<Compile>`/`<Page>` nel file di progetto. Il file di progetto diventa più piccolo e leggibile, si compila da riga di comando, ed è facile aggiungere un progetto di test automatici per i motori di calcolo (oggi i test si compilano a parte, a mano) |
| **Distribuzione** | L'app può portarsi dietro il proprio runtime e non dipendere da cosa è installato sul PC. Si può distribuire anche come file unico |
| **Durata nel tempo** | Il .NET Framework 4.8 resta supportato in Windows ma non evolve più. Molte librerie nuove, e le nuove versioni di quelle in uso, escono solo per .NET moderno |

## Costi e rischi, dalle dipendenze del progetto

**Probabilmente semplici**

Newtonsoft, MathNet, CsvHelper, PropertyChanged.Fody, EntityFramework 6.4, ScottPlot 5, SkiaSharp, Ab3d.PowerToys,
Unity, System.Reactive.

**Lavoro vero**

- **Parquet.Net 3.9 → 4/5:** cambia l'interfaccia di programmazione, va riscritta la parte di lettura e scrittura in `GestioneFileSources.vb`.
- **Mapsui 2.0 → 4/5:** cambiamenti importanti nella mappa.
- **PdfSharp 1.51 beta → 6:** cambia la gestione dei font, va adattato il codice dei PDF.
- **SciChart 6.0.1:** va verificato il supporto al .NET moderno. Passare alla 7 o alla 8 richiede una licenza aggiornata.

**Da verificare**

- **Accord.Statistics 3.8:** abbandonata dal 2020. Meglio sostituirla con MathNet, che è già nel progetto.
- **System.Data.SQLite 1.0.112:** va aggiornata.
- **Eziriz Reactor Licensing:** va verificata la versione compatibile.
- **`AKCurrentLib.dll`:** è compilata per .NET Framework e non è detto che si possa ricompilare. Spesso funziona lo stesso, ma va provata.
- **Interop Office:** funziona anche sul .NET moderno, va solo controllato.

**Codice**

- In `Classi2020.vb` c'è un `Imports System.Runtime.Remoting.Channels` che sul .NET moderno non esiste. Sembra inutilizzato, quindi basta toglierlo.
- `My.Computer.Keyboard` e `My.Computer.FileSystem` (in LoadingForm, MainWindow e cls2021) sono in gran parte supportati, ma vanno verificati.

**Linguaggio**

Il VB.NET con WPF è supportato su .NET 10, ma il linguaggio non riceve più novità. Non è un problema per farlo
funzionare; lo è solo se un giorno si volessero le novità del C#.

**Rischio principale**

Non ci sono test automatici dell'intera app. Il porting va verificato confrontando, sugli stessi dataset, i valori dei
canali prima e dopo.

**Impegno indicativo**

Uno o due giorni per convertire il progetto, da una a tre settimane per il porting vero (soprattutto Parquet, Mapsui,
PdfSharp e SciChart), più il collaudo.

## Percorso consigliato, a tappe e senza bloccare il lavoro

1. **Progetto SDK-style restando su .NET 4.8.** Rischio basso e beneficio immediato (file inclusi da soli, file di
   progetto pulito); poi si aggiunge un progetto di test per i motori di calcolo (Feed Forward, sensitività, inerzia).
2. **Inventario della compatibilità**, dipendenza per dipendenza, senza cambiare nulla.
3. **Compilazione doppia, 4.8 e .NET 10.** Si risolvono i problemi uno alla volta e si confrontano i valori dei canali
   tra le due versioni sugli stessi dati.
4. **Passaggio definitivo** e aggiornamento di Parquet.Net, la libreria che dà il guadagno più grande.

Per le regole del progetto (`CLAUDE.md`) il cambio di framework e l'aggiornamento dei pacchetti NuGet si fanno solo con
un via libera esplicito.
