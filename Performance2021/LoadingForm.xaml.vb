Public Class LoadingForm

  Private Sub LoadingForm_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
    'Dim VM As New LoadingFormVM
    'Me.DataContext = VM

  End Sub

End Class


Public Class clsProfileSelection
  Public Property ListaProfili As New List(Of String)
  Public Property ProfiloAttivo As String

  Public Property AppTmp = New clsSettings2021

  Public Sub New()

    ' apro la lista dei profili
    ' se ne esiste uno solo vado avanti con quello
    ' se ne esiste piu' di uno
    '   e' premuto il tasto shift apro la finestra di dialogo
    '   non e' premuto nulla apro quello selezionato
    LeggiFileDataPath()
    If Not System.IO.File.Exists(ProfiloAttivo) Then
      MsgBox("Profile Not Found! " & vbCrLf & "a new profile will be created in the BoatProfile folder")
      ListaProfili.Clear()
      CreaStrutturaVuota()
      Dim ptmp As clsProfile2021 = clsKillerSeriale.LoadConfigurationGeneric(Of clsProfile2021)(ProfiloAttivo)
      If ptmp Is Nothing Then
        AppTmp.ActiveProfile = New clsProfile2021
        AppTmp.ActiveProfile.ActiveProfileFilePath = ProfiloAttivo
        AppTmp.ActiveProfile.ProfileName = "MyBoat"
        AppTmp.ActiveProfile.ImpostaValoriDefaultMancanti()
        ' scrive subito il profilo: cosi' le cartelle standard e i path di default
        ' sono gia' registrati anche se l'utente chiude l'applicazione senza fare altro
        AppTmp.Salva()
        AppTmp.ProfileLoaded = True
        Exit Sub
      End If
      'End
    End If
    Dim SelProf As Boolean = System.IO.File.Exists(System.IO.Path.Combine(My.Application.Info.DirectoryPath, "selprof"))
    If ListaProfili.Count > 1 Then
      If SelProf OrElse My.Computer.Keyboard.CtrlKeyDown Then
        Dim f As New SelectProfileForm
        For Each p As String In ListaProfili
          f.VM.ListaProfili.Add(New Profilo(p, p = ProfiloAttivo))
        Next
        f.ShowDialog()
        If f.VM.Save Then
          ListaProfili.Clear()
          Dim Contenuto As String = ""
          For Each p In f.VM.ListaProfili
            Contenuto &= p.FilePath
            ListaProfili.Add(p.FilePath)
            If p.IsSelected Then
              Contenuto &= "*"
              ProfiloAttivo = p.FilePath
            End If
            Contenuto &= vbCrLf
          Next
          ObjFiles.SalvaNuovoFileSostituendoContenuto(Contenuto.TrimEnd(vbCrLf), AppTmp.DataPathFile)
        End If
      End If
    End If

    AppTmp.ActiveProfile = clsKillerSeriale.LoadConfigurationGeneric(Of clsProfile2021)(ProfiloAttivo)
    If AppTmp.ActiveProfile Is Nothing Then
      ' file .prf illeggibile o vuoto: creo un profilo nuovo invece di andare in NullReference
      AppTmp.ActiveProfile = New clsProfile2021
      AppTmp.ActiveProfile.ProfileName = System.IO.Path.GetFileNameWithoutExtension(ProfiloAttivo)
    End If
    AppTmp.ActiveProfile.ActiveProfileFilePath = ProfiloAttivo
    AppTmp.ActiveProfile.ImpostaValoriDefaultMancanti()
    AppTmp.ProfileLoaded = True

    'Stop
    'LeggiFileDataPath()
    'AppConfig = AppTmp

  End Sub

  Private Sub CreaStrutturaVuota()
    Dim pTmp As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "BoatProfile")
    If Not System.IO.Directory.Exists(pTmp) Then
      System.IO.Directory.CreateDirectory(pTmp)
      System.IO.Directory.CreateDirectory(System.IO.Path.Combine(pTmp, "Target"))
      System.IO.Directory.CreateDirectory(System.IO.Path.Combine(pTmp, "Data"))
      System.IO.Directory.CreateDirectory(System.IO.Path.Combine(pTmp, "Settings"))
    End If
    'Dim file As System.IO.StreamWriter
    'file = My.Computer.FileSystem.OpenTextFileWriter(AppTmp.DataPathFile, True)
    'ProfiloAttivo = System.IO.Path.Combine(pTmp, "Default.prf")
    'file.WriteLine(ProfiloAttivo)
    'file.Close()
    'ListaProfili.Add(ProfiloAttivo)
  End Sub


  Private Sub LeggiFileDataPath()
    ListaProfili.Clear()
    If Not System.IO.File.Exists(AppTmp.DataPathFile) Then
      Dim file As System.IO.StreamWriter
      file = My.Computer.FileSystem.OpenTextFileWriter(AppTmp.DataPathFile, True)
      Dim pTmp As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "BoatProfile")
      ProfiloAttivo = System.IO.Path.Combine(pTmp, "Default.prf")
      file.WriteLine(ProfiloAttivo)
      file.Close()
    End If
    Dim Contenuto As String = My.Computer.FileSystem.ReadAllText(AppTmp.DataPathFile).Trim
    Dim c As Char() = {vbCr, vbLf}
    Dim files As String() = Contenuto.Split(c, StringSplitOptions.RemoveEmptyEntries)
    Dim ActiveProfileFile As String = ""
    If files.Count = 0 Then
      Dim file As System.IO.StreamWriter
      file = My.Computer.FileSystem.OpenTextFileWriter(AppTmp.DataPathFile, True)
      file.WriteLine("C:\Performance2021\Profiles\Default\Default.prf")
      file.Close()
      ProfiloAttivo = "C:\Performance2021\Profiles\Default\Default.prf"
      ListaProfili.Add(ProfiloAttivo)
    ElseIf files.Count = 1 Then
      ProfiloAttivo = My.Computer.FileSystem.ReadAllText(AppTmp.DataPathFile).Trim.TrimEnd("*")
      ListaProfili.Add(ProfiloAttivo)
    Else
      Dim tmp As String = files.Where(Function(x) x.ToString.EndsWith("*")).FirstOrDefault()
      If tmp = Nothing Then
        Stop
        End
      Else
        ProfiloAttivo = tmp.Trim.TrimEnd("*")
        For Each fl In files
          ListaProfili.Add(fl.Trim.TrimEnd("*"))
        Next
      End If
    End If

  End Sub



End Class