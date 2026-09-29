Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged

Public Class UserControlSelFromList
  Public Property VM As clsUserControlSelFromListViewModel
  Public Property VMstring As clsUserControlSelFromStringListViewModel

  Public Sub New(Titolo As String, Lista As List(Of clsChannel2020), CanaliSelezionati As List(Of clsChannel2020), SelMode As System.Windows.Controls.SelectionMode)
    Title = Titolo
    VM = New clsUserControlSelFromListViewModel(Lista, CanaliSelezionati, SelMode)
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
  End Sub

  Public Sub New(Titolo As String, Lista As List(Of String), CanaliSelezionati As List(Of String), SelMode As System.Windows.Controls.SelectionMode)
    Title = Titolo
    VMstring = New clsUserControlSelFromStringListViewModel(Lista, CanaliSelezionati, SelMode)
    Me.DataContext = VMstring
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
    Me.Close()
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    If _VM Is Nothing Then Exit Sub
    _VM.TestoRicerca = ""
    'ListaCanali.ItemsSource = _VM.ListaCompleta

  End Sub



  Public Function CanaliSelezionatiString() As List(Of String)
    If ListaCanali.SelectedItem Is Nothing Then Return New List(Of String)
    Return VMstring.ListaCompleta.Where(Function(x) x.IsSelected = True).Select(Function(x) x.Header).ToList
  End Function

  Public Function CanaliSelezionati() As List(Of clsChannel2020)
    If ListaCanali.SelectedItem Is Nothing Then Return New List(Of clsChannel2020)
    Return VM.ListaCompleta.Where(Function(x) x.IsSelected).Select(Function(x) x.Channel).ToList
  End Function

  Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
    '_VM.TestoRicerca = txtSearch.Text
  End Sub

  Private Sub ListaCanali_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    If ListaCanali.SelectedItem Is Nothing Then Exit Sub
    Me.DialogResult = True
    Me.Close()
  End Sub
End Class



<AddINotifyPropertyChangedInterface>
Public Class clsChannelAdv
  Public Property IsSelected As Boolean
  Public Property Channel As clsChannel2020

  Public ReadOnly Property Descrizione As String
    Get
      Return Channel.LongName
    End Get
  End Property


  Public Sub New(IsSelected As Boolean, Channel As clsChannel2020)
    Me.IsSelected = IsSelected
    Me.Channel = Channel
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsHeaderAdv
  Public Property IsSelected As Boolean
  Public Property Header As String

  Public ReadOnly Property Descrizione As String
    Get
      Return Header
    End Get
  End Property


  Public Sub New(IsSelected As Boolean, Header As String)
    Me.IsSelected = IsSelected
    Me.Header = Header
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsUserControlSelFromListViewModel
  'Implements INotifyPropertyChanged

  Public Property ListaCompleta As New List(Of clsChannelAdv)
  Public Property ListaFiltrata As New ObservableCollection(Of clsChannelAdv)
  'Public Property CanaliSelezionati As New ObservableCollection(Of clsChannelAdv)
  Public Property SelMode As System.Windows.Controls.SelectionMode

  Dim _TestoRicerca As String
  Public Property TestoRicerca As String
    Get
      Return _TestoRicerca
    End Get
    Set(value As String)
      _TestoRicerca = value
      If TestoRicerca.Trim = "" Then
        ListaFiltrata.Clear()
        For Each e In ListaCompleta.OrderByDescending(Function(x) x.IsSelected).ToList
          ListaFiltrata.Add(e)
        Next
      Else
        ListaFiltrata.Clear()
        For Each e In ListaCompleta.OrderByDescending(Function(x) x.IsSelected).ToList
          If e.IsSelected Then
            ListaFiltrata.Add(e)
          ElseIf Not e.Channel.ActualLogHeader Is Nothing AndAlso e.Channel.ActualLogHeader.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          ElseIf e.Channel.CanaleChiaveStringa.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          ElseIf e.Channel.ChannelId.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          ElseIf e.Channel.ShortName.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          ElseIf e.Channel.LongName.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          Else
            If Not e.Channel.KnownHeaders Is Nothing Then
              For Each nh In e.Channel.KnownHeaders
                If nh.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
                  ListaFiltrata.Add(e)
                End If
              Next
            End If
          End If
        Next
      End If
    End Set
  End Property

  Public Sub New(ListaCompleta As List(Of clsChannel2020), CanaliSelezionati As List(Of clsChannel2020), SelMode As System.Windows.Controls.SelectionMode)
    Me.SelMode = SelMode
    Me.ListaCompleta.Clear()
    For Each e In ListaCompleta.OrderBy(Function(x) x.IsSelected).ToList
            Select Case e.DataType
                Case clsChannel2020.eDataType.eTimeOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eBoolean
                Case Else
                    Try
                        If CanaliSelezionati.Where(Function(x) x.ChannelId = e.ChannelId).Count > 0 Then
                            Me.ListaCompleta.Add(New clsChannelAdv(True, e))
                        Else
                            Me.ListaCompleta.Add(New clsChannelAdv(False, e))
                        End If
                    Catch ex As Exception

                    End Try
            End Select
        Next
    TestoRicerca = ""
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsUserControlSelFromStringListViewModel
  'Implements INotifyPropertyChanged

  Public Property ListaCompleta As New List(Of clsHeaderAdv)
  Public Property ListaFiltrata As New ObservableCollection(Of clsHeaderAdv)
  'Public Property CanaliSelezionati As New ObservableCollection(Of clsHeaderAdv)
  Public Property SelMode As System.Windows.Controls.SelectionMode

  Dim _TestoRicerca As String
  Public Property TestoRicerca As String
    Get
      Return _TestoRicerca
    End Get
    Set(value As String)
      _TestoRicerca = value
      If TestoRicerca.Trim = "" Then
        ListaFiltrata.Clear()
        For Each e In ListaCompleta.OrderByDescending(Function(x) x.IsSelected).ToList
          ListaFiltrata.Add(e)
        Next
      Else
        ListaFiltrata.Clear()
        For Each e In ListaCompleta.OrderByDescending(Function(x) x.IsSelected).ToList
          If e.IsSelected Then
            ListaFiltrata.Add(e)
          ElseIf e.Header.ToLower.IndexOf(_TestoRicerca.ToLower) > -1 Then
            ListaFiltrata.Add(e)
          End If
        Next
      End If
    End Set
  End Property

  Public Sub New(ListaCompleta As List(Of String), CanaliSelezionati As List(Of String), SelMode As System.Windows.Controls.SelectionMode)
    Me.SelMode = SelMode
    For Each e In ListaCompleta
      If CanaliSelezionati.Where(Function(x) x.TrimStart("e") = e).Count > 0 Then
        Me.ListaCompleta.Add(New clsHeaderAdv(True, e))
      Else
        Me.ListaCompleta.Add(New clsHeaderAdv(False, e))
      End If
    Next
    TestoRicerca = ""
  End Sub

End Class
