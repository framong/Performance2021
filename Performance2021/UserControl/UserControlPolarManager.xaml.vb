Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged
Imports SciChart
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Axes
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.PaletteProviders
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model

Public Class UserControlPolarManager
  'Dim VM As clsUserControlPolarManagerViwModel
  Dim VM As clsUserControlPolarManagerViwModel

  Public Sub New()

    'Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    ' --- Nulla di ciò che segue deve girare nel designer XAML ---
    If IsInDesignMode Then Exit Sub

    VM = New clsUserControlPolarManagerViwModel

    Me.DataContext = VM

    ' Add any initialization after the InitializeComponent() call.
    'If VM.objSCpolar Is Nothing Then VM.objSCpolar = New clsSciChart(SciChartPolars)
    VM.VisualizzaPolare("bs")

  End Sub

  Private Sub PolarExport_Click(sender As Object, e As RoutedEventArgs)
    VM.EsportaPolareToFaRo()
  End Sub

  Private Sub PolarSelect_Click(sender As Object, e As RoutedEventArgs)
    VM.SelezionaFilePolare()

  End Sub

  Private Sub Btn_PolarTest_Click(sender As Object, e As RoutedEventArgs)
    VM.ImportaDaFileXls()
  End Sub

  Private Sub ItemChkBx_Checked(sender As Object, e As RoutedEventArgs)
    VM.AggiornaPeriodiVisibili()
  End Sub

  Private Sub ItemChkBx_Unchecked(sender As Object, e As RoutedEventArgs)
    VM.AggiornaPeriodiVisibili()
  End Sub

  Private Sub Period_MouseMove(sender As Object, e As MouseEventArgs)

  End Sub

  Private Sub Period_MouseLeave(sender As Object, e As MouseEventArgs)

  End Sub

  Private Sub TextBlock_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)

  End Sub

  Private Sub cmb_Polari_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    Try
      If Not sender.selecteditem Is Nothing Then
        VM.VisualizzaPolare(sender.selecteditem)
        SciChartPolars.ZoomExtents()
      End If
    Catch ex As Exception

    End Try

  End Sub

  Private Sub btn_NewPolarTest_Click(sender As Object, e As RoutedEventArgs)
    Dim oo As New clsTgtManager
    'oo.CreaFileTargetTest()
    oo.CaricaLastJsonTgtFile()
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    If False Then
      Dim righe As New List(Of String)
      Dim twss As New List(Of Double)
      twss.Add(6.8)
      twss.Add(7.9)
      twss.Add(9.1)
      twss.Add(10.2)
      twss.Add(11.3)
      twss.Add(12.5)
      twss.Add(13.6)
      twss.Add(14.7)
      twss.Add(15.9)
      twss.Add(17.0)
      righe.Add("Up")
      For Each Tws As Double In twss
        Dim t = TgtManager.Tgt.ValoreTgt(True, Tws, "bs")
        righe.Add(Tws.ToString("F1") & vbTab & t.Vmg.ToString("F1") & vbTab & t.Bs.ToString("F1") & vbTab & t.Twa.ToString("F1"))
      Next
      righe.Add("Dn")
      For Each Tws As Double In twss
        Dim t = TgtManager.Tgt.ValoreTgt(False, Tws, "bs")
        righe.Add(Tws.ToString("F1") & vbTab & t.Vmg.ToString("F1") & vbTab & t.Bs.ToString("F1") & vbTab & t.Twa.ToString("F1"))
      Next
      Clipboard.SetText(String.Join(vbCrLf, righe))
    End If


    'esporta in expedition
    Dim n As New clsExportToExpedition
    n.CreaFileTgtExpedition()
    'Dim a As New clsTargetWithCurrent
    'a.Test()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Stop
    '  Dim l1 As New clsLiftAndDrag
    '  Dim Tws As Double = 10
    '  Dim tgt = TgtManager.Tgt.ValoreTgtUp(Tws, "bs")
    '  l1.Settings.AirFlowAngleOfAttack = tgt.Awa
    '  l1.Settings.AirFlowSpeed = tgt.Aws
    '  l1.Settings.AirFlowAngleOfAttack = 7
    '  l1.Settings.AirFlowSpeed = 160 / 3.6
    '  l1.Settings.AirP = 1012.62
    '  l1.Settings.AirT = 15
    '  l1.Settings.DragK = 0.1
    '  l1.Settings.K = 0.1
    '  l1.Settings.RelHumidity = 60
    '  l1.Settings.WingSpan = 30
    '  l1.Settings.WingChord = 3.5
    '  l1.Settings.WingCamber = 7 ' 7%
    '  'l1.Settings.WingLiftOffsetAngle = 15
    '  l1.Settings.WingSettingAngle = 0
    '  l1.Settings.WingEfficencyFactor = 0.5
    '  l1.CalcolaTutto()

    '  Dim Lift As Double = l1.Lift
    '  Dim LiftV2 = l1.LiftVectorBoatAxis
    '  Dim TotalDrag As Double = l1.TotalDrag
    '  Dim DragAtZeroLift As Double = l1.DragAtZeroLift
    '  Dim InducedDrag As Double = l1.InducedDrag
    '  Dim DragV2 = l1.TotalDragVectorBoatAxis
    '  Dim cl = l1.CiEl
    '  Dim cd = l1.Settings.DragK
    '  Dim r = l1.Rho

    '  Dim TwsEq As Double = l1.TwsEquivalent(5, 0, 0, True)
    '  Dim tgtEq = TgtManager.Tgt.ValoreTgtUp(TwsEq, "bs")

    '  Stop

    '  'Dim l2 As New clsLiftAndDrag
    '  'l2.AirFlowAngleOfAttack = tgt.Awa
    '  'l2.AirFlowSpeed = tgt.Aws
    '  'l2.AirP = 1013
    '  'l2.AirT = 25
    '  'l2.DragK = 0.1
    '  'l2.K = 0.1
    '  'l2.RelHumidity = 60
    '  'l2.WingArea = 90
    '  'l2.WingChord = 6
    '  'l2.WingFrontFace = 1
    '  'l2.WingLiftOffsetAngle = 15
    '  'l1.WingSettingAngle = 6
    '  'Dim Lift2 As Double = l2.Lift
    '  'Dim LiftV22 = l2.LiftVectorBoatAxis
    '  'Dim TotalDrag2 As Double = l2.TotalDrag
    '  'Dim DragAtZeroLift2 As Double = l2.DragAtZeroLift
    '  'Dim InducedDrag2 As Double = l2.InducedDrag
    '  'Dim DragV22 = l2.TotalDragVectorBoatAxis

  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    Dim R As String = clsRatingUtilities.SimulazioneWLetCoastal()
    Clipboard.SetText(R)
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsUserControlPolarManagerViwModel
  'Implements INotifyPropertyChanged

  Public Property ListaPolariDisponibili As New List(Of String)
  Public Property CanalAttuale As String
  Public Property ListaMainVarVals As New ObservableCollection(Of clsMainVarVals)
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property Titolo As String
  Public Property LoadedFile As String

  Public Sub New()
    ' TgtManager e' globale e viene ormai creato all'avvio da MainWindow:
    ' qui ci si limita a garantirne l'esistenza, senza ricaricare il file.
    MainWindow.AssicuraTgtManager()
    If Not TgtManager.Tgt Is Nothing Then
      LoadedFile = TgtManager.Tgt.Fi.Name
      ImpostaListe()
    End If
  End Sub

  Public Sub ImpostaListe()
    ListaPolariDisponibili.Clear()
    If Not TgtManager.Tgt Is Nothing Then
      ListaPolariDisponibili = TgtManager.Tgt.PolariDisponibili
    End If

    ListaMainVarVals.Clear()

    If TgtManager Is Nothing OrElse TgtManager.Tgt Is Nothing OrElse TgtManager.Tgt.ValoriMainChannel Is Nothing Then
      For i As Integer = 8 To 26
        ListaMainVarVals.Add(New clsMainVarVals(i, "Tws: " & i & " k", i / 2 = CInt(i / 2)))
      Next
    Else
      For Each v In TgtManager.Tgt.ValoriMainChannel
        ListaMainVarVals.Add(New clsMainVarVals(v, TgtManager.Tgt.MainVarName & ": " & v & " k", True))
      Next
    End If
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub
  'Dim Target As clsPolare2019ValoriTarget

  'Public Property ListaPolariDisponibili As List(Of String)
  '  Get
  '    Return _ListaPolariDisponibili
  '  End Get
  '  Set(value As List(Of String))
  '    _ListaPolariDisponibili = value
  '    OnPropertyChanged("ListaPolariDisponibili")
  '  End Set
  'End Property

  'Public Property ListaPolariDisponibili As List(Of clsPolare2019CanaleValori)
  '  Get
  '    Return _ListaPolariDisponibili
  '  End Get
  '  Set(value As List(Of clsPolare2019CanaleValori))
  '    _ListaPolariDisponibili = value
  '    OnPropertyChanged("ListaPolariDisponibili")
  '  End Set
  'End Property

  'Public Property ListaMainVarVals As ObservableCollection(Of clsMainVarVals)
  '  Get
  '    Return _ListaMainVarVals
  '  End Get
  '  Set(value As ObservableCollection(Of clsMainVarVals))
  '    _ListaMainVarVals = value
  '    OnPropertyChanged("ListaMainVarVals")
  '  End Set
  'End Property

  'Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
  '  Get
  '    Return _SeriesSource
  '  End Get
  '  Set(value As ObservableCollection(Of IChartSeriesViewModel))
  '    _SeriesSource = value
  '    OnPropertyChanged("SeriesSource")
  '  End Set
  'End Property

  'Public Property Titolo As String
  '  Get
  '    Return _Titolo
  '  End Get
  '  Set(value As String)
  '    _Titolo = value
  '    OnPropertyChanged("Titolo")
  '  End Set
  'End Property

  'Public Property LoadedFile As String
  '  Get
  '    Return _LoadedFile
  '  End Get
  '  Set(value As String)
  '    _LoadedFile = value
  '    OnPropertyChanged("LoadedFile")
  '  End Set
  'End Property

  Public Sub EsportaPolareToFaRo()
    If TgtManager.Tgt Is Nothing Then Exit Sub
    Dim ObjExpToFaro As New clsExportPolarToFaRo
    Clipboard.SetText(ObjExpToFaro.StringaDaEsportare(True))

    'MsgBox("Copied into clipboard")
  End Sub

  Public Sub SelezionaFilePolare()
    'ObjFiles.SelezionaFilePolare(False)
    TgtManager.CaricaJsonTgtFile()
    If Not TgtManager.Tgt Is Nothing Then
      LoadedFile = TgtManager.Tgt.Fi.Name
      ImpostaListe()
      VisualizzaPolare("bs")
      RicalcolaCanaliDipendentiDallaPolare()
    End If
  End Sub

  ''' <summary>
  ''' Dopo il caricamento di una polare diversa i canali target e tutti quelli che ne
  ''' dipendono contengono ancora i valori della polare precedente: vanno rifatti e i
  ''' grafici ridisegnati.
  ''' </summary>
  Private Sub RicalcolaCanaliDipendentiDallaPolare()
    If DataProvider2020 Is Nothing Then Exit Sub
    Dim MW As MainWindow = TryCast(Application.Current.MainWindow, MainWindow)
    If MW Is Nothing Then Exit Sub
    MW.RicalcolaDopoCambioPolare()
  End Sub

  Public Sub ImportaDaFileXls()
    Dim objXls As New clsExcelVpp(clsExcelVpp.eFileType.eVppXlsFile)
    If objXls.FilePolare Is Nothing Then Exit Sub
    Dim FileVppCaricato As System.IO.FileInfo = objXls.FilePolare
    Dim PathFiledaCaricare As String = FileVppCaricato.FullName
    AppConfig.ActiveProfile.TargetFile = PathFiledaCaricare
    AppConfig.Salva()
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", PathFiledaCaricare, True, True)
    'Targets = Nothing
    'Targets = New clsPolare2019(PathFiledaCaricare)
    VisualizzaPolare("bs")
    RicalcolaCanaliDipendentiDallaPolare()
  End Sub

  Public Sub VisualizzaPolare(NomeCanale As String)
    DrawPolar(NomeCanale, ListaMainVarVals.OrderBy(Function(x) x.Valore).ToList, True)
  End Sub


  Public Sub DrawPolar(NomeCanale As String, ListaMainVarVals As List(Of clsMainVarVals), VmgIfAvailable As Boolean)
    If TgtManager.Tgt Is Nothing Then Exit Sub
    SeriesSource.Clear()
    'If Polare Is Nothing Then Exit Sub
    Titolo = NomeCanale

    Dim IsVmg As Boolean = NomeCanale = "bs"
    IsVmg = IsVmg And VmgIfAvailable

    Dim DataSeriesVmg As XyDataSeries(Of Double, Double) = Nothing
    Dim LineaVmg As FastLineRenderableSeries = Nothing
    'Dim LineaAwa As FastLineRenderableSeries = Nothing
    'Dim LineaAws As FastLineRenderableSeries = Nothing

    For Each MainVarVals In ListaMainVarVals
      MainVarVals.Colore = ColoriDifferenziati(MainVarVals.Valore - ListaMainVarVals.First.Valore)
      Dim DataSerie As New XyDataSeries(Of Double, Double)
      Dim DataSerieTgt As New XyDataSeries(Of Double, Double)
      Dim DataSeriePol As New XyDataSeries(Of Double, Double)
      DataSerie.SeriesName = MainVarVals.Descrizione
      DataSerie.AcceptsUnsortedData = False
      Dim Linea As New FastLineRenderableSeries
      Linea.Stroke = MainVarVals.Colore
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Tag = MainVarVals

      If IsVmg Then
        DataSeriesVmg = New XyDataSeries(Of Double, Double)
        DataSeriesVmg.SeriesName = "Vmg"
        DataSeriesVmg.AcceptsUnsortedData = False
        LineaVmg = New FastLineRenderableSeries
        LineaVmg.XAxisId = Linea.XAxisId
        LineaVmg.YAxisId = Linea.YAxisId
        LineaVmg.Stroke = MainVarVals.Colore
        LineaVmg.StrokeDashArray = {3, 3}
        LineaVmg.Tag = MainVarVals
      End If
      Dim PuntiPolare = TgtManager.Tgt.PuntiPolare(MainVarVals.Valore, "twa", NomeCanale)
      If Not PuntiPolare Is Nothing Then
        For Each punto In PuntiPolare.OrderBy(Function(x) x.PuntoX)
          DataSerie.Append(punto.PuntoX, punto.PuntoY, New clsPuntoMetadata(False))
          If punto.IsTgt Then
            DataSerieTgt.Append(punto.PuntoX, punto.PuntoY, New clsPuntoMetadata(False))
          Else
            DataSeriePol.Append(punto.PuntoX, punto.PuntoY, New clsPuntoMetadata(False))
          End If
          If IsVmg Then
            DataSeriesVmg.Append(punto.PuntoX, punto.PuntoY * System.Math.Abs(System.Math.Cos(Radians(punto.PuntoX))))
          End If
        Next
      End If

      'For TWA As Integer = 0 To 180 Step 1
      '  Dim Valore As clsVmgVals = TgtManager.Tgt.Valore(MainVarVals.Valore, TWA, NomeCanale)
      '  'Console.WriteLine(TWA & " " & Valore.Twa)
      '  DataSerie.Append(TWA, Valore.Bs, New clsPuntoMetadata(False))
      '  If IsVmg Then
      '    DataSeriesVmg.Append(TWA, Valore.Vmg)
      '  End If
      'Next

      Linea.DataSeries = DataSerie
      Dim CSVM As New ChartSeriesViewModel(DataSerie, Linea)
      SeriesSource.Add(CSVM)
      If IsVmg Then
        LineaVmg.DataSeries = DataSeriesVmg
        CSVM = New ChartSeriesViewModel(DataSeriesVmg, LineaVmg)
        SeriesSource.Add(CSVM)
      End If

      ' disegna i punti
      Dim LineaPuntiTgt As New XyScatterRenderableSeries
      Dim LineaPuntiPol As New XyScatterRenderableSeries
      LineaPuntiTgt.XAxisId = "DefaultAxisId"
      LineaPuntiTgt.YAxisId = "DefaultAxisId"
      LineaPuntiPol.XAxisId = "DefaultAxisId"
      LineaPuntiPol.YAxisId = "DefaultAxisId"

      LineaPuntiPol.PointMarker = New EllipsePointMarker
      LineaPuntiTgt.PointMarker = New SquarePointMarker

      LineaPuntiTgt.PointMarker.Stroke = MainVarVals.Colore
      LineaPuntiTgt.PointMarker.Height = 10
      LineaPuntiTgt.PointMarker.Width = 10
      LineaPuntiTgt.PointMarker.StrokeThickness = 1
      LineaPuntiTgt.PointMarker.Fill = MainVarVals.Colore
      LineaPuntiTgt.Tag = MainVarVals

      LineaPuntiPol.PointMarker.Stroke = MainVarVals.Colore
      LineaPuntiPol.PointMarker.Height = 10
      LineaPuntiPol.PointMarker.Width = 10
      LineaPuntiPol.PointMarker.StrokeThickness = 1
      LineaPuntiPol.PointMarker.Fill = MainVarVals.Colore
      LineaPuntiPol.Tag = MainVarVals

      LineaPuntiTgt.DataSeries = DataSerieTgt
      Dim CSVMtgt As New ChartSeriesViewModel(LineaPuntiTgt.DataSeries, LineaPuntiTgt)
      SeriesSource.Add(CSVMtgt)

      LineaPuntiPol.DataSeries = DataSeriePol
      Dim CSVMpol As New ChartSeriesViewModel(LineaPuntiPol.DataSeries, LineaPuntiPol)
      SeriesSource.Add(CSVMpol)


    Next
    AggiornaPeriodiVisibili()
  End Sub


  Public Sub AggiornaPeriodiVisibili()
    'If PeriodsManager.PeriodsTrigger.ListaInAggiornamento Then Exit Sub
    Dim AtLeastOneTack As Boolean = False
    Dim AtLeastOneGybe As Boolean = False
    For Each linea In SeriesSource
      Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea.RenderSeries, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
      If Not LineaFS.Tag Is Nothing Then
        Dim MainVarVals As clsMainVarVals = DirectCast(LineaFS.Tag, clsMainVarVals)
        If Not MainVarVals Is Nothing Then
          LineaFS.IsVisible = MainVarVals.IsChecked
          'If Not LineaFS.PointMarker Is Nothing Then
          '  LineaFS.PointMarker
          'End If
        End If
      End If
    Next
  End Sub


  'Private Sub AggiornaColoriLista(Lista As ObservableCollection(Of clsMainVarVals), Selezione As ObservableCollection(Of SciChart.Charting.Visuals.RenderableSeries.IRenderableSeries))
  '  Dim ColoreNonSelezionati As Windows.Media.Color = Colors.Gray
  '  ColoreNonSelezionati.A = 20
  '  Dim SfondoBase As Windows.Media.Color = Colors.White
  '  Dim SfondoSelezionati As Windows.Media.Color = Colors.Yellow
  '  If Selezione.Count = 0 Then
  '    For Each Controllo In Lista
  '      For Each linea In SeriesSource
  '        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
  '        If Not LineaFS.Tag Is Nothing Then
  '          Dim MainVarVals As clsMainVarVals = DirectCast(LineaFS.Tag, clsMainVarVals)
  '          If Not MainVarVals Is Nothing Then
  '            MainVarVals.ColoreSfondo = SfondoBase 'per cambiare il colore nella listvew
  '            LineaFS.Stroke = MainVarVals.Colore
  '            If Not LineaFS.PointMarker Is Nothing Then
  '              LineaFS.PointMarker.Stroke = MainVarVals.Colore
  '              LineaFS.PointMarker.Fill = MainVarVals.Colore
  '            End If
  '          End If
  '        End If
  '      Next
  '    Next
  '  Else
  '    For Each Controllo In Lista
  '      For Each linea In SeriesSource
  '        Dim LineaFS As SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries = DirectCast(linea, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries)
  '        If Not IsNumeric(LineaFS.Tag) Then
  '          Dim MainVarVals As clsMainVarVals = DirectCast(LineaFS.Tag, clsMainVarVals)
  '          If Not MainVarVals Is Nothing Then
  '            Dim listOk = Selezione.Where(Function(x) DirectCast(x, SciChart.Charting.Visuals.RenderableSeries.BaseRenderableSeries).Tag Is MainVarVals).ToList
  '            MainVarVals.ColoreSfondo = ColoreNonSelezionati
  '            LineaFS.Stroke = ColoreNonSelezionati
  '            If Not LineaFS.PointMarker Is Nothing Then
  '              LineaFS.PointMarker.Stroke = ColoreNonSelezionati
  '              LineaFS.PointMarker.Fill = ColoreNonSelezionati
  '            End If
  '            For Each pp In listOk
  '              MainVarVals.ColoreSfondo = SfondoSelezionati
  '              LineaFS.Stroke = MainVarVals.Colore
  '              If Not LineaFS.PointMarker Is Nothing Then
  '                LineaFS.PointMarker.Stroke = MainVarVals.Colore
  '                LineaFS.PointMarker.Fill = MainVarVals.Colore
  '              End If
  '            Next
  '          End If
  '        End If
  '      Next
  '    Next
  '  End If
  'End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsMainVarVals
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property Valore As Double
  Public Property IsChecked As Boolean
  Public Property Descrizione As String
  Public Property StrTempVal As String
  Public Property Colore As Color
  Public Property ColoreSfondo As Color

  Public Sub New(Valore As Double, Descrizione As String, IsChecked As Boolean)
    _Valore = Valore
    _Descrizione = Descrizione
    _IsChecked = IsChecked
  End Sub

  'Public Property Valore As Double
  '  Get
  '    Return _Valore
  '  End Get
  '  Set(value As Double)
  '    _Valore = value
  '    OnPropertyChanged("Valore")
  '  End Set
  'End Property

  'Public Property IsChecked As Boolean
  '  Get
  '    Return _IsChecked
  '  End Get
  '  Set(value As Boolean)
  '    _IsChecked = value
  '    OnPropertyChanged("IsChecked")
  '  End Set
  'End Property

  'Public Property Descrizione As String
  '  Get
  '    Return _Descrizione
  '  End Get
  '  Set(value As String)
  '    _Descrizione = value
  '    OnPropertyChanged("Descrizione")
  '  End Set
  'End Property

  'Public Property StrTempVal As String
  '  Get
  '    Return _StrTempVal
  '  End Get
  '  Set(value As String)
  '    _StrTempVal = value
  '    OnPropertyChanged("StrTempVal")
  '  End Set
  'End Property

  'Public Property Colore As Color
  '  Get
  '    Return _Colore
  '  End Get
  '  Set(value As Color)
  '    _Colore = value
  '    OnPropertyChanged("Colore")
  '  End Set
  'End Property

  'Public Property ColoreSfondo As Color
  '  Get
  '    Return _ColoreSfondo
  '  End Get
  '  Set(value As Color)
  '    _ColoreSfondo = value
  '    OnPropertyChanged("ColoreSfondo")
  '  End Set
  'End Property

End Class


Public Class clsTargetWithCurrent


  Public Sub Test()

    Dim EffettiCorrente As New List(Of clsEffettoCorrente)
    Dim txt As String = "Upwind" & vbCrLf & "OvGr WndSpd" & vbTab & "Cr Rate" & vbTab & "Cr RelDir" & vbTab & "OvWt WndSpd" & vbTab & "OvGr VmgNetGain" & vbTab & "OvGrMtMin VmgNetGain" & vbTab & "OvGr Vmg" & vbTab & "OvWt Vmg" & vbTab & "Curr Vmg" & vbCrLf
    Dim twsFrom As Double = 8
    Dim twsTo As Double = 20
    Dim cRate As Double = 1
    For tws As Integer = twsFrom To twsTo Step 1
      Dim ec As List(Of clsEffettoCorrente) = DeltaVmg(tws, cRate, True, TgtManager.Tgt)
      For Each el In ec.Where(Function(x) x.CurrRelAngle <= 180).ToList
        If el Is ec.First Then
          txt &= tws & vbTab & cRate & vbTab
        Else
          txt &= vbTab & vbTab
        End If
        txt &= el.CurrRelAngle.ToString("F0") & vbTab & el.vWOW.Range.ToString("F1") & vbTab
        If el.vWOW.Range >= twsFrom AndAlso el.vWOW.Range <= twsTo Then
          txt &= el.DeltaVmgOverGround.ToString("F1") & vbTab & el.DeltaVmgOverGroundMtMin.ToString("F0") & vbTab & el.VmgOG.ToString("F1") & vbTab & el.VmgOW.ToString("F1") & vbTab & el.CurrentAlongVmg.ToString("F1") & vbCrLf
        Else
          txt &= "na" & vbTab & "na" & vbTab & "na" & vbTab & "na" & vbCrLf
        End If
      Next
      'txt &= vbCrLf
    Next
    txt &= vbCrLf
    txt &= vbCrLf
    Clipboard.SetText(txt)
    Stop
    txt = "Downwind" & vbCrLf & "OvGr WndSpd" & vbTab & "Cr Rate" & vbTab & "Cr RelDir" & vbTab & "OvWt WndSpd" & vbTab & "OvGr VmgNetGain" & vbTab & "OvGrMtMin VmgNetGain" & vbTab & "OvGr Vmg" & vbTab & "OvWt Vmg" & vbTab & "Curr Vmg" & vbCrLf
    For tws As Integer = twsFrom To twsTo Step 1
      Dim ec As List(Of clsEffettoCorrente) = DeltaVmg(tws, cRate, False, TgtManager.Tgt)
      For Each el In ec.Where(Function(x) x.CurrRelAngle <= 180).ToList
        If el Is ec.First Then
          txt &= tws & vbTab & cRate & vbTab
        Else
          txt &= vbTab & vbTab
        End If
        txt &= el.CurrRelAngle.ToString("F0") & vbTab & el.vWOW.Range.ToString("F1") & vbTab
        If el.vWOW.Range >= twsFrom AndAlso el.vWOW.Range <= twsTo Then
          txt &= el.DeltaVmgOverGround.ToString("F1") & vbTab & el.DeltaVmgOverGroundMtMin.ToString("F0") & vbTab & el.VmgOG.ToString("F1") & vbTab & el.VmgOW.ToString("F1") & vbTab & el.CurrentAlongVmg.ToString("F1") & vbCrLf
        Else
          txt &= "na" & vbTab & "na" & vbTab & "na" & vbTab & "na" & vbCrLf
        End If
      Next
      'txt &= vbCrLf
    Next
    Clipboard.SetText(txt)
    Stop

  End Sub



  Public Function DeltaVmg(Tws As Double, cRate As Double, Upwind As Boolean, Tgt As clsTgt) As List(Of clsEffettoCorrente) ' AngoloRelativo , Valore delta Vmg
    Dim lTmp As New List(Of clsEffettoCorrente)
    Dim vOverGround As clsValoriPuntoPolare = Tgt.ValoreTgt(Upwind, Tws, "bs")
    Dim vTws As New clsVettore2D(Tws, 0) 'wind from north
    For i As Integer = 0 To 359 Step 30 'orientamento relativo della corrente rispetto al vento, 0 significa che VIENE dalla stassa direzione del vento
      Dim EC As New clsEffettoCorrente(Tws, i, vOverGround.Vmg) ' nel calcolo la corrente viene girata perche lei correttamente va verso mentre il vento viene da
      Dim vCur As New clsVettore2D(cRate, SommaAngolo180adAngolo360(180, i))
      Dim vWoW As clsVettore2D = vTws.SommaVettoriale(vCur)
      EC.vWOW = vWoW
      Dim vOverWater As clsValoriPuntoPolare = Tgt.ValoreTgt(Upwind, vWoW.Range, "bs")
      EC.VmgOW = vOverWater.Vmg
      EC.CurrentAlongVmg = If(Upwind, -1, 1) * cRate * System.Math.Cos(Radians(i))
      EC.DeltaVmgOverGround = vOverWater.Vmg - vOverGround.Vmg + EC.CurrentAlongVmg
      lTmp.Add(EC)
    Next
    Return lTmp
  End Function


End Class

Public Class clsEffettoCorrente
  Dim _TwsOg As Double
  Dim _CurrRelAngle As Double
  Dim _vWoW As clsVettore2D
  Dim _VmgOG As Double
  Dim _VmgOW As Double
  Dim _DeltaVmgOverGround As Double
  Dim _CurrentAlongVmg As Double

  Public Property TwsOg As Double
    Get
      Return _TwsOg
    End Get
    Set(value As Double)
      _TwsOg = value
    End Set
  End Property

  Public Property CurrRelAngle As Double
    Get
      Return _CurrRelAngle
    End Get
    Set(value As Double)
      _CurrRelAngle = value
    End Set
  End Property

  Public Property vWOW As clsVettore2D
    Get
      Return _vWoW
    End Get
    Set(value As clsVettore2D)
      _vWoW = value
    End Set
  End Property

  Public Property VmgOG As Double
    Get
      Return _VmgOG
    End Get
    Set(value As Double)
      _VmgOG = value
    End Set
  End Property

  Public Property VmgOW As Double
    Get
      Return _VmgOW
    End Get
    Set(value As Double)
      _VmgOW = value
    End Set
  End Property

  Public Property DeltaVmgOverGround As Double
    Get
      Return _DeltaVmgOverGround
    End Get
    Set(value As Double)
      _DeltaVmgOverGround = value
    End Set
  End Property
  Public ReadOnly Property DeltaVmgOverGroundMtMin As Double
    Get
      Return _DeltaVmgOverGround / 60 * 1852
    End Get
  End Property

  Public Property CurrentAlongVmg As Double
    Get
      Return _CurrentAlongVmg
    End Get
    Set(value As Double)
      _CurrentAlongVmg = value
    End Set
  End Property

  Public Sub New(twsOg As Double, currRelAngle As Double, vmgOG As Double)
    _TwsOg = twsOg
    _CurrRelAngle = currRelAngle
    _VmgOG = vmgOG
  End Sub

End Class