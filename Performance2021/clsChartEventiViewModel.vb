Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged
Imports SciChart.Charting.Model
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Annotations
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class clsChartEventiViewModel
  Public Property Annotazioni As New SciChart.Charting.Visuals.Annotations.AnnotationCollection

  Public Property CurrentPositionMouseDown As DateTime
  Public Property StringaPosizione As String

  Public Property CampoTWA As clsChannel2020
  Public Property CanaliValori As List(Of clsChannel2020)
  Public Property CampoValori As clsChannel2020

  Public Property SharedTR As clsTimeRange
  Public Property BloccaEventi As Boolean = False


  Public Property Yassi As AxisCollection
  Public Property Xassi As AxisCollection

  Public Property _ShowCursorValues As Boolean = True


  Public Property ShowPositionLabel As Visibility = Visibility.Visible

  Public Property RubberBandEnabled As Boolean = True


  Public Enum eGruppoAnnotazione
    eSfondo = 0
    eBarraAnte = 1
    eBarraPost = 2
    eSelezioneAnte = 3
    eSelezionePost = 4
    ePeriods = 5
    eValidRows = 6
    eNothing = 7
  End Enum

  Public Property ShowCursorValues As Boolean
    Get
      Return _ShowCursorValues
    End Get
    Set(value As Boolean)
      _ShowCursorValues = value
      If _ShowCursorValues Then
        ShowPositionLabel = Visibility.Visible
      Else
        ShowPositionLabel = Visibility.Collapsed
      End If
    End Set
  End Property

  Dim _CurrentPosition As Date
  Public Property CurrentPosition As Date
    Get
      Return _CurrentPosition
    End Get
    Set(value As Date)
      _CurrentPosition = value
      AggiornaStringaPosizione()
    End Set
  End Property

  Private Sub AggiornaStringaPosizione()
    If CampoTWA Is Nothing Then Exit Sub
    Dim Riga As Integer = DataProvider2020.TrovaIndice(CurrentPosition)
    Riga = System.Math.Min(Riga, CampoTWA.Valori.Count - 1)
    Riga = System.Math.Max(Riga, 0)
    Dim strTmp As String = ""
    If DataProvider2020.TimeRange.Durata.TotalDays > 0 Then
      strTmp = CurrentPosition.ToShortDateString & " "
    End If
    strTmp &= CurrentPosition.ToLongTimeString
    strTmp &= " " & CampoTWA.ShortName & ": " & Format(CampoTWA.Valori(Riga), "F" & CampoTWA.Decimals.ToString)
    For Each campo In CanaliValori
      strTmp &= " " & campo.ShortName & ": " & Format(campo.Valori(Riga), "F" & campo.Decimals.ToString)
    Next
    StringaPosizione = strTmp
  End Sub


  Public Sub SetCurrentMousePosition(IsMovingVisibleRangeLine As Boolean)
    If IsMovingVisibleRangeLine Then
      ' CurrentPositionMouseDown diventa la posizione dell altra linea
      If Math.Abs(CurrentPosition.Subtract(SharedTR.Start).TotalSeconds) < Math.Abs(CurrentPosition.Subtract(SharedTR.Finish).TotalSeconds) Then
        CurrentPositionMouseDown = SharedTR.Finish
      Else
        CurrentPositionMouseDown = SharedTR.Start
      End If
    Else
      CurrentPositionMouseDown = CurrentPosition
    End If
  End Sub

  Public Function MousePositionChanged() As Boolean
    Return System.Math.Abs(CurrentPositionMouseDown.Subtract(CurrentPosition).TotalSeconds) > 1
  End Function

  Public ReadOnly Property VisibleX As clsTimeRange
    Get
      If Xassi Is Nothing Then Return Nothing
      Return New clsTimeRange(DirectCast(Xassi.First.VisibleRange.Min, DateTime), DirectCast(Xassi.First.VisibleRange.Max, DateTime))
    End Get
  End Property

  Public Property Inizio As DateTime
    Get
      Return SharedTR.Start
    End Get
    Set(value As DateTime)
      If SharedTR.Start = value Then Exit Property
      If value > SharedTR.Finish Then
        SharedTR.Finish = value
      Else
        SharedTR.Start = value
      End If
      'If Not BloccaEventi Then RaiseEvent SelectionChanged(SharedTR)
    End Set
  End Property

  Public Property Fine As DateTime
    Get
      Return SharedTR.Finish
    End Get
    Set(value As DateTime)
      If SharedTR.Finish = value Then Exit Property
      If value < SharedTR.Start Then
        SharedTR.Start = value
      Else
        SharedTR.Finish = value
      End If
    End Set
  End Property

  Public ReadOnly Property SelectedTimeRange As clsTimeRange
    Get
      Return SharedTR
    End Get
  End Property


  Public Sub AggiornaSelezione(Selezione As clsTimeRange)
    BloccaEventi = True
    SharedTR = Selezione
    For Each Annotazione In Annotazioni
      If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
        Dim AB As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
        If Not AB.Tag Is Nothing Then
          If DirectCast(AB.Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSelezioneAnte Then
            AB.X2 = SharedTR.Start
          ElseIf DirectCast(AB.Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSelezionePost Then
            AB.X1 = SharedTR.Finish
          End If
        End If
      ElseIf TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation Then
        Dim AB As SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation)
        If Not AB.Tag Is Nothing Then
          If DirectCast(AB.Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eBarraAnte Then
            AB.X1 = SharedTR.Start
          ElseIf DirectCast(AB.Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eBarraPost Then
            AB.X1 = SharedTR.Finish
          End If
        End If
      End If
    Next
    If Not Mouse.LeftButton = MouseButtonState.Pressed Then
      AggiornaYrange()
    End If
    BloccaEventi = False
  End Sub

  Public Sub AggiornaYrange()
    If MatriceControlliBase Is Nothing Then Exit Sub
    For Each Vm In MatriceControlliBase.MatriceControlli
      ' con la virtualizzazione i plot fuori dalla viewport non hanno una superficie
      If Vm.Surface Is Nothing Then Continue For
      If Vm.Surface.YAxes Is Nothing Then Continue For
      If Vm.Surface.YAxes.Count = 0 Then Continue For
      If Vm.Surface.YAxes.First Is Nothing Then Continue For
      Vm.Surface.ZoomExtentsY()
    Next
  End Sub

  Public Sub AggiungiAnnotazioniScorrimento()
    Dim ds As Integer = 0
    Dim dsmin As Integer = 5
    If SharedTR.Finish.Subtract(SharedTR.Start).TotalSeconds < dsmin Then ds = dsmin
    Dim Annotazione As New SciChart.Charting.Visuals.Annotations.BoxAnnotation
    Annotazione.X1 = SharedTR.Start.AddSeconds(-ds)
    Annotazione.X2 = Inizio
    Annotazione.Y1 = 0
    Annotazione.Y2 = 1
    Annotazione.Background = New SolidColorBrush(Color.FromArgb(150, 128, 128, 128))
    Annotazione.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    Annotazione.Tag = eGruppoAnnotazione.eSelezioneAnte
    Annotazione.IsHidden = False
    Annotazione.IsEditable = False
    Annotazioni.Add(Annotazione)

    Dim An As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    An.X1 = Inizio
    An.Stroke = New SolidColorBrush(Colors.Yellow)
    An.StrokeThickness = 3
    An.Tag = eGruppoAnnotazione.eBarraAnte
    An.IsHidden = False
    An.IsEditable = True
    Annotazioni.Add(An)

    Dim VBR As New Binding("Inizio")
    VBR.Source = Me
    VBR.Mode = BindingMode.TwoWay
    Annotazione.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)
    An.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)



    Annotazione = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
    Annotazione.X1 = Fine
    Annotazione.X2 = SharedTR.Finish.AddSeconds(ds)
    Annotazione.Y1 = 0
    Annotazione.Y2 = 1
    Annotazione.Background = New SolidColorBrush(Color.FromArgb(150, 128, 128, 128))
    Annotazione.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    Annotazione.Tag = eGruppoAnnotazione.eSelezionePost
    Annotazione.IsHidden = False
    Annotazione.IsEditable = False
    Annotazioni.Add(Annotazione)

    An = New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    An.X1 = Fine
    An.Stroke = New SolidColorBrush(Colors.Yellow)
    An.StrokeThickness = 3
    An.Tag = eGruppoAnnotazione.eBarraPost
    An.IsHidden = False
    An.IsEditable = True
    Annotazioni.Add(An)

    VBR = New Binding("Fine")
    VBR.Source = Me
    VBR.Mode = BindingMode.TwoWay
    Annotazione.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)
    An.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

    An = New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    An.X1 = CurrentPosition
    An.Stroke = New SolidColorBrush(Colors.OrangeRed)
    An.StrokeThickness = 5
    An.Tag = eGruppoAnnotazione.eNothing
    An.IsHidden = False
    An.IsEditable = False
    An.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(An)

    VBR = New Binding("CurrentPosition")
    VBR.Source = Me
    VBR.Mode = BindingMode.TwoWay
    An.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

  End Sub


  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)

  ''' <summary>Svuota il grafico eventi: usato al cambio di dataset.</summary>
  Public Sub Azzera()
    Try
      SeriesSource.Clear()
      Annotazioni.Clear()
      Yassi = New AxisCollection()
      Xassi = New AxisCollection()
    Catch ex As Exception
    End Try
  End Sub

  Public Sub DrawTimePlotEventi(ArrayTimeStamps As DateTime(), CanaleTWA As clsChannel2020, CanaliValore As List(Of clsChannel2020), Intervallo As clsTimeRange, Periodi As ObservableCollection(Of clsPeriod2021))
    Yassi = New AxisCollection()
    Xassi = New AxisCollection()
    SeriesSource.Clear()
    Annotazioni.Clear()
    SharedTR = Intervallo

    CampoTWA = CanaleTWA
    CanaliValori = CanaliValore


    AggiungiAnnotazioniScorrimento()


    Dim Xasse As New SciChart.Charting.Visuals.Axes.DateTimeAxis
    Xasse.AxisTitle = "Time"
    Xasse.Id = "DefaultAxisId"
    Xasse.SubDayTextFormatting = "HH:mm:ss.fF"
    Xasse.Visibility = Visibility.Visible
    Xasse.AxisAlignment = SciChart.Charting.Visuals.Axes.AxisAlignment.Top
    Xasse.DrawMajorGridLines = False
    Xasse.DrawMinorGridLines = False
    Xasse.AxisBandsFill = Nothing
    Xasse.DrawMajorBands = False
    Xasse.AutoRange = SciChart.Charting.Visuals.Axes.AutoRange.Once
    Xasse.VisibleRangeLimit = New DateRange(DataProvider2020.TimeRange.Start, DataProvider2020.TimeRange.Finish)
    Xassi.Add(Xasse)

    Dim Yasse As New SciChart.Charting.Visuals.Axes.NumericAxis
    Yasse.AxisTitle = "TWA"
    Yasse.Id = "DefaultAxisId"
    Yasse.Visibility = Visibility.Visible
    Yasse.GrowBy = New DoubleRange(0, 0.3)
    Yasse.DrawMajorGridLines = False
    Yasse.DrawMinorGridLines = False
    Yasse.AxisBandsFill = Nothing
    Yasse.DrawMajorBands = False
    Yasse.LabelProvider = New clsLabelProvider(CanaleTWA.Decimals)
    ' GetType() sul tipo, non su un'istanza: un AxisTitle creato fuori da un asse
    ' fa fallire i binding del suo template (System.Windows.Data Error: 4)
    Dim TitleLabelStyle As New Style(GetType(AxisTitle))
    Dim sc As New Setter(Label.ForegroundProperty, New SolidColorBrush(Colors.Black))
    TitleLabelStyle.Setters.Add(sc)
    Yasse.TitleStyle = TitleLabelStyle

    Yassi.Add(Yasse)

    Dim DataSeriesPort As New XyDataSeries(Of DateTime, Double)
    Dim DataSeriesStbd As New XyDataSeries(Of DateTime, Double)
    DataSeriesPort.SeriesName = "Port"
    DataSeriesPort.AcceptsUnsortedData = False
    DataSeriesStbd.SeriesName = "Stbd"
    DataSeriesStbd.AcceptsUnsortedData = False
    Dim SeriePort As New FastMountainRenderableSeries
    Dim SerieStbd As New FastMountainRenderableSeries
    SeriePort.XAxisId = Xasse.Id
    SeriePort.YAxisId = Yasse.Id
    SerieStbd.XAxisId = Xasse.Id
    SerieStbd.YAxisId = Yasse.Id
    SeriePort.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.MinMax
    SerieStbd.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.MinMax
    Dim LastValidData As DateTime = ArrayTimeStamps(0)
    For i As Integer = 0 To ArrayTimeStamps.Count - 1
      If Not ArrayTimeStamps(i) = Nothing Then
        If ArrayTimeStamps(i) > LastValidData Then
          If ArrayTimeStamps(i) > LastValidData.AddSeconds(10) Then
            DataSeriesPort.Append(LastValidData.AddMilliseconds(10), Double.NaN)
            DataSeriesStbd.Append(LastValidData.AddMilliseconds(10), Double.NaN)
            DataSeriesPort.Append(ArrayTimeStamps(i).AddMilliseconds(-10), Double.NaN)
            DataSeriesStbd.Append(ArrayTimeStamps(i).AddMilliseconds(-10), Double.NaN)
          End If
          LastValidData = ArrayTimeStamps(i)
          If Not Double.IsNaN(CanaleTWA.Valori(i)) Then
            Dim Twa As Double = CanaleTWA.Valori(i)
            If Twa >= 0 Then
              DataSeriesPort.Append(ArrayTimeStamps(i), Double.NaN)
              DataSeriesStbd.Append(ArrayTimeStamps(i), System.Math.Abs(Twa))
            Else
              DataSeriesPort.Append(ArrayTimeStamps(i), System.Math.Abs(Twa))
              DataSeriesStbd.Append(ArrayTimeStamps(i), Double.NaN)
            End If
          End If
        End If
      End If
    Next
    SeriePort.DataSeries = DataSeriesPort
    SeriePort.StrokeThickness = 0
    SeriePort.Stroke = Colors.DarkRed
    SeriePort.Fill = New SolidColorBrush(Color.FromArgb(100, 255, 0, 0))
    Dim Tmp As New ChartSeriesViewModel(DataSeriesPort, SeriePort)
    SeriesSource.Add(Tmp)

    SerieStbd.DataSeries = DataSeriesStbd
    SerieStbd.StrokeThickness = 0
    SerieStbd.Stroke = Colors.DarkGreen
    SerieStbd.Fill = New SolidColorBrush(Color.FromArgb(100, 0, 255, 0))
    Tmp = New ChartSeriesViewModel(DataSeriesStbd, SerieStbd)
    SeriesSource.Add(Tmp)



    Dim Contatore As Integer = 2
    For Each CanaleValore In CanaliValore
      Yasse = New SciChart.Charting.Visuals.Axes.NumericAxis
      Yasse.AxisTitle = CanaleValore.ChannelId
      Yasse.Id = CanaleValore.ChannelId
      Yasse.Visibility = Visibility.Visible
      Yasse.GrowBy = New DoubleRange(0, 0.3)
      Yasse.DrawMajorGridLines = False
      Yasse.DrawMinorGridLines = False
      Yasse.AxisBandsFill = Nothing
      Yasse.DrawMajorBands = False
      Yasse.LabelProvider = New clsLabelProvider(CanaleValore.Decimals)

      TitleLabelStyle = New Style(GetType(AxisTitle))
      sc = New Setter(Label.ForegroundProperty, New SolidColorBrush(ColoriDifferenziati(Contatore)))
      TitleLabelStyle.Setters.Add(sc)
      Yasse.TitleStyle = TitleLabelStyle

      Yassi.Add(Yasse)

      Dim DataSeriesValori As New XyDataSeries(Of DateTime, Double)
      DataSeriesValori.SeriesName = CanaleValore.ChannelId
      DataSeriesValori.AcceptsUnsortedData = False
      Dim SerieValori As New FastLineRenderableSeries
      SerieValori.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.MinMax

      SerieValori.XAxisId = Xasse.Id
      SerieValori.YAxisId = CanaleValore.ChannelId
      LastValidData = ArrayTimeStamps(0)
      For i As Integer = 0 To ArrayTimeStamps.Count - 1
        If Not ArrayTimeStamps(i) = Nothing Then
          If ArrayTimeStamps(i) > LastValidData Then
            If ArrayTimeStamps(i) > LastValidData.AddSeconds(10) Then
              DataSeriesValori.Append(LastValidData.AddMilliseconds(10), Double.NaN)
              DataSeriesValori.Append(ArrayTimeStamps(i).AddMilliseconds(-10), Double.NaN)
            End If
            LastValidData = ArrayTimeStamps(i)
            If Not Double.IsNaN(CanaleValore.Valori(i)) Then
              DataSeriesValori.Append(ArrayTimeStamps(i), CanaleValore.Valori(i))
            End If
          End If
        End If
      Next
      SerieValori.DataSeries = DataSeriesValori
      SerieValori.StrokeThickness = 2
      SerieValori.Stroke = ColoriDifferenziati(Contatore)
      Contatore += 1
      Tmp = New ChartSeriesViewModel(DataSeriesValori, SerieValori)
      SeriesSource.Add(Tmp)
    Next

    Dim An As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    'An.X1 = 7
    An.Y1 = 0
    An.Stroke = New SolidColorBrush(Colors.DarkGoldenrod)
    An.StrokeThickness = 2
    An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
    An.Tag = eGruppoAnnotazione.eNothing
    An.IsHidden = False
    An.IsEditable = True
    An.ShowLabel = True
    Annotazioni.Add(An)

    AggiornaPeriodi()

  End Sub

  Public Sub AggiornaPeriodi()

    CancellaAnnotazioniPeriodi()
    If PeriodsManager Is Nothing Then Exit Sub
    If PeriodsManager.Periods Is Nothing Then Exit Sub
    For Each Periodo In PeriodsManager.Periods.Lista
      Periodo.IsSelected = False
      Dim AnBA As New SciChart.Charting.Visuals.Annotations.BoxAnnotation
      If Periodo.KeyMoment = Nothing Then
        AnBA.X1 = Periodo.TR.Start
        AnBA.X2 = Periodo.TR.Finish
      Else
        If Periodo.TR Is Nothing Then
          AnBA.X1 = Periodo.KeyMoment.AddSeconds(-10)
          AnBA.X2 = Periodo.KeyMoment.AddSeconds(10)
        Else
          AnBA.X1 = Periodo.TR.Start
          AnBA.X2 = Periodo.TR.Finish
        End If
      End If
      AnBA.Y1 = 0.06
      AnBA.Y2 = 0.15
      AnBA.Background = New SolidColorBrush(ColoreDaTipo(Periodo.PeriodType, 150)) 'Color.FromArgb(255, 255, 112, 52))
      AnBA.BorderBrush = New SolidColorBrush(ColoreDaTipo(Periodo.PeriodType, 255))
      AnBA.CornerRadius = New CornerRadius(3)
      AnBA.BorderThickness = New Thickness(1, 1, 1, 1)
      AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
      AnBA.Tag = eGruppoAnnotazione.ePeriods
      AnBA.IsHidden = False
      AnBA.AnnotationCanvas = AnnotationCanvas.AboveChart
      AnBA.ToolTip = Periodo.TR.DurataInStringa
      Annotazioni.Add(AnBA)
    Next
  End Sub

  Private Sub CancellaAnnotazioniPeriodi()
    If Annotazioni Is Nothing Then Exit Sub
    Dim AnTmp As New List(Of SciChart.Charting.Visuals.Annotations.IAnnotation)
    For Each Annotazione In Annotazioni
      If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
        Dim AB As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
        If Not AB.Tag Is Nothing Then
          If DirectCast(AB.Tag, eGruppoAnnotazione) = eGruppoAnnotazione.ePeriods Then
            AnTmp.Add(AB)
          End If
        End If
      End If
    Next
    For Each Annotazione In AnTmp
      Annotazioni.Remove(Annotazione)
    Next

  End Sub

End Class