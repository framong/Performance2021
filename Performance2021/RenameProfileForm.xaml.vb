''' <summary>
''' Permette di cambiare il nome della barca (ProfileName) nel profilo attivo.
''' Non tocca ne' il file .prf ne' le cartelle: solo la proprieta' e il salvataggio.
''' </summary>
Public Class RenameProfileForm

  ''' <summary>Nome confermato dall'utente, valorizzato solo se DialogResult = True.</summary>
  Public Property NuovoNome As String = ""

  Public Sub New()
    InitializeComponent()

    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      txt_CurrentName.Text = ""
      Exit Sub
    End If

    txt_CurrentName.Text = AppConfig.ActiveProfile.ProfileName
    txt_NewName.Text = AppConfig.ActiveProfile.ProfileName
    txt_ProfilePath.Text = "Profile file: " & AppConfig.ActiveProfile.ProfileFilePath
  End Sub

  Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
    txt_NewName.Focus()
    txt_NewName.SelectAll()
  End Sub

  Private Sub btn_Ok_Click(sender As Object, e As RoutedEventArgs)
    Dim n As String = txt_NewName.Text.Trim

    If String.IsNullOrWhiteSpace(n) Then
      MsgBox("The profile name cannot be empty.", MsgBoxStyle.Exclamation, "Rename Profile")
      Exit Sub
    End If

    If n = txt_CurrentName.Text Then
      Me.DialogResult = False
      Me.Close()
      Exit Sub
    End If

    If MsgBox("Rename the profile from """ & txt_CurrentName.Text & """ to """ & n & """?" & vbCrLf & vbCrLf &
              "The profile file and folders will not be moved or renamed.",
              MsgBoxStyle.OkCancel Or MsgBoxStyle.Question, "Rename Profile") <> MsgBoxResult.Ok Then
      Exit Sub
    End If

    NuovoNome = n
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub btn_Cancel_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
    Me.Close()
  End Sub

End Class
