Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Parquet.File.Values.Primitives
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineTable
  'Implements INotifyPropertyChanged

  'Dim pPolareRiferimento As clsPolare2019 = Nothing
  Public Property PeriodiAdvanced As New ObservableCollection(Of clsPeriodoAdvanced)
  Public Property PeriodiOrdinati As List(Of clsPeriodoAdvanced)

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  'Public Property PolareRiferimento As clsPolare2019
  '  Get
  '    Return pPolareRiferimento
  '  End Get
  '  Set(value As clsPolare2019)
  '    pPolareRiferimento = value
  '  End Set
  'End Property

  'Public Property PeriodiAdvanced As ObservableCollection(Of clsPeriodoAdvanced)
  '  Get
  '    Return pPeriodiAdvanced
  '  End Get
  '  Set(value As ObservableCollection(Of clsPeriodoAdvanced))
  '    pPeriodiAdvanced = value
  '  End Set
  'End Property

  'Public ReadOnly Property PeriodiOrdinati As List(Of clsPeriodoAdvanced)
  '  Get
  '    Return pPeriodiOrdinati
  '  End Get
  'End Property

  Public Sub ImpostaCanaliTest(Canali As List(Of clsChannel2020), Periodi As List(Of clsPeriod2021), NumeroColori As Integer)
    'il primo canale é quello di riferimento
    PeriodiAdvanced.Clear()
    For Each periodo In Periodi
      Dim CT As New clsPeriodoAdvanced(Canali, periodo, NumeroColori)
      PeriodiAdvanced.Add(CT)
    Next
    PeriodiOrdinati = PeriodiAdvanced.OrderByDescending(Function(x) x.Canali.First.Valori.Avg).ToList

    Dim AvgMax(Canali.Count - 1) As Double
    Dim AvgMin(Canali.Count - 1) As Double

    If PeriodiAdvanced.Count > 0 Then
      For i As Integer = 0 To PeriodiAdvanced.First.Canali.Count - 1
        If PeriodiAdvanced.Count > 1 Then
          Dim ValoriMedi(PeriodiAdvanced.Count - 1) As Double
          For ii As Integer = 0 To PeriodiAdvanced.Count - 1
            ValoriMedi(ii) = PeriodiAdvanced(ii).Canali(i).Valori.Avg
          Next
          Dim Max As Double = ValoriMedi.Max
          Dim Min As Double = ValoriMedi.Min

          For ii As Integer = 0 To PeriodiAdvanced.Count - 1
            Dim Valore As Double = PeriodiAdvanced(ii).Canali(i).Valori.Avg
            Dim IndiceColore As Double = 0.5
            If Not Max = Min Then IndiceColore = (Valore - Min) / (Max - Min)
            If IndiceColore = Double.NaN Then
              PeriodiAdvanced(ii).Canali(i).Colore = ColoreBeneMale(255, 0.5)
            Else
              PeriodiAdvanced(ii).Canali(i).Colore = ColoreBeneMale(255, IndiceColore)
            End If
          Next
        Else
          PeriodiAdvanced.First.Canali(i).Colore = ColoreBeneMale(255, 0.5)
        End If
      Next
    End If

  End Sub

End Class



Public Class clsPeriodoAdvanced
  Dim pCanali As New List(Of clsCanaleTest)
  Dim pPeriodo As clsPeriod2021

  Public Sub New(Canali As List(Of clsChannel2020), Periodo As clsPeriod2021, NumeroColori As Integer)
    pPeriodo = Periodo
    For Each Canale In Canali
      If Not Canale Is Nothing Then
        pCanali.Add(New clsCanaleTest(Canale, Periodo))
      End If
    Next

  End Sub

  Public ReadOnly Property Colore(Indice As Integer) As Color
    Get
      Return pCanali(Indice).Colore
    End Get
  End Property

  Public ReadOnly Property ColoreBackGround(Indice As Integer) As SolidColorBrush
    Get
      Return New SolidColorBrush(pCanali(Indice).Colore)
    End Get
  End Property


  Public Property Canali As List(Of clsCanaleTest)
    Get
      Return pCanali
    End Get
    Set(value As List(Of clsCanaleTest))
      pCanali = value
    End Set
  End Property
End Class


Public Class clsCanaleTest
  Dim pCanale As clsChannel2020
  Dim pPeriodo As clsPeriod2021
  Dim pValori As clsValoriPeriodoCanale2020
  Dim pColore As Color

  Public Sub New(Canale As clsChannel2020, Periodo As clsPeriod2021)
    pCanale = Canale
    pPeriodo = Periodo
    pValori = New clsValoriPeriodoCanale2020(Canale, Periodo.TR, False)
  End Sub

  Public Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
    Set(value As clsChannel2020)
      pCanale = value
    End Set
  End Property

  Public Property Periodo As clsPeriod2021
    Get
      Return pPeriodo
    End Get
    Set(value As clsPeriod2021)
      pPeriodo = value
    End Set
  End Property

  Public Property Valori As clsValoriPeriodoCanale2020
    Get
      Return pValori
    End Get
    Set(value As clsValoriPeriodoCanale2020)
      pValori = value
    End Set
  End Property

  Public Property Colore As Color
    Get
      Return pColore
    End Get
    Set(value As Color)
      pColore = value
    End Set
  End Property



End Class

'Public Class clsStraightLines
'  Implements INotifyPropertyChanged

'  Dim pListaStraightLines As New ObservableCollection(Of clsStraightLine)
'  Dim pStraightLineTable As New clsStraightLineTable
'  Dim pListaCanaliTabella As New List(Of clsChannel2020)
'  Dim pListaPeriodi As New List(Of clsPeriod2021)
'  Dim pSelettoreCanali As New clsSelezionaCanali

'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Public Property ListaStraightLines As ObservableCollection(Of clsStraightLine)
'    Get
'      Return pListaStraightLines
'    End Get
'    Set(value As ObservableCollection(Of clsStraightLine))
'      pListaStraightLines = value
'      OnPropertyChanged("ListaStraightLines")
'    End Set
'  End Property

'  Public ReadOnly Property CheckedStraightLines As List(Of clsStraightLine)
'    Get
'      Dim ListaTmp As New List(Of clsStraightLine)
'      For Each Pav In pListaStraightLines
'        If Pav.Periodo.IsChecked Then
'          ListaTmp.Add(Pav)
'        End If
'      Next
'      Return ListaTmp
'    End Get
'  End Property

'  Public Property StraightLineTable As clsStraightLineTable
'    Get
'      Return pStraightLineTable
'    End Get
'    Set(value As clsStraightLineTable)
'      pStraightLineTable = value
'      OnPropertyChanged("StraightLineTable")
'    End Set
'  End Property

'  Public Sub EliminaStraightLine(Periodo As clsPeriod2021)
'    For Each StrLine In pListaStraightLines
'      If StrLine.Periodo Is Periodo Then
'        ListaStraightLines.Remove(StrLine)
'        Exit For
'      End If
'    Next

'  End Sub

'  Public Sub AggiornaColori()
'    'Dim mColori As List(Of Color) = ScalaColoriScuri(pListaStraightLines.Count)
'    For i As Integer = 0 To pListaStraightLines.Count - 1
'      pListaPeriod.Periodo.Colore = ColoriDifferenziati(i)
'    Next
'  End Sub

'  Public Sub AggiungiStraightLine(Periodo As clsPeriod2021)
'    Dim Elemento As New clsStraightLine(Periodo)
'    'Periodo.AggiornaTimeRangeDescription()
'    ListaStraightLines.Add(Elemento)
'    If pListaCanaliTabella.Count = 0 Then
'      pListaCanaliTabella = pSelettoreCanali.CaricaListaCorrente(clsSelezionaCanali.eTipo.eStraightLineDataGridTable)
'      'ImpostacanaliStraightLineTable()
'    End If
'  End Sub

'  Public Sub AggiornaTabellaCanaliPeriodi()
'    pListaPeriodi.Clear()
'    For Each A As clsStraightLine In CheckedStraightLines
'      pListaPeriodi.Add(A.Periodo)
'    Next
'    StraightLineTable.ImpostaCanaliTest(pListaCanaliTabella, pListaPeriodi, 10)
'  End Sub

'  Public Sub AggiornaTabellaCanali()
'    pListaCanaliTabella = pSelettoreCanali.GestisciLista(clsSelezionaCanali.eTipo.eStraightLineDataGridTable, "Straight Line Report Channel Selector")
'    If pListaCanaliTabella Is Nothing Then Exit Sub
'    If pListaPeriodi.Count = 0 Then
'      For Each A As clsStraightLine In CheckedStraightLines
'        pListaPeriodi.Add(A.Periodo)
'      Next
'    End If
'    StraightLineTable.ImpostaCanaliTest(pListaCanaliTabella, pListaPeriodi, 10)
'  End Sub

'End Class





<AddINotifyPropertyChangedInterface>
Public Class clsStraightLine
  'Implements INotifyPropertyChanged
  Public Property Periodo As clsPeriod2021
  Public Property Descrizione As String
  'Dim pVmgTp As clsValoriPeriodoCanale2020
  'Dim pBsTp As clsValoriPeriodoCanale2020

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub


  'Public ReadOnly Property IDprimaRiga As Integer
  '  Get
  '    Return pPeriodo.TR.IdRigaIniziale
  '  End Get
  'End Property

  'Public ReadOnly Property IDultimaRiga As Integer
  '  Get
  '    Return pPeriodo.TR.IdRigaFinale
  '  End Get
  'End Property

  Public Sub New(Periodo As clsPeriod2021)
    Me.Periodo = Periodo
    AggiornaDescrizione()
    'AggiornaValoriPercentage()
  End Sub

  'Private Sub AggiornaValoriPercentage()
  '  Dim ChVmgTp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
  '  'Dim ChBsTp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
  '  If Not ChVmgTp Is Nothing Then
  '    pVmgTp = New clsValoriPeriodoCanale2020(ChVmgTp, pPeriodo.TR, True)
  '  End If
  '  'If Not ChBsTp Is Nothing Then
  '  '  pBsTp = New clsValoriPeriodoCanale2020(ChBsTp, pPeriodo.TR, False)
  '  'End If
  'End Sub

  Private Sub AggiornaDescrizione()
    Descrizione = Periodo.TR.Start.ToLongTimeString & " (" & Periodo.TR.DurataInStringaConSeparatore & "), Tws:" & Format(Periodo.TwsDetails.AvgVal, "F1") & ", Twa:" & Format(Periodo.StraightLineVmgDetails.Twa.AvgVal, "F0") & ""
  End Sub

  'Public Property Periodo As clsPeriod2021
  '  Get
  '    Return pPeriodo
  '  End Get
  '  Set(value As clsPeriod2021)
  '    pPeriodo = value
  '    AggiornaDescrizione()
  '    OnPropertyChanged("Periodo")
  '  End Set
  'End Property

  'Public Property Descrizione As String
  '  Get
  '    Return pDescrizione
  '  End Get
  '  Set(value As String)
  '    pDescrizione = value
  '    OnPropertyChanged("Descrizione")
  '  End Set
  'End Property

  'Public Property VmgTp As clsValoriPeriodoCanale2020
  '  Get
  '    Return pVmgTp
  '  End Get
  '  Set(value As clsValoriPeriodoCanale2020)
  '    pVmgTp = value
  '  End Set
  'End Property

  'Public Property BsTp As clsValoriPeriodoCanale2020
  '  Get
  '    Return pBsTp
  '  End Get
  '  Set(value As clsValoriPeriodoCanale2020)
  '    pBsTp = value
  '  End Set
  'End Property
End Class

Public Class PuntoGroupByTack
  Public Property Xval As Double
  Public Property Yval As Double
  Public Property IsStbd As Boolean
  Public Property IsUp As Boolean

  Public Sub New(Xval As Double, Yval As Double, IsStbd As Boolean, IsUp As Boolean)
    Me.Xval = Xval
    Me.Yval = Yval
    Me.IsStbd = IsStbd
    Me.IsUp = IsUp
  End Sub

End Class



Public Class clsStraightLineStandardPlotViewModelXY
  Inherits clsStraightLineStandardPlotViewModel

  Public Sub New(ChAscissa As clsChannel2020, ChOrdinata As clsChannel2020, TitoloGrafico As String, ChartSync As clsStraightLineChartSync, ParentVM As clsStraightLineVM2020)
    MyBase.New(ChAscissa, ChOrdinata, TitoloGrafico, ChartSync, ParentVM)
  End Sub

  Public Overrides Sub DrawChart(StraightLines As List(Of clsPeriod2021))

  End Sub


  Public Overrides Sub DrawChart(StraightLines As List(Of clsPeriod2021), OutputType As clsStraightLineVM2020.eOutputType)




    SeriesSource.Clear()
    'Dim pColori As List(Of Color) = ScalaColori(StraightLines.Count)
    If CanaleAscissa Is Nothing Then Exit Sub
    If CanaleOrdinata Is Nothing Then Exit Sub

    Dim Canale As clsChannel2020 = CanaleOrdinata
    Dim CanaleOpposite As clsChannel2020 = CanaleOrdinata
    If CanaleOrdinata.ChannelId.IndexOf("Port") > -1 Then
      ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
      CanaleOpposite = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
    ElseIf CanaleOrdinata.ChannelId.IndexOf("Stbd") > -1 Then
      ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
      CanaleOpposite = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
    End If

    If CanaleOpposite Is Nothing Then
      ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
      CanaleOpposite = CanaleOrdinata
    End If

    'Dim xx As New List(Of Double)
    'Dim yy As New List(Of Double)

    Dim MinX As Double = Nothing
    Dim MaxX As Double = Nothing
    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False
    Dim TL As New clsTrendLines

    ' --- strumentazione temporanea: separa il costo delle statistiche da quello
    '     della creazione degli oggetti grafici (una serie per periodo)
    Dim MsStat As Double = 0
    Dim MsSerie As Double = 0
    Dim NPeriodi As Integer = 0
    Dim TmrDC As DateTime = Now
    Dim TmrX As DateTime

    Dim Chiavi As New List(Of String)
    For Each Period In StraightLines
      Dim chiave As String = Period.Keys.Trim
      If chiave = "" Then
        chiave = "empty"
      End If
      If Not Chiavi.Contains(chiave) Then
        Chiavi.Add(chiave)
      End If
    Next


    For Each Period In StraightLines ' PeriodsManager.ListaStraightLineVmg

      Dim Carica As Boolean = True
      If OutputType = clsStraightLineVM2020.eOutputType.eGroupByTack Then Carica = Period.IsChecked

      If Carica Then

        If Not Period.StraightLineVmgDetails Is Nothing Then
          AtLeastOneUp = AtLeastOneUp OrElse (Period.IsChecked AndAlso Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30)))
          AtLeastOneDn = AtLeastOneDn OrElse (Period.IsChecked AndAlso Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(160, 120)))
        End If

        'Dim StraightLine As clsStraightLine = Period.DettagliStraightLine
        If Period.IsStbd Then
          Canale = CanaleOrdinata
        Else
          Canale = CanaleOpposite
        End If
        'Dim Colore As System.Windows.Media.Color = pColori(i)
        TmrX = Now
        Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
        Dim LineaTmp As New XyScatterRenderableSeries
        LineaTmp.XAxisId = "DefaultAxisId"
        LineaTmp.YAxisId = "DefaultAxisId"


        If Period.IsStbd Then
          LineaTmp.PointMarker = New EllipsePointMarker
        Else
          LineaTmp.PointMarker = New SquarePointMarker
        End If

        Select Case OutputType
          Case clsStraightLineVM2020.eOutputType.eColorByTack
            If Period.IsStbd Then
              LineaTmp.PointMarker.Stroke = Colors.Green
              LineaTmp.PointMarker.Fill = Colors.Green
            Else
              LineaTmp.PointMarker.Stroke = Colors.Red
              LineaTmp.PointMarker.Fill = Colors.Red
            End If
          'Case clsStraightLineVM2020.eOutputType.eColorByTackUpDn
          '  If Period.IsStbd Then
          '    If Period.IsUpwindVmgRange Then
          '      LineaTmp.PointMarker.Stroke = Colors.Green
          '      LineaTmp.PointMarker.Fill = Colors.Green
          '    ElseIf Period.IsUpwindVmgRange Then
          '      LineaTmp.PointMarker.Stroke = Colors.LimeGreen
          '      LineaTmp.PointMarker.Fill = Colors.LimeGreen
          '    Else
          '      LineaTmp.PointMarker.Stroke = Colors.GreenYellow
          '      LineaTmp.PointMarker.Fill = Colors.GreenYellow
          '    End If
          '  Else
          '    If Period.IsUpwindVmgRange Then
          '      LineaTmp.PointMarker.Stroke = Colors.Red
          '      LineaTmp.PointMarker.Fill = Colors.Red
          '    ElseIf Period.IsUpwindVmgRange Then
          '      LineaTmp.PointMarker.Stroke = Colors.OrangeRed
          '      LineaTmp.PointMarker.Fill = Colors.OrangeRed
          '    Else
          '      LineaTmp.PointMarker.Stroke = Colors.IndianRed
          '      LineaTmp.PointMarker.Fill = Colors.IndianRed
          '    End If
          '  End If
          Case clsStraightLineVM2020.eOutputType.eColorByKey
            LineaTmp.PointMarker.Stroke = ColoreDaOutputType(Period, OutputType)
            LineaTmp.PointMarker.Fill = ColoreDaOutputType(Period, OutputType)
          Case clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
            LineaTmp.PointMarker.Stroke = ColoreDaOutputType(Period, OutputType)
            LineaTmp.PointMarker.Fill = ColoreDaOutputType(Period, OutputType)
          Case Else
            LineaTmp.PointMarker.Stroke = Period.Colore
            LineaTmp.PointMarker.Fill = Period.Colore
        End Select



        LineaTmp.PointMarker.Height = 10
        LineaTmp.PointMarker.Width = 10
        LineaTmp.PointMarker.StrokeThickness = 1
        LineaTmp.Tag = Period
        LineaTmp.IsVisible = Period.IsChecked
        DataSeriesTMP.AcceptsUnsortedData = True
        MsSerie += Now.Subtract(TmrX).TotalMilliseconds

        TmrX = Now
        Dim MedieAscissa As New clsValoriPeriodoCanale2020(CanaleAscissa, Period.TR, CanaleAscissa.DataType = clsChannel2020.eDataType.e180)
        MedieAscissa.AggiornaValori(CanaleAscissa.DataType = clsChannel2020.eDataType.e180)
        Dim MedieOrdinata As New clsValoriPeriodoCanale2020(Canale, Period.TR, Canale.DataType = clsChannel2020.eDataType.e180)
        MedieOrdinata.AggiornaValori(CanaleAscissa.DataType = clsChannel2020.eDataType.e180)
        MsStat += Now.Subtract(TmrX).TotalMilliseconds
        NPeriodi += 1
        Dim X As Double = MedieAscissa.Avg
        If X = 0 Then Stop
        Dim Y As Double = MedieOrdinata.Avg

        If MinX = Nothing Then
          MinX = X
        Else
          MinX = System.Math.Min(MinX, X)
        End If
        If MaxX = Nothing Then
          MaxX = X
        Else
          MaxX = System.Math.Max(MaxX, X)
        End If
        If CanaleOrdinata Is CanaleAscissa Then
          Y = Period.TR.Durata.TotalMinutes
          TitoloPlot = "Minutes"
        End If
        If OutputType = clsStraightLineVM2020.eOutputType.eColorByKey Then
          If Chiavi.Count = 1 OrElse Not Period.Keys.Trim = "" Then
            DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
          End If
        Else
          DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
        End If
        If Not Period.StraightLineVmgDetails Is Nothing Then
          TL.AccodaCoppia(Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30)), Period.IsStbd, X, Y)
        End If
        'xx.Add(X)
        'yy.Add(Y)
        TmrX = Now
        LineaTmp.DataSeries = DataSeriesTMP
        Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
        'If Period.IsChecked Then
        SeriesSource.Add(CSVMtmp)
        'End If
        MsSerie += Now.Subtract(TmrX).TotalMilliseconds
      End If
    Next
    If dbg Then Console.WriteLine("     DC [" & TitoloPlot & "] periodi:" & NPeriodi &
                                  " statistiche:" & MsStat.ToString("F0") & " ms" &
                                  " serie+marker:" & MsSerie.ToString("F0") & " ms" &
                                  " totale:" & Now.Subtract(TmrDC).TotalMilliseconds.ToString("F0") & " ms")
    MaxX = System.Math.Max(CInt(MinX + 1), MaxX)
    MinX = System.Math.Min(CInt(MaxX - 2), MinX)
    Dim TwsRange As New DoubleRange(MinX, MaxX)
    StraightLineChartSync.SharedXVisibleRange = TwsRange

    If Not CanaleOrdinata Is CanaleAscissa Then
      TL.StampaTrendLines(4, SeriesSource, ParentVM)
    End If

    If ParentVM.PlotTargetsIfAvailable Then
      DisegnaTarget(CanaleOrdinata.PolarHeader, AtLeastOneUp, AtLeastOneDn, MinX, MaxX)
      TwsRange = New DoubleRange(Int(MinX), Int(MaxX + 0.5) + 1)
      StraightLineChartSync.SharedXVisibleRange = TwsRange
    End If


    'ShowLegend = OutputType = clsStraightLineVM2020.eOutputType.eColorByKey AndAlso CanaleAscissa Is CanaleOrdinata
    ShowLegend = False
    'Dim p As MathNet.Numerics.Polynomial
    'Dim x As Double()
    'Dim y As Double()
    'Stop
  End Sub



  Public Sub DisegnaTarget(PolarHeader As String, AtLeastOneUp As Boolean, AtLeastOneDn As Boolean, MinVal As Double, MaxVal As Double)
    If Not TgtManager.Tgt Is Nothing Then

      Dim tgt As clsChannelTarget = TgtManager.Tgt.Polare(PolarHeader)
      If Not tgt Is Nothing Then

        Dim DataSeriesTgtUp As New XyDataSeries(Of Double, Double)
        Dim LineaTgtUp As New FastLineRenderableSeries
        LineaTgtUp.XAxisId = "DefaultAxisId"
        LineaTgtUp.YAxisId = "DefaultAxisId"
        LineaTgtUp.Stroke = Colors.DarkRed
        LineaTgtUp.StrokeThickness = 4
        'LineaTgtUp.StrokeDashArray = {2, 2}
        LineaTgtUp.Tag = "TgtUp"
        LineaTgtUp.IsVisible = AtLeastOneUp
        DataSeriesTgtUp.AcceptsUnsortedData = True

        Dim DataSeriesTgtDn As New XyDataSeries(Of Double, Double)
        Dim LineaTgtDn As New FastLineRenderableSeries
        LineaTgtDn.XAxisId = "DefaultAxisId"
        LineaTgtDn.YAxisId = "DefaultAxisId"
        LineaTgtDn.Stroke = Colors.DarkBlue
        LineaTgtDn.StrokeThickness = 4
        'LineaTgtDn.StrokeDashArray = {2, 2}
        LineaTgtDn.Tag = "TgtDn"
        LineaTgtDn.IsVisible = AtLeastOneDn
        DataSeriesTgtDn.AcceptsUnsortedData = True

        For Each row In tgt.Rows
          Dim x As Double = row.RowValue
          If x >= Int(MinVal) AndAlso x <= (Int(MaxVal + 0.5) + 1) Then
            Dim yUp As Double = row.TargetValue(True)
            Dim yDn As Double = row.TargetValue(False)
            DataSeriesTgtUp.Append(x, yUp, New clsPuntoMetadata(False))
            DataSeriesTgtDn.Append(x, yDn, New clsPuntoMetadata(False))
          End If
        Next
        LineaTgtUp.DataSeries = DataSeriesTgtUp
        Dim CSVMtgtup As New ChartSeriesViewModel(DataSeriesTgtUp, LineaTgtUp)
        SeriesSource.Add(CSVMtgtup)
        LineaTgtDn.DataSeries = DataSeriesTgtDn
        Dim CSVMtgtDn As New ChartSeriesViewModel(DataSeriesTgtDn, LineaTgtDn)
        SeriesSource.Add(CSVMtgtDn)
      End If
    End If
  End Sub


  Public Overrides Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021), UpDn As Boolean, Tack As Boolean)

    SeriesSource.Clear()
    'Dim pColori As List(Of Color) = ScalaColori(StraightLines.Count)
    If CanaleAscissa Is Nothing Then Exit Sub
    If CanaleOrdinata Is Nothing Then Exit Sub
    Dim GBT As New List(Of PuntoGroupByTack)

    If CanaleOrdinata Is CanaleAscissa Then
      TitoloPlot = "Minutes"
    End If
    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False
    For Each Period In StraightLines ' PeriodsManager.ListaStraightLineVmg
      If Period.IsChecked Then
        AtLeastOneUp = AtLeastOneUp OrElse (Period.IsChecked AndAlso Period.IsUpwindVmgRange)
        AtLeastOneDn = AtLeastOneDn OrElse (Period.IsChecked AndAlso Period.IsDownwindVmgRange)
        For i As Integer = Period.TR.IdRigaIniziale To Period.TR.IdRigaFinale
          If Not Double.IsNaN(CanaleOrdinata.Valori(i)) Then
            If CanaleOrdinata Is CanaleAscissa Then
              GBT.Add(New PuntoGroupByTack(CanaleAscissa.Valori(i), Period.TR.Durata.TotalMinutes, Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))
            Else
              Dim y = CanaleOrdinata.Valori(i)
              Select Case CanaleOrdinata.DataType
                Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                  y = System.Math.Abs(y)
              End Select
              GBT.Add(New PuntoGroupByTack(CanaleAscissa.Valori(i), y, Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))
            End If
          End If
        Next
      End If
    Next

    Dim inizio As Integer = 0
    Dim fine As Integer = 1
    If Tack Then
      If UpDn Then
        inizio = 2
        fine = 5
      End If
    Else
      If UpDn Then
        inizio = 6
        fine = 7
      Else
        inizio = 8
        fine = 8
      End If
    End If

    Dim TwsVals As New List(Of Double)

    For i As Integer = inizio To fine
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastLineRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.PointMarker = New EllipsePointMarker
      Linea.PointMarker.Height = 10
      Linea.PointMarker.Width = 10
      Linea.PointMarker.StrokeThickness = 1
      Dim Punti As List(Of PuntoGroupByTack)
      Select Case i
        Case 0
          Punti = GBT.Where(Function(x) x.IsStbd = False).ToList
          Linea.PointMarker.Stroke = Colors.Red
          Linea.Tag = -1
          DataSeries.SeriesName = "Port"
        Case 1
          Punti = GBT.Where(Function(x) x.IsStbd = True).ToList
          Linea.PointMarker.Stroke = Colors.Green
          Linea.Tag = 1
          DataSeries.SeriesName = "Stbd"
        Case 2
          Punti = GBT.Where(Function(x) Not x.IsStbd AndAlso x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.Red
          Linea.Tag = -1
          DataSeries.SeriesName = "UpPort"
        Case 3
          Punti = GBT.Where(Function(x) x.IsStbd AndAlso x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.Green
          Linea.Tag = 1
          DataSeries.SeriesName = "UpStbd"
        Case 4
          Punti = GBT.Where(Function(x) Not x.IsStbd AndAlso Not x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.Gold
          Linea.Tag = -2
          DataSeries.SeriesName = "DnPort"
        Case 5
          Punti = GBT.Where(Function(x) x.IsStbd AndAlso Not x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.RoyalBlue
          Linea.Tag = 2
          DataSeries.SeriesName = "DnStbd"
        Case 6
          Punti = GBT.Where(Function(x) x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.Gold
          Linea.Tag = -3
          DataSeries.SeriesName = "Upwind"
        Case 7
          Punti = GBT.Where(Function(x) Not x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.RoyalBlue
          Linea.Tag = 3
          DataSeries.SeriesName = "Downwind"
        Case 8
          Punti = GBT.Where(Function(x) x.IsUp).ToList
          Linea.PointMarker.Stroke = Colors.RoyalBlue
          Linea.Tag = 10
          DataSeries.SeriesName = "AllTogether"
      End Select
      Linea.PointMarker.Fill = Linea.PointMarker.Stroke
      If Punti.Count > 0 Then
        Dim vX As Double = Punti.Select(Function(x) x.Xval).Average
        Dim vY As Double = Punti.Select(Function(x) x.Yval).Average
        TwsVals.Add(vX)
        DataSeries.Append(vX, vY, New clsPuntoMetadata(False))
        Linea.DataSeries = DataSeries
        Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
        SeriesSource.Add(CSVM)
      End If
    Next

    If ParentVM.PlotTargetsIfAvailable Then DisegnaTarget(CanaleOrdinata.PolarHeader, AtLeastOneUp, AtLeastOneDn, TwsVals.Min, TwsVals.Max)
    Dim TwsRange As New DoubleRange(Int(TwsVals.Min), Int(TwsVals.Max + 0.5) + 1)
    StraightLineChartSync.SharedXVisibleRange = TwsRange

    ShowLegend = (CanaleOrdinata.ChannelId = CanaleAscissa.ChannelId)
  End Sub



  Public Overrides Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021))

    SeriesSource.Clear()
    'Dim pColori As List(Of Color) = ScalaColori(StraightLines.Count)
    If CanaleAscissa Is Nothing Then Exit Sub
    If CanaleOrdinata Is Nothing Then Exit Sub
    Dim MinX As Double = Nothing
    Dim MaxX As Double = Nothing
    Dim TwsPort As New List(Of Double)
    Dim TwsStbd As New List(Of Double)
    Dim ValsPort As New List(Of Double)
    Dim ValsStbd As New List(Of Double)

    Dim GBT As New List(Of PuntoGroupByTack)

    If CanaleOrdinata Is CanaleAscissa Then
      TitoloPlot = "Minutes"
    End If
    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False
    For Each Period In StraightLines ' PeriodsManager.ListaStraightLineVmg
      If Period.IsChecked Then
        AtLeastOneUp = AtLeastOneUp OrElse (Period.IsChecked AndAlso Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30)))
        AtLeastOneDn = AtLeastOneDn OrElse (Period.IsChecked AndAlso Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(160, 120)))
        If Period.IsStbd Then
          TwsStbd.AddRange(DataProvider2020.ValoriIntervallo(CanaleAscissa.Valori, Period.TR))
          If CanaleOrdinata Is CanaleAscissa Then
            ValsStbd.Add(Period.TR.Durata.TotalMinutes)
          Else
            Dim Rtmp As List(Of Double) = DataProvider2020.ValoriIntervallo(CanaleOrdinata.Valori, Period.TR).ToList
            If CanaleOrdinata.DataType = clsChannel2020.eDataType.e180 Then
              ValsStbd.AddRange(Rtmp.Select(Function(x) System.Math.Abs(x)).ToList)
            Else
              ValsStbd.AddRange(Rtmp)
            End If
          End If
        Else
          TwsPort.AddRange(DataProvider2020.ValoriIntervallo(CanaleAscissa.Valori, Period.TR))
          If CanaleOrdinata Is CanaleAscissa Then
            ValsPort.Add(Period.TR.Durata.TotalMinutes)
          Else
            Dim Rtmp As List(Of Double) = DataProvider2020.ValoriIntervallo(CanaleOrdinata.Valori, Period.TR).ToList
            If CanaleOrdinata.DataType = clsChannel2020.eDataType.e180 Then
              ValsPort.AddRange(Rtmp.Select(Function(x) System.Math.Abs(x)).ToList)
            Else
              ValsPort.AddRange(Rtmp)
            End If
          End If
        End If
      End If
    Next

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    Dim LineaPort As New FastLineRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.PointMarker = New EllipsePointMarker
    LineaPort.PointMarker.Stroke = Colors.Red
    LineaPort.PointMarker.Height = 10
    LineaPort.PointMarker.Width = 10
    LineaPort.PointMarker.StrokeThickness = 1
    LineaPort.PointMarker.Fill = Colors.Red


    'LineaPort.Stroke = Colors.Red
    'LineaPort.StrokeThickness = 2
    LineaPort.Tag = -1
    Dim TwsP = TwsPort.Where(Function(x) Not Double.IsNaN(x))
    Dim ValsP = ValsPort.Where(Function(x) Not Double.IsNaN(x))
    If TwsP.Count > 0 AndAlso ValsP.Count > 0 Then
      DataSeriesPort.Append(TwsP.Average, ValsP.Average, New clsPuntoMetadata(False))
      LineaPort.DataSeries = DataSeriesPort
      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
      SeriesSource.Add(CSVMport)
    End If


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim LineaStbd As New FastLineRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.PointMarker = New EllipsePointMarker
    LineaStbd.PointMarker.Stroke = Colors.Green
    LineaStbd.PointMarker.Height = 10
    LineaStbd.PointMarker.Width = 10
    LineaStbd.PointMarker.StrokeThickness = 1
    LineaStbd.PointMarker.Fill = Colors.Green
    'LineaStbd.Stroke = Colors.Green
    'LineaStbd.StrokeThickness = 2
    LineaStbd.Tag = 1
    Dim TwsS = TwsStbd.Where(Function(x) Not Double.IsNaN(x))
    Dim ValsS = ValsStbd.Where(Function(x) Not Double.IsNaN(x))
    If TwsS.Count > 0 AndAlso ValsS.Count > 0 Then
      'If TwsStbd.Count > 0 Then
      DataSeriesStbd.Append(TwsS.Average, ValsS.Average, New clsPuntoMetadata(False))
      LineaStbd.DataSeries = DataSeriesStbd
      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
      SeriesSource.Add(CSVMStbd)
    End If



    If TwsS.Count = 0 AndAlso TwsP.Count = 0 Then
      MaxX = 0
      MinX = 0
    ElseIf TwsS.Count = 0 Then
      MaxX = TwsP.Max
      MinX = TwsP.Min
    ElseIf TwsP.Count = 0 Then
      MaxX = TwsS.Max
      MinX = TwsS.Min
    Else
      MaxX = System.Math.Max(TwsP.Max, TwsS.Max)
      MinX = System.Math.Min(TwsP.Min, TwsS.Min)
    End If

    'MaxX = System.Math.Max(CInt(MinX + 1), MaxX)
    'MinX = System.Math.Min(CInt(MaxX - 2), MinX)
    MaxX = CInt(MaxX) + 1
    MinX = CInt(MinX) - 1

    If ParentVM.PlotTargetsIfAvailable Then DisegnaTarget(CanaleOrdinata.PolarHeader, AtLeastOneUp, AtLeastOneDn, MinX, MaxX)
    Dim TwsRange As New DoubleRange(MinX, MaxX)
    StraightLineChartSync.SharedXVisibleRange = TwsRange

    ShowLegend = False


    'If Not CanaleOrdinata Is CanaleAscissa Then
    '  TL.StampaTrendLines(4, SeriesSource, ParentVM.StraightLinesSelectionSettings)
    'End If



    'OnPropertyChanged("SeriesSource")
  End Sub

  Public Overrides Sub DrawChartGroupByKeys(StraightLines As List(Of clsPeriod2021))
    'DrawChartGroupByTack(StraightLines)

    SeriesSource.Clear()
    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0
    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False

    For Each Period In StraightLines
      If Period.IsChecked Then
        Dim Chiave As String = Period.Keys
        AtLeastOneUp = AtLeastOneUp OrElse Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(60, 30))
        AtLeastOneDn = AtLeastOneDn OrElse Period.StraightLineVmgDetails.IsTWAinRange(New clsDoubleRange(160, 120))
        If Chiave.Trim = "" Then
          Chiave = "empty"
        End If
        If Not GrId.ContainsKey(Chiave) Then
          GrId.Add(Chiave, contatore)
          contatore += 1
        End If
        Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
        If Period.IsStbd Then
          'If ParentVM.OutputType = clsStraightLineVM2020.eOutputType.eGroupByTackAndKey Then
          '  Chiave = "Stbd " & Chiave
          'End If
          Mure = clsGruppoPeriodi.eMure.eStbd
        Else
          'If ParentVM.OutputType = clsStraightLineVM2020.eOutputType.eGroupByTackAndKey Then
          '  Chiave = "Port " & Chiave
          'End If
          Mure = clsGruppoPeriodi.eMure.ePort
        End If

        Dim Canale As clsChannel2020 = CanaleOrdinata
        Dim CanaleOpposite As clsChannel2020 = CanaleOrdinata
        If CanaleOrdinata.ChannelId.IndexOf("Port") > -1 Then
          ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
          CanaleOpposite = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
        ElseIf CanaleOrdinata.ChannelId.IndexOf("Stbd") > -1 Then
          ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
          CanaleOpposite = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
        End If

        If CanaleOpposite Is Nothing Then
          ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
          CanaleOpposite = CanaleOrdinata
        End If
        If Period.IsStbd Then
          ' siamo mure a dritta, il sopravento è pertanto il canale stbd ovvero quello corrente
          Canale = CanaleOrdinata
        Else
          ' siamo mure a sinistra, il sottovento è pertanto il canale stbd ovvero quello opposto
          Canale = CanaleOpposite
        End If

        For i As Integer = Period.TR.IdRigaIniziale To Period.TR.IdRigaFinale
          Dim x As Double = CanaleAscissa.Valori(i)
          Dim y As Double = Canale.Valori(i)
          Select Case Canale.DataType
            Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
              y = System.Math.Abs(Canale.Valori(i))
          End Select
          If CanaleAscissa.ChannelId = CanaleOrdinata.ChannelId Then
            y = Period.TR.Durata.TotalMinutes
          End If

          If Gruppi.ContainsKey(Chiave) Then
            Gruppi(Chiave).AggiungiCoppia(x, y)
          Else
            Gruppi.Add(Chiave, New clsGruppoPeriodi(x, y, Mure, GrId(Chiave), Chiave, ColoreDaOutputType(Period, clsStraightLineVM2020.eOutputType.eColorByKey)))
          End If
        Next
      End If
    Next

    Dim ColoriDescrizione As New Dictionary(Of Integer, Color)

    Dim xvals As New List(Of Double)
    For Each Gruppo In Gruppi
      If Gruppi.Count = 1 OrElse Not Gruppo.Key = "empty" Then
        Try
          Dim DataSeries As New XyDataSeries(Of Double, Double)
          DataSeries.AcceptsUnsortedData = True
          DataSeries.SeriesName = Gruppo.Key
          Dim Linea As New XyScatterRenderableSeries
          Linea.XAxisId = "DefaultAxisId"
          Linea.YAxisId = "DefaultAxisId"
          If Gruppo.Value.Mure = clsGruppoPeriodi.eMure.ePort Then
            Linea.PointMarker = New SquarePointMarker
          Else
            Linea.PointMarker = New EllipsePointMarker()
          End If
          Linea.PointMarker.Stroke = Gruppo.Value.Colore '  Colors.Red
          Linea.PointMarker.Height = 10
          Linea.PointMarker.Width = 10
          Linea.PointMarker.StrokeThickness = 1
          'Linea.PointMarker.Fill = ColoriDifferenziati(Gruppo.Value.IdGruppo) ' Colors.Red
          Linea.PointMarker.Fill = Gruppo.Value.Colore ' Colors.Red
          ColoriDescrizione.Add(Gruppo.Value.IdGruppo, Linea.PointMarker.Fill)
          Linea.Tag = Gruppo.Value.IdGruppo + Gruppo.Value.Mure / 10
          Dim vx = Gruppo.Value.CoppieTwsValori.Tws.Where(Function(x) Not Double.IsNaN(x)).Average
          Dim vy = Gruppo.Value.CoppieTwsValori.Valori.Where(Function(x) Not Double.IsNaN(x)).Average
          xvals.Add(vx)
          DataSeries.Append(vx, vy, New clsPuntoMetadata(False))
          Linea.DataSeries = DataSeries
          Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
          SeriesSource.Add(CSVMport)
        Catch ex As Exception

        End Try
      End If
    Next
    If ParentVM.PlotTargetsIfAvailable AndAlso xvals.Count > 0 Then DisegnaTarget(CanaleOrdinata.PolarHeader, AtLeastOneUp, AtLeastOneDn, xvals.Min, xvals.Max)

    ShowLegend = CanaleAscissa Is CanaleOrdinata
  End Sub

  Public Overrides Sub DrawChart(StraightLine As clsPeriod2021, OutputType As clsStraightLineVM2020.eOutputType)
    ' qui non viene usata
  End Sub

End Class

Public Class clsStraightLineStandardPlotViewModelDistributions
  Inherits clsStraightLineStandardPlotViewModel
  Dim Intervalli As Integer = 10

  Public Sub New(ChAscissa As clsChannel2020, ChOrdinata As clsChannel2020, TitoloGrafico As String, ChartSync As clsStraightLineChartSync, ParentVM As clsStraightLineVM2020)
    MyBase.New(ChAscissa, ChOrdinata, TitoloGrafico, ChartSync, ParentVM)
  End Sub

  Public Overrides Sub DrawChart(StraightLines As List(Of clsPeriod2021), OutputType As clsStraightLineVM2020.eOutputType)
    ' qui viene usata per fare il group by tack
    Dim Tack As Boolean = (OutputType = clsStraightLineVM2020.eOutputType.eGroupByTackUpDn) OrElse (OutputType = clsStraightLineVM2020.eOutputType.eColorByTack) OrElse (OutputType = clsStraightLineVM2020.eOutputType.eGroupByTack)
    Dim UpDn As Boolean = (OutputType = clsStraightLineVM2020.eOutputType.eGroupByTackUpDn) OrElse (OutputType = clsStraightLineVM2020.eOutputType.eGroupByUpDn)
    DrawChartGroupByTack(StraightLines, UpDn, Tack)
  End Sub

  Public Overrides Sub DrawChart(Period As clsPeriod2021, OutputType As clsStraightLineVM2020.eOutputType)


    SeriesSource.Clear()
    Dim Samples(Intervalli) As Integer

    Dim Canale As clsChannel2020 = CanaleAscissa
    Dim CanaleOpposite As clsChannel2020 = CanaleAscissa
    If CanaleAscissa.ChannelId.IndexOf("Port") > -1 Then
      ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
      CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
    ElseIf CanaleAscissa.ChannelId.IndexOf("Stbd") > -1 Then
      ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
      CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
    End If

    If CanaleOpposite Is Nothing Then
      ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
      CanaleOpposite = CanaleAscissa
    End If
    If Period.IsStbd Then
      ' siamo mure a dritta, il sopravento è pertanto il canale stbd ovvero quello corrente
      Canale = CanaleAscissa
    Else
      ' siamo mure a sinistra, il sottovento è pertanto il canale stbd ovvero quello opposto
      Canale = CanaleOpposite
    End If

    Dim ValoriStat As New clsValoriPeriodoCanale2020(Canale, Period.TR, False)
    Dim VNN As Double() = ValoriStat.AggiornaValori(Canale.DataType = clsChannel2020.eDataType.e180)
    Dim e As Integer = Canale.Decimals - 1
    Dim MaxVal As Double = ValoriStat.Max
    Dim MinVal As Double = ValoriStat.Min
    Dim TotalSamples As Integer
    If MaxVal = MinVal Then
      Intervalli = 1
      ReDim Samples(0)
      TotalSamples = Canale.Valori.Count ' Period.IDultimaRiga - StraightLine.IDprimaRiga
      Samples(0) = TotalSamples
    Else
      MaxVal = (Int(MaxVal * 10 ^ e) / 10 ^ e) + (1 / 10 ^ e)
      MinVal = Int(MinVal * 10 ^ e) / 10 ^ e - 1
      For Riga As Integer = 0 To VNN.Count - 1
        Dim IndiceCorrente As Integer = Int((VNN(Riga) - MinVal) / (MaxVal - MinVal) * Intervalli)
        IndiceCorrente = System.Math.Max(0, IndiceCorrente)
        IndiceCorrente = System.Math.Min(Samples.Count - 1, IndiceCorrente)
        Samples(IndiceCorrente) += 1
        TotalSamples += 1
      Next
    End If

    Dim DataSeriesTMP As New XyDataSeries(Of Double, Double)
    DataSeriesTMP.AcceptsUnsortedData = True
    Dim LineaTmp As New FastMountainRenderableSeries
    LineaTmp.XAxisId = "DefaultAxisId"
    LineaTmp.YAxisId = "DefaultAxisId"
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eColorByTack
        If Period.IsStbd Then
          LineaTmp.Stroke = Colors.Green
        Else
          LineaTmp.Stroke = Colors.Red
        End If

      Case clsStraightLineVM2020.eOutputType.eColorByKey
        LineaTmp.Stroke = ColoreDaOutputType(Period, OutputType)
      Case clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
        Stop
        LineaTmp.Stroke = Period.Colore
      Case Else
        LineaTmp.Stroke = Period.Colore
    End Select
    If Period.IsStbd Then
      LineaTmp.StrokeDashArray = {3, 3}
    End If
    LineaTmp.StrokeThickness = 3
    LineaTmp.Tag = Period
    'DataSeriesTMP.AcceptsUnsortedData = True

    For Riga As Integer = 0 To Samples.Count - 1
      Dim X As Double = MinVal + (Riga / Intervalli * (MaxVal - MinVal))
      If Riga = 0 AndAlso Samples(Riga) > 0 Then
        DataSeriesTMP.Append(X, 0, New clsPuntoMetadata(False))
      End If
      Dim Y As Double = Samples(Riga) / TotalSamples * 100
      DataSeriesTMP.Append(X, Y, New clsPuntoMetadata(False))
      If Riga = Samples.Count - 1 AndAlso Samples(Riga) > 0 Then
        DataSeriesTMP.Append(X, 0, New clsPuntoMetadata(False))
      End If
    Next
    LineaTmp.DataSeries = DataSeriesTMP
    Dim CSVMtmp As New ChartSeriesViewModel(DataSeriesTMP, LineaTmp)
    SeriesSource.Add(CSVMtmp)

    'ShowLegend = OutputType = clsStraightLineVM2020.eOutputType.eColorByKey AndAlso CanaleAscissa Is CanaleOrdinata
    ShowLegend = False

  End Sub


  Public Overrides Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021), UpDn As Boolean, Tack As Boolean)

    'DrawChartGroupByTack(StraightLines)
    'Exit Sub

    SeriesSource.Clear()
    'Dim pColori As List(Of Color) = ScalaColori(StraightLines.Count)
    If CanaleAscissa Is Nothing Then Exit Sub
    'If CanaleOrdinata Is Nothing Then Exit Sub
    Dim GBT As New List(Of PuntoGroupByTack)

    Dim AtLeastOneUp As Boolean = False
    Dim AtLeastOneDn As Boolean = False
    For Each Period In StraightLines ' PeriodsManager.ListaStraightLineVmg
      If Period.IsChecked Then
        For i As Integer = Period.TR.IdRigaIniziale To Period.TR.IdRigaFinale
          If Not Double.IsNaN(CanaleAscissa.Valori(i)) Then
            Dim y = CanaleAscissa.Valori(i)
            Select Case CanaleAscissa.DataType
              Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                y = System.Math.Abs(y)
            End Select
            'GBT.Add(New PuntoGroupByTack(CanaleAscissa.Valori(i), y, Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))

            GBT.Add(New PuntoGroupByTack(y, y, Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))
          End If
        Next

      End If
    Next

    'Dim intervalli = 20

    Dim inizio As Integer = 0
    Dim fine As Integer = 1
    If Tack Then
      If UpDn Then
        inizio = 2
        fine = 5
      End If
    Else
      If UpDn Then
        inizio = 6
        fine = 7
      Else
        inizio = 8
        fine = 8
      End If
    End If

    For i As Integer = inizio To fine
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastMountainRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.StrokeThickness = 3
      Dim Punti As List(Of PuntoGroupByTack)
      Select Case i
        Case 0
          Punti = GBT.Where(Function(x) x.IsStbd = False).ToList
          Linea.Stroke = Colors.Red
          Linea.Tag = -1
          DataSeries.SeriesName = "Port"
        Case 1
          Punti = GBT.Where(Function(x) x.IsStbd = True).ToList
          Linea.Stroke = Colors.Green
          Linea.Tag = 1
          DataSeries.SeriesName = "Stbd"
        Case 2
          Punti = GBT.Where(Function(x) Not x.IsStbd AndAlso x.IsUp).ToList
          Linea.Stroke = Colors.Red
          Linea.Tag = -1
          DataSeries.SeriesName = "UpPort"
        Case 3
          Punti = GBT.Where(Function(x) x.IsStbd AndAlso x.IsUp).ToList
          Linea.Stroke = Colors.Green
          Linea.Tag = 1
          DataSeries.SeriesName = "UpStbd"
        Case 4
          Punti = GBT.Where(Function(x) Not x.IsStbd AndAlso Not x.IsUp).ToList
          Linea.Stroke = Colors.Gold
          Linea.Tag = -2
          DataSeries.SeriesName = "DnPort"
        Case 5
          Punti = GBT.Where(Function(x) x.IsStbd AndAlso Not x.IsUp).ToList
          Linea.Stroke = Colors.RoyalBlue
          Linea.Tag = 2
          DataSeries.SeriesName = "DnStbd"
        Case 6
          Punti = GBT.Where(Function(x) x.IsUp).ToList
          Linea.Stroke = Colors.Gold
          Linea.Tag = -3
          DataSeries.SeriesName = "Upwind"
        Case 7
          Punti = GBT.Where(Function(x) Not x.IsUp).ToList
          Linea.Stroke = Colors.RoyalBlue
          Linea.Tag = 3
          DataSeries.SeriesName = "Downwind"
        Case 8
          Punti = GBT.Where(Function(x) x.IsUp).ToList
          Linea.Stroke = Colors.RoyalBlue
          Linea.Tag = 10
          DataSeries.SeriesName = "AllTogether"
      End Select
      Dim Colore As New SolidColorBrush(Linea.Stroke)
      Colore.Opacity = 0.2
      Linea.Fill = Colore

      If Punti.Count > 0 Then
        Dim xx As New List(Of Double)
        Dim yy As New List(Of Double)
        Dim Vals = Punti.Select(Function(x) x.Yval).ToList
        Dim min = Vals.Min
        Dim max = Vals.Max
        Dim d = max - min
        If d > 0 Then
          Dim h As Double = d / Intervalli
          Dim vx As Double = min - h
          Dim vy As Double = 0
          xx.Add(vx)
          yy.Add(vy)
          'DataSeries.Append(vx, vy, New clsPuntoMetadata(False))
          'Dim Ytot As Double = 0
          For ii As Integer = 0 To Intervalli
            vx = min + (h * ii)
            vy = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(vx, h, Vals)
            xx.Add(vx)
            yy.Add(vy)
            'DataSeries.Append(vx, vy, New clsPuntoMetadata(False))
          Next
          xx.Add(max + h)
          yy.Add(0)
          Dim Tot As Double = yy.Sum
          'DataSeries.Append(vx, vy, New clsPuntoMetadata(False))
          For ii As Integer = 0 To yy.Count - 1
            If Not Double.IsNaN(yy(ii)) Then
              yy(ii) = yy(ii) / Tot * 100
              DataSeries.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
            End If
          Next
          Linea.DataSeries = DataSeries
          Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
          SeriesSource.Add(CSVM)
        End If
      End If
    Next

    ShowLegend = CanaleAscissa.CanaleChiave = clsChannels2020.eCanaliChiave.eTWS

  End Sub


  'Public Overrides Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021), UpDn As Boolean)

  '  SeriesSource.Clear()
  '  'Dim pColori As List(Of Color) = ScalaColori(StraightLines.Count)
  '  If CanaleAscissa Is Nothing Then Exit Sub
  '  If CanaleOrdinata Is Nothing Then Exit Sub
  '  Dim GBT As New List(Of PuntoGroupByTack)

  '  If CanaleOrdinata Is CanaleAscissa Then
  '    TitoloPlot = "Minutes"
  '  End If
  '  Dim AtLeastOneUp As Boolean = False
  '  Dim AtLeastOneDn As Boolean = False
  '  For Each Period In StraightLines ' PeriodsManager.ListaStraightLineVmg
  '    If Period.IsChecked Then
  '      AtLeastOneUp = AtLeastOneUp OrElse (Period.IsChecked AndAlso Period.IsUpwindVmgRange)
  '      AtLeastOneDn = AtLeastOneDn OrElse (Period.IsChecked AndAlso Period.IsDownwindVmgRange)
  '      If CanaleOrdinata Is CanaleAscissa Then
  '        GBT.Add(New PuntoGroupByTack(Period.TR.Durata.TotalMinutes, AvgCanaleIntervallo(CanaleOrdinata, Period.TR), Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))
  '      Else
  '        GBT.Add(New PuntoGroupByTack(AvgCanaleIntervallo(CanaleAscissa, Period.TR), AvgCanaleIntervallo(CanaleOrdinata, Period.TR), Period.IsStbd, Math.Abs(Period.AvgTwa) < 90))
  '      End If

  '    End If
  '  Next

  '  For Each P In GBT
  '    Dim DataSeries As New XyDataSeries(Of Double, Double)
  '    Dim Linea As New FastLineRenderableSeries
  '    Linea.XAxisId = "DefaultAxisId"
  '    Linea.YAxisId = "DefaultAxisId"
  '    Linea.PointMarker = New EllipsePointMarker
  '    Linea.PointMarker.Height = 10
  '    Linea.PointMarker.Width = 10
  '    Linea.PointMarker.StrokeThickness = 1
  '    Linea.Tag = P
  '    If UpDn Then
  '      If P.IsStbd Then
  '        If P.IsUp Then
  '          Linea.Stroke = Colors.Green
  '        Else
  '          Linea.Stroke = Colors.LimeGreen
  '        End If
  '      Else
  '        If P.IsUp Then
  '          Linea.Stroke = Colors.Red
  '        Else
  '          Linea.Stroke = Colors.OrangeRed
  '        End If
  '      End If
  '    Else
  '      If P.IsStbd Then
  '        Linea.PointMarker.Stroke = Colors.Green
  '      Else
  '        Linea.PointMarker.Stroke = Colors.Red
  '      End If
  '    End If
  '    DataSeries.Append(P.Xval, P.Yval, New clsPuntoMetadata(False))
  '    Linea.DataSeries = DataSeries
  '    Dim CSVM As New ChartSeriesViewModel(DataSeries, Linea)
  '    SeriesSource.Add(CSVM)
  '  Next


  '  Dim TwsRange As New DoubleRange(GBT.Select(Function(x) x.Xval).Min, GBT.Select(Function(x) x.Xval).Max)
  '  StraightLineChartSync.SharedXVisibleRange = TwsRange

  '  ShowLegend = False
  'End Sub



  Public Overrides Sub DrawChartGroupByTack(Periods As List(Of clsPeriod2021)) 'Kde

    'Dim Intervalli As Integer = 10
    'Intervalli = 30
    SeriesSource.Clear()
    Dim PnSmax As Double = Nothing
    Dim PnSmin As Double = Nothing
    Dim ValsNotNanPort As New List(Of Double())
    Dim ValsNotNanStbd As New List(Of Double())
    For Each Period In Periods
      If Period.IsChecked Then


        Dim Canale As clsChannel2020 = CanaleAscissa
        Dim CanaleOpposite As clsChannel2020 = CanaleAscissa
        If CanaleAscissa.ChannelId.IndexOf("Port") > -1 Then
          ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
          CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
        ElseIf CanaleAscissa.ChannelId.IndexOf("Stbd") > -1 Then
          ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
          CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
        End If

        If CanaleOpposite Is Nothing Then
          ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
          CanaleOpposite = CanaleAscissa
        End If
        If Period.IsStbd Then
          ' siamo mure a dritta, il sopravento è pertanto il canale stbd ovvero quello corrente
          Canale = CanaleAscissa
        Else
          ' siamo mure a sinistra, il sottovento è pertanto il canale stbd ovvero quello opposto
          Canale = CanaleOpposite
        End If


        Dim ValoriStat As New clsValoriPeriodoCanale2020(Canale, Period.TR, False)
        If Period.IsStbd Then
          ValsNotNanStbd.Add(ValoriStat.AggiornaValori(Canale.DataType = clsChannel2020.eDataType.e180))
        Else
          ValsNotNanPort.Add(ValoriStat.AggiornaValori(Canale.DataType = clsChannel2020.eDataType.e180))
        End If
        If PnSmax = Nothing Then
          PnSmax = ValoriStat.Max
          PnSmin = ValoriStat.Min
        Else
          PnSmax = System.Math.Max(PnSmax, ValoriStat.Max)
          PnSmin = System.Math.Min(PnSmin, ValoriStat.Min)
        End If
      End If
    Next
    Dim e As Integer = CanaleAscissa.Decimals - 1
    Dim MaxVal As Double = (Int(PnSmax * 10 ^ e) / 10 ^ e) + (1 / 10 ^ e)
    Dim MinVal As Double = Int(PnSmin * 10 ^ e) / 10 ^ e - 1

    Dim SamplesPort(Intervalli) As Integer
    Dim SamplesStbd(Intervalli) As Integer

    Dim GlobalSamples As Integer = ValsNotNanPort.Select(Function(x) x.Count).Sum
    GlobalSamples += ValsNotNanStbd.Select(Function(x) x.Count).Sum

    Dim H As Double = (MaxVal - MinVal) / Intervalli


    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine("Port")
    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    Dim LineaPort As New FastMountainRenderableSeries
    LineaPort.XAxisId = "DefaultAxisId"
    LineaPort.YAxisId = "DefaultAxisId"
    LineaPort.Stroke = Colors.DarkRed
    LineaPort.StrokeThickness = 3
    LineaPort.Tag = -1
    Dim Colore As New SolidColorBrush(Colors.Red)
    Colore.Opacity = 0.2
    LineaPort.Fill = Colore
    Dim xx As New List(Of Double)
    Dim yy As New List(Of Double)

    If ValsNotNanPort.Count > 0 Then
      Dim vP As New List(Of Double)
      For Each v In ValsNotNanPort
        vP.AddRange(v)
      Next
      Dim PortSamples As Integer = ValsNotNanPort.Select(Function(x) x.Count).Sum
      Dim somma As Double = 0
      xx.Add(MinVal - Math.Abs(MinVal * 0.001))
      yy.Add(0)
      For ii As Integer = 0 To Intervalli
        Dim x As Double = MinVal + (H * ii)
        xx.Add(x)
        Dim kde = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, vP)
        Dim y As Double = kde / PortSamples ' * 100
        If Not ParentVM.NormalizeDistributions Then
          y = PortSamples / GlobalSamples
        End If
        somma += y
        yy.Add(y)

      Next
      xx.Add(xx.Last + Math.Abs(xx.Last * 0.001))
      yy.Add(0)
      For j As Integer = 0 To yy.Count - 1
        yy(j) = yy(j) * (100 / somma)
        DataSeriesPort.Append(xx(j), yy(j), New clsPuntoMetadata(False))
      Next


      LineaPort.DataSeries = DataSeriesPort
      Dim CSVMport As New ChartSeriesViewModel(DataSeriesPort, LineaPort)
      SeriesSource.Add(CSVMport)
      Console.WriteLine(somma)
    End If

    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine()
    'Console.WriteLine("Stbd")
    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim LineaStbd As New FastMountainRenderableSeries
    LineaStbd.XAxisId = "DefaultAxisId"
    LineaStbd.YAxisId = "DefaultAxisId"
    LineaStbd.Stroke = Colors.Green
    LineaStbd.StrokeThickness = 3
    LineaStbd.Tag = 1
    Dim Colore2 As New SolidColorBrush(Colors.Green)
    Colore2.Opacity = 0.2
    LineaStbd.Fill = Colore2

    If ValsNotNanStbd.Count > 0 Then
      xx.Clear()
      yy.Clear()
      Dim vS As New List(Of Double)
      For Each v In ValsNotNanStbd
        vS.AddRange(v)
      Next
      Dim StbdSamples As Integer = ValsNotNanStbd.Select(Function(x) x.Count).Sum
      Dim somma As Double = 0
      xx.Add(MinVal - Math.Abs(MinVal * 0.001))
      yy.Add(0)
      For ii As Integer = 0 To Intervalli
        Dim x As Double = MinVal + (H * ii)
        xx.Add(x)
        Dim kde = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, vS)
        Dim y As Double = kde / StbdSamples ' * 100
        If ParentVM.NormalizeDistributions Then
          y *= StbdSamples / GlobalSamples
        End If
        yy.Add(y)
        somma += y
        Console.WriteLine(y)
      Next
      xx.Add(xx.Last + Math.Abs(xx.Last * 0.001))
      yy.Add(0)
      For j As Integer = 0 To yy.Count - 1
        yy(j) = yy(j) * (100 / somma)
        DataSeriesStbd.Append(xx(j), yy(j), New clsPuntoMetadata(False))
      Next

      LineaStbd.DataSeries = DataSeriesStbd
      Dim CSVMStbd As New ChartSeriesViewModel(DataSeriesStbd, LineaStbd)
      SeriesSource.Add(CSVMStbd)
      Console.WriteLine(somma)
    End If

    'Stop
    ShowLegend = False

  End Sub




  Public Overrides Sub DrawChart(Periods As List(Of clsPeriod2021))

    SeriesSource.Clear()
    Dim PnSmax As Double = Nothing
    Dim PnSmin As Double = Nothing
    'Dim ValsNotNan As New List(Of Double())
    Dim ValsNotNan As New Dictionary(Of clsPeriod2021, Double())
    For Each Period In Periods

      Dim Canale As clsChannel2020 = CanaleAscissa
      Dim CanaleOpposite As clsChannel2020 = CanaleAscissa
      If CanaleAscissa.ChannelId.IndexOf("Port") > -1 Then
        ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
        CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
      ElseIf CanaleAscissa.ChannelId.IndexOf("Stbd") > -1 Then
        ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
        CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
      End If

      If CanaleOpposite Is Nothing Then
        ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
        CanaleOpposite = CanaleAscissa
      End If
      If Period.IsStbd Then
        ' siamo mure a dritta, il sopravento è pertanto il canale stbd ovvero quello corrente
        Canale = CanaleAscissa
      Else
        ' siamo mure a sinistra, il sottovento è pertanto il canale stbd ovvero quello opposto
        Canale = CanaleOpposite
      End If


      Dim ValoriStat As New clsValoriPeriodoCanale2020(Canale, Period.TR, False)
      ValsNotNan.Add(Period, ValoriStat.AggiornaValori(Canale.DataType = clsChannel2020.eDataType.e180))
      If PnSmax = Nothing Then
        PnSmax = ValoriStat.Max
        PnSmin = ValoriStat.Min
      Else
        PnSmax = System.Math.Max(PnSmax, ValoriStat.Max)
        PnSmin = System.Math.Min(PnSmin, ValoriStat.Min)
      End If
    Next

    If ValsNotNan.Count = 0 Then
      Exit Sub
    End If

    Dim e As Integer = CanaleAscissa.Decimals - 1
    Dim MaxVal As Double = (Int(PnSmax * 10 ^ e) / 10 ^ e) + (1 / 10 ^ e)
    Dim MinVal As Double = Int(PnSmin * 10 ^ e) / 10 ^ e - 1

    Dim GlobalSamples As Integer = 0
    For Each vv In ValsNotNan
      If Not vv.Value Is Nothing Then
        GlobalSamples += vv.Value.Where(Function(x) Not x = Nothing).Count
      End If
    Next
    If GlobalSamples = 0 Then Exit Sub

    Dim H As Double = (MaxVal - MinVal) / Intervalli

    For i As Integer = 0 To ValsNotNan.Count - 1
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim Linea As New FastMountainRenderableSeries
      Linea.XAxisId = "DefaultAxisId"
      Linea.YAxisId = "DefaultAxisId"
      Linea.Stroke = ValsNotNan.Keys(i).Colore
      Linea.StrokeThickness = 1
      Linea.Tag = ValsNotNan.Keys(i)
      Dim Colore As New SolidColorBrush(Linea.Stroke)
      Colore.Opacity = 0.2
      Linea.Fill = Colore
      Linea.IsVisible = ValsNotNan.Keys(i).IsChecked
      If Not ValsNotNan.Values(i) Is Nothing AndAlso ValsNotNan.Values(i).Count > 0 Then
        Dim xx As New List(Of Double)
        Dim yy As New List(Of Double)
        Dim somma As Double = 0
        xx.Add(MinVal - Math.Abs(MinVal * 0.001))
        yy.Add(0)
        For ii As Integer = 0 To Intervalli
          Dim x As Double = MinVal + (H * ii)
          xx.Add(x)
          Dim kde = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, H, ValsNotNan.Values(i))
          Dim y As Double = kde / ValsNotNan.Values(i).Count
          If Not ParentVM.NormalizeDistributions Then
            y = ValsNotNan.Values(i).Count / GlobalSamples
          End If
          somma += y
          yy.Add(y)
        Next
        xx.Add(xx.Last + Math.Abs(xx.Last * 0.001))
        yy.Add(0)
        For j As Integer = 0 To yy.Count - 1
          yy(j) = yy(j) * (100 / somma)
          DataSeries.Append(xx(j), yy(j), New clsPuntoMetadata(False))
        Next
        Linea.DataSeries = DataSeries
        Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
        SeriesSource.Add(CSVMport)
      End If
    Next

    'ShowLegend = CanaleAscissa Is DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eTWS)

    ShowLegend = False

  End Sub

  Public Overrides Sub DrawChartGroupByKeys(StraightLines As List(Of clsPeriod2021))

    SeriesSource.Clear()

    Dim Gruppi As New Dictionary(Of String, clsGruppoPeriodi)
    Dim GrId As New Dictionary(Of String, Integer)
    Dim contatore As Integer = 0

    Dim PnSmax As Double = Nothing
    Dim PnSmin As Double = Nothing
    Dim GlobalSamples As Integer = 0

    For Each periodo In StraightLines
      If periodo.IsChecked Then
        Dim Chiave As String = periodo.Keys
        If True Then
          If Chiave.Trim = "" Then
            Chiave = "empty"
          End If
          If Not GrId.ContainsKey(Chiave) Then
            GrId.Add(Chiave, contatore)
            contatore += 1
          End If
          Dim Mure As clsGruppoPeriodi.eMure = clsGruppoPeriodi.eMure.eBoth
          If periodo.IsStbd Then
            Mure = clsGruppoPeriodi.eMure.eStbd
          Else
            Mure = clsGruppoPeriodi.eMure.ePort
          End If

          Dim Canale As clsChannel2020 = CanaleAscissa
          Dim CanaleOpposite As clsChannel2020 = CanaleAscissa
          If CanaleAscissa.ChannelId.IndexOf("Port") > -1 Then
            ' se c'è la scritta Port è una potenziale richiesta dei valori sottovento
            CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Port", "Stbd")) ' è l'omologo stbd
          ElseIf CanaleAscissa.ChannelId.IndexOf("Stbd") > -1 Then
            ' se c'è la scritta Stbd è una potenziale richiesta dei valori sopravento
            CanaleOpposite = DataProvider2020.CanaleDbl(CanaleAscissa.ChannelId.Replace("Stbd", "Port")) ' è l'omologo port
          End If

          If CanaleOpposite Is Nothing Then
            ' non esiste canale opposite quindi l output è relativo al CanaleAscissa/Ordinata
            CanaleOpposite = CanaleAscissa
          End If
          If periodo.IsStbd Then
            ' siamo mure a dritta, il sopravento è pertanto il canale stbd ovvero quello corrente
            Canale = CanaleAscissa
          Else
            ' siamo mure a sinistra, il sottovento è pertanto il canale stbd ovvero quello opposto
            Canale = CanaleOpposite
          End If


          For i As Integer = periodo.TR.IdRigaIniziale To periodo.TR.IdRigaFinale
            Dim x As Double = Canale.Valori(i)
            Select Case Canale.DataType
              Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                x = System.Math.Abs(Canale.Valori(i))
            End Select
            Dim y As Double = x
            If Gruppi.ContainsKey(Chiave) Then
              Gruppi(Chiave).AggiungiCoppia(x, y)
            Else
              Gruppi.Add(Chiave, New clsGruppoPeriodi(x, y, Mure, GrId(Chiave), Chiave, ColoreDaOutputType(periodo, clsStraightLineVM2020.eOutputType.eColorByKey)))
            End If
            GlobalSamples += 1
            If PnSmax = Nothing Then
              PnSmax = x
              PnSmin = x
            Else
              If Not Double.IsNaN(x) Then
                PnSmax = System.Math.Max(PnSmax, x)
                PnSmin = System.Math.Min(PnSmin, x)
              End If
            End If

          Next
        End If
      End If
    Next
    If GlobalSamples = 0 Then Exit Sub

    Dim e As Integer = CanaleAscissa.Decimals - 1
    Dim MaxVal As Double = (Int(PnSmax * 10 ^ e) / 10 ^ e) + (1 / 10 ^ e)
    Dim MinVal As Double = Int(PnSmin * 10 ^ e) / 10 ^ e - 1
    Dim H As Double = (MaxVal - MinVal) / Intervalli


    For Each gruppo In Gruppi
      If Gruppi.Count = 1 OrElse Not gruppo.Key = "empty" Then
        Dim p As clsPeriod2021 = StraightLines.Where(Function(x) x.Keys = gruppo.Key).FirstOrDefault
        Dim DataSeries As New XyDataSeries(Of Double, Double)
        DataSeries.AcceptsUnsortedData = True
        DataSeries.SeriesName = gruppo.Key
        Dim Linea As New FastMountainRenderableSeries
        Linea.XAxisId = "DefaultAxisId"
        Linea.YAxisId = "DefaultAxisId"
        'Linea.Stroke = ColoriDifferenziati(gruppo.Value.IdGruppo)
        Linea.Stroke = gruppo.Value.Colore
        If Not (p Is Nothing) Then Linea.Stroke = ColoreDaOutputType(p, clsStraightLineVM2020.eOutputType.eColorByKey)
        Linea.StrokeThickness = 1
        Linea.Tag = gruppo.Value.IdGruppo + gruppo.Value.Mure / 10
        Dim Colore As New SolidColorBrush(Linea.Stroke)
        Colore.Opacity = 0.2
        Linea.Fill = Colore
        If gruppo.Value.CoppieTwsValori.Valori.Count > 0 Then
          Dim xx As New List(Of Double)
          Dim yy As New List(Of Double)
          Dim Vals = gruppo.Value.CoppieTwsValori.Valori.Where(Function(k) Not Double.IsNaN(k)).ToList
          If Vals.Count > 0 Then
            Dim min = Vals.Min
            Dim max = Vals.Max
            Dim vx As Double = min - H
            Dim vy As Double = 0
            xx.Add(vx)
            yy.Add(vy)
            For ii As Integer = 0 To Intervalli
              vx = MinVal + (H * ii)
              vy = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(vx, H, Vals)
              xx.Add(vx)
              yy.Add(vy)
            Next
            xx.Add(max + H)
            yy.Add(0)
            Dim Tot As Double = yy.Sum
            For ii As Integer = 0 To yy.Count - 1
              yy(ii) = yy(ii) / Tot * 100
              DataSeries.Append(xx(ii), yy(ii), New clsPuntoMetadata(False))
            Next
            Linea.DataSeries = DataSeries
            Dim CSVMport As New ChartSeriesViewModel(DataSeries, Linea)
            SeriesSource.Add(CSVMport)
          End If
        End If
      End If
    Next

    ShowLegend = CanaleAscissa Is DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eTWS)
    'ShowLegend = CanaleAscissa Is CanaleOrdinata

  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public MustInherit Class clsStraightLineStandardPlotViewModel
  'Implements INotifyPropertyChanged
  Public Property CanaleAscissa As clsChannel2020
  Public Property CanaleOrdinata As clsChannel2020
  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)
  Public Property TitoloPlot As String = "XY components"
  Public Property StraightLineChartSync As clsStraightLineChartSync
  Public Property DataBoxHeight As Double
  Public Property ParentVM As clsStraightLineVM2020
  Public Property ShowLegend As Boolean = False

  Public Sub New(ChAscissa As clsChannel2020, ChOrdinata As clsChannel2020, TitoloGrafico As String, ChartSync As clsStraightLineChartSync, ParentVM As clsStraightLineVM2020)
    Me.CanaleAscissa = ChAscissa
    Me.CanaleOrdinata = ChOrdinata
    Me.TitoloPlot = TitoloGrafico
    Me.StraightLineChartSync = ChartSync
    Me.ParentVM = ParentVM
  End Sub

  Public MustOverride Sub DrawChart(StraightLines As List(Of clsPeriod2021), OutputType As clsStraightLineVM2020.eOutputType)

  Public MustOverride Sub DrawChart(StraightLines As List(Of clsPeriod2021))

  Public MustOverride Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021))

  Public MustOverride Sub DrawChartGroupByTack(StraightLines As List(Of clsPeriod2021), UpDn As Boolean, Tack As Boolean)

  Public MustOverride Sub DrawChartGroupByKeys(StraightLines As List(Of clsPeriod2021))

  Public MustOverride Sub DrawChart(StraightLine As clsPeriod2021, OutputType As clsStraightLineVM2020.eOutputType)

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineChartSync
  'Implements INotifyPropertyChanged
  Public Property SharedXVisibleRange As SciChart.Data.Model.IRange
  Public Property WheelZoomEnabled As Boolean = True
  Public Property CurrentPosition As Double

  Public Property ZoomPanEnabled As Boolean = False
  Public Property DataPointSelectionEnabled As Boolean = True
  Public Property SeriesSelectionEnabled As Boolean = False

  Public Property CanaliDaStampare As New ObservableCollection(Of clsChannel2020)
  Public Property DataBoxHeight As Double = 200

  'Public Property DataBoxHeight As Double
  '  Get
  '    Return pDataBoxHeight
  '  End Get
  '  Set(value As Double)
  '    If pDataBoxHeight = value Then Exit Property
  '    pDataBoxHeight = value
  '    OnPropertyChanged("DataBoxHeight")
  '  End Set
  'End Property

  'Public Property ZoomPanEnabled As Boolean
  '  Get
  '    Return pZoomPanEnabled
  '  End Get
  '  Set(value As Boolean)
  '    pZoomPanEnabled = value
  '    OnPropertyChanged("ZoomPanEnabled")
  '  End Set
  'End Property

  'Public Property DataPointSelectionEnabled As Boolean
  '  Get
  '    Return pDataPointSelectionEnabled
  '  End Get
  '  Set(value As Boolean)
  '    pDataPointSelectionEnabled = value
  '    OnPropertyChanged("DataPointSelectionEnabled")
  '  End Set
  'End Property

  'Public Property SeriesSelectionEnabled As Boolean
  '  Get
  '    Return pSeriesSelectionEnabled
  '  End Get
  '  Set(value As Boolean)
  '    pSeriesSelectionEnabled = value
  '    OnPropertyChanged("SeriesSelectionEnabled")
  '  End Set
  'End Property

  Public Enum eLoadedDataMouseMode
    eSelection = 0
    ePan = 1
  End Enum

  'Public Property SharedXVisibleRange As IRange
  '  Get
  '    Return pSharedXVisibleRange
  '  End Get
  '  Set(value As IRange)
  '    If pSharedXVisibleRange Is value Then Exit Property
  '    pSharedXVisibleRange = value
  '    OnPropertyChanged("SharedXVisibleRange")
  '  End Set
  'End Property

  'Public Property WheelZoomEnabled As Boolean
  '  Get
  '    Return pWheelZoomEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pWheelZoomEnabled = value Then Exit Property
  '    pWheelZoomEnabled = value
  '    OnPropertyChanged("WheelZoomEnabled")
  '  End Set
  'End Property


  'Public Property CurrentPosition As Double
  '  Get
  '    Return pCurrentPosition
  '  End Get
  '  Set(value As Double)
  '    If pCurrentPosition = value Then Exit Property
  '    pCurrentPosition = value
  '    OnPropertyChanged("CurrentPosition")
  '  End Set
  'End Property

  'Public Property CanaliDaStampare As ObservableCollection(Of clsChannel2020)
  '  Get
  '    Return pCanaliDaStampare
  '  End Get
  '  Set(value As ObservableCollection(Of clsChannel2020))
  '    pCanaliDaStampare = value
  '    OnPropertyChanged("CanaliDaStampare")
  '  End Set
  'End Property



  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

End Class