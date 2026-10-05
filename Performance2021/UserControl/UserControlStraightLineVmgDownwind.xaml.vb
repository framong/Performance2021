Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class UserControlStraightLineVmgDownwind
  WithEvents _StraightLineVM2020 As New clsStraightLineVM2020()

  Public Property StraightLineVM2020 As clsStraightLineVM2020
    Get
      Return _StraightLineVM2020
    End Get
    Set(value As clsStraightLineVM2020)
      _StraightLineVM2020 = value
    End Set
  End Property

  Public Sub New()
    Me.DataContext = StraightLineVM2020
    ' This call is required by the designer.
    StraightLineVM2020.Andatura = StraightLineVM2020.eAndatura.DownwindVmg  ' prima di InitializeComponent: i binding leggono le opzioni del tab giusto
    InitializeComponent()

    StraightLineVM2020.Andatura = StraightLineVM2020.eAndatura.DownwindVmg ' ShowVmg
    StraightLineVM2020.StraightLinesSelectionSettings.UpwindSelected = False
    StraightLineVM2020.StraightLinesSelectionSettings.DownwindSelected = True
    StraightLineVM2020.StraightLinesSelectionSettings.ApplyTwaFilter = False


    'StraightLineVM2020.Andatura = StraightLineVM2020.eAndatura.UpwindVmg ' ShowVmg
    'StraightLineVM2020.StraightLinesSelectionSettings.UpwindSelected = True
    'StraightLineVM2020.StraightLinesSelectionSettings.DownwindSelected = False
    'StraightLineVM2020.StraightLinesSelectionSettings.ApplyTwaFilter = False
    Dim Valori = System.Enum.GetValues(GetType(clsStraightLineVM2020.eOutputType)).Cast(Of clsStraightLineVM2020.eOutputType)
    Dim v As New List(Of clsStraightLineVM2020.eOutputType)
    For i As Integer = 0 To Valori.Count - 4
      v.Add(Valori(i))
    Next
    OutPutType.ItemsSource = v
    'OutPutType.ItemsSource = System.Enum.GetValues(GetType(clsStraightLineVM2020.eOutputType)).Cast(Of clsStraightLineVM2020.eOutputType)


  End Sub

  Private Sub AggiornaPeriodiVisibili()
    Dim p As clsPeriod2021 = Nothing
    StraightLineVM2020.AggiornaControlli(p)
  End Sub

  Private Sub TextBlock_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
    Try
      Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
      If p Is Nothing Then Exit Sub

      Dim IR As New SciChart.Data.Model.DateRange(p.TR.Start, p.TR.Finish)
      DataPlotSync.SharedXVisibleRange = IR
      GraficoEventiViewModel.AggiornaSelezione(p.TR.Clone)
      MapControl.AggiornaSelezione(p.TR)


      StraightLineVM2020.AggiornaControlli(p)
    Catch ex As Exception

    End Try

  End Sub

  Private Sub btn_SelectChannels_Click(sender As Object, e As RoutedEventArgs) Handles btn_SelectChannels.Click
    Dim ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn.Select(Function(x) x.ChannelName).ToList
    Dim res As List(Of String) = GestisciListaCanali("Straight Line Channels", DataProvider2020.Channels.ListaCanali.ToList, ListaTmp, eSelectChannelType.eTwin)
    If res.Count > 0 Then
      AppConfig.ActiveProfile.ImpostaStraightLineChartSettings(AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn, res)
      StraightLineVM2020.AggiornaCanali()
      StraightLineVM2020.AggiornaGrafici()
      StraightLineVM2020.AggiornaPeriodiVisibili()
      RefreshExtents()
      AppConfig.Salva()
    End If
  End Sub

  Private Async Sub btn_PrintHtml_Click(sender As Object, e As RoutedEventArgs)
    Await StraightLineVM2020.CreaReportHtml(AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn, "")
  End Sub

  Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Stop
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(DirectCast(sender, SciChart.Charting.ChartModifiers.SeriesSelectionModifier).ParentSurface.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    StraightLineVM2020.AggiornaControlli(Selezione)
  End Sub

  Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
    StraightLineVM2020.AggiornaControlli(Selezione)
  End Sub

  Private Sub Btn_MouseMode_Click(sender As Object, e As RoutedEventArgs)
    btn_MouseMode.Content = StraightLineVM2020.ChangeMouseMode   ' SyncViewModel.ChangeMouseMode
  End Sub

  Private Sub PeriodsToCharts_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
    Dim Delta As Double = e.HorizontalChange
    IcLineUpSelPeriods.Width += Delta
  End Sub

  Private Sub PeriodsToXYsplitter_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
    Dim Delta As Double = e.VerticalChange
    XYvmg.Height += Delta

  End Sub

  Private Sub Btn_Refresh_Click(sender As Object, e As RoutedEventArgs)
    StraightLineVM2020.VerificaSelectedStraightLines()
    StraightLineVM2020.AggiornaGrafici()
    RefreshExtents()
  End Sub


  Private Sub SplitGrafici_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
    Dim Delta As Double = e.VerticalChange
    ItmCtrl_PeriodsCharts.Height += Delta
  End Sub

  Private Sub Period_MouseMove(sender As Object, e As MouseEventArgs)
    Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
    StraightLineVM2020.CurrentPeriod(p)
  End Sub

  Private Sub Period_MouseLeave(sender As Object, e As MouseEventArgs)
    StraightLineVM2020.CurrentPeriod(Nothing)
  End Sub

  Private Sub FontSizeBigger(sender As Object, e As RoutedEventArgs)
    StraightLineVM2020.FontSizeBigger()
  End Sub

  Private Sub FontSizeSmaller(sender As Object, e As RoutedEventArgs)
    StraightLineVM2020.FontSizeSmaller()
  End Sub

  Private Sub RefreshExtents()
    For Each c In StraightLineVM2020.ContenitoreTwsVsChannels.ListaControlli
      c.Plots.ZoomExtentsX()
      c.Plots.ZoomExtentsY()
    Next
    For Each c In StraightLineVM2020.ContenitoreDistributions.ListaControlli
      c.Plots.ZoomExtentsX()
      c.Plots.ZoomExtentsY()
    Next

  End Sub

  Private Async Sub btn_PrintPdf_Click(sender As Object, e As RoutedEventArgs)
        Await StraightLineVM2020.CreaReportPdf(AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn, "")
    End Sub


  Private Sub TextBlock_PreviewMouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
    Dim p As clsPeriod2021 = DirectCast(sender.datacontext, clsPeriod2021)
    If Not p Is Nothing Then
      If MsgBox("Do You Really want to delete the '" & p.Descrizione & "'", MsgBoxStyle.YesNo, "Delete current period") = MsgBoxResult.Yes Then
        p.IsChecked = False
        PeriodsManager.Elimina(p, True)
      End If
    End If
  End Sub

  Private Sub btn_UpdateKeys_Click(sender As Object, e As RoutedEventArgs)
    PeriodsManager.AssignKeysToPeriodsInTimeRange(clsPeriod2021.ePeriodType.eStraightLineVmg)
  End Sub


  Private Sub _StraightLineVM2020_GraficiAggiornati() Handles _StraightLineVM2020.GraficiAggiornati
    RefreshExtents()
  End Sub

  Private Sub ItemChkBx_Click(sender As Object, e As RoutedEventArgs)
    StraightLineVM2020.VerificaAggiornaGrafici()
  End Sub

  Private Sub btn_SetKeySelected_Click(sender As Object, e As RoutedEventArgs)
    If StraightLineVM2020.ImpostaKeysPeriodiSelezionati() Then Btn_Refresh_Click(sender, e)
  End Sub

  Private Sub btn_DeleteSelected_Click(sender As Object, e As RoutedEventArgs)
    If StraightLineVM2020.EliminaPeriodiSelezionati() Then Btn_Refresh_Click(sender, e)
  End Sub

  Private Sub ReportOption_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    ' ignora la selezione iniziale del binding: salva solo se l'utente ha cambiato valore
    If e.RemovedItems.Count = 0 Then Exit Sub
    AppConfig.Salva()
  End Sub

  Private Sub ReportOption_Click(sender As Object, e As RoutedEventArgs)
    AppConfig.Salva()
  End Sub

End Class

