Public Class UserControlStartExpedition

  Public Sub New()
    Me.DataContext = ExpStarts

    ' This call is required by the designer.
    InitializeComponent()
    ' Add any initialization after the InitializeComponent() call.

  End Sub

  Private Sub NumberValidationTextBox(sender As Object, e As TextCompositionEventArgs)
    Dim regex = New System.Text.RegularExpressions.Regex("[^0-9]+")
    e.Handled = regex.IsMatch(e.Text)
  End Sub




  '    Using System.Text.RegularExpressions;
  'Private void NumberValidationTextBox(Object sender, TextCompositionEventArgs e)
  '{
  '    Regex regex = New Regex("[^0-9]+");
  '    e.Handled = regex.IsMatch(e.Text);
  '}


  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    'Dim ExpStarts As New clsExpeditionStarts
    ExpStarts.CercaPartenze()
    AppConfig.Salva()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStartPdfReport(SCsurface)
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    ExpStarts.CsvPartenzeToClipboard()
    MsgBox("Starts Table copied to the Clipboard", MsgBoxStyle.OkOnly)

  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.SetCurrentPoint(-180)
  End Sub

  Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.SetAtGun()
  End Sub

  Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.SetAtLineCrossing()
  End Sub

  Private Sub Button_Click_6(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.SetCurrentPoint(60)
  End Sub

  Private Sub Button_Click_7(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.GoPrevious()
  End Sub

  Private Sub Button_Click_8(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.GoNext()
  End Sub

  Private Sub Button_Click_9(sender As Object, e As RoutedEventArgs)
    ExpStarts.SelectedStart.SetCurrentPoint(-60)
  End Sub

  Private Sub listBoatConfig_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    Try
      Dim itm = DirectCast(sender.selecteditem, clsPrestartBoatData)
      ExpStarts.SelectedStart.SetCurrentPoint(itm.SecondsToStart)
    Catch ex As Exception

    End Try
    'Stop
  End Sub

  Private Sub Button_Click_10(sender As Object, e As RoutedEventArgs)
    Dim TR As New clsTimeRange(ExpStarts.SelectedStart.StartTime.AddSeconds(-180), ExpStarts.SelectedStart.StartTime.AddSeconds(60))
    Dim IR As New SciChart.Data.Model.DateRange(ExpStarts.SelectedStart.StartTime.AddSeconds(-180), ExpStarts.SelectedStart.StartTime.AddSeconds(60))
    DataPlotSync.SharedXVisibleRange = IR
    GraficoEventiViewModel.AggiornaSelezione(TR)
    MapControl.AggiornaSelezione(TR)
  End Sub

  Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)
    ExpStarts.StartPlotHeight = Math.Min(Me.ActualWidth / 2 - 3, Me.ActualHeight - 180)
  End Sub

  Private Sub Button_Click_11(sender As Object, e As RoutedEventArgs)
    System.Diagnostics.Process.Start("explorer.exe", DataProvider2020.Files.First.Directory.FullName)
  End Sub

    Private Sub Button_Click_12(sender As Object, e As RoutedEventArgs)
        ExpStarts.RaceReportPdf(SCsurface)
    End Sub

    Private Sub Button_Click_13(sender As Object, e As RoutedEventArgs)
        ExpStarts.SendEmailToMailingList()
    End Sub
End Class
