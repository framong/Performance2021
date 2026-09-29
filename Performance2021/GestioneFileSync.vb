Imports SPwpf

'Public Class clsFileSyncAndCorrected
'	Dim pFilePrincipale As clsSingoloFile
'	Dim pFileSecondario As clsSingoloFile
'	Dim DizionarioIndici As New Dictionary(Of Integer, Integer)


'	Public Sub New()
'		CaricaFiles()
'		AccodaTuttiCanali()
'		'CreaFileCorretto()
'	End Sub

'	Private Sub CaricaFiles()
'		' seleziona i file principali dai quali ricreare il principale 
'		Dim PathFilePrincipale As String = MergeFileDati(SelezionaFileDati("Select Primary Files"))
'		' seleziona i file secondari dai quali prende i valori da mettere nel principale
'		Dim PathFileSecondario As String = MergeFileDati(SelezionaFileDati("Select Secondary Files"))
'		If Not PathFilePrincipale Is Nothing Then pFilePrincipale = New clsSingoloFile(PathFilePrincipale)
'		If Not PathFileSecondario Is Nothing Then pFileSecondario = New clsSingoloFile(PathFileSecondario)
'	End Sub


'	'Private Sub CreaFileCorretto()
'	'	Dim Adesso As Date = Now
'	'	Console.WriteLine("CreaFileCorretto 0")

'	'	DizionarioIndici.Clear()
'	'	AggiungiCoppiaIndiciCanale("SystemTime_DaySeconds", "SystemTime_DaySeconds")
'	'	AggiungiCoppiaIndiciCanale("LatBow", "UBX2_NAV_PVT_Lat_deg")
'	'	AggiungiCoppiaIndiciCanale("LonBow", "UBX2_NAV_PVT_Lon_deg")
'	'	AggiungiCoppiaIndiciCanale("Cog", "UBX2_NAV_PVT_COG_deg")
'	'	AggiungiCoppiaIndiciCanale("Sog", "UBX2_NAV_PVT_SOG_Knots")
'	'	AggiungiCoppiaIndiciCanale("Bs", "UBX2_NAV_PVT_SOG_Knots")
'	'	AggiungiCoppiaIndiciCanale("Twd", "none")
'	'	AggiungiCoppiaIndiciCanale("Tws", "none")
'	'	AggiungiCoppiaIndiciCanale("Cse", "none")
'	'	AggiungiCoppiaIndiciCanale("Twa", "none")
'	'	AggiungiCoppiaIndiciCanale("Awa", "none")
'	'	AggiungiCoppiaIndiciCanale("Aws", "none")
'	'	Dim RigheDaEsportare As New List(Of String)
'	'	RigheDaEsportare.Add(pFilePrincipale.RigheRaw(0))
'	'	Dim RigheDaEsportare1Hz As New List(Of String)
'	'	RigheDaEsportare1Hz.Add(pFilePrincipale.RigheRaw(0))
'	'	Dim OffsetSecondario As Integer = pFilePrincipale.RangeRaw.Inizio.Subtract(pFileSecondario.RangeRaw.Inizio).TotalSeconds * pFileSecondario.Hz
'	'	Dim IndiceSec As Integer = 0
'	'	Dim idRel As Integer = 0

'	'	'Dim indiceprova As Integer = 320000
'	'	'Dim idp As Integer = pFilePrincipale.Indici(indiceprova)
'	'	'Dim RP As New clsSingolaRiga(pFilePrincipale.RigheRaw(idp), vbTab)
'	'	'IndiceSec = indiceprova + OffsetSecondario
'	'	'Dim ids As Integer = pFileSecondario.Indici(IndiceSec)
'	'	'Dim RS As New clsSingolaRiga(pFileSecondario.RigheRaw(ids), vbTab)
'	'	'Dim sP As Double = RP.ValoreCanali(DizionarioIndici.Keys(0))
'	'	'Dim sS As Double = RS.ValoreCanali(DizionarioIndici.Valori(0))

'	'	Dim idxp As Integer = 0
'	'	Dim idxs As Integer = 0

'	'	Dim FI As New System.IO.FileInfo(pFilePrincipale.PathFile)
'	'	Dim Cartella As String = FI.FullName.Replace(FI.Name, "")
'	'	Dim RadiceNome As String = FI.Name.Replace(FI.Extension, "")
'	'	Dim NuovoFile As String = Cartella & RadiceNome & "_Corrected" & FI.Extension
'	'	Dim NuovoFile1Hz As String = Cartella & RadiceNome & "_1Hz_Corrected" & FI.Extension
'	'	Dim contatore As Integer = 0
'	'	Dim RigaPrincipale As clsSingolaRiga = Nothing
'	'	Dim RigaSecondario As clsSingolaRiga = Nothing
'	'	Console.WriteLine("CreaFileCorretto PreLoop: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	For i As Integer = 0 To pFilePrincipale.Indici.Count - 1
'	'		idxp = pFilePrincipale.Indici(i)
'	'		RigaPrincipale = pFilePrincipale.IndiciAdvanced.Valori(i)
'	'		IndiceSec = i + OffsetSecondario
'	'		If IndiceSec >= pFileSecondario.Indici.First AndAlso IndiceSec <= pFileSecondario.Indici.Last Then
'	'			idxs = pFileSecondario.Indici(IndiceSec)
'	'			RigaSecondario = pFileSecondario.IndiciAdvanced.Valori(IndiceSec)
'	'			If IsNumeric(RigaSecondario.ValoreCanali(0)) Then
'	'				'Dim SecPrinc As Double = RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(0))
'	'				'Dim SecSec As Double = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(0))
'	'				'Dim delta As Double = SecPrinc - SecSec
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(1)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(1))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(2)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(2))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(3)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(3))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(4)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(4))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(5)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(5))
'	'				'Else
'	'				'	Stop
'	'			End If
'	'		End If
'	'		'RigheDaEsportare.Add(RigaPrincipale.ValoreCanali.ToString)
'	'		RigheDaEsportare.Add(RigaPrincipale.StringaAggiornata)
'	'		idRel += 1
'	'		If idRel = pFilePrincipale.Hz Then
'	'			RigheDaEsportare1Hz.Add(RigheDaEsportare.Last)
'	'			idRel = 0
'	'		End If
'	'		If contatore = 100000 Then
'	'			Console.WriteLine("CreaFileCorretto Pre Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'			If System.IO.File.Exists(NuovoFile) Then
'	'				System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'	'			Else
'	'				System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'	'			End If
'	'			If System.IO.File.Exists(NuovoFile1Hz) Then
'	'				System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'			Else
'	'				System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'			End If
'	'			RigheDaEsportare.Clear()
'	'			RigheDaEsportare1Hz.Clear()
'	'			contatore = 0
'	'			Console.WriteLine("CreaFileCorretto Post Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'		End If
'	'		contatore += 1
'	'	Next
'	'	Console.WriteLine("CreaFileCorretto Pre Last Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	If System.IO.File.Exists(NuovoFile) Then
'	'		System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'	'	Else
'	'		System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'	'	End If
'	'	If System.IO.File.Exists(NuovoFile1Hz) Then
'	'		System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'	Else
'	'		System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'	End If
'	'	Console.WriteLine("CreaFileCorretto Fine: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	MsgBox("Done!")
'	'End Sub


'	'Private Sub CreaFileCorretto()
'	'	Dim Adesso As Date = Now
'	'	Console.WriteLine("CreaFileCorretto 0")

'	'	DizionarioIndici.Clear()
'	'	AggiungiCoppiaIndiciCanale("SystemTime_DaySeconds", "SystemTime_DaySeconds")
'	'	AggiungiCoppiaIndiciCanale("LatBow", "UBX2_NAV_PVT_Lat_deg")
'	'	AggiungiCoppiaIndiciCanale("LonBow", "UBX2_NAV_PVT_Lon_deg")
'	'	AggiungiCoppiaIndiciCanale("Cog", "UBX2_NAV_PVT_COG_deg")
'	'	AggiungiCoppiaIndiciCanale("Sog", "UBX2_NAV_PVT_SOG_Knots")
'	'	AggiungiCoppiaIndiciCanale("Bs", "UBX2_NAV_PVT_SOG_Knots")
'	'	AggiungiCoppiaIndiciCanale("Twd", "none")
'	'	AggiungiCoppiaIndiciCanale("Tws", "none")
'	'	AggiungiCoppiaIndiciCanale("Cse", "none")
'	'	AggiungiCoppiaIndiciCanale("Twa", "none")
'	'	AggiungiCoppiaIndiciCanale("Awa", "none")
'	'	AggiungiCoppiaIndiciCanale("Aws", "none")
'	'	Dim RigheDaEsportare As New List(Of String)
'	'	RigheDaEsportare.Add(pFilePrincipale.RigheRaw(0))
'	'	Dim RigheDaEsportare1Hz As New List(Of String)
'	'	RigheDaEsportare1Hz.Add(pFilePrincipale.RigheRaw(0))
'	'	Dim OffsetSecondario As Integer = pFilePrincipale.RangeRaw.Inizio.Subtract(pFileSecondario.RangeRaw.Inizio).TotalSeconds * pFileSecondario.Hz
'	'	Dim IndiceSec As Integer = 0
'	'	Dim idRel As Integer = 0

'	'	'Dim indiceprova As Integer = 320000
'	'	'Dim idp As Integer = pFilePrincipale.Indici(indiceprova)
'	'	'Dim RP As New clsSingolaRiga(pFilePrincipale.RigheRaw(idp), vbTab)
'	'	'IndiceSec = indiceprova + OffsetSecondario
'	'	'Dim ids As Integer = pFileSecondario.Indici(IndiceSec)
'	'	'Dim RS As New clsSingolaRiga(pFileSecondario.RigheRaw(ids), vbTab)
'	'	'Dim sP As Double = RP.ValoreCanali(DizionarioIndici.Keys(0))
'	'	'Dim sS As Double = RS.ValoreCanali(DizionarioIndici.Valori(0))

'	'	Dim idxp As Integer = 0
'	'	Dim idxs As Integer = 0

'	'	Dim FI As New System.IO.FileInfo(pFilePrincipale.PathFile)
'	'	Dim Cartella As String = FI.FullName.Replace(FI.Name, "")
'	'	Dim RadiceNome As String = FI.Name.Replace(FI.Extension, "")
'	'	Dim NuovoFile As String = Cartella & RadiceNome & "_Corrected" & FI.Extension
'	'	Dim NuovoFile1Hz As String = Cartella & RadiceNome & "_1Hz_Corrected" & FI.Extension
'	'	Dim contatore As Integer = 0
'	'	Console.WriteLine("CreaFileCorretto PreLoop: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	For i As Integer = 0 To pFilePrincipale.Indici.Count - 1
'	'		idxp = pFilePrincipale.Indici(i)
'	'		Dim RigaPrincipale As New clsSingolaRiga(pFilePrincipale.RigheRaw(idxp), vbTab)
'	'		IndiceSec = i + OffsetSecondario
'	'		If IndiceSec >= pFileSecondario.Indici.First AndAlso IndiceSec <= pFileSecondario.Indici.Last Then
'	'			idxs = pFileSecondario.Indici(IndiceSec)
'	'			Dim RigaSecondario As New clsSingolaRiga(pFileSecondario.RigheRaw(idxs), vbTab)
'	'			If IsNumeric(RigaSecondario.ValoreCanali(0)) Then
'	'				Dim SecPrinc As Double = RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(0))
'	'				Dim SecSec As Double = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(0))
'	'				Dim delta As Double = SecPrinc - SecSec
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(1)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(1))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(2)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(2))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(3)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(3))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(4)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(4))
'	'				RigaPrincipale.ValoreCanali(DizionarioIndici.Keys(5)) = RigaSecondario.ValoreCanali(DizionarioIndici.Valori(5))
'	'				'Else
'	'				'	Stop
'	'			End If
'	'		End If
'	'		'RigheDaEsportare.Add(RigaPrincipale.ValoreCanali.ToString)
'	'		RigheDaEsportare.Add(RigaPrincipale.StringaAggiornata)
'	'		idRel += 1
'	'		If idRel = pFilePrincipale.Hz Then
'	'			RigheDaEsportare1Hz.Add(RigheDaEsportare.Last)
'	'			idRel = 0
'	'		End If
'	'		If contatore = 100000 Then
'	'			Console.WriteLine("CreaFileCorretto Pre Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'			If System.IO.File.Exists(NuovoFile) Then
'	'				System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'	'			Else
'	'				System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'	'			End If
'	'			If System.IO.File.Exists(NuovoFile1Hz) Then
'	'				System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'			Else
'	'				System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'			End If
'	'			RigheDaEsportare.Clear()
'	'			RigheDaEsportare1Hz.Clear()
'	'			contatore = 0
'	'			Console.WriteLine("CreaFileCorretto Post Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'		End If
'	'		contatore += 1
'	'	Next
'	'	Console.WriteLine("CreaFileCorretto Pre Last Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	If System.IO.File.Exists(NuovoFile) Then
'	'		System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'	'	Else
'	'		System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'	'	End If
'	'	If System.IO.File.Exists(NuovoFile1Hz) Then
'	'		System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'	Else
'	'		System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'	'	End If
'	'	Console.WriteLine("CreaFileCorretto Fine: " & Now.Subtract(Adesso).TotalMilliseconds)
'	'	MsgBox("Done!")
'	'End Sub

'	Private Sub AccodaTuttiCanali()
'		Dim Adesso As Date = Now
'		'Console.WriteLine("CreaFileCorretto 0")

'		DizionarioIndici.Clear()
'		AggiungiCoppiaIndiciCanale("SystemTime_DaySeconds", "SystemTime_DaySeconds")
'		Dim RigheDaEsportare As New List(Of String)
'		Dim RigheDaEsportare1Hz As New List(Of String)
'		Dim OffsetSecondario As Integer = pFilePrincipale.RangeRaw.Start.Subtract(pFileSecondario.RangeRaw.Start).TotalSeconds * pFileSecondario.Hz
'		Dim IndiceSec As Integer = 0
'		Dim idRel As Integer = 0

'		'Dim indiceprova As Integer = 320000
'		'Dim idp As Integer = pFilePrincipale.Indici(indiceprova)
'		'Dim RP As New clsSingolaRiga(pFilePrincipale.RigheRaw(idp), vbTab)
'		'IndiceSec = indiceprova + OffsetSecondario
'		'Dim ids As Integer = pFileSecondario.Indici(IndiceSec)
'		'Dim RS As New clsSingolaRiga(pFileSecondario.RigheRaw(ids), vbTab)
'		'Dim sP As Double = RP.ValoreCanali(DizionarioIndici.Keys(0))
'		'Dim sS As Double = RS.ValoreCanali(DizionarioIndici.Valori(0))

'		Dim CanaliFileSecondario As New List(Of Integer)
'		Dim StringaDaAccodare As String = ""
'		Dim Intestazioni As String() = pFileSecondario.RigheRaw(0).Split(vbTab)
'		For i As Integer = 0 To pFileSecondario.RigheRaw(0).Split(vbTab).Count - 1
'			Dim Canale As String = pFileSecondario.RigheRaw(0).Split(vbTab)(i)
'			If pFilePrincipale.RigheRaw(0).IndexOf(Canale) > -1 Then
'				'Dim Aggiungi As Boolean = True
'				'For Each ChTmp As String In pFilePrincipale.RigheRaw(0).Split(vbTab)
'				'	If ChTmp = Canale Then
'				'		Aggiungi = False
'				'		Exit For
'				'	End If
'				'Next
'				'If Aggiungi Then
'				'	pFilePrincipale.RigheRaw(0) &= pFileSecondario.RigheRaw(0).Split(vbTab)(i) & vbTab
'				'	CanaliFileSecondario.Add(i)
'				'End If
'			Else
'				StringaDaAccodare &= vbTab & Intestazioni(i)
'				CanaliFileSecondario.Add(i)
'			End If
'		Next
'		RigheDaEsportare.Add(pFilePrincipale.RigheRaw(0).TrimEnd(vbTab) & StringaDaAccodare)
'		RigheDaEsportare1Hz.Add(RigheDaEsportare.Last)
'		'RigheDaEsportare1Hz.Add(pFilePrincipale.RigheRaw(0).TrimEnd(vbTab) & vbTab & StringaDaAccodare.TrimEnd(vbTab))
'		Clipboard.SetText(RigheDaEsportare.Last)
'		Dim idxp As Integer = 0
'		Dim idxs As Integer = 0

'		Dim FI As New System.IO.FileInfo(pFilePrincipale.PathFile)
'		Dim Cartella As String = FI.FullName.Replace(FI.Name, "")
'		Dim RadiceNome As String = FI.Name.Replace(FI.Extension, "")
'		Dim NuovoFile As String = Cartella & RadiceNome & "_Corrected" & FI.Extension
'		Dim NuovoFile1Hz As String = Cartella & RadiceNome & "_1Hz_Corrected" & FI.Extension
'		Dim contatore As Integer = 0
'		'Console.WriteLine("CreaFileCorretto PreLoop: " & Now.Subtract(Adesso).TotalMilliseconds)
'		For i As Integer = 0 To pFilePrincipale.Indici.Count - 1
'			idxp = pFilePrincipale.Indici(i)
'			Dim RigaPrincipale As New clsSingolaRiga(pFilePrincipale.RigheRaw(idxp), vbTab)
'			IndiceSec = i + OffsetSecondario
'			If IndiceSec >= pFileSecondario.Indici.First AndAlso IndiceSec <= pFileSecondario.Indici.Last Then
'				idxs = pFileSecondario.Indici(IndiceSec)
'				Dim RigaSecondario As New clsSingolaRiga(pFileSecondario.RigheRaw(idxs), vbTab)
'				StringaDaAccodare = ""
'				If IsNumeric(RigaSecondario.ValoreCanali(0)) Then
'					For Each Id In CanaliFileSecondario
'						'StringaDaAccodare &= RigaSecondario.Valori(Id) & vbTab
'						StringaDaAccodare &= vbTab & RigaSecondario.ValoreCanali(Id)
'						'pFilePrincipale.RigheRaw(idxp) &= RigaSecondario.ValoreCanali(Id) & vbTab
'						'pFilePrincipale.RigheRaw(idxp) &= pFileSecondario.RigheRaw(idxs).Split(vbTab)(Id) & vbTab
'					Next
'				End If
'				RigheDaEsportare.Add(RigaPrincipale.StringaAggiornata.TrimEnd(vbTab) & StringaDaAccodare)
'				'Clipboard.SetText(RigheDaEsportare.Last)
'				'RigheDaEsportare.Add(RigaPrincipale.StringaAggiornata)
'				idRel += 1
'				If idRel = pFilePrincipale.Hz Then
'					RigheDaEsportare1Hz.Add(RigheDaEsportare.Last)
'					'Clipboard.SetText(RigheDaEsportare.Last)
'					idRel = 0
'				End If
'			End If
'			If contatore = 100000 Then
'				'Console.WriteLine("CreaFileCorretto Pre Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'				If System.IO.File.Exists(NuovoFile) Then
'					System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'				Else
'					System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'				End If
'				If System.IO.File.Exists(NuovoFile1Hz) Then
'					System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'				Else
'					System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'				End If
'				RigheDaEsportare.Clear()
'				RigheDaEsportare1Hz.Clear()
'				contatore = 0
'				'Console.WriteLine("CreaFileCorretto Post Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'			End If
'			contatore += 1
'		Next
'		'Console.WriteLine("CreaFileCorretto Pre Last Flush: " & Now.Subtract(Adesso).TotalMilliseconds)
'		If System.IO.File.Exists(NuovoFile) Then
'			System.IO.File.AppendAllLines(NuovoFile, RigheDaEsportare)
'		Else
'			System.IO.File.WriteAllLines(NuovoFile, RigheDaEsportare)
'		End If
'		If System.IO.File.Exists(NuovoFile1Hz) Then
'			System.IO.File.AppendAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'		Else
'			System.IO.File.WriteAllLines(NuovoFile1Hz, RigheDaEsportare1Hz)
'		End If
'		'Console.WriteLine("CreaFileCorretto Fine: " & Now.Subtract(Adesso).TotalMilliseconds)
'		MsgBox("Done!")
'	End Sub




'	Private Sub AggiungiCoppiaIndiciCanale(CanalePrincipale As String, CanaleSecondario As String)
'		DizionarioIndici.Add(pFilePrincipale.IdCampo(CanalePrincipale), pFileSecondario.IdCampo(CanaleSecondario))
'	End Sub


'	Private Function MergeFileDati(pathFiles As List(Of String)) As String
'		If pathFiles Is Nothing Then Return Nothing
'		Dim pathFile As String = pathFiles.First
'		If pathFiles.Count > 1 Then ' bisogna aggiungere che la cosa è possibile solo sui file di testo
'			'crea un file di testo unico accodando tutte le righe dei file selezionati
'			pathFiles.Sort()
'			Dim Righe As New List(Of String)
'			For Each FileStr As String In pathFiles
'				Righe.AddRange(System.IO.File.ReadAllLines(FileStr))
'			Next
'			Dim FI As New System.IO.FileInfo(pathFiles.First)
'			Dim Cartella As String = FI.FullName.Replace(FI.Name, "")
'			Dim RadiceNome As String = FI.Name.Replace(FI.Extension, "")
'			pathFile = Cartella & RadiceNome.Split("_")(0) & "_" & RadiceNome.Split("_")(1) & "_" & TempoInStringaFormattata(Now, eFormatType.YYYYMMDDHHMMSS) & FI.Extension
'			System.IO.File.WriteAllLines(pathFile, Righe)
'		End If
'		Return pathFile
'	End Function

'	Private Function SelezionaFileDati(Titolo As String) As List(Of String)
'    Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LogUtility", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
'    Dim LastExt As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LogUtility", "LastExt", "txt", True, True)

'		'Dim SelectedFile As String = objFiles.SelezionaFile(UltimoPath, "Select Source File", "", "", "")
'		Dim SelFileNames As New List(Of String)
'		Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, Titolo, "Source File |*.txt;*.csv;*.db;*.ppf|All Files|*.*", LastExt, SelFileNames)
'		If Not SelectedFiles Is Nothing Then
'			AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LogUtility", "LastUsed", GetFileInfo(SelectedFiles.First).Directory.FullName, True, True)
'			AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LogUtility", "LastExt", GetFileInfo(SelectedFiles.First).Extension, True, True)
'		End If
'		Return SelectedFiles
'	End Function


'End Class


'Public Class clsSingoloFile
'	Dim pPathFile As String
'	Dim pRigheRaw As New List(Of String)
'	Dim pIntestazioni As List(Of String)
'	Dim pHz As Integer
'	Dim pIndici As Integer()
'	Dim pIndiciAdvanced As New Dictionary(Of Integer, clsSingolaRiga)
'	'Dim pRighe As clsSingolaRiga()
'	Dim pRangeRaw As clsTimeRange
'	'Dim pRangeImportazione As clsTimeRange
'	Dim pIdDate As Integer
'	Dim pIdTime As Integer
'	Dim pIdDaySec As Integer
'	Dim pGiornoFileRaw As Date

'	Public Property RangeRaw As clsTimeRange
'		Get
'			Return pRangeRaw
'		End Get
'		Set(value As clsTimeRange)
'			pRangeRaw = value
'		End Set
'	End Property

'	Public Property Indici As Integer()
'		Get
'			Return pIndici
'		End Get
'		Set(value As Integer())
'			pIndici = value
'		End Set
'	End Property

'	Public Property RigheRaw As List(Of String)
'		Get
'			Return pRigheRaw
'		End Get
'		Set(value As List(Of String))
'			pRigheRaw = value
'		End Set
'	End Property

'	Public Property Hz As Integer
'		Get
'			Return pHz
'		End Get
'		Set(value As Integer)
'			pHz = value
'		End Set
'	End Property

'	Public Property PathFile As String
'		Get
'			Return pPathFile
'		End Get
'		Set(value As String)
'			pPathFile = value
'		End Set
'	End Property

'	Public Property IndiciAdvanced As Dictionary(Of Integer, clsSingolaRiga)
'		Get
'			Return pIndiciAdvanced
'		End Get
'		Set(value As Dictionary(Of Integer, clsSingolaRiga))
'			pIndiciAdvanced = value
'		End Set
'	End Property

'	Public Sub New(PathFile As String)
'		pPathFile = PathFile
'		pRigheRaw.AddRange(System.IO.File.ReadAllLines(pPathFile))
'		pIntestazioni = pRigheRaw(0).Split(vbTab).ToList
'		pIdDate = IdCampo("SystemTime_Date")
'		pIdTime = IdCampo("SystemTime_Local")
'		pIdDaySec = IdCampo("SystemTime_DaySeconds")
'		If pIdDate = -1 OrElse pIdTime = -1 Then Stop
'		' verifica gli Hz del log
'		Dim TestRighe As Integer = 1000
'		TestRighe = System.Math.Min(TestRighe, pRigheRaw.Count / 2)
'		Dim HzA, HzB, HzC As Double
'		HzA = SecondiTotali(100, TestRighe)
'		HzB = SecondiTotali((pRigheRaw.Count - 2) / 2, TestRighe)
'		HzC = SecondiTotali((pRigheRaw.Count - TestRighe), TestRighe)
'		pHz = (HzA + HzB + HzC) / 3
'		NormalizzaHz()
'		pRangeRaw = New clsTimeRange(DataTimeDaRaw(1), DataTimeDaRaw(pRigheRaw.Count - 1))
'		'pRangeRaw.ImpostaRigheRaw(1, pRigheRaw.Count - 1)
'		'MappaRigheDT()

'		pGiornoFileRaw = New Date(pRangeRaw.Start.Year, pRangeRaw.Start.Month, pRangeRaw.Start.Day, 0, 0, 0, 0)
'		MappaRigheDTfast()

'	End Sub

'	Private Function SecondiTotali(RigaIniziale As Long, Righe As Integer) As Double
'		Dim Inizio As DateTime = DataTimeDaRaw(RigaIniziale)
'		Dim RowFin As Long = System.Math.Min(RigaIniziale + Righe, pRigheRaw.Count - 2)
'		Dim Fine As DateTime = DataTimeDaRaw(RowFin)
'		Return Righe / Fine.Subtract(Inizio).TotalSeconds
'	End Function

'	Private Function DataTimeDaRaw(Riga As Integer) As DateTime
'		'Dim Valori As List(Of String) = pRigheRaw(Riga).Split(vbTab).ToList
'		Dim Valori As String() = pRigheRaw(Riga).Split(vbTab)
'		Return FaroDateTimeToSystemDT(Valori(pIdDate), Valori(pIdTime))
'	End Function

'	Private Function FastDataTimeDaRaw(Riga As Integer) As DateTime
'		'Dim Valori As List(Of String) = pRigheRaw(Riga).Split(vbTab).ToList
'		Dim SecFromMidnight As String = pRigheRaw(Riga).Split(vbTab)(pIdDaySec)
'		If Not IsNumeric(SecFromMidnight) Then Return Nothing
'		Return pGiornoFileRaw.AddSeconds(SecFromMidnight)
'	End Function

'	Public Function IdCampo(NomeCanale As String) As Integer
'		For i As Integer = 0 To pIntestazioni.Count - 1
'			If pIntestazioni(i).ToLower = NomeCanale.ToLower Then Return i
'		Next
'		Return -1
'		'Return pIntestazioni.Find(Function(x) x = NomeCanale).IndexOf
'	End Function

'	'Public Function FaroDateTimeToSystemDT(StringaData As String, StringaTime As String) As DateTime
'	'	If Not IsNumeric(StringaData) Then Return Nothing
'	'	If Not IsNumeric(StringaTime) Then Return Nothing
'	'	Dim hms As String = StringaTime.Split(".")(0)
'	'	StringaTime = hms.PadLeft(6, "0")
'	'	If StringaTime.IndexOf(".") > -1 Then
'	'		'Dim fff As String = StringaTime.Split(".")(1)
'	'		'fff = fff.PadRight(3, "0")
'	'		StringaTime &= "." & StringaTime.Split(".")(1)
'	'	End If
'	'	'Dim dt As DateTime
'	'	'DateTime.TryParseExact(StringaData & hms & "." & fff, "yyyyMMddHmmss.fff", meCultureInfo, Globalization.DateTimeStyles.None, dt)

'	'	Return DateTime.ParseExact(StringaData & StringaTime, FormatiFaro, Nothing, Globalization.DateTimeStyles.None)

'	'	'Dim HMS As String = StringaTime.Split(".")(0)
'	'	'Dim a As Integer = StringaData.Substring(0, 4)
'	'	'Dim b As Integer = StringaData.Substring(4, 2)
'	'	'Dim c As Integer = StringaData.Substring(6, 2)
'	'	'Dim d As Integer = StringaTime.Substring(0, HMS.Length - 4).PadLeft(2, "0")
'	'	'Dim e As Integer = StringaTime.Substring(HMS.Length - 4, 2)
'	'	'Dim f As Integer = StringaTime.Substring(HMS.Length - 2, 2)
'	'	'Dim g As Integer = If(StringaTime.IndexOf(".") < 0, 0.ToString, StringaTime.Split(".")(1).PadRight(3, "0"))
'	'	'Dim DT2 As New DateTime(a, b, c, d, e, f, g)
'	'	'Dim DT = DateTime.ParseExact(StringaData & StringaTime, "yyyyMMddHHmmss.fff", Nothing)
'	'	''Dim Dtfi As Globalization.DateTimeFormatInfo
'	'	''Dtfi = CType(Globalization.DateTimeFormatInfo.CurrentInfo.Clone, Globalization.DateTimeFormatInfo)
'	'	''Dtfi.LongTimePattern = "yyyyMMdd"
'	'	''Dtfi.LongDatePattern = "HHmmss.fff"
'	'	''Dtfi.ShortTimePattern = "yyyyMMdd"
'	'	''Dtfi.ShortDatePattern = "HHmmss.fff"
'	'	''Dim dt1 = DateTime.Parse(StringaData & " " & StringaTime, Dtfi)
'	'	'Dim strDt As String = StringaData
'	'	'Dim strTm As String = StringaTime
'	'	'DT = DateTime.ParseExact(strDt & strTm, FormatiFaro, Nothing, Globalization.DateTimeStyles.None)
'	'	'Dim mls As Integer = DT.Millisecond

'	'	'Return DT2
'	'End Function

'	Private Sub NormalizzaHz()
'		Select Case pHz
'			Case 4
'				pHz = 5
'			Case 6 To 9
'				pHz = 10
'			Case 11 To 19
'				pHz = 20
'			Case 21 To 49
'				pHz = 50
'			Case 51 To 99
'				pHz = 100
'			Case 101 To 199
'				pHz = 200
'			Case 201 To 499
'				pHz = 500
'			Case 501 To 999
'				pHz = 1000
'			Case Else
'				' lascia invariato
'		End Select
'	End Sub

'	Private Sub LoopTest()
'		Dim Adesso As Date = Now
'		'Console.WriteLine("LoopTest 0")

'		Dim RigheTotali As Integer = Int(pRangeRaw.Durata.TotalSeconds * pHz) + 1
'		ReDim pIndici(RigheTotali)

'		For i As Long = pRangeRaw.IdRigaIniziale To pRangeRaw.IdRigaFinale
'			Dim Riga As String = pRigheRaw(i).Trim
'			If Not Riga = "" AndAlso Not Riga = pRigheRaw(0) Then
'				Dim RigaTime As DateTime = Nothing ' DataTimeDaRaw(i)
'				If Not RigaTime = Nothing Then
'					Dim SsDaInizio As Double = RigaTime.Subtract(pRangeRaw.Start).TotalSeconds
'					Dim RigaDaInizio As Integer = SsDaInizio * pHz
'					If RigaDaInizio >= 0 Then
'						pIndici(RigaDaInizio) = i
'					End If
'				Else
'					Dim SsDaInizio As Double = Now.Subtract(pRangeRaw.Start).TotalSeconds
'					pIndici(i) = i
'				End If
'			End If
'		Next
'		'Console.WriteLine("LoopTest: " & Now.Subtract(Adesso).TotalMilliseconds)

'		Dim UltimaRigaValida As Integer = 0
'		For i As Long = 0 To pIndici.Count - 1
'			If pIndici(i) = Nothing Then
'				pIndici(i) = UltimaRigaValida
'			Else
'				UltimaRigaValida = pIndici(i)
'			End If
'		Next
'		'Console.WriteLine("LoopTest: " & Now.Subtract(Adesso).TotalMilliseconds)

'	End Sub


'	Public Sub MappaRigheDT()

'		LoopTest()

'		Dim Adesso As Date = Now
'		'Console.WriteLine("MappaRightDT 0")


'		Dim RigheTotali As Integer = Int(pRangeRaw.Durata.TotalSeconds * pHz) + 1
'		ReDim pIndici(RigheTotali)

'		For i As Long = pRangeRaw.IdRigaIniziale To pRangeRaw.IdRigaFinale
'			' l' array Righe viene riempito in ogni riga dall'inizio alla fine delle righe valide del file raw
'			' se manca una riga del file raw la riga di pDati viene salvata con i dati della riga precedente
'			'Try
'			Dim Riga As String = pRigheRaw(i).Trim
'			If Not Riga = "" AndAlso Not Riga = pRigheRaw(0) Then
'				Dim RigaTime As DateTime = DataTimeDaRaw(i)
'				If Not RigaTime = Nothing Then
'					Dim SsDaInizio As Double = RigaTime.Subtract(pRangeRaw.Start).TotalSeconds
'					Dim RigaDaInizio As Integer = SsDaInizio * pHz
'					If RigaDaInizio >= 0 Then
'						pIndici(RigaDaInizio) = i
'					End If
'				End If
'			End If
'		Next
'		'Console.WriteLine("MappaRightDT: " & Now.Subtract(Adesso).TotalMilliseconds)

'		Dim UltimaRigaValida As Integer = 0
'		For i As Long = 0 To pIndici.Count - 1
'			If pIndici(i) = Nothing Then
'				pIndici(i) = UltimaRigaValida
'			Else
'				UltimaRigaValida = pIndici(i)
'			End If
'		Next
'		'Console.WriteLine("MappaRightDT: " & Now.Subtract(Adesso).TotalMilliseconds)

'		'Dim Test As Integer = 400000
'		'Dim IdRiga As Integer = pIndici(Test)
'		'Dim Momento As Date = DataTimeDaRaw(IdRiga)


'	End Sub

'	'Public Sub MappaRigheDTfast()

'	'	LoopTest()

'	'	Dim Adesso As Date = Now
'	'	Console.WriteLine("MappaRightDTfast 0")


'	'	Dim RigheTotali As Integer = Int(pRangeRaw.Durata.TotalSeconds * pHz) + 1
'	'	ReDim pIndici(RigheTotali)

'	'	For i As Long = pRangeRaw.IdRigaInizialeRaw To pRangeRaw.IdRigaFinaleRaw
'	'		' l' array Righe viene riempito in ogni riga dall'inizio alla fine delle righe valide del file raw
'	'		' se manca una riga del file raw la riga di pDati viene salvata con i dati della riga precedente
'	'		'Try
'	'		Dim Riga As String = pRigheRaw(i)
'	'		If Not Riga = "" AndAlso Not Riga = pRigheRaw(0) Then
'	'			Dim RigaTmp As New clsSingolaRiga(Riga, vbTab)
'	'			Dim SFM As Double = RigaTmp.ValoreCanali(pIdDaySec)
'	'			Dim RigaTime As DateTime = pGiornoFileRaw.AddSeconds(SFM)
'	'			If Not RigaTime = Nothing Then
'	'				Dim SsDaInizio As Double = RigaTime.Subtract(pRangeRaw.Inizio).TotalSeconds
'	'				Dim RigaDaInizio As Integer = SsDaInizio * pHz
'	'				If RigaDaInizio >= 0 Then
'	'					pIndici(RigaDaInizio) = i
'	'					pIndiciAdvanced.Add(i, RigaTmp)
'	'				End If
'	'			End If
'	'		End If
'	'	Next
'	'	Console.WriteLine("MappaRightDTfast: " & Now.Subtract(Adesso).TotalMilliseconds)

'	'	Dim UltimaRigaValida As Integer = 0
'	'	For i As Long = 0 To pIndici.Count - 1
'	'		If pIndici(i) = Nothing Then
'	'			pIndici(i) = UltimaRigaValida
'	'			pIndiciAdvanced.Add(i, pIndiciAdvanced.Valori(UltimaRigaValida))
'	'		Else
'	'			UltimaRigaValida = pIndici(i)
'	'		End If
'	'	Next
'	'	Console.WriteLine("MappaRightDTfast: " & Now.Subtract(Adesso).TotalMilliseconds)

'	'	'Dim Test As Integer = 400000
'	'	'Dim IdRiga As Integer = pIndici(Test)
'	'	'Dim Momento As Date = DataTimeDaRaw(IdRiga)


'	'End Sub

'	Public Sub MappaRigheDTfast()

'		'LoopTest()

'		'Dim Adesso As Date = Now
'		'Console.WriteLine("MappaRightDTfast 0")


'		Dim RigheTotali As Integer = Int(pRangeRaw.Durata.TotalSeconds * pHz) + 1
'		ReDim pIndici(RigheTotali)

'		For i As Long = pRangeRaw.IdRigaIniziale To pRangeRaw.IdRigaFinale
'			' l' array Righe viene riempito in ogni riga dall'inizio alla fine delle righe valide del file raw
'			' se manca una riga del file raw la riga di pDati viene salvata con i dati della riga precedente
'			'Try
'			Dim Riga As String = pRigheRaw(i)
'			If Not Riga = "" AndAlso Not Riga = pRigheRaw(0) Then
'				Dim RigaTime As DateTime = FastDataTimeDaRaw(i)
'				If Not RigaTime = Nothing Then
'					Dim SsDaInizio As Double = RigaTime.Subtract(pRangeRaw.Start).TotalSeconds
'					Dim RigaDaInizio As Integer = SsDaInizio * pHz
'					If RigaDaInizio >= 0 Then
'						pIndici(RigaDaInizio) = i
'					End If
'				End If
'			End If
'		Next
'		'Console.WriteLine("MappaRightDTfast: " & Now.Subtract(Adesso).TotalMilliseconds)

'		Dim UltimaRigaValida As Integer = 0
'		For i As Long = 0 To pIndici.Count - 1
'			If pIndici(i) = Nothing Then
'				pIndici(i) = UltimaRigaValida
'			Else
'				UltimaRigaValida = pIndici(i)
'			End If
'		Next
'		'Console.WriteLine("MappaRightDTfast: " & Now.Subtract(Adesso).TotalMilliseconds)

'		'Dim Test As Integer = 400000
'		'Dim IdRiga As Integer = pIndici(Test)
'		'Dim Momento As Date = DataTimeDaRaw(IdRiga)


'	End Sub


'End Class


'Public Class clsSingolaRiga
'  Dim pValoreCanali As List(Of String)
'  Dim pCsep As Char

'  Public Sub New(StringaRiga As String, cSep As Char)
'    pValoreCanali = StringaRiga.Split(cSep).ToList
'    pCsep = cSep
'  End Sub

'  Public ReadOnly Property ValoreDBL(Indice As Integer) As Double
'    Get
'      Dim vTmp As String = pValoreCanali(Indice)
'      If vTmp.Trim = "" Then Return 0
'      Return vTmp
'    End Get
'  End Property

'  Public Property ValoreCanali As List(Of String)
'    Get
'      Return pValoreCanali
'    End Get
'    Set(value As List(Of String))
'      pValoreCanali = value
'    End Set
'  End Property

'  Public Function StringaAggiornataOld() As String
'    Dim strTmp As String = ""
'    For Each Valore In pValoreCanali
'      strTmp &= Valore & pCsep
'    Next
'    Return strTmp.TrimEnd(pCsep)
'  End Function

'  Public Function StringaAggiornata() As String
'    Return String.Join(pCsep, pValoreCanali)
'  End Function

'End Class
