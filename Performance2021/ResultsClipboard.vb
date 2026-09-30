Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Text
Imports System.Globalization
Imports System.Windows            ' WPF: System.Windows.Clipboard
' Per WinForms usa invece:  Imports System.Windows.Forms  (e System.Windows.Forms.Clipboard)

Public Module ResultsClipboard

    ' Cultura per i numeri:
    '  - InvariantCulture -> separatore decimale '.'  (file di testo portabile)
    '  - Per incollare in Excel con locale IT, usa CultureInfo.GetCultureInfo("it-IT")
    '    cosi' i decimali usano la virgola (il TAB resta il separatore di colonna, nessuna ambiguita').
    Private ReadOnly Ci As CultureInfo = CultureInfo.InvariantCulture
    Private Const DtFmt As String = "yyyy-MM-dd HH:mm:ss"

    Public Function NomeMura(segno As Integer) As String
        Return If(segno > 0, "Stbd", "Port")
    End Function

    Private Sub Riga(sb As StringBuilder, ParamArray cols As String())
        sb.Append(String.Join(vbTab, cols)).Append(vbCrLf)
    End Sub

    ''' <summary>
    ''' Testo di riepilogo delle correzioni (una riga per modello), per la finestra a schermo.
    ''' </summary>
    Public Function TestoCorrezioni(rep As ManeuverReport) As String
        If rep Is Nothing OrElse rep.Unica Is Nothing Then Return "Nessuna manovra utilizzabile nel range visibile."
        Dim sb As New StringBuilder()
        For Each m In {rep.Unica, rep.ByTime, rep.ByZone}
            If m Is Nothing Then Continue For
            sb.Append(m.Name.PadRight(34)).
               Append("gruppi ").Append(m.GroupCount.ToString(Ci).PadLeft(2)).
               Append("   k ").Append(m.LogCoefficient.ToString("0.0000", Ci)).
               Append("   Stbd ").Append(m.OffsetStbd.ToString("+0.00;-0.00", Ci)).Append("°").
               Append("   Port ").Append(m.OffsetPort.ToString("+0.00;-0.00", Ci)).Append("°").
               Append("   RMS ").Append(m.Rms.ToString("0.000", Ci)).Append(" kn").
               Append(vbCrLf)
        Next
        If rep.UnderIdentified Then
            sb.Append("Attenzione: prue poco diverse per mura, k fissato d'ufficio (modello poco identificabile).")
        End If
        Return sb.ToString().TrimEnd()
    End Function

    ''' <summary>
    ''' Costruisce il TSV (tab + CRLF): manovre, fasce di tempo, zone, correzioni globali.
    ''' </summary>
    Public Function TestoReport(rep As ManeuverReport) As String
        Dim sb As New StringBuilder()
        If rep Is Nothing Then Return ""

        Riga(sb, "Ora", "Tipo", "Mura", "Fascia", "Zona", "OffsetPrima", "OffsetDopo",
             "CorrenteSet", "CorrenteDrift", "ResiduoGlobale", "Convergita")
        For Each r In rep.Maneuvers
            Riga(sb,
                 r.TackChangeTime.ToString(DtFmt, Ci),
                 If(r.IsUpwind, "Tack", "Gybe"),
                 NomeMura(r.BeforeTackSign) & ">" & NomeMura(r.AfterTackSign),
                 r.TimeGroupId.ToString(Ci),
                 r.ZoneId.ToString(Ci),
                 r.OffsetBefore.ToString("0.00", Ci),
                 r.OffsetAfter.ToString("0.00", Ci),
                 r.CurrentSet.ToString("0", Ci),
                 r.CurrentDrift.ToString("0.00", Ci),
                 r.GlobalPairResidual.ToString("0.00", Ci),
                 If(r.PairConverged, "Si", "No"))
        Next

        sb.Append(vbCrLf)
        Riga(sb, "Fascia", "Intervallo", "Manovre", "CorrenteSet", "CorrenteDrift", "RMS")
        For Each g In rep.TimeGroups
            Riga(sb, g.Id.ToString(Ci), g.Label, g.ManeuverCount.ToString(Ci),
                 g.CurrentSet.ToString("0", Ci), g.CurrentDrift.ToString("0.00", Ci), g.Rms.ToString("0.000", Ci))
        Next

        sb.Append(vbCrLf)
        Riga(sb, "Zona", "Nome", "Manovre", "MeanLat", "MeanLon", "CorrenteSet", "CorrenteDrift", "RMS")
        For Each g In rep.ZoneGroups
            Riga(sb, g.Id.ToString(Ci), g.Label, g.ManeuverCount.ToString(Ci),
                 g.MeanLat.ToString("0.0000", Ci), g.MeanLon.ToString("0.0000", Ci),
                 g.CurrentSet.ToString("0", Ci), g.CurrentDrift.ToString("0.00", Ci), g.Rms.ToString("0.000", Ci))
        Next

        sb.Append(vbCrLf)
        Riga(sb, "Modello", "Gruppi", "CoeffLog", "OffsetStbd", "OffsetPort", "RMS")
        For Each m In {rep.Unica, rep.ByTime, rep.ByZone}
            If m Is Nothing Then Continue For
            Riga(sb, m.Name, m.GroupCount.ToString(Ci), m.LogCoefficient.ToString("0.0000", Ci),
                 m.OffsetStbd.ToString("0.00", Ci), m.OffsetPort.ToString("0.00", Ci), m.Rms.ToString("0.000", Ci))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Mette il report negli appunti. Restituisce anche la stringa.</summary>
    Public Function CopyReportToClipboard(rep As ManeuverReport) As String
        Dim text As String = TestoReport(rep)
        ' Deve girare su thread STA (il thread UI lo e'). SetDataObject(..., True)
        ' mantiene il contenuto negli appunti anche dopo la chiusura dell'app.
        Clipboard.SetDataObject(text, True)
        Return text
    End Function

End Module
