Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged
Imports SciChart
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Axes
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.PaletteProviders
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class clsSciChart
  'Implements INotifyPropertyChanged


  Public WithEvents Grafico As SciChart.Charting.Visuals.SciChartSurface

  Public Property CanaleAscissa As clsChannel2020
  Public Property CanaliOrdinata As List(Of clsChannel2020)
  Public Property DSXYselection As clsSelezioneGraficoLatlong

  Public Property Annotazioni As SciChart.Charting.Visuals.Annotations.AnnotationCollection
  Public Property SelezioneAnte As SciChart.Charting.Visuals.Annotations.BoxAnnotation
  Public Property SelezionePost As SciChart.Charting.Visuals.Annotations.BoxAnnotation

  Public Property AnnotazioniChartEventi As SciChart.Charting.Visuals.Annotations.AnnotationCollection

  Dim BindGroupSelezione As New BindingGroup

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub


  Public Enum eTipoGrafico
    eLinea = 0
    eCandele = 1
    eBarre = 2
    eHighLow = 3
    eXY = 4

  End Enum

  Public Sub New(Grafico As SciChart.Charting.Visuals.SciChartSurface)
    Me.Grafico = Grafico
  End Sub

  'Public Property Grafico As SciChart.Charting.Visuals.SciChartSurface
  '  Get
  '    Return pGrafico
  '  End Get
  '  Set(value As SciChart.Charting.Visuals.SciChartSurface)
  '    pGrafico = value
  '  End Set
  'End Property

  'Public Property CanaleAscissa As clsChannel2020
  '  Get
  '    Return pCanaleAscissa
  '  End Get
  '  Set(value As clsChannel2020)
  '    pCanaleAscissa = value
  '  End Set
  'End Property

  'Public Property CanaliOrdinata As List(Of clsChannel2020)
  '  Get
  '    Return pCanaliOrdinata
  '  End Get
  '  Set(value As List(Of clsChannel2020))
  '    pCanaliOrdinata = value
  '  End Set
  'End Property

  'Public Property PosizioneCorrenteX As Double
  '  Get
  '    Return pDSXYselection.Xcorrente
  '  End Get
  '  Set(value As Double)
  '    If pDSXYselection.Xcorrente = value Then Exit Property
  '    pDSXYselection.Xcorrente = value
  '    OnPropertyChanged("PosizioneCorrenteX")
  '  End Set
  'End Property

  'Public Property PosizioneCorrenteY As Double
  '  Get
  '    Return pDSXYselection.Ycorrente
  '  End Get
  '  Set(value As Double)
  '    If pDSXYselection.Ycorrente = value Then Exit Property
  '    pDSXYselection.Ycorrente = value
  '    OnPropertyChanged("PosizioneCorrenteY")
  '  End Set
  'End Property

  Public Sub AggiornaCursoreLatLong(IndiceRiga As Integer)
    DSXYselection.AggiornaPuntoCorrente(IndiceRiga)
    'OnPropertyChanged("PosizioneCorrenteX")
    'OnPropertyChanged("PosizioneCorrenteY")
  End Sub

  'Public Property AnnotazioniChartEventi As SciChart.Charting.Visuals.Annotations.AnnotationCollection
  '  Get
  '    Return pAnnotazioni ' .Union(vv)
  '  End Get
  '  Set(value As SciChart.Charting.Visuals.Annotations.AnnotationCollection)
  '    If pAnnotazioni Is value Then Exit Property
  '    pAnnotazioni = value
  '    OnPropertyChanged("AnnotazioniChartEventi")
  '  End Set
  'End Property

  'Public Property SelezioneAnte As SciChart.Charting.Visuals.Annotations.BoxAnnotation
  '  Get
  '    Return pSelezioneAnte ' .Union(vv)
  '  End Get
  '  Set(value As SciChart.Charting.Visuals.Annotations.BoxAnnotation)
  '    pSelezioneAnte = value
  '  End Set
  'End Property

  'Public Property SelezionePost As SciChart.Charting.Visuals.Annotations.BoxAnnotation
  '  Get
  '    Return pselezionePost ' .Union(vv)
  '  End Get
  '  Set(value As SciChart.Charting.Visuals.Annotations.BoxAnnotation)
  '    pselezionePost = value
  '  End Set
  'End Property

  Public Sub CambiaSelezione(DTselezione As clsTimeRange)
    SelezioneAnte.X2 = DTselezione.Start
    SelezionePost.X1 = DTselezione.Finish
  End Sub

  'Public Property DSXYselection As clsSelezioneGraficoLatlong
  '  Get
  '    Return pDSXYselection
  '  End Get
  '  Set(value As clsSelezioneGraficoLatlong)
  '    pDSXYselection = value
  '  End Set
  'End Property

  Private Sub PulisciGrafico()
    Grafico.RenderableSeries.Clear()
    Grafico.XAxes.Clear()
    Grafico.YAxes.Clear()
    If Not Grafico.XAxis Is Nothing Then Grafico.XAxis.Clear()
    If Not Grafico.YAxis Is Nothing Then Grafico.YAxis.Clear()
  End Sub


  Public Sub DrawScatterTest()

    Dim DataSeries As New XyDataSeries(Of Double, Double)
    Dim Random As New Random
    Dim dataPointColors As New List(Of Color)

    PulisciGrafico()

    DataSeries.AcceptsUnsortedData = True


    For i As Integer = 0 To 100000
      DataSeries.Append(Random.NextDouble(), Random.NextDouble)
      dataPointColors.Add(GetRandomColor(Random))
    Next

    Dim scatterSeries As New ExtremeScatterRenderableSeries
    scatterSeries.PointMarker = New EllipsePointMarker
    scatterSeries.PointMarker.Width = 5
    scatterSeries.PointMarker.Height = 5

    Grafico.RenderableSeries.Add(scatterSeries)
    scatterSeries.DataSeries = DataSeries


  End Sub

  Private Function GetRandomColor(Random As Random) As Color
    Return Color.FromArgb(255, Random.Next(255), Random.Next(255), Random.Next(255))
  End Function


  Public Sub AggiornaSelezioneGraficoEventi(Intervallo As clsTimeRange)
    SelezioneAnte.X2 = Intervallo.Start
    selezionePost.X1 = Intervallo.Finish
    SelezioneAnte.Background = New SolidColorBrush(Color.FromArgb(255, 128, 128, 128))
    selezionePost.Background = New SolidColorBrush(Color.FromArgb(255, 128, 128, 128))
    AnnotazioniChartEventi(0).X2 = Intervallo.Start
    AnnotazioniChartEventi(1).X1 = Intervallo.Finish
    'OnPropertyChanged("AnnotazioniChartEventi")
  End Sub



  Public Sub AggiungiBox(Annotazione As SciChart.Charting.Visuals.Annotations.BoxAnnotation, Gruppo As BindingGroup, Inizio As DateTime, Fine As DateTime)
    Annotazione.X1 = Inizio
    Annotazione.X2 = Fine
    Annotazione.Y1 = 0
    Annotazione.Y2 = 1
    Annotazione.Background = New SolidColorBrush(Color.FromArgb(50, 128, 128, 128))
    Annotazione.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    Annotazione.BindingGroup = Gruppo
    Annotazione.IsHidden = False
    AnnotazioniChartEventi.Add(Annotazione)
  End Sub

  Public Sub DrawTimePlot(CanaleAscissa As clsChannel2020, CanaliOrdinata As List(Of clsChannel2020), Intervallo As clsTimeRange, objSMACVM As clsSynchronizeMouseAcrossChartsViewModel, AsseYUnificato As Boolean, AggiornaVisibleRangeX As Boolean, ValoreAssoluto As Boolean)

    Me.CanaleAscissa = CanaleAscissa
    Me.CanaliOrdinata = CanaliOrdinata

    PulisciGrafico()

    If CanaliOrdinata Is Nothing Then Exit Sub
    If CanaliOrdinata.Count = 0 Then Exit Sub

    Dim MaxY, MinY As Double

    Dim ID As Integer = 0

    For Each CanaleOrdinata As clsChannel2020 In CanaliOrdinata
      If Not CanaleOrdinata Is Nothing Then
        Dim DataSeriesTMP As New XyDataSeries(Of DateTime, Double)
        DataSeriesTMP.SeriesName = Intervallo.StringaPeriodo & " " & CanaleOrdinata.LongName
        DataSeriesTMP.AcceptsUnsortedData = False
        Dim Linea As New FastLineRenderableSeries
        Dim Xasse As New SciChart.Charting.Visuals.Axes.DateTimeAxis
        Dim Yasse As New SciChart.Charting.Visuals.Axes.NumericAxis
        Linea.Stroke = ColoriDifferenziati(ID)
        CanaleOrdinata.PrintedColor = Linea.Stroke
        'Linea.Foreground = New SolidColorBrush(Colori(ID))
        Xasse.AxisTitle = "" ' CanaleAscissa.LongName
        Xasse.Id = "ascissa"
        Xasse.SubDayTextFormatting = "HH:mm:ss.fF"

        Dim DataColorPoints As New List(Of Color)


        Dim VBR As New Binding("SharedXVisibleRange")
        VBR.Source = objSMACVM
        VBR.Mode = BindingMode.TwoWay
        Xasse.SetBinding(SciChart.Charting.Visuals.Axes.AxisCore.VisibleRangeProperty, VBR)
        Grafico.DataContext = objSMACVM


        Linea.XAxisId = Xasse.Id

        Yasse.AxisTitle = CanaleOrdinata.LongName
        If CanaleOrdinata Is CanaliOrdinata.First Then
          Yasse.Id = "DefaultAxisId"
        Else
          Yasse.Id = "Canale" & CanaleOrdinata.IdIntestazione
        End If
        Linea.YAxisId = Yasse.Id


        Dim suka As New Charting.Visuals.Axes.LabelProviders.DefaultTickLabel
        Dim LabelStyle As New Style(suka.GetType())
        LabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(ColoriDifferenziati(ID))))
        Yasse.TickLabelStyle = LabelStyle
        Dim suka2 As New AxisTitle()
        Dim TitleYLabelStyle As New Style(suka2.GetType)
        TitleYLabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(ColoriDifferenziati(ID))))
        Yasse.TitleStyle = TitleYLabelStyle


        Grafico.XAxis = Xasse
        Grafico.YAxes.Add(Yasse)
        If CanaleOrdinata.Valori.Count > 0 Then
          For Indice As Integer = Intervallo.IdRigaIniziale To Intervallo.IdRigaFinale
            Dim X As DateTime = CanaleAscissa.ValoriDT(Indice)
            Dim Y As Double = CanaleOrdinata.Valori(Indice)

            If ValoreAssoluto Then
              Select Case CanaleOrdinata.DataType
                Case clsChannel2020.eDataType.e180
                  If Y > 0 Then
                    DataColorPoints.Add(Colors.Green)
                  Else
                    DataColorPoints.Add(Colors.Red)
                  End If
                Case Else
                  If Y > 0 Then
                    DataColorPoints.Add(ColoriDifferenziati(ID))
                  Else
                    'Dim C As Color = Colori(ID)
                    'C.A = 100
                    'DataColorPoints.Add(C)
                    DataColorPoints.Add(Colors.OrangeRed)
                  End If
              End Select

              Y = System.Math.Abs(Y)
            Else
              DataColorPoints.Add(ColoriDifferenziati(ID))
            End If

            DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
            If Indice = Intervallo.IdRigaIniziale Then
              MaxY = Y
              MinY = Y
            Else
              MaxY = System.Math.Max(MaxY, Y)
              MinY = System.Math.Min(MinY, Y)
            End If
          Next
          Linea.DataSeries = DataSeriesTMP
          Linea.PaletteProvider = New clsStrokePaletteProvider(DataColorPoints)
          Grafico.RenderableSeries.Add(Linea)
          ID += 1
        End If
      End If
    Next

    If AsseYUnificato Then
      For Each AsseY As SciChart.Charting.Visuals.Axes.NumericAxis In Grafico.YAxes
        AsseY.VisibleRange = New SciChart.Data.Model.DoubleRange(MinY, MaxY)
      Next
    End If
    If AggiornaVisibleRangeX Then Grafico.ZoomExtents()


  End Sub


  Private Function LatLongMediane(CanaleLat As clsChannel2020, CanaleLong As clsChannel2020, RigaIniziale As Integer, RigaFinale As Integer) As List(Of Double)
    'restituisce lat all' indice 0 e long all' indice 1
    Dim Lat(1) As Double
    Dim Lon(1) As Double
    Dim ValoriLat = CanaleLat.Valori.ToList.GetRange(RigaIniziale, RigaFinale - RigaIniziale).Where(Function(x) Not Double.IsNaN(x))
    Dim ValoriLon = CanaleLong.Valori.ToList.GetRange(RigaIniziale, RigaFinale - RigaIniziale).Where(Function(x) Not Double.IsNaN(x))
    Dim LatAvg As Double = ValoriLat.Average
    Dim LonAvg As Double = ValoriLon.Average
    ValoriLat = ValoriLat.Where(Function(x) CInt(x / LatAvg) = 1)
    ValoriLon = ValoriLon.Where(Function(x) CInt(x / LonAvg) = 1)

    Lat(0) = ValoriLat(0)
    Lat(1) = ValoriLat(0)
    Lon(0) = ValoriLon(0)
    Lon(1) = ValoriLon(0)
    For Indice As Integer = 0 To Lat.Count - 1
      Lat(0) = System.Math.Min(Lat(0), ValoriLat(Indice))
      Lat(1) = System.Math.Max(Lat(1), ValoriLat(Indice))
      Lon(0) = System.Math.Min(Lon(0), ValoriLon(Indice))
      Lon(1) = System.Math.Max(Lon(1), ValoriLon(Indice))

    Next

    Dim SolTMP As New List(Of Double)
    SolTMP.Add((Lat(0) + Lat(1)) / 2)
    SolTMP.Add((Lon(0) + Lon(1)) / 2)
    Return SolTMP
  End Function


  Public Sub AggiornaSelezioneGraficoLatLong(Intervallo As clsTimeRange)
    DSXYselection.AggiornaSelezioneGraficoLatLong(Intervallo)
  End Sub

  Public Sub DrawlatLongCleanup()
    DSXYselection = Nothing
    PulisciGrafico()
  End Sub

  Public Sub DrawLatLong(CanaleLat As clsChannel2020, CanaleLong As clsChannel2020, CanaleValore As clsChannel2020, Intervallo As clsTimeRange, IntervalloSel As clsTimeRange, Steppi As Integer)

    Me.CanaleAscissa = CanaleValore
    PulisciGrafico()
    If CanaleValore Is Nothing Then Exit Sub
    ' la traccia visualizzata deve essere tutto con un layer piú spesso ma trasparente del periodo correntemente selezionato



    ' questa funzione serve a visualizzare la/le traccia/tracce
    ' Long viene convertita in coordinate geografiche assumendo la terra sferica....
    ' se l'intervallo é uno solo, la mappa viene disegnata dal centro delle coordinate

    ' per prima cosa viene individuato il punto medio di ogni intervallo


    Dim Intervalli As Integer = 10
    Dim Valori = CanaleValore.Valori.Where(Function(x) Not Double.IsNaN(x))
    Dim Va As Double = ImpostaAggregazione(-1, Valori.Max, Valori.Min, Intervalli)
    Dim GruppoMinimo As Integer = CInt(Valori.Min / Va) * Va

    Dim NColori As Integer = Int(System.Math.Abs(CDbl(Valori.Max - Valori.Min)) / Va) + 1
    'Dim Colori As List(Of Color) = ColoreBeneMale(NColori)

    Dim RigaIniziale As Integer = Intervallo.IdRigaIniziale
    Dim RigaFinale As Integer = Intervallo.IdRigaFinale
    'IndiciIntervallo(Intervallo, RigaIniziale, RigaFinale)
    Dim PuntoCentrale As List(Of Double) = LatLongMediane(CanaleLat, CanaleLong, RigaIniziale, RigaFinale)


    Dim Xasse As New SciChart.Charting.Visuals.Axes.NumericAxis
    Dim Yasse As New SciChart.Charting.Visuals.Axes.NumericAxis
    Xasse.AxisTitle = "Longitude"
    Xasse.Id = "ascissa"
    Xasse.GrowBy = New DoubleRange(0.2, 0.2)
    Yasse.AxisTitle = "Latitude"
    Yasse.Id = "DefaultAxisId"
    Yasse.GrowBy = New DoubleRange(0.2, 0.2)
    Grafico.XAxis = Xasse
    Grafico.YAxes.Add(Yasse)

    Dim DS As New List(Of XyDataSeries(Of Double, Double))
    'Dim RL As New List(Of FastLineRenderableSeries)
    Dim Contatore(NColori) As Integer
    Dim RLt As New FastLineRenderableSeries
    For i As Integer = 0 To NColori
      Dim DStmp As New XyDataSeries(Of Double, Double)
      DStmp.AcceptsUnsortedData = True
      DStmp.SeriesName = i.ToString
      DS.Add(DStmp)
    Next

    Dim DSXYSel As New XyDataSeries(Of Double, Double)
    DSXYSel.AcceptsUnsortedData = True
    If DSXYselection Is Nothing Then
      DSXYselection = New clsSelezioneGraficoLatlong(DSXYSel, CanaleLat, CanaleLong, Steppi, PuntoCentrale)
    End If


    RLt.XAxisId = Xasse.Id
    RLt.YAxisId = Yasse.Id
    RLt.Stroke = Colors.Black
    RLt.StrokeThickness = 1
    RLt.DataSeries = DSXYselection.DSXYselection
    RLt.PointMarker = New SquarePointMarker()
    RLt.PointMarker.Stroke = Colors.Black
    RLt.PointMarker.Height = 8
    RLt.PointMarker.Width = 8
    RLt.PointMarker.StrokeThickness = 3
    RLt.PointMarker.Fill = Colors.Transparent

    Grafico.RenderableSeries.Add(RLt)

    Dim MaxX As Double = CanaleLong.Valori(RigaIniziale)
    Dim MaxY As Double = CanaleLat.Valori(RigaIniziale)
    Dim MinX As Double = CanaleLong.Valori(RigaIniziale)
    Dim MInY As Double = CanaleLat.Valori(RigaIniziale)

    For Indice As Integer = RigaIniziale To RigaFinale Step Steppi
      Dim Y As Double = CanaleLat.Valori(Indice)
      Dim X As Double = CanaleLong.Valori(Indice)
      If Not (Double.IsNaN(X) OrElse Double.IsNaN(Y)) Then
        Dim dX As Double = LongProiezione(Y, PuntoCentrale(1) - X)
        X -= dX
        If CInt(CanaleLat.Valori(Indice) / PuntoCentrale(0)) = 1 Then
          If CInt(CanaleLong.Valori(Indice) / PuntoCentrale(1)) = 1 Then
            MaxX = System.Math.Max(MaxX, X)
            MaxY = System.Math.Max(MaxY, Y)
            MinX = System.Math.Min(MinX, X)
            MInY = System.Math.Min(MInY, Y)
            Dim Gruppo As Integer = CInt(CanaleValore.Valori(Indice) / Va) * Va
            Dim idx As Integer = System.Math.Min(CInt((Gruppo - GruppoMinimo) / Va), NColori)
            'Dim Colore As Color = ColoreBeneMale(255, idx) ' ColoriDifferenziati(idx)
            'DSXYSel.Append(X, Y)
            For Each S In DS
              If S Is DS(idx) Then
                DS(idx).Append(X, Y)
                Contatore(idx) += 1
              Else
                'DS(idx).Append(X, Double.NaN)
              End If
            Next
          Else
            'se passa qui significa che la longitudine del punto in questione é di oltre 1 grado distante dal punto centrale...
            'Stop
          End If
        End If
      Else
        'se passa qui significa che la latitudine del punto in questione é di oltre 1 grado distante dal punto centrale...
        'Stop
      End If
    Next


    For i As Integer = 0 To DS.Count - 1
      Dim RLtmp As New XyScatterRenderableSeries
      'RL.Add(New FastLineRenderableSeries)
      RLtmp.XAxisId = Xasse.Id
      RLtmp.YAxisId = Yasse.Id
      RLtmp.Stroke = ColoreBeneMale(255, i / DS.Count) ' ColoriDifferenziati(i)
      RLtmp.StrokeThickness = 10

      RLtmp.PointMarker = New EllipsePointMarker()
      RLtmp.PointMarker.Stroke = ColoreBeneMale(255, i / DS.Count) 'ColoriDifferenziati(i)
      RLtmp.PointMarker.Height = 3
      RLtmp.PointMarker.Width = 3
      RLtmp.PointMarker.StrokeThickness = 1
      RLtmp.PointMarker.Fill = ColoreBeneMale(255, i / DS.Count) 'ColoriDifferenziati(i)

      RLtmp.DataSeries = DS(i)
      Grafico.RenderableSeries.Add(RLtmp)
    Next


    Dim DeltaLat As Double = MaxX - MinX
    Dim DeltaLong As Double = MaxY - MInY
    Dim Ratio As Double = DeltaLat / DeltaLong
    Dim MaxD As Double = System.Math.Max(DeltaLat, DeltaLong)
    Grafico.XAxis.VisibleRange = New DoubleRange(PuntoCentrale(1) - (MaxD / 2), PuntoCentrale(1) + (MaxD / 2))
    Grafico.YAxes.First.VisibleRange = New DoubleRange(PuntoCentrale(0) - (MaxD / 2), PuntoCentrale(0) + (MaxD / 2))


    'pGrafico.ZoomExtents()

  End Sub

  Private Function VerificaFiltro(Filtro As clsFiltro, ValoreAscissa As Double, ValoreOrdinata As Double) As Boolean
    Return VerificaFiltroAscissa(Filtro, ValoreAscissa) And VerificaFiltroOrdinata(Filtro, ValoreOrdinata)
  End Function

  Private Function VerificaFiltroAscissa(Filtro As clsFiltro, Valore As Double) As Boolean
    If Filtro Is Nothing Then
      Return True
    Else
      Return Filtro.VerificaAscissa(Valore)
    End If
  End Function

  Private Function VerificaFiltroOrdinata(Filtro As clsFiltro, Valore As Double) As Boolean
    If Filtro Is Nothing Then
      Return True
    Else
      Return Filtro.VerificaOrdinata(Valore)
    End If
  End Function


End Class


Public Class clsSelezioneGraficoLatlong
  Dim pDSXYselection As New XyDataSeries(Of Double, Double)
  Dim pCanaleLat As clsChannel2020
  Dim pCanaleLong As clsChannel2020
  Dim pSteppi As Integer
  Dim pPuntoCentrale As List(Of Double)
  Dim pXcorrente As Double
  Dim pYcorrente As Double

  Public Property Xcorrente As Double
    Get
      Return pXcorrente
    End Get
    Set(value As Double)
      pXcorrente = value
    End Set
  End Property

  Public Property Ycorrente As Double
    Get
      Return pYcorrente
    End Get
    Set(value As Double)
      pYcorrente = value
    End Set
  End Property

  Public Property DSXYselection As XyDataSeries(Of Double, Double)
    Get
      Return pDSXYselection
    End Get
    Set(value As XyDataSeries(Of Double, Double))
      pDSXYselection = value
    End Set
  End Property

  Public Sub New(DSXYselection As XyDataSeries(Of Double, Double), CanaleLat As clsChannel2020, CanaleLong As clsChannel2020, Steppi As Integer, PuntoCentrale As List(Of Double))
    pDSXYselection = DSXYselection
    pCanaleLat = CanaleLat
    pCanaleLong = CanaleLong
    pSteppi = Steppi
    pPuntoCentrale = PuntoCentrale

  End Sub

  Public Sub AggiornaSelezioneGraficoLatLong(Intervallo As clsTimeRange)
    'Dim RigaIniziale, RigaFinale As Integer
    'IndiciIntervallo(Intervallo, RigaIniziale, RigaFinale)
    If Intervallo.IdRigaIniziale = 0 AndAlso Intervallo.IdRigaFinale = 0 Then
      Exit Sub
    End If
    pDSXYselection.Clear()
    For Indice As Integer = Intervallo.IdRigaIniziale To Intervallo.IdRigaFinale Step pSteppi
      Dim Y As Double = pCanaleLat.Valori(Indice)
      Dim X As Double = pCanaleLong.Valori(Indice)
      If Not (Double.IsNaN(X) OrElse Double.IsNaN(Y)) Then
        Dim dX As Double = LongProiezione(Y, pPuntoCentrale(1) - X)
        X -= dX
        If CInt(pCanaleLat.Valori(Indice) / pPuntoCentrale(0)) = 1 Then
          If CInt(pCanaleLong.Valori(Indice) / pPuntoCentrale(1)) = 1 Then
            pDSXYselection.Append(X, Y)
          End If
        End If
      End If

    Next

  End Sub

  Public Sub AggiornaPuntoCorrente(Indice As Integer)
    Dim Y As Double = pCanaleLat.Valori(Indice)
    Dim X As Double = pCanaleLong.Valori(Indice)
    If Not (Double.IsNaN(X) OrElse Double.IsNaN(Y)) Then
      Dim dX As Double = LongProiezione(Y, pPuntoCentrale(1) - X)
      X -= dX
      If CInt(pCanaleLat.Valori(Indice) / pPuntoCentrale(0)) = 1 Then
        If CInt(pCanaleLong.Valori(Indice) / pPuntoCentrale(1)) = 1 Then
          Xcorrente = X
          Ycorrente = Y
        End If
      End If
    End If

  End Sub

End Class


Public Class clsExtremePointMarkerPaletteProvider
  Implements IExtremePointMarkerPaletteProvider

  Dim pListaColori As List(Of Color)
  Dim pColori As New Values(Of Color)

  Public Sub New(ListaColori As List(Of Color))
    pListaColori = ListaColori
  End Sub

  Public ReadOnly Property Colors As Values(Of Color) Implements IExtremePointMarkerPaletteProvider.Colors
    Get
      Return pColori
      'Throw New NotImplementedException()
    End Get
  End Property

  Public Sub OnBeginSeriesDraw(rSeries As IRenderableSeries) Implements IPaletteProvider.OnBeginSeriesDraw
    Dim Indici As Values(Of Integer) = rSeries.CurrentRenderPassData.PointSeries.Indexes
    Dim Conteggio As Integer = Indici.Count
    pColori.Count = Conteggio
    For i As Integer = 0 To Conteggio - 1
      Dim IndiceTMP As Integer = Indici(i)
      pColori(i) = pListaColori(IndiceTMP)
    Next
    'Throw New NotImplementedException()
  End Sub
End Class

Public Class clsPointMarkerPaletteProvider
  Implements IPointMarkerPaletteProvider

  Dim pListaColori As List(Of Color)
  Dim pPPI As New List(Of PointPaletteInfo)
  Dim pSeries As IRenderableSeries = Nothing

  Public Sub New(ListaColori As List(Of Color))
    pListaColori = ListaColori
  End Sub

  Public Sub OnBeginSeriesDraw(rSeries As IRenderableSeries) Implements IPaletteProvider.OnBeginSeriesDraw
    If Not pSeries Is Nothing Then
      If pSeries Is rSeries Then Exit Sub
    End If

    pSeries = rSeries
    Dim Indici As Values(Of Integer) = pSeries.CurrentRenderPassData.PointSeries.Indexes
    Dim Conteggio As Integer = Indici.Count
    For i As Integer = 0 To Conteggio - 1
      Dim IndiceTMP As Integer = Indici(i)
      Dim PPItmp As New PointPaletteInfo
      PPItmp.Stroke = pListaColori(IndiceTMP)
      pPPI.Add(PPItmp)

    Next
  End Sub

  Public Function OverridePointMarker(rSeries As IRenderableSeries, index As Integer, metadata As IPointMetadata) As PointPaletteInfo? Implements IPointMarkerPaletteProvider.OverridePointMarker

    Return pPPI(index)

  End Function
End Class


Public Class clsStrokePaletteProvider
  Implements IStrokePaletteProvider

  Dim pListaColori As List(Of Color)
  Dim pColori As New Values(Of Color)
  Dim pSeries As IRenderableSeries = Nothing

  Public Sub New(ListaColori As List(Of Color))
    pListaColori = ListaColori
  End Sub

  Public Sub OnBeginSeriesDraw(rSeries As IRenderableSeries) Implements IPaletteProvider.OnBeginSeriesDraw

  End Sub

  Public Function OverrideStrokeColor(rSeries As IRenderableSeries, index As Integer, metadata As IPointMetadata) As Color? Implements IStrokePaletteProvider.OverrideStrokeColor
    Return pListaColori(index)
  End Function
End Class


Public Class clsFillPaletteProvider
  Implements IFillPaletteProvider

  Dim pListaColori As List(Of Color)
  Dim pPennarelli As List(Of Brush)
  Dim pSeries As IRenderableSeries = Nothing

  Public Sub New(ListaColori As List(Of Color))
    pListaColori = ListaColori
  End Sub

  Public Sub OnBeginSeriesDraw(rSeries As IRenderableSeries) Implements IPaletteProvider.OnBeginSeriesDraw

  End Sub

  Public Function OverrideFillBrush(rSeries As IRenderableSeries, index As Integer, metadata As IPointMetadata) As Brush Implements IFillPaletteProvider.OverrideFillBrush
    Return New SolidColorBrush(pListaColori(index))
  End Function
End Class


Public Class clsVincolo
  Dim pCondizione As eCondizione
  Dim pValoreVincolo As Double
  Dim pValoreVincolo2 As Double
  Dim pCoppiaValoriVicolo As Boolean

  Public ReadOnly Property Condizione As eCondizione
    Get
      Return pCondizione
    End Get
  End Property

  Public ReadOnly Property ValoreVincolo As Double
    Get
      Return pValoreVincolo
    End Get
  End Property


  Public Sub New(Condizione As eCondizione, ValoreVincolo As Double)
    pCondizione = Condizione
    pValoreVincolo = ValoreVincolo
    pCoppiaValoriVicolo = False
  End Sub

  Public Sub New(Condizione As eCondizione, ValoreMinoreVincolo As Double, ValoreMaggioreVincolo As Double)
    pCondizione = Condizione
    pValoreVincolo = ValoreMinoreVincolo
    pValoreVincolo = ValoreMaggioreVincolo
    pCoppiaValoriVicolo = True
  End Sub

  Public Enum eCondizione
    eUguale = 0
    eDiverso = 1
    eMinore = 2
    eMinoreUguale = 3
    eMaggiore = 4
    eMaggioreUguale = 5
    eInterno = 6
    eInternoInclusiEstremi = 7
    eEsterno = 8
    eEsternoInclusiEstremi = 9
  End Enum

  Public Function Verifica(Valore As Double) As Boolean
    Select Case pCondizione
      Case eCondizione.eDiverso
        Return Not Valore = pValoreVincolo
      Case eCondizione.eMaggiore
        Return Valore > pValoreVincolo
      Case eCondizione.eMaggioreUguale
        Return Valore >= pValoreVincolo
      Case eCondizione.eMinore
        Return Valore < pValoreVincolo
      Case eCondizione.eMinoreUguale
        Return Valore <= pValoreVincolo
      Case eCondizione.eUguale
        Return Valore = pValoreVincolo
      Case eCondizione.eInterno
        If pCoppiaValoriVicolo Then
          Return Valore > ValoreVincolo And Valore < pValoreVincolo2
        Else
          Return True
        End If
      Case eCondizione.eInternoInclusiEstremi
        If pCoppiaValoriVicolo Then
          Return Valore >= ValoreVincolo And Valore <= pValoreVincolo2
        Else
          Return True
        End If
      Case eCondizione.eEsterno
        If pCoppiaValoriVicolo Then
          Return Valore < ValoreVincolo Or Valore > pValoreVincolo2
        Else
          Return True
        End If
      Case eCondizione.eEsternoInclusiEstremi
        If pCoppiaValoriVicolo Then
          Return Valore <= ValoreVincolo Or Valore >= pValoreVincolo2
        Else
          Return True
        End If

    End Select
    Return True
  End Function

End Class

Public Class clsFiltro
  Dim pVincoloAscissa As clsVincolo
  Dim pVincoloOrdinata As clsVincolo

  Public ReadOnly Property VincoloAscissa As clsVincolo
    Get
      Return pVincoloAscissa
    End Get
  End Property

  Public ReadOnly Property VincoloOrdinata As clsVincolo
    Get
      Return pVincoloOrdinata
    End Get
  End Property

  Public Sub New(VincoloAscissa As clsVincolo, VincoloOrdinata As clsVincolo)
    pVincoloAscissa = VincoloAscissa
    pVincoloOrdinata = VincoloOrdinata

  End Sub


  Public Function VerificaAscissa(Valore As Double) As Boolean
    If pVincoloAscissa Is Nothing Then
      Return True
    Else
      Return pVincoloAscissa.Verifica(Valore)
    End If
  End Function

  Public Function VerificaOrdinata(Valore As Double) As Boolean
    If pVincoloOrdinata Is Nothing Then
      Return True
    Else
      Return pVincoloOrdinata.Verifica(Valore)
    End If
  End Function

End Class



<AddINotifyPropertyChangedInterface>
Public Class clsSynchronizeMouseAcrossChartsViewModel
  'Implements INotifyPropertyChanged

  Public Property MouseWheelEnabled As Boolean
  Public Property PanEnabled As Boolean
  Public Property RolloverEnabled As Boolean
  Public Property CursorEnabled As Boolean
  Public Property ZoomEnabled As Boolean
  Dim _SharedXVisibleRange As IRange
  Public Property Name As String
  Public Property TimeRange As clsTimeRange
  Public Property VerticalBarPosition As DateTime


  Public Sub New(Name As String)
    MouseWheelEnabled = False
    PanEnabled = True
    CursorEnabled = True
    RolloverEnabled = True
    Name = Name

  End Sub

  'Public ReadOnly Property Name As String
  '  Get
  '    Return pName
  '  End Get
  'End Property

  'Public ReadOnly Property TimeRange As clsTimeRange
  '  Get
  '    Return pTimeRange
  '  End Get
  'End Property


  Public Property SharedXVisibleRange As IRange
    Get
      Return _SharedXVisibleRange
    End Get
    Set(value As IRange)
      'Console.Write("Set SharedXVisibleRange '" & pName & "'")
      If IsNothing(_SharedXVisibleRange) Then
        'Console.WriteLine(" IsNothing RAISE EVENT")
        _SharedXVisibleRange = value
        Dim inizio As DateTime = DirectCast(SharedXVisibleRange.Min, DateTime)
        Dim fine As DateTime = DirectCast(SharedXVisibleRange.Max, DateTime)
        TimeRange = New clsTimeRange(inizio, fine)
        VerticalBarPosition = TimeRange.Start.AddSeconds(200)
        'OnPropertyChanged("SharedXVisibleRange")
      Else
        'Console.Write(" Not IsNothing")
        If RangeIsChanged(value) Then
          'Console.WriteLine(" RangeIsChanged RAISE EVENT")
          _SharedXVisibleRange = value
          Dim inizio As DateTime = DirectCast(_SharedXVisibleRange.Min, DateTime)
          Dim fine As DateTime = DirectCast(_SharedXVisibleRange.Max, DateTime)
          TimeRange = New clsTimeRange(inizio, fine)
          VerticalBarPosition = TimeRange.Start.AddSeconds(100)
          'pTimeRange.Descrizione = "wertwert"
          'OnPropertyChanged("SharedXVisibleRange")
        Else
          'Console.WriteLine(" Not RangeIsChanged NO EVENT")
        End If
      End If
    End Set
  End Property

  Private Function RangeIsChanged(NewRange As IRange) As Boolean
    Dim inizio As DateTime = DirectCast(NewRange.Min, DateTime)
    Dim fine As DateTime = DirectCast(NewRange.Max, DateTime)
    Dim NewTR As New clsTimeRange(inizio, fine)
    If Not TimeRange.HasSameRange(NewTR) Then
      Return True
    End If
    Return False
  End Function

  'Public Property MouseWheelEnabled As Boolean
  '  Get
  '    Return pMouseWheelEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pMouseWheelEnabled = value Then Exit Property
  '    pMouseWheelEnabled = value
  '    OnPropertyChanged("MouseWheelEnabled")
  '  End Set
  'End Property

  'Public Property PanEnabled As Boolean
  '  Get
  '    Return pPanEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pPanEnabled = value Then Exit Property
  '    pPanEnabled = value
  '    OnPropertyChanged("PanEnabled")
  '  End Set
  'End Property

  'Public Property ZoomEnabled As Boolean
  '  Get
  '    Return pZoomEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pZoomEnabled = value Then Exit Property
  '    pZoomEnabled = value
  '    OnPropertyChanged("ZoomEnabled")
  '  End Set
  'End Property

  'Public Property CursorEnabled As Boolean
  '  Get
  '    Return pCursorEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pCursorEnabled = value Then Exit Property
  '    pCursorEnabled = value
  '    OnPropertyChanged("CursorEnabled")
  '  End Set
  'End Property

  'Public Property RolloverEnabled As Boolean
  '  Get
  '    Return pRolloverEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pRolloverEnabled = value Then Exit Property
  '    pRolloverEnabled = value
  '    OnPropertyChanged("RolloverEnabled")
  '  End Set
  'End Property

  'Public Property VerticalBarPosition As DateTime
  '  Get
  '    Return pVerticalBarPosition
  '  End Get
  '  Set(value As DateTime)
  '    If pVerticalBarPosition = value Then Exit Property
  '    pVerticalBarPosition = value
  '    OnPropertyChanged("VerticalBarPosition")
  '  End Set
  'End Property


  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub



  'Dim _currentHeight As Double
  'Public Property CurrentHeight As Double
  '  Get
  '    Return _currentHeight
  '  End Get
  '  Set(value As Double)
  '    'If pOvjSyMng Is value Then Exit Property
  '    _currentHeight = value
  '    OnPropertyChanged("CurrentHeight")
  '  End Set
  'End Property

End Class


Public Class clsSciChartTimeRange
  Dim pTimeRange As clsTimeRange
  Dim pCurrentRange As IRange
  Dim pName As String

  Public Event RangeChanged(CurrentRange As IRange, TimeRange As clsTimeRange, Name As String)

  Public Property CurrentRange As IRange
    Get
      Return pCurrentRange
    End Get
    Set(value As IRange)
      Dim TRtmp As clsTimeRange = IRangeToTimeRange(value)
      If Not pTimeRange.HasSameRange(TRtmp) Then
        pTimeRange = TRtmp
        pCurrentRange = value
        ScatenaEvento()
      End If
    End Set
  End Property

  Public Property TimeRange As clsTimeRange
    Get
      Return pTimeRange
    End Get
    Set(value As clsTimeRange)
      Dim TRtmp As IRange = TimeRangeToIRange(value)
      If pTimeRange Is Nothing OrElse Not pTimeRange.HasSameRange(value) Then
        pTimeRange = value
        pCurrentRange = TRtmp
        ScatenaEvento()
      End If
    End Set
  End Property

  Private Function IRangeToTimeRange(Range As IRange) As clsTimeRange
    Dim inizio As DateTime = DirectCast(Range.Min, DateTime)
    Dim fine As DateTime = DirectCast(Range.Max, DateTime)
    Return New clsTimeRange(inizio, fine)
  End Function

  Private Function TimeRangeToIRange(Range As clsTimeRange) As IRange
    Return New DateRange(Range.Start, Range.Finish)
  End Function

  Public Sub New(Name As String)
    pName = Name
  End Sub

  Private Sub ScatenaEvento()
    RaiseEvent RangeChanged(pCurrentRange, pTimeRange, pName)
  End Sub

End Class


Public Class cls360LabelProvider
  Inherits NumericLabelProvider

  Public Overrides Sub Init(parentAxis As IAxisCore)
    MyBase.Init(parentAxis)
  End Sub

  Public Overrides Sub OnBeginAxisDraw()
    MyBase.OnBeginAxisDraw()
  End Sub

  Public Overrides Function FormatLabel(dataValue As IComparable) As String
    Dim vTmp As Double = CDbl(dataValue)
    If vTmp < 0 Then vTmp += 360
    If vTmp > 360 Then vTmp -= 360
    Return Format(vTmp, "F0").PadLeft(3, "0")
    'Return MyBase.FormatLabel(vTmp)
  End Function

  Public Overrides Function FormatCursorLabel(dataValue As IComparable) As String
    Dim vTmp As Double = CDbl(dataValue)
    If vTmp < 0 Then vTmp += 360
    If vTmp > 360 Then vTmp -= 360
    Return Format(vTmp, "F0").PadLeft(3, "0")
    'Return MyBase.FormatCursorLabel(vTmp)
  End Function

End Class

Public Class clsLabelProvider
  Inherits NumericLabelProvider
  Dim pDecimals As Integer

  Public Sub New(Decimals As Integer)
    pDecimals = Decimals
  End Sub

  Public Overrides Sub Init(parentAxis As IAxisCore)
    MyBase.Init(parentAxis)
  End Sub

  Public Overrides Sub OnBeginAxisDraw()
    MyBase.OnBeginAxisDraw()
  End Sub

  Public Overrides Function FormatLabel(dataValue As IComparable) As String
    Return Format(dataValue, "F" & pDecimals)
    'Return MyBase.FormatLabel(vTmp)
  End Function

  Public Overrides Function FormatCursorLabel(dataValue As IComparable) As String
    Return Format(dataValue, "F" & pDecimals)
    'Return MyBase.FormatCursorLabel(vTmp)
  End Function

End Class

'<AddINotifyPropertyChangedInterface>
Public Class clsPuntoMetadata
  Implements IPointMetadata
  Dim _IsSelected As Boolean
  Dim _Momento As DateTime

  Public Sub New(IsSelected As Boolean)
    _IsSelected = IsSelected
  End Sub

  Public Sub New(IsSelected As Boolean, Momento As DateTime)
    _IsSelected = IsSelected
    _Momento = Momento
  End Sub

  Public Property IsSelected As Boolean Implements IPointMetadata.IsSelected
    Get
      Return _IsSelected
    End Get
    Set(value As Boolean)
      _IsSelected = value
    End Set
  End Property

  Public Property Momento As Date
    Get
      Return _Momento
    End Get
    Set(value As Date)
      _Momento = value
    End Set
  End Property

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class


