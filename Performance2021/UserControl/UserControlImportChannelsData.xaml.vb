Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged

Public Class UserControlImportChannelsData
  Dim VM As New clsUserControlImportChannelsDataViewModel

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    Me.DataContext = VM
    If Not DataProvider2020 Is Nothing Then
      VM.Abilita = DataProvider2020.ValoriCaricati
    End If
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.SelezionaFile()
  End Sub

  Private Sub btn_SelToNotSel_Click(sender As Object, e As RoutedEventArgs)
    VM.MoveLeftToRight(DirectCast(LV_Left.SelectedItem, clsCanaleParquetAdvanced))
  End Sub

  Private Sub btn_NotSelToSel_Click(sender As Object, e As RoutedEventArgs)
    VM.MoveRightToLeft(DirectCast(LV_Right.SelectedItem, clsCanaleParquetAdvanced))
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    If VM.ImportaValori() Then Me.Close()
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    Me.Close()
  End Sub

  Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
    VM.TxtFiltro = txt_Filtro.Text
  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    If VM.ImportaValoriManualmente Then Me.Close()
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsUserControlImportChannelsDataViewModel
  'Implements INotifyPropertyChanged

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property PathFile As String
  Public Property Source_Files As New List(Of clsParquetFile)
  Public Property CanaliFileSelezionato As New ObservableCollection(Of clsCanaleParquetAdvanced)
  Public Property CanaliDaAggiungere As New ObservableCollection(Of clsCanaleParquetAdvanced)
  Public Property AggiornaParquetCaricati As Boolean = True
  Public Property TxtFiltro As String
  Public Property Abilita As Boolean = False


  'Public Property CanaliFileSelezionato As ObservableCollection(Of clsCanaleParquetAdvanced)
  '  Get
  '    Return _CanaliFileSelezionato
  '  End Get
  '  Set(value As ObservableCollection(Of clsCanaleParquetAdvanced))
  '    _CanaliFileSelezionato = value
  '    OnPropertyChanged("CanaliFileSelezionato")
  '  End Set
  'End Property

  'Public ReadOnly Property CanaliFileSelezionatoFiltrati As List(Of clsCanaleParquetAdvanced)
  '  Get
  '    If TxtFiltro Is Nothing Then Return _CanaliFileSelezionato.ToList
  '    If TxtFiltro.Trim = "" Then
  '      Return _CanaliFileSelezionato.ToList
  '    Else
  '      Dim r = _CanaliFileSelezionato.Where(Function(x) x.ChannelId.ToLower.Contains(TxtFiltro.ToLower)).ToList
  '      Return r
  '    End If
  '  End Get
  'End Property

  'Public Property PathFile As String
  '  Get
  '    Return _PathFile
  '  End Get
  '  Set(value As String)
  '    _PathFile = value
  '  End Set
  'End Property

  'Public Property SourceFiles As List(Of clsParquetFile)
  '  Get
  '    Return Source_Files
  '  End Get
  '  Set(value As List(Of clsParquetFile))
  '    Source_Files = value
  '  End Set
  'End Property

  'Public Property CanaliDaAggiungere As ObservableCollection(Of clsCanaleParquetAdvanced)
  '  Get
  '    Return _CanaliDaAggiungere
  '  End Get
  '  Set(value As ObservableCollection(Of clsCanaleParquetAdvanced))
  '    _CanaliDaAggiungere = value
  '    OnPropertyChanged("CanaliDaAggiungere")
  '  End Set
  'End Property

  'Public Property AggiornaParquetCaricati As Boolean
  '  Get
  '    Return _AggiornaParquetCaricati
  '  End Get
  '  Set(value As Boolean)
  '    _AggiornaParquetCaricati = value
  '    OnPropertyChanged("AggiornaParquetCaricati")
  '  End Set
  'End Property

  'Public Property TxtFiltro As String
  '  Get
  '    Return _TxtFiltro
  '  End Get
  '  Set(value As String)
  '    _TxtFiltro = value
  '    OnPropertyChanged("CanaliFileSelezionatoFiltrati")
  '    OnPropertyChanged("TxtFiltro")
  '  End Set
  'End Property

  'Public Property Abilita As Boolean
  '  Get
  '    Return _Abilita
  '  End Get
  '  Set(value As Boolean)
  '    _Abilita = value
  '    OnPropertyChanged("Abilita")
  '  End Set
  'End Property

  Public Sub SelezionaFile()
    Dim SelFileName As String
    Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LoadChannels", "LastFilePath", DataProvider2020.Files.First.FullName, True, True)
    Dim PathFile As String = ObjFiles.SelezionaFile(UltimoPath, "Select performance source file", "performance file|*.ppf|all files|*.*", ".ppf", SelFileName)
    If System.IO.File.Exists(PathFile) Then
      Dim i As New System.IO.FileInfo(PathFile)
      AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "LoadChannels", "LastFilePath", i.Directory.FullName, True, True)
      Dim ListaCanali As New List(Of clsCanaleParquetAdvanced)
      CanaliFileSelezionato.Clear()
      Source_Files.Clear()
      Source_Files.Add(New clsParquetFile(PathFile))
      For Each c In Source_Files.Last.CanaliAdvanced
        ' aggiinge i soli canali non presenti nel file caricato (intestazioni raw)
        If DataProvider2020.Channels.ListaCanali.Where(Function(x) x.ActualLogHeader = c.ActualLogHeader).Count = 0 Then
          Dim cxml = CercaCanaleXML(c.ChannelId)
          If Not cxml Is Nothing Then
            c.DataType = cxml.DataType
            c.ShortName = cxml.ShortName
            c.LongName = cxml.LongName
            c.ShortUM = cxml.ShortUM
            c.LongUM = cxml.LongUM
            c.Decimals = cxml.Decimals
          End If
          CanaliFileSelezionato.Add(c)
        End If
      Next
      OnPropertyChanged("CanaliFileSelezionato")
      OnPropertyChanged("CanaliFileSelezionatoFiltrati")
    End If
  End Sub


  Public Sub MoveLeftToRight(SelectedItem As clsCanaleParquetAdvanced)
    CanaliDaAggiungere.Add(SelectedItem)
    CanaliFileSelezionato.Remove(SelectedItem)
    OnPropertyChanged("CanaliFileSelezionato")
    OnPropertyChanged("CanaliDaAggiungere")
    OnPropertyChanged("CanaliFileSelezionatoFiltrati")
  End Sub

  Public Sub MoveRightToLeft(SelectedItem As clsCanaleParquetAdvanced)
    CanaliDaAggiungere.Remove(SelectedItem)
    CanaliFileSelezionato.Add(SelectedItem)
    OnPropertyChanged("CanaliFileSelezionato")
    OnPropertyChanged("CanaliDaAggiungere")
    OnPropertyChanged("CanaliFileSelezionatoFiltrati")
  End Sub

  Public Function ImportaValori(Canali As List(Of String), FilePerormance As List(Of String), PathParentFileSorgente As String)

    Dim CartellaParentFileSorgente As New System.IO.DirectoryInfo(PathParentFileSorgente)

    For Each FlP As String In FilePerormance
      Dim Fi As New System.IO.FileInfo(FlP)
      Dim Cartella As String = Fi.Name


    Next

  End Function

  Public Function ImportaValoriManualmente()
    Dim Folder As String = "D:\Performance\Logs\AC75B2\LastSailingsTogetherTest\"
    Dim TipoFileSource As String = "ECC0"
    Dim FileSources As New List(Of String)
    If System.IO.Directory.Exists(Folder) Then
      Dim fld As New System.IO.DirectoryInfo(Folder)
      For Each fl In fld.GetFiles
        If fl.Name.StartsWith(TipoFileSource) Then
          FileSources.Add(fl.FullName)
        End If
      Next
      Dim Canali As New List(Of String)
      Canali.Add("StbdOutFlapFus_Ang")
      Canali.Add("StbdInFlapFus_Ang")
      Canali.Add("PortOutFlapFus_Ang")
      Canali.Add("PortInFlapFus_Ang")
      Return ImportaCanali(FileSources, Canali)
    End If
    Return False
  End Function


  Private Function ImportaCanali(FileSources As List(Of String), Canali As List(Of String)) As Boolean
    'TipoFileSource = ECC0, ECC1, ILS0, ILS1
    If Canali.Count = 0 Then Return False
    Dim CoppieInOut As New Dictionary(Of String, String)
    For Each fls As String In FileSources
      Dim finf As New System.IO.FileInfo(fls)
      Dim nome As String = "Performance__" & finf.Name.Substring(finf.Name.Length - 21)
      Dim perffullpath As String = finf.FullName.Replace(finf.Name, nome)
      If System.IO.File.Exists(perffullpath) Then
        CoppieInOut.Add(fls, perffullpath)
      End If
    Next

    Dim NewFilePerformance As New List(Of String)
    For i As Integer = 0 To CoppieInOut.Count - 1
      ' apro il file performance
      Dim Perf As New clsParquetFile(CoppieInOut.Values(i))
      ' apro il file source
      Dim Src As New clsParquetFile(CoppieInOut.Keys(i))
      Dim NuoviCanali As List(Of clsCanaleParquet) = Perf.Canali
      For Each cc In Canali
        Dim ccTmp As clsCanaleParquet = Src.Canali.Where(Function(x) x.Intestazione = cc).FirstOrDefault
        If ccTmp Is Nothing Then
          'cea il nuovo canale cn valori nan
          ccTmp = New clsCanaleParquet(cc, Src)
        End If
        NuoviCanali.Add(ccTmp)
      Next

      ' copio tutto nel nuovo file eccetto eventualmente i canali nuovi se giá esistenti
      CreaFileParquet(Perf.FilePath.Replace(".ppf", ".new"), Perf.Canali)

      ' se i nuovi canali non esistono riempio con nan
      ' se i nuovi canali esistono li creo e li riempio
      ' salvo il nuovo file con estensione new
    Next

    For i As Integer = 0 To CoppieInOut.Count - 1
      Dim Perf As String = CoppieInOut.Values(i)
      Dim ff As New System.IO.FileInfo(Perf)
      Dim OrigFolder As String = ff.Directory.FullName & "\OriginalFiles"
      If Not System.IO.Directory.Exists(OrigFolder) Then
        System.IO.Directory.CreateDirectory(OrigFolder)
      End If

      FileMove(Perf, ff.Directory.FullName & "\OriginalFiles\" & ff.Name) 'System.IO.File.Move(Perf, ff.Directory.FullName & "\OriginalFiles\" & ff.Name)
      FileMove(Perf.Replace(".ppf", ".new"), Perf)
    Next


    'Dim fi As New System.IO.FileInfo(Source_Files.First.FilePath)
    'Source_Files.First.LeggiFileParquet(True)
    'For Each f In fi.Directory.GetFiles
    '  If Not f.FullName = fi.FullName Then
    '    If f.Extension = ".parquet" Then
    '      If f.Name.Substring(0, f.Name.Length - 12) = fi.Name.Substring(0, f.Name.Length - 12) Then
    '        Source_Files.Add(New clsParquetFile(f.FullName))
    '        Source_Files.Last.LeggiFileParquet(True)
    '      End If
    '    End If
    '  End If
    'Next

    'Dim ListaCanali As New List(Of clsChannel2020)

    'For Each sf In Source_Files.OrderBy(Function(x) x.FileParquet.TimeRangeRealDateTime.Start)
    '  'Stop
    '  For Each cn In Canali
    '    Dim sfc = sf.Canali.Where(Function(x) x.Intestazione = cn).FirstOrDefault
    '    If Not sfc Is Nothing Then
    '      sfc.Valori = sf.FileParquet.CaricaValoriCanale(cn)

    '      Dim momentinan As Double = 0
    '      Dim indici As New List(Of Integer)
    '      Dim momenti As New List(Of DateTime)
    '      For ii As Integer = 0 To sf.FileParquet.TimeStamps.Count - 1
    '        Dim Momento As DateTime = sf.FileParquet.TimeStamps(ii)
    '        If Momento = Nothing Then
    '          momentinan += 1
    '        Else
    '          Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
    '          indici.Add(Id)
    '          momenti.Add(Momento)
    '          'ch.Valori(Id) = sfc.Valori(ii)
    '        End If
    '      Next
    '    End If
    '  Next
    'Next

    'Dim CreationTimeString As String = Now.ToString("HHmmss")
    'If AggiornaParquetCaricati Then
    '  For Each lf In DataProvider2020.Files
    '    AggiornaFileParquet(New clsParquetFile(lf.FullName), ListaCanali) ', CreationTimeString)
    '  Next

    '  Dim Csep As String = PathSepChar(DataProvider2020.Files.First.FullName)
    '  Dim OldFilesFolder As String = DataProvider2020.Files.First.DirectoryName & Csep & "oldfiles" & Csep & CreationTimeString
    '  If Not System.IO.Directory.Exists(OldFilesFolder) Then
    '    System.IO.Directory.CreateDirectory(OldFilesFolder)
    '  End If

    '  Dim Testo As String = String.Join(vbCrLf, CanaliDaAggiungere.Select(Function(x) x.ChannelId).ToList)
    '  MsgBox("Channels:" & vbCrLf & Testo & vbCrLf & "Added", MsgBoxStyle.OkOnly)

    '  For Each lf In DataProvider2020.Files
    '    Try

    '      Dim PathFileDestinazione As String = lf.DirectoryName & Csep & "oldfiles" & Csep & CreationTimeString & Csep & lf.Name
    '      System.IO.File.Move(lf.FullName, PathFileDestinazione)
    '      System.IO.File.Move(lf.FullName.Replace(".parquet", ".tmp"), lf.FullName)
    '    Catch ex As Exception
    '      Stop
    '    End Try
    '  Next


    'Else
    '  Dim Testo As String = String.Join(vbCrLf, CanaliDaAggiungere.Select(Function(x) x.ChannelId).ToList)
    '  MsgBox("Channels:" & vbCrLf & Testo & vbCrLf & "Successfully added", MsgBoxStyle.OkOnly)
    'End If



    Return True
  End Function

  Private Sub FileMove(strFrom As String, strTo As String)
    System.IO.File.Copy(strFrom, strTo)
    System.IO.File.Delete(strFrom)
  End Sub



  Private Sub CreaFileParquet(pathNewParquet As String, Canali As List(Of clsCanaleParquet))
    'Dim pfi As New System.IO.FileInfo(_PathFileSource)
    'Dim Estensione As String = pfi.Extension
    'Dim ParquetName As String = pfi.Name.Replace(Estensione, ".parquet")
    'Dim pathParquet As String = pfi.FullName.Replace(pfi.Name, ParquetName)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

    'Dim IdChannel = _Channels.Where(Function(x) x.ChannelName = "TgtId").FirstOrDefault
    Dim OutputChannels As New List(Of clsCanaleParquet)
    For Each canale In Canali
      If canale.FileSource.FileParquet Is Nothing Then
        canale.FileSource.LeggiFileParquet(False)
      End If
      If canale.FileSource.Canali.Where(Function(x) x.Intestazione = canale.Intestazione).FirstOrDefault Is Nothing Then
        Dim Nome As String = canale.Intestazione
        Dim co = OutputChannels.Where(Function(x) x.Intestazione = Nome).FirstOrDefault
        If co Is Nothing Then
          co = New clsCanaleParquet(Nome, Nothing)
          OutputChannels.Add(co)
        End If
        ReDim co.Valori(canale.FileSource.FileParquet.TimeStamps.Count - 1)
        For i As Integer = 0 To canale.FileSource.FileParquet.TimeStamps.Count - 1
          co.Valori(i) = Double.NaN
        Next
      Else
        canale.Valori = canale.FileSource.FileParquet.CaricaValoriCanale(canale.Intestazione)
        Dim Nome As String = canale.Intestazione
        Dim co = OutputChannels.Where(Function(x) x.Intestazione = Nome).FirstOrDefault
        If co Is Nothing Then
          co = New clsCanaleParquet(Nome, Nothing)
          OutputChannels.Add(co)
        End If
        co.Valori = canale.Valori
      End If
    Next
    For Each canale In OutputChannels ' _Channels
      Dim DataField As New Parquet.Data.DataField(canale.Intestazione, Parquet.Data.DataType.Double, False)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, canale.Valori.ToArray))
    Next
    Dim fsTmp As New System.IO.StreamWriter(pathNewParquet)
    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)
    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup
    For Each Canale In CanaliPq
      GroupWriter.WriteColumn(Canale)
    Next
    ParquetWriter.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()
  End Sub


  Public Function ImportaValori() As Boolean
    If CanaliDaAggiungere.Count = 0 Then Return False
    Dim fi As New System.IO.FileInfo(Source_Files.First.FilePath)
    Source_Files.First.LeggiFileParquet(True)
    For Each f In fi.Directory.GetFiles
      If Not f.FullName = fi.FullName Then
        If f.Extension = ".ppf" Then
          If f.Name.Substring(0, f.Name.Length - 12) = fi.Name.Substring(0, f.Name.Length - 12) Then
            Source_Files.Add(New clsParquetFile(f.FullName))
            Source_Files.Last.LeggiFileParquet(True)
          End If
        End If
      End If
    Next

    Dim ListaCanali As New List(Of clsChannel2020)

    For Each sf In Source_Files.OrderBy(Function(x) x.FileParquet.TimeRangeRealDateTime.Start)
      'Stop
      For Each cn In CanaliDaAggiungere
        Dim sfc = sf.Canali.Where(Function(x) x.Intestazione = cn.Intestazione).FirstOrDefault
        If Not sfc Is Nothing Then
          sfc.Valori = sf.FileParquet.CaricaValoriCanale(cn.Intestazione)

          ' 1. trovo o creo il canale del dataprovider
          Dim ch As clsChannel2020 = DataProvider2020.CanaleDbl(cn.ChannelId)
          If ch Is Nothing Then
            ch = CercaCanaleXML(cn.Intestazione)
            If ch Is Nothing Then
              ch = New clsChannel2020(cn.ChannelId, clsChannels2020.eCanaliChiave.eNone, cn.ShortName, cn.LongName, cn.ShortUM, cn.LongUM, cn.DataType, cn.ActualLogHeader, cn.KnownHeaders, "", False, cn.Decimals) ', DataProvider2020.Channels)
              'ch.Importa = AggiornaParquetCaricati
              ch.SalvaSuXML(DataProvider2020.SuffissoFileType, True)
            End If
            If AggiornaParquetCaricati Then SalvaNuovaIntestazioneDaMergiareNextTime(cn.ChannelId)
            DataProvider2020.Channels.ListaCanali.Add(ch)
            ReDim ch.Valori(DataProvider2020.TimeStamps.Count - 1)
            For ii As Integer = 0 To ch.Valori.Length - 1
              ch.Valori(ii) = Double.NaN
            Next
            ListaCanali.Add(ch)
          End If

          Dim momentinan As Double = 0
          Dim indici As New List(Of Integer)
          Dim momenti As New List(Of DateTime)
          For ii As Integer = 0 To sf.FileParquet.TimeStamps.Count - 1
            Dim Momento As DateTime = sf.FileParquet.TimeStamps(ii)
            If Momento = Nothing Then
              momentinan += 1
            Else
              Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
              indici.Add(Id)
              momenti.Add(Momento)
              ch.Valori(Id) = sfc.Valori(ii)
            End If
          Next
        End If
      Next
    Next

    Dim CreationTimeString As String = Now.ToString("HHmmss")
    If AggiornaParquetCaricati Then
      For Each lf In DataProvider2020.Files
        AggiornaFileParquet(New clsParquetFile(lf.FullName), ListaCanali) ', CreationTimeString)
      Next

      Dim Csep As String = PathSepChar(DataProvider2020.Files.First.FullName)
      Dim OldFilesFolder As String = DataProvider2020.Files.First.DirectoryName & Csep & "oldfiles" & Csep & CreationTimeString
      If Not System.IO.Directory.Exists(OldFilesFolder) Then
        System.IO.Directory.CreateDirectory(OldFilesFolder)
      End If

      Dim Testo As String = String.Join(vbCrLf, CanaliDaAggiungere.Select(Function(x) x.ChannelId).ToList)
      MsgBox("Channels:" & vbCrLf & Testo & vbCrLf & "Added", MsgBoxStyle.OkOnly)

      For Each lf In DataProvider2020.Files
        Try

          Dim PathFileDestinazione As String = lf.DirectoryName & Csep & "oldfiles" & Csep & CreationTimeString & Csep & lf.Name
          System.IO.File.Move(lf.FullName, PathFileDestinazione)
          System.IO.File.Move(lf.FullName.Replace(".ppf", ".tmp"), lf.FullName)
        Catch ex As Exception
          Stop
        End Try
      Next


    Else
      Dim Testo As String = String.Join(vbCrLf, CanaliDaAggiungere.Select(Function(x) x.ChannelId).ToList)
      MsgBox("Channels:" & vbCrLf & Testo & vbCrLf & "Successfully added", MsgBoxStyle.OkOnly)
    End If



    Return True
  End Function


  Private Sub AggiornaFileParquet(FileToBeUpdated As clsParquetFile, ListaCanaliDaAggiungere As List(Of clsChannel2020)) ', CreationTimeString As String)

    Dim CanaliPq As New List(Of Parquet.Data.DataColumn)
    For Each Canale In FileToBeUpdated.Canali
      Dim DataField As New Parquet.Data.DataField(Canale.Intestazione, Parquet.Data.DataType.Double, False)
      If Canale.FileSource.FileParquet Is Nothing Then
        Canale.FileSource.LeggiFileParquet(True)
      End If
      Dim Valori As Double() = Canale.FileSource.FileParquet.CaricaValoriCanale(Canale.Intestazione)
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, Valori))
    Next
    Dim idiniziale As Integer = DataProvider2020.TrovaIndice(FileToBeUpdated.FileParquet.TimeRangeRealDateTime.Start)
    Dim idfinale As Integer = DataProvider2020.TrovaIndice(FileToBeUpdated.FileParquet.TimeRangeRealDateTime.Finish)
    For Each canale In ListaCanaliDaAggiungere
      Dim DataField As New Parquet.Data.DataField(canale.ChannelId, Parquet.Data.DataType.Double, False)
      Dim Valori As New List(Of Double)
      For i As Integer = idiniziale To idfinale
        Valori.Add(canale.Valori(i))
      Next
      CanaliPq.Add(New Parquet.Data.DataColumn(DataField, Valori.ToArray))
    Next


    Dim OldFilePath As String = FileToBeUpdated.FilePath
    Dim NewFilePath As String = FileToBeUpdated.FilePath.Replace(".ppf", ".tmp")
    Dim fsTmp As New System.IO.StreamWriter(NewFilePath)
    Dim PqSchema As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
    Dim ParquetWriter As Parquet.ParquetWriter = New Parquet.ParquetWriter(PqSchema, fsTmp.BaseStream)

    Dim GroupWriter As Parquet.ParquetRowGroupWriter = ParquetWriter.CreateRowGroup

    For Each Canale In CanaliPq
      GroupWriter.WriteColumn(Canale)
    Next
    ParquetWriter.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()

    'Dim d As String = ""
    'Try
    '  Dim Csep As String = PathSepChar(FileToBeUpdated.FilePath)
    '  d &= "a"
    '  Dim OldFilesFolder As String = FileToBeUpdated.FileParquet.FileInfo.DirectoryName & Csep & "oldfiles" & Csep & CreationTimeString
    '  d &= "b"
    '  If Not System.IO.Directory.Exists(OldFilesFolder) Then
    '    d &= "c"
    '    System.IO.Directory.CreateDirectory(OldFilesFolder)
    '  End If
    '  Dim fi As New System.IO.FileInfo(OldFilePath)
    '  d &= "d"
    '  Dim c = 0
    '  Do While c < 10000
    '    c += 1
    '    Try
    '      System.IO.File.Copy(OldFilePath, OldFilesFolder & Csep & fi.Name)
    '      Exit Do
    '      'System.IO.File.Move(OldFilePath, OldFilesFolder & Csep & fi.Name)
    '      'Exit Do
    '    Catch ex As Exception
    '      Console.WriteLine(c)
    '    End Try
    '  Loop
    '  d &= "e"
    '  c = 0
    '  Do While c < 10000
    '    c += 1
    '    Try
    '      System.IO.File.Delete(OldFilePath)
    '      'System.IO.File.Copy(NewFilePath, NewFilePath.Replace(".tmp", ".parquet"))
    '      System.IO.File.Move(NewFilePath, NewFilePath.Replace(".tmp", ".parquet"))
    '      Exit Do
    '    Catch ex As Exception
    '      Console.WriteLine(c)
    '    End Try
    '  Loop
    '  d &= "f"
    'Catch ex As Exception
    '  Console.WriteLine(d)
    '  Stop
    'End Try

    'System.IO.File.Delete(OldFilePath)
    'If System.IO.File.Exists(fi.FullName.Replace(".parquet", ".old")) Then
    '  System.IO.File.Delete(fi.FullName.Replace(".parquet", ".old"))
    'End If
    'My.Computer.FileSystem.RenameFile(OldFilePath, fi.Name.Replace(".parquet", ".old1"))
    'My.Computer.FileSystem.RenameFile(NewFilePath, fi.Name)
  End Sub

  Private Sub SalvaNuovaIntestazioneDaMergiareNextTime(ChannelId As String)
    '"C:\Discodati\progettidotnet\Performance2021\Performance2021\Config\IlsEccMergingChannels.txt"
    Dim MergingFilePath As String = AppConfig.FI.FullName.Replace(AppConfig.FI.Name, "IlsEccMergingChannels.txt")
    If System.IO.File.Exists(MergingFilePath) Then
      Dim Righe As List(Of String) = System.IO.File.ReadAllLines(MergingFilePath).ToList
      Dim ListaCanali As New List(Of String)
      If Righe.Count > 0 Then
        For Each Riga In Righe
          ListaCanali.Add(Riga)
        Next
      End If
      If ListaCanali.Where(Function(x) x = ChannelId).Count = 0 Then
        ListaCanali.Add(ChannelId)
        ObjFiles.SalvaNuovoFileSostituendoContenuto(String.Join(vbCrLf, ListaCanali), MergingFilePath)
      End If
    End If

  End Sub



  Private Function CercaCanaleXML(ChannelId As String) As clsChannel2020
    Dim NodoCanali As Xml.XmlNode = AppConfig.CercaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eChannels, True)

    If Not NodoCanali Is Nothing Then
      For Each Nodo As Xml.XmlNode In NodoCanali
        If Nodo.Name = "Channel_" & ChannelId Then
          Dim pChannelId As String = Nodo.Name.Substring(8) 'i nodi dei canali iniziano con "Channel_"
          Dim pCanaleChiave As clsChannels2020.eCanaliChiave = clsChannels2020.eCanaliChiave.eNone
          Dim pShortName As String = pChannelId
          Dim pLongName As String = pChannelId
          Dim pShortUM As String = "°"
          Dim pLongUM As String = "Deg"
          Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
          Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
          Dim pActualLogHeader As String = pKnownHeaders.First
          Dim pPolarHeader As String = ""
          Dim pImporta As Boolean = False
          Dim pIsMath As Boolean = False
          Dim pDecimals As Integer = 1
          For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
            Select Case SottoNodo.Name
              Case "DataType"
                pDataType = CInt(SottoNodo.InnerText)
              Case "LongName"
                pLongName = SottoNodo.InnerText
              Case "ShortName"
                pShortName = SottoNodo.InnerText
              Case "LongUM"
                pLongUM = SottoNodo.InnerText
              Case "ShortUM"
                pShortUM = SottoNodo.InnerText
              Case "Load"
                pImporta = SottoNodo.InnerText
              Case "Decimals"
                pDecimals = CInt(SottoNodo.InnerText)
              Case "KeyChannel"
                pCanaleChiave = CInt(SottoNodo.InnerText.Trim)
              Case "Headers"
                pKnownHeaders = HeadersDaStringa(SottoNodo.InnerText.Trim)
              Case "PolarHeader"
                pPolarHeader = SottoNodo.InnerText
              Case "IsMath"
                pIsMath = CBool(SottoNodo.InnerText)
              Case Else
                Stop
            End Select
          Next
          Dim CanaleTmp As New clsChannel2020(pChannelId, pCanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pIsMath, pDecimals) ', DataProvider2020.Channels)
          'CanaleTmp.Importa = True
          Return CanaleTmp
        End If
      Next
    End If
    Return Nothing
  End Function

  Private Function HeadersDaStringa(StringaHeaders As String) As List(Of String)
    If StringaHeaders.IndexOf(",") > -1 Then
      Return StringaHeaders.Split(",").ToList
    Else
      Return New List(Of String)({StringaHeaders})
    End If
  End Function


End Class

Public Class clsCanaleParquetAdvanced
  Inherits clsCanaleParquet
  Implements INotifyPropertyChanged

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim _ChannelId As String
  Dim _ShortName As String
  Dim _LongName As String
  Dim _ShortUM As String
  Dim _LongUM As String
  Dim _DataType As clsChannel2020.eDataType
  Dim _ActualLogHeader As String
  Dim _KnownHeaders As List(Of String)
  Dim _Decimals As Integer
  Dim _DataTypes As List(Of clsChannel2020.eDataType)

  Public Property ShortName As String
    Get
      Return _ShortName
    End Get
    Set(value As String)
      _ShortName = value
      OnPropertyChanged("ShortName")
    End Set
  End Property

  Public Property LongName As String
    Get
      Return _LongName
    End Get
    Set(value As String)
      _LongName = value
      OnPropertyChanged("LongName")
    End Set
  End Property

  Public Property ShortUM As String
    Get
      Return _ShortUM
    End Get
    Set(value As String)
      _ShortUM = value
      OnPropertyChanged("ShortUM")
    End Set
  End Property

  Public Property LongUM As String
    Get
      Return _LongUM
    End Get
    Set(value As String)
      _LongUM = value
      OnPropertyChanged("LongUM")
    End Set
  End Property

  Public Property DataType As clsChannel2020.eDataType
    Get
      Return _DataType
    End Get
    Set(value As clsChannel2020.eDataType)
      _DataType = value
      OnPropertyChanged("DataType")
    End Set
  End Property

  Public Property ActualLogHeader As String
    Get
      Return _ActualLogHeader
    End Get
    Set(value As String)
      _ActualLogHeader = value
      OnPropertyChanged("ActualLogHeader")
    End Set
  End Property

  Public Property KnownHeaders As List(Of String)
    Get
      Return _KnownHeaders
    End Get
    Set(value As List(Of String))
      _KnownHeaders = value
      OnPropertyChanged("KnownHeaders")
    End Set
  End Property

  Public Property Decimals As Integer
    Get
      Return _Decimals
    End Get
    Set(value As Integer)
      _Decimals = value
      OnPropertyChanged("Decimals")
    End Set
  End Property

  Public Property ChannelId As String
    Get
      Return _ChannelId
    End Get
    Set(value As String)
      _ChannelId = value
      OnPropertyChanged("ChannelId")
    End Set
  End Property

  Public Property DataTypes As List(Of clsChannel2020.eDataType)
    Get
      Return _DataTypes
    End Get
    Set(value As List(Of clsChannel2020.eDataType))
      _DataTypes = value
      OnPropertyChanged("DataTypes")
    End Set
  End Property

  Public Sub New(Intestazione As String, FileSource As clsParquetFile)
    MyBase.New(Intestazione, FileSource)

    _ChannelId = Intestazione
    _ShortName = Intestazione
    _LongName = Intestazione
    _ShortUM = "°"
    _LongUM = "Deg"
    _DataType = clsChannel2020.eDataType.eLinear
    _ActualLogHeader = Intestazione
    _KnownHeaders = New List(Of String)({Intestazione})
    _Decimals = 1

    DataTypes = System.Enum.GetValues(GetType(clsChannel2020.eDataType)).Cast(Of clsChannel2020.eDataType).ToList

    'For Each valore In System.Enum.GetValues(GetType(clsChannel2020.eDataType)).Cast(Of clsChannel2020.eDataType)
    '  DataTypes.Add(valore)
    'Next
  End Sub

  Protected Overrides Sub Finalize()
    MyBase.Finalize()
  End Sub

  Public Overrides Function ToString() As String
    Return MyBase.ToString()
  End Function

  Public Overrides Function Equals(obj As Object) As Boolean
    Return MyBase.Equals(obj)
  End Function

  Public Overrides Function GetHashCode() As Integer
    Return MyBase.GetHashCode()
  End Function
End Class
