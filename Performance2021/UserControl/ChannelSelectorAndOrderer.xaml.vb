Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class ChannelSelectorAndOrderer
  'Implements INotifyPropertyChanged
  Dim _AvailableChannels As clsAvailableChannels
  Dim pCurrentChannel As clsAvailableChannel

  Dim pStatus As eStatus = eStatus.eCancel
  Dim pPuntoNotSelected As Point
  Dim pPuntoSelected As Point

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Enum eStatus
    eSave = 0
    eCancel = 1
  End Enum

  Public Property AvailableChannels As clsAvailableChannels
    Get
      Return _AvailableChannels
    End Get
    Set(value As clsAvailableChannels)
      _AvailableChannels = value
    End Set
  End Property

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

  Public Sub New(ListaCanaliDisponibili As ObservableCollection(Of clsChannel2020), ListaCanaliSelezionati As List(Of String), Titolo As String)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    Me.Title = Titolo

    AvailableChannels = New clsAvailableChannels(ListaCanaliDisponibili, ListaCanaliSelezionati)

    Me.DataContext = AvailableChannels

    'ListaSelected.ItemsSource = _AvailableChannels.CanaliSelezionati
    'ListaNotSelected.ItemsSource = _AvailableChannels.CanaliNonSelezionati

  End Sub

  Private Sub Btn_Salva_Click(sender As Object, e As RoutedEventArgs) Handles btn_Salva.Click
    pStatus = eStatus.eSave
    Me.Close()
  End Sub

  Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles btn_Cancel.Click
    pStatus = eStatus.eCancel
    Me.Close()
  End Sub

  Private Sub Btn_Reset_Click(sender As Object, e As RoutedEventArgs) Handles btn_Reset.Click
    _AvailableChannels.RipristinaListe()
  End Sub

  Private Sub ListaNotSelected_PreviewMouseMove(sender As Object, e As MouseEventArgs)
    If e.LeftButton = MouseButtonState.Pressed Then
      If pCurrentChannel Is Nothing Then
        Dim LV As ListView = DirectCast(sender, ListView)
        pCurrentChannel = DirectCast(LV.SelectedItem, clsAvailableChannel)
      End If
    Else
      pCurrentChannel = Nothing
    End If
  End Sub

  Private Sub ListaSelected_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
    pPuntoSelected = e.GetPosition(Nothing)
    pPuntoNotSelected = Nothing
  End Sub

  Private Sub ListaSelected_PreviewMouseMove(sender As Object, e As MouseEventArgs)
    'If Not pPuntoNotSelected = Nothing Then Exit Sub
    If e.LeftButton = MouseButtonState.Pressed Then
      If pCurrentChannel Is Nothing Then
        Dim LV As ListView = DirectCast(sender, ListView)
        pCurrentChannel = DirectCast(LV.SelectedItem, clsAvailableChannel)
      End If
    Else
      pCurrentChannel = Nothing
    End If

  End Sub

  Private Sub ListaSelected_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
    Dim PuntoDrag As Point = New Point(0, 0)
    If Not pPuntoSelected = Nothing Then
      'viene da selected
      PuntoDrag = pPuntoSelected
    Else
      'viene da non selected
    End If
    Dim PuntoDrop As Point = e.GetPosition(Nothing)
    Dim Delta As Vector = PuntoDrag - PuntoDrop
    If System.Math.Abs(Delta.X) > SystemParameters.MinimumHorizontalDragDistance Or System.Math.Abs(Delta.Y) > SystemParameters.MinimumVerticalDragDistance Then
      Dim LV As ListView = DirectCast(sender, ListView)
      Dim ToBeReplacedChannel = DirectCast(LV.SelectedItem, clsAvailableChannel)
      AvailableChannels.MuoviCanale(pCurrentChannel, ToBeReplacedChannel)
      pCurrentChannel = Nothing
    End If

  End Sub

  Private Sub Btn_NotSelToSel_Click(sender As Object, e As RoutedEventArgs) Handles btn_NotSelToSel.Click
    AvailableChannels.MuoviCanale(ListaNotSelected.SelectedItem, ListaSelected.SelectedItem)
  End Sub

  Private Sub Btn_SelToNotSel_Click(sender As Object, e As RoutedEventArgs) Handles btn_SelToNotSel.Click
    AvailableChannels.MuoviCanale(ListaSelected.SelectedItem, ListaNotSelected.SelectedItem)
  End Sub

  Private Sub ListaNotSelected_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles ListaNotSelected.PreviewMouseLeftButtonDown

  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    AvailableChannels.FiltroCanale = ""
  End Sub

  'Private Sub FiltraListaNotSelected(sender As Object, e As TextChangedEventArgs)
  '  FiltraGrigliaCanali()
  'End Sub

  'Private Sub FiltraGrigliaCanali()
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  Dim Visibile As Visibility = DataProvider2020.Channels.ListaCanali.First.Visibility

  '  If txt_Search.Text.Trim = "" Then
  '    dg_LoadedFileChannels.ItemsSource = DataProvider2020.Channels.ListaCanali
  '  Else
  '    Dim ListaTmp As New ObservableCollection(Of clsChannel2020)
  '    For Each canale In DataProvider2020.Channels.ListaCanali
  '      Dim strTmp As String = canale.CanaleChiaveStringa & " " & canale.ChannelId & " " & If(canale.KnownHeaders Is Nothing, "", String.Join(" ", canale.KnownHeaders)) & " " & canale.LongName & " " & canale.PolarHeader
  '      If strTmp.IndexOf(txt_Search.Text.Trim, StringComparison.CurrentCultureIgnoreCase) > -1 Then
  '        ListaTmp.Add(canale)
  '      End If
  '    Next
  '    dg_LoadedFileChannels.ItemsSource = ListaTmp
  '  End If
  'End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsAvailableChannels
  'Implements INotifyPropertyChanged
  Public Property ListaCanaliDisponibili As ObservableCollection(Of clsChannel2020)
  Public Property ListaCanaliSelezionati As List(Of String)
  Public Property CanaliDisponibili As New ObservableCollection(Of clsAvailableChannel)
  Public Property CanaliSelezionati As New ObservableCollection(Of clsAvailableChannel)
  Public Property CanaliNonSelezionati As New ObservableCollection(Of clsAvailableChannel)
  Public Property FiltroCanale As String = ""

  Public Sub New(ListaCanaliDisponibili As ObservableCollection(Of clsChannel2020), ListaCanaliSelezionati As List(Of String))
    Me.ListaCanaliDisponibili = ListaCanaliDisponibili
    Me.ListaCanaliSelezionati = ListaCanaliSelezionati
    ImpostaListe()
  End Sub

  Public ReadOnly Property GetListaCanaliSelezionati As ObservableCollection(Of clsChannel2020)
    Get
      Dim lTmp As New ObservableCollection(Of clsChannel2020)
      For Each Canale In CanaliSelezionati
        lTmp.Add(Canale.Channel)
      Next
      Return lTmp
    End Get
  End Property

  'Public Property CanaliSelezionati As ObservableCollection(Of clsAvailableChannel)
  '  Get
  '    Return pCanaliSelezionati
  '  End Get
  '  Set(value As ObservableCollection(Of clsAvailableChannel))
  '    pCanaliSelezionati = value
  '    OnPropertyChanged("CanaliSelezionati")
  '  End Set
  'End Property

  'Public Property CanaliNonSelezionati As ObservableCollection(Of clsAvailableChannel)
  '  Get
  '    Return pCanaliNonSelezionati
  '  End Get
  '  Set(value As ObservableCollection(Of clsAvailableChannel))
  '    pCanaliNonSelezionati = value
  '    OnPropertyChanged("CanaliNonSelezionati")
  '    OnPropertyChanged("CanaliNonSelezionatiFiltrati")
  '  End Set
  'End Property
  'Public Property FiltroCanale As String
  '  Get
  '    Return _FiltroCanale
  '  End Get
  '  Set(value As String)
  '    _FiltroCanale = value
  '    OnPropertyChanged("FiltroCanale")
  '    OnPropertyChanged("CanaliNonSelezionatiFiltrati")
  '  End Set
  'End Property

  Public ReadOnly Property CanaliNonSelezionatiFiltrati As ObservableCollection(Of clsAvailableChannel)
    Get
      If _FiltroCanale.Trim = "" Then
        Return CanaliNonSelezionati
      Else
        Dim f As String() = _FiltroCanale.ToUpper.Split(" ")
        Dim oc As New ObservableCollection(Of clsAvailableChannel)
        For Each ac In CanaliNonSelezionati.ToList
          Dim trovato As Boolean = True
          For Each s In f
            If Not ac.ChannelName.ToUpper.Contains(s) Then
              trovato = False
              Exit For
            End If
          Next
          If trovato Then oc.Add(ac)
        Next
        Return oc
      End If
    End Get
  End Property


  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Sub RipristinaListe()
    ImpostaListe()
  End Sub

  Private Sub ImpostaListe()
    CanaliDisponibili.Clear()
    CanaliSelezionati.Clear()
    CanaliNonSelezionati.Clear()

    For Each canale In ListaCanaliDisponibili
      Dim objCh As New clsAvailableChannel(canale, False)
      'RemoveHandler objCh.PropertyChanged, AddressOf ChannelPropertyChanged
      'AddHandler objCh.PropertyChanged, AddressOf ChannelPropertyChanged

      CanaliDisponibili.Add(objCh)
    Next
    For Each intestazione In ListaCanaliSelezionati
      For Each canale In CanaliDisponibili
        If canale.Channel.ChannelId = intestazione Then
          canale.Selected = True
          CanaliSelezionati.Add(canale)
          Exit For
        End If
      Next
    Next
    For Each Canale In CanaliDisponibili
      If Not Canale.Selected Then
        CanaliNonSelezionati.Add(Canale)
      End If
    Next
    'OnPropertyChanged("CanaliSelezionati")
    'OnPropertyChanged("CanaliNonSelezionati")

    'AggiornaListe()


  End Sub

  'Private Sub ChannelPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
  '  Select Case e.PropertyName
  '    Case "Selected"
  '      'Stop
  '      'AggiornaListe()
  '    Case Else
  '  End Select
  'End Sub


  Public Sub MuoviCanale(CanaleDaMuovere As clsAvailableChannel, CanaleDiRiferimento As clsAvailableChannel)
    If CanaleDaMuovere Is Nothing Then
      CambiaSelect(CanaleDiRiferimento)
      Exit Sub
    End If
    If CanaleDiRiferimento Is Nothing Then
      CambiaSelect(CanaleDaMuovere)
      Exit Sub
    End If
    If CanaleDaMuovere.Selected = CanaleDiRiferimento.Selected Then
      'vale solo se sono entrambi selected e va quindi riordinato
      If CanaleDiRiferimento.Selected Then
        Dim idDaMuovere As Integer
        For i As Integer = 0 To CanaliSelezionati.Count - 1
          If CanaleDaMuovere Is CanaliSelezionati(i) Then
            idDaMuovere = i
            Exit For
          End If
        Next
        For i As Integer = 0 To CanaliSelezionati.Count - 1
          If CanaleDiRiferimento Is CanaliSelezionati(i) Then
            CanaliSelezionati.Move(idDaMuovere, i)
            Exit For
          End If
        Next
      End If
    Else
      'e' da cambiare il selected
      Dim idCanaleRiferimento As Integer
      For i As Integer = 0 To CanaliSelezionati.Count - 1
        If CanaleDiRiferimento Is CanaliSelezionati(i) Then
          idCanaleRiferimento = i
          Exit For
        End If
      Next
      If CanaleDiRiferimento.Selected Then
        CanaleDaMuovere.Selected = True
        CanaliSelezionati.Insert(idCanaleRiferimento, CanaleDaMuovere)
        CanaliNonSelezionati.Remove(CanaleDaMuovere)
      Else
        CanaleDaMuovere.Selected = False
        CanaliNonSelezionati.Insert(idCanaleRiferimento, CanaleDaMuovere)
        CanaliSelezionati.Remove(CanaleDaMuovere)
      End If
    End If

  End Sub

  Private Sub CambiaSelect(Canale As clsAvailableChannel)
    If Canale Is Nothing Then Exit Sub
    If Canale.Selected Then
      Canale.Selected = False
      CanaliNonSelezionati.Add(Canale)
      CanaliSelezionati.Remove(Canale)
    Else
      Canale.Selected = True
      CanaliSelezionati.Add(Canale)
      CanaliNonSelezionati.Remove(Canale)
    End If
  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsAvailableChannel
  'Implements INotifyPropertyChanged
  Public Property Channel As clsChannel2020
  Public Property Selected As Boolean

  Public ReadOnly Property ChannelName As String
    Get
      Return Channel.LongName
    End Get
  End Property

  'Public Property Channel As clsChannel2020
  '  Get
  '    Return pChannel
  '  End Get
  '  Set(value As clsChannel2020)
  '    pChannel = value
  '  End Set
  'End Property

  'Public Property Selected As Boolean
  '  Get
  '    Return pSelected
  '  End Get
  '  Set(value As Boolean)
  '    pSelected = value
  '  End Set
  'End Property

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Sub New(Channel As clsChannel2020, Selected As Boolean)
    Me.Channel = Channel
    Me.Selected = Selected
  End Sub

End Class
