Imports System.Collections.ObjectModel
Imports Newtonsoft.Json
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Model.DataSeries.Heatmap2DArrayDataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries

Public Class UserControlCrossoverTwsTwa

  Public Property VM As UserControlCrossoverTwsTwaViewModel

  Public Sub New()
    'Me.DataContext = VM
    'If AppConfig.ActiveProfile.CrossoverSettings Is Nothing Then
    '    AppConfig.ActiveProfile.CrossoverSettings = New clsCrossoverSettings
    '    AppConfig.ActiveProfile.CrossoverSettings.ImpostaValoriDefault()
    'End If
    '' This call is required by the designer.
    'VM.Settings.Impostazioni()

    InitializeComponent()

    ' --- Nulla di ciò che segue deve girare nel designer XAML ---
    If IsInDesignMode Then Exit Sub

    VM = New UserControlCrossoverTwsTwaViewModel

    Me.DataContext = VM
    If AppConfig.ActiveProfile.CrossoverSettings Is Nothing Then
      AppConfig.ActiveProfile.CrossoverSettings = New clsCrossoverSettings
      AppConfig.ActiveProfile.CrossoverSettings.ImpostaValoriDefault()
    End If
    ' This call is required by the designer.
    VM.Settings.Impostazioni()


    ' Add any initialization after the InitializeComponent() call.

  End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        VM.AggiornaPuntiEtGrafico()
        'aaa.DataSeries = VM.FastLineDataSeries
        'bbb.DataSeries = VM.FastMountainDataSeries
        'ccc.DataSeries = VM.ScatterDataSeries
        'ddd.DataSeries = VM.HeatMapDataSeries
        Plot.ZoomExtents()
        AggiornaGrafico3D()

    End Sub

    Private Sub DoubleUpDown_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        VM.AggiornaGrafico()
        If Not Plot Is Nothing Then Plot.ZoomExtents()
        AggiornaGrafico3D()
    End Sub

    Private Sub Plot_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Plot.ZoomExtents()
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim ImgTmp = Plot.ExportToBitmapSource()
        VM.SaveImage(ImgTmp, Plot)
    End Sub

    Private Sub btn_YzoomIn_Click(sender As Object, e As RoutedEventArgs)
        Plot.YAxes.First.ZoomBy(-0.1, -0.1)
    End Sub

    Private Sub btn_YzoomOut_Click(sender As Object, e As RoutedEventArgs)
        Plot.YAxes.First.ZoomBy(0.1, 0.1)
    End Sub

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        'Dim DataRange As New Media3D.Rect3D(0, 0, 0, 180, 40, 120)
        'Dim displayedDataBounds = Plot3D.ResetBoxCrossover(DataRange)
        'Plot3D.PlottaGraficoXYZ(VM.Punti, displayedDataBounds, DataRange, 0.3)
    End Sub

    Private Sub AggiornaGrafico3D()
        If Plot3D Is Nothing Then Exit Sub
        Dim PuntiFiltrati As New List(Of clsXYZColore)
        For Each punto In VM.Punti
            If punto.Z >= (VM.Settings.FilterChannelAvg - VM.Settings.FilterChannelRange) AndAlso punto.Z <= (VM.Settings.FilterChannelAvg + VM.Settings.FilterChannelRange) Then
                PuntiFiltrati.Add(punto)
            End If
        Next
        Dim DataRange As New Media3D.Rect3D(0, 0, 0, 180, 40, 120)
        Dim displayedDataBounds = Plot3D.ResetBoxGraficoXYZ(DataRange, 5, 5, 5)
        Plot3D.PlottaGraficoXYZ(PuntiFiltrati, displayedDataBounds, DataRange, 0.3)
    End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class HeatmapSettings
    Public Property HeatMapType As String
    Public Property HeatMapTypes As New List(Of String)
    Public Property HeatMapMinTwa As Integer = 20
    Public Property HeatMapMaxTwa As Integer = 160
    Public Property HeatMapMinTws As Integer = 4
    Public Property HeatMapMaxTws As Integer = 30
    Public Property HeatMapMinAwaColor As Integer = 15
    Public Property HeatMapMaxAwaColor As Integer = 110
    Public Property HeatMapRevAwaColor As Boolean = False
    Public Property HeatMapMinAwsColor As Integer = 5
    Public Property HeatMapMaxAwsColor As Integer = 50
    Public Property HeatMapRevAwsColor As Boolean = True
    Public Property HeatMapMinAwsYColor As Integer = 0
    Public Property HeatMapMaxAwsYColor As Integer = 40
    Public Property HeatMapRevAwsYColor As Boolean = True

    <JsonIgnore>
    Public Property HeatMapMinColor As Integer
    <JsonIgnore>
    Public Property HeatMapMaxColor As Integer
    <JsonIgnore>
    Public Property HeatMapRevColor As Integer


    Public Sub ImpostazioniIniziali()
        HeatMapTypes.Clear()
        HeatMapTypes.Add("None")
        HeatMapTypes.Add("AwaTgt")
        HeatMapTypes.Add("AwsTgt")
        HeatMapTypes.Add("AwsYTgt")
        HeatMapTypes.Add("DrivingForce")
        HeatMapTypes.Add("LateralForce")
        'HeatMapTypes.Add("HeelTgt")
        'HeatMapTypes.Add("Awa")
        'HeatMapTypes.Add("Aws")
        'HeatMapTypes.Add("Heel")
        'HeatMapTypes.Add("Lift")
        'HeatMapTypes.Add("LiftFwd")
        'HeatMapTypes.Add("LiftSide")
        HeatMapType = "None"


        HeatMapMinTwa = 20
        HeatMapMaxTwa = 160
        HeatMapMinTws = 4
        HeatMapMaxTws = 30
        HeatMapMinAwaColor = 15
        HeatMapMaxAwaColor = 110
        HeatMapRevAwaColor = False
        HeatMapMinAwsColor = 5
        HeatMapMaxAwsColor = 50
        HeatMapRevAwsColor = True
        HeatMapMinAwsYColor = 0
        HeatMapMaxAwsYColor = 40
        HeatMapRevAwsYColor = True

    End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class UserControlCrossoverSettings

    Public Property DataSource As String
    Public Property DataSources As New List(Of String)
    Public Property HeatMapSettings As New HeatmapSettings


    Public Property WindFilterVisibility As Visibility = Visibility.Collapsed

    Public Property ShowTarget As Boolean
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.ShowTarget
        End Get
        Set(value As Boolean)
            AppConfig.ActiveProfile.CrossoverSettings.ShowTarget = value
        End Set
    End Property

    Public Property ShowAwaTarget As Boolean
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.ShowAwaTarget
        End Get
        Set(value As Boolean)
            AppConfig.ActiveProfile.CrossoverSettings.ShowAwaTarget = value
        End Set
    End Property

    Public Property ShowAwsTarget As Boolean
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.ShowAwsTarget
        End Get
        Set(value As Boolean)
            AppConfig.ActiveProfile.CrossoverSettings.ShowAwsTarget = value
        End Set
    End Property



    Public Property ColorByChannel As String
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.ColorByChannel
        End Get
        Set(value As String)
            AppConfig.ActiveProfile.CrossoverSettings.ColorByChannel = value
        End Set
    End Property

    Public Property ColorByChannels As New List(Of String)

    Dim _PlotDistribution As Boolean = False
    Public Property PlotDistribution As Boolean
        Get
            Return _PlotDistribution
        End Get
        Set(value As Boolean)
            _PlotDistribution = value
            If value = True Then
                WindFilterVisibility = Visibility.Visible
            Else
                WindFilterVisibility = Visibility.Collapsed
            End If
        End Set
    End Property

    Public Property VmcMode As Boolean

    Public Property FilterChannelAvg As Double = 100

    Public Property FilterChannelRange As Double = 10

    Public Property TwsAvg As Double = 11

    Public Property TwsRange As Double = 1

    Public Property TwaAvg As Double = 50

    Public Property TwaRange As Double = 3

    Public Property FilterChannel As String
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.FilterChannel
        End Get
        Set(value As String)
            AppConfig.ActiveProfile.CrossoverSettings.FilterChannel = value
        End Set
    End Property

    Public Function NomeCanaleColorBy() As String
        If ColorByChannel = "Keys" Then Return ""
        Return ColorByChannel.Replace("Channel_", "")
    End Function

    Public Property FilterChannels As New List(Of String)

    Public Sub Impostazioni()
        FilterChannels.Clear()
        For Each n In AppConfig.ActiveProfile.CrossoverSettings.FilterChannels
            FilterChannels.Add(n)
        Next
        FilterChannel = FilterChannels.First

        ColorByChannels.Clear()
        ColorByChannels.Add("Keys")
        ColorByChannels.Add("VMGp")
        ColorByChannels.Add("BSPp")
        For Each n In AppConfig.ActiveProfile.CrossoverSettings.ColorByChannels
            ColorByChannels.Add("Channel_" & n)
        Next
        ColorByChannel = ColorByChannels.First



        DataSources.Clear()
        DataSources.Add("AllVmgStraightLines")
        DataSources.Add("SelectedVmgStraightLines")
        DataSources.Add("AllReachingStraightLines")
        DataSources.Add("SelectedReachingStraightLines")
        DataSources.Add("AllStraightLines")
        DataSources.Add("SelectedStraightLines")
        DataSources.Add("AllStraightLinesOfVisibleRange")
        DataSources.Add("VisibleRange")

        DataSource = "SelectedStraightLines"

        If HeatMapSettings.HeatMapTypes.Count = 0 Then
            'HeatMapSettings.HeatMapTypes.Clear()
            'HeatMapSettings.HeatMapTypes.Add("None")
            'HeatMapSettings.HeatMapTypes.Add("AwaTgt")
            'HeatMapSettings.HeatMapTypes.Add("AwsTgt")
            'HeatMapSettings.HeatMapTypes.Add("HeelTgt")
            'HeatMapSettings.HeatMapTypes.Add("Awa")
            'HeatMapSettings.HeatMapTypes.Add("Aws")
            'HeatMapSettings.HeatMapTypes.Add("Heel")
            ''HeatMapTypes.Add("Lift")
            ''HeatMapTypes.Add("LiftFwd")
            ''HeatMapTypes.Add("LiftSide")
            'HeatMapSettings.HeatMapType = "None"

            HeatMapSettings.ImpostazioniIniziali()
            AppConfig.Salva()
        End If

    End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class UserControlCrossoverTwsTwaViewModel
    Public Property Punti As New List(Of clsXYZColore)
    Public Property Chiavi As New Dictionary(Of String, Color)
    Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
    'Public Property FastLineDataSeries As New ObservableCollection(Of FastLineRenderableSeries)
    'Public Property FastMountainDataSeries As New ObservableCollection(Of FastMountainRenderableSeries)
    'Public Property ScatterDataSeries As New ObservableCollection(Of XyScatterRenderableSeries)
    'Public Property HeatMapDataSeries As New ObservableCollection(Of IDataSeries)
    'Public Property FastLineDataSeries As New ObservableCollection(Of IDataSeries)
    'Public Property FastMountainDataSeries As New ObservableCollection(Of IDataSeries)
    'Public Property ScatterDataSeries As New ObservableCollection(Of IDataSeries)
    'Public Property HeatMapDataSeries As New ObservableCollection(Of IDataSeries)


    Public Property Settings As New UserControlCrossoverSettings
    Public Property ImagesCollection As New List(Of System.IO.Stream)

    Dim LiftAndDragVectorTable As clsLiftAndDragVectorTable


    Public Property Ascissa As String
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.Ascissa
        End Get
        Set(value As String)
            AppConfig.ActiveProfile.CrossoverSettings.Ascissa = value
        End Set
    End Property

    Public Property Titolo As String
    Public Property ScalaColore As String

    Public Property Ordinata As String
        Get
            Return AppConfig.ActiveProfile.CrossoverSettings.Ordinata
        End Get
        Set(value As String)
            AppConfig.ActiveProfile.CrossoverSettings.Ordinata = value
        End Set
    End Property

    Private Function ChannelExtendedName(Name As String) As String
        If DataProvider2020 Is Nothing Then Return Name
        Dim c = DataProvider2020.CanaleDbl(Name)
        If c Is Nothing Then
            Select Case Name
                Case "VMGp"
                    Return "Vmg Tgt %"
                Case "BSPp"
                    Return "Bs Tgt %"
                Case Else
                    Return Name
            End Select
        Else
            Return c.ShortName
        End If
    End Function

    Public Sub AggiornaPuntiEtGrafico()
        RiempiListaPunti()
        AggiornaGrafico()
        AppConfig.Salva()
    End Sub

    Public Sub AggiornaGrafico()
        'FastLineDataSeries.Clear()
        'FastMountainDataSeries.Clear()
        'ScatterDataSeries.Clear()
        'HeatMapDataSeries.Clear()

        'If SeriesSource.Count = 0 Then Exit Sub
        SeriesSource.Clear()
        If Settings.PlotDistribution Then
            DisegnaPuntiDistribution()
        Else
            'Settings.HeatMapSettings.HeatMapType = "AwaTgt"
            DisegnaHeatMap()
            DisegnaPunti()
            StampaTarget()
        End If
        ScalaColore = "Colored By: " & ChannelExtendedName(Settings.ColorByChannel)
    End Sub

    Dim _CollectImages As Boolean
    Public Property CollectImages As Boolean
        Get
            Return _CollectImages
        End Get
        Set(value As Boolean)
            _CollectImages = value
            GestisciImagesCollector()
        End Set
    End Property

    Private Sub GestisciImagesCollector()
        If _CollectImages Then
            ImagesCollection.Clear()
        Else
            StampaPdfDaImagesCollection()
        End If
    End Sub

    Private Sub StampaPdfDaImagesCollection()
        If ImagesCollection.Count > 0 Then
            Dim pdf As New clsPdf
            pdf.StampaReportTwsTwa(ImagesCollection)
            ImagesCollection.Clear()
        End If

    End Sub

    Public Sub SaveImage(Image As BitmapSource, Surface As SciChart.Charting.Visuals.SciChartSurface)
        Clipboard.SetImage(Image)
        If _CollectImages Then
            ImagesCollection.Add(Surface.ExportToStream(SciChart.Core.ExportType.Bmp, False))
        Else
            MsgBox("Chart Copied to Clipboard")
        End If

    End Sub


    Private Sub RiempiListaPunti()
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chFiltro As clsChannel2020 = DataProvider2020.CanaleDbl(Settings.FilterChannel)
        Dim TR As clsTimeRange = Nothing
        Dim Periodi As New List(Of clsPeriod2021)
        Select Case Settings.DataSource
            Case "AllVmgStraightLinesSettings."
                Periodi = PeriodsManager.ListaStraightLineVmg.ToList
            Case "SelectedVmgStraightLines"
                Periodi = PeriodsManager.ListaStraightLineVmg.Where(Function(x) x.IsChecked).ToList
            Case "AllReachingStraightLines"
                Periodi = PeriodsManager.ListaStraightLineReaching.ToList
            Case "SelectedReachingStraightLines"
                Periodi = PeriodsManager.ListaStraightLineReaching.Where(Function(x) x.IsChecked).ToList
            Case "AllStraightLines"
                Periodi = PeriodsManager.ListaStraightLine.ToList
            Case "SelectedStraightLines"
                Periodi = PeriodsManager.ListaStraightLine.Where(Function(x) x.IsChecked).ToList
            Case "AllStraightLinesOfVisibleRange"
                Periodi = PeriodsManager.ListaStraightLine.Where(Function(x) x.TR.IsOverlapped(DataPlotSync.VisibleRange)).ToList
            Case Else
                TR = DataPlotSync.VisibleRange
        End Select


        Punti.Clear()
        If Periodi.Count > 0 Then
            Chiavi.Clear()
            Dim ColoraPerChiave As Boolean = Settings.NomeCanaleColorBy = ""
            Dim chColorBy As clsChannel2020 = DataProvider2020.CanaleDbl(Settings.NomeCanaleColorBy)
            For Each p In Periodi
                If (Not ColoraPerChiave And Not chColorBy Is Nothing) OrElse Not p.Keys.Trim = "" Then
                    For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
                        Dim c As Color
                        If ColoraPerChiave Then
                            If Not Chiavi.ContainsKey(p.Keys) Then
                                Chiavi.Add(p.Keys, ColoriDifferenziati(Chiavi.Count))
                            End If
                            c = Chiavi(p.Keys)
                        Else
                            Select Case Settings.NomeCanaleColorBy
                                Case "VMGp", "BSPp"
                                    Dim chiave As String = ""
                                    c = ColoreDaPerformance(chColorBy.Valori(i), chiave)
                                    If Not chiave = "" Then
                                        If Not Chiavi.ContainsKey(chiave) Then
                                            Chiavi.Add(chiave, c)
                                        End If
                                    End If
                                Case Else
                                    Dim v = chColorBy.Valori(i)
                                    If Double.IsNaN(v) Then
                                        c = Colors.Transparent
                                        'If Not Chiavi.ContainsKey(c.ToString) Then
                                        '  Chiavi.Add(c.ToString, c)
                                        'End If
                                    Else
                                        If Not Chiavi.ContainsKey(v.ToString) Then
                                            Chiavi.Add(v.ToString, ColoriDifferenziati(Chiavi.Count))
                                        End If
                                        c = Chiavi(v.ToString)
                                    End If
                            End Select
                        End If
                        'Dim c As Color = Chiavi(p.Keys)
                        Dim NP = NuovoPunto(i, chTwa.Valori(i), chTws.Valori(i), chFiltro.Valori(i), c)
                        If Not NP Is Nothing Then
                            Punti.Add(NP)
                            If Settings.VmcMode Then
                                If Math.Abs(chTwa.Valori(i)) <= 90 Then
                                    For a As Integer = Math.Abs(chTwa.Valori(i)) To 90 Step 1
                                        Dim k As Double = Math.Cos(Radians(Math.Abs(a - Math.Abs(chTwa.Valori(i)))))
                                        Dim NPVMC = NuovoPunto(i, a, chTws.Valori(i), chFiltro.Valori(i) * k, c)
                                        If Not NPVMC Is Nothing Then Punti.Add(NPVMC)
                                    Next
                                Else
                                    For a As Integer = 90 To Math.Abs(chTwa.Valori(i)) Step 1
                                        Dim k As Double = Math.Cos(Radians(Math.Abs(a - Math.Abs(chTwa.Valori(i)))))
                                        Dim NPVMC = NuovoPunto(i, a, chTws.Valori(i), chFiltro.Valori(i) * k, c)
                                        If Not NPVMC Is Nothing Then Punti.Add(NPVMC)
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If
            Next

        ElseIf Not TR Is Nothing Then
            Dim chColorBy As clsChannel2020 = DataProvider2020.CanaleDbl(Settings.NomeCanaleColorBy)
            If Not chColorBy Is Nothing Then
                For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
                    Dim v As Double = chColorBy.Valori(i)
                    If Not Double.IsNaN(v) Then
                        Dim ColoreDaCanale As Color
                        Select Case Settings.NomeCanaleColorBy
                            Case "VMGp", "BSPp"
                                v = CInt(chColorBy.Valori(i))
                                If v < 90 Then v = 90
                                If v > 120 Then v = 120
                                ColoreDaCanale = ColoreDaPerformance(v)
                                If Not Chiavi.ContainsKey(v.ToString) Then
                                    Chiavi.Add(v.ToString, ColoreDaCanale)
                                End If
                            Case Else
                                v = chColorBy.Valori(i)
                                If Double.IsNaN(v) Then
                                    ColoreDaCanale = Colors.Transparent
                                Else
                                    If Not Chiavi.ContainsKey(v.ToString) Then
                                        Chiavi.Add(v.ToString, ColoriDifferenziati(Chiavi.Count))
                                    End If
                                    ColoreDaCanale = Chiavi(v.ToString)
                                End If
                        End Select
                        If Not Chiavi.ContainsKey(v.ToString) Then
                            Chiavi.Add(v.ToString, ColoreDaCanale)
                        End If
                        Dim NP = NuovoPunto(i, chTwa.Valori(i), chTws.Valori(i), chFiltro.Valori(i), ColoreDaCanale)
                        If Not NP Is Nothing Then Punti.Add(NP)
                        'Dim NP = NuovoPunto(i, chTwa.Valori(i), chTws.Valori(i), chFiltro.Valori(i), ColoreDaCanale)
                        'If Not NP Is Nothing Then
                        '    Punti.Add(NP)
                        '    If Settings.VmcMode Then
                        '        If Math.Abs(chTwa.Valori(i)) <= 90 Then
                        '            For a As Integer = Math.Abs(chTwa.Valori(i)) To 90 Step 1
                        '                Dim k As Double = Math.Cos(Radians(Math.Abs(a - Math.Abs(chTwa.Valori(i)))))
                        '                Dim NPVMC = NuovoPunto(i, a, chTws.Valori(i), chFiltro.Valori(i) * k, ColoreDaCanale)
                        '                If Not NPVMC Is Nothing Then Punti.Add(NPVMC)
                        '            Next
                        '        Else
                        '            For a As Integer = 90 To Math.Abs(chTwa.Valori(i)) Step 1
                        '                Dim k As Double = Math.Cos(Radians(Math.Abs(a - Math.Abs(chTwa.Valori(i)))))
                        '                Dim NPVMC = NuovoPunto(i, a, chTws.Valori(i), chFiltro.Valori(i) * k, ColoreDaCanale)
                        '                If Not NPVMC Is Nothing Then Punti.Add(NPVMC)
                        '            Next
                        '        End If
                        '    End If
                        'End If
                    End If
                Next
            End If
        End If


    End Sub




    Private Function NuovoPunto(Id As Integer, x As Double, y As Double, z As Double, c As Color) As clsXYZColore
        If Double.IsNaN(x) OrElse Double.IsNaN(x) OrElse Double.IsNaN(x) Then Return Nothing
        Return New clsXYZColore(Id, Math.Abs(x), Math.Abs(y), z, c)
        'Dim t As Byte = Math.Min(255, (z / 100 * 255))
        'Return New clsPuntoCrossover(Math.Abs(x), Math.Abs(y), z, Color.FromArgb(t, c.R, c.G, c.B))
    End Function

    Private Function Ordine(Valore As String) As String
        If IsNumeric(Valore) Then
            Return Valore.PadLeft(5, "0")
        Else
            Return Valore
        End If
    End Function


    Private Sub DisegnaPunti()
        Ordinata = "Tws"
        Ascissa = "Twa"
        Titolo = ChannelExtendedName(Settings.FilterChannel) & " between " & Settings.FilterChannelAvg - Settings.FilterChannelRange & " and " & Settings.FilterChannelAvg + Settings.FilterChannelRange & " %"
        'ScatterDataSeries.Clear()
        'SeriesSource.Clear()
        For Each chiave In Chiavi.OrderBy(Function(x) Ordine(x.Key)).ToList
            Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
            Dim LineaTmp As New XyScatterRenderableSeries
            LineaTmp.XAxisId = "DefaultAxisId"
            LineaTmp.YAxisId = "DefaultAxisId"
            LineaTmp.PointMarker = New EllipsePointMarker
            LineaTmp.PointMarker.Stroke = chiave.Value
            LineaTmp.PointMarker.Width = 4
            LineaTmp.PointMarker.StrokeThickness = 0
            LineaTmp.PointMarker.Fill = chiave.Value
            LineaTmp.IsSelected = True
            DataSeriesTMP.SeriesName = chiave.Key
            DataSeriesTMP.AcceptsUnsortedData = True
            Dim lista = Punti.Where(Function(x) x.Colore = chiave.Value).ToList
            For Each punto In lista
                If punto.Z >= (Settings.FilterChannelAvg - Settings.FilterChannelRange) AndAlso punto.Z <= (Settings.FilterChannelAvg + Settings.FilterChannelRange) Then
                    DataSeriesTMP.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
                End If
            Next
            LineaTmp.DataSeries = DataSeriesTMP
            Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
            'ScatterDataSeries.Add(DataSeriesTMP)

            SeriesSource.Add(CSVMtmp)
        Next

    End Sub


    Private Function MappaColori() As HeatmapColorPalette
        Dim tmp As New HeatmapColorPalette
        tmp.Maximum = Settings.HeatMapSettings.HeatMapMaxColor
        tmp.Minimum = Settings.HeatMapSettings.HeatMapMinColor
        Dim stp As Integer = (tmp.Maximum - tmp.Minimum) / 30
        tmp.GradientStops.Add(New GradientStop(Colors.Transparent, Settings.HeatMapSettings.HeatMapMinColor))
        Dim vero As Boolean = True
        Dim k As Double
        For x As Integer = (tmp.Minimum) To tmp.Maximum Step stp
            k = x / (tmp.Maximum - tmp.Minimum)
            If Settings.HeatMapSettings.HeatMapRevColor Then
                k = 1 - k
            End If
            Dim c = ColoreBeneMale(100, k)
            If vero Then c = Colors.Transparent
            tmp.GradientStops.Add(New GradientStop(c, x))
            vero = Not vero
        Next

        k = 1
        If Settings.HeatMapSettings.HeatMapRevColor Then
            k = 0
        End If

        tmp.GradientStops.Add(New GradientStop(ColoreBeneMale(100, k), Settings.HeatMapSettings.HeatMapMaxColor + stp))
        Return tmp
    End Function

    Private Sub DisegnaHeatMap()
        If TgtManager Is Nothing Then Exit Sub
        If TgtManager.Tgt Is Nothing Then Exit Sub
        Dim DS = PuntiUniformHeatMap()
        If DS Is Nothing Then Exit Sub
        Dim AreaTmp As New FastUniformHeatmapRenderableSeries
        'Dim AreaTmp As New FastUniformContourRenderableSeries
        AreaTmp.XAxisId = "DefaultAxisId"
        AreaTmp.YAxisId = "DefaultAxisId"
        AreaTmp.IsSelected = False
        AreaTmp.DataSeries = DS
        AreaTmp.ColorMap = MappaColori()
        Dim CSVMtmp As New ChartSeriesViewModel(DS, AreaTmp)
        SeriesSource.Add(CSVMtmp)

    End Sub

    Private Function PuntiUniformHeatMap() As IDataSeries
        Dim Dati = ValoriDaTipo()
        If Dati Is Nothing Then Return Nothing
        Dim xStart As Double = Settings.HeatMapSettings.HeatMapMinTwa
        Dim xStep As Double = 1
        Dim yStart As Double = Settings.HeatMapSettings.HeatMapMinTws
        Dim yStep As Double = 1
        Return New UniformHeatmapDataSeries(Of Double, Double, Double)(Dati, xStart, xStep, yStart, yStep)
    End Function


    Private Function ValoriDaTipo() As Double(,)
        Select Case Settings.HeatMapSettings.HeatMapType
            Case "AwaTgt"
                Settings.HeatMapSettings.HeatMapMinColor = Settings.HeatMapSettings.HeatMapMinAwaColor
                Settings.HeatMapSettings.HeatMapMaxColor = Settings.HeatMapSettings.HeatMapMaxAwaColor
                Settings.HeatMapSettings.HeatMapRevColor = Settings.HeatMapSettings.HeatMapRevAwaColor
                Return TabellaAwa(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
            Case "AwsTgt"
                Settings.HeatMapSettings.HeatMapMinColor = Settings.HeatMapSettings.HeatMapMinAwsColor
                Settings.HeatMapSettings.HeatMapMaxColor = Settings.HeatMapSettings.HeatMapMaxAwsColor
                Settings.HeatMapSettings.HeatMapRevColor = Settings.HeatMapSettings.HeatMapRevAwsColor
                Return TabellaAws(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
            Case "AwsYTgt"
                Settings.HeatMapSettings.HeatMapMinColor = Settings.HeatMapSettings.HeatMapMinAwsYColor
                Settings.HeatMapSettings.HeatMapMaxColor = Settings.HeatMapSettings.HeatMapMaxAwsYColor
                Settings.HeatMapSettings.HeatMapRevColor = Settings.HeatMapSettings.HeatMapRevAwsYColor
                Return TabellaAwsY(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
            Case "DrivingForce"
                Dim tmp = TabellaDrivingForce(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
                Dim Array As New List(Of Double)
                'Dim Txt As New List(Of String)
                For s As Integer = 0 To tmp.GetLength(0) - 1
                    Dim r As String = ""
                    For a As Integer = 0 To tmp.GetLength(1) - 1
                        Array.Add(tmp(s, a))
                        'r &= tmp(s, a) & vbTab
                    Next
                    'Txt.Add(r)
                Next
                'Clipboard.SetText(String.Join(vbCrLf, Txt))
                Dim MinVal = Array.Min
                Dim MaxVal = Array.Max
                MinVal = MathNet.Numerics.Statistics.Statistics.Percentile(Array.ToArray, 5)
                MaxVal = MathNet.Numerics.Statistics.Statistics.Percentile(Array.ToArray, 95)
                Settings.HeatMapSettings.HeatMapMinColor = MinVal
                Settings.HeatMapSettings.HeatMapMaxColor = MaxVal
                Settings.HeatMapSettings.HeatMapRevColor = True
                Return tmp
            Case "LateralForce"
                Dim tmp = TabellaLateralForce(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
                Dim Array As New List(Of Double)
                For s As Integer = 0 To tmp.GetLength(0) - 1
                    For a As Integer = 0 To tmp.GetLength(1) - 1
                        Array.Add(tmp(s, a))
                    Next
                Next
                Dim MinVal = MathNet.Numerics.Statistics.Statistics.Percentile(Array.ToArray, 5)
                Dim MaxVal = MathNet.Numerics.Statistics.Statistics.Percentile(Array.ToArray, 95)
                Settings.HeatMapSettings.HeatMapMinColor = MinVal
                Settings.HeatMapSettings.HeatMapMaxColor = MaxVal
                Settings.HeatMapSettings.HeatMapRevColor = True
                Return tmp
                'Case "LateralForce"
                '  Settings.HeatMapSettings.HeatMapMinColor = Settings.HeatMapSettings.HeatMapMinAwsYColor
                '  Settings.HeatMapSettings.HeatMapMaxColor = Settings.HeatMapSettings.HeatMapMaxAwsYColor
                '  Settings.HeatMapSettings.HeatMapRevColor = Settings.HeatMapSettings.HeatMapRevAwsYColor
                '  Return TabellaLateralForce(Settings.HeatMapSettings.HeatMapMinTwa, Settings.HeatMapSettings.HeatMapMaxTwa, Settings.HeatMapSettings.HeatMapMinTws, Settings.HeatMapSettings.HeatMapMaxTws)
            Case Else
                'If punto.Z >= (Settings.FilterChannelAvg - Settings.FilterChannelRange) AndAlso punto.Z <= (Settings.FilterChannelAvg + Settings.FilterChannelRange) Then

                'End If
                Return Nothing
        End Select
    End Function

    'Private Function ValoreDaTipo(Twa As Double, Tws As Double) As Double
    '  'HeatMapTypes.Add("AwaTgt")
    '  'HeatMapTypes.Add("AwsTgt")
    '  'HeatMapTypes.Add("HeelTgt")
    '  'HeatMapTypes.Add("Awa")
    '  'HeatMapTypes.Add("Aws")
    '  'HeatMapTypes.Add("Heel")
    '  Select Case Settings.HeatMapType
    '    Case "AwaTgt"
    '      Return TgtManager.Tgt.ValoreTgt(Twa < 90, Tws, "bs").Awa
    '    Case "AwsTgt"
    '      Return TgtManager.Tgt.ValoreTgt(Twa < 90, Tws, "bs").Aws
    '    Case Else
    '      'If punto.Z >= (Settings.FilterChannelAvg - Settings.FilterChannelRange) AndAlso punto.Z <= (Settings.FilterChannelAvg + Settings.FilterChannelRange) Then

    '      'End If
    '      Return Double.NaN
    '  End Select
    'End Function


    Public Function TabellaAwa(MinTwa As Double, MaxTwa As Double, MinTws As Double, MaxTws As Double) As Double(,)
        Dim ss As Integer = MaxTws - MinTws + 1
        Dim aa As Integer = MaxTwa - MinTwa + 1
        Dim tmp(ss, aa) As Double
        For tws As Double = MinTws To MaxTws Step 1
            For twa As Double = MinTwa To MaxTwa Step 1
                Dim polspd = TgtManager.Tgt.Polare("bs").PolarValue(tws, twa)
                Dim a As Double
                Dim s As Double
                ApparentFromTrue(a, s, twa, tws, polspd)
                tmp(tws - MinTws, twa - MinTwa) = a
            Next
        Next
        Return tmp
    End Function

    Public Function TabellaAws(MinTwa As Double, MaxTwa As Double, MinTws As Double, MaxTws As Double) As Double(,)
        Dim ss As Integer = MaxTws - MinTws + 1
        Dim aa As Integer = MaxTwa - MinTwa + 1
        Dim tmp(ss, aa) As Double
        For tws As Double = MinTws To MaxTws Step 1
            For twa As Double = MinTwa To MaxTwa Step 1
                Dim polspd = TgtManager.Tgt.Polare("bs").PolarValue(tws, twa)
                Dim a As Double
                Dim s As Double
                ApparentFromTrue(a, s, twa, tws, polspd)
                tmp(tws - MinTws, twa - MinTwa) = s
            Next
        Next
        Return tmp
    End Function

    Public Function TabellaAwsY(MinTwa As Double, MaxTwa As Double, MinTws As Double, MaxTws As Double) As Double(,)
        ' PowerZoneReport
        Dim ss As Integer = MaxTws - MinTws + 1
        Dim aa As Integer = MaxTwa - MinTwa + 1
        Dim tmp(ss, aa) As Double
        For tws As Double = MinTws To MaxTws Step 1
            For twa As Double = MinTwa To MaxTwa Step 1
                Dim polspd = TgtManager.Tgt.Polare("bs").PolarValue(tws, twa)
                Dim a As Double
                Dim s As Double
                ApparentFromTrue(a, s, twa, tws, polspd)
                tmp(tws - MinTws, twa - MinTwa) = s * Math.Sin(Radians(a))
            Next
        Next
        Return tmp
    End Function

    Public Function TabellaDrivingForce(MinTwa As Double, MaxTwa As Double, MinTws As Double, MaxTws As Double) As Double(,)
        ' PowerZoneReport
        Dim ss As Integer = MaxTws - MinTws + 1
        Dim aa As Integer = MaxTwa - MinTwa + 1
        Dim tmp(ss, aa) As Double
        If LiftAndDragVectorTable Is Nothing Then
            LiftAndDragVectorTable = New clsLiftAndDragVectorTable(True)
        End If
        For tws As Double = MinTws To MaxTws Step 1
            For twa As Double = MinTwa To MaxTwa Step 1
                tmp(tws - MinTws, twa - MinTwa) = LiftAndDragVectorTable.LiftAndDragValues(tws, twa).DrivingForce
            Next
        Next
        Return tmp
    End Function

    Public Function TabellaLateralForce(MinTwa As Double, MaxTwa As Double, MinTws As Double, MaxTws As Double) As Double(,)
        ' PowerZoneReport
        Dim ss As Integer = MaxTws - MinTws + 1
        Dim aa As Integer = MaxTwa - MinTwa + 1
        Dim tmp(ss, aa) As Double
        If LiftAndDragVectorTable Is Nothing Then
            LiftAndDragVectorTable = New clsLiftAndDragVectorTable(True)
        End If
        For tws As Double = MinTws To MaxTws Step 1
            For twa As Double = MinTwa To MaxTwa Step 1
                tmp(tws - MinTws, twa - MinTwa) = LiftAndDragVectorTable.LiftAndDragValues(tws, twa).LateralForce
            Next
        Next
        Return tmp
    End Function

    'Public Sub ImpostaTabelleAwaAws(ByRef TabellaAwa As Double(,), ByRef TabellaAws As Double(,))
    '  Dim minTws As Double = TgtManager.Tgt.TgtRowValues.Min(Function(x) x.Value)
    '  Dim maxTws As Double = TgtManager.Tgt.TgtRowValues.Max(Function(x) x.Value)
    '  For tws As Double = minTws To maxTws Step 1
    '    For twa As Double = 0 To 180 Step 1
    '      Dim polspd = TgtManager.Tgt.Polare("bs").PolarValue(tws, twa)
    '      Dim a As Double
    '      Dim s As Double
    '      ApparentFromTrue(a, s, twa, tws, polspd)
    '      TabellaAwa(twa, tws) = a
    '      TabellaAws(twa, tws) = s
    '    Next
    '  Next
    'End Sub


    Private Sub DisegnaPuntiDistribution()
        Dim Intervalli As Integer = 15
        Ordinata = ""
        Ascissa = Settings.FilterChannel
        Titolo = ChannelExtendedName(Settings.FilterChannel) & " between " & Settings.FilterChannelAvg - Settings.FilterChannelRange & " and " & Settings.FilterChannelAvg + Settings.FilterChannelRange & " %"
        Titolo &= ", Tws between " & Settings.TwsAvg - Settings.TwsRange & " and " & Settings.TwsAvg + Settings.TwsRange & "kts"
        Titolo &= ", Twa between " & Settings.TwaAvg - Settings.TwaRange & " and " & Settings.TwaAvg + Settings.TwaRange & "°"

        'SeriesSource.Clear()
        'FastMountainDataSeries.Clear()

        For Each chiave In Chiavi
            Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
            Dim LineaTmp As New FastMountainRenderableSeries
            LineaTmp.XAxisId = "DefaultAxisId"
            LineaTmp.YAxisId = "DefaultAxisId"
            LineaTmp.Stroke = chiave.Value
            LineaTmp.StrokeThickness = 2
            LineaTmp.IsSelected = True
            Dim Colore As New SolidColorBrush(LineaTmp.Stroke)
            Colore.Opacity = 0.2
            LineaTmp.Fill = Colore
            DataSeriesTMP.SeriesName = chiave.Key
            DataSeriesTMP.AcceptsUnsortedData = True
            Dim Vals = Punti.Where(Function(x) x.Colore = chiave.Value _
                                     AndAlso x.Z >= (Settings.FilterChannelAvg - Settings.FilterChannelRange) AndAlso x.Z <= (Settings.FilterChannelAvg + Settings.FilterChannelRange) _
                                     AndAlso x.Y >= (Settings.TwsAvg - Settings.TwsRange) AndAlso x.Y <= (Settings.TwsAvg + Settings.TwsRange) _
                                     AndAlso x.X >= (Settings.TwaAvg - Settings.TwaRange) AndAlso x.X <= (Settings.TwaAvg + Settings.TwaRange)
                                   ).Select(Function(x) x.Z).ToList
            If Vals.Count > 4 Then
                Dim xx As New List(Of Double)
                Dim yy As New List(Of Double)
                Dim min = Vals.Min
                Dim max = Vals.Max
                Dim d = max - min
                Dim h As Double = d / Intervalli
                Dim vx As Double = min - h
                Dim vy As Double = 0
                xx.Add(vx)
                yy.Add(vy)
                For ii As Integer = 0 To Intervalli
                    vx = min + (h * ii)
                    vy = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(vx, h, Vals)
                    xx.Add(vx)
                    yy.Add(vy)
                Next
                xx.Add(max + h)
                yy.Add(0)
                Dim Tot As Double = yy.Sum
                For ii As Integer = 0 To yy.Count - 1
                    yy(ii) = yy(ii) / Tot * 100
                    DataSeriesTMP.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
                Next
                LineaTmp.DataSeries = DataSeriesTMP
                'FastMountainDataSeries.Add(LineaTmp)
                Dim CSVMstbd As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
                SeriesSource.Add(CSVMstbd)
            End If

        Next


    End Sub

    Public Sub StampaTarget()
        If Not AppConfig.ActiveProfile.CrossoverSettings.ShowTarget Then Exit Sub
        If TgtManager Is Nothing Then Exit Sub
        If TgtManager.Tgt Is Nothing Then Exit Sub

        Dim DataSeriesTgtUp As New XyDataSeries(Of Double, Double)
        Dim LineaTgtUp As New FastLineRenderableSeries
        LineaTgtUp.XAxisId = "DefaultAxisId"
        LineaTgtUp.YAxisId = "DefaultAxisId"
        LineaTgtUp.Stroke = Colors.DeepSkyBlue
        LineaTgtUp.StrokeThickness = 6
        'LineaTgtUp.StrokeDashArray = {3, 3}
        LineaTgtUp.Tag = "TgtUp"
        LineaTgtUp.IsVisible = True
        LineaTgtUp.IsSelected = True
        DataSeriesTgtUp.AcceptsUnsortedData = True
        DataSeriesTgtUp.SeriesName = LineaTgtUp.Tag

        Dim DataSeriesTgtDn As New XyDataSeries(Of Double, Double)
        Dim LineaTgtDn As New FastLineRenderableSeries
        LineaTgtDn.XAxisId = "DefaultAxisId"
        LineaTgtDn.YAxisId = "DefaultAxisId"
        LineaTgtDn.Stroke = Colors.DarkBlue
        LineaTgtDn.StrokeThickness = 4
        'LineaTgtDn.StrokeDashArray = {2, 2}
        LineaTgtDn.Tag = "TgtDn"
        LineaTgtDn.IsVisible = True
        LineaTgtDn.IsSelected = True
        DataSeriesTgtDn.AcceptsUnsortedData = True
        DataSeriesTgtDn.SeriesName = LineaTgtDn.Tag

        'FastLineDataSeries.Clear()

        Dim MaxY As Double = SeriesSource.Max(Function(x) x.DataSeries.YMax)
        Dim MinY As Double = SeriesSource.Min(Function(x) x.DataSeries.YMin)
        If Not MaxY = MinY AndAlso MaxY > 2 Then
            For tws As Integer = Int(MinY) To (Int(MaxY) + 1)
                Dim twaUp As Double = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Twa
                Dim twaDn As Double = TgtManager.Tgt.ValoreTgtDn(tws, "bs").Twa
                DataSeriesTgtUp.Append(twaUp, tws, New clsPuntoMetadata(False))
                DataSeriesTgtDn.Append(twaDn, tws, New clsPuntoMetadata(False))
            Next
            LineaTgtUp.DataSeries = DataSeriesTgtUp
            'FastLineDataSeries.Add(DataSeriesTgtUp)
            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTgtUp, LineaTgtUp)
            SeriesSource.Add(CSVMtgtup)
            LineaTgtDn.DataSeries = DataSeriesTgtDn
            'FastLineDataSeries.Add(DataSeriesTgtDn)
            Dim CSVMtgtDn As New ChartSeriesViewModel(DataSeriesTgtDn, LineaTgtDn)
            SeriesSource.Add(CSVMtgtDn)
        End If


        Dim MaxX As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
        Dim MinX As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
        MaxY = SeriesSource.Max(Function(x) x.DataSeries.YMax)
        MinY = SeriesSource.Min(Function(x) x.DataSeries.YMin)

        VerificaStampaAwaTarget(MinX, MaxX, MinY, MaxY)
        VerificaStampaAwsTarget(MinX, MaxX, MinY, MaxY)

    End Sub


    Private Sub VerificaStampaAwaTarget(MinX As Double, MaxX As Double, MinY As Double, MaxY As Double)

        If Settings.ShowAwaTarget Then
            If TgtManager.Tgt.AwaIsolines.Count = 0 Then
                TgtManager.Tgt.ImpostaAwaAwsIsolines()
            End If
            'Dim MaxX As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
            'Dim MinX As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
            'Dim MaxY As Double = SeriesSource.Max(Function(x) x.DataSeries.YMax)
            'Dim MinY As Double = SeriesSource.Min(Function(x) x.DataSeries.YMin)
            For Each aw In TgtManager.Tgt.AwaIsolines
                Dim DataSeries As New XyDataSeries(Of Double, Double)
                Dim Linea As New FastLineRenderableSeries
                Linea.XAxisId = "DefaultAxisId"
                Linea.YAxisId = "DefaultAxisId"
                Linea.Stroke = Colors.Gray
                Linea.StrokeThickness = 2
                Linea.Tag = "Awa " & aw.RefVal
                Linea.IsVisible = True
                DataSeries.AcceptsUnsortedData = True
                DataSeries.SeriesName = Linea.Tag
                Linea.IsSelected = False
                For Each v In aw.ValoriTwaTws
                    If v.X >= MinX AndAlso v.X <= MaxX AndAlso v.Y >= MinY AndAlso v.Y <= MaxY Then
                        DataSeries.Append(v.X, v.Y, New clsPuntoMetadata(False))
                    End If
                Next
                Linea.DataSeries = DataSeries
                'FastLineDataSeries.Add(Linea)
                Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
                SeriesSource.Add(CSVM)
            Next

        End If


    End Sub

    Private Sub VerificaStampaAwsTarget(MinX As Double, MaxX As Double, MinY As Double, MaxY As Double)

        If Settings.ShowAwsTarget Then
            If TgtManager.Tgt.AwsIsolines.Count = 0 Then
                TgtManager.Tgt.ImpostaAwaAwsIsolines()
            End If
            'Dim MaxX As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
            'Dim MinX As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
            'Dim MaxY As Double = SeriesSource.Max(Function(x) x.DataSeries.YMax)
            'Dim MinY As Double = SeriesSource.Min(Function(x) x.DataSeries.YMin)
            For Each aw In TgtManager.Tgt.AwsIsolines
                Dim DataSeries As New XyDataSeries(Of Double, Double)
                Dim Linea As New FastLineRenderableSeries
                Linea.XAxisId = "DefaultAxisId"
                Linea.YAxisId = "DefaultAxisId"
                Linea.Stroke = Colors.Gray
                Linea.StrokeThickness = 2
                Linea.Tag = "Aws " & aw.RefVal
                Linea.IsVisible = True
                DataSeries.AcceptsUnsortedData = True
                DataSeries.SeriesName = Linea.Tag
                Linea.IsSelected = False
                For Each v In aw.ValoriTwaTws
                    If v.X >= MinX AndAlso v.X <= MaxX AndAlso v.Y >= MinY AndAlso v.Y <= MaxY Then
                        DataSeries.Append(v.X, v.Y, New clsPuntoMetadata(False))
                    End If
                Next
                Linea.DataSeries = DataSeries
                'FastLineDataSeries.Add(Linea)
                Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
                SeriesSource.Add(CSVM)
            Next

        End If


    End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsXYZColore
    Public Property Id As Integer
    Public Property X As Double
    Public Property Y As Double
    Public Property Z As Double
    Public Property Colore As Color
    Public Property ValoreHeatMap As Double

    Public Sub New()

    End Sub

    Public Sub New(Id As Integer, X As Double, Y As Double, Z As Double, Colore As Color)
        Me.Id = Id
        Me.X = X
        Me.Y = Y
        Me.Z = Z
        Me.Colore = Colore
    End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsCrossoverSettings
    Public Property Ascissa As String
    Public Property Ordinata As String
    Public Property ShowTarget As Boolean
    Public Property ShowAwaTarget As Boolean
    Public Property ShowAwsTarget As Boolean
    Public Property ColorByChannel As String
    Public Property ColorByChannels As New List(Of String)
    Public Property FilterChannel As String
    Public Property FilterChannels As New List(Of String)
    Public Property FilterChannelAvg As Double
    Public Property FilterChannelRange As Double
    Public Property TwsAvg As Double
    Public Property TwsRange As Double
    Public Property TwaAvg As Double
    Public Property TwaRange As Double


    Public Sub ImpostaValoriDefault()
        Ascissa = "Twa"
        Ordinata = "Tws"
        ShowTarget = True
        ShowAwaTarget = False
        ShowAwsTarget = False
        ColorByChannel = ""
        ColorByChannels.Clear()
        ColorByChannels.Add("MainCode")
        ColorByChannels.Add("BowspritCode")
        ColorByChannels.Add("HeadstayCode")
        ColorByChannels.Add("MidCode")
        FilterChannel = "VMGp"
        FilterChannels.Clear()
        FilterChannels.Add("VMGp")
        FilterChannels.Add("BSPp")
        FilterChannelAvg = 100
        FilterChannelRange = 10
        TwsAvg = 11
        TwsRange = 1
        TwaAvg = 50
        TwaRange = 3
    End Sub

End Class