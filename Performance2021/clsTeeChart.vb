Imports System.Reflection
Imports Steema.TeeChart.WPF.Styles

Public Class clsTeeChart
  Inherits Steema.TeeChart.WPF.TChart

  Public Sub New()

    Dim Tipo As Type = Chart.GetType
    Dim Campo As FieldInfo = Tipo.GetField("evalExpired", BindingFlags.Instance Or BindingFlags.NonPublic)
    Campo.SetValue(Chart, False)
    'var Type = Chart.GetType();
    'var field = Type.GetField("evalExpired", BindingFlags.Instance | BindingFlags.NonPublic);
    '        field.SetValue(Chart, False);
  End Sub




End Class


Public Class clsTC
  'WithEvents pGrafico As clsTeeChart
  WithEvents pGrafico As Steema.TeeChart.WPF.TChart

  Public Enum eTipoGrafico
    eLinea = 0
    eCandele = 1
    eBarre = 2
    eHighLow = 3
    eXY = 4

  End Enum
  'Public Property Grafico As clsTeeChart
  '  Get
  '    Return pGrafico
  '  End Get
  '  Set(value As clsTeeChart)
  '    pGrafico = value
  '  End Set
  'End Property

  Public Property Grafico As Steema.TeeChart.WPF.TChart
    Get
      Return pGrafico
    End Get
    Set(value As Steema.TeeChart.WPF.TChart)
      pGrafico = value
    End Set
  End Property

  Public Sub New(ByRef Grafico As Steema.TeeChart.WPF.TChart)
    pGrafico = Grafico

  End Sub


  Public Sub DrawDemoBar()
    Dim SerieTmp As New Steema.TeeChart.WPF.Styles.Bar(pGrafico.Chart)

    SerieTmp.FillSampleValues(25)



  End Sub


  Public Sub DrawTimePlot(CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel), Intervalli As List(Of clsTimeRange))

    Dim Serie(Intervalli.Count - 1) As Steema.TeeChart.WPF.Styles.Line
    pGrafico.Header.Text = CanaliOrdinata.ToString
    pGrafico.Header.Alignment = TextAlignment.Center
    Dim IDi As Integer = 0
    For Each Intervallo As clsTimeRange In Intervalli
      Dim RigaIniziale, RigaFinale As Integer
      IndiciIntervallo(Intervallo, RigaIniziale, RigaFinale)
      For Each CanaleOrdinata As clsChannel In CanaliOrdinata
        Serie(IDi) = New Steema.TeeChart.WPF.Styles.Line(pGrafico.Chart)
        Serie(IDi).Title = "[" & IDi & "] " & Intervallo.StringaPeriodo & "  " & CanaleOrdinata.LongName
        Serie(IDi).XValues.DateTime = True
        Serie(IDi).Legend.Visible = True
        Serie(IDi).XValues.Order = Steema.TeeChart.Styles.ValueListOrder.None
        Serie(IDi).Pointer.Style = Steema.TeeChart.Styles.PointerStyles.SmallDot
        For Indice As Integer = RigaIniziale To RigaFinale
          Dim X As DateTime = CanaleAscissa.ValuesDT(Indice)
          Dim Y As Double = CanaleOrdinata.Values(Indice)
          Serie(IDi).Add(X, Y)
        Next
      Next
    Next

  End Sub


  Public Sub DrawXY(CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel), Intervalli As List(Of clsTimeRange), CanaleFiltro As clsChannel, valoreMinimo As Double)


    pGrafico.Chart.Series.Clear()


    pGrafico.Aspect.ClipPoints = False
    pGrafico.Header.Visible = True
    pGrafico.Legend.Visible = True
    pGrafico.Axes.Left.AxisPen.Width = 1
    pGrafico.Axes.Bottom.AxisPen.Width = 1
    pGrafico.Axes.Bottom.Labels.RoundFirstLabel = False
    pGrafico.Aspect.View3D = False
    'pGrafico.Aspec = System.Drawing.Drawing2D.SmoothingMode.HighSpeed
    pGrafico.Panel.Gradient.Visible = False
    '//tChart1.Walls.Back.Gradient.Visible = false;
    pGrafico.Walls.Back.Visible = False

    '// set ordering to none, to increment speed when adding points

    '// initialize axis scales
    'tChart1.Axes.Bottom.SetMinMax(1, maxpoints);



    Dim Serie(Intervalli.Count - 1) As Steema.TeeChart.WPF.Styles.Points
    pGrafico.Header.Text = CanaleAscissa.LongName
    pGrafico.Header.Alignment = TextAlignment.Center
    Dim IDi As Integer = 0
    For Each Intervallo As clsTimeRange In Intervalli
      Dim RigaIniziale, RigaFinale As Integer
      IndiciIntervallo(Intervallo, RigaIniziale, RigaFinale)
      For Each CanaleOrdinata As clsChannel In CanaliOrdinata
        Serie(IDi) = New Steema.TeeChart.WPF.Styles.Points(pGrafico.Chart)
        Serie(IDi).Title = "[" & IDi & "] " & Intervallo.StringaPeriodo & "  " & CanaleOrdinata.LongName
        Serie(IDi).Legend.Visible = True
        Serie(IDi).XValues.Order = Steema.TeeChart.Styles.ValueListOrder.None
        Serie(IDi).Pointer.Style = Steema.TeeChart.Styles.PointerStyles.SmallDot
        For Indice As Integer = RigaIniziale To RigaFinale
          If CanaleFiltro.Values(Indice) >= valoreMinimo Then
            Dim X As Double = CanaleAscissa.Values(Indice)
            Dim Y As Double = CanaleOrdinata.Values(Indice)
            Serie(IDi).Add(X, Y)
          End If
        Next
      Next
    Next



  End Sub

  Public Sub DrawConsistency(Risultato As clsRisultatoGroupBy)

    pGrafico.Chart.Series.Clear()
    Dim SerieBase As New Steema.TeeChart.WPF.Styles.Bar(pGrafico.Chart)
    SerieBase.Title = Risultato.CanaleGroupBy.LongName
    'SerieBase.Legend.Visible = False
    Dim SerieSecondarie(Risultato.ValoriGroupBy.First.DettagliCanaliSecondari.Count - 1) As Steema.TeeChart.WPF.Styles.Candle
    Dim SerieSecondarieLinea(Risultato.ValoriGroupBy.First.DettagliCanaliSecondari.Count - 1) As Steema.TeeChart.WPF.Styles.Line
    'Dim SerieSecondarieHL(Risultato.ValoriGroupBy.First.DettagliCanaliSecondari.Count - 1) As Steema.TeeChart.WPF.Styles.HighLow
    Dim AssiSecondari(Risultato.ValoriGroupBy.First.DettagliCanaliSecondari.Count - 1) As Steema.TeeChart.WPF.Axis

    pGrafico.Panel.MarginUnits = Steema.TeeChart.WPF.PanelMarginUnits.Pixels

    For Each Gruppo As clsValoriGroupBy In Risultato.ValoriGroupBy
      Dim X As Double = Gruppo.DefinizioneGruppo
      Dim Xavg As Double = Gruppo.DettagliCanaleRaggruppamento.Valori.Average
      Dim Campioni As Integer = Gruppo.IndiciIntervallo.Count
      Dim Y As Double = Campioni
      SerieBase.Add(X, Y, "g" & X)
      SerieBase.Marks.Visible = False


      For id As Integer = 0 To Gruppo.DettagliCanaliSecondari.Count - 1
        Dim Dettaglio As clsDettagliCanale = Gruppo.DettagliCanaliSecondari(id)
        Dim Mediana As Double = Dettaglio.Statistics.Median
        Dim Best90 As Double = Dettaglio.Statistics.RightOfMode90
        Dim Worst90 As Double = Dettaglio.Statistics.LeftOfMode90
        Dim Best70 As Double = Dettaglio.Statistics.RightOfMode70
        Dim Worst70 As Double = Dettaglio.Statistics.LeftOfMode70
        Dim Moda As Double = Dettaglio.Statistics.ModaMedia
        Dim StdDev As Double = Dettaglio.Statistics.StandardDeviation
        Dim Media As Double = Dettaglio.Valori.Average
        Dim Massimo As Double = Dettaglio.Valori.Max
        Dim Minimo As Double = Dettaglio.Valori.Min
        If SerieSecondarieLinea(id) Is Nothing Then
          SerieSecondarieLinea(id) = New Steema.TeeChart.WPF.Styles.Line(pGrafico.Chart)
          SerieSecondarieLinea(id).Title = Dettaglio.Canale.LongName

          SerieSecondarie(id) = New Steema.TeeChart.WPF.Styles.Candle(pGrafico.Chart)
          SerieSecondarie(id).Color = SerieSecondarieLinea(id).Color
          SerieSecondarie(id).Legend.Visible = False


          'SerieSecondarieHL(id) = New Steema.TeeChart.WPF.Styles.HighLow(pGrafico.Chart)
          'SerieSecondarieHL(id).Color = SerieSecondarieLinea(id).Color
          'SerieSecondarieHL(id).Legend.Visible = False

          AssiSecondari(id) = New Steema.TeeChart.WPF.Axis(pGrafico.Chart)
          pGrafico.Axes.Custom.Add(AssiSecondari(id))
          pGrafico(id).CustomVertAxis = pGrafico.Axes.Custom(id)
          pGrafico.Axes.Custom(id).AxisPen.Color = pGrafico(id).Color
          pGrafico.Axes.Custom(id).Grid.Visible = False
          pGrafico.Axes.Custom(id).Title.Visible = True
          pGrafico.Axes.Custom(id).Title.Caption = "Series" + id.ToString()
          pGrafico.Axes.Custom(id).PositionUnits = Steema.TeeChart.WPF.PositionUnits.Pixels
          pGrafico.Axes.Custom(id).RelativePosition = 100 + id * (pGrafico.Axes.Custom(id).MaxLabelsWidth + pGrafico.Axes.Custom(id).Ticks.Length + pGrafico.Axes.Custom(id).Title.Width)
          SerieSecondarie(id).FillSampleValues(21)
        End If

        SerieSecondarie(id).Add(Xavg, Best90, Best70, Worst70, Worst90)

        SerieSecondarieLinea(id).Add(Xavg, Moda)
        'SerieSecondarieHL(id).Add(Xavg, L, H)

        'SerieSecondarie(id).Add(Xavg, A, C, H, L)
        'SerieSecondarie(id).Add(A, C, H, L, Xavg)
      Next
    Next

  End Sub

  Public Sub DrawPolar(Polare As clsTabellePolari)

  End Sub

  Private Sub pGrafico_ClickSeries(sender As Object, s As Series, valueIndex As Integer, e As MouseEventArgs) Handles pGrafico.ClickSeries
    'Stop
  End Sub

  Private Sub pGrafico_ClickBackground(sender As Object, e As MouseEventArgs) Handles pGrafico.ClickBackground
    'Stop
  End Sub

  Private Sub pGrafico_MouseUp(sender As Object, e As MouseButtonEventArgs)
    'Stop
  End Sub
End Class
