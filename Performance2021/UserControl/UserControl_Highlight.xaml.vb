Imports System.ComponentModel
Imports System.Globalization
Imports System.IO.Pipes
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices.WindowsRuntime
Imports System.Security.AccessControl
Imports System.Text.RegularExpressions
'Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data
Imports OfficeOpenXml
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.Database
Imports PdfSharp.Pdf.Content.Objects
Imports PropertyChanged
Imports SciChart.Charting.Visuals.Axes
Imports ScottPlot.Plottables
Imports ScottPlot.WPF
Imports SharpKml.Dom
Imports SPwpf.clsChannelAdvanced
Imports SPwpf.clsTimeRangeReportUtility
Imports SPwpf.SummaryReport
Imports Thrift.Protocol



<AddINotifyPropertyChangedInterface>
Public Class DecimalsFromElementoConverter : Implements IMultiValueConverter
  ' values(0)=Double cella, values(1)=Elemento riga
  Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object _
      Implements IMultiValueConverter.Convert
    If values Is Nothing OrElse values.Length < 2 OrElse values(0) Is Nothing OrElse values(1) Is Nothing Then
      Return DependencyProperty.UnsetValue
    End If
    Dim d As Double
    If TypeOf values(0) Is Double Then d = CDbl(values(0)) Else If Not Double.TryParse(values(0).ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return values(0)
    Dim el = TryCast(values(1), clsChannel2020)
    Dim dec = If(el IsNot Nothing, el.Decimals, 2)
    Return d.ToString("N" & dec, culture)
  End Function

  Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() _
      Implements IMultiValueConverter.ConvertBack
    ' Solo visualizzazione; per editing implementare qui il parsing
    Return New Object() {Binding.DoNothing, Binding.DoNothing}
  End Function
End Class

<AddINotifyPropertyChangedInterface>
Public Class BooleanToFontWeightConverter : Implements IValueConverter
  Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object _
        Implements IValueConverter.Convert
    Dim b = DirectCast(value, Boolean?)
    Return If(b.HasValue AndAlso b.Value, FontWeights.Bold, FontWeights.Normal)
  End Function
  Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object _
        Implements IValueConverter.ConvertBack
    Return Object.Equals(value, FontWeights.Bold)
  End Function
End Class

Public Class UserControl_Highlight
  Dim VM As New HighlightViewModel

  Public Sub New()
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.AggiornaSettaggiIniziali()
    VM.MakeReport(DataPlotSync.VisibleRange)
    AppConfig.Salva()
  End Sub

  'Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
  '  VM.AggiornaSettaggiIniziali()
  '  Dim Periodi = PeriodsManager.ListaStraightLineVmg.Where(Function(p) p.TR.IsOverlapped(DataPlotSync.VisibleRange)).ToList
  '  VM.MakeReport(Periodi)
  '  AppConfig.Salva()
  'End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    VM.SelectUpwindChannels()
  End Sub

  Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
    VM.SelectFilterChannel
  End Sub

  Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)
    VM.SelectDownwindChannels()
  End Sub



  Private Sub TextBox_PreviewTextInputByte(sender As Object, e As TextCompositionEventArgs)
    Dim Rx As Regex = New Regex("[^0-9]+-")
    Dim IsNumeric As Boolean = Rx.IsMatch(e.Text)
    e.Handled = IsNumeric AndAlso Double.Parse(e.Text) <= 255
  End Sub

  Private Sub UserControl_GotFocus(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If VM.CanaleFiltro Is Nothing Then
      VM.AggiornaSettaggiIniziali()
    End If
  End Sub

  Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
    If DataProvider2020 Is Nothing Then Exit Sub
    If VM.CanaleFiltro Is Nothing Then
      VM.AggiornaSettaggiIniziali()
    End If
  End Sub

  Private Sub DataGrid_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs)
    Dim name = e.PropertyName
    Dim t = TryCast(e.PropertyDescriptor, PropertyDescriptor)?.PropertyType

    If name = "Channel" Then
      e.Column = New DataGridTextColumn With {
          .Header = "Channel Name",
          .Binding = New Binding("[Channel].LongName")
      }
      Return
    End If

    If t Is GetType(Double) Then
      Dim txtFactory = New FrameworkElementFactory(GetType(TextBlock))
      Dim mb As New MultiBinding With {.Converter = CType(FindResource("FmtDec"), DecimalsFromElementoConverter)}
      mb.Bindings.Add(New Binding("[" & name & "]")) ' valore double
      mb.Bindings.Add(New Binding("[Channel]"))     ' oggetto riga
      txtFactory.SetBinding(TextBlock.TextProperty, mb)
      e.Column = New DataGridTemplateColumn With {.Header = name, .CellTemplate = New DataTemplate With {.VisualTree = txtFactory}}
    End If
  End Sub

  Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)
    VM.StampaPdf()
  End Sub
End Class


Public Enum RowKind
  Header
  Empty
  Data
End Enum

<AddINotifyPropertyChangedInterface>
Public MustInherit Class RowBase
  Public MustOverride ReadOnly Property Kind As RowKind
End Class

' Intestazione che occupa tutta la riga
<AddINotifyPropertyChangedInterface>
Public Class HeaderRow
  Inherits RowBase
  Public Property Title As String
  Public Overrides ReadOnly Property Kind As RowKind
    Get
      Return RowKind.Header
    End Get
  End Property
End Class


' Riga vuota (spacer)
<AddINotifyPropertyChangedInterface>
Public Class EmptyRow
  Inherits RowBase
  Public Property Height As Double = 8
  Public Overrides ReadOnly Property Kind As RowKind
    Get
      Return RowKind.Empty
    End Get
  End Property
End Class

' Cella generica
<AddINotifyPropertyChangedInterface>
Public Class Cell

  Private _value As String
  Private _width As Integer = 100
  Private _borderThikness As Integer = 0
  Private _background As Brush = Brushes.White
  Private _fontSize As Double = 16
  Private _isBold As Boolean = False
  Private _isReadOnly As Boolean = False
  Private _hCAlign As HorizontalAlignment = HorizontalAlignment.Right

  Public Property Value As String
    Get
      Return _value
    End Get
    Set(value As String)
      If _value <> value Then _value = value
    End Set
  End Property

  Public Property Background As Brush
    Get
      Return _background
    End Get
    Set(value As Brush)
      If _background IsNot value Then _background = value
    End Set
  End Property

  Public Property HCAlign As HorizontalAlignment
    Get
      Return _hCAlign
    End Get
    Set(value As HorizontalAlignment)
      If _hCAlign = value Then _hCAlign = value
    End Set
  End Property

  Public Property FontSize As Double
    Get
      Return _fontSize
    End Get
    Set(value As Double)
      If _fontSize <> value Then _fontSize = value
    End Set
  End Property

  Public Property IsBold As Boolean
    Get
      Return _isBold
    End Get
    Set(value As Boolean)
      If _isBold <> value Then _isBold = value
    End Set
  End Property

  Public Property Width As Integer
    Get
      Return _width
    End Get
    Set(value As Integer)
      If _width <> value Then _width = value
    End Set
  End Property

  Public Property BorderThikness As Integer
    Get
      Return _borderThikness
    End Get
    Set(value As Integer)
      If _borderThikness <> value Then _borderThikness = value
    End Set
  End Property

  Public Property IsReadOnly As Boolean
    Get
      Return _isReadOnly
    End Get
    Set(value As Boolean)
      If _isReadOnly <> value Then _isReadOnly = value
    End Set
  End Property

End Class
' Riga dati con numero di celle variabile (non incolonnate)
<AddINotifyPropertyChangedInterface>
Public Class DataRow
  Inherits RowBase
  Public Property Cells As Collections.ObjectModel.ObservableCollection(Of Cell)
  Public Overrides ReadOnly Property Kind As RowKind
    Get
      Return RowKind.Data
    End Get
  End Property
End Class

'' ViewModel
'Public Class MainVM
'  Public Property Rows As Collections.ObjectModel.ObservableCollection(Of RowBase)

'  Public Sub FillGeneralInfoTable()
'    Rows = New Collections.ObjectModel.ObservableCollection(Of RowBase) From {
'            New HeaderRow With {.Title = "Sezione A"},
'            New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell) From {
'                New Cell With {.Value = "Codice A001"},
'                New Cell With {.Value = "Descrizione lunga dell'articolo A"},
'                New Cell With {.Value = "Q.tà: 10"}
'            }},
'            New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell) From {
'                New Cell With {.Value = "Solo una cella in questa riga"}
'            }},
'            New EmptyRow With {.Height = 12},
'            New HeaderRow With {.Title = "Sezione B"},
'            New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell) From {
'                New Cell With {.Value = "B002"},
'                New Cell With {.Value = "Articolo B"},
'                New Cell With {.Value = "Q.tà: 5"},
'                New Cell With {.Value = "Note: urgente"}
'            }}
'        }
'  End Sub
'End Class


<AddINotifyPropertyChangedInterface>
Public Class HighlightViewModel
  Public Property ListaCanaliUp As New List(Of clsChannelAdvanced)
  Public Property ListaCanaliDn As New List(Of clsChannelAdvanced)
  Public Property CanaleFiltro As clsChannel2020
  Public Property VminFiltro As Double
  Public Property VmaxFiltro As Double
  Public Property TabellaDatiUp As New System.Data.DataTable
  Public Property TabellaDatiUpPort As New System.Data.DataTable
  Public Property TabellaDatiUpStbd As New System.Data.DataTable
  Public Property TabellaDatiDn As New System.Data.DataTable
  Public Property TabellaDatiDnPort As New System.Data.DataTable
  Public Property TabellaDatiDnStbd As New System.Data.DataTable

  Public Property GeneralTableRows As Collections.ObjectModel.ObservableCollection(Of RowBase)
  'Public Property TimeRangeRowsDetails As Dictionary(Of clsTimeRangeReportUtility.RowStatus, List(Of Integer))

  Public PlotAvg As WpfPlot
  Public PlotSD As WpfPlot
  Public PlotPortAvg As WpfPlot
  Public PlotPortSD As WpfPlot
  Public PlotStbdAvg As WpfPlot
  Public PlotStbdSD As WpfPlot


  Dim RigheSopraSoglia As New Dictionary(Of Integer, Boolean) 'id riga, IsStbd
  Dim RigheSottoSoglia As New Dictionary(Of Integer, Boolean) 'id riga, IsStbd
  Dim CanaleTwa As clsChannel2020
  Dim CanaleTws As clsChannel2020

  Public Sub New()
  End Sub


  Public Sub AggiornaSettaggiIniziali()
    'AppConfig.ActiveProfile.SummaryHighlightSettings = Nothing
    If AppConfig.ActiveProfile.SummaryHighlightSettings Is Nothing Then
      AppConfig.ActiveProfile.SummaryHighlightSettings = New clsSummaryHighlightSettings
      AppConfig.ActiveProfile.SummaryHighlightSettings.CanaleFiltro = "VmgPercInertial"
      AppConfig.ActiveProfile.SummaryHighlightSettings.FiltroMinVal = 96
      AppConfig.ActiveProfile.SummaryHighlightSettings.FiltroMaxVal = 120

      Dim cidup As New List(Of String)
      cidup.Add("SeaState")
      cidup.Add("VmgPercInertial")
      cidup.Add("Vmg")
      cidup.Add("TWS")
      cidup.Add("TWD")
      cidup.Add("SOW")
      cidup.Add("TWA")
      cidup.Add("AWA")
      cidup.Add("AWS")
      cidup.Add("Vang")
      cidup.Add("HeelNorm")
      cidup.Add("TrimNorm")
      cidup.Add("RdrNorm")
      cidup.Add("Forestay")
      cidup.Add("Rake")

      Dim ciddn As New List(Of String)
      ciddn.Add("VmgPercInertial")
      ciddn.Add("Vmg")
      ciddn.Add("TWS")
      ciddn.Add("TWD")
      ciddn.Add("SOW")
      ciddn.Add("TWA")
      ciddn.Add("AWA")
      ciddn.Add("AWS")
      ciddn.Add("Vang")
      ciddn.Add("HeelNorm")
      ciddn.Add("TrimNorm")
      ciddn.Add("RdrNorm")
      ciddn.Add("Forestay")
      ciddn.Add("Rake")

      AppConfig.ActiveProfile.SummaryHighlightSettings.ListaUpwindCannelsId = cidup
      AppConfig.ActiveProfile.SummaryHighlightSettings.ListaDownwindCannelsId = ciddn
      AppConfig.Salva()
    End If

    If AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels Is Nothing OrElse AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels.Count = 0 Then
      AggiornaCanaliUp()
    End If

    If AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels Is Nothing OrElse AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels.Count = 0 Then
      AggiornaCanaliDn()
    End If

    ImpostazioniIniziali()
  End Sub

  Public Sub AggiornaCanaliUp()
    Dim Canali As New List(Of clsChannel2020)
    For Each cid As String In AppConfig.ActiveProfile.SummaryHighlightSettings.ListaUpwindCannelsId
      Dim c As clsChannel2020 = DataProvider2020.Channels.Canale(cid)
      If Not c Is Nothing Then
        Canali.Add(c)
      End If
    Next
    AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels = Canali
  End Sub

  Public Sub AggiornaCanaliDn()
    Dim Canali As New List(Of clsChannel2020)
    For Each cid As String In AppConfig.ActiveProfile.SummaryHighlightSettings.ListaDownwindCannelsId
      Dim c As clsChannel2020 = DataProvider2020.Channels.Canale(cid)
      If Not c Is Nothing Then
        Canali.Add(c)
      End If
    Next
    AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels = Canali
  End Sub

  Public Sub AggiornaUpwindChannelsId()
    Dim Canali As New List(Of String)
    For Each c In AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels
      Canali.Add(c.ChannelId)
    Next
    AppConfig.ActiveProfile.SummaryHighlightSettings.ListaUpwindCannelsId = Canali
  End Sub

  Public Sub AggiornaDownwindChannelsId()
    Dim Canali As New List(Of String)
    For Each c In AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels
      Canali.Add(c.ChannelId)
    Next
    AppConfig.ActiveProfile.SummaryHighlightSettings.ListaDownwindCannelsId = Canali
  End Sub

  Public Sub SelectUpwindChannels()
    Dim Canali As List(Of String) = AppConfig.ActiveProfile.SummaryHighlightSettings.ListaUpwindCannelsId
    Dim Res = GestisciListaCanaliDaStringaToChannel("Select Channels", DataProvider2020.Channels.ListaCanali.ToList, Canali, eSelectChannelType.eTwin)
    If Not Res Is Nothing Then
      AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels = Res.ToList
      AggiornaUpwindChannelsId()
      AppConfig.Salva()
    End If
  End Sub

  Public Sub SelectDownwindChannels()
    Dim Canali As List(Of String) = AppConfig.ActiveProfile.SummaryHighlightSettings.ListaDownwindCannelsId
    Dim Res = GestisciListaCanaliDaStringaToChannel("Select Downwind Channels", DataProvider2020.Channels.ListaCanali.ToList, Canali, eSelectChannelType.eTwin)
    If Not Res Is Nothing Then
      AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels = Res.ToList
      AggiornaDownwindChannelsId()
      AppConfig.Salva()
    End If
  End Sub

  Public Sub SelectFilterChannel()
    Dim l As New List(Of String)
    l.Add(AppConfig.ActiveProfile.SummaryHighlightSettings.CanaleFiltro)
    Dim Res = GestisciListaCanali("Select Filter Channel", DataProvider2020.Channels.ListaCanali.ToList, l, eSelectChannelType.eSingle)
    If Not Res Is Nothing Then
      AppConfig.ActiveProfile.SummaryHighlightSettings.CanaleFiltro = Res.FirstOrDefault
      CanaleFiltro = DataProvider2020.Channels.Canale(AppConfig.ActiveProfile.SummaryHighlightSettings.CanaleFiltro)
      AppConfig.Salva()
    End If
  End Sub



  Public Sub ImpostazioniIniziali()
    CanaleTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    CanaleTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    CanaleFiltro = DataProvider2020.Channels.Canale(AppConfig.ActiveProfile.SummaryHighlightSettings.CanaleFiltro)
    VminFiltro = AppConfig.ActiveProfile.SummaryHighlightSettings.FiltroMinVal
    VmaxFiltro = AppConfig.ActiveProfile.SummaryHighlightSettings.FiltroMaxVal

    ListaCanaliUp.Clear()
    For Each C In AppConfig.ActiveProfile.SummaryHighlightSettings.UpChannels
      ListaCanaliUp.Add(New clsChannelAdvanced(C))
    Next
    ListaCanaliDn.Clear()
    For Each C In AppConfig.ActiveProfile.SummaryHighlightSettings.DnChannels
      ListaCanaliDn.Add(New clsChannelAdvanced(C))
    Next
  End Sub


  Public Sub AzzeraRighe()
    RigheSopraSoglia.Clear()
    RigheSottoSoglia.Clear()
  End Sub



  'Public Sub AggiornaValoriBestWorst()
  '  AggiornaValoriBestWorst(ListaCanaliUp)
  '  AggiornaValoriBestWorst(ListaCanaliDn)
  'End Sub

  Public Sub AggiornaValoriBestWorst(Lista As List(Of clsChannelAdvanced))
    AggiornaValoriBestWorst(clsChannelAdvanced.eValori.eBoth, Lista)
    AggiornaValoriBestWorst(clsChannelAdvanced.eValori.ePort, Lista)
    AggiornaValoriBestWorst(clsChannelAdvanced.eValori.eStbd, Lista)
  End Sub


  Public Sub RiempiValori(TR As clsTimeRange)
    'TimeRangeRowsDetails = clsTimeRangeReportUtility.SetSummaryReport(TR)
    clsTimeRangeReportUtility.SetSummaryReport(TR)
    AzzeraRighe()
    Dim VmgRows As New List(Of Integer)
    VmgRows.AddRange(UpwindVmgIndexes)
    VmgRows.AddRange(DownwindVmgIndexes)
    'Dim IndiciUpDn = clsTimeRangeReportUtility.RowsIndexes.Where(Function(x) x.Key = clsTimeRangeReportUtility.RowStatus.DownWindVmg Or x.Key = clsTimeRangeReportUtility.RowStatus.UpWindVmg).ToList
    'For Each r In IndiciUpDn
    '  VmgRows.AddRange(r.Value)
    'Next

    For Each id In VmgRows
      RiempiRigaValida(id)
    Next

    RiempiCanaliAvanzati(ListaCanaliUp)
    RiempiCanaliAvanzati(ListaCanaliDn)

    Statistiche()

  End Sub

  Public Sub Statistiche()

    Dim Rows = clsTimeRangeReportUtility.Rows

    Dim ts = Rows.Last.DT.Subtract(Rows.First.DT)
    Dim formatted As String = $"{CInt(ts.Hours):00}:{ts.Minutes:00}"
    Console.WriteLine("Rows: " & Rows.Count)
    Console.WriteLine("Time: " & formatted)

    Console.WriteLine("UpVmg Time: " & ((UpwindVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("Tacking Time: " & ((TackingIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("Tacks: " & Tacks)
    Console.WriteLine("xTack: " & ((TackingIndexes.Count / Tacks)).ToString("F0"))
    Console.WriteLine("DnVmg Time: " & ((DownwindVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("Gybing Time: " & ((GybingIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("Gybess: " & Gybes)
    Console.WriteLine("xGybe: " & ((GybingIndexes.Count / Gybes)).ToString("F0"))
    Console.WriteLine("Undefined Time: " & ((UndefynedIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("UpVmgUnderMinPerf Time: " & ((UnderMinPerfUpVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")
    Console.WriteLine("DnVmgUnderMinPerf Time: " & ((UnderMinPerfDnVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%")



    Dim chP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVmgInertial)
    Dim chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)

    Dim sopraUp = ListaCanaliUp.First.ValoriBoth.ValoriSopraSoglia.ToList
    Dim sottoUp = ListaCanaliUp.First.ValoriBoth.ValoriSottoSoglia.ToList

    Console.WriteLine("Upwind Vmg above 96% performance: " & ((sopraUp.Count / (sopraUp.Count + sottoUp.Count)) * 100).ToString("F0") & "%")
    Console.WriteLine("Upwind Vmg below 96% performance: " & ((sottoUp.Count / (sopraUp.Count + sottoUp.Count)) * 100).ToString("F0") & "%")

    Dim sopraDn = ListaCanaliDn.First.ValoriBoth.ValoriSopraSoglia
    Dim sottoDn = ListaCanaliDn.First.ValoriBoth.ValoriSottoSoglia
    Console.WriteLine("Downwind Vmg above 96% performance: " & ((sopraDn.Count / (sopraDn.Count + sottoDn.Count)) * 100).ToString("F0") & "%")
    Console.WriteLine("Downwind Vmg below 96% performance: " & ((sottoDn.Count / (sopraDn.Count + sottoDn.Count)) * 100).ToString("F0") & "%")

    Dim VmgWhileTacking As New List(Of Double)
    For Each r In clsTimeRangeReportUtility.TackingIndexes.ToList
      Dim v = chP.Valori(r)
      If Not Double.IsNaN(v) Then
        VmgWhileTacking.Add(v)
      End If
    Next
    Dim vmgtacking = VmgWhileTacking.Average

    Dim VmgWhileGybing As New List(Of Double)
    For Each r In clsTimeRangeReportUtility.GybingIndexes.ToList
      Dim v = chP.Valori(r)
      If Not Double.IsNaN(v) Then
        VmgWhileGybing.Add(v)
      End If
    Next
    Dim vmggybing = VmgWhileGybing.Average

    Dim IndiciVmgUpSopraSoglia = sopraUp.Select(Function(x) x.Id).ToList
    Dim VmgWhileUpSopraSoglia As New List(Of Double)
    Dim BsWhileUpSopraSoglia As New List(Of Double)
    For Each r In IndiciVmgUpSopraSoglia
      Dim v = chP.Valori(r)
      Dim b = chBs.Valori(r)
      If Not Double.IsNaN(v) Then
        VmgWhileUpSopraSoglia.Add(v)
      End If
      If Not Double.IsNaN(b) Then
        BsWhileUpSopraSoglia.Add(b)
      End If
    Next
    Dim vmgupsoprasoglia = VmgWhileUpSopraSoglia.Average
    Dim bsupsoprasoglia = BsWhileUpSopraSoglia.Average

    Dim IndiciVmgDnSopraSoglia = sopraDn.Select(Function(x) x.Id).ToList
    Dim VmgWhileDnSopraSoglia As New List(Of Double)
    Dim BsWhileDnSopraSoglia As New List(Of Double)
    For Each r In IndiciVmgDnSopraSoglia
      Dim v = chP.Valori(r)
      Dim b = chBs.Valori(r)
      If Not Double.IsNaN(v) Then
        VmgWhileDnSopraSoglia.Add(v)
      End If
      If Not Double.IsNaN(b) Then
        BsWhileDnSopraSoglia.Add(b)
      End If
    Next
    Dim vmgdnsoprasoglia = VmgWhileDnSopraSoglia.Average
    Dim bsdnsoprasoglia = BsWhileDnSopraSoglia.Average

    Dim tacksVmgMetersLoss = (vmgupsoprasoglia - vmgtacking) / 3600 * 1852 * VmgWhileTacking.Count / DataProvider2020.Hz
    Dim tacksSecondsLoss = tacksVmgMetersLoss / (vmgupsoprasoglia / 3600 * 1852)
    Dim tackSecondsLoss = tacksSecondsLoss / clsTimeRangeReportUtility.Tacks
    Dim bsupms = bsupsoprasoglia / 3600 * 1852
    Console.WriteLine("Tacks Total Loss [ss]: " & tacksSecondsLoss.ToString("F0"))
    Console.WriteLine("Single Tack Loss [ss]: " & tackSecondsLoss.ToString("F0"))
    Console.WriteLine("Tacks In line Total Loss[mt]: " & (tacksSecondsLoss * bsupms).ToString("F0"))
    Console.WriteLine("Single Tack In Line Loss [mt]: " & (tackSecondsLoss * bsupms).ToString("F0"))

    Dim gybesVmgMetersLoss = (vmgdnsoprasoglia - vmggybing) / 3600 * 1852 * VmgWhileGybing.Count / DataProvider2020.Hz
    Dim gybesSecondsLoss = gybesVmgMetersLoss / (vmgdnsoprasoglia / 3600 * 1852)
    Dim gybeSecondsLoss = gybesSecondsLoss / clsTimeRangeReportUtility.Gybes
    Dim bsdnms = bsupsoprasoglia / 3600 * 1852
    Console.WriteLine("Gybes Total Loss [ss]: " & gybesSecondsLoss.ToString("F0"))
    Console.WriteLine("Single Gybe Loss [ss]: " & gybeSecondsLoss.ToString("F0"))
    Console.WriteLine("Gybes In line Total Loss[mt]: " & (gybesSecondsLoss * bsdnms).ToString("F0"))
    Console.WriteLine("Single Gybe In Line Loss [mt]: " & (gybeSecondsLoss * bsdnms).ToString("F0"))

    FillGeneralInfoTable()

  End Sub

  Function NewCell(Value As String, Background As Brush, IsBold As Boolean, Whidth As Integer, FontSize As Integer, BorderThickness As Integer, IsReadOnly As Boolean, HAlign As HorizontalAlignment) As Cell
    Return New Cell With {.Value = Value, .Background = Background, .IsBold = IsBold, .Width = Whidth, .FontSize = FontSize, .BorderThikness = BorderThickness, .IsReadOnly = IsReadOnly, .HCAlign = HAlign}
  End Function

  Function NewDataCell(Value As String, Optional Width As Integer = 100, Optional FontSize As Integer = 14) As Cell
    Return NewCell(Value, Brushes.White, False, Width, FontSize, 0, True, HorizontalAlignment.Right)
  End Function

  Function NewDataCellHighlight(Value As String, Highlilight As Boolean, Alarm As Boolean, Optional Width As Integer = 100, Optional FontSize As Integer = 14) As Cell
    Return NewCell(Value, IIf(Alarm, Brushes.Red, IIf(Highlilight, Brushes.Yellow, Brushes.White)), False, Width, FontSize, 0, True, HorizontalAlignment.Right)
  End Function


  Function NewHeaderCell(Value As String, Optional Width As Integer = 100) As Cell
    Return NewCell(Value, Brushes.LightYellow, True, Width, 16, 1, True, HorizontalAlignment.Center)
  End Function

  Function VerticalSpacer(Height As Integer) As EmptyRow
    Return New EmptyRow With {.Height = Height}
  End Function

  Public Sub FillGeneralInfoTable()

    Dim Rows = clsTimeRangeReportUtility.Rows

    Dim ts = Rows.Last.DT.Subtract(Rows.First.DT)
    Dim formatted As String = $"{CInt(ts.Hours):00}:{ts.Minutes:00}:{ts.Seconds:00}"
    If GeneralTableRows Is Nothing Then
      GeneralTableRows = New Collections.ObjectModel.ObservableCollection(Of RowBase)
    Else
      GeneralTableRows.Clear()
    End If
    Dim H As HeaderRow
    Dim DR As DataRow
    Dim CW As Integer = 102

    Dim Start = clsTimeRangeReportUtility.SelectedStart
    If Not Start Is Nothing Then
      H = New HeaderRow With {.Title = Start.StartTime.ToString("dd MMM yyyy") & ": Start @ " & Start.StartTime.ToString("HH:mm:ss") & " (duration " & formatted & " )"}
      GeneralTableRows.Add(H)
      DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
      DR.Cells.Add(NewHeaderCell("Line Bias", 201))
      DR.Cells.Add(NewHeaderCell("Geometry", 201))
      DR.Cells.Add(NewHeaderCell("Boat @ Gun", 150))
      DR.Cells.Add(NewHeaderCell("Cross @ss"))
      DR.Cells.Add(NewHeaderCell("V Dist BL"))
      DR.Cells.Add(NewHeaderCell("Tws", 80))
      DR.Cells.Add(NewHeaderCell("Twd", 80))
      GeneralTableRows.Add(DR)

      DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
      DR.Cells.Add(NewDataCell(Start.StartBiasSimple, 201))
      DR.Cells.Add(NewDataCell(Start.LineGeometry, 201))
      DR.Cells.Add(NewDataCell(Start.ZoneBiasSimple, 150))
      DR.Cells.Add(NewDataCell(Start.CrossingSeconds.ToString("F1")))
      DR.Cells.Add(NewDataCell(Start.BoatDataAtZero.VdistBL.ToString("F1")))
      DR.Cells.Add(NewDataCell(Start.BoatDataAtZero.Tws.ToString("F1"), 80))
      DR.Cells.Add(NewDataCell(Start.BoatDataAtZero.Twd.ToString("F0"), 80))
      GeneralTableRows.Add(DR)
      GeneralTableRows.Add(VerticalSpacer(4))
      CW = 100
      DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
      DR.Cells.Add(NewHeaderCell("@Gun +2", (CW * 3) + 12))
      DR.Cells.Add(NewHeaderCell("First 30ss", (CW * 3) + 12))
      DR.Cells.Add(NewHeaderCell("First Minute", (CW * 3) + 12))
      GeneralTableRows.Add(DR)
      DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
      DR.Cells.Add(NewHeaderCell("Vmg%", CW))
      DR.Cells.Add(NewHeaderCell("Bs%", CW))
      DR.Cells.Add(NewHeaderCell("dTwa", CW))
      DR.Cells.Add(NewHeaderCell("Vmg%", CW))
      DR.Cells.Add(NewHeaderCell("Bs%", CW))
      DR.Cells.Add(NewHeaderCell("dTwa", CW))
      DR.Cells.Add(NewHeaderCell("Vmg%", CW))
      DR.Cells.Add(NewHeaderCell("Bs%", CW))
      DR.Cells.Add(NewHeaderCell("dTwa", CW))
      GeneralTableRows.Add(DR)
      DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
      DR.Cells.Add(NewDataCell(Start.BoatDataAtPlusTwo.VmgP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.BoatDataAtPlusTwo.BstP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.BoatDataAtPlusTwo.TwaD.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.First30secAverages.VmgP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.First30secAverages.BstP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.First30secAverages.TwaD.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.FirstMinuteAverages.VmgP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.FirstMinuteAverages.BstP.ToString("F0"), CW))
      DR.Cells.Add(NewDataCell(Start.FirstMinuteAverages.TwaD.ToString("F0"), CW))
      GeneralTableRows.Add(DR)
      GeneralTableRows.Add(VerticalSpacer(10))
    End If

    Dim chP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVmgInertial)
    Dim chIsStbd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eIsStbd)
    Dim chVmg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVmgInertial)
    Dim chVmgP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVmgPercInertial)
    Dim chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chBsP = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
    Dim chTwaD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)
    Dim chSeaState = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)
    If chSeaState Is Nothing Then
      chSeaState = DataProvider2020.CanaleDbl("SeaState")
    End If
    CW = 105
    H = New HeaderRow With {.Title = "Straight Line (Best = Performance above 96%)"}
    GeneralTableRows.Add(H)
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewHeaderCell("", CW))
    DR.Cells.Add(NewHeaderCell("RaceTime%", CW))
    DR.Cells.Add(NewHeaderCell("RaceTime", CW))
    DR.Cells.Add(NewHeaderCell("BestTm%", CW))
    DR.Cells.Add(NewHeaderCell("WorstTm%", CW))
    DR.Cells.Add(NewHeaderCell("WLoss BL[m]", CW))
    DR.Cells.Add(NewHeaderCell("WL(ss)", CW / 2))
    DR.Cells.Add(NewHeaderCell("SeaSt.", CW / 2))
    DR.Cells.Add(NewHeaderCell("Vmg%", CW / 2))
    DR.Cells.Add(NewHeaderCell("Bs%", CW / 2))
    DR.Cells.Add(NewHeaderCell("dTwa", CW / 2))
    GeneralTableRows.Add(DR)


    Dim RD = RigaDatiStraightLine("Upwind", ListaCanaliUp.First.ValoriBoth, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState)
    GeneralTableRows.Add(RD)
    RD = RigaDatiStraightLine("  Up Port", ListaCanaliUp.First.ValoriPort, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState, 11)
    GeneralTableRows.Add(RD)
    RD = RigaDatiStraightLine("  Up Stbd", ListaCanaliUp.First.ValoriStbd, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState, 11)
    GeneralTableRows.Add(RD)
    GeneralTableRows.Add(VerticalSpacer(4))
    RD = RigaDatiStraightLine("Downwind", ListaCanaliDn.First.ValoriBoth, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState)
    GeneralTableRows.Add(RD)
    RD = RigaDatiStraightLine("  Dn Port", ListaCanaliDn.First.ValoriPort, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState, 11)
    GeneralTableRows.Add(RD)
    RD = RigaDatiStraightLine("  Dn Stbd", ListaCanaliDn.First.ValoriStbd, CW, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd, chSeaState, 11)
    GeneralTableRows.Add(RD)

    GeneralTableRows.Add(VerticalSpacer(4))
    CW = 132
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewHeaderCell("WindStats", 106))
    DR.Cells.Add(NewHeaderCell("Tws", CW))
    DR.Cells.Add(NewHeaderCell("Tws Port", CW))
    DR.Cells.Add(NewHeaderCell("Tws Stbd", CW))
    DR.Cells.Add(NewHeaderCell("Twd", CW))
    DR.Cells.Add(NewHeaderCell("Twd Port", CW))
    DR.Cells.Add(NewHeaderCell("Twd Stbd", CW))
    GeneralTableRows.Add(DR)

    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell("Upwind", 106))
    DR.Cells.Add(NewDataCell(WindStatString(True, True, eValori.eBoth), CW))
    DR.Cells.Add(NewDataCell(WindStatString(True, True, eValori.ePort), CW))
    DR.Cells.Add(NewDataCell(WindStatString(True, True, eValori.eStbd), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, True, eValori.eBoth), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, True, eValori.ePort), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, True, eValori.eStbd), CW))
    GeneralTableRows.Add(DR)
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell("Downwind", 106))
    DR.Cells.Add(NewDataCell(WindStatString(True, False, eValori.eBoth), CW))
    DR.Cells.Add(NewDataCell(WindStatString(True, False, eValori.ePort), CW))
    DR.Cells.Add(NewDataCell(WindStatString(True, False, eValori.eStbd), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, False, eValori.eBoth), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, False, eValori.ePort), CW))
    DR.Cells.Add(NewDataCell(WindStatString(False, False, eValori.eStbd), CW))
    GeneralTableRows.Add(DR)


    GeneralTableRows.Add(VerticalSpacer(10))
    CW = 112
    H = New HeaderRow With {.Title = "Manoeuvers (loss relative to best sailing)"}
    GeneralTableRows.Add(H)
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewHeaderCell("", CW))
    DR.Cells.Add(NewHeaderCell("RaceTime%", CW))
    DR.Cells.Add(NewHeaderCell("RaceTime", CW))
    DR.Cells.Add(NewHeaderCell("Loss BL [m]", CW))
    DR.Cells.Add(NewHeaderCell("Loss [ss]", CW))
    DR.Cells.Add(NewHeaderCell("Total #", CW))
    DR.Cells.Add(NewHeaderCell("x1 Loss BL [m]", CW))
    DR.Cells.Add(NewHeaderCell("x1 Loss [ss]", CW))
    GeneralTableRows.Add(DR)


    RD = RigaDatiManovre("Tacks", TackingIndexes, ListaCanaliUp.First.ValoriBoth.IndiciSopraSoglia, CW, clsTimeRangeReportUtility.Tacks, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd)
    GeneralTableRows.Add(RD)
    RD = RigaDatiManovre("Gybes", GybingIndexes, ListaCanaliDn.First.ValoriBoth.IndiciSopraSoglia, CW, clsTimeRangeReportUtility.Gybes, chVmgP, chBsP, chTwaD, chVmg, chBs, chIsStbd)
    GeneralTableRows.Add(RD)

    Dim UnderMinPerfUpPortVmgIndexes As New List(Of Integer)
    Dim UnderMinPerfUpStbdVmgIndexes As New List(Of Integer)
    For Each i In UnderMinPerfUpVmgIndexes
      Dim IsS = chIsStbd.Valori(i)
      If Not Double.IsNaN(IsS) Then
        If IsS Then
          UnderMinPerfUpStbdVmgIndexes.Add(i)
        Else
          UnderMinPerfUpPortVmgIndexes.Add(i)
        End If
      End If
    Next

    Dim UnderMinPerfDnPortVmgIndexes As New List(Of Integer)
    Dim UnderMinPerfDnStbdVmgIndexes As New List(Of Integer)
    For Each i In UnderMinPerfDnVmgIndexes
      Dim IsS = chIsStbd.Valori(i)
      If Not Double.IsNaN(IsS) Then
        If IsS Then
          UnderMinPerfDnStbdVmgIndexes.Add(i)
        Else
          UnderMinPerfDnPortVmgIndexes.Add(i)
        End If
      End If
    Next

    Dim UndefynedPortIndexes As New List(Of Integer)
    Dim UndefynedStbdIndexes As New List(Of Integer)
    For Each i In UndefynedIndexes
      Dim IsS = chIsStbd.Valori(i)
      If Not Double.IsNaN(IsS) Then
        If IsS Then
          UndefynedPortIndexes.Add(i)
        Else
          UndefynedStbdIndexes.Add(i)
        End If
      End If
    Next

    GeneralTableRows.Add(VerticalSpacer(4))
    Dim TmpTotalSeconds As Double = UndefynedIndexes.Count / DataProvider2020.Hz
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell("Undefyned", CW, 11))
    DR.Cells.Add(NewDataCell(((UndefynedIndexes.Count / Rows.Count) * 100).ToString("F0") & "%", CW, 11))
    DR.Cells.Add(NewDataCell(New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UndefynedPortIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Port: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UndefynedStbdIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Stbd: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    GeneralTableRows.Add(DR)

    TmpTotalSeconds = UnderMinPerfUpVmgIndexes.Count / DataProvider2020.Hz
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell("Upwind <80%", CW, 11))
    DR.Cells.Add(NewDataCell(((UnderMinPerfUpVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%", CW, 11))
    DR.Cells.Add(NewDataCell(New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UnderMinPerfUpPortVmgIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Port: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UnderMinPerfUpStbdVmgIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Stbd: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    GeneralTableRows.Add(DR)

    TmpTotalSeconds = UnderMinPerfDnVmgIndexes.Count / DataProvider2020.Hz
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell("Downwind <80%", CW, 11))
    DR.Cells.Add(NewDataCell(((UnderMinPerfDnVmgIndexes.Count / Rows.Count) * 100).ToString("F0") & "%", CW, 11))
    DR.Cells.Add(NewDataCell(New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UnderMinPerfDnPortVmgIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Port: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    TmpTotalSeconds = UnderMinPerfDnStbdVmgIndexes.Count / DataProvider2020.Hz
    DR.Cells.Add(NewDataCell("Stbd: " & New DateTime().AddSeconds(TmpTotalSeconds).ToString("mm:ss"), CW, 11))
    GeneralTableRows.Add(DR)

    GeneralTableRows.Add(VerticalSpacer(4))
    CW = 70
    'DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    'DR.Cells.Add(NewDataCell("Best: perf >= 96% - Worst: perf < 96%", 600, 16))
    'GeneralTableRows.Add(DR)
    H = New HeaderRow With {.Title = "Upwind Channels"}
    GeneralTableRows.Add(H)
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewHeaderCell("", 150))
    DR.Cells.Add(NewHeaderCell("Tgt", CW))
    DR.Cells.Add(NewHeaderCell("Best", CW))
    DR.Cells.Add(NewHeaderCell("Worst", CW))
    DR.Cells.Add(NewHeaderCell("TgtSd", CW))
    DR.Cells.Add(NewHeaderCell("BestSd", CW))
    DR.Cells.Add(NewHeaderCell("WorstSd", CW))
    DR.Cells.Add(NewHeaderCell("BestP", CW))
    DR.Cells.Add(NewHeaderCell("WorstP", CW))
    DR.Cells.Add(NewHeaderCell("BestS", CW))
    DR.Cells.Add(NewHeaderCell("WorstS", CW))
    GeneralTableRows.Add(DR)
    For Each Ch In ListaCanaliUp
      GeneralTableRows.Add(RigaValoriCanale(Ch, CW, 12))
    Next

    GeneralTableRows.Add(VerticalSpacer(4))
    H = New HeaderRow With {.Title = "Downwind Channels"}
    GeneralTableRows.Add(H)
    DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewHeaderCell("", 150))
    DR.Cells.Add(NewHeaderCell("Tgt", CW))
    DR.Cells.Add(NewHeaderCell("Best", CW))
    DR.Cells.Add(NewHeaderCell("Worst", CW))
    DR.Cells.Add(NewHeaderCell("TgtSd", CW))
    DR.Cells.Add(NewHeaderCell("BestSd", CW))
    DR.Cells.Add(NewHeaderCell("WorstSd", CW))
    DR.Cells.Add(NewHeaderCell("BestP", CW))
    DR.Cells.Add(NewHeaderCell("WorstP", CW))
    DR.Cells.Add(NewHeaderCell("BestS", CW))
    DR.Cells.Add(NewHeaderCell("WorstS", CW))
    GeneralTableRows.Add(DR)
    For Each Ch In ListaCanaliDn
      GeneralTableRows.Add(RigaValoriCanale(Ch, CW, 12))
    Next


  End Sub

  Public Sub StampaPdf()
    Dim pdf As New clsPdf
    Dim ss = clsTimeRangeReportUtility.SelectedStart
    Dim sstime As String = DataPlotSync.VisibleRange.Start.ToString("yyyyMMdd_HHmmss")
    If Not ss Is Nothing Then
      sstime = clsTimeRangeReportUtility.SelectedStart.StartTime.ToString("yyyyMMdd_HHmmss")
    End If
    pdf.StampaSummaryHighlightReport(AppConfig.ActiveProfile.ProfileName & " " & sstime & " ", "SummaryAndHighlights_" & AppConfig.ActiveProfile.ProfileName & "_" & sstime, GeneralTableRows)

  End Sub

  Function RigaValoriCanale(Canale As clsChannelAdvanced, CW As Integer, FontSize As Integer) As DataRow
    Dim pThreshold As Double = Canale.Canale.AlertDelta
    Dim pMinValAlarm As Double = Canale.Canale.MinVal
    Dim pMaxValAlarm As Double = Canale.Canale.MaxVal
    Dim DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    Dim HLB As New Dictionary(Of eValori, Boolean)
    Dim HLW As New Dictionary(Of eValori, Boolean)
    Dim ALB As New Dictionary(Of eValori, Boolean)
    Dim ALW As New Dictionary(Of eValori, Boolean)
    HLB.Add(eValori.eBoth, False)
    HLB.Add(eValori.ePort, False)
    HLB.Add(eValori.eStbd, False)
    HLW.Add(eValori.eBoth, False)
    HLW.Add(eValori.ePort, False)
    HLW.Add(eValori.eStbd, False)
    ALB.Add(eValori.eBoth, False)
    ALB.Add(eValori.ePort, False)
    ALB.Add(eValori.eStbd, False)
    ALW.Add(eValori.eBoth, False)
    ALW.Add(eValori.ePort, False)
    ALW.Add(eValori.eStbd, False)
    'Dim HLTB As Boolean = False
    'Dim HLBW As Boolean = False
    'Dim HLBPS As Boolean = False
    'Dim HLWPS As Boolean = False

    If Not Double.IsNaN(pThreshold) Then
      If Not Double.IsNaN(Canale.ValoriBoth.BestTgtAvg) Then
        HLB(eValori.eBoth) = Math.Abs(Canale.ValoriBoth.BestTgtAvg - Canale.ValoriBoth.BestAvg) > pThreshold
        HLB(eValori.ePort) = Math.Abs(Canale.ValoriPort.BestTgtAvg - Canale.ValoriPort.BestAvg) > pThreshold
        HLB(eValori.eStbd) = Math.Abs(Canale.ValoriStbd.BestTgtAvg - Canale.ValoriStbd.BestAvg) > pThreshold
      End If
      HLW(eValori.eBoth) = Math.Abs(Canale.ValoriBoth.WorstAvg - Canale.ValoriBoth.BestAvg) > pThreshold
      HLW(eValori.ePort) = Math.Abs(Canale.ValoriPort.WorstAvg - Canale.ValoriPort.BestAvg) > pThreshold
      HLW(eValori.eStbd) = Math.Abs(Canale.ValoriStbd.WorstAvg - Canale.ValoriStbd.BestAvg) > pThreshold
    End If
    If Not Double.IsNaN(pMinValAlarm) Then
      ALB(eValori.eBoth) = Canale.ValoriBoth.BestAvg < pMinValAlarm
      ALB(eValori.ePort) = Canale.ValoriPort.BestAvg < pMinValAlarm
      ALB(eValori.eStbd) = Canale.ValoriStbd.BestAvg < pMinValAlarm
      ALW(eValori.eBoth) = Canale.ValoriBoth.WorstAvg < pMinValAlarm
      ALW(eValori.ePort) = Canale.ValoriPort.WorstAvg < pMinValAlarm
      ALW(eValori.eStbd) = Canale.ValoriStbd.WorstAvg < pMinValAlarm
    End If
    If Not Double.IsNaN(pMaxValAlarm) Then
      ALB(eValori.eBoth) = ALB(eValori.eBoth) OrElse Canale.ValoriBoth.BestAvg > pMaxValAlarm
      ALB(eValori.ePort) = ALB(eValori.ePort) OrElse Canale.ValoriPort.BestAvg > pMaxValAlarm
      ALB(eValori.eStbd) = ALB(eValori.eStbd) OrElse Canale.ValoriStbd.BestAvg > pMaxValAlarm
      ALW(eValori.eBoth) = ALW(eValori.eBoth) OrElse Canale.ValoriBoth.WorstAvg > pMaxValAlarm
      ALW(eValori.ePort) = ALW(eValori.ePort) OrElse Canale.ValoriPort.WorstAvg > pMaxValAlarm
      ALW(eValori.eStbd) = ALW(eValori.eStbd) OrElse Canale.ValoriStbd.WorstAvg > pMaxValAlarm
    End If

    DR.Cells.Add(NewDataCell(Canale.Canale.LongName, 150, FontSize))
    DR.Cells.Add(NewDataCell(Canale.ValoreFormattato(Canale.ValoriBoth.BestTgtAvg), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriBoth.BestAvg), HLB(eValori.eBoth), ALB(eValori.eBoth), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriBoth.WorstAvg), HLW(eValori.eBoth), ALW(eValori.eBoth), CW, FontSize))
    DR.Cells.Add(NewDataCell(Canale.ValoreFormattato(Canale.ValoriBoth.BestTgtSd), CW, FontSize))
    DR.Cells.Add(NewDataCell(Canale.ValoreFormattato(Canale.ValoriBoth.BestSd), CW, FontSize))
    DR.Cells.Add(NewDataCell(Canale.ValoreFormattato(Canale.ValoriBoth.WorstSd), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriPort.BestAvg), HLB(eValori.ePort), ALB(eValori.ePort), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriPort.WorstAvg), HLW(eValori.ePort), ALB(eValori.eStbd), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriStbd.BestAvg), HLB(eValori.eStbd), ALW(eValori.ePort), CW, FontSize))
    DR.Cells.Add(NewDataCellHighlight(Canale.ValoreFormattato(Canale.ValoriStbd.WorstAvg), HLW(eValori.eStbd), ALW(eValori.eStbd), CW, FontSize))

    'Dim bp As Double = Canale.ValoriPort.BestAvg
    'Dim bs As Double = Canale.ValoriStbd.BestAvg
    'Dim wp As Double = Canale.ValoriPort.WorstAvg
    'Dim ws As Double = Canale.ValoriStbd.WorstAvg
    'DR.Cells.Add(NewDataCell((bp - bs).ToString("F" & Canale.Canale.Decimals.ToString), CW, FontSize))
    'DR.Cells.Add(NewDataCell((wp - ws).ToString("F" & Canale.Canale.Decimals.ToString), CW, FontSize))
    Return DR




  End Function

  Function WindStatString(Tws As Boolean, Upwind As Boolean, Valori As clsChannelAdvanced.eValori) As String
    Dim WS As clsVmgWindStat
    Select Case Valori
      Case clsChannelAdvanced.eValori.eStbd
        If Upwind Then
          WS = clsTimeRangeReportUtility.VmgWindStats.UpwindStbd
        Else
          WS = clsTimeRangeReportUtility.VmgWindStats.DownwindStbd
        End If
      Case clsChannelAdvanced.eValori.ePort
        If Upwind Then
          WS = clsTimeRangeReportUtility.VmgWindStats.UpwindPort
        Else
          WS = clsTimeRangeReportUtility.VmgWindStats.DownwindPort
        End If
      Case Else
        If Upwind Then
          WS = clsTimeRangeReportUtility.VmgWindStats.Upwind
        Else
          WS = clsTimeRangeReportUtility.VmgWindStats.Downwind
        End If
    End Select
    If Tws Then
      Return WS.TwsAvg.ToString("F1") & " (" & WS.TwsMin.ToString("F1") & "-" & WS.TwsMax.ToString("F1") & ")"
    Else
      Return WS.TwdAvg.ToString("F0") & " (" & WS.TwdMaxLeft.ToString("F0") & "-" & WS.TwdMaxRight.ToString("F0") & ")"
    End If
  End Function


  Function RigaDatiStraightLine(Name As String, Dati As clsBestWorstDetails, CW As Integer, chVmgP As clsChannel2020, chBsP As clsChannel2020, chTwaD As clsChannel2020, chVmg As clsChannel2020, chBs As clsChannel2020, chIsStbd As clsChannel2020, chSeaState As clsChannel2020, Optional FontSize As Integer = 14) As DataRow
    Dim Rows = clsTimeRangeReportUtility.Rows

    Dim SopraSoglia = Dati.IndiciSopraSoglia
    Dim SottoSoglia = Dati.IndiciSottoSoglia
    Dim SopraEtSottoSoglia As New List(Of Integer)
    SopraEtSottoSoglia.AddRange(SopraSoglia)
    SopraEtSottoSoglia.AddRange(SottoSoglia)

    Dim VmgSopraSoglia = GetAverage(chVmg, SopraSoglia, chIsStbd)
    Dim VmgSottoSoglia = GetAverage(chVmg, SottoSoglia, chIsStbd)
    Dim BsSopraSoglia = GetAverage(chBs, SopraSoglia, chIsStbd)
    Dim DeltaVmgMs = KtsToMS(VmgSopraSoglia - VmgSottoSoglia)
    Dim TotMtVmg = DeltaVmgMs * (SottoSoglia.Count / DataProvider2020.Hz) ' metri totali in vmg
    Dim TotSsVmg = TotMtVmg / KtsToMS(VmgSopraSoglia) ' secondi totali da delta vmg
    Dim TotMtInLinea = TotSsVmg * KtsToMS(BsSopraSoglia) ' secondi totali da delta vmg
    Dim StringaLoss As String = (TotMtInLinea / AppConfig.ActiveProfile.BoatLenghtInMeters).ToString("F1")
    StringaLoss &= " [" & TotMtInLinea.ToString("F0") & "]"

    Dim VmgTotalSeconds As Double = SopraEtSottoSoglia.Count / DataProvider2020.Hz

    Dim DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell(Name, CW, FontSize))
    DR.Cells.Add(NewDataCell(((SopraEtSottoSoglia.Count / Rows.Count) * 100).ToString("F0") & "%", CW, FontSize))
    DR.Cells.Add(NewDataCell(New DateTime().AddSeconds(VmgTotalSeconds).ToString("mm:ss"), CW, FontSize))
    DR.Cells.Add(NewDataCell(((SopraSoglia.Count / (SopraSoglia.Count + SottoSoglia.Count)) * 100).ToString("F0") & "%", CW, FontSize))
    DR.Cells.Add(NewDataCell(((SottoSoglia.Count / (SopraSoglia.Count + SottoSoglia.Count)) * 100).ToString("F0") & "%", CW, FontSize))
    DR.Cells.Add(NewDataCell(StringaLoss, CW, FontSize))
    DR.Cells.Add(NewDataCell(TotSsVmg.ToString("F0"), CW / 2, FontSize))
    DR.Cells.Add(NewDataCell(GetAverage(chSeaState, SopraEtSottoSoglia, chIsStbd).ToString("F1"), CW / 2, FontSize))
    DR.Cells.Add(NewDataCell(GetAverage(chVmgP, SopraEtSottoSoglia, chIsStbd).ToString("F0"), CW / 2, FontSize))
    DR.Cells.Add(NewDataCell(GetAverage(chBsP, SopraEtSottoSoglia, chIsStbd).ToString("F0"), CW / 2, FontSize))
    DR.Cells.Add(NewDataCell(GetAverage(chTwaD, SopraEtSottoSoglia, chIsStbd).ToString("F0"), CW / 2, FontSize))
    'DR.Cells.Add(NewDataCell(((IndiciSottoMin.Count / Rows.Count) * 100).ToString("F0") & "%", CW))

    Return DR

  End Function

  Function RigaDatiManovre(Name As String, ListaIndici As List(Of Integer), IndiciSopraSoglia As List(Of Integer), CW As Integer, NrOfManoeuvers As Integer, chVmgP As clsChannel2020, chBsP As clsChannel2020, chTwaD As clsChannel2020, chVmg As clsChannel2020, chBs As clsChannel2020, chIsStbd As clsChannel2020) As DataRow
    Dim Rows = clsTimeRangeReportUtility.Rows

    Dim ManoeuveringTotalSeconds As Double = ListaIndici.Count / DataProvider2020.Hz

    Dim VmgSopraSoglia = GetAverage(chVmg, IndiciSopraSoglia, chIsStbd)
    Dim BsSopraSoglia = GetAverage(chBs, IndiciSopraSoglia, chIsStbd)
    Dim VmgManoeuvering = GetAverage(chVmg, ListaIndici, chIsStbd)
    Dim DeltaVmgMs = KtsToMS(VmgSopraSoglia - VmgManoeuvering)
    Dim TotMtVmg = DeltaVmgMs * (ListaIndici.Count / DataProvider2020.Hz) ' metri totali in vmg
    Dim TotSsVmg = TotMtVmg / KtsToMS(VmgSopraSoglia) ' secondi totali da delta vmg
    Dim TotMtInLinea = TotSsVmg * KtsToMS(BsSopraSoglia) ' secondi totali da delta vmg
    Dim StringaLoss As String = (TotMtInLinea / AppConfig.ActiveProfile.BoatLenghtInMeters).ToString("F1")
    StringaLoss &= " [" & TotMtInLinea.ToString("F0") & "]"

    Dim TotSsVmgXm = TotSsVmg / NrOfManoeuvers ' secondi totali da delta vmg
    Dim TotMtInLineaXm = TotSsVmgXm * KtsToMS(BsSopraSoglia) ' secondi totali da delta vmg
    Dim StringaLossXm As String = (TotMtInLineaXm / AppConfig.ActiveProfile.BoatLenghtInMeters).ToString("F1")
    StringaLossXm &= " [" & TotMtInLineaXm.ToString("F0") & "]"


    Dim DR = New DataRow With {.Cells = New Collections.ObjectModel.ObservableCollection(Of Cell)}
    DR.Cells.Add(NewDataCell(Name, CW))
    DR.Cells.Add(NewDataCell(((ListaIndici.Count / Rows.Count) * 100).ToString("F0") & "%", CW))
    DR.Cells.Add(NewDataCell(New DateTime().AddSeconds(ManoeuveringTotalSeconds).ToString("mm:ss"), CW))
    DR.Cells.Add(NewDataCell(StringaLoss, CW))
    DR.Cells.Add(NewDataCell(TotSsVmg.ToString("F0"), CW))
    DR.Cells.Add(NewDataCell(NrOfManoeuvers, CW))
    DR.Cells.Add(NewDataCell(StringaLossXm, CW))
    DR.Cells.Add(NewDataCell(TotSsVmgXm.ToString("F0"), CW))

    Return DR


  End Function


  Function GetAverage(Channel As clsChannel2020, Idxs As List(Of Integer), CanaleIsStbd As clsChannel2020) As Double
    If Channel Is Nothing Then Return Double.NaN
    Dim vv As New List(Of Double)
    For Each i In Idxs
      Dim v As Double = Channel.Valori(i)
      Dim IsStbd As Double = CanaleIsStbd.Valori(i)
      If Not Double.IsNaN(v) AndAlso Not Double.IsNaN(IsStbd) Then
        Select Case Channel.DataType
          Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180
            vv.Add(System.Math.Abs(v))
          Case clsChannel2020.eDataType.eTack
            If Not IsStbd Then
              vv.Add(System.Math.Abs(-v))
            End If
          Case clsChannel2020.eDataType.eTackReversed
            If IsStbd Then
              vv.Add(System.Math.Abs(-v))
            End If
          Case Else
            vv.Add(v)
        End Select
      End If
    Next
    If vv.Count = 0 Then Return Double.NaN
    Return vv.Average
  End Function


  Public Sub RiempiValori(Periodi As List(Of clsPeriod2021))
    AzzeraRighe()

    For Each p In Periodi
      For i As Integer = p.TR.IdRigaIniziale To p.TR.IdRigaFinale
        RiempiRigaValida(i)
      Next
    Next
    RiempiCanaliAvanzati(ListaCanaliUp)
    RiempiCanaliAvanzati(ListaCanaliDn)
  End Sub

  Private Sub RiempiRigaValida(IdRiga As Integer)
    Dim v = CanaleFiltro.Valori(IdRiga)
    Dim twa = CanaleTwa.Valori(IdRiga)
    If Not Double.IsNaN(v) AndAlso Not Double.IsNaN(twa) Then
      Dim IsStbd As Boolean = twa >= 0
      Dim valido As Boolean = v >= VminFiltro AndAlso v <= VmaxFiltro
      If valido Then
        RigheSopraSoglia.Add(IdRiga, IsStbd)
      Else
        RigheSottoSoglia.Add(IdRiga, IsStbd)
      End If
    End If

  End Sub


  Private Sub RiempiCanaliAvanzati(Lista As List(Of clsChannelAdvanced))

    Dim ListaIsUp As Boolean = Lista Is ListaCanaliUp
    For Each c As clsChannelAdvanced In Lista
      Dim tgt As clsChannelTarget = TgtManager.Tgt.Polare(c.Canale.PolarHeader)
      Dim ver = DataProvider2020.CanaleDbl(c.Canale)
      'Dim spsu As Integer = 0
      'Dim stsu As Integer = 0
      'Dim spsk As Integer = 0
      'Dim stsk As Integer = 0
      'Dim spsd As Integer = 0
      'Dim stsd As Integer = 0
      For Each r In RigheSopraSoglia
        Dim v As Double = c.Canale.Valori(r.Key)
        Dim t As Double = Double.NaN
        Dim twa As Double = CanaleTwa.Valori(r.Key)
        Dim tws As Double = CanaleTws.Valori(r.Key)
        If Not tgt Is Nothing AndAlso Not Double.IsNaN(tws) AndAlso Not Double.IsNaN(twa) Then
          t = tgt.TargetValue(tws, Math.Abs(twa) <= 90)
        End If
        If Not Double.IsNaN(v) Then
          Select Case c.Canale.DataType
            Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180
              v = Math.Abs(v)
            Case clsChannel2020.eDataType.eTack
              v = IIf(twa >= 0, v, -v)
            Case clsChannel2020.eDataType.eTackReversed
              v = IIf(twa >= 0, -v, v)
          End Select
          Dim IsUp As Boolean = Math.Abs(twa) <= 90
          If IsUp AndAlso ListaIsUp Then
            'spsu += 1
            c.ValoriBoth.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            If r.Value = True Then 'stbd
              c.ValoriStbd.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            Else 'port
              c.ValoriPort.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            End If
          ElseIf Not IsUp AndAlso Not ListaIsUp Then
            'spsd += 1
            c.ValoriBoth.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            If r.Value = True Then
              c.ValoriStbd.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            Else
              c.ValoriPort.ValoriSopraSoglia.Add(New clsIdXY(r.Key, v, t))
            End If
          Else
            'spsk += 1
          End If
        End If
      Next
      For Each r In RigheSottoSoglia
        Dim v As Double = c.Canale.Valori(r.Key)
        Dim t As Double = Double.NaN
        Dim tws As Double = CanaleTws.Valori(r.Key)
        Dim twa As Double = CanaleTwa.Valori(r.Key)
        If Not tgt Is Nothing AndAlso Not Double.IsNaN(tws) AndAlso Not Double.IsNaN(twa) Then
          t = tgt.TargetValue(tws, Math.Abs(twa) <= 90)
        End If
        If Not Double.IsNaN(v) Then
          Select Case c.Canale.DataType
            Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180
              v = Math.Abs(v)
            Case clsChannel2020.eDataType.eTack
              v = IIf(twa >= 0, v, -v)
            Case clsChannel2020.eDataType.eTackReversed
              v = IIf(twa >= 0, -v, v)
          End Select
          Dim IsUp As Boolean = Math.Abs(twa) <= 90
          If IsUp AndAlso ListaIsUp Then
            'stsu += 1
            c.ValoriBoth.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            If r.Value = True Then
              c.ValoriStbd.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            Else
              c.ValoriPort.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            End If
          ElseIf Not IsUp AndAlso Not ListaIsUp Then
            'stsd += 1
            c.ValoriBoth.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            If r.Value = True Then
              c.ValoriStbd.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            Else
              c.ValoriPort.ValoriSottoSoglia.Add(New clsIdXY(r.Key, v, t))
            End If
          Else
            'stsk += 1
          End If
        End If
      Next
      'Dim a = RigheSopraSoglia.Count + RigheSottoSoglia.Count
      'Dim b = spsu + spsd + stsu + stsd
      'Dim d = spsk + stsk
      'Stop
    Next

    AggiornaValoriBestWorst(Lista)
  End Sub

  Public Sub MakeReport(TR As clsTimeRange)
    RiempiValori(TR)
    FillDataTables()
  End Sub

  Public Sub MakeReport(Periodi As List(Of clsPeriod2021))
    RiempiValori(Periodi)
    FillDataTables()
  End Sub

  Sub FillDataTables()
    Dim Righe As New List(Of String)
    Righe.Add("Upwind")
    Righe.AddRange(FillDataTables(ListaCanaliUp))
    Righe.Add("")
    Righe.Add("")
    Righe.Add("DownWind")
    Righe.AddRange(FillDataTables(ListaCanaliDn))
    Clipboard.SetText(String.Join(vbCrLf, Righe))

  End Sub


  Public Function FillDataTables(Lista As List(Of clsChannelAdvanced)) As List(Of String)

    FillDataTable(clsChannelAdvanced.eValori.eBoth, Lista)
    FillDataTable(clsChannelAdvanced.eValori.ePort, Lista)
    FillDataTable(clsChannelAdvanced.eValori.eStbd, Lista)

    Return CopyReportFullToClipboard(Lista)

  End Function

  Public Sub FillDataTable(Valori As clsChannelAdvanced.eValori, Lista As List(Of clsChannelAdvanced))
    Dim TAB As New System.Data.DataTable
    TAB.Columns.Clear()
    TAB.Columns.Add("Channel", GetType(Object))
    ' Colonne successive: double
    TAB.Columns.Add("BestAvg", GetType(Double))
    TAB.Columns.Add("TgtBestAvg", GetType(Double))
    TAB.Columns.Add("dBestToTgtAvg", GetType(Double))
    TAB.Columns.Add("WstAvg", GetType(Double))
    TAB.Columns.Add("TgtWstAvg", GetType(Double))
    TAB.Columns.Add("dWstToTgtAvg", GetType(Double))
    TAB.Columns.Add("dBestToWstAvg", GetType(Double))
    TAB.Columns.Add("dBestAndWstToTgtAvg", GetType(Double))
    TAB.Columns.Add("BestSD", GetType(Double))
    TAB.Columns.Add("TgtBestSD", GetType(Double))
    TAB.Columns.Add("dBestToTgtSD", GetType(Double))
    TAB.Columns.Add("WstSD", GetType(Double))
    TAB.Columns.Add("TgtWstSD", GetType(Double))
    TAB.Columns.Add("dWstToTgtSD", GetType(Double))
    TAB.Columns.Add("dBestToWstSD", GetType(Double))
    TAB.Columns.Add("dBestAndWstToTgtSD", GetType(Double))

    TAB.Columns.Add("BestTrg%Avg", GetType(Double))
    TAB.Columns.Add("BestTrg%Sd", GetType(Double))
    TAB.Columns.Add("BestWorst%Avg", GetType(Double))
    TAB.Columns.Add("BestWorst%Sd", GetType(Double))

    TAB.Rows.Clear()
    Dim Vtmp As Double = Double.NaN
    For Each c In Lista
      Dim ListaValori As clsBestWorstDetails = Nothing
      Select Case Valori
        Case eValori.eBoth
          ListaValori = c.ValoriBoth
        Case eValori.ePort
          ListaValori = c.ValoriPort
        Case eValori.eStbd
          ListaValori = c.ValoriStbd
      End Select
      Dim Vv As New List(Of Object)
      Vv.Add(c.Canale)
      Vtmp = ListaValori.BestAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.BestTgtAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestToTgtAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.WorstAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.WorstTgtAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dWorstToTgtAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestToWorstAvg
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestVsTgtToWorstVsTgtAvg
      Vv.Add(Vtmp)

      Vtmp = ListaValori.BestSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.BestTgtSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestToTgtSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.WorstSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.WorstTgtSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dWorstToTgtSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestToWorstSd
      Vv.Add(Vtmp)
      Vtmp = ListaValori.dBestVsTgtToWorstVsTgtSd
      Vv.Add(Vtmp)

      If Double.IsNaN(ListaValori.BestTgtAvg) Then
        Vv.Add(Double.NaN)
        Vv.Add(Double.NaN)
      Else
        Vtmp = (ListaValori.BestAvg - ListaValori.BestTgtAvg) / ListaValori.BestAvg * 100
        Vv.Add(Vtmp)
        Vtmp = (ListaValori.BestSd - ListaValori.BestTgtSd) / ListaValori.BestSd * 100
        Vv.Add(Vtmp)
      End If
      Vtmp = (ListaValori.BestAvg - ListaValori.WorstAvg) / ListaValori.BestAvg * 100
      Vv.Add(Vtmp)
      Vtmp = (ListaValori.BestSd - ListaValori.WorstSd) / ListaValori.BestSd * 100
      Vv.Add(Vtmp)


      TAB.Rows.Add(Vv.ToArray)
    Next

    Select Case Valori
      Case clsChannelAdvanced.eValori.eBoth
        If Lista Is ListaCanaliUp Then
          TabellaDatiUp = TAB
        Else
          TabellaDatiDn = TAB
        End If
      Case clsChannelAdvanced.eValori.ePort
        If Lista Is ListaCanaliUp Then
          TabellaDatiUpPort = TAB
        Else
          TabellaDatiDnPort = TAB
        End If
      Case clsChannelAdvanced.eValori.eStbd
        If Lista Is ListaCanaliUp Then
          TabellaDatiUpStbd = TAB
        Else
          TabellaDatiDnStbd = TAB
        End If
    End Select

  End Sub

  Public Sub AggiornaValoriBestWorst(Valori As clsChannelAdvanced.eValori, Lista As List(Of clsChannelAdvanced))
    Dim Vtmp As Double = Double.NaN
    For Each c In Lista
      Dim ListaValori As clsBestWorstDetails = Nothing
      Select Case Valori
        Case eValori.eBoth
          ListaValori = c.ValoriBoth
        Case eValori.ePort
          ListaValori = c.ValoriPort
        Case eValori.eStbd
          ListaValori = c.ValoriStbd
      End Select

      ListaValori.BestAvg = c.GetValore(True, clsChannelAdvanced.eStatistiche.Media, Valori, False)
      ListaValori.WorstAvg = c.GetValore(False, clsChannelAdvanced.eStatistiche.Media, Valori, False)
      ListaValori.BestTgtAvg = c.GetValore(True, clsChannelAdvanced.eStatistiche.Media, Valori, True)
      ListaValori.WorstTgtAvg = c.GetValore(False, clsChannelAdvanced.eStatistiche.Media, Valori, True)

      ListaValori.dBestToTgtAvg = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, Valori, clsChannelAdvanced.eDeltaType.eBestToTarget)
      ListaValori.dWorstToTgtAvg = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, Valori, clsChannelAdvanced.eDeltaType.eWorstToTarget)
      ListaValori.dBestToWorstAvg = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, Valori, clsChannelAdvanced.eDeltaType.eBestToWorst)
      ListaValori.dBestVsTgtToWorstVsTgtAvg = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, Valori, clsChannelAdvanced.eDeltaType.eBestVsTgtToWorstVsTgt)


      ListaValori.BestSd = c.GetValore(True, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, False)
      ListaValori.WorstSd = c.GetValore(False, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, False)
      ListaValori.BestTgtSd = c.GetValore(True, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, True)
      ListaValori.WorstTgtSd = c.GetValore(False, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, True)

      ListaValori.dBestToTgtSd = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, clsChannelAdvanced.eDeltaType.eBestToTarget)
      ListaValori.dWorstToTgtSd = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, clsChannelAdvanced.eDeltaType.eWorstToTarget)
      ListaValori.dBestToWorstSd = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, clsChannelAdvanced.eDeltaType.eBestToWorst)
      ListaValori.dBestVsTgtToWorstVsTgtSd = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, clsChannelAdvanced.eDeltaType.eBestVsTgtToWorstVsTgt)
    Next

  End Sub

  'Public Sub CopyReportToClipboard()
  '  Dim Righe As New List(Of String)
  '  Dim riga As String = "Channel Name" & vbTab & "TopAvg" & vbTab & "TopSD" & vbTab & "BtmAvg" & vbTab & "BtmSD"
  '  riga &= vbTab & "TgtTopAvg" & vbTab & "TgtTopSD" & vbTab & "TgtBtmAvg" & vbTab & "TgtBtmSD"
  '  Righe.Add(riga)
  '  For Each canale In ListaCanali
  '    riga = canale.Canale.LongName & GetRow(canale, clsChannelAdvanced.eValori.eBoth, False)
  '    riga &= GetRow(canale, clsChannelAdvanced.eValori.eBoth, False)
  '    Righe.Add(riga)
  '  Next
  '  Righe.Add("")
  '  Righe.Add("Port Details")
  '  For Each canale In ListaCanali
  '    riga = canale.Canale.LongName & GetRow(canale, clsChannelAdvanced.eValori.ePort, False)
  '    riga &= GetRow(canale, clsChannelAdvanced.eValori.ePort, False)
  '    Righe.Add(riga)
  '  Next
  '  Righe.Add("")
  '  Righe.Add("Stbd Details")
  '  For Each canale In ListaCanali
  '    riga = canale.Canale.LongName & GetRow(canale, clsChannelAdvanced.eValori.eStbd, False)
  '    riga &= GetRow(canale, clsChannelAdvanced.eValori.eStbd, False)
  '    Righe.Add(riga)
  '  Next

  '  Clipboard.SetText(String.Join(vbCrLf, Righe))
  '  MsgBox("Copied to the clipboard")
  'End Sub

  'Private Function GetRow(canale As clsChannelAdvanced, Valori As clsChannelAdvanced.eValori, GetTarget As Boolean) As String
  '  Dim Riga As String = vbTab & canale.GetValoreFormattato(True, clsChannelAdvanced.eStatistiche.Media, Valori, GetTarget)
  '  Riga &= vbTab & canale.GetValoreFormattato(False, clsChannelAdvanced.eStatistiche.Media, Valori, GetTarget)
  '  Riga &= vbTab & canale.GetValoreFormattato(True, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, GetTarget)
  '  Riga &= vbTab & canale.GetValoreFormattato(False, clsChannelAdvanced.eStatistiche.DeviazioneStandard, Valori, GetTarget)
  '  Return Riga
  'End Function

  Public Function CopyReportFullToClipboard(Lista As List(Of clsChannelAdvanced)) As List(Of String)

    Dim Righe As New List(Of String)
    Dim Intestazione As String = "Channel Name" & vbTab & "BestAvg" & vbTab & "BestTgtAvg" & vbTab & "DeltaBestToTgtAvg"
    Intestazione &= vbTab & "WorstAvg" & vbTab & "WorstTgtAvg" & vbTab & "DeltaWorstToTgtAvg"
    Intestazione &= vbTab & "DeltaBestToWorstAvg" & vbTab & "DeltaBestVsTgtToWorstVsTgtAvg"
    Intestazione &= vbTab & "BestSd" & vbTab & "BestTgtSd" & vbTab & "DeltaBestToTgtSd"
    Intestazione &= vbTab & "WorstSd" & vbTab & "WorstTgtSd" & vbTab & "DeltaWorstToTgtSd"
    Intestazione &= vbTab & "DeltaBestToWorstSd" & vbTab & "DeltaBestVsTgtToWorstVsTgtSd"

    Intestazione &= vbTab & "BestToTgt%Avg" & vbTab & "BestToTgt%Sd"
    Intestazione &= vbTab & "BestWorst%Avg" & vbTab & "BestWorst%Sd"
    Righe.Add(Intestazione)
    For Each C In Lista
      Righe.Add(C.Canale.LongName & GetRowFull(C, eValori.eBoth))
    Next
    Righe.Add("")
    Righe.Add("Port Details")
    Righe.Add(Intestazione)
    For Each C In Lista
      Righe.Add(C.Canale.LongName & GetRowFull(C, eValori.ePort))
    Next
    Righe.Add("")
    Righe.Add("Stbd Details")
    Righe.Add(Intestazione)
    For Each C In Lista
      Righe.Add(C.Canale.LongName & GetRowFull(C, eValori.eStbd))
    Next

    Return Righe

    'MsgBox("Copied to the clipboard")
  End Function

  Private Function GetRowFull(canale As clsChannelAdvanced, Valori As eValori) As String
    Dim ListaValori As clsBestWorstDetails = Nothing
    Select Case Valori
      Case eValori.eBoth
        ListaValori = canale.ValoriBoth
      Case eValori.ePort
        ListaValori = canale.ValoriPort
      Case eValori.eStbd
        ListaValori = canale.ValoriStbd
      Case Else
        Return Nothing
    End Select
    Dim Riga As String = vbTab & canale.ValoreFormattato(ListaValori.BestAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.BestTgtAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestToTgtAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.WorstAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.WorstTgtAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dWorstToTgtAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestToWorstAvg)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestVsTgtToWorstVsTgtAvg)

    Riga &= vbTab & canale.ValoreFormattato(ListaValori.BestSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.BestTgtSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestToTgtSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.WorstSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.WorstTgtSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dWorstToTgtSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestToWorstSd)
    Riga &= vbTab & canale.ValoreFormattato(ListaValori.dBestVsTgtToWorstVsTgtSd)

    If Double.IsNaN(ListaValori.BestTgtAvg) Then
      Riga &= vbTab & ""
      Riga &= vbTab & ""
    Else
      Riga &= vbTab & ((ListaValori.BestAvg - ListaValori.BestTgtAvg) / ListaValori.BestAvg * 1).ToString("F3")
      Riga &= vbTab & ((ListaValori.BestSd - ListaValori.BestTgtSd) / ListaValori.BestSd * 1).ToString("F3")
    End If
    Riga &= vbTab & ((ListaValori.BestAvg - ListaValori.WorstAvg) / ListaValori.BestAvg * 1).ToString("F3")
    Riga &= vbTab & ((ListaValori.BestSd - ListaValori.WorstSd) / ListaValori.BestSd * 1).ToString("F3")

    Return Riga
  End Function



  Private Sub DisegnaGraficiAvg(VersusTarget As Boolean, Lista As List(Of clsChannelAdvanced))
    Dim plt = PlotAvg.Plot

    For Each c In Lista
      Dim x, y As Double
      If VersusTarget Then
        Dim BestTgtAvg As Double = c.GetValore(True, clsChannelAdvanced.eStatistiche.Media, clsChannelAdvanced.eValori.eBoth, True)
        If Not Double.IsNaN(BestTgtAvg) Then
          'quando VersusTarget = true, va avanti solo se esiste il target
          'la x ha best avg vs tgt vg
          'la y la devizaione standard best vs target
          'x = differenza percentuale tra besttgtavg e bestavg rispetto allo stesso besttgtavg value
          Dim dBestToTgtAvg As Double = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, clsChannelAdvanced.eValori.eBoth, clsChannelAdvanced.eDeltaType.eBestToTarget)
          x = dBestToTgtAvg / BestTgtAvg * 100
          'y se esiste il target e' la differenza percentuale tra delta bestavgtotarget e delta worstavgtotarget rispetto al besttgt value
          Dim BestTgtSD As Double = c.GetValore(True, clsChannelAdvanced.eStatistiche.DeviazioneStandard, clsChannelAdvanced.eValori.eBoth, True)
          Dim dBestToTgtSD As Double = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, clsChannelAdvanced.eValori.eBoth, clsChannelAdvanced.eDeltaType.eBestToTarget)
          y = dBestToTgtSD / BestTgtSD * 100
        End If
      Else
        'non e' VersusTarget quindi la differenza e'relativa tra soprasoglia(best) e sottosoglia(worst)
        'x = differenza percentuale tra bestavg e worstavg rispetto al bestavg value
        Dim BestAvg As Double = c.GetValore(True, clsChannelAdvanced.eStatistiche.Media, clsChannelAdvanced.eValori.eBoth, False)
        Dim dBestToWorstAvg As Double = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.Media, clsChannelAdvanced.eValori.eBoth, clsChannelAdvanced.eDeltaType.eBestToWorst)
        x = dBestToWorstAvg / BestAvg * 100
        'y se esiste il target e' la differenza percentuale tra delta bestavgtotarget e delta worstavgtotarget rispetto al besttgt value
        'y e' la differenza percentuale tra bestsd e worstsd rispetto al bestsd value
        Dim BestSd As Double = c.GetValore(True, clsChannelAdvanced.eStatistiche.DeviazioneStandard, clsChannelAdvanced.eValori.eBoth, False)
        Dim dBestToWorstSd As Double = c.GetDeltaValue(clsChannelAdvanced.eStatistiche.DeviazioneStandard, clsChannelAdvanced.eValori.eBoth, clsChannelAdvanced.eDeltaType.eBestToWorst)
        x = dBestToWorstSd / BestSd * 100

      End If
      If Not Double.IsNaN(x) AndAlso Not Double.IsNaN(y) Then
        Dim pts = plt.Add.Scatter(x, y, ScottPlot.Colors.SteelBlue)
      End If
    Next



  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBestWorstDetails
  Public Property ValoriSopraSoglia As New List(Of clsIdXY)
  Public Property ValoriSottoSoglia As New List(Of clsIdXY)

  Public Property BestAvg As Double = Double.NaN
  Public Property WorstAvg As Double = Double.NaN
  Public Property BestTgtAvg As Double = Double.NaN
  Public Property WorstTgtAvg As Double = Double.NaN
  Public Property dBestToTgtAvg As Double = Double.NaN
  Public Property dWorstToTgtAvg As Double = Double.NaN
  Public Property dBestToWorstAvg As Double = Double.NaN
  Public Property dBestVsTgtToWorstVsTgtAvg As Double = Double.NaN

  Public Property BestSd As Double = Double.NaN
  Public Property WorstSd As Double = Double.NaN
  Public Property BestTgtSd As Double = Double.NaN
  Public Property WorstTgtSd As Double = Double.NaN
  Public Property dBestToTgtSd As Double = Double.NaN
  Public Property dWorstToTgtSd As Double = Double.NaN
  Public Property dBestToWorstSd As Double = Double.NaN
  Public Property dBestVsTgtToWorstVsTgtSd As Double = Double.NaN

  Public Function IndiciSopraSoglia() As List(Of Integer)
    Return ValoriSopraSoglia.Select(Function(x) CInt(x.Id)).ToList()
  End Function

  Public Function IndiciSottoSoglia() As List(Of Integer)
    Return ValoriSottoSoglia.Select(Function(x) CInt(x.Id)).ToList()
  End Function

  Public Function BestSdBestFit() As Double
    If Double.IsNaN(BestTgtSd) Then Return BestSd
    Return BestTgtSd - BestSd
  End Function

  Public Function WorstSdBestFit() As Double
    If Double.IsNaN(WorstTgtSd) Then Return WorstSd
    Return WorstTgtSd - WorstSd
  End Function

End Class



<AddINotifyPropertyChangedInterface>
Public Class clsChannelAdvanced
  Public Property Canale As clsChannel2020
  Public Property ValoriBoth As New clsBestWorstDetails
  Public Property ValoriPort As New clsBestWorstDetails
  Public Property ValoriStbd As New clsBestWorstDetails

  Public Function ValoreFormattato(Valore As Double) As String
    If Double.IsNaN(Valore) Then Return ""
    Return Valore.ToString("F" & (Canale.Decimals + 0).ToString)
  End Function

  Public Enum eStatistiche
    Media
    Mediana
    Moda
    DeviazioneStandard
    Varianza
  End Enum


  Public Function GetValorePercentile(SopraSoglia As Boolean, Soglia As Double, Valori As eValori) As Double
    Dim ListaValori As clsBestWorstDetails = Nothing
    Select Case Valori
      Case eValori.eBoth
        ListaValori = ValoriBoth
      Case eValori.ePort
        ListaValori = ValoriPort
      Case eValori.eStbd
        ListaValori = ValoriStbd
    End Select
    If SopraSoglia Then
      If ListaValori.ValoriSopraSoglia.Count = 0 Then Return Double.NaN
      Return GetPercentile(ListaValori.ValoriSopraSoglia, Soglia)
    Else
      If ListaValori.ValoriSottoSoglia.Count = 0 Then Return Double.NaN
      Return GetPercentile(ListaValori.ValoriSottoSoglia, Soglia)
    End If
  End Function

  Public Function GetValore(SopraSoglia As Boolean, Statistica As eStatistiche, Valori As eValori, GetTarget As Boolean) As Double
    Dim lista As List(Of Double)
    lista = GetListaValori(SopraSoglia, Valori, GetTarget)
    If lista Is Nothing Then Return Double.NaN
    If lista.Count = 0 Then Return Double.NaN
    Dim Valore As Double = Double.NaN
    Select Case Statistica
      Case eStatistiche.Media
        Valore = GetMedia(lista)
      Case eStatistiche.Mediana
        Valore = GetMediana(lista)
      Case eStatistiche.Moda
        Valore = GetModa(lista)
      Case eStatistiche.DeviazioneStandard
        Valore = GetDeviazioneStandard(lista)
      Case eStatistiche.Varianza
        Valore = GetVarianza(lista)
      Case Else
        Return Double.NaN
    End Select

    Return Valore
  End Function

  Public Function GetValoreFormattato(SopraSoglia As Boolean, Statistica As eStatistiche, Valori As eValori, GetTarget As Boolean) As String
    Dim lista As List(Of Double)
    lista = GetListaValori(SopraSoglia, Valori, GetTarget)
    If lista Is Nothing Then Return Double.NaN
    If lista.Count = 0 Then Return Double.NaN
    Dim Valore As Double = Double.NaN
    Select Case Statistica
      Case eStatistiche.Media
        Valore = GetMedia(lista)
      Case eStatistiche.Mediana
        Valore = GetMediana(lista)
      Case eStatistiche.Moda
        Valore = GetModa(lista)
      Case eStatistiche.DeviazioneStandard
        Valore = GetDeviazioneStandard(lista)
      Case eStatistiche.Varianza
        Valore = GetVarianza(lista)
      Case Else
        Return Double.NaN
    End Select
    If Double.IsNaN(Valore) Then
      Return ""
    End If
    Return Valore.ToString("F" & Canale.Decimals.ToString)
  End Function

  Public Enum eDeltaType
    eBestToTarget
    eBestToWorst
    eWorstToTarget
    eWorstToBestTarget
    eBestVsTgtToWorstVsTgt
  End Enum

  Public Function GetDeltaValue(Statistica As eStatistiche, Valori As eValori, DeltaType As eDeltaType) As Double
    Dim listA As List(Of Double) = Nothing
    Dim listB As List(Of Double) = Nothing
    Dim listC As List(Of Double) = Nothing
    Dim listD As List(Of Double) = Nothing
    Select Case DeltaType
      Case eDeltaType.eBestToTarget
        listA = GetListaValori(True, Valori, False)
        listB = GetListaValori(True, Valori, True)
      Case eDeltaType.eBestToWorst
        listA = GetListaValori(True, Valori, False)
        listB = GetListaValori(False, Valori, False)
      Case eDeltaType.eWorstToTarget
        listA = GetListaValori(False, Valori, False)
        listB = GetListaValori(False, Valori, True)
      Case eDeltaType.eWorstToBestTarget
        listA = GetListaValori(False, Valori, False)
        listB = GetListaValori(True, Valori, True)
      Case eDeltaType.eBestVsTgtToWorstVsTgt
        listA = GetListaValori(True, Valori, False)
        listB = GetListaValori(True, Valori, True)
        listC = GetListaValori(False, Valori, False)
        listD = GetListaValori(False, Valori, True)
    End Select
    If listA Is Nothing Then Return Double.NaN
    If listA.Count = 0 Then Return Double.NaN
    If listB Is Nothing Then Return Double.NaN
    If listB.Count = 0 Then Return Double.NaN

    If DeltaType = eDeltaType.eBestVsTgtToWorstVsTgt Then
      If listC Is Nothing Then Return Double.NaN
      If listC.Count = 0 Then Return Double.NaN
      If listD Is Nothing Then Return Double.NaN
      If listD.Count = 0 Then Return Double.NaN
      Select Case Statistica
        Case eStatistiche.Media
          Return (GetMedia(listA) - GetMedia(listB)) - (GetMedia(listC) - GetMedia(listD))
        Case eStatistiche.Mediana
          Return (GetMediana(listA) - GetMediana(listB)) - (GetMediana(listC) - GetMediana(listD))
        Case eStatistiche.Moda
          Return (GetModa(listA) - GetModa(listB)) - (GetModa(listC) - GetModa(listD))
        Case eStatistiche.DeviazioneStandard
          Return (GetDeviazioneStandard(listA) - GetDeviazioneStandard(listB)) - (GetDeviazioneStandard(listC) - GetDeviazioneStandard(listD))
        Case eStatistiche.Varianza
          Return (GetVarianza(listA) - GetVarianza(listB)) - (GetVarianza(listC) - GetVarianza(listD))
        Case Else
          Return Double.NaN
      End Select
    Else
      Select Case Statistica
        Case eStatistiche.Media
          Return GetMedia(listA) - GetMedia(listB)
        Case eStatistiche.Mediana
          Return GetMediana(listA) - GetMediana(listB)
        Case eStatistiche.Moda
          Return GetModa(listA) - GetModa(listB)
        Case eStatistiche.DeviazioneStandard
          Return GetDeviazioneStandard(listA) - GetDeviazioneStandard(listB)
        Case eStatistiche.Varianza
          Return GetVarianza(listA) - GetVarianza(listB)
        Case Else
          Return Double.NaN
      End Select
    End If
  End Function

  Public Enum eValori
    eBoth
    ePort
    eStbd
  End Enum

  Public Function GetListaValori(SopraSoglia As Boolean, Valori As eValori, GetTarget As Boolean) As List(Of Double)
    Dim ListaValori As clsBestWorstDetails = Nothing
    Select Case Valori
      Case eValori.eBoth
        ListaValori = ValoriBoth
      Case eValori.ePort
        ListaValori = ValoriPort
      Case eValori.eStbd
        ListaValori = ValoriStbd
      Case Else
        Return Nothing
    End Select
    If SopraSoglia Then
      If GetTarget Then
        Return ListaValori.ValoriSopraSoglia.Select(Function(x) x.Y).ToList
      Else
        Return ListaValori.ValoriSopraSoglia.Select(Function(x) x.X).ToList
      End If
    Else
      If GetTarget Then
        Return ListaValori.ValoriSottoSoglia.Select(Function(x) x.Y).ToList
      Else
        Return ListaValori.ValoriSottoSoglia.Select(Function(x) x.X).ToList
      End If
    End If

  End Function


  Public Sub New(Channel As clsChannel2020)
    Canale = Channel
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsVmgWindStat
  Public Property IsStbd As Boolean
  Public Property IsUpwind As Boolean
  Public Property TwdAvg As Double
  Public Property TwdMaxLeft As Double
  Public Property TwdMaxRight As Double
  Public Property TwsAvg As Double
  Public Property TwsMin As Double
  Public Property TwsMax As Double
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsVmgWindStats
  Public Property Upwind As New clsVmgWindStat
  Public Property UpwindPort As New clsVmgWindStat
  Public Property UpwindStbd As New clsVmgWindStat
  Public Property Downwind As New clsVmgWindStat
  Public Property DownwindPort As New clsVmgWindStat
  Public Property DownwindStbd As New clsVmgWindStat
End Class


<AddINotifyPropertyChangedInterface>
Public Class clsTimeRangeReportUtility

  Public Shared Property Tacks As Integer
  Public Shared Property Gybes As Integer
  Public Shared Rows As New List(Of BoatStatusIndex)
  Public Shared UpwindVmgIndexes As New List(Of Integer)
  Public Shared DownwindVmgIndexes As New List(Of Integer)
  Public Shared ReachingIndexes As New List(Of Integer)
  Public Shared UnderMinPerfUpVmgIndexes As New List(Of Integer)
  Public Shared UnderMinPerfDnVmgIndexes As New List(Of Integer)
  Public Shared UndefynedIndexes As New List(Of Integer)
  Public Shared TackingIndexes As New List(Of Integer)
  Public Shared GybingIndexes As New List(Of Integer)

  Public Shared RowsIndexes As New Dictionary(Of RowStatus, List(Of Integer))

  Public Shared SelectedStart As clsExpeditionStart

  Public Shared VmgWindStats As New clsVmgWindStats


  'Public Shared Function GetSummaryReport(TR As clsTimeRange) As Dictionary(Of RowStatus, List(Of Integer))
  Public Shared Sub SetSummaryReport(TR As clsTimeRange)

    Dim YrtTreshold = AppConfig.ActiveProfile.SummarySettings.YrtTreshold
    Dim StartFinderRangeSeconds = AppConfig.ActiveProfile.SummarySettings.StartFinderRangeSeconds
    Dim SecBeforeRotation = AppConfig.ActiveProfile.SummarySettings.SecBeforeRotation
    Dim SecAfterRotation = AppConfig.ActiveProfile.SummarySettings.SecAfterRotation
    Dim SecBeforeTack = AppConfig.ActiveProfile.SummarySettings.SecBeforeTack
    Dim SecAfterTack = AppConfig.ActiveProfile.SummarySettings.SecAfterTack
    Dim SecBeforeGybe = AppConfig.ActiveProfile.SummarySettings.SecBeforeGybe
    Dim SecAfterGybe = AppConfig.ActiveProfile.SummarySettings.SecAfterGybe
    Dim UpwindMaxAngle = AppConfig.ActiveProfile.SummarySettings.UpwindMaxAngle
    Dim DownWindMinAngle = AppConfig.ActiveProfile.SummarySettings.DownWindMinAngle
    Dim MinAcceptableVmpPerfPerc = AppConfig.ActiveProfile.SummarySettings.MinAcceptableVmpPerfPerc

    YrtTreshold = 8

    'cerca la partenza nell intorno dell inizio del time range
    ExpStarts.CercaPartenze()
    SelectedStart = Nothing

    Dim TRtmp As New clsTimeRange(TR.Start.AddSeconds(-StartFinderRangeSeconds), TR.Start.AddSeconds(StartFinderRangeSeconds))
    For Each s In ExpStarts.StartsList
      If TRtmp.IsInRange(s.StartTime, True, True) Then
        TR.Start = s.StartTime
        SelectedStart = s
        Exit For
      End If
    Next

    Dim chYRT = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim twa As Double = chTwa.Valori(TR.IdRigaIniziale)
    Dim PrevIsStbd As Boolean = Double.IsNaN(twa) OrElse twa > 0
    Rows.Clear()

    ' Set Rotations and Changes of Tack
    For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
      Dim nr As New BoatStatusIndex(i, DataProvider2020.TimeStamps(i))
      twa = chTwa.Valori(i)
      Dim yrt As Double = chYRT.Valori(i)
      If Not Double.IsNaN(twa) AndAlso Not Double.IsNaN(yrt) Then
        nr.IsStbd = twa >= 0
        nr.Angle = AngleFromTwa(Math.Abs(twa), UpwindMaxAngle, DownWindMinAngle)
        nr.IsRotating = Math.Abs(yrt) > YrtTreshold
        nr.IsChangeOfTack = Not (PrevIsStbd = nr.IsStbd)
        PrevIsStbd = nr.IsStbd
      Else
        nr.HasValidData = False
      End If
      Rows.Add(nr)
    Next

    ' Set Rotating ranges
    Dim idxTmp As New List(Of Integer)
    Dim ids, idf As Integer
    For Each row In Rows
      If row.IsRotating Then
        ids = GetRowId(row, -SecBeforeRotation, Rows)
        idf = GetRowId(row, SecAfterRotation, Rows)
        For ii As Integer = ids To idf
          'idxTmp.Add(ii)
          Rows(ii).IsAroundRotation = True
        Next
      End If
    Next

    ' Set Manoeuvering ranges
    For Each row In Rows
      If row.IsChangeOfTack Then
        If row.Angle = Andatura.UpwindVmg Then
          ids = GetRowId(row, -SecBeforeTack, Rows)
          idf = GetRowId(row, SecAfterTack, Rows)
          For ii As Integer = ids To idf
            Rows(ii).IsTacking = True
          Next
        ElseIf row.Angle = Andatura.DownWindVmg Then
          ids = GetRowId(row, -SecBeforeGybe, Rows)
          idf = GetRowId(row, SecAfterGybe, Rows)
          For ii As Integer = ids To idf
            Rows(ii).IsGybing = True
          Next
        End If
      End If
    Next


    ' Count Tacks and Gybes
    Dim PrevIsNotPavarot As Boolean = True
    Tacks = 0
    Gybes = 0
    For Each row In Rows
      If PrevIsNotPavarot And row.IsGybing Then
        Gybes += 1
      ElseIf PrevIsNotPavarot And row.IsTacking Then
        Tacks += 1
      Else
      End If
      PrevIsNotPavarot = Not row.IsGybing And Not row.IsTacking
    Next


    For Each r In Rows
      Dim vmgp As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVmgPercInertial).Valori(r.IdRiga)
      If Not Double.IsNaN(vmgp) Then
        If vmgp < MinAcceptableVmpPerfPerc Then
          'esclude i punti con performance troppo bassa
          If r.Angle = Andatura.DownWindVmg Then
            r.Angle = Andatura.UnderMinDnVmgPerf
          ElseIf r.Angle = Andatura.UpwindVmg Then
            r.Angle = Andatura.UnderMinUpVmgPerf
          Else
          End If
        End If
      End If
    Next

    UndefynedIndexes.Clear()
    UpwindVmgIndexes.Clear()
    DownwindVmgIndexes.Clear()
    TackingIndexes.Clear()
    GybingIndexes.Clear()
    UnderMinPerfUpVmgIndexes.Clear()
    UnderMinPerfDnVmgIndexes.Clear()
    ReachingIndexes.Clear()

    For Each row In Rows
      If row.IsTacking Then
        TackingIndexes.Add(row.IdRiga)
      ElseIf row.IsGybing Then
        GybingIndexes.Add(row.IdRiga)
      ElseIf row.IsPerformanceSailing(Andatura.UpwindVmg) Then
        UpwindVmgIndexes.Add(row.IdRiga)
      ElseIf row.IsPerformanceSailing(Andatura.Reaching) Then
        ReachingIndexes.Add(row.IdRiga)
      ElseIf row.IsPerformanceSailing(Andatura.UnderMinUpVmgPerf) Then
        UnderMinPerfUpVmgIndexes.Add(row.IdRiga)
      ElseIf row.IsPerformanceSailing(Andatura.UnderMinDnVmgPerf) Then
        UnderMinPerfDnVmgIndexes.Add(row.IdRiga)
      ElseIf row.IsPerformanceSailing(Andatura.DownWindVmg) Then
        DownwindVmgIndexes.Add(row.IdRiga)
      Else
        UndefynedIndexes.Add(row.IdRiga)
      End If
    Next

    'Dim RowsIndexes As New Dictionary(Of RowStatus, List(Of Integer))
    RowsIndexes.Clear()
    RowsIndexes.Add(RowStatus.Undefined, UndefynedIndexes)
    RowsIndexes.Add(RowStatus.UpWindVmg, UpwindVmgIndexes)
    RowsIndexes.Add(RowStatus.DownWindVmg, DownwindVmgIndexes)
    RowsIndexes.Add(RowStatus.Tacking, TackingIndexes)
    RowsIndexes.Add(RowStatus.Gybing, GybingIndexes)
    RowsIndexes.Add(RowStatus.UnderMinPerfUpVmg, UnderMinPerfUpVmgIndexes)
    RowsIndexes.Add(RowStatus.UnderMinPerfDnVmg, UnderMinPerfDnVmgIndexes)
    RowsIndexes.Add(RowStatus.Reaching, ReachingIndexes)

    'VmgWindStats = New clsVmgWindStats
    Dim Indici = Rows.Where(Function(x) x.Angle = Andatura.UpwindVmg)
    VmgWindStats.Upwind = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)
    Indici = Rows.Where(Function(x) x.Angle = Andatura.UpwindVmg AndAlso x.IsStbd)
    VmgWindStats.UpwindStbd = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)
    Indici = Rows.Where(Function(x) x.Angle = Andatura.UpwindVmg AndAlso Not x.IsStbd)
    VmgWindStats.UpwindPort = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)

    Indici = Rows.Where(Function(x) x.Angle = Andatura.DownWindVmg)
    VmgWindStats.Downwind = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)
    Indici = Rows.Where(Function(x) x.Angle = Andatura.DownWindVmg AndAlso x.IsStbd)
    VmgWindStats.DownwindStbd = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)
    Indici = Rows.Where(Function(x) x.Angle = Andatura.DownWindVmg AndAlso Not x.IsStbd)
    VmgWindStats.DownwindPort = StraightLineWindStats(Indici.Select(Function(x) x.IdRiga).ToList)



  End Sub

  Shared Function StraightLineWindStats(Indici As List(Of Integer)) As clsVmgWindStat
    If Indici Is Nothing Then Return Nothing
    If Indici.Count = 0 Then Return Nothing
    Dim Result As New clsVmgWindStat
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    If Indici.Count = 1 Then
      Result.TwdAvg = chTwd.Valori(Indici.First)
      Result.TwdMaxRight = Result.TwdAvg
      Result.TwdMaxLeft = Result.TwdAvg
      Result.TwsAvg = chTws.Valori(Indici.First)
      Result.TwsMin = Result.TwsAvg
      Result.TwsMax = Result.TwsAvg
      Return Result
    End If
    Dim TwdValues As New List(Of Double)
    Dim TwsValues As New List(Of Double)
    For Each i In Indici
      Dim twd As Double = chTwd.Valori(i)
      If Not Double.IsNaN(twd) Then
        TwdValues.Add(twd)
      End If
      Dim tws As Double = chTws.Valori(i)
      If Not Double.IsNaN(tws) Then
        TwsValues.Add(tws)
      End If
    Next
    Dim avg = Media360(DirectCast(TwdValues.ToArray, Double()))
    Dim r As New List(Of Double)
    Dim l As New List(Of Double)
    For Each e In TwdValues
      Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
      If d >= 0 Then
        r.Add(d)
      Else
        l.Add(Math.Abs(d))
      End If
    Next
    Result.TwdAvg = avg
    Result.TwdMaxRight = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
    Result.TwdMaxLeft = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)

    Result.TwsAvg = Media(TwsValues.ToArray)
    Result.TwsMin = MathNet.Numerics.Statistics.Statistics.Percentile(TwsValues.ToArray(), 5)
    Result.TwsMax = MathNet.Numerics.Statistics.Statistics.Percentile(TwsValues.ToArray(), 95)
    Return Result
  End Function



  Public Enum RowStatus
    Undefined = 0
    UpWindVmg = 1
    DownWindVmg = 2
    Tacking = 3
    Gybing = 4
    Reaching = 5
    UnderMinPerfUpVmg = 6
    UnderMinPerfDnVmg = 7
  End Enum

  Shared Function AngleFromTwa(AbsTwa As Double, UpwindMaxAngle As Double, DownWindMinAngle As Double) As Andatura
    If AbsTwa < UpwindMaxAngle Then Return Andatura.UpwindVmg
    If AbsTwa > DownWindMinAngle Then Return Andatura.DownWindVmg
    Return Andatura.Reaching
  End Function

  Shared Function GetRowId(Row As BoatStatusIndex, Secondi As Integer, Rows As List(Of BoatStatusIndex)) As Integer

    Dim NM As DateTime = Row.DT.AddSeconds(Secondi)
    Dim idt As Integer = DataProvider2020.TrovaIndice(NM)
    If idt < Rows.First.IdRiga Then Return 0
    If idt > Rows.Last.IdRiga Then Return Rows.Count - 1
    Dim id = Rows.Where(Function(x) x.IdRiga = idt).FirstOrDefault
    If Not id Is Nothing Then
      Return Rows.IndexOf(id)
    Else
      Dim segno As Integer = Math.Abs(Secondi) / Secondi
      idt += segno
      Do
        id = Rows.Where(Function(x) x.IdRiga = idt).FirstOrDefault
        If Not id Is Nothing Then
          Return Rows.IndexOf(id)
        End If
        idt += segno
        If idt < 0 Then Return -1
        If idt >= Rows.Count Then Return -1
      Loop
    End If
  End Function


End Class
' ===========================
' File: Statistica.vb
' ===========================
' Utility statistiche in VB.NET:
' - Media, Varianza, Deviazione standard
' - Mediana, Moda
' - Percentile (interpolazione lineare)
' - Valore normale (gaussiano) = media
' Tutte le funzioni lavorano su IEnumerable(Of Double).
' ===========================


Public Module Statistica

#Region "Puro .NET"

  ''' <summary>Media aritmetica.</summary>
  <Extension>
  Public Function GetMedia(source As IEnumerable(Of Double)) As Double
    Dim arr = EnsureArray(source)
    Return arr.Average()
  End Function

  ''' <summary>
  ''' Varianza. Per default è campionaria (denominatore N-1). Metti campionaria:=False per la varianza della popolazione (denominatore N).
  ''' </summary>
  <Extension>
  Public Function GetVarianza(source As IEnumerable(Of Double), Optional campionaria As Boolean = True) As Double
    Dim arr = EnsureArray(source)
    Dim m = arr.Average()
    Dim ss = arr.Sum(Function(x) (x - m) * (x - m))
    Dim denom = If(campionaria, Math.Max(1, arr.Length - 1), arr.Length)
    If denom = 0 Then Throw New InvalidOperationException("Varianza non definita per sequenza di lunghezza 0.")
    Return ss / denom
  End Function

  ''' <summary>Deviazione standard. Per default è campionaria.</summary>
  <Extension>
  Public Function GetDeviazioneStandard(source As IEnumerable(Of Double), Optional campionaria As Boolean = True) As Double
    Return Math.Sqrt(GetVarianza(source, campionaria))
  End Function

  ''' <summary>Mediana (se N è pari, media dei due centrali).</summary>
  <Extension>
  Public Function GetMediana(source As IEnumerable(Of Double)) As Double
    Dim arr = EnsureArray(source).OrderBy(Function(x) x).ToArray()
    Dim n = arr.Length
    If n = 0 Then Throw New ArgumentException("La sequenza è vuota.")
    Dim mid = n \ 2
    If n Mod 2 = 1 Then
      Return arr(mid)
    Else
      Return (arr(mid - 1) + arr(mid)) / 2.0
    End If
  End Function

  ''' <summary>
  ''' Moda (valore più frequente). In caso di più mode con stessa frequenza, restituisce la più piccola.
  ''' Nota: per dati continui potrebbe essere utile un raggruppamento (binning); qui si usa l’uguaglianza esatta.
  ''' </summary>
  <Extension>
  Public Function GetModa(source As IEnumerable(Of Double)) As Double
    Dim arr = EnsureArray(source)
    If arr.Length = 0 Then Throw New ArgumentException("La sequenza è vuota.")
    Return arr.GroupBy(Function(x) x).
                 OrderByDescending(Function(g) g.Count()).
                 ThenBy(Function(g) g.Key).
                 First().Key
  End Function

  ''' <summary>
  ''' Percentile p in [0,100] con interpolazione lineare (metodo tipo "Nearest Rank" interpolato).
  ''' </summary>
  <Extension>
  Public Function GetPercentile(source As IEnumerable(Of Double), p As Double) As Double
    If p < 0 OrElse p > 100 Then Throw New ArgumentOutOfRangeException(NameOf(p), "Il percentile deve essere tra 0 e 100.")
    Dim arr = EnsureArray(source).OrderBy(Function(x) x).ToArray()
    Dim n = arr.Length
    If n = 0 Then Throw New ArgumentException("La sequenza è vuota.")
    If n = 1 OrElse p = 0 Then Return arr(0)
    If p = 100 Then Return arr(n - 1)

    Dim pos = (p / 100.0) * (n - 1)   ' indice reale 0-based
    Dim k = CInt(Math.Floor(pos))
    Dim d = pos - k
    If k >= n - 1 Then
      Return arr(n - 1)
    Else
      Return arr(k) + d * (arr(k + 1) - arr(k))
    End If
  End Function

  ''' <summary>
  ''' Valore normale nel senso gaussiano (valore più probabile in una distribuzione normale) = Media.
  ''' </summary>
  <Extension>
  Public Function ValoreNormaleGaussiano(source As IEnumerable(Of Double)) As Double
    Return GetMedia(source)
  End Function

#End Region

#Region "Math.NET Numerics (opzionale)"
  ' Per usare questa sezione:
  ' 1) Installa il pacchetto: Install-Package MathNet.Numerics
  ' 2) Decommenta gli Imports sotto.

  'Imports MathNet.Numerics.Statistics

  ' <Extension>
  'Public Function MediaMN(source As IEnumerable(Of Double)) As Double
  '    Return MathNet.Numerics.Statistics.Statistics.Mean(source)
  'End Function

  ' <Extension>
  'Public Function VarianzaMN(source As IEnumerable(Of Double)) As Double
  '    ' Restituisce la varianza campionaria.
  '    Return MathNet.Numerics.Statistics.Statistics.Variance(source)
  'End Function

  ' <Extension>
  'Public Function DeviazioneStandardMN(source As IEnumerable(Of Double)) As Double
  '    Return MathNet.Numerics.Statistics.Statistics.StandardDeviation(source)
  'End Function

  ' <Extension>
  'Public Function MedianaMN(source As IEnumerable(Of Double)) As Double
  '    Return MathNet.Numerics.Statistics.Statistics.Median(source)
  'End Function

  ' <Extension>
  'Public Function ModaMN(source As IEnumerable(Of Double)) As Double
  '    Return MathNet.Numerics.Statistics.Statistics.Mode(source)
  'End Function

  ' <Extension>
  'Public Function PercentileMN(source As IEnumerable(Of Double), p As Double) As Double
  '    ' p in [0,100]
  '    Return MathNet.Numerics.Statistics.Statistics.Percentile(source, p)
  'End Function

  ' <Extension>
  'Public Function ValoreNormaleGaussianoMN(source As IEnumerable(Of Double)) As Double
  '    ' Su distribuzione normale: valore più probabile = media
  '    Return MathNet.Numerics.Statistics.Statistics.Mean(source)
  'End Function
#End Region

  ' ---- Helpers ----
  Private Function EnsureArray(source As IEnumerable(Of Double)) As Double()
    If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
    Dim arr = source.ToArray()
    If arr.Length = 0 Then Throw New ArgumentException("La sequenza è vuota.")
    Return arr
  End Function

  ''' <summary>
  ''' Moda robusta per dati continui tramite istogramma con binning.
  ''' - Larghezza bin automatica: Freedman–Diaconis (usa IQR). Fallback a Scott se IQR=0.
  ''' - Se tutto è costante, restituisce quel valore.
  ''' - Rifinitura parabolica usando i 3 bin attorno al picco per una stima sub-bin.
  ''' </summary>
  <Extension>
  Public Function ModaBinned(source As IEnumerable(Of Double), Optional binWidth As Double? = Nothing) As Double
    Dim arr = EnsureArray(source)
    Dim n = arr.Length
    If n = 1 Then Return arr(0)

    Dim minV = arr.Min()
    Dim maxV = arr.Max()
    If minV = maxV Then Return minV

    ' Scegli larghezza bin
    Dim bw As Double
    If binWidth.HasValue AndAlso binWidth.Value > 0 Then
      bw = binWidth.Value
    Else
      ' Freedman–Diaconis: h = 2 * IQR / n^(1/3)
      Dim q75 = GetPercentile(arr, 75)
      Dim q25 = GetPercentile(arr, 25)
      Dim iqr = q75 - q25
      If iqr > 0 Then
        bw = 2.0 * iqr / Math.Pow(n, 1.0 / 3.0)
      Else
        ' Fallback Scott: h = 3.5 * sigma / n^(1/3)
        Dim sigma = GetDeviazioneStandard(arr, campionaria:=False)
        If sigma > 0 Then
          bw = 3.5 * sigma / Math.Pow(n, 1.0 / 3.0)
        Else
          ' Fallback finale: 10 bin
          bw = (maxV - minV) / 10.0
        End If
      End If
    End If

    If bw <= 0 Then bw = (maxV - minV) / 10.0
    If bw <= 0 Then Return minV ' estrema difesa

    Dim nb = CInt(Math.Ceiling((maxV - minV) / bw))
    If nb < 1 Then nb = 1

    Dim counts = New Integer(nb - 1) {}
    ' Popola istogramma
    For Each x In arr
      Dim idx = CInt(Math.Floor((x - minV) / bw))
      If idx < 0 Then idx = 0
      If idx >= nb Then idx = nb - 1 ' include il max nell’ultimo bin
      counts(idx) += 1
    Next

    ' Trova bin di picco
    Dim k = 0
    Dim cmax = counts(0)
    For i = 1 To nb - 1
      If counts(i) > cmax Then
        cmax = counts(i)
        k = i
      End If
    Next

    ' Centro del bin di picco
    Dim centerK = minV + (k + 0.5) * bw

    ' Rifinitura parabolica: usa i 3 bin (k-1, k, k+1) se disponibili
    If k > 0 AndAlso k < nb - 1 Then
      Dim c0 = counts(k - 1)
      Dim c1 = counts(k)
      Dim c2 = counts(k + 1)
      Dim denom = (c0 - 2.0 * c1 + c2)
      If denom <> 0 Then
        ' Spostamento in unità di bin dal centro di k, clamp in [-0.5, 0.5]
        Dim delta = 0.5 * (c0 - c2) / denom
        If delta > 0.5 Then delta = 0.5
        If delta < -0.5 Then delta = -0.5
        Return centerK + delta * bw
      End If
    End If

    Return centerK
  End Function


End Module

