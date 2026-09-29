Imports System.ComponentModel
Imports PropertyChanged
'Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class UserControlPavarot
  'Implements INotifyPropertyChanged
  'Public WithEvents PavarotVM2020 As New UserControlPavarotViewModel

  'Public WithEvents PavarotVM2020 As UserControlPavarotViewModel


  'Public Sub New()

  '  'Me.DataContext = PavarotVM2020

  '  ' This call is required by the designer.
  '  InitializeComponent()

  '  ' --- Nulla di ciò che segue deve girare nel designer XAML ---
  '  If IsInDesignMode Then Exit Sub

  '  Me.DataContext = PavarotVM2020

  '  ' Add any initialization after the InitializeComponent() call.

  '  OutPutType.ItemsSource = System.Enum.GetValues(GetType(UserControlPavarotViewModel.eOutputType)).Cast(Of UserControlPavarotViewModel.eOutputType)


  'End Sub



  Public WithEvents PavarotVM2020 As UserControlPavarotViewModel

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' --- Nulla di ciò che segue deve girare nel designer XAML ---
    If IsInDesignMode Then Exit Sub

    ' >>> QUESTA RIGA MANCAVA <
    PavarotVM2020 = New UserControlPavarotViewModel()

    Me.DataContext = PavarotVM2020

    ' Add any initialization after the InitializeComponent() call.
    OutPutType.ItemsSource = System.Enum.GetValues(GetType(UserControlPavarotViewModel.eOutputType)).Cast(Of UserControlPavarotViewModel.eOutputType)

  End Sub


  Private Sub ItemCheckedStatusChanged(sender As Object, e As PropertyChangedEventArgs)
    'Stop
    PavarotVM2020.PavarotTabGotFocus()
  End Sub

  Private Sub TextBlock_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
    If e.RightButton = MouseButtonState.Pressed Then Exit Sub
    Try
      Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
      If p Is Nothing Then Exit Sub
      Dim IR As New SciChart.Data.Model.DateRange(p.TR.Start, p.TR.Finish)
      'ObjChartSyncManagerBasic.SetCurrentVisibleRange()
      DataPlotSync.SharedXVisibleRange = IR
      GraficoEventiViewModel.AggiornaSelezione(p.TR.Clone)
      MapControl.AggiornaSelezione(p.TR)
      PavarotVM2020.LineaSelezionata(p)
    Catch ex As Exception

    End Try
  End Sub

  Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(DirectCast(sender, SciChart.Charting.ChartModifiers.SeriesSelectionModifier).ParentSurface.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    PavarotVM2020.LineaSelezionata(Me, Selezione)
  End Sub

  Private Sub btn_SelectChannels_Click(sender As Object, e As RoutedEventArgs) Handles btn_SelectChannels.Click
    Dim Res As List(Of String) = GestisciListaCanali("Select Channel", DataProvider2020.Channels.ListaCanali.ToList, AppConfig.ActiveProfile.PavarotChartSettings.Select(Function(x) x.YaxisChannel).ToList, eSelectChannelType.eTwin)
    If Res.Count > 0 Then
      AppConfig.ActiveProfile.ImpostaChartPavarot(Res)
      PavarotVM2020.ControlliPavarotBasicPlot = Nothing
      PavarotVM2020.AggiornaGraficiPavarot(True)
      RefreshExtents()
      AppConfig.Salva()
    End If

  End Sub

  Private Sub btn_Refresh_Click(sender As Object, e As RoutedEventArgs) Handles btn_Refresh.Click
    'LoadingProgressVisualizza()
    PavarotVM2020.Refresh()
    RefreshExtents()
    'LoadingProgressNascondi()
  End Sub

  Private Sub btn_Recalc_Click(sender As Object, e As RoutedEventArgs) Handles btn_Recalc.Click
    PavarotVM2020.Recalc()
    PavarotVM2020.Refresh()
    RefreshExtents()
  End Sub

  Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
    PavarotVM2020.LineaSelezionata(Me, Selezione)
  End Sub

  Private Sub Btn_ExportToBazzo_Click(sender As Object, e As RoutedEventArgs)
    Stop
    'Dim objBazzo As New clsPavarotsBazzosData
    'objBazzo.EsportaJson()
  End Sub

  Private Sub Splitter_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
    Dim Delta As Double = e.VerticalChange
    Dim element = DirectCast(sender, FrameworkElement)

    If TypeOf element.DataContext Is UserControlPavarotViewModel Then
      Select Case element.Name
        'Case "SplitterRot"
        '  DirectCast(ItmCtrl_Rotation.DataContext, UserControlPavarotViewModel).ControlliPavarotAdvRotation.Altezza += Delta
        'Case "SplitterDrop"
        '  DirectCast(ItmCtrl_Rotation.DataContext, UserControlPavarotViewModel).ControlliPavarotAdvBoardDrop.Altezza += Delta
        'Case "SplitterUp"
        '  DirectCast(ItmCtrl_Rotation.DataContext, UserControlPavarotViewModel).ControlliPavarotAdvBoardUp.Altezza += Delta
        'Case "SplitterEntryExit"
        '  DirectCast(ItmCtrl_Rotation.DataContext, UserControlPavarotViewModel).ControlliPavarotAdvEntryExit.Altezza += Delta
      End Select
    End If

  End Sub

  Private Sub Period_MouseMove(sender As Object, e As MouseEventArgs)
    Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
    PavarotVM2020.CurrentPeriod(p)
  End Sub

  Private Sub Period_MouseLeave(sender As Object, e As MouseEventArgs)
    PavarotVM2020.CurrentPeriod(Nothing)
  End Sub

  Private Sub FontSizeBigger(sender As Object, e As RoutedEventArgs)
    PavarotVM2020.FontSizeBigger()
  End Sub

  Private Sub FontSizeSmaller(sender As Object, e As RoutedEventArgs)
    PavarotVM2020.FontSizeSmaller()
  End Sub

  Private Sub btn_PedestalStats_Click(sender As Object, e As RoutedEventArgs)
    'PavarotVM2020.CreaStatistichePedestals()
  End Sub

  Private Sub btn_Pdf_Click(sender As Object, e As RoutedEventArgs)
    PavarotVM2020.CreaReportPdf(GainLossVsTws)

  End Sub

  Private Sub TextBlock_MouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
    Stop
  End Sub

  Private Sub TextBlock_PreviewMouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
    Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
    If Not p Is Nothing Then
      If MsgBox("Do You Really want to delete the '" & p.Descrizione & "'", MsgBoxStyle.YesNo, "Delete current period") = MsgBoxResult.Yes Then
        p.IsChecked = False
        PeriodsManager.Elimina(p, True)
        e.Handled = True
        PavarotVM2020.Refresh()
      End If
    End If
  End Sub

  Private Sub btn_UpdateKeys_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.AssignKeysToPeriodsInTimeRange(clsPeriod2021.ePeriodType.eTack)
    PeriodsManager.AssignKeysToPeriodsInTimeRange(clsPeriod2021.ePeriodType.eGybe)
  End Sub

  Private Sub ItemChbx_Click(sender As Object, e As RoutedEventArgs)
    'Stop
    PavarotVM2020.PavarotTabGotFocus()
  End Sub

  Private Sub RefreshExtents()
    For Each c In PavarotVM2020.ControlliPavarotBasicAvg.ListaControlli
      c.Plot.ZoomExtentsX()
      c.Plot.ZoomExtentsY()
    Next
  End Sub

  Private Sub PavarotVM2020_GraficiAggiornati() Handles PavarotVM2020.GraficiAggiornati
    RefreshExtents()
  End Sub
End Class

