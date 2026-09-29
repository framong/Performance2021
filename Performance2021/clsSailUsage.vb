Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Net.Security
Imports System.Windows.Forms
Imports Accord
Imports Accord.Math
Imports SciChart.Core.Extensions
Imports SciChart.UI.Reactive.Extensions


Public Class clsSailList
  Public Property FilePath As String
  Public Property Sails As New ObservableCollection(Of clsSail)
  Public Property SelectedSail As clsSail = Nothing

  Public Sub New(Path As String)
    If Not System.IO.File.Exists(Path) Then
      Exit Sub
    End If
    FilePath = Path
    Dim cc As List(Of String) = ObjFiles.TestoInLista(FilePath)
    For Each c As String In cc
      If (Not c.Trim = "") Then
        Try
          Sails.Add(New clsSail(c))
        Catch ex As Exception
          Stop
        End Try
      End If
    Next

  End Sub

  Public Function SailTypeConverter(Type As clsKnownSail.eSailType) As clsSail.SailType
    Select Case Type
      Case clsKnownSail.eSailType.eMainSail
        Return clsSail.SailType.M
      Case clsKnownSail.eSailType.eStaySail
        Return clsSail.SailType.S
      Case clsKnownSail.eSailType.eHeadSail
        Return clsSail.SailType.H
      Case clsKnownSail.eSailType.eProdSail
        Return clsSail.SailType.P
      Case Else
        Return clsSail.SailType.U
    End Select
  End Function

  Public Function SailTypeConverter(Type As clsSail.SailType) As clsKnownSail.eSailType
    Select Case Type
      Case clsSail.SailType.M
        Return clsKnownSail.eSailType.eMainSail
      Case clsSail.SailType.S
        Return clsKnownSail.eSailType.eStaySail
      Case clsSail.SailType.H
        Return clsKnownSail.eSailType.eHeadSail
      Case clsSail.SailType.P
        Return clsKnownSail.eSailType.eProdSail
      Case Else
        Return clsKnownSail.eSailType.eUnknown
    End Select
  End Function


  Public Class clsSail
    Public Type As SailType
    Public Name As String
    Public Code As Double

    Public Enum SailType
      M = 0
      S = 1
      H = 2
      P = 3
      U = -1
    End Enum

    Public Sub New(SailString As String)
      Name = SailString.Split(vbTab)(0)
      Type = GetSailType(SailString.Split(vbTab)(1))
      Code = SailString.Split(vbTab)(2)
    End Sub

    Function GetSailType(Type As String) As SailType
      Select Case Type
        Case "M"
          Return SailType.M
        Case "S"
          Return SailType.S
        Case "H"
          Return SailType.H
        Case "P"
          Return SailType.P
        Case Else
          Return SailType.U
      End Select
    End Function

    Public Function SailTypeName()
      Select Case Type
        Case SailType.H
          Return "H"
        Case SailType.M
          Return "M"
        Case SailType.P
          Return "P"
        Case SailType.S
          Return "S"
        Case Else
          Return "U"
      End Select
    End Function


  End Class

End Class

Public Class clsSailUsage

  Public Property Sails As New List(Of clsSailUsageData)
  Public Property SSList As New List(Of clsSailSet)


  Public Sub UpdateSailUsageFromTextFile(SalvaParquet As Boolean, msg As Boolean)
    ' cerca il file di testo nella stessa cartella del file parquet
    ' Headers usati: MainSail	HeadSail	ProdSail	StaySail

    Dim CurrentFolderFiles As System.IO.FileInfo() = DataProvider2020.ParquetFiles.First.FileInfo.Directory.GetFiles
    SSList.Clear()

    For Each f In CurrentFolderFiles
      Select Case f.Extension
        Case ".xml"
          Dim datafolder As String = New System.IO.FileInfo(AppConfig.ActiveProfile.ActiveProfileFilePath).Directory.FullName
          datafolder = System.IO.Path.Combine(datafolder, "Data")
          Dim SL As New clsSailList(System.IO.Path.Combine(datafolder, "SailList.txt"))
          'Dim SL As New clsSailList(System.IO.Path.Combine(f.Directory.Parent.FullName, "SailList.txt"))
          UpdateSailUsageFromXmlEventFile(f.FullName, SL)
          For Each s In SL.Sails
            If DataProvider2020.KnownSails.Where(Function(x) x.SailCode = s.Code).FirstOrDefault Is Nothing Then
              DataProvider2020.KnownSails.Add(New clsKnownSail(s.Name, s.Code, SL.SailTypeConverter(s.Type), s.Name))
            End If
          Next
        Case ".txt"
          UpdateSailUsageFromSailLogFile(f.FullName)
        Case ".gz"
          If SailsLogFileNotExisting(f.Name, CurrentFolderFiles) Then
            UpdateSailUsageFromFaroGzLogFile(f.FullName)
          End If
        Case Else
          SalvaParquet = False
      End Select
    Next

    AggiornaCanaliVele()



    ' salva il nuovo file parquet

    If SalvaParquet Then
      LoadingProgressVisualizza()
      For Each f In DataProvider2020.ParquetFiles
        Dim N As New clsParquetCopy(f.FileInfo.FullName, "WithSails")
      Next
      LoadingProgressNascondi()
      ApriExplorer(DataProvider2020.ParquetFiles(0).FileInfo.Directory.FullName)
    End If
    If msg Then MsgBox("Sails Imported")
  End Sub

  Private Function SailsLogFileNotExisting(GzFileName As String, CurrentFolderFiles As System.IO.FileInfo()) As Boolean
    Dim data As String = GzFileName.Split("_")(1)
    Return CurrentFolderFiles.Where(Function(x) x.Name = "" & data & ".txt") Is Nothing
  End Function

  Dim Days As New List(Of DateTime)

  Private Sub UpdateSailUsageFromSailLogFile(FilePath As String)
    ' legge il contenuto del file sails e crea il sail set
    Dim cc As List(Of String) = ObjFiles.TestoInLista(FilePath)
    Dim fi As New System.IO.FileInfo(FilePath)
    If fi.Name.Contains("Sails") OrElse fi.Name.Contains("Notes") Then
      Dim strD As String = fi.Name.Replace("Sails", "").Replace("Notes", "").Replace(fi.Extension, "")
      If strD.Length = 8 Then
        Dim Data As DateTime = New DateTime(strD.Substring(0, 4), strD.Substring(4, 2), strD.Substring(6, 2), 0, 0, 0)
        If Not Days.Contains(Data) Then
          Days.Add(Data)
        End If
        'SSList.Clear()
        Dim lista As List(Of clsKnownSail) = CaricaSailList()
        For Each c In cc
          If Not c.Trim = "" Then
            Dim ss As New clsSailSet(c, Data, lista)
            SSList.Add(ss)
          End If
        Next
        If Not DataProvider2020 Is Nothing Then
          DataProvider2020.KnownSails = lista
        End If
      End If
    End If
  End Sub

  Private Function CaricaSailList() As List(Of clsKnownSail)

    Dim ns As New List(Of clsKnownSail)

    Dim path = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, "SailsList.txt")

    If System.IO.File.Exists(path) Then


      Dim ListaVele = ObjFiles.TestoInLista(path)

      For Each vela In ListaVele
        If Not vela = String.Empty Then
          Dim v = vela.Split(" ")
          Dim n = v(0)
          If v.Length > 3 Then
            n = v(3)
          End If
          Dim ks As New clsKnownSail(v(0).ToString, v(1).ToDouble, v(2).ToString, n.ToString)
          ns.Add(ks)
        End If

      Next

    End If
    Return ns

  End Function




  Private Sub UpdateSailUsageFromXmlEventFile(FilePath As String, ByRef SailList As clsSailList)
    ' legge il contenuto del file sails e crea il sail set
    Dim cc As List(Of String) = ObjFiles.TestoInLista(FilePath)
    Dim ListaVele As New Dictionary(Of String, String)
    Dim SailsUp As New Dictionary(Of DateTime, List(Of String))
    Dim SailSets As New List(Of clsSailSet)
    Dim ii As Integer = 0
    For i As Integer = 0 To cc.Count
      If cc(i).Contains("<saillist>") Then
        ii = 1

        Do
          If (cc(i + ii).Contains("</saillist>")) Then Exit Do
          Dim sp = cc(i + ii).Split(" ")
          Dim vela As String = sp(5).Split("=")(1).ToString
          Dim tipo As String = sp(6).Split("=")(1).ToString
          ListaVele.Add(vela.Replace("""", ""), tipo.Replace("""", ""))
          ii += 1
        Loop
        i += ii
        'ElseIf c.Contains("/saillist") Then

      ElseIf cc(i).Contains("<events>") Then
        ii = 1
        Do
          If (cc(i + ii).Contains("</events>")) Then Exit Do
          Dim sp = cc(i + ii).Split(" ")
          Dim type As String = sp(7).Split("=")(1).ToString.Replace("""", "")
          If type = "SailsUp" Then
            Dim strDate As String = sp(5).Split("=")(1).ToString.Replace("""", "")
            Dim strTime As String = sp(6).Split("=")(1).ToString.Replace("""", "")
            Dim Sails As String = sp(8).Split("=")(1).ToString.Replace("""", "")
            Dim DT As New DateTime(strDate.Split("-")(0), strDate.Split("-")(1), strDate.Split("-")(2), strTime.Split(":")(0), strTime.Split(":")(1), strTime.Split(":")(2))
            If SailsUp.ContainsKey(DT) Then
              SailsUp(DT).Clear()
            Else
              SailsUp.Add(DT, New List(Of String))
            End If
            For Each sail As String In Sails.Split(";")
              If sail = "" Then
                SailsUp(DT).Add("0.0")
              Else
                SailsUp(DT).Add(sail)
              End If
            Next
          ElseIf type = "DayStop" Then
            Dim strDate As String = sp(5).Split("=")(1).ToString.Replace("""", "")
            Dim strTime As String = sp(6).Split("=")(1).ToString.Replace("""", "")
            Dim DT As New DateTime(strDate.Split("-")(0), strDate.Split("-")(1), strDate.Split("-")(2), strTime.Split(":")(0), strTime.Split(":")(1), strTime.Split(":")(2))
            SailsUp.Add(DT, New List(Of String))
          End If
          'Dim vela As String = sp(0).Split("=").ToString
          'Dim tipo As String = sp(1).Split("=").ToString
          'ListaVele.Add(vela, tipo)
          ii += 1
        Loop
        i += ii
        Exit For
        'ElseIf c.Contains("/saillist") Then

      End If
    Next

    CheckNotListedSails(ListaVele, SailList)


    If NewSails.Count > 0 Then
      MsgBox("New Sails found." & vbCrLf & "Please add/check the SailList.txt file:" & vbCrLf & String.Join(vbCrLf, NewSails) & vbCrLf & "The standard named sails will be added to the list giving a temporary code" & vbCrLf & "please open the saillist.txt file anc correct codes and types where needed" & vbCrLf & "Some Sail can be ignored if the name has is not standard")
      Clipboard.SetText(String.Join(vbCrLf, NewSails))

      Dim Rande As String() = {"MN", "LM", "MM", "HM", "LMN", "MMN", "HMN", "LTM", "LTMN"}
      Dim NSails As New List(Of clsSailList.clsSail)
      For Each NS As String In NewSails
        Dim NuovaVela As clsSailList.clsSail
        If NS.StartsWith("J") Then
          NuovaVela = Fiocco(NS)
        ElseIf NS.StartsWith("A") Or NS.StartsWith("G") Then
          NuovaVela = Gennaker(NS)
        ElseIf NS.StartsWith("SS") Or NS.StartsWith("GS") Or NS.StartsWith("ST") Or NS.StartsWith("STS") Then
          NuovaVela = StaySail(NS)
        Else
          If Rande.Any(Function(p) NS.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) Then
            NuovaVela = Randa(NS)
          End If
        End If
        If (Not NuovaVela Is Nothing) Then
          Dim v = SailList.Sails.Where(Function(x) x.Type = NuovaVela.Type).Count()
          Dim vv = NSails.Where(Function(x) x.Type = NuovaVela.Type).Count()
          NuovaVela.Code = v + vv + 1
          NSails.Add(NuovaVela)
        End If
      Next
      If NSails.Count() > 0 Then
        Dim righe = New List(Of String)
        For Each ns In NSails
          righe.Add(ns.Name & vbTab & ns.SailTypeName() & vbTab & ns.Code)
          SailList.Sails.Add(ns)
        Next
        System.IO.File.AppendAllLines(SailList.FilePath, righe)
        ApriExplorer(SailList.FilePath)
      End If
      'End
    End If

    Dim Offset = AppConfig.ActiveProfile.SailFilesOffset
    Dim DS = SailsUp.First.Key
    Dim Main = GetSailMap(SailsUp.First.Value, SailList, clsSailList.clsSail.SailType.M)
    Dim Head = GetSailMap(SailsUp.First.Value, SailList, clsSailList.clsSail.SailType.H)
    Dim Stay = GetSailMap(SailsUp.First.Value, SailList, clsSailList.clsSail.SailType.S)
    Dim Prod = GetSailMap(SailsUp.First.Value, SailList, clsSailList.clsSail.SailType.P)

    For Each Sail In SailsUp
      Dim DF = Sail.Key
      Dim ss As New clsSailSet(DS.Add(Offset), DF.Add(Offset), Main, Head, Stay, Prod)
      SailSets.Add(ss)
      DS = Sail.Key
      Main = GetSailMap(Sail.Value, SailList, clsSailList.clsSail.SailType.M)
      Head = GetSailMap(Sail.Value, SailList, clsSailList.clsSail.SailType.H)
      Stay = GetSailMap(Sail.Value, SailList, clsSailList.clsSail.SailType.S)
      Prod = GetSailMap(Sail.Value, SailList, clsSailList.clsSail.SailType.P)
    Next

    SSList = SailSets

    'Dim fi As New IO.FileInfo(FilePath)
    'Dim strD As String = fi.Name.Replace("Sails", "").Replace(fi.Extension, "")
    'Dim Data As DateTime = New DateTime(strD.Substring(0, 4), strD.Substring(4, 2), strD.Substring(6, 2), 0, 0, 0)
    'If Not Days.Contains(Data) Then
    '    Days.Add(Data)
    'End If
    ''SSList.Clear()
    'For Each c In cc
    '    If Not c.Trim = "" Then
    '        Dim ss As New clsSailSet(c, Data)
    '        SSList.Add(ss)
    '    End If
    'Next
  End Sub

  Private Function Fiocco(Stringa As String) As clsSailList.clsSail
    Return New clsSailList.clsSail(Stringa & vbTab & "H" & vbTab & "999")
  End Function

  Private Function Randa(Stringa As String) As clsSailList.clsSail
    Return New clsSailList.clsSail(Stringa & vbTab & "M" & vbTab & "999")
  End Function

  Private Function Gennaker(Stringa As String) As clsSailList.clsSail
    Return New clsSailList.clsSail(Stringa & vbTab & "P" & vbTab & "999")
  End Function

  Private Function StaySail(Stringa As String) As clsSailList.clsSail
    Return New clsSailList.clsSail(Stringa & vbTab & "S" & vbTab & "999")
  End Function


  Dim NewSails As New List(Of String)

  Private Sub CheckNotListedSails(ListaVele As Dictionary(Of String, String), ByRef SailList As clsSailList)
    NewSails.Clear()
    For Each Vela In ListaVele
      Dim sail = SailList.Sails.Where(Function(x) x.Name = Vela.Key).FirstOrDefault
      If sail Is Nothing Then
        NewSails.Add(Vela.Key)
      End If
    Next
  End Sub

  Private Function GetSailMap(SailNames As List(Of String), ByRef SailList As clsSailList, SailType As clsSailList.clsSail.SailType) As clsSailsMap
    For Each SailName In SailNames
      Dim sail = SailList.Sails.Where(Function(x) x.Name = SailName).FirstOrDefault
      If Not sail Is Nothing Then
        If sail.Type = SailType Then Return New clsSailsMap(sail.Name, sail.Code)
      End If
    Next
    Return Nothing ' vela non presente nella sail list
  End Function


  Private Sub UpdateSailUsageFromFaroGzLogFile(FilePath As String)
    ' legge il contenuto del file log e crea il sail set

    Dim FaroGz As New clsFileFaroGz(FilePath)

    ' SystemTime_Year	SystemTime_Month	SystemTime_Day	SystemTime_Hour	SystemTime_Minute	SystemTime_Second
    Dim Righe As New List(Of String)
    Dim ssPrev As FaroGzSailCodes = FaroGz.ListaVele.First
    For Each v In FaroGz.ListaVele
      If Not (SailSetUnchanged(v, ssPrev)) Then
        Dim ss As New clsSailSet(ssPrev.Tm, v.Tm.AddSeconds(-1), ssPrev.MainSail, ssPrev.HeadStaySail, ssPrev.StaySail, ssPrev.ProdSail)
        SSList.Add(ss)
        ssPrev = v
        Righe.Add(ScriviRiga(ss))
      ElseIf v Is FaroGz.ListaVele.Last Then
        Dim ss As New clsSailSet(ssPrev.Tm, v.Tm, ssPrev.MainSail, ssPrev.HeadStaySail, ssPrev.StaySail, ssPrev.ProdSail)
        SSList.Add(ss)
        ssPrev = v
        Righe.Add(ScriviRiga(ss))
      End If
    Next


    Dim fi As New System.IO.FileInfo(FilePath)
    Dim strD As String = fi.Name.Split("_")(1)
    Dim Data As DateTime = New DateTime(strD.Substring(0, 4), strD.Substring(4, 2), strD.Substring(6, 2), 0, 0, 0)
    Dim newFile As String = System.IO.Path.Combine(fi.Directory.FullName, "Sails" & Data.ToString("yyyyMMdd") & ".txt")
    Dim FullPath As String = clsFiles.NomeProgressivo(newFile)
    ObjFiles.SalvaNuovoFileSostituendoContenuto(String.Join(vbCrLf, Righe), FullPath)


  End Sub

  Private Function ScriviRiga(ss As clsSailSet) As String
    Dim riga As String = ss.UsedFrom.ToString("HH:mm") & "-" & ss.UsedTo.ToString("HH:mm")
    If Not ss.MainSail Is Nothing Then
      riga &= " " & ss.MainSail.SailName
    End If
    If Not ss.HeadSail Is Nothing Then
      riga &= " " & ss.HeadSail.SailName
    End If
    If Not ss.StaySail Is Nothing Then
      riga &= " " & ss.StaySail.SailName
    End If
    If Not ss.ProdSail Is Nothing Then
      riga &= " " & ss.ProdSail.SailName
    End If
    Return riga
  End Function


  Private Function SailSetUnchanged(ssCurrent As FaroGzSailCodes, ssPrev As FaroGzSailCodes) As Boolean
    Return ssCurrent.MainSail = ssPrev.MainSail AndAlso ssCurrent.HeadStaySail = ssPrev.HeadStaySail AndAlso ssCurrent.StaySail = ssPrev.StaySail AndAlso ssCurrent.ProdSail = ssPrev.ProdSail
  End Function

  Private Sub AggiornaCanaliVele()
    ' verifica se esistono i canali dei codici vela
    ' se non esistono li crea 
    ' aggiorna i valori dei codici vela come da file log

    Dim ChMainSail As clsChannel2020 = VerificaCanale("MainSail")
    Dim ChHeadSail As clsChannel2020 = VerificaCanale("HeadSail")
    Dim ChProdSail As clsChannel2020 = VerificaCanale("ProdSail")
    Dim ChStaySail As clsChannel2020 = VerificaCanale("StaySail")

    For Each ss In SSList
      Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(ss.UsedFrom)
      Dim IdFinale As Integer = DataProvider2020.TrovaIndice(ss.UsedTo)

      If IdIniziale > -1 AndAlso IdFinale > -1 Then
        For i As Integer = IdIniziale To IdFinale
          ChMainSail.Valori(i) = ss.MainSailCode
          ChHeadSail.Valori(i) = ss.HeadStaySailCode
          ChStaySail.Valori(i) = ss.StaySailCode
          ChProdSail.Valori(i) = ss.ProdSailCode
        Next
      End If


    Next



  End Sub

  Private Function VerificaCanale(ChannelId As String) As clsChannel2020
    Dim ChTmp As clsChannel2020 = DataProvider2020.Channels.Canale(ChannelId)
    If ChTmp Is Nothing Then
      ChTmp = CreaNuovoCanaleSails(ChannelId)
      DataProvider2020.Channels.ListaCanali.Add(ChTmp)
      ReDim ChTmp.Valori(DataProvider2020.TimeStamps.Count - 1)
    Else
      If ChTmp.Valori Is Nothing Then
        ReDim ChTmp.Valori(DataProvider2020.TimeStamps.Count - 1)
      End If
    End If
    For i As Integer = 0 To ChTmp.Valori.Count - 1
      ChTmp.Valori(i) = Double.NaN
    Next
    Return ChTmp
  End Function


  Private Function CreaNuovoCanaleSails(Intestazione As String) As clsChannel2020
    Dim pChannelId As String = Intestazione
    Dim CanaleChiave As clsChannels2020.eCanaliChiave = clsChannels2020.eCanaliChiave.eNone
    Dim pShortName As String = pChannelId
    Dim pLongName As String = pChannelId
    Dim pShortUM As String = ""
    Dim pLongUM As String = ""
    Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
    Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
    Dim pActualLogHeader As String = pKnownHeaders.First
    Dim pPolarHeader As String = ""
    Dim pBenchmarkHeader As String = ""
    Dim pIsMath As Boolean = False
    Dim pDecimals As Integer = 4
    Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pBenchmarkHeader, pIsMath, pDecimals)
    Return CanaleTmp
  End Function





End Class


Public Class clsSailUse
  Public SailName As String
  Public TR As New List(Of clsTimeRange)

  Public Sub New(Name As String, usedfrom As DateTime, usedto As DateTime)
    SailName = Name
    TR.Add(New clsTimeRange(usedfrom, usedto))
  End Sub


  Public Function TotalUse() As TimeSpan
    Dim ts As New TimeSpan(0)
    For Each t In TR
      ts = ts + t.Durata
    Next
    Return ts
  End Function

End Class

Public Class clsFileFaroGz
  Public Variabili As New clsVariabili

  Public ListaVele As New List(Of FaroGzSailCodes)

  Dim MainSailChannelId As Integer
  Dim HeadSailChannelId As Integer
  Dim StaySailChannelId As Integer
  Dim ProdSailChannelId As Integer

  Dim DTY As Integer
  Dim DTM As Integer
  Dim DTD As Integer
  Dim DTH As Integer
  Dim DTMn As Integer
  Dim DTS As Integer

  Public Sub New(PathFile As String)
    LeggiFileFaRoGzip(PathFile)
  End Sub

  Private Sub ImpostaIdCanaliObbligatori()
    DTY = IdVariabile("SystemTime_Year")
    DTM = IdVariabile("SystemTime_Month")
    DTD = IdVariabile("SystemTime_Day")
    DTH = IdVariabile("SystemTime_Hour")
    DTMn = IdVariabile("SystemTime_Minute")
    DTS = IdVariabile("SystemTime_Second")

    MainSailChannelId = IdVariabile("MainSail")
    HeadSailChannelId = IdVariabile("HeadSail")
    StaySailChannelId = IdVariabile("StaySail")
    ProdSailChannelId = IdVariabile("ProdSail")
  End Sub

  Private Sub RiempiListaVele()
    ListaVele.Clear()
    For i As Integer = 0 To Variabili.Lista.First.Valori.Count - 1
      Try
        Dim v As New FaroGzSailCodes()
        v.Tm = New DateTime(Variabili.Lista(DTY).Valori(i), Variabili.Lista(DTM).Valori(i), Variabili.Lista(DTD).Valori(i), Variabili.Lista(DTH).Valori(i), Variabili.Lista(DTMn).Valori(i), Variabili.Lista(DTS).Valori(i))
        v.MainSail = Variabili.Lista(MainSailChannelId).Valori(i)
        v.HeadStaySail = Variabili.Lista(HeadSailChannelId).Valori(i)
        v.StaySail = Variabili.Lista(StaySailChannelId).Valori(i)
        v.ProdSail = Variabili.Lista(ProdSailChannelId).Valori(i)
        ListaVele.Add(v)
      Catch ex As Exception
        Stop
      End Try
    Next
  End Sub



  Public Function IdVariabile(NomeCanale As String) As Integer
    Dim Variabile = Variabili.Lista.Where(Function(x) x.Nome = NomeCanale).FirstOrDefault
    If Variabile Is Nothing Then Return -1
    Return Variabili.Lista.IndexOf(Variabile)
  End Function

  Public Sub LeggiFileFaRoGzip(PathFile As String)
    Dim RigheFileTesto As New List(Of String)
    Dim GZ As New clsReadGZipFiles(PathFile)
    Dim Riga As String
    RigheFileTesto.Clear()
    Do
      Riga = GZ.NextLine
      If Riga = Nothing Then Exit Do
      RigheFileTesto.Add(Riga)
    Loop
    For Each Riga In RigheFileTesto
      Dim Contenuto As List(Of String) = Riga.Split(vbTab).ToList
      For IdColonna As Integer = 0 To Contenuto.Count - 1
        If Riga Is RigheFileTesto.First Then
          Variabili.Lista.Add(New clsVariabile(Contenuto(IdColonna)))
        Else
          Variabili.Lista(IdColonna).Valori.Add(StringToDouble(Contenuto(IdColonna)))
        End If
      Next
    Next
    ImpostaIdCanaliObbligatori()
    RiempiListaVele()
  End Sub

  Private Function StringToDouble(Valore As String) As Double
    'Stop
    If IsNumeric(Valore) Then Return CDbl(Valore)
    Return Double.NaN
  End Function



End Class

Public Class FaroGzSailCodes
  ' SystemTime_Year	SystemTime_Month	SystemTime_Day	SystemTime_Hour	SystemTime_Minute	SystemTime_Second
  Public Tm As DateTime
  Public Property MainSail As Double
  Public Property StaySail As Double
  Public Property HeadStaySail As Double
  Public Property ProdSail As Double

End Class


Public Class clsSailUsageData

  Public Property SailName As String
  Public Property BasicDataList As New List(Of clsBasicData)
  Public Property Tacks As Integer
  Public Property Gybes As Integer

End Class
Public Class clsBasicData

  Public Property PolPerc As Double
  Public Property Tws As Double
  Public Property Twa As Double
  Public Property Aws As Double
  Public Property Awa As Double


End Class



Public Class clsSailSet
  Public Property UsedFrom As DateTime
  Public Property UsedTo As DateTime
  Public Property MainSail As clsSailsMap
  Public Property StaySail As clsSailsMap
  Public Property HeadSail As clsSailsMap
  Public Property ProdSail As clsSailsMap

  Public Vele As New Dictionary(Of String, Double)

  Public Function MainSailCode() As Double
    If MainSail Is Nothing Then Return Double.NaN
    Return MainSail.SailCode
  End Function
  Public Function StaySailCode() As Double
    If StaySail Is Nothing Then Return Double.NaN
    Return StaySail.SailCode
  End Function
  Public Function HeadStaySailCode() As Double
    If HeadSail Is Nothing Then Return Double.NaN
    Return HeadSail.SailCode
  End Function
  Public Function ProdSailCode() As Double
    If ProdSail Is Nothing Then Return Double.NaN
    Return ProdSail.SailCode
  End Function

  Public Sub New(Start As DateTime, Finish As DateTime, MainSailCode As Double, HeadSailCode As Double, StaySailCode As Double, ProdSailCode As Double)
    UsedFrom = Start
    UsedTo = Finish
    MainSail = MainSailFromCode(MainSailCode)
    HeadSail = HeadSailFromCode(HeadSailCode)
    StaySail = StaySailFromCode(StaySailCode)
    ProdSail = ProdSailFromCode(ProdSailCode)
  End Sub
  Public Sub New(Start As DateTime, Finish As DateTime, MainSailCode As clsSailsMap, HeadSailCode As clsSailsMap, StaySailCode As clsSailsMap, ProdSailCode As clsSailsMap)
    UsedFrom = Start
    UsedTo = Finish
    MainSail = MainSailCode
    HeadSail = HeadSailCode
    StaySail = StaySailCode
    ProdSail = ProdSailCode
  End Sub




  Private Function MainSailFromCode(Code As Double) As clsSailsMap
    Select Case Code
      Case 10.02
        Return New clsSailsMap("MNB", Code)
      Case 11.02
        Return New clsSailsMap("MNB1Reef", Code)
      Case 12.02
        Return New clsSailsMap("MNB2Reef", Code)
      Case 10.03
        Return New clsSailsMap("MNC", Code)
      Case 11.03
        Return New clsSailsMap("MNC1Reef", Code)
      Case 10.04
        Return New clsSailsMap("MND", Code)
      Case 2
        Return New clsSailsMap("MNB", Code)
      Case Else
        Return Nothing
    End Select
  End Function

  Private Function HeadSailFromCode(Code As Double) As clsSailsMap
    Select Case Code
      Case 10.02
        Return New clsSailsMap("J1B", Code)
      Case 15.02
        Return New clsSailsMap("J15B", Code)
                'Return New clsSailsMap("J1.5B", Code)
      Case 20.02
        Return New clsSailsMap("J2B", Code)
      Case 10.03
        Return New clsSailsMap("J1C", Code)
      Case 10.04
        Return New clsSailsMap("J1D", Code)
      Case 20.03
        Return New clsSailsMap("J2C", Code)
      Case 20.04
        Return New clsSailsMap("J2D", Code)
      Case 30.02
        Return New clsSailsMap("J3B", Code)
      Case 35.02
        Return New clsSailsMap("J35B", Code)
      Case 30.03
        Return New clsSailsMap("J3C", Code)
                'Return New clsSailsMap("J3.5B", Code)
      Case 40.02
        Return New clsSailsMap("J4B", Code)
      Case Else
        Return Nothing
    End Select
  End Function

  Private Function ProdSailFromCode(Code As Double) As clsSailsMap
    Select Case Code
      Case 10.02
        Return New clsSailsMap("A1C", Code)
      Case 20.02
        Return New clsSailsMap("A2B", Code)
      Case 20.03
        Return New clsSailsMap("A2C", Code)
      Case 20.04
        Return New clsSailsMap("A2D", Code)
      Case 30.02
        Return New clsSailsMap("A3B", Code)
      Case 40.02
        Return New clsSailsMap("A4B", Code)
      Case 5.02
        Return New clsSailsMap("FROB", Code)
      Case 8.01
        Return New clsSailsMap("JTA", Code)
      Case 0.02
        Return New clsSailsMap("DNOB", Code)
      Case 0.03
        Return New clsSailsMap("MH0C", Code)
      Case Else
        Return Nothing
    End Select
  End Function

  Private Function StaySailFromCode(Code As Double) As clsSailsMap
    Select Case Code
      Case 40.02
        Return New clsSailsMap("J4B", Code)
      Case 10.02
        Return New clsSailsMap("GSB", Code)
      Case 15.02
        Return New clsSailsMap("SSB", Code)
      Case Else
        Return Nothing
    End Select
  End Function

  Public Sub New(Message As String, Data As DateTime, Sails As List(Of clsKnownSail))

    Dim msg As String() = Message.Split(" ")
    Dim TR As String = msg(0).Replace(".", ":")
    If TR.Length = 11 Then
      UsedFrom = New DateTime(Data.Year, Data.Month, Data.Day, TR.Split("-")(0).Split(":")(0), TR.Split("-")(0).Split(":")(1), 0)
      UsedTo = New DateTime(Data.Year, Data.Month, Data.Day, TR.Split("-")(1).Split(":")(0), TR.Split("-")(1).Split(":")(1), 0)
    ElseIf TR.Length = 17 Then
      UsedFrom = New DateTime(Data.Year, Data.Month, Data.Day, TR.Split("-")(0).Split(":")(0), TR.Split("-")(0).Split(":")(1), TR.Split("-")(0).Split(":")(2))
      UsedTo = New DateTime(Data.Year, Data.Month, Data.Day, TR.Split("-")(1).Split(":")(0), TR.Split("-")(1).Split(":")(1), TR.Split("-")(1).Split(":")(2))
    Else
      Exit Sub
    End If

    Dim Vele As New List(Of String)
    For i As Integer = 1 To msg.Count - 1
      Vele.Add(msg(i))
    Next
    KnownSails = Sails
    SalvaVeleDaKnownSails(Vele)
    'SalvaVele(Vele)
  End Sub


  Private KnownSails As List(Of clsKnownSail)


  Private Sub SalvaVeleDaKnownSails(Vele As List(Of String))

    For Each vela In Vele
      Dim v = KnownSails.Where(Function(x) x.SailName.ToLower = vela.ToLower).FirstOrDefault
      If Not v Is Nothing Then
        Dim vv = DirectCast(v, clsKnownSail)
        Select Case vv.SailType
          Case clsKnownSail.eSailType.eMainSail
            MainSail = New clsSailsMap(vv.AliasName, vv.SailCode)
          Case clsKnownSail.eSailType.eStaySail
            If vv.SailCode = 40.02 AndAlso Vele.Count = 2 Then
              HeadSail = New clsSailsMap(vv.AliasName, vv.SailCode)
            Else
              StaySail = New clsSailsMap(vv.AliasName, vv.SailCode)
            End If
          Case clsKnownSail.eSailType.eProdSail
            ProdSail = New clsSailsMap(vv.AliasName, vv.SailCode)
          Case clsKnownSail.eSailType.eHeadSail
            HeadSail = New clsSailsMap(vv.AliasName, vv.SailCode)
        End Select
      End If


    Next


  End Sub


End Class


Public Class clsKnownSail
  Public Property SailName As String
  Public Property AliasName As String
  Public Property SailCode As Double
  Public Property SailType As eSailType


  Public Enum eSailType
    eMainSail = 0
    eHeadSail = 1
    eProdSail = 2
    eStaySail = 3
    eUnknown = -1
  End Enum


  Public Sub New(Name As String, Code As Double, Type As String, FriendlyName As String)
    SailName = Name
    SailCode = Code
    AliasName = FriendlyName
    Select Case Type.ToLower
      Case "m"
        SailType = eSailType.eMainSail
      Case "p"
        SailType = eSailType.eProdSail
      Case "s"
        SailType = eSailType.eStaySail
      Case "h"
        SailType = eSailType.eHeadSail
      Case Else
        SailType = eSailType.eUnknown
    End Select
  End Sub

  Public Sub New(Name As String, Code As Double, Type As eSailType, FriendlyName As String)
    SailName = Name
    SailCode = Code
    AliasName = FriendlyName
    SailType = Type
  End Sub

End Class

Public Class clsSailsMap
  Public Property SailName As String
  Public Property SailCode As Double


  Public Sub New(Name As String, Code As Double)
    SailName = Name
    SailCode = Code
  End Sub

End Class
