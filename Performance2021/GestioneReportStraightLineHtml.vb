Imports System.Text
Imports System.Net
Imports SciChart.Charting.Visuals

''' <summary>
''' Report HTML delle straight line: un solo file con gli stessi contenuti del pdf (riepilogo per TWS, grafici, tabella dati,
''' elenco periodi). I grafici sono incorporati come immagini png, quindi il file si sposta e si invia senza altri file.
''' </summary>
Partial Public Class clsStraightLineReport

#Region "Report html"

  Public Sub CreaReportHtml(ControlliAvg As List(Of SciChartSurface), ControlliDistr As List(Of SciChartSurface), OutputType As clsStraightLineVM2020.eOutputType, ListaPeriodi As List(Of clsPeriod2021), ListaCanali As List(Of clsStraightLineChartSettings), Filtro As String, Opzioni As clsStraightLineReportOptions, Optional Av As FinestraAvanzamento = Nothing)
    If Not Opzioni.PrintCharts AndAlso Not Opzioni.PrintTable AndAlso Not Opzioni.PrintPeriods Then
      MsgBox("Nothing to create: tick at least one option under Reports.", MsgBoxStyle.Information, "Straight Line report")
      Exit Sub
    End If
    ImpostaReport(Opzioni, Av)
    Dim Info As clsInfo = CreaInfo(ListaPeriodi, OutputType, Filtro, "")
    If Info Is Nothing Then Exit Sub
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info)
    Dim Tabella As clsTabella = Nothing
    If Opzioni.PrintTable Then Tabella = CostruisciTabella(Info, ListaCanali)
    If Opzioni.PrintCharts AndAlso Not ControlliAvg Is Nothing Then _ImgTot = ControlliAvg.Count + If(ControlliDistr Is Nothing, 0, ControlliDistr.Count)

    Dim H As New StringBuilder
    H.AppendLine("<!DOCTYPE html>")
    H.AppendLine("<html lang=""en""><head><meta charset=""utf-8"">")
    H.AppendLine("<meta name=""viewport"" content=""width=device-width, initial-scale=1"">")
    H.AppendLine("<title>" & Enc(Info.TitoloBreve) & "</title>")
    H.AppendLine(StileHtml())
    H.AppendLine("</head><body>")

    H.AppendLine("<h1>" & Enc(Info.Titolo) & "</h1>")
    Dim Minuti As Double = Info.Periodi.Sum(Function(p) p.TR.Durata.TotalMinutes)
    Dim Sotto As String = Info.Periodi.Count & " periods, " & Minuti.ToString("F1") & " min"
    Dim Nome As String = NomeFileDati()
    If Nome <> "" Then Sotto &= "  |  " & Nome
    H.AppendLine("<p class=""sub"">" & Enc(Sotto) & "</p>")
    H.AppendLine(LegendaHtml(Info, OutputType))

    H.AppendLine(RiepilogoHtml(Info, Stat))
    If Info.IsReaching Then H.AppendLine(RiepilogoTwaHtml(Info, Opzioni.TwaBinDegrees))
    If Opzioni.PrintCharts AndAlso Not ControlliAvg Is Nothing AndAlso ControlliAvg.Count > 0 Then H.AppendLine(GraficiHtml(ControlliAvg, ControlliDistr))
    If Opzioni.PrintTable AndAlso Not Tabella Is Nothing Then H.AppendLine(TabellaHtml(Tabella))
    If Opzioni.PrintPeriods Then H.AppendLine(PeriodiHtml(Info, OutputType))

    H.AppendLine("<p class=""foot"">Performance2021" & If(Nome = "", "", "  |  " & Enc(Nome)) & "  |  " & Now.ToString("yyyy-MM-dd HH:mm") & "</p>")
    H.AppendLine("</body></html>")

    Fase("Saving the html", 90, 100)
    Dim Percorso As String = PercorsoLibero(Info, OutputType, ".html")
    System.IO.File.WriteAllText(Percorso, H.ToString, New UTF8Encoding(True))
    If Opzioni.CreateTableCsv AndAlso Not Tabella Is Nothing Then ScriviCsv(Tabella, Percorso.Substring(0, Percorso.Length - 5) & ".csv")
    Process.Start(Percorso)
  End Sub

#End Region

#Region "Pezzi della pagina"

  Private Shared Function Enc(Testo As String) As String
    Return WebUtility.HtmlEncode(If(Testo, ""))
  End Function

  Private Shared Function Css(c As System.Windows.Media.Color) As String
    Return "#" & c.R.ToString("X2") & c.G.ToString("X2") & c.B.ToString("X2")
  End Function

  Private Shared Function StileHtml() As String
    Return "<style>" & vbCrLf &
      "body{font-family:Verdana,Segoe UI,sans-serif;font-size:13px;color:#111;margin:24px auto;max-width:1400px;padding:0 16px}" & vbCrLf &
      "h1{font-size:22px;margin:0 0 4px}h2{font-size:16px;margin:28px 0 8px;border-bottom:1px solid #ccc;padding-bottom:3px}" & vbCrLf &
      "h3{font-size:13px;margin:16px 0 6px;color:#444;background:#ebeff5;padding:3px 8px}" & vbCrLf &
      ".sub{color:#666;margin:0 0 10px}.foot{color:#888;font-size:11px;margin-top:30px;border-top:1px solid #ddd;padding-top:6px}" & vbCrLf &
      ".leg{display:flex;flex-wrap:wrap;gap:14px;align-items:center;margin:6px 0 4px}.dot{display:inline-block;width:10px;height:10px;border-radius:50%;margin-right:5px}" & vbCrLf &
      ".bar{display:inline-block;width:140px;height:10px;vertical-align:middle;margin:0 4px}" & vbCrLf &
      "table{border-collapse:collapse;font-size:12px}td,th{border:1px solid #ddd;padding:2px 7px}" & vbCrLf &
      "th{background:#d3d3d3;text-align:center}td.n{text-align:right;font-variant-numeric:tabular-nums}td.l{text-align:left}" & vbCrLf &
      ".alt{background:#f5f5f5}.all{background:#ffffe0;font-weight:bold}.port{background:#ffe4e1}.stbd{background:#f0fff0}" & vbCrLf &
      ".small{color:#666;font-size:10px}.wrap{overflow-x:auto}" & vbCrLf &
      ".grid{display:grid;grid-template-columns:repeat(3,1fr);gap:8px}.grid img{width:100%;display:block;border:1px solid #eee}" & vbCrLf &
      ".cell{display:flex;flex-direction:column;gap:4px}" & vbCrLf &
      ".tbl th.c1,.tbl td.c1{position:sticky;left:0;background:#fff;text-align:left;white-space:nowrap;z-index:1}.tbl th.c1{background:#d3d3d3}" & vbCrLf &
      ".tbl .grp td{background:#dce4f0;font-weight:bold;text-align:left}.tbl .dur td{background:#e0ffff;font-weight:bold}" & vbCrLf &
      ".tbl .avg td{background:#ffffe0;font-weight:bold}.tbl .avg td.c1{background:#ffffe0}" & vbCrLf &
      ".tbl .st td{background:#fff}.tbl .st.alt td{background:#f5f5f5}.tbl .st td.c1{padding-left:18px;color:#444}" & vbCrLf &
      "@media (max-width:800px){.grid{grid-template-columns:1fr 1fr}}@media print{body{max-width:none}.grid{grid-template-columns:repeat(3,1fr)}}" & vbCrLf &
      "</style>"
  End Function

  Private Function LegendaHtml(Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType) As String
    Dim Sb As New StringBuilder("<div class=""leg"">")
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eGroupByKey, clsStraightLineVM2020.eOutputType.eColorByKey
        For Each Chiave In Info.Chiavi
          Sb.Append("<span><span class=""dot"" style=""background:" & Css(Chiave.Value) & """></span>" & Enc(Chiave.Key) & "</span>")
        Next
      Case clsStraightLineVM2020.eOutputType.eGroupByTack, clsStraightLineVM2020.eOutputType.eColorByTack,
           clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
        Sb.Append("<span><span class=""dot"" style=""background:red""></span>Port: " & Info.Porta & " periods</span>")
        Sb.Append("<span><span class=""dot"" style=""background:green""></span>Stbd: " & Info.Dritta & " periods</span>")
        If OutputType = clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc OrElse OutputType = clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc Then
          Dim Minimo As Double = AppConfig.ActiveProfile.MinVmgPerformanceValue
          Dim Massimo As Double = AppConfig.ActiveProfile.MaxVmgPerformenceValue
          Dim Passi As New List(Of String)
          For k As Integer = 0 To 10
            Passi.Add(Css(ColoreDaPerformance(Minimo + (Massimo - Minimo) * k / 10)))
          Next
          Sb.Append("<span>" & If(OutputType = clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, "Dots: Vmg % of target", "Dots: Bs % of polar") &
                   " <span class=""small"">&le;" & CInt(Minimo) & "</span><span class=""bar"" style=""background:linear-gradient(to right," & String.Join(",", Passi) & ")""></span><span class=""small"">&ge;" & CInt(Massimo) & "</span></span>")
        End If
      Case Else
        Sb.Append("<span>Periods: " & Info.Dritta & " Stbd, " & Info.Porta & " Port</span>")
    End Select
    Sb.Append("</div>")
    Return Sb.ToString
  End Function

  Private Function RiepilogoHtml(Info As clsInfo, Stat As List(Of clsStatPeriodo)) As String
    Dim Perf As String = If(Info.IsReaching, "Polar %", "Vmg %")
    Dim Titoli As String() = {"TWS", Perf & " avg", Perf & " min - max", "Bs (kn)", "TWA (deg)", "Port/Stbd", "Duration"}
    Dim Sb As New StringBuilder("<h2>Summary by TWS</h2><div class=""wrap""><table><tr>")
    For Each T In Titoli
      Sb.Append("<th>" & Enc(T) & "</th>")
    Next
    Sb.AppendLine("</tr>")
    Dim Alt As Boolean = False
    For Each G In Stat.Where(Function(s) Not Double.IsNaN(s.Tws)).GroupBy(Function(s) CInt(Math.Round(s.Tws))).OrderBy(Function(g2) g2.Key)
      Sb.AppendLine(RigaHtml(RigaRiepilogo(G.Key.ToString, G.ToList), If(Alt, "alt", "")))
      Alt = Not Alt
    Next
    Sb.AppendLine(RigaHtml(RigaRiepilogo("All", Stat), "all"))
    Sb.AppendLine(RigaHtml(RigaRiepilogo("Port", Stat.Where(Function(s) Not s.IsStbd).ToList), "port"))
    Sb.AppendLine(RigaHtml(RigaRiepilogo("Stbd", Stat.Where(Function(s) s.IsStbd).ToList), "stbd"))
    Sb.Append("</table></div>" & NotaHtml())
    Return Sb.ToString
  End Function

  ''' <summary>Nota sotto una tabella: Min e Max sono percentili.</summary>
  Private Shared Function NotaHtml() As String
    Return "<p class=""small"">" & Enc(NotaBanda()) & "</p>"
  End Function

  Private Shared Function RigaHtml(Valori As String(), Classe As String) As String
    Dim Sb As New StringBuilder("<tr" & If(Classe = "", "", " class=""" & Classe & """") & ">")
    For i As Integer = 0 To Valori.Length - 1
      Sb.Append("<td class=""" & If(i = 0, "l", "n") & """>" & Enc(Valori(i)) & "</td>")
    Next
    Sb.Append("</tr>")
    Return Sb.ToString
  End Function

  ''' <summary>Solo reaching: Polar % per TWS e fasce di TWA, una tabella per mura.</summary>
  Private Function RiepilogoTwaHtml(Info As clsInfo, AmpiezzaTwa As Integer) As String
    Dim Passo As Integer = If(AmpiezzaTwa <= 0, 20, Math.Max(5, Math.Min(90, AmpiezzaTwa)))
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info).
      Where(Function(s) Not Double.IsNaN(s.Tws) AndAlso Not Double.IsNaN(s.Twa)).ToList
    Return TabellaTwaHtml("Port", Stat.Where(Function(s) Not s.IsStbd).ToList, Passo, "port") &
           TabellaTwaHtml("Stbd", Stat.Where(Function(s) s.IsStbd).ToList, Passo, "stbd")
  End Function

  Private Shared Function CellaPerfHtml(Stat As List(Of clsStatPeriodo)) As String
    If Stat.Count = 0 Then Return "<td></td>"
    Dim T As String = Enc(Formato(MediaPesata(Stat, Function(s) s.Perf), 1)) & " <span class=""small"">(" & FormatoDurata(Stat.Sum(Function(s) s.Minuti) * 60) & ")</span>"
    Dim Lo, Hi As Double
    If IntervalloPerf(Stat, Lo, Hi) Then T &= "<br><span class=""small"">" & Lo.ToString("F1") & " - " & Hi.ToString("F1") & "</span>"
    Return "<td class=""n"">" & T & "</td>"
  End Function

  Private Shared Function TabellaTwaHtml(Mura As String, Stat As List(Of clsStatPeriodo), Passo As Integer, ClasseMura As String) As String
    If Stat.Count = 0 Then Return ""
    Dim Fascia As Func(Of clsStatPeriodo, Integer) = Function(s) CInt(Math.Floor(Math.Abs(s.Twa) / Passo))
    Dim Primo As Integer = Stat.Min(Fascia)
    Dim Ultimo As Integer = Stat.Max(Fascia)
    Dim Sb As New StringBuilder("<h2>Polar % by TWS and TWA - " & Mura & " tack <span class=""small"">(avg (duration), min - max below)</span></h2><div class=""wrap""><table><tr><th>TWS \ TWA</th>")
    For b As Integer = Primo To Ultimo
      Sb.Append("<th>" & (b * Passo) & "-" & ((b + 1) * Passo) & "</th>")
    Next
    Sb.AppendLine("<th>All</th></tr>")
    Dim Alt As Boolean = False
    For Each G In Stat.GroupBy(Function(s) CInt(Math.Round(s.Tws))).OrderBy(Function(g2) g2.Key)
      Sb.Append("<tr" & If(Alt, " class=""alt""", "") & "><td class=""l"">" & G.Key & "</td>")
      For b As Integer = Primo To Ultimo
        Dim bb As Integer = b
        Sb.Append(CellaPerfHtml(G.Where(Function(s) Fascia(s) = bb).ToList))
      Next
      Sb.AppendLine(CellaPerfHtml(G.ToList) & "</tr>")
      Alt = Not Alt
    Next
    Sb.Append("<tr class=""" & ClasseMura & """><td class=""l""><b>All</b></td>")
    For b As Integer = Primo To Ultimo
      Dim bb As Integer = b
      Sb.Append(CellaPerfHtml(Stat.Where(Function(s) Fascia(s) = bb).ToList))
    Next
    Sb.AppendLine(CellaPerfHtml(Stat) & "</tr></table></div>" & NotaHtml())
    Return Sb.ToString
  End Function

  Private Shared Function ImmagineHtml(Surf As SciChartSurface) As String
    _ImgFatte += 1
    Passo(_ImgFatte, _ImgTot, NomeGrafico(Surf))
    Try
      Using Flusso = Surf.ExportToStream(SciChart.Core.ExportType.Bmp, False)
        Dim Dec As New System.Windows.Media.Imaging.BmpBitmapDecoder(Flusso, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.OnLoad)
        Dim Enco As New System.Windows.Media.Imaging.PngBitmapEncoder
        Enco.Frames.Add(Dec.Frames(0))
        Using Ms As New System.IO.MemoryStream
          Enco.Save(Ms)
          Return "<img alt="""" src=""data:image/png;base64," & Convert.ToBase64String(Ms.ToArray) & """>"
        End Using
      End Using
    Catch ex As Exception
      Return "<div class=""small"">chart not available</div>"
    End Try
  End Function

  Private Function GraficiHtml(ControlliAvg As List(Of SciChartSurface), ControlliDistr As List(Of SciChartSurface)) As String
    Dim ConDistribuzioni As Boolean = Not ControlliDistr Is Nothing AndAlso ControlliDistr.Count >= ControlliAvg.Count
    Dim Gruppo As Integer() = ControlliAvg.Select(Function(s) GruppoCanale(NomeGrafico(s))).ToArray
    Fase("Exporting the charts", 60, 90)
    Dim Sb As New StringBuilder("<h2>Charts</h2>")
    For g As Integer = 0 To NomiGruppi.Length - 1
      Dim Indici As List(Of Integer) = Enumerable.Range(0, ControlliAvg.Count).Where(Function(i) Gruppo(i) = g).ToList
      If Indici.Count = 0 Then Continue For
      Sb.AppendLine("<h3>" & Enc(NomiGruppi(g)) & "</h3><div class=""grid"">")
      For Each i In Indici
        Sb.Append("<div class=""cell"">" & ImmagineHtml(ControlliAvg(i)))
        If ConDistribuzioni Then Sb.Append(ImmagineHtml(ControlliDistr(i)))
        Sb.AppendLine("</div>")
      Next
      Sb.AppendLine("</div>")
    Next
    Return Sb.ToString
  End Function

  Private Function TabellaHtml(T As clsTabella) As String
    Dim NumBin As Integer = T.Ultimo - T.Primo + 1
    Fase("Building the data table", 45, 60)
    Dim Sb As New StringBuilder("<h2>Data Table</h2>" & NotaHtml() & "<div class=""wrap""><table class=""tbl""><tr><th class=""c1"">Channel / TWS</th>")
    For b As Integer = 0 To NumBin - 1
      Sb.Append("<th>" & (T.Primo + b) & "</th>")
    Next
    Sb.AppendLine("</tr>")

    Dim Prima = T.Voci.First.Coppia
    Sb.Append("<tr class=""dur""><td class=""c1"">Duration</td>")
    For b As Integer = 0 To NumBin - 1
      Dim N As Integer = Prima.Conteggio(T.Primo + b - 0.5, T.Primo + b + 0.5)
      Sb.Append("<td class=""n"">" & If(N = 0, "", FormatoDurata(N / T.Hz)) & "</td>")
    Next
    Sb.AppendLine("</tr>")

    Dim GruppoCorrente As Integer = -1
    Dim NumVoce As Integer = 0
    For Each Voce In T.Voci
      Passo(NumVoce, T.Voci.Count, Voce.Coppia.Canale.LongName)
      NumVoce += 1
      If Voce.Gruppo <> GruppoCorrente Then
        GruppoCorrente = Voce.Gruppo
        Sb.AppendLine("<tr class=""grp""><td colspan=""" & (NumBin + 1) & """>" & Enc(NomiGruppi(GruppoCorrente)) & "</td></tr>")
      End If
      Dim Canale = Voce.Coppia.Canale
      Dim Fmt As String = "F" & Canale.Decimals.ToString
      Dim Nome As String = Canale.LongName
      If Not String.IsNullOrWhiteSpace(Canale.ShortUM) Then Nome &= " (" & Canale.ShortUM & ")"
      Dim Valori As New List(Of clsValoriBase)
      For b As Integer = 0 To NumBin - 1
        Dim Tws As Integer = T.Primo + b
        Valori.Add(If(Voce.Coppia.Conteggio(Tws - 0.5, Tws + 0.5) = 0, Nothing, Voce.Coppia.Valori(Tws - 0.5, Tws + 0.5, _Banda)))
      Next
      Dim Righe As String() = {Nome, "Max", "Min", "Sd"}
      For k As Integer = 0 To 3
        Sb.Append("<tr class=""" & If(k = 0, "avg", If(k = 2, "st alt", "st")) & """><td class=""c1"">" & Enc(Righe(k)) & "</td>")
        For Each v In Valori
          If v Is Nothing Then
            Sb.Append("<td></td>")
          Else
            Dim x As Double = If(k = 0, v.Avg, If(k = 1, v.Max, If(k = 2, v.Min, v.Ds)))
            Sb.Append("<td class=""n"">" & x.ToString(Fmt) & "</td>")
          End If
        Next
        Sb.AppendLine("</tr>")
      Next
    Next
    Sb.Append("</table></div>")
    Return Sb.ToString
  End Function

  Private Function PeriodiHtml(Info As clsInfo, OutputType As clsStraightLineVM2020.eOutputType) As String
    Dim Stat As List(Of clsStatPeriodo) = StatInfo(Info).
      OrderBy(Function(s) If(Double.IsNaN(s.Tws), 0, Math.Round(s.Tws))).ThenBy(Function(s) s.Periodo.TR.Start).ToList
    Dim Perf As String = If(Info.IsReaching, "Polar %", "Vmg %")
    Dim Sb As New StringBuilder("<h2>Periods</h2><div class=""wrap""><table><tr><th></th>")
    For Each T In {"Start", "Tack", "Dur (s)", "TWS", "TWA", "Bs (kn)", Perf, "Key"}
      Sb.Append("<th>" & Enc(T) & "</th>")
    Next
    Sb.AppendLine("</tr>")
    Dim Alt As Boolean = False
    For Each s In Stat
      Sb.Append("<tr" & If(Alt, " class=""alt""", "") & "><td><span class=""dot"" style=""background:" & Css(ColoreDaOutputType(s.Periodo, OutputType)) & """></span></td>")
      Sb.Append("<td class=""l"">" & s.Periodo.TR.Start.ToString("HH:mm:ss") & "</td><td class=""l"">" & If(s.IsStbd, "Stbd", "Port") & "</td>")
      Sb.Append("<td class=""n"">" & s.Periodo.TR.Durata.TotalSeconds.ToString("F0") & "</td><td class=""n"">" & Formato(s.Tws, 1) & "</td>")
      Sb.Append("<td class=""n"">" & Formato(Math.Abs(s.Twa), 0) & "</td><td class=""n"">" & Formato(s.Sow, 2) & "</td><td class=""n"">" & Formato(s.Perf, 1) & "</td>")
      Sb.AppendLine("<td class=""l"">" & Enc(s.Periodo.Keys.Trim) & "</td></tr>")
      Alt = Not Alt
    Next
    Sb.Append("</table></div>")
    Return Sb.ToString
  End Function

#End Region

End Class
