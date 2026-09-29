Imports System.IO
Imports OfficeOpenXml
Imports SPwpf
'Imports Excel = Microsoft.Office.Interop.Excel


Public Class clsXls

  Public Sub CreaPedestalReportExcelFileSinglePeriod(NewFileFullPath As String, TimeRangeSpecifico As clsTimeRange)
    Stop
    'Dim EP As New ExcelPackage()
    'EP.Workbook.Worksheets.Add("Period")
    'Dim statistiche As New clsPedestalsAnalysis(PedestalControl.Pedestals.ToList, TimeRangeSpecifico, Nothing)
    'Dim strTmp = PeriodsManager.ReportStatistichePedestals(TimeRangeSpecifico.StringaPeriodo, statistiche)
    'Clipboard.SetText(strTmp)
    'Dim StringaReport As String = strTmp
    'Dim FoglioExcel As ExcelWorksheet = EP.Workbook.Worksheets("Period")
    'RiempiXlsWorksheet(StringaReport, FoglioExcel)

    'Dim fi As New FileInfo(NewFileFullPath)
    'EP.SaveAs(fi)

  End Sub

  'Public Sub CreaMultiperiodReportExcelFile(NewFileFullPath As String, TimeRange As clsTimeRange)
  '  Dim EP As New ExcelPackage()
  '  EP.Workbook.Worksheets.Add("Summary")
  '  'EP.Workbook.Worksheets.Add("Summary")
  '  EP.Workbook.Worksheets.Add("StraightLines")

  '  'EP.Workbook.Worksheets.Add("Manoeuvers")
  '  'EP.Workbook.Worksheets.Add("Accelerations")


  '  Dim t As New clsDettagliMultiManoeuver(TimeRange)

  '  Dim l As New System.Collections.ObjectModel.ObservableCollection(Of clsPeriod2021)
  '  For Each p In t.Periods
  '    If p.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg Then
  '      l.Add(p)
  '    End If
  '  Next


  '  PeriodsManager.SalvaPeriodiJsonFile(l, TimeRange.Start.ToString("yyMMdd_HHmmss") & TimeRange.Finish.ToString("_HHmmss"))

  '  Dim StringaReport As String = t.SummaryXlsNew ' crea la stringa sommario
  '  If Not StringaReport = "" Then
  '    Dim FoglioExcel As ExcelWorksheet = EP.Workbook.Worksheets("Summary")
  '    'RiempiXlsWorkSheetMultiperiod(StringaReport, FoglioExcel, t.NumeroLati)
  '    FormattazioneMultiperiodXls(StringaReport, FoglioExcel, t.NumeroLati)
  '  End If

  '  't.ImpostaReportXls()

  '  'StringaReport = t.SummaryXls
  '  'If Not StringaReport = "" Then
  '  '  Dim FoglioExcelSL As ExcelWorksheet = EP.Workbook.Worksheets("Summary")
  '  '  RiempiXlsWorkSheetMultiperiod(StringaReport, FoglioExcelSL)
  '  'End If

  '  StringaReport = t.StraightLinesFullTable ' PeriodsManager.CreaStatistichePedestals_StraightLines
  '  If Not StringaReport = "" Then
  '    Dim FoglioExcelSL As ExcelWorksheet = EP.Workbook.Worksheets("StraightLines")
  '    RiempiXlsWorkSheetSL(StringaReport, FoglioExcelSL)
  '  End If

  '  'StringaReport = "" ' PeriodsManager.CreaStatistichePedestals_Pavarots
  '  'If Not StringaReport = "" Then
  '  '  Dim FoglioExcelPav As ExcelWorksheet = EP.Workbook.Worksheets("Manoeuvers")
  '  '  RiempiXlsWorkSheetMultiperiod(StringaReport, FoglioExcelPav)
  '  'End If

  '  'StringaReport = "" ' PeriodsManager.CreaStatistichePedestals_Accelerations
  '  'If Not StringaReport = "" Then
  '  '  Dim FoglioExcelAcc As ExcelWorksheet = EP.Workbook.Worksheets("Accelerations")
  '  '  RiempiXlsWorkSheetMultiperiod(StringaReport, FoglioExcelAcc)
  '  'End If

  '  Dim fi As New FileInfo(NewFileFullPath)
  '  Try
  '    EP.SaveAs(fi)
  '  Catch ex As Exception
  '    Dim Path As String = fi.FullName
  '    Dim Nome As String = fi.Name
  '    Dim ext As String = fi.Extension
  '    Dim NN As String = Nome.Replace(ext, "") & Now.ToString("HHmmss") & ext
  '    fi = New System.IO.FileInfo(Path.Replace(Nome, NN))
  '    EP.SaveAs(fi)
  '  End Try

  'End Sub




  Private Sub RiempiXlsWorkSheetSL(Contentuto As String, WS As ExcelWorksheet)
    Dim idRiga As Integer = 1

    Dim BRTop As Integer = -1
    Dim BRbottom As Integer = -1
    Dim BRRight As Integer = -1

    For Each Riga In Contentuto.Split(vbCrLf)
      Dim Celle As New List(Of String())
      Celle.Add(Riga.Split(vbTab))
      Dim RangeCelle As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
      WS.Cells("A" & idRiga & ":A" & idRiga).LoadFromArrays(Celle)

      BRRight = System.Math.Max(BRRight, Celle.First.Count)
      BRbottom = System.Math.Max(BRbottom, idRiga)
      If Celle.First.Count > 3 Then
        If BRTop = -1 Then BRTop = idRiga
        BRRight = System.Math.Max(BRRight, Celle.First.Count)

        If Not IsNumeric(DirectCast(Celle.First, String()).First) AndAlso Not IsNumeric(DirectCast(Celle.First, String()).Last) Then
          WS.Cells(RangeCelle).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
          WS.Cells(RangeCelle).Style.Font.Bold = True
          WS.Cells(RangeCelle).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeCelle).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)
        Else
          For i As Integer = 0 To Celle.First.Count - 1
            If IsNumeric(DirectCast(Celle.First, String())(i)) Then
              RangeCelle = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              Exit For
            End If
          Next
          For Each Cl In WS.Cells(RangeCelle)
            If IsNumeric(Cl.Value) Then
              Cl.Value = CDbl(Cl.Value)
            End If
          Next
          Dim Range As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(Range).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          If Celle.First.GetValue(2).ToString.IndexOf("Stbd") > -1 Then
            WS.Cells(Range).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen)
          ElseIf Celle.First.GetValue(2).ToString.IndexOf("Port") > -1 Then
            WS.Cells(Range).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 200, 180))
          Else
            WS.Cells(Range).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)
          End If

        End If

      End If
      idRiga += 1
    Next

    Dim BrL As String = ColonnaExcelMultiperiod(1) & BRTop & ":" & ColonnaExcelMultiperiod(BRRight) & BRbottom - 1
    WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
    WS.Cells(BrL).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

    For i As Integer = 1 To BRRight
      WS.Cells(ColonnaExcelMultiperiod(i) & BRTop).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
      WS.Cells(ColonnaExcelMultiperiod(i) & BRTop).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
      WS.Cells(ColonnaExcelMultiperiod(i) & BRTop).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
      WS.Cells(ColonnaExcelMultiperiod(i) & BRbottom - 1).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
    Next

    WS.Column(3).Width = 25
    'For i As Integer = 4 To BRRight
    '  WS.Column(i).Width = 15
    'Next


  End Sub


  Private Sub FormattazioneMultiperiodXls(Contentuto As String, WS As ExcelWorksheet, Lati As Integer)
    Dim idRiga As Integer = 1

    Dim BRTop As Integer = -1
    Dim ColonnaClear As Integer = -1
    Dim ColonnaVmgP As Integer = -1
    Dim BRRight As Integer = -1

    For Each Riga In Contentuto.Split(vbCrLf)
      Dim Celle As New List(Of String())
      Celle.Add(Riga.Split(vbTab))
      Dim RangeCelle As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
      WS.Cells("A" & idRiga & ":A" & idRiga).LoadFromArrays(Celle)
      BRRight = System.Math.Max(BRRight, Celle.First.Count)

      If Celle.First.Count > 3 Then
        Dim Intestazione As Boolean = False
        Dim Righe As Integer = (Lati * 3)
        Dim Colonne As Integer = 1
        Dim OffsetTitolo As Integer = 1
        Dim ContenutoCellaIniziale As String = Celle.First.GetValue(0).ToString.Trim

        Select Case ContenutoCellaIniziale
          Case "WholePeriod"
            Righe = 6
            Dim RangeTitolo As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 1
            WS.Cells(RangeTitolo).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            WS.Cells(RangeTitolo).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

            WS.Cells(RangeTitolo).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
            WS.Cells(RangeTitolo).Style.Font.Bold = True
            Intestazione = True

            BRTop = idRiga
          Case "LegByLeg"
            Dim RangeTitolo As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 1
            WS.Cells(RangeTitolo).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            WS.Cells(RangeTitolo).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

            WS.Cells(RangeTitolo).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
            WS.Cells(RangeTitolo).Style.Font.Bold = True
            Intestazione = True
          Case Else
            If ContenutoCellaIniziale.IndexOf("Stbd") > -1 Then
              Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
              WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen)
              'Intestazione = True
            ElseIf ContenutoCellaIniziale.IndexOf("Port") > -1 Then
              Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
              WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 200, 180))
              'Intestazione = True
            ElseIf ContenutoCellaIniziale.IndexOf("Upwind") > -1 Then
              Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
              WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)
            ElseIf ContenutoCellaIniziale.IndexOf("Downwind") > -1 Then
              Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
              WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)
            ElseIf ContenutoCellaIniziale.IndexOf("Leg") > -1 Then
              If ContenutoCellaIniziale.Trim = "Leg" Then
                Intestazione = True
              Else
                Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
                WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)
                'Intestazione = True
              End If
            End If

            For Each Cl In WS.Cells(RangeCelle)
              If IsNumeric(Cl.Value) Then
                Cl.Value = CDbl(Cl.Value)
              End If
            Next

        End Select

        If Intestazione Then
          For Each cella In Celle
            For i As Integer = 0 To cella.Count - 1
              Dim Formatta As Boolean = True

              Select Case cella(i).Trim
                Case "Vmg %"
                  ColonnaVmgP = i + 1
                  Colonne = 3
                  OffsetTitolo = 1
                  Dim RangeBold As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + Colonne) & idRiga + Righe + 1
                  WS.Cells(RangeBold).Style.Font.Bold = True
                Case "Light Wind Scenario", "Mid Wind Scenario", "Strong Wind Scenario", "Investment"
                  Colonne = 2
                  OffsetTitolo = 1
                Case "Time %", "Manoeuvers"
                  Colonne = 3
                  OffsetTitolo = 1
                Case "Configuration"
                  Colonne = 1
                  OffsetTitolo = 1
                Case "Leg", "Start", "Tws", "TgtTws", "TwsTgt", "Seconds"
                  Colonne = 1
                  OffsetTitolo = 0
                  If cella(1) = "" Then
                    Righe = 6
                  End If
                Case "_"
                  ColonnaClear = i + 1
                  Formatta = False
                Case Else
                  Formatta = False
              End Select
              If Formatta Then
                If Colonne > 1 Then
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + Colonne) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Merge = True
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                End If

                ' le intestazioni
                Dim BrTitolo As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + Colonne) & idRiga + OffsetTitolo
                WS.Cells(BrTitolo).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                WS.Cells(BrTitolo).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                WS.Cells(BrTitolo).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                WS.Cells(BrTitolo).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin

                ' i due lati
                Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + Righe + OffsetTitolo
                Dim BrR As String = ColonnaExcelMultiperiod(i + Colonne) & idRiga & ":" & ColonnaExcelMultiperiod(i + Colonne) & idRiga + Righe + OffsetTitolo
                Dim BrB As String = ColonnaExcelMultiperiod(i + 1) & idRiga + Righe + OffsetTitolo & ":" & ColonnaExcelMultiperiod(i + Colonne) & idRiga + Righe + OffsetTitolo
                WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
              End If
            Next
          Next
        End If

      End If

      'End If
      idRiga += 1
    Next

    Dim CelleClear As String = ColonnaExcelMultiperiod(ColonnaClear) & BRTop & ":" & ColonnaExcelMultiperiod(ColonnaClear) & idRiga
    WS.Cells(CelleClear).Style.Fill.PatternType = Style.ExcelFillStyle.None
    'WS.Cells(CelleClear).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White)

    WS.Column(1).Width = 20
    WS.Column(ColonnaClear).Width = 3
    For i As Integer = ColonnaClear + 1 To ColonnaClear + 7
      WS.Column(i).Width = 12
    Next
    For i As Integer = ColonnaVmgP To ColonnaVmgP + 2
      WS.Column(i).Width = 12
    Next
    WS.Column(ColonnaClear + 7).Width = 80

  End Sub


  Private Sub RiempiXlsWorkSheetMultiperiod(Contentuto As String, WS As ExcelWorksheet, Lati As Integer)
    Dim idRiga As Integer = 1

    Dim BRTop As Integer = -1
    Dim BRRight As Integer = -1

    For Each Riga In Contentuto.Split(vbCrLf)
      Dim Celle As New List(Of String())
      Celle.Add(Riga.Split(vbTab))
      Dim RangeCelle As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
      WS.Cells("A" & idRiga & ":A" & idRiga).LoadFromArrays(Celle)
      BRRight = System.Math.Max(BRRight, Celle.First.Count)

      If Celle.First.Count > 3 Then
        If Celle.First.GetValue(2) = "Tack" OrElse Celle.First.GetValue(2) = "Entry" Then
          Dim righe As Integer = (Lati * 2)
          Dim BrL As String = ColonnaExcelMultiperiod(1) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + righe
          WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrL).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
          For i As Integer = 1 To Celle.First.Count
            WS.Cells(ColonnaExcelMultiperiod(i) & idRiga).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(ColonnaExcelMultiperiod(i) & idRiga + righe).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
          Next

          If Not IsNumeric(DirectCast(Celle.First, String()).First) AndAlso Not IsNumeric(DirectCast(Celle.First, String()).Last) Then
            WS.Cells(RangeCelle).Style.Font.Bold = True
          Else
            For i As Integer = 0 To Celle.First.Count - 1
              If IsNumeric(DirectCast(Celle.First, String())(i)) Then
                RangeCelle = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
                Exit For
              End If
            Next
            For Each Cl In WS.Cells(RangeCelle)
              If IsNumeric(Cl.Value) Then
                Cl.Value = CDbl(Cl.Value)
              End If
            Next
          End If


        Else
          For Each cella In Celle
            For i As Integer = 0 To cella.Count - 1
              Select Case cella(i)
                Case "Vmg %", "Delta%ofSL", "Delta%fromStLn"
                  Dim righe As Integer = (Lati * 3)
                  If cella(i + 1) = "_" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 2) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Merge = True
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
                  WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                  WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

                  Fine = ColonnaExcelMultiperiod(i + 2) & idRiga + 1
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + 1 + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  For ii As Integer = 0 To 1
                    Dim BrR As String = ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 + righe
                    WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                  Next
                  BRTop = idRiga
                Case "Light wind scenario Versus Ghost", "Mid wind scenario Versus Ghost", "Strong wind scenario Versus Ghost", "Versus Straight Line"
                  Dim righe As Integer = (Lati * 3)
                  If cella(i + 1) = "_" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 4) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Merge = True
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
                  WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                  WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

                  Fine = ColonnaExcelMultiperiod(i + 4) & idRiga + 1
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + 1 + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  For ii As Integer = 0 To 1
                    Dim BrR As String = ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 + righe
                    WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                  Next
                  BRTop = idRiga - 1
                Case "Time %"
                  Dim righe As Integer = (Lati * 3)
                  If cella(i + 1) = "_" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 3) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Merge = True
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
                  WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                  WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

                  Fine = ColonnaExcelMultiperiod(i + 3) & idRiga + 1
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + 1 + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  For ii As Integer = 0 To 2
                    Dim BrR As String = ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 & ":" & ColonnaExcelMultiperiod(i + 2 + (2 * ii)) & idRiga + 1 + righe
                    WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                  Next
                  BRTop = idRiga
                  BRTop = idRiga
                Case "Start", "Tws", "Seconds"
                  Dim righe As Integer = (Lati * 3)
                  If cella(2) = "" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Fine = ColonnaExcelMultiperiod(i + 1) & idRiga
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  Dim BrR As String = ColonnaExcelMultiperiod(i + 2) & idRiga & ":" & ColonnaExcelMultiperiod(i + 2) & idRiga + righe
                  WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  BRTop = idRiga
                Case "Manoeuv"
                  Dim righe As Integer = (Lati * 3)
                  If cella(2) = "" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga - 1
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Fine = ColonnaExcelMultiperiod(i + 1) & idRiga
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(Fine).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(Inizio).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  'WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  'WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  Dim BrR As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  BRTop = idRiga
                Case "Type"
                  Dim righe As Integer = (Lati * 3)
                  If cella(2) = "" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(Inizio).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Fine = ColonnaExcelMultiperiod(i + 1) & idRiga
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  Dim BrR As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  BRTop = idRiga
                Case "Leg", vbLf & "Leg"
                  Dim righe As Integer = (Lati * 3)
                  If cella(2) = "" Then righe = 6
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine
                  WS.Cells(Inizio).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center

                  Fine = ColonnaExcelMultiperiod(i + 1) & idRiga
                  RangeMerge = Inizio & ":" & Fine
                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  Dim BrL As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  Dim BrR As String = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(i + 1) & idRiga + righe
                  WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

                  BRTop = idRiga
                Case "Tot Mt", "StLn Mt", "Inv Mt", "Man Mt"
                  Dim Inizio As String = ColonnaExcelMultiperiod(i + 1) & idRiga
                  Dim Fine As String = ColonnaExcelMultiperiod(i + 2) & idRiga
                  Dim RangeMerge As String = Inizio & ":" & Fine


                  WS.Cells(RangeMerge).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
                  WS.Cells(RangeMerge).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
                  BRTop = idRiga

                  BRTop = idRiga
                Case Else
              End Select
            Next
          Next
        End If

        If Celle.First.GetValue(1).IndexOf("Stbd") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen)
        ElseIf Celle.First.GetValue(1).IndexOf("Port") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 200, 180))
        ElseIf Celle.First.GetValue(2).IndexOf("Stbd") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen)
        ElseIf Celle.First.GetValue(2).IndexOf("Port") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 200, 180))
        ElseIf Celle.First.GetValue(0).IndexOf("Leg") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          If Celle.First.GetValue(1).IndexOf("Type") > -1 Then
            WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)
          ElseIf Celle.First.GetValue(1).IndexOf("Upwind") > -1 Then
            WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)

            'Dim BorderRange As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            Dim BrL As String = "A" & idRiga & ":A" & idRiga + 2
            Dim BrR As String = ColonnaExcelMultiperiod(Celle.First.Count) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            Dim BrT As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
            Dim BrB As String = "A" & idRiga + 2 & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrT).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin

          ElseIf Celle.First.GetValue(1).IndexOf("Downwind") > -1 Then
            WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)

            'Dim BorderRange As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            Dim BrL As String = "A" & idRiga & ":A" & idRiga + 2
            Dim BrR As String = ColonnaExcelMultiperiod(Celle.First.Count) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            Dim BrT As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
            Dim BrB As String = "A" & idRiga + 2 & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
            WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrT).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
            WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
          End If
        ElseIf Celle.First.GetValue(0).IndexOf("Upwind") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)

          'Dim BorderRange As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          Dim BrL As String = "A" & idRiga & ":A" & idRiga + 2
          Dim BrR As String = ColonnaExcelMultiperiod(Celle.First.Count) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          Dim BrT As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          Dim BrB As String = "A" & idRiga + 2 & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrT).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
        ElseIf Celle.First.GetValue(0).IndexOf("Downwind") > -1 Then
          Dim RangeTmp As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          WS.Cells(RangeTmp).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
          WS.Cells(RangeTmp).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow)

          'Dim BorderRange As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          Dim BrL As String = "A" & idRiga & ":A" & idRiga + 2
          Dim BrR As String = ColonnaExcelMultiperiod(Celle.First.Count) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          Dim BrT As String = "A" & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
          Dim BrB As String = "A" & idRiga + 2 & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga + 2
          WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrT).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
        End If
        If Not IsNumeric(DirectCast(Celle.First, String()).First) AndAlso Not IsNumeric(DirectCast(Celle.First, String()).Last) Then
          WS.Cells(RangeCelle).Style.Font.Bold = True
        Else
          For i As Integer = 0 To Celle.First.Count - 1
            If IsNumeric(DirectCast(Celle.First, String())(i)) Then
              RangeCelle = ColonnaExcelMultiperiod(i + 1) & idRiga & ":" & ColonnaExcelMultiperiod(Celle.First.Count) & idRiga
              Exit For
            End If
          Next
          For Each Cl In WS.Cells(RangeCelle)
            If IsNumeric(Cl.Value) Then
              Cl.Value = CDbl(Cl.Value)
            End If
          Next
        End If
      Else
      End If
      If BRTop > -1 Then
        Dim StampaBordo As Boolean = False
        If Celle.First.Count = 1 Then
          StampaBordo = True
        ElseIf Celle.First.Count > 4 Then
          StampaBordo = True
          For i As Integer = 1 To Celle.First.Count - 1
            If Not Celle.First.GetValue(i) = "" Then
              StampaBordo = False
              Exit For
            End If
          Next
        End If
        StampaBordo = False
        If StampaBordo Then
          Dim BrL As String = "A" & BRTop & ":A" & idRiga - 1
          Dim BrR As String = ColonnaExcelMultiperiod(BRRight) & BRTop & ":" & ColonnaExcelMultiperiod(BRRight) & idRiga - 1
          Dim BrT As String = "A" & BRTop & ":" & ColonnaExcelMultiperiod(BRRight) & BRTop
          Dim BrB As String = "A" & idRiga - 1 & ":" & ColonnaExcelMultiperiod(BRRight) & idRiga - 1
          WS.Cells(BrB).Style.Border.Bottom.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrT).Style.Border.Top.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrL).Style.Border.Left.Style = Style.ExcelBorderStyle.Thin
          WS.Cells(BrR).Style.Border.Right.Style = Style.ExcelBorderStyle.Thin
          BRTop = -1
          BRRight = -1
        End If
      End If
      idRiga += 1
    Next


  End Sub

  Private Function ColonnaExcelMultiperiod(Indice As Integer) As String
    'If Indice = 26 Then Stop
    If Indice = 0 Then Return "A"
    Dim Giri As Integer = System.Math.Floor(Indice / 26)
    Dim Resto As Integer = Indice Mod 26
    If Resto = 0 Then
      Giri -= 1
      Resto = 26
    End If
    Dim s As String = ""
    If Giri > 0 Then s = Char.ConvertFromUtf32(Giri + 64)
    s &= Char.ConvertFromUtf32(Resto + 64)
    Return s
  End Function


  Public Sub CreaPedestalReportExcelFile(NewFileFullPath As String, TimeRangeSpecifico As clsTimeRange)
    Stop
    'Dim EP As New ExcelPackage()
    'EP.Workbook.Worksheets.Add("WholeDay")
    'EP.Workbook.Worksheets.Add("StraightLineTests")
    'EP.Workbook.Worksheets.Add("Manoeuvers")
    'EP.Workbook.Worksheets.Add("Accelerations")

    'Dim statistiche As New clsPedestalsAnalysis(PedestalControl.Pedestals.ToList, TimeRangeSpecifico, Nothing)
    'Dim strTmp = PeriodsManager.ReportStatistichePedestals(TimeRangeSpecifico.StringaPeriodo, statistiche)
    'Dim StringaReport As String = strTmp
    'If Not StringaReport = "" Then
    '  Dim FoglioExcel As ExcelWorksheet = EP.Workbook.Worksheets("WholeDay")
    '  RiempiXlsWorksheet(StringaReport, FoglioExcel)
    'End If

    'StringaReport = PeriodsManager.CreaStatistichePedestals_StraightLines
    'If Not StringaReport = "" Then
    '  Dim FoglioExcelSL As ExcelWorksheet = EP.Workbook.Worksheets("StraightLineTests")
    '  RiempiXlsWorksheet(StringaReport, FoglioExcelSL)
    'End If

    'StringaReport = PeriodsManager.CreaStatistichePedestals_Pavarots
    'If Not StringaReport = "" Then
    '  Dim FoglioExcelPav As ExcelWorksheet = EP.Workbook.Worksheets("Manoeuvers")
    '  RiempiXlsWorksheet(StringaReport, FoglioExcelPav)
    'End If

    'StringaReport = PeriodsManager.CreaStatistichePedestals_Accelerations
    'If Not StringaReport = "" Then
    '  Dim FoglioExcelAcc As ExcelWorksheet = EP.Workbook.Worksheets("Accelerations")
    '  RiempiXlsWorksheet(StringaReport, FoglioExcelAcc)
    'End If

    'Dim fi As New FileInfo(NewFileFullPath)
    'EP.SaveAs(fi)
  End Sub

  Private Sub RiempiXlsWorksheet(Contentuto As String, WS As ExcelWorksheet)
    'For i As Integer = 0 To 200
    '  Console.WriteLine(i & ": " & ColonnaExcel(i))
    'Next

    Dim idRiga As Integer = 1
    For Each Riga In Contentuto.Split(vbCrLf)
      Dim Celle As New List(Of String())
      Celle.Add(Riga.Split(vbTab))
      'Dim RangeCelle As String = "A" & idRiga & ":" & Char.ConvertFromUtf32(Celle.First.Count + 64) & idRiga
      Dim RangeCelle As String = "A" & idRiga & ":" & ColonnaExcel(Celle.First.Count) & idRiga
      WS.Cells("A" & idRiga & ":A" & idRiga).LoadFromArrays(Celle)
      If Celle.First.Count > 3 Then
        If Not IsNumeric(DirectCast(Celle.First, String()).First) AndAlso Not IsNumeric(DirectCast(Celle.First, String()).Last) Then
          WS.Cells(RangeCelle).Style.Font.Bold = True
        Else
          For i As Integer = 0 To Celle.First.Count - 1
            If IsNumeric(DirectCast(Celle.First, String())(i)) Then
              RangeCelle = ColonnaExcel(i + 1) & idRiga & ":" & ColonnaExcel(Celle.First.Count) & idRiga
              Exit For
            End If
          Next
          'WS.Cells(RangeCelle).Style.Numberformat.Format = "0.0"
          'WS.Cells(RangeCelle).Style.Font.Italic = True
          For Each Cl In WS.Cells(RangeCelle)
            If IsNumeric(Cl.Value) Then Cl.Value = CDbl(Cl.Value)
          Next
        End If
      End If
      idRiga += 1
    Next


  End Sub

  Private Function ColonnaExcel(Indice As Integer) As String
    'If Indice = 26 Then Stop
    If Indice = 0 Then Return "A"
    Dim Giri As Integer = System.Math.Floor(Indice / 26)
    Dim Resto As Integer = Indice Mod 26
    If Resto = 0 Then
      Giri -= 1
      Resto = 26
    End If
    Dim s As String = ""
    If Giri > 0 Then s = Char.ConvertFromUtf32(Giri + 64)
    s &= Char.ConvertFromUtf32(Resto + 64)
    Return s
  End Function

End Class

Public Class clsXlsPeriod
  Dim _TR As clsTimeRange
  Dim _Descrizione As String
  Dim _Chiave As String

  Public Sub New(TR As clsTimeRange, Descrizione As String, Chiave As String)
    _TR = TR
    _Descrizione = Descrizione
    _Chiave = Chiave
  End Sub

  Public Property TR As clsTimeRange
    Get
      Return _TR
    End Get
    Set(value As clsTimeRange)
      _TR = value
    End Set
  End Property

  Public Property Descrizione As String
    Get
      Return _Descrizione
    End Get
    Set(value As String)
      _Descrizione = value
    End Set
  End Property

  Public Property Chiave As String
    Get
      Return _Chiave
    End Get
    Set(value As String)
      _Chiave = value
    End Set
  End Property
End Class


Public Class clsXlsPeriodsML
  Dim _CurrentFile As System.IO.FileInfo
  Dim _ListaPeriodi As New List(Of clsXlsPeriod)

  Public Property CurrentFile As FileInfo
    Get
      Return _CurrentFile
    End Get
    Set(value As FileInfo)
      _CurrentFile = value
    End Set
  End Property

  Public Property ListaPeriodi As List(Of clsXlsPeriod)
    Get
      Return _ListaPeriodi
    End Get
    Set(value As List(Of clsXlsPeriod))
      _ListaPeriodi = value
    End Set
  End Property

  Public Sub New()
    Dim pathfile As String = SelezionaFile()
    If System.IO.File.Exists(pathfile) Then
      _CurrentFile = New System.IO.FileInfo(pathfile)
      LeggiFileExcel()
    End If
  End Sub

  Private Function SelezionaFile() As String

    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile '   AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
    'Dim LastExt As String = "parquet"
    'If Not System.IO.Directory.Exists(UltimoPath) Then
    '  UltimoPath = AppConfig.ApplicationDataFolder
    'End If
    'Dim SelFileNames As New List(Of String)
    'Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Parquet Files |*.parquet|All Files|*.*", LastExt, SelFileNames)


    'Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastImported", AppConfig.ApplicationDataFolder, True, True)
    Dim SelectedFile As String = ObjFiles.SelezionaFile(UltimoPath, "Select Excel Periods File", "Excel Periods Files |*.xlsx|All Files|*.*", "xlsx", "")
    If SelectedFile = "" Then Return ""
    Dim Cartella As String = GetFileInfo(SelectedFile).Directory.FullName
    If Cartella.IndexOf("/") > 0 Then
      Cartella &= "/"
    Else
      Cartella &= "\"
    End If
    AppConfig.ActiveProfile.TargetFile = SelectedFile
    AppConfig.Salva()
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastImported", Cartella, True, True)
    Return SelectedFile
  End Function

  Private Sub LeggiFileExcel()
    Dim EP As New ExcelPackage(_CurrentFile)
    Dim FoglioExcelSL As ExcelWorksheet = EP.Workbook.Worksheets(EP.Workbook.Worksheets.Count - 1)
    Dim FoglioExcelG As ExcelWorksheet = EP.Workbook.Worksheets(EP.Workbook.Worksheets.Count - 1)
    Dim FoglioExcelT As ExcelWorksheet = EP.Workbook.Worksheets(EP.Workbook.Worksheets.Count - 1)
    Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets
    FoglioExcelSL = Fogli.First
    FoglioExcelG = Fogli.First
    FoglioExcelT = Fogli.First
    Dim TipoSL As Integer = -1
    Dim TipoT As Integer = -1
    Dim TipoG As Integer = -1

    For Each Foglio In Fogli
      If Foglio.Name = "Performance" Then
        FoglioExcelSL = Foglio
        TipoSL = 0
      ElseIf Foglio.Name = "StraightLine" Then
        FoglioExcelSL = Foglio
        TipoSL = 1
      ElseIf Foglio.Name = "Tacks" Then
        FoglioExcelG = Foglio
        TipoG = 0
      ElseIf Foglio.Name = "Gybes" Then
        FoglioExcelT = Foglio
        TipoT = 0
      End If
    Next

    Dim CellaInBassoDestra As ExcelAddressBase = FoglioExcelSL.Dimension

    _ListaPeriodi.Clear()
    Dim idcn As Integer = 1
    Dim idcf As Integer = 2
    Dim idct As Integer = 3
    Dim idcd As Integer = 4
    Select Case TipoSL
      Case 0
        Stop
        For IdRiga As Integer = 1 To CellaInBassoDestra.Rows ' ArrayDati.GetUpperBound(0)
          Dim ValoriRiga As New List(Of String)
          Dim RigaVuota As Boolean = True
          Dim cn = FoglioExcelSL.Cells(IdRiga, idcn).Value
          Dim cf = FoglioExcelSL.Cells(IdRiga, idcf).Value
          Dim ct = FoglioExcelSL.Cells(IdRiga, idct).Value
          Dim cd = FoglioExcelSL.Cells(IdRiga, idcd).Value
          If Not cf Is Nothing Then
            If Not cn = "Name" Then
              Dim inizio As DateTime = DateTime.Parse(cf)
              Dim fine As DateTime = DateTime.Parse(ct)
              Dim tr As New clsTimeRange(inizio, fine)
              Dim np As New clsXlsPeriod(tr, cn & " " & cd, cn)
              If DataProvider2020.TimeRange.IsFullyOverlapped(tr) Then
                _ListaPeriodi.Add(np)
              End If
            End If
          End If

        Next
      Case 1
        Dim idcdt As Integer = -1
        Dim idcmn As Integer = -1
        Dim idchs As Integer = -1
        Dim idcjc As Integer = -1
        For i As Integer = 1 To CellaInBassoDestra.Columns
          Dim Header As String = FoglioExcelSL.Cells(1, i).Value
          Select Case Header.ToLower
            Case "name"
              idcn = i
            Case "shortdescription"
              idcd = i
            Case "date"
              idcdt = i
            Case "from"
              idcf = i
            Case "to"
              idct = i
            Case "mainsail"
              idcmn = i
            Case "headsail"
              idchs = i
            Case "jibclew"
              idcjc = i
          End Select
        Next

        For IdRiga As Integer = 1 To CellaInBassoDestra.Rows ' ArrayDati.GetUpperBound(0)
          Dim ValoriRiga As New List(Of String)
          Dim RigaVuota As Boolean = True
          Dim cf = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcf)
          If Not cf Is Nothing Then
            Dim cn = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcn)
            If Not cn = "Name" Then
              Dim cd = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcd)
              Dim ct = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idct)
              Dim cdt = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcdt)
              Dim cmn = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcmn)
              Dim chs = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idchs)
              Dim cjc = ValoreDaCella(FoglioExcelSL.Cells, IdRiga, idcjc)
              Dim inizio As DateTime = DateTime.ParseExact(cdt & " " & cf, "yyyyMMdd HH:mm:ss", meCultureInfo)
              Dim fine As DateTime = DateTime.ParseExact(cdt & " " & ct, "yyyyMMdd HH:mm:ss", meCultureInfo)
              Dim tr As New clsTimeRange(inizio, fine)
              Dim np As New clsXlsPeriod(tr, cn & " " & cd & " " & cmn & " " & chs & "@" & cjc & "", cd & "_" & cmn.ToString.Replace("-", "") & "_" & chs.ToString.Replace("-", ""))
              _ListaPeriodi.Add(np)
            End If
          End If

        Next
    End Select




  End Sub

  Private Function ValoreDaCella(Cella As ExcelRange, Riga As Integer, Colonna As Integer) As Object
    If Colonna = -1 Then
      Return ""
    Else
      Return Cella(Riga, Colonna).Value
    End If
  End Function
End Class


Public Class clsExcelVpp
  Dim pCurrentFile As System.IO.FileInfo
  Dim pFilePolare As System.IO.FileInfo
  'Dim pFileXML As clsSettings

  Public Property FilePolare As FileInfo
    Get
      Return pFilePolare
    End Get
    Set(value As FileInfo)
      pFilePolare = value
    End Set
  End Property

  Public Property CurrentFile As FileInfo
    Get
      Return pCurrentFile
    End Get
    Set(value As FileInfo)
      pCurrentFile = value
    End Set
  End Property

  'Public Sub New()
  '    Dim pathfile As String = SelezionaFile()
  '    If System.IO.File.Exists(pathfile) Then
  '        'If pathfile.Contains("OrcSimulations") Then
  '        '    LeggiFileExcelOrc(pathfile, 39, 0.1)
  '        'Else
  '        pCurrentFile = New System.IO.FileInfo(pathfile)
  '        '    'ApriFile()
  '        '    'LeggiFileExcel()
  '        LeggiFileExcelV2()
  '        'End If
  '    End If
  'End Sub

  Public Enum eFileType
    eVppXlsFile
    eFaroFormat
    eExpeditionFormat
    eOrcExcelCustomFormat
  End Enum


  Public NomeFoglioExcel As String

  Public Sub New(FileType As eFileType)
    Dim pathfile As String = SelezionaFile()
    If System.IO.File.Exists(pathfile) Then
      Select Case FileType
        Case eFileType.eVppXlsFile
          pCurrentFile = New System.IO.FileInfo(pathfile)
          LeggiFileExcelV2()
        Case eFileType.eExpeditionFormat
          Stop
        Case eFileType.eFaroFormat
          Stop
        Case eFileType.eOrcExcelCustomFormat
          LeggiFileExcelOrc(pathfile, AppConfig.ActiveProfile.MastHeight, AppConfig.ActiveProfile.MastHeightAlfa)
      End Select
    End If
  End Sub

  Private Function SelezionaFile() As String
    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastImported", AppConfig.ApplicationDataFolder, True, True)
    Dim SelectedFile As String = ObjFiles.SelezionaFile(UltimoPath, "Select Excel Polar File V2", "Excel V2 Files |*.xlsx|All Files|*.*", "xlsx", "")
    If SelectedFile = "" Then Return ""
    Dim Cartella As String = GetFileInfo(SelectedFile).Directory.FullName
    If Cartella.IndexOf("/") > 0 Then
      Cartella &= "/"
    Else
      Cartella &= "\"
    End If
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastImported", Cartella, True, True)
    Return SelectedFile
  End Function

  Private Enum eTipoRiga
    eIntestazione = 0
    eCanali = 1
    eValori = 2
    eSubTable = 3

  End Enum


  Private Sub LeggiFileExcelV2()
    ' Ok per non VPP
    Dim EP As New ExcelPackage(pCurrentFile)
    Dim FoglioExcel As ExcelWorksheet = EP.Workbook.Worksheets(EP.Workbook.Worksheets.Count - 1)
    Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets
    FoglioExcel = Fogli.First
    For Each Foglio In Fogli
      If Foglio.View.TabSelected Then
        FoglioExcel = Foglio
        NomeFoglioExcel = FoglioExcel.Name
        Exit For
      End If
    Next

    Dim CellaInBassoDestra As ExcelAddressBase = FoglioExcel.Dimension

    Dim TipoRiga As New List(Of eTipoRiga)
    Dim Righe As New List(Of List(Of String))
    For IdRiga As Integer = 1 To CellaInBassoDestra.Rows ' ArrayDati.GetUpperBound(0)
      Dim ValoriRiga As New List(Of String)
      Dim RigaVuota As Boolean = True
      For IdColonna As Integer = 1 To CellaInBassoDestra.Columns ' ArrayDati.GetUpperBound(1)
        Dim C = FoglioExcel.Cells(IdRiga, IdColonna).Value
        Dim Cella As String = "" ' ArrayDati(IdRiga, IdColonna)
        If Not C Is Nothing Then
          Cella = C.ToString ' ArrayDati(IdRiga, IdColonna)
          RigaVuota = False
        End If
        ValoriRiga.Add(Cella)
      Next
      If Not RigaVuota Then
        Dim Tipo As eTipoRiga
        If ValoriRiga(0) Is Nothing Then
          Tipo = eTipoRiga.eIntestazione
        ElseIf ValoriRiga(0).ToString.StartsWith("Tws") Then
          Tipo = eTipoRiga.eCanali
        ElseIf IsNumeric(ValoriRiga(0)) Then
          Tipo = eTipoRiga.eValori
        Else
          Tipo = eTipoRiga.eSubTable
        End If
        Righe.Add(ValoriRiga)
        TipoRiga.Add(Tipo)
      End If
    Next

    Dim Tabelline As New List(Of clsTabellinaVpp)
    Dim TabellinaCorrente As clsTabellinaVpp = Nothing
    Dim VmgUpFatto As Boolean = False
    Dim VmgDnFatto As Boolean = False
    Dim SubTableType As String = ""
    For i As Integer = 0 To TipoRiga.Count - 1
      'If i = 22 Then Stop
      Dim TipoRigaCorrente As eTipoRiga = TipoRiga(i)
      Select Case TipoRigaCorrente
        Case eTipoRiga.eCanali
          If TipoRiga(i - 1) = eTipoRiga.eSubTable AndAlso TipoRiga(i + 1) = eTipoRiga.eValori Then
            If Not TabellinaCorrente Is Nothing Then
              Tabelline.Add(TabellinaCorrente)
            End If
            ' crea una nuova tabellina vpp
            Dim Descrizione As String = ""
            Dim ListaCanali As New List(Of String)
            For Each Campo In Righe(i)
              If Not Campo = Nothing Then
                If Campo.StartsWith("Tws") Then
                  Descrizione = Campo & " "
                  ListaCanali.Add(Campo.Split(" ")(0))
                Else
                  ListaCanali.Add(Campo.Replace(" ", "").Split("[")(0))
                End If
              End If
            Next
            Dim isUpVmg As Boolean = False
            Dim isDnVmg As Boolean = False
            If Not VmgUpFatto Then
              isUpVmg = SubTableType = "Upwind"
              VmgUpFatto = isUpVmg
            End If
            If Not VmgDnFatto Then
              isDnVmg = SubTableType = "Downwind"
              VmgDnFatto = isDnVmg
            End If
            TabellinaCorrente = New clsTabellinaVpp(ListaCanali, Descrizione.TrimEnd, isUpVmg, isDnVmg)
          End If
        Case eTipoRiga.eIntestazione
        Case eTipoRiga.eSubTable
          SubTableType = Righe(i)(0)
        Case eTipoRiga.eValori
          ' aggiunge i valori alla tabellina vpp corrente
          If Not TabellinaCorrente Is Nothing Then
            Dim Valori As New List(Of Double)
            For Canale As Integer = 0 To TabellinaCorrente.ListaCanali.Count - 1
              Dim strV As String = Righe(i)(Canale)
              If strV.Trim = "" OrElse Not IsNumeric(strV) Then
                ' se la cella e' vuota mette nan
                Valori.Add(Double.NaN)
                'For ii As Integer = 0 To TipoRiga.Count - i - 1

                '  If TipoRiga(i + ii) = eTipoRiga.eValori Then
                '    Dim strVtmp As String = Righe(i + ii)(Canale).Trim
                '    If strVtmp = "" Then
                '      Valori.Add(Double.NaN)
                '    Else
                '      'If TabellinaCorrente.ListaCanali(Canale) = "BoatSpeed" Then
                '      '  Dim deltaTws As Double = Righe(i + ii)(0) - Righe(i)(0)
                '      '  Dim V3 As Double = Righe(i)(0) / Righe(i + ii)(0) * CDbl(strVtmp) 'intepolazione lineare rispetto allo 0
                '      '  strV = V3 'System.Math.Max(V1, V2)
                '      'Else
                '      strV = CDbl(strVtmp) 'ultimo valore valido
                '      Valori.Add(CDbl(strV))
                '      'End If
                '      Exit For
                '    End If
                '  Else
                '    strV = "0"
                '    Valori.Add(CDbl(strV))
                '    Exit For
                '  End If
                'Next
              Else
                Valori.Add(CDbl(strV))
              End If

            Next
            TabellinaCorrente.AggiungiRiga(Valori)
            If Not Righe(i)(TabellinaCorrente.ListaCanali.Count - 1) = Nothing Then
              TabellinaCorrente.Descrizione &= " " & Righe(i)(TabellinaCorrente.IDcanaleTWS)
            End If
          End If
      End Select
    Next

    If Not TabellinaCorrente Is Nothing Then
      Tabelline.Add(TabellinaCorrente)
    End If

    Dim Tabelle = Tabelline.OrderBy(Function(x) x.LastTWA)

    CreaTgtFileJson(Tabelle.ToList)

  End Sub
  Private Sub LeggiFileExcelOrc(OrcTargetFilePath As String, MastHeigth As Double, Alfa As Double)
    ' Mettere AltezzaMhu = 10 se la polare e' gia' in testa d albero
    ' Ok per non VPP
    Dim FileCorrente As New System.IO.FileInfo(OrcTargetFilePath)
    Dim EP As New ExcelPackage(FileCorrente)
    Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets

    OrcJsonFilesCreated.Clear()

    For Each FoglioDiLavoro In Fogli
      Dim CellaInBassoDestra = FoglioDiLavoro.Dimension.End
      Dim CellaInAltoSinistra = FoglioDiLavoro.Dimension.Start


      Dim Righe As New List(Of List(Of String))
      For IdRiga As Integer = CellaInAltoSinistra.Row To CellaInBassoDestra.Row
        Dim ValoriRiga As New List(Of String)
        Dim RigaVuota As Boolean = True
        For IdColonna As Integer = CellaInAltoSinistra.Column To CellaInBassoDestra.Column
          Dim C = FoglioDiLavoro.Cells(IdRiga, IdColonna).Value
          Dim Cella As String = "" ' ArrayDati(IdRiga, IdColonna)
          If Not C Is Nothing Then
            Cella = C.ToString ' ArrayDati(IdRiga, IdColonna)
            RigaVuota = False
          End If
          ValoriRiga.Add(Cella)
        Next
        If Not RigaVuota Then
          Righe.Add(ValoriRiga)
        End If
      Next

      For r As Integer = 0 To Righe.Count - 1
        Righe(r).Insert(1, "")
      Next
      Dim RigaTmp As List(Of String) = Righe(0).ToList
      Righe.Insert(3, RigaTmp)
      RigaTmp = Righe(0).ToList
      Righe.Insert(12, RigaTmp)

      Righe(3)(0) = "0°"
      For c As Integer = 2 To Righe(3).Count - 1
        Righe(3)(c) = 0
      Next

      Righe(12)(0) = "180°"
      For c As Integer = 2 To Righe(3).Count - 1
        Righe(12)(c) = Righe(11)(c) * 0.4
      Next

      For r As Integer = 0 To Righe.Count - 1
        Select Case r
          Case 0
            Righe(r)(1) = "4 kt"
          Case 1
            Righe(r)(1) = CDbl(Righe(r)(2).Replace("°", "")) + 5
          Case 2
            Righe(r)(1) = CDbl(Righe(r)(2)) * 0.8
          Case 13
            Righe(r)(1) = CDbl(Righe(r)(2)) * 0.6
          Case 14
            Righe(r)(1) = CDbl(Righe(r)(2).Replace("°", "")) - 4
          Case Else
            Righe(r)(1) = CDbl(Righe(r)(2)) * (0.8 - (0.02 * (r - 2)))
        End Select
      Next


      Dim PrimaRiga = Righe(0)
      Dim Tabelline As New List(Of clsTabellinaVpp)
      Dim TabellinaCorrente As clsTabellinaVpp = Nothing
      Dim ListaCanali As New List(Of String)
      ListaCanali.Add("Tws")
      ListaCanali.Add("Bs")
      ListaCanali.Add("Twa")
      For idRiga As Integer = 0 To Righe.Count - 1
        ' verifica il tipo di riga dall intestazione della prima colonna
        Dim RigaCorrente = Righe(idRiga)
        If RigaCorrente(0).Contains("Beat VMG") Then
          TabellinaCorrente = New clsTabellinaVpp(ListaCanali, "Tws", True, False)
          For idColonna As Integer = 1 To Righe(idRiga).Count - 1
            Dim Valori As New List(Of Double)
            Dim Tws As Double = MastHeadTws(CDbl(PrimaRiga(idColonna).Replace("kt", "").Trim), MastHeigth, Alfa)
            Dim RigaTwa = Righe(idRiga - 1)
            Dim Twa As Double = CDbl(RigaTwa(idColonna).Replace("°", "").Trim) ' twa
            Dim Vmg As Double = CDbl(RigaCorrente(idColonna).Trim) ' vmg
            Dim Bs As Double = Vmg / (Math.Cos(Radians(Twa))) ' bs
            Valori.Add(Tws)
            Valori.Add(Bs) ' Bs
            Valori.Add(Twa) ' Twa
            TabellinaCorrente.AggiungiRiga(Valori)
          Next
        ElseIf RigaCorrente(0).Contains("°") Then
          TabellinaCorrente = New clsTabellinaVpp(ListaCanali, "Tws", False, False)
          For idColonna As Integer = 1 To Righe(idRiga).Count - 1
            Dim Valori As New List(Of Double)
            Dim Tws As Double = MastHeadTws(CDbl(PrimaRiga(idColonna).Replace("kt", "").Trim), MastHeigth, Alfa)
            Valori.Add(Tws)
            Valori.Add(CDbl(RigaCorrente(idColonna).Trim)) ' Bs
            Valori.Add(CDbl(RigaCorrente(0).Replace("°", "").Trim)) ' Twa
            TabellinaCorrente.AggiungiRiga(Valori)
          Next
        ElseIf RigaCorrente(0).Contains("Run VMG") Then
          TabellinaCorrente = New clsTabellinaVpp(ListaCanali, "Tws", False, True)
          For idColonna As Integer = 1 To Righe(idRiga).Count - 1
            Dim Valori As New List(Of Double)
            Dim Tws As Double = MastHeadTws(CDbl(PrimaRiga(idColonna).Replace("kt", "").Trim), MastHeigth, Alfa)
            Dim RigaTwa = Righe(idRiga + 1)
            Dim Twa As Double = CDbl(RigaTwa(idColonna).Replace("°", "").Trim) ' twa
            Dim Vmg As Double = CDbl(RigaCorrente(idColonna).Trim) ' vmg
            Dim Bs As Double = -Vmg / (Math.Cos(Radians(Twa))) ' bs
            Valori.Add(Tws)
            Valori.Add(Bs) ' Bs
            Valori.Add(Twa) ' Twa
            TabellinaCorrente.AggiungiRiga(Valori)
          Next
        Else
          TabellinaCorrente = Nothing
        End If
        If Not TabellinaCorrente Is Nothing Then
          Tabelline.Add(TabellinaCorrente)
        End If

      Next


      'Dim Tabelle = Tabelline.OrderBy(Function(x) x.LastTWA)
      Dim Tabelle = CorreggiEtOrdinaTabella(Tabelline, 2, 1)
      Dim NewJsonFilePath As String = Path.Combine(FileCorrente.Directory.FullName, FoglioDiLavoro.Name & ".json")
      OrcJsonFilesCreated.Add(NewJsonFilePath)
      CreaTgtFileJson(Tabelle.ToList, NewJsonFilePath)

    Next

    ApriExplorer(FileCorrente.Directory.FullName)

  End Sub

  Public Property OrcJsonFilesCreated As New List(Of String)

  Private Function CorreggiEtOrdinaTabella(Tabella As List(Of clsTabellinaVpp), IdTwa As Integer, IdBs As Integer) As List(Of clsTabellinaVpp)
    Dim VmgUp = Tabella.Where(Function(x) x.IsUpwindTarget = True).FirstOrDefault
    Dim VmgDn = Tabella.Where(Function(x) x.IsDownwindTarget = True).FirstOrDefault
    Dim Altre = Tabella.Where(Function(x) x.IsUpwindTarget = False AndAlso x.IsDownwindTarget = False).ToList
    Dim NuovaTabella As New List(Of clsTabellinaVpp)
    NuovaTabella.Add(VmgUp)
    NuovaTabella.Add(VmgDn)
    For Each Rc In Altre
      For TwsId As Integer = 0 To Rc.RigheValori.Count - 1
        Dim Twa As Double = Rc.RigheValori(TwsId)(IdTwa)
        Dim Bs As Double = Rc.RigheValori(TwsId)(IdBs)
        Dim Vmg As Double = Math.Abs(Bs * Math.Cos(Radians(Twa)))
        Dim NewBs As Double = Bs
        If Twa < 80 Then
          Dim VmgTgtTwa As Double = VmgUp.RigheValori(TwsId)(IdTwa)
          Dim VmgTgtBs As Double = VmgUp.RigheValori(TwsId)(IdBs)
          Dim VmgTgtVmg As Double = VmgTgtBs * Math.Cos(Radians(VmgTgtTwa))
          If Vmg > VmgTgtVmg Then
            NewBs = VmgTgtVmg / Math.Cos(Radians(Twa))
          End If
        ElseIf Twa > 100 Then
          Dim VmgTgtTwa As Double = VmgDn.RigheValori(TwsId)(IdTwa)
          Dim VmgTgtBs As Double = VmgDn.RigheValori(TwsId)(IdBs)
          Dim VmgTgtVmg As Double = Math.Abs(VmgTgtBs * Math.Cos(Radians(VmgTgtTwa)))
          If Vmg > VmgTgtVmg Then
            NewBs = Math.Abs(VmgTgtVmg / Math.Cos(Radians(Twa)))
          End If
        End If
        Rc.RigheValori(TwsId)(IdBs) = NewBs
      Next
      NuovaTabella.Add(Rc)
    Next
    Return NuovaTabella.OrderBy(Function(x) x.LastTWA).ToList
  End Function


  Private Function MastHeadTws(Tws As Double, MastHeigth As Double, Alfa As Double) As Double
    ' ricalcola il vento in testa d albero
    If MastHeigth = 10 Then
      Return Tws
    Else
      Return Tws * ((MastHeigth / 10) ^ Alfa)
    End If
  End Function

  Private Sub LeggiFileExcel()
    ' Ok per VPP
    Dim EP As New ExcelPackage(pCurrentFile)
    Dim FoglioExcel As ExcelWorksheet = EP.Workbook.Worksheets(EP.Workbook.Worksheets.Count - 1)
    Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets
    FoglioExcel = Fogli.First
    For Each Foglio In Fogli
      If Foglio.View.TabSelected Then
        FoglioExcel = Foglio
        Exit For
      End If
    Next

    Dim CellaInBassoDestra As ExcelAddressBase = FoglioExcel.Dimension

    Dim TipoRiga As New List(Of eTipoRiga)
    Dim Righe As New List(Of List(Of String))
    For IdRiga As Integer = 1 To CellaInBassoDestra.Rows ' ArrayDati.GetUpperBound(0)
      Dim ValoriRiga As New List(Of String)
      Dim RigaVuota As Boolean = True
      For IdColonna As Integer = 1 To CellaInBassoDestra.Columns ' ArrayDati.GetUpperBound(1)
        Dim C = FoglioExcel.Cells(IdRiga, IdColonna).Value
        Dim Cella As String = "" ' ArrayDati(IdRiga, IdColonna)
        If Not C Is Nothing Then
          Cella = C.ToString ' ArrayDati(IdRiga, IdColonna)
          RigaVuota = False
        End If
        ValoriRiga.Add(Cella)
      Next
      If Not RigaVuota Then
        Dim Tipo As eTipoRiga
        If ValoriRiga(0) Is Nothing Then
          Tipo = eTipoRiga.eIntestazione
        ElseIf ValoriRiga(0).ToString.StartsWith("TWS") Then
          Tipo = eTipoRiga.eCanali
        ElseIf IsNumeric(ValoriRiga(0)) Then
          Tipo = eTipoRiga.eValori
        Else
          Tipo = eTipoRiga.eIntestazione
        End If
        Righe.Add(ValoriRiga)
        TipoRiga.Add(Tipo)
      End If
    Next

    Dim Tabelline As New List(Of clsTabellinaVpp)
    Dim TabellinaCorrente As clsTabellinaVpp = Nothing
    Dim VmgUpFatto As Boolean = False
    Dim VmgDnFatto As Boolean = False
    For i As Integer = 0 To TipoRiga.Count - 1
      'If i = 22 Then Stop
      Dim TipoRigaCorrente As eTipoRiga = TipoRiga(i)
      Select Case TipoRigaCorrente
        Case eTipoRiga.eCanali
          If TipoRiga(i - 1) = eTipoRiga.eIntestazione AndAlso TipoRiga(i + 1) = eTipoRiga.eValori Then
            If Not TabellinaCorrente Is Nothing Then
              Tabelline.Add(TabellinaCorrente)
            End If
            ' crea una nuova tabellina vpp
            Dim Descrizione As String = ""
            Dim ListaCanali As New List(Of String)
            For Each Campo In Righe(i)
              If Not Campo = Nothing Then
                If Campo.StartsWith("TWS") Then
                  Descrizione = Campo & " "
                  ListaCanali.Add(Campo.Split(" ")(0))
                Else
                  ListaCanali.Add(Campo.Replace(" ", "").Split("[")(0))
                End If
              End If
            Next
            For Each Nota In Righe(i - 1)
              If Not Nota = Nothing Then
                Descrizione &= Nota & " "
              End If
            Next
            Dim isUpVmg As Boolean = False
            Dim isDnVmg As Boolean = False
            If Not VmgUpFatto Then
              isUpVmg = Descrizione.LastIndexOf("UPWIND") > -1
              VmgUpFatto = isUpVmg
            End If
            If Not VmgDnFatto Then
              isDnVmg = Descrizione.LastIndexOf("DOWNWIND") > -1
              VmgDnFatto = isDnVmg
            End If
            TabellinaCorrente = New clsTabellinaVpp(ListaCanali, Descrizione.TrimEnd, isUpVmg, isDnVmg)
          End If
        Case eTipoRiga.eIntestazione
        Case eTipoRiga.eValori
          ' aggiunge i valori alla tabellina vpp corrente
          If Not TabellinaCorrente Is Nothing Then
            Dim Valori As New List(Of Double)
            For Canale As Integer = 0 To TabellinaCorrente.ListaCanali.Count - 1
              Dim strV As String = Righe(i)(Canale)
              If strV.Trim = "" Then
                For ii As Integer = 0 To TipoRiga.Count - i - 1
                  If TipoRiga(i + ii) = eTipoRiga.eValori Then
                    Dim strVtmp As String = Righe(i + ii)(Canale).Trim
                    If Not strVtmp = "" Then
                      If TabellinaCorrente.ListaCanali(Canale) = "BoatSpeed" Then
                        Dim deltaTws As Double = Righe(i + ii)(0) - Righe(i)(0)
                        Dim V3 As Double = Righe(i)(0) / Righe(i + ii)(0) * CDbl(strVtmp) 'intepolazione lineare rispetto allo 0
                        strV = V3 'System.Math.Max(V1, V2)
                      Else
                        strV = CDbl(strVtmp) 'ultimo valore valido
                      End If
                      Exit For
                    End If
                  Else
                    strV = "0"
                    Exit For
                  End If
                Next
              End If
              Valori.Add(CDbl(strV))

            Next
            TabellinaCorrente.AggiungiRiga(Valori)
            If Not Righe(i)(TabellinaCorrente.ListaCanali.Count) = Nothing Then
              TabellinaCorrente.Descrizione &= " " & Righe(i)(TabellinaCorrente.ListaCanali.Count)
            End If
          End If
      End Select
    Next

    If Not TabellinaCorrente Is Nothing Then
      Tabelline.Add(TabellinaCorrente)
    End If

    Dim Tabelle = Tabelline.OrderBy(Function(x) x.LastTWA)

    CreaTgtFileJson(Tabelle.ToList)


  End Sub



  'Private Sub ApriFile()


  '  pApp = New Excel.Application
  '  pFileExcel = pApp.Workbooks.Open(pCurrentFile.FullName)

  '  pSheets = pFileExcel.Sheets

  '  pActiveSheet = pFileExcel.ActiveSheet

  '  Dim RangeDati As Excel.Range = pActiveSheet.UsedRange
  '  Dim ArrayDati(,) As Object = RangeDati.Value(Excel.XlRangeValueDataType.xlRangeValueDefault)
  '  Dim TipoRiga As New List(Of eTipoRiga)
  '  Dim Righe As New List(Of List(Of String))
  '  For IdRiga As Integer = 1 To ArrayDati.GetUpperBound(0)
  '    Dim ValoriRiga As New List(Of String)
  '    Dim RigaVuota As Boolean = True
  '    For IdColonna As Integer = 1 To ArrayDati.GetUpperBound(1)
  '      Dim Cella As String = ArrayDati(IdRiga, IdColonna)
  '      ValoriRiga.Add(Cella)
  '      If RigaVuota AndAlso Not Cella = Nothing Then RigaVuota = False
  '    Next
  '    If Not RigaVuota Then
  '      Dim Tipo As eTipoRiga
  '      If ValoriRiga(0) Is Nothing Then
  '        Tipo = eTipoRiga.eIntestazione
  '      ElseIf ValoriRiga(0).ToString.StartsWith("TWS") Then
  '        Tipo = eTipoRiga.eCanali
  '      ElseIf IsNumeric(ValoriRiga(0)) Then
  '        Tipo = eTipoRiga.eValori
  '      Else
  '        Tipo = eTipoRiga.eIntestazione
  '      End If
  '      Righe.Add(ValoriRiga)
  '      TipoRiga.Add(Tipo)
  '    End If
  '  Next

  '  Dim Tabelline As New List(Of clsTabellinaVpp)
  '  Dim TabellinaCorrente As clsTabellinaVpp = Nothing
  '  Dim VmgUpFatto As Boolean = False
  '  Dim VmgDnFatto As Boolean = False
  '  For i As Integer = 0 To TipoRiga.Count - 1
  '    Dim TipoRigaCorrente As eTipoRiga = TipoRiga(i)
  '    Select Case TipoRigaCorrente
  '      Case eTipoRiga.eCanali
  '        If TipoRiga(i - 1) = eTipoRiga.eIntestazione AndAlso TipoRiga(i + 1) = eTipoRiga.eValori Then
  '          If Not TabellinaCorrente Is Nothing Then
  '            Tabelline.Add(TabellinaCorrente)
  '          End If
  '          ' crea una nuova tabellina vpp
  '          Dim Descrizione As String = ""
  '          Dim ListaCanali As New List(Of String)
  '          For Each Campo In Righe(i)
  '            If Not Campo = Nothing Then
  '              If Campo.StartsWith("TWS") Then
  '                Descrizione = Campo & " "
  '                ListaCanali.Add(Campo.Split(" ")(0))
  '              Else
  '                ListaCanali.Add(Campo.Replace(" ", "").Split("[")(0))
  '              End If
  '            End If
  '          Next
  '          For Each Nota In Righe(i - 1)
  '            If Not Nota = Nothing Then
  '              Descrizione &= Nota & " "
  '            End If
  '          Next
  '          Dim isUpVmg As Boolean = False
  '          Dim isDnVmg As Boolean = False
  '          If Not VmgUpFatto Then
  '            isUpVmg = Descrizione.LastIndexOf("UPWIND") > -1
  '            VmgUpFatto = isUpVmg
  '          End If
  '          If Not VmgDnFatto Then
  '            isDnVmg = Descrizione.LastIndexOf("DOWNWIND") > -1
  '            VmgDnFatto = isDnVmg
  '          End If
  '          TabellinaCorrente = New clsTabellinaVpp(ListaCanali, Descrizione.TrimEnd, isUpVmg, isDnVmg)
  '        End If
  '      Case eTipoRiga.eIntestazione
  '      Case eTipoRiga.eValori
  '        ' aggiunge i valori alla tabellina vpp corrente
  '        If Not TabellinaCorrente Is Nothing Then
  '          Dim Valori As New List(Of Double)
  '          For Canale As Integer = 0 To TabellinaCorrente.ListaCanali.Count - 1
  '            Valori.Add(CDbl(Righe(i)(Canale)))
  '          Next
  '          TabellinaCorrente.AggiungiRiga(Valori)
  '          If Not Righe(i)(TabellinaCorrente.ListaCanali.Count) = Nothing Then
  '            TabellinaCorrente.Descrizione &= " " & Righe(i)(TabellinaCorrente.ListaCanali.Count)
  '          End If
  '        End If
  '    End Select
  '  Next

  '  If Not TabellinaCorrente Is Nothing Then
  '    Tabelline.Add(TabellinaCorrente)
  '  End If

  '  pFileExcel.Close()
  '  pApp.Quit()

  '  Dim Tabelle = Tabelline.OrderBy(Function(x) x.LastTWA)

  '  CreaFileTraget(Tabelle.ToList)

  'End Sub

  Private Sub CreaTgtFileJson(Tabelline As List(Of clsTabellinaVpp))
    Dim tgtM As New clsTgtManager
    Dim NomeXlsx As String = pCurrentFile.FullName
    Dim estensione As String = pCurrentFile.Extension
    'Dim NomeFileJson As String = NomeXlsx.Replace(estensione, ".json").Replace(" ", "")
    Dim NomeFileJson As String = System.IO.Path.Combine(pCurrentFile.Directory.FullName, NomeFoglioExcel & ".json")
    Dim idUpVmg As Integer = 0
    Dim idDnVmg As Integer = 0

    Dim Tgt = New clsTgt
    Tgt.MainVarName = "tws"


    For Each tabellina In Tabelline
      For Each Valore In tabellina.RigheValori
        Dim Tws As Double = Valore(tabellina.IDcanaleTWS)
        'Dim Twa As Double = Valore(tabellina.IDcanaleTWA) + Valore(tabellina.IDcanaleLwy)
        Dim RigaValori As clsTgtRowValues = Tgt.TgtRowValues.Where(Function(x) x.Value = Tws).FirstOrDefault
        If RigaValori Is Nothing Then
          RigaValori = New clsTgtRowValues(Tws, Tgt)
          Tgt.TgtRowValues.Add(RigaValori)
        End If
        Dim tipo As clsTgtColonna.eTgtType = clsTgtColonna.eTgtType.None
        If tabellina.IsUpwindTarget Then
          tipo = clsTgtColonna.eTgtType.Upwind
        ElseIf tabellina.IsDownwindTarget Then
          tipo = clsTgtColonna.eTgtType.Downwind
        End If
        Dim Punto = New clsTgtColonna(tipo, RigaValori)
        RigaValori.TgtPoints.Add(Punto)
        For i As Integer = 0 To Valore.Count - 1
          Select Case i
            Case tabellina.IDcanaleTWS
              'non aggiunge nulla
            Case tabellina.IDcanaleBS
              Punto.Values.Add("bs", Valore(i))
              'Case tabellina.IDcanaleTWA
              '  Punto.Values.Add("twa", Valore(i) + Valore(tabellina.IDcanaleLwy))
              '  Punto.Values.Add("twa", Valore(i) + Valore(tabellina.IDcanaleLwy))
            Case Else
              Punto.Values.Add(tabellina.ListaCanali(i).ToLower, Valore(i))
          End Select
        Next
      Next
    Next
    Tgt.AggiornaTgtValues()

    'For idTws As Integer = 0 To Tabelline.First.RigheValori.Count - 1
    '  Dim tws As Double = Tabelline.First.Tws(idTws)
    '  Tgt.TgtRowValues.Add(New clsTgtRowValues(tws, Tgt)) 'aggiunge una riga per tws
    '  For IdTabella As Integer = 0 To Tabelline.Count - 1
    '    Dim Tabella As clsTabellinaVpp = Tabelline(IdTabella)
    '    If idTws = 0 Then
    '      If Tabella.IsUpwindTarget Then idUpVmg = IdTabella
    '      If Tabella.IsDownwindTarget Then idDnVmg = IdTabella
    '      'Descrizione &= Tabella.Descrizione & vbCrLf
    '    End If
    '    Dim twa As Double = Tabella.Twa(idTws)
    '    If twa = 0 Then twa = PrimaTWAvalida(Tabella)
    '    'Dim stringaOrdinata As String = "TWA_" & Format(twa, "F1").ToString.Replace(".", "d")
    '    Dim bs As Double = Tabella.Bs(idTws)
    '    'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, "Bs", bs, True, False)
    '    For idCanale As Integer = 0 To Tabella.ListaCanali.Count - 1
    '      If Tabella.IsCanaleNonChiave(idCanale) Then
    '        Dim Intestazione As String = Tabella.ListaCanali(idCanale)
    '        Dim Valore As Double = Tabella.RigheValori(idTws).Item(idCanale)
    '        'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, Intestazione, Valore, True, False)
    '      End If
    '    Next
    '  Next

    'Next

    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", NomeFileJson, True, True)
    AppConfig.ActiveProfile.TargetFile = NomeFileJson
    AppConfig.Salva()
    tgtM.SalvaJsonTgtFile(Tgt)
  End Sub

  Private Sub CreaTgtFileJson(Tabelline As List(Of clsTabellinaVpp), TargetFileFullPath As String)
    Dim tgtM As New clsTgtManager
    Dim idUpVmg As Integer = 0
    Dim idDnVmg As Integer = 0

    Dim CurrentTgtFile = New clsTgt
    CurrentTgtFile.MainVarName = "tws"


    For Each tabellina In Tabelline
      For Each Valore In tabellina.RigheValori
        Dim Tws As Double = Valore(tabellina.IDcanaleTWS)
        'Dim Twa As Double = Valore(tabellina.IDcanaleTWA) + Valore(tabellina.IDcanaleLwy)
        Dim RigaValori As clsTgtRowValues = CurrentTgtFile.TgtRowValues.Where(Function(x) x.Value = Tws).FirstOrDefault
        If RigaValori Is Nothing Then
          RigaValori = New clsTgtRowValues(Tws, CurrentTgtFile)
          CurrentTgtFile.TgtRowValues.Add(RigaValori)
        End If
        Dim tipo As clsTgtColonna.eTgtType = clsTgtColonna.eTgtType.None
        If tabellina.IsUpwindTarget Then
          tipo = clsTgtColonna.eTgtType.Upwind
        ElseIf tabellina.IsDownwindTarget Then
          tipo = clsTgtColonna.eTgtType.Downwind
        End If
        Dim Punto = New clsTgtColonna(tipo, RigaValori)
        RigaValori.TgtPoints.Add(Punto)
        For i As Integer = 0 To Valore.Count - 1
          Select Case i
            Case tabellina.IDcanaleTWS
              'non aggiunge nulla
            Case tabellina.IDcanaleBS
              Punto.Values.Add("bs", Valore(i))
              'Case tabellina.IDcanaleTWA
              '  Punto.Values.Add("twa", Valore(i) + Valore(tabellina.IDcanaleLwy))
              '  Punto.Values.Add("twa", Valore(i) + Valore(tabellina.IDcanaleLwy))
            Case Else
              Punto.Values.Add(tabellina.ListaCanali(i).ToLower, Valore(i))
          End Select
        Next
      Next
    Next
    'CurrentTgtFile.AggiornaTgtValues()

    clsKillerSeriale.SaveConfigurationGeneric(Of clsTgt)(CurrentTgtFile, TargetFileFullPath)


    'AppConfig.ActiveProfile.TargetFile = NomeFileJson
    'AppConfig.Salva()
    'tgtM.SalvaJsonTgtFile(Tgt)
  End Sub


  'Public Sub CreaFileTargetTest()
  '  _tgt = New clsTgt
  '  Dim Canali As String() = {"twa", "bs", "leeway", "heel", "minsink", "cantangle"}
  '  _tgt.MainVarName = "tws"
  '  For i As Integer = 8 To 26 Step 2
  '    _tgt.TgtRowValues.Add(New clsTgtRowValues(i, _tgt))
  '    Dim tgtType As clsTgtPoint.eTgtType
  '    For ii As Integer = 30 To 150 Step 20
  '      tgtType = clsTgtPoint.eTgtType.None
  '      Dim Twa As Double = ii
  '      If ii = 50 Then
  '        Twa = 44
  '        tgtType = clsTgtPoint.eTgtType.Upwind
  '      ElseIf ii = 130 Then
  '        Twa = 146
  '        tgtType = clsTgtPoint.eTgtType.Downwind
  '      End If
  '      _tgt.TgtRowValues.Last.TgtPoints.Add(New clsTgtPoint(tgtType, _tgt.TgtRowValues.Last))
  '      For Each Canale In Canali
  '        Dim idx As Integer = Canali.ToList.IndexOf(Canale)
  '        _tgt.TgtRowValues.Last.TgtPoints.Last.Values.Add(Canale, idx + i + ii)
  '      Next
  '    Next
  '  Next
  '  _tgt.AggiornaTgtValues()

  '  SalvaJsonTgtFile()
  'End Sub


  'Private Sub CreaFileTraget(Tabelline As List(Of clsTabellinaVpp))
  '  Dim NomeXlsx As String = pCurrentFile.FullName
  '  Dim estensione As String = pCurrentFile.Extension
  '  Dim NomeFileXml As String = NomeXlsx.Replace(estensione, ".xml").Replace(" ", "")
  '  Dim idUpVmg As Integer = 0
  '  Dim idDnVmg As Integer = 0
  '  Dim Descrizione As String = ""
  '  pFileXML = New clsSettings(NomeFileXml, "LR2021", "Performance Software Polar File")

  '  Dim NodoRadice As Xml.XmlNode = pFileXML.CercaNodo("PerformanceTable", clsSettings.eNodoSTD.eChannels, True)
  '  If Not NodoRadice Is Nothing Then pFileXML.EliminaNodo(NodoRadice)


  '  For idTws As Integer = 0 To Tabelline.First.RigheValori.Count - 1
  '    Dim tws As Double = Tabelline.First.Tws(idTws)
  '    Dim stringaAscissa As String = "TWS_" & Format(tws, "F1").ToString.Replace(".", "d")

  '    For IdTabella As Integer = 0 To Tabelline.Count - 1
  '      Dim Tabella As clsTabellinaVpp = Tabelline(IdTabella)
  '      If idTws = 0 Then
  '        If Tabella.IsUpwindTarget Then idUpVmg = IdTabella
  '        If Tabella.IsDownwindTarget Then idDnVmg = IdTabella
  '        Descrizione &= Tabella.Descrizione & vbCrLf
  '      End If
  '      Dim twa As Double = Tabella.Twa(idTws)
  '      If twa = 0 Then twa = PrimaTWAvalida(Tabella)
  '      Dim stringaOrdinata As String = "TWA_" & Format(twa, "F1").ToString.Replace(".", "d")
  '      Dim bs As Double = Tabella.Bs(idTws)
  '      pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, "Bs", bs, True, False)
  '      For idCanale As Integer = 0 To Tabella.ListaCanali.Count - 1
  '        If Tabella.IsCanaleNonChiave(idCanale) Then
  '          Dim Intestazione As String = Tabella.ListaCanali(idCanale)
  '          Dim Valore As Double = Tabella.RigheValori(idTws).Item(idCanale)
  '          pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, Intestazione, Valore, True, False)
  '        End If
  '      Next
  '    Next

  '  Next

  '  'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Upwind", idUpVmg, True, False)
  '  'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Downwind", idDnVmg, True, False)

  '  'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Annotations", "description", Descrizione, True, False)
  '  'pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Annotations", "filesource", pCurrentFile.FullName, True, False)
  '  'If System.IO.File.Exists(NomeFileXml) Then
  '  '	System.IO.File.Delete(NomeFileXml)
  '  'End If

  '  pFileXML.SalvaFileXML()

  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "TargetFiles", "LastLoaded", NomeFileXml, True, True)

  '  pFilePolare = New System.IO.FileInfo(NomeFileXml)

  'End Sub

  Private Function PrimaTWAvalida(Tabella As clsTabellinaVpp) As Double
    For i As Integer = 0 To Tabella.RigheValori.Count - 1
      Dim TWA As Double = Tabella.Twa(i)
      If TWA > 0 Then Return TWA
    Next
    Return 0
  End Function

End Class

Public Class clsTabellinaVpp
  Dim pListaCanali As List(Of String)
  Dim pDescrizione As String
  Dim pRigheValori As New List(Of List(Of Double))
  Dim pIsUpwindTarget As Boolean
  Dim pIsDownwindTarget As Boolean
  Dim pIDcanaleTWS As Integer
  Dim pIDcanaleTWA As Integer
  Dim pIDcanaleCWA As Integer
  Dim pIDcanaleLwy As Integer
  Dim pIDcanaleBS As Integer
  Dim pLastTWA As Double

  Public Sub New(ListaCanali As List(Of String), Descrizione As String, IsUpwindTarget As Boolean, IsDownwindTarget As Boolean)
    pListaCanali = ListaCanali
    pDescrizione = Descrizione
    pIsUpwindTarget = IsUpwindTarget
    pIsDownwindTarget = IsDownwindTarget
    MappaCanaliChiave()
  End Sub

  Public ReadOnly Property Tws(indice As Integer) As Double
    Get
      Return RigheValori.Item(indice).Item(pIDcanaleTWS)
    End Get
  End Property

  Public ReadOnly Property Twa(indice As Integer) As Double
    Get
      Return RigheValori.Item(indice).Item(pIDcanaleTWA) + RigheValori.Item(indice).Item(pIDcanaleLwy)
    End Get
  End Property

  Public ReadOnly Property Bs(indice As Integer) As Double
    Get
      Return RigheValori.Item(indice).Item(pIDcanaleBS)
    End Get
  End Property

  Public ReadOnly Property IsCanaleNonChiave(indice As Integer) As Boolean
    Get
      Select Case indice
        Case pIDcanaleBS, pIDcanaleCWA, pIDcanaleTWS
          Return False
        Case Else
          Return True
      End Select
    End Get
  End Property

  Public Property ListaCanali As List(Of String)
    Get
      Return pListaCanali
    End Get
    Set(value As List(Of String))
      pListaCanali = value
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

  Public Property RigheValori As List(Of List(Of Double))
    Get
      Return pRigheValori
    End Get
    Set(value As List(Of List(Of Double)))
      pRigheValori = value
    End Set
  End Property

  Public Property IsUpwindTarget As Boolean
    Get
      Return pIsUpwindTarget
    End Get
    Set(value As Boolean)
      pIsUpwindTarget = value
    End Set
  End Property

  Public Property IsDownwindTarget As Boolean
    Get
      Return pIsDownwindTarget
    End Get
    Set(value As Boolean)
      pIsDownwindTarget = value
    End Set
  End Property

  Public Property IDcanaleTWS As Integer
    Get
      Return pIDcanaleTWS
    End Get
    Set(value As Integer)
      pIDcanaleTWS = value
    End Set
  End Property

  Public Property IDcanaleTWA As Integer
    Get
      Return pIDcanaleTWA
    End Get
    Set(value As Integer)
      pIDcanaleTWA = value
    End Set
  End Property

  Public Property IDcanaleCWA As Integer
    Get
      Return pIDcanaleCWA
    End Get
    Set(value As Integer)
      pIDcanaleCWA = value
    End Set
  End Property

  Public Property IDcanaleLwy As Integer
    Get
      Return pIDcanaleLwy
    End Get
    Set(value As Integer)
      pIDcanaleLwy = value
    End Set
  End Property

  Public Property IDcanaleBS As Integer
    Get
      Return pIDcanaleBS
    End Get
    Set(value As Integer)
      pIDcanaleBS = value
    End Set
  End Property

  Public Property LastTWA As Double
    Get
      Return pLastTWA
    End Get
    Set(value As Double)
      pLastTWA = value
    End Set
  End Property

  Private Sub MappaCanaliChiave()
    For i As Integer = 0 To pListaCanali.Count - 1
      Select Case pListaCanali(i).ToLower
        Case "tws"
          pIDcanaleTWS = i
        Case "bs"
          pIDcanaleBS = i
        Case "leeway"
          pIDcanaleLwy = i
        Case "twa"
          pIDcanaleTWA = i
        Case "cwa"
          pIDcanaleCWA = i
        Case Else
      End Select
    Next
  End Sub

  Public Sub AggiungiRiga(Valori As List(Of Double))
    pRigheValori.Add(Valori)
    If Valori(pIDcanaleTWA) > 0 Then pLastTWA = Valori(pIDcanaleTWA)
    'calcolaTWA(Valori(pIDcanaleTWA), Valori(pIDcanaleLwy))
  End Sub

  Private Sub calcolaTWA(FakeTWA As Double, Leeway As Double)
    Dim vTmp As Double = FakeTWA + Leeway
    If vTmp > 0 Then pLastTWA = vTmp
  End Sub

End Class

'Public Class clsPolare2019
'  Dim pNomeMainChannel As String 'normalmente TWS
'  Dim pNomeSecondaryChannel As String 'normalmente TWA
'  Dim pListaCanaliValori As New List(Of clsPolare2019CanaleValori) 'nomrmalmente almeno la BS
'  Dim pValoriMainChannel As New List(Of Double) ' elenco delle TWS del file polare
'  Dim pTipo = eTipoPolare.ePolareSP
'  Dim pFileXML As clsSettings
'  Dim pFilePolare As System.IO.FileInfo

'  Public Enum eTipoPolare
'    eNonRiconosciuto = 0
'    eFaroRowTwsColsTwa = 1
'    eFaRoRowTwsColsBsTwa = 2
'    eExpedition = 3
'    eVpp = 4
'    ePolareSP = 5
'  End Enum

'  Public Sub New(PathFile As String)
'    pTipo = eTipoPolare.ePolareSP
'    pFilePolare = New System.IO.FileInfo(PathFile)
'    CaricaPolare2019(PathFile)
'  End Sub

'  Public ReadOnly Property GetPolare(NomeCanale As String) As clsPolare2019CanaleValori
'    Get
'      For Each Canale As clsPolare2019CanaleValori In pListaCanaliValori
'        If Canale.NomeCanale.ToLower = NomeCanale.ToLower Then
'          Return Canale
'        End If
'      Next
'      Return Nothing
'    End Get
'  End Property

'  Public Property ListaCanaliValori As List(Of clsPolare2019CanaleValori)
'    Get
'      Return pListaCanaliValori
'    End Get
'    Set(value As List(Of clsPolare2019CanaleValori))
'      pListaCanaliValori = value
'    End Set
'  End Property

'  Public Property NomeMainChannel As String
'    Get
'      Return pNomeMainChannel
'    End Get
'    Set(value As String)
'      pNomeMainChannel = value
'    End Set
'  End Property

'  Public Property NomeSecondaryChannel As String
'    Get
'      Return pNomeSecondaryChannel
'    End Get
'    Set(value As String)
'      pNomeSecondaryChannel = value
'    End Set
'  End Property

'  Public Property ValoriMainChannel As List(Of Double)
'    Get
'      Return pValoriMainChannel
'    End Get
'    Set(value As List(Of Double))
'      pValoriMainChannel = value
'    End Set
'  End Property

'  Public Property FilePolare As FileInfo
'    Get
'      Return pFilePolare
'    End Get
'    Set(value As FileInfo)
'      pFilePolare = value
'    End Set
'  End Property

'  Public Sub CaricaPolare2019(PathFile As String)
'    Dim pKpoppa As Double = 0.3
'    pNomeMainChannel = ""
'    pNomeSecondaryChannel = "" ' Nothing
'    pValoriMainChannel.Clear()
'    Dim ValoriSecondaryChannel As New List(Of Double) ' elenco delle TWA del file polare
'    Dim ListaCanaliValoriTemp As New List(Of clsPolare2019CanaleValori) 'nomrmalmente almeno la BS
'    pListaCanaliValori.Clear()
'    pFileXML = New clsSettings(pFilePolare.FullName, "LR2021", "Performance Software Polar File")
'    Dim NodoPolare As Xml.XmlNode = pFileXML.CercaNodo("PerformanceTable", clsSettings.eNodoSTD.eChannels, True)
'    If Not NodoPolare Is Nothing Then
'      For Each Nodo As Xml.XmlNode In NodoPolare
'        Select Case Nodo.Name
'          Case "Table"
'            Dim MainChannelIndex As Integer = 0 ' parte da 1 perchè la riga 0 è artificiale per i valori a tws=0

'            For Each NodoMainChannel As Xml.XmlNode In Nodo
'              If pNomeMainChannel = "" Then
'                pNomeMainChannel = NodoMainChannel.Name.Split("_")(0).ToLower
'              End If
'              pValoriMainChannel.Add(CDbl(NodoMainChannel.Name.Split("_")(1).Replace("d", ".")))
'              If ListaCanaliValoriTemp.Count > 0 Then
'                For Each Canalevalori In ListaCanaliValoriTemp
'                  Canalevalori.AggiungiRiga(pValoriMainChannel(MainChannelIndex))
'                Next
'              End If
'              If NodoMainChannel.HasChildNodes Then
'                For Each NodoSecondaryChannel As Xml.XmlNode In NodoMainChannel.ChildNodes
'                  If pNomeSecondaryChannel = "" Then
'                    pNomeSecondaryChannel = NodoSecondaryChannel.Name.Split("_")(0).ToLower
'                  End If
'                  ValoriSecondaryChannel.Add(CDbl(NodoSecondaryChannel.Name.Split("_")(1).Replace("d", ".")))
'                  If NodoSecondaryChannel.HasChildNodes Then
'                    If NodoSecondaryChannel Is NodoMainChannel.FirstChild Then
'                      If NodoMainChannel Is Nodo.FirstChild Then
'                        'al primo passaggio viene riempita la lista dei canalivalore
'                        For Each Canale As Xml.XmlNode In NodoSecondaryChannel.ChildNodes
'                          Dim CanaleTmp As String = Canale.Name
'                          ListaCanaliValoriTemp.Add(New clsPolare2019CanaleValori(CanaleTmp, pNomeSecondaryChannel.ToLower = "twa" AndAlso CanaleTmp.ToLower = "bs", Me))
'                          ListaCanaliValoriTemp.Last.AggiungiRiga(0) ' riga iniziale con TWS 0
'                          ListaCanaliValoriTemp.Last.RigheMainChannel.Last.AggiungiCoppiaValori(0, 0) ' aggiunge la coppia bs e twa = 0 per tws = 0
'                          ListaCanaliValoriTemp.Last.AggiungiRiga(pValoriMainChannel(MainChannelIndex))
'                          'ListaCanaliValoriTemp.Last.RigheMainChannel.Last.AggiungiCoppiaValori(0, 0) ' aggiunge la coppia bs e twa = 0 per tws = 0
'                        Next
'                      End If
'                    End If
'                    For i As Integer = 0 To NodoSecondaryChannel.ChildNodes.Count - 1
'                      If NodoSecondaryChannel Is NodoMainChannel.FirstChild Then
'                        ListaCanaliValoriTemp(i).RigheMainChannel(MainChannelIndex + 1).AggiungiCoppiaValori(0, 0)
'                      End If

'                      If MainChannelIndex = 0 Then
'                        ' riempie i valori della prima riga solo al primo giro mettendo i valori della secondary come quelli della prima valida
'                        ListaCanaliValoriTemp(i).RigheMainChannel(MainChannelIndex).AggiungiCoppiaValori(ValoriSecondaryChannel.Last, 0)
'                        If NodoSecondaryChannel Is NodoMainChannel.LastChild Then
'                          ListaCanaliValoriTemp(i).RigheMainChannel(MainChannelIndex).AggiungiCoppiaValori(180, 0)
'                        End If
'                      End If
'                      Dim NodoCanaliValori = NodoSecondaryChannel.ChildNodes(i)
'                      ListaCanaliValoriTemp(i).RigheMainChannel(MainChannelIndex + 1).AggiungiCoppiaValori(ValoriSecondaryChannel.Last, NodoCanaliValori.InnerText)
'                      If NodoSecondaryChannel Is NodoMainChannel.LastChild Then
'                        ListaCanaliValoriTemp(i).RigheMainChannel(MainChannelIndex + 1).AggiungiCoppiaValori(180, pKpoppa * CDbl(NodoCanaliValori.InnerText))
'                      End If

'                    Next
'                  End If
'                Next
'              End If
'              MainChannelIndex += 1
'            Next
'        End Select
'      Next
'      Dim CanaleVmg As clsPolare2019CanaleValori = Nothing
'      For Each CanaleValori In ListaCanaliValoriTemp
'        If CanaleValori.IsVmgChannel Then
'          CanaleVmg = CanaleValori
'          Exit For
'        End If
'      Next
'      If Not CanaleVmg Is Nothing Then
'        'per essere sicuri che il canale vmg sia il primo ad essere fatto
'        CanaleVmg.RiempiMatriceCompleta(CanaleVmg)
'        pListaCanaliValori.Add(CanaleVmg)
'        For Each CanaleValori In ListaCanaliValoriTemp
'          If Not CanaleValori Is CanaleVmg Then
'            If CanaleValori.RigheMainChannel.Count = CanaleVmg.RigheMainChannel.Count Then
'              If CanaleValori.RigheMainChannel.First.CoppieValori.Count = CanaleVmg.RigheMainChannel.First.CoppieValori.Count Then
'                CanaleValori.RiempiMatriceCompleta(CanaleVmg)
'                pListaCanaliValori.Add(CanaleValori)
'              End If
'            End If
'          End If
'        Next
'      End If
'    End If



'  End Sub



'End Class

'Public Class clsPolare2019CanaleValori
'  Dim pParent As clsPolare2019
'  Dim pNomeCanale As String 'normalmente BS
'  Dim pIsVmgChannel As Boolean
'  Dim pRigheMainChannel As New List(Of clsPolare2019Righe)
'  Dim pDefSplineType As clsPolare2019Righe.eSplineType = clsPolare2019Righe.eSplineType.eAkima
'  Dim pMatriceValoriCompleta As Double(,)
'  Dim pObjInterpolatore As New alglib.spline1dinterpolant 'richiede Alglib2 www.alglib.net
'  Dim pValoriTargetUp As clsPolare2019ValoriTarget
'  Dim pValoriTargetDn As clsPolare2019ValoriTarget
'  Dim pCanaliSecondari As clsPolare2019DatiSecondari

'  Public Sub New(NomeCanale As String, IsVmgChannel As Boolean, Parent As clsPolare2019)
'    pNomeCanale = NomeCanale
'    pIsVmgChannel = IsVmgChannel
'    pParent = Parent
'    pCanaliSecondari = New clsPolare2019DatiSecondari(Me)
'  End Sub

'  Public Property RigheMainChannel As List(Of clsPolare2019Righe)
'    Get
'      Return pRigheMainChannel
'    End Get
'    Set(value As List(Of clsPolare2019Righe))
'      pRigheMainChannel = value
'    End Set

'  End Property

'  Public Property IsVmgChannel As Boolean
'    Get
'      Return pIsVmgChannel
'    End Get
'    Set(value As Boolean)
'      pIsVmgChannel = value
'    End Set
'  End Property

'  Public Property NomeCanale As String
'    Get
'      Return pNomeCanale
'    End Get
'    Set(value As String)
'      pNomeCanale = value
'    End Set
'  End Property

'  Public Property Parent As clsPolare2019
'    Get
'      Return pParent
'    End Get
'    Set(value As clsPolare2019)
'      pParent = value
'    End Set
'  End Property

'  Public Property CanaliSecondari As clsPolare2019DatiSecondari
'    Get
'      Return pCanaliSecondari
'    End Get
'    Set(value As clsPolare2019DatiSecondari)
'      pCanaliSecondari = value
'    End Set
'  End Property

'  Public Sub AggiungiRiga(ValoreMainChannel As Double)
'    pRigheMainChannel.Add(New clsPolare2019Righe(ValoreMainChannel, pIsVmgChannel, pDefSplineType))
'  End Sub


'  Public Sub RiempiMatriceCompleta(CanaleVmg As clsPolare2019CanaleValori)
'    pValoriTargetUp = New clsPolare2019ValoriTarget
'    pValoriTargetDn = New clsPolare2019ValoriTarget

'    Dim IndiciTgUp As New List(Of Integer)
'    Dim IndiciTgDn As New List(Of Integer)
'    For i As Integer = 0 To pRigheMainChannel.Count - 1
'      If Not pIsVmgChannel Then
'        pRigheMainChannel(i).ImpostaIndiciTgtUpDn(CanaleVmg.RigheMainChannel(i).IdColonnaTgUp, CanaleVmg.RigheMainChannel(i).IdColonnaTgDn)
'      End If
'      pRigheMainChannel(i).OrdinaEtInterpola()
'      IndiciTgUp.Add(pRigheMainChannel(i).IdColonnaTgUp)
'      IndiciTgDn.Add(pRigheMainChannel(i).IdColonnaTgDn)
'    Next
'    If IsVmgChannel AndAlso pRigheMainChannel(0).ValoreMainChannel = 0 Then
'      pRigheMainChannel(0).ImpostaIndiciTgtUpDn(pRigheMainChannel(1).IdColonnaTgUp, pRigheMainChannel(1).IdColonnaTgDn)
'    End If



'    Dim MainChannelCount As Integer = pRigheMainChannel.Last.ValoreMainChannel
'    Dim SecondaryChannelCount As Integer = pRigheMainChannel.First.TabellaCompletaValueChannel.Keys.Last
'    ReDim pMatriceValoriCompleta(MainChannelCount, SecondaryChannelCount) 'Tws, Twa
'    For i As Integer = 0 To pRigheMainChannel.First.TabellaCompletaValueChannel.Last.Key
'      Dim Valore As Double
'      Dim Trovato As Boolean = pRigheMainChannel.First.TabellaCompletaValueChannel.TryGetValue(i, Valore)
'      If Trovato Then
'        Dim pIndici As New List(Of Double)
'        Dim pValori As New List(Of Double)
'        For Each RigaMainChannel In pRigheMainChannel
'          pIndici.Add(RigaMainChannel.ValoreMainChannel)
'          pValori.Add(RigaMainChannel.TabellaCompletaValueChannel.Item(i))
'        Next
'        Select Case pDefSplineType
'          Case clsPolare2019Righe.eSplineType.eAkima
'            alglib.spline1dbuildakima(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'          Case clsPolare2019Righe.eSplineType.eCatmullRom
'            alglib.spline1dbuildcatmullrom(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'          Case clsPolare2019Righe.eSplineType.eCubic
'            alglib.spline1dbuildcubic(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'              'Case eSplineType.eHermite
'              '    alglib.spline1dbuildhermite(Indici.ToArray, Valori.ToArray, pObjInterpolatore)
'          Case clsPolare2019Righe.eSplineType.eLinear
'            alglib.spline1dbuildlinear(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'          Case clsPolare2019Righe.eSplineType.eMonotone
'            alglib.spline1dbuildmonotone(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'        End Select
'        For ii = pRigheMainChannel.First.ValoreMainChannel To pRigheMainChannel.Last.ValoreMainChannel Step 1
'          Dim ValoreInterpolato As Double = alglib.spline1dcalc(pObjInterpolatore, ii)
'          pMatriceValoriCompleta(ii, i) = ValoreInterpolato
'        Next
'      End If
'    Next

'    For Each Riga In pRigheMainChannel
'      pValoriTargetUp.AggiungiCoppia(Riga.ValoreMainChannel, Riga.TargetUpReference, Riga.TargetUpValue)
'      pValoriTargetDn.AggiungiCoppia(Riga.ValoreMainChannel, Riga.TargetDnReference, Riga.TargetDnValue)
'    Next
'    pValoriTargetUp.RiempiMatriceCompleta(pDefSplineType)
'    pValoriTargetDn.RiempiMatriceCompleta(pDefSplineType)

'  End Sub

'  Public ReadOnly Property Valore(MainChannelValue As Integer, SecondaryChannelValue As Integer) As Double
'    Get
'      Return pMatriceValoriCompleta(System.Math.Min(MainChannelValue, pMatriceValoriCompleta.GetLength(0) - 1), System.Math.Min(SecondaryChannelValue, pMatriceValoriCompleta.GetLength(1) - 1))
'    End Get
'  End Property

'  Public ReadOnly Property Valore(MainChannelValue As Double, SecondaryChannelValue As Double) As Double
'    Get
'      If Double.IsNaN(MainChannelValue) OrElse Double.IsNaN(SecondaryChannelValue) Then Return Double.NaN

'      Dim idxmain As Integer = System.Math.Min(Int(MainChannelValue), pMatriceValoriCompleta.GetLength(0) - 1)
'      Dim idxsec As Integer = System.Math.Min(Int(SecondaryChannelValue), pMatriceValoriCompleta.GetLength(1) - 1)
'      Dim Vinf As Double = pMatriceValoriCompleta(idxmain, idxsec)
'      idxmain = System.Math.Min(Int(MainChannelValue) + 1, pMatriceValoriCompleta.GetLength(0) - 1)
'      idxsec = System.Math.Min(Int(SecondaryChannelValue) + 1, pMatriceValoriCompleta.GetLength(1) - 1)
'      Dim Vsup As Double = pMatriceValoriCompleta(idxmain, idxsec)
'      Dim K As Double = MainChannelValue - Int(MainChannelValue)
'      Return Vinf * (1 - K) + Vsup * K
'    End Get
'  End Property

'  Public ReadOnly Property ValoreMainChannelEquivalentTargetUp(MainChannelValue As Double, Coefficient As Double) As Double
'    Get
'      Dim BsTgt As Double = ValoreTargetUp(MainChannelValue)
'      Dim BsEq As Double = BsTgt * Coefficient / 100
'      If Coefficient < 100 Then
'        For i As Integer = MainChannelValue - 1 To 0 Step -1
'          Dim TgTmp As Double = ValoreTargetUp(i)
'          If TgTmp < BsEq Then
'            Dim Delta As Double = (BsEq - TgTmp) * (MainChannelValue - i) / (BsTgt - TgTmp)
'            Return i + Delta
'            Exit For
'          End If
'        Next
'      Else
'        For i As Integer = MainChannelValue + 1 To 30 Step 1
'          Dim TgTmp As Double = ValoreTargetUp(i)
'          If TgTmp > BsEq Then
'            Dim Delta As Double = (TgTmp - BsEq) * (i - MainChannelValue) / (TgTmp - BsTgt)
'            Return i - Delta
'            Exit For
'          End If
'        Next
'      End If
'      Stop
'      Return 0
'    End Get
'  End Property

'  Public ReadOnly Property ValoreMainChannelEquivalentTargetDn(MainChannelValue As Double, Coefficient As Double) As Double
'    Get
'      Dim BsTgt As Double = ValoreTargetDn(MainChannelValue)
'      Dim BsEq As Double = BsTgt * Coefficient / 100
'      If Coefficient < 100 Then
'        For i As Integer = MainChannelValue - 1 To 0 Step -1
'          Dim TgTmp As Double = ValoreTargetDn(i)
'          If TgTmp < BsEq Then
'            Dim Delta As Double = (BsEq - TgTmp) * (MainChannelValue - i) / (BsTgt - TgTmp)
'            Return i + Delta
'            Exit For
'          End If
'        Next
'      Else
'        For i As Integer = MainChannelValue + 1 To 30 Step 1
'          Dim TgTmp As Double = ValoreTargetDn(i)
'          If TgTmp > BsEq Then
'            Dim Delta As Double = (TgTmp - BsEq) * (i - MainChannelValue) / (TgTmp - BsTgt)
'            Return i - Delta
'            Exit For
'          End If
'        Next
'      End If
'      Stop
'      Return 0
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetUp(MainChannelValue As Integer) As Double
'    Get
'      Return pValoriTargetUp.TargetValue(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetDn(MainChannelValue As Integer) As Double
'    Get
'      Return pValoriTargetDn.TargetValue(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetUpReferece(MainChannelValue As Integer) As Double
'    Get
'      Return pValoriTargetUp.TargetValueReference(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetDnReferece(MainChannelValue As Integer) As Double
'    Get
'      Return pValoriTargetDn.TargetValueReference(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTarget(IsUp As Boolean, MainChannelValue As Double) As Double
'    Get
'      If IsUp Then
'        Return pValoriTargetUp.TargetValue(MainChannelValue)
'      Else
'        Return pValoriTargetDn.TargetValue(MainChannelValue)
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetReference(IsUp As Boolean, MainChannelValue As Double) As Double
'    Get
'      If IsUp Then
'        Return pValoriTargetUp.TargetValueReference(MainChannelValue)
'      Else
'        Return pValoriTargetDn.TargetValueReference(MainChannelValue)
'      End If
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetVmg(IsUp As Boolean, MainChannelValue As Double) As Double
'    Get
'      Dim Bs As Double
'      Dim Twa As Double
'      If IsUp Then
'        Bs = pValoriTargetUp.TargetValueReference(MainChannelValue)
'        Twa = pValoriTargetUp.TargetValueReference(MainChannelValue)
'      Else
'        Bs = pValoriTargetDn.TargetValueReference(MainChannelValue)
'        Twa = pValoriTargetDn.TargetValueReference(MainChannelValue)
'      End If
'      Return Bs * System.Math.Cos(Radians(Twa))
'    End Get
'  End Property

'  Public Sub ValoreTargetApparent(IsUp As Boolean, MainChannelValue As Double, ByRef Awa As Double, ByRef Aws As Double)
'    Dim Bs As Double
'    Dim Twa As Double
'    If IsUp Then
'      Bs = pValoriTargetUp.TargetValueReference(MainChannelValue)
'      Twa = pValoriTargetUp.TargetValueReference(MainChannelValue)
'    Else
'      Bs = pValoriTargetDn.TargetValueReference(MainChannelValue)
'      Twa = pValoriTargetDn.TargetValueReference(MainChannelValue)
'    End If
'    ApparentFromTrue(Awa, Aws, Twa, MainChannelValue, Bs)
'  End Sub

'  Public ReadOnly Property ValoreTargetUp(MainChannelValue As Double) As Double
'    Get
'      Return pValoriTargetUp.TargetValue(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetDn(MainChannelValue As Double) As Double
'    Get
'      Return pValoriTargetDn.TargetValue(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetUpReferece(MainChannelValue As Double) As Double
'    Get
'      Return pValoriTargetUp.TargetValueReference(MainChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property ValoreTargetDnReferece(MainChannelValue As Double) As Double
'    Get
'      Return pValoriTargetDn.TargetValueReference(MainChannelValue)
'    End Get
'  End Property

'End Class

'Public Class clsPolare2019DatiSecondari
'  Dim pCanaleValori As clsPolare2019CanaleValori

'  Public Sub New(CanaleValori As clsPolare2019CanaleValori)
'    pCanaleValori = CanaleValori

'  End Sub


'  Public ReadOnly Property AggiornaValoriUp(MainChannelValue As Double) As List(Of Double)
'    Get
'      Dim BSt As Double = pCanaleValori.ValoreTargetUp(MainChannelValue)
'      Dim Twat As Double = pCanaleValori.ValoreTargetUp(MainChannelValue)
'      Dim VmgT As Double = BSt * System.Math.Abs(System.Math.Cos(Radians(Twat)))
'      Dim Awa As Double
'      Dim Aws As Double
'      ApparentFromTrue(Awa, Aws, Twat, MainChannelValue, BSt)
'      Dim ListaTmp As New List(Of Double)
'      ListaTmp.Add(VmgT)
'      ListaTmp.Add(Awa)
'      ListaTmp.Add(Aws)
'      Return ListaTmp
'    End Get
'  End Property

'  Public ReadOnly Property AggiornaValoriDn(MainChannelValue As Double) As List(Of Double)
'    Get
'      Dim BSt As Double = pCanaleValori.ValoreTargetDn(MainChannelValue)
'      Dim Twat As Double = pCanaleValori.ValoreTargetDn(MainChannelValue)
'      Dim VmgT As Double = BSt * System.Math.Abs(System.Math.Cos(Radians(Twat)))
'      Dim Awa As Double
'      Dim Aws As Double
'      ApparentFromTrue(Awa, Aws, Twat, MainChannelValue, BSt)
'      Dim ListaTmp As New List(Of Double)
'      ListaTmp.Add(VmgT)
'      ListaTmp.Add(Awa)
'      ListaTmp.Add(Aws)
'      Return ListaTmp
'    End Get
'  End Property

'End Class

'Public Class clsPolare2019ValoriTarget
'  Dim pPrimaryChannelValues As New List(Of Double)
'  Dim pSecondayChannelValues As New List(Of Double)
'  Dim pValueChannelValues As New List(Of Double)
'  Dim pSecondayChannelValuesCompleta As New List(Of Double)
'  Dim pValueChannelValuesCompleta As New List(Of Double)
'  Dim ObjInterpolatoreSecondary As New alglib.spline1dinterpolant 'richiede Alglib2 www.alglib.net
'  Dim ObjInterpolatoreValue As New alglib.spline1dinterpolant 'richiede Alglib2 www.alglib.net


'  Public Sub AggiungiCoppia(PrimaryChannelValue As Double, SecondayChannelValue As Double, ValueChannelValues As Double)
'    pPrimaryChannelValues.Add(PrimaryChannelValue)
'    pSecondayChannelValues.Add(SecondayChannelValue)
'    pValueChannelValues.Add(ValueChannelValues)
'  End Sub

'  Public ReadOnly Property TargetValue(PrimaryChannelValue As Integer) As Double
'    Get
'      Return pValueChannelValuesCompleta(PrimaryChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property TargetValueReference(PrimaryChannelValue As Integer) As Double
'    Get
'      Return pSecondayChannelValuesCompleta(PrimaryChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property TargetValue(PrimaryChannelValue As Double) As Double
'    Get
'      Return alglib.spline1dcalc(ObjInterpolatoreValue, PrimaryChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property TargetValueReference(PrimaryChannelValue As Double) As Double
'    Get
'      Return alglib.spline1dcalc(ObjInterpolatoreSecondary, PrimaryChannelValue)
'    End Get
'  End Property

'  Public Sub RiempiMatriceCompleta(SplineType As clsPolare2019Righe.eSplineType)

'    Dim pIndici As New List(Of Double)
'    Dim pvaloriSec As New List(Of Double)
'    Dim pValoriVal As New List(Of Double)
'    For i As Integer = 0 To pSecondayChannelValues.Count - 1
'      pIndici.Add(pPrimaryChannelValues(i))
'      pvaloriSec.Add(pSecondayChannelValues(i))
'      pValoriVal.Add(pValueChannelValues(i))
'    Next
'    Select Case SplineType
'      Case clsPolare2019Righe.eSplineType.eAkima
'        alglib.spline1dbuildakima(pIndici.ToArray, pvaloriSec.ToArray, ObjInterpolatoreSecondary)
'        alglib.spline1dbuildakima(pIndici.ToArray, pValoriVal.ToArray, ObjInterpolatoreValue)
'      Case clsPolare2019Righe.eSplineType.eCatmullRom
'        alglib.spline1dbuildcatmullrom(pIndici.ToArray, pvaloriSec.ToArray, ObjInterpolatoreSecondary)
'        alglib.spline1dbuildcatmullrom(pIndici.ToArray, pValoriVal.ToArray, ObjInterpolatoreValue)
'      Case clsPolare2019Righe.eSplineType.eCubic
'        alglib.spline1dbuildcubic(pIndici.ToArray, pvaloriSec.ToArray, ObjInterpolatoreSecondary)
'        alglib.spline1dbuildcubic(pIndici.ToArray, pValoriVal.ToArray, ObjInterpolatoreValue)
'      Case clsPolare2019Righe.eSplineType.eLinear
'        alglib.spline1dbuildlinear(pIndici.ToArray, pvaloriSec.ToArray, ObjInterpolatoreSecondary)
'        alglib.spline1dbuildlinear(pIndici.ToArray, pValoriVal.ToArray, ObjInterpolatoreValue)
'      Case clsPolare2019Righe.eSplineType.eMonotone
'        alglib.spline1dbuildmonotone(pIndici.ToArray, pvaloriSec.ToArray, ObjInterpolatoreSecondary)
'        alglib.spline1dbuildmonotone(pIndici.ToArray, pValoriVal.ToArray, ObjInterpolatoreValue)
'    End Select
'    For ii = 0 To pPrimaryChannelValues.Where(Function(x) Not Double.IsNaN(x)).Max Step 1
'      Dim ValoreInterpolatoSec As Double = alglib.spline1dcalc(ObjInterpolatoreSecondary, ii)
'      Dim ValoreInterpolatoVal As Double = alglib.spline1dcalc(ObjInterpolatoreValue, ii)
'      pSecondayChannelValuesCompleta.Add(ValoreInterpolatoSec)
'      pValueChannelValuesCompleta.Add(ValoreInterpolatoVal)
'    Next

'  End Sub

'End Class

'Public Class clsPolare2019Righe
'  Dim pCoppieValori As New List(Of clsPolare2019Coppia) 'le coppie SecondaryChannel, ValueChannel (Twa, Bs)
'  Dim pCoppieOrdinate As List(Of clsPolare2019Coppia) 'le coppie SecondaryChannel, ValueChannel (Twa, Bs)
'  Dim pValoreMainChannel As Double 'il Main Channel (Tws)
'  Dim pIsVmgChannel As Boolean
'  Dim pIdColonnaTgUp As Integer
'  Dim pIdColonnaTgDn As Integer
'  Dim pObjInterpolatore As New alglib.spline1dinterpolant 'richiede Alglib2 www.alglib.net
'  Dim pSplineType As eSplineType
'  Dim pTabellaCompletaValueChannel As New Dictionary(Of Integer, Double)

'  Public Enum eSplineType
'    eLinear = 0
'    eCubic = 1
'    'eHermite = 2
'    eAkima = 3
'    eCatmullRom = 4
'    eMonotone = 5
'  End Enum

'  Public ReadOnly Property SplineType As eSplineType
'    Get
'      Return pSplineType
'    End Get
'  End Property

'  Public Sub New(ValoreMainChannel As Double, IsVmgChannel As Boolean, SplineType As eSplineType)
'    pValoreMainChannel = ValoreMainChannel
'    pIsVmgChannel = IsVmgChannel
'    pSplineType = SplineType
'  End Sub

'  Public Property CoppieOrdinate As List(Of clsPolare2019Coppia)
'    Get
'      Return pCoppieOrdinate
'    End Get
'    Set(value As List(Of clsPolare2019Coppia))
'      pCoppieOrdinate = value
'    End Set
'  End Property

'  Public ReadOnly Property CoppieValori As List(Of clsPolare2019Coppia)
'    Get
'      Return pCoppieValori
'    End Get
'  End Property

'  Public Property ValoreMainChannel As Double
'    Get
'      Return pValoreMainChannel
'    End Get
'    Set(value As Double)
'      pValoreMainChannel = value
'    End Set
'  End Property

'  Public Property IsVmgChannel As Boolean
'    Get
'      Return pIsVmgChannel
'    End Get
'    Set(value As Boolean)
'      pIsVmgChannel = value
'    End Set
'  End Property

'  Public Property IdColonnaTgUp As Integer
'    Get
'      Return pIdColonnaTgUp
'    End Get
'    Set(value As Integer)
'      pIdColonnaTgUp = value
'    End Set
'  End Property

'  Public Property IdColonnaTgDn As Integer
'    Get
'      Return pIdColonnaTgDn
'    End Get
'    Set(value As Integer)
'      pIdColonnaTgDn = value
'    End Set
'  End Property

'  Public Property TabellaCompletaValueChannel As Dictionary(Of Integer, Double)
'    Get
'      Return pTabellaCompletaValueChannel
'    End Get
'    Set(value As Dictionary(Of Integer, Double))
'      pTabellaCompletaValueChannel = value
'    End Set
'  End Property

'  Public Sub AggiungiCoppiaValori(SecondaryChannelValue As Double, ValueChannelValue As Double)
'    pCoppieValori.Add(New clsPolare2019Coppia(SecondaryChannelValue, ValueChannelValue))
'  End Sub

'  Public Sub OrdinaEtInterpola()
'    If pCoppieValori.Count = 7 Then Stop
'    'le ordina e prende gli indici dalle coppie ordinate
'    pCoppieOrdinate = pCoppieValori.OrderBy(Function(x) x.SecondaryChannelValue).ToList
'    If pIsVmgChannel Then
'      Dim VmgUp As Double = 0
'      Dim VmgDn As Double = 0
'      For i As Integer = 0 To pCoppieOrdinate.Count - 1
'        Dim VmgTmp As Double = pCoppieOrdinate(i).ValueChannelValue * System.Math.Cos(Radians(pCoppieOrdinate(i).SecondaryChannelValue))
'        If VmgTmp > VmgUp Then
'          VmgUp = VmgTmp
'          pIdColonnaTgUp = i
'        End If
'        If VmgTmp < VmgDn Then
'          VmgDn = VmgTmp
'          pIdColonnaTgDn = i
'        End If
'      Next
'      'Else
'      '  pCoppieOrdinate = pCoppieValori
'    End If

'    'configura l 'oggetto interpolatore
'    Dim pIndici As New List(Of Double)
'    Dim pValori As New List(Of Double)
'    For Each Coppia In pCoppieOrdinate
'      pIndici.Add(Coppia.SecondaryChannelValue)
'      pValori.Add(Coppia.ValueChannelValue)
'    Next
'    Select Case pSplineType
'      Case eSplineType.eAkima
'        alglib.spline1dbuildakima(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eCatmullRom
'        alglib.spline1dbuildcatmullrom(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eCubic
'        alglib.spline1dbuildcubic(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'            'Case eSplineType.eHermite
'            '    alglib.spline1dbuildhermite(Indici.ToArray, Valori.ToArray, pObjInterpolatore)
'      Case eSplineType.eLinear
'        alglib.spline1dbuildlinear(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eMonotone
'        alglib.spline1dbuildmonotone(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'    End Select

'    'qui dovrebbe fare da 0 a 180 di twa
'    For i As Integer = pCoppieOrdinate.First.SecondaryChannelValue To pCoppieOrdinate.Last.SecondaryChannelValue Step 1
'      Dim ValoreIntepolato As Double = alglib.spline1dcalc(pObjInterpolatore, i)
'      Dim v180 As Double = alglib.spline1dcalc(pObjInterpolatore, 180)
'      Dim Min As Double = 0
'      If i < 90 Then
'        Dim v1tmp As Double = pCoppieOrdinate(1).SecondaryChannelValue
'        If i < v1tmp Then
'          Dim vTmp As Double = pCoppieOrdinate(1).ValueChannelValue
'          Min = i / v1tmp * vTmp
'        End If
'        'pTabellaCompletaValueChannel.Add(i, System.Math.Max(Min, ValoreIntepolato))
'        pTabellaCompletaValueChannel.Add(i, ValoreIntepolato)
'      Else
'        Dim v1tmp As Double = pCoppieOrdinate(pCoppieOrdinate.Count - 2).SecondaryChannelValue
'        If i > v1tmp Then
'          Dim vTmp As Double = pCoppieOrdinate(pCoppieOrdinate.Count - 2).ValueChannelValue
'          Min = System.Math.Max(v180, v180 + ((180 - i) / v1tmp * vTmp))
'        End If
'        pTabellaCompletaValueChannel.Add(i, ValoreIntepolato)
'        'pTabellaCompletaValueChannel.Add(i, System.Math.Max(Min, ValoreIntepolato))
'      End If
'    Next
'  End Sub

'  Public Sub ImpostaIndiciTgtUpDn(TgtUpIndex As Integer, TgtDnIndex As Integer)
'    pIdColonnaTgUp = TgtUpIndex
'    pIdColonnaTgDn = TgtDnIndex
'  End Sub

'  Public ReadOnly Property Valore(SecondaryChannelValue As Double) As Double
'    Get
'      Return pTabellaCompletaValueChannel(SecondaryChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property Valore(SecondaryChannelValue As Integer) As Double
'    Get
'      Return pTabellaCompletaValueChannel(SecondaryChannelValue)
'    End Get
'  End Property

'  Public ReadOnly Property TargetUpValue() As Double
'    Get
'      Return pCoppieOrdinate(IdColonnaTgUp).ValueChannelValue
'    End Get
'  End Property

'  Public ReadOnly Property TargetDnValue() As Double
'    Get
'      Return pCoppieOrdinate(IdColonnaTgDn).ValueChannelValue
'    End Get
'  End Property

'  Public ReadOnly Property TargetUpReference() As Double
'    Get
'      Return pCoppieOrdinate(IdColonnaTgUp).SecondaryChannelValue
'    End Get
'  End Property

'  Public ReadOnly Property TargetDnReference() As Double
'    Get
'      Return pCoppieOrdinate(IdColonnaTgDn).SecondaryChannelValue
'    End Get
'  End Property


'End Class

'Public Class clsPolare2019Coppia
'  Dim pSecondaryChannelValue As Double
'  Dim pValueChannelValue As Double

'  Public Sub New(SecondaryChannelValue As Double, ValueChannelValue As Double)
'    pSecondaryChannelValue = SecondaryChannelValue
'    pValueChannelValue = ValueChannelValue
'  End Sub

'  Public Property SecondaryChannelValue As Double
'    Get
'      Return pSecondaryChannelValue
'    End Get
'    Set(value As Double)
'      pSecondaryChannelValue = value
'    End Set
'  End Property

'  Public Property ValueChannelValue As Double
'    Get
'      Return pValueChannelValue
'    End Get
'    Set(value As Double)
'      pValueChannelValue = value
'    End Set
'  End Property

'End Class




Public Class clsExportToExpedition

  Public Sub CreaFileTgtExpedition()
    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile '   AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
    Dim fi As New System.IO.FileInfo(UltimoPath)
    UltimoPath = fi.Directory.FullName & "\Expedition"
    If Not System.IO.Directory.Exists(UltimoPath) Then
      System.IO.Directory.CreateDirectory(UltimoPath)
    End If
    Dim FileNamePrefix As String = InputBox("ExpeditionTargetName", "Files Name", Today.ToString("yyyyMMdd"))
    'Dim CT As clsChannelTarget = TgtManager.Tgt.Polare("bs")
    Dim Contenuto As String = EsportaPolareExpedition("bs", 1)
    ObjFiles.SalvaNuovoFileSostituendoContenuto(Contenuto, UltimoPath & "\" & FileNamePrefix & "_BsPolar.txt")

  End Sub

  Private Function EsportaPolareExpedition(Campo As String, Decimali As Integer) As String
    Dim StringaToExport As String = ""
    Dim Angoli As Double() = {0, 999, 70, 90, 110, 9999, 180}
    Dim idx As Integer = 0
    'For Each Angolo In Angoli
    '  Select Case Angolo
    '    Case 999, 9999
    '      StringaToExport &= vbTab & "*v" & idx & vbTab & "*a" & idx
    '    Case Else
    '      StringaToExport &= vbTab & "v" & idx & vbTab & "a" & idx
    '  End Select
    '  idx += 1
    'Next
    StringaToExport = "Performance2021 Polar" & vbCrLf
    If Not TgtManager.Tgt Is Nothing Then
      For Each valore In TgtManager.Tgt.ValoriMainChannel
        StringaToExport &= valore
        For Each angolo In Angoli
          Dim Twa As Double = angolo
          Dim Bs As Double = 0
          Select Case angolo
            Case 0
              Bs = 0
            Case 999
              Dim v = TgtManager.Tgt.ValoreTgtUp(valore, Campo)
              Bs = If(v Is Nothing, 0, v.Bs) 'Targets.GetPolare(Campo).ValoreTargetUp(valore)
              Twa = If(v Is Nothing, 0, v.Twa) ' Targets.GetPolare("Twa").ValoreTargetUpReferece(valore)
            Case 9999
              Dim v = TgtManager.Tgt.ValoreTgtDn(valore, Campo)
              Bs = If(v Is Nothing, 0, v.Bs) 'Targets.GetPolare(Campo).ValoreTargetUp(valore)
              Twa = If(v Is Nothing, 0, v.Twa) ' Targets.GetPolare("Twa").ValoreTargetUpReferece(valore)
            Case 180
              Dim v = TgtManager.Tgt.ValoreTgtDn(valore, Campo)
              Bs = If(v Is Nothing, 0, v.Bs / 3) 'Targets.GetPolare(Campo).ValoreTargetUp(valore)
            Case Else
              Dim v = TgtManager.Tgt.Valore(valore, angolo, Campo)
              Bs = If(v Is Nothing, 0, v.Valore) ' Targets.GetPolare(Campo).Valore(valore, angolo)
          End Select
          If Campo = "bs" Then
            Bs = System.Math.Max(0, Bs)
          End If
          StringaToExport &= vbTab & Format(Twa, "F1") & vbTab & Format(Bs, "F" & Decimali.ToString)
        Next
        StringaToExport &= vbCrLf
      Next
    End If

    Return StringaToExport

  End Function


End Class

Public Class clsExportPolarToFaRo


  Public Function StringaDaEsportare(CreaFiles As Boolean) As String
    Dim strTmp As String = ""
    Dim strTgt As String

    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile ' AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
    Dim fi As New System.IO.FileInfo(UltimoPath)
    UltimoPath = fi.Directory.FullName & "\FaroFiles"
    If Not System.IO.Directory.Exists(UltimoPath) Then
      System.IO.Directory.CreateDirectory(UltimoPath)
    End If
    Dim FileNamePrefix As String = InputBox("Prefix Of each FaRo file name", "Files Name Prefix", Today.ToString("yyyyMMdd"))
    For Each Canale In TgtManager.Tgt.ListaCanaliValori
      Select Case Canale
        Case "cwa"
          'Stop
        Case "twa"
          strTgt = EsportaTarget(TgtManager.Tgt, "twa", 1)
          strTmp &= "CwaTgt" & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_CwaTgt.txt")
          End If
          strTgt = vbCrLf & EsportaTarget(TgtManager.Tgt, Canale, 1)
          strTmp &= Canale & "Tgt" & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_" & Canale & "Tgt.txt")
          End If
        Case "botangle"
          strTgt = EsportaTargetSpanner(TgtManager.Tgt) & vbCrLf & vbCrLf
          strTmp &= "SpannerTgt" & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_SpannerTgt.txt")
          End If
          strTgt = EsportaTarget(TgtManager.Tgt, Canale, 1) & vbCrLf & vbCrLf
          strTmp &= Canale & "Tgt" & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_" & Canale & "Tgt.txt")
          End If
        Case "bs"
          strTgt = EsportaPolareDfw(Canale, 1) & vbCrLf & vbCrLf
          strTmp &= Canale & "_" & TgtManager.Tgt.Fi.Name.Replace(TgtManager.Tgt.Fi.Extension, "") & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_BsPolar.txt")
          End If
          strTgt = EsportaTarget(TgtManager.Tgt, Canale, 1) & vbCrLf & vbCrLf
          strTmp &= Canale & "Tgt" & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_BsTgt.txt")
          End If
        Case Else
          strTgt = EsportaTarget(TgtManager.Tgt, Canale, 1) & vbCrLf & vbCrLf
          strTmp &= Canale & "Tgt" & vbCrLf & strTgt & vbCrLf & vbCrLf
          If CreaFiles Then
            ObjFiles.SalvaNuovoFileSostituendoContenuto(strTgt, UltimoPath & "\" & FileNamePrefix & "_" & Canale & "Tgt.txt")
          End If
      End Select
    Next
    Return strTmp
  End Function


  Public Function EsportaPolareDfw(Campo As String, Decimali As Integer) As String
    Dim StringaToExport As String = ""
    Dim Angoli As Double() = {0, 30, 999, 70, 90, 110, 9999, 155, 180}
    Dim idx As Integer = 0
    For Each Angolo In Angoli
      Select Case Angolo
        Case 999, 9999
          StringaToExport &= vbTab & "*v" & idx & vbTab & "*a" & idx
        Case Else
          StringaToExport &= vbTab & "v" & idx & vbTab & "a" & idx
      End Select
      idx += 1
    Next
    StringaToExport &= vbCrLf & vbCrLf
    If Not TgtManager.Tgt Is Nothing Then
      For Each valore In TgtManager.Tgt.ValoriMainChannel
        StringaToExport &= valore
        For Each angolo In Angoli
          Dim Twa As Double = angolo
          Dim Bs As Double = 0
          Select Case angolo
            Case 0
              Bs = 0
            Case 999
              Dim v = TgtManager.Tgt.ValoreTgtUp(valore, Campo)
              Bs = If(v Is Nothing, 0, v.Bs) 'Targets.GetPolare(Campo).ValoreTargetUp(valore)
              Twa = If(v Is Nothing, 0, v.Twa) ' Targets.GetPolare("Twa").ValoreTargetUpReferece(valore)
            Case 9999
              Dim v = TgtManager.Tgt.ValoreTgtDn(valore, Campo)
              Bs = If(v Is Nothing, 0, v.Bs) 'Targets.GetPolare(Campo).ValoreTargetUp(valore)
              Twa = If(v Is Nothing, 0, v.Twa) ' Targets.GetPolare("Twa").ValoreTargetUpReferece(valore)
            Case Else
              Dim v = TgtManager.Tgt.Valore(valore, angolo, Campo)
              Bs = If(v Is Nothing, 0, v.Valore) ' Targets.GetPolare(Campo).Valore(valore, angolo)
          End Select
          If Campo = "bs" Then
            Bs = System.Math.Max(0, Bs)
          End If
          StringaToExport &= vbTab & Format(Bs, "F" & Decimali.ToString) & vbTab & Format(Twa, "F1")
        Next
        StringaToExport &= vbCrLf
      Next
    End If

    Return StringaToExport

  End Function

  Public Function EsportaTarget(Polare As clsTgt, Campo As String, Decimali As Integer) As String
    Dim StringaToExport As String = ""
    Dim Angoli As Double() = {0, 70, 90, 180}
    For Each valore In TgtManager.Tgt.ValoriMainChannel
      StringaToExport &= vbTab & valore
    Next
    StringaToExport &= vbCrLf & vbCrLf
    For Each Angolo In Angoli
      StringaToExport &= Angolo
      For Each valore In TgtManager.Tgt.ValoriMainChannel
        If Angolo >= 90 Then
          StringaToExport &= vbTab & Format(Polare.ValoreTgtDn(valore, Campo).Bs, "F" & Decimali.ToString)
        Else
          StringaToExport &= vbTab & Format(Polare.ValoreTgtUp(valore, Campo).Bs, "F" & Decimali.ToString)
        End If
      Next
      StringaToExport &= vbCrLf
    Next
    Return StringaToExport
  End Function

  Public Function EsportaTargetSpanner(Polare As clsTgt) As String
    'Dim Polare As clsPolare2019CanaleValori = Targets.GetPolare("Travel")
    'Dim Polare2 As clsPolare2019CanaleValori = Targets.GetPolare("BotAngle")
    'If Polare Is Nothing Then Return ""
    Dim StringaToExport As String = ""
    Dim Angoli As Double() = {0, 70, 90, 180}
    For Each valore In TgtManager.Tgt.ValoriMainChannel
      StringaToExport &= vbTab & valore
    Next
    StringaToExport &= vbCrLf & vbCrLf
    For Each Angolo In Angoli
      StringaToExport &= Angolo
      For Each valore In TgtManager.Tgt.ValoriMainChannel
        If Angolo >= 90 Then
          StringaToExport &= vbTab & Format(Polare.ValoreTgtDn(valore, "travel").Valore + Polare.ValoreTgtDn(valore, "botangle").Valore, "F1")
        Else
          StringaToExport &= vbTab & Format(Polare.ValoreTgtUp(valore, "travel").Valore + Polare.ValoreTgtUp(valore, "botangle").Valore, "F1")
        End If
      Next
      StringaToExport &= vbCrLf
    Next
    Return StringaToExport
  End Function

  'Public Function EsportaTarget(Campo As String, Decimali As Integer) As String
  '  Dim Polare As clsPolare2019CanaleValori = Targets.GetPolare(Campo)
  '  If Polare Is Nothing Then Return ""
  '  Dim StringaToExport As String = ""
  '  Dim Angoli As Double() = {0, 70, 90, 180}
  '  For Each valore In Targets.ValoriMainChannel
  '    StringaToExport &= vbTab & valore
  '  Next
  '  StringaToExport &= vbCrLf & vbCrLf
  '  For Each Angolo In Angoli
  '    StringaToExport &= Angolo
  '    For Each valore In Targets.ValoriMainChannel
  '      If Angolo >= 90 Then
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetDn(valore), "F" & Decimali.ToString)
  '      Else
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetUp(valore), "F" & Decimali.ToString)
  '      End If
  '    Next
  '    StringaToExport &= vbCrLf
  '  Next
  '  Return StringaToExport
  'End Function

  'Public Function EsportaTargetCwa() As String
  '  Dim Polare As clsPolare2019CanaleValori = Targets.GetPolare("Twa")
  '  Dim Polare2 As clsPolare2019CanaleValori = Targets.GetPolare("Leeway")
  '  If Polare Is Nothing Then Return ""
  '  Dim StringaToExport As String = ""
  '  Dim Angoli As Double() = {0, 70, 90, 180}
  '  For Each valore In Targets.ValoriMainChannel
  '    StringaToExport &= vbTab & valore
  '  Next
  '  StringaToExport &= vbCrLf & vbCrLf
  '  For Each Angolo In Angoli
  '    StringaToExport &= Angolo
  '    For Each valore In Targets.ValoriMainChannel
  '      If Angolo >= 90 Then
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetDn(valore) + Polare2.ValoreTargetDn(valore), "F1")
  '      Else
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetUp(valore) + Polare2.ValoreTargetUp(valore), "F1")
  '      End If
  '    Next
  '    StringaToExport &= vbCrLf
  '  Next
  '  Return StringaToExport
  'End Function

  'Public Function EsportaTargetSpanner() As String
  '  Dim Polare As clsPolare2019CanaleValori = Targets.GetPolare("Travel")
  '  Dim Polare2 As clsPolare2019CanaleValori = Targets.GetPolare("BotAngle")
  '  If Polare Is Nothing Then Return ""
  '  Dim StringaToExport As String = ""
  '  Dim Angoli As Double() = {0, 70, 90, 180}
  '  For Each valore In Targets.ValoriMainChannel
  '    StringaToExport &= vbTab & valore
  '  Next
  '  StringaToExport &= vbCrLf & vbCrLf
  '  For Each Angolo In Angoli
  '    StringaToExport &= Angolo
  '    For Each valore In Targets.ValoriMainChannel
  '      If Angolo >= 90 Then
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetDn(valore) + Polare2.ValoreTargetDn(valore), "F1")
  '      Else
  '        StringaToExport &= vbTab & Format(Polare.ValoreTargetUp(valore) + Polare2.ValoreTargetUp(valore), "F1")
  '      End If
  '    Next
  '    StringaToExport &= vbCrLf
  '  Next
  '  Return StringaToExport
  'End Function


End Class