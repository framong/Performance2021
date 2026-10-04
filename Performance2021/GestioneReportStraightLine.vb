Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports SciChart.Charting.Visuals

''' <summary>
''' Report PDF delle straight line (upwind, downwind, reaching).
''' Due versioni che condividono impaginazione e tabelle:
'''   CreaReportCompleto     intestazione, riepilogo per TWS, grafici raggruppati, elenco periodi, tabella dati
'''   CreaReportStatistiche  come sopra ma senza grafici (veloce, 1-3 pagine)
''' Ogni pagina ha numero di pagina e nome del file; l'altezza delle righe di grafici e' uguale su tutte le pagine.
''' </summary>
Public Class clsStraightLineReport

  Private Const Margine As Double = 28
  Private Const BandaPiePagina As Double = 26
  Private Const BandaIntestazioneRidotta As Double = 18
  Private Const Distanza As Double = 8
  Private Const AltezzaGruppo As Double = 16
  Private Const GraficiPerRiga As Integer = 3

  Private Class clsCtx
    Public Doc As PdfDocument
    Public Pagina As PdfPage
    Public Gfx As XGraphics
    Public Y As Double
    Public W As Double
    Public H As Double
    Public TitoloBreve As String
  End Class

  Private Class clsInfo
    Public Titolo As String
    Public TitoloBreve As String
    Public Tipo As String
    Public Periodi As New List(Of clsPeriod2021)
    Public Porta As Integer
    Public Dritta As Integer
    Public Chiavi As New Dictionary(Of String, System.Windows.Media.Color)
    Public Tr As clsTimeRange
    Public TwsMin As Double
    Public TwsMax As Double
    Public IsReaching As Boolean
  End Class

  Private Class clsStatPeriodo
    Public Periodo As clsPeriod2021
    Public Tws As Double = Double.NaN
    Public Twa As Double = Double.NaN
    Public Sow As Double = Double.NaN
    Public Perf As Double = Double.NaN
    Public Minuti As Double
    Public IsStbd As Boolean
  End Class

  Private ReadOnly NomiGruppi As String() = {"Performance", "Wind & sea", "Attitude, foils & leeway", "Rig & sails"}

#Region "Punti di ingresso"

  ''' <summary>
  ''' Crea il report secondo le opzioni: pagina 1 con intestazione e riepilogo, poi grafici, tabella dati e,
  ''' ultimo, l'elenco dei periodi. Con la sola opzione csv non crea il pdf.
  ''' </summary>
  Public Sub CreaReport(ControlliAvg As List(Of SciChartSurface), ControlliDistr As List(Of SciChartSurface), OutputType As clsStraightLineVM2020.eOutputType, ListaPeriodi As List(Of clsPeriod2021), ListaCanali As List(Of clsStraightLineChartSettings), Filtro As String, Opzioni As clsStraightLineReportOptions)
    Dim ConPdf As Boolean = Opzioni.PrintCharts OrElse Opzioni.PrintTable OrElse Opzioni.PrintPeriods
    If Not ConPdf AndAlso Not Opzioni.CreateTableCsv Then
      MsgBox("Nothing to create: tick at least one option under Make a pdf.", MsgBoxStyle.Information, "Straight Line report")
      Exit Sub
    End If
    Dim Info As clsInfo = CreaInfo(ListaPeriodi, OutputType, Filtro, "")
    If Info Is Nothing Then Exit Sub

    Dim Tabella As clsTabella = Nothing
    If Opzioni.PrintTable OrElse Opzioni.CreateTableCsv Then Tabella = CostruisciTabella(Info, ListaCanali)

    Dim PercorsoPdf As String = PercorsoLibero(Info, OutputType, ".pdf")
    If ConPdf Then
      Dim C As New clsCtx With {.Doc = New PdfDocument, .TitoloBreve = Info.TitoloBreve}
      NuovaPagina(C, False)
      DisegnaIntestazione(C, Info, OutputType)
      DisegnaRiepilogo(C, Info)
      If Opzioni.PrintCharts AndAlso Not ControlliAvg Is Nothing AndAlso ControlliAvg.Count > 0 Then DisegnaGrafici(C, ControlliAvg, ControlliDistr)
      If Opzioni.PrintTable AndAlso Not Tabella Is Nothing Then DisegnaTabellaDati(C, Tabella)
      If Opzioni.PrintPeriods Then DisegnaElencoPeriodi(C, Info, OutputType)
      Salva(C, PercorsoPdf)
    End If

    If Opzioni.CreateTableCsv Then
      If Tabella Is Nothing Then
        MsgBox("The table has no data: the csv file was not created.", MsgBoxStyle.Exclamation, "Straight Line report")
      Else
        Dim PercorsoCsv As String = If(ConPdf, PercorsoPdf.Substring(0, PercorsoPdf.Length - 4) & ".csv", PercorsoLibero(Info, OutputType, ".csv"))
        ScriviCsv(Tabella, PercorsoCsv)
        If Not ConPdf Then Process.Start("explorer.exe", "/select,""" & PercorsoCsv & """")
      End If
    End If
    If ConPdf Then Process.Start(PercorsoPdf)
  End Sub

#End Region

#Region "Dati del report"

  Private Function CreaInfo(ListaPeriodi As List(Of clsPeriod2021), OutputType As clsStraightLineVM2020.eOutputType, Filtro As String, Suffisso As String) As clsInfo
    Dim Info As New clsInfo
    Dim Up As Integer = 0
    Dim Dn As Integer = 0
    Dim Tws As New List(Of Double)
    Dim Inizio As DateTime = Nothing
    Dim Fine As DateTime = Nothing
    For Each StLn In ListaPeriodi
      If Not StLn.IsChecked Then Continue For
      Info.Periodi.Add(StLn)
      If Inizio = Nothing OrElse StLn.TR.Start < Inizio Then Inizio = StLn.TR.Start
      If Fine = Nothing OrElse StLn.TR.Finish > Fine Then Fine = StLn.TR.Finish
      If Not StLn.Keys.Trim = "" AndAlso Not Info.Chiavi.ContainsKey(StLn.Keys.Trim) Then
        Info.Chiavi.Add(StLn.Keys.Trim, ColoreDaOutputType(StLn, OutputType))
      End If
      Tws.Add(StLn.TwsDetails.AvgVal)
      If Math.Abs(StLn.AvgTwa) < 90 Then Up += 1 Else Dn += 1
      If StLn.IsStbd Then Info.Dritta += 1 Else Info.Porta += 1
      If Not StLn.StraightLineReachingDetails Is Nothing AndAlso StLn.StraightLineVmgDetails Is Nothing Then Info.IsReaching = True
    Next
    If Info.Periodi.Count = 0 Then
      MsgBox("No periods are checked: check the periods to include in the report.", MsgBoxStyle.Exclamation, "Straight Line report")
      Return Nothing
    End If

    Info.Tipo = "Straight Line"
    If Dn = 0 Then
      Info.Tipo &= " Upwind"
    ElseIf Up = 0 Then
      Info.Tipo &= " Downwind"
    End If
    Info.TwsMin = Tws.Min
    Info.TwsMax = Tws.Max
    Info.Tr = New clsTimeRange(Inizio, Fine)
    Dim Titolo As String = Info.Tipo & " (Tws:" & Info.TwsMin.ToString("F0") & "-" & Info.TwsMax.ToString("F0") & ") " & Filtro
    If Info.Tr.Durata.TotalDays < 1 Then
      Titolo &= " " & Info.Tr.Start.ToString("yyyy MM dd")
    Else
      Titolo &= ", " & Info.Tr.Start.ToString("yyyy MM dd") & " - " & Info.Tr.Finish.ToString("yyyy MM dd")
    End If
    Info.TitoloBreve = Titolo.Trim
    Info.Titolo = Titolo.Trim & Suffisso
    Return Info
  End Function

  ''' <summary>Media del canale nel periodo, con lo stesso calcolo dei punti dei grafici.</summary>
  Private Shared Function MediaCanale(Canale As clsChannel2020, p As clsPeriod2021) As Double
    If Canale Is Nothing OrElse Canale.Valori Is Nothing OrElse Canale.Valori.Count = 0 Then Return Double.NaN
    Dim Assoluto As Boolean = Canale.DataType = clsChannel2020.eDataType.e180
    Dim M As New clsValoriPeriodoCanale2020(Canale, p.TR, Assoluto)
    M.AggiornaValori(Assoluto)
    Return M.Avg
  End Function

  ''' <summary>
  ''' Valori del periodo per tabelle e elenco. Si calcolano dai canali attuali come i punti dei grafici: i dettagli salvati nel
  ''' file dei periodi sono stati calcolati alla creazione e possono differire se canali, target o correzioni sono cambiati.
  ''' </summary>
  Private Shared Function StatPeriodo(p As clsPeriod2021, IsReaching As Boolean) As clsStatPeriodo
    Dim S As New clsStatPeriodo With {.Periodo = p, .IsStbd = p.IsStbd, .Minuti = p.TR.Durata.TotalMinutes}
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chSow = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chPerf = DataProvider2020.CanaleDbl(If(IsReaching, clsChannels2020.eCanaliChiave.eBSPp, clsChannels2020.eCanaliChiave.eVMGp))
    If Not chTws Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSow Is Nothing AndAlso Not chPerf Is Nothing Then
      S.Tws = MediaCanale(chTws, p)
      S.Twa = MediaCanale(chTwa, p)
      S.Sow = MediaCanale(chSow, p)
      S.Perf = MediaCanale(chPerf, p)
      Return S
    End If
    ' canali non disponibili: ripiego sui dettagli salvati
    If Not p.TwsDetails Is Nothing Then S.Tws = p.TwsDetails.AvgVal
    If Not p.StraightLineVmgDetails Is Nothing Then
      With p.StraightLineVmgDetails
        If Not .Twa Is Nothing Then S.Twa = .Twa.AvgVal
        If Not .Sow Is Nothing Then S.Sow = .Sow.AvgVal
        If Not .VmgPerc Is Nothing Then S.Perf = .VmgPerc.AvgVal
      End With
    ElseIf Not p.StraightLineReachingDetails Is Nothing Then
      With p.StraightLineReachingDetails
        If Not .Twa Is Nothing Then S.Twa = .Twa.AvgVal
        If Not .Sow Is Nothing Then S.Sow = .Sow.AvgVal
        If Not .PolarPerc Is Nothing Then S.Perf = .PolarPerc.AvgVal
      End With
    End If
    Return S
  End Function

  ''' <summary>Media pesata sui minuti ignorando i valori non validi.</summary>
  Private Shared Function MediaPesata(Stat As IEnumerable(Of clsStatPeriodo), Valore As Func(Of clsStatPeriodo, Double)) As Double
    Dim Somma As Double = 0
    Dim Peso As Double = 0
    For Each s In Stat
      Dim v As Double = Valore(s)
      If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Continue For
      Somma += v * s.Minuti
      Peso += s.Minuti
    Next
    If Peso <= 0 Then Return Double.NaN
    Return Somma / Peso
  End Function

  Private Shared Function Formato(v As Double, Decimali As Integer) As String
    If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return "-"
    Return v.ToString("F" & Decimali)
  End Function

  Private Shared Function ToX(c As System.Windows.Media.Color) As XColor
    Return XColor.FromArgb(c.A, c.R, c.G, c.B)
  End Function

  Private Shared Function NomeFileDati() As String
    Try
      If DataProvider2020 IsNot Nothing AndAlso DataProvider2020.Files IsNot Nothing AndAlso DataProvider2020.Files.Count > 0 Then
        Return DataProvider2020.Files.First.Name
      End If
    Catch ex As Exception
    End Try
    Return ""
  End Function

#End Region

#Region "Pagine, intestazione, piè di pagina"

  Private Shared Sub NuovaPagina(C As clsCtx, ConIntestazione As Boolean)
    ' PdfSharp scrive il contenuto alla chiusura e non ammette due XGraphics aperti sulla stessa pagina
    If Not C.Gfx Is Nothing Then
      C.Gfx.Dispose()
      C.Gfx = Nothing
    End If
    C.Pagina = C.Doc.AddPage
    C.Gfx = XGraphics.FromPdfPage(C.Pagina)
    C.W = C.Pagina.Width.Point
    C.H = C.Pagina.Height.Point
    C.Y = Margine
    If ConIntestazione Then
      C.Gfx.DrawString(C.TitoloBreve, New XFont("Verdana", 7), XBrushes.Gray, New XRect(Margine, Margine - 10, C.W - 2 * Margine, 10), XStringFormats.CenterLeft)
      C.Gfx.DrawLine(New XPen(XColors.LightGray, 0.5), Margine, Margine + 2, C.W - Margine, Margine + 2)
      C.Y = Margine + BandaIntestazioneRidotta
    End If
  End Sub

  Private Shared Sub AssicuraSpazio(C As clsCtx, Altezza As Double)
    If C.Y + Altezza > C.H - BandaPiePagina Then NuovaPagina(C, True)
  End Sub

  Private Sub DisegnaIntestazione(C As clsCtx, Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType)
    C.Gfx.DrawString(Info.Titolo, New XFont("Verdana", 15, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 22), XStringFormats.CenterLeft)
    C.Y += 24
    Dim Minuti As Double = Info.Periodi.Sum(Function(p) p.TR.Durata.TotalMinutes)
    Dim Sotto As String = Info.Periodi.Count & " periods, " & Minuti.ToString("F1") & " min"
    Dim Nome As String = NomeFileDati()
    If Nome <> "" Then Sotto &= "   |   " & Nome
    C.Gfx.DrawString(Sotto, New XFont("Verdana", 8), XBrushes.Gray, New XRect(Margine, C.Y, C.W - 2 * Margine, 12), XStringFormats.CenterLeft)
    C.Y += 16
    DisegnaLegenda(C, Info, OutputType)
  End Sub

  ''' <summary>Una riga di legenda: mura (Port rosso / Stbd verde), chiavi, scala dei colori dei punti.</summary>
  Private Sub DisegnaLegenda(C As clsCtx, Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType)
    Dim F As New XFont("Verdana", 8)
    Dim X As Double = Margine
    Dim Riga As Double = 13

    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eGroupByKey, clsStraightLineVM2020.eOutputType.eColorByKey
        For Each Chiave In Info.Chiavi
          Dim Larghezza As Double = C.Gfx.MeasureString(Chiave.Key, F).Width + 22
          If X + Larghezza > C.W - Margine Then
            X = Margine
            C.Y += Riga
          End If
          C.Gfx.DrawRectangle(New XSolidBrush(ToX(Chiave.Value)), X, C.Y + 2, 9, 9)
          C.Gfx.DrawString(Chiave.Key, F, XBrushes.Black, X + 13, C.Y + 10)
          X += Larghezza
        Next
        C.Y += Riga + 4
      Case clsStraightLineVM2020.eOutputType.eGroupByTack, clsStraightLineVM2020.eOutputType.eColorByTack,
           clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
        X = DisegnaPallino(C, F, X, "Port: " & Info.Porta & " periods", XColors.Red)
        X = DisegnaPallino(C, F, X + 12, "Stbd: " & Info.Dritta & " periods", XColors.Green)
        If OutputType = clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc OrElse OutputType = clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc Then
          DisegnaScalaColori(C, F, X + 24, OutputType)
        End If
        C.Y += Riga + 4
      Case Else
        C.Gfx.DrawString("Periods: " & Info.Dritta & " Stbd, " & Info.Porta & " Port", F, XBrushes.Black, X, C.Y + 10)
        C.Y += Riga + 4
    End Select
  End Sub

  Private Shared Function DisegnaPallino(C As clsCtx, F As XFont, X As Double, Testo As String, Colore As XColor) As Double
    C.Gfx.DrawEllipse(New XSolidBrush(Colore), X, C.Y + 3, 8, 8)
    C.Gfx.DrawString(Testo, F, XBrushes.Black, X + 12, C.Y + 10)
    Return X + 12 + C.Gfx.MeasureString(Testo, F).Width
  End Function

  ''' <summary>Barra dei colori usata dai punti quando si colora per Vmg% o Bs% (stessa scala dei grafici).</summary>
  Private Shared Sub DisegnaScalaColori(C As clsCtx, F As XFont, X As Double, OutputType As clsStraightLineVM2020.eOutputType)
    Dim Minimo As Double = AppConfig.ActiveProfile.MinVmgPerformanceValue
    Dim Massimo As Double = AppConfig.ActiveProfile.MaxVmgPerformenceValue
    Dim Etichetta As String = If(OutputType = clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, "Dots: Vmg % of target", "Dots: Bs % of polar")
    Dim L As Double = C.Gfx.MeasureString(Etichetta, F).Width
    C.Gfx.DrawString(Etichetta, F, XBrushes.Black, X, C.Y + 10)
    X += L + 8
    Dim Passi As Integer = 40
    Dim Larghezza As Double = 110
    Dim Lp As Double = Larghezza / Passi
    For k As Integer = 0 To Passi - 1
      Dim Perf As Double = Minimo + (Massimo - Minimo) * (k + 0.5) / Passi
      C.Gfx.DrawRectangle(New XSolidBrush(ToX(ColoreDaPerformance(Perf))), X + k * Lp, C.Y + 2, Lp + 0.5, 9)
    Next
    C.Gfx.DrawString("<=" & CInt(Minimo), New XFont("Verdana", 7), XBrushes.Gray, X - 2, C.Y + 21)
    Dim T As String = ">=" & CInt(Massimo)
    C.Gfx.DrawString(T, New XFont("Verdana", 7), XBrushes.Gray, X + Larghezza - C.Gfx.MeasureString(T, New XFont("Verdana", 7)).Width + 2, C.Y + 21)
    C.Y += 8 ' le etichette della scala stanno sotto la barra
  End Sub


  ''' <summary>Nome file libero nella cartella dei dati (aggiunge _1, _2... se esiste gia'): pdf e csv della stessa creazione condividono il nome.</summary>
  Private Function PercorsoLibero(Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType, Estensione As String) As String
    Dim OutputTypeString As String = ""
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eColorByKey : OutputTypeString = "_ByKey"
      Case clsStraightLineVM2020.eOutputType.eColorByTack : OutputTypeString = "_ByTack"
      Case clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc : OutputTypeString = "_ByVmgTgtPerc"
      Case clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc : OutputTypeString = "_ByBsPolarPerc"
      Case clsStraightLineVM2020.eOutputType.eGroupByKey : OutputTypeString = "_GrByKey"
      Case clsStraightLineVM2020.eOutputType.eGroupByTack : OutputTypeString = "_GrGrByTack"
    End Select

    Dim Cartella As String = DataProvider2020.Files.First.Directory.FullName
    Dim Data As String = Info.Tr.Start.ToString("yyyyMMdd")
    If Info.Tr.Durata.TotalDays >= 1 Then Data = Info.Tr.Start.ToString("yyyyMMdd") & "_" & Info.Tr.Finish.ToString("yyyyMMdd")
    Dim Base As String = Cartella & "\" & Data & "_" & Info.Tipo & OutputTypeString & "_Tws" & Info.TwsMin.ToString("F0") & "t" & Info.TwsMax.ToString("F0")
    Dim Numero As Integer = 0
    Do
      Dim Suffisso As String = If(Numero = 0, "", "_" & Numero)
      ' si controllano entrambe le estensioni cosi' pdf e csv hanno lo stesso numero
      If Not System.IO.File.Exists(Base & Suffisso & ".pdf") AndAlso Not System.IO.File.Exists(Base & Suffisso & ".csv") Then
        Return Base & Suffisso & Estensione
      End If
      Numero += 1
    Loop While Numero <= 100
    Return Base & "_" & Numero & Estensione
  End Function

  ''' <summary>Piè di pagina su tutte le pagine (nome file, numero di pagina) e salvataggio.</summary>
  Private Sub Salva(C As clsCtx, Percorso As String)
    ' chiude l'XGraphics dell'ultima pagina prima di riaprire le pagine per il piè di pagina
    If Not C.Gfx Is Nothing Then
      C.Gfx.Dispose()
      C.Gfx = Nothing
    End If
    Dim Pagine As Integer = C.Doc.PageCount
    Dim F As New XFont("Verdana", 7)
    Dim Nome As String = NomeFileDati()
    For i As Integer = 0 To Pagine - 1
      Dim Pg As PdfPage = C.Doc.Pages(i)
      Dim G As XGraphics = XGraphics.FromPdfPage(Pg, XGraphicsPdfPageOptions.Append)
      Dim W As Double = Pg.Width.Point
      Dim H As Double = Pg.Height.Point
      G.DrawLine(New XPen(XColors.LightGray, 0.5), Margine, H - BandaPiePagina + 6, W - Margine, H - BandaPiePagina + 6)
      G.DrawString("Performance2021" & If(Nome = "", "", "  |  " & Nome), F, XBrushes.Gray, New XRect(Margine, H - BandaPiePagina + 8, W - 2 * Margine, 10), XStringFormats.CenterLeft)
      G.DrawString("Page " & (i + 1) & " / " & Pagine, F, XBrushes.Gray, New XRect(Margine, H - BandaPiePagina + 8, W - 2 * Margine, 10), XStringFormats.CenterRight)
      G.Dispose()
    Next
    C.Doc.Save(Percorso)
  End Sub

#End Region

#Region "Riepilogo per TWS"

  Private Shared Sub Cella(Gfx As XGraphics, Testo As String, F As XFont, X As Double, Y As Double, L As Double, H As Double, Riempimento As XColor, Allineamento As XStringFormat)
    Gfx.DrawRectangle(New XSolidBrush(Riempimento), X, Y, L, H)
    If Testo Is Nothing OrElse Testo = "" Then Exit Sub
    Gfx.DrawString(Testo, F, XBrushes.Black, New XRect(X + 2, Y, L - 4, H), Allineamento)
  End Sub

  Private Sub DisegnaRiepilogo(C As clsCtx, Info As clsInfo)
    Dim Stat As List(Of clsStatPeriodo) = Info.Periodi.Select(Function(p) StatPeriodo(p, Info.IsReaching)).ToList
    Dim Perf As String = If(Info.IsReaching, "Polar %", "Vmg %")
    Dim Fh As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim F As New XFont("Verdana", 7.5)
    Dim Fb As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim Hr As Double = 12
    Dim Larghezze As Double() = {70, 70, 60, 70, 110, 60, 60}
    Dim Totale As Double = Larghezze.Sum
    Dim Scala As Double = (C.W - 2 * Margine) / Totale
    For i As Integer = 0 To Larghezze.Length - 1
      Larghezze(i) *= Scala
    Next
    Dim Titoli As String() = {"TWS", "Periods P/S", "Minutes", Perf & " avg", Perf & " min - max", "Bs (kn)", "TWA (deg)"}

    AssicuraSpazio(C, 18 + Hr * 4)
    C.Gfx.DrawString("Summary by TWS", New XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 14), XStringFormats.CenterLeft)
    C.Y += 16
    DisegnaRigaRiepilogo(C, Titoli, Larghezze, Fh, Hr, XColors.LightGray, XStringFormats.Center)

    Dim Gruppi = Stat.Where(Function(s) Not Double.IsNaN(s.Tws)).GroupBy(Function(s) CInt(Math.Round(s.Tws))).OrderBy(Function(g) g.Key)
    Dim Alt As Boolean = False
    For Each G In Gruppi
      AssicuraSpazio(C, Hr)
      DisegnaRigaRiepilogo(C, RigaRiepilogo(G.Key.ToString, G.ToList), Larghezze, F, Hr, If(Alt, XColors.WhiteSmoke, XColors.White), XStringFormats.CenterRight)
      Alt = Not Alt
    Next
    AssicuraSpazio(C, Hr * 4)
    DisegnaRigaRiepilogo(C, RigaRiepilogo("All", Stat), Larghezze, Fb, Hr, XColors.LightYellow, XStringFormats.CenterRight)
    DisegnaRigaRiepilogo(C, RigaRiepilogo("Port", Stat.Where(Function(s) Not s.IsStbd).ToList), Larghezze, F, Hr, XColors.MistyRose, XStringFormats.CenterRight)
    DisegnaRigaRiepilogo(C, RigaRiepilogo("Stbd", Stat.Where(Function(s) s.IsStbd).ToList), Larghezze, F, Hr, XColors.Honeydew, XStringFormats.CenterRight)
    C.Y += 8
  End Sub

  Private Shared Function RigaRiepilogo(Etichetta As String, Stat As List(Of clsStatPeriodo)) As String()
    If Stat.Count = 0 Then Return {Etichetta, "0", "-", "-", "-", "-", "-"}
    Dim Porta As Integer = Stat.Where(Function(s) Not s.IsStbd).Count
    Dim Dritta As Integer = Stat.Where(Function(s) s.IsStbd).Count
    Dim Minuti As Double = Stat.Sum(Function(s) s.Minuti)
    Dim Valide = Stat.Where(Function(s) Not Double.IsNaN(s.Perf)).ToList
    Dim Intervallo As String = "-"
    If Valide.Count > 0 Then Intervallo = Valide.Min(Function(s) s.Perf).ToString("F1") & " - " & Valide.Max(Function(s) s.Perf).ToString("F1")
    Return {Etichetta, Porta & "/" & Dritta, Minuti.ToString("F1"),
            Formato(MediaPesata(Stat, Function(s) s.Perf), 1), Intervallo,
            Formato(MediaPesata(Stat, Function(s) s.Sow), 2), Formato(MediaPesata(Stat, Function(s) Math.Abs(s.Twa)), 1)}
  End Function

  Private Shared Sub DisegnaRigaRiepilogo(C As clsCtx, Valori As String(), Larghezze As Double(), F As XFont, H As Double, Riempimento As XColor, Allineamento As XStringFormat)
    Dim X As Double = Margine
    For i As Integer = 0 To Valori.Length - 1
      Cella(C.Gfx, Valori(i), F, X, C.Y, Larghezze(i), H, Riempimento, If(i = 0, XStringFormats.CenterLeft, Allineamento))
      X += Larghezze(i)
    Next
    C.Y += H
  End Sub

#End Region

#Region "Elenco periodi"

  Private Sub DisegnaElencoPeriodi(C As clsCtx, Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType)
    Dim Stat As List(Of clsStatPeriodo) = Info.Periodi.Select(Function(p) StatPeriodo(p, Info.IsReaching)).
      OrderBy(Function(s) If(Double.IsNaN(s.Tws), 0, Math.Round(s.Tws))).ThenBy(Function(s) s.Periodo.TR.Start).ToList
    Dim Perf As String = If(Info.IsReaching, "Polar %", "Vmg %")
    Dim Fh As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim F As New XFont("Verdana", 7.5)
    Dim Hr As Double = 11
    Dim Larghezze As Double() = {12, 62, 30, 44, 44, 44, 44, 52, 90}
    Dim Resto As Double = (C.W - 2 * Margine) - Larghezze.Sum
    Larghezze(Larghezze.Length - 1) += Math.Max(0, Resto)
    Dim Titoli As String() = {"", "Start", "Tack", "Dur (s)", "TWS", "TWA", "Bs (kn)", Perf, "Key"}

    NuovaPagina(C, True)
    C.Gfx.DrawString("Periods", New XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 14), XStringFormats.CenterLeft)
    C.Y += 16
    RigaElenco(C, Titoli, Larghezze, Fh, Hr, XColors.LightGray, Nothing)
    Dim Alt As Boolean = False
    For Each s In Stat
      If C.Y + Hr > C.H - BandaPiePagina Then
        NuovaPagina(C, True)
        RigaElenco(C, Titoli, Larghezze, Fh, Hr, XColors.LightGray, Nothing)
      End If
      Dim Valori As String() = {"", s.Periodo.TR.Start.ToString("HH:mm:ss"), If(s.IsStbd, "Stbd", "Port"),
                                s.Periodo.TR.Durata.TotalSeconds.ToString("F0"), Formato(s.Tws, 1), Formato(Math.Abs(s.Twa), 0),
                                Formato(s.Sow, 2), Formato(s.Perf, 1), s.Periodo.Keys.Trim}
      RigaElenco(C, Valori, Larghezze, F, Hr, If(Alt, XColors.WhiteSmoke, XColors.White), s.Periodo)
      ' pallino del colore del periodo e, a destra, la mura
      C.Gfx.DrawEllipse(New XSolidBrush(ToX(ColoreDaOutputType(s.Periodo, OutputType))), Margine + 2, C.Y - Hr + 2, 7, 7)
      Alt = Not Alt
    Next
    C.Y += 10
  End Sub

  Private Shared Sub RigaElenco(C As clsCtx, Valori As String(), Larghezze As Double(), F As XFont, H As Double, Riempimento As XColor, Periodo As clsPeriod2021)
    Dim X As Double = Margine
    For i As Integer = 0 To Valori.Length - 1
      Dim Al As XStringFormat = XStringFormats.CenterRight
      If i = 0 OrElse i = 1 OrElse i = 2 OrElse i = Valori.Length - 1 Then Al = XStringFormats.CenterLeft
      Cella(C.Gfx, Valori(i), F, X, C.Y, Larghezze(i), H, Riempimento, Al)
      X += Larghezze(i)
    Next
    C.Y += H
  End Sub

#End Region

#Region "Grafici"

  ''' <summary>Gruppo del canale in base al nome (0 performance, 1 vento e mare, 2 assetto e foil, 3 rig e vele).</summary>
  Private Shared Function GruppoCanale(Nome As String) As Integer
    Dim n As String = If(Nome, "").ToLower
    If n = "" Then Return 3
    For Each t In {"tws", "twd", "sea", "mws", "mwa", "awa", "aws", "wind", "minutes"}
      If n.Contains(t) Then Return 1
    Next
    For Each t In {"vmg", "bst", "bsp", "twa", "polar", "perf"}
      If n.Contains(t) Then Return 0
    Next
    If n = "bs" OrElse n = "sow" OrElse n = "sog" OrElse n = "cog" Then Return 0
    For Each t In {"heel", "trim", "trm", "rdr", "rudder", "keel", "dagger", "dag", "board", "canard", "lwy", "leeway", "pitch", "cant", "aoa", "rake"}
      If n.Contains(t) Then Return 2
    Next
    Return 3
  End Function

  Private Shared Function NomeGrafico(Surf As SciChartSurface) As String
    Try
      Dim Vm = TryCast(Surf.DataContext, clsStraightLineStandardPlotViewModel)
      If Vm Is Nothing Then Return ""
      If Not Vm.CanaleOrdinata Is Nothing Then Return If(Vm.CanaleOrdinata.ShortName, Vm.CanaleOrdinata.ChannelId)
      If Not Vm.CanaleAscissa Is Nothing Then Return If(Vm.CanaleAscissa.ShortName, Vm.CanaleAscissa.ChannelId)
      Return Vm.TitoloPlot
    Catch ex As Exception
      Return ""
    End Try
  End Function

  Private Sub DisegnaGrafici(C As clsCtx, ControlliAvg As List(Of SciChartSurface), ControlliDistr As List(Of SciChartSurface))
    Dim Hspazio As Double = 8
    Dim L As Double = (C.W - 2 * Margine - Hspazio * (GraficiPerRiga - 1)) / GraficiPerRiga
    ' altezza uguale su tutte le pagine: 3 righe e un'intestazione di gruppo per pagina
    Dim Utile As Double = C.H - (Margine + BandaIntestazioneRidotta) - BandaPiePagina
    Dim Himg As Double = (Utile - AltezzaGruppo - 4 - 3 * Distanza) / 3
    Dim ConDistribuzioni As Boolean = Not ControlliDistr Is Nothing AndAlso ControlliDistr.Count >= ControlliAvg.Count
    Dim Hg As Double = If(ConDistribuzioni, (Himg / 2) - 2, Himg)

    Dim Gruppo As Integer() = ControlliAvg.Select(Function(s) GruppoCanale(NomeGrafico(s))).ToArray
    For g As Integer = 0 To NomiGruppi.Length - 1
      Dim Indici As List(Of Integer) = Enumerable.Range(0, ControlliAvg.Count).Where(Function(i) Gruppo(i) = g).ToList
      If Indici.Count = 0 Then Continue For

      AssicuraSpazio(C, AltezzaGruppo + 4 + Himg)
      C.Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(235, 239, 245)), Margine, C.Y, C.W - 2 * Margine, AltezzaGruppo)
      C.Gfx.DrawString(NomiGruppi(g), New XFont("Verdana", 9, XFontStyle.Bold), XBrushes.DimGray, New XRect(Margine + 4, C.Y, C.W - 2 * Margine, AltezzaGruppo), XStringFormats.CenterLeft)
      C.Y += AltezzaGruppo + 4

      For k As Integer = 0 To Indici.Count - 1
        Dim Col As Integer = k Mod GraficiPerRiga
        If Col = 0 AndAlso k > 0 Then C.Y += Himg + Distanza
        If Col = 0 Then AssicuraSpazio(C, Himg)
        Dim X As Double = Margine + Col * (L + Hspazio)
        DisegnaImmagine(C, ControlliAvg(Indici(k)), X, C.Y, L, Hg)
        If ConDistribuzioni Then DisegnaImmagine(C, ControlliDistr(Indici(k)), X, C.Y + Hg + 4, L, Hg)
      Next
      C.Y += Himg + Distanza
    Next
    C.Y += 4
  End Sub

  Private Shared Sub DisegnaImmagine(C As clsCtx, Surf As SciChartSurface, X As Double, Y As Double, L As Double, H As Double)
    Try
      Dim Flusso = Surf.ExportToStream(SciChart.Core.ExportType.Bmp, False)
      Dim Img = XImage.FromStream(Flusso)
      C.Gfx.DrawImage(Img, New XRect(X, Y, L, H))
    Catch ex As Exception
      C.Gfx.DrawRectangle(New XPen(XColors.LightGray, 0.5), X, Y, L, H)
      C.Gfx.DrawString("chart not available", New XFont("Verdana", 7), XBrushes.Gray, New XRect(X, Y, L, H), XStringFormats.Center)
    End Try
  End Sub

#End Region

#Region "Tabella dati per TWS"

  Private Shared Function Tronca(Gfx As XGraphics, Testo As String, F As XFont, L As Double) As String
    If Gfx.MeasureString(Testo, F).Width <= L Then Return Testo
    Dim T As String = Testo
    While T.Length > 1 AndAlso Gfx.MeasureString(T & "...", F).Width > L
      T = T.Substring(0, T.Length - 1)
    End While
    Return T & "..."
  End Function

  Private Class clsVoceTabella
    Public Coppia As clsCoppieValoriTwsVsCanale
    Public Gruppo As Integer
  End Class

  Private Class clsTabella
    Public Voci As New List(Of clsVoceTabella)
    Public Primo As Integer
    Public Ultimo As Integer
    Public Hz As Integer
  End Class

  ''' <summary>Dati della tabella (canali per fascia di TWS): servono sia al pdf sia al csv. Nothing se non ci sono dati.</summary>
  Private Function CostruisciTabella(Info As clsInfo, ListaCanali As List(Of clsStraightLineChartSettings)) As clsTabella
    If ListaCanali Is Nothing Then Return Nothing
    Dim Coppie As New List(Of clsCoppieValoriTwsVsCanale)
    For Each cs In ListaCanali
      Dim Canale = DataProvider2020.Channels.Canale(cs.ChannelName)
      If Canale Is Nothing OrElse Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eTWS Then Continue For
      Dim Coppia As New clsCoppieValoriTwsVsCanale(Canale)
      For Each p In Info.Periodi
        Coppia.AccodaDati(p.TR)
      Next
      Coppie.Add(Coppia)
    Next
    If Coppie.Count = 0 Then Return Nothing

    Dim T As New clsTabella
    ' stesso raggruppamento dei grafici, ordine originale dentro il gruppo
    T.Voci = Coppie.Select(Function(cp, i) New With {.Coppia = cp, .Indice = i, .Gruppo = GruppoCanale(cp.Canale.ShortName)}).
      OrderBy(Function(x) x.Gruppo).ThenBy(Function(x) x.Indice).
      Select(Function(x) New clsVoceTabella With {.Coppia = x.Coppia, .Gruppo = x.Gruppo}).ToList

    ' colonne TWS: dal primo all'ultimo valore presente
    T.Primo = Integer.MaxValue
    T.Ultimo = Integer.MinValue
    For t0 As Integer = 0 To 40
      If Coppie.Any(Function(cp) cp.Conteggio(t0 - 0.5, t0 + 0.5) > 0) Then
        T.Primo = Math.Min(T.Primo, t0)
        T.Ultimo = Math.Max(T.Ultimo, t0)
      End If
    Next
    If T.Primo > T.Ultimo Then Return Nothing
    T.Hz = Math.Max(1, DataProvider2020.Hz)
    Return T
  End Function

  Private Sub DisegnaTabellaDati(C As clsCtx, T As clsTabella)
    Dim Primo As Integer = T.Primo
    Dim NumBin As Integer = T.Ultimo - T.Primo + 1
    Dim Hz As Integer = T.Hz
    Dim Fh As New XFont("Tahoma", 7.5, XFontStyle.Bold)
    Dim F As New XFont("Tahoma", 7)
    Dim Fb As New XFont("Tahoma", 7.5, XFontStyle.Bold)
    Dim Hr As Double = 10
    Dim Col1 As Double = 124
    Dim Lb As Double = Math.Min(48, (C.W - 2 * Margine - Col1) / NumBin)

    Dim Intestazione As Action =
      Sub()
        Cella(C.Gfx, "Channel / TWS", Fh, Margine, C.Y, Col1, Hr + 2, XColors.LightGray, XStringFormats.CenterLeft)
        For b As Integer = 0 To NumBin - 1
          Cella(C.Gfx, (Primo + b).ToString, Fh, Margine + Col1 + b * Lb, C.Y, Lb, Hr + 2, XColors.LightGray, XStringFormats.Center)
        Next
        C.Y += Hr + 2
      End Sub

    NuovaPagina(C, True)
    C.Gfx.DrawString("Data Table", New XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 14), XStringFormats.CenterLeft)
    C.Y += 16
    Intestazione()

    ' minuti di navigazione per fascia
    Dim Prima = T.Voci.First.Coppia
    Cella(C.Gfx, "Time (min)", Fb, Margine, C.Y, Col1, Hr, XColors.LightCyan, XStringFormats.CenterLeft)
    For b As Integer = 0 To NumBin - 1
      Dim N As Integer = Prima.Conteggio(Primo + b - 0.5, Primo + b + 0.5)
      Cella(C.Gfx, If(N = 0, "", (N / Hz / 60.0).ToString("F1")), Fb, Margine + Col1 + b * Lb, C.Y, Lb, Hr, XColors.LightCyan, XStringFormats.CenterRight)
    Next
    C.Y += Hr + 3

    Dim GruppoCorrente As Integer = -1
    For Each Voce In T.Voci
      Dim Necessario As Double = Hr * 4 + 3 + If(Voce.Gruppo <> GruppoCorrente, Hr + 3, 0)
      If C.Y + Necessario > C.H - BandaPiePagina Then
        NuovaPagina(C, True)
        Intestazione()
        GruppoCorrente = -1
        Necessario = Hr * 4 + 3 + Hr + 3
      End If
      If Voce.Gruppo <> GruppoCorrente Then
        GruppoCorrente = Voce.Gruppo
        Cella(C.Gfx, NomiGruppi(GruppoCorrente), New XFont("Tahoma", 7.5, XFontStyle.Bold), Margine, C.Y, C.W - 2 * Margine, Hr, XColor.FromArgb(220, 228, 240), XStringFormats.CenterLeft)
        C.Y += Hr + 3
      End If

      Dim Canale = Voce.Coppia.Canale
      Dim Decimali As Integer = Canale.Decimals
      Dim Nome As String = Canale.LongName
      If Not String.IsNullOrWhiteSpace(Canale.ShortUM) Then Nome &= " (" & Canale.ShortUM & ")"
      Nome = Tronca(C.Gfx, Nome, Fb, Col1 - 4)
      Cella(C.Gfx, Nome, Fb, Margine, C.Y, Col1, Hr, XColors.LightYellow, XStringFormats.CenterLeft)
      Cella(C.Gfx, "  Max", F, Margine, C.Y + Hr, Col1, Hr, XColors.White, XStringFormats.CenterLeft)
      Cella(C.Gfx, "  Min", F, Margine, C.Y + 2 * Hr, Col1, Hr, XColors.WhiteSmoke, XStringFormats.CenterLeft)
      Cella(C.Gfx, "  Sd", F, Margine, C.Y + 3 * Hr, Col1, Hr, XColors.White, XStringFormats.CenterLeft)
      For b As Integer = 0 To NumBin - 1
        Dim Tws As Integer = Primo + b
        Dim X As Double = Margine + Col1 + b * Lb
        Dim N As Integer = Voce.Coppia.Conteggio(Tws - 0.5, Tws + 0.5)
        Dim V As clsValoriBase = If(N = 0, Nothing, Voce.Coppia.Valori(Tws - 0.5, Tws + 0.5))
        Dim Fmt As String = "F" & Decimali.ToString
        ' fascia senza dati: celle vuote, non zero
        Cella(C.Gfx, If(V Is Nothing, "", V.Avg.ToString(Fmt)), Fb, X, C.Y, Lb, Hr, XColors.LightYellow, XStringFormats.CenterRight)
        Cella(C.Gfx, If(V Is Nothing, "", V.Max.ToString(Fmt)), F, X, C.Y + Hr, Lb, Hr, XColors.White, XStringFormats.CenterRight)
        Cella(C.Gfx, If(V Is Nothing, "", V.Min.ToString(Fmt)), F, X, C.Y + 2 * Hr, Lb, Hr, XColors.WhiteSmoke, XStringFormats.CenterRight)
        Cella(C.Gfx, If(V Is Nothing, "", V.Ds.ToString(Fmt)), F, X, C.Y + 3 * Hr, Lb, Hr, XColors.White, XStringFormats.CenterRight)
      Next
      C.Y += Hr * 4 + 3
    Next
  End Sub

  Private Shared Function CampoCsv(Testo As String) As String
    If Testo Is Nothing Then Return ""
    If Testo.Contains(";") OrElse Testo.Contains("""") OrElse Testo.Contains(vbLf) Then Return """" & Testo.Replace("""", """""") & """"
    Return Testo
  End Function

  ''' <summary>
  ''' Csv con la tabella completa: una riga per canale e statistica (Avg, Max, Min, Sd) e una colonna per fascia di TWS,
  ''' piu' la colonna All su tutti i campioni. Separatore ';' e numeri nel formato del computer, come nel pdf, cosi' si apre in Excel.
  ''' </summary>
  Private Sub ScriviCsv(T As clsTabella, Percorso As String)
    Dim Righe As New List(Of String)
    Dim Intestazione As New List(Of String) From {"Group", "Channel", "Unit", "Stat"}
    For tws As Integer = T.Primo To T.Ultimo
      Intestazione.Add("TWS " & tws)
    Next
    Intestazione.Add("All")
    Righe.Add(String.Join(";", Intestazione))

    Dim Prima = T.Voci.First.Coppia
    Dim Campioni As New List(Of String) From {"", "Samples (n)", "", ""}
    Dim Minuti As New List(Of String) From {"", "Time (min)", "", ""}
    For tws As Integer = T.Primo To T.Ultimo
      Dim N As Integer = Prima.Conteggio(tws - 0.5, tws + 0.5)
      Campioni.Add(If(N = 0, "", N.ToString))
      Minuti.Add(If(N = 0, "", (N / T.Hz / 60.0).ToString("F1")))
    Next
    Dim Tot As Integer = Prima.Conteggio(-1000, 1000)
    Campioni.Add(Tot.ToString)
    Minuti.Add((Tot / T.Hz / 60.0).ToString("F1"))
    Righe.Add(String.Join(";", Campioni))
    Righe.Add(String.Join(";", Minuti))

    Dim Nomi As String() = {"Avg", "Max", "Min", "Sd"}
    For Each Voce In T.Voci
      Dim Canale = Voce.Coppia.Canale
      Dim Fmt As String = "F" & Canale.Decimals.ToString
      Dim Valori As New List(Of clsValoriBase)
      For tws As Integer = T.Primo To T.Ultimo
        Valori.Add(If(Voce.Coppia.Conteggio(tws - 0.5, tws + 0.5) = 0, Nothing, Voce.Coppia.Valori(tws - 0.5, tws + 0.5)))
      Next
      Valori.Add(If(Voce.Coppia.Conteggio(-1000, 1000) = 0, Nothing, Voce.Coppia.Valori(-1000, 1000)))
      For k As Integer = 0 To 3
        Dim Riga As New List(Of String) From {CampoCsv(NomiGruppi(Voce.Gruppo)), CampoCsv(Canale.LongName), CampoCsv(If(Canale.ShortUM, "")), Nomi(k)}
        For Each v In Valori
          If v Is Nothing Then
            Riga.Add("")
          Else
            Dim x As Double = If(k = 0, v.Avg, If(k = 1, v.Max, If(k = 2, v.Min, v.Ds)))
            Riga.Add(x.ToString(Fmt))
          End If
        Next
        Righe.Add(String.Join(";", Riga))
      Next
    Next
    System.IO.File.WriteAllText(Percorso, String.Join(vbCrLf, Righe) & vbCrLf, New System.Text.UTF8Encoding(True))
  End Sub


#End Region

End Class
