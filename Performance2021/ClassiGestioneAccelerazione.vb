Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Drawing
Imports SPwpf
Imports SciChart.Charting.Model
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SciChart.Charting.Visuals.Annotations
Imports SciChart.Charting.Visuals.PointMarkers


'Public Class UserControlAccelerationViewModel
'  Implements INotifyPropertyChanged

'  Dim pAccSyncViewModel As New UserControlAccSyncViewModel
'  Dim pAccelerationControls As clsAccelerationControls

'  Dim pCmdAggiungiCanale As New clsComando(AddressOf AggiungiGrafico)
'  Dim pCmdExportTable As New clsComando(AddressOf ExportTable)

'  Dim pFwdLatSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
'  Dim pAnnotazioniFwdLat As New SciChart.Charting.Visuals.Annotations.AnnotationCollection

'  Dim pVmgMetersSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
'  Dim pAnnotazioniVmgMeters As New SciChart.Charting.Visuals.Annotations.AnnotationCollection

'  Dim pLateralMetersSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
'  Dim pAnnotazioniLateralMeters As New SciChart.Charting.Visuals.Annotations.AnnotationCollection

'  Dim pTimeToTakeOffSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)

'  Dim _BsAtRideHeightZeroSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)

'  Dim pBsLimite As Double = 10
'  Dim pSeconds As Double = 60

'  Dim pTitoloPlot As String = "Acceleration Paths"
'  Dim pTitoloPlot2 As String = "Acceleration Progression"
'  Dim pTitoloPlot3 As String = "Acceleration Progression"
'  Dim pTitoloPlot4 As String = "Time To Take Off"
'  Dim _TitoloPlot5 As String = "Bs @ RH Zero"

'  Dim pVisibleRangeChangedFwdLat_X As IRange
'  Dim pVisibleRangeChangedFwdLat_Y As IRange

'  Dim pVisibleRangeChangedVmgMt_X As IRange
'  Dim pVisibleRangeChangedVmgMt_Y As IRange

'  Dim pVisibleRangeChangedLatMt_X As IRange
'  Dim pVisibleRangeChangedLatMt_Y As IRange

'  Dim pCurrentPeriodDescription As String
'  Dim pPeriodDetailsFontSize As Double = 16

'  'WithEvents pPeriodsTrigger As clsPeriodsTrigger

'  Public Sub New()
'    AccSyncViewModel.ParentUserControlAccelerationViewModel = Me
'    'pPeriodsTrigger = PeriodsManager.PeriodsTrigger

'    RemoveHandler pAccSyncViewModel.PropertyChanged, AddressOf AccSyncViewModelPropertyChanged
'    AddHandler pAccSyncViewModel.PropertyChanged, AddressOf AccSyncViewModelPropertyChanged

'  End Sub

'  Private Sub AccSyncViewModelPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
'    Select Case e.PropertyName
'      Case "SharedXVisibleRange"
'        AccVisibleRange.Max = pAccSyncViewModel.SharedXVisibleRange.Max
'        AccVisibleRange.Min = pAccSyncViewModel.SharedXVisibleRange.Min
'    End Select
'  End Sub
'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Public Property PeriodsMng As clsPeriodsManager2020
'    Get
'      Return PeriodsManager
'    End Get
'    Set(value As clsPeriodsManager2020)
'      PeriodsManager = value
'      OnPropertyChanged("PeriodsMng")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedFwdLat_X As IRange
'    Get
'      Return pVisibleRangeChangedFwdLat_X
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedFwdLat_X = value
'      OnPropertyChanged("VisibleRangeChangedFwdLat_X")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedFwdLat_Y As IRange
'    Get
'      Return pVisibleRangeChangedFwdLat_Y
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedFwdLat_Y = value
'      OnPropertyChanged("VisibleRangeChangedFwdLat_Y")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedVmgMt_X As IRange
'    Get
'      Return pVisibleRangeChangedVmgMt_X
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedVmgMt_X = value
'      OnPropertyChanged("VisibleRangeChangedVmgMt_X")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedVmgMt_Y As IRange
'    Get
'      Return pVisibleRangeChangedVmgMt_Y
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedVmgMt_Y = value
'      OnPropertyChanged("VisibleRangeChangedVmgMt_Y")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedLatMt_X As IRange
'    Get
'      Return pVisibleRangeChangedLatMt_X
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedLatMt_X = value
'      OnPropertyChanged("VisibleRangeChangedLatMt_X")
'    End Set
'  End Property

'  Public Property VisibleRangeChangedLatMt_Y As IRange
'    Get
'      Return pVisibleRangeChangedLatMt_Y
'    End Get
'    Set(value As IRange)
'      pVisibleRangeChangedLatMt_Y = value
'      OnPropertyChanged("VisibleRangeChangedLatMt_Y")
'    End Set
'  End Property

'  Public ReadOnly Property CmdAggiungiCanale() As ICommand
'    Get
'      Return pCmdAggiungiCanale
'    End Get
'  End Property

'  Public ReadOnly Property CmdExportTable() As ICommand
'    Get
'      Return pCmdExportTable
'    End Get
'  End Property

'  Public Property AccSyncViewModel As UserControlAccSyncViewModel
'    Get
'      Return pAccSyncViewModel
'    End Get
'    Set(value As UserControlAccSyncViewModel)
'      pAccSyncViewModel = value
'    End Set
'  End Property

'  Public Property FwdLatSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return pFwdLatSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      pFwdLatSeriesSource = value
'      OnPropertyChanged("FwdLatSeriesSource")
'    End Set
'  End Property

'  Public Property AnnotazioniFwdLat As AnnotationCollection
'    Get
'      Return pAnnotazioniFwdLat
'    End Get
'    Set(value As AnnotationCollection)
'      pAnnotazioniFwdLat = value
'      OnPropertyChanged("AnnotazioniFwdLat")
'    End Set
'  End Property

'  Public Property VmgMetersSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return pVmgMetersSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      pVmgMetersSeriesSource = value
'      OnPropertyChanged("VmgMetersSeriesSource")
'    End Set
'  End Property

'  Public Property AnnotazioniVmgMeters As AnnotationCollection
'    Get
'      Return pAnnotazioniVmgMeters
'    End Get
'    Set(value As AnnotationCollection)
'      pAnnotazioniVmgMeters = value
'      OnPropertyChanged("AnnotazioniVmgMeters")
'    End Set
'  End Property

'  Public Property LateralMetersSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return pLateralMetersSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      pLateralMetersSeriesSource = value
'      OnPropertyChanged("LateralMetersSeriesSource")
'    End Set
'  End Property

'  Public Property TimeToTakeOffSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return pTimeToTakeOffSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      pTimeToTakeOffSeriesSource = value
'      OnPropertyChanged("TimeToTakeOffSeriesSource")
'    End Set
'  End Property

'  Public Property AnnotazioniLateralMeters As AnnotationCollection
'    Get
'      Return pAnnotazioniLateralMeters
'    End Get
'    Set(value As AnnotationCollection)
'      pAnnotazioniLateralMeters = value
'      OnPropertyChanged("AnnotazioniLateralMeters")
'    End Set
'  End Property

'  Public Property AccelerationControls As clsAccelerationControls
'    Get
'      Return pAccelerationControls
'    End Get
'    Set(value As clsAccelerationControls)
'      pAccelerationControls = value
'      OnPropertyChanged("AccelerationControls")
'    End Set
'  End Property

'  Public Property TitoloPlot As String
'    Get
'      Return pTitoloPlot
'    End Get
'    Set(value As String)
'      pTitoloPlot = value
'      OnPropertyChanged("TitoloPlot")
'    End Set
'  End Property

'  Public Property TitoloPlot2 As String
'    Get
'      Return pTitoloPlot2
'    End Get
'    Set(value As String)
'      pTitoloPlot2 = value
'      OnPropertyChanged("TitoloPlot2")
'    End Set
'  End Property

'  Public Property TitoloPlot3 As String
'    Get
'      Return pTitoloPlot3
'    End Get
'    Set(value As String)
'      pTitoloPlot3 = value
'      OnPropertyChanged("TitoloPlot3")
'    End Set
'  End Property

'  Public Property TitoloPlot4 As String
'    Get
'      Return pTitoloPlot4
'    End Get
'    Set(value As String)
'      pTitoloPlot4 = value
'      OnPropertyChanged("TitoloPlot4")
'    End Set
'  End Property

'  Public Property BsLimite As Double
'    Get
'      Return pBsLimite
'    End Get
'    Set(value As Double)
'      pBsLimite = value
'      OnPropertyChanged("BsLimite")
'    End Set
'  End Property

'  'Private Sub pPeriodsTrigger_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pPeriodsTrigger.PropertyChanged
'  '  If DataProvider2020 Is Nothing Then Exit Sub
'  '  Select Case e.PropertyName
'  '    Case "ListaInAggiornamento"
'  '      If Not pPeriodsTrigger.ListaInAggiornamento Then
'  '        AggiornaGrafici()
'  '      End If
'  '    Case "PeriodiAggiornati"
'  '      AggiornaGrafici()
'  '    Case Else
'  '  End Select
'  'End Sub


'  Public Sub VerificaImpostaControlli()
'    If AccelerationControls Is Nothing Then
'      AccelerationControls = New clsAccelerationControls("AccelerationCustomCharts", AccSyncViewModel, Me)
'    End If
'  End Sub

'  Public Sub SalvaSettaggiAccelerazioni(BsLimite As Double, TakeOffSpeed As Double, TestDuration As Double)
'    PeriodsManager.AccelerationSettings2020.BsLimite = BsLimite
'    PeriodsManager.AccelerationSettings2020.TestDuration = TestDuration
'    PeriodsManager.AccelerationSettings2020.TakeOffSpeed = TakeOffSpeed
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", BsLimite, True, False)
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", TakeOffSpeed, True, False)
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "AccTotSeconds", TestDuration, True, True)
'  End Sub

'  Public Sub AggiornaAccelerazioni()
'    LoadingProgressVisualizza()
'    For Each Accelerazione In PeriodsManager.ListaAccelerazioni
'      Accelerazione.DettagliAcceleration.ImpostaAccelerationKeyMoments(PeriodsManager.AccelerationSettings2020.BsLimite, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, PeriodsManager.AccelerationSettings2020.TestDuration, False)
'    Next
'    AggiornaGrafici()
'    LoadingProgressNascondi()
'  End Sub

'  Public Property CurrentPeriodDescription As String
'    Get
'      Return pCurrentPeriodDescription
'    End Get
'    Set(value As String)
'      pCurrentPeriodDescription = value
'      OnPropertyChanged("CurrentPeriodDescription")
'    End Set
'  End Property

'  Public ReadOnly Property PeriodDetailsFontSize As Double
'    Get
'      Return pPeriodDetailsFontSize
'    End Get
'  End Property

'  Public Property BsAtRideHeightZeroSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return _BsAtRideHeightZeroSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      _BsAtRideHeightZeroSeriesSource = value
'      OnPropertyChanged("BsAtRideHeightZeroSeriesSource")
'    End Set
'  End Property

'  Public Property TitoloPlot5 As String
'    Get
'      Return _TitoloPlot5
'    End Get
'    Set(value As String)
'      _TitoloPlot5 = value
'      OnPropertyChanged("TitoloPlot5")
'    End Set
'  End Property

'  Public Sub FontSizeSmaller()
'    pPeriodDetailsFontSize -= 1
'    OnPropertyChanged("PeriodDetailsFontSize")
'  End Sub

'  Public Sub FontSizeBigger()
'    pPeriodDetailsFontSize += 1
'    OnPropertyChanged("PeriodDetailsFontSize")
'  End Sub

'  Public Sub CurrentPeriod(Period As clsPeriod2020)
'    If Period Is Nothing Then
'      CurrentPeriodDescription = ""
'    Else
'      CurrentPeriodDescription = Period.DescrizioneMultiriga
'    End If
'  End Sub


'  Public Sub CreaReportPdf(ListaXY As List(Of SciChart.Charting.Visuals.SciChartSurface))
'    Dim Lista As New List(Of SciChart.Charting.Visuals.SciChartSurface)
'    For Each c In AccelerationControls.ListaControlli
'      Lista.Add(c.Plot)
'    Next
'    Dim objPdf As New clsPdf
'    objPdf.StampReportAccelerations(Lista, ListaXY)
'  End Sub


'  Public Sub Disposami()
'    pFwdLatSeriesSource.Clear()
'    pVmgMetersSeriesSource.Clear()
'    pLateralMetersSeriesSource.Clear()
'    If Not AccelerationControls Is Nothing Then AccelerationControls.ListaControlli.Clear()
'  End Sub

'  Public Sub CreaStatistichePedestals()
'    Clipboard.SetText(PeriodsManager.CreaStatistichePedestals_Accelerations)
'  End Sub

'  Public Sub AggiornaGrafici()
'    LoadingProgressVisualizza()
'    VerificaImpostaControlli()
'    If DataProvider2020 Is Nothing Then Exit Sub

'    If AccSyncViewModel.GroupByTack Then
'      DrawXyPlotFwdLatGroupByTack()
'      DrawXyPlotVmgMetersProgressionGroupByTack()
'      DrawXyPlotLateralMetersProgressionGroupByTack()
'      DrawXyPlotTimeToTakeOffGroupByTack()
'      DrawXyPlotBsAtRhZeroGroupByTack()
'      For Each Controllo In AccelerationControls.ListaControlli
'        Dim VM As UserControlAccPlotViewModel = DirectCast(Controllo.DataContext, UserControlAccPlotViewModel)
'        VM.DrawChartGroupByTack()
'        Controllo.Plot.ZoomExtents()
'      Next
'    Else
'      'Stop
'      DrawXyPlotFwdLat()
'      DrawXyPlotVmgMetersProgression()
'      DrawXyPlotLateralMetersProgression()
'      DrawXyPlotTimeToTakeOff()
'      DrawXyPlotBsAtRhZero()
'      For Each Controllo In AccelerationControls.ListaControlli
'        Dim VM As UserControlAccPlotViewModel = DirectCast(Controllo.DataContext, UserControlAccPlotViewModel)
'        VM.DrawChart()
'        Controllo.Plot.ZoomExtents()
'      Next
'    End If
'    LoadingProgressNascondi()
'  End Sub

'  Public Function MinCommonBs() As Double
'    Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim vMin As Double = -1
'    For Each Accelerazione In PeriodsManager.ListaAccelerazioni
'      Dim ValoriIntervallo As New clsStatisticheIntervallo(chBs)
'      ValoriIntervallo.AggiornaIntervallo(Accelerazione.TimeRange, clsGroupLines.eLineType.eDataTypeSigned)

'      If vMin = -1 Then
'        vMin = ValoriIntervallo.Min
'      Else
'        vMin = System.Math.Max(vMin, ValoriIntervallo.Min)
'      End If
'      'chBs.ValoriIntervallo.AggiornaValori(Accelerazione.TimeRange, False)
'      'If vMin = -1 Then
'      '  vMin = chBs.ValoriIntervallo.Min
'      'Else
'      '  vMin = System.Math.Max(vMin, chBs.ValoriIntervallo.Min)
'      'End If
'    Next
'    Return vMin
'  End Function

'  Private Function Visibilitá(Value As Boolean) As Boolean
'    If Value Then
'      Return Visibility.Visible
'    Else
'      Return Visibility.Hidden
'    End If
'  End Function

'  Public Sub AggiornaPeriodiVisibili()
'    ''If PeriodsManager.PeriodsTrigger.ListaInAggiornamento Then Exit Sub

'    For Each linea In FwdLatSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If IsNumeric(LineaFS.Tag) Then
'        LineaFS.IsVisible = True
'      Else
'        If Not TypeOf (LineaFS.Tag) Is String Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          LineaFS.IsVisible = Periodo.IsChecked
'        End If
'      End If
'    Next

'    For Each linea In VmgMetersSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If IsNumeric(LineaFS.Tag) Then
'        LineaFS.IsVisible = True
'      Else
'        If Not TypeOf (LineaFS.Tag) Is String Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          LineaFS.IsVisible = Periodo.IsChecked
'        End If
'      End If
'    Next

'    For Each linea In LateralMetersSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If IsNumeric(LineaFS.Tag) Then
'        LineaFS.IsVisible = True
'      Else
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        LineaFS.IsVisible = Periodo.IsChecked
'      End If
'    Next

'    For Each linea In TimeToTakeOffSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If IsNumeric(LineaFS.Tag) Then
'        LineaFS.IsVisible = True
'      Else
'        If Not TypeOf (LineaFS.Tag) Is String Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          LineaFS.IsVisible = Periodo.IsChecked
'        End If
'      End If
'    Next

'    For Each linea In BsAtRideHeightZeroSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If IsNumeric(LineaFS.Tag) Then
'        LineaFS.IsVisible = True
'      Else
'        If Not TypeOf (LineaFS.Tag) Is String Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          LineaFS.IsVisible = Periodo.IsChecked
'        End If
'      End If
'    Next

'    If Not AccelerationControls Is Nothing Then
'      For Each Controllo In AccelerationControls.ListaControlli
'        For Each linea In Controllo.Plot.RenderableSeries
'          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'          If IsNumeric(LineaFS.Tag) Then
'            LineaFS.IsVisible = True
'          Else
'            If Not TypeOf (LineaFS.Tag) Is String Then
'              Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'              LineaFS.IsVisible = Periodo.IsChecked
'            End If
'          End If
'        Next
'      Next
'    End If
'  End Sub

'  Public Sub LineaSelezionata(SailingTack As clsSailingState.eTack)
'    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
'    For Each linea In FwdLatSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      If Not IsNumeric(LineaFS.Tag) Then
'        Dim Prd As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        If Prd.IsStbd Then
'          Select Case SailingTack
'            Case clsSailingState.eTack.eBoth, clsSailingState.eTack.eStbd
'              Selezione.Add(LineaFS)
'          End Select
'        Else
'          Select Case SailingTack
'            Case clsSailingState.eTack.eBoth, clsSailingState.eTack.ePort
'              Selezione.Add(LineaFS)
'          End Select
'        End If
'      End If
'    Next
'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(Periodo As clsPeriod2020)
'    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
'    For Each linea In FwdLatSeriesSource
'      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'      Dim Prd As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'      If Prd Is Periodo Then
'        Selezione.Add(LineaFS)
'      End If
'    Next

'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(ControlloCorrente As UserControlAcceleration, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(ControlloCorrente As UserControlAcceleration, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(ControlloCorrente As UserControlAccPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(ControlloCorrente As UserControlAccPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
'    LineaSelezionata(Selezione)
'  End Sub

'  Public Sub LineaSelezionata(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
'    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
'    ColoreNonSelezionati.A = 20
'    Dim SfondoBase As Windows.Media.Color = Colors.White
'    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow

'    If Selezione.Count = 0 Then
'      For Each Controllo In AccelerationControls.ListaControlli
'        For Each linea In Controllo.Plot.RenderableSeries.ToList
'          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          If AccSyncViewModel.ColorByTack Then
'            linea.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            linea.Stroke = Periodo.Colore
'          End If
'        Next
'      Next
'      For Each linea In FwdLatSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        Periodo.ColoreSfondo = SfondoBase
'        If AccSyncViewModel.ColorByTack Then
'          LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaFS.Stroke = Periodo.Colore
'        End If
'      Next
'      For Each linea In VmgMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        If AccSyncViewModel.ColorByTack Then
'          LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaFS.Stroke = Periodo.Colore
'        End If
'      Next
'      For Each linea In LateralMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        If AccSyncViewModel.ColorByTack Then
'          LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaFS.Stroke = Periodo.Colore
'        End If
'      Next
'      For Each linea In TimeToTakeOffSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        If AccSyncViewModel.ColorByTack Then
'          LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaFS.PointMarker.Stroke = Periodo.Colore
'          LineaFS.PointMarker.Fill = Periodo.Colore
'        End If
'      Next
'      For Each linea In BsAtRideHeightZeroSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        If AccSyncViewModel.ColorByTack Then
'          LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaFS.PointMarker.Stroke = Periodo.Colore
'          LineaFS.PointMarker.Fill = Periodo.Colore
'        End If
'      Next
'    Else
'      For Each Controllo In AccelerationControls.ListaControlli
'        For Each linea In Controllo.Plot.RenderableSeries.ToList
'          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'          If Not IsNumeric(LineaFS.Tag) Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            linea.Stroke = ColoreNonSelezionati
'            For Each pp In listOk
'              If AccSyncViewModel.ColorByTack Then
'                linea.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                linea.Stroke = Periodo.Colore
'              End If
'            Next
'          End If
'        Next
'      Next
'      For Each linea In FwdLatSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'        Periodo.ColoreSfondo = ColoreNonSelezionati
'        LineaFS.Stroke = ColoreNonSelezionati
'        For Each pp In listOk
'          Periodo.ColoreSfondo = SfondoSelezionati
'          If AccSyncViewModel.ColorByTack Then
'            LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaFS.Stroke = Periodo.Colore
'          End If
'        Next
'      Next
'      For Each linea In VmgMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'        LineaFS.Stroke = ColoreNonSelezionati
'        For Each pp In listOk
'          If AccSyncViewModel.ColorByTack Then
'            LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaFS.Stroke = Periodo.Colore
'          End If
'        Next
'      Next
'      For Each linea In LateralMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'        Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'        LineaFS.Stroke = ColoreNonSelezionati
'        For Each pp In listOk
'          If AccSyncViewModel.ColorByTack Then
'            LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaFS.Stroke = Periodo.Colore
'          End If
'        Next
'      Next
'      For Each linea In TimeToTakeOffSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If TypeOf (LineaFS.Tag) Is String Then

'        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          Dim listOk2 = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'          LineaFS.PointMarker.Stroke = ColoreNonSelezionati
'          LineaFS.PointMarker.Fill = ColoreNonSelezionati
'          For Each pp In listOk2
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.PointMarker.Stroke = Periodo.Colore
'              LineaFS.PointMarker.Fill = Periodo.Colore
'            End If
'          Next
'        End If
'      Next
'      For Each linea In BsAtRideHeightZeroSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If TypeOf (LineaFS.Tag) Is String Then

'        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'          Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'          Dim listOk2 = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'          LineaFS.PointMarker.Stroke = ColoreNonSelezionati
'          LineaFS.PointMarker.Fill = ColoreNonSelezionati
'          For Each pp In listOk2
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.PointMarker.Stroke = Periodo.Colore
'              LineaFS.PointMarker.Fill = Periodo.Colore
'            End If
'          Next
'        End If
'      Next
'    End If
'  End Sub

'  Public Sub LineaSelezionata(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
'    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
'    ColoreNonSelezionati.A = 20
'    Dim SfondoBase As Windows.Media.Color = Colors.White
'    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow

'    If Selezione.Count = 0 Then
'      For Each Controllo In AccelerationControls.ListaControlli
'        For Each linea In Controllo.Plot.RenderableSeries.ToList
'          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'          If Not IsNumeric(LineaFS.Tag) Then
'            If TypeOf (LineaFS.Tag) Is String Then
'              Select Case LineaFS.Tag.ToString
'                Case "BenchUp"
'                Case "BenchDn"
'                Case "TgtUp"
'                Case "TgtDn"
'              End Select
'            ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'              Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.Stroke = Periodo.Colore
'              End If
'            End If
'          End If
'        Next
'      Next
'      For Each linea In FwdLatSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then
'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Periodo.ColoreSfondo = SfondoBase
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.Stroke = Periodo.Colore
'            End If
'          End If
'        End If
'      Next
'      For Each linea In VmgMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then

'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.Stroke = Periodo.Colore
'            End If
'          End If

'        End If
'      Next
'      For Each linea In LateralMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then

'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.Stroke = Periodo.Colore
'            End If
'          End If
'        End If
'      Next
'      For Each linea In TimeToTakeOffSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then

'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.PointMarker.Stroke = Periodo.Colore
'              LineaFS.PointMarker.Fill = Periodo.Colore
'            End If
'          End If
'        End If
'      Next
'      For Each linea In BsAtRideHeightZeroSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then

'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            If AccSyncViewModel.ColorByTack Then
'              LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaFS.PointMarker.Stroke = Periodo.Colore
'              LineaFS.PointMarker.Fill = Periodo.Colore
'            End If
'          End If

'        End If
'      Next
'    Else
'      For Each Controllo In AccelerationControls.ListaControlli
'        For Each linea In Controllo.Plot.RenderableSeries.ToList
'          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'          If Not IsNumeric(LineaFS.Tag) Then

'            If TypeOf (LineaFS.Tag) Is String Then
'              Select Case LineaFS.Tag.ToString
'                Case "BenchUp"
'                Case "BenchDn"
'                Case "TgtUp"
'                Case "TgtDn"
'              End Select
'            ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'              Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'              Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'              linea.Stroke = ColoreNonSelezionati
'              For Each pp In listOk
'                If AccSyncViewModel.ColorByTack Then
'                  LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'                Else
'                  LineaFS.Stroke = Periodo.Colore
'                End If
'              Next
'            End If

'          End If
'        Next
'      Next
'      For Each linea In FwdLatSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then

'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then

'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            Periodo.ColoreSfondo = ColoreNonSelezionati
'            LineaFS.Stroke = ColoreNonSelezionati
'            For Each pp In listOk
'              Periodo.ColoreSfondo = SfondoSelezionati
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.Stroke = Periodo.Colore
'              End If
'            Next
'          End If
'        End If
'      Next
'      For Each linea In VmgMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then
'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            LineaFS.Stroke = ColoreNonSelezionati
'            For Each pp In listOk
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.Stroke = Periodo.Colore
'              End If
'            Next
'          End If
'        End If
'      Next
'      For Each linea In LateralMetersSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then
'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            LineaFS.Stroke = ColoreNonSelezionati
'            For Each pp In listOk
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.Stroke = Periodo.Colore
'              End If
'            Next
'          End If
'        End If
'      Next
'      For Each linea In TimeToTakeOffSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then
'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk2 = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            LineaFS.PointMarker.Stroke = ColoreNonSelezionati
'            LineaFS.PointMarker.Fill = ColoreNonSelezionati
'            For Each pp In listOk2
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'                LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.PointMarker.Stroke = Periodo.Colore
'                LineaFS.PointMarker.Fill = Periodo.Colore
'              End If
'            Next
'          End If
'        End If
'      Next
'      For Each linea In BsAtRideHeightZeroSeriesSource
'        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
'        If Not IsNumeric(LineaFS.Tag) Then
'          If TypeOf (LineaFS.Tag) Is String Then
'            Select Case LineaFS.Tag.ToString
'              Case "BenchUp"
'              Case "BenchDn"
'              Case "TgtUp"
'              Case "TgtDn"
'            End Select
'          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2020 Then
'            Dim Periodo As clsPeriod2020 = DirectCast(LineaFS.Tag, clsPeriod2020)
'            Dim listOk2 = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
'            LineaFS.PointMarker.Stroke = ColoreNonSelezionati
'            LineaFS.PointMarker.Fill = ColoreNonSelezionati
'            For Each pp In listOk2
'              If AccSyncViewModel.ColorByTack Then
'                LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'                LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
'              Else
'                LineaFS.PointMarker.Stroke = Periodo.Colore
'                LineaFS.PointMarker.Fill = Periodo.Colore
'              End If
'            Next
'          End If
'        End If
'      Next
'    End If
'  End Sub

'  Public Sub OperazioniSuListaControlli(ControlloCorrente As UserControlAccPlot, Operazione As UserControlAccPlot.eTipoOperazione)
'    Dim IndiceCorrente As Integer = IndiceControlloNellaLista(AccelerationControls.ListaControlli, ControlloCorrente)
'    If IndiceCorrente = 0 Then Exit Sub ' non si possono fare operazioni sul grafico loss progression
'    Select Case Operazione
'      Case UserControlAccPlot.eTipoOperazione.eDelete
'        AccelerationControls.ListaControlli.RemoveAt(IndiceCorrente)
'      Case UserControlAccPlot.eTipoOperazione.eMoveDn
'        If IndiceCorrente < AccelerationControls.ListaControlli.Count - 1 Then
'          AccelerationControls.ListaControlli.RemoveAt(IndiceCorrente)
'          AccelerationControls.ListaControlli.Insert(IndiceCorrente + 1, ControlloCorrente)
'        End If
'      Case UserControlAccPlot.eTipoOperazione.eMoveUp
'        If IndiceCorrente > 1 Then
'          AccelerationControls.ListaControlli.RemoveAt(IndiceCorrente)
'          AccelerationControls.ListaControlli.Insert(IndiceCorrente - 1, ControlloCorrente)
'        End If
'      Case UserControlAccPlot.eTipoOperazione.eSelectChannel
'        Dim VM As UserControlAccPlotViewModel = DirectCast(AccelerationControls.ListaControlli(IndiceCorrente).DataContext, UserControlAccPlotViewModel)
'        Dim Lista As New List(Of clsChannel2020)
'        Lista.Add(VM.Canale)
'        ImpostaCanaliAsIsSelected(Lista)
'        Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, False)
'        If WPFpup.ShowDialog() Then
'          VM.Canale = WPFpup.Canale
'          VM.DrawChart()
'          AccelerationControls.ListaControlli(IndiceCorrente).Plot.ZoomExtents()
'        End If
'        ImpostaCanaliAsIsSelected(Nothing)
'    End Select
'    AccelerationControls.SalvaImpostazione()

'  End Sub

'  Public Sub ExportTable()
'    Dim FolderReport As String = SelectFolder(AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "Folders", "DailyReports", AppConfig.ApplicationDataFolder & "\DailyReports\", True, True))
'    If System.IO.Directory.Exists(FolderReport) Then
'      AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "Folders", "DailyReports", FolderReport, True, True)
'      Dim Titolo As String = DataProvider2020.TimeRange.Start.ToShortDateString & " Accelerations"
'      FolderReport &= SepDaPath(FolderReport) & TempoInStringaFormattata(DataProvider2020.TimeRange.Start, eFormatType.YYYYMMDD) & SepDaPath(FolderReport)
'      If Not System.IO.Directory.Exists(FolderReport) Then
'        System.IO.Directory.CreateDirectory(FolderReport)
'      End If
'      Dim pathFileReport As String = FolderReport & TempoInStringaFormattata(DataProvider2020.TimeRange.Start, eFormatType.YYYYMMDD) & "_AccelerationsReport.html"
'      Dim ReportHtml As New clsReportAccelerationHtml("AccelerationReportTemplate.html", False, Titolo, clsPeriod2020.ePeriodType.eAcceleration, False, pathFileReport)

'      ApriExplorer(FolderReport, pathFileReport)
'    End If
'  End Sub

'  Public Sub AggiungiGrafico()
'    ImpostaCanaliAsIsSelected(Nothing)
'    Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, False)
'    If WPFpup.ShowDialog() Then
'      AccelerationControls.AggiungiCanale(WPFpup.Canale)
'      Dim Controllo As UserControlAccPlot = AccelerationControls.ListaControlli.Last
'      Dim VM As UserControlAccPlotViewModel = DirectCast(Controllo.DataContext, UserControlAccPlotViewModel)
'      VM.DrawChart()
'      Controllo.Plot.ZoomExtents()
'    End If
'    ImpostaCanaliAsIsSelected(Nothing)

'    AccelerationControls.SalvaImpostazione()

'  End Sub


'  Public Sub DrawXyPlotFwdLat()
'    Dim CanaleLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    Dim CanaleLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)

'    FwdLatSeriesSource.Clear()

'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      Dim InizioAccel As New clsCourseMark(CanaleLat.Valori(Accel.MomentOfInizioAccelerazione.IdRiga), CanaleLng.Valori(Accel.MomentOfInizioAccelerazione.IdRiga), "Centro", clsCourseMark.eSideToBeLeft.eToMark)
'      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'      DataSeriesTMP.AcceptsUnsortedData = True
'      Dim DataSeriesOutside As New XyDataSeries(Of Double, Double)
'      DataSeriesOutside.AcceptsUnsortedData = True

'      Dim LineaTmp As New FastLineRenderableSeries
'      LineaTmp.XAxisId = "DefaultAxisId"
'      LineaTmp.YAxisId = "DefaultAxisId"
'      If AccSyncViewModel.ColorByTack Then
'        LineaTmp.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'      Else
'        LineaTmp.Stroke = Accel.Periodo.Colore
'      End If
'      LineaTmp.StrokeThickness = 3
'      If Not Accel.IsStbd Then
'        LineaTmp.StrokeDashArray = {3, 5}
'      End If
'      LineaTmp.Tag = Accel.Periodo

'      Dim SegnoStbd As Integer = If(Accel.IsStbd, -1, 1)
'      Dim SegnoUpwind As Integer = 1 ' IIf(Accel.IsUpwind, -1, 1)
'      For Riga As Integer = Accel.MomentOfInizioAccelerazione.IdRiga To Accel.MomentOfFineTestAccelerazione.IdRiga
'        Dim RigaPos As Integer = Riga - Accel.MomentOfInizioAccelerazione.IdRiga
'        If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'          If Not Accel.MatriceGeoPosRelToPuntoIniziale(RigaPos).IsEmpty Then
'            If RigaPos < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'              Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(RigaPos)
'              If Riga < Accel.MomentOfInizioAccelerazione.IdRiga Then
'                DataSeriesOutside.Append(SegnoStbd * Punto.X, SegnoUpwind * Punto.Y)
'              Else
'                DataSeriesTMP.Append(SegnoStbd * Punto.X, SegnoUpwind * Punto.Y, New clsPuntoMetadata(False))
'                DataSeriesOutside.Append(Double.NaN, Double.NaN)
'              End If
'            End If
'          End If
'        End If
'      Next

'      LineaTmp.DataSeries = DataSeriesTMP
'      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'      FwdLatSeriesSource.Add(CSVMtmp)
'    Next

'    ImpostaAnnotazioniFwdLat()

'  End Sub

'  Public Sub DrawXyPlotFwdLatGroupByTack()
'    Dim CanaleLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    Dim CanaleLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)

'    Dim ValoriXPort As New clsValoriAggregati
'    Dim ValoriXStbd As New clsValoriAggregati
'    Dim ValoriYPort As New clsValoriAggregati
'    Dim ValoriYStbd As New clsValoriAggregati
'    FwdLatSeriesSource.Clear()
'    Dim Hz As Integer = DataProvider2020.Hz
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      If PeriodsManager.ListaAccelerazioni(i).IsChecked Then
'        Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'        Dim InizioAccel As New clsCourseMark(CanaleLat.Valori(Accel.MomentOfInizioAccelerazione.IdRiga), CanaleLng.Valori(Accel.MomentOfInizioAccelerazione.IdRiga), "Centro", clsCourseMark.eSideToBeLeft.eToMark)
'        Dim SegnoStbd As Integer = If(Accel.IsStbd, -1, 1)
'        Dim SegnoUpwind As Integer = 1 ' IIf(Accel.IsUpwind, -1, 1)
'        For Riga As Integer = Accel.MomentOfInizioAccelerazione.IdRiga To Accel.MomentOfFineTestAccelerazione.IdRiga
'          Dim RigaPos As Integer = Riga - Accel.MomentOfInizioAccelerazione.IdRiga
'          If Not Accel.MatriceGeoPosRelToPuntoIniziale(RigaPos).IsEmpty Then
'            If RigaPos < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'              Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(RigaPos)
'              If Accel.IsStbd Then
'                ValoriXStbd.AggiungiCoppia(CInt(RigaPos / Hz), SegnoStbd * Punto.X)
'                ValoriYStbd.AggiungiCoppia(CInt(RigaPos / Hz), SegnoUpwind * Punto.Y)
'              Else
'                ValoriXPort.AggiungiCoppia(CInt(RigaPos / Hz), SegnoStbd * Punto.X)
'                ValoriYPort.AggiungiCoppia(CInt(RigaPos / Hz), SegnoUpwind * Punto.Y)
'              End If
'            End If
'          End If
'        Next
'      End If

'    Next

'    If ValoriXPort.Dizionario.Count > 0 Then
'      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'      DataSeriesPort.AcceptsUnsortedData = True
'      Dim LineaPort As New FastLineRenderableSeries
'      LineaPort.XAxisId = "DefaultAxisId"
'      LineaPort.YAxisId = "DefaultAxisId"
'      LineaPort.Stroke = Colors.Red
'      LineaPort.StrokeThickness = 2
'      LineaPort.Tag = -1
'      Dim DizionarioOrdinato = ValoriXPort.Dizionario.OrderBy(Function(x) x.Key)
'      For i As Integer = 0 To DizionarioOrdinato.Count - 1
'        Dim Indice As Integer = DizionarioOrdinato(i).Key
'        Dim vX As Double = ValoriXPort.Dizionario(Indice).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
'        Dim vY As Double = ValoriYPort.Dizionario(Indice).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
'        DataSeriesPort.Append(vX, vY, New clsPuntoMetadata(False))
'      Next
'      LineaPort.DataSeries = DataSeriesPort
'      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'      FwdLatSeriesSource.Add(CSVMport)
'    End If

'    If ValoriXStbd.Dizionario.Count > 0 Then
'      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'      DataSeriesStbd.AcceptsUnsortedData = True
'      Dim LineaStbd As New FastLineRenderableSeries
'      LineaStbd.XAxisId = "DefaultAxisId"
'      LineaStbd.YAxisId = "DefaultAxisId"
'      LineaStbd.Stroke = Colors.Green
'      LineaStbd.StrokeThickness = 2
'      LineaStbd.Tag = 1
'      Dim DizionarioOrdinato = ValoriXStbd.Dizionario.OrderBy(Function(x) x.Key)
'      For i As Integer = 0 To ValoriXStbd.Dizionario.Count - 1
'        Dim Indice As Integer = DizionarioOrdinato(i).Key
'        Dim vX As Double = ValoriXStbd.Dizionario(Indice).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
'        Dim vY As Double = ValoriYStbd.Dizionario(Indice).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
'        DataSeriesStbd.Append(vX, vY, New clsPuntoMetadata(False))
'      Next
'      LineaStbd.DataSeries = DataSeriesStbd
'      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'      FwdLatSeriesSource.Add(CSVMStbd)
'    End If


'    'pAnnotazioniFwdLat.Clear()
'    ' va implementata la funzione con puntatore per group by tack
'    ImpostaAnnotazioniFwdLat()

'  End Sub

'  Private Sub ImpostaAnnotazioniFwdLat()


'    pAnnotazioniFwdLat.Clear()
'    Dim a As New DoubleCollection
'    a.Add(2)
'    a.Add(2)

'    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
'    AnV.X1 = 0
'    AnV.X2 = 0
'    AnV.Y1 = 0
'    AnV.Y2 = 1
'    AnV.Stroke = New SolidColorBrush(Colors.Black)
'    AnV.StrokeDashArray = a
'    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnV.IsHidden = False
'    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnV)

'    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
'    AnH.X1 = 0
'    AnH.X2 = 1
'    AnH.Y1 = 0
'    AnH.Y2 = 0
'    AnH.Stroke = New SolidColorBrush(Colors.Black)
'    AnH.StrokeDashArray = a
'    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
'    AnH.IsHidden = False
'    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnH)


'    Dim AnWA As New SciChart.Charting.Visuals.Annotations.LineArrowAnnotation
'    AnWA.X1 = 0
'    AnWA.X2 = 0
'    AnWA.Y1 = 0.02
'    AnWA.Y2 = 0.15
'    Dim MC As New Windows.Media.Color
'    MC.A = 150
'    MC.R = 0
'    MC.G = 0
'    MC.B = 139
'    AnWA.Stroke = New SolidColorBrush(MC)
'    AnWA.StrokeThickness = 10
'    AnWA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWA.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnWA)

'    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0
'    AnWT.Y1 = 0.02
'    AnWT.Text = "Mt towards the Wind"
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 1
'    AnWT.Y1 = 0.85
'    AnWT.Text = "Mt Side"
'    AnWT.FontSize = 40
'    MC = New Windows.Media.Color
'    MC.A = 68
'    MC.R = 0
'    MC.G = 0
'    MC.B = 0
'    AnWT.Foreground = New SolidColorBrush(MC)
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0.7
'    AnWT.Y1 = 0.0
'    AnWT.FontSize = 20
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniFwdLat.Add(AnWT)

'    Dim VBR As New Binding("AccSyncViewModel.CurrentXposition")
'    VBR.Source = Me
'    VBR.Mode = BindingMode.OneWay
'    AnWT.SetBinding(SciChart.Charting.Visuals.Annotations.TextAnnotation.TextProperty, VBR)



'    If PeriodsManager.ListaAccelerazioni.Count > 0 Then
'      For Each Accel In PeriodsManager.ListaAccelerazioni
'        Dim AnBA = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
'        AnBA.Background = New SolidColorBrush(Accel.Colore) 'Color.FromArgb(255, 255, 112, 52))
'        AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
'        AnBA.IsHidden = False
'        AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
'        AnBA.CornerRadius = New CornerRadius(5)
'        pAnnotazioniFwdLat.Add(AnBA)


'        VBR = New Binding("AccSyncViewModel.TrackX1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackX2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackY1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackY2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y2Property, VBR)


'      Next
'    End If
'  End Sub

'  Public Sub DrawXyPlotTimeToTakeOff()
'    TimeToTakeOffSeriesSource.Clear()
'    Dim Max As Double = -1
'    Dim Min As Double = 100
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then
'        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'        Dim LineaTmp As New XyScatterRenderableSeries
'        LineaTmp.XAxisId = "DefaultAxisId"
'        LineaTmp.YAxisId = "DefaultAxisId"

'        If Accel.IsStbd Then
'          LineaTmp.PointMarker = New EllipsePointMarker
'        Else
'          LineaTmp.PointMarker = New SquarePointMarker
'        End If
'        If AccSyncViewModel.ColorByTack Then
'          LineaTmp.PointMarker.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTmp.PointMarker.Stroke = Accel.Periodo.Colore
'        End If
'        LineaTmp.PointMarker.Height = 10
'        LineaTmp.PointMarker.Width = 10
'        LineaTmp.PointMarker.StrokeThickness = 1
'        If AccSyncViewModel.ColorByTack Then
'          LineaTmp.PointMarker.Fill = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTmp.PointMarker.Fill = Accel.Periodo.Colore
'        End If
'        LineaTmp.Tag = Accel.Periodo

'        DataSeriesTMP.AcceptsUnsortedData = True
'        Dim x As Double = Accel.Periodo.ValoriCanaleTWS.Avg
'        Dim y As Double = Accel.TimeToTakeOffSpeed.TotalSeconds
'        DataSeriesTMP.Append(x, y, New clsPuntoMetadata(False))
'        Min = System.Math.Min(x, Min)
'        Max = System.Math.Max(x, Max)
'        LineaTmp.DataSeries = DataSeriesTMP
'        Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'        TimeToTakeOffSeriesSource.Add(CSVMtmp)
'      End If
'    Next

'    DisegnaBenchmarks(Min, Max)

'  End Sub

'  Public Sub DrawXyPlotTimeToTakeOffGroupByTack()

'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    Dim TwsPort As New List(Of Double)
'    Dim TwsStbd As New List(Of Double)
'    Dim Max As Double = -1
'    Dim Min As Double = 100
'    TimeToTakeOffSeriesSource.Clear()
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then

'        Dim X As Double = Accel.Periodo.ValoriCanaleTWS.Avg
'        Dim Y As Double = Accel.TimeToTakeOffSpeed.TotalSeconds

'        If Accel.IsStbd Then
'          ValoriStbd.AggiungiCoppia(0, Y)
'          TwsStbd.Add(X)
'        Else
'          ValoriPort.AggiungiCoppia(0, Y)
'          TwsPort.Add(X)
'        End If
'        Min = System.Math.Min(X, Min)
'        Max = System.Math.Max(X, Max)

'      End If
'    Next

'    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'    DataSeriesPort.AcceptsUnsortedData = True
'    Dim LineaPort As New XyScatterRenderableSeries
'    LineaPort.XAxisId = "DefaultAxisId"
'    LineaPort.YAxisId = "DefaultAxisId"
'    LineaPort.PointMarker = New EllipsePointMarker()
'    LineaPort.PointMarker.Stroke = Colors.Red
'    LineaPort.PointMarker.Height = 10
'    LineaPort.PointMarker.Width = 10
'    LineaPort.PointMarker.StrokeThickness = 1
'    LineaPort.PointMarker.Fill = Colors.Red
'    LineaPort.Tag = -1
'    For Each Valore In ValoriPort.Dizionario
'      DataSeriesPort.Append(TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'    Next
'    LineaPort.DataSeries = DataSeriesPort
'    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'    TimeToTakeOffSeriesSource.Add(CSVMport)


'    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'    DataSeriesStbd.AcceptsUnsortedData = True
'    Dim LineaStbd As New XyScatterRenderableSeries
'    LineaStbd.XAxisId = "DefaultAxisId"
'    LineaStbd.YAxisId = "DefaultAxisId"
'    LineaStbd.PointMarker = New EllipsePointMarker()
'    LineaStbd.PointMarker.Stroke = Colors.Green
'    LineaStbd.PointMarker.Height = 10
'    LineaStbd.PointMarker.Width = 10
'    LineaStbd.PointMarker.StrokeThickness = 1
'    LineaStbd.PointMarker.Fill = Colors.Green
'    LineaStbd.Tag = 1
'    For Each Valore In ValoriStbd.Dizionario
'      DataSeriesStbd.Append(TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'    Next
'    LineaStbd.DataSeries = DataSeriesStbd
'    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'    TimeToTakeOffSeriesSource.Add(CSVMStbd)

'    DisegnaBenchmarks(Min, Max)

'  End Sub

'  Private Sub DisegnaBenchmarks(MinTws As Double, MaxTws As Double)
'    If AccBenchManager Is Nothing Then Exit Sub

'    ' traccia le linee target
'    MaxTws = System.Math.Floor(MaxTws) + 1
'    MinTws = System.Math.Floor(MinTws)

'    Dim a(1) As Double
'    a(0) = 1
'    a(1) = 3

'    Dim DataSeriesAcc As New XyDataSeries(Of Double, Double)
'    DataSeriesAcc.AcceptsUnsortedData = False
'    Dim LineaAcc As New FastLineRenderableSeries
'    LineaAcc.XAxisId = "DefaultAxisId"
'    LineaAcc.YAxisId = "DefaultAxisId"
'    LineaAcc.Stroke = Colors.DarkRed
'    LineaAcc.StrokeThickness = 3
'    LineaAcc.StrokeDashArray = a
'    LineaAcc.Tag = "BenchAcc"


'    For Each elemento In AccBenchManager.ListaValori.OrderBy(Function(x) x.X).ToList
'      If elemento.X >= MinTws And elemento.X <= MaxTws Then
'        DataSeriesAcc.Append(elemento.X, elemento.Y)
'      End If
'    Next
'    LineaAcc.DataSeries = DataSeriesAcc
'    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesAcc, LineaAcc)
'    TimeToTakeOffSeriesSource.Add(CSVMStbd)

'  End Sub


'  Public Sub DrawXyPlotBsAtRhZero()
'    BsAtRideHeightZeroSeriesSource.Clear()
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then
'        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'        Dim LineaTmp As New XyScatterRenderableSeries
'        LineaTmp.XAxisId = "DefaultAxisId"
'        LineaTmp.YAxisId = "DefaultAxisId"

'        If Accel.IsStbd Then
'          LineaTmp.PointMarker = New EllipsePointMarker
'        Else
'          LineaTmp.PointMarker = New SquarePointMarker
'        End If
'        If AccSyncViewModel.ColorByTack Then
'          LineaTmp.PointMarker.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTmp.PointMarker.Stroke = Accel.Periodo.Colore
'        End If
'        LineaTmp.PointMarker.Height = 10
'        LineaTmp.PointMarker.Width = 10
'        LineaTmp.PointMarker.StrokeThickness = 1
'        If AccSyncViewModel.ColorByTack Then
'          LineaTmp.PointMarker.Fill = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTmp.PointMarker.Fill = Accel.Periodo.Colore
'        End If
'        LineaTmp.Tag = Accel.Periodo

'        DataSeriesTMP.AcceptsUnsortedData = True

'        DataSeriesTMP.Append(Accel.Periodo.ValoriCanaleTWS.Avg, Accel.BsAtMinSinkZero, New clsPuntoMetadata(False))

'        LineaTmp.DataSeries = DataSeriesTMP
'        Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'        BsAtRideHeightZeroSeriesSource.Add(CSVMtmp)
'      End If
'    Next
'  End Sub

'  Public Sub DrawXyPlotBsAtRhZeroGroupByTack()

'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    Dim TwsPort As New List(Of Double)
'    Dim TwsStbd As New List(Of Double)
'    BsAtRideHeightZeroSeriesSource.Clear()
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then

'        Dim X As Double = Accel.Periodo.ValoriCanaleTWS.Avg
'        Dim Y As Double = Accel.BsAtMinSinkZero

'        If Accel.IsStbd Then
'          ValoriStbd.AggiungiCoppia(0, Y)
'          TwsStbd.Add(X)
'        Else
'          ValoriPort.AggiungiCoppia(0, Y)
'          TwsPort.Add(X)
'        End If
'      End If
'    Next

'    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'    DataSeriesPort.AcceptsUnsortedData = True
'    Dim LineaPort As New XyScatterRenderableSeries
'    LineaPort.XAxisId = "DefaultAxisId"
'    LineaPort.YAxisId = "DefaultAxisId"
'    LineaPort.PointMarker = New EllipsePointMarker()
'    LineaPort.PointMarker.Stroke = Colors.Red
'    LineaPort.PointMarker.Height = 10
'    LineaPort.PointMarker.Width = 10
'    LineaPort.PointMarker.StrokeThickness = 1
'    LineaPort.PointMarker.Fill = Colors.Red
'    LineaPort.Tag = -1
'    For Each Valore In ValoriPort.Dizionario
'      DataSeriesPort.Append(TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'    Next
'    LineaPort.DataSeries = DataSeriesPort
'    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'    BsAtRideHeightZeroSeriesSource.Add(CSVMport)


'    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'    DataSeriesStbd.AcceptsUnsortedData = True
'    Dim LineaStbd As New XyScatterRenderableSeries
'    LineaStbd.XAxisId = "DefaultAxisId"
'    LineaStbd.YAxisId = "DefaultAxisId"
'    LineaStbd.PointMarker = New EllipsePointMarker()
'    LineaStbd.PointMarker.Stroke = Colors.Green
'    LineaStbd.PointMarker.Height = 10
'    LineaStbd.PointMarker.Width = 10
'    LineaStbd.PointMarker.StrokeThickness = 1
'    LineaStbd.PointMarker.Fill = Colors.Green
'    LineaStbd.Tag = 1
'    For Each Valore In ValoriStbd.Dizionario
'      DataSeriesStbd.Append(TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'    Next
'    LineaStbd.DataSeries = DataSeriesStbd
'    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'    BsAtRideHeightZeroSeriesSource.Add(CSVMStbd)

'  End Sub

'  Public Sub DrawXyPlotVmgMetersProgression()
'    Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim SecsAnte As Double = 10
'    VmgMetersSeriesSource.Clear()

'    Dim MaxMetriVmg As Double = 0
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'        If Accel.Periodo.IsChecked Then
'          MaxMetriVmg = System.Math.Max(MaxMetriVmg, Accel.MatriceGeoPosRelToPuntoIniziale.Last.Y)
'        End If
'      End If
'    Next

'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'        If Accel.Periodo.IsChecked Then
'          If Accel.MomentOfInizioAccelerazione.Momento.ToOADate > 0 Then

'            Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'            DataSeriesTMP.AcceptsUnsortedData = True
'            Dim DataSeriesOutside As New XyDataSeries(Of Double, Double)
'            DataSeriesOutside.AcceptsUnsortedData = True

'            Dim LineaTmp As New FastLineRenderableSeries
'            LineaTmp.XAxisId = "DefaultAxisId"
'            LineaTmp.YAxisId = "DefaultAxisId"
'            If AccSyncViewModel.ColorByTack Then
'              LineaTmp.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaTmp.Stroke = Accel.Periodo.Colore
'            End If
'            LineaTmp.StrokeThickness = 2
'            If Accel.IsStbd Then
'              LineaTmp.StrokeDashArray = {3, 3}
'            End If
'            LineaTmp.Tag = Accel.Periodo

'            Dim LineaOutside As New FastLineRenderableSeries
'            LineaOutside.XAxisId = LineaTmp.XAxisId
'            LineaOutside.YAxisId = LineaTmp.YAxisId
'            If AccSyncViewModel.ColorByTack Then
'              LineaOutside.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaOutside.Stroke = Accel.Periodo.Colore
'            End If
'            LineaOutside.StrokeThickness = 1
'            LineaOutside.StrokeDashArray = {3, 3}
'            LineaOutside.Tag = Accel.Periodo

'            Dim RigaInizioPeriodoAccelerazione As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento)
'            Dim IdTmp As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento.AddSeconds(-SecsAnte))
'            Dim BSentry As Double = CanaleTmp.Valori(IdTmp)
'            Dim BSaccstart As Double = CanaleTmp.Valori(RigaInizioPeriodoAccelerazione)
'            DataSeriesOutside.Append(-SecsAnte, (BSentry / BSaccstart) / 2 * MaxMetriVmg)
'            DataSeriesOutside.Append(0, 0)


'            Dim SegnoUpwind As Integer = If(Accel.IsUpwind, 1, -1)
'            For Riga As Integer = RigaInizioPeriodoAccelerazione To DataProvider2020.TrovaIndice(Accel.MomentOfFineTestAccelerazione.Momento)
'              Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'              Dim SS As Double = Momento.Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds '(Riga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'              Dim rigaRaw As Integer = System.Math.Max(0, Riga - RigaInizioPeriodoAccelerazione)
'              If Not Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw).IsEmpty Then
'                If rigaRaw < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'                  Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw)
'                  DataSeriesTMP.Append(SS, SegnoUpwind * Punto.Y, New clsPuntoMetadata(False))
'                End If
'              End If
'            Next

'            LineaTmp.DataSeries = DataSeriesTMP
'            Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'            VmgMetersSeriesSource.Add(CSVMtmp)

'          End If
'        End If
'      End If
'    Next

'    ImpostaAnnotazioniVmgMeters()

'  End Sub

'  Public Sub DrawXyPlotVmgMetersProgressionGroupByTack()
'    Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim SecsAnte As Double = 10
'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    VmgMetersSeriesSource.Clear()

'    Dim MaxMetriVmg As Double = 0
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then
'        MaxMetriVmg = System.Math.Max(MaxMetriVmg, Accel.MatriceGeoPosRelToPuntoIniziale.Last.Y)
'      End If
'    Next

'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.Periodo.IsChecked Then
'        Dim RigaInizioPeriodoAccelerazione As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento)
'        Dim SegnoUpwind As Integer = If(Accel.IsUpwind, 1, -1)
'        For Riga As Integer = RigaInizioPeriodoAccelerazione To DataProvider2020.TrovaIndice(Accel.MomentOfFineTestAccelerazione.Momento)
'          Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'          Dim SS As Double = Momento.Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds '(Riga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'          Dim rigaRaw As Integer = System.Math.Max(0, Riga - RigaInizioPeriodoAccelerazione)
'          If Not Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw).IsEmpty Then
'            If rigaRaw < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'              Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw)
'              If Accel.IsStbd Then
'                ValoriStbd.AggiungiCoppia(CInt(SS), SegnoUpwind * Punto.Y)
'              Else
'                ValoriPort.AggiungiCoppia(CInt(SS), SegnoUpwind * Punto.Y)
'              End If
'            End If
'          End If
'        Next
'      End If

'    Next

'    If ValoriPort.Dizionario.Count > 0 Then
'      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'      Dim LineaPort As New FastLineRenderableSeries
'      LineaPort.XAxisId = "DefaultAxisId"
'      LineaPort.YAxisId = "DefaultAxisId"
'      LineaPort.Stroke = Colors.Red
'      LineaPort.StrokeThickness = 2
'      LineaPort.Tag = -1
'      For Each Valore In ValoriPort.Dizionario
'        DataSeriesPort.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaPort.DataSeries = DataSeriesPort
'      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'      VmgMetersSeriesSource.Add(CSVMport)
'    End If

'    If ValoriStbd.Dizionario.Count > 0 Then
'      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'      Dim LineaStbd As New FastLineRenderableSeries
'      LineaStbd.XAxisId = "DefaultAxisId"
'      LineaStbd.YAxisId = "DefaultAxisId"
'      LineaStbd.Stroke = Colors.Green
'      LineaStbd.StrokeThickness = 2
'      LineaStbd.Tag = 1
'      For Each Valore In ValoriStbd.Dizionario
'        DataSeriesStbd.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaStbd.DataSeries = DataSeriesStbd
'      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'      VmgMetersSeriesSource.Add(CSVMStbd)
'    End If


'    ImpostaAnnotazioniVmgMeters()

'  End Sub

'  Public Sub DrawXyPlotLateralMetersProgression()
'    Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim SecsAnte As Double = 10
'    LateralMetersSeriesSource.Clear()

'    Dim MaxMetriVmg As Double = 0
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'        If Accel.Periodo.IsChecked Then
'          MaxMetriVmg = System.Math.Max(MaxMetriVmg, Accel.MatriceGeoPosRelToPuntoIniziale.Last.X)
'        End If
'      End If
'    Next


'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then

'        If Accel.Periodo.IsChecked Then
'          If Accel.MomentOfInizioAccelerazione.Momento.ToOADate > 0 Then
'            Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'            DataSeriesTMP.AcceptsUnsortedData = True
'            Dim DataSeriesOutside As New XyDataSeries(Of Double, Double)
'            DataSeriesOutside.AcceptsUnsortedData = True

'            Dim LineaTmp As New FastLineRenderableSeries
'            LineaTmp.XAxisId = "DefaultAxisId"
'            LineaTmp.YAxisId = "DefaultAxisId"
'            If AccSyncViewModel.ColorByTack Then
'              LineaTmp.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaTmp.Stroke = Accel.Periodo.Colore
'            End If
'            LineaTmp.StrokeThickness = 2
'            If Accel.IsStbd Then
'              LineaTmp.StrokeDashArray = {3, 3}
'            End If
'            LineaTmp.Tag = Accel.Periodo

'            Dim LineaOutside As New FastLineRenderableSeries
'            LineaOutside.XAxisId = LineaTmp.XAxisId
'            LineaOutside.YAxisId = LineaTmp.YAxisId
'            If AccSyncViewModel.ColorByTack Then
'              LineaOutside.Stroke = If(Accel.Periodo.IsStbd, Colors.Green, Colors.Red)
'            Else
'              LineaOutside.Stroke = Accel.Periodo.Colore
'            End If
'            LineaOutside.StrokeThickness = 1
'            LineaOutside.StrokeDashArray = {3, 3}
'            LineaOutside.Tag = Accel.Periodo

'            Dim RigaInizioPeriodoAccelerazione As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento)
'            Dim IdTmp As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento.AddSeconds(-SecsAnte))
'            Dim BSentry As Double = CanaleTmp.Valori(IdTmp)
'            Dim BSaccstart As Double = CanaleTmp.Valori(RigaInizioPeriodoAccelerazione)
'            DataSeriesOutside.Append(-MaxMetriVmg / 10, (BSentry / BSaccstart) / 2 * 60)
'            DataSeriesOutside.Append(0, 0)


'            Dim SegnoStbd As Integer = IIf(Accel.IsStbd, -1, 1)
'            For Riga As Integer = RigaInizioPeriodoAccelerazione To Accel.MomentOfFineTestAccelerazione.IdRiga
'              Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'              Dim SS As Double = Momento.Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds '(Riga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'              Dim rigaRaw As Integer = System.Math.Max(0, Riga - RigaInizioPeriodoAccelerazione)
'              If Not Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw).IsEmpty Then
'                If rigaRaw < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'                  Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw)
'                  DataSeriesTMP.Append(SegnoStbd * Punto.X, SS, New clsPuntoMetadata(False))
'                End If
'              End If
'            Next

'            LineaTmp.DataSeries = DataSeriesTMP
'            Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'            LateralMetersSeriesSource.Add(CSVMtmp)
'          End If
'        End If
'      End If

'    Next

'    ImpostaAnnotazioniLateralMeters()

'  End Sub

'  Public Sub DrawXyPlotLateralMetersProgressionGroupByTack()
'    Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim SecsAnte As Double = 10
'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    LateralMetersSeriesSource.Clear()

'    Dim MaxMetriVmg As Double = 0
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'        If Accel.Periodo.IsChecked Then
'          MaxMetriVmg = System.Math.Max(MaxMetriVmg, Accel.MatriceGeoPosRelToPuntoIniziale.Last.Y)
'        End If
'      End If
'    Next

'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accel As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      'Dim test As DateTime = Accel.MomentOfInizioAccelerazione.Momento
'      If Accel.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'        If Accel.Periodo.IsChecked Then
'          Dim RigaInizioPeriodoAccelerazione As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento)
'          Dim SegnoStbd As Integer = IIf(Accel.IsStbd, -1, 1)
'          'Dim Momento As DateTime = DataProvider2020.Momento(RigaInizioPeriodoAccelerazione - 10)
'          'Dim SSprev As Double = Momento.Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds '(Riga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'          For Riga As Integer = RigaInizioPeriodoAccelerazione To DataProvider2020.TrovaIndice(Accel.MomentOfFineTestAccelerazione.Momento)
'            Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'            Dim SS As Double = Momento.Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds '(Riga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'            Dim rigaRaw As Integer = System.Math.Max(0, Riga - RigaInizioPeriodoAccelerazione)
'            If Not Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw).IsEmpty Then
'              If rigaRaw < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'                Dim Punto As PointF = Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw)
'                If Accel.IsStbd Then
'                  ValoriStbd.AggiungiCoppia(CInt(SS), SegnoStbd * Punto.X)
'                Else
'                  ValoriPort.AggiungiCoppia(CInt(SS), SegnoStbd * Punto.X)
'                End If
'              End If
'            End If
'          Next
'        End If
'      End If

'    Next

'    If ValoriPort.Dizionario.Count > 0 Then
'      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'      DataSeriesPort.AcceptsUnsortedData = True
'      Dim LineaPort As New FastLineRenderableSeries

'      LineaPort.XAxisId = "DefaultAxisId"
'      LineaPort.YAxisId = "DefaultAxisId"
'      LineaPort.Stroke = Colors.Red
'      LineaPort.StrokeThickness = 2
'      LineaPort.Tag = -1
'      For Each Valore In ValoriPort.Dizionario
'        DataSeriesPort.Append(Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Key, New clsPuntoMetadata(False))
'      Next
'      LineaPort.DataSeries = DataSeriesPort
'      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'      LateralMetersSeriesSource.Add(CSVMport)
'    End If

'    If ValoriStbd.Dizionario.Count > 0 Then
'      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'      DataSeriesStbd.AcceptsUnsortedData = True
'      Dim LineaStbd As New FastLineRenderableSeries
'      LineaStbd.XAxisId = "DefaultAxisId"
'      LineaStbd.YAxisId = "DefaultAxisId"
'      LineaStbd.Stroke = Colors.Green
'      LineaStbd.StrokeThickness = 2
'      LineaStbd.Tag = 1
'      For Each Valore In ValoriStbd.Dizionario
'        DataSeriesStbd.Append(Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Key, New clsPuntoMetadata(False))
'      Next
'      LineaStbd.DataSeries = DataSeriesStbd
'      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'      LateralMetersSeriesSource.Add(CSVMStbd)
'    End If


'    ImpostaAnnotazioniLateralMeters()

'  End Sub

'  Private Sub ImpostaAnnotazioniVmgMeters()


'    pAnnotazioniVmgMeters.Clear()
'    Dim a As New DoubleCollection
'    a.Add(2)
'    a.Add(2)

'    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
'    AnV.X1 = 0
'    AnV.X2 = 0
'    AnV.Y1 = 0
'    AnV.Y2 = 1
'    AnV.Stroke = New SolidColorBrush(Colors.Black)
'    AnV.StrokeDashArray = a
'    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnV.IsHidden = False
'    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnV)

'    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
'    AnH.X1 = 0
'    AnH.X2 = 1
'    AnH.Y1 = 0
'    AnH.Y2 = 0
'    AnH.Stroke = New SolidColorBrush(Colors.Black)
'    AnH.StrokeDashArray = a
'    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
'    AnH.IsHidden = False
'    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnH)


'    Dim AnWA As New SciChart.Charting.Visuals.Annotations.LineArrowAnnotation
'    AnWA.X1 = 0
'    AnWA.X2 = 0
'    AnWA.Y1 = 0.02
'    AnWA.Y2 = 0.15
'    Dim MC As New Windows.Media.Color
'    MC.A = 150
'    MC.R = 0
'    MC.G = 0
'    MC.B = 139
'    AnWA.Stroke = New SolidColorBrush(MC)
'    AnWA.StrokeThickness = 10
'    AnWA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWA.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnWA)

'    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0
'    AnWT.Y1 = 0.02
'    AnWT.Text = "Vmg Meters"
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 1
'    AnWT.Y1 = 0.85
'    AnWT.Text = "Seconds"
'    AnWT.FontSize = 40
'    MC = New Windows.Media.Color
'    MC.A = 68
'    MC.R = 0
'    MC.G = 0
'    MC.B = 0
'    AnWT.Foreground = New SolidColorBrush(MC)
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0.7
'    AnWT.Y1 = 0.0
'    AnWT.FontSize = 20
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniVmgMeters.Add(AnWT)

'    Dim VBR As New Binding("AccSyncViewModel.CurrentXposition")
'    VBR.Source = Me
'    VBR.Mode = BindingMode.OneWay
'    AnWT.SetBinding(SciChart.Charting.Visuals.Annotations.TextAnnotation.TextProperty, VBR)



'    If PeriodsManager.ListaAccelerazioni.Count > 0 Then
'      For Each Accel In PeriodsManager.ListaAccelerazioni
'        Dim AnBA = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
'        AnBA.Background = New SolidColorBrush(Accel.Colore) 'Color.FromArgb(255, 255, 112, 52))
'        AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
'        AnBA.IsHidden = False
'        AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
'        AnBA.CornerRadius = New CornerRadius(5)
'        pAnnotazioniVmgMeters.Add(AnBA)


'        VBR = New Binding("AccSyncViewModel.TrackVmgX1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackVmgX2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackVmgY1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackVmgY2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y2Property, VBR)


'      Next
'    End If
'  End Sub

'  Private Sub ImpostaAnnotazioniLateralMeters()

'    '    <sc:SciChartSurface.Annotations>
'    '  <sc:TextAnnotation Text="{Binding TitoloPlot3}" X1="0.02" Y1="0.85" FontSize="30" AnnotationCanvas="BelowChart" Foreground="#44000000" CoordinateMode="Relative"></sc:TextAnnotation>
'    '  <sc:TextAnnotation Text = "Seconds" X1="0.0" Y1="0.0" FontSize="20" AnnotationCanvas="BelowChart" Foreground="Gray" CoordinateMode="Relative"></sc: TextAnnotation>
'    '  <sc:LineArrowAnnotation X1="0.0" X2="0.0" Y1="0.93" Y2="0.1" CoordinateMode="RelativeY" Stroke="Gray" StrokeDashArray="2,3" StrokeThickness="2"></sc:LineArrowAnnotation>
'    '  <sc:TextAnnotation Text = "Lateral Meters" X1="0.55" Y1="0.8" FontSize="20" AnnotationCanvas="BelowChart" Foreground="Gray" CoordinateMode="Relative"></sc: TextAnnotation>
'    '  <sc:LineArrowAnnotation X1="0.07" X2="0.9" Y1="0" Y2="0" CoordinateMode="RelativeX" StrokeDashArray="2,3" Stroke="Gray" StrokeThickness="2"></sc:LineArrowAnnotation>
'    '</sc:SciChartSurface.Annotations>


'    pAnnotazioniLateralMeters.Clear()
'    Dim a As New DoubleCollection
'    a.Add(2)
'    a.Add(2)

'    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
'    AnV.X1 = 0
'    AnV.X2 = 0
'    AnV.Y1 = 0
'    AnV.Y2 = 1
'    AnV.Stroke = New SolidColorBrush(Colors.Black)
'    AnV.StrokeDashArray = a
'    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnV.IsHidden = False
'    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnV)

'    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
'    AnH.X1 = 0
'    AnH.X2 = 1
'    AnH.Y1 = 0
'    AnH.Y2 = 0
'    AnH.Stroke = New SolidColorBrush(Colors.Black)
'    AnH.StrokeDashArray = a
'    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
'    AnH.IsHidden = False
'    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnH)


'    Dim AnWA As New SciChart.Charting.Visuals.Annotations.LineArrowAnnotation
'    AnWA.X1 = 0
'    AnWA.X2 = 0
'    AnWA.Y1 = 0.02
'    AnWA.Y2 = 0.15
'    Dim MC As New Windows.Media.Color
'    MC.A = 150
'    MC.R = 0
'    MC.G = 0
'    MC.B = 139
'    AnWA.Stroke = New SolidColorBrush(MC)
'    AnWA.StrokeThickness = 10
'    AnWA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWA.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnWA)

'    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0
'    AnWT.Y1 = 0.02
'    AnWT.Text = "Seconds"
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 1
'    AnWT.Y1 = 0.85
'    AnWT.Text = "Lateral Meters"
'    AnWT.FontSize = 40
'    MC = New Windows.Media.Color
'    MC.A = 68
'    MC.R = 0
'    MC.G = 0
'    MC.B = 0
'    AnWT.Foreground = New SolidColorBrush(MC)
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnWT)

'    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
'    AnWT.X1 = 0.7
'    AnWT.Y1 = 0.0
'    AnWT.FontSize = 20
'    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
'    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
'    pAnnotazioniLateralMeters.Add(AnWT)

'    Dim VBR As New Binding("AccSyncViewModel.CurrentXposition")
'    VBR.Source = Me
'    VBR.Mode = BindingMode.OneWay
'    AnWT.SetBinding(SciChart.Charting.Visuals.Annotations.TextAnnotation.TextProperty, VBR)



'    If PeriodsManager.ListaAccelerazioni.Count > 0 Then
'      For Each Accel In PeriodsManager.ListaAccelerazioni
'        Dim AnBA = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
'        AnBA.Background = New SolidColorBrush(Accel.Colore) 'Color.FromArgb(255, 255, 112, 52))
'        AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
'        AnBA.IsHidden = False
'        AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
'        AnBA.CornerRadius = New CornerRadius(5)
'        pAnnotazioniLateralMeters.Add(AnBA)


'        VBR = New Binding("AccSyncViewModel.TrackLatX1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackLatX2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackLatY1[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y1Property, VBR)

'        VBR = New Binding("AccSyncViewModel.TrackLatY2[" & Accel.Id & "]")
'        VBR.Source = Me
'        VBR.Mode = BindingMode.TwoWay
'        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y2Property, VBR)


'      Next
'    End If
'  End Sub

'End Class

'Public Class UserControlAccPlotViewModel
'  Implements INotifyPropertyChanged
'  Dim pAccSyncViewModel As UserControlAccSyncViewModel
'  Dim pSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
'  'Dim pPCSVM As New List(Of clsPavarotChartSeriesViewModel)
'  Dim pTitolo As String
'  Dim pTitolo2 As String
'  Dim pToggleSigned As Boolean
'  Dim pValoriDerivati As Boolean
'  Dim pCanale As clsChannel2020

'  Public Sub New(AccSyncViewModel As UserControlAccSyncViewModel, Canale As clsChannel2020)
'    pAccSyncViewModel = AccSyncViewModel
'    pCanale = Canale
'  End Sub

'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub


'  Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'    Get
'      Return pSeriesSource
'    End Get
'    Set(value As ObservableCollection(Of IChartSeriesViewModel))
'      pSeriesSource = value
'      OnPropertyChanged("SeriesSource")
'    End Set
'  End Property

'  Public Property ValoriDerivati As Boolean
'    Get
'      Return pValoriDerivati
'    End Get
'    Set(value As Boolean)
'      pValoriDerivati = value
'      OnPropertyChanged("ValoriDerivati")
'      DrawChart()
'    End Set
'  End Property


'  Public Property Titolo As String
'    Get
'      Return pTitolo
'    End Get
'    Set(value As String)
'      pTitolo = value
'      OnPropertyChanged("Titolo")
'    End Set
'  End Property

'  Public Property ToggleSigned As Boolean
'    Get
'      Return pToggleSigned
'    End Get
'    Set(value As Boolean)
'      pToggleSigned = value
'      OnPropertyChanged("ToggleSigned")
'      DrawChart()
'    End Set
'  End Property

'  Public Property Canale As clsChannel2020
'    Get
'      Return pCanale
'    End Get
'    Set(value As clsChannel2020)
'      pCanale = value
'    End Set
'  End Property

'  Public Property AccSyncViewModel As UserControlAccSyncViewModel
'    Get
'      Return pAccSyncViewModel
'    End Get
'    Set(value As UserControlAccSyncViewModel)
'      pAccSyncViewModel = value
'    End Set
'  End Property

'  Public Sub DrawVmgMeters()
'    Titolo = "Vmg Meters Progression"
'    SeriesSource.Clear()
'    Dim SecBefore As Integer = 10
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accelerazione As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accelerazione.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then

'        If Accelerazione.MomentOfInizioAccelerazione.Momento.ToOADate > 0 Then


'          Dim Colore As System.Windows.Media.Color = Accelerazione.Periodo.Colore ' AccSyncViewModel.Colori(i)
'          Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'          DataSeriesTMP.AcceptsUnsortedData = True
'          Dim LineaTmp As New FastLineRenderableSeries
'          LineaTmp.XAxisId = "DefaultAxisId"
'          LineaTmp.YAxisId = "DefaultAxisId"
'          If Accelerazione.IsStbd Then
'            LineaTmp.StrokeDashArray = {3, 3}
'          End If
'          If AccSyncViewModel.ColorByTack Then
'            LineaTmp.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaTmp.Stroke = Colore
'          End If
'          LineaTmp.StrokeThickness = 2
'          LineaTmp.Tag = Accelerazione.Periodo

'          Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'          Dim LineaTO As New XyScatterRenderableSeries
'          LineaTO.XAxisId = "DefaultAxisId"
'          LineaTO.YAxisId = "DefaultAxisId"
'          If Accelerazione.IsStbd Then
'            LineaTO.PointMarker = New EllipsePointMarker
'          Else
'            LineaTO.PointMarker = New SquarePointMarker
'          End If
'          If AccSyncViewModel.ColorByTack Then
'            LineaTO.PointMarker.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaTO.PointMarker.Stroke = Colore
'          End If
'          LineaTO.PointMarker.Height = 10
'          LineaTO.PointMarker.Width = 10
'          LineaTO.PointMarker.StrokeThickness = 1
'          If AccSyncViewModel.ColorByTack Then
'            LineaTO.PointMarker.Fill = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaTO.PointMarker.Fill = Colore
'          End If
'          LineaTO.Tag = Accelerazione.Periodo

'          Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'          Dim LineaBsT As New XyScatterRenderableSeries
'          LineaBsT.XAxisId = "DefaultAxisId"
'          LineaBsT.YAxisId = "DefaultAxisId"
'          LineaBsT.PointMarker = New SquarePointMarker
'          If AccSyncViewModel.ColorByTack Then
'            LineaBsT.PointMarker.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaBsT.PointMarker.Stroke = Colore
'          End If
'          LineaBsT.PointMarker.Height = 20
'          LineaBsT.PointMarker.Width = 3
'          LineaBsT.PointMarker.StrokeThickness = 1
'          If AccSyncViewModel.ColorByTack Then
'            LineaBsT.PointMarker.Fill = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'          Else
'            LineaBsT.PointMarker.Fill = Colore
'          End If
'          LineaBsT.Tag = Accelerazione.Periodo

'          Dim IdRigaIniziale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento)
'          Dim IdRigaInizialeLoop As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento.AddSeconds(-10))
'          Dim IdRigaFinale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfFineTestAccelerazione.Momento)
'          Dim IndiceTO As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfTakeOffSpeed.Momento)
'          Dim ValoreTO As Double = Double.NaN
'          Dim IndiceBsT As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfBSatTgt.Momento)
'          Dim ValoreBsT As Double = Double.NaN
'          For Riga As Integer = IdRigaInizialeLoop To IdRigaFinale
'            Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'            Dim SecsFromKeyMoment As Double = Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds
'            Dim RigaPos As Integer = Riga - IdRigaIniziale
'            If SecsFromKeyMoment < 0 Then
'              DataSeriesTMP.Append(SecsFromKeyMoment, 0, New clsPuntoMetadata(False))
'            ElseIf RigaPos >= 0 AndAlso RigaPos < Accelerazione.MatriceGeoPosRelToPuntoIniziale.Count Then
'              If Not Accelerazione.MatriceGeoPosRelToPuntoIniziale(RigaPos).IsEmpty Then
'                Dim ValoreRiga As Double = Accelerazione.MatriceGeoPosRelToPuntoIniziale(RigaPos).Y
'                If Not Accelerazione.IsUpwind Then ValoreRiga *= -1
'                If Double.IsNaN(ValoreTO) AndAlso Riga >= IndiceTO Then
'                  ValoreTO = ValoreRiga
'                End If
'                If Double.IsNaN(ValoreBsT) AndAlso Riga >= IndiceBsT Then
'                  ValoreBsT = ValoreRiga
'                End If
'                DataSeriesTMP.Append(SecsFromKeyMoment, ValoreRiga, New clsPuntoMetadata(False))
'              End If
'            End If

'          Next

'          LineaTmp.DataSeries = DataSeriesTMP
'          Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'          pSeriesSource.Add(CSVMtmp)

'          DataSeriesTO.Append(Accelerazione.TimeToTakeOffSpeed.TotalSeconds, ValoreTO, New clsPuntoMetadata(False))
'          LineaTO.DataSeries = DataSeriesTO
'          Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'          pSeriesSource.Add(CSVMtO)

'          DataSeriesBsT.Append(Accelerazione.MomentOfBSatTgt.Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds, ValoreBsT, New clsPuntoMetadata(False))
'          LineaBsT.DataSeries = DataSeriesBsT
'          Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'          pSeriesSource.Add(CSVMBsT)
'        End If
'      End If

'    Next
'    OnPropertyChanged("SeriesSource")
'  End Sub

'  Public Sub DrawVmgMetersGroupByTack()
'    Titolo = "Vmg Meters Progression"
'    Dim SecBefore As Integer = 10


'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    SeriesSource.Clear()

'    Dim ToPort As New List(Of Double)
'    Dim ToStbd As New List(Of Double)
'    Dim BsTPort As New List(Of Double)
'    Dim BsTStbd As New List(Of Double)
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      If PeriodsManager.ListaAccelerazioni(i).IsChecked Then
'        Dim Acc As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'        If PeriodsManager.ListaAccelerazioni(i).IsStbd Then
'          ToStbd.Add(Acc.TimeToTakeOffSpeed.TotalSeconds)
'          BsTStbd.Add(Acc.MomentOfBSatTgt.Momento.Subtract(Acc.MomentOfInizioAccelerazione.Momento).TotalSeconds)
'        Else
'          ToPort.Add(Acc.TimeToTakeOffSpeed.TotalSeconds)
'          BsTPort.Add(Acc.MomentOfBSatTgt.Momento.Subtract(Acc.MomentOfInizioAccelerazione.Momento).TotalSeconds)
'        End If
'      End If
'    Next


'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      If PeriodsManager.ListaAccelerazioni(i).IsChecked Then
'        Dim Accelerazione As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'        If Accelerazione.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'          Dim Colore As System.Windows.Media.Color = Accelerazione.Periodo.Colore ' AccSyncViewModel.Colori(i)
'          Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'          Dim LineaTmp As New FastLineRenderableSeries
'          LineaTmp.XAxisId = "DefaultAxisId"
'          LineaTmp.YAxisId = "DefaultAxisId"
'          If Accelerazione.IsStbd Then
'            LineaTmp.StrokeDashArray = {3, 3}
'          End If
'          LineaTmp.Stroke = Colore
'          LineaTmp.StrokeThickness = 2
'          LineaTmp.Tag = Accelerazione.Periodo

'          Dim IdRigaIniziale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento)
'          Dim IdRigaInizialeLoop As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento.AddSeconds(-10))
'          Dim IdRigaFinale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfFineTestAccelerazione.Momento)

'          For Riga As Integer = IdRigaInizialeLoop To IdRigaFinale
'            Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'            Dim SecsFromKeyMoment As Double = Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds
'            Dim RigaPos As Integer = Riga - IdRigaIniziale
'            If SecsFromKeyMoment < 0 Then
'              If Accelerazione.IsStbd Then
'                ValoriStbd.AggiungiCoppia(SecsFromKeyMoment, 0)
'              Else
'                ValoriPort.AggiungiCoppia(SecsFromKeyMoment, 0)
'              End If
'            ElseIf RigaPos < Accelerazione.MatriceGeoPosRelToPuntoIniziale.Count Then
'              If Not Accelerazione.MatriceGeoPosRelToPuntoIniziale(RigaPos).IsEmpty Then
'                Dim ValoreRiga As Double = Accelerazione.MatriceGeoPosRelToPuntoIniziale(RigaPos).Y
'                If Not Accelerazione.IsUpwind Then ValoreRiga *= -1
'                If Accelerazione.IsStbd Then
'                  ValoriStbd.AggiungiCoppia(SecsFromKeyMoment, ValoreRiga)
'                Else
'                  ValoriPort.AggiungiCoppia(SecsFromKeyMoment, ValoreRiga)
'                End If
'              End If
'            End If
'          Next

'        End If
'      End If
'    Next

'    If ValoriPort.Dizionario.Count > 0 Then
'      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'      Dim LineaPort As New FastLineRenderableSeries
'      LineaPort.XAxisId = "DefaultAxisId"
'      LineaPort.YAxisId = "DefaultAxisId"
'      LineaPort.Stroke = Colors.Red
'      LineaPort.StrokeThickness = 2
'      LineaPort.Tag = -1
'      For Each Valore In ValoriPort.Dizionario
'        DataSeriesPort.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaPort.DataSeries = DataSeriesPort
'      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'      SeriesSource.Add(CSVMport)

'      Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'      Dim LineaTO As New XyScatterRenderableSeries
'      LineaTO.XAxisId = "DefaultAxisId"
'      LineaTO.YAxisId = "DefaultAxisId"
'      LineaTO.PointMarker = New EllipsePointMarker
'      LineaTO.PointMarker.Stroke = Colors.Red
'      LineaTO.PointMarker.Height = 10
'      LineaTO.PointMarker.Width = 10
'      LineaTO.PointMarker.StrokeThickness = 1
'      LineaTO.PointMarker.Fill = Colors.Red
'      LineaTO.Tag = -1

'      Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'      Dim LineaBsT As New XyScatterRenderableSeries
'      LineaBsT.XAxisId = "DefaultAxisId"
'      LineaBsT.YAxisId = "DefaultAxisId"
'      LineaBsT.PointMarker = New SquarePointMarker
'      LineaBsT.PointMarker.Stroke = Colors.Red
'      LineaBsT.PointMarker.Height = 20
'      LineaBsT.PointMarker.Width = 3
'      LineaBsT.PointMarker.StrokeThickness = 1
'      LineaBsT.PointMarker.Fill = Colors.Red
'      LineaBsT.Tag = -1

'      Dim SSto As Double = ToPort.Average
'      Dim SSbst As Double = BsTPort.Average
'      Dim ValTo As Double = Double.NaN
'      Dim ValBsT As Double = Double.NaN
'      For i As Integer = 0 To DataSeriesPort.XValues.Count - 1
'        Dim Valore As Integer = DataSeriesPort.XValues(i)
'        If Double.IsNaN(ValTo) AndAlso Valore >= SSto Then
'          ValTo = DataSeriesPort.YValues(i)
'        End If
'        If Double.IsNaN(ValBsT) AndAlso Valore >= SSbst Then
'          ValBsT = DataSeriesPort.YValues(i)
'        End If
'      Next

'      DataSeriesTO.Append(SSto, ValTo, New clsPuntoMetadata(False))
'      LineaTO.DataSeries = DataSeriesTO
'      Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'      pSeriesSource.Add(CSVMtO)

'      DataSeriesBsT.Append(SSbst, ValBsT, New clsPuntoMetadata(False))
'      LineaBsT.DataSeries = DataSeriesBsT
'      Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'      pSeriesSource.Add(CSVMBsT)


'    End If

'    If ValoriStbd.Dizionario.Count > 0 Then
'      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'      Dim LineaStbd As New FastLineRenderableSeries
'      LineaStbd.XAxisId = "DefaultAxisId"
'      LineaStbd.YAxisId = "DefaultAxisId"
'      LineaStbd.Stroke = Colors.Green
'      LineaStbd.StrokeThickness = 2
'      LineaStbd.Tag = 1
'      For Each Valore In ValoriStbd.Dizionario
'        DataSeriesStbd.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaStbd.DataSeries = DataSeriesStbd
'      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'      SeriesSource.Add(CSVMStbd)

'      Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'      Dim LineaTO As New XyScatterRenderableSeries
'      LineaTO.XAxisId = "DefaultAxisId"
'      LineaTO.YAxisId = "DefaultAxisId"
'      LineaTO.PointMarker = New EllipsePointMarker
'      LineaTO.PointMarker.Stroke = Colors.Green
'      LineaTO.PointMarker.Height = 10
'      LineaTO.PointMarker.Width = 10
'      LineaTO.PointMarker.StrokeThickness = 1
'      LineaTO.PointMarker.Fill = Colors.Green
'      LineaTO.Tag = -1

'      Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'      Dim LineaBsT As New XyScatterRenderableSeries
'      LineaBsT.XAxisId = "DefaultAxisId"
'      LineaBsT.YAxisId = "DefaultAxisId"
'      LineaBsT.PointMarker = New SquarePointMarker
'      LineaBsT.PointMarker.Stroke = Colors.Green
'      LineaBsT.PointMarker.Height = 20
'      LineaBsT.PointMarker.Width = 3
'      LineaBsT.PointMarker.StrokeThickness = 1
'      LineaBsT.PointMarker.Fill = Colors.Green
'      LineaBsT.Tag = -1

'      Dim SSto As Double = ToStbd.Average
'      Dim SSbst As Double = BsTStbd.Average
'      Dim ValTo As Double = Double.NaN
'      Dim ValBsT As Double = Double.NaN
'      For i As Integer = 0 To DataSeriesStbd.XValues.Count - 1
'        Dim Valore As Integer = DataSeriesStbd.XValues(i)
'        If Double.IsNaN(ValTo) AndAlso Valore >= SSto Then
'          ValTo = DataSeriesStbd.YValues(i)
'        End If
'        If Double.IsNaN(ValBsT) AndAlso Valore >= SSbst Then
'          ValBsT = DataSeriesStbd.YValues(i)
'        End If
'      Next

'      DataSeriesTO.Append(SSto, ValTo, New clsPuntoMetadata(False))
'      LineaTO.DataSeries = DataSeriesTO
'      Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'      pSeriesSource.Add(CSVMtO)

'      DataSeriesBsT.Append(SSbst, ValBsT, New clsPuntoMetadata(False))
'      LineaBsT.DataSeries = DataSeriesBsT
'      Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'      pSeriesSource.Add(CSVMBsT)

'    End If
'  End Sub


'  Public Sub DrawChart()
'    If PeriodsManager.ListaAccelerazioni Is Nothing Then Exit Sub
'    If Canale Is Nothing Then
'      DrawVmgMeters()
'      'DrawAcc()
'      Exit Sub
'    End If
'    Titolo = Canale.LongName
'    If ValoriDerivati Then Titolo &= " Derivative"

'    SeriesSource.Clear()
'    Dim SecBefore As Integer = 10
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      Dim Accelerazione As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'      If Accelerazione.MomentOfInizioAccelerazione.Momento.ToOADate > 0 Then

'        Dim Colore As System.Windows.Media.Color = Accelerazione.Periodo.Colore ' AccSyncViewModel.Colori(i)
'        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'        Dim LineaTmp As New FastLineRenderableSeries
'        LineaTmp.XAxisId = "DefaultAxisId"
'        LineaTmp.YAxisId = "DefaultAxisId"
'        If AccSyncViewModel.ColorByTack Then
'          LineaTmp.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTmp.Stroke = Accelerazione.Periodo.Colore
'        End If
'        If Accelerazione.IsStbd Then
'          LineaTmp.StrokeDashArray = {3, 3}
'        End If
'        LineaTmp.StrokeThickness = 2
'        LineaTmp.Tag = Accelerazione.Periodo

'        Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'        Dim LineaTO As New XyScatterRenderableSeries
'        LineaTO.XAxisId = "DefaultAxisId"
'        LineaTO.YAxisId = "DefaultAxisId"
'        LineaTO.PointMarker = New EllipsePointMarker
'        If AccSyncViewModel.ColorByTack Then
'          LineaTO.PointMarker.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTO.PointMarker.Stroke = Accelerazione.Periodo.Colore
'        End If

'        'LineaTO.PointMarker.Stroke = Accelerazione.Periodo.Colore
'        LineaTO.PointMarker.Height = 10
'        LineaTO.PointMarker.Width = 10
'        LineaTO.PointMarker.StrokeThickness = 1
'        If AccSyncViewModel.ColorByTack Then
'          LineaTO.PointMarker.Fill = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaTO.PointMarker.Fill = Accelerazione.Periodo.Colore
'        End If
'        LineaTO.Tag = Accelerazione.Periodo

'        Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'        Dim LineaBsT As New XyScatterRenderableSeries
'        LineaBsT.XAxisId = "DefaultAxisId"
'        LineaBsT.YAxisId = "DefaultAxisId"
'        LineaBsT.PointMarker = New SquarePointMarker
'        If AccSyncViewModel.ColorByTack Then
'          LineaBsT.PointMarker.Stroke = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaBsT.PointMarker.Stroke = Accelerazione.Periodo.Colore
'        End If
'        'LineaBsT.PointMarker.Stroke = Accelerazione.Periodo.Colore
'        LineaBsT.PointMarker.Height = 20
'        LineaBsT.PointMarker.Width = 3
'        LineaBsT.PointMarker.StrokeThickness = 1
'        If AccSyncViewModel.ColorByTack Then
'          LineaBsT.PointMarker.Fill = If(Accelerazione.Periodo.IsStbd, Colors.Green, Colors.Red)
'        Else
'          LineaBsT.PointMarker.Fill = Accelerazione.Periodo.Colore
'        End If
'        LineaBsT.Tag = Accelerazione.Periodo




'        Dim IdRigaIniziale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento.AddSeconds(-SecBefore))
'        Dim IdRigaFinale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfFineTestAccelerazione.Momento)

'        Select Case Canale.DataType
'          Case clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
'          Case Else
'            If Not pCanale.Valori Is Nothing AndAlso pCanale.Valori.Count > 0 Then
'              Dim ValoreRigaPrev As Double = pCanale.Valori(IdRigaIniziale)
'              If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then ValoreRigaPrev = System.Math.Abs(pCanale.Valori(IdRigaIniziale))
'              Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdRigaIniziale)
'              Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
'              Dim IndiceTO As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfTakeOffSpeed.Momento)
'              Dim ValoreTO As Double = Double.NaN
'              Dim IndiceBsT As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfBSatTgt.Momento)
'              Dim ValoreBsT As Double = Double.NaN
'              For Riga As Integer = IdRigaIniziale To IdRigaFinale
'                If Not Double.IsNaN(pCanale.Valori(Riga)) Then
'                  Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'                  If Momento.ToOADate > 0 Then

'                    Dim SecsFromKeyMoment As Double = Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds
'                    Dim ValoreRiga As Double = 0
'                    Select Case Canale.DataType
'                      Case clsChannel2020.eDataType.eAbs180
'                        ValoreRiga = System.Math.Abs(pCanale.Valori(Riga))
'                      Case Else
'                        ValoreRiga = pCanale.Valori(Riga)
'                    End Select
'                    If ValoriDerivati Then
'                      Dim T As Double = Momento.Subtract(MomentoPrev).TotalSeconds
'                      Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / T
'                      If T > 0 Then
'                        MediaMobile.AggiornaMedia(Derivata)
'                        DataSeriesTMP.Append(SecsFromKeyMoment, MediaMobile.Valore, New clsPuntoMetadata(False))
'                        If Double.IsNaN(ValoreTO) AndAlso Riga >= IndiceTO Then
'                          ValoreTO = MediaMobile.Valore
'                        End If
'                        If Double.IsNaN(ValoreBsT) AndAlso Riga >= IndiceBsT Then
'                          ValoreBsT = MediaMobile.Valore
'                        End If
'                        ValoreRigaPrev = ValoreRiga
'                        MomentoPrev = Momento
'                      End If
'                    Else
'                      DataSeriesTMP.Append(SecsFromKeyMoment, ValoreRiga, New clsPuntoMetadata(False))
'                      If Double.IsNaN(ValoreTO) AndAlso Riga >= IndiceTO Then
'                        ValoreTO = ValoreRiga
'                      End If
'                      If Double.IsNaN(ValoreBsT) AndAlso Riga >= IndiceBsT Then
'                        ValoreBsT = ValoreRiga
'                      End If
'                    End If
'                  End If
'                End If
'              Next

'              LineaTmp.DataSeries = DataSeriesTMP
'              Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'              pSeriesSource.Add(CSVMtmp)

'              DataSeriesTO.Append(Accelerazione.TimeToTakeOffSpeed.TotalSeconds, ValoreTO, New clsPuntoMetadata(False))
'              LineaTO.DataSeries = DataSeriesTO
'              Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'              pSeriesSource.Add(CSVMtO)

'              DataSeriesBsT.Append(Accelerazione.MomentOfBSatTgt.Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds, ValoreBsT, New clsPuntoMetadata(False))
'              LineaBsT.DataSeries = DataSeriesBsT
'              Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'              pSeriesSource.Add(CSVMBsT)
'            End If
'        End Select
'      End If
'    Next

'    If Not ValoriDerivati And Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eSOW Then
'      'disegna le due linee orizzontali entry BS e take off bs
'      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'      Dim LineaTmp As New FastLineRenderableSeries
'      LineaTmp.XAxisId = "DefaultAxisId"
'      LineaTmp.YAxisId = "DefaultAxisId"
'      LineaTmp.Stroke = Colors.OrangeRed
'      LineaTmp.StrokeDashArray = {3, 3}
'      LineaTmp.StrokeThickness = 2
'      LineaTmp.Tag = 0
'      DataSeriesTMP.Append(-SecBefore, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, New clsPuntoMetadata(False))
'      DataSeriesTMP.Append(PeriodsManager.AccelerationSettings2020.TestDuration, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, New clsPuntoMetadata(False))
'      LineaTmp.DataSeries = DataSeriesTMP
'      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'      pSeriesSource.Add(CSVMtmp)

'      DataSeriesTMP = New XyDataSeries(Of Double, Double)
'      LineaTmp = New FastLineRenderableSeries
'      LineaTmp.XAxisId = "DefaultAxisId"
'      LineaTmp.YAxisId = "DefaultAxisId"
'      LineaTmp.Stroke = Colors.Gold
'      LineaTmp.StrokeDashArray = {3, 3}
'      LineaTmp.StrokeThickness = 3
'      LineaTmp.Tag = 0
'      DataSeriesTMP.Append(-SecBefore, PeriodsManager.AccelerationSettings2020.BsLimite, New clsPuntoMetadata(False))
'      DataSeriesTMP.Append(PeriodsManager.AccelerationSettings2020.TestDuration, PeriodsManager.AccelerationSettings2020.BsLimite, New clsPuntoMetadata(False))
'      LineaTmp.DataSeries = DataSeriesTMP
'      CSVMtmp = New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'      pSeriesSource.Add(CSVMtmp)
'    End If

'    OnPropertyChanged("SeriesSource")
'  End Sub

'  Public Sub DrawChartGroupByTack()
'    If Canale Is Nothing Then
'      DrawVmgMetersGroupByTack()
'      Exit Sub
'    End If
'    Titolo = Canale.LongName
'    If ValoriDerivati Then Titolo &= " Derivative"
'    Dim SecBefore As Integer = 10

'    Dim ValoriPort As New clsValoriAggregati
'    Dim ValoriStbd As New clsValoriAggregati
'    SeriesSource.Clear()


'    Dim ToPort As New List(Of Double)
'    Dim ToStbd As New List(Of Double)
'    Dim BsTPort As New List(Of Double)
'    Dim BsTStbd As New List(Of Double)
'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      If PeriodsManager.ListaAccelerazioni(i).IsChecked Then
'        Dim Acc As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'        If PeriodsManager.ListaAccelerazioni(i).IsStbd Then
'          ToStbd.Add(Acc.TimeToTakeOffSpeed.TotalSeconds)
'          BsTStbd.Add(Acc.MomentOfBSatTgt.Momento.Subtract(Acc.MomentOfInizioAccelerazione.Momento).TotalSeconds)
'        Else
'          ToPort.Add(Acc.TimeToTakeOffSpeed.TotalSeconds)
'          BsTPort.Add(Acc.MomentOfBSatTgt.Momento.Subtract(Acc.MomentOfInizioAccelerazione.Momento).TotalSeconds)
'        End If
'      End If
'    Next


'    For i As Integer = 0 To PeriodsManager.ListaAccelerazioni.Count - 1
'      If PeriodsManager.ListaAccelerazioni(i).IsChecked Then

'        Dim Accelerazione As clsAcceleration = PeriodsManager.ListaAccelerazioni(i).DettagliAcceleration
'        Dim Colore As System.Windows.Media.Color = Accelerazione.Periodo.Colore ' AccSyncViewModel.Colori(i)

'        Dim IdRigaIniziale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfInizioAccelerazione.Momento.AddSeconds(-SecBefore))
'        Dim IdRigaFinale As Integer = DataProvider2020.TrovaIndice(Accelerazione.MomentOfFineTestAccelerazione.Momento)

'        Select Case Canale.DataType
'          Case clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
'          Case Else
'            If (pCanale.Valori.Count + pCanale.Valori.Count) > 0 Then
'              Dim ValoreRigaPrev As Double = pCanale.Valori(IdRigaIniziale)
'              If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then ValoreRigaPrev = System.Math.Abs(pCanale.Valori(IdRigaIniziale))
'              Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdRigaIniziale)
'              Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
'              'Dim MediaMobile As New clsMediaMobile(2, Canale.DataType = clsChannel2020.eDataType.e360)
'              For Riga As Integer = IdRigaIniziale To IdRigaFinale
'                If Not Double.IsNaN(pCanale.Valori(Riga)) Then
'                  Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'                  Dim SecsFromKeyMoment As Double = Momento.Subtract(Accelerazione.MomentOfInizioAccelerazione.Momento).TotalSeconds
'                  Dim ValoreRiga As Double = 0
'                  Select Case Canale.DataType
'                    Case clsChannel2020.eDataType.eAbs180
'                      ValoreRiga = System.Math.Abs(pCanale.Valori(Riga))
'                    Case Else
'                      ValoreRiga = pCanale.Valori(Riga)
'                  End Select
'                  If ValoriDerivati Then
'                    Dim T As Double = Momento.Subtract(MomentoPrev).TotalSeconds
'                    Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / T
'                    If T > 0 Then
'                      MediaMobile.AggiornaMedia(Derivata)
'                      If Accelerazione.IsStbd Then
'                        ValoriStbd.AggiungiCoppia(SecsFromKeyMoment, MediaMobile.Valore)
'                      Else
'                        ValoriPort.AggiungiCoppia(SecsFromKeyMoment, MediaMobile.Valore)
'                      End If
'                      ValoreRigaPrev = ValoreRiga
'                      MomentoPrev = Momento
'                    End If
'                  Else
'                    If Accelerazione.IsStbd Then
'                      ValoriStbd.AggiungiCoppia(CInt(SecsFromKeyMoment), ValoreRiga)
'                    Else
'                      ValoriPort.AggiungiCoppia(CInt(SecsFromKeyMoment), ValoreRiga)
'                    End If
'                  End If
'                End If
'              Next
'            End If
'        End Select
'      End If
'    Next

'    If ValoriPort.Dizionario.Count > 0 Then
'      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
'      Dim LineaPort As New FastLineRenderableSeries
'      LineaPort.XAxisId = "DefaultAxisId"
'      LineaPort.YAxisId = "DefaultAxisId"
'      LineaPort.Stroke = Colors.Red
'      LineaPort.StrokeThickness = 2
'      LineaPort.Tag = -1
'      For Each Valore In ValoriPort.Dizionario
'        DataSeriesPort.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaPort.DataSeries = DataSeriesPort
'      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
'      SeriesSource.Add(CSVMport)

'      Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'      Dim LineaTO As New XyScatterRenderableSeries
'      LineaTO.XAxisId = "DefaultAxisId"
'      LineaTO.YAxisId = "DefaultAxisId"
'      LineaTO.PointMarker = New EllipsePointMarker
'      LineaTO.PointMarker.Stroke = Colors.Red
'      LineaTO.PointMarker.Height = 10
'      LineaTO.PointMarker.Width = 10
'      LineaTO.PointMarker.StrokeThickness = 1
'      LineaTO.PointMarker.Fill = Colors.Red
'      LineaTO.Tag = -1

'      Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'      Dim LineaBsT As New XyScatterRenderableSeries
'      LineaBsT.XAxisId = "DefaultAxisId"
'      LineaBsT.YAxisId = "DefaultAxisId"
'      LineaBsT.PointMarker = New SquarePointMarker
'      LineaBsT.PointMarker.Stroke = Colors.Red
'      LineaBsT.PointMarker.Height = 20
'      LineaBsT.PointMarker.Width = 3
'      LineaBsT.PointMarker.StrokeThickness = 1
'      LineaBsT.PointMarker.Fill = Colors.Red
'      LineaBsT.Tag = -1

'      Dim SSto As Double = ToPort.Average
'      Dim SSbst As Double = BsTPort.Average
'      Dim ValTo As Double = Double.NaN
'      Dim ValBsT As Double = Double.NaN
'      For i As Integer = 0 To DataSeriesPort.XValues.Count - 1
'        Dim Valore As Integer = DataSeriesPort.XValues(i)
'        If Double.IsNaN(ValTo) AndAlso Valore >= SSto Then
'          ValTo = DataSeriesPort.YValues(i)
'        End If
'        If Double.IsNaN(ValBsT) AndAlso Valore >= SSbst Then
'          ValBsT = DataSeriesPort.YValues(i)
'        End If
'      Next

'      DataSeriesTO.Append(SSto, ValTo, New clsPuntoMetadata(False))
'      LineaTO.DataSeries = DataSeriesTO
'      Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'      pSeriesSource.Add(CSVMtO)

'      DataSeriesBsT.Append(SSbst, ValBsT, New clsPuntoMetadata(False))
'      LineaBsT.DataSeries = DataSeriesBsT
'      Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'      pSeriesSource.Add(CSVMBsT)
'    End If

'    If ValoriStbd.Dizionario.Count > 0 Then
'      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
'      Dim LineaStbd As New FastLineRenderableSeries
'      LineaStbd.XAxisId = "DefaultAxisId"
'      LineaStbd.YAxisId = "DefaultAxisId"
'      LineaStbd.Stroke = Colors.Green
'      LineaStbd.StrokeThickness = 2
'      LineaStbd.Tag = 1
'      For Each Valore In ValoriStbd.Dizionario
'        DataSeriesStbd.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
'      Next
'      LineaStbd.DataSeries = DataSeriesStbd
'      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
'      SeriesSource.Add(CSVMStbd)

'      Dim DataSeriesTO As New XyDataSeries(Of Double, Double)
'      Dim LineaTO As New XyScatterRenderableSeries
'      LineaTO.XAxisId = "DefaultAxisId"
'      LineaTO.YAxisId = "DefaultAxisId"
'      LineaTO.PointMarker = New EllipsePointMarker
'      LineaTO.PointMarker.Stroke = Colors.Green
'      LineaTO.PointMarker.Height = 10
'      LineaTO.PointMarker.Width = 10
'      LineaTO.PointMarker.StrokeThickness = 1
'      LineaTO.PointMarker.Fill = Colors.Green
'      LineaTO.Tag = -1

'      Dim DataSeriesBsT As New XyDataSeries(Of Double, Double)
'      Dim LineaBsT As New XyScatterRenderableSeries
'      LineaBsT.XAxisId = "DefaultAxisId"
'      LineaBsT.YAxisId = "DefaultAxisId"
'      LineaBsT.PointMarker = New SquarePointMarker
'      LineaBsT.PointMarker.Stroke = Colors.Green
'      LineaBsT.PointMarker.Height = 20
'      LineaBsT.PointMarker.Width = 3
'      LineaBsT.PointMarker.StrokeThickness = 1
'      LineaBsT.PointMarker.Fill = Colors.Green
'      LineaBsT.Tag = -1

'      Dim SSto As Double = ToStbd.Average
'      Dim SSbst As Double = BsTStbd.Average
'      Dim ValTo As Double = Double.NaN
'      Dim ValBsT As Double = Double.NaN
'      For i As Integer = 0 To DataSeriesStbd.XValues.Count - 1
'        Dim Valore As Integer = DataSeriesStbd.XValues(i)
'        If Double.IsNaN(ValTo) AndAlso Valore >= SSto Then
'          ValTo = DataSeriesStbd.YValues(i)
'        End If
'        If Double.IsNaN(ValBsT) AndAlso Valore >= SSbst Then
'          ValBsT = DataSeriesStbd.YValues(i)
'        End If
'      Next

'      DataSeriesTO.Append(SSto, ValTo, New clsPuntoMetadata(False))
'      LineaTO.DataSeries = DataSeriesTO
'      Dim CSVMtO As New ChartSeriesViewModel(DataSeriesTO, LineaTO)
'      pSeriesSource.Add(CSVMtO)

'      DataSeriesBsT.Append(SSbst, ValBsT, New clsPuntoMetadata(False))
'      LineaBsT.DataSeries = DataSeriesBsT
'      Dim CSVMBsT As New ChartSeriesViewModel(DataSeriesBsT, LineaBsT)
'      pSeriesSource.Add(CSVMBsT)




'    End If

'    If Not ValoriDerivati And Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eSOW Then
'      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
'      Dim LineaTmp As New FastLineRenderableSeries
'      LineaTmp.XAxisId = "DefaultAxisId"
'      LineaTmp.YAxisId = "DefaultAxisId"
'      LineaTmp.Stroke = Colors.Orange
'      LineaTmp.StrokeDashArray = {3, 3}
'      LineaTmp.StrokeThickness = 2
'      LineaTmp.Tag = 0
'      DataSeriesTMP.Append(-SecBefore, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, New clsPuntoMetadata(False))
'      DataSeriesTMP.Append(PeriodsManager.AccelerationSettings2020.TestDuration, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, New clsPuntoMetadata(False))
'      LineaTmp.DataSeries = DataSeriesTMP
'      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'      SeriesSource.Add(CSVMtmp)

'      DataSeriesTMP = New XyDataSeries(Of Double, Double)
'      LineaTmp = New FastLineRenderableSeries
'      LineaTmp.XAxisId = "DefaultAxisId"
'      LineaTmp.YAxisId = "DefaultAxisId"
'      LineaTmp.Stroke = Colors.Gold
'      LineaTmp.StrokeDashArray = {3, 3}
'      LineaTmp.StrokeThickness = 3
'      LineaTmp.Tag = 0
'      DataSeriesTMP.Append(-SecBefore, PeriodsManager.AccelerationSettings2020.BsLimite, New clsPuntoMetadata(False))
'      DataSeriesTMP.Append(PeriodsManager.AccelerationSettings2020.TestDuration, PeriodsManager.AccelerationSettings2020.BsLimite, New clsPuntoMetadata(False))
'      LineaTmp.DataSeries = DataSeriesTMP
'      CSVMtmp = New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
'      SeriesSource.Add(CSVMtmp)

'    End If

'  End Sub

'End Class

'Public Class UserControlAccSyncViewModel
'  Implements INotifyPropertyChanged
'  'Dim pAccelerations As List(Of clsAcceleration)
'  'Dim pPeriodi As List(Of clsPeriod2020)
'  Dim pColori As List(Of System.Windows.Media.Color)
'  Dim pSharedXVisibleRange As IRange
'  Dim pCurrentPosition As Double 'secondi dallo 0 accetta numeri negativi)
'  'Dim pAsseRiferimento As clsPavarot2019.eAxisRef
'  Dim pWheelZoomEnabled As Boolean = True

'  Dim pInizioCampionamento As Integer
'  Dim pFineCampionamento As Integer

'  Dim pTrackX1(999) As Double
'  Dim pTrackX2(999) As Double
'  Dim pTrackY1(999) As Double
'  Dim pTrackY2(999) As Double

'  Dim pTrackVmgX1(999) As Double
'  Dim pTrackVmgX2(999) As Double
'  Dim pTrackVmgY1(999) As Double
'  Dim pTrackVmgY2(999) As Double

'  Dim pTrackLatX1(999) As Double
'  Dim pTrackLatX2(999) As Double
'  Dim pTrackLatY1(999) As Double
'  Dim pTrackLatY2(999) As Double




'  Dim pParentUserControlAccelerationViewModel As UserControlAccelerationViewModel

'  Dim pCurrentRenderableSeries As IRenderableSeries

'  Dim pGroupByTack As Boolean

'  Dim pColorByTack As Boolean

'  Dim pZoomPanEnabled As Boolean = False
'  Dim pDataPointSelectionEnabled As Boolean = True



'  Dim pLoadedDataMouseMode As eLoadedDataMouseMode = eLoadedDataMouseMode.eSelection

'  Public Enum eLoadedDataMouseMode
'    eSelection = 0
'    ePan = 1
'  End Enum


'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Public Property GroupByTack As Boolean
'    Get
'      Return pGroupByTack
'    End Get
'    Set(value As Boolean)
'      pGroupByTack = value
'      OnPropertyChanged("GroupByTack")
'    End Set
'  End Property

'  Public Property ColorByTack As Boolean
'    Get
'      Return pColorByTack
'    End Get
'    Set(value As Boolean)
'      pColorByTack = value
'      OnPropertyChanged("ColorByTack")
'    End Set
'  End Property


'  Public Property LoadedDataMouseMode As eLoadedDataMouseMode
'    Get
'      Return pLoadedDataMouseMode
'    End Get
'    Set(value As eLoadedDataMouseMode)
'      pLoadedDataMouseMode = value
'      OnPropertyChanged("LoadedDataMouseMode")
'    End Set
'  End Property

'  Public Property SharedXVisibleRange As IRange
'    Get
'      Return pSharedXVisibleRange
'    End Get
'    Set(value As IRange)
'      If pSharedXVisibleRange Is value Then Exit Property
'      pSharedXVisibleRange = value
'      OnPropertyChanged("SharedXVisibleRange")
'    End Set
'  End Property

'  Public Property InizioCampionamento As Integer
'    Get
'      Return pInizioCampionamento
'    End Get
'    Set(value As Integer)
'      If pInizioCampionamento = value Then Exit Property
'      pInizioCampionamento = value
'      OnPropertyChanged("InizioCampionamento")
'    End Set
'  End Property

'  Public Property FineCampionamento As Integer
'    Get
'      Return pFineCampionamento
'    End Get
'    Set(value As Integer)
'      If pFineCampionamento = value Then Exit Property
'      pFineCampionamento = value
'      OnPropertyChanged("FineCampionamento")
'    End Set
'  End Property

'  'Public Property AsseRiferimento As clsPavarot2019.eAxisRef
'  '  Get
'  '    Return pAsseRiferimento
'  '  End Get
'  '  Set(value As clsPavarot2019.eAxisRef)
'  '    pAsseRiferimento = value
'  '  End Set
'  'End Property

'  Public Property TrackX1 As Double()
'    Get
'      Return pTrackX1
'    End Get
'    Set(value As Double())
'      If pTrackX1 Is value Then Exit Property
'      pTrackX1 = value
'      OnPropertyChanged("TrackX1")
'    End Set
'  End Property

'  Public Property TrackX2 As Double()
'    Get
'      Return pTrackX2
'    End Get
'    Set(value As Double())
'      If pTrackX2 Is value Then Exit Property
'      pTrackX2 = value
'      OnPropertyChanged("TrackX2")
'    End Set
'  End Property

'  Public Property TrackY1 As Double()
'    Get
'      Return pTrackY1
'    End Get
'    Set(value As Double())
'      If pTrackY1 Is value Then Exit Property
'      pTrackY1 = value
'      OnPropertyChanged("TrackY1")
'    End Set
'  End Property

'  Public Property TrackY2 As Double()
'    Get
'      Return pTrackY2
'    End Get
'    Set(value As Double())
'      If pTrackY2 Is value Then Exit Property
'      pTrackY2 = value
'      OnPropertyChanged("TrackY2")
'    End Set
'  End Property

'  Public Property TrackVmgX1 As Double()
'    Get
'      Return pTrackVmgX1
'    End Get
'    Set(value As Double())
'      If pTrackVmgX1 Is value Then Exit Property
'      pTrackVmgX1 = value
'      OnPropertyChanged("TrackVmgX1")
'    End Set
'  End Property

'  Public Property TrackVmgX2 As Double()
'    Get
'      Return pTrackVmgX2
'    End Get
'    Set(value As Double())
'      If pTrackVmgX2 Is value Then Exit Property
'      pTrackVmgX2 = value
'      OnPropertyChanged("TrackVmgX2")
'    End Set
'  End Property

'  Public Property TrackVmgY1 As Double()
'    Get
'      Return pTrackVmgY1
'    End Get
'    Set(value As Double())
'      If pTrackVmgY1 Is value Then Exit Property
'      pTrackVmgY1 = value
'      OnPropertyChanged("TrackVmgY1")
'    End Set
'  End Property

'  Public Property TrackVmgY2 As Double()
'    Get
'      Return pTrackVmgY2
'    End Get
'    Set(value As Double())
'      If pTrackVmgY2 Is value Then Exit Property
'      pTrackVmgY2 = value
'      OnPropertyChanged("TrackVmgY2")
'    End Set
'  End Property

'  Public Property TrackLatX1 As Double()
'    Get
'      Return pTrackLatX1
'    End Get
'    Set(value As Double())
'      If pTrackLatX1 Is value Then Exit Property
'      pTrackLatX1 = value
'      OnPropertyChanged("TrackLatX1")
'    End Set
'  End Property

'  Public Property TrackLatX2 As Double()
'    Get
'      Return pTrackLatX2
'    End Get
'    Set(value As Double())
'      If pTrackLatX2 Is value Then Exit Property
'      pTrackLatX2 = value
'      OnPropertyChanged("TrackLatX2")
'    End Set
'  End Property

'  Public Property TrackLatY1 As Double()
'    Get
'      Return pTrackLatY1
'    End Get
'    Set(value As Double())
'      If pTrackLatY1 Is value Then Exit Property
'      pTrackLatY1 = value
'      OnPropertyChanged("TrackLatY1")
'    End Set
'  End Property

'  Public Property TrackLatY2 As Double()
'    Get
'      Return pTrackLatY2
'    End Get
'    Set(value As Double())
'      If pTrackLatY2 Is value Then Exit Property
'      pTrackLatY2 = value
'      OnPropertyChanged("TrackLatY2")
'    End Set
'  End Property

'  Public Property CurrentPosition As Double
'    Get
'      Return pCurrentPosition
'    End Get
'    Set(value As Double)
'      If pCurrentPosition = value Then Exit Property
'      pCurrentPosition = value
'      'AggiornaPosizioniSuTrack()
'      AggiornaPosizioniSuGrafici()
'      OnPropertyChanged("CurrentPosition")
'      OnPropertyChanged("CurrentXposition")
'    End Set
'  End Property

'  Public Property WheelZoomEnabled As Boolean
'    Get
'      Return pWheelZoomEnabled
'    End Get
'    Set(value As Boolean)
'      If pWheelZoomEnabled = value Then Exit Property
'      pWheelZoomEnabled = value
'      OnPropertyChanged("WheelZoomEnabled")
'    End Set
'  End Property

'  Public ReadOnly Property CurrentXposition As String
'    Get
'      Return pCurrentPosition.ToString("F1")
'    End Get
'  End Property

'  Public Property Colori As List(Of Windows.Media.Color)
'    Get
'      Return pColori
'    End Get
'    Set(value As List(Of Windows.Media.Color))
'      pColori = value
'    End Set
'  End Property

'  Public Property CurrentRenderableSeries As IRenderableSeries
'    Get
'      Return pCurrentRenderableSeries
'    End Get
'    Set(value As IRenderableSeries)
'      pCurrentRenderableSeries = value
'    End Set
'  End Property

'  Public Property ParentUserControlAccelerationViewModel As UserControlAccelerationViewModel
'    Get
'      Return pParentUserControlAccelerationViewModel
'    End Get
'    Set(value As UserControlAccelerationViewModel)
'      pParentUserControlAccelerationViewModel = value
'    End Set
'  End Property

'  Private Sub AggiornaPosizioniSuTrack()
'    'Stop
'    Dim Dimensione As Integer = 20
'    If PeriodsManager.ListaAccelerazioni.Count > 0 Then
'      For Each Periodo In PeriodsManager.ListaAccelerazioni
'        If Periodo.DettagliAcceleration.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then

'          If Periodo.IsChecked Then
'            Dim MomentoCorrente As DateTime = Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.Momento.AddSeconds(pCurrentPosition)
'            Dim Riga As Integer = DataProvider2020.TrovaIndice(MomentoCorrente)
'            Riga = System.Math.Max(Riga, DataProvider2020.TrovaIndice(Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.Momento.AddSeconds(-10)))
'            Riga = System.Math.Min(Riga, DataProvider2020.TrovaIndice(Periodo.DettagliAcceleration.MomentOfFineTestAccelerazione.Momento))
'            Dim SegnoStbd As Integer = IIf(Periodo.DettagliAcceleration.IsStbd, -1, 1)

'            Dim Punto As PointF = Periodo.DettagliAcceleration.MatriceGeoPosRelToPuntoIniziale(System.Math.Max(0, Riga - Periodo.DettagliAcceleration.Periodo.TimeRange.IdRigaIniziale))
'            pTrackX1(Periodo.Id) = (SegnoStbd * Punto.X - (Dimensione / 2))
'            pTrackX2(Periodo.Id) = (SegnoStbd * Punto.X + (Dimensione / 2))
'            pTrackY1(Periodo.Id) = Punto.Y - Dimensione
'            pTrackY2(Periodo.Id) = Punto.Y + Dimensione
'          End If
'        End If
'      Next
'    End If
'    OnPropertyChanged("TrackX1")
'    OnPropertyChanged("TrackX2")
'    OnPropertyChanged("TrackY1")
'    OnPropertyChanged("TrackY2")
'  End Sub

'  Private Sub AggiornaPosizioniSuGrafici()
'    'pCurrentPosition é la posizione in secondi dal momento chiave ovvero di inizio accelerazione
'    Dim Perc As Integer = 50

'    If PeriodsManager.ListaAccelerazioni.Count > 0 Then
'      For Each Periodo In PeriodsManager.ListaAccelerazioni
'        If Not Periodo.DettagliAcceleration Is Nothing Then
'          If Periodo.DettagliAcceleration.MatriceGeoPosRelToPuntoIniziale.Count > 0 Then
'            Dim MomentoCorrente As DateTime = Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.Momento.AddSeconds(pCurrentPosition)
'            Dim Riga As Integer = DataProvider2020.TrovaIndice(MomentoCorrente)
'            Riga = System.Math.Max(Riga, DataProvider2020.TrovaIndice(Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.Momento.AddSeconds(-10)))
'            Riga = System.Math.Min(Riga, DataProvider2020.TrovaIndice(Periodo.DettagliAcceleration.MomentOfFineTestAccelerazione.Momento))
'            Dim SegnoStbd As Integer = IIf(Periodo.DettagliAcceleration.IsStbd, -1, 1)

'            Dim W As Integer = ParentUserControlAccelerationViewModel.VisibleRangeChangedFwdLat_X.AsDoubleRange.Diff / Perc
'            Dim H As Integer = ParentUserControlAccelerationViewModel.VisibleRangeChangedFwdLat_Y.AsDoubleRange.Diff / Perc
'            Dim IdRiga As Integer = System.Math.Max(0, Riga - Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.IdRiga)
'            If IdRiga < Periodo.DettagliAcceleration.MatriceGeoPosRelToPuntoIniziale.Count Then
'              Dim Punto As PointF = Periodo.DettagliAcceleration.MatriceGeoPosRelToPuntoIniziale(IdRiga)
'              pTrackX1(Periodo.Id) = (SegnoStbd * Punto.X - W)
'              pTrackX2(Periodo.Id) = (SegnoStbd * Punto.X + W)
'              pTrackY1(Periodo.Id) = Punto.Y - H
'              pTrackY2(Periodo.Id) = Punto.Y + H

'              Dim SegnoUpwind As Integer = IIf(Periodo.DettagliAcceleration.IsUpwind, 1, -1)
'              Dim RigaInizioPeriodoAccelerazione As Integer = Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.IdRiga
'              Dim SS As Double = MomentoCorrente.Subtract(Periodo.DettagliAcceleration.MomentOfInizioAccelerazione.Momento).TotalSeconds

'              W = ParentUserControlAccelerationViewModel.VisibleRangeChangedVmgMt_X.AsDoubleRange.Diff / Perc
'              H = ParentUserControlAccelerationViewModel.VisibleRangeChangedVmgMt_Y.AsDoubleRange.Diff / Perc
'              pTrackVmgX1(Periodo.Id) = (SS - W)
'              pTrackVmgX2(Periodo.Id) = (SS + W)
'              pTrackVmgY1(Periodo.Id) = SegnoUpwind * Punto.Y - H
'              pTrackVmgY2(Periodo.Id) = SegnoUpwind * Punto.Y + H

'              W = ParentUserControlAccelerationViewModel.VisibleRangeChangedLatMt_X.AsDoubleRange.Diff / Perc
'              H = ParentUserControlAccelerationViewModel.VisibleRangeChangedLatMt_Y.AsDoubleRange.Diff / Perc
'              pTrackLatX1(Periodo.Id) = (SegnoStbd * Punto.X - W)
'              pTrackLatX2(Periodo.Id) = (SegnoStbd * Punto.X + W)
'              pTrackLatY1(Periodo.Id) = SS - H
'              pTrackLatY2(Periodo.Id) = SS + H
'            End If
'          End If
'        End If
'      Next
'    End If
'    OnPropertyChanged("TrackX1")
'    OnPropertyChanged("TrackX2")
'    OnPropertyChanged("TrackY1")
'    OnPropertyChanged("TrackY2")
'    OnPropertyChanged("TrackVmgX1")
'    OnPropertyChanged("TrackVmgX2")
'    OnPropertyChanged("TrackVmgY1")
'    OnPropertyChanged("TrackVmgY2")
'    OnPropertyChanged("TrackLatX1")
'    OnPropertyChanged("TrackLatX2")
'    OnPropertyChanged("TrackLatY1")
'    OnPropertyChanged("TrackLatY2")
'  End Sub


'  Public Property ZoomPanEnabled As Boolean
'    Get
'      Return pZoomPanEnabled
'    End Get
'    Set(value As Boolean)
'      pZoomPanEnabled = value
'      OnPropertyChanged("ZoomPanEnabled")
'    End Set
'  End Property

'  Public Property DataPointSelectionEnabled As Boolean
'    Get
'      Return pDataPointSelectionEnabled
'    End Get
'    Set(value As Boolean)
'      pDataPointSelectionEnabled = value
'      OnPropertyChanged("DataPointSelectionEnabled")
'    End Set
'  End Property



'  Public Function ChangeMouseMode() As String
'    Dim descrizione As String = ""
'    Select Case pLoadedDataMouseMode
'      Case eLoadedDataMouseMode.ePan
'        pLoadedDataMouseMode = eLoadedDataMouseMode.eSelection
'        ZoomPanEnabled = False
'        DataPointSelectionEnabled = True
'        descrizione = "mMode: Selection"
'      Case eLoadedDataMouseMode.eSelection
'        pLoadedDataMouseMode = eLoadedDataMouseMode.ePan
'        ZoomPanEnabled = True
'        DataPointSelectionEnabled = False
'        descrizione = "mMode: Pan"
'    End Select
'    Return descrizione
'  End Function





'End Class



'Public Class clsAccelerationControls
'  Implements INotifyPropertyChanged

'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Dim pParentViewModel As UserControlAccelerationViewModel

'  Dim pAccSyncViewModel As UserControlAccSyncViewModel
'  Dim pListaControlli As New ObservableCollection(Of UserControlAccPlot)
'  Dim pNomeMatrice As String

'  Public Sub New(NomeMatrice As String, AccSyncViewModel As UserControlAccSyncViewModel, ParentViewModel As UserControlAccelerationViewModel)
'    pParentViewModel = ParentViewModel
'    pNomeMatrice = NomeMatrice
'    pAccSyncViewModel = AccSyncViewModel
'    CaricaControlliDaXML()
'  End Sub

'  Public Property ListaControlli As ObservableCollection(Of UserControlAccPlot)
'    Get
'      Return pListaControlli
'    End Get
'    Set(value As ObservableCollection(Of UserControlAccPlot))
'      pListaControlli = value
'      OnPropertyChanged("ListaControlli")
'    End Set
'  End Property

'  Public Property AccSyncViewModel As UserControlAccSyncViewModel
'    Get
'      Return pAccSyncViewModel
'    End Get
'    Set(value As UserControlAccSyncViewModel)
'      pAccSyncViewModel = value
'    End Set
'  End Property

'  Private Sub ControlliDefault(Indice As Integer, ByRef Canale As clsChannel2020)
'    Select Case Indice
'      Case 0
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'      Case 1
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'      Case 2
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
'      'Case 3
'      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRideHeight)
'      Case 4
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
'      Case 5
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM)
'      'Case 6
'      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrRake)
'      'Case 7
'      '  Canale = DataProvider2020.CanaleDbl("PortCantAngle")
'      'Case 8
'      '  Canale = DataProvider2020.CanaleDbl("PortFoilInFlap1Angle")
'      'Case 9
'      '  Canale = DataProvider2020.CanaleDbl("PortFoilOutFlap1Angle")
'      Case 10
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
'      Case 11
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWS)
'      Case Else
'        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
'    End Select
'  End Sub


'  Public Sub CaricaControlliDaXML()
'    ListaControlli.Clear()
'    ' Crea il controllo Acc quale derivata della BS nel tempo, questo esiste sempre e non viene mai ne salvato ne caricato da xml
'    Dim ctrlTmp As New UserControlAccPlot(200, pParentViewModel)
'    Dim ctrlTmpVm As New UserControlAccPlotViewModel(AccSyncViewModel, Nothing)
'    ctrlTmp.DataContext = ctrlTmpVm
'    ListaControlli.Add(ctrlTmp)

'    ' cerca i controlli custom
'    Dim NodoCharts As Xml.XmlNode = AppConfig.CercaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, True)
'    If NodoCharts Is Nothing OrElse Not NodoCharts.HasChildNodes Then
'      ' se non li trova mette quelli di default salvandoli
'      Dim Canale As clsChannel2020 = Nothing
'      For i As Integer = 0 To 11
'        ControlliDefault(i, Canale)
'        If Not Canale Is Nothing Then
'          ctrlTmp = New UserControlAccPlot(200, pParentViewModel)
'          Dim ctrlTmpVMsc As New UserControlAccPlotViewModel(AccSyncViewModel, Canale)
'          ctrlTmp.DataContext = ctrlTmpVMsc
'          ListaControlli.Add(ctrlTmp)
'          AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & i + 1, "Yaxis", Header(Canale), True, False)
'        End If
'      Next
'      AppConfig.SalvaFileXML()
'    Else
'      For Each Nodo As Xml.XmlNode In NodoCharts.ChildNodes
'        Dim HeaderCanale As String = Nodo.Item("Yaxis").InnerText
'        Dim Canale As clsChannel2020 = DataProvider2020.CanaleDbl(HeaderCanale)
'        If Not Canale Is Nothing Then
'          ctrlTmp = New UserControlAccPlot(200, pParentViewModel)
'          Dim ctrlTmpVMsc As New UserControlAccPlotViewModel(AccSyncViewModel, Canale)
'          ctrlTmp.DataContext = ctrlTmpVMsc
'          ListaControlli.Add(ctrlTmp)
'        End If
'      Next
'    End If
'    'ListaControlli = ListaControlli
'  End Sub

'  Private Function Header(CanaleAscissa As clsChannel2020) As String
'    If CanaleAscissa Is Nothing Then Return ""
'    If CanaleAscissa.IsMath Then
'      Return CanaleAscissa.CanaleChiave.ToString.TrimStart("e")
'    Else
'      Return CanaleAscissa.ChannelId
'    End If
'  End Function

'  Public Sub SalvaImpostazione()
'    AppConfig.EliminaNodo(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, True)
'    Dim ID As Integer = 0
'    If ListaControlli.Count > 0 Then
'      For Each ctrl As UserControlAccPlot In ListaControlli
'        If ID > 0 Then
'          Dim AccPlotViewModel As UserControlAccPlotViewModel = DirectCast(ctrl.DataContext, UserControlAccPlotViewModel)
'          AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Yaxis", Header(AccPlotViewModel.Canale), True, False)
'        End If
'        ID += 1
'      Next
'      AppConfig.SalvaFileXML()
'    End If

'  End Sub


'  Public Sub AggiungiCanale(Canale As clsChannel2020)
'    Dim AccPlotViewModel As New UserControlAccPlotViewModel(AccSyncViewModel, Canale)
'    Dim ctrlTmp As New UserControlAccPlot(200, pParentViewModel)
'    ctrlTmp.DataContext = AccPlotViewModel
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ListaControlli.Count, "Yaxis", Header(Canale), True, True)
'    ListaControlli.Add(ctrlTmp)
'  End Sub
'End Class


'Public Class clsAccelerations
'  Dim pListaAccelerazioni As New ObservableCollection(Of clsAcceleration)
'  Dim pBsLimite As Double ' BS superata la quale ha inizio l accelerazione, é lo zero dell'accelerazione
'  Dim pTakeOffSpeed As Double ' BS necessaria al take off
'  Dim pTestDuration As Double ' secondi dopo superata la bs limite per dove l accelerazione viene considerata conclusa

'  Public Property ListaAccelerazioni As ObservableCollection(Of clsAcceleration)
'    Get
'      Return pListaAccelerazioni
'    End Get
'    Set(value As ObservableCollection(Of clsAcceleration))
'      pListaAccelerazioni = value
'    End Set
'  End Property

'  Public Property BsLimite As Double
'    Get
'      Return pBsLimite
'    End Get
'    Set(value As Double)
'      pBsLimite = value
'    End Set
'  End Property

'  Public Property TakeOffSpeed As Double
'    Get
'      Return pTakeOffSpeed
'    End Get
'    Set(value As Double)
'      pTakeOffSpeed = value
'    End Set
'  End Property

'  Public Property TestDuration As Double
'    Get
'      Return pTestDuration
'    End Get
'    Set(value As Double)
'      pTestDuration = value
'    End Set
'  End Property

'  Public Sub AggiornaColori()
'    'Dim mColori As List(Of System.Windows.Media.Color) = ScalaColoriScuri(pListaAccelerazioni.Count)
'    For i As Integer = 0 To pListaAccelerazioni.Count - 1
'      pListaAccelerazioni(i).Periodo.Colore = ColoriDifferenziati(i)
'    Next
'  End Sub

'  Public Sub EliminaAccelerazione(Periodo As clsPeriod2020)
'    For Each Acc As clsAcceleration In pListaAccelerazioni
'      If Acc.Periodo Is Periodo Then
'        ListaAccelerazioni.Remove(Acc)
'        Exit For
'      End If
'    Next

'  End Sub

'  Private Sub CaricaSettings()
'    Select Case DataProvider2020.FileType
'      Case clsDataProvider2020.eFileType.eGombocSqlLite
'        pBsLimite = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", 10, True, True)
'        pTakeOffSpeed = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", 18, True, True)
'        pTestDuration = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TestDuration", 60, True, True)
'      Case clsDataProvider2020.eFileType.eFaRoCsv, clsDataProvider2020.eFileType.eFaRoBin, clsDataProvider2020.eFileType.eParquet
'        pBsLimite = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", 10, True, True)
'        pTakeOffSpeed = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", 14.5, True, True)
'        pTestDuration = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TestDuration", 60, True, True)
'      Case Else
'        pBsLimite = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", 10, True, True)
'        pTakeOffSpeed = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", 14.5, True, True)
'        pTestDuration = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TestDuration", 60, True, True)
'    End Select
'  End Sub

'  Public Function AggiungiAccelerazione(Periodo As clsPeriod2020) As Boolean
'    If pListaAccelerazioni.Count = 0 Then
'      CaricaSettings()
'    End If

'    Dim Elemento As New clsAcceleration(Periodo)
'    Dim PeriodoValido As Boolean = Elemento.ImpostaAccelerationKeyMoments(pBsLimite, pTakeOffSpeed, pTestDuration, False)

'    If PeriodoValido Then pListaAccelerazioni.Add(Elemento)
'    Return PeriodoValido
'  End Function

'  Public Sub AggiornaAccelerazioni(BsLimite As Double, TakeOffSpeed As Double, TestDuration As Double)
'    pBsLimite = BsLimite
'    pTestDuration = TestDuration
'    pTakeOffSpeed = TakeOffSpeed
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", pBsLimite, True, False)
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", pTakeOffSpeed, True, False)
'    AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "AccTotSeconds", pTestDuration, True, True)

'    For Each Accelerazione In pListaAccelerazioni
'      Accelerazione.ImpostaAccelerationKeyMoments(pBsLimite, pTakeOffSpeed, pTestDuration, False)
'    Next
'  End Sub

'  Public Sub StampaTabellaToClipboard()
'    Dim Intestazione As String = ""
'    Dim Righe As New List(Of String)
'    For Each acc In pListaAccelerazioni
'      If acc.Periodo.IsChecked Then
'        Righe.Add(acc.StringaAccelerazione(Intestazione, True))
'      End If
'    Next
'    Righe.Insert(0, Intestazione)
'    Righe.Add("")
'    Righe.Add("")
'    Righe.Add("Data Details")

'    Dim Contatore As Integer = 0
'    For Each acc In pListaAccelerazioni
'      If acc.Periodo.IsChecked Then
'        Dim strTmp As String = acc.StringaAccelerazione(Intestazione, False)
'        If Contatore = 0 Then Righe.Add(Intestazione)
'        Righe.Add(strTmp)
'        Contatore += 1
'      End If
'    Next

'    Clipboard.SetText(String.Join(vbCrLf, Righe))
'  End Sub

'End Class

'Public Class clsAcceleration
'  Implements INotifyPropertyChanged

'  Dim pPeriodo As clsPeriod2020

'  Dim pMomentOfInizioAccelerazione As clsAccelerationMoment
'  Dim pMomentOfTakeOffSpeed As clsAccelerationMoment
'  Dim pMomentOfBSatTgt As clsAccelerationMoment
'  Dim pMomentOfVMGatTgt As clsAccelerationMoment
'  Dim pMomentOfFineTestAccelerazione As clsAccelerationMoment
'  Dim pMatriceGeoPos As New List(Of clsGeograficPosition)
'  Dim pMatriceGeoPosRelToPuntoIniziale As New List(Of PointF)


'  Dim pValoriCanaleId As List(Of Integer?)
'  Dim pValoriCanaleTime As List(Of Double?)
'  Dim pValoriCanaleSpeed As List(Of Double?)
'  Dim pValoriCanaleTwd As List(Of Double?)
'  Dim pValoriCanaleTwa As List(Of Double?)
'  Dim pValoriCanaleTws As List(Of Double?)
'  Dim pValoriCanaleLat As List(Of Double?)
'  Dim pValoriCanaleLon As List(Of Double?)
'  Dim pHz As Integer

'  Dim CG As New clsCalcoliDuePuntiGeo()
'  Dim Lat As Double
'  Dim Lon As Double
'  Dim pPuntoRef As clsGeograficPosition
'  Dim Asse As Double
'  Dim _MedieFinali As clsMediePeriodo
'  Dim _MedieTakeOff As clsMediePeriodo
'  Dim _Descrizione As String
'  Dim _IsUpwind As Boolean
'  Dim _MinBs As Double

'  Dim _BsAtMinSinkZero As Double = -1
'  Dim _SecMinimiPerBsAtRhZero As Double = 2
'  Dim _BStgt As Double = -1
'  Dim _VmgTgt As Double = -1


'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Public Sub New(Periodo As clsPeriod2020)
'    pPeriodo = Periodo
'    'pIdRigaIniziale = pPeriodo.TimeRange.IdRigaIniziale
'    'pIdRigaFinale = System.Math.Min(pPeriodo.TimeRange.IdRigaFinale, dataProvider2020.TimeStamps.Count - 1)
'    pMomentOfInizioAccelerazione = New clsAccelerationMoment(pPeriodo)
'    pMomentOfTakeOffSpeed = New clsAccelerationMoment(pPeriodo)
'    pMomentOfBSatTgt = New clsAccelerationMoment(pPeriodo)
'    pMomentOfVMGatTgt = New clsAccelerationMoment(pPeriodo)
'    pMomentOfFineTestAccelerazione = New clsAccelerationMoment(pPeriodo)
'    ImpostaAccelerationKeyMoments(PeriodsManager.AccelerationSettings2020.BsLimite, PeriodsManager.AccelerationSettings2020.TakeOffSpeed, PeriodsManager.AccelerationSettings2020.TestDuration, False)
'    'AggiornaDescrizione()
'  End Sub

'  Private Sub AggiornaDescrizione()
'    Descrizione = pMomentOfInizioAccelerazione.Momento.ToShortDateString & " " & pMomentOfInizioAccelerazione.Momento.ToLongTimeString & " (" & Format(pMomentOfFineTestAccelerazione.Momento.Subtract(pMomentOfInizioAccelerazione.Momento).TotalSeconds, "F0") & "s), Tws: " & Format(pPeriodo.ValoriCanaleTWS.Avg, "F1") & ", SecToTakeOff: " & Format(TimeToTakeOffSpeed.TotalSeconds, "F0") & ""
'  End Sub

'  Private Sub AggiornaDescrizioneDb()
'    'Descrizione = pDbPeriod.TimeRange.Inizio.ToLongTimeString & " (" & pDbPeriod.TimeRange.DurataInStringaConSeparatore & "), BsTws:" & Format(FinalBsTwsRatio, "F3") & ", Tws:" & Format(pValoriCanaleTws.Average, "F1") & ", fTwa:" & Format(FinalAbsTWA, "F0") & ", Tm2TakeOffSpeed:" & Format(TimeToTakeOffSpeed.TotalSeconds, "F0") & "s"
'    Descrizione = pMomentOfInizioAccelerazione.Momento.ToShortDateString & " " & pMomentOfInizioAccelerazione.Momento.ToLongTimeString & " (" & Format(pMomentOfFineTestAccelerazione.Momento.Subtract(pMomentOfInizioAccelerazione.Momento).TotalSeconds, "F0") & "s), Tws: " & Format(pValoriCanaleTws.Average, "F1") & ", fTwa:" & Format(FinalAbsTWA, "F0") & ", SecToTakeOff: " & Format(TimeToTakeOffSpeed.TotalSeconds, "F0") & ""
'  End Sub

'  Public ReadOnly Property IsTWAinRange(Range As clsDoubleRange) As Boolean
'    Get
'      If Range Is Nothing Then
'        Return True
'      Else
'        Dim AbsTwa As Double = FinalAbsTWA
'        Return (System.Math.Abs(AbsTwa) >= Range.Min And System.Math.Abs(AbsTwa) <= Range.Max)
'      End If
'    End Get
'  End Property

'  Public Property Periodo As clsPeriod2020
'    Get
'      Return pPeriodo
'    End Get
'    Set(value As clsPeriod2020)
'      pPeriodo = value
'      AggiornaDescrizione()
'      OnPropertyChanged("Periodo")
'    End Set
'  End Property

'  Public Property MomentOfBSatTgt As clsAccelerationMoment
'    Get
'      Return pMomentOfBSatTgt
'    End Get
'    Set(value As clsAccelerationMoment)
'      pMomentOfBSatTgt = value
'      OnPropertyChanged("BSatTgt")
'    End Set
'  End Property

'  Public Property MomentOfTakeOffSpeed As clsAccelerationMoment
'    Get
'      Return pMomentOfTakeOffSpeed
'    End Get
'    Set(value As clsAccelerationMoment)
'      pMomentOfTakeOffSpeed = value
'      OnPropertyChanged("TakeOffSpeed")
'    End Set
'  End Property

'  Public Property MomentOfVMGatTgt As clsAccelerationMoment
'    Get
'      Return pMomentOfVMGatTgt
'    End Get
'    Set(value As clsAccelerationMoment)
'      pMomentOfVMGatTgt = value
'      OnPropertyChanged("VMGatTgt")
'    End Set
'  End Property

'  Public Property MomentOfInizioAccelerazione As clsAccelerationMoment
'    Get
'      Return pMomentOfInizioAccelerazione
'    End Get
'    Set(value As clsAccelerationMoment)
'      pMomentOfInizioAccelerazione = value
'      OnPropertyChanged("InizioAccelerazione")
'    End Set
'  End Property

'  Public Property MatriceGeoPos As List(Of clsGeograficPosition)
'    Get
'      Return pMatriceGeoPos
'    End Get
'    Set(value As List(Of clsGeograficPosition))
'      pMatriceGeoPos = value
'    End Set
'  End Property

'  Public Property MatriceGeoPosRelToPuntoIniziale As List(Of PointF)
'    Get
'      Return pMatriceGeoPosRelToPuntoIniziale
'    End Get
'    Set(value As List(Of PointF))
'      pMatriceGeoPosRelToPuntoIniziale = value
'    End Set
'  End Property

'  Public ReadOnly Property IsStbd As Boolean
'    Get
'      If pPeriodo Is Nothing Then
'        Return pValoriCanaleTwa.Average.Value >= 0
'      Else
'        Return pPeriodo.ValoriCanaleTWA.AvgOrg >= 0
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property IsUpwind As Boolean
'    Get
'      Return _IsUpwind
'    End Get
'  End Property

'  Public ReadOnly Property FinalAbsTWA As Double
'    Get
'      Return System.Math.Abs(FinalTWA)
'    End Get
'  End Property

'  Public ReadOnly Property FinalTWA As Double
'    Get
'      If pPeriodo Is Nothing Then
'        Dim Twa = pValoriCanaleTwa.ToList.GetRange(pMomentOfFineTestAccelerazione.IdRiga - pHz, pHz)
'        Return Twa.Average.Value
'      Else
'        Return _MedieFinali.TWAmedia ' DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(MomentOfFineTestAccelerazione.IdRiga).Value
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property TakeOffAbsTWA As Double
'    Get
'      Return System.Math.Abs(TakeOffTWA)
'    End Get
'  End Property

'  Public ReadOnly Property TakeOffTWA As Double
'    Get
'      If pPeriodo Is Nothing Then
'        Dim Twa = pValoriCanaleTwa.ToList.GetRange(MomentOfTakeOffSpeed.IdRiga - pHz, pHz)
'        Return Twa.Average.Value
'      Else
'        Return _MedieTakeOff.TWAmedia ' DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(MomentOfFineTestAccelerazione.IdRiga).Value
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property FinalSpeed As Double
'    Get
'      If pPeriodo Is Nothing Then
'        Dim bs = pValoriCanaleSpeed.ToList.GetRange(pMomentOfFineTestAccelerazione.IdRiga - pHz, pHz)
'        Return bs.Average.Value
'      Else
'        Return _MedieFinali.SpeedMedia ' DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(MomentOfFineTestAccelerazione.IdRiga).Value
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property FinalVmg As Double
'    Get
'      If pPeriodo Is Nothing Then
'        Return System.Math.Abs(FinalSpeed * System.Math.Cos(Radians(FinalAbsTWA)))
'      Else
'        Return _MedieFinali.VMGmedia ' DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(MomentOfFineTestAccelerazione.IdRiga).Value
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property FinalBsTwsRatio As Double
'    Get
'      If pPeriodo Is Nothing Then
'        Dim Bs = pValoriCanaleSpeed.ToList.GetRange(pMomentOfFineTestAccelerazione.IdRiga - pHz, pHz)
'        Dim Tws = pValoriCanaleTws.ToList.GetRange(pMomentOfFineTestAccelerazione.IdRiga - pHz, pHz)
'        Return Bs.Average.Value / Tws.Average.Value
'      Else
'        Return _MedieFinali.SpeedMedia / _MedieFinali.TWSMedia
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property TestDurationInSeconds As Double
'    Get
'      Return pMomentOfFineTestAccelerazione.Momento.Subtract(pMomentOfInizioAccelerazione.Momento).TotalSeconds
'    End Get
'  End Property

'  Public ReadOnly Property TimeToTakeOffSpeed As TimeSpan
'    Get
'      Return pMomentOfTakeOffSpeed.Momento.Subtract(pMomentOfInizioAccelerazione.Momento)
'    End Get
'  End Property

'  Public Property MomentOfFineTestAccelerazione As clsAccelerationMoment
'    Get
'      Return pMomentOfFineTestAccelerazione
'    End Get
'    Set(value As clsAccelerationMoment)
'      pMomentOfFineTestAccelerazione = value
'    End Set
'  End Property

'  Public Property PuntoRef As clsGeograficPosition
'    Get
'      Return pPuntoRef
'    End Get
'    Set(value As clsGeograficPosition)
'      pPuntoRef = value
'    End Set
'  End Property

'  Public Property Descrizione As String
'    Get
'      Return _Descrizione
'    End Get
'    Set(value As String)
'      _Descrizione = value
'      OnPropertyChanged("Descrizione")
'    End Set
'  End Property

'  Public Property ValoriCanaleSpeed As List(Of Double?)
'    Get
'      Return pValoriCanaleSpeed
'    End Get
'    Set(value As List(Of Double?))
'      pValoriCanaleSpeed = value
'    End Set
'  End Property

'  Public Property ValoriCanaleTwa As List(Of Double?)
'    Get
'      Return pValoriCanaleTwa
'    End Get
'    Set(value As List(Of Double?))
'      pValoriCanaleTwa = value
'    End Set
'  End Property

'  Public Property ValoriCanaleTime As List(Of Double?)
'    Get
'      Return pValoriCanaleTime
'    End Get
'    Set(value As List(Of Double?))
'      pValoriCanaleTime = value
'    End Set
'  End Property

'  Public Property ValoriCanaleId As List(Of Integer?)
'    Get
'      Return pValoriCanaleId
'    End Get
'    Set(value As List(Of Integer?))
'      pValoriCanaleId = value
'    End Set
'  End Property

'  Public ReadOnly Property MetriVmgFineTest As Double
'    Get
'      Return pMatriceGeoPosRelToPuntoIniziale(pMomentOfFineTestAccelerazione.IdRiga - pMomentOfInizioAccelerazione.IdRiga).Y
'    End Get
'  End Property

'  Public ReadOnly Property MetriVmgtakeOff As Double
'    Get
'      Return pMatriceGeoPosRelToPuntoIniziale(pMomentOfTakeOffSpeed.IdRiga - pMomentOfInizioAccelerazione.IdRiga).Y
'    End Get
'  End Property

'  Public ReadOnly Property MinBs As Double
'    Get
'      Return _MinBs
'    End Get
'  End Property

'  Public Property BsAtMinSinkZero As Double
'    Get
'      Return _BsAtMinSinkZero
'    End Get
'    Set(value As Double)
'      _BsAtMinSinkZero = value
'    End Set
'  End Property

'  Public Property BStgt As Double
'    Get
'      Return _BStgt
'    End Get
'    Set(value As Double)
'      _BStgt = value
'    End Set
'  End Property

'  Public Property VmgTgt As Double
'    Get
'      Return _VmgTgt
'    End Get
'    Set(value As Double)
'      _VmgTgt = value
'    End Set
'  End Property

'  Private Sub CalcolaPuntiXY(IdRigaInizioAccelerazione As Integer, IdRigaFinale As Integer)
'    pMatriceGeoPos.Clear()
'    pMatriceGeoPosRelToPuntoIniziale.Clear()

'    If IdRigaInizioAccelerazione = -1 Then
'      IdRigaInizioAccelerazione = pPeriodo.TimeRange.IdRigaIniziale
'    End If
'    Lat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(IdRigaInizioAccelerazione)
'    Lon = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(IdRigaInizioAccelerazione)
'    pPuntoRef = New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'    Asse = pPeriodo.ValoriCanaleTWD.Avg
'    For Riga As Integer = IdRigaInizioAccelerazione To IdRigaFinale
'      Lat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(Riga)
'      Lon = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(Riga)
'      If Not (Double.IsNaN(Lat) OrElse Double.IsNaN(Lon)) Then
'        Dim GeoPoint As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'        pMatriceGeoPos.Add(GeoPoint)
'        CG.CalcolaDistanzaAndRotta(pPuntoRef, GeoPoint)
'        Dim RNG As Double = CG.DistanzaMetri
'        Dim BRG As Double = CG.Rotta
'        Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Asse, BRG)
'        Dim BRGc As Double = BRGinCartesiano(BRGoc)
'        Dim X As Double = RNG * System.Math.Cos(Radians(BRGc))
'        Dim Y As Double = RNG * System.Math.Sin(Radians(BRGc))
'        Dim PuntoTraccia As New PointF(X, Y)
'        pMatriceGeoPosRelToPuntoIniziale.Add(PuntoTraccia)
'      Else
'        pMatriceGeoPos.Add(Nothing) ' il punto viene marcato come IsEmpty
'        pMatriceGeoPosRelToPuntoIniziale.Add(Nothing)
'      End If
'    Next
'    If IdRigaInizioAccelerazione = IdRigaFinale Then
'      _IsUpwind = True
'    Else
'      _IsUpwind = pMatriceGeoPosRelToPuntoIniziale.ToList.GetRange(pMatriceGeoPosRelToPuntoIniziale.Count - 10, 10).Select(Function(a) a.Y).Average > 0
'    End If
'  End Sub

'  'Private Sub CalcolaPuntiXyDb(IdRigaInizioAccelerazione As Integer, IdRigaFinale As Integer)
'  '  pMatriceGeoPos.Clear()
'  '  pMatriceGeoPosRelToPuntoIniziale.Clear()

'  '  If IdRigaInizioAccelerazione < 0 Then
'  '    IdRigaInizioAccelerazione = 0
'  '  Else
'  '  End If
'  '  Lat = pValoriCanaleLat(IdRigaInizioAccelerazione)
'  '  Lon = pValoriCanaleLon(IdRigaInizioAccelerazione)
'  '  pPuntoRef = New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'  '  Asse = pValoriCanaleTwd.Average.Value
'  '  Dim MetriTotaliVmg As Double = 0
'  '  For Riga As Integer = IdRigaInizioAccelerazione To IdRigaFinale
'  '    Try
'  '      Lat = pValoriCanaleLat(Riga)
'  '      Lon = pValoriCanaleLon(Riga)
'  '      Dim GeoPoint As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'  '      pMatriceGeoPos.Add(GeoPoint)
'  '      CG.CalcolaDistanzaAndRotta(pPuntoRef, GeoPoint)
'  '      Dim RNG As Double = CG.Distanza
'  '      Dim BRG As Double = CG.Rotta
'  '      Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Asse, BRG)
'  '      Dim BRGc As Double = BRGinCartesiano(BRGoc)
'  '      Dim X As Double = RNG * System.Math.Cos(Radians(BRGc))
'  '      Dim Y As Double = RNG * System.Math.Sin(Radians(BRGc))
'  '      Dim PuntoTraccia As New PointF(X, Y)
'  '      pMatriceGeoPosRelToPuntoIniziale.Add(PuntoTraccia)
'  '      MetriTotaliVmg += Y
'  '    Catch ex As Exception
'  '      Stop
'  '    End Try
'  '  Next
'  '  pIsUpwind = pMatriceGeoPosRelToPuntoIniziale.Last.Y > 0
'  'End Sub

'  Private Function IdRigaSuperamentoBsLimite(BsLimite As Double) As Integer
'    Dim RigaTmp As Integer = -1
'    For Riga As Integer = pPeriodo.TimeRange.IdRigaIniziale To pPeriodo.TimeRange.IdRigaFinale
'      Dim Bs As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori(Riga)
'      If RigaTmp > -1 Then
'        If Riga > RigaTmp + 5 * pHz Then Exit For
'        If Bs < BsLimite Then
'          RigaTmp = -1 ' se la velocità va sotto quella limite ricomincia a cercare
'        End If
'      Else
'        If Bs >= BsLimite Then
'          RigaTmp = Riga
'        End If
'      End If
'    Next
'    Return RigaTmp
'  End Function

'  Private Function SuperamentoBsLimite(BsLimite As Double) As DateTime
'    Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(pPeriodo.TimeRange.Start)
'    Dim IdFinale As Integer = DataProvider2020.TrovaIndice(pPeriodo.TimeRange.Finish)
'    Dim PrimoMomentoOk As DateTime = Nothing
'    For Riga As Integer = IdIniziale To IdFinale
'      Dim Momento As DateTime = DataProvider2020.Momento(Riga)
'      Dim Bs As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori(Riga)
'      If Not PrimoMomentoOk = Nothing Then
'        If Momento.Subtract(PrimoMomentoOk).TotalSeconds > 5 Then Exit For
'        If Bs < BsLimite Then
'          PrimoMomentoOk = Nothing ' se la velocità va sotto quella limite ricomincia a cercare
'        End If
'      Else
'        If Bs >= BsLimite Then
'          PrimoMomentoOk = Momento
'        End If
'      End If
'    Next
'    Return PrimoMomentoOk
'  End Function


'  Private Function IdRigaSuperamentoValoreCanale(ValoreLimite As Double, Canale As clsChannel2020) As Integer
'    If ValoreLimite = -1 Then Return pMomentOfFineTestAccelerazione.IdRiga
'    For Riga As Integer = pMomentOfInizioAccelerazione.IdRiga To pMomentOfFineTestAccelerazione.IdRiga
'      Dim Bs As Double = Canale.Valori(Riga)
'      If Bs >= ValoreLimite Then
'        Return Riga
'      End If
'    Next
'    Return pMomentOfFineTestAccelerazione.IdRiga
'  End Function

'  Private Function IdRigaSuperamentoValoreCanale(ValoreLimite As Double, Canale As clsChannel2020, SecondiMinimi As Double) As Integer
'    If ValoreLimite = -1 Then Return pMomentOfFineTestAccelerazione.IdRiga
'    Dim MomentoIniziale As DateTime = Nothing
'    For Riga As Integer = pMomentOfInizioAccelerazione.IdRiga To pMomentOfFineTestAccelerazione.IdRiga
'      Dim Bs As Double = Canale.Valori(Riga)
'      If Bs >= ValoreLimite Then
'        If MomentoIniziale = Nothing Then MomentoIniziale = DataProvider2020.Momento(Riga)
'        If DataProvider2020.Momento(Riga).Subtract(MomentoIniziale).TotalSeconds >= SecondiMinimi Then
'          Return DataProvider2020.TrovaIndice(MomentoIniziale)
'        End If
'      Else
'        MomentoIniziale = Nothing
'      End If
'    Next
'    Return pMomentOfFineTestAccelerazione.IdRiga
'  End Function

'  Public Function ImpostaAccelerationKeyMoments(BsLimite As Double, TakeOffSpeed As Double, TestDuration As Double, UsaRefTargetDaTws As Boolean) As Boolean
'    Try

'      Dim MomentoSupBsLimite As DateTime = SuperamentoBsLimite(BsLimite)
'      Dim IdRigaTmp As Integer = DataProvider2020.TrovaIndice(MomentoSupBsLimite)
'      If MomentoSupBsLimite = Nothing Then
'        Return False
'      End If
'      If MomentoSupBsLimite = Periodo.TimeRange.Start Then
'        Dim ValoriTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori.ToList.ToList.GetRange(pPeriodo.TimeRange.IdRigaIniziale, pPeriodo.TimeRange.IdRigaFinale - pPeriodo.TimeRange.IdRigaIniziale).Where(Function(x) Not Double.IsNaN(x))
'        _MinBs = ValoriTmp.Min
'      End If
'      Dim IdRigaFineTest As Integer = System.Math.Min(DataProvider2020.TimeRange.IdRigaFinale, DataProvider2020.TrovaIndice(MomentoSupBsLimite.AddSeconds(TestDuration)))

'      CalcolaPuntiXY(IdRigaTmp, IdRigaFineTest)
'      If IdRigaTmp = IdRigaFineTest Then Return False
'      pMomentOfInizioAccelerazione.AggiornaDati(BsLimite, IdRigaTmp, pMatriceGeoPos(0), pMatriceGeoPosRelToPuntoIniziale(0))
'      pMomentOfFineTestAccelerazione.AggiornaDati(TestDuration, IdRigaFineTest, pMatriceGeoPos(IdRigaFineTest - pMomentOfInizioAccelerazione.IdRiga), pMatriceGeoPosRelToPuntoIniziale(IdRigaFineTest - pMomentOfInizioAccelerazione.IdRiga))


'      _MedieFinali = New clsMediePeriodo(New clsTimeRange(pMomentOfFineTestAccelerazione.Momento.AddSeconds(-2), pMomentOfFineTestAccelerazione.Momento))

'      Dim SecAnteEndForAverages As Double = 30
'      Dim Intervallo As New clsTimeRange(pMomentOfFineTestAccelerazione.Momento.AddSeconds(-SecAnteEndForAverages), pMomentOfFineTestAccelerazione.Momento)
'      Dim CanaleTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
'      Dim viTws As New clsStatisticheIntervallo(CanaleTws, Intervallo)
'      If TgtManager.Tgt Is Nothing Then
'        If UsaRefTargetDaTws Then
'          'CanaleTws.ValoriIntervallo.AggiornaValori(New clsTimeRange(pMomentOfFineTestAccelerazione.Momento.AddSeconds(-SecAnteEndForAverages), pMomentOfFineTestAccelerazione.Momento), False)
'          Stop
'        Else
'          Dim CanaleBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'          Dim CanaleVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'          Dim viBs As New clsStatisticheIntervallo(CanaleBs, Intervallo)
'          Dim viVmg As New clsStatisticheIntervallo(CanaleVmg, Intervallo)
'          BStgt = viBs.Avg
'          VmgTgt = viVmg.Avg
'          'CanaleBs.ValoriIntervallo.AggiornaValori(, False)
'          'CanaleVmg.ValoriIntervallo.AggiornaValori(New clsTimeRange(pMomentOfFineTestAccelerazione.Momento.AddSeconds(-SecAnteEndForAverages), pMomentOfFineTestAccelerazione.Momento), False)
'          'BStgt = CanaleBs.ValoriIntervallo.Avg
'          'VmgTgt = CanaleVmg.ValoriIntervallo.Avg
'        End If
'      Else
'        'Dim CanaleTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
'        'CanaleTws.ValoriIntervallo.AggiornaValori(New clsTimeRange(pMomentOfFineTestAccelerazione.Momento.AddSeconds(-SecAnteEndForAverages), pMomentOfFineTestAccelerazione.Momento), False)
'        'Dim v = TgtManager.Tgt.ValoreTgt(IsUpwind, CanaleTws.ValoriIntervallo.Avg, "bs")
'        Dim v = TgtManager.Tgt.ValoreTgt(IsUpwind, viTws.Avg, "bs")
'        BStgt = v.Bs
'        VmgTgt = v.Vmg
'      End If
'      IdRigaTmp = IdRigaSuperamentoValoreCanale(TakeOffSpeed, DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW))
'      pMomentOfTakeOffSpeed.AggiornaDati(TakeOffSpeed, IdRigaTmp, pMatriceGeoPos(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga), pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga))
'      _MedieTakeOff = New clsMediePeriodo(New clsTimeRange(pMomentOfTakeOffSpeed.Momento.AddSeconds(-0.5), pMomentOfTakeOffSpeed.Momento.AddSeconds(0.5)))


'      IdRigaTmp = IdRigaSuperamentoValoreCanale(BStgt, DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW))
'      pMomentOfBSatTgt.AggiornaDati(BStgt, IdRigaTmp, pMatriceGeoPos(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga), pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga))
'      IdRigaTmp = IdRigaSuperamentoValoreCanale(VmgTgt, DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG))
'      pMomentOfVMGatTgt.AggiornaDati(VmgTgt, IdRigaTmp, pMatriceGeoPos(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga), pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp - pMomentOfInizioAccelerazione.IdRiga))


'      'Dim CanaleRH As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRideHeight)
'      'If Not CanaleRH Is Nothing Then
'      '  IdRigaTmp = IdRigaSuperamentoValoreCanale(0.01, CanaleRH, _SecMinimiPerBsAtRhZero)
'      '  Dim CanaleBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'      '  If Not CanaleBs Is Nothing Then
'      '    _BsAtMinSinkZero = CanaleBs.Valori(IdRigaTmp)
'      '    For i As Integer = 0 To 100
'      '      _BsAtMinSinkZero = CanaleBs.Valori(IdRigaTmp + i)
'      '      If Not Double.IsNaN(_BsAtMinSinkZero) Then
'      '        Exit For
'      '      End If
'      '    Next
'      '  End If
'      'End If


'      Periodo.AggiornaDescrizionePeriodo()
'    Catch ex As Exception
'      Return False
'    End Try
'    Return True
'  End Function


'  Public Function StringaAccelerazione(ByRef Descrizione As String, FirstPart As Boolean) As String

'    Dim DescrizioneRigaAlta As String = "" & vbTab
'    Descrizione = "Acceleration Time" & vbTab
'    DescrizioneRigaAlta &= "" & vbTab ' Format(pMomentOfFineTestAccelerazione.Valore, "F0") & " sec" & vbTab
'    Descrizione &= "UpDn" & vbTab
'    DescrizioneRigaAlta &= "" & vbTab
'    Descrizione &= "Tack" & vbTab
'    DescrizioneRigaAlta &= "" & vbTab
'    Descrizione &= "TWS" & vbTab

'    If FirstPart Then
'      DescrizioneRigaAlta &= "TakeOff" & vbTab
'      Descrizione &= "VmgMeters" & vbTab
'      DescrizioneRigaAlta &= "" & vbTab
'      Descrizione &= "LatMeters" & vbTab
'      DescrizioneRigaAlta &= "" & vbTab
'      Descrizione &= "Seconds" & vbTab
'      DescrizioneRigaAlta &= "Final" & vbTab
'      Descrizione &= "VmgMeters" & vbTab
'      DescrizioneRigaAlta &= "" & vbTab
'      Descrizione &= "LatMeters" & vbTab
'      DescrizioneRigaAlta &= "" & vbTab
'      Descrizione &= "Seconds" & vbTab
'    Else
'      DescrizioneRigaAlta &= "Cant" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "EffectiveCant" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "InFlap" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "OutFlap" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "RdrRake" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "EffectiveRdrRake" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Heel" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Trim" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Bs" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Vmg" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Twa" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Awa" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'      DescrizioneRigaAlta &= "Aws" & vbTab & "" & vbTab & "" & vbTab
'      Descrizione &= "Initial" & vbTab & "TakeOff" & vbTab & "Final" & vbTab
'    End If
'    Descrizione = DescrizioneRigaAlta & vbCrLf & Descrizione

'    Dim strTmp As String = pMomentOfInizioAccelerazione.Momento.ToShortDateString & " " & pMomentOfInizioAccelerazione.Momento.ToLongTimeString & vbTab
'    strTmp &= If(FinalAbsTWA > 90, "Downwind", "Upwind") & vbTab
'    strTmp &= If(FinalTWA > 0, "Stbd", "Port") & vbTab
'    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
'    Dim viTws As New clsStatisticheIntervallo(chTws, New clsTimeRange(pMomentOfInizioAccelerazione.Momento, pMomentOfFineTestAccelerazione.Momento))
'    'chTws.ValoriIntervallo.AggiornaValori(New clsTimeRange(pMomentOfInizioAccelerazione.Momento, pMomentOfFineTestAccelerazione.Momento), False)
'    strTmp &= Format(viTws.Avg, "F1") & vbTab
'    'strTmp &= Format(chTws.ValoriIntervallo.Avg, "F1") & vbTab

'    If FirstPart Then

'      Dim IdRigaTmp As Integer = pMomentOfTakeOffSpeed.IdRiga - pMomentOfInizioAccelerazione.IdRiga
'      If IdRigaTmp < pMatriceGeoPosRelToPuntoIniziale.Count Then
'        strTmp &= Format(IIf(IsUpwind, -1, 1) * pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp).Y, "F0") & vbTab
'        strTmp &= Format(System.Math.Abs(pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp).X), "F0") & vbTab
'      Else
'        strTmp &= "-" & vbTab
'        strTmp &= "-" & vbTab
'      End If
'      strTmp &= Format(pMomentOfTakeOffSpeed.Momento.Subtract(pMomentOfInizioAccelerazione.Momento).TotalMilliseconds / 1000, "F1") & vbTab
'      IdRigaTmp = pMomentOfFineTestAccelerazione.IdRiga - pMomentOfInizioAccelerazione.IdRiga
'      If IdRigaTmp < pMatriceGeoPosRelToPuntoIniziale.Count Then
'        strTmp &= Format(IIf(IsUpwind, -1, 1) * pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp).Y, "F0") & vbTab
'        strTmp &= Format(System.Math.Abs(pMatriceGeoPosRelToPuntoIniziale(IdRigaTmp).X), "F0") & vbTab
'      Else
'        strTmp &= "-" & vbTab
'        strTmp &= "-" & vbTab
'      End If
'      strTmp &= Format(pMomentOfFineTestAccelerazione.Momento.Subtract(pMomentOfInizioAccelerazione.Momento).TotalMilliseconds / 1000, "F0") & vbTab
'    Else
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl("PortCantAngle"))
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl("PortCantAngleEffective"))
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl("PortFoilInFlap1Angle"))
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl("PortFoilOutFlap1Angle"))
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrRake))
'      'strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrRakeEffective))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA))
'      strTmp &= StringaValori(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWS))
'    End If


'    Return strTmp
'  End Function

'  Private Function StringaValori(Canale As clsChannel2020) As String
'    Return Format(Canale.Valori(pMomentOfInizioAccelerazione.IdRiga), "F" & Canale.Decimals.ToString) & vbTab & Format(Canale.Valori(pMomentOfTakeOffSpeed.IdRiga), "F" & Canale.Decimals.ToString) & vbTab & Format(Canale.Valori(pMomentOfFineTestAccelerazione.IdRiga), "F" & Canale.Decimals.ToString) & vbTab
'  End Function

'End Class


'Public Class clsAccelerationMoment
'  Dim pMomento As DateTime
'  Dim pPuntoGeo As clsGeograficPosition
'  Dim pPuntoRel As PointF
'  Dim pIdRiga As Integer
'  Dim pPeriodoParent As clsPeriod2020
'  Dim pValore As Double

'  Public Sub New(PeriodoParent As clsPeriod2020)
'    pPeriodoParent = PeriodoParent
'  End Sub

'  Public Property Valore As Double
'    Get
'      Return pValore
'    End Get
'    Set(value As Double)
'      pValore = Valore
'    End Set
'  End Property

'  Public Property PuntoGeo As clsGeograficPosition
'    Get
'      Return pPuntoGeo
'    End Get
'    Set(value As clsGeograficPosition)
'      pPuntoGeo = value
'    End Set
'  End Property

'  Public Property PuntoRel As PointF
'    Get
'      Return pPuntoRel
'    End Get
'    Set(value As PointF)
'      pPuntoRel = value
'    End Set
'  End Property

'  Public Property IdRiga As Integer
'    Get
'      Return pIdRiga
'    End Get
'    Set(value As Integer)
'      pIdRiga = value
'    End Set
'  End Property

'  Public Property PeriodoParent As clsPeriod2020
'    Get
'      Return pPeriodoParent
'    End Get
'    Set(value As clsPeriod2020)
'      pPeriodoParent = value
'    End Set
'  End Property

'  Public Property Momento As Date
'    Get
'      Return pMomento
'    End Get
'    Set(value As Date)
'      pMomento = value
'    End Set
'  End Property

'  Public Sub AggiornaDati(Valore As Double, IdRiga As Integer, PuntoGeo As clsGeograficPosition, PuntoRel As PointF)
'    pValore = Valore
'    pIdRiga = IdRiga
'    pMomento = DataProvider2020.TimeStamps(pIdRiga)
'    pPuntoGeo = PuntoGeo
'    pPuntoRel = PuntoRel
'  End Sub

'  Public Sub AggiornaDati(Valore As Double, IdRiga As Integer, Momento As DateTime, PuntoGeo As clsGeograficPosition, PuntoRel As PointF)
'    pValore = Valore
'    pIdRiga = IdRiga
'    pMomento = Momento
'    pPuntoGeo = PuntoGeo
'    pPuntoRel = PuntoRel
'  End Sub

'End Class



'Public Class clsAccBazzosDataDetails
'  Dim pTime As Integer 'secondi dal keymoment
'  Dim pTws As Double
'  Dim pTwa As Double
'  Dim pBs As Double
'  Dim pXy As PointF

'  Public Sub New()
'  End Sub

'  Public Sub New(Time As Integer, Tws As Double, Twa As Double, Bs As Double, Xy As PointF)
'    'valori medi
'    pTime = Time
'    pTws = Tws
'    pTwa = Twa
'    pBs = Bs
'    pXy = Xy
'  End Sub

'  Public Sub New(Acc As clsAcceleration, Time As Integer)
'    pTime = Time
'    pTws = Acc.Periodo.ValoriCanaleTWS.Avg
'    Dim Momento As DateTime = Acc.MomentOfInizioAccelerazione.Momento.AddSeconds(pTime)
'    Dim IdMomento As Integer = DataProvider2020.TrovaIndice(Momento)
'    pXy = CreaPuntoVmgLat(Momento, Acc).PuntoCartesianoDaRef.ToPointF
'    pTwa = System.Math.Abs(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori(IdMomento))
'    pBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori(IdMomento)
'  End Sub

'  Public Property Time As Integer
'    Get
'      Return pTime
'    End Get
'    Set(value As Integer)
'      pTime = value
'    End Set
'  End Property

'  Public Property Tws As Double
'    Get
'      Return pTws
'    End Get
'    Set(value As Double)
'      pTws = value
'    End Set
'  End Property

'  Public Property Twa As Double
'    Get
'      Return pTwa
'    End Get
'    Set(value As Double)
'      pTwa = value
'    End Set
'  End Property

'  Public Property Bs As Double
'    Get
'      Return pBs
'    End Get
'    Set(value As Double)
'      pBs = value
'    End Set
'  End Property

'  Public Property Xy As PointF
'    Get
'      Return pXy
'    End Get
'    Set(value As PointF)
'      pXy = value
'    End Set
'  End Property


'  Public Function CreaPuntoVmgLat(Momento As DateTime, Acc As clsAcceleration) As clsPuntoGeografico
'    Dim CampoLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    Dim CampoLong As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'    Dim IdRiga As Integer = DataProvider2020.TrovaIndice(Momento)
'    If Double.IsNaN(CampoLat.Valori(IdRiga)) OrElse Double.IsNaN(CampoLong.Valori(IdRiga)) Then Return Nothing
'    Dim IdRigaRef As Double = DataProvider2020.TrovaIndice(Acc.MomentOfInizioAccelerazione.Momento)
'    Dim PuntoRef As New clsGeograficPosition(CampoLat.Valori(IdRigaRef), CampoLong.Valori(IdRigaRef))
'    Dim PuntoGeo As New clsGeograficPosition(CampoLat.Valori(IdRiga), CampoLong.Valori(IdRiga))
'    Dim CG As New clsCalcoliDuePuntiGeo
'    CG.CalcolaDistanzaAndRotta(PuntoRef, PuntoGeo)
'    Dim RNG As Double = CG.DistanzaMetri
'    Dim BRG As Double = CG.Rotta
'    Dim BRGoc As Double = DifferenzaAssolutaTraAngoli360(Acc.Periodo.ValoriCanaleTWD.Avg, BRG, True)
'    Dim BRGc As Double = BRGinCartesiano(BRGoc)
'    Dim MetriLat As Double = RNG * System.Math.Cos(Radians(BRGc))
'    Dim MetriVmg As Double = RNG * System.Math.Sin(Radians(BRGc))
'    Return New clsPuntoGeografico(MetriVmg, MetriLat, PuntoGeo, Momento)
'  End Function

'End Class

'Public Class clsAccBazzosData
'  Dim pAccBazzosDataDetails As New List(Of clsAccBazzosDataDetails)
'  Dim pTws As Double

'  Public Sub New()
'  End Sub

'  Public Sub New(Acc As clsAcceleration, Times As Integer())
'    pTws = Acc.Periodo.ValoriCanaleTWS.Avg
'    For Each Tempo In Times
'      pAccBazzosDataDetails.Add(New clsAccBazzosDataDetails(Acc, Tempo))
'    Next
'  End Sub

'  Public Property AccDetails As List(Of clsAccBazzosDataDetails)
'    Get
'      Return pAccBazzosDataDetails
'    End Get
'    Set(value As List(Of clsAccBazzosDataDetails))
'      pAccBazzosDataDetails = value
'    End Set
'  End Property

'  Public Property Tws As Double
'    Get
'      Return pTws
'    End Get
'    Set(value As Double)
'      pTws = value
'    End Set
'  End Property
'End Class

'Public Class clsAccsBazzosData
'  Dim pTimes As Integer() = {-7, 0, 10, 30, 50}
'  Dim pUpEnd As New List(Of clsAccBazzosData)
'  Dim pDnEnd As New List(Of clsAccBazzosData)
'  Dim pAvgTacks As New List(Of clsAccBazzosDataDetails) 'una per intero di tws
'  Dim pAvgGybes As New List(Of clsAccBazzosDataDetails) 'una per intero di tws

'  Public Property UpEnd As List(Of clsAccBazzosData)
'    Get
'      Return pUpEnd
'    End Get
'    Set(value As List(Of clsAccBazzosData))
'      pUpEnd = value
'    End Set
'  End Property

'  Public Property DnEnd As List(Of clsAccBazzosData)
'    Get
'      Return pDnEnd
'    End Get
'    Set(value As List(Of clsAccBazzosData))
'      pDnEnd = value
'    End Set
'  End Property

'  Public Sub New()
'    CalcolaDati()
'  End Sub

'  Private Sub CalcolaDati()
'    For Each Acc In PeriodsManager.ListaAccelerazioni
'      If Acc.IsChecked Then
'        If Acc.DettagliAcceleration.IsUpwind Then
'          pUpEnd.Add(New clsAccBazzosData(Acc.DettagliAcceleration, pTimes))
'        Else
'          pDnEnd.Add(New clsAccBazzosData(Acc.DettagliAcceleration, pTimes))
'        End If
'      End If
'    Next

'    'Dim d = Gybes.GroupBy(Function(x) x.AccDetails).GroupBy(Function(y) New With {AccDetails = y.Key, .Name = "paperclips", .Price = 1.29}


'  End Sub

'  Public Sub EsportaJson()
'    Dim path As String = DataProvider2020.Files.First.Directory.FullName
'    Dim pstringadata As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd")
'    clsKillerSeriale.SaveConfigurationGeneric(Of List(Of clsAccBazzosData))(UpEnd.ToList, path & "\" & pstringadata & "_EndingUpwind.json")
'    clsKillerSeriale.SaveConfigurationGeneric(Of List(Of clsAccBazzosData))(DnEnd.ToList, path & "\" & pstringadata & "_EndingDownwind.json")
'    ApriExplorer(path, path & "\" & pstringadata & "_EndingDownwind.json")
'  End Sub

'End Class
