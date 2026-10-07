''' <summary>
''' Selettore di canale con ricerca, al posto di una ComboBox editabile: un pulsante che mostra il canale corrente e,
''' al clic, apre un popup con una casella di ricerca e la lista filtrata. Ha un comportamento prevedibile (nessuna delle
''' stranezze delle ComboBox editabili su selezione, focus e testo selezionato aprendo la lista).
'''  - il filtro tiene i canali il cui LongName o ShortName contiene tutte le parole scritte (senza maiuscole/minuscole);
'''  - Su/Giu spostano la selezione, Invio sceglie, Esc chiude; il clic su una voce la sceglie;
'''  - SelectedChannel e' a doppio binding: scegliere una voce scrive il canale nel modello, cambiarlo dal codice aggiorna il testo.
''' ItemsSource si legge all'apertura del popup, quindi riflette sempre la lista corrente dei canali.
''' </summary>
Public Class ChannelPicker

  Public Shared ReadOnly ItemsSourceProperty As DependencyProperty =
    DependencyProperty.Register("ItemsSource", GetType(IEnumerable), GetType(ChannelPicker), New PropertyMetadata(Nothing))

  Public Shared ReadOnly SelectedChannelProperty As DependencyProperty =
    DependencyProperty.Register("SelectedChannel", GetType(clsChannel2020), GetType(ChannelPicker),
      New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnSelectedChannelChanged))

  Public Property ItemsSource As IEnumerable
    Get
      Return DirectCast(GetValue(ItemsSourceProperty), IEnumerable)
    End Get
    Set(value As IEnumerable)
      SetValue(ItemsSourceProperty, value)
    End Set
  End Property

  Public Property SelectedChannel As clsChannel2020
    Get
      Return DirectCast(GetValue(SelectedChannelProperty), clsChannel2020)
    End Get
    Set(value As clsChannel2020)
      SetValue(SelectedChannelProperty, value)
    End Set
  End Property

  Private _Tutti As New List(Of clsChannel2020)
  Private _UltimaChiusura As DateTime = DateTime.MinValue

  Public Sub New()
    InitializeComponent()
  End Sub

  Private Shared Sub OnSelectedChannelChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
    Dim p As ChannelPicker = TryCast(d, ChannelPicker)
    If p Is Nothing Then Exit Sub
    Dim c As clsChannel2020 = TryCast(e.NewValue, clsChannel2020)
    p.txt_Nome.Text = If(c Is Nothing, "", c.LongName)
    p.btn_Apri.ToolTip = If(c Is Nothing, "Click to choose a channel", c.LongName & " (click to change; type to filter)")
  End Sub

  ' --- apertura / chiusura ---

  Private Sub btn_Apri_Click(sender As Object, e As RoutedEventArgs)
    ' il clic che chiude il popup (clic fuori) arriva prima del Click del pulsante: senza questo controllo
    ' cliccando di nuovo sul pulsante per chiudere, il popup si richiuderebbe e riaprirebbe subito
    If (DateTime.Now - _UltimaChiusura).TotalMilliseconds < 250 Then Exit Sub
    pop_Lista.IsOpen = True
  End Sub

  Private Sub pop_Lista_Opened(sender As Object, e As EventArgs)
    _Tutti = New List(Of clsChannel2020)
    If Not ItemsSource Is Nothing Then
      For Each o As Object In ItemsSource
        Dim c As clsChannel2020 = TryCast(o, clsChannel2020)
        If Not c Is Nothing Then _Tutti.Add(c)
      Next
    End If
    txt_Cerca.Text = ""
    Filtra()
    ' la casella di ricerca prende il focus dopo l'apertura, cosi' si scrive subito
    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
        Sub()
          txt_Cerca.Focus()
          Keyboard.Focus(txt_Cerca)
          If Not lst_Canali.SelectedItem Is Nothing Then lst_Canali.ScrollIntoView(lst_Canali.SelectedItem)
        End Sub)
  End Sub

  Private Sub pop_Lista_Closed(sender As Object, e As EventArgs)
    _UltimaChiusura = DateTime.Now
  End Sub

  ' --- filtro ---

  Private Shared Function Corrisponde(c As clsChannel2020, Parole() As String) As Boolean
    Dim Nomi As String = (c.LongName & " " & c.ShortName).ToLowerInvariant
    Return Parole.All(Function(p) Nomi.Contains(p))
  End Function

  Private Sub Filtra()
    Dim Parole() As String = txt_Cerca.Text.ToLowerInvariant.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
    Dim Trovati As List(Of clsChannel2020) = If(Parole.Length = 0, _Tutti, _Tutti.Where(Function(c) Corrisponde(c, Parole)).ToList)
    lst_Canali.ItemsSource = Trovati
    txt_Conteggio.Text = Trovati.Count & " of " & _Tutti.Count & " channels"
    If Trovati.Count = 0 Then Exit Sub
    ' evidenzia il canale corrente se e' in lista (senza ricerca), altrimenti il primo che corrisponde
    Dim Corrente As clsChannel2020 = SelectedChannel
    If Parole.Length = 0 AndAlso Not Corrente Is Nothing AndAlso Trovati.Contains(Corrente) Then
      lst_Canali.SelectedItem = Corrente
    Else
      lst_Canali.SelectedIndex = 0
    End If
  End Sub

  Private Sub txt_Cerca_TextChanged(sender As Object, e As TextChangedEventArgs)
    If Not pop_Lista.IsOpen Then Exit Sub
    Filtra()
    If Not lst_Canali.SelectedItem Is Nothing Then lst_Canali.ScrollIntoView(lst_Canali.SelectedItem)
  End Sub

  ' --- scelta ---

  Private Sub Scegli(c As clsChannel2020)
    If c Is Nothing Then Exit Sub
    pop_Lista.IsOpen = False
    SelectedChannel = c
  End Sub

  Private Sub txt_Cerca_PreviewKeyDown(sender As Object, e As KeyEventArgs)
    Dim n As Integer = lst_Canali.Items.Count
    Select Case e.Key
      Case Key.Down
        If n > 0 Then Muovi(Math.Min(n - 1, lst_Canali.SelectedIndex + 1))
        e.Handled = True
      Case Key.Up
        If n > 0 Then Muovi(Math.Max(0, lst_Canali.SelectedIndex - 1))
        e.Handled = True
      Case Key.PageDown
        If n > 0 Then Muovi(Math.Min(n - 1, lst_Canali.SelectedIndex + 10))
        e.Handled = True
      Case Key.PageUp
        If n > 0 Then Muovi(Math.Max(0, lst_Canali.SelectedIndex - 10))
        e.Handled = True
      Case Key.Enter
        Scegli(TryCast(lst_Canali.SelectedItem, clsChannel2020))
        e.Handled = True
      Case Key.Escape
        pop_Lista.IsOpen = False
        e.Handled = True
    End Select
  End Sub

  Private Sub Muovi(Indice As Integer)
    lst_Canali.SelectedIndex = Indice
    If Not lst_Canali.SelectedItem Is Nothing Then lst_Canali.ScrollIntoView(lst_Canali.SelectedItem)
  End Sub

  Private Sub lst_Canali_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
    ' solo il clic su una voce (non sulla barra di scorrimento)
    Dim o As DependencyObject = TryCast(e.OriginalSource, DependencyObject)
    While Not o Is Nothing AndAlso Not TypeOf o Is ListBoxItem
      ' le parti di testo (Run) non sono Visual: per quelle si risale dall'albero logico
      If TypeOf o Is Visual OrElse TypeOf o Is Media.Media3D.Visual3D Then
        o = VisualTreeHelper.GetParent(o)
      Else
        o = LogicalTreeHelper.GetParent(o)
      End If
    End While
    Dim Voce As ListBoxItem = TryCast(o, ListBoxItem)
    If Voce Is Nothing Then Exit Sub
    Scegli(TryCast(Voce.DataContext, clsChannel2020))
  End Sub

End Class
