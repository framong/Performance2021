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
Partial Public Class clsStraightLineReport

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
    Public Orizzontale As Boolean
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
    ''' <summary>Periodi spuntati lasciati fuori perche' nessun loro campione passa il filtro XY (come nei grafici a video).</summary>
    Public EsclusiDalFiltro As Integer
    ''' <summary>Statistiche dei periodi, calcolate una sola volta (riepilogo, tabelle per TWA ed elenco periodi le riusano).</summary>
    Public Stat As List(Of clsStatPeriodo)
  End Class

  Private Class clsStatPeriodo
    Public Periodo As clsPeriod2021
    ''' <summary>Campioni del canale di performance nel periodo: servono per i percentili sull'insieme dei periodi.</summary>
    Public Campioni As Double()
    Public Tws As Double = Double.NaN
    Public Twa As Double = Double.NaN
    Public Sow As Double = Double.NaN
    Public Perf As Double = Double.NaN
    ''' <summary>Minimo e massimo dei campioni del canale di performance nel periodo (NaN se non disponibili).</summary>
    Public PerfMin As Double = Double.NaN
    Public PerfMax As Double = Double.NaN
    Public Minuti As Double
    Public IsStbd As Boolean
  End Class

  Private ReadOnly NomiGruppi As String() = {"Performance", "Wind & sea", "Attitude, foils & leeway", "Rig & sails"}

  ' impostati all'inizio di ogni report (thread UI, un report alla volta): banda dei percentili per Min e Max dei valori
  ' e finestra di avanzamento. I grafici mostrano sempre tutti i dati.
  Private Shared _Banda As Double = 90
  ''' <summary>Filtro sui campioni del tab XY Plots (Nothing = nessuno) e sua descrizione: li imposta il chiamante prima del report, gli stessi dei grafici a video.</summary>
  Public Shared FiltroMaschera As Func(Of Integer, Boolean) = Nothing
  Public Shared FiltroDescrizione As String = ""
  Private Shared _Av As FinestraAvanzamento = Nothing
  Private Shared _ImgFatte As Integer = 0
  Private Shared _ImgTot As Integer = 0

  ''' <summary>Nota da stampare sotto le tabelle con Min e Max: spiega che sono percentili.</summary>
  Private Shared Function NotaBanda() As String
    If _Banda >= 100 Then Return "Min and Max are the true minimum and maximum of the samples."
    Dim Basso As Double = (100 - _Banda) / 2
    Return "Min and Max are the P" & Basso.ToString("0.##") & " and P" & (100 - Basso).ToString("0.##") & " percentiles of the samples (" & _Banda.ToString("0.##") & "% central band). Charts show all the data."
  End Function

  Private Shared Sub ImpostaReport(Opzioni As clsStraightLineReportOptions, Av As FinestraAvanzamento)
    _Banda = If(Opzioni.RangeBandPercent <= 0 OrElse Opzioni.RangeBandPercent > 100, 100, Opzioni.RangeBandPercent)
    _Av = Av
    _ImgFatte = 0
    _ImgTot = 0
  End Sub

  Private Shared Sub Fase(Titolo As String, Da As Double, A As Double)
    If Not _Av Is Nothing Then _Av.Fase(Titolo, Da, A)
  End Sub

  Private Shared Sub Passo(i As Integer, n As Integer, Dettaglio As String)
    If Not _Av Is Nothing Then _Av.Passo(i, n, Dettaglio)
  End Sub

  ''' <summary>Statistiche dei periodi: una sola volta per report, con avanzamento (ogni periodo rilegge i canali).</summary>
  Private Shared Function StatInfo(Info As clsInfo) As List(Of clsStatPeriodo)
    If Info.Stat Is Nothing Then
      Fase("Computing the statistics of the periods", 5, 25)
      Info.Stat = New List(Of clsStatPeriodo)
      For i As Integer = 0 To Info.Periodi.Count - 1
        Passo(i, Info.Periodi.Count, "")
        Info.Stat.Add(StatPeriodo(Info.Periodi(i), Info.IsReaching))
      Next
    End If
    Return Info.Stat
  End Function

  ''' <summary>
  ''' Min e Max della performance sull'insieme dei campioni dei periodi indicati (percentili della banda scelta, o minimo e massimo
  ''' veri con banda 100). Se i campioni non ci sono ripiega sui min e max salvati per periodo. False se non ci sono dati.
  ''' </summary>
  Private Shared Function IntervalloPerf(Stat As IEnumerable(Of clsStatPeriodo), ByRef Minimo As Double, ByRef Massimo As Double) As Boolean
    Dim Tutti As New List(Of Double)
    For Each s In Stat
      If Not s.Campioni Is Nothing Then Tutti.AddRange(s.Campioni.Where(Function(x) Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)))
    Next
    If Tutti.Count > 0 Then
      If _Banda >= 100 Then
        Minimo = Tutti.Min
        Massimo = Tutti.Max
      Else
        Dim Ordinati As Double() = Tutti.OrderBy(Function(x) x).ToArray
        Dim Basso As Double = (100 - _Banda) / 2
        Minimo = clsValoriBase.Percentile(Ordinati, Basso)
        Massimo = clsValoriBase.Percentile(Ordinati, 100 - Basso)
      End If
      Return True
    End If
    Dim Valide = Stat.Where(Function(s) Not Double.IsNaN(s.PerfMin) AndAlso Not Double.IsNaN(s.PerfMax)).ToList
    If Valide.Count = 0 Then Return False
    Minimo = Valide.Min(Function(s) s.PerfMin)
    Massimo = Valide.Max(Function(s) s.PerfMax)
    Return True
  End Function

#Region "Punti di ingresso"

  ''' <summary>
  ''' Crea il report secondo le opzioni: pagina 1 con intestazione e riepilogo, poi grafici, tabella dati e,
  ''' ultimo, l'elenco dei periodi. Con la sola opzione csv non crea il pdf.
  ''' </summary>
  Public Sub CreaReport(ControlliAvg As List(Of SciChartSurface), ControlliDistr As List(Of SciChartSurface), OutputType As clsStraightLineVM2020.eOutputType, ListaPeriodi As List(Of clsPeriod2021), ListaCanali As List(Of clsStraightLineChartSettings), Filtro As String, Opzioni As clsStraightLineReportOptions, Optional Av As FinestraAvanzamento = Nothing)
    Dim ConPdf As Boolean = Opzioni.PrintCharts OrElse Opzioni.PrintTable OrElse Opzioni.PrintPeriods
    If Not ConPdf AndAlso Not Opzioni.CreateTableCsv Then
      MsgBox("Nothing to create: tick at least one option under Make a pdf.", MsgBoxStyle.Information, "Straight Line report")
      Exit Sub
    End If
    ImpostaReport(Opzioni, Av)
    Dim Info As clsInfo = CreaInfo(ListaPeriodi, OutputType, Filtro, "")
    If Info Is Nothing Then Exit Sub

    StatInfo(Info)
    Dim Tabella As clsTabella = Nothing
    If Opzioni.PrintTable OrElse Opzioni.CreateTableCsv Then Tabella = CostruisciTabella(Info, ListaCanali)
    If Opzioni.PrintCharts AndAlso Not ControlliAvg Is Nothing Then _ImgTot = ControlliAvg.Count + If(ControlliDistr Is Nothing, 0, ControlliDistr.Count)

    Dim PercorsoPdf As String = PercorsoLibero(Info, OutputType, ".pdf")
    If ConPdf Then
      Dim C As New clsCtx With {.Doc = New PdfDocument, .TitoloBreve = Info.TitoloBreve}
      NuovaPagina(C, False)
      DisegnaIntestazione(C, Info, OutputType)
      DisegnaRiepilogo(C, Info)
      If Info.IsReaching Then DisegnaRiepilogoPerTwa(C, Info, Opzioni.TwaBinDegrees)
      If Opzioni.PrintCharts AndAlso Not ControlliAvg Is Nothing AndAlso ControlliAvg.Count > 0 Then DisegnaGrafici(C, ControlliAvg, ControlliDistr)
      If Opzioni.PrintTable AndAlso Not Tabella Is Nothing Then DisegnaTabellaDati(C, Tabella)
      If Opzioni.PrintPeriods Then DisegnaElencoPeriodi(C, Info, OutputType)
      Fase("Saving the pdf", 90, 100)
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
    Dim chTws As clsChannel2020 = If(FiltroMaschera Is Nothing, Nothing, DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS))
    For Each StLn In ListaPeriodi
      If Not StLn.IsChecked Then Continue For
      Dim TwsPeriodo As Double = StLn.TwsDetails.AvgVal
      If Not FiltroMaschera Is Nothing Then
        ' come nei grafici a video: un periodo senza campioni che passano il filtro XY non entra nel report
        If MinutiPeriodo(StLn) <= 0 Then
          Info.EsclusiDalFiltro += 1
          Continue For
        End If
        ' Tws del titolo calcolato sugli stessi campioni filtrati
        TwsPeriodo = MediaCanale(chTws, StLn)
      End If
      Info.Periodi.Add(StLn)
      If Inizio = Nothing OrElse StLn.TR.Start < Inizio Then Inizio = StLn.TR.Start
      If Fine = Nothing OrElse StLn.TR.Finish > Fine Then Fine = StLn.TR.Finish
      If Not StLn.Keys.Trim = "" AndAlso Not Info.Chiavi.ContainsKey(StLn.Keys.Trim) Then
        Info.Chiavi.Add(StLn.Keys.Trim, ColoreDaOutputType(StLn, OutputType))
      End If
      If Not Double.IsNaN(TwsPeriodo) Then Tws.Add(TwsPeriodo)
      If Math.Abs(StLn.AvgTwa) < 90 Then Up += 1 Else Dn += 1
      If StLn.IsStbd Then Info.Dritta += 1 Else Info.Porta += 1
      If Not StLn.StraightLineReachingDetails Is Nothing AndAlso StLn.StraightLineVmgDetails Is Nothing Then Info.IsReaching = True
    Next
    If Info.Periodi.Count = 0 Then
      If Info.EsclusiDalFiltro > 0 Then
        MsgBox("None of the checked periods has samples that pass the XY Plots filter:" & vbCrLf & FiltroDescrizione, MsgBoxStyle.Exclamation, "Straight Line report")
      Else
        MsgBox("No periods are checked: check the periods to include in the report.", MsgBoxStyle.Exclamation, "Straight Line report")
      End If
      Return Nothing
    End If

    Info.Tipo = "Straight Line"
    If Dn = 0 Then
      Info.Tipo &= " Upwind"
    ElseIf Up = 0 Then
      Info.Tipo &= " Downwind"
    End If
    Info.TwsMin = If(Tws.Count = 0, 0, Tws.Min)
    Info.TwsMax = If(Tws.Count = 0, 0, Tws.Max)
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

  ''' <summary>Minuti del periodo che passano il filtro XY (durata intera senza filtro), come nei grafici a video.</summary>
  Private Shared Function MinutiPeriodo(p As clsPeriod2021) As Double
    Return clsFiltroCampioniXY.MinutiValidi(p.TR, FiltroMaschera)
  End Function

  ''' <summary>Riga del filtro XY per l'intestazione (vuota con l'opzione spenta), con i periodi esclusi.</summary>
  Private Shared Function TestoFiltro(Info As clsInfo) As String
    If FiltroDescrizione = "" Then Return ""
    Dim T As String = FiltroDescrizione
    If Info.EsclusiDalFiltro > 0 Then T &= " " & Info.EsclusiDalFiltro & " checked period(s) without samples passing the filter left out."
    Return T
  End Function

  ''' <summary>Media del canale nel periodo, con lo stesso calcolo dei punti dei grafici.</summary>
  Private Shared Function MediaCanale(Canale As clsChannel2020, p As clsPeriod2021) As Double
    Dim M = ValoriCanale(Canale, p)
    Return If(M Is Nothing, Double.NaN, M.Avg)
  End Function

  ''' <summary>Statistiche (avg, min, max) del canale nel periodo; Nothing se il canale non ha dati.</summary>
  Private Shared Function ValoriCanale(Canale As clsChannel2020, p As clsPeriod2021) As clsValoriPeriodoCanale2020
    Dim Campioni As Double() = Nothing
    Return ValoriCanale(Canale, p, Campioni)
  End Function

  ''' <summary>Come sopra, restituendo anche i campioni validi del periodo (per i percentili).</summary>
  Private Shared Function ValoriCanale(Canale As clsChannel2020, p As clsPeriod2021, ByRef Campioni As Double()) As clsValoriPeriodoCanale2020
    Campioni = Nothing
    If Canale Is Nothing OrElse Canale.Valori Is Nothing OrElse Canale.Valori.Count = 0 Then Return Nothing
    Dim Assoluto As Boolean = Canale.DataType = clsChannel2020.eDataType.e180
    Dim M As New clsValoriPeriodoCanale2020(Canale, p.TR, Assoluto)
    M.Maschera = FiltroMaschera
    Campioni = M.AggiornaValori(Assoluto)
    If Not FiltroMaschera Is Nothing AndAlso Not M.HaDati Then
      ' tutti i campioni del periodo scartati dal filtro
      Campioni = Nothing
      Return Nothing
    End If
    Return M
  End Function

  ''' <summary>
  ''' Valori del periodo per tabelle e elenco. Si calcolano dai canali attuali come i punti dei grafici: i dettagli salvati nel
  ''' file dei periodi sono stati calcolati alla creazione e possono differire se canali, target o correzioni sono cambiati.
  ''' </summary>
  Private Shared Function StatPeriodo(p As clsPeriod2021, IsReaching As Boolean) As clsStatPeriodo
    Dim S As New clsStatPeriodo With {.Periodo = p, .IsStbd = p.IsStbd, .Minuti = MinutiPeriodo(p)}
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chSow = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chPerf = DataProvider2020.CanaleDbl(If(IsReaching, clsChannels2020.eCanaliChiave.eBSPp, clsChannels2020.eCanaliChiave.eVMGp))
    If Not chTws Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chSow Is Nothing AndAlso Not chPerf Is Nothing Then
      S.Tws = MediaCanale(chTws, p)
      S.Twa = MediaCanale(chTwa, p)
      S.Sow = MediaCanale(chSow, p)
      Dim Campioni As Double() = Nothing
      Dim Vp = ValoriCanale(chPerf, p, Campioni)
      If Not Vp Is Nothing Then
        S.Campioni = Campioni
        S.Perf = Vp.Avg
        S.PerfMin = Vp.Min
        S.PerfMax = Vp.Max
      End If
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

  Private Shared Sub NuovaPagina(C As clsCtx, ConIntestazione As Boolean, Optional Orizzontale As Boolean = False)
    ' PdfSharp scrive il contenuto alla chiusura e non ammette due XGraphics aperti sulla stessa pagina
    If Not C.Gfx Is Nothing Then
      C.Gfx.Dispose()
      C.Gfx = Nothing
    End If
    C.Pagina = C.Doc.AddPage
    C.Orizzontale = Orizzontale
    If Orizzontale AndAlso C.Pagina.Width.Point < C.Pagina.Height.Point Then
      Dim Largo As XUnit = C.Pagina.Height
      C.Pagina.Height = C.Pagina.Width
      C.Pagina.Width = Largo
    End If
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
    If C.Y + Altezza > C.H - BandaPiePagina Then NuovaPagina(C, True, C.Orizzontale)
  End Sub

  Private Sub DisegnaIntestazione(C As clsCtx, Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType)
    C.Gfx.DrawString(Info.Titolo, New XFont("Verdana", 15, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 22), XStringFormats.CenterLeft)
    C.Y += 24
    Dim Minuti As Double = Info.Periodi.Sum(Function(p) MinutiPeriodo(p))
    Dim Sotto As String = Info.Periodi.Count & " periods, " & Minuti.ToString("F1") & " min"
    Dim Nome As String = NomeFileDati()
    If Nome <> "" Then Sotto &= "   |   " & Nome
    C.Gfx.DrawString(Sotto, New XFont("Verdana", 8), XBrushes.Gray, New XRect(Margine, C.Y, C.W - 2 * Margine, 12), XStringFormats.CenterLeft)
    C.Y += 16
    ' filtro XY: sempre scritto quando l'opzione e' attiva (anche se non filtra nulla o ignora un canale), a capo se non sta in una riga
    Dim TF As String = TestoFiltro(Info)
    If TF <> "" Then
      Dim Ff As New XFont("Verdana", 8, XFontStyle.Bold)
      Dim Riga As String = ""
      For Each Parola In TF.Split(" "c)
        Dim Prova As String = If(Riga = "", Parola, Riga & " " & Parola)
        If Riga <> "" AndAlso C.Gfx.MeasureString(Prova, Ff).Width > C.W - 2 * Margine Then
          C.Gfx.DrawString(Riga, Ff, XBrushes.DarkRed, New XRect(Margine, C.Y, C.W - 2 * Margine, 12), XStringFormats.CenterLeft)
          C.Y += 12
          Riga = Parola
        Else
          Riga = Prova
        End If
      Next
      C.Gfx.DrawString(Riga, Ff, XBrushes.DarkRed, New XRect(Margine, C.Y, C.W - 2 * Margine, 12), XStringFormats.CenterLeft)
      C.Y += 16
    End If
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
      If Not System.IO.File.Exists(Base & Suffisso & ".pdf") AndAlso Not System.IO.File.Exists(Base & Suffisso & ".csv") AndAlso Not System.IO.File.Exists(Base & Suffisso & ".html") Then
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
    ' griglia sottile
    Gfx.DrawRectangle(New XPen(XColor.FromArgb(205, 205, 205), 0.4), X, Y, L, H)
    If Testo Is Nothing OrElse Testo = "" Then Exit Sub
    Gfx.DrawString(Testo, F, XBrushes.Black, New XRect(X + 2, Y, L - 4, H), Allineamento)
  End Sub

  Private Sub DisegnaRiepilogo(C As clsCtx, Info As clsInfo)
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info)
    Dim Perf As String = If(Info.IsReaching, "Polar %", "Vmg %")
    Dim Fh As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim F As New XFont("Verdana", 7.5)
    Dim Fb As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim Hr As Double = 12
    Dim Larghezze As Double() = {70, 70, 110, 60, 60, 70, 90}
    Dim Totale As Double = Larghezze.Sum
    Dim Scala As Double = (C.W - 2 * Margine) / Totale
    For i As Integer = 0 To Larghezze.Length - 1
      Larghezze(i) *= Scala
    Next
    Dim Titoli As String() = {"TWS", Perf & " avg", Perf & " min - max", "Bs (kn)", "TWA (deg)", "Port/Stbd", "Duration"}

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
    DisegnaNota(C)
    C.Y += 8
  End Sub

  ''' <summary>Nota in carattere piccolo sotto una tabella: Min e Max sono percentili.</summary>
  Private Shared Sub DisegnaNota(C As clsCtx)
    AssicuraSpazio(C, 12)
    C.Gfx.DrawString(NotaBanda(), New XFont("Verdana", 6.5), XBrushes.Gray, New XRect(Margine, C.Y + 1, C.W - 2 * Margine, 10), XStringFormats.CenterLeft)
    C.Y += 11
  End Sub

  Private Shared Function RigaRiepilogo(Etichetta As String, Stat As List(Of clsStatPeriodo)) As String()
    If Stat.Count = 0 Then Return {Etichetta, "-", "-", "-", "-", "0/0", FormatoDurata(0)}
    Dim Porta As Integer = Stat.Where(Function(s) Not s.IsStbd).Count
    Dim Dritta As Integer = Stat.Where(Function(s) s.IsStbd).Count
    Dim Minuti As Double = Stat.Sum(Function(s) s.Minuti)
    Dim Valide = Stat.Where(Function(s) Not Double.IsNaN(s.Perf)).ToList
    Dim Intervallo As String = "-"
    Dim Lo, Hi As Double
    If IntervalloPerf(Stat, Lo, Hi) Then Intervallo = Lo.ToString("F1") & " - " & Hi.ToString("F1")
    Return {Etichetta,
            Formato(MediaPesata(Stat, Function(s) s.Perf), 1), Intervallo,
            Formato(MediaPesata(Stat, Function(s) s.Sow), 2), Formato(MediaPesata(Stat, Function(s) Math.Abs(s.Twa)), 1),
            Porta & "/" & Dritta, FormatoDurata(Minuti * 60)}
  End Function

  ''' <summary>Durata come dd HH:mm:ss: giorni e ore compaiono solo se servono, mm:ss sempre.</summary>
  Private Shared Function FormatoDurata(Secondi As Double) As String
    Dim T As TimeSpan = TimeSpan.FromSeconds(Math.Round(Secondi))
    If T.Days > 0 Then Return T.Days.ToString("00") & " " & T.Hours.ToString("00") & ":" & T.Minutes.ToString("00") & ":" & T.Seconds.ToString("00")
    If T.Hours > 0 Then Return T.Hours.ToString("00") & ":" & T.Minutes.ToString("00") & ":" & T.Seconds.ToString("00")
    Return T.Minutes.ToString("00") & ":" & T.Seconds.ToString("00")
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

#Region "Riepilogo per TWS e TWA (reaching)"

  ''' <summary>
  ''' Solo reaching: Polar % medio e durata per TWS (righe) e fasce di TWA (colonne), una tabella per mura.
  ''' Una tabella senza dati non viene stampata.
  ''' </summary>
  Private Sub DisegnaRiepilogoPerTwa(C As clsCtx, Info As clsInfo, AmpiezzaTwa As Integer)
    Dim Passo As Integer = If(AmpiezzaTwa <= 0, 20, Math.Max(5, Math.Min(90, AmpiezzaTwa)))
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info).
      Where(Function(s) Not Double.IsNaN(s.Tws) AndAlso Not Double.IsNaN(s.Twa)).ToList
    If Stat.Count = 0 Then Exit Sub
    ' molte colonne (fasce di TWA): sezione su pagine orizzontali, a partire da una pagina nuova
    NuovaPagina(C, True, True)
    DisegnaTabellaTwa(C, "Port", Stat.Where(Function(s) Not s.IsStbd).ToList, Passo, XColors.MistyRose)
    DisegnaTabellaTwa(C, "Stbd", Stat.Where(Function(s) s.IsStbd).ToList, Passo, XColors.Honeydew)
  End Sub

  Private Shared Sub DisegnaTabellaTwa(C As clsCtx, Mura As String, Stat As List(Of clsStatPeriodo), Passo As Integer, ColoreMura As XColor)
    If Stat.Count = 0 Then Exit Sub
    Dim Fascia As Func(Of clsStatPeriodo, Integer) = Function(s) CInt(Math.Floor(Math.Abs(s.Twa) / Passo))
    Dim Primo As Integer = Stat.Min(Fascia)
    Dim Ultimo As Integer = Stat.Max(Fascia)
    Dim NumBin As Integer = Ultimo - Primo + 1
    Dim Fh As New XFont("Verdana", 7.5, XFontStyle.Bold)
    Dim F As New XFont("Verdana", 7.5)
    Dim Fs As New XFont("Verdana", 6.5)
    Dim Fd As New XFont("Verdana", 5.5)
    Dim Hr As Double = 24
    Dim Col1 As Double = 50
    Dim ColAll As Double = 54
    Dim Lb As Double = Math.Min(64, (C.W - 2 * Margine - Col1 - ColAll) / NumBin)

    Dim Intestazione As Action =
      Sub()
        Cella(C.Gfx, "TWS \ TWA", Fh, Margine, C.Y, Col1, 14, XColors.LightGray, XStringFormats.CenterLeft)
        For b As Integer = 0 To NumBin - 1
          Cella(C.Gfx, ((Primo + b) * Passo) & "-" & ((Primo + b + 1) * Passo), Fh, Margine + Col1 + b * Lb, C.Y, Lb, 14, XColors.LightGray, XStringFormats.Center)
        Next
        Cella(C.Gfx, "All", Fh, Margine + Col1 + NumBin * Lb, C.Y, ColAll, 14, XColors.LightGray, XStringFormats.Center)
        C.Y += 14
      End Sub

    Dim Riga As Action(Of String, List(Of clsStatPeriodo), XFont, XColor) =
      Sub(Etichetta, Gruppo, Fnt, Sfondo)
        Cella(C.Gfx, Etichetta, Fnt, Margine, C.Y, Col1, Hr, Sfondo, XStringFormats.CenterLeft)
        For b As Integer = 0 To NumBin - 1
          Dim Sub1 = Gruppo.Where(Function(s) Fascia(s) = Primo + b).ToList
          CellaPerf(C, Sub1, Fnt, Fs, Fd, Margine + Col1 + b * Lb, Lb, Hr, Sfondo)
        Next
        CellaPerf(C, Gruppo, Fnt, Fs, Fd,Margine + Col1 + NumBin * Lb, ColAll, Hr, Sfondo)
        C.Y += Hr
      End Sub

    AssicuraSpazio(C, 16 + 14 + Hr * 3)
    C.Gfx.DrawString("Polar % by TWS and TWA - " & Mura & " tack (avg (duration), min - max below)", New XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 14), XStringFormats.CenterLeft)
    C.Y += 16
    Intestazione()
    Dim Alt As Boolean = False
    For Each G In Stat.GroupBy(Function(s) CInt(Math.Round(s.Tws))).OrderBy(Function(g2) g2.Key)
      If C.Y + Hr > C.H - BandaPiePagina Then
        NuovaPagina(C, True, C.Orizzontale)
        Intestazione()
      End If
      Riga(G.Key.ToString, G.ToList, F, If(Alt, XColors.WhiteSmoke, XColors.White))
      Alt = Not Alt
    Next
    If C.Y + Hr > C.H - BandaPiePagina Then
      NuovaPagina(C, True, C.Orizzontale)
      Intestazione()
    End If
    Riga("All", Stat, Fh, ColoreMura)
    DisegnaNota(C)
    C.Y += 8
  End Sub

  ''' <summary>Cella con Polar % medio (pesato sui minuti) e, sotto, la durata. Vuota se non ci sono periodi.</summary>
  Private Shared Sub CellaPerf(C As clsCtx, Stat As List(Of clsStatPeriodo), F As XFont, Fs As XFont, Fd As XFont, X As Double, L As Double, H As Double, Sfondo As XColor)
    Cella(C.Gfx, "", F, X, C.Y, L, H, Sfondo, XStringFormats.Center)
    If Stat.Count = 0 Then Exit Sub
    ' prima riga: avg e, accanto, la durata tra parentesi in carattere piccolo (gruppo centrato)
    Dim Avg As String = Formato(MediaPesata(Stat, Function(s) s.Perf), 1)
    Dim Durata As String = " (" & FormatoDurata(Stat.Sum(Function(s) s.Minuti) * 60) & ")"
    Dim Lo, Hi As Double
    Dim ConIntervallo As Boolean = IntervalloPerf(Stat, Lo, Hi)
    Dim Intervallo As String = If(ConIntervallo, Lo.ToString("F1") & " - " & Hi.ToString("F1"), "")
    ' cella stretta (molte fasce di TWA): i caratteri si riducono in proporzione finche' avg+durata e min-max entrano
    Dim Fattore As Double = 1
    While Fattore > 0.5 AndAlso
          (C.Gfx.MeasureString(Avg, F).Width + C.Gfx.MeasureString(Durata, Fd).Width > L - 4 OrElse
           (ConIntervallo AndAlso C.Gfx.MeasureString(Intervallo, Fs).Width > L - 4))
      Fattore -= 0.05
      F = New XFont(F.Name, F.Size * 0.95, F.Style)
      Fs = New XFont(Fs.Name, Fs.Size * 0.95, Fs.Style)
      Fd = New XFont(Fd.Name, Fd.Size * 0.95, Fd.Style)
    End While
    Dim La As Double = C.Gfx.MeasureString(Avg, F).Width
    Dim Ld As Double = C.Gfx.MeasureString(Durata, Fd).Width
    Dim X0 As Double = X + (L - La - Ld) / 2
    Dim Yr As Double = C.Y + H * 0.28
    C.Gfx.DrawString(Avg, F, XBrushes.Black, X0, Yr + 4)
    C.Gfx.DrawString(Durata, Fd, XBrushes.Gray, X0 + La, Yr + 4)
    ' min e max sono quelli dei campioni dentro i periodi, non delle medie dei periodi
    If ConIntervallo Then
      C.Gfx.DrawString(Intervallo, Fs, XBrushes.DimGray, New XRect(X, C.Y + H * 0.5, L, H * 0.45), XStringFormats.Center)
    End If
  End Sub

#End Region

#Region "Elenco periodi"

  Private Sub DisegnaElencoPeriodi(C As clsCtx, Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType)
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info).
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
                                (s.Minuti * 60).ToString("F0"), Formato(s.Tws, 1), Formato(Math.Abs(s.Twa), 0),
                                Formato(s.Sow, 2), Formato(s.Perf, 1), s.Periodo.Keys.Trim}
      RigaElenco(C, Valori, Larghezze, F, Hr, If(Alt, XColors.WhiteSmoke, XColors.White), s.Periodo)
      ' pallino del colore del periodo e, a destra, la mura
      C.Gfx.DrawEllipse(New XSolidBrush(ToX(ColoreDaOutputType(s.Periodo, OutputType, FiltroMaschera))), Margine + 2, C.Y - Hr + 2, 7, 7)
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
    ' la sezione grafici parte sempre da una pagina nuova (verticale)
    NuovaPagina(C, True)
    Dim Hspazio As Double = 8
    Dim L As Double = (C.W - 2 * Margine - Hspazio * (GraficiPerRiga - 1)) / GraficiPerRiga
    ' altezza uguale su tutte le pagine: 3 righe e un'intestazione di gruppo per pagina
    Dim Utile As Double = C.H - (Margine + BandaIntestazioneRidotta) - BandaPiePagina
    Dim Himg As Double = (Utile - AltezzaGruppo - 4 - 3 * Distanza) / 3
    Dim ConDistribuzioni As Boolean = Not ControlliDistr Is Nothing AndAlso ControlliDistr.Count >= ControlliAvg.Count
    Fase("Exporting the charts", 60, 90)
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
    _ImgFatte += 1
    Passo(_ImgFatte, _ImgTot, NomeGrafico(Surf))
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
    Fase("Reading the channels for the table", 25, 45)
    Dim NumCanale As Integer = 0
    For Each cs In ListaCanali
      Passo(NumCanale, ListaCanali.Count, cs.ChannelName)
      NumCanale += 1
      Dim Canale = DataProvider2020.Channels.Canale(cs.ChannelName)
      If Canale Is Nothing OrElse Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eTWS Then Continue For
      Dim Coppia As New clsCoppieValoriTwsVsCanale(Canale)
      For Each p In Info.Periodi
        Coppia.AccodaDati(p.TR, FiltroMaschera)
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
    Fase("Building the data table", 45, 60)
    ' pagine orizzontali: piu' larghezza per colonna di TWS, quindi caratteri leggibili
    Dim Fh As New XFont("Tahoma", 8, XFontStyle.Bold)
    Dim F As New XFont("Tahoma", 7.5)
    Dim Fb As New XFont("Tahoma", 8, XFontStyle.Bold)
    Dim Hr As Double = 11
    Dim Col1 As Double = 118
    Dim LargPagina As Double = Math.Max(C.W, C.H)
    Dim Lb As Double = Math.Min(48, (LargPagina - 2 * Margine - Col1) / NumBin)

    ' durata: carattere piccolo e non bold, ridotto finche' "00:00" e "1d 00h" entrano nella colonna
    Dim DimDurata As Double = 7
    Dim Fd As New XFont("Tahoma", DimDurata)
    Using Gm As XGraphics = XGraphics.CreateMeasureContext(New XSize(100, 100), XGraphicsUnit.Point, XPageDirection.Downwards)
      While DimDurata > 4 AndAlso Math.Max(Gm.MeasureString("00:00", Fd).Width, Gm.MeasureString("1d 00h", Fd).Width) > Lb - 3
        DimDurata -= 0.5
        Fd = New XFont("Tahoma", DimDurata)
      End While
    End Using

    Dim Intestazione As Action =
      Sub()
        Cella(C.Gfx, "Channel / TWS", Fh, Margine, C.Y, Col1, Hr + 2, XColors.LightGray, XStringFormats.CenterLeft)
        For b As Integer = 0 To NumBin - 1
          Cella(C.Gfx, (Primo + b).ToString, Fh, Margine + Col1 + b * Lb, C.Y, Lb, Hr + 2, XColors.LightGray, XStringFormats.Center)
        Next
        C.Y += Hr + 2
      End Sub

    NuovaPagina(C, True, True)
    C.Gfx.DrawString("Data Table", New XFont("Verdana", 10, XFontStyle.Bold), XBrushes.Black, New XRect(Margine, C.Y, C.W - 2 * Margine, 14), XStringFormats.CenterLeft)
    C.Y += 16
    C.Gfx.DrawString(NotaBanda(), New XFont("Verdana", 6.5), XBrushes.Gray, New XRect(Margine, C.Y, C.W - 2 * Margine, 10), XStringFormats.CenterLeft)
    C.Y += 12
    Intestazione()

    ' minuti di navigazione per fascia
    Dim Prima = T.Voci.First.Coppia
    ' due righe: giorni e ore sopra, minuti e secondi sotto
    Cella(C.Gfx, "Duration", Fb, Margine, C.Y, Col1, Hr * 2, XColors.LightCyan, XStringFormats.CenterLeft)
    For b As Integer = 0 To NumBin - 1
      Dim N As Integer = Prima.Conteggio(Primo + b - 0.5, Primo + b + 0.5)
      Dim X As Double = Margine + Col1 + b * Lb
      Cella(C.Gfx, "", Fd, X, C.Y, Lb, Hr * 2, XColors.LightCyan, XStringFormats.Center)
      If N > 0 Then
        Dim Ts As TimeSpan = TimeSpan.FromSeconds(Math.Round(N / Hz))
        Dim Alto As String = ""
        If Ts.Days > 0 Then
          Alto = Ts.Days & "d " & Ts.Hours.ToString("00") & "h"
        ElseIf Ts.Hours > 0 Then
          Alto = Ts.Hours & "h"
        End If
        C.Gfx.DrawString(Alto, Fd, XBrushes.Black, New XRect(X, C.Y, Lb, Hr), XStringFormats.Center)
        C.Gfx.DrawString(Ts.Minutes.ToString("00") & ":" & Ts.Seconds.ToString("00"), Fd, XBrushes.Black, New XRect(X, C.Y + Hr, Lb, Hr), XStringFormats.Center)
      End If
    Next
    C.Y += Hr * 2 + 3

    Dim GruppoCorrente As Integer = -1
    Dim NumVoce As Integer = 0
    For Each Voce In T.Voci
      Passo(NumVoce, T.Voci.Count, Voce.Coppia.Canale.LongName)
      NumVoce += 1
      Dim Necessario As Double = Hr * 4 + 3 + If(Voce.Gruppo <> GruppoCorrente, Hr + 3, 0)
      If C.Y + Necessario > C.H - BandaPiePagina Then
        NuovaPagina(C, True, True)
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
      Dim Fmt As String = "F" & Decimali.ToString
      ' valori della voce: il carattere si riduce (stesso per tutta la voce) se un numero, ad esempio 123.45, non entra nella colonna
      Dim Testi(NumBin - 1)() As String
      Dim PiuLungo As String = ""
      For b As Integer = 0 To NumBin - 1
        Dim Tws As Integer = Primo + b
        Dim N As Integer = Voce.Coppia.Conteggio(Tws - 0.5, Tws + 0.5)
        Dim V As clsValoriBase = If(N = 0, Nothing, Voce.Coppia.Valori(Tws - 0.5, Tws + 0.5, _Banda))
        ' fascia senza dati: celle vuote, non zero
        Testi(b) = If(V Is Nothing, {"", "", "", ""}, {V.Avg.ToString(Fmt), V.Max.ToString(Fmt), V.Min.ToString(Fmt), V.Ds.ToString(Fmt)})
        For Each s In Testi(b)
          If s.Length > PiuLungo.Length Then PiuLungo = s
        Next
      Next
      Dim DimValori As Double = 7.5
      Using Gm As XGraphics = XGraphics.CreateMeasureContext(New XSize(100, 100), XGraphicsUnit.Point, XPageDirection.Downwards)
        While DimValori > 4.5 AndAlso Gm.MeasureString(PiuLungo, New XFont("Tahoma", DimValori + 0.5, XFontStyle.Bold)).Width > Lb - 4
          DimValori -= 0.5
        End While
      End Using
      Dim Fv As New XFont("Tahoma", DimValori)
      Dim Fvb As New XFont("Tahoma", DimValori + 0.5, XFontStyle.Bold)
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
        Cella(C.Gfx, Testi(b)(0), Fvb, X, C.Y, Lb, Hr, XColors.LightYellow, XStringFormats.CenterRight)
        Cella(C.Gfx, Testi(b)(1), Fv, X, C.Y + Hr, Lb, Hr, XColors.White, XStringFormats.CenterRight)
        Cella(C.Gfx, Testi(b)(2), Fv, X, C.Y + 2 * Hr, Lb, Hr, XColors.WhiteSmoke, XStringFormats.CenterRight)
        Cella(C.Gfx, Testi(b)(3), Fv, X, C.Y + 3 * Hr, Lb, Hr, XColors.White, XStringFormats.CenterRight)
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

    ' con la banda sotto 100 Max e Min sono percentili; si aggiungono due righe con il minimo e il massimo veri
    Dim ConVeri As Boolean = _Banda < 100
    Dim Nomi As String() = If(ConVeri, {"Avg", "Max", "Min", "Sd", "Max (true)", "Min (true)"}, {"Avg", "Max", "Min", "Sd"})
    For Each Voce In T.Voci
      Dim Canale = Voce.Coppia.Canale
      Dim Fmt As String = "F" & Canale.Decimals.ToString
      Dim Valori As New List(Of clsValoriBase)
      Dim Veri As New List(Of clsValoriBase)
      For tws As Integer = T.Primo To T.Ultimo
        Dim Presente As Boolean = Voce.Coppia.Conteggio(tws - 0.5, tws + 0.5) > 0
        Valori.Add(If(Presente, Voce.Coppia.Valori(tws - 0.5, tws + 0.5, _Banda), Nothing))
        If ConVeri Then Veri.Add(If(Presente, Voce.Coppia.Valori(tws - 0.5, tws + 0.5), Nothing))
      Next
      Dim ConDati As Boolean = Voce.Coppia.Conteggio(-1000, 1000) > 0
      Valori.Add(If(ConDati, Voce.Coppia.Valori(-1000, 1000, _Banda), Nothing))
      If ConVeri Then Veri.Add(If(ConDati, Voce.Coppia.Valori(-1000, 1000), Nothing))
      For k As Integer = 0 To Nomi.Length - 1
        Dim Riga As New List(Of String) From {CampoCsv(NomiGruppi(Voce.Gruppo)), CampoCsv(Canale.LongName), CampoCsv(If(Canale.ShortUM, "")), Nomi(k)}
        For i As Integer = 0 To Valori.Count - 1
          Dim v As clsValoriBase = If(k >= 4, Veri(i), Valori(i))
          If v Is Nothing Then
            Riga.Add("")
          Else
            Dim x As Double
            Select Case k
              Case 0 : x = v.Avg
              Case 1, 4 : x = v.Max
              Case 2, 5 : x = v.Min
              Case Else : x = v.Ds
            End Select
            Riga.Add(x.ToString(Fmt))
          End If
        Next
        Righe.Add(String.Join(";", Riga))
      Next
    Next
    Righe.Add(CampoCsv(NotaBanda()))
    System.IO.File.WriteAllText(Percorso, String.Join(vbCrLf, Righe) & vbCrLf, New System.Text.UTF8Encoding(True))
  End Sub


#End Region

End Class
