
Imports System.Buffers
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Data.Odbc
Imports System.Data.SQLite
Imports System.IO
Imports log4net.DateFormatter
Imports OpenTK.Graphics.OpenGL.GL
Imports Parquet
Imports Parquet.Data
Imports SPwpf

'Public Class clsParquetMultiFiles
'  Dim pFiles As New List(Of clsParquetFile)
'  Dim pListaCanali As New List(Of clsCanaleParquet)
'  'Dim TR As clsTimeRange
'  Dim pListaCanaliDaCaricare As New List(Of String)
'  Dim pExportHz As Integer = 20
'  Dim pPath As String

'  Public ReadOnly Property PathFileSalvato As String
'    Get
'      Return pPath
'    End Get
'  End Property

'  Public Sub New(PathFiles As List(Of String))
'    For Each Path In PathFiles.OrderByDescending(Function(x) x.IndexOf("ILS1_")).OrderByDescending(Function(x) x.IndexOf("ILS0_"))
'      pFiles.Add(New clsParquetFile(Path))
'      pListaCanali.AddRange(pFiles.Last.Canali)
'    Next
'    CaricaListaCanaliSelezionati()
'    ImpostaCanaliSelezionati()
'  End Sub

'  Public Sub ImpostaCanaliSelezionati()
'    ' imposta come selezionati i canali presenti nel file di configurazione
'    For Each canale In pListaCanali
'      canale.Selezionato = Selected(canale.Intestazione)
'    Next
'    ImportaCanaliSelezionati()
'  End Sub

'  Private Function Selected(Intestazione As String) As Boolean
'    'If Intestazione.ToLower = "PortFlapInTgt_1_AP_deg".ToLower Then Stop
'    Return pListaCanaliDaCaricare.Exists(Function(x) x.ToLower = Intestazione.ToLower)
'  End Function

'  Public Sub ImportaCanaliSelezionati()
'    Dim Lista = pListaCanali.Where(Function(x) x.Selezionato = True)
'    'lista é la lista dei canali presenti in questo file 
'    For Each canale In Lista
'      If canale.FileSource.FileParquet Is Nothing Then
'        canale.FileSource.LeggiFileParquet(True)
'      End If
'    Next
'    'riempie i valori dei canali selezionati
'    For Each canale In Lista
'      canale.Valori = canale.FileSource.FileParquet.CaricaValoriCanale(canale.Intestazione)
'    Next

'    'Stop

'    'imposta il timerange comune a tutti i files
'    Dim Inizio As DateTime = Nothing
'    Dim Fine As DateTime = Nothing
'    For Each file In pFiles
'      If Inizio = Nothing Then
'        Inizio = file.FileParquet.TimeRangeRealDateTime.Start
'        Fine = file.FileParquet.TimeRangeRealDateTime.Finish
'      Else
'        If file.FileParquet.TimeRangeRealDateTime.Start < Inizio Then
'          Inizio = file.FileParquet.TimeRangeRealDateTime.Start
'        End If
'        If file.FileParquet.TimeRangeRealDateTime.Finish > Fine Then
'          Fine = file.FileParquet.TimeRangeRealDateTime.Finish
'        End If
'        'If file.FileParquet.TimeRangeRealDateTime.Start > Inizio Then
'        '  Inizio = file.FileParquet.TimeRangeRealDateTime.Start
'        'End If
'        'If file.FileParquet.TimeRangeRealDateTime.Finish < Fine Then
'        '  Fine = file.FileParquet.TimeRangeRealDateTime.Finish
'        'End If
'      End If
'    Next

'    Dim TRlimiti As New clsTimeRange(Inizio, Fine)

'    SalvaParquet(TRlimiti)
'  End Sub

'  Private Function VerificaIntervallo(TR As clsTimeRange, SecFromMidnight As Double) As Boolean
'    Return SecFromMidnight >= TR.Start.TimeOfDay.TotalSeconds AndAlso SecFromMidnight <= TR.Finish.TimeOfDay.TotalSeconds
'  End Function

'  Private Sub CaricaListaCanaliSelezionati()

'    'Dim strTmp As String = String.Join(vbCrLf, pListaCanali)
'    'Dim strTmp2 As String = String.Join("pListaCanaliDaCaricare.Add(""", pListaCanali)
'    'strTmp = ""
'    'For Each canale In pListaCanali
'    '  'strTmp &= "pListaCanaliDaCaricare.Add(""" & canale.Intestazione & """)" & vbCrLf
'    '  strTmp &= canale.Intestazione & vbCrLf
'    'Next
'    'Clipboard.SetText(strTmp)
'    pListaCanaliDaCaricare.Clear()

'    pListaCanaliDaCaricare.Add("PortInFlap1_Ang")
'    pListaCanaliDaCaricare.Add("PortOutFlap1_Ang")
'    pListaCanaliDaCaricare.Add("StbdOutFlap1_Ang")
'    pListaCanaliDaCaricare.Add("StbdOutFlap2_Ang")
'    pListaCanaliDaCaricare.Add("StbdInFlap1_Ang")
'    pListaCanaliDaCaricare.Add("StbdInFlap2_Ang")
'    pListaCanaliDaCaricare.Add("RudderRake_Ang")
'    pListaCanaliDaCaricare.Add("PortOutFlap1_Tgt")
'    pListaCanaliDaCaricare.Add("PortInFlap1_Tgt")
'    pListaCanaliDaCaricare.Add("StbdOutFlap1_Tgt")
'    pListaCanaliDaCaricare.Add("StbdInFlap1_Tgt")
'    pListaCanaliDaCaricare.Add("RudderRake_Tgt")
'    pListaCanaliDaCaricare.Add("PortOutFlapAPActive")
'    pListaCanaliDaCaricare.Add("PortInFlapAPActive")
'    pListaCanaliDaCaricare.Add("StbdInFlapAPActive")
'    pListaCanaliDaCaricare.Add("StbdOutFlapAPActive")
'    pListaCanaliDaCaricare.Add("SystemTime_DaySeconds")
'    pListaCanaliDaCaricare.Add("SystemTime_Local")
'    pListaCanaliDaCaricare.Add("SystemTime_Date")
'    pListaCanaliDaCaricare.Add("LatBow")
'    pListaCanaliDaCaricare.Add("LonBow")
'    pListaCanaliDaCaricare.Add("Tws")
'    pListaCanaliDaCaricare.Add("Twd")
'    pListaCanaliDaCaricare.Add("Twa")
'    pListaCanaliDaCaricare.Add("Awa")
'    pListaCanaliDaCaricare.Add("Bs")
'    pListaCanaliDaCaricare.Add("Cog")
'    pListaCanaliDaCaricare.Add("Sog")
'    pListaCanaliDaCaricare.Add("BsTgt")
'    pListaCanaliDaCaricare.Add("TwaTgt")
'    pListaCanaliDaCaricare.Add("Heel")
'    pListaCanaliDaCaricare.Add("Aws")
'    pListaCanaliDaCaricare.Add("Hdg")
'    pListaCanaliDaCaricare.Add("Lwy")
'    pListaCanaliDaCaricare.Add("Cse")
'    pListaCanaliDaCaricare.Add("Pitch")
'    pListaCanaliDaCaricare.Add("Roll")
'    pListaCanaliDaCaricare.Add("AccX")
'    pListaCanaliDaCaricare.Add("AccY")
'    pListaCanaliDaCaricare.Add("AccZ")
'    pListaCanaliDaCaricare.Add("PitchRate")
'    pListaCanaliDaCaricare.Add("RollRate")
'    pListaCanaliDaCaricare.Add("YawRate")
'    pListaCanaliDaCaricare.Add("AbsTwa")
'    pListaCanaliDaCaricare.Add("Trim")
'    pListaCanaliDaCaricare.Add("TrimRate")
'    pListaCanaliDaCaricare.Add("HeelRate")
'    pListaCanaliDaCaricare.Add("Vmg")
'    pListaCanaliDaCaricare.Add("Battery_Current")
'    pListaCanaliDaCaricare.Add("Battery_Voltage")
'    pListaCanaliDaCaricare.Add("Battery_Temp")
'    pListaCanaliDaCaricare.Add("SailingPP_Temp")
'    pListaCanaliDaCaricare.Add("FoilingPP_Temp")
'    pListaCanaliDaCaricare.Add("PortTwistLine1_Load")
'    pListaCanaliDaCaricare.Add("PortTwistLine2_Load")
'    pListaCanaliDaCaricare.Add("StbdTwistLine1_Load")
'    pListaCanaliDaCaricare.Add("StbdTwistLine2_Load")
'    pListaCanaliDaCaricare.Add("MainCunnPortLine_Load")
'    pListaCanaliDaCaricare.Add("MainCunnStbdLine_Load")
'    pListaCanaliDaCaricare.Add("PortV1Pin_Load")
'    pListaCanaliDaCaricare.Add("PortD1Pin_Load")
'    pListaCanaliDaCaricare.Add("StbdV1Pin_Load")
'    pListaCanaliDaCaricare.Add("StbdD1Pin_Load")
'    pListaCanaliDaCaricare.Add("ForestayPin_Load")
'    pListaCanaliDaCaricare.Add("PortTravRam_Load")
'    pListaCanaliDaCaricare.Add("StbdTravRam_Load")
'    pListaCanaliDaCaricare.Add("SpannerRam_Load")
'    pListaCanaliDaCaricare.Add("TwistRam_Load")
'    pListaCanaliDaCaricare.Add("TopPostRam_Load")
'    pListaCanaliDaCaricare.Add("PortJibUpDwRam_Load")
'    pListaCanaliDaCaricare.Add("StbdJibUpDwRam_Load")
'    pListaCanaliDaCaricare.Add("MainOuthaulRam_Load")
'    pListaCanaliDaCaricare.Add("MainCunnRam_Load")
'    pListaCanaliDaCaricare.Add("JibInhaulRam_Load")
'    pListaCanaliDaCaricare.Add("JibOuthaulRam_Load")
'    pListaCanaliDaCaricare.Add("JibCunnRam_Load")
'    pListaCanaliDaCaricare.Add("ForestayRam_Load")
'    pListaCanaliDaCaricare.Add("PortV1Strain_Load")
'    pListaCanaliDaCaricare.Add("PortD1Strain_Load")
'    pListaCanaliDaCaricare.Add("StbdV1Strain_Load")
'    pListaCanaliDaCaricare.Add("StbdD1Strain_Load")
'    pListaCanaliDaCaricare.Add("BobstayStrain_Load")
'    pListaCanaliDaCaricare.Add("StbdJibSheetRam_Load")
'    pListaCanaliDaCaricare.Add("PortJibSheetRam_Load")
'    pListaCanaliDaCaricare.Add("StbdRunnerRam_Load")
'    pListaCanaliDaCaricare.Add("PortRunnerRam_Load")
'    pListaCanaliDaCaricare.Add("MainSheetRam_Load")
'    pListaCanaliDaCaricare.Add("MainSheetPlusOuthaul_Load")
'    pListaCanaliDaCaricare.Add("PortOutFlapRam1_Load")
'    pListaCanaliDaCaricare.Add("PortOutFlapRam2_Load")
'    pListaCanaliDaCaricare.Add("PortInFlapRam1_Load")
'    pListaCanaliDaCaricare.Add("PortInFlapRam2_Load")
'    pListaCanaliDaCaricare.Add("StbdOutFlapRam1_Load")
'    pListaCanaliDaCaricare.Add("StbdOutFlapRam2_Load")
'    pListaCanaliDaCaricare.Add("StbdInFlapRam1_Load")
'    pListaCanaliDaCaricare.Add("StbdInFlapRam2_Load")
'    pListaCanaliDaCaricare.Add("RudderRakeRam_Load")
'    pListaCanaliDaCaricare.Add("Traveller_Load")
'    pListaCanaliDaCaricare.Add("PortCantRam_Load")
'    pListaCanaliDaCaricare.Add("StbdCantRam_Load")
'    pListaCanaliDaCaricare.Add("JibCunnRam_Pos")
'    pListaCanaliDaCaricare.Add("FlapsTank_Pos")
'    pListaCanaliDaCaricare.Add("StbdMainTank_Pos")
'    pListaCanaliDaCaricare.Add("PortMainTank_Pos")
'    pListaCanaliDaCaricare.Add("PowerPackTank_Pos")
'    pListaCanaliDaCaricare.Add("PortJibUpDwRam_Pos")
'    pListaCanaliDaCaricare.Add("StbdJibUpDwRam_Pos")
'    pListaCanaliDaCaricare.Add("PortJibSheetTank_Pos")
'    pListaCanaliDaCaricare.Add("StbdJibSheetTank_Pos")
'    pListaCanaliDaCaricare.Add("JibInhaulRam_Pos")
'    pListaCanaliDaCaricare.Add("JibOuthaulRam_Pos")
'    pListaCanaliDaCaricare.Add("PortJibSheetRam_Pos")
'    pListaCanaliDaCaricare.Add("StbdJibSheetRam_Pos")
'    pListaCanaliDaCaricare.Add("MainOuthaulRam_Pos")
'    pListaCanaliDaCaricare.Add("TwistRam_Pos")
'    pListaCanaliDaCaricare.Add("PortTravRam_Pos")
'    pListaCanaliDaCaricare.Add("StbdTravRam_Pos")
'    pListaCanaliDaCaricare.Add("MainSheetRam_Pos")
'    pListaCanaliDaCaricare.Add("MainSheetTank_Pos")
'    pListaCanaliDaCaricare.Add("ForestayRam_Pos")
'    pListaCanaliDaCaricare.Add("SpannerRam_Pos")
'    pListaCanaliDaCaricare.Add("TopPostRam_Pos")
'    pListaCanaliDaCaricare.Add("StbdRunnerRam_Pos")
'    pListaCanaliDaCaricare.Add("StbdRunnerTank_Pos")
'    pListaCanaliDaCaricare.Add("PortRunnerRam_Pos")
'    pListaCanaliDaCaricare.Add("PortRunnerTank_Pos")
'    pListaCanaliDaCaricare.Add("Rudder_Ang")
'    pListaCanaliDaCaricare.Add("TopCtrlArm_Ang")
'    pListaCanaliDaCaricare.Add("Traveller_Ang")
'    pListaCanaliDaCaricare.Add("PortTravRam_Ang")
'    pListaCanaliDaCaricare.Add("StbdTravRam_Ang")
'    pListaCanaliDaCaricare.Add("Spanner_Ang")
'    pListaCanaliDaCaricare.Add("Twist_Ang")
'    pListaCanaliDaCaricare.Add("TopMastWindUnit_Ang")
'    pListaCanaliDaCaricare.Add("BowWindUnit_Ang")
'    pListaCanaliDaCaricare.Add("TopMastWindUnit_Speed")
'    pListaCanaliDaCaricare.Add("BowWindUnit_Speed")
'    pListaCanaliDaCaricare.Add("FCS_PortCant_Ang")
'    pListaCanaliDaCaricare.Add("FCS_StbdCant_Ang")
'    pListaCanaliDaCaricare.Add("FCS_PortRam_Pos")
'    pListaCanaliDaCaricare.Add("FCS_StbdRam_Pos")
'    pListaCanaliDaCaricare.Add("FCS_PortRam_Spd")
'    pListaCanaliDaCaricare.Add("FCS_StbdRam_Spd")
'    pListaCanaliDaCaricare.Add("FCS_StorageLevel_Pos")
'    pListaCanaliDaCaricare.Add("FCS_Oil_Temp")
'    pListaCanaliDaCaricare.Add("FCS_Battery_Volt")
'    pListaCanaliDaCaricare.Add("FCS_Motor_Temp")
'    pListaCanaliDaCaricare.Add("FCS_MotorCtrl_Temp")
'    pListaCanaliDaCaricare.Add("FCS_MotorCmd_RPM")
'    pListaCanaliDaCaricare.Add("FCS_MotorAct_RPM")
'    pListaCanaliDaCaricare.Add("FCS_Battery_Amp")
'    pListaCanaliDaCaricare.Add("PortBulbRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("StbdBulbRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("PortTipInRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("StbdTipInRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("PortTipOutRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("StbdTipOutRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("RudderRideHeight_AP_m")
'    pListaCanaliDaCaricare.Add("SinkMin_AP")
'    pListaCanaliDaCaricare.Add("PortBulbSpeed_AP_kts")
'    pListaCanaliDaCaricare.Add("StbdBulbSpeed_AP_kts")
'    pListaCanaliDaCaricare.Add("RudderSpeed_AP_kts")
'    pListaCanaliDaCaricare.Add("WaveForm_AP")
'    pListaCanaliDaCaricare.Add("WaveCrest_AP")
'    pListaCanaliDaCaricare.Add("WaveTrough_AP")
'    pListaCanaliDaCaricare.Add("PortFlapInTgt_1_AP_deg")
'    pListaCanaliDaCaricare.Add("PortFlapInTgt_2_AP_deg")
'    pListaCanaliDaCaricare.Add("PortFlapOutTgt_1_AP_deg")
'    pListaCanaliDaCaricare.Add("PortFlapOutTgt_2_AP_deg")
'    pListaCanaliDaCaricare.Add("StbdFlapInTgt_1_AP_deg")
'    pListaCanaliDaCaricare.Add("StbdFlapInTgt_2_AP_deg")
'    pListaCanaliDaCaricare.Add("StbdFlapOutTgt_1_AP_deg")
'    pListaCanaliDaCaricare.Add("StbdFlapOutTgt_2_AP_deg")
'    pListaCanaliDaCaricare.Add("RudderRakeTgt_AP_deg")
'    pListaCanaliDaCaricare.Add("ZDatum_AP")
'    pListaCanaliDaCaricare.Add("SWH_AP")
'    pListaCanaliDaCaricare.Add("TowLine_Load")
'    pListaCanaliDaCaricare.Add("RudderYaw_Ang")
'    pListaCanaliDaCaricare.Add("JibInOut_Ang")
'    pListaCanaliDaCaricare.Add("JibInhaulRam_Ang")
'    pListaCanaliDaCaricare.Add("JibOuthaulRam_Ang")
'    pListaCanaliDaCaricare.Add("TwaBow")
'    pListaCanaliDaCaricare.Add("TwsBow")
'    pListaCanaliDaCaricare.Add("TwdBow")
'    pListaCanaliDaCaricare.Add("ChazTwd")
'    pListaCanaliDaCaricare.Add("ChazTws")
'    pListaCanaliDaCaricare.Add("JibCar_Ang")
'    pListaCanaliDaCaricare.Add("PortJibCar_Ang")
'    pListaCanaliDaCaricare.Add("StbdJibCar_Ang")
'    pListaCanaliDaCaricare.Add("PortArmIB1_Strain")
'    pListaCanaliDaCaricare.Add("PortArmIB2_Strain")
'    pListaCanaliDaCaricare.Add("PortArmIB3_Strain")
'    pListaCanaliDaCaricare.Add("PortArmOB1_Strain")
'    pListaCanaliDaCaricare.Add("PortArmOB2_Strain")
'    pListaCanaliDaCaricare.Add("PortArmOB3_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmIB1_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmIB2_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmIB3_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmOB1_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmOB2_Strain")
'    pListaCanaliDaCaricare.Add("StbdArmOB3_Strain")
'    pListaCanaliDaCaricare.Add("PortWingIB1_Strain")
'    pListaCanaliDaCaricare.Add("PortWingIB2_Strain")
'    pListaCanaliDaCaricare.Add("PortWingIB3_Strain")
'    pListaCanaliDaCaricare.Add("PortWingOB1_Strain")
'    pListaCanaliDaCaricare.Add("PortWingOB2_Strain")
'    pListaCanaliDaCaricare.Add("PortWingOB3_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingIB1_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingIB2_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingIB3_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingOB1_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingOB2_Strain")
'    pListaCanaliDaCaricare.Add("StbdWingOB3_Strain")
'    pListaCanaliDaCaricare.Add("RudderPort1_Strain")
'    pListaCanaliDaCaricare.Add("ElevatorPort1_Strain")
'    pListaCanaliDaCaricare.Add("ElevatorPort2_Strain")
'    pListaCanaliDaCaricare.Add("RudderStbd1_Strain")
'    pListaCanaliDaCaricare.Add("ElevatorStbd1_Strain")
'    pListaCanaliDaCaricare.Add("ElevatorStbd2_Strain")
'    pListaCanaliDaCaricare.Add("MDPed1EffectiveFunction")
'    pListaCanaliDaCaricare.Add("PumpLine1_Press")
'    pListaCanaliDaCaricare.Add("Pump1_Gear")
'    pListaCanaliDaCaricare.Add("Pump1_RPM")
'    pListaCanaliDaCaricare.Add("MDPed2EffectiveFunction")
'    pListaCanaliDaCaricare.Add("PumpLine2_Press")
'    pListaCanaliDaCaricare.Add("Pump2_Gear")
'    pListaCanaliDaCaricare.Add("Pump2_RPM")
'    pListaCanaliDaCaricare.Add("MDPed3EffectiveFunction")
'    pListaCanaliDaCaricare.Add("PumpLine3_Press")
'    pListaCanaliDaCaricare.Add("Pump3_Gear")
'    pListaCanaliDaCaricare.Add("Pump3_RPM")
'    pListaCanaliDaCaricare.Add("MDPed4EffectiveFunction")
'    pListaCanaliDaCaricare.Add("PumpLine4_Press")
'    pListaCanaliDaCaricare.Add("Pump4_Gear")
'    pListaCanaliDaCaricare.Add("Pump4_RPM")
'    pListaCanaliDaCaricare.Add("MainLine1_Press")
'    pListaCanaliDaCaricare.Add("MainLine2_Press")
'    pListaCanaliDaCaricare.Add("MainLine3_Press")
'    pListaCanaliDaCaricare.Add("MainLine4_Press")
'    pListaCanaliDaCaricare.Add("Ped1_Funct")
'    pListaCanaliDaCaricare.Add("Ped2_Funct")
'    pListaCanaliDaCaricare.Add("Ped3_Funct")
'    pListaCanaliDaCaricare.Add("Ped4_Funct")
'    pListaCanaliDaCaricare.Add("TopMastIMU_Hdg")
'    pListaCanaliDaCaricare.Add("TopMastIMU_Pitch")
'    pListaCanaliDaCaricare.Add("TopMastIMU_Roll")
'    pListaCanaliDaCaricare.Add("V_BowWindUnitMeasuredRaw_MWS")
'    pListaCanaliDaCaricare.Add("V_TopMastWindUnitMeasuredRaw_MWS")
'    pListaCanaliDaCaricare.Add("V_BowWindUnitMeasuredRaw_MWA")
'    pListaCanaliDaCaricare.Add("V_TopMastWindUnitMeasuredRaw_MWA")
'    pListaCanaliDaCaricare.Add("SC_MainSail")
'    pListaCanaliDaCaricare.Add("SC_HeadSail")
'    pListaCanaliDaCaricare.Add("SC_JibClewBoard")

'    'Clipboard.SetText(String.Join(vbCrLf, pListaCanaliDaCaricare))


'    CaricaListaDaFileConfig()
'  End Sub

'  Private Sub CaricaListaDaFileConfig()
'    '"C:\Discodati\progettidotnet\Performance2021\Performance2021\Config\IlsEccMergingChannels.txt"
'    Dim MergingFilePath As String = AppConfig.FI.FullName.Replace(AppConfig.FI.Name, "IlsEccMergingChannels.txt")
'    If System.IO.File.Exists(MergingFilePath) Then
'      Dim Righe As List(Of String) = System.IO.File.ReadAllLines(MergingFilePath).ToList
'      If Righe.Count > 0 Then
'        pListaCanaliDaCaricare.Clear()
'        For Each Riga In Righe
'          pListaCanaliDaCaricare.Add(Riga)
'        Next
'      End If
'    End If

'  End Sub


'  Private Sub SalvaParquet(TR As clsTimeRange)

'    Dim pfi As New System.IO.FileInfo(pFiles.First.FilePath)
'    Dim Nome As String = pfi.Name.Replace("ECC0_XChannel", "Performance").Replace("ILS0_XChannel", "Performance").Replace("ECC1_XChannel", "Performance").Replace("ILS1_XChannel", "Performance")
'    Nome = Nome.Replace("ECC0Log_", "Performance").Replace("ECC1Log_", "Performance").Replace("ILS0Log_", "Performance").Replace("ILS1Log_", "Performance")
'    Nome = Nome.Replace("ECC0_DiffXChannel", "Performance").Replace("ECC1_DiffXChannel", "Performance").Replace("ILS0_DiffXChannel", "Performance").Replace("ILS1_DiffXChannel", "Performance")
'    pPath = pfi.FullName.Replace(pfi.Name, "") & Nome

'    Dim Lista = pListaCanali.Where(Function(x) x.Selezionato = True)
'    Dim ListaSelezionati As New List(Of clsCanaleParquet)
'    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
'    For Each canale In Lista
'      If Not ListaSelezionati.Exists(Function(x) x.Intestazione = canale.Intestazione) Then
'        ListaSelezionati.Add(canale)
'        Dim DataField As New Parquet.Data.DataField(canale.Intestazione, Parquet.Data.DataType.Double, False)
'        Dim Valori(TR.Durata.TotalSeconds * pExportHz) As Double
'        For i As Integer = 0 To Valori.Count - 1
'          Valori(i) = Double.NaN
'        Next
'        'If TR.Start < canale.FileSource.FileParquet.TimeRangeRealDateTime.Start Then
'        '  Stop

'        'End If

'        For i As Integer = 0 To canale.FileSource.FileParquet.TimeStamps.Count - 1
'          Dim Momento As DateTime = canale.FileSource.FileParquet.TimeStamps(i)
'          If Momento.ToOADate > 0 Then
'            Dim Id As Integer = Momento.Subtract(TR.Start).TotalMilliseconds * pExportHz / 1000
'            If Id >= 0 AndAlso Id < Valori.Count Then
'              Valori(Id) = canale.Valori(i)
'            End If
'          End If
'        Next
'        CanaliPq.Add(New Parquet.Data.DataColumn(DataField, Valori))
'        'CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori))
'      End If
'    Next


'    Dim fsTmp As New System.IO.StreamWriter(pPath)
'    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
'    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)

'    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup

'    For Each Canale In CanaliPq
'      GroupWriter.WriteColumn(Canale)
'    Next
'    ParquetWriter.Dispose()
'    fsTmp.Close()
'    fsTmp.Dispose()
'    ApriExplorer(pfi.FullName.Replace(pfi.Name, ""), pPath)


'  End Sub

'End Class

Public Class clsCanaleParquet
  Dim pIntestazione As String
  Dim pFileSource As clsParquetFile
  Dim pSelezionato As Boolean
  Dim pValori As Double()

  Public Sub New(Intestazione As String, FileSource As clsParquetFile)
    pIntestazione = Intestazione
    pFileSource = FileSource

  End Sub

  Public Property Intestazione As String
    Get
      Return pIntestazione
    End Get
    Set(value As String)
      pIntestazione = value
    End Set
  End Property

  Public Property FileSource As clsParquetFile
    Get
      Return pFileSource
    End Get
    Set(value As clsParquetFile)
      pFileSource = value
    End Set
  End Property

  Public Property Selezionato As Boolean
    Get
      Return pSelezionato
    End Get
    Set(value As Boolean)
      pSelezionato = value
    End Set
  End Property

  Public Property Valori As Double()
    Get
      Return pValori
    End Get
    Set(value As Double())
      pValori = value
    End Set
  End Property
End Class

Public Class clsParquetFile
  Dim _FilePath As String
  Dim _Canali As New List(Of clsCanaleParquet)
  'Dim _CanaliAdvanced As New List(Of clsCanaleParquetAdvanced)
  Dim _FileParquet As clsFileParquet2020
  Dim _ParquetReader As Parquet.ParquetReader
  Dim _fsTmp As System.IO.StreamReader

  Public Sub New(FilePath As String)
    ImpostazioniIniziali(FilePath, True)
  End Sub

  Public Sub New(FilePath As String, DisposeReader As Boolean)
    ImpostazioniIniziali(FilePath, DisposeReader)
  End Sub

  Public Sub ImpostazioniIniziali(FilePath As String, DisposeReader As Boolean)
    _FilePath = FilePath
    _fsTmp = New System.IO.StreamReader(_FilePath)
    _ParquetReader = New Parquet.ParquetReader(_fsTmp.BaseStream)
    'Dim PqDataFileds As List(Of Parquet.Data.DataField) = _ParquetReader.Schema.GetDataFields.ToList
    Dim IntestazioniTmp = _ParquetReader.Schema.GetDataFields.Select(Function(x) x.Name).ToList
    For Each intestazione In IntestazioniTmp
      _Canali.Add(New clsCanaleParquet(intestazione.Replace(vbNullChar, ""), Me))
      '_CanaliAdvanced.Add(New clsCanaleParquetAdvanced(intestazione.Replace(vbNullChar, ""), Me))
    Next
    If DisposeReader Then
      _ParquetReader.Dispose()
      _fsTmp.Close()
      _fsTmp.Dispose()
    End If
  End Sub

  Public Sub DisposeReader()
    _ParquetReader.Dispose()
    _fsTmp.Close()
    _fsTmp.Dispose()
  End Sub

  Public Property FilePath As String
    Get
      Return _FilePath
    End Get
    Set(value As String)
      _FilePath = value
    End Set
  End Property

  Public Property Canali As List(Of clsCanaleParquet)
    Get
      Return _Canali
    End Get
    Set(value As List(Of clsCanaleParquet))
      _Canali = value
    End Set
  End Property

  'Public Property CanaliAdvanced As List(Of clsCanaleParquetAdvanced)
  '  Get
  '    Return _CanaliAdvanced
  '  End Get
  '  Set(value As List(Of clsCanaleParquetAdvanced))
  '    _CanaliAdvanced = value
  '  End Set
  'End Property

  Public Property FileParquet As clsFileParquet2020
    Get
      Return _FileParquet
    End Get
    Set(value As clsFileParquet2020)
      _FileParquet = value
    End Set
  End Property

  Public Property ParquetReader As ParquetReader
    Get
      Return _ParquetReader
    End Get
    Set(value As ParquetReader)
      _ParquetReader = value
    End Set
  End Property

  Public Sub LeggiFileParquet(DisposeReader As Boolean)
    _FileParquet = New clsFileParquet2020(New System.IO.FileInfo(_FilePath), DisposeReader)
    If DisposeReader Then _FileParquet.DisposeReader()
  End Sub

End Class

Public Class clsExportToCsv


  Public Sub ExportNotEmpty(OutputHz As Integer, EsportaTuttiCanali As Boolean)

    If Not DataProvider2020.ValoriCaricati Then Exit Sub



    Dim FolderReport As String = SelectFolder(AppConfig.ActiveProfile.ReportFolder)
    If System.IO.Directory.Exists(FolderReport) Then
      AppConfig.ActiveProfile.ReportFolder = FolderReport
      AppConfig.Salva()
      Dim pathFileCsv As String = FolderReport & SepDaPath(FolderReport) & TempoInStringaFormattata(Now, eFormatType.YYYYMMDDHHMMSS) & "_" & OutputHz & "Hz.csv"
      Dim ListaCanali As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.ToList
      If Not EsportaTuttiCanali Then
        ListaCanali = GestisciLista("Select Channels To Be Exported").ToList
      End If
      Dim DaRimuovere As New List(Of clsChannel2020)
      For Each elemento In ListaCanali
        If elemento.Valori Is Nothing OrElse elemento.Valori.Count = 0 Then
          DaRimuovere.Add(elemento)
        End If
      Next
      For Each elemento In DaRimuovere
        ListaCanali.Remove(elemento)
      Next
      LoadingProgressVisualizza()
      If ListaCanali Is Nothing Then Exit Sub

      Dim fsTmp As New System.IO.StreamWriter(pathFileCsv)
      Dim cSep As Char = vbTab
      Dim Righe As New List(Of String)

      Dim Riga As String = "Date"
      Riga &= vbTab & "Time"
      For Each Canale In ListaCanali
        Riga &= vbTab & Canale.ChannelId
      Next
      Righe.Add(Riga)

      Dim LastExported As DateTime = DataProvider2020.TimeRange.Start
      Riga = LastExported.ToString("yyyy/MM/dd")
      Riga &= vbTab & LastExported.ToString("HH:mm:ss.fff")
      For Each Canale In ListaCanali
        If Not Canale.ValoriDT Is Nothing Then
          Riga &= vbTab & Canale.ValoriDT(DataProvider2020.TrovaIndice(LastExported))
        Else
          Riga &= vbTab & Canale.Valori(DataProvider2020.TrovaIndice(LastExported))
        End If
      Next
      'Righe.Add(Riga)

      For Each Momento In DataProvider2020.TimeStamps
        If Momento.Subtract(LastExported).TotalMilliseconds >= 1 / OutputHz AndAlso VerificaFiltroTimeRange(Momento) Then
          Riga = Momento.ToString("yyyy/MM/dd")
          'Riga &= vbTab & Momento.ToString("HH:mm:ss.fff")
          Riga &= "," & Momento.ToString("HH:mm:ss.fff")
          For Each Canale In ListaCanali
            Dim i As Integer = DataProvider2020.TrovaIndice(Momento)
            If i > -1 Then
              'Riga &= vbTab & Canale.Valori(i)
              Riga &= "," & Canale.Valori(i)
            Else
              Stop
            End If
          Next
          Righe.Add(Riga)
          If Righe.Count > 1000 Then
            fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
            Righe.Clear()
          End If
          LastExported = Momento
        End If
      Next

      If Righe.Count > 0 Then
        fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
      End If
      Dim pfi As New System.IO.FileInfo(pathFileCsv)
      fsTmp.Close()
      fsTmp.Dispose()
      ApriExplorer(pfi.FullName)
      LoadingProgressNascondi()
    End If


  End Sub

  Public Sub ExportSelected()

    If Not DataProvider2020.ValoriCaricati Then Exit Sub

    'Dim FolderReport As String = SelectFolder(DataProvider2020.Files.First.Directory.FullName)
    Dim FolderReport As String = DataProvider2020.Files.First.Directory.FullName
    If System.IO.Directory.Exists(FolderReport) Then
      AppConfig.ActiveProfile.ReportFolder = FolderReport
      AppConfig.Salva()
      Dim pathFileCsv As String = FolderReport & SepDaPath(FolderReport) & TempoInStringaFormattata(Now, eFormatType.YYYYMMDDHHMMSS) & ".csv"
      'Dim ListaCanali As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.IsMath = False).ToList
      Dim ListaCanali As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.Export = True).ToList
      'Dim ListaCanali As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.ToList
      If ListaCanali Is Nothing Then
        Exit Sub
      End If
      If ListaCanali.Count = 0 Then
        MsgBox("No channels marked as exportable")
        Exit Sub
      End If

      LoadingProgressVisualizza()
      Dim fsTmp As New System.IO.StreamWriter(pathFileCsv)
      Dim cSep As Char = vbTab
      cSep = ","
      Dim Righe As New List(Of String)

      Dim Riga As String = "Date"
      'Riga &= cSep & "Time"
      'For Each Canale In ListaCanali
      '  Riga &= cSep & Canale.ChannelId
      '  Dim c = DataProvider2020.CanaleDbl(Canale)
      'Next
      'Righe.Add(Riga)

      'Riga = "Date"
      Riga &= cSep & "Time"
      For Each Canale In ListaCanali
        Riga &= cSep & Canale.Name
      Next
      Righe.Add(Riga)

      'Riga = "Is"
      'Riga &= cSep & "Math"
      'For Each Canale In ListaCanali
      '  Riga &= cSep & IIf(Canale.IsMath, 1, 0)
      'Next
      'Righe.Add(Riga)

      Dim LastExported As DateTime = DataProvider2020.TimeRange.Start
      Riga = LastExported.ToString("yyyy/MM/dd")
      Riga &= cSep & LastExported.ToString("HH:mm:ss.fff")
      For Each Canale In ListaCanali
        If Not Canale.ValoriDT Is Nothing Then
          Riga &= cSep & Canale.ValoriDT(DataProvider2020.TrovaIndice(LastExported))
        Else
          If Canale.Valori Is Nothing Then
            Dim c = DataProvider2020.CanaleDbl(Canale)
          End If
          Riga &= cSep & Canale.Valori(DataProvider2020.TrovaIndice(LastExported))
        End If
      Next
      'Righe.Add(Riga)

      Dim i As Integer = 0 'DataProvider2020.TrovaIndice(Momento)
      For Each Momento In DataProvider2020.TimeStamps
        Riga = Momento.ToString("yyyy/MM/dd")
        'Riga &= vbTab & Momento.ToString("HH:mm:ss.fff")
        Riga &= cSep & Momento.ToString("HH:mm:ss.fff")
        For Each Canale In ListaCanali
          Riga &= cSep & Canale.Valori(i)
        Next
        Righe.Add(Riga)
        If Righe.Count > 1000 Then
          fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
          Righe.Clear()
        End If
        LastExported = Momento
        i += 1
      Next

      If Righe.Count > 0 Then
        fsTmp.Write(String.Join(vbCrLf, Righe) & vbCrLf)
      End If
      Dim pfi As New System.IO.FileInfo(pathFileCsv)
      fsTmp.Close()
      fsTmp.Dispose()
      ApriExplorer(pfi.FullName)
      LoadingProgressNascondi()
    End If


  End Sub

  Private Function VerificaFiltroTimeRange(DT As DateTime) As Boolean
    Dim FiltaPerTR As Boolean = True
    If Not FiltaPerTR Then Return True
    Dim MinDT As New DateTime(2022, 11, 9, 15, 0, 0)
    Dim MaxDT As New DateTime(2022, 11, 9, 15, 26, 0)
    Return DT >= MinDT AndAlso DT <= MaxDT
  End Function


  Public Function GestisciLista(Titolo As String) As ObservableCollection(Of clsChannel2020)
    Dim strIntestazioni As List(Of String) = AppConfig.ActiveProfile.CanaliDaEsportareNelCsv
    If strIntestazioni Is Nothing OrElse strIntestazioni.Count = 0 Then
      strIntestazioni = IntestazioniDefault()
    End If
    Dim frmSelChannel As New ChannelSelectorAndOrderer(DataProvider2020.Channels.ListaCanali, AppConfig.ActiveProfile.CanaliDaEsportareNelCsv, Titolo)
    frmSelChannel.ShowDialog()
    Dim CanaliDaStampare As ObservableCollection(Of clsChannel2020) = Nothing
    If frmSelChannel.Status = ChannelSelectorAndOrderer.eStatus.eSave Then
      CanaliDaStampare = frmSelChannel.AvailableChannels.GetListaCanaliSelezionati
      If CanaliDaStampare Is Nothing Then Return Nothing
      If CanaliDaStampare.Count = 0 Then Return Nothing
      AppConfig.ActiveProfile.CanaliDaEsportareNelCsv = CanaliDaStampare.Select(Function(x) x.ChannelId).ToList
      AppConfig.Salva()
      'SalvaListaIntestazioniDaXml(CanaliDaStampare.ToList)
      Return CanaliDaStampare
    Else
      Return Nothing
    End If
  End Function

  'Private Function CaricaListaIntestazioniDaXml() As List(Of String)

  '  Dim NodoIntestazioni As Xml.XmlNode = AppConfig.CercaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, "ExportToCsv", True)
  '  If NodoIntestazioni Is Nothing Then
  '    Return IntestazioniDefault()
  '  Else
  '    NodoIntestazioni = AppConfig.CercaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, True)
  '    Dim lTmp As New List(Of String)
  '    For Each Nodo As Xml.XmlNode In NodoIntestazioni
  '      Select Case Nodo.Name
  '        Case "ExportToCsv"
  '          For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
  '            lTmp.Add(SottoNodo.Name)
  '          Next
  '          Return lTmp
  '        Case Else
  '      End Select
  '    Next
  '  End If
  '  Return Nothing
  'End Function


  Private Function IntestazioniDefault() As List(Of String)
    Dim lTmp As New List(Of String)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
    AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTrimNorm)
    Return lTmp
  End Function

  Private Sub AggiungiIntestazioneSeEsiste(ByRef lista As List(Of String), CanaleChiave As clsChannels2020.eCanaliChiave)
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then
      lista.Add("_" & System.Enum.GetName(GetType(clsChannels2020.eCanaliChiave), CanaleChiave).TrimStart("e"))
    Else
      If DataProvider2020.Channels.ListaCanali.Where(Function(x) x.CanaleChiave = CanaleChiave).Count = 0 Then
        lista.Add(DataProvider2020.CanaleDbl(CanaleChiave).ChannelId)
      End If
    End If
  End Sub

  'Private Sub SalvaListaIntestazioniDaXml(Canali As List(Of clsChannel2020))
  '  AppConfig.EliminaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, "ExportToCsv", True)
  '  For Each Canale In Canali
  '    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, "ExportToCsv", Canale.ChannelId, "", True, False)
  '  Next
  '  AppConfig.SalvaFileXML()
  'End Sub

  'Private Sub SalvaListaIntestazioniDaXml(Canali As List(Of String))
  '  AppConfig.EliminaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, "ExportToCsv", True)
  '  For Each Canale In Canali
  '    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eReports, "ExportToCsv", Canale, "", True, False)
  '  Next
  '  AppConfig.SalvaFileXML()
  'End Sub



End Class

Public Class clsParquetCopy
  Dim pCanali As New clsVariabili

  Public Sub New(ParquetFileFullPath As String, Prefisso As String)

    If Not DataProvider2020 Is Nothing Then
      If DataProvider2020.ValoriCaricati Then
        'Dim testo As String = "Do you want to make a file with visible time range only?" & vbCrLf
        'testo &= "YES to proceed" & vbCrLf
        'testo &= "NO if you want, for example, if you need to better check selected periods first..." & vbCrLf
        'If MsgBox(testo, MsgBoxStyle.YesNo, "Parquet File Copy") = MsgBoxResult.Yes Then
        '    LoadingProgressVisualizza()
        RiempiCanaliDaCopiare(ParquetFileFullPath)
        SalvaParquet(ParquetFileFullPath, Prefisso) ' DataProvider2020.Files.First.FullName)
        '    LoadingProgressNascondi()
        'End If
      End If
    End If

  End Sub

  Private Sub RiempiCanaliDaCopiare(ParquetFileFullPath As String)
    Dim bSystemTime_DaySeconds As Boolean = False
    pCanali.Lista.Clear()
    For Each canale In DataProvider2020.Channels.ListaCanali.Where(Function(x) x.IsMath = False).ToList
      'If canale.Name = "SeaState" Then Stop
      Dim c = DataProvider2020.CanaleDbl(canale.ChannelId)
      If Not canale.Valori Is Nothing Then
        If canale.ActualLogHeader = "" Then
          canale.ActualLogHeader = canale.ChannelId
        End If
        pCanali.Lista.Add(New clsVariabile(canale.ActualLogHeader))
        pCanali.Lista.Last.Canale = canale
        If canale.ActualLogHeader = "SystemTime_DaySeconds" Then bSystemTime_DaySeconds = True
      End If
    Next
    If Not bSystemTime_DaySeconds Then
      pCanali.Lista.Add(New clsVariabile("SystemTime_DaySeconds"))
      Stop
    End If
    Dim pp = DataProvider2020.ParquetFiles.Where(Function(x) x.FileInfo.FullName = ParquetFileFullPath).FirstOrDefault
    Dim ti = pp.TimeStamps(1)
    Dim tf = pp.TimeStamps.Last
    Dim IdRigaIniziale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(ti)
    Dim IdRigaFinale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(tf)
    For Each Variabile In pCanali.Lista
      If Variabile.Canale.Valori Is Nothing Then
        If Not bSystemTime_DaySeconds AndAlso Variabile.Nome = "SystemTime_DaySeconds" Then
          For i As Integer = IdRigaIniziale To IdRigaFinale
            Dim Valore As Double = Variabile.Canale.ValoriDT(i).TimeOfDay.TotalSeconds
            Variabile.Valori.Add(Valore)
          Next
        ElseIf Variabile.Nome = "SystemTime_Date" Then
          For i As Integer = IdRigaIniziale To IdRigaFinale
            Dim Valore As Double = Variabile.Canale.ValoriDT(i).ToString("yyyyMMdd")
            Variabile.Valori.Add(Valore)
          Next
        Else
          If Not Variabile.Canale.ValoriDT Is Nothing Then
            For i As Integer = IdRigaIniziale To IdRigaFinale
              Dim Valore As Double = Variabile.Canale.ValoriDT(i).ToOADate
              Variabile.Valori.Add(Valore)
            Next
          End If
        End If
      Else
        For i As Integer = IdRigaIniziale To IdRigaFinale
          Dim Valore As Double = Variabile.Canale.Valori(i)
          Variabile.Valori.Add(Valore)
        Next
      End If
    Next

  End Sub


  Private Sub SalvaParquet(PathFileOrigine As String, Prefisso As String)
    Dim pfi As New System.IO.FileInfo(PathFileOrigine)
    Dim Estensione As String = pfi.Extension
    Dim ParquetName As String = Prefisso & "_" & pfi.Name
    Dim pathParquet As String = pfi.FullName.Replace(pfi.Name, ParquetName)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In pCanali.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori.ToArray))
    Next
    Dim fsTmp As New System.IO.StreamWriter(pathParquet)
    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)
    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup
    For Each Canale In CanaliPq
      GroupWriter.WriteColumn(Canale)
    Next
    ParquetWriter.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()
    'ApriExplorer(pfi.FullName.Replace(pfi.Name, ""), pathParquet)
  End Sub

End Class

Public Class clsParquetUtilities

  Public Shared Sub MakeOneSingleParquetFromFilesAndPeriods(SelectedOnly As Boolean, Optional IncludeMathChannels As Boolean = False)

    LoadingProgressVisualizza()

    Dim Variabili As New clsVariabili
    Variabili.Lista.Clear()
    ' carica i valori di tutti i canali non math
    Dim LC As List(Of clsChannel2020)
    If IncludeMathChannels Then
      LC = DataProvider2020.Channels.ListaCanali.ToList
    Else
      LC = DataProvider2020.Channels.ListaCanali.Where(Function(x) Not x.IsMath).ToList
    End If
    If SelectedOnly Then
      LC = LC.Where(Function(x) x.Export).ToList
    End If
    For Each canale In LC

      Variabili.Lista.Add(New clsVariabile(canale))
      'If canale.IsMath Then Stop
      DataProvider2020.VerificaSeValoriCaricati(canale, IncludeMathChannels)
      If canale.IsMath And (canale.Valori.Count = 0 OrElse Double.IsNaN(canale.Valori(1000))) Then
        'Stop
      End If

    Next
    Dim adesso As DateTime = Now
    'riempie una lista con gli indici da importare nell intorno dei periodi
    Dim ListaIndiciDaImportare As List(Of Long) = CalcolaIndiciDaImportare()
    'Dim ListaIndiciDaImportare As New List(Of Long)
    'For i As Long = 0 To DataProvider2020.TimeStamps.Count - 1
    '  For Each p In PeriodsManager.Periods.ListaOrdinata
    '    Dim trtmp As New clsTimeRange(p.TR.Start.AddMinutes(-5), p.TR.Finish.AddMinutes(5))
    '    If DataProvider2020.TimeStamps(i) <= trtmp.Finish AndAlso DataProvider2020.TimeStamps(i) >= trtmp.Start Then
    '      ListaIndiciDaImportare.Add(i)
    '      Exit For
    '    End If
    '  Next
    'Next


    'Dim fineciclo As Double = Now.Subtract(adesso).TotalSeconds

    For Each indice In LIstaIndiciDaImportare
      For Each variabile In Variabili.Lista
        variabile.Valori.Add(variabile.Canale.Valori(indice))
      Next
    Next
    'qui carica il canale DT

    Dim filecreated = SalvaParquetCompact(DataProvider2020.ParquetFiles.First.FileInfo.FullName, Variabili, SelectedOnly, IncludeMathChannels)
    'Dim filecreated = SalvaParquetCompactV2(DataProvider2020.ParquetFiles.First.FileInfo.FullName, Variabili, SelectedOnly, IncludeMathChannels)

    Dim fineciclo As Double = Now.Subtract(adesso).TotalSeconds

    Dim fc As New System.IO.FileInfo(filecreated)

    Dim PeriodsFilePath As String = fc.FullName.Replace(fc.Extension, ".json")

    clsKillerSeriale.SaveConfigurationGeneric(Of clsPeriods2021)(PeriodsManager.Periods, PeriodsFilePath)

    LoadingProgressNascondi()

    ApriExplorer(filecreated)


  End Sub

  'Option Strict On
  'Option Infer On


  ' Tiny value type for merged ranges
  Private Structure TimeRange
    Public Start As DateTime
    Public [End] As DateTime
  End Structure

  ' Collect indices i of DataProvider2020.TimeStamps() that fall inside any
  ' period expanded by ±5 minutes. O(N + M) after merging.
  Public Shared Function CalcolaIndiciDaImportare() As List(Of Long)
    Dim stamps As DateTime() = DataProvider2020.TimeStamps
    If stamps Is Nothing OrElse stamps.Length = 0 Then
      Return New List(Of Long)(0)
    End If

    ' ---- Snapshot periods to avoid ObservableCollection changes during processing
    Dim src As ObservableCollection(Of clsPeriod2021) = PeriodsManager.Periods.ListaOrdinata
    Dim n As Integer = src.Count
    If n = 0 Then Return New List(Of Long)(0)

    Dim periods(n - 1) As clsPeriod2021
    src.CopyTo(periods, 0) ' avoids LINQ; stable snapshot

    ' ---- Build expanded ranges (assumes periods are already ordered by TR.Start)
    Dim ranges As New List(Of TimeRange)(n)
    For i As Integer = 0 To n - 1
      Dim p = periods(i)
      ranges.Add(New TimeRange With {
            .Start = p.TR.Start.AddMinutes(-5),
            .[End] = p.TR.Finish.AddMinutes(5)
        })
    Next

    ' ---- Merge overlapping/adjacent ranges
    Dim merged As New List(Of TimeRange)(ranges.Count)
    Dim cur As TimeRange = ranges(0)
    For i As Integer = 1 To ranges.Count - 1
      Dim r = ranges(i)
      If r.Start <= cur.[End] Then
        If r.[End] > cur.[End] Then cur.[End] = r.[End]
      Else
        merged.Add(cur)
        cur = r
      End If
    Next
    merged.Add(cur)

    ' ---- One linear sweep over stamps and merged ranges
    Dim result As New List(Of Long)(Math.Min(stamps.Length, 1024))

    ' Jump to first relevant timestamp using binary search
    Dim iStamp As Integer
    Dim pos As Integer = Array.BinarySearch(stamps, merged(0).Start)
    iStamp = If(pos >= 0, pos, Not pos) ' insertion index if not exact match

    Dim jRange As Integer = 0
    While iStamp < stamps.Length AndAlso jRange < merged.Count
      Dim ts As DateTime = stamps(iStamp)
      Dim rng As TimeRange = merged(jRange)

      If ts < rng.Start Then
        iStamp += 1
      ElseIf ts > rng.[End] Then
        jRange += 1
      Else
        result.Add(CLng(iStamp)) ' inside range
        iStamp += 1
      End If
    End While

    Return result
  End Function


  Private Shared Function SalvaParquetCompact(PathFileOrigine As String, Variabili As clsVariabili, SelectedOnly As Boolean, ExportToParquet As Boolean) As String
    Dim pfi As New System.IO.FileInfo(PathFileOrigine)
    Dim fd = pfi.Directory.FullName
    Dim nfd = Path.Combine(fd, "Compact")
    If SelectedOnly Then nfd = Path.Combine(fd, "SuperCompact")
    If ExportToParquet Then nfd = Path.Combine(fd, "Parquet")
    System.IO.Directory.CreateDirectory(nfd)
    Dim Estensione As String = pfi.Extension
    Dim ParquetName As String = "Compact_" & pfi.Directory.Name
    If SelectedOnly Then ParquetName = "SuperCompact_" & pfi.Directory.Name
    If ExportToParquet Then ParquetName = "Parquet_" & pfi.Directory.Name
    If ExportToParquet Then Estensione = ".parquet"
    Dim pathParquet As String = Path.Combine(nfd, ParquetName & Estensione) ' pfi.FullName.Replace(pfi.Name, ParquetName)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In Variabili.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori.ToArray))
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

    Return pathParquet
  End Function

  Private Shared Function SalvaParquetCompactV2(
    pathFileOrigine As String,
    variabili As clsVariabili,
    selectedOnly As Boolean,
    exportToParquet As Boolean
) As String

    Dim pfi As New System.IO.FileInfo(pathFileOrigine)
    Dim baseDir As String = pfi.Directory.FullName

    Dim subFolder As String, prefix As String, estensione As String = pfi.Extension
    If exportToParquet Then
      subFolder = "Parquet" : prefix = "Parquet_" : estensione = ".parquet"
    ElseIf selectedOnly Then
      subFolder = "SuperCompact" : prefix = "SuperCompact_"
    Else
      subFolder = "Compact" : prefix = "Compact_"
    End If

    Dim outDir As String = System.IO.Path.Combine(baseDir, subFolder)
    System.IO.Directory.CreateDirectory(outDir)

    Dim parquetName As String = prefix & pfi.Directory.Name
    Dim pathParquet As String = System.IO.Path.Combine(outDir, parquetName & estensione)

    ' ---- Guards
    If variabili Is Nothing OrElse variabili.Lista Is Nothing OrElse variabili.Lista.Count = 0 Then
      Return pathParquet
    End If

    ' ---- Schema (all non-nullable Double columns)
    Dim chCount As Integer = variabili.Lista.Count
    Dim fields(chCount - 1) As Parquet.Data.DataField
    For i As Integer = 0 To chCount - 1
      Dim ch = variabili.Lista(i)
      fields(i) = New Parquet.Data.DataField(ch.Nome, Parquet.Data.DataType.Double, hasNulls:=False)
    Next
    'Dim schema As New Parquet.Schema.ParquetSchema(fields)
    Dim schema As New Parquet.Data.Schema(fields)

    ' ---- Row count = minimum across channels
    Dim rowCount As Integer = Integer.MaxValue
    For i As Integer = 0 To chCount - 1
      Dim cnt As Integer = variabili.Lista(i).Valori?.Count
      If cnt <= 0 Then Return pathParquet
      If cnt < rowCount Then rowCount = cnt
    Next

    ' ---- Binary stream + Parquet writer
    Using fs As New System.IO.FileStream(pathParquet, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read, bufferSize:=1 << 20, options:=System.IO.FileOptions.SequentialScan)
      Using writer As New Parquet.ParquetWriter(schema, fs)
        writer.CompressionMethod = Parquet.CompressionMethod.Snappy

        ' You can change this to rowCount if memory allows (writes one row group)
        Const defaultBatchRows As Integer = 65536
        Dim batchRows As Integer = If(rowCount < defaultBatchRows, rowCount, defaultBatchRows)

        Dim offset As Integer = 0
        While offset < rowCount
          Dim take As Integer = Math.Min(batchRows, rowCount - offset)
          Using rg As Parquet.ParquetRowGroupWriter = writer.CreateRowGroup()
            For i As Integer = 0 To chCount - 1
              Dim lst As System.Collections.Generic.List(Of Double) = variabili.Lista(i).Valori

              If take = rowCount Then
                ' Single row group → one ToArray() per column
                Dim arr As Double() = lst.ToArray()
                If arr.Length <> rowCount Then
                  Dim sliced = New Double(rowCount - 1) {}
                  System.Array.Copy(arr, 0, sliced, 0, rowCount)
                  arr = sliced
                End If
                rg.WriteColumn(New Parquet.Data.DataColumn(fields(i), arr))
              Else
                ' Chunked → allocate exact-size slice and copy once
                Dim exact = New Double(take - 1) {}
                lst.CopyTo(offset, exact, 0, take)
                rg.WriteColumn(New Parquet.Data.DataColumn(fields(i), exact))
              End If
            Next
          End Using
          offset += take
        End While
      End Using
    End Using

    Return pathParquet
  End Function


End Class


Public Class clsCompaqParquet
  Dim pVariabili As New clsVariabili

  Public Sub New()

    If Not DataProvider2020 Is Nothing Then
      If DataProvider2020.ValoriCaricati Then
        Dim testo As String = "Do you want to make a file with checked periods data only?" & vbCrLf
        testo &= "YES to proceed" & vbCrLf
        testo &= "NO" & vbCrLf
        If MsgBox(testo, MsgBoxStyle.YesNo, "Compaq File Creation") = MsgBoxResult.Yes Then
          LoadingProgressVisualizza()
          RiempiVariabili()
          SalvaParquet(DataProvider2020.Files.First.FullName)
          LoadingProgressNascondi()
        End If
      End If
    End If

  End Sub

  Private Sub RiempiVariabili()
    Dim strTmp As String = ""
    Dim bSystemTime_DaySeconds As Boolean = False
    pVariabili.Lista.Clear()
    For Each canale In DataProvider2020.Channels.ListaCanali
      If Not canale.IsMath Then
        pVariabili.Lista.Add(New clsVariabile(canale.ActualLogHeader))
        pVariabili.Lista.Last.Canale = canale
        If canale.ActualLogHeader = "SystemTime_DaySeconds" Then bSystemTime_DaySeconds = True
        'strTmp &= canale.ActualLogHeader & vbCrLf
      End If
    Next
    If Not bSystemTime_DaySeconds Then
      pVariabili.Lista.Add(New clsVariabile("SystemTime_DaySeconds"))
      Stop
      'pVariabili.Lista.Last.Canale = DataProvider2020.Channels.CanaleDT
      'strTmp &= "SystemTime_DaySeconds" & vbCrLf
    End If
    'Clipboard.SetText(strTmp)

    Dim IdRigaIniziale As Integer = 0
    Dim IdRigaFinale As Integer = 0
    For Each periodo In PeriodsManager.Periods.Lista.OrderBy(Function(x) x.TR.Start)
      If periodo.IsChecked Then
        Dim TR As New clsTimeRange(periodo.TR.Start.AddSeconds(-60), periodo.TR.Finish.AddSeconds(60))
        'va controllato che non vengano aggiunti due volte gli stessi periodi

        Dim IdRigaInizialeTmp As Integer = DataProvider2020.TrovaIndice(TR.Start)
        If IdRigaInizialeTmp < 0 Then IdRigaInizialeTmp = 0
        If IdRigaInizialeTmp < IdRigaFinale Then
          IdRigaIniziale = IdRigaFinale
        Else
          IdRigaIniziale = IdRigaInizialeTmp
        End If
        Dim IdRigaFinaleTmp As Integer = DataProvider2020.TrovaIndice(TR.Finish)
        If IdRigaFinaleTmp > IdRigaFinale Then
          If IdRigaFinaleTmp > DataProvider2020.TimeRange.IdRigaFinale Then
            IdRigaFinale = DataProvider2020.TimeRange.IdRigaFinale
          Else
            IdRigaFinale = IdRigaFinaleTmp
          End If
        End If
        strTmp &= TR.StringaPeriodo & vbTab & IdRigaIniziale & vbTab & IdRigaFinale & vbCrLf

        For Each Variabile In pVariabili.Lista
          If Variabile.Canale.Valori Is Nothing Then
            If Not bSystemTime_DaySeconds AndAlso Variabile.Nome = "SystemTime_DaySeconds" Then
              For i As Integer = IdRigaIniziale To IdRigaFinale
                'If i > Variabile.Canale.ValoriDT.Count - 1 Then Stop
                Dim Valore As Double = Variabile.Canale.ValoriDT(i).TimeOfDay.TotalSeconds
                Variabile.Valori.Add(Valore)
              Next
            ElseIf Variabile.Nome = "SystemTime_Date" Then
              For i As Integer = IdRigaIniziale To IdRigaFinale
                'If i > Variabile.Canale.ValoriDT.Count - 1 Then Stop
                Dim Valore As Double = Variabile.Canale.ValoriDT(i).ToString("yyyyMMdd")
                Variabile.Valori.Add(Valore)
              Next
            Else
              For i As Integer = IdRigaIniziale To IdRigaFinale
                'If i > Variabile.Canale.ValoriDT.Count - 1 Then Stop
                Dim Valore As Double = Variabile.Canale.ValoriDT(i).ToOADate
                Variabile.Valori.Add(Valore)
              Next
            End If
          Else
            For i As Integer = IdRigaIniziale To IdRigaFinale
              'If i > Variabile.Canale.Valori.Count - 1 Then Stop
              Dim Valore As Double = Variabile.Canale.Valori(i)
              Variabile.Valori.Add(Valore)
            Next
          End If
        Next
      End If
    Next
    Clipboard.SetText(strTmp)

  End Sub


  Private Sub SalvaParquet(PathFileOrigine As String)
    Dim pfi As New System.IO.FileInfo(PathFileOrigine)
    Dim Estensione As String = pfi.Extension
    Dim ParquetName As String = "c_" & pfi.Name
    Dim pathParquet As String = pfi.FullName.Replace(pfi.Name, ParquetName)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In pVariabili.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      'DataFields.Add(DataField)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori.ToArray))
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
    ApriExplorer(pathParquet)
  End Sub

End Class


Class clsReadGZipFiles

  Private ptr As FileStream
  Private UnGZPtr As Compression.GZipStream
  Private line_ptr As StreamReader
  Private spath As String

  Sub New(full_filename As String)
    spath = full_filename
    Open()
  End Sub

  Sub Open()
    Me.ptr = File.OpenRead(spath)
    Me.UnGZPtr = New Compression.GZipStream(ptr, Compression.CompressionMode.Decompress)
    Me.line_ptr = New StreamReader(UnGZPtr)
  End Sub

  Function NextLine() As String
    'will return Nothing if EOF
    Return Me.line_ptr.ReadLine()
  End Function

  Sub Close()
    Me.line_ptr.Close()
    Me.line_ptr.Dispose()
    Me.UnGZPtr.Close()
    Me.UnGZPtr.Dispose()
    Me.ptr.Close()
    Me.ptr.Dispose()
  End Sub

End Class

Public Class clsFaRoLogToParquet
  Dim pRigheFileTesto As New List(Of String)
  Dim pVariabili As New clsVariabili
  Public Property SelectedFaRoFileSource As String
  Public Property PathFileParquetCreato As String

  Public Property ListaFaRoFilesSource As List(Of String)
  Public Property ListaFileParquetCreati As New List(Of String)

  Public Sub New()
    Dim UltimoPath As String = AppConfig.ActiveProfile.LastFaroToParquet '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "FaRoToParquet", "LastImported", AppConfig.ApplicationDataFolder, True, True)
    'SelectedFaRoFileSource = ObjFiles.SelezionaFile(UltimoPath, "Select FaRo File", "FaRo Log Files |*.txt;*.gz;*.bin|All Files|*.*", "txt", "")
    Dim fn As New List(Of String)
    ListaFaRoFilesSource = ObjFiles.SelezionaFiles(UltimoPath, "Select FaRo File", "FaRo Log Files |*.txt;*.gz;*.bin|All Files|*.*", "txt", fn)

    ListaFileParquetCreati.Clear()
    If Not ListaFaRoFilesSource Is Nothing Then
      For Each f In ListaFaRoFilesSource
        SelectedFaRoFileSource = f
        If Not SelectedFaRoFileSource = "" Then
          AppConfig.ActiveProfile.LastFaroToParquet = SelectedFaRoFileSource
          Dim Fi As New System.IO.FileInfo(SelectedFaRoFileSource)
          Select Case Fi.Extension.ToLower
            Case ".gz"
              LeggiFileFaRoGzip(SelectedFaRoFileSource)
            Case ".txt"
              LeggiFileFaRoTxt(SelectedFaRoFileSource)
            Case ".csv"
              LeggiFileFaRoCsv(SelectedFaRoFileSource)
            Case ".bin"
              LeggiFileFaRoBin(SelectedFaRoFileSource)
          End Select
          SalvaParquet(SelectedFaRoFileSource, False)
          ListaFileParquetCreati.Add(PathFileParquetCreato)
        End If
      Next
      ApriExplorer(SelectedFaRoFileSource)
    End If
  End Sub


  Private Sub LeggiFileFaRoGzip(PathFile As String)
    'Stop
    Dim GZ As New clsReadGZipFiles(PathFile)
    Dim Riga As String
    pRigheFileTesto.Clear()
    pVariabili.Lista.Clear()
    Do
      Riga = GZ.NextLine
      If Riga = Nothing Then Exit Do
      pRigheFileTesto.Add(Riga)
    Loop
    For Each Riga In pRigheFileTesto
      Dim Contenuto As List(Of String) = Riga.Split(vbTab).ToList
      For Variabile As Integer = 0 To Contenuto.Count - 1
        If Riga Is pRigheFileTesto.First Then
          pVariabili.Lista.Add(New clsVariabile(Contenuto(Variabile)))
        Else
          pVariabili.Lista(Variabile).Valori.Add(StringToDouble(Contenuto(Variabile)))
        End If
      Next
    Next
  End Sub

  Private Sub LeggiFileFaRoTxt(PathFile As String)
    'Stop
    pRigheFileTesto.AddRange(System.IO.File.ReadAllLines(PathFile))
    For Each Riga In pRigheFileTesto
      Dim Contenuto As List(Of String) = Riga.Split(vbTab).ToList
      For Variabile As Integer = 0 To Contenuto.Count - 1
        If Riga Is pRigheFileTesto.First Then
          pVariabili.Lista.Add(New clsVariabile(Contenuto(Variabile)))
        Else
          pVariabili.Lista(Variabile).Valori.Add(StringToDouble(Contenuto(Variabile)))
        End If
      Next
    Next
  End Sub

  Private Sub LeggiFileFaRoCsv(PathFile As String)
    Stop
    pRigheFileTesto.AddRange(System.IO.File.ReadAllLines(PathFile))
    For Each Riga In pRigheFileTesto
      Dim Contenuto As List(Of String) = Riga.Split(";").ToList
      For Variabile As Integer = 0 To Contenuto.Count - 1
        If Riga Is pRigheFileTesto.First Then
          pVariabili.Lista.Add(New clsVariabile(Contenuto(Variabile)))
        Else
          pVariabili.Lista(Variabile).Valori.Add(StringToDouble(Contenuto(Variabile)))
        End If
      Next
    Next
  End Sub

  Private Function StringToDouble(Valore As String) As Double
    'Stop
    If IsNumeric(Valore) Then Return CDbl(Valore)
    Return Double.NaN
  End Function

  Private Sub LeggiFileFaRoBin(PathFile As String)
    Dim FileFaRoBin As New clsFaRoBin(New FileInfo(PathFile), False)
    Dim Contatore As Integer = 0
    For Each Riga In FileFaRoBin.Records
      'Console.WriteLine(Riga.ValoriDbl.Count)
      For Variabile As Integer = 0 To Riga.Valori.Count - 1
        If Riga Is FileFaRoBin.Records.First Then
          'Clipboard.SetText(String.Join(vbTab, Riga.Valori.ToArray))
          pVariabili.Lista.Add(New clsVariabile(Riga.Valori(Variabile)))
        Else
          If Variabile < pVariabili.Lista.Count Then
            'Clipboard.SetText(String.Join(vbTab, Riga.ValoriDbl.ToArray))
            pVariabili.Lista(Variabile).Valori.Add(Riga.ValoriDbl(Variabile))
          End If
        End If
      Next
      Contatore += 1

    Next
    FileFaRoBin = Nothing
  End Sub


  Private Sub SalvaParquet(PathFileOrigine As String, ApriFolder As Boolean)
    Dim pfi As New System.IO.FileInfo(PathFileOrigine)
    Dim Estensione As String = pfi.Extension
    Dim ParquetName As String = pfi.Name.Replace(Estensione, ".ppf")
    PathFileParquetCreato = pfi.FullName.Replace(pfi.Name, ParquetName)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each canale In pVariabili.Lista
      Dim DataField As New Parquet.Data.DataField(canale.Nome, Parquet.Data.DataType.Double, False)
      'DataFields.Add(DataField)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori.ToArray))
    Next
    Dim fsTmp As New System.IO.StreamWriter(PathFileParquetCreato)
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
    If ApriFolder Then ApriExplorer(PathFileParquetCreato)
  End Sub


End Class

Public Class clsVariabili
  Dim pLista As New List(Of clsVariabile)

  Public Property Lista As List(Of clsVariabile)
    Get
      Return pLista
    End Get
    Set(value As List(Of clsVariabile))
      pLista = value
    End Set
  End Property

End Class

Public Class clsVariabile
  Dim pNome As String
  Dim pValori As New List(Of Double)
  Dim pCanale As clsChannel2020

  Public Sub New(Nome As String)
    pNome = Nome

  End Sub

  Public Sub New(Canale As clsChannel2020)
    pNome = Canale.ActualLogHeader
    pCanale = Canale

  End Sub

  Public Property Valori As List(Of Double)
    Get
      Return pValori
    End Get
    Set(value As List(Of Double))
      pValori = value
    End Set
  End Property

  Public Property Nome As String
    Get
      Return pNome
    End Get
    Set(value As String)
      pNome = value
    End Set
  End Property

  Public Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
    Set(value As clsChannel2020)
      pCanale = value
    End Set
  End Property

End Class


'Public Class clsParquetRecord
'  Dim pValori As String()
'  Dim pValoriDbl As Double()
'  Dim pMomentoDT As DateTime

'  Public Sub New(NumeroValori As Integer)
'    ReDim pValori(NumeroValori)
'    ReDim pValoriDbl(NumeroValori)
'  End Sub

'  Public Property Valori As String()
'    Get
'      Return pValori
'    End Get
'    Set(value As String())
'      pValori = value
'    End Set
'  End Property

'  Public Property ValoriDbl As Double()
'    Get
'      Return pValoriDbl
'    End Get
'    Set(value As Double())
'      pValoriDbl = value
'    End Set
'  End Property

'  Public Property MomentoDT As Date
'    Get
'      Return pMomentoDT
'    End Get
'    Set(value As Date)
'      pMomentoDT = value
'    End Set
'  End Property

'  Public Sub AggiornaTime(IdCampoSecFromMidnight As Integer, IdCampoData As Integer)
'    Dim pSecFromMidnight As Double = pValoriDbl(IdCampoSecFromMidnight)
'    pMomentoDT = New Date(pValoriDbl(IdCampoData).ToString.Substring(0, 4), pValoriDbl(IdCampoData).ToString.Substring(4, 2), pValoriDbl(IdCampoData).ToString.Substring(6, 2), 0, 0, 0)
'    'pMomentoDT = CDate(pValoriDbl(IdCampoData).ToString)
'    pMomentoDT = pMomentoDT.AddSeconds(pSecFromMidnight)
'  End Sub

'End Class

