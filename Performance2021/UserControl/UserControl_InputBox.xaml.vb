
Public Class UserControl_InputBox

  Public ReadOnly Property Testo As String
    Get
      Return txt_Testo.Text
    End Get
  End Property

  Public Sub New(TestoIniziale As String, Titolo As String)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    Me.Title = Titolo
    txt_Testo.Text = TestoIniziale

  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
    Me.Close()
  End Sub



End Class
