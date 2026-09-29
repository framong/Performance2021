Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Media.TextFormatting
Imports Newtonsoft.Json
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SPwpf.clsXYPlotSettings

Public Class UserControlBenchmarks
  'Public Property VM As UserControlBenchmarksViewModel
  ''Public Property VM As New UserControlBenchmarksViewModel
  'Dim cambiato As Boolean = False

  'Public Sub New()
  '  'Me.DataContext = VM
  '  ' This call is required by the designer.
  '  InitializeComponent()

  '  ' --- Nulla di ciò che segue deve girare nel designer XAML ---
  '  If IsInDesignMode Then Exit Sub

  '  Me.DataContext = VM

  '  ' Add any initialization after the InitializeComponent() call.
  '  BenchmarkTypes.ItemsSource = System.Enum.GetValues(GetType(clsBenchmark.eBenchmarkType)).Cast(Of clsBenchmark.eBenchmarkType)

  'End Sub

  Public Property VM As UserControlBenchmarksViewModel
  Dim cambiato As Boolean = False

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' --- Nulla di ciò che segue deve girare nel designer XAML ---
    If IsInDesignMode Then Exit Sub

    ' >>> QUESTA RIGA MANCAVA <
    VM = New UserControlBenchmarksViewModel()

    Me.DataContext = VM

    ' Add any initialization after the InitializeComponent() call.
    BenchmarkTypes.ItemsSource = System.Enum.GetValues(GetType(clsBenchmark.eBenchmarkType)).Cast(Of clsBenchmark.eBenchmarkType)

  End Sub

  'Public Property VM As UserControlBenchmarksViewModel
  '  Get
  '    Return _VM
  '  End Get
  '  Set(value As UserControlBenchmarksViewModel)
  '    _VM = value
  '  End Set
  'End Property

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.BM.AggiornaPuntiGrafico()
    Plot.ZoomExtents()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    VM.BM.SalvaFile()
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    Dim Current As String = VM.BM.SelectedBenchmark.Name
    VM.BM.ImpostazioniIniziali()
    VM.BM.AggiornaPuntiGrafico()
    VM.BM.SelectedBenchmark = VM.BM.ListaBenchmarks.Where(Function(x) x.Name = Current).FirstOrDefault
    Plot.ZoomExtents()
  End Sub

  Private Sub ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    If Plot Is Nothing Then Exit Sub
    Plot.ZoomExtents()
  End Sub

  Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
    VM.BM.AggiornaPuntiGrafico()
    Plot.ZoomExtents()
  End Sub

  Private Sub TextBox_TargetUpdated(sender As Object, e As DataTransferEventArgs)
    VM.BM.AggiornaPuntiGrafico()
    Plot.ZoomExtents()
  End Sub

  Private Sub BenchDG_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
    cambiato = False
    If e.EditAction = DataGridEditAction.Commit Then
      Dim c = e.Column.DisplayIndex
      Dim r = e.Row.GetIndex
      Dim cella = DirectCast(e.EditingElement, TextBox)
      Dim nv = cella.Text
      Dim el = VM.BM.SelectedBenchmark.Values(r)
      If c = 0 Then
        cambiato = Not el.X.ToString = nv
      Else
        cambiato = Not el.Y.ToString = nv
      End If
    End If
  End Sub

  Private Sub BenchDG_CurrentCellChanged(sender As Object, e As EventArgs)
    If cambiato Then
      VM.BM.AggiornaPuntiGrafico()
      Plot.ZoomExtents()
      cambiato = False
    End If
  End Sub

  Private Sub BenchDG_SelectedCellsChanged(sender As Object, e As SelectedCellsChangedEventArgs)
    If cambiato Then
      VM.BM.AggiornaPuntiGrafico()
      Plot.ZoomExtents()
      cambiato = False
    End If
  End Sub

  Private Sub TextBox_KeyUp(sender As Object, e As KeyEventArgs)
    If e.Key = Key.Enter Then
      If cambiato Then
        VM.BM.AggiornaPuntiGrafico()
        Plot.ZoomExtents()
        cambiato = False
      End If
    End If
  End Sub

  Private Sub TextBox_PreviewKeyUp(sender As Object, e As KeyEventArgs)
    Stop
  End Sub

  Private Sub TextBox_PreviewKeyDown(sender As Object, e As KeyEventArgs)

  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    VM.BM.ImportBenchmarksFromTxt()
  End Sub

  Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)
    VM.BM.RemoveBenchmark()
  End Sub

  Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)

  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class UserControlBenchmarksViewModel

  Public Property BM As clsBenchmarksManager
    Get
      Return BenchManager
    End Get
    Set(value As clsBenchmarksManager)
      BenchManager = value
    End Set
  End Property


  Public Sub New()
    BenchManager.ImpostazioniIniziali()
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsBenchmarksManager
  Public Property FilePath As String
  Private Property Benchmarks As clsBenchmarks
  Public Property ListaBenchmarks As New ObservableCollection(Of clsBenchmark)
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property Ascissa As String
  Public Property Ordinata As String
  Public Property Titolo As String

  Public Property DrawSameType As Boolean = True


  Public ReadOnly Property GetGybeLoss As clsBenchmark
    Get
      Return ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eGybeLoss).FirstOrDefault
    End Get
  End Property

  Public ReadOnly Property GetTackLoss As clsBenchmark
    Get
      Return ListaBenchmarks.Where(Function(x) x.Type = clsBenchmark.eBenchmarkType.eTackLoss).FirstOrDefault
    End Get
  End Property

  Public ReadOnly Property GetBenchmark(Type As clsBenchmark.eBenchmarkType, RefChannel As String, ValChannel As String) As clsBenchmark
    Get
      Return ListaBenchmarks.Where(Function(x) x.Type = Type AndAlso x.RefChannel = RefChannel AndAlso x.ValChannel = ValChannel).FirstOrDefault
    End Get
  End Property



  Dim _SelectedBenchmark As clsBenchmark
  Public Property SelectedBenchmark As clsBenchmark
    Get
      Return _SelectedBenchmark
    End Get
    Set(value As clsBenchmark)
      _SelectedBenchmark = value
      If Not value Is Nothing Then AggiornaPuntiGrafico()
    End Set
  End Property

  Public Sub New()

  End Sub

  Public Sub ImpostazioniIniziali()
    If AppConfig.ActiveProfile.BenchmarksFileName = "" Then
      AppConfig.ActiveProfile.BenchmarksFileName = "Benchmarks.json"
      AppConfig.Salva()
    End If
    FilePath = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.BenchmarksFileName)
    Benchmarks = clsKillerSeriale.LoadConfigurationGeneric(Of clsBenchmarks)(FilePath)
    If Benchmarks Is Nothing OrElse Benchmarks.Benchmarks Is Nothing Then
      Benchmarks = New clsBenchmarks
      Benchmarks.Benchmarks = New List(Of clsBenchmark)
      Dim Tmp As New clsBenchmark
      Tmp.ImpostaValori(clsBenchmark.eBenchmarkType.eTackLoss, "TacksLoss", "Tws", "", ValoriDefaultTacksLoss)
      Benchmarks.Benchmarks.Add(Tmp)
      Tmp = New clsBenchmark
      Tmp.ImpostaValori(clsBenchmark.eBenchmarkType.eGybeLoss, "GybesLoss", "Tws", "", ValoriDefaultGybesLoss)
      Benchmarks.Benchmarks.Add(Tmp)
      For Each b In Benchmarks.Benchmarks.OrderBy(Function(x) x.ValChannel).OrderBy(Function(x) x.RefChannel).OrderBy(Function(x) x.Name).OrderBy(Function(x) x.Type).ToList
        ListaBenchmarks.Add(b)
      Next
      SalvaFile()
    End If
    ListaBenchmarks.Clear()


    Dim vmglist = New List(Of clsBenchmark)
    For Each b In Benchmarks.Benchmarks.Where(Function(x) x.RefChannel = "Tws" AndAlso x.ValChannel = "Bs" AndAlso (x.Type = clsBenchmark.eBenchmarkType.eVmgUp OrElse x.Type = clsBenchmark.eBenchmarkType.eVmgDn)).ToList
      Dim t = Benchmarks.Benchmarks.Where(Function(x) x.Name = b.Name AndAlso x.ValChannel = "Twa" AndAlso x.Type = b.Type).FirstOrDefault
      If Not t Is Nothing Then
        Dim v = New clsBenchmark
        v.RefChannel = b.RefChannel
        v.ValChannel = "Vmg"
        v.Name = b.Name
        v.Type = b.Type
        v.Show = b.Show
        v.Save = False
        v.Values = New List(Of clsDoubleXY)
        For Each vv In b.Values
          Dim tt = t.Values.Where(Function(x) x.X = vv.X).FirstOrDefault
          If Not tt Is Nothing Then
            Dim bs = vv.Y
            Dim twa = tt.Y
            Dim vmg = Math.Abs(bs * Math.Cos(Radians(twa)))
            v.Values.Add(New clsDoubleXY(vv.X, CInt(vmg * 10) / 10))
          End If
        Next
        If v.Values.Count > 0 Then
          vmglist.Add(v)
        End If
      End If
    Next
    For Each v In vmglist
      Benchmarks.Benchmarks.Add(v)
    Next


    For Each b In Benchmarks.Benchmarks.OrderBy(Function(x) x.ValChannel).OrderBy(Function(x) x.RefChannel).OrderBy(Function(x) x.Name).OrderBy(Function(x) x.Type).ToList
      ListaBenchmarks.Add(b)
    Next
    If ListaBenchmarks.Count > 0 Then
      SelectedBenchmark = ListaBenchmarks.First
    End If
  End Sub

  Public Function SalvaFile()
    Benchmarks.Benchmarks.Clear()
    For Each b In ListaBenchmarks.Where(Function(x) x.Save = True).ToList
      Benchmarks.Benchmarks.Add(b)
    Next
    clsKillerSeriale.SaveConfigurationGeneric(Of clsBenchmarks)(Benchmarks, FilePath)
    Return True
  End Function

  Public Sub RemoveBenchmark()
    Dim msg As MsgBoxResult = MsgBox("Permantely Delete Selected Benchmark from the list?" & vbCrLf & "Yes: Permanetly" & vbCrLf & "No: just don't show it" & vbCrLf & "Cancel abort", MsgBoxStyle.YesNoCancel, "Benchmark Manager")
    Select Case msg
      Case MsgBoxResult.Yes
        ListaBenchmarks.Remove(SelectedBenchmark)
        SalvaFile()
      Case MsgBoxResult.No
        SelectedBenchmark.Show = False
        SalvaFile()
      Case Else
    End Select
  End Sub

  Public Sub ImportBenchmarksFromTxt()
    Dim FP As String = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.BenchmarksFileName)
    Dim FD = New System.IO.FileInfo(FP)
    Dim n, e
    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(FD.Directory.FullName, "", "", "", n)
    If pf Is Nothing Then Exit Sub
    Dim nb As clsBenchmark
    Dim vTmp As New List(Of clsDoubleXY)
    Dim bt, bn, rh, vh

    Dim listatmp = New List(Of clsBenchmark)
    For Each b In ListaBenchmarks
      listatmp.Add(b)
    Next
    ListaBenchmarks.Clear()

    For Each fl In pf
      Dim c = ObjFiles.LeggiTuttoFileTesto(fl, e)
      Dim rr = c.Split(vbCrLf)
      For Each r In rr
        r = r.TrimEnd
        r = r.TrimStart
        If r.IndexOf(":") > -1 Then
          Dim col = r.Split(":")
          If col(0).ToLower = "type" Then
            bt = TypeFromString(col(1))
            nb = Nothing
          ElseIf col(0).ToLower = "name" Then
            bn = col(1)
          End If
        ElseIf r.IndexOf(vbTab) > -1 Then
          Dim col = r.Split(vbTab)
          If Not IsNumeric(col(0)) Then
            If Not nb Is Nothing Then
              nb.Type = bt
              nb.Name = bn
              nb.RefChannel = rh
              nb.ValChannel = vh
              nb.Show = True
              nb.Values = New List(Of clsDoubleXY)
              For Each v In vTmp
                nb.Values.Add(v)
              Next
              listatmp.Add(nb)
            End If
            nb = New clsBenchmark
            rh = col(0)
            vh = col(1)
            vTmp.Clear()
          Else
            vTmp.Add(New clsDoubleXY(col(0), col(1)))
          End If

        End If
      Next
      If Not nb Is Nothing AndAlso Not vTmp Is Nothing OrElse vTmp.Count = 0 Then
        nb.Type = bt
        nb.Name = bn
        nb.RefChannel = rh
        nb.ValChannel = vh
        nb.Show = True
        nb.Values = New List(Of clsDoubleXY)
        For Each v In vTmp
          nb.Values.Add(v)
        Next
        listatmp.Add(nb)
      End If
    Next

    Dim vmglist = New List(Of clsBenchmark)
    For Each b In listatmp.Where(Function(x) x.RefChannel = "Tws" AndAlso x.ValChannel = "Bs" AndAlso (x.Type = clsBenchmark.eBenchmarkType.eVmgUp OrElse x.Type = clsBenchmark.eBenchmarkType.eVmgDn)).ToList
      Dim t = listatmp.Where(Function(x) x.Name = b.Name AndAlso x.ValChannel = "Twa" AndAlso x.Type = b.Type).FirstOrDefault
      If Not t Is Nothing Then
        Dim v = New clsBenchmark
        v.RefChannel = b.RefChannel
        v.ValChannel = "Vmg"
        v.Name = b.Name
        v.Type = b.Type
        v.Show = b.Show
        v.Save = False
        v.Values = New List(Of clsDoubleXY)
        For Each vv In b.Values
          Dim tt = t.Values.Where(Function(x) x.X = vv.X).FirstOrDefault
          If Not tt Is Nothing Then
            Dim bs = vv.Y
            Dim twa = tt.Y
            Dim vmg = Math.Abs(bs * Math.Cos(Radians(twa)))
            v.Values.Add(New clsDoubleXY(vv.X, CInt(vmg * 10) / 10))
          End If
        Next
        If v.Values.Count > 0 Then
          vmglist.Add(v)
        End If
      End If
    Next
    For Each v In vmglist
      listatmp.Add(v)
    Next

    For Each v In listatmp.OrderBy(Function(x) x.ValChannel).OrderBy(Function(x) x.RefChannel).OrderBy(Function(x) x.Name).OrderBy(Function(x) x.Type).ToList
      ListaBenchmarks.Add(v)
    Next


    Dim msg As MsgBoxResult = MsgBox("Benchmarks imported, save it now?" & vbCrLf & "Yes: Permanetly" & vbCrLf & "No: save later", MsgBoxStyle.YesNo, "Benchmark Manager")
    Select Case msg
      Case MsgBoxResult.Yes
        SalvaFile()
      Case Else
    End Select


  End Sub

  Private Function TypeFromString(Type As String) As clsBenchmark.eBenchmarkType
    Select Case Type.ToLower
      Case "upwind"
        Return clsBenchmark.eBenchmarkType.eVmgUp
      Case "downwind"
        Return clsBenchmark.eBenchmarkType.eVmgDn
      Case Else
        Return clsBenchmark.eBenchmarkType.eUnknown
    End Select
  End Function


  Public Function ValoriDefaultTacksLoss() As List(Of clsDoubleXY)
    Dim vTmp As New List(Of clsDoubleXY)
    vTmp.Add(New clsDoubleXY(4, 30))
    vTmp.Add(New clsDoubleXY(6, 28))
    vTmp.Add(New clsDoubleXY(8, 26))
    vTmp.Add(New clsDoubleXY(10, 24))
    vTmp.Add(New clsDoubleXY(12, 22))
    vTmp.Add(New clsDoubleXY(14, 20))
    vTmp.Add(New clsDoubleXY(16, 18))
    vTmp.Add(New clsDoubleXY(20, 16))
    vTmp.Add(New clsDoubleXY(25, 16))
    vTmp.Add(New clsDoubleXY(30, 16))
    Return vTmp
  End Function

  Public Function ValoriDefaultGybesLoss() As List(Of clsDoubleXY)
    Dim vTmp As New List(Of clsDoubleXY)
    vTmp.Add(New clsDoubleXY(4, 26))
    vTmp.Add(New clsDoubleXY(6, 24))
    vTmp.Add(New clsDoubleXY(8, 22))
    vTmp.Add(New clsDoubleXY(10, 20))
    vTmp.Add(New clsDoubleXY(12, 18))
    vTmp.Add(New clsDoubleXY(14, 16))
    vTmp.Add(New clsDoubleXY(16, 14))
    vTmp.Add(New clsDoubleXY(20, 12))
    vTmp.Add(New clsDoubleXY(25, 12))
    vTmp.Add(New clsDoubleXY(30, 12))
    Return vTmp
  End Function


  Public Sub AggiornaPuntiGrafico()
    Dim diametro As Integer = 10
    Dim StrArray(1) As Double
    StrArray(0) = 5
    StrArray(1) = 8

    SeriesSource.Clear()
    Ascissa = SelectedBenchmark.RefChannel
    Ordinata = SelectedBenchmark.ValChannel
    Titolo = SelectedBenchmark.Name

    Dim ListaBnch As New List(Of clsBenchmark)
    ListaBnch.Add(SelectedBenchmark)
    If DrawSameType Then
      Dim bb = ListaBenchmarks.Where(Function(x) x.Show = True AndAlso x.Type = SelectedBenchmark.Type AndAlso x.RefChannel = SelectedBenchmark.RefChannel AndAlso x.ValChannel = SelectedBenchmark.ValChannel).ToList
      If Not bb Is Nothing Then
        For Each b In bb
          If Not b Is SelectedBenchmark Then
            ListaBnch.Add(b)
          End If
        Next
      End If
    End If

    Dim idColore As Integer = 0
    For Each b In ListaBnch
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim DataSeriesLinea As New XyDataSeries(Of Double, Double)
      Dim Punti As New XyScatterRenderableSeries
      Dim Linea As New FastLineRenderableSeries
      Punti.XAxisId = "DefaultAxisId"
      Punti.YAxisId = "DefaultAxisId"
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Punti.PointMarker = New EllipsePointMarker
      Dim c As Color = ColoriDifferenziati(idColore)
      idColore += 1
      'Select Case b.Type
      '  Case clsBenchmark.eBenchmarkType.eGybeLoss
      '    c = Colors.Gold
      '  Case clsBenchmark.eBenchmarkType.eTackLoss
      '    c = Colors.SkyBlue
      '  Case clsBenchmark.eBenchmarkType.eVmgUp
      '    c = Colors.DarkBlue
      '  Case clsBenchmark.eBenchmarkType.eVmgDn
      '    c = Colors.Yellow
      '  Case Else
      '    c = Colors.Green
      'End Select
      'c.A = 255
      Punti.PointMarker.Stroke = Colors.Black
      Linea.Stroke = c
      If b Is SelectedBenchmark Then
        Linea.StrokeThickness = diametro / 2
        Punti.PointMarker.Width = diametro
        Punti.PointMarker.Height = diametro
        Punti.PointMarker.StrokeThickness = 2
        Punti.PointMarker.Fill = c
      Else
        Linea.StrokeThickness = diametro / 4
        Linea.StrokeDashArray = StrArray
        Punti.PointMarker.Width = diametro / 2
        Punti.PointMarker.Height = diametro / 2
        Punti.PointMarker.StrokeThickness = 1
        Punti.PointMarker.Fill = c
      End If
      DataSeries.SeriesName = "" 'b.Name & " dots"
      DataSeriesLinea.SeriesName = b.Name
      DataSeries.AcceptsUnsortedData = True
      For Each punto In b.Values
        DataSeries.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
        DataSeriesLinea.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
      Next
      Linea.DataSeries = DataSeriesLinea
      Dim CSVMlinea As New ChartSeriesViewModel(DataSeriesLinea, Linea)
      SeriesSource.Add(CSVMlinea)
      Punti.DataSeries = DataSeries
      Dim CSVMpunti As New ChartSeriesViewModel(DataSeries, Punti)
      SeriesSource.Add(CSVMpunti)
    Next



    'Dim DataSeries As New XyDataSeries(Of Double, Double)
    'Dim DataSeriesLinea As New XyDataSeries(Of Double, Double)
    'Dim Punti As New XyScatterRenderableSeries
    'Dim Linea As New FastLineRenderableSeries
    'Punti.XAxisId = "DefaultAxisId"
    'Punti.YAxisId = "DefaultAxisId"
    'Linea.XAxisId = "DefaultAxisId"
    'Linea.YAxisId = "DefaultAxisId"
    'Punti.PointMarker = New EllipsePointMarker
    'Dim c As Color = Colors.Green
    'Select Case SelectedBenchmark.Type
    '  Case clsBenchmark.eBenchmarkType.eGybeLoss
    '    c = Colors.Gold
    '  Case clsBenchmark.eBenchmarkType.eTackLoss
    '    c = Colors.SkyBlue
    '  Case clsBenchmark.eBenchmarkType.eVmgUp
    '    c = Colors.DarkBlue
    '  Case clsBenchmark.eBenchmarkType.eVmgDn
    '    c = Colors.Yellow
    '  Case Else
    '    c = Colors.Green
    'End Select
    ''c.A = 255
    'Punti.PointMarker.Stroke = Colors.Black
    'Linea.Stroke = c
    'Linea.StrokeThickness = diametro / 2
    'Punti.PointMarker.Width = diametro
    'Punti.PointMarker.Height = diametro
    'Punti.PointMarker.StrokeThickness = 2
    'Punti.PointMarker.Fill = c
    'DataSeries.SeriesName = ""
    'DataSeries.AcceptsUnsortedData = True
    'For Each punto In SelectedBenchmark.Values
    '  DataSeries.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
    '  DataSeriesLinea.Append(punto.X, punto.Y, New clsPuntoMetadata(False))
    'Next
    'Linea.DataSeries = DataSeriesLinea
    'Dim CSVMlinea As New ChartSeriesViewModel(DataSeriesLinea, Linea)
    'SeriesSource.Add(CSVMlinea)
    'Punti.DataSeries = DataSeries
    'Dim CSVMpunti As New ChartSeriesViewModel(DataSeries, Punti)
    'SeriesSource.Add(CSVMpunti)
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBenchmarks
  Public Property Benchmarks As List(Of clsBenchmark)
  'Public Property TacksLoss As clsBenchmark
  'Public Property GybesLoss As clsBenchmark

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBenchmark
  Public Property Type As eBenchmarkType = eBenchmarkType.eUnknown
  Public Property Name As String
  Public Property RefChannel As String
  Public Property ValChannel As String
  Public Property Values As List(Of clsDoubleXY)
  Public Property Show As Boolean = True
  <JsonIgnore>
  Public Property Save As Boolean = True
  <JsonIgnore>
  Public Property Draw As Boolean = False
  <JsonIgnore>
  Public Property RefCh As clsChannel2020 = Nothing
  <JsonIgnore>
  Public Property ValCh As clsChannel2020 = Nothing

  <JsonIgnore>
  Public ReadOnly Property FullName
    Get
      Return Tipo() & Name & " " & RefChannel & " " & ValChannel
    End Get
  End Property



  Public Sub ImpostaValori(Type As eBenchmarkType, Name As String, RefChannel As String, ValChannel As String, Valori As List(Of clsDoubleXY))
    Me.Type = Type
    Me.Name = Name
    Me.RefChannel = RefChannel
    Me.ValChannel = ValChannel
    Me.Values = Valori
  End Sub

  Public Sub ImpostaCanali()
    RefCh = DataProvider2020.CanaleDbl(Me.RefChannel)
    ValCh = DataProvider2020.CanaleDbl(Me.ValChannel)
  End Sub


  Private Function Tipo() As String
    Select Case Type
      Case eBenchmarkType.eGybeLoss, eBenchmarkType.eTackLoss, eBenchmarkType.eUnknown
        Return ""
      Case eBenchmarkType.eVmgUp
        Return "TgtUp "
      Case eBenchmarkType.eVmgDn
        Return "TgtDn "
      Case Else
        Return ""
    End Select
  End Function


  Public Enum eBenchmarkType
    eUnknown = -1
    eTackLoss = 0
    eGybeLoss = 1
    eVmgUp = 2
    eVmgDn = 3
    eReaching = 4
  End Enum

End Class

'<AddINotifyPropertyChangedInterface>
'  Public Class UserControlBenchmarksViewModel
'  'Implements INotifyPropertyChanged

'  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
'  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  'End Sub

'  'Dim _DefaultManoeuversConfigFile As String = AppConfig.ApplicationDataFolder & "\config\ManoeuversConfigFile.json"
'  'Dim _ManoeuversConfigFile As String ' =  AppConfig.ApplicationDataFolder & "\config\ManoeuversConfigFile.json"
'  'Dim _DefaultAccelerationsConfigFile As String = AppConfig.ApplicationDataFolder & "\config\AccelerationsConfigFile.json"
'  'Dim _AccelerationsConfigFile As String ' = AppConfig.ApplicationDataFolder & "\config\AccelerationsConfigFile.json"
'  Dim _ManoeuversConfigFile As String ' = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.ManoeuversBenchmarksFileName)
'  Dim _AccelerationsConfigFile As String ' = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.AccelerationsBenchmarksFileName)

'  Public Property MBManager As clsManoeuversBenchmarks
'  Dim _ABManager As clsAccelerationBenchmarks
'  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
'  Public Property Titolo As String = "Manoeuvers Benchmarks"
'  Dim _CurrentPlot As ePlots = ePlots.eSecLoss

'  'Public Property MBManager As clsManoeuversBenchmarks
'  '  Get
'  '    Return ManBenchManager
'  '  End Get
'  '  Set(value As clsManoeuversBenchmarks)
'  '    ManBenchManager = value
'  '    OnPropertyChanged("ManBenchManager")
'  '  End Set
'  'End Property

'  'Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
'  '  Get
'  '    Return _SeriesSource
'  '  End Get
'  '  Set(value As ObservableCollection(Of IChartSeriesViewModel))
'  '    _SeriesSource = value
'  '    OnPropertyChanged("SeriesSource")
'  '  End Set
'  'End Property

'  'Public Property Titolo As String
'  '  Get
'  '    Return _Titolo
'  '  End Get
'  '  Set(value As String)
'  '    _Titolo = value
'  '    OnPropertyChanged("Titolo")
'  '  End Set
'  'End Property

'  Public Enum ePlots
'    eSecLoss = 0
'    eMetLoss = 1
'    eVmgAvg = 2
'    eAccel = 3
'  End Enum

'  Public Sub New()
'    _ManoeuversConfigFile = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.ManoeuversBenchmarksFileName)
'    _AccelerationsConfigFile = System.IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.AccelerationsBenchmarksFileName)
'    CaricaFileBenchmark()
'  End Sub

'  Private Sub CaricaFileBenchmark()
'    '_ManoeuversConfigFile = AppConfig.ActiveProfile.ManoeuversBenchmarksFile '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "ManBenchmarkFile", "LastLoaded", _DefaultManoeuversConfigFile, True, True)
'    ManBenchManager = clsKillerSeriale.LoadConfigurationGeneric(Of clsManoeuversBenchmarks)(_ManoeuversConfigFile)
'    If ManBenchManager Is Nothing Then
'      ManBenchManager = New clsManoeuversBenchmarks
'      ManBenchManager.CaricaDefault()
'      SalvaManoeuversConfig()
'    End If
'    AccBenchManager = clsKillerSeriale.LoadConfigurationGeneric(Of clsAccelerationBenchmarks)(_AccelerationsConfigFile)
'    If AccBenchManager Is Nothing Then
'      AccBenchManager = New clsAccelerationBenchmarks()
'      AccBenchManager.CaricaDefault()
'      SalvaAccelerationsConfig()
'    End If


'    DrawBenchmarks()
'  End Sub

'  Public Sub SelezionaFileManovre()
'    Stop
'    'Dim NomeFile As String = ""
'    'Dim pathTmp As String = ObjFiles.SelezionaFile(_ManoeuversConfigFile, "Manoeuvers Benchmark Files", "Manoeuvers Benchmark Files |*.json|All Files|*.*", "json", NomeFile)
'    'Dim ManBenchTmp = clsKillerSeriale.LoadConfigurationGeneric(Of clsManoeuversBenchmarks)(pathTmp)
'    'If Not ManBenchTmp Is Nothing Then
'    '  _ManoeuversConfigFile = pathTmp
'    '  AppConfig.ActiveProfile.ManoeuversBenchmarksFile = _ManoeuversConfigFile
'    '  AppConfig.Salva()
'    '  'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "ManBenchmarkFile", _ManoeuversConfigFile, True, True)
'    '  CaricaFileBenchmark()
'    '  TabellaLossToClipBoard()
'    'End If
'  End Sub

'  Public Sub ReloadBenchmarkFiles()
'    ManBenchManager = clsKillerSeriale.LoadConfigurationGeneric(Of clsManoeuversBenchmarks)(_ManoeuversConfigFile)
'    AccBenchManager = clsKillerSeriale.LoadConfigurationGeneric(Of clsAccelerationBenchmarks)(_AccelerationsConfigFile)
'    DrawBenchmarks()
'  End Sub

'  Public Sub TogglePlot()
'    Select Case _CurrentPlot
'      Case ePlots.eSecLoss
'        _CurrentPlot = ePlots.eMetLoss
'      Case ePlots.eMetLoss
'        _CurrentPlot = ePlots.eVmgAvg
'      Case ePlots.eVmgAvg
'        _CurrentPlot = ePlots.eAccel
'      Case ePlots.eAccel
'        _CurrentPlot = ePlots.eSecLoss
'    End Select
'    DrawBenchmarks()
'  End Sub

'  Public Sub DrawBenchmarks()
'    Select Case _CurrentPlot
'      Case ePlots.eSecLoss
'        DrawManoeuversBenchmarksSecLoss()
'      Case ePlots.eMetLoss
'        DrawManoeuversBenchmarksMetLoss()
'      Case ePlots.eVmgAvg
'        DrawManoeuversBenchmarksVmgAvg()
'      Case ePlots.eAccel
'        DrawAccelerationsBenchmarks()
'    End Select
'  End Sub

'  Public Sub DrawManoeuversBenchmarksSecLoss()
'    Titolo = "Manoeuvers Loss Time"
'    SeriesSource.Clear()
'    If ManBenchManager Is Nothing Then Exit Sub


'    For Each Bench In ManBenchManager.Lista.ToList
'      Dim DataSerie As New XyDataSeries(Of Double, Double)
'      DataSerie.SeriesName = Bench.Descrizione
'      DataSerie.AcceptsUnsortedData = False
'      Dim DataSeriePunti As New XyDataSeries(Of Double, Double)
'      DataSeriePunti.SeriesName = Bench.Descrizione
'      DataSeriePunti.AcceptsUnsortedData = False

'      ' per disegnare la linea
'      Dim Linea As New FastLineRenderableSeries
'      Linea.Stroke = Bench.Colore
'      Linea.XAxisId = "DefaultAxisId"
'      Linea.YAxisId = "DefaultAxisId"
'      Linea.Tag = Bench

'      ' per disegnare i punti
'      Dim LineaPunti As New XyScatterRenderableSeries
'      LineaPunti.XAxisId = "DefaultAxisId"
'      LineaPunti.YAxisId = "DefaultAxisId"
'      LineaPunti.PointMarker = New SquarePointMarker
'      LineaPunti.PointMarker.Stroke = Bench.Colore
'      LineaPunti.PointMarker.Height = 10
'      LineaPunti.PointMarker.Width = 10
'      LineaPunti.PointMarker.StrokeThickness = 1
'      LineaPunti.PointMarker.Fill = Bench.Colore
'      LineaPunti.Tag = Bench

'      For Each CoppiaTwsLoss In Bench.ListaValoriLoss
'        DataSerie.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.GainSec, New clsPuntoMetadata(False))
'        DataSeriePunti.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.GainSec, New clsPuntoMetadata(False))
'      Next

'      Linea.DataSeries = DataSerie
'      Dim CSVM As New ChartSeriesViewModel(DataSerie, Linea)
'      SeriesSource.Add(CSVM)

'      LineaPunti.DataSeries = DataSeriePunti
'      Dim CSVMtgt As New ChartSeriesViewModel(LineaPunti.DataSeries, LineaPunti)
'      SeriesSource.Add(CSVMtgt)

'    Next
'  End Sub

'  Public Sub DrawManoeuversBenchmarksMetLoss()
'    Titolo = "Manoeuvers Loss Meters"
'    SeriesSource.Clear()
'    If ManBenchManager Is Nothing Then Exit Sub


'    For Each Bench In ManBenchManager.Lista.ToList
'      Dim DataSerie As New XyDataSeries(Of Double, Double)
'      DataSerie.SeriesName = Bench.Descrizione
'      DataSerie.AcceptsUnsortedData = False
'      Dim DataSeriePunti As New XyDataSeries(Of Double, Double)
'      DataSeriePunti.SeriesName = Bench.Descrizione
'      DataSeriePunti.AcceptsUnsortedData = False

'      ' per disegnare la linea
'      Dim Linea As New FastLineRenderableSeries
'      Linea.Stroke = Bench.Colore
'      Linea.XAxisId = "DefaultAxisId"
'      Linea.YAxisId = "DefaultAxisId"
'      Linea.Tag = Bench

'      ' per disegnare i punti
'      Dim LineaPunti As New XyScatterRenderableSeries
'      LineaPunti.XAxisId = "DefaultAxisId"
'      LineaPunti.YAxisId = "DefaultAxisId"
'      LineaPunti.PointMarker = New SquarePointMarker
'      LineaPunti.PointMarker.Stroke = Bench.Colore
'      LineaPunti.PointMarker.Height = 10
'      LineaPunti.PointMarker.Width = 10
'      LineaPunti.PointMarker.StrokeThickness = 1
'      LineaPunti.PointMarker.Fill = Bench.Colore
'      LineaPunti.Tag = Bench

'      For Each CoppiaTwsLoss In Bench.ListaValoriLoss
'        DataSerie.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.GainMeters, New clsPuntoMetadata(False))
'        DataSeriePunti.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.GainMeters, New clsPuntoMetadata(False))
'      Next

'      Linea.DataSeries = DataSerie
'      Dim CSVM As New ChartSeriesViewModel(DataSerie, Linea)
'      SeriesSource.Add(CSVM)

'      LineaPunti.DataSeries = DataSeriePunti
'      Dim CSVMtgt As New ChartSeriesViewModel(LineaPunti.DataSeries, LineaPunti)
'      SeriesSource.Add(CSVMtgt)

'    Next
'  End Sub


'  Public Sub DrawManoeuversBenchmarksVmgAvg()
'    Titolo = "Manoeuvers Average Vmg"
'    SeriesSource.Clear()
'    If ManBenchManager Is Nothing Then Exit Sub


'    For Each Bench In ManBenchManager.Lista
'      Dim DataSerie As New XyDataSeries(Of Double, Double)
'      DataSerie.SeriesName = Bench.Descrizione
'      DataSerie.AcceptsUnsortedData = False
'      Dim DataSeriePunti As New XyDataSeries(Of Double, Double)
'      DataSeriePunti.SeriesName = Bench.Descrizione
'      DataSeriePunti.AcceptsUnsortedData = False

'      ' per disegnare la linea
'      Dim Linea As New FastLineRenderableSeries
'      Linea.Stroke = Bench.Colore
'      Linea.XAxisId = "DefaultAxisId"
'      Linea.YAxisId = "DefaultAxisId"
'      Linea.Tag = Bench

'      ' per disegnare i punti
'      Dim LineaPunti As New XyScatterRenderableSeries
'      LineaPunti.XAxisId = "DefaultAxisId"
'      LineaPunti.YAxisId = "DefaultAxisId"
'      LineaPunti.PointMarker = New SquarePointMarker
'      LineaPunti.PointMarker.Stroke = Bench.Colore
'      LineaPunti.PointMarker.Height = 10
'      LineaPunti.PointMarker.Width = 10
'      LineaPunti.PointMarker.StrokeThickness = 1
'      LineaPunti.PointMarker.Fill = Bench.Colore
'      LineaPunti.Tag = Bench

'      For Each CoppiaTwsLoss In Bench.ListaValoriLoss
'        DataSerie.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.VmgAvg, New clsPuntoMetadata(False))
'        DataSeriePunti.Append(CoppiaTwsLoss.Tws, CoppiaTwsLoss.VmgAvg, New clsPuntoMetadata(False))
'      Next

'      Linea.DataSeries = DataSerie
'      Dim CSVM As New ChartSeriesViewModel(DataSerie, Linea)
'      SeriesSource.Add(CSVM)

'      LineaPunti.DataSeries = DataSeriePunti
'      Dim CSVMtgt As New ChartSeriesViewModel(LineaPunti.DataSeries, LineaPunti)
'      SeriesSource.Add(CSVMtgt)

'    Next
'  End Sub

'  Public Sub DrawAccelerationsBenchmarks()
'    Titolo = "Acceleration Time To Take Off Speed"
'    SeriesSource.Clear()
'    If ManBenchManager Is Nothing Then Exit Sub

'    Dim DataSerie As New XyDataSeries(Of Double, Double)
'    DataSerie.SeriesName = AccBenchManager.Descrizione
'    DataSerie.AcceptsUnsortedData = False
'    Dim DataSeriePunti As New XyDataSeries(Of Double, Double)
'    DataSeriePunti.SeriesName = AccBenchManager.Descrizione
'    DataSeriePunti.AcceptsUnsortedData = False

'    ' per disegnare la linea
'    Dim Linea As New FastLineRenderableSeries
'    Linea.Stroke = AccBenchManager.Colore
'    Linea.XAxisId = "DefaultAxisId"
'    Linea.YAxisId = "DefaultAxisId"
'    Linea.Tag = AccBenchManager

'    ' per disegnare i punti
'    Dim LineaPunti As New XyScatterRenderableSeries
'    LineaPunti.XAxisId = "DefaultAxisId"
'    LineaPunti.YAxisId = "DefaultAxisId"
'    LineaPunti.PointMarker = New SquarePointMarker
'    LineaPunti.PointMarker.Stroke = AccBenchManager.Colore
'    LineaPunti.PointMarker.Height = 10
'    LineaPunti.PointMarker.Width = 10
'    LineaPunti.PointMarker.StrokeThickness = 1
'    LineaPunti.PointMarker.Fill = AccBenchManager.Colore
'    LineaPunti.Tag = AccBenchManager

'    For Each Coppia In AccBenchManager.ListaValori
'      DataSerie.Append(Coppia.X, Coppia.Y, New clsPuntoMetadata(False))
'      DataSeriePunti.Append(Coppia.X, Coppia.Y, New clsPuntoMetadata(False))
'    Next

'    Linea.DataSeries = DataSerie
'    Dim CSVM As New ChartSeriesViewModel(DataSerie, Linea)
'    SeriesSource.Add(CSVM)

'    LineaPunti.DataSeries = DataSeriePunti
'    Dim CSVMtgt As New ChartSeriesViewModel(LineaPunti.DataSeries, LineaPunti)
'    SeriesSource.Add(CSVMtgt)

'  End Sub


'  Public Sub SalvaManoeuversConfig()
'    clsKillerSeriale.SaveConfigurationGeneric(Of clsManoeuversBenchmarks)(ManBenchManager, _ManoeuversConfigFile)
'  End Sub

'  Public Sub SalvaAccelerationsConfig()
'    clsKillerSeriale.SaveConfigurationGeneric(Of clsAccelerationBenchmarks)(AccBenchManager, _AccelerationsConfigFile)
'  End Sub

'  Public Sub UpdateSecGainFromTgt()
'    UpdateSecGainFromTgt(TgtManager.Tgt.Polare("vmg"))
'  End Sub
'  Public Sub UpdateSecGainFromTgt(Tgt As clsChannelTarget)
'    Dim DurataSecondi As Double = AppConfig.ActiveProfile.PeriodsSettings.SecsPostManoeuver + AppConfig.ActiveProfile.PeriodsSettings.SecsAnteManoeuver
'    For Each elemento In ManBenchManager.Lista
'      For Each Valore In elemento.ListaValoriLoss
'        Dim Tws As Double = Valore.Tws
'        If Tws >= Tgt.Rows.First.RowValue AndAlso Tws <= Tgt.Rows.Last.RowValue Then
'          Dim VmgTgt As Double = Tgt.TargetValue(Tws, elemento.ManoeuversType = clsPeriod2021.ePeriodType.eTack)
'          Dim Vmg As Double = Valore.VmgAvg
'          Dim DistAtvmg As Double = KtsToMS(Vmg) * DurataSecondi 'metri percorsi
'          Dim DistAtvmgTgt As Double = KtsToMS(VmgTgt) * DurataSecondi 'metri percorsi
'          Dim GainMeters As Double = DistAtvmg - DistAtvmgTgt
'          Valore.GainMeters = GainMeters
'          Dim Gain As Double = (DistAtvmg / KtsToMS(VmgTgt)) - DurataSecondi 'tempo a vmg target
'          Valore.GainSec = Gain
'        End If
'      Next
'    Next
'    SalvaManoeuversConfig()
'    TabellaLossToClipBoard()
'    DrawBenchmarks()
'  End Sub

'  Private Sub TabellaLossToClipBoard()
'    Dim txt As String = ""
'    For Each elemento In ManBenchManager.Lista
'      txt &= elemento.ManoeuversType.ToString.TrimStart("e") & vbCrLf
'      txt &= "Tws" & vbTab
'      For Each Valore In elemento.ListaValoriLoss
'        txt &= Valore.Tws.ToString("F0") & vbTab
'      Next
'      txt = txt.TrimEnd(vbTab) & vbCrLf
'      txt &= "LossSec" & vbTab
'      For Each Valore In elemento.ListaValoriLoss
'        txt &= -Valore.GainSec.ToString("F1") & vbTab
'      Next
'      txt = txt.TrimEnd(vbTab) & vbCrLf
'      txt &= "LossMt" & vbTab
'      For Each Valore In elemento.ListaValoriLoss
'        txt &= -Valore.GainMeters.ToString("F0") & vbTab
'      Next
'      txt = txt.TrimEnd(vbTab) & vbCrLf
'      txt &= "VmgAvg" & vbTab
'      For Each Valore In elemento.ListaValoriLoss
'        txt &= Valore.VmgAvg.ToString("F1") & vbTab
'      Next
'      txt = txt.TrimEnd(vbTab) & vbCrLf & vbCrLf
'    Next

'    Clipboard.SetText(txt)
'  End Sub

'End Class

'<AddINotifyPropertyChangedInterface>
'Public Class clsManoeuversBenchmarks
'  'Implements INotifyPropertyChanged
'  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged


'  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  'End Sub

'  Public Property TacksLossBenchmarks As clsManoeuverBenchmarks
'  Public Property GybesLossBenchmarks As clsManoeuverBenchmarks
'  Public Property _Lista As New List(Of clsManoeuverBenchmarks)

'  'Public Property TacksLossBenchmarks As clsManoeuverBenchmarks
'  '  Get
'  '    Return _TacksLossBenchmarks
'  '  End Get
'  '  Set(value As clsManoeuverBenchmarks)
'  '    _TacksLossBenchmarks = value
'  '    OnPropertyChanged("TacksLossBenchmarks")
'  '  End Set
'  'End Property
'  'Public Property GybesLossBenchmarks As clsManoeuverBenchmarks
'  '  Get
'  '    Return _GybesLossBenchmarks
'  '  End Get
'  '  Set(value As clsManoeuverBenchmarks)
'  '    _GybesLossBenchmarks = value
'  '    OnPropertyChanged("GybesLossBenchmarks")
'  '  End Set
'  'End Property

'  <JsonIgnore>
'  Public Property Lista As List(Of clsManoeuverBenchmarks)
'    Get
'      If _Lista.Count = 0 Then
'        _Lista.Add(TacksLossBenchmarks)
'        _Lista.Add(GybesLossBenchmarks)
'      End If
'      Return _Lista
'    End Get
'    Set(value As List(Of clsManoeuverBenchmarks))
'      _Lista = value
'      'OnPropertyChanged("Lista")
'    End Set
'  End Property

'  Public Sub CaricaDefault()
'    TacksLossBenchmarks = New clsManoeuverBenchmarks(clsPeriod2021.ePeriodType.eTack)
'    GybesLossBenchmarks = New clsManoeuverBenchmarks(clsPeriod2021.ePeriodType.eGybe)
'  End Sub


'End Class

'Public Class clsManoeuverBenchmarks
'  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation
'  Dim _ManoeuversType As clsPeriod2021.ePeriodType
'  'Dim _ListaValori As New List(Of clsDoubleXY)
'  Dim _ListaValoriLoss As New List(Of clsLossDetails)

'  Public Property ManoeuversType As clsPeriod2021.ePeriodType
'    Get
'      Return _ManoeuversType
'    End Get
'    Set(value As clsPeriod2021.ePeriodType)
'      _ManoeuversType = value
'    End Set
'  End Property

'  Public ReadOnly Property Descrizione As String
'    Get
'      Select Case ManoeuversType
'        Case clsPeriod2021.ePeriodType.eTack
'          Return "Tack Loss Entry to Exit [ss]"
'        Case clsPeriod2021.ePeriodType.eGybe
'          Return "Gybe Loss Entry to Exit [ss]"
'        Case Else
'          Return "Error"
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property Colore As Color
'    Get
'      Select Case ManoeuversType
'        Case clsPeriod2021.ePeriodType.eTack
'          Return Colors.DarkRed
'        Case clsPeriod2021.ePeriodType.eGybe
'          Return Colors.Blue
'        Case Else
'          Return Colors.Black
'      End Select
'    End Get
'  End Property



'  'Public Property ListaValori As List(Of clsDoubleXY)
'  '  Get
'  '    Return _ListaValori
'  '  End Get
'  '  Set(value As List(Of clsDoubleXY))
'  '    _ListaValori = value
'  '  End Set
'  'End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkTws As Double()
'    Get
'      Return ListaValoriLoss.Select(Function(x) x.Tws).ToArray
'    End Get
'  End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkSecGain As Double()
'    Get
'      Return ListaValoriLoss.Select(Function(x) x.GainSec).ToArray
'    End Get
'  End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkVmgLoss As Double()
'    Get
'      Return ListaValoriLoss.Select(Function(x) x.VmgAvg).ToArray
'    End Get
'  End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkMetersLoss As Double()
'    Get
'      Return ListaValoriLoss.Select(Function(x) x.GainMeters).ToArray
'    End Get
'  End Property

'  Public Property ListaValoriLoss As List(Of clsLossDetails)
'    Get
'      Return _ListaValoriLoss
'    End Get
'    Set(value As List(Of clsLossDetails))
'      _ListaValoriLoss = value
'    End Set
'  End Property

'  Public Function GetValue(Tws As Double) As Double
'    Return 0
'    _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(BenchmarkTws, BenchmarkSecGain)
'    Dim Int1 As Double = _Interpolatore.Interpolate(Tws)
'    Return Int1
'  End Function

'  Public Sub New()

'  End Sub
'  'Public Sub New(ManoeuversType As clsPeriod2021.ePeriodType)
'  '  _ManoeuversType = ManoeuversType
'  '  ListaValori.Clear()
'  '  If ManoeuversType = clsPeriod2021.ePeriodType.eTack Then
'  '    ListaValori.Add(New clsDoubleXY(6, -25))
'  '    ListaValori.Add(New clsDoubleXY(8, -17))
'  '    ListaValori.Add(New clsDoubleXY(10, -13))
'  '    ListaValori.Add(New clsDoubleXY(12, -10))
'  '    ListaValori.Add(New clsDoubleXY(15, -8))
'  '    ListaValori.Add(New clsDoubleXY(20, -7))
'  '    ListaValori.Add(New clsDoubleXY(25, -6))
'  '  ElseIf ManoeuversType = clsPeriod2021.ePeriodType.eGybe Then
'  '    ListaValori.Add(New clsDoubleXY(6, -28))
'  '    ListaValori.Add(New clsDoubleXY(8, -20))
'  '    ListaValori.Add(New clsDoubleXY(10, -13))
'  '    ListaValori.Add(New clsDoubleXY(12, -9))
'  '    ListaValori.Add(New clsDoubleXY(15, -6))
'  '    ListaValori.Add(New clsDoubleXY(20, -4))
'  '    ListaValori.Add(New clsDoubleXY(25, -3))
'  '  End If

'  'End Sub

'  Public Sub New(ManoeuversType As clsPeriod2021.ePeriodType) ', Nuova As Boolean)
'    _ManoeuversType = ManoeuversType
'    ListaValoriLoss.Clear()
'    If ManoeuversType = clsPeriod2021.ePeriodType.eTack Then
'      ListaValoriLoss.Add(New clsLossDetails(6, -15, 7, 0))
'      ListaValoriLoss.Add(New clsLossDetails(7, -7, 8, 0))
'      ListaValoriLoss.Add(New clsLossDetails(8, -7, 9, 0))
'      ListaValoriLoss.Add(New clsLossDetails(10, -6.7, 13, 0))
'      ListaValoriLoss.Add(New clsLossDetails(12, -5.0, 16.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(14, -5.0, 19.0, 0))
'      ListaValoriLoss.Add(New clsLossDetails(16, -8, 21.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(18, -8, 22.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(20, -7, 23.0, 0))
'      ListaValoriLoss.Add(New clsLossDetails(22, -6, 23.2, 0))
'      ListaValoriLoss.Add(New clsLossDetails(25, -6, 23.3, 0))
'    ElseIf ManoeuversType = clsPeriod2021.ePeriodType.eGybe Then
'      ListaValoriLoss.Add(New clsLossDetails(6, -28, 4, 0))
'      ListaValoriLoss.Add(New clsLossDetails(7, -28, 7, 0))
'      ListaValoriLoss.Add(New clsLossDetails(8, -20, 10, 0))
'      ListaValoriLoss.Add(New clsLossDetails(10, -13, 17.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(12, -9, 22, 0))
'      ListaValoriLoss.Add(New clsLossDetails(14, -6, 25.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(16, -6, 28.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(18, -6, 30.5, 0))
'      ListaValoriLoss.Add(New clsLossDetails(20, -4, 32, 0))
'      ListaValoriLoss.Add(New clsLossDetails(22, -6, 33.3, 0))
'      ListaValoriLoss.Add(New clsLossDetails(25, -3, 34.5, 0))
'    End If

'  End Sub


'End Class

'Public Class clsLossDetails
'  Dim _Tws As Double
'  Dim _GainSec As Double
'  Dim _GainMeters As Double
'  Dim _VmgAvg As Double

'  Public Sub New(Tws As Double, GainSec As Double, VmgAvg As Double, GainMeters As Double)
'    _Tws = Tws
'    _GainSec = GainSec
'    _VmgAvg = VmgAvg
'    _GainMeters = GainMeters
'  End Sub

'  Public Property Tws As Double
'    Get
'      Return _Tws
'    End Get
'    Set(value As Double)
'      _Tws = value
'    End Set
'  End Property

'  Public Property GainSec As Double
'    Get
'      Return _GainSec
'    End Get
'    Set(value As Double)
'      _GainSec = value
'    End Set
'  End Property

'  Public Property VmgAvg As Double
'    Get
'      Return _VmgAvg
'    End Get
'    Set(value As Double)
'      _VmgAvg = value
'    End Set
'  End Property

'  Public Property GainMeters As Double
'    Get
'      Return _GainMeters
'    End Get
'    Set(value As Double)
'      _GainMeters = value
'    End Set
'  End Property
'End Class

'Public Class clsAccelerationBenchmarks
'  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation
'  Dim _ListaValori As New List(Of clsDoubleXY)

'  Public ReadOnly Property Descrizione As String
'    Get
'      Return "Tack Loss Entry to Exit [ss]"
'    End Get
'  End Property

'  Public ReadOnly Property Colore As Color
'    Get
'      Return Colors.Black
'    End Get
'  End Property


'  Public Property ListaValori As List(Of clsDoubleXY)
'    Get
'      Return _ListaValori
'    End Get
'    Set(value As List(Of clsDoubleXY))
'      _ListaValori = value
'    End Set
'  End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkTws As Double()
'    Get
'      Return ListaValori.Select(Function(x) x.X).ToArray
'    End Get
'  End Property

'  <JsonIgnore>
'  Public ReadOnly Property BenchmarkTimeToTakeOffSpeed As Double()
'    Get
'      Return ListaValori.Select(Function(x) x.Y).ToArray
'    End Get
'  End Property


'  Public Function GetValue(Tws As Double) As Double
'    Return 0
'    _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(BenchmarkTws, BenchmarkTimeToTakeOffSpeed)
'    Dim Int1 As Double = _Interpolatore.Interpolate(Tws)
'    Return Int1
'  End Function

'  Public Sub New()

'  End Sub

'  Public Sub CaricaDefault()
'    ListaValori.Clear()
'    ListaValori.Add(New clsDoubleXY(6, 60))
'    ListaValori.Add(New clsDoubleXY(8, 40))
'    ListaValori.Add(New clsDoubleXY(10, 28))
'    ListaValori.Add(New clsDoubleXY(12, 20))
'    ListaValori.Add(New clsDoubleXY(14, 14))
'    ListaValori.Add(New clsDoubleXY(16, 11))
'    ListaValori.Add(New clsDoubleXY(18, 10))
'    ListaValori.Add(New clsDoubleXY(20, 10))
'    ListaValori.Add(New clsDoubleXY(22, 10))
'    ListaValori.Add(New clsDoubleXY(25, 10))
'  End Sub


'End Class

'Public Class clsPolinomial
'  Dim _Coeff As Double()
'  '0 termine noto, 1 primo grado

'  Public Property Coeff As Double()
'    Get
'      Return _Coeff
'    End Get
'    Set(value As Double())
'      _Coeff = value
'    End Set
'  End Property


'  Public Function Y(X As Double) As Double
'    If Coeff.Count = 0 Then Return 0
'    Dim yTmp As Double = 0
'    For i As Integer = 0 To Coeff.Count
'      yTmp += Coeff(i) * X ^ i
'    Next
'    Return yTmp
'  End Function


'End Class

