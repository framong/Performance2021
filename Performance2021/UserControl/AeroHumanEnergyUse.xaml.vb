Imports System.Collections.ObjectModel
Imports System.ComponentModel



'Ci sono tre casi per la JIB

'1.       Small Chamber TRIM
'Sono in Fast trim SE
'pump1smallChamber = 1 And MDPed1EffectiveFunction = 7
'in questo caso devo usare il sensore di pressione
'portjibsheetram2_pressA


'2.       Big chamber TRIM
'Sono in TRIM SE
'pump1smallChamber = 0 And MDPed1EffectiveFunction = 7
'in questo caso devo usare il sensore di pressione
'portjibsheetram1_Press



'3.       Power Ease
'Sono in Power Ease SE
'PortJibSheetDI0_ToggleFastEase = 1 And MDPed1EffectiveFunction = 7
'In questo caso devo usare il sensore di pressione
'portjibsheetram2_pressB

'Per la JibSheet di Stbd devi usare le stesse condizioni ma coi nomi dei canali diversi:
'pump1smallChamber diventa pump2smallChamber
'MDPed1EffectiveFunction diventa MDPed2EffectiveFunction
'portjibsheetram2_pressA diventa Stbdjibsheetram2_pressA
'portjibsheetram1_Press diventa Stbdjibsheetram1_Press
'portjibsheetram2_pressB diventa Stbdjibsheetram2_pressB
'PortJibSheetDI0_ToggleFastEase diventa StbdJibSheetDI0_ToggleFastEase

Public Class AeroHumanEnergyUse

  Public Sub New()
    Me.DataContext = PedestalControl

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

  End Sub


  Public Sub Test()
    PedestalControl.ImpostazioniIniziali()
    Stop
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    'Dim dataTmp As New DateTime
    'Dim statistiche As New clsPedestalsAnalysis(PedestalControl.Pedestals.ToList, ObjChartSyncManagerBasic.VisibleRange, Nothing)
    'Dim statistiche As New clsPedestalsAnalysis(PedestalControl.Pedestals.ToList, ObjChartSyncManagerBasic.VisibleRange, Nothing)
    'Dim strTmp = PeriodsManager.ReportStatistichePedestals(ObjChartSyncManagerBasic.VisibleRange.StringaPeriodo, statistiche)
    'Clipboard.SetText(strTmp)

    Dim objXls As New clsXls
    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = ObjChartSyncManagerBasic.VisibleRange.Start.ToString("yyyyMMdd_HHmmss") & "_" & ObjChartSyncManagerBasic.VisibleRange.Finish.ToString("HHmmss")

    objXls.CreaPedestalReportExcelFileSinglePeriod(path & "\" & pstringadata & "_MenAtWork.xlsx", ObjChartSyncManagerBasic.VisibleRange)
    ApriExplorer(path, path & "\" & pstringadata & "_MenAtWork.xlsx")

  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Dim objXls As New clsXls
    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd")

    objXls.CreaPedestalReportExcelFile(path & "\" & pstringadata & "_MenAtWork.xlsx", ObjChartSyncManagerBasic.VisibleRange)
    ApriExplorer(path, path & "\" & pstringadata & "_MenAtWork.xlsx")

  End Sub
End Class


Public Class clsLines
  Dim _CanaleMainLine As New List(Of clsChannel2020)
  Dim _MainLinesLoaded As Boolean

  Public Property CanaleMainLine As List(Of clsChannel2020)
    Get
      Return _CanaleMainLine
    End Get
    Set(value As List(Of clsChannel2020))
      _CanaleMainLine = value
    End Set
  End Property

  Public Property MainLinesLoaded As Boolean
    Get
      Return _MainLinesLoaded
    End Get
    Set(value As Boolean)
      _MainLinesLoaded = value
    End Set
  End Property

  Public Sub New()
    Dim BasicName As String = "MainLineX_Press"
    _MainLinesLoaded = True
    For i As Integer = 1 To 4
      _CanaleMainLine.Add(DataProvider2020.CanaleDbl(BasicName.Replace("X", i)))
      _MainLinesLoaded = _MainLinesLoaded And Not _CanaleMainLine.Last Is Nothing
    Next
  End Sub

End Class

Public Class clsPedestal
  Implements INotifyPropertyChanged

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim _Nome As String
  Dim _CanaleSailingFunction As clsChannel2020
  Dim _CanalePompaPressione As clsChannel2020
  Dim _CanalePompaMarcia As clsChannel2020
  Dim _CanalePompaRpm As clsChannel2020
  Dim _CanaliOk As Boolean = False
  Dim _Funzioni As Dictionary(Of Integer, String)

  Dim _VolumeLitriP1 As Double
  Dim _VolumeLitriP2 As Double
  Dim _VolumeLitriP3 As Double

  Dim _PedestalDescription As String
  Dim _PedestalPower As String
  Dim _SailingFunction As String
  Dim _SailingFunctionDetail As String
  Dim _PedestalGear As String
  Dim _PedestalRpm As String
  Dim _FunctionPressure As String
  Dim _PedestalColor As Color

  Dim _PedestalId As Integer
  Dim _PedestalPowerWatt As Integer
  Dim _SailingFunctionId As Integer
  Dim _PedestalGearId As Integer
  Dim _PedestalRpmVal As Integer
  Dim _FunctionPressureBar As Integer

  Dim _MainLines As clsLines


  Public Property PedestalDescription As String
    Get
      Return _PedestalDescription
    End Get
    Set(value As String)
      If _PedestalDescription = value Then Exit Property
      _PedestalDescription = value
      OnPropertyChanged("PedestalDescription")
    End Set
  End Property

  Public Property PedestalPower As String
    Get
      Return _PedestalPower
    End Get
    Set(value As String)
      If _PedestalPower = value Then Exit Property
      _PedestalPower = value
      OnPropertyChanged("PedestalPower")
    End Set
  End Property

  Public Property PedestalColor As Color
    Get
      Return _PedestalColor
    End Get
    Set(value As Color)
      If _PedestalColor = value Then Exit Property
      _PedestalColor = value
      OnPropertyChanged("PedestalColor")
    End Set
  End Property

  Public Property SailingFunction As String
    Get
      Return _SailingFunction
    End Get
    Set(value As String)
      If _SailingFunction = value Then Exit Property
      _SailingFunction = value
      OnPropertyChanged("SailingFunction")
    End Set
  End Property

  Public Property SailingFunctionDetail As String
    Get
      Return _SailingFunctionDetail
    End Get
    Set(value As String)
      _SailingFunctionDetail = value
      OnPropertyChanged("SailingFunctionDetail")
    End Set
  End Property

  Public Property PedestalGear As String
    Get
      Return _PedestalGear
    End Get
    Set(value As String)
      If _PedestalGear = value Then Exit Property
      _PedestalGear = value
      OnPropertyChanged("PedestalGear")
    End Set
  End Property

  Public Property PedestalRpm As String
    Get
      Return _PedestalRpm
    End Get
    Set(value As String)
      If _PedestalRpm = value Then Exit Property
      _PedestalRpm = value
      OnPropertyChanged("PedestalRpm")
    End Set
  End Property

  Public Property FunctionPressure As String
    Get
      Return _FunctionPressure
    End Get
    Set(value As String)
      If _FunctionPressure = value Then Exit Property
      _FunctionPressure = value
      OnPropertyChanged("FunctionPressure")
    End Set
  End Property

  Public Property PedestalId As Integer
    Get
      Return _PedestalId
    End Get
    Set(value As Integer)
      _PedestalId = value
    End Set
  End Property

  Public Property PedestalPowerWatt As Integer
    Get
      Return _PedestalPowerWatt
    End Get
    Set(value As Integer)
      _PedestalPowerWatt = value
    End Set
  End Property

  Public Property SailingFunctionId As Integer
    Get
      Return _SailingFunctionId
    End Get
    Set(value As Integer)
      _SailingFunctionId = value
    End Set
  End Property

  Public Property PedestalGearId As Integer
    Get
      Return _PedestalGearId
    End Get
    Set(value As Integer)
      _PedestalGearId = value
    End Set
  End Property

  Public Property PedestalRpmVal As Integer
    Get
      Return _PedestalRpmVal
    End Get
    Set(value As Integer)
      _PedestalRpmVal = value
    End Set
  End Property

  Public Property FunctionPressureBar As Integer
    Get
      Return _FunctionPressureBar
    End Get
    Set(value As Integer)
      _FunctionPressureBar = value
    End Set
  End Property

  Public Property MainLines As clsLines
    Get
      Return _MainLines
    End Get
    Set(value As clsLines)
      _MainLines = value
    End Set
  End Property

  Public Sub New(Nome As String, CanaleSailingFunction As String, CanalePompaPressione As String, CanalePompaMarcia As String, CanalePompaRpm As String, VolumeLitriP1 As Double, VolumeLitriP2 As Double, VolumeLitriP3 As Double, Funzioni As Dictionary(Of Integer, String), MainLines As clsLines)
    PedestalDescription = Nome
    _MainLines = MainLines
    If PedestalDescription.IndexOf("Port") > -1 Then
      PedestalColor = Colors.Red
    Else
      PedestalColor = Colors.LightGreen
    End If
    If VolumeLitriP1 = -1 Then
      SailingFunction = CanaleSailingFunction
      PedestalRpm = CanalePompaRpm
      PedestalGear = CanalePompaMarcia
      FunctionPressure = CanalePompaPressione
      PedestalPower = "Watt"
    Else
      _CanaleSailingFunction = DataProvider2020.CanaleDbl(CanaleSailingFunction)
      _CanalePompaPressione = DataProvider2020.CanaleDbl(CanalePompaPressione)
      _CanalePompaMarcia = DataProvider2020.CanaleDbl(CanalePompaMarcia)
      _CanalePompaRpm = DataProvider2020.CanaleDbl(CanalePompaRpm)
      _VolumeLitriP1 = VolumeLitriP1
      _VolumeLitriP2 = VolumeLitriP2
      _VolumeLitriP3 = VolumeLitriP3
      _Funzioni = Funzioni
      _CanaliOk = VerificaCanali()
    End If
  End Sub

  Private Function VerificaCanali() As Boolean
    If _CanaleSailingFunction Is Nothing Then Return False
    If _CanalePompaPressione Is Nothing Then Return False
    If _CanalePompaMarcia Is Nothing Then Return False
    If _CanalePompaRpm Is Nothing Then Return False
    Return True
  End Function

  Public Function PotenzaIstantanea(IdRiga As Integer) As Double
    PedestalPowerWatt = 0
    If _CanaliOk Then
      PedestalPowerWatt = CalcolaPotenzaIstantanea(IdRiga)
      PedestalPower = PedestalPowerWatt.ToString("F0")
    End If
    Return PedestalPowerWatt
  End Function

  Private Function CalcolaPotenzaIstantanea(IdRiga As Integer) As Double

    SailingFunctionId = CInt(ValoreNotNan(_CanaleSailingFunction.Valori(IdRiga)))
    If _Funzioni.ContainsKey(SailingFunctionId) Then
      SailingFunction = _Funzioni(SailingFunctionId)
    Else
      SailingFunction = "Not Valid"
      'For i As Integer = 0 To 1000
      '  SailingFunctionId = CInt(ValoreNotNan(_CanaleLinea.Valori(IdRiga - i)))
      '  If SailingFunctionId > 0 Then
      '    SailingFunction = _Funzioni(SailingFunctionId)
      '    Exit For
      '  End If
      'Next
    End If
    PedestalRpmVal = ValoreNotNan(_CanalePompaRpm.Valori(IdRiga))
    PedestalRpm = PedestalRpmVal.ToString("F0")
    PedestalGearId = ValoreNotNan(_CanalePompaMarcia.Valori(IdRiga))
    PedestalGear = _PedestalGearId.ToString("F0")
    If SailingFunctionId = 7 AndAlso Not _MainLines Is Nothing Then
      FunctionPressureBar = ValoreNotNan(_MainLines.CanaleMainLine(1).Valori(IdRiga))
    Else
      FunctionPressureBar = ValoreNotNan(_CanalePompaPressione.Valori(IdRiga))
    End If
    FunctionPressure = FunctionPressureBar.ToString("F0")
    Return PedestalRpmVal * CalcolaVolumeLitri(PedestalGearId) * FunctionPressureBar / 600 * 1000
  End Function

  Private Function ValoreNotNan(Valore As Double) As Double
    If Double.IsNaN(Valore) Then Return 0
    Return Valore
  End Function

  Private Function CalcolaVolumeLitri(Marcia As Integer) As Double
    Select Case Marcia
      Case 1
        Return _VolumeLitriP1
      Case 2
        Return _VolumeLitriP2
      Case 3
        Return _VolumeLitriP1 + _VolumeLitriP2
      Case 4
        Return _VolumeLitriP3
      Case 5
        Return _VolumeLitriP1 + _VolumeLitriP3
      Case 6
        Return _VolumeLitriP2 + _VolumeLitriP3
      Case Else
        Return _VolumeLitriP1 + _VolumeLitriP2 + _VolumeLitriP3
    End Select
  End Function




End Class


Public Class clsCalcoliPedestals
  Implements INotifyPropertyChanged

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim _Funzioni As New Dictionary(Of Integer, String)
  Dim _FunzioniDetail As New Dictionary(Of Integer, String)
  Dim _Pedestals As New ObservableCollection(Of clsPedestal)
  Dim _MainLines As clsLines

  Public Property Pedestals As ObservableCollection(Of clsPedestal)
    Get
      Return _Pedestals
    End Get
    Set(value As ObservableCollection(Of clsPedestal))
      _Pedestals = value
      OnPropertyChanged("Pedestals")
    End Set
  End Property

  Public ReadOnly Property Funzioni As Dictionary(Of Integer, String)
    Get
      If _FunzioniDetail Is Nothing Then
        Return _Funzioni
      Else
        Return _FunzioniDetail
      End If
    End Get
    'Set(value As Dictionary(Of Integer, String))
    '  _Funzioni = value
    'End Set
  End Property

  'Public Property FunzioniDetail As Dictionary(Of Integer, String)
  '  Get
  '    Return _FunzioniDetail
  '  End Get
  '  Set(value As Dictionary(Of Integer, String))
  '    _FunzioniDetail = value
  '  End Set
  'End Property

  Private Sub ImpostaDictionaryFunzioni()
    Dim strTmp As New List(Of String)
    strTmp.Add("1 SmallFunctions")
    strTmp.Add("2 JibSheetLink")
    strTmp.Add("3 MainsheetTravSpanner")
    strTmp.Add("4 Twist")
    strTmp.Add("5 Runners")
    strTmp.Add("6 SailingAccum")
    strTmp.Add("7 JibSheet")
    strTmp.Add("8 JibAccum")
    Dim Funzioni As String = AppConfig.CercaValoreInnerText("Ac75Settings", clsSettings.eNodoSTD.eStartUp, "SailingFunction", "Lines", String.Join("|", strTmp), True, False)

    _Funzioni.Clear()
    For i As Integer = 0 To Funzioni.Split("|").Length - 1
      _Funzioni.Add(i + 1, Funzioni.Split("|")(i))
    Next


    strTmp.Clear()
    strTmp.Add("1;main post")
    strTmp.Add("2;main outhaul")
    strTmp.Add("3;main Cunningham")
    strTmp.Add("4;jib in/out")
    strTmp.Add("5;jib dw")
    strTmp.Add("6;jib Cunningham")
    strTmp.Add("10;runner port")
    strTmp.Add("11;runner stbd")
    strTmp.Add("19;more than a FUNCTION active")
    strTmp.Add("18;No function active on Line 1")
    strTmp.Add("20;jib port")
    strTmp.Add("21;jib stbd")
    strTmp.Add("30;Trav")
    strTmp.Add("31;main sheet")
    strTmp.Add("32;spanner")
    strTmp.Add("40;Twist")
    strTmp.Add("39;more than a FUNCTION active")
    strTmp.Add("38;no function active on line 3")
    strTmp.Add("22; accumulator")
    Funzioni = AppConfig.CercaValoreInnerText("Ac75Settings", clsSettings.eNodoSTD.eStartUp, "SailingFunctionDetail", "Lines", String.Join("|", strTmp), True, False)

    _FunzioniDetail.Clear()
    For Each dict In Funzioni.Split("|")
      _FunzioniDetail.Add(dict.Split(";")(0), dict.Split(";")(1))
    Next
    'For i As Integer = 0 To Funzioni.Split("|").Length - 1
    '  _FunzioniDetail.Add(i + 1, Funzioni.Split("|")(i))
    'Next


  End Sub

  Private Sub ImpostaPedestals()
    Dim strTmp As New List(Of String)
    strTmp.Add("FrontPort")
    strTmp.Add("FrontStbd")
    strTmp.Add("AftPort")
    strTmp.Add("AftStbd")
    Dim Peds As String = AppConfig.CercaValoreInnerText("Ac75Settings", clsSettings.eNodoSTD.eStartUp, "Pedestals", "Names", String.Join("|", strTmp), True, False)
    _Pedestals.Clear()
    For i As Integer = 0 To Peds.Split("|").Length - 1
      _Pedestals.Add(AggiungiPedestal(i + 1, Peds.Split("|")(i)))
    Next

  End Sub

  Private Function AggiungiPedestal(Pedestal As Integer, Nome As String) As clsPedestal
    Dim P1dm3 As Double = PumpVolume(1, Pedestal)
    Dim P2dm3 As Double = PumpVolume(2, Pedestal)
    Dim P3dm3 As Double = PumpVolume(3, Pedestal)
    Dim CanaleSF As String = "MDPed#EffectiveFunction".Replace("#", Pedestal)
    Dim ListaFunzioni As Dictionary(Of Integer, String) = _Funzioni
    If Not DataProvider2020.CanaleDbl("Ped#_Funct".Replace("#", Pedestal)) Is Nothing Then
      CanaleSF = "Ped#_Funct".Replace("#", Pedestal)
      ListaFunzioni = _FunzioniDetail
    End If
    Select Case Pedestal
      Case 1, 2
        Return New clsPedestal("#: ".Replace("#", Pedestal) & Nome, CanaleSF, "PumpLine#_Press".Replace("#", Pedestal), "Pump#_Gear".Replace("#", Pedestal), "Pump#_RPM".Replace("#", Pedestal), P1dm3, P2dm3, P3dm3, ListaFunzioni, _MainLines)
      Case Else
        Return New clsPedestal("#: ".Replace("#", Pedestal) & Nome, CanaleSF, "PumpLine#_Press".Replace("#", Pedestal), "Pump#_Gear".Replace("#", Pedestal), "Pump#_RPM".Replace("#", Pedestal), P1dm3, P2dm3, P3dm3, ListaFunzioni, Nothing)
    End Select
  End Function

  Public Sub ImpostazioniIniziali()
    _MainLines = New clsLines
    If Not _MainLines.MainLinesLoaded Then _MainLines = Nothing
    ImpostaDictionaryFunzioni()
    ImpostaPedestals()
    AppConfig.SalvaFileXML()
  End Sub

  Private Function DefaultPumpVolume(Pump As Integer, Pedestal As Integer) As Double
    Select Case Pump
      Case 1
        Select Case Pedestal
          Case 3
            Return Volume_dm3(9, 12, 8)
          Case Else
            Return Volume_dm3(9, 12, 9)
        End Select
      Case 2
        Select Case Pedestal
          Case 3
            Return Volume_dm3(9, 12, 11)
          Case Else
            Return Volume_dm3(9, 12, 12)
        End Select
      Case 3
        Select Case Pedestal
          Case 3
            Return Volume_dm3(9, 18, 12)
          Case Else
            Return Volume_dm3(9, 18, 12)
        End Select
    End Select
  End Function

  Private Function PumpVolume(Pump As Integer, Pedestal As Integer) As Double
    Return AppConfig.CercaValoreInnerText("Ac75Settings", clsSettings.eNodoSTD.eStartUp, "PumpsVolumesLiters", "Pedestal_" & Pedestal.ToString & "_Pump_" & Pump.ToString, DefaultPumpVolume(Pump, Pedestal), True, False)
  End Function


  Private Function Volume_dm3(Cylinders As Integer, Diameter As Double, Stroke As Double) As Double
    Return Cylinders * Stroke / 100 * (Diameter / 100 / 2) ^ 2 * System.Math.PI
  End Function

  Public Sub AggiornaPosizioneCorrente(Momento As DateTime)
    If DataProvider2020 Is Nothing Then Exit Sub
    Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
    For Each P In Pedestals
      P.PotenzaIstantanea(Id)
    Next
  End Sub

End Class

Public Class clsPedestalsAnalysis
  Dim _Pedestals As List(Of clsPedestal)
  Dim _PedestalAnalisys As New List(Of clsPedestalAnalisys)
  Dim _TimeRange As clsTimeRange
  Dim _Prd As clsPeriod2021

  Dim _TotalSecondsOfActivity As Double
  Dim _TotalEnergyJoule As Double
  Dim _PowerAvgWatt As Double

  Public ReadOnly Property PowerAvgWatt As Double
    Get
      Return _PowerAvgWatt
    End Get
  End Property

  Public ReadOnly Property TotalSecondsOfActivity As Double
    Get
      Return _TotalSecondsOfActivity
    End Get
  End Property

  Public ReadOnly Property AvgSecondsOfActivity As Double
    Get
      Return TotalSecondsOfActivity / PedestalAnalisys.Count
    End Get
  End Property

  Public ReadOnly Property TotalEnergyJoule() As Double
    Get
      Return _TotalEnergyJoule
    End Get
  End Property

  Public ReadOnly Property TotalEnergyKcal() As Double
    Get
      Return JouleToKcal(TotalEnergyJoule)
    End Get
  End Property

  Public ReadOnly Property OneHourProjKcal() As Double
    Get
      Return ((TotalEnergyKcal / TotalSecondsOfActivity) * 3600)
    End Get
  End Property

  Public ReadOnly Property OneHourProjJoule() As Double
    Get
      Return ((TotalEnergyJoule / TotalSecondsOfActivity) * 3600)
    End Get
  End Property


  Public ReadOnly Property TotalActiveTimePercentage As Double
    Get
      Return _TotalSecondsOfActivity / (_TimeRange.Durata.TotalSeconds * _Pedestals.Count) * 100
    End Get
  End Property

  Public Sub New(Pedestals As List(Of clsPedestal), TimeRange As clsTimeRange, Prd As clsPeriod2021)
    _TimeRange = TimeRange
    _Pedestals = Pedestals
    _Prd = Prd
    AnalizzaPeriodo()
  End Sub

  Public Sub AnalizzaPeriodo()
    _TotalSecondsOfActivity = 0
    Dim pTot As Double = 0
    For Each pedestal In _Pedestals
      Dim obj = New clsPedestalAnalisys(pedestal, _TimeRange)
      _PedestalAnalisys.Add(obj)
      obj.CalcolaValoriMedi()
      'Console.WriteLine(obj.TotalSecondsOfActivity & " " & obj.PowerAvgWatt & " " & obj.PowerAvgWatt * obj.TotalSecondsOfActivity)
      _TotalSecondsOfActivity += obj.TotalSecondsOfActivity
      pTot += obj.PowerAvgWatt * obj.TotalSecondsOfActivity
    Next
    _PowerAvgWatt = pTot / _TotalSecondsOfActivity
    _TotalEnergyJoule = _PowerAvgWatt * _TotalSecondsOfActivity
    'Console.WriteLine(_TotalSecondsOfActivity & " " & _PowerAvgWatt & " " & _TotalEnergyJoule)
  End Sub

  Public ReadOnly Property TextTable(Headers As Boolean) As String
    Get
      Dim strTmp As String = ""
      If Headers Then
        strTmp &= "Description" & vbTab & "ActiveTimePerc" & vbTab
        strTmp &= "ActiveTimeAvgSecs" & vbTab & "PowerAvgWatt" & vbTab & "EnergyJoule" & vbTab & "HourProjKcal" & vbTab
      Else
        If _Prd Is Nothing Then
          strTmp &= _TimeRange.StringaPeriodo & vbTab & TotalActiveTimePercentage.ToString("F0") & vbTab
        Else
          Select Case _Prd.PeriodType
            Case clsPeriod2021.ePeriodType.eGybe
              strTmp &= "Gybe " & _Prd.KeyMoment.ToString("yyyyMMdd HH:mm:ss") & vbTab & TotalActiveTimePercentage.ToString("F0") & vbTab
            Case clsPeriod2021.ePeriodType.eTack
              strTmp &= "Tack " & _Prd.KeyMoment.ToString("yyyyMMdd HH:mm:ss") & vbTab & TotalActiveTimePercentage.ToString("F0") & vbTab
            Case clsPeriod2021.ePeriodType.eAcceleration
              strTmp &= "Accel " & _TimeRange.StringaPeriodo & vbTab & TotalActiveTimePercentage.ToString("F0") & vbTab
            Case Else
              strTmp &= _TimeRange.StringaPeriodo & vbTab & TotalActiveTimePercentage.ToString("F0") & vbTab
          End Select
        End If
        strTmp &= AvgSecondsOfActivity.ToString("F0") & vbTab & PowerAvgWatt.ToString("F0") & vbTab & TotalEnergyJoule.ToString("F0") & vbTab & OneHourProjJoule.ToString("F0") & vbTab
      End If
      Return strTmp.TrimEnd(vbTab)
    End Get
  End Property

  Public Property PedestalAnalisys As List(Of clsPedestalAnalisys)
    Get
      Return _PedestalAnalisys
    End Get
    Set(value As List(Of clsPedestalAnalisys))
      _PedestalAnalisys = value
    End Set
  End Property

  Public Property TimeRange As clsTimeRange
    Get
      Return _TimeRange
    End Get
    Set(value As clsTimeRange)
      _TimeRange = value
    End Set
  End Property
End Class

Public Class clsPedestalAnalisys
  'valori aggregti per tutte le sailingfunction con tutte le marce del pedestal
  Dim _Pedestal As clsPedestal
  Dim _TimeRange As clsTimeRange
  Dim _FunzioniPedestal As New List(Of clsPedestalFuncion)

  Dim _TotalSecondsOfActivity As Double
  Dim _TotalEnergyJoule As Double
  Dim _PowerAvgWatt As Double

  Public ReadOnly Property PowerAvgWatt As Double
    Get
      Return _PowerAvgWatt
    End Get
  End Property

  Public ReadOnly Property TotalSecondsOfActivity() As Double
    Get
      Return _TotalSecondsOfActivity
    End Get
  End Property

  Public ReadOnly Property AvgSecondsOfActivity() As Double
    Get
      Return TotalSecondsOfActivity / _FunzioniPedestal.Count
    End Get
  End Property

  Public ReadOnly Property TimePercOfActivity() As Double
    Get
      Return TotalSecondsOfActivity / _TimeRange.Durata.TotalSeconds * 100
    End Get
  End Property

  Public ReadOnly Property TotalEnergyJoule() As Double
    Get
      Return _TotalEnergyJoule
    End Get
  End Property

  Public ReadOnly Property TotalEnergyKcal() As Double
    Get
      Return JouleToKcal(TotalEnergyJoule)
    End Get
  End Property

  Public Sub New(Pedestal As clsPedestal, TimeRange As clsTimeRange)
    _Pedestal = Pedestal
    _TimeRange = TimeRange
    AnalizzaPeriodo(DataProvider2020.Hz)
  End Sub

  ' numero di funzioni selezionate, attivate
  Public Sub AnalizzaPeriodo(Hz As Integer)
    For i As Integer = _TimeRange.IdRigaIniziale To _TimeRange.IdRigaFinale
      _Pedestal.PotenzaIstantanea(i)
      If _Pedestal.PedestalPowerWatt > 0 Then
        Dim SfId As Integer = _Pedestal.SailingFunctionId
        Dim PF = _FunzioniPedestal.Where(Function(x) x.SailingFunction = SfId).FirstOrDefault
        If PF Is Nothing Then
          PF = New clsPedestalFuncion(SfId, _Pedestal.SailingFunction, Hz, _TimeRange.Durata.TotalSeconds)
          _FunzioniPedestal.Add(PF)
        End If
        PF.AggiungiInfoMomento(_Pedestal.PedestalGearId, _Pedestal.PedestalRpmVal, _Pedestal.FunctionPressureBar, _Pedestal.PedestalPower)
      End If
    Next

  End Sub

  Public Sub CalcolaValoriMedi()
    _TotalSecondsOfActivity = 0
    _TotalEnergyJoule = 0
    Dim pTot As Double = 0
    For Each FP In _FunzioniPedestal
      FP.CalcolaValoriMedi()
      _TotalSecondsOfActivity += FP.TotalSecondsOfActivity
      _TotalEnergyJoule += FP.TotalEnergyJoule
      pTot += FP.PowerAvgWatt * FP.TotalSecondsOfActivity
    Next
    _PowerAvgWatt = pTot / _TotalSecondsOfActivity
  End Sub

  Public ReadOnly Property TextTable(Headers As Boolean) As String
    Get
      Dim strTmp As String = ""
      If Headers Then
        strTmp &= "Pedestal" & vbTab & "ActiveTimePerc" & vbTab
        strTmp &= "SecondsOfActivity" & vbTab & "PowerAvgWatt" & vbTab & "EnergyJoule" & vbTab
        For Each funzione In PedestalControl.Funzioni
          'uno per sailing function
          strTmp &= "Sf" & funzione.Key & "ss" & vbTab
        Next
        For Each funzione In PedestalControl.Funzioni
          'uno per sailing function
          strTmp &= "Sf" & funzione.Key & "j" & vbTab
        Next
        For Each funzione In PedestalControl.Funzioni
          'uno per sailing function
          strTmp &= "Sf" & funzione.Key & "w" & vbTab
        Next

        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  'uno per sailing function
        '  strTmp &= "Sf" & i & "ss" & vbTab
        'Next
        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  'uno per sailing function
        '  strTmp &= "Sf" & i & "j" & vbTab
        'Next
        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  'uno per sailing function
        '  strTmp &= "Sf" & i & "w" & vbTab
        'Next

      Else
        strTmp &= _Pedestal.PedestalDescription & vbTab & TimePercOfActivity.ToString("F0") & vbTab
        strTmp &= TotalSecondsOfActivity.ToString("F1") & vbTab & PowerAvgWatt.ToString("F0") & vbTab & TotalEnergyJoule.ToString("F0") & vbTab

        Dim SFs As New Dictionary(Of Integer, Double)
        Dim SFj As New Dictionary(Of Integer, Double)
        Dim SFw As New Dictionary(Of Integer, Double)
        'Dim SFs(PedestalControl.Funzioni.Count) As Double
        'Dim SFj(PedestalControl.Funzioni.Count) As Double
        'Dim SFw(PedestalControl.Funzioni.Count) As Double
        For Each Fp In FunzioniPedestal
          If Not SFs.ContainsKey(Fp.SailingFunction) Then
            SFs.Add(Fp.SailingFunction, Fp.TotalSecondsOfActivity)
          Else
            Stop
          End If
          If Not SFj.ContainsKey(Fp.SailingFunction) Then
            SFj.Add(Fp.SailingFunction, Fp.TotalEnergyJoule)
          Else
            Stop
          End If
          If Not SFw.ContainsKey(Fp.SailingFunction) Then
            SFw.Add(Fp.SailingFunction, Fp.PowerAvgWatt)
          Else
            Stop
          End If
          'SFs(Fp.SailingFunction) = Fp.TotalSecondsOfActivity
          'SFj(Fp.SailingFunction) = Fp.TotalEnergyJoule
          'SFw(Fp.SailingFunction) = Fp.PowerAvgWatt
        Next
        For Each funzione In PedestalControl.Funzioni
          'uno per sailing function
          Dim Id As Integer = funzione.Key
          If SFs.ContainsKey(Id) Then
            strTmp &= SFs(Id).ToString("F0") & vbTab
          End If
        Next
        For Each funzione In PedestalControl.Funzioni
          Dim Id As Integer = funzione.Key
          'uno per sailing function
          If SFj.ContainsKey(Id) Then
            strTmp &= SFj(Id).ToString("F0") & vbTab
          End If
        Next
        For Each funzione In PedestalControl.Funzioni
          Dim Id As Integer = funzione.Key
          'uno per sailing function
          If SFw.ContainsKey(Id) Then
            strTmp &= SFw(Id).ToString("F0") & vbTab
          End If
        Next

        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  strTmp &= SFs(i).ToString("F0") & vbTab
        'Next
        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  strTmp &= SFj(i).ToString("F0") & vbTab
        'Next
        'For i As Integer = 1 To PedestalControl.Funzioni.Count
        '  strTmp &= SFw(i).ToString("F0") & vbTab
        'Next

      End If
      Return strTmp.TrimEnd(vbTab)
    End Get
  End Property

  Public Property FunzioniPedestal As List(Of clsPedestalFuncion)
    Get
      Return _FunzioniPedestal
    End Get
    Set(value As List(Of clsPedestalFuncion))
      _FunzioniPedestal = value
    End Set
  End Property

  Public Property Pedestal As clsPedestal
    Get
      Return _Pedestal
    End Get
    Set(value As clsPedestal)
      _Pedestal = value
    End Set
  End Property
End Class


Public Class clsPedestalFuncion
  'valori aggregati di tutte le marce usate per la sailing function del pedestal
  Dim _SailingFunction As Integer
  Dim _SailingFunctionDescription As String
  Dim _Gears As New List(Of clsPedestalGear)
  Dim _TotalSecondsOfActivity As Double
  Dim _TotalEnergyJoule As Double
  Dim _PowerAvgWatt As Double
  Dim _Hz As Integer
  Dim _TotalPeriodSeconds As Double

  Public Sub New(SailingFunction As Integer, SailingFunctionDescription As String, Hz As Integer, TotalPeriodSeconds As Double)
    _SailingFunction = SailingFunction
    'If SailingFunction = 0 Then Stop
    _SailingFunctionDescription = SailingFunctionDescription
    _Hz = Hz
    _TotalPeriodSeconds = TotalPeriodSeconds
  End Sub

  Public Property SailingFunction As Integer
    Get
      Return _SailingFunction
    End Get
    Set(value As Integer)
      _SailingFunction = value
    End Set
  End Property

  Public Property Gears As List(Of clsPedestalGear)
    Get
      Return _Gears
    End Get
    Set(value As List(Of clsPedestalGear))
      _Gears = value
    End Set
  End Property

  Public Sub AggiungiInfoMomento(Gear As Integer, Rpm As Integer, Press As Integer, Power As Integer)
    Dim ObjGear = _Gears.Where(Function(x) x.Gear = Gear).FirstOrDefault
    If ObjGear Is Nothing Then
      ObjGear = New clsPedestalGear(Gear, _Hz, _TotalPeriodSeconds)
      _Gears.Add(ObjGear)
    End If

    With ObjGear
      .Rpm.Add(Rpm)
      .Press.Add(Press)
      .Power.Add(Power)
    End With

  End Sub

  Public Sub CalcolaValoriMedi()
    _TotalSecondsOfActivity = 0
    _TotalEnergyJoule = 0
    Dim pTot As Double = 0
    For Each G In _Gears
      _TotalSecondsOfActivity += G.SecondsOfActivity
      _TotalEnergyJoule += G.TotalEnergyJoule
      pTot += (G.PowerAvg * G.SecondsOfActivity)
    Next
    _PowerAvgWatt = pTot / _TotalSecondsOfActivity
  End Sub

  Public ReadOnly Property PowerAvgWatt As Double
    Get
      Return _PowerAvgWatt
    End Get
  End Property

  Public ReadOnly Property TotalSecondsOfActivity() As Double
    Get
      Return _TotalSecondsOfActivity
    End Get
  End Property

  Public ReadOnly Property AvgSecondsOfActivity() As Double
    Get
      Return TotalSecondsOfActivity / _Gears.Count
    End Get
  End Property

  Public ReadOnly Property TimePercOfActivity() As Double
    Get
      Return TotalSecondsOfActivity / _TotalPeriodSeconds * 100
    End Get
  End Property

  Public ReadOnly Property TotalEnergyJoule() As Double
    Get
      Return _TotalEnergyJoule
    End Get
  End Property

  Public ReadOnly Property TotalEnergyKcal() As Double
    Get
      Return JouleToKcal(TotalEnergyJoule)
    End Get
  End Property

  Public ReadOnly Property TextTable(Headers As Boolean) As String
    Get
      Dim strTmp As String = ""
      If Headers Then
        strTmp &= "SailingFunction" & vbTab
        strTmp &= "SecondsOfActivity" & vbTab & "ActiveTimePerc" & vbTab & "PowerAvgWatt" & vbTab & "EnergyJoule" & vbTab
        For i As Integer = 1 To 7
          'uno per marcia
          strTmp &= "G" & i & "ss" & vbTab
        Next
        For i As Integer = 1 To 7
          'uno per marcia
          strTmp &= "G" & i & "j" & vbTab
        Next
        For i As Integer = 1 To 7
          'uno per marcia
          strTmp &= "G" & i & "w" & vbTab
        Next
      Else
        strTmp &= _SailingFunctionDescription & vbTab
        strTmp &= TotalSecondsOfActivity.ToString("F0") & vbTab & TimePercOfActivity.ToString("F0") & vbTab & PowerAvgWatt.ToString("F0") & vbTab & TotalEnergyJoule.ToString("F0") & vbTab

        Dim Gs(7) As Double
        Dim Gj(7) As Double
        Dim Gw(7) As Double
        For Each Gr In Gears
          Gs(Gr.Gear) = Gr.SecondsOfActivity
          Gj(Gr.Gear) = Gr.TotalEnergyJoule
          Gw(Gr.Gear) = Gr.PowerAvg
        Next
        For i As Integer = 1 To 7
          strTmp &= Gs(i).ToString("F0") & vbTab
        Next
        For i As Integer = 1 To 7
          strTmp &= Gj(i).ToString("F0") & vbTab
        Next
        For i As Integer = 1 To 7
          strTmp &= Gw(i).ToString("F0") & vbTab
        Next
      End If
      Return strTmp.TrimEnd(vbTab)
    End Get
  End Property

  Public Property SailingFunctionDescription As String
    Get
      Return _SailingFunctionDescription
    End Get
    Set(value As String)
      _SailingFunctionDescription = value
    End Set
  End Property
End Class


Public Class clsPedestalGear
  ' valori puntuali per ciascuna marcia, funzione pedestal
  Dim _Gear As Integer
  Dim _Rpm As New List(Of Integer)
  Dim _Press As New List(Of Integer)
  Dim _Power As New List(Of Integer)
  Dim _Hz As Integer
  Dim _TotalPeriodSeconds As Double

  Public Sub New(Gear As Integer, Hz As Integer, TotalPeriodSeconds As Double)
    _Gear = Gear
    _Hz = Hz
    _TotalPeriodSeconds = TotalPeriodSeconds
  End Sub

  Public Property Gear As Integer
    Get
      Return _Gear
    End Get
    Set(value As Integer)
      _Gear = value
    End Set
  End Property

  Public Property Rpm As List(Of Integer)
    Get
      Return _Rpm
    End Get
    Set(value As List(Of Integer))
      _Rpm = value
    End Set
  End Property

  Public Property Press As List(Of Integer)
    Get
      Return _Press
    End Get
    Set(value As List(Of Integer))
      _Press = value
    End Set
  End Property

  Public Property Power As List(Of Integer)
    Get
      Return _Power
    End Get
    Set(value As List(Of Integer))
      _Power = value
    End Set
  End Property

  Public ReadOnly Property RpmAvg As Integer
    Get
      Return Rpm.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property RpmMax As Integer
    Get
      Return Rpm.Where(Function(x) Not Double.IsNaN(x)).Max
    End Get
  End Property

  Public ReadOnly Property RpmMin As Integer
    Get
      Return Rpm.Where(Function(x) Not Double.IsNaN(x)).Min
    End Get
  End Property

  Public ReadOnly Property PressAvg As Integer
    Get
      Return Press.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property PressMax As Integer
    Get
      Return Press.Where(Function(x) Not Double.IsNaN(x)).Max
    End Get
  End Property

  Public ReadOnly Property PressMin As Integer
    Get
      Return Press.Where(Function(x) Not Double.IsNaN(x)).Min
    End Get
  End Property

  Public ReadOnly Property PowerAvg As Integer
    Get
      Return Power.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property PowerMax As Integer
    Get
      Return Power.Where(Function(x) Not Double.IsNaN(x)).Max
    End Get
  End Property

  Public ReadOnly Property PowerMin As Integer
    Get
      Return Power.Where(Function(x) Not Double.IsNaN(x)).Min
    End Get
  End Property

  Public ReadOnly Property SecondsOfActivity() As Double
    Get
      Return Rpm.Count / _Hz
    End Get
  End Property

  Public ReadOnly Property TimePercOfActivity() As Double
    Get
      Return SecondsOfActivity / _TotalPeriodSeconds * 100
    End Get
  End Property

  Public ReadOnly Property TotalEnergyJoule() As Double
    Get
      Return PowerAvg * SecondsOfActivity
    End Get
  End Property

  Public ReadOnly Property TotalEnergyKcal() As Double
    Get
      Return JouleToKcal(TotalEnergyJoule)
    End Get
  End Property

  Public ReadOnly Property TextTable(Headers As Boolean) As String
    Get
      Dim strTmp As String = ""
      If Headers Then
        strTmp &= "Gear" & vbTab
        strTmp &= "SecondsOfActivity" & vbTab & "ActiveTimePerc" & vbTab & "EnergyJoule" & vbTab
        strTmp &= "PowerAvg" & vbTab & "PowerMax" & vbTab & "PowerMin" & vbTab
        strTmp &= "RpmAvg" & vbTab & "RpmMax" & vbTab & "RpmMin" & vbTab
        strTmp &= "PressAvg" & vbTab & "PressMax" & vbTab & "PressMin" & vbTab
      Else
        strTmp &= _Gear & vbTab
        strTmp &= SecondsOfActivity.ToString("F1") & vbTab & TimePercOfActivity.ToString("F1") & vbTab & TotalEnergyJoule.ToString("F0") & vbTab
        strTmp &= PowerAvg & vbTab & PowerMax & vbTab & PowerMin & vbTab
        strTmp &= RpmAvg & vbTab & RpmMax & vbTab & RpmMin & vbTab
        strTmp &= PressAvg & vbTab & PressMax & vbTab & PressMin & vbTab
      End If
      Return strTmp.TrimEnd(vbTab)
    End Get
  End Property

  Public Property TotalPeriodSeconds As Double
    Get
      Return _TotalPeriodSeconds
    End Get
    Set(value As Double)
      _TotalPeriodSeconds = value
    End Set
  End Property
End Class