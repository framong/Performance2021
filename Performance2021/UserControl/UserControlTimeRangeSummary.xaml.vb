Imports System.Collections.ObjectModel
Imports Newtonsoft.Json
Imports PropertyChanged
Imports SciChart.Core.Extensions


Public Class BoatStatusIndex

  Public IdRiga As Integer
  Public DT As DateTime

  Public IsTakingPenalty As Boolean = False
  Public IsMarkApproach As Boolean = False
  Public IsLegFirstMinute As Boolean = False
  Public IsRotating As Boolean = False
  Public IsAroundRotation As Boolean = False
  Public IsChangeOfTack As Boolean = False
  Public IsTacking As Boolean = False
  Public IsGybing As Boolean = False
  Public IsStbd As Boolean = False

  Public Angle As SummaryReport.Andatura
  Public LegIndex As Double
  Public IsDogLeg As Boolean = False

  Public HasValidData As Boolean = False


  Public ReadOnly Property IsPerformanceSailing() As Boolean
    Get
      If IsTakingPenalty Then Return False
      If IsMarkApproach Then Return False
      If IsLegFirstMinute Then Return False
      If IsAroundRotation Then Return False
      If IsTacking Then Return False
      If IsGybing Then Return False
      Return True
    End Get
  End Property
  Public ReadOnly Property IsPerformanceSailing(Andatura As SummaryReport.Andatura) As Boolean
    Get
      If Not Angle = Andatura Then Return False
      If IsTakingPenalty Then Return False
      If IsMarkApproach Then Return False
      If IsLegFirstMinute Then Return False
      If IsAroundRotation Then Return False
      If IsTacking Then Return False
      If IsGybing Then Return False
      Return True
    End Get
  End Property

  Public Sub New(RowIndex As Integer, Momento As DateTime)
    IdRiga = RowIndex
    DT = Momento
  End Sub


End Class


Public Class ChannelStats
  Public Property Channel As clsChannel2020
  Public Property Avg As Double
  Public Property MaxVal As Double
  Public Property MinVal As Double
  Public Property PAvg As Double
  Public Property PMaxVal As Double
  Public Property PMinVal As Double
  Public Property SAvg As Double
  Public Property SMaxVal As Double
  Public Property SMinVal As Double
End Class

Public Class ChannelArrays
  Public Both As New List(Of Double)
  Public Port As New List(Of Double)
  Public Stbd As New List(Of Double)
End Class

Public Class LegDataDetails
  Public Property Tws As New ChannelStats
  Public Property Twd As New ChannelStats
  Public Property Bs As New ChannelStats
  Public Property BsTp As New ChannelStats
  Public Property Twa As New ChannelStats
  Public Property TwaD As New ChannelStats
  Public Property VmgP As New ChannelStats
  Public Property RecVmgP As New ChannelStats
  Public Property ExtraChannels As New List(Of ChannelStats)

  Public Sub UpdateStats(Andatura As SummaryReport.Andatura, Indici As List(Of Integer), ChannelsUp As List(Of clsChannel2020), ChannelsRc As List(Of clsChannel2020), ChannelsDn As List(Of clsChannel2020))
    If (Indici.Count = 0) Then Exit Sub
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chVmgP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    Dim chBsTp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
    Dim chTwaD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)

    Tws.Channel = chTws
    Twd.Channel = chTwd
    Twa.Channel = chTwa
    Bs.Channel = chBs
    TwaD.Channel = chTwaD
    BsTp.Channel = chBsTp
    VmgP.Channel = chVmgP

    Dim CA = ValoriNotNan(Indici, chTws, chTwa)
    SetChannelStats(CA, Tws)

    CA = ValoriNotNan(Indici, chTwd, chTwa)
    SetChannelStats360(CA, Twd)

    CA = ValoriNotNan(Indici, chBs, chTwa)
    SetChannelStats(CA, Bs)

    CA = ValoriNotNan(Indici, chTwa, chTwa)
    SetChannelStats180(CA, Twa)

    CA = ValoriNotNan(Indici, chBsTp, chTwa)
    SetChannelStats(CA, BsTp)

    CA = ValoriNotNan(Indici, chTwaD, chTwa)
    SetChannelStats(CA, TwaD)

    CA = ValoriNotNan(Indici, chVmgP, chTwa)
    SetChannelStats(CA, VmgP)

    Dim Upwindleg As Boolean = Andatura = SummaryReport.Andatura.UpwindVmg
    Dim Tgt = TgtManager.Tgt.ValoreTgt(Upwindleg, Tws.Avg, "bs")
    Dim TgtP = TgtManager.Tgt.ValoreTgt(Upwindleg, Tws.PAvg, "bs")
    Dim TgtS = TgtManager.Tgt.ValoreTgt(Upwindleg, Tws.SAvg, "bs")


    RecVmgP.Avg = (Bs.Avg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.MinVal = (Bs.MinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.MaxVal = (Bs.MaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.PAvg = (Bs.PAvg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.PMinVal = (Bs.PMinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.PMaxVal = (Bs.PMaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.SAvg = (Bs.SAvg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.SMinVal = (Bs.SMinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100
    RecVmgP.SMaxVal = (Bs.SMaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg * 100



    Dim Channels As New List(Of clsChannel2020)
    Select Case Andatura
      Case SummaryReport.Andatura.UpwindVmg
        Channels = ChannelsUp
      Case SummaryReport.Andatura.Reaching
        Channels = ChannelsRc
      Case SummaryReport.Andatura.DownWindVmg
        Channels = ChannelsDn
    End Select




    ExtraChannels.Clear()
    For Each channel In Channels
      Dim c = DataProvider2020.CanaleDbl(channel.ChannelId)
      If Not c Is Nothing AndAlso c.Valori.Count > 0 Then
        CA = ValoriNotNan(Indici, channel, chTwa)
        Dim nc = New ChannelStats
        nc.Channel = channel
        ExtraChannels.Add(nc)
        If channel.DataType = clsChannel2020.eDataType.e360 Then
          SetChannelStats360(CA, ExtraChannels.Last)
        Else
          SetChannelStats(CA, ExtraChannels.Last)
        End If
      End If
    Next



  End Sub

  Public Sub UpdateStats(Indici As List(Of Integer))
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chVmgP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    Dim chBsTp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
    Dim chTwaD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)

    Tws.Channel = chTws
    Twd.Channel = chTwd
    Twa.Channel = chTwa
    Bs.Channel = chBs
    TwaD.Channel = chTwaD
    BsTp.Channel = chBsTp
    VmgP.Channel = chVmgP

    Dim CA = ValoriNotNan(Indici, chTws, chTwa)
    SetChannelStats(CA, Tws)

    CA = ValoriNotNan(Indici, chTwd, chTwa)
    SetChannelStats360(CA, Twd)

    CA = ValoriNotNan(Indici, chBs, chTwa)
    SetChannelStats(CA, Bs)

    CA = ValoriNotNan(Indici, chTwa, chTwa)
    SetChannelStats(CA, Twa)

    CA = ValoriNotNan(Indici, chBsTp, chTwa)
    SetChannelStats(CA, BsTp)

    CA = ValoriNotNan(Indici, chTwaD, chTwa)
    SetChannelStats(CA, TwaD)

    CA = ValoriNotNan(Indici, chVmgP, chTwa)
    SetChannelStats(CA, VmgP)

    'Dim Tgt = TgtManager.Tgt.ValoreTgt(UpwindLeg, Tws.Avg, "bs")
    'Dim TgtP = TgtManager.Tgt.ValoreTgt(UpwindLeg, Tws.PAvg, "bs")
    'Dim TgtS = TgtManager.Tgt.ValoreTgt(UpwindLeg, Tws.SAvg, "bs")


    'RecVmgP.Avg = (Bs.Avg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.MinVal = (Bs.MinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.MaxVal = (Bs.MaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.PAvg = (Bs.PAvg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.PMinVal = (Bs.PMinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.PMaxVal = (Bs.PMaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.SAvg = (Bs.SAvg * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.SMinVal = (Bs.SMinVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg
    'RecVmgP.SMaxVal = (Bs.SMaxVal * System.Math.Abs(System.Math.Cos(Radians(Tgt.Twa)))) / Tgt.Vmg

  End Sub

  Function ValoriNotNan(Indexes As List(Of Integer), Canale As clsChannel2020, TwaChannel As clsChannel2020) As ChannelArrays
    Dim CA As New ChannelArrays
    Canale = DataProvider2020.CanaleDbl(Canale.ChannelId)
    For Each idx In Indexes
      Dim v = Canale.Valori(idx)
      Dim a = TwaChannel.Valori(idx)
      If Not Double.IsNaN(v) AndAlso Not Double.IsNaN(a) Then
        CA.Both.Add(v)
        If a >= 0 Then
          CA.Stbd.Add(v)
        Else
          CA.Port.Add(v)
        End If
      End If
    Next
    Return CA
  End Function


  Sub SetChannelStats360(CA As ChannelArrays, ByRef CS As ChannelStats)
    Dim avg = Media360(CA.Both.ToArray)
    Dim r As New List(Of Double)
    Dim l As New List(Of Double)
    For Each e In CA.Both.ToArray
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    Dim MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    Dim MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
    CS.Avg = avg
    CS.MinVal = MaxL
    CS.MaxVal = MaxR

    avg = Media360(CA.Stbd.ToArray)
    r.Clear()
    l.Clear()
    For Each e In CA.Stbd.ToArray
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
    CS.SAvg = avg
    CS.SMinVal = MaxL
    CS.SMaxVal = MaxR

    avg = Media360(CA.Port.ToArray)
    r.Clear()
    l.Clear()
    For Each e In CA.Port.ToArray
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
    CS.PAvg = avg
    CS.PMinVal = MaxL
    CS.PMaxVal = MaxR
  End Sub

  Sub SetChannelStats180(CA As ChannelArrays, ByRef CS As ChannelStats)
    Dim avg = MediaAbs(CA.Both.ToArray)
    Dim Above As New List(Of Double)
    Dim Below As New List(Of Double)
    For Each e In CA.Both.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    Dim MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    Dim MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.Avg = avg
    CS.MinVal = MaxBelow
    CS.MaxVal = MaxAbove

    avg = MediaAbs(CA.Stbd.ToArray)
    Above.Clear()
    Below.Clear()
    For Each e In CA.Stbd.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.SAvg = avg
    CS.SMinVal = MaxBelow
    CS.SMaxVal = MaxAbove

    avg = MediaAbs(CA.Port.ToArray)
    Above.Clear()
    Below.Clear()
    For Each e In CA.Port.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.PAvg = avg
    CS.PMinVal = MaxBelow
    CS.PMaxVal = MaxAbove
  End Sub
  Sub SetChannelStats(CA As ChannelArrays, ByRef CS As ChannelStats)
    Dim avg = Media(CA.Both.ToArray)
    Dim Above As New List(Of Double)
    Dim Below As New List(Of Double)
    For Each e In CA.Both.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    Dim MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    Dim MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.Avg = avg
    CS.MinVal = MaxBelow
    CS.MaxVal = MaxAbove

    avg = Media(CA.Stbd.ToArray)
    Above.Clear()
    Below.Clear()
    For Each e In CA.Stbd.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.SAvg = avg
    CS.SMinVal = MaxBelow
    CS.SMaxVal = MaxAbove

    avg = Media(CA.Port.ToArray)
    Above.Clear()
    Below.Clear()
    For Each e In CA.Port.ToArray
      Dim d = e - avg
      If d >= 0 Then
        Above.Add(d)
      Else
        Below.Add(Math.Abs(d))
      End If
    Next
    MaxAbove = avg + MathNet.Numerics.Statistics.Statistics.Percentile(Above.ToArray(), 98)
    MaxBelow = avg - MathNet.Numerics.Statistics.Statistics.Percentile(Below.ToArray(), 98)
    CS.PAvg = avg
    CS.PMinVal = MaxBelow
    CS.PMaxVal = MaxAbove
  End Sub


End Class

Public Class LegData
  Public Property Andatura As SummaryReport.Andatura
  Public Property LegStart As DateTime
  Public Property LegFinish As DateTime
  Public Property SailingVmg As Double
  Public Property FirstMinuteVmg As Double
  Public Property MarkApproachVmg As Double
  Public Property ManoeuveringFirstMinuteVmg As Double
  Public Property ManoeuveringVmg As Double
  Public Property NrOfManoeuvers As Integer
  Public Property NrOfManoeuversInTheFirstMinute As Integer
  Public Property ManoeuversLossTwsEqTime As TimeSpan
  Public Property ManoeuversLossTwaEqTime As TimeSpan
  Public Property ManoeuversTime As TimeSpan
  Public Property VmgSailingTime As TimeSpan
  Public Property ReachingSailingTime As TimeSpan
  Public Property TwsPlus2eq As Double
  Public Property TwsMinus2eq As Double
  Public Property Details As New LegDataDetails
  Public Property LegAvgTws As Double
  Public Property LegInitialTws As Double
  Public Property LegFinalTws As Double
  Public Property LegAvgTwd As Double
  Public Property LegInitialTwd As Double
  Public Property LegFinalTwd As Double
  Public Property TimePercAtVmgMoreThan90 As Double
  Public Property TimePercAtVmgMoreThan95 As Double
  Public Property TimePercAtVmgMoreThan98 As Double

  Public ReadOnly Property LegTime As TimeSpan
    Get
      Return LegFinish.Subtract(LegStart)
    End Get
  End Property
  Public ReadOnly Property LegVmgTime As TimeSpan
    Get
      Return LegTime.Subtract(ReachingSailingTime)
    End Get
  End Property
  Public ReadOnly Property ManoeuveringTotalLossVmgMeters As Double
    Get
      Return KtsToMS(SailingVmg - ManoeuveringVmg) * ManoeuversTime.TotalSeconds
    End Get
  End Property
  Public ReadOnly Property ManoeuveringFirstMinuteTotalLossVmgMeters As Double
    Get
      Return KtsToMS(SailingVmg - ManoeuveringFirstMinuteVmg) * 60
    End Get
  End Property
  Public ReadOnly Property FirstMinuteTotalLossVmgMeters As Double
    Get
      Return KtsToMS(SailingVmg - FirstMinuteVmg) * 60
    End Get
  End Property
  Public ReadOnly Property LastMinuteTotalLossVmgMeters As Double
    Get
      Return KtsToMS(SailingVmg - MarkApproachVmg) * 60
    End Get
  End Property
  Public ReadOnly Property PerManoeuverLossVmgMeters As Double
    Get
      If (NrOfManoeuvers = 0) Then Return 0
      Return ManoeuveringTotalLossVmgMeters / NrOfManoeuvers
    End Get
  End Property
  Public ReadOnly Property PerManoeuverFirstMinuteLossVmgMeters As Double
    Get
      If (NrOfManoeuversInTheFirstMinute = 0) Then Return 0
      Return ManoeuveringFirstMinuteTotalLossVmgMeters / NrOfManoeuversInTheFirstMinute
    End Get
  End Property
  Public ReadOnly Property ManoeuveringTotalLossInlineMeters As Double
    Get
      Dim twa = TgtManager.Tgt.ValoreTgt(Andatura = SummaryReport.Andatura.UpwindVmg, LegAvgTws, "bs").Twa
      If Andatura = SummaryReport.Andatura.DownWindVmg Then twa = 180 - twa
      Return ManoeuveringTotalLossVmgMeters / Math.Cos(Radians(twa))
    End Get
  End Property
  Public ReadOnly Property PerManoeuverLossInlineMeters As Double
    Get
      If (NrOfManoeuvers = 0) Then Return 0
      Return ManoeuveringTotalLossInlineMeters / NrOfManoeuvers
    End Get
  End Property
  Public ReadOnly Property PerManoeuverFirtMinuteLossInlineMeters As Double
    Get
      If (NrOfManoeuversInTheFirstMinute = 0) Then Return 0
      Return ManoeuveringFirstMinuteTotalLossVmgMeters / NrOfManoeuversInTheFirstMinute
    End Get
  End Property

  Public ReadOnly Property VmgSailingVmgTimePerc As Double
    Get
      Return VmgSailingTime.TotalSeconds / (LegTime.TotalSeconds - ReachingSailingTime.TotalSeconds)
    End Get
  End Property
  Public ReadOnly Property ManoeuveringVmgTimePerc As Double
    Get
      Return ManoeuversTime.TotalSeconds / (LegTime.TotalSeconds - ReachingSailingTime.TotalSeconds)
    End Get
  End Property
  Public ReadOnly Property ReachingTimePerc As Double
    Get
      Return ReachingSailingTime.TotalSeconds / LegTime.TotalSeconds
    End Get
  End Property




End Class

Public Class LossMatrixCell
  Public IdX As Integer
  Public IdY As Integer
  'Dim UpperRangeX As Double
  'Dim LowerRangeX As Double
  'Dim UpperRangeY As Double
  'Dim LowerRangeY As Double
  Public Values As New List(Of Double)


  Public Sub New(X As Integer, Y As Integer)
    IdX = X
    IdY = Y
  End Sub

End Class
Public Class LossMatrix
  Public Cells As New List(Of LossMatrixCell)
  Dim TwsStep As Integer = 2
  Dim TwaStep As Integer = 5

  Public Sub AddValue(Tws As Double, Twa As Double, BspP As Double)
    Dim Xidx As Integer = Math.Floor(Tws / TwsStep)
    Dim Yidx As Integer = Math.Floor(Twa / TwaStep)
    Dim c = GetCell(Xidx, Yidx)
    If Double.IsNaN(BspP) Then
      Exit Sub
    End If
    c.Values.Add(BspP)
  End Sub

  Public Function GetCell(X As Integer, Y As Integer) As LossMatrixCell
    Dim c = Cells.Where(Function(k) k.IdX = X AndAlso k.IdY = Y).ToList
    If c.Count = 0 Then
      Dim nc = New LossMatrixCell(X, Y)
      Cells.Add(nc)
      Return nc
    Else
      Return c.FirstOrDefault()
    End If
  End Function

  Public Function GetCsvTable(CellsMatrix As List(Of LossMatrixCell)) As String
    If CellsMatrix.Count = 0 Then Return ""
    Dim rows As New List(Of String)
    Dim Tws = CellsMatrix.Select(Function(x) x.IdX).ToList
    Dim Twa = CellsMatrix.Select(Function(x) x.IdY).ToList
    Dim MinTws = Tws.Min * TwsStep
    Dim MaxTws = (Tws.Max + 1) * TwsStep
    Dim MinTwa = Twa.Min * TwaStep
    Dim MaxTwa = (Twa.Max + 1) * TwaStep

    Dim r As String = vbTab
    For a As Integer = Twa.Min To Twa.Max
      If (a + 1) * TwaStep > 30 Then
        r += "Twa: " & a * TwaStep & "-" & (a + 1) * TwaStep & vbTab
      End If
    Next
    rows.Add(r.TrimEnd(vbTab))

    For s As Integer = Tws.Min To Tws.Max
      r = "Tws: " & s * TwsStep & "-" & (s + 1) * TwsStep & vbTab
      For a As Integer = Twa.Min To Twa.Max
        If (a + 1) * TwaStep > 30 Then
          Dim c = CellsMatrix.Where(Function(x) x.IdX = s AndAlso x.IdY = a).ToList
          If c.Count = 0 Then
            r += "" & vbTab
          Else
            r += c.FirstOrDefault.Values.FirstOrDefault.ToString("F1") & vbTab
          End If
        End If
      Next
      rows.Add(r.TrimEnd(vbTab))
    Next
    Return String.Join(vbCrLf, rows)
  End Function


  Public Function GetTotalLossMatrix() As List(Of LossMatrixCell)
    Dim TotalLoss As Double = 0
    For Each C In Cells
      For Each v In C.Values
        TotalLoss += v
      Next
    Next
    Dim ResMatrix As New List(Of LossMatrixCell)
    For Each C In Cells
      Dim rc As New LossMatrixCell(C.IdX, C.IdY)
      Dim Loss As Double = 0
      For Each v In C.Values
        Loss += v
      Next
      rc.Values.Add(Loss / TotalLoss * 100)
      ResMatrix.Add(rc)
    Next
    Return ResMatrix
  End Function

  Public Function GetAvgLossMatrix() As List(Of LossMatrixCell)
    Dim TotalLoss As Double = 0
    For Each C In Cells
      For Each v In C.Values
        TotalLoss += v
      Next
    Next
    Dim ResMatrix As New List(Of LossMatrixCell)
    For Each C In Cells
      Dim rc As New LossMatrixCell(C.IdX, C.IdY)
      If C.Values.Count > 0 Then
        rc.Values.Add(C.Values.Average)
      Else
        rc.Values.Add(0)
      End If
      ResMatrix.Add(rc)
    Next
    Return ResMatrix
  End Function


End Class

Public Class clsLegTmp
  Public Property Andatura As SummaryReport.Andatura = SummaryReport.Andatura.Undefined
  Public Property FirstDT As DateTime
  Public Property LastDT As DateTime
  Public Property LegIndex As Integer
  Public Property Rows As New List(Of BoatStatusIndex)

End Class


Public Class SummaryReport

  Public Property Rows As New List(Of BoatStatusIndex)
  Public Property LegsData As New List(Of LegData)
  Public Property PeriodData As LegData
  Public Property UpwindAndReachingData As LegData
  Public Property DownWindData As LegData
  Public Enum Andatura
    UpwindVmg = 0
    Reaching = 1
    DownWindVmg = 2
    UnderMinUpVmgPerf = 3
    UnderMinDnVmgPerf = 4
    Undefined = 5
  End Enum

  Function AngleFromTwa(AbsTwa As Double, UpwindMaxAngle As Double, DownWindMinAngle As Double) As Andatura
    If AbsTwa < UpwindMaxAngle Then Return Andatura.UpwindVmg
    If AbsTwa > DownWindMinAngle Then Return Andatura.DownWindVmg
    Return Andatura.Reaching
  End Function

  Function GetRowId(Row As BoatStatusIndex, Secondi As Integer) As Integer

    Dim NM As DateTime = Row.DT.AddSeconds(Secondi)

    Dim steppi As Integer = 1
    If Secondi < 0 Then
      For ii As Integer = Row.IdRiga To 0 Step -1
        If (Rows(ii).DT.ToOADate <= NM.ToOADate) Then Return ii
      Next
      Return 0
    Else
      For ii As Integer = Row.IdRiga To Rows.Count - 1
        If (Rows(ii).DT.ToOADate >= NM.ToOADate) Then Return ii
      Next
      Return Rows.Count - 1
    End If
  End Function

  Function GetRowId(indiceIniziale As Integer, Secondi As Integer) As Integer

    Dim NM As DateTime = Rows(indiceIniziale).DT.AddSeconds(Secondi)
    Dim steppi As Integer = 1
    If Secondi < 0 Then
      For ii As Integer = indiceIniziale To 0 Step -1
        If (Rows(ii).DT.ToOADate <= NM.ToOADate) Then Return ii
      Next
      Return 0
    Else
      For ii As Integer = indiceIniziale To Rows.Count - 1
        If (Rows(ii).DT.ToOADate >= NM.ToOADate) Then Return ii
      Next
      Return Rows.Count - 1
    End If
  End Function


  Function NextAngle(DT As DateTime, secondsCheck As Integer, chTwa As clsChannel2020) As Andatura
    Dim idi As Integer = DataProvider2020.TrovaIndice(DT)
    Dim idf As Integer = DataProvider2020.TrovaIndice(DT.AddSeconds(secondsCheck))
    If (idi = -1 OrElse idf = -1) Then Return Andatura.Undefined
    Dim AbsTwa As New List(Of Double)
    For ii As Integer = idi To idf
      If Not Double.IsNaN(chTwa.Valori(ii)) Then
        AbsTwa.Add(chTwa.Valori(ii))
      End If
    Next
    If (AbsTwa.Count = 0) Then Return Andatura.Undefined
    Return AngleFromTwa(AbsTwa.Average, AppConfig.ActiveProfile.SummarySettings.UpwindMaxAngle, AppConfig.ActiveProfile.SummarySettings.DownWindMinAngle)
  End Function

  Function AndaturaDominante(inizio As DateTime, fine As DateTime) As Andatura
    Dim u As Integer = 0
    Dim d As Integer = 0
    Dim r As Integer = 0

    For Each Riga In Rows
      If Riga.DT.ToOADate >= inizio.ToOADate AndAlso Riga.DT.ToOADate <= fine.ToOADate Then
        Select Case Riga.Angle
          Case Andatura.UpwindVmg
            u += 1
          Case Andatura.DownWindVmg
            d += 1
          Case Else
            r += 1
        End Select
      End If
    Next
    If u > r AndAlso u > d Then Return Andatura.UpwindVmg
    If d > u AndAlso d > r Then Return Andatura.DownWindVmg

    Return Andatura.Reaching

  End Function

  Public Sub setWLRows(TR As clsTimeRange, StartTime As DateTime, SecBeforeRotation As Integer, SecAfterRotation As Integer, SecBeforeChangeOfTack As Integer, SecAfterChangeOfTack As Integer, YrtTreshold As Double)


    If Not StartTime = Nothing Then
      TR.Start = StartTime
      If TR.Finish.ToOADate < TR.Start.ToOADate Then
        TR.Finish = TR.Start.AddHours(1)
      End If
    End If

    Dim chYRT = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)

    Dim twa As Double = chTwa.Valori(TR.IdRigaIniziale)
    Dim PrevIsStbd As Boolean = Double.IsNaN(twa) OrElse twa > 0
    Rows.Clear()

    Dim idi As Integer
    Dim idf As Integer


    ' Set Rotations and Changes of Tack
    For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
      Dim nr As New BoatStatusIndex(i, DataProvider2020.TimeStamps(i))
      twa = chTwa.Valori(i)
      Dim yrt As Double = chYRT.Valori(i)
      If Not Double.IsNaN(twa) AndAlso Not Double.IsNaN(yrt) Then
        nr.IsStbd = twa >= 0
        nr.Angle = AngleFromTwa(Math.Abs(twa), AppConfig.ActiveProfile.SummarySettings.UpwindMaxAngle, AppConfig.ActiveProfile.SummarySettings.DownWindMinAngle)
        nr.IsRotating = Math.Abs(yrt) > YrtTreshold
        nr.IsChangeOfTack = Not (PrevIsStbd = nr.IsStbd)
        Rows.Add(nr)
        PrevIsStbd = nr.IsStbd
      End If
    Next

    ' Set Rotating and Manoeuvering
    For i As Integer = 0 To Rows.Count - 1
      If Rows(i).IsRotating Then
        idi = GetRowId(i, -SecBeforeRotation)
        idf = GetRowId(i, SecAfterRotation)
        For ii As Integer = idi To idf
          Rows(ii).IsAroundRotation = True
        Next
      End If
      If Rows(i).IsChangeOfTack Then
        idi = GetRowId(i, -SecBeforeChangeOfTack)
        idf = GetRowId(i, SecAfterChangeOfTack)
        If Rows(i).Angle = Andatura.UpwindVmg Then
          For ii As Integer = idi To idf
            Rows(ii).IsTacking = True
          Next
        ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
          For ii As Integer = idi To idf
            Rows(ii).IsGybing = True
          Next
        End If
      End If
    Next


    'la logica del cambio lato deve essere che se si passa di angolo va controllata la media di andatura di due minuti del minuto successivo
    Dim LegsTmp As New List(Of clsLegTmp)
    Dim pLeg As New clsLegTmp
    pLeg.Andatura = Rows(0).Angle
    pLeg.FirstDT = Rows(0).DT
    pLeg.Rows.Add(Rows(0))
    LegsTmp.Add(pLeg)
    For i As Integer = 1 To Rows.Count - 1
      Dim cAndatura = Rows(i).Angle
      If cAndatura = pLeg.Andatura Then
        pLeg.LastDT = Rows(i).DT
      Else
        pLeg = New clsLegTmp
        pLeg.Andatura = Rows(i).Angle
        pLeg.FirstDT = Rows(i).DT
        LegsTmp.Add(pLeg)
      End If
      pLeg.Rows.Add(Rows(i))
    Next
    'Stop
    'LegsTmp = LegsTmp.Where(Function(x) (x.LastDT.Subtract(x.FirstDT).TotalSeconds > 15)).ToList()
    Dim l As Integer = 1
    For Each Lg In LegsTmp
      If (Lg.LastDT.Subtract(Lg.FirstDT).TotalSeconds > 15) Then
        Lg.LegIndex = l
        l += 1
      Else
        Lg.Andatura = Andatura.Undefined
        Lg.LegIndex = -1
      End If
    Next
    'Stop


    ' Set Leg
    Dim PrevAngle As Andatura = Rows.First.Angle
    Dim Leg As Integer = 1
    Dim LegChangeTime As DateTime = Nothing
    For i As Integer = 0 To Rows.Count - 1
      Dim ActualAngle As Andatura = Rows(i).Angle
      If ActualAngle = Andatura.Reaching Then
        ' se i due minuti successivi' sono di traverso allora e' un lato di traverso altirmenti viene considerato dogleg e segnato come tale successivamente
        If NextAngle(Rows(i).DT, 120, chTwa) = Andatura.Reaching Then
          Leg += 1
        End If
      Else
        ' actual leg not reaching
        If Not ActualAngle = PrevAngle Then
          Dim idPeneltyFinder = GetRowId(i, 30)
          If Rows(idPeneltyFinder).Angle = PrevAngle Then
            ' segna le righe dell inotrno come di penalita'
            idi = GetRowId(i, -20)
            idf = GetRowId(i, 30)
            For ii As Integer = idi To idf
              Rows(ii).IsTakingPenalty = True
            Next
          End If
          If Not Rows(i).IsTakingPenalty Then Leg += 1
        End If
      End If
      Rows(i).LegIndex = Leg
      PrevAngle = ActualAngle
    Next

    Leg = 1
    '' segna le righe dei primi 45 secondi del lato iniziale come IslegBeginning

    If Not StartTime = Nothing Then
      idi = 0
      idf = GetRowId(0, 60)
      For ii As Integer = idi To idf
        Rows(ii).IsLegFirstMinute = True
      Next
    End If
    ' segna come IsMarkApproach i secondi antecedenti il cambio di lato
    For i As Integer = 0 To Rows.Count - 1
      If Not Rows(i).LegIndex = Leg Then
        ' leg change
        For ii As Integer = i - 1 To 0 Step -1 ' gira indietro
          If Not Rows(ii).Angle = Andatura.Reaching Then ' se non e' di traverso ovvero il dogleg
            idi = GetRowId(ii, -60)
            idf = ii
            For iii As Integer = idi To idf ' segna le righe dei 45 second precedenti come di approccio alla boa
              Rows(iii).IsMarkApproach = True
            Next
            Exit For
          End If
        Next
        ' segna le righe dei primi 45 secondi del lato come IslegBeginning
        idi = i
        idf = GetRowId(i, 60)
        For ii As Integer = idi To idf
          Rows(ii).IsLegFirstMinute = True
        Next
      End If
      Leg = Rows(i).LegIndex
    Next

    Dim DogLegSecs As Integer = 120
    For i As Integer = 0 To Rows.Count - 1
      If Rows(i).Angle = Andatura.Reaching Then ' se non e' di traverso ovvero il dogleg
        If NextAngle(Rows(i).DT.AddSeconds(120), 120, chTwa) = Andatura.UpwindVmg Then
          If NextAngle(Rows(i).DT.AddSeconds(-240), 120, chTwa) = Andatura.DownWindVmg Then
            Rows(i).IsDogLeg = True
          End If
        End If
      End If
    Next


    Dim chVmgKts = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
    Dim chVmgPerc = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)

    Dim VmgUpwindPerc As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim VmgDownwindPerc As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra

    Dim VmgUpwind As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim VmgDownwind As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim VmgTacking As New List(Of Double) ' tutti quelli in virata
    Dim VmgGybing As New List(Of Double) ' tutti quelli in strambata
    Dim Reaching As New List(Of Double) ' tutti quelli di traverso
    Dim MarkApproachVmgUp As New List(Of Double) ' tutti quelli approcciando la boa di bolina
    Dim MarkApproachVmgDn As New List(Of Double) ' tutti quelli approcciando la boa di poppa
    Dim FirstMinuteVmgUp As New List(Of Double) ' tutti quelli di inizio lato bolina
    Dim FirstMinuteVmgDn As New List(Of Double) ' tutti quelli di inizio lato poppa
    Dim FirstMinuteVmgTaking As New List(Of Double) ' tutti quelli di inizio lato bolina con virata
    Dim FirstMinuteVmgGybing As New List(Of Double) ' tutti quelli di inizio lato poppa con strambata
    Dim Tacks As Integer = 0
    Dim Gybes As Integer = 0
    Dim EarlyTacks As Integer = 0
    Dim Indians As Integer = 0

    Dim LegTws As New List(Of Double)
    Dim LegTwd As New List(Of Double)
    Dim TwsUp As New List(Of Double)
    Dim TwsDn As New List(Of Double)
    Dim TwdUp As New List(Of Double)
    Dim TwdDn As New List(Of Double)

    ' del singolo lato, ovvero che vengono azzerati sul cambio lato
    Dim LegReaching As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim LegSailingVmg As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim LegSailingVmgPerc As New List(Of Double) ' tutti quelli non di traverso, non in rotazione e non in manovra
    Dim LegManouveringVmg As New List(Of Double) ' tutti quelli in virata
    Dim LegFirstMinuteVmg As New List(Of Double) ' tutti quelli di inizio lato
    Dim LegFirstMinuteVmgManoeuvering As New List(Of Double) ' tutti quelli di inizio lato
    Dim LegMarkApproach As New List(Of Double) ' tutti quelli approcciando la boa 

    LegsData.Clear()
    Dim LD As LegData
    Dim LegStart As DateTime = Rows.First.DT
    Dim PrevLeg As Integer = Rows.First.LegIndex
    Dim LegManoeuvers As Integer = 0
    Dim FirstMinuteManoeuvers As Integer = 0
    Dim IsManoeuvering As Boolean = Rows.First.IsGybing OrElse Rows.First.IsTacking
    Dim IsFirstMinuteTack As Boolean = Rows.First.IsTacking
    Dim IsFirstMinuteGybe As Boolean = Rows.First.IsGybing
    For i As Integer = 0 To Rows.Count - 1
      Dim CurrentLeg As Integer = Rows(i).LegIndex
      If Not CurrentLeg = PrevLeg Then
        LD = New LegData
        LD.LegStart = LegStart
        LD.LegFinish = Rows(i).DT
        LD.LegAvgTws = LegTws.Average
        LD.LegAvgTwd = Media360(LegTwd.ToArray())
        LD.Andatura = AndaturaDominante(LD.LegStart, LD.LegFinish)
        idi = DataProvider2020.TrovaIndice(LD.LegStart)
        idf = DataProvider2020.TrovaIndice(LD.LegStart.AddSeconds(90))
        LD.LegInitialTws = Media(chTws.Valori.Skip(idi).Take(idf - idi).ToArray)
        LD.LegInitialTwd = Media360(chTwd.Valori.Skip(idi).Take(idf - idi).ToArray)
        idi = DataProvider2020.TrovaIndice(LD.LegFinish.AddSeconds(-90))
        idf = DataProvider2020.TrovaIndice(LD.LegFinish)
        LD.LegFinalTws = Media(chTws.Valori.Skip(idi).Take(idf - idi).ToArray)
        LD.LegFinalTwd = Media360(chTwd.Valori.Skip(idi).Take(idf - idi).ToArray)

        LD.SailingVmg = getAverageValue(LegSailingVmg)
        LD.VmgSailingTime = TimeSpan.FromSeconds(LegSailingVmg.Count * DataProvider2020.Hz)
        LD.TimePercAtVmgMoreThan90 = getTimePerc(LegSailingVmgPerc, 90)
        LD.TimePercAtVmgMoreThan95 = getTimePerc(LegSailingVmgPerc, 95)
        LD.TimePercAtVmgMoreThan98 = getTimePerc(LegSailingVmgPerc, 98)
        LD.ManoeuveringVmg = getAverageValue(LegManouveringVmg)
        LD.ManoeuversTime = TimeSpan.FromSeconds(LegManouveringVmg.Count * DataProvider2020.Hz)
        LD.ManoeuveringFirstMinuteVmg = getAverageValue(LegFirstMinuteVmg)
        LD.ManoeuveringFirstMinuteVmg = getAverageValue(LegFirstMinuteVmgManoeuvering)
        LD.MarkApproachVmg = getAverageValue(LegMarkApproach)
        LD.ReachingSailingTime = TimeSpan.FromSeconds(LegReaching.Count * DataProvider2020.Hz)
        LD.TwsPlus2eq = TgtManager.Tgt.LiftEquivalent(LegTws.Average, 2, LD.Andatura = Andatura.UpwindVmg)
        LD.TwsMinus2eq = TgtManager.Tgt.LiftEquivalent(LegTws.Average, -2, LD.Andatura = Andatura.UpwindVmg)
        LD.NrOfManoeuvers = LegManoeuvers
        LD.NrOfManoeuversInTheFirstMinute = FirstMinuteManoeuvers
        If LegManoeuvers > 0 Then
          LD.ManoeuversLossTwsEqTime = TgtManager.Tgt.TwsTimeEquivalent(LegTws.Average, -2, LD.Andatura = Andatura.UpwindVmg, LD.PerManoeuverLossVmgMeters)
          LD.ManoeuversLossTwaEqTime = TgtManager.Tgt.TwaTimeEquivalent(LegTws.Average, 5, LD.Andatura = Andatura.UpwindVmg, LD.PerManoeuverLossVmgMeters)
        End If
        LegsData.Add(LD)
        LegReaching.Clear()
        LegSailingVmg.Clear()
        LegSailingVmgPerc.Clear()
        LegManouveringVmg.Clear()
        LegFirstMinuteVmg.Clear()
        LegFirstMinuteVmgManoeuvering.Clear()
        LegMarkApproach.Clear()
        LegTws.Clear()
        LegTwd.Clear()
        LegManoeuvers = 0
        FirstMinuteManoeuvers = 0
        If Rows.Count >= i + 1 Then
          LegStart = Rows(i + 1).DT
        Else
          LegStart = Rows(i).DT
        End If
      End If
      PrevLeg = CurrentLeg
      If Not Rows(i).Angle = Andatura.Reaching AndAlso Not Rows(i).IsAroundRotation AndAlso Not Rows(i).IsMarkApproach AndAlso Not Rows(i).IsLegFirstMinute AndAlso Not Rows(i).IsTakingPenalty AndAlso Not Rows(i).IsTacking AndAlso Not Rows(i).IsGybing Then
        LegSailingVmg.Add(chVmgKts.Valori(Rows(i).IdRiga))
        LegSailingVmgPerc.Add(chVmgPerc.Valori(Rows(i).IdRiga))
        If Rows(i).Angle = Andatura.UpwindVmg Then
          VmgUpwind.Add(chVmgKts.Valori(Rows(i).IdRiga))
          VmgUpwindPerc.Add(chVmgPerc.Valori(Rows(i).IdRiga))
        ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
          VmgDownwind.Add(chVmgKts.Valori(Rows(i).IdRiga))
          VmgDownwindPerc.Add(chVmgPerc.Valori(Rows(i).IdRiga))
        End If
      End If
      If Rows(i).Angle = Andatura.Reaching Then
        Reaching.Add(chVmgKts.Valori(Rows(i).IdRiga)) ' da migliorare cosa mettere
        LegReaching.Add(chVmgKts.Valori(Rows(i).IdRiga)) ' da migliorare cosa mettere
      End If

      If Rows(i).IsMarkApproach Then
        LegMarkApproach.Add(chVmgKts.Valori(Rows(i).IdRiga))
        If Rows(i).Angle = Andatura.UpwindVmg Then
          MarkApproachVmgUp.Add(chVmgKts.Valori(Rows(i).IdRiga))
        ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
          MarkApproachVmgDn.Add(chVmgKts.Valori(Rows(i).IdRiga))
        End If
      End If

      Dim tws As Double = chTws.Valori(Rows(i).IdRiga)
      If Not Double.IsNaN(tws) Then
        LegTws.Add(tws)
        If Rows(i).Angle = Andatura.UpwindVmg Then
          TwsUp.Add(tws)
        ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
          TwsDn.Add(tws)
        End If
      End If

      Dim twd As Double = chTwd.Valori(Rows(i).IdRiga)
      If Not Double.IsNaN(twd) Then
        LegTwd.Add(twd)
        If Rows(i).Angle = Andatura.UpwindVmg Then
          TwdUp.Add(twd)
        ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
          TwdDn.Add(twd)
        End If
      End If

      If Rows(i).IsTacking Then
        If Rows(i).IsLegFirstMinute Then
          LegFirstMinuteVmgManoeuvering.Add(chVmgKts.Valori(Rows(i).IdRiga))
          FirstMinuteVmgTaking.Add(chVmgKts.Valori(Rows(i).IdRiga))
          FirstMinuteVmgUp.Add(chVmgKts.Valori(Rows(i).IdRiga))
          IsFirstMinuteTack = True
        Else
          LegManouveringVmg.Add(chVmgKts.Valori(Rows(i).IdRiga))
          VmgTacking.Add(chVmgKts.Valori(Rows(i).IdRiga))
          IsManoeuvering = True
        End If
      ElseIf Rows(i).IsGybing Then
        If Rows(i).IsLegFirstMinute AndAlso Rows(i).Angle = Andatura.DownWindVmg Then
          LegFirstMinuteVmgManoeuvering.Add(chVmgKts.Valori(Rows(i).IdRiga))
          FirstMinuteVmgGybing.Add(chVmgKts.Valori(Rows(i).IdRiga))
          FirstMinuteVmgDn.Add(chVmgKts.Valori(Rows(i).IdRiga))
          IsFirstMinuteGybe = True
        Else
          LegManouveringVmg.Add(chVmgKts.Valori(Rows(i).IdRiga))
          VmgGybing.Add(chVmgKts.Valori(Rows(i).IdRiga))
          IsManoeuvering = True
        End If
      Else
        If Rows(i).IsLegFirstMinute AndAlso Rows(i).Angle = Andatura.UpwindVmg Then
          LegFirstMinuteVmg.Add(chVmgKts.Valori(Rows(i).IdRiga))
          If Rows(i).Angle = Andatura.UpwindVmg Then
            FirstMinuteVmgUp.Add(chVmgKts.Valori(Rows(i).IdRiga))
          ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
            FirstMinuteVmgDn.Add(chVmgKts.Valori(Rows(i).IdRiga))
          End If
        End If

        If IsManoeuvering = True Then
          LegManoeuvers += 1
          If Rows(i).Angle = Andatura.UpwindVmg Then
            Tacks += 1
          ElseIf Rows(i).Angle = Andatura.DownWindVmg Then
            Gybes += 1
          End If
          IsManoeuvering = False
        ElseIf IsFirstMinuteTack = True Then
          FirstMinuteManoeuvers += 1
          EarlyTacks += 1
          IsFirstMinuteTack = False
          IsFirstMinuteGybe = False
        ElseIf IsFirstMinuteGybe = True Then
          FirstMinuteManoeuvers += 1
          Indians += 1
          IsFirstMinuteTack = False
          IsFirstMinuteGybe = False
        End If
      End If
    Next
    LD = New LegData
    LD.LegStart = LegStart
    LD.LegFinish = Rows.Last.DT
    LD.Andatura = AndaturaDominante(LD.LegStart, LD.LegFinish)
    LD.LegAvgTws = LegTws.Average
    LD.LegAvgTwd = Media360(LegTwd.ToArray())

    idi = DataProvider2020.TrovaIndice(LD.LegStart)
    idf = DataProvider2020.TrovaIndice(LD.LegStart.AddSeconds(90))
    LD.LegInitialTws = Media(chTws.Valori.Skip(idi).Take(idf - idi).ToArray)
    LD.LegInitialTwd = Media360(chTwd.Valori.Skip(idi).Take(idf - idi).ToArray)
    idi = DataProvider2020.TrovaIndice(LD.LegFinish.AddSeconds(-90))
    idf = DataProvider2020.TrovaIndice(LD.LegFinish)
    LD.LegFinalTws = Media(chTws.Valori.Skip(idi).Take(idf - idi).ToArray)
    LD.LegFinalTwd = Media360(chTwd.Valori.Skip(idi).Take(idf - idi).ToArray)

    LD.SailingVmg = getAverageValue(LegSailingVmg)
    LD.VmgSailingTime = TimeSpan.FromSeconds(LegSailingVmg.Count * DataProvider2020.Hz)
    LD.TimePercAtVmgMoreThan90 = getTimePerc(LegSailingVmgPerc, 90)
    LD.TimePercAtVmgMoreThan95 = getTimePerc(LegSailingVmgPerc, 95)
    LD.TimePercAtVmgMoreThan98 = getTimePerc(LegSailingVmgPerc, 98)
    LD.ManoeuveringVmg = getAverageValue(LegManouveringVmg)
    LD.ManoeuversTime = TimeSpan.FromSeconds(LegManouveringVmg.Count * DataProvider2020.Hz)
    LD.ManoeuveringFirstMinuteVmg = getAverageValue(LegFirstMinuteVmg)
    LD.ManoeuveringFirstMinuteVmg = getAverageValue(LegFirstMinuteVmgManoeuvering)
    LD.MarkApproachVmg = getAverageValue(LegMarkApproach)
    LD.ReachingSailingTime = TimeSpan.FromSeconds(LegReaching.Count * DataProvider2020.Hz)


    LD.TwsPlus2eq = TgtManager.Tgt.LiftEquivalent(LegTws.Average, 2, LD.Andatura = Andatura.UpwindVmg)
    LD.TwsMinus2eq = TgtManager.Tgt.LiftEquivalent(LegTws.Average, -2, LD.Andatura = Andatura.UpwindVmg)
    LD.NrOfManoeuvers = LegManoeuvers
    If LegManoeuvers > 0 Then
      LD.ManoeuversLossTwsEqTime = TgtManager.Tgt.TwsTimeEquivalent(LegTws.Average, -2, LD.Andatura = Andatura.UpwindVmg, LD.PerManoeuverLossVmgMeters)
      LD.ManoeuversLossTwaEqTime = TgtManager.Tgt.TwaTimeEquivalent(LegTws.Average, 5, LD.Andatura = Andatura.UpwindVmg, LD.PerManoeuverLossVmgMeters)
    End If
    LegsData.Add(LD)


    UpwindAndReachingData = New LegData
    UpwindAndReachingData.LegStart = Rows.First.DT
    UpwindAndReachingData.LegFinish = Rows.Last.DT
    UpwindAndReachingData.LegAvgTws = TwsUp.Average
    UpwindAndReachingData.LegAvgTwd = Media360(TwdUp.ToArray())

    UpwindAndReachingData.Andatura = Andatura.UpwindVmg
    If VmgUpwind.Count > 0 Then
      UpwindAndReachingData.SailingVmg = VmgUpwind.Average
      UpwindAndReachingData.VmgSailingTime = TimeSpan.FromSeconds(VmgUpwind.Count * DataProvider2020.Hz)
      UpwindAndReachingData.TimePercAtVmgMoreThan90 = getTimePerc(VmgUpwindPerc, 90)
      UpwindAndReachingData.TimePercAtVmgMoreThan95 = getTimePerc(VmgUpwindPerc, 95)
      UpwindAndReachingData.TimePercAtVmgMoreThan98 = getTimePerc(VmgUpwindPerc, 98)
    Else
      UpwindAndReachingData.SailingVmg = 0
      UpwindAndReachingData.VmgSailingTime = TimeSpan.FromSeconds(0)
      UpwindAndReachingData.TimePercAtVmgMoreThan90 = 0
      UpwindAndReachingData.TimePercAtVmgMoreThan95 = 0
      UpwindAndReachingData.TimePercAtVmgMoreThan98 = 0
    End If
    If (VmgTacking.Count > 0) Then
      UpwindAndReachingData.ManoeuveringVmg = VmgTacking.Average
      UpwindAndReachingData.ManoeuversTime = TimeSpan.FromSeconds(VmgTacking.Count * DataProvider2020.Hz)
    Else
      UpwindAndReachingData.ManoeuveringVmg = 0
      UpwindAndReachingData.ManoeuversTime = TimeSpan.FromSeconds(0)
    End If
    If (Reaching.Count > 0) Then
      UpwindAndReachingData.ReachingSailingTime = TimeSpan.FromSeconds(Reaching.Count * DataProvider2020.Hz)
    Else
      UpwindAndReachingData.ReachingSailingTime = TimeSpan.FromSeconds(0)
    End If
    UpwindAndReachingData.TwsPlus2eq = TgtManager.Tgt.LiftEquivalent(TwsUp.Average, 2, True)
    UpwindAndReachingData.TwsMinus2eq = TgtManager.Tgt.LiftEquivalent(TwsUp.Average, -2, True)
    UpwindAndReachingData.NrOfManoeuvers = Tacks
    If Tacks > 0 Then
      UpwindAndReachingData.ManoeuversLossTwsEqTime = TgtManager.Tgt.TwsTimeEquivalent(TwsUp.Average, -2, True, UpwindAndReachingData.PerManoeuverLossVmgMeters)
      UpwindAndReachingData.ManoeuversLossTwaEqTime = TgtManager.Tgt.TwaTimeEquivalent(TwsUp.Average, 5, True, UpwindAndReachingData.PerManoeuverLossVmgMeters)
    End If
    UpwindAndReachingData.NrOfManoeuversInTheFirstMinute = EarlyTacks
    If FirstMinuteVmgUp.Count > 5 Then
      UpwindAndReachingData.FirstMinuteVmg = FirstMinuteVmgUp.Average
    Else
      UpwindAndReachingData.FirstMinuteVmg = 0
    End If
    If FirstMinuteVmgTaking.Count > 5 Then
      UpwindAndReachingData.ManoeuveringFirstMinuteVmg = FirstMinuteVmgTaking.Average
    Else
      UpwindAndReachingData.ManoeuveringFirstMinuteVmg = 0
    End If
    If (MarkApproachVmgUp.Count > 5) Then
      UpwindAndReachingData.MarkApproachVmg = MarkApproachVmgUp.Average
    Else
      UpwindAndReachingData.MarkApproachVmg = 0
    End If


    DownWindData = New LegData
    DownWindData.LegStart = Rows.First.DT
    DownWindData.LegFinish = Rows.Last.DT
    DownWindData.LegAvgTws = TwsDn.Average
    DownWindData.LegAvgTwd = Media360(TwdDn.ToArray())
    DownWindData.Andatura = Andatura.DownWindVmg
    If VmgDownwind.Count > 0 Then
      DownWindData.SailingVmg = VmgDownwind.Average
      DownWindData.VmgSailingTime = TimeSpan.FromSeconds(VmgDownwind.Count * DataProvider2020.Hz)
      DownWindData.TimePercAtVmgMoreThan90 = getTimePerc(VmgDownwindPerc, 90)
      DownWindData.TimePercAtVmgMoreThan95 = getTimePerc(VmgDownwindPerc, 95)
      DownWindData.TimePercAtVmgMoreThan98 = getTimePerc(VmgDownwindPerc, 98)
    Else
      DownWindData.SailingVmg = 0
      DownWindData.VmgSailingTime = TimeSpan.FromSeconds(0)
      DownWindData.TimePercAtVmgMoreThan90 = 0
      DownWindData.TimePercAtVmgMoreThan95 = 0
      DownWindData.TimePercAtVmgMoreThan98 = 0
    End If
    If (VmgGybing.Count > 0) Then
      DownWindData.ManoeuveringVmg = VmgGybing.Average
      DownWindData.ManoeuversTime = TimeSpan.FromSeconds(VmgGybing.Count * DataProvider2020.Hz)
    Else
      DownWindData.ManoeuveringVmg = 0
      DownWindData.ManoeuversTime = TimeSpan.FromSeconds(0)
    End If
    DownWindData.ReachingSailingTime = TimeSpan.FromSeconds(0)
    DownWindData.TwsPlus2eq = TgtManager.Tgt.LiftEquivalent(TwsDn.Average, 2, False)
    DownWindData.TwsMinus2eq = TgtManager.Tgt.LiftEquivalent(TwsDn.Average, -2, False)
    DownWindData.NrOfManoeuvers = Gybes
    If Gybes > 0 Then
      DownWindData.ManoeuversLossTwsEqTime = TgtManager.Tgt.TwsTimeEquivalent(TwsDn.Average, -2, False, DownWindData.PerManoeuverLossVmgMeters)
      DownWindData.ManoeuversLossTwaEqTime = TgtManager.Tgt.TwaTimeEquivalent(TwsDn.Average, 5, False, DownWindData.PerManoeuverLossVmgMeters)
    End If
    DownWindData.NrOfManoeuversInTheFirstMinute = Indians
    If FirstMinuteVmgDn.Count > 5 Then
      DownWindData.FirstMinuteVmg = FirstMinuteVmgDn.Average
    Else
      DownWindData.FirstMinuteVmg = 0
    End If
    If FirstMinuteVmgGybing.Count > 5 Then
      DownWindData.ManoeuveringFirstMinuteVmg = FirstMinuteVmgGybing.Average
    Else
      DownWindData.ManoeuveringFirstMinuteVmg = 0
    End If
    If (MarkApproachVmgDn.Count > 5) Then
      DownWindData.MarkApproachVmg = MarkApproachVmgDn.Average
    Else
      DownWindData.MarkApproachVmg = 0
    End If

    Dim ChsU As List(Of clsChannel2020)
    Dim ChsR As List(Of clsChannel2020)
    Dim ChsD As List(Of clsChannel2020)
    Dim s = AppConfig.ActiveProfile.RaceReportSettings
    If s Is Nothing Then
      ChsU = New List(Of clsChannel2020)
      Dim c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl("RudderToe")
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm)
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl("Forestay")
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl("DeflectorP")
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl("Rake")
      If Not c Is Nothing Then ChsU.Add(c)
      c = DataProvider2020.CanaleDbl("Tack")
      If Not c Is Nothing Then ChsU.Add(c)

      ChsR = New List(Of clsChannel2020)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
      If Not c Is Nothing Then ChsR.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
      If Not c Is Nothing Then ChsR.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm)
      If Not c Is Nothing Then ChsR.Add(c)

      ChsD = New List(Of clsChannel2020)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
      If Not c Is Nothing Then ChsD.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
      If Not c Is Nothing Then ChsD.Add(c)
      c = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm)
      If Not c Is Nothing Then ChsD.Add(c)

      AppConfig.ActiveProfile.RaceReportSettings = New clsRaceReportSettings
      AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsUpwind = ChsU
      AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsReaching = ChsR
      AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsDownwind = ChsD
      AppConfig.Salva()
    Else
      ChsU = AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsUpwind
      ChsR = AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsReaching
      ChsD = AppConfig.ActiveProfile.RaceReportSettings.ExtraChannelsDownwind
    End If

    Dim LT As New List(Of Integer)
    For Each LegData In LegsData
      LT.Clear()
      For Each r In Rows
        If r.DT.ToOADate >= LegData.LegStart.ToOADate AndAlso r.DT.ToOADate <= LegData.LegFinish.ToOADate Then
          If r.IsPerformanceSailing(LegData.Andatura) Then
            LT.Add(r.IdRiga)
          End If
        End If
      Next
      LegData.Details.UpdateStats(LegData.Andatura, LT, ChsU, ChsR, ChsD)
    Next

    LT.Clear()
    For Each r In Rows
      If r.IsPerformanceSailing(UpwindAndReachingData.Andatura) Then
        LT.Add(r.IdRiga)
      End If
    Next
    UpwindAndReachingData.Details.UpdateStats(SummaryReport.Andatura.UpwindVmg, LT, ChsU, ChsR, ChsD)

    LT.Clear()
    For Each r In Rows
      If r.IsPerformanceSailing(DownWindData.Andatura) Then
        LT.Add(r.IdRiga)
      End If
    Next
    DownWindData.Details.UpdateStats(SummaryReport.Andatura.DownWindVmg, LT, ChsU, ChsR, ChsD)

    LT.Clear()
    For Each r In Rows
      LT.Add(r.IdRiga)
    Next
    PeriodData = New LegData
    PeriodData.Details.UpdateStats(LT)


    Dim chBsPerc = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
    'chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    'chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim psr = Rows.Where(Function(x) x.IsPerformanceSailing).ToList
    Dim LossMatrix = New LossMatrix()
    For Each r In psr
      LossMatrix.AddValue(chTws.Valori(r.IdRiga), Math.Abs(chTwa.Valori(r.IdRiga)), chBsPerc.Valori(r.IdRiga))
    Next
    Dim T = LossMatrix.GetTotalLossMatrix()
    Dim strTotLoss As String = LossMatrix.GetCsvTable(T)
    If Not LossMatrix Is Nothing Then
      Dim A = LossMatrix.GetAvgLossMatrix()
      Dim strAvgLoss As String = LossMatrix.GetCsvTable(A)
      Dim TotLossHeader As String = "Total Loss, each cell is a perventage of the total loss" & vbCrLf
      Dim AvgLossHeader As String = "Average Performance, each cell is the average boat speed polar percentage" & vbCrLf
      Clipboard.SetText(TotLossHeader & strTotLoss & vbCrLf & vbCrLf & vbCrLf & AvgLossHeader & strAvgLoss)
    End If
  End Sub

  Function getAverageValue(Values As List(Of Double)) As Double
    If Values.Count = 0 Then Return 0
    Return Values.Average
  End Function
  Function getTimePerc(Values As List(Of Double), PercLimit As Double) As Double
    If Values.Count = 0 Then Return 0
    Dim v = Values.Where(Function(x) x >= PercLimit).Count()
    Return v / Values.Count * 100
  End Function

  Public Function TwdDelta(Angle As Andatura) As Double
    Dim v As New List(Of Double)
    For Each l In LegsData
      If l.Andatura = Angle Then
        v.Add(DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(l.LegInitialTwd, l.LegFinalTwd))
      End If
    Next
    Return v.Average
  End Function
  Public Function TwsDelta(Angle As Andatura) As Double
    Dim v As New List(Of Double)
    For Each l In LegsData
      If l.Andatura = Angle Then
        v.Add(l.LegFinalTws - l.LegInitialTws)
      End If
    Next
    Return v.Average
  End Function


End Class



Public Class UserControlTimeRangeSummary
  'Dim VM As UserControlTimeRangeSummaryViewModel
  ''Dim VM As New UserControlTimeRangeSummaryViewModel


  'Public Sub New()
  '  'Me.DataContext = VM
  '  ' This call is required by the designer.
  '  InitializeComponent()

  '  ' --- Nulla di ciò che segue deve girare nel designer XAML ---
  '  If IsInDesignMode Then Exit Sub


  '  Me.DataContext = VM


  '  ' Add any initialization after the InitializeComponent() call.

  '  ImpostaGrafici()
  'End Sub


  Public WithEvents VM As UserControlTimeRangeSummaryViewModel   ' dichiarazione nuda

  Public Sub New()

    InitializeComponent()

    If IsInDesignMode Then Exit Sub

    ' 1) PRIMA istanzia tutto ciò che era "As New"
    VM = New UserControlTimeRangeSummaryViewModel()
    Me.DataContext = VM

    ' 2) POI chiama i metodi che lo usano
    ImpostaGrafici()

  End Sub

  Private Sub ImpostaGrafici()
    VM.AggiungiGrafico(Plot1, 1)
    VM.AggiungiGrafico(Plot2, 2)
    VM.AggiungiGrafico(Plot3, 3)
    VM.AggiungiGrafico(Plot4, 4)
    VM.AggiungiGrafico(Plot5, 5)
    VM.AggiungiGrafico(Plot6, 6)
    VM.AggiungiGrafico(Plot7, 7)
    VM.AggiungiGrafico(Plot8, 8)
    VM.AggiungiGrafico(Plot9, 9)
    VM.AggiungiGrafico(Plot10, 10)
    VM.AggiungiGrafico(Plot11, 11)
    VM.AggiungiGrafico(Plot12, 12)
    VM.AggiungiGrafico(Plot13, 13)
    VM.AggiungiGrafico(Plot14, 14)
    VM.AggiungiGrafico(Plot15, 15)
    VM.AggiungiGrafico(Plot16, 16)
    VM.AggiungiGrafico(Plot17, 17)
    VM.AggiungiGrafico(Plot18, 18)
    VM.AggiungiGrafico(Plot19, 19)
    VM.AggiungiGrafico(Plot20, 20)
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.AggiornaGrafici()
    VM.VisibleRangeReport.setWLRows(DataPlotSync.VisibleRange, Nothing, 5, 10, 10, 30, 3)
  End Sub

  Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)
    VM.PlotHeight = Math.Max(200, (e.NewSize.Height - 330) / 5)
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Dim r = New clsPdf
    Dim d As New Dictionary(Of String, List(Of System.IO.Stream))
    Dim ltmp As New List(Of System.IO.Stream)
    ltmp.Add(Plot1.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot2.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot20.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot19.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    d.Add("L1", ltmp.ToList)

    ltmp.Clear()
    ltmp.Add(Plot11.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot3.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot14.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot4.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    d.Add("L2", ltmp.ToList)

    ltmp.Clear()
    ltmp.Add(Plot12.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot5.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot15.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot6.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    d.Add("L3", ltmp.ToList)

    ltmp.Clear()
    ltmp.Add(Plot13.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot7.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot16.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot8.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    d.Add("L4", ltmp.ToList)

    ltmp.Clear()
    ltmp.Add(Plot17.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot18.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot9.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    ltmp.Add(Plot10.Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False))
    d.Add("L5", ltmp.ToList)

    Dim RigheTesto As New List(Of String)
    RigheTesto.Add(Plot1.VM.Titolo & vbTab & Plot2.VM.Titolo) ' & vbTab & "")
    RigheTesto.Add("Upwind Vmg Tgt %" & vbTab & Plot3.VM.SottoTitolo & vbTab & Plot3.VM.SottoTitolo1 & vbTab & Plot3.VM.SottoTitolo2)
    RigheTesto.Add("Upwind Bs Tgt %" & vbTab & Plot5.VM.SottoTitolo & vbTab & Plot5.VM.SottoTitolo1 & vbTab & Plot5.VM.SottoTitolo2)
    RigheTesto.Add("Upwind Twa Delta" & vbTab & Plot7.VM.SottoTitolo & vbTab & Plot7.VM.SottoTitolo1 & vbTab & Plot7.VM.SottoTitolo2)
    RigheTesto.Add("Downwind Vmg Tgt %" & vbTab & Plot4.VM.SottoTitolo & vbTab & Plot4.VM.SottoTitolo1 & vbTab & Plot4.VM.SottoTitolo2)
    RigheTesto.Add("Downwind Bs Tgt %" & vbTab & Plot6.VM.SottoTitolo & vbTab & Plot6.VM.SottoTitolo1 & vbTab & Plot6.VM.SottoTitolo2)
    RigheTesto.Add("Downwind Twa Delta" & vbTab & Plot8.VM.SottoTitolo & vbTab & Plot8.VM.SottoTitolo1 & vbTab & Plot8.VM.SottoTitolo2)
    RigheTesto.Add(Plot19.VM.Titolo & vbTab & Plot19.VM.SottoTitolo1 & vbTab & Plot19.VM.SottoTitolo2)
    r.StampaSummary(VM.SummaryHeader & "_SummaryReport", VM.SummaryHeader & " " & VM.PeriodDescription, RigheTesto, VM.RigheVmgUp.ToList, VM.RigheVmgDn.ToList, VM.RigheReaching.ToList, d)
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    VM.SelezionaTutteCoseRelative()
  End Sub
End Class



<AddINotifyPropertyChangedInterface>
Public Class UserControlTimeRangeSummaryViewModel
  Public Property VisibleRangeReport As New SummaryReport
  Public Property PlotHeight As Single
  Public Property Grafici As New List(Of UserControlSciChartPlot)
  Public Property RigheVmgUp As New ObservableCollection(Of clsRigaVmg)
  Public Property RigheVmgDn As New ObservableCollection(Of clsRigaVmg)
  Public Property RigheReaching As New ObservableCollection(Of clsRigaReaching)

  Public Property SummaryHeader As String
    Get
      Return AppConfig.ActiveProfile.SummarySettings.SummaryHeader
    End Get
    Set(value As String)
      AppConfig.ActiveProfile.SummarySettings.SummaryHeader = value
    End Set
  End Property


  Public Sub New()
    If AppConfig.ActiveProfile.SummarySettings Is Nothing Then
      AppConfig.ActiveProfile.SummarySettings = New clsSummarySettings
      AppConfig.Salva()
    End If
  End Sub

  Dim _PeriodDescription As String
  Public Property PeriodDescription As String
    Get
      Return _PeriodDescription
    End Get
    Set(value As String)
      _PeriodDescription = value
    End Set
  End Property

  Public Property TrendLineOrder As Integer
    Get
      Return AppConfig.ActiveProfile.SummarySettings.TrendLineOrder
    End Get
    Set(value As Integer)
      AppConfig.ActiveProfile.SummarySettings.TrendLineOrder = value
    End Set
  End Property

  Public Sub SelezionaTutteCoseRelative()
    For Each period In PeriodsManager.Periods.Lista
      period.IsChecked = period.TR.IsOverlapped(DataPlotSync.VisibleRange)
    Next
    If ExpStarts.StartsList.Count > 0 Then
      Dim s = ExpStarts.StartsList.OrderBy(Function(x) Math.Abs(x.StartTime.Subtract(DataPlotSync.VisibleRange.Start).TotalSeconds)).FirstOrDefault
      If Not s Is Nothing Then
        ExpStarts.SelectedStart = s
      End If
    End If
  End Sub

  Public Sub AggiornaGrafici()
    For Each grafico In Grafici
      grafico.VM.AggiornaGrafico(DataPlotSync.VisibleRange)
      grafico.Plot.ZoomExtents()
    Next
    AggiornaRigheTabelle()
    AggiornaTrueWindStats()
    If DataPlotSync.VisibleRange.Durata.TotalDays < 1 Then
      PeriodDescription = DataPlotSync.VisibleRange.Start.ToString("dd MMM yyyy") & " " & DataPlotSync.VisibleRange.StringaPeriodo
    Else
      PeriodDescription = DataPlotSync.VisibleRange.StringaPeriodo
    End If
    AppConfig.Salva()
  End Sub

  Private Sub AggiornaTrueWindStats()
    'clsXYZKpoint
    Dim gtwd = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_Twd).FirstOrDefault
    Dim v = gtwd.VM.PuntiStbd.Select(Function(x) x.X).ToArray
    Dim avg = Media360(DirectCast(v, Double()))
    Dim r As New List(Of Double)
    Dim l As New List(Of Double)
    For Each e In v
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    Dim MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    Dim MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
    gtwd.VM.Titolo = "Twd Up: " & avg.ToString("F0").PadLeft(3, "0") & "° (" & MaxL.ToString("F0").PadLeft(3, "0") & "-" & MaxR.ToString("F0").PadLeft(3, "0") & ")"

    v = gtwd.VM.PuntiPort.Select(Function(x) x.X).ToArray
    avg = Media360(DirectCast(v, Double()))
    r = New List(Of Double)
    l = New List(Of Double)
    For Each e In v
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
    gtwd.VM.Titolo &= " Dn: " & avg.ToString("F0").PadLeft(3, "0") & "° (" & MaxL.ToString("F0").PadLeft(3, "0") & "-" & MaxR.ToString("F0").PadLeft(3, "0") & ")"




    Dim gtws = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_Tws).FirstOrDefault
    v = gtws.VM.PuntiStbd.Select(Function(x) x.X).ToArray
    gtws.VM.Titolo = "Tws Up: " & StringaAverage(v, 1) & "k (" & v.Min.ToString("F1") & "-" & v.Max.ToString("F1") & ")"

    v = gtws.VM.PuntiPort.Select(Function(x) x.X).ToArray
    gtws.VM.Titolo &= " Dn: " & StringaAverage(v, 1) & "k (" & v.Min.ToString("F1") & "-" & v.Max.ToString("F1") & ")"


    Dim gbspp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_PolarPerc).FirstOrDefault
    v = gbspp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    Dim Punti As Integer = v.Count
    Dim v90 As Integer
    Dim v95 As Integer
    If Punti > 0 Then
      avg = v.Average
      Dim SD = MathNet.Numerics.Statistics.Statistics.StandardDeviation(v)
      v90 = v.Where(Function(X) X >= 90).Count / Punti * 100
      v95 = v.Where(Function(X) X >= 95).Count / Punti * 100
      gbspp.VM.Titolo = "StraightLine Bs Percentage"
      gbspp.VM.SottoTitolo1 = "Avg: " & avg.ToString("F0") & ", Sd: " & SD.ToString("F0") & ""
      gbspp.VM.SottoTitolo2 = "Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    Dim gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpVmgTgtP).FirstOrDefault
    Dim sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    Dim pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    Dim gg As New List(Of Double)
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "UpVmgTgt%"
    If gg.Count > 0 Then
      avg = gg.Average
      v90 = gg.Where(Function(X) X >= 90).Count / gg.Count * 100
      v95 = gg.Where(Function(X) X >= 95).Count / gg.Count * 100
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo = "Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    If sTmp.Count > 0 Then
      avg = sTmp.Average
      v90 = sTmp.Where(Function(X) X >= 90).Count / sTmp.Count * 100
      v95 = sTmp.Where(Function(X) X >= 95).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo1 = "Stbd Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      Dim SD = MathNet.Numerics.Statistics.Statistics.StandardDeviation(v)
      v90 = pTmp.Where(Function(X) X >= 90).Count / pTmp.Count * 100
      v95 = pTmp.Where(Function(X) X >= 95).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo2 = "Port Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnVmgTgtP).FirstOrDefault
    sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    gg.Clear()
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "DownVmgTgt%"

    If gg.Count > 0 Then
      avg = gg.Average
      v90 = gg.Where(Function(X) X >= 90).Count / gg.Count * 100
      v95 = gg.Where(Function(X) X >= 95).Count / gg.Count * 100
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo = "Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    If sTmp.Count > 0 Then
      avg = sTmp.Average
      v90 = sTmp.Where(Function(X) X >= 90).Count / sTmp.Count * 100
      v95 = sTmp.Where(Function(X) X >= 95).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo1 = "Stbd Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      v90 = pTmp.Where(Function(X) X >= 90).Count / pTmp.Count * 100
      v95 = pTmp.Where(Function(X) X >= 95).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo2 = "Port Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpBsTgtP).FirstOrDefault
    sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    gg.Clear()
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "UpBsTgt%"
    If gg.Count > 0 Then
      avg = gg.Average
      v90 = gg.Where(Function(X) X >= 90).Count / gg.Count * 100
      v95 = gg.Where(Function(X) X >= 95).Count / gg.Count * 100
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo = "Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    If sTmp.Count > 0 Then
      avg = sTmp.Average
      v90 = sTmp.Where(Function(X) X >= 90).Count / sTmp.Count * 100
      v95 = sTmp.Where(Function(X) X >= 95).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo1 = "Stbd Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      v90 = pTmp.Where(Function(X) X >= 90).Count / pTmp.Count * 100
      v95 = pTmp.Where(Function(X) X >= 95).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo2 = "Port Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnBsTgtP).FirstOrDefault
    sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    gg.Clear()
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "DownBsTgt%"
    If gg.Count > 0 Then
      avg = gg.Average
      v90 = gg.Where(Function(X) X >= 90).Count / gg.Count * 100
      v95 = gg.Where(Function(X) X >= 95).Count / gg.Count * 100
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo = "Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    If sTmp.Count > 0 Then
      avg = sTmp.Average
      v90 = sTmp.Where(Function(X) X >= 90).Count / sTmp.Count * 100
      v95 = sTmp.Where(Function(X) X >= 95).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo1 = "Stbd Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      v90 = pTmp.Where(Function(X) X >= 90).Count / pTmp.Count * 100
      v95 = pTmp.Where(Function(X) X >= 95).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "%"
      gTmp.VM.SottoTitolo2 = "Port Time%>90%: " & v90.ToString("F0") & ", >95%: " & v95.ToString("F0") & ""
    End If

    gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpTwaD).FirstOrDefault
    sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    gg.Clear()
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "UpTwaTgtD"
    Dim Hi As Integer = 0
    Dim Low As Integer = 0
    If sTmp.Count > 0 Then
      avg = gg.Average
      Hi = sTmp.Where(Function(X) X <= 0).Count / sTmp.Count * 100
      Low = sTmp.Where(Function(X) X > 0).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo = "Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
      avg = sTmp.Average
      Hi = sTmp.Where(Function(X) X <= 0).Count / sTmp.Count * 100
      Low = sTmp.Where(Function(X) X > 0).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo1 = "Stbd Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      Hi = pTmp.Where(Function(X) X <= 0).Count / pTmp.Count * 100
      Low = pTmp.Where(Function(X) X > 0).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo2 = "Port Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
    End If

    gTmp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnTwaD).FirstOrDefault
    sTmp = gTmp.VM.PuntiStbd.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    pTmp = gTmp.VM.PuntiPort.Where(Function(x) Not Double.IsNaN(x.Y)).Select(Function(x) x.Y).ToArray
    gg.Clear()
    For Each s In sTmp
      gg.Add(s)
    Next
    For Each p In pTmp
      gg.Add(p)
    Next
    gTmp.VM.Titolo = "DownTwaTgtD"
    If gg.Count > 0 Then
      avg = gg.Average
      Hi = sTmp.Where(Function(X) X <= 0).Count / sTmp.Count * 100
      Low = sTmp.Where(Function(X) X > 0).Count / sTmp.Count * 100
    End If

    If sTmp.Count > 0 Then
      gTmp.VM.Titolo &= ",Avg : " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo = "Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
      avg = sTmp.Average
      Hi = sTmp.Where(Function(X) X <= 0).Count / sTmp.Count * 100
      Low = sTmp.Where(Function(X) X > 0).Count / sTmp.Count * 100
      gTmp.VM.Titolo &= ",Avg Stbd: " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo1 = "Stbd Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
    End If
    If pTmp.Count > 0 Then
      avg = pTmp.Average
      Hi = pTmp.Where(Function(X) X <= 0).Count / pTmp.Count * 100
      Low = pTmp.Where(Function(X) X > 0).Count / pTmp.Count * 100
      gTmp.VM.Titolo &= ", Port: " & avg.ToString("F0") & "°"
      gTmp.VM.SottoTitolo2 = "Port Time%<Tgt: " & Hi.ToString("F0") & ", >Tgt: " & Low.ToString("F0") & ""
    End If



  End Sub


  Private Sub AggiornaRigheTabelle()
    'RigheVmg.First.Group
    'RigheVmg.First.Tws
    'RigheVmg.First.Both.UpVmgTgtP

    RigheVmgUp.Clear()
    Dim guv = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpVmgTgtP).FirstOrDefault
    Dim gus = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpBsTgtP).FirstOrDefault
    Dim gua = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_UpTwaD).FirstOrDefault
    Dim gt = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eTacks).FirstOrDefault


    Dim Tws As String = StringaAverage(guv.VM.Points_X(0, 100), 1)
    Dim Riga As New clsRigaVmg("Totals", Tws, Colors.LightBlue)
    Dim VmgTgtP As String = StringaAverage(guv.VM.Points_Y(0, 100), 0)
    Dim BsTgtP As String = StringaAverage(gus.VM.Points_Y(0, 100), 0)
    Dim TwaTgtD As String = StringaAverage(gua.VM.Points_Y(0, 100), 0)
    Dim Mn = gt.VM.Points_Y(0, 100).Count
    Dim B As New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(0, 100), 1))
    Riga.Both = B
    VmgTgtP = StringaAverage(guv.VM.StbdPoints_Y(0, 100), 0)
    BsTgtP = StringaAverage(gus.VM.StbdPoints_Y(0, 100), 0)
    TwaTgtD = StringaAverage(gua.VM.StbdPoints_Y(0, 100), 0)
    Mn = gt.VM.StbdPoints_Y(0, 100).Count
    Dim S As New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(0, 100), 1))
    Riga.Stbd = S
    VmgTgtP = StringaAverage(guv.VM.PortPoints_Y(0, 100), 0)
    BsTgtP = StringaAverage(gus.VM.PortPoints_Y(0, 100), 0)
    TwaTgtD = StringaAverage(gua.VM.PortPoints_Y(0, 100), 0)
    Mn = gt.VM.StbdPoints_Y(0, 100).Count
    Dim P As New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(0, 100), 1))
    Riga.Port = P
    RigheVmgUp.Add(Riga)


    For i As Integer = 4 To 30 Step 2
      Dim puv = guv.VM.Points_Y(i - 1, i + 1)
      If puv.Count > 0 Then
        Tws = StringaAverage(guv.VM.Points_X(i - 1, i + 1), 1)
        Riga = New clsRigaVmg((i - 1).ToString & "-" & (i + 1).ToString, Tws, Colors.WhiteSmoke)

        Dim UpVmgTgtP As String = StringaAverage(guv.VM.Points_Y(i - 1, i + 1), 0)
        Dim UpBsTgtP As String = StringaAverage(gus.VM.Points_Y(i - 1, i + 1), 0)
        Dim UpTwaTgtD As String = StringaAverage(gua.VM.Points_Y(i - 1, i + 1), 0)
        Mn = gt.VM.Points_Y(i - 1, i + 1).Count
        B = New clsValoriRigaVmg(UpVmgTgtP, UpBsTgtP, UpTwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(i - 1, i + 1), 1))
        Riga.Both = B

        UpVmgTgtP = StringaAverage(guv.VM.StbdPoints_Y(i - 1, i + 1), 0)
        UpBsTgtP = StringaAverage(gus.VM.StbdPoints_Y(i - 1, i + 1), 0)
        UpTwaTgtD = StringaAverage(gua.VM.StbdPoints_Y(i - 1, i + 1), 0)
        Mn = gt.VM.StbdPoints_Y(i - 1, i + 1).Count
        S = New clsValoriRigaVmg(UpVmgTgtP, UpBsTgtP, UpTwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(i - 1, i + 1), 1))
        Riga.Stbd = S

        UpVmgTgtP = StringaAverage(guv.VM.PortPoints_Y(i - 1, i + 1), 0)
        UpBsTgtP = StringaAverage(gus.VM.PortPoints_Y(i - 1, i + 1), 0)
        UpTwaTgtD = StringaAverage(gua.VM.PortPoints_Y(i - 1, i + 1), 0)
        Mn = gt.VM.StbdPoints_Y(i - 1, i + 1).Count
        P = New clsValoriRigaVmg(UpVmgTgtP, UpBsTgtP, UpTwaTgtD, Mn, StringaAverage(gt.VM.Points_Y(i - 1, i + 1), 1))
        Riga.Port = P

        RigheVmgUp.Add(Riga)
      End If
    Next



    RigheVmgDn.Clear()
    Dim gdv = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnVmgTgtP).FirstOrDefault
    Dim gds = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnBsTgtP).FirstOrDefault
    Dim gda = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eD_DnTwaD).FirstOrDefault
    Dim gg = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eGybes).FirstOrDefault

    Tws = StringaAverage(gdv.VM.Points_X(0, 100), 1)
    Riga = New clsRigaVmg("Totals", Tws, Colors.Gold)
    VmgTgtP = StringaAverage(gdv.VM.Points_Y(0, 100), 0)
    BsTgtP = StringaAverage(gds.VM.Points_Y(0, 100), 0)
    TwaTgtD = StringaAverage(gda.VM.Points_Y(0, 100), 0)
    Mn = gg.VM.Points_Y(0, 100).Count
    B = New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gg.VM.Points_Y(0, 100), 1))
    Riga.Both = B
    VmgTgtP = StringaAverage(gdv.VM.StbdPoints_Y(0, 100), 0)
    BsTgtP = StringaAverage(gds.VM.StbdPoints_Y(0, 100), 0)
    TwaTgtD = StringaAverage(gda.VM.StbdPoints_Y(0, 100), 0)
    Mn = gg.VM.StbdPoints_Y(0, 100).Count
    S = New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gg.VM.Points_Y(0, 100), 1))
    Riga.Stbd = S
    VmgTgtP = StringaAverage(gdv.VM.PortPoints_Y(0, 100), 0)
    BsTgtP = StringaAverage(gds.VM.PortPoints_Y(0, 100), 0)
    TwaTgtD = StringaAverage(gda.VM.PortPoints_Y(0, 100), 0)
    Mn = gg.VM.StbdPoints_Y(0, 100).Count
    P = New clsValoriRigaVmg(VmgTgtP, BsTgtP, TwaTgtD, Mn, StringaAverage(gg.VM.Points_Y(0, 100), 1))
    Riga.Port = P
    RigheVmgDn.Add(Riga)

    For i As Integer = 4 To 30 Step 2
      Dim pdv = gdv.VM.Points_Y(i - 1, i + 1)
      If pdv.Count > 0 Then
        Tws = StringaAverage(gdv.VM.Points_X(i - 1, i + 1), 1)
        Riga = New clsRigaVmg((i - 1).ToString & "-" & (i + 1).ToString, Tws, Colors.WhiteSmoke)

        Dim DnVmgTgtP As String = StringaAverage(gdv.VM.Points_Y(i - 1, i + 1), 0)
        Dim DnBsTgtP As String = StringaAverage(gds.VM.Points_Y(i - 1, i + 1), 0)
        Dim DnTwaTgtD As String = StringaAverage(gda.VM.Points_Y(i - 1, i + 1), 0)
        Mn = gg.VM.Points_Y(i - 1, i + 1).Count
        B = New clsValoriRigaVmg(DnVmgTgtP, DnBsTgtP, DnTwaTgtD, Mn, StringaAverage(gg.VM.Points_Y(i - 1, i + 1), 1))
        Riga.Both = B

        DnVmgTgtP = StringaAverage(gdv.VM.StbdPoints_Y(i - 1, i + 1), 0)
        DnBsTgtP = StringaAverage(gds.VM.StbdPoints_Y(i - 1, i + 1), 0)
        DnTwaTgtD = StringaAverage(gda.VM.StbdPoints_Y(i - 1, i + 1), 0)
        Mn = gg.VM.Points_Y(i - 1, i + 1).Count
        S = New clsValoriRigaVmg(DnVmgTgtP, DnBsTgtP, DnTwaTgtD, Mn, StringaAverage(gg.VM.StbdPoints_Y(i - 1, i + 1), 1))
        Riga.Stbd = S

        DnVmgTgtP = StringaAverage(gdv.VM.PortPoints_Y(i - 1, i + 1), 0)
        DnBsTgtP = StringaAverage(gds.VM.PortPoints_Y(i - 1, i + 1), 0)
        DnTwaTgtD = StringaAverage(gda.VM.PortPoints_Y(i - 1, i + 1), 0)
        Mn = gg.VM.Points_Y(i - 1, i + 1).Count
        P = New clsValoriRigaVmg(DnVmgTgtP, DnBsTgtP, DnTwaTgtD, Mn, StringaAverage(gg.VM.PortPoints_Y(i - 1, i + 1), 1))
        Riga.Port = P

        RigheVmgDn.Add(Riga)
      End If
    Next

    RigheReaching.Clear()
    Dim rp = Grafici.Where(Function(x) x.VM.PlotType = UserControlSciChartPlotViewModel.ePlotType.eXY_TwsTwa).FirstOrDefault

    Dim Vals As New List(Of clsValoriRigaReaching)
    If rp.VM.PointsXYZ(55, 135, 0, 100).Count > 0 Then
      For ii As Integer = 60 To 130 Step 10
        Vals.Add(New clsValoriRigaReaching(StringaAverage(rp.VM.PointsXYZ_X(ii - 5, ii + 5, 0, 100), 0), StringaAverage(rp.VM.PointsXYZ_Y(ii - 5, ii + 5, 0, 100), 1), StringaAverage(rp.VM.PointsXYZ_Z(ii - 5, ii + 5, 0, 100), 0), StringaAverage(rp.VM.PointsXYZ_K(ii - 5, ii + 5, 0, 100), 1, 130), VerificaColore(rp.VM.PointsXYZ_Z(ii - 5, ii + 5, 0, 100))))
      Next
      Dim r As New clsRigaReaching("Totals", Vals)
      RigheReaching.Add(r)
    End If

    For i As Integer = 4 To 30 Step 2
      Vals = New List(Of clsValoriRigaReaching)
      If rp.VM.PointsXYZ(55, 135, i - 1, i + 1).Count > 0 Then
        For ii As Integer = 60 To 130 Step 10
          Vals.Add(New clsValoriRigaReaching(StringaAverage(rp.VM.PointsXYZ_X(ii - 5, ii + 5, i - 1, i + 1), 0), StringaAverage(rp.VM.PointsXYZ_Y(ii - 5, ii + 5, i - 1, i + 1), 1), StringaAverage(rp.VM.PointsXYZ_Z(ii - 5, ii + 5, i - 1, i + 1), 0, 130), StringaAverage(rp.VM.PointsXYZ_K(ii - 5, ii + 5, i - 1, i + 1), 1), VerificaColore(rp.VM.PointsXYZ_Z(ii - 5, ii + 5, i - 1, i + 1))))
        Next
        Dim r As New clsRigaReaching((i - 1).ToString & "-" & (i + 1).ToString, Vals)
        RigheReaching.Add(r)
      End If
    Next


  End Sub

  Public Function StringaAverage(Valori As Double(), Decimali As Integer) As String
    If Valori Is Nothing Then Return "-"
    If Valori.Count = 0 Then Return "-"
    Return Valori.Average.ToString("F" & Decimali.ToString)
  End Function

  Public Function StringaAverage(Valori As Double(), Decimali As Integer, MaxVal As Double) As String
    If Valori Is Nothing Then Return "-"
    If Valori.Count = 0 Then Return "-"
    Return Math.Min(MaxVal, CDbl(Valori.Average)).ToString("F" & Decimali.ToString)
  End Function

  Public Function VerificaColore(Performance As Double()) As Color
    If Performance Is Nothing Then Return Colors.Black
    If Performance.Count = 0 Then Return Colors.Black
    Return ColoreDaPerformance(Performance.Average)
  End Function

  Public Sub AggiungiGrafico(Grafico As UserControlSciChartPlot, PlotType As Integer)
    Grafico.VM.PlotType = PlotType
    Grafici.Add(Grafico)
  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsRigaVmg
  Public Property Group As String
  Public Property Tws As String
  Public Property Colore As New SolidColorBrush
  Public Property Both As clsValoriRigaVmg
  Public Property Port As clsValoriRigaVmg
  Public Property Stbd As clsValoriRigaVmg

  Public Sub New(Group As String, Tws As String, Colore As Color)
    Me.Group = Group
    Me.Tws = Tws
    Me.Colore.Color = Colore
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsValoriRigaVmg
  Public Property VmgTgtP As String
  Public Property BsTgtP As String
  Public Property TwaTgtD As String
  Public Property ManoeuversNr As String
  Public Property ManoeuversLossAvg As String

  Public Sub New(VmgTgtP As String, BsTgtP As String, TwaTgtD As String, ManoeuversNr As String, ManoeuversLossAvg As String)

    Me.VmgTgtP = VmgTgtP
    Me.BsTgtP = BsTgtP
    Me.ManoeuversNr = ManoeuversNr
    Me.ManoeuversLossAvg = ManoeuversLossAvg
    Me.TwaTgtD = TwaTgtD

  End Sub
End Class


<AddINotifyPropertyChangedInterface>
Public Class clsRigaReaching
  Public Property Group As String
  Public Property PolPercAtTwa60 As clsValoriRigaReaching
  Public Property PolPercAtTwa70 As clsValoriRigaReaching
  Public Property PolPercAtTwa80 As clsValoriRigaReaching
  Public Property PolPercAtTwa90 As clsValoriRigaReaching
  Public Property PolPercAtTwa100 As clsValoriRigaReaching
  Public Property PolPercAtTwa110 As clsValoriRigaReaching
  Public Property PolPercAtTwa120 As clsValoriRigaReaching
  Public Property PolPercAtTwa130 As clsValoriRigaReaching


  Public Sub New(Group As String, PolPercAtTwa As List(Of clsValoriRigaReaching))
    Me.Group = Group
    Me.PolPercAtTwa60 = PolPercAtTwa(0)
    Me.PolPercAtTwa70 = PolPercAtTwa(1)
    Me.PolPercAtTwa80 = PolPercAtTwa(2)
    Me.PolPercAtTwa90 = PolPercAtTwa(3)
    Me.PolPercAtTwa100 = PolPercAtTwa(4)
    Me.PolPercAtTwa110 = PolPercAtTwa(5)
    Me.PolPercAtTwa120 = PolPercAtTwa(6)
    Me.PolPercAtTwa130 = PolPercAtTwa(7)
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsValoriRigaReaching
  Public Property Twa As String
  Public Property Tws As String
  Public Property BsPolP As String
  Public Property Bs As String
  Public Property Colore As New SolidColorBrush

  Public Sub New(Twa As String, Tws As String, BsPolP As String, Bs As String, Colore As Color)
    Me.Twa = Twa
    Me.Tws = Tws
    Me.BsPolP = BsPolP
    Me.Bs = Bs
    Me.Colore.Color = Colore
  End Sub
End Class


Public Class clsSummarySettings
  Public Property TrendLineOrder As Integer = 3
  Public Property SummaryHeader As String
  Public Property YrtTreshold As Double = 8
  Public Property StartFinderRangeSeconds As Double = 180
  Public Property SecBeforeRotation As Double = 15
  Public Property SecAfterRotation As Double = 15
  Public Property SecBeforeTack As Double = 12
  Public Property SecAfterTack As Double = 30
  Public Property SecBeforeGybe As Double = 12
  Public Property SecAfterGybe As Double = 30
  Public Property UpwindMaxAngle As Double = 62
  Public Property DownWindMinAngle As Double = 128
  Public Property MinAcceptableVmpPerfPerc As Double = 80

  <JsonIgnore>
  Public Property Channels As ObservableCollection(Of clsChannel2020)
  Public Property ListaCannelsId As List(Of String)


End Class

Public Class clsSummaryHighlightSettings
  Public Property ListaUpwindCannelsId As List(Of String)
  Public Property ListaDownwindCannelsId As List(Of String)
  Public Property CanaleFiltro As String
  Public Property FiltroMinVal As Double
  Public Property FiltroMaxVal As Double

  <JsonIgnore>
  Public Property UpChannels As List(Of clsChannel2020)
  <JsonIgnore>
  Public Property DnChannels As List(Of clsChannel2020)


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsSummary2025
  Public Property RaceStartTime As DateTime = Nothing
  Public Property RaceFinishTime As DateTime = Nothing
  Public Property TR As clsTimeRange
  Public Property RowsAdv As New List(Of clsRowsAdv2025)
  Public Property OtherChannels As New List(Of clsChannel2020)

  Public Enum eRowStatus
    eUnknown = -1
    eVmgUp = 0
    eVmgDn = 1
    eReaching = 2
    eTacking = 3
    eGybing = 4
    eRoundingUp = 5
    eBearingAway = 6
    eIsTackChange = 7
    eIsGybeChange = 8
    eIsRotating = 9
  End Enum



End Class

<AddINotifyPropertyChangedInterface>
Public Class clsRowsAdv2025
  Public Property RowIdx As Integer
  Public Property DeltaSecsFromBefore As Double
  Public Property IsStbd As Boolean
  Public Property RowStatus As clsSummary2025.eRowStatus
  Public Property Leg As Integer
  Public Property ValBs As Double
  Public Property ValSog As Double
  Public Property ValBsTgt As Double
  Public Property ValBsTgtPerc As Double
  Public Property ValBsPol As Double
  Public Property ValBsPolPerc As Double
  Public Property ValTwa As Double
  Public Property ValTwaTgt As Double
  Public Property ValTwaTgtDelta As Double
  Public Property ValVmg As Double
  Public Property ValVmgTgt As Double
  Public Property ValVmgTgtPerc As Double
  Public Property ValSeaState As Double
  Public Property ValTws As Double
  Public Property ValTwd As Double
  Public Property ValHdg As Double
  Public Property ValCos As Double
  Public Property ValLwy As Double
  Public Property ValRecLwy As Double
  Public Property ValCurrRate As Double
  Public Property ValCurrDir As Double
  Public Property ValHeelNorm As Double
  Public Property ValRudderNorm As Double
  Public Property ValAwaNorm As Double
  Public Property ValAws As Double
  Public Property ValTrimNorm As Double
  Public Property ValOthers As New List(Of Double)

  Public Sub New(RowIndex As Integer)
    RowIdx = RowIndex
  End Sub



End Class

