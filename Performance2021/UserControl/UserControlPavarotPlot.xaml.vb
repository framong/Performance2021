Public Class UserControlPavarotPlot
	Dim _ParentVM As UserControlPavarotViewModel

	Public Sub New()
		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.

	End Sub

	Public Sub New(ControlHeight As Double, ParentVM As UserControlPavarotViewModel)
		_ParentVM = ParentVM
		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.
		GrigliaGrafico.Height = ControlHeight

	End Sub

	Public Enum eTipoOperazione
		eSelectChannel = 0
		eDelete = 1
		eMoveUp = 2
		eMoveDn = 3
		eMoveTop = 4
		eMoveBottom = 5
	End Enum


	'Public Property ControlHeight As Double
	'	Get
	'		Return GrigliaGrafico.Height
	'	End Get
	'	Set(value As Double)
	'		GrigliaGrafico.Height = value
	'	End Set
	'End Property

	Private Sub SyncChartsMouseMove(sender As Object, e As MouseEventArgs)
		If Not _ParentVM Is Nothing Then
			If _ParentVM.ControlliPavarotBasicPlot Is Nothing Then Exit Sub
			Dim Point As System.Windows.Point = e.GetPosition(e.Source)
			Dim scs As SciChart.Charting.Visuals.SciChartSurface = DirectCast(sender, SciChart.Charting.Visuals.SciChartSurface)
			Dim Range As Double = scs.XAxes.First.VisibleRange.Diff
			Dim Dx As Double = Point.X / scs.RenderSurface.ActualWidth
			Dim Delta As Double = Range * Dx
			Dim Posizione As Double = DirectCast(scs.XAxes.First.VisibleRange.Min, Double) + Delta
			_ParentVM.PavarotSyncViewModel.CurrentPosition = Posizione




			'New System.Collections.Generic.Mscorlib_CollectionDebugView(Of SciChart.Charting.Model.ChartData.SeriesInfo)(DirectCast((New System.Collections.Generic.Mscorlib_CollectionDebugView(Of SciChart.Charting.ChartModifiers.IChartModifier)(DirectCast(scs.ChartModifier, SciChart.Charting.ChartModifiers.ModifierGroup).ChildModifiers).Items(4)), SciChart.Charting.ChartModifiers.InspectSeriesModifierBase).SeriesData.SeriesInfo).Items(2)).DataSeriesIndex
			Dim SS = DirectCast(scs.DataContext, SPwpf.UserControlPavarotPlotViewModel).SeriesSource
			For Each s In SS
				Dim ds = s.DataSeries
				Dim rs As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(s.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
				Dim Canale = DirectCast(rs.DataContext, UserControlPavarotPlotViewModel).Canale
				Dim Indice As Integer = TrovaIndice(Posizione, DirectCast(ds, SciChart.Charting.Model.DataSeries.DataSeries(Of Double, Double)).XValues.ToList, ds.Count)
				Dim Valore As Double = ds.YValues(Indice)
				If IsNumeric(rs.Tag) Then
					For Each pav In DirectCast(scs.DataContext, SPwpf.UserControlPavarotPlotViewModel).ParentVM.Lista
						If pav.IsStbd AndAlso CInt(rs.Tag) = 1 Then
							If Canale Is Nothing Then
								pav.StrTempVal = Format(Valore, "F0")
							Else
								pav.StrTempVal = Format(Valore, "F" & Canale.Decimals.ToString)
							End If
						ElseIf CInt(rs.Tag) = -1 Then
							If Canale Is Nothing Then
								pav.StrTempVal = Format(Valore, "F0")
							Else
								pav.StrTempVal = Format(Valore, "F" & Canale.Decimals.ToString)
							End If
						End If
					Next
				Else
					If TypeOf (rs.Tag) Is clsGruppoPeriodi Then

					ElseIf TypeOf (rs.Tag) Is clsPeriod2021 Then
						Dim p = DirectCast(rs.Tag, clsPeriod2021)
						If Not p Is Nothing Then
							If Canale Is Nothing Then
								p.StrTempVal = Format(Valore, "F0")
							Else
								p.StrTempVal = Format(Valore, "F" & Canale.Decimals.ToString)
							End If
						End If
					Else
					End If
				End If
			Next
		End If
	End Sub

	Public Function TrovaIndice(Secondi As Double, Valori As List(Of Double), ValoriCount As Integer) As Integer
		Dim L As Integer = 0
		Dim U As Integer = Valori.Count
		Dim DTtmp As Double
		Dim IndexTmp As Integer
		Do While U - L > 1
			IndexTmp = CInt((U + L) / 2)
			DTtmp = Valori(IndexTmp)
			If Secondi = DTtmp Then
				Exit Do
			ElseIf Secondi > DTtmp Then
				L = IndexTmp
			Else
				U = IndexTmp
			End If
		Loop
		Return IndexTmp
	End Function

	Private Sub SplitGrafici_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs) Handles SplitGrafici.DragCompleted
		Dim Delta As Double = e.VerticalChange
		Dim H As Double = Plot.Height
		If Double.IsNaN(GrigliaGrafico.Height) Then
			GrigliaGrafico.Height = GrigliaGrafico.ActualHeight
		End If
		If GrigliaGrafico.Height + Delta < 5 Then
			GrigliaGrafico.Height = 5
		Else
			GrigliaGrafico.Height += Delta
		End If
	End Sub

	Private Sub Btn_Sel_Click(sender As Object, e As RoutedEventArgs) Handles btn_Sel.Click
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eSelectChannel)
	End Sub

	Private Sub Btn_Up_Click(sender As Object, e As RoutedEventArgs) Handles btn_Up.Click
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eMoveUp)
	End Sub

	Private Sub Btn_Dn_Click(sender As Object, e As RoutedEventArgs) Handles btn_Dn.Click
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eMoveDn)
	End Sub

	Private Sub Btn_Del_Click(sender As Object, e As RoutedEventArgs) Handles btn_Del.Click
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eDelete)
	End Sub

	Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
		Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(Plot.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
		_ParentVM.LineaSelezionata(Me, Selezione)
	End Sub

	Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
		Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
		_ParentVM.LineaSelezionata(Me, Selezione)
	End Sub

	Private Sub btn_ZoomExtents_Click(sender As Object, e As RoutedEventArgs)
		Plot.ZoomExtents()
	End Sub

	Private Sub btn_YExtends_Click(sender As Object, e As RoutedEventArgs)
		Plot.ZoomExtentsY()
	End Sub

	Private Sub btn_YzoomIn_Click(sender As Object, e As RoutedEventArgs)
		Plot.YAxes.First.ZoomBy(-0.1, -0.1)
	End Sub

	Private Sub btn_YzoomOut_Click(sender As Object, e As RoutedEventArgs)
		Plot.YAxes.First.ZoomBy(0.1, 0.1)
	End Sub

	Private Sub btn_Btm_Click(sender As Object, e As RoutedEventArgs)
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eMoveBottom)
	End Sub

	Private Sub btn_Top_Click(sender As Object, e As RoutedEventArgs)
		_ParentVM.OperazioniSuListaControlli(Me, eTipoOperazione.eMoveTop)
	End Sub
End Class
