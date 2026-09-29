Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.IO
Imports System.Security.Cryptography.Xml
Imports PropertyChanged
Imports SciChart.Charting2D.Interop

Public Class UserControlParquetFinder
  Dim _VM As New clsParquetFinder
  Dim _Loading As Boolean = True
  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    Me.DataContext = _VM
    'Dim Inizio As New DateTime(2020, 4, 16, 0, 0, 0)
    'Dim Fine As New DateTime(2020, 5, 29, 0, 0, 0)
    '_VM.TR = New clsTimeRange(Inizio, Fine)

    _VM.ImpostaCanaliOutputDefault()
    _Loading = False
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    _VM.AggiungiFiltro()

  End Sub

  Private Sub chkPeriods_Checked(sender As Object, e As RoutedEventArgs)
    If chkAllRows_DataTable Is Nothing Then Exit Sub
    chkAllRows_DataTable.IsChecked = False
    chkAllRows_PnP.IsChecked = False
    chkAllRows_Statistics.IsChecked = False
    _VM.CercaPeriodi = True
    _VM.OutPut = clsParquetFinder.eOutput.ePeriods
  End Sub

  Private Sub chkPeriods_Unchecked(sender As Object, e As RoutedEventArgs)
    _VM.OutPut = clsParquetFinder.eOutput.eNone
  End Sub

  Private Sub AggiornaPeriodiDaCercare()
    If _Loading Then Exit Sub
    _VM.AggiornaPeriodiDaCercare(chkStrLines.IsChecked, chkAccelerations.IsChecked, chkTacks.IsChecked, chkGybes.IsChecked)
  End Sub

  Private Sub chkStrLines_Checked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkStrLines_Unchecked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkTacks_Checked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkTacks_Unchecked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkGybes_Checked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkGybes_Unchecked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkAccelerations_Checked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkAccelerations_Unchecked(sender As Object, e As RoutedEventArgs)
    AggiornaPeriodiDaCercare()
  End Sub

  Private Sub chkAllRows_PnP_Checked(sender As Object, e As RoutedEventArgs)
    If chkAllRows_DataTable Is Nothing Then Exit Sub
    chkPeriods.IsChecked = False
    chkAllRows_DataTable.IsChecked = False
    chkAllRows_Statistics.IsChecked = False
    _VM.OutPut = clsParquetFinder.eOutput.ePeriods
  End Sub

  Private Sub chkAllRows_PnP_Unchecked(sender As Object, e As RoutedEventArgs)
    _VM.OutPut = clsParquetFinder.eOutput.eNone
  End Sub

  Private Sub chkAllRows_DataTable_Checked(sender As Object, e As RoutedEventArgs)
    chkPeriods.IsChecked = False
    chkAllRows_PnP.IsChecked = False
    chkAllRows_Statistics.IsChecked = False
    _VM.OutPut = clsParquetFinder.eOutput.eData
  End Sub

  Private Sub chkAllRows_DataTable_Unchecked(sender As Object, e As RoutedEventArgs)
    _VM.OutPut = clsParquetFinder.eOutput.eNone
  End Sub

  Private Sub chkAllRows_Statistics_Checked(sender As Object, e As RoutedEventArgs)
    chkPeriods.IsChecked = False
    chkAllRows_DataTable.IsChecked = False
    chkAllRows_PnP.IsChecked = False
    _VM.OutPut = clsParquetFinder.eOutput.eStatitics
  End Sub

  Private Sub chkAllRows_Statistics_Unchecked(sender As Object, e As RoutedEventArgs)
    _VM.OutPut = clsParquetFinder.eOutput.eNone
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    _VM.ImpostaCanaliOutputDefault()
    Dim res As List(Of String) = GestisciListaCanali("", _VM.ListaCanali, _VM.ListaCanaliOutput, eSelectChannelType.eTwin)
    If res.Count > 0 Then
      _VM.ListaCanaliOutput = res
    End If
    'Dim ChannelSelector As New UserControlSqlChannelSelector(_VM.ListaCanali, _VM.ListaCanaliOutput)
    'ChannelSelector.ShowDialog()
    'If ChannelSelector.Status = UserControlSqlChannelSelector.eStatus.eSave Then
    '  _VM.ListaCanaliOutput = ChannelSelector.AvailableChannels.ListaCanaliSelezionati
    'End If
  End Sub

  Private Async Sub btnSearch(sender As Object, e As RoutedEventArgs)
    Await Task.Run(Sub() UpdateApplicationDataUI())
  End Sub

  'Async Sub RicercaDaInterfaccia()
  '  _VM.SalvaConfigurazione()
  '  _VM.LanciaRicercaDaInterfaccia()
  'End Sub

  Private Sub UpdateApplicationDataUI()
    _VM.SearchEnabled = False
    _VM.SalvaConfigurazione()
    _VM.LanciaRicercaDaInterfaccia()
    _VM.SearchEnabled = True  '_VM.OutPut = Not clsParquetFinder.eOutput.eNone
  End Sub

End Class


Public Class clsParquetFinderStorage
  Dim _TR As clsTimeRange
  Dim _ListaCanaliOutput As New List(Of String)
  Dim _ListaCompletaCanali As New List(Of String)
  Dim _ListaTipoPeriodi As New List(Of clsPeriod2021.ePeriodType)
  Dim _ListaFiltri As New List(Of clsFiltroParquetFinder)
  Dim _IdOutput As Integer

  Public Sub New()

  End Sub



  Public Property ListaCanaliOutput As List(Of String)
    Get
      Return _ListaCanaliOutput
    End Get
    Set(value As List(Of String))
      _ListaCanaliOutput = value
    End Set
  End Property

  Public Property ListaCompletaCanali As List(Of String)
    Get
      Return _ListaCompletaCanali
    End Get
    Set(value As List(Of String))
      _ListaCompletaCanali = value
    End Set
  End Property

  Public Property ListaTipoPeriodi As List(Of clsPeriod2021.ePeriodType)
    Get
      Return _ListaTipoPeriodi
    End Get
    Set(value As List(Of clsPeriod2021.ePeriodType))
      _ListaTipoPeriodi = value
    End Set
  End Property

  Public Property ListaFiltri As List(Of clsFiltroParquetFinder)
    Get
      Return _ListaFiltri
    End Get
    Set(value As List(Of clsFiltroParquetFinder))
      _ListaFiltri = value
    End Set
  End Property

  Public Property IdOutput As Integer
    Get
      Return _IdOutput
    End Get
    Set(value As Integer)
      _IdOutput = value
    End Set
  End Property

  Public Property TR As clsTimeRange
    Get
      Return _TR
    End Get
    Set(value As clsTimeRange)
      _TR = value
    End Set
  End Property

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsParquetFinder
  'Implements INotifyPropertyChanged
  'cerca i file parquet con data conpresa nei criteri di ricerca
  'legge tutto il file oppure i soli periodi selezionati
  'crea un file parquet con i valori dei campi cercati pú quelli base obbligatori
  'crea il json con i periodi del criterio di selezione
  public property TR As New clsTimeRange
  Public Property ListaCanaliOutput As New List(Of String)
  Public Property ListaCompletaCanali As New List(Of String)
  Public Property ListaFilePeriodi As New List(Of clsParquetAndPeriods)
  Public Property BasicChannelsOutput As New clsBasicChannelsFinder
  Public Property PeriodiDaEsportare As New List(Of clsPeriod2021)
  Public Property PathParquetFolder As String = "D:\Performance\Logs\AC75B1"
  Public Property StringaDescrizioneFiltro As String = ""
  Public Property CercaPeriodi As Boolean = True
  Public Property ListaTipoPeriodi As New List(Of clsPeriod2021.ePeriodType)
  'Dim _ListaFiltri As New List(Of clsFiltroParquetFinder)
  Public Property ListaControlliFiltro As New ObservableCollection(Of UserControlParquetFinderFilter)
  Public Property OutPut As eOutput = eOutput.eNone

  Public Property PgbMaxVal As Double = 100
  Public Property PgbMinVal As Double = 0
  Public Property PgbValue As Double = 10
  Public Property PgbVisibility As Visibility = Visibility.Hidden

  Public Property ResultTxt As String
  Public Property SearchEnabled As Boolean = False
  Public Property PgbText As String

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Enum eOutput
    ePeriods = 0 'file parquet con i dati dei periodo selezionati ed un file json con i periodi
    eStatitics = 1 'csv con le statistiche dei canali selezionati
    eData = 2 'csv con tutti i dati dei canali selezionati
    eNone = 3
  End Enum

  Public Sub New()
    CaricaConfigurazione()
  End Sub

  Public Sub SalvaConfigurazione()
    Dim Obj As New clsParquetFinderStorage
    Obj.TR = _TR
    Obj.ListaCanaliOutput = _ListaCanaliOutput
    Obj.ListaCompletaCanali = _ListaCompletaCanali
    Obj.ListaTipoPeriodi = _ListaTipoPeriodi
    Obj.ListaFiltri = _ListaControlliFiltro.Select(Function(x) x.VM).ToList
    If CercaPeriodi Then
      Obj.IdOutput = 0
    Else
      Select Case _OutPut
        Case eOutput.eNone
          Obj.IdOutput = 4
        Case eOutput.eData
          Obj.IdOutput = 3
        Case eOutput.eStatitics
          Obj.IdOutput = 2
        Case eOutput.ePeriods
          Obj.IdOutput = 1
      End Select
    End If
    If clsKillerSeriale.SaveConfigurationGeneric(Of clsParquetFinderStorage)(Obj, _PathParquetFolder & "\Filter.json") Then

    End If

  End Sub

  Public Sub CaricaConfigurazione()
    Dim Obj As clsParquetFinderStorage = clsKillerSeriale.LoadConfigurationGeneric(Of clsParquetFinderStorage)(_PathParquetFolder & "\Filter.json")
    If Obj Is Nothing Then
      ImpostaListaCanaliCompleta()
    Else
      _TR = Obj.TR
      If _TR Is Nothing Then
        _TR = New clsTimeRange(Now.AddDays(-30), Now)
      End If
      _ListaCanaliOutput = Obj.ListaCanaliOutput
      _ListaCompletaCanali = Obj.ListaCompletaCanali
      _ListaTipoPeriodi = Obj.ListaTipoPeriodi
      For Each filtro In Obj.ListaFiltri
        filtro.Parent = Me
        Dim Controllo As New UserControlParquetFinderFilter(filtro)
        _ListaControlliFiltro.Add(Controllo)
      Next
      Select Case Obj.IdOutput
        Case 0
          _CercaPeriodi = True
          _OutPut = eOutput.ePeriods
        Case 1
          _CercaPeriodi = False
          _OutPut = eOutput.ePeriods
        Case 2
          _CercaPeriodi = False
          _OutPut = eOutput.eStatitics
        Case 3
          _CercaPeriodi = False
          _OutPut = eOutput.eData
        Case 4
          _CercaPeriodi = False
          _OutPut = eOutput.eNone
      End Select
      'ScatenaOnPropertyChanged()
    End If

  End Sub

  Public Sub LanciaRiercaManuale()
    ImpostaManualmenteTipoPeriodi()
    ImpostaCanaliOutput("SystemTime_DaySeconds,Bs,Tws,Twa,Awa")
    Dim Inizio As New DateTime(2020, 4, 16, 0, 0, 0)
    Dim Fine As New DateTime(2020, 5, 29, 0, 0, 0)
    _TR = New clsTimeRange(Inizio, Fine)
    'Dim Inizio As New DateTime(2020, 6, 2, 0, 0, 0)
    'Dim Fine As New DateTime(2020, 6, 17, 0, 0, 0)
    '_Tr = New clsTimeRange(Now.AddDays(-20), Now.AddDays(-10))
    _CercaPeriodi = False
    LanciaRicerca()
  End Sub

  Public Sub LanciaRicercaDaInterfaccia()
    LanciaRicerca()
  End Sub

  Public Sub AggiornaPeriodiDaCercare(StraightLines As Boolean, Accelerazioni As Boolean, Tacks As Boolean, Gybes As Boolean)
    _ListaTipoPeriodi.Clear()
    If StraightLines Then _ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eStraightLineVmg)
    If Accelerazioni Then _ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eAcceleration)
    If Tacks Then _ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eTack)
    If Gybes Then _ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eGybe)
  End Sub

  Private Sub LanciaRicerca()
    PgbVisibility = Visibility.Visible
    If _CercaPeriodi Then
      TrovaPeriodi()
      EsportaPeriodi()
    Else
      TrovaRighe()
      'EsportaRighe(eOutput.ePeriods, Nothing)
      'EsportaRighe(eOutput.eData, _ListaCanaliBase)
      EsportaRighe(eOutput.eStatitics, _ListaCanaliOutput)
    End If
    PgbVisibility = Visibility.Hidden
  End Sub

  'Public Property SearchEnabled As Boolean
  '  Get
  '    Return _SearchEnabled
  '  End Get
  '  Set(value As Boolean)
  '    _SearchEnabled = value
  '    OnPropertyChanged("SearchEnabled")
  '  End Set
  'End Property

  Public ReadOnly Property OutputChannelsEnabled As Boolean
    Get
      Return _OutPut = eOutput.eData OrElse _OutPut = eOutput.eStatitics
    End Get
    'Set(value As Boolean)
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchPeriods As Boolean
    Get
      Return _OutPut = eOutput.ePeriods AndAlso _CercaPeriodi
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchPeriods")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchRowsOutPeriods As Boolean
    Get
      Return _OutPut = eOutput.ePeriods AndAlso Not _CercaPeriodi
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchRowsOutPeriods")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchRowsOutData As Boolean
    Get
      Return _OutPut = eOutput.eData AndAlso Not _CercaPeriodi
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchRowsOutData")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchRowsOutStatistics As Boolean
    Get
      Return _OutPut = eOutput.eStatitics AndAlso Not _CercaPeriodi
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchRowsOutStatistics")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchSL As Boolean
    Get
      Return _ListaTipoPeriodi.Where(Function(x) x = clsPeriod2021.ePeriodType.eStraightLineVmg).Count > 0
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchSL")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchTacks As Boolean
    Get
      Return _ListaTipoPeriodi.Where(Function(x) x = clsPeriod2021.ePeriodType.eTack).Count > 0
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchTacks")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchGybes As Boolean
    Get
      Return _ListaTipoPeriodi.Where(Function(x) x = clsPeriod2021.ePeriodType.eGybe).Count > 0
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchGybes")
    '  Stop
    'End Set
  End Property

  Public ReadOnly Property SearchAcc As Boolean
    Get
      Return _ListaTipoPeriodi.Where(Function(x) x = clsPeriod2021.ePeriodType.eAcceleration).Count > 0
    End Get
    'Set(value As Boolean)
    '  OnPropertyChanged("SearchAcc")
    '  Stop
    'End Set
  End Property


  'Public Property TR As clsTimeRange
  '  Get
  '    Return _TR
  '  End Get
  '  Set(value As clsTimeRange)
  '    _TR = value
  '    OnPropertyChanged("TR")
  '  End Set
  'End Property

  'Public Property ListaFilePeriodi As List(Of clsParquetAndPeriods)
  '  Get
  '    Return _ListaFilePeriodi
  '  End Get
  '  Set(value As List(Of clsParquetAndPeriods))
  '    _ListaFilePeriodi = value
  '  End Set
  'End Property

  Public ReadOnly Property ListaCanali As List(Of String)
    Get
      Return _ListaCompletaCanali
    End Get
  End Property

  Public Sub AggiungiFiltro()
    Dim VM As New clsFiltroParquetFinder()
    VM.Parent = Me
    Dim Controllo As New UserControlParquetFinderFilter(VM) 'Me, _ListaCompletaCanali)
    If ListaControlliFiltro.Count > 0 Then
      Controllo.VM.HeaderVisibility = Visibility.Hidden
    End If
    'Controllo.VM.Parent = Me
    'Controllo.ListaCanali = _ListaCompletaCanali
    ListaControlliFiltro.Add(Controllo)
    '_ListaFiltri.Add(New clsFiltroParquetFinder)
    'OnPropertyChanged("ListaControlliFiltro")
  End Sub


  'Public Property CercaPeriodi As Boolean
  '  Get
  '    Return _CercaPeriodi
  '  End Get
  '  Set(value As Boolean)
  '    _CercaPeriodi = value
  '    OnPropertyChanged("CercaPeriodi")
  '  End Set
  'End Property

  'Public Property ListaCanaliOutput As List(Of String)
  '  Get
  '    Return _ListaCanaliOutput
  '  End Get
  '  Set(value As List(Of String))
  '    _ListaCanaliOutput = value
  '    OnPropertyChanged("ListaCanaliOutput")
  '  End Set
  'End Property

  'Public Property StringaDescrizioneFiltro As String
  '  Get
  '    Return _StringaDescrizioneFiltro
  '  End Get
  '  Set(value As String)
  '    _StringaDescrizioneFiltro = value
  '    OnPropertyChanged("StringaDescrizioneFiltro")
  '  End Set
  'End Property

  'Public Property ListaTipoPeriodi As List(Of clsPeriod2021.ePeriodType)
  '  Get
  '    Return _ListaTipoPeriodi
  '  End Get
  '  Set(value As List(Of clsPeriod2021.ePeriodType))
  '    _ListaTipoPeriodi = value
  '    OnPropertyChanged("ListaTipoPeriodi")
  '  End Set
  'End Property

  'Public Property OutPut As eOutput
  '  Get
  '    Return _OutPut
  '  End Get
  '  Set(value As eOutput)
  '    _OutPut = value
  '    SearchEnabled = Not _OutPut = eOutput.eNone
  '    OnPropertyChanged("OutPut")
  '    OnPropertyChanged("OutputChannelsEnabled")
  '  End Set
  'End Property

  'Public Property ListaControlliFiltro As ObservableCollection(Of UserControlParquetFinderFilter)
  '  Get
  '    Return _ListaControlliFiltro
  '  End Get
  '  Set(value As ObservableCollection(Of UserControlParquetFinderFilter))
  '    _ListaControlliFiltro = value
  '    OnPropertyChanged("ListaControlliFiltro")
  '  End Set
  'End Property

  'Public Property PgbMaxVal As Double
  '  Get
  '    Return _PgbMaxVal
  '  End Get
  '  Set(value As Double)
  '    _PgbMaxVal = value
  '    OnPropertyChanged("PgbMaxVal")
  '  End Set
  'End Property

  'Public Property PgbMinVal As Double
  '  Get
  '    Return _PgbMinVal
  '  End Get
  '  Set(value As Double)
  '    _PgbMinVal = value
  '    OnPropertyChanged("PgbMinVal")
  '  End Set
  'End Property

  'Public Property PgbValue As Double
  '  Get
  '    Return _PgbValue
  '  End Get
  '  Set(value As Double)
  '    _PgbValue = value
  '    OnPropertyChanged("PgbValue")
  '  End Set
  'End Property

  'Public Property PgbVisibility As Visibility
  '  Get
  '    Return _PgbVisibility
  '  End Get
  '  Set(value As Visibility)
  '    _PgbVisibility = value
  '    OnPropertyChanged("PgbVisibility")
  '  End Set
  'End Property

  'Public Property ResultTxt As String
  '  Get
  '    Return _ResultTxt
  '  End Get
  '  Set(value As String)
  '    _ResultTxt = value
  '    OnPropertyChanged("ResultTxt")
  '  End Set
  'End Property

  'Public Property PgbText As String
  '  Get
  '    Return _PgbText
  '  End Get
  '  Set(value As String)
  '    _PgbText = value
  '    OnPropertyChanged("PgbText")
  '  End Set
  'End Property

  Private Sub ImpostaListaCanaliCompleta()
    _ListaCompletaCanali.Clear()
    _ListaCompletaCanali.Add("SystemTime_DaySeconds")
    _ListaCompletaCanali.Add("Bs")
    _ListaCompletaCanali.Add("PortOutFlap1_Ang")
    _ListaCompletaCanali.Add("PortInFlap1_Ang")
    _ListaCompletaCanali.Add("StbdOutFlap1_Ang")
    _ListaCompletaCanali.Add("StbdInFlap1_Ang")
    _ListaCompletaCanali.Add("RudderRake_Ang")
    _ListaCompletaCanali.Add("LatBow")
    _ListaCompletaCanali.Add("LonBow")
    _ListaCompletaCanali.Add("Tws")
    _ListaCompletaCanali.Add("Twd")
    _ListaCompletaCanali.Add("Twa")
    _ListaCompletaCanali.Add("Awa")
    _ListaCompletaCanali.Add("Cog")
    _ListaCompletaCanali.Add("Sog")
    _ListaCompletaCanali.Add("BsTgt")
    _ListaCompletaCanali.Add("TwaTgt")
    _ListaCompletaCanali.Add("Heel")
    _ListaCompletaCanali.Add("Aws")
    _ListaCompletaCanali.Add("Hdg")
    _ListaCompletaCanali.Add("Lwy")
    _ListaCompletaCanali.Add("Cse")
    _ListaCompletaCanali.Add("Pitch")
    _ListaCompletaCanali.Add("Roll")
    _ListaCompletaCanali.Add("AccX")
    _ListaCompletaCanali.Add("AccY")
    _ListaCompletaCanali.Add("AccZ")
    _ListaCompletaCanali.Add("PitchRate")
    _ListaCompletaCanali.Add("RollRate")
    _ListaCompletaCanali.Add("YawRate")
    _ListaCompletaCanali.Add("Trim")
    _ListaCompletaCanali.Add("ForestayPin_Load")
    _ListaCompletaCanali.Add("MainSheetPlusOuthaul_Load")
    _ListaCompletaCanali.Add("Rudder_Ang")
    _ListaCompletaCanali.Add("Traveller_Ang")
    _ListaCompletaCanali.Add("Spanner_Ang")
    _ListaCompletaCanali.Add("FCS_PortCant_Ang")
    _ListaCompletaCanali.Add("FCS_StbdCant_Ang")
    _ListaCompletaCanali.Add("JibCar_Ang")


    _ListaCompletaCanali.Add("AbsTwa")
    _ListaCompletaCanali.Add("StbdOutFlap2_Ang")
    _ListaCompletaCanali.Add("StbdInFlap2_Ang")
    _ListaCompletaCanali.Add("PortOutFlapAPActive")
    _ListaCompletaCanali.Add("PortInFlapAPActive")
    _ListaCompletaCanali.Add("StbdInFlapAPActive")
    _ListaCompletaCanali.Add("StbdOutFlapAPActive")
    _ListaCompletaCanali.Add("SystemTime_Local")
    _ListaCompletaCanali.Add("SystemTime_Date")
    _ListaCompletaCanali.Add("TrimRate")
    _ListaCompletaCanali.Add("HeelRate")
    _ListaCompletaCanali.Add("Vmg")
    _ListaCompletaCanali.Add("Battery_Current")
    _ListaCompletaCanali.Add("Battery_Voltage")
    _ListaCompletaCanali.Add("Battery_Temp")
    _ListaCompletaCanali.Add("SailingPP_Temp")
    _ListaCompletaCanali.Add("FoilingPP_Temp")
    _ListaCompletaCanali.Add("PortTwistLine1_Load")
    _ListaCompletaCanali.Add("PortTwistLine2_Load")
    _ListaCompletaCanali.Add("StbdTwistLine1_Load")
    _ListaCompletaCanali.Add("StbdTwistLine2_Load")
    _ListaCompletaCanali.Add("MainCunnPortLine_Load")
    _ListaCompletaCanali.Add("MainCunnStbdLine_Load")
    _ListaCompletaCanali.Add("PortV1Pin_Load")
    _ListaCompletaCanali.Add("PortD1Pin_Load")
    _ListaCompletaCanali.Add("StbdV1Pin_Load")
    _ListaCompletaCanali.Add("StbdD1Pin_Load")
    _ListaCompletaCanali.Add("PortTravRam_Load")
    _ListaCompletaCanali.Add("StbdTravRam_Load")
    _ListaCompletaCanali.Add("SpannerRam_Load")
    _ListaCompletaCanali.Add("TwistRam_Load")
    _ListaCompletaCanali.Add("TopPostRam_Load")
    _ListaCompletaCanali.Add("PortJibUpDwRam_Load")
    _ListaCompletaCanali.Add("StbdJibUpDwRam_Load")
    _ListaCompletaCanali.Add("MainOuthaulRam_Load")
    _ListaCompletaCanali.Add("MainCunnRam_Load")
    _ListaCompletaCanali.Add("JibInhaulRam_Load")
    _ListaCompletaCanali.Add("JibOuthaulRam_Load")
    _ListaCompletaCanali.Add("JibCunnRam_Load")
    _ListaCompletaCanali.Add("ForestayRam_Load")
    _ListaCompletaCanali.Add("PortV1Strain_Load")
    _ListaCompletaCanali.Add("PortD1Strain_Load")
    _ListaCompletaCanali.Add("StbdV1Strain_Load")
    _ListaCompletaCanali.Add("StbdD1Strain_Load")
    _ListaCompletaCanali.Add("BobstayStrain_Load")
    _ListaCompletaCanali.Add("StbdJibSheetRam_Load")
    _ListaCompletaCanali.Add("PortJibSheetRam_Load")
    _ListaCompletaCanali.Add("StbdRunnerRam_Load")
    _ListaCompletaCanali.Add("PortRunnerRam_Load")
    _ListaCompletaCanali.Add("MainSheetRam_Load")
    _ListaCompletaCanali.Add("PortOutFlapRam1_Load")
    _ListaCompletaCanali.Add("PortOutFlapRam2_Load")
    _ListaCompletaCanali.Add("PortInFlapRam1_Load")
    _ListaCompletaCanali.Add("PortInFlapRam2_Load")
    _ListaCompletaCanali.Add("StbdOutFlapRam1_Load")
    _ListaCompletaCanali.Add("StbdOutFlapRam2_Load")
    _ListaCompletaCanali.Add("StbdInFlapRam1_Load")
    _ListaCompletaCanali.Add("StbdInFlapRam2_Load")
    _ListaCompletaCanali.Add("RudderRakeRam_Load")
    _ListaCompletaCanali.Add("Traveller_Load")
    _ListaCompletaCanali.Add("PortCantRam_Load")
    _ListaCompletaCanali.Add("StbdCantRam_Load")
    _ListaCompletaCanali.Add("JibCunnRam_Pos")
    _ListaCompletaCanali.Add("FlapsTank_Pos")
    _ListaCompletaCanali.Add("StbdMainTank_Pos")
    _ListaCompletaCanali.Add("PortMainTank_Pos")
    _ListaCompletaCanali.Add("PowerPackTank_Pos")
    _ListaCompletaCanali.Add("PortJibUpDwRam_Pos")
    _ListaCompletaCanali.Add("StbdJibUpDwRam_Pos")
    _ListaCompletaCanali.Add("PortJibSheetTank_Pos")
    _ListaCompletaCanali.Add("StbdJibSheetTank_Pos")
    _ListaCompletaCanali.Add("JibInhaulRam_Pos")
    _ListaCompletaCanali.Add("JibOuthaulRam_Pos")
    _ListaCompletaCanali.Add("PortJibSheetRam_Pos")
    _ListaCompletaCanali.Add("StbdJibSheetRam_Pos")
    _ListaCompletaCanali.Add("MainOuthaulRam_Pos")
    _ListaCompletaCanali.Add("TwistRam_Pos")
    _ListaCompletaCanali.Add("PortTravRam_Pos")
    _ListaCompletaCanali.Add("StbdTravRam_Pos")
    _ListaCompletaCanali.Add("MainSheetRam_Pos")
    _ListaCompletaCanali.Add("MainSheetTank_Pos")
    _ListaCompletaCanali.Add("ForestayRam_Pos")
    _ListaCompletaCanali.Add("SpannerRam_Pos")
    _ListaCompletaCanali.Add("TopPostRam_Pos")
    _ListaCompletaCanali.Add("StbdRunnerRam_Pos")
    _ListaCompletaCanali.Add("StbdRunnerTank_Pos")
    _ListaCompletaCanali.Add("PortRunnerRam_Pos")
    _ListaCompletaCanali.Add("PortRunnerTank_Pos")
    _ListaCompletaCanali.Add("TopCtrlArm_Ang")
    _ListaCompletaCanali.Add("PortTravRam_Ang")
    _ListaCompletaCanali.Add("StbdTravRam_Ang")
    _ListaCompletaCanali.Add("Twist_Ang")
    _ListaCompletaCanali.Add("TopMastWindUnit_Ang")
    _ListaCompletaCanali.Add("BowWindUnit_Ang")
    _ListaCompletaCanali.Add("TopMastWindUnit_Speed")
    _ListaCompletaCanali.Add("BowWindUnit_Speed")
    _ListaCompletaCanali.Add("FCS_PortRam_Pos")
    _ListaCompletaCanali.Add("FCS_StbdRam_Pos")
    _ListaCompletaCanali.Add("FCS_PortRam_Spd")
    _ListaCompletaCanali.Add("FCS_StbdRam_Spd")
    _ListaCompletaCanali.Add("FCS_StorageLevel_Pos")
    _ListaCompletaCanali.Add("FCS_Oil_Temp")
    _ListaCompletaCanali.Add("FCS_Battery_Volt")
    _ListaCompletaCanali.Add("FCS_Motor_Temp")
    _ListaCompletaCanali.Add("FCS_MotorCtrl_Temp")
    _ListaCompletaCanali.Add("FCS_MotorCmd_RPM")
    _ListaCompletaCanali.Add("FCS_MotorAct_RPM")
    _ListaCompletaCanali.Add("FCS_Battery_Amp")
    _ListaCompletaCanali.Add("PortBulbRideHeight_AP_m")
    _ListaCompletaCanali.Add("StbdBulbRideHeight_AP_m")
    _ListaCompletaCanali.Add("PortTipInRideHeight_AP_m")
    _ListaCompletaCanali.Add("StbdTipInRideHeight_AP_m")
    _ListaCompletaCanali.Add("PortTipOutRideHeight_AP_m")
    _ListaCompletaCanali.Add("StbdTipOutRideHeight_AP_m")
    _ListaCompletaCanali.Add("RudderRideHeight_AP_m")
    _ListaCompletaCanali.Add("SinkMin_AP")
    _ListaCompletaCanali.Add("PortBulbSpeed_AP_kts")
    _ListaCompletaCanali.Add("StbdBulbSpeed_AP_kts")
    _ListaCompletaCanali.Add("RudderSpeed_AP_kts")
    _ListaCompletaCanali.Add("WaveForm_AP")
    _ListaCompletaCanali.Add("WaveCrest_AP")
    _ListaCompletaCanali.Add("WaveTrough_AP")
    _ListaCompletaCanali.Add("PortFlapInTgt_1_AP_deg")
    _ListaCompletaCanali.Add("PortFlapInTgt_2_AP_deg")
    _ListaCompletaCanali.Add("PortFlapOutTgt_1_AP_deg")
    _ListaCompletaCanali.Add("PortFlapOutTgt_2_AP_deg")
    _ListaCompletaCanali.Add("StbdFlapInTgt_1_AP_deg")
    _ListaCompletaCanali.Add("StbdFlapInTgt_2_AP_deg")
    _ListaCompletaCanali.Add("StbdFlapOutTgt_1_AP_deg")
    _ListaCompletaCanali.Add("StbdFlapOutTgt_2_AP_deg")
    _ListaCompletaCanali.Add("RudderRakeTgt_AP_deg")
    _ListaCompletaCanali.Add("ZDatum_AP")
    _ListaCompletaCanali.Add("SWH_AP")
    _ListaCompletaCanali.Add("TowLine_Load")
    _ListaCompletaCanali.Add("RudderYaw_Ang")
    _ListaCompletaCanali.Add("JibInOut_Ang")
    _ListaCompletaCanali.Add("JibInhaulRam_Ang")
    _ListaCompletaCanali.Add("JibOuthaulRam_Ang")
    _ListaCompletaCanali.Add("TwaBow")
    _ListaCompletaCanali.Add("TwsBow")
    _ListaCompletaCanali.Add("TwdBow")
    _ListaCompletaCanali.Add("ChazTwd")
    _ListaCompletaCanali.Add("ChazTws")
    _ListaCompletaCanali.Add("PortJibCar_Ang")
    _ListaCompletaCanali.Add("StbdJibCar_Ang")
    _ListaCompletaCanali.Add("PortArmIB1_Strain")
    _ListaCompletaCanali.Add("PortArmIB2_Strain")
    _ListaCompletaCanali.Add("PortArmIB3_Strain")
    _ListaCompletaCanali.Add("PortArmOB1_Strain")
    _ListaCompletaCanali.Add("PortArmOB2_Strain")
    _ListaCompletaCanali.Add("PortArmOB3_Strain")
    _ListaCompletaCanali.Add("StbdArmIB1_Strain")
    _ListaCompletaCanali.Add("StbdArmIB2_Strain")
    _ListaCompletaCanali.Add("StbdArmIB3_Strain")
    _ListaCompletaCanali.Add("StbdArmOB1_Strain")
    _ListaCompletaCanali.Add("StbdArmOB2_Strain")
    _ListaCompletaCanali.Add("StbdArmOB3_Strain")
    _ListaCompletaCanali.Add("PortWingIB1_Strain")
    _ListaCompletaCanali.Add("PortWingIB2_Strain")
    _ListaCompletaCanali.Add("PortWingIB3_Strain")
    _ListaCompletaCanali.Add("PortWingOB1_Strain")
    _ListaCompletaCanali.Add("PortWingOB2_Strain")
    _ListaCompletaCanali.Add("PortWingOB3_Strain")
    _ListaCompletaCanali.Add("StbdWingIB1_Strain")
    _ListaCompletaCanali.Add("StbdWingIB2_Strain")
    _ListaCompletaCanali.Add("StbdWingIB3_Strain")
    _ListaCompletaCanali.Add("StbdWingOB1_Strain")
    _ListaCompletaCanali.Add("StbdWingOB2_Strain")
    _ListaCompletaCanali.Add("StbdWingOB3_Strain")
    _ListaCompletaCanali.Add("RudderPort1_Strain")
    _ListaCompletaCanali.Add("ElevatorPort1_Strain")
    _ListaCompletaCanali.Add("ElevatorPort2_Strain")
    _ListaCompletaCanali.Add("RudderStbd1_Strain")
    _ListaCompletaCanali.Add("ElevatorStbd1_Strain")
    _ListaCompletaCanali.Add("ElevatorStbd2_Strain")
    _ListaCompletaCanali.Add("MDPed1EffectiveFunction")
    _ListaCompletaCanali.Add("PumpLine1_Press")
    _ListaCompletaCanali.Add("Pump1_Gear")
    _ListaCompletaCanali.Add("Pump1_RPM")
    _ListaCompletaCanali.Add("MDPed2EffectiveFunction")
    _ListaCompletaCanali.Add("PumpLine2_Press")
    _ListaCompletaCanali.Add("Pump2_Gear")
    _ListaCompletaCanali.Add("Pump2_RPM")
    _ListaCompletaCanali.Add("MDPed3EffectiveFunction")
    _ListaCompletaCanali.Add("PumpLine3_Press")
    _ListaCompletaCanali.Add("Pump3_Gear")
    _ListaCompletaCanali.Add("Pump3_RPM")
    _ListaCompletaCanali.Add("MDPed4EffectiveFunction")
    _ListaCompletaCanali.Add("PumpLine4_Press")
    _ListaCompletaCanali.Add("Pump4_Gear")
    _ListaCompletaCanali.Add("Pump4_RPM")
    _ListaCompletaCanali.Add("MainLine1_Press")
    _ListaCompletaCanali.Add("MainLine2_Press")
    _ListaCompletaCanali.Add("MainLine3_Press")
    _ListaCompletaCanali.Add("MainLine4_Press")
    _ListaCompletaCanali.Add("TopMastIMU_Hdg")
    _ListaCompletaCanali.Add("TopMastIMU_Pitch")
    _ListaCompletaCanali.Add("TopMastIMU_Roll")
    _ListaCompletaCanali.Add("PortJibCar_Pos")
    _ListaCompletaCanali.Add("StbdJibCar_Pos")
    _ListaCompletaCanali.Add("JibCunnLine_Load")
    _ListaCompletaCanali.Add("JibPennant_Load")
    _ListaCompletaCanali.Add("MainTravLine_Load")
    _ListaCompletaCanali.Add("StbdJibSheetLine_Load")
    _ListaCompletaCanali.Add("PortJibSheetLine_Load")
    _ListaCompletaCanali.Add("RideHeightSensor_m")
    _ListaCompletaCanali.Add("StbdJibSheetRam1_Press")
    _ListaCompletaCanali.Add("StbdJibSheetRam2_PressA")
    _ListaCompletaCanali.Add("StbdJibSheetRam2_PressB")
    _ListaCompletaCanali.Add("PortJibSheetRam1_Press")
    _ListaCompletaCanali.Add("PortJibSheetRam2_PressA")
    _ListaCompletaCanali.Add("PortJibSheetRam2_PressB")
    _ListaCompletaCanali.Add("Pump1SmallChamber")
    _ListaCompletaCanali.Add("Pump2SmallChamber")
    _ListaCompletaCanali.Add("PortJibSheetDI0_ToggleFastEase")
    _ListaCompletaCanali.Add("StbdJibSheetDI0_ToggleFastEase")
    _ListaCompletaCanali.Add("RudderStbd2_Strain")
    _ListaCompletaCanali.Add("RudderPort2_Strain")
    _ListaCompletaCanali.Add("ElevatorStbd2P45_Strain")
    _ListaCompletaCanali.Add("ElevatorStbd2M45_Strain")
    _ListaCompletaCanali.Add("ElevatorPort2P45_Strain")
    _ListaCompletaCanali.Add("ElevatorPort2M45_Strain")
    _ListaCompletaCanali.Add("V_BowWindUnitMeasuredRaw_MWA")
    _ListaCompletaCanali.Add("V_BowWindUnitMeasuredRaw_MWS")
    _ListaCompletaCanali.Add("V_TopMastWindUnitMeasuredRaw_MWA")
    _ListaCompletaCanali.Add("V_TopMastWindUnitMeasuredRaw_MWS")
    _ListaCompletaCanali.Add("TwaMast")
    _ListaCompletaCanali.Add("TwsMast")
    _ListaCompletaCanali.Add("TwdMast")
    _ListaCompletaCanali.Add("AwaMast")
    _ListaCompletaCanali.Add("AwsMast")
    _ListaCompletaCanali.Add("AwaBow")
    _ListaCompletaCanali.Add("AwsBow")
    _ListaCompletaCanali.Add("RudderPortLine_Load")
    _ListaCompletaCanali.Add("RudderStbdLine_Load")
    _ListaCompletaCanali.Add("FootCamber")
    _ListaCompletaCanali.Add("BotAngle_Ang")
    _ListaCompletaCanali.Add("PortTravRam_Press")
    _ListaCompletaCanali.Add("StbdTravRam_Press")
    _ListaCompletaCanali.Add("SC_MainSail")
    _ListaCompletaCanali.Add("SC_HeadSail")
    _ListaCompletaCanali.Add("SC_JibClewBoard")
    _ListaCompletaCanali.Add("SC_ConfigId")
    _ListaCompletaCanali.Add("TopMastCant_AP_deg")
    _ListaCompletaCanali.Add("TopMastCantTack_AP_deg")
    _ListaCompletaCanali.Add("TopMastRake_AP_deg")
    _ListaCompletaCanali.Add("TopMastTwist_AP_deg")
    _ListaCompletaCanali.Add("Elevator1Delta_Strain")
    _ListaCompletaCanali.Add("Elevator2Delta_Strain")

  End Sub

  Public Sub ImpostaCanaliOutputDefault()
    ImpostaCanaliOutput("SystemTime_DaySeconds,Bs,Tws,Twa,Awa")
  End Sub

  Private Sub ImpostaCanaliOutput(Canali As String)
    ImpostaCanaliOutput(Canali.Split(",").ToList)
  End Sub

  Public Sub EliminaDaListaFiltri(Filtro As clsFiltroParquetFinder)
    Dim Controllo As UserControlParquetFinderFilter = Nothing
    For Each ctrl In _ListaControlliFiltro
      If ctrl.VM Is Filtro Then
        Controllo = ctrl
        Exit For
      End If
    Next
    If Not Controllo Is Nothing Then _ListaControlliFiltro.Remove(Controllo)
  End Sub

  'Public Sub EliminaDaListaFiltri(Filtro As clsFiltroParquetFinder)
  '  _ListaFiltri.Remove(Filtro)
  'End Sub

  Private Sub ImpostaCanaliOutput(Canali As List(Of String))
    _ListaCanaliOutput.Clear()
    For Each canale In Canali
      _ListaCanaliOutput.Add(canale)
    Next
  End Sub

  Private Sub TrovaPeriodi()
    _ListaFilePeriodi.Clear()
    Dim ParquetFolder As New DirectoryInfo(_PathParquetFolder)
    Dim NrPeriodiTrovati As Integer = 1
    PgbMaxVal = ParquetFolder.GetDirectories.Count + 1
    PgbValue = 1
    PgbVisibility = Visibility.Visible
    For Each Fld In ParquetFolder.GetDirectories
      'cicla in tutte le cartelle contenenti i file parquet
      Dim DataSessione As DateTime = Nothing
      If DateTime.TryParseExact(Fld.Name, "yyyyMMdd", meCultureInfo, Globalization.DateTimeStyles.None, DataSessione) Then
        Dim PFs As New List(Of clsParquetFile)
        Dim PeriodsFiles As New List(Of String)
        For Each file In Fld.GetFiles
          If file.Name.StartsWith("Performance_") AndAlso file.Name.EndsWith(".ppf") Then
            'analizza i soli file Performance
            Dim pTmp As New clsParquetFile(file.FullName, False)
            'Dim pTmp As New clsParquetFile(file.FullName)
            'pTmp.LeggiFileParquet(True)
            If TR Is Nothing Then
              ' li carica tutti senza distinzione di data
              PFs.Add(pTmp)
              'Exit For
            Else
              If CInt(DataSessione.ToOADate) >= CInt(_TR.Start.ToOADate) Then
                If CInt(DataSessione.ToOADate) <= CInt(_TR.Finish.ToOADate) Then
                  PFs.Add(pTmp)
                  'Exit For
                End If
              End If
            End If
          ElseIf file.Name.EndsWith(".prd") Then
            PeriodsFiles.Add(file.FullName)
          ElseIf file.Name.EndsWith("_Periods.json") Then
            PeriodsFiles.Add(file.FullName)
          End If
        Next
        If PFs.Count > 0 Then
          For Each pf In PFs
            pf.LeggiFileParquet(False)
            For Each FilePeriodo In PeriodsFiles
              'carica i nuovi periodi dal file json
              Dim PeriodiTrovati = CercaPeriodiFile(FilePeriodo)
              If PeriodiTrovati.Count > 0 Then
                Dim PeriodiValidi As List(Of clsPeriod2021) = FiltroPeriodi(PeriodiTrovati, pf)
                If Not PeriodiValidi Is Nothing Then
                  Dim PeriodiUnivoci As New List(Of clsPeriod2021)
                  VerificaPeriodiUnivoci(PeriodiValidi, PeriodiUnivoci)
                  If PeriodiUnivoci.Count > 0 Then
                    NrPeriodiTrovati += PeriodiUnivoci.Count
                    _ListaFilePeriodi.Add(New clsParquetAndPeriods(pf, PeriodiUnivoci, _ListaFilePeriodi.Count))
                    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " #" & PeriodiUnivoci.Count & " Periods Found in: '" & pf.FilePath & "'")
                  Else
                    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " NO Valid Periods Found in: '" & pf.FilePath & "'")
                  End If
                End If
              End If
            Next
            'PF.DisposeReader()
            'PF = Nothing
          Next
        Else
          'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " NO VALID PERFORMANCE FILE FOUND IN:'" & Fld.Name & "'")
        End If
      Else
        'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " INVALID DATA FOLDER NAME: '" & Fld.Name & "'")
      End If
      PgbValue += 1
      PgbText = "1/3 " & (PgbValue / PgbMaxVal * 100).ToString("F0") & "%"

    Next

  End Sub

  Private Sub TrovaRighe()
    _ListaFilePeriodi.Clear()
    Dim ParquetFolder As New DirectoryInfo(_PathParquetFolder)

    PgbMaxVal = ParquetFolder.GetDirectories.Count + 1
    PgbValue = 1
    For Each Fld In ParquetFolder.GetDirectories
      'cicla in tutte le cartelle contenenti i file parquet
      Dim DataSessione As DateTime = Nothing
      If DateTime.TryParseExact(Fld.Name, "yyyyMMdd", meCultureInfo, Globalization.DateTimeStyles.None, DataSessione) Then
        Dim PF As clsParquetFile = Nothing
        Dim PeriodsFiles As New List(Of String)
        For Each file In Fld.GetFiles
          If file.Name.StartsWith("Performance_") AndAlso file.Name.EndsWith(".ppf") Then
            'analizza i soli file Performance
            Dim pTmp As New clsParquetFile(file.FullName) ', False)
            'pTmp.LeggiFileParquet(True)
            If TR Is Nothing Then
              PF = pTmp
              Exit For 'qui esce perché NON occorre trovare anche i file dei periodi
            Else
              If CInt(DataSessione.ToOADate) >= CInt(_TR.Start.ToOADate) Then
                If CInt(DataSessione.ToOADate) <= CInt(_TR.Finish.ToOADate) Then
                  PF = pTmp
                  Exit For 'qui esce perché NON occorre trovare anche i file dei periodi
                End If
              End If
            End If
          End If
        Next
        If Not PF Is Nothing Then
          PF.LeggiFileParquet(False)
          Dim Momenti As New List(Of DateTime)
          Dim Indici As List(Of Integer) = FiltroIndiciEtMomenti(PF, Momenti)
          If Not Indici Is Nothing Then
            _ListaFilePeriodi.Add(New clsParquetAndPeriods(PF, Momenti, Indici, _ListaFilePeriodi.Count))
            'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " #" & Momenti.Count & " Records found in: '" & PF.FilePath & "'")
          Else
            'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " NO Valid Records found in: '" & PF.FilePath & "'")
          End If
          PF = Nothing
        Else
          'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " NO VALID PERFORMANCE FILE FOUND IN:'" & Fld.Name & "'")
        End If
      Else
        'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " INVALID DATA FOLDER NAME: '" & Fld.Name & "'")
      End If
      PgbValue += 1
    Next

  End Sub

  Private Sub VerificaPeriodiUnivoci(PeriodiFileCorrente As List(Of clsPeriod2021), ByRef ListaCompletaPeriodi As List(Of clsPeriod2021))
    For Each p In PeriodiFileCorrente
      If ListaCompletaPeriodi.Where(Function(x) x.TR.IsSameRange(p.TR, True)).Count = 0 Then
        ListaCompletaPeriodi.Add(p)
      End If
    Next
  End Sub


  Public Function CercaPeriodiFile(PathFile As String) As List(Of clsPeriod2021)
    Dim l As New List(Of clsPeriod2021)
    Try
      If PathFile.EndsWith(".prd") Then
        ' cerca nei vecchi prd
        Stop
        'l = LoadPeriordsFromPrdFile(PathFile)
      ElseIf PathFile.EndsWith(".json") Then
        ' carica i file da eventuali file json esistenti
        Dim jj = clsKillerSeriale.LoadConfigurationGeneric(Of clsPeriodsJson)(PathFile)
        If Not jj Is Nothing Then
          If jj.ListaPeriodi.Count > 0 Then
            l = jj.ListaPeriodi.ToList
          End If
        End If
      End If
    Catch ex As Exception

    End Try
    Return l
  End Function

  'Public Function LoadPeriordsFromPrdFile(PathPrdFile As String) As List(Of clsPeriod2021)
  '  Dim Ltmp As New List(Of clsPeriod2021)
  '  Dim FilePrd = New clsSettings(PathPrdFile, AppConfig.Utente, "PeriodsList") 'pSuffisso) ' ObjFiles.PulisciCaratteriBastardi(ObjDataProvider.FI.Name) & "_PeriodsManager")
  '  Dim NodoPeriodi As Xml.XmlNode = FilePrd.CercaNodo("PeriodsList", clsSettings.eNodoSTD.ePeriods, True)
  '  If Not NodoPeriodi Is Nothing Then
  '    For Each Nodo As Xml.XmlNode In NodoPeriodi
  '      Dim Inizio As Double
  '      Dim Fine As Double
  '      Dim PeriodType As clsPeriod2021.ePeriodType = clsPeriod2021.ePeriodType.eUndefined
  '      Dim KeyMoment As DateTime = Nothing
  '      Dim Keys As String = ""
  '      Dim ShortDescription As String = ""
  '      Dim ExtendedDescription As String = ""
  '      'Dim SqlId As Integer = -1

  '      For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
  '        Select Case SottoNodo.Name
  '          Case "From"
  '            Inizio = SottoNodo.InnerText
  '          Case "To"
  '            Fine = SottoNodo.InnerText
  '          Case "PeriodType"
  '            PeriodType = CInt(SottoNodo.InnerText)
  '          Case "Keys"
  '            Keys = SottoNodo.InnerText.Trim
  '          Case "KeyMoment"
  '            Dim StrTmp As String = SottoNodo.InnerText
  '            If IsNumeric(StrTmp) AndAlso CDbl(StrTmp) > 0 Then
  '              KeyMoment = DateTime.FromOADate(StrTmp)
  '            Else
  '              KeyMoment = Nothing
  '            End If
  '          Case "ShortDescription"
  '            ShortDescription = SottoNodo.InnerText
  '          Case "ExtendedDescription"
  '            ExtendedDescription = SottoNodo.InnerText
  '            'Case "SqlId"
  '            '  SqlId = SottoNodo.InnerText
  '        End Select
  '      Next
  '      If Not PeriodType = clsPeriod2021.ePeriodType.eUndefined Then
  '        Dim TmRg As New clsTimeRange(DateTime.FromOADate(Inizio), DateTime.FromOADate(Fine))
  '        Dim PeriodoTmp As New clsPeriod2021(TmRg, PeriodType) ' , SqlId) ' , TWA, TWD, TWS, BS)
  '        PeriodoTmp.KeyMoment = KeyMoment
  '        PeriodoTmp.Keys = Keys
  '        PeriodoTmp.ExtendedDescription = ExtendedDescription
  '        PeriodoTmp.ShortDescription = ShortDescription
  '        Ltmp.Add(PeriodoTmp)
  '      End If
  '    Next
  '  End If
  '  Return Ltmp
  'End Function

  Private Function FiltroPeriodi(Periodi As List(Of clsPeriod2021), Parquet As clsParquetFile) As List(Of clsPeriod2021)
    Dim Risultato As New List(Of clsPeriod2021)
    Try
      Parquet.ParquetReader.Dispose()

      ImpostaDescrizionePeriodiAndTimeRange()

      For Each filtro In _ListaControlliFiltro
        If filtro.VM.ValidFilter Then
          filtro.VM.CaricaValoriCanali(Parquet)
          If filtro.VM.ValidFilter Then
            _StringaDescrizioneFiltro &= "_" & filtro.VM.DescrizioneFiltro("")
          End If
        End If
      Next

      For Each Periodo In Periodi
        If _ListaTipoPeriodi.Count = 0 OrElse _ListaTipoPeriodi.Where(Function(x) x = Periodo.PeriodType).Count > 0 Then
          For i As Integer = 0 To Parquet.FileParquet.TimeStamps.Count - 1
            If Parquet.FileParquet.TimeStamps(i) > Periodo.TR.Start Then
              'verifica la condizione ovvero che almeno un momento tutti i filtri devono essere validi
              Dim Valido As Boolean = True
              For Each Filtro In _ListaControlliFiltro
                If Filtro.VM.ValidFilter Then
                  If Not Filtro.VM.IsValid(i) Then
                    Valido = False
                    Exit For
                  End If
                End If
              Next
              If Valido Then
                Risultato.Add(Periodo)
                Exit For
              End If
              If Parquet.FileParquet.TimeStamps(i) >= Periodo.TR.Finish Then
                'timerange dl periodo superato passa al successivo
                Exit For
              End If
            End If
          Next
        End If
      Next
      Return Risultato
    Catch ex As Exception
      'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " ERROR reading file: " & Parquet.FilePath)
      Return Nothing
    End Try
  End Function

  Private Function FiltroIndiciEtMomenti(Parquet As clsParquetFile, ByRef Momenti As List(Of DateTime)) As List(Of Integer)
    Dim Risultato As New List(Of Integer)

    Try
      'Parquet.LeggiFileParquet(False)
      Dim ListaFiltri As New List(Of clsFiltroParquetFinder)
      ListaFiltri.Add(New clsFiltroParquetFinder("Bs", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMin, 20, 0, False, Me))
      ListaFiltri.Add(New clsFiltroParquetFinder("Tws", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMin, 11, 0, False, Me))
      ListaFiltri.Add(New clsFiltroParquetFinder("Awa", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMin, 11, 0, False, Me))
      ListaFiltri.Add(New clsFiltroParquetFinder("Twa", Parquet, clsFiltroParquetFinder.eTipoFiltro.eBetween, 40, 60, True, Me))
      ListaFiltri.Add(New clsFiltroParquetFinder("FCS_PortCant_Ang", "FCS_StbdCant_Ang", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMax, clsFiltroParquetFinder.eRelazioneTraCampi.eOneOnly, 0, 25, False, Me))
      Parquet.ParquetReader.Dispose()


      _StringaDescrizioneFiltro = "AllRows"
      DescrizioneFiltroTimeRange()


      For Each filtro In ListaFiltri
        If filtro.ValidFilter Then
          _StringaDescrizioneFiltro &= "_" & filtro.DescrizioneFiltro("")
        End If
      Next

      For i As Integer = 0 To Parquet.FileParquet.TimeStamps.Count - 1
        'verifica la condizione ovvero che almeno un momento tutti i filtri devono essere validi
        Dim Valido As Boolean = True
        For Each Filtro In ListaFiltri
          If Filtro.ValidFilter Then
            If Not Filtro.IsValid(i) Then
              Valido = False
              Exit For
            End If
          End If
        Next
        If Valido Then
          Risultato.Add(i)
          Momenti.Add(Parquet.FileParquet.TimeStamps(i))
          'Exit For
        End If
      Next
      If Risultato.Count = 0 Then Return Nothing
      Return Risultato
    Catch ex As Exception
      'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " ERROR reading file: " & Parquet.FilePath)
      Return Nothing
    End Try
  End Function

  Private Sub ImpostaManualmenteTipoPeriodi()

    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eAcceleration)
    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eGybe)
    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eTack)
    _ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eStraightLineVmg)

    '_ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eBrAway)
    '_ListaTipoPeriodi.Add(clsPeriod2021.ePeriodType.eFinish)
    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eMultiManoeuvres)
    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eRoundUp)
    '_ListaTipoPeriodi.Add(clsPeriod2020.ePeriodType.eStart)

  End Sub

  Private Sub ImpostaDescrizionePeriodiAndTimeRange()
    _StringaDescrizioneFiltro = ""
    If _ListaTipoPeriodi.Count = 0 Then
      _StringaDescrizioneFiltro = "AllPeriodTypes"
    Else
      For Each Tipo In _ListaTipoPeriodi
        _StringaDescrizioneFiltro &= Tipo.ToString.Substring(1) & ""
      Next
    End If
    DescrizioneFiltroTimeRange()
  End Sub

  Private Sub DescrizioneFiltroTimeRange()
    If TR Is Nothing Then
      _StringaDescrizioneFiltro &= "_WholeArchive_"
    Else
      _StringaDescrizioneFiltro &= "_From" & TR.Start.ToString("yyyyMMdd") & "To" & TR.Finish.ToString("yyyyMMdd") & "_"
    End If
  End Sub

  Public Sub EsportaPeriodi()
    _BasicChannelsOutput.Lista.Clear()
    _PeriodiDaEsportare.Clear()
    PgbMaxVal = _ListaFilePeriodi.Count + 1
    PgbValue = 1
    For Each fp In _ListaFilePeriodi.OrderBy(Function(x) x.Parquet.FileParquet.TimeRangeRealDateTime.Start).ToList
      AggiungiCanaliFileParquet(fp)
    Next

    For Each fp In _ListaFilePeriodi.OrderBy(Function(x) x.Parquet.FileParquet.TimeRangeRealDateTime.Start).ToList
      RiempiBasicChannelsPeriodi(fp)
      PgbValue += 1
      PgbText = "2/3 " & (PgbValue / PgbMaxVal * 100).ToString("F0") & "%"
      fp.Parquet.ParquetReader.Dispose()

    Next
    'Stop
    SalvaParquet(StandardFolderPath) '_PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & "\")
  End Sub

  Private Function StandardFolderPath() As String
    Return _PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & _StringaDescrizioneFiltro & "\"
  End Function

  Public Sub EsportaRighe(Output As eOutput, ListaCanaliOutput As List(Of String))
    _BasicChannelsOutput.Lista.Clear()
    _BasicChannelsOutput.Lista.Add(New clsBasicChannelFinder("SystemTime_DaySeconds", True)) 'il canale day seconds deve essere comunque presente
    For Each canale In ListaCanaliOutput
      If _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).Count = 0 Then
        _BasicChannelsOutput.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      End If
    Next

    PgbMaxVal = _ListaFilePeriodi.Count + 1
    PgbValue = 1
    For Each fp In _ListaFilePeriodi
      Select Case Output
        Case eOutput.ePeriods
          Stop
          RiempiBasicChannelsCreaPeriodiDaMomenti(fp, 30)
        Case eOutput.eStatitics
          RiempiBasicChannelsCreaPeriodiDaMomenti(fp, ListaCanaliOutput)
        Case eOutput.eData
          RiempiBasicChannelsCreaPeriodiDaMomenti(fp, ListaCanaliOutput)
      End Select
      PgbValue += 1
    Next

    Select Case Output
      Case eOutput.ePeriods
        SalvaParquet(StandardFolderPath) 'SalvaParquet(_PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & "\")
      Case eOutput.eStatitics
        SalvaStats(StandardFolderPath, ListaCanaliOutput)'(_PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & "\", ListaCanaliOutput)
      Case eOutput.eData
        SalvaData(StandardFolderPath, ListaCanaliOutput) '(_PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & "\", ListaCanaliOutput)
    End Select
  End Sub

  Public Sub EsportaRighePeriodo()
    _BasicChannelsOutput.Lista.Clear()
    For Each fp In _ListaFilePeriodi
      RiempiBasicChannelsCreaPeriodiDaMomenti(fp, 30)
      SalvaParquet(StandardFolderPath) 'SalvaParquet(_PathParquetFolder & "\SearchResults\" & Now.ToString("yyyyMMdd") & "\")
    Next
  End Sub


  Private Sub AggiungiCanaliFileParquet(FP As clsParquetAndPeriods)
    Dim BasicChannelsParquetCorrente As New clsBasicChannelsFinder
    For Each canale In FP.Parquet.FileParquet.Intestazioni
      BasicChannelsParquetCorrente.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      If _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).Count = 0 Then
        'la aggiunge alla lista di tutti i file se non esiste
        _BasicChannelsOutput.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      End If
    Next
  End Sub


  Private Sub RiempiBasicChannelsPeriodi(FP As clsParquetAndPeriods)
    'Console.WriteLine("a " & Now.ToString("ss.ffff"))
    Dim BasicChannelsParquetCorrente As New clsBasicChannelsFinder
    'carica i valori di tutti i canali del file corrente
    For Each canale In FP.Parquet.FileParquet.Intestazioni
      BasicChannelsParquetCorrente.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      Dim PqDataField As Parquet.Data.DataField = FP.Parquet.ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = canale).FirstOrDefault
      If BasicChannelsParquetCorrente.Lista.Last.IsDT Then
        BasicChannelsParquetCorrente.Lista.Last.ValoriDT = FP.Parquet.FileParquet.TimeStamps.ToList
      Else
        Dim a As Double() = RiempiCanaleArray(FP, canale, FP.Parquet.ParquetReader, PqDataField, Nothing)
        BasicChannelsParquetCorrente.Lista.Last.ArrayValori = a 'RiempiCanale(FP, canale, FP.Parquet.ParquetReader, PqDataField, Nothing)
      End If
    Next
    'Console.WriteLine("b " & Now.ToString("ss.ffff"))

    ' il json dei periodi contiene tutti i periodi del giorno
    ' essendoci più parquet dello stesso giorno vengono caricati i soli periodi relativi a quel file specifico
    'Console.WriteLine(FP.PeriodsTR.StringaPeriodo)
    'For i As Integer = 0 To 100
    '  If FP.Parquet.FileParquet.Momenti(i).ToOADate > 0 Then
    '    Console.Write(FP.Parquet.FileParquet.Momenti(i) & " ")
    '    Exit For
    '  End If
    'Next
    'For i As Integer = 0 To 100
    '  If FP.Parquet.FileParquet.Momenti(FP.Parquet.FileParquet.Momenti.Count - 1 - i).ToOADate > 0 Then
    '    Console.WriteLine(FP.Parquet.FileParquet.Momenti(FP.Parquet.FileParquet.Momenti.Count - 1 - i) & " ")
    '    Exit For
    '  End If
    'Next
    'Console.WriteLine(FP.PeriodsTR.StringaPeriodo)
    Dim IndiciItervallo As New List(Of clsIndexIndex)
    For Each Periodo In FP.Periods.OrderBy(Function(x) x.TR.Start)
      Dim IdIniziale As Integer = 0
      Dim IdFinale As Integer = 0
      Select Case Periodo.PeriodType
        Case clsPeriod2021.ePeriodType.eGybe, clsPeriod2021.ePeriodType.eTack
          ' importa 60 secondi prima e dopo al key moment, prima serve anche per il calcolo dell investment
          IdIniziale = TrovaIndice(Periodo.KeyMoment.AddSeconds(-60), FP.Parquet.FileParquet.TimeStamps)
          IdFinale = TrovaIndice(Periodo.KeyMoment.AddSeconds(60), FP.Parquet.FileParquet.TimeStamps)
        Case clsPeriod2021.ePeriodType.eAcceleration
          ' importa almeno i due minuti successivi all inizio della selezione manuale e dai 20 secondi precedenti
          IdIniziale = TrovaIndice(Periodo.TR.Start.AddSeconds(-20), FP.Parquet.FileParquet.TimeStamps)
          Dim IdF As Integer = TrovaIndice(Periodo.TR.Start.AddSeconds(120), FP.Parquet.FileParquet.TimeStamps)
          IdFinale = TrovaIndice(Periodo.TR.Finish, FP.Parquet.FileParquet.TimeStamps)
          IdFinale = System.Math.Max(IdFinale, IdF)
        Case Else
          IdIniziale = TrovaIndice(Periodo.TR.Start, FP.Parquet.FileParquet.TimeStamps)
          IdFinale = TrovaIndice(Periodo.TR.Finish, FP.Parquet.FileParquet.TimeStamps)
      End Select
      If IdIniziale = IdFinale Then
        'Console.WriteLine(Periodo.PeriodType.ToString & " " & Periodo.TimeRange.Start & " " & Periodo.TimeRange.Finish)
      Else
        Dim VerificaUnivocità = _PeriodiDaEsportare.Where(Function(x) System.Math.Abs(x.TR.Start.Subtract(Periodo.TR.Start).TotalSeconds) < 1 AndAlso System.Math.Abs(x.TR.Finish.Subtract(Periodo.TR.Finish).TotalSeconds) < 1).ToList
        If VerificaUnivocità.Count = 0 Then
          _PeriodiDaEsportare.Add(Periodo)
          IndiciItervallo.Add(New clsIndexIndex(IdIniziale, IdFinale))
        End If
      End If
    Next
    For Each OutputChannel In _BasicChannelsOutput.Lista
      Dim UltimoIdxInserito As Integer = -1
      If OutputChannel.IsDT Then
        For Each Idx In IndiciItervallo
          For i As Integer = Idx.Inizio To Idx.Fine
            If i > UltimoIdxInserito Then ' per evitare di mettere due volte righe che facevano parte dell  intervallo precedente
              OutputChannel.ValoriDT.Add(FP.Parquet.FileParquet.TimeStamps(i))
              UltimoIdxInserito = i
            End If
          Next
        Next
      Else
        'Dim CanaleSource = BasicChannelsParquetCorrente.Lista.Where(Function(x) x.Nome = OutputChannel.Nome).ToList
        Dim CanaleSource = BasicChannelsParquetCorrente.Lista.Where(Function(x) x.Nome = OutputChannel.Nome).FirstOrDefault
        If CanaleSource Is Nothing Then
          'se non esiste aggiunge un nan
          For Each Idx In IndiciItervallo
            For i As Integer = Idx.Inizio To Idx.Fine
              If i > UltimoIdxInserito Then ' per evitare di mettere due volte righe che facevano parte dell  intervallo precedente
                OutputChannel.Valori.Add(Double.NaN)
                UltimoIdxInserito = i
              End If
            Next
          Next
        Else
          'se esiste aggiunge il valore
          'OutputChannel.Valori.AddRange(CanaleSource.ArrayValori.ToList)
          For Each Idx In IndiciItervallo
            For i As Integer = Idx.Inizio To Idx.Fine
              If i > UltimoIdxInserito Then ' per evitare di mettere due volte righe che facevano parte dell  intervallo precedente
                OutputChannel.Valori.Add(CanaleSource.ArrayValori(i))
                UltimoIdxInserito = i
              End If
            Next
          Next
        End If
      End If
    Next
    'Console.WriteLine("c " & Now.ToString("ss.ffff"))
  End Sub

  Private Sub RiempiBasicChannelsCreaPeriodiDaMomenti(FP As clsParquetAndPeriods, IntervalloSecondiAntePost As Integer)
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " RiempiVariabiliDi: " & FP.Parquet.FilePath)

    Dim strTmp As String = ""
    'FP.Parquet.LeggiFileParquet(False)
    Dim BasicChannelsParquetCorrente As New clsBasicChannelsFinder

    For Each canale In FP.Parquet.FileParquet.Intestazioni
      BasicChannelsParquetCorrente.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      If _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).Count = 0 Then
        'la aggiunge alla lista di tutti i file se non esiste
        _BasicChannelsOutput.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      End If
      Dim PqDataField As Parquet.Data.DataField = FP.Parquet.ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = canale).FirstOrDefault
      If BasicChannelsParquetCorrente.Lista.Last.IsDT Then
        BasicChannelsParquetCorrente.Lista.Last.ValoriDT = FP.Parquet.FileParquet.TimeStamps.ToList
      Else
        BasicChannelsParquetCorrente.Lista.Last.Valori = RiempiCanale(FP, canale, FP.Parquet.ParquetReader, PqDataField, Nothing)
      End If

    Next


    Dim IndiciValidi(FP.Parquet.FileParquet.TimeStamps.Count - 1) As Boolean
    For Each indice In IndiciValidi
      indice = False
    Next
    For Each M In FP.Momenti
      Dim IndiceAnte As Integer = TrovaIndice(M.AddSeconds(-IntervalloSecondiAntePost), FP.Momenti.ToArray)
      Dim IndicePost As Integer = TrovaIndice(M.AddSeconds(IntervalloSecondiAntePost), FP.Momenti.ToArray)
      'marca come true tutti i momenti nell ' intorno di quello filtro
      For i As Integer = IndiceAnte To IndicePost
        IndiciValidi(i) = True
      Next
    Next

    'crea dei periodi di continuitá
    Dim Vprev As Boolean = IndiciValidi(0)
    Dim Inizio As DateTime = Nothing
    Dim vTwa = BasicChannelsParquetCorrente.Lista.Where(Function(x) x.Nome = "Twa").FirstOrDefault.Valori.ToArray
    Dim vBs = BasicChannelsParquetCorrente.Lista.Where(Function(x) x.Nome = "Bs").FirstOrDefault.Valori.ToArray
    FP.Periods.Clear()
    For i As Integer = 0 To IndiciValidi.Count - 1
      Dim Vcurr As Boolean = IndiciValidi(i)
      If Vprev Then
        'il valore precedente era true
        If Not Vcurr Then
          'chiude il periodo
          Dim tr As New clsTimeRange(Inizio, FP.Parquet.FileParquet.TimeStamps(i))
          Dim p As New clsPeriod2021(tr, clsPeriod2021.ePeriodType.eUndefined)
          'Dim p As New clsPeriod2021(tr, clsPeriod2021.ePeriodType.eUndefined, vTwa.Cast(Of Double), vBs.Cast(Of Double), FP.Momenti.ToArray)
          FP.Periods.Add(p)
        End If
      Else
        'il valore precedente era false
        If Vcurr Then
          'inizia un nuovo periodo
          Inizio = FP.Parquet.FileParquet.TimeStamps(i)
        End If
      End If
    Next
    If Vprev Then
      Dim tr As New clsTimeRange(Inizio, FP.Parquet.FileParquet.TimeStamps.Last)
      Dim p As New clsPeriod2021(tr, clsPeriod2021.ePeriodType.eUndefined)
      'Dim p As New clsPeriod2021(tr, clsPeriod2020.ePeriodType.eUndefined, vTwa.Cast(Of Double), vBs.Cast(Of Double), FP.Momenti.ToArray)
      FP.Periods.Add(p)
    End If


    '         For Each Periodo In FP.Periods.OrderBy(Function(x) x.TimeRange.Start)

    Dim IdCampoDaySeconds As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = "SystemTime_DaySeconds")
    Dim DaySecondsChannel As clsBasicChannelFinder = _BasicChannelsOutput.Lista(IdCampoDaySeconds)
    Dim DaySecondsParquetCorrente As clsBasicChannelFinder = BasicChannelsParquetCorrente.Lista(FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = "SystemTime_DaySeconds"))
    Dim ListaIndiciValidi As New List(Of Integer)
    For Each Periodo In FP.Periods.OrderBy(Function(x) x.TR.Start)
      'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " RiempiValoriDT: " & Periodo.PeriodType.ToString & " " & Periodo.TimeRange.StringaPeriodo)
      Dim IdIniziale As Integer = TrovaIndice(Periodo.TR.Start, FP.Parquet.FileParquet.TimeStamps)
      Dim IdFinale As Integer = TrovaIndice(Periodo.TR.Finish, FP.Parquet.FileParquet.TimeStamps)
      For i As Integer = IdIniziale To IdFinale
        If DaySecondsChannel.ValoriDT.Count = 0 OrElse DaySecondsParquetCorrente.ValoriDT(i) > DaySecondsChannel.ValoriDT.Last Then
          ' aggiunge il valore solo se nuovo e non sovrappsto con momenti di periodi precedenti
          DaySecondsChannel.ValoriDT.Add(DaySecondsParquetCorrente.ValoriDT(i))
          ListaIndiciValidi.Add(i)
        End If
      Next
    Next

    For Each BasicChannel In _BasicChannelsOutput.Lista
      Dim IdCampo As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = BasicChannel.Nome)
      If Not BasicChannel.IsDT Then
        For Each id In ListaIndiciValidi
          If IdCampo = -1 Then
            BasicChannel.Valori.Add(Double.NaN)
          Else
            BasicChannel.Valori.Add(BasicChannelsParquetCorrente.Lista(IdCampo).Valori(id))
          End If
        Next
      End If

    Next

    FP.Parquet.ParquetReader.Dispose()

  End Sub

  Private Sub RiempiBasicChannelsCreaPeriodiDaMomenti(FP As clsParquetAndPeriods, CanaliDaCaricare As List(Of String))
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " RiempiVariabiliDi: " & FP.Parquet.FilePath)

    Dim strTmp As String = ""
    Dim BasicChannelsParquetCorrente As New clsBasicChannelsFinder

    For Each canale In FP.Parquet.FileParquet.Intestazioni
      'vanno creati tutti i canali (altrimenti salta l índice) ma riempiti solo quelli da esportare
      BasicChannelsParquetCorrente.Lista.Add(New clsBasicChannelFinder(canale, canale = "SystemTime_DaySeconds"))
      If Not CanaliDaCaricare.Where(Function(x) x = canale).FirstOrDefault Is Nothing Then
        'il canale del file parquet corrente viene elaborato solo se presente nella lista dei canali da esportare 
        Dim PqDataField As Parquet.Data.DataField = FP.Parquet.ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = canale).FirstOrDefault
        If BasicChannelsParquetCorrente.Lista.Last.IsDT Then
          BasicChannelsParquetCorrente.Lista.Last.ValoriDT = FP.Parquet.FileParquet.TimeStamps.ToList
        Else
          BasicChannelsParquetCorrente.Lista.Last.Valori = RiempiCanale(FP, canale, FP.Parquet.ParquetReader, PqDataField, Nothing)
        End If
      End If
    Next

    'gira nei canali da esportare
    For Each BasicChannel In _BasicChannelsOutput.Lista
      Console.Write(BasicChannel.Nome & " ")
      'se il canale esiste nel file parquet corrente carica i dati altrimenti mette tutti nan
      Dim IdCampo As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = BasicChannel.Nome)
      If IdCampo = -1 Then
        If BasicChannel.IsDT Then
          For Each idx In FP.Indici
            BasicChannel.ValoriDT.Add(Nothing)
          Next
        Else
          For Each idx In FP.Indici
            BasicChannel.Valori.Add(Double.NaN)
          Next
        End If
      Else
        If BasicChannel.IsDT Then
          For Each idx In FP.Indici
            BasicChannel.ValoriDT.Add(BasicChannelsParquetCorrente.Lista(IdCampo).ValoriDT(idx))
          Next
        Else
          For Each idx In FP.Indici
            BasicChannel.Valori.Add(BasicChannelsParquetCorrente.Lista(IdCampo).Valori(idx))
          Next
        End If
      End If
    Next
    'Console.WriteLine()
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Fine RiempiVariabiliDi: " & FP.Parquet.FilePath)

    FP.Parquet.ParquetReader.Dispose()

  End Sub

  Public Function TrovaIndice(Momento As DateTime, ValoriDT As Date()) As Integer
    If Momento = Nothing Then Return -1
    Dim L As Integer = 0
    Dim U As Integer = ValoriDT.Count - 1
    Dim DTtmp As DateTime
    Dim PrevIndex As Integer = 0
    Dim IndexTmp As Integer
    'Dim delta As Integer = -1

    Do While U - L > 1 '  AndAlso PrevIndex = IndexTmp
      IndexTmp = CInt((U + L) / 2)
      DTtmp = ValoriDT(IndexTmp)


      Do While IndexTmp > L AndAlso IndexTmp < U
        If ValoriDT(IndexTmp).ToOADate = 0 Then
          IndexTmp -= 1
          DTtmp = ValoriDT(IndexTmp)
        Else
          Exit Do
        End If
      Loop
      If Momento = DTtmp Then
        Return IndexTmp
      ElseIf Momento > DTtmp Then
        L = IndexTmp
      Else
        U = IndexTmp
      End If

      If PrevIndex = IndexTmp Then Exit Do
      PrevIndex = IndexTmp
    Loop

    Dim UpTmp As Integer = L + 1
    If UpTmp >= ValoriDT.Count - 1 Then

    Else
      Do
        If ValoriDT(UpTmp).ToOADate = 0 Then
          UpTmp += 1
          If UpTmp >= ValoriDT.Count - 1 Then
            Exit Do
          End If
        Else
          Exit Do
        End If
      Loop
    End If
    U = UpTmp
    If System.Math.Abs(Momento.Subtract(ValoriDT(L)).TotalSeconds) < System.Math.Abs(Momento.Subtract(ValoriDT(U)).TotalSeconds) Then
      IndexTmp = L
    Else
      IndexTmp = U
    End If
    Dim d As TimeSpan = Momento.Subtract(ValoriDT(IndexTmp))
    If System.Math.Abs(d.TotalSeconds) > 1 Then
      'Dim Minimatrix = ValoriDT.Skip(L - 100).Take(200).ToList
      'Console.WriteLine("Delta Mills:" & d.TotalMilliseconds & " Search:" & Momento.ToString("dd/MM/yyyy HH:mm:ss.fff") & " Find:" & ValoriDT(IndexTmp).ToString("dd/MM/yyyy HH:mm:ss.fff"))
    Else
      'Console.WriteLine(d.TotalMilliseconds)
    End If
    Return IndexTmp
  End Function

  'Public Function TrovaIndice(Momento As DateTime, ValoriDT As Date()) As Integer
  '  If Momento = Nothing Then Return -1
  '  Dim L As Integer = 0
  '  Dim U As Integer = ValoriDT.Count - 1
  '  Dim DTtmp As DateTime
  '  Dim IndexTmp As Integer
  '  Do While U - L > 1
  '    IndexTmp = CInt((U + L) / 2)
  '    DTtmp = ValoriDT(IndexTmp)
  '    If DTtmp = Nothing Then
  '      Dim i As Integer = 1
  '      Do While IndexTmp > 0 AndAlso IndexTmp < ValoriDT.Count
  '        DTtmp = ValoriDT(IndexTmp - i)
  '        If Not DTtmp = Nothing Then
  '          IndexTmp -= i
  '          Exit Do
  '        End If
  '        DTtmp = ValoriDT(IndexTmp + i)
  '        If Not DTtmp = Nothing Then
  '          IndexTmp += i
  '          Exit Do
  '        End If
  '        i += 1
  '      Loop
  '      If Momento = DTtmp Then
  '        Return IndexTmp
  '      ElseIf Momento > DTtmp Then
  '        If L = IndexTmp Then Exit Do
  '        L = IndexTmp
  '      Else
  '        If U = IndexTmp Then Exit Do
  '        U = IndexTmp
  '      End If
  '    Else
  '      If Momento = DTtmp Then
  '        Return IndexTmp
  '      ElseIf Momento > DTtmp Then
  '        L = IndexTmp
  '      Else
  '        U = IndexTmp
  '      End If
  '    End If
  '  Loop
  '  Do
  '    If Not ValoriDT(L) = Nothing Then
  '      Exit Do
  '    End If
  '    If L = 0 Then Exit Do
  '    L -= 1
  '  Loop
  '  Do
  '    If Not ValoriDT(U) = Nothing Then
  '      Exit Do
  '    End If
  '    If U = ValoriDT.Count - 1 Then Exit Do
  '    U += 1
  '  Loop
  '  If Momento - ValoriDT(L) < Momento - ValoriDT(U) Then
  '    IndexTmp = L
  '  Else
  '    IndexTmp = U
  '  End If
  '  Return IndexTmp
  'End Function

  Private Function RiempiCanale(FP As clsParquetAndPeriods, NomeCanale As String, ParquetReader As Parquet.ParquetReader, PqDataFiled As Parquet.Data.DataField, ListaToBeChecked As List(Of String)) As List(Of Double?)
    If ListaToBeChecked Is Nothing OrElse ListaToBeChecked.Where(Function(x) x = NomeCanale).Count > 0 Then
      Dim IdCampo As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = NomeCanale)
      If IdCampo = -1 Then
        'questo canale non esiste nel file corrente
        Return Nothing
      Else
        PqDataFiled = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = NomeCanale).FirstOrDefault
        Dim Temp1 = ParquetReader.OpenRowGroupReader(0)
        Dim Temp2 = Temp1.ReadColumn(PqDataFiled).Data
        Dim Temp3(Temp2.Length - 1)
        Array.Copy(Temp2, Temp3, Temp2.Length - 1)
        Return Temp3.Cast(Of Double?).ToList
      End If
    Else
      Return Nothing
    End If

  End Function

  Private Function RiempiCanaleArray(FP As clsParquetAndPeriods, NomeCanale As String, ParquetReader As Parquet.ParquetReader, PqDataFiled As Parquet.Data.DataField, ListaToBeChecked As List(Of String)) As Double()
    If ListaToBeChecked Is Nothing OrElse ListaToBeChecked.Where(Function(x) x = NomeCanale).Count > 0 Then
      Dim IdCampo As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = NomeCanale)
      If IdCampo = -1 Then
        'questo canale non esiste nel file corrente
        Return Nothing
      Else
        PqDataFiled = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = NomeCanale).FirstOrDefault
        Dim Group = ParquetReader.OpenRowGroupReader(0)
        Dim Data = Group.ReadColumn(PqDataFiled).Data
        Dim Ret = CType(Data, Double())
        Return Ret
      End If
    End If
    Return Nothing
  End Function

  Private Sub SalvaStats(NewFileFolder As String, ListaCanaliOutput As List(Of String))
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva CSV Statistic")
    Dim pathCsv As String = NewFileFolder & "Distribution_" & _StringaDescrizioneFiltro & ".csv"

    If Not System.IO.Directory.Exists(NewFileFolder) Then
      System.IO.Directory.CreateDirectory(NewFileFolder)
    End If

    If System.IO.File.Exists(pathCsv) Then
      For i As Integer = 1 To 999
        If Not System.IO.File.Exists(NewFileFolder & "Distribution_" & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".csv") Then
          pathCsv = NewFileFolder & "Distribution_" & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".csv"
          Exit For
        End If
      Next
    End If

    'crea la lista dei canali da esportare
    Dim ListaCanaliDaEsportare As New List(Of clsBasicChannelFinder)
    For Each canale In ListaCanaliOutput
      Dim CanaleCorrente = _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).FirstOrDefault
      If Not CanaleCorrente Is Nothing Then
        ListaCanaliDaEsportare.Add(CanaleCorrente)
      End If
    Next

    'scive il filtro ed i valori base del primo canale double
    Dim Righe As New List(Of String)
    Dim Riga As String = "Data Filter:" & vbTab & _StringaDescrizioneFiltro
    Righe.Add(Riga.TrimEnd(vbTab))

    Dim CanalePrimario As String = ""
    PgbMaxVal = ListaCanaliOutput.Count + 1
    PgbValue = 1
    For Each canale In ListaCanaliOutput
      Dim CanaleCorrente = _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).FirstOrDefault
      If Not CanaleCorrente Is Nothing Then
        If Not CanaleCorrente.IsDT Then
          CanalePrimario = CanaleCorrente.Nome
          Riga = CanaleCorrente.Nome & vbCrLf
          Dim Valori = CanaleCorrente.Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
          If Valori.Count > 0 Then
            Select Case CanaleCorrente.Nome
              Case "Twa", "Awa"
                Valori = Valori.Select(Function(x) System.Math.Abs(x.Value))
            End Select
            Riga &= "Avg:" & vbTab & CDbl(Valori.Average).ToString("F2") & vbCrLf
            Riga &= "Max:" & vbTab & CDbl(Valori.Max).ToString("F2") & vbCrLf
            Riga &= "Min:" & vbTab & CDbl(Valori.Min).ToString("F2") & vbCrLf
            Dim StandardDeviation As Double = 0
            Dim V(Valori.Count - 1) As Double
            For i As Integer = 0 To V.Count - 1
              V(i) = Valori(i)
            Next
            alglib.basestat.sampleadev(V, Valori.Count, StandardDeviation)
            Riga &= "SD:" & vbTab & StandardDeviation.ToString("F2") & vbCrLf
            Riga &= CanalePrimario & " Distribution" & vbTab & vbTab & vbTab & vbTab
            Dim ListaCanaliSecondari = ListaCanaliDaEsportare.Where(Function(x) Not x.IsDT AndAlso Not x.Nome = CanalePrimario).ToList
            For Each cs In ListaCanaliSecondari
              Riga &= cs.Nome & vbTab & vbTab & vbTab & vbTab & vbTab
            Next
            Righe.Add(Riga.TrimEnd(vbTab))

            Riga = "Range Avg" & vbTab & "Samples" & vbTab & "Percentage" & vbTab & vbTab
            For Each cs In ListaCanaliSecondari
              Riga &= "Avg" & vbTab & "Max" & vbTab & "Min" & vbTab & "SD" & vbTab & vbTab
            Next
            Righe.Add(Riga.TrimEnd(vbTab))

            Dim Intervalli As Integer = 20
            Dim H As Double = (Valori.Max - Valori.Min) / Intervalli

            If Not Valori Is Nothing AndAlso Valori.Count > 0 Then

              Dim Ysum As Double = 0
              Dim coppie As New List(Of clsDoubleXY)
              For ii As Integer = 0 To Intervalli - 1
                Dim x As Double = Valori.Min + H * 0.5 + (H * ii)
                Dim y = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, V)
                Ysum += y
                coppie.Add(New clsDoubleXY(x, y))
              Next
              For Each coppia In coppie
                Riga = coppia.X.ToString("F2") & vbTab & (coppia.Y / Ysum * V.Count).ToString("F0") & vbTab & (coppia.Y / Ysum * 100).ToString("F1") & vbTab
                'gira tra i canali e riempie gli array dei valori che rientrano nel range corrente del canale primario
                For Each c In ListaCanaliSecondari
                  c.ValoriTmp.Clear()
                  For i As Integer = 0 To Valori.Count - 1
                    If Valori(i) >= (coppia.X - H / 2) AndAlso Valori(i) <= (coppia.X + H / 2) Then
                      c.ValoriTmp.Add(c.Valori(i))
                    End If
                  Next
                  'stampa la distribuzione
                  Riga &= vbTab & RigaCanaleSecondario(c.ValoriTmp.ToArray)
                Next
                Righe.Add(Riga.TrimEnd(vbTab))

              Next
            End If
            Exit For
          End If
        End If
      End If
      PgbValue += 1
    Next

    ResultTxt = "Full data set copied in the file '" & pathCsv & "'" & vbCrLf
    ResultTxt &= "Statistic Result:" & vbCrLf
    For i As Integer = 0 To Righe.Count - 1
      ResultTxt &= Righe(i) & vbCrLf
    Next

    Dim fsTmp As New System.IO.StreamWriter(pathCsv)
    If Righe.Count > 0 Then
      fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
    End If
    Dim pfi As New System.IO.FileInfo(pathCsv)
    fsTmp.Close()
    fsTmp.Dispose()
    ApriExplorer(pathCsv)


    ''gira tra le righe 
    ''intestazioni
    'Riga = "DateTime" & vbTab
    'For Each canale In ListaCanaliDaEsportare
    '  If Not canale.IsDT Then
    '    If Not canale.Nome = CanalePrimario Then

    '      Riga &= canale.Nome & vbTab
    '    End If
    '  End If
    'Next
    'Righe.Add(Riga.TrimEnd(vbTab))

  End Sub

  Private Function RigaCanaleSecondario(Valori As Double()) As String
    Dim Riga As String = CDbl(Valori.Average).ToString("F2") & vbTab
    Riga &= CDbl(Valori.Max).ToString("F2") & vbTab
    Riga &= CDbl(Valori.Min).ToString("F2") & vbTab
    Dim StandardDeviation As Double = 0
    alglib.basestat.sampleadev(Valori, Valori.Count, StandardDeviation)
    Riga &= CDbl(StandardDeviation).ToString("F2") & vbTab
    Return Riga
  End Function

  Private Function RigaCanaleSecondario(Canale As clsBasicChannelFinder, Intervalli As Integer) As String
    Dim Riga As String = ""
    Dim TuttiValori = Canale.Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    Dim H As Double = (TuttiValori.Max - TuttiValori.Min) / Intervalli


    Dim Ysum As Double = 0
    Dim coppie As New List(Of clsDoubleXY)
    For ii As Integer = 0 To Intervalli - 1
      Dim x As Double = TuttiValori.Min + H * 0.5 + (H * ii)
      Dim y = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, Canale.ValoriTmp)
      Ysum += y
      coppie.Add(New clsDoubleXY(x, y))
    Next
    For Each coppia In coppie
      'For ii As Integer = 0 To Intervalli - 1
      'Dim x As Double = TuttiValori.Min + H * 0.5 + (H * ii)
      'Dim y = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, Canale.ValoriTmp)
      Riga &= (coppia.Y / Ysum * 100).ToString("F1") & vbTab
    Next
    Return Riga
  End Function

  'Private Function RigaCanaleSecondario(Valori As Double(), ArrayIndici As List(Of Integer), ListaCanaliSecondari As List(Of clsBasicChannelFinder)) As String
  '  Dim Riga As String = ""
  '  For Each canale In ListaCanaliSecondari
  '    Dim ValoriCanaleSecondario As New List(Of Double)
  '    For Each indice In ArrayIndici
  '      ValoriCanaleSecondario.Add(Valori(indice))
  '    Next
  '    Dim VnotNan = ValoriCanaleSecondario.Where(Function(x) Not Double.IsNaN(x)).ToList
  '    Dim StandardDeviation As Double = 0
  '    Dim V(VnotNan.Count - 1) As Double
  '    For i As Integer = 0 To V.Count - 1
  '      V(i) = VnotNan(i)
  '    Next

  '    alglib.basestat.sampleadev(VnotNan.ToArray, Valori.Count, StandardDeviation)
  '    Riga &= "Avg:" & vbTab & CDbl(VnotNan.Average).ToString("F2") & vbTab
  '    Riga &= "Max:" & vbTab & CDbl(VnotNan.Max).ToString("F2") & vbTab
  '    Riga &= "Min:" & vbTab & CDbl(VnotNan.Min).ToString("F2") & vbTab
  '    Riga &= "SD:" & vbTab & CDbl(StandardDeviation).ToString("F2") & vbTab

  '  Next

  'End Function


  Private Sub SalvaData(NewFileFolder As String, ListaCanaliOutput As List(Of String))
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva CSV data")
    Dim pathCsv As String = NewFileFolder & _StringaDescrizioneFiltro & ".csv"

    If Not System.IO.Directory.Exists(NewFileFolder) Then
      System.IO.Directory.CreateDirectory(NewFileFolder)
    End If

    If System.IO.File.Exists(pathCsv) Then
      For i As Integer = 1 To 999
        If Not System.IO.File.Exists(NewFileFolder & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".csv") Then
          pathCsv = NewFileFolder & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".csv"
          Exit For
        End If
      Next
    End If

    'crea la lista dei canali da esportare
    Dim ListaCanaliDaEsportare As New List(Of clsBasicChannelFinder)
    PgbMaxVal = ListaCanaliOutput.Count + 1
    PgbValue = 1
    For Each canale In ListaCanaliOutput
      Dim CanaleCorrente = _BasicChannelsOutput.Lista.Where(Function(x) x.Nome = canale).FirstOrDefault
      If Not CanaleCorrente Is Nothing Then
        ListaCanaliDaEsportare.Add(CanaleCorrente)
      End If
      PgbValue += 1
    Next

    'gira tra le righe 
    'intestazioni
    Dim Righe As New List(Of String)
    Dim Riga As String = "DateTime" & vbTab
    For Each canale In ListaCanaliDaEsportare
      If Not canale.IsDT Then
        Riga &= canale.Nome & vbTab
      End If
    Next
    Righe.Add(Riga.TrimEnd(vbTab))

    Dim cDT = ListaCanaliDaEsportare.Where(Function(x) x.Nome = "SystemTime_DaySeconds").FirstOrDefault

    PgbMaxVal = cDT.ValoriDT.Count + 1
    PgbValue = 1
    For i As Integer = 0 To cDT.ValoriDT.Count - 1
      Riga = cDT.ValoriDT(i).ToString("yyyy/MM/dd HH:mm:ss.fff") & vbTab
      For Each canale In ListaCanaliDaEsportare
        If Not canale.IsDT Then
          Riga &= canale.Valori(i).Value.ToString("F3") & vbTab
        End If
      Next
      Righe.Add(Riga.TrimEnd(vbTab))
      PgbValue += 1
    Next

    ResultTxt = "Full data set copied in the file '" & pathCsv & "'" & vbCrLf
    ResultTxt &= "Top 100 Rows:" & vbCrLf
    For i As Integer = 0 To 100
      ResultTxt &= Righe(i) & vbCrLf
    Next


    Dim fsTmp As New System.IO.StreamWriter(pathCsv)
    If Righe.Count > 0 Then
      fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
    End If
    fsTmp.Close()
    fsTmp.Dispose()
    ApriExplorer(pathCsv)
  End Sub





  Private Sub SalvaParquet(NewFileFolder As String)
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva Parquet")

    Dim pathParquet As String = NewFileFolder & _StringaDescrizioneFiltro & ".ppf" ' ".parquet"

    If Not System.IO.Directory.Exists(NewFileFolder) Then
      System.IO.Directory.CreateDirectory(NewFileFolder)
    End If

    If System.IO.File.Exists(pathParquet) Then
      For i As Integer = 1 To 999
        If Not System.IO.File.Exists(NewFileFolder & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".ppf") Then
          pathParquet = NewFileFolder & _StringaDescrizioneFiltro & "_" & i.ToString.PadLeft(2, "0") & ".ppf"
          Exit For
        End If
      Next
    End If

    'For Each canale In _BasicChannelsOutput.Lista
    '  Console.WriteLine(canale.Nome & " DT:" & canale.ValoriDT.Count & " Vals:" & canale.Valori.Count)
    'Next

    'Stop

    Dim DictGiorni As New Dictionary(Of Integer, Integer)
    Dim DataRef As New DateTime(1971, 11, 26, 0, 0, 0)
    Dim prog As Integer = 0
    For Each fp In _ListaFilePeriodi
      If Not DictGiorni.ContainsKey(Int(fp.PeriodsTR.Start.ToOADate)) Then
        DictGiorni.Add(Int(fp.PeriodsTR.Start.ToOADate), prog)
        prog += 1
      End If
    Next

    PgbMaxVal = _BasicChannelsOutput.Lista.Count + 1
    PgbValue = 1
    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In _BasicChannelsOutput.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      'DataFields.Add(DataField)
      If canale.Nome = "SystemTime_DaySeconds" Then ' aggiunge l equivalente di un giorno per ogni
        Dim valori(canale.ValoriDT.Count - 1) As Double
        Dim valoriData(canale.ValoriDT.Count - 1) As Double
        Dim SecFromSourceStart As Double = 0
        For i As Integer = 0 To valori.Count - 1
          Dim Momento As DateTime = canale.ValoriDT(i)
          If Momento = Nothing Then
            valori(i) = SecFromSourceStart + 0.001
          Else
            Dim IdProgressivo As Integer = DictGiorni(Int(Momento.ToOADate))
            SecFromSourceStart = Momento.TimeOfDay.TotalSeconds + (IdProgressivo * 24 * 60 * 60)
            valori(i) = SecFromSourceStart
          End If
          valoriData(i) = canale.ValoriDT(i).ToString("yyyyMMdd")
        Next
        CanaliPq.Add(New Parquet.Data.DataColumn(DataField, valori))

        Dim DataFieldPerf As New Parquet.Data.DataField("SystemTime_Performance", Parquet.Data.DataType.Double, False)
        CanaliPq.Add(New Parquet.Data.DataColumn(DataFieldPerf, canale.ValoriDT.Select(Function(x) x.ToOADate).ToArray))

        Dim DataFieldData As New Parquet.Data.DataField("SystemTime_Date", Parquet.Data.DataType.Double, False)
        CanaliPq.Add(New Parquet.Data.DataColumn(DataFieldData, valoriData))
      Else
        Dim valori(canale.Valori.Count - 1) As Double
        For i As Integer = 0 To canale.Valori.Count - 1
          valori(i) = canale.Valori(i)
        Next
        CanaliPq.Add(New Parquet.Data.DataColumn(DataField, valori))
      End If
      PgbValue += 1
      PgbText = "3/3 " & (PgbValue / PgbMaxVal * 100).ToString("F0") & "%"
    Next
    Dim fsTmp As New System.IO.StreamWriter(pathParquet)
    'Dim PqSchema As New Parquet.Data.Schema(DataFields.ToList)
    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)
    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup

    PgbMaxVal = CanaliPq.Count + 1
    PgbValue = 1
    For Each Canale In CanaliPq
      GroupWriter.WriteColumn(Canale)
      PgbValue += 1
    Next
    ParquetWriter.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()

    'crea il file json dei periodi
    'Dim pathJson As String = pathParquet.Replace(".parquet", ".json")
    Dim pathJson As String = NewFileFolder & Now.ToString("yyyyMMdd") & "_Periods" & ".json"
    Dim l As New ObservableCollection(Of clsPeriod2021)
    'For Each fp In _ListaFilePeriodi
    For Each fp In _PeriodiDaEsportare
      'For Each p In fp.Periods
      l.Add(fp)
      'Next
    Next
    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva Json Periodi")

    Dim JJ As New clsPeriodsJson
    JJ.ListaPeriodi = l
    clsKillerSeriale.SaveConfigurationGeneric(Of clsPeriodsJson)(JJ, pathJson)

    'Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Fine!")


    ResultTxt = "Stocazzo " & pathParquet & "" & vbCrLf
    ResultTxt &= "And" & vbCrLf
    ResultTxt &= "" & pathJson & "" & vbCrLf
    ResultTxt &= "Created" & vbCrLf

    ApriExplorer(pathParquet)
  End Sub

End Class

Public Class clsBasicChannelsFinder
  Dim pLista As New List(Of clsBasicChannelFinder)

  Public Property Lista As List(Of clsBasicChannelFinder)
    Get
      Return pLista
    End Get
    Set(value As List(Of clsBasicChannelFinder))
      pLista = value
    End Set
  End Property

End Class

Public Class clsBasicChannelFinder
  Dim _NomeCanale As String
  Dim _IsDT As Boolean = False
  Dim _ArrayValori As Double()
  Dim _Valori As New List(Of Double?)
  Dim _ValoriTmp As New List(Of Double)
  Dim _ValoriDT As New List(Of DateTime)

  Public Sub New(NomeCanale As String, isDT As Boolean)
    _NomeCanale = NomeCanale
    _IsDT = isDT
  End Sub

  Public Property Valori As List(Of Double?)
    Get
      Return _Valori
    End Get
    Set(value As List(Of Double?))
      _Valori = value
    End Set
  End Property

  Public Property Nome As String
    Get
      Return _NomeCanale
    End Get
    Set(value As String)
      _NomeCanale = value
    End Set
  End Property

  Public Property ValoriDT As List(Of Date)
    Get
      Return _ValoriDT
    End Get
    Set(value As List(Of Date))
      _ValoriDT = value
    End Set
  End Property

  Public Property IsDT As Boolean
    Get
      Return _IsDT
    End Get
    Set(value As Boolean)
      _IsDT = value
    End Set
  End Property

  Public Property ValoriTmp As List(Of Double)
    Get
      Return _ValoriTmp
    End Get
    Set(value As List(Of Double))
      _ValoriTmp = value
    End Set
  End Property

  Public Property ArrayValori As Double()
    Get
      Return _ArrayValori
    End Get
    Set(value As Double())
      _ArrayValori = value
    End Set
  End Property
End Class

Public Class clsParquetAndPeriods
  Dim _Parquet As clsParquetFile
  Dim _Periods As List(Of clsPeriod2021)
  Dim _Momenti As List(Of DateTime)
  Dim _Indici As List(Of Integer)
  Dim _PeriodsTR As clsTimeRange
  Dim _IdProgressivo As Integer

  Public Property Parquet As clsParquetFile
    Get
      Return _Parquet
    End Get
    Set(value As clsParquetFile)
      _Parquet = value
    End Set
  End Property

  Public Property Periods As List(Of clsPeriod2021)
    Get
      Return _Periods
    End Get
    Set(value As List(Of clsPeriod2021))
      _Periods = value
    End Set
  End Property

  Public Property PeriodsTR As clsTimeRange
    Get
      Return _PeriodsTR
    End Get
    Set(value As clsTimeRange)
      _PeriodsTR = value
    End Set
  End Property

  Public Property IdProgressivo As Integer
    Get
      Return _IdProgressivo
    End Get
    Set(value As Integer)
      _IdProgressivo = value
    End Set
  End Property

  Public Property Momenti As List(Of Date)
    Get
      Return _Momenti
    End Get
    Set(value As List(Of Date))
      _Momenti = value
    End Set
  End Property

  Public Property Indici As List(Of Integer)
    Get
      Return _Indici
    End Get
    Set(value As List(Of Integer))
      _Indici = value
    End Set
  End Property

  Public Sub New(Parquet As clsParquetFile, Periods As List(Of clsPeriod2021), IdProgressivo As Integer)
    _Parquet = Parquet
    _Periods = Periods
    _IdProgressivo = IdProgressivo
    Dim inizio As DateTime = Periods.Select(Function(x) x.TR.Start).Min
    Dim fine As DateTime = Periods.Select(Function(x) x.TR.Finish).Max
    _PeriodsTR = New clsTimeRange(inizio, fine)
  End Sub

  Public Sub New(Parquet As clsParquetFile, Momenti As List(Of DateTime), Indici As List(Of Integer), IdProgressivo As Integer)
    _Parquet = Parquet
    _Momenti = Momenti
    _Indici = Indici
    _IdProgressivo = IdProgressivo
    Dim inizio As DateTime = Momenti.Min
    Dim fine As DateTime = Momenti.Max
    _PeriodsTR = New clsTimeRange(inizio, fine)
  End Sub



End Class
