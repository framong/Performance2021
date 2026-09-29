Imports PropertyChanged

Public Class SelectProfileForm
    Public VM As New SelectProfileVM

  Public Sub New()
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    'Me.OpenButton.Content = VM.SelectedProfileName

    ' Add any initialization after the InitializeComponent() call.

  End Sub

  Private Sub Lista_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Me.OpenButton.Content = VM.SelectedProfileName
    VM.Save = True
    Me.Close()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        VM.Save = True
        Me.Close()
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        VM.Save = False
        Me.Close()
    End Sub

  Private Sub Lista_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
    Me.OpenButton.Content = VM.SelectedProfileName
  End Sub

  Private Sub SelectProfileForm_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
    Me.OpenButton.Content = VM.SelectedProfileName
  End Sub
End Class


<AddINotifyPropertyChangedInterface>
Public Class SelectProfileVM
    Public Property ListaProfili As New List(Of Profilo)
  Public Property Save As Boolean = False

  Public ReadOnly Property SelectedProfileName As String
    Get
      If ListaProfili Is Nothing Then Return "Open Selected"
      If ListaProfili.Count = 0 Then Return "Open Selected"
      Dim p = ListaProfili.Where(Function(x) x.IsSelected).FirstOrDefault
      Return "Open " & p.Descrizione
    End Get
  End Property

End Class


<AddINotifyPropertyChangedInterface>
Public Class Profilo

    Public ReadOnly Property Descrizione
        Get
            Return FileInf.Directory.Name & " ( Profile: '" & FileInf.Name.Replace(FileInf.Extension, "") & "' )"
        End Get
    End Property

    Public Sub New(ProfileFilePath As String, Selected As Boolean)
        FilePath = ProfileFilePath
        IsSelected = Selected
        FileInf = New System.IO.FileInfo(FilePath)
    End Sub

    Public Property FileInf As System.IO.FileInfo
    Public Property FilePath As String
    Public Property IsSelected As Boolean


End Class