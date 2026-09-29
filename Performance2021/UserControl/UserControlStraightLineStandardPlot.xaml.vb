Imports SPwpf

Public Class UserControlStraightLineStandardPlot
  Dim pSyncObj As clsStraightLineChartSync

  Public Sub New(SyncObj As clsStraightLineChartSync)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    pSyncObj = SyncObj
  End Sub

  Private Sub Plots_MouseMove(sender As Object, e As MouseEventArgs) Handles Plots.MouseMove
    Dim Point As System.Windows.Point = e.GetPosition(e.Source)
    Dim scs As SciChart.Charting.Visuals.SciChartSurface = DirectCast(sender, SciChart.Charting.Visuals.SciChartSurface)
    Dim VR As SciChart.Data.Model.DoubleRange = scs.XAxes.First.VisibleRange.AsDoubleRange
    Dim Dx As Double = Point.X / scs.RenderSurface.ActualWidth
    Dim Valore As Double = VR.Min + VR.Diff * Dx
    pSyncObj.CurrentPosition = Valore

  End Sub

  Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(DirectCast(sender, SciChart.Charting.ChartModifiers.SeriesSelectionModifier).ParentSurface.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    ViewModel.ParentVM.AggiornaControlli(Me, Selezione)
  End Sub

  Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
    ViewModel.ParentVM.AggiornaControlli(Me, Selezione)
  End Sub

  Public Property SyncObj As clsStraightLineChartSync
    Get
      Return pSyncObj
    End Get
    Set(value As clsStraightLineChartSync)
      pSyncObj = value
    End Set
  End Property

  Public ReadOnly Property ViewModel As clsStraightLineStandardPlotViewModel
    Get
      Return Me.DataContext
    End Get
  End Property

End Class
