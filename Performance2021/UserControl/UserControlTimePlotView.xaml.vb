Imports System.ComponentModel
Imports System.Windows.Controls.Primitives
Imports SPwpf
Imports System.Collections.ObjectModel
Imports SciChart.Charting.Model
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Annotations
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports PropertyChanged
Imports SciChart.Charting.Visuals.Axes
Imports System.Windows.Threading

Public Class UserControlTimePlotView
  Public Property VM As clsTimePlotViewModel

  ' Costruttore senza parametri: richiesto per l'uso in DataTemplate (virtualizzazione).
  ' Il VM arriva dal DataContext assegnato dal contenitore.
  Public Sub New()
    InitializeComponent()
    AddHandler Me.DataContextChanged, AddressOf UserControlTimePlotView_DataContextChanged
    AddHandler Me.Loaded, AddressOf UserControlTimePlotView_Loaded
    AddHandler Me.Unloaded, AddressOf UserControlTimePlotView_Unloaded
  End Sub

  ' Costruttore storico mantenuto per compatibilita' con eventuali usi diretti
  Public Sub New(VM As clsTimePlotViewModel)
    InitializeComponent()
    AddHandler Me.DataContextChanged, AddressOf UserControlTimePlotView_DataContextChanged
    AddHandler Me.Loaded, AddressOf UserControlTimePlotView_Loaded
    AddHandler Me.Unloaded, AddressOf UserControlTimePlotView_Unloaded
    Me.VM = VM
    Me.DataContext = Me.VM
  End Sub

  Private Sub UserControlTimePlotView_DataContextChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
    ' con il container recycling il DataContext puo' cambiare a runtime
    Dim VmPrecedente As clsTimePlotViewModel = VM
    If Not VmPrecedente Is Nothing Then
      If VmPrecedente.Surface Is SCsurface Then VmPrecedente.Surface = Nothing
    End If
    VM = TryCast(Me.DataContext, clsTimePlotViewModel)
    If VM Is Nothing Then Exit Sub
    ' Gli assi vivono nel VM e sono in binding su XAxes/YAxes. Se questo VM era
    ' mostrato da un'altra vista (spostamento nella lista, rigenerazione del
    ' contenitore per virtualizzazione) gli assi sono ancora figli logici della
    ' superficie vecchia: vanno staccati PRIMA che il binding li consegni a questa,
    ' altrimenti SciChart li inserisce nel suo pannello e WPF lancia
    ' "Element already has a logical parent".
    StaccaAssiDaAltreSuperfici(VM)
    If Me.IsLoaded Then AgganciaVm()
  End Sub

  ''' <summary>
  ''' Stacca gli assi del VM dal genitore logico se appartengono a una superficie
  ''' diversa da SCsurface. Gli assi gia' agganciati a questa superficie non vengono
  ''' toccati: se il binding non cambia valore SciChart non li reinserirebbe e
  ''' sparirebbero dal grafico.
  ''' </summary>
  Private Sub StaccaAssiDaAltreSuperfici(VmDaAgganciare As clsTimePlotViewModel)
    If VmDaAgganciare Is Nothing Then Exit Sub
    StaccaAssi(VmDaAgganciare.Xassi)
    StaccaAssi(VmDaAgganciare.Yassi)
  End Sub

  Private Sub StaccaAssi(Assi As AxisCollection)
    If Assi Is Nothing Then Exit Sub
    For Each Asse As IAxis In Assi.ToList()
      If Asse Is Nothing Then Continue For
      If Asse.ParentSurface Is SCsurface Then Continue For
      Dim d As DependencyObject = TryCast(Asse, DependencyObject)
      If d Is Nothing Then Continue For
      Dim Genitore As DependencyObject = LogicalTreeHelper.GetParent(d)
      If Genitore Is Nothing Then Continue For
      Try
        If TypeOf Genitore Is ItemsControl Then
          Dim ic As ItemsControl = DirectCast(Genitore, ItemsControl)
          If ic.ItemsSource Is Nothing Then ic.Items.Remove(d)
        ElseIf TypeOf Genitore Is Panel Then
          Dim pnl As Panel = DirectCast(Genitore, Panel)
          Dim ui As UIElement = TryCast(d, UIElement)
          If Not ui Is Nothing Then pnl.Children.Remove(ui)
        ElseIf TypeOf Genitore Is ContentControl Then
          Dim cc As ContentControl = DirectCast(Genitore, ContentControl)
          If cc.Content Is d Then cc.Content = Nothing
        End If
      Catch ex As Exception
        ' la superficie vecchia e' in dismissione: un distacco fallito non deve
        ' impedire di mostrare il grafico nella vista nuova
        Console.WriteLine("StaccaAssi: " & ex.Message)
      End Try
    Next
  End Sub

  Private Sub UserControlTimePlotView_Loaded(sender As Object, e As RoutedEventArgs)
    AgganciaVm()
  End Sub

  Private Sub UserControlTimePlotView_Unloaded(sender As Object, e As RoutedEventArgs)
    ' il controllo esce dalla viewport: si sgancia dal VM ma il VM (e i dati) restano vivi
    If VM Is Nothing Then Exit Sub
    If VM.Surface Is SCsurface Then VM.Surface = Nothing
  End Sub

  ''' <summary>
  ''' Collega la superficie al VM e programma il caricamento dati se non ancora fatto.
  ''' Il caricamento NON e' sincrono: verrebbe eseguito dentro il ciclo di layout e
  ''' bloccherebbe la scrollbar. Viene accodato a priorita' Background, cosi' il frame
  ''' corrente si completa subito e nel frattempo si vede il segnaposto con i nomi canale.
  ''' </summary>
  Private Sub AgganciaVm()
    If VM Is Nothing Then Exit Sub
    VM.Surface = SCsurface
    If VM.DatiCaricati Then Exit Sub
    If _CaricamentoProgrammato Then Exit Sub
    _CaricamentoProgrammato = True

    Dim VmDaCaricare As clsTimePlotViewModel = VM
    VmDaCaricare.TestoStatoCaricamento = "Loading..."

    Dispatcher.BeginInvoke(DispatcherPriority.Background,
        Sub()
          _CaricamentoProgrammato = False
          ' il controllo potrebbe essere uscito dalla viewport o essere stato riciclato
          If Not VmDaCaricare Is VM Then Return
          Try
            ' fire-and-forget: il calcolo pesante gira in background e la UI
            ' non resta bloccata mentre si scorre
            Dim Ignorato = VmDaCaricare.CaricaSeNecessarioAsync()
          Catch ex As Exception
            VmDaCaricare.TestoStatoCaricamento = "Error: " & ex.Message
          End Try
        End Sub)
  End Sub

  Private _CaricamentoProgrammato As Boolean = False

  ''' <summary>
  ''' ZoomExtentsY protetto: con la virtualizzazione gli handler dei controlli possono scattare
  ''' mentre il DataTemplate si aggancia, quando il VM non ha ancora caricato gli assi.
  ''' </summary>
  Private Sub ZoomYSicuro()
    If VM Is Nothing Then Exit Sub
    If Not VM.DatiCaricati Then Exit Sub
    If SCsurface Is Nothing Then Exit Sub
    If SCsurface.YAxes Is Nothing Then Exit Sub
    If SCsurface.YAxes.Count = 0 Then Exit Sub
    If SCsurface.YAxes.First Is Nothing Then Exit Sub
    VM.ZoomExtentsYConRange()
  End Sub

  ''' <summary>ZoomExtents protetto, stessa logica di ZoomYSicuro.</summary>
  Private Sub ZoomExtentsSicuro()
    If VM Is Nothing Then Exit Sub
    If Not VM.DatiCaricati Then Exit Sub
    If SCsurface Is Nothing Then Exit Sub
    If SCsurface.YAxes Is Nothing Then Exit Sub
    If SCsurface.YAxes.Count = 0 Then Exit Sub
    If SCsurface.XAxes Is Nothing Then Exit Sub
    If SCsurface.XAxes.Count = 0 Then Exit Sub
    SCsurface.ZoomExtents()
    If VM.HaVincoloY Then VM.ApplicaRangeY()
  End Sub

  Public Enum eTipoOperazione
    eMoveMeUp = 0
    eMoveMeDn = 1
    eMoveMeToTheTop = 2
    eMoveMeToTheBottom = 3
    eRemoveMe = 4
  End Enum

  Private Sub Btn_180mode_Click(sender As Object, e As RoutedEventArgs)
    ZoomYSicuro()
  End Sub

  Private Sub SplitGrafici_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles SplitGrafici.DragCompleted
    Dim Delta As Double = e.VerticalChange
    Dim AltezzaCorrente As Double = GrigliaGrafico.Height
    If Double.IsNaN(AltezzaCorrente) Then
      AltezzaCorrente = GrigliaGrafico.ActualHeight
    End If
    AltezzaCorrente += Delta
    If AltezzaCorrente < 5 Then AltezzaCorrente = 5
    ' l'altezza va memorizzata nel VM: con la virtualizzazione il controllo viene distrutto
    ' quando esce dalla viewport e ricreato al rientro
    If Not VM Is Nothing Then
      VM.PlotHeight = AltezzaCorrente
    Else
      GrigliaGrafico.Height = AltezzaCorrente
    End If
  End Sub


  Private Sub SplitDati_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles SplitDati.DragCompleted
    Dim Delta As Double = e.VerticalChange
    Dim vm = DirectCast(ControlloDataDisplay.DataContext, clsTimePlotViewModel)
    vm.DataBoxHeight += Delta
  End Sub


  Private Sub Btn_MoveUp_Click(sender As Object, e As RoutedEventArgs) Handles btn_MoveUp.Click
    OperazioniSuListaControlli(eTipoOperazione.eMoveMeUp)
  End Sub

  Private Sub Btn_MoveDn_Click(sender As Object, e As RoutedEventArgs) Handles btn_MoveDn.Click
    OperazioniSuListaControlli(eTipoOperazione.eMoveMeDn)
  End Sub

  Private Sub Btn_Del_Click(sender As Object, e As RoutedEventArgs) Handles btn_Del.Click
    If MsgBox("Do you really want to DELETE current chart?", MsgBoxStyle.YesNo, "Chart Manager") = MsgBoxResult.Yes Then
      OperazioniSuListaControlli(eTipoOperazione.eRemoveMe)
    End If
  End Sub

  Private Sub OperazioniSuListaControlli(Operazione As UserControlTimePlotView.eTipoOperazione)
    If VM Is Nothing Then Exit Sub
    Dim VmCorrente As clsTimePlotViewModel = VM
    Dim Lista As ObservableCollection(Of clsTimePlotViewModel) = VmCorrente.ListaParent.MatriceControlli
    Dim IndiceCorrente As Integer = IndiceControlloNellaLista(Lista, VmCorrente)
    If IndiceCorrente < 0 Then Exit Sub
    ' Gli spostamenti usano Move: una sola notifica Move invece di Remove + Add,
    ' cosi' la ListBox non distrugge e ricrea la vista del grafico spostato.
    Select Case Operazione
      Case UserControlTimePlotView.eTipoOperazione.eMoveMeUp
        If IndiceCorrente > 0 Then
          Lista.Move(IndiceCorrente, IndiceCorrente - 1)
        End If
      Case UserControlTimePlotView.eTipoOperazione.eMoveMeDn
        If IndiceCorrente < Lista.Count - 1 Then
          Lista.Move(IndiceCorrente, IndiceCorrente + 1)
        End If
      Case UserControlTimePlotView.eTipoOperazione.eMoveMeToTheTop
        If IndiceCorrente > 0 Then
          Lista.Move(IndiceCorrente, 0)
        End If
      Case UserControlTimePlotView.eTipoOperazione.eMoveMeToTheBottom
        If IndiceCorrente < Lista.Count - 1 Then
          Lista.Move(IndiceCorrente, Lista.Count - 1)
        End If
      Case UserControlTimePlotView.eTipoOperazione.eRemoveMe
        Lista.RemoveAt(IndiceCorrente)
        'AppConfig.ActiveProfile.BasicChartSettings.Remove(VM.Settings)
    End Select
    AppConfig.ActiveProfile.BasicChartSettings.Clear()
    For Each c In Lista
      AppConfig.ActiveProfile.BasicChartSettings.Add(c.Settings)
    Next
    AppConfig.Salva()
    ' il primo e l'ultimo plot sono cambiati: vanno riallineati gli assi X visibili
    For Each c In Lista
      c.AggiornaVisibilitaAsseX()
    Next
  End Sub

  Private Function IndiceControlloNellaLista(Lista As ObservableCollection(Of clsTimePlotViewModel), VmCorrente As clsTimePlotViewModel) As Integer
    For i As Integer = 0 To Lista.Count - 1
      If Lista(i) Is VmCorrente Then Return i
    Next
    Return -1
  End Function



  Private Sub Lbl_Descrizione_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Dim lbl = DirectCast(sender, Label)
    Dim channel = DirectCast(lbl.DataContext, clsChannel2020)
    'channel.ToggleNomi()
    Stop
  End Sub

  Private Sub SCsurface_MouseMove(sender As Object, e As MouseEventArgs)
    If VM.ObjChartSyncManager.VisibleRange Is Nothing Then Exit Sub
    Dim Point As System.Windows.Point = e.GetPosition(e.Source)
    Dim TR As clsTimeRange = VM.ObjChartSyncManager.VisibleRange
    Dim scs As SciChart.Charting.Visuals.SciChartSurface = DirectCast(sender, SciChart.Charting.Visuals.SciChartSurface)
    Dim Dx As Double = Point.X / scs.RenderSurface.ActualWidth
    Dim MillsFromStart As Double = TR.Durata.TotalMilliseconds * Dx
    Dim Momento As DateTime = TR.Start.AddMilliseconds(MillsFromStart)
    GraficoEventiViewModel.CurrentPosition = Momento
    VM.ObjChartSyncManager.CurrentPosition = Momento
    MapControl.AggiornaPosizioneCorrente(Momento)
    'PedestalControl.AggiornaPosizioneCorrente(Momento)
  End Sub

  Private Sub btn_SaveToClipboard_Click(sender As Object, e As RoutedEventArgs) Handles btn_SaveToClipboard.Click
    Dim ImgTmp = SCsurface.ExportToBitmapSource()
    Clipboard.SetImage(ImgTmp)
    MsgBox("Chart Copied to Clipboard")
  End Sub

  Private Sub btn_ZoomExtents_Click(sender As Object, e As RoutedEventArgs)
    ZoomExtentsSicuro()
  End Sub

  Private Sub btn_WwdLwd_Click(sender As Object, e As RoutedEventArgs)
    ZoomYSicuro()
  End Sub

  Private Sub btn_MoveTop_Click(sender As Object, e As RoutedEventArgs)
    OperazioniSuListaControlli(eTipoOperazione.eMoveMeToTheTop)
  End Sub

  Private Sub btn_MoveBottom_Click(sender As Object, e As RoutedEventArgs)
    OperazioniSuListaControlli(eTipoOperazione.eMoveMeToTheBottom)
  End Sub

  Private Sub btn_ToggleYaxes_Click(sender As Object, e As RoutedEventArgs)
    ZoomYSicuro()
  End Sub

  Private Sub btn_Derivative_Click(sender As Object, e As RoutedEventArgs)
    ZoomYSicuro()
  End Sub

  Private Sub btn_ShowTgt_Click(sender As Object, e As RoutedEventArgs)
    ZoomYSicuro()
  End Sub

  Private Sub btn_ShowBack_Click(sender As Object, e As RoutedEventArgs)
    'Dim a = DirectCast(Me.DataContext, clsTimePlotViewModel)
    'a.SelectedLineType.LineType = clsGroupLines.eLineType.eDataTypeSignedDeriv
  End Sub

  Private Sub ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    ZoomYSicuro()
  End Sub

  'Private Sub SCsurface_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseUp
  '  VM.ObjChartSyncManager.SetCurrentVisibleRange()
  'End Sub

  'Private Sub SCsurface_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseDoubleClick
  '  VM.ObjChartSyncManager.SetCurrentVisibleRange()
  'End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsLineType
  'Implements INotifyPropertyChanged
  Public Property LineType As clsGroupLines.eLineType
  Public Property Descrizione As String

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  'Public Property LineType As clsGroupLines.eLineType
  '  Get
  '    Return _LineType
  '  End Get
  '  Set(value As clsGroupLines.eLineType)
  '    _LineType = value
  '    OnPropertyChanged("LineType")
  '  End Set
  'End Property

  'Public Property Descrizione As String
  '  Get
  '    Return _Descrizione
  '  End Get
  '  Set(value As String)
  '    _Descrizione = value
  '    OnPropertyChanged("Descrizione")
  '  End Set
  'End Property

  Public Sub New(LineType As clsGroupLines.eLineType)
    Me.LineType = LineType
    Descrizione = _LineType.ToString.TrimStart("e")
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsTimePlotViewModel
  'Implements INotifyPropertyChanged

  Public WithEvents ObjChartSyncManager As clsChartSyncManager

  Public Property ListaParent As clsMatriceControlliSinglePeriod

  ''' <summary>True quando i dati del plot sono gia' stati caricati (LoadTimePlotData eseguita).</summary>
  Public Property DatiCaricati As Boolean = False

  ''' <summary>Altezza scelta dall'utente per questo plot. NaN = automatica.</summary>
  Public Property PlotHeight As Double = Double.NaN

  ''' <summary>Visibilita' del segnaposto mostrato finche' i dati non sono pronti.</summary>
  Public Property PlaceholderVisibile As Windows.Visibility = Windows.Visibility.Visible

  ''' <summary>Testo di stato del segnaposto.</summary>
  Public Property TestoStatoCaricamento As String = "Waiting..."

  ''' <summary>
  ''' Nomi dei canali del plot, disponibili subito dai settings senza leggere i parquet.
  ''' </summary>
  Public ReadOnly Property NomiCanali As String
    Get
      If Settings Is Nothing Then Return ""
      If Settings.YaxisChannels Is Nothing Then Return ""
      Return String.Join(" - ", Settings.YaxisChannels)
    End Get
  End Property

  ''' <summary>
  ''' Superficie SciChart associata, valorizzata dal UserControl quando entra in viewport
  ''' e azzerata quando ne esce. Nothing significa "plot non realizzato".
  ''' </summary>
  <DoNotNotify> Public Property Surface As SciChart.Charting.Visuals.SciChartSurface

  Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
  Public Property Annotazioni As SciChart.Charting.Visuals.Annotations.AnnotationCollection

  Public Property Yassi As AxisCollection
  Public Property Xassi As AxisCollection
  Public Property YassePerc As SciChart.Charting.Visuals.Axes.NumericAxis
  Public Property YassiOrg As AxisCollection
  Public Property YasseTitle As String
  Public Property CanaleAscissa As clsChannel2020
  Public Property CanaliOrdinata As ObservableCollection(Of clsChannel2020)

  Public Property ListaLineType As New ObservableCollection(Of clsLineType)

  Public Property GroupLines As New List(Of clsGroupLines)

  Public Property DataBoxHeight As Double = 70

  Public Property CmdSelezionaCanale As New clsComando(AddressOf SelezionaCanaliGrafico)

  Public Property AggiornaSelezione As Boolean

  Public Property Settings As clsBasicChartSettings

  Public Enum eGruppoAnnotazione
    eSfondo = 0
    eGruppo1 = 1
    eGruppo2 = 2
    eNothing = 3
  End Enum


  Public Property CommonYaxis As Boolean
    Get
      Return Settings.CommonYAxis
    End Get
    Set(value As Boolean)
      If Not value = Settings.CommonYAxis Then
        Settings.CommonYAxis = value
        UpdateCommonYaxis()
        'UpdateSeriesSource()
        AppConfig.Salva()
      End If
    End Set
  End Property

  ' ---- vincoli sull'asse Y: minimo e massimo indipendenti ----
  ' Si applicano al caricamento e dopo gli zoom automatici (ZoomExtentsY); spostando o zoomando a mano non vengono imposti.

  Public ReadOnly Property HaVincoloY As Boolean
    Get
      Return Settings.YMinEnabled OrElse Settings.YMaxEnabled
    End Get
  End Property

  Private Function RangeVisibileCorrente() As DoubleRange
    If YassiOrg Is Nothing OrElse YassiOrg.Count = 0 Then Return Nothing
    Return TryCast(YassiOrg.First.VisibleRange, DoubleRange)
  End Function

  Public Property YMinEnabled As Boolean
    Get
      Return Settings.YMinEnabled
    End Get
    Set(value As Boolean)
      If Not value = Settings.YMinEnabled Then
        Settings.YMinEnabled = value
        ' all'attivazione si parte dal minimo visibile in questo momento
        Dim R As DoubleRange = RangeVisibileCorrente()
        If value AndAlso Not R Is Nothing Then YMin = Math.Round(CDbl(R.Min), 2)
        ApplicaRangeY()
        AppConfig.Salva()
      End If
    End Set
  End Property

  Public Property YMaxEnabled As Boolean
    Get
      Return Settings.YMaxEnabled
    End Get
    Set(value As Boolean)
      If Not value = Settings.YMaxEnabled Then
        Settings.YMaxEnabled = value
        Dim R As DoubleRange = RangeVisibileCorrente()
        If value AndAlso Not R Is Nothing Then YMax = Math.Round(CDbl(R.Max), 2)
        ApplicaRangeY()
        AppConfig.Salva()
      End If
    End Set
  End Property

  Public Property YMin As Double
    Get
      Return Settings.YMin
    End Get
    Set(value As Double)
      If Not value = Settings.YMin Then
        Settings.YMin = value
        If Settings.YMinEnabled Then ApplicaRangeY()
        AppConfig.Salva()
      End If
    End Set
  End Property

  Public Property YMax As Double
    Get
      Return Settings.YMax
    End Get
    Set(value As Double)
      If Not value = Settings.YMax Then
        Settings.YMax = value
        If Settings.YMaxEnabled Then ApplicaRangeY()
        AppConfig.Salva()
      End If
    End Set
  End Property

  ''' <summary>
  ''' Dopo uno zoom automatico sostituisce il minimo e/o il massimo vincolati sul primo asse del plot
  ''' (con asse unico e' l'unico); il lato non vincolato resta quello dello zoom automatico.
  ''' Senza vincoli l'asse torna in autorange.
  ''' </summary>
  Public Sub ApplicaRangeY()
    If YassiOrg Is Nothing OrElse YassiOrg.Count = 0 Then Exit Sub
    Dim Asse As SciChart.Charting.Visuals.Axes.IAxis = YassiOrg.First
    If Not Surface Is Nothing Then Surface.ZoomExtentsY()
    If HaVincoloY Then ImponiVincoloY()
  End Sub

  Private _InImposizioneY As Boolean = False
  Private _SpanYPrecedente As Double = Double.NaN

  Private Sub ImponiVincoloY()
    Dim R As DoubleRange = RangeVisibileCorrente()
    If R Is Nothing Then Exit Sub
    Dim Mn As Double = If(Settings.YMinEnabled, Settings.YMin, CDbl(R.Min))
    Dim Mx As Double = If(Settings.YMaxEnabled, Settings.YMax, CDbl(R.Max))
    If Not Mn < Mx Then Exit Sub
    _InImposizioneY = True
    Try
      YassiOrg.First.VisibleRange = New DoubleRange(Mn, Mx)
      _SpanYPrecedente = Mx - Mn
    Finally
      _InImposizioneY = False
    End Try
  End Sub

  ''' <summary>
  ''' Qualunque cambio del range Y che modifica l'ampiezza (zoom con il tasto destro, rotella, zoom extents)
  ''' rispetta i vincoli; uno spostamento a mano (ampiezza invariata) non viene toccato.
  ''' </summary>
  Private Sub AsseY_VisibleRangeChanged(sender As Object, e As SciChart.Charting.Visuals.Events.VisibleRangeChangedEventArgs)
    If _InImposizioneY Then Exit Sub
    Dim R As DoubleRange = RangeVisibileCorrente()
    If R Is Nothing Then Exit Sub
    Dim Span As Double = CDbl(R.Max) - CDbl(R.Min)
    Dim Zoom As Boolean = Double.IsNaN(_SpanYPrecedente) OrElse Math.Abs(Span - _SpanYPrecedente) > 0.000001 * Math.Max(1, Math.Abs(Span))
    _SpanYPrecedente = Span
    If Zoom AndAlso HaVincoloY Then ImponiVincoloY()
  End Sub

  ''' <summary>ZoomExtentsY che mantiene i vincoli impostati.</summary>
  Public Sub ZoomExtentsYConRange()
    If Surface Is Nothing Then Exit Sub
    Surface.ZoomExtentsY()
    If HaVincoloY Then ImponiVincoloY()
  End Sub

  Public Property SelectedLineType As clsLineType
    Get
      Return ListaLineType.Where(Function(x) x.LineType = Settings.LineType).FirstOrDefault
    End Get
    Set(value As clsLineType)
      If Not value.LineType = Settings.LineType Then
        Settings.LineType = value.LineType
        ToggleSeriesSource()
        AppConfig.Salva()
      End If
    End Set
  End Property

  Public Property ShowPortStbdBackGround As Boolean
    Get
      Return Settings.ShowPortStbdBackground
    End Get
    Set(value As Boolean)
      If Not value = Settings.ShowPortStbdBackground Then
        Settings.ShowPortStbdBackground = value
        CambiaStatoSfondo()
        AppConfig.Salva()
      End If
    End Set
  End Property

  Public Property ShowTargetIfAvailable As Boolean
    Get
      Return Settings.ShowTargetIfAvailable
    End Get
    Set(value As Boolean)
      If Not value = Settings.ShowTargetIfAvailable Then
        Settings.ShowTargetIfAvailable = value
        UpdateSeriesSource()
        AppConfig.Salva()
      End If
    End Set
  End Property


  'Public Sub New(ObjChartSyncManager As clsChartSyncManager, ListaParent As clsMatriceControlliSinglePeriod) ', VisualizzazioneGroupBy As eVisualizzazioneGroupBy, GruppoGrafici As eGruppoGrafici)
  '  Me.ObjChartSyncManager = ObjChartSyncManager
  '  Me.ListaParent = ListaParent
  '  SeriesSource = New ObservableCollection(Of IChartSeriesViewModel)
  '  For Each v In [Enum].GetValues(GetType(clsGroupLines.eLineType))
  '    _ListaLineType.Add(New clsLineType(v))
  '  Next
  '  SelectedLineType = _ListaLineType.Where(Function(x) x.LineType = clsGroupLines.eLineType.eDataTypeSigned).FirstOrDefault

  'End Sub

  Public Sub New(ObjChartSyncManager As clsChartSyncManager, ListaParent As clsMatriceControlliSinglePeriod, Settings As clsBasicChartSettings)
    Me.ObjChartSyncManager = ObjChartSyncManager
    Me.ListaParent = ListaParent
    SeriesSource = New ObservableCollection(Of IChartSeriesViewModel)
    For Each v In [Enum].GetValues(GetType(clsGroupLines.eLineType))
      _ListaLineType.Add(New clsLineType(v))
    Next
    Me.Settings = Settings
    Me.ShowPortStbdBackGround = Settings.ShowPortStbdBackground
    Me.ShowTargetIfAvailable = Settings.ShowTargetIfAvailable
    Me.CommonYaxis = Settings.CommonYAxis
    Me.SelectedLineType = _ListaLineType.Where(Function(x) x.LineType = Settings.LineType).FirstOrDefault
  End Sub

#Region "Calcolo serie fuori dal thread UI"

  ''' <summary>
  ''' Serie gia' calcolate, indicizzate per canale e tipo di linea.
  ''' Riempita dal calcolo in background, consumata da LoadTimePlotData sul thread UI.
  ''' </summary>
  Private _CacheSerie As New Dictionary(Of String, clsSerieCalcolata)

  Private Shared Function ChiaveSerie(Canale As clsChannel2020, LineType As clsGroupLines.eLineType) As String
    Return Canale.ChannelId & "|" & CInt(LineType).ToString()
  End Function

  ''' <summary>
  ''' Restituisce la serie dalla cache se presente, altrimenti la calcola subito.
  ''' Cosi' LoadTimePlotData funziona identica sia dopo il precalcolo sia senza.
  ''' </summary>
  Private Function PrendiSerie(Canale As clsChannel2020, Omologo As clsChannel2020,
                               TimeRangeToShow As clsTimeRange, LineType As clsGroupLines.eLineType,
                               timeStamps As IList(Of DateTime), timeStampsCount As Integer) As clsSerieCalcolata
    Dim Chiave As String = ChiaveSerie(Canale, LineType)
    Dim Serie As clsSerieCalcolata = Nothing
    SyncLock _CacheSerie
      If _CacheSerie.TryGetValue(Chiave, Serie) Then Return Serie
    End SyncLock
    Serie = CalcolaSerieCanale(Canale, Omologo, TimeRangeToShow, LineType, timeStamps, timeStampsCount)
    If Serie IsNot Nothing Then
      SyncLock _CacheSerie
        _CacheSerie(Chiave) = Serie
      End SyncLock
    End If
    Return Serie
  End Function

  Private Class clsRichiestaCalcolo
    Public Canale As clsChannel2020
    Public Omologo As clsChannel2020
    Public Chiave As String
  End Class

  ''' <summary>
  ''' Fase da eseguire SUL THREAD UI: risolve i canali e forza tutto cio' che e' lazy
  ''' (lettura parquet, canali target, array TwaModes, canale di mura). Dopo questa fase
  ''' il calcolo puo' girare in background leggendo soltanto array gia' popolati.
  ''' </summary>
  Private Function PreparaRichieste(CS As List(Of clsChannel2020)) As List(Of clsRichiestaCalcolo)
    Dim Richieste As New List(Of clsRichiestaCalcolo)
    If CS Is Nothing Then Return Richieste

    ' forza le dipendenze lazy del DataProvider
    CanaleTackDefault()
    Dim ModiTmp As Byte() = DataProvider2020.TwaModes
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)

    For Each Canale In CS
      If Canale Is Nothing Then Continue For
      Dim Caricato As clsChannel2020 = DataProvider2020.CanaleDbl(Canale.ChannelId)
      If Caricato Is Nothing Then Continue For
      If Caricato.Valori Is Nothing OrElse Caricato.Valori.Count = 0 Then Continue For

      ' omologo Port/Stbd: va risolto qui perche' puo' innescare una lettura da parquet
      Dim Omologo As clsChannel2020 = Caricato
      If Caricato.ChannelId.IndexOf("Port") > -1 Then
        Omologo = DataProvider2020.CanaleDbl(Caricato.ChannelId.Replace("Port", "Stbd"))
        If Omologo Is Nothing Then Omologo = Caricato
      ElseIf Caricato.ChannelId.IndexOf("Stbd") > -1 Then
        Omologo = DataProvider2020.CanaleDbl(Caricato.ChannelId.Replace("Stbd", "Port"))
        If Omologo Is Nothing Then Omologo = Caricato
      End If

      ' canale target: anche questo puo' creare canali e leggere dati
      If Not TgtManager Is Nothing AndAlso Not TgtManager.Tgt Is Nothing _
         AndAlso Not chTwa Is Nothing AndAlso Not chTws Is Nothing Then
        Try
          Dim TgtIgnorato As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(Caricato)
        Catch ex As Exception
        End Try
      End If

      Dim R As New clsRichiestaCalcolo
      R.Canale = Caricato
      R.Omologo = Omologo
      R.Chiave = ChiaveSerie(Caricato, SelectedLineType.LineType)
      Richieste.Add(R)
    Next
    Return Richieste
  End Function

  ''' <summary>
  ''' Fase eseguibile SU UN THREAD QUALSIASI: solo matematica su array gia' pronti.
  ''' </summary>
  Private Sub EseguiCalcoli(Richieste As List(Of clsRichiestaCalcolo), TimeRangeToShow As clsTimeRange,
                            LineType As clsGroupLines.eLineType,
                            timeStamps As IList(Of DateTime), timeStampsCount As Integer)
    For Each R In Richieste
      Dim GiaFatta As Boolean = False
      SyncLock _CacheSerie
        GiaFatta = _CacheSerie.ContainsKey(R.Chiave)
      End SyncLock
      If GiaFatta Then Continue For

      Dim Serie As clsSerieCalcolata = CalcolaSerieCanale(R.Canale, R.Omologo, TimeRangeToShow,
                                                          LineType, timeStamps, timeStampsCount)
      If Serie Is Nothing Then Continue For
      SyncLock _CacheSerie
        _CacheSerie(R.Chiave) = Serie
      End SyncLock
    Next
  End Sub

  ''' <summary>
  ''' False quando il tab Data Plots non e' quello visibile: il caricamento dei plot (che gira per buona parte sul
  ''' thread UI) viene rinviato, cosi' non rallenta il tab su cui si sta lavorando. Lo imposta MainWindow
  ''' al cambio di tab e, tornando su Data Plots, rilancia il riempimento dei plot rimasti da caricare.
  ''' </summary>
  Public Shared Property CaricamentoConsentito As Boolean = True

  ''' <summary>
  ''' Un solo plot alla volta: prima i plot visibili ne avviavano cinque insieme, le cui parti sul thread UI si
  ''' intrecciavano (ognuno piu' lento) e, una volta partiti, non si potevano fermare al cambio di tab.
  ''' In coda, invece, i plot non ancora iniziati si fermano appena il tab Data Plots non e' piu' visibile.
  ''' </summary>
  Private Shared ReadOnly _CodaCaricamento As New Threading.SemaphoreSlim(1, 1)

  ''' <summary>
  ''' Versione asincrona di CaricaSeNecessario: il calcolo pesante gira su un thread di
  ''' background, la UI resta reattiva, e al ritorno si costruiscono assi, serie e annotazioni.
  ''' </summary>
  Public Async Function CaricaSeNecessarioAsync() As Threading.Tasks.Task(Of Boolean)
    If DatiCaricati Then Return True
    If Not CaricamentoConsentito Then Return False
    If _CalcoloInCorso Then Return False ' in coda o in corso
    If DataProvider2020 Is Nothing Then Return False
    If Not DataProvider2020.ValoriCaricati Then Return False

    _CalcoloInCorso = True
    Dim InCodaAcquisita As Boolean = False
    Dim SwPlot As Stopwatch = Nothing
    Try
      Await _CodaCaricamento.WaitAsync()
      InCodaAcquisita = True
      ' dopo l'attesa la situazione puo' essere cambiata: tab lasciato, dataset ricaricato, plot gia' caricato
      If DatiCaricati Then Return True
      If Not CaricamentoConsentito Then Return False
      If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then Return False

      SwPlot = Stopwatch.StartNew()
      clsLogTempi.Scrivi("  plot dati: inizio caricamento " & TitoloPlotLog())
      TestoStatoCaricamento = "Loading data..."

      Dim CS As List(Of clsChannel2020) = RisolviCanali()
      If CS.Count = 0 Then
        DatiCaricati = True
        TestoStatoCaricamento = "No data available"
        Return False
      End If

      ' --- thread UI: tutto cio' che e' lazy o muta stato condiviso
      Dim Richieste As List(Of clsRichiestaCalcolo) = PreparaRichieste(CS)
      Dim TimeRangeToShow As clsTimeRange = ObjChartSyncManager.VisibleRange
      Dim LineType As clsGroupLines.eLineType = SelectedLineType.LineType
      Dim timeStamps As DateTime() = DataProvider2020.TimeStamps
      Dim timeStampsCount As Integer = If(timeStamps Is Nothing, 0, timeStamps.Length)

      ' --- thread di background: solo matematica
      If Richieste.Count > 0 AndAlso timeStampsCount > 0 Then
        Await Threading.Tasks.Task.Run(
            Sub()
              EseguiCalcoli(Richieste, TimeRangeToShow, LineType, timeStamps, timeStampsCount)
            End Sub)
      End If

      ' --- di nuovo sul thread UI: costruzione di assi, serie renderizzabili e annotazioni,
      '     che consuma le serie gia' pronte in cache
      If LoadTimePlotData(False, CS) Then
        UpdateCommonYaxis()
      End If
      DatiCaricati = True
      PlaceholderVisibile = Windows.Visibility.Collapsed
      AggiornaVisibilitaAsseX()
      Return True

    Finally
      _CalcoloInCorso = False
      If InCodaAcquisita Then _CodaCaricamento.Release()
      If Not SwPlot Is Nothing Then clsLogTempi.Scrivi("  plot dati: fine caricamento " & TitoloPlotLog() & " : " & SwPlot.ElapsedMilliseconds & " ms")
    End Try
  End Function

  Private Function TitoloPlotLog() As String
    Try
      Return If(Settings Is Nothing, "", Convert.ToString(Settings.YaxisChannels.FirstOrDefault))
    Catch
      Return ""
    End Try
  End Function

  Private _CalcoloInCorso As Boolean = False

#End Region

  ''' <summary>
  ''' Sgancia il ViewModel dal sync manager globale e libera i dati.
  ''' Indispensabile al cambio di dataset: DataPlotSync e' una globale mai ricreata e,
  ''' finche' il VM resta sottoscritto tramite WithEvents, il delegato dell'evento lo tiene
  ''' in vita insieme a tutti gli array del set precedente.
  ''' </summary>
  Public Sub Scollega()
    Try
      ObjChartSyncManager = Nothing
    Catch ex As Exception
    End Try
    Try
      Surface = Nothing
      DatiCaricati = False
      SyncLock _CacheSerie
        _CacheSerie.Clear()
      End SyncLock
      If _GroupLines IsNot Nothing Then _GroupLines.Clear()
      If SeriesSource IsNot Nothing Then SeriesSource.Clear()
      If Annotazioni IsNot Nothing Then Annotazioni.Clear()
      Yassi = Nothing
      Xassi = Nothing
      YassiOrg = Nothing
      CanaliOrdinata = Nothing
    Catch ex As Exception
    End Try
  End Sub

  ''' <summary>
  ''' Risolve i canali del plot a partire da Settings.YaxisChannels.
  ''' E' qui che avviene il caricamento dei valori dal parquet (CanaleDbl e' lazy).
  ''' </summary>
  Private Function RisolviCanali() As List(Of clsChannel2020)
    Dim CS As New List(Of clsChannel2020)
    If Settings Is Nothing Then Return CS
    If Settings.YaxisChannels Is Nothing Then Return CS
    If DataProvider2020 Is Nothing Then Return CS
    For Each Hdr As String In Settings.YaxisChannels
      Dim Ch As clsChannel2020 = DataProvider2020.CanaleDbl(Hdr)
      If Not Ch Is Nothing Then CS.Add(Ch)
    Next
    Return CS
  End Function

  ''' <summary>
  ''' Carica i dati del plot solo la prima volta. Idempotente: chiamabile a ogni realizzazione
  ''' del controllo senza costi aggiuntivi.
  ''' </summary>
  Public Function CaricaSeNecessario() As Boolean
    If DatiCaricati Then Return True
    If DataProvider2020 Is Nothing Then Return False
    If Not DataProvider2020.ValoriCaricati Then Return False

    TestoStatoCaricamento = "Loading data..."

    Dim CS As List(Of clsChannel2020) = RisolviCanali()
    If CS.Count = 0 Then
      ' nessun canale valido: si marca comunque come "fatto" per non ritentare a ogni scroll
      DatiCaricati = True
      TestoStatoCaricamento = "No data available"
      Return False
    End If

    If LoadTimePlotData(False, CS) Then
      UpdateCommonYaxis()
    End If
    DatiCaricati = True
    PlaceholderVisibile = Windows.Visibility.Collapsed
    AggiornaVisibilitaAsseX()
    Return True
  End Function

  ''' <summary>
  ''' Forza il ricaricamento completo del plot (usata quando cambiano i dati sorgente).
  ''' </summary>
  Public Sub InvalidaDati()
    DatiCaricati = False
    SyncLock _CacheSerie
      _CacheSerie.Clear()
    End SyncLock
    PlaceholderVisibile = Windows.Visibility.Visible
    TestoStatoCaricamento = "Waiting..."
    _GroupLines.Clear()
    If Not SeriesSource Is Nothing Then SeriesSource.Clear()
  End Sub

  ''' <summary>
  ''' Versione pubblica di NascondiAssiXsecondari: l'asse X con etichette si mostra
  ''' solo sul primo e sull'ultimo plot della lista.
  ''' </summary>
  Public Sub AggiornaVisibilitaAsseX()
    NascondiAssiXsecondari()
  End Sub

  'Public Sub New(ObjChartSyncManager As clsChartSyncManager, ListaParent As clsMatriceControlliSinglePeriod, LineType As clsGroupLines.eLineType, ShowPortStbdBackGround As Boolean, ShowTarget As Boolean, CommonYaxis As Boolean)
  '  Me.ObjChartSyncManager = ObjChartSyncManager
  '  Me.ListaParent = ListaParent
  '  SeriesSource = New ObservableCollection(Of IChartSeriesViewModel)
  '  For Each v In [Enum].GetValues(GetType(clsGroupLines.eLineType))
  '    _ListaLineType.Add(New clsLineType(v))
  '  Next
  '  Me.ShowPortStbdBackGround = ShowPortStbdBackGround
  '  Me.ShowTargetIfAvailable = ShowTarget
  '  Me.CommonYaxis = CommonYaxis
  '  Me.SelectedLineType = _ListaLineType.Where(Function(x) x.LineType = LineType).FirstOrDefault
  '  'OnPropertyChanged("CommonYaxis")
  '  'OnPropertyChanged("ShowPortStbdBackGround")
  '  'OnPropertyChanged("ShowTargetIfAvailable")
  '  'OnPropertyChanged("SelectedLineType")
  'End Sub

  'Public Property ObjChartSyncManager As clsChartSyncManager
  '  Get
  '    Return pObjChartSyncManager
  '  End Get
  '  Set(value As clsChartSyncManager)
  '    pObjChartSyncManager = value
  '  End Set
  'End Property

  'Public Property Yassi As AxisCollection
  '  Get
  '    Return pYassi
  '  End Get
  '  Set(value As AxisCollection)
  '    If pYassi Is value Then Exit Property
  '    pYassi = value
  '    OnPropertyChanged("Yassi")
  '  End Set
  'End Property

  'Public Property Xassi As AxisCollection
  '  Get
  '    Return pXassi
  '  End Get
  '  Set(value As AxisCollection)
  '    If pXassi Is value Then Exit Property
  '    pXassi = value
  '    OnPropertyChanged("Xassi")
  '  End Set
  'End Property

  'Public Property DataBoxHeight As Double
  '  Get
  '    Return pDataBoxHeight
  '  End Get
  '  Set(value As Double)
  '    If pDataBoxHeight = value Then Exit Property
  '    pDataBoxHeight = value
  '    OnPropertyChanged("DataBoxHeight")
  '  End Set
  'End Property

  'Public Property CommonYaxis As Boolean
  '  Get
  '    Return _CommonYaxis
  '  End Get
  '  Set(value As Boolean)
  '    If _CommonYaxis = value Then Exit Property
  '    _CommonYaxis = value
  '    UpdateCommonYaxis()
  '    OnPropertyChanged("CommonYaxis")
  '    SalvaImpostazioni()
  '  End Set
  'End Property

  'Public ReadOnly Property CmdSelezionaCanale() As ICommand
  '  Get
  '    Return pCmdSelezionaCanale
  '  End Get
  'End Property

  Private Sub CambiaStatoSfondo()
    If _Annotazioni Is Nothing Then Exit Sub
    For Each Annotazione As SciChart.Charting.Visuals.Annotations.IAnnotation In _Annotazioni
      If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
        If DirectCast(DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation).Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSfondo Then
          Annotazione.IsHidden = Not ShowPortStbdBackGround '(ShowAbsolute OrElse ShowWindwardLeeward OrElse ShowDerivative)
        End If
      End If
    Next
  End Sub

  'Public Property Annotazioni As SciChart.Charting.Visuals.Annotations.AnnotationCollection
  '  Get
  '    Return _Annotazioni ' .Union(vv)
  '  End Get
  '  Set(value As SciChart.Charting.Visuals.Annotations.AnnotationCollection)
  '    If _Annotazioni Is value Then Exit Property
  '    _Annotazioni = value
  '    OnPropertyChanged("Annotazioni")
  '  End Set
  'End Property

  'Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
  '  Get
  '    Return pSeriesSource
  '  End Get
  '  Set(value As ObservableCollection(Of IChartSeriesViewModel))
  '    If pSeriesSource Is value Then Exit Property
  '    pSeriesSource = value
  '    OnPropertyChanged("SeriesSource")
  '  End Set
  'End Property

  'Public Property CanaleAscissa As clsChannel2020
  '  Get
  '    Return pCanaleAscissa
  '  End Get
  '  Set(value As clsChannel2020)
  '    pCanaleAscissa = value
  '  End Set
  'End Property

  'Public Property CanaliOrdinata As ObservableCollection(Of clsChannel2020)
  '  Get
  '    Return pCanaliOrdinata
  '  End Get
  '  Set(value As ObservableCollection(Of clsChannel2020))
  '    If pCanaliOrdinata Is value Then Exit Property
  '    pCanaliOrdinata = value
  '    OnPropertyChanged("CanaliOrdinata")
  '  End Set
  'End Property

  'Public Property ListaParent As clsMatriceControlliSinglePeriod
  '  Get
  '    Return pListaParent
  '  End Get
  '  Set(value As clsMatriceControlliSinglePeriod)
  '    pListaParent = value
  '  End Set
  'End Property

  'Public Property SelectedLineType As clsLineType ' clsGroupLines.eLineType
  '  Get
  '    Return _SelectedLineType
  '  End Get
  '  Set(value As clsLineType) ' clsGroupLines.eLineType)
  '    _SelectedLineType = value
  '    If Not Xassi Is Nothing Then
  '      ToggleSeriesSource()
  '      AggiornaDisplayValori()
  '    End If
  '    OnPropertyChanged("SelectedLineType")
  '    SalvaImpostazioni()
  '    'OnPropertyChanged("SelectedLineTypeStringa")

  '  End Set
  'End Property

  'Public ReadOnly Property SelectedLineTypeStringa As String
  '  Get
  '    Return _SelectedLineType
  '  End Get
  'End Property

  'Public Property ShowPortStbdBackGround As Boolean
  '  Get
  '    Return _ShowPortStbdBackGround
  '  End Get
  '  Set(value As Boolean)
  '    _ShowPortStbdBackGround = value
  '    CambiaStatoSfondo()
  '    OnPropertyChanged("ShowPortStbdBackGround")
  '    SalvaImpostazioni()
  '  End Set
  'End Property

  'Public Property ShowTargetIfAvailable As Boolean
  '  Get
  '    Return _ShowTargetIfAvailable
  '  End Get
  '  Set(value As Boolean)
  '    _ShowTargetIfAvailable = value
  '    UpdateSeriesSource()
  '    OnPropertyChanged("ShowTargetIfAvailable")
  '    SalvaImpostazioni()
  '  End Set
  'End Property

  'Public Property ListaLineType As ObservableCollection(Of clsLineType)
  '  Get
  '    Return _ListaLineType
  '  End Get
  '  Set(value As ObservableCollection(Of clsLineType))
  '    _ListaLineType = value
  '    OnPropertyChanged("ListaLineType")
  '  End Set
  'End Property

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Private Sub NascondiAssiXsecondari()
    If Xassi Is Nothing Then Exit Sub
    If Xassi.Count = 0 Then Exit Sub
    Dim VisualizzaAsseX As Boolean = False

    If ListaParent Is Nothing Then Exit Sub
    If ListaParent.MatriceControlli Is Nothing Then Exit Sub
    If ListaParent.MatriceControlli.Count = 0 Then Exit Sub

    If Me Is ListaParent.MatriceControlli.First Then
      VisualizzaAsseX = True
    End If
    If Not VisualizzaAsseX Then
      VisualizzaAsseX = Me Is ListaParent.MatriceControlli.Last
    End If
    Xassi.First.DrawMajorTicks = VisualizzaAsseX
    Xassi.First.DrawMinorTicks = VisualizzaAsseX
    Xassi.First.DrawLabels = VisualizzaAsseX
  End Sub

  Private Sub ToggleSeriesSource()
    ' se i dati non sono ancora stati caricati non c'e' nulla da commutare:
    ' ci pensera' CaricaSeNecessario() usando il LineType corrente
    If Not DatiCaricati Then Exit Sub

    Dim Aggiorna As Boolean = False
    For Each s In _GroupLines
      Select Case SelectedLineType.LineType
        Case clsGroupLines.eLineType.eRawValue
          If s.RawValue Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
        Case clsGroupLines.eLineType.eRawValueDeriv
          If s.RawValueDeriv Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
          'LoadTimePlotData(True, CanaliOrdinata)
          'Aggiorna = False
        Case clsGroupLines.eLineType.eDataTypeSigned
          If s.Signed Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
          'LoadTimePlotData(True, CanaliOrdinata)
          'Aggiorna = False
        Case clsGroupLines.eLineType.eDataTypeSignedDeriv
          If s.SignedDeriv Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
          If s.WwdLwd Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
          If s.WwdLwdDeriv Is Nothing Then
            'LoadTimePlotData(False, CanaliOrdinata)
            Aggiorna = True
          End If
        Case Else
          Stop
      End Select
      'If Not s.Tgt Is Nothing Then
      '  If Not s.Tgt Is Nothing Then
      '    SeriesSource.Add(s.Tgt)
      '  End If
      'End If
    Next

    If Aggiorna Then
      If LoadTimePlotData(False, CanaliOrdinata) Then

      End If
    Else
      UpdateSeriesSource()
    End If
  End Sub


  Private Sub UpdateSeriesSource()
    ' puo' essere invocata da un setter mentre il DataTemplate si aggancia,
    ' quando gli assi non sono ancora stati costruiti da LoadTimePlotData
    If SeriesSource Is Nothing Then Exit Sub
    If Xassi Is Nothing OrElse Yassi Is Nothing Then Exit Sub
    If Xassi.Count = 0 OrElse Yassi.Count = 0 Then Exit Sub

    SeriesSource.Clear()
    For Each s In _GroupLines
      Select Case SelectedLineType.LineType
        Case clsGroupLines.eLineType.eRawValue
          SeriesSource.Add(s.RawValue)
        Case clsGroupLines.eLineType.eRawValueDeriv
          SeriesSource.Add(s.RawValueDeriv)
        Case clsGroupLines.eLineType.eDataTypeSigned
          SeriesSource.Add(s.Signed)
        Case clsGroupLines.eLineType.eDataTypeSignedDeriv
          SeriesSource.Add(s.SignedDeriv)
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
          SeriesSource.Add(s.WwdLwd)
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
          SeriesSource.Add(s.WwdLwdDeriv)
      End Select
      If ShowTargetIfAvailable And s.Tgt Is Nothing Then
        If Not TgtManager.Tgt Is Nothing Then
          If TgtManager.Tgt.PolareDisponibile(s.Canale.PolarHeader) Then
            If LoadTimePlotData(True, CanaliOrdinata) Then

            End If
          End If
        End If
      ElseIf Not s.Tgt Is Nothing And ShowTargetIfAvailable Then
        SeriesSource.Add(s.Tgt)
      End If
    Next

    Dim ShowAxisFormat As Boolean = True
    For Each Asse In Xassi
      Asse.DrawMajorGridLines = True 'ShowAxisFormat
      Asse.DrawMinorGridLines = ShowAxisFormat
      If ShowAxisFormat Then
        Asse.AxisBandsFill = Color.FromArgb(5, 33, 33, 33)
      Else
        Asse.AxisBandsFill = Nothing
      End If
      Asse.DrawMajorBands = ShowAxisFormat
    Next

    For Each Asse In Yassi
      If Asse Is Yassi.First Then
        Asse.DrawMajorGridLines = True 'ShowAxisFormat
        Asse.DrawMinorGridLines = ShowAxisFormat
        If ShowAxisFormat Then
          Asse.AxisBandsFill = Color.FromArgb(5, 33, 33, 33)
        Else
          Asse.AxisBandsFill = Nothing
        End If
        Asse.DrawMajorBands = ShowAxisFormat
      Else
        Asse.DrawMajorGridLines = False 'ShowAxisFormat
        Asse.DrawMinorGridLines = False
        Asse.AxisBandsFill = Nothing
        Asse.DrawMajorBands = False
      End If
    Next

  End Sub

  Private Sub AssegnaAsse(Interfaccia As IChartSeriesViewModel, IdAsse As String)
    If Not Interfaccia Is Nothing Then
      Interfaccia.RenderSeries.YAxisId = IdAsse
    End If
  End Sub

  Public Sub UpdateCommonYaxis()
    If CanaliOrdinata.Count = 0 Then Exit Sub
    If CanaliOrdinata.Count = 1 Then
      Yassi.First.AxisTitle = NomeDaCanale(YasseTitle)
      Exit Sub
    End If
    If CommonYaxis Then
      'un solo asse
      For Each Canale In _GroupLines
        AssegnaAsse(Canale.RawValue, YassiOrg.First.Id)
        AssegnaAsse(Canale.RawValueDeriv, YassiOrg.First.Id)
        AssegnaAsse(Canale.Signed, YassiOrg.First.Id)
        AssegnaAsse(Canale.SignedDeriv, YassiOrg.First.Id)
        AssegnaAsse(Canale.WwdLwd, YassiOrg.First.Id)
        AssegnaAsse(Canale.WwdLwdDeriv, YassiOrg.First.Id)
        If Not Canale.Tgt Is Nothing Then Canale.Tgt.RenderSeries.YAxisId = YassiOrg.First.Id
      Next
      Yassi.Clear()
      Yassi.Add(YassiOrg.First)
      Yassi.First.AxisTitle = NomeDaCanali()
    Else
      'assi separati per canale
      For i As Integer = 0 To YassiOrg.Count - 1
        AssegnaAsse(_GroupLines(i).RawValue, YassiOrg(i).Id)
        AssegnaAsse(_GroupLines(i).RawValueDeriv, YassiOrg(i).Id)
        AssegnaAsse(_GroupLines(i).Signed, YassiOrg(i).Id)
        AssegnaAsse(_GroupLines(i).SignedDeriv, YassiOrg(i).Id)
        AssegnaAsse(_GroupLines(i).WwdLwd, YassiOrg(i).Id)
        AssegnaAsse(_GroupLines(i).WwdLwdDeriv, YassiOrg(i).Id)
        If Not _GroupLines(i).Tgt Is Nothing Then _GroupLines(i).Tgt.RenderSeries.YAxisId = YassiOrg(i).Id
      Next
      Yassi.Clear()
      For Each Asse In YassiOrg
        Yassi.Add(Asse)
      Next
      Yassi.First.AxisTitle = NomeDaCanale(YasseTitle)
    End If
  End Sub

  Private Function NomeDaCanale(NomeDefault As String) As String
    Dim strTMP As String = ""
    Select Case SelectedLineType.LineType
      Case clsGroupLines.eLineType.eRawValueDeriv, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
        strTMP = "Deriv of: " & NomeDefault
      Case clsGroupLines.eLineType.eDataTypeSignedDeriv
        strTMP = "Deriv of signed: " & NomeDefault & ""
      Case clsGroupLines.eLineType.eRawValue
        strTMP = NomeDefault
      Case clsGroupLines.eLineType.eDataTypeSigned
        strTMP = "Signed: " & NomeDefault & ""
      Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
        strTMP = NomeDefault.Replace("Port", "Lwd").Replace("Stbd", "Wwd")
    End Select
    Return strTMP
  End Function

  Public Function NomeDaCanali() As String

    Dim strTMP As String = ""
    Select Case SelectedLineType.LineType
      Case clsGroupLines.eLineType.eRawValueDeriv, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
        strTMP = "Deriv of: "
      Case clsGroupLines.eLineType.eDataTypeSignedDeriv
        strTMP = "Deriv of signed: "
    End Select
    For Each canale In CanaliOrdinata
      If Not canale Is Nothing Then
        Select Case SelectedLineType.LineType
          Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
            strTMP &= canale.ShortName.Replace("Port", "Lwd").Replace("Stbd", "Wwd") & "-"
          Case Else
            strTMP &= canale.ShortName & "-"
        End Select
      End If
    Next

    Return strTMP.TrimEnd("-")
  End Function


  Public Function LoadTimePlotData(Reset As Boolean, CanaliOrdinata As List(Of clsChannel2020)) As Boolean
    Return LoadTimePlotData(Reset, CanaliOrdinata, ObjChartSyncManager.VisibleRange)
  End Function

  Public Function LoadTimePlotData(Reset As Boolean, CanaliOrdinata As IEnumerable(Of clsChannel2020)) As Boolean
    Return LoadTimePlotData(Reset, CanaliOrdinata, ObjChartSyncManager.VisibleRange)
  End Function

  Private Function NomeAsse(Assi As AxisCollection, Nome As String) As String
    Dim Counter As Integer = 0
    For Each Asse In Assi
      If Asse.Id.StartsWith(Nome) Then
        Counter += 1
      End If
    Next
    If Counter = 0 Then
      Return Nome
    Else
      Return Nome & "_" & Counter
    End If
  End Function

  Private Function CanaleRipetuto(CanaliStampati As List(Of clsChannel2020), Canale As clsChannel2020) As Boolean
    'per non stampare due volte lo stesso canale...
    For Each CanaleTmp In CanaliStampati
      If Canale Is CanaleTmp Then Return True
    Next
    Return False
  End Function

  Private Enum eTwaMode
    eStbd = 0
    ePort = 1
    eHeadDeadToWind = 2
  End Enum


  ' Brush condivisi e freezati: prima ne veniva creato uno nuovo per ogni box,
  ' su 44 plot erano migliaia di oggetti WPF con relativo costo di rendering.
  Private Shared ReadOnly BrushSfondoStbd As SolidColorBrush = CreaBrushSfondo(0, 255, 0)
  Private Shared ReadOnly BrushSfondoPort As SolidColorBrush = CreaBrushSfondo(255, 0, 0)
  Private Shared ReadOnly BrushSfondoNeutro As SolidColorBrush = CreaBrushSfondo(255, 255, 255)

  Private Shared Function CreaBrushSfondo(R As Byte, G As Byte, B As Byte) As SolidColorBrush
    Dim Br As New SolidColorBrush(Color.FromArgb(20, R, G, B))
    Br.Freeze()
    Return Br
  End Function

  ''' <summary>
  ''' Crea le BoxAnnotation di sfondo dagli intervalli di mura gia' calcolati dal DataProvider,
  ''' invece di ricavarli campione per campione dentro il loop dei dati.
  ''' </summary>
  Private Sub CostruisciAnnotazioniSfondo()
    If Annotazioni Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    Dim Intervalli = DataProvider2020.IntervalliTack
    If Intervalli Is Nothing Then Exit Sub

    For Each Iv In Intervalli
      Dim AnBA As New SciChart.Charting.Visuals.Annotations.BoxAnnotation
      AnBA.X1 = Iv.X1
      AnBA.X2 = Iv.X2
      AnBA.Y1 = 0
      AnBA.Y2 = 1
      Select Case Iv.Modo
        Case clsDataProvider2020.TwaModeStbd
          AnBA.Background = BrushSfondoStbd
        Case clsDataProvider2020.TwaModePort
          AnBA.Background = BrushSfondoPort
        Case Else
          AnBA.Background = BrushSfondoNeutro
      End Select
      AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
      AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
      AnBA.IsHidden = Not ShowPortStbdBackGround
      AnBA.Tag = eGruppoAnnotazione.eSfondo
      Annotazioni.Add(AnBA)
    Next
  End Sub

  ''' <summary>
  ''' Riporta una differenza angolare nell'intervallo -180..+180.
  ''' </summary>
  Private Shared Function WrapTo180(Delta As Double) As Double
    Dim d As Double = (Delta + 180) Mod 360
    If d < 0 Then d += 360
    Return d - 180
  End Function

  ''' <summary>
  ''' Risultato del calcolo di una serie: solo array, nessun oggetto WPF.
  ''' </summary>
  Private Class clsSerieCalcolata
    Public X As DateTime()
    Public Y As Double()
    ''' <summary>Indici originali dei campioni mantenuti: servono a ricostruire il target.</summary>
    Public Indici As Integer()
    Public N As Integer
  End Class

  ''' <summary>
  ''' Calcola gli array X/Y (ed eventualmente il target) di un canale.
  ''' PURA: non tocca alcun oggetto WPF e legge soltanto dai canali gia' caricati,
  ''' quindi e' eseguibile fuori dal thread della UI.
  ''' </summary>
  Private Function CalcolaSerieCanale(CanaleOrdinataLoaded As clsChannel2020,
                                      Omologo As clsChannel2020,
                                      TimeRangeToShow As clsTimeRange,
                                      LineType As clsGroupLines.eLineType,
                                      timeStamps As IList(Of DateTime),
                                      timeStampsCount As Integer) As clsSerieCalcolata

    ' Avg viene usato ESCLUSIVAMENTE per decidere il wrap dei canali a 360 gradi.
    Dim Avg As Double = Double.NaN
    If CanaleOrdinataLoaded.DataType = clsChannel2020.eDataType.e360 Then
      Dim ivCanale As New clsStatisticheIntervallo(CanaleOrdinataLoaded, TimeRangeToShow)
      Avg = ivCanale.Avg
    End If

    Dim valori As Double() = CanaleOrdinataLoaded.Valori
    Dim nMax As Integer = Math.Min(valori.Length, timeStampsCount)
    If nMax <= 0 Then Return Nothing

    Dim ValoriX(nMax - 1) As DateTime
    Dim ValoriY(nMax - 1) As Double
    Dim Indici(nMax - 1) As Integer
    Dim nOut As Integer = 0
    Dim UltimoX As DateTime = Nothing
    Dim CurrentTwaMode As eTwaMode = eTwaMode.eHeadDeadToWind

    Dim Modi As Byte() = DataProvider2020.TwaModes
    Dim ValoriOmologo As Double() = Nothing
    If Not Omologo Is Nothing Then ValoriOmologo = Omologo.Valori

    Dim LastValidData As DateTime = timeStamps(0)
    Dim objDerivata As New clsDerivata(DataProvider2020.Hz, CanaleOrdinataLoaded.DataType = clsChannel2020.eDataType.e360, LastValidData)

    For Indice As Integer = 0 To nMax - 1
      Dim X As DateTime = timeStamps(Indice)
      Dim Y As Double = valori(Indice)

      If Double.IsNaN(Y) Then Continue For
      If X = Nothing Then Continue For
      If nOut > 0 AndAlso X <= UltimoX Then Continue For

      LastValidData = X

      If Indice < Modi.Length Then
        Select Case Modi(Indice)
          Case clsDataProvider2020.TwaModeStbd
            CurrentTwaMode = eTwaMode.eStbd
          Case clsDataProvider2020.TwaModePort
            CurrentTwaMode = eTwaMode.ePort
          Case Else
            CurrentTwaMode = eTwaMode.eHeadDeadToWind
        End Select
      End If

      ValoriX(nOut) = X
      Indici(nOut) = Indice
      UltimoX = X

      Dim yOut As Double = Double.NaN

      Select Case LineType
        Case clsGroupLines.eLineType.eRawValue, clsGroupLines.eLineType.eRawValueDeriv
          ' i canali a 360 gradi vanno srotolati anche sul valore grezzo: e' una
          ' trasformazione di sola visualizzazione (l'asse mostra comunque 0-360
          ' grazie a cls360LabelProvider) e senza di essa la linea salta da 359 a 1
          If CanaleOrdinataLoaded.DataType = clsChannel2020.eDataType.e360 Then
            Y = Avg + WrapTo180(Y - Avg)
          End If
          If LineType = clsGroupLines.eLineType.eRawValueDeriv Then
            If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
            Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
            objDerivata.MediaMobile.AggiornaMedia(Derivata)
            yOut = objDerivata.MediaMobile.Valore
            objDerivata.AggiornaValoriPrev(Y, X)
          Else
            yOut = Y
          End If

        Case clsGroupLines.eLineType.eDataTypeSigned, clsGroupLines.eLineType.eDataTypeSignedDeriv
          Select Case CanaleOrdinataLoaded.DataType
            Case clsChannel2020.eDataType.eDays
              yOut = Y * 24 * 60 * 60
            Case clsChannel2020.eDataType.e360
              ' srotolamento centrato sulla media circolare: ogni campione viene
              ' portato entro +/-180 gradi dalla media, qualunque essa sia.
              ' La vecchia regola (Avg vicino a nord, soglia fissa a 270) lasciava
              ' salti fino a 255 gradi quando un campione cadeva lontano dalla media.
              yOut = Avg + WrapTo180(Y - Avg)
            Case clsChannel2020.eDataType.eTack
              If CurrentTwaMode = eTwaMode.eStbd Then
                yOut = Y
              ElseIf CurrentTwaMode = eTwaMode.ePort Then
                yOut = -Y
              Else
                yOut = Double.NaN
              End If
            Case clsChannel2020.eDataType.eTackReversed
              If CurrentTwaMode = eTwaMode.eStbd Then
                yOut = -Y
              ElseIf CurrentTwaMode = eTwaMode.ePort Then
                yOut = Y
              Else
                yOut = Double.NaN
              End If
            Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
              yOut = System.Math.Abs(Y)
            Case clsChannel2020.eDataType.eLinear, clsChannel2020.eDataType.ePercentage
              yOut = Y
            Case clsChannel2020.eDataType.eBoolean
              yOut = -Y
            Case Else
              yOut = Y
          End Select

          If LineType = clsGroupLines.eLineType.eDataTypeSignedDeriv Then
            Dim Ytmp As Double = yOut
            If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Ytmp
            Dim Derivata As Double = (Ytmp - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
            objDerivata.MediaMobile.AggiornaMedia(Derivata)
            yOut = objDerivata.MediaMobile.Valore
            objDerivata.AggiornaValoriPrev(Ytmp, X)
          End If

        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
          If Not DataProvider2020.IsStbd(Indice) Then
            If Not ValoriOmologo Is Nothing AndAlso Indice < ValoriOmologo.Length Then Y = ValoriOmologo(Indice)
          End If

          ' ✅ FIX CRITICO: anche se Y è NaN, DEVI aggiungere un valore in Y (Double.NaN)
          If Double.IsNaN(Y) Then
            yOut = Double.NaN
          Else
            Select Case CanaleOrdinataLoaded.DataType
              Case clsChannel2020.eDataType.e360
                yOut = Avg + WrapTo180(Y - Avg)
              Case clsChannel2020.eDataType.eDays
                yOut = Y * 24 * 60 * 60
              Case clsChannel2020.eDataType.eAbs180
                yOut = System.Math.Abs(Y)
              Case Else
                yOut = Y
            End Select

            If LineType = clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv Then
              Dim Ytmp As Double = yOut
              If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Ytmp
              Dim Derivata As Double = (Ytmp - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
              objDerivata.MediaMobile.AggiornaMedia(Derivata)
              yOut = objDerivata.MediaMobile.Valore
              objDerivata.AggiornaValoriPrev(Ytmp, X)
            End If
          End If
      End Select

      ValoriY(nOut) = yOut
      nOut += 1
    Next

    ' array troncati alla lunghezza effettiva: X e Y hanno sempre la stessa Count
    Dim Risultato As New clsSerieCalcolata
    Risultato.N = nOut
    Risultato.X = New DateTime(Math.Max(nOut - 1, 0)) {}
    Risultato.Y = New Double(Math.Max(nOut - 1, 0)) {}
    Risultato.Indici = New Integer(Math.Max(nOut - 1, 0)) {}
    Array.Copy(ValoriX, Risultato.X, nOut)
    Array.Copy(ValoriY, Risultato.Y, nOut)
    Array.Copy(Indici, Risultato.Indici, nOut)
    Return Risultato

  End Function

  Public Function LoadTimePlotData(Reset As Boolean, rCanaliOrdinata As IEnumerable(Of clsChannel2020), TimeRangeToShow As clsTimeRange) As Boolean

    If Not Reset Then
      If _GroupLines.Count > 0 Then
        Dim same As Boolean = True

        Dim newList As IList(Of clsChannel2020) = TryCast(rCanaliOrdinata, IList(Of clsChannel2020))
        If newList Is Nothing Then newList = rCanaliOrdinata.ToList()

        If _GroupLines.Count <> newList.Count Then
          same = False
        Else
          For i As Integer = 0 To newList.Count - 1
            If Not Object.ReferenceEquals(_GroupLines(i).Canale, newList(i)) Then
              same = False
              Exit For
            End If
          Next
        End If

        If same Then
          For Each gl In _GroupLines
            If gl.IsLoaded(SelectedLineType.LineType) Then
              Return False
            End If
          Next
        Else
          _GroupLines.Clear()
        End If
      End If
    End If

    Try
      Dim CanaliStampati As New List(Of clsChannel2020)

      Yassi = New AxisCollection()
      Xassi = New AxisCollection()
      YassiOrg = New AxisCollection()

      CanaleAscissa = Nothing
      CanaliOrdinata = New ObservableCollection(Of clsChannel2020)(rCanaliOrdinata)

      If Annotazioni Is Nothing Then Annotazioni = New SciChart.Charting.Visuals.Annotations.AnnotationCollection
      If CanaliOrdinata Is Nothing OrElse CanaliOrdinata.Count = 0 Then Return False

      Dim ID As Integer = 0
      Dim PrimoCanaleCompletato As Boolean = False

      If Not _Annotazioni Is Nothing Then
        For Each Annotazione As SciChart.Charting.Visuals.Annotations.IAnnotation In _Annotazioni
          If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
            If DirectCast(DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation).Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSfondo Then
              PrimoCanaleCompletato = True
              Exit For
            End If
          End If
        Next
      End If

      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
      Dim CanaleTack As clsChannel2020 = CanaleTackDefault()

      Dim ListaTarget As New List(Of String)

      Dim timeStamps As IList(Of DateTime) = DataProvider2020.TimeStamps
      Dim timeStampsCount As Integer = If(timeStamps Is Nothing, 0, timeStamps.Count)
      If timeStampsCount = 0 Then Return False

      For Each CanaleOrdinataItem As clsChannel2020 In CanaliOrdinata
        If CanaleOrdinataItem Is Nothing Then Continue For

        Dim CanaleOrdinataLoaded As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleOrdinataItem.ChannelId)
        If CanaleOrdinataLoaded Is Nothing Then Continue For
        If CanaleOrdinataLoaded.Valori Is Nothing OrElse CanaleOrdinataLoaded.Valori.Count = 0 Then Continue For
        If CanaleRipetuto(CanaliStampati, CanaleOrdinataLoaded) Then Continue For

        Dim OrdinataIsStbd As Boolean = True
        Dim Omologo As clsChannel2020 = CanaleOrdinataLoaded

        If CanaleOrdinataLoaded.ChannelId.IndexOf("Port") > -1 Then
          Omologo = DataProvider2020.CanaleDbl(CanaleOrdinataLoaded.ChannelId.Replace("Port", "Stbd"))
          If Omologo Is Nothing Then Omologo = CanaleOrdinataLoaded
          OrdinataIsStbd = False
        ElseIf CanaleOrdinataLoaded.ChannelId.IndexOf("Stbd") > -1 Then
          Omologo = DataProvider2020.CanaleDbl(CanaleOrdinataLoaded.ChannelId.Replace("Stbd", "Port"))
          If Omologo Is Nothing Then Omologo = CanaleOrdinataLoaded
          OrdinataIsStbd = True
        End If

        Dim Tgt As clsTgt = TgtManager.Tgt
        Dim DataSeriesTarget As XyDataSeries(Of DateTime, Double) = Nothing
        Dim LineaTarget As FastLineRenderableSeries = Nothing
        Dim Colore As Color = ColoriDifferenziati(ID)

        Dim CanaleTgt As clsChannel2020 = Nothing
        If Reset OrElse _GroupLines.Where(Function(x) Not x.Tgt Is Nothing).Count = 0 Then
          If Not Tgt Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chTws Is Nothing Then
            ListaTarget.Add(CanaleOrdinataLoaded.PolarHeader.ToLower)
            CanaleTgt = DataProvider2020.VerificaCanaleTarget(CanaleOrdinataLoaded)
            If Tgt.PolareDisponibile(CanaleOrdinataLoaded.PolarHeader.ToLower) Then
              DataSeriesTarget = New XyDataSeries(Of DateTime, Double) With {
              .SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinataLoaded.LongName & " Target",
              .AcceptsUnsortedData = False
            }
              LineaTarget = New FastLineRenderableSeries With {
              .XAxisId = "DefaultAxisId",
              .StrokeThickness = 4
            }
              If CanaliOrdinata.Count = 1 Then
                LineaTarget.Stroke = Color.FromArgb(30, 0, 0, 255)
              Else
                LineaTarget.Stroke = Color.FromArgb(30, Colore.R, Colore.G, Colore.B)
              End If
            End If
          End If
        End If

        CanaliStampati.Add(CanaleOrdinataLoaded)

        Dim DataSeriesXy As New XyDataSeries(Of DateTime, Double) With {
        .SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinataLoaded.LongName,
        .AcceptsUnsortedData = False
      }

        Dim LineaXy As New FastLineRenderableSeries
        Dim Yasse As New SciChart.Charting.Visuals.Axes.NumericAxis

        LineaXy.Stroke = Colore
        CanaleOrdinataLoaded.PrintedColor = LineaXy.Stroke

        If CanaleOrdinataLoaded.DataType = clsChannel2020.eDataType.e360 Then
          Yasse.LabelProvider = New cls360LabelProvider()
        End If

        LineaXy.XAxisId = "DefaultAxisId"

        Yasse.AxisTitle = CanaleOrdinataLoaded.LongName
        Yasse.AutoRange = SciChart.Charting.Visuals.Axes.AutoRange.Once
        Yasse.GrowBy = New DoubleRange(0.1, 0.1)

        ' GetType() sul tipo, non su un'istanza: creare un DefaultTickLabel fuori da un asse
        ' fa fallire i binding del suo template (System.Windows.Data Error: 4)
        Dim LabelStyle As New Style(GetType(SciChart.Charting.Visuals.Axes.LabelProviders.DefaultTickLabel))
        LabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(Colore)))
        Yasse.TickLabelStyle = LabelStyle

        ' idem per AxisTitle: era la causa dei quattro Error 4 su TickTextBrush,
        ' TitleFontSize, TitleFontWeight e AxisTitle ripetuti per ogni asse creato
        Dim TitleLabelStyle As New Style(GetType(AxisTitle))
        TitleLabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(Colore)))
        Yasse.TitleStyle = TitleLabelStyle

        If Object.ReferenceEquals(CanaleOrdinataLoaded, CanaliOrdinata(0)) Then
          Dim Xasse As New SciChart.Charting.Visuals.Axes.DateTimeAxis
          Xasse.AxisTitle = ""
          Xasse.Id = "DefaultAxisId"
          Xasse.SubDayTextFormatting = "HH:mm:ss.fF"
          Dim VBR As New Binding("SharedXVisibleRange")
          VBR.Source = ObjChartSyncManager
          VBR.Mode = BindingMode.TwoWay
          Xasse.SetBinding(SciChart.Charting.Visuals.Axes.AxisCore.VisibleRangeProperty, VBR)
          Dim DR As New DateRange(DataProvider2020.TimeRange.Start, DataProvider2020.TimeRange.Finish)
          Xasse.VisibleRangeLimit = DR
          Xassi.Add(Xasse)
          Yasse.Id = NomeAsse(Yassi, "DefaultAxisId")
          YasseTitle = Yasse.AxisTitle
          _SpanYPrecedente = Double.NaN
          AddHandler Yasse.VisibleRangeChanged, AddressOf AsseY_VisibleRangeChanged
        Else
          Yasse.Id = NomeAsse(Yassi, "Channel" & CanaleOrdinataLoaded.ChannelId)
        End If

        LineaXy.YAxisId = Yasse.Id
        LineaXy.XAxisId = "DefaultAxisId"

        If Not DataSeriesTarget Is Nothing Then
          LineaTarget.YAxisId = Yasse.Id
          LineaTarget.XAxisId = LineaXy.XAxisId
        End If

        Yassi.Add(Yasse)
        YassiOrg.Add(Yasse)

        ' Serie gia' calcolata in background dal prefetch, oppure calcolata ora al volo.
        Dim Serie As clsSerieCalcolata = PrendiSerie(CanaleOrdinataLoaded, Omologo, TimeRangeToShow,
                                                     SelectedLineType.LineType, timeStamps, timeStampsCount)
        If Serie Is Nothing Then Continue For

        ' Le annotazioni di sfondo non dipendono dal canale: si costruiscono una sola volta
        ' per plot a partire dagli intervalli di mura precalcolati nel DataProvider.
        If Not PrimoCanaleCompletato AndAlso Serie.N > 0 Then
          CostruisciAnnotazioniSfondo()
        End If

        DataSeriesXy.Append(Serie.X, Serie.Y)

        If Not DataSeriesTarget Is Nothing AndAlso Not CanaleTgt Is Nothing Then
          ' il target si ricostruisce dagli indici mantenuti: e' una semplice copia,
          ' non vale la pena metterlo in cache insieme alla serie
          Dim ValoriTgtLoc As Double() = CanaleTgt.Valori
          If Not ValoriTgtLoc Is Nothing Then
            Dim TgtOut(Math.Max(Serie.N - 1, 0)) As Double
            For k As Integer = 0 To Serie.N - 1
              Dim idx As Integer = Serie.Indici(k)
              If idx < ValoriTgtLoc.Length Then TgtOut(k) = ValoriTgtLoc(idx)
            Next
            DataSeriesTarget.Append(Serie.X, TgtOut)
          End If
        End If

        LineaXy.DataSeries = DataSeriesXy
        LineaXy.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.MinMax

        Dim LineeDelCanale = _GroupLines.Where(Function(z) z.Canale Is CanaleOrdinataLoaded).FirstOrDefault
        If LineeDelCanale Is Nothing Then
          LineeDelCanale = New clsGroupLines(CanaleOrdinataLoaded)
          _GroupLines.Add(LineeDelCanale)
        End If

        Dim csvmTmp As New ChartSeriesViewModel(DataSeriesXy, LineaXy)
        Select Case SelectedLineType.LineType
          Case clsGroupLines.eLineType.eRawValue
            LineeDelCanale.RawValue = csvmTmp
          Case clsGroupLines.eLineType.eRawValueDeriv
            LineeDelCanale.RawValueDeriv = csvmTmp
          Case clsGroupLines.eLineType.eDataTypeSigned
            LineeDelCanale.Signed = csvmTmp
          Case clsGroupLines.eLineType.eDataTypeSignedDeriv
            LineeDelCanale.SignedDeriv = csvmTmp
          Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
            LineeDelCanale.WwdLwd = csvmTmp
          Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
            LineeDelCanale.WwdLwdDeriv = csvmTmp
        End Select

        If Not DataSeriesTarget Is Nothing Then
          LineaTarget.DataSeries = DataSeriesTarget
          LineaTarget.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.MinMax
          LineeDelCanale.Tgt = New ChartSeriesViewModel(DataSeriesTarget, LineaTarget)
        End If

        PrimoCanaleCompletato = True
        ID += 1
      Next

      ImpostaAnnotazioni(False)
      UpdateSeriesSource()
      If HaVincoloY Then
        ' i dati sono nella superficie solo dopo il binding: il vincolo si applica a layout concluso
        Dim S = Surface
        If Not S Is Nothing Then S.Dispatcher.BeginInvoke(DispatcherPriority.Background, New Action(AddressOf ApplicaRangeY))
      End If

    Catch ex As Exception
      Stop
      Return False
    End Try

    Return True
  End Function

  Private Sub ImpostaAnnotazioni(Reset As Boolean)
    If Not Reset Then
      For Each Annotazione As SciChart.Charting.Visuals.Annotations.IAnnotation In _Annotazioni
        If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation Then
          If DirectCast(DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation).Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eGruppo2 Then
            Exit Sub
          End If
        End If
      Next
    End If

    'Annotazioni = New SciChart.Charting.Visuals.Annotations.AnnotationCollection

    Dim An As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    An.X1 = 7
    An.Y1 = 0
    An.Stroke = New SolidColorBrush(Colors.DarkOrange)
    An.StrokeThickness = 2
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)
    An.StrokeDashArray = a
    An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
    An.IsHidden = False
    An.Tag = eGruppoAnnotazione.eGruppo1
    Annotazioni.Add(An)

    If OneIsPercentage() Then
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 100
      An.Stroke = New SolidColorBrush(Colors.Green)
      An.StrokeThickness = 3
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 90
      An.Stroke = New SolidColorBrush(Colors.DarkRed)
      An.StrokeThickness = 2
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 110
      An.Stroke = New SolidColorBrush(Colors.DarkGreen)
      An.StrokeThickness = 2
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.BindingGroup = Nothing
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
    End If

    Dim AnVL As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnVL.X1 = ObjChartSyncManager.CurrentPosition
    AnVL.Stroke = New SolidColorBrush(Colors.OrangeRed)
    AnVL.StrokeThickness = 1
    AnVL.BindingGroup = Nothing
    AnVL.IsHidden = False
    AnVL.IsEditable = False
    AnVL.Name = "CurrentPosition"
    AnVL.Tag = eGruppoAnnotazione.eGruppo2
    Annotazioni.Add(AnVL)

    Dim VbrVL = New Binding("CurrentPosition")
    VbrVL.Source = ObjChartSyncManager
    VbrVL.Mode = BindingMode.TwoWay
    AnVL.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VbrVL)
  End Sub

  Private Function OneIsPercentage() As Boolean
    For Each Ch In CanaliOrdinata
      If Not Ch Is Nothing Then
        If Ch.DataType = clsChannel2020.eDataType.ePercentage Then
          Return True
        End If
      End If
    Next
    Return False
  End Function

  Private Function IsStbd(CanaleTack As clsChannel2020, Indice As Integer, ValoreIfNaN As Boolean) As Boolean
    If CanaleTack Is Nothing Then Return ValoreIfNaN
    If Double.IsNaN(CanaleTack.Valori(Indice)) Then Return ValoreIfNaN
    Return CanaleTack.Valori(Indice) >= 0
  End Function

  Private Function TwaMode(CanaleTack As clsChannel2020, Indice As Integer, ValoreIfNaN As eTwaMode) As eTwaMode
    'If DataProvider2020.IsStbd(Indice) Then
    '  Return eTwaMode.eStbd
    'Else
    '  Return eTwaMode.ePort
    'End If

    If CanaleTack Is Nothing Then Return ValoreIfNaN
    If Double.IsNaN(CanaleTack.Valori(Indice)) Then Return ValoreIfNaN
    Dim Valore As Double = CanaleTack.Valori(Indice)
    If System.Math.Abs(Valore) < 5 OrElse System.Math.Abs(Valore) > 175 Then
      Return eTwaMode.eHeadDeadToWind
    Else
      If Valore > 0 Then
        Return eTwaMode.eStbd
      Else
        Return eTwaMode.ePort
      End If
    End If
  End Function

  'Private Sub pObjChartSyncManager_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles ObjChartSyncManager.PropertyChanged
  '  Select Case e.PropertyName
  '    Case "SharedXVisibleRange"
  '      AggiornaDisplayValori()
  '    Case "CurrentPosition"
  '      AggiornaCurrentValues()
  '  End Select

  'End Sub



  Public Sub AggiornaCurrentValues()
    If Not DatiCaricati Then Exit Sub
    If CanaliOrdinata Is Nothing Then Exit Sub
    For Each Canale In CanaliOrdinata
      If Not Canale Is Nothing Then
        If Not Canale.Valori Is Nothing Then
          If Canale.Valori.Count > 0 AndAlso Canale.Valori.Count > ObjChartSyncManager.CurrentRow Then
            Canale.CurrentValue = Canale.Valori(ObjChartSyncManager.CurrentRow)
            'Canale.StrCurrentValue = Canale.CurrentValue.ToString("F" & Canale.Decimals)
          End If
        End If
      End If
    Next
  End Sub

  Public Sub AggiornaDisplayValoriPeriodoVisibile()
    AggiornaDisplayValori(ObjChartSyncManager.VisibleRange)
  End Sub

  Public Sub AggiornaDisplayValori(Intervallo As clsTimeRange)
    If Not DatiCaricati Then Exit Sub
    If CanaliOrdinata Is Nothing Then Exit Sub
    For Each Canale As clsChannel2020 In CanaliOrdinata
      If Not Canale Is Nothing Then
        Canale.StatisticheIntervallo.AggiornaIntervallo(Intervallo, SelectedLineType.LineType, Canale.ChannelId)
        'Canale.ScatenaEventoStatisticheIntervallo()
        'Canale.ValoriIntervallo.AggiornaValori(Intervallo, False)
      End If
    Next
  End Sub


  Public Sub SelezionaCanaliGrafico()
    'ImpostaCanaliAsIsSelected(CanaliOrdinata)
    'Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, True)
    Dim res = GestisciListaCanali("SelectChannel", DataProvider2020.Channels.ListaCanali.ToList, CanaliOrdinata.ToList, eSelectChannelType.eMulti)
    If Not res Is Nothing Then
      Dim CM As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTimeOnly)
      Dim CS As List(Of clsChannel2020) = res
      Settings.YaxisChannels.Clear()
      For Each c In CS
        Settings.YaxisChannels.Add(c.ChannelId)
      Next
      If LoadTimePlotData(False, CS) Then
        AggiornaDisplayValoriPeriodoVisibile()
        NascondiAssiXsecondari()
        AppConfig.Salva()
      End If
    End If
    'ImpostaCanaliAsIsSelected(Nothing)
  End Sub

  Private Sub ObjChartSyncManager_MousePositionChanged(Position As Date) Handles ObjChartSyncManager.MousePositionChanged
    AggiornaCurrentValues()
  End Sub

  Private Sub ObjChartSyncManager_VisibleRangeChanged(TimeRange As clsTimeRange) Handles ObjChartSyncManager.VisibleRangeChanged
    AggiornaDisplayValoriPeriodoVisibile()
  End Sub
End Class

Public Class clsSciChartTimeRange
  Dim pTimeRange As clsTimeRange
  Dim pCurrentRange As IRange
  Dim pName As String

  Public Event RangeChanged(CurrentRange As IRange, TimeRange As clsTimeRange, Name As String)

  Public Property CurrentRange As IRange
    Get
      Return pCurrentRange
    End Get
    Set(value As IRange)
      Dim TRtmp As clsTimeRange = IRangeToTimeRange(value)
      If Not pTimeRange.HasSameRange(TRtmp) Then
        pTimeRange = TRtmp
        pCurrentRange = value
        ScatenaEvento()
      End If
    End Set
  End Property

  Public Property TimeRange As clsTimeRange
    Get
      Return pTimeRange
    End Get
    Set(value As clsTimeRange)
      Dim TRtmp As IRange = TimeRangeToIRange(value)
      If pTimeRange Is Nothing OrElse Not pTimeRange.HasSameRange(value) Then
        pTimeRange = value
        pCurrentRange = TRtmp
        ScatenaEvento()
      End If
    End Set
  End Property

  Private Function IRangeToTimeRange(Range As IRange) As clsTimeRange
    Dim inizio As DateTime = DirectCast(Range.Min, DateTime)
    Dim fine As DateTime = DirectCast(Range.Max, DateTime)
    Return New clsTimeRange(inizio, fine)
  End Function

  Private Function TimeRangeToIRange(Range As clsTimeRange) As IRange
    Return New DateRange(Range.Start, Range.Finish)
  End Function

  Public Sub New(Name As String)
    pName = Name
  End Sub

  Private Sub ScatenaEvento()
    RaiseEvent RangeChanged(pCurrentRange, pTimeRange, pName)
  End Sub

End Class


Public Class cls360LabelProvider
  Inherits NumericLabelProvider

  Public Overrides Sub Init(parentAxis As IAxisCore)
    MyBase.Init(parentAxis)
  End Sub

  Public Overrides Sub OnBeginAxisDraw()
    MyBase.OnBeginAxisDraw()
  End Sub

  ''' <summary>
  ''' Normalizza in 0..360 con un modulo vero: con il centraggio sulla media i valori
  ''' dell'asse possono uscire di piu' di un giro (es. -190 oppure 550), e la vecchia
  ''' correzione con due If singoli copriva un solo giro.
  ''' </summary>
  Private Shared Function Normalizza360(v As Double) As Double
    Dim r As Double = v Mod 360
    If r < 0 Then r += 360
    Return r
  End Function

  Public Overrides Function FormatLabel(dataValue As IComparable) As String
    Return Format(Normalizza360(CDbl(dataValue)), "F0").PadLeft(3, "0")
  End Function

  Public Overrides Function FormatCursorLabel(dataValue As IComparable) As String
    Return Format(Normalizza360(CDbl(dataValue)), "F0").PadLeft(3, "0")
  End Function

End Class

Public Class clsLabelProvider
  Inherits NumericLabelProvider
  Dim pDecimals As Integer

  Public Sub New(Decimals As Integer)
    pDecimals = Decimals
  End Sub

  Public Overrides Sub Init(parentAxis As IAxisCore)
    MyBase.Init(parentAxis)
  End Sub

  Public Overrides Sub OnBeginAxisDraw()
    MyBase.OnBeginAxisDraw()
  End Sub

  Public Overrides Function FormatLabel(dataValue As IComparable) As String
    Return Format(dataValue, "F" & pDecimals)
    'Return MyBase.FormatLabel(vTmp)
  End Function

  Public Overrides Function FormatCursorLabel(dataValue As IComparable) As String
    Return Format(dataValue, "F" & pDecimals)
    'Return MyBase.FormatCursorLabel(vTmp)
  End Function

End Class

'<AddINotifyPropertyChangedInterface>
Public Class clsPuntoMetadata
  Implements IPointMetadata
  Dim _IsSelected As Boolean
  Dim _Momento As DateTime

  Public Sub New(IsSelected As Boolean)
    _IsSelected = IsSelected
  End Sub

  Public Sub New(IsSelected As Boolean, Momento As DateTime)
    _IsSelected = IsSelected
    _Momento = Momento
  End Sub

  Public Property IsSelected As Boolean Implements IPointMetadata.IsSelected
    Get
      Return _IsSelected
    End Get
    Set(value As Boolean)
      _IsSelected = value
    End Set
  End Property

  Public Property Momento As Date
    Get
      Return _Momento
    End Get
    Set(value As Date)
      _Momento = value
    End Set
  End Property

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class