Imports SPwpf

Public Class UserControlPavarotAdvancedPlot
  Dim pParentSyncViewModel As clsPavarotSyncViewModel
  Dim _LastXRange As SciChart.Data.Model.DoubleRange

  Public Sub New(ParentSyncViewModel As clsPavarotSyncViewModel)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    pParentSyncViewModel = ParentSyncViewModel
    'Plot.ActualHeight = 300
  End Sub

  Private Sub Plot_MouseMove(sender As Object, e As MouseEventArgs) Handles Plot.MouseMove
		Dim Point As System.Windows.Point = e.GetPosition(e.Source)
		Dim scs As SciChart.Charting.Visuals.SciChartSurface = DirectCast(sender, SciChart.Charting.Visuals.SciChartSurface)
    If Not scs.XAxes.First.VisibleRange Is Nothing Then
      Dim VR As SciChart.Data.Model.DoubleRange = scs.XAxes.First.VisibleRange.AsDoubleRange
      Dim Dx As Double = Point.X / scs.RenderSurface.ActualWidth
      Dim Valore As Double = VR.Min + VR.Diff * Dx
      If Not pParentSyncViewModel Is Nothing Then
        pParentSyncViewModel.CurrentPosition = Valore
      End If
    End If

  End Sub

	Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
		Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(DirectCast(sender, SciChart.Charting.ChartModifiers.SeriesSelectionModifier).ParentSurface.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
		ViewModel.ParentVM.LineaSelezionata(Me, Selezione)
	End Sub

	Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
		Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
		ViewModel.ParentVM.LineaSelezionata(Me, Selezione)
	End Sub

	Public Property ParentSyncViewModel As clsPavarotSyncViewModel
    Get
      Return pParentSyncViewModel
    End Get
    Set(value As clsPavarotSyncViewModel)
      pParentSyncViewModel = value
    End Set
  End Property

  Public ReadOnly Property ViewModel As clsPavarotAdvancedPlotViewModel
		Get
			Return Me.DataContext
		End Get
	End Property

  Private Sub NumericAxis_VisibleRangeChanged(sender As Object, e As SciChart.Charting.Visuals.Events.VisibleRangeChangedEventArgs)
    _LastXRange = e.NewVisibleRange.AsDoubleRange
    If pParentSyncViewModel.SquaredAxis Then SquadraYconX()
    'pParentSyncViewModel.VisibleXRangeChanged(e.NewVisibleRange) ' DirectCast(sender, SciChart.Charting.Visuals))
  End Sub

  Private Sub NumericAxis_VisibleRangeChanged_1(sender As Object, e As SciChart.Charting.Visuals.Events.VisibleRangeChangedEventArgs)
    'pParentSyncViewModel.VisibleYRangeChanged(e.NewVisibleRange)
  End Sub

  Private Sub SquadraYconX()
    If Plot.XAxes.First.VisibleRange Is Nothing Then Exit Sub
    If Plot.YAxes.First.VisibleRange Is Nothing Then Exit Sub
    Dim Xrange As Double = Plot.XAxes.First.VisibleRange.AsDoubleRange.Diff
    Dim Ymed As Double = (Plot.YAxes.First.VisibleRange.AsDoubleRange.Max + Plot.YAxes.First.VisibleRange.AsDoubleRange.Min) / 2
    Plot.YAxes.First.VisibleRange = New SciChart.Data.Model.DoubleRange(Ymed - Xrange / 2, Ymed + Xrange / 2)

  End Sub

  Private Sub Plot_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Plot.ZoomExtents()
    'Plot.ZoomExtentsX()
    If pParentSyncViewModel.SquaredAxis Then SquadraYconX()
  End Sub
End Class
