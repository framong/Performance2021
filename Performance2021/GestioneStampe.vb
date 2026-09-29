Public Class clsStampaTabellaStraightLine





End Class


Public Class clsStampaVisualizzazione
  Public Sub New(Grigliadati As DataGrid, Titolo As String)

    Dim pd As New PrintDialog
    Dim stampa As Boolean = pd.ShowDialog
    If stampa Then
      pd.PrintVisual(Grigliadati, Titolo)
    End If

  End Sub
End Class
