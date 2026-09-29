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
    Private Const DtFmt As String = "yyyy-MM-dd HH:mm:ss.fff"

    ''' <summary>Costruisce il TSV (tab + CRLF) con tutte le proprieta' e lo mette negli appunti. Restituisce anche la stringa.</summary>
    Public Function CopyResultsToClipboard(results As ManeuverResult()) As String
        Dim sb As New StringBuilder()

        Dim headers As String() = {
            "TackChangeTime", "TransitionStart", "TransitionEnd", "BeforeStart", "BeforeEnd",
            "AfterStart", "AfterEnd", "IsUpwind", "BeforeTackSign", "AfterTackSign",
            "MeanLat", "MeanLon", "EpochId",
            "CurrentSet", "CurrentDrift", "OffsetBefore", "OffsetAfter", "PairResidual", "PairConverged",
            "GlobalCurrentSet", "GlobalCurrentDrift", "GlobalLogCoefficient",
            "GlobalCompassOffsetStbd", "GlobalCompassOffsetPort", "GlobalRms",
            "ModeUsed", "UnderIdentified", "HeadingSpreadStbd", "HeadingSpreadPort"}
        sb.Append(String.Join(vbTab, headers)).Append(vbCrLf)

        If results IsNot Nothing Then
            For Each r In results
                Dim cols As String() = {
                    r.TackChangeTime.ToString(DtFmt, Ci),
                    r.TransitionStart.ToString(DtFmt, Ci),
                    r.TransitionEnd.ToString(DtFmt, Ci),
                    r.BeforeStart.ToString(DtFmt, Ci),
                    r.BeforeEnd.ToString(DtFmt, Ci),
                    r.AfterStart.ToString(DtFmt, Ci),
                    r.AfterEnd.ToString(DtFmt, Ci),
                    r.IsUpwind.ToString(),
                    r.BeforeTackSign.ToString(Ci),
                    r.AfterTackSign.ToString(Ci),
                    r.MeanLat.ToString("0.000000", Ci),
                    r.MeanLon.ToString("0.000000", Ci),
                    r.EpochId.ToString(Ci),
                    r.CurrentSet.ToString("0.0", Ci),
                    r.CurrentDrift.ToString("0.000", Ci),
                    r.OffsetBefore.ToString("0.000", Ci),
                    r.OffsetAfter.ToString("0.000", Ci),
                    r.PairResidual.ToString("0.000", Ci),
                    r.PairConverged.ToString(),
                    r.GlobalCurrentSet.ToString("0.0", Ci),
                    r.GlobalCurrentDrift.ToString("0.000", Ci),
                    r.GlobalLogCoefficient.ToString("0.0000", Ci),
                    r.GlobalCompassOffsetStbd.ToString("0.000", Ci),
                    r.GlobalCompassOffsetPort.ToString("0.000", Ci),
                    r.GlobalRms.ToString("0.000", Ci),
                    r.ModeUsed.ToString(),
                    r.UnderIdentified.ToString(),
                    r.HeadingSpreadStbd.ToString("0.0", Ci),
                    r.HeadingSpreadPort.ToString("0.0", Ci)}
                sb.Append(String.Join(vbTab, cols)).Append(vbCrLf)
            Next
        End If

        Dim text As String = sb.ToString()

        ' Deve girare su thread STA (il thread UI lo e'). SetDataObject(..., True)
        ' mantiene il contenuto negli appunti anche dopo la chiusura dell'app.
        Clipboard.SetDataObject(text, True)

        Return text
    End Function

End Module
