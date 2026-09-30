Imports System.Globalization

''' <summary>
''' Mostra il risultato di "Long Range Auto Check": lista manovre, corrente per fascia di tempo
''' e per zona, correzioni globali. Tiene una copia dei canali del range analizzato
''' (letta dal chiamante sul thread UI): i ricalcoli usano solo quella, mai il DataProvider.
''' </summary>
Public Class ManeuverCurrentWindow

  Private ReadOnly _nc As NavChannels
  Private ReadOnly _inizio As DateTime
  Private ReadOnly _fine As DateTime
  Private _report As ManeuverReport

  Public Sub New(nc As NavChannels, inizio As DateTime, fine As DateTime)
    InitializeComponent()
    _nc = nc
    _inizio = inizio
    _fine = fine
    Calcola()
  End Sub

  Private Function LeggiNumero(txt As TextBox, predefinito As Double) As Double
    Dim v As Double
    Dim s As String = txt.Text.Trim().Replace(",", ".")
    If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, v) AndAlso v > 0 Then Return v
    txt.Text = predefinito.ToString(CultureInfo.InvariantCulture)
    Return predefinito
  End Function

  Private Sub Calcola()
    Dim finestraMin As Double = LeggiNumero(txt_TimeWindow, 30)
    Dim zonaNm As Double = LeggiNumero(txt_ZoneNm, 0.5)

    _report = ManeuverCurrentAnalyzer.AnalyzeReport(_nc, _inizio, _fine, finestraMin, zonaNm)

    grd_Manovre.ItemsSource = _report.Maneuvers
    grd_Tempo.ItemsSource = _report.TimeGroups
    grd_Zone.ItemsSource = _report.ZoneGroups
    txt_Correzioni.Text = ResultsClipboard.TestoCorrezioni(_report)
  End Sub

  Private Sub btn_Ricalcola_Click(sender As Object, e As RoutedEventArgs)
    Calcola()
  End Sub

  Private Sub btn_Copia_Click(sender As Object, e As RoutedEventArgs)
    ResultsClipboard.CopyReportToClipboard(_report)
  End Sub

End Class
