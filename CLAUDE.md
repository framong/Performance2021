# CLAUDE.md

Indicazioni per Claude Code su questo repository.

## Progetto

- **Performance2021** (root namespace `SPwpf`): applicazione WPF in VB.NET, .NET Framework 4.8.
- Analisi delle prestazioni in barca a vela: polari, straight line, Pavarot, manovre, correnti, dati Parquet.
- Soluzione: `Performance2021.sln` → un solo progetto, `Performance2021\Performance2021.vbproj`.
- Progetto in formato **vecchio stile (non SDK)**: i file `.vb` e `.xaml` **NON** vengono inclusi automaticamente,
  fa fede solo l'elenco `<Compile>` / `<Page>` del `.vbproj`.
- Librerie principali (NuGet): SciChart 6.0.1 (grafici), Parquet.Net 3.9.1, Mapsui 2.0 (mappe),
  Ab3d.PowerToys (3D), EPPlus / ExcelDataReader (Excel), PdfSharp, MathNet.Numerics, Accord.Statistics,
  alglib, PropertyChanged.Fody (INotifyPropertyChanged automatico), System.Data.SQLite.
- Riferimenti locali: `AKCurrentLib.dll` (calcolo correnti), `Libs\` (interop Office), `x64\` e `x86\` (SQLite.Interop).
- Pubblicazione tramite ClickOnce (impostazioni nel `.vbproj`).

## Build

Usa sempre questo comando (mostra solo gli errori), dalla root del repository:

```
msbuild Performance2021.sln /restore /p:Configuration=Debug /v:quiet /nologo /clp:ErrorsOnly
```

- Da eseguire nella **PowerShell per sviluppatori di Visual Studio** (serve `msbuild` nel PATH).
- Il progetto ha centinaia di warning storici: ignorali, conta solo che non ci siano errori.

## Architettura

### Avvio e finestra principale
- `Application.xaml(.vb)`: mostra `LoadingForm` all'avvio e imposta le licenze SciChart / Ab3d.
- `LoadingForm`, `SelectProfileForm`, `RenameProfileForm`: selezione e gestione dei profili (barca/configurazione).
- `MainWindow.xaml(.vb)`: finestra principale a schede (TabItem), ciascuna ospita uno UserControl:
  File Load, Data Plots, Summary (Time Range Summary / Channels Highlight), Starts, Upwind, Downwind,
  Reaching, StraightLine, TacksGybes (Pavarot), Crossover, XY Plots, Polars, Benchmarks.
- `QualitySettingsForm`, `SailList`: finestre di impostazioni qualità canali ed elenco vele.

### File di classi principali (codice attivo)
| File | Contenuto |
|---|---|
| `mdlCommon.vb` | Modulo `mdlCommon` con utilità comuni, calcoli geografici/vento/correnti (`clsGeoCalculations`, `clsWindCalc`, `clsCurrentCalcs`), tabelle 1D/2D, KML/GPX, tipi di base (`clsTimeRange`, `clsXYpoint`, ...) |
| `cls2021.vb` | Impostazioni e profili (`clsSettings2021`, `clsProfile2021`, `clsQualitySettings`), gestione periodi (`clsPeriodsManager2021`, `clsPeriod2021`), dettagli analisi (straight line, Pavarot, bear away, round up, accelerazioni), sorgenti Parquet (`ParquetDataSource`, `ParquetChannel`), configurazioni barca, partenze Expedition/DFW/FastSkipper |
| `Classi2020.vb` | Nucleo dati storico: `clsDataProvider2020` (caricamento file e canali), `clsChannels2020`/`clsChannel2020`, `clsFileParquet2020`, file binari FaRo, legs/segmenti, statistiche, target e polari (`clsTgtManager`, `clsTgt`, `clsPuntoPolare`), tabelle vele |
| `ClassiGestionePolari.vb` | Polari/VPP da Excel (`clsXls`, `clsExcelVpp`, `clsTabellinaVpp`), export verso Expedition e FaRo |
| `ClassiGestioneStraightLine.vb` | Analisi straight line: `clsStraightLine`, tabelle e ViewModel dei grafici standard |
| `ClassiGestionePavarot.vb` | Analisi manovre (virate/strambate) "Pavarot": ViewModel dei grafici, calcoli geometrici, eventi, percorso (`clsCourseLeg`, `clsCourseMark`) |
| `ClassiGestionePeriodi.vb` | `clsPeriodManagerViewModel`, tabelle periodi/canali, test e commenti |
| `GestioneFileSources.vb` | Lettura/scrittura Parquet (`clsParquetFile`, `clsParquetUtilities`, `clsCompaqParquet`), conversione log FaRo → Parquet, export CSV, file GZip |
| `ManeuverCurrentAnalyzer.vb`, `CurrentCalibration_dHdg.vb` | Stima della corrente da manovre e calibrazione dHdg |
| `clsSailUsage.vb`, `clsSailShape2022.vb` | Uso vele e forma delle vele |
| `GestioneMapsui.vb` | Mappe Mapsui con cache delle tile su disco |
| `GestionePdf.vb`, `GestioneStampe.vb`, `ResultsClipboard.vb` | Report PDF, stampe, copia risultati |
| `clsChartsSyncManager.vb`, `clsChartEventiViewModel.vb`, `clsGroupLines.vb`, `SciChart\RubberBandModifier.vb` | Sincronizzazione e supporto ai grafici SciChart |
| `clsRatingUtilities.vb`, `MilfDataProvider.vb` | Utilità di rating, provider dati MILF |

### UserControl (cartella `UserControl\`, codice attivo)
- **Dati e grafici temporali**: `UserControlTimePlotView`, `UserControlSciChartPlot`, `UserControlSciChartXYplot`,
  `UserControlTimeRangeSummary`, `UserControl_Highlight`, `UserControlLiftAndDrag`, `UserControlAb3D`, `SailShape`.
- **Straight line**: `UserControlStraightLine`, `UserControlStraightLinePlotContainer`, `UserControlStraightLineStandardPlot`,
  `UserControlStraightLineVmgUpwind`, `UserControlStraightLineVmgDownwind`, `UserControlStraightLineReaching`.
- **Manovre (Pavarot)**: `UserControlPavarot`, `UserControlPavarotPlot`, `UserControlPavarotAdvancedPlot`.
- **Polari e benchmark**: `UserControlPolarManager`, `UserControlPolarSelect`, `UserControlBenchmarks`,
  `UserControl360Checks`, `UserControlCrossoverTwsTwa`.
- **Partenze**: `UserControlStartExpedition`.
- **Periodi e file**: `PeriodManagerView`, `UserControl_ImportaPeriodi`, `UserControlParquetFinder`, `UserControlParquetFinderFilter`.
- **Selezione canali e utilità UI**: `ChannelSelectorAndOrderer`, `UserControlChannelSelectorTwin`, `UserControlSelFromList`,
  `UserControlNumericUpDown`, `UserControl_InputBox`, `UserControlExportHtml`.

### File NON compilati (non sono codice attivo)
Presenti su disco ma non inclusi nel `.vbproj`, tra cui:
`ClassiDesuete.vb`, `ClassiGestioneAccelerazione.vb`, `ClassiGestioneCanale.vb`, `clsSciChart.vb`, `clsTeeChart.vb`,
`clsTimePlotViewModel.vb`, `clsGroupByPlotViewModel.vb`, `clsRangeFilterViewModel.vb`, `clsDenisBinding.vb`,
`clsBloggerUtilitiy.vb`, `DataViewerInteraction.vb`, `GestioneFileSync.vb`, `GestionePeriodi2020.vb`, `GestioneReports.vb`,
`ManeuverCurrentAnalyzer_.vb`, `ParquetFinder.vb`, `ScriptHttpMidd.vb` (è `Content`), `ScriptHttpMiddCallGroupChannel.cs`,
`ctrl_*.xaml(.vb)` e, in `UserControl\`: `AeroHumanEnergyUse`, `NumPad`, `UserControlDataDisplay`,
`UserControlImportChannelsData`, `UserControlPortStbd`, `UserControlSelChannel`.

## Regole

- Compila solo quello che è elencato come `<Compile>` nel `.vbproj`. I file non inclusi (vedi elenco sopra, es.
  `ClassiDesuete.vb`, `clsTeeChart.vb`, `ManeuverCurrentAnalyzer_.vb`, `ctrl_*.xaml`): non modificarli e non considerarli
  codice attivo, a meno che non venga chiesto esplicitamente.
- Se crei un nuovo file `.vb` o `.xaml`, aggiungilo anche al `.vbproj` con la voce `<Compile>` / `<Page>` corretta
  (per gli UserControl: `<Page>` per il `.xaml` e `<Compile>` con `<DependentUpon>` per il `.xaml.vb`, come gli esistenti).
- `Option Strict` è **Off**: NON attivarlo e non riscrivere il codice per renderlo strict.
- Non aggiornare pacchetti NuGet, versione di SciChart o target framework senza chiedere.
- Non toccare `AKCurrentLib.dll`, le DLL in `x64\`, `x86\` e `Libs\`.
- Non modificare le impostazioni di pubblicazione ClickOnce nel `.vbproj`.
- SciChart usa ancora le API obsolete `SeriesSource` / `ChartSeriesViewModel`: non migrarle se non richiesto.
- Dopo ogni modifica al codice esegui la build e correggi gli errori.
- Mantieni lo stile esistente: nomi e commenti in italiano.
