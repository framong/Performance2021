Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged
Imports SPwpf

Public Class UserControlChannelSelectorTwin

  Dim pStatus As eStatus = eStatus.eCancel
  Dim pPuntoNotSelected As Point
  Dim pPuntoSelected As Point
  Public Property VM As ChannelSelectorTwoFramesViewModel
  Dim CurrentChannel As clsChannelAdv

  Public Enum eStatus
    eSave = 0
    eCancel = 1
  End Enum

  Public Property Status As eStatus
    Get
      Return pStatus
    End Get
    Set(value As eStatus)
      pStatus = value
    End Set
  End Property

  Public Sub New()
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

  End Sub

  Public Sub New(Titolo As String, ListaCanaliDisponibili As List(Of clsChannel2020), ListaCanaliSelezionati As List(Of clsChannel2020))
    Me.Title = Titolo
    VM = New ChannelSelectorTwoFramesViewModel(ListaCanaliDisponibili, ListaCanaliSelezionati)
    Me.DataContext = VM

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.


  End Sub

  Private Sub Btn_Salva_Click(sender As Object, e As RoutedEventArgs) Handles btn_Salva.Click
    pStatus = eStatus.eSave
    Me.Close()
  End Sub

  Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles btn_Cancel.Click
    pStatus = eStatus.eCancel
    Me.Close()
  End Sub

  'Private Sub Btn_Reset_Click(sender As Object, e As RoutedEventArgs) Handles btn_Reset.Click
  '  'VM.MuoviCanale()
  '  'pAvailableChannels.RipristinaListe()
  'End Sub

  Private Sub ListaNotSelected_PreviewMouseMove(sender As Object, e As MouseEventArgs)
    If e.LeftButton = MouseButtonState.Pressed Then
      If CurrentChannel Is Nothing Then
        Dim LV As ListView = DirectCast(sender, ListView)
        CurrentChannel = DirectCast(LV.SelectedItem, clsChannelAdv)
      End If
    Else
      CurrentChannel = Nothing
    End If
  End Sub

  Private Sub ListaSelected_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
    pPuntoSelected = e.GetPosition(Nothing)
    pPuntoNotSelected = Nothing
  End Sub

  Private Sub ListaSelected_PreviewMouseMove(sender As Object, e As MouseEventArgs)
    'If Not pPuntoNotSelected = Nothing Then Exit Sub
    If e.LeftButton = MouseButtonState.Pressed Then
      If CurrentChannel Is Nothing Then
        Dim LV As ListView = DirectCast(sender, ListView)
        CurrentChannel = DirectCast(LV.SelectedItem, clsChannelAdv)
      End If
    Else
      CurrentChannel = Nothing
    End If

  End Sub

  Private Sub ListaSelected_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
    'Dim PuntoDrag As Point = New Point(0, 0)
    'If Not pPuntoSelected = Nothing Then
    '  'viene da selected
    '  PuntoDrag = pPuntoSelected
    'Else
    '  'viene da non selected
    'End If
    'Dim PuntoDrop As Point = e.GetPosition(Nothing)
    'Dim Delta As Vector = PuntoDrag - PuntoDrop
    'If System.Math.Abs(Delta.X) > SystemParameters.MinimumHorizontalDragDistance Or System.Math.Abs(Delta.Y) > SystemParameters.MinimumVerticalDragDistance Then
    '  Dim LV As ListView = DirectCast(sender, ListView)
    '  Dim ToBeReplacedChannel = DirectCast(LV.SelectedItem, clsChannelAdv)
    '  VM.RiordinaCanale(CurrentChannel, ToBeReplacedChannel)
    '  CurrentChannel = Nothing
    'End If

  End Sub

  Private Sub Btn_NotSelToSel_Click(sender As Object, e As RoutedEventArgs)
    'VM.MuoviCanale(ListaNotSelected.SelectedItem, ListaSelected.SelectedItem)
    'VM.AggiungiCanale_ToSelezionati(ListaNotSelected.SelectedItem, ListaSelected.SelectedItem)
    VM.MuoviCanale_ToSelezionati()
  End Sub

  Private Sub Btn_SelToNotSel_Click(sender As Object, e As RoutedEventArgs)
    'VM.MuoviCanale(ListaSelected.SelectedItem, ListaNotSelected.SelectedItem)
    VM.MuoviCanale_ToNonSelezionati()
  End Sub

  Private Sub ListaNotSelected_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles ListaNotSelected.PreviewMouseLeftButtonDown

  End Sub

  Private Sub ListaNotSelected_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Dim cs = VM.ListaFiltrata.Where(Function(x) x.IsSelected).FirstOrDefault
    If cs Is Nothing Then Exit Sub
    VM.ListaFiltrata.Remove(cs)
    VM.CanaliNonSelezionati.Remove(cs)
    Dim css = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    If css Is Nothing Then
      VM.CanaliSelezionati.Add(cs)
    Else
      Dim idx = VM.CanaliSelezionati.IndexOf(css)
      VM.CanaliSelezionati.Insert(idx, cs)
    End If
    cs.IsSelected = False
  End Sub

  Private Sub ListaSelected_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    If cs Is Nothing Then Exit Sub
    VM.CanaliSelezionati.Remove(cs)
    VM.ListaFiltrata.Insert(0, cs)
    VM.CanaliNonSelezionati.Insert(0, cs)
    cs.IsSelected = False
  End Sub


  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.Move(ChannelSelectorTwoFramesViewModel.eMoveType.eTop)
    'Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    'If cs Is Nothing Then Exit Sub
    '' top
    'VM.CanaliSelezionati.Remove(cs)
    'VM.CanaliSelezionati.Insert(0, cs)
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    VM.Move(ChannelSelectorTwoFramesViewModel.eMoveType.eUp)
    'Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    'If cs Is Nothing Then Exit Sub
    ''up
    'Dim idx As Integer = VM.CanaliSelezionati.IndexOf(cs) - 1
    'VM.CanaliSelezionati.Remove(cs)
    'VM.CanaliSelezionati.Insert(Math.Max(idx, 0), cs)
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    VM.Move(ChannelSelectorTwoFramesViewModel.eMoveType.eDn)
    'Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    'If cs Is Nothing Then Exit Sub
    ''dn
    'Dim idx As Integer = VM.CanaliSelezionati.IndexOf(cs) + 1
    'VM.CanaliSelezionati.Remove(cs)
    'VM.CanaliSelezionati.Insert(Math.Min(idx, VM.CanaliSelezionati.Count), cs)

  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    VM.Move(ChannelSelectorTwoFramesViewModel.eMoveType.eBottom)
    'Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    'If cs Is Nothing Then Exit Sub
    '' bottom
    'VM.CanaliSelezionati.Remove(cs)
    'VM.CanaliSelezionati.Insert(VM.CanaliSelezionati.Count, cs)
  End Sub

  Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)
    Dim cs = VM.CanaliSelezionati.Where(Function(x) x.IsSelected).FirstOrDefault
    If cs Is Nothing Then Exit Sub
    VM.CanaliSelezionati.Remove(cs)
    VM.ListaFiltrata.Insert(0, cs)
    VM.CanaliNonSelezionati.Insert(0, cs)
    cs.IsSelected = False
  End Sub

  Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)
    TxtBoxTestoFiltro.Text = ""
    'VM.TestoRicerca = ""
  End Sub
End Class



<AddINotifyPropertyChangedInterface>
Public Class ChannelSelectorTwoFramesViewModel
  'Implements INotifyPropertyChanged
  Public Property ListaFiltrata As New ObservableCollection(Of clsChannel2020)
  Public Property CanaliNonSelezionati As New ObservableCollection(Of clsChannel2020)
  Public Property CanaliSelezionati As New ObservableCollection(Of clsChannel2020)
  Public Property CanaleNonSelezionato As clsChannel2020
  Public Property CanaleSelezionato As clsChannel2020

  Dim _TestoRicerca As String
  Public Property TestoRicerca As String
    Get
      Return _TestoRicerca
    End Get
    Set(value As String)
      _TestoRicerca = value
      AggiornaListaFiltrata()
    End Set
  End Property

  Private Sub AggiornaListaFiltrata()
    If TestoRicerca.Trim = "" Then
      ListaFiltrata.Clear()
      For Each e In CanaliNonSelezionati.OrderBy(Function(x) x.LongName).ToList
        ListaFiltrata.Add(e)
      Next
    Else
      ListaFiltrata.Clear()
      For Each e In CanaliNonSelezionati.OrderBy(Function(x) x.LongName).ToList
        If e Is CanaleNonSelezionato Then
          ListaFiltrata.Add(e)
        ElseIf e.ActualLogHeader.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
          ListaFiltrata.Add(e)
        ElseIf e.CanaleChiaveStringa.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
          ListaFiltrata.Add(e)
        ElseIf e.ChannelId.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
          ListaFiltrata.Add(e)
        ElseIf e.ShortName.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
          ListaFiltrata.Add(e)
        ElseIf e.LongName.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
          ListaFiltrata.Add(e)
        Else
          If Not e.KnownHeaders Is Nothing Then
            For Each nh In e.KnownHeaders
              If nh.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
                ListaFiltrata.Add(e)
              End If
            Next
          End If
        End If
      Next
    End If
    If Not CanaleNonSelezionato Is Nothing Then
      ListaFiltrata.Remove(CanaleNonSelezionato)
      ListaFiltrata.Insert(0, CanaleNonSelezionato)
    End If

  End Sub

  Public Enum eMoveType
    eTop
    eUp
    eDn
    eBottom
  End Enum

  Public Sub Move(MoveTo As eMoveType)
    If CanaleSelezionato Is Nothing Then Exit Sub
    Dim i As Integer = CanaliSelezionati.IndexOf(CanaleSelezionato)
    Select Case MoveTo
      Case eMoveType.eTop
        CanaliSelezionati.Move(i, 0)
      Case eMoveType.eUp
        Dim MoveId As Integer = Math.Max(0, i - 1)
        CanaliSelezionati.Move(i, MoveId)
      Case eMoveType.eDn
        Dim MoveId As Integer = Math.Min(CanaliSelezionati.Count - 1, i + 1)
        CanaliSelezionati.Move(i, MoveId)
      Case eMoveType.eBottom
        CanaliSelezionati.Move(i, CanaliSelezionati.Count - 1)
    End Select

  End Sub


  Public Sub New(ListaCanaliDisponibili As List(Of clsChannel2020), ListaCanaliSelezionati As List(Of clsChannel2020))
    For Each canale In ListaCanaliDisponibili.OrderBy(Function(x) x.LongName).ToList
      Dim Ctmp = ListaCanaliSelezionati.Where(Function(x) x.ChannelId = canale.ChannelId).ToList
      If Ctmp.Count = 0 Then
        'non e'tra quelli selezionati lo aggiunge ai non selezionati
        CanaliNonSelezionati.Add(canale)
      End If
    Next
    For Each canale In ListaCanaliSelezionati.ToList ' va fatto dopo altrimenti non rispetto l ordine dei canali selezionati
      CanaliSelezionati.Add(canale)
    Next
    TestoRicerca = ""
  End Sub



  Public Sub MuoviCanale_ToSelezionati()
    'Public Sub AggiungiCanale_ToSelezionati(CanaleDaAggiungere As clsChannelAdv, CanaleSelezionato As clsChannelAdv)
    If CanaleNonSelezionato Is Nothing Then Exit Sub


    If CanaleSelezionato Is Nothing Then ' non c'é un canale marcato come selezionato tra quelli della lista canali selezionati (colonna di destra)
      CanaliSelezionati.Add(CanaleNonSelezionato)
    Else ' esiste un canale marcato come selezionato nella lista dei canali selezionati
      Dim idDaMuovere As Integer = CanaliSelezionati.IndexOf(CanaleSelezionato)
      CanaliSelezionati.Insert(idDaMuovere, CanaleNonSelezionato)
    End If
    CanaliNonSelezionati.Remove(CanaleNonSelezionato)
    ListaFiltrata.Remove(CanaleNonSelezionato)

  End Sub

  Public Sub MuoviCanale_ToNonSelezionati()
    If CanaleSelezionato Is Nothing Then Exit Sub
    CanaliNonSelezionati.Add(CanaleSelezionato)
    CanaliSelezionati.Remove(CanaleSelezionato)
    CanaleSelezionato = Nothing
    AggiornaListaFiltrata()
    'Dim ltmp = CanaliNonSelezionati.OrderBy(Function(x) x.Channel.Name).ToList
    'CanaliNonSelezionati.Clear()
    'For Each l In ltmp
    '  CanaliNonSelezionati.Add(l)
    'Next
    'CanaliNonSelezionati = CanaliNonSelezionati.OrderBy(Function(x) x.Channel.Name)
  End Sub

  'Public Sub RiordinaCanale(CanaleDaMuovere As clsChannelAdv, CanaleDiRiferimento As clsChannelAdv)
  '  If CanaleDaMuovere Is Nothing Then
  '    CambiaSelect(CanaleDiRiferimento)
  '    Exit Sub
  '  End If
  '  If CanaleDiRiferimento Is Nothing Then
  '    CambiaSelect(CanaleDaMuovere)
  '    Exit Sub
  '  End If
  '  If CanaleDaMuovere.IsSelected = CanaleDiRiferimento.IsSelected Then
  '    'vale solo se sono entrambi selected e va quindi riordinato
  '    If CanaleDiRiferimento.IsSelected Then
  '      Dim idDaMuovere As Integer
  '      For i As Integer = 0 To CanaliSelezionati.Count - 1
  '        If CanaleDaMuovere Is CanaliSelezionati(i) Then
  '          idDaMuovere = i
  '          Exit For
  '        End If
  '      Next
  '      For i As Integer = 0 To CanaliSelezionati.Count - 1
  '        If CanaleDiRiferimento Is CanaliSelezionati(i) Then
  '          CanaliSelezionati.Move(idDaMuovere, i)
  '          Exit For
  '        End If
  '      Next
  '    End If
  '  Else
  '    'e' da cambiare il selected
  '    Dim idCanaleRiferimento As Integer
  '    For i As Integer = 0 To CanaliSelezionati.Count - 1
  '      If CanaleDiRiferimento Is CanaliSelezionati(i) Then
  '        idCanaleRiferimento = i
  '        Exit For
  '      End If
  '    Next
  '    If CanaleDiRiferimento.IsSelected Then
  '      CanaleDaMuovere.IsSelected = True
  '      CanaliSelezionati.Insert(idCanaleRiferimento, CanaleDaMuovere)
  '      CanaliNonSelezionati.Remove(CanaleDaMuovere)
  '    Else
  '      CanaleDaMuovere.IsSelected = False
  '      CanaliNonSelezionati.Insert(idCanaleRiferimento, CanaleDaMuovere)
  '      CanaliSelezionati.Remove(CanaleDaMuovere)
  '    End If
  '  End If

  'End Sub

  'Private Sub CambiaSelect(Canale As clsChannelAdv)
  '  If Canale Is Nothing Then Exit Sub
  '  If Canale.IsSelected Then
  '    Canale.IsSelected = False
  '    CanaliNonSelezionati.Add(Canale)
  '    CanaliSelezionati.Remove(Canale)
  '  Else
  '    Canale.IsSelected = True
  '    CanaliSelezionati.Add(Canale)
  '    CanaliNonSelezionati.Remove(Canale)
  '  End If
  'End Sub


End Class


Public Class clsCanaleSql
  'Implements INotifyPropertyChanged

  Public Property HeaderSql As String
  Public Property IsSelected As Boolean = False

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Sub New(HeaderSql As String, IsSelected As Boolean)
    Me.HeaderSql = HeaderSql
    Me.IsSelected = IsSelected
  End Sub

  'Public Property HeaderSql As String
  '  Get
  '    Return pHeaderSql
  '  End Get
  '  Set(value As String)
  '    pHeaderSql = value
  '  End Set
  'End Property

  'Public Property IsSelected As Boolean
  '  Get
  '    Return pIsSelected
  '  End Get
  '  Set(value As Boolean)
  '    pIsSelected = value
  '    OnPropertyChanged("IsSelected")
  '  End Set
  'End Property



End Class