Imports System.ComponentModel

Public Class UserControlSelChannel

  Dim _LoadedChannels As eLoadedChannels
  Dim _OBJDP As clsDataProvider2020
  Dim _MultiSelect As Boolean
  Dim _CanaleChiave As clsChannels2020.eCanaliChiave
  Dim _CanaleAscissa As clsChannel2020


  Public Sub New(CanaleChiave As clsChannels2020.eCanaliChiave, OBJDP As clsDataProvider2020, MultiSelect As Boolean)

    ' This call is required by the designer.
    InitializeComponent()


    ' Add any initialization after the InitializeComponent() call.
    _OBJDP = OBJDP
    _LoadedChannels = eLoadedChannels.eKeyChannel
    _MultiSelect = MultiSelect
    If MultiSelect Then
      lv_Canali.SelectionMode = SelectionMode.Multiple
    Else
      lv_Canali.SelectionMode = SelectionMode.Single
    End If
    _CanaleChiave = CanaleChiave
    Dim VisualizzaMainChannel As Boolean = Not _CanaleAscissa Is Nothing
    If _CanaleAscissa Is _OBJDP.Channels.Canale(clsChannels2020.eCanaliChiave.eDateTime) Then
      VisualizzaMainChannel = False
    End If
    RiempiListViewCanali(VisualizzaMainChannel)
    'Me.DialogResult = False
  End Sub


  Public Sub New(LoadedChannels As eLoadedChannels, CanaleAscissa As clsChannel2020, OBJDP As clsDataProvider2020, MultiSelect As Boolean)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    _OBJDP = OBJDP
    _LoadedChannels = LoadedChannels
    _MultiSelect = MultiSelect
    _CanaleAscissa = CanaleAscissa
    If MultiSelect Then
      lv_Canali.SelectionMode = SelectionMode.Multiple
    Else
      lv_Canali.SelectionMode = SelectionMode.Single
    End If
    Dim VisualizzaMainChannel As Boolean = Not _CanaleAscissa Is Nothing
    If _CanaleAscissa Is _OBJDP.Channels.Canale(clsChannels2020.eCanaliChiave.eDateTime) Then
      VisualizzaMainChannel = False
    End If
    RiempiListViewCanali(VisualizzaMainChannel)
    ImpostaComboDettagliCanaleAscissa()
  End Sub

  Public Property LoadedChannels As eLoadedChannels
    Get
      Return _LoadedChannels
    End Get
    Set(value As eLoadedChannels)
      _LoadedChannels = value
    End Set
  End Property

  Public Property OBJDP As clsDataProvider2020
    Get
      Return _OBJDP
    End Get
    Set(value As clsDataProvider2020)
      _OBJDP = value
    End Set
  End Property

  Public Property MultiSelect As Boolean
    Get
      Return _MultiSelect
    End Get
    Set(value As Boolean)
      _MultiSelect = value
    End Set
  End Property

  Public ReadOnly Property CanaleAscissa As clsChannel2020
    Get
      Return cb_CanaleAscissa.SelectedItem
    End Get
  End Property

  Public ReadOnly Property Canali As List(Of clsChannel2020)
    Get
      Dim Ctmp As New List(Of clsChannel2020)
      For Each item As clsChannel2020 In lv_Canali.SelectedItems
        Ctmp.Add(item)
      Next
      Return Ctmp
    End Get
  End Property

  Public ReadOnly Property Canale As clsChannel2020
    Get
      Return Canali.First
    End Get
  End Property

  Public ReadOnly Property CanaleChiave As clsChannels2020.eCanaliChiave
    Get
      If lv_Canali.SelectedItem Is Nothing Then Return -1
      'If lv_Canali.SelectedItem Is lv_Canali.Items(0) Then Return -1
      If DirectCast(lv_Canali.SelectedItem, clsCanaleChiave).LongName = "None" Then Return -1
      Return DirectCast(lv_Canali.SelectedItem, clsCanaleChiave).Canale
    End Get
  End Property

  Public Enum eLoadedChannels
    eKeyChannel = 0
    eAvailable = 1
    eLoaded = 2
    eLoadedKeyChannel = 3
  End Enum


  Private Sub RiempiListViewCanali(VisualizzaMainChannel As Boolean)
    If VisualizzaMainChannel Then
      GrigliaBase.RowDefinitions(1).Height = New GridLength(50)
      lbl_MainVar.Visibility = Visibility.Visible
      cb_CanaleAscissa.Visibility = Visibility.Visible
      cb_CanaleAscissa.DisplayMemberPath = "LongName"
      cb_CanaleAscissa.Items.Clear()
      'cb_CanaleAscissa.Items.Add(pOBJDP.Channels.Canale(clsChannels2020.eCanaliChiave.eTimeOnly))
      'cb_CanaleAscissa.Items.Add("TimeStamp")
    Else
      GrigliaBase.RowDefinitions(1).Height = New GridLength(0)
      lbl_MainVar.Visibility = Visibility.Hidden
      cb_CanaleAscissa.Visibility = Visibility.Hidden
    End If
    lv_Canali.Items.Clear()
    lv_Canali.DisplayMemberPath = "LongName"
    Dim Canali As List(Of clsChannel2020) = _OBJDP.Channels.ListaCanali.ToList
    Select Case _LoadedChannels
      Case eLoadedChannels.eAvailable
        Canali = _OBJDP.Channels.ListaCanali.ToList
      Case eLoadedChannels.eLoadedKeyChannel
        Stop
        'Canali = pOBJDP.Channels.CanaliChiaveCaricati
      Case eLoadedChannels.eKeyChannel
        Dim Indici As Integer() = System.Enum.GetValues(GetType(clsChannels2020.eCanaliChiave))
        Dim Names As String() = System.Enum.GetNames(GetType(clsChannels2020.eCanaliChiave))
        Dim CanaliChiave As New List(Of clsCanaleChiave)
        'Dim CC As New clsCanaleChiave(Nothing, False, False)
        'CanaliChiave.Add(CC)
        For i As Integer = 0 To Names.Count - 1
          If Not CanaleIsMath(Indici(i)) Then
            Dim CC As New clsCanaleChiave(Indici(i), Indici(i) = _CanaleChiave, CanaleChiaveAssegnato(Indici(i)))
            CanaliChiave.Add(CC)
          End If
          'lv_Canali.Items.Add(CC)
        Next
        Dim ListaOrdinata = CanaliChiave.OrderBy(Function(x) x.LongName).OrderByDescending(Function(x) x.IsSelected)
        lv_Canali.ItemsSource = ListaOrdinata
        Exit Sub
        'Canali = pOBJDP.Channels.CanaliChiave
      Case eLoadedChannels.eLoaded
        Canali = _OBJDP.Channels.ListaCanali.ToList
      Case Else
        Canali = _OBJDP.Channels.ListaCanali.ToList
    End Select

    ' qui bisogna ordinare i canali, prima i selected poi per in ordine alfabetico

    Dim C = Canali.OrderBy(Function(x) x.LongName)
    Dim C1 = C.OrderByDescending(Function(x) x.IsSelected)

    'Dim Lista = Canali.OrderBy(Function(x) x.LongName).OrderByDescending(Function(x) x.IsSelected)


    If Not Canali Is Nothing Then

      For Each Canale As clsChannel2020 In C1
        'If Canale.IsSelected Then Stop
        Select Case Canale.DataType
          Case clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
          Case Else
            lv_Canali.Items.Add(Canale)
            If VisualizzaMainChannel Then
              cb_CanaleAscissa.Items.Add(Canale)
            End If
        End Select
      Next
    End If
  End Sub

  Private Function CanaleChiaveAssegnato(CanaleCorrente As clsChannels2020.eCanaliChiave) As Boolean
    Return DataProvider2020.Channels.ListaCanali.ToList.Where(Function(x) x.CanaleChiave = CanaleCorrente).Count > 0
  End Function

  Private Function CanaleIsMath(CanaleCorrente As clsChannels2020.eCanaliChiave) As Boolean
    Return DataProvider2020.Channels.ListaCanali.Where(Function(x) x.CanaleChiave = CanaleCorrente And x.IsMath).Count > 0
  End Function

  Private Sub ImpostaComboDettagliCanaleAscissa()
    If _CanaleAscissa Is Nothing Then Exit Sub
    For i As Integer = 0 To cb_CanaleAscissa.Items.Count - 1
      If cb_CanaleAscissa.Items(i) Is _CanaleAscissa Then
        cb_CanaleAscissa.SelectedIndex = i
        Exit For
      End If
    Next
  End Sub

  Private Sub btn_Close_Click(sender As Object, e As RoutedEventArgs) Handles btn_Close.Click
    'pSave = False
    Me.DialogResult = False
    Me.Close()
  End Sub

  Private Sub btn_Select_Click(sender As Object, e As RoutedEventArgs) Handles btn_Select.Click
    'pSave = True
    Me.DialogResult = True
    Me.Close()
  End Sub
End Class



Friend Class clsCanaleChiave
  Dim pLongName As String
  Dim pCanale As clsChannels2020.eCanaliChiave
  Dim pIsSelected As Boolean
  Dim pAssegnato As Boolean

  Public ReadOnly Property LongName As String
    Get
      Return pLongName
    End Get
  End Property

  Public ReadOnly Property Canale As clsChannels2020.eCanaliChiave
    Get
      Return pCanale
    End Get
  End Property

  Public Property IsSelected As Boolean
    Get
      Return pIsSelected
    End Get
    Set(value As Boolean)
      pIsSelected = value
    End Set
  End Property

  Public ReadOnly Property Assegnato As Boolean
    Get
      Return pAssegnato
    End Get
  End Property

  Public Property ForeGround As Brush
    Get
      Return If(pAssegnato, Brushes.Black, Brushes.DarkRed)
    End Get
    Set(value As Brush)

    End Set
  End Property

  Public Sub New(Canale As clsChannels2020.eCanaliChiave, IsSelected As Boolean, Assegnato As Boolean)
    pCanale = Canale
    pIsSelected = IsSelected
    If Canale = Nothing Then
      pLongName = "None"
    Else
      pLongName = pCanale.ToString.TrimStart("e")
    End If

    pAssegnato = Assegnato
  End Sub
End Class