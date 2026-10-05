Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports edm
Imports PdfSharp
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports SciChart.Charting.Visuals
Imports SciChart.Core.Extensions
Imports ScottPlot.Colormaps
Imports SharpKml.Dom
Imports SQLitePCL

Public Class clsPdf
  Dim _ObjPdf As New PdfSharp.Pdf.PdfDocument()



  Public Sub New()
    'qui andrá messa l interfaccia per esportare i soli controlli selezionati

  End Sub



  'Public Sub CreaPdfProva()
  '  Dim document As PdfDocument = New PdfDocument
  '  ' Create an empty page
  '  Dim page As PdfPage = document.AddPage
  '  ' Get an XGraphics object for drawing
  '  Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
  '  ' Draw crossing lines
  '  Dim pen As XPen = New XPen(XColor.FromArgb(255, 0, 0))
  '  gfx.DrawLine(pen, New XPoint(0, 0), New XPoint(page.Width.Point, page.Height.Point))
  '  gfx.DrawLine(pen, New XPoint(page.Width.Point, 0), New XPoint(0, page.Height.Point))
  '  ' Draw an ellipse
  '  gfx.DrawEllipse(pen, 3 * page.Width.Point / 10, 3 * page.Height.Point / 10, 2 * page.Width.Point / 5, 2 * page.Height.Point / 5)
  '  ' Create a font
  '  Dim font As XFont = New XFont("Verdana", 20, XFontStyle.Bold)
  '  ' Draw the text
  '  gfx.DrawString("Hello, World!", font, XBrushes.Black, New XRect(0, 0, page.Width.Point, page.Height.Point), XStringFormats.Center)
  '  ' Save the document...
  '  Dim filename As String = "HelloWorld.pdf"
  '  document.Save(filename)
  '  ' ...and start a viewer.
  '  Process.Start(filename)
  'End Sub


  Public Sub StampReportPavarot(Controlli As List(Of SciChart.Charting.Visuals.SciChartSurface), GraficiXY As List(Of SciChart.Charting.Visuals.SciChartSurface), Optional Manovre As IEnumerable(Of clsPeriod2021) = Nothing)
    If Manovre Is Nothing Then Manovre = PeriodsManager.CollectionPavarot
    Dim PlotPerPage As Integer = 8
    Dim document As PdfDocument = New PdfDocument
    ' Create an empty page
    Dim page As PdfPage = document.AddPage
    ' Get an XGraphics object for drawing
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    Dim Hdisp As Double = page.Height
    Dim Wdisp As Double = page.Width
    'Wdisp \= 2

    Dim CurrY As Integer = 0

    Dim sFont As New XFont("Verdana", 20)
    Dim Fcolor As XBrush = XBrushes.Black

    Dim ChartTitle As String = "Manoeuvers"
    Dim Tws As New List(Of Double)
    Dim inizio As DateTime = Nothing
    Dim fine As DateTime = Nothing

    Dim T, G As Double
    Dim Pt As New List(Of String)
    Dim St As New List(Of String)
    For Each Pav In Manovre ' .OrderBy(Function(X) X.TwsGroup(2)).OrderByDescending(Function(x) x.DettagliPavarot.VmgEntryToExitGainLossTotalMeters)
      If Pav.IsChecked Then
        If inizio = Nothing Then
          inizio = Pav.TR.Start
          fine = Pav.TR.Finish
        Else
          If Pav.TR.Start < inizio Then
            inizio = Pav.TR.Start
          End If
          If Pav.TR.Finish > fine Then
            fine = Pav.TR.Finish
          End If
        End If
        Tws.Add(Pav.TwsDetails.AvgVal)

        If Pav.PeriodType = clsPeriod2021.ePeriodType.eTack Then
          T += 1
        Else
          G += 1
        End If
        If Pav.IsStbd Then
          St.Add(IIf(Pav.IsHighlighted, "* ", "") & Pav.TR.Start.ToString("HH:mm:ss") & " tws:" & Pav.TwsDetails.AvgVal.ToString("F1"))
        Else
          Pt.Add(IIf(Pav.IsHighlighted, "* ", "") & Pav.TR.Start.ToString("HH:mm:ss") & " tws:" & Pav.TwsDetails.AvgVal.ToString("F1"))
        End If
      End If
    Next
    If G = 0 Then
      ChartTitle = "Tacks"
    ElseIf T = 0 Then
      ChartTitle = "Gybes"
    End If

    Dim TitoloGrafico As String = ChartTitle & " (Tws:" & Tws.Min.ToString("F0") & "-" & Tws.Max.ToString("F0") & ") "
    Dim Tr As New clsTimeRange(inizio, fine)
    If Tr.Durata.TotalDays < 1 Then
      TitoloGrafico &= " " & Tr.Start.ToString("yyyy MM dd")
    Else
      TitoloGrafico &= ", " & Tr.Start.ToString("yyyy MM dd") & " - " & Tr.Finish.ToString("yyyy MM dd") & ""
    End If
    Dim H As Double = gfx.MeasureString(TitoloGrafico, sFont).Height

    gfx.DrawString(TitoloGrafico, sFont, Fcolor, 0, CurrY + H)

    CurrY += H * 2

    Dim s = Controlli.First
    Dim c = DirectCast(s.DataContext, UserControlPavarotPlotViewModelMathPlots)
    Dim rid = System.Math.Floor(c.SeriesSource.Count / 8)
    Dim fSize As Integer = System.Math.Max(8, 16 - rid)
    sFont = New XFont("Verdana", fSize)

    Dim Etichetta As String = ""
    Dim gBy As String = ""
    Dim PeriodiSingoli As Boolean = False
    Dim Contatore As Integer = 0
    For Each ss In c.SeriesSource
      Dim lineaFS = DirectCast(ss.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.FastLineRenderableSeries)
      If IsNumeric(lineaFS.Tag) Then
        Select Case lineaFS.Tag
          Case -1
            Etichetta = "Port Entry (#" & Pt.Count & ")"
            H = gfx.MeasureString(Etichetta, sFont).Height
            Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
            Dim pn = New XPen(XColors.Red, 2)
            gfx.DrawRectangle(pn, R)
            gfx.DrawString(Etichetta, sFont, Fcolor, R.Left + 10, CurrY + H)
          Case 1
            Etichetta = "Stbd Entry (#" & St.Count & ")"
            H = gfx.MeasureString(Etichetta, sFont).Height
            Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
            Dim pn = New XPen(XColors.Green, 2)
            gfx.DrawRectangle(pn, R)
            gfx.DrawString(Etichetta, sFont, Fcolor, R.Left + 10, CurrY + H)
          Case Else
            Etichetta = lineaFS.Tag
            H = gfx.MeasureString(Etichetta, sFont).Height
            Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
            Dim pn = New XPen(XColors.Green, 2)
            gfx.DrawRectangle(pn, R)
            gfx.DrawString(Etichetta, sFont, Fcolor, R.Left + 10, CurrY + H)

        End Select
        gBy = "GrByEntry"
        CurrY += H * 1.5
      ElseIf TypeOf (lineaFS.Tag) Is clsPeriod2021 Then
        Dim Pav = DirectCast(lineaFS.Tag, clsPeriod2021)
        Pav.IsHighlighted = lineaFS.IsSelected
        PeriodiSingoli = True
      Else
        Dim grp As clsGruppoPeriodi = DirectCast(lineaFS.Tag, clsGruppoPeriodi)
        Dim testo As String = grp.Chiave ' "sukka" ' grp.Chiave
        H = gfx.MeasureString(testo, sFont).Height
        Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
        Dim clr As Color = ColoriDifferenziati(grp.IdGruppo) '  Contatore)
        Dim pn = New XPen(XColor.FromArgb(clr.R, clr.G, clr.B), 2)
        gfx.DrawRectangle(pn, R)
        gfx.DrawString(testo, sFont, Fcolor, R.Left + 5, CurrY + H)
        CurrY += H * 1.5
        If (CurrY + H) > Hdisp Then
          CurrY = 0
          page = document.AddPage
          gfx = XGraphics.FromPdfPage(page)
        End If
        'Etichetta = lineaFS.Tag
        'H = gfx.MeasureString(Etichetta, sFont).Height
        'Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
        'Dim clr As Color = ColoriDifferenziati(Contatore)
        'Dim pn = New XPen(XColor.FromArgb(clr.R, clr.G, clr.B), 2)
        'gfx.DrawRectangle(pn, R)
        'gfx.DrawString(Etichetta, sFont, Fcolor, R.Left + 10, CurrY + H)
        Contatore += 1
      End If
      If (CurrY + H) > Hdisp Then
        CurrY = 0
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
      End If
    Next

    If PeriodiSingoli Then
      sFont = New XFont("Verdana", 10)
      For Each Pav In Manovre.OrderByDescending(Function(X) X.PavarotDetails.VmgTgtLossMt).OrderBy(Function(x) x.TwsGroup(2))
        If Pav.IsChecked Then
          Dim testo As String = If(Pav.IsHighlighted, "* ", "") & Pav.Descrizione & " " & Pav.Keys
          H = gfx.MeasureString(testo, sFont).Height
          Dim R As New XRect(0, CurrY, Wdisp, H * 1.2)
          Dim pn = New XPen(XColor.FromArgb(Pav.Colore.R, Pav.Colore.G, Pav.Colore.B), 2)
          gfx.DrawRectangle(pn, R)
          gfx.DrawString(testo, sFont, Fcolor, R.Left + 5, CurrY + H)

          Dim b As XBrush = XBrushes.Red
          If Pav.IsStbd Then
            pn = New XPen(XColors.Green, 1)
            b = XBrushes.Green
          Else
            pn = New XPen(XColors.Red, 1)
          End If
          gfx.DrawEllipse(pn, b, R.X, R.Y + 1, 5, 5)

          CurrY += H * 1.5
          If (CurrY + H) > Hdisp Then
            CurrY = 0
            page = document.AddPage
            gfx = XGraphics.FromPdfPage(page)
          End If
        End If
      Next
    End If

    CurrY += H


    Dim gXrow As Integer = 3
    Dim hSpace As Integer = 5
    Dim gW As Double = (Wdisp / gXrow) - hSpace
    Dim currX As Integer = 0
    Dim Himg As Double = (Hdisp / 4)
    For Each SCsurface In GraficiXY
      Dim ImgTmp = SCsurface.ExportToStream(SciChart.Core.ExportType.Bmp, False)
      Dim rightTmp = currX + gW
      If rightTmp > Wdisp Then
        currX = 0
        CurrY += Himg
      End If
      Dim bottomTmp = CurrY + Himg
      If bottomTmp > Hdisp Then
        CurrY = 0
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
      End If
      Dim R As New XRect(currX, CurrY, gW, Himg)
      Dim pdfimg = XImage.FromStream(ImgTmp)
      gfx.DrawImage(pdfimg, R)
      currX += gW
    Next
    CurrY += Himg + H

    Himg = Hdisp / PlotPerPage
    For Each SCsurface In Controlli
      Dim ImgTmp = SCsurface.ExportToStream(SciChart.Core.ExportType.Bmp, False)
      Dim bottomTmp = CurrY + Himg

      If bottomTmp > Hdisp Then
        CurrY = 0
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
      End If
      Dim R As New XRect(0, CurrY, Wdisp, Himg)
      Dim pdfimg = XImage.FromStream(ImgTmp)
      gfx.DrawImage(pdfimg, R)
      CurrY += Himg
    Next

    If Not gBy = "" Then
      sFont = New XFont("Verdana", 8)
      Dim x As Integer = 5
      For Each Stringa In Pt
        If Stringa Is Pt.First Then
          Stringa = "Port Entry Ref: " & Stringa
        End If
        If Not Stringa Is Pt.Last Then
          Stringa &= ", "
        End If
        Dim Htmp = gfx.MeasureString(Stringa, sFont).Height
        Dim Wtmp = gfx.MeasureString(Stringa, sFont).Width
        If x + Wtmp * 1.1 > Wdisp Then
          x = 30
          CurrY += H * 0.6
          If CurrY + H * 0.6 > Hdisp Then
            CurrY = 0
            page = document.AddPage
            gfx = XGraphics.FromPdfPage(page)
          End If
        End If
        gfx.DrawString(Stringa, sFont, Fcolor, x, CurrY + H)
        x += Wtmp
      Next

      x = 5
      CurrY += H * 1.3
      If CurrY + H * 2 > Hdisp Then
        CurrY = 0
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
      End If

      For Each Stringa In St
        If Stringa Is St.First Then
          Stringa = "Stbd Entry Ref: " & Stringa
        End If
        If Not Stringa Is St.Last Then
          Stringa &= ", "
        End If
        Dim Htmp = gfx.MeasureString(Stringa, sFont).Height
        Dim Wtmp = gfx.MeasureString(Stringa, sFont).Width
        If x + Wtmp * 1.1 > Wdisp Then
          x = 30
          CurrY += H * 0.6
          If CurrY + H * 0.6 > Hdisp Then
            CurrY = 0
            page = document.AddPage
            gfx = XGraphics.FromPdfPage(page)
          End If
        End If
        gfx.DrawString(Stringa, sFont, Fcolor, x, CurrY + H)
        x += Wtmp
      Next

    End If

    'For Each Pav In PeriodsManager.ListaPavarot
    '  Pav.IsHighlighted = False
    'Next


    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = Tr.Start.ToString("yyyyMMdd")
    If Tr.Durata.TotalDays >= 1 Then
      pstringadata = Tr.Start.ToString("yyyyMMdd") & "_" & Tr.Finish.ToString("yyyyMMdd")
    End If
    'ApriExplorer(path, path & "\" & pstringadata & "_PavarotPlots.xlsx")
    Dim filename As String = VerificaFilePath(path & "\" & pstringadata & "_" & ChartTitle & gBy & "_Tws" & Tws.Min.ToString("F0") & "t" & Tws.Max.ToString("F0") & ".pdf")

    document.Save(filename)
    ' ...and start a viewer.
    Process.Start(filename)

  End Sub

  Private Function VerificaFilePath(PathFile As String) As String
    Dim PathTmp As String = PathFile
    For i As Integer = 1 To 100
      If System.IO.File.Exists(PathTmp) Then
        PathTmp = PathFile.Replace(".pdf", "_" & i & ".pdf")
      Else
        Exit For
      End If
    Next
    Return PathTmp
  End Function



  ''' <summary>Report completo delle straight line: l'impaginazione e' in clsStraightLineReport (GestioneReportStraightLine.vb).</summary>
  Public Sub StampaReportStraightLine(ControlliAvg As List(Of SciChart.Charting.Visuals.SciChartSurface), ControlliDistr As List(Of SciChart.Charting.Visuals.SciChartSurface), OutputType As clsStraightLineVM2020.eOutputType, ListaPeriodi As List(Of clsPeriod2021), ListaCanali As List(Of clsStraightLineChartSettings), Filtro As String, Opzioni As clsStraightLineReportOptions, Optional Av As FinestraAvanzamento = Nothing)
    Dim Report As New clsStraightLineReport
    Report.CreaReport(ControlliAvg, ControlliDistr, OutputType, ListaPeriodi, ListaCanali, Filtro, Opzioni, Av)
  End Sub


  Public Sub StampaMeteoReport(Righe As List(Of String))

    Dim document As PdfDocument = New PdfDocument
    Dim page As PdfPage = document.AddPage
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    Dim Hdisp As Double = page.Height
    Dim Wdisp As Double = page.Width

    Hdisp *= 0.9
    Wdisp *= 0.9

    Dim Xspace As Integer = 2
    Dim Yspace As Integer = 2
    Dim CurrY As Integer = 30
    Dim CurrX As Integer = Wdisp * 0.05

    Dim tFont As New XFont("Verdana", 14, XFontStyle.Bold)
    Dim hFont As New XFont("Verdana", 11, XFontStyle.Bold)
    Dim sFont As New XFont("Verdana", 11)


    Dim Fcolor As XBrush = XBrushes.Black

    Dim ChartTitle As String = Righe.First.TrimEnd
    Dim s As XSize

    s = gfx.MeasureString(ChartTitle, sFont)
    Dim H As Double = s.Height
    gfx.DrawString(ChartTitle, tFont, Fcolor, (Wdisp - s.Width) / 2, CurrY)

    CurrY += H + 20


    'Dim Tws As New List(Of Double)
    'Dim inizio As DateTime = Nothing
    'Dim fine As DateTime = Nothing
    Dim Colonne = Righe(1).Split(vbTab)
    Dim W As Double = (Wdisp - (Xspace * Colonne.Count + 1)) / Colonne.Count
    For Each colonna In Colonne
      s = DrawStringCenter(colonna, gfx, hFont, Fcolor, CurrX, CurrY, W, XColors.LightGray, -1)
      CurrX += (W + Xspace)
    Next
    CurrX = Wdisp * 0.05
    CurrY += s.Height + (Yspace * 2)



    For Riga As Integer = 2 To Righe.Count - 1
      Colonne = Righe(Riga).Split(vbTab)
      For Each colonna In Colonne
        If Riga / 2 = CInt(Riga / 2) Then
          s = DrawStringRight(colonna & " ", gfx, sFont, Fcolor, CurrX, CurrY, W, XColors.AntiqueWhite, -1)
        Else
          s = DrawStringRight(colonna & " ", gfx, sFont, Fcolor, CurrX, CurrY, W, XColors.LightSteelBlue, -1)
        End If
        CurrX += (W + Xspace)
      Next
      CurrX = Wdisp * 0.05
      CurrY += s.Height + Yspace
      If CurrY > Hdisp Then
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)

        CurrY = 50

        Colonne = Righe(1).Split(vbTab)
        W = (Wdisp - (Xspace * Colonne.Count + 1)) / Colonne.Count
        For Each colonna In Colonne
          s = DrawStringCenter(colonna, gfx, hFont, Fcolor, CurrX, CurrY, W, XColors.LightGray, -1)
          CurrX += (W + Xspace)
        Next
        CurrX = Wdisp * 0.05
        CurrY += s.Height + (Yspace * 2)

      End If
    Next


    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = Righe.First.Trim.Replace(" ", "")
    'ApriExplorer(path, path & "\" & pstringadata & "_PavarotPlots.xlsx")
    Dim filename As String = VerificaFilePath(path & "\" & pstringadata & ".pdf")
    document.Save(filename)
    ' ...and start a viewer.
    Process.Start(filename)


  End Sub


  'Public Function StampaReportXY(ImageStreams As List(Of System.IO.Stream), RowsPerPage As Integer, ColsPerPage As Integer, Intestazione As String) As Boolean


  '  Dim path As String = DataProvider2020.Files.First.Directory.FullName
  '  Dim pstringadata As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd") & "_" & Intestazione
  '  'ApriExplorer(path, path & "\" & pstringadata & "_PavarotPlots.xlsx")
  '  Dim Input As New UserControl_InputBox(pstringadata, "Pdf File Name")
  '  Input.ShowDialog()

  '  If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then

  '    Dim NomePdf As String = Input.Testo
  '    Dim vs As Integer = 5
  '    Dim hs As Integer = 3


  '    Dim document As PdfDocument = New PdfDocument
  '    ' Create an empty page
  '    Dim page As PdfPage = document.AddPage
  '    ' Get an XGraphics object for drawing
  '    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

  '    Dim Hdisp As Double = page.Height
  '    Dim Wdisp As Double = (page.Width.Value - (hs * (ColsPerPage - 1))) / ColsPerPage

  '    Dim CurrY As Integer = 0

  '    Dim sFont As New XFont("Verdana", 16)
  '    Dim Fcolor As XBrush = XBrushes.Black

  '    'Dim Intestazione As String = DataProvider2020.TimeRange.Start.ToString("dd MMM yyyy")

  '    Dim H As Double = gfx.MeasureString(NomePdf, sFont).Height * 2
  '    Dim W As Double = gfx.MeasureString(NomePdf, sFont).Width
  '    gfx.DrawString(NomePdf, sFont, Fcolor, (page.Width.Value / 2) - (W / 2), CurrY + (H * 0.7))

  '    Hdisp -= H * 1.5
  '    Hdisp -= (vs * RowsPerPage)
  '    'Hdisp *= 0.95

  '    CurrY += H '* 2

  '    Dim c As Integer = 0
  '    Dim rw As Integer = 0

  '    For Each Immagine In ImageStreams
  '      Dim Himg As Double = ((Hdisp - (RowsPerPage * vs)) / RowsPerPage)
  '      CurrY = ((Himg + vs) * rw) + H + vs
  '      Dim bottomTmp = CurrY + Himg
  '      If bottomTmp > page.Height Then
  '        rw = 0
  '        CurrY = H + vs
  '        page = document.AddPage
  '        gfx = XGraphics.FromPdfPage(page)
  '        gfx.DrawString(NomePdf, sFont, Fcolor, (page.Width.Value / 2) - (W / 2), (H * 0.7))
  '      End If
  '      Dim x = Wdisp * c
  '      Dim R As New XRect(x, CurrY, Wdisp - hs, Himg)
  '      Dim pdfimg = XImage.FromStream(Immagine)
  '      gfx.DrawImage(pdfimg, R)
  '      c += 1
  '      If c >= ColsPerPage Then
  '        c = 0
  '        rw += 1
  '      End If
  '    Next

  '    'crea la tabella csv completa
  '    XYplotDataTables.SetCsvTable()
  '    'crea il file di testo con tutti i dati completi

  '    Dim lastxrow As String = ""
  '    Dim lastyok As String = ""
  '    page = document.AddPage
  '    gfx = XGraphics.FromPdfPage(page)
  '    W = gfx.MeasureString(NomePdf & " Tables", sFont).Width
  '    hs = 10
  '    Dim totalw As Double = page.Width.Value - (2 * hs)
  '    Dim col1n2w As Double = totalw * 0.25
  '    Dim colsw As Double = totalw - col1n2w
  '    gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))
  '    CurrY = H * 1.3 + vs
  '    Dim hh As Double = 0
  '    Dim fc As New XStringFormat()
  '    'fc.Alignment = AlignmentX.Center
  '    'fc.Alignment = AlignmentY.Center
  '    fc.Alignment = XStringAlignment.Center
  '    fc.LineAlignment = XLineAlignment.Center
  '    Dim fl As New XStringFormat()
  '    'fl.Alignment = AlignmentX.Left
  '    'fl.Alignment = AlignmentY.Center
  '    fl.Alignment = XStringAlignment.Near
  '    fl.LineAlignment = XLineAlignment.Center
  '    Dim fr As New XStringFormat()
  '    'fr.Alignment = AlignmentX.Right
  '    'fr.Alignment = AlignmentY.Center
  '    fr.Alignment = XStringAlignment.Far
  '    fr.LineAlignment = XStringAlignment.Far

  '    Dim LastYchName As String = ""


  '    For Each row In XYplotDataTables.CsvTable.Split(vbCrLf).ToList
  '      Dim col = row.Trim(vbLf).Split(vbTab).ToList
  '      If col.Count < 3 Then
  '        'rw = 0
  '        'CurrY = H * 1.3 + vs
  '        'page = document.AddPage
  '        'gfx = XGraphics.FromPdfPage(page)
  '        'gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))

  '        rw = 0
  '        CurrY = H * 1.3 + vs
  '        page = document.AddPage
  '        gfx = XGraphics.FromPdfPage(page)
  '        gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))
  '        ' Reset intestazioni: il plot successivo le riscriverà correttamente
  '        lastxrow = ""
  '        lastyok = ""
  '        LastYchName = ""

  '      ElseIf col(0).StartsWith("-----") Then
  '        'riga che porta i valori del canale asse X
  '        lastxrow = row
  '        Dim F As New XFont("Verdana", 11, XFontStyle.Bold)
  '        hh = gfx.MeasureString("A", F).Height * 1.2
  '        Dim cellsize = colsw / (col.Count - 2)

  '        Dim R As New XRect(hs, CurrY - hh, totalw, hh)
  '        gfx.DrawRectangle(XBrushes.LightGray, R)

  '        R = New XRect(hs, CurrY - hh, col1n2w + cellsize, hh)
  '        gfx.DrawString(col(2), F, Fcolor, R, fr)
  '        For cc As Integer = 3 To col.Count - 1
  '          R = New XRect(col1n2w + hs + cellsize * (cc - 2), CurrY - hh, cellsize, hh)
  '          gfx.DrawString(col(cc), F, Fcolor, R, fr)
  '        Next
  '        CurrY += hh
  '      ElseIf col(0).ToString.Trim = "" AndAlso col(1).ToString.Trim = "" Then
  '        'righe Max,Min,SD,Samples
  '        Dim F As New XFont("Verdana", 7, XFontStyle.Regular)
  '        hh = gfx.MeasureString("A", F).Height * 1.2
  '        Dim cellsize = colsw / (col.Count - 2)

  '        Dim R As New XRect(hs + col1n2w, CurrY - hh, colsw, hh)
  '        gfx.DrawRectangle(RowColorByType(col(2)), R)

  '        For cc As Integer = 0 To 1
  '          R = New XRect(hs + col1n2w / 2 * cc, CurrY - hh, cellsize, hh)
  '          gfx.DrawString(col(cc), F, Fcolor, R, fr)
  '        Next
  '        For cc As Integer = 2 To col.Count - 1
  '          R = New XRect(hs + col1n2w + (cc - 2) * cellsize, CurrY - hh, cellsize, hh)
  '          gfx.DrawString(col(cc), F, Fcolor, R, fr)
  '        Next
  '        CurrY += hh
  '        If col(2).ToString.ToLower.StartsWith("sampl") Then
  '          CurrY += hh * 0.5
  '        End If
  '      Else

  '        'righe intestazione canale y avg o tgt
  '        Dim F As New XFont("Verdana", 8, XFontStyle.Bold)
  '        hh = gfx.MeasureString("A", F).Height * 1.2
  '        Dim cellsize = colsw / (col.Count - 2)
  '        Dim R As XRect
  '        Dim bottomTmp = CurrY + hh * 8
  '        Dim c0txt = col(0)
  '        Dim np As Boolean = False
  '        If bottomTmp > page.Height Then
  '          'riscrive la riga delle intestazione del canale x
  '          rw = 0
  '          CurrY = H * 1.3 + vs
  '          page = document.AddPage
  '          gfx = XGraphics.FromPdfPage(page)
  '          gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))

  '          Dim coltmp = lastxrow.Split(vbTab).ToList
  '          F = New XFont("Verdana", 11, XFontStyle.Bold)
  '          hh = gfx.MeasureString("A", F).Height * 1.2

  '          R = New XRect(hs, CurrY - hh, totalw, hh)
  '          gfx.DrawRectangle(XBrushes.LightGray, R)

  '          R = New XRect(hs, CurrY - hh, col1n2w, hh)
  '          gfx.DrawString(coltmp(2), F, Fcolor, R, fr)
  '          For cc As Integer = 3 To coltmp.Count - 1
  '            R = New XRect(col1n2w + hs + cellsize * (cc - 2), CurrY - hh, cellsize, hh)
  '            gfx.DrawString(coltmp(cc), F, Fcolor, R, fc)
  '          Next
  '          CurrY += hh
  '          F = New XFont("Verdana", 8, XFontStyle.Bold)
  '          hh = gfx.MeasureString("A", F).Height * 1.2
  '          c0txt = lastyok
  '          np = True
  '        End If


  '        If Not col(0) = "" Then
  '          lastyok = c0txt
  '        End If

  '        R = New XRect(hs + col1n2w, CurrY - hh, colsw, hh)
  '        gfx.DrawRectangle(RowColorByType(col(2)), R)

  '        'fl.Alignment = XStringAlignment.Near
  '        R = New XRect(hs, CurrY - hh, col1n2w / 2, hh)
  '        gfx.DrawString(c0txt, F, Fcolor, R, fl)

  '        If np Then
  '          col(1) = LastYchName
  '          np = False
  '        End If

  '        R = New XRect(hs + col1n2w / 2, CurrY - hh, col1n2w / 2, hh)
  '        gfx.DrawString(col(1), F, Fcolor, R, fr)

  '        If Not col(1).Trim = "" Then LastYchName = col(1)


  '        'For cc As Integer = 0 To 1
  '        '  R = New XRect(hs + col1n2w / 2 * cc, CurrY - hh, col1n2w / 2, hh)
  '        '  gfx.DrawString(col(cc), F, Fcolor, R, fl)
  '        'Next
  '        For cc As Integer = 2 To col.Count - 1
  '          R = New XRect(hs + col1n2w + (cc - 2) * cellsize, CurrY - hh, cellsize, hh)
  '          gfx.DrawString(col(cc), F, Fcolor, R, fr)
  '        Next
  '        CurrY += hh * 0.9
  '      End If


  '    Next

  '    'qui devo aggiungere le tabelle in calce ai grafici


  '    Dim filename As String = VerificaFilePath(path & "\" & NomePdf & ".pdf")
  '    document.Save(filename)
  '    Process.Start(filename)


  '    ObjFiles.SalvaNuovoFileSostituendoContenuto(XYplotDataTables.CsvTable, path & "\" & NomePdf & ".csv")

  '    Return True
  '  End If

  '  Return False
  'End Function

  Public Function StampaReportXY(ImageStreams As List(Of System.IO.Stream), RowsPerPage As Integer, ColsPerPage As Integer, Intestazione As String) As Boolean

    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd") & "_" & Intestazione
    Dim Input As New UserControl_InputBox(pstringadata, "Pdf File Name")
    Input.ShowDialog()

    If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then

      Dim NomePdf As String = Input.Testo
      Dim vs As Integer = 5
      Dim hs As Integer = 3

      Dim document As PdfDocument = New PdfDocument
      Dim page As PdfPage = document.AddPage
      Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

      Dim Hdisp As Double = page.Height
      Dim Wdisp As Double = (page.Width.Value - (hs * (ColsPerPage - 1))) / ColsPerPage

      Dim CurrY As Integer = 0

      Dim sFont As New XFont("Verdana", 16)
      Dim Fcolor As XBrush = XBrushes.Black

      Dim H As Double = gfx.MeasureString(NomePdf, sFont).Height * 2
      Dim W As Double = gfx.MeasureString(NomePdf, sFont).Width
      gfx.DrawString(NomePdf, sFont, Fcolor, (page.Width.Value / 2) - (W / 2), CurrY + (H * 0.7))

      Hdisp -= H * 1.5
      Hdisp -= (vs * RowsPerPage)

      CurrY += H

      Dim c As Integer = 0
      Dim rw As Integer = 0

      For Each Immagine In ImageStreams
        Dim Himg As Double = ((Hdisp - (RowsPerPage * vs)) / RowsPerPage)
        CurrY = ((Himg + vs) * rw) + H + vs
        Dim bottomTmp = CurrY + Himg
        If bottomTmp > page.Height Then
          rw = 0
          CurrY = H + vs
          page = document.AddPage
          gfx = XGraphics.FromPdfPage(page)
          gfx.DrawString(NomePdf, sFont, Fcolor, (page.Width.Value / 2) - (W / 2), (H * 0.7))
        End If
        Dim x = Wdisp * c
        Dim R As New XRect(x, CurrY, Wdisp - hs, Himg)
        Dim pdfimg = XImage.FromStream(Immagine)
        gfx.DrawImage(pdfimg, R)
        c += 1
        If c >= ColsPerPage Then
          c = 0
          rw += 1
        End If
      Next

      ' Crea la tabella csv completa
      XYplotDataTables.SetCsvTable()

      ' Variabili di stato per la sezione tabelle
      Dim lastxrow As String = ""     ' ultima riga "-----" (intestazione X) del plot corrente
      Dim LastYchName As String = ""  ' ultimo nome canale Y (col 1) stampato nel plot corrente

      page = document.AddPage
      gfx = XGraphics.FromPdfPage(page)
      W = gfx.MeasureString(NomePdf & " Tables", sFont).Width
      hs = 10
      Dim totalw As Double = page.Width.Value - (2 * hs)
      Dim col1n2w As Double = totalw * 0.25
      Dim colsw As Double = totalw - col1n2w
      gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))
      CurrY = H * 1.3 + vs
      Dim hh As Double = 0

      Dim fc As New XStringFormat()
      fc.Alignment = XStringAlignment.Center
      fc.LineAlignment = XLineAlignment.Center

      Dim fl As New XStringFormat()
      fl.Alignment = XStringAlignment.Near
      fl.LineAlignment = XLineAlignment.Center

      Dim fr As New XStringFormat()
      fr.Alignment = XStringAlignment.Far
      fr.LineAlignment = XStringAlignment.Far

      For Each row In XYplotDataTables.CsvTable.Split(vbCrLf).ToList
        Dim col = row.Trim(vbLf).Split(vbTab).ToList

        If col.Count < 3 Then
          ' -------------------------------------------------------------------
          ' Separatore tra un plot e il successivo: nuova pagina + reset stato
          ' -------------------------------------------------------------------
          page = document.AddPage
          gfx = XGraphics.FromPdfPage(page)
          gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))
          CurrY = H * 1.3 + vs
          rw = 0
          lastxrow = ""
          LastYchName = ""

        ElseIf col(0).StartsWith("-----") Then
          ' -------------------------------------------------------------------
          ' Riga intestazione asse X (es: "-----[tab]Tws[tab]2[tab]4...")
          ' -------------------------------------------------------------------
          lastxrow = row
          Dim F As New XFont("Verdana", 11, XFontStyle.Bold)
          hh = gfx.MeasureString("A", F).Height * 1.2
          Dim cellsize = colsw / (col.Count - 2)

          Dim R As New XRect(hs, CurrY - hh, totalw, hh)
          gfx.DrawRectangle(XBrushes.LightGray, R)

          R = New XRect(hs, CurrY - hh, col1n2w + cellsize, hh)
          gfx.DrawString(col(2), F, Fcolor, R, fr)
          For cc As Integer = 3 To col.Count - 1
            R = New XRect(col1n2w + hs + cellsize * (cc - 2), CurrY - hh, cellsize, hh)
            gfx.DrawString(col(cc), F, Fcolor, R, fr)
          Next
          CurrY += hh

        ElseIf col(0).ToString.Trim = "" AndAlso col(1).ToString.Trim = "" Then
          ' -------------------------------------------------------------------
          ' Righe Max / Min / SD / Samples
          ' -------------------------------------------------------------------
          Dim F As New XFont("Verdana", 7, XFontStyle.Regular)
          hh = gfx.MeasureString("A", F).Height * 1.2
          Dim cellsize = colsw / (col.Count - 2)

          Dim R As New XRect(hs + col1n2w, CurrY - hh, colsw, hh)
          gfx.DrawRectangle(RowColorByType(col(2)), R)

          For cc As Integer = 0 To 1
            R = New XRect(hs + col1n2w / 2 * cc, CurrY - hh, cellsize, hh)
            gfx.DrawString(col(cc), F, Fcolor, R, fr)
          Next
          For cc As Integer = 2 To col.Count - 1
            R = New XRect(hs + col1n2w + (cc - 2) * cellsize, CurrY - hh, cellsize, hh)
            gfx.DrawString(col(cc), F, Fcolor, R, fr)
          Next
          CurrY += hh
          If col(2).ToString.ToLower.StartsWith("sampl") Then
            CurrY += hh * 0.5
          End If

        Else
          ' -------------------------------------------------------------------
          ' Righe intestazione canale Y (Avg / Tgt) con eventuale cambio pagina
          ' -------------------------------------------------------------------
          Dim F As New XFont("Verdana", 8, XFontStyle.Bold)
          hh = gfx.MeasureString("A", F).Height * 1.2
          Dim cellsize = colsw / (col.Count - 2)
          Dim bottomTmp = CurrY + hh * 8

          ' Salva i valori della riga corrente PRIMA di qualsiasi modifica
          Dim c0txt As String = col(0)
          Dim c1txt As String = col(1)

          If bottomTmp > page.Height Then
            ' -----------------------------------------------------------------
            ' Cambio pagina per overflow
            ' Usa col(0) e col(1) della riga CORRENTE — sono gia' i valori giusti
            ' -----------------------------------------------------------------
            page = document.AddPage
            gfx = XGraphics.FromPdfPage(page)
            gfx.DrawString(NomePdf & " Tables", sFont, Fcolor, (totalw / 2) - (W / 2), (H * 0.7))
            CurrY = H * 1.3 + vs
            rw = 0

            ' Ridisegna intestazione X del plot corrente
            If lastxrow <> "" Then
              Dim coltmp = lastxrow.Split(vbTab).ToList
              F = New XFont("Verdana", 11, XFontStyle.Bold)
              hh = gfx.MeasureString("A", F).Height * 1.2

              Dim Rx As New XRect(hs, CurrY - hh, totalw, hh)
              gfx.DrawRectangle(XBrushes.LightGray, Rx)

              Rx = New XRect(hs, CurrY - hh, col1n2w, hh)
              gfx.DrawString(coltmp(2), F, Fcolor, Rx, fr)
              For cc As Integer = 3 To coltmp.Count - 1
                Rx = New XRect(col1n2w + hs + cellsize * (cc - 2), CurrY - hh, cellsize, hh)
                gfx.DrawString(coltmp(cc), F, Fcolor, Rx, fc)
              Next
              CurrY += hh
            End If

            F = New XFont("Verdana", 8, XFontStyle.Bold)
            hh = gfx.MeasureString("A", F).Height * 1.2

            ' Se col(0) e' vuoto (riga Port/Stbd successiva stesso canale),
            ' usa LastYchName per la col 1; altrimenti col(1) e' gia' corretto
            If c1txt.Trim = "" Then c1txt = LastYchName
          End If

          ' Aggiorna LastYchName con il nome canale Y effettivamente stampato
          If Not c1txt.Trim = "" Then LastYchName = c1txt

          Dim R2 As New XRect(hs + col1n2w, CurrY - hh, colsw, hh)
          gfx.DrawRectangle(RowColorByType(col(2)), R2)

          Dim R3 As New XRect(hs, CurrY - hh, col1n2w / 2, hh)
          gfx.DrawString(c0txt, F, Fcolor, R3, fl)

          Dim R4 As New XRect(hs + col1n2w / 2, CurrY - hh, col1n2w / 2, hh)
          gfx.DrawString(c1txt, F, Fcolor, R4, fr)

          For cc As Integer = 2 To col.Count - 1
            Dim R5 As New XRect(hs + col1n2w + (cc - 2) * cellsize, CurrY - hh, cellsize, hh)
            gfx.DrawString(col(cc), F, Fcolor, R5, fr)
          Next
          CurrY += hh * 0.9
        End If

      Next

      Dim filename As String = VerificaFilePath(path & "\" & NomePdf & ".pdf")
      document.Save(filename)
      Process.Start(filename)

      ObjFiles.SalvaNuovoFileSostituendoContenuto(XYplotDataTables.CsvTable, path & "\" & NomePdf & ".csv")

      Return True
    End If

    Return False
  End Function

  Function RowColorByType(Type As String) As XBrush
    Select Case Type.ToLower
      Case "tgtup"
        Return XBrushes.LightSkyBlue
      Case "tgtdn"
        Return XBrushes.LightSteelBlue
      Case "avg"
        Return XBrushes.Yellow
      Case "max"
        Return XBrushes.LightGoldenrodYellow
      Case "min"
        Return XBrushes.LightYellow
      Case "sd"
        Return XBrushes.LightGoldenrodYellow
      Case "samples"
        Return XBrushes.LightYellow
      Case Else
        Return XBrushes.Orange
    End Select
  End Function

  Public Sub StampaReportTwsTwa(ImageStreams As List(Of System.IO.Stream))
    Dim PlotPerPage As Integer = 3
    Dim document As PdfDocument = New PdfDocument
    ' Create an empty page
    Dim page As PdfPage = document.AddPage
    ' Get an XGraphics object for drawing
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    Dim Hdisp As Double = page.Height
    Dim Wdisp As Double = page.Width
    'Wdisp \= 2

    Dim CurrY As Integer = 0

    Dim sFont As New XFont("Verdana", 20)
    Dim Fcolor As XBrush = XBrushes.Black

    Dim Intestazione As String = DataProvider2020.TimeRange.Start.ToString("dd MMM yyyy")

    Dim H As Double = gfx.MeasureString(Intestazione, sFont).Height
    gfx.DrawString(Intestazione, sFont, Fcolor, 0, CurrY + H)

    CurrY += H * 2

    For Each Immagine In ImageStreams
      Dim Himg As Double = (Hdisp / PlotPerPage)
      Dim bottomTmp = CurrY + Himg

      If bottomTmp > Hdisp Then
        CurrY = 0
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
      End If
      Dim R As New XRect(0, CurrY, Wdisp, Himg)
      Dim pdfimg = XImage.FromStream(Immagine)
      gfx.DrawImage(pdfimg, R)
      CurrY += Himg
    Next


    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd")
    'ApriExplorer(path, path & "\" & pstringadata & "_PavarotPlots.xlsx")

    Dim Input As New UserControl_InputBox(pstringadata & "_TwsTwa", "Pdf File Name")
    Input.ShowDialog()

    If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then
      Dim Nome As String = Input.Testo
      Dim filename As String = VerificaFilePath(path & "\" & Nome & ".pdf")
      document.Save(filename)
      Process.Start(filename)
    End If

  End Sub

  Public Sub StampaSummary(NomeFile As String, Intestazione As String, RigheTesto As List(Of String), UpwindDG As List(Of clsRigaVmg), DownwindDG As List(Of clsRigaVmg), ReachingDG As List(Of clsRigaReaching), Images As Dictionary(Of String, List(Of System.IO.Stream)))
    Dim document As PdfDocument = New PdfDocument
    Dim page As PdfPage = document.AddPage
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
    Dim Hdisp As Double = page.Height.Point

    Dim LeftMargin As Double = 10
    Dim TopMargin As Double = 5
    Dim Spacer As Double = 2
    Dim Wdisp As Double = page.Width.Point - (2 * LeftMargin) - (3 * Spacer)
    Dim CurrX As Double = LeftMargin
    Dim CurrY As Integer = TopMargin

    Dim sFont As New XFont("Verdana", 16)
    Dim Fcolor As XBrush = XBrushes.Black
    Dim H As Double = gfx.MeasureString(Intestazione, sFont).Height
    Dim W As Double = gfx.MeasureString(Intestazione, sFont).Width
    gfx.DrawString(Intestazione, sFont, Fcolor, (Wdisp / 2) - (W / 2), CurrY + H)
    CurrY += H * 1.2
    sFont = New XFont("Verdana", 10)
    gfx.DrawString(RigheTesto(0).Replace(vbTab, "  "), sFont, Fcolor, (Wdisp / 2) - (W / 2), CurrY + H)
    CurrY += H * 1.2


    sFont = New XFont("Verdana", 7)
    Dim LTab = Wdisp
    CurrY += H
    CurrX = LeftMargin
    CurrY = StampaTabellaVmg(gfx, "VMG Sailing Upwind", UpwindDG, CurrY, LeftMargin, LTab, sFont, Fcolor, "#Tacks")
    CurrY += H * 0.2
    LTab = Wdisp / 4
    For i As Integer = 1 To 3
      Dim riga = RigheTesto(i)
      H = gfx.MeasureString(riga, sFont).Height
      CurrX = LeftMargin
      Dim c = 0
      For Each colonna In riga.Split(vbTab)
        DrawStringLeft(colonna, gfx, sFont, Fcolor, CurrX + (c * LTab), CurrY, LTab, ColoreDaRigaColonnaTesto(i, c), -1)
        c += 1
      Next
      CurrY += H * 1.2
    Next


    LTab = Wdisp
    CurrY += H
    CurrX = LeftMargin
    CurrY = StampaTabellaVmg(gfx, "VMG Sailing Downwind", DownwindDG, CurrY, CurrX, LTab, sFont, Fcolor, "#Gybes")
    CurrY += H * 0.2
    LTab = Wdisp / 4
    For i As Integer = 4 To 6
      'Dim riga = RigheTesto(i).Replace("Tack", "Gybe")
      Dim riga = RigheTesto(i)
      H = gfx.MeasureString(riga, sFont).Height
      CurrX = LeftMargin
      Dim c = 0
      For Each colonna In riga.Split(vbTab)
        DrawStringLeft(colonna, gfx, sFont, Fcolor, CurrX + (c * LTab), CurrY, LTab, ColoreDaRigaColonnaTesto(i, c), -1)
        c += 1
      Next
      CurrY += H * 1.2
    Next



    CurrY += H * 2
    CurrX = LeftMargin
    'sFont = New XFont("Verdana", 10)
    CurrY = StampaTabellaReaching(gfx, "Straight Line Sailing (Not Vmg)", ReachingDG, CurrY, CurrX, Wdisp, sFont, Fcolor)
    CurrY += H * 0.2
    Dim cln = 0
    For Each colonna In RigheTesto(7).Split(vbTab)
      DrawStringLeft(colonna, gfx, sFont, Fcolor, CurrX + (cln * LTab), CurrY, LTab, ColoreDaRigaColonnaTesto(7, cln), -1)
      cln += 1
    Next


    page = document.AddPage
    gfx = XGraphics.FromPdfPage(page)

    CurrY = TopMargin
    sFont = New XFont("Verdana", 16)
    H = gfx.MeasureString(Intestazione, sFont).Height
    W = gfx.MeasureString(Intestazione, sFont).Width
    gfx.DrawString(Intestazione, sFont, Fcolor, (Wdisp / 2) - (W / 2), CurrY + H)
    CurrY += H * 3

    Dim LatoImg As Double = Math.Min(Wdisp / 4, (Hdisp - H * 4) / 5)
    For Each Riga In Images
      CurrX = LeftMargin
      'If Riga.Value.Count = 3 Then
      '  For i As Integer = 0 To Riga.Value.Count - 1
      '    Dim LI = LatoImg
      '    If i = 1 Then LI = LatoImg * 2 + Spacer
      '    Dim R As New XRect(CurrX, CurrY, LI, LatoImg)
      '    Dim pdfimg = XImage.FromStream(Riga.Value(i))
      '    gfx.DrawImage(pdfimg, R)
      '    CurrX += LatoImg + Spacer
      '    If i = 1 Then
      '      CurrX += LatoImg
      '    End If
      '  Next
      'Else ' 4
      For i As Integer = 0 To Riga.Value.Count - 1
        Dim R As New XRect(CurrX, CurrY, LatoImg, LatoImg)
        Dim pdfimg = XImage.FromStream(Riga.Value(i))
        gfx.DrawImage(pdfimg, R)
        CurrX += LatoImg + Spacer
      Next
      'End If
      CurrY += (LatoImg + Spacer)
    Next



    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim Input As New UserControl_InputBox(NomeFile, "Pdf File Name")
    Input.ShowDialog()

    If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then
      Dim Nome As String = Input.Testo
      Dim filename As String = VerificaFilePath(path & "\" & Nome & ".pdf")
      document.Save(filename)
      Process.Start(filename)
    End If


  End Sub

  Private Function StampaTabellaVmg(ByRef gfx As XGraphics, Header As String, Tabella As List(Of clsRigaVmg), CurrY As Double, X As Double, Larghezza As Double, sFont As XFont, Fcolor As XBrush, ManLable As String) As Double
    ' restituisce la y per la riga successiva
    Dim Intfont As New XFont(sFont.FontFamily.Name, sFont.Size + 2)
    Dim H As Double = gfx.MeasureString(Header, Intfont).Height
    Dim W As Double = gfx.MeasureString(Header, Intfont).Width
    gfx.DrawString(Header, Intfont, Fcolor, X + (Larghezza / 2) - (W / 2), CurrY + H)
    CurrY += H * 1.2
    Dim CurrX = X

    Dim Hfont As New XFont("Tahoma", sFont.Size - 1, XFontStyle.Bold)
    Dim LB = Larghezza / 13
    Dim Testo As String
    Dim r = 0
    For Each riga In Tabella
      If riga Is Tabella.First Then
        Dim Intestazioni As String() = {"Range", "Tws", "VmgT%", "Stbd", "Port", "BsT%", "Stbd", "Port", "TwaD", "Stbd", "Port", ManLable, "LossBL"}
        For Each Testo In Intestazioni
          H = DrawStringCenter(Testo, gfx, Hfont, Fcolor, CurrX, CurrY, LB, XColors.LightSlateGray, -1).Height * 1.1
          CurrX += LB
        Next
      End If
      CurrY += H * 1.2

      Dim Valori As String() = {riga.Group, riga.Tws, riga.Both.VmgTgtP, riga.Stbd.VmgTgtP, riga.Port.VmgTgtP, riga.Both.BsTgtP, riga.Stbd.BsTgtP, riga.Port.BsTgtP, riga.Both.TwaTgtD, riga.Stbd.TwaTgtD, riga.Port.TwaTgtD, riga.Both.ManoeuversNr, riga.Both.ManoeuversLossAvg}
      CurrX = X
      Dim c = 0
      For Each Testo In Valori
        H = DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, ColoreDaRigaColonnaVmg(r, c), -1).Height
        CurrX += LB
        c += 1
      Next
      r += 1
      'CurrY += H
    Next
    Return CurrY + H
  End Function

  Private Function ColoreDaRigaColonnaTesto(Riga As Integer, Colonna As Integer) As XColor
    Select Case Riga
      Case 0
        Return XColors.LightSkyBlue
      Case 7
        Return XColors.LightYellow
      Case Else
        Select Case Colonna
          Case 1
            Return XColors.LightGreen
          Case 2
            Return XColors.IndianRed
          Case Else
            If Riga Mod 2 = 0 Then
              Return XColors.WhiteSmoke
            Else
              Return XColors.LightGray
            End If
        End Select
    End Select
  End Function

  Private Function ColoreDaRigaColonnaVmg(Riga As Integer, Colonna As Integer) As XColor
    Select Case Colonna
      Case 0
        Return XColors.Gold
      Case 3, 6, 9
        Return XColors.LightGreen
      Case 4, 7, 10
        Return XColors.IndianRed
      Case 11, 12
        Return XColors.LightSkyBlue
      Case Else
        If Riga Mod 2 = 0 Then
          Return XColors.WhiteSmoke
        Else
          Return XColors.LightGray
        End If
    End Select
  End Function

  Private Function ColoreDaRigaColonnaReaching(Riga As Integer, Colonna As Integer) As XColor
    Select Case Colonna
      Case 0
        Return XColors.Gold
      Case 1, 5, 9, 13
        Return XColors.LightYellow
      Case Else
        If Riga Mod 2 = 0 Then
          Return XColors.WhiteSmoke
        Else
          Return XColors.LightGray
        End If

    End Select
  End Function

  Private Function StampaTabellaReaching(ByRef gfx As XGraphics, Header As String, Tabella As List(Of clsRigaReaching), CurrY As Double, X As Double, Larghezza As Double, sFont As XFont, Fcolor As XBrush) As Double
    ' restituisce la y per la riga successiva
    Dim Intfont As New XFont(sFont.FontFamily.Name, sFont.Size + 2)
    Dim H As Double = gfx.MeasureString(Header, Intfont).Height
    Dim W As Double = gfx.MeasureString(Header, Intfont).Width
    gfx.DrawString(Header, Intfont, Fcolor, X + (Larghezza / 2) - (W / 2), CurrY + H)
    CurrY += H * 1.2
    Dim CurrX = X

    Dim Hfont As New XFont("Tahoma", sFont.Size - 1, XFontStyle.Bold)
    Dim LB = Larghezza / 17
    Dim Testo As String
    Dim r = 0
    For Each riga In Tabella
      Dim vr As New List(Of clsValoriRigaReaching)
      vr.Add(riga.PolPercAtTwa60)
      vr.Add(riga.PolPercAtTwa70)
      vr.Add(riga.PolPercAtTwa80)
      vr.Add(riga.PolPercAtTwa90)
      If riga Is Tabella.First Then
        Dim Intestazioni As New List(Of String)
        Intestazioni.Add("Range")
        For i As Integer = 0 To vr.Count - 1
          Intestazioni.Add("Tws" & (60 + (i * 10)).ToString)
          Intestazioni.Add("Twa")
          Intestazioni.Add("Pol%")
          Intestazioni.Add("Bs")
        Next
        For Each Testo In Intestazioni
          H = DrawStringCenter(Testo, gfx, Hfont, Fcolor, CurrX, CurrY, LB, XColors.LightSlateGray, -1).Height * 1.1
          CurrX += LB
        Next
      End If
      CurrY += H * 1.2
      Dim Valori As New List(Of String)
      Valori.Add(riga.Group)
      For i As Integer = 0 To vr.Count - 1
        Valori.Add(vr(i).Tws)
        Valori.Add(vr(i).Twa)
        Valori.Add(vr(i).BsPolP)
        Valori.Add(vr(i).Bs)
      Next
      CurrX = X
      Dim c = 0
      For Each Testo In Valori
        H = DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, ColoreDaRigaColonnaReaching(r, c), -1).Height
        CurrX += LB
        c += 1
      Next
      r += 1
    Next

    CurrX = X
    CurrY += 2 * H
    r = 0
    For Each riga In Tabella
      Dim vr As New List(Of clsValoriRigaReaching)
      vr.Add(riga.PolPercAtTwa100)
      vr.Add(riga.PolPercAtTwa110)
      vr.Add(riga.PolPercAtTwa120)
      vr.Add(riga.PolPercAtTwa130)
      If riga Is Tabella.First Then
        Dim Intestazioni As New List(Of String)
        Intestazioni.Add("Range")
        For i As Integer = 0 To vr.Count - 1
          Intestazioni.Add("Tws" & (100 + (i * 10)).ToString)
          Intestazioni.Add("Twa")
          Intestazioni.Add("Pol%")
          Intestazioni.Add("Bs")
        Next
        For Each Testo In Intestazioni
          H = DrawStringCenter(Testo, gfx, Hfont, Fcolor, CurrX, CurrY, LB, XColors.LightSlateGray, -1).Height * 1.1
          CurrX += LB
        Next
      End If
      CurrY += H * 1.2
      Dim Valori As New List(Of String)
      Valori.Add(riga.Group)
      For i As Integer = 0 To vr.Count - 1
        Valori.Add(vr(i).Tws)
        Valori.Add(vr(i).Twa)
        Valori.Add(vr(i).BsPolP)
        Valori.Add(vr(i).Bs)
      Next
      CurrX = X
      Dim c = 0
      For Each Testo In Valori
        H = DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, ColoreDaRigaColonnaReaching(r, c), -1).Height
        CurrX += LB
        c += 1
      Next
      r += 1
    Next
    Return CurrY + H
  End Function

  Public Function StampaReportPartenza(Immagine As System.IO.Stream, DatiPartenza As clsExpeditionStart, Optional RaceReport As SummaryReport = Nothing) As String
    If DatiPartenza Is Nothing Then Return String.Empty
    Dim PlotPerPage As Integer = 3
    Dim document As PdfDocument = New PdfDocument
    ' Create an empty page
    Dim page As PdfPage = document.AddPage
    ' Get an XGraphics object for drawing
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    Dim Hdisp As Double = page.Height
    'Wdisp \= 2

    Dim LeftMargin As Double = 5
    Dim Wdisp As Double = page.Width.Point - (3 * LeftMargin)
    Dim CurrX As Double = LeftMargin
    Dim CurrY As Integer = 0

    Dim sFont As New XFont("Verdana", 20)
    Dim Fcolor As XBrush = XBrushes.Black

    Dim ValidRaceReport As Boolean = False
    If Not RaceReport Is Nothing Then

      sFont = New XFont("Verdana", 26)
      Dim TestoReport As String = RaceReport.Rows.First.DT.ToString("dd MMM yyyy HH:mm:ss")
      TestoReport &= " - " & RaceReport.Rows.Last.DT.ToString("HH:mm:ss")
      CurrY += DrawStringCenter(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightGray, -1).Height * 1.2

      sFont = New XFont("Verdana", 18)
      TestoReport = "TWS: " & RaceReport.PeriodData.Details.Tws.Avg.ToString("F1")
      TestoReport &= " (" & RaceReport.PeriodData.Details.Tws.MinVal.ToString("F1") & "-" & RaceReport.PeriodData.Details.Tws.MaxVal.ToString("F1") & ")   "
      'CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.Black, -1).Height * 1.5

      TestoReport &= "TWD: " & RaceReport.PeriodData.Details.Twd.Avg.ToString("F0")
      TestoReport &= " (" & RaceReport.PeriodData.Details.Twd.MinVal.ToString("F0") & "-" & RaceReport.PeriodData.Details.Twd.MaxVal.ToString("F0") & ")"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.3

      sFont = New XFont("Verdana", 14)
      TestoReport = "Wind @Top Mark: Tws: " & RaceReport.TwsDelta(SummaryReport.Andatura.UpwindVmg).ToString("F1") & "k, Twd: " & RaceReport.TwdDelta(SummaryReport.Andatura.UpwindVmg).ToString("F0") & "°"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1

      TestoReport = "Wind @Bottom Mark: Tws: " & RaceReport.TwsDelta(SummaryReport.Andatura.DownWindVmg).ToString("F1") & "k, Twd: " & RaceReport.TwdDelta(SummaryReport.Andatura.DownWindVmg).ToString("F0") & "°"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1

      sFont = New XFont("Verdana", 12)
      TestoReport = "Tacks: #" & RaceReport.UpwindAndReachingData.NrOfManoeuvers
      TestoReport &= ", avg inLine Loss " & RaceReport.UpwindAndReachingData.PerManoeuverLossInlineMeters.ToString("F0") & " m"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1
      If (RaceReport.UpwindAndReachingData.NrOfManoeuversInTheFirstMinute > 0) Then
        sFont = New XFont("Verdana", 11)
        TestoReport = " + " & RaceReport.UpwindAndReachingData.NrOfManoeuversInTheFirstMinute & " early"
        TestoReport &= ", avg inLine Loss " & RaceReport.UpwindAndReachingData.PerManoeuverFirtMinuteLossInlineMeters.ToString("F0") & " m"
        CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1
      End If

      sFont = New XFont("Verdana", 12)
      TestoReport = "Gybes: #" & RaceReport.DownWindData.NrOfManoeuvers
      TestoReport &= ", avg inLine Loss " & RaceReport.DownWindData.PerManoeuverLossInlineMeters.ToString("F0") & " m"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1
      If (RaceReport.DownWindData.NrOfManoeuversInTheFirstMinute > 0) Then
        sFont = New XFont("Verdana", 11)
        TestoReport = " (+" & RaceReport.DownWindData.NrOfManoeuversInTheFirstMinute & " early)"
        TestoReport &= ", avg inLine Loss " & RaceReport.DownWindData.PerManoeuverFirtMinuteLossInlineMeters.ToString("F0") & " m"
        CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1
      End If

      sFont = New XFont("Verdana", 20)
      TestoReport = "Upwind"
      CurrY += DrawStringCenter(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.Orange, -1).Height * 1.1

      sFont = New XFont("Verdana", 14)
      TestoReport = "First minute performance: " & (RaceReport.UpwindAndReachingData.FirstMinuteVmg / RaceReport.UpwindAndReachingData.SailingVmg * 100).ToString("F1") & "%"
      TestoReport &= ", Mark approach performance: " & (RaceReport.UpwindAndReachingData.MarkApproachVmg / RaceReport.UpwindAndReachingData.SailingVmg * 100).ToString("F1") & "%"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1

      sFont = New XFont("Verdana", 12)
      TestoReport = "Time@98+ " & (RaceReport.UpwindAndReachingData.TimePercAtVmgMoreThan98).ToString("F0") & "%"
      TestoReport &= ", Time@95+ " & (RaceReport.UpwindAndReachingData.TimePercAtVmgMoreThan95).ToString("F0") & "%"
      TestoReport &= ", Time@90+ " & (RaceReport.UpwindAndReachingData.TimePercAtVmgMoreThan90).ToString("F0") & "%"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1

      sFont = New XFont("Verdana", 12)
      TestoReport = " Tws of " & (RaceReport.UpwindAndReachingData.Details.Tws.Avg - 2).ToString("F1") & " is like a " & Math.Abs(RaceReport.UpwindAndReachingData.TwsMinus2eq).ToString("F0") & "° lift, Tws of " & (RaceReport.UpwindAndReachingData.Details.Tws.Avg + 2).ToString("F1") & " is like a " & Math.Abs(RaceReport.UpwindAndReachingData.TwsPlus2eq).ToString("F0") & "° header"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1

      TestoReport = " Tack loss is like to sail " & RaceReport.UpwindAndReachingData.ManoeuversLossTwsEqTime.ToString("mm\:ss") & " in a 2 kts lull"
      TestoReport &= " or " & RaceReport.UpwindAndReachingData.ManoeuversLossTwaEqTime.ToString("mm\:ss") & " in a 5° header"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1


      Dim TableCharSize As Integer = 14
      Dim Columns As Integer = 5
      Dim CellW As Double = Wdisp / Columns
      ' Tabella Upwind
      sFont = New XFont("Verdana", TableCharSize, XFontStyle.Bold)
      CurrY += PrintRowHeaders("", CellW, XColors.WhiteSmoke, gfx, Fcolor, sFont, LeftMargin, CurrY)
      Dim RrCPari As XColor = XColors.WhiteSmoke
      Dim RrCDispari As XColor = XColors.LightGray
      sFont = New XFont("Verdana", TableCharSize, XFontStyle.Regular)
      CurrY += PrintRowValues("Bs", 1, RaceReport.UpwindAndReachingData.Details.Bs, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Twa", 0, RaceReport.UpwindAndReachingData.Details.Twa, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCDispari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("BsT%", 1, RaceReport.UpwindAndReachingData.Details.BsTp, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("TwaD", 0, RaceReport.UpwindAndReachingData.Details.TwaD, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCDispari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Vmg%", 1, RaceReport.UpwindAndReachingData.Details.VmgP, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Vmg*", 1, RaceReport.UpwindAndReachingData.Details.RecVmgP, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      Dim p As Boolean = False
      For Each c In RaceReport.UpwindAndReachingData.Details.ExtraChannels
        CurrY += PrintRowValues(c.Channel.ShortName, c.Channel.Decimals, c, RaceReport.UpwindAndReachingData.Details.Tws.Avg, True, CellW, IIf(p, RrCPari, RrCDispari), gfx, Fcolor, sFont, LeftMargin, CurrY)
        p = Not p
      Next
      CurrX = LeftMargin

      sFont = New XFont("Verdana", 22)
      TestoReport = "Downwind"
      CurrY += DrawStringCenter(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightBlue, -1).Height * 1.2

      sFont = New XFont("Verdana", 14)
      TestoReport = "First minute performance: " & (RaceReport.DownWindData.FirstMinuteVmg / RaceReport.DownWindData.SailingVmg * 100).ToString("F1") & "%"
      TestoReport &= ", Mark approach performance: " & (RaceReport.DownWindData.MarkApproachVmg / RaceReport.DownWindData.SailingVmg * 100).ToString("F1") & "%"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1

      sFont = New XFont("Verdana", 12)
      TestoReport = "Time@98+: " & (RaceReport.DownWindData.TimePercAtVmgMoreThan98).ToString("F0") & "%"
      TestoReport &= ", Time@95+: " & (RaceReport.DownWindData.TimePercAtVmgMoreThan95).ToString("F0") & "%"
      TestoReport &= ", Time@90+: " & (RaceReport.DownWindData.TimePercAtVmgMoreThan90).ToString("F0") & "%"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.1

      sFont = New XFont("Verdana", 12)
      TestoReport = " Tws of " & (RaceReport.DownWindData.Details.Tws.Avg - 2).ToString("F1") & " is like a " & Math.Abs(RaceReport.DownWindData.TwsMinus2eq).ToString("F0") & "° lift, Tws of " & (RaceReport.DownWindData.Details.Tws.Avg + 2).ToString("F1") & " is like a " & Math.Abs(RaceReport.DownWindData.TwsPlus2eq).ToString("F0") & "° header"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1

      TestoReport = " Gybe loss is like to sail " & RaceReport.DownWindData.ManoeuversLossTwsEqTime.ToString("mm\:ss") & " in a 2 kts lull"
      TestoReport &= " or " & RaceReport.DownWindData.ManoeuversLossTwaEqTime.ToString("mm\:ss") & " in a 5° lift"
      CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.WhiteSmoke, -1).Height * 1.1

      'sFont = New XFont("Verdana", 13)
      'TestoReport = "Gybes: #" & RaceReport.DownWindData.NrOfManoeuvers
      'TestoReport &= ", avg inLine Loss " & RaceReport.DownWindData.PerManoeuverLossInlineMeters.ToString("F0") & " m"
      'TestoReport &= ", it's like " & RaceReport.DownWindData.ManoeuversLossTwsEqTime.ToString("mm\:ss") & " in a 2 kts lull"
      'TestoReport &= " or like " & RaceReport.DownWindData.ManoeuversLossTwaEqTime.ToString("mm\:ss") & " in a 5° lift"
      'CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightBlue, -1).Height * 1.2

      'If (RaceReport.DownWindData.NrOfManoeuversInTheFirstMinute > 0) Then
      '    TestoReport = " Early Gybe: #" & RaceReport.DownWindData.NrOfManoeuversInTheFirstMinute
      '    TestoReport &= ", avg inLine Loss " & RaceReport.DownWindData.PerManoeuverFirtMinuteLossInlineMeters.ToString("F0") & " m"
      '    CurrY += DrawStringLeft(TestoReport, gfx, sFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightYellow, -1).Height * 1.2
      'End If

      ' Tabella Downwind
      sFont = New XFont("Verdana", TableCharSize, XFontStyle.Bold)
      CurrY += PrintRowHeaders("", CellW, XColors.WhiteSmoke, gfx, Fcolor, sFont, LeftMargin, CurrY)
      sFont = New XFont("Verdana", TableCharSize, XFontStyle.Regular)
      CurrY += PrintRowValues("Bs", 1, RaceReport.DownWindData.Details.Bs, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Twa", 0, RaceReport.DownWindData.Details.Twa, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCDispari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("BsT%", 1, RaceReport.DownWindData.Details.BsTp, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("TwaD", 0, RaceReport.DownWindData.Details.TwaD, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCDispari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Vmg%", 1, RaceReport.DownWindData.Details.VmgP, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      CurrY += PrintRowValues("Vmg*", 1, RaceReport.DownWindData.Details.RecVmgP, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, RrCPari, gfx, Fcolor, sFont, LeftMargin, CurrY)
      p = False
      For Each c In RaceReport.DownWindData.Details.ExtraChannels
        CurrY += PrintRowValues(c.Channel.ShortName, c.Channel.Decimals, c, RaceReport.DownWindData.Details.Tws.Avg, False, CellW, IIf(p, RrCPari, RrCDispari), gfx, Fcolor, sFont, LeftMargin, CurrY)
        p = Not p
      Next
      CurrX = LeftMargin

      sFont = New XFont("Verdana", 20)
      ValidRaceReport = True
      page = document.AddPage
      gfx = XGraphics.FromPdfPage(page)
      CurrY = 0
    End If



    Dim Intestazione As String = DatiPartenza.GpsStartTime.ToString("dd MMM yyyy HH:mm:ss") & " Start Report"
    Dim ImgK As Double = 0.42
    Dim H As Double = gfx.MeasureString(Intestazione, sFont).Height
    Dim W As Double = gfx.MeasureString(Intestazione, sFont).Width
    gfx.DrawString(Intestazione, sFont, Fcolor, (Wdisp / 2) - (W / 2), CurrY + H)
    CurrY += H * 2
    CurrX = (2 * LeftMargin) + (Wdisp * ImgK)
    Dim TopImg As Double = CurrY

    sFont = New XFont("Verdana", 18)

    Dim Testo As String = DatiPartenza.StartDetails.Split(",")(0)
    H = DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height
    CurrY += H * 1.1
    If Not DatiPartenza.StartDetails = "" Then
      Testo = DatiPartenza.StartDetails.Split(",")(1)
      H = DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height
      CurrY += H * 2
    End If

    sFont = New XFont("Verdana", 14)

    Testo = DatiPartenza.LineGeometry
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.2
    Testo = DatiPartenza.StartBias
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.4
    Testo = DatiPartenza.BasicDataAtGun
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.2
    Testo = DatiPartenza.BasicDataAtPlusTwo
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.2
    Testo = DatiPartenza.AccelerationLine5
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.5

    sFont = New XFont("Verdana", 12)
    Testo = DatiPartenza.AccelerationLine1
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.1
    Testo = DatiPartenza.AccelerationLine2
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.1
    Testo = DatiPartenza.AccelerationLine3
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.1
    Testo = DatiPartenza.AccelerationLine4
    CurrY += DrawString(Testo, gfx, sFont, Fcolor, CurrX, CurrY).Height * 1.3



    ' stampa l immagine della partenza

    Dim Himg As Double = Wdisp * ImgK
    Dim bottomTmp = CurrY + Himg

    If bottomTmp > Hdisp Then
      CurrY = 0
      page = document.AddPage
      gfx = XGraphics.FromPdfPage(page)
    End If
    Dim R As New XRect(LeftMargin, TopImg, Himg, Himg)
    Dim pdfimg = XImage.FromStream(Immagine)
    gfx.DrawImage(pdfimg, R)
    CurrY = TopImg + Himg + LeftMargin

    ' stampa la tabella
    Dim LB As Integer = Wdisp / 13
    sFont = New XFont("Verdana", 10, XFontStyle.Bold)

    Dim LS As Integer = -1
    Dim Ca As XColor = XColors.LightYellow
    Dim Cb As XColor = XColors.WhiteSmoke
    Dim Ci As XColor = XColors.LightGray
    Dim C0 As XColor = XColors.Yellow

    CurrX = LeftMargin
    Testo = "To Gun"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "VdistBL"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Tws"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Twd"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Bs"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Twa"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Vmg %"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Bs %"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Twa D"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Heel"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "Rdr"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    Testo = "eTTK"
    DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    CurrX += LB
    'Testo = "FS"
    'DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS)
    'CurrX += LB
    Testo = "FSm"
    CurrY += DrawStringCenter(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, Ci, LS).Height * 1.1

    sFont = New XFont("Verdana", 10, XFontStyle.Regular)
    Dim d As Boolean = True
    For Each BD In DatiPartenza.BoatDataTable
      Dim C As XColor = Ca
      If d Then C = Cb
      CurrX = LeftMargin
      If BD.SecondsToStart = 0 Then C = C0
      Testo = "" & BD.SecondsToStart.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & (BD.Vdist / AppConfig.ActiveProfile.BoatLenghtInMeters).ToString("F1") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Tws.ToString("F1") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Twd.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Bs.ToString("F1") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Twa.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.VmgP.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.BstP.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.TwaD.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Heel.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.Rdr.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      Testo = "" & BD.ExpTimeToBurn.ToString("F0") & ""
      DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      CurrX += LB
      'Testo = "" & BD.FinalShut.ToString("F0") & ""
      'DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS)
      'CurrX += LB
      Testo = "" & BD.FinalShutMt.ToString("F0") & ""
      CurrY += DrawStringRight(Testo, gfx, sFont, Fcolor, CurrX, CurrY, LB, C, LS).Height
      d = Not d
    Next


    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = DatiPartenza.GpsStartTime.ToString("yyyyMMdd_HHmmss")
    'ApriExplorer(path, path & "\" & pstringadata & "_PavarotPlots.xlsx")

    Dim txt As String = "StartReport_"
    If ValidRaceReport Then txt = "RaceReport_StarTime_"

    Dim Input As New UserControl_InputBox(txt & pstringadata, "Pdf File Name")
    Input.ShowDialog()

    If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then
      Dim Nome As String = Input.Testo
      Dim filename As String = VerificaFilePath(path & "\" & Nome & ".pdf")
      document.Save(filename)
      Process.Start(filename)
      Return filename
    End If
    Return String.Empty
  End Function

  Private Function DrawString(Testo As String, gfx As XGraphics, sFont As XFont, Fcolor As XBrush, X As Double, Y As Double) As XSize
    If Testo Is Nothing OrElse Testo.Trim = "" Then
      Testo = " "
    End If
    Dim MS = gfx.MeasureString(Testo, sFont)
    gfx.DrawString(Testo, sFont, Fcolor, X, Y + MS.Height)
    Return MS
  End Function

  Private Function DrawStringLeft(Testo As String, gfx As XGraphics, sFont As XFont, Fcolor As XBrush, X As Double, Y As Double, Boxwidth As Double, LineColor As XColor, LineSize As Integer) As XSize
    If Testo Is Nothing OrElse Testo.Trim = "" Then
      Testo = " "
    End If
    Dim MS = gfx.MeasureString(Testo, sFont)
    If LineSize = -1 Then LineSize = MS.Height
    gfx.DrawLine(New XPen(LineColor, LineSize), New XPoint(X, Y + 1 + (MS.Height / 2)), New XPoint(X + Boxwidth, Y + 1 + (MS.Height / 2)))
    gfx.DrawString(Testo, sFont, Fcolor, X, Y + MS.Height)
    Return MS
  End Function

  Private Function DrawStringCenter(Testo As String, gfx As XGraphics, sFont As XFont, Fcolor As XBrush, X As Double, Y As Double, Boxwidth As Double, LineColor As XColor, LineSize As Integer) As XSize
    If Testo Is Nothing OrElse Testo.Trim = "" Then
      Testo = " "
    End If
    Dim MS = gfx.MeasureString(Testo, sFont)
    If LineSize = -1 Then LineSize = MS.Height
    gfx.DrawLine(New XPen(LineColor, LineSize), New XPoint(X, Y + 1 + (MS.Height / 2)), New XPoint(X + Boxwidth, Y + 1 + (MS.Height / 2)))
    gfx.DrawString(Testo, sFont, Fcolor, X + (Boxwidth / 2) - (MS.Width / 2), Y + MS.Height)
    Return MS
  End Function

  Private Function DrawStringCenterTitle(Testo As String, gfx As XGraphics, sFont As XFont, Fcolor As XBrush, X As Double, Y As Double, Boxwidth As Double, LineColor As XColor, LineSize As Integer) As XSize
    If Testo Is Nothing OrElse Testo.Trim = "" Then
      Testo = " "
    End If
    Dim MS = gfx.MeasureString(Testo, sFont)
    If LineSize = -1 Then LineSize = MS.Height * 2
    gfx.DrawLine(New XPen(LineColor, LineSize), New XPoint(X, Y + MS.Height / 2), New XPoint(X + Boxwidth, Y + MS.Height / 2))
    gfx.DrawString(Testo, sFont, Fcolor, X + (Boxwidth / 2) - (MS.Width / 2), Y + MS.Height)
    Return MS
  End Function

  Private Function DrawStringRight(Testo As String, gfx As XGraphics, sFont As XFont, Fcolor As XBrush, X As Double, Y As Double, Boxwidth As Double, LineColor As XColor, LineSize As Integer) As XSize
    If Testo Is Nothing OrElse Testo.Trim = "" Then
      Testo = " "
    End If
    Dim MS = gfx.MeasureString(Testo, sFont)
    If LineSize = -1 Then LineSize = MS.Height
    gfx.DrawLine(New XPen(LineColor, LineSize), New XPoint(X, Y + 1 + (MS.Height / 2)), New XPoint(X + Boxwidth, Y + 1 + (MS.Height / 2)))
    gfx.DrawString(Testo, sFont, Fcolor, X + Boxwidth - MS.Width, Y + MS.Height)
    Return MS
  End Function

  Private Function PrintRowValues(Name As String, Dec As Integer, Values As ChannelStats, Tws As Double, IsUpwind As Boolean, CellWidth As Double, CellColor As XColor, gfx As XGraphics, Fcolor As XBrush, Font As XFont, CurrX As Double, CurrY As Double) As Double
    Dim Testo As String = Name
    DrawStringRight(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)
    CurrX += CellWidth
    Testo = Values.Avg.ToString("F" & Dec.ToString)
    DrawStringRight(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)

    CurrX += CellWidth
    Testo = ""
    If Not Values.Channel Is Nothing Then
      Dim p = TgtManager.Tgt.Polare(Values.Channel.PolarHeader)
      If Not p Is Nothing Then
        Testo = p.TargetValue(Tws, IsUpwind).ToString("F" & Dec.ToString)
      End If
    End If
    DrawStringRight(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)


    CurrX += CellWidth
    Testo = Values.PAvg.ToString("F" & Dec.ToString)
    DrawStringRight(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)
    CurrX += CellWidth
    Testo = Values.SAvg.ToString("F" & Dec.ToString)
    DrawStringRight(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)
    Dim MS = gfx.MeasureString(Testo, Font)
    Return MS.Height * 1.1
  End Function

  Private Function PrintRowHeaders(TableName As String, CellWidth As Double, CellColor As XColor, gfx As XGraphics, Fcolor As XBrush, Font As XFont, CurrX As Double, CurrY As Double) As Double
    Dim Testo As String = TableName
    DrawStringCenter(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, CellColor, -1)
    CurrX += CellWidth
    Testo = "Avg"
    DrawStringCenter(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, XColors.LightYellow, -1)
    CurrX += CellWidth
    Testo = "Tgt"
    DrawStringCenter(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, XColors.LightYellow, -1)
    CurrX += CellWidth
    Testo = "Port"
    DrawStringCenter(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, XColors.Red, -1)
    CurrX += CellWidth
    Testo = "Stbd"
    DrawStringCenter(Testo, gfx, Font, Fcolor, CurrX, CurrY, CellWidth, XColors.Green, -1)
    Dim MS = gfx.MeasureString(Testo, Font)
    Return MS.Height * 1.05
  End Function

  Public Sub StampaSummaryHighlightReport(Titolo As String, NomeFile As String, GeneralTableRows As Collections.ObjectModel.ObservableCollection(Of RowBase))

    If GeneralTableRows Is Nothing Then Exit Sub

    'impostazioni pdf

    Dim document As PdfDocument = New PdfDocument
    Dim page As PdfPage = document.AddPage
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    Dim Hdisp As Double = page.Height
    Dim Wdisp As Double = page.Width

    Hdisp *= 0.9
    Wdisp *= 0.9

    Dim Xspace As Integer = 2
    Dim Yspace As Integer = 2
    Dim CurrY As Integer = 30
    Dim CurrX As Integer = Wdisp * 0.05

    Dim ttFont As New XFont("Verdana", 12, XFontStyle.Bold)
    Dim tFont As New XFont("Verdana", 14, XFontStyle.Bold)
    Dim hFont As New XFont("Verdana", 8, XFontStyle.Bold)
    Dim sFont As New XFont("Verdana", 8)


    Dim Fcolor As XBrush = XBrushes.Black

    Dim ChartTitle As String = Titolo & "Summary and Highlights"
    Dim s As XSize

    s = DrawStringCenterTitle(ChartTitle, gfx, ttFont, Fcolor, CurrX, CurrY, Wdisp, XColors.Goldenrod, -1)
    CurrY += s.Height * 4

    Dim DataRowCounter As Integer = 0

    Dim LastHeader As List(Of Cell) = Nothing
    For Each RB As RowBase In GeneralTableRows
      LastHeader = Nothing
      Select Case RB.Kind
        Case RowKind.Header
          Dim hdr As String = DirectCast(RB, SPwpf.HeaderRow).Title
          If hdr.Contains("Channels") Then
            page = document.AddPage
            gfx = XGraphics.FromPdfPage(page)
            CurrX = Wdisp * 0.05
            CurrY = 50
            s = DrawStringCenterTitle(ChartTitle, gfx, ttFont, Fcolor, CurrX, CurrY, Wdisp, XColors.Goldenrod, -1)
            CurrY += s.Height * 4
          End If
          s = DrawStringCenterTitle(hdr, gfx, tFont, Fcolor, CurrX, CurrY, Wdisp, XColors.LightSkyBlue, -1)
          CurrY += s.Height
          DataRowCounter = 0
        Case RowKind.Data
          Dim Celle = DirectCast(RB, SPwpf.DataRow).Cells
          Dim tw As Integer = 0
          For Each Cella In Celle
            tw += Cella.Width
          Next
          Dim W As Double = (Wdisp - (Xspace * Celle.Count + 1)) / Celle.Count
          If Celle.Last.IsBold Then
            LastHeader = Celle.ToList
            For Each Cella In Celle
              Dim K As Double = Cella.Width / (tw / Celle.Count)
              s = DrawStringCenter(Cella.Value, gfx, hFont, Fcolor, CurrX, CurrY, W * K, XColors.LightGray, -1)
              CurrX += (W * K + Xspace)
            Next
            DataRowCounter = 0
          Else
            For Each Cella In Celle
              Dim K As Double = Cella.Width / (tw / Celle.Count)
              Dim cc = DirectCast(Cella.Background, System.Windows.Media.SolidColorBrush).Color
              If cc = Colors.White Then
                If DataRowCounter / 2 = CInt(DataRowCounter / 2) Then
                  s = DrawStringRight(Cella.Value, gfx, sFont, Fcolor, CurrX, CurrY, W * K, XColors.LightYellow, -1)
                Else
                  s = DrawStringRight(Cella.Value, gfx, sFont, Fcolor, CurrX, CurrY, W * K, XColors.AntiqueWhite, -1)
                End If
              Else
                s = DrawStringRight(Cella.Value, gfx, sFont, Fcolor, CurrX, CurrY, W * K, XColors.Yellow, -1)
              End If
              CurrX += (W * K + Xspace)
            Next
            DataRowCounter += 1
          End If
        Case RowKind.Empty
          Dim h = DirectCast(RB, SPwpf.EmptyRow).Height
          CurrY += h + Yspace
          DataRowCounter = 0
      End Select
      CurrX = Wdisp * 0.05
      CurrY += s.Height + Yspace
      If CurrY > Hdisp Then
        page = document.AddPage
        gfx = XGraphics.FromPdfPage(page)
        CurrY = 50
        If Not LastHeader Is Nothing Then
          Dim tw As Integer = 0
          For Each Cella In LastHeader
            tw += Cella.Width
          Next
          Dim W As Double = (Wdisp - (Xspace * LastHeader.Count + 1)) / LastHeader.Count
          For Each Cella In LastHeader
            Dim K As Double = Cella.Width / (tw / LastHeader.Count)
            s = DrawStringCenter(Cella.Value, gfx, hFont, Fcolor, CurrX, CurrY, W * K, XColors.LightGray, -1)
            CurrX += (W * K + Xspace)
          Next
          CurrX = Wdisp * 0.05
        End If
        CurrY += s.Height * 2

      End If

    Next

    Dim path As String = DataProvider2020.Files.First.Directory.FullName

    Dim filename As String = VerificaFilePath(System.IO.Path.Combine(path, NomeFile & ".pdf"))
    document.Save(filename)
    ' ...and start a viewer.
    Process.Start(filename)



  End Sub



End Class


