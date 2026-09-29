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
Public Class UserControlStraightLine
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
    InitializeComponent()

    StraightLineVM2020.Andatura = StraightLineVM2020.eAndatura.Vmg ' ShowVmg
    OutPutType.ItemsSource = System.Enum.GetValues(GetType(clsStraightLineVM2020.eOutputType)).Cast(Of clsStraightLineVM2020.eOutputType)


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
    Dim ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsAll.Select(Function(x) x.ChannelName).ToList
    Dim res As List(Of String) = GestisciListaCanali("Straight Line Channels", DataProvider2020.Channels.ListaCanali.ToList, ListaTmp, eSelectChannelType.eTwin)
    If res.Count > 0 Then
      AppConfig.ActiveProfile.ImpostaStraightLineChartSettings(AppConfig.ActiveProfile.StraightLineChartSettingsAll, res)
      StraightLineVM2020.AggiornaCanali()
      StraightLineVM2020.VerificaSelectedStraightLines()
      StraightLineVM2020.AggiornaGrafici()
      StraightLineVM2020.AggiornaPeriodiVisibili()
      RefreshExtents()
      AppConfig.Salva()
    End If
  End Sub

  Private Sub btn_SelectExportChannels_Click(sender As Object, e As RoutedEventArgs) Handles btn_SelectExportChannels.Click
    Stop
  End Sub

  Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Stop
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries) = DirectCast(DirectCast(sender, SciChart.Charting.ChartModifiers.SeriesSelectionModifier).ParentSurface.SelectedRenderableSeries, ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    StraightLineVM2020.AggiornaControlli(Me, Selezione)
  End Sub

  Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers
    StraightLineVM2020.AggiornaControlli(Me, Selezione)
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

  Private Sub btn_PrintPdf_Click(sender As Object, e As RoutedEventArgs)
    StraightLineVM2020.CreaReportPdf(AppConfig.ActiveProfile.StraightLineChartSettingsAll, "")
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

End Class



Public Class clsTrendLines
  Dim _UpPort As New List(Of clsDoubleXY)
  Dim _UpStbd As New List(Of clsDoubleXY)
  Dim _DnPort As New List(Of clsDoubleXY)
  Dim _DnStbd As New List(Of clsDoubleXY)

  Public Property UpPort As List(Of clsDoubleXY)
    Get
      Return _UpPort
    End Get
    Set(value As List(Of clsDoubleXY))
      _UpPort = value
    End Set
  End Property

  Public Property UpStbd As List(Of clsDoubleXY)
    Get
      Return _UpStbd
    End Get
    Set(value As List(Of clsDoubleXY))
      _UpStbd = value
    End Set
  End Property

  Public Property DnPort As List(Of clsDoubleXY)
    Get
      Return _DnPort
    End Get
    Set(value As List(Of clsDoubleXY))
      _DnPort = value
    End Set
  End Property

  Public Property DnStbd As List(Of clsDoubleXY)
    Get
      Return _DnStbd
    End Get
    Set(value As List(Of clsDoubleXY))
      _DnStbd = value
    End Set
  End Property

  Public Function AccodaCoppia(IsUp As Boolean, isStbd As Boolean, X As Double, Y As Double) As String
    If Double.IsNaN(X) Then Return ""
    If Double.IsNaN(Y) Then Return ""
    If IsUp Then
      If isStbd Then
        _UpStbd.Add(New clsDoubleXY(X, Y))
        Return "UpStbd"
      Else
        _UpPort.Add(New clsDoubleXY(X, Y))
        Return "UpPort"
      End If
    Else
      If isStbd Then
        _DnStbd.Add(New clsDoubleXY(X, Y))
        Return "DnStbd"
      Else
        _DnPort.Add(New clsDoubleXY(X, Y))
        Return "DnPort"
      End If
    End If
  End Function

  Public Sub ClearData()
    _UpPort.Clear()
    _UpStbd.Clear()
    _DnPort.Clear()
    _DnStbd.Clear()
  End Sub


  Private Function ArrayDbl(IsUp As Boolean, isStbd As Boolean, X As Boolean) As Double()
    If IsUp Then
      If isStbd Then
        If X Then
          Return _UpStbd.OrderBy(Function(y) y.X).Select(Function(z) z.X).ToArray
        Else
          Return _UpStbd.OrderBy(Function(y) y.X).Select(Function(z) z.Y).ToArray
        End If
      Else
        If X Then
          Return _UpPort.OrderBy(Function(y) y.X).Select(Function(z) z.X).ToArray
        Else
          Return _UpPort.OrderBy(Function(y) y.X).Select(Function(z) z.Y).ToArray
        End If
      End If
    Else
      If isStbd Then
        If X Then
          Return _DnStbd.OrderBy(Function(y) y.X).Select(Function(z) z.X).ToArray
        Else
          Return _DnStbd.OrderBy(Function(y) y.X).Select(Function(z) z.Y).ToArray
        End If
      Else
        If X Then
          Return _DnPort.OrderBy(Function(y) y.X).Select(Function(z) z.X).ToArray
        Else
          Return _DnPort.OrderBy(Function(y) y.X).Select(Function(z) z.Y).ToArray
        End If
      End If
    End If
  End Function


  Public Sub StampaTrendLines(Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), VM As clsStraightLineVM2020)
    StampaTrendLine(True, True, "TL_UpStbd", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.StraightLinesSelectionSettings.StbdSelected AndAlso VM.StraightLinesSelectionSettings.UpwindSelected)
    StampaTrendLine(True, False, "TL_UpPort", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.StraightLinesSelectionSettings.PortSelected AndAlso VM.StraightLinesSelectionSettings.UpwindSelected)
    StampaTrendLine(False, True, "TL_DnStbd", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.StraightLinesSelectionSettings.StbdSelected AndAlso VM.StraightLinesSelectionSettings.DownwindSelected)
    StampaTrendLine(False, False, "TL_DnPort", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.StraightLinesSelectionSettings.PortSelected AndAlso VM.StraightLinesSelectionSettings.DownwindSelected)
  End Sub

  Public Sub StampaTrendLines(Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), VM As UserControlPavarotViewModel)
    StampaTrendLine(True, True, "TL_UpStbd", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.PavarotSelectionSettings.StbdSelected AndAlso VM.PavarotSelectionSettings.UpwindSelected)
    StampaTrendLine(True, False, "TL_UpPort", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.PavarotSelectionSettings.PortSelected AndAlso VM.PavarotSelectionSettings.UpwindSelected)
    StampaTrendLine(False, True, "TL_DnStbd", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.PavarotSelectionSettings.StbdSelected AndAlso VM.PavarotSelectionSettings.DownwindSelected)
    StampaTrendLine(False, False, "TL_DnPort", Ordine, SeriesSource, VM.PlotTrendLines AndAlso VM.PavarotSelectionSettings.PortSelected AndAlso VM.PavarotSelectionSettings.DownwindSelected)
  End Sub

  Public Sub StampaTrendLines(Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel))
    StampaTrendLine(True, True, "TL_UpStbd", Ordine, SeriesSource, True)
    StampaTrendLine(True, False, "TL_UpPort", Ordine, SeriesSource, True)
    StampaTrendLine(False, True, "TL_DnStbd", Ordine, SeriesSource, True)
    StampaTrendLine(False, False, "TL_DnPort", Ordine, SeriesSource, True)
  End Sub

  Public Sub StampaTrendLine(IsUp As Boolean, isStbd As Boolean, TagString As String, Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), Stampa As Boolean)


    Dim ds3 = SeriesSource.Where(Function(k) Not k.RenderSeries Is Nothing)
    Dim ds2 = ds3.Where(Function(k) Not k.RenderSeries.DataSeries Is Nothing)
    Dim ds1 = ds2.Where(Function(k) TypeOf (k.RenderSeries.DataSeries.Tag) Is String)
    Dim DS = ds1.Where(Function(z) z.RenderSeries.DataSeries.Tag.ToString = TagString).FirstOrDefault
    'Dim dst = SeriesSource.Where(Function(z) z.RenderSeries.DataSeries.Tag.ToString = TagString)
    If Not DS Is Nothing Then
      SeriesSource.Remove(DS)
    End If
    Dim x As Double() = ArrayDbl(IsUp, isStbd, True)
    Dim y As Double() = ArrayDbl(IsUp, isStbd, False)
    If x.Count > 4 Then
      Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ', MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquation)
      Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.StrokeThickness = 2
      If isStbd Then
        LineaTmp.Stroke = Colors.Green
        If IsUp Then
          LineaTmp.StrokeDashArray = {3, 3}
        End If
      Else
        LineaTmp.Stroke = Colors.Red
        If IsUp Then
          LineaTmp.StrokeDashArray = {3, 3}
        End If
      End If
      LineaTmp.Tag = TagString
      LineaTmp.IsVisible = Stampa
      DataSeriesTmp.AcceptsUnsortedData = True
      'For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
      For i As Double = x.Min To x.Max Step (x.Max - x.Min) / 10
        Dim ii As Double = p.Coefficients(0)
        For g As Integer = 1 To Ordine
          ii += p.Coefficients(g) * i ^ g
        Next
        DataSeriesTmp.Append(i, ii, New clsPuntoMetadata(False))
      Next
      LineaTmp.DataSeries = DataSeriesTmp
      DataSeriesTmp.SeriesName = TagString.Replace("TL_", "TrndLn")
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, LineaTmp)
      SeriesSource.Add(CSVMtgtup)
    End If

  End Sub

  Public Sub StampaTrendLine(IsUp As Boolean, TagString As String, Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), Visible As Boolean)
    Dim DS = SeriesSource.Where(Function(z) z.RenderSeries.DataSeries.Tag = TagString).FirstOrDefault
    If Not DS Is Nothing Then
      SeriesSource.Remove(DS)
    End If
    Dim ListTmp As List(Of Double) = ArrayDbl(IsUp, True, True).ToList
    ListTmp.AddRange(ArrayDbl(IsUp, False, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(IsUp, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(IsUp, False, False).ToList)
    Dim y As Double() = ListTmp.ToArray
    If x.Count > 0 Then
      Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
      Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.StrokeThickness = 3
      If IsUp Then
        LineaTmp.Stroke = Colors.DarkRed
      Else
        LineaTmp.Stroke = Colors.DarkBlue
      End If
      LineaTmp.StrokeDashArray = {3, 3}
      LineaTmp.Tag = TagString
      LineaTmp.IsVisible = Visible
      DataSeriesTmp.AcceptsUnsortedData = True
      For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
        Dim ii As Double = p.Coefficients(0)
        For g As Integer = 1 To Ordine
          ii += p.Coefficients(g) * i ^ g
        Next
        DataSeriesTmp.Append(i, ii, New clsPuntoMetadata(False))
      Next
      LineaTmp.DataSeries = DataSeriesTmp
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, LineaTmp)
      SeriesSource.Add(CSVMtgtup)
    End If

  End Sub

  Public Sub StampaTrendLinePortStbd(IsStbd As Boolean, TagString As String, Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), Visible As Boolean)
    Dim DS = SeriesSource.Where(Function(z) z.RenderSeries.DataSeries.Tag = TagString).FirstOrDefault
    If Not DS Is Nothing Then
      SeriesSource.Remove(DS)
    End If
    Dim ListTmp As List(Of Double) = ArrayDbl(True, IsStbd, True).ToList
    ListTmp.AddRange(ArrayDbl(False, IsStbd, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(True, IsStbd, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, IsStbd, False).ToList)
    Dim y As Double() = ListTmp.ToArray
    If x.Count > 0 Then
      Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
      Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.StrokeThickness = 3
      If IsStbd Then
        LineaTmp.Stroke = Colors.Green
      Else
        LineaTmp.Stroke = Colors.Red
      End If
      LineaTmp.StrokeDashArray = {3, 3}
      LineaTmp.Tag = TagString
      LineaTmp.IsVisible = Visible
      DataSeriesTmp.AcceptsUnsortedData = True
      For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
        Dim ii As Double = p.Coefficients(0)
        For g As Integer = 1 To Ordine
          ii += p.Coefficients(g) * i ^ g
        Next
        DataSeriesTmp.Append(i, ii, New clsPuntoMetadata(False))
      Next
      LineaTmp.DataSeries = DataSeriesTmp
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, LineaTmp)
      SeriesSource.Add(CSVMtgtup)
    End If

  End Sub

  Public Sub StampaTrendLine(TagString As String, Ordine As Integer, SeriesSource As ObservableCollection(Of IChartSeriesViewModel), Visible As Boolean)
    Dim DS = SeriesSource.Where(Function(z) z.RenderSeries.DataSeries.Tag = TagString).FirstOrDefault
    If Not DS Is Nothing Then
      SeriesSource.Remove(DS)
    End If
    Dim ListTmp As List(Of Double) = ArrayDbl(True, True, True).ToList
    ListTmp.AddRange(ArrayDbl(True, False, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(True, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(True, False, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, False).ToList)
    Dim y As Double() = ListTmp.ToArray
    If x.Count > 0 Then
      Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
      Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.StrokeThickness = 4
      LineaTmp.StrokeDashArray = {3, 3}
      LineaTmp.Stroke = Colors.DarkOrange
      LineaTmp.Tag = TagString
      LineaTmp.IsVisible = Visible
      DataSeriesTmp.AcceptsUnsortedData = True
      For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
        Dim ii As Double = p.Coefficients(0)
        For g As Integer = 1 To Ordine
          ii += p.Coefficients(g) * i ^ g
        Next
        DataSeriesTmp.Append(i, ii, New clsPuntoMetadata(False))
      Next
      LineaTmp.DataSeries = DataSeriesTmp
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, LineaTmp)
      SeriesSource.Add(CSVMtgtup)
    End If

  End Sub

  Public Function TabellaTrendLine(IsUp As Boolean, isStbd As Boolean, Xheader As String, Yheader As String, Ordine As Integer) As String
    Try
      Dim strTmp As String = IIf(IsUp, "Upwind", "Downwind") & " " & IIf(isStbd, "Stbd", "Port") & vbCrLf
      strTmp &= Xheader & vbTab & Yheader & vbCrLf
      Dim x As Double() = ArrayDbl(IsUp, isStbd, True)
      Dim y As Double() = ArrayDbl(IsUp, isStbd, False)
      If x.Count > 0 Then
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
        For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
          Dim ii As Double = p.Coefficients(0)
          For g As Integer = 1 To Ordine
            ii += p.Coefficients(g) * i ^ g
          Next
          strTmp &= i.ToString("F3") & vbTab & ii.ToString("F3") & vbCrLf
        Next
        Return strTmp
      Else
        Return ""
      End If
    Catch ex As Exception
      Return ""
    End Try
  End Function

  Public Function FormulaTrendLine(IsUp As Boolean, isStbd As Boolean, Xheader As String, Yheader As String, Ordine As Integer) As String
    Dim strTmp As String = IIf(IsUp, "Upwind", "Downwind") & " " & IIf(isStbd, "Stbd", "Port") & vbTab
    strTmp &= "X:" & Xheader & vbTab & "Y:" & Yheader & vbTab
    Dim x As Double() = ArrayDbl(IsUp, isStbd, True)
    Dim y As Double() = ArrayDbl(IsUp, isStbd, False)
    Return strTmp & FormulaTrendLine(x, y, Ordine)
    'If x.Count > 0 Then
    '	Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)

    '	strTmp &= p.Coefficients(0) & vbTab
    '	strTmp &= p.Coefficients(1) & vbTab ' & "X" & vbTab
    '	For g As Integer = 2 To Ordine
    '		strTmp &= p.Coefficients(g) & vbTab '& "X^" & g & vbTab
    '	Next
    '	Return strTmp.TrimEnd(vbTab) & vbCrLf
    'Else
    '	Return ""
    'End If
  End Function

  Public Function TabellaTrendLine(IsUp As Boolean, Xheader As String, Yheader As String, Ordine As Integer) As String
    Try
      Dim strTmp As String = IIf(IsUp, "Upwind", "Downwind") & vbCrLf
      strTmp &= Xheader & vbTab & Yheader & vbCrLf
      Dim ListTmp As List(Of Double) = ArrayDbl(IsUp, True, True).ToList
      ListTmp.AddRange(ArrayDbl(IsUp, False, True).ToList)
      Dim x As Double() = ListTmp.ToArray
      ListTmp.Clear()
      ListTmp.AddRange(ArrayDbl(IsUp, True, False).ToList)
      ListTmp.AddRange(ArrayDbl(IsUp, False, False).ToList)
      Dim y As Double() = ListTmp.ToArray

      If x.Count > 0 Then
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
        For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
          Dim ii As Double = p.Coefficients(0)
          For g As Integer = 1 To Ordine
            ii += p.Coefficients(g) * i ^ g
          Next
          strTmp &= i.ToString("F3") & vbTab & ii.ToString("F3") & vbCrLf
        Next
        Return strTmp
      Else
        Return ""
      End If
    Catch ex As Exception
      Return ""
    End Try
  End Function

  Public Function FormulaTrendLine(IsUp As Boolean, Xheader As String, Yheader As String, Ordine As Integer) As String
    Dim strTmp As String = IIf(IsUp, "Upwind", "Downwind") & vbTab
    strTmp &= "X:" & Xheader & vbTab & "Y:" & Yheader & vbTab
    Dim ListTmp As List(Of Double) = ArrayDbl(IsUp, True, True).ToList
    ListTmp.AddRange(ArrayDbl(IsUp, False, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(IsUp, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(IsUp, False, False).ToList)
    Dim y As Double() = ListTmp.ToArray
    Return strTmp & FormulaTrendLine(x, y, Ordine)
    'If x.Count > 0 Then
    '	Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)

    '	strTmp &= p.Coefficients(0) & vbTab
    '	strTmp &= p.Coefficients(1) & vbTab ' & "X" & vbTab
    '	For g As Integer = 2 To Ordine
    '		strTmp &= p.Coefficients(g) & vbTab '& "X^" & g & vbTab
    '	Next
    '	Return strTmp.TrimEnd(vbTab) & vbCrLf
    'Else
    '	Return ""
    'End If
  End Function

  Public Function TabellaTrendLine(Xheader As String, Yheader As String, Ordine As Integer) As String
    Dim strTmp As String = "All Together" & vbTab
    strTmp &= Xheader & vbTab & Yheader & vbCrLf
    Dim ListTmp As List(Of Double) = ArrayDbl(True, True, True).ToList
    ListTmp.AddRange(ArrayDbl(True, False, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(True, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(True, False, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, False).ToList)
    Dim y As Double() = ListTmp.ToArray

    If x.Count > Ordine + 1 Then
      If IsPositiveDefiniteMatrix(x, y) Then
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
        For i As Double = x.Min * 0.95 To x.Max * 1.05 Step (x.Max - x.Min) / 10
          Dim ii As Double = p.Coefficients(0)
          For g As Integer = 1 To Ordine
            ii += p.Coefficients(g) * i ^ g
          Next
          strTmp &= i.ToString("F3") & vbTab & ii.ToString("F3") & vbCrLf
        Next
        Return strTmp
      Else
        Return ""
      End If
    Else
      Return ""
    End If
  End Function

  Public Function FormulaTrendLineFirstRow(Ordine As Integer) As String
    Dim strTmp As String = vbTab & vbTab & vbTab & "0" & vbTab
    strTmp &= "1" & vbTab
    For g As Integer = 2 To Ordine
      strTmp &= g & vbTab
    Next
    Return strTmp.TrimEnd(vbTab)

  End Function

  Function IsPositiveDefiniteMatrix(mm As Double(,)) As Boolean
    Dim rows = mm.GetLength(0)
    Dim cols = mm.GetLength(1)
    If rows <> cols Then Return False
    Dim M = MathNet.Numerics.LinearAlgebra.Matrix(Of Double).Build.DenseOfArray(mm)
    If Not M.Equals(M.Transpose()) Then Return False
    Try
      Dim chol = M.Cholesky() ' eccezione se non PD
      Return True
    Catch
      Return False
    End Try
  End Function

  Function IsPositiveDefiniteMatrix(rows As Double(), cols As Double()) As Boolean
    If rows.Length <> cols.Length Then Return False
    Dim n As Integer = rows.Length
    Dim mm(n, 1) As Double
    For i As Integer = 0 To rows.Length - 1
      mm(i, 0) = rows(i)
      mm(i, 1) = cols(i)
    Next
    'Dim prev As Double
    'For i As Integer = 0 To rows.Length - 1
    '  Dim v As Double = rows(i)
    '  If v <= prev Then
    '    Stop
    '  End If
    '  prev = v
    'Next
    'prev = 0
    'For i As Integer = 0 To cols.Length - 1
    '  Dim v As Double = cols(i)
    '  If v <= prev Then
    '    Stop
    '  End If
    '  prev = v
    'Next


    Dim M = MathNet.Numerics.LinearAlgebra.Matrix(Of Double).Build.DenseOfArray(mm)
    If Not M.Equals(M.Transpose()) Then Return False
    Try
      Dim chol = M.Cholesky() ' eccezione se non PD
      Return True
    Catch
      Return False
    End Try
  End Function

  Public Function FormulaTrendLine(Xheader As String, Yheader As String, Ordine As Integer) As String
    Dim strTmp As String = "All Together" & vbTab
    strTmp &= "X:" & Xheader & vbTab & "Y:" & Yheader & vbTab
    Dim ListTmp As List(Of Double) = ArrayDbl(True, True, True).ToList
    ListTmp.AddRange(ArrayDbl(True, False, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, True).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, True).ToList)
    Dim x As Double() = ListTmp.ToArray
    ListTmp.Clear()
    ListTmp.AddRange(ArrayDbl(True, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(True, False, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, True, False).ToList)
    ListTmp.AddRange(ArrayDbl(False, False, False).ToList)
    Dim y As Double() = ListTmp.ToArray
    Return strTmp & FormulaTrendLine(x, y, Ordine)

  End Function

  Private Function FormulaTrendLine(x As Double(), y As Double(), Ordine As Integer) As String
    Try
      If x.Count > 0 Then
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)

        Dim strTmp As String = p.Coefficients(0) & vbTab
        strTmp &= p.Coefficients(1) & vbTab ' & "X" & vbTab
        For g As Integer = 2 To Ordine
          strTmp &= p.Coefficients(g) & vbTab '& "X^" & g & vbTab
        Next
        Return strTmp.TrimEnd(vbTab)
      Else
        Return ""
      End If
    Catch ex As Exception
      Return ""
    End Try
  End Function


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsStraightLinesSelectionSettings

  Public Property _PerformanceSelected As Boolean = True
  Public Property _TechnicalSelected As Boolean = False

  Public Property _StbdSelected As Boolean = True
  Public Property _PortSelected As Boolean = True
  Public Property _UpwindSelected As Boolean = True
  Public Property _DownwindSelected As Boolean = True



  Public Property _ApplyTwsFilter As Boolean = False
  Public Property _ApplyTwsMin As Boolean = False
  Public Property _TwsMin As Double = 6
  Public Property _ApplyTwsMax As Boolean = False
  Public Property _TwsMax As Double = 25

  Public Property _ApplyTwaFilter As Boolean = False
  Public Property _ApplyTwaMin As Boolean = False
  Public Property _TwaMin As Double = 35
  Public Property _ApplyTwaMax As Boolean = False
  Public Property _TwaMax As Double = 160

  Public Event SettingChanged()

  Public Property PerformanceSelected As Boolean
    Get
      Return _PerformanceSelected
    End Get
    Set(value As Boolean)
      _PerformanceSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property TechnicalSelected As Boolean
    Get
      Return _TechnicalSelected
    End Get
    Set(value As Boolean)
      _TechnicalSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property StbdSelected As Boolean
    Get
      Return _StbdSelected
    End Get
    Set(value As Boolean)
      _StbdSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property PortSelected As Boolean
    Get
      Return _PortSelected
    End Get
    Set(value As Boolean)
      _PortSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property UpwindSelected As Boolean
    Get
      Return _UpwindSelected
    End Get
    Set(value As Boolean)
      _UpwindSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property DownwindSelected As Boolean
    Get
      Return _DownwindSelected
    End Get
    Set(value As Boolean)
      _DownwindSelected = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property ApplyTwsFilter As Boolean
    Get
      Return _ApplyTwsFilter
    End Get
    Set(value As Boolean)
      _ApplyTwsFilter = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property ApplyTwsMin As Boolean
    Get
      Return _ApplyTwsMin
    End Get
    Set(value As Boolean)
      _ApplyTwsMin = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property TwsMin As Double
    Get
      Return _TwsMin
    End Get
    Set(value As Double)
      _TwsMin = value
      'RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property ApplyTwsMax As Boolean
    Get
      Return _ApplyTwsMax
    End Get
    Set(value As Boolean)
      _ApplyTwsMax = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property TwsMax As Double
    Get
      Return _TwsMax
    End Get
    Set(value As Double)
      _TwsMax = value
      'RaiseEvent SettingChanged()
    End Set
  End Property

  <DependsOn("ApplyTwsFilter", "ApplyTwsMin", "_ApplyTwsFilter", "_ApplyTwsMin")>
  Public ReadOnly Property TwsMinEnabled As Boolean
    Get
      Return _ApplyTwsFilter AndAlso _ApplyTwsMin
    End Get
  End Property

  <DependsOn("ApplyTwsFilter", "ApplyTwsMax", "_ApplyTwsFilter", "_ApplyTwsMax")>
  Public ReadOnly Property TwsMaxEnabled As Boolean
    Get
      Return _ApplyTwsFilter AndAlso _ApplyTwsMax
    End Get
  End Property

  Public Property ApplyTwaFilter As Boolean
    Get
      Return _ApplyTwaFilter
    End Get
    Set(value As Boolean)
      _ApplyTwaFilter = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property ApplyTwaMin As Boolean
    Get
      Return _ApplyTwaMin
    End Get
    Set(value As Boolean)
      _ApplyTwaMin = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property TwaMin As Double
    Get
      Return _TwaMin
    End Get
    Set(value As Double)
      _TwaMin = value
      'RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property ApplyTwaMax As Boolean
    Get
      Return _ApplyTwaMax
    End Get
    Set(value As Boolean)
      _ApplyTwaMax = value
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public Property TwaMax As Double
    Get
      Return _TwaMax
    End Get
    Set(value As Double)
      _TwaMax = value
      'RaiseEvent SettingChanged()
    End Set
  End Property

  <DependsOn("ApplyTwaFilter", "ApplyTwaMin", "_ApplyTwaFilter", "_ApplyTwaMin")>
  Public ReadOnly Property TwaMinEnabled As Boolean
    Get
      Return _ApplyTwaFilter AndAlso _ApplyTwaMin
    End Get
  End Property

  <DependsOn("ApplyTwaFilter", "ApplyTwaMax", "_ApplyTwaFilter", "_ApplyTwaMax")>
  Public ReadOnly Property TwaMaxEnabled As Boolean
    Get
      Return _ApplyTwaFilter AndAlso _ApplyTwaMax
    End Get
  End Property

  Public Sub VerificaStraightLines(Lista As ObservableCollection(Of clsPeriod2021))
    If Not PeriodsManager Is Nothing Then
      For Each sl In Lista
        If sl.TR.IsOverlapped(DataPlotSync.VisibleRange) Then
          sl.IsChecked = StraightLineIsSelected(sl)
          'If sl.IsChecked Then
          '    sl.IsChecked = StraightLineIsSelected(sl)
          'End If
        Else
          sl.IsChecked = False
        End If
      Next
      'PeriodsManager.Periods.AggiornaCollections()
    End If
  End Sub

  Public Function StraightLineIsSelected(Period As clsPeriod2021) As Boolean
    Dim Valid As Boolean = False
    If Not Period.TechnicalOrSystems And PerformanceSelected Then
      Valid = True
    ElseIf Period.TechnicalOrSystems And TechnicalSelected Then
      Valid = True
    End If
    If Not Valid Then Return False
    If Not Period.StraightLineVmgDetails Is Nothing Then
      Valid = TwaValido(Period.StraightLineVmgDetails.Twa.AvgVal)
    ElseIf Not Period.StraightLineReachingDetails Is Nothing Then
      Valid = TwaValido(Period.StraightLineReachingDetails.Twa.AvgVal)
    Else
      Return False
    End If
    If Not Valid Then Return False
    Return TwsValido(Period.TwsDetails.AvgVal)

  End Function

  Private Function CheckStbdPortUpDn(Twa As Double) As Boolean
    If Twa < 0 Then
      If Not PortSelected Then Return False
    Else
      If Not StbdSelected Then Return False
    End If
    If Not UpwindSelected AndAlso Not DownwindSelected Then Return True ' reaching
    If Math.Abs(Twa) <= 90 Then
      If Not UpwindSelected Then Return False
    Else
      If Not DownwindSelected Then Return False
    End If
    Return True
  End Function


  Private Function TwaValido(ValoreTwa As Double) As Boolean
    Dim AbsTwa As Double = System.Math.Abs(ValoreTwa)
    If Double.IsNaN(ValoreTwa) Then
      Return False
    Else
      If Not CheckStbdPortUpDn(ValoreTwa) Then Return False
      If ApplyTwaFilter Then
        If ApplyTwaMin Then
          Return AbsTwa >= TwaMin And AbsTwa <= TwaMax
        ElseIf ApplyTwaMax Then
          Return AbsTwa <= TwaMax
        Else
          Return True
        End If
      Else
        Return True
      End If
    End If
    Return False
  End Function



  Private Function TwsValido(ValoreTws As Double) As Boolean
    If Double.IsNaN(ValoreTws) Then
      Return False
    Else
      If ApplyTwsFilter Then
        If ApplyTwsMin Then
          Return ValoreTws >= TwsMin And ValoreTws <= TwsMax
        ElseIf ApplyTwsMax Then
          Return ValoreTws <= TwsMax
        Else
          Return True
        End If
      Else
        Return True
      End If
    End If
  End Function
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineVM2020

  Public Property StraightLineTrackXyPerMinute As clsStraightLineTrackXyPerMinute
  Public Property ContenitoreTwsVsChannels As clsStraightLineContainerViewModel
  Public Property ContenitoreDistributions As clsStraightLineContainerViewModel

  Public Property ZoomPanEnabled As Boolean = False
  Public Property DataPointSelectionEnabled As Boolean = True
  Public Property LoadedDataMouseMode As eLoadedDataMouseMode = eLoadedDataMouseMode.eSelection
  Public Property StraightLineChartSync As New clsStraightLineChartSync
  Public Property HorizontalOffset As Double
  Public Property PlotConsistency As Boolean = False
  Dim _OutputType As eOutputType = eOutputType.eColorByVmgTgtPerc
  Public Property AutoRefresh As Boolean = False
  Public Property PlotTargetsIfAvailable As Boolean = True
  Public Property PlotTrendLines As Boolean = False
  Public Property NormalizeDistributions As Boolean = True

  Public WithEvents StraightLinesSelectionSettings As New clsStraightLinesSelectionSettings

  Public Property CurrentPeriodDescription As String

  Public Property PeriodDetailsFontSize As Double = 16

  Public Property ListaPeriodiCaricati As List(Of clsPeriod2021)

  Public Property Andatura As eAndatura

  Public Enum eAndatura
    UpwindVmg = 0
    DownwindVmg = 1
    Reaching = 2
    Vmg = 3
  End Enum

  'Dim ListaChiavi As New Dictionary(Of String, Color)

  Public Event GraficiAggiornati()

  Public Sub CurrentPeriod(Period As clsPeriod2021)
    If Period Is Nothing Then
      CurrentPeriodDescription = ""
    Else
      CurrentPeriodDescription = Period.DescrizioneMultiriga
    End If
  End Sub

  Public Property OutputType As eOutputType
    Get
      Return _OutputType
    End Get
    Set(value As eOutputType)
      _OutputType = value
      VerificaAggiornaGrafici()
    End Set
  End Property


  Public Sub CreaReportPdf(ListaCanali As List(Of clsStraightLineChartSettings), Filtro As String)
    If ContenitoreTwsVsChannels Is Nothing Then Exit Sub
    If ContenitoreTwsVsChannels.ListaControlli Is Nothing Then Exit Sub
    If ContenitoreTwsVsChannels.ListaControlli.Count = 0 Then Exit Sub
    Dim ListaAvg As New List(Of SciChart.Charting.Visuals.SciChartSurface)
    For Each c In ContenitoreTwsVsChannels.ListaControlli
      ListaAvg.Add(c.Plots)
    Next

    Dim ListaDistr As New List(Of SciChart.Charting.Visuals.SciChartSurface)
    For Each c In ContenitoreDistributions.ListaControlli
      ListaDistr.Add(c.Plots)
    Next

    Dim objPdf As New clsPdf
    objPdf.StampaReportStraightLine(ListaAvg, ListaDistr, OutputType, Lista.ToList, ListaCanali, Filtro)
  End Sub

  Public Sub FontSizeSmaller()
    PeriodDetailsFontSize -= 1
    'OnPropertyChanged("PeriodDetailsFontSize")
  End Sub

  Public Sub FontSizeBigger()
    PeriodDetailsFontSize += 1
    'OnPropertyChanged("PeriodDetailsFontSize")
  End Sub


  Public Property Lista As ObservableCollection(Of clsPeriod2021)
    Get
      Select Case Andatura
        Case eAndatura.Reaching
          Return PeriodsManager.Periods.CollectionStraightLineReaching
        Case eAndatura.UpwindVmg
          Return PeriodsManager.Periods.CollectionStraightLineVmgUp
        Case eAndatura.DownwindVmg
          Return PeriodsManager.Periods.CollectionStraightLineVmgDn
        Case Else
          Return PeriodsManager.Periods.CollectionStraightLineAll
      End Select
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      Select Case Andatura
        Case eAndatura.Reaching
          PeriodsManager.Periods.CollectionStraightLineReaching = value
        Case eAndatura.UpwindVmg
          PeriodsManager.Periods.CollectionStraightLineVmgUp = value
        Case eAndatura.DownwindVmg
          PeriodsManager.Periods.CollectionStraightLineVmgDn = value
        Case Else
          PeriodsManager.Periods.CollectionStraightLineAll = value
      End Select
    End Set
  End Property


  Public Property PeriodsMng As clsPeriodsManager2021
    Get
      Return PeriodsManager
    End Get
    Set(value As clsPeriodsManager2021)
      PeriodsManager = value
    End Set
  End Property

  Public Sub New()

    StraightLineTrackXyPerMinute = New clsStraightLineTrackXyPerMinute(StraightLineChartSync, Me)
    ContenitoreTwsVsChannels = New clsStraightLineContainerViewModel(False, StraightLineChartSync, Nothing, Me)
    ContenitoreDistributions = New clsStraightLineContainerViewModel(True, StraightLineChartSync, Nothing, Me)

  End Sub

  Public Enum eLoadedDataMouseMode
    eSelection = 0
    ePan = 1
  End Enum


  Public Property CanaliDaStampare As ObservableCollection(Of clsChannel2020)
    Get
      Return StraightLineChartSync.CanaliDaStampare
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      StraightLineChartSync.CanaliDaStampare = value
    End Set
  End Property

  Public Enum eOutputType
    eSinglePeriod = 0
    eColorByTack = 1
    eColorByKey = 2
    eGroupByTack = 3
    eGroupByKey = 4
    eColorByVmgTgtPerc = 5
    eColorByBsPolarPerc = 6
    eGroupByTackUpDn = 7
    eGroupByUpDn = 8
  End Enum

  Private Function PeriodiNonCambiati() As Boolean
    If Not ListaPeriodiCaricati.ToList.Count = PeriodsManager.ListaStraightLine.Count Then Return False
    For i As Integer = 0 To Lista.ToList.Count - 1
      If Not ListaPeriodiCaricati(i).TR Is Lista(i).TR Then Return False
    Next
    Return True
  End Function

  Public Function VerificaAggiornaGrafici() As Boolean
    'é una funzione in quanto fa da trigger per il zoomextents dei grafici
    If ListaPeriodiCaricati Is Nothing Then Return True
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eSinglePeriod, eOutputType.eColorByTack, eOutputType.eColorByVmgTgtPerc, eOutputType.eColorByBsPolarPerc
        If PeriodiNonCambiati() Then
          AggiornaPeriodiVisibili()
        Else
          AggiornaGrafici()
        End If
        Return True
      Case Else
        If AutoRefresh Then
          AggiornaGrafici()
          Return True
        End If
    End Select
    Return False
  End Function


  Public Function ChangeMouseMode() As String
    Dim descrizione As String = ""
    Select Case LoadedDataMouseMode
      Case eLoadedDataMouseMode.ePan
        LoadedDataMouseMode = eLoadedDataMouseMode.eSelection
        ZoomPanEnabled = False
        DataPointSelectionEnabled = True
        descrizione = "mMode: Selection"
      Case eLoadedDataMouseMode.eSelection
        LoadedDataMouseMode = eLoadedDataMouseMode.ePan
        ZoomPanEnabled = True
        DataPointSelectionEnabled = False
        descrizione = "mMode: Pan"
    End Select
    ContenitoreTwsVsChannels.StraightLineChartSync.DataPointSelectionEnabled = DataPointSelectionEnabled
    ContenitoreTwsVsChannels.StraightLineChartSync.ZoomPanEnabled = ZoomPanEnabled
    Return descrizione
  End Function

  Public Function CurrentList() As List(Of String)
    Dim ListaTmp
    Select Case Andatura
      Case eAndatura.UpwindVmg
        If AppConfig.ActiveProfile.StraightLineChartSettingsVmgUp Is Nothing Then
          AppConfig.ActiveProfile.ImpostaStraightLineChartSettingsDefault(AppConfig.ActiveProfile.StraightLineChartSettingsVmgUp)
        End If
        ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsVmgUp.Select(Function(x) x.ChannelName).ToList
      Case eAndatura.DownwindVmg
        If AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn Is Nothing Then
          AppConfig.ActiveProfile.ImpostaStraightLineChartSettingsDefault(AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn)
        End If
        ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsVmgDn.Select(Function(x) x.ChannelName).ToList
      Case eAndatura.Reaching
        If AppConfig.ActiveProfile.StraightLineChartSettingsReaching Is Nothing Then
          AppConfig.ActiveProfile.ImpostaStraightLineChartSettingsDefault(AppConfig.ActiveProfile.StraightLineChartSettingsReaching)
        End If
        ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsReaching.Select(Function(x) x.ChannelName).ToList
      Case Else
        If AppConfig.ActiveProfile.StraightLineChartSettingsAll Is Nothing Then
          AppConfig.ActiveProfile.ImpostaStraightLineChartSettingsDefault(AppConfig.ActiveProfile.StraightLineChartSettingsAll)
        End If
        ListaTmp = AppConfig.ActiveProfile.StraightLineChartSettingsAll.Select(Function(x) x.ChannelName).ToList
    End Select
    Return ListaTmp
  End Function


  Public Sub AggiornaCanali()
    LoadingProgressVisualizza()
    Dim ListaTmp = CurrentList.ToList
    CanaliDaStampare.Clear()
    For Each elemento In ListaTmp
      Dim c = DataProvider2020.CanaleDbl(elemento)
      If Not c Is Nothing Then
        CanaliDaStampare.Add(c)
      End If
    Next
    ' sopra è stata impostata la lista dei canali che vanno plottati
    LoadingProgressNascondi()
  End Sub

  Private Function RicaricaListaCanali() As Boolean
    If CanaliDaStampare.Count = 0 Then Return True
    Dim ListaTmp = CurrentList()
    If Not ListaTmp.Count = CanaliDaStampare.Count Then Return True
    For i As Integer = 0 To CanaliDaStampare.Count - 1
      If Not CanaliDaStampare(i).ChannelId = ListaTmp(i) Then Return True
    Next
    Return False
  End Function

  Public Sub AggiornaGrafici()
    Dim TmrAG As DateTime = Now
    If RicaricaListaCanali() Then AggiornaCanali()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " AG.1 - RicaricaListaCanali/AggiornaCanali " & Now.Subtract(TmrAG).TotalMilliseconds.ToString("F0") & " ms")
    ' vengono aggiornati i grafici
    LoadingProgressVisualizza()
    ListaPeriodiCaricati = Lista.ToList
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " AG.2 - Lista periodi: " & ListaPeriodiCaricati.Count & " OutputType: " & OutputType.ToString())
    Select Case OutputType
      Case eOutputType.eGroupByTack
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinuteGroupByTack(Nothing, False, True, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGraficiGroupByTack(False, True)
        ContenitoreDistributions.AggiornaGraficiGroupByTack(False, True)
        AggiornaPeriodiVisibili()
      Case eOutputType.eGroupByTackUpDn
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinuteGroupByTack(Nothing, False, True, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGraficiGroupByTack(True, True)
        ContenitoreDistributions.AggiornaGraficiGroupByTack(True, True)
        AggiornaPeriodiVisibili()
      Case eOutputType.eGroupByUpDn
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinuteGroupByTack(Nothing, False, True, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGraficiGroupByTack(True, False)
        ContenitoreDistributions.AggiornaGraficiGroupByTack(True, False)
        AggiornaPeriodiVisibili()
      Case eOutputType.eGroupByKey ', eOutputType.eGroupByTackAndKey
        ImpostaListaChiaviSoloSelected(ListaPeriodiCaricati) ' PeriodsManager.ListaStraightLineVmg)
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinuteGroupByTack(Nothing, False, True, ListaPeriodiCaricati) 'questo andrá implementato con il groupbykeys
        ContenitoreTwsVsChannels.AggiornaGraficiGroupByKeys()
        ContenitoreDistributions.AggiornaGraficiGroupByKeys()
        AggiornaPeriodiVisibili()
      Case eOutputType.eColorByTack
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinute(Nothing, False, True, True, OutputType, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGrafici(OutputType)
        ContenitoreDistributions.AggiornaGraficiGroupByTack(False, True)
      Case eOutputType.eColorByVmgTgtPerc, eOutputType.eColorByBsPolarPerc
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinute(Nothing, False, True, True, OutputType, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGrafici(OutputType)
        ContenitoreDistributions.AggiornaGraficiGroupByTack(False, True)
      Case eOutputType.eColorByKey
        ImpostaListaChiaviSoloSelected(ListaPeriodiCaricati) ' PeriodsManager.ListaStraightLineVmg)
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinute(Nothing, False, True, True, OutputType, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGrafici(OutputType)
        ContenitoreDistributions.AggiornaGraficiGroupByKeys()
        'ContenitoreDistributions.AggiornaGraficiGroupByTack()
      Case Else
        StraightLineTrackXyPerMinute.AggiornaGraficoTrackXyPerMinute(Nothing, False, True, False, OutputType, ListaPeriodiCaricati)
        ContenitoreTwsVsChannels.AggiornaGrafici(OutputType)
        ContenitoreDistributions.AggiornaGrafici(OutputType)
    End Select

    LoadingProgressNascondi()
  End Sub


  Public Sub AggiornaControlli(Periodo As clsPeriod2021)
    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
    For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      Dim Prd As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
      If Prd Is Periodo Then
        Selezione.Add(LineaFS)
      End If
    Next
    AggiornaControlli(Selezione)
  End Sub


  Public Sub AggiornaControlli(ControlloCorrente As UserControlStraightLineStandardPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    AggiornaControlli(Selezione)
  End Sub

  Public Sub AggiornaControlli(ControlloCorrente As UserControlStraightLine, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    AggiornaControlli(Selezione)
  End Sub

  Public Sub AggiornaControlli(ControlloCorrente As UserControlStraightLineStandardPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    AggiornaControlli(Selezione)
  End Sub

  Public Sub AggiornaControlli(ControlloCorrente As UserControlStraightLine, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    AggiornaControlli(Selezione)
  End Sub

  Public Sub AggiornaControlli(SailingTack As clsSailingState.eTack)
    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
    For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
        Dim Prd As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
        If Prd.IsStbd Then
          Select Case SailingTack
            Case clsSailingState.eTack.eBoth, clsSailingState.eTack.eStbd
              Selezione.Add(LineaFS)
          End Select
        Else
          Select Case SailingTack
            Case clsSailingState.eTack.eBoth, clsSailingState.eTack.ePort
              Selezione.Add(LineaFS)
          End Select
        End If
      End If
    Next
    AggiornaControlli(Selezione)
  End Sub


  Public Sub AggiornaPeriodiVisibili()
    'If PeriodsManager.PeriodsTrigger.ListaInAggiornamento Then Exit Sub
    If OutputType = eOutputType.eColorByKey Then ImpostaListaChiaviSoloSelected(Lista.ToList)

    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False

    For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If IsNumeric(LineaFS.Tag) Then
        LineaFS.IsVisible = True
      ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
        Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
        LineaFS.IsVisible = Periodo.IsChecked
      ElseIf TypeOf (LineaFS.Tag) Is String Then
        LineaFS.IsVisible = True
      End If
    Next
    'Dim c As Integer = 0
    For Each Controllo In ContenitoreTwsVsChannels.ListaControlli
      Dim TL As New clsTrendLines

      For Each linea In Controllo.Plots.RenderableSeries.ToArray
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If IsNumeric(LineaFS.Tag) Then
          LineaFS.IsVisible = True
        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          LineaFS.IsVisible = Periodo.IsChecked
          If LineaFS.IsVisible Then
            If Periodo.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg Then
              AtLeastOneUp = AtLeastOneUp OrElse Periodo.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30))
              AtLeastOneDn = AtLeastOneDn OrElse Periodo.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(160, 120))
              'ElseIf Periodo.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching Then
              '  AtLeastOneUp = AtLeastOneUp OrElse Periodo.StraightLineReachingDetails.IsTWAinRange(New clsDoubleRange(60, 30))
              '  AtLeastOneDn = AtLeastOneDn OrElse Periodo.StraightLineReachingDetails.IsTWAinRange(New clsDoubleRange(160, 120))
              TL.AccodaCoppia(Periodo.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30)), Periodo.IsStbd, LineaFS.DataSeries.XValues(0), LineaFS.DataSeries.YValues(0))
            Else
              'TL.AccodaCoppia(Periodo.StraightLineReachingDetails.IsTWAinRange(New clsDoubleRange(60, 30)), Periodo.IsStbd, LineaFS.DataSeries.XValues(0), LineaFS.DataSeries.YValues(0))
            End If
          End If

        End If
      Next
      If Not Controllo.Plots.SeriesSource Is Nothing Then TL.StampaTrendLines(4, Controllo.Plots.SeriesSource, Me)
      For Each linea In Controllo.Plots.RenderableSeries
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If TypeOf (LineaFS.Tag) Is String Then
          Select Case LineaFS.Tag
            Case "TgtUp"
              LineaFS.IsVisible = AtLeastOneUp
            Case "TgtDn"
              LineaFS.IsVisible = AtLeastOneDn
          End Select
        End If
      Next
    Next
    'c = 0
    For Each Controllo In ContenitoreDistributions.ListaControlli
      'If Controllo.Plots.RenderableSeries.Count = 0 Then
      '	Stop
      'End If
      For Each linea In Controllo.Plots.RenderableSeries
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries)
        If IsNumeric(LineaFS.Tag) Then
          LineaFS.IsVisible = True
        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          LineaFS.IsVisible = Periodo.IsChecked
          If Controllo Is ContenitoreDistributions.ListaControlli.First Then
            'Console.WriteLine(c & " Dst " & Periodo.TR.Start.ToString("HH:mm:ss") & " ")
            'c += 1
          End If
        ElseIf TypeOf (LineaFS.Tag) Is String Then
          LineaFS.IsVisible = True
        End If
      Next
    Next
    PeriodsManager.AggiornaDailyVmgPerc()
  End Sub

  Public Function Colore(Periodo As clsPeriod2021) As Color
    Return ColoreDaOutputType(Periodo, OutputType)
  End Function


  ''' <summary>Ultima selezione applicata, per poterla riapplicare ai grafici
  ''' disegnati in modo progressivo dopo che la selezione era gia' attiva.</summary>
  Private _UltimaSelezioneSerie As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
  Private _UltimaSelezionePunti As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo)

  ''' <summary>
  ''' Riapplica l'ultima selezione nota. Chiamata al termine del disegno progressivo,
  ''' perche' i grafici riempiti per ultimi nascono con i colori di default.
  ''' </summary>
  Public Sub RiapplicaSelezioneCorrente()
    If Not _UltimaSelezionePunti Is Nothing AndAlso _UltimaSelezionePunti.Count > 0 Then
      AggiornaControlli(_UltimaSelezionePunti)
    ElseIf Not _UltimaSelezioneSerie Is Nothing AndAlso _UltimaSelezioneSerie.Count > 0 Then
      AggiornaControlli(_UltimaSelezioneSerie)
    End If
  End Sub

  Public Sub AggiornaControlli(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    _UltimaSelezioneSerie = Selezione
    _UltimaSelezionePunti = Nothing
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow


    If Selezione.Count = 0 Then
      For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          LineaFS.Stroke = Colore(Periodo)
        End If
      Next
      For Each Controllo In ContenitoreTwsVsChannels.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Periodo.ColoreSfondo = SfondoBase
            LineaFS.PointMarker.Stroke = Colore(Periodo)
            LineaFS.PointMarker.Fill = Colore(Periodo)
          End If
        Next
      Next
      For Each Controllo In ContenitoreDistributions.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Periodo.ColoreSfondo = SfondoBase
            If Not LineaFS.PointMarker Is Nothing Then
              LineaFS.PointMarker.Stroke = Colore(Periodo)
              LineaFS.PointMarker.Fill = Colore(Periodo)
            End If
          End If
        Next
      Next

    Else
      For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
          LineaFS.Stroke = ColoreNonSelezionati
          For Each pp In listOk
            LineaFS.Stroke = Colore(Periodo)
          Next
        End If
      Next
      For Each Controllo In ContenitoreTwsVsChannels.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
            LineaFS.PointMarker.Stroke = ColoreNonSelezionati
            LineaFS.PointMarker.Fill = ColoreNonSelezionati
            Periodo.ColoreSfondo = ColoreNonSelezionati
            For Each pp In listOk
              Periodo.ColoreSfondo = SfondoSelezionati
              LineaFS.PointMarker.Stroke = Colore(Periodo)
              LineaFS.PointMarker.Fill = Colore(Periodo)
            Next
          End If
        Next
      Next
      For Each Controllo In ContenitoreDistributions.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
            Periodo.ColoreSfondo = ColoreNonSelezionati
            LineaFS.Stroke = ColoreNonSelezionati
            Dim ColoreFill As New SolidColorBrush(LineaFS.Stroke)
            ColoreFill.Opacity = 0.2
            LineaFS.Fill = ColoreFill
            For Each pp In listOk
              Periodo.ColoreSfondo = SfondoSelezionati
              LineaFS.Stroke = Colore(Periodo)
              ColoreFill.Color = LineaFS.Stroke
              LineaFS.Fill = ColoreFill
            Next
          End If
        Next
      Next
    End If
  End Sub


  Public Sub AggiornaControlli(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    _UltimaSelezionePunti = Selezione
    _UltimaSelezioneSerie = Nothing
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow



    If Selezione.Count = 0 Then
      For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If Not IsNumeric(LineaFS.Tag) Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          LineaFS.Stroke = Colore(Periodo)
        End If
      Next
      For Each Controllo In ContenitoreTwsVsChannels.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Periodo.ColoreSfondo = SfondoBase
            LineaFS.PointMarker.Stroke = Colore(Periodo)
            LineaFS.PointMarker.Fill = Colore(Periodo)
          ElseIf TypeOf (LineaFS.Tag) Is String Then
            LineaFS.Visibility = Visibility.Visible
          End If
        Next
      Next
      For Each Controllo In ContenitoreDistributions.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Periodo.ColoreSfondo = SfondoBase
            LineaFS.Stroke = Colore(Periodo)
            Dim ColoreFill As New SolidColorBrush(LineaFS.Stroke)
            ColoreFill.Opacity = 0.2
            LineaFS.Fill = ColoreFill
          End If
        Next
      Next

    Else
      For Each linea In StraightLineTrackXyPerMinute.VmgXySeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
          LineaFS.Stroke = ColoreNonSelezionati
          Periodo.ColoreSfondo = ColoreNonSelezionati
          For Each pp In listOk
            Periodo.ColoreSfondo = SfondoSelezionati
            LineaFS.Stroke = Colore(Periodo)
          Next
        End If
      Next
      For Each Controllo In ContenitoreTwsVsChannels.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
            LineaFS.PointMarker.Stroke = ColoreNonSelezionati
            LineaFS.PointMarker.Fill = ColoreNonSelezionati
            Periodo.ColoreSfondo = ColoreNonSelezionati
            For Each pp In listOk
              Periodo.ColoreSfondo = SfondoSelezionati
              LineaFS.PointMarker.Stroke = Colore(Periodo)
              LineaFS.PointMarker.Fill = Colore(Periodo)
            Next
          ElseIf TypeOf (LineaFS.Tag) Is String Then
            LineaFS.Visibility = Visibility.Visible
          End If

        Next
      Next
      For Each Controllo In ContenitoreDistributions.ListaControlli
        For Each linea In Controllo.Plots.RenderableSeries
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseMountainRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
            Periodo.ColoreSfondo = ColoreNonSelezionati
            LineaFS.Stroke = ColoreNonSelezionati
            Dim ColoreFill As New SolidColorBrush(LineaFS.Stroke)
            ColoreFill.Opacity = 0.2
            LineaFS.Fill = ColoreFill
            For Each pp In listOk
              Periodo.ColoreSfondo = SfondoSelezionati
              LineaFS.Stroke = Colore(Periodo)
              ColoreFill.Color = LineaFS.Stroke
              LineaFS.Fill = ColoreFill
            Next

          End If

        Next
      Next

    End If
  End Sub

  Private Sub StraightLinesSelectionSettings_SettingChanged() Handles StraightLinesSelectionSettings.SettingChanged
    VerificaSelectedStraightLines()
    VerificaAggiornaGrafici()
  End Sub

  Public Sub VerificaSelectedStraightLines()
    StraightLinesSelectionSettings.VerificaStraightLines(Lista)
  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineTrackXyPerMinute

  Public Property VmgXySeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property TitoloPlot As String = "XY components"
  Public Property CombinazioneCanali As eCombinazioneCanali = eCombinazioneCanali.eBsTwa
  Public Property StraightLineChartSync As New clsStraightLineChartSync
  Public Property ParentVM As clsStraightLineVM2020


  Public Sub New(StraightLineChartSync As clsStraightLineChartSync, ParentVM As clsStraightLineVM2020)
    Me.StraightLineChartSync = StraightLineChartSync
    Me.ParentVM = ParentVM
  End Sub

  Public Enum eCombinazioneCanali
    eSogCogGwd = 0
    eSogCogVmcRef = 1
    eBsTwa = 2
    eBSCrsVmcRef = 3
  End Enum

  Public Sub AggiornaGraficoTrackXyPerMinute(VmcReference As Double, ShowLatLong As Boolean, MetriAlMinuto As Boolean, ColorByTack As Boolean, OutputType As clsStraightLineVM2020.eOutputType, ListaPeriodi As List(Of clsPeriod2021))
    'Grafico con Traccia XY in metri minuto

    'VmcReference Rotta Verso la quale si vuole verificare il VMC
    VmgXySeriesSource.Clear()
    'Dim pColori As List(Of Color) = ScalaColoriScuri(StraightLines.Count)

    Dim chLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    If chLat Is Nothing OrElse chLat.Valori.Count = 0 Then Exit Sub

    For Each StraightLine In ListaPeriodi

      'If System.Math.Abs(PeriodsManager.ListaStraightLineVmg(i).TR.Start.Subtract(New DateTime(2020, 8, 24, 9, 51, 15)).TotalSeconds) < 1 Then
      '	Stop
      'End If

      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.Append(0, 0, New clsPuntoMetadata(False))
      DataSeriesTMP.AcceptsUnsortedData = True
      Dim DataSeriesLatLong As New XyDataSeries(Of Double, Double)
      DataSeriesLatLong.AcceptsUnsortedData = True
      Dim LineaTmp As New FastLineRenderableSeries
      Dim LineaLatLong As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.Stroke = ColoreDaOutputType(StraightLine, OutputType)
      If StraightLine.IsStbd Then
        LineaTmp.StrokeDashArray = {3, 3}
      End If
      LineaTmp.StrokeThickness = 3
      LineaTmp.Tag = StraightLine

      LineaLatLong.XAxisId = "DefaultAxisId"
      LineaLatLong.YAxisId = "DefaultAxisId"
      LineaLatLong.Stroke = ColoreDaOutputType(StraightLine, OutputType) ' StraightLine.Colore
      LineaLatLong.StrokeThickness = 1
      LineaLatLong.StrokeDashArray = {3, 3}

      Dim X As Double = 0
      Dim Y As Double = 0
      'Dim SecXrow As Double = 1 / DataProvider2020.RawFileHz 'StraightLine.Periodo.TR.Durata.TotalSeconds / System.Math.Abs(StraightLine.IDultimaRiga - StraightLine.IDprimaRiga + 1)
      Dim KmetriMinuto As Double = 1
      If MetriAlMinuto Then
        KmetriMinuto = 1 / StraightLine.TR.Durata.TotalMinutes
        TitoloPlot = "XY Mt per Min"
      Else
        TitoloPlot = "XY Meters"
      End If
      Dim Xy As Double = 0
      Dim Yy As Double = 0
      Dim LatRef As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(StraightLine.TR.IdRigaIniziale)
      Dim LngRef As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(StraightLine.TR.IdRigaIniziale)
      Dim PuntoRef As New clsGeographicPosition(LatRef, LngRef, Not (LatRef = Nothing OrElse LngRef = Nothing))
      Dim CG As New clsCalcoliDuePuntiGeo()

      'Dim Moltiplicatore As Integer = 1

      'For Riga As Integer = StraightLine.IDprimaRiga To StraightLine.IDultimaRiga
      Dim IndiceIniziale As Integer = DataProvider2020.TrovaIndice(StraightLine.TR.Start)
      Dim IndiceFinale As Integer = DataProvider2020.TrovaIndice(StraightLine.TR.Finish)
      Dim MomentoPrev As DateTime = DataProvider2020.Momento(IndiceIniziale)
      For Riga = IndiceIniziale To IndiceFinale
        Dim Momento As DateTime = DataProvider2020.Momento(Riga)
        Dim BS As Double = 0
        Dim TWA As Double = 0
        Dim TWD As Double = 0
        Select Case CombinazioneCanali
          Case eCombinazioneCanali.eSogCogGwd
            BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
            Dim Cog As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG).Valori(Riga)
            Dim Gwd As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD).Valori(Riga)
            TWA = DifferenzaAssolutaTraAngoli360(Cog, Gwd, True)
            TWD = Gwd
          Case eCombinazioneCanali.eSogCogVmcRef
            BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
            Dim Cog As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG).Valori(Riga)
            TWA = DifferenzaAssolutaTraAngoli360(Cog, VmcReference, True)
            TWD = VmcReference
          Case eCombinazioneCanali.eBSCrsVmcRef
            BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
            Dim Cse As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE).Valori(Riga)
            TWA = DifferenzaAssolutaTraAngoli360(Cse, VmcReference, True)
            TWD = VmcReference
          Case Else
            BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori(Riga)
            TWA = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(Riga)
            TWD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD).Valori(Riga)
        End Select
        If NoneIsNan(BS, TWA, TWD) Then
          Dim DeltaSec As Double = Momento.Subtract(MomentoPrev).TotalSeconds
          MomentoPrev = Momento
          'If (Double.IsNaN(BS)) Then
          '  Moltiplicatore += 1
          'Else
          BS = KtsToMS(BS)
          'BS *= SecXrow * KmetriMinuto * Moltiplicatore
          BS *= DeltaSec * KmetriMinuto
          X += BS * System.Math.Abs(System.Math.Sin(Radians(TWA)))
          Y += BS * System.Math.Abs(System.Math.Cos(Radians(TWA)))
          DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
          'Moltiplicatore = 1

          If ShowLatLong Then
            Dim Lat As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(Riga)
            Dim Lng As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(Riga)
            Dim GeoPoint As New clsGeographicPosition(Lat, Lng, Not (Lat = Nothing OrElse Lng = Nothing))
            CG.CalcolaDistanzaAndRotta(PuntoRef, GeoPoint)
            Dim RNG As Double = CG.DistanzaMetri
            Dim BRG As Double = CG.Rotta
            Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TWD, BRG)
            Dim BRGc As Double = BRGinCartesiano(BRGoc)
            Dim Xx As Double = RNG * System.Math.Cos(Radians(BRGc)) 'Long
            Dim Yx As Double = RNG * System.Math.Sin(Radians(BRGc)) 'Lat
            DataSeriesLatLong.Append(Yx, Xx)
          End If
          'End If
        End If
      Next

      LineaTmp.DataSeries = DataSeriesTMP
      If ShowLatLong Then LineaLatLong.DataSeries = DataSeriesLatLong
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      Dim CSVMlatlong As New ChartSeriesViewModel(DataSeriesLatLong, LineaLatLong)
      VmgXySeriesSource.Add(CSVMtmp)
      If ShowLatLong Then VmgXySeriesSource.Add(CSVMlatlong)
    Next
  End Sub

  Public Sub AggiornaGraficoTrackXyPerMinuteGroupByTack(VmcReference As Double, ShowLatLong As Boolean, MetriAlMinuto As Boolean, ListaPeriodi As List(Of clsPeriod2021))
    'Grafico con Traccia XY in metri minuto
    'VmcReference Rotta Verso la quale si vuole verificare il VMC

    VmgXySeriesSource.Clear()

    Dim TotalPortMinutes As Double = 0
    Dim TotalStbdMinutes As Double = 0
    For Each StraightLine In ListaPeriodi
      'Dim StraightLine As clsPeriod2021 = PeriodsManager.CollectionStraightLineVmg(i)
      If StraightLine.IsChecked Then
        If StraightLine.IsStbd Then
          TotalStbdMinutes += StraightLine.TR.Durata.TotalMinutes
        Else
          TotalPortMinutes += StraightLine.TR.Durata.TotalMinutes
        End If
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    DataSeriesPort.AcceptsUnsortedData = True
    DataSeriesStbd.AcceptsUnsortedData = True
    'Dim X As Double = 0
    'Dim Y As Double = 0
    Dim Xport As Double = 0
    Dim Yport As Double = 0
    Dim Xstbd As Double = 0
    Dim Ystbd As Double = 0
    For Each StraightLine In ListaPeriodi
      If StraightLine.IsChecked Then
        Dim KmetriMinuto As Double = 1
        If MetriAlMinuto Then
          If StraightLine.IsStbd Then
            KmetriMinuto = 1 / TotalStbdMinutes
          Else
            KmetriMinuto = 1 / TotalPortMinutes
          End If
          TitoloPlot = "XY Mt per Min"
        Else
          TitoloPlot = "XY Meters"
        End If
        Dim Xy As Double = 0
        Dim Yy As Double = 0
        Dim LatRef As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(StraightLine.TR.IdRigaIniziale)
        Dim LngRef As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(StraightLine.TR.IdRigaIniziale)
        Dim PuntoRef As New clsGeographicPosition(LatRef, LngRef, Not (LatRef = Nothing OrElse LngRef = Nothing))
        Dim CG As New clsCalcoliDuePuntiGeo()
        Dim IndiceIniziale As Integer = DataProvider2020.TrovaIndice(StraightLine.TR.Start)
        Dim IndiceFinale As Integer = DataProvider2020.TrovaIndice(StraightLine.TR.Finish)
        Dim MomentoPrev As DateTime = DataProvider2020.Momento(IndiceIniziale)
        For Riga = IndiceIniziale To IndiceFinale
          Dim Momento As DateTime = DataProvider2020.Momento(Riga)
          Dim BS As Double = 0
          Dim TWA As Double = 0
          Dim TWD As Double = 0
          Select Case CombinazioneCanali
            Case eCombinazioneCanali.eSogCogGwd
              BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
              Dim Cog As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG).Valori(Riga)
              Dim Gwd As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD).Valori(Riga)
              TWA = DifferenzaAssolutaTraAngoli360(Cog, Gwd, True)
              TWD = Gwd
            Case eCombinazioneCanali.eSogCogVmcRef
              BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
              Dim Cog As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG).Valori(Riga)
              TWA = DifferenzaAssolutaTraAngoli360(Cog, VmcReference, True)
              TWD = VmcReference
            Case eCombinazioneCanali.eBSCrsVmcRef
              BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori(Riga)
              Dim Cse As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE).Valori(Riga)
              TWA = DifferenzaAssolutaTraAngoli360(Cse, VmcReference, True)
              TWD = VmcReference
            Case Else
              BS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori(Riga)
              TWA = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(Riga)
              TWD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD).Valori(Riga)
          End Select
          If NoneIsNan(BS, TWA, TWD) Then
            Dim DeltaSec As Double = Momento.Subtract(MomentoPrev).TotalSeconds
            MomentoPrev = Momento
            BS = KtsToMS(BS)
            BS *= DeltaSec * KmetriMinuto
            If StraightLine.IsStbd Then
              Xstbd += BS * System.Math.Abs(System.Math.Sin(Radians(TWA)))
              Ystbd += BS * System.Math.Abs(System.Math.Cos(Radians(TWA)))
              DataSeriesStbd.Append(Xstbd, Ystbd, New clsPuntoMetadata(False))
            Else
              Xport += BS * System.Math.Abs(System.Math.Sin(Radians(TWA)))
              Yport += BS * System.Math.Abs(System.Math.Cos(Radians(TWA)))
              DataSeriesPort.Append(Xport, Yport, New clsPuntoMetadata(False))
            End If
          End If
        Next
      End If
    Next

    Dim LineaPort As New FastLineRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.Stroke = Colors.Red
    LineaPort.StrokeThickness = 2
    LineaPort.Tag = -1
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    VmgXySeriesSource.Add(CSVMport)

    Dim LineaStbd As New FastLineRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.Stroke = Colors.Green
    LineaStbd.StrokeThickness = 2
    LineaStbd.Tag = 1
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    VmgXySeriesSource.Add(CSVMStbd)

  End Sub

  Private Function NoneIsNan(Bs As Double, Twa As Double, Twd As Double) As Boolean
    If Double.IsNaN(Bs) Then Return False
    If Double.IsNaN(Bs) Then Return False
    If Double.IsNaN(Bs) Then Return False
    Return True
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineControlliPeriodChannelsDistributions

  Public Property ContenitoriPeriodiChannelsDistributions As New ObservableCollection(Of UserControlStraightLinePlotContainer)
  Public Property StraightLineChartSync As New clsStraightLineChartSync
  Public Property ParentVM As clsStraightLineVM2020

  Public Sub New(StraightLineChartSync As clsStraightLineChartSync, ParentVM As clsStraightLineVM2020)
    Me.StraightLineChartSync = StraightLineChartSync
    Me.ParentVM = ParentVM
  End Sub


  Public Property CanaliDaStampare As ObservableCollection(Of clsChannel2020)
    Get
      Return StraightLineChartSync.CanaliDaStampare
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      StraightLineChartSync.CanaliDaStampare = value
    End Set
  End Property

  Public ReadOnly Property ListaPeriodi As List(Of clsPeriod2021)
    Get
      Return ParentVM.Lista.ToList
      'Return PeriodsManager.ListaStraightLineVmg
    End Get
  End Property

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineContainerViewModel


  Public Property ListaControlli As New ObservableCollection(Of UserControlStraightLineStandardPlot)
  Public Property IsDistributions As Boolean
  Public Property StraightLineChartSync As New clsStraightLineChartSync
  Public Property Periodo As clsPeriod2021

  Public Property DataBoxHeight As Double

  Public Property ParentVM As clsStraightLineVM2020


  Public Sub New(IsDistributions As Boolean, StraightLineChartSync As clsStraightLineChartSync, Periodo As clsPeriod2021, ParentVM As clsStraightLineVM2020)
    Me.IsDistributions = IsDistributions
    Me.StraightLineChartSync = StraightLineChartSync
    Me.Periodo = Periodo
    Me.ParentVM = ParentVM
  End Sub

#Region "Disegno progressivo"

  ' Disegnare tutti i grafici in un colpo solo costa ~150 ms ciascuno: con 39 canali
  ' sono quasi 6 secondi di blocco della UI, mentre a schermo se ne vedono 3 o 4.
  ' Qui il disegno avviene un grafico alla volta, a priorita' Background: la tab si apre
  ' subito e i grafici compaiono da sinistra a destra, cioe' nell'ordine in cui li si vede.
  ' Nulla cambia nella struttura delle serie, quindi Tag, IsVisible e la selezione
  ' sincronizzata continuano a funzionare esattamente come prima.

  Private _TokenDisegno As Integer = 0

  Public ReadOnly Property DisegnoInCorso As Boolean
    Get
      Return _DisegnoInCorso
    End Get
  End Property
  Private _DisegnoInCorso As Boolean = False

  ''' <summary>Interrompe un eventuale disegno progressivo ancora in coda.</summary>
  Public Sub AnnullaDisegno()
    _TokenDisegno += 1
    _DisegnoInCorso = False
  End Sub

  ''' <summary>
  ''' Esegue Disegna su ogni controllo, uno per ciclo del dispatcher.
  ''' </summary>
  Private Sub DisegnaProgressivo(Disegna As Action(Of UserControlStraightLineStandardPlot))
    AnnullaDisegno()
    Dim MioToken As Integer = _TokenDisegno
    _DisegnoInCorso = True

    Dim Indice As Integer = 0
    Dim Dsp = System.Windows.Application.Current.Dispatcher

    Dim Passo As Action = Nothing
    Passo = Sub()
              ' un nuovo disegno ha invalidato questo: si abbandona
              If Not MioToken = _TokenDisegno Then Return
              If Indice >= ListaControlli.Count Then
                _DisegnoInCorso = False
                ' i grafici disegnati per ultimi devono ricevere l'eventuale
                ' selezione attiva, che e' cambiata mentre venivano riempiti
                Try
                  If Not ParentVM Is Nothing Then ParentVM.RiapplicaSelezioneCorrente()
                Catch ex As Exception
                End Try
                Return
              End If
              Dim Controllo = ListaControlli(Indice)
              Indice += 1
              Try
                Disegna(Controllo)
              Catch ex As Exception
                Console.WriteLine("Disegno grafico fallito: " & ex.Message)
              End Try
              Dsp.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, Passo)
            End Sub

    Dsp.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, Passo)
  End Sub

#End Region


  Public ReadOnly Property SplitterEnabled As Boolean
    Get
      Return Periodo Is Nothing
    End Get
  End Property

  Public Property CanaliDaStampare As ObservableCollection(Of clsChannel2020)
    Get
      Return StraightLineChartSync.CanaliDaStampare
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      StraightLineChartSync.CanaliDaStampare = value
    End Set
  End Property

  Public ReadOnly Property ListaPeriodi As List(Of clsPeriod2021)
    Get
      Return ParentVM.Lista.ToList
    End Get
  End Property

  Private Sub ImpostaControlli()
    Dim TmrIC As DateTime = Now
    ListaControlli.Clear()
    Dim CanaleTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.xy")
    Dim Contatore As Integer = 0
    If IsDistributions Then
      For Each CanaleAscissa In CanaliDaStampare
        If CanaleAscissa.StraightLineChartSync Is Nothing Then CanaleAscissa.StraightLineChartSync = New clsStraightLineChartSync
        CanaleAscissa.StraightLineChartSync.ZoomPanEnabled = True
        CanaleAscissa.StraightLineChartSync.DataPointSelectionEnabled = False
        CanaleAscissa.StraightLineChartSync.SeriesSelectionEnabled = True
        Dim ctrlTmp As New UserControlStraightLineStandardPlot(CanaleAscissa.StraightLineChartSync)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.x1." & Contatore)
        'Dim ctrlTmpVm As New clsStraightLineStandardPlotViewModelDistributions(CanaleAscissa, Nothing, CanaleAscissa.ShortName, CanaleAscissa.StraightLineChartSync, ParentVM)
        Dim Titolo As String = CanaleAscissa.ShortName
        If CanaleAscissa.ChannelId.IndexOf("Port") > -1 Then
          ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
          Dim CanaleOpposite As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
          If Not CanaleOpposite Is Nothing Then Titolo = Titolo.Replace("Port", "Lwd")
        ElseIf CanaleAscissa.ChannelId.IndexOf("Stbd") > -1 Then
          ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
          Dim CanaleOpposite As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
          If Not CanaleOpposite Is Nothing Then Titolo = Titolo.Replace("Stbd", "Wwd")
        End If

        Dim ctrlTmpVm As New clsStraightLineStandardPlotViewModelDistributions(CanaleAscissa, Nothing, Titolo, Nothing, ParentVM)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.x2." & Contatore)
        ctrlTmp.DataContext = ctrlTmpVm
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.x3." & Contatore)
        ListaControlli.Add(ctrlTmp)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.x4." & Contatore)
        Contatore += 1
      Next
    Else
      For Each CanaleOrdinata In CanaliDaStampare
        Dim ctrlTmp As New UserControlStraightLineStandardPlot(StraightLineChartSync)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.y1." & Contatore)

        Dim Titolo As String = ""
        If CanaleOrdinata Is Nothing Then
          Titolo = CanaleTws.ShortName
        Else
          Titolo = CanaleOrdinata.ShortName
          If CanaleOrdinata.ChannelId.IndexOf("Port") > -1 Then
            ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
            Dim CanaleOpposite As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
            If Not CanaleOpposite Is Nothing Then Titolo = Titolo.Replace("Port", "Lwd")
          ElseIf CanaleOrdinata.ChannelId.IndexOf("Stbd") > -1 Then
            ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
            Dim CanaleOpposite As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
            If Not CanaleOpposite Is Nothing Then Titolo = Titolo.Replace("Stbd", "Wwd")
          End If
        End If

        Dim ctrlTmpVm As New clsStraightLineStandardPlotViewModelXY(CanaleTws, CanaleOrdinata, Titolo, StraightLineChartSync, ParentVM)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.y2." & Contatore)
        ctrlTmp.DataContext = ctrlTmpVm
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.y3." & Contatore)
        ListaControlli.Add(ctrlTmp)
        ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a.y4." & Contatore)
        Contatore += 1
      Next
    End If
  End Sub

  Public Sub AggiornaGrafici(OutputType As clsStraightLineVM2020.eOutputType)
    Dim TmrC As DateTime = Now
    If AggiornaCanali() Then ImpostaControlli()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   CT.1 - ImpostaControlli (" & If(IsDistributions, "Distr", "TwsVsCh") & ") " & Now.Subtract(TmrC).TotalMilliseconds.ToString("F0") & " ms - controlli: " & ListaControlli.Count)
    Dim Periodi = ListaPeriodi
    DisegnaProgressivo(Sub(Controllo)
                         If IsDistributions Then
                           Controllo.ViewModel.DrawChart(Periodi)
                         Else
                           Controllo.ViewModel.DrawChart(Periodi, OutputType)
                         End If
                       End Sub)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   CT.2 - disegno progressivo avviato x" & ListaControlli.Count & " (" & If(IsDistributions, "Distr", "TwsVsCh") & ")")

  End Sub

  Public Sub AggiornaGraficiGroupByTack(UpDn As Boolean, Tack As Boolean)
    Dim TmrG As DateTime = Now
    If AggiornaCanali() Then ImpostaControlli()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   GT.1 - ImpostaControlli (" & If(IsDistributions, "Distr", "TwsVsCh") & ") " & Now.Subtract(TmrG).TotalMilliseconds.ToString("F0") & " ms - controlli: " & ListaControlli.Count)
    Dim PeriodiGT = ListaPeriodi
    DisegnaProgressivo(Sub(Controllo)
                         Controllo.ViewModel.DrawChartGroupByTack(PeriodiGT, UpDn, Tack)
                       End Sub)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   GT.2 - disegno progressivo avviato x" & ListaControlli.Count & " (" & If(IsDistributions, "Distr", "TwsVsCh") & ")")

  End Sub

  Public Sub AggiornaGraficiGroupByKeys()
    'Stop
    If AggiornaCanali() Then ImpostaControlli()
    ' ObjContaTempo.StampaMillisecondiTrascorsi("SL2020.iii.a")
    Dim PeriodiGK = ListaPeriodi
    DisegnaProgressivo(Sub(Controllo)
                         Controllo.ViewModel.DrawChartGroupByKeys(PeriodiGK)
                       End Sub)

  End Sub

  Private Function AggiornaCanali() As Boolean
    If Not CanaliDaStampare.Count = ListaControlli.Count Then Return True
    For i As Integer = 0 To ListaControlli.Count - 1
      If Not ListaControlli(i).ViewModel.CanaleOrdinata Is CanaliDaStampare(i) Then
        Return True
      End If
    Next
    Return False
  End Function

End Class