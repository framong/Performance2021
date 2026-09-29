
Imports System.ComponentModel
Imports SPwpf

'Public Class clsChannels
'	Dim pCanaliConosciuti As New List(Of clsChannel2020)
'	Dim pCanaliChiaveDefault As New List(Of clsChannel2020)
'	Dim pDizionarioHeadersCanaliChiaveDefault As New Dictionary(Of String, Integer)
'  Dim pCanaliChiave As New List(Of clsChannel2020)
'  'Dim pCanaliChiaveCaricati As List(Of clsChannel2020)
'  Dim pCanaliCaricati As List(Of clsChannel2020)
'	Dim pCanaliNumericiCaricati As List(Of clsChannel2020)
'	Dim pCanaliTrovati As List(Of clsChannel2020)
'  Dim pCanaliMath As New List(Of clsChannel2020)
'  Dim pDataProvider As clsDataProvider2020
'	Dim pLoadedToBeImportedConfiguration As New Dictionary(Of Integer, Boolean)
'	Dim pToggleImporta As eToggleImporta = eToggleImporta.eDefault
'	Dim pRndm As New System.Random()
'	Dim pCanaleDT As New clsChannel2020("CanaleDT")
'	'Dim pCanaleNVR As New clsChannel2020("CanaleNVR")
'	Dim pCanaleValidRows As clsValidRows



'	Public Enum eChannels
'    'NON CAMBIARE ID!!!!!! accodarne solamente di nuovi
'    eCanaleDT = -2
'    eNone = -1
'		eDateTime = 0
'		eDateOnly = 1
'		eTimeOnly = 2
'		eSOW = 3
'		eHDG = 4
'		eSOG = 5
'		eCOG = 6
'		eLat = 7
'		eLng = 8
'		eLWY = 9
'		eHEEL = 10
'		eTRIM = 11
'		eYRT = 12
'		ePRT = 13
'		eRRT = 14
'		eTWA = 15
'		eTWS = 16
'		eTWD = 17
'		eAWA = 18
'		eAWS = 19
'		eRdrAngle = 20
'		eRdrRake = 21
'		eRdrAngle2 = 22
'		eRdrRake2 = 23
'		ePortCantAngle = 24
'		eStbdCantAngle = 25
'		eRideHeightPointX = 26
'		eRideHeightPointY = 27
'		eIsStbd = 28
'		eVMG = 29
'		eVMGp = 30
'		eBSTp = 31
'		eBSPp = 32
'		eTWAd = 33
'		eRideHeight = 34
'		eMWS = 35
'		eMWA = 36
'		eSOGms = 37
'		eIsPavarot = 38
'		ePortFoilOutFlap2Angle = 39
'		ePortFoilOutFlap1Angle = 40
'		ePortFoilInFlap1Angle = 41
'		ePortFoilInFlap2Angle = 42
'		eStbdFoilInFlap2Angle = 43
'		eStbdFoilInFlap1Angle = 44
'		eStbdFoilOutFlap1Angle = 45
'		eStbdFoilOutFlap2Angle = 46
'		eLwdCantAngle = 47
'		eWwdCantAngle = 48
'		eLwdFoilOutFlap2Angle = 49
'		eLwdFoilOutFlap1Angle = 50
'		eLwdFoilInFlap1Angle = 51
'		eLwdFoilInFlap2Angle = 52
'		eWwdFoilInFlap2Angle = 53
'		eWwdFoilInFlap1Angle = 54
'		eWwdFoilOutFlap1Angle = 55
'		eWwdFoilOutFlap2Angle = 56
'		eCSE = 57
'		ePortCantAngleEffective = 58
'		eStbdCantAngleEffective = 59
'		eLwdCantAngleEffective = 60
'		eWwdCantAngleEffective = 61
'		eRdrRakeEffective = 62
'		eBsTwsRatio = 63
'    eVmgTwsRatio = 64
'    eTrimRT = 65
'    eHeelRT = 66

'    ePortV1 = 67
'    ePortD1 = 68
'    ePortRunner = 69
'    ePortJibSheet = 70
'    ePortJibUpDn = 71

'    eStbdV1 = 72
'    eStbdD1 = 73
'    eStbdRunner = 74
'    eStbdJibSheet = 75
'    eStbdJibUpDn = 76

'    eLwdV1 = 77
'    eLwdD1 = 78
'    eLwdRunner = 79
'    eLwdJibSheet = 80
'    eLwdJibUpDn = 81

'    eWwdV1 = 82
'    eWwdD1 = 83
'    eWwdRunner = 84
'    eWwdJibSheet = 85
'    eWwdJibUpDn = 86

'    eSpannerCounter = 87
'    eTopCtrlArmCounter = 88
'    eTravellerCounter = 89

'    eSpanner = 90
'    eTopCtrlArm = 91
'    eTraveller = 92
'    eTopTwist = 93

'    eRoll = 94


'  End Enum

'  Public Sub New(ByRef DataProvider As clsDataProvider2020, objChannels As clsChannels)
'    pDataProvider = DataProvider

'    'riempie la lista dei canali base
'    RigheStopWatch.Add("CaricaCanaliChiaveDiDefault Inizio: " & StpW.ElapsedMilliseconds)
'    If objChannels Is Nothing Then
'      CaricaCanaliChiaveDiDefault()
'    Else
'      pCanaliChiaveDefault = objChannels.CanaliChiaveDefault
'      pCanaliChiave = objChannels.CanaliChiave
'      pCanaliMath = objChannels.CanaliMath
'      pDizionarioHeadersCanaliChiaveDefault = objChannels.DizionarioHeadersCanaliChiaveDefault
'    End If
'    'mappa tutti i canali presenti del data provider
'    RigheStopWatch.Add("CaricaCanaliChiaveDiDefault Fine and MappaCanali Inizio: " & StpW.ElapsedMilliseconds)
'    MappaCanali() '(True)
'    RigheStopWatch.Add("MappaCanali Fine: " & StpW.ElapsedMilliseconds)
'  End Sub

'  Public Sub CaricaValori()
'    CaricaValoriCanaliCaricati()
'  End Sub

'	Public Sub CaricaValoriSqLite()
'		CaricaValoriCanaliCaricatiSqLite()
'	End Sub

'  Public Sub CaricaValoriFaroBin()
'    CaricaValoriCanaliCaricatiFaroBin()
'  End Sub

'  Public Sub CaricaValoriParquet()
'    CaricaValoriCanaliCaricatiParquet()
'  End Sub

'  Public ReadOnly Property DizionarioHeadersCanaliChiaveDefault As Dictionary(Of String, Integer)
'    Get
'      Return pDizionarioHeadersCanaliChiaveDefault
'    End Get
'  End Property

'  Public ReadOnly Property CanaliChiaveDefault As List(Of clsChannel2020)
'    Get
'      Return pCanaliChiaveDefault
'    End Get
'  End Property

'  Public ReadOnly Property CanaliChiave As List(Of clsChannel2020)
'    Get
'      Return pCanaliChiave
'    End Get
'  End Property

'  Public ReadOnly Property CanaliCaricati As List(Of clsChannel2020)
'		Get
'			Return pCanaliCaricati
'		End Get
'	End Property

'	Public ReadOnly Property CanaliNumericiCaricati As List(Of clsChannel2020)
'		Get
'			Return pCanaliNumericiCaricati
'		End Get
'	End Property

'	Public ReadOnly Property CanaliTrovati As List(Of clsChannel2020)
'		Get
'			Return pCanaliTrovati
'		End Get
'	End Property

'	'Public ReadOnly Property CanaliOrdinati As List(Of clsChannel2020)
'	'	Get
'	'		'Dim Lista = dataProvider2020.Channels.CanaliTrovati.OrderBy(Function(x) x.ActualLogHeader).OrderByDescending(Function(x) x.Importa)
'	'		Return pCanaliTrovati.OrderBy(Function(x) x.ActualLogHeader).OrderByDescending(Function(x) x.Importa)
'	'	End Get
'	'End Property

'	Public ReadOnly Property CanaliMath As List(Of clsChannel2020)
'		Get
'			Return pCanaliMath
'		End Get
'	End Property

'	Public ReadOnly Property CanaleDateTime As clsChannel2020
'		Get
'			Return pCanaleDT
'		End Get
'	End Property

'	Public ReadOnly Property CanaleChiave(Channel As eChannels) As clsChannel2020
'		Get
'      If Channel < 0 Then
'        Return Nothing
'      Else
'        If pCanaliCaricati Is Nothing Then
'          Return pCanaliChiave(Channel)
'        Else
'          If pCanaliChiave(Channel) Is Nothing Then Return Nothing
'          Return CanaleDaNomeNoSpaces(pCanaliChiave(Channel).LongNameNoSpaces)
'        End If
'      End If
'    End Get
'	End Property

'	Public ReadOnly Property Canale(IDcanale As Integer) As clsChannel2020
'		Get
'			Return pCanaliCaricati(IDcanale)
'		End Get
'	End Property

'  Public ReadOnly Property CanaleFileLog(Intestazione As String) As clsChannel2020
'    Get
'      For Each CanaleTmp As clsChannel2020 In pCanaliTrovati
'        If CanaleTmp.ActualLogHeader.ToLower = Intestazione Then
'          Return CanaleTmp
'        End If
'      Next
'      Return Nothing
'    End Get
'  End Property


'  Public ReadOnly Property Canale(LogHeader As String) As clsChannel2020
'		Get
'			If LogHeader.Trim = "" Then Return Nothing
'			For Each CanaleTmp As clsChannel2020 In pCanaliCaricati
'				If Not CanaleTmp.ActualLogHeader Is Nothing Then
'					If CanaleTmp.ActualLogHeader.ToLower = LogHeader.ToLower Then
'						Return CanaleTmp
'					End If
'				End If
'			Next
'			' cerca nell'intestazione dei canali math
'			For Each CanaleTmp As clsChannel2020 In pCanaliCaricati
'				If CanaleTmp.CanaleChiave.ToString.ToLower = LogHeader.ToLower Then
'					Return CanaleTmp
'				End If
'			Next
'			Return Nothing
'		End Get
'	End Property

'	Public ReadOnly Property CanaleDaNomeNoSpaces(LongNameNoSpaces As String) As clsChannel2020
'		Get
'			If LongNameNoSpaces.Trim = "" Then Return Nothing
'			For Each CanaleTmp As clsChannel2020 In pCanaliCaricati
'				If Not CanaleTmp.LongNameNoSpaces Is Nothing Then
'					If CanaleTmp.LongNameNoSpaces.ToLower = LongNameNoSpaces.ToLower Then
'						Return CanaleTmp
'					End If
'				End If
'			Next
'			' cerca nell'intestazione dei canali math
'			For Each CanaleTmp As clsChannel2020 In pCanaliCaricati
'				If CanaleTmp.CanaleChiave.ToString.ToLower = LongNameNoSpaces.ToLower Then
'					Return CanaleTmp
'				End If
'			Next
'			Return Nothing
'		End Get
'	End Property

'	Public ReadOnly Property TrovaCanale(PolarHeader As String) As clsChannel2020
'    Get
'      For Each CanaleTmp As clsChannel2020 In pCanaliTrovati
'        If CanaleTmp.PolarHeader.ToLower = PolarHeader.ToLower Then
'          Return CanaleTmp
'        End If
'      Next
'      Return Nothing
'    End Get
'  End Property

'  Public Property CanaleValidRows As clsValidRows
'		Get
'			Return pCanaleValidRows
'		End Get
'		Set(value As clsValidRows)
'			pCanaleValidRows = value
'		End Set
'	End Property


'	Public Function ImpostaTestValues() As Integer
'		Dim RigaMin As Integer = dataProvider2020.RawFileTimeRange.IdRigaInizialeRaw
'		Dim RigaMax As Integer = dataProvider2020.RawFileTimeRange.IdRigaFinaleRaw
'		If dataProvider2020.ValoriCaricati Then
'			RigaMin = dataProvider2020.TimeRange.IdRigaIniziale
'			RigaMax = dataProvider2020.TimeRange.IdRigaFinale
'		End If
'    Dim Riga As Integer = pRndm.Next(RigaMin, RigaMax)
'      ImpostaTestValues(Riga)
'		Return Riga
'	End Function


'	Public Sub ImpostaTestValues(Riga As Integer)
'		Dim RigaCaricata As Integer = 0
'		If dataProvider2020.ValoriCaricati Then
'			Dim DTraw As DateTime = dataProvider2020.TimeStamps(Riga)
'			RigaCaricata = dataProvider2020.TrovaIndice(DTraw)
'		End If
'		Select Case dataProvider2020.FileType
'			Case clsDataProvider2020.eFileType.eSQLite
'				For Each Chnl In pCanaliTrovati
'					Dim Vraw As String = dataProvider2020.SQLiteRecords(Riga).ValoriDbl(Chnl.IdCanale)
'					If dataProvider2020.ValoriCaricati Then
'						Chnl.ImpostaTestValue(Vraw, RigaCaricata)
'					Else
'						Chnl.ImpostaTestValue(Vraw)
'					End If
'				Next
'      Case clsDataProvider2020.eFileType.eFaRoBin
'        For Each Chnl In pCanaliTrovati
'          Dim Vraw As String = dataProvider2020.FaroBinRecords(Riga).ValoriDbl(Chnl.IdCanale)
'          If dataProvider2020.ValoriCaricati Then
'            Chnl.ImpostaTestValue(Vraw, RigaCaricata)
'          Else
'            Chnl.ImpostaTestValue(Vraw)
'          End If
'        Next
'      Case clsDataProvider2020.eFileType.eParquet
'        For Each Chnl In pCanaliTrovati
'          Dim Vraw As String = 0
'          If dataProvider2020.ValoriCaricati Then
'						Chnl.ImpostaTestValue(Vraw, RigaCaricata)
'					Else
'						Chnl.ImpostaTestValue(Vraw)
'					End If
'				Next
'			Case Else
'				For Each Chnl In pCanaliTrovati
'					Dim Vraw As String = dataProvider2020.RigheRaw(Riga).Split(dataProvider2020.DataCsep)(Chnl.IdCanale)
'					If dataProvider2020.ValoriCaricati Then
'						Chnl.ImpostaTestValue(Vraw, RigaCaricata)
'					Else
'						Chnl.ImpostaTestValue(Vraw)
'					End If
'				Next
'		End Select
'	End Sub


'	Private Function CaricaCanaliConosciuti() As Dictionary(Of String, Integer)

'		' va gestito quando il canale chiave non è salvato nel file xml.


'		Dim DizionarioTMP As New Dictionary(Of String, Integer)
'		Dim NodoCanali As Xml.XmlNode = AppConfig.CercaNodo(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, True)
'		pCanaliConosciuti = New List(Of clsChannel2020)

'		If Not NodoCanali Is Nothing Then
'			For Each Nodo As Xml.XmlNode In NodoCanali
'				Dim chTmp As New clsChannel2020(Nodo.Name)
'				chTmp.ActualLogHeader = "Nothing"
'				For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
'					Select Case SottoNodo.Name
'						Case "DataType"
'							chTmp.DataType = CInt(SottoNodo.InnerText)
'						Case "LongName"
'							chTmp.LongName = SottoNodo.InnerText
'						Case "ShortName"
'							chTmp.ShortName = SottoNodo.InnerText
'						Case "LongUM"
'							chTmp.LongUM = SottoNodo.InnerText
'						Case "ShortUM"
'							chTmp.ShortUM = SottoNodo.InnerText
'						Case "Load"
'							chTmp.Importa = SottoNodo.InnerText
'            Case "Decimals"
'              chTmp.Decimals = CInt(SottoNodo.InnerText)
'            Case "KeyChannel"
'							chTmp.CanaleChiave = CInt(SottoNodo.InnerText)
'						Case "Headers"
'							chTmp.HeadersDaStringa(SottoNodo.InnerText.Trim)
'							For Each intestazione In chTmp.Headers
'								If Not DizionarioTMP.ContainsKey(intestazione) Then
'									DizionarioTMP.Add(intestazione, pCanaliConosciuti.Count)
'								End If
'							Next
'            Case "IsMath"
'              chTmp.IsMath = CBool(SottoNodo.InnerText)
'            Case "PolarHeader"
'            Case Else
'              Stop
'					End Select
'				Next
'				pCanaliConosciuti.Add(chTmp)
'			Next
'		End If
'		Return DizionarioTMP
'	End Function

'	Private Sub AssociaIntestazioniToCanaliConosciuti(CanaliConosciuti As Dictionary(Of String, Integer))
'		Dim LoadedKeyChannels As New List(Of Integer)
'		Dim CanaliTrovatiTmp As New List(Of clsChannel2020)
'		CanaliTrovatiTmp.Clear()
'		pLoadedToBeImportedConfiguration.Clear()
'		For Int As Integer = 0 To pDataProvider.Intestazioni.MatriceValori.Length - 1
'			'gira in tutti i canali caricati nel data provider
'			Dim Header As String = pDataProvider.Intestazioni.MatriceValori(Int)
'			If Not Header.Trim = "" Then
'				Dim Canale As clsChannel2020
'				If CanaliConosciuti.ContainsKey(Header) Then
'					' L' header è presente tra quelli dei canali conosciuti
'					Canale = pCanaliConosciuti(CanaliConosciuti(Header))
'					'  il canale viene riempito con i valori del canale conosciuto
'					VerificaConfigurazioneCanale(Canale)
'					Canale.IdCanale = Int
'					Canale.ActualLogHeader = Header
'				Else
'					Canale = CreaNuovoCanale(Int, Header.Trim)
'					' cerca se è tra i canalichiavedefault
'					If pDizionarioHeadersCanaliChiaveDefault.ContainsKey(Canale.ActualLogHeader) Then
'						If Not LoadedKeyChannels.Contains(pDizionarioHeadersCanaliChiaveDefault(Canale.ActualLogHeader)) Then
'							' se il canale chiave ha più di una intestazione nota
'							' il canale chiave viene assegnato solo alla prima
'							Canale.CanaleChiave = pDizionarioHeadersCanaliChiaveDefault(Canale.ActualLogHeader)
'							Canale.DataType = pCanaliChiaveDefault(Canale.CanaleChiave).DataType
'              Canale.Decimals = pCanaliChiaveDefault(Canale.CanaleChiave).Decimals
'              Canale.LongName = pCanaliChiaveDefault(Canale.CanaleChiave).LongName
'							Canale.ShortName = pCanaliChiaveDefault(Canale.CanaleChiave).ShortName
'							Canale.LongUM = pCanaliChiaveDefault(Canale.CanaleChiave).LongUM
'							Canale.ShortUM = pCanaliChiaveDefault(Canale.CanaleChiave).ShortUM
'							Canale.IsMath = pCanaliChiaveDefault(Canale.CanaleChiave).IsMath

'							LoadedKeyChannels.Add(Canale.CanaleChiave)
'						End If
'					End If
'					' lo salva
'					Canale.SalvaSuXML(pDataProvider.SuffissoFileType, False)
'				End If

'				If Not Canale.CanaleChiave = eChannels.eNone Then
'					' viene impostata la lista dei canali chiave
'					pCanaliChiave(Canale.CanaleChiave) = Canale
'				End If
'				pLoadedToBeImportedConfiguration.Add(Canale.IdCanale, Canale.Importa)
'				CanaliTrovatiTmp.Add(Canale)

'				' i canali math se esistono vengono importati dopo di che sovrascritti dalla funzione math

'				'If pCanaliMath.Exists(Function(x) x.LongNameNoSpaces = Canale.LongNameNoSpaces) Then
'				'	Dim cTmp = pCanaliMath.Find(Function(x) x.LongNameNoSpaces = Canale.LongNameNoSpaces)
'				'	pCanaliMath.Remove(cTmp)
'				'End If
'			End If
'    Next

'		Dim Lista = CanaliTrovatiTmp.OrderBy(Function(x) x.ActualLogHeader).ThenByDescending(Function(x) x.Importa)

'		pCanaliTrovati = Lista.ToList

'		' salva il file XML in quanto potrebbero esserci stati nuovi canali
'		AppConfig.SalvaFileXML()
'	End Sub


'	Private Function CreaNuovoCanale(IndiceIntestazione As Integer, LogHeader As String) As clsChannel2020
'    Dim CHtmp As New clsChannel2020(LogHeader) With {
'                  .CanaleChiave = eChannels.eNone,
'                  .IdCanale = IndiceIntestazione,
'                  .ActualLogHeader = LogHeader,
'                  .Headers = {LogHeader}.ToList,
'                  .LongName = LogHeader,
'                  .ShortName = ShortNameFromLogHeader(LogHeader), 'LogHeader.Substring(0, System.Math.Min(3, LogHeader.Length)), ' qui potrei fare una funzione che fa lo short name se ci sono pocchi caratteri maiuscoli
'                  .LongUM = "",
'                  .ShortUM = "",
'                  .IsMath = False,
'                  .DataType = clsChannel2020.eDataType.eLinear,
'                  .Importa = True,
'                  .IsNuovoCanale = True,
'                  .Decimals = 1
'                }

'    Return CHtmp
'	End Function

'	Public Sub CorreggiShortNamesCanaliNonChiave()
'		For Each Chnl In pCanaliTrovati
'			If Chnl.CanaleChiave = -1 Then
'				Dim sn As String = ShortNameFromLogHeader(Chnl.LongName)
'				AppConfig.SalvaValoreInnerText(dataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Chnl.LongNameNoSpaces, "ShortName", sn, True, False)
'				Chnl.ShortName = sn
'			End If
'		Next
'		AppConfig.SalvaFileXML()
'	End Sub


'	Private Function ShortNameFromLogHeader(LogHeader As String) As String
'		Dim NomeUpper As String = ""
'		Dim NomeLower As String = ""
'		Dim Hdr As String = LogHeader.Replace("_ILS", "")
'		Hdr = Hdr.Replace("_ECC", "")
'		Hdr = Hdr.Replace("Ecc", "")
'		For Each C In Hdr
'			If Char.IsUpper(C) Then
'				NomeUpper &= C
'			Else
'				NomeLower &= C
'			End If
'		Next
'		If NomeUpper.Trim = "" OrElse NomeUpper.Length > 10 Then
'			Return Hdr.Substring(0, System.Math.Min(10, Hdr.Length))
'		Else
'			Return NomeUpper
'		End If
'	End Function

'	Private Sub MappaCanali()

'		pCanaliTrovati = New List(Of clsChannel2020)

'		Dim DizionarioTMP As Dictionary(Of String, Integer) = CaricaCanaliConosciuti()
'		AssociaIntestazioniToCanaliConosciuti(DizionarioTMP)

'		If pDataProvider.RigheDT Is Nothing Then Exit Sub
'		CaricaValoriCanaliCaricati()

'	End Sub

'	Private Function DefaultValueOfImportaCanale(Indice As Integer) As Boolean
'		Return pLoadedToBeImportedConfiguration.TryGetValue(Indice, False)
'	End Function

'	Public Enum eToggleImporta
'		eDefault = 0
'		eKeyChannelsOnly = 1
'		eAllChannels = 2
'		eNone = 3
'	End Enum

'	Public Function ToggleCanaliDaImportare() As String
'		Dim strTmp As String = ""
'		Select Case pToggleImporta
'			Case eToggleImporta.eDefault
'				pToggleImporta = eToggleImporta.eKeyChannelsOnly
'				strTmp = "Toggle Channels (Key)"
'			Case eToggleImporta.eKeyChannelsOnly
'				pToggleImporta = eToggleImporta.eAllChannels
'				strTmp = "Toggle Channels (All)"
'			Case eToggleImporta.eAllChannels
'				pToggleImporta = eToggleImporta.eNone
'				strTmp = "Toggle Channels (None)"
'			Case eToggleImporta.eNone
'				pToggleImporta = eToggleImporta.eDefault
'				strTmp = "Toggle Channels (Dflt)"
'		End Select
'		SetCanaliDaImportare()
'		Return strTmp
'	End Function

'	Public Sub SetCanaliDaImportare()
'		For Each Canale As clsChannel2020 In pCanaliTrovati
'			Select Case pToggleImporta
'				Case eToggleImporta.eDefault
'					Canale.Importa = DefaultValueOfImportaCanale(Canale.IdCanale)
'				Case eToggleImporta.eKeyChannelsOnly
'					Canale.Importa = Canale.IsCanaleChiave
'				Case eToggleImporta.eAllChannels
'					Canale.Importa = True
'				Case eToggleImporta.eNone
'					Canale.Importa = False
'			End Select
'		Next

'	End Sub

'	Public Sub SalvaConfigurazioneCanaliDaImportare()
'		For Each Canale As clsChannel2020 In pCanaliTrovati
'			AppConfig.SalvaValoreInnerText(dataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Load", Canale.Importa, True, False)
'		Next
'		AppConfig.SalvaFileXML()
'	End Sub

'  Private Sub CaricaValoriCanaliCaricati()

'    'qui devo inserire la funzione che riempie il campo data time
'    Dim RiempiCampoDateTime As Boolean = False
'    Dim CanaleVRtmp As New clsChannel2020("ValidRows")
'    Select Case pDataProvider.FileType
'      Case clsDataProvider2020.eFileType.eExpedition
'        Stop
'      Case clsDataProvider2020.eFileType.eFaRo, clsDataProvider2020.eFileType.eFaRoLR7d5
'        If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'          If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'            RiempiCampoDateTime = True
'          End If
'        End If
'        If RiempiCampoDateTime Then
'          'Dim adesso As DateTime = Now
'          'Console.WriteLine("0")
'          'Dim IDSFM As Integer = pDataProvider.IdCampoSecFromMidnight
'          'Dim CanaleData As Integer = CanaleChiave(eChannels.eDateOnly).IdCanale
'          'pCanaleDT.ValoriDT.Clear()
'          'Dim strData As String = pDataProvider.RigheRaw(pDataProvider.TimeRange.IdRigaInizialeRaw).Split(pDataProvider.DataCsep)(CanaleData)
'          'Dim Data As New DateTime(strData.Substring(0, 4), strData.Substring(4, 2), strData.Substring(6, 2), 0, 0, 0)
'          'Dim cSep As String = pDataProvider.DataCsep
'          'Dim SecFrmoMid As Double = 0
'          'For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'          '	SecFrmoMid = pDataProvider.RigheRaw(pDataProvider.RigheDT(R)).Split(cSep)(IDSFM)
'          '	pCanaleDT.ValoriDT.Add(FaroDateAndSecFromMidnightToSystemDT(Data, SecFrmoMid))
'          '	CanaleVRtmp.ValuesBl.Add(True)
'          'Next
'          'Console.WriteLine(Now.Subtract(adesso).TotalMilliseconds)

'          Dim IDD As Integer = CanaleChiave(eChannels.eDateOnly).IdCanale
'          Dim IDT As Integer = CanaleChiave(eChannels.eTimeOnly).IdCanale
'          pCanaleDT.ValoriDT.Clear()
'          For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'            'Dim Record As New clsRecord(R, pDataProvider)
'            Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'            pCanaleDT.ValoriDT.Add(Record.ValoreDT(IDD, IDT))
'            CanaleVRtmp.ValuesBl.Add(True)
'          Next
'          'Console.WriteLine(Now.Subtract(adesso).TotalMilliseconds)
'        End If
'      Case clsDataProvider2020.eFileType.eIMoth
'        Stop
'			Case clsDataProvider2020.eFileType.eSQLite
'				If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'					If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'						RiempiCampoDateTime = True
'					End If
'				End If
'				If RiempiCampoDateTime Then
'					Dim IDD As Integer = CanaleChiave(eChannels.eDateOnly).IdCanale
'					Dim IDT As Integer = CanaleChiave(eChannels.eTimeOnly).IdCanale
'					pCanaleDT.ValoriDT.Clear()
'					For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'						'Dim Record As New clsRecord(R, pDataProvider)
'						Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'						pCanaleDT.ValoriDT.Add(Record.ValoreDT(IDD, IDT))
'						CanaleVRtmp.ValuesBl.Add(True)
'					Next
'				End If
'			Case clsDataProvider2020.eFileType.eFaRoBin
'				If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'          If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'            RiempiCampoDateTime = True
'          End If
'        End If
'        If RiempiCampoDateTime Then
'          Dim IDD As Integer = CanaleChiave(eChannels.eDateOnly).IdCanale
'          Dim IDT As Integer = CanaleChiave(eChannels.eTimeOnly).IdCanale
'          pCanaleDT.ValoriDT.Clear()
'          For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'            'Dim Record As New clsRecord(R, pDataProvider)
'            Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'            pCanaleDT.ValoriDT.Add(Record.ValoreDT(IDD, IDT))
'            CanaleVRtmp.ValuesBl.Add(True)
'          Next
'        End If
'      Case clsDataProvider2020.eFileType.eParquet
'        Stop
'    End Select
'    If Not RiempiCampoDateTime Then
'      Dim Giorno As DateTime = pDataProvider.FCs.First.FI.LastWriteTime.Date
'      Giorno.AddSeconds(-pDataProvider.RigheDT.Length)
'      For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'        pCanaleDT.ValoriDT.Add(Giorno.AddSeconds(R))
'        CanaleVRtmp.ValuesBl.Add(True)
'      Next
'    End If
'    pCanaleValidRows = New clsValidRows(CanaleVRtmp)


'    pCanaliCaricati = New List(Of clsChannel2020)
'    pCanaliNumericiCaricati = New List(Of clsChannel2020)
'    'pCanaliChiaveCaricati = New List(Of clsChannel2020)




'    For Each Canale As clsChannel2020 In pCanaliTrovati


'      If Canale.CaricaCanale Then
'        pCanaliCaricati.Add(Canale)
'        'If Canale.IsCanaleChiave Then pCanaliChiaveCaricati.Add(Canale)
'        If Canale.IsNumericOrBoolean Then pCanaliNumericiCaricati.Add(Canale)

'        'qui carico i dati del canale
'        For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'          Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'          If Record.MatriceValori.Length > Canale.IdCanale Then
'            Dim v As String = Record.MatriceValori(Canale.IdCanale)
'            Select Case Canale.DataType
'              Case clsChannel2020.eDataType.eDateTime
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(Record.ValoreDT(Canale.IdCanale))
'              Case clsChannel2020.eDataType.eTimeOnly
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(Record.ValoreDT(Canale.IdCanale))
'              Case clsChannel2020.eDataType.eText
'                Canale.ValuesTxt.Add(v)
'              Case clsChannel2020.eDataType.eDateOnly
'                Canale.ValoriDT.Add(Record.ValoreData(Canale.IdCanale))
'              Case clsChannel2020.eDataType.eBoolean
'                Canale.ValuesBl.Add(v)
'              Case Else
'                If IsNumeric(v) Then
'                  Canale.Valori.Add(v)
'                Else
'                  If Canale.IsNuovoCanale Then
'                    Dim IsBool As Boolean = False
'                    Boolean.TryParse(v, IsBool)
'                    If IsBool Then
'                      Dim txt As String = "The new channel '" & Canale.Headers(0) & "' has '" & v & "' as value at row: " & R & ", do you whant to set channel's data type to boolean?"
'                      If MsgBox(txt, MsgBoxStyle.YesNo, "Channel Data Type") = MsgBoxResult.Yes Then
'                        Canale.DataType = clsChannel2020.eDataType.eBoolean
'                        AppConfig.SalvaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "DataType", Canale.DataType, False, False)
'                        AppConfig.SalvaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "DataType", Canale.DataType, True, True)
'                        Canale.ValuesBl.Add(Convert.ToBoolean(v))
'                      Else
'                        Canale.Valori.Add(vbNull)
'                      End If
'                    Else
'                      Dim txt As String = "The new channel '" & Canale.Headers(0) & "' has '" & v & "' as value at row: " & R & ", do you whant to set channel's data type to text?"
'                      If MsgBox(txt, MsgBoxStyle.YesNo, "Channel Data Type") = MsgBoxResult.Yes Then
'                        Canale.DataType = clsChannel2020.eDataType.eText
'                        AppConfig.SalvaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "DataType", Canale.DataType, False, False)
'                        AppConfig.SalvaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "DataType", Canale.DataType, True, True)
'                        Canale.ValuesTxt.Add(v)
'                      Else
'                        Canale.Valori.Add(vbNull)
'                      End If
'                    End If
'                    ' risolto il dubbio lo marca come canale non nuovo di modo da non rifare la domanda ai successivi record
'                    Canale.IsNuovoCanale = False
'                  Else
'                    Canale.Valori.Add(vbNull)
'                  End If
'                End If
'            End Select
'          Else
'            Canale.Valori.Add(vbNull)
'          End If

'        Next
'      End If
'    Next
'    ' loop per aggiungere i canali math a quelli trovati
'    'For Each CK In pCanaliMath
'    '  ' pCanaliMath a questo passaggio contiene solo i canali math non trovati nel file log e che vanno pertanto aggiunti
'    '  pCanaliCaricati.Add(CK)
'    '  pCanaliChiave(CK.CanaleChiave) = CK
'    'Next
'    For Each CK In pCanaliMath
'      ' se esiste un canale da caricare uguale a quello math
'      ' lo stesso viene segnato come math così da non aggiungere un nuovo canale identico

'      If pCanaliCaricati.Exists(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces) Then
'        Dim cTmp = pCanaliCaricati.Find(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces)
'        cTmp.IsMath = True
'        'pCanaliMath.Remove(cTmp)
'      Else
'        pCanaliCaricati.Add(CK)
'      End If
'      If Not CK.CanaleChiave = eChannels.eNone Then
'        pCanaliChiave(CK.CanaleChiave) = CK
'      End If
'    Next


'    Select Case pDataProvider.FileType
'      Case clsDataProvider2020.eFileType.eIMoth
'        'CanaleBase(eChannels.eHDG).IsMath = True
'    End Select

'    For Each Canale As clsChannel2020 In pCanaliCaricati
'      If Not Canale Is Nothing Then
'        ' vengono fatte le operazioni matematiche custom
'        If Canale.IsMath Then
'          MathsCanali(Canale)
'        End If
'      End If
'    Next
'  End Sub


'	Private Sub CaricaValoriCanaliCaricatiSqLite()

'		'qui devo inserire la funzione che riempie il campo data time
'		Dim RiempiCampoDateTime As Boolean = False
'		Dim CanaleVRtmp As New clsChannel2020("ValidRows")
'		Select Case pDataProvider.FileType
'			Case clsDataProvider2020.eFileType.eSQLite
'				If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'					If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'						RiempiCampoDateTime = True
'					End If
'				End If
'				If RiempiCampoDateTime Then
'					'Dim IDD As Integer = CanaleChiave(eChannels.eDateOnly).IdCanale
'					'Dim IDT As Integer = CanaleChiave(eChannels.eTimeOnly).IdCanale
'					pCanaleDT.ValoriDT.Clear()
'					For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'						'Dim Record As New clsRecord(R, pDataProvider)
'						'Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'						pCanaleDT.ValoriDT.Add(pDataProvider.SQLiteRecords(pDataProvider.RigheDT(R)).MomentoDT)
'						CanaleVRtmp.ValuesBl.Add(True)
'					Next
'				End If
'			Case Else
'				Stop
'		End Select
'		If Not RiempiCampoDateTime Then
'      Dim Giorno As DateTime = pDataProvider.FCs.First.FI.LastWriteTime.Date
'      Giorno.AddSeconds(-pDataProvider.RigheDT.Length)
'			For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'				pCanaleDT.ValoriDT.Add(Giorno.AddSeconds(R))
'				CanaleVRtmp.ValuesBl.Add(True)
'			Next
'		End If
'		pCanaleValidRows = New clsValidRows(CanaleVRtmp)


'		pCanaliCaricati = New List(Of clsChannel2020)
'		pCanaliNumericiCaricati = New List(Of clsChannel2020)
'		'pCanaliChiaveCaricati = New List(Of clsChannel2020)




'		For Each Canale As clsChannel2020 In pCanaliTrovati


'			If Canale.CaricaCanale Then
'				pCanaliCaricati.Add(Canale)
'				'If Canale.IsCanaleChiave Then pCanaliChiaveCaricati.Add(Canale)
'				If Canale.IsNumericOrBoolean Then pCanaliNumericiCaricati.Add(Canale)

'				'qui carico i dati del canale
'				For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'          Dim SQLrecord As clsSQLiteRecord2020 = pDataProvider.SQLiteRecords(pDataProvider.RigheDT(R))
'          'Dim Record As New clsRecord(pDataProvider.RigheDT(R), pDataProvider)
'          If SQLrecord.ValoriDbl.Length > Canale.IdCanale Then
'						Dim v As Double = SQLrecord.ValoriDbl(Canale.IdCanale)
'            Select Case Canale.DataType
'              Case clsChannel2020.eDataType.eDateTime
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eTimeOnly
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eText
'                Canale.ValuesTxt.Add(v)
'              Case clsChannel2020.eDataType.eDateOnly
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eBoolean
'                Canale.ValuesBl.Add(v)
'              Case Else
'                Canale.Valori.Add(v)
'            End Select
'          Else
'						Canale.Valori.Add(vbNull)
'					End If

'				Next
'			End If
'		Next
'    ' loop per aggiungere i canali math a quelli trovati
'    For Each CK In pCanaliMath
'      ' se esiste un canale da caricare uguale a quello math
'      ' lo stesso viene segnato come math così da non aggiungere un nuovo canale identico

'      If pCanaliCaricati.Exists(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces) Then
'        Dim cTmp = pCanaliCaricati.Find(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces)
'        cTmp.IsMath = True
'        'pCanaliMath.Remove(cTmp)
'      Else
'        pCanaliCaricati.Add(CK)
'      End If
'      If Not CK.CanaleChiave = eChannels.eNone Then
'        pCanaliChiave(CK.CanaleChiave) = CK
'      End If
'    Next
'    'For Each CK In pCanaliMath
'    '	' pCanaliMath a questo passaggio contiene solo i canali math non trovati nel file log e che vanno pertanto aggiunti
'    '	pCanaliCaricati.Add(CK)
'    '	pCanaliChiave(CK.CanaleChiave) = CK
'    'Next


'    Select Case pDataProvider.FileType
'			Case clsDataProvider2020.eFileType.eIMoth
'				'CanaleBase(eChannels.eHDG).IsMath = True
'		End Select

'		For Each Canale As clsChannel2020 In pCanaliCaricati
'			If Not Canale Is Nothing Then
'				' vengono fatte le operazioni matematiche custom
'				If Canale.IsMath Then
'					MathsCanali(Canale)
'				End If
'			End If
'		Next
'	End Sub


'  Private Sub CaricaValoriCanaliCaricatiFaroBin()

'    'qui devo inserire la funzione che riempie il campo data time
'    Dim RiempiCampoDateTime As Boolean = False
'    Dim CanaleVRtmp As New clsChannel2020("ValidRows")
'    Select Case pDataProvider.FileType
'      Case clsDataProvider2020.eFileType.eFaRoBin
'        If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'          If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'            RiempiCampoDateTime = True
'          End If
'        End If
'        If RiempiCampoDateTime Then
'          pCanaleDT.ValoriDT.Clear()
'          For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'            pCanaleDT.ValoriDT.Add(pDataProvider.FaroBinRecords(pDataProvider.RigheDT(R)).MomentoDT)
'            CanaleVRtmp.ValuesBl.Add(True)
'          Next
'        End If
'      Case Else
'        Stop
'    End Select
'    If Not RiempiCampoDateTime Then
'      Dim Giorno As DateTime = pDataProvider.FCs.First.FI.LastWriteTime.Date
'      Giorno.AddSeconds(-pDataProvider.RigheDT.Length)
'      For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'        pCanaleDT.ValoriDT.Add(Giorno.AddSeconds(R))
'        CanaleVRtmp.ValuesBl.Add(True)
'      Next
'    End If
'    pCanaleValidRows = New clsValidRows(CanaleVRtmp)


'    pCanaliCaricati = New List(Of clsChannel2020)
'    pCanaliNumericiCaricati = New List(Of clsChannel2020)

'    For Each Canale As clsChannel2020 In pCanaliTrovati


'      If Canale.CaricaCanale Then
'        pCanaliCaricati.Add(Canale)
'        If Canale.IsNumericOrBoolean Then pCanaliNumericiCaricati.Add(Canale)

'        'qui carico i dati del canale
'        For R As Integer = 0 To pDataProvider.RigheDT.Length - 1
'          Dim SQLrecord As clsFaroBinRecord = pDataProvider.FaroBinRecords(pDataProvider.RigheDT(R))
'          If SQLrecord.ValoriDbl.Length > Canale.IdCanale Then
'            Dim v As Double = SQLrecord.ValoriDbl(Canale.IdCanale)
'            Select Case Canale.DataType
'              Case clsChannel2020.eDataType.eDateTime
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eTimeOnly
'                ' va gestito l'eventuale errore di formato del record specifico
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eText
'                Canale.ValuesTxt.Add(v)
'              Case clsChannel2020.eDataType.eDateOnly
'                Canale.ValoriDT.Add(SQLrecord.MomentoDT)
'              Case clsChannel2020.eDataType.eBoolean
'                Canale.ValuesBl.Add(v)
'              Case Else
'                Canale.Valori.Add(v)
'            End Select
'          Else
'            Canale.Valori.Add(vbNull)
'          End If

'        Next
'      End If
'    Next
'    ' loop per aggiungere i canali math a quelli trovati
'    For Each CK In pCanaliMath
'      ' se esiste un canale da caricare uguale a quello math
'      ' lo stesso viene segnato come math così da non aggiungere un nuovo canale identico

'      If pCanaliCaricati.Exists(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces) Then
'        Dim cTmp = pCanaliCaricati.Find(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces)
'        cTmp.IsMath = True
'        'pCanaliMath.Remove(cTmp)
'      Else
'        pCanaliCaricati.Add(CK)
'      End If
'      If Not CK.CanaleChiave = eChannels.eNone Then
'        pCanaliChiave(CK.CanaleChiave) = CK
'      End If
'    Next


'    For Each Canale As clsChannel2020 In pCanaliCaricati
'      If Not Canale Is Nothing Then
'        ' vengono fatte le operazioni matematiche custom
'        If Canale.IsMath Then
'          MathsCanali(Canale)
'        End If
'        'Dim chtmp As clsChannel2020 = CanaleChiave(eChannels.eBsTwsRatio)
'      End If
'    Next
'  End Sub


'  Private Sub CaricaValoriCanaliCaricatiParquet()
'    Dim RiempiCampoDateTime As Boolean = False
'    Dim CanaleVRtmp As New clsChannel2020("ValidRows")
'    Select Case pDataProvider.FileType
'      Case clsDataProvider2020.eFileType.eParquet
'        If Not CanaleChiave(eChannels.eDateOnly) Is Nothing Then
'          If Not CanaleChiave(eChannels.eTimeOnly) Is Nothing Then
'            RiempiCampoDateTime = True
'          End If
'        End If
'        If RiempiCampoDateTime Then
'          pCanaleDT.ValuesDT = pDataProvider.FileParquet.Momenti.Cast(Of DateTime?).ToList
'          For R As Integer = 0 To pCanaleDT.ValoriDT.Count - 1
'            CanaleVRtmp.ValuesBl.Add(True)
'          Next
'        End If
'      Case Else
'        Stop
'    End Select
'    If Not RiempiCampoDateTime Then
'      pCanaleDT.ValuesDT = pDataProvider.FileParquet.Momenti.Cast(Of DateTime?).ToList
'      For R As Integer = 0 To pCanaleDT.ValoriDT.Count - 1
'        CanaleVRtmp.ValuesBl.Add(True)
'      Next
'    End If
'    pCanaleValidRows = New clsValidRows(CanaleVRtmp)


'    pCanaliCaricati = New List(Of clsChannel2020)
'    pCanaliNumericiCaricati = New List(Of clsChannel2020)
'    For Each Canale As clsChannel2020 In pCanaliTrovati
'      If Canale.CaricaCanale Then
'        pCanaliCaricati.Add(Canale)
'      End If
'    Next

'    pDataProvider.FileParquet.CaricaValoriCanaliFast(pCanaliCaricati)
'    'pDataProvider.FileParquet.CaricaValoriCanali(pCanaliCaricati)
'    ' loop per aggiungere i canali math a quelli trovati
'    For Each CK In pCanaliMath
'      ' se esiste un canale da caricare uguale a quello math
'      ' lo stesso viene segnato come math così da non aggiungere un nuovo canale identico

'      If pCanaliCaricati.Exists(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces) Then
'        Dim cTmp = pCanaliCaricati.Find(Function(x) x.LongNameNoSpaces = CK.LongNameNoSpaces)
'        cTmp.IsMath = True
'        'pCanaliMath.Remove(cTmp)
'      Else
'        pCanaliCaricati.Add(CK)
'      End If
'      If Not CK.CanaleChiave = eChannels.eNone Then
'        pCanaliChiave(CK.CanaleChiave) = CK
'      End If
'    Next

'    For Each Canale As clsChannel2020 In pCanaliCaricati
'      If Not Canale Is Nothing Then
'        ' vengono fatte le operazioni matematiche custom
'        If Canale.IsMath Then
'          MathsCanali(Canale)
'        End If
'        'Dim chtmp As clsChannel2020 = CanaleChiave(eChannels.eBsTwsRatio)
'      End If
'    Next

'  End Sub



'  Private Sub MathsCanali(Canale As clsChannel2020)
'		Select Case pDataProvider.FileType
'			Case clsDataProvider2020.eFileType.eExpedition
'				MathsCanaliExpedition(Canale)
'      Case clsDataProvider2020.eFileType.eFaRo
'        MathsCanaliFaRo(Canale)
'      Case clsDataProvider2020.eFileType.eFaRoLR7d5, clsDataProvider2020.eFileType.eFaRoBin
'        MathsCanaliFaRoLR7d5(Canale)
'      Case clsDataProvider2020.eFileType.eParquet
'        MathsCanaliParquet(Canale)
'      Case clsDataProvider2020.eFileType.eIMoth
'        MathsCanaliIMoth(Canale)
'			Case clsDataProvider2020.eFileType.eSQLite
'				MathsCanaliSQLite(Canale)
'		End Select
'	End Sub

'	Private Sub MathsCanaliExpedition(Canale As clsChannel2020)

'	End Sub

'	Private Sub MathsCanaliFaRo(Canale As clsChannel2020)
'		Select Case Canale.CanaleChiave
'			Case eChannels.eIsStbd
'				Dim PrevStbd As Boolean = True
'				If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'					pCanaliChiave(eChannels.eIsStbd).ValuesBl = New List(Of Boolean)
'					pCanaliChiave(eChannels.eIsPavarot).ValuesBl = New List(Of Boolean)
'					For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'						Dim IsStbd As Boolean = TWA > 0
'						pCanaliChiave(eChannels.eIsStbd).ValuesBl.Add(IsStbd)
'						pCanaliChiave(eChannels.eIsPavarot).ValuesBl.Add(Not IsStbd = PrevStbd)
'						PrevStbd = IsStbd
'					Next
'				End If
'			Case eChannels.eLat
'				Stop
'				For i As Integer = 0 To Canale.Valori.Count - 1

'				Next
'			Case eChannels.eLng
'				Stop
'			Case eChannels.eIsPavarot
'				'non fa nulla altrimenti lo farebbe due volte...
'		End Select
'	End Sub

'  Private Sub MathsCanaliFaRoLR7d5(Canale As clsChannel2020)
'    Select Case Canale.CanaleChiave
'      Case eChannels.eVMG
'        If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'          If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'            Dim chSow As clsChannel2020 = CanaliChiave(eChannels.eSOW)
'            Dim chTwa As clsChannel2020 = CanaliChiave(eChannels.eTWA)
'            Dim chTws As clsChannel2020 = CanaliChiave(eChannels.eTWS)
'            Dim chBsTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBsTwsRatio).LongNameNoSpaces)
'            Dim chVmgTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eVmgTwsRatio).LongNameNoSpaces)
'            Dim TwsOk As Boolean = chSow.CaricaCanale
'            Canale.Valori.Clear()
'            chBsTwsR.Valori.Clear()
'            chVmgTwsR.Valori.Clear()
'            For i As Long = 0 To chSow.Valori.Count - 1
'              Canale.Valori.Add(chSow.Valori(i) * System.Math.Abs(System.Math.Cos(Radians(chTwa.Valori(i)))))
'              If Not chTws Is Nothing Then
'                chBsTwsR.Valori.Add(chSow.Valori(i) / chTws.Valori(i) * 100)
'                chVmgTwsR.Valori.Add(Canale.Valori(i) / chTws.Valori(i) * 100)
'              End If
'            Next
'          End If
'        End If
'      Case eChannels.eIsStbd
'        Dim PrevStbd As Boolean = True
'        If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'          Dim chIsStbd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces)
'          Dim chIsPvrt As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces)
'          chIsStbd.ValuesBl.Clear()
'          chIsPvrt.ValuesBl.Clear()
'          For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'            Dim IsStbd As Boolean = TWA > 0
'            chIsStbd.ValuesBl.Add(IsStbd)
'            chIsPvrt.ValuesBl.Add(Not IsStbd = PrevStbd)
'            PrevStbd = IsStbd
'          Next
'        End If
'      Case eChannels.eIsPavarot
'        'non fa nulla altrimenti lo farebbe due volte...fatto con eIsStbd
'      Case eChannels.eBsTwsRatio, eChannels.eVmgTwsRatio
'        'non fa nulla altrimenti lo farebbe due volte...fatto con eChannels.eVMG
'      Case eChannels.eLwdCantAngle
'        AggiornaCanaliWindwardLeeward()
'      Case eChannels.eWwdCantAngle, eChannels.eLwdFoilOutFlap2Angle, eChannels.eLwdFoilOutFlap1Angle, eChannels.eLwdFoilInFlap1Angle, eChannels.eLwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap1Angle, eChannels.eWwdFoilOutFlap1Angle, eChannels.eWwdFoilOutFlap2Angle, eChannels.eWwdCantAngleEffective, eChannels.eLwdCantAngleEffective
'        ' già fatto con eLwdCantAngle
'      Case Else
'        'Stop
'    End Select
'  End Sub

'  Private Sub MathsCanaliParquet(Canale As clsChannel2020)
'    Select Case Canale.CanaleChiave
'      Case eChannels.eVMG
'        If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'          If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'            Dim chSow As clsChannel2020 = CanaliChiave(eChannels.eSOW)
'            Dim chTwa As clsChannel2020 = CanaliChiave(eChannels.eTWA)
'            Dim chTws As clsChannel2020 = CanaliChiave(eChannels.eTWS)
'            Dim chBsTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBsTwsRatio).LongNameNoSpaces)
'            Dim chVmgTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eVmgTwsRatio).LongNameNoSpaces)
'            Dim TwsOk As Boolean = chSow.CaricaCanale
'            Canale.Valori.Clear()
'            chBsTwsR.Valori.Clear()
'            chVmgTwsR.Valori.Clear()
'            For i As Long = 0 To chSow.Valori.Count - 1
'              Canale.Valori.Add(chSow.Valori(i) * System.Math.Abs(System.Math.Cos(Radians(chTwa.Valori(i)))))
'              If Not chTws Is Nothing Then
'                chBsTwsR.Valori.Add(chSow.Valori(i) / chTws.Valori(i) * 100)
'                chVmgTwsR.Valori.Add(Canale.Valori(i) / chTws.Valori(i) * 100)
'              End If
'            Next
'          End If
'        End If
'      Case eChannels.eVMGp
'        If Not Targets Is Nothing Then
'          Dim BS As clsPolare2019CanaleValori = Targets.GetPolare("bs")
'          If Not BS Is Nothing Then
'            If pCanaliChiave(eChannels.eTWS).CaricaCanale Then
'              If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'                If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'                  Dim chVMGp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eVMGp).LongNameNoSpaces)
'                  Dim chBSPp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBSPp).LongNameNoSpaces)
'                  Dim chBSTp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBSTp).LongNameNoSpaces)
'                  Dim chTWAd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eTWAd).LongNameNoSpaces)

'                  For i As Long = 0 To pCanaliChiave(eChannels.eTWS).Valori.Count - 1
'                    Dim TWS As Double = pCanaliChiave(eChannels.eTWS).Valori(i)
'                    Dim TWA As Double = System.Math.Abs(System.Math.Cos(Radians(pCanaliChiave(eChannels.eTWA).Valori(i))))
'                    Dim SOW As Double = pCanaliChiave(eChannels.eSOW).Valori(i)
'                    Dim VMG As Double = SOW * System.Math.Abs(System.Math.Cos(Radians(TWA)))
'                    Dim BSp As Double = BS.Valore(TWS, TWA)
'                    Dim BSt As Double = IIf(TWA > 90, BS.ValoreTargetDn(TWS), BS.ValoreTargetUp(TWS))
'                    Dim TWAt As Double = IIf(TWA > 90, BS.ValoreTargetDnReferece(TWS), BS.ValoreTargetUpReferece(TWS))
'                    Dim VMGt As Double = BSt * System.Math.Abs(System.Math.Cos(Radians(TWAt)))
'                    Dim BSTp As Double = SOW / BSt
'                    Dim BSPp As Double = SOW / BSPp
'                    Dim TWAd As Double = TWA - TWAt
'                    Dim VMGp As Double = TWA - TWAt
'                    chVMGp.Valori.Add(VMGp)
'                    chBSPp.Valori.Add(BSPp)
'                    chBSTp.Valori.Add(BSTp)
'                    chTWAd.Valori.Add(TWAd)
'                  Next
'                End If
'              End If
'            End If
'          End If
'        End If
'      Case eChannels.eBSPp
'        ' fatto con eVMGp
'      Case eChannels.eBSTp
'        ' fatto con eVMGp
'      Case eChannels.eTWAd
'        ' fatto con eVMGp
'      Case eChannels.ePRT
'        For i As Long = 0 To pCanaliChiave(eChannels.ePRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.ePRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eYRT
'        For i As Long = 0 To pCanaliChiave(eChannels.eYRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eYRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.eYRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eRRT
'        For i As Long = 0 To pCanaliChiave(eChannels.eRRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.eRRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eIsStbd
'        Dim PrevStbd As Boolean = True
'        If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'					Dim chIsStbd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces)
'					Dim chIsPvrt As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces)
'					chIsStbd.ValuesBl.Clear()
'					'chIsStbd.Valori.Clear()
'					chIsPvrt.ValuesBl.Clear()
'					For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'						If Double.IsNaN(TWA) Then
'							chIsStbd.ValuesBl.Add(PrevStbd)
'							chIsPvrt.ValuesBl.Add(False)
'						Else
'							Dim IsStbd As Boolean = TWA > 0
'							chIsStbd.ValuesBl.Add(IsStbd)
'							'chIsStbd.Valori.Add(If(IsStbd, 1, -1))
'							chIsPvrt.ValuesBl.Add(Not IsStbd = PrevStbd)
'							PrevStbd = IsStbd
'						End If

'					Next
'				End If
'      Case eChannels.eRdrRakeEffective
'        Dim chPortCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePortCantAngle).LongNameNoSpaces)
'        Dim chEffPortCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePortCantAngleEffective).LongNameNoSpaces)
'        Dim chStbdCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eStbdCantAngle).LongNameNoSpaces)
'        Dim chEffStbdCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eStbdCantAngleEffective).LongNameNoSpaces)
'        Dim chRoll As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRoll).LongNameNoSpaces)
'        Dim EffCant As Boolean = Not chRoll Is Nothing AndAlso Not chStbdCant Is Nothing AndAlso Not chPortCant Is Nothing
'        If EffCant Then
'          If chEffPortCant Is Nothing Then Stop
'          If chEffStbdCant Is Nothing Then Stop
'        End If

'        Dim chRdrRk As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRake).LongNameNoSpaces)
'        Dim chRdrRkEff As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRakeEffective).LongNameNoSpaces)
'        Dim chTrim As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eTRIM).LongNameNoSpaces)
'        For i As Long = 0 To chRdrRk.Valori.Count - 1
'          chRdrRkEff.Valori.Add(chRdrRk.Valori(i) + chTrim.Valori(i))
'          If EffCant Then
'            chEffPortCant.Valori.Add(chPortCant.Valori(i) + chRoll.Valori(i))
'            chEffStbdCant.Valori.Add(chStbdCant.Valori(i) + chRoll.Valori(i))
'          End If
'        Next
'      Case eChannels.eAWS

'      Case eChannels.eIsPavarot
'        'non fa nulla altrimenti lo farebbe due volte...fatto con eIsStbd
'      Case eChannels.eLwdCantAngle
'      Case eChannels.eTopTwist
'        AggiornaCanaliWindwardLeeward()
'      Case eChannels.eWwdCantAngle, eChannels.eLwdFoilOutFlap2Angle, eChannels.eLwdFoilOutFlap1Angle, eChannels.eLwdFoilInFlap1Angle, eChannels.eLwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap1Angle, eChannels.eWwdFoilOutFlap1Angle, eChannels.eWwdFoilOutFlap2Angle
'        ' già fatto con eLwdCantAngle
'      Case eChannels.eWwdCantAngleEffective, eChannels.eLwdCantAngleEffective
'        'Stop
'        ' già fatto con eLwdCantAngle
'      Case Else
'        'Stop
'        'Case eChannels.eIsStbd
'        '  Dim PrevStbd As Boolean = True
'        '  If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'        '    Dim chIsStbd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces)
'        '    Dim chIsPvrt As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces)
'        '    chIsStbd.ValuesBl.Clear()
'        '    chIsPvrt.ValuesBl.Clear()
'        '    For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'        '      Dim IsStbd As Boolean = TWA > 0
'        '      chIsStbd.ValuesBl.Add(IsStbd)
'        '      chIsPvrt.ValuesBl.Add(Not IsStbd = PrevStbd)
'        '      PrevStbd = IsStbd
'        '    Next
'        '  End If
'        'Case eChannels.eIsPavarot
'        '  'non fa nulla altrimenti lo farebbe due volte...fatto con eIsStbd
'        'Case eChannels.eBsTwsRatio, eChannels.eVmgTwsRatio
'        '  'non fa nulla altrimenti lo farebbe due volte...fatto con eChannels.eVMG
'        'Case eChannels.eLwdCantAngle
'        '  AggiornaCanaliWindwardLeeward()
'        'Case eChannels.eWwdCantAngle, eChannels.eLwdFoilOutFlap2Angle, eChannels.eLwdFoilOutFlap1Angle, eChannels.eLwdFoilInFlap1Angle, eChannels.eLwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap1Angle, eChannels.eWwdFoilOutFlap1Angle, eChannels.eWwdFoilOutFlap2Angle, eChannels.eWwdCantAngleEffective, eChannels.eLwdCantAngleEffective
'        '  ' già fatto con eLwdCantAngle
'        'Case Else
'        '  'Stop
'    End Select
'  End Sub

'  Private Sub MathsCanaliSQLite(Canale As clsChannel2020)
'    Select Case Canale.CanaleChiave
'      Case eChannels.eVMG
'				If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'					If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'						Dim chSow As clsChannel2020 = pCanaliChiave(eChannels.eSOW)
'						Dim chTwa As clsChannel2020 = pCanaliChiave(eChannels.eTWA)
'						Dim chTws As clsChannel2020 = pCanaliChiave(eChannels.eTWS)
'						Dim chBsTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBsTwsRatio).LongNameNoSpaces)
'						Dim chVmgTwsR As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eVmgTwsRatio).LongNameNoSpaces)
'						Dim TwsOk As Boolean = chSow.CaricaCanale
'						Canale.Valori.Clear()
'						chBsTwsR.Valori.Clear()
'						chVmgTwsR.Valori.Clear()
'						For i As Long = 0 To chSow.Valori.Count - 1
'							Canale.Valori.Add(chSow.Valori(i) * System.Math.Abs(System.Math.Cos(Radians(chTwa.Valori(i)))))
'							If Not chTws Is Nothing Then
'								chBsTwsR.Valori.Add(chSow.Valori(i) / chTws.Valori(i) * 100)
'								chVmgTwsR.Valori.Add(Canale.Valori(i) / chTws.Valori(i) * 100)
'							End If
'						Next
'					End If
'				End If
'      Case eChannels.eVMGp
'        If Not Targets Is Nothing Then
'          Dim BS As clsPolare2019CanaleValori = Targets.GetPolare("bs")
'          If Not BS Is Nothing Then
'            If pCanaliChiave(eChannels.eTWS).CaricaCanale Then
'              If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'								If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'									Dim chVMGp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eVMGp).LongNameNoSpaces)
'									Dim chBSPp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBSPp).LongNameNoSpaces)
'									Dim chBSTp As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eBSTp).LongNameNoSpaces)
'									Dim chTWAd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eTWAd).LongNameNoSpaces)

'									For i As Long = 0 To pCanaliChiave(eChannels.eTWS).Valori.Count - 1
'										Dim TWS As Double = pCanaliChiave(eChannels.eTWS).Valori(i)
'										Dim TWA As Double = System.Math.Abs(System.Math.Cos(Radians(pCanaliChiave(eChannels.eTWA).Valori(i))))
'										Dim SOW As Double = pCanaliChiave(eChannels.eSOW).Valori(i)
'										Dim VMG As Double = SOW * System.Math.Abs(System.Math.Cos(Radians(TWA)))
'										Dim BSp As Double = BS.Valore(TWS, TWA)
'										Dim BSt As Double = IIf(TWA > 90, BS.ValoreTargetDn(TWS), BS.ValoreTargetUp(TWS))
'										Dim TWAt As Double = IIf(TWA > 90, BS.ValoreTargetDnReferece(TWS), BS.ValoreTargetUpReferece(TWS))
'										Dim VMGt As Double = BSt * System.Math.Abs(System.Math.Cos(Radians(TWAt)))
'										Dim BSTp As Double = SOW / BSt
'										Dim BSPp As Double = SOW / BSPp
'										Dim TWAd As Double = TWA - TWAt
'										Dim VMGp As Double = TWA - TWAt
'										chVMGp.Valori.Add(VMGp)
'										chBSPp.Valori.Add(BSPp)
'										chBSTp.Valori.Add(BSTp)
'										chTWAd.Valori.Add(TWAd)
'									Next
'								End If
'							End If
'            End If
'          End If
'        End If
'      Case eChannels.eBSPp
'        ' fatto con eVMGp
'      Case eChannels.eBSTp
'        ' fatto con eVMGp
'      Case eChannels.eTWAd
'        ' fatto con eVMGp
'      Case eChannels.ePRT
'        For i As Long = 0 To pCanaliChiave(eChannels.ePRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.ePRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eYRT
'        For i As Long = 0 To pCanaliChiave(eChannels.eYRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eYRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.eYRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eRRT
'        For i As Long = 0 To pCanaliChiave(eChannels.eRRT).Valori.Count - 1
'          CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRRT).LongNameNoSpaces).Valori(i) = pCanaliChiave(eChannels.eRRT).Valori(i) / System.Math.PI * 360
'        Next
'      Case eChannels.eIsStbd
'        Dim PrevStbd As Boolean = True
'        If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'          Dim chIsStbd As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces)
'          Dim chIsPvrt As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces)
'          chIsStbd.ValuesBl.Clear()
'          chIsPvrt.ValuesBl.Clear()
'          For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'            Dim IsStbd As Boolean = TWA > 0
'            chIsStbd.ValuesBl.Add(IsStbd)
'            chIsPvrt.ValuesBl.Add(Not IsStbd = PrevStbd)
'            PrevStbd = IsStbd
'          Next
'          'CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces).ValuesBl.Clear()
'          'CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces).ValuesBl.Clear()
'          ''pCanaliChiave(eChannels.eIsPavarot).ValuesBl = New List(Of Boolean)
'          'For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'          '  Dim IsStbd As Boolean = TWA > 0
'          '  CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsStbd).LongNameNoSpaces).ValuesBl.Add(IsStbd)
'          '  CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eIsPavarot).LongNameNoSpaces).ValuesBl.Add(Not IsStbd = PrevStbd)
'          '  PrevStbd = IsStbd
'          'Next
'        End If
'      Case eChannels.eRdrRakeEffective
'        Dim chPortCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePortCantAngle).LongNameNoSpaces)
'        Dim chPortCantEff As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.ePortCantAngleEffective).LongNameNoSpaces)
'        Dim chStbdCant As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eStbdCantAngle).LongNameNoSpaces)
'        Dim chStbdCantEff As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eStbdCantAngleEffective).LongNameNoSpaces)
'        Dim chRoll As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRoll).LongNameNoSpaces)

'        Dim chRdrRk As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRake).LongNameNoSpaces)
'        Dim chRdrRkEff As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRakeEffective).LongNameNoSpaces)
'				Dim chTrim As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eTRIM).LongNameNoSpaces)
'				For i As Long = 0 To chRdrRk.Valori.Count - 1
'					chRdrRkEff.Valori.Add(chRdrRk.Valori(i) + chTrim.Valori(i))
'				Next
'			Case eChannels.eAWS
'				'Dim chAWS As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eAWS).LongNameNoSpaces)
'				'Dim chRdrRk As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRake).LongNameNoSpaces)
'				'Dim chRdrRkEff As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eRdrRakeEffective).LongNameNoSpaces)
'				'Dim chTrim As clsChannel2020 = CanaleDaNomeNoSpaces(CanaliChiave(eChannels.eTRIM).LongNameNoSpaces)
'				'For i As Long = 0 To chAWS.Valori.Count - 1
'				'  chAWS.Valori(i) = chAWS.Valori(i) / 1852 * 3600
'				'  chRdrRkEff.Valori.Add(chRdrRk.Valori(i) + chTrim.Valori(i))
'				'Next
'			Case eChannels.eIsPavarot
'        'non fa nulla altrimenti lo farebbe due volte...fatto con eIsStbd
'      Case eChannels.eLwdCantAngle
'        AggiornaCanaliWindwardLeeward()
'      Case eChannels.eWwdCantAngle, eChannels.eLwdFoilOutFlap2Angle, eChannels.eLwdFoilOutFlap1Angle, eChannels.eLwdFoilInFlap1Angle, eChannels.eLwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap1Angle, eChannels.eWwdFoilOutFlap1Angle, eChannels.eWwdFoilOutFlap2Angle
'        ' già fatto con eLwdCantAngle
'      Case eChannels.eWwdCantAngleEffective, eChannels.eLwdCantAngleEffective
'        'Stop
'        ' già fatto con eLwdCantAngle
'      Case Else
'        'Stop
'    End Select
'  End Sub

'  'Private Sub MathsCanaliSQLite(Canale As clsChannel2020)
'  '	Select Case Canale.CanaleChiave
'  '		Case eChannels.eVMG
'  '			If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'  '				If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'  '					For i As Long = 0 To pCanaliChiave(eChannels.eSOW).Valori.Count - 1
'  '						pCanaliChiave(eChannels.eVMG).Valori.Add(pCanaliChiave(eChannels.eSOW).Valori(i) * System.Math.Abs(System.Math.Cos(Radians(pCanaliChiave(eChannels.eTWA).Valori(i)))))
'  '					Next
'  '				End If
'  '			End If

'  '		Case eChannels.eVMGp
'  '			If Not Polari Is Nothing Then
'  '				Dim BS As clsTabella = Polari.GetTabella("bs")
'  '				If Not BS Is Nothing Then
'  '					If pCanaliChiave(eChannels.eTWS).CaricaCanale Then
'  '						If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'  '							If pCanaliChiave(eChannels.eSOW).CaricaCanale Then
'  '								For i As Long = 0 To pCanaliChiave(eChannels.eTWS).Valori.Count - 1
'  '									Dim TWS As Double = pCanaliChiave(eChannels.eTWS).Valori(i)
'  '									Dim TWA As Double = System.Math.Abs(System.Math.Cos(Radians(pCanaliChiave(eChannels.eTWA).Valori(i))))
'  '									Dim SOW As Double = pCanaliChiave(eChannels.eSOW).Valori(i)
'  '									Dim VMG As Double = SOW * System.Math.Abs(System.Math.Cos(Radians(TWA)))
'  '									Dim BSp As Double = BS.Valore(TWS, TWA)
'  '									Dim BSt As Double = BS.ValoreTarget(TWS, TWA > 90)
'  '                   Dim TWAt As Double = Polari.RigaTgtUp.Valore(TWS)
'  '                   Dim VMGt As Double = BSt * System.Math.Abs(System.Math.Cos(Radians(TWAt)))
'  '									Dim BSTp As Double = SOW / BSt
'  '									Dim BSPp As Double = SOW / BSPp
'  '									Dim TWAd As Double = TWA - TWAt
'  '									Dim VMGp As Double = TWA - TWAt
'  '									pCanaliChiave(eChannels.eVMGp).Valori.Add(VMGp)
'  '									pCanaliChiave(eChannels.eBSPp).Valori.Add(BSPp)
'  '									pCanaliChiave(eChannels.eBSTp).Valori.Add(BSTp)
'  '									pCanaliChiave(eChannels.eTWAd).Valori.Add(TWAd)
'  '								Next
'  '							End If
'  '						End If
'  '					End If
'  '				End If
'  '			End If
'  '		Case eChannels.eBSPp
'  '			' fatto con eVMGp
'  '		Case eChannels.eBSTp
'  '			' fatto con eVMGp
'  '		Case eChannels.eTWAd
'  '			' fatto con eVMGp
'  '		Case eChannels.ePRT
'  '			For i As Long = 0 To pCanaliChiave(eChannels.ePRT).Valori.Count - 1
'  '				pCanaliChiave(eChannels.ePRT).Valori(i) = pCanaliChiave(eChannels.ePRT).Valori(i) / System.Math.PI * 360
'  '			Next
'  '		Case eChannels.eYRT
'  '			For i As Long = 0 To pCanaliChiave(eChannels.eYRT).Valori.Count - 1
'  '				pCanaliChiave(eChannels.eYRT).Valori(i) = pCanaliChiave(eChannels.eYRT).Valori(i) / System.Math.PI * 360
'  '			Next
'  '		Case eChannels.eRRT
'  '			For i As Long = 0 To pCanaliChiave(eChannels.eRRT).Valori.Count - 1
'  '				pCanaliChiave(eChannels.eRRT).Valori(i) = pCanaliChiave(eChannels.eRRT).Valori(i) / System.Math.PI * 360
'  '			Next
'  '		Case eChannels.eIsStbd
'  '			Dim PrevStbd As Boolean = True
'  '			If pCanaliChiave(eChannels.eTWA).CaricaCanale Then
'  '				pCanaliChiave(eChannels.eIsStbd).ValuesBl = New List(Of Boolean)
'  '				pCanaliChiave(eChannels.eIsPavarot).ValuesBl = New List(Of Boolean)
'  '				For Each TWA As Double In pCanaliChiave(eChannels.eTWA).Values
'  '					Dim IsStbd As Boolean = TWA > 0
'  '					pCanaliChiave(eChannels.eIsStbd).ValuesBl.Add(IsStbd)
'  '					pCanaliChiave(eChannels.eIsPavarot).ValuesBl.Add(Not IsStbd = PrevStbd)
'  '					PrevStbd = IsStbd
'  '				Next
'  '			End If
'  '		Case eChannels.eAWS
'  '			For i As Long = 0 To pCanaliChiave(eChannels.eAWS).Valori.Count - 1
'  '				pCanaliChiave(eChannels.eAWS).Valori(i) = pCanaliChiave(eChannels.eAWS).Valori(i) / 1852 * 3600
'  '			Next
'  '		Case eChannels.eIsPavarot
'  '			'non fa nulla altrimenti lo farebbe due volte...fatto con eIsStbd

'  '		Case eChannels.eLwdCantAngle
'  '			AggiornaCanaliWindwardLeeward()
'  '		Case eChannels.eWwdCantAngle, eChannels.eLwdFoilOutFlap2Angle, eChannels.eLwdFoilOutFlap1Angle, eChannels.eLwdFoilInFlap1Angle, eChannels.eLwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap2Angle, eChannels.eWwdFoilInFlap1Angle, eChannels.eWwdFoilOutFlap1Angle, eChannels.eWwdFoilOutFlap2Angle
'  '			' già fatto con eLwdCantAngle
'  '		Case Else
'  '			'Stop
'  '	End Select
'  'End Sub



'  Private Sub AggiornaCanaliWindwardLeeward()

'    'va gestito quando i canali base non ci sono...
'    Dim CanaleTack As clsChannel2020 = CanaleTackDefault()

'    Dim StbdCant As clsChannel2020 = pCanaliChiave(eChannels.eStbdCantAngle)
'    Dim PortCant As clsChannel2020 = pCanaliChiave(eChannels.ePortCantAngle)
'    Dim EffStbdCant As clsChannel2020 = pCanaliChiave(eChannels.eStbdCantAngleEffective)
'    Dim EffPortCant As clsChannel2020 = pCanaliChiave(eChannels.ePortCantAngleEffective)
'    Dim SFI1 As clsChannel2020 = pCanaliChiave(eChannels.eStbdFoilInFlap1Angle)
'    Dim SFI2 As clsChannel2020 = pCanaliChiave(eChannels.eStbdFoilInFlap2Angle)
'    Dim SFO1 As clsChannel2020 = pCanaliChiave(eChannels.eStbdFoilOutFlap1Angle)
'    Dim SFO2 As clsChannel2020 = pCanaliChiave(eChannels.eStbdFoilOutFlap2Angle)
'    Dim PFI1 As clsChannel2020 = pCanaliChiave(eChannels.ePortFoilInFlap1Angle)
'    Dim PFI2 As clsChannel2020 = pCanaliChiave(eChannels.ePortFoilInFlap2Angle)
'    Dim PFO1 As clsChannel2020 = pCanaliChiave(eChannels.ePortFoilOutFlap1Angle)
'    Dim PFO2 As clsChannel2020 = pCanaliChiave(eChannels.ePortFoilOutFlap2Angle)

'    Dim LCA As clsChannel2020 = pCanaliChiave(eChannels.eLwdCantAngle)
'    Dim WCA As clsChannel2020 = pCanaliChiave(eChannels.eWwdCantAngle)
'    Dim LCAE As clsChannel2020 = pCanaliChiave(eChannels.eLwdCantAngleEffective)
'    Dim WCAE As clsChannel2020 = pCanaliChiave(eChannels.eWwdCantAngleEffective)
'    Dim LFI1 As clsChannel2020 = pCanaliChiave(eChannels.eLwdFoilInFlap1Angle)
'    Dim LFI2 As clsChannel2020 = pCanaliChiave(eChannels.eLwdFoilInFlap2Angle)
'    Dim LFO1 As clsChannel2020 = pCanaliChiave(eChannels.eLwdFoilOutFlap1Angle)
'    Dim LFO2 As clsChannel2020 = pCanaliChiave(eChannels.eLwdFoilOutFlap2Angle)
'    Dim WFI1 As clsChannel2020 = pCanaliChiave(eChannels.eWwdFoilInFlap1Angle)
'    Dim WFI2 As clsChannel2020 = pCanaliChiave(eChannels.eWwdFoilInFlap2Angle)
'    Dim WFO1 As clsChannel2020 = pCanaliChiave(eChannels.eWwdFoilOutFlap1Angle)
'    Dim WFO2 As clsChannel2020 = pCanaliChiave(eChannels.eWwdFoilOutFlap2Angle)

'    Dim PortV1 As clsChannel2020 = pCanaliChiave(eChannels.ePortV1)
'    Dim PortD1 As clsChannel2020 = pCanaliChiave(eChannels.ePortD1)
'    Dim PortRunner As clsChannel2020 = pCanaliChiave(eChannels.ePortRunner)
'    Dim PortJibSheet As clsChannel2020 = pCanaliChiave(eChannels.ePortJibSheet)
'    Dim PortJibUpDn As clsChannel2020 = pCanaliChiave(eChannels.ePortJibUpDn)

'    Dim StbdV1 As clsChannel2020 = pCanaliChiave(eChannels.eStbdV1)
'    Dim StbdD1 As clsChannel2020 = pCanaliChiave(eChannels.eStbdD1)
'    Dim StbdRunner As clsChannel2020 = pCanaliChiave(eChannels.eStbdRunner)
'    Dim StbdJibSheet As clsChannel2020 = pCanaliChiave(eChannels.eStbdJibSheet)
'    Dim StbdJibUpDn As clsChannel2020 = pCanaliChiave(eChannels.eStbdJibUpDn)

'    Dim LwdV1 As clsChannel2020 = pCanaliChiave(eChannels.eLwdV1)
'    Dim LwdD1 As clsChannel2020 = pCanaliChiave(eChannels.eLwdD1)
'    Dim LwdRunner As clsChannel2020 = pCanaliChiave(eChannels.eLwdRunner)
'    Dim LwdJibSheet As clsChannel2020 = pCanaliChiave(eChannels.eLwdJibSheet)
'    Dim LwdJibUpDn As clsChannel2020 = pCanaliChiave(eChannels.eLwdJibUpDn)

'    Dim WwdV1 As clsChannel2020 = pCanaliChiave(eChannels.eWwdV1)
'    Dim WwdD1 As clsChannel2020 = pCanaliChiave(eChannels.eWwdD1)
'    Dim WwdRunner As clsChannel2020 = pCanaliChiave(eChannels.eWwdRunner)
'    Dim WwdJibSheet As clsChannel2020 = pCanaliChiave(eChannels.eWwdJibSheet)
'    Dim WwdJibUpDn As clsChannel2020 = pCanaliChiave(eChannels.eWwdJibUpDn)

'    Dim SpannerCounter As clsChannel2020 = pCanaliChiave(eChannels.eSpannerCounter)
'    Dim TopCtrlArmCounter As clsChannel2020 = pCanaliChiave(eChannels.eTopCtrlArmCounter)
'    Dim TravellerCounter As clsChannel2020 = pCanaliChiave(eChannels.eTravellerCounter)

'    Dim Spanner As clsChannel2020 = pCanaliChiave(eChannels.eSpanner)
'    Dim TopCtrlArm As clsChannel2020 = pCanaliChiave(eChannels.eTopCtrlArm)
'    Dim Traveller As clsChannel2020 = pCanaliChiave(eChannels.eTraveller)
'    Dim TopTwist As clsChannel2020 = pCanaliChiave(eChannels.eTopTwist)


'    If EffStbdCant Is Nothing Then EffStbdCant = StbdCant
'    If EffPortCant Is Nothing Then EffPortCant = PortCant
'    If SFI2 Is Nothing Then SFI2 = SFI1
'    If SFO2 Is Nothing Then SFO2 = SFO1
'    If PFI2 Is Nothing Then PFI2 = PFI1
'    If PFO2 Is Nothing Then PFO2 = PFO1

'    If PFO2 Is Nothing Then PFO2 = PFO1
'    If PFO2 Is Nothing Then PFO2 = PFO1
'    If PFO2 Is Nothing Then PFO2 = PFO1
'    'If TopTwist Is Nothing Then TopTwist = SpannerCounter
'    For i As Long = 0 To CanaleTack.Valori.Count - 1
'      If CanaleTack.StbdTack(i) Then 'Starboard
'        ' leeward
'        AggiungiValore(PortCant, LCA, i)
'        AggiungiValore(EffPortCant, LCAE, i)
'        AggiungiValore(PFI1, LFI1, i)
'        AggiungiValore(PFI2, LFI2, i)
'        AggiungiValore(PFO1, LFO1, i)
'        AggiungiValore(PFO2, LFO2, i)

'        AggiungiValore(PortV1, LwdV1, i)
'        AggiungiValore(PortD1, LwdV1, i)
'        AggiungiValore(PortRunner, LwdRunner, i)
'        AggiungiValore(PortJibSheet, LwdJibSheet, i)
'        AggiungiValore(PortJibUpDn, LwdJibUpDn, i)
'        ' windward
'        AggiungiValore(StbdCant, WCA, i)
'        AggiungiValore(EffStbdCant, WCAE, i)
'        AggiungiValore(SFI1, WFI1, i)
'        AggiungiValore(SFI2, WFI2, i)
'        AggiungiValore(SFO1, WFO1, i)
'        AggiungiValore(SFO2, WFO2, i)

'        AggiungiValore(StbdV1, WwdV1, i)
'        AggiungiValore(StbdD1, WwdV1, i)
'        AggiungiValore(StbdRunner, WwdRunner, i)
'        AggiungiValore(StbdJibSheet, WwdJibSheet, i)
'        AggiungiValore(StbdJibUpDn, WwdJibUpDn, i)

'        AggiungiValore(SpannerCounter, Spanner, 1, i)
'        AggiungiValore(TopCtrlArmCounter, TopCtrlArm, 1, i)
'        AggiungiValore(TravellerCounter, Traveller, 1, i)

'      Else
'        ' leeward
'        AggiungiValore(StbdCant, LCA, i)
'        AggiungiValore(EffStbdCant, LCAE, i)
'        AggiungiValore(SFI1, LFI1, i)
'        AggiungiValore(SFI2, LFI2, i)
'        AggiungiValore(SFO1, LFO1, i)
'        AggiungiValore(SFO2, LFO2, i)

'        AggiungiValore(StbdV1, LwdV1, i)
'        AggiungiValore(StbdD1, LwdV1, i)
'        AggiungiValore(StbdRunner, LwdRunner, i)
'        AggiungiValore(StbdJibSheet, LwdJibSheet, i)
'        AggiungiValore(StbdJibUpDn, LwdJibUpDn, i)
'        ' windward
'        AggiungiValore(PortCant, WCA, i)
'        AggiungiValore(EffPortCant, WCAE, i)
'        AggiungiValore(PFI1, WFI1, i)
'        AggiungiValore(PFI2, WFI2, i)
'        AggiungiValore(PFO1, WFO1, i)
'        AggiungiValore(PFO2, WFO2, i)

'        AggiungiValore(PortV1, WwdV1, i)
'        AggiungiValore(PortD1, WwdV1, i)
'        AggiungiValore(PortRunner, WwdRunner, i)
'        AggiungiValore(PortJibSheet, WwdJibSheet, i)
'        AggiungiValore(PortJibUpDn, WwdJibUpDn, i)

'        AggiungiValore(SpannerCounter, Spanner, -1, i)
'        AggiungiValore(TopCtrlArmCounter, TopCtrlArm, -1, i)
'        AggiungiValore(TravellerCounter, Traveller, -1, i)
'      End If
'      If TopCtrlArm.Valori.Count > 0 Then
'        TopTwist.Valori.Add(Spanner.Valori(i) + TopCtrlArm.Valori(i) - Traveller.Valori(i))
'      End If


'    Next
'    'For i As Long = 0 To CanaleTack.Valori.Count - 1
'    '	If CanaleTack.StbdTack(i) Then
'    '      ' leeward
'    '      If Not PCA Is Nothing AndAlso PCA.Valori.Count > 0 Then
'    '        LCA.Valori.Add(PCA.Valori(i))
'    '        LCAE.Valori.Add(PCAE.Valori(i))
'    '      End If
'    '      If Not PFI1 Is Nothing AndAlso PFI1.Valori.Count > 0 Then
'    '        LFI1.Valori.Add(PFI1.Valori(i))
'    '        LFI2.Valori.Add(PFI2.Valori(i))
'    '      End If
'    '      If Not PFO1 Is Nothing AndAlso PFO1.Valori.Count > 0 Then
'    '        LFO1.Valori.Add(PFO1.Valori(i))
'    '        LFO2.Valori.Add(PFO2.Valori(i))
'    '      End If
'    '      ' windward
'    '      If Not SCA Is Nothing AndAlso SCA.Valori.Count > 0 Then
'    '        WCA.Valori.Add(SCA.Valori(i))
'    '        WCAE.Valori.Add(SCAE.Valori(i))
'    '      End If

'    '      If Not SFI1 Is Nothing AndAlso SFI1.Valori.Count > 0 Then
'    '        WFI1.Valori.Add(SFI1.Valori(i))
'    '        WFI2.Valori.Add(SFI2.Valori(i))
'    '      End If
'    '      If Not SFO1 Is Nothing AndAlso SFO1.Valori.Count > 0 Then
'    '        WFO1.Valori.Add(SFO1.Valori(i))
'    '        WFO2.Valori.Add(SFO2.Valori(i))
'    '      End If

'    '    Else
'    '      ' leeward
'    '      If Not SCA Is Nothing AndAlso SCA.Valori.Count > 0 Then
'    '        LCA.Valori.Add(SCA.Valori(i))
'    '        LCAE.Valori.Add(SCAE.Valori(i))
'    '      End If
'    '      If Not SFI1 Is Nothing AndAlso SFI1.Valori.Count > 0 Then
'    '        LFI1.Valori.Add(SFI1.Valori(i))
'    '        LFI2.Valori.Add(SFI2.Valori(i))
'    '      End If
'    '      If Not SFO1 Is Nothing AndAlso SFO1.Valori.Count > 0 Then
'    '        LFO1.Valori.Add(SFO1.Valori(i))
'    '        LFO2.Valori.Add(SFO2.Valori(i))
'    '      End If

'    '      ' windward
'    '      If Not PCA Is Nothing AndAlso PCA.Valori.Count > 0 Then
'    '        WCA.Valori.Add(PCA.Valori(i))
'    '        WCAE.Valori.Add(PCAE.Valori(i))
'    '      End If
'    '      If Not PFI1 Is Nothing AndAlso PFI1.Valori.Count > 0 Then
'    '        WFI1.Valori.Add(PFI1.Valori(i))
'    '        WFI2.Valori.Add(PFI2.Valori(i))
'    '      End If
'    '      If Not PFO1 Is Nothing AndAlso PFO1.Valori.Count > 0 Then
'    '        WFO1.Valori.Add(PFO1.Valori(i))
'    '        WFO2.Valori.Add(PFO2.Valori(i))
'    '      End If
'    '    End If
'    'Next
'  End Sub


'  Private Sub AggiungiValore(CanaleSource As clsChannel2020, ByRef CanaleDestinazione As clsChannel2020, Indice As Integer)
'    If Not CanaleSource Is Nothing AndAlso CanaleSource.Valori.Count > 0 Then
'      CanaleDestinazione.Valori.Add(CanaleSource.Valori(Indice))
'    Else
'      CanaleDestinazione.Valori.Add(0)
'    End If
'  End Sub

'  Private Sub AggiungiValore(CanaleSource As clsChannel2020, ByRef CanaleDestinazione As clsChannel2020, Moltiplicatore As Integer, Indice As Integer)
'    If Not CanaleSource Is Nothing AndAlso CanaleSource.Valori.Count > 0 Then
'      CanaleDestinazione.Valori.Add(CanaleSource.Valori(Indice) * Moltiplicatore)
'      'Else
'      '  CanaleDestinazione.Valori.Add(0)
'    End If
'  End Sub


'  Private Sub MathsCanaliIMoth(Canale As clsChannel2020)
'		Select Case Canale.CanaleChiave
'			Case eChannels.eSOGms
'				pCanaliChiave(eChannels.eSOGms).Values = New List(Of Nullable(Of Double))(pCanaliChiave(eChannels.eSOG).Values)
'				For i As Long = 0 To pCanaliChiave(eChannels.eSOG).Valori.Count - 1
'					pCanaliChiave(eChannels.eSOG).Valori(i) = pCanaliChiave(eChannels.eSOGms).Valori(i) * 3600 / 1852
'				Next
'			Case eChannels.eVMG, eChannels.eVMGp, eChannels.eBSPp, eChannels.eBSTp, eChannels.eTWAd

'			Case eChannels.eHDG
'				'Stop
'				For i As Long = 0 To pCanaliChiave(eChannels.eHDG).Valori.Count - 1
'					Dim HDG As Double = pCanaliChiave(eChannels.eHDG).Valori(i)
'					If HDG < 0 Then
'						pCanaliChiave(eChannels.eHDG).Valori(i) = 360 + HDG
'					Else
'						pCanaliChiave(eChannels.eHDG).Valori(i) = HDG
'					End If
'				Next
'			Case eChannels.eIsStbd, eChannels.eIsPavarot

'			Case Else
'				Stop
'		End Select
'	End Sub

'	Private Function CercaStringaNellaLista(Stringa As String, Lista As List(Of String)) As Boolean
'		Return Lista.Where(Function(x) x.ToUpper() = Stringa.ToUpper()).Count() > 0
'		''Return Lista.IndexOf(Stringa, StringComparison.OrdinalIgnoreCase) > 0
'	End Function


'	Private Sub VerificaConfigurazioneCanale(ByRef Canale As clsChannel2020)

'		Dim Suffisso As String = pDataProvider.SuffissoFileType
'		Dim IsUser As Boolean = True

'		Canale.IsNuovoCanale = AppConfig.NodoEsistente(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, IsUser)
'		'cerca nel file di configurazione se il canale esiste e ne copia i valori di configurazione
'		Canale.DataType = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "DataType", Canale.DataType, IsUser, False)
'		Canale.LongName = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "LongName", Canale.LongName, IsUser, False)
'		Canale.LongUM = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "LongUM", Canale.LongUM, IsUser, False)
'		Canale.ShortName = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "ShortName", Canale.ShortName, IsUser, False)
'		Canale.ShortUM = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "ShortUM", Canale.ShortUM, IsUser, False)
'		Canale.Importa = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Load", Canale.Importa, IsUser, False)
'    Canale.Decimals = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Decimals", Canale.Decimals, IsUser, False)
'    'If Not Canale.IsMath Then
'    Canale.IsMath = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "IsMath", Canale.IsMath, IsUser, False)

'    'Canale.Tack = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Tack", Canale.Tack, IsUser, False)
'    Dim strHeaders As String = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Headers", Canale.HeadersString, IsUser, False)
'		Canale.Headers = strHeaders.Split(",").ToList
'		Dim CC As clsChannels2020.eCanaliChiave = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "KeyChannel", Canale.CanaleChiave, IsUser, False)
'		Canale.CanaleChiave = CC


'		'Dim LH As List(Of String) = Canale.Headers.ToList
'		'Dim strTMP As String = ""
'		'For Each Elemento As String In LH
'		'	strTMP &= Elemento & ","
'		'Next
'		'strTMP = strTMP.TrimEnd(",")
'		'Dim strHeaders As String = AppConfig.CercaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, Canale.LongNameNoSpaces, "Headers", strTMP, IsUser)
'		'Canale.Headers = strHeaders.Split(",").ToList

'	End Sub

'	Private Function VerificaImpostazioniHeaders(Chiave As String, ValoriDefault As String) As List(Of String)
'		Dim Headers As String = ""
'		'Headers = AppConfig.CercaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "KeyChannelsDefaultHeaders", Chiave, ValoriDefault, False)
'		Headers = AppConfig.CercaValoreInnerText(pDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "KeyChannelsDefaultHeaders", Chiave, ValoriDefault, True, False)
'		If Headers = "" Then Return Nothing
'		Return Headers.Split(",").ToList
'	End Function

'	Private Sub PersonalizzaSuFileType(KeyChannelName As String, ByRef Canale As clsChannel2020)
'		Select Case pDataProvider.FileType
'			Case clsDataProvider2020.eFileType.eFaRo
'      Case clsDataProvider2020.eFileType.eFaRoLR7d5, clsDataProvider2020.eFileType.eFaRoBin, clsDataProvider2020.eFileType.eParquet
'        Select Case KeyChannelName
'					Case "eSOW"
'						Canale.Headers = {"Bs"}.ToList
'					Case "eSOG"
'						Canale.Headers = {"Sog"}.ToList
'					Case "eCOG"
'						Canale.Headers = {"Cog"}.ToList
'					Case "eLat"
'						Canale.Headers = {"LatBow"}.ToList
'					Case "eLng"
'						Canale.Headers = {"LonBow"}.ToList
'					Case "eRideHeight"
'            Canale.Headers = {"RideHeight", "HullRideHeight_ILS_m"}.ToList
'          Case "eMWS"
'						Canale.Headers = {"Aws"}.ToList
'					Case "eMWA"
'						Canale.Headers = {"Awa"}.ToList
'					Case "eTWA"
'						Canale.Headers = {"Twa"}.ToList
'					Case "eTWS"
'						Canale.Headers = {"Tws"}.ToList
'					Case "eTWD"
'						Canale.Headers = {"Twd"}.ToList
'					Case "eAWA"
'						Canale.Headers = {"Awa"}.ToList
'					Case "eAWS"
'						Canale.Headers = {"Aws"}.ToList
'					Case "eHDG"
'						Canale.Headers = {"Hdg"}.ToList
'					Case "eCSE"
'						Canale.Headers = {"Cse"}.ToList
'					Case "eHEEL"
'						Canale.Headers = {"Heel"}.ToList
'					Case "eVMG"
'						'Canale.Headers = {"Vmg"}.ToList
'						Canale.IsMath = True
'					Case "eRdrAngle"
'						Canale.Headers = {"ECC_RudderYawAngle_deg", "RudderYawAngle_ILS"}.ToList
'					Case "eRdrRake"
'						Canale.Headers = {"ECC_RudderRakeAngle_deg", "RudderRakeAngle_ILS"}.ToList
'					Case "eRdrRakeEffective"
'            Canale.IsMath = True
'          Case "ePortCantAngle"
'            Canale.Headers = {"ECC_PortArmCantAngle_deg", "PortArmCantAngle_ILS"}.ToList
'					Case "eStbdCantAngle"
'						Canale.Headers = {"ECC_StbdArmCantAngle_deg", "StbdArmCantAngle_ILS"}.ToList
'					Case "ePortCantAngleEffective"
'            Canale.IsMath = True
'          Case "eStbdCantAngleEffective"
'            Canale.IsMath = True
'          Case "ePortFoilOutFlap2Angle"
'            Canale.Headers = {"ECC_PortFlapOut2Angle_deg", "PortFlapOutAngle_2_ILS"}.ToList
'					Case "ePortFoilOutFlap1Angle"
'						Canale.Headers = {"ECC_PortFlapOut1Angle_deg", "PortFlapOutAngle_1_ILS", "ECC_PortFlapOutAngle_deg", "PortFlapOutAngle_ILS"}.ToList
'					Case "ePortFoilInFlap1Angle"
'						Canale.Headers = {"ECC_PortFlapIn1Angle_deg", "PortFlapInAngle_1_ILS", "ECC_PortFlapInAngle_deg", "PortFlapInAngle_ILS"}.ToList
'					Case "ePortFoilInFlap2Angle"
'						Canale.Headers = {"ECC_PortFlapIn2Angle_deg", "PortFlapInAngle_2_ILS"}.ToList
'					Case "eStbdFoilOutFlap2Angle"
'						Canale.Headers = {"ECC_StbdFlapOut2Angle_deg", "StbdFlapOutAngle_2_ILS"}.ToList
'					Case "eStbdFoilOutFlap1Angle"
'						Canale.Headers = {"ECC_StbdFlapOut1Angle_deg", "StbdFlapOutAngle_1_ILS", "ECC_StbdFlapOutAngle_deg", "StbdFlapOutAngle_ILS"}.ToList
'					Case "eStbdFoilInFlap1Angle"
'						Canale.Headers = {"ECC_StbdFlapIn1Angle_deg", "StbdFlapInAngle_1_ILS", "ECC_StbdFlapInAngle_deg", "StbdFlapInAngle_ILS"}.ToList
'					Case "eStbdFoilInFlap2Angle"
'						Canale.Headers = {"ECC_StbdFlapIn2Angle_deg", "StbdFlapInAngle_2_ILS"}.ToList
'					Case "eLwdCantAngle"
'						Canale.IsMath = True
'					Case "eWwdCantAngle"
'						Canale.IsMath = True
'					Case "eLwdCantAngleEffective"
'						Canale.IsMath = True
'					Case "eWwdCantAngleEffective"
'						Canale.IsMath = True
'					Case "eLwdFoilOutFlap2Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilOutFlap1Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilInFlap1Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilInFlap2Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilOutFlap2Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilOutFlap1Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilInFlap1Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilInFlap2Angle"
'						Canale.IsMath = True
'					Case "eBsTwsRatio"
'						Canale.IsMath = True
'					Case "eVmgTwsRatio"
'						Canale.IsMath = True
'				End Select

'' imposta solo i canali del faro diversi dai settaggi di default
'			Case clsDataProvider2020.eFileType.eSQLite
'				Select Case KeyChannelName
'					Case "eDateTime"
'						Canale.Headers = {"epochtime"}.ToList
'					Case "eDateOnly"
'						Canale.Headers = {"date"}.ToList
'					Case "eTimeOnly"
'						Canale.Headers = {"time"}.ToList
'					Case "eSOW"
'						Canale.Headers = {"bs"}.ToList
'					Case "eHDG"
'            Canale.Headers = {"hdg"}.ToList
'          Case "eCSE"
'            Canale.Headers = {"cse"}.ToList
'          Case "eSOG"
'						Canale.Headers = {"sog"}.ToList
'					Case "eCOG"
'						Canale.Headers = {"cog"}.ToList
'					Case "eLat"
'						Canale.Headers = {"latitude"}.ToList
'					Case "eLng"
'						Canale.Headers = {"longitude"}.ToList
'					Case "eLWY"
'						Canale.Headers = {"lwy"}.ToList
'					Case "eHEEL"
'						Canale.Headers = {"heel"}.ToList
'					Case "eTRIM"
'						Canale.Headers = {"trim"}.ToList
'					Case "eYRT"
'						'Canale.IsMath = True ' arriva in radianti al secondo
'						'Canale.Headers = {"yawrtradsec"}.ToList
'						Canale.Headers = {"yawrt"}.ToList
'					Case "ePRT"
'						'Canale.IsMath = True ' arriva in radianti al secondo
'						'Canale.Headers = {"ptchrtradsec"}.ToList
'						Canale.Headers = {"ptchrt"}.ToList
'					Case "eRRT"
'						'Canale.IsMath = True ' arriva in radianti al secondo
'						'Canale.Headers = {"rollrtradsec"}.ToList
'						Canale.Headers = {"rollrt"}.ToList
'					Case "eRideHeight"
'						Canale.Headers = {"rideheight"}.ToList
'					Case "eMWS"
'						Canale.Headers = {"aws"}.ToList
'					Case "eMWA"
'						Canale.Headers = {"awa"}.ToList
'					Case "eTWA"
'						Canale.Headers = {"twa"}.ToList
'					Case "eTWS"
'						Canale.Headers = {"tws"}.ToList
'					Case "eTWD"
'						Canale.Headers = {"twd"}.ToList
'					Case "eAWA"
'						Canale.Headers = {"awa"}.ToList
'					Case "eAWS"
'						'Canale.IsMath = True ' arriva in metri al secondo
'						Canale.Headers = {"aws"}.ToList
'					Case "eRdrAngle"
'						Canale.Headers = {"rdr"}.ToList
'					Case "eRdrRake"
'						Canale.Headers = {"rdrrk"}.ToList
'					Case "ePortCantAngle"
'						Canale.Headers = {"cantport"}.ToList
'					Case "eStbdCantAngle"
'						Canale.Headers = {"cantstbd"}.ToList
'					Case "eIsStbd"
'						Canale.Headers = {"tack"}.ToList
'					Case "eIsPavarot"
'						Canale.Headers = {"pavarot"}.ToList
'					Case "eVMG"
'						Canale.Headers = {"vmg"}.ToList
'					Case "eVMGp"
'						Canale.Headers = {"vmgp"}.ToList
'					Case "eBSTp"
'						Canale.Headers = {"bstp"}.ToList
'					Case "eBSPp"
'						Canale.Headers = {"bspp"}.ToList
'					Case "eTWAd"
'						Canale.Headers = {"twad"}.ToList
'					Case "eSOGms"
'						Canale.IsMath = False
'						Canale.Headers = {"none"}.ToList
'					Case "eRideHeightPointX"
'						Canale.Headers = {"RideHeightPointX"}.ToList
'					Case "eRideHeightPointY"
'						Canale.Headers = {"RideHeightPointY"}.ToList
'					Case "ePortFoilOutFlap2Angle"
'						Canale.Headers = {"PortFoilOutFlap2Angle"}.ToList
'					Case "ePortFoilOutFlap1Angle"
'						Canale.Headers = {"PortFoilOutFlap1Angle"}.ToList
'					Case "ePortFoilInFlap1Angle"
'						Canale.Headers = {"PortFoilInFlap1Angle"}.ToList
'					Case "ePortFoilInFlap2Angle"
'						Canale.Headers = {"PortFoilInFlap2Angle"}.ToList
'					Case "eStbdFoilInFlap2Angle"
'						Canale.Headers = {"StbdFoilInFlap2Angle"}.ToList
'					Case "eStbdFoilInFlap1Angle"
'						Canale.Headers = {"StbdFoilInFlap1Angle"}.ToList
'					Case "eStbdFoilOutFlap1Angle"
'						Canale.Headers = {"StbdFoilOutFlap1Angle"}.ToList
'					Case "eStbdFoilOutFlap2Angle"
'						Canale.Headers = {"StbdFoilOutFlap2Angle"}.ToList
'					Case "eLwdCantAngle"
'						Canale.IsMath = True
'					Case "eWwdCantAngle"
'						Canale.IsMath = True
'					Case "eLwdFoilOutFlap2Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilOutFlap1Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilInFlap1Angle"
'						Canale.IsMath = True
'					Case "eLwdFoilInFlap2Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilOutFlap2Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilOutFlap1Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilInFlap1Angle"
'						Canale.IsMath = True
'					Case "eWwdFoilInFlap2Angle"
'						Canale.IsMath = True
'          Case "eRdrRakeEffective"
'            'Canale.IsMath = True
'            Canale.Headers = {"RuddeRakeEffective"}.ToList
'          Case "ePortCantAngleEffective"
'            'Canale.IsMath = True
'            Canale.Headers = {"PortCantAngleEffective"}.ToList
'          Case "eStbdCantAngleEffective"
'            'Canale.IsMath = True
'            Canale.Headers = {"StbdCantAngleEffective"}.ToList
'          Case "eLwdCantAngleEffective"
'            Canale.IsMath = True
'          Case "eWwdCantAngleEffective"
'            Canale.IsMath = True
'          Case "eBsTwsRatio"
'            Canale.IsMath = True
'          Case "eVmgTwsRatio"
'            Canale.IsMath = True
'          Case Else
'            'Stop
'        End Select
'			Case Else
'				Stop
'		End Select




'	End Sub


'	Public Sub CaricaCanaliChiaveDiDefault()


'    pCanaleDT.LongName = "Time"
'    pCanaleDT.DataType = clsChannel2020.eDataType.eDateTime

'		Dim strKeyChannel As String() = System.Enum.GetNames(GetType(eChannels))
'		Dim valKeyChannel As Integer() = System.Enum.GetValues(GetType(eChannels))

'    pCanaliChiaveDefault.Clear()
'    pCanaliChiave.Clear()
'    pCanaliMath.Clear()
'    pDizionarioHeadersCanaliChiaveDefault.Clear()

'    'pCanaliChiave = New List(Of clsChannel2020)
'    'pCanaliMath = New List(Of clsChannel2020)

'    For i As Integer = 0 To strKeyChannel.Count - 1
'			Dim aggiungi As Boolean = True
'			Dim KeyChannelName As String = strKeyChannel(i)
'			Dim ChTmp As New clsChannel2020(KeyChannelName)
'			ChTmp.IdCanale = -1
'			ChTmp.IsMath = False
'			ChTmp.DataType = clsChannel2020.eDataType.eLinear
'			ChTmp.CanaleChiave = System.Enum.GetValues(GetType(eChannels))(i)
'      ChTmp.Decimals = 1
'      Select Case KeyChannelName
'				Case "eDateTime"
'					ChTmp.DataType = clsChannel2020.eDataType.eDateTime
'					ChTmp.Headers = {"DT", "DateTime"}.ToList
'					ChTmp.LongName = "DateTime"
'					ChTmp.ShortName = "DT"
'					ChTmp.LongUM = ""
'					ChTmp.ShortUM = ""
'				Case "eDateOnly"
'					ChTmp.DataType = clsChannel2020.eDataType.eDateOnly
'					ChTmp.Headers = {"Date", "SystemTime_Date"}.ToList
'					ChTmp.LongName = "Date"
'					ChTmp.ShortName = "Dt"
'					ChTmp.LongUM = ""
'					ChTmp.ShortUM = ""
'				Case "eTimeOnly"
'					ChTmp.DataType = clsChannel2020.eDataType.eTimeOnly
'					ChTmp.Headers = {"Time", "SystemTime_Local"}.ToList
'					ChTmp.LongName = "Time"
'					ChTmp.ShortName = "tm"
'					ChTmp.LongUM = ""
'					ChTmp.ShortUM = ""
'				Case "eSOW"
'					ChTmp.Headers = {"BS", "SOW"}.ToList
'					ChTmp.LongName = "Speed Over Water"
'					ChTmp.ShortName = "SOW"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'          ChTmp.Decimals = 2
'        Case "eHDG"
'					ChTmp.Headers = {"HDG", "Heading", "Yaw", "Hdg"}.ToList
'					ChTmp.LongName = "Heading"
'					ChTmp.ShortName = "HDG"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e360
'				Case "eCSE"
'					ChTmp.Headers = {"CSE", "Course", "Cse"}.ToList
'					ChTmp.LongName = "Course"
'					ChTmp.ShortName = "CSE"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e360
'				Case "eSOG"
'					ChTmp.Headers = {"SOG"}.ToList
'					ChTmp.LongName = "Speed Over Ground"
'					ChTmp.ShortName = "SOG"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'          ChTmp.Decimals = 2
'        Case "eCOG"
'					ChTmp.Headers = {"COG"}.ToList
'					ChTmp.LongName = "Course Over Ground"
'					ChTmp.ShortName = "COG"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e360
'				Case "eLat"
'					ChTmp.Headers = {"GpsLat", "Latitude", "Lat", "LatBow"}.ToList
'					ChTmp.LongName = "Latitude"
'					ChTmp.ShortName = "Lat"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 8
'        Case "eLng"
'					ChTmp.Headers = {"GpsLon", "Long", "Longitude", "Lon", "LonBow"}.ToList
'					ChTmp.LongName = "Longitude"
'					ChTmp.ShortName = "Lng"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 8
'        Case "eLWY"
'					ChTmp.Headers = {"Lee", "Lwy", "Leeway"}.ToList
'					ChTmp.LongName = "Leeway"
'					ChTmp.ShortName = "Lwy"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.eLinear
'				Case "eHEEL"
'          ChTmp.Headers = {"Hee", "Heel", "Heeling"}.ToList
'          ChTmp.LongName = "Heeling"
'					ChTmp.ShortName = "Heel"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.eLinear
'				Case "eTRIM"
'          ChTmp.Headers = {"Trim", "Trm"}.ToList
'          ChTmp.LongName = "Trim"
'					ChTmp.ShortName = "Trm"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'				Case "eYRT"
'					ChTmp.Headers = {"YawRate", "Yrate", "Yrt", "QuadransBin_HeadingRate_DegSec"}.ToList
'					ChTmp.LongName = "Yaw Rate"
'					ChTmp.ShortName = "Yrt"
'					ChTmp.LongUM = "Deg x Sec"
'					ChTmp.ShortUM = "°/s"
'        Case "ePRT"
'          ChTmp.Headers = {"PitchRate", "Prate", "Prt", "QuadransBin_PitchRate_DegSec"}.ToList
'          ChTmp.LongName = "Pitch Rate"
'          ChTmp.ShortName = "Prt"
'          ChTmp.LongUM = "Deg x Sec"
'          ChTmp.ShortUM = "°/s"
'        Case "eRRT"
'          ChTmp.Headers = {"RollRate", "Rrate", "Rrt", "QuadransBin_RollRate_DegSec"}.ToList
'          ChTmp.LongName = "Roll Rate"
'          ChTmp.ShortName = "Rrt"
'          ChTmp.LongUM = "Deg x Sec"
'          ChTmp.ShortUM = "°/s"
'        Case "eTrimRT"
'          ChTmp.Headers = {"Trimrate", "Trate", "Trt"}.ToList
'          ChTmp.LongName = "Trim Rate"
'          ChTmp.ShortName = "Trt"
'          ChTmp.LongUM = "Deg x Sec"
'					ChTmp.ShortUM = "°/s"
'        Case "eHeelRT"
'          ChTmp.Headers = {"HeelRate", "Hrate", "Hrt"}.ToList
'          ChTmp.LongName = "Heel Rate"
'          ChTmp.ShortName = "Hrt"
'          ChTmp.LongUM = "Deg x Sec"
'					ChTmp.ShortUM = "°/s"
'				Case "eRideHeight"
'					ChTmp.Headers = {"RH", "RdHg", "RideHeight"}.ToList
'					ChTmp.LongName = "Ride Height"
'					ChTmp.ShortName = "RH"
'					ChTmp.LongUM = "Meters"
'					ChTmp.ShortUM = "M"
'				Case "eMWS"
'					ChTmp.Headers = {"MWS", "CMS"}.ToList
'					ChTmp.LongName = "Measured Wind Speed"
'					ChTmp.ShortName = "MWS"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'				Case "eMWA"
'					ChTmp.Headers = {"MWA", "CMA"}.ToList
'					ChTmp.LongName = "Measured Wind Angle"
'					ChTmp.ShortName = "MWA"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e180
'				Case "eTWA"
'					ChTmp.Headers = {"TWA", "TWangle"}.ToList
'					ChTmp.LongName = "True Wind Angle"
'					ChTmp.ShortName = "TWA"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e180
'				Case "eTWS"
'					ChTmp.Headers = {"TWS", "TWspeed"}.ToList
'					ChTmp.LongName = "True Wind Speed"
'					ChTmp.ShortName = "TWS"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'				Case "eTWD"
'					ChTmp.Headers = {"TWD", "TWdir"}.ToList
'					ChTmp.LongName = "True Wind Direction"
'					ChTmp.ShortName = "TWD"
'					ChTmp.DataType = clsChannel2020.eDataType.e360
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 0
'        Case "eAWA"
'					ChTmp.Headers = {"AWA", "AWangle"}.ToList
'					ChTmp.LongName = "Apparent Wind Angle"
'					ChTmp.ShortName = "AWA"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e180
'				Case "eAWS"
'					ChTmp.Headers = {"AWS", "AWspeed"}.ToList
'					ChTmp.LongName = "Apparent Wind Speed"
'					ChTmp.ShortName = "AWS"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'				Case "eRdrAngle"
'					ChTmp.Headers = {"RDR", "RDRPort", "RDP"}.ToList
'					ChTmp.LongName = "Rudder Angle"
'					ChTmp.ShortName = "RDR"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.eTack
'				Case "eRdrRake"
'					ChTmp.Headers = {"RDRK", "RDRPortRake", "RDPR"}.ToList
'					ChTmp.LongName = "Rudder Rake"
'					ChTmp.ShortName = "RRk"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'				Case "eRdrRakeEffective"
'					ChTmp.Headers = {"ElevPitchAngle"}.ToList
'					ChTmp.LongName = "Effective Rudder Rake"
'					ChTmp.ShortName = "EfRRk"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'				Case "eRdrAngle2"
'					ChTmp.Headers = {"RDR2", "RDRStbd", "RDS"}.ToList
'					ChTmp.LongName = "Stbd Rudder Angle"
'					ChTmp.ShortName = "RDS"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.eTack
'				Case "eRdrRake2"
'					ChTmp.Headers = {"RDRK2", "RDRStbdRake", "RDSR"}.ToList
'					ChTmp.LongName = "Stbd Rudder Rake"
'					ChTmp.ShortName = "RRk"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'				Case "ePortCantAngle"
'					ChTmp.Headers = {"PortCantAngle"}.ToList
'					ChTmp.LongName = "Port Arm Cant Angle"
'					ChTmp.ShortName = "PCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "eStbdCantAngle"
'					ChTmp.Headers = {"StbdCantAngle"}.ToList
'					ChTmp.LongName = "Stbd Arm Cant Angle"
'					ChTmp.ShortName = "SCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "ePortCantAngleEffective"
'					ChTmp.Headers = {"EffPortCantAngle"}.ToList
'					ChTmp.LongName = "Eff Port Arm Cant Angle"
'					ChTmp.ShortName = "EfPCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "eStbdCantAngleEffective"
'					ChTmp.Headers = {"EffStbdCantAngle"}.ToList
'					ChTmp.LongName = "Eff Stbd Arm Cant Angle"
'					ChTmp.ShortName = "EfSCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eRideHeightPointX"
'					ChTmp.Headers = {"RideHeightPointX"}.ToList
'					ChTmp.LongName = "RideHeight Point X"
'					ChTmp.ShortName = "RHX"
'					ChTmp.LongUM = "Meters"
'					ChTmp.ShortUM = "M"
'				Case "eRideHeightPointY"
'					ChTmp.Headers = {"RideHeightPointY"}.ToList
'					ChTmp.LongName = "RideHeight Point Y"
'					ChTmp.ShortName = "RHY"
'					ChTmp.LongUM = "Meters"
'					ChTmp.ShortUM = "M"
'				Case "eIsStbd"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"Tack"}.ToList
'					ChTmp.LongName = "Tack"
'					ChTmp.ShortName = "Tk"
'					ChTmp.LongUM = "Y/N"
'					ChTmp.ShortUM = ""
'					ChTmp.DataType = clsChannel2020.eDataType.eBoolean
'				Case "eIsPavarot"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"Pavarot"}.ToList
'					ChTmp.LongName = "IsPavarot"
'					ChTmp.ShortName = "Pv"
'					ChTmp.LongUM = "Y/N"
'					ChTmp.ShortUM = ""
'					ChTmp.DataType = clsChannel2020.eDataType.eBoolean
'				Case "eVMG"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"VMG"}.ToList
'					ChTmp.LongName = "Velocity Made Good"
'					ChTmp.ShortName = "VMG"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'					ChTmp.DataType = clsChannel2020.eDataType.eLinear
'				Case "eVMGp"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"VMGP"}.ToList
'					ChTmp.LongName = "Velocity Made Good Perc"
'					ChTmp.ShortName = "VMG%"
'					ChTmp.LongUM = "Perc"
'					ChTmp.ShortUM = "%"
'					ChTmp.DataType = clsChannel2020.eDataType.ePercentage
'				Case "eBSTp"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"BSTP"}.ToList
'					ChTmp.LongName = "Boat Speed Target Perc"
'					ChTmp.ShortName = "BST%"
'					ChTmp.LongUM = "Perc"
'					ChTmp.ShortUM = "%"
'					ChTmp.DataType = clsChannel2020.eDataType.ePercentage
'				Case "eBSPp"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"BSPP"}.ToList
'					ChTmp.LongName = "Boat Speed Polar Perc"
'					ChTmp.ShortName = "BSP%"
'					ChTmp.LongUM = "Perc"
'					ChTmp.ShortUM = "%"
'					ChTmp.DataType = clsChannel2020.eDataType.ePercentage
'				Case "eTWAd"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"TWAD"}.ToList
'					ChTmp.LongName = "True Wind Angle Delta"
'					ChTmp.ShortName = "TWAd"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					ChTmp.DataType = clsChannel2020.eDataType.e180
'				Case "eSOGms"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"SOGms"}.ToList
'					ChTmp.LongName = "Speed Over Ground MS"
'					ChTmp.ShortName = "SOGms"
'					ChTmp.LongUM = "Knots"
'					ChTmp.ShortUM = "kts"
'					ChTmp.DataType = clsChannel2020.eDataType.eLinear
'          ChTmp.Decimals = 2
'        Case "ePortFoilOutFlap2Angle"
'					ChTmp.Headers = {"PortFoilOutFlap2Angle"}.ToList
'					ChTmp.LongName = "Port Foil Out Flap2 Angle"
'					ChTmp.ShortName = "PFOF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "ePortFoilOutFlap1Angle"
'					ChTmp.Headers = {"PortFoilOutFlap1Angle"}.ToList
'					ChTmp.LongName = "Port Foil Out Flap1 Angle"
'					ChTmp.ShortName = "PFOF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "ePortFoilInFlap1Angle"
'					ChTmp.Headers = {"PortFoilInFlap1Angle"}.ToList
'					ChTmp.LongName = "Port Foil In Flap1 Angle"
'					ChTmp.ShortName = "PFIF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "ePortFoilInFlap2Angle"
'					ChTmp.Headers = {"PortFoilInFlap2Angle"}.ToList
'					ChTmp.LongName = "Port Foil In Flap2 Angle"
'					ChTmp.ShortName = "PFIF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "eStbdFoilOutFlap2Angle"
'					ChTmp.Headers = {"StbdFoilOutFlap2Angle"}.ToList
'					ChTmp.LongName = "Stbd Foil Out Flap2 Angle"
'					ChTmp.ShortName = "SFOF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eStbdFoilOutFlap1Angle"
'					ChTmp.Headers = {"StbdFoilOutFlap1Angle"}.ToList
'					ChTmp.LongName = "Stbd Foil Out Flap1 Angle"
'					ChTmp.ShortName = "SFOF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eStbdFoilInFlap1Angle"
'					ChTmp.Headers = {"StbdFoilInFlap1Angle"}.ToList
'					ChTmp.LongName = "Stbd Foil In Flap1 Angle"
'					ChTmp.ShortName = "SFIF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eStbdFoilInFlap2Angle"
'					ChTmp.Headers = {"StbdFoilInFlap2Angle"}.ToList
'					ChTmp.LongName = "Stbd Foil In Flap2 Angle"
'					ChTmp.ShortName = "SFIF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdCantAngle"
'					ChTmp.Headers = {"LwdArmCantAngle"}.ToList
'					ChTmp.LongName = "Leeward Arm Cant Angle"
'					ChTmp.ShortName = "LwCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "eWwdCantAngle"
'					ChTmp.Headers = {"WwdCantAngle"}.ToList
'					ChTmp.LongName = "Windward Arm Cant Angle"
'					ChTmp.ShortName = "WwCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdCantAngleEffective"
'					ChTmp.Headers = {"EffLwdArmCantAngle"}.ToList
'					ChTmp.LongName = "Eff Lwd Arm Cant Angle"
'					ChTmp.ShortName = "EfLwCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.ePort
'				Case "eWwdCantAngleEffective"
'					ChTmp.Headers = {"EffWwdCantAngle"}.ToList
'					ChTmp.LongName = "Eff Wwd Arm Cant Angle"
'					ChTmp.ShortName = "EfWwCa"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdFoilOutFlap2Angle"
'					ChTmp.Headers = {"LwdFoilOutFlap2Angle"}.ToList
'					ChTmp.LongName = "Leeward Foil Out Flap2 Angle"
'					ChTmp.ShortName = "LwFOF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdFoilOutFlap1Angle"
'					ChTmp.Headers = {"LwdFoilOutFlap1Angle"}.ToList
'					ChTmp.LongName = "Leeward Foil Out Flap1 Angle"
'					ChTmp.ShortName = "LwFOF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdFoilInFlap1Angle"
'					ChTmp.Headers = {"LwdFoilInFlap1Angle"}.ToList
'					ChTmp.LongName = "Leeward Foil In Flap1 Angle"
'					ChTmp.ShortName = "LwFIF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eLwdFoilInFlap2Angle"
'					ChTmp.Headers = {"LwdFoilInFlap2Angle"}.ToList
'					ChTmp.LongName = "Leeward Foil In Flap2 Angle"
'					ChTmp.ShortName = "LwFIF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eWwdFoilOutFlap2Angle"
'					ChTmp.Headers = {"WwdFoilOutFlap2Angle"}.ToList
'					ChTmp.LongName = "Windward Foil Out Flap2 Angle"
'					ChTmp.ShortName = "WwFOF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eWwdFoilOutFlap1Angle"
'					ChTmp.Headers = {"WwdFoilOutFlap1Angle"}.ToList
'					ChTmp.LongName = "Windward Foil Out Flap1 Angle"
'					ChTmp.ShortName = "WwFOF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eWwdFoilInFlap1Angle"
'					ChTmp.Headers = {"WwdFoilInFlap1Angle"}.ToList
'					ChTmp.LongName = "Windward Foil In Flap1 Angle"
'					ChTmp.ShortName = "WwFIF1"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eWwdFoilInFlap2Angle"
'					ChTmp.Headers = {"WwdFoilInFlap2Angle"}.ToList
'					ChTmp.LongName = "Windward Foil In Flap2 Angle"
'					ChTmp.ShortName = "WwFIF2"
'					ChTmp.LongUM = "Degrees"
'					ChTmp.ShortUM = "°"
'					'ChTmp.Tack = clsChannel2020.eTack.eStbd
'				Case "eBsTwsRatio"
'					ChTmp.IsMath = True
'					ChTmp.Headers = {"BsTwsRatio"}.ToList
'					ChTmp.LongName = "Bs Tws Ratio"
'					ChTmp.ShortName = "BTR"
'					ChTmp.LongUM = "Perc"
'					ChTmp.ShortUM = "%"
'          ChTmp.Decimals = 1
'        Case "eVmgTwsRatio"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"VmgTwsRatio"}.ToList
'          ChTmp.LongName = "Vmg Tws Ratio"
'          ChTmp.ShortName = "VTR"
'          ChTmp.LongUM = "Perc"
'          ChTmp.ShortUM = "%"
'          ChTmp.Decimals = 1
'        Case "ePortV1"
'          ChTmp.Headers = {"PortV1Pin_Load"}.ToList
'          ChTmp.LongName = "PortPinV1"
'          ChTmp.ShortName = "V1Port"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eStbdV1"
'          ChTmp.Headers = {"StbdV1Pin_Load"}.ToList
'          ChTmp.LongName = "StbdPinV1"
'          ChTmp.ShortName = "V1Stbd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eLwdV1"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"LwdV1Pin_Load"}.ToList
'          ChTmp.LongName = "LwdPinV1"
'          ChTmp.ShortName = "V1Lwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eWwdV1"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"WwdV1Pin_Load"}.ToList
'          ChTmp.LongName = "WwdPinV1"
'          ChTmp.ShortName = "V1Wwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "ePortD1"
'          ChTmp.Headers = {"PortD1Pin_Load"}.ToList
'          ChTmp.LongName = "PortPinD1"
'          ChTmp.ShortName = "D1Port"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eStbdD1"
'          ChTmp.Headers = {"StbdD1Pin_Load"}.ToList
'          ChTmp.LongName = "StbdPinD1"
'          ChTmp.ShortName = "D1Stbd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eLwdD1"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"LwdD1Pin_Load"}.ToList
'          ChTmp.LongName = "LwdPinD1"
'          ChTmp.ShortName = "D1Lwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eWwdD1"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"WwdD1Pin_Load"}.ToList
'          ChTmp.LongName = "WwdPinD1"
'          ChTmp.ShortName = "D1Wwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "ePortRunner"
'          ChTmp.Headers = {"PortRunnerRam_Load"}.ToList
'          ChTmp.LongName = "PortRunner"
'          ChTmp.ShortName = "RunnerPort"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eStbdRunner"
'          ChTmp.Headers = {"StbdRunnerRam_Load"}.ToList
'          ChTmp.LongName = "StbdRunner"
'          ChTmp.ShortName = "RunnerStbd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eLwdRunner"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"LwdRunnerRam_Load"}.ToList
'          ChTmp.LongName = "LwdRunner"
'          ChTmp.ShortName = "RunnerLwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eWwdRunner"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"WwdRunnerRam_Load"}.ToList
'          ChTmp.LongName = "WwdRunner"
'          ChTmp.ShortName = "RunnerWwd"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "ePortJibSheet"
'          ChTmp.Headers = {"PortJibSheetRam_Load"}.ToList
'          ChTmp.LongName = "PortJibSheet"
'          ChTmp.ShortName = "PortJibSheet"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eStbdJibSheet"
'          ChTmp.Headers = {"StbdJibSheetRam_Load"}.ToList
'          ChTmp.LongName = "StbdJibSheet"
'          ChTmp.ShortName = "StbdJibSheet"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eLwdJibSheet"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"LwdJibSheetRam_Load"}.ToList
'          ChTmp.LongName = "LwdJibSheet"
'          ChTmp.ShortName = "LwdJibSheet"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eWwdJibSheet"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"WwdJibSheetRam_Load"}.ToList
'          ChTmp.LongName = "WwdJibSheet"
'          ChTmp.ShortName = "WwdJibSheet"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "ePortJibUpDn"
'          ChTmp.Headers = {"PortJibUpDnRam_Load"}.ToList
'          ChTmp.LongName = "PortJibUpDn"
'          ChTmp.ShortName = "PortJibUpDn"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eStbdJibUpDn"
'          ChTmp.Headers = {"StbdJibUpDnRam_Load"}.ToList
'          ChTmp.LongName = "StbdJibUpDn"
'          ChTmp.ShortName = "StbdJibUpDn"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eLwdJibUpDn"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"LwdJibUpDnRam_Load"}.ToList
'          ChTmp.LongName = "LwdJibUpDn"
'          ChTmp.ShortName = "LwdJibUpDn"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eWwdJibUpDn"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"WwdJibUpDnRam_Load"}.ToList
'          ChTmp.LongName = "WwdJibUpDn"
'          ChTmp.ShortName = "WwdJibUpDn"
'          ChTmp.LongUM = "Tons"
'          ChTmp.ShortUM = "t"
'          ChTmp.Decimals = 1
'        Case "eSpannerCounter"
'          ChTmp.Headers = {"Spanner_Ang"}.ToList
'          ChTmp.LongName = "Spanner_Ang"
'          ChTmp.ShortName = "Spanner"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eTopCtrlArmCounter"
'          ChTmp.Headers = {"TopCtrlArm_Ang"}.ToList
'          ChTmp.LongName = "TopArm_Ang"
'          ChTmp.ShortName = "TopArm"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eTravellerCounter"
'          ChTmp.Headers = {"Traveller_Ang"}.ToList
'          ChTmp.LongName = "Traveller_Ang"
'          ChTmp.ShortName = "Trav"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eSpanner"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"Spanner_n"}.ToList
'          ChTmp.LongName = "SpannerAngle"
'          ChTmp.ShortName = "Spanner"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eTopCtrlArm"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"TopCtrlArm_n"}.ToList
'          ChTmp.LongName = "TopArmAngle"
'          ChTmp.ShortName = "TopArm"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eTraveller"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"Traveller_n"}.ToList
'          ChTmp.LongName = "TravellerAngle"
'          ChTmp.ShortName = "Trav"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eTopTwist"
'          ChTmp.IsMath = True
'          ChTmp.Headers = {"TopTwist"}.ToList
'          ChTmp.LongName = "TopTwist"
'          ChTmp.ShortName = "HeadTwist"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eRoll"
'          ChTmp.Headers = {"Roll"}.ToList
'          ChTmp.LongName = "Roll"
'          ChTmp.ShortName = "Roll"
'          ChTmp.LongUM = "Degrees"
'          ChTmp.ShortUM = "°"
'          ChTmp.Decimals = 1
'        Case "eNotBasicChannel", "eNone", "eCanaleDT", "eCanaleNVR"
'          aggiungi = False
'				Case Else
'					Dim Nome As String = KeyChannelName
'          Stop
'      End Select
'			If aggiungi Then
'				' personalizza i settaggi secondo il tipo di file
'				PersonalizzaSuFileType(KeyChannelName, ChTmp)
'				' i valori memorizzati nel file XML salvando quelli appena impostati se primo passaggio
'				VerificaConfigurazioneCanale(ChTmp)
'				'ChTmp.Headers = VerificaImpostazioniHeaders(KeyChannelName, ChTmp.HeadersString)
'				pCanaliChiaveDefault.Add(ChTmp) ' il canale in quanto preso dalla lista di quelli chiave hardcoded viene aggiunto alla lista di default
'				pCanaliChiave.Add(Nothing) ' viene aggiunto un canale vuoto alla lista dei canale chiave
'				If ChTmp.IsMath Then
'					pCanaliMath.Add(ChTmp)
'				Else
'					For Each Header In ChTmp.Headers
'						If Not pDizionarioHeadersCanaliChiaveDefault.ContainsKey(Header) Then
'							pDizionarioHeadersCanaliChiaveDefault.Add(Header, valKeyChannel(i))
'						End If
'					Next
'				End If
'			End If
'		Next
'		AppConfig.SalvaFileXML()
'	End Sub

'End Class

'Public Class clsNotValidRow
'  Public Property Start As DateTime
'  Public Property Finish As DateTime
'End Class

'Public Class clsValidRows
'  Public Property CanaleNVR As clsChannel2020
'  'Public Property Suffisso As String
'  'Dim pFileXML As clsSettings
'  Public Property PeriodiNonValidi As New List(Of clsNotValidRow)

'  'Public Sub New(Canale As clsChannel2020)
'  '  pCanaleNVR = Canale
'  'End Sub

'  'Public Property CanaleNVR As clsChannel2020
'  '  Get
'  '    Return pCanaleNVR
'  '  End Get
'  '  Set(value As clsChannel2020)
'  '    pCanaleNVR = value
'  '  End Set
'  'End Property

'  'Public Property PeriodiNonValidi As List(Of clsTimeRange)
'  '  Get
'  '    Return pPeriodiNonValidi
'  '  End Get
'  '  Set(value As List(Of clsTimeRange))
'  '    pPeriodiNonValidi = value
'  '  End Set
'  'End Property



'  Public Sub CaricaNotValidRowsFromXML(FileXML As clsSettings)
'    If FileXML Is Nothing Then Exit Sub
'    pFileXML = FileXML
'    pSuffisso = ObjFiles.PulisciCaratteriBastardi(DataProvider2020.Files.First.Name)
'    Dim NodoPeriodi As Xml.XmlNode = pFileXML.CercaNodo(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, True)
'    If Not NodoPeriodi Is Nothing Then
'      For Each Nodo As Xml.XmlNode In NodoPeriodi
'        Dim Inizio As Double
'        Dim Fine As Double
'        Dim Nome As String = Nodo.Name
'        For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
'          Select Case SottoNodo.Name
'            Case "From"
'              Inizio = CDbl(SottoNodo.InnerText)
'            Case "To"
'              Fine = CDbl(SottoNodo.InnerText)
'          End Select
'        Next
'        Dim TR As New clsTimeRange(DateTime.FromOADate(Inizio), DateTime.FromOADate(Fine))
'        SetNotValidRowsValue(TR, False, False)
'        pPeriodiNonValidi.Add(TR.Clone)
'      Next
'    End If

'  End Sub

'  Public Sub SetNotValidRowsValue(Intervallo As clsTimeRange, SetAsValid As Boolean, SalvaXML As Boolean)
'    'Intervallo.ImpostaRigheAssociate()
'    Dim Inizio As Integer = DataProvider2020.TrovaIndice(Intervallo.Start)
'    Dim Fine As Integer = DataProvider2020.TrovaIndice(Intervallo.Finish)
'    For i As Integer = Inizio To Fine
'      pCanaleNVR.Valori(i) = IIf(SetAsValid, 1, -1)
'    Next
'    If SalvaXML Then SalvaNotValidRowsToXML()
'  End Sub


'  Private Sub SalvaNotValidRowsToXML()
'    pPeriodiNonValidi.Clear()
'    If pFileXML Is Nothing Then Exit Sub
'    Dim NodoPeriodi As Xml.XmlNode = pFileXML.CercaNodo(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, True)
'    If Not NodoPeriodi Is Nothing Then
'      NodoPeriodi.RemoveAll()
'    End If
'    Dim ValorePre As Boolean = pCanaleNVR.Valori(0)
'    Dim Intervallo As New clsTimeRange(DataProvider2020.TimeStamps(0), DataProvider2020.TimeStamps.Last)
'    Dim Contatore As Integer = 1
'    For i As Integer = 1 To pCanaleNVR.Valori.Count - 1
'      Dim Valore As Boolean = pCanaleNVR.Valori(i) > 0
'      If (ValorePre = True) And (Valore = False) Then
'        Intervallo.Start = DataProvider2020.TimeStamps(i)
'      ElseIf (ValorePre = False) And (Valore = True) Then
'        Intervallo.Finish = DataProvider2020.TimeStamps(i)
'        pFileXML.SalvaValoreInnerText(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, "NVR_" & Contatore.ToString.PadLeft(3, "0"), "From", Intervallo.Start.ToOADate, True, False)
'        pFileXML.SalvaValoreInnerText(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, "NVR_" & Contatore.ToString.PadLeft(3, "0"), "To", Intervallo.Finish.ToOADate, True, False)
'        pPeriodiNonValidi.Add(Intervallo.Clone)
'        Contatore += 1
'      End If
'      ValorePre = Valore
'    Next
'    If Not ValorePre Then
'      Intervallo.Finish = DataProvider2020.TimeStamps.Last
'      pFileXML.SalvaValoreInnerText(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, "NVR_" & Contatore.ToString.PadLeft(3, "0"), "From", Intervallo.Start.ToOADate, True, False)
'      pFileXML.SalvaValoreInnerText(pSuffisso, clsSettings.eNodoSTD.eNotValidRows, "NVR_" & Contatore.ToString.PadLeft(3, "0"), "To", Intervallo.Finish.ToOADate, True, False)
'      pPeriodiNonValidi.Add(Intervallo.Clone)
'    End If
'    pFileXML.SalvaFileXML()
'  End Sub


'End Class

'Public Class clsChannelSubSet
'  Implements INotifyPropertyChanged

'  Dim pCanale As clsChannel2020
'  Dim pValues As List(Of Nullable(Of Double))
'  Dim pIntervallo As clsTimeRange
'  Dim pIndiceIniziale As Integer
'  Dim pIndiceFinale As Integer
'  Dim pMax As Double
'  Dim pMin As Double
'  Dim pAvg As Double
'  Dim pAvgRaw As Double
'  Dim pAvgPort As Double
'  Dim pAvgStbd As Double
'  Dim pCurrentValue As Double
'  Dim pDs As Double
'  Dim pName As String
'  Dim pUM As String
'  Dim pUsaLongName As Boolean
'  Dim pValoreAssoluto As Boolean
'	Dim pTack As clsSailingState.eTack

'  Public Sub New(Canale As clsChannel2020)
'    pCanale = Canale
'    ImpostaNomi()
'  End Sub

'  Public Sub New(Canale As clsChannel2020, Intervallo As clsTimeRange)
'    pCanale = Canale
'    ImpostaNomi()
'    pIntervallo = Intervallo
'    pIndiceIniziale = Intervallo.IdRigaIniziale
'    pIndiceFinale = Intervallo.IdRigaFinale
'    AggiornaSubSet()
'  End Sub

'  Public ReadOnly Property ValoreIniziale As Double
'    Get
'      Return pValues.First
'    End Get
'  End Property

'  Public ReadOnly Property ValoreFinale As Double
'    Get
'      Return pValues.Last
'    End Get
'  End Property


'  Public Property Values As List(Of Nullable(Of Double))
'    Get
'      Return pValues
'    End Get
'    Set(value As List(Of Nullable(Of Double)))
'      pValues = value
'    End Set
'  End Property

'  Public Property Intervallo As clsTimeRange
'    Get
'      Return pIntervallo
'    End Get
'    Set(value As clsTimeRange)
'      pIntervallo = value
'      OnPropertyChanged("Intervallo")
'    End Set
'  End Property

'  Public ReadOnly Property Canale As clsChannel2020
'    Get
'      Return pCanale
'    End Get
'  End Property


'  Public Property SubSetChannelName As String
'    Get
'      Return pName
'    End Get
'    Set(value As String)
'      pName = value
'      OnPropertyChanged("SubSetChannelName")
'      OnPropertyChanged("PrintedColor")
'    End Set
'  End Property

'  Public Property SubSetChannelUM As String
'    Get
'      Return pUM
'    End Get
'    Set(value As String)
'      pUM = value
'      OnPropertyChanged("SubSetChannelUM")
'    End Set
'  End Property

'  Public Property MaxVal As Double
'    Get
'      Return pMax
'    End Get
'    Set(value As Double)
'      pMax = value
'      OnPropertyChanged("Max")
'      OnPropertyChanged("SubSetMax")
'    End Set
'  End Property

'  Public Property MinVal As Double
'    Get
'      Return pMin
'    End Get
'    Set(value As Double)
'      pMin = value
'      OnPropertyChanged("Min")
'      OnPropertyChanged("SubSetMin")
'    End Set
'  End Property

'  Public ReadOnly Property SubSetMax As String
'    Get
'      Return Format(pMax, "F" & pCanale.Decimals.ToString)
'    End Get
'    'Set(value As String)
'    '  pMax = value
'    '  OnPropertyChanged("SubSetMax")
'    'End Set
'  End Property

'  Public ReadOnly Property SubSetMin As String
'    Get
'      Return Format(pMin, "F" & pCanale.Decimals.ToString)
'    End Get
'    'Set(value As String)
'    '  pMin = value
'    '  OnPropertyChanged("SubSetMin")
'    'End Set
'  End Property

'  Public ReadOnly Property SubSetAvg As String
'    Get
'      Return Format(pAvg, "F" & pCanale.Decimals.ToString)
'    End Get
'    'Set(value As String)
'    '  pAvg = value
'    '  OnPropertyChanged("SubSetAvg")
'    'End Set
'  End Property

'  Public ReadOnly Property SubSetAvgRaw As String
'    Get
'      Return Format(pAvgRaw, "F" & pCanale.Decimals.ToString)
'    End Get
'    'Set(value As String)
'    '  pAvgRaw = value
'    '  OnPropertyChanged("SubSetAvgRaw")
'    'End Set
'  End Property

'  Public Property Avg As Double
'    Get
'      Return pAvg
'    End Get
'    Set(value As Double)
'      pAvg = value
'      OnPropertyChanged("Avg")
'      OnPropertyChanged("SubSetAvg")
'    End Set
'  End Property

'  Public Property AvgRaw As Double
'    Get
'      Return pAvgRaw
'    End Get
'    Set(value As Double)
'      pAvgRaw = value
'      OnPropertyChanged("AvgRaw")
'      OnPropertyChanged("SubSetAvgRaw")
'    End Set
'  End Property

'  Public ReadOnly Property SubSetAvgPort As String
'    Get
'      If pValoreAssoluto Then
'        Return Format(pAvgPort, "F" & pCanale.Decimals.ToString)
'      Else
'        Return ""
'      End If
'    End Get
'    'Set(value As String)
'    '  pAvgPort = value
'    '  OnPropertyChanged("SubSetAvgPort")
'    'End Set
'  End Property

'  Public ReadOnly Property SubSetAvgStbd As String
'    Get
'      If pValoreAssoluto Then
'        Return Format(pAvgStbd, "F" & pCanale.Decimals.ToString)
'      Else
'        Return ""
'      End If
'    End Get
'    'Set(value As String)
'    '  pAvgStbd = value
'    '  OnPropertyChanged("SubSetAvgStbd")
'    'End Set
'  End Property

'  Public Property AvgPort As Double
'    Get
'      If pValoreAssoluto Then
'        Return pAvgPort
'      Else
'        Return Nothing
'      End If
'    End Get
'    Set(value As Double)
'      pAvgPort = value
'      OnPropertyChanged("AvgPort")
'      OnPropertyChanged("SubSetAvgPort")
'    End Set
'  End Property

'  Public Property AvgStbd As Double
'    Get
'      If pValoreAssoluto Then
'        Return pAvgStbd
'      Else
'        Return ""
'      End If
'    End Get
'    Set(value As Double)
'      pAvgStbd = value
'      OnPropertyChanged("AvgStbd")
'      OnPropertyChanged("SubSetAvgStbd")
'    End Set
'  End Property

'  Public Property CurrentValue As String
'    Get
'      Return Format(pCurrentValue, "F" & pCanale.Decimals.ToString)
'    End Get
'    Set(value As String)
'      pCurrentValue = value
'      OnPropertyChanged("CurrentValue")
'    End Set
'  End Property

'  Public ReadOnly Property SubSetDs As String
'    Get
'      Return Format(pDs, "F" & pCanale.Decimals.ToString)
'    End Get
'    'Set(value As String)
'    '  pDs = value
'    '  OnPropertyChanged("SubSetDs")
'    'End Set
'  End Property

'  Public Property Ds As Double
'    Get
'      Return pDs
'    End Get
'    Set(value As Double)
'      pDs = value
'      OnPropertyChanged("Ds")
'      OnPropertyChanged("SubSetDs")
'    End Set
'  End Property

'  Public Property UsaLongName As Boolean
'    Get
'      Return pUsaLongName
'    End Get
'    Set(value As Boolean)
'      pUsaLongName = value
'    End Set
'  End Property

'  Public Property IndiceIniziale As Integer
'    Get
'      Return pIndiceIniziale
'    End Get
'    Set(value As Integer)
'      pIndiceIniziale = value
'    End Set
'  End Property

'  Public Property IndiceFinale As Integer
'    Get
'      Return pIndiceFinale
'    End Get
'    Set(value As Integer)
'      pIndiceFinale = value
'    End Set
'  End Property

'  Public Property ValoreAssoluto As Boolean
'    Get
'      Return pValoreAssoluto
'    End Get
'    Set(value As Boolean)
'      pValoreAssoluto = value
'    End Set
'  End Property

'  Public ReadOnly Property SignedByTack As Double
'    Get
'      If pTack = clsSailingState.eTack.ePort Then
'        Return -pAvg
'      Else
'        Return pAvg
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property Tack As clsSailingState.eTack
'    Get
'      Return pTack
'    End Get
'  End Property

'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Public Sub ToggleNomi()
'    pUsaLongName = Not pUsaLongName
'    ImpostaNomi()
'  End Sub

'  Public Sub ImpostaNomi()
'    If pUsaLongName Then
'      SubSetChannelName = pCanale.LongName
'      SubSetChannelUM = pCanale.LongUM
'    Else
'      SubSetChannelName = pCanale.ShortName
'      SubSetChannelUM = pCanale.ShortUM
'    End If
'  End Sub

'  'Public Sub AggiornaSubSet(Intervallo As clsTimeRange, AbsVal As Boolean)
'  '  pIntervallo = Intervallo
'  '  pValoreAssoluto = AbsVal
'  '  IndiciIntervallo(pIntervallo, pIndiceIniziale, pIndiceFinale)
'  '  AggiornaSubSet()

'  'End Sub

'  Public Sub AggiornaSubSet(IndiceIniziale As Integer, IndiceFinale As Integer, AbsVal As Boolean)
'    pIndiceIniziale = IndiceIniziale
'    pIndiceFinale = IndiceFinale
'    pValoreAssoluto = AbsVal
'    AggiornaSubSet()

'  End Sub

'  Public Sub AggiornaSubSet()
'    If pIndiceFinale < pIndiceIniziale Then Exit Sub
'    If pCanale.Valori.Count = 0 Then Exit Sub
'    Dim Vtmp = pCanale.Valori.ToList.GetRange(pIndiceIniziale, pIndiceFinale - pIndiceIniziale).Where(Function(x) Not Double.IsNaN(x))
'    If Vtmp.Count = 0 Then Exit Sub

'    pValues = Vtmp.ToList
'    Dim NrV As Integer = System.Math.Min(pIndiceFinale - pIndiceIniziale, pValues.Count - 1)
'    If pCanale.DataType = clsChannel2020.eDataType.e360 Then
'      Dim obj360 As New cls360(pCanale.Values, pIndiceIniziale, pIndiceFinale, Nothing)
'      If pValoreAssoluto Then
'        AvgPort = obj360.Avgerage(cls360.eTack.ePort)
'        AvgStbd = obj360.Avgerage(cls360.eTack.eStbd)
'      Else
'        AvgPort = 0
'        AvgStbd = 0
'      End If
'			Avg = obj360.Avgerage(cls360.eTack.eBoth)
'			MaxVal = obj360.MaxRight(cls360.eTack.eBoth)
'      MinVal = obj360.MaxLeft(cls360.eTack.eBoth)
'      Ds = obj360.StabdardDeviation(cls360.eTack.eBoth)
'    Else
'      If pValoreAssoluto Then
'        Dim AbsVals As New List(Of Double?)
'        Dim PortVals As New List(Of Double)
'        Dim StbdVals As New List(Of Double)
'				AvgRaw = pValues.Average.Value
'				'GetValoriPortStbd(pValues, pIndiceIniziale, pIndiceFinale, PortVals, StbdVals, AbsVals)
'				GetValoriPortStbd(pCanale, pIndiceIniziale, pIndiceFinale, PortVals, StbdVals, AbsVals)
'				'GetValoriPortStbd(pValues, PortVals, StbdVals, AbsVals)
'				If PortVals.Count = 0 Then
'          AvgPort = 0
'        Else
'          AvgPort = PortVals.Average
'        End If
'        If StbdVals.Count = 0 Then
'          AvgStbd = 0
'        Else
'          AvgStbd = StbdVals.Average
'        End If

'        If StbdVals.Count > 0 Then
'          If PortVals.Count > 0 Then
'            pTack = clsSailingState.eTack.eBoth
'          Else
'            pTack = clsSailingState.eTack.eStbd
'          End If
'        ElseIf PortVals.Count > 0 Then
'          pTack = clsSailingState.eTack.ePort
'        Else
'          pTack = clsSailingState.eTack.eNone
'        End If

'        If AbsVals.Count = 0 Then
'          Avg = 0
'          MaxVal = 0
'          MinVal = 0
'        Else
'					Avg = AbsVals.Average.Value
'					MaxVal = AbsVals.Max.Value
'					MinVal = AbsVals.Min.Value
'				End If
'        Dim Valori As IEnumerable(Of Double) = AbsVals.Cast(Of Double)
'				alglib.basestat.sampleadev(Valori, Valori.Count, Ds)
'				'Ds = TAlex.MathCore.Statistics.MathStats.PopulationStandardDeviation(Valori)
'			Else
'				Avg = If(pValues.Count = 0, 0, pValues.Average.Value)
'				AvgPort = 0
'        AvgStbd = 0
'				MaxVal = If(pValues.Count = 0, 0, pValues.Max.Value)
'				MinVal = If(pValues.Count = 0, 0, pValues.Min.Value)
'				Dim Valori As IEnumerable(Of Double) = pValues.Cast(Of Double)
'				alglib.basestat.sampleadev(Valori, Valori.Count, Ds)

'				'Ds = TAlex.MathCore.Statistics.MathStats.PopulationStandardDeviation(Valori)
'			End If
'    End If

'  End Sub

'  ' Public Sub AggiornaSubSet()
'  '	If pIndiceFinale < pIndiceIniziale Then Exit Sub
'  '   If pCanale.Valori.Count = 0 Then Exit Sub
'  '   Dim NrV As Integer = System.Math.Min(pIndiceFinale - pIndiceIniziale, pCanale.Valori.Where(Function(x) Not Double.IsNaN(x)).Count - 1)
'  '   pValues = pCanale.Valori.ToList.GetRange(pIndiceIniziale, NrV).Where(Function(x) Not Double.IsNaN(x)).ToList
'  '   'pIntervallo = New clsTimeRange(pIndiceIniziale, pIndiceFinale)
'  '   If pCanale.DataType = clsChannel2020.eDataType.e360 Then
'  '		Dim obj360 As New cls360(pCanale.Values, pIndiceIniziale, pIndiceFinale, Nothing)
'  '		If pValoreAssoluto Then
'  '			SubSetAvgPort = obj360.Avgerage(cls360.eTack.ePort)
'  '			SubSetAvgStbd = obj360.Avgerage(cls360.eTack.eStbd)
'  '		Else
'  '			SubSetAvgPort = 0
'  '			SubSetAvgStbd = 0
'  '		End If
'  '		SubSetAvg = obj360.Avgerage(cls360.eTack.eBoth)
'  '		SubSetMax = obj360.MaxRight(cls360.eTack.eBoth)
'  '		SubSetMin = obj360.MaxLeft(cls360.eTack.eBoth)
'  '		SubSetDs = obj360.StabdardDeviation(cls360.eTack.eBoth)
'  '     'Dim NrV As Integer = pIndiceFinale - pIndiceIniziale + 1
'  '     'pValues = pCanale.Valori.Where(Function(x) Not Double.IsNaN(x)).ToList.ToList.GetRange(pIndiceIniziale, NrV)
'  '   Else
'  '		If pValoreAssoluto Then
'  '			Dim AbsVals As New List(Of Double?)
'  '			Dim PortVals As New List(Of Double)
'  '			Dim StbdVals As New List(Of Double)
'  '			SubSetAvgRaw = pCanale.Valori.Average
'  '			GetValoriPortStbd(pCanale.Values, pIndiceIniziale, pIndiceFinale, PortVals, StbdVals, AbsVals)
'  '			If PortVals.Count = 0 Then
'  '				SubSetAvgPort = 0
'  '			Else
'  '				SubSetAvgPort = PortVals.Average
'  '			End If
'  '			If StbdVals.Count = 0 Then
'  '				SubSetAvgStbd = 0
'  '			Else
'  '				SubSetAvgStbd = StbdVals.Average
'  '			End If

'  '			If StbdVals.Count > 0 Then
'  '				If PortVals.Count > 0 Then
'  '					pTack = clsSailingState.eTack.eBoth
'  '				Else
'  '					pTack = clsSailingState.eTack.eStbd
'  '				End If
'  '			ElseIf PortVals.Count > 0 Then
'  '				pTack = clsSailingState.eTack.ePort
'  '			Else
'  '				pTack = clsSailingState.eTack.eNone
'  '			End If

'  '			If AbsVals.Count = 0 Then
'  '				SubSetAvg = 0
'  '				SubSetMax = 0
'  '				SubSetMin = 0
'  '			Else
'  '				SubSetAvg = AbsVals.Average
'  '				SubSetMax = AbsVals.Max
'  '				SubSetMin = AbsVals.Min
'  '			End If
'  '			Dim Valori As IEnumerable(Of Double) = AbsVals.Cast(Of Double)
'  '			SubSetDs = TAlex.MathCore.Statistics.MathStats.PopulationStandardDeviation(Valori)
'  '       'Dim NrV As Integer = pIndiceFinale - pIndiceIniziale + 1
'  '       '    pValues = pCanale.Valori.Where(Function(x) Not Double.IsNaN(x)).ToList.ToList.GetRange(IndiceIniziale, NrV)
'  '     Else
'  '       'Dim NrV As Integer = pIndiceFinale - pIndiceIniziale + 1
'  '       '    pValues = pCanale.Valori.Where(Function(x) Not Double.IsNaN(x)).ToList.ToList.GetRange(IndiceIniziale, NrV)
'  '       SubSetAvg = IIf(pValues.Count = 0, 0, pValues.Average)
'  '			SubSetAvgPort = 0
'  '			SubSetAvgStbd = 0
'  '			SubSetMax = IIf(pValues.Count = 0, 0, pValues.Max)
'  '			SubSetMin = IIf(pValues.Count = 0, 0, pValues.Min)
'  '			Dim Valori As IEnumerable(Of Double) = pValues.Cast(Of Double)
'  '			SubSetDs = TAlex.MathCore.Statistics.MathStats.PopulationStandardDeviation(Valori)
'  '		End If
'  '	End If

'  'End Sub

'  'Private Sub Values360(ByRef ValuesSin As List(Of Double?), ByRef ValuesCos As List(Of Double?))
'  '  For i As Integer = 0 To pValues.Count - 1
'  '    Dim VR As Double = Radians(pValues(i).Value)
'  '    ValuesSin.Add(System.Math.Sin(VR))
'  '    ValuesCos.Add(System.Math.Cos(VR))
'  '  Next
'  'End Sub

'End Class


Public Class cls360
  Dim pValori As List(Of Double?)
  Dim pIndiceIniziale As Integer
  Dim pIndiceFinale As Integer
  Dim pCanaleTack As clsChannel2020

  Dim pAvg, pPortAvg, pStbdAvg, pMaxR, pPortMaxR, pStbdMaxR, pMaxL, pPortMaxL, pStbdMaxL As Double
  Dim pDS, pPortDS, pStbdDS

  Public Enum eTack
    eBoth = 0
    ePort = 1
    eStbd = 2
  End Enum

  Public ReadOnly Property Avgerage(Tack As eTack) As Double
    Get
      Select Case Tack
        Case eTack.ePort
          Return pPortAvg
        Case eTack.eStbd
          Return pStbdAvg
        Case Else
          Return pAvg
      End Select
    End Get
  End Property

  Public ReadOnly Property MaxRight(Tack As eTack) As Double
    Get
      Select Case Tack
        Case eTack.ePort
          Return pPortMaxR
        Case eTack.eStbd
          Return pStbdMaxR
        Case Else
          Return pMaxR
      End Select
    End Get
  End Property

  Public ReadOnly Property MaxLeft(Tack As eTack) As Double
    Get
      Select Case Tack
        Case eTack.ePort
          Return pPortMaxL
        Case eTack.eStbd
          Return pStbdMaxL
        Case Else
          Return pMaxL
      End Select
    End Get
  End Property

  Public ReadOnly Property StabdardDeviation(Tack As eTack) As Double
    Get
      Select Case Tack
        Case eTack.ePort
          Return pPortDS
        Case eTack.eStbd
          Return pStbdDS
        Case Else
          Return pDS
      End Select
    End Get
  End Property

  Public Property CanaleTack As clsChannel2020
    Get
      Return pCanaleTack
    End Get
    Set(value As clsChannel2020)
      pCanaleTack = value
    End Set
  End Property

  Public Sub New(Valori As List(Of Double?), IndiceIniziale As Integer, IndiceFinale As Integer, CanaleTack As clsChannel2020)
    pValori = Valori
    pIndiceIniziale = IndiceIniziale
    pIndiceFinale = IndiceFinale
    pCanaleTack = CanaleTack
    If CanaleTack Is Nothing Then pCanaleTack = CanaleTackDefault()

    Values360()
  End Sub

  Public Sub New(Valori As List(Of Double?), CanaleTack As clsChannel2020)
    pValori = Valori
    pIndiceIniziale = 0
    pIndiceFinale = Valori.Count - 1
    pCanaleTack = CanaleTack
    If CanaleTack Is Nothing Then pCanaleTack = CanaleTackDefault()

    Values360()
  End Sub

  Public Sub New(Valori As List(Of Double?))
    pValori = Valori
    pIndiceIniziale = 0
    pIndiceFinale = Valori.Count - 1
    pCanaleTack = CanaleTackDefault()
    Values360()
  End Sub

  Private Sub Values360()
    Dim pValues As New List(Of Double?)
    Dim pValuesSin As New List(Of Double?)
    Dim pValuesCos As New List(Of Double?)

    'Dim pDeltas As New List(Of Double?)
    'Dim pPortDeltas As New List(Of Double?)
    'Dim pStbdDeltas As New List(Of Double?)

    Dim pPortValori As New List(Of Double?)
    Dim pPortValuesSin As New List(Of Double?)
    Dim pPortValuesCos As New List(Of Double?)

    Dim pStbdValori As New List(Of Double?)
    Dim pStbdValuesSin As New List(Of Double?)
    Dim pStbdValuesCos As New List(Of Double?)

    'Dim ValoriTemp = pValori.ToList.GetRange(pIndiceIniziale, pIndiceFinale - pIndiceIniziale).Where(Function(x) Not Double.IsNaN(x)).ToList

    For i As Integer = pIndiceIniziale To pIndiceFinale
      If Not Double.IsNaN(pValori(i).Value) Then
        Dim VG As Double = pValori(i).Value
        Dim VR As Double = Radians(VG)
        pValues.Add(VG)
        pValuesSin.Add(System.Math.Sin(VR))
        pValuesCos.Add(System.Math.Cos(VR))
        If pCanaleTack.Valori(i) < 0 Then
          pPortValori.Add(VG)
          pPortValuesSin.Add(System.Math.Sin(VR))
          pPortValuesCos.Add(System.Math.Cos(VR))
        Else
          pStbdValori.Add(VG)
          pStbdValuesSin.Add(System.Math.Sin(VR))
          pStbdValuesCos.Add(System.Math.Cos(VR))
        End If
      End If
    Next
    pAvg = Verifica360(Degrees(System.Math.Atan2(pValuesSin.Average.Value, pValuesCos.Average.Value)))
    If pPortValuesSin.Count > 0 Then
      pPortAvg = Verifica360(Degrees(System.Math.Atan2(pPortValuesSin.Average.Value, pPortValuesCos.Average.Value)))
      Dim MaxPort As Double = pPortValori.Max.Value
      Dim MinPort As Double = pPortValori.Min.Value
      If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MinPort, MaxPort) > 0 Then
        pPortMaxR = MaxPort
        pPortMaxL = MinPort
      Else
        pPortMaxL = MaxPort
        pPortMaxR = MinPort
      End If
    Else
      pPortAvg = 0
    End If
    If pStbdValuesSin.Count > 0 Then
      pStbdAvg = Verifica360(Degrees(System.Math.Atan2(pStbdValuesSin.Average.Value, pStbdValuesCos.Average.Value)))
      Dim MaxStbd As Double = pStbdValori.Max.Value
      Dim MinStbd As Double = pStbdValori.Min.Value
      If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MinStbd, MaxStbd) > 0 Then
        pStbdMaxR = MaxStbd
        pStbdMaxL = MinStbd
      Else
        pStbdMaxL = MaxStbd
        pStbdMaxR = MinStbd
      End If
    Else
      pStbdAvg = 0
    End If

    If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pValues.Min.Value, pValues.Max.Value) > 0 Then
      pMaxR = pValues.Max.Value
      pMaxL = pValues.Min.Value
    Else
      pMaxR = pValues.Min.Value
      pMaxL = pValues.Max.Value
    End If

    Dim DStmp = pValues.Select(Function(x) System.Math.Sqrt(DifferenzaAssolutaTraAngoli360(x.Value, pAvg, True)))
    pDS = DStmp.Average
    If pPortValori.Count > 0 Then
      DStmp = pPortValori.Select(Function(x) System.Math.Sqrt(DifferenzaAssolutaTraAngoli360(x.Value, pPortAvg, True)))
      pPortDS = DStmp.Average
    End If
    If pStbdValori.Count > 0 Then
      DStmp = pStbdValori.Select(Function(x) System.Math.Sqrt(DifferenzaAssolutaTraAngoli360(x.Value, pStbdAvg, True)))
      pStbdDS = DStmp.Average
    End If
  End Sub

  Private Function Verifica360(Valore As Double) As Double
    If Valore < 0 Then Return Valore + 360
    If Valore > 360 Then Return Valore - 360
    Return Valore
  End Function

End Class

'Public Class clsChannel2020
'  Implements INotifyPropertyChanged


'  Dim pIdCanale As Integer
'  Dim pShortName As String
'  Dim pLongName As String
'  Dim pShortUM As String
'  Dim pLongUM As String
'  Dim pDataType As eDataType
'  Dim pLogHeader As String
'  Dim pPolarHeader As String
'  Dim pHeaders As List(Of String)
'  Dim pIsMath As Boolean = False
'  Dim pImporta As Boolean = True
'  Dim pValues As List(Of Nullable(Of Double))
'  Dim pValuesDT As List(Of Nullable(Of DateTime))
'  Dim pValuesTxt As List(Of String)
'  Dim pValuesBl As List(Of Boolean)
'  Dim pPrintedColor As Color
'  Dim pDecimals As Integer = 1
'  Dim pTestValue As String
'	Dim pXMLname As String
'	'Dim pTack As eTack = eTack.eUndefined

'	'Dim pDisplayControl As ctrl_DataDisplay

'	Dim pCanaleChiave As clsChannels2020.eCanaliChiave = Nothing

'  Dim pIsNuovoCanale As Boolean = True

'  Dim pIsSelected As Boolean = False

'  Dim pChannelSubSet As clsChannelSubSet

'	Dim pStraightLineChartSync As clsStraightLineChartSync

'	Public Sub New(XMLname As String)

'		pXMLname = XMLname
'    Values = New List(Of Nullable(Of Double))
'    ValuesDT = New List(Of Nullable(Of DateTime))
'    ValuesTxt = New List(Of String)
'    ValuesBl = New List(Of Boolean)
'    pChannelSubSet = New clsChannelSubSet(Me)

'	End Sub

'  Public Enum eDataType
'    eLinear = 0
'    e180 = 1
'    e360 = 2
'    eAbsLinear = 3
'    eAbs180 = 4
'    eTack = 5
'    eText = 6
'    eBoolean = 7
'    ePercentage = 8
'    eDateTime = 9
'    eDateOnly = 10
'    eTimeOnly = 11
'    eTackReversed = 12
'  End Enum

'	Public Enum eTack
'		ePort = 0
'		eStbd = 1
'		eUndefined = 2
'	End Enum

'	Public Property StraightLineChartSync As clsStraightLineChartSync
'		Get
'			Return pStraightLineChartSync
'		End Get
'		Set(value As clsStraightLineChartSync)
'			pStraightLineChartSync = value
'		End Set
'	End Property

'	Public Property PrintedColor As Color
'    Get
'      Return pPrintedColor
'    End Get
'    Set(value As Color)
'      pPrintedColor = value
'    End Set
'  End Property

'  Public ReadOnly Property DataTypeList As List(Of String)
'    Get
'      Dim Vals As String() = System.Enum.GetNames(GetType(eDataType))
'      Return Vals.ToList
'    End Get
'  End Property

'  Public Property ChannelSubSet As clsChannelSubSet
'    Get
'      Return pChannelSubSet
'    End Get
'    Set(value As clsChannelSubSet)
'      pChannelSubSet = value
'    End Set
'  End Property

'  Public Function ToggleNomi() As ICommand
'    pChannelSubSet.ToggleNomi()
'  End Function

'  Public ReadOnly Property SqlDbName As String
'    Get
'      If pCanaleChiave = clsChannels2020.eCanaliChiave.eNone Then
'        Return LongNameNoSpaces
'      Else
'        Return "_" & System.Enum.GetName(GetType(clsChannels2020.eCanaliChiave), pCanaleChiave).TrimStart("e")
'      End If
'    End Get
'  End Property

'  Public Sub AggiornaSubSetTimeRange(IndiceIniziale As Long, IndiceFinale As Long, ValoreAssoluto As Boolean)
'    pChannelSubSet.IndiceIniziale = IndiceIniziale
'    pChannelSubSet.IndiceFinale = IndiceFinale
'    pChannelSubSet.ValoreAssoluto = ValoreAssoluto
'    pChannelSubSet.AggiornaSubSet()
'  End Sub

'  Public Function BasicStats(IndiceIniziale As Integer, IndiceFinale As Integer) As clsChannelBasicStats
'		Return New clsChannelBasicStats(Me, IndiceIniziale, IndiceFinale)
'	End Function

'  Public Function ValoreDBL(Riga As Integer) As Double
'    If Riga < 0 Then Return 0
'    Select Case pDataType
'			Case eDataType.eAbs180, eDataType.eAbsLinear, eDataType.e180
'				If pValues.Count = 0 Then Return Nothing
'        Return System.Math.Abs(pValues(Riga).Value)
'      Case eDataType.eBoolean
'        If pValuesBl.Count = 0 Then Return Nothing
'        Return IIf(pValuesBl(Riga), 1, 0)
'      Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly
'        If pValuesDT.Count = 0 Then Return Nothing
'        Return pValuesDT(Riga).Value.ToOADate
'      Case eDataType.eTack
'        If pValues.Count = 0 Then Return Nothing
'        Return CanaleTackDefault.TackSign(Riga) * pValues(Riga).Value
'      Case eDataType.eTackReversed
'        If pValues.Count = 0 Then Return Nothing
'        Return -CanaleTackDefault.TackSign(Riga) * pValues(Riga).Value
'      Case eDataType.eText
'        Return 0
'      Case Else
'        If pValues.Count = 0 Then Return Nothing
'        Return pValues(Riga).Value
'    End Select

'  End Function

'  Public Function ValorePerSql(Riga As Integer) As Nullable(Of Double)
'    Select Case pDataType
'      Case eDataType.eBoolean
'        If pValuesBl.Count = 0 Then Return Nothing
'        If pValuesBl(Riga) Then
'          Return 0
'        Else
'          Return 1
'        End If
'        'Return IIf(pValuesBl(Riga), 1, 0)
'      Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly
'        If pValuesDT.Count = 0 Then Return Nothing
'        Return pValuesDT(Riga).Value.ToOADate
'      Case eDataType.eText
'        Return Nothing
'      Case Else
'        If pValues.Count = 0 Then Return Nothing
'        Return pValues(Riga)
'    End Select

'  End Function

'  '   StbdTack e TackSign servono solo per il canale che determina le mura
'  Public Function StbdTack(Riga) As Boolean
'		Return pValues(Riga).Value > 0
'	End Function

'	Public Function TackSign(Riga) As Integer
'		Return IIf(pValues(Riga).Value > 0, 1, -1)
'	End Function

'	Public Property Values As List(Of Nullable(Of Double))
'    Get
'      Return pValues
'    End Get
'    Set(value As List(Of Nullable(Of Double)))
'      pValues = value
'    End Set
'  End Property

'  Public Property ValuesDT As List(Of Nullable(Of DateTime))
'    Get
'      Return pValuesDT
'    End Get
'    Set(value As List(Of Nullable(Of DateTime)))
'      pValuesDT = value
'    End Set
'  End Property

'  Public Property ValuesTxt As List(Of String)
'    Get
'      Return pValuesTxt
'    End Get
'    Set(value As List(Of String))
'      pValuesTxt = value
'    End Set
'  End Property

'  Public Property ValuesBl As List(Of Boolean)
'    Get
'      Return pValuesBl
'    End Get
'    Set(value As List(Of Boolean))
'      pValuesBl = value
'    End Set
'  End Property

'  Public ReadOnly Property Loaded As Boolean
'    Get
'      Return pIdCanale > -1
'    End Get
'  End Property

'  Public Property IsMath As Boolean
'    Get
'      Return pIsMath
'    End Get
'    Set(value As Boolean)
'      pIsMath = value

'		End Set
'  End Property

'  Public Property Importa As Boolean
'    Get
'      Return pImporta
'    End Get
'    Set(value As Boolean)
'      If pImporta = value Then Exit Property
'      pImporta = value
'			OnPropertyChanged("Importa")
'		End Set
'  End Property

'  Public Property TestValue As String
'    Get
'      Return pTestValue
'    End Get
'    Set(value As String)
'      pTestValue = value
'      OnPropertyChanged("TestValue")
'    End Set
'  End Property

'  Public Property IsNuovoCanale As Boolean
'    Get
'      Return pIsNuovoCanale
'    End Get
'    Set(value As Boolean)
'      pIsNuovoCanale = value
'    End Set
'  End Property

'  Public Property IdCanale As Integer
'    Get
'      Return pIdCanale
'    End Get
'    Set(value As Integer)
'      pIdCanale = value
'    End Set
'  End Property

'  Public Property Decimals As Integer
'    Get
'      Return pDecimals
'    End Get
'    Set(value As Integer)
'      If pDecimals = value Then Exit Property
'      pDecimals = value
'      OnPropertyChanged("Decimals")
'    End Set
'  End Property

'  Public ReadOnly Property CaricaCanale As Boolean
'    Get
'      Return Importa AndAlso IdCanale > -1
'    End Get
'  End Property

'  Public Property ShortName As String
'    Get
'      Return pShortName
'    End Get
'    Set(value As String)
'      If pShortName = value Then Exit Property
'      pShortName = value
'      OnPropertyChanged("ShortName")
'    End Set
'  End Property

'  Public Property LongName As String
'    Get
'      Return pLongName
'    End Get
'    Set(value As String)
'      If value = "FCS_EmcyStop" & vbNullChar Then Stop

'      If pLongName = value Then Exit Property
'      pLongName = value
'      OnPropertyChanged("LongName")
'      'pLongName = value
'    End Set
'  End Property

'  Public Property XMLname As String
'    Get
'      Return pXMLname
'    End Get
'    Set(value As String)
'      If pXMLname = value Then Exit Property
'      pXMLname = value
'      OnPropertyChanged("XMLname")
'      'pLongName = value
'    End Set
'  End Property

'  Public ReadOnly Property LongNameNoSpaces As String
'    Get
'      Return pLongName.Replace(" ", "")
'    End Get
'  End Property

'  Public Property ShortUM As String
'    Get
'      Return pShortUM
'    End Get
'    Set(value As String)
'      If pShortUM = value Then Exit Property
'      pShortUM = value
'      OnPropertyChanged("ShortUM")
'      'pShortUM = value
'    End Set
'  End Property

'  Public Property LongUM As String
'    Get
'      Return pLongUM
'    End Get
'    Set(value As String)
'      If pLongUM = value Then Exit Property
'      pLongUM = value
'      OnPropertyChanged("LongUM")
'      'pLongUM = value
'    End Set
'  End Property

'  Public Property DataType As eDataType
'    Get
'      Return pDataType
'    End Get
'    Set(value As eDataType)
'      If pDataType = value Then Exit Property
'      pDataType = value
'      OnPropertyChanged("DataType")
'      'pDataType = value
'    End Set
'  End Property

'  Public Property CanaleChiave As clsChannels2020.eCanaliChiave
'    Get
'      Return pCanaleChiave
'    End Get
'    Set(value As clsChannels2020.eCanaliChiave)
'      If pCanaleChiave = value Then Exit Property
'      pCanaleChiave = value
'      OnPropertyChanged("strCanaleChiave")
'    End Set
'  End Property

'	Public ReadOnly Property strCanaleChiave As String
'		Get
'			If pCanaleChiave = clsChannels2020.eCanaliChiave.eNone Then
'				Return "-"
'			Else
'				Return pCanaleChiave.ToString.TrimStart("e")
'			End If
'		End Get
'	End Property

'  Public Property LogHeader As String
'    Get
'      Return pLogHeader
'    End Get
'    Set(value As String)
'      pLogHeader = value
'    End Set
'  End Property

'  Public Property PolarHeader As String
'    Get
'      'va implementata come lammadonnassarda comanda
'      If pPolarHeader = "" Then
'        Return pShortName
'      Else
'        Return pPolarHeader
'      End If
'    End Get
'    Set(value As String)
'      If pPolarHeader = value Then Exit Property
'      pPolarHeader = value
'      OnPropertyChanged("PolarHeader")
'    End Set
'  End Property

'  Public Sub HeadersDaStringa(StringaHeaders As String)
'		If StringaHeaders.IndexOf(",") > -1 Then
'			Headers = StringaHeaders.Split(",").ToList
'		Else
'			Dim lT As New List(Of String)
'			lT.Add(StringaHeaders)
'			Headers = lT
'		End If
'	End Sub

'	Public Property Headers As List(Of String)
'    Get
'      Return pHeaders
'    End Get
'    Set(value As List(Of String))
'      pHeaders = value
'    End Set
'  End Property

'  Public ReadOnly Property HeadersString As String
'    Get
'      Dim strTMP As String = ""
'      For Each HDR In Headers
'        strTMP &= HDR & ","
'      Next
'      Return strTMP.TrimEnd(",")
'      Return pHeaders.ToString
'    End Get
'  End Property

'  Public ReadOnly Property IsCanaleChiave As Boolean
'    Get
'      Return Not pCanaleChiave = clsChannels2020.eCanaliChiave.eNone
'    End Get
'  End Property

'  Public ReadOnly Property IsNumericOrBoolean As Boolean
'    Get
'      Select Case pDataType
'        Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly, eDataType.eText
'          Return False
'        Case Else
'          Return True
'      End Select
'    End Get
'  End Property

'  Public Property IsSelected As Boolean
'    Get
'      Return pIsSelected
'    End Get
'    Set(value As Boolean)
'      If pIsSelected = value Then Exit Property
'      pIsSelected = value
'      OnPropertyChanged("IsSelected")
'    End Set
'  End Property

'	'Public Property Tack As eTack
'	'	Get
'	'		Return pTack
'	'	End Get
'	'	Set(value As eTack)
'	'		pTack = value
'	'	End Set
'	'End Property

'	Public Sub ImpostaTestValue(ValoreRaw As String, Riga As Integer)
'		If pImporta Then
'			Select Case pDataType
'				Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly
'					TestValue = ValoreRaw & " - " & Format(pValuesDT(Riga), "yyyy MM dd HH:mm:ss.fff")
'				Case eDataType.eBoolean
'					TestValue = ValoreRaw & " - " & IIf(pValuesBl(Riga) = True, "V", "X")
'				Case eDataType.eText
'					TestValue = ValoreRaw & " - " & pValuesTxt(Riga)
'				Case Else
'          TestValue = ValoreRaw & " - " & Format(pValues(Riga), "F" & pDecimals.ToString)
'      End Select
'		Else
'			TestValue = ValoreRaw
'		End If
'	End Sub

'	Public Sub ImpostaTestValue(ValoreRaw As String)
'    TestValue = ValoreRaw
'  End Sub


'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'	Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'		RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'	End Sub

'	Public Sub SalvaSuXML(Suffisso As String, SalvaSuFile As Boolean)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "KeyChannel", pCanaleChiave, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "DataType", pDataType, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "LongName", pLongName, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "LongUM", pLongUM, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "ShortName", pShortName, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "ShortUM", pShortUM, True, False)
'		AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "Load", pImporta, True, False)
'    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "Decimals", pDecimals, True, False)
'    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, LongNameNoSpaces, "Headers", HeadersString, True, False)
'		If SalvaSuFile Then AppConfig.SalvaFileXML()
'	End Sub

'End Class

'Public Class clsChannelBasicStats
'	Dim pAvg As Double
'	Dim pMax As Double
'	Dim pMin As Double
'  Dim pValoriNotNan As List(Of Double?)
'  Dim pCanale As clsChannel2020
'  Dim pValoriTack As List(Of Double?)

'  Public Sub New(Canale As clsChannel2020, IndiceIniziale As Double, IndiceFinale As Double)
'    pCanale = Canale
'    ImpostaValori(Canale, IndiceIniziale, IndiceFinale)
'  End Sub

'  Public Property Avg As Double
'		Get
'			Return pAvg
'		End Get
'		Set(value As Double)
'			pAvg = value
'		End Set
'	End Property

'	Public Property Max As Double
'		Get

'			Return System.Math.Max(pMax, pMin)
'		End Get
'		Set(value As Double)
'			pMax = value
'		End Set
'	End Property

'	Public Property Min As Double
'		Get
'			Return System.Math.Min(pMax, pMin)
'		End Get
'		Set(value As Double)
'			pMin = value
'		End Set
'	End Property

'  Public Property ValoriNotNan As List(Of Double?)
'    Get
'      Return pValoriNotNan
'    End Get
'    Set(value As List(Of Double?))
'      pValoriNotNan = value
'    End Set
'  End Property

'  Public Property Canale As clsChannel2020
'    Get
'      Return pCanale
'    End Get
'    Set(value As clsChannel2020)
'      pCanale = value
'    End Set
'  End Property

'  Public ReadOnly Property ValoreNotNan(IndiceRelativo As Integer) As Double
'    Get
'      Select Case pCanale.DataType
'        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear, clsChannel2020.eDataType.e180
'          Return System.Math.Abs(pValoriNotNan(IndiceRelativo).Value)
'        Case clsChannel2020.eDataType.eTack
'          Return (pValoriNotNan(IndiceRelativo) * IIf(pValoriTack(IndiceRelativo) > 0, 1, -1))
'        Case clsChannel2020.eDataType.eTackReversed
'          Return (pValoriNotNan(IndiceRelativo) * IIf(pValoriTack(IndiceRelativo) > 0, -1, 1))
'        Case Else
'          Return pValoriNotNan(IndiceRelativo).Value
'      End Select
'    End Get
'  End Property

'  Public Sub ImpostaValori(Canale As clsChannel2020, RigaIniziale As Integer, RigaFinale As Integer)
'    If Canale.Valori.Count + Canale.ValuesBl.Count = 0 Then Exit Sub
'    Dim NrCampioni = System.Math.Min(RigaFinale - RigaIniziale + 1, Canale.Valori.Count - RigaIniziale)
'    NrCampioni = System.Math.Max(NrCampioni, 1)
'    If Not Canale.Valori.Count = 0 Then
'      pValoriNotNan = Canale.Valori.ToList.GetRange(RigaIniziale, NrCampioni).Where(Function(x) Not Double.IsNaN(x)).ToList
'    End If
'    Select Case Canale.DataType
'      Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbsLinear
'        Dim Rng As New List(Of Double)
'        For Each v In pValoriNotNan
'          Rng.Add(System.Math.Abs(v.Value))
'        Next
'        pMax = System.Math.Abs(Rng.Max)
'        pMin = System.Math.Abs(Rng.Min)
'        pAvg = System.Math.Abs(Rng.Average)
'      Case clsChannel2020.eDataType.e360
'        'max e min sono il valore piú a destra e quello piú a sinistra del range
'        Dim obj360 As New cls360(pValoriNotNan)
'        pMax = obj360.MaxRight(cls360.eTack.eBoth)
'        pMin = obj360.MaxLeft(cls360.eTack.eBoth)
'        pAvg = obj360.Avgerage(cls360.eTack.eBoth)
'      Case clsChannel2020.eDataType.eBoolean
'        pValoriNotNan = New List(Of Double?)
'        For i As Integer = RigaIniziale To RigaFinale
'          pValoriNotNan.Add(If(Canale.ValuesBl(i), 1, -1))
'        Next
'        pMax = 1
'        pMin = -1
'        pAvg = 0
'      Case clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
'        pMax = Canale.ValoriDT(RigaFinale).Value.ToOADate
'        pMin = Canale.ValoriDT(RigaIniziale).Value.ToOADate
'        pAvg = Canale.ValoriDT(CInt((RigaFinale + RigaIniziale) / 2)).Value.ToOADate
'      Case clsChannel2020.eDataType.eTack
'        pValoriTack = CanaleTackDefault.Valori.ToList.GetRange(RigaIniziale, NrCampioni).Where(Function(x) Not Double.IsNaN(x)).ToList
'        pMax = MediaTack(pValoriNotNan.ToArray, pValoriTack.ToArray)
'        pMin = MediaTack(pValoriNotNan.ToArray, pValoriTack.ToArray)
'        pAvg = MediaTack(pValoriNotNan.ToArray, pValoriTack.ToArray)
'      Case clsChannel2020.eDataType.eTackReversed
'        pValoriTack = CanaleTackDefault.Valori.ToList.GetRange(RigaIniziale, NrCampioni).Where(Function(x) Not Double.IsNaN(x)).ToList
'        pMax = MediaTackReversed(pValoriNotNan.ToArray, pValoriTack.ToArray)
'        pMin = MediaTackReversed(pValoriNotNan.ToArray, pValoriTack.ToArray)
'        pAvg = MediaTackReversed(pValoriNotNan.ToArray, pValoriTack.ToArray)
'      Case clsChannel2020.eDataType.eText
'        pMax = 0
'        pMin = 0
'        pAvg = 0
'      Case Else
'        If Canale.Valori.Count = 0 Then
'          pMax = 0
'          pMin = 0
'          pAvg = 0
'        Else
'          pMax = pValoriNotNan.Max.Value
'          pMin = pValoriNotNan.Min.Value
'          pAvg = pValoriNotNan.Average.Value
'        End If
'    End Select

'  End Sub


'End Class


Public Class clsRisultatoGroupBy
  Dim pCanaleGroupBy As clsChannel2020
  Dim pValoriGroupBy As New List(Of clsValoriGroupBy)
  Dim pValoreAggregazione As Double

  Public ReadOnly Property ValoreAggregazione As Double
    Get
      Return pValoreAggregazione
    End Get
  End Property

  Public ReadOnly Property CanaleGroupBy As clsChannel2020
    Get
      Return pCanaleGroupBy
    End Get
  End Property

  Public ReadOnly Property CanaliSecondari As List(Of clsChannel2020)
    Get
      Dim tmp As New List(Of clsChannel2020)
      For Each VGB In ValoriGroupBy.First.DettagliCanaliSecondari
        tmp.Add(VGB.Canale)
      Next
      Return tmp
    End Get
  End Property



  Public Property ValoriGroupBy As List(Of clsValoriGroupBy)
    Get
      Return pValoriGroupBy
    End Get
    Set(value As List(Of clsValoriGroupBy))
      pValoriGroupBy = value
    End Set
  End Property

  Public Sub New(CanaleGroupBy As clsChannel2020, ValoreAggregazione As Double)
    pValoreAggregazione = ValoreAggregazione
    pCanaleGroupBy = CanaleGroupBy
  End Sub

  Public Function AggiungiRaggruppamento(DefinizioneGruppo As Double, IndiciGruppo As List(Of Integer)) As clsValoriGroupBy
    Dim NuovoGruppo As New clsValoriGroupBy(DefinizioneGruppo, pCanaleGroupBy, IndiciGruppo)
    pValoriGroupBy.Add(NuovoGruppo)
    Return NuovoGruppo
  End Function

End Class

Public Class clsValoriGroupBy
  Dim pCanalePrincipale As clsChannel2020
  Dim pDefinizioneGruppo As Double
  Dim pIndici As List(Of Integer)
  Dim pIndiciFiltrati As List(Of Integer)
  Dim pDettagliCanaleRaggruppamento As clsDettagliCanale
  Dim pDettagliCanaliSecondari As New List(Of clsDettagliCanale) ' Canale , Indice che corrisponde a quello della variabile groupby e relativo valore del canale secondario
  Dim pCanaleFiltrato As Boolean
  Dim pValoriIndiciFiltrati As List(Of Double?)
  Dim pCampioniPort As List(Of Integer)
  Dim pCampioniStbd As List(Of Integer)

  Public ReadOnly Property DefinizioneGruppo As Double
    Get
      Return pDefinizioneGruppo
    End Get
  End Property

  Public ReadOnly Property Indici() As List(Of Integer)
    Get
      Return pIndici
    End Get
  End Property

  Public ReadOnly Property ValoreMedioDatiFiltrati As Double
    Get
      If pValoriIndiciFiltrati Is Nothing OrElse pValoriIndiciFiltrati.Count = 0 Then
        Return 0
      Else
        Return ValoriIndiciFiltrati.Average
      End If
    End Get
  End Property

  Public ReadOnly Property ValoriIndiciFiltrati As List(Of Double?)
    Get
      Return pValoriIndiciFiltrati
    End Get
  End Property

  Public ReadOnly Property IndiciFiltrati() As List(Of Integer)
    Get
      If Not pCanaleFiltrato Then Return pIndici
      If pIndiciFiltrati Is Nothing Then Return pIndici
      Return pIndiciFiltrati
    End Get
  End Property

  Public ReadOnly Property NrCampioni As Integer
    Get
      Dim Val As Integer = 0
      If pCanaleFiltrato Then
        If pIndiciFiltrati Is Nothing Then
          Val = 0
        Else
          Val = pIndiciFiltrati.Count
        End If
      Else
        If pIndici Is Nothing Then
          Val = 0
        Else
          Val = pIndici.Count
        End If
      End If
      Return Val
    End Get
  End Property


  Public ReadOnly Property NrCampioniOneTackOnly(Port As Boolean) As Integer
    Get
      If Port Then
        If pCampioniPort Is Nothing Then AggiornaListeTacks()
        Return pCampioniPort.Count
      Else
        If pCampioniStbd Is Nothing Then AggiornaListeTacks()
        Return pCampioniStbd.Count
      End If
    End Get
  End Property

  Private Sub AggiornaListeTacks()
    pCampioniPort = New List(Of Integer)
    pCampioniStbd = New List(Of Integer)
    Dim CT As clsChannel2020 = CanaleTackDefault()
    Dim Lista As List(Of Integer) = IIf(pIndiciFiltrati Is Nothing, pIndici, pIndiciFiltrati)
    For Each Indice In Lista
      If CT.Valori(Indice) < 0 Then
        pCampioniPort.Add(Indice)
      Else
        pCampioniStbd.Add(Indice)
      End If
    Next
  End Sub

  Public ReadOnly Property DettagliCanaleRaggruppamento As clsDettagliCanale
    Get
      Return pDettagliCanaleRaggruppamento
    End Get
  End Property

  Public ReadOnly Property DettagliCanaliSecondari As List(Of clsDettagliCanale)
    Get
      Return pDettagliCanaliSecondari
    End Get
  End Property


  'Public Function IndiciIntervallo() As List(Of Integer)
  '  ' tutto il file
  '  Return IndiciIntervallo(0, dataProvider2020.Dati.Count - 1)
  'End Function

  'Public Function IndiciIntervallo(Intervallo As clsTimeRange) As List(Of Integer)
  '  Dim SecTmp As Double = Intervallo.Inizio.Subtract(dataProvider2020.MomentoPrimaRiga).TotalSeconds
  '  Dim RigaInizio As Long = System.Math.Max(0, SecTmp * dataProvider2020.RawFileHz)
  '  SecTmp = Intervallo.Fine.Subtract(dataProvider2020.MomentoPrimaRiga).TotalSeconds
  '  Dim RigaFine As Long = System.Math.Min(dataProvider2020.Dati.Count, SecTmp * dataProvider2020.RawFileHz)
  '  Return IndiciIntervallo(RigaInizio, RigaFine)
  'End Function

  'Public Function IndiciIntervallo(RigaIniziale As Integer, RigaFinale As Integer) As List(Of Integer)
  '  Dim NuovaLista As New List(Of Integer)
  '  For Each Indice As Integer In pIndici
  '    If Indice >= RigaIniziale AndAlso Indice <= RigaFinale Then
  '      NuovaLista.Add(Indice)
  '    End If
  '  Next
  '  Return NuovaLista
  'End Function

  Public Sub New(DefinizioneGruppo As Double, CanalePrincipale As clsChannel2020, Indici As List(Of Integer))
    pCanalePrincipale = CanalePrincipale
    pDefinizioneGruppo = DefinizioneGruppo
    pIndici = Indici
    Dim Valori As New List(Of Double)
    For Each Indice In pIndici
      Valori.Add(CanalePrincipale.Valori(Indice))
    Next
    pDettagliCanaleRaggruppamento = New clsDettagliCanale(CanalePrincipale, Valori, pIndici)
  End Sub

  Public Function AggiungiCanale(Canale As clsChannel2020, Aggregazione As Double, Intervalli As Integer, Output As clsStatistic.eOutput) As clsDettagliCanale
    If Output = clsStatistic.eOutput.eNegatives OrElse Output = clsStatistic.eOutput.eZeroAndPositives Then Return AggiungiCanaleFiltrato(Canale, Aggregazione, Intervalli, Output)
    pCanaleFiltrato = False
    Dim Valori As New List(Of Double)
    pValoriIndiciFiltrati = New List(Of Double?)
    For Each Indice In pIndici
      Valori.Add(Canale.Valori(Indice))
      pValoriIndiciFiltrati.Add(pCanalePrincipale.Valori(Indice))
    Next
    Dim tmp As New clsDettagliCanale(Canale, Valori, pIndici)
    tmp.AggiungiStatistiche(Aggregazione, Intervalli, Output)
    pDettagliCanaliSecondari.Add(tmp)
    Return tmp
  End Function

  Private Function AggiungiCanaleFiltrato(Canale As clsChannel2020, Aggregazione As Double, Intervalli As Integer, Output As clsStatistic.eOutput) As clsDettagliCanale
    pCanaleFiltrato = True
    Dim Valori As New List(Of Double)
    pIndiciFiltrati = New List(Of Integer)
    pValoriIndiciFiltrati = New List(Of Double?)
    For Each Indice In pIndici
      Dim Valore As Double = Canale.Valori(Indice)
      Select Case Output
        Case clsStatistic.eOutput.eZeroAndPositives
          If Valore >= 0 Then
            pIndiciFiltrati.Add(Indice)
            Valori.Add(Valore)
            pValoriIndiciFiltrati.Add(pCanalePrincipale.Valori(Indice))
          End If
        Case clsStatistic.eOutput.eNegatives
          If Valore < 0 Then
            pIndiciFiltrati.Add(Indice)
            Valori.Add(Valore)
            pValoriIndiciFiltrati.Add(pCanalePrincipale.Valori(Indice))
          End If
      End Select
    Next
    Dim tmp As New clsDettagliCanale(Canale, Valori, pIndici)
    tmp.AggiungiStatistiche(Aggregazione, Intervalli, Output)
    pDettagliCanaliSecondari.Add(tmp)
    Return tmp
  End Function


  'Public Function DettagliCanale(Canale As clsChannel2020, Indici As List(Of Integer)) As clsDettagliCanale
  '  Dim Valori As New List(Of Double)
  '  For Each Indice In Indici
  '    Valori.Add(Canale.Valori(Indice))
  '  Next
  '  Return New clsDettagliCanale(Canale, Valori)
  'End Function


End Class

Public Class clsDettagliCanale
  Dim pCanale As clsChannel2020
  Dim pValori As List(Of Double)
  Dim pValoriCanaleTack As List(Of Double)
  Dim pCanaleTack As clsChannel2020

  Dim pIndici As List(Of Integer)

  Dim pValoriAbs As List(Of Double)
  Dim pValoriTack As List(Of Double)
  Dim pValoriPort As List(Of Double)
  Dim pValoriStbd As List(Of Double)
  'Dim pValoriFiltrati As List(Of Double)
  Dim pStatistics As clsStatistic

  Public ReadOnly Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
  End Property


  Public ReadOnly Property Valori As List(Of Double)
    Get
      Return pValori
    End Get
  End Property

  Public ReadOnly Property ValoriAbs As List(Of Double)
    Get
      If pValoriAbs Is Nothing Then
        pValoriAbs = GetValoriAbs(pValori)
      End If
      Return pValoriAbs
    End Get
  End Property

  Public ReadOnly Property ValoriTack(CurrentChannelStbdPositive As Boolean, CanaleTack As clsChannel2020) As List(Of Double)
    Get
      VerificaValoriCanaleTack(CanaleTack)
      If pValoriTack Is Nothing Then
        pValoriTack = GetValoriTack(CurrentChannelStbdPositive, pValori, pValoriCanaleTack)
      End If
      Return pValoriTack
    End Get
  End Property

  Public ReadOnly Property ValoriPort(CanaleTack As clsChannel2020) As List(Of Double)
    Get
      VerificaValoriCanaleTack(CanaleTack)
      If pValoriPort Is Nothing Then
        pValoriPort = GetValoriPort(pValori, pValoriCanaleTack)
      End If
      Return pValoriPort
    End Get
  End Property

  Public ReadOnly Property ValoriStbd(CanaleTack As clsChannel2020) As List(Of Double)
    Get
      VerificaValoriCanaleTack(CanaleTack)
      If pValoriStbd Is Nothing Then
        pValoriStbd = GetValoriStbd(pValori, pValoriCanaleTack)
      End If
      Return pValoriStbd
    End Get
  End Property


  Public ReadOnly Property Statistics As clsStatistic
    Get
      Return pStatistics
    End Get
    'Set(value As clsStatistic)
    '  pStatistics = value
    'End Set
  End Property

  Public Sub New(Canale As clsChannel2020, Valori As List(Of Double), Indici As List(Of Integer))
    pCanale = Canale
    pValori = Valori
    pIndici = Indici
  End Sub

  Public Sub AggiungiStatistiche(Aggregazione As Double, Intervalli As Integer, Output As clsStatistic.eOutput)
    pStatistics = New clsStatistic(pCanale, Aggregazione, Intervalli, pValori, Output)
  End Sub

  Private Sub VerificaValoriCanaleTack(CanaleTack As clsChannel2020)
    If CanaleTack Is Nothing Then CanaleTack = CanaleTackDefault()
    Dim Aggiorna As Boolean = pValoriCanaleTack Is Nothing
    Aggiorna = Aggiorna OrElse pCanaleTack Is Nothing
    Aggiorna = Aggiorna OrElse (Not pCanaleTack Is CanaleTack)
    pCanaleTack = CanaleTack
    If Aggiorna Then
      pValoriCanaleTack = New List(Of Double)
      For Each indice In pIndici
        pValoriCanaleTack.Add(CanaleTack.Valori(indice))
      Next
    End If

  End Sub

  Private Sub SvuotaListe()
    pValoriAbs = Nothing
    pValoriPort = Nothing
    pValoriStbd = Nothing
    pValoriTack = Nothing
  End Sub

End Class

Public Class clsLinQ
  Dim pRisultatoGroupBy As clsRisultatoGroupBy
  Dim pNIntervalli As Integer = 15

  Public ReadOnly Property NIntervalli As Integer
    Get
      Return pNIntervalli
    End Get
  End Property

  Public Property RisultatoGroupBy As clsRisultatoGroupBy
    Get
      Return pRisultatoGroupBy
    End Get
    Set(value As clsRisultatoGroupBy)
      pRisultatoGroupBy = value
    End Set
  End Property


  'Public Sub TestGroupBy(CanaleAscissa As clsChannel2020, CanaliSecondari As List(Of clsChannel2020), ValoreAggregazione As Double, NIntervalli As Integer)
  '	pNIntervalli = NIntervalli
  '	ValoreAggregazione = ImpostaAggregazione(ValoreAggregazione, CanaleAscissa.Valori.Max, CanaleAscissa.Valori.Min, pNIntervalli)

  '	Dim ValoriAdvanced = CanaleAscissa.Valori.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv}).ToList

  '	'Dim Ranges = (From v In ValoriAdvanced
  '	'              Select New With {Key .range = CInt(v.val), .pos = v.index}).ToList()

  '	Dim Ranges = (From v In ValoriAdvanced
  '								Select New With {Key .range = CInt(v.val / ValoreAggregazione) * ValoreAggregazione, .pos = v.index}).ToList()

  '	Dim group = (From x In Ranges
  '							 Group By Key = x.range
  '												 Into res = Group
  '							 Select New With {Key .gruppo = Key, .indexs = res.Select(Function(x) x.pos).ToList()}).OrderBy(Function(x) x.gruppo).ToList()

  '	pRisultatoGroupBy = New clsRisultatoGroupBy(CanaleAscissa, ValoreAggregazione)

  '	For Each p In group
  '		Dim Raggruppamento As clsValoriGroupBy = pRisultatoGroupBy.AggiungiRaggruppamento(p.gruppo, p.indexs)
  '		For Each Canale As clsChannel2020 In CanaliSecondari
  '			'Dim pAgr As Double = ImpostaAggregazione(-1, Canale.Valori.Max, Canale.Valori.Min, pIntervalli)
  '			Dim Check As clsDettagliCanale = Raggruppamento.AggiungiCanale(Canale, -1, pNIntervalli)
  '			'Raggruppamento.DefinizioneGruppo
  '			'Raggruppamento.AggiungiStisticheCanale(Canale, New clsStatistic(Canale, pAgr, pIntervalli))
  '		Next
  '	Next


  'End Sub


  'Public Sub GroupBy(CanaleAscissa As clsChannel2020, CanaliSecondari As List(Of clsChannel2020), ValoreAggregazione As Double, NIntervalli As Integer, Intervallo As clsTimeRange, Output As clsStatistic.eOutput)
  '	' La funzione va verificata con denis e poi adottata al posto di quella test che non riene conto del timerange corente ma opera su tutto il file
  '	pNIntervalli = NIntervalli
  '	Dim IndiceIniziale, IndiceFinale As Long
  '	IndiciIntervallo(Intervallo, IndiceIniziale, IndiceFinale)



  '	Dim Mvalori = CanaleAscissa.Valori.ToList.GetRange(IndiceIniziale, IndiceFinale - IndiceIniziale)

  '	'ValoreAggregazione = ImpostaAggregazione(ValoreAggregazione, CanaleAscissa.Valori.Max, CanaleAscissa.Valori.Min, pNIntervalli)
  '	'ValoreAggregazione = -1
  '	ValoreAggregazione = ImpostaAggregazione(ValoreAggregazione, Mvalori.Max, Mvalori.Min, pNIntervalli)

  '	Dim ValoriAdvancedTmp = CanaleAscissa.Valori.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv})

  '	Dim ValoriAdvanced = ValoriAdvancedTmp.Where(Function(x) x.index >= IndiceIniziale And x.index <= IndiceFinale).ToList


  '	Dim Ranges = (From v In ValoriAdvanced
  '								Select New With {Key .range = CInt(v.val / ValoreAggregazione) * ValoreAggregazione, .pos = v.index}).ToList()

  '	Dim group = (From x In Ranges
  '							 Group By Key = x.range
  '												 Into res = Group
  '							 Select New With {Key .gruppo = Key, .indexs = res.Select(Function(x) x.pos).ToList()}).OrderBy(Function(x) x.gruppo).ToList()

  '	pRisultatoGroupBy = New clsRisultatoGroupBy(CanaleAscissa, ValoreAggregazione)

  '	For Each p In group
  '		Dim Raggruppamento As clsValoriGroupBy = pRisultatoGroupBy.AggiungiRaggruppamento(p.gruppo, p.indexs)
  '		For Each Canale As clsChannel2020 In CanaliSecondari
  '			Dim Check As clsDettagliCanale = Raggruppamento.AggiungiCanale(Canale, -1, pNIntervalli, Output)
  '		Next
  '	Next


  'End Sub

  Public Sub GroupBy(CanaleAscissa As clsChannel2020, CanaliSecondari As IEnumerable(Of clsChannel2020), ValoreAggregazione As Double, NIntervalli As Integer, Intervallo As clsTimeRange, Output As clsStatistic.eOutput)
    ' La funzione va verificata con denis e poi adottata al posto di quella test che non riene conto del timerange corente ma opera su tutto il file
    pNIntervalli = NIntervalli
    'Dim IndiceIniziale, IndiceFinale As Long
    'IndiciIntervallo(Intervallo, IndiceIniziale, IndiceFinale)
    'If IndiceFinale < IndiceIniziale Then Exit Sub

    If CanaleAscissa.Valori Is Nothing Then Exit Sub

    Dim VnotNan = CanaleAscissa.Valori.Where(Function(x) Not Double.IsNaN(x))

    Dim Mvalori = VnotNan.ToList.ToList.GetRange(Intervallo.IdRigaIniziale, Intervallo.RigheIntervallo) '.Where(Function(x) Not Double.IsNaN(x))

    ValoreAggregazione = ImpostaAggregazione(ValoreAggregazione, Mvalori.Max, Mvalori.Min, pNIntervalli)

    'Dim ValoriAdvancedTmp = CanaleAscissa.Valori.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv})
    Dim ValoriAdvancedTmp = Mvalori.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv})


    ' qui prende i soli valori compresi tra indice iniziale ed indice finale (mi sembra una ripetizione...)
    Dim ValoriAdvanced = ValoriAdvancedTmp.Where(Function(x) x.index >= Intervallo.IdRigaIniziale And x.index <= Intervallo.IdRigaFinale).ToList


    Dim Ranges = (From v In ValoriAdvanced
                  Select New With {Key .range = CInt(v.val / ValoreAggregazione) * ValoreAggregazione, .pos = v.index}).ToList()

    Dim group = (From x In Ranges
                 Group By Key = x.range
                           Into res = Group
                 Select New With {Key .gruppo = Key, .indexs = res.Select(Function(x) x.pos).ToList()}).OrderBy(Function(x) x.gruppo).ToList()

    pRisultatoGroupBy = New clsRisultatoGroupBy(CanaleAscissa, ValoreAggregazione)

    For Each p In group
      Dim Raggruppamento As clsValoriGroupBy = pRisultatoGroupBy.AggiungiRaggruppamento(p.gruppo, p.indexs)
      For Each Canale As clsChannel2020 In CanaliSecondari
        Dim Check As clsDettagliCanale = Raggruppamento.AggiungiCanale(Canale, -1, pNIntervalli, Output)
      Next
    Next


  End Sub



End Class

Public Class clsStatistic
  Dim pCanale As clsChannel2020
  Dim pValori As List(Of Double)
	Dim pAggregazione As Double
	Dim pRightOfMode90 As Double
	Dim pRightOfMode70 As Double
	Dim pRightOfMode50 As Double
	Dim pLeftOfMode90 As Double
	Dim pLeftOfMode70 As Double
	Dim pLeftOfMode50 As Double
	Dim pRisultatoGroupBy As clsRisultatoGroupBy
	Dim pMedian As Double
	Dim pMode As List(Of Double)
	Dim pStandardDeviation As Double
	Dim pIntervalli As Integer
	Dim pOutput As eOutput

	Public Enum eOutput
		eNormal = 0
		eAbsolute = 1
		eZeroAndPositives = 2
		eNegatives = 3
	End Enum


  Public Sub New(Canale As clsChannel2020, Aggregazione As Double, Intervalli As Integer, Valori As List(Of Double), Output As eOutput)
    pCanale = Canale
    pValori = Valori
    pIntervalli = Intervalli
    pOutput = Output
    If pValori.Count = 0 OrElse (pValori.Max - pValori.Min) Then
      pAggregazione = 1
    Else
      pAggregazione = ImpostaAggregazione(Aggregazione, pValori.Max, pValori.Min, pIntervalli)
    End If
    'If pAggregazione = 0 Then Stop

    ImpostaStatistiche()
  End Sub

  Public Property Intervalli As Integer
		Get
			Return pIntervalli
		End Get
		Set(value As Integer)
			pIntervalli = value
		End Set
	End Property

	Public ReadOnly Property Output As eOutput
		Get
			Return pOutput
		End Get
	End Property

  Public ReadOnly Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
  End Property

  Public ReadOnly Property ValoriAvg As Double
		Get
			If pValori Is Nothing OrElse pValori.Count = 0 Then Return 0
			Return pValori.Average
		End Get
	End Property

	Public ReadOnly Property ValoriMax As Double
		Get
			If pValori Is Nothing OrElse pValori.Count = 0 Then Return 0
			Return pValori.Max
		End Get
	End Property

	Public ReadOnly Property ValoriMin As Double
		Get
			If pValori Is Nothing OrElse pValori.Count = 0 Then Return 0
			Return pValori.Min
		End Get
	End Property

	Public ReadOnly Property Valori As List(Of Double)
		Get
			Return pValori
		End Get
	End Property

	Public ReadOnly Property Aggregazione As Double
		Get
			Return pAggregazione
		End Get
	End Property

	Public ReadOnly Property RightOfMode90 As Double
		Get
			Return pRightOfMode90
		End Get
	End Property

	Public ReadOnly Property RightOfMode70 As Double
		Get
			Return pRightOfMode70
		End Get
	End Property

	Public ReadOnly Property RightOfMode50 As Double
		Get
			Return pRightOfMode50
		End Get
	End Property

	Public ReadOnly Property LeftOfMode90 As Double
		Get
			Return pLeftOfMode90
		End Get
	End Property

	Public ReadOnly Property LeftOfMode70 As Double
		Get
			Return pLeftOfMode70
		End Get
	End Property

	Public ReadOnly Property LeftOfMode50 As Double
		Get
			Return pLeftOfMode50
		End Get
	End Property

	Public ReadOnly Property RisultatoGroupBy As clsRisultatoGroupBy
		Get
			Return pRisultatoGroupBy
		End Get
	End Property

	Public ReadOnly Property Median As Double
		Get
			Return pMedian
		End Get
	End Property

	Public ReadOnly Property Mode As List(Of Double)
		Get
			Return pMode
		End Get
	End Property

	Public ReadOnly Property ModaMedia As Double
		Get
			Dim ModeAvg As Double = 0
			For Each Moda As Double In pMode
				ModeAvg += Moda
			Next
			Return ModeAvg / pMode.Count
		End Get
	End Property

	Public ReadOnly Property StandardDeviation As Double
		Get
			Return pStandardDeviation
		End Get
	End Property

  Private Sub ImpostaStatistiche()
    Dim ValoriTmp = pValori.Where(Function(x) Not Double.IsNaN(x))

    Dim ValoriAdvanced = ValoriTmp.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv}).ToList
    Select Case pOutput
      Case eOutput.eAbsolute
        ValoriAdvanced = ValoriTmp.Select(Function(ValoreAdv, IndiciAdv) New With {.val = Math.Abs(ValoreAdv), .index = IndiciAdv}).ToList
      Case eOutput.eZeroAndPositives
        ValoriAdvanced = ValoriTmp.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv}).Where(Function(x) x.val >= 0).ToList
      Case eOutput.eNegatives
        ValoriAdvanced = ValoriTmp.Select(Function(ValoreAdv, IndiciAdv) New With {.val = ValoreAdv, .index = IndiciAdv}).Where(Function(x) x.val < 0).Select(Function(v) New With {.val = Math.Abs(v.val), .index = v.index}).ToList

    End Select



    Dim Ranges = (From v In ValoriAdvanced
                  Select New With {Key .range = CInt(v.val / pAggregazione) * pAggregazione, .pos = v.index}).ToList()

    Dim group = (From x In Ranges
                 Group By Key = x.range
                     Into res = Group
                 Select New With {Key .gruppo = Key, .indexs = res.Select(Function(x) x.pos).ToList()}).OrderBy(Function(x) x.gruppo).ToList()


    pRisultatoGroupBy = New clsRisultatoGroupBy(pCanale, pAggregazione)
    For Each p In group
      Dim Raggruppamento As clsValoriGroupBy = pRisultatoGroupBy.AggiungiRaggruppamento(p.gruppo, p.indexs)
    Next

    Dim step1 = (From x In group
                 Select x.gruppo, x.indexs.Count()
                              )
    Dim step2 = (From x In step1.OrderByDescending(Function(i) i.Count)
                 Select x.gruppo, x.Count).ToList()




    pMode = New List(Of Double)
    Dim avgMode As Double
    Dim MaxSamples As Double
    For i As Integer = 0 To step2.Count - 1
      Dim Samples As Double = step2(i).Count
      If i = 0 Then
        MaxSamples = Samples
        pMode.Add(step2(i).gruppo)
        avgMode = step2(i).gruppo
      Else
        If Samples = MaxSamples Then
          ' viene aggiunto un gruppo alla lista se i samples sono gli stessi del gruppo con più samples
          pMode.Add(step2(i).gruppo)
          For Each m As Double In pMode
            avgMode += m / pMode.Count
          Next
        End If
      End If
    Next

    For i As Integer = 0 To step2.Count - 1
      Dim Samples As Double = step2(i).Count
      If i = 0 Then
        pRightOfMode90 = step2(i).gruppo
        pRightOfMode70 = step2(i).gruppo
        pRightOfMode50 = step2(i).gruppo
        pLeftOfMode90 = step2(i).gruppo
        pLeftOfMode70 = step2(i).gruppo
        pLeftOfMode50 = step2(i).gruppo
      Else
        If step2(i).gruppo >= avgMode Then
          If Samples >= MaxSamples * 0.9 Then
            pRightOfMode90 = System.Math.Max(pRightOfMode90, step2(i).gruppo)
            pRightOfMode70 = pRightOfMode90
            pRightOfMode50 = pRightOfMode90
          ElseIf Samples >= MaxSamples * 0.7 Then
            pRightOfMode70 = System.Math.Max(pRightOfMode70, step2(i).gruppo)
            pRightOfMode50 = pRightOfMode70
          ElseIf Samples >= MaxSamples * 0.5 Then
            pRightOfMode50 = System.Math.Max(pRightOfMode50, step2(i).gruppo)
          End If
        Else
          If Samples >= MaxSamples * 0.9 Then
            pLeftOfMode90 = System.Math.Min(pLeftOfMode90, step2(i).gruppo)
            pLeftOfMode70 = pLeftOfMode90
            pLeftOfMode50 = pLeftOfMode90
          ElseIf Samples >= MaxSamples * 0.7 Then
            pLeftOfMode70 = System.Math.Min(pLeftOfMode70, step2(i).gruppo)
            pLeftOfMode50 = pLeftOfMode70
          ElseIf Samples >= MaxSamples * 0.5 Then
            pLeftOfMode50 = System.Math.Min(pLeftOfMode50, step2(i).gruppo)
          End If
        End If

      End If
    Next

    'pStandardDeviation = TAlex.MathCore.Statistics.MathStats.PopulationStandardDeviation(ValoriTmp)
    alglib.basestat.sampleadev(ValoriTmp.ToArray, ValoriTmp.Count, pStandardDeviation)

    If ValoriTmp.Count = 0 Then
      pMedian = 0
    Else
      alglib.basestat.samplemedian(ValoriTmp.ToArray, ValoriTmp.Count, pMedian)
      'pMedian = TAlex.MathCore.Statistics.MathStats.Median(ValoriTmp)
    End If

    'Dim a As New MathNet.Numerics.Statistics.DescriptiveStatistics(ValoriTmp)

  End Sub


End Class

