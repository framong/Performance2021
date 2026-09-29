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
Imports SQLitePCL
Imports SciChart.Charting2D.Interop
Imports Newtonsoft.Json
Imports PropertyChanged

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotSelectionSettings


  Public Property _PerformanceSelected As Boolean = True
  Public Property _TechnicalSelected As Boolean = False

  Public Property _StbdSelected As Boolean = True
  Public Property _PortSelected As Boolean = True
  Public Property _UpwindSelected As Boolean = True
  Public Property _DownwindSelected As Boolean = True
  Public Property _PlotTrendLines As Boolean = False

  Public Property WheelZoomEnabled As Boolean = True
  Public Property _MouseModeSelEnabled As Boolean = True
  Public Property MouseModeSelDescription As String = "mMode: Sel"
  Public Property ZoomPanEnabled As Boolean = False
  Public Property DataPointSelEnabled As Boolean = True

  Public Property _ApplyTwsFilter As Boolean = False
  Public Property _ApplyTwsMin As Boolean = False
  Public Property _TwsMin As Double = 6
  Public Property _ApplyTwsMax As Boolean = False
  Public Property _TwsMax As Double = 25

  Public Event SettingChanged()

  Public Property PerformanceSelected As Boolean
    Get
      Return _PerformanceSelected
    End Get
    Set(value As Boolean)
      _PerformanceSelected = value
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

  Public Property PlotTrendLines As Boolean
    Get
      Return _PlotTrendLines
    End Get
    Set(value As Boolean)
      _PlotTrendLines = value
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
      RaiseEvent SettingChanged()
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
      RaiseEvent SettingChanged()
    End Set
  End Property

  Public ReadOnly Property TwsMinEnabled As Boolean
    Get
      Return _ApplyTwsFilter AndAlso _ApplyTwsMin
    End Get
  End Property

  Public ReadOnly Property TwsMaxEnabled As Boolean
    Get
      Return _ApplyTwsFilter AndAlso _ApplyTwsMax
    End Get
  End Property

  Public Property MouseModeSelEnabled As Boolean
    Get
      Return _MouseModeSelEnabled
    End Get
    Set(value As Boolean)
      _MouseModeSelEnabled = value
      If _MouseModeSelEnabled Then
        MouseModeSelDescription = "mMode: Sel"
        ZoomPanEnabled = False
        DataPointSelEnabled = True
      Else
        ZoomPanEnabled = True
        DataPointSelEnabled = False
        MouseModeSelDescription = "mMode: Pan"
      End If
      'OnPropertyChanged("MouseModeSelEnabled")
    End Set
  End Property

  Public Sub VerificaIsCheckedPavarots()
    If Not PeriodsManager Is Nothing Then
      For Each sl In PeriodsManager.CollectionPavarot
        sl.IsChecked = PeriodIsSelected(sl)
      Next
    End If
  End Sub

  Public Function PeriodIsSelected(Period As clsPeriod2021) As Boolean
    Dim Valid As Boolean = False
    If (Not Period.TechnicalOrSystems) And PerformanceSelected Then
      Valid = True
    ElseIf Period.TechnicalOrSystems And TechnicalSelected Then
      Valid = True
    End If
    If Not Valid Then Return False
    Valid = VerificaPavarot(Period)
    If Not Valid Then Return False
    Return TwsValido(Period.TwsDetails.AvgVal)

  End Function

  Private Function VerificaPavarot(Period As clsPeriod2021) As Boolean
    If Period.PavarotDetails Is Nothing Then Return False
    If UpwindSelected And Period.PeriodType = clsPeriod2021.ePeriodType.eTack Then
      If StbdSelected And Period.IsStbd Then Return True
      If PortSelected And Not Period.IsStbd Then Return True
    ElseIf DownwindSelected And Period.PeriodType = clsPeriod2021.ePeriodType.eGybe Then
      If StbdSelected And Period.IsStbd Then Return True
      If PortSelected And Not Period.IsStbd Then Return True
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

Public Class clsLastPavarotPlot
  Public OutputType As UserControlPavarotViewModel.eOutputType
  Public CheckedPavarots As New List(Of clsPeriod2021)
  'Public SecAnte As Integer = AppConfig.ActiveProfile.PeriodsSettings.SecsAnteManoeuver
  'Public SecPost As Integer = AppConfig.ActiveProfile.PeriodsSettings.SecsPostManoeuver

  Public SecAnteTack As Integer = 0
  Public SecPostTack As Integer = 0
  Public SecAnteGybe As Integer = 0
  Public SecPostGybe As Integer = 0

  Public Enum eResult
    eRefreshNotNeeded = 0
    eVisiblePeriodsRefresh = 1
    eDataRefresh = 2
  End Enum


  Public Sub New(ActualSettings As clsPavarotSettings)
    If Not ActualSettings Is Nothing Then
      SecAnteTack = ActualSettings.TackSecAnte
      SecPostTack = ActualSettings.TackSecPost
      SecAnteTack = ActualSettings.TackSecAnte
      SecPostTack = ActualSettings.TackSecPost
    End If
  End Sub

  Public Function DataHasChanged(ActualSettings As clsPavarotSettings, ActualCheckedPavarots As List(Of clsPeriod2021), ActualOutputType As UserControlPavarotViewModel.eOutputType) As eResult
    'Public Function DataHasChanged(ActualSettings As clsPeriodsSettings2021, ActualCheckedPavarots As List(Of clsPeriod2021), ActualOutputType As UserControlPavarotViewModel.eOutputType) As eResult
    If ActualOutputType = OutputType Then
      ' e' lo stesso tipo di output di prima
      If ActualOutputType = UserControlPavarotViewModel.eOutputType.eColorByTack OrElse ActualOutputType = UserControlPavarotViewModel.eOutputType.eSinglePeriod Then
        ' se l output e' a singoli periodi 
        If SettingsChanged(ActualSettings) Then
          ' ricalcolare se e' cambiato il range
          AppConfig.Salva()
          AggiornaLastSettings(ActualSettings, ActualCheckedPavarots, ActualOutputType)
          Return eResult.eDataRefresh
        ElseIf PeriodsChanged(ActualCheckedPavarots) Then
          ' aggiornare i soli disegni se sono cambiati i periodi selezionati
          AggiornaLastSettings(ActualSettings, ActualCheckedPavarots, ActualOutputType)
          Return eResult.eVisiblePeriodsRefresh
        Else
          Return eResult.eRefreshNotNeeded
        End If
      Else
        ' se l output e' raggruppato il calcolo va rifatto se sono cambiati i periodi checkati
        If SettingsChanged(ActualSettings) OrElse PeriodsChanged(ActualCheckedPavarots) Then
          AppConfig.Salva()
          AggiornaLastSettings(ActualSettings, ActualCheckedPavarots, ActualOutputType)
          Return eResult.eDataRefresh
        Else
          Return eResult.eRefreshNotNeeded
        End If
      End If
    Else
      ' diverso output
      AggiornaLastSettings(ActualSettings, ActualCheckedPavarots, ActualOutputType)
      Return eResult.eDataRefresh
    End If
  End Function

  Private Function SettingsChanged(ActualSettings As clsPavarotSettings) As Boolean
    'Private Function SettingsChanged(ActualSettings As clsPeriodsSettings2021) As Boolean
    If Not SecAnteTack = ActualSettings.TackSecAnte Then Return True
    If Not SecPostTack = ActualSettings.TackSecPost Then Return True
    If Not SecAnteTack = ActualSettings.TackSecAnte Then Return True
    If Not SecPostTack = ActualSettings.TackSecPost Then Return True
    'If Not SecAnte = ActualSettings.SecsAnteManoeuver Then Return True
    'If Not SecPost = ActualSettings.SecsPostManoeuver Then Return True
    Return False
  End Function

  Private Function PeriodsChanged(ActualCheckedPavarots As List(Of clsPeriod2021)) As Boolean
    Dim Cambiati As Boolean = Not (ActualCheckedPavarots.Count = CheckedPavarots.Count)
    If Not Cambiati Then
      For i As Integer = 0 To ActualCheckedPavarots.Count - 1
        If Not CheckedPavarots(i).KeyMoment = ActualCheckedPavarots(i).KeyMoment Then
          Cambiati = True
          Exit For
        Else
          If Not CheckedPavarots(i).TR.HasSameRange(ActualCheckedPavarots(i).TR) Then
            Cambiati = True
            Exit For
          End If
        End If
      Next
    End If
    Return Cambiati
  End Function

  'Private Sub AggiornaLastSettings(ActualSettings As clsPeriodsSettings2021, ActualPeriodiCheckati As List(Of clsPeriod2021), ActualOutputType As UserControlPavarotViewModel.eOutputType)
  Private Sub AggiornaLastSettings(ActualSettings As clsPavarotSettings, ActualPeriodiCheckati As List(Of clsPeriod2021), ActualOutputType As UserControlPavarotViewModel.eOutputType)
    OutputType = ActualOutputType
    CheckedPavarots = ActualPeriodiCheckati.ToList
    SecAnteTack = ActualSettings.TackSecAnte
    SecPostTack = ActualSettings.TackSecPost
    SecAnteTack = ActualSettings.TackSecAnte
    SecPostTack = ActualSettings.TackSecPost
  End Sub


End Class


<AddINotifyPropertyChangedInterface>
Public Class UserControlPavarotViewModel
  'Implements INotifyPropertyChanged

  Public Property LastPavarotPlot As clsLastPavarotPlot

  Public Property GainLossVsTwsSeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property AnnotazioniGainLossVsTws As New SciChart.Charting.Visuals.Annotations.AnnotationCollection

  Public Property PavarotSyncViewModel As New clsPavarotSyncViewModel
  Public Property PavarotAdvSyncViewModel As New clsPavarotSyncViewModel
  Public Property PavarotBasicSyncViewModel As New clsPavarotSyncViewModel

  Public Property ControlliPavarotBasicPlot As clsPavarotControls
  Public Property ControlliPavarotBasicAvg As clsPavarotAdvancedPlotContainerViewModel


  Public Property CmdAggiungiCanale As New clsComando(AddressOf AggiungiGrafico)

  Public Property CurrentPeriodDescription As String
  Public Property PeriodDetailsFontSize As Double = 16

  Public Property AggiornaGraficiAdvanced As Boolean = True
  'Public Property LastUsedPavarotSettings2020 As clsPavarotSettings2020

  Public WithEvents PavarotSelectionSettings As New clsPavarotSelectionSettings
  Dim _OutputType As eOutputType = eOutputType.eSinglePeriod
  Public Property AutoRefreshGroupBy As Boolean
  Public Property PlotTrendLines As Boolean = False
  Public Property PlotTargetsIfAvailable As Boolean = True
  Public Property ListaPeriodiCaricati As List(Of clsPeriod2021)

  Public Property ListaControlliCustom As New List(Of UserControlPavarotPlotViewModelMathPlots.eCanaleCustom)


  Public Enum eOutputType
    eSinglePeriod = 0
    eColorByTack = 1
    eGroupByTack = 2
    eGroupByKey = 3
    eGroupByTackAndKey = 4
  End Enum

  Public Property PavSettings As clsPavarotSettings
    Get
      Return AppConfig.ActiveProfile.PavarotSettings
    End Get
    Set(value As clsPavarotSettings)
      AppConfig.ActiveProfile.PavarotSettings = value
    End Set
  End Property




  Public Sub New()
    LastPavarotPlot = New clsLastPavarotPlot(AppConfig.ActiveProfile.PavarotSettings)
    'AppConfig.ActiveProfile.PavarotSettings.GybeSecPost
    'Stop
  End Sub

  Private Sub PavarotSyncViewModelPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
    Select Case e.PropertyName
      Case "SharedXVisibleRange"
        PavarotVisibleRange.Max = PavarotSyncViewModel.SharedXVisibleRange.Max
        PavarotVisibleRange.Min = PavarotSyncViewModel.SharedXVisibleRange.Min
    End Select
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Public Event GraficiAggiornati()

  Public Property OutputType As eOutputType
    Get
      Return _OutputType
    End Get
    Set(value As eOutputType)
      _OutputType = value
      VerificaAutoRefresh()
    End Set
  End Property


  Private Sub VerificaAutoRefresh()
    If OutputType = eOutputType.eSinglePeriod OrElse OutputType = eOutputType.eColorByTack Then
      AggiornaGraficiPavarot(False)
    Else
      If AutoRefreshGroupBy Then AggiornaGraficiPavarot(False)
    End If
  End Sub

  Public Sub CurrentPeriod(Period As clsPeriod2021)
    If Period Is Nothing Then
      CurrentPeriodDescription = ""
    Else
      CurrentPeriodDescription = Period.DescrizioneMultiriga
    End If
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
      Return PeriodsManager.Periods.CollectionPavarot
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      PeriodsManager.Periods.CollectionPavarot = value
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

  'Public Property PavarotSettings As clsPeriodsSettings2021
  '    Get
  '        Return AppConfig.ActiveProfile.PeriodsSettings
  '    End Get
  '    Set(value As clsPeriodsSettings2021)
  '        AppConfig.ActiveProfile.PeriodsSettings = value
  '    End Set
  'End Property

  Public Sub CreaReportPdf(VMGscSurface As SciChart.Charting.Visuals.SciChartSurface)
    If ControlliPavarotBasicPlot Is Nothing Then Exit Sub
    If ControlliPavarotBasicPlot.ListaControlli Is Nothing Then Exit Sub
    If ControlliPavarotBasicPlot.ListaControlli.Count = 0 Then Exit Sub
    Dim Lista As New List(Of SciChart.Charting.Visuals.SciChartSurface)
    For Each c In ControlliPavarotBasicPlot.ListaControlli
      Lista.Add(c.Plot)
    Next
    Dim ListaXY As New List(Of SciChart.Charting.Visuals.SciChartSurface)
    ListaXY.Add(VMGscSurface)
    For Each c In ControlliPavarotBasicAvg.ListaControlli
      ListaXY.Add(c.Plot)
    Next

    Dim objPdf As New clsPdf
    objPdf.StampReportPavarot(Lista, ListaXY)
  End Sub

  Public Sub Recalc()
    RicalcolaGraficiPavarot()
  End Sub

  Public Sub Refresh()
    AggiornaGraficiPavarot(True)
  End Sub

  Public Sub PavarotTabGotFocus()
    AggiornaGraficiPavarot(False)
  End Sub

  Public Sub AggiornaGraficiPavarot(ForzaRefresh As Boolean)
    Dim TmrPV As DateTime = Now
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If ControlliPavarotBasicPlot Is Nothing Then
      ControlliPavarotBasicPlot = New clsPavarotControls(Me)
      ForzaRefresh = True
    End If
    If ControlliPavarotBasicAvg Is Nothing Then
      ControlliPavarotBasicAvg = New clsPavarotAdvancedPlotContainerViewModel(PavarotBasicSyncViewModel, Me, clsPavarotAdvancedPlotContainerViewModel.eCollection.eBasicAvg)
      ForzaRefresh = True
    End If
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " PV.1 - contenitori creati " & Now.Subtract(TmrPV).TotalMilliseconds.ToString("F0") & " ms")
    TmrPV = Now
    Dim ListaChecked = PeriodsManager.ListaPavarot.Where(Function(x) x.IsChecked = True).ToList
    'Dim Risultato As clsLastPavarotPlot.eResult = LastPavarotPlot.DataHasChanged(AppConfig.ActiveProfile.PeriodsSettings, ListaChecked, OutputType)
    Dim Risultato As clsLastPavarotPlot.eResult = LastPavarotPlot.DataHasChanged(AppConfig.ActiveProfile.PavarotSettings, ListaChecked, OutputType)
    If ForzaRefresh Then Risultato = clsLastPavarotPlot.eResult.eDataRefresh
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " PV.2 - DataHasChanged: " & Risultato.ToString() & " " & Now.Subtract(TmrPV).TotalMilliseconds.ToString("F0") & " ms")
    TmrPV = Now
    Select Case Risultato
      Case clsLastPavarotPlot.eResult.eDataRefresh
        AggiornaGrafici()
        If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " PV.3 - AggiornaGrafici " & Now.Subtract(TmrPV).TotalMilliseconds.ToString("F0") & " ms")
        TmrPV = Now
        AggiornaPeriodiVisibili()
        If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " PV.4 - AggiornaPeriodiVisibili " & Now.Subtract(TmrPV).TotalMilliseconds.ToString("F0") & " ms")
      Case clsLastPavarotPlot.eResult.eVisiblePeriodsRefresh
        AggiornaPeriodiVisibili()
        If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & " PV.4 - AggiornaPeriodiVisibili " & Now.Subtract(TmrPV).TotalMilliseconds.ToString("F0") & " ms")
      Case clsLastPavarotPlot.eResult.eRefreshNotNeeded
    End Select

  End Sub

  Public Sub RicalcolaGraficiPavarot()
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    If PeriodsManager Is Nothing Then Exit Sub
    If PeriodsManager.CollectionPavarot Is Nothing Then Exit Sub

    For Each p In PeriodsManager.CollectionPavarot
      p.UpdateDetails()
    Next

  End Sub


  Private Function Visibilitá(Value As Boolean) As Boolean
    If Value Then
      Return Visibility.Visible
    Else
      Return Visibility.Hidden
    End If
  End Function

  Public Sub AggiornaPeriodiVisibili()
    Try

      'If PeriodsManager.PeriodsTrigger.ListaInAggiornamento Then Exit Sub
      Dim AtLeastOneTack As Boolean = False
      Dim AtLeastOneGybe As Boolean = False
      For Each linea In GainLossVsTwsSeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)

        If TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then

        ElseIf TypeOf (LineaFS.Tag) Is String Then
          Select Case LineaFS.Tag.ToString
            Case "BenchUp"
            Case "BenchDn"
            Case "TgtUp"
            Case "TgtDn"
          End Select

        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          If Not Periodo Is Nothing Then
            LineaFS.IsVisible = Periodo.IsChecked
            If Periodo.IsChecked Then
              AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
              AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
            End If
          End If
        End If
      Next

      For Each linea In GainLossVsTwsSeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then

        ElseIf TypeOf (LineaFS.Tag) Is String Then
          Select Case LineaFS.Tag.ToString
            Case "BenchUp"
              LineaFS.IsVisible = AtLeastOneTack
            Case "BenchDn"
              LineaFS.IsVisible = AtLeastOneGybe
            Case "TgtUp"
              LineaFS.IsVisible = AtLeastOneTack
            Case "TgtDn"
              LineaFS.IsVisible = AtLeastOneGybe
          End Select
        ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
          Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          If Not Periodo Is Nothing Then
            LineaFS.IsVisible = Periodo.IsChecked
            If Periodo.IsChecked Then
              AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
              AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
            End If
          End If
        End If
      Next

      If ControlliPavarotBasicPlot Is Nothing Then Exit Sub
      For Each Controllo In ControlliPavarotBasicPlot.ListaControlli
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then

          ElseIf TypeOf (LineaFS.Tag) Is String Then
            Select Case LineaFS.Tag.ToString
              Case "BenchUp"
                LineaFS.IsVisible = AtLeastOneTack
              Case "BenchDn"
                LineaFS.IsVisible = AtLeastOneGybe
              Case "TgtUp"
                LineaFS.IsVisible = AtLeastOneTack
              Case "TgtDn"
                LineaFS.IsVisible = AtLeastOneGybe
            End Select
          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            If Not Periodo Is Nothing Then
              LineaFS.IsVisible = Periodo.IsChecked
              If Periodo.IsChecked Then
                AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
                AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
              End If
            End If
          End If

        Next
      Next

      If ControlliPavarotBasicAvg Is Nothing Then Exit Sub
      For Each Controllo In ControlliPavarotBasicAvg.ListaControlli
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then

          ElseIf TypeOf (LineaFS.Tag) Is String Then
            Select Case LineaFS.Tag.ToString
              Case "BenchUp"
                LineaFS.IsVisible = AtLeastOneTack
              Case "BenchDn"
                LineaFS.IsVisible = AtLeastOneGybe
              Case "TgtUp"
                LineaFS.IsVisible = AtLeastOneTack
              Case "TgtDn"
                LineaFS.IsVisible = AtLeastOneGybe
            End Select
          ElseIf TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            If Not Periodo Is Nothing Then
              LineaFS.IsVisible = Periodo.IsChecked
              If Periodo.IsChecked Then
                AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
                AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
              End If
            End If
          End If

        Next
      Next
      RaiseEvent GraficiAggiornati()
    Catch ex As Exception
      Stop
    End Try

  End Sub

  Public Sub LineaSelezionata(SailingTack As clsSailingState.eTack)
    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
    For Each linea In GainLossVsTwsSeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If Not IsNumeric(LineaFS.Tag) Then
        If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then

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
      End If
    Next
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(Periodo As clsPeriod2021)
    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
    For Each linea In GainLossVsTwsSeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If Not IsNumeric(LineaFS.Tag) Then
        If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
          Dim Prd As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          If Prd Is Periodo Then
            Selezione.Add(LineaFS)
          End If
        End If
      End If
    Next
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(Periodi As List(Of clsPeriod2021))
    Dim Selezione As New ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries)
    For Each linea In GainLossVsTwsSeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If Not IsNumeric(LineaFS.Tag) Then
        If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
          Dim Prd As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
          If Periodi.Where(Function(x) x Is Prd).Count > 0 Then
            Selezione.Add(LineaFS)
          End If
        End If
      End If
    Next
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarotAdvancedPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarotAdvancedPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarotPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    LineaSelezionata(Selezione)
  End Sub

  Public Sub LineaSelezionata(ControlloCorrente As UserControlPavarotPlot, Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    LineaSelezionata(Selezione)
  End Sub

  Private Sub AggiornaColoriLista(Lista As ObservableCollection(Of UserControlPavarotPlot), Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
    If Selezione.Count = 0 Then
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Periodo.ColoreSfondo = SfondoBase
              LineaFS.Stroke = Periodo.Colore
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Periodo.Colore
                LineaFS.PointMarker.Fill = Periodo.Colore
              End If
            End If
          End If
        Next
      Next
    Else
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
              Periodo.ColoreSfondo = ColoreNonSelezionati
              LineaFS.Stroke = ColoreNonSelezionati
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = ColoreNonSelezionati
                LineaFS.PointMarker.Fill = ColoreNonSelezionati
              End If
              For Each pp In listOk
                Periodo.ColoreSfondo = SfondoSelezionati
                LineaFS.Stroke = Periodo.Colore
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = Periodo.Colore
                  LineaFS.PointMarker.Fill = Periodo.Colore
                End If
              Next
            End If
          End If
        Next
      Next
    End If
  End Sub

  Private Sub AggiornaColoriLista(Lista As ObservableCollection(Of UserControlPavarotAdvancedPlot), Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
    If Selezione.Count = 0 Then
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Periodo.ColoreSfondo = SfondoBase
              LineaFS.Stroke = Periodo.Colore
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Periodo.Colore
                LineaFS.PointMarker.Fill = Periodo.Colore
              End If
            End If
          End If
        Next
        For Each Annotazione In Controllo.ViewModel.Annotazioni
          If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
            Dim BA As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
            If Not BA.Tag Is Nothing Then
              If Not IsNumeric(BA.Tag) Then
                If Not TypeOf (BA.Tag) Is clsGruppoPeriodi Then
                  Dim Periodo As clsPeriod2021 = DirectCast(BA.Tag, clsPeriod2021)
                  If Not Periodo Is Nothing Then
                    BA.Background = New SolidColorBrush(ColoreTrasparente(20, Periodo.Colore))
                    BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(100, Periodo.Colore))
                  End If
                End If
              End If
            End If
          End If
        Next
      Next
    Else
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
              Periodo.ColoreSfondo = ColoreNonSelezionati
              LineaFS.Stroke = ColoreNonSelezionati
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = ColoreNonSelezionati
                LineaFS.PointMarker.Fill = ColoreNonSelezionati
              End If
              For Each pp In listOk
                Periodo.ColoreSfondo = SfondoSelezionati
                LineaFS.Stroke = Periodo.Colore
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = Periodo.Colore
                  LineaFS.PointMarker.Fill = Periodo.Colore
                End If
              Next
            End If
          End If
        Next
        For Each Annotazione In Controllo.ViewModel.Annotazioni
          If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
            Dim BA As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
            If Not BA.Tag Is Nothing Then
              Dim Periodo As clsPeriod2021 = DirectCast(BA.Tag, clsPeriod2021)
              If Not Periodo Is Nothing Then
                Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
                BA.Background = New SolidColorBrush(ColoreTrasparente(20, ColoreNonSelezionati))
                BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(100, ColoreNonSelezionati))
                For Each pp In listOk
                  BA.Background = New SolidColorBrush(ColoreTrasparente(80, Periodo.Colore))
                  BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(150, Periodo.Colore))
                Next

              End If
            End If
          End If
        Next
      Next
    End If
  End Sub

  Private Sub AggiornaColoriLista(Lista As ObservableCollection(Of UserControlPavarotPlot), Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
    ' aggiorna i colori della lista secondo la selezione fatta
    If Selezione.Count = 0 Then
      'nessun DataPointInfo  selezionato
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            'periodi singoli e color by tack
            If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Periodo.ColoreSfondo = SfondoBase
              If OutputType = eOutputType.eColorByTack Then
                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
              Else
                LineaFS.Stroke = Periodo.Colore
              End If
              If Not LineaFS.PointMarker Is Nothing Then
                If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                  LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                Else
                  LineaFS.PointMarker.Stroke = Periodo.Colore
                  LineaFS.PointMarker.Fill = Periodo.Colore
                End If
              End If
            End If
          Else
            'group by tack
            If LineaFS.Tag = -1 Then
              LineaFS.Visibility = Visibility.Visible
              LineaFS.Stroke = Colors.Red
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Colors.Red
                LineaFS.PointMarker.Fill = Colors.Red
              End If
            ElseIf LineaFS.Tag = 1 Then
              LineaFS.Visibility = Visibility.Visible
              LineaFS.Stroke = Colors.Green
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Colors.Green
                LineaFS.PointMarker.Fill = Colors.Green
              End If
            End If
          End If
        Next
      Next
    Else
      For Each Controllo In Lista
        For Each linea In Controllo.Plot.RenderableSeries.ToList
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              If Not Periodo Is Nothing Then
                Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
                Periodo.ColoreSfondo = ColoreNonSelezionati
                LineaFS.Stroke = ColoreNonSelezionati
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = ColoreNonSelezionati
                  LineaFS.PointMarker.Fill = ColoreNonSelezionati
                End If
                For Each pp In listOk
                  Periodo.ColoreSfondo = SfondoSelezionati
                  If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                    LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  Else
                    LineaFS.Stroke = Periodo.Colore
                  End If
                  If Not LineaFS.PointMarker Is Nothing Then
                    If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                      LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                      LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                    Else
                      LineaFS.PointMarker.Stroke = Periodo.Colore
                      LineaFS.PointMarker.Fill = Periodo.Colore
                    End If
                  End If
                  'LineaFS.Stroke = Periodo.Colore
                  'If Not LineaFS.PointMarker Is Nothing Then
                  '  LineaFS.PointMarker.Stroke = Periodo.Colore
                  '  LineaFS.PointMarker.Fill = Periodo.Colore
                  'End If
                Next
              End If
            End If
          Else
            If LineaFS.Tag = -1 Then
              LineaFS.Stroke = Colors.Red
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Colors.Red
                LineaFS.PointMarker.Fill = Colors.Red
              End If
            ElseIf LineaFS.Tag = 1 Then
              LineaFS.Stroke = Colors.Green
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Colors.Green
                LineaFS.PointMarker.Fill = Colors.Green
              End If
            End If
          End If
        Next
      Next
    End If
  End Sub

  Private Sub AggiornaColoriLista(Lista As ObservableCollection(Of UserControlPavarotAdvancedPlot), Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    Try

      Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
      ColoreNonSelezionati.A = 20
      Dim SfondoBase As Windows.Media.Color = Colors.White
      Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
      If Selezione.Count = 0 Then
        For Each Controllo In Lista
          For Each linea In Controllo.Plot.RenderableSeries.ToList
            Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
            If Not IsNumeric(LineaFS.Tag) Then
              If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
                Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
                Periodo.ColoreSfondo = SfondoBase
                If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                  LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                Else
                  LineaFS.Stroke = Periodo.Colore
                End If
                If Not LineaFS.PointMarker Is Nothing Then
                  If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                    LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                    LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  Else
                    LineaFS.PointMarker.Stroke = Periodo.Colore
                    LineaFS.PointMarker.Fill = Periodo.Colore
                  End If
                End If
              End If
            End If
          Next
          For Each Annotazione In Controllo.ViewModel.Annotazioni.ToList
            If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
              Dim BA As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
              If Not BA.Tag Is Nothing Then
                If IsNumeric(BA.Tag) Then
                Else
                  Dim Periodo As clsPeriod2021 = DirectCast(BA.Tag, clsPeriod2021)
                  If Not Periodo Is Nothing Then
                    BA.Background = New SolidColorBrush(ColoreTrasparente(20, Periodo.Colore))
                    BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(100, Periodo.Colore))
                  End If
                End If
              End If
            End If
          Next
        Next
      Else
        For Each Controllo In Lista
          For Each linea In Controllo.Plot.RenderableSeries.ToList
            Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
            If Not IsNumeric(LineaFS.Tag) Then
              If Not TypeOf (LineaFS.Tag) Is clsGruppoPeriodi Then
                Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
                Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
                Periodo.ColoreSfondo = ColoreNonSelezionati
                LineaFS.Stroke = ColoreNonSelezionati
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = ColoreNonSelezionati
                  LineaFS.PointMarker.Fill = ColoreNonSelezionati
                End If
                For Each pp In listOk
                  Periodo.ColoreSfondo = SfondoSelezionati
                  If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                    LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  Else
                    LineaFS.Stroke = Periodo.Colore
                  End If
                  If Not LineaFS.PointMarker Is Nothing Then
                    If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                      LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                      LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                    Else
                      LineaFS.PointMarker.Stroke = Periodo.Colore
                      LineaFS.PointMarker.Fill = Periodo.Colore
                    End If
                  End If
                  'LineaFS.Stroke = Periodo.Colore
                  'If Not LineaFS.PointMarker Is Nothing Then
                  '  LineaFS.PointMarker.Stroke = Periodo.Colore
                  '  LineaFS.PointMarker.Fill = Periodo.Colore
                  'End If
                Next
              End If
            Else
              If LineaFS.Tag = -1 Then
                LineaFS.Stroke = Colors.Red
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = Colors.Red
                  LineaFS.PointMarker.Fill = Colors.Red
                End If
              ElseIf LineaFS.Tag = 1 Then
                LineaFS.Stroke = Colors.Green
                If Not LineaFS.PointMarker Is Nothing Then
                  LineaFS.PointMarker.Stroke = Colors.Green
                  LineaFS.PointMarker.Fill = Colors.Green
                End If
              End If
            End If
          Next
          For Each Annotazione In Controllo.ViewModel.Annotazioni
            If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
              Dim BA As SciChart.Charting.Visuals.Annotations.BoxAnnotation = DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation)
              If Not BA.Tag Is Nothing Then
                If IsNumeric(BA.Tag) Then

                Else
                  Dim Periodo As clsPeriod2021 = DirectCast(BA.Tag, clsPeriod2021)
                  If Not Periodo Is Nothing Then
                    Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
                    BA.Background = New SolidColorBrush(ColoreTrasparente(20, ColoreNonSelezionati))
                    BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(100, ColoreNonSelezionati))
                    For Each pp In listOk
                      BA.Background = New SolidColorBrush(ColoreTrasparente(80, Periodo.Colore))
                      BA.BorderBrush = New SolidColorBrush(ColoreTrasparente(150, Periodo.Colore))
                    Next

                  End If
                End If
              End If
            End If
          Next
        Next
      End If
    Catch ex As Exception

    End Try
  End Sub

  Private Sub AggiornaColoriGainLossVsTws(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    Try
      Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
      ColoreNonSelezionati.A = 20
      Dim SfondoBase As Windows.Media.Color = Colors.White
      Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
      Dim AtLeastOneTack As Boolean = False
      Dim AtLeastOneGybe As Boolean = False

      If Selezione.Count = 0 Then
        'nulla selezionato
        For Each linea In GainLossVsTwsSeriesSource
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Periodo.ColoreSfondo = SfondoBase
              If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
              Else
                LineaFS.Stroke = Periodo.Colore
              End If
              If Not LineaFS.PointMarker Is Nothing Then
                If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                  LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                Else
                  LineaFS.PointMarker.Stroke = Periodo.Colore
                  LineaFS.PointMarker.Fill = Periodo.Colore
                End If
              End If
              AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
              AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
            End If
          ElseIf LineaFS.Tag < 10 Then
            AtLeastOneTack = True
            AtLeastOneGybe = True
          End If
        Next
      Else
        For Each linea In GainLossVsTwsSeriesSource
          Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
          If Not IsNumeric(LineaFS.Tag) Then
            If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then
              Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
              Dim listOk = Selezione.Where(Function(x) DirectCast(x.RenderableSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
              Periodo.ColoreSfondo = ColoreNonSelezionati
              LineaFS.Stroke = ColoreNonSelezionati
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = ColoreNonSelezionati
                LineaFS.PointMarker.Fill = ColoreNonSelezionati
              End If
              For Each pp In listOk
                Periodo.ColoreSfondo = SfondoSelezionati
                If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                  LineaFS.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                Else
                  LineaFS.Stroke = Periodo.Colore
                End If
                If Not LineaFS.PointMarker Is Nothing Then
                  If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
                    LineaFS.PointMarker.Stroke = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                    LineaFS.PointMarker.Fill = If(Periodo.IsStbd, Colors.Green, Colors.Red)
                  Else
                    LineaFS.PointMarker.Stroke = Periodo.Colore
                    LineaFS.PointMarker.Fill = Periodo.Colore
                  End If
                End If
                'LineaFS.Stroke = Periodo.Colore
                'If Not LineaFS.PointMarker Is Nothing Then
                '  LineaFS.PointMarker.Stroke = Periodo.Colore
                '  LineaFS.PointMarker.Fill = Periodo.Colore
                'End If
                AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
                AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
              Next
            End If
          ElseIf LineaFS.Tag < 10 Then
            AtLeastOneTack = True
            AtLeastOneGybe = True
          End If
        Next
      End If
      For Each linea In GainLossVsTwsSeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If IsNumeric(LineaFS.Tag) Then
          If LineaFS.Tag = 10 Then
            LineaFS.IsVisible = AtLeastOneTack
          ElseIf LineaFS.Tag = 20 Then
            LineaFS.IsVisible = AtLeastOneGybe
          End If
        End If
      Next

    Catch ex As Exception

    End Try

  End Sub

  Private Sub AggiornaColoriGainLossVsTws(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
    ColoreNonSelezionati.A = 20
    Dim SfondoBase As Windows.Media.Color = Colors.White
    Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False

    If Selezione.Count = 0 Then
      For Each linea In GainLossVsTwsSeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If Not IsNumeric(LineaFS.Tag) Then
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then

            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Periodo.ColoreSfondo = SfondoBase
            LineaFS.Stroke = Periodo.Colore
            If Not LineaFS.PointMarker Is Nothing Then
              LineaFS.PointMarker.Stroke = Periodo.Colore
              LineaFS.PointMarker.Fill = Periodo.Colore
            End If
            AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
            AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
          End If
        End If
      Next
    Else
      For Each linea In GainLossVsTwsSeriesSource
        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
        If Not IsNumeric(LineaFS.Tag) Then
          If TypeOf (LineaFS.Tag) Is clsPeriod2021 Then

            Dim Periodo As clsPeriod2021 = DirectCast(LineaFS.Tag, clsPeriod2021)
            Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is Periodo).ToList
            Periodo.ColoreSfondo = ColoreNonSelezionati
            LineaFS.Stroke = ColoreNonSelezionati
            If Not LineaFS.PointMarker Is Nothing Then
              LineaFS.PointMarker.Stroke = ColoreNonSelezionati
              LineaFS.PointMarker.Fill = ColoreNonSelezionati
            End If
            For Each pp In listOk
              Periodo.ColoreSfondo = SfondoSelezionati
              LineaFS.Stroke = Periodo.Colore
              If Not LineaFS.PointMarker Is Nothing Then
                LineaFS.PointMarker.Stroke = Periodo.Colore
                LineaFS.PointMarker.Fill = Periodo.Colore
              End If
              AtLeastOneTack = AtLeastOneTack OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eTack
              AtLeastOneGybe = AtLeastOneGybe OrElse Periodo.PeriodType = clsPeriod2021.ePeriodType.eGybe
            Next
          End If
        ElseIf LineaFS.Tag < 10 Then
          AtLeastOneTack = True
          AtLeastOneGybe = True
        End If
      Next
    End If
    For Each linea In GainLossVsTwsSeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If IsNumeric(LineaFS.Tag) Then
        If LineaFS.Tag = 10 Then
          LineaFS.IsVisible = AtLeastOneTack
        ElseIf LineaFS.Tag = 20 Then
          LineaFS.IsVisible = AtLeastOneGybe
        End If
      End If
    Next

  End Sub


  Public Sub LineaSelezionata(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
    AggiornaColoriLista(ControlliPavarotBasicPlot.ListaControlli, Selezione)
    AggiornaColoriLista(ControlliPavarotBasicAvg.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvRotation.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvBoardDrop.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvBoardUp.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvEntryExit.ListaControlli, Selezione)
    AggiornaColoriGainLossVsTws(Selezione)
  End Sub

  Public Sub LineaSelezionata(Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo))
    AggiornaColoriLista(ControlliPavarotBasicPlot.ListaControlli, Selezione)
    AggiornaColoriLista(ControlliPavarotBasicAvg.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvRotation.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvBoardDrop.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvBoardUp.ListaControlli, Selezione)
    'AggiornaColoriLista(ControlliPavarotAdvEntryExit.ListaControlli, Selezione)
    AggiornaColoriGainLossVsTws(Selezione)
  End Sub

  Private Function ColoreTrasparente(Trasparenza As Byte, Colorebase As Windows.Media.Color) As Windows.Media.Color
    Return Windows.Media.Color.FromArgb(Trasparenza, Colorebase.R, Colorebase.G, Colorebase.B)
  End Function

  Public Sub AggiornaGrafici()

    Dim TmrAG As DateTime = Now
    'aggiorna i grafici generali
    DrawPlotAvgVmgVsTws() ' questo e' il grafico a sinistra con la media del vmg da entry ad exit
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   PVG.1 - DrawPlotAvgVmgVsTws " & Now.Subtract(TmrAG).TotalMilliseconds.ToString("F0") & " ms")
    TmrAG = Now
    _DisegnoBasicPlot.Esegui(ControlliPavarotBasicPlot.ListaControlli,
        Sub(Controllo)
          Dim VM As UserControlPavarotPlotViewModel = DirectCast(Controllo.DataContext, UserControlPavarotPlotViewModel)
          VM.DrawChart()
          Controllo.Plot.ZoomExtents()
        End Sub,
        Sub()
          ' i grafici riempiti per ultimi devono ricevere lo stato di visibilita' corrente
          AggiornaPeriodiVisibili()
        End Sub)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   PVG.2 - disegno progressivo avviato x" & ControlliPavarotBasicPlot.ListaControlli.Count)
    TmrAG = Now

    ControlliPavarotBasicAvg.AggiornaGrafici() ' grafici in alto
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "   PVG.3 - ControlliPavarotBasicAvg " & Now.Subtract(TmrAG).TotalMilliseconds.ToString("F0") & " ms")

  End Sub

  Private _DisegnoBasicPlot As New clsDisegnoProgressivoPavarot

  Private Sub AggiornaSync()
    Stop
    'If PeriodsManager.ListaPavarot.Count = 0 Then Exit Sub
    'PavarotSyncViewModel.InizioCampionamentoEntry = PeriodsManager.ListaPavarot.First.DettagliPavarot.TimeRangeEntry.Start.Subtract(PeriodsManager.ListaPavarot.First.DettagliPavarot.KeyMoment).TotalSeconds
    'PavarotSyncViewModel.FineCampionamentoEntry = PeriodsManager.ListaPavarot.First.DettagliPavarot.PavarotEntry.Subtract(PeriodsManager.ListaPavarot.First.DettagliPavarot.KeyMoment).TotalSeconds 'PavarotSyncViewModel.Pavarots.First.SecondiFineCampionamentoAnte
    'PavarotSyncViewModel.InizioCampionamentoExit = PeriodsManager.ListaPavarot.First.DettagliPavarot.TimeRangeExit.Start.Subtract(PeriodsManager.ListaPavarot.First.DettagliPavarot.KeyMoment).TotalSeconds
    'PavarotSyncViewModel.FineCampionamentoExit = PeriodsManager.ListaPavarot.First.DettagliPavarot.TimeRangeExit.Finish.Subtract(PeriodsManager.ListaPavarot.First.DettagliPavarot.KeyMoment).TotalSeconds 'PavarotSyncViewModel.Pavarots.First.SecondiFineCampionamentoAnte
  End Sub

  Public Sub DrawPlotAvgVmgVsTws2021()
    GainLossVsTwsSeriesSource.Clear()
    Select Case OutputType
      Case eOutputType.eGroupByKey, eOutputType.eGroupByTackAndKey
        DrawPlotAvgVmgVsTwsGroupByKeys2021()
        Exit Sub
      Case eOutputType.eGroupByTack
        DrawPlotAvgVmgVsTwsGroupByTack2021()
        Exit Sub
    End Select
    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False
    Dim TL As New clsTrendLines



    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      'Dim Pavarot2020 As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If True Then
        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
        Dim LineaTmp As New XyScatterRenderableSeries
        LineaTmp.XAxisId = "DefaultAxisId"
        LineaTmp.YAxisId = "DefaultAxisId"
        AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
        AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe
        'If Period2021.IsStbd Then
        '  LineaTmp.PointMarker = New EllipsePointMarker
        'Else
        '  LineaTmp.PointMarker = New SquarePointMarker
        'End If
        LineaTmp.PointMarker = New EllipsePointMarker()

        If Pavarot.IsStbd Then
          LineaTmp.PointMarker.Stroke = Colors.Green
        Else
          LineaTmp.PointMarker.Stroke = Colors.Red
        End If
        If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
          If Pavarot.IsStbd Then
            LineaTmp.PointMarker.Fill = Colors.Green
          Else
            LineaTmp.PointMarker.Fill = Colors.Red
          End If
        Else
          LineaTmp.PointMarker.Fill = Pavarot.Colore
        End If

        LineaTmp.PointMarker.Height = 10
        LineaTmp.PointMarker.Width = 10
        LineaTmp.PointMarker.StrokeThickness = 1
        LineaTmp.Tag = Pavarot

        DataSeriesTMP.AcceptsUnsortedData = True

        Dim X As Double = Pavarot.TwsDetails.AvgVal
        If Not Double.IsNaN(X) Then
          If Not Double.IsInfinity(X) Then
            MaxTws = System.Math.Max(X, MaxTws)
            MinTws = System.Math.Min(X, MinTws)
          End If
        End If
        Dim Y As Double = Pavarot.PavarotDetails.InlineTgtLossMt
        'Dim Y As Double = Pavarot.PavarotDetails.VmgAvg
        DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
        TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
        LineaTmp.DataSeries = DataSeriesTMP
        Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
        GainLossVsTwsSeriesSource.Add(CSVMtmp)

      End If

    Next

    'DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)

    'TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    ImpostaAnnotazioniGainLossVsTws()

  End Sub


  Public Sub DrawPlotAvgVmgVsTws()
    DrawPlotAvgVmgVsTws2021()
    Exit Sub
    GainLossVsTwsSeriesSource.Clear()
    Select Case OutputType
      Case eOutputType.eGroupByKey, eOutputType.eGroupByTackAndKey
        DrawPlotAvgVmgVsTwsGroupByKeys()
        Exit Sub
      Case eOutputType.eGroupByTack
        DrawPlotAvgVmgVsTwsGroupByTack()
        Exit Sub
    End Select
    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False
    Dim TL As New clsTrendLines

    '    For Each Pavarot In PeriodsManager.ListaPavarot
    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For

      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New XyScatterRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
      AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

      If Pavarot.IsStbd Then
        LineaTmp.PointMarker = New EllipsePointMarker
      Else
        LineaTmp.PointMarker = New SquarePointMarker
      End If

      LineaTmp.PointMarker = New EllipsePointMarker()
      If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          LineaTmp.PointMarker.Stroke = Colors.Green
          LineaTmp.PointMarker.Fill = Colors.Green
        Else
          LineaTmp.PointMarker.Stroke = Colors.Red
          LineaTmp.PointMarker.Fill = Colors.Red
        End If
      Else
        LineaTmp.PointMarker.Stroke = Pavarot.Colore
        LineaTmp.PointMarker.Fill = Pavarot.Colore
      End If

      LineaTmp.PointMarker.Height = 10
      LineaTmp.PointMarker.Width = 10
      LineaTmp.PointMarker.StrokeThickness = 1
      LineaTmp.Tag = Pavarot

      DataSeriesTMP.AcceptsUnsortedData = True

      Dim X As Double = Pavarot.TwsDetails.AvgVal
      MaxTws = System.Math.Max(X, MaxTws)
      MinTws = System.Math.Min(X, MinTws)
      Dim Y As Double = Pavarot.PavarotDetails.VmgTgtLossMt '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
      DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
      TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
      LineaTmp.DataSeries = DataSeriesTMP
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      GainLossVsTwsSeriesSource.Add(CSVMtmp)
    Next
    'If _PlotTargetsIfAvailable Then
    DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)
    'End If
    TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    ImpostaAnnotazioniGainLossVsTws()

  End Sub

  Public Sub DrawPlotAvgVmgVsTwsGroupByTack()
    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False

    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati
    Dim TwsPort As New List(Of Double)
    Dim TwsStbd As New List(Of Double)
    Dim TL As New clsTrendLines

    '    For Each Pavarot In PeriodsManager.ListaPavarot
    '  Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
        AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

        Dim X As Double = Pavarot.TwsDetails.AvgVal
        MaxTws = System.Math.Max(X, MaxTws)
        MinTws = System.Math.Min(X, MinTws)
        Dim Y As Double = Pavarot.PavarotDetails.VmgTgtLossMt '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
        'Dim Y As Double = Pavarot.VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
        'DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
        If Pavarot.IsStbd Then
          ValoriStbd.AggiungiCoppia(0, Y)
          TwsStbd.Add(X)
        Else
          ValoriPort.AggiungiCoppia(0, Y)
          TwsPort.Add(X)
        End If
        TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    'DataSeriesPort.AcceptsUnsortedData = True
    Dim LineaPort As New XyScatterRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.PointMarker = New EllipsePointMarker()
    LineaPort.PointMarker.Stroke = Colors.Red
    LineaPort.PointMarker.Height = 10
    LineaPort.PointMarker.Width = 10
    LineaPort.PointMarker.StrokeThickness = 1
    LineaPort.PointMarker.Fill = Colors.Red
    LineaPort.Tag = -1
    For Each Valore In ValoriPort.Dizionario
      DataSeriesPort.Append(TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    GainLossVsTwsSeriesSource.Add(CSVMport)


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    'DataSeriesStbd.AcceptsUnsortedData = True
    Dim LineaStbd As New XyScatterRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker()
    LineaStbd.PointMarker.Stroke = Colors.Green
    LineaStbd.PointMarker.Height = 10
    LineaStbd.PointMarker.Width = 10
    LineaStbd.PointMarker.StrokeThickness = 1
    LineaStbd.PointMarker.Fill = Colors.Green
    LineaStbd.Tag = 1
    For Each Valore In ValoriStbd.Dizionario
      DataSeriesStbd.Append(TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    GainLossVsTwsSeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")

    'If _PlotTargetsIfAvailable Then
    DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)
    'End If
    TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    ImpostaAnnotazioniGainLossVsTws()

  End Sub

  Private Sub DrawPlotAvgVmgVsTwsGroupByKeys()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False
    Dim TL As New clsTrendLines

    '    For Each Pavarot In PeriodsManager.ListaPavarot
    '  'cicla tra le pavarot checkate
    '  If PeriodsManager.ListaPavarot(i).IsChecked Then
    '    Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot

    '    AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
    '    AtLeastOneGybe = AtLeastOneGybe OrElse Not Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
    '    Dim X As Double = Pavarot.MedieStandardPavarot.TWSMedia
    '    MaxTws = System.Math.Max(X, MaxTws)
    '    MinTws = System.Math.Min(X, MinTws)
    '    Dim Y As Double = Pavarot.VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
        AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

        Dim X As Double = Pavarot.TwsDetails.AvgVal
        MaxTws = System.Math.Max(X, MaxTws)
        MinTws = System.Math.Min(X, MinTws)
        Dim Y As Double = Pavarot.PavarotDetails.VmgTgtLossMt '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)

        Dim Chiave As String = Pavarot.Keys
        If Not Chiave.Trim = "" Then
          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, contatore)
            contatore += 1
          End If
          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
            If Pavarot.IsStbd Then
              Chiave = "Stbd " & Chiave
              Mure = clsGruppoPeriodi.eMure.eStbd
            Else
              Chiave = "Port " & Chiave
              Mure = clsGruppoPeriodi.eMure.ePort
            End If
          End If

          If Gruppi.ContainsKey(Chiave) Then
            Gruppi(Chiave).AggiungiCoppia(X, Y)
          Else
            Gruppi.Add(Chiave, New clsGruppoPeriodi(X, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          End If
          TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
        End If
      End If

    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      DataSeries.AcceptsUnsortedData = True
      Dim Linea As New XyScatterRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      If Gruppo.Value.Mure = clsGruppoPeriodi.eMure.ePort Then
        Linea.PointMarker = New SquarePointMarker
      Else
        Linea.PointMarker = New EllipsePointMarker()
      End If
      'Linea.PointMarker.Stroke = ColoriDifferenziati(Gruppo.Value.IdGruppo) '  Colors.Red
      Linea.PointMarker.Stroke = Gruppo.Value.Colore '  Colors.Red
      Linea.PointMarker.Height = 10
      Linea.PointMarker.Width = 10
      Linea.PointMarker.StrokeThickness = 1
      'Linea.PointMarker.Fill = ColoriDifferenziati(Gruppo.Value.IdGruppo) ' Colors.Red
      Linea.PointMarker.Fill = Linea.PointMarker.Stroke
      Linea.Tag = Gruppo ' .Key '  Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      DataSeries.Append(Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Average, Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
      Linea.DataSeries = DataSeries
      Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
      GainLossVsTwsSeriesSource.Add(CSVMport)
    Next

    'OnPropertyChanged("SeriesSource")

    'If _PlotTargetsIfAvailable Then
    DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)
    'End If
    TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    AnnotazioniGainLossVsTws.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Vmg Kts"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)


    Dim StbdPort As Boolean = False
    Dim Ytmp As Double = 0.1
    For Each Gruppo In Gruppi
      AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
      AnWT.X1 = 0
      AnWT.Y1 = Ytmp
      AnWT.FontSize = 12
      If Gruppo.Key.ToString.IndexOf("Stbd") > -1 Then
        'If Pavarot.IsStbd Then
        '  LineaTmp.StrokeDashArray = {3, 3}
        'End If
        AnWT.Text = Gruppo.Key ' & " circle/dotted"
        StbdPort = True
      Else
        AnWT.Text = Gruppo.Key
      End If
      AnWT.BorderBrush = New SolidColorBrush(ColoriDifferenziati(Gruppo.Value.IdGruppo))
      AnWT.BorderThickness = New Thickness(3, 0, 0, 0)
      AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
      AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
      AnnotazioniGainLossVsTws.Add(AnWT)
      Ytmp += 0.07
    Next

    If StbdPort Then
      AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
      AnWT.X1 = 0
      AnWT.Y1 = 0.92
      AnWT.FontSize = 10
      AnWT.Text = "Stbd circles and dotted lines"
      AnWT.BorderBrush = New SolidColorBrush(Colors.Black)
      AnWT.BorderThickness = New Thickness(0, 0, 0, 0)
      AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
      AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
      AnnotazioniGainLossVsTws.Add(AnWT)
    End If


  End Sub

  Public Sub DrawPlotAvgVmgVsTwsGroupByTack2021()
    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False

    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati
    Dim TwsPort As New List(Of Double)
    Dim TwsStbd As New List(Of Double)
    Dim TL As New clsTrendLines

    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      '    For Each Pavarot In PeriodsManager.ListaPavarot
      '  Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      '  Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If True Then
        If Pavarot.IsChecked Then
          AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
          AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

          Dim X As Double = Pavarot.TwsDetails.AvgVal
          If Not Double.IsNaN(X) Then
            If Not Double.IsInfinity(X) Then
              MaxTws = System.Math.Max(X, MaxTws)
              MinTws = System.Math.Min(X, MinTws)
            End If
          End If
          Dim Y As Double = Pavarot.PavarotDetails.VmgAvg '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)

          'AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
          'AtLeastOneGybe = AtLeastOneGybe OrElse Not Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack

          'Dim X As Double = Period2021.TwsDetails.AvgVal
          'MaxTws = System.Math.Max(X, MaxTws)
          'MinTws = System.Math.Min(X, MinTws)
          'Dim Y As Double = Period2021.PavarotDetails.VmgAvg '  Pavarot.VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
          'DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
          If Pavarot.IsStbd Then
            ValoriStbd.AggiungiCoppia(0, Y)
            TwsStbd.Add(X)
          Else
            ValoriPort.AggiungiCoppia(0, Y)
            TwsPort.Add(X)
          End If
          TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
        End If
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    'DataSeriesPort.AcceptsUnsortedData = True
    Dim LineaPort As New XyScatterRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.PointMarker = New EllipsePointMarker()
    LineaPort.PointMarker.Stroke = Colors.Red
    LineaPort.PointMarker.Height = 10
    LineaPort.PointMarker.Width = 10
    LineaPort.PointMarker.StrokeThickness = 1
    LineaPort.PointMarker.Fill = Colors.Red
    LineaPort.Tag = -1
    For Each Valore In ValoriPort.Dizionario
      Dim tws As Double = TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average
      Dim val As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
      DataSeriesPort.Append(tws, val, New clsPuntoMetadata(False))
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    GainLossVsTwsSeriesSource.Add(CSVMport)


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    'DataSeriesStbd.AcceptsUnsortedData = True
    Dim LineaStbd As New XyScatterRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker()
    LineaStbd.PointMarker.Stroke = Colors.Green
    LineaStbd.PointMarker.Height = 10
    LineaStbd.PointMarker.Width = 10
    LineaStbd.PointMarker.StrokeThickness = 1
    LineaStbd.PointMarker.Fill = Colors.Green
    LineaStbd.Tag = 1
    For Each Valore In ValoriStbd.Dizionario
      Dim tws As Double = TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average
      Dim val As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
      DataSeriesStbd.Append(tws, val, New clsPuntoMetadata(False))
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    GainLossVsTwsSeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")

    'If _PlotTargetsIfAvailable Then
    DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)
    'End If
    TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    ImpostaAnnotazioniGainLossVsTws()

  End Sub

  Private Sub DrawPlotAvgVmgVsTwsGroupByKeys2021()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False
    Dim TL As New clsTrendLines

    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      'cicla tra le pavarot checkate
      'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If True Then
        If Pavarot.IsChecked Then
          AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
          AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

          Dim X As Double = Pavarot.TwsDetails.AvgVal
          If Not Double.IsNaN(X) Then
            If Not Double.IsInfinity(X) Then
              MaxTws = System.Math.Max(X, MaxTws)
              MinTws = System.Math.Min(X, MinTws)
            End If
          End If
          Dim Y As Double = Pavarot.PavarotDetails.VmgAvg '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)

          Dim Chiave As String = Pavarot.Keys
          If Not Chiave.Trim = "" Then
            If Not GrId.ContainsKey(Chiave) Then
              GrId.Add(Chiave, contatore)
              contatore += 1
            End If

            Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
            If OutputType = eOutputType.eGroupByTack OrElse OutputType = eOutputType.eColorByTack Then
              If Pavarot.IsStbd Then
                Chiave = "Stbd " & Chiave
                Mure = clsGruppoPeriodi.eMure.eStbd
              Else
                Chiave = "Port " & Chiave
                Mure = clsGruppoPeriodi.eMure.ePort
              End If
            End If

            If Gruppi.ContainsKey(Chiave) Then
              Gruppi(Chiave).AggiungiCoppia(X, Y)
            Else
              Gruppi.Add(Chiave, New clsGruppoPeriodi(X, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
            End If
            TL.AccodaCoppia(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.IsStbd, X, Y)
          End If
        End If
      End If
    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      DataSeries.AcceptsUnsortedData = True
      Dim Linea As New XyScatterRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      If Gruppo.Value.Mure = clsGruppoPeriodi.eMure.ePort Then
        Linea.PointMarker = New SquarePointMarker
      Else
        Linea.PointMarker = New EllipsePointMarker()
      End If
      'Linea.PointMarker.Stroke = ColoriDifferenziati(Gruppo.Value.IdGruppo) '  Colors.Red
      Linea.PointMarker.Stroke = Gruppo.Value.Colore
      Linea.PointMarker.Height = 10
      Linea.PointMarker.Width = 10
      Linea.PointMarker.StrokeThickness = 1
      Linea.PointMarker.Fill = Gruppo.Value.Colore
      Linea.Tag = Gruppo ' .Key '  Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      DataSeries.Append(Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Average, Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
      Linea.DataSeries = DataSeries
      Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
      GainLossVsTwsSeriesSource.Add(CSVMport)
    Next

    'OnPropertyChanged("SeriesSource")

    'If _PlotTargetsIfAvailable Then
    DisegnaTargets(MinTws, MaxTws, AtLeastOneTack, AtLeastOneGybe)
    'End If
    TL.StampaTrendLines(4, GainLossVsTwsSeriesSource, Me)

    AnnotazioniGainLossVsTws.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Vmg Kts"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)


    Dim StbdPort As Boolean = False
    Dim Ytmp As Double = 0.1
    For Each Gruppo In Gruppi
      AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
      AnWT.X1 = 0
      AnWT.Y1 = Ytmp
      AnWT.FontSize = 12
      If Gruppo.Key.ToString.IndexOf("Stbd") > -1 Then
        'If Pavarot.IsStbd Then
        '  LineaTmp.StrokeDashArray = {3, 3}
        'End If
        AnWT.Text = Gruppo.Key ' & " circle/dotted"
        StbdPort = True
      Else
        AnWT.Text = Gruppo.Key
      End If
      AnWT.BorderBrush = New SolidColorBrush(ColoriDifferenziati(Gruppo.Value.IdGruppo))
      AnWT.BorderThickness = New Thickness(3, 0, 0, 0)
      AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
      AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
      AnnotazioniGainLossVsTws.Add(AnWT)
      Ytmp += 0.07
    Next

    If StbdPort Then
      AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
      AnWT.X1 = 0
      AnWT.Y1 = 0.92
      AnWT.FontSize = 10
      AnWT.Text = "Stbd circles and dotted lines"
      AnWT.BorderBrush = New SolidColorBrush(Colors.Black)
      AnWT.BorderThickness = New Thickness(0, 0, 0, 0)
      AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
      AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
      AnnotazioniGainLossVsTws.Add(AnWT)
    End If


  End Sub


  Private Sub DisegnaTargets(MinTws As Double, MaxTws As Double, AtLeastOnetack As Boolean, AtLeastOneGybe As Boolean)
    If Not TgtManager.Tgt Is Nothing AndAlso TgtManager.Tgt.PolareDisponibile("bs") Then
      ' traccia le linee target
      MaxTws = System.Math.Floor(MaxTws) + 1
      MinTws = System.Math.Floor(MinTws)

      Dim a(1) As Double
      a(0) = 1
      a(1) = 3

      Dim DataSeriesUp As New XyDataSeries(Of Double, Double)
      DataSeriesUp.AcceptsUnsortedData = False
      Dim LineaUp As New FastLineRenderableSeries
      LineaUp.XAxisId = "DefaultAxisId"
      LineaUp.YAxisId = "DefaultAxisId"
      Dim c = Windows.Media.Colors.DarkRed
      c.A = 50
      LineaUp.Stroke = c 'Windows.Media.Color.FromArgb(100, Windows.Media.Colors.DarkRed)
      LineaUp.StrokeThickness = 8
      LineaUp.StrokeDashArray = a
      LineaUp.Tag = "TgtUp"

      Dim DataSeriesDn As New XyDataSeries(Of Double, Double)
      DataSeriesDn.AcceptsUnsortedData = False
      Dim LineaDn As New FastLineRenderableSeries
      LineaDn.XAxisId = "DefaultAxisId"
      LineaDn.YAxisId = "DefaultAxisId"
      c = Windows.Media.Colors.DarkBlue
      c.A = 50
      LineaUp.Stroke = c 'Windows.Media.Color.FromArgb(100, Windows.Media.Colors.DarkRed)
      'LineaDn.Stroke = Colors.DarkBlue
      LineaDn.StrokeThickness = 8
      LineaDn.StrokeDashArray = a
      LineaDn.Tag = "TgtDn"


      For i As Integer = MinTws To MaxTws Step 1
        Dim vup = TgtManager.Tgt.ValoreTgtUp(i, "bs")
        DataSeriesUp.Append(i, vup.Vmg)
        Dim vdn = TgtManager.Tgt.ValoreTgtDn(i, "bs")
        DataSeriesDn.Append(i, vdn.Vmg)
      Next

      GainLossVsTwsSeriesSource.Add(New ChartSeriesViewModel(DataSeriesUp, LineaUp))
      GainLossVsTwsSeriesSource.Add(New ChartSeriesViewModel(DataSeriesDn, LineaDn))

      LineaUp.IsVisible = PlotTargetsIfAvailable AndAlso AtLeastOnetack
      LineaDn.IsVisible = PlotTargetsIfAvailable AndAlso AtLeastOneGybe


    End If

    DisegnaBenchmarks(MinTws, MaxTws, AtLeastOnetack, AtLeastOneGybe)

  End Sub

  Private Sub DisegnaBenchmarks(MinTws As Double, MaxTws As Double, AtLeastOneTack As Boolean, AtLeastOneGybe As Boolean)

    If BenchManager Is Nothing Then Exit Sub

    ' traccia le linee target
    MaxTws = System.Math.Floor(MaxTws) + 1
    MinTws = System.Math.Floor(MinTws)

    Dim a(1) As Double
    a(0) = 1
    a(1) = 3

    Dim DataSeriesUp As New XyDataSeries(Of Double, Double)
    DataSeriesUp.AcceptsUnsortedData = True
    Dim LineaUp As New FastLineRenderableSeries
    LineaUp.XAxisId = "DefaultAxisId"
    LineaUp.YAxisId = "DefaultAxisId"
    LineaUp.Stroke = Colors.DarkRed
    LineaUp.StrokeThickness = 3
    LineaUp.StrokeDashArray = a
    LineaUp.Tag = "BenchUp"

    Dim DataSeriesDn As New XyDataSeries(Of Double, Double)
    DataSeriesDn.AcceptsUnsortedData = True
    Dim LineaDn As New FastLineRenderableSeries
    LineaDn.XAxisId = "DefaultAxisId"
    LineaDn.YAxisId = "DefaultAxisId"
    LineaDn.Stroke = Colors.DarkBlue
    LineaDn.StrokeThickness = 3
    LineaDn.StrokeDashArray = a
    LineaDn.Tag = "BenchDn"

    Dim durata As Double = 40

    Dim b = BenchManager.ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eTackLoss)
    If Not b Is Nothing Then
      Dim elementi = b.First.Values.OrderBy(Function(x) x.X).ToList
      For e As Integer = 0 To elementi.Count - 1
        Dim nx As Integer = Math.Min(e + 1, elementi.Count - 1)
        Dim pv As Integer = Math.Max(e - 1, 0)
        If elementi(nx).X >= MinTws And elementi(pv).X <= MaxTws Then
          DataSeriesUp.Append(elementi(e).X, InlineLossMetersToVmgSpeed(elementi(e).X, elementi(e).Y, True, durata))
        End If
      Next
    End If

    b = BenchManager.ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eGybeLoss)
    If Not b Is Nothing Then
      Dim elementi = b.First.Values.OrderBy(Function(x) x.X).ToList
      For e As Integer = 0 To elementi.Count - 1
        Dim nx As Integer = Math.Min(e + 1, elementi.Count - 1)
        Dim pv As Integer = Math.Max(e - 1, 0)
        If elementi(nx).X >= MinTws And elementi(pv).X <= MaxTws Then
          DataSeriesDn.Append(elementi(e).X, InlineLossMetersToVmgSpeed(elementi(e).X, elementi(e).Y, False, durata))
        End If
      Next
    End If

    'elementi = BenchManager.Benchmarks.GybesLoss.Values.OrderBy(Function(x) x.X).ToList
    'For e As Integer = 0 To elementi.Count - 1
    '  Dim nx As Integer = Math.Min(e + 1, elementi.Count - 1)
    '  Dim pv As Integer = Math.Max(e - 1, 0)
    '  If elementi(nx).X >= MinTws And elementi(pv).X <= MaxTws Then
    '    DataSeriesDn.Append(elementi(e).X, InlineLossMetersToVmgSpeed(elementi(e).X, elementi(e).Y, False, durata))
    '  End If
    'Next

    'For Each elemento In BenchManager.Benchmarks.TacksLoss.Values.OrderBy(Function(x) x.X).ToList
    '  If elemento.X >= MinTws And elemento.X <= MaxTws Then
    '    DataSeriesUp.Append(elemento.X, InlineLossMetersToVmgSpeed(elemento.X, elemento.Y, True, durata))
    '  End If
    'Next
    'For Each elemento In BenchManager.Benchmarks.GybesLoss.Values.OrderBy(Function(x) x.X).ToList
    '  If elemento.X >= MinTws And elemento.X <= MaxTws Then
    '    DataSeriesDn.Append(elemento.X, InlineLossMetersToVmgSpeed(elemento.X, elemento.Y, False, durata))
    '  End If
    'Next

    'For Each Bench In ManBenchManager.Lista
    '  For Each elemento In Bench.ListaValoriLoss.OrderBy(Function(x) x.Tws).ToList
    '    If elemento.Tws >= MinTws And elemento.Tws <= MaxTws Then
    '      Select Case Bench.ManoeuversType
    '        Case clsPeriod2021.ePeriodType.eGybe
    '          DataSeriesDn.Append(elemento.Tws, elemento.VmgAvg)
    '        Case clsPeriod2021.ePeriodType.eTack
    '          DataSeriesUp.Append(elemento.Tws, elemento.VmgAvg)
    '      End Select
    '    End If
    '  Next
    'Next
    'For i As Integer = MinTws To MaxTws Step 1
    '    Dim vup = TgtManager.Tgt.ValoreTgtUp(i, "bs")
    '    DataSeriesUp.Append(i, vup.Vmg)
    '    Dim vdn = TgtManager.Tgt.ValoreTgtDn(i, "bs")
    '    DataSeriesDn.Append(i, vdn.Vmg)
    '  Next

    GainLossVsTwsSeriesSource.Add(New ChartSeriesViewModel(DataSeriesUp, LineaUp))
    GainLossVsTwsSeriesSource.Add(New ChartSeriesViewModel(DataSeriesDn, LineaDn))

    LineaUp.IsVisible = PlotTargetsIfAvailable AndAlso AtLeastOneTack
    LineaDn.IsVisible = PlotTargetsIfAvailable AndAlso AtLeastOneGybe



  End Sub

  Private Function InlineLossMetersToVmgSpeed(Tws As Double, InlineLossMt As Double, IsUp As Boolean, DurataSec As Double) As Double
    If Not TgtManager.Tgt Is Nothing AndAlso TgtManager.Tgt.PolareDisponibile("bs") Then
      Dim t = TgtManager.Tgt.ValoreTgt(IsUp, Tws, "bs")
      Dim mbs = KtsToMS(t.Vmg) * DurataSec / Math.Abs(Math.Cos(Radians(t.Twa)))
      Dim mbch = mbs - InlineLossMt
      Return MsToKts(mbch / DurataSec) * Math.Abs(Math.Cos(Radians(t.Twa)))
    End If
    Return Double.NaN
  End Function

  Private Sub ImpostaAnnotazioniGainLossVsTws()
    AnnotazioniGainLossVsTws.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.85
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Inline loss [m]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    AnnotazioniGainLossVsTws.Add(AnWT)

  End Sub

  Public Sub AggiungiGrafico()
    'ImpostaCanaliAsIsSelected(Nothing)
    Dim res As List(Of clsChannel2020) = GestisciListaCanali("Select Channel", DataProvider2020.Channels.ListaCanali.ToList, New List(Of clsChannel2020), eSelectChannelType.eSingle)
    'Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, False)
    'If WPFpup.ShowDialog() Then
    If Not res Is Nothing Then
      ControlliPavarotBasicPlot.AggiungiCanale(res.First)
      Dim Controllo As UserControlPavarotPlot = ControlliPavarotBasicPlot.ListaControlli.Last
      Dim VM As UserControlPavarotPlotViewModel = DirectCast(Controllo.DataContext, UserControlPavarotPlotViewModel)
      VM.DrawChart() '(pValoriRelativi)
      Controllo.Plot.ZoomExtents()
    End If
    'ImpostaCanaliAsIsSelected(Nothing)
  End Sub

  Public Sub OperazioniSuListaControlli(ControlloCorrente As UserControlPavarotPlot, Operazione As UserControlPavarotPlot.eTipoOperazione)

    Dim IndiceCorrente As Integer = IndiceControlloNellaLista(ControlliPavarotBasicPlot.ListaControlli, ControlloCorrente)
    If IndiceCorrente < ListaControlliCustom.Count Then Exit Sub ' non si possono fare operazioni sul grafico loss progression
    Select Case Operazione
      Case UserControlPavarotPlot.eTipoOperazione.eDelete
        ControlliPavarotBasicPlot.ListaControlli.RemoveAt(IndiceCorrente)
      Case UserControlPavarotPlot.eTipoOperazione.eMoveDn
        If IndiceCorrente < ControlliPavarotBasicPlot.ListaControlli.Count - 1 Then
          ControlliPavarotBasicPlot.ListaControlli.RemoveAt(IndiceCorrente)
          ControlliPavarotBasicPlot.ListaControlli.Insert(IndiceCorrente + 1, ControlloCorrente)
        End If
      Case UserControlPavarotPlot.eTipoOperazione.eMoveTop
        ControlliPavarotBasicPlot.ListaControlli.RemoveAt(IndiceCorrente)
        ControlliPavarotBasicPlot.ListaControlli.Insert(ListaControlliCustom.Count, ControlloCorrente)
      Case UserControlPavarotPlot.eTipoOperazione.eMoveBottom
        ControlliPavarotBasicPlot.ListaControlli.RemoveAt(IndiceCorrente)
        ControlliPavarotBasicPlot.ListaControlli.Insert(ControlliPavarotBasicPlot.ListaControlli.Count - 1, ControlloCorrente)
      Case UserControlPavarotPlot.eTipoOperazione.eMoveUp
        If IndiceCorrente > ListaControlliCustom.Count Then
          ControlliPavarotBasicPlot.ListaControlli.RemoveAt(IndiceCorrente)
          ControlliPavarotBasicPlot.ListaControlli.Insert(IndiceCorrente - 1, ControlloCorrente)
        End If
      Case UserControlPavarotPlot.eTipoOperazione.eSelectChannel
        Dim VM As UserControlPavarotPlotViewModel = DirectCast(ControlliPavarotBasicPlot.ListaControlli(IndiceCorrente).DataContext, UserControlPavarotPlotViewModel)
        Dim Lista As New List(Of clsChannel2020)
        Lista.Add(VM.Canale)
        'ImpostaCanaliAsIsSelected(Lista)
        Dim res As List(Of clsChannel2020) = GestisciListaCanali("Select Channel", DataProvider2020.Channels.ListaCanali.ToList, Lista, eSelectChannelType.eSingle)

        'Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, False)
        'If WPFpup.ShowDialog() Then
        '  VM.Canale = WPFpup.Canale
        If Not res Is Nothing Then
          VM.Canale = res.First
          VM.DrawChart() ' (pValoriRelativi)
          ControlliPavarotBasicPlot.ListaControlli(IndiceCorrente).Plot.ZoomExtents()
          AggiornaPeriodiVisibili()
        End If
        'ImpostaCanaliAsIsSelected(Nothing)
    End Select
    ControlliPavarotBasicPlot.SalvaImpostazioneJson()
  End Sub

  Public Function IndiceControlloNellaLista(Lista As ObservableCollection(Of UserControlPavarotPlot), ControlloCorrente As UserControlPavarotPlot) As Integer
    For i As Integer = 0 To Lista.Count - 1
      If Lista(i) Is ControlloCorrente Then Return i
    Next
    Return -1
  End Function

  Private Sub PavarotSelectionSettings_SettingChanged() Handles PavarotSelectionSettings.SettingChanged
    PavarotSelectionSettings.VerificaIsCheckedPavarots()
    VerificaAutoRefresh()
  End Sub

  'Private Sub _LastUsedPavarotSettings2020_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _LastUsedPavarotSettings2020.PropertyChanged
  '  VerificaRefresh()
  'End Sub

  'Private Sub _PavarotSelectionSettings_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _PavarotSelectionSettings.PropertyChanged
  '  VerificaAggiornaPavarot()
  'End Sub
End Class

Public Class UserControlPavarotPlotViewModelStandardChannel ' canali nella lista customizzabile tra quelli da visualizzare
  Inherits UserControlPavarotPlotViewModel

  Public Sub New(ParentViewModel As UserControlPavarotViewModel, Canale As clsChannel2020)
    MyBase.New(ParentViewModel, Canale)
    Me.Canale = Canale
    Me.CanaleCustom = Nothing
  End Sub


  Private Function MediaAnte(TR As clsTimeRange) As Double
    If Canale.Valori Is Nothing OrElse Canale.Valori.Count = 0 Then Return 0
    Dim sublist As Double() = Canale.Valori.ToList.GetRange(TR.IdRigaIniziale, (TR.IdRigaFinale - TR.IdRigaIniziale)).ToArray
    Select Case Canale.DataType
      Case clsChannel2020.eDataType.e360
        Return Media360(sublist)
      Case clsChannel2020.eDataType.e180
        Return System.Math.Abs(Media180(sublist))
      Case clsChannel2020.eDataType.eTack
        Dim sublistTack As Double() = CanaleTackDefault.Valori.ToList.GetRange(TR.IdRigaIniziale, (TR.IdRigaFinale - TR.IdRigaIniziale)).ToArray
        Return MediaTack(sublist, sublistTack)
      Case clsChannel2020.eDataType.eTackReversed
        Dim sublistTack As Double() = CanaleTackDefault.Valori.ToList.GetRange(TR.IdRigaIniziale, (TR.IdRigaFinale - TR.IdRigaIniziale)).ToArray
        Return MediaTackReversed(sublist, sublistTack)
      Case clsChannel2020.eDataType.eBoolean, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
        Return 0
      Case Else
        Return Media(sublist)
    End Select

    'End If
  End Function

  'Private Function WindwardChannelForEntry(NomeCanale As String) As Boolean
  '  Dim Lista As String() = {"Runner", "V1", "D1"}
  '  For Each Str As String In Lista
  '    If NomeCanale.IndexOf(Str) > -1 Then
  '      Return True
  '    End If
  '  Next
  '  Return False
  '  'Return Lista.Where(Function(x) x.IndexOf(NomeCanale) > -1).Count > 0
  'End Function

  'Private Function Valori(TR As clsTimeRange, IsStbdEntry As Boolean) As List(Of Double)

  '  'vengon intercettati i canali stbd port in modo da avere una plottatat non legata alle mura assolute ma a quelle entry ed exit
  '  'per convenzione se chiedo canale port mette exit altrimenti entry per stbd
  '  Select Case Canale.DataType
  '    Case clsChannel2020.eDataType.eBoolean, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
  '      Return Nothing
  '    Case Else
  '      If Canale.Valori Is Nothing OrElse Canale.Valori.Count = 0 Then
  '        Return Nothing
  '      End If
  '      'Dim CanaleCorrente As clsChannel2020 = DataProvider2020.CanaleDbl(Canale.CanaleChiave) 'va messo cosí in modo da caricare i dati del canale se non é sato ancora fatto
  '      'If CanaleCorrente.ChannelId.IndexOf("run") > -1 Then Stop
  '      If Canale.ChannelId.Contains("Stbd") Then
  '        Dim Omologo As clsChannel2020 = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.ChannelId = Canale.ChannelId.Replace("Stbd", "Port")).FirstOrDefault
  '        If Not Omologo Is Nothing Then
  '          If WindwardChannelForEntry(CanaleCorrente.ChannelId) Then
  '            Titolo = Canale.LongName.Replace("Stbd", "Wwd@Entry")
  '            If Not IsStbdEntry Then
  '              CanaleCorrente = Omologo
  '            End If
  '          Else
  '            Titolo = Canale.LongName.Replace("Stbd", "Lwd@Entry")
  '            If IsStbdEntry Then
  '              Canale = Omologo
  '            End If
  '          End If
  '        End If
  '      ElseIf CanaleCorrente.ChannelId.Contains("Port") Then
  '        Dim Omologo As clsChannel2020 = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.ChannelId = Canale.ChannelId.Replace("Port", "Stbd")).FirstOrDefault
  '        If Not Omologo Is Nothing Then
  '          If WindwardChannelForEntry(Canale.ChannelId) Then
  '            Titolo = Canale.LongName.Replace("Port", "Wwd@Exit")
  '            If Not IsStbdEntry Then
  '              Canale = Omologo
  '            End If
  '          Else
  '            Titolo = Canale.LongName.Replace("Port", "Lwd@Exit")
  '            If IsStbdEntry Then
  '              Canale = Omologo
  '            End If
  '          End If
  '        End If
  '      End If
  '      Dim sublist As List(Of Double) = Canale.Valori.ToList.GetRange(TR.IdRigaIniziale, (TR.IdRigaFinale - TR.IdRigaIniziale) + 1)
  '      Dim risultato As New List(Of Double)
  '      For Each elemento In sublist
  '        If Canale.DataType = clsChannel2020.eDataType.e180 Then
  '          risultato.Add(System.Math.Abs(elemento))
  '        Else
  '          risultato.Add(elemento)
  '        End If
  '      Next
  '      Return risultato
  '  End Select

  'End Function

  Private Function Valori(TR As clsTimeRange, IsStbdEntry As Boolean) As Double()
    If TR.IdRigaIniziale = TR.IdRigaFinale Then Return Nothing
    Dim CanaleCorrente As clsChannel2020 = Canale
    If Not CanaleOmologo Is Nothing Then
      If IsStbdEntry And CanaleCorrente.ChannelId.Contains("Port") Then
        CanaleCorrente = CanaleOmologo
      ElseIf Not IsStbdEntry And CanaleCorrente.ChannelId.Contains("Stbd") Then
        CanaleCorrente = CanaleOmologo
      End If
    End If
    If CanaleCorrente.Valori Is Nothing OrElse CanaleCorrente.Valori.Count = 0 Then
      Return Nothing
    End If
    Select Case CanaleCorrente.DataType
      Case clsChannel2020.eDataType.eBoolean, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
        Return Nothing
      Case Else
        If CanaleCorrente.DataType = clsChannel2020.eDataType.e180 Then
          Dim sa(TR.IdRigaFinale - TR.IdRigaIniziale + 1) As Double
          Dim IdIniziale As Integer = TR.IdRigaIniziale
          For i As Integer = 0 To sa.Length - 1
            sa(i) = CanaleCorrente.Valori(i + IdIniziale)
          Next
          Return sa
        Else
          Return CanaleCorrente.Valori.Skip(TR.IdRigaIniziale).Take(TR.IdRigaFinale - TR.IdRigaIniziale + 1).ToArray
        End If
    End Select

  End Function

  Public Overrides Sub DrawChart()
    If Canale Is Nothing Then Exit Sub
    Select Case ParentVM.OutputType
      Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
        DrawChartGroupByKeys()
        Exit Sub
      Case UserControlPavarotViewModel.eOutputType.eGroupByTack
        DrawChartGroupByTack()
        Exit Sub
    End Select
    Dim Inizio As DateTime = Now

    Dim TogglaSegno As Boolean = ToggleSigned
    Select Case Canale.CanaleChiave
      Case clsChannels2020.eCanaliChiave.eYRT
        TogglaSegno = True
      Case clsChannels2020.eCanaliChiave.eNone
        Select Case Canale.ActualLogHeader
          Case "maintwist", "maintrav"
            TogglaSegno = True
        End Select
      Case Else

    End Select

    Titolo = Canale.LongName


    SeriesSource.Clear()
    For Each Pavarot In PeriodsManager.CollectionPavarot ' questa e' la lista delle manovre gia' selezionate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      Dim Colore As System.Windows.Media.Color = Pavarot.Colore
      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          Colore = Colors.Green
        Else
          Colore = Colors.Red
        End If
      End If
      Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, -1, 1)
      Dim MatriceValori As Double() = Valori(Pavarot.TimeRangeStandardPavarot, Pavarot.IsChecked)
      If MatriceValori Is Nothing Then Exit Sub
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.Stroke = Colore
      'If Pavarot.IsStbd Then
      '  LineaTmp.StrokeDashArray = {3, 3}
      'End If
      LineaTmp.StrokeThickness = 2
      LineaTmp.Tag = Pavarot

      'LineaTmp.Visibility = IIf(Pavarot.IsChecked, Visibility.Visible, Visibility.Collapsed)

      'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      Dim righe As Integer = RigaFinale - RigaIniziale
      Dim ss = righe / DataProvider2020.Hz
      If ss > Pavarot.TimeRangeStandardPavarot.Durata.TotalSeconds + 1 Then
        Dim momento As DateTime = Pavarot.TimeRangeStandardPavarot.Finish
        Do While RigaFinale > RigaIniziale
          RigaFinale -= 1
          Dim momentoP As DateTime = DataProvider2020.Momento(RigaFinale)
          If momento > momentoP Then
            Exit Do
          End If
        Loop
      End If
      'If Pavarot.TimeRangeStandardPavarot.Durata.TotalSeconds > 60 Then Stop

      RigaFinale = System.Math.Min(RigaFinale, RigaIniziale + MatriceValori.Count - 1)
      Dim ValoreRigaPrev As Double = MatriceValori(0)
      Dim MomentoPrev As DateTime = Pavarot.TimeRangeStandardPavarot.Start
      Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
      Dim MaxMom As Double
      For Riga As Integer = RigaIniziale To RigaFinale
        Dim Momento As DateTime = DataProvider2020.Momento(Riga)

        If Momento.ToOADate > 0 Then
          If Momento.ToOADate > MaxMom Then
            MaxMom = System.Math.Max(MaxMom, Momento.ToOADate)
            Dim SecsFromKeyMoment As Double = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds
            Dim ValoreRiga As Double = MatriceValori(Riga - Pavarot.TimeRangeStandardPavarot.IdRigaIniziale)
            If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
              ValoreRiga = System.Math.Abs(ValoreRiga)
            End If
            If TogglaSegno Then ValoreRiga *= ToggleSign
            If ValoriDerivati Then
              Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
              MediaMobile.AggiornaMedia(Derivata)
              DataSeriesTMP.Append(SecsFromKeyMoment, MediaMobile.Valore, New clsPuntoMetadata(False))
              ValoreRigaPrev = ValoreRiga
              MomentoPrev = Momento
            Else
              DataSeriesTMP.Append(SecsFromKeyMoment, ValoreRiga, New clsPuntoMetadata(False))
            End If
          End If
        End If
      Next

      LineaTmp.DataSeries = DataSeriesTMP
      'LineaTmp.Visibility = VisibilityStatus(Pavarot.IsChecked)
      LineaTmp.IsVisible = Pavarot.IsChecked
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
      'PCSVM.Add(New clsPavarotChartSeriesViewModel(Pavarot, CSVMtmp))
      'Console.WriteLine(Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    Next
    If ValoriDerivati Then Titolo &= " Derivative"

    'Console.WriteLine("Riempi DrawChart " & Now.Subtract(Inizio).TotalSeconds.ToString("F3"))
    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine()

    'OnPropertyChanged("SeriesSource")
  End Sub

  Private Function VisibilityStatus(Selezionata As Boolean) As Visibility
    If Selezionata Then Return Visibility.Visible
    Return Visibility.Hidden
  End Function
  'standard channels plots
  Public Sub DrawChartGroupByTack()
    Dim Inizio As DateTime = Now
    If Canale Is Nothing Then Exit Sub
    Dim TogglaSegno As Boolean = ToggleSigned
    Select Case Canale.CanaleChiave
      Case clsChannels2020.eCanaliChiave.eRdrAngle
        TogglaSegno = True
      Case clsChannels2020.eCanaliChiave.eYRT
        TogglaSegno = True
      Case clsChannels2020.eCanaliChiave.eNone
        Select Case Canale.ActualLogHeader
          Case "maintwist", "maintrav"
            TogglaSegno = True
        End Select
      Case Else

    End Select

    'Console.WriteLine("Inizio DrawChartGroupByTack")

    Titolo = Canale.LongName


    'For Each Pavarot In PeriodsManager.Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe OrElse x.PeriodType = clsPeriod2021.ePeriodType.eTack).ToList ' questa e' la lista delle manovre gia' selezionate
    '  'cicla tra le pavarot checkate
    '  'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
    '  If True Then
    '    If Pavarot.IsChecked Then
    '      AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
    '      AtLeastOneGybe = AtLeastOneGybe OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe

    '      Dim X As Double = Pavarot.TwsDetails.AvgVal
    '      MaxTws = System.Math.Max(X, MaxTws)
    '      MinTws = System.Math.Min(X, MinTws)
    '      Dim Y As Double = Pavarot.PavarotDetails.VmgAvg '  VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)


    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati
    Dim Moltiplicatore As Integer = DataProvider2020.Hz
    SeriesSource.Clear()
    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        'Dim adesso As DateTime = Now

        'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
        Dim Colore As System.Windows.Media.Color = Pavarot.Colore ' PavarotSyncViewModel.Colori(i)
        'Dim ValoreAnte As Double = MediaAnte(Pavarot.TimeRangeEntry) '.VMGms(PavarotSyncViewModel.AsseRiferimento, True)
        'If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
        '  ValoreAnte = System.Math.Abs(ValoreAnte)
        'End If
        Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, -1, 1)
        ToggleSign *= IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1)

        'If TogglaSegno Then ValoreAnte *= ToggleSign
        Dim MatriceValori As Double() = Valori(Pavarot.TimeRangeStandardPavarot, Pavarot.IsStbd)
        If MatriceValori Is Nothing Then Exit Sub

        'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
        'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
        RigaFinale = System.Math.Min(RigaFinale, RigaIniziale + MatriceValori.Count - 1)
        RigaFinale -= 1
        Dim ValoreRigaPrev As Double = MatriceValori(0)
        Dim MomentoPrev As DateTime = Pavarot.TimeRangeStandardPavarot.Start
        Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
        For Riga As Integer = RigaIniziale To RigaFinale
          Dim Momento As DateTime = DataProvider2020.Momento(Riga)
          If Momento.ToOADate > 0 Then
            Dim SecsFromKeyMoment As Double = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds
            Dim ValoreRiga As Double = MatriceValori(Riga - Pavarot.TimeRangeStandardPavarot.IdRigaIniziale)
            If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
              ValoreRiga = System.Math.Abs(ValoreRiga)
            End If
            If TogglaSegno Then ValoreRiga *= ToggleSign
            If ValoriDerivati Then
              Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
              MediaMobile.AggiornaMedia(Derivata)
              If Pavarot.IsStbd Then
                ValoriStbd.AggiungiCoppia(CInt(SecsFromKeyMoment * Moltiplicatore), MediaMobile.Valore)
              Else
                ValoriPort.AggiungiCoppia(CInt(SecsFromKeyMoment * Moltiplicatore), MediaMobile.Valore)
              End If
              ValoreRigaPrev = ValoreRiga
              MomentoPrev = Momento
            Else
              If Pavarot.IsStbd Then
                ValoriStbd.AggiungiCoppia(CInt(SecsFromKeyMoment * Moltiplicatore), ValoreRiga)
              Else
                ValoriPort.AggiungiCoppia(CInt(SecsFromKeyMoment * Moltiplicatore), ValoreRiga)
              End If
            End If
          End If

        Next
        'Console.WriteLine(Now.Subtract(adesso).TotalSeconds.ToString("F3"))
      End If
    Next
    If ValoriDerivati Then Titolo &= " Derivative"

    'Console.WriteLine("Riempi DrawChartGroupByTack " & Now.Subtract(Inizio).TotalSeconds.ToString("F3"))

    If ValoriPort.Dizionario.Count > 0 Then
      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
      Dim LineaPort As New FastLineRenderableSeries
      LineaPort.XAxisId = "DefaultAxisId"
      LineaPort.YAxisId = "DefaultAxisId"
      LineaPort.Stroke = Colors.Red
      LineaPort.StrokeThickness = 2
      LineaPort.Tag = -1
      'DataSeriesPort.AcceptsUnsortedData = True
      For Each Valore In ValoriPort.Dizionario.OrderBy(Function(x) x.Key)
        If Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Count > 0 Then
          DataSeriesPort.Append(Valore.Key / Moltiplicatore, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
        Else
          DataSeriesPort.Append(Valore.Key / Moltiplicatore, Double.NaN, New clsPuntoMetadata(False))
        End If
      Next
      LineaPort.DataSeries = DataSeriesPort
      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
      SeriesSource.Add(CSVMport)
    End If

    If ValoriStbd.Dizionario.Count > 0 Then
      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
      Dim LineaStbd As New FastLineRenderableSeries
      LineaStbd.XAxisId = "DefaultAxisId"
      LineaStbd.YAxisId = "DefaultAxisId"
      LineaStbd.Stroke = Colors.Green
      LineaStbd.StrokeThickness = 2
      LineaStbd.Tag = 1
      'DataSeriesStbd.AcceptsUnsortedData = True
      For Each Valore In ValoriStbd.Dizionario.OrderBy(Function(x) x.Key)
        If Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Count > 0 Then
          DataSeriesStbd.Append(Valore.Key / Moltiplicatore, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
        End If
      Next
      LineaStbd.DataSeries = DataSeriesStbd
      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
      SeriesSource.Add(CSVMStbd)
    End If
    'Console.WriteLine("Fine DrawChartGroupByTack " & Now.Subtract(Inizio).TotalSeconds.ToString("F3"))
    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine()

  End Sub

  Public Sub DrawChartGroupByKeys()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    If Canale Is Nothing Then Exit Sub
    Dim TogglaSegno As Boolean = ToggleSigned
    Select Case Canale.CanaleChiave
      Case clsChannels2020.eCanaliChiave.eRdrAngle
        TogglaSegno = True
      Case clsChannels2020.eCanaliChiave.eYRT
        TogglaSegno = True
      Case clsChannels2020.eCanaliChiave.eNone
        Select Case Canale.ActualLogHeader
          Case "maintwist", "maintrav"
            TogglaSegno = True
        End Select
      Case Else

    End Select

    Dim Moltiplicatore As Integer = 2

    Titolo = Canale.LongName
    SeriesSource.Clear()

    For Each Pavarot In PeriodsManager.CollectionPavarot
      'cicla tra le pavarot checkate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        Dim Chiave As String = Pavarot.Keys
        If Not Chiave.Trim = "" Then

          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, contatore)
            contatore += 1
          End If

          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eGroupByTack OrElse ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
            If Pavarot.IsStbd Then
              Chiave = "Stbd " & Chiave
              Mure = clsGruppoPeriodi.eMure.eStbd
            Else
              Chiave = "Port " & Chiave
              Mure = clsGruppoPeriodi.eMure.ePort
            End If
          End If


          Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, -1, 1)
          ToggleSign *= IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1)

          'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
          'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
          Dim MatriceValori As Double() = Valori(Pavarot.TimeRangeStandardPavarot, Pavarot.IsStbd)
          If MatriceValori Is Nothing Then Exit Sub
          RigaFinale = System.Math.Min(RigaFinale, RigaIniziale + MatriceValori.Count - 1)
          Dim ValoreRigaPrev As Double = MatriceValori(0)
          Dim MomentoPrev As DateTime = Pavarot.TimeRangeStandardPavarot.Start
          Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
          For Riga As Integer = RigaIniziale To RigaFinale
            Dim Momento As DateTime = DataProvider2020.Momento(Riga)
            If Momento.ToOADate > 0 Then
              Dim SecsFromKeyMoment As Double = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds
              Dim ValoreRiga As Double = MatriceValori(Riga - Pavarot.TimeRangeStandardPavarot.IdRigaIniziale)
              If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
                ValoreRiga = System.Math.Abs(ValoreRiga)
              End If
              If TogglaSegno Then ValoreRiga *= ToggleSign
              If ValoriDerivati Then
                Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
                MediaMobile.AggiornaMedia(Derivata)
                ValoreRiga = MediaMobile.Valore
                ValoreRigaPrev = ValoreRiga
                MomentoPrev = Momento
              End If

              If Gruppi.ContainsKey(Chiave) Then
                Gruppi(Chiave).AggiungiCoppia(CInt(SecsFromKeyMoment * Moltiplicatore), ValoreRiga)
              Else
                Gruppi.Add(Chiave, New clsGruppoPeriodi(CInt(SecsFromKeyMoment * Moltiplicatore), ValoreRiga, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
              End If

            End If
          Next
        End If

      End If
    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastLineRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Stroke = Gruppo.Value.Colore
      If Gruppo.Key.ToString.IndexOf("Stbd") > -1 Then
        Linea.StrokeDashArray = {3, 3}
      End If

      Linea.StrokeThickness = 2
      Linea.Tag = Gruppo.Value ' .Key ' Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      For Each Valore In Gruppo.Value.ValoriAggregati.Dizionario.OrderBy(Function(x) x.Key)
        DataSeries.Append(Valore.Key / Moltiplicatore, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
      Next
      Linea.DataSeries = DataSeries
      Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVM)
    Next

    'OnPropertyChanged("SeriesSource")


  End Sub



End Class

Public Class clsValoriY
  Dim pValoriY As New List(Of Double)

  Public Sub New(Valore As Double)
    pValoriY.Add(Valore)
  End Sub

  Public Property ValoriY As List(Of Double)
    Get
      Return pValoriY
    End Get
    Set(value As List(Of Double))
      pValoriY = value
    End Set
  End Property

End Class

Public Class clsValoriAggregati
  Dim pDizionario As New Dictionary(Of Integer, clsValoriY)

  Public Property Dizionario As Dictionary(Of Integer, clsValoriY)
    Get
      Return pDizionario
    End Get
    Set(value As Dictionary(Of Integer, clsValoriY))
      pDizionario = value
    End Set
  End Property

  Public Sub AggiungiCoppia(ValoreX As Integer, ValoreY As Double)
    If pDizionario.ContainsKey(ValoreX) Then
      pDizionario(ValoreX).ValoriY.Add(ValoreY)
    Else
      pDizionario.Add(ValoreX, New clsValoriY(ValoreY))
    End If
  End Sub

End Class

Public Class clsDerivata
  Dim pValoreRigaPrev As Double = Double.NaN
  Dim pMomentoPrev As DateTime
  Dim pMediaMobile As clsMediaMobile

  Public Sub New(Campioni As Integer, Is360 As Boolean, MomentoPrev As DateTime)
    pMediaMobile = New clsMediaMobile(Campioni, Is360)
    pMomentoPrev = MomentoPrev

  End Sub

  Public Property ValoreRigaPrev As Double
    Get
      Return pValoreRigaPrev
    End Get
    Set(value As Double)
      pValoreRigaPrev = value
    End Set
  End Property

  Public Property MomentoPrev As Date
    Get
      Return pMomentoPrev
    End Get
    Set(value As Date)
      pMomentoPrev = value
    End Set
  End Property

  Public Property MediaMobile As clsMediaMobile
    Get
      Return pMediaMobile
    End Get
    Set(value As clsMediaMobile)
      pMediaMobile = value
    End Set
  End Property

  Public Sub AggiornaValoriPrev(ValoreRigaPrev As Double, MomentoPrev As Date)
    pValoreRigaPrev = ValoreRigaPrev
    pMomentoPrev = MomentoPrev
  End Sub
End Class


Public Class UserControlPavarotPlotViewModelMathPlots ' canali custom calcolati da altri messi in alto tra quelli orizzontali
  Inherits UserControlPavarotPlotViewModel

  'Public Sub New(ParentViewModel As UserControlPavarotViewModel, Canale As clsChannel2020)
  '  MyBase.New(ParentViewModel, Canale)
  '  Titolo = "Loss Progression"
  '  Me.Canale = Canale
  '  Me.CanaleCustom = Nothing
  'End Sub

  Public Sub New(ParentViewModel As UserControlPavarotViewModel, CanaleCustom As eCanaleCustom)
    MyBase.New(ParentViewModel, Nothing)
    Me.CanaleCustom = CanaleCustom
    Me.Canale = Nothing

    Select Case CanaleCustom
      Case eCanaleCustom.eProgressionLoss
        Titolo = "Loss Progression"
      Case eCanaleCustom.eEntryTack
        Titolo = "1:Stbd -1:Port"
      Case eCanaleCustom.eHdgVariation
        Titolo = "Heading Variation"
      Case eCanaleCustom.eCseVariation
        Titolo = "Course Variation"
      Case eCanaleCustom.eCogVariation
        Titolo = "Cog Variation"
      Case eCanaleCustom.eTwaVariation
        Titolo = "Twa Variation"
      Case eCanaleCustom.eTwdVariation
        Titolo = "Twd delta (positive if lifting)"
      Case eCanaleCustom.eHdgTwaDelta
        Titolo = "Hdg Twa delta"
      Case eCanaleCustom.eCseTwaDelta
        Titolo = "Cse Twa delta"
      Case eCanaleCustom.eCogTwaDelta
        Titolo = "Cog Twa delta"
      Case eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
        Titolo = "Twd Variation minus theoretical shift"
    End Select

  End Sub

  Private Function Is360() As Boolean
    Select Case CanaleCustom
      Case eCanaleCustom.eCogVariation, eCanaleCustom.eCseVariation, eCanaleCustom.eHdgVariation, eCanaleCustom.eTwdVariation, eCanaleCustom.eCogTwaDelta, eCanaleCustom.eCseTwaDelta, eCanaleCustom.eHdgTwaDelta, eCanaleCustom.eTwaVariation, eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
        Return False
      Case Else
        Return False
    End Select
  End Function

  'special channels
  Public Overrides Sub DrawChart()
    SeriesSource.Clear()
    Select Case ParentVM.OutputType
      Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
        DrawChartGroupByKeys()
        Exit Sub
      Case UserControlPavarotViewModel.eOutputType.eGroupByTack
        DrawChartAggregatiPerMure()
        Exit Sub
    End Select

    'qui potrei ottimizzare ma... almeno per ora mastica
    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chCse Is Nothing Then
      chCse = DataProvider2020.CseFromHdgAndLeeway()
    End If
    Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

    Dim CanaleNotNothing As clsChannel2020 = Nothing

    For Each Pavarot In PeriodsManager.CollectionPavarot

      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For

      Dim Colore As System.Windows.Media.Color = Pavarot.Colore ' PavarotSyncViewModel.Colori(i)
      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
      If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          Colore = Colors.Green
        Else
          Colore = Colors.Red
        End If
      End If
      Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, 1, -1)

      Dim a As Integer = 0
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      LineaTmp.Stroke = Colore
      'If Pavarot.IsStbd Then
      '  LineaTmp.StrokeDashArray = {3, 3}
      'End If
      LineaTmp.StrokeThickness = 2
      LineaTmp.Tag = Pavarot
      'LineaTmp.Visibility = IIf(Pavarot.IsChecked, Visibility.Visible, Visibility.Collapsed)
      'Dim ss As Double = Pavarot.TimeRangeStandardPavarot.Start.Subtract(Pavarot.KeyMoment).TotalSeconds
      Dim ss As Double = -1 ' -Pavarot.PavarotStandardRange
      'Select Case pCanaleCustom
      '  Case eCanaleCustom.eProgressionLoss, eCanaleCustom.eHdgVariation, eCanaleCustom.eCseVariation, eCanaleCustom.eCogVariation, eCanaleCustom.eTwdVariation, eCanaleCustom.eCogTwaDelta, eCanaleCustom.eCseTwaDelta, eCanaleCustom.eHdgTwaDelta, eCanaleCustom.eTwavariation
      '    DataSeriesTMP.Append(ss, 0, New clsPuntoMetadata(False))
      '  Case eCanaleCustom.eEntryTack
      '    Dim Valore As Double = IIf(Pavarot.IsStbd, 1, -1)
      '    DataSeriesTMP.Append(ss, Valore, New clsPuntoMetadata(False))
      'End Select
      'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      ''Dim RigaRef As Integer = DataProvider2020.TrovaIndice(Pavarot.PavarotEntry)
      'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      'Dim righe As Integer = RigaFinale - RigaRef
      'Dim sss = righe / DataProvider2020.Hz
      'If sss > Pavarot.TimeRangeStandardPavarot.Durata.TotalSeconds + 1 Then
      '  Dim momento As DateTime = Pavarot.TimeRangeStandardPavarot.Finish
      '  Do While RigaFinale > RigaRef
      '    RigaFinale -= 1
      '    Dim momentoP As DateTime = DataProvider2020.Momento(RigaFinale)
      '    If momento > momentoP Then
      '      Exit Do
      '    End If
      '  Loop
      'End If

      Dim objDerivata As New clsDerivata(DataProvider2020.Hz, Is360, Pavarot.PavarotEntry)
      'Dim VmgTgt As Double = -1

      Dim RefTime As DateTime = Pavarot.PavarotEntry ' Period2021.KeyMoment

      Dim RefPos As clsGeographicPosition = Nothing
      Dim RigaRef As Integer = DataProvider2020.TrovaIndice(RefTime)

      For Riga As Integer = RigaIniziale To RigaFinale
        Dim Momento As DateTime = DataProvider2020.Momento(Riga)
        If Momento.ToOADate > 0 Then
          ss = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds
          Dim ValoreRiga As Double = Double.NaN

          Select Case CanaleCustom
            Case eCanaleCustom.eProgressionLoss
              Dim SecFromEntry As Double = Momento.Subtract(RefTime).TotalSeconds
              If RefPos Is Nothing Then
                RefPos = Pavarot.PavarotDetails.PosizioneAtTime(RefTime)
              End If
              'If Riga = RigaRef Then Stop
              ValoreRiga = Pavarot.PavarotDetails.MetriGhostVmgDaPosizione(SecFromEntry, Riga, RefPos, Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe, Pavarot.PeriodTgt.Vmg)
            Case eCanaleCustom.eEntryTack
              Dim Valore As Double = IIf(Pavarot.IsStbd, 1, -1)
              ValoreRiga = Valore
            Case eCanaleCustom.eHdgVariation
              CanaleNotNothing = chHdg
              If CanaleNotNothing Is Nothing Then
                CanaleNotNothing = chCse
                If CanaleNotNothing Is Nothing Then
                  CanaleNotNothing = chCog
                End If
              End If
              Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
              Dim Valore As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(ValoreNaN(CanaleNotNothing, Riga), ValoreNaN(CanaleNotNothing, RigaRef)) * Segno
              If Double.IsNaN(ValoreNaN(CanaleNotNothing, Riga)) Then
                ValoreRiga = Double.NaN
              Else
                ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
              End If
            Case eCanaleCustom.eCseVariation
              CanaleNotNothing = chCse
              If CanaleNotNothing Is Nothing Then
                CanaleNotNothing = chHdg
                If CanaleNotNothing Is Nothing Then
                  CanaleNotNothing = chCog
                End If
              End If
              Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
              Dim Valore As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(ValoreNaN(CanaleNotNothing, Riga), ValoreNaN(CanaleNotNothing, RigaRef)) * Segno
              If Double.IsNaN(ValoreNaN(CanaleNotNothing, Riga)) Then
                ValoreRiga = Double.NaN
              Else
                ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
              End If
            Case eCanaleCustom.eTwdVariation
              Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaRef)
              Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
              If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                ' twd variation positiva se twd stbd > twd port
                Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            Case eCanaleCustom.eHdgTwaDelta
              Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaRef))
              Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
              Dim BrgIniziale As Double = ValoreNaN(chHdg, RigaRef)
              Dim BrgAttuale As Double = ValoreNaN(chHdg, Riga)
              If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                ValoreRiga = DeltaBrg - DeltaTwa
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            Case eCanaleCustom.eCseTwaDelta
              Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaRef))
              Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
              Dim BrgIniziale As Double = ValoreNaN(chCse, RigaRef)
              Dim BrgAttuale As Double = ValoreNaN(chCse, Riga)
              If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                ValoreRiga = DeltaBrg - DeltaTwa
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            Case eCanaleCustom.eTwaVariation
              Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaRef))
              Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
              If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) Then
                Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                ValoreRiga = (TwaIniziale - TwaAttuale) * Segno
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            Case eCanaleCustom.eCogTwaDelta
              Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaRef))
              Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
              Dim BrgIniziale As Double = ValoreNaN(chCog, RigaRef)
              Dim BrgAttuale As Double = ValoreNaN(chCog, Riga)
              If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                ValoreRiga = DeltaBrg - DeltaTwa
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            Case eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
              Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
              Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
              Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
              Dim TheoreticalShift As Double = PavarotTgtAngle - DeltaCse ' positivo = lift

              Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaRef)
              Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
              If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                ' twd variation positiva se twd stbd > twd port
                Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
                ValoreRiga -= TheoreticalShift
              Else
                ValoreRiga = Double.NaN
              End If
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If

            Case eCanaleCustom.eCogVariation
              CanaleNotNothing = chCog
              If CanaleNotNothing Is Nothing Then
                CanaleNotNothing = chCse
                If CanaleNotNothing Is Nothing Then
                  CanaleNotNothing = chHdg
                End If
              End If
              Dim CogIniziale As Double = ValoreNaN(CanaleNotNothing, RigaRef)
              Dim CogAttuale As Double = ValoreNaN(CanaleNotNothing, Riga)
              If Not Double.IsNaN(CogIniziale) AndAlso Not Double.IsNaN(CogAttuale) Then
                Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                Dim Valore As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CogAttuale, CogIniziale) * Segno
                ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
              Else
                ValoreRiga = Double.NaN
              End If
              'If Double.IsNaN(ValoreNaN(CanaleNotNothing, Riga)) Then
              '  ValoreRiga = Double.NaN
              'Else
              '  ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
              'End If
              'If ValoreRiga < -90 Then ValoreRiga += 360
          End Select
          If Not ValoreRiga = Double.NaN Then
            If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
            If ValoriDerivati Then
              If Not Double.IsNaN(ValoreRiga) Then
                Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                objDerivata.MediaMobile.AggiornaMedia(Derivata)
                DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
              End If
            Else
              If Not Double.IsNaN(ValoreRiga) Then DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
            End If
          End If
        End If

      Next
      LineaTmp.IsVisible = Pavarot.IsChecked
      LineaTmp.DataSeries = DataSeriesTMP

      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
    Next
  End Sub

  Private Function ValoreNaN(Canale As clsChannel2020, Indice As Integer) As Double
    If Canale Is Nothing Then Return Double.NaN
    Return Canale.Valori(Indice)
  End Function


  Public Sub DrawChartAggregatiPerMure()

    'qui potrei ottimizzare ma... almeno per ora mastica
    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chCse Is Nothing Then
      chCse = DataProvider2020.CseFromHdgAndLeeway()
    End If
    Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)

    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then

        Dim Colore As System.Windows.Media.Color = Pavarot.Colore ' PavarotSyncViewModel.Colori(i)
        'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)


        Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, 1, -1)


        Dim ss As Double = Pavarot.TimeRangeStandardPavarot.Start.Subtract(Pavarot.KeyMoment).TotalSeconds
        Select Case CanaleCustom
          Case eCanaleCustom.eProgressionLoss, eCanaleCustom.eHdgVariation, eCanaleCustom.eCseVariation, eCanaleCustom.eCogVariation, eCanaleCustom.eTwdVariation, eCanaleCustom.eCogTwaDelta, eCanaleCustom.eCseTwaDelta, eCanaleCustom.eHdgTwaDelta, eCanaleCustom.eTwaVariation, eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
            If Pavarot.IsStbd Then
              ValoriStbd.AggiungiCoppia(ss, 0)
            Else
              ValoriPort.AggiungiCoppia(ss, 0)
            End If
          'DataSeriesTMP.Append(ss, 0, New clsPuntoMetadata(False))
          Case eCanaleCustom.eEntryTack
            Dim Valore As Double = IIf(Pavarot.IsStbd, 1, -1)
            If Pavarot.IsStbd Then
              ValoriStbd.AggiungiCoppia(ss, Valore)
            Else
              ValoriPort.AggiungiCoppia(ss, Valore)
            End If
            'DataSeriesTMP.Append(ss, Valore, New clsPuntoMetadata(False))
        End Select
        'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.PavarotEntry)
        'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
        Dim objDerivata As New clsDerivata(DataProvider2020.Hz, Is360, Pavarot.PavarotEntry)
        Dim VmgTgt As Double = 0
        If Not TgtManager Is Nothing Then
          If Not TgtManager.Tgt Is Nothing Then
            VmgTgt = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Vmg
          End If
        End If
        For Riga As Integer = RigaIniziale To RigaFinale 'Pavarot.IdUltimaRigaTabella
          Dim Momento As DateTime = DataProvider2020.Momento(Riga)
          If Momento.ToOADate > 0 Then
            ss = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds '(Riga - Pavarot.IdRigaKeyMoment(clsPavarot2019.eAxisRef.eTWD)) / DataProvider2020.RawFileHz
            Dim ValoreRiga As Double = Double.NaN

            Select Case CanaleCustom
              Case eCanaleCustom.eProgressionLoss
                Dim SecFromEntry As Double = Momento.Subtract(Pavarot.PavarotEntry).TotalSeconds
                If SecFromEntry < 0 Then
                  ValoreRiga = 0
                Else
                  ValoreRiga = Pavarot.PavarotDetails.MetriGhostVmgDaEntry(SecFromEntry, VmgTgt, Pavarot.IdRigaPavarotEntry, Riga)
                End If
              Case eCanaleCustom.eEntryTack
                Dim Valore As Double = IIf(Pavarot.IsStbd, 1, -1)
                ValoreRiga = Valore
              Case eCanaleCustom.eHdgVariation
                Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                Dim Valore As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(chHdg.Valori(Riga), chHdg.Valori(RigaIniziale)) * Segno
                If Double.IsNaN(chHdg.Valori(Riga)) Then
                  ValoreRiga = Double.NaN
                Else
                  ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                End If
              Case eCanaleCustom.eCseVariation
                Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                Dim Valore As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(chCse.Valori(Riga), chCse.Valori(RigaIniziale)) * Segno
                If Double.IsNaN(chCse.Valori(Riga)) Then
                  ValoreRiga = Double.NaN
                Else
                  ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                End If
              Case eCanaleCustom.eTwdVariation
                Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaIniziale)
                Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
                If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                  ' twd variation positiva se twd stbd > twd port
                  Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                  ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eHdgTwaDelta
                Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                Dim BrgIniziale As Double = ValoreNaN(chHdg, RigaIniziale)
                Dim BrgAttuale As Double = ValoreNaN(chHdg, Riga)
                If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                  Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                  Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                  ValoreRiga = DeltaBrg - DeltaTwa
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eCseTwaDelta
                Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                Dim BrgIniziale As Double = ValoreNaN(chCse, RigaIniziale)
                Dim BrgAttuale As Double = ValoreNaN(chCse, Riga)
                If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                  Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                  Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                  ValoreRiga = DeltaBrg - DeltaTwa
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eCogTwaDelta
                Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                Dim BrgIniziale As Double = ValoreNaN(chCog, RigaIniziale)
                Dim BrgAttuale As Double = ValoreNaN(chCog, Riga)
                If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                  Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                  Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                  ValoreRiga = DeltaBrg - DeltaTwa
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eTwaVariation
                Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) Then
                  Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                  ValoreRiga = (TwaIniziale - TwaAttuale) * Segno
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eCogVariation
                Dim CogIniziale As Double = ValoreNaN(chCog, RigaIniziale)
                Dim CogAttuale As Double = ValoreNaN(chCog, Riga)
                If Not Double.IsNaN(CogIniziale) AndAlso Not Double.IsNaN(CogAttuale) Then
                  Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                  Dim DeltaBrg As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CogAttuale, CogIniziale)
                  Dim Valore As Double = DeltaBrg * Segno
                  ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                Else
                  ValoreRiga = Double.NaN
                End If
              Case eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
                Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
                Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
                Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
                Dim TheoreticalShift As Double = PavarotTgtAngle - DeltaCse ' positivo = lift

                Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaIniziale)
                Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
                If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                  ' twd variation positiva se twd stbd > twd port
                  Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                  ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
                  ValoreRiga -= TheoreticalShift
                Else
                  ValoreRiga = Double.NaN
                End If



                'Dim PavarotTws As New clsStatisticheIntervallo(chTwa)
                'PavarotTws.AggiornaIntervallo(New clsTimeRange(Pavarot.PavarotEntry, Pavarot.TimeRangeStandardPavarot.Finish), clsGroupLines.eLineType.eRawValue)
                'Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa

                Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                Dim BrgIniziale As Double = ValoreNaN(chCse, RigaIniziale)
                Dim BrgAttuale As Double = ValoreNaN(chCse, Riga)
                If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                  Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                  Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                  ValoreRiga = DeltaBrg - DeltaTwa
                Else
                  ValoreRiga = Double.NaN
                End If

            End Select
            If Not Double.IsNaN(ValoreRiga) Then
              If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = ValoreRiga
              If ValoriDerivati Then
                If Not Double.IsNaN(ValoreRiga) Then
                  Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                  objDerivata.MediaMobile.AggiornaMedia(Derivata)
                  If Pavarot.IsStbd Then
                    ValoriStbd.AggiungiCoppia(ss, objDerivata.MediaMobile.Valore)
                  Else
                    ValoriPort.AggiungiCoppia(ss, objDerivata.MediaMobile.Valore)
                  End If
                  'DataSeriesTMP.Append(ss, objDerivata.MediaMobile.Valore, New clsPuntoMetadata(False))
                  objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                End If
              Else
                If Not Double.IsNaN(ValoreRiga) Then
                  If Pavarot.IsStbd Then
                    ValoriStbd.AggiungiCoppia(ss, ValoreRiga)
                  Else
                    ValoriPort.AggiungiCoppia(ss, ValoreRiga)
                  End If
                  'DataSeriesTMP.Append(ss, ValoreRiga, New clsPuntoMetadata(False))
                End If
              End If
            End If
          End If
        Next
      End If
    Next

    'If ValoriDerivati Then Titolo &= " Derivative"


    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    Dim LineaPort As New FastLineRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.Stroke = Colors.Red
    LineaPort.StrokeThickness = 2
    LineaPort.Tag = -1
    'DataSeriesPort.AcceptsUnsortedData = True
    For Each Valore In ValoriPort.Dizionario.OrderBy(Function(x) x.Key)
      Dim SS As Double = Valore.Key
      Dim Y As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
      DataSeriesPort.Append(SS, Y, New clsPuntoMetadata(False))
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    SeriesSource.Add(CSVMport)

    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim LineaStbd As New FastLineRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.Stroke = Colors.Green
    LineaStbd.StrokeThickness = 2
    LineaStbd.Tag = 1
    'DataSeriesStbd.AcceptsUnsortedData = True
    For Each Valore In ValoriStbd.Dizionario.OrderBy(Function(x) x.Key)
      Dim SS As Double = Valore.Key
      Dim Y As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
      DataSeriesStbd.Append(SS, Y, New clsPuntoMetadata(False))
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    SeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")

  End Sub

  Public Sub DrawChartGroupByKeys()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chCse Is Nothing Then
      chCse = DataProvider2020.CseFromHdgAndLeeway()
    End If
    Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

    Dim Moltiplicatore As Integer = 2

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      'cicla tra le pavarot checkate
      If Pavarot.IsChecked Then
        'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
        'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
        Dim Chiave As String = Pavarot.Keys

        If Not Chiave.Trim = "" Then

          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, contatore)
            contatore += 1
          End If

          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack OrElse ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
            If Pavarot.IsStbd Then
              Chiave = "Stbd " & Chiave
              Mure = clsGruppoPeriodi.eMure.eStbd
            Else
              Chiave = "Port " & Chiave
              Mure = clsGruppoPeriodi.eMure.ePort
            End If
          End If

          Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, 1, -1)
          Dim ss As Double = Pavarot.TimeRangeStandardPavarot.Start.Subtract(Pavarot.KeyMoment).TotalSeconds
          Dim Valore As Double = 0
          Select Case CanaleCustom
            Case eCanaleCustom.eProgressionLoss, eCanaleCustom.eHdgVariation, eCanaleCustom.eCseVariation, eCanaleCustom.eCogVariation, eCanaleCustom.eTwdVariation, eCanaleCustom.eCogTwaDelta, eCanaleCustom.eCseTwaDelta, eCanaleCustom.eHdgTwaDelta, eCanaleCustom.eTwaVariation, eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
            Case eCanaleCustom.eEntryTack
              Valore = IIf(Pavarot.IsStbd, 1, -1)
          End Select
          If Gruppi.ContainsKey(Chiave) Then
            Gruppi(Chiave).AggiungiCoppia(CInt(ss * Moltiplicatore), Valore)
          Else
            Gruppi.Add(Chiave, New clsGruppoPeriodi(CInt(ss * Moltiplicatore), Valore, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          End If

          'Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.PavarotEntry)
          'Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
          Dim objDerivata As New clsDerivata(DataProvider2020.Hz, Is360, Pavarot.PavarotEntry)
          Dim VmgTgt As Double = 0
          If Not TgtManager Is Nothing Then
            If Not TgtManager.Tgt Is Nothing Then
              VmgTgt = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Vmg
            End If
          End If
          For Riga As Integer = RigaIniziale To RigaFinale
            Dim Momento As DateTime = DataProvider2020.Momento(Riga)
            If Momento.ToOADate > 0 Then
              ss = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds '(Riga - Pavarot.IdRigaKeyMoment(clsPavarot2019.eAxisRef.eTWD)) / DataProvider2020.RawFileHz
              Dim ValoreRiga As Double = Double.NaN

              Select Case CanaleCustom
                Case eCanaleCustom.eProgressionLoss
                  Dim SecFromEntry As Double = Momento.Subtract(Pavarot.PavarotEntry).TotalSeconds
                  If SecFromEntry < 0 Then
                    ValoreRiga = 0
                  Else
                    ValoreRiga = Pavarot.PavarotDetails.MetriGhostVmgDaEntry(SecFromEntry, VmgTgt, Pavarot.IdRigaPavarotEntry, Riga)
                  End If
                Case eCanaleCustom.eEntryTack
                  Valore = IIf(Pavarot.IsStbd, 1, -1)
                  ValoreRiga = Valore
                Case eCanaleCustom.eHdgVariation
                  Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                  Valore = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(chHdg.Valori(Riga), chHdg.Valori(RigaIniziale)) * Segno
                  If Double.IsNaN(chCse.Valori(Riga)) Then
                    ValoreRiga = Double.NaN
                  Else
                    ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                  End If
                Case eCanaleCustom.eCseVariation
                  Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                  Valore = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(chCse.Valori(Riga), chCse.Valori(RigaIniziale)) * Segno
                  If Double.IsNaN(chCse.Valori(Riga)) Then
                    ValoreRiga = Double.NaN
                  Else
                    ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                  End If
                Case eCanaleCustom.eTwdVariation
                  Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaIniziale)
                  Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
                  If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                    ' twd variation positiva se twd stbd > twd port
                    Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                    ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eHdgTwaDelta
                  Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                  Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                  Dim BrgIniziale As Double = ValoreNaN(chHdg, RigaIniziale)
                  Dim BrgAttuale As Double = ValoreNaN(chHdg, Riga)
                  If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                    Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                    Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                    ValoreRiga = DeltaBrg - DeltaTwa
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eCseTwaDelta
                  Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                  Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                  Dim BrgIniziale As Double = ValoreNaN(chCse, RigaIniziale)
                  Dim BrgAttuale As Double = ValoreNaN(chCse, Riga)
                  If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                    Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                    Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                    ValoreRiga = DeltaBrg - DeltaTwa
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eCogTwaDelta
                  Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                  Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                  Dim BrgIniziale As Double = ValoreNaN(chCog, RigaIniziale)
                  Dim BrgAttuale As Double = ValoreNaN(chCog, Riga)
                  If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) AndAlso Not Double.IsNaN(BrgIniziale) AndAlso Not Double.IsNaN(BrgAttuale) Then
                    Dim DeltaTwa As Double = Math.Abs(TwaIniziale - TwaAttuale)
                    Dim DeltaBrg As Double = DifferenzaAssolutaTraAngoli360(BrgAttuale, BrgIniziale)
                    ValoreRiga = DeltaBrg - DeltaTwa
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eTwaVariation
                  Dim TwaIniziale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, RigaIniziale))
                  Dim TwaAttuale As Double = TwaPoppaPerFunzioneDelta(ValoreNaN(chTwa, Riga))
                  If Not Double.IsNaN(TwaIniziale) AndAlso Not Double.IsNaN(TwaAttuale) Then
                    Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                    ValoreRiga = (TwaIniziale - TwaAttuale) * Segno
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eCogVariation
                  Dim CogIniziale As Double = ValoreNaN(chCog, RigaIniziale)
                  Dim CogAttuale As Double = ValoreNaN(chCog, Riga)
                  If Not Double.IsNaN(CogIniziale) AndAlso Not Double.IsNaN(CogAttuale) Then
                    Dim Segno As Double = IIf(Pavarot.IsStbd, 1, -1)
                    Valore = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CogAttuale, CogIniziale) * Segno
                    ValoreRiga = IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1) * Valore
                  Else
                    ValoreRiga = Double.NaN
                  End If
                Case eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta
                  Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
                  Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
                  Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
                  Dim TheoreticalShift As Double = PavarotTgtAngle - DeltaCse ' positivo = lift

                  Dim TwdIniziale As Double = ValoreNaN(chTwd, RigaIniziale)
                  Dim TwdAttuale As Double = ValoreNaN(chTwd, Riga)
                  If Not Double.IsNaN(TwdIniziale) AndAlso Not Double.IsNaN(TwdAttuale) Then
                    ' twd variation positiva se twd stbd > twd port
                    Dim Segno As Double = IIf(Pavarot.IsStbd, -1, 1)
                    ValoreRiga = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdIniziale, TwdAttuale) * Segno
                    ValoreRiga -= TheoreticalShift
                  Else
                    ValoreRiga = Double.NaN
                  End If

              End Select
              If Not ValoreRiga = Double.NaN Then
                If objDerivata.ValoreRigaPrev = Double.NaN Then objDerivata.ValoreRigaPrev = ValoreRiga
                If ValoriDerivati Then
                  If Not Double.IsNaN(ValoreRiga) Then
                    Dim Derivata As Double = (ValoreRiga - objDerivata.ValoreRigaPrev) / Momento.Subtract(objDerivata.MomentoPrev).TotalSeconds
                    objDerivata.MediaMobile.AggiornaMedia(Derivata)
                    If Gruppi.ContainsKey(Chiave) Then
                      Gruppi(Chiave).AggiungiCoppia(CInt(ss * Moltiplicatore), objDerivata.MediaMobile.Valore)
                    Else
                      Gruppi.Add(Chiave, New clsGruppoPeriodi(CInt(ss * Moltiplicatore), objDerivata.MediaMobile.Valore, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
                    End If
                    objDerivata.AggiornaValoriPrev(ValoreRiga, Momento)
                  End If
                Else
                  If Not Double.IsNaN(ValoreRiga) Then
                    If Gruppi.ContainsKey(Chiave) Then
                      Gruppi(Chiave).AggiungiCoppia(CInt(ss * Moltiplicatore), ValoreRiga)
                    Else
                      Gruppi.Add(Chiave, New clsGruppoPeriodi(CInt(ss * Moltiplicatore), ValoreRiga, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
                    End If
                  End If
                End If
              End If
            End If
          Next
        End If
      End If
    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastLineRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Stroke = Gruppo.Value.Colore
      Linea.StrokeThickness = 2
      If Gruppo.Key.ToString.IndexOf("Stbd") > -1 Then
        Linea.StrokeDashArray = {3, 3}
      End If
      Linea.Tag = Gruppo.Value ' .Key ' Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      For Each Valore In Gruppo.Value.ValoriAggregati.Dizionario.OrderBy(Function(x) x.Key).ToArray
        DataSeries.Append(Valore.Key / Moltiplicatore, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
      Next
      Linea.DataSeries = DataSeries
      Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVM)
    Next

    'OnPropertyChanged("SeriesSource")


  End Sub


  'Public Sub DrawChartAggregatiPerMure()
  '  If Canale Is Nothing Then Exit Sub
  '  Dim TogglaSegno As Boolean = ToggleSigned
  '  Select Case Canale.CanaleChiave
  '    Case clsChannels2020.eCanaliChiave.eRdrAngle, clsChannels2020.eCanaliChiave.eYRT
  '      TogglaSegno = True
  '    Case clsChannels2020.eCanaliChiave.eNone
  '      Select Case Canale.ActualLogHeader
  '        Case "maintwist", "maintrav"
  '          TogglaSegno = True
  '      End Select
  '    Case Else

  '  End Select

  '  Titolo = Canale.LongName


  '  Dim ValoriPort As New clsValoriAggregati
  '  Dim ValoriStbd As New clsValoriAggregati

  '  SeriesSource.Clear()
  '      For Each Pavarot In PeriodsManager.ListaPavarot
  '    If PeriodsManager.ListaPavarot(i).IsChecked Then

  '      Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
  '      Dim Colore As System.Windows.Media.Color = PeriodsManager.ListaPavarot(i).Colore ' PavarotSyncViewModel.Colori(i)
  '      Dim ValoreAnte As Double = MediaAnte(Pavarot.TimeRangeEntry) '.VMGms(PavarotSyncViewModel.AsseRiferimento, True)
  '      If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
  '        ValoreAnte = System.Math.Abs(ValoreAnte)
  '      End If
  '      Dim ToggleSign As Integer = IIf(Pavarot.IsStbd, -1, 1)
  '      ToggleSign *= IIf(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, -1, 1)

  '      If TogglaSegno Then ValoreAnte *= ToggleSign
  '      Dim MatriceValori As List(Of Double) = Valori(Pavarot.TimeRangeStandardPavarot, Pavarot.IsStbd)
  '      If MatriceValori Is Nothing Then Exit Sub

  '      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Inizio)
  '      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Fine)
  '      RigaFinale = System.Math.Min(RigaFinale, RigaIniziale + MatriceValori.Count - 1)
  '      Dim ValoreRigaPrev As Double = MatriceValori(0)
  '      Dim MomentoPrev As DateTime = Pavarot.TimeRangeStandardPavarot.Inizio
  '      Dim MediaMobile As New clsMediaMobile(DataProvider2020.Hz, Canale.DataType = clsChannel2020.eDataType.e360)
  '      For Riga As Integer = RigaIniziale To RigaFinale
  '        Dim Momento As DateTime = DataProvider2020.Momento(Riga)
  '        If Momento.ToOADate > 0 Then
  '          Dim SecsFromKeyMoment As Double = Momento.Subtract(Pavarot.KeyMoment).TotalSeconds
  '          Dim ValoreRiga As Double = MatriceValori(Riga - Pavarot.TimeRangeStandardPavarot.IdRigaIniziale)
  '          If Canale.DataType = clsChannel2020.eDataType.eAbs180 Then
  '            ValoreRiga = System.Math.Abs(ValoreRiga)
  '          End If
  '          If TogglaSegno Then ValoreRiga *= ToggleSign
  '          If ValoriDerivati Then
  '            Dim Derivata As Double = (ValoreRiga - ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            MediaMobile.AggiornaMedia(Derivata)
  '            If Pavarot.IsStbd Then
  '              ValoriStbd.AggiungiCoppia(CInt(SecsFromKeyMoment), MediaMobile.Valore)
  '            Else
  '              ValoriPort.AggiungiCoppia(CInt(SecsFromKeyMoment), MediaMobile.Valore)
  '            End If
  '            ValoreRigaPrev = ValoreRiga
  '            MomentoPrev = Momento
  '          Else
  '            If Pavarot.IsStbd Then
  '              ValoriStbd.AggiungiCoppia(CInt(SecsFromKeyMoment), ValoreRiga)
  '            Else
  '              ValoriPort.AggiungiCoppia(CInt(SecsFromKeyMoment), ValoreRiga)
  '            End If
  '          End If
  '        End If

  '      Next
  '    End If
  '  Next
  '  If ValoriDerivati Then Titolo &= " Derivative"


  '  Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
  '  Dim LineaPort As New FastLineRenderableSeries
  '  LineaPort.XAxisId = "DefaultAxisId"
  '  LineaPort.YAxisId = "DefaultAxisId"
  '  LineaPort.Stroke = Colors.Red
  '  LineaPort.StrokeThickness = 2
  '  LineaPort.Tag = -1
  '  For Each Valore In ValoriPort.Dizionario
  '    DataSeriesPort.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
  '  Next
  '  LineaPort.DataSeries = DataSeriesPort
  '  Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
  '  SeriesSource.Add(CSVMport)

  '  Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
  '  Dim LineaStbd As New FastLineRenderableSeries
  '  LineaStbd.XAxisId = "DefaultAxisId"
  '  LineaStbd.YAxisId = "DefaultAxisId"
  '  LineaStbd.Stroke = Colors.Green
  '  LineaStbd.StrokeThickness = 2
  '  LineaStbd.Tag = 1
  '  For Each Valore In ValoriStbd.Dizionario
  '    DataSeriesStbd.Append(Valore.Key, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
  '  Next
  '  LineaStbd.DataSeries = DataSeriesStbd
  '  Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
  '  SeriesSource.Add(CSVMStbd)

  '  OnPropertyChanged("SeriesSource")
  'End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public MustInherit Class UserControlPavarotPlotViewModel
  'Implements INotifyPropertyChanged
  'Dim pPavarotSyncViewModel As clsPavarotSyncViewModel
  Public Property ParentVM As UserControlPavarotViewModel
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property Titolo As String

  Dim _ToggleSigned As Boolean
  Public Property ToggleSigned As Boolean
    Get
      Return _ToggleSigned
    End Get
    Set(value As Boolean)
      _ToggleSigned = value
      DrawChart()
    End Set
  End Property

  Dim _ValoriDerivati As Boolean
  Public Property ValoriDerivati As Boolean
    Get
      Return _ValoriDerivati
    End Get
    Set(value As Boolean)
      _ValoriDerivati = value
      DrawChart()
    End Set
  End Property

  Public Property Canale As clsChannel2020
  Public Property CanaleOmologo As clsChannel2020
  Public Property CanaleCustom As eCanaleCustom

  Public Enum eCanaleCustom
    eProgressionLoss = 11
    eEntryTack = 1
    eHdgVariation = 2
    eCseVariation = 3
    eCogVariation = 4
    eTwdVariation = 5
    eHdgTwaDelta = 6
    eCseTwaDelta = 7
    eCogTwaDelta = 8
    eTwaVariation = 9
    eTwdDeltaMinusTwaToTgtDelta = 10
  End Enum

  Public Sub New(objParentVM As UserControlPavarotViewModel, Canale As clsChannel2020)
    Me.ParentVM = objParentVM
    Me.Canale = Canale
    If Not Canale Is Nothing Then
      Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(Canale.CanaleChiave) 'va messo cosí in modo da caricare i dati del canale se non é stato ancora fatto
      If Canale.ChannelId.Contains("Port") > -1 Then
        CanaleOmologo = DataProvider2020.CanaleDbl(Canale.ChannelId.Replace("Port", "Stbd"))
      ElseIf Canale.ChannelId.Contains("Stbd") > -1 Then
        CanaleOmologo = DataProvider2020.CanaleDbl(Canale.ChannelId.Replace("Stbd", "Port"))
      End If
    End If
  End Sub

  Public MustOverride Sub DrawChart()

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsPavarotSyncViewModel
  Public Sub New()
    ReDim Track2019X1(999)
    ReDim Track2019X2(999)
    ReDim Track2019Y1(999)
    ReDim Track2019Y2(999)
  End Sub


  Public Property SharedXVisibleRange As IRange
  Public Property SharedYVisibleRange As IRange
  Public Property CurrentPosition As Double 'secondi dallo 0 accetta numeri negativi)

  Public Property InizioCampionamentoEntry As Integer
  Public Property FineCampionamentoEntry As Integer
  Public Property InizioCampionamentoExit As Integer
  Public Property FineCampionamentoExit As Integer


  Public Property Track2019X1 As Double()
  Public Property Track2019X2 As Double()
  Public Property Track2019Y1 As Double()
  Public Property Track2019Y2 As Double()


  Public Property SquaredAxis As Boolean = False

  Public Property DataSeriesIndex As Integer

  Public Property LastXvisibleRange As DoubleRange
  Public Property LastYvisibleRange As DoubleRange

  Private Sub AggiornaValoriTemp(SecondsFromKeyMoment As Double)
    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      Dim Indice As Integer = DataProvider2020.TrovaIndice(Pavarot.KeyMoment.AddSeconds(SecondsFromKeyMoment))
      Pavarot.StrTempVal = Indice
    Next
  End Sub

  Private Sub AggiornaPosizioniSuTrack2019()

    Exit Sub


  End Sub


End Class

Public Class clsEllisse
  Dim pX1 As New List(Of Double)
  Dim pX2 As New List(Of Double)
  Dim pY1 As New List(Of Double)
  Dim pY2 As New List(Of Double)

  Public Sub AggiornaValori(X1 As Double, X2 As Double, Y1 As Double, Y2 As Double)
    pX1.Add(X1)
    pX2.Add(X2)
    pY1.Add(Y1)
    pY2.Add(Y2)
  End Sub

  Public Property X1 As List(Of Double)
    Get
      Return pX1
    End Get
    Set(value As List(Of Double))
      pX1 = value
    End Set
  End Property

  Public Property X2 As List(Of Double)
    Get
      Return pX2
    End Get
    Set(value As List(Of Double))
      pX2 = value
    End Set
  End Property

  Public Property Y1 As List(Of Double)
    Get
      Return pY1
    End Get
    Set(value As List(Of Double))
      pY1 = value
    End Set
  End Property

  Public Property Y2 As List(Of Double)
    Get
      Return pY2
    End Get
    Set(value As List(Of Double))
      pY2 = value
    End Set
  End Property
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotAdvancedPlotViewModel
  'Implements INotifyPropertyChanged
  Public Property CanaleAscissa As clsChannel2020
  Public Property Grafico As clsPavarotAdvancedPlotContainerViewModel.eGraphics
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property Annotazioni As New SciChart.Charting.Visuals.Annotations.AnnotationCollection
  Public Property TitoloPlot As String = "XY components"
  Public Property SyncVM As clsPavarotSyncViewModel
  Public Property DataBoxHeight As Double
  Public Property ParentVM As UserControlPavarotViewModel
  Public Property ApplicaDailyVmgPerc As Boolean = False


  Public Sub New(ChAscissa As clsChannel2020, Grafico As clsPavarotAdvancedPlotContainerViewModel.eGraphics, TitoloGrafico As String, objSyncVM As clsPavarotSyncViewModel, ParentVM As UserControlPavarotViewModel)
    Me.CanaleAscissa = ChAscissa
    Me.Grafico = Grafico
    Me.TitoloPlot = TitoloGrafico
    Me.SyncVM = objSyncVM
    Me.ParentVM = ParentVM
  End Sub

  Private Function ColoreTrasparente(Trasparenza As Byte, Colorebase As Windows.Media.Color) As Windows.Media.Color
    Return Windows.Media.Color.FromArgb(Trasparenza, Colorebase.R, Colorebase.G, Colorebase.B)
  End Function

  Public Sub DrawChart()
    Dim TmrDC As DateTime = Now
    ImpostaAnnotazioniBase()
    Dim MsAnn As Double = Now.Subtract(TmrDC).TotalMilliseconds
    SeriesSource.Clear()
    Dim TmrSel As DateTime = Now

    Select Case Grafico
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicTwsGainLoss
      '  Stop
      '  DrawPlotVmgTgVsTws()
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicTrackFromKeyMoment
        DrawXyPlotTrack()
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicTrackFromEntry
      '  DrawXyPlotTrackFromEntry()
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicPavarotSchema
        Stop
        'DrawPlotPavarotSchema()
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicPavarotManoeuverLoss
      '  DrawPlotPavarotLossSeconds(False, False, eOutputDelta.eSeconds)
      '  'DrawPlotPavarotLossManAndInvSeconds()
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicPavaroManoeverAndInvestmentLoss
      '  DrawPlotPavarotLossSeconds(True, False, eOutputDelta.eSeconds)
      '  'DrawPlotPavarotLossManInvAndPrjSeconds()
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicPavarotManInvAndExtraAccLoss
      '  DrawPlotPavarotLossSeconds(True, True, eOutputDelta.eSeconds)
      '  'DrawPlotPavarotGlobalLossInline()
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicBottomSpeed
        DrawPlotPavarotBottomSpeed()
      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicRateOfTurn
      '  DrawPlotPavarotRateOfTurn()
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicEntryStability
        'DrawPlotPavarotEntryStability()
        'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicRideHeightDs
        '  DrawPlotPavarotRideHeightStdDeviation()
        'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicCantDropMaxSpeed
        '  DrawPlotPavarotCantDropMaxSpeed()
      Case Else
        'Annotazioni.Clear()
        DrawXyPlot2021()
        If Grafico = clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta Then
          ImpostaAnnotazioni2022CseVsTgtDelta()
        End If

    End Select

    If dbg Then Console.WriteLine("     PD [" & TitoloPlot & "] tipo:" & Grafico.ToString() &
                                  " annotazioni:" & MsAnn.ToString("F0") & " ms" &
                                  " disegno:" & Now.Subtract(TmrSel).TotalMilliseconds.ToString("F0") & " ms" &
                                  " serie:" & SeriesSource.Count)

  End Sub

  Private Sub ImpostaAnnotazioniBase()
    Annotazioni.Clear()

    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.Y1 = 0
    AnV.StrokeThickness = 1
    AnV.Stroke = New SolidColorBrush(Colors.Red)
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart

    Dim VBR As New Binding("SyncVM.CurrentPosition")
    VBR.Source = Me
    VBR.Mode = BindingMode.TwoWay
    AnV.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

    Annotazioni.Add(AnV)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.Text = TitoloPlot
    AnWT.FontSize = 14
    AnWT.Foreground = New SolidColorBrush(Colors.SlateGray)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)
  End Sub


  'Private Sub DrawXyPlot()
  '    DrawXyPlot2021()
  '    Exit Sub
  '    Select Case ParentVM.OutputType
  '        Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
  '            DrawXyPlotGroupByKeys()
  '            Exit Sub
  '        Case UserControlPavarotViewModel.eOutputType.eGroupByTack, UserControlPavarotViewModel.eOutputType.eColorByTack
  '            DrawXyPlotGroupByTack()
  '            Exit Sub
  '    End Select
  '    'If ParentVM.PavarotStatusSync.GroupByKeys Then
  '    '  DrawXyPlotGroupByKeys()
  '    '  Exit Sub
  '    'ElseIf ParentVM.PavarotStatusSync.GroupByTack Then
  '    '  DrawXyPlotGroupByTack()
  '    '  Exit Sub
  '    'End If
  '    For Each Pavarot In PeriodsManager.CollectionPavarot
  '        'cicla tra le pavarot checkate
  '        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '        DataSeriesTMP.AcceptsUnsortedData = True
  '        Dim LineaTmp As New FastLineRenderableSeries
  '        LineaTmp.XAxisId = "DefaultAxisId"
  '        LineaTmp.YAxisId = "DefaultAxisId"

  '        If Pavarot.IsStbd Then
  '            LineaTmp.PointMarker = New EllipsePointMarker
  '        Else
  '            LineaTmp.PointMarker = New SquarePointMarker
  '        End If
  '        LineaTmp.PointMarker.Height = 10
  '        LineaTmp.PointMarker.Width = 10
  '        LineaTmp.PointMarker.StrokeThickness = 1
  '        If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
  '            If Pavarot.IsStbd Then
  '                LineaTmp.PointMarker.Stroke = Colors.Green
  '                LineaTmp.PointMarker.Fill = Colors.Green
  '            Else
  '                LineaTmp.PointMarker.Stroke = Colors.Red
  '                LineaTmp.PointMarker.Fill = Colors.Red
  '            End If
  '        Else
  '            LineaTmp.PointMarker.Stroke = Pavarot.Colore
  '            LineaTmp.PointMarker.Fill = Pavarot.Colore
  '        End If
  '        LineaTmp.Tag = Pavarot

  '        Dim X As Double = Pavarot.TwsDetails.AvgVal
  '        If Grafico = clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta Then
  '            X = YfromType(Pavarot, clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta)
  '        End If
  '        Dim Y As Double = YfromType(Pavarot)


  '        DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))

  '        LineaTmp.DataSeries = DataSeriesTMP
  '        Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
  '        SeriesSource.Add(CSVMtmp)
  '    Next
  'End Sub

  Private Sub DrawXyPlot2021()

    Select Case ParentVM.OutputType
      Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
        DrawXyPlotGroupByKeys()
        Exit Sub
      Case UserControlPavarotViewModel.eOutputType.eGroupByTack
        DrawXyPlotGroupByTack()
        Exit Sub
    End Select
    For Each Pavarot In PeriodsManager.CollectionPavarot
      'cicla tra le pavarot checkate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For

      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"

      If Pavarot.IsStbd Then
        LineaTmp.PointMarker = New EllipsePointMarker
      Else
        LineaTmp.PointMarker = New SquarePointMarker
      End If
      LineaTmp.PointMarker.Height = 10
      LineaTmp.PointMarker.Width = 10
      LineaTmp.PointMarker.StrokeThickness = 1
      If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          LineaTmp.PointMarker.Stroke = Colors.Green
          LineaTmp.PointMarker.Fill = Colors.Green
        Else
          LineaTmp.PointMarker.Stroke = Colors.Red
          LineaTmp.PointMarker.Fill = Colors.Red
        End If
      Else
        LineaTmp.PointMarker.Stroke = Pavarot.Colore
        LineaTmp.PointMarker.Fill = Pavarot.Colore
      End If
      LineaTmp.Tag = Pavarot

      Dim X As Double = Pavarot.TwsDetails.AvgVal '  Pavarot.MedieStandardPavarot.TWSMedia
      Dim Y As Double = YfromType(Pavarot)
      If Grafico = clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta Then
        X = Y
        Y = YfromType(Pavarot, clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwdDelta)
      End If


      DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))

      LineaTmp.DataSeries = DataSeriesTMP
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
    Next


  End Sub

  Private Sub DrawXyPlotGroupByTack()

    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati
    Dim TwsPort As New List(Of Double)
    Dim TwsStbd As New List(Of Double)

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      'cicla tra le pavarot checkate
      If Pavarot.IsChecked Then
        'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
        If True Then
          'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot

          Dim X As Double = Pavarot.TwsDetails.AvgVal ' Pavarot.MedieStandardPavarot.TWSMedia
          Dim Y As Double = YfromType(Pavarot)
          If Grafico = clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta Then
            X = Y
            Y = YfromType(Pavarot, clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwdDelta)
          End If
          ' YfromType(Pavarot)

          If Pavarot.IsStbd Then
            ValoriStbd.AggiungiCoppia(0, Y)
            TwsStbd.Add(X)
          Else
            ValoriPort.AggiungiCoppia(0, Y)
            TwsPort.Add(X)
          End If
        End If
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    'DataSeriesPort.AcceptsUnsortedData = True
    Dim LineaPort As New XyScatterRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.PointMarker = New EllipsePointMarker()
    LineaPort.PointMarker.Stroke = Colors.Red
    LineaPort.PointMarker.Height = 10
    LineaPort.PointMarker.Width = 10
    LineaPort.PointMarker.StrokeThickness = 1
    LineaPort.PointMarker.Fill = Colors.Red
    LineaPort.Tag = -1
    For Each Valore In ValoriPort.Dizionario
      If Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Count > 0 Then
        Dim Tws As Double = TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average
        Dim Val As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
        DataSeriesPort.Append(Tws, Val, New clsPuntoMetadata(False))
      End If
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    SeriesSource.Add(CSVMport)


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    'DataSeriesStbd.AcceptsUnsortedData = True
    Dim LineaStbd As New XyScatterRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker()
    LineaStbd.PointMarker.Stroke = Colors.Green
    LineaStbd.PointMarker.Height = 10
    LineaStbd.PointMarker.Width = 10
    LineaStbd.PointMarker.StrokeThickness = 1
    LineaStbd.PointMarker.Fill = Colors.Green
    LineaStbd.Tag = 1
    For Each Valore In ValoriStbd.Dizionario
      If Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Count > 0 Then
        Dim Tws As Double = TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average
        Dim Val As Double = Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average
        DataSeriesStbd.Append(Tws, Val, New clsPuntoMetadata(False))
      End If
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    SeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")


  End Sub

  Private Sub DrawXyPlotGroupByKeys()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      'cicla tra le pavarot checkate
      If Pavarot.IsChecked Then
        'Dim Period2021 As clsPeriod2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
        If True Then
          'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
          Dim X As Double = Pavarot.TwsDetails.AvgVal ' Pavarot.MedieStandardPavarot.TWSMedia
          Dim Y As Double = YfromType(Pavarot) 'YfromType(Pavarot)
          If Grafico = clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta Then
            X = Y
            Y = YfromType(Pavarot, clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwdDelta)
          End If
          Dim Chiave As String = Pavarot.Keys
          If Not Chiave.Trim = "" Then
            If Not GrId.ContainsKey(Chiave) Then
              GrId.Add(Chiave, contatore)
              contatore += 1
            End If

            Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
            If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey Then
              If Pavarot.IsStbd Then
                Chiave = "Stbd " & Chiave
                Mure = clsGruppoPeriodi.eMure.eStbd
              Else
                Chiave = "Port " & Chiave
                Mure = clsGruppoPeriodi.eMure.ePort
              End If
            End If

            If Gruppi.ContainsKey(Chiave) Then
              Gruppi(Chiave).AggiungiCoppia(X, Y)
            Else
              Gruppi.Add(Chiave, New clsGruppoPeriodi(X, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
            End If
          End If
        End If
      End If
    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      DataSeries.AcceptsUnsortedData = True
      Dim Linea As New XyScatterRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      If Gruppo.Value.Mure = clsGruppoPeriodi.eMure.ePort Then
        Linea.PointMarker = New SquarePointMarker
      Else
        Linea.PointMarker = New EllipsePointMarker()
      End If
      Linea.PointMarker.Stroke = Gruppo.Value.Colore '  Colors.Red
      Linea.PointMarker.Height = 10
      Linea.PointMarker.Width = 10
      Linea.PointMarker.StrokeThickness = 1
      Linea.PointMarker.Fill = Gruppo.Value.Colore ' Colors.Red
      Linea.Tag = Gruppo ' .Key ' Gruppo.Key ' Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      If Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Count > 0 AndAlso Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Count > 0 Then
        Dim Tws As Double = Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Average
        Dim Val As Double = Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Average
        DataSeries.Append(Tws, Val, New clsPuntoMetadata(False))
        Linea.DataSeries = DataSeries
        Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
        SeriesSource.Add(CSVMport)
      End If
    Next

    'OnPropertyChanged("SeriesSource")


  End Sub

  'Private Function YfromType(Pavarot As clsPavarot2019) As Double
  '  Return 0
  'End Function

  Private Function YfromType(Pavarot As clsPeriod2021) As Double
    Return YfromType(Pavarot, Grafico)
    'Select Case Grafico
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021BottomSpeed
    '        Return Pavarot.PavarotDetails.BottomSpeed
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021CogDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaCog
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021CseDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaCse
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021HdgDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaHdg
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021RotPerc95
    '        Return Pavarot.PavarotDetails.RotPerc95
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwaDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaTwa
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwdDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaTwd
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta
    '        Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
    '        Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
    '        Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
    '        Dim TheoreticalShift As Double = PavarotTgtAngle - DeltaCse ' positivo = lift
    '        Return TheoreticalShift
    '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwsDelta
    '        Return Pavarot.PavarotDetails.EntryExitDeltaTws
    '    Case Else
    '        Return Double.NaN
    'End Select
  End Function

  Private Function YfromType(Pavarot As clsPeriod2021, Type As clsPavarotAdvancedPlotContainerViewModel.eGraphics) As Double
    Select Case Type
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021BottomSpeed
        Return Pavarot.PavarotDetails.BottomSpeed
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2024TwaCogDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwaCog
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2024TwaCseDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwaCse
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2024TwaHdgDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwaHdg
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2024TwaTgtDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwaTgt
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021CogDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaCog
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021CseDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaCse
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021HdgDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaHdg
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021RotPerc95
        Return Pavarot.PavarotDetails.RotPerc95
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwaDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwa
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwdDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTwd
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBasicTwsGainLoss
        Return Pavarot.PavarotDetails.VmgTgtLossMt
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022CseVsTgtDelta
        Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
        Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
        Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
        Dim TheoreticalShift As Double = PavarotTgtAngle - DeltaCse ' positivo = lift
        Return TheoreticalShift
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2022TheoreticalUpwash
        'Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack, Pavarot.TwsDetails.AvgVal, "bs").Twa
        'Dim PavarotTgtAngle = Math.Min(TwaTgt, (180 - TwaTgt)) * 2
        Dim DeltaCse As Double = Pavarot.PavarotDetails.EntryExitDeltaCse
        Dim TheoreticalShift As Double = Pavarot.PavarotDetails.EntryExitDeltaTwa - DeltaCse ' positivo = lift
        Return TheoreticalShift
                'Dim TwdLift As Double = Pavarot.PavarotDetails.EntryExitDeltaTwd
                'Return TwdLift - TheoreticalShift
      Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.e2021TwsDelta
        Return Pavarot.PavarotDetails.EntryExitDeltaTws
      Case Else
        Return Double.NaN
    End Select
  End Function

  'Private Function YfromType(Pavarot As clsPavarot2019) As Double
  '  Dim y As Double = 0
  '  Select Case Grafico
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationStartTime
  '      y = Pavarot.PavarotData.PavarotPhases.RotationStartTime
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationDuration
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.TR.Durata.TotalSeconds
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationBottomSpeed
  '      'y = Pavarot.DettagliPavarot.PavarotDetails.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Bs).Min
  '      y = Pavarot.PavarotData.PavarotPhases.BottomSpeed
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationYawRateAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Avg
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationRdrAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Avg
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationRdrMax
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Max
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationLwyMax
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Leeway).Max
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationMinSinkAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RideHeight).Max
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationHeelAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Heel).Avg
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationHeelDs
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Heel).Ds
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationTrimAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Trim).Avg
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationTrimDs
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Trim).Ds
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationRdrRkAvg
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrRake).Avg
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eRotationRdrRkDs
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrRake).Ds


  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropStartTime
  '      y = Pavarot.PavarotData.PavarotPhases.BoardDropStartTime
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropCantAngle
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardDrop.NewCantRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropFlapIn
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardDrop.NewFlapInRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropFlapOut
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardDrop.NewFlapOutRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropHeel
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardDrop.FinalHeel
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardDropTrim
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardDrop.FinalTrim


  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpStartTime
  '      y = Pavarot.PavarotData.PavarotPhases.BoardUpStartTime
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpCantAngle
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardUp.NewCantRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpFlapIn
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardUp.NewFlapInRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpFlapOut
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardUp.NewFlapOutRealAngle
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpHeel
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardUp.FinalHeel
  '    Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eBoardUpTrim
  '      y = Pavarot.PavarotData.PavarotPhases.PavarotDetailBoardUp.FinalTrim

  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitBs
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg
  '      '  y = A - B
  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitTwa
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg
  '      '  y = A - B
  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitVmg
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Vmg).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Vmg).Avg
  '      '  y = A - B
  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitHeel
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg
  '      '  y = A - B
  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitTrim
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg
  '      '  y = A - B
  '      'Case clsPavarotAdvancedPlotContainerViewModel.eGraphics.eEntryExitRdrRake
  '      '  Dim A As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg
  '      '  Dim B As Double = Pavarot.PavarotData.PavarotPhases.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg
  '      '  y = A - B
  '    Case Else

  '  End Select
  '  Return y
  'End Function

  Private Sub DrawXyPlotTrack()
    'Exit Sub
    Select Case ParentVM.OutputType
      Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
        DrawXyPlotTrackGroupByKeys()
        Exit Sub
      Case UserControlPavarotViewModel.eOutputType.eGroupByTack
        DrawXyPlotTrackGroupByTack()
        Exit Sub
    End Select
    Dim CanaleLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim CanaleLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      'Dim CentroPavarot As New clsCourseMark(CanaleLat.Valori(Pavarot.IdRigaKeyMoment), CanaleLng.Valori(Pavarot.IdRigaKeyMoment), "Centro", clsCourseMark.eSideToBeLeft.eToMark)
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          LineaTmp.Stroke = Colors.Green
        Else
          LineaTmp.Stroke = Colors.Red
        End If
      Else
        LineaTmp.Stroke = Pavarot.Colore
      End If
      LineaTmp.StrokeThickness = 2
      LineaTmp.Tag = Pavarot

      Dim km = Pavarot.MomentoGpsHegHeadToWind
      Dim pr As clsGeographicPosition = Pavarot.PavarotDetails.PosizioneAtTime(km) ' Pavarot.KeyMoment
      For ii As Integer = Pavarot.TimeRangeStandardPavarot.IdRigaIniziale To Pavarot.TimeRangeStandardPavarot.IdRigaFinale
        Dim RP As clsDoubleXY = Pavarot.PavarotDetails.PosizioneRelativa_XY(ii, pr, Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe, Pavarot.IsStbd)
        DataSeriesTMP.Append(RP.X, RP.Y, New clsPuntoMetadata(False))
      Next

      LineaTmp.DataSeries = DataSeriesTMP
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
    Next

    ImpostaAnnotazioniTrack2019()

  End Sub

  Private Sub DrawXyPlotTrackGroupByTack()
    Dim CanaleLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim CanaleLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)


    Dim ValoriYPort As New clsValoriAggregati
    Dim ValoriYStbd As New clsValoriAggregati
    Dim ValoriXPort As New clsValoriAggregati
    Dim ValoriXStbd As New clsValoriAggregati

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        Dim CentroPavarot As New clsCourseMark(CanaleLat.Valori(Pavarot.IdRigaKeyMoment), CanaleLng.Valori(Pavarot.IdRigaKeyMoment), "Centro", clsCourseMark.eSideToBeLeft.eToMark)
        Dim Contatore As Integer = 0

        Dim pr As clsGeographicPosition = Pavarot.PavarotDetails.PosizioneAtTime(Pavarot.KeyMoment)
        For ii As Integer = Pavarot.TimeRangeStandardPavarot.IdRigaIniziale To Pavarot.TimeRangeStandardPavarot.IdRigaFinale
          Dim RP As clsDoubleXY = Pavarot.PavarotDetails.PosizioneRelativa_XY(ii, pr, Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe, Pavarot.IsStbd)
          If Pavarot.IsStbd Then
            ValoriYStbd.AggiungiCoppia(Contatore, RP.Y)
            ValoriXStbd.AggiungiCoppia(Contatore, RP.X)
          Else
            ValoriYPort.AggiungiCoppia(Contatore, RP.Y)
            ValoriXPort.AggiungiCoppia(Contatore, RP.X)
          End If
          Contatore += 1
        Next

        'For pt As Integer = 0 To Period2021.PavarotDetails.LatLonXY.XY_OverGround.GetLength(0) - 1
        '  Dim X As Double = Period2021.PavarotDetails.LatLonXY.XY_OverGround(pt, 0)
        '  Dim Y As Double = Period2021.PavarotDetails.LatLonXY.XY_OverGround(pt, 1)
        '  If Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack Then
        '    Y *= -1
        '  End If
        '  If Not Pavarot.IsStbd Then
        '    X *= -1
        '  End If
        '  If Pavarot.IsStbd Then
        '    ValoriYStbd.AggiungiCoppia(Contatore, Y)
        '    ValoriXStbd.AggiungiCoppia(Contatore, X)
        '  Else
        '    ValoriYPort.AggiungiCoppia(Contatore, Y)
        '    ValoriXPort.AggiungiCoppia(Contatore, X)
        '  End If
        '  Contatore += 1
        'Next
        'For Each Punto In Pavarot.PavarotData.ListaPuntiVmgVmbDaKeyMoment
        '  If Pavarot.IsStbd Then
        '    ValoriYStbd.AggiungiCoppia(Contatore, Punto.PuntoCartesianoDaRef.ToPointF.Y)
        '    ValoriXStbd.AggiungiCoppia(Contatore, Punto.PuntoCartesianoDaRef.ToPointF.X)
        '  Else
        '    ValoriYPort.AggiungiCoppia(Contatore, Punto.PuntoCartesianoDaRef.ToPointF.Y)
        '    ValoriXPort.AggiungiCoppia(Contatore, Punto.PuntoCartesianoDaRef.ToPointF.X)
        '  End If
        '  Contatore += 1
        'Next
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    DataSeriesPort.AcceptsUnsortedData = True
    Dim LineaPort As New FastLineRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.Stroke = Colors.Red
    LineaPort.StrokeThickness = 2
    LineaPort.Tag = -1

    'For Each v In ValoriYPort.Dizionario.OrderBy(Function(x) x.Key)
    '  DataSeriesPort.Append(v.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, v.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    'Next

    For i As Integer = 0 To ValoriYPort.Dizionario.Count - 1
      DataSeriesPort.Append(ValoriXPort.Dizionario(i).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, ValoriYPort.Dizionario(i).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    SeriesSource.Add(CSVMport)


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    DataSeriesStbd.AcceptsUnsortedData = True
    Dim LineaStbd As New FastLineRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.Stroke = Colors.Green
    LineaStbd.StrokeThickness = 2
    LineaStbd.Tag = 1

    'For Each v In ValoriYStbd.Dizionario.OrderBy(Function(x) x.Key)
    '  DataSeriesStbd.Append(v.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, v.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    'Next
    For i As Integer = 0 To ValoriYStbd.Dizionario.Count - 1
      DataSeriesStbd.Append(ValoriXStbd.Dizionario(i).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, ValoriYStbd.Dizionario(i).ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next

    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    SeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")


    ImpostaAnnotazioniTrack2019()

  End Sub

  Private Sub DrawXyPlotTrackGroupByKeys()
    Dim GruppiX As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GruppiY As New Dictionary(Of String, clsGruppoPeriodi)

    Dim GrId As New Dictionary(Of String, Integer)
    Dim Contatore As Integer = 0

    Dim CanaleLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim CanaleLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)

    For Each Pavarot In PeriodsManager.CollectionPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      'cicla tra le pavarot checkate
      If Pavarot.IsChecked Then
        Dim Chiave As String = Pavarot.Keys
        If Not Chiave.Trim = "" Then
          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, Contatore)
            Contatore += 1
          End If

          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey Then
            If Pavarot.IsStbd Then
              Chiave = "Stbd " & Chiave
              Mure = clsGruppoPeriodi.eMure.eStbd
            Else
              Chiave = "Port " & Chiave
              Mure = clsGruppoPeriodi.eMure.ePort
            End If
          End If

          Dim CentroPavarot As New clsCourseMark(CanaleLat.Valori(Pavarot.IdRigaKeyMoment), CanaleLng.Valori(Pavarot.IdRigaKeyMoment), "Centro", clsCourseMark.eSideToBeLeft.eToMark)
          Dim cnt As Integer = 0

          'Dim Period2021 = DataProvider2020.PeriodsManager2021.Periods.TrovaPeriodo2021(PeriodsManager.ListaPavarot(i).TimeRange)
          Dim pr As clsGeographicPosition = Pavarot.PavarotDetails.PosizioneAtTime(Pavarot.KeyMoment)
          For ii As Integer = Pavarot.TimeRangeStandardPavarot.IdRigaIniziale To Pavarot.TimeRangeStandardPavarot.IdRigaFinale
            Dim RP As clsDoubleXY = Pavarot.PavarotDetails.PosizioneRelativa_XY(ii, pr, Pavarot.PeriodType = clsPeriod2021.ePeriodType.eGybe, Pavarot.IsStbd)
            If GruppiX.ContainsKey(Chiave) Then
              GruppiX(Chiave).AggiungiCoppia(cnt, RP.X)
            Else
              GruppiX.Add(Chiave, New clsGruppoPeriodi(cnt, RP.X, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
            End If
            If GruppiY.ContainsKey(Chiave) Then
              GruppiY(Chiave).AggiungiCoppia(cnt, RP.Y)
            Else
              GruppiY.Add(Chiave, New clsGruppoPeriodi(cnt, RP.Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
            End If
            cnt += 1
          Next
          'For pt As Integer = 0 To Period2021.PavarotDetails.LatLonXY.XY_OverGround.GetLength(0) - 1
          '  Dim X As Double = Period2021.PavarotDetails.LatLonXY.XY_OverGround(pt, 0)
          '  Dim Y As Double = Period2021.PavarotDetails.LatLonXY.XY_OverGround(pt, 1)
          '  If Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack Then
          '    Y *= -1
          '  End If
          '  If Not Pavarot.IsStbd Then
          '    X *= -1
          '  End If

          '  If GruppiX.ContainsKey(Chiave) Then
          '    GruppiX(Chiave).AggiungiCoppia(cnt, X)
          '  Else
          '    GruppiX.Add(Chiave, New clsGruppoPeriodi(cnt, X, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          '  End If
          '  If GruppiY.ContainsKey(Chiave) Then
          '    GruppiY(Chiave).AggiungiCoppia(cnt, Y)
          '  Else
          '    GruppiY.Add(Chiave, New clsGruppoPeriodi(cnt, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          '  End If
          '  cnt += 1
          'Next


          'For Each Punto In Pavarot.PavarotData.ListaPuntiVmgVmbDaKeyMoment
          '  Dim X As Double = Punto.PuntoCartesianoDaRef.ToPointF.X
          '  Dim Y As Double = Punto.PuntoCartesianoDaRef.ToPointF.Y
          '  If Not Pavarot.IsStbd Then
          '    X *= 1
          '  End If
          '  If GruppiX.ContainsKey(Chiave) Then
          '    GruppiX(Chiave).AggiungiCoppia(cnt, X)
          '  Else
          '    GruppiX.Add(Chiave, New clsGruppoPeriodi(cnt, X, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          '  End If
          '  If GruppiY.ContainsKey(Chiave) Then
          '    GruppiY(Chiave).AggiungiCoppia(cnt, Y)
          '  Else
          '    GruppiY.Add(Chiave, New clsGruppoPeriodi(cnt, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          '  End If
          '  cnt += 1
          'Next

        End If
      End If
    Next


    For Each GruppoX In GruppiX
      Dim GruppoY As clsGruppoPeriodi = GruppiY(GruppoX.Key)
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      DataSeries.AcceptsUnsortedData = True
      Dim Linea As New FastLineRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Stroke = GruppoX.Value.Colore '  Colors.Red
      Linea.StrokeThickness = 1
      Linea.Tag = GruppoX.Value.IdGruppo + GruppoX.Value.Mure / 10
      For i As Integer = 0 To GruppoX.Value.ValoriAggregati.Dizionario.Count - 1
        Dim vX As Double = GruppoX.Value.ValoriAggregati.Dizionario.Values(i).ValoriY.Average
        Dim vY As Double = GruppoY.ValoriAggregati.Dizionario.Values(i).ValoriY.Average
        DataSeries.Append(vX, vY, New clsPuntoMetadata(False))
      Next
      Linea.DataSeries = DataSeries
      Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVMport)
    Next

    'OnPropertyChanged("SeriesSource")



  End Sub


  Private Sub DrawPlotPavarotBottomSpeed()
    Select Case ParentVM.OutputType
      Case UserControlPavarotViewModel.eOutputType.eGroupByKey, UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey
        DrawPlotPavarotBottomSpeedGroupByKeys()
        Exit Sub
      Case UserControlPavarotViewModel.eOutputType.eColorByTack, UserControlPavarotViewModel.eOutputType.eGroupByTack
        DrawPlotPavarotBottomSpeedGroupByTack()
        Exit Sub
    End Select
    'If ParentVM.PavarotStatusSync.GroupByKeys Then
    '  DrawPlotPavarotBottomSpeedGroupByKeys()
    '  Exit Sub
    'ElseIf ParentVM.PavarotStatusSync.GroupByTack Then
    '  DrawPlotPavarotBottomSpeedGroupByTack()
    '  Exit Sub
    'End If

    For Each Pavarot In PeriodsManager.CollectionPavarot
      'cicla tra le pavarot checkate
      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      Dim LineaTmp As New FastLineRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"

      If Pavarot.IsStbd Then
        LineaTmp.PointMarker = New EllipsePointMarker
      Else
        LineaTmp.PointMarker = New SquarePointMarker
      End If
      'LineaTmp.PointMarker = New EllipsePointMarker
      LineaTmp.PointMarker.Height = 10
      LineaTmp.PointMarker.Width = 10
      LineaTmp.PointMarker.StrokeThickness = 1
      If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eColorByTack Then
        If Pavarot.IsStbd Then
          LineaTmp.PointMarker.Stroke = Colors.Green
          LineaTmp.PointMarker.Fill = Colors.Green
        Else
          LineaTmp.PointMarker.Stroke = Colors.Red
          LineaTmp.PointMarker.Fill = Colors.Red
        End If
      Else
        LineaTmp.PointMarker.Stroke = Pavarot.Colore
        LineaTmp.PointMarker.Fill = Pavarot.Colore
      End If
      LineaTmp.Tag = Pavarot

      Dim X As Double = Pavarot.TwsDetails.AvgVal
      Dim Y As Double = Pavarot.PavarotDetails.BottomSpeed
      DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))


      LineaTmp.DataSeries = DataSeriesTMP
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
    Next

    ImpostaAnnotazioniBottomSpeed()

  End Sub

  Private Sub DrawPlotPavarotBottomSpeedGroupByTack()


    Dim ValoriPort As New clsValoriAggregati
    Dim ValoriStbd As New clsValoriAggregati
    Dim TwsPort As New List(Of Double)
    Dim TwsStbd As New List(Of Double)

    For Each Pavarot In PeriodsManager.CollectionPavarot
      'cicla tra le pavarot checkate
      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        Dim X As Double = Pavarot.TwsDetails.AvgVal
        Dim Y As Double = Pavarot.PavarotDetails.BottomSpeed
        If Pavarot.IsStbd Then
          ValoriStbd.AggiungiCoppia(0, Y)
          TwsStbd.Add(X)
        Else
          ValoriPort.AggiungiCoppia(0, Y)
          TwsPort.Add(X)
        End If
      End If
    Next

    'Dim astbd As Double = TwsStbd.Average
    'Dim aport As Double = TwsPort.Average

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    'DataSeriesPort.AcceptsUnsortedData = True
    Dim LineaPort As New XyScatterRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.PointMarker = New EllipsePointMarker()
    LineaPort.PointMarker.Stroke = Colors.Red
    LineaPort.PointMarker.Height = 10
    LineaPort.PointMarker.Width = 10
    LineaPort.PointMarker.StrokeThickness = 1
    LineaPort.PointMarker.Fill = Colors.Red
    LineaPort.Tag = -1
    For Each Valore In ValoriPort.Dizionario
      DataSeriesPort.Append(TwsPort.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next
    LineaPort.DataSeries = DataSeriesPort
    Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
    SeriesSource.Add(CSVMport)


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    'DataSeriesStbd.AcceptsUnsortedData = True
    Dim LineaStbd As New XyScatterRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker()
    LineaStbd.PointMarker.Stroke = Colors.Green
    LineaStbd.PointMarker.Height = 10
    LineaStbd.PointMarker.Width = 10
    LineaStbd.PointMarker.StrokeThickness = 1
    LineaStbd.PointMarker.Fill = Colors.Green
    LineaStbd.Tag = 1
    For Each Valore In ValoriStbd.Dizionario
      DataSeriesStbd.Append(TwsStbd.Where(Function(x) Not Double.IsNaN(x)).Average, Valore.Value.ValoriY.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    SeriesSource.Add(CSVMStbd)

    'OnPropertyChanged("SeriesSource")

    ImpostaAnnotazioniBottomSpeed()

  End Sub


  Private Sub DrawPlotPavarotBottomSpeedGroupByKeys()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    For Each Pavarot In PeriodsManager.CollectionPavarot
      'cicla tra le pavarot checkate
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For
      If Pavarot.IsChecked Then
        'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
        Dim X As Double = Pavarot.TwsDetails.AvgVal
        Dim Y As Double = Pavarot.PavarotDetails.BottomSpeed
        Dim Chiave As String = Pavarot.Keys
        If Not Chiave.Trim = "" Then
          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, contatore)
            contatore += 1
          End If

          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If ParentVM.OutputType = UserControlPavarotViewModel.eOutputType.eGroupByTackAndKey Then
            If Pavarot.IsStbd Then
              Chiave = "Stbd " & Chiave
              Mure = clsGruppoPeriodi.eMure.eStbd
            Else
              Chiave = "Port " & Chiave
              Mure = clsGruppoPeriodi.eMure.ePort
            End If
          End If

          If Gruppi.ContainsKey(Chiave) Then
            Gruppi(Chiave).AggiungiCoppia(X, Y)
          Else
            Gruppi.Add(Chiave, New clsGruppoPeriodi(X, Y, Mure, GrId(Pavarot.Keys), Chiave, ColoreDaOutputType(Pavarot, clsStraightLineVM2020.eOutputType.eColorByKey)))
          End If
        End If
      End If
    Next


    For Each Gruppo In Gruppi
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      DataSeries.AcceptsUnsortedData = True
      Dim Linea As New XyScatterRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      If Gruppo.Value.Mure = clsGruppoPeriodi.eMure.ePort Then
        Linea.PointMarker = New SquarePointMarker
      Else
        Linea.PointMarker = New EllipsePointMarker()
      End If
      Linea.PointMarker.Stroke = Gruppo.Value.Colore '  Colors.Red
      Linea.PointMarker.Height = 10
      Linea.PointMarker.Width = 10
      Linea.PointMarker.StrokeThickness = 1
      Linea.PointMarker.Fill = Gruppo.Value.Colore ' Colors.Red
      Linea.Tag = Gruppo ' .Key ' Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
      DataSeries.Append(Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Average, Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Average, New clsPuntoMetadata(False))
      Linea.DataSeries = DataSeries
      Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVMport)
    Next

    'OnPropertyChanged("SeriesSource")

    ImpostaAnnotazioniBottomSpeed()


  End Sub


  Public Enum eOutputDelta
    eSeconds = 0
    eInline = 1
    eVmg = 2
  End Enum
  Private Sub DisegnaBenchmarks(MinTws As Double, MaxTws As Double, AtLeastOnetack As Boolean, AtLeastOneGybe As Boolean)

    If BenchManager Is Nothing Then Exit Sub
    'If ManBenchManager.Lista.Count = 0 Then Exit Sub

    ' traccia le linee target
    MaxTws = System.Math.Floor(MaxTws) + 1
    MinTws = System.Math.Floor(MinTws)

    Dim a(1) As Double
    a(0) = 1
    a(1) = 3

    Dim DataSeriesUp As New XyDataSeries(Of Double, Double)
    DataSeriesUp.AcceptsUnsortedData = False
    Dim LineaUp As New FastLineRenderableSeries
    LineaUp.XAxisId = "DefaultAxisId"
    LineaUp.YAxisId = "DefaultAxisId"
    LineaUp.Stroke = Colors.DarkRed
    LineaUp.StrokeThickness = 4
    LineaUp.StrokeDashArray = a
    LineaUp.Tag = "BenchUp"

    Dim DataSeriesDn As New XyDataSeries(Of Double, Double)
    DataSeriesDn.AcceptsUnsortedData = False
    Dim LineaDn As New FastLineRenderableSeries
    LineaDn.XAxisId = "DefaultAxisId"
    LineaDn.YAxisId = "DefaultAxisId"
    LineaDn.Stroke = Colors.DarkBlue
    LineaDn.StrokeThickness = 4
    LineaDn.StrokeDashArray = a
    LineaDn.Tag = "BenchDn"


    Dim b = BenchManager.ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eTackLoss)
    If Not b Is Nothing Then
      Dim elementi = b.First.Values.OrderBy(Function(x) x.X).ToList
      For Each elemento In elementi
        If elemento.X >= MinTws And elemento.X <= MaxTws Then
          DataSeriesUp.Append(elemento.X, elemento.Y)
        End If
      Next
    End If

    b = BenchManager.ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eGybeLoss)
    If Not b Is Nothing Then
      Dim elementi = b.First.Values.OrderBy(Function(x) x.X).ToList
      For Each elemento In elementi
        If elemento.X >= MinTws And elemento.X <= MaxTws Then
          DataSeriesDn.Append(elemento.X, elemento.Y)
        End If
      Next
    End If


    'For Each elemento In BenchManager.Benchmarks.TacksLoss.Values.OrderBy(Function(x) x.X).ToList
    '  If elemento.X >= MinTws And elemento.X <= MaxTws Then
    '    DataSeriesUp.Append(elemento.X, elemento.Y)
    '  End If
    'Next
    'For Each elemento In BenchManager.Benchmarks.GybesLoss.Values.OrderBy(Function(x) x.X).ToList
    '  If elemento.X >= MinTws And elemento.X <= MaxTws Then
    '    DataSeriesUp.Append(elemento.X, elemento.Y)
    '  End If
    'Next

    SeriesSource.Add(New ChartSeriesViewModel(DataSeriesUp, LineaUp))
    SeriesSource.Add(New ChartSeriesViewModel(DataSeriesDn, LineaDn))

    LineaUp.IsVisible = ParentVM.PlotTargetsIfAvailable AndAlso AtLeastOnetack
    LineaDn.IsVisible = ParentVM.PlotTargetsIfAvailable AndAlso AtLeastOneGybe



  End Sub



  Private Sub DrawPlotAvgVmgVsTws()
    Dim MaxTws As Double = 0
    Dim MinTws As Double = 999
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False

    For Each Pavarot In PeriodsManager.CollectionPavarot
      'Dim Pavarot As clsPavarot2019 = PeriodsManager.ListaPavarot(i).DettagliPavarot
      Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Start)
      Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(Pavarot.TimeRangeStandardPavarot.Finish)
      If RigaIniziale = RigaFinale Then Continue For

      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      Dim LineaTmp As New XyScatterRenderableSeries
      LineaTmp.XAxisId = "DefaultAxisId"
      LineaTmp.YAxisId = "DefaultAxisId"
      AtLeastOneTack = AtLeastOneTack OrElse Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack
      AtLeastOneGybe = AtLeastOneGybe OrElse Not Pavarot.PeriodType = clsPeriod2021.ePeriodType.eTack

      If Pavarot.IsStbd Then
        LineaTmp.PointMarker = New EllipsePointMarker
      Else
        LineaTmp.PointMarker = New SquarePointMarker
      End If
      'LineaTmp.PointMarker = New EllipsePointMarker()
      LineaTmp.PointMarker.Stroke = Pavarot.Colore
      LineaTmp.PointMarker.Height = 10
      LineaTmp.PointMarker.Width = 10
      LineaTmp.PointMarker.StrokeThickness = 1
      LineaTmp.PointMarker.Fill = Pavarot.Colore
      LineaTmp.Tag = Pavarot

      DataSeriesTMP.AcceptsUnsortedData = True

      Dim X As Double = Pavarot.TwsDetails.AvgVal
      MaxTws = System.Math.Max(X, MaxTws)
      MinTws = System.Math.Min(X, MinTws)
      Dim Y As Double = Pavarot.PavarotDetails.VmgAvg  ' VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
      DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))

      LineaTmp.DataSeries = DataSeriesTMP
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
      SeriesSource.Add(CSVMtmp)
    Next

    If TgtManager.Tgt Is Nothing AndAlso TgtManager.Tgt.PolareDisponibile("bs") Then
      ' traccia le linee target
      MaxTws = System.Math.Floor(MaxTws) + 1
      MinTws = System.Math.Floor(MinTws)

      Dim a(1) As Double
      a(0) = 1
      a(1) = 3

      Dim DataSeriesUp As New XyDataSeries(Of Double, Double)
      DataSeriesUp.AcceptsUnsortedData = False
      Dim LineaUp As New FastLineRenderableSeries
      LineaUp.XAxisId = "DefaultAxisId"
      LineaUp.YAxisId = "DefaultAxisId"
      LineaUp.Stroke = Colors.DarkRed
      LineaUp.StrokeThickness = 4
      LineaUp.StrokeDashArray = a
      LineaUp.Tag = 1

      Dim DataSeriesDn As New XyDataSeries(Of Double, Double)
      DataSeriesDn.AcceptsUnsortedData = False
      Dim LineaDn As New FastLineRenderableSeries
      LineaDn.XAxisId = "DefaultAxisId"
      LineaDn.YAxisId = "DefaultAxisId"
      LineaDn.Stroke = Colors.DarkBlue
      LineaDn.StrokeThickness = 4
      LineaDn.StrokeDashArray = a
      LineaDn.Tag = 2


      For i As Integer = MinTws To MaxTws Step 1
        Dim v = TgtManager.Tgt.ValoreTgtUp(i, "bs")
        If v Is Nothing Then
          DataSeriesUp.Append(i, 0)
        Else
          DataSeriesUp.Append(i, v.Vmg) ' Bs * System.Math.Abs(System.Math.Cos(Radians(Twa))))
        End If
        v = TgtManager.Tgt.ValoreTgtDn(i, "bs")
        If v Is Nothing Then
          DataSeriesDn.Append(i, 0)
        Else
          DataSeriesDn.Append(i, v.Vmg) 'Bs * System.Math.Abs(System.Math.Cos(Radians(Twa))))
        End If

      Next

      SeriesSource.Add(New ChartSeriesViewModel(DataSeriesUp, LineaUp))
      SeriesSource.Add(New ChartSeriesViewModel(DataSeriesDn, LineaDn))

      LineaUp.IsVisible = AtLeastOneTack
      LineaDn.IsVisible = AtLeastOneGybe


    End If


    ImpostaAnnotazioniGainLossVsTws()

  End Sub

  Private Sub ImpostaAnnotazioni2022CseVsTgtDelta()
    'Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.LineAnnotation
    AnV.X1 = -30
    AnV.X2 = 30
    AnV.Y1 = -30
    AnV.Y2 = 30
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)
  End Sub
  Private Sub ImpostaAnnotazioniTrack2019()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)


    Dim AnWA As New SciChart.Charting.Visuals.Annotations.LineArrowAnnotation
    AnWA.X1 = 0
    AnWA.X2 = 0
    AnWA.Y1 = 0.3
    AnWA.Y2 = 0.1
    Dim MC As New Windows.Media.Color
    MC.A = 150
    MC.R = 0
    MC.G = 0
    MC.B = 139
    AnWA.Stroke = New SolidColorBrush(MC)
    AnWA.StrokeThickness = 10
    AnWA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnWA.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWA)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0.3
    AnWT.Text = "Boat Direction"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 1
    AnWT.Y1 = 0.85
    AnWT.Text = "Tracks"
    AnWT.FontSize = 20
    MC = New Windows.Media.Color
    MC.A = 68
    MC.R = 0
    MC.G = 0
    MC.B = 0
    AnWT.Foreground = New SolidColorBrush(MC)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.8
    AnWT.Y1 = 0.0
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    Dim VBR As New Binding("ParentVM.PavarotSyncViewModel.CurrentXposition")
    VBR.Source = Me
    VBR.Mode = BindingMode.OneWay
    AnWT.SetBinding(SciChart.Charting.Visuals.Annotations.TextAnnotation.TextProperty, VBR)



    If Not PeriodsManager.CollectionPavarot Is Nothing Then
      For Each pavarot In PeriodsManager.CollectionPavarot
        Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(pavarot.TimeRangeStandardPavarot.Start)
        Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(pavarot.TimeRangeStandardPavarot.Finish)
        If RigaIniziale = RigaFinale Then Continue For
        Dim AnBA = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
        AnBA.Background = New SolidColorBrush(pavarot.Colore) 'Color.FromArgb(255, 255, 112, 52))
        AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
        AnBA.IsHidden = False
        AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
        AnBA.CornerRadius = New CornerRadius(5)
        Annotazioni.Add(AnBA)


        VBR = New Binding("ParentVM.PavarotSyncViewModel.Track2019X1[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.Track2019X2[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.Track2019Y1[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y1Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.Track2019Y2[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y2Property, VBR)


      Next
    End If
  End Sub

  Private Sub ImpostaAnnotazioniTrackFromEntry2019()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)


    Dim AnWA As New SciChart.Charting.Visuals.Annotations.LineArrowAnnotation
    AnWA.X1 = 0
    AnWA.X2 = 0
    AnWA.Y1 = 0.3
    AnWA.Y2 = 0.1
    Dim MC As New Windows.Media.Color
    MC.A = 150
    MC.R = 0
    MC.G = 0
    MC.B = 139
    AnWA.Stroke = New SolidColorBrush(MC)
    AnWA.StrokeThickness = 10
    AnWA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnWA.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWA)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0.3
    AnWT.Text = "Boat Direction"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 1
    AnWT.Y1 = 0.85
    AnWT.Text = "Tracks"
    AnWT.FontSize = 20
    MC = New Windows.Media.Color
    MC.A = 68
    MC.R = 0
    MC.G = 0
    MC.B = 0
    AnWT.Foreground = New SolidColorBrush(MC)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.8
    AnWT.Y1 = 0.0
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    Dim VBR As New Binding("ParentVM.PavarotSyncViewModel.CurrentXposition")
    VBR.Source = Me
    VBR.Mode = BindingMode.OneWay
    AnWT.SetBinding(SciChart.Charting.Visuals.Annotations.TextAnnotation.TextProperty, VBR)

    If Not PeriodsManager.CollectionPavarot Is Nothing Then
      For Each pavarot In PeriodsManager.CollectionPavarot
        Dim RigaIniziale As Integer = DataProvider2020.TrovaIndice(pavarot.TimeRangeStandardPavarot.Start)
        Dim RigaFinale As Integer = DataProvider2020.TrovaIndice(pavarot.TimeRangeStandardPavarot.Finish)
        If RigaIniziale = RigaFinale Then Continue For
        Dim AnBA = New SciChart.Charting.Visuals.Annotations.BoxAnnotation
        AnBA.Background = New SolidColorBrush(pavarot.Colore) 'Color.FromArgb(255, 255, 112, 52))
        AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
        AnBA.IsHidden = False
        AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
        AnBA.CornerRadius = New CornerRadius(5)
        Annotazioni.Add(AnBA)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.TrackFromEntry2019X1[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.TrackFromEntry2019X2[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X2Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.TrackFromEntry2019Y1[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y1Property, VBR)

        VBR = New Binding("ParentVM.PavarotSyncViewModel.TrackFromEntry2019Y2[" & PeriodsManager.PeriodId(pavarot) & "]")
        VBR.Source = Me
        VBR.Mode = BindingMode.TwoWay
        AnBA.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.Y2Property, VBR)


      Next
    End If
  End Sub

  Private Sub ImpostaAnnotazioniGainLoss()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Lateral Meters"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Vmg Meters"
    'AnWT.FontSize = 40
    'Dim MC As New Windows.Media.Color
    'MC.A = 68
    'MC.R = 0
    'MC.G = 0
    'MC.B = 0
    'AnWT.Foreground = New SolidColorBrush(MC)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniPavarotSchema()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "[m]"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Manoeuvres Schema"
    'AnWT.FontSize = 40
    'Dim MC As New Windows.Media.Color
    'MC.A = 68
    'MC.R = 0
    'MC.G = 0
    'MC.B = 0
    'AnWT.Foreground = New SolidColorBrush(MC)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniPavarotGlobalLossSeconds()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "TotalSecondsLoss (with invest.) [ss]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniPavarotLossManInvAndPrjSeconds()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "TotalVmgLoss (with invest. and proj.) [m]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniPavarotLossDeltas(IncludesInvestments As Boolean, IncludesExtraAcceleration As Boolean, OutputDelta As eOutputDelta)
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    Select Case OutputDelta
      Case eOutputDelta.eVmg
        AnWT.Text = "Vmg Loss XX [m]"
      Case eOutputDelta.eInline
        AnWT.Text = "InLine Loss XX [m]"
      Case eOutputDelta.eSeconds
        AnWT.Text = "Seconds Loss XX [s]"
    End Select
    Dim txtTmp As String = ""
    If IncludesInvestments Then
      txtTmp &= "Inv"
    End If
    If IncludesExtraAcceleration Then
      If Not txtTmp = "" Then txtTmp &= "&"
      txtTmp &= "XtrAcc"
    End If
    If Not txtTmp = "" Then txtTmp = " (" & txtTmp & ") "
    AnWT.Text = AnWT.Text.Replace("XX", txtTmp)
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniPavarotGlobalLossInline()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "TotalInlineLoss (with invest.) [m]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniGainLossVsTws()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.6
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 20
    AnWT.Text = "Vmg Kts"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniVmgTgVsTws()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "EntryExitLoss + Proj. [m]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniGainLossProgression2019()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Seconds"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "Gain/Loss From Entry [m]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniEntryStability()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Heel"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0.2
    AnWT.FontSize = 20
    AnWT.Text = "Trim"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniRateOfTurn()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "Rotation Time [ss]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniRideHeightStdDev()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "Ride Height Std Dev"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniCantDropMax()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "Cant Drop Max Speed"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub

  Private Sub ImpostaAnnotazioniBottomSpeed()
    Annotazioni.Clear()
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)

    Dim AnV As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnV.X1 = 0
    AnV.X2 = 0
    AnV.Y1 = 0
    AnV.Y2 = 1
    AnV.Stroke = New SolidColorBrush(Colors.Black)
    AnV.StrokeDashArray = a
    AnV.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
    AnV.IsHidden = False
    AnV.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnV)

    Dim AnH As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    AnH.X1 = 0
    AnH.X2 = 1
    AnH.Y1 = 0
    AnH.Y2 = 0
    AnH.Stroke = New SolidColorBrush(Colors.Black)
    AnH.StrokeDashArray = a
    AnH.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeX
    AnH.IsHidden = False
    AnH.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnH)

    Dim AnWT As New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0.7
    AnWT.Y1 = 0.9
    AnWT.Text = "Tws"
    AnWT.FontSize = 20
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

    AnWT = New SciChart.Charting.Visuals.Annotations.TextAnnotation
    AnWT.X1 = 0
    AnWT.Y1 = 0
    AnWT.FontSize = 14
    AnWT.Text = "Bottom Speed [kts]"
    AnWT.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Relative
    AnWT.AnnotationCanvas = AnnotationCanvas.BelowChart
    Annotazioni.Add(AnWT)

  End Sub


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsPavarotAdvancedPlotContainerViewModel
  'questo view model gestisce l oggetto contenitore sia dei grafici Tws vs Channels che Periods Channels Distributions
  'Implements INotifyPropertyChanged

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property ListaControlli As New ObservableCollection(Of UserControlPavarotAdvancedPlot)
  Public Property PavarotSyncViewModel As New clsPavarotSyncViewModel
  Public Property GraficiDaStampare As New ObservableCollection(Of eGraphics)

  Public Property DataBoxHeight As Double
  Public Property Collection As eCollection

  Public Property ParentVM As UserControlPavarotViewModel

  Public Property Altezza As Double = 300

  Public Enum eCollection
    eBoadDrop = 0
    eBoardUp = 1
    eRotation = 2
    eEntryExit = 3
    eBasicAvg = 4
  End Enum

  Public Enum eGraphics
    eRotationStartTime = 0
    eRotationDuration = 1
    eRotationBottomSpeed = 2
    eRotationYawRateAvg = 3
    eRotationRdrAvg = 4
    eRotationRdrMax = 5
    eRotationLwyMax = 6
    eRotationMinSinkAvg = 7
    eRotationHeelAvg = 8
    eRotationHeelDs = 9
    eRotationTrimAvg = 10
    eRotationTrimDs = 11
    eRotationRdrRkAvg = 12
    eRotationRdrRkDs = 13

    eBoardDropStartTime = 20
    eBoardDropCantAngle = 21
    eBoardDropFlapIn = 22
    eBoardDropFlapOut = 23
    eBoardDropHeel = 24
    eBoardDropTrim = 25

    eBoardUpStartTime = 40
    eBoardUpCantAngle = 41
    eBoardUpFlapIn = 42
    eBoardUpFlapOut = 43
    eBoardUpHeel = 44
    eBoardUpTrim = 45

    eEntryExitBs = 60
    eEntryExitTwa = 61
    eEntryExitVmg = 62
    eEntryExitHeel = 63
    eEntryExitTrim = 64
    eEntryExitRdrRake = 65

    e2021TwdDelta = 70
    e2021TwsDelta = 71
    e2021TwaDelta = 72
    e2021CogDelta = 73
    e2021CseDelta = 74
    e2021HdgDelta = 75
    e2021BottomSpeed = 76
    e2021RotPerc95 = 77

    e2022CseVsTgtDelta = 78
    e2022TheoreticalUpwash = 79

    '    Public Property EntryExitDeltaHdg As Double
    'Public Property EntryExitDeltaCse As Double
    'Public Property EntryExitDeltaCog As Double



    eBasicTwsGainLoss = 80
    eBasicTrackFromKeyMoment = 81
    eBasicTrackFromEntry = 82
    eBasicPavarotSchema = 83
    eBasicBottomSpeed = 84
    eBasicRateOfTurn = 85
    eBasicEntryStability = 86
    eBasicPavarotManoeuverLoss = 87
    eBasicPavaroManoeverAndInvestmentLoss = 88
    eBasicPavarotManInvAndExtraAccLoss = 89
    eBasicRideHeightDs = 90

    eBasicCantDropMaxSpeed = 91
    eBasicCantRiseMaxSpeed = 92


    e2024TwaCseDelta = 93
    e2024TwaCogDelta = 94
    e2024TwaHdgDelta = 95
    e2024TwaTgtDelta = 96


  End Enum

  Public Sub New(objPavarotSyncViewModel As clsPavarotSyncViewModel, objParentVM As UserControlPavarotViewModel, Collection As eCollection)
    Me.PavarotSyncViewModel = objPavarotSyncViewModel
    Me.ParentVM = objParentVM
    Me.Collection = Collection
    ImpostaGrafici()
  End Sub

  Private Sub ImpostaGrafici()
    Select Case Collection
      Case eCollection.eBoadDrop
        GraficiDaStampare.Add(eGraphics.eBoardDropStartTime)
        GraficiDaStampare.Add(eGraphics.eBoardDropCantAngle)
        GraficiDaStampare.Add(eGraphics.eBoardDropFlapIn)
        GraficiDaStampare.Add(eGraphics.eBoardDropFlapOut)
        GraficiDaStampare.Add(eGraphics.eBoardDropHeel)
        GraficiDaStampare.Add(eGraphics.eBoardDropTrim)
      Case eCollection.eBoardUp
        GraficiDaStampare.Add(eGraphics.eBoardUpStartTime)
        GraficiDaStampare.Add(eGraphics.eBoardUpCantAngle)
        GraficiDaStampare.Add(eGraphics.eBoardUpFlapIn)
        GraficiDaStampare.Add(eGraphics.eBoardUpFlapOut)
        GraficiDaStampare.Add(eGraphics.eBoardUpHeel)
        GraficiDaStampare.Add(eGraphics.eBoardUpTrim)
      Case eCollection.eEntryExit
        GraficiDaStampare.Add(eGraphics.eEntryExitBs)
        GraficiDaStampare.Add(eGraphics.eEntryExitTwa)
        GraficiDaStampare.Add(eGraphics.eEntryExitVmg)
        GraficiDaStampare.Add(eGraphics.eEntryExitHeel)
        GraficiDaStampare.Add(eGraphics.eEntryExitTrim)
        GraficiDaStampare.Add(eGraphics.eEntryExitRdrRake)
      Case eCollection.eRotation
        GraficiDaStampare.Add(eGraphics.eRotationStartTime)
        GraficiDaStampare.Add(eGraphics.eRotationDuration)
        GraficiDaStampare.Add(eGraphics.eRotationBottomSpeed)
        GraficiDaStampare.Add(eGraphics.eRotationYawRateAvg)
        GraficiDaStampare.Add(eGraphics.eRotationRdrAvg)
        GraficiDaStampare.Add(eGraphics.eRotationRdrMax)
        GraficiDaStampare.Add(eGraphics.eRotationLwyMax)
        GraficiDaStampare.Add(eGraphics.eRotationMinSinkAvg)
        GraficiDaStampare.Add(eGraphics.eRotationHeelAvg)
        GraficiDaStampare.Add(eGraphics.eRotationHeelDs)
        GraficiDaStampare.Add(eGraphics.eRotationTrimAvg)
        GraficiDaStampare.Add(eGraphics.eRotationTrimDs)
        GraficiDaStampare.Add(eGraphics.eRotationRdrRkAvg)
        GraficiDaStampare.Add(eGraphics.eRotationRdrRkDs)
      Case eCollection.eBasicAvg
        'GraficiDaStampare.Add(eGraphics.eBasicPavarotManoeuverLoss)
        'GraficiDaStampare.Add(eGraphics.eBasicPavaroManoeverAndInvestmentLoss)
        'GraficiDaStampare.Add(eGraphics.eBasicPavarotManInvAndExtraAccLoss)
        'GraficiDaStampare.Add(eGraphics.eBasicTwsGainLoss)
        'GraficiDaStampare.Add(eGraphics.eBasicEntryStability)
        'GraficiDaStampare.Add(eGraphics.eBasicTrackFromKeyMoment)
        'GraficiDaStampare.Add(eGraphics.eBasicTrackFromEntry)
        GraficiDaStampare.Add(eGraphics.eBasicTwsGainLoss)
        GraficiDaStampare.Add(eGraphics.e2021BottomSpeed)
        GraficiDaStampare.Add(eGraphics.e2021RotPerc95)
        GraficiDaStampare.Add(eGraphics.e2021TwsDelta)
        GraficiDaStampare.Add(eGraphics.e2021TwdDelta)
        GraficiDaStampare.Add(eGraphics.e2024TwaTgtDelta)
        'GraficiDaStampare.Add(eGraphics.e2022TheoreticalUpwash)
        GraficiDaStampare.Add(eGraphics.e2024TwaCogDelta)
        GraficiDaStampare.Add(eGraphics.e2024TwaCseDelta)
        GraficiDaStampare.Add(eGraphics.e2024TwaHdgDelta)
        'GraficiDaStampare.Add(eGraphics.e2022CseVsTgtDelta)
        'GraficiDaStampare.Add(eGraphics.e2021TwaDelta)
        'GraficiDaStampare.Add(eGraphics.e2021CogDelta)
        'GraficiDaStampare.Add(eGraphics.e2021CseDelta)
        'GraficiDaStampare.Add(eGraphics.e2021HdgDelta)

        'GraficiDaStampare.Add(eGraphics.eBasicRideHeightDs)
        'GraficiDaStampare.Add(eGraphics.eBasicCantDropMaxSpeed)
        'GraficiDaStampare.Add(eGraphics.eBasicPavarotSchema)





    End Select
  End Sub


  'Public Property DataBoxHeight As Double
  '  Get
  '    Return pDataBoxHeight
  '  End Get
  '  Set(value As Double)
  '    If pDataBoxHeight = value Then Exit Property
  '    pDataBoxHeight = value
  '    OnPropertyChanged("DataBoxHeight")
  '  End Set
  'End Property

  'Public Property ListaControlli As ObservableCollection(Of UserControlPavarotAdvancedPlot)
  '  Get
  '    Return pListaControlli
  '  End Get
  '  Set(value As ObservableCollection(Of UserControlPavarotAdvancedPlot))
  '    pListaControlli = value
  '    OnPropertyChanged("ListaControlli")
  '  End Set
  'End Property

  'Public Property PavarotSyncViewModel As clsPavarotSyncViewModel
  '  Get
  '    Return pPavarotSyncViewModel
  '  End Get
  '  Set(value As clsPavarotSyncViewModel)
  '    pPavarotSyncViewModel = value
  '    OnPropertyChanged("PavarotSyncViewModel")
  '  End Set
  'End Property

  Public ReadOnly Property ListaPeriodi As List(Of clsPeriod2021)
    Get
      Return PeriodsManager.CollectionPavarot.ToList
    End Get
  End Property

  Private Sub ImpostaControlli()
    ListaControlli.Clear()
    Dim CanaleTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    For Each Grafico In GraficiDaStampare
      Dim ctrlTmp As New UserControlPavarotAdvancedPlot(SyncViewModel(Grafico)) 'PavarotSyncViewModel)
      Dim ctrlTmpVm As New clsPavarotAdvancedPlotViewModel(CanaleTws, Grafico, Grafico.ToString.TrimStart("e"), SyncViewModel(Grafico), ParentVM) ' PavarotSyncViewModel, ParentVM)
      ctrlTmp.DataContext = ctrlTmpVm
      ListaControlli.Add(ctrlTmp)
    Next
  End Sub

  Private Function SyncViewModel(Grafico As eGraphics) As clsPavarotSyncViewModel
    Select Case Grafico
      Case eGraphics.eBasicTrackFromEntry, eGraphics.eBasicTrackFromKeyMoment, eGraphics.eBasicPavarotSchema, eGraphics.eBasicEntryStability
        Dim Psvm As New clsPavarotSyncViewModel
        Psvm.SquaredAxis = True
        Return Psvm
      Case Else
        Return PavarotSyncViewModel
    End Select
  End Function

  Private _DisegnoAvg As New clsDisegnoProgressivoPavarot

  Public Sub AggiornaGrafici()
    Dim TmrAv As DateTime = Now
    If Not GraficiDaStampare.Count = ListaControlli.Count Then ImpostaControlli()
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "     AV.1 - ImpostaControlli " & Now.Subtract(TmrAv).TotalMilliseconds.ToString("F0") & " ms - controlli: " & ListaControlli.Count)
    _DisegnoAvg.Esegui(ListaControlli,
        Sub(Controllo)
          Controllo.ViewModel.DrawChart()
        End Sub,
        Nothing)
    If dbg Then Console.WriteLine(Now.ToString("mm:ss.fff") & "     AV.2 - disegno progressivo avviato x" & ListaControlli.Count)
  End Sub

End Class


''' <summary>
''' Esegue un'azione su una lista di controlli un elemento per ciclo del dispatcher.
''' Disegnare 18 o 9 grafici in blocco costa 1-2 secondi di UI bloccata (ogni grafico crea
''' un centinaio di serie SciChart, ~1 ms l'una); cosi' la tab si apre subito e i grafici
''' compaiono progressivamente. La struttura delle serie non cambia, quindi Tag, IsVisible
''' e la selezione sincronizzata continuano a funzionare come prima.
''' </summary>
Public Class clsDisegnoProgressivoPavarot

  Private _Token As Integer = 0

  Public Sub Annulla()
    _Token += 1
  End Sub

  Public Sub Esegui(Of T)(Lista As System.Collections.Generic.IList(Of T),
                          Disegna As Action(Of T),
                          AlTermine As Action)
    Annulla()
    Dim MioToken As Integer = _Token
    Dim Indice As Integer = 0
    Dim Dsp = System.Windows.Application.Current.Dispatcher

    Dim Passo As Action = Nothing
    Passo = Sub()
              If Not MioToken = _Token Then Return
              If Indice >= Lista.Count Then
                If Not AlTermine Is Nothing Then
                  Try
                    AlTermine()
                  Catch ex As Exception
                  End Try
                End If
                Return
              End If
              Dim Elemento As T = Lista(Indice)
              Indice += 1
              Try
                Disegna(Elemento)
              Catch ex As Exception
                Console.WriteLine("Disegno grafico Pavarot fallito: " & ex.Message)
              End Try
              Dsp.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, Passo)
            End Sub

    Dsp.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, Passo)
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotControls
  'Implements INotifyPropertyChanged

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property ParentVM As UserControlPavarotViewModel
  Public Property ListaControlli As New ObservableCollection(Of UserControlPavarotPlot)
  Public Property NomeMatrice As String = "PavarotCustomCharts"


  Public Sub New(objParentVM As UserControlPavarotViewModel)
    ParentVM = objParentVM
    CaricaControlliDaJson()
  End Sub

  'Public Property ListaControlli As ObservableCollection(Of UserControlPavarotPlot)
  '  Get
  '    Return pListaControlli
  '  End Get
  '  Set(value As ObservableCollection(Of UserControlPavarotPlot))
  '    pListaControlli = value
  '    OnPropertyChanged("ListaControlli")
  '  End Set
  'End Property

  'Public Property ParentVM As UserControlPavarotViewModel
  '  Get
  '    Return pParentVM
  '  End Get
  '  Set(value As UserControlPavarotViewModel)
  '    pParentVM = value
  '    OnPropertyChanged("ParentVM")
  '  End Set
  'End Property


  Private Sub ControlliDefault(Indice As Integer, ByRef Canale As clsChannel2020)
    Select Case Indice
      Case 0
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
      Case 1
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
      Case 2
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Case 3
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
      'Case 4
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRideHeight)
      Case 5
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrAngle)
      'Case 6
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrRake)
      'Case 7
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrRakeEffective)
      'Case 8
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.ePortCantAngle)
      'Case 9
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.ePortCantAngleEffective)
      Case 10
        Canale = DataProvider2020.CanaleDbl("PortFoilInFlap1Angle")
      Case 11
        Canale = DataProvider2020.CanaleDbl("PortFoilOutFlap1Angle")
      'Case 12
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eStbdCantAngle)
      'Case 13
      '  Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eStbdCantAngleEffective)
      Case 14
        Canale = DataProvider2020.CanaleDbl("StbdFoilInFlap1Angle")
      Case 15
        Canale = DataProvider2020.CanaleDbl("StbdFoilOutFlap1Angle")
      Case 16
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
      Case 17
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWS)
      Case Else
        Canale = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    End Select
  End Sub


  Public Sub CaricaControlliDaJson()


    ParentVM.ListaControlliCustom.Clear()
    For Each PavarotChart In AppConfig.ActiveProfile.PavarotChartSettings.Where(Function(x) Not x.MathChannel = Nothing)
      ParentVM.ListaControlliCustom.Add(PavarotChart.MathChannel)
    Next

    ListaControlli.Clear()
    For Each c In ParentVM.ListaControlliCustom
      Dim ctrlTmp As New UserControlPavarotPlot(200, ParentVM)
      Dim ctrlTmpVm As New UserControlPavarotPlotViewModelMathPlots(ParentVM, c)
      ctrlTmp.DataContext = ctrlTmpVm
      ListaControlli.Add(ctrlTmp)
    Next

    For Each c In AppConfig.ActiveProfile.PavarotChartSettings.Where(Function(x) x.MathChannel = Nothing)
      Dim ctrlTmpSc As New UserControlPavarotPlot(200, ParentVM)
      Dim ctrlTmpVMsc As New UserControlPavarotPlotViewModelStandardChannel(ParentVM, DataProvider2020.CanaleDbl(c.YaxisChannel))
      ctrlTmpSc.DataContext = ctrlTmpVMsc
      ListaControlli.Add(ctrlTmpSc)
    Next

  End Sub


  Private Function Header(CanaleAscissa As clsChannel2020) As String
    If CanaleAscissa Is Nothing Then Return ""
    If CanaleAscissa.IsMath Then
      Return CanaleAscissa.CanaleChiave.ToString.TrimStart("e")
    Else
      Return CanaleAscissa.ChannelId
    End If
  End Function

  Public Sub SalvaImpostazioneJson()
    AppConfig.ActiveProfile.PavarotChartSettings.Clear()
    For Each ctrl As UserControlPavarotPlot In ListaControlli
      Dim PavarotPlotViewModel As UserControlPavarotPlotViewModel = DirectCast(ctrl.DataContext, UserControlPavarotPlotViewModel)
      If PavarotPlotViewModel.Canale Is Nothing Then
        AppConfig.ActiveProfile.PavarotChartSettings.Add(New clsPavarotChartSettings("", PavarotPlotViewModel.CanaleCustom, PavarotPlotViewModel.ToggleSigned))
      Else
        AppConfig.ActiveProfile.PavarotChartSettings.Add(New clsPavarotChartSettings(PavarotPlotViewModel.Canale.ChannelId, Nothing, PavarotPlotViewModel.ToggleSigned))
      End If
    Next
    AppConfig.Salva()
  End Sub

  'Public Sub SalvaImpostazione()
  '  Dim Suffisso As String = DataProvider2020.SuffissoFileType
  '  AppConfig.EliminaNodo(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, True)
  '  Dim ID As Integer = 0
  '  If ListaControlli.Count > 0 Then
  '    For Each ctrl As UserControlPavarotPlot In ListaControlli
  '      If ID > 1 Then
  '        Dim PavarotPlotViewModel As UserControlPavarotPlotViewModel = DirectCast(ctrl.DataContext, UserControlPavarotPlotViewModel)
  '        AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Yaxis", Header(PavarotPlotViewModel.Canale), True, False)
  '      End If
  '      ID += 1
  '    Next
  '    AppConfig.SalvaFileXML()
  '  End If

  'End Sub


  Public Sub AggiungiCanale(Canale As clsChannel2020)
    Dim PavarotPlotViewModel As New UserControlPavarotPlotViewModelStandardChannel(ParentVM, Canale)
    Dim ctrlTmp As New UserControlPavarotPlot(200, ParentVM)
    ctrlTmp.DataContext = PavarotPlotViewModel
    'AppConfig.SalvaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ListaControlli.Count, "Yaxis", Header(Canale), True, True)
    ListaControlli.Add(ctrlTmp)
    AppConfig.ActiveProfile.PavarotChartSettings.Add(New clsPavarotChartSettings(PavarotPlotViewModel.Canale.ChannelId, Nothing, PavarotPlotViewModel.ToggleSigned))
    AppConfig.Salva()
  End Sub
End Class


Public Class clsStatTWD
  Dim pMatriceValori(3, 359) As Integer
  Dim pMaxSamples(3) As Integer
  Dim pSubMatrice0(21) As Integer
  Dim pSubMatrice1(21) As Integer
  Dim pSubMatrice2(21) As Integer
  Dim pSubMatrice3(21) As Integer

  Public ReadOnly Property MaxSamples(samples As Integer()) As Integer
    Get
      If samples(0) = 0 OrElse samples(1) = 0 OrElse samples(2) = 0 OrElse samples(3) = 0 Then Return 0
      Return System.Math.Max(pMaxSamples(0) / samples(0), System.Math.Max(pMaxSamples(1) / samples(1), System.Math.Max(pMaxSamples(2) / samples(2), pMaxSamples(3) / samples(3)))) * 100
    End Get
  End Property

  Public ReadOnly Property SubMatrice(Indice As Integer) As Integer()
    Get
      Select Case Indice
        Case 0
          Return pSubMatrice0
        Case 1
          Return pSubMatrice1
        Case 2
          Return pSubMatrice2
        Case Else
          Return pSubMatrice3
      End Select
    End Get
  End Property

  Public Property MatriceValori As Integer(,)
    Get
      Return pMatriceValori
    End Get
    Set(value As Integer(,))
      pMatriceValori = value
    End Set
  End Property

  Public Sub New()

    For i As Integer = 0 To pMatriceValori.GetLength(0) - 1
      For ii As Integer = 0 To pMatriceValori.GetLength(1) - 1
        pMatriceValori(i, ii) = 0
      Next
    Next
  End Sub

  Public Sub AggiornaStatistiche(TWD As Integer)
    Try
      If TWD < 0 Then TWD = 0
      If TWD > 359 Then TWD = 0
      For i As Integer = 0 To 3
        pMaxSamples(i) = 0
      Next
      For ii As Integer = TWD - 10 To TWD + 10
        Dim TWDtmp As Integer = ii
        If TWDtmp < 0 Then TWDtmp += 360
        If TWDtmp >= 360 Then TWDtmp -= 360
        For i As Integer = 0 To 3
          If pMatriceValori(i, TWDtmp) > pMaxSamples(i) Then pMaxSamples(i) = pMatriceValori(i, TWDtmp)
        Next
      Next
    Catch ex As Exception

    End Try
  End Sub
End Class

Public Class clsRettaPerDuePunti
  Dim pPuntoGeoA As clsGeographicPosition
  Dim pPuntoGeoB As clsGeographicPosition
  Dim pPuntoGeoRef As clsGeographicPosition
  Dim pPuntoCartA As clsCartesianRelativePosition
  Dim pPuntoCartB As clsCartesianRelativePosition
  Dim pCoeffAng As Double
  Dim pOrdOrig As Double

  Public ReadOnly Property CoeffAng As Double
    Get
      Return pCoeffAng
    End Get
  End Property

  Public ReadOnly Property OrdOrig As Double
    Get
      Return pOrdOrig
    End Get
  End Property


  Public Property PuntoGeoA As clsGeographicPosition
    Get
      Return pPuntoGeoA
    End Get
    Set(value As clsGeographicPosition)
      pPuntoGeoA = value
      RettaPerDuePunti()
    End Set
  End Property

  Public ReadOnly Property PuntoGeoB As clsGeographicPosition
    Get
      Return pPuntoGeoB
    End Get
  End Property

  Public ReadOnly Property PuntoGeoRef As clsGeographicPosition
    Get
      Return pPuntoGeoRef
    End Get
  End Property

  Public Sub New(PuntoGeoA As clsGeographicPosition, PuntoGeoB As clsGeographicPosition, PuntoGeoRef As clsGeographicPosition)
    pPuntoGeoA = PuntoGeoA
    pPuntoGeoB = PuntoGeoB
    pPuntoGeoRef = PuntoGeoRef

    pPuntoCartA = New clsCartesianRelativePosition(pPuntoGeoA, pPuntoGeoRef)
    pPuntoCartB = New clsCartesianRelativePosition(pPuntoGeoB, pPuntoGeoRef)

    Dim pControlloA As clsGeographicPosition = New clsCartesianRelativePosition(pPuntoCartA.LatCartDeg, pPuntoCartA.LngCartDec, pPuntoGeoRef).PuntoGeo
    Dim pControlloB As clsGeographicPosition = New clsCartesianRelativePosition(pPuntoCartB.LatCartDeg, pPuntoCartB.LngCartDec, pPuntoGeoRef).PuntoGeo


    RettaPerDuePunti()
  End Sub

  Public Sub RettaPerDuePunti()
    ' Y1 = A*X1 + B  --> B = Y1 - A*X1
    ' Y2 = A*X2 + B  --> B = Y2 - A*X2
    ' Y1 - A*X1 = Y2 - A*X2 --> A = (Y1-Y2)/(X1-X2)
    pCoeffAng = (pPuntoCartA.LatCartDeg - pPuntoCartB.LatCartDeg) / (pPuntoCartA.LngCartDec - pPuntoCartB.LngCartDec)
    pOrdOrig = pPuntoCartA.LatCartDeg - pCoeffAng * pPuntoCartA.LngCartDec
    'pOrdOrig = pPuntoCartB.LatCartDec - pCoeffAng * pPuntoCartB.LngCartDec
  End Sub


End Class

Public Class clsCartesianRelativePosition
  Dim pPuntoGeo As clsGeographicPosition
  Dim pPuntoGeoRef As clsGeographicPosition
  Dim pLatCartDeg As Double
  Dim pLngCartDeg As Double

  Public ReadOnly Property PuntoGeo As clsGeographicPosition
    Get
      Return pPuntoGeo
    End Get
  End Property

  Public ReadOnly Property PuntoGeoRef As clsGeographicPosition
    Get
      Return pPuntoGeoRef
    End Get
  End Property

  Public ReadOnly Property LatCartDeg As Double
    Get
      Return pLatCartDeg
    End Get
  End Property

  Public ReadOnly Property LngCartDec As Double
    Get
      Return pLngCartDeg
    End Get
  End Property

  Public Sub New(PuntoGeo As clsGeographicPosition, PuntoGeoRef As clsGeographicPosition)
    pPuntoGeo = PuntoGeo
    pPuntoGeoRef = PuntoGeoRef
    pLatCartDeg = PuntoGeo.LatDec - PuntoGeoRef.LatDec
    pLngCartDeg = (PuntoGeo.LngDec - PuntoGeoRef.LngDec) * System.Math.Abs(System.Math.Cos(Radians(PuntoGeo.LatDec)))
  End Sub

  Public Sub New(LatCartDec As Double, LngCartDec As Double, PuntoGeoRef As clsGeographicPosition)
    pLatCartDeg = LatCartDec
    pLngCartDeg = LngCartDec
    pPuntoGeoRef = PuntoGeoRef
    Dim pLatGeoTemp As Double = PuntoGeoRef.LatDec + LatCartDec
    Dim pLngGeoTemp As Double = PuntoGeoRef.LngDec + (LngCartDec / System.Math.Abs(System.Math.Cos(Radians(pLatGeoTemp))))
    pPuntoGeo = New clsGeographicPosition(pLatGeoTemp, pLngGeoTemp)

  End Sub

End Class

Public Class clsCalcoliDuePuntiGeo
  Dim pPuntoA As clsGeographicPosition
  Dim pPuntoB As clsGeographicPosition
  Dim _DistanzaMetri As Double
  Dim pRotta As Double
  Const RT As Double = 6372795.477598 ' Metri

  Public ReadOnly Property PuntoA As clsGeographicPosition
    Get
      Return pPuntoA
    End Get
  End Property

  Public ReadOnly Property PuntoB As clsGeographicPosition
    Get
      Return pPuntoB
    End Get
  End Property

  Public ReadOnly Property DistanzaMetri As Double
    Get
      Return _DistanzaMetri
    End Get
  End Property

  Public ReadOnly Property Rotta As Double
    Get
      Return pRotta
    End Get
  End Property

  Public ReadOnly Property RottaTrue As Double
    Get
      Return pRotta
    End Get
  End Property

  Public ReadOnly Property DistanzaNM As Double
    Get
      Return _DistanzaMetri / 1852
    End Get
  End Property


  Public Sub CalcolaDistanzaAndRotta(PuntoAlat As Double, PuntoAlong As Double, PuntoBlat As Double, PuntoBlong As Double)
    pPuntoA = New clsGeographicPosition(PuntoAlat, PuntoAlong, True)
    pPuntoB = New clsGeographicPosition(PuntoBlat, PuntoBlong, True)
    CalcolaDistanza()
    CalcolaRotta()
  End Sub

  Public Sub CalcolaDistanzaAndRotta(PuntoA As clsGeographicPosition, PuntoB As clsGeographicPosition)
    pPuntoA = PuntoA
    pPuntoB = PuntoB
    CalcolaDistanza()
    If _DistanzaMetri = 0 Then
      pRotta = 0
    Else
      CalcolaRotta()
      'RottaIniziale()
    End If
  End Sub

  Private Sub CalcolaDistanza()
    'pDistanza = DistanceMeters(PuntoA, PuntoB)
    'Exit Sub
    '    distanza(A, B) = R * arccos(sin(latA) * sin(latB) + cos(latA) * cos(latB) * cos(lonA - lonB))
    'R = 6372,795477598 Chilometri
    'LatA, LatB, LongA e LongB in radianti
    _DistanzaMetri = RT * System.Math.Acos((System.Math.Sin(pPuntoA.LatR) * System.Math.Sin(pPuntoB.LatR)) + (System.Math.Cos(pPuntoA.LatR) * System.Math.Cos(pPuntoB.LatR) * System.Math.Cos(pPuntoA.LngR - pPuntoB.LngR)))
    If Double.IsNaN(_DistanzaMetri) Then _DistanzaMetri = 0
    'p = Math.Acos((Math.Sin(radLatA) * Math.Sin(radLatB)) + (Math.Cos(radLatA) * Math.Cos(radLatB) * Math.Cos(phi)))

  End Sub



  Private Sub CalcolaRotta()
    'CalcolaRottaOLD()
    'Exit Sub
    'Dim DeltaPhi As Double = System.Math.Log(System.Math.Tan(pPuntoB.LatR / 2 + System.Math.PI / 4) / System.Math.Tan(pPuntoA.LatR / 2 + System.Math.PI / 4))
    'Dim DeltaLon As Double = System.Math.Abs(pPuntoA.LngR - pPuntoB.LngR)
    'Dim RottaRad As Double = System.Math.Atan2(DeltaLon, DeltaPhi)
    'pRotta = pObjCalcoli.RadiantiToGradi(RottaRad)




    Dim pDlatMediaD As Double = (pPuntoA.LatDec + pPuntoB.LatDec) / 2
    Dim pDlatD As Double = pPuntoB.LatDec - pPuntoA.LatDec
    Dim pDlongD As Double = pPuntoB.LngDec - pPuntoA.LngDec
    If pDlatD = 0 Then
      If pDlongD > 0 Then
        pRotta = 90
        Exit Sub
      ElseIf pDlongD < 0 Then
        pRotta = 270
        Exit Sub
      Else
        pRotta = 0
        Exit Sub
      End If
    ElseIf pDlongD = 0 Then
      If pDlatD > 0 Then
        pRotta = 0
        Exit Sub
      ElseIf pDlatD < 0 Then
        pRotta = 180
        Exit Sub
      Else
        pRotta = 0
        Exit Sub
      End If
    Else

    End If

    Dim pDlatMedia As Double = Radians(pDlatMediaD)
    Dim pDlat As Double = Radians(pDlatD)
    Dim pDlong As Double = Radians(pDlongD)
    Dim pRt As Double = System.Math.Atan(pDlong * System.Math.Cos(pDlatMedia) / pDlat)
    ' Vale Nell'emisfero NORD va verificato con il sud
    If pPuntoB.LatDec > pPuntoA.LatDec Then
      If pRt < 0 Then pRt += System.Math.PI * 2
    Else
      pRt += System.Math.PI
    End If

    'If pRt < 0 Then pRt += System.Math.PI * 2
    Dim pRtRad As Double = Degrees(pRt)
    pRotta = pRtRad

  End Sub

  Private Sub RottaIniziale()
    ' cot Ri * sec lat.a = cosec diff.long * tan lat.b - cot diff.long. * tan lat.a
    Dim DeltaLon As Double = pPuntoA.LngR - pPuntoB.LngR
    Dim C As Double = Secante(pPuntoA.LatR)
    Dim A As Double = Cosecante(DeltaLon) * System.Math.Tan(pPuntoB.LatR)
    Dim B As Double = Cotangente(DeltaLon) * System.Math.Tan(pPuntoA.LatR)
    Dim RI As Double = CotangenteInv((A - B) / C)
    RI = Radians(RI)

  End Sub

  Private Function Secante(X As Double) As Double
    Return 1 / System.Math.Cos(X)
  End Function

  Private Function Cosecante(X As Double) As Double
    Return 1 / System.Math.Sin(X)
  End Function

  Private Function Cotangente(X As Double) As Double
    Return 1 / System.Math.Tan(X)
  End Function

  Private Function SecanteInv(X As Double) As Double
    Return 2 * System.Math.Atan(1) - System.Math.Atan(System.Math.Sign(X) / System.Math.Sqrt(X * X - 1))
  End Function

  Private Function CosecanteInv(X As Double) As Double
    Return System.Math.Atan(System.Math.Sign(X) / System.Math.Sqrt(X * X - 1))
  End Function

  Private Function CotangenteInv(X As Double) As Double
    Return 2 * System.Math.Atan(1) - System.Math.Atan(1)
  End Function

  Public Function PuntoDestinazione(PuntoOrig As clsGeographicPosition, DistanzaMetri As Double, Rotta As Double) As clsGeographicPosition
    _DistanzaMetri = DistanzaMetri
    pRotta = Rotta
    pPuntoA = PuntoOrig
    Dim LatDestR As Double
    Dim LongDestR As Double
    Dim RottaRad As Double = Radians(Rotta)
    LatDestR = System.Math.Asin(System.Math.Sin(pPuntoA.LatR) * System.Math.Cos(DistanzaMetri / RT) + System.Math.Cos(pPuntoA.LatR) * System.Math.Sin(DistanzaMetri / RT) * System.Math.Cos(RottaRad))
    LongDestR = pPuntoA.LngR + System.Math.Atan2(System.Math.Sin(RottaRad) * System.Math.Sin(DistanzaMetri / RT) * System.Math.Cos(pPuntoA.LatR), System.Math.Cos(DistanzaMetri / RT) - System.Math.Sin(pPuntoA.LatR) * System.Math.Sin(LatDestR))
    pPuntoB = New clsGeographicPosition(Degrees(LatDestR), Degrees(LongDestR), PuntoOrig.IsValid)
    Return pPuntoB
  End Function


End Class

Public Class clsGeographicPosition
  Dim pLatDec As Double
  Dim pLngDec As Double
  Dim pIsValid As Boolean
  'Dim pObjCalcoli As New clsCalcoli


  Public ReadOnly Property IsValid As Boolean
    Get
      Return pIsValid
    End Get
  End Property

  Public ReadOnly Property PositionString As String
    Get
      Return pLatDec.ToString("F8") & "" & pLngDec.ToString("F8")
    End Get
  End Property

  Public ReadOnly Property LatDec As Double
    Get
      Return pLatDec
    End Get
  End Property

  Public ReadOnly Property LngDec As Double
    Get
      Return pLngDec
    End Get
  End Property

  Public ReadOnly Property LatR As Double
    Get
      Return Radians(pLatDec)
    End Get
  End Property

  Public ReadOnly Property LngR As Double
    Get
      Return Radians(pLngDec)
    End Get
  End Property


  Public Sub New(LatDec As Double, LngDec As Double, IsValid As Boolean)
    pLatDec = LatDec
    pLngDec = LngDec
    pIsValid = IsValid
  End Sub

  Public Sub New(LatDec As Double, LngDec As Double)
    pLatDec = LatDec
    pLngDec = LngDec
    pIsValid = True
  End Sub

  Public Sub New(LatDfw As String, LngDfw As String)
    pLatDec = clsGeoCalculations.StrLatLonDfwToDeg(LatDfw)
    pLngDec = clsGeoCalculations.StrLatLonDfwToDeg(LngDfw)
    pIsValid = True
  End Sub






End Class

Public Class clsCourseLeg
  Dim pMarkFrom As clsCourseMark
  Dim pMarkTo As clsCourseMark
  Dim pObjPointToPoint As New clsCalcoliDuePuntiGeo
  'Dim pLegInfo As clsLegInfo
  'Dim pLegInfoWithCorrections As clsLegInfo

  Public Property MarkFrom As clsCourseMark
    Get
      Return pMarkFrom
    End Get
    Set(value As clsCourseMark)
      pMarkFrom = value
      AggiornaRngBrg()
    End Set
  End Property

  Public Property MarkTo As clsCourseMark
    Get
      Return pMarkTo
    End Get
    Set(value As clsCourseMark)
      pMarkTo = value
      AggiornaRngBrg()
    End Set
  End Property

  'Public ReadOnly Property strLegRangeNM As String
  '  Get
  '    Return Format(pObjPointToPoint.DistanzaNM, "F2")
  '  End Get
  'End Property

  'Public ReadOnly Property LegRangeNM As Double
  '  Get
  '    Return pObjPointToPoint.DistanzaNM
  '  End Get
  'End Property

  Public ReadOnly Property strLegRangeMeters As String
    Get
      Return Format(pObjPointToPoint.DistanzaMetri, "F0")
    End Get
  End Property

  Public ReadOnly Property LegRangeMeters As Double
    Get
      Return pObjPointToPoint.DistanzaMetri
    End Get
  End Property

  Public ReadOnly Property strLegBearingTrue As String
    Get
      Return Format(pObjPointToPoint.RottaTrue, "F0")
    End Get
  End Property

  'Public ReadOnly Property strLegBearingNormalizzato As String
  '  Get
  '    Return Format(LegBearingNormalizzato, "F0")
  '  End Get
  'End Property

  Public ReadOnly Property LegBearingTrue As Double
    Get
      Return pObjPointToPoint.RottaTrue
    End Get
  End Property

  'Public ReadOnly Property LegBearingNormalizzato As Double
  '  Get
  '    'Return BRGnormalizzatoDaBRGtrue(pObjPointToPoint.RottaTrue)
  '    Return pObjPointToPoint.Rotta
  '  End Get
  'End Property

  Public ReadOnly Property ObjPointToPoint As clsCalcoliDuePuntiGeo
    Get
      Return pObjPointToPoint
    End Get
  End Property

  'Public ReadOnly Property LegInfo As clsLegInfo
  '  Get
  '    Return pLegInfo
  '  End Get
  'End Property

  'Public ReadOnly Property LegInfoWithCorrection As clsLegInfo
  '  Get
  '    Return pLegInfoWithCorrections
  '  End Get
  'End Property

  Public Sub New(ByRef MarkFrom As clsCourseMark, ByRef MarkTo As clsCourseMark)
    pMarkFrom = MarkFrom
    pMarkTo = MarkTo
    AggiornaRngBrg()
    'pLegInfo = New clsLegInfo(Me)
    'pLegInfoWithCorrections = New clsLegInfo(Me)
  End Sub

  Public Sub AggiornaRngBrg()
    If pMarkFrom Is Nothing OrElse pMarkTo Is Nothing Then Exit Sub
    pObjPointToPoint.CalcolaDistanzaAndRotta(pMarkFrom.MarkPosition, pMarkTo.MarkPosition)
  End Sub

  'Public Sub AggiornaLegInfo()
  '  pLegInfo.AggiornaLegInfo(False)
  '  pLegInfoWithCorrections.AggiornaLegInfo(True)
  'End Sub

End Class

Public Class clsCourseMark
  Dim pMarkPosition As clsGeographicPosition
  Dim pMarkName As String
  Dim pSideToBeLeft As eSideToBeLeft

  Public Property MarkPosition As clsGeographicPosition
    Get
      Return pMarkPosition
    End Get
    Set(value As clsGeographicPosition)
      pMarkPosition = value
    End Set
  End Property

  Public Property MarkName As String
    Get
      Return pMarkName
    End Get
    Set(value As String)
      pMarkName = value
    End Set
  End Property

  Public Property SideToBeLeft As eSideToBeLeft
    Get
      Return pSideToBeLeft
    End Get
    Set(value As eSideToBeLeft)
      pSideToBeLeft = value
    End Set
  End Property

  Public Enum eSideToBeLeft
    eToMark = 0
    ePort = 1
    eStbd = 2
  End Enum

  Public Sub New(GeograficPosition As clsGeographicPosition, MarkName As String, SideToBeLeft As eSideToBeLeft)
    pMarkPosition = GeograficPosition
    pMarkName = MarkName
    pSideToBeLeft = SideToBeLeft

  End Sub

  Public Sub New(LatDeg As Double, lngDeg As Double, MarkName As String, SideToBeLeft As eSideToBeLeft)
    pMarkPosition = New clsGeographicPosition(LatDeg, lngDeg)
    pMarkName = MarkName
    pSideToBeLeft = SideToBeLeft
  End Sub

End Class

Public Class clsEvent
  Dim pMomento As Date
  Dim pDescrizione As String
  Dim pShortName As String

  Public Sub New(Momento As DateTime, Descrizione As String, ShortName As String)
    pMomento = Momento
    pDescrizione = Descrizione
    pShortName = ShortName
  End Sub

  Public Property Momento As Date
    Get
      Return pMomento
    End Get
    Set(value As Date)
      pMomento = value
    End Set
  End Property

  Public Property Descrizione As String
    Get
      Return pDescrizione
    End Get
    Set(value As String)
      pDescrizione = value
    End Set
  End Property

  Public Property ShortName As String
    Get
      Return pShortName
    End Get
    Set(value As String)
      pShortName = value
    End Set
  End Property
End Class

Public Class clsChannelStats
  Dim _ChannelId As String
  Dim _Channel As clsChannel2020
  Dim _Avg As Double
  Dim _Max As Double
  Dim _Min As Double
  Dim _SD As Double

  Public Sub New()
    'quando carica i valori da Json

  End Sub

  Public Sub New(Channel As clsChannel2020, Period As clsTimeRange)
    _Channel = Channel
    _ChannelId = Channel.ChannelId
    'fa i calcoli
    Dim ValoriCanale As Double() = Channel.Valori.Skip(Period.IdRigaIniziale).Take(Period.IdRigaFinale - Period.IdRigaIniziale).Where(Function(x) Not Double.IsNaN(x)).ToArray
    If ValoriCanale.Count = 0 Then
      _Max = Double.NaN
      _Min = Double.NaN
      _Avg = Double.NaN
      _SD = Double.NaN
    Else
      _Max = ValoriCanale.Max
      _Min = ValoriCanale.Min
      _Avg = ValoriCanale.Average
      alglib.basestat.sampleadev(ValoriCanale.ToArray, ValoriCanale.Count, _SD)
    End If
  End Sub

  Public Property Avg As Double
    Get
      Return _Avg
    End Get
    Set(value As Double)
      _Avg = value
    End Set
  End Property

  Public Property Max As Double
    Get
      Return _Max
    End Get
    Set(value As Double)
      _Max = value
    End Set
  End Property

  Public Property Min As Double
    Get
      Return _Min
    End Get
    Set(value As Double)
      _Min = value
    End Set
  End Property

  Public Property SD As Double
    Get
      Return _SD
    End Get
    Set(value As Double)
      _SD = value
    End Set
  End Property

  Public Property ChannelId As String
    Get
      Return _ChannelId
    End Get
    Set(value As String)
      _ChannelId = value
    End Set
  End Property

  <JsonIgnore>
  Public Property Channel As clsChannel2020
    Get
      If _Channel Is Nothing Then
        _Channel = DataProvider2020.CanaleDbl(ChannelId)
      End If
      Return _Channel
    End Get
    Set(value As clsChannel2020)
      _Channel = value
    End Set
  End Property
End Class

Public Class clsPeriodBasicDetails
  Dim _SecFrom As Integer
  Dim _SecTo As Integer
  Dim _Tws As Double
  Dim _Vmg As Double
  Dim _ChannelsStats As New List(Of clsChannelStats)

  Public Property SecFrom As Integer
    Get
      Return _SecFrom
    End Get
    Set(value As Integer)
      _SecFrom = value
    End Set
  End Property

  Public Property SecTo As Integer
    Get
      Return _SecTo
    End Get
    Set(value As Integer)
      _SecTo = value
    End Set
  End Property

  Public Property Tws As Double
    Get
      Return _Tws
    End Get
    Set(value As Double)
      _Tws = value
    End Set
  End Property

  Public Property Vmg As Double
    Get
      Return _Vmg
    End Get
    Set(value As Double)
      _Vmg = value
    End Set
  End Property

  Public Property ChannelsStats As List(Of clsChannelStats)
    Get
      Return _ChannelsStats
    End Get
    Set(value As List(Of clsChannelStats))
      _ChannelsStats = value
    End Set
  End Property

  Public Sub New()

  End Sub

  Public Sub New(Period As clsPeriod2021, SecFrom As Integer, SecTo As Integer, Channels As List(Of clsChannel2020))
    _SecFrom = SecFrom
    _SecTo = SecTo
    Dim Tr As New clsTimeRange(Period.KeyMoment.AddSeconds(SecFrom), Period.KeyMoment.AddSeconds(SecTo))
    For Each canale In Channels
      _ChannelsStats.Add(New clsChannelStats(canale, Tr))
      Select Case canale.ChannelId
        Case "vmg"
          _Vmg = ChannelsStats.Last.Avg
        Case "tws"
          _Tws = ChannelsStats.Last.Avg
      End Select
    Next
  End Sub

  Public Function DeltaToGostMetersVmg(TargetVmg As Double) As Double
    Dim DeltaMs As Double = KtsToMS(Vmg - TargetVmg) 'differenza di metri persi per ogni secondo
    Return System.Math.Abs(SecFrom - SecTo) * DeltaMs
  End Function

  Public Function DeltaToGostMetersInLine(TargetVmg As Double, TargetTwa As Double) As Double
    Dim DeltaMs As Double = KtsToMS(Vmg - TargetVmg) / System.Math.Abs(System.Math.Cos(Radians(TargetTwa))) 'differenza di metri in linea persi per ogni secondo
    Return System.Math.Abs(SecFrom - SecTo) * DeltaMs
  End Function

  Public Function DeltaToGostSeconds(TargetVmg As Double) As Double
    Dim Eff As Double = Vmg / TargetVmg 'differenza di metri persi per ogni secondo
    Return System.Math.Abs(SecFrom - SecTo) * (1 - Eff)
  End Function


End Class

Public Class clsVettore2D
  Dim _Range As Double
  Dim _Bearing As Double

  Public Sub New(Range As Double, Bearing As Double)
    _Range = Range
    _Bearing = Bearing
  End Sub

  Public Property Range As Double
    Get
      Return _Range
    End Get
    Set(value As Double)
      _Range = value
    End Set
  End Property

  Public Property Bearing As Double
    Get
      Return _Bearing
    End Get
    Set(value As Double)
      _Bearing = value
    End Set
  End Property

  Public Function SommaVettoriale(VettoreDaSommare As clsVettore2D) As clsVettore2D
    Dim V As New List(Of clsVettore2D)
    V.Add(VettoreDaSommare)
    Return SommaVettoriale(V)
  End Function

  Public Function SommaVettoriale(VettoriDaSommare As List(Of clsVettore2D)) As clsVettore2D
    Dim _U As New List(Of Double)
    Dim _V As New List(Of Double)
    _U.Add(Range * System.Math.Sin(Radians(Bearing)))
    _V.Add(Range * System.Math.Cos(Radians(Bearing)))
    For Each VettoreDaSommare In VettoriDaSommare
      _U.Add(VettoreDaSommare.Range * System.Math.Sin(Radians(VettoreDaSommare.Bearing)))
      _V.Add(VettoreDaSommare.Range * System.Math.Cos(Radians(VettoreDaSommare.Bearing)))
    Next
    Dim _R As Double = System.Math.Sqrt(_U.Sum ^ 2 + _V.Sum ^ 2)
    Dim _B As Double = Degrees(System.Math.Atan2(_U.Sum, _V.Sum))
    Dim _Bb As Double = Degrees(System.Math.Atan(_U.Sum / _V.Sum))

    Return New clsVettore2D(_R, _Bb)
  End Function

End Class

Public Class clsPeriodPoint
  Dim _SecFromRef As Integer
  Dim _MetersFromRefXoverwater As Double
  Dim _MetersFromRefYoverwater As Double
  Dim _MetersFromRefXoverground As Double
  Dim _MetersFromRefYoverground As Double

  Public Sub New()

  End Sub

  Public Sub New(SecFromRef As Integer, MetersFromRefXoverwater As Double, MetersFromRefYoverwater As Double, MetersFromRefXoverground As Double, MetersFromRefYoverground As Double)
    _SecFromRef = SecFromRef
    _MetersFromRefXoverwater = MetersFromRefXoverwater
    _MetersFromRefYoverwater = MetersFromRefYoverwater
    _MetersFromRefXoverground = MetersFromRefXoverground
    _MetersFromRefYoverground = MetersFromRefYoverground
  End Sub

  Public Property MetersFromRefXoverwater As Double
    Get
      Return _MetersFromRefXoverwater
    End Get
    Set(value As Double)
      _MetersFromRefXoverwater = value
    End Set
  End Property

  Public Property MetersFromRefYoverwater As Double
    Get
      Return _MetersFromRefYoverwater
    End Get
    Set(value As Double)
      _MetersFromRefYoverwater = value
    End Set
  End Property

  Public Property MetersFromRefXoverground As Double
    Get
      Return _MetersFromRefXoverground
    End Get
    Set(value As Double)
      _MetersFromRefXoverground = value
    End Set
  End Property

  Public Property MetersFromRefYoverground As Double
    Get
      Return _MetersFromRefYoverground
    End Get
    Set(value As Double)
      _MetersFromRefYoverground = value
    End Set
  End Property
End Class

Public Class clsEntryStability
  Dim pChHeel As clsChannel2020
  Dim pChTrim As clsChannel2020
  Dim pPeriodo As clsTimeRange

  Dim pValoriHeel As clsValoriPeriodoCanale2020
  Dim pValoriTrim As clsValoriPeriodoCanale2020

  Public Sub New(Inizio As DateTime, Fine As DateTime)
    pChHeel = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
    pChTrim = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm)
    pPeriodo = New clsTimeRange(Inizio, Fine)
    If Not pChTrim Is Nothing AndAlso Not pChTrim Is Nothing Then
      pValoriHeel = New clsValoriPeriodoCanale2020(pChHeel, pPeriodo, False)
      pValoriTrim = New clsValoriPeriodoCanale2020(pChTrim, pPeriodo, False)
      pValoriHeel.AggiornaValori(False)
      pValoriTrim.AggiornaValori(False)
    End If
  End Sub

  Public Property ValoriHeel As clsValoriPeriodoCanale2020
    Get
      Return pValoriHeel
    End Get
    Set(value As clsValoriPeriodoCanale2020)
      pValoriHeel = value
    End Set
  End Property

  Public Property ValoriTrim As clsValoriPeriodoCanale2020
    Get
      Return pValoriTrim
    End Get
    Set(value As clsValoriPeriodoCanale2020)
      pValoriTrim = value
    End Set
  End Property

End Class

Public Class clsPavarotSchema
  Dim pPuntoEntry As clsPuntoGeografico
  Dim pPuntoHDtW As clsPuntoGeografico
  Dim pPuntoExit As clsPuntoGeografico

  Public Sub New(PuntoEntry As clsPuntoGeografico, PuntoHDtW As clsPuntoGeografico, PuntoExit As clsPuntoGeografico)
    pPuntoEntry = PuntoEntry
    pPuntoHDtW = PuntoHDtW
    pPuntoExit = PuntoExit

  End Sub

  Public Property PuntoEntry As clsPuntoGeografico
    Get
      Return pPuntoEntry
    End Get
    Set(value As clsPuntoGeografico)
      pPuntoEntry = value
    End Set
  End Property

  Public Property PuntoHDtW As clsPuntoGeografico
    Get
      Return pPuntoHDtW
    End Get
    Set(value As clsPuntoGeografico)
      pPuntoHDtW = value
    End Set
  End Property

  Public Property PuntoExit As clsPuntoGeografico
    Get
      Return pPuntoExit
    End Get
    Set(value As clsPuntoGeografico)
      pPuntoExit = value
    End Set
  End Property
End Class

Public Class clsPuntoGeografico
  Dim pPuntoCartesianoDaRef As clsPuntoCartesiano
  Dim _PuntoCartesianoDaRefConCorrente As clsPuntoCartesiano
  Dim _PuntoGeo As clsGeographicPosition
  Dim _PuntoGeoOverWater As clsGeographicPosition
  Dim pMomento As DateTime

  Public Sub New(MetriVmgDaRef As Double, MetriLatDaRef As Double, PuntoGeo As clsGeographicPosition, Momento As DateTime)
    _PuntoGeo = PuntoGeo
    pMomento = Momento
    pPuntoCartesianoDaRef = New clsPuntoCartesiano(MetriVmgDaRef, MetriLatDaRef, Momento)
  End Sub

  Public Property PuntoCartesianoDaRef As clsPuntoCartesiano
    Get
      Return pPuntoCartesianoDaRef
    End Get
    Set(value As clsPuntoCartesiano)
      pPuntoCartesianoDaRef = value
    End Set
  End Property

  Public ReadOnly Property PuntoCartesianoDaRefConCorrente As clsPuntoCartesiano
    Get
      Return _PuntoCartesianoDaRefConCorrente
    End Get
  End Property

  Public Sub RiposizionaPerCorrente(PuntoRef As clsGeographicPosition, CurrDir As Double, CurrRate As Double, SecDaRef As Double, StbdToPort As Boolean, IsTack As Boolean, AsseRiferimento360 As Double)
    'applico al punto l'effetto che la corrente ha avuto per ricostruire una traccia sull acqua priva della corrente
    Dim CG As New clsCalcoliDuePuntiGeo()
    _PuntoGeoOverWater = CG.PuntoDestinazione(_PuntoGeo, CurrRate / 3600 * 1852 * Math.Abs(SecDaRef), SommaAngolo180adAngolo360(180, CurrDir))

    'Dim RangeMt As Double = clsGeoCalculations.DistanceMeters(PuntoRef, _PuntoGeoOverWater)
    'Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(PuntoRef, _PuntoGeoOverWater)

    CG.CalcolaDistanzaAndRotta(PuntoRef, _PuntoGeoOverWater)
    Dim RNG As Double = CG.DistanzaMetri
    Dim BRG As Double = CG.RottaTrue
    Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(AsseRiferimento360, BRG)
    Dim BRGc As Double = BRGinCartesiano(BRGoc)
    Dim MetriLat As Double = RNG * System.Math.Cos(Radians(BRGc))
    If Not StbdToPort Then MetriLat *= -1
    Dim MetriVmg As Double = RNG * System.Math.Sin(Radians(BRGc))
    If Not IsTack Then
      MetriVmg *= -1
    End If
    _PuntoCartesianoDaRefConCorrente = New clsPuntoCartesiano(MetriVmg, MetriLat, Momento)
  End Sub

  Public Property Momento As Date
    Get
      Return pMomento
    End Get
    Set(value As Date)
      pMomento = value
    End Set
  End Property

  Public Property PuntoGeoOverWater As clsGeographicPosition
    Get
      Return _PuntoGeoOverWater
    End Get
    Set(value As clsGeographicPosition)
      _PuntoGeoOverWater = value
    End Set
  End Property

  Public Property PuntoGeo As clsGeographicPosition
    Get
      Return _PuntoGeo
    End Get
    Set(value As clsGeographicPosition)
      _PuntoGeo = value
    End Set
  End Property
End Class

Public Class clsPuntoCartesiano
  Dim pMetriVmg As Double
  Dim pMetriLat As Double
  Dim pMomento As DateTime

  Public Sub New(MetriVmg As Double, MetriLat As Double, Momento As DateTime)
    pMetriVmg = MetriVmg
    pMetriLat = MetriLat
    pMomento = Momento
  End Sub

  Public Property MetriVmg As Double
    Get
      Return pMetriVmg
    End Get
    Set(value As Double)
      pMetriVmg = value
    End Set
  End Property

  Public Property MetriLat As Double
    Get
      Return pMetriLat
    End Get
    Set(value As Double)
      pMetriLat = value
    End Set
  End Property

  Public ReadOnly Property ToPointF As PointF
    Get
      Return New PointF(pMetriLat, pMetriVmg)
    End Get
  End Property

  Public Property Momento As Date
    Get
      Return pMomento
    End Get
    Set(value As Date)
      pMomento = value
    End Set
  End Property
End Class


Public Class clsMediePeriodo
  'Dim pMediePeriodoDb As clsMediePeriodoDb

  Dim cRotta As clsChannel2020
  Dim cSpeed As clsChannel2020
  Dim cTWD As clsChannel2020
  Dim cTWS As clsChannel2020
  Dim cLat As clsChannel2020
  Dim CLong As clsChannel2020

  Dim pRigaIniziale As Integer
  Dim pRigaFinale As Integer
  'Dim pLegPuntoPunto As clsCourseLeg

  Dim pRottaMedia As Double
  Dim pSpeedMedia As Double
  Dim pSpeedMinima As Double
  Dim pTWDMedia As Double
  Dim pTWSMedia As Double


  Dim pMatriceMsVMGtwd As List(Of Double)
  Dim pMatriceMsVMGmediana As List(Of Double)
  Dim pMatriceMsVMGmedianaRec As List(Of Double)

  Dim pMatriceNodiVMGtwd As List(Of Double)
  Dim pMatriceNodiVMGmediana As List(Of Double)
  Dim pMatriceNodiVMGmedianaRec As List(Of Double)

  Dim pMatriceTWAtwd As List(Of Double)
  Dim pMatriceTWAmediana As List(Of Double)
  Dim pMatriceTWAmedianaRec As List(Of Double)

  Public Sub New(Periodo As clsTimeRange)

    pRigaIniziale = DataProvider2020.TrovaIndice(Periodo.Start)
    pRigaFinale = DataProvider2020.TrovaIndice(Periodo.Finish)

    cRotta = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    cSpeed = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
    If cSpeed Is Nothing Then
      cSpeed = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    End If
    cTWD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    cTWS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    cLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    CLong = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)

    Dim ivRotta As New clsStatisticheIntervallo(cRotta, Periodo)
    Dim ivSpeed As New clsStatisticheIntervallo(cSpeed, Periodo)
    Dim ivTwd As New clsStatisticheIntervallo(cTWD, Periodo)
    Dim ivtws As New clsStatisticheIntervallo(cTWS, Periodo)

    pRottaMedia = ivRotta.Avg
    pSpeedMedia = ivSpeed.Avg
    pTWDMedia = ivTwd.Avg
    pTWSMedia = ivtws.Avg
    pSpeedMinima = ivSpeed.Min

  End Sub

  Public Property RottaMedia As Double
    Get
      Return pRottaMedia
    End Get
    Set(value As Double)
      pRottaMedia = value
    End Set
  End Property

  Public Property SpeedMedia As Double
    Get
      Return pSpeedMedia
    End Get
    Set(value As Double)
      pSpeedMedia = value
    End Set
  End Property

  Public Property SpeedMinima As Double
    Get
      Return pSpeedMinima
    End Get
    Set(value As Double)
      pSpeedMinima = value
    End Set
  End Property

  Public ReadOnly Property SpeedMediaMS As Double
    Get
      Return SpeedMedia / 3600 * 1852
    End Get
  End Property

  Public Property TWDMedia As Double
    Get
      Return pTWDMedia
    End Get
    Set(value As Double)
      pTWDMedia = value
    End Set
  End Property

  Public Property TWSMedia As Double
    Get
      Return pTWSMedia
    End Get
    Set(value As Double)
      pTWSMedia = value
    End Set
  End Property

  Public ReadOnly Property TWAmedia As Double
    Get
      ' TWA quale differenza tra la TWD e la Rotta del periodo stesso
      Return DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(RottaMedia, TWDMedia)
    End Get
  End Property

  Public ReadOnly Property VMGmedia() As Double
    Get
      ' VMG calcolata da Speed e TWA del periodo stesso
      Return SpeedMedia * System.Math.Abs(System.Math.Cos(Radians(System.Math.Abs(TWAmedia))))
    End Get
  End Property

  Public ReadOnly Property VMGmedia(AxisRef As Double) As Double
    Get
      ' VMG calcolata da Speed del periodo stesso e la TWA derivata da rotta del periodo ed asse esterno
      Return SpeedMedia * System.Math.Abs(System.Math.Cos(Radians(DifferenzaAssolutaTraAngoli360(pRottaMedia, AxisRef, True))))
    End Get
  End Property

  Public ReadOnly Property VMGmediaMS(AxisRef As Double) As Double
    Get
      Return KtsToMS(VMGmedia(AxisRef))
    End Get
  End Property

  Public ReadOnly Property TWAmedia(AxisRef As Double) As Double
    Get
      ' TWA derivata da rotta del periodo ed asse esterno
      Return DifferenzaAssolutaTraAngoli360(pRottaMedia, AxisRef, True)
    End Get
  End Property

  'Public Property LegPuntoPunto As clsCourseLeg
  '  Get
  '    Return pLegPuntoPunto
  '  End Get
  '  Set(value As clsCourseLeg)
  '    pLegPuntoPunto = value
  '  End Set
  'End Property

  Public ReadOnly Property MatriceTWAtwd As List(Of Double)
    Get
      Return pMatriceTWAtwd
    End Get
  End Property

  Public ReadOnly Property MatriceTWAmediana As List(Of Double)
    Get
      Return pMatriceTWAmediana
    End Get
  End Property

  Public ReadOnly Property MatriceTWAmedianaRec As List(Of Double)
    Get
      Return pMatriceTWAmedianaRec
    End Get
  End Property

  Public ReadOnly Property MatriceMsVMGtwd As List(Of Double)
    Get
      Return pMatriceMsVMGtwd
    End Get
  End Property

  Public ReadOnly Property MatriceMsVMGmediana As List(Of Double)
    Get
      Return pMatriceMsVMGmediana
    End Get
  End Property

  Public ReadOnly Property MatriceMsVMGmedianaRec As List(Of Double)
    Get
      Return pMatriceMsVMGmedianaRec
    End Get
  End Property

  Public ReadOnly Property MatriceNodiVMGtwd As List(Of Double)
    Get
      Return pMatriceNodiVMGtwd
    End Get
  End Property

  Public ReadOnly Property MatriceNodiVMGmediana As List(Of Double)
    Get
      Return pMatriceNodiVMGmediana
    End Get
  End Property

  Public ReadOnly Property MatriceNodiVMGmedianaRec As List(Of Double)
    Get
      Return pMatriceNodiVMGmedianaRec
    End Get
  End Property

  'Public Sub ImpostaMatricTWAandeMetriVMG(AsseRef As clsPavarot.eAxisRef, Asse As Double)
  '  Select Case AsseRef
  '    Case clsPavarot.eAxisRef.eMediana
  '      If pMatriceMsVMGmediana Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGmediana, pMatriceNodiVMGmediana, pMatriceTWAmediana, Asse)
  '      End If
  '    Case clsPavarot.eAxisRef.eMedianaRec
  '      If pMatriceMsVMGmedianaRec Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGmedianaRec, pMatriceNodiVMGmedianaRec, pMatriceTWAmedianaRec, Asse)
  '      End If
  '    Case clsPavarot.eAxisRef.eTWD
  '      If pMatriceMsVMGtwd Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGtwd, pMatriceNodiVMGtwd, pMatriceTWAtwd, Asse)
  '      End If
  '  End Select
  'End Sub

  'Public Sub ImpostaMatricTWAandeMetriVMG(AsseRef As clsPavarot2019.eAxisRef, Asse As Double, IsUpwindLeg As Boolean)
  '  Select Case AsseRef
  '    Case clsPavarot2019.eAxisRef.eMediana
  '      If pMatriceMsVMGmediana Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGmediana, pMatriceNodiVMGmediana, pMatriceTWAmediana, Asse, IsUpwindLeg)
  '      End If
  '    Case clsPavarot2019.eAxisRef.eMedianaVmg
  '      If pMatriceMsVMGmedianaRec Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGmedianaRec, pMatriceNodiVMGmedianaRec, pMatriceTWAmedianaRec, Asse, IsUpwindLeg)
  '      End If
  '    Case clsPavarot2019.eAxisRef.eTWD
  '      If pMatriceMsVMGtwd Is Nothing Then
  '        ImpostaMatrici(pMatriceMsVMGtwd, pMatriceNodiVMGtwd, pMatriceTWAtwd, Asse, IsUpwindLeg)
  '      End If
  '  End Select
  'End Sub

  'Private Sub ImpostaMatrici(ByRef MatriceMsVMG As List(Of Double), ByRef MatriceNodiVMG As List(Of Double), ByRef MatriceTWA As List(Of Double), Asse As Double, IsUpwindLeg As Boolean)
  '  MatriceMsVMG = New List(Of Double)
  '  MatriceNodiVMG = New List(Of Double)
  '  MatriceTWA = New List(Of Double)
  '  Dim segno As Integer = -1
  '  If IsUpwindLeg Then segno = 1
  '  For i As Integer = pRigaIniziale To pRigaFinale
  '    MatriceTWA.Add(DifferenzaAssolutaTraAngoli360(Asse, cRotta.Valori(i), True))
  '    MatriceNodiVMG.Add(cSpeed.Valori(i) * segno * System.Math.Cos(Radians(MatriceTWA.Last)))
  '    MatriceMsVMG.Add(KtsToMS(MatriceNodiVMG.Last))
  '  Next

  'End Sub

  'Private Sub ImpostaMatrici(ByRef MatriceMsVMG As List(Of Double), ByRef MatriceNodiVMG As List(Of Double), ByRef MatriceTWA As List(Of Double), Asse As Double)
  '  MatriceMsVMG = New List(Of Double)
  '  MatriceNodiVMG = New List(Of Double)
  '  MatriceTWA = New List(Of Double)
  '  For i As Integer = pRigaIniziale To pRigaFinale
  '    MatriceTWA.Add(DifferenzaAssolutaTraAngoli360(Asse, cRotta.Valori(i), True))
  '    MatriceNodiVMG.Add(cSpeed.Valori(i) * System.Math.Abs(System.Math.Cos(Radians(MatriceTWA.Last))))
  '    MatriceMsVMG.Add(KtsToMS(MatriceNodiVMG.Last))
  '  Next
  'End Sub


End Class