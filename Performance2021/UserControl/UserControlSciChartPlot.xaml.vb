Imports System.Collections.ObjectModel
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries

Public Class UserControlSciChartPlot

  Public Property VM As New UserControlSciChartPlotViewModel

  Public Sub New()
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

  End Sub



  'Public Shared DP As DependencyProperty = DependencyProperty.Register("PlotType", GetType(Integer), GetType(UserControlSciChartPlot), New PropertyMetadata(1))


  'Public Property PlotType As Integer
  '  Get
  '    Return CType(GetValue(DP), Integer)
  '  End Get
  '  Set(value As Integer)
  '    SetValue(DP, value)
  '  End Set
  'End Property




End Class

<AddINotifyPropertyChangedInterface>
Public Class UserControlSciChartPlotViewModel
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property PuntiStbd As List(Of clsDoubleXY)
  Public Property PuntiPort As List(Of clsDoubleXY)
  Public Property PuntiStbdDn As List(Of clsDoubleXY)
  Public Property PuntiPortDn As List(Of clsDoubleXY)
  Public Property PuntiColored As List(Of clsXYZKpoint)
  Public Property Ascissa As String
  Public Property Ordinata As String
  Public Property Titolo As String
  Public Property SottoTitolo As String
  Public Property SottoTitolo1 As String
  Public Property SottoTitolo2 As String
  Public Property PlotType As ePlotType
  Public Property TR As clsTimeRange
  Dim Intervalli As Integer = 20
  Public Property TL As New clsTrendLines


  Public Enum ePlotType
    eD_Tws = 1
    eD_Twd = 2
    eD_UpVmgTgtP = 3
    eD_DnVmgTgtP = 4
    eD_UpBsTgtP = 5
    eD_DnBsTgtP = 6
    eD_UpTwaD = 7
    eD_DnTwaD = 8
    eXY_TwsTwa = 9
    eXY_BspTwa = 10
    eXY_UpVmg = 11
    eXY_UpBs = 12
    eXY_UpTwa = 13
    eXY_DnVmg = 14
    eXY_DnBs = 15
    eXY_DnTwa = 16
    eTacks = 17
    eGybes = 18
    eD_PolarPerc = 19
    eD_SeaState = 20
  End Enum


  Public Sub AggiornaGrafico(TR As clsTimeRange)
    Dim OrdineTrendLines As Integer = AppConfig.ActiveProfile.SummarySettings.TrendLineOrder
    Me.TR = TR
    ImpostaPunti()
    Select Case PlotType
      Case ePlotType.eD_Tws, ePlotType.eD_Twd, ePlotType.eD_UpBsTgtP, ePlotType.eD_UpVmgTgtP, ePlotType.eD_UpTwaD, ePlotType.eD_DnBsTgtP, ePlotType.eD_DnVmgTgtP, ePlotType.eD_DnTwaD, ePlotType.eD_PolarPerc, ePlotType.eD_SeaState
        DisegnaPuntiDistribution()
      Case ePlotType.eXY_BspTwa
        DisegnaPuntiXY()
      Case ePlotType.eXY_TwsTwa
        DisegnaPuntiXYperformance()
        StampaTarget()
      Case ePlotType.eXY_UpVmg, ePlotType.eXY_UpBs, ePlotType.eXY_UpTwa
        DisegnaPuntiXY()
        TL.StampaTrendLine(True, True, "TL_UpStbd", OrdineTrendLines, SeriesSource, OrdineTrendLines > 0)
        TL.StampaTrendLine(True, False, "TL_UpPort", OrdineTrendLines, SeriesSource, OrdineTrendLines > 0)
        StampaTargetTws()
      Case ePlotType.eXY_DnVmg, ePlotType.eXY_DnBs, ePlotType.eXY_DnTwa
        DisegnaPuntiXY()
        TL.StampaTrendLine(False, True, "TL_DnStbd", OrdineTrendLines, SeriesSource, OrdineTrendLines > 0)
        TL.StampaTrendLine(False, False, "TL_DnPort", OrdineTrendLines, SeriesSource, OrdineTrendLines > 0)
        StampaTargetTws()
      Case ePlotType.eTacks, ePlotType.eGybes
        DisegnaPuntiXYpavarot()
        StampaBenchmarks(PlotType = ePlotType.eTacks)
    End Select

  End Sub


  Public Function PointsXYZ_X(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As Double()
    Return PointsXYZ(XMin, XMax, YMin, YMax).Select(Function(x) x.X).ToArray
  End Function

  Public Function PointsXYZ_Y(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As Double()
    Return PointsXYZ(XMin, XMax, YMin, YMax).Select(Function(x) x.Y).ToArray
  End Function

  Public Function PointsXYZ_Z(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As Double()
    Return PointsXYZ(XMin, XMax, YMin, YMax).Select(Function(x) x.Z).ToArray
  End Function

  Public Function PointsXYZ_K(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As Double()
    Return PointsXYZ(XMin, XMax, YMin, YMax).Select(Function(x) x.K).ToArray
  End Function

  Public Function PointsXYZ_Color(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As Color()
    Return PointsXYZ(XMin, XMax, YMin, YMax).Select(Function(x) x.Color).ToArray
  End Function

  Public Function PointsXYZ(XMin As Double, XMax As Double, YMin As Double, YMax As Double) As clsXYZKpoint()
    Return PuntiColored.Where(Function(x) x.X >= XMin And x.X < XMax And x.Y >= YMin And x.Y < YMax).ToArray
  End Function

  Public Function Points_X(XMin As Double, XMax As Double) As Double()
    Dim s = StbdPoints_X(XMin, XMax).ToList
    If Not PuntiPort Is Nothing Then
      Dim p = PortPoints_X(XMin, XMax).ToList
      s.AddRange(p)
    End If
    Return s.ToArray
  End Function

  Public Function StbdPoints_X(XMin As Double, XMax As Double) As Double()
    Return StbdPoints(XMin, XMax).Select(Function(x) x.X).ToArray
  End Function

  Public Function PortPoints_X(XMin As Double, XMax As Double) As Double()
    Return PortPoints(XMin, XMax).Select(Function(x) x.X).ToArray
  End Function

  Public Function Points_Y(XMin As Double, XMax As Double) As Double()
    Dim s = StbdPoints_Y(XMin, XMax).ToList
    If Not PuntiPort Is Nothing Then
      Dim p = PortPoints_Y(XMin, XMax).ToList
      s.AddRange(p)
    End If
    Return s.ToArray
  End Function

  Public Function StbdPoints_Y(XMin As Double, XMax As Double) As Double()
    Return StbdPoints(XMin, XMax).Select(Function(x) x.Y).ToArray
  End Function

  Public Function PortPoints_Y(XMin As Double, XMax As Double) As Double()
    Return PortPoints(XMin, XMax).Select(Function(x) x.Y).ToArray
  End Function

  Public Function StbdPoints(XMin As Double, XMax As Double) As clsDoubleXY()
    Return PuntiStbd.Where(Function(x) x.X >= XMin And x.X < XMax).ToArray
  End Function

  Public Function PortPoints(XMin As Double, XMax As Double) As clsDoubleXY()
    Return PuntiPort.Where(Function(x) x.X >= XMin And x.X < XMax).ToArray
  End Function

  Private Sub ImpostaPunti()
    PuntiStbd = New List(Of clsDoubleXY)
    PuntiPort = Nothing
    Select Case PlotType
      Case ePlotType.eD_Tws
        PuntiPort = New List(Of clsDoubleXY)
        Titolo = "True Wind Speed"
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not ch Is Nothing Then
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim v = ch.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim a = chTwa.Valori(i)
              If Math.Abs(a) > 90 Then
                PuntiPort.Add(New clsDoubleXY(v, v))
              Else
                PuntiStbd.Add(New clsDoubleXY(v, v))
              End If
              'PuntiStbd.Add(New clsDoubleXY(v, v))
            End If
          Next
        End If
      Case ePlotType.eD_Twd
        PuntiPort = New List(Of clsDoubleXY)
        Titolo = "True Wind Direction"
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not ch Is Nothing Then
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim v = ch.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim a = chTwa.Valori(i)
              If Math.Abs(a) > 90 Then
                PuntiPort.Add(New clsDoubleXY(v, v))
              Else
                PuntiStbd.Add(New clsDoubleXY(v, v))
              End If
            End If
          Next
        End If
      'Case ePlotType.eD_Twd
      '  Titolo = "True Wind Direction"
      '  Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
      '  Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      '  If Not ch Is Nothing AndAlso Not chTwa Is Nothing Then
      '    PuntiPort = New List(Of clsDoubleXY)
      '    PuntiStbdDn = New List(Of clsDoubleXY)
      '    PuntiPortDn = New List(Of clsDoubleXY)
      '    For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
      '      Dim twa = chTwa.Valori(i)
      '      Dim v = ch.Valori(i)
      '      If Not Double.IsNaN(twa) AndAlso Not Double.IsNaN(v) Then
      '        If Math.Abs(twa) > 90 Then
      '          If twa >= 0 Then
      '            PuntiStbdDn.Add(New clsDoubleXY(v, v))
      '          Else
      '            PuntiPortDn.Add(New clsDoubleXY(v, v))
      '          End If
      '        Else
      '          If twa >= 0 Then
      '            PuntiStbd.Add(New clsDoubleXY(v, v))
      '          Else
      '            PuntiPort.Add(New clsDoubleXY(v, v))
      '          End If
      '        End If
      '      End If
      '    Next
      '  End If
      Case ePlotType.eD_PolarPerc
        Titolo = "StraightLine Bs Polar%"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing Then
          'PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            Dim v = ch.Valori(i)
            Dim tws = chTws.Valori(i)
            Dim ss = chSS.Valori(i)
            If Not Double.IsNaN(twa) AndAlso Not Double.IsNaN(v) AndAlso Not Double.IsNaN(tws) AndAlso Not Double.IsNaN(ss) Then
              If IsValidStraightLine(ss) Then
                PuntiStbd.Add(New clsDoubleXY(tws, Math.Min(130, v)))
              End If
            End If
          Next
        End If
      Case ePlotType.eD_SeaState
        Titolo = "Upwind SeaState"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim v = ch.Valori(i)
            Dim tws = chTws.Valori(i)
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(v) AndAlso Not Double.IsNaN(tws) AndAlso Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 70 Then
                If twa >= 0 Then
                  PuntiStbd.Add(New clsDoubleXY(tws, v))
                Else
                  PuntiPort.Add(New clsDoubleXY(tws, v))
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_UpVmgTgtP
        Titolo = "Upwind VmgTgt%"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_DnVmgTgtP
        Titolo = "Downwind VmgTgt%"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_UpBsTgtP
        Titolo = "Upwind BsTgt%"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_DnBsTgtP
        Titolo = "Downwind BsTgt%"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_UpTwaD
        Titolo = "Upwind TwaTgtD"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eD_DnTwaD
        Titolo = "Downwind TwaTgtD"
        Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chTws.Valori(i), ch.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_TwsTwa
        Titolo = "StraightLineNotVmg TwsVsTwa ColorByPol%"
        Ascissa = "Twa"
        Ordinata = "Tws"
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chPerf = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
        Dim chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        If Not chPerf Is Nothing AndAlso Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing AndAlso Not chBs Is Nothing Then
          PuntiColored = New List(Of clsXYZKpoint)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(chPerf.Valori(i)) Then
              If IsValidReaching(chSS.Valori(i)) Then
                PuntiColored.Add(New clsXYZKpoint(Math.Abs(chTwa.Valori(i)), ch.Valori(i), chPerf.Valori(i), chBs.Valori(i), ColoreDaPerformance(chPerf.Valori(i))))
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_BspTwa
        Titolo = "StraightLineNotVmg Polar%VsTwa"
        Ascissa = "Twa"
        Ordinata = "BsPolar%"
        Dim ch = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        If Not ch Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If IsValidReaching(chSS.Valori(i)) Then
                If twa >= 0 Then
                  PuntiStbd.Add(New clsDoubleXY(Math.Abs(chTwa.Valori(i)), Math.Min(130, ch.Valori(i))))
                Else
                  PuntiPort.Add(New clsDoubleXY(Math.Abs(chTwa.Valori(i)), Math.Min(130, ch.Valori(i))))
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_UpVmg
        Titolo = "Upwind Vmg"
        Ascissa = "Tws"
        Ordinata = "Vmg"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_UpBs
        Titolo = "Upwind Bs"
        Ascissa = "Tws"
        Ordinata = "Bs"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_UpTwa
        Titolo = "Upwind Twa"
        Ascissa = "Tws"
        Ordinata = "Twa"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) <= 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), Math.Abs(chY.Valori(i))))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), Math.Abs(chY.Valori(i))))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_DnVmg
        Titolo = "Downwind Vmg"
        Ascissa = "Tws"
        Ordinata = "Vmg"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_DnBs
        Titolo = "Downwind Bs"
        Ascissa = "Tws"
        Ordinata = "Bs"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), chY.Valori(i)))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eXY_DnTwa
        Titolo = "Downwind Twa"
        Ascissa = "Tws"
        Ordinata = "Twa"
        Dim chX = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chY = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSS = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
        Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If Not chTwa Is Nothing AndAlso Not chX Is Nothing AndAlso Not chY Is Nothing AndAlso Not chSS Is Nothing Then
          PuntiPort = New List(Of clsDoubleXY)
          For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
            Dim twa = chTwa.Valori(i)
            If Not Double.IsNaN(twa) Then
              If Math.Abs(twa) > 90 Then
                If chSS.Valori(i) = clsPeriodsManager2021.eRowType.eStraightLineVmg Then
                  If twa >= 0 Then
                    PuntiStbd.Add(New clsDoubleXY(chX.Valori(i), Math.Abs(chY.Valori(i))))
                  Else
                    PuntiPort.Add(New clsDoubleXY(chX.Valori(i), Math.Abs(chY.Valori(i))))
                  End If
                End If
              End If
            End If
          Next
        End If
      Case ePlotType.eTacks
        Titolo = "Tacks Loss Inline BL"
        Ascissa = "Tws"
        Ordinata = "BL"
        Dim periodi = PeriodsManager.Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).Where(Function(x) x.TR.OverlappedWith(TR)).ToList
        PuntiPort = New List(Of clsDoubleXY)
        For Each p In periodi
          If p.IsStbd >= 0 Then
            PuntiStbd.Add(New clsDoubleXY(p.TwsDetails.AvgVal, p.PavarotDetails.InlineTgtLossBL))
          Else
            PuntiPort.Add(New clsDoubleXY(p.TwsDetails.AvgVal, p.PavarotDetails.InlineTgtLossBL))
          End If
        Next
      Case ePlotType.eGybes
        Titolo = "Gybes Loss Inline BL"
        Ascissa = "Tws"
        Ordinata = "BL"
        Dim periodi = PeriodsManager.Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe).Where(Function(x) x.TR.OverlappedWith(TR)).ToList
        PuntiPort = New List(Of clsDoubleXY)
        For Each p In periodi
          If p.IsStbd >= 0 Then
            PuntiStbd.Add(New clsDoubleXY(p.TwsDetails.AvgVal, p.PavarotDetails.InlineTgtLossBL))
          Else
            PuntiPort.Add(New clsDoubleXY(p.TwsDetails.AvgVal, p.PavarotDetails.InlineTgtLossBL))
          End If
        Next
      Case Else
        Stop
    End Select
  End Sub

  Private Function IsValidReaching(Valore As Double) As Boolean
    Select Case Valore
      Case clsPeriodsManager2021.eRowType.eStraightLineVmg
        Return False
      Case clsPeriodsManager2021.eRowType.eBearAway, clsPeriodsManager2021.eRowType.eRoundUp
        Return False
      Case clsPeriodsManager2021.eRowType.eTack, clsPeriodsManager2021.eRowType.eGybe
        Return False
      Case Else
        Return True
    End Select
  End Function

  Private Function IsValidStraightLine(Valore As Double) As Boolean
    Select Case Valore
      Case clsPeriodsManager2021.eRowType.eBearAway, clsPeriodsManager2021.eRowType.eRoundUp
        Return False
      Case clsPeriodsManager2021.eRowType.eTack, clsPeriodsManager2021.eRowType.eGybe
        Return False
      Case Else
        Return True
    End Select
  End Function


  Private Sub DisegnaPuntiDistribution()
    If Not PuntiPortDn Is Nothing Then
      DisegnaPuntiDistributionStbdPortUpDn()
      Exit Sub
    End If


    SeriesSource.Clear()

    Dim min As Double
    Dim max As Double
    Dim d As Double
    Dim h As Double
    Dim vx As Double
    Dim vy As Double
    Dim Tot As Double


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim LineaStbd As New FastMountainRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    If PuntiPort Is Nothing Then
      LineaStbd.Stroke = Colors.Blue
    Else
      If PlotType = ePlotType.eD_Twd OrElse PlotType = ePlotType.eD_Tws Then
        LineaStbd.Stroke = Colors.Gold
      Else
        LineaStbd.Stroke = Colors.Green
      End If

    End If
    LineaStbd.StrokeThickness = 2
    Dim Colore As New SolidColorBrush(LineaStbd.Stroke)
    Colore.Opacity = 0.2
    LineaStbd.Fill = Colore
    DataSeriesStbd.SeriesName = ""
    DataSeriesStbd.AcceptsUnsortedData = False
    Dim xx As New List(Of Double)
    Dim yy As New List(Of Double)
    Dim Vals = PuntiStbd.Where(Function(x) Not Double.IsNaN(x.X) AndAlso Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToList



    If Vals.Count > 0 Then
      min = Vals.Min
      max = Vals.Max
      d = max - min
      h = d / Intervalli
      If h > 0 Then
        vx = min - h
        vy = 0
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
        Tot = yy.Sum
        For ii As Integer = 0 To yy.Count - 1
          yy(ii) = yy(ii) / Tot * 100
          DataSeriesStbd.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
        Next
        LineaStbd.DataSeries = DataSeriesStbd
        Dim CSVMstbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
        SeriesSource.Add(CSVMstbd)
      End If
    End If

    If Not PuntiPort Is Nothing AndAlso PuntiPort.Count > 0 Then
      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
      Dim LineaPort As New FastMountainRenderableSeries
      LineaPort.XAxisId = "DefaultAxisId"
      LineaPort.YAxisId = "DefaultAxisId"
      If PlotType = ePlotType.eD_Twd OrElse PlotType = ePlotType.eD_Tws Then
        LineaPort.Stroke = Colors.Blue
      Else
        LineaPort.Stroke = Colors.Red
      End If
      LineaPort.StrokeThickness = 2
      Colore = New SolidColorBrush(LineaPort.Stroke)
      Colore.Opacity = 0.2
      LineaPort.Fill = Colore
      DataSeriesPort.SeriesName = ""
      DataSeriesPort.AcceptsUnsortedData = False
      xx = New List(Of Double)
      yy = New List(Of Double)
      Vals = PuntiPort.Where(Function(x) Not Double.IsNaN(x.X) AndAlso Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToList
      If Vals.Count > 0 Then
        min = Vals.Min
        max = Vals.Max
        d = max - min
        h = d / Intervalli
        vx = min - h
        vy = 0
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
        Tot = yy.Sum
        For ii As Integer = 0 To yy.Count - 1
          yy(ii) = yy(ii) / Tot * 100
          DataSeriesPort.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
        Next
        LineaPort.DataSeries = DataSeriesPort
        Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
        SeriesSource.Add(CSVMport)
      End If
    End If

  End Sub

  Private Sub DisegnaPuntiDistributionStbdPortUpDn()
    SeriesSource.Clear()
    Dim Punti As New Dictionary(Of Color, List(Of clsDoubleXY))
    If PlotType = ePlotType.eD_Twd OrElse PlotType = ePlotType.eD_Tws Then
      Punti.Add(Colors.Gold, PuntiStbd)
      Punti.Add(Colors.LightSkyBlue, PuntiPort)
    Else
      Punti.Add(Colors.Green, PuntiStbd)
      Punti.Add(Colors.Red, PuntiPort)
    End If
    Punti.Add(Colors.YellowGreen, PuntiStbdDn)
    Punti.Add(Colors.OrangeRed, PuntiPortDn)

    For Each SeriePunti In Punti
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastMountainRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Stroke = SeriePunti.Key
      Linea.StrokeThickness = 2
      Dim Colore As New SolidColorBrush(Linea.Stroke)
      Colore.Opacity = 0.2
      Linea.Fill = Colore
      DataSeries.SeriesName = ""
      DataSeries.AcceptsUnsortedData = False
      Dim xx As New List(Of Double)
      Dim yy As New List(Of Double)
      'Dim Vals = SeriePunti.Value.Select(Function(x) x.Y).ToList
      Dim Vals = SeriePunti.Value.Where(Function(x) Not Double.IsNaN(x.X) AndAlso Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToList
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
        DataSeries.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
      Next
      Linea.DataSeries = DataSeries
      Dim CSVMstbd As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVMstbd)
    Next



  End Sub

  Private Sub DisegnaPuntiXYpavarot()
    SeriesSource.Clear()

    For Each punto In PuntiStbd
      Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
      Dim LineaStbd As New XyScatterRenderableSeries
      LineaStbd.XAxisId = "DefaultAxisId"
      LineaStbd.YAxisId = "DefaultAxisId"
      LineaStbd.PointMarker = New EllipsePointMarker
      Dim c As Color = Colors.Green
      c.A = 150
      LineaStbd.PointMarker.Stroke = Colors.Green
      LineaStbd.PointMarker.Width = 16
      LineaStbd.PointMarker.Height = 16
      LineaStbd.PointMarker.StrokeThickness = 2
      LineaStbd.PointMarker.Fill = c
      DataSeriesStbd.SeriesName = ""
      DataSeriesStbd.AcceptsUnsortedData = True
      DataSeriesStbd.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
      LineaStbd.DataSeries = DataSeriesStbd
      Dim CSVMstbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
      SeriesSource.Add(CSVMstbd)
    Next

    For Each punto In PuntiPort
      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
      Dim LineaPort As New XyScatterRenderableSeries
      LineaPort.XAxisId = "DefaultAxisId"
      LineaPort.YAxisId = "DefaultAxisId"
      LineaPort.PointMarker = New EllipsePointMarker
      Dim c As Color = Colors.Red
      c.A = 150
      LineaPort.PointMarker.Stroke = Colors.Red
      LineaPort.PointMarker.Width = 16
      LineaPort.PointMarker.Height = 16
      LineaPort.PointMarker.StrokeThickness = 2
      LineaPort.PointMarker.Fill = c
      DataSeriesPort.SeriesName = ""
      DataSeriesPort.AcceptsUnsortedData = True
      DataSeriesPort.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
      LineaPort.DataSeries = DataSeriesPort
      Dim CSVMPort As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
      SeriesSource.Add(CSVMPort)
    Next

  End Sub

  Private Sub DisegnaPuntiXYperformance()
    SeriesSource.Clear()
    Dim Colori As New List(Of Color)
    For Each p In PuntiColored
      If Not Colori.Contains(p.Color) Then
        Colori.Add(p.Color)
      End If
    Next

    For Each colore In Colori
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New XyScatterRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.PointMarker = New EllipsePointMarker
      Dim c As Color = colore
      'c = Colors.Violet
      c.A = 100
      Linea.PointMarker.Stroke = c
      Linea.PointMarker.Width = 4
      Linea.PointMarker.StrokeThickness = 0
      Linea.PointMarker.Fill = Linea.PointMarker.Stroke
      DataSeries.SeriesName = ""
      DataSeries.AcceptsUnsortedData = True
      For Each punto In PuntiColored
        If punto.Color = colore Then
          DataSeries.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
        End If
      Next
      Linea.DataSeries = DataSeries
      Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
      SeriesSource.Add(CSVM)
      'Exit For
    Next

  End Sub

  Private Function IsUpwindPlot() As Boolean
    Select Case PlotType
      Case ePlotType.eXY_UpBs, ePlotType.eXY_UpTwa, ePlotType.eXY_UpVmg, ePlotType.eTacks, ePlotType.eD_UpBsTgtP, ePlotType.eD_UpTwaD, ePlotType.eD_UpVmgTgtP
        Return True
      Case Else
        Return False
    End Select
  End Function

  Private Sub DisegnaPuntiXY()
    SeriesSource.Clear()
    TL.ClearData()
    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim LineaStbd As New XyScatterRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker
    Dim c As Color = Colors.Green
    If PuntiPort Is Nothing Then
      c = Colors.Blue
    End If
    c.A = 100
    LineaStbd.PointMarker.Stroke = c
    LineaStbd.PointMarker.Width = 4
    LineaStbd.PointMarker.StrokeThickness = 0
    LineaStbd.PointMarker.Fill = LineaStbd.PointMarker.Stroke
    DataSeriesStbd.SeriesName = ""
    DataSeriesStbd.AcceptsUnsortedData = True
    For Each punto In PuntiStbd
      DataSeriesStbd.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
      TL.AccodaCoppia(IsUpwindPlot, True, punto.X, punto.Y)
    Next
    LineaStbd.DataSeries = DataSeriesStbd
    Dim CSVMstbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
    SeriesSource.Add(CSVMstbd)

    If Not PuntiPort Is Nothing AndAlso PuntiPort.Count > 0 Then
      Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
      Dim LineaPort As New XyScatterRenderableSeries
      LineaPort.XAxisId = "DefaultAxisId"
      LineaPort.YAxisId = "DefaultAxisId"
      LineaPort.PointMarker = New EllipsePointMarker
      c = Colors.Red
      c.A = 100
      LineaPort.PointMarker.Stroke = c
      LineaPort.PointMarker.Width = 4
      LineaPort.PointMarker.StrokeThickness = 0
      LineaPort.PointMarker.Fill = LineaPort.PointMarker.Stroke
      DataSeriesPort.SeriesName = ""
      DataSeriesPort.AcceptsUnsortedData = True
      For Each punto In PuntiPort
        DataSeriesPort.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
        TL.AccodaCoppia(IsUpwindPlot, False, punto.X, punto.Y)
      Next
      LineaPort.DataSeries = DataSeriesPort
      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
      SeriesSource.Add(CSVMport)
    End If

  End Sub

  Public Sub StampaTarget()
    If TgtManager Is Nothing Then Exit Sub
    If TgtManager.Tgt Is Nothing Then Exit Sub

    Dim DataSeriesTgtUp As New XyDataSeries(Of Double, Double)
    Dim LineaTgtUp As New FastLineRenderableSeries
    LineaTgtUp.XAxisId = "DefaultAxisId"
    LineaTgtUp.YAxisId = "DefaultAxisId"
    LineaTgtUp.Stroke = Colors.DeepSkyBlue
    LineaTgtUp.StrokeThickness = 3
    'LineaTgtUp.StrokeDashArray = {3, 3}
    LineaTgtUp.Tag = "TgtUp"
    LineaTgtUp.IsVisible = True
    DataSeriesTgtUp.AcceptsUnsortedData = True
    DataSeriesTgtUp.SeriesName = LineaTgtUp.Tag

    Dim DataSeriesTgtDn As New XyDataSeries(Of Double, Double)
    Dim LineaTgtDn As New FastLineRenderableSeries
    LineaTgtDn.XAxisId = "DefaultAxisId"
    LineaTgtDn.YAxisId = "DefaultAxisId"
    LineaTgtDn.Stroke = Colors.DarkBlue
    LineaTgtDn.StrokeThickness = 3
    'LineaTgtDn.StrokeDashArray = {3, 3}
    LineaTgtDn.Tag = "TgtDn"
    LineaTgtDn.IsVisible = True
    DataSeriesTgtDn.AcceptsUnsortedData = True
    DataSeriesTgtDn.SeriesName = LineaTgtDn.Tag

    Dim MaxY As Double = SeriesSource.Max(Function(x) x.DataSeries.YMax)
    Dim MinY As Double = SeriesSource.Min(Function(x) x.DataSeries.YMin)
    If Not MaxY = MinY Then
      For tws As Integer = Int(MinY) To (Int(MaxY) + 1)
        Dim twaUp As Double = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Twa
        Dim twaDn As Double = TgtManager.Tgt.ValoreTgtDn(tws, "bs").Twa
        DataSeriesTgtUp.Append(twaUp, tws, New clsPuntoMetadata(False))
        DataSeriesTgtDn.Append(twaDn, tws, New clsPuntoMetadata(False))
      Next
      LineaTgtUp.DataSeries = DataSeriesTgtUp
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTgtUp, LineaTgtUp)
      SeriesSource.Add(CSVMtgtup)
      LineaTgtDn.DataSeries = DataSeriesTgtDn
      Dim CSVMtgtDn As New ChartSeriesViewModel(DataSeriesTgtDn, LineaTgtDn)
      SeriesSource.Add(CSVMtgtDn)
    End If


  End Sub

  Public Sub StampaBenchmarks(IsUp As Boolean)

    Dim DataSeriesBench As New XyDataSeries(Of Double, Double)
    Dim LineaBench As New FastLineRenderableSeries
    LineaBench.XAxisId = "DefaultAxisId"
    LineaBench.YAxisId = "DefaultAxisId"
    LineaBench.Stroke = Colors.DeepSkyBlue
    LineaBench.StrokeThickness = 3
    LineaBench.Tag = "Benchmark"
    LineaBench.IsVisible = True
    DataSeriesBench.AcceptsUnsortedData = True
    DataSeriesBench.SeriesName = LineaBench.Tag


    Dim MaxTws As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
    Dim MinTws As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
    If Not MaxTws = MinTws Then
      Dim b As clsBenchmark = BenchManager.GetGybeLoss
      If IsUp Then b = BenchManager.GetTackLoss
      If Not b Is Nothing Then
        Dim elementi = b.Values.OrderBy(Function(x) x.X).ToList
        For e As Integer = 0 To elementi.Count - 1
          Dim nx As Integer = Math.Min(e + 1, elementi.Count - 1)
          Dim pv As Integer = Math.Max(e - 1, 0)
          If elementi(nx).X >= MinTws And elementi(pv).X <= MaxTws Then
            DataSeriesBench.Append(elementi(e).X, elementi(e).Y / AppConfig.ActiveProfile.BoatLenghtInMeters)
          End If
        Next
      End If

      'For tws As Integer = Int(MinY) To (Int(MaxY) + 1)
      '  Dim twaUp As Double = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Twa
      '  DataSeriesBench.Append(twaUp, tws, New clsPuntoMetadata(False))
      'Next
      LineaBench.DataSeries = DataSeriesBench
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesBench, LineaBench)
      SeriesSource.Add(CSVMtgtup)
    End If


  End Sub

  Public Sub StampaTargetTws()
    If TgtManager Is Nothing Then Exit Sub
    If TgtManager.Tgt Is Nothing Then Exit Sub

    Dim DataSeriesTgt As New XyDataSeries(Of Double, Double)
    Dim LineaTgt As New FastLineRenderableSeries
    LineaTgt.XAxisId = "DefaultAxisId"
    LineaTgt.YAxisId = "DefaultAxisId"
    LineaTgt.Stroke = Colors.DeepSkyBlue
    LineaTgt.StrokeThickness = 3
    'LineaTgt.StrokeDashArray = {3, 3}
    LineaTgt.Tag = "TgtUp"
    LineaTgt.IsVisible = True
    DataSeriesTgt.AcceptsUnsortedData = True
    DataSeriesTgt.SeriesName = LineaTgt.Tag

    If PuntiPort.Count + PuntiStbd.Count < 10 Then Exit Sub
    Dim MaxY As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
    Dim MinY As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
    If Not MaxY = MinY Then
      For tws As Integer = Int(MinY) To (Int(MaxY) + 1)
        Dim v As Double
        Select Case PlotType
          Case ePlotType.eXY_UpVmg
            v = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Vmg
          Case ePlotType.eXY_UpBs
            v = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Bs
          Case ePlotType.eXY_UpTwa
            v = TgtManager.Tgt.ValoreTgtUp(tws, "bs").Twa
          Case ePlotType.eXY_DnVmg
            v = TgtManager.Tgt.ValoreTgtDn(tws, "bs").Vmg
          Case ePlotType.eXY_DnBs
            v = TgtManager.Tgt.ValoreTgtDn(tws, "bs").Bs
          Case ePlotType.eXY_DnTwa
            v = TgtManager.Tgt.ValoreTgtDn(tws, "bs").Twa
          Case Else
            v = 0
        End Select
        DataSeriesTgt.Append(tws, v, New clsPuntoMetadata(False))
      Next
      LineaTgt.DataSeries = DataSeriesTgt
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTgt, LineaTgt)
      SeriesSource.Add(CSVMtgtup)
    End If


  End Sub

End Class


