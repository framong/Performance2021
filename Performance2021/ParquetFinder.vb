Imports System.Collections.ObjectModel
Imports System.IO
Imports SciChart.Charting2D.Interop

Public Class clsParquetFinder
  'cerca i file parquet con data conpresa nei criteri di ricerca
  'legge tutto il file oppure i soli periodi selezionati
  'crea un file parquet con i valori dei campi cercati pú quelli base obbligatori
  'crea il json con i periodi del criterio di selezione
  Dim _Tr As clsTimeRange
  Dim _ListaCanaliBase As New List(Of String)
  Dim _ListaFilePeriodi As New List(Of clsParquetAndPeriods)
  Dim _Variabili As New clsVariabiliFinder
  Dim _PathParquetFolder As String = "D:\Performance\Logs\AC75B1"
  Dim _StringaDescrizioneFiltro As String = ""

  Public Sub New(Tr As clsTimeRange)
    Tr = New clsTimeRange(Now.AddDays(-20), Now.AddDays(-10))
    _Tr = Tr
    ImpostaCanaliBase()
    TrovaPeriodi()
    EsportaPeriodi()
  End Sub

  Public Property Tr As clsTimeRange
    Get
      Return _Tr
    End Get
    Set(value As clsTimeRange)
      _Tr = value
    End Set
  End Property

  Public Property ListaFilePeriodi As List(Of clsParquetAndPeriods)
    Get
      Return _ListaFilePeriodi
    End Get
    Set(value As List(Of clsParquetAndPeriods))
      _ListaFilePeriodi = value
    End Set
  End Property

  'Public Property Periodi As List(Of clsPeriod2020)
  '  Get
  '    Return _Periodi
  '  End Get
  '  Set(value As List(Of clsPeriod2020))
  '    _Periodi = value
  '  End Set
  'End Property

  Private Sub ImpostaCanaliBase()
    _ListaCanaliBase.Clear()
    _ListaCanaliBase.Add("SystemTime_DaySeconds")
    _ListaCanaliBase.Add("PortOutFlap1_Ang")
    _ListaCanaliBase.Add("PortInFlap1_Ang")
    _ListaCanaliBase.Add("StbdOutFlap1_Ang")
    _ListaCanaliBase.Add("StbdOutFlap2_Ang")
    _ListaCanaliBase.Add("StbdInFlap1_Ang")
    '_ListaCanaliBase.Add("StbdInFlap2_Ang")
    _ListaCanaliBase.Add("RudderRake_Ang")
    '_ListaCanaliBase.Add("PortOutFlapAPActive")
    '_ListaCanaliBase.Add("PortInFlapAPActive")
    '_ListaCanaliBase.Add("StbdInFlapAPActive")
    '_ListaCanaliBase.Add("StbdOutFlapAPActive")
    '_ListaCanaliBase.Add("SystemTime_Local")
    '_ListaCanaliBase.Add("SystemTime_Date")
    _ListaCanaliBase.Add("LatBow")
    _ListaCanaliBase.Add("LonBow")
    _ListaCanaliBase.Add("Tws")
    _ListaCanaliBase.Add("Twd")
    _ListaCanaliBase.Add("Twa")
    _ListaCanaliBase.Add("Awa")
    _ListaCanaliBase.Add("Bs")
    _ListaCanaliBase.Add("Cog")
    _ListaCanaliBase.Add("Sog")
    _ListaCanaliBase.Add("BsTgt")
    _ListaCanaliBase.Add("TwaTgt")
    _ListaCanaliBase.Add("Heel")
    _ListaCanaliBase.Add("Aws")
    _ListaCanaliBase.Add("Hdg")
    _ListaCanaliBase.Add("Lwy")
    _ListaCanaliBase.Add("Cse")
    _ListaCanaliBase.Add("Pitch")
    _ListaCanaliBase.Add("Roll")
    _ListaCanaliBase.Add("AccX")
    _ListaCanaliBase.Add("AccY")
    _ListaCanaliBase.Add("AccZ")
    _ListaCanaliBase.Add("PitchRate")
    _ListaCanaliBase.Add("RollRate")
    _ListaCanaliBase.Add("YawRate")
    '_ListaCanaliBase.Add("AbsTwa")
    _ListaCanaliBase.Add("Trim")
    '_ListaCanaliBase.Add("TrimRate")
    '_ListaCanaliBase.Add("HeelRate")
    '_ListaCanaliBase.Add("Vmg")
    '_ListaCanaliBase.Add("Battery_Current")
    '_ListaCanaliBase.Add("Battery_Voltage")
    '_ListaCanaliBase.Add("Battery_Temp")
    '_ListaCanaliBase.Add("SailingPP_Temp")
    '_ListaCanaliBase.Add("FoilingPP_Temp")
    '_ListaCanaliBase.Add("PortTwistLine1_Load")
    '_ListaCanaliBase.Add("PortTwistLine2_Load")
    '_ListaCanaliBase.Add("StbdTwistLine1_Load")
    '_ListaCanaliBase.Add("StbdTwistLine2_Load")
    '_ListaCanaliBase.Add("MainCunnPortLine_Load")
    '_ListaCanaliBase.Add("MainCunnStbdLine_Load")
    '_ListaCanaliBase.Add("PortV1Pin_Load")
    '_ListaCanaliBase.Add("PortD1Pin_Load")
    '_ListaCanaliBase.Add("StbdV1Pin_Load")
    '_ListaCanaliBase.Add("StbdD1Pin_Load")
    _ListaCanaliBase.Add("ForestayPin_Load")
    '_ListaCanaliBase.Add("PortTravRam_Load")
    '_ListaCanaliBase.Add("StbdTravRam_Load")
    '_ListaCanaliBase.Add("SpannerRam_Load")
    '_ListaCanaliBase.Add("TwistRam_Load")
    '_ListaCanaliBase.Add("TopPostRam_Load")
    '_ListaCanaliBase.Add("PortJibUpDwRam_Load")
    '_ListaCanaliBase.Add("StbdJibUpDwRam_Load")
    '_ListaCanaliBase.Add("MainOuthaulRam_Load")
    '_ListaCanaliBase.Add("MainCunnRam_Load")
    '_ListaCanaliBase.Add("JibInhaulRam_Load")
    '_ListaCanaliBase.Add("JibOuthaulRam_Load")
    '_ListaCanaliBase.Add("JibCunnRam_Load")
    '_ListaCanaliBase.Add("ForestayRam_Load")
    '_ListaCanaliBase.Add("PortV1Strain_Load")
    '_ListaCanaliBase.Add("PortD1Strain_Load")
    '_ListaCanaliBase.Add("StbdV1Strain_Load")
    '_ListaCanaliBase.Add("StbdD1Strain_Load")
    '_ListaCanaliBase.Add("BobstayStrain_Load")
    '_ListaCanaliBase.Add("StbdJibSheetRam_Load")
    '_ListaCanaliBase.Add("PortJibSheetRam_Load")
    '_ListaCanaliBase.Add("StbdRunnerRam_Load")
    '_ListaCanaliBase.Add("PortRunnerRam_Load")
    '_ListaCanaliBase.Add("MainSheetRam_Load")
    _ListaCanaliBase.Add("MainSheetPlusOuthaul_Load")
    '_ListaCanaliBase.Add("PortOutFlapRam1_Load")
    '_ListaCanaliBase.Add("PortOutFlapRam2_Load")
    '_ListaCanaliBase.Add("PortInFlapRam1_Load")
    '_ListaCanaliBase.Add("PortInFlapRam2_Load")
    '_ListaCanaliBase.Add("StbdOutFlapRam1_Load")
    '_ListaCanaliBase.Add("StbdOutFlapRam2_Load")
    '_ListaCanaliBase.Add("StbdInFlapRam1_Load")
    '_ListaCanaliBase.Add("StbdInFlapRam2_Load")
    '_ListaCanaliBase.Add("RudderRakeRam_Load")
    '_ListaCanaliBase.Add("Traveller_Load")
    '_ListaCanaliBase.Add("PortCantRam_Load")
    '_ListaCanaliBase.Add("StbdCantRam_Load")
    '_ListaCanaliBase.Add("JibCunnRam_Pos")
    '_ListaCanaliBase.Add("FlapsTank_Pos")
    '_ListaCanaliBase.Add("StbdMainTank_Pos")
    '_ListaCanaliBase.Add("PortMainTank_Pos")
    '_ListaCanaliBase.Add("PowerPackTank_Pos")
    '_ListaCanaliBase.Add("PortJibUpDwRam_Pos")
    '_ListaCanaliBase.Add("StbdJibUpDwRam_Pos")
    '_ListaCanaliBase.Add("PortJibSheetTank_Pos")
    '_ListaCanaliBase.Add("StbdJibSheetTank_Pos")
    '_ListaCanaliBase.Add("JibInhaulRam_Pos")
    '_ListaCanaliBase.Add("JibOuthaulRam_Pos")
    '_ListaCanaliBase.Add("PortJibSheetRam_Pos")
    '_ListaCanaliBase.Add("StbdJibSheetRam_Pos")
    '_ListaCanaliBase.Add("MainOuthaulRam_Pos")
    '_ListaCanaliBase.Add("TwistRam_Pos")
    '_ListaCanaliBase.Add("PortTravRam_Pos")
    '_ListaCanaliBase.Add("StbdTravRam_Pos")
    '_ListaCanaliBase.Add("MainSheetRam_Pos")
    '_ListaCanaliBase.Add("MainSheetTank_Pos")
    '_ListaCanaliBase.Add("ForestayRam_Pos")
    '_ListaCanaliBase.Add("SpannerRam_Pos")
    '_ListaCanaliBase.Add("TopPostRam_Pos")
    '_ListaCanaliBase.Add("StbdRunnerRam_Pos")
    '_ListaCanaliBase.Add("StbdRunnerTank_Pos")
    '_ListaCanaliBase.Add("PortRunnerRam_Pos")
    '_ListaCanaliBase.Add("PortRunnerTank_Pos")
    _ListaCanaliBase.Add("Rudder_Ang")
    '_ListaCanaliBase.Add("TopCtrlArm_Ang")
    '_ListaCanaliBase.Add("Traveller_Ang")
    '_ListaCanaliBase.Add("PortTravRam_Ang")
    '_ListaCanaliBase.Add("StbdTravRam_Ang")
    _ListaCanaliBase.Add("Spanner_Ang")
    '_ListaCanaliBase.Add("Twist_Ang")
    '_ListaCanaliBase.Add("TopMastWindUnit_Ang")
    '_ListaCanaliBase.Add("BowWindUnit_Ang")
    '_ListaCanaliBase.Add("TopMastWindUnit_Speed")
    '_ListaCanaliBase.Add("BowWindUnit_Speed")
    '_ListaCanaliBase.Add("FCS_PortCant_Ang")
    '_ListaCanaliBase.Add("FCS_StbdCant_Ang")
    '_ListaCanaliBase.Add("FCS_PortRam_Pos")
    '_ListaCanaliBase.Add("FCS_StbdRam_Pos")
    '_ListaCanaliBase.Add("FCS_PortRam_Spd")
    '_ListaCanaliBase.Add("FCS_StbdRam_Spd")
    '_ListaCanaliBase.Add("FCS_StorageLevel_Pos")
    '_ListaCanaliBase.Add("FCS_Oil_Temp")
    '_ListaCanaliBase.Add("FCS_Battery_Volt")
    '_ListaCanaliBase.Add("FCS_Motor_Temp")
    '_ListaCanaliBase.Add("FCS_MotorCtrl_Temp")
    '_ListaCanaliBase.Add("FCS_MotorCmd_RPM")
    '_ListaCanaliBase.Add("FCS_MotorAct_RPM")
    '_ListaCanaliBase.Add("FCS_Battery_Amp")
    '_ListaCanaliBase.Add("PortBulbRideHeight_AP_m")
    '_ListaCanaliBase.Add("StbdBulbRideHeight_AP_m")
    '_ListaCanaliBase.Add("PortTipInRideHeight_AP_m")
    '_ListaCanaliBase.Add("StbdTipInRideHeight_AP_m")
    '_ListaCanaliBase.Add("PortTipOutRideHeight_AP_m")
    '_ListaCanaliBase.Add("StbdTipOutRideHeight_AP_m")
    '_ListaCanaliBase.Add("RudderRideHeight_AP_m")
    '_ListaCanaliBase.Add("SinkMin_AP")
    '_ListaCanaliBase.Add("PortBulbSpeed_AP_kts")
    '_ListaCanaliBase.Add("StbdBulbSpeed_AP_kts")
    '_ListaCanaliBase.Add("RudderSpeed_AP_kts")
    '_ListaCanaliBase.Add("WaveForm_AP")
    '_ListaCanaliBase.Add("WaveCrest_AP")
    '_ListaCanaliBase.Add("WaveTrough_AP")
    '_ListaCanaliBase.Add("PortFlapInTgt_1_AP_deg")
    '_ListaCanaliBase.Add("PortFlapInTgt_2_AP_deg")
    '_ListaCanaliBase.Add("PortFlapOutTgt_1_AP_deg")
    '_ListaCanaliBase.Add("PortFlapOutTgt_2_AP_deg")
    '_ListaCanaliBase.Add("StbdFlapInTgt_1_AP_deg")
    '_ListaCanaliBase.Add("StbdFlapInTgt_2_AP_deg")
    '_ListaCanaliBase.Add("StbdFlapOutTgt_1_AP_deg")
    '_ListaCanaliBase.Add("StbdFlapOutTgt_2_AP_deg")
    '_ListaCanaliBase.Add("RudderRakeTgt_AP_deg")
    '_ListaCanaliBase.Add("ZDatum_AP")
    '_ListaCanaliBase.Add("SWH_AP")
    '_ListaCanaliBase.Add("TowLine_Load")
    '_ListaCanaliBase.Add("RudderYaw_Ang")
    '_ListaCanaliBase.Add("JibInOut_Ang")
    '_ListaCanaliBase.Add("JibInhaulRam_Ang")
    '_ListaCanaliBase.Add("JibOuthaulRam_Ang")
    _ListaCanaliBase.Add("TwaBow")
    _ListaCanaliBase.Add("TwsBow")
    _ListaCanaliBase.Add("TwdBow")
    _ListaCanaliBase.Add("ChazTwd")
    _ListaCanaliBase.Add("ChazTws")
    _ListaCanaliBase.Add("JibCar_Ang")
    '_ListaCanaliBase.Add("PortJibCar_Ang")
    '_ListaCanaliBase.Add("StbdJibCar_Ang")
    '_ListaCanaliBase.Add("PortArmIB1_Strain")
    '_ListaCanaliBase.Add("PortArmIB2_Strain")
    '_ListaCanaliBase.Add("PortArmIB3_Strain")
    '_ListaCanaliBase.Add("PortArmOB1_Strain")
    '_ListaCanaliBase.Add("PortArmOB2_Strain")
    '_ListaCanaliBase.Add("PortArmOB3_Strain")
    '_ListaCanaliBase.Add("StbdArmIB1_Strain")
    '_ListaCanaliBase.Add("StbdArmIB2_Strain")
    '_ListaCanaliBase.Add("StbdArmIB3_Strain")
    '_ListaCanaliBase.Add("StbdArmOB1_Strain")
    '_ListaCanaliBase.Add("StbdArmOB2_Strain")
    '_ListaCanaliBase.Add("StbdArmOB3_Strain")
    '_ListaCanaliBase.Add("PortWingIB1_Strain")
    '_ListaCanaliBase.Add("PortWingIB2_Strain")
    '_ListaCanaliBase.Add("PortWingIB3_Strain")
    '_ListaCanaliBase.Add("PortWingOB1_Strain")
    '_ListaCanaliBase.Add("PortWingOB2_Strain")
    '_ListaCanaliBase.Add("PortWingOB3_Strain")
    '_ListaCanaliBase.Add("StbdWingIB1_Strain")
    '_ListaCanaliBase.Add("StbdWingIB2_Strain")
    '_ListaCanaliBase.Add("StbdWingIB3_Strain")
    '_ListaCanaliBase.Add("StbdWingOB1_Strain")
    '_ListaCanaliBase.Add("StbdWingOB2_Strain")
    '_ListaCanaliBase.Add("StbdWingOB3_Strain")
    '_ListaCanaliBase.Add("RudderPort1_Strain")
    '_ListaCanaliBase.Add("ElevatorPort1_Strain")
    '_ListaCanaliBase.Add("ElevatorPort2_Strain")
    '_ListaCanaliBase.Add("RudderStbd1_Strain")
    '_ListaCanaliBase.Add("ElevatorStbd1_Strain")
    '_ListaCanaliBase.Add("ElevatorStbd2_Strain")
    '_ListaCanaliBase.Add("MDPed1EffectiveFunction")
    '_ListaCanaliBase.Add("PumpLine1_Press")
    '_ListaCanaliBase.Add("Pump1_Gear")
    '_ListaCanaliBase.Add("Pump1_RPM")
    '_ListaCanaliBase.Add("MDPed2EffectiveFunction")
    '_ListaCanaliBase.Add("PumpLine2_Press")
    '_ListaCanaliBase.Add("Pump2_Gear")
    '_ListaCanaliBase.Add("Pump2_RPM")
    '_ListaCanaliBase.Add("MDPed3EffectiveFunction")
    '_ListaCanaliBase.Add("PumpLine3_Press")
    '_ListaCanaliBase.Add("Pump3_Gear")
    '_ListaCanaliBase.Add("Pump3_RPM")
    '_ListaCanaliBase.Add("MDPed4EffectiveFunction")
    '_ListaCanaliBase.Add("PumpLine4_Press")
    '_ListaCanaliBase.Add("Pump4_Gear")
    '_ListaCanaliBase.Add("Pump4_RPM")
    '_ListaCanaliBase.Add("MainLine1_Press")
    '_ListaCanaliBase.Add("MainLine2_Press")
    '_ListaCanaliBase.Add("MainLine3_Press")
    '_ListaCanaliBase.Add("MainLine4_Press")
    '_ListaCanaliBase.Add("TopMastIMU_Hdg")
    '_ListaCanaliBase.Add("TopMastIMU_Pitch")
    '_ListaCanaliBase.Add("TopMastIMU_Roll")
    '_ListaCanaliBase.Add("PortJibCar_Pos")
    '_ListaCanaliBase.Add("StbdJibCar_Pos")
    '_ListaCanaliBase.Add("JibCunnLine_Load")
    '_ListaCanaliBase.Add("JibPennant_Load")
    '_ListaCanaliBase.Add("MainTravLine_Load")
    '_ListaCanaliBase.Add("StbdJibSheetLine_Load")
    '_ListaCanaliBase.Add("PortJibSheetLine_Load")
    '_ListaCanaliBase.Add("RideHeightSensor_m")
    '_ListaCanaliBase.Add("StbdJibSheetRam1_Press")
    '_ListaCanaliBase.Add("StbdJibSheetRam2_PressA")
    '_ListaCanaliBase.Add("StbdJibSheetRam2_PressB")
    '_ListaCanaliBase.Add("PortJibSheetRam1_Press")
    '_ListaCanaliBase.Add("PortJibSheetRam2_PressA")
    '_ListaCanaliBase.Add("PortJibSheetRam2_PressB")
    '_ListaCanaliBase.Add("Pump1SmallChamber")
    '_ListaCanaliBase.Add("Pump2SmallChamber")
    '_ListaCanaliBase.Add("PortJibSheetDI0_ToggleFastEase")
    '_ListaCanaliBase.Add("StbdJibSheetDI0_ToggleFastEase")
    '_ListaCanaliBase.Add("RudderStbd2_Strain")
    '_ListaCanaliBase.Add("RudderPort2_Strain")
    '_ListaCanaliBase.Add("ElevatorStbd2P45_Strain")
    '_ListaCanaliBase.Add("ElevatorStbd2M45_Strain")
    '_ListaCanaliBase.Add("ElevatorPort2P45_Strain")
    '_ListaCanaliBase.Add("ElevatorPort2M45_Strain")
    '_ListaCanaliBase.Add("V_BowWindUnitMeasuredRaw_MWA")
    '_ListaCanaliBase.Add("V_BowWindUnitMeasuredRaw_MWS")
    '_ListaCanaliBase.Add("V_TopMastWindUnitMeasuredRaw_MWA")
    '_ListaCanaliBase.Add("V_TopMastWindUnitMeasuredRaw_MWS")
    '_ListaCanaliBase.Add("TwaMast")
    '_ListaCanaliBase.Add("TwsMast")
    '_ListaCanaliBase.Add("TwdMast")
    '_ListaCanaliBase.Add("AwaMast")
    '_ListaCanaliBase.Add("AwsMast")
    '_ListaCanaliBase.Add("AwaBow")
    '_ListaCanaliBase.Add("AwsBow")
    '_ListaCanaliBase.Add("RudderPortLine_Load")
    '_ListaCanaliBase.Add("RudderStbdLine_Load")
    '_ListaCanaliBase.Add("FootCamber")
    '_ListaCanaliBase.Add("BotAngle_Ang")
    '_ListaCanaliBase.Add("PortTravRam_Press")
    '_ListaCanaliBase.Add("StbdTravRam_Press")
    '_ListaCanaliBase.Add("SC_MainSail")
    '_ListaCanaliBase.Add("SC_HeadSail")
    '_ListaCanaliBase.Add("SC_JibClewBoard")
    '_ListaCanaliBase.Add("SC_ConfigId")
    '_ListaCanaliBase.Add("TopMastCant_AP_deg")
    '_ListaCanaliBase.Add("TopMastCantTack_AP_deg")
    '_ListaCanaliBase.Add("TopMastRake_AP_deg")
    '_ListaCanaliBase.Add("TopMastTwist_AP_deg")
    '_ListaCanaliBase.Add("Elevator1Delta_Strain")
    '_ListaCanaliBase.Add("Elevator2Delta_Strain

  End Sub


  Private Sub TrovaPeriodi()
    _ListaFilePeriodi.Clear()
    Dim ParquetFolder As New DirectoryInfo(_PathParquetFolder)
    Dim NrPeriodiTrovati As Integer = 0

    For Each Fld In ParquetFolder.GetDirectories
      Dim DataSessione As DateTime = Nothing
      If DateTime.TryParseExact(Fld.Name, "yyyyMMdd", meCultureInfo, Globalization.DateTimeStyles.None, DataSessione) Then
        Dim PF As clsParquetFile = Nothing
        Dim PeriodsFiles As New List(Of String)
        For Each file In Fld.GetFiles
          If file.Name.StartsWith("Performance_") AndAlso file.Name.EndsWith(".parquet") Then
            Dim pTmp As New clsParquetFile(file.FullName, False)
            'pTmp.LeggiFileParquet(True)
            If Tr Is Nothing Then
              PF = pTmp
              'Exit For
            Else
              If CInt(DataSessione.ToOADate) >= CInt(_Tr.Start.ToOADate) Then
                If CInt(DataSessione.ToOADate) <= CInt(_Tr.Finish.ToOADate) Then
                  PF = pTmp
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
        If Not PF Is Nothing Then
          For Each FilePeriodo In PeriodsFiles
            'carica i nuovi periodi dal file json
            Dim PeriodiTrovati = CercaPeriodiFile(FilePeriodo)
            If PeriodiTrovati.Count > 0 Then
              Dim PeriodiValidi As List(Of clsPeriod2020) = FiltroPeriodi(PeriodiTrovati, PF)
              If Not PeriodiValidi Is Nothing Then
                Dim PeriodiUnivoci As New List(Of clsPeriod2020)
                VerificaPeriodiUnivoci(PeriodiValidi, PeriodiUnivoci)
                Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " " & PF.FilePath)
                If PeriodiUnivoci.Count > 0 Then
                  NrPeriodiTrovati += PeriodiUnivoci.Count
                  _ListaFilePeriodi.Add(New clsParquetAndPeriods(PF, PeriodiUnivoci))
                End If
              End If
            End If
          Next
        End If
        PF = Nothing
      Else
        Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " INVALID DATA FOLDER NAME: " & Fld.Name)
      End If
    Next

  End Sub

  Private Sub VerificaPeriodiUnivoci(PeriodiFileCorrente As List(Of clsPeriod2020), ByRef ListaCompletaPeriodi As List(Of clsPeriod2020))
    For Each p In PeriodiFileCorrente
      If ListaCompletaPeriodi.Where(Function(x) x.TimeRange.IsSameRange(p.TimeRange, True)).Count = 0 Then
        ListaCompletaPeriodi.Add(p)
      End If
    Next
  End Sub


  Public Function CercaPeriodiFile(PathFile As String) As List(Of clsPeriod2020)
    Dim l As New List(Of clsPeriod2020)
    Try
      If PathFile.EndsWith(".prd") Then
        ' cerca nei vecchi prd
        l = LoadTestFromPrdFile(PathFile)
      ElseIf PathFile.EndsWith(".json") Then
        ' carica i file da eventuali file json esistenti
        Dim jj = clsKillerSeriale.LoadConfigurationGeneric(Of clsPeriodsJson)(PathFile)
        If jj.ListaPeriodi.Count > 0 Then
          ' file json del performer
          l = jj.ListaPeriodi.ToList
          'Else
          '  ' file json del testmanager
          '  l = LoadTestFromJsonFile(PathFile, False)
        End If
      End If
    Catch ex As Exception

    End Try
    Return l
  End Function

  Public Function LoadTestFromPrdFile(PathPrdFile As String) As List(Of clsPeriod2020)
    Dim Ltmp As New List(Of clsPeriod2020)
    Dim FilePrd = New clsSettings(PathPrdFile, AppConfig.Utente, "PeriodsList") 'pSuffisso) ' ObjFiles.PulisciCaratteriBastardi(ObjDataProvider.FI.Name) & "_PeriodsManager")
    Dim NodoPeriodi As Xml.XmlNode = FilePrd.CercaNodo("PeriodsList", clsSettings.eNodoSTD.ePeriods, True)
    If Not NodoPeriodi Is Nothing Then
      For Each Nodo As Xml.XmlNode In NodoPeriodi
        Dim Inizio As Double
        Dim Fine As Double
        Dim PeriodType As clsPeriod2020.ePeriodType = clsPeriod2020.ePeriodType.eUndefined
        Dim KeyMoment As DateTime = Nothing
        Dim Keys As String = ""
        Dim ShortDescription As String = ""
        Dim ExtendedDescription As String = ""
        'Dim SqlId As Integer = -1

        For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
          Select Case SottoNodo.Name
            Case "From"
              Inizio = SottoNodo.InnerText
            Case "To"
              Fine = SottoNodo.InnerText
            Case "PeriodType"
              PeriodType = CInt(SottoNodo.InnerText)
            Case "Keys"
              Keys = SottoNodo.InnerText.Trim
            Case "KeyMoment"
              Dim StrTmp As String = SottoNodo.InnerText
              If IsNumeric(StrTmp) AndAlso CDbl(StrTmp) > 0 Then
                KeyMoment = DateTime.FromOADate(StrTmp)
              Else
                KeyMoment = Nothing
              End If
            Case "ShortDescription"
              ShortDescription = SottoNodo.InnerText
            Case "ExtendedDescription"
              ExtendedDescription = SottoNodo.InnerText
              'Case "SqlId"
              '  SqlId = SottoNodo.InnerText
          End Select
        Next
        If Not PeriodType = clsPeriod2020.ePeriodType.eUndefined Then
          Dim TmRg As New clsTimeRange(DateTime.FromOADate(Inizio), DateTime.FromOADate(Fine))
          Dim PeriodoTmp As New clsPeriod2020(TmRg, PeriodType) ' , SqlId) ' , TWA, TWD, TWS, BS)
          PeriodoTmp.KeyMoment = KeyMoment
          PeriodoTmp.Keys = Keys
          PeriodoTmp.ExtendedDescription = ExtendedDescription
          PeriodoTmp.ShortDescription = ShortDescription
          Ltmp.Add(PeriodoTmp)
        End If
      Next
    End If
    Return Ltmp
  End Function

  Private Function FiltroPeriodi(Periodi As List(Of clsPeriod2020), Parquet As clsParquetFile) As List(Of clsPeriod2020)
    Dim Risultato As New List(Of clsPeriod2020)

    Try
      Parquet.LeggiFileParquet(False)
      'Dim fsTmp As New System.IO.StreamReader(pFileInfo.FullName)
      'Dim ParquetReader As Parquet.ParquetReader = New Parquet.ParquetReader(fsTmp.BaseStream)

      Dim ListaFiltri As New List(Of clsFiltroParquetFinder)
      ListaFiltri.Add(New clsFiltroParquetFinder("Bs", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMin, 10, 0))
      ListaFiltri.Add(New clsFiltroParquetFinder("Tws", Parquet, clsFiltroParquetFinder.eTipoFiltro.eBetween, 12, 14))
      'ListaFiltri.Add(New clsFiltroParquetFinder("Stocazzo", Parquet, clsFiltroParquetFinder.eTipoFiltro.eBetween, 12, 14))
      Parquet.ParquetReader.Dispose()

      'ListaFiltri.Add(New clsFiltroParquetFinder("Bs", Parquet, clsFiltroParquetFinder.eTipoFiltro.eMin, 0, 10))
      'Dim ValoriFiltro1 As Double() = Parquet.FileParquet.CaricaValoriCanale("Bs") 'canale filtro
      'Dim ValoriFiltro2 As Double() = Parquet.FileParquet.CaricaValoriCanale("Tws") 'canale filtro 2
      'Dim ValoriFiltro3 As Double() = Parquet.FileParquet.CaricaValoriCanale("") 'canale filtro
      Dim TipoPeriodi = ListaTipoPeriodi()
      For Each filtro In ListaFiltri
        If filtro.ValidFilter Then
          _StringaDescrizioneFiltro &= "_" & filtro.DescrizioneFiltro("")
        End If
      Next

      For Each Periodo In Periodi
        If TipoPeriodi.Count = 0 OrElse TipoPeriodi.Where(Function(x) x = Periodo.PeriodType).Count > 0 Then
          For i As Integer = 0 To Parquet.FileParquet.Momenti.Count - 1
            If Parquet.FileParquet.Momenti(i) > Periodo.TimeRange.Start Then
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
                Risultato.Add(Periodo)
                Exit For
              End If
              'If ValoriFiltro1(i) > 10 Then
              '  'filtro positivo aggiunge il periodo e passa al successivo
              '  If ValoriFiltro2(i) > 8 Then
              '    Risultato.Add(Periodo)
              '    Exit For
              '  End If
              'End If
              If Parquet.FileParquet.Momenti(i) >= Periodo.TimeRange.Finish Then
                'timerange dl periodo superato passa al successivo
                Exit For
              End If
            End If
          Next
        End If
      Next
      Return Risultato
    Catch ex As Exception
      Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " ERROR reading file: " & Parquet.FilePath)
      Return Nothing
    End Try
  End Function

  Private Function ListaTipoPeriodi() As List(Of clsPeriod2020.ePeriodType)
    Dim ListaTmp As New List(Of clsPeriod2020.ePeriodType)
    ListaTmp.Add(clsPeriod2020.ePeriodType.eAcceleration)
    ListaTmp.Add(clsPeriod2020.ePeriodType.eGybe)
    ListaTmp.Add(clsPeriod2020.ePeriodType.eTack)
    ListaTmp.Add(clsPeriod2020.ePeriodType.eStraightLine)

    'ListaTmp.Add(clsPeriod2020.ePeriodType.eBrAway)
    'ListaTmp.Add(clsPeriod2020.ePeriodType.eFinish)
    'ListaTmp.Add(clsPeriod2020.ePeriodType.eMultiManoeuvres)
    'ListaTmp.Add(clsPeriod2020.ePeriodType.eRoundUp)
    'ListaTmp.Add(clsPeriod2020.ePeriodType.eStart)

    If Tr Is Nothing Then
      _StringaDescrizioneFiltro = "WholeArchive_"
    Else
      _StringaDescrizioneFiltro = "From" & Tr.Start.ToString("yyyyMMdd") & "To" & Tr.Finish.ToString("yyyyMMdd") & "_"
    End If
    If ListaTmp.Count = 0 Then
      _StringaDescrizioneFiltro &= "AllPeriodTypes"
    Else
      For Each Tipo In ListaTmp
        _StringaDescrizioneFiltro &= Tipo.ToString.Substring(1) & ""
      Next
    End If

    Return ListaTmp
  End Function


  Public Sub EsportaPeriodi()
    _Variabili.Lista.Clear()
    For Each fp In _ListaFilePeriodi
      RiempiVariabili(fp)
    Next
    'Stop
    SalvaParquet(_PathParquetFolder & "\SearchResults_" & Now.ToString("yyyyMMdd") & "\")
  End Sub


  Private Sub RiempiVariabili(FP As clsParquetAndPeriods)
    Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " RiempiVariabiliDi: " & FP.Parquet.FilePath)

    Dim strTmp As String = ""
    FP.Parquet.LeggiFileParquet(False)
    Dim VariabiliParquetCorrente As New clsVariabiliFinder

    For Each canale In FP.Parquet.FileParquet.Intestazioni
      VariabiliParquetCorrente.Lista.Add(New clsVariabileFinder(canale, canale = "SystemTime_DaySeconds"))
      'VariabiliParquetCorrente.Lista.Add(New clsVariabileFinder(canale, canale = "SystemTime_Date"))
      If _Variabili.Lista.Where(Function(x) x.Nome = canale).Count = 0 Then
        'la aggiunge alla lista di tutti i file se non esiste
        _Variabili.Lista.Add(New clsVariabileFinder(canale, canale = "SystemTime_DaySeconds"))
      End If
      Dim PqDataField As Parquet.Data.DataField = FP.Parquet.ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = canale).FirstOrDefault
      If VariabiliParquetCorrente.Lista.Last.IsDT Then
        VariabiliParquetCorrente.Lista.Last.ValoriDT = FP.Parquet.FileParquet.Momenti.ToList
      Else
        VariabiliParquetCorrente.Lista.Last.Valori = RiempiCanale(FP, canale, FP.Parquet.ParquetReader, PqDataField)
      End If

    Next


    For Each Periodo In FP.Periods
      Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " RiempiValoriPeriodo: " & Periodo.PeriodType.ToString & " " & Periodo.TimeRange.StringaPeriodo)
      For i As Integer = 0 To FP.Parquet.FileParquet.Momenti.Count - 1
        If FP.Parquet.FileParquet.Momenti(i) >= Periodo.TimeRange.Start Then
          ' aggiunge il valore
          For Each Variabile In _Variabili.Lista
            Dim IdCampo As Integer = FP.Parquet.FileParquet.Intestazioni.FindIndex(Function(x) x = Variabile.Nome)
            If IdCampo = -1 Then
              If Variabile.IsDT Then
                Variabile.ValoriDT.Add(Nothing)
              Else
                Variabile.Valori.Add(Double.NaN)
              End If
            Else
              If Variabile.IsDT Then
                Variabile.ValoriDT.Add(FP.Parquet.FileParquet.Momenti(i))
                'Variabile.Valori.Add(FP.Parquet.FileParquet.Momenti(i).ToOADate)
              Else
                Variabile.Valori.Add(VariabiliParquetCorrente.Lista(IdCampo).Valori(i))
              End If
            End If

          Next
          If FP.Parquet.FileParquet.Momenti(i) > Periodo.TimeRange.Finish Then
            Exit For
          End If
        End If
      Next
    Next
    FP.Parquet.ParquetReader.Dispose()

  End Sub


  Private Function RiempiCanale(FP As clsParquetAndPeriods, NomeCanale As String, ParquetReader As Parquet.ParquetReader, PqDataFiled As Parquet.Data.DataField) As List(Of Double?)
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
  End Function



  Private Sub SalvaParquet(NewFileFolder As String)
    Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva Parquet")

    Dim pathParquet As String = NewFileFolder & _StringaDescrizioneFiltro & ".parquet"

    If Not System.IO.Directory.Exists(NewFileFolder) Then
      System.IO.Directory.CreateDirectory(NewFileFolder)
    End If

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In _Variabili.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      'DataFields.Add(DataField)
      If canale.Nome = "SystemTime_DaySeconds" Then
        CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.ValoriDT.Select(Function(x) x.TimeOfDay.TotalSeconds).ToArray))
        Dim DataFieldData As New Parquet.Data.DataField("SystemTime_Date", Parquet.Data.DataType.Double, False)
        CanaliPq.Add(New Parquet.Data.DataColumn(DataFieldData, canale.ValoriDT.Select(Function(x) CDbl(x.ToString("yyyyMMdd"))).ToArray))
        Dim DataFieldPerf As New Parquet.Data.DataField("SystemTime_Performance", Parquet.Data.DataType.Double, False)
        CanaliPq.Add(New Parquet.Data.DataColumn(DataFieldPerf, canale.ValoriDT.Select(Function(x) x.ToOADate).ToArray))
      Else
        Dim valori(canale.Valori.Count - 1) As Double
        For i As Integer = 0 To valori.Count - 1
          valori(i) = canale.Valori(i)
        Next
        CanaliPq.Add(New Parquet.Data.DataColumn(DataField, valori))
      End If
    Next
    Dim fsTmp As New System.IO.StreamWriter(pathParquet)
    'Dim PqSchema As New Parquet.Data.Schema(DataFields.ToList)
    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)
    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup
    For Each Canale In CanaliPq
      GroupWriter.WriteColumn(Canale)
    Next
    ParquetWriter.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()

    'crea il file json dei periodi
    Dim pathJson As String = NewFileFolder & _StringaDescrizioneFiltro & ".json"
    Dim l As New ObservableCollection(Of clsPeriod2020)
    For Each fp In _ListaFilePeriodi
      For Each p In fp.Periods
        l.Add(p)
      Next
    Next
    Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Inizio Salva Json Periodi")

    Dim JJ As New clsPeriodsJson
    JJ.ListaPeriodi = l
    clsKillerSeriale.SaveConfigurationGeneric(Of clsPeriodsJson)(JJ, pathJson)


    'PeriodsManager.SalvaPeriodiJsonFile(l, pathJson)
    Console.WriteLine(Now.ToString("HH:mm:ss.fff") & " Fine!")

    ApriExplorer(NewFileFolder, pathParquet)
  End Sub

End Class

Public Class clsFiltroParquetFinder
  Dim _NomeCanale As String
  Dim _ValoriCanale As Double()
  Dim _TipoFiltro As eTipoFiltro
  Dim _UpperLimit As Double
  Dim _LowerLimit As Double
  Dim _Parquet As clsParquetFile
  Dim _ValidFilter As Boolean = True

  Public Property ValidFilter As Boolean
    Get
      Return _ValidFilter
    End Get
    Set(value As Boolean)
      _ValidFilter = value
    End Set
  End Property

  Public Enum eTipoFiltro
    eBetween
    eMin
    eMax
    eNone
  End Enum

  Public Sub New(NomeCanale As String, Parquet As clsParquetFile, TipoFiltro As eTipoFiltro, LowerLimit As Double, UpperLimit As Double)
    _Parquet = Parquet
    _NomeCanale = NomeCanale
    _TipoFiltro = TipoFiltro
    _LowerLimit = LowerLimit
    _UpperLimit = UpperLimit
    _ValoriCanale = Parquet.FileParquet.CaricaValoriCanale(NomeCanale, Parquet.ParquetReader)
    If _ValoriCanale Is Nothing Then _ValidFilter = False
    If TipoFiltro = eTipoFiltro.eNone Then _ValidFilter = False
  End Sub

  Public Function IsValid(Indice As Integer) As Boolean
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return Valore(Indice) <= _UpperLimit AndAlso Valore(Indice) >= _LowerLimit
      Case eTipoFiltro.eMax
        Return Valore(Indice) <= _UpperLimit
      Case eTipoFiltro.eMin
        Return Valore(Indice) >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Public Function Valore(Indice As Integer) As Double
    Return _ValoriCanale(Indice)
  End Function

  Public Function DescrizioneFiltro(Separatore As String) As String
    Dim strTmp As String = _NomeCanale & Separatore
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        strTmp &= "Between" & _LowerLimit.ToString("F0") & "And" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMax
        strTmp &= "Max" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMin
        strTmp &= "Min" & _LowerLimit.ToString("F0") '.Replace(".", "d")
      Case Else
        strTmp = ""
    End Select
    Return strTmp
  End Function

End Class

Public Class clsVariabiliFinder
  Dim pLista As New List(Of clsVariabileFinder)

  Public Property Lista As List(Of clsVariabileFinder)
    Get
      Return pLista
    End Get
    Set(value As List(Of clsVariabileFinder))
      pLista = value
    End Set
  End Property

End Class

Public Class clsVariabileFinder
  Dim _NomeCanale As String
  Dim _IsDT As Boolean = False
  Dim _Valori As New List(Of Double?)
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
End Class

Public Class clsParquetAndPeriods
  Dim _Parquet As clsParquetFile
  Dim _Periods As List(Of clsPeriod2020)

  Public Property Parquet As clsParquetFile
    Get
      Return _Parquet
    End Get
    Set(value As clsParquetFile)
      _Parquet = value
    End Set
  End Property

  Public Property Periods As List(Of clsPeriod2020)
    Get
      Return _Periods
    End Get
    Set(value As List(Of clsPeriod2020))
      _Periods = value
    End Set
  End Property

  Public Sub New(Parquet As clsParquetFile, Periods As List(Of clsPeriod2020))
    _Parquet = Parquet
    _Periods = Periods
  End Sub

End Class
