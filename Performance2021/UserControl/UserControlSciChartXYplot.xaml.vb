Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports System.Timers
Imports System.Windows.Threading
Imports Accord.Math.Distances
Imports Newtonsoft.Json
Imports OfficeOpenXml.ConditionalFormatting
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.Database
Imports Parquet.File.Values.Primitives
Imports PdfSharp.Pdf.IO
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Charting2D.Interop
Imports ScottPlot.Colormaps
Imports Unity.Lifetime.LifetimeManager


Public Class UserControlSciChartXYplot
  Dim _VM As New clsSciChartXyPlotViewModel
  'WithEvents tmavviopdf As New Timers.Timer
  'WithEvents tmavvioshowplot As New Timers.Timer
  'WithEvents tm As New Timers.Timer

  Public Sub New()
    clsLogTempi.Scrivi("XY UserControl: costruttore, inizio")
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()
    clsLogTempi.Scrivi("XY UserControl: InitializeComponent finito")

    ' --- Nulla di ciò che segue deve girare nel designer XAML ---
    If IsInDesignMode Then Exit Sub

    ' Add any initialization after the InitializeComponent() call.
    GroupingType.ItemsSource = System.Enum.GetValues(GetType(clsXYPlotSettings.eGroupingType)).Cast(Of clsXYPlotSettings.eGroupingType)
    SorgenteDati.ItemsSource = System.Enum.GetValues(GetType(clsXYPlotSettings.eDataSource)).Cast(Of clsXYPlotSettings.eDataSource)
    SailingState.ItemsSource = System.Enum.GetValues(GetType(clsPeriodsManager2021.eRowType)).Cast(Of clsPeriodsManager2021.eRowType)
    VM.CurrentPlotSettings = AppConfig.ActiveProfile.XYPlotSettings
    VM.XyReports = AppConfig.ActiveProfile.XyReports
    'tm.Interval = 300
    'tmavviopdf.Interval = 500
    'tmavvioshowplot.Interval = 500
  End Sub


  Public Property VM As clsSciChartXyPlotViewModel
    Get
      Return _VM
    End Get
    Set(value As clsSciChartXyPlotViewModel)
      _VM = value
    End Set
  End Property

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

    Using clsLogTempi.Misura("XY Refresh: AggiornaGrafico")
      VM.AggiornaGrafico(VM.CollectImages = False)
    End Using
    Plot.ZoomExtents()


    ' --- Range manuale assi principali ---
    If VM.CurrentPlotSettings.ApplyMinX Then
      Plot.XAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
            VM.CurrentPlotSettings.MinX,
            VM.CurrentPlotSettings.MaxX)
    End If
    If VM.CurrentPlotSettings.ApplyMinY Then
      Plot.YAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
            VM.CurrentPlotSettings.MinY,
            VM.CurrentPlotSettings.MaxY)
    End If

    Plot.XAxes(1).VisibleRange = Plot.XAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
    Plot.XAxes(2).VisibleRange = Plot.YAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
    Plot.YAxes(1).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
    Plot.YAxes(2).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
    VM.CurrentPlotSettings.SalvaNomiCanali()
    AppConfig.Salva() ' VM.SalvaSettings()
  End Sub

  Private Sub AggiornaGrafico3D()
    If Plot3D Is Nothing Then Exit Sub
    Dim mX As Double = VM.PuntiXYZ.Select(Function(x) x.X).Max
    Dim mY As Double = VM.PuntiXYZ.Select(Function(x) x.Y).Max
    Dim mZ As Double = VM.PuntiXYZ.Select(Function(x) x.Z).Max
    If (Double.IsNaN(mX)) Then mX = 0
    If (Double.IsNaN(mY)) Then mY = 0
    If (Double.IsNaN(mZ)) Then mZ = 0
    Dim DataRange As New Media3D.Rect3D(0, 0, 0, mX, mY, mZ)
    Dim displayedDataBounds = Plot3D.ResetBoxGraficoXYZ(DataRange, CInt(mX / 10), CInt(mY / 10), CInt(mZ / 10))
    'Dim displayedDataBounds = Plot3D.ResetBoxGraficoXYZ(DataRange, 0.5, 0.5, 0.5)
    Plot3D.AxesBox.XAxis1Title = VM.CurrentPlotSettings.XAxisChannelName
    Plot3D.AxesBox.YAxis1Title = VM.CurrentPlotSettings.YAxisChannelName
    Plot3D.AxesBox.ZAxis1Title = VM.CurrentPlotSettings.ZAxisChannelName
    Plot3D.PlottaGraficoXYZ(VM.PuntiXYZ, displayedDataBounds, DataRange, 0.3)
  End Sub


  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    'Dim ImgTmp = Plot.ExportToBitmapSource()
    VM.SaveImage(Plot)
  End Sub

  Private Sub CopyFullDataTableToClipboard()
    If Not VM.SubSet Is Nothing Then VM.SubSet.CopyTableToClipboard(True)
  End Sub

  Private Sub CopyTrendLinesDataToClipboard()
    If Not VM.SubSet Is Nothing Then VM.EsportaTrendLines(True)
  End Sub

  Private Sub CopyStatisticsToClipboard()
    If Not VM.SubSet Is Nothing Then VM.SubSet.CopyStatisticsToClipboard(True)
  End Sub

  Private Sub Plot_MouseMove(sender As Object, e As MouseEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If DataProvider2020.TimeStamps.Count = 0 Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    Dim p As Point = e.GetPosition(e.Source)
    Dim Dx As Double = p.X / Plot.RenderSurface.ActualWidth ' percentuale di dove sei con il cursore rispetto all asse x
    Dim Dy As Double = p.Y / Plot.RenderSurface.ActualHeight ' percentuale di dove sei con il cursore rispetto all asse x
    Dx = CDbl(Plot.XAxes(0).VisibleRange.Min) + CDbl(Plot.XAxes(0).VisibleRange.Diff) * Dx
    Dy = CDbl(Plot.YAxes(0).VisibleRange.Max) - CDbl(Plot.YAxes(0).VisibleRange.Diff) * Dy
    VM.CursorPosition = New clsDoubleXY(Dx, Dy)
  End Sub

  Private Sub DataPointSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)
    Dim Selezione As ObjectModel.ObservableCollection(Of SciChart.Charting.ChartModifiers.DataPointInfo) = DirectCast(sender, SciChart.Charting.ChartModifiers.DataPointSelectionModifier).SelectedPointMarkers

    SelectedPoints.Clear()
    If Not Selezione Is Nothing Then
      For Each S In Selezione
        If S.DataPointIndex > -1 Then
          SelectedPoints.Add(DirectCast(S.DataPointMetadata, clsPuntoMetadata).Momento)
        End If
      Next
    End If
    VM.PuntiSelezionati = SelectedPoints.Count
    VM.AggiornaPeriodiSelezionati()
  End Sub


  Private Sub Plot_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    Plot.ZoomExtents()
    Plot.XAxes(1).VisibleRange = Plot.XAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
    Plot.XAxes(2).VisibleRange = Plot.YAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
    Plot.YAxes(1).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
    Plot.YAxes(2).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  End Sub

  Private Sub TextBlock_MouseUp(sender As Object, e As MouseButtonEventArgs)
    Dim p = DirectCast(sender.datacontext, clsPeriod2021)
    Dim IR As New SciChart.Data.Model.DateRange(p.TR.Start, p.TR.Finish)
    DataPlotSync.SharedXVisibleRange = IR
    GraficoEventiViewModel.AggiornaSelezione(p.TR.Clone)
    MapControl.AggiornaSelezione(p.TR)
  End Sub

  Private Sub btn_ZoomExtents_Click(sender As Object, e As RoutedEventArgs)
    Plot.ZoomExtents()
  End Sub

  Private Sub btn_YExtends_Click(sender As Object, e As RoutedEventArgs)
    Plot.ZoomExtentsY()
  End Sub

  Private Sub btn_YzoomIn_Click(sender As Object, e As RoutedEventArgs)
    Plot.YAxes.First.ZoomBy(-0.1, -0.1)
  End Sub

  Private Sub btn_YzoomOut_Click(sender As Object, e As RoutedEventArgs)
    Plot.YAxes.First.ZoomBy(0.1, 0.1)
  End Sub


  Private Sub Plot_PreviewMouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
    VM.AggiornaValoriCursore(DirectCast(Plot.XAxes.First.VisibleRange.Min, Double), DirectCast(Plot.XAxes.First.VisibleRange.Max, Double), DirectCast(Plot.YAxes.First.VisibleRange.Min, Double), DirectCast(Plot.YAxes.First.VisibleRange.Max, Double))
  End Sub

  Private Sub Label_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    VM.CursorStats = ""
  End Sub

  'Private Sub Plot_Rendered(sender As Object, e As EventArgs)
  '  ' evento che si scatena quando il plot e'stato disegnato
  '  ' in realta'serve altro tempo perche'la superficie non e'ancora aggiornata
  '  ' quindi avvia un timer che ritarda di tm.interval millisecondi, l'esecuzione della verifica della macro pdf
  '  ' lasciando il tempo alla superficie di aggiornarsi e quindi di avere il plot completo
  '  tm.Start()
  'End Sub




  '' Sostituisce Plot_Rendered + tm + tm_Elapsed
  'Private Sub Plot_Rendered(sender As Object, e As EventArgs)
  '  ' Ci si iscrive UNA SOLA VOLTA al composition pass di WPF,
  '  ' che garantisce che i pixel siano effettivamente pronti
  '  AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  'End Sub

  ''Private Sub OnCompositionReady(sender As Object, e As EventArgs)
  ''  ' Rimuovi subito l'handler: vogliamo ONE-SHOT, non ogni frame
  ''  RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady

  ''  ' Verifica aggiuntiva: SciChart non deve essere in stato sospeso
  ''  If Plot.IsSuspended Then
  ''    ' Se per qualche motivo è ancora sospeso, rimanda al prossimo frame
  ''    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  ''    Return
  ''  End If

  ''  VM.VerificaMacroPdf(Plot)
  ''End Sub



  'Private Sub OnCompositionReady(sender As Object, e As EventArgs)
  '  RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady

  '  If Plot.IsSuspended Then
  '    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '    Return
  '  End If

  '  ' Agisce SOLO se c'e' una stampa PDF in corso
  '  If VM.XyReports IsNot Nothing AndAlso
  '     VM.XyReports.ActiveReport IsNot Nothing AndAlso
  '     VM.XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay Then
  '    VM.VerificaMacroPdf(Plot)
  '  End If
  'End Sub



  'Private _StampaInCorso As Boolean = False

  'Private Sub Plot_Rendered(sender As Object, e As EventArgs)
  '  ' Scatta ad ogni render: interessa solo durante stampa PDF
  '  If VM.XyReports Is Nothing Then Return
  '  If VM.XyReports.ActiveReport Is Nothing Then Return
  '  If VM.XyReports.ActiveReport.Status <> clsSciChartXyReport.eStatus.ePlay Then Return
  '  ' Evita re-entri: se VerificaMacroPdf e' gia' in esecuzione non accodare
  '  If _StampaInCorso Then Return
  '  AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  'End Sub

  'Private Sub OnCompositionReady(sender As Object, e As EventArgs)
  '  RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady

  '  If Plot.IsSuspended Then
  '    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '    Return
  '  End If

  '  _StampaInCorso = True
  '  Try
  '    VM.VerificaMacroPdf(Plot)
  '  Finally
  '    _StampaInCorso = False
  '  End Try
  'End Sub



  'Private Sub Plot_Rendered(sender As Object, e As EventArgs)
  '  If VM.XyReports Is Nothing Then Return
  '  If VM.XyReports.ActiveReport Is Nothing Then Return
  '  If VM.XyReports.ActiveReport.Status <> clsSciChartXyReport.eStatus.ePlay Then Return
  '  If _StampaInCorso Then Return   ' blocca re-entri inclusi quelli da ShowDialog
  '  AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  'End Sub

  'Private Sub OnCompositionReady(sender As Object, e As EventArgs)
  '  RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '  If Plot.IsSuspended Then
  '    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '    Return
  '  End If
  '  If _StampaInCorso Then Return   ' doppia protezione
  '  _StampaInCorso = True
  '  ' NON usare Try/Finally: il flag deve restare True
  '  ' anche durante ShowDialog dentro StampaReportXY
  '  VM.VerificaMacroPdf(Plot)
  '  ' Abbassa il flag SOLO dopo che VerificaMacroPdf e' completamente tornato,
  '  ' incluso il ritorno di ShowDialog
  '  _StampaInCorso = False
  'End Sub

  Private _StampaInCorso As Boolean = False
  'Private _RenderAtteso As Boolean = False

  'Private Sub Plot_Rendered(sender As Object, e As EventArgs)
  '  If Not VM.RenderAtteso Then Return  ' ignora tutti i render non richiesti
  '  If VM.XyReports Is Nothing Then Return
  '  If VM.XyReports.ActiveReport Is Nothing Then Return
  '  If VM.XyReports.ActiveReport.Status <> clsSciChartXyReport.eStatus.ePlay Then Return
  '  VM.RenderAtteso = False  ' consuma il token: il prossimo render verrà ignorato
  '  AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  'End Sub

  'Private Sub OnCompositionReady(sender As Object, e As EventArgs)
  '  RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '  If Plot.IsSuspended Then
  '    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  '    Return
  '  End If
  '  VM.VerificaMacroPdf(Plot)
  'End Sub


  Private Sub Plot_Rendered(sender As Object, e As EventArgs)
    If Not VM.RenderAtteso Then Return
    If VM.XyReports Is Nothing Then Return
    If VM.XyReports.ActiveReport Is Nothing Then Return
    If VM.XyReports.ActiveReport.Status <> clsSciChartXyReport.eStatus.ePlay Then Return
    VM.RenderAtteso = False  ' consuma il token
    AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
  End Sub

  Private Sub OnCompositionReady(sender As Object, e As EventArgs)
    RemoveHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
    If Plot.IsSuspended Then
      AddHandler CompositionTarget.Rendering, AddressOf OnCompositionReady
      Return
    End If
    VM.VerificaMacroPdf(Plot)
  End Sub

  Private Sub btn_Macro_Click(sender As Object, e As RoutedEventArgs)
    VM.AggiungiPlotToCurrentReport()
  End Sub

  'Private Sub tm_Elapsed(sender As Object, e As ElapsedEventArgs) Handles tm.Elapsed
  '  If Not Plot.IsSuspended Then
  '    tm.Stop()
  '    Dispatcher.Invoke(Sub()
  '                        VM.VerificaMacroPdf(Plot)
  '                      End Sub)
  '  End If
  'End Sub

  ''' <summary>
  ''' Prepara la lista canali e le impostazioni (calcola anche i canali math di X/Y/Color, es. SailingState) senza
  ''' aspettare il primo clic nel controllo: MainWindow la lancia a priorita' idle appena il tab XY e' mostrato.
  ''' Idempotente: se la lista e' gia' pronta non fa nulla.
  ''' </summary>
  Public Sub PreparaListeCanali()
    If VM Is Nothing OrElse VM.CurrentPlotSettings Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then Exit Sub
    Using clsLogTempi.Misura("XY PreparaListeCanali (anticipata, idle)")
      VM.CurrentPlotSettings.VerificaImpostaListe()
    End Using
  End Sub

  Private Sub UserControl_GotFocus(sender As Object, e As RoutedEventArgs)
    If VM.CurrentPlotSettings Is Nothing Then Exit Sub
    Using clsLogTempi.Misura("XY UserControl_GotFocus: VerificaImpostaListe")
      VM.CurrentPlotSettings.VerificaImpostaListe()
    End Using
  End Sub


  Private Sub SeriesSelectionModifier_SelectionChanged(sender As Object, e As EventArgs)

  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    VM.AddNewEmptyReport()
  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    XYplotDataTables.Reset()
    VM.AvviaPrint2Pdf = True
    MainTabControl.SelectedItem = PlotTab
  End Sub

  Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)
    VM.MuoviPlotCorrente(clsSciChartXyPlotViewModel.eDirezione.eTop)
  End Sub

  Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)
    VM.MuoviPlotCorrente(clsSciChartXyPlotViewModel.eDirezione.eUp)
  End Sub

  Private Sub Button_Click_6(sender As Object, e As RoutedEventArgs)
    VM.MuoviPlotCorrente(clsSciChartXyPlotViewModel.eDirezione.eDn)
  End Sub

  Private Sub Button_Click_7(sender As Object, e As RoutedEventArgs)
    VM.MuoviPlotCorrente(clsSciChartXyPlotViewModel.eDirezione.eBottom)
  End Sub

  Private Sub Button_Click_8(sender As Object, e As RoutedEventArgs)
    VM.EliminaPlotCorrente()
  End Sub

  Private Sub Button_Click_9(sender As Object, e As RoutedEventArgs)
    VM.MuoviReportCorrente(clsSciChartXyPlotViewModel.eDirezione.eTop)
  End Sub

  Private Sub Button_Click_10(sender As Object, e As RoutedEventArgs)
    VM.MuoviReportCorrente(clsSciChartXyPlotViewModel.eDirezione.eUp)
  End Sub

  Private Sub Button_Click_11(sender As Object, e As RoutedEventArgs)
    VM.MuoviReportCorrente(clsSciChartXyPlotViewModel.eDirezione.eDn)
  End Sub

  Private Sub Button_Click_12(sender As Object, e As RoutedEventArgs)
    VM.MuoviReportCorrente(clsSciChartXyPlotViewModel.eDirezione.eBottom)
  End Sub

  Private Sub Button_Click_13(sender As Object, e As RoutedEventArgs)
    VM.EliminaReportCorrente()
  End Sub

  Private Sub TextBox_MouseLeave(sender As Object, e As MouseEventArgs)
    VM.SalvaReportConfig()
  End Sub

  Private Sub Button_Click_14(sender As Object, e As RoutedEventArgs)
    VM.DuplicateReportSameChannelsAndCurrentSettings()
  End Sub


  'Private Sub PlotTab_GotFocus(sender As Object, e As RoutedEventArgs) Handles PlotTab.GotFocus
  '  ' quando prende il focus se c'e'la richiesta di stampa in pdf allora avvia il timer della stampa
  '  If VM.AvviaPrint2Pdf Then
  '    VM.IsReport = True
  '    tmavviopdf.Interval = 1000
  '    'XYplotDataTables.Reset()
  '    'XYplotDataTables.SetMaxMin()
  '    'Clipboard.SetText(XYplotDataTables.GetCsvTable())
  '    tmavviopdf.Start()
  '  ElseIf VM.ShowReportsPlot Then ' in questo caso e'sttao premuto il tasto show report plot
  '    VM.IsReport = True
  '    tmavvioshowplot.Start()
  '  Else 'tutti gli altri casi non fa nulla e setta a false la variabile IsReport
  '    VM.IsReport = False
  '  End If
  'End Sub


  Private Sub PlotTab_GotFocus(sender As Object, e As RoutedEventArgs) Handles PlotTab.GotFocus

    If VM.AvviaPrint2Pdf Then
      VM.IsReport = True
      ' DispatcherPriority.Loaded = dopo layout e render del tab, prima che sia "idle"
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, Sub()
                                                          VM.AvviaStampaPdfReport(Plot)
                                                        End Sub)

    ElseIf VM.ShowReportsPlot Then
      VM.IsReport = True
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, Sub()
                                                          VM.VisualizzaReportsPlot(Plot)
                                                        End Sub)

    Else
      VM.IsReport = False
    End If

  End Sub

  'Private Sub tmavviopdf_Elapsed(sender As Object, e As ElapsedEventArgs) Handles tmavviopdf.Elapsed
  '  tmavviopdf.Stop()
  '  Dispatcher.Invoke(Sub()
  '                      VM.AvviaStampaPdfReport(Plot)
  '                    End Sub)
  'End Sub

  'Private Sub tmavvioshowplot_Elapsed(sender As Object, e As ElapsedEventArgs) Handles tmavvioshowplot.Elapsed
  '  tmavvioshowplot.Stop()
  '  Dispatcher.Invoke(Sub()
  '                      VM.VisualizzaReportsPlot(Plot)
  '                    End Sub)
  'End Sub

  Private Sub Button_Click_15(sender As Object, e As RoutedEventArgs)
    AggiornaGrafico3D()
  End Sub

  Private Sub Button_Click_16(sender As Object, e As RoutedEventArgs)
    VM.ShowReportsPlot = True
    MainTabControl.SelectedItem = PlotTab
  End Sub

  Private Shared ReadOnly NumericPattern As New Regex("^-?\d*(\.\d*)?$")


  Private Sub TextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)

    Dim tb = CType(sender, TextBox)
    Dim ch As String = If(e.Text = ",", ".", e.Text) ' normalizza virgola → punto

    ' testo risultante dopo l'input (considera selezione e caret)
    Dim start = tb.SelectionStart
    Dim length = tb.SelectionLength
    Dim before = If(start > 0, tb.Text.Substring(0, start), String.Empty)
    Dim after = If(start + length < tb.Text.Length, tb.Text.Substring(start + length), String.Empty)
    Dim candidate = before & ch & after

    If IsValidNumberText(candidate) Then
      ' inseriamo noi per evitare che altri handler rimuovano il punto
      tb.Text = candidate
      tb.SelectionStart = before.Length + ch.Length
    End If

    e.Handled = True ' sempre gestito qui

    'Dim tb = CType(sender, TextBox)

    '' normalizza: virgola -> punto
    'Dim ch As String = If(e.Text = ",", ".", e.Text)

    '' testo come sarà dopo l’input (considera selezione e caret)
    'Dim start = tb.SelectionStart
    'Dim length = tb.SelectionLength
    'Dim before = If(start > 0, tb.Text.Substring(0, start), String.Empty)
    'Dim after = If(start + length < tb.Text.Length, tb.Text.Substring(start + length), String.Empty)
    'Dim candidate = before & ch & after

    '' valida
    'If NumericPattern.IsMatch(candidate) Then
    '  ' gestiamo NOI l’inserimento per evitare che altri handler lo cancellino
    '  tb.Text = candidate
    '  tb.SelectionStart = before.Length + ch.Length
    '  e.Handled = True
    'Else
    '  e.Handled = True ' blocca caratteri non validi
    'End If


    'Dim tb = CType(sender, TextBox)
    'Dim ch As String = If(e.Text = ",", ".", e.Text)

    'Dim start = tb.SelectionStart
    'Dim length = tb.SelectionLength
    'Dim before = If(start > 0, tb.Text.Substring(0, start), "")
    'Dim after = If(start + length < tb.Text.Length, tb.Text.Substring(start + length), "")
    'Dim candidate = before & ch & after

    'If Regex.IsMatch(candidate, "^-?\d*\.?\d*$") Then
    '  If e.Text = "," Then
    '    tb.Text = candidate
    '    tb.SelectionStart = start + 1
    '    e.Handled = True
    '  Else
    '    e.Handled = False
    '  End If
    'Else
    '  e.Handled = True
    'End If

    'Dim Rx As Regex = New Regex("[^0-9.-]+")
    'e.Handled = Rx.IsMatch(e.Text)
  End Sub

  Private Function IsValidNumberText(s As String) As Boolean
    If s Is Nothing Then Return False
    If s = "" OrElse s = "-" OrElse s = "." OrElse s = "-." Then Return True

    ' solo caratteri ammessi
    For Each c In s
      If Not (Char.IsDigit(c) OrElse c = "."c OrElse c = "-"c) Then
        Return False
      End If
    Next

    ' "-" al massimo una volta e solo in posizione 0
    Dim minusCount = s.Count(Function(c) c = "-"c)
    If minusCount > 1 Then Return False
    If minusCount = 1 AndAlso s.IndexOf("-"c) <> 0 Then Return False

    ' "." al massimo una volta
    If s.Count(Function(c) c = "."c) > 1 Then Return False

    ' tutto ok (consente "1.", ".5", "-.3", "-1.23", ecc.)
    Return True
  End Function


  ' (consigliato) consenti backspace, delete, frecce ecc.
  Private Sub TxtNumber_PreviewKeyDown(sender As Object, e As KeyEventArgs)
    Select Case e.Key
      Case Key.Back, Key.Delete, Key.Left, Key.Right, Key.Tab, Key.Home, Key.End
        ' lascia passare
      Case Else
        ' niente: l’input testuale lo gestiamo in PreviewTextInput
    End Select
  End Sub

  ' (opzionale) normalizza/incolla e valida anche nel paste
  Private Sub TxtNumber_Pasting(sender As Object, e As DataObjectPastingEventArgs)
    If Not e.DataObject.GetDataPresent(GetType(String)) Then
      e.CancelCommand()
      Return
    End If

    Dim tb = CType(sender, TextBox)
    Dim pasteText As String = CStr(e.DataObject.GetData(GetType(String))).Replace(","c, "."c)

    Dim start = tb.SelectionStart
    Dim length = tb.SelectionLength
    Dim before = If(start > 0, tb.Text.Substring(0, start), String.Empty)
    Dim after = If(start + length < tb.Text.Length, tb.Text.Substring(start + length), String.Empty)
    Dim candidate = before & pasteText & after

    If IsValidNumberText(candidate) Then
      tb.Text = candidate
      tb.SelectionStart = before.Length + pasteText.Length
    End If

    e.CancelCommand() ' blocca incolla di default
  End Sub

  Private Sub TextBox_PreviewTextInputInt(sender As Object, e As TextCompositionEventArgs)
    Dim Rx As Regex = New Regex("[^0-9]+")
    Dim IsNumeric As Boolean = Rx.IsMatch(e.Text)
    e.Handled = IsNumeric
  End Sub
  Private Sub TextBox_PreviewTextInputByte(sender As Object, e As TextCompositionEventArgs)
    Dim Rx As Regex = New Regex("[^0-9]+-")
    Dim IsNumeric As Boolean = Rx.IsMatch(e.Text)
    e.Handled = IsNumeric AndAlso Double.Parse(e.Text) <= 255
  End Sub

  Private Sub Button_Click_17(sender As Object, e As RoutedEventArgs)

  End Sub

  ''' <summary>Rende le combo dei canali X e Y editabili con filtro di ricerca.</summary>
  Private Sub Combo_Canali_Loaded(sender As Object, e As RoutedEventArgs)
    clsComboFiltro.Attiva(DirectCast(sender, ComboBox))
  End Sub

  ''' <summary>Doppio click sull'etichetta X: imposta la True Wind Speed come canale dell'asse X.</summary>
  Private Sub Label_X_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
    If VM Is Nothing OrElse VM.CurrentPlotSettings Is Nothing OrElse VM.CurrentPlotSettings.AvailableChannels Is Nothing Then Exit Sub
    Dim Tws = VM.CurrentPlotSettings.AvailableChannels.FirstOrDefault(Function(c) c.CanaleChiave = clsChannels2020.eCanaliChiave.eTWS)
    If Not Tws Is Nothing Then VM.CurrentPlotSettings.XAxisChannel = Tws
  End Sub

  Private Sub Button_Click_SwapAxes(sender As Object, e As RoutedEventArgs)
    Dim X = VM.CurrentPlotSettings.XAxisChannel
    Dim Y = VM.CurrentPlotSettings.YAxisChannel

    VM.CurrentPlotSettings.XAxisChannel = Y
    VM.CurrentPlotSettings.YAxisChannel = X

  End Sub

  Private Sub btn_OpenDataFolder_Click(sender As Object, e As RoutedEventArgs)
    ApriExplorer(AppConfig.ActiveProfile.LastPpfFolder)
  End Sub

  Private Sub btn_NextReportPlot_Click(sender As Object, e As RoutedEventArgs)
    VM.VisualizzaPrevNextReportsPlot(Plot, True)
  End Sub

  Private Sub btn_PrevReportPlot_Click(sender As Object, e As RoutedEventArgs)
    VM.VisualizzaPrevNextReportsPlot(Plot, False)
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsSciChartXyReports
  Public Property Reports As New ObservableCollection(Of clsSciChartXyReport)

  <JsonIgnore>
  Public Property ActiveReport As clsSciChartXyReport

  Public Function ElininaCurrentReport() As Boolean
    If MessageBox.Show("Do you really want to delete Report: '" & ActiveReport.Name & "' ?", "", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
      Reports.Remove(ActiveReport)
      ActiveReport = Nothing
      Return True
    End If
    Return False
  End Function

  Public Function DuplicateReportSameChannelsAndCurrentSettings(CurrentSettings As clsXYPlotSettings) As Boolean
    If ActiveReport Is Nothing Then Return False
    Dim NomeTmp As String = InputBox("Name of the new report ", "Report Manager", "Copy of " & ActiveReport.Name)
    If Not NomeTmp.Trim = "" Then
      Dim NewReport As New clsSciChartXyReport()
      NewReport.Name = NomeTmp
      For Each p In ActiveReport.Plots
        Dim ptmp = CurrentSettings.Clone
        ptmp.XAxisChannel = p.XAxisChannel
        ptmp.XAxisChannelName = p.XAxisChannelName
        ptmp.YAxisChannel = p.YAxisChannel
        ptmp.YAxisChannelName = p.YAxisChannelName
        NewReport.Plots.Add(ptmp)
      Next
      Reports.Add(NewReport)
      ActiveReport = NewReport
      Return True
    End If
    Return False
  End Function


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsSciChartXyReport
  Public Property Status As eStatus = eStatus.eOff
  Public Property Name As String
  Public Property PlotsXpage As Integer = 3
  Public Property ColsXpage As Integer = 1

  Public Property Plots As New ObservableCollection(Of clsXYPlotSettings)

  Dim _ActivePlot As clsXYPlotSettings
  <JsonIgnore>
  Public Property ActivePlot As clsXYPlotSettings
    Get
      Return _ActivePlot
    End Get
    Set(value As clsXYPlotSettings)
      _ActivePlot = value
      If Not value Is Nothing Then
        ActivePlot.ImpostaCanaliDaNomi()
      End If
    End Set
  End Property

  Public Enum eStatus
    eOff = 0
    eRec = 1
    ePlay = 2
  End Enum

  Public Function EliminaCurrentPlot() As Boolean
    If MessageBox.Show("Do you really want to delete Plot: '" & ActivePlot.DescriptionShort & "' ?", "", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
      Plots.Remove(ActivePlot)
      ActivePlot = Nothing
      Return True
    End If
    Return False
  End Function


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsSciChartXyPlotViewModel



  Public Property XyReports As clsSciChartXyReports
  Public Property AvviaPrint2Pdf As Boolean = False
  Public Property ShowReportsPlot As Boolean = False

  Public Property CurrentPlotSettings As clsXYPlotSettings
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property Titolo As String
  Public Property SottoTitolo As String
  Public Property XLabel As String
  Public Property YLabel As String
  Public Property Lista As ObservableCollection(Of clsPeriod2021)
  Public Property TR As clsTimeRange
  Public Property DataPointSelEnabled As Boolean = True
  Public Property PuntiSelezionati As Integer
  Public Property SelectedPeriods As New ObservableCollection(Of clsPeriod2021)
  'Public Property CollectImages As Boolean
  Public Property ImagesCollection As New List(Of System.IO.Stream)
  Public Property ImageDataCollection As New List(Of String)
  Public Property FirstDataLoad As Boolean = True
  Public Property SubSet As clsDataSubSet
  Public Property StatisticheCursore As New clsValoriCursore
  Public Property TL As New clsTrendLines
  Public Property TLNotSel As New clsTrendLines

  Public Property PuntiXYZ As New List(Of clsXYZColore)

  Dim _CursorPosition As clsDoubleXY
  Public Property CursorPosition As clsDoubleXY
    Get
      Return _CursorPosition
    End Get
    Set(value As clsDoubleXY)
      _CursorPosition = value
      CursorPos = "X: " & CursorPosition.X.ToString("F2") & ", Y: " & CursorPosition.Y.ToString("F2")
      'CursorStats = "Xavg: " & StatisticheCursore.AvgY.ToString("F2") & ", Yavg: " & StatisticheCursore.AvgX.ToString("F2")
    End Set
  End Property


  Public ReadOnly Property HasPeriods As Boolean
    Get
      Return Not (_Lista Is Nothing)
    End Get
  End Property

  Public ReadOnly Property ZoomPanEnabled As Boolean
    Get
      Return Not DataPointSelEnabled
    End Get
  End Property

  Public ReadOnly Property SelEnabledText As String
    Get
      If DataPointSelEnabled Then
        Return "Select Mode"
      Else
        Return "ZoomPan Mode"
      End If
    End Get
  End Property

  Public ReadOnly Property SelectedPeriodsVisible As Visibility
    Get
      If _SelectedPeriods Is Nothing Then Return Visibility.Collapsed
      Return IIf(_SelectedPeriods.Count > 0, Visibility.Visible, Visibility.Collapsed)
    End Get
  End Property

  Dim _CollectImages As Boolean
  Public Property CollectImages As Boolean
    Get
      Return _CollectImages
    End Get
    Set(value As Boolean)
      _CollectImages = value
      GestisciImagesCollector()
    End Set
  End Property

  Public Sub AggiornaPeriodiSelezionati()
    SelectedPeriods.Clear()
    If _Lista Is Nothing Then Exit Sub
    For Each Per In _Lista
      Per.IsSelected = False
      For Each m In SelectedPoints
        If Per.TR.InRange(m) Then
          Per.IsSelected = True
          SelectedPeriods.Add(Per)
          Exit For
        End If
      Next
    Next
  End Sub

  Private Sub GestisciImagesCollector()
    If _CollectImages Then
      ImagesCollection.Clear()
      'Else
      '  If XyReports.ActiveReport Is Nothing Then
      '    StampaPdfDaImagesCollection(2, 1, "XY Plots")
      '  Else
      '    StampaPdfDaImagesCollection(XyReports.ActiveReport.PlotsXpage, XyReports.ActiveReport.ColsXpage, XyReports.ActiveReport.Name)
      '  End If

    End If
  End Sub

  Private Sub StampaPdfDaImagesCollection(RowsPerPage As Integer, ColsPerPage As Integer, Intestazione As String)
    If ImagesCollection.Count > 0 Then
      Dim pdf As New clsPdf
      If pdf.StampaReportXY(ImagesCollection, RowsPerPage, ColsPerPage, Intestazione) Then
        Clipboard.SetText(String.Join(vbCrLf & vbCrLf & "######################" & vbCrLf & vbCrLf, _ImageDataCollection))
        MsgBox("Charts Statistics Copied to Clipboard")
      End If
      ' Clear sempre, sia OK che Cancel: evita accumulo su tentativi successivi
      ImagesCollection.Clear()
      _ImageDataCollection.Clear()
    End If
  End Sub


  Public Sub SaveImage(Surface As SciChart.Charting.Visuals.SciChartSurface)
    If _CollectImages Then
      ImagesCollection.Add(Surface.ExportToStream(SciChart.Core.ExportType.Bmp, False))
      If Not SubSet Is Nothing Then
        _ImageDataCollection.Add(EsportaTrendLines(False) & vbCrLf & vbCrLf & vbCrLf & vbCrLf & SubSet.CopyStatisticsToClipboard(False))
      End If
    Else
      Try
        Dim Image = Surface.ExportToBitmapSource()
        Clipboard.SetImage(Image)
      Catch ex As Exception
        Stop
      End Try
      MsgBox("Chart Copied to Clipboard")
    End If

  End Sub

  Public Enum eLoadedDataMouseMode
    eSelection = 0
    ePan = 1
  End Enum


  Public Sub AggiornaValoriCursore(XaxisMin As Double, XaxisMax As Double, YaxisMin As Double, YaxisMax As Double)
    Try
      Dim Range As Double = 0.02 ' 2%
      Dim x As New List(Of Double)
      Dim y As New List(Of Double)
      Dim xRange, yRange As Double
      For Each s In SeriesSource
        xRange = Math.Max(xRange, CDbl(s.DataSeries.XRange.Diff))
        yRange = Math.Max(yRange, CDbl(s.DataSeries.YRange.Diff))
      Next
      ' devo implementare con il range dettato dal visible range
      ' non sarebbe male anche per tipo di visualizzazione tipo stbd portup down etc etc
      Dim xMinVal As Double = CursorPosition.X - (xRange * Range) ' range del X percento
      Dim xMaxVal As Double = CursorPosition.X + (xRange * Range)
      Dim yMinVal As Double = CursorPosition.Y - (yRange * Range) ' range del X percento
      Dim yMaxVal As Double = CursorPosition.Y + (yRange * Range)

      Dim vseries = SeriesSource.Where(Function(xx) Not xx.DataSeries.SeriesName.StartsWith("TrndLn") AndAlso Not xx.DataSeries.SeriesName.StartsWith("Tgt")).ToList
      Dim strPosStat As String = ""
      Dim SingleStats As Boolean = vseries.Count < 6
      For Each s In vseries
        If Not s.DataSeries.SeriesName.StartsWith("TrndLn") Then
          For i As Integer = 0 To s.DataSeries.XValues.Count - 1
            Dim v As Double = s.DataSeries.XValues(i)
            If v >= xMinVal AndAlso v <= xMaxVal Then
              Dim vv As Double = s.DataSeries.YValues(i)
              If vv >= YaxisMin AndAlso vv <= YaxisMax Then
                x.Add(vv)
              End If
            End If
          Next
          For i As Integer = 0 To s.DataSeries.YValues.Count - 1
            Dim v As Double = s.DataSeries.YValues(i)
            If v >= yMinVal AndAlso v <= yMaxVal Then
              Dim vv As Double = s.DataSeries.XValues(i)
              If vv >= XaxisMin AndAlso vv <= XaxisMax Then
                y.Add(vv)
              End If
              'y.Add(s.DataSeries.XValues(i))
            End If
          Next
          If SingleStats Then
            StatisticheCursore.AggiornaStats(x.ToArray, y.ToArray, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
            strPosStat &= s.DataSeries.SeriesName & ": Xavg: " & StatisticheCursore.AvgY.ToString("F2") & ", Yavg: " & StatisticheCursore.AvgX.ToString("F2") & "  ,  "
            x.Clear()
            y.Clear()
          End If
        End If
      Next
      If Not SingleStats Then
        StatisticheCursore.AggiornaStats(x.ToArray, y.ToArray, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
        strPosStat = "Xavg: " & StatisticheCursore.AvgY.ToString("F2") & ", Yavg: " & StatisticheCursore.AvgX.ToString("F2")
      End If
      If Not CursorPosition Is Nothing Then
        CursorPos = "X: " & CursorPosition.X.ToString("F2") & ", Y: " & CursorPosition.Y.ToString("F2")
        'CursorStats = "Xavg: " & StatisticheCursore.AvgY.ToString("F2") & ", Yavg: " & StatisticheCursore.AvgX.ToString("F2")
      End If
      CursorStats = strPosStat
    Catch ex As Exception

    End Try

  End Sub

  Dim _CursorStats As String
  Public Property CursorStats As String
    Get
      Return _CursorStats
    End Get
    Set(value As String)
      _CursorStats = value
    End Set
  End Property

  Dim _CursorPos As String
  Public Property CursorPos As String
    Get
      Return _CursorPos
    End Get
    Set(value As String)
      _CursorPos = value
    End Set
  End Property

  Public Sub SalvaReportConfig()
    AppConfig.ActiveProfile.XyReports = XyReports
    AppConfig.Salva()
  End Sub

  Public Sub AggiungiPlotToCurrentReport()
    VerificaNuovoReport()
    If Not XyReports.ActiveReport Is Nothing Then
      XyReports.ActiveReport.Plots.Add(CurrentPlotSettings.Clone)
      SalvaReportConfig()
    End If
  End Sub

  Public Sub AddNewEmptyReport()
    Dim NomeTmp As String = InputBox("No active report selected, a new one will be created, report's name: ", "Report Manager", "New Report")
    If Not NomeTmp.Trim = "" Then
      XyReports.ActiveReport = New clsSciChartXyReport()
      XyReports.ActiveReport.Name = NomeTmp
      XyReports.Reports.Add(XyReports.ActiveReport)
      SalvaReportConfig()
    End If

  End Sub

  Public Sub DuplicateReportSameChannelsAndCurrentSettings()
    XyReports.DuplicateReportSameChannelsAndCurrentSettings(CurrentPlotSettings)
    SalvaReportConfig()
  End Sub

  Public Sub VerificaNuovoReport()
    If XyReports Is Nothing Then
      XyReports = AppConfig.ActiveProfile.XyReports
      If XyReports Is Nothing Then
        XyReports = New clsSciChartXyReports
      End If
    End If
    If XyReports.ActiveReport Is Nothing Then
      Dim NomeTmp As String = InputBox("No active report selected, a new one will be created, report's name: ", "Report Manager", "New Report")
      If Not NomeTmp.Trim = "" Then
        XyReports.ActiveReport = New clsSciChartXyReport()
        XyReports.ActiveReport.Name = NomeTmp
        XyReports.Reports.Add(XyReports.ActiveReport)
      End If
    End If
    If Not XyReports.ActiveReport Is Nothing Then
      SalvaReportConfig()
    End If
  End Sub

  Public Sub EliminaReportCorrente()
    'Stop
    If XyReports Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then Exit Sub
    If XyReports.ElininaCurrentReport() Then
      SalvaReportConfig()
    End If
  End Sub

  Public Sub EliminaPlotCorrente()
    If XyReports Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then Exit Sub
    If XyReports.ActiveReport.ActivePlot Is Nothing Then Exit Sub
    If XyReports.ActiveReport.EliminaCurrentPlot() Then
      SalvaReportConfig()
    End If
  End Sub

  Public Sub MuoviPlotCorrente(Direzione As eDirezione)
    If XyReports Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then Exit Sub
    If XyReports.ActiveReport.ActivePlot Is Nothing Then Exit Sub
    Dim ipc = XyReports.ActiveReport.Plots.IndexOf(XyReports.ActiveReport.ActivePlot)
    Select Case Direzione
      Case eDirezione.eTop
        XyReports.ActiveReport.Plots.Move(ipc, 0)
      Case eDirezione.eUp
        Dim newipc = System.Math.Max(0, ipc - 1)
        XyReports.ActiveReport.Plots.Move(ipc, newipc)
      Case eDirezione.eDn
        Dim newipc = System.Math.Min(XyReports.ActiveReport.Plots.Count - 1, ipc + 1)
        XyReports.ActiveReport.Plots.Move(ipc, newipc)
      Case eDirezione.eBottom
        XyReports.ActiveReport.Plots.Move(ipc, XyReports.ActiveReport.Plots.Count - 1)
    End Select
    SalvaReportConfig()
  End Sub

  Public Sub MuoviReportCorrente(Direzione As eDirezione)
    If XyReports Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then Exit Sub
    Dim irc = XyReports.Reports.IndexOf(XyReports.ActiveReport)
    Select Case Direzione
      Case eDirezione.eTop
        XyReports.Reports.Move(irc, 0)
      Case eDirezione.eUp
        Dim newirc = System.Math.Max(0, irc - 1)
        XyReports.Reports.Move(irc, newirc)
      Case eDirezione.eDn
        Dim newirc = System.Math.Min(XyReports.Reports.Count - 1, irc + 1)
        XyReports.Reports.Move(irc, newirc)
      Case eDirezione.eBottom
        XyReports.Reports.Move(irc, XyReports.Reports.Count - 1)
    End Select
    SalvaReportConfig()
  End Sub

  Public Enum eDirezione
    eTop = 0
    eUp = 1
    eDn = 2
    eBottom = 3
  End Enum

  'Public Sub AvviaStampaPdfReport(Surface As SciChart.Charting.Visuals.SciChartSurface)
  '  'qui ci si arriva quando il tab prende il focus e la variabile AvviaPrint2Pdf e' su true
  '  If XyReports Is Nothing Then Exit Sub
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  AvviaPrint2Pdf = False
  '  If XyReports.ActiveReport Is Nothing Then
  '    XyReports.ActiveReport = XyReports.Reports.First
  '  End If
  '  XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay
  '  XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots.First
  '  CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
  '  _CollectImages = True
  '  'XYplotDataTables.Reset()
  '  'XYplotDataTables.SetMaxMin()
  '  'Clipboard.SetText(XYplotDataTables.GetCsvTable())
  '  AggiornaGrafico(Surface)
  '  IsReport = False
  'End Sub

  'Public Sub AvviaStampaPdfReport(Surface As SciChart.Charting.Visuals.SciChartSurface)
  '  If XyReports Is Nothing Then Exit Sub
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  AvviaPrint2Pdf = False
  '  If XyReports.ActiveReport Is Nothing Then
  '    XyReports.ActiveReport = XyReports.Reports.First
  '  End If
  '  XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay
  '  XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots.First
  '  CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
  '  _CollectImages = True
  '  IsReport = True
  '  ' NON chiama AggiornaGrafico(Surface) direttamente:
  '  ' chiama solo l'overload dati che popola SeriesSource,
  '  ' poi Plot_Rendered guidera' il resto
  '  _RenderAtteso = True  ' autorizza esattamente un render
  '  AggiornaGrafico(False)
  'End Sub
  ' Nel ViewModel clsSciChartXyPlotViewModel
  Public Property RenderAtteso As Boolean = False

  Public Sub AvviaStampaPdfReport(Surface As SciChart.Charting.Visuals.SciChartSurface)
    If XyReports Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    AvviaPrint2Pdf = False
    If XyReports.ActiveReport Is Nothing Then
      XyReports.ActiveReport = XyReports.Reports.First
    End If
    XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay
    XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots.First
    CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
    _CollectImages = True
    IsReport = True
    RenderAtteso = True   ' ← autorizza esattamente un render
    AggiornaGrafico(False)
  End Sub

  Public Sub VisualizzaReportsPlot(Surface As SciChart.Charting.Visuals.SciChartSurface)
    'qui ci si arriva quando il tab prende il focus e la variabile AvviaPrint2Pdf e'su false ma la variabile ShowReportsPlot e' su true
    ShowReportsPlot = False
    If XyReports Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    If XyReports.ActiveReport.ActivePlot Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then
      XyReports.ActiveReport = XyReports.Reports.First
    End If
    'XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay
    'XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots.First
    CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
    '_CollectImages = True
    AggiornaGrafico(Surface)
    IsReport = False
  End Sub

  Public Sub VisualizzaPrevNextReportsPlot(Surface As SciChart.Charting.Visuals.SciChartSurface, prossimo As Boolean)
    ' mostra il precedente o il successivo grafico del report corrente, se non c'e' report corrente allora esce, cosi'come se si trva gia' ad un estremo, ovvero non fa loop.
    For i As Integer = 0 To XyReports.ActiveReport.Plots.Count - 1
      If (XyReports.ActiveReport.Plots(i) Is XyReports.ActiveReport.ActivePlot) Then
        If prossimo Then
          If i < XyReports.ActiveReport.Plots.Count - 1 Then
            XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots(i + 1)
          End If
        Else
          If i > 0 Then
            XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots(i - 1)
          End If
        End If
        Exit For
      End If
    Next

    ShowReportsPlot = False
    If XyReports Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    If XyReports.ActiveReport.ActivePlot Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then
      XyReports.ActiveReport = XyReports.Reports.First
    End If
    CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
    AggiornaGrafico(Surface)
    IsReport = False
  End Sub


  Public Sub VerificaMacroPdf(Surface As SciChart.Charting.Visuals.SciChartSurface)
    If XyReports Is Nothing Then Exit Sub
    If XyReports.ActiveReport Is Nothing Then Exit Sub
    If XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.ePlay Then
      SaveImage(Surface)
      If XyReports.ActiveReport.ActivePlot Is XyReports.ActiveReport.Plots.Last Then
        'se l active plot e'l ultimo disattiva il timer della macro
        StampaPdfDaImagesCollection(XyReports.ActiveReport.PlotsXpage, XyReports.ActiveReport.ColsXpage, XyReports.ActiveReport.Name)
        XyReports.ActiveReport.Status = clsSciChartXyReport.eStatus.eOff
      Else
        Dim idx = XyReports.ActiveReport.Plots.IndexOf(XyReports.ActiveReport.ActivePlot)
        XyReports.ActiveReport.ActivePlot = XyReports.ActiveReport.Plots(idx + 1)
        CurrentPlotSettings = XyReports.ActiveReport.ActivePlot.Clone
        RenderAtteso = True  ' autorizza il prossimo render
        AggiornaGrafico(Surface)
      End If
    End If
  End Sub

  'Private Sub AggiornaGrafico(Plot As SciChart.Charting.Visuals.SciChartSurface)
  '  'qui ci si arriva quando:
  '  ' 1. PlotTab prende il focus e la variabile AvviaPrint2Pdf e' su true.
  '  ' 2. PlotTab prende il focus e la variabile AvviaPrint2Pdf e'su false ma la variabile ShowReportsPlot e' su true.
  '  ' 3. Viene premuto il tasto Prev o Next per mostrare il precedente o il successivo grafico del report corrente.
  '  ' 4. dopo l interval di del timer tm che viene scatenato alla fine di Plot_Rendered (DA RIVEDERE......)

  '  AggiornaGrafico(False)
  '  Plot.ZoomExtents()

  '  ' --- Range manuale assi principali (stesso del Button_Click) ---
  '  If CurrentPlotSettings.ApplyMinX Then
  '    Plot.XAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
  '          CurrentPlotSettings.MinX,
  '          CurrentPlotSettings.MaxX)
  '  End If
  '  If CurrentPlotSettings.ApplyMinY Then
  '    Plot.YAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
  '          CurrentPlotSettings.MinY,
  '          CurrentPlotSettings.MaxY)
  '  End If

  '  ' Assi secondari (invariato)
  '  Plot.XAxes(1).VisibleRange = Plot.XAxes(0).VisibleRange
  '  Plot.XAxes(2).VisibleRange = Plot.YAxes(0).VisibleRange
  '  Plot.YAxes(1).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  '  Plot.YAxes(2).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  'End Sub


  Private Sub AggiornaGrafico(Plot As SciChart.Charting.Visuals.SciChartSurface)
    '  'qui ci si arriva quando:
    '  ' 1. PlotTab prende il focus e la variabile AvviaPrint2Pdf e' su true.
    '  ' 2. PlotTab prende il focus e la variabile AvviaPrint2Pdf e'su false ma la variabile ShowReportsPlot e' su true.
    '  ' 3. Viene premuto il tasto Prev o Next per mostrare il precedente o il successivo grafico del report corrente.
    '  ' 4. dopo l interval di del timer tm che viene scatenato alla fine di Plot_Rendered (DA RIVEDERE......)

    AggiornaGrafico(False)
    Plot.ZoomExtents()
    If CurrentPlotSettings.ApplyMinX Then
      Plot.XAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
            CurrentPlotSettings.MinX, CurrentPlotSettings.MaxX)
    End If
    If CurrentPlotSettings.ApplyMinY Then
      Plot.YAxes(0).VisibleRange = New SciChart.Data.Model.DoubleRange(
            CurrentPlotSettings.MinY, CurrentPlotSettings.MaxY)
    End If
    Plot.XAxes(1).VisibleRange = Plot.XAxes(0).VisibleRange
    Plot.XAxes(2).VisibleRange = Plot.YAxes(0).VisibleRange
    Plot.YAxes(1).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
    Plot.YAxes(2).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  End Sub


  'Private Sub AggiornaGrafico(Plot As SciChart.Charting.Visuals.SciChartSurface)
  '  'qui ci si arriva quando:
  '  ' 1. PlotTab prende il focus e la variabile AvviaPrint2Pdf e' su true.
  '  ' 2. PlotTab prende il focus e la variabile AvviaPrint2Pdf e'su false ma la variabile ShowReportsPlot e' su true.
  '  ' 3. Viene premuto il tasto Prev o Next per mostrare il precedente o il successivo grafico del report corrente.
  '  ' 4. dopo l interval di del timer tm che viene scatenato alla fine di Plot_Rendered (DA RIVEDERE......)

  '  AggiornaGrafico(False)
  '  Plot.ZoomExtents()
  '  Plot.XAxes(1).VisibleRange = Plot.XAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
  '  Plot.XAxes(2).VisibleRange = Plot.YAxes(0).VisibleRange '  New SciChart.Data.Model.DoubleRange(-1, 6)
  '  Plot.YAxes(1).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  '  Plot.YAxes(2).VisibleRange = New SciChart.Data.Model.DoubleRange(0, 100)
  'End Sub

  Private Function TrovaCanaleDaChannelId(Canale As clsChannel2020) As clsChannel2020
    If Canale Is Nothing Then Return Nothing
    Return DataProvider2020.CanaleDbl(Canale.ChannelId)
  End Function

  Public IsReport As Boolean = False

  Public Sub New()

  End Sub

  Public Sub AggiornaGrafico(clearTables As Boolean)
    If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eNone Then Exit Sub
    If DataProvider2020 Is Nothing Then Exit Sub
    If clearTables Then XYplotDataTables.Tables.Clear()
    TL.ClearData()
    Dim x1 = TrovaCanaleDaChannelId(CurrentPlotSettings.XAxisChannel)
    Dim x2 = TrovaCanaleDaChannelId(CurrentPlotSettings.YAxisChannel)
    Dim x3 = TrovaCanaleDaChannelId(CurrentPlotSettings.ColorChannel)
    Dim x4 = TrovaCanaleDaChannelId(CurrentPlotSettings.SailingStateChannel)
    Dim x5 = TrovaCanaleDaChannelId(CurrentPlotSettings.FilterChannel)
    Dim x6 = TrovaCanaleDaChannelId(CurrentPlotSettings.Filter2Channel)
    Dim x7 = TrovaCanaleDaChannelId(CurrentPlotSettings.ZAxisChannel)

    ' --- I canali richiesti dal plot non esistono in questo dataset: niente da plottare ---
    If x1 Is Nothing OrElse x2 Is Nothing Then
      Dim manc As String = ""
      If x1 Is Nothing AndAlso CurrentPlotSettings.XAxisChannel IsNot Nothing Then manc = "X: " & CurrentPlotSettings.XAxisChannel.LongName
      If x2 Is Nothing AndAlso CurrentPlotSettings.YAxisChannel IsNot Nothing Then manc &= If(manc = "", "", " - ") & "Y: " & CurrentPlotSettings.YAxisChannel.LongName
      SottoTitolo = "Channel not available in this dataset (" & manc & ")"
      SeriesSource.Clear()
      Exit Sub
    End If
    'If Not IsReport AndAlso AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilter Then ' non deve essere in modalita' stampa pdf o mostra report e deve essere attivo il filtro automatico per VMC
    'Dim CheckVmc As Boolean = AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilter
    'If IsReport Then CheckVmc = CurrentPlotSettings.VmcAutoFilter
    'If CheckVmc Then ' deve essere attivo il filtro automatico per VMC
    '  Dim Vmc As Boolean = False
    '  Dim Mid As Integer = -1
    '  Select Case x2.CanaleChiave
    '    Case clsChannels2020.eCanaliChiave.eVmc40
    '      Mid = 40
    '    Case clsChannels2020.eCanaliChiave.eVmc50
    '      Mid = 50
    '    Case clsChannels2020.eCanaliChiave.eVmc60
    '      Mid = 60
    '    Case clsChannels2020.eCanaliChiave.eVmc70
    '      Mid = 70
    '    Case clsChannels2020.eCanaliChiave.eVmc80
    '      Mid = 80
    '    Case clsChannels2020.eCanaliChiave.eVmc90
    '      Mid = 90
    '    Case clsChannels2020.eCanaliChiave.eVmc100
    '      Mid = 100
    '    Case clsChannels2020.eCanaliChiave.eVmc110
    '      Mid = 110
    '    Case clsChannels2020.eCanaliChiave.eVmc120
    '      Mid = 120
    '    Case clsChannels2020.eCanaliChiave.eVmc130
    '      Mid = 130
    '    Case clsChannels2020.eCanaliChiave.eVmc140
    '      Mid = 140
    '    Case clsChannels2020.eCanaliChiave.eVmc150
    '      Mid = 150
    '  End Select
    '  If (Mid > -1) Then
    '    Vmc = True
    '    If IsReport Then
    '      CurrentPlotSettings.FilterValueMin = Mid - CurrentPlotSettings.VmcAutoFilterRange
    '      CurrentPlotSettings.FilterValueMax = Mid + CurrentPlotSettings.VmcAutoFilterRange
    '    Else
    '      CurrentPlotSettings.FilterValueMin = Mid - AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilterRange
    '      CurrentPlotSettings.FilterValueMax = Mid + AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilterRange
    '    End If
    '  End If
    '  If Vmc Then
    '    x5 = TrovaCanaleDaChannelId(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway))
    '    CurrentPlotSettings.ShowDownwindVmg = True
    '    CurrentPlotSettings.ShowUpwindVmg = True
    '    CurrentPlotSettings.ShowNotVmg = True
    '    CurrentPlotSettings.ShowPort = True
    '    CurrentPlotSettings.ShowStbd = True
    '    CurrentPlotSettings.ApplyFilter1 = True
    '    CurrentPlotSettings.ApplyFilterMin = True
    '    CurrentPlotSettings.ApplyFilterMax = True
    '  End If
    'End If

    Dim CheckVmc As Boolean = If(IsReport, CurrentPlotSettings.VmcAutoFilter, AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilter)
    Dim FilterRange As Integer = If(IsReport, CurrentPlotSettings.VmcAutoFilterRange, AppConfig.ActiveProfile.XYPlotSettings.VmcAutoFilterRange)

    If CheckVmc AndAlso x2 IsNot Nothing Then
      Dim Vmc As Boolean = False
      Dim Mid As Integer = -1
      Select Case x2.CanaleChiave
        Case clsChannels2020.eCanaliChiave.eVmc40 : Mid = 40
        Case clsChannels2020.eCanaliChiave.eVmc50 : Mid = 50
        Case clsChannels2020.eCanaliChiave.eVmc60 : Mid = 60
        Case clsChannels2020.eCanaliChiave.eVmc70 : Mid = 70
        Case clsChannels2020.eCanaliChiave.eVmc80 : Mid = 80
        Case clsChannels2020.eCanaliChiave.eVmc90 : Mid = 90
        Case clsChannels2020.eCanaliChiave.eVmc100 : Mid = 100
        Case clsChannels2020.eCanaliChiave.eVmc110 : Mid = 110
        Case clsChannels2020.eCanaliChiave.eVmc120 : Mid = 120
        Case clsChannels2020.eCanaliChiave.eVmc130 : Mid = 130
        Case clsChannels2020.eCanaliChiave.eVmc140 : Mid = 140
        Case clsChannels2020.eCanaliChiave.eVmc150 : Mid = 150
      End Select
      If Mid > -1 Then
        Vmc = True
        CurrentPlotSettings.FilterValueMin = Mid - FilterRange
        CurrentPlotSettings.FilterValueMax = Mid + FilterRange
      End If
      If Vmc Then
        x5 = TrovaCanaleDaChannelId(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway))
        If x5 Is Nothing OrElse x5.HasValues = False Then
          x5 = TrovaCanaleDaChannelId(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertial))
        End If
        If x5 Is Nothing OrElse x5.HasValues = False Then
          x5 = TrovaCanaleDaChannelId(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA))
        End If
        CurrentPlotSettings.ShowDownwindVmg = True
        CurrentPlotSettings.ShowUpwindVmg = True
        CurrentPlotSettings.ShowNotVmg = True
        CurrentPlotSettings.ShowPort = True
        CurrentPlotSettings.ShowStbd = True
        CurrentPlotSettings.ApplyFilter1 = True
        CurrentPlotSettings.ApplyFilterMin = True
        CurrentPlotSettings.ApplyFilterMax = True
      End If
    End If

    CurrentPlotSettings.XAxisChannel = x1
    CurrentPlotSettings.YAxisChannel = x2
    CurrentPlotSettings.ColorChannel = x3
    CurrentPlotSettings.SailingStateChannel = x4
    CurrentPlotSettings.FilterChannel = x5
    CurrentPlotSettings.Filter2Channel = x6
    CurrentPlotSettings.ZAxisChannel = x7
    Select Case CurrentPlotSettings.SorgenteDati
      'Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered
      '  Lista = Nothing
      '  TR = DataPlotSync.VisibleRange
      'Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods, clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered, clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
      '  Lista = PeriodsManager.CollectionStraightLine
      '  TR = DataPlotSync.VisibleRange
      Case clsXYPlotSettings.eDataSource.eSelectedManoeuversEntryToExit
        Lista = PeriodsManager.CollectionPavarot
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eSelectedManoeuversVisibleRange
        CurrentPlotSettings.ManAndAccVisibleRange = True
        Lista = PeriodsManager.CollectionPavarot
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLinesVmg
        Lista = PeriodsManager.CollectionStraightLineVmgSelected
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eAllStraightLinesVmg, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected, clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter
        Lista = PeriodsManager.CollectionStraightLineVmg
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eAllStraightLines, clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected
        Lista = PeriodsManager.CollectionStraightLine
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLines, clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter
        Lista = PeriodsManager.CollectionStraightLineSelected
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLinesReaching, clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
        Lista = PeriodsManager.CollectionStraightLineReachingSelected
        TR = Nothing
      Case clsXYPlotSettings.eDataSource.eAllStraightLinesReaching, clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected
        Lista = PeriodsManager.CollectionStraightLineReaching
        TR = Nothing
      Case Else 'visiblerange
        Lista = PeriodsManager.CollectionStraightLine 'seleziona tutti i periodi caricati, ovvero anche quelli non inclusi nel timerange
        TR = DataPlotSync.VisibleRange
    End Select
    Select Case CurrentPlotSettings.SorgenteDati
      Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered
        DrawTimeRangeChart(Nothing, False)
        'DrawChartTimeRange(DataPlotSync.VisibleRange)
      Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
        DrawTimeRangeChart(Colors.Gray, True)
        'DrawChartTimeRange(DataPlotSync.VisibleRange, Colors.Gray)
      Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
        DrawTimeRangeChart(Colors.Gray, False)
        'DrawChartTimeRange(DataPlotSync.VisibleRange, Colors.Gray, True)
      Case clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected
        CurrentPlotSettings.ApplyFilterSailingState = False
        DrawPeriodsChart(Colors.Gray, False)
      Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
        CurrentPlotSettings.ApplyFilterSailingState = False
        DrawPeriodsChart(Colors.Gray, True)
      Case Else
        CurrentPlotSettings.ApplyFilterSailingState = False
        DrawPeriodsChart(Nothing, False)
    End Select
    If Not CurrentPlotSettings.YAxisChannel Is Nothing Then
      HighlightSpecialLines(CurrentPlotSettings.YAxisChannel.CanaleChiave)
    End If
    SelectedPeriods.Clear()
  End Sub

  Private Function TrovaId(Periodo As clsPeriod2021, Iniziale As Boolean) As Integer
    Return DataProvider2020.TrovaIndice(If(Iniziale, Periodo.TR.Start, Periodo.TR.Finish))
  End Function

  Private Function IsStbd(Periodo As clsPeriod2021) As Boolean
    Return Periodo.IsStbd
  End Function

  Private Function IsUp(Periodo As clsPeriod2021) As Boolean
    Return Math.Abs(Periodo.AvgTwa) < 90
  End Function

  Private Function ColoreDaTipo(Periodo As clsPeriod2021) As Color
    Select Case CurrentPlotSettings.GroupingType
      Case clsXYPlotSettings.eGroupingType.eTackOnly
        Return If(IsStbd(Periodo), System.Windows.Media.Colors.Green, System.Windows.Media.Colors.Red)
      Case clsXYPlotSettings.eGroupingType.eUpDnOnly
        Return If(IsUp(Periodo), System.Windows.Media.Colors.Blue, System.Windows.Media.Colors.Orange)
      Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
        Dim Colore = Periodo.Colore
        If IsStbd(Periodo) Then
          Colore = If(IsUp(Periodo), System.Windows.Media.Colors.DarkGreen, System.Windows.Media.Color.FromArgb(255, 0, 255, 0))
        Else
          Colore = If(IsUp(Periodo), System.Windows.Media.Colors.DarkRed, System.Windows.Media.Colors.Red)
        End If
        Return Colore
      'Case clsXYPlotSettings.eGroupingType.ePerformance
      '  Return ColoreDaPerformance(Periodo)
      Case clsXYPlotSettings.eGroupingType.eAllTogether
        Return System.Windows.Media.Colors.DarkGreen
      Case Else
        Return Periodo.Colore
    End Select
  End Function

  Private Function WindwardChannelForEntry(NomeCanale As String) As Boolean
    Dim Lista As String() = {"Runner", "V1", "D1", "TravRam"}
    For Each Str As String In Lista
      If NomeCanale.IndexOf(Str) > -1 Then
        Return True
      End If
    Next
    Return False
    'Return Lista.Where(Function(x) x.IndexOf(NomeCanale) > -1).Count > 0
  End Function

  Private Sub Valori(NomeCanale As String, ChannelId As String, ByRef CanaleWhenOnStbd As clsChannel2020, ByRef CanaleWhenOnPort As clsChannel2020, ByRef Label As String)

    'vengon intercettati i canali stbd port in modo da avere una plottatat non legata alle mura assolute ma a quelle entry ed exit
    ' La convenzione è Stbd Active side, Port lazy side
    ' nel caso di manovre è riferito all'entry
    Dim strDer As String = If(Label.EndsWith("Der "), " Der", "")
    Dim CanaleOriginale = DataProvider2020.CanaleDbl(ChannelId)
    CanaleWhenOnStbd = DataProvider2020.CanaleDbl(ChannelId)
    CanaleWhenOnPort = DataProvider2020.CanaleDbl(ChannelId)

    Select Case CanaleOriginale.DataType
      Case clsChannel2020.eDataType.eBoolean, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eTimeOnly
        Exit Sub
      Case Else
        If CanaleOriginale.LongName.Contains("Stbd") Then
          ' richiesti valori dell active side, normalmente sottovento eccetto volanti, sartie e carrello randa
          Dim Omologo As clsChannel2020 = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.LongName = NomeCanale.Replace("Stbd", "Port")).FirstOrDefault
          If Not Omologo Is Nothing Then
            If WindwardChannelForEntry(CanaleOriginale.LongName) Then
              ' active side sopravento
              If CurrentPlotSettings.WindwardLeewardFunction Then Label = NomeCanale.Replace("Stbd", "Wwd") & strDer
              CanaleWhenOnStbd = CanaleOriginale
              CanaleWhenOnPort = Omologo
            Else
              ' active side sottovento
              If CurrentPlotSettings.WindwardLeewardFunction Then Label = NomeCanale.Replace("Stbd", "Lwd") & strDer
              CanaleWhenOnStbd = Omologo
              CanaleWhenOnPort = CanaleOriginale
            End If
          End If
        ElseIf CanaleOriginale.LongName.Contains("Port") Then
          ' richiesti valori del lazy side
          Dim Omologo As clsChannel2020 = DataProvider2020.Channels.ListaCanali.Where(Function(x) x.LongName = NomeCanale.Replace("Port", "Stbd")).FirstOrDefault
          If Not Omologo Is Nothing Then
            If WindwardChannelForEntry(CanaleOriginale.LongName) Then
              ' lazy side sottovento
              If CurrentPlotSettings.WindwardLeewardFunction Then Label = NomeCanale.Replace("Port", "Lwd") & strDer
              CanaleWhenOnStbd = CanaleOriginale
              CanaleWhenOnPort = Omologo
            Else
              ' lazy side sopravento
              If CurrentPlotSettings.WindwardLeewardFunction Then Label = NomeCanale.Replace("Port", "Wwd") & strDer
              CanaleWhenOnStbd = Omologo
              CanaleWhenOnPort = CanaleOriginale
            End If
          End If
        Else
          ' non siamo nel caso in cui si debbano usare i valori wwdlwd
          Exit Sub
        End If
    End Select

  End Sub

  Private Function Valore(IsStbd As Boolean, Indice As Integer, chStbd As clsChannel2020, chPort As clsChannel2020) As Double
    If IsStbd Then
      Return ApplicaCorrezione(chStbd, Indice)
    End If
    Return ApplicaCorrezione(chPort, Indice)
  End Function

  Private Function ApplicaCorrezione(Canale As clsChannel2020, Indice As Integer) As Double
    Dim offset As Double = 0
    Dim coeff As Double = 1
    Select Case Canale.ChannelId
      'Case "ElevatorPort1_Strain"
      '  coeff = 1358 / 1220
      'Case "ElevatorStbd1_Strain"
      '  coeff = 4 ' 1358 / 1220
      Case Else
    End Select
    Try
      'Dim v = Canale.Valori(Indice)
      'If v = 0 AndAlso CurrentPlotSettings.YaxisZeroIsNanThen Then
      '    Return Double.NaN
      'Else
      Return (Canale.Valori(Indice) + offset) * coeff
      'End If
    Catch ex As Exception
      Return Double.NaN
    End Try
  End Function


  Private Enum eTipoFiltro
    eBetween
    eMin
    eMax
    eNone
  End Enum

  Private Sub FiltroAndatura()
    SottoTitolo = ""
    If CurrentPlotSettings.ShowUpwindVmg And CurrentPlotSettings.ShowDownwindVmg And CurrentPlotSettings.ShowNotVmg Then
      If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
        SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, "Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, "Port", "")
      End If
    ElseIf CurrentPlotSettings.ShowUpwindVmg And CurrentPlotSettings.ShowDownwindVmg Then
      SottoTitolo &= "Vmg Up/Dn"
      If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
        SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, " Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, " Port", "")
      End If
    ElseIf CurrentPlotSettings.ShowUpwindVmg Then
      SottoTitolo &= "Upwind"
      If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
        SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, " Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, " Port", "")
      End If
    ElseIf CurrentPlotSettings.ShowNotVmg Then
      SottoTitolo &= "Reaching Angles"
      If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
        SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, " Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, " Port", "")
      End If
    ElseIf CurrentPlotSettings.ShowDownwindVmg Then
      SottoTitolo &= "Downwind"
      If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
        SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, " Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, " Port", "")
      End If
      'Else
      '  If Not (CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd) Then
      '    SottoTitolo &= IIf(CurrentPlotSettings.ShowStbd, "Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, "Port", "")
      '  End If
    End If
    'SottoTitolo = IIf(ShowUpwind And ShowDownwind And CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd, "", IIf(ShowUpwind, "Up", "") & "" & IIf(ShowDownwind, "Dn", "") & "" & IIf(CurrentPlotSettings.ShowStbd, "Stbd", "") & "" & IIf(CurrentPlotSettings.ShowPort, "Port", ""))
  End Sub

  Private Sub FiltroSailingState()
    If CurrentPlotSettings.ApplyFilterSailingState Then
      'SottoTitolo &= " SailingState: " & SailingState.ToString.TrimStart("e")
      SottoTitolo &= " " & CurrentPlotSettings.SailingState.ToString.TrimStart("e")
    End If
  End Sub


  Private Function TipoFiltro() As eTipoFiltro
    Select Case CurrentPlotSettings.SorgenteDati
      Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange
        Return eTipoFiltro.eNone
      Case Else
    End Select

    If Not CurrentPlotSettings.ApplyFilter1 Then Return eTipoFiltro.eNone
    If Not CurrentPlotSettings.FilterChannel Is Nothing Then
      SottoTitolo &= " Filtered by:" & IIf(CurrentPlotSettings.ApplyFilterAbsVal, " AbsVal of", "") & " '" & CurrentPlotSettings.FilterChannel.ShortName & "' "
      If CurrentPlotSettings.ApplyFilterMin And CurrentPlotSettings.ApplyFilterMax Then
        SottoTitolo &= "between " & CurrentPlotSettings.FilterValueMin.ToString("F" & CurrentPlotSettings.FilterChannel.Decimals.ToString) & " and " & _CurrentPlotSettings.FilterValueMax.ToString("F" & CurrentPlotSettings.FilterChannel.Decimals.ToString) & ""
        Return eTipoFiltro.eBetween
      ElseIf CurrentPlotSettings.ApplyFilterMin Then
        SottoTitolo &= "min " & CurrentPlotSettings.FilterValueMin.ToString("F" & CurrentPlotSettings.FilterChannel.Decimals.ToString)
        Return eTipoFiltro.eMin
      ElseIf CurrentPlotSettings.ApplyFilterMax Then
        SottoTitolo &= "max " & CurrentPlotSettings.FilterValueMax.ToString("F" & CurrentPlotSettings.FilterChannel.Decimals.ToString)
        Return eTipoFiltro.eMax
      Else
        Return eTipoFiltro.eNone
      End If
    End If
    Return eTipoFiltro.eNone
  End Function

  Private Function TipoFiltro2() As eTipoFiltro
    'If Not CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered Then Return eTipoFiltro.eNone
    If Not CurrentPlotSettings.ApplyFilter2 Then Return eTipoFiltro.eNone
    If CurrentPlotSettings.Filter2Channel Is Nothing Then
      Return eTipoFiltro.eNone
    Else
      SottoTitolo &= " and:" & IIf(CurrentPlotSettings.ApplyFilter2AbsVal, " AbsVal of", "") & " '" & CurrentPlotSettings.Filter2Channel.ShortName & "' "
      If CurrentPlotSettings.ApplyFilter2Min And CurrentPlotSettings.ApplyFilter2Max Then
        SottoTitolo &= "between " & CurrentPlotSettings.Filter2ValueMin.ToString("F" & CurrentPlotSettings.Filter2Channel.Decimals.ToString) & " and " & CurrentPlotSettings.Filter2ValueMax.ToString("F" & CurrentPlotSettings.Filter2Channel.Decimals.ToString) & ""
        Return eTipoFiltro.eBetween
      ElseIf CurrentPlotSettings.ApplyFilter2Min Then
        SottoTitolo &= "min " & CurrentPlotSettings.Filter2ValueMin.ToString("F" & CurrentPlotSettings.Filter2Channel.Decimals.ToString)
        Return eTipoFiltro.eMin
      ElseIf CurrentPlotSettings.ApplyFilter2Max Then
        SottoTitolo &= "max " & CurrentPlotSettings.Filter2ValueMax.ToString("F" & CurrentPlotSettings.Filter2Channel.Decimals.ToString)
        Return eTipoFiltro.eMax
      Else
        Return eTipoFiltro.eNone
      End If
    End If
    Return eTipoFiltro.eNone
  End Function

  Private Function DatoValido(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
    Try
      If Double.IsNaN(Valore) Then
        Return False
      Else
        If AbsVal Then Valore = System.Math.Abs(Valore)
        Select Case TipoFiltro
          Case eTipoFiltro.eBetween
            Return Valore >= CurrentPlotSettings.FilterValueMin And Valore <= CurrentPlotSettings.FilterValueMax
          Case eTipoFiltro.eMax
            Return Valore <= CurrentPlotSettings.FilterValueMax
          Case eTipoFiltro.eMin
            Return Valore >= CurrentPlotSettings.FilterValueMin
          Case eTipoFiltro.eNone
            Return True
        End Select
      End If
      Return True
    Catch ex As Exception
      MsgBox("DatoValido")
      Return True
    End Try
  End Function

  Private Function DatoValido2(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
    Try
      If Double.IsNaN(Valore) Then
        Return False
      Else
        If AbsVal Then Valore = System.Math.Abs(Valore)
        Select Case TipoFiltro
          Case eTipoFiltro.eBetween
            Return Valore >= CurrentPlotSettings.Filter2ValueMin And Valore <= CurrentPlotSettings.Filter2ValueMax
          Case eTipoFiltro.eMax
            Return Valore <= CurrentPlotSettings.Filter2ValueMax
          Case eTipoFiltro.eMin
            Return Valore >= CurrentPlotSettings.Filter2ValueMin
          Case eTipoFiltro.eNone
            Return True
        End Select
      End If
      Return True
    Catch ex As Exception
      MsgBox("DatoValido")
      Return True
    End Try
  End Function

  Private Function DatoValidoSailingState(Valore As Double) As Boolean
    Try
      If Double.IsNaN(Valore) Then
        Return False
      Else
        Return Valore = CurrentPlotSettings.SailingState
      End If
    Catch ex As Exception
      MsgBox("DatoValido")
      Return True
    End Try
  End Function

  Private Function TwaValido(ValoreTwa As Double) As Boolean
    Try
      Dim TwaLimits As Integer() = {0, 60, 120, 180}

      Dim AbsTwa As Double = System.Math.Abs(ValoreTwa)
      If Double.IsNaN(ValoreTwa) Then
        Return False
      Else
        If AbsTwa <= TwaLimits(0) AndAlso AbsTwa >= TwaLimits(3) Then
          'troppo stretti e troppo larghi
          Return False
        ElseIf AbsTwa > TwaLimits(0) AndAlso AbsTwa < TwaLimits(1) Then
          'UpVmg
          If CurrentPlotSettings.ShowUpwindVmg Then
            If ValoreTwa > 0 Then
              Return CurrentPlotSettings.ShowStbd
            Else
              Return CurrentPlotSettings.ShowPort
            End If
          Else
            Return False
          End If
        ElseIf AbsTwa > TwaLimits(2) AndAlso AbsTwa < TwaLimits(3) Then
          'DnVmg
          If CurrentPlotSettings.ShowDownwindVmg Then
            If ValoreTwa > 0 Then
              Return CurrentPlotSettings.ShowStbd
            Else
              Return CurrentPlotSettings.ShowPort
            End If
          Else
            Return False
          End If
        Else
          'Not Vmg
          If CurrentPlotSettings.ShowNotVmg Then
            If ValoreTwa > 0 Then
              Return CurrentPlotSettings.ShowStbd
            Else
              Return CurrentPlotSettings.ShowPort
            End If
          Else
            Return False
          End If
        End If
      End If
    Catch ex As Exception
      MsgBox("twavalido")
      Stop
      Return True
    End Try
  End Function

  Private Function RigaValida(Indice As Integer, Filtro As eTipoFiltro, FilterChannel As clsChannel2020, Filtro2 As eTipoFiltro, Filte2Channel As clsChannel2020, CanaleTwa As clsChannel2020) As Boolean

    Dim v = CurrentPlotSettings.YAxisChannel.Valori(Indice)
    If v = 0 AndAlso CurrentPlotSettings.YaxisZeroIsNan Then
      Return False
    End If

    Dim Valido As Boolean = True
    If Not CanaleTwa Is Nothing Then
      Valido = TwaValido(CanaleTwa.Valori(Indice))
    End If
    If Valido Then
      If Not (CurrentPlotSettings.FilterChannel Is Nothing OrElse Filtro = eTipoFiltro.eNone) Then
        Valido = DatoValido(Filtro, CurrentPlotSettings.FilterChannel.Valori(Indice), CurrentPlotSettings.ApplyFilterAbsVal)
      End If
      If Valido AndAlso Not (CurrentPlotSettings.Filter2Channel Is Nothing OrElse Filtro2 = eTipoFiltro.eNone) Then
        Valido = DatoValido2(Filtro2, CurrentPlotSettings.Filter2Channel.Valori(Indice), CurrentPlotSettings.ApplyFilter2AbsVal)
      End If
    End If
    If Valido AndAlso CurrentPlotSettings.ApplyFilterSailingState Then
      Valido = DatoValidoSailingState(CurrentPlotSettings.SailingStateChannel.Valori(Indice))
    End If
    Return Valido
  End Function




  'Private Function RigaValida(IdIniziale As Integer, IdFinale As Integer, Periodo As clsPeriod2021, Filtro As eTipoFiltro, FilterChannel As clsChannel2020, Filtro2 As eTipoFiltro, Filte2Channel As clsChannel2020, CanaleTwa As clsChannel2020) As Boolean
  '    If IdIniziale = IdFinale Then Return False
  '    Dim Valido As Boolean = True
  '    Dim ValoreToBeChecked As Double = 0
  '    If Not CanaleTwa Is Nothing Then
  '        Select Case Periodo.PeriodType
  '            Case clsPeriod2021.ePeriodType.eTack
  '                ValoreToBeChecked = If(Periodo.IsStbd, 50, -50)
  '            Case clsPeriod2021.ePeriodType.eGybe
  '                ValoreToBeChecked = If(Periodo.IsStbd, 135, -135)
  '            Case Else
  '                ValoreToBeChecked = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori.Skip(IdIniziale).Take(IdFinale - IdIniziale).Where(Function(x) Not Double.IsNaN(x)).Average
  '        End Select
  '        Valido = TwaValido(ValoreToBeChecked)
  '        ' verifica se il filtro twa, se applicato, sia nel range
  '    End If
  '    If Valido Then
  '        If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '            ValoreToBeChecked = CurrentPlotSettings.FilterChannel.Valori.Skip(IdIniziale).Take(IdFinale - IdIniziale).Where(Function(x) Not Double.IsNaN(x)).Average
  '            Valido = DatoValido(Filtro, ValoreToBeChecked, CurrentPlotSettings.ApplyFilterAbsVal)
  '            ' verifica che il primo filtro, se applicato, sia valido
  '        End If
  '        If Valido AndAlso Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '            ValoreToBeChecked = CurrentPlotSettings.Filter2Channel.Valori.Skip(IdIniziale).Take(IdFinale - IdIniziale).Where(Function(x) Not Double.IsNaN(x)).Average
  '            Valido = DatoValido2(Filtro2, ValoreToBeChecked, CurrentPlotSettings.ApplyFilter2AbsVal)
  '            ' verifica che il secondo filtro, se applicato, sia valido
  '        End If
  '    End If
  '    Return Valido
  'End Function

  Private Sub VerificaTargetAndTrendLines(SeriesSource As ObservableCollection(Of IChartSeriesViewModel), DatiPerTL As List(Of clsTLdata))
    StampaTargetSeDisponibile(SeriesSource)
    StampaBenchmarksSeDisponibile(SeriesSource)
    Exit Sub
    'If Not CurrentPlotSettings.StampaTrendLines Then Exit Sub
    'TL.StampaTrendLineGruppo(SeriesSource, DatiPerTL, CurrentPlotSettings.GradoTrendLines)

  End Sub

  Private Sub VerificaTargetAndTrendLines(SeriesSource As ObservableCollection(Of IChartSeriesViewModel))
    StampaTargetSeDisponibile(SeriesSource)
    StampaBenchmarksSeDisponibile(SeriesSource)
    Exit Sub
    '' stampa trendline fatta da un altra parte

    'If Not CurrentPlotSettings.StampaTrendLines Then Exit Sub
    'Select Case CurrentPlotSettings.GroupingType
    '  Case clsXYPlotSettings.eGroupingType.eTackAndUpDown, clsXYPlotSettings.eGroupingType.eSinglePeriod, clsXYPlotSettings.eGroupingType.eColorBins, clsXYPlotSettings.eGroupingType.eDayBins, clsXYPlotSettings.eGroupingType.eYearBins, clsXYPlotSettings.eGroupingType.eKeyBins, clsXYPlotSettings.eGroupingType.eMonthBins, clsXYPlotSettings.eGroupingType.eValueBins
    '    TL.StampaTrendLines(CurrentPlotSettings.GradoTrendLines, SeriesSource)
    '  Case clsXYPlotSettings.eGroupingType.eTackOnly
    '    TL.StampaTrendLinesPortStbd(CurrentPlotSettings.GradoTrendLines, SeriesSource)
    '  Case clsXYPlotSettings.eGroupingType.eUpDnOnly
    '    TL.StampaTrendLinesUpDn(CurrentPlotSettings.GradoTrendLines, SeriesSource)
    '  Case Else
    '    TL.StampaTrendLineAllTogether(CurrentPlotSettings.GradoTrendLines, SeriesSource)
    'End Select

  End Sub

  Private Sub StampaTargetSeDisponibile(SeriesSource As ObservableCollection(Of IChartSeriesViewModel))
    If Not CurrentPlotSettings.ShowTargetIfAvailable Then Exit Sub
    If TgtManager Is Nothing Then Exit Sub
    If TgtManager.Tgt Is Nothing Then Exit Sub
    Dim NomePolare As String = ""
    Dim TgtInAscissa As Boolean
    If DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId) Is DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS) Then
      TgtInAscissa = False
      If CurrentPlotSettings.YAxisChannel.CanaleChiave.ToString.StartsWith("eVmc") Then
        TgtInAscissa = True
        NomePolare = CurrentPlotSettings.XAxisChannel.PolarHeader
        DisegnaPolareVmc("bs", CurrentPlotSettings.YAxisChannel.CanaleChiave)
        Exit Sub
      Else
        NomePolare = CurrentPlotSettings.YAxisChannel.PolarHeader
      End If
    ElseIf DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId) Is DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS) Then
      TgtInAscissa = True
      NomePolare = CurrentPlotSettings.XAxisChannel.PolarHeader
    Else
      Exit Sub
    End If
    DisegnaTarget(NomePolare, TgtInAscissa)


  End Sub

  Private Sub StampaBenchmarksSeDisponibile(SeriesSource As ObservableCollection(Of IChartSeriesViewModel))
    If Not CurrentPlotSettings.ShowBenchmarksIfAvailable Then Exit Sub
    DisegnaBenchmark(CurrentPlotSettings.ShowUpwindVmg, CurrentPlotSettings.ShowDownwindVmg, CurrentPlotSettings.XAxisChannel, CurrentPlotSettings.YAxisChannel)
  End Sub


  Public Sub HighlightSpecialLines(CanaleChiaveY As clsChannels2020.eCanaliChiave)
    Select Case CanaleChiaveY
      Case clsChannels2020.eCanaliChiave.eBSPp, clsChannels2020.eCanaliChiave.eBSTp, clsChannels2020.eCanaliChiave.eVMGp
        CurrentPlotSettings.HLine0Visibility = Visibility.Hidden
        CurrentPlotSettings.HLine1Visibility = Visibility.Hidden
        CurrentPlotSettings.HLine100Visibility = Visibility.Visible
      Case clsChannels2020.eCanaliChiave.eWindRatio
        CurrentPlotSettings.HLine0Visibility = Visibility.Visible
        CurrentPlotSettings.HLine1Visibility = Visibility.Visible
        CurrentPlotSettings.HLine100Visibility = Visibility.Hidden
      Case clsChannels2020.eCanaliChiave.eTWAd, clsChannels2020.eCanaliChiave.eTrimDelta, clsChannels2020.eCanaliChiave.eSowSogDelta, clsChannels2020.eCanaliChiave.eRudderDelta, clsChannels2020.eCanaliChiave.eLwyNorm, clsChannels2020.eCanaliChiave.eLWY, clsChannels2020.eCanaliChiave.eLeewayRecalc, clsChannels2020.eCanaliChiave.eHeelDelta
        CurrentPlotSettings.HLine0Visibility = Visibility.Visible
        CurrentPlotSettings.HLine1Visibility = Visibility.Visible
        CurrentPlotSettings.HLine100Visibility = Visibility.Hidden
      Case Else
        CurrentPlotSettings.HLine0Visibility = Visibility.Hidden
        CurrentPlotSettings.HLine1Visibility = Visibility.Hidden
        CurrentPlotSettings.HLine100Visibility = Visibility.Hidden
    End Select
  End Sub

  Public Sub DisegnaPolareVmc(PolarHeader As String, CanaleChiave As clsChannels2020.eCanaliChiave)
    Dim tgt As clsChannelTarget = TgtManager.Tgt.Polare(PolarHeader)
    If Not tgt Is Nothing Then
      Dim DataSeriesPolar As New XyDataSeries(Of Double, Double)
      Dim LineaPolar As New FastLineRenderableSeries
      LineaPolar.XAxisId = "DefaultAxisId"
      LineaPolar.YAxisId = "DefaultAxisId"
      LineaPolar.Stroke = Colors.DeepSkyBlue
      LineaPolar.StrokeThickness = 6
      'LineaTgtUp.StrokeDashArray = {3, 3}
      LineaPolar.Tag = "Polar"
      LineaPolar.IsVisible = True
      DataSeriesPolar.AcceptsUnsortedData = True
      DataSeriesPolar.SeriesName = LineaPolar.Tag

      Dim MaxX As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
      Dim MinX As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
      Dim twa As Double = 0
      Select Case CanaleChiave
        Case clsChannels2020.eCanaliChiave.eVmc40
          twa = 40
        Case clsChannels2020.eCanaliChiave.eVmc50
          twa = 50
        Case clsChannels2020.eCanaliChiave.eVmc60
          twa = 60
        Case clsChannels2020.eCanaliChiave.eVmc70
          twa = 70
        Case clsChannels2020.eCanaliChiave.eVmc80
          twa = 80
        Case clsChannels2020.eCanaliChiave.eVmc90
          twa = 90
        Case clsChannels2020.eCanaliChiave.eVmc100
          twa = 100
        Case clsChannels2020.eCanaliChiave.eVmc110
          twa = 110
        Case clsChannels2020.eCanaliChiave.eVmc120
          twa = 120
        Case clsChannels2020.eCanaliChiave.eVmc130
          twa = 130
        Case clsChannels2020.eCanaliChiave.eVmc140
          twa = 140
        Case clsChannels2020.eCanaliChiave.eVmc150
          twa = 150
      End Select


      For i As Integer = 1 To tgt.Rows.Count - 2
        Dim prevx As Double = tgt.Rows(i - 1).RowValue
        Dim nextx As Double = tgt.Rows(i + 1).RowValue
        If (nextx >= MinX) AndAlso (prevx <= MaxX) Then
          Dim X As Double = tgt.Rows(i).TargetValue(True)
          Dim Y As Double = tgt.PolarValue(tgt.Rows(i).RowValue, twa)
          If Not Double.IsNaN(Y) Then DataSeriesPolar.Append(tgt.Rows(i).RowValue, Y, New clsPuntoMetadata(False))
        End If
      Next
      LineaPolar.DataSeries = DataSeriesPolar
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesPolar, LineaPolar)
      If DataSeriesPolar.XValues.Count > 0 Then SeriesSource.Add(CSVMtgtup)

    End If
  End Sub

  Public Sub DisegnaTarget(PolarHeader As String, TgtInAscissa As Boolean)
    Dim tgt As clsChannelTarget = TgtManager.Tgt.Polare(PolarHeader)
    If Not tgt Is Nothing Then
      Dim DataSeriesTgtUp As New XyDataSeries(Of Double, Double)
      Dim LineaTgtUp As New FastLineRenderableSeries
      LineaTgtUp.XAxisId = "DefaultAxisId"
      LineaTgtUp.YAxisId = "DefaultAxisId"
      LineaTgtUp.Stroke = Colors.DeepSkyBlue
      LineaTgtUp.StrokeThickness = 6
      'LineaTgtUp.StrokeDashArray = {3, 3}
      LineaTgtUp.Tag = "TgtUp"
      LineaTgtUp.IsVisible = CurrentPlotSettings.ShowUpwindVmg
      DataSeriesTgtUp.AcceptsUnsortedData = True
      DataSeriesTgtUp.SeriesName = LineaTgtUp.Tag

      Dim DataSeriesTgtDn As New XyDataSeries(Of Double, Double)
      Dim LineaTgtDn As New FastLineRenderableSeries
      LineaTgtDn.XAxisId = "DefaultAxisId"
      LineaTgtDn.YAxisId = "DefaultAxisId"
      LineaTgtDn.Stroke = Colors.DarkBlue
      LineaTgtDn.StrokeThickness = 4
      'LineaTgtDn.StrokeDashArray = {2, 2}
      LineaTgtDn.Tag = "TgtDn"
      LineaTgtDn.IsVisible = CurrentPlotSettings.ShowDownwindVmg
      DataSeriesTgtDn.AcceptsUnsortedData = True
      DataSeriesTgtDn.SeriesName = LineaTgtDn.Tag

      Dim MaxX As Double = SeriesSource.Max(Function(x) x.DataSeries.XMax)
      Dim MinX As Double = SeriesSource.Min(Function(x) x.DataSeries.XMin)
      'Dim d = MaxX - MinX
      'MaxX += (d / 10)
      'MinX -= (d / 10)

      If (tgt.Rows(0).RowValue >= MinX) AndAlso (tgt.Rows(0).RowValue <= MaxX) Then
        Dim yUp As Double = tgt.Rows(0).TargetValue(True)
        Dim yDn As Double = tgt.Rows(0).TargetValue(False)
        If TgtInAscissa Then
          If Not Double.IsNaN(yUp) Then DataSeriesTgtUp.Append(yUp, tgt.Rows(0).RowValue, New clsPuntoMetadata(False))
          If Not Double.IsNaN(yDn) Then DataSeriesTgtDn.Append(yDn, tgt.Rows(0).RowValue, New clsPuntoMetadata(False))
        Else
          If Not Double.IsNaN(yUp) Then DataSeriesTgtUp.Append(tgt.Rows(0).RowValue, yUp, New clsPuntoMetadata(False))
          If Not Double.IsNaN(yDn) Then DataSeriesTgtDn.Append(tgt.Rows(0).RowValue, yDn, New clsPuntoMetadata(False))
        End If
      End If


      For i As Integer = 1 To tgt.Rows.Count - 2
        Dim prevx As Double = tgt.Rows(i - 1).RowValue
        Dim nextx As Double = tgt.Rows(i + 1).RowValue
        If (nextx >= MinX) AndAlso (prevx <= MaxX) Then
          Dim yUp As Double = tgt.Rows(i).TargetValue(True)
          Dim yDn As Double = tgt.Rows(i).TargetValue(False)
          If TgtInAscissa Then
            If Not Double.IsNaN(yUp) Then DataSeriesTgtUp.Append(yUp, tgt.Rows(i).RowValue, New clsPuntoMetadata(False))
            If Not Double.IsNaN(yDn) Then DataSeriesTgtDn.Append(yDn, tgt.Rows(i).RowValue, New clsPuntoMetadata(False))
          Else
            If Not Double.IsNaN(yUp) Then DataSeriesTgtUp.Append(tgt.Rows(i).RowValue, yUp, New clsPuntoMetadata(False))
            If Not Double.IsNaN(yDn) Then DataSeriesTgtDn.Append(tgt.Rows(i).RowValue, yDn, New clsPuntoMetadata(False))
          End If
        End If
      Next
      LineaTgtUp.DataSeries = DataSeriesTgtUp
      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTgtUp, LineaTgtUp)
      If CurrentPlotSettings.ShowUpwindVmg AndAlso DataSeriesTgtUp.XValues.Count > 0 Then SeriesSource.Add(CSVMtgtup)
      LineaTgtDn.DataSeries = DataSeriesTgtDn
      Dim CSVMtgtDn As New ChartSeriesViewModel(DataSeriesTgtDn, LineaTgtDn)
      If CurrentPlotSettings.ShowDownwindVmg AndAlso DataSeriesTgtDn.XValues.Count > 0 Then SeriesSource.Add(CSVMtgtDn)

    End If
  End Sub

  Public Sub DisegnaBenchmark(Upwind As Boolean, Downwind As Boolean, Xch As clsChannel2020, Ych As clsChannel2020)

    Dim StrArray(1) As Double
    StrArray(0) = 4
    StrArray(1) = 3

    'SeriesSource.Clear()
    Dim ListaBnch As New List(Of clsBenchmark)

    For Each b In BenchManager.ListaBenchmarks.Where(Function(x) x.Show).ToList
      Dim add As Boolean = (Xch.BenchmarkHeader = b.RefChannel OrElse Xch.BenchmarkHeader = b.ValChannel) AndAlso (Ych.BenchmarkHeader = b.RefChannel OrElse Ych.BenchmarkHeader = b.ValChannel)
      If add Then
        If b.Type = clsBenchmark.eBenchmarkType.eReaching Then
          add = True
        Else
          If Upwind And Downwind Then
            add = (b.Type = clsBenchmark.eBenchmarkType.eVmgUp OrElse b.Type = clsBenchmark.eBenchmarkType.eVmgDn)
          ElseIf Upwind Then
            add = b.Type = clsBenchmark.eBenchmarkType.eVmgUp
          ElseIf Downwind Then
            add = b.Type = clsBenchmark.eBenchmarkType.eVmgDn
          End If
        End If
        If add Then
          ListaBnch.Add(b)
        End If
      End If
    Next

    Dim idColore As Integer = 2
    For Each b In ListaBnch
      Dim DataSeriesLinea As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastLineRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Dim c As Color = ColoriDifferenziati(idColore)
      idColore += 1
      Linea.Stroke = c
      Linea.StrokeThickness = 3
      Linea.StrokeDashArray = StrArray
      DataSeriesLinea.SeriesName = b.Name
      DataSeriesLinea.AcceptsUnsortedData = True
      For Each punto In b.Values
        If Xch.BenchmarkHeader = b.RefChannel Then
          DataSeriesLinea.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
        Else
          DataSeriesLinea.Append(punto.Y, punto.X, New clsPuntoMetadata(False))
        End If
      Next
      Linea.DataSeries = DataSeriesLinea
      Dim CSVMlinea As New ChartSeriesViewModel(DataSeriesLinea, Linea)
      SeriesSource.Add(CSVMlinea)
    Next

  End Sub


  Public Function EsportaTrendLines(Msg As Boolean) As String
    If CurrentPlotSettings.YAxisChannel Is Nothing Then Return ""
    Dim Grado As Integer = 4
    Dim strTmp As String = TL.FormulaTrendLineFirstRow(Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(True, True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(True, False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(False, True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(False, False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.FormulaTrendLine(CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf & vbCrLf & vbCrLf

    strTmp &= TL.TabellaTrendLine(True, True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(True, False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(False, True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(False, False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(True, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(False, CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf
    strTmp &= TL.TabellaTrendLine(CurrentPlotSettings.XAxisChannel.ChannelId, CurrentPlotSettings.YAxisChannel.ChannelId, Grado) & vbCrLf & vbCrLf

    Clipboard.SetText(strTmp)
    If Msg Then MsgBox("Trend Lines Formulas and Tables Copied to Clipboard")
    Return strTmp

  End Function

  'Private Sub CompassCheck()
  '  Dim CX As clsChannel2020 = CurrentPlotSettings.XAxisChannel
  '  Dim CY As clsChannel2020 = CurrentPlotSettings.YAxisChannel

  'End Sub


  'Private Sub DrawChartTimeRange(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing)
  '  Dim dbg As Boolean = False
  '  If dbg Then MsgBox("1")
  '  If CurrentPlotSettings.XAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '  End If
  '  If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '  End If
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      'le chiavi sono associate ai soli periodi quindi questa funzione cerca i periodi nel time range ed plotta come se fosse una funzione periodi e non time range
  '      If NotSelectedColor = Nothing Then
  '        Lista = PeriodsManager.CollectionStraightLine
  '        Lista.Clear()
  '        For Each p In PeriodsManager.Periods.Lista
  '          If p.TR.IsOverlapped(TimeRange) Then
  '            p.IsChecked = True
  '            Lista.Add(p)
  '          Else
  '            p.IsChecked = False
  '          End If
  '        Next
  '        TR = Nothing
  '      End If


  '      DrawChartPeriods(NotSelectedColor)
  '      'DrawChartTimeRangeKeysColored(TimeRange)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelBins(TimeRange, NotSelectedColor)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      DrawChartTimeRange360ChecksChannelColoredByChannelValues(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelValues(TimeRange, NotSelectedColor)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      DrawChartTimeRangeChannelColoredByDay(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      DrawChartTimeRangeChannelColoredByMonth(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      DrawChartTimeRangeChannelColoredByYear(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown, clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      DrawChartTimeRangeTackUpDnColored(TimeRange, NotSelectedColor)
  '      Exit Sub
  '  End Select

  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = False

  '  Dim Colore = System.Windows.Media.Colors.DarkBlue
  '  Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)
  '  Dim ColoreNotSel = IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor)
  '  Dim ColorePuntoNotSel = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, ColoreNotSel.R, ColoreNotSel.G, ColoreNotSel.B)
  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Finish)

  '  If dbg Then MsgBox("2")
  '  Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmp As New XyScatterRenderableSeries
  '  LineaTmp.XAxisId = "DefaultAxisId"
  '  LineaTmp.YAxisId = "DefaultAxisId"
  '  LineaTmp.PointMarker = New EllipsePointMarker
  '  LineaTmp.PointMarker.Stroke = Colore
  '  LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmp.PointMarker.StrokeThickness = 0
  '  LineaTmp.PointMarker.Fill = ColorePunto
  '  DataSeriesTMP.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMP.AcceptsUnsortedData = True

  '  Dim DataSeriesTMPnotSel As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmpNotSel As New XyScatterRenderableSeries
  '  LineaTmpNotSel.XAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.YAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.PointMarker = New EllipsePointMarker
  '  LineaTmpNotSel.PointMarker.Stroke = ColoreNotSel
  '  LineaTmpNotSel.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmpNotSel.PointMarker.StrokeThickness = 0
  '  LineaTmpNotSel.PointMarker.Fill = ColorePuntoNotSel
  '  DataSeriesTMPnotSel.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMPnotSel.AcceptsUnsortedData = True

  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim XYC As New clsPrdXYZC(Colore)
  '  Dim XYCNotSel As New clsPrdXYZC(IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor))

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  If dbg Then MsgBox("3")
  '  For i As Integer = IdIniziale To Idfinale
  '    Try
  '      'If dbg Then MsgBox("a")

  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)

  '      If Valido Then
  '        If dbg Then MsgBox("b")
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          If dbg Then MsgBox("c")
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select

  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            XYC.Append(X, Y, Z)
  '            If dbg Then MsgBox("i")
  '            DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '          Else
  '            XYCNotSel.Append(X, Y, Z)
  '            If dbg Then MsgBox("ins")
  '            DataSeriesTMPnotSel.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '          If dbg Then MsgBox("l")
  '        End If
  '      End If
  '    Catch ex As Exception
  '      dbg = True
  '      MsgBox(i & vbCrLf & ex.Message & vbCrLf & vbCrLf & ex.StackTrace & vbCrLf & vbCrLf & ex.Source & vbCrLf & vbCrLf & ex.InnerException.Message)
  '      Exit Sub
  '    End Try

  '  Next
  '  If dbg Then MsgBox("4")

  '  ListaXYZC.Add(XYC)
  '  ListaXYZC.Add(XYCNotSel)

  '  LineaTmp.DataSeries = DataSeriesTMP
  '  Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
  '  SeriesSource.Add(CSVMtmp)
  '  LineaTmpNotSel.DataSeries = DataSeriesTMPnotSel
  '  Dim CSVMtmpNotSel As New ChartSeriesViewModel(DataSeriesTMPnotSel, LineaTmpNotSel)
  '  SeriesSource.Add(CSVMtmpNotSel)

  '  If CurrentPlotSettings.Export360check Then
  '    Dim d As New Dictionary(Of Integer, List(Of Double))
  '    For i As Integer = 0 To XYC.X.Count - 1
  '      Dim T = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(XYC.X(i), XYC.Y(i))
  '      Dim Ref As Integer = CInt(XYC.X(i) / 10) * 10
  '      If Not d.ContainsKey(Ref) Then
  '        d.Add(Ref, New List(Of Double))
  '      End If
  '      d(Ref).Add(T)
  '    Next
  '    Dim l As New Dictionary(Of Integer, Double)
  '    Dim s As String = CurrentPlotSettings.XAxisChannel.ActualLogHeader & vbTab & CurrentPlotSettings.YAxisChannel.ActualLogHeader & vbCrLf
  '    For Each item In d.OrderBy(Function(x) x.Key).ToList()
  '      l.Add(item.Key, item.Value.Average)
  '      s &= item.Key & vbTab & item.Value.Average & vbCrLf
  '    Next
  '    Clipboard.SetText(s)
  '  End If


  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If dbg Then MsgBox("5")

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  If dbg Then MsgBox("6")

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  If dbg Then MsgBox("7")

  'End Sub

  'Private Sub DrawChartTimeRange(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim dbg As Boolean = False
  '  If dbg Then MsgBox("1")
  '  If CurrentPlotSettings.XAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '  End If
  '  If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '  End If

  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      'le chiavi sono associate ai soli periodi quindi questa funzione cerca i periodi nel time range ed plotta come se fosse una funzione periodi e non time range
  '      If NotSelectedColor = Nothing Then
  '        Lista = PeriodsManager.CollectionStraightLine
  '        Lista.Clear()
  '        For Each p In PeriodsManager.Periods.Lista
  '          If p.TR.IsOverlapped(TimeRange) Then
  '            p.IsChecked = True
  '            Lista.Add(p)
  '          Else
  '            p.IsChecked = False
  '          End If
  '        Next
  '        TR = Nothing
  '      End If
  '      DrawPeriodsChart(NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelBins(TimeRange, NotSelectedColor, HighlightFilter)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      DrawChartTimeRange360ChecksChannelColoredByChannelValues(TimeRange, NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelValues(TimeRange, NotSelectedColor, HighlightFilter)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      DrawChartTimeRangeChannelColoredByDay(TimeRange, NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      DrawChartTimeRangeChannelColoredByMonth(TimeRange, NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      DrawChartTimeRangeChannelColoredByYear(TimeRange, NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown, clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      DrawChartTimeRangeTackUpDnColored(TimeRange, NotSelectedColor, HighlightFilter)
  '      Exit Sub
  '  End Select



  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = False

  '  Dim Colore = System.Windows.Media.Colors.DarkBlue
  '  Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)
  '  Dim ColoreNotSel = IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor)
  '  Dim ColorePuntoNotSel = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, ColoreNotSel.R, ColoreNotSel.G, ColoreNotSel.B)
  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Finish)

  '  If dbg Then MsgBox("2")
  '  Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmp As New XyScatterRenderableSeries
  '  LineaTmp.XAxisId = "DefaultAxisId"
  '  LineaTmp.YAxisId = "DefaultAxisId"
  '  LineaTmp.PointMarker = New EllipsePointMarker
  '  LineaTmp.PointMarker.Stroke = Colore
  '  LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmp.PointMarker.StrokeThickness = 0
  '  LineaTmp.PointMarker.Fill = ColorePunto
  '  DataSeriesTMP.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMP.AcceptsUnsortedData = True

  '  Dim DataSeriesTMPnotSel As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmpNotSel As New XyScatterRenderableSeries
  '  LineaTmpNotSel.XAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.YAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.PointMarker = New EllipsePointMarker
  '  LineaTmpNotSel.PointMarker.Stroke = ColoreNotSel
  '  LineaTmpNotSel.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmpNotSel.PointMarker.StrokeThickness = 0
  '  LineaTmpNotSel.PointMarker.Fill = ColorePuntoNotSel
  '  DataSeriesTMPnotSel.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMPnotSel.AcceptsUnsortedData = True

  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim XYC As New clsPrdXYZC(Colore)
  '  Dim XYCNotSel As New clsPrdXYZC(IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor))

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.LongName, CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.LongName, CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  If dbg Then MsgBox("3")
  '  For i As Integer = IdIniziale To Idfinale
  '    Try
  '      'If dbg Then MsgBox("a")

  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)

  '      If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '        If dbg Then MsgBox("b")
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          If dbg Then MsgBox("c")
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select

  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '          If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '            XYC.Append(X, Y, Z)
  '            If dbg Then MsgBox("i")
  '            DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '          Else
  '            XYCNotSel.Append(X, Y, Z)
  '            If dbg Then MsgBox("ins")
  '            DataSeriesTMPnotSel.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '          If dbg Then MsgBox("l")
  '        End If
  '      End If
  '    Catch ex As Exception
  '      dbg = True
  '      MsgBox(i & vbCrLf & ex.Message & vbCrLf & vbCrLf & ex.StackTrace & vbCrLf & vbCrLf & ex.Source & vbCrLf & vbCrLf & ex.InnerException.Message)
  '      Exit Sub
  '    End Try

  '  Next
  '  If dbg Then MsgBox("4")

  '  ListaXYZC.Add(XYC)
  '  ListaXYZC.Add(XYCNotSel)

  '  LineaTmp.DataSeries = DataSeriesTMP
  '  Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
  '  SeriesSource.Add(CSVMtmp)
  '  LineaTmpNotSel.DataSeries = DataSeriesTMPnotSel
  '  Dim CSVMtmpNotSel As New ChartSeriesViewModel(DataSeriesTMPnotSel, LineaTmpNotSel)
  '  SeriesSource.Add(CSVMtmpNotSel)

  '  If CurrentPlotSettings.Export360check Then
  '    Dim d As New Dictionary(Of Integer, List(Of Double))
  '    For i As Integer = 0 To XYC.X.Count - 1
  '      Dim T = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(XYC.X(i), XYC.Y(i))
  '      Dim Ref As Integer = CInt(XYC.X(i) / 10) * 10
  '      If Not d.ContainsKey(Ref) Then
  '        d.Add(Ref, New List(Of Double))
  '      End If
  '      d(Ref).Add(T)
  '    Next
  '    Dim l As New Dictionary(Of Integer, Double)
  '    Dim s As String = CurrentPlotSettings.XAxisChannel.ActualLogHeader & vbTab & CurrentPlotSettings.YAxisChannel.ActualLogHeader & vbCrLf
  '    For Each item In d.OrderBy(Function(x) x.Key).ToList()
  '      l.Add(item.Key, item.Value.Average)
  '      s &= item.Key & vbTab & item.Value.Average & vbCrLf
  '    Next
  '    Clipboard.SetText(s)
  '  End If


  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If dbg Then MsgBox("5")

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  If dbg Then MsgBox("6")

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  If dbg Then MsgBox("7")

  'End Sub

  Private Function Highlight(indice As Integer, NotSelectedColor As System.Windows.Media.Color, HighlightFilter As Boolean, FiltroValido As Boolean) As Boolean
    If HighlightFilter Then
      Return FiltroValido
    Else
      Return NotSelectedColor = Nothing OrElse IsInSelectedPeriod(indice)
    End If
  End Function


  'Private Sub DrawChartTimeRange(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing)
  '  Dim dbg As Boolean = False
  '  If dbg Then MsgBox("1")
  '  If CurrentPlotSettings.XAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '  End If
  '  If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '  End If
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      'le chiavi sono associate ai soli periodi quindi questa funzione cerca i periodi nel time range ed plotta come se fosse una funzione periodi e non time range
  '      If NotSelectedColor = Nothing Then
  '        Lista = PeriodsManager.CollectionStraightLine
  '        Lista.Clear()
  '        For Each p In PeriodsManager.Periods.Lista
  '          If p.TR.IsOverlapped(TimeRange) Then
  '            p.IsChecked = True
  '            Lista.Add(p)
  '          Else
  '            p.IsChecked = False
  '          End If
  '        Next
  '        TR = Nothing
  '      End If


  '      DrawChartPeriods(NotSelectedColor)
  '      'DrawChartTimeRangeKeysColored(TimeRange)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelBins(TimeRange, NotSelectedColor)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      DrawChartTimeRange360ChecksChannelColoredByChannelValues(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '        DrawChartTimeRangeChannelColoredByChannelValues(TimeRange, NotSelectedColor)
  '        Exit Sub
  '      End If
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      DrawChartTimeRangeChannelColoredByDay(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      DrawChartTimeRangeChannelColoredByMonth(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      DrawChartTimeRangeChannelColoredByYear(TimeRange, NotSelectedColor)
  '      Exit Sub
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown, clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      DrawChartTimeRangeTackUpDnColored(TimeRange, NotSelectedColor)
  '      Exit Sub
  '  End Select

  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = False

  '  Dim Colore = System.Windows.Media.Colors.DarkBlue
  '  Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)
  '  Dim ColoreNotSel = IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor)
  '  Dim ColorePuntoNotSel = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, ColoreNotSel.R, ColoreNotSel.G, ColoreNotSel.B)
  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndiceDaCanaleDT(TimeRange.Finish)

  '  If dbg Then MsgBox("2")
  '  Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmp As New XyScatterRenderableSeries
  '  LineaTmp.XAxisId = "DefaultAxisId"
  '  LineaTmp.YAxisId = "DefaultAxisId"
  '  LineaTmp.PointMarker = New EllipsePointMarker
  '  LineaTmp.PointMarker.Stroke = Colore
  '  LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmp.PointMarker.StrokeThickness = 0
  '  LineaTmp.PointMarker.Fill = ColorePunto
  '  DataSeriesTMP.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMP.AcceptsUnsortedData = True

  '  Dim DataSeriesTMPnotSel As New XyDataSeries(Of Double, Double)
  '  Dim LineaTmpNotSel As New XyScatterRenderableSeries
  '  LineaTmpNotSel.XAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.YAxisId = "DefaultAxisId"
  '  LineaTmpNotSel.PointMarker = New EllipsePointMarker
  '  LineaTmpNotSel.PointMarker.Stroke = ColoreNotSel
  '  LineaTmpNotSel.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  LineaTmpNotSel.PointMarker.StrokeThickness = 0
  '  LineaTmpNotSel.PointMarker.Fill = ColorePuntoNotSel
  '  DataSeriesTMPnotSel.SeriesName = TimeRange.StringaPeriodo
  '  DataSeriesTMPnotSel.AcceptsUnsortedData = True

  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim XYC As New clsPrdXYZC(Colore)
  '  Dim XYCNotSel As New clsPrdXYZC(IIf(NotSelectedColor = Nothing, Colors.Gray, NotSelectedColor))

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  If dbg Then MsgBox("3")
  '  For i As Integer = IdIniziale To Idfinale
  '    Try
  '      'If dbg Then MsgBox("a")

  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)

  '      If Valido Then
  '        If dbg Then MsgBox("b")
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          If dbg Then MsgBox("c")
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select

  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            XYC.Append(X, Y, Z)
  '            If dbg Then MsgBox("i")
  '            DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '          Else
  '            XYCNotSel.Append(X, Y, Z)
  '            If dbg Then MsgBox("ins")
  '            DataSeriesTMPnotSel.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '          If dbg Then MsgBox("l")
  '        End If
  '      End If
  '    Catch ex As Exception
  '      dbg = True
  '      MsgBox(i & vbCrLf & ex.Message & vbCrLf & vbCrLf & ex.StackTrace & vbCrLf & vbCrLf & ex.Source & vbCrLf & vbCrLf & ex.InnerException.Message)
  '      Exit Sub
  '    End Try

  '  Next
  '  If dbg Then MsgBox("4")

  '  ListaXYZC.Add(XYC)
  '  ListaXYZC.Add(XYCNotSel)

  '  LineaTmp.DataSeries = DataSeriesTMP
  '  Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
  '  SeriesSource.Add(CSVMtmp)
  '  LineaTmpNotSel.DataSeries = DataSeriesTMPnotSel
  '  Dim CSVMtmpNotSel As New ChartSeriesViewModel(DataSeriesTMPnotSel, LineaTmpNotSel)
  '  SeriesSource.Add(CSVMtmpNotSel)

  '  If CurrentPlotSettings.Export360check Then
  '    Dim d As New Dictionary(Of Integer, List(Of Double))
  '    For i As Integer = 0 To XYC.X.Count - 1
  '      Dim T = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(XYC.X(i), XYC.Y(i))
  '      Dim Ref As Integer = CInt(XYC.X(i) / 10) * 10
  '      If Not d.ContainsKey(Ref) Then
  '        d.Add(Ref, New List(Of Double))
  '      End If
  '      d(Ref).Add(T)
  '    Next
  '    Dim l As New Dictionary(Of Integer, Double)
  '    Dim s As String = CurrentPlotSettings.XAxisChannel.ActualLogHeader & vbTab & CurrentPlotSettings.YAxisChannel.ActualLogHeader & vbCrLf
  '    For Each item In d.OrderBy(Function(x) x.Key).ToList()
  '      l.Add(item.Key, item.Value.Average)
  '      s &= item.Key & vbTab & item.Value.Average & vbCrLf
  '    Next
  '    Clipboard.SetText(s)
  '  End If


  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If dbg Then MsgBox("5")

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  If dbg Then MsgBox("6")

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  If dbg Then MsgBox("7")

  'End Sub

  'Private Sub DrawChartTimeRangeChannelColoredByChannelBins(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  For i As Integer = 0 To CurrentPlotSettings.ColorChannelIntervals - 1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoreBeneMale(255, i / CurrentPlotSettings.ColorChannelIntervals)))
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)

  '  For i As Integer = IdIniziale To Idfinale
  '    Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '    If (Not Double.IsNaN(v)) Then
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'If Valido Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '      If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '        RigheValide.Add(i, TwaValido)
  '        If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '          ValoriCanaleColore.Add(v)
  '        End If
  '      End If
  '    End If
  '  Next
  '  Dim vMin As Double = 0
  '  Dim Intervallo As Double = 0
  '  If (ValoriCanaleColore.Count > 0) Then
  '    vMin = ValoriCanaleColore.Min
  '    Intervallo = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals
  '  End If
  '  Intervallo *= 1.001

  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel

  '  'For Each i As Integer In RigheValide

  '  'Next



  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys


  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If CurrentPlotSettings.WindwardLeewardFunction Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim Indice As Integer = 0
  '      Dim v = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) Then
  '        If Intervallo > 0 Then Indice = System.Math.Floor((v - vMin) / Intervallo)
  '      End If
  '      'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '      If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '        ListaXYZC(Indice).Append(X, Y, Z)
  '        BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '        TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        _SubSet.ListaMomenti.Add(Momento)
  '        _SubSet.ListaValori(0).Add(X)
  '        _SubSet.ListaValori(1).Add(Y)
  '        _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '      Else
  '        ListaXYZC.Last.Append(X, Y, Z)
  '        BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '        TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '      End If
  '    End If

  '    'End If

  '  Next


  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then

  '      Dim coeff As Double = i / BinsOfDataSeries.Count
  '      'Dim Colore = ColoreBeneMale(CurrentPlotSettings.DataPointOpacity, coeff)
  '      Dim Colore = ListaXYZC(i).Colore
  '      'If Not NotSelectedColor = Nothing Then

  '      'End If
  '      'Dim Colore = ColoriDifferenziati(i)
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" ' & Chiavi(i)
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next


  'End Sub

  'Private Sub DrawChartTimeRange360ChecksChannelColoredByChannelBins(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  For i As Integer = 0 To CurrentPlotSettings.ColorChannelIntervals - 1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoreBeneMale(255, i / CurrentPlotSettings.ColorChannelIntervals)))
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  For i As Integer = IdIniziale To Idfinale
  '    Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '    If (Not Double.IsNaN(v)) Then
  '      'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      If Valido Then
  '        ValoriCanaleColore.Add(v)
  '      End If
  '    End If
  '  Next
  '  Dim vMin As Double = 0
  '  Dim Intervallo As Double = 0
  '  If (ValoriCanaleColore.Count > 0) Then
  '    vMin = ValoriCanaleColore.Min
  '    Intervallo = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals
  '  End If
  '  Intervallo *= 1.001


  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    If Valido Then
  '      Dim Momento As DateTime = DataProvider2020.Momento(i)
  '      Dim X As Double
  '      Dim Y As Double
  '      Dim Z As Double = 0
  '      If False Then
  '        X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '        Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '      Else
  '        X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '        Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(i), CurrentPlotSettings.YAxisChannel.Valori(i))
  '        Y = delta
  '        'X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '        'Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '      End If
  '      If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '      If Momento.ToOADate > 0 Then
  '        Select Case CurrentPlotSettings.XAxisChannel.DataType
  '          Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '            X = System.Math.Abs(X)
  '          Case clsChannel2020.eDataType.e180
  '            If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '        End Select
  '        Select Case CurrentPlotSettings.YAxisChannel.DataType
  '          Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '            Y = System.Math.Abs(Y)
  '          Case clsChannel2020.eDataType.e180
  '            If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '        End Select
  '        If CurrentPlotSettings.XAxisDerivative Then
  '          Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '          X_ValoreRigaPrev = X
  '          If Double.IsNaN(Derivata) Then
  '            X = Double.NaN
  '          Else
  '            X_MediaMobile.AggiornaMedia(Derivata)
  '            X = X_MediaMobile.Valore
  '          End If
  '        End If
  '        If CurrentPlotSettings.YAxisDerivative Then
  '          Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '          Y_ValoreRigaPrev = Y
  '          If Double.IsNaN(Derivata) Then
  '            Y = Double.NaN
  '          Else
  '            Y_MediaMobile.AggiornaMedia(Derivata)
  '            Y = Y_MediaMobile.Valore
  '          End If
  '        End If
  '        MomentoPrev = Momento
  '        Dim Indice As Integer = 0
  '        Dim v = CurrentPlotSettings.ColorChannel.Valori(i)
  '        If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) Then
  '          'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '          If Intervallo > 0 Then Indice = System.Math.Floor((v - vMin) / Intervallo)
  '          If Indice > -1 Then
  '            If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '              _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '        End If
  '        'Dim Indice As Integer = 0
  '        'If Indice > -1 Then
  '        '  Dim v = CurrentPlotSettings.ColorChannel.Valori(i)
  '        '  If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) Then
  '        '    If Intervallo > 0 Then Indice = System.Math.Floor((v - vMin) / Intervallo)
  '        '  End If
  '        '  ListaXYZC(Indice).Append(X, Y, Z)
  '        '  BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '        '  TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        '  _SubSet.ListaMomenti.Add(Momento)
  '        '  _SubSet.ListaValori(0).Add(X)
  '        '  _SubSet.ListaValori(1).Add(Y)
  '        '  _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '        'End If
  '      End If

  '    End If

  '  Next


  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim coeff As Double = i / BinsOfDataSeries.Count
  '      'Dim Colore = ColoreBeneMale(CurrentPlotSettings.DataPointOpacity, coeff)
  '      Dim Colore = ListaXYZC(i).Colore
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))
  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next


  'End Sub

  'Private Sub DrawChartTimeRangeChannelColoredByChannelValues(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = True '  CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12

  '  Dim ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  'Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  ''Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '  '  If (Not Double.IsNaN(v)) Then
  '  '    'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '  '    'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    'If Valido Then
  '  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '      RigheValide.Add(i, TwaValido)

  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  'Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '    If (Not Double.IsNaN(v)) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '      If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '        RigheValide.Add(i, TwaValido)
  '        If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '          If Not ChannelValuesD.ContainsKey(v) Then
  '            ChannelValuesD.Add(v, 0)
  '          End If
  '          ChannelValuesD(v) += 1
  '          'ValidSamples += 1
  '        End If
  '      End If
  '    End If
  '  Next


  '  'If Not NotSelectedColor = Nothing Then
  '  '  ChannelValuesD.Add(Double.NaN, 0)
  '  'End If

  '  If ChannelValuesD.Count = 0 Then Exit Sub
  '  If ChannelValuesD.Count > CurrentPlotSettings.ColorChannelIntervals Then
  '    DrawChartTimeRangeChannelColoredByChannelBins(TimeRange, NotSelectedColor)
  '    Exit Sub
  '  End If

  '  Dim MinPerc As Integer = RigheValide.Count / 100 * 1
  '  Dim a = ChannelValuesD.Where(Function(x) x.Value > MinPerc).ToList
  '  Dim b = a.OrderBy(Function(x) x.Key).ToList
  '  Dim ChannelValues As List(Of Double) = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim idColore As Integer = 0
  '  For Each ChannelValue As Double In ChannelValues
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
  '    idColore += 1
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If


  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel



  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If CurrentPlotSettings.WindwardLeewardFunction Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim Indice As Integer = 0
  '      Dim v = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) Then
  '        'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '        Indice = ChannelValues.IndexOf(v)
  '        If Indice > -1 Then
  '          'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '          If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '            ListaXYZC(Indice).Append(X, Y, Z)
  '            BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '            _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '        Else
  '          ListaXYZC.Last.Append(X, Y, Z)
  '          BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        End If
  '      End If
  '    End If

  '    'End If

  '  Next


  '  'Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim coeff As Double = i / BinsOfDataSeries.Count
  '      'Dim Colore = ColoreBeneMale(CurrentPlotSettings.DataPointOpacity, coeff)
  '      Dim Colore = ListaXYZC(i).Colore
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        If CurrentPlotSettings.ColorChannel.CanaleChiave = clsChannels2020.eCanaliChiave.eSailSet Then
  '          BinsOfDataSeries(i).SeriesName = DataProvider2020.SailSet(CInt(ChannelValues(i))) '
  '        Else
  '          BinsOfDataSeries(i).SeriesName = ChannelValues(i).ToString() ' ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '        End If
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next


  'End Sub

  'Private Sub DrawChartTimeRange360ChecksChannelColoredByChannelValues(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = True '  CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12

  '  'Dim ChannelValuesT As New List(Of Double)
  '  Dim ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  'Dim RigheValide As New Dictionary(Of Integer, Boolean)

  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '  '  If (Not Double.IsNaN(v)) Then
  '  '    'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    'If Valido Then
  '  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '      RigheValide.Add(i, TwaValido)



  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  'Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '    If (Not Double.IsNaN(v)) Then
  '      'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'If Valido Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '      If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '        RigheValide.Add(i, TwaValido)
  '        If Not ChannelValuesD.ContainsKey(v) Then
  '          ChannelValuesD.Add(v, 0)
  '        End If
  '        ChannelValuesD(v) += 1
  '        'ValidSamples += 1
  '      End If
  '    End If
  '  Next

  '  If ChannelValuesD.Count = 0 Then Exit Sub
  '  If ChannelValuesD.Count > CurrentPlotSettings.ColorChannelIntervals Then
  '    DrawChartTimeRange360ChecksChannelColoredByChannelBins(TimeRange, NotSelectedColor)
  '    Exit Sub
  '  End If

  '  Dim MinPerc As Integer = RigheValide.Count / 100 * 1
  '  Dim a = ChannelValuesD.Where(Function(x) x.Value > MinPerc).ToList
  '  Dim b = a.OrderBy(Function(x) x.Key).ToList
  '  Dim ChannelValues As List(Of Double) = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim idColore As Integer = 0
  '  For Each ChannelValue As Double In ChannelValues
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
  '    idColore += 1
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If


  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel


  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If False Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(i), CurrentPlotSettings.YAxisChannel.Valori(i))
  '      Y = delta
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim Indice As Integer = 0
  '      Dim v = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) Then
  '        'v = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 10)
  '        Indice = ChannelValues.IndexOf(v)
  '        If Indice > -1 Then
  '          'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '          If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '            ListaXYZC(Indice).Append(X, Y, Z)
  '            BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '            _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          End If
  '        Else
  '          ListaXYZC.Last.Append(X, Y, Z)
  '          BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        End If
  '      End If
  '    End If

  '    'End If

  '  Next


  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim coeff As Double = i / BinsOfDataSeries.Count
  '      Dim Colore = ListaXYZC(i).Colore
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ChannelValues(i).ToString() ' ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 10
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next


  'End Sub

  'Private Sub DrawChartTimeRangeKeysColored(TimeRange As clsTimeRange)



  'SeriesSource.Clear()
  'FiltroAndatura()
  'FiltroSailingState()
  'Dim Filtro As eTipoFiltro = TipoFiltro()
  'Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  'SottoTitolo &= ", Colored by Key needs periods as source"


  'XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  'YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  'Titolo = TimeRange.StringaPeriodo(True)

  'Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  'Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  'Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  'Dim ListaXYZC As New List(Of clsPrdXYZC)
  'For i As Integer = 0 To CurrentPlotSettings.ColorChannelIntervals-1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(i)))
  'Next

  'Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  'CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  'Dim ValoriCanaleColore As New List(Of Double)
  'If Not CurrentPlotSettings.ColorChannel Is Nothing AndAlso CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  'End If

  'For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Valido = Valido AndAlso Not CurrentPlotSettings.ColorChannel Is Nothing
  '    If Valido Then
  '        ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '    End If
  'Next
  'Dim vMin As Double = ValoriCanaleColore.Where(Function(x) Not Double.IsInfinity(x)).Min
  'Dim vMax As Double = ValoriCanaleColore.Where(Function(x) Not Double.IsInfinity(x)).Max
  'Dim Intervallo As Double = System.Math.Abs(vMax - vMin) / CurrentPlotSettings.ColorChannelIntervals


  'Dim MomentoPrev As DateTime = TimeRange.Start

  'Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  'Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  'Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  'Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  ''Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  'Dim chXstbd As clsChannel2020
  'Dim chXport As clsChannel2020
  'Dim chYstbd As clsChannel2020
  'Dim chYport As clsChannel2020
  'Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  'Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  'If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  'End If
  'If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  'End If
  'If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  'End If
  'If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  'End If
  'If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  'End If
  'If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  'End If

  'Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '_SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  'Dim cZ = CurrentPlotSettings.ZAxisChannel
  'For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Valido = Valido AndAlso Not Double.IsInfinity(CurrentPlotSettings.ColorChannel.Valori(i))
  '    If Valido Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '            X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '            Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '            X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '            Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '            Select Case CurrentPlotSettings.XAxisChannel.DataType
  '                Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                    X = System.Math.Abs(X)
  '                Case clsChannel2020.eDataType.e180
  '                    If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            Select Case CurrentPlotSettings.YAxisChannel.DataType
  '                Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                    Y = System.Math.Abs(Y)
  '                Case clsChannel2020.eDataType.e180
  '                    If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            If CurrentPlotSettings.XAxisDerivative Then
  '                Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '                X_MediaMobile.AggiornaMedia(Derivata)
  '                X_ValoreRigaPrev = X
  '                X = X_MediaMobile.Valore
  '            End If
  '            If CurrentPlotSettings.YAxisDerivative Then
  '                Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '                Y_MediaMobile.AggiornaMedia(Derivata)
  '                Y_ValoreRigaPrev = Y
  '                Y = Y_MediaMobile.Valore
  '            End If
  '            MomentoPrev = Momento
  '            Dim Indice As Integer = 0
  '            If Intervallo > 0 Then Indice = System.Math.Round((CurrentPlotSettings.ColorChannel.Valori(i) - vMin) / Intervallo)
  '            ListaXYZC(Indice).Append(X, Y, Z)
  '            BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '            TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '            _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '        End If

  '    End If

  'Next


  'Dim Bins As Integer = 10
  'Dim Ordine = CurrentPlotSettings.GradoTrendLines

  'For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    Dim coeff As Double = i / BinsOfDataSeries.Count
  '    Dim Colore = ColoreBeneMale(CurrentPlotSettings.DataPointOpacity, coeff)
  '    'Dim Colore = ColoriDifferenziati(i)
  '    Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '    Dim LineaTmp As New XyScatterRenderableSeries
  '    LineaTmp.XAxisId = "DefaultAxisId"
  '    LineaTmp.YAxisId = "DefaultAxisId"
  '    LineaTmp.PointMarker = New EllipsePointMarker
  '    LineaTmp.PointMarker.Stroke = ColorePunto
  '    LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '    LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '    LineaTmp.PointMarker.StrokeThickness = 0
  '    LineaTmp.PointMarker.Fill = ColorePunto
  '    BinsOfDataSeries(i).SeriesName = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '    LineaTmp.DataSeries = BinsOfDataSeries(i)
  '    SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '    If CurrentPlotSettings.StampaTrendLines Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '            Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        If x.Count() > 4 AndAlso y.Count > 4 Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = 4
  '            TrendLine.StrokeDashArray = {3, 3}
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & ' Chiavi(i)
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '                Dim jj As Double = p.Coefficients(0)
  '                For g As Integer = 1 To Ordine
  '                    jj += p.Coefficients(g) * j ^ g
  '                Next
  '                DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '        End If
  '    End If

  'Next

  'VerificaTargetAndTrendLines(SeriesSource)


  'If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  'End If

  'PuntiXYZ.Clear()
  'For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '        PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  'Next

  'End Sub

  'Private Sub DrawChartTimeRangeChannelColoredByDay(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If
  '  Dim ListaGiorni As New List(Of String)


  '  'Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  ''Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '  '  If (Not Double.IsNaN(v)) Then
  '  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '      RigheValide.Add(i, TwaValido)

  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '      RigheValide.Add(i, TwaValido)
  '      If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '      End If
  '    End If
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    If Momento.ToOADate > 0 Then
  '      Dim ms As String = Momento.ToString("ddMMMyyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice = -1 Then
  '        ListaGiorni.Add(ms)
  '        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '        DataSeriesTMP.AcceptsUnsortedData = True
  '        BinsOfDataSeries.Add(DataSeriesTMP)
  '        ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(ListaGiorni.Count - 1)))
  '      End If

  '    End If
  '  Next

  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim vMin As Double = ValoriCanaleColore.Min
  '  Dim Intervallo As Double = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals
  '  Intervallo *= 1.001


  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If CurrentPlotSettings.WindwardLeewardFunction Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim ms As String = Momento.ToString("ddMMMyyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice > -1 Then
  '        'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '          ListaXYZC(Indice).Append(X, Y, Z)
  '          BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '          TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          _SubSet.ListaMomenti.Add(Momento)
  '          _SubSet.ListaValori(0).Add(X)
  '          _SubSet.ListaValori(1).Add(Y)
  '          _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '        Else
  '          ListaXYZC.Last.Append(X, Y, Z)
  '          BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        End If
  '      End If
  '    End If

  '    'End If

  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      'Dim Colore = ColoriDifferenziati(idCol)
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso i >= ListaGiorni.Count Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected" '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ListaGiorni(i) '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If

  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  'End Sub

  'Private Sub DrawChartTimeRangeChannelColoredByMonth(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by '" & CurrentPlotSettings.ColorChannel.ShortName & ""


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If
  '  Dim ListaGiorni As New List(Of String)


  '  'For i As Integer = IdIniziale To Idfinale
  '  '  'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  'If Valido Then
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '  If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '    If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '  '      ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '  '    End If
  '  '  End If
  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '      RigheValide.Add(i, TwaValido)
  '      If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '      End If
  '    End If
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    If Momento.ToOADate > 0 Then
  '      Dim ms As String = Momento.ToString("MMM yyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice = -1 Then
  '        ListaGiorni.Add(ms)
  '        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '        DataSeriesTMP.AcceptsUnsortedData = True
  '        BinsOfDataSeries.Add(DataSeriesTMP)
  '        ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(ListaGiorni.Count - 1)))
  '      End If

  '    End If
  '  Next

  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim vMin As Double = ValoriCanaleColore.Min
  '  Dim Intervallo As Double = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals
  '  Intervallo *= 1.001


  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If CurrentPlotSettings.WindwardLeewardFunction Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim ms As String = Momento.ToString("MMM yyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice > -1 Then
  '        'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '          ListaXYZC(Indice).Append(X, Y, Z)
  '          BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '          TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          _SubSet.ListaMomenti.Add(Momento)
  '          _SubSet.ListaValori(0).Add(X)
  '          _SubSet.ListaValori(1).Add(Y)
  '          _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '        Else
  '          ListaXYZC.Last.Append(X, Y, Z)
  '          BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        End If
  '      End If
  '    End If

  '    'End If

  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines


  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      'Dim Colore = ColoriDifferenziati(idCol)
  '      idCol += 1

  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso i >= ListaGiorni.Count Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected" '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ListaGiorni(i) '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  'End Sub

  'Private Sub DrawChartTimeRangeChannelColoredByYear(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  SottoTitolo &= ", colored by Year"


  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If
  '  Dim ListaGiorni As New List(Of String)
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  'If Valido Then
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '  If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '    If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '  '      ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '  '    End If
  '  '  End If
  '  Dim RigheValide As New Dictionary(Of Integer, Boolean)
  '  For i As Integer = IdIniziale To Idfinale
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '      RigheValide.Add(i, TwaValido)
  '      If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        ValoriCanaleColore.Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '      End If
  '    End If
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    If Momento.ToOADate > 0 Then
  '      Dim ms As String = Momento.ToString("yyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice = -1 Then
  '        ListaGiorni.Add(ms)
  '        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '        DataSeriesTMP.AcceptsUnsortedData = True
  '        BinsOfDataSeries.Add(DataSeriesTMP)
  '        ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(ListaGiorni.Count - 1)))
  '      End If

  '    End If
  '  Next

  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If
  '  If ValoriCanaleColore.Count = 0 Then Exit Sub
  '  Dim vMin As Double = ValoriCanaleColore.Min
  '  Dim Intervallo As Double = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals
  '  Intervallo *= 1.001



  '  Dim MomentoPrev As DateTime = TimeRange.Start

  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  'For i As Integer = IdIniziale To Idfinale
  '  '  Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '  If Valido Then
  '  For Each i As Integer In RigheValide.Keys
  '    Dim Momento As DateTime = DataProvider2020.Momento(i)
  '    Dim X As Double
  '    Dim Y As Double
  '    Dim Z As Double = 0
  '    If CurrentPlotSettings.WindwardLeewardFunction Then
  '      X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '      Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '    Else
  '      X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '      Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '    End If
  '    If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '    If Momento.ToOADate > 0 Then
  '      Select Case CurrentPlotSettings.XAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          X = System.Math.Abs(X)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      Select Case CurrentPlotSettings.YAxisChannel.DataType
  '        Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '          Y = System.Math.Abs(Y)
  '        Case clsChannel2020.eDataType.e180
  '          If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '      End Select
  '      If CurrentPlotSettings.XAxisDerivative Then
  '        Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        X_ValoreRigaPrev = X
  '        If Double.IsNaN(Derivata) Then
  '          X = Double.NaN
  '        Else
  '          X_MediaMobile.AggiornaMedia(Derivata)
  '          X = X_MediaMobile.Valore
  '        End If
  '      End If
  '      If CurrentPlotSettings.YAxisDerivative Then
  '        Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '        Y_ValoreRigaPrev = Y
  '        If Double.IsNaN(Derivata) Then
  '          Y = Double.NaN
  '        Else
  '          Y_MediaMobile.AggiornaMedia(Derivata)
  '          Y = Y_MediaMobile.Valore
  '        End If
  '      End If
  '      MomentoPrev = Momento
  '      Dim ms As String = DataProvider2020.Momento(i).ToString("yyyy")
  '      Dim Indice As Integer = ListaGiorni.IndexOf(ms)
  '      If Indice > -1 Then
  '        'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '        If Highlight(i, NotSelectedColor, HighlightFilter, RigheValide(i)) Then
  '          ListaXYZC(Indice).Append(X, Y, Z)
  '          BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '          TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '          _SubSet.ListaMomenti.Add(Momento)
  '          _SubSet.ListaValori(0).Add(X)
  '          _SubSet.ListaValori(1).Add(Y)
  '          _SubSet.ListaValori(2).Add(CurrentPlotSettings.ColorChannel.Valori(i))
  '        Else
  '          ListaXYZC.Last.Append(X, Y, Z)
  '          BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '        End If
  '      End If
  '    End If

  '    'End If

  '  Next

  '  Dim DatiPerTL As New List(Of clsTLdata)
  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      'Dim Colore = ColoriDifferenziati(i)
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)
  '      Dim ColoreTL = Color.FromArgb(255, Colore.R, Colore.G, Colore.B)


  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso i >= ListaGiorni.Count Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected" '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '        DatiPerTL.Add(New clsTLdata(BinsOfDataSeries(i).SeriesName, ColoreTL, BinsOfDataSeries(i)))
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ListaGiorni(i) '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '        DatiPerTL.Add(New clsTLdata(ListaGiorni(i), ColoreTL, BinsOfDataSeries(i)))
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 6 AndAlso y.Count > 6 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource, DatiPerTL)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  'End Sub

  Private Function IsInSelectedPeriod(Momento As DateTime) As Boolean
    'For Each prd In Lista()
    '  If prd.TR.InRange(Momento) AndAlso prd.IsChecked Then Return True
    'Next
    'Return False
    Return Not Lista.Where(Function(x) x.IsChecked AndAlso x.TR.InRange(Momento)) Is Nothing
  End Function

  Private Function IsInSelectedPeriod(IndiceRiga As Integer) As Boolean
    'For Each prd In Lista()
    '  If prd.TR.InRange(IndiceRiga) AndAlso prd.IsChecked Then Return True
    'Next
    'Return False
    Return Not Lista.Where(Function(x) x.IsChecked AndAlso x.TR.InRange(IndiceRiga)) Is Nothing
  End Function



  Private Enum eTackUpDn
    eUpPort = 0
    eUpStbd = 1
    eDnPort = 2
    eDnStbd = 3
  End Enum

  'Private Function strTipo() As String
  '  Dim enumType As Type = GetType(eTackUpDn)
  '  Dim names() As String = [Enum].GetNames(enumType)
  '  Return names(pTipo)
  'End Function

  'Private Sub DrawChartTimeRangeTackUpDnColored(TimeRange As clsTimeRange, Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SeriesSource.Clear()
  '  FiltroAndatura()
  '  FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  Titolo = TimeRange.StringaPeriodo(True)

  '  Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
  '  Dim Idfinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim enumType As Type = GetType(eTackUpDn)

  '  For i As Integer = 0 To [Enum].GetNames(enumType).Count - 1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(i)))
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If
  '  CurrentPlotSettings.LegendIsVisible = True

  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    CurrentPlotSettings.XAxisChannel = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '    If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '      Exit Sub
  '    End If
  '  End If
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    CurrentPlotSettings.YAxisChannel = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '    If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '      Exit Sub
  '    End If
  '  End If

  '  Dim MomentoPrev As DateTime = TimeRange.Start
  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If



  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, "GroupId"}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For i As Integer = IdIniziale To Idfinale
  '    'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    'If Valido Then
  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '      Dim Momento As DateTime = DataProvider2020.Momento(i)
  '      Dim X As Double
  '      Dim Y As Double
  '      Dim Z As Double = 0
  '      If CurrentPlotSettings.WindwardLeewardFunction Then
  '        X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '        Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '      Else
  '        X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '        Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '      End If
  '      If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '      If Momento.ToOADate > 0 Then
  '        Select Case CurrentPlotSettings.XAxisChannel.DataType
  '          Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '            X = System.Math.Abs(X)
  '          Case clsChannel2020.eDataType.e180
  '            If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '        End Select
  '        Select Case CurrentPlotSettings.YAxisChannel.DataType
  '          Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '            Y = System.Math.Abs(Y)
  '          Case clsChannel2020.eDataType.e180
  '            If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '        End Select
  '        If CurrentPlotSettings.XAxisDerivative Then
  '          Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '          X_ValoreRigaPrev = X
  '          If Double.IsNaN(Derivata) Then
  '            X = Double.NaN
  '          Else
  '            X_MediaMobile.AggiornaMedia(Derivata)
  '            X = X_MediaMobile.Valore
  '          End If
  '        End If
  '        If CurrentPlotSettings.YAxisDerivative Then
  '          Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '          Y_ValoreRigaPrev = Y
  '          If Double.IsNaN(Derivata) Then
  '            Y = Double.NaN
  '          Else
  '            Y_MediaMobile.AggiornaMedia(Derivata)
  '            Y = Y_MediaMobile.Valore
  '          End If
  '        End If
  '        MomentoPrev = Momento
  '        Dim Indice As Integer = IndiceTackUpDn(ChTwa.Valori(i))
  '        If Indice > -1 Then
  '          If Not Double.IsNaN(X) OrElse Not Double.IsNaN(Y) Then
  '            'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, i) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              TL.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              TLNotSel.AccodaCoppia(System.Math.Abs(ChTwa.Valori(i)) <= 90, ChTwa.Valori(i) >= 0, X, Y)
  '            End If
  '            _SubSet.ListaMomenti.Add(Momento)
  '            _SubSet.ListaValori(0).Add(X)
  '            _SubSet.ListaValori(1).Add(Y)
  '            _SubSet.ListaValori(2).Add(Indice)
  '          End If
  '        End If
  '      End If
  '    End If

  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim ColorePuntoNotSel = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, NotSelectedColor.R, NotSelectedColor.G, NotSelectedColor.B)

  '  Dim NrOftems = [Enum].GetNames(enumType).Count - 1
  '  Dim TotNrOfItems = NrOftems
  '  If Not NotSelectedColor = Nothing Then TotNrOfItems += 1
  '  For i As Integer = 0 To TotNrOfItems
  '    Dim LineaTmp As New XyScatterRenderableSeries
  '    LineaTmp.XAxisId = "DefaultAxisId"
  '    LineaTmp.YAxisId = "DefaultAxisId"
  '    LineaTmp.PointMarker = New EllipsePointMarker
  '    If Not NotSelectedColor = Nothing AndAlso i > NrOftems Then
  '      LineaTmp.PointMarker.Stroke = NotSelectedColor
  '      BinsOfDataSeries(i).SeriesName = "Not Selected"
  '    Else
  '      LineaTmp.PointMarker.Stroke = ColoreTackUpDn(i)
  '      BinsOfDataSeries(i).SeriesName = [Enum].GetNames(enumType)(i).ToString.TrimStart("e")
  '    End If
  '    LineaTmp.PointMarker.Fill = LineaTmp.PointMarker.Stroke
  '    LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '    LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '    LineaTmp.PointMarker.StrokeThickness = 0
  '    LineaTmp.DataSeries = BinsOfDataSeries(i)
  '    If BinsOfDataSeries(i).XValues.Count > 0 Then
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))
  '    End If

  '    If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '      Dim Coppie As New List(Of clsDoubleXY)
  '      For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '        Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '      Next
  '      Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '      Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '      Dim xx, yy As New List(Of Double)
  '      For ii As Integer = 0 To x.Count - 1
  '        If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '          xx.Add(x(ii))
  '          yy.Add(y(ii))
  '        End If
  '      Next
  '      x = xx.ToArray
  '      y = yy.ToArray
  '      If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '        If Not x.Min = x.Max Then
  '          Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '          Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '          Dim TrendLine As New FastLineRenderableSeries
  '          TrendLine.XAxisId = "DefaultAxisId"
  '          TrendLine.YAxisId = "DefaultAxisId"
  '          TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '          TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '          If i > NrOftems Then
  '            TrendLine.Stroke = NotSelectedColor
  '          Else
  '            TrendLine.Stroke = ColoreTackUpDn(i)
  '          End If
  '          TrendLine.Tag = "TL_" ' & Chiavi(i)
  '          TrendLine.IsVisible = True
  '          DataSeriesTmp.AcceptsUnsortedData = True
  '          For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '            Dim jj As Double = p.Coefficients(0)
  '            For g As Integer = 1 To Ordine
  '              jj += p.Coefficients(g) * j ^ g
  '            Next
  '            DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '          Next
  '          TrendLine.DataSeries = DataSeriesTmp
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '      End If
  '      If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '        Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '        Dim MainAverage As New XyScatterRenderableSeries
  '        MainAverage.XAxisId = "DefaultAxisId"
  '        MainAverage.YAxisId = "DefaultAxisId"
  '        MainAverage.PointMarker = New EllipsePointMarker
  '        MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '        MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '        MainAverage.PointMarker.StrokeThickness = 0
  '        If i > NrOftems Then
  '          MainAverage.Stroke = NotSelectedColor
  '        Else
  '          MainAverage.Stroke = ColoreTackUpDn(i)
  '        End If
  '        MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '        MainAverage.IsVisible = True
  '        DataSeriesMA.AcceptsUnsortedData = False
  '        DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '        MainAverage.DataSeries = DataSeriesMA
  '        Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '        SeriesSource.Add(CSVMtgtup)
  '      End If
  '      If CurrentPlotSettings.StampaTabellaDati Then
  '        Dim XYpdt As New XYplotDataTable
  '        XYplotDataTables.Counter += 1
  '        XYpdt.Order = XYplotDataTables.Counter
  '        XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '        XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '        XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '        XYpdt.XValues = x.ToList
  '        XYpdt.YValues = y.ToList
  '        XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '        XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '        XYplotDataTables.Tables.Add(XYpdt)
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next
  'End Sub

  Private Function ListaOrdinata(ListaRef As List(Of clsPeriod2021)) As List(Of clsPeriod2021)
    Select Case CurrentPlotSettings.GroupingType
      Case clsXYPlotSettings.eGroupingType.eTackOnly
        Return ListaRef.OrderBy(Function(x) x.IsStbd).ToList
        'Select Case CurrentPlotSettings.SorgenteDati
        '  Case clsXYPlotSettings.eDataSource.eSelectedManoeuversEntryToExit, clsXYPlotSettings.eDataSource.eSelectedManoeuversVisibleRange
        '    Return Lista.OrderBy(Function(x) x.IsStbd).ToList
        '  Case Else
        '    Return Lista.OrderBy(Function(x) x.IsStbd).ToList
        'End Select
      Case clsXYPlotSettings.eGroupingType.eUpDnOnly
        Select Case CurrentPlotSettings.SorgenteDati
          Case clsXYPlotSettings.eDataSource.eSelectedManoeuversEntryToExit, clsXYPlotSettings.eDataSource.eSelectedManoeuversVisibleRange
            Return ListaRef.OrderBy(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).ToList
          Case Else
            Return ListaRef.OrderBy(Function(x) Math.Abs(x.AvgTwa) < 90).ToList
        End Select
      Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
        Select Case CurrentPlotSettings.SorgenteDati
          Case clsXYPlotSettings.eDataSource.eSelectedManoeuversEntryToExit, clsXYPlotSettings.eDataSource.eSelectedManoeuversVisibleRange
            Return ListaRef.OrderBy(Function(x) x.IsStbd).OrderBy(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).ToList
          Case Else
            Return ListaRef.OrderBy(Function(x) x.IsStbd).OrderBy(Function(x) Math.Abs(x.AvgTwa) < 90).ToList
        End Select
      Case Else
        Return ListaRef.OrderBy(Function(x) x.TR.Start).ToList
    End Select

  End Function

  Private Function ValoreConSegno(valore As Double) As String
    If valore = 0 Then Return "0"
    Return If(valore > 0, "+", "") & valore.ToString("F0")
  End Function


  Private Function ListaPeriodiFiltratiPerTwa() As List(Of clsPeriod2021)
    Dim ListaFiltrata As New List(Of clsPeriod2021)
    'Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    For Each periodo In Lista.ToList
      periodo.RigheValide.Clear()
      If VerificaIfCheckedMatter(periodo) Then
        Dim Valido As Boolean = TwaValido(periodo.AvgTwa)
        If Valido Then
          ListaFiltrata.Add(periodo)
        End If
        'End If
      End If
    Next

    Return ListaFiltrata
  End Function

  Private Function ListaPeriodiFiltratiPerTipoStraightLine() As List(Of clsPeriod2021)
    Dim ListaFiltrata As New List(Of clsPeriod2021)
    For Each periodo In Lista.ToList
      periodo.RigheValide.Clear()
      If VerificaIfCheckedMatter(periodo) Then
        Dim Valido As Boolean = False
        If periodo.IsStbd Then
          Valido = CurrentPlotSettings.ShowStbd
        Else
          Valido = CurrentPlotSettings.ShowPort
        End If
        If periodo.IsDownwindVmgRange Then
          Valido = Valido AndAlso CurrentPlotSettings.ShowDownwindVmg
        ElseIf periodo.IsUpwindVmgRange Then
          Valido = Valido AndAlso CurrentPlotSettings.ShowUpwindVmg
        Else
          Valido = Valido AndAlso CurrentPlotSettings.ShowNotVmg
        End If
        'Select Case CurrentPlotSettings.SorgenteDati
        '  Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter
        '    Valido = True
        '  Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter
        '    Valido = periodo.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg
        '  Case clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
        '    Valido = periodo.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching
        'End Select
        If Valido Then
          ListaFiltrata.Add(periodo)
        End If
        'End If
      End If
    Next

    Return ListaFiltrata
  End Function

  'Private Sub DrawChartTimeRange(Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SottoTitolo = ""

  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '  End If
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '  End If
  '  Dim ListaFiltrata As List(Of clsPeriod2021) = Nothing
  '  If HighlightFilter Then
  '    ListaFiltrata = Lista.Where(Function(x) x.IsChecked).ToList()
  '  End If
  '  If CurrentPlotSettings.ColorChannel Is Nothing Then
  '    CurrentPlotSettings.ColorChannel = CurrentPlotSettings.YAxisChannel
  '  End If
  '  DrawTimeRangeChart(ListaFiltrata, NotSelectedColor, HighlightFilter)

  'End Sub

  'Private Sub DrawChartPeriods(Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  SottoTitolo = ""
  '  'XYplotDataTables.Tables.Clear()
  '  'FiltroAndatura()

  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
  '  If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
  '  End If
  '  If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
  '  End If
  '  Dim ListaFiltrata As List(Of clsPeriod2021) = ListaPeriodiFiltratiPerTwa()
  '  If HighlightFilter Then
  '    ListaFiltrata = ListaPeriodiFiltratiPerTipoStraightLine()
  '  End If
  '  If Not CurrentPlotSettings.ColorChannel Is Nothing Then
  '    'Select Case CurrentPlotSettings.GroupingType
  '    '  Case clsXYPlotSettings.eGroupingType.e360checks
  '    '    DrawChartPeriods360ChecksChannelColoredByChannelValues(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '    'Case clsXYPlotSettings.eGroupingType.eSinglePeriod
  '    '  Case Else
  '    '    DrawChartPeriodsChannelColoredBy(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    'End Select
  '    'Select Case CurrentPlotSettings.GroupingType
  '    '  Case clsXYPlotSettings.eGroupingType.e360checks
  '    '    DrawChartPeriods360ChecksChannelColoredByChannelValues(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eTackOnly
  '    '    DrawChartPeriodsChannelColoredBy(1, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '    '    DrawChartPeriodsChannelColoredBy(2, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '    '    DrawChartPeriodsChannelColoredBy(3, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eAllTogether
  '    '    DrawChartPeriodsChannelColoredBy(4, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eValueBins
  '    '    'DrawChartPeriodsChannelColoredByChannelValues(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    DrawChartPeriodsChannelColoredBy(0, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eColorBins
  '    '    DrawChartPeriodsChannelColoredBy(5, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    'DrawChartPeriodsChannelColoredByChannelBins(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eDayBins
  '    '    DrawChartPeriodsChannelColoredBy(7, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    'DrawChartPeriodsChannelColoredByDay(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eMonthBins
  '    '    DrawChartPeriodsChannelColoredBy(8, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    'DrawChartPeriodsChannelColoredByMonth(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eYearBins
  '    '    DrawChartPeriodsChannelColoredBy(9, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    'DrawChartPeriodsChannelColoredByYear(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case clsXYPlotSettings.eGroupingType.eKeyBins
  '    '    DrawChartPeriodsChannelColoredBy(6, ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    'DrawChartPeriodsChannelColoredByKey(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '    '    Exit Sub
  '    '  Case Else
  '    'End Select
  '  Else
  '    CurrentPlotSettings.ColorChannel = CurrentPlotSettings.YAxisChannel
  '  End If
  '  DrawPeriodsChart(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '  'Select Case CurrentPlotSettings.GroupingType
  '  '  Case clsXYPlotSettings.eGroupingType.e360checks
  '  '    DrawChartPeriods360ChecksChannelColoredByChannelValues(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '  '    Exit Sub
  '  '    'Case clsXYPlotSettings.eGroupingType.eSinglePeriod
  '  '  Case Else
  '  '    DrawChartPeriodsChannelColoredBy(ListaFiltrata, NotSelectedColor, HighlightFilter)
  '  '    Exit Sub
  '  'End Select

  '  'SeriesSource.Clear()


  '  'XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  'YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")
  '  'Dim ListaOrd As List(Of clsPeriod2021) = ListaOrdinata(ListaFiltrata)

  '  'CurrentPlotSettings.LegendIsVisible = ListaOrd.Where(Function(x) x.IsChecked).Count > 1 And ListaOrd.Where(Function(x) x.IsChecked).Count <= 12

  '  'Select Case CurrentPlotSettings.GroupingType
  '  '  Case clsXYPlotSettings.eGroupingType.eTackOnly
  '  '    Titolo &= " Grouped By Tack "
  '  '  Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '  '    Titolo &= " Grouped By Upwind Downwind "
  '  '  Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '  '    Titolo &= " Grouped By Tack and UpDn"
  '  '  Case clsXYPlotSettings.eGroupingType.eAllTogether
  '  '    Titolo &= " All Periods Together "
  '  'End Select
  '  'Dim Filtro As eTipoFiltro = TipoFiltro()
  '  'Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  'Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  'Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  'Dim chXstbd As clsChannel2020
  '  'Dim chXport As clsChannel2020
  '  'Dim chYstbd As clsChannel2020
  '  'Dim chYport As clsChannel2020
  '  'Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  'Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  'If chXstbd.Valori Is Nothing Then
  '  '  Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  'End If
  '  'If chXport.Valori Is Nothing Then
  '  '  Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  'End If
  '  'If chYstbd.Valori Is Nothing Then
  '  '  Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  'End If
  '  'If chYport.Valori Is Nothing Then
  '  '  Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  'End If
  '  'If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '  '  If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '  '  End If
  '  'End If
  '  'If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '  '  If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '  '  End If
  '  'End If

  '  'Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName}
  '  '_SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  'Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  'Dim GruppiPerTL As New Dictionary(Of Color, List(Of clsDoubleXY))

  '  'Dim NomeColore As New Dictionary(Of Color, String)

  '  'For Each periodo In ListaOrd
  '  '  Dim IdIniziale As Integer = TrovaId(periodo, True)
  '  '  Dim Idfinale As Integer = TrovaId(periodo, False)
  '  '  _ListaCount += 1
  '  '  _LastPeriod = periodo.TR.StringaPeriodo(True)
  '  '  Dim Colore = ColoreDaTipo(periodo)
  '  '  If Not periodo.IsChecked Then Colore = NotSelectedColor
  '  '  Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)
  '  '  Dim XYC As New clsPrdXYZC(Colore)

  '  '  Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '  '  Dim LineaTmp As New XyScatterRenderableSeries
  '  '  LineaTmp.XAxisId = "DefaultAxisId"
  '  '  LineaTmp.YAxisId = "DefaultAxisId"
  '  '  LineaTmp.PointMarker = New EllipsePointMarker
  '  '  LineaTmp.PointMarker.Stroke = ColorePunto
  '  '  LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '  '  LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '  '  LineaTmp.PointMarker.StrokeThickness = 0
  '  '  LineaTmp.PointMarker.Fill = ColorePunto
  '  '  'LineaTmp.Name = "sa" ' periodo.TR.StringaPeriodo
  '  '  DataSeriesTMP.SeriesName = periodo.DescrizioneBreve
  '  '  DataSeriesTMP.AcceptsUnsortedData = True
  '  '  Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

  '  '  Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '  '  Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '  '  Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '  '  Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)


  '  '  For i As Integer = IdIniziale To Idfinale
  '  '    'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '  '    'If Valido Then

  '  '    Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '    Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '    If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then

  '  '      Dim Momento As DateTime = DataProvider2020.Momento(i)
  '  '      Dim X As Double
  '  '      Dim Y As Double
  '  '      Dim Z As Double = 0
  '  '      If CurrentPlotSettings.WindwardLeewardFunction Then
  '  '        X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '  '        Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '  '      Else
  '  '        X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '  '        Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '  '      End If
  '  '      If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '  '      If Not Double.IsNaN(X) AndAlso Not Double.IsNaN(Y) Then
  '  '        If Momento.ToOADate > 0 Then
  '  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '  '              X = System.Math.Abs(X)
  '  '            Case clsChannel2020.eDataType.e180
  '  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '  '          End Select
  '  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '  '              Y = System.Math.Abs(Y)
  '  '            Case clsChannel2020.eDataType.e180
  '  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '  '          End Select
  '  '          If CurrentPlotSettings.XAxisDerivative Then
  '  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '  '            X_ValoreRigaPrev = X
  '  '            If Double.IsNaN(Derivata) Then
  '  '              X = Double.NaN
  '  '            Else
  '  '              X_MediaMobile.AggiornaMedia(Derivata)
  '  '              X = X_MediaMobile.Valore
  '  '            End If
  '  '          End If
  '  '          If CurrentPlotSettings.YAxisDerivative Then
  '  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '  '            Y_ValoreRigaPrev = Y
  '  '            If Double.IsNaN(Derivata) Then
  '  '              Y = Double.NaN
  '  '            Else
  '  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '  '              Y = Y_MediaMobile.Valore
  '  '            End If
  '  '          End If
  '  '          MomentoPrev = Momento
  '  '          XYC.Append(X, Y, Z)
  '  '          DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False, Momento))
  '  '          Dim Nome As String = ""
  '  '          If periodo.IsVmgRange Then
  '  '            Nome = TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '  '          End If
  '  '          _SubSet.ListaMomenti.Add(Momento)
  '  '          _SubSet.ListaValori(0).Add(X)
  '  '          _SubSet.ListaValori(1).Add(Y)

  '  '          If Not GruppiPerTL.ContainsKey(Colore) Then
  '  '            GruppiPerTL.Add(Colore, New List(Of clsDoubleXY))
  '  '            NomeColore.Add(Colore, Nome)
  '  '          End If
  '  '          GruppiPerTL(Colore).Add(New clsDoubleXY(X, Y))

  '  '        End If
  '  '      End If
  '  '    End If
  '  '  Next
  '  '  ListaXYZC.Add(XYC)
  '  '  LineaTmp.DataSeries = DataSeriesTMP
  '  '  Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
  '  '  SeriesSource.Add(CSVMtmp)
  '  'Next


  '  'Dim Ordine = CurrentPlotSettings.GradoTrendLines
  '  'If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '  '  For Each Gruppo In GruppiPerTL
  '  '    'Dim x As Double() = Gruppo.Value.OrderBy(Function(k) k.X).Select(Function(z) z.X).Where(Function(k) Not Double.IsNaN(k)).ToArray
  '  '    'Dim y As Double() = Gruppo.Value.OrderBy(Function(k) k.X).Select(Function(z) z.Y).Where(Function(k) Not Double.IsNaN(k)).ToArray
  '  '    Dim x As Double() = Gruppo.Value.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '  '    Dim y As Double() = Gruppo.Value.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '  '    Dim xx, yy As New List(Of Double)
  '  '    For ii As Integer = 0 To x.Count - 1
  '  '      If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '  '        xx.Add(x(ii))
  '  '        yy.Add(y(ii))
  '  '      End If
  '  '    Next
  '  '    x = xx.ToArray
  '  '    y = yy.ToArray
  '  '    If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '  '      If Not x.Min = x.Max Then
  '  '        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '  '        Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '  '        Dim TrendLine As New FastLineRenderableSeries
  '  '        TrendLine.XAxisId = "DefaultAxisId"
  '  '        TrendLine.YAxisId = "DefaultAxisId"
  '  '        TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '  '        TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '  '        TrendLine.Stroke = Gruppo.Key
  '  '        TrendLine.Tag = "TL_" ' & Chiavi(i)
  '  '        TrendLine.IsVisible = True
  '  '        DataSeriesTmp.AcceptsUnsortedData = True
  '  '        For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '  '          Dim jj As Double = p.Coefficients(0)
  '  '          For g As Integer = 1 To Ordine
  '  '            jj += p.Coefficients(g) * j ^ g
  '  '          Next
  '  '          DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '  '        Next
  '  '        TrendLine.DataSeries = DataSeriesTmp
  '  '        Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '  '        SeriesSource.Add(CSVMtgtup)
  '  '      End If
  '  '    End If
  '  '    If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '  '      Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '  '      Dim MainAverage As New XyScatterRenderableSeries
  '  '      MainAverage.XAxisId = "DefaultAxisId"
  '  '      MainAverage.YAxisId = "DefaultAxisId"
  '  '      MainAverage.PointMarker = New EllipsePointMarker
  '  '      MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '  '      MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '  '      MainAverage.PointMarker.StrokeThickness = 0
  '  '      MainAverage.Stroke = Gruppo.Key
  '  '      MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '  '      MainAverage.IsVisible = True
  '  '      DataSeriesMA.AcceptsUnsortedData = False
  '  '      DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '  '      MainAverage.DataSeries = DataSeriesMA
  '  '      Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '  '      SeriesSource.Add(CSVMtgtup)
  '  '    End If
  '  '    If CurrentPlotSettings.StampaTabellaDati Then
  '  '      Dim XYpdt As New XYplotDataTable
  '  '      XYplotDataTables.Counter += 1
  '  '      XYpdt.Order = XYplotDataTables.Counter
  '  '      XYpdt.Name = NomeColore(Gruppo.Key)
  '  '      XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '  '      XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '  '      XYpdt.XValues = x.ToList
  '  '      XYpdt.YValues = y.ToList
  '  '      XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '  '      XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '  '      XYplotDataTables.Tables.Add(XYpdt)
  '  '    End If
  '  '  Next
  '  'End If


  '  'VerificaTargetAndTrendLines(SeriesSource)


  '  'If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '  '  PlottaConsistency(ListaXYZC)
  '  'End If

  '  'PuntiXYZ.Clear()
  '  'For Each L In ListaXYZC
  '  '  For i As Integer = 0 To L.X.Count - 1
  '  '    PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '  '  Next
  '  'Next

  '  'Titolo = StringaTitolo(_ListaCount, _LastPeriod)

  'End Sub

  'Private Sub DrawChartPeriodsChannelColoredByChannelBins(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  'FiltroAndatura()
  '  'FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  For i As Integer = 0 To CurrentPlotSettings.ColorChannelIntervals - 1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(i)))
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

  '  'For Each periodo In PeriodiValidi
  '  '  periodo.RigheValide.Clear()
  '  '  Dim IdIniziale As Integer = TrovaId(periodo, True)
  '  '  Dim Idfinale As Integer = TrovaId(periodo, False)
  '  '  For i As Integer = IdIniziale To Idfinale
  '  '    'Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '  '    Dim v As Double = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 60)
  '  '    If Not Double.IsNaN(v) Then
  '  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '  '      Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '  '      If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '  '        periodo.RigheValide.Add(i, TwaValido)


  '  For Each periodo In PeriodiValidi
  '    If periodo.IsChecked Then
  '      periodo.RigheValide.Clear()
  '      Dim IdIniziale As Integer = TrovaId(periodo, True)
  '      Dim Idfinale As Integer = TrovaId(periodo, False)
  '      For i As Integer = IdIniziale To Idfinale
  '        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '        If Not Double.IsNaN(v) Then
  '          'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '          'If Valido Then
  '          Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '          Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '          If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '            periodo.RigheValide.Add(i, TwaValido)
  '            ValoriCanaleColore.Add(v)
  '          End If
  '        End If
  '      Next
  '    End If
  '  Next

  '  If ValoriCanaleColore.Count = 0 Then Exit Sub
  '  Dim vMin As Double = ValoriCanaleColore.Min
  '  Dim Intervallo As Double = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals

  '  If Intervallo = 0 Then
  '    Intervallo = 1
  '    'Exit Sub
  '  End If
  '  Intervallo *= 1.001

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)

  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then

  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      If HighlightFilter OrElse (Not HighlightFilter And Valido) Then

  '        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '        If Not Double.IsNaN(v) Then
  '          Dim Momento As DateTime = DataProvider2020.Momento(i)
  '          Dim X As Double
  '          Dim Y As Double
  '          Dim Z As Double = 0
  '          If CurrentPlotSettings.WindwardLeewardFunction Then
  '            X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '            Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '          Else
  '            X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '            Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '          End If
  '          If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '          If Momento.ToOADate > 0 Then
  '            Select Case CurrentPlotSettings.XAxisChannel.DataType
  '              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                X = System.Math.Abs(X)
  '              Case clsChannel2020.eDataType.e180
  '                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            Select Case CurrentPlotSettings.YAxisChannel.DataType
  '              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                Y = System.Math.Abs(Y)
  '              Case clsChannel2020.eDataType.e180
  '                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            If CurrentPlotSettings.XAxisDerivative Then
  '              Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '              X_ValoreRigaPrev = X
  '              If Double.IsNaN(Derivata) Then
  '                X = Double.NaN
  '              Else
  '                X_MediaMobile.AggiornaMedia(Derivata)
  '                X = X_MediaMobile.Valore
  '              End If
  '            End If
  '            If CurrentPlotSettings.YAxisDerivative Then
  '              Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '              Y_ValoreRigaPrev = Y
  '              If Double.IsNaN(Derivata) Then
  '                Y = Double.NaN
  '              Else
  '                Y_MediaMobile.AggiornaMedia(Derivata)
  '                Y = Y_MediaMobile.Valore
  '              End If
  '            End If
  '            MomentoPrev = Momento
  '            Dim Indice As Integer = System.Math.Floor((v - vMin) / Intervallo)
  '            If Indice > -1 Then
  '              'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '              If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '                ListaXYZC(Indice).Append(X, Y, Z)
  '                BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '                If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '                _SubSet.ListaMomenti.Add(Momento)
  '                _SubSet.ListaValori(0).Add(X)
  '                _SubSet.ListaValori(1).Add(Y)
  '              Else
  '                ListaXYZC.Last.Append(X, Y, Z)
  '                BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '                If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              End If
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore ' ColoriDifferenziati(idCol)
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eTackOnly
  '      Titolo &= " Colored By Tack "
  '    Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      Titolo &= " Colored By Upwind Downwind "
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '      Titolo &= " Colored By Tack and UpDn"
  '    Case clsXYPlotSettings.eGroupingType.eAllTogether
  '      Titolo &= " All Periods Together "
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      Titolo &= " Colored By Day "
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      Titolo &= " Colored By Month "
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      Titolo &= " Colored By Year "
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      Titolo &= " Colored By Keys "
  '  End Select

  'End Sub

  'Private Sub DrawChartPeriods360ChecksChannelColoredByChannelBins(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  'FiltroAndatura()
  '  'FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 15


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  For i As Integer = 0 To CurrentPlotSettings.ColorChannelIntervals - 1
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(i)))
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    For i As Integer = IdIniziale To Idfinale
  '      Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) Then
  '        Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '        If Valido Then
  '          ValoriCanaleColore.Add(v)
  '        End If
  '      End If
  '    Next
  '  Next
  '  If ValoriCanaleColore.Count = 0 Then Exit Sub
  '  Dim vMin As Double = ValoriCanaleColore.Min
  '  Dim Intervallo As Double = System.Math.Abs(ValoriCanaleColore.Max - vMin) / CurrentPlotSettings.ColorChannelIntervals

  '  If Intervallo = 0 Then
  '    Intervallo = 1
  '    'Exit Sub
  '  End If
  '  Intervallo *= 1.001

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      If Valido Then

  '        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '        If Not Double.IsNaN(v) Then
  '          Dim Momento As DateTime = DataProvider2020.Momento(i)
  '          Dim X As Double
  '          Dim Y As Double
  '          Dim Z As Double = 0
  '          If False Then
  '            X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '            Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '          Else
  '            X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '            Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(i), CurrentPlotSettings.YAxisChannel.Valori(i))
  '            Y = delta
  '            'X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '            'Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '          End If
  '          If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '          If Momento.ToOADate > 0 Then
  '            Select Case CurrentPlotSettings.XAxisChannel.DataType
  '              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                X = System.Math.Abs(X)
  '              Case clsChannel2020.eDataType.e180
  '                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            Select Case CurrentPlotSettings.YAxisChannel.DataType
  '              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '                Y = System.Math.Abs(Y)
  '              Case clsChannel2020.eDataType.e180
  '                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '            End Select
  '            If CurrentPlotSettings.XAxisDerivative Then
  '              Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '              X_ValoreRigaPrev = X
  '              If Double.IsNaN(Derivata) Then
  '                X = Double.NaN
  '              Else
  '                X_MediaMobile.AggiornaMedia(Derivata)
  '                X = X_MediaMobile.Valore
  '              End If
  '            End If
  '            If CurrentPlotSettings.YAxisDerivative Then
  '              Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '              Y_ValoreRigaPrev = Y
  '              If Double.IsNaN(Derivata) Then
  '                Y = Double.NaN
  '              Else
  '                Y_MediaMobile.AggiornaMedia(Derivata)
  '                Y = Y_MediaMobile.Valore
  '              End If
  '            End If
  '            MomentoPrev = Momento
  '            Dim Indice As Integer = System.Math.Floor((v - vMin) / Intervallo)
  '            If Indice > -1 Then
  '              If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '                ListaXYZC(Indice).Append(X, Y, Z)
  '                BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '                If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '                _SubSet.ListaMomenti.Add(Momento)
  '                _SubSet.ListaValori(0).Add(X)
  '                _SubSet.ListaValori(1).Add(Y)
  '              Else
  '                ListaXYZC.Last.Append(X, Y, Z)
  '                BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '                If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              End If
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      idCol += 1

  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eTackOnly
  '      Titolo &= " Colored By Tack "
  '    Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      Titolo &= " Colored By Upwind Downwind "
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '      Titolo &= " Colored By Tack and UpDn"
  '    Case clsXYPlotSettings.eGroupingType.eAllTogether
  '      Titolo &= " All Periods Together "
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      Titolo &= " Colored By Day "
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      Titolo &= " Colored By Month "
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      Titolo &= " Colored By Year "
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      Titolo &= " Colored By Keys "
  '  End Select

  'End Sub
  Private Function ValidRange(IdRiga As Integer) As Boolean
    Dim bTmp As Boolean = True
    Dim x As Double = CurrentPlotSettings.XAxisChannel.Valori(IdRiga)
    If Double.IsNaN(x) Then Return False
    If bTmp AndAlso CurrentPlotSettings.ApplyMinX Then
      bTmp = bTmp And x >= CurrentPlotSettings.MinX
    End If
    If bTmp AndAlso CurrentPlotSettings.ApplyMaxX Then
      bTmp = bTmp And x <= CurrentPlotSettings.MaxX
    End If

    Dim y As Double = CurrentPlotSettings.YAxisChannel.Valori(IdRiga)
    If Double.IsNaN(y) Then Return False
    If bTmp AndAlso CurrentPlotSettings.ApplyMinY Then
      bTmp = bTmp And y >= CurrentPlotSettings.MinY
    End If
    If bTmp AndAlso CurrentPlotSettings.ApplyMaxY Then
      bTmp = bTmp And y <= CurrentPlotSettings.MaxY
    End If
    Return bTmp
  End Function

  Private Sub DrawPeriodsChart(NotSelectedColor As System.Windows.Media.Color, HighlightFilter As Boolean)
    SottoTitolo = ""

    Dim _ListaCount As Integer = 0
    Dim _LastPeriod As String = ""
    If CurrentPlotSettings.XAxisChannel Is Nothing Then Exit Sub
    If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
    If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
    End If
    If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
    End If
    Dim PeriodiValidi As List(Of clsPeriod2021) = ListaPeriodiFiltratiPerTwa()
    If HighlightFilter Then
      PeriodiValidi = ListaPeriodiFiltratiPerTipoStraightLine()
    End If
    If CurrentPlotSettings.ColorChannel Is Nothing Then
      CurrentPlotSettings.ColorChannel = CurrentPlotSettings.YAxisChannel
    End If

    SeriesSource.Clear()
    XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
    YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

    Dim Filtro As eTipoFiltro = TipoFiltro()
    Dim Filtro2 As eTipoFiltro = TipoFiltro2()
    Select Case CurrentPlotSettings.SorgenteDati
      Case clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected
        SottoTitolo = "Selected Highlighted"
      Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
        SottoTitolo = "Highlighted:" & SottoTitolo.Replace("Filtered by:", "")
    End Select
    CurrentPlotSettings.LegendIsVisible = True ' CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 20

    If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
    End If

    Dim Adv = New PeriodsChartAdvanced(CurrentPlotSettings, NotSelectedColor, HighlightFilter, Filtro, Filtro2, PeriodiValidi)
    Adv.SetByTypeId()

    If Adv.ChannelValues.Count = 0 Then Exit Sub

    Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
    Dim ListaXYZC As New List(Of clsPrdXYZC)
    Dim idColore As Integer = 0
    For Each ChannelValue As Double In Adv.ChannelValues
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      BinsOfDataSeries.Add(DataSeriesTMP)
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Select Case ChannelValue
            Case -2
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.Red))
            Case -1
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkRed))
            Case 1
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkGreen))
            Case 2
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Color.FromArgb(255, 0, 255, 0)))
            Case Else
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkGray))
          End Select
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Select Case ChannelValue
            Case 0
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.Goldenrod))
            Case 1
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.Blue))
          End Select
        Case Else
          ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
      End Select
      idColore += 1
    Next
    If Not NotSelectedColor = Nothing Then
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      BinsOfDataSeries.Add(DataSeriesTMP)
      ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
    End If

    Dim chXstbd As clsChannel2020
    Dim chXport As clsChannel2020
    Dim chYstbd As clsChannel2020
    Dim chYport As clsChannel2020
    Valori(CurrentPlotSettings.XAxisChannel.LongName, CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
    Valori(CurrentPlotSettings.YAxisChannel.LongName, CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
    If chXstbd.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
    End If
    If chXport.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
    End If
    If chYstbd.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
    End If
    If chYport.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
    End If
    If Not CurrentPlotSettings.FilterChannel Is Nothing Then
      If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
      End If
    End If
    If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
      If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
      End If
    End If

    Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
    _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
    Dim cZ = CurrentPlotSettings.ZAxisChannel
    For Each periodo In PeriodiValidi
      Dim IdIniziale As Integer = TrovaId(periodo, True)
      Dim Idfinale As Integer = TrovaId(periodo, False)
      _ListaCount += 1
      _LastPeriod = periodo.TR.StringaPeriodo
      Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

      Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
      Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
      Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
      Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
      For Each i In periodo.RigheValide.Keys
        'Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
        Dim v As Double = Adv.GetValue(i)
        If Not Double.IsNaN(v) AndAlso ValidRange(i) Then
          Dim Momento As DateTime = DataProvider2020.Momento(i)
          Dim X As Double
          Dim Y As Double
          Dim Z As Double = 0
          If CurrentPlotSettings.WindwardLeewardFunction Then
            X = Valore(Adv.ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
            Y = Valore(Adv.ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
          Else
            X = CurrentPlotSettings.XAxisChannel.Valori(i)
            Y = CurrentPlotSettings.YAxisChannel.Valori(i)
          End If
          If Not cZ Is Nothing Then Z = cZ.Valori(i)
          If Momento.ToOADate > 0 Then
            Select Case CurrentPlotSettings.XAxisChannel.DataType
              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                X = System.Math.Abs(X)
              Case clsChannel2020.eDataType.e180
                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
            End Select
            Select Case CurrentPlotSettings.YAxisChannel.DataType
              Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                Y = System.Math.Abs(Y)
              Case clsChannel2020.eDataType.e180
                If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
            End Select
            If CurrentPlotSettings.XAxisDerivative Then
              Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
              X_ValoreRigaPrev = X
              If Double.IsNaN(Derivata) Then
                X = Double.NaN
              Else
                X_MediaMobile.AggiornaMedia(Derivata)
                X = X_MediaMobile.Valore
              End If
            End If
            If CurrentPlotSettings.YAxisDerivative Then
              Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
              Y_ValoreRigaPrev = Y
              If Double.IsNaN(Derivata) Then
                Y = Double.NaN
              Else
                Y_MediaMobile.AggiornaMedia(Derivata)
                Y = Y_MediaMobile.Valore
              End If
            End If

            If CurrentPlotSettings.GroupingType = clsXYPlotSettings.eGroupingType.e360checks Then
              Y = v
              YLabel = CurrentPlotSettings.XAxisChannel.ShortName & "-" & CurrentPlotSettings.YAxisChannel.ShortName & " (positive when Y on right of X)"
            End If

            MomentoPrev = Momento
            'Dim Indice As Integer = Adv.ChannelValues.IndexOf(v)
            Dim indice = -1
            Dim Id As Integer = Adv.GetIndex(v)

            For idx As Integer = 0 To Adv.ChannelValues.Count - 1
              If Adv.ChannelValues(idx) = Id Then
                indice = idx
                Exit For
              End If
            Next



            'Dim elemento = Adv.ChannelValuesD.FirstOrDefault(Function(K) K.Value = id)

            'If Not elemento.Equals(Nothing) AndAlso Adv.ChannelValuesD.ContainsKey(elemento.Key) Then
            '  Indice = elemento.Key
            'End If
            'If Indice > 47 Then Stop
            If indice > -1 Then
              If periodo.RigheValide(i) Then
                ListaXYZC(indice).Append(X, Y, Z)
                BinsOfDataSeries(indice).Append(X, Y, New clsPuntoMetadata(False))
                TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
                _SubSet.ListaMomenti.Add(Momento)
                _SubSet.ListaValori(0).Add(X)
                _SubSet.ListaValori(1).Add(Y)
              Else
                ListaXYZC.Last.Append(X, Y, Z)
                BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
                TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
              End If
            Else
              ListaXYZC.Last.Append(X, Y, Z)
              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
              TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
            End If
          End If
        End If
        'End If
      Next
    Next

    Dim Bins As Integer = 10
    Dim Ordine = CurrentPlotSettings.GradoTrendLines

    Dim CanaleIsSailCode As Boolean = False
    Select Case CurrentPlotSettings.ColorChannel.Name.ToLower
      Case "mainsail", "headsail", "prodsail", "staysail"
        CanaleIsSailCode = True
    End Select


    Dim idCol As Integer = 0
    'For i As Integer = 0 To BinsOfDataSeries.Count - 1
    For i As Integer = BinsOfDataSeries.Count - 1 To 0 Step -1
      If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
        'ListaXYZC(i).Colore = ColoriDifferenziati(idCol)
        Dim Colore = ListaXYZC(i).Colore
        idCol += 1
        Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

        Dim LineaTmp As New XyScatterRenderableSeries
        LineaTmp.XAxisId = "DefaultAxisId"
        LineaTmp.YAxisId = "DefaultAxisId"
        LineaTmp.PointMarker = New EllipsePointMarker
        LineaTmp.PointMarker.Stroke = ColorePunto
        LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
        LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
        LineaTmp.PointMarker.StrokeThickness = 0
        LineaTmp.PointMarker.Fill = ColorePunto
        If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
          BinsOfDataSeries(i).SeriesName = "Not Selected"
        Else
          If CurrentPlotSettings.GroupingType = clsXYPlotSettings.eGroupingType.eValueBins AndAlso CurrentPlotSettings.ColorChannel.CanaleChiave = clsChannels2020.eCanaliChiave.eSailSet Then
            BinsOfDataSeries(i).SeriesName = DataProvider2020.SailSet(CInt(Adv.ChannelValues(i))) '
          ElseIf CurrentPlotSettings.GroupingType = clsXYPlotSettings.eGroupingType.eValueBins AndAlso CanaleIsSailCode Then
            BinsOfDataSeries(i).SeriesName = DataProvider2020.GetSailNameBySailCode(Adv.ChannelValues(i)) '
          Else
            BinsOfDataSeries(i).SeriesName = Adv.GetName(Adv.ChannelValues(i))
          End If
          'idCol += 1
        End If
        LineaTmp.DataSeries = BinsOfDataSeries(i)
        SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

        If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
          Dim Coppie As New List(Of clsDoubleXY)
          For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
            Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
          Next
          Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
          Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
          Dim xx, yy As New List(Of Double)
          For ii As Integer = 0 To x.Count - 1
            If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
              xx.Add(x(ii))
              yy.Add(y(ii))
            End If
          Next
          x = xx.ToArray
          y = yy.ToArray
          If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
            If Not x.Min = x.Max Then
              Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
              Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
              Dim TrendLine As New FastLineRenderableSeries
              TrendLine.XAxisId = "DefaultAxisId"
              TrendLine.YAxisId = "DefaultAxisId"
              TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
              TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
              TrendLine.Stroke = Colore
              TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
              TrendLine.IsVisible = True
              DataSeriesTmp.AcceptsUnsortedData = True
              For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
                Dim jj As Double = p.Coefficients(0)
                For g As Integer = 1 To Ordine
                  jj += p.Coefficients(g) * j ^ g
                Next
                DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
              Next
              TrendLine.DataSeries = DataSeriesTmp
              Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
              SeriesSource.Add(CSVMtgtup)
            End If
          End If
          If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
            Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
            Dim MainAverage As New XyScatterRenderableSeries
            MainAverage.XAxisId = "DefaultAxisId"
            MainAverage.YAxisId = "DefaultAxisId"
            MainAverage.PointMarker = New EllipsePointMarker
            MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
            MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
            MainAverage.PointMarker.StrokeThickness = 0
            MainAverage.Stroke = Colore
            MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
            MainAverage.IsVisible = True
            DataSeriesMA.AcceptsUnsortedData = False
            DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
            MainAverage.DataSeries = DataSeriesMA
            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
            SeriesSource.Add(CSVMtgtup)
          End If
          If CurrentPlotSettings.StampaTabellaDati Then
            Dim XYpdt As New XYplotDataTable
            XYplotDataTables.Counter += 1
            XYpdt.Order = XYplotDataTables.Counter
            XYpdt.Name = BinsOfDataSeries(i).SeriesName
            XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
            XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
            XYpdt.XValues = x.ToList
            XYpdt.YValues = y.ToList
            XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
            XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
            XYplotDataTables.Tables.Add(XYpdt)
          End If
        End If
      End If

    Next

    VerificaTargetAndTrendLines(SeriesSource)

    If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
      PlottaConsistency(ListaXYZC)
    End If

    PuntiXYZ.Clear()
    For Each L In ListaXYZC
      For i As Integer = 0 To L.X.Count - 1
        PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
      Next
    Next

    Titolo = StringaTitolo(idCol, _LastPeriod)
    Select Case CurrentPlotSettings.GroupingType
      Case clsXYPlotSettings.eGroupingType.eTackOnly
        Titolo &= " Colored By Tack "
      Case clsXYPlotSettings.eGroupingType.eUpDnOnly
        Titolo &= " Colored By Upwind Downwind "
      Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
        Titolo &= " Colored By Tack and UpDn"
      Case clsXYPlotSettings.eGroupingType.eAllTogether
        Titolo &= " All Periods Together "
      Case clsXYPlotSettings.eGroupingType.e360checks
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
      Case clsXYPlotSettings.eGroupingType.eValueBins
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
      Case clsXYPlotSettings.eGroupingType.eColorBins
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
      Case clsXYPlotSettings.eGroupingType.eDayBins
        Titolo &= " Colored By Day "
      Case clsXYPlotSettings.eGroupingType.eMonthBins
        Titolo &= " Colored By Month "
      Case clsXYPlotSettings.eGroupingType.eYearBins
        Titolo &= " Colored By Year "
      Case clsXYPlotSettings.eGroupingType.eKeyBins
        Titolo &= " Colored By Keys "
      Case clsXYPlotSettings.eGroupingType.eRaceBins
        Titolo &= " Colored By Race "
      Case clsXYPlotSettings.eGroupingType.eRaceLegBins
        Titolo &= " Colored By Race and Leg "
    End Select

  End Sub


  Private Sub DrawTimeRangeChart(Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
    SottoTitolo = ""
    Dim _ListaCount As Integer = 0
    Dim _LastPeriod As String = ""

    If CurrentPlotSettings.XAxisChannel Is Nothing Then Exit Sub
    If CurrentPlotSettings.YAxisChannel Is Nothing Then Exit Sub
    If CurrentPlotSettings.XAxisChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.XAxisChannel.ChannelId)
    End If
    If CurrentPlotSettings.YAxisChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.YAxisChannel.ChannelId)
    End If
    Dim PeriodiValidi As List(Of clsPeriod2021) = Nothing
    If HighlightFilter Then
      PeriodiValidi = Lista.Where(Function(x) x.IsChecked).ToList()
    End If
    If CurrentPlotSettings.ColorChannel Is Nothing Then
      CurrentPlotSettings.ColorChannel = CurrentPlotSettings.YAxisChannel
    End If

    SeriesSource.Clear()

    XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
    YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

    Dim Filtro As eTipoFiltro = TipoFiltro()
    Dim Filtro2 As eTipoFiltro = TipoFiltro2()
    Select Case CurrentPlotSettings.SorgenteDati
      Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
        SottoTitolo = "Selected Periods Highlighted"
      Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
        SottoTitolo = "Highlighted:" & SottoTitolo.Replace("Filtered by:", "")
        PeriodiValidi = Lista.ToList()
      Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered
        PeriodiValidi = Lista.ToList()
    End Select
    CurrentPlotSettings.LegendIsVisible = True ' CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 20

    If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
    End If

    Dim Adv = New TimeRangeChartAdvanced(CurrentPlotSettings, NotSelectedColor, HighlightFilter, Filtro, Filtro2, DataPlotSync.VisibleRange, PeriodiValidi)
    Adv.SetByTypeId()

    If Adv.ChannelValues.Count = 0 Then Exit Sub

    Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
    Dim ListaXYZC As New List(Of clsPrdXYZC)
    Dim idColore As Integer = 0
    For Each ChannelValue As Double In Adv.ChannelValues
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      BinsOfDataSeries.Add(DataSeriesTMP)
      ' colori assegnati in base al VALORE del gruppo (non alla sua posizione), come in DrawPeriodsChart
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Select Case ChannelValue
            Case -2 ' Port Downwind
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.Red))
            Case -1 ' Port Upwind
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkRed))
            Case 1 ' Stbd Upwind
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkGreen))
            Case 2 ' Stbd Downwind
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Color.FromArgb(255, 0, 255, 0)))
            Case Else
              ListaXYZC.Add(New clsPrdXYZC(System.Windows.Media.Colors.DarkGray))
          End Select
        Case Else
          ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore, CurrentPlotSettings.GroupingType, ChannelValue = Adv.ChannelValues.First())))
      End Select
      idColore += 1
    Next
    If Not NotSelectedColor = Nothing Then
      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
      DataSeriesTMP.AcceptsUnsortedData = True
      BinsOfDataSeries.Add(DataSeriesTMP)
      ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
    End If

    Dim chXstbd As clsChannel2020
    Dim chXport As clsChannel2020
    Dim chYstbd As clsChannel2020
    Dim chYport As clsChannel2020
    Valori(CurrentPlotSettings.XAxisChannel.LongName, CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
    Valori(CurrentPlotSettings.YAxisChannel.LongName, CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
    If chXstbd.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
    End If
    If chXport.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
    End If
    If chYstbd.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
    End If
    If chYport.Valori Is Nothing Then
      Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
    End If
    If Not CurrentPlotSettings.FilterChannel Is Nothing Then
      If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
      End If
    End If
    If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
      If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
        Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
      End If
    End If

    Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
    _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
    Dim cZ = CurrentPlotSettings.ZAxisChannel
    'For Each periodo In PeriodiValidi
    Dim IdIniziale As Integer = Adv.Periodo.TR.IdRigaIniziale
    Dim Idfinale As Integer = Adv.Periodo.TR.IdRigaFinale
    _ListaCount += 1
    _LastPeriod = Adv.Periodo.TR.StringaPeriodo
    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
    For Each i In Adv.Periodo.RigheValide.Keys
      'Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
      Dim v As Double = Adv.GetValue(i)
      If Not Double.IsNaN(v) Then
        Dim Momento As DateTime = DataProvider2020.Momento(i)
        Dim X As Double
        Dim Y As Double
        Dim Z As Double = 0
        If CurrentPlotSettings.WindwardLeewardFunction Then
          X = Valore(Adv.ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
          Y = Valore(Adv.ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
        Else
          X = CurrentPlotSettings.XAxisChannel.Valori(i)
          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
        End If
        If Not cZ Is Nothing Then Z = cZ.Valori(i)
        If Momento.ToOADate > 0 Then
          Select Case CurrentPlotSettings.XAxisChannel.DataType
            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
              X = System.Math.Abs(X)
            Case clsChannel2020.eDataType.e180
              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
          End Select
          Select Case CurrentPlotSettings.YAxisChannel.DataType
            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
              Y = System.Math.Abs(Y)
            Case clsChannel2020.eDataType.e180
              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
          End Select
          If CurrentPlotSettings.XAxisDerivative Then
            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
            X_ValoreRigaPrev = X
            If Double.IsNaN(Derivata) Then
              X = Double.NaN
            Else
              X_MediaMobile.AggiornaMedia(Derivata)
              X = X_MediaMobile.Valore
            End If
          End If
          If CurrentPlotSettings.YAxisDerivative Then
            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
            Y_ValoreRigaPrev = Y
            If Double.IsNaN(Derivata) Then
              Y = Double.NaN
            Else
              Y_MediaMobile.AggiornaMedia(Derivata)
              Y = Y_MediaMobile.Valore
            End If
          End If

          If CurrentPlotSettings.GroupingType = clsXYPlotSettings.eGroupingType.e360checks Then
            Y = v
            YLabel = CurrentPlotSettings.XAxisChannel.ShortName & "-" & CurrentPlotSettings.YAxisChannel.ShortName & "  (positive when Y on right of X)"
          End If

          MomentoPrev = Momento
          'Dim Indice As Integer = Adv.ChannelValues.IndexOf(v)
          Dim Indice As Integer = Adv.GetIndex(v)
          'If Indice > 47 Then Stop
          If Indice > -1 Then
            If Adv.Periodo.RigheValide(i) Then
              ListaXYZC(Indice).Append(X, Y, Z)
              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
              TL.AccodaCoppia(Adv.Periodo.IsUpwindVmgRange, Adv.Periodo.IsStbd, X, Y)
              _SubSet.ListaMomenti.Add(Momento)
              _SubSet.ListaValori(0).Add(X)
              _SubSet.ListaValori(1).Add(Y)
            Else
              ListaXYZC.Last.Append(X, Y, Z)
              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
              TLNotSel.AccodaCoppia(Adv.Periodo.IsUpwindVmgRange, Adv.Periodo.IsStbd, X, Y)
            End If
          Else
            ListaXYZC.Last.Append(X, Y, Z)
            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
            TLNotSel.AccodaCoppia(Adv.Periodo.IsUpwindVmgRange, Adv.Periodo.IsStbd, X, Y)
          End If
        End If
      End If
      'End If
    Next
    'Next

    Dim Bins As Integer = 10
    Dim Ordine = CurrentPlotSettings.GradoTrendLines



    Dim idCol As Integer = 0
    'For i As Integer = 0 To BinsOfDataSeries.Count - 1
    For i As Integer = BinsOfDataSeries.Count - 1 To 0 Step -1
      If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
        'ListaXYZC(i).Colore = ColoriDifferenziati(idCol)
        Dim Colore = ListaXYZC(i).Colore
        idCol += 1
        Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

        Dim LineaTmp As New XyScatterRenderableSeries
        LineaTmp.XAxisId = "DefaultAxisId"
        LineaTmp.YAxisId = "DefaultAxisId"
        LineaTmp.PointMarker = New EllipsePointMarker
        LineaTmp.PointMarker.Stroke = ColorePunto
        LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
        LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
        LineaTmp.PointMarker.StrokeThickness = 0
        LineaTmp.PointMarker.Fill = ColorePunto
        If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
          BinsOfDataSeries(i).SeriesName = "Not Selected"
        Else
          If CurrentPlotSettings.GroupingType = clsXYPlotSettings.eGroupingType.eValueBins AndAlso CurrentPlotSettings.ColorChannel.CanaleChiave = clsChannels2020.eCanaliChiave.eSailSet Then
            BinsOfDataSeries(i).SeriesName = DataProvider2020.SailSet(CInt(Adv.ChannelValues(i))) '
          Else
            BinsOfDataSeries(i).SeriesName = Adv.GetName(Adv.ChannelValues(i))
          End If
          'idCol += 1
        End If
        LineaTmp.DataSeries = BinsOfDataSeries(i)
        SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

        If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
          Dim Coppie As New List(Of clsDoubleXY)
          For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
            Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
          Next
          Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
          Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
          Dim xx, yy As New List(Of Double)
          For ii As Integer = 0 To x.Count - 1
            If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
              xx.Add(x(ii))
              yy.Add(y(ii))
            End If
          Next
          x = xx.ToArray
          y = yy.ToArray
          If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
            If Not x.Min = x.Max Then
              Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
              Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
              Dim TrendLine As New FastLineRenderableSeries
              TrendLine.XAxisId = "DefaultAxisId"
              TrendLine.YAxisId = "DefaultAxisId"
              TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
              TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
              TrendLine.Stroke = Colore
              TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
              TrendLine.IsVisible = True
              DataSeriesTmp.AcceptsUnsortedData = True
              For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
                Dim jj As Double = p.Coefficients(0)
                For g As Integer = 1 To Ordine
                  jj += p.Coefficients(g) * j ^ g
                Next
                DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
              Next
              TrendLine.DataSeries = DataSeriesTmp
              Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
              SeriesSource.Add(CSVMtgtup)
            End If
          End If
          If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
            Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
            Dim MainAverage As New XyScatterRenderableSeries
            MainAverage.XAxisId = "DefaultAxisId"
            MainAverage.YAxisId = "DefaultAxisId"
            MainAverage.PointMarker = New EllipsePointMarker
            MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
            MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
            MainAverage.PointMarker.StrokeThickness = 0
            MainAverage.Stroke = Colore
            MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
            MainAverage.IsVisible = True
            DataSeriesMA.AcceptsUnsortedData = False
            DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
            MainAverage.DataSeries = DataSeriesMA
            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
            SeriesSource.Add(CSVMtgtup)
          End If
          If CurrentPlotSettings.StampaTabellaDati Then
            Dim XYpdt As New XYplotDataTable
            XYplotDataTables.Counter += 1
            XYpdt.Order = XYplotDataTables.Counter
            XYpdt.Name = BinsOfDataSeries(i).SeriesName
            XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
            XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
            XYpdt.XValues = x.ToList
            XYpdt.YValues = y.ToList
            XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
            XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
            XYplotDataTables.Tables.Add(XYpdt)
          End If
        End If
      End If

    Next

    VerificaTargetAndTrendLines(SeriesSource)

    If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
      PlottaConsistency(ListaXYZC)
    End If

    PuntiXYZ.Clear()
    For Each L In ListaXYZC
      For i As Integer = 0 To L.X.Count - 1
        PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
      Next
    Next

    Titolo = StringaTitolo(idCol, _LastPeriod)
    Select Case CurrentPlotSettings.GroupingType
      Case clsXYPlotSettings.eGroupingType.eTackOnly
        Titolo &= " Colored By Tack "
      Case clsXYPlotSettings.eGroupingType.eUpDnOnly
        Titolo &= " Colored By Upwind Downwind "
      Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
        Titolo &= " Colored By Tack and UpDn"
      Case clsXYPlotSettings.eGroupingType.eAllTogether
        Titolo &= " All Periods Together "
      Case clsXYPlotSettings.eGroupingType.e360checks
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
      Case clsXYPlotSettings.eGroupingType.eValueBins
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
      Case clsXYPlotSettings.eGroupingType.eColorBins
        Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
      Case clsXYPlotSettings.eGroupingType.eDayBins
        Titolo &= " Colored By Day "
      Case clsXYPlotSettings.eGroupingType.eMonthBins
        Titolo &= " Colored By Month "
      Case clsXYPlotSettings.eGroupingType.eYearBins
        Titolo &= " Colored By Year "
      Case clsXYPlotSettings.eGroupingType.eKeyBins
        Titolo &= " Colored By Keys "
      Case clsXYPlotSettings.eGroupingType.eRaceBins
        Titolo &= " Colored By Race "
      Case clsXYPlotSettings.eGroupingType.eRaceLegBins
        Titolo &= " Colored By Race and Leg "
    End Select

  End Sub



  'Private Sub DrawChartPeriodsChannelColoredByChannelValues(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()
  '  Select Case CurrentPlotSettings.SorgenteDati
  '    Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
  '    Case clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected
  '      SottoTitolo = "Selected Highlighted"
  '    Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter, clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
  '      SottoTitolo = "Highlighted:" & SottoTitolo.Replace("Filtered by:", "")
  '  End Select
  '  CurrentPlotSettings.LegendIsVisible = True ' CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 20

  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If
  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
  '  Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  Dim DaEvidenziareSamples As Integer = 0 ' contiene i valori univoci del canale
  '  For Each periodo In PeriodiValidi
  '    periodo.RigheValide.Clear()
  '    For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
  '      Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) Then
  '        Dim Valido As Boolean = False
  '        Dim DaEvidenziare As Boolean = False
  '        If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
  '          Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
  '          DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
  '        Else
  '          If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
  '            DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '            Valido = DaEvidenziare 'se visualizza evidenzia
  '          Else ' 
  '            Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
  '            DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
  '          End If
  '        End If
  '        If Valido Then
  '          If DaEvidenziare Then DaEvidenziareSamples += 1
  '          periodo.RigheValide.Add(i, DaEvidenziare)
  '          If Not ChannelValuesD.ContainsKey(v) Then
  '            ChannelValuesD.Add(v, 0)
  '          End If
  '          ChannelValuesD(v) += 1
  '          ValidSamples += 1
  '        End If
  '      End If
  '    Next
  '  Next

  '  If ChannelValuesD.Count = 0 Then Exit Sub
  '  If ChannelValuesD.Count > CurrentPlotSettings.ColorChannelIntervals Then
  '    DrawChartPeriodsChannelColoredByChannelBins(PeriodiValidi, NotSelectedColor)
  '    Exit Sub
  '  End If

  '  Dim MinPerc As Integer = ValidSamples / 100 * 1
  '  Dim a = ChannelValuesD.Where(Function(x) x.Value > MinPerc).ToList
  '  Dim b = a.OrderBy(Function(x) x.Key).ToList
  '  Dim ChannelValues As List(Of Double) = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim idColore As Integer = 0
  '  For Each ChannelValue As Double In ChannelValues
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
  '    idColore += 1
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For Each i In periodo.RigheValide.Keys
  '      Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '      If Not Double.IsNaN(v) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = ChannelValues.IndexOf(v)
  '          If Indice > -1 Then
  '            If periodo.RigheValide(i) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '          End If
  '        End If
  '      End If
  '      'End If
  '    Next
  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines



  '  Dim idCol As Integer = 0
  '  'For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '  For i As Integer = BinsOfDataSeries.Count - 1 To 0 Step -1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        If CurrentPlotSettings.ColorChannel.CanaleChiave = clsChannels2020.eCanaliChiave.eSailSet Then
  '          BinsOfDataSeries(i).SeriesName = DataProvider2020.SailSet(CInt(ChannelValues(i))) '
  '        Else
  '          BinsOfDataSeries(i).SeriesName = ChannelValues(i).ToString()
  '        End If
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eTackOnly
  '      Titolo &= " Colored By Tack "
  '    Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      Titolo &= " Colored By Upwind Downwind "
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '      Titolo &= " Colored By Tack and UpDn"
  '    Case clsXYPlotSettings.eGroupingType.eAllTogether
  '      Titolo &= " All Periods Together "
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      Titolo &= " Colored By Day "
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      Titolo &= " Colored By Month "
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      Titolo &= " Colored By Year "
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      Titolo &= " Colored By Keys "
  '  End Select

  'End Sub


  'Private Sub DrawChartPeriods360ChecksChannelColoredByChannelValues(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  'FiltroAndatura()
  '  'FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = True ' CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 20

  '  'Dim ValoriCanaleColore As New List(Of Double)
  '  If CurrentPlotSettings.ColorChannel.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.ColorChannel.ChannelId)
  '  End If




  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

  '  Dim ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
  '  If Not NotSelectedColor = Nothing Then
  '    ChannelValuesD.Add(Double.NaN, 0)
  '  End If

  '  Dim ValidSamples As Integer = 0 ' contiene i valori univoci del canale
  '  For Each periodo In PeriodiValidi
  '    periodo.RigheValide.Clear()
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '      Dim v As Double = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 60)
  '      If Not Double.IsNaN(v) Then
  '        'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '        'If Valido Then
  '        Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '        Dim TwaValido As Boolean = RigaValida(i, eTipoFiltro.eNone, Nothing, eTipoFiltro.eNone, Nothing, ChTwa)
  '        If (HighlightFilter And TwaValido) OrElse (Not HighlightFilter And Valido) Then
  '          periodo.RigheValide.Add(i, TwaValido)
  '          If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            If Not ChannelValuesD.ContainsKey(v) Then
  '              ChannelValuesD.Add(v, 0)
  '            End If
  '            ChannelValuesD(v) += 1
  '            ValidSamples += 1
  '          Else
  '            ChannelValuesD(Double.NaN) += 1
  '            ValidSamples += 1
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next

  '  If ChannelValuesD.Count = 0 Then Exit Sub
  '  If ChannelValuesD.Count > CurrentPlotSettings.ColorChannelIntervals Then
  '    DrawChartPeriods360ChecksChannelColoredByChannelBins(PeriodiValidi, NotSelectedColor)
  '    Exit Sub
  '  End If

  '  Dim MinPerc As Integer = ValidSamples / 100 * 1
  '  Dim a = ChannelValuesD.Where(Function(x) x.Value > MinPerc).ToList
  '  Dim b = a.OrderBy(Function(x) x.Key).ToList
  '  Dim ChannelValues As List(Of Double) = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim idColore As Integer = 0
  '  For Each ChannelValue As Double In ChannelValues
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
  '    'If NotSelectedColor = Nothing Then
  '    '  ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore)))
  '    'Else
  '    '  If idColore = 0 Then
  '    '    ListaXYZC.Add(New clsPrdXYZC(Colors.Gray))
  '    '  Else
  '    '    ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(idColore - 1)))
  '    '  End If
  '    'End If
  '    idColore += 1
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)

  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For Each i In periodo.RigheValide.Keys
  '      '
  '      'For i As Integer = IdIniziale To Idfinale
  '      'Dim Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then

  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'If HighlightFilter OrElse (Not HighlightFilter And Valido) Then
  '      'Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
  '      Dim v As Double = CInt(CurrentPlotSettings.ColorChannel.Valori(i) / 60)
  '      If Not Double.IsNaN(v) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If False Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(i), CurrentPlotSettings.YAxisChannel.Valori(i))
  '          Y = delta
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = ChannelValues.IndexOf(v)
  '          If Indice > -1 Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, periodo.RigheValide(i)) Then
  '              'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '          End If
  '        End If
  '      End If
  '      'End If
  '    Next
  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines
  '  Dim idCol As Integer = 0

  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      'Dim Colore = ColoriDifferenziati(idCol)
  '      Dim Colore = ListaXYZC(i).Colore
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = ChannelValues(i).ToString()
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)

  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)
  '  Select Case CurrentPlotSettings.GroupingType
  '    Case clsXYPlotSettings.eGroupingType.eTackOnly
  '      Titolo &= " Colored By Tack "
  '    Case clsXYPlotSettings.eGroupingType.eUpDnOnly
  '      Titolo &= " Colored By Upwind Downwind "
  '    Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
  '      Titolo &= " Colored By Tack and UpDn"
  '    Case clsXYPlotSettings.eGroupingType.eAllTogether
  '      Titolo &= " All Periods Together "
  '    Case clsXYPlotSettings.eGroupingType.e360checks
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eValueBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " Values "
  '    Case clsXYPlotSettings.eGroupingType.eColorBins
  '      Titolo &= " Colored By " & CurrentPlotSettings.ColorChannel.ShortName & " "
  '    Case clsXYPlotSettings.eGroupingType.eDayBins
  '      Titolo &= " Colored By Day "
  '    Case clsXYPlotSettings.eGroupingType.eMonthBins
  '      Titolo &= " Colored By Month "
  '    Case clsXYPlotSettings.eGroupingType.eYearBins
  '      Titolo &= " Colored By Year "
  '    Case clsXYPlotSettings.eGroupingType.eKeyBins
  '      Titolo &= " Colored By Keys "
  '  End Select

  'End Sub

  'Private Sub DrawChartPeriodsChannelColoredByKey(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12
  '  'Stop

  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim Chiavi As New List(Of String)
  '  For Each periodo In PeriodiValidi
  '    If Chiavi.Where(Function(x) x = periodo.Keys).Count = 0 Then
  '      If NotSelectedColor = Nothing OrElse periodo.IsChecked Then
  '        Chiavi.Add(periodo.Keys)
  '        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '        DataSeriesTMP.AcceptsUnsortedData = True
  '        BinsOfDataSeries.Add(DataSeriesTMP)
  '        ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(Chiavi.Count - 1)))
  '      End If
  '    End If
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Chiavi.Add("Not Selected")
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If

  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)
  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then
  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      If HighlightFilter OrElse (Not HighlightFilter And Valido) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = Chiavi.IndexOf(periodo.Keys)
  '          If Indice > -1 Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '              'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              BinsOfDataSeries(Indice).SeriesName = periodo.Keys
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next


  '  Dim DatiPerTL As New List(Of clsTLdata)

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      'Dim Colore = ColoriDifferenziati(idCol)
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      DatiPerTL.Add(New clsTLdata(Chiavi(i), Colore, BinsOfDataSeries(i)))

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = Chiavi(i) '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > Ordine + 1 AndAlso y.Count > Ordine + 1 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & Chiavi(i)
  '            'TrendLine.Name = "TL_" & Chiavi(i)
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            DataSeriesTmp.SeriesName = "TL_" & BinsOfDataSeries(i).SeriesName
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" ' & Chiavi(i)
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = Chiavi(i).ToString
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If
  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource, DatiPerTL)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next


  '  Titolo = StringaTitolo(idCol, _LastPeriod)

  '  Titolo &= " Colored By Key "

  'End Sub


  Private Function VerificaIfCheckedMatter(Periodo As clsPeriod2021) As Boolean
    Select Case CurrentPlotSettings.SorgenteDati
      Case clsXYPlotSettings.eDataSource.eAllStraightLinesReaching, clsXYPlotSettings.eDataSource.eAllStraightLinesVmg, clsXYPlotSettings.eDataSource.eAllStraightLines,
           clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected, clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected,
           clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
        Return True
      Case Else
        Return Periodo.IsChecked
    End Select
  End Function


  'Private Sub DrawChartPeriodsChannelColoredByDay(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  'FiltroAndatura()
  '  'FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)
  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim Chiavi As New List(Of String)
  '  For Each periodo In PeriodiValidi
  '    If Chiavi.Where(Function(x) x = periodo.TR.Start.ToString("dd/MM/yyyy")).Count = 0 Then
  '      Chiavi.Add(periodo.TR.Start.ToString("dd/MM/yyyy"))
  '      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '      DataSeriesTMP.AcceptsUnsortedData = True
  '      BinsOfDataSeries.Add(DataSeriesTMP)
  '      ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(Chiavi.Count - 1)))
  '    End If
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo(True)
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)
  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then
  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      If HighlightFilter OrElse (Not HighlightFilter And Valido) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = Chiavi.IndexOf(periodo.TR.Start.ToString("dd/MM/yyyy"))
  '          If Indice > -1 Then
  '            'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              BinsOfDataSeries(Indice).SeriesName = periodo.TR.Start.ToString("dd/MM/yyyy")
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next

  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = Chiavi(i)  '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))
  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)

  '  Titolo &= " Colored By Day "

  'End Sub

  'Private Sub DrawChartPeriodsChannelColoredByMonth(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  'FiltroAndatura()
  '  'FiltroSailingState()
  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim Chiavi As New List(Of String)
  '  For Each periodo In PeriodiValidi
  '    If Chiavi.Where(Function(x) x = periodo.TR.Start.ToString("MMM yyyy")).Count = 0 Then
  '      Chiavi.Add(periodo.TR.Start.ToString("MMM yyyy"))
  '      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '      DataSeriesTMP.AcceptsUnsortedData = True
  '      BinsOfDataSeries.Add(DataSeriesTMP)
  '      ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(Chiavi.Count - 1)))
  '    End If
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo(True)
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)
  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then
  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      If HighlightFilter OrElse (Not HighlightFilter And Valido) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = Chiavi.IndexOf(periodo.TR.Start.ToString("MMM yyyy"))
  '          If Indice > -1 Then
  '            'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              BinsOfDataSeries(Indice).SeriesName = periodo.TR.Start.ToString("MMM yyyy")
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next


  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)

  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = Chiavi(i)  '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray
  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)

  '  Titolo &= " Colored By Month "

  'End Sub

  'Private Sub DrawChartPeriodsChannelColoredByYear(PeriodiValidi As List(Of clsPeriod2021), Optional NotSelectedColor As System.Windows.Media.Color = Nothing, Optional HighlightFilter As Boolean = False)
  '  Dim _ListaCount As Integer = 0
  '  Dim _LastPeriod As String = ""
  '  SeriesSource.Clear()

  '  XLabel = CurrentPlotSettings.XAxisChannel.LongName & If(CurrentPlotSettings.XAxisDerivative, " Der ", "")
  '  YLabel = CurrentPlotSettings.YAxisChannel.LongName & If(CurrentPlotSettings.YAxisDerivative, " Der ", "")

  '  Dim Filtro As eTipoFiltro = TipoFiltro()
  '  Dim Filtro2 As eTipoFiltro = TipoFiltro2()

  '  CurrentPlotSettings.LegendIsVisible = CurrentPlotSettings.ColorChannelIntervals > 1 And CurrentPlotSettings.ColorChannelIntervals <= 12


  '  Dim BinsOfDataSeries As New List(Of XyDataSeries(Of Double, Double))
  '  Dim ListaXYZC As New List(Of clsPrdXYZC)

  '  Dim ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim Chiavi As New List(Of String)
  '  For Each periodo In PeriodiValidi
  '    If Chiavi.Where(Function(x) x = periodo.TR.Start.ToString("yyyy")).Count = 0 Then
  '      Chiavi.Add(periodo.TR.Start.ToString("yyyy"))
  '      Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '      DataSeriesTMP.AcceptsUnsortedData = True
  '      BinsOfDataSeries.Add(DataSeriesTMP)
  '      ListaXYZC.Add(New clsPrdXYZC(ColoriDifferenziati(Chiavi.Count - 1)))
  '    End If
  '  Next
  '  If Not NotSelectedColor = Nothing Then
  '    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
  '    DataSeriesTMP.AcceptsUnsortedData = True
  '    BinsOfDataSeries.Add(DataSeriesTMP)
  '    ListaXYZC.Add(New clsPrdXYZC(NotSelectedColor))
  '  End If


  '  Dim chXstbd As clsChannel2020
  '  Dim chXport As clsChannel2020
  '  Dim chYstbd As clsChannel2020
  '  Dim chYport As clsChannel2020
  '  Valori(CurrentPlotSettings.XAxisChannel.ChannelId, chXstbd, chXport, XLabel)
  '  Valori(CurrentPlotSettings.YAxisChannel.ChannelId, chYstbd, chYport, YLabel)
  '  If chXstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXstbd.ChannelId)
  '  End If
  '  If chXport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chXport.ChannelId)
  '  End If
  '  If chYstbd.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYstbd.ChannelId)
  '  End If
  '  If chYport.Valori Is Nothing Then
  '    Dim c = DataProvider2020.CanaleDbl(chYport.ChannelId)
  '  End If
  '  If Not CurrentPlotSettings.FilterChannel Is Nothing Then
  '    If CurrentPlotSettings.FilterChannel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.FilterChannel.ChannelId)
  '    End If
  '  End If
  '  If Not CurrentPlotSettings.Filter2Channel Is Nothing Then
  '    If CurrentPlotSettings.Filter2Channel.Valori Is Nothing Then
  '      Dim c = DataProvider2020.CanaleDbl(CurrentPlotSettings.Filter2Channel.ChannelId)
  '    End If
  '  End If

  '  Dim headers As String() = {CurrentPlotSettings.XAxisChannel.ShortName, CurrentPlotSettings.YAxisChannel.ShortName, CurrentPlotSettings.ColorChannel.ShortName}
  '  _SubSet = New clsDataSubSet(headers.ToList, Titolo, SottoTitolo)
  '  Dim cZ = CurrentPlotSettings.ZAxisChannel
  '  For Each periodo In PeriodiValidi
  '    Dim IdIniziale As Integer = TrovaId(periodo, True)
  '    Dim Idfinale As Integer = TrovaId(periodo, False)
  '    _ListaCount += 1
  '    _LastPeriod = periodo.TR.StringaPeriodo(True)
  '    Dim MomentoPrev As DateTime = DataProvider2020.Momento(IdIniziale)
  '    Dim X_ValoreRigaPrev As Double = CurrentPlotSettings.XAxisChannel.Valori(IdIniziale)
  '    Dim X_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.XAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    Dim Y_ValoreRigaPrev As Double = CurrentPlotSettings.YAxisChannel.Valori(IdIniziale)
  '    Dim Y_MediaMobile As New clsMediaMobile(DataProvider2020.Hz, CurrentPlotSettings.YAxisChannel.DataType = clsChannel2020.eDataType.e360)
  '    For i As Integer = IdIniziale To Idfinale
  '      'Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
  '      'Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      'If Valido Then
  '      'If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '      Dim Valido As Boolean = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing)
  '      If HighlightFilter OrElse (Not HighlightFilter And Valido) Then
  '        Dim Momento As DateTime = DataProvider2020.Momento(i)
  '        Dim X As Double
  '        Dim Y As Double
  '        Dim Z As Double = 0
  '        If CurrentPlotSettings.WindwardLeewardFunction Then
  '          X = Valore(ChTwa.Valori(i) >= 0, i, chXstbd, chXport)
  '          Y = Valore(ChTwa.Valori(i) >= 0, i, chYstbd, chYport)
  '        Else
  '          X = CurrentPlotSettings.XAxisChannel.Valori(i)
  '          Y = CurrentPlotSettings.YAxisChannel.Valori(i)
  '        End If
  '        If Not cZ Is Nothing Then Z = cZ.Valori(i)
  '        If Momento.ToOADate > 0 Then
  '          Select Case CurrentPlotSettings.XAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              X = System.Math.Abs(X)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          Select Case CurrentPlotSettings.YAxisChannel.DataType
  '            Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
  '              Y = System.Math.Abs(Y)
  '            Case clsChannel2020.eDataType.e180
  '              If CurrentPlotSettings.UseAbsValFor180 Then Y = System.Math.Abs(Y)
  '          End Select
  '          If CurrentPlotSettings.XAxisDerivative Then
  '            Dim Derivata As Double = (X - X_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            X_ValoreRigaPrev = X
  '            If Double.IsNaN(Derivata) Then
  '              X = Double.NaN
  '            Else
  '              X_MediaMobile.AggiornaMedia(Derivata)
  '              X = X_MediaMobile.Valore
  '            End If
  '          End If
  '          If CurrentPlotSettings.YAxisDerivative Then
  '            Dim Derivata As Double = (Y - Y_ValoreRigaPrev) / Momento.Subtract(MomentoPrev).TotalSeconds
  '            Y_ValoreRigaPrev = Y
  '            If Double.IsNaN(Derivata) Then
  '              Y = Double.NaN
  '            Else
  '              Y_MediaMobile.AggiornaMedia(Derivata)
  '              Y = Y_MediaMobile.Valore
  '            End If
  '          End If
  '          MomentoPrev = Momento
  '          Dim Indice As Integer = Chiavi.IndexOf(periodo.TR.Start.ToString("yyyy"))
  '          If Indice > -1 Then
  '            'If NotSelectedColor = Nothing OrElse IsInSelectedPeriod(i) Then
  '            If Highlight(i, NotSelectedColor, HighlightFilter, Valido) Then
  '              ListaXYZC(Indice).Append(X, Y, Z)
  '              BinsOfDataSeries(Indice).Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TL.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '              BinsOfDataSeries(Indice).SeriesName = periodo.TR.Start.ToString("yyyy")
  '              _SubSet.ListaMomenti.Add(Momento)
  '              _SubSet.ListaValori(0).Add(X)
  '              _SubSet.ListaValori(1).Add(Y)
  '            Else
  '              ListaXYZC.Last.Append(X, Y, Z)
  '              BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '              If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '            End If
  '          Else
  '            ListaXYZC.Last.Append(X, Y, Z)
  '            BinsOfDataSeries.Last.Append(X, Y, New clsPuntoMetadata(False))
  '            If periodo.IsVmgRange Then TLNotSel.AccodaCoppia(periodo.IsUpwindVmgRange, periodo.IsStbd, X, Y)
  '          End If
  '        End If
  '      End If
  '    Next
  '  Next


  '  Dim Bins As Integer = 10
  '  Dim Ordine = CurrentPlotSettings.GradoTrendLines

  '  Dim idCol As Integer = 0
  '  For i As Integer = 0 To BinsOfDataSeries.Count - 1
  '    If (BinsOfDataSeries(i).XValues.Count > CurrentPlotSettings.MinSamples) Then
  '      Dim Colore = ListaXYZC(i).Colore
  '      idCol += 1
  '      Dim ColorePunto = Color.FromArgb(CurrentPlotSettings.DataPointOpacity, Colore.R, Colore.G, Colore.B)


  '      Dim LineaTmp As New XyScatterRenderableSeries
  '      LineaTmp.XAxisId = "DefaultAxisId"
  '      LineaTmp.YAxisId = "DefaultAxisId"
  '      LineaTmp.PointMarker = New EllipsePointMarker
  '      LineaTmp.PointMarker.Stroke = ColorePunto
  '      LineaTmp.PointMarker.Height = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.Width = CurrentPlotSettings.DataPointSize
  '      LineaTmp.PointMarker.StrokeThickness = 0
  '      LineaTmp.PointMarker.Fill = ColorePunto
  '      If Not NotSelectedColor = Nothing AndAlso BinsOfDataSeries(i) Is BinsOfDataSeries.Last Then
  '        BinsOfDataSeries(i).SeriesName = "Not Selected"
  '      Else
  '        BinsOfDataSeries(i).SeriesName = Chiavi(i)  '((i * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " - " & (((i + 1) * Intervallo) + vMin).ToString("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
  '      End If
  '      LineaTmp.DataSeries = BinsOfDataSeries(i)
  '      SeriesSource.Add(New ChartSeriesViewModel(BinsOfDataSeries(i), LineaTmp))

  '      If CurrentPlotSettings.StampaTrendLines OrElse CurrentPlotSettings.StampaMeanAverages OrElse CurrentPlotSettings.StampaTabellaDati Then
  '        Dim Coppie As New List(Of clsDoubleXY)
  '        For ii As Integer = 0 To BinsOfDataSeries(i).YValues.Count - 1
  '          Coppie.Add(New clsDoubleXY(BinsOfDataSeries(i).XValues(ii), BinsOfDataSeries(i).YValues(ii)))
  '        Next
  '        Dim x As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.X).ToArray
  '        Dim y As Double() = Coppie.OrderBy(Function(k) k.X).Select(Function(z) z.Y).ToArray
  '        Dim xx, yy As New List(Of Double)
  '        For ii As Integer = 0 To x.Count - 1
  '          If Not Double.IsNaN(x(ii)) AndAlso Not Double.IsNaN(y(ii)) Then
  '            xx.Add(x(ii))
  '            yy.Add(y(ii))
  '          End If
  '        Next
  '        x = xx.ToArray
  '        y = yy.ToArray

  '        If CurrentPlotSettings.StampaTrendLines AndAlso x.Count() > 4 AndAlso y.Count > 4 Then
  '          If Not x.Min = x.Max Then
  '            Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(x, y, Ordine) ' , MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)
  '            Dim DataSeriesTmp As New XyDataSeries(Of Double, Double)
  '            Dim TrendLine As New FastLineRenderableSeries
  '            TrendLine.XAxisId = "DefaultAxisId"
  '            TrendLine.YAxisId = "DefaultAxisId"
  '            TrendLine.StrokeThickness = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeThickness
  '            TrendLine.StrokeDashArray = AppConfig.ActiveProfile.XYPlotSettings.TrendLinesStrokeArray
  '            TrendLine.Stroke = Colore
  '            TrendLine.Tag = "TL_" & BinsOfDataSeries(i).SeriesName
  '            TrendLine.IsVisible = True
  '            DataSeriesTmp.AcceptsUnsortedData = True
  '            For j As Double = x.Min * 0.995 To x.Max * 1.005 Step (x.Max - x.Min) / 10
  '              Dim jj As Double = p.Coefficients(0)
  '              For g As Integer = 1 To Ordine
  '                jj += p.Coefficients(g) * j ^ g
  '              Next
  '              DataSeriesTmp.Append(j, jj, New clsPuntoMetadata(False))
  '            Next
  '            TrendLine.DataSeries = DataSeriesTmp
  '            Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTmp, TrendLine)
  '            SeriesSource.Add(CSVMtgtup)
  '          End If
  '        End If
  '        If CurrentPlotSettings.StampaMeanAverages AndAlso x.Count() > 1 AndAlso y.Count > 1 Then
  '          Dim DataSeriesMA As New XyDataSeries(Of Double, Double)
  '          Dim MainAverage As New XyScatterRenderableSeries
  '          MainAverage.XAxisId = "DefaultAxisId"
  '          MainAverage.YAxisId = "DefaultAxisId"
  '          MainAverage.PointMarker = New EllipsePointMarker
  '          MainAverage.PointMarker.Height = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.Width = CurrentPlotSettings.DataPointSize * 8
  '          MainAverage.PointMarker.StrokeThickness = 0
  '          MainAverage.Stroke = Colore
  '          MainAverage.Tag = "MeanAverage" & BinsOfDataSeries(i).SeriesName
  '          MainAverage.IsVisible = True
  '          DataSeriesMA.AcceptsUnsortedData = False
  '          DataSeriesMA.Append(x.Average, y.Average, New clsPuntoMetadata(False))
  '          MainAverage.DataSeries = DataSeriesMA
  '          Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesMA, MainAverage)
  '          SeriesSource.Add(CSVMtgtup)
  '        End If
  '        If CurrentPlotSettings.StampaTabellaDati Then
  '          Dim XYpdt As New XYplotDataTable
  '          XYplotDataTables.Counter += 1
  '          XYpdt.Order = XYplotDataTables.Counter
  '          XYpdt.Name = BinsOfDataSeries(i).SeriesName
  '          XYpdt.Xchannel = CurrentPlotSettings.XAxisChannel
  '          XYpdt.Ychannel = CurrentPlotSettings.YAxisChannel
  '          XYpdt.XValues = x.ToList
  '          XYpdt.YValues = y.ToList
  '          XYpdt.ShowTgtUp = CurrentPlotSettings.ShowUpwindVmg
  '          XYpdt.ShowTgtDn = CurrentPlotSettings.ShowDownwindVmg
  '          XYplotDataTables.Tables.Add(XYpdt)
  '        End If
  '      End If
  '    End If

  '  Next

  '  VerificaTargetAndTrendLines(SeriesSource)


  '  If CurrentPlotSettings.PlotXDistribution OrElse CurrentPlotSettings.PlotYDistribution Then
  '    PlottaConsistency(ListaXYZC)
  '  End If

  '  PuntiXYZ.Clear()
  '  For Each L In ListaXYZC
  '    For i As Integer = 0 To L.X.Count - 1
  '      PuntiXYZ.Add(New clsXYZColore(ListaXYZC.IndexOf(L), L.X(i), L.Y(i), L.Z(i), L.Colore))
  '    Next
  '  Next

  '  Titolo = StringaTitolo(idCol, _LastPeriod)

  '  Titolo &= " Colored By Year "

  'End Sub



  Private Function StringaTitolo(NrOfPeriods As Integer, LastPeriodDescription As String) As String
    'If NrOfPeriods = 1 Then Return LastPeriodDescription
    Select Case CurrentPlotSettings.SorgenteDati
        'Case clsXYPlotSettings.eDataSource.eSelectedAccelerationsKeySpeeds
        '  Titolo = "" & _ListaCount & " Accelerations [MinSpeed to TakeOff]"
        'Case clsXYPlotSettings.eDataSource.eSelectedAccelerationsVisibleRange
        '  Titolo = "" & _ListaCount & " Accelerations [" & ValoreConSegno(AccVisibleRange.Min) & " to " & ValoreConSegno(AccVisibleRange.Max) & "]"
      Case clsXYPlotSettings.eDataSource.eSelectedManoeuversEntryToExit
        Return "" & NrOfPeriods & " Manoeuvers [Entry To Exit]"
      Case clsXYPlotSettings.eDataSource.eSelectedManoeuversVisibleRange
        Return "" & NrOfPeriods & " Manoeuvers [" & ValoreConSegno(PavarotVisibleRange.Min) & " to " & ValoreConSegno(PavarotVisibleRange.Max) & "]"
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLinesVmg, clsXYPlotSettings.eDataSource.eAllStraightLinesVmg, clsXYPlotSettings.eDataSource.eAllStraightLinesVmgVsSelected
        Return UpDnVmgInStringa() & "Vmg Straight Lines"
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLines, clsXYPlotSettings.eDataSource.eAllStraightLines, clsXYPlotSettings.eDataSource.eAllStraightLinesVsSelected
        Return UpDnVmgInStringa() & "Straight Lines"
      Case clsXYPlotSettings.eDataSource.eSelectedStraightLinesReaching, clsXYPlotSettings.eDataSource.eAllStraightLinesReaching, clsXYPlotSettings.eDataSource.eAllStraightLinesReachingVsSelected
        Return UpDnVmgInStringa() & "Reaching Straight Lines"
      Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVsFilter
        Return UpDnVmgInStringa() & "Straight Lines"
      Case clsXYPlotSettings.eDataSource.eSelectedStrLinesVmgVsFilter
        Return UpDnVmgInStringa() & "Vmg Straight Lines"
      Case clsXYPlotSettings.eDataSource.eSelectedStrLinesReachingVsFilter
        Return UpDnVmgInStringa() & "Reaching Straight Lines"
      Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrentVisibleRangeFiltered, clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods, clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
        Return UpDnVmgInStringa() & "Visible Range"
      Case Else
        Return NrOfPeriods & " " & CurrentPlotSettings.SorgenteDati.ToString.TrimStart("e")
    End Select
  End Function

  Private Function UpDnVmgInStringa() As String
    If CurrentPlotSettings.ShowUpwindVmg And CurrentPlotSettings.ShowDownwindVmg And CurrentPlotSettings.ShowNotVmg Then
      Return "" & PortStbdInStringa()
    ElseIf CurrentPlotSettings.ShowUpwindVmg And CurrentPlotSettings.ShowDownwindVmg Then
      Return "Up/Dn " & PortStbdInStringa()
    ElseIf CurrentPlotSettings.ShowUpwindVmg And CurrentPlotSettings.ShowNotVmg Then
      Return "Up/Rc " & PortStbdInStringa()
    ElseIf CurrentPlotSettings.ShowDownwindVmg And CurrentPlotSettings.ShowNotVmg Then
      Return "Dn/Rc " & PortStbdInStringa()
    ElseIf CurrentPlotSettings.ShowUpwindVmg Then
      Return "Upwind " & PortStbdInStringa()
    ElseIf CurrentPlotSettings.ShowDownwindVmg Then
      Return "Downwind " & PortStbdInStringa()
    End If
    Return Nothing
  End Function

  Private Function PortStbdInStringa() As String
    If CurrentPlotSettings.ShowPort And CurrentPlotSettings.ShowStbd Then
      Return ""
    ElseIf CurrentPlotSettings.ShowPort Then
      Return "Port "
    ElseIf CurrentPlotSettings.ShowStbd Then
      Return "Stbd "
    End If
    Return ""
  End Function

  Private Sub PlottaConsistency(ListaXYC As List(Of clsPrdXYZC))

    'PlottaConsistencyKDE(ListaXYC)
    'Exit Sub
    'PlottaConsistency(ListaXYC, "DefaultAxisId", "DefaultAxisId")
    'Exit Sub

    If ListaXYC.Count = 0 Then Exit Sub
    Dim xMax As Double = Nothing
    Dim xMin As Double = Nothing
    Dim yMax As Double = Nothing
    Dim yMin As Double = Nothing
    Dim primogiro As Boolean = True
    For Each elemento In ListaXYC
      For i As Integer = 0 To elemento.X.Count - 1
        'If i = 2699 Then Stop
        Dim Vx As Double = elemento.X(i)
        Dim Vy As Double = elemento.Y(i)
        If Not Double.IsNaN(Vx) AndAlso Not Double.IsNaN(Vy) Then
          If Not Double.IsInfinity(Vx) AndAlso Not Double.IsInfinity(Vy) Then
            If primogiro Then
              xMax = Vx
              xMin = Vx
              yMax = Vy
              yMin = Vy
              primogiro = False
            Else
              xMax = System.Math.Max(xMax, Vx)
              xMin = System.Math.Min(xMin, Vx)
              yMax = System.Math.Max(yMax, Vy)
              yMin = System.Math.Min(yMin, Vy)
            End If
          End If
        End If
      Next
    Next
    Dim IntervalloX As Double = System.Math.Abs(xMax - xMin) / CurrentPlotSettings.DistributionIntervals
    Dim IntervalloY As Double = System.Math.Abs(yMax - yMin) / CurrentPlotSettings.DistributionIntervals
    If IntervalloX = 0 Then IntervalloX = 1
    If IntervalloY = 0 Then IntervalloY = 1
    Dim Xtotali As Integer
    Dim Ytotali As Integer
    For Each elemento In ListaXYC
      For i As Integer = 0 To elemento.X.Count - 1
        Dim Vx As Double = elemento.X(i)
        Dim Vy As Double = elemento.Y(i)
        If Not Double.IsNaN(Vx) AndAlso Not Double.IsNaN(Vy) Then
          Xtotali += 1
          Ytotali += 1
        End If
      Next
    Next

    For Each elemento In ListaXYC
      Dim DataSeriesLeft As New XyDataSeries(Of Double, Double)
      Dim DataSeriesBottom As New XyDataSeries(Of Double, Double)

      DataSeriesLeft.AcceptsUnsortedData = True
      DataSeriesBottom.AcceptsUnsortedData = True



      Dim ValoriX(CurrentPlotSettings.DistributionIntervals) As Double
      Dim ValoriY(CurrentPlotSettings.DistributionIntervals) As Double
      For i As Integer = 0 To elemento.X.Count - 1
        Dim Vx As Double = elemento.X(i)
        Dim Vy As Double = elemento.Y(i)
        If Not Double.IsNaN(Vx) AndAlso Not Double.IsNaN(Vy) Then
          If Not Double.IsInfinity(Vx) AndAlso Not Double.IsInfinity(Vy) Then
            Dim IndiceX As Integer = System.Math.Round((Vx - xMin) / IntervalloX)
            Dim IndiceY As Integer = System.Math.Round((Vy - yMin) / IntervalloY)
            ValoriX(IndiceX) += 1
            ValoriY(IndiceY) += 1
          End If
        End If
      Next
      For i As Integer = 0 To ValoriX.Count - 1
        DataSeriesBottom.Append(i * IntervalloX + xMin, ValoriX(i) / Xtotali * 100, New clsPuntoMetadata(False))
        DataSeriesLeft.Append(i * IntervalloY + yMin, ValoriY(i) / Ytotali * 100, New clsPuntoMetadata(False))
      Next

      Dim LineaBottom As New StackedColumnRenderableSeries
      LineaBottom.XAxisId = "BottomX" ' {Binding SecondaryAxisVisibilty}
      LineaBottom.YAxisId = "BottomY"
      LineaBottom.StackedGroupId = "Xdistribution"
      LineaBottom.DataPointWidth = CurrentPlotSettings.DistributionBarWidth / 100
      LineaBottom.StrokeThickness = 0
      LineaBottom.Fill = New SolidColorBrush(Color.FromArgb(CurrentPlotSettings.DistributionOpacity, elemento.Colore.R, elemento.Colore.G, elemento.Colore.B))
      LineaBottom.DataSeries = DataSeriesBottom

      SciChart.Charting.ChartModifiers.LegendModifier.SetIncludeSeries(LineaBottom, False)



      Dim CSVMtmp2 As New ChartSeriesViewModel(DataSeriesBottom, LineaBottom)
      If CurrentPlotSettings.PlotXDistribution Then
        SeriesSource.Insert(0, CSVMtmp2)
      End If

      Dim LineaLeft As New StackedColumnRenderableSeries
      LineaLeft.XAxisId = "LeftX"
      LineaLeft.YAxisId = "LeftY"
      LineaLeft.StackedGroupId = "Ydistribution"
      LineaLeft.DataPointWidth = CurrentPlotSettings.DistributionBarWidth / 100
      LineaLeft.StrokeThickness = 0
      LineaLeft.Fill = New SolidColorBrush(Color.FromArgb(CurrentPlotSettings.DistributionOpacity, elemento.Colore.R, elemento.Colore.G, elemento.Colore.B))
      LineaLeft.DataSeries = DataSeriesLeft
      SciChart.Charting.ChartModifiers.LegendModifier.SetIncludeSeries(LineaLeft, False)

      Dim CSVMtmp3 As New ChartSeriesViewModel(DataSeriesLeft, LineaLeft)
      If CurrentPlotSettings.PlotYDistribution Then
        SeriesSource.Insert(0, CSVMtmp3)
      End If
    Next

  End Sub


  Private Class TimeRangeChartAdvanced
    Public CurrentPlotSettings As clsXYPlotSettings
    Public ChTwa As clsChannel2020 'DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Public ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
    Public ValidSamples As Integer = 0 ' contiene i valori univoci del canale
    Public DaEvidenziareSamples As Integer = 0 ' contiene i valori univoci del canale
    Public NotSelectedColor As System.Windows.Media.Color
    Public HighlightFilter As Boolean
    Public Filtro As clsSciChartXyPlotViewModel.eTipoFiltro
    Public Filtro2 As clsSciChartXyPlotViewModel.eTipoFiltro
    Public TimeRange As clsTimeRange
    Public Periodo As clsPeriod2021
    Public PeriodiValidi As List(Of clsPeriod2021)
    Public ChannelValues As List(Of Double)

    Public Sub New(CPS As clsXYPlotSettings, NSC As System.Windows.Media.Color, HF As Boolean,
                   F As clsSciChartXyPlotViewModel.eTipoFiltro, F2 As clsSciChartXyPlotViewModel.eTipoFiltro, TR As clsTimeRange, PV As List(Of clsPeriod2021))
      CurrentPlotSettings = CPS
      NotSelectedColor = NSC
      HighlightFilter = HF
      Filtro = F
      Filtro2 = F2
      TimeRange = TR
      PeriodiValidi = PV
      If PeriodiValidi Is Nothing Then PeriodiValidi = New List(Of clsPeriod2021)
      ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Periodo = New clsPeriod2021(TR, clsPeriod2021.ePeriodType.eStraightLineVmg)
    End Sub

    Dim DayString As String = "yyyy MMM dd"
    Dim MonthString As String = "yyyy MMM"
    Dim YearString As String = "yyyy"


    Dim FiltroAbilitato As Boolean = False
    Public Sub SetByTypeId()
      Select Case CurrentPlotSettings.SorgenteDati
        Case clsXYPlotSettings.eDataSource.eCurrentVisibleRange, clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
          FiltroAbilitato = False
        Case Else
          FiltroAbilitato = True
      End Select

      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          SetByAllTogether()
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          SetByPortStbd()
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          SetByUpDn()
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          SetByUpDnPortStbd()
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          SetByAllTogether()
        Case clsXYPlotSettings.eGroupingType.eValueBins
          SetByColorChannelValues()
        Case clsXYPlotSettings.eGroupingType.eColorBins
          SetByColorChannelBins()
        Case clsXYPlotSettings.eGroupingType.eDayBins
          SetByTimeBins(DayString)
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          SetByTimeBins(MonthString)
        Case clsXYPlotSettings.eGroupingType.eYearBins
          SetByTimeBins(YearString)
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          SetByKeyBins()
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          SetByRaceNameBins()
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          SetByRaceLegNameBins()
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          SetByAllTogether()
        Case Else
          Exit Sub
      End Select


    End Sub

    Public Function GetIndex(value As Double) As Integer
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          Return 0 ' c'é solo un gruppo
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return GetPortStbdIndex(value)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return GetUpDnIndex(value)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return GetUpDnPortStbdIndex(value)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return GetAllTogetherIndex(value)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          If ForceSingleValuesToIntervals Then Return GetIntervalIndex(value)
          Return GetChannelValuesIndex(value)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return GetIntervalIndex(value)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(DayString))
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(MonthString))
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(YearString))
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Dim p = GetPeriodByRowId(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.Keys)
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Dim p = GetPeriodByRowId(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.RaceName)
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Dim p = GetPeriodByRowId(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.RaceLegName)
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Return GetAllTogetherIndex(value)
          ''Dim IdRiga As Integer = value
          ''Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= IdRiga AndAlso x.TR.IdRigaFinale >= IdRiga).FirstOrDefault
          'Dim p = GetIndexFast2(value)
          'If p Is Nothing Then Return -1
          'Return GetKeyIndex(p.StringaPeriodoXYplot)
        Case Else
          Return -1
      End Select

    End Function

    Private Function GetPeriodByRowId(value As Double) As clsPeriod2021
      Dim lo As Integer = 0
      Dim hi As Integer = PeriodiValidi.Count - 1

      While lo <= hi
        Dim mid As Integer = (lo + hi) \ 2
        Dim tr = PeriodiValidi(mid).TR

        If value < tr.IdRigaIniziale Then
          hi = mid - 1
        ElseIf value > tr.IdRigaFinale Then
          lo = mid + 1
        Else
          Return PeriodiValidi(mid) ' trovato
        End If
      End While

      Return Nothing
    End Function

    Public Function GetName(Id As Integer) As String
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          Return "360 check"
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return GetNameByValuePortStbd(Id)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return GetNameByValueUpDn(Id)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return GetNameByValueUpDnPortStbd(Id)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return GetNameByValueAllTogether(Id)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          If ForceSingleValuesToIntervals Then Return GetNameByInterval(Id)
          Return GetNameByChannelValue(Id)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return GetNameByInterval(Id)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Return GetNameByValueAllTogether(Id)
          'Return GetNameByKey(Id)
        Case Else
          Return "ERRORE"
      End Select

    End Function

    Public Function GetValue(IdRiga As Integer) As Double
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          Return DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(IdRiga), CurrentPlotSettings.YAxisChannel.Valori(IdRiga))
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          Return CurrentPlotSettings.ColorChannel.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return CurrentPlotSettings.ColorChannel.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Return IdRiga
        Case Else
          Return -1
      End Select

    End Function

    Dim minValue As Double = Double.NaN
    Dim maxValue As Double = Double.NaN
    Dim Chiavi As New List(Of String)

    Sub SetByKeyBins() 'valori in gruppi

      Dim ChiaviTmp As New List(Of String)
      ChiaviTmp.Add("notaperiod")
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            Dim p = GetPeriodByRowId(i)
            If Not p Is Nothing AndAlso Not ChiaviTmp.Contains(p.Keys) Then
              ChiaviTmp.Add(p.Keys)
            End If
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim IdChiave As Integer = 0 'ChiaviTmp.IndexOf("notaperiod")
            If Not p Is Nothing Then
              IdChiave = ChiaviTmp.IndexOf(p.Keys)
            End If
            If Not ChannelValuesD.ContainsKey(IdChiave) Then
              ChannelValuesD.Add(IdChiave, 0)
            End If
            ChannelValuesD(IdChiave) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim IdChiave As Integer = ChiaviTmp.IndexOf(Periodo.Keys)
                  If Not ChannelValuesD.ContainsKey(IdChiave) Then
                    ChannelValuesD.Add(IdChiave, 0)
                  End If
                  ChannelValuesD(IdChiave) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByRaceNameBins() 'valori in gruppi

      Dim ChiaviTmp As New List(Of String)
      ChiaviTmp.Add("notaperiod")
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            Dim p = GetPeriodByRowId(i)
            If Not p Is Nothing AndAlso Not ChiaviTmp.Contains(p.RaceName) Then
              ChiaviTmp.Add(p.RaceName)
            End If
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim IdChiave As Integer = 0 'ChiaviTmp.IndexOf("notaperiod")
            If Not p Is Nothing Then
              IdChiave = ChiaviTmp.IndexOf(p.RaceName)
            End If
            If Not ChannelValuesD.ContainsKey(IdChiave) Then
              ChannelValuesD.Add(IdChiave, 0)
            End If
            ChannelValuesD(IdChiave) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim IdChiave As Integer = ChiaviTmp.IndexOf(Periodo.RaceName)
                  If Not ChannelValuesD.ContainsKey(IdChiave) Then
                    ChannelValuesD.Add(IdChiave, 0)
                  End If
                  ChannelValuesD(IdChiave) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByRaceLegNameBins() 'valori in gruppi

      Dim ChiaviTmp As New List(Of String)
      ChiaviTmp.Add("notaperiod")
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            Dim p = GetPeriodByRowId(i)
            If Not p Is Nothing AndAlso Not ChiaviTmp.Contains(p.RaceLegName) Then
              ChiaviTmp.Add(p.RaceLegName)
            End If
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim IdChiave As Integer = 0 'ChiaviTmp.IndexOf("notaperiod")
            If Not p Is Nothing Then
              IdChiave = ChiaviTmp.IndexOf(p.RaceLegName)
            End If
            If Not ChannelValuesD.ContainsKey(IdChiave) Then
              ChannelValuesD.Add(IdChiave, 0)
            End If
            ChannelValuesD(IdChiave) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim IdChiave As Integer = ChiaviTmp.IndexOf(Periodo.RaceLegName)
                  If Not ChannelValuesD.ContainsKey(IdChiave) Then
                    ChannelValuesD.Add(IdChiave, 0)
                  End If
                  ChannelValuesD(IdChiave) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetChannelValues()
      Dim b = ChannelValuesD.OrderBy(Function(x) x.Key).ToList
      ChannelValues = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale
    End Sub


    Function GetKeyIndex(Chiave As String) As Integer
      Dim idx = Chiavi.IndexOf(Chiave)
      Return ChannelValues.IndexOf(idx)
    End Function

    Function GetNameByKey(KeyId As Integer) As String
      Return Chiavi(KeyId)
    End Function



    Sub SetByColorChannelBins() 'valori in gruppi
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                End If
              End If
            End If
          Next
        Next
      End If


      For Each riga In Periodo.RigheValide
        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(riga.Key)
        If Double.IsNaN(minValue) Then
          minValue = v
          maxValue = v
        Else
          If v < minValue Then minValue = v
          If v > maxValue Then maxValue = v
        End If
      Next
      For Each riga In Periodo.RigheValide
        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(riga.Key)
        Dim binnedValue As Double = GetIntervalIndex(v)
        If Not ChannelValuesD.ContainsKey(binnedValue) Then
          ChannelValuesD.Add(binnedValue, 0)
        End If
        ChannelValuesD(binnedValue) += 1
        ValidSamples += 1
      Next

      SetChannelValues()
    End Sub

    Function GetIntervalIndex(value As Double) As Integer
      Dim numIntervals As Integer = CurrentPlotSettings.ColorChannelIntervals
      If numIntervals <= 0 Then
        Throw New ArgumentException("Il numero di intervalli deve essere maggiore di zero.")
      End If
      If value <= minValue Then
        Return 0
      End If
      If value >= maxValue Then
        Return numIntervals - 1
      End If
      Dim intervalSize As Double = (maxValue - minValue) / numIntervals
      Dim index As Integer = CInt(Math.Floor((value - minValue) / intervalSize))
      ' Protezione contro errori di arrotondamento
      If index >= numIntervals Then
        index = numIntervals - 1
      End If
      Return index
    End Function

    Function GetNameByInterval(intervallo As Integer) As String
      Dim numIntervals As Integer = CurrentPlotSettings.ColorChannelIntervals
      If numIntervals <= 0 Then
        Throw New ArgumentException("Il numero di intervalli deve essere maggiore di zero.")
      End If
      Dim intervalSize As Double = (maxValue - minValue) / numIntervals
      Dim lowerBound As Double = minValue + (intervallo * intervalSize)
      Dim upperBound As Double = lowerBound + intervalSize
      'Return String.Format("[{0:F2} - {1:F2}]", lowerBound, upperBound)
      Return CurrentPlotSettings.ColorChannel.ShortName & " " & FormatInterval(lowerBound, upperBound, CurrentPlotSettings.ColorChannel.Decimals) & " " & CurrentPlotSettings.ColorChannel.ShortUM
    End Function

    Function FormatInterval(lowerBound As Double, upperBound As Double, Optional decimals As Integer = 2) As String
      Dim formatString As String = "F" & decimals.ToString()
      Return String.Format("{0:" & formatString & "}-{1:" & formatString & "}", lowerBound, upperBound)
    End Function

    Sub SetByColorChannelValues() 'valori univoci del canale
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            If Not ChannelValuesD.ContainsKey(v) Then
              ChannelValuesD.Add(v, 0)
            End If
            ChannelValuesD(v) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  If Not ChannelValuesD.ContainsKey(v) Then
                    ChannelValuesD.Add(v, 0)
                  End If
                  ChannelValuesD(v) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If




      SetChannelValues()
      If Not CurrentPlotSettings.ColorChannel.CanaleChiave = clsChannels2020.eCanaliChiave.eSailSet AndAlso ChannelValues.Count > CurrentPlotSettings.ColorChannelIntervals Then
        ChannelValuesD.Clear()
        SetByColorChannelBins()
        ForceSingleValuesToIntervals = True
      End If
    End Sub

    Dim ForceSingleValuesToIntervals = False

    Function GetChannelValuesIndex(value As Double) As Integer
      Return ChannelValues.IndexOf(value)
    End Function

    Function GetNameByChannelValue(value As Integer) As String
      Return ChannelValues(value).ToString() '("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
    End Function


    'Sub SetBySinglePeriod()
    '  Dim ChiaviTmp As New List(Of String)
    '  Periodo.RigheValide.Clear()
    '  Dim str As String = Periodo.StringaPeriodoXYplot
    '  If Not ChiaviTmp.Contains(str) Then
    '    ChiaviTmp.Add(str)
    '  End If
    '  For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
    '    Dim v As Double = ChTwa.Valori(i)
    '    If Not Double.IsNaN(v) Then
    '      Dim Valido As Boolean = False
    '      Dim DaEvidenziare As Boolean = False
    '      If FiltroAbilitato Then
    '        Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
    '      Else
    '        'se il filtro non e'abilitato vengono presi tutti i dati del range twa
    '        Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
    '      End If
    '      If Valido Then
    '        If NotSelectedColor = Nothing Then
    '          DaEvidenziare = True
    '        Else
    '          Select Case CurrentPlotSettings.SorgenteDati
    '            Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
    '              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
    '            Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
    '              'se la riga coincide con un periodo selezionato
    '              Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
    '              If p Is Nothing Then
    '                DaEvidenziare = False
    '              Else
    '                DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
    '              End If
    '            Case Else
    '              DaEvidenziare = True 'evidenzia se selezionato
    '          End Select
    '        End If
    '        If DaEvidenziare Then DaEvidenziareSamples += 1
    '        Periodo.RigheValide.Add(i, DaEvidenziare)
    '        Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
    '        If Not ChannelValuesD.ContainsKey(IdChiave) Then
    '          ChannelValuesD.Add(IdChiave, 0)
    '        End If
    '        ChannelValuesD(IdChiave) += 1
    '        ValidSamples += 1
    '      End If
    '    End If
    '  Next

    '  'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
    '  For Each p In PeriodiValidi
    '    For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
    '      Dim v As Double = ChTwa.Valori(i)
    '      If Not Double.IsNaN(v) Then
    '        Dim idRiga As Integer = i
    '                      If Not Periodo.RigheValide.ContainsKey(idRiga) Then
    '          Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
    '          If DaEvidenziare Then
    '            DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
    '            Periodo.RigheValide.Add(i, True)
    '            Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
    '            If Not ChannelValuesD.ContainsKey(IdChiave) Then
    '              ChannelValuesD.Add(IdChiave, 0)
    '            End If
    '            ChannelValuesD(IdChiave) += 1
    '            ValidSamples += 1
    '          End If
    '        End If
    '      End If
    '    Next
    '  Next

    '  Chiavi.Clear()
    '  For Each chiave In ChiaviTmp.ToList
    '    If chiave.Trim = "" Then chiave = "not assigned"
    '    Chiavi.Add(chiave)
    '  Next
    '  SetChannelValues()
    'End Sub

    Sub SetByTimeBins(TimeString As String)
      Dim ChiaviTmp As New List(Of String)
      Periodo.RigheValide.Clear()

      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As DateTime = DataProvider2020.Momento(i)
        If Not v = Nothing Then
          Dim str As String = v.ToString(TimeString)
          If Not ChiaviTmp.Contains(str) Then
            ChiaviTmp.Add(str)
          End If
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
            If Not ChannelValuesD.ContainsKey(IdChiave) Then
              ChannelValuesD.Add(IdChiave, 0)
            End If
            ChannelValuesD(IdChiave) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim str As String = v.ToString(TimeString)
                  Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
                  If Not ChannelValuesD.ContainsKey(IdChiave) Then
                    ChannelValuesD.Add(IdChiave, 0)
                  End If
                  ChannelValuesD(IdChiave) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByPortStbd()
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim vv As Integer = IIf(v >= 0, 1, -1)
            If Not ChannelValuesD.ContainsKey(vv) Then
              ChannelValuesD.Add(vv, 0)
            End If
            ChannelValuesD(vv) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim vv As Integer = IIf(v >= 0, 1, -1)
                  If Not ChannelValuesD.ContainsKey(vv) Then
                    ChannelValuesD.Add(vv, 0)
                  End If
                  ChannelValuesD(vv) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      SetChannelValues()
    End Sub

    Function GetNameByValuePortStbd(value As Integer) As String
      Select Case value
        Case -1
          Return "Port"
        Case Else
          Return "Stbd"
      End Select
    End Function

    Function GetPortStbdIndex(value As Double) As Integer
      Dim vv As Integer = IIf(value >= 0, 1, -1)
      Return ChannelValues.IndexOf(vv)
    End Function

    Sub SetByUpDn()
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim vv As Integer = IIf(Math.Abs(v) >= 90, 1, 0)
            If Not ChannelValuesD.ContainsKey(vv) Then
              ChannelValuesD.Add(vv, 0)
            End If
            ChannelValuesD(vv) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim vv As Integer = IIf(Math.Abs(v) >= 90, 1, 0)
                  If Not ChannelValuesD.ContainsKey(vv) Then
                    ChannelValuesD.Add(vv, 0)
                  End If
                  ChannelValuesD(vv) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If
      SetChannelValues()
    End Sub


    Function GetNameByValueUpDn(value As Integer) As String
      Select Case value
        Case 0
          Return "Upwind"
        Case Else
          Return "Downwind"
      End Select
    End Function

    Function GetUpDnIndex(value As Double) As Integer
      Dim vv As Integer = IIf(Math.Abs(value) >= 90, 1, 0)
      Return ChannelValues.IndexOf(vv)
    End Function

    Sub SetByUpDnPortStbd()
      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            Dim vv As Integer = 0
            If Math.Abs(v) < 90 Then 'bolina
              vv = IIf(v >= 0, 1, -1)
            Else
              vv = IIf(v >= 0, 2, -2)
            End If
            If Not ChannelValuesD.ContainsKey(vv) Then
              ChannelValuesD.Add(vv, 0)
            End If
            ChannelValuesD(vv) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  Dim vv As Integer = 0
                  If Math.Abs(v) < 90 Then 'bolina
                    vv = IIf(v >= 0, 1, -1)
                  Else
                    vv = IIf(v >= 0, 2, -2)
                  End If
                  If Not ChannelValuesD.ContainsKey(vv) Then
                    ChannelValuesD.Add(vv, 0)
                  End If
                  ChannelValuesD(vv) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      SetChannelValues()
    End Sub

    Function GetNameByValueUpDnPortStbd(value As Integer) As String
      Select Case value
        Case -2
          Return "Port Downwind"
        Case -1
          Return "Port Upwind"
        Case 1
          Return "Stbd Upwind"
        Case 2
          Return "Stbd Downwind"
        Case Else
          Return "Not Selected"
      End Select
    End Function

    Function GetUpDnPortStbdIndex(value As Double) As Integer
      Dim vv As Integer = 0
      If Math.Abs(value) < 90 Then 'bolina
        vv = IIf(value >= 0, 1, -1)
      Else
        vv = IIf(value >= 0, 2, -2)
      End If
      'Return vv
      Return ChannelValues.IndexOf(vv)
    End Function

    Public Sub SetByAllTogether()

      Periodo.RigheValide.Clear()
      For i As Integer = Periodo.TR.IdRigaIniziale To Periodo.TR.IdRigaFinale
        Dim v As Double = ChTwa.Valori(i)
        If Not Double.IsNaN(v) Then
          Dim Valido As Boolean = False
          Dim DaEvidenziare As Boolean = False
          If FiltroAbilitato Then
            Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
          Else
            'se il filtro non e'abilitato vengono presi tutti i dati del range twa
            Valido = RigaValida(i, eTipoFiltro.eNone, CurrentPlotSettings.FilterChannel, eTipoFiltro.eNone, CurrentPlotSettings.Filter2Channel, ChTwa)
          End If
          If Valido Then
            If NotSelectedColor = Nothing Then
              DaEvidenziare = True
            Else
              Select Case CurrentPlotSettings.SorgenteDati
                Case clsXYPlotSettings.eDataSource.eCurrVisRngVsFilter
                  DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa) 'se la riga coincide con il filtro
                Case clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods
                  'se la riga coincide con un periodo selezionato
                  Dim p = GetPeriodByRowId(i)
                  'Dim p = PeriodiValidi.Where(Function(x) x.TR.IdRigaIniziale <= i AndAlso x.TR.IdRigaFinale >= i).FirstOrDefault
                  If p Is Nothing Then
                    DaEvidenziare = False
                  Else
                    DaEvidenziare = Periodo.IsChecked 'se la riga coincide con un periodo selezionato
                  End If
                Case Else
                  DaEvidenziare = True 'evidenzia se selezionato
              End Select
            End If
            If DaEvidenziare Then DaEvidenziareSamples += 1
            Periodo.RigheValide.Add(i, DaEvidenziare)
            If Not ChannelValuesD.ContainsKey(99) Then
              ChannelValuesD.Add(99, 0)
            End If
            ChannelValuesD(99) += 1
            ValidSamples += 1
          End If
        End If
      Next

      'questo loop serve per aggiungere (evidenziate) le righe che appartengono a periodi selezionati esterni al time range visibile
      If CurrentPlotSettings.SorgenteDati = clsXYPlotSettings.eDataSource.eCurrVisRngFiltVsSelPeriods Then
        For Each p In PeriodiValidi
          For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
            Dim v As Double = ChTwa.Valori(i)
            If Not Double.IsNaN(v) Then
              Dim idRiga As Integer = i
              If Not Periodo.RigheValide.ContainsKey(idRiga) Then
                Dim DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                If DaEvidenziare Then
                  DaEvidenziareSamples += 1 'viene aggiunta solo se da evidenziare
                  Periodo.RigheValide.Add(i, True)
                  If Not ChannelValuesD.ContainsKey(99) Then
                    ChannelValuesD.Add(99, 0)
                  End If
                  ChannelValuesD(99) += 1
                  ValidSamples += 1
                End If
              End If
            End If
          Next
        Next
      End If

      SetChannelValues()
    End Sub


    Function GetNameByValueAllTogether(value As Integer) As String
      Return "Filtered Samples"
    End Function

    Function GetAllTogetherIndex(value As Double) As Integer
      Return ChannelValues.IndexOf(99)
    End Function



    Function TwaValido(ValoreTwa As Double) As Boolean
      Try
        Dim TwaLimits As Integer() = {0, 60, 120, 180}

        Dim AbsTwa As Double = System.Math.Abs(ValoreTwa)
        If Double.IsNaN(ValoreTwa) Then
          Return False
        Else
          If AbsTwa <= TwaLimits(0) AndAlso AbsTwa >= TwaLimits(3) Then
            'troppo stretti e troppo larghi
            Return False
          ElseIf AbsTwa > TwaLimits(0) AndAlso AbsTwa < TwaLimits(1) Then
            'UpVmg
            If CurrentPlotSettings.ShowUpwindVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          ElseIf AbsTwa > TwaLimits(2) AndAlso AbsTwa < TwaLimits(3) Then
            'DnVmg
            If CurrentPlotSettings.ShowDownwindVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          Else
            'Not Vmg
            If CurrentPlotSettings.ShowNotVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          End If
        End If
      Catch ex As Exception
        MsgBox("twavalido")
        Stop
        Return True
      End Try
    End Function


    Function RigaValida(Indice As Integer, Filtro As eTipoFiltro, FilterChannel As clsChannel2020, Filtro2 As eTipoFiltro, Filte2Channel As clsChannel2020, CanaleTwa As clsChannel2020) As Boolean

      Dim v = CurrentPlotSettings.YAxisChannel.Valori(Indice)
      If v = 0 AndAlso CurrentPlotSettings.YaxisZeroIsNan Then
        Return False
      End If

      Dim Valido As Boolean = True
      If Not CanaleTwa Is Nothing Then
        Valido = TwaValido(CanaleTwa.Valori(Indice))
      End If
      If Valido Then
        If Not (CurrentPlotSettings.FilterChannel Is Nothing OrElse Filtro = eTipoFiltro.eNone) Then
          Valido = DatoValido(Filtro, CurrentPlotSettings.FilterChannel.Valori(Indice), CurrentPlotSettings.ApplyFilterAbsVal)
        End If
        If Valido AndAlso Not (CurrentPlotSettings.Filter2Channel Is Nothing OrElse Filtro2 = eTipoFiltro.eNone) Then
          Valido = DatoValido2(Filtro2, CurrentPlotSettings.Filter2Channel.Valori(Indice), CurrentPlotSettings.ApplyFilter2AbsVal)
        End If
      End If
      If Valido AndAlso CurrentPlotSettings.ApplyFilterSailingState Then
        Valido = DatoValidoSailingState(CurrentPlotSettings.SailingStateChannel.Valori(Indice))
      End If
      Return Valido
    End Function

    Function DatoValido(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          If AbsVal Then Valore = System.Math.Abs(Valore)
          Select Case TipoFiltro
            Case eTipoFiltro.eBetween
              Return Valore >= CurrentPlotSettings.FilterValueMin And Valore <= CurrentPlotSettings.FilterValueMax
            Case eTipoFiltro.eMax
              Return Valore <= CurrentPlotSettings.FilterValueMax
            Case eTipoFiltro.eMin
              Return Valore >= CurrentPlotSettings.FilterValueMin
            Case eTipoFiltro.eNone
              Return True
          End Select
        End If
        Return True
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function

    Function DatoValido2(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          If AbsVal Then Valore = System.Math.Abs(Valore)
          Select Case TipoFiltro
            Case eTipoFiltro.eBetween
              Return Valore >= CurrentPlotSettings.Filter2ValueMin And Valore <= CurrentPlotSettings.Filter2ValueMax
            Case eTipoFiltro.eMax
              Return Valore <= CurrentPlotSettings.Filter2ValueMax
            Case eTipoFiltro.eMin
              Return Valore >= CurrentPlotSettings.Filter2ValueMin
            Case eTipoFiltro.eNone
              Return True
          End Select
        End If
        Return True
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function

    Function DatoValidoSailingState(Valore As Double) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          Return Valore = CurrentPlotSettings.SailingState
        End If
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function


  End Class

  Private Class PeriodsChartAdvanced
    Public CurrentPlotSettings As clsXYPlotSettings
    Public ChTwa As clsChannel2020 'DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Public ChannelValuesD As New Dictionary(Of Double, Integer) ' contiene i valori univoci del canale
    Public ValidSamples As Integer = 0 ' contiene i valori univoci del canale
    Public DaEvidenziareSamples As Integer = 0 ' contiene i valori univoci del canale
    Public NotSelectedColor As System.Windows.Media.Color
    Public HighlightFilter As Boolean
    Public Filtro As clsSciChartXyPlotViewModel.eTipoFiltro
    Public Filtro2 As clsSciChartXyPlotViewModel.eTipoFiltro
    Public PeriodiValidi As List(Of clsPeriod2021)
    Public ChannelValues As List(Of Double)

    Public Sub New(CPS As clsXYPlotSettings, NSC As System.Windows.Media.Color, HF As Boolean,
                   F As clsSciChartXyPlotViewModel.eTipoFiltro, F2 As clsSciChartXyPlotViewModel.eTipoFiltro, PV As List(Of clsPeriod2021))
      CurrentPlotSettings = CPS
      NotSelectedColor = NSC
      HighlightFilter = HF
      Filtro = F
      Filtro2 = F2
      PeriodiValidi = PV
      ChTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

    End Sub

    Dim DayString As String = "yyyy MMM dd"
    Dim MonthString As String = "yyyy MMM"
    Dim YearString As String = "yyyy"


    Public Sub SetByTypeId()
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          SetByAllTogether()
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          SetByPortStbd()
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          SetByUpDn()
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          SetByUpDnPortStbd()
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          SetByAllTogether()
        Case clsXYPlotSettings.eGroupingType.eValueBins
          SetByColorChannelValues()
        Case clsXYPlotSettings.eGroupingType.eColorBins
          SetByColorChannelBins()
        Case clsXYPlotSettings.eGroupingType.eDayBins
          SetByTimeBins(DayString)
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          SetByTimeBins(MonthString)
        Case clsXYPlotSettings.eGroupingType.eYearBins
          SetByTimeBins(YearString)
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          SetByKeyBins()
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          SetByRaceNameBins()
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          SetByRaceLegNameBins()
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          SetBySinglePeriod()
        Case Else
          Exit Sub
      End Select


    End Sub

    Public Function GetIndex(value As Double) As Integer
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          Return 0 ' c'é solo un gruppo
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return GetPortStbdIndex(value)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return GetUpDnIndex(value)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return GetUpDnPortStbdIndex(value)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return GetAllTogetherIndex(value)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          Return GetChannelValuesIndex(value)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return GetIntervalIndex(value)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(DayString))
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(MonthString))
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Dim IdRiga As Integer = value
          Dim p = DataProvider2020.Momento(IdRiga)
          If p = Nothing Then Return -1
          Return GetKeyIndex(p.ToString(YearString))
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Dim p = GetIndexFast2(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.Keys)
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Dim p = GetIndexFast2(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.RaceName)
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Dim p = GetIndexFast2(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.RaceLegName)
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Dim p = GetIndexFast2(value)
          If p Is Nothing Then Return -1
          Return GetKeyIndex(p.StringaPeriodoXYplot)
        Case Else
          Return -1
      End Select
    End Function

    Private Function GetIndexFast2(value As Double) As clsPeriod2021
      Dim lo As Integer = 0
      Dim hi As Integer = PeriodiValidi.Count - 1

      While lo <= hi
        Dim mid As Integer = (lo + hi) \ 2
        Dim tr = PeriodiValidi(mid).TR

        If value < tr.IdRigaIniziale Then
          hi = mid - 1
        ElseIf value > tr.IdRigaFinale Then
          lo = mid + 1
        Else
          Return PeriodiValidi(mid) ' trovato
        End If
      End While

      Return Nothing
    End Function

    Public Function GetName(Id As Double) As String
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          Return "360 check"
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return GetNameByValuePortStbd(Id)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return GetNameByValueUpDn(Id)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return GetNameByValueUpDnPortStbd(Id)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return GetNameByValueAllTogether(Id)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          Return GetNameByChannelValue(Id)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return GetNameByInterval(Id)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Return GetNameByKey(Id)
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Return GetNameByKey(Id)
        Case Else
          Return "ERRORE"
      End Select

    End Function

    Public Function GetValue(IdRiga As Integer) As Double
      Select Case CurrentPlotSettings.GroupingType
        Case clsXYPlotSettings.eGroupingType.e360checks
          'Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(i), CurrentPlotSettings.YAxisChannel.Valori(i))
          'y = delta
          Return DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentPlotSettings.XAxisChannel.Valori(IdRiga), CurrentPlotSettings.YAxisChannel.Valori(IdRiga))
        Case clsXYPlotSettings.eGroupingType.eTackOnly
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eUpDnOnly
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eTackAndUpDown
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eAllTogether
          Return ChTwa.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eValueBins
          Return CurrentPlotSettings.ColorChannel.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eColorBins
          Return CurrentPlotSettings.ColorChannel.Valori(IdRiga)
        Case clsXYPlotSettings.eGroupingType.eDayBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eMonthBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eYearBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eKeyBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eRaceBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eRaceLegBins
          Return IdRiga 'restituisce l'id della riga
        Case clsXYPlotSettings.eGroupingType.eSinglePeriod
          Return IdRiga
        Case Else
          Return -1
      End Select

    End Function

    Dim minValue As Double = Double.NaN
    Dim maxValue As Double = Double.NaN
    Dim Chiavi As New List(Of String)

    Sub SetByKeyBins() 'valori in gruppi

      Dim ChiaviTmp As New List(Of String)
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        If Not ChiaviTmp.Contains(periodo.Keys) Then
          ChiaviTmp.Add(periodo.Keys)
        End If
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim IdChiave As Integer = ChiaviTmp.IndexOf(periodo.Keys)
              If Not ChannelValuesD.ContainsKey(IdChiave) Then
                ChannelValuesD.Add(IdChiave, 0)
              End If
              ChannelValuesD(IdChiave) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByRaceNameBins() 'valori in gruppi
      Dim ChiaviTmp As New List(Of String)
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        If Not ChiaviTmp.Contains(periodo.RaceName) Then
          ChiaviTmp.Add(periodo.RaceName)
        End If
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim IdChiave As Integer = ChiaviTmp.IndexOf(periodo.RaceName)
              If Not ChannelValuesD.ContainsKey(IdChiave) Then
                ChannelValuesD.Add(IdChiave, 0)
              End If
              ChannelValuesD(IdChiave) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByRaceLegNameBins() 'valori in gruppi
      Dim ChiaviTmp As New List(Of String)
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        If Not ChiaviTmp.Contains(periodo.RaceLegName) Then
          ChiaviTmp.Add(periodo.RaceLegName)
        End If
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim IdChiave As Integer = ChiaviTmp.IndexOf(periodo.RaceLegName)
              If Not ChannelValuesD.ContainsKey(IdChiave) Then
                ChannelValuesD.Add(IdChiave, 0)
              End If
              ChannelValuesD(IdChiave) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Dim PercentileBottom As Double = Double.NaN
    Dim PercentileTop As Double = Double.NaN
    Function PercentileCheck(IdRiga As Integer) As Boolean
      If CurrentPlotSettings.ApplyPercentileFilter Then
        If Double.IsNaN(PercentileBottom) Then
          Dim ValoriNotNan = CurrentPlotSettings.YAxisChannel.Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
          PercentileBottom = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, CurrentPlotSettings.PercentileFilter)
          PercentileTop = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 100 - CurrentPlotSettings.PercentileFilter)
        End If
        Dim v As Double = CurrentPlotSettings.YAxisChannel.Valori(IdRiga)
        If Double.IsNaN(v) Then
          Return False
        Else
          If v < PercentileBottom Or v > PercentileTop Then
            Return False
          End If
        End If
      End If
      Return True
    End Function


    Sub SetChannelValues()
      Dim b = ChannelValuesD.OrderBy(Function(x) x.Key).ToList
      If CurrentPlotSettings.ApplyGroupFilterPerc Then
        Dim min = ChannelValuesD.Select(Function(x) x.Value).Sum() * (CurrentPlotSettings.GroupFilterPerc / 100)
        b = b.Where(Function(x) x.Value > min).ToList
      End If
      ChannelValues = b.Select(Function(x) x.Key).ToList ' contiene i valori univoci del canale
    End Sub


    Function GetKeyIndex(Chiave As String) As Integer
      Dim idx = Chiavi.IndexOf(Chiave)
      Return ChannelValues.IndexOf(idx)
    End Function

    Function GetNameByKey(KeyId As Integer) As String
      Return Chiavi(KeyId)
    End Function



    Sub SetByColorChannelBins() 'valori in gruppi
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
            End If
          End If
        Next
      Next

      For Each periodo In PeriodiValidi
        For Each riga In periodo.RigheValide
          Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(riga.Key)
          If Double.IsNaN(minValue) Then
            minValue = v
            maxValue = v
          Else
            If v < minValue Then minValue = v
            If v > maxValue Then maxValue = v
          End If
        Next
      Next
      For Each periodo In PeriodiValidi
        For Each riga In periodo.RigheValide
          Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(riga.Key)
          Dim binnedValue As Double = GetIntervalIndex(v)
          If Not ChannelValuesD.ContainsKey(binnedValue) Then
            ChannelValuesD.Add(binnedValue, 0)
          End If
          ChannelValuesD(binnedValue) += 1
          ValidSamples += 1
        Next
      Next

      SetChannelValues()
    End Sub

    Function GetIntervalIndex(value As Double) As Integer
      Dim numIntervals As Integer = CurrentPlotSettings.ColorChannelIntervals
      If numIntervals <= 0 Then
        Throw New ArgumentException("Il numero di intervalli deve essere maggiore di zero.")
      End If
      If value <= minValue Then
        Return 0
      End If
      If value >= maxValue Then
        Return numIntervals - 1
      End If
      Dim intervalSize As Double = (maxValue - minValue) / numIntervals
      Dim index As Integer = CInt(Math.Floor((value - minValue) / intervalSize))
      ' Protezione contro errori di arrotondamento
      If index >= numIntervals Then
        index = numIntervals - 1
      End If
      Return index
    End Function

    Function GetNameByInterval(intervallo As Integer) As String
      Dim numIntervals As Integer = CurrentPlotSettings.ColorChannelIntervals
      If numIntervals <= 0 Then
        Throw New ArgumentException("Il numero di intervalli deve essere maggiore di zero.")
      End If
      Dim intervalSize As Double = (maxValue - minValue) / numIntervals
      Dim lowerBound As Double = minValue + (intervallo * intervalSize)
      Dim upperBound As Double = lowerBound + intervalSize
      'Return String.Format("[{0:F2} - {1:F2}]", lowerBound, upperBound)
      Return CurrentPlotSettings.ColorChannel.ShortName & " " & FormatInterval(lowerBound, upperBound, CurrentPlotSettings.ColorChannel.Decimals) & " " & CurrentPlotSettings.ColorChannel.ShortUM
    End Function

    Function FormatInterval(lowerBound As Double, upperBound As Double, Optional decimals As Integer = 2) As String
      Dim formatString As String = "F" & decimals.ToString()
      Return String.Format("{0:" & formatString & "}-{1:" & formatString & "}", lowerBound, upperBound)
    End Function

    Sub SetByColorChannelValues() 'valori univoci del canale
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = CurrentPlotSettings.ColorChannel.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              If Not ChannelValuesD.ContainsKey(v) Then
                ChannelValuesD.Add(v, 0)
              End If
              ChannelValuesD(v) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      SetChannelValues()
    End Sub

    Function GetChannelValuesIndex(value As Double) As Integer
      Return ChannelValues.IndexOf(value)
    End Function

    Function GetNameByChannelValue(value As Double) As String
      Dim id = GetChannelValuesIndex(value)
      Return ChannelValues(id).ToString() '("F" & CurrentPlotSettings.ColorChannel.Decimals.ToString) & " " & CurrentPlotSettings.ColorChannel.LongUM
    End Function


    Sub SetBySinglePeriod()
      Dim ChiaviTmp As New List(Of String)
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        Dim str As String = periodo.StringaPeriodoXYplot
        If Not ChiaviTmp.Contains(str) Then
          ChiaviTmp.Add(str)
        End If
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
              If Not ChannelValuesD.ContainsKey(IdChiave) Then
                ChannelValuesD.Add(IdChiave, 0)
              End If
              ChannelValuesD(IdChiave) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByTimeBins(TimeString As String)
      Dim ChiaviTmp As New List(Of String)
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As DateTime = DataProvider2020.Momento(i)
          If Not v = Nothing Then
            Dim str As String = v.ToString(TimeString)
            If Not ChiaviTmp.Contains(str) Then
              ChiaviTmp.Add(str)
            End If
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim IdChiave As Integer = ChiaviTmp.IndexOf(str)
              If Not ChannelValuesD.ContainsKey(IdChiave) Then
                ChannelValuesD.Add(IdChiave, 0)
              End If
              ChannelValuesD(IdChiave) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next
      Chiavi.Clear()
      For Each chiave In ChiaviTmp.ToList
        If chiave.Trim = "" Then chiave = "not assigned"
        Chiavi.Add(chiave)
      Next
      SetChannelValues()
    End Sub

    Sub SetByPortStbd()
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim vv As Integer = IIf(v >= 0, 1, -1)
              If Not ChannelValuesD.ContainsKey(vv) Then
                ChannelValuesD.Add(vv, 0)
              End If
              ChannelValuesD(vv) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next

      SetChannelValues()
    End Sub

    Function GetNameByValuePortStbd(value As Integer) As String
      Select Case value
        Case -1
          Return "Port"
        Case Else
          Return "Stbd"
      End Select
    End Function

    Function GetPortStbdIndex(value As Double) As Integer
      Dim vv As Integer = IIf(value >= 0, 1, -1)
      Return vv
    End Function

    Sub SetByUpDn()
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim vv As Integer = IIf(Math.Abs(v) >= 90, 1, 0)
              If Not ChannelValuesD.ContainsKey(vv) Then
                ChannelValuesD.Add(vv, 0)
              End If
              ChannelValuesD(vv) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next

      SetChannelValues()
    End Sub


    Function GetNameByValueUpDn(value As Integer) As String
      Select Case value
        Case 0
          Return "Upwind"
        Case Else
          Return "Downwind"
      End Select
    End Function

    Function GetUpDnIndex(value As Double) As Integer
      Dim vv As Integer = IIf(Math.Abs(value) >= 90, 1, 0)
      Return vv
    End Function

    Sub SetByUpDnPortStbd()
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              Dim vv As Integer = 0
              If Math.Abs(v) < 90 Then 'bolina
                vv = IIf(v >= 0, 1, -1)
              Else
                vv = IIf(v >= 0, 2, -2)
              End If
              If Not ChannelValuesD.ContainsKey(vv) Then
                ChannelValuesD.Add(vv, 0)
              End If
              ChannelValuesD(vv) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next

      SetChannelValues()
    End Sub

    Function GetNameByValueUpDnPortStbd(value As Integer) As String
      Select Case value
        Case -2
          Return "Port Downwind"
        Case -1
          Return "Port Upwind"
        Case 1
          Return "Stbd Upwind"
        Case 2
          Return "Stbd Downwind"
        Case Else
          Return "Not Selected"
      End Select
    End Function

    Function GetUpDnPortStbdIndex(value As Double) As Integer
      Dim vv As Integer = 0
      If Math.Abs(value) < 90 Then 'bolina
        vv = IIf(value >= 0, 1, -1)
      Else
        vv = IIf(value >= 0, 2, -2)
      End If
      Return vv
      'Return ChannelValues.IndexOf(vv)
    End Function

    Public Sub SetByAllTogether()
      For Each periodo In PeriodiValidi
        periodo.RigheValide.Clear()
        For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
          Dim v As Double = ChTwa.Valori(i)
          If Not Double.IsNaN(v) Then
            Dim Valido As Boolean = False
            Dim DaEvidenziare As Boolean = False
            If HighlightFilter Then 'Visualizza Selected, evidenzia per filtro 
              Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
              DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, Nothing) 'filtra solo per filtro non per angolo
            Else
              If NotSelectedColor = Nothing Then 'Visualizza ed evidenzia per andatura e filtro 
                DaEvidenziare = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                Valido = DaEvidenziare 'se visualizza evidenzia
              Else ' 
                'Valido = TwaValido(ChTwa.Valori(i)) 'filtra per mura e andatura
                Valido = RigaValida(i, Filtro, CurrentPlotSettings.FilterChannel, Filtro2, CurrentPlotSettings.Filter2Channel, ChTwa)
                DaEvidenziare = periodo.IsChecked 'evidenzia se selezionato
              End If
            End If
            If Valido AndAlso PercentileCheck(i) Then
              If DaEvidenziare Then DaEvidenziareSamples += 1
              periodo.RigheValide.Add(i, DaEvidenziare)
              If Not ChannelValuesD.ContainsKey(99) Then
                ChannelValuesD.Add(99, 0)
              End If
              ChannelValuesD(99) += 1
              ValidSamples += 1
            End If
          End If
        Next
      Next

      SetChannelValues()
    End Sub


    Function GetNameByValueAllTogether(value As Integer) As String
      Return "All Periods Together"
    End Function

    Function GetAllTogetherIndex(value As Double) As Integer
      Return ChannelValues.IndexOf(99)
    End Function



    Function TwaValido(ValoreTwa As Double) As Boolean
      Try
        Dim TwaLimits As Integer() = {0, 60, 120, 180}

        Dim AbsTwa As Double = System.Math.Abs(ValoreTwa)
        If Double.IsNaN(ValoreTwa) Then
          Return False
        Else
          If AbsTwa <= TwaLimits(0) AndAlso AbsTwa >= TwaLimits(3) Then
            'troppo stretti e troppo larghi
            Return False
          ElseIf AbsTwa > TwaLimits(0) AndAlso AbsTwa < TwaLimits(1) Then
            'UpVmg
            If CurrentPlotSettings.ShowUpwindVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          ElseIf AbsTwa > TwaLimits(2) AndAlso AbsTwa < TwaLimits(3) Then
            'DnVmg
            If CurrentPlotSettings.ShowDownwindVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          Else
            'Not Vmg
            If CurrentPlotSettings.ShowNotVmg Then
              If ValoreTwa > 0 Then
                Return CurrentPlotSettings.ShowStbd
              Else
                Return CurrentPlotSettings.ShowPort
              End If
            Else
              Return False
            End If
          End If
        End If
      Catch ex As Exception
        MsgBox("twavalido")
        Stop
        Return True
      End Try
    End Function


    Function RigaValida(Indice As Integer, Filtro As eTipoFiltro, FilterChannel As clsChannel2020, Filtro2 As eTipoFiltro, Filte2Channel As clsChannel2020, CanaleTwa As clsChannel2020) As Boolean

      Dim v = CurrentPlotSettings.YAxisChannel.Valori(Indice)
      If v = 0 AndAlso CurrentPlotSettings.YaxisZeroIsNan Then
        Return False
      End If

      Dim Valido As Boolean = True
      If Not CanaleTwa Is Nothing Then
        Valido = TwaValido(CanaleTwa.Valori(Indice))
      End If
      If Valido Then
        If Not (CurrentPlotSettings.FilterChannel Is Nothing OrElse Filtro = eTipoFiltro.eNone) Then
          Valido = DatoValido(Filtro, CurrentPlotSettings.FilterChannel.Valori(Indice), CurrentPlotSettings.ApplyFilterAbsVal)
        End If
        If Valido AndAlso Not (CurrentPlotSettings.Filter2Channel Is Nothing OrElse Filtro2 = eTipoFiltro.eNone) Then
          Valido = DatoValido2(Filtro2, CurrentPlotSettings.Filter2Channel.Valori(Indice), CurrentPlotSettings.ApplyFilter2AbsVal)
        End If
      End If
      If Valido AndAlso CurrentPlotSettings.ApplyFilterSailingState Then
        Valido = DatoValidoSailingState(CurrentPlotSettings.SailingStateChannel.Valori(Indice))
      End If
      Return Valido
    End Function

    Function DatoValido(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          If AbsVal Then Valore = System.Math.Abs(Valore)
          Select Case TipoFiltro
            Case eTipoFiltro.eBetween
              Return Valore >= CurrentPlotSettings.FilterValueMin And Valore <= CurrentPlotSettings.FilterValueMax
            Case eTipoFiltro.eMax
              Return Valore <= CurrentPlotSettings.FilterValueMax
            Case eTipoFiltro.eMin
              Return Valore >= CurrentPlotSettings.FilterValueMin
            Case eTipoFiltro.eNone
              Return True
          End Select
        End If
        Return True
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function

    Function DatoValido2(TipoFiltro As eTipoFiltro, Valore As Double, AbsVal As Boolean) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          If AbsVal Then Valore = System.Math.Abs(Valore)
          Select Case TipoFiltro
            Case eTipoFiltro.eBetween
              Return Valore >= CurrentPlotSettings.Filter2ValueMin And Valore <= CurrentPlotSettings.Filter2ValueMax
            Case eTipoFiltro.eMax
              Return Valore <= CurrentPlotSettings.Filter2ValueMax
            Case eTipoFiltro.eMin
              Return Valore >= CurrentPlotSettings.Filter2ValueMin
            Case eTipoFiltro.eNone
              Return True
          End Select
        End If
        Return True
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function

    Function DatoValidoSailingState(Valore As Double) As Boolean
      Try
        If Double.IsNaN(Valore) Then
          Return False
        Else
          Return Valore = CurrentPlotSettings.SailingState
        End If
      Catch ex As Exception
        MsgBox("DatoValido")
        Return True
      End Try
    End Function


  End Class


End Class



Public Class clsTLdata
  Public LineName As String
  Public LineColor As Color
  Public LineValues As XyDataSeries(Of Double, Double)

  Public Sub New(LineName As String, LineColor As Color, LineValues As XyDataSeries(Of Double, Double))
    Me.LineName = LineName
    Me.LineColor = LineColor
    Me.LineValues = LineValues

  End Sub

End Class



<AddINotifyPropertyChangedInterface>
Public Class clsValoriCursore
  Public IsX360 As Boolean
  Public MaxX As Double
  Public MinX As Double
  Public AvgX As Double
  Public SdX As Double

  Public IsY360 As Boolean
  Public MaxY As Double
  Public MinY As Double
  Public AvgY As Double
  Public SdY As Double

  Public Sub AggiornaStats(ArrayX As Double(), ArrayY As Double(), IsX360 As Boolean, IsY360 As Boolean)
    Me.IsX360 = IsX360
    Me.IsY360 = IsY360

    If ArrayX.Count > 0 Then
      MaxX = ArrayX.Max
      MinX = ArrayX.Min
      AvgX = ArrayX.Average
      alglib.basestat.sampleadev(ArrayX, ArrayX.Count, SdX)
    Else
      MaxX = 0
      MinX = 0
      AvgX = 0
      SdX = 0
    End If

    If ArrayY.Count > 0 Then
      MaxY = ArrayY.Max
      MinY = ArrayY.Min
      AvgY = ArrayY.Average
      alglib.basestat.sampleadev(ArrayY, ArrayY.Count, SdY)
    Else
      MaxY = 0
      MinY = 0
      AvgY = 0
      SdY = 0
    End If

  End Sub

End Class

Public Class clsDataSubSet
  Dim _Titolo As String
  Dim _SottoTitolo As String
  Dim _ListaMomenti As New List(Of DateTime)
  Dim _ListaValori As New List(Of List(Of Double))
  Dim _Headers As List(Of String)


  Public Sub New(ValChannels As List(Of String), Titolo As String, SottoTitolo As String)
    _Headers = ValChannels
    _SottoTitolo = SottoTitolo
    _Titolo = Titolo
    For Each Stringa In ValChannels
      _ListaValori.Add(New List(Of Double))
    Next
  End Sub

  Public Property ListaMomenti As List(Of Date)
    Get
      Return _ListaMomenti
    End Get
    Set(value As List(Of Date))
      _ListaMomenti = value
    End Set
  End Property

  Public Property ListaValori As List(Of List(Of Double))
    Get
      Return _ListaValori
    End Get
    Set(value As List(Of List(Of Double)))
      _ListaValori = value
    End Set
  End Property

  Public Function CopyTableToClipboard(Msg As Boolean) As String
    Dim righe As New List(Of String)
    righe.Add(_Titolo)
    righe.Add(_SottoTitolo)
    righe.Add("")
    Dim riga As String = "DateTime"
    For Each H In _Headers
      riga &= vbTab & H
    Next
    righe.Add(riga)

    For i As Integer = 0 To _ListaMomenti.Count - 1
      riga = _ListaMomenti(i).ToString("yyyyMMdd HH:mm:ss.fff")
      For Each V In _ListaValori
        If V.Count > 0 Then
          riga &= vbTab & V(i)
        Else
          riga &= vbTab & 0
        End If
      Next
      righe.Add(riga)
    Next
    Dim t As String = String.Join(vbCrLf, righe)
    Clipboard.SetText(t)
    If Msg Then MsgBox("Full Data Table Copied to Clipboard")
    Return t
  End Function

  Public Function CopyStatisticsToClipboard(Msg As Boolean) As String
    Dim righe As New List(Of String)
    righe.Add(_Titolo)
    righe.Add(_SottoTitolo)
    righe.Add("")


    For i As Integer = 0 To _Headers.Count - 1
      Dim riga As String = _Headers(i) & vbCrLf
      Dim Valori = _ListaValori(i).Where(Function(x) Not Double.IsNaN(x)).ToArray
      If Valori.Count > 0 Then
        riga &= "Avg:" & vbTab & CDbl(Valori.Average).ToString("F2") & vbCrLf
        riga &= "Max:" & vbTab & CDbl(Valori.Max).ToString("F2") & vbCrLf
        riga &= "Min:" & vbTab & CDbl(Valori.Min).ToString("F2") & vbCrLf
        Dim StandardDeviation As Double = 0
        alglib.basestat.sampleadev(Valori, Valori.Count, StandardDeviation)
        riga &= "SD:" & vbTab & StandardDeviation.ToString("F2") & vbCrLf
        righe.Add(riga)
        riga = "Distribution:" & vbCrLf
        riga &= "Range Avg" & vbTab & "Samples" & vbTab & "Percentage"
        righe.Add(riga)
        Dim Intervalli As Integer = 20
        Dim H As Double = (Valori.Max - Valori.Min) / Intervalli
        If Not H = 0 Then
          If Not Valori Is Nothing AndAlso Valori.Count > 0 Then
            Dim Ysum As Double = 0
            Dim coppie As New List(Of clsDoubleXY)
            For ii As Integer = 0 To Intervalli - 1
              Dim x As Double = Valori.Min + (H * ii)
              Dim y = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, Valori)
              Ysum += y
              coppie.Add(New clsDoubleXY(x, y))
            Next
            For Each coppia In coppie
              riga = coppia.X.ToString("F2") & vbTab & (coppia.Y / Ysum * Valori.Count).ToString("F0") & vbTab & (coppia.Y / Ysum * 100).ToString("F1") & vbTab
              righe.Add(riga.TrimEnd(vbTab))
            Next
            righe.Add(vbTab)
            righe.Add(vbTab)
          End If
        End If
      Else
        riga &= "Avg:" & vbTab & "-" & vbCrLf
        riga &= "Max:" & vbTab & "-" & vbCrLf
        riga &= "Min:" & vbTab & "-" & vbCrLf
        Dim StandardDeviation As Double = 0
        alglib.basestat.sampleadev(Valori, Valori.Count, StandardDeviation)
        riga &= "SD:" & vbTab & "-" & vbCrLf
        righe.Add(riga)
        riga = "Distribution:" & vbCrLf
        riga &= "Range Avg" & vbTab & "Samples" & vbTab & "Percentage"
        righe.Add(riga)
        Dim Intervalli As Integer = 20
        Dim H As Double = 1 / Intervalli
        If Not Valori Is Nothing AndAlso Valori.Count > 0 Then
          Dim Ysum As Double = 0
          Dim coppie As New List(Of clsDoubleXY)
          For ii As Integer = 0 To Intervalli - 1
            Dim x As Double = Valori.Min + (H * ii)
            Dim y = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, Valori)
            Ysum += y
            coppie.Add(New clsDoubleXY(x, y))
          Next
          For Each coppia In coppie
            riga = coppia.X.ToString("F2") & vbTab & (coppia.Y / Ysum * Valori.Count).ToString("F0") & vbTab & (coppia.Y / Ysum * 100).ToString("F1") & vbTab
            righe.Add(riga.TrimEnd(vbTab))
          Next
          righe.Add(vbTab)
          righe.Add(vbTab)
        End If
      End If
      'Select Case CanaleCorrente.Nome
      '  Case "Twa", "Awa"
      '    Valori = Valori.Select(Function(x) System.Math.Abs(x.Value))
      'End Select
    Next

    Dim t As String = String.Join(vbCrLf, righe)
    Clipboard.SetText(t)
    If Msg Then MsgBox("Statistic Table Copied to Clipboard")
    Return t
  End Function

End Class

Public Class clsPrdXYZC
  Public Property X As New List(Of Double)
  Public Property Y As New List(Of Double)
  Public Property Z As New List(Of Double)
  Public Property Colore As Color

  Public Sub New(Colore As Color)
    Me.Colore = Colore
  End Sub

  Public Sub Append(X As Double, Y As Double)
    Me.X.Add(X)
    Me.Y.Add(Y)
  End Sub

  Public Sub Append(X As Double, Y As Double, Z As Double)
    Me.X.Add(X)
    Me.Y.Add(Y)
    Me.Z.Add(Z)
  End Sub

End Class

'Public Class clsGroupValues
'  Public NotNanValues As clsDoubleXY()
'  Public Valori(9) As clsGroupShape

'  Public Sub Aggiorna()
'    Dim MinX As Double = NotNanValues.Select(Function(x) x.X).Min
'    Dim MaxX As Double = NotNanValues.Select(Function(x) x.X).Max


'  End Sub


'End Class

'Public Class clsGroupShape
'  Dim X_Val As Double
'  Dim Y_Max As Double
'  Dim Y_Min As Double
'  Dim Y_Avg As Double

'  Public Sub New(X_Val As Double, Y_Max As Double, Y_Min As Double, Y_Avg As Double)
'    Me.X_Val = X_Val
'    Me.Y_Max = Y_Max
'    Me.Y_Min = Y_Min
'    Me.Y_Avg = Y_Avg
'  End Sub

'End Class