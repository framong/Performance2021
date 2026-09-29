Imports SPwpf

Public Class PeriodManagerView
  Dim pAction As eAction

  Dim pObjPMVM As clsPeriodManagerViewModel

  Public Enum eAction
    eSave = 0
    eDelete = 1
  End Enum

  Public Sub New(SelectedPeriod As clsPeriod2021, ItsNew As Boolean)
    'Dim PeriodoTmp As New clsPeriod2020(SelectedPeriod.TimeRange.Clone, SelectedPeriod.PeriodType, SelectedPeriod.Id)
    'PeriodoTmp.CopiaProperties(SelectedPeriod)

    ObjPMVM = New clsPeriodManagerViewModel(SelectedPeriod)
    Me.DataContext = ObjPMVM

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    cmbType.ItemsSource = System.Enum.GetValues(GetType(clsPeriod2021.ePeriodType)).Cast(Of clsPeriod2021.ePeriodType)

    ObjPMVM.KeyMomentIsChecked = Not ObjPMVM.SelectedPeriod.KeyMoment = Nothing

    Delete.IsEnabled = Not ItsNew

    MostraMedieCanali()

  End Sub

  ''' <summary>
  ''' Riempie le medie di BS, TWA, TWS e TWD dell'intervallo del periodo.
  ''' I valori sono calcolati qui e non via binding perche' in clsChannelStats2021 le medie
  ''' sono campi pubblici, e WPF non fa binding sui campi.
  ''' </summary>
  Private Sub MostraMedieCanali()
    lblAvgBs.Content = ""
    lblAvgTwa.Content = ""
    lblAvgTws.Content = ""
    lblAvgTwd.Content = ""

    If ObjPMVM Is Nothing Then Exit Sub
    If ObjPMVM.SelectedPeriod Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub

    Try
      Dim TR As clsTimeRange = ObjPMVM.SelectedPeriod.TR
      Dim i As Integer = TR.IdRigaIniziale
      Dim l As Integer = TR.IdRigaFinale - i
      If l < 1 Then Exit Sub

      lblAvgBs.Content = MediaCanale(clsChannels2020.eCanaliChiave.eSOW, i, l, "F1", False)
      lblAvgTwa.Content = MediaCanale(clsChannels2020.eCanaliChiave.eTWA, i, l, "F0", False)
      lblAvgTws.Content = MediaCanale(clsChannels2020.eCanaliChiave.eTWS, i, l, "F1", False)
      lblAvgTwd.Content = MediaCanale(clsChannels2020.eCanaliChiave.eTWD, i, l, "F0", True)

    Catch ex As Exception
      ' le medie sono informative: un errore qui non deve impedire la gestione del periodo
    End Try
  End Sub

  ''' <summary>
  ''' Media di un canale sull'intervallo indicato. Per i canali circolari (TWD) la media
  ''' si fa sui vettori, altrimenti a cavallo dei 360 gradi il risultato non ha senso.
  ''' </summary>
  Private Function MediaCanale(CanaleChiave As clsChannels2020.eCanaliChiave, IdIniziale As Integer, Campioni As Integer, Formato As String, Circolare As Boolean) As String
    Dim ch As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleChiave)
    If ch Is Nothing Then Return ""
    If ch.Valori Is Nothing Then Return ""

    Dim Valori As Double() = ch.Valori.Skip(IdIniziale).Take(Campioni).Where(Function(x) Not Double.IsNaN(x)).ToArray
    If Valori.Count = 0 Then Return ""

    If Not Circolare Then Return Valori.Average.ToString(Formato)

    Dim sx As Double = 0
    Dim sy As Double = 0
    For Each v As Double In Valori
      sx += Math.Cos(v * Math.PI / 180)
      sy += Math.Sin(v * Math.PI / 180)
    Next
    Dim a As Double = Math.Atan2(sy, sx) * 180 / Math.PI
    If a < 0 Then a += 360
    Return a.ToString(Formato)
  End Function

  Public Property ObjPMVM As clsPeriodManagerViewModel
    Get
      Return pObjPMVM
    End Get
    Set(value As clsPeriodManagerViewModel)
      pObjPMVM = value
    End Set
  End Property

  Public Property Action As eAction
    Get
      Return pAction
    End Get
    Set(value As eAction)
      pAction = value
    End Set
  End Property

  Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Cancel.Click
    Me.DialogResult = False
    Me.Close()
  End Sub

  Private Sub Save_Click(sender As Object, e As RoutedEventArgs) Handles Save.Click
    pAction = eAction.eSave
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub Delete_Click(sender As Object, e As RoutedEventArgs) Handles Delete.Click
    If MsgBox("Do you really want to DELETE current period?", MsgBoxStyle.YesNo, "Period Manager") = MsgBoxResult.Yes Then
      pAction = eAction.eDelete
      Me.DialogResult = True
      Me.Close()
    End If
  End Sub
End Class
