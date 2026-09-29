Public Class clsRatingUtilities


  Public Class clsTargetWithRating
    Public Tgt As clsTgt
    Public IrcRating As Double

    Public Sub New(_Tgt As clsTgt, _IrcRating As Double)
      Tgt = _Tgt
      IrcRating = _IrcRating
    End Sub

  End Class

  Public Class clsTimeRacingResult
    Public Target As clsTargetWithRating
    Public TimesArray As Double(,)
    Public TotalSeconds As Double
    Public TotalRecalculatedSeconds As Double
    Public TotalManoeuveringSeconds As Double
    Public TotalNotValidSeconds As Double
    Public Sub New(_Target As clsTargetWithRating, _TimesArray As Double(,), _TotalSeconds As Double, _TotalRecalculatedSeconds As Double, _TotalManoeuveringSeconds As Double, _TotalNotValidSeconds As Double)
      TimesArray = _TimesArray
      TotalSeconds = _TotalSeconds
      TotalRecalculatedSeconds = _TotalRecalculatedSeconds
      TotalManoeuveringSeconds = _TotalManoeuveringSeconds
      TotalNotValidSeconds = _TotalNotValidSeconds
      Target = _Target
    End Sub


    Public Function TimePercentagesTable() As String
      Dim ArrayTmp(4, 4) As Double ' <5, 5-10,10-15,15-20,>20    <55,55-80,80-110,110-130,>130
      Dim Totale As Integer = 0
      For s As Integer = 0 To TimesArray.GetUpperBound(0)
        Dim TwsIdx As Integer = 0
        If s >= 5 And s < 10 Then
          TwsIdx = 1
        ElseIf s >= 10 And s < 15 Then
          TwsIdx = 2
        ElseIf s >= 15 And s < 20 Then
          TwsIdx = 3
        ElseIf s >= 20 Then
          TwsIdx = 4
        End If
        For a As Integer = 0 To TimesArray.GetUpperBound(1)
          Dim TwaIdx As Integer = 0
          If a >= 55 And a < 80 Then
            TwaIdx = 1
          ElseIf a >= 80 And a < 110 Then
            TwaIdx = 2
          ElseIf a >= 110 And a < 130 Then
            TwaIdx = 3
          ElseIf a >= 130 Then
            TwaIdx = 4
          End If
          ArrayTmp(TwsIdx, TwaIdx) += TimesArray(s, a)
          Totale += TimesArray(s, a)
        Next
      Next

      Dim TableTmp As String = "Tws/Twa" & vbTab & "<5 kts" & vbTab & "5-10 kts" & vbTab & "10-15 kts" & vbTab & "15-20 kts" & vbTab & ">20 kts" & vbCrLf
      For a As Integer = 0 To ArrayTmp.GetUpperBound(1)
        Select Case a
          Case 0
            TableTmp &= "<55°"
          Case 1
            TableTmp &= "55°-80°"
          Case 2
            TableTmp &= "80°-110°"
          Case 3
            TableTmp &= "110°-135°"
          Case 4
            TableTmp &= ">130°"
        End Select
        For s As Integer = 0 To ArrayTmp.GetUpperBound(0)
          TableTmp &= vbTab & (ArrayTmp(s, a) / Totale * 100).ToString("F0")
        Next
        TableTmp &= vbCrLf
      Next
      Return TableTmp
    End Function

  End Class

  Public Shared Function SimulazioneOrcTimeRange() As clsTimeRacingResult()
    Dim TWR = CaricaTargetEtRating()
    If TWR Is Nothing Then Return Nothing
    Dim cTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim cTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim cSailingState = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
    'Return RecalculateVisibleTimeRangePerformances(cTws, cTwa, cSailingState, TWR.ToArray, AppConfig.ActiveProfile.MastHeight, AppConfig.ActiveProfile.MastHeightAlfa)
    Return RecalculateVisibleTimeRangePerformances(cTws, cTwa, cSailingState, TWR.ToArray)
  End Function

  Public Shared Function SimulazioneOrcWL(LegLengthNm As Double, Tws As Double, TWR As List(Of clsTargetWithRating)) As clsTimeRacingResult()
    Return SimulateWindwardLeewardRace(TWR.ToArray, AppConfig.ActiveProfile.MastHeight, AppConfig.ActiveProfile.MastHeightAlfa, LegLengthNm, Tws)
  End Function

  Public Shared Function SimulazioneOrcCoastal(LenghtUpwind As Double, LenghtDownwind As Double, LenghtReaching60 As Double, LenghtReaching90 As Double, LenghtReaching120 As Double, Tws As Double, TWR As List(Of clsTargetWithRating)) As clsTimeRacingResult()
    'Dim TWR = CaricaTargetEtRating()
    Return SimulateCoastalRace(TWR.ToArray, AppConfig.ActiveProfile.MastHeight, AppConfig.ActiveProfile.MastHeightAlfa, LenghtUpwind, LenghtDownwind, LenghtReaching60, LenghtReaching90, LenghtReaching120, Tws)
  End Function

  Public Shared Function SecondsToTimeString(Seconds As Double) As String
    Dim NT As New DateTime(0)
    NT = NT.AddSeconds(Seconds)
    'Return NT.ToShortTimeString
    Return NT.ToString("HH:mm:ss")
  End Function

  Public Shared Function MastHeadWind(TWS As Double) As Double
    If AppConfig.ActiveProfile.MastHeight = 10 Then
      Return TWS
    Else
      Return TWS * ((AppConfig.ActiveProfile.MastHeight / 10) ^ AppConfig.ActiveProfile.MastHeightAlfa)
    End If
  End Function


  Public Shared Function SimulazioneWLetCoastal() As String
    Dim TWSs As Double() = {6, 12, 18}
    Dim ContentTmp As String = "WL 1 lap 1 Nm Leg" & vbCrLf
    Dim TWR = CaricaTargetEtRating()
    For Each Tws In TWSs
      Dim WLs = SimulazioneOrcWL(1, MastHeadWind(Tws), TWR)
      'Dim Coastals = SimulazioneOrcCoastal(1, 1, 0.3, 0.4, 0.3, Tws)
      ContentTmp &= "Tws @MH" & vbTab & "Race" & vbTab & "Boat" & vbTab & "Irc" & vbTab & "RealTime" & vbTab & "Corrected" & vbCrLf
      For Each WL In WLs
        ContentTmp &= MastHeadWind(Tws).ToString("F1") & vbTab & "WL1Nm" & vbTab & WL.Target.Tgt.Fi.Name & vbTab & WL.Target.IrcRating & vbTab & SecondsToTimeString(WL.TotalRecalculatedSeconds) & vbTab & SecondsToTimeString(WL.TotalRecalculatedSeconds * WL.Target.IrcRating) & vbCrLf
      Next
      'For Each Coastal In Coastals
      '    ContentTmp += Tws & vbTab & "Coastal" & vbTab & Coastal.Target.Tgt.Fi.Name & vbTab & Coastal.Target.IrcRating & vbTab & Coastal.TotalRecalculatedSeconds & vbTab & Coastal.TotalRecalculatedSeconds * Coastal.Target.IrcRating & vbCrLf
      'Next
    Next
    Clipboard.SetText(ContentTmp)
    ContentTmp &= vbCrLf & vbCrLf & "Coastal 1Nm Up, 1Nm Dn, 0.3Nm @60, 0.4Nm @90, 0.3Nm @120" & vbCrLf
    For Each Tws In TWSs
      'Dim WLs = SimulazioneOrcWL(1, Tws)
      Dim Coastals = SimulazioneOrcCoastal(1, 1, 0.3, 0.4, 0.3, MastHeadWind(Tws), TWR)
      ContentTmp &= "Tws @MH" & vbTab & "Race" & vbTab & "Boat" & vbTab & "Irc" & vbTab & "RealTime" & vbTab & "Corrected" & vbCrLf
      'For Each WL In WLs
      '    ContentTmp += Tws & vbTab & "WL" & vbTab & WL.Target.Tgt.Fi.Name & vbTab & WL.Target.IrcRating & vbTab & WL.TotalRecalculatedSeconds & vbTab & WL.TotalRecalculatedSeconds * WL.Target.IrcRating & vbCrLf
      'Next
      For Each Coastal In Coastals
        ContentTmp &= MastHeadWind(Tws).ToString("F1") & vbTab & "Coastal" & vbTab & Coastal.Target.Tgt.Fi.Name & vbTab & Coastal.Target.IrcRating & vbTab & SecondsToTimeString(Coastal.TotalRecalculatedSeconds) & vbTab & SecondsToTimeString(Coastal.TotalRecalculatedSeconds * Coastal.Target.IrcRating) & vbCrLf
      Next
    Next
    Return ContentTmp
  End Function

  Public Shared Function RaceReplay() As String
    Dim TWSs As Double() = {6, 12, 18}
    Dim ContentTmp As String = ""
    Dim TRs = SimulazioneOrcTimeRange()
    'Dim Coastals = SimulazioneOrcCoastal(1, 1, 0.3, 0.4, 0.3, Tws)
    ContentTmp &= "Race Replay" & vbTab & "Date" & vbTab & "Boat" & vbTab & "Irc" & vbTab & "RealTime" & vbTab & "Corrected" & vbCrLf
    For Each TR In TRs
      ContentTmp &= "Inshore" & vbTab & DataPlotSync.VisibleRange.Start.ToString("dd/MM/yyyy") & vbTab & TR.Target.Tgt.Fi.Name & vbTab & TR.Target.IrcRating & vbTab & SecondsToTimeString(TR.TotalRecalculatedSeconds) & vbTab & SecondsToTimeString(TR.TotalRecalculatedSeconds * TR.Target.IrcRating) & vbCrLf
    Next
    'ContentTmp &= vbCrLf
    ContentTmp &= "Race Wind Percentages:"
    ContentTmp &= vbCrLf
    'ContentTmp &= DataPlotSync.VisibleRange.Start.ToString("dd/MM/yyyy") & vbTab & TRs.First.Target.Tgt.Fi.Name & vbCrLf
    ContentTmp &= TRs.First.TimePercentagesTable & vbCrLf
    'For Each TR In TRs
    '    ContentTmp &= DataPlotSync.VisibleRange.Start.ToString("DD/MM/YYYY") & vbTab & TR.Target.Tgt.Fi.Name & vbCrLf
    '    ContentTmp &= TR.TimePercentagesTable & vbCrLf
    'Next
    Return ContentTmp
  End Function

  Public Shared Function LoadOrcTarget() As List(Of clsTgt)
    Dim objXls As New clsExcelVpp(clsExcelVpp.eFileType.eOrcExcelCustomFormat)
    Dim ListaFiles = objXls.OrcJsonFilesCreated
    Dim TgtTmp As New List(Of clsTgt)
    For Each f In ListaFiles
      If System.IO.File.Exists(f) Then
        Dim nt As clsTgt = clsKillerSeriale.LoadConfigurationGeneric(Of clsTgt)(f)
        If Not nt Is Nothing Then
          nt.Fi = New System.IO.FileInfo(f)
          TgtTmp.Add(nt)
        End If
      End If
    Next
    Return TgtTmp
  End Function

  Public Shared Function LeggiRatings(PathFile As String) As Dictionary(Of String, String)
    Dim ListaTgt = ObjFiles.TestoInLista(PathFile)
    ' il file deve essere nome del file target inclusa l estensione e rating irc
    Dim d As New Dictionary(Of String, String)
    For Each Riga In ListaTgt
      d.Add(Riga.Split(vbTab)(0), Riga.Split(vbTab)(1))
    Next
    Return d
  End Function


  Public Shared Function CaricaTargetEtRating() As List(Of clsTargetWithRating)
    Dim tgts = LoadOrcTarget()
    If tgts.Count = 0 Then Return Nothing
    Dim path As String = System.IO.Path.Combine(tgts.First.Fi.Directory.FullName, "orctargets_ircratings.txt")
    Dim d = LeggiRatings(path)
    Dim TWR As New List(Of clsTargetWithRating)
    For Each t In tgts
      Dim TgtFileName As String = t.Fi.Name.Replace(t.Fi.Extension, "")
      If d.ContainsKey(TgtFileName) Then
        Dim r = CDbl(d(TgtFileName))
        TWR.Add(New clsTargetWithRating(t, r))
      End If
    Next
    Return TWR
  End Function


  'Public Shared Function RecalculateVisibleTimeRangePerformances(cTws As clsChannel2020, cTwa As clsChannel2020, cSailingState As clsChannel2020, TargetsList As clsTargetWithRating(), MastHeigth As Double, Alfa As Double) As clsTimeRacingResult()
  Public Shared Function RecalculateVisibleTimeRangePerformances(cTws As clsChannel2020, cTwa As clsChannel2020, cSailingState As clsChannel2020, TargetsList As clsTargetWithRating()) As clsTimeRacingResult()
    'Dim twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eAbsTwa)
    'Dim twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaRecLeeway)
    'Dim c As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)



    Dim Miniz As Integer = DataPlotSync.VisibleRange.IdRigaIniziale
    Dim Mfin As Integer = DataPlotSync.VisibleRange.IdRigaFinale
    Dim Upwind As Integer = 0
    Dim Downwind As Integer = 0
    Dim Reaching As Integer = 0
    Dim TimesArray(30, 180) As Double
    Dim TotalManoeuveringSeconds As Double = 0
    Dim TotalNotValidSeconds As Double = 0
    If Miniz > -1 AndAlso Mfin > -1 Then
      For i As Integer = Miniz To Mfin
        Dim Tws As Double = cTws.Valori(i)
        Dim Twa As Double = cTwa.Valori(i)
        Dim SailingState As clsPeriodsManager2021.eRowType = cSailingState.Valori(i)
        If Not Double.IsNaN(Tws) AndAlso Not Double.IsNaN(Twa) AndAlso Not Double.IsNaN(SailingState) Then
          'Dim TwsAt10m As Double = Tws / ((MastHeigth / 10) ^ Alfa) ' =B22/((39/10)^0.1)
          Select Case SailingState
            Case clsPeriodsManager2021.eRowType.eGybe, clsPeriodsManager2021.eRowType.eTack
              TotalManoeuveringSeconds += 1
            Case Else
              TimesArray(CInt(Tws), CInt(Math.Abs(Twa))) += 1
              'TimesArray(CInt(TwsAt10m), CInt(Math.Abs(Twa))) += 1
          End Select
        Else
          TotalNotValidSeconds += 1
        End If
      Next
    End If

    Dim TotalSeconds As Double = 0
    Dim TotalRecalcSeconds As Double = 0

    Dim Results As New List(Of clsTimeRacingResult)

    For Each t In TargetsList
      t.Tgt.AggiornaTgtValues()
    Next

    Dim RefTgt = TargetsList.Where(Function(x) x.Tgt.Fi.Name.Contains("BaseLine") OrElse x.Tgt.Fi.Name.Contains("Reference")).FirstOrDefault
    If RefTgt Is Nothing Then RefTgt = TargetsList.First
    For Each Boat In TargetsList
      TotalSeconds = 0
      TotalRecalcSeconds = 0
      'If Not Boat Is RefTgt Then
      For s As Integer = 0 To TimesArray.GetUpperBound(0)
        For a As Integer = 0 To TimesArray.GetUpperBound(1)
          If TimesArray(s, a) > 0 Then
            Dim Target = Boat.Tgt.ValoreTgt(a < 90, s, "bs")
            Dim TgtTwa As Double = Target.Twa
            Dim Bs As Double = Target.Bs
            Dim RefBs As Double = 0
            If (a < 90 AndAlso a < TgtTwa + 10) OrElse (a > 90 AndAlso a > TgtTwa - 10) Then
              Dim RefTarget = RefTgt.Tgt.ValoreTgt(a < 90, s, "bs")
              RefBs = RefTarget.Bs
            Else
              Bs = Boat.Tgt.Valore(s, a, "bs").Bs
              RefBs = RefTgt.Tgt.Valore(s, a, "bs").Bs
            End If
            TotalSeconds += TimesArray(s, a)
            Dim k As Double = Bs / RefBs
            Dim t = TimesArray(s, a) / k
            TotalRecalcSeconds += t
          End If
        Next
      Next
      Results.Add(New clsTimeRacingResult(Boat, TimesArray, TotalSeconds, TotalRecalcSeconds, TotalManoeuveringSeconds, TotalNotValidSeconds))
      'End If
    Next


    Return Results.ToArray
  End Function
  Public Shared Function SimulateWindwardLeewardRace(TargetsList As clsTargetWithRating(), MastHeigth As Double, Alfa As Double, LegLenghtNm As Double, Tws As Double) As clsTimeRacingResult()


    Dim TotalSeconds As Double = 0
    Dim TotalRecalcSeconds As Double = 0
    Dim TimesArray(30, 180) As Double
    Dim Results As New List(Of clsTimeRacingResult)

    Dim RefTgt = TargetsList.Where(Function(x) x.Tgt.Fi.Name.Contains("BaseLine") OrElse x.Tgt.Fi.Name.Contains("Reference")).FirstOrDefault
    If RefTgt Is Nothing Then RefTgt = TargetsList.First
    Dim TgtUp = RefTgt.Tgt.ValoreTgtUp(Tws, "bs")
    Dim VmgUp As Double = TgtUp.Vmg
    Dim TmUp As Double = LegLenghtNm / VmgUp ' in ore
    TimesArray(Tws, TgtUp.Twa) = TmUp * 3600
    Dim TgtDn = RefTgt.Tgt.ValoreTgtDn(Tws, "bs")
    Dim VmgDn As Double = TgtDn.Vmg
    Dim TmDn As Double = LegLenghtNm / VmgDn ' in ore
    TimesArray(Tws, TgtDn.Twa) = TmDn * 3600
    TotalSeconds = TmUp * 3600 + TmDn * 3600

    For Each Boat In TargetsList
      TgtUp = Boat.Tgt.ValoreTgtUp(Tws, "bs")
      VmgUp = TgtUp.Vmg
      TmUp = LegLenghtNm / VmgUp ' in ore
      TimesArray(Tws, TgtUp.Twa) = TmUp * 3600
      TgtDn = Boat.Tgt.ValoreTgtDn(Tws, "bs")
      VmgDn = TgtDn.Vmg
      TmDn = LegLenghtNm / VmgDn ' in ore
      TimesArray(Tws, TgtDn.Twa) = TmDn * 3600
      TotalRecalcSeconds = TmUp * 3600 + TmDn * 3600
      Results.Add(New clsTimeRacingResult(Boat, TimesArray, TotalSeconds, TotalRecalcSeconds, 0, 0))
    Next

    Return Results.ToArray
  End Function

  Public Shared Function SimulateCoastalRace(TargetsList As clsTargetWithRating(), MastHeigth As Double, Alfa As Double, LenghtUpwind As Double, LenghtDownwind As Double, LenghtReaching60 As Double, LenghtReaching90 As Double, LenghtReaching120 As Double, Tws As Double) As clsTimeRacingResult()


    Dim TotalSeconds As Double = 0
    Dim TotalRecalcSeconds As Double = 0
    Dim TimesArray(30, 180) As Double
    Dim Results As New List(Of clsTimeRacingResult)

    For Each t In TargetsList
      t.Tgt.AggiornaTgtValues()
    Next

    Dim RefTgt = TargetsList.Where(Function(x) x.Tgt.Fi.Name.Contains("BaseLine") OrElse x.Tgt.Fi.Name.Contains("Reference")).FirstOrDefault
    If RefTgt Is Nothing Then RefTgt = TargetsList.First
    Dim TgtUp = RefTgt.Tgt.ValoreTgtUp(Tws, "bs")
    Dim VmgUp As Double = TgtUp.Vmg
    Dim TmUp As Double = LenghtUpwind / VmgUp ' in ore
    TimesArray(Tws, TgtUp.Twa) = TmUp * 3600
    Dim TgtDn = RefTgt.Tgt.ValoreTgtDn(Tws, "bs")
    Dim VmgDn As Double = TgtDn.Vmg
    Dim TmDn As Double = LenghtDownwind / VmgDn ' in ore
    TimesArray(Tws, TgtDn.Twa) = TmDn * 3600
    Dim Bs60 As Double = RefTgt.Tgt.Valore(Tws, 60, "bs").Bs
    Dim Tm60 As Double = LenghtReaching60 / Bs60 ' in ore
    TimesArray(Tws, 60) = Tm60 * 3600
    Dim Bs90 As Double = RefTgt.Tgt.Valore(Tws, 90, "bs").Bs
    Dim Tm90 As Double = LenghtReaching90 / Bs90 ' in ore
    TimesArray(Tws, 90) = Tm90 * 3600
    Dim Bs120 As Double = RefTgt.Tgt.Valore(Tws, 120, "bs").Bs
    Dim Tm120 As Double = LenghtReaching120 / Bs120 ' in ore
    TimesArray(Tws, 120) = Tm120 * 3600
    TotalSeconds = TmUp * 3600 + TmDn * 3600 + Tm60 * 3600 + Tm90 * 3600 + Tm120 * 3600

    For Each Boat In TargetsList
      TgtUp = Boat.Tgt.ValoreTgtUp(Tws, "bs")
      VmgUp = TgtUp.Vmg
      TmUp = LenghtUpwind / VmgUp ' in ore
      TimesArray(Tws, TgtUp.Twa) = TmUp * 3600
      TgtDn = Boat.Tgt.ValoreTgtDn(Tws, "bs")
      VmgDn = TgtDn.Vmg
      TmDn = LenghtDownwind / VmgDn ' in ore
      TimesArray(Tws, TgtDn.Twa) = TmDn * 3600
      Bs60 = Boat.Tgt.Valore(Tws, 60, "bs").Bs
      Tm60 = LenghtReaching60 / Bs60 ' in ore
      TimesArray(Tws, 60) = Tm60 * 3600
      Bs90 = Boat.Tgt.Valore(Tws, 90, "bs").Bs
      Tm90 = LenghtReaching90 / Bs90 ' in ore
      TimesArray(Tws, 90) = Tm90 * 3600
      Bs120 = Boat.Tgt.Valore(Tws, 120, "bs").Bs
      Tm120 = LenghtReaching120 / Bs120 ' in ore
      TimesArray(Tws, 120) = Tm120 * 3600
      TotalRecalcSeconds = TmUp * 3600 + TmDn * 3600 + Tm60 * 3600 + Tm90 * 3600 + Tm120 * 3600
      Results.Add(New clsTimeRacingResult(Boat, TimesArray, TotalSeconds, TotalRecalcSeconds, 0, 0))
    Next

    Return Results.ToArray
  End Function



End Class
