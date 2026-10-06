Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Controls.Primitives
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports BruTile.Predefined
Imports BruTile.Web
Imports Mapsui.Geometries
Imports Mapsui.Layers
Imports Mapsui.Providers
'Imports Mapsui.Samples.Common.Helpers
Imports Mapsui.Styles
Imports Mapsui.UI
Imports Mapsui.UI.Wpf
Imports Mapsui.Utilities
Imports OfficeOpenXml
Imports PropertyChanged
Imports SciChart.Data.Model
Imports SPwpf
Imports System.Windows.Threading

<AddINotifyPropertyChangedInterface>
Class MainWindow
  Public Property Versione As String
  Public Property HdwID As String

  Public Property CurrentPeriodDescription As String
  Public Property CurrentMapsuiChart As String
  Public Property PeriodDetailsFontSize As Double = 16


  Public Sub New()

    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startA - inizio")

    If Not clsEzriz.ValidLicense() Then
      End
    End If
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startB - licenza")

    HdwID = clsEzriz.GetHdwId & ", exp: " & clsEzriz.GetExpirationDate

    'AppConfig = New clsSettings2021()
    Dim Profile As New clsProfileSelection
    AppConfig = Profile.AppTmp
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startC - profilo")


    If Not (AppConfig.ProfileLoaded) Then
      MsgBox("Unable to load a valid profile", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Critical Error")
      End
    End If

    AggiornaTitolo()

    ' TgtManager e' una globale di mdlCommon usata da tutta l'applicazione (mappa, plot,
    ' report). Veniva inizializzata come effetto collaterale del costruttore della tab Polars:
    ' ora che le tab sono differite va creata esplicitamente qui.
    AssicuraTgtManager()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startC2 - TgtManager")

    CambiaLingua("en-US")
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startD - lingua")


    ' This call is required by the designer.
    InitializeComponent()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startE - InitializeComponent")
    clsLogTempi.Avvia()



    SciChartEventi.DataContext = New clsChartEventiViewModel
    lbl_ValoriEventi.DataContext = SciChartEventi.DataContext
    btn_ShowCursor.DataContext = SciChartEventi.DataContext
    btn_RubberBandEnabled.DataContext = SciChartEventi.DataContext
    GraficoEventiViewModel = SciChartEventi.DataContext
    pb_sb_Loading.Visibility = Visibility.Hidden
    DataContext = Me ' ma o vero fai???
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startF - viewmodel eventi")
    MapControl = New clsGestioneMapsui(TrackPlot)
    AggiornaEtichettaMappa()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startG - mapsui")

    Versione = "v02 - 2026 10 06 12"

    Application.CloseLoadingForm()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " startH - fine")

  End Sub

  Dim _Titolo As String
  Public Property Titolo As String
    Get
      Return _Titolo
    End Get
    Set(value As String)
      _Titolo = value
    End Set
  End Property

  Public Property ListaPeriodi As ObservableCollection(Of clsPeriod2021)
    Get
      Return PeriodsManager.Periods.ListaOrdinata
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      PeriodsManager.Periods.Lista = value
    End Set
  End Property


  Public Sub FontSizeSmaller()
    PeriodDetailsFontSize -= 1
  End Sub

  Public Sub FontSizeBigger()
    PeriodDetailsFontSize += 1
  End Sub

  Public Sub CurrentPeriod(Period As clsPeriod2021)
    If Period Is Nothing Then
      CurrentPeriodDescription = ""
    Else
      CurrentPeriodDescription = Period.DescrizioneMultiriga
    End If
  End Sub


  Private Function NuovaColonna(Name As String, BindingName As String) As DataGridTextColumn
    Dim ClnTmp As New DataGridTextColumn
    ClnTmp.Header = Name
    Dim b = New Binding(BindingName)
    Select Case BindingName
      Case "LongName", "ShortName", "LongUM", "ShortUM", "Decimals", "BenchmarkHeader", "Export", "MinVal", "MaxVal", "AlertDelta"
        ClnTmp.IsReadOnly = False
      Case Else
        ClnTmp.IsReadOnly = True
    End Select

    ClnTmp.Binding = b

    Return ClnTmp
  End Function

  Private Function NuovaColonnaCheckBox(Name As String, BindingName As String) As DataGridCheckBoxColumn
    Dim ClnTmp As New DataGridCheckBoxColumn
    ClnTmp.Header = Name
    Dim b = New Binding(BindingName)
    Select Case BindingName
      Case "Export"
        ClnTmp.IsReadOnly = False
      Case Else
        ClnTmp.IsReadOnly = True
    End Select
    ClnTmp.Binding = b
    Return ClnTmp
  End Function

  Private Function NuovaColonnaCombo(Name As String, BindingName As String, ListaEnum As List(Of String)) As DataGridComboBoxColumn
    Dim ClnTmp As New DataGridComboBoxColumn
    ClnTmp.Header = Name
    ClnTmp.ItemsSource = System.Enum.GetValues(GetType(clsChannel2020.eDataType)).Cast(Of clsChannel2020.eDataType) 'Enum.GetValues(TypeOf(EffectStyle)).Cast<EffectStyle>() 'ListaEnum
    Dim b = New Binding(BindingName)
    ClnTmp.SelectedItemBinding = b
    Return ClnTmp
  End Function

  Private Function NuovaColonnaCombo(Name As String, BindingName As String) As DataGridComboBoxColumn
    Dim ClnTmp As New DataGridComboBoxColumn
    ClnTmp.Header = Name
    ClnTmp.ItemsSource = System.Enum.GetValues(GetType(clsChannel2020.eDataType)).Cast(Of clsChannel2020.eDataType) 'Enum.GetValues(TypeOf(EffectStyle)).Cast<EffectStyle>() 'ListaEnum
    Dim b = New Binding(BindingName)
    ClnTmp.SelectedItemBinding = b
    Return ClnTmp
  End Function



  'Private Sub RiempiGraficiBase()
  '  CaricaGraficiBase()
  'End Sub


  ''' <summary>
  ''' Costruisce l'elenco dei nomi canale realmente disponibili, senza forzare il caricamento
  ''' dei valori dal parquet (a differenza di DataProvider2020.CanaleDbl, che e' lazy ma carica).
  ''' </summary>
  Private Function CanaliDisponibili() As HashSet(Of String)
    Dim Risultato As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    If DataProvider2020 Is Nothing Then Return Risultato
    If DataProvider2020.Channels Is Nothing Then Return Risultato
    If DataProvider2020.Channels.ListaCanali Is Nothing Then Return Risultato
    For Each Canale In DataProvider2020.Channels.ListaCanali
      If Not Canale Is Nothing AndAlso Not Canale.ChannelId Is Nothing Then
        Risultato.Add(Canale.ChannelId)
      End If
    Next
    Return Risultato
  End Function

  Private Sub CaricaGraficiBase()
    Dim Inizio As DateTime = Now
    clsLogTempi.Scrivi("CaricaGraficiBase: inizio (poi parte il prefetch dei Data Plots)")
    Dim ChartsName As String = "BasicCharts"
    MatriceControlliBase = New clsMatriceControlliSinglePeriod(ChartsName)

    DataPlotSync.AggiornaVisibleRange(DataProvider2020.TimeRange, False)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD0")

    ' Si creano SOLO i ViewModel (operazione leggerissima).
    ' I dati vengono caricati da clsTimePlotViewModel.CaricaSeNecessario() quando il plot
    ' entra effettivamente in viewport, oppure dal prefetch in background.
    Dim Disponibili As HashSet(Of String) = CanaliDisponibili()
    Dim c As Integer = 0
    For Each GB In AppConfig.ActiveProfile.BasicChartSettings
      Dim NumCanaliValidi As Integer = 0
      If Not GB.YaxisChannels Is Nothing Then
        For Each Hdr As String In GB.YaxisChannels
          If Not Hdr Is Nothing AndAlso Disponibili.Contains(Hdr) Then NumCanaliValidi += 1
        Next
      End If
      If NumCanaliValidi > 0 Then
        Dim TmpViewModel As New clsTimePlotViewModel(DataPlotSync, MatriceControlliBase, GB)
        MatriceControlliBase.MatriceControlli.Add(TmpViewModel)
      End If
      c += 1
    Next
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD0d" & c & " VM creati:" & MatriceControlliBase.MatriceControlli.Count)

    ItmCtrl_BasicCharts.ItemsSource = MatriceControlliBase.MatriceControlli
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD0e" & c)
    btn_BasicMouseWheel.Content = "w" & DataPlotSync.SetMouseWheelMode(clsChartSyncManager.eMouseWheelZoomMode.eXonly)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD0f" & c)
    btn_BasicPanSelection.Content = "rb" & DataPlotSync.SetPanSelect(clsChartSyncManager.eRubberMode.eXonly)

    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD00")
    FillGraphEventi()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD000")

    ' Riempimento progressivo dei plot non visibili, a priorita' bassa: l'interfaccia resta
    ' reattiva e chi scorre trova i grafici gia' pronti.
    AvviaPrefetchGraficiBase()

  End Sub

  ''' <summary>
  ''' Carica in background (thread UI, priorita' Background) i plot non ancora realizzati,
  ''' uno per ciclo del dispatcher.
  ''' </summary>
  Private _GenerazionePrefetch As Integer = 0

  Private Sub AvviaPrefetchGraficiBase()
    If MatriceControlliBase Is Nothing Then Exit Sub
    Dim Matrice As clsMatriceControlliSinglePeriod = MatriceControlliBase
    Dim Indice As Integer = 0
    ' un nuovo avvio rende obsoleta la catena precedente (evita due prefetch in parallelo)
    _GenerazionePrefetch += 1
    Dim Generazione As Integer = _GenerazionePrefetch

    ' Il prefetch e' asincrono: ogni plot calcola le proprie serie su un thread di
    ' background e solo la costruzione dei grafici torna sul thread UI. I passi sono
    ' seriali, cosi' non si satura la CPU con 44 calcoli concorrenti.
    Dim Passo As Func(Of Threading.Tasks.Task) = Nothing
    Passo = Async Function() As Threading.Tasks.Task
              While Indice < Matrice.MatriceControlli.Count
                ' se nel frattempo e' stato ricaricato un altro set di file, si abbandona
                If Not Matrice Is MatriceControlliBase Then Return
                ' catena superata da un nuovo avvio, oppure tab Data Plots non visibile: ci si ferma,
                ' al ritorno sul tab il prefetch riparte (vedi MainTabControl_SelectionChanged)
                If Generazione <> _GenerazionePrefetch OrElse Not clsTimePlotViewModel.CaricamentoConsentito Then Return
                Dim Vm As clsTimePlotViewModel = Matrice.MatriceControlli(Indice)
                Indice += 1
                If Not Vm.DatiCaricati Then
                  Try
                    Using clsLogTempi.Misura("prefetch plot " & Indice & "/" & Matrice.MatriceControlli.Count)
                      Await Vm.CaricaSeNecessarioAsync()
                    End Using
                  Catch ex As Exception
                    Console.WriteLine("Prefetch plot fallito: " & ex.Message)
                  End Try
                  ' cede il turno: se l'utente sta scorrendo, i plot visibili passano avanti
                  Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, Sub() Passo())
                  Return
                End If
              End While
            End Function

    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, Sub() Passo())
  End Sub

  ''' <summary>
  ''' Azzera OGNI stato derivato dal dataset precedente. Va chiamata prima di creare il nuovo
  ''' clsDataProvider2020, cosi' nulla di quanto verra' mostrato puo' provenire dal set vecchio.
  ''' </summary>
  Private Sub ResetStatoDataset()

    ' 1) sgancia e abbandona i ViewModel dei plot base.
    '    Sono gli unici oggetti che si iscrivono alla globale DataPlotSync tramite WithEvents:
    '    senza sganciarli resterebbero vivi (li tiene il delegato dell'evento) e continuerebbero
    '    a reagire ai cambi di range ricalcolando statistiche sui canali del set precedente.
    If MatriceControlliBase IsNot Nothing Then
      If MatriceControlliBase.MatriceControlli IsNot Nothing Then
        For Each Vm In MatriceControlliBase.MatriceControlli
          Try
            Vm.Scollega()
          Catch ex As Exception
          End Try
        Next
        MatriceControlliBase.MatriceControlli.Clear()
      End If
    End If
    ' azzerando il riferimento si fa abortire anche l'eventuale prefetch ancora in coda
    MatriceControlliBase = Nothing
    ItmCtrl_BasicCharts.ItemsSource = Nothing

    ' 2) distrugge i controlli delle tab: verranno ricostruiti da zero alla prima apertura,
    '    quindi non possono contenere grafici o tabelle del set precedente.
    ResetControlliTab()

    ' 3) azzera le globali con contenuto derivato dai dati
    SelectedPoints = New List(Of Date)
    PavarotVisibleRange = New clsDoubleRange(0, 0)
    AccVisibleRange = New clsDoubleRange(0, 0)
    PeriodsManager.Periods.Lista.Clear()
    ExpStarts.StartsList.Clear()
    CurrentPeriodDescription = ""

    ' 4) sincronizzazione dei grafici: via il range condiviso del set precedente
    DataPlotSync.SharedXVisibleRange = Nothing

    ' 5) grafico eventi e mappa
    Try
      If GraficoEventiViewModel IsNot Nothing Then GraficoEventiViewModel.Azzera()
    Catch ex As Exception
    End Try
    Try
      If MapControl IsNot Nothing Then MapControl.AzzeraTraccia()
    Catch ex As Exception
    End Try

    lbl_sb_LoadedFiles.Content = ""

    ' 6) finestra Long Range Auto Check: conserva i canali del set precedente
    If _finestraCorrente IsNot Nothing Then
      Try
        _finestraCorrente.Close()
      Catch ex As Exception
      End Try
      _finestraCorrente = Nothing
    End If

  End Sub

  ''' <summary>
  ''' Distrugge i controlli delle tab. Grazie alla creazione differita basta azzerare il campo
  ''' e il contenuto dell'host: la prossima AssicuraCtrlXxx() ne creera' uno nuovo e pulito.
  ''' </summary>
  Private Sub ResetControlliTab()
    ctrlRaceReport = Nothing : hostRaceReport.Content = Nothing
    ctrlHighlight = Nothing : hostHighlight.Content = Nothing
    ctrlStarts = Nothing : hostStarts.Content = Nothing
    ctrlVmgUp = Nothing : hostVmgUp.Content = Nothing
    ctrlVmgDn = Nothing : hostVmgDn.Content = Nothing
    ctrlReaching = Nothing : hostReaching.Content = Nothing
    ctrlLineUp = Nothing : hostLineUp.Content = Nothing
    ctrlPavarot = Nothing : hostPavarot.Content = Nothing
    FirmaFinestreBase = Nothing
    ctrlTacks = Nothing : hostTacks.Content = Nothing
    ctrlGybes = Nothing : hostGybes.Content = Nothing
    ctrlXover = Nothing : hostXover.Content = Nothing
    ctrlXYplot = Nothing : hostXYplot.Content = Nothing
    ctrlBenchmarks = Nothing : hostBenchmarks.Content = Nothing
    ' la tab Polars dipende dal profilo e non dal dataset: si lascia com'e'
  End Sub

  Private Sub ApriFileSelezionati(FileSelezionati As List(Of String))
    LoadingProgressVisualizza()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbg0")
    ' PRIMA di tutto: via ogni traccia del dataset precedente
    ResetStatoDataset()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbg0r - reset stato")
    DataProvider2020 = New clsDataProvider2020(FileSelezionati)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbg00")
    txtSelectedFiles.Text = DataProvider2020.SelectedFilesList
    ImpostaDataGridCanali2020(DataProvider2020)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbg000")
    LoadingProgressNascondi()
  End Sub

  Private Sub SelezionaFiles()
    Dim FileSelezionati As List(Of String) = ObjFiles2020.SelezionaFileDati
    If FileSelezionati Is Nothing Then Exit Sub
    ApriFileSelezionati(FileSelezionati)
  End Sub


  Private Sub FileSelect_Click(sender As Object, e As RoutedEventArgs) Handles btn_FileSelect.Click
    SelezionaFiles()
  End Sub


  Private Sub FillGraphEventi()
    Dim Lista As New List(Of clsChannel2020)
    Lista.Add(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW))
    Lista.Add(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS))
    GraficoEventiViewModel.DrawTimePlotEventi(DataProvider2020.TimeStamps, DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA), Lista, DataProvider2020.TimeRange, PeriodsManager.Periods.Lista)
    SciChartEventi.DataContext = GraficoEventiViewModel
    lbl_ValoriEventi.DataContext = GraficoEventiViewModel
    btn_ShowCursor.DataContext = GraficoEventiViewModel
    GraficoEventiViewModel.ShowCursorValues = True
  End Sub


  Private Sub dg_LoadedFileChannels_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) 'Handles dg_LoadedFileChannels.MouseDoubleClick
    Dim DG As DataGrid = DirectCast(sender, DataGrid)
    If DG.CurrentColumn Is Nothing Then Exit Sub
    Dim Colonna As Integer = DG.CurrentColumn.DisplayIndex
    Dim ColName As String = DG.CurrentColumn.Header
    Dim Canale As clsChannel2020 = DirectCast(DG.SelectedItem, clsChannel2020)
    Dim ChannelNode As String = "Channel_" & Canale.ChannelId
    Select Case Colonna
      'Case 5
        'Canale.Importa = Not Canale.Importa
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "Load", Canale.Importa, True, True)
      Case 5 ' 6
        ' per selezionare il canale chiave
        SelezionaCanaleChiave(Canale)
      Case 10 ' 11 ' tbc
        SelezionaPolare(Canale)
      Case Else
    End Select
  End Sub

  Private Sub Dg_LoadedFileChannels_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
    Dim DG As DataGrid = DirectCast(sender, DataGrid)
    If DG.CurrentColumn Is Nothing Then Exit Sub

    Dim Colonna As Integer = e.Column.DisplayIndex ' indice della colonna modificata
    Dim ColName As String = DG.CurrentColumn.Header ' nome della colonna corrente che non va bene se il salvataggio avviene per passaggio di focus da una colonna all altra
    Dim Canale As clsChannel2020 = DirectCast(DG.SelectedItem, clsChannel2020)
    Dim ChannelNode As String = "Channel_" & Canale.ChannelId

    Select Case Colonna
      Case 0
        Dim tx As TextBox = e.EditingElement
        Canale.LongName = tx.Text
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongName", tx.Text, True, True)
      Case 1
        Dim tx As TextBox = e.EditingElement
        Canale.ShortName = tx.Text
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "ShortName", tx.Text, True, True)
      Case 2
        Dim tx As ComboBox = e.EditingElement
        Dim Indice As clsChannel2020.eDataType = DirectCast(tx.SelectedItem, clsChannel2020.eDataType)
        Canale.DataType = Indice
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "DataType", Indice, True, True)
      Case 3
        Dim tx As TextBox = e.EditingElement
        Canale.LongUM = tx.Text
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongUM", tx.Text, True, True)
      Case 4
        Dim tx As TextBox = e.EditingElement
        Canale.ShortUM = tx.Text
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "ShortUM", tx.Text, True, True)
      Case 7
        Dim tx As TextBox = e.EditingElement
        If IsNumeric(tx.Text) Then
          Dim Valore As Integer = CInt(tx.Text)
          Canale.Decimals = Valore
          'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "Decimals", Valore, True, True)
        End If
      Case 11
        Dim tx As TextBox = e.EditingElement
        Canale.BenchmarkHeader = tx.Text
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongName", tx.Text, True, True)
      Case 12
        Dim tx As CheckBox = e.EditingElement
        Canale.Export = tx.IsChecked
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongName", tx.Text, True, True)
      Case 13
        Dim tx As TextBox = e.EditingElement
        If IsNumeric(tx.Text) Then
          Canale.MinVal = Double.Parse(tx.Text)
        Else
          Canale.MinVal = Double.NaN
        End If
      Case 14
        Dim tx As TextBox = e.EditingElement
        If IsNumeric(tx.Text) Then
          Canale.MaxVal = Double.Parse(tx.Text)
        Else
          Canale.MaxVal = Double.NaN
        End If
      Case 15
        Dim tx As TextBox = e.EditingElement
        If IsNumeric(tx.Text) Then
          Canale.AlertDelta = Double.Parse(tx.Text)
        Else
          Canale.AlertDelta = Double.NaN
        End If
      Case Else

    End Select

    DataProvider2020.Channels.SalvaCanaliJson()

  End Sub

  Public dbg As Boolean = True

  Private Sub CaricaFileSelezionati()
    If DataProvider2020 Is Nothing Then
      SelezionaFiles()
      If DataProvider2020 Is Nothing Then Exit Sub
      If DataProvider2020.ParquetFiles Is Nothing Then Exit Sub
      If DataProvider2020.ParquetFiles.Count = 0 Then Exit Sub
    End If
    'If Not DataPlotSync Is Nothing Then
    DataPlotSync.SharedXVisibleRange = Nothing
    'End If
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgA")

    LoadingProgressVisualizza()
    ' le liste sono gia' state azzerate da ResetStatoDataset in ApriFileSelezionati,
    ' ma il Load Data puo' essere premuto anche con un provider gia' presente
    PeriodsManager.Periods.Lista.Clear()
    ExpStarts.StartsList.Clear()
    PeriodsManager.ReloadPeriods(DataProvider2020.ParquetFiles.First.FileInfo.Directory.FullName)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgB")

    DataProvider2020.CaricaValoriCanaliSelezionati()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgC")

    ApriFileSailUsage(False)
    ' qui vanno azzerati tutti i canali
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgD")
    CaricaGraficiBase()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgE")

    lbl_sb_LoadedFiles.Content = ""
    For Each fl In DataProvider2020.Files
      lbl_sb_LoadedFiles.Content &= fl.Name & " "
    Next
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgF")

    DataPlotSync.AggiornaVisibleRange(DataProvider2020.TimeRange, True)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgG")

    MapControl.AggiornaTracciaBase(1)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgH")


    MainTabControl.SelectedItem = BasicCharts
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " dbgI")

    LoadingProgressNascondi()

  End Sub

  Private Sub btn_FileOpen_Click(sender As Object, e As RoutedEventArgs)
    CaricaFileSelezionati()
  End Sub


  Private Sub SelezionaPolare(Canale As clsChannel2020)
    Dim WPFpup As New UserControlPolarSelect(Canale.PolarHeader)
    If WPFpup.ShowDialog() Then
      Canale.PolarHeader = WPFpup.NomePolare.Trim
      Dim ChannelNode As String = "Channel_" & Canale.ChannelId
      'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "PolarHeader", Canale.PolarHeader, True, True)
      DataProvider2020.Channels.SalvaCanaliJson()
    End If
  End Sub

  Private Sub SelezionaCanaleChiave(Canale As clsChannel2020)
    Dim CanaleBaseAssociato As clsChannels2020.eCanaliChiave = clsChannels2020.eCanaliChiave.eNone
    Dim Nomi As List(Of String) = System.Enum.GetNames(GetType(clsChannels2020.eCanaliChiave)).ToList
    Dim Indici As Integer() = System.Enum.GetValues(GetType(clsChannels2020.eCanaliChiave))
    Dim CanCh = Nomi.Select(Function(x) x.TrimStart("e")).ToList
    'CanCh.Add("None")
    For i As Integer = 0 To Nomi.Count - 1
      If Nomi(i).TrimStart("e") = Canale.CanaleChiaveStringa Then
        CanaleBaseAssociato = Indici(i)
        Exit For
      End If
    Next

    SelezionaCanaleCorrente(DataProvider2020.CanaleDbl(CanaleBaseAssociato))
    'Dim CanCh = DataProvider2020.Channels.ListaCanali.Where(Function(x) Not x.CanaleChiave = clsChannels2020.eCanaliChiave.eNone AndAlso Not x.CanaleChiave = Nothing).Select(Function(x) x.CanaleChiave.ToString.TrimStart("e")).ToList
    Dim sel As New List(Of String)
    sel.Add(CanaleBaseAssociato.ToString)
    Dim res As List(Of String) = GestisciListaCanali("Select Key Channel", CanCh, sel, False)

    'Dim WPFpup As New UserControlSelChannel(Canale.CanaleChiave, DataProvider2020, False)
    'If WPFpup.ShowDialog() Then
    If res.Count > 0 Then
      Dim id = CanCh.IndexOf(res.First)
      Dim CS As clsChannels2020.eCanaliChiave = id
      ' non si fa nulla se il canale chiave è lo stesso, anche se none
      If Not CS = Canale.CanaleChiave Then
        ' il canalechiave non é lo stesso
        ' salva il nuovo canale associato
        Canale.CanaleChiave = CS
        Dim ChannelNode As String = "Channel_" & Canale.ChannelId
        'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "KeyChannel", Canale.CanaleChiave, True, True)
        If Not CS = clsChannels2020.eCanaliChiave.eNone Then
          'cerca eventuali canali che erano associati al canale chiave neo associato e li dissocia
          For Each CanaleTmp As clsChannel2020 In DataProvider2020.Channels.ListaCanali
            If Not CanaleTmp Is Canale Then
              If CanaleTmp.CanaleChiave = Canale.CanaleChiave Then
                CanaleTmp.CanaleChiave = clsChannels2020.eCanaliChiave.eNone
                ChannelNode = "Channel_" & CanaleTmp.ChannelId
                'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, ChannelNode, "KeyChannel", CanaleTmp.CanaleChiave, True, True)
              End If
            End If
          Next
        End If
        DataProvider2020.Channels.SalvaCanaliJson()
      End If

      'RiempiGraficiBase()
    End If
    SelezionaCanaleCorrente(Nothing)
  End Sub


  Private Sub SelezionaCanaleCorrente(CanaleCorrente As clsChannel2020)
    For Each Canale As clsChannel2020 In DataProvider2020.Channels.ListaCanali
      Canale.IsSelected = False
    Next
    If CanaleCorrente Is Nothing Then
      Exit Sub
    Else
      CanaleCorrente.IsSelected = True
    End If

  End Sub

  Private Sub MainTabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MainTabControl.SelectionChanged
    If Not e.OriginalSource Is MainTabControl Then Exit Sub
    Dim Tab As TabItem = TryCast(MainTabControl.SelectedItem, TabItem)
    clsLogTempi.Scrivi("TAB selezionato: " & If(Tab Is Nothing, "?", Convert.ToString(Tab.Header)))

    ' il caricamento dei plot di Data Plots e' consentito solo mentre quel tab e' visibile:
    ' altrimenti rallenterebbe gli altri tab (es. XY Plots). Tornando su Data Plots riparte il riempimento.
    Dim DataPlotsVisibile As Boolean = (Tab Is BasicCharts)
    If DataPlotsVisibile = clsTimePlotViewModel.CaricamentoConsentito Then Exit Sub
    clsTimePlotViewModel.CaricamentoConsentito = DataPlotsVisibile
    If DataPlotsVisibile Then AvviaPrefetchGraficiBase()
  End Sub

  Private Sub Btn_BasicMouseWheel_Click(sender As Object, e As RoutedEventArgs) Handles btn_BasicMouseWheel.Click
    Dim strTmp As String = DataPlotSync.ToggleMouseWheel()
    btn_BasicMouseWheel.Content = "w" & strTmp
    btn_BasicMouseWheel.ToolTip = "Mouse Wheel " & strTmp
  End Sub

  Private Sub Btn_BasicPanSelection_Click(sender As Object, e As RoutedEventArgs) Handles btn_BasicPanSelection.Click
    Dim strTmp As String = DataPlotSync.TogglePanSelect()
    btn_BasicPanSelection.Content = "rb" & strTmp
    btn_BasicPanSelection.ToolTip = "Mouse Right Button " & strTmp
  End Sub

  Private Sub SciChartEventi_MouseMove(sender As Object, e As MouseEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If DataProvider2020.TimeStamps Is Nothing Then Exit Sub
    If DataProvider2020.TimeStamps.Count = 0 Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub

    Dim Point As System.Windows.Point = e.GetPosition(e.Source)
    Dim TR As clsTimeRange = GraficoEventiViewModel.VisibleX
    Dim scs As SciChart.Charting.Visuals.SciChartSurface = DirectCast(sender, SciChart.Charting.Visuals.SciChartSurface)
    Dim Dx As Double = Point.X / scs.RenderSurface.ActualWidth
    Dim MillsFromStart As Double = TR.Durata.TotalMilliseconds * Dx
    Dim Momento As DateTime = TR.Start.AddMilliseconds(MillsFromStart)
    If Momento > TR.Finish Then Momento = TR.Finish
    If Momento < TR.Start Then Momento = TR.Start

    GraficoEventiViewModel.CurrentPosition = Momento
    DataPlotSync.CurrentPosition = Momento
    MapControl.AggiornaPosizioneCorrente(Momento)

    For Each Periodo In PeriodsManager.Periods.Lista
      Dim IsInRange As Boolean = Periodo.TR.InRange(Momento)
      Periodo.IsSelected = IsInRange
    Next
    ListViewPeriodiSelectionChanged()
  End Sub


  Private Sub ListViewPeriodiSelectionChanged()
    For Each itm In ListViewPeriodi.Items
      Dim p = DirectCast(itm, clsPeriod2021)
      If p.IsSelected Then
        ListViewPeriodi.ScrollIntoView(itm)
        Exit For
      End If
    Next
    'Stop
  End Sub



  Private Sub SciChartEventi_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Dim p As clsPeriod2021 = PeriodsManager.SelectedPeriod
    If Not p Is Nothing Then
      GestisciPeriodo(p, False)
    End If
  End Sub

  Private Sub ListViewPeriodi_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Dim ic As ItemsControl = DirectCast(sender, ItemsControl)
    Dim p As clsPeriod2021 = DirectCast(DirectCast(ic, System.Windows.Controls.Primitives.Selector).SelectedItem, clsPeriod2021)
    GestisciPeriodo(p, False)
  End Sub


  Private Sub ListViewPeriodi_MouseUp(sender As Object, e As MouseButtonEventArgs)
    Dim ic As ItemsControl = DirectCast(sender, ItemsControl)
    Dim p As clsPeriod2021 = DirectCast(DirectCast(ic, System.Windows.Controls.Primitives.Selector).SelectedItem, clsPeriod2021)
    CurrentPeriod(p)
    If p Is Nothing Then Exit Sub
    Dim IR As New DateRange(p.TR.Start, p.TR.Finish)
    AggiornaSelezionePeriodo(p.TR, IR)
    ''ObjChartSyncManagerBasic.SetCurrentVisibleRange()
    'ObjChartSyncManagerBasic.SharedXVisibleRange = IR
    'GraficoEventiViewModel.AggiornaSelezione(p.TR.Clone)
    'MapControl.AggiornaSelezione(p.TR)
    'ObjChartSyncManagerBasic.AggiornaYrange()
  End Sub

  Private Sub SciChartEventi_MouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
    AzioniMouseUpGraficoEventi(Not GraficoEventiViewModel.MousePositionChanged)
  End Sub

  Private Sub SciChartEventi_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
    AzioniMouseUpGraficoEventi(Not GraficoEventiViewModel.MousePositionChanged)
  End Sub

  Private Sub AzioniMouseUpGraficoEventi(SelezionePeriodi As Boolean)
    If SelezionePeriodi Then
      Dim p = PeriodsManager.SelectedPeriods
      If p.Count > 0 Then
        Dim TR = PeriodsManager.SelectedPeriodsTimeRange
        Dim IR As New DateRange(TR.Start, TR.Finish)
        AggiornaSelezionePeriodo(TR, IR)
        'GraficoEventiViewModel.AggiornaSelezione(TR)
        'ObjChartSyncManagerBasic.SharedXVisibleRange = IR
        'MapControl.AggiornaSelezione(TR)
        'ObjChartSyncManagerBasic.AggiornaYrange()
      End If
    Else
      'Stop
      Dim PosizioneIniziale As DateTime = GraficoEventiViewModel.CurrentPositionMouseDown
      Dim MomentoCorrente As DateTime = GraficoEventiViewModel.CurrentPosition
      Dim TR = New clsTimeRange(PosizioneIniziale, MomentoCorrente)
      Dim IR As New DateRange(TR.Start, TR.Finish)
      AggiornaSelezionePeriodo(TR, IR)
      'GraficoEventiViewModel.AggiornaSelezione(TR)
      'ObjChartSyncManagerBasic.SharedXVisibleRange = IR
      'MapControl.AggiornaSelezione(TR)
      'ObjChartSyncManagerBasic.AggiornaYrange()
    End If
  End Sub


  Private Sub GestisciPeriodo(Periodo As clsPeriod2021, Nuovo As Boolean)
    If Periodo Is Nothing Then Exit Sub
    Dim TR As clsTimeRange = Periodo.TR.Clone
    Dim KM As DateTime = Periodo.KeyMoment
    Dim WPFpup As New PeriodManagerView(Periodo, Nuovo)
    If WPFpup.ShowDialog() Then
      'Stop
      Select Case WPFpup.Action
        Case PeriodManagerView.eAction.eDelete
          PeriodsManager.Elimina(Periodo, True)
          GraficoEventiViewModel.AggiornaPeriodi()
        Case PeriodManagerView.eAction.eSave
          Dim p As clsPeriod2021 = WPFpup.ObjPMVM.SelectedPeriod
          p.UpdateDetails()
          If Nuovo Then
            PeriodsManager.AddIfNew(p)
          End If
          PeriodsManager.SalvaPeriodiJsonFile()
          p.IsSelected = True
          GraficoEventiViewModel.AggiornaPeriodi()
      End Select
      'Else
      '	Stop
    End If
  End Sub


  Private Sub AggiungiPeriodo(TR As clsTimeRange)
    Dim Tipo As clsPeriod2021.ePeriodType = DeduciTipoPeriodo(TR)
    Dim NuovoPeriodo As New clsPeriod2021(TR.Clone, Tipo)

    GestisciPeriodo(NuovoPeriodo, True)
  End Sub

  ''' <summary>
  ''' Propone il tipo del periodo creato a mano invece di lasciarlo sempre indefinito.
  ''' Prima cerca tra i periodi individuati automaticamente quello che copre di piu' il range;
  ''' se non ce ne sono, decide in base all'andatura media e alla sua variazione.
  ''' </summary>
  Private Function DeduciTipoPeriodo(TR As clsTimeRange) As clsPeriod2021.ePeriodType
    If DataProvider2020 Is Nothing Then Return clsPeriod2021.ePeriodType.eUndefined

    Try
      ' 1) il finder ha gia' classificato le righe: si prende il tipo piu' sovrapposto
      If Not PeriodsManager Is Nothing AndAlso Not PeriodsManager.TempPeriods Is Nothing Then
        Dim Migliore As clsPeriod2021 = Nothing
        Dim MaxSovrapposizione As Double = 0
        For Each p As clsPeriod2021 In PeriodsManager.TempPeriods.Lista
          If p.PeriodType = clsPeriod2021.ePeriodType.eUndefined Then Continue For
          If Not p.TR.IsOverlapped(TR) Then Continue For
          Dim Da As DateTime = If(p.TR.Start > TR.Start, p.TR.Start, TR.Start)
          Dim A As DateTime = If(p.TR.Finish < TR.Finish, p.TR.Finish, TR.Finish)
          Dim Sec As Double = A.Subtract(Da).TotalSeconds
          If Sec > MaxSovrapposizione Then
            MaxSovrapposizione = Sec
            Migliore = p
          End If
        Next

        ' si accetta solo se copre almeno meta' del range selezionato
        If Not Migliore Is Nothing Then
          Dim Durata As Double = TR.Finish.Subtract(TR.Start).TotalSeconds
          If Durata > 0 AndAlso MaxSovrapposizione >= Durata / 2 Then Return Migliore.PeriodType
        End If
      End If

      ' 2) nessun periodo automatico utile: si guarda l'andatura
      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      If chTwa Is Nothing Then Return clsPeriod2021.ePeriodType.eUndefined

      Dim i As Integer = TR.IdRigaIniziale
      Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
      If l < 1 Then Return clsPeriod2021.ePeriodType.eUndefined

      Dim Valori As Double() = chTwa.Valori.Skip(i).Take(l).Where(Function(x) Not Double.IsNaN(x)).ToArray
      If Valori.Count < 2 Then Return clsPeriod2021.ePeriodType.eUndefined

      ' cambio di mura dentro il range: e' una manovra, virata o strambata secondo l'andatura
      Dim Positivi As Integer = Valori.Where(Function(x) x >= 0).Count
      Dim CambioMure As Boolean = Positivi > 0 AndAlso Positivi < Valori.Count
      Dim TwaAssoluto As Double = Valori.Select(Function(x) Math.Abs(x)).Average

      If CambioMure Then
        If TwaAssoluto > 90 Then Return clsPeriod2021.ePeriodType.eGybe
        Return clsPeriod2021.ePeriodType.eTack
      End If

      ' mure costanti: bolina o poppa danno un tratto in VMG, il resto e' lasco
      If TwaAssoluto <= 65 OrElse TwaAssoluto > 120 Then Return clsPeriod2021.ePeriodType.eStraightLineVmg
      Return clsPeriod2021.ePeriodType.eStraightLineReaching

    Catch ex As Exception
      Return clsPeriod2021.ePeriodType.eUndefined
    End Try
  End Function


  Private Sub CheckNewPeriods(sender As Object, e As RoutedEventArgs)
    'If DataProvider2020 Is Nothing Then Exit Sub
    'If PeriodsManager.Periods.Lista.Count = 0 Then
    '  If AppConfig.ActiveProfile.PeriodsFinderSettings Is Nothing Then
    '    AppConfig.ActiveProfile.PeriodsFinderSettings = New clsPeriodsFinderSettings
    '    AppConfig.ActiveProfile.PeriodsFinderSettings.ImpostaValoriDefault()
    '    AppConfig.Salva()
    '  End If
    '  Dim c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
    'End If

    If DataProvider2020 Is Nothing Then Exit Sub

    If AppConfig.ActiveProfile.PeriodsFinderSettings Is Nothing Then
      AppConfig.ActiveProfile.PeriodsFinderSettings = New clsPeriodsFinderSettings
      AppConfig.ActiveProfile.PeriodsFinderSettings.ImpostaValoriDefault()
      AppConfig.Salva()
    End If

    ' forza il ricalcolo di SailingState: e' quello che riempie TempPeriods,
    ' indipendentemente dal fatto che il file abbia gia' periodi salvati
    Dim ss = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eSailingState)
    If ss IsNot Nothing Then ss.Valori = Nothing
    Dim c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)




    For Each p In PeriodsManager.TempPeriods.Lista
      If p.TR.IsOverlapped(DataPlotSync.VisibleRange) Then
        Select Case p.PeriodType
          Case clsPeriod2021.ePeriodType.eGybe, clsPeriod2021.ePeriodType.eTack
            PeriodsManager.AddIfNew(p, False, True)', False)
          Case clsPeriod2021.ePeriodType.eStraightLineVmg
            PeriodsManager.AddIfNew(p, False, True)', False)
          Case clsPeriod2021.ePeriodType.eStraightLineReaching
            PeriodsManager.AddIfNew(p, False, True)', False)
          Case clsPeriod2021.ePeriodType.eRoundUp, clsPeriod2021.ePeriodType.eBearAway
            PeriodsManager.AddIfNew(p, False, True) ', False)
          Case Else
        End Select
      End If
    Next
    PeriodsManager.Periods.AggiornaCollections()
    PeriodsManager.SalvaPeriodiJsonFile()
    GraficoEventiViewModel.AggiornaPeriodi()
  End Sub

  Private Sub SciChartEventi_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
    GraficoEventiViewModel.SetCurrentMousePosition(e.OriginalSource.GetType Is GetType(System.Windows.Shapes.Line))
  End Sub

  Private Sub SciChartEventi_PreviewMouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
    GraficoEventiViewModel.SetCurrentMousePosition(e.OriginalSource.GetType Is GetType(System.Windows.Shapes.Line))
  End Sub


  Private Sub MarkRows(Intervallo As clsTimeRange, SetAsValidRows As Boolean)
    Stop
    'DataProvider2020.Channels.CanaleValidRows.SetNotValidRowsValue(Intervallo, SetAsValidRows, True)
    'GraficoEventiViewModel.AggiornaRigheValide(DataProvider2020.Channels.CanaleValidRows.PeriodiNonValidi)
  End Sub

  Private Sub Btn_MarkAsNotValid_Click(sender As Object, e As RoutedEventArgs) Handles btn_MarkAsNotValid.Click
    MarkRows(GraficoEventiViewModel.SelectedTimeRange, False)
  End Sub

  Private Sub Btn_MarkAsValid_Click(sender As Object, e As RoutedEventArgs) Handles btn_MarkAsValid.Click
    MarkRows(GraficoEventiViewModel.SelectedTimeRange, True)
  End Sub

  Private Sub Btn_GraficoEventiZoomExtents_Click(sender As Object, e As RoutedEventArgs) Handles btn_GraficoEventiZoomExtents.Click
    SciChartEventi.ZoomExtents()
  End Sub

  Private Sub Lbl_ValoriEventi_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles lbl_ValoriEventi.MouseUp
    Dim lbl As Label = DirectCast(sender, Label)
    If lbl.HorizontalAlignment = HorizontalAlignment.Left Then
      If lbl.VerticalAlignment = VerticalAlignment.Top Then
        lbl.HorizontalAlignment = HorizontalAlignment.Right
      Else
        lbl.VerticalAlignment = VerticalAlignment.Top
      End If
    Else
      If lbl.VerticalAlignment = VerticalAlignment.Top Then
        lbl.VerticalAlignment = VerticalAlignment.Bottom
      Else
        lbl.HorizontalAlignment = HorizontalAlignment.Left
      End If
    End If

  End Sub





  Private Sub ExportToCsv(sender As Object, e As RoutedEventArgs)
    Dim tmp As New clsExportToCsv
    'tmp.ExportNotEmpty(5, True)
    tmp.ExportSelected()
  End Sub

  Private Sub ApriDataFolder(sender As Object, e As RoutedEventArgs)
    ApriExplorer(AppConfig.ActiveProfile.ProfileFilePath)
  End Sub

  Private Sub ApriCurrentFileFolder(sender As Object, e As RoutedEventArgs)
    ApriExplorer(AppConfig.ActiveProfile.LastPpfFolder)
  End Sub


  Private Sub CompactCurrentFile(sender As Object, e As RoutedEventArgs)
    '' compatta file
    'Dim tmp As New clsCompaqParquet

    clsParquetUtilities.MakeOneSingleParquetFromFilesAndPeriods(False)

  End Sub

  Private Sub CompactCurrentFileExportSelected(sender As Object, e As RoutedEventArgs)
    '' compatta file
    'Dim tmp As New clsCompaqParquet

    clsParquetUtilities.MakeOneSingleParquetFromFilesAndPeriods(True)

  End Sub



  Private Sub ImpostaDataGridCanali2020(DataProvider As clsDataProvider2020)

    dg_LoadedFileChannels.Columns.Clear()
    dg_LoadedFileChannels.ItemsSource = Nothing
    dg_LoadedFileChannels.Items.Clear()

    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Channel Name", "LongName")) '0
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Short Name", "ShortName")) '1
    dg_LoadedFileChannels.Columns.Add(NuovaColonnaCombo("Data Type", "DataType")) '2
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Unit", "LongUM")) '3
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Short Unit", "ShortUM"))
    'dg_LoadedFileChannels.Columns.Add(NuovaColonna("To Be Loaded", "Importa"))
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Key Channel", "CanaleChiaveStringa"))
    'dg_LoadedFileChannels.Columns.Add(NuovaColonna("Raw ID", "IdCanale"))
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Raw Header", "ActualLogHeader"))
    'dg_LoadedFileChannels.Columns.Add(NuovaColonna("Key Channel Default Headers", "HeadersString"))
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Decimals", "Decimals"))
    'dg_LoadedFileChannels.Columns.Add(NuovaColonna("TestValue", "TestValue"))
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("ChannelId", "ChannelId"))

    dg_LoadedFileChannels.Columns.Add(NuovaColonnaCheckBox("MathCh", "IsMath"))

    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Polar", "PolarHeader")) '10

    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Benchmark", "BenchmarkHeader"))

    dg_LoadedFileChannels.Columns.Add(NuovaColonnaCheckBox("Export", "Export")) '12

    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Min", "MinVal")) '13
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("Max", "MaxVal")) '14
    dg_LoadedFileChannels.Columns.Add(NuovaColonna("AlertD", "AlertDelta")) '15

    'Dim Lista = dataProvider2020.Channels.CanaliTrovati.OrderBy(Function(x) x.ActualLogHeader).OrderByDescending(Function(x) x.Importa)
    dg_LoadedFileChannels.ItemsSource = DataProvider.Channels.ListaCanali 'Lista.ToList



  End Sub

  Private Sub Btn_ProveGraficoEventi_Click(sender As Object, e As RoutedEventArgs)

  End Sub

  Private Sub MapControlZoomAll(sender As Object, e As RoutedEventArgs)
    MapControl.ZoomToAll()
  End Sub

  Private Sub MapControlZoomSel(sender As Object, e As RoutedEventArgs)
    MapControl.ZoomToActualSelection()
  End Sub

  Private Sub MapControlZoomIn(sender As Object, e As RoutedEventArgs)
    MapControl.ZoomIn()
  End Sub

  Private Sub MapControlZoomOut(sender As Object, e As RoutedEventArgs)
    MapControl.ZoomOut()
  End Sub

  Private Sub MapControlToggleChart(sender As Object, e As RoutedEventArgs)
    MapControl.TogglaMappa()
    AggiornaEtichettaMappa()
  End Sub

  Private Sub TrackPlot_MouseUp(sender As Object, e As MouseButtonEventArgs)
    Dim obj = DirectCast(sender, Mapsui.UI.Wpf.MapControl)
    Dim a = e.GetPosition(obj).ToMapsui
    Dim p = obj.Viewport.ScreenToWorld(a)
    Dim l = Mapsui.Projection.SphericalMercator.ToLonLat(p.X, p.Y)
    'Cursor.Content = l.Y.ToString("F6") & " " & l.X.ToString("F6")
    If e.ChangedButton = MouseButton.Right Then
      If mcDrawLine.IsChecked Then
        MapControl.AggiungiPuntoToLayerDisegno(p)
        Cursor.Content = MapControl.DescrizioneDisegno
      End If
    End If

  End Sub


  Private Sub MapControlStopDrawing(sender As Object, e As RoutedEventArgs)
    MapControl.NascondiLayerDisegno()
    Cursor.Content = ""

  End Sub


  Private Sub FiltraGrigliaCanali()
    If DataProvider2020 Is Nothing Then Exit Sub
    Dim Visibile As Visibility = DataProvider2020.Channels.ListaCanali.First.Visibility

    If txt_Search.Text.Trim = "" Then
      dg_LoadedFileChannels.ItemsSource = DataProvider2020.Channels.ListaCanali
    Else
      Dim ListaTmp As New ObservableCollection(Of clsChannel2020)
      For Each canale In DataProvider2020.Channels.ListaCanali
        Dim strTmp As String = canale.CanaleChiaveStringa & " " & canale.ChannelId & " " & If(canale.KnownHeaders Is Nothing, "", String.Join(" ", canale.KnownHeaders)) & " " & canale.LongName & " " & canale.PolarHeader & " " & canale.BenchmarkHeader
        If strTmp.IndexOf(txt_Search.Text.Trim, StringComparison.CurrentCultureIgnoreCase) > -1 Then
          ListaTmp.Add(canale)
        End If
      Next
      dg_LoadedFileChannels.ItemsSource = ListaTmp
    End If
  End Sub

  Private Sub MapControl1Hz(sender As Object, e As RoutedEventArgs)
    MapControl.AggiornaTracciaBase(1)
  End Sub

  Private Sub MapControl20Hz(sender As Object, e As RoutedEventArgs)
    MapControl.AggiornaTracciaBase(20)
  End Sub

  Private Sub btn_PesestalStatistics_Click(sender As Object, e As RoutedEventArgs)
    Stop
    'Dim strTmp As String = "StraightLine" & vbCrLf & PeriodsManager.CreaStatistichePedestals_StraightLines & vbCrLf & vbCrLf & vbCrLf & vbCrLf
    'strTmp &= "Manoeuvers" & vbCrLf & PeriodsManager.CreaStatistichePedestals_Pavarots & vbCrLf & vbCrLf & vbCrLf & vbCrLf
    'strTmp &= "Accelerations" & vbCrLf & PeriodsManager.CreaStatistichePedestals_Accelerations & vbCrLf & vbCrLf & vbCrLf & vbCrLf
    'Clipboard.SetText(strTmp)
  End Sub

  Private Sub btn_ExportPrd_Click(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    clsSailShapeUtilities.ExportStraightLinesPeriods(PeriodsManager.Periods.ListaOrdinata.ToList, DataProvider2020)
    'PeriodsManager.EsportaPrd()
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    txt_Search.Text = ""
    'PowerZoneReport()
    'FiltraGrigliaCanali()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)

  End Sub

  Private Sub btn_UpdateKeys_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.AssignKeysToPeriodsInTimeRange(DataPlotSync.VisibleRange)
    'PeriodsManager.AssignKeysToPeriodsInTimeRangeSelectiveChannel(ObjChartSyncManagerBasic.VisibleRange)
    'PeriodsManager.AssignKeysToPeriodsInTimeRangeCustom(ObjChartSyncManagerBasic.VisibleRange)
  End Sub

  Private Sub btn_AddNewBasicChart_Click(sender As Object, e As RoutedEventArgs)
    AggiungiGrafico(MatriceControlliBase, DataPlotSync)
  End Sub



  Private Sub AggiungiGrafico(Matrice As clsMatriceControlliSinglePeriod, SyncManager As clsChartSyncManager)

    'ImpostaCanaliAsIsSelected(Nothing)

    Dim csel As New List(Of clsChannel2020)
    Dim res As List(Of clsChannel2020) = GestisciListaCanali("Seleziona Canale", DataProvider2020.Channels.ListaCanali.ToList, csel, eSelectChannelType.eMulti)
    If Not res Is Nothing Then
      Dim CS As List(Of clsChannel2020) = res
      If CS.Count > 0 Then
        Dim bcs As New clsBasicChartSettings(CS.Select(Function(x) x.ChannelId).ToList, clsGroupLines.eLineType.eRawValue, True, False, True)
        Dim TmpViewModel As New clsTimePlotViewModel(DataPlotSync, MatriceControlliBase, bcs)
        If TmpViewModel.CaricaSeNecessario() Then
          MatriceControlliBase.MatriceControlli.Add(TmpViewModel)
          AppConfig.ActiveProfile.BasicChartSettings.Add(bcs)
          AppConfig.Salva()
          ' il nuovo plot e' l'ultimo: cambia chi deve mostrare l'asse X
          For Each Vm In MatriceControlliBase.MatriceControlli
            Vm.AggiornaVisibilitaAsseX()
          Next
        End If
      End If
      'ImpostaCanaliAsIsSelected(Nothing)
    End If



  End Sub

  ''' <summary>
  ''' Aggiorna il tooltip del pulsante e l'etichetta in basso a destra sulla mappa.
  ''' Va chiamata sia all'avvio sia a ogni cambio di mappa.
  ''' </summary>
  Public Sub AggiornaEtichettaMappa()
    Try
      If MapControl Is Nothing Then Exit Sub
      CurrentMapsuiChart = "Change Chart, Current: " & MapControl.LayersString
      lbl_MapCorrente.Text = MapControl.LayersString
    Catch ex As Exception
    End Try
  End Sub

#Region "Creazione differita dei controlli delle tab"

  ''' <summary>
  ''' Crea il TgtManager globale se non esiste. Idempotente: chiamabile sia all'avvio sia
  ''' dal viewmodel della tab Polars senza ricaricare due volte il file delle polari.
  ''' </summary>
  Public Shared Sub AssicuraTgtManager()
    If TgtManager IsNot Nothing Then Exit Sub
    TgtManager = New clsTgtManager
    TgtManager.CaricaLastJsonTgtFile()
  End Sub

  ' I UserControl delle tab non vengono piu' istanziati da InitializeComponent (che costruiva
  ' tutte e dodici le schede anche se non venivano mai aperte) ma alla prima visita della tab.
  ' I campi qui sotto sostituiscono quelli che il designer generava dagli x:Name.

  Public ctrlRaceReport As UserControlTimeRangeSummary
  Public ctrlHighlight As UserControl_Highlight
  Public ctrlStarts As UserControlStartExpedition
  Public ctrlVmgUp As UserControlStraightLineVmgUpwind
  Public ctrlVmgDn As UserControlStraightLineVmgDownwind
  Public ctrlReaching As UserControlStraightLineReaching
  Public ctrlLineUp As UserControlStraightLine
  Public ctrlPavarot As UserControlPavarot
  Public ctrlTacks As UserControlPavarot ' come TacksGybes ma solo virate
  Public ctrlGybes As UserControlPavarot ' come TacksGybes ma solo strambate
  Public ctrlXover As UserControlCrossoverTwsTwa
  Public ctrlXYplot As UserControlSciChartXYplot
  Public ctrlPolars As UserControlPolarManager
  Public ctrlBenchmarks As UserControlBenchmarks

  Private Sub AssicuraCtrlRaceReport()
    If ctrlRaceReport IsNot Nothing Then Exit Sub
    ctrlRaceReport = New UserControlTimeRangeSummary
    hostRaceReport.Content = ctrlRaceReport
  End Sub

  Private Sub AssicuraCtrlHighlight()
    If ctrlHighlight IsNot Nothing Then Exit Sub
    ctrlHighlight = New UserControl_Highlight
    hostHighlight.Content = ctrlHighlight
  End Sub

  Private Sub AssicuraCtrlStarts()
    If ctrlStarts IsNot Nothing Then Exit Sub
    ctrlStarts = New UserControlStartExpedition
    hostStarts.Content = ctrlStarts
  End Sub

  Private Sub AssicuraCtrlVmgUp()
    If ctrlVmgUp IsNot Nothing Then Exit Sub
    ctrlVmgUp = New UserControlStraightLineVmgUpwind
    hostVmgUp.Content = ctrlVmgUp
  End Sub

  Private Sub AssicuraCtrlVmgDn()
    If ctrlVmgDn IsNot Nothing Then Exit Sub
    ctrlVmgDn = New UserControlStraightLineVmgDownwind
    hostVmgDn.Content = ctrlVmgDn
  End Sub

  Private Sub AssicuraCtrlReaching()
    If ctrlReaching IsNot Nothing Then Exit Sub
    ctrlReaching = New UserControlStraightLineReaching
    hostReaching.Content = ctrlReaching
  End Sub

  Private Sub AssicuraCtrlLineUp()
    If ctrlLineUp IsNot Nothing Then Exit Sub
    ctrlLineUp = New UserControlStraightLine
    hostLineUp.Content = ctrlLineUp
  End Sub

  Private Sub AssicuraCtrlPavarot()
    If ctrlPavarot IsNot Nothing Then Exit Sub
    ctrlPavarot = New UserControlPavarot
    hostPavarot.Content = ctrlPavarot
  End Sub

  Private Sub AssicuraCtrlTacks()
    If ctrlTacks IsNot Nothing Then Exit Sub
    ctrlTacks = New UserControlPavarot(UserControlPavarotViewModel.eFiltroManovre.eVirate)
    hostTacks.Content = ctrlTacks
  End Sub

  Private Sub AssicuraCtrlGybes()
    If ctrlGybes IsNot Nothing Then Exit Sub
    ctrlGybes = New UserControlPavarot(UserControlPavarotViewModel.eFiltroManovre.eStrambate)
    hostGybes.Content = ctrlGybes
  End Sub

  Private Sub AssicuraCtrlXover()
    If ctrlXover IsNot Nothing Then Exit Sub
    ctrlXover = New UserControlCrossoverTwsTwa
    hostXover.Content = ctrlXover
  End Sub

  Private Sub AssicuraCtrlXYplot()
    If ctrlXYplot IsNot Nothing Then Exit Sub
    ctrlXYplot = New UserControlSciChartXYplot
    hostXYplot.Content = ctrlXYplot
  End Sub

  Private Sub AssicuraCtrlPolars()
    If ctrlPolars IsNot Nothing Then Exit Sub
    ctrlPolars = New UserControlPolarManager
    hostPolars.Content = ctrlPolars
  End Sub

  Private Sub AssicuraCtrlBenchmarks()
    If ctrlBenchmarks IsNot Nothing Then Exit Sub
    ctrlBenchmarks = New UserControlBenchmarks
    hostBenchmarks.Content = ctrlBenchmarks
  End Sub

  Private Sub TimeRangeSummary_GotFocus(sender As Object, e As RoutedEventArgs) Handles TimeRangeSummary.GotFocus
    AssicuraCtrlRaceReport()
    AssicuraCtrlHighlight()
  End Sub

#End Region

  Private Sub LineUpsAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles LineUpsAnalysis.GotFocus
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabSL.0 - inizio")
    AssicuraCtrlLineUp()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabSL.1 - controllo creato")
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If ctrlLineUp.StraightLineVM2020.ContenitoreTwsVsChannels.ListaControlli.Count = 0 Then
      ctrlLineUp.StraightLineVM2020.VerificaSelectedStraightLines()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabSL.2 - VerificaSelectedStraightLines")
      ctrlLineUp.StraightLineVM2020.AggiornaGrafici()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabSL.3 - AggiornaGrafici")
    End If

  End Sub

  Private Sub btn_AddPeriod_Click(sender As Object, e As RoutedEventArgs) Handles btn_AddPeriod.Click
    AggiungiPeriodo(DataPlotSync.VisibleRange)
  End Sub

  'Private Sub TackGybesAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles TackGybesAnalysis.GotFocus
  '  If IsInDesignMode Then Exit Sub
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  If Not DataProvider2020.ValoriCaricati Then Exit Sub

  '  If ctrlPavarot.PavarotVM2020.ControlliPavarotBasicPlot Is Nothing Then
  '    ctrlPavarot.PavarotVM2020.PavarotTabGotFocus()
  '  End If
  'End Sub


  Private Sub TackGybesAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles TackGybesAnalysis.GotFocus
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabPAV.0 - inizio")
    AssicuraCtrlPavarot()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabPAV.1 - controllo creato")

    If IsInDesignMode Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub

    If ctrlPavarot Is Nothing OrElse ctrlPavarot.PavarotVM2020 Is Nothing Then Exit Sub

    If ctrlPavarot.PavarotVM2020.ControlliPavarotBasicPlot Is Nothing Then
      ctrlPavarot.PavarotVM2020.PavarotTabGotFocus()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabPAV.2 - PavarotTabGotFocus")
    End If

  End Sub

  Private Sub AggiornaTabPavarot(Ctrl As UserControlPavarot)
    If IsInDesignMode Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If Ctrl Is Nothing OrElse Ctrl.PavarotVM2020 Is Nothing Then Exit Sub
    If Ctrl.PavarotVM2020.ControlliPavarotBasicPlot Is Nothing Then
      Ctrl.PavarotVM2020.PavarotTabGotFocus()
    ElseIf Ctrl.PavarotVM2020.ListaCambiataDaUltimoDisegno() Then
      ' manovre eliminate/aggiunte da un altro tab: la lista e' aggiornata, i grafici vanno ridisegnati
      Ctrl.PavarotVM2020.Refresh()
    End If

    ' avvisa se le manovre sono state calcolate con un prima/dopo diverso dalle impostazioni (una volta per ogni stato, non a ogni focus).
    ' Se l utente ha cambiato ante/post in questa sessione non si propone nulla: il ricalcolo e' lasciato al pulsante Recalc.
    Dim Firma As String = FirmaFinestreManovre()
    If FirmaFinestreBase Is Nothing Then FirmaFinestreBase = Firma
    If Firma <> FirmaFinestreBase Then Exit Sub
    Dim Avviso As String = Ctrl.PavarotVM2020.AvvisoFinestreDiverse()
    If Avviso = "" Then
      Ctrl.UltimoAvvisoFinestre = ""
    ElseIf Avviso <> Ctrl.UltimoAvvisoFinestre Then
      Ctrl.UltimoAvvisoFinestre = Avviso
      Dispatcher.BeginInvoke(Sub()
                               If MsgBox(Avviso, MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation, "Manoeuvers window") = MsgBoxResult.Yes Then
                                 Ctrl.RicalcolaOra()
                               End If
                             End Sub)
    End If
  End Sub

  Private FirmaFinestreBase As String = Nothing ' impostazioni ante/post al primo focus di una scheda delle manovre dopo il caricamento del dataset

  Private Function FirmaFinestreManovre() As String
    Dim s = AppConfig.ActiveProfile.PavarotSettings
    If s Is Nothing Then Return ""
    Return s.TackSecAnte & "/" & s.TackSecPost & "/" & s.GybeSecAnte & "/" & s.GybeSecPost
  End Function

  Private Sub TacksAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles TacksAnalysis.GotFocus
    AssicuraCtrlTacks()
    AggiornaTabPavarot(ctrlTacks)
  End Sub

  Private Sub GybesAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles GybesAnalysis.GotFocus
    AssicuraCtrlGybes()
    AggiornaTabPavarot(ctrlGybes)
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    DataProvider2020.ExportInExpeditionFormat()
    'Dim ExpStarts As New clsExpeditionStarts
    'ExpStarts.CercaPartenze()
    'ExpStarts.CsvPartenzeToClipboard()
    'Dim ventiventuno As New cls2021
  End Sub

  Private Sub btn_FileExpselect_Click(sender As Object, e As RoutedEventArgs)
    If My.Computer.Keyboard.ShiftKeyDown Then
      clsExpeditionUtilities.SelezionaFilesExpeditionAndMakeParquet()
    Else
      Dim NuovoFile As String = clsExpeditionUtilities.SelezionaFileExpeditionAndMakeParquet()
      If System.IO.File.Exists(NuovoFile) Then
        ObjFiles2020.SalvaImpostazioniFileCreato(NuovoFile)
        Dim Lista As New List(Of String)
        Lista.Add(NuovoFile)
        ApriFileSelezionati(Lista)
      End If
    End If
  End Sub

  Private Sub btn_MeteoReport_Click(sender As Object, e As RoutedEventArgs)
    clsMeteoReport.CreateReport(DataPlotSync.VisibleRange, 600)
    clsKML.EsportaTracciaKml(DataPlotSync.VisibleRange, 600)
    'MsgBox("Meteo Report in the clipboard, Kml files done!")

  End Sub

  Private Sub btn_FileDfwSelect_Click(sender As Object, e As RoutedEventArgs)
    clsDfwUtilities.LogFileTmOffset = New TimeSpan(6, 0, 0)
    If My.Computer.Keyboard.ShiftKeyDown Then
      clsDfwUtilities.SelezionaFilesDeckmanAndMakeParquet()
    Else
      Dim NuovoFile As String = clsDfwUtilities.SelezionaFileDeckmanAndMakeParquet()
      If System.IO.File.Exists(NuovoFile) Then
        ObjFiles2020.SalvaImpostazioniFileCreato(NuovoFile)
        Dim Lista As New List(Of String)
        Lista.Add(NuovoFile)
        ApriFileSelezionati(Lista)
      End If
    End If
  End Sub

  Private Sub btn_PowerZoneReport_Click(sender As Object, e As RoutedEventArgs)
    PowerZoneReport()
  End Sub

  Private Sub btn_UpdateSailingState_Click(sender As Object, e As RoutedEventArgs)
    Dim ss = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eSailingState)
    ss.Valori = Nothing
    AppConfig.ActiveProfile.PeriodsFinderSettings.ValidRotationMinAngle = 40
    AppConfig.ActiveProfile.PeriodsFinderSettings.RotationYrt = 4
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteTack = 25
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostTack = 30
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteGybe = 25
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostGybe = 30
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteRot = 5
    AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostRot = 10
    AppConfig.ActiveProfile.PeriodsFinderSettings.MinDataQualityIndex = 3
    AppConfig.ActiveProfile.PeriodsFinderSettings.MaxTwaDeltaUpDn = 12
    AppConfig.ActiveProfile.PeriodsFinderSettings.MaxBsTgtPercDelta = 20
    ss = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
  End Sub

  Private Sub ReachingAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles ReachingAnalysis.GotFocus
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabRCH.0 - inizio")
    AssicuraCtrlReaching()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabRCH.1 - controllo creato")
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If ctrlReaching.StraightLineVM2020.ContenitoreTwsVsChannels.ListaControlli.Count = 0 Then
      ctrlReaching.StraightLineVM2020.VerificaSelectedStraightLines()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabRCH.2 - VerificaSelectedStraightLines")
      ctrlReaching.StraightLineVM2020.AggiornaGrafici()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabRCH.3 - AggiornaGrafici")
    End If
  End Sub

  Private Sub VmgUpAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles VmgUpAnalysis.GotFocus
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGUP.0 - inizio")
    AssicuraCtrlVmgUp()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGUP.1 - controllo creato")
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If ctrlVmgUp.StraightLineVM2020.ContenitoreTwsVsChannels.ListaControlli.Count = 0 Then
      ctrlVmgUp.StraightLineVM2020.VerificaSelectedStraightLines()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGUP.2 - VerificaSelectedStraightLines")
      ctrlVmgUp.StraightLineVM2020.AggiornaGrafici()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGUP.3 - AggiornaGrafici")
    End If
  End Sub

  Private Sub VmgDnAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles VmgDnAnalysis.GotFocus
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGDN.0 - inizio")
    AssicuraCtrlVmgDn()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGDN.1 - controllo creato")
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If ctrlVmgDn.StraightLineVM2020.ContenitoreTwsVsChannels.ListaControlli.Count = 0 Then
      ctrlVmgDn.StraightLineVM2020.VerificaSelectedStraightLines()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGDN.2 - VerificaSelectedStraightLines")
      ctrlVmgDn.StraightLineVM2020.AggiornaGrafici()
      If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " tabVMGDN.3 - AggiornaGrafici")
    End If
  End Sub

  Private Sub StartAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles StartAnalysis.GotFocus
    AssicuraCtrlStarts()
    'Stop
    If ExpStarts.StartsList.Count = 0 Then
      ExpStarts.CercaPartenze()
      If ExpStarts.StartsList.Count > 0 Then
        ExpStarts.SelectedStart = ExpStarts.StartsList.First
        AppConfig.Salva()
      End If
    End If

  End Sub

  Private Sub btn_FileFaRoSelect_Click(sender As Object, e As RoutedEventArgs)
    Dim tmp As New clsFaRoLogToParquet

  End Sub

  Private Sub btn_DeleteVisible_Click(sender As Object, e As RoutedEventArgs)
    If PeriodsManager.DeleteVisibleRangePeriods(DataPlotSync.VisibleRange) Then
      PeriodsManager.Periods.AggiornaCollections()
      PeriodsManager.SalvaPeriodiJsonFile()
      GraficoEventiViewModel.AggiornaPeriodi()
    End If
  End Sub

  Private Sub btn_FileSailShapeSelect_Click(sender As Object, e As RoutedEventArgs)
    If Not DataProvider2020 Is Nothing Then
      Dim o As New clsSailUsage()
      o.UpdateSailUsageFromTextFile(False, False)

      Dim SS As New clsSailShape2022(DataProvider2020.ParquetFiles.First.FileInfo.Directory.FullName, True)
      AppConfig.ActiveProfile.XYPlotSettings.ChannelListUpdated = True
    End If
  End Sub

  Private Sub btn_FileFastSkipper_Click(sender As Object, e As RoutedEventArgs)
    Dim NuovoFile As String = clsFastSkipperUtilities.SelezionaFilesLogFastSkipperAndMakeParquet()
    'Dim NuovoFile As String = clsExpeditionUtilities.SelezionaFileExpeditionAndMakeParquet()
    If System.IO.File.Exists(NuovoFile) Then
      ObjFiles2020.SalvaImpostazioniFileCreato(NuovoFile)
      Dim Lista As New List(Of String)
      Lista.Add(NuovoFile)
      ApriFileSelezionati(Lista)
    End If

  End Sub

  Private Sub btn_CheckVisibles_Click(sender As Object, e As RoutedEventArgs)
    For Each p In PeriodsManager.Periods.Lista
      p.IsChecked = p.TR.IsOverlapped(DataPlotSync.VisibleRange)
    Next

  End Sub

  Private Sub btn_CreaFileSails_Click(sender As Object, e As RoutedEventArgs)
    ApriFileSailUsage(True)
  End Sub

  Private Sub ApriFileSailUsage(msg As Boolean)
    Dim o As New clsSailUsage()
    'o.CreaFileSailsTxtDaGz("c:\Users\FMM\Downloads\Logs-20221129T102341Z-001_Gdrive_Faro\Logs\AllGzFiles\")
    'o.CreaTabellaOreDaSailsLog("C:\DiscoDati\Performance2021\Profiles\MySong80\Data\Vele")
    'o.CreaTabellaOreDaSailsLog("C:\DiscoDati\Performance2021\Profiles\MySong80\Data\Seasons2023vs2024\Notes")
    'o.CreaTabellaOreDaSailsLog("C:\DiscoDati\Performance2021\Profiles\MySong80\Data") ''\2025\Season\KeysOriginali")
    o.UpdateSailUsageFromTextFile(True, msg)
  End Sub


  Private Sub btn_CustomFunction_Click(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    DataProvider2020.FunzioneCustom()
  End Sub

  Private Sub btn_SelectionQuickStats_Click(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    'DataProvider2020.QuickStatsUpDnRc()
    DataProvider2020.OrcRaceReplay()
  End Sub

  Private Sub btn_Corrections_Click(sender As Object, e As RoutedEventArgs)
    clsDataCorrections.ApplicaCorrezioni(DataPlotSync.VisibleRange)
    clsDataCorrections.SalvaCopiaParquet()
  End Sub

  Private Sub btn_ImportTest_Click(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    PeriodsManager.ImportaPeriodiDaTestFile()
    'PeriodsManager.Periods.AggiornaCollections()
    PeriodsManager.SalvaPeriodiJsonFile()
    GraficoEventiViewModel.AggiornaPeriodi()

  End Sub

  Dim ListaExpTmp As New List(Of clsChannel2020)

  Private Sub ToggleExportAll(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    Dim ExpChs = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.Export = True).Count
    Select Case ExpChs
      Case 0
        For Each c In ListaExpTmp.ToList()
          c.Export = True
        Next
      Case DataProvider2020.Channels.ListaCanali.Count
        For Each c In DataProvider2020.Channels.ListaCanali.ToList()
          c.Export = False
        Next
      Case Else
        ListaExpTmp.Clear()
        For Each c In DataProvider2020.Channels.ListaCanali.Where(Function(x) x.Export).ToList
          ListaExpTmp.Add(c)
        Next
        For Each c In DataProvider2020.Channels.ListaCanali.ToList()
          c.Export = True
        Next
    End Select


  End Sub

  Private Sub ExportWayPoint(sender As Object, e As RoutedEventArgs)
    Dim GP = MapControl.GeographicPosition(GraficoEventiViewModel.CurrentPosition)
    Dim inp = InputBox("Waypoint Name", "Insert the name of the waypoint", "")
    If inp = "" Then Exit Sub
    Dim fp = System.IO.Path.Combine(AppConfig.ActiveProfile.LastPpfFolder, inp & "_" & Now.ToString("yyyyMMddhhmmss") & ".gpx")
    GpxWriter.SaveGpxWaypoint(fp, GP.LatDec, GP.LngDec, inp)
    ApriExplorer(AppConfig.ActiveProfile.LastPpfFolder)
  End Sub

  Private Sub ExportToParquet(sender As Object, e As RoutedEventArgs)
    clsParquetUtilities.MakeOneSingleParquetFromFilesAndPeriods(False, False)
  End Sub

  Private Sub btn_KeysToRaceLeg_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.SplitKeysToRaceLegToPeriodsInTimeRange(DataPlotSync.VisibleRange)
  End Sub

  Private Sub btn_UpdateRace_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.AssignRaceNameToPeriodsInTimeRange(DataPlotSync.VisibleRange)
  End Sub

  Private Sub btn_UpdateLeg_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.AssignLegNameToPeriodsInTimeRange(DataPlotSync.VisibleRange)
  End Sub

  Private Sub btn_TestStats_Click(sender As Object, e As RoutedEventArgs)
  End Sub

#Region "Profilo"

  ''' <summary>Allinea il titolo della finestra al nome del profilo attivo.</summary>
  Private Sub AggiornaTitolo()
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      Titolo = "SailingPerformer Data Analyzer"
    Else
      Titolo = "SailingPerformer Data Analyzer: " & AppConfig.ActiveProfile.ProfileName
    End If
  End Sub

  ''' <summary>Cambia il nome della barca memorizzato nel profilo attivo e lo salva.</summary>
  Private Sub btn_RenameProfile_Click(sender As Object, e As RoutedEventArgs)
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      MsgBox("No active profile.", MsgBoxStyle.Exclamation, "Rename Profile")
      Exit Sub
    End If

    Dim f As New RenameProfileForm
    f.Owner = Me
    If f.ShowDialog() <> True Then Exit Sub

    AppConfig.ActiveProfile.ProfileName = f.NuovoNome
    AppConfig.Salva()
    AggiornaTitolo()
    MsgBox("Profile renamed to """ & f.NuovoNome & """.", MsgBoxStyle.Information, "Rename Profile")
  End Sub

#End Region

#Region "Canali di qualita'"

  ''' <summary>Apre la finestra di configurazione dei canali di qualita'.</summary>
  Private Sub btn_QualitySettings_Click(sender As Object, e As RoutedEventArgs)
    Dim f As New QualitySettingsForm
    f.Owner = Me
    If f.ShowDialog() <> True Then Exit Sub
    If f.RichiestoRefresh Then RicalcolaCanaliQualita()
  End Sub

  ''' <summary>Apre la finestra del modello teorico del leeway; con "Save and Refresh" ricalcola il canale Leeway Model.</summary>
  Private Sub btn_LeewayModel_Click(sender As Object, e As RoutedEventArgs)
    Dim f As New LeewayModelForm
    f.Owner = Me
    If f.ShowDialog() <> True Then Exit Sub
    If Not f.RichiestoRefresh Then Exit Sub
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then Exit Sub
    clsDataCorrections.ClearChannelValues(clsChannels2020.eCanaliChiave.eLeewayModel)
    RidisegnaPlot(False)
  End Sub

  ''' <summary>Apre la tabella TWS / SeaState atteso; al salvataggio ricalcola i canali SeaStateNorm e SeaStateNormDelta.</summary>
  Private Sub btn_SeaStateNorm_Click(sender As Object, e As RoutedEventArgs)
    Dim f As New SeaStateNormForm
    f.Owner = Me
    If f.ShowDialog() <> True Then Exit Sub
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then Exit Sub
    clsDataCorrections.ClearChannelValues(clsChannels2020.eCanaliChiave.eSeaStateNorm)
    clsDataCorrections.ClearChannelValues(clsChannels2020.eCanaliChiave.eSeaStateNormDelta)
    RidisegnaPlot(False)
  End Sub

  ''' <summary>Forza la ricostruzione delle serie dei grafici dai valori correnti dei canali.</summary>
  Private Sub btn_RefreshPlots_Click(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then
      MsgBox("Load a file first.", MsgBoxStyle.Exclamation, "Refresh Plots")
      Exit Sub
    End If

    ' NB: Me.Cursor non e' utilizzabile, in MainWindow.xaml esiste una Label di nome "Cursor"
    Dim Cursore As Input.Cursor = Input.Mouse.OverrideCursor
    Input.Mouse.OverrideCursor = Input.Cursors.Wait
    Try
      RidisegnaPlot(False)
    Catch ex As Exception
      MsgBox("Error while refreshing the plots:" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Refresh Plots")
    Finally
      Input.Mouse.OverrideCursor = Cursore
    End Try
  End Sub

  ''' <summary>
  ''' Invalida la cache delle serie dei plot e le ricostruisce, altrimenti i grafici
  ''' continuerebbero a disegnare i valori calcolati in precedenza.
  ''' </summary>
  ''' <param name="AggiornaAnchePeriodi">
  ''' False quando cambiano solo i valori dei canali: ridisegnare i periodi non serve e
  ''' avrebbe l'effetto collaterale di azzerare la selezione corrente.
  ''' </param>
  Public Sub RidisegnaPlot(Optional AggiornaAnchePeriodi As Boolean = True)
    If Not MatriceControlliBase Is Nothing AndAlso Not MatriceControlliBase.MatriceControlli Is Nothing Then
      For Each VM As clsTimePlotViewModel In MatriceControlliBase.MatriceControlli
        If VM Is Nothing Then Continue For
        VM.InvalidaDati()
        VM.CaricaSeNecessario()
      Next
    End If

    If AggiornaAnchePeriodi Then
      If Not GraficoEventiViewModel Is Nothing Then GraficoEventiViewModel.AggiornaPeriodi()
    End If
  End Sub

  ''' <summary>
  ''' Da chiamare dopo il caricamento di una polare diversa: rifa' i canali target e tutti
  ''' quelli che ne dipendono, poi ridisegna i grafici.
  ''' </summary>
  Public Sub RicalcolaDopoCambioPolare()
    If DataProvider2020 Is Nothing Then Exit Sub

    ' NB: Me.Cursor non e' utilizzabile, in MainWindow.xaml esiste una Label di nome "Cursor"
    Dim Cursore As Input.Cursor = Input.Mouse.OverrideCursor
    Input.Mouse.OverrideCursor = Input.Cursors.Wait
    Try
      clsDataCorrections.RicalcolaCanaliPolare()
      RidisegnaPlot(False)
    Catch ex As Exception
      MsgBox("Error while recalculating the polar related channels:" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Polar")
    Finally
      Input.Mouse.OverrideCursor = Cursore
    End Try
  End Sub

  ''' <summary>
  ''' Ricalcola i tre canali di qualita' con i settaggi correnti e aggiorna i grafici,
  ''' senza bisogno di ricaricare il file.
  ''' </summary>
  Private Sub RicalcolaCanaliQualita()
    If DataProvider2020 Is Nothing Then
      MsgBox("Load a file first.", MsgBoxStyle.Exclamation, "Quality Channels")
      Exit Sub
    End If

    ' NB: Me.Cursor non e' utilizzabile, in MainWindow.xaml esiste una Label di nome "Cursor"
    Dim Cursore As Input.Cursor = Input.Mouse.OverrideCursor
    Input.Mouse.OverrideCursor = Input.Cursors.Wait
    Try

      clsDataCorrections.RicalcolaCanaliQualita()

      ' i periodi gia' creati restano quelli che sono: le nuove soglie valgono dal prossimo
      ' Auto Periods, e ridisegnarli qui azzererebbe solo la selezione corrente
      RidisegnaPlot(False)

    Catch ex As Exception
      MsgBox("Error while recalculating the quality channels:" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Quality Channels")
    Finally
      Input.Mouse.OverrideCursor = Cursore
    End Try
  End Sub

#End Region

  Private Sub TabItem_GotFocus(sender As Object, e As RoutedEventArgs)
    'Stop
  End Sub

  Private Sub btn_Curr1stSegm_Click(sender As Object, e As RoutedEventArgs)
    CurrentCalibration.AddSegment(DataPlotSync.VisibleRange, True)
  End Sub

  Private Sub btn_Curr2ndSegm_Click(sender As Object, e As RoutedEventArgs)
    CurrentCalibration.AddSegment(DataPlotSync.VisibleRange, False)
  End Sub

  Private Sub btn_CurrRes_Click(sender As Object, e As RoutedEventArgs)
    Dim s As String = CurrentCalibration.GetResult()
    Clipboard.SetText(s)
    MsgBox(s & vbCrLf & "Result copied to clipboard", vbOKOnly)
  End Sub

  Private Sub btn_CurrAnalyzer_Click(sender As Object, e As RoutedEventArgs)
    Dim NC As New NavChannels
    NC.SetChannels(DataPlotSync.VisibleRange.IdRigaIniziale, DataPlotSync.VisibleRange.IdRigaFinale)

    ' una sola finestra: se e' gia' aperta la sostituisco (i canali sono quelli del range attuale)
    If _finestraCorrente IsNot Nothing Then _finestraCorrente.Close()
    _finestraCorrente = New ManeuverCurrentWindow(NC, DataPlotSync.VisibleRange.Start, DataPlotSync.VisibleRange.Finish)
    _finestraCorrente.Owner = Me
    _finestraCorrente.Show()
  End Sub

  ' finestra "Long Range Auto Check": tiene una copia dei canali, va chiusa al cambio dataset
  Private _finestraCorrente As ManeuverCurrentWindow

  Private Sub CrossoversAnalysis_GotFocus(sender As Object, e As RoutedEventArgs) Handles CrossoversAnalysis.GotFocus
    AssicuraCtrlXover()
    If IsInDesignMode Then Exit Sub
  End Sub

  Private Sub XYplot_GotFocus(sender As Object, e As RoutedEventArgs) Handles XYplot.GotFocus
    Using clsLogTempi.Misura("XYplot_GotFocus: AssicuraCtrlXYplot (creazione controllo se prima volta)")
      AssicuraCtrlXYplot()
    End Using
    If IsInDesignMode Then Exit Sub
    ' anticipa la preparazione dei canali (SailingState compreso): si sovrappone al tempo in cui l'utente guarda
    ' il tab, invece di partire al primo clic nel controllo
    Dim Ctrl As UserControlSciChartXYplot = ctrlXYplot
    Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
        Sub()
          If Ctrl Is ctrlXYplot AndAlso MainTabControl.SelectedItem Is XYplot Then Ctrl.PreparaListeCanali()
        End Sub)
  End Sub

  Private Sub PolarChecks_GotFocus(sender As Object, e As RoutedEventArgs) Handles PolarChecks.GotFocus
    AssicuraCtrlPolars()
    If IsInDesignMode Then Exit Sub
  End Sub

  Private Sub Benchmarks_GotFocus(sender As Object, e As RoutedEventArgs) Handles Benchmarks.GotFocus
    AssicuraCtrlBenchmarks()
    If IsInDesignMode Then Exit Sub
  End Sub





  'Private Sub XYplot_GotFocus(sender As Object, e As RoutedEventArgs) Handles XYplot.GotFocus
  '  VerificaImpostaListe
  'End Sub
End Class