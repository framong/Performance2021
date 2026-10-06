''' <summary>Finestra modale per la tabella TWS / SeaState normalizzato del profilo attivo. Lavora su una copia fino a Save.</summary>
Public Class SeaStateNormForm

  Private _Tab As System.Collections.ObjectModel.ObservableCollection(Of clsSeaStateNormPoint)

  Public Sub New()
    InitializeComponent()
    If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) Then Exit Sub
    Mostra(clsSeaStateNorm.TabellaCorrente())
  End Sub

  Private Sub Mostra(Tab As List(Of clsSeaStateNormPoint))
    _Tab = New System.Collections.ObjectModel.ObservableCollection(Of clsSeaStateNormPoint)(
      Tab.Select(Function(p) New clsSeaStateNormPoint(p.Tws, p.SeaState)))
    grd_Tabella.ItemsSource = _Tab
  End Sub

  Private Sub btn_Defaults_Click(sender As Object, e As RoutedEventArgs)
    Mostra(clsSeaStateNorm.TabellaDefault())
  End Sub

  Private Sub btn_Save_Click(sender As Object, e As RoutedEventArgs)
    grd_Tabella.CommitEdit(DataGridEditingUnit.Row, True)
    Dim Tab = _Tab.OrderBy(Function(p) p.Tws).ToList
    If Tab.Count < 2 Then
      MsgBox("At least two points are needed.", MsgBoxStyle.Exclamation, "Sea State Normalized")
      Exit Sub
    End If
    If Tab.Select(Function(p) p.Tws).Distinct().Count() <> Tab.Count Then
      MsgBox("TWS values must be unique.", MsgBoxStyle.Exclamation, "Sea State Normalized")
      Exit Sub
    End If
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Exit Sub
    AppConfig.ActiveProfile.SeaStateNormTable = Tab
    AppConfig.Salva()
    Me.DialogResult = True
  End Sub

  Private Sub btn_Cancel_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
  End Sub

End Class
