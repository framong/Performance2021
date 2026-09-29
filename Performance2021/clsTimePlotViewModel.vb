Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports SciChart.Charting.Model
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Annotations
Imports SciChart.Charting.Visuals.Axes.LabelProviders
Imports SciChart.Charting.Visuals.RenderableSeries
Imports SciChart.Data.Model
Imports SPwpf

Public Class clsTimePlotViewModel
	Implements INotifyPropertyChanged

	WithEvents pObjChartSyncManager As clsChartSyncManager

	Dim pListaParent As clsMatriceControlliSinglePeriod

	Dim pSeriesSource As ObservableCollection(Of IChartSeriesViewModel)
	Dim pSeriesSourceAbsVal As ObservableCollection(Of IChartSeriesViewModel)
  Dim _Annotazioni As SciChart.Charting.Visuals.Annotations.AnnotationCollection
  'Dim BindGroupSfondo As New BindingGroup

  Dim pYassi As AxisCollection
  Dim pXassi As AxisCollection
  Dim pYassePerc As SciChart.Charting.Visuals.Axes.NumericAxis
  Dim pYassiOrg As AxisCollection
  Dim pYasseTitle As String
  Dim pCanaleAscissa As clsChannel2020
  Dim pCanaliOrdinata As ObservableCollection(Of clsChannel2020)

  Dim pShowAbsolute As Boolean
  Dim pShowDerivative As Boolean 'ShowDerivative
  Dim _ShowWindwardLeeward As Boolean
  Dim _CommonYaxis As Boolean

  Dim _LineType As clsGroupLines.eLineType = clsGroupLines.eLineType.eValue
  Dim _ShowPortStbdBackGround As Boolean
  Dim _ShowTargetIfAvailable As Boolean

  Dim _GroupLines As New List(Of clsGroupLines)

  Dim pDataBoxHeight As Double = 70

  Dim pCmdSelezionaCanale As New clsComando(AddressOf SelezionaCanaliGrafico)

  Dim pAggiornaSelezione As Boolean

  'Dim pGruppoGrafici As eGruppoGrafici

  'Public Enum eGruppoGrafici
  '  eNone = -1
  '  eBasic = 0
  '  eDetails = 1
  'End Enum


  'Public Enum eTipoGrafico
  '  eTimePlot = 0
  '  eGroupBy = 1
  'End Enum

  'Public Enum eVisualizzazioneGroupBy
  '  eNormale = 0
  '  eAssoluta = 1
  '  eAssolutaTbT = 2
  '  ePort = 3
  '  eStbd = 4
  'End Enum

  Public Enum eGruppoAnnotazione
    eSfondo = 0
    eGruppo1 = 1
    eGruppo2 = 2
    eNothing = 3
  End Enum


  Public Sub New(ObjChartSyncManager As clsChartSyncManager, ListaParent As clsMatriceControlliSinglePeriod) ', VisualizzazioneGroupBy As eVisualizzazioneGroupBy, GruppoGrafici As eGruppoGrafici)
    'pGruppoGrafici = GruppoGrafici
    pObjChartSyncManager = ObjChartSyncManager
    pListaParent = ListaParent
    'pVisualizzazioneGroupBy = VisualizzazioneGroupBy
    SeriesSource = New ObservableCollection(Of IChartSeriesViewModel)

  End Sub

  Public Property ObjChartSyncManager As clsChartSyncManager
    Get
      Return pObjChartSyncManager
    End Get
    Set(value As clsChartSyncManager)
      pObjChartSyncManager = value
    End Set
  End Property

  Public Property Yassi As AxisCollection
    Get
      Return pYassi
    End Get
    Set(value As AxisCollection)
      If pYassi Is value Then Exit Property
      pYassi = value
      OnPropertyChanged("Yassi")
    End Set
  End Property

  Public Property Xassi As AxisCollection
    Get
      Return pXassi
    End Get
    Set(value As AxisCollection)
      If pXassi Is value Then Exit Property
      pXassi = value
      OnPropertyChanged("Xassi")
    End Set
  End Property

  Public Property DataBoxHeight As Double
    Get
      Return pDataBoxHeight
    End Get
    Set(value As Double)
      If pDataBoxHeight = value Then Exit Property
      pDataBoxHeight = value
      OnPropertyChanged("DataBoxHeight")
    End Set
  End Property

  Public Property ShowAbsolute As Boolean
    Get
      Return pShowAbsolute
    End Get
    Set(value As Boolean)
      If pShowAbsolute = value Then Exit Property
      pShowAbsolute = value
      UpdateCommonYaxis()
      CambiaStatoSfondo()
      ToggleSeriesSource()
      OnPropertyChanged("ShowAbsolute")
      AggiornaDisplayValori()
    End Set
  End Property

  Public Property ShowDerivative As Boolean
    Get
      Return pShowDerivative
    End Get
    Set(value As Boolean)
      If pShowDerivative = value Then Exit Property
      pShowDerivative = value
      CambiaStatoSfondo()
      UpdateCommonYaxis()
      ToggleSeriesSource()
      OnPropertyChanged("ShowDerivative")
      AggiornaDisplayValori()
    End Set
  End Property

  Public Property ShowWindwardLeeward As Boolean
    Get
      Return _ShowWindwardLeeward
    End Get
    Set(value As Boolean)
      _ShowWindwardLeeward = value
      CambiaStatoSfondo()
      UpdateCommonYaxis()
      ToggleSeriesSource()
      OnPropertyChanged("ShowWindwardLeeward")
      AggiornaDisplayValori()
    End Set
  End Property

  Public Property CommonYaxis As Boolean
    Get
      Return _CommonYaxis
    End Get
    Set(value As Boolean)
      If _CommonYaxis = value Then Exit Property
      _CommonYaxis = value
      UpdateCommonYaxis()
    End Set
  End Property

  Public ReadOnly Property CmdSelezionaCanale() As ICommand
    Get
      Return pCmdSelezionaCanale
    End Get
  End Property

  Private Sub CambiaStatoSfondo()
    If _Annotazioni Is Nothing Then Exit Sub
    For Each Annotazione As SciChart.Charting.Visuals.Annotations.IAnnotation In _Annotazioni
      If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
        If DirectCast(DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation).Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSfondo Then
          Annotazione.IsHidden = Not (ShowAbsolute OrElse ShowWindwardLeeward OrElse ShowDerivative)
        End If
      End If
    Next
  End Sub

  Public Property Annotazioni As SciChart.Charting.Visuals.Annotations.AnnotationCollection
    Get
      Return _Annotazioni ' .Union(vv)
    End Get
    Set(value As SciChart.Charting.Visuals.Annotations.AnnotationCollection)
      If _Annotazioni Is value Then Exit Property
      _Annotazioni = value
      OnPropertyChanged("Annotazioni")
    End Set
  End Property

  Public Property SeriesSource As ObservableCollection(Of IChartSeriesViewModel)
    Get
      Return pSeriesSource
    End Get
    Set(value As ObservableCollection(Of IChartSeriesViewModel))
      If pSeriesSource Is value Then Exit Property
      pSeriesSource = value
      OnPropertyChanged("SeriesSource")
    End Set
  End Property

  Public Property CanaleAscissa As clsChannel2020
    Get
      Return pCanaleAscissa
    End Get
    Set(value As clsChannel2020)
      pCanaleAscissa = value
    End Set
  End Property

  Public Property CanaliOrdinata As ObservableCollection(Of clsChannel2020)
    Get
      Return pCanaliOrdinata
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      If pCanaliOrdinata Is value Then Exit Property
      pCanaliOrdinata = value
      OnPropertyChanged("CanaliOrdinata")
    End Set
  End Property

  Public Property ListaParent As clsMatriceControlliSinglePeriod
    Get
      Return pListaParent
    End Get
    Set(value As clsMatriceControlliSinglePeriod)
      pListaParent = value
    End Set
  End Property

  Public Property LineType As clsGroupLines.eLineType
    Get
      Return _LineType
    End Get
    Set(value As clsGroupLines.eLineType)
      _LineType = value
      OnPropertyChanged("LineType")
    End Set
  End Property

  Public Property ShowPortStbdBackGround As Boolean
    Get
      Return _ShowPortStbdBackGround
    End Get
    Set(value As Boolean)
      _ShowPortStbdBackGround = value
      OnPropertyChanged("ShowPortStbdBackGround")
    End Set
  End Property

  Public Property ShowTargetIfAvailable As Boolean
    Get
      Return _ShowTargetIfAvailable
    End Get
    Set(value As Boolean)
      _ShowTargetIfAvailable = value
      OnPropertyChanged("ShowTargetIfAvailable")
    End Set
  End Property

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Private Sub ToggleSeriesSource()
    UpdateSeriesSource()
  End Sub


  Private Sub UpdateSeriesSource()
    SeriesSource.Clear()
    Dim ShowAxisFormat As Boolean = True
    For Each s In _GroupLines
      If Not s.Tgt Is Nothing Then SeriesSource.Add(s.Tgt)
      Select Case _LineType
        Case clsGroupLines.eLineType.eValue
          SeriesSource.Add(s.Main)
        Case clsGroupLines.eLineType.eValueDeriv
          SeriesSource.Add(s.Deriv)
        Case clsGroupLines.eLineType.eDataTypeSigned
          SeriesSource.Add(s.Stbd)
          SeriesSource.Add(s.Port)
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
          SeriesSource.Add(s.WwdLwd)
        Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
          SeriesSource.Add(s.WwdLwdDeriv)
      End Select
    Next

    For Each Asse In Xassi
          Asse.DrawMajorGridLines = True 'ShowAxisFormat
          Asse.DrawMinorGridLines = ShowAxisFormat
          If ShowAxisFormat Then
            Asse.AxisBandsFill = Color.FromArgb(5, 33, 33, 33)
          Else
            Asse.AxisBandsFill = Nothing
          End If
          Asse.DrawMajorBands = ShowAxisFormat
        Next

        For Each Asse In Yassi
          If Asse Is Yassi.First Then
            Asse.DrawMajorGridLines = True 'ShowAxisFormat
            Asse.DrawMinorGridLines = ShowAxisFormat
            If ShowAxisFormat Then
              Asse.AxisBandsFill = Color.FromArgb(5, 33, 33, 33)
            Else
              Asse.AxisBandsFill = Nothing
            End If
            Asse.DrawMajorBands = ShowAxisFormat
          Else
            Asse.DrawMajorGridLines = False 'ShowAxisFormat
            Asse.DrawMinorGridLines = False
            Asse.AxisBandsFill = Nothing
            Asse.DrawMajorBands = False
          End If
        Next

  End Sub


  Private Sub UpdateCommonYaxis()
    If pCanaliOrdinata.Count = 0 Then Exit Sub
    If pCanaliOrdinata.Count = 1 Then
      Yassi.First.AxisTitle = NomeDaCanale(pYasseTitle)
      Exit Sub
    End If
    If _CommonYaxis Then
      'un solo asse
      For Each Canale In _GroupLines
        Canale.Main.RenderSeries.YAxisId = pYassiOrg.First.Id
        Canale.Deriv.RenderSeries.YAxisId = pYassiOrg.First.Id
        Canale.WwdLwd.RenderSeries.YAxisId = pYassiOrg.First.Id
        Canale.WwdLwdDeriv.RenderSeries.YAxisId = pYassiOrg.First.Id
        Canale.Port.RenderSeries.YAxisId = pYassiOrg.First.Id
        Canale.Stbd.RenderSeries.YAxisId = pYassiOrg.First.Id
        If Not Canale.Tgt Is Nothing Then Canale.Tgt.RenderSeries.YAxisId = pYassiOrg.First.Id
      Next
      pYassi.Clear()
      pYassi.Add(pYassiOrg.First)
      Yassi.First.AxisTitle = NomeDaCanali()
    Else
      'assi separati per canale
      For i As Integer = 0 To pYassiOrg.Count - 1
        _GroupLines(i).Main.RenderSeries.YAxisId = pYassiOrg(i).Id
        _GroupLines(i).Deriv.RenderSeries.YAxisId = pYassiOrg(i).Id
        _GroupLines(i).WwdLwd.RenderSeries.YAxisId = pYassiOrg(i).Id
        _GroupLines(i).WwdLwdDeriv.RenderSeries.YAxisId = pYassiOrg(i).Id
        _GroupLines(i).Port.RenderSeries.YAxisId = pYassiOrg(i).Id
        _GroupLines(i).Stbd.RenderSeries.YAxisId = pYassiOrg(i).Id
        If Not _GroupLines(i).Tgt Is Nothing Then _GroupLines(i).Tgt.RenderSeries.YAxisId = pYassiOrg(i).Id
      Next
      pYassi.Clear()
      For Each Asse In pYassiOrg
        pYassi.Add(Asse)
      Next
      Yassi.First.AxisTitle = NomeDaCanale(pYasseTitle)
    End If
  End Sub

  Private Function NomeDaCanale(NomeDefault As String) As String
    Dim strTMP As String = ""
    If ShowDerivative Then
      strTMP = "Deriv of: "
    End If
    If ShowWindwardLeeward Then
      strTMP &= NomeDefault.Replace("Port", "Lwd").Replace("Stbd", "Wwd") & "-"
    Else
      strTMP &= NomeDefault
    End If
    Return strTMP
  End Function

  Public Function NomeDaCanali() As String
    Dim strTMP As String = ""
    If ShowDerivative Then
      strTMP = "Deriv of: "
    End If
    For Each canale In pCanaliOrdinata
      If Not canale Is Nothing Then
        If ShowWindwardLeeward Then
          strTMP &= canale.ShortName.Replace("Port", "Lwd").Replace("Stbd", "Wwd") & "-"
        Else
          strTMP &= canale.ShortName & "-"
        End If
      End If
    Next
    Return strTMP.TrimEnd("-")
  End Function


  Public Sub LoadTimePlotData(Reset As Boolean, CanaliOrdinata As List(Of clsChannel2020))
    LoadTimePlotData(Reset, CanaliOrdinata, pObjChartSyncManager.VisibleRange)
  End Sub

  Public Sub LoadTimePlotData(Reset As Boolean, CanaliOrdinata As IEnumerable(Of clsChannel2020))
    LoadTimePlotData(Reset, CanaliOrdinata, pObjChartSyncManager.VisibleRange)
  End Sub

  Private Function NomeAsse(Assi As AxisCollection, Nome As String) As String
    Dim Counter As Integer = 0
    For Each Asse In Assi
      If Asse.Id.StartsWith(Nome) Then
        Counter += 1
      End If
    Next
    If Counter = 0 Then
      Return Nome
    Else
      Return Nome & "_" & Counter
    End If
  End Function

  Private Function CanaleRipetuto(CanaliStampati As List(Of clsChannel2020), Canale As clsChannel2020) As Boolean
    'per non stampare due volte lo stesso canale...
    For Each CanaleTmp In CanaliStampati
      If Canale Is CanaleTmp Then Return True
    Next
    Return False
  End Function

  Private Enum eTwaMode
    eStbd = 0
    ePort = 1
    eHeadDeadToWind = 2
  End Enum


  Public Sub LoadTimePlotData(Reset As Boolean, rCanaliOrdinata As IEnumerable(Of clsChannel2020), TimeRangeToShow As clsTimeRange)
    Dim CurrentTwaMode As eTwaMode = eTwaMode.eHeadDeadToWind
    ObjContaTempo.StampaMillisecondiTrascorsi("   INIZIO TimePlot, Canali: " & String.Join("-", rCanaliOrdinata.Select(Function(x) x.ChannelId).ToList))
    _ShowTargetIfAvailable = True
    _ShowPortStbdBackGround = True

    If Not Reset Then
      If _GroupLines.Count > 0 Then
        If _GroupLines.Where(Function(x) x.IsLoaded(LineType)).Count = 0 Then
          ObjContaTempo.StampaMillisecondiTrascorsi("   FINE TimePlot, LineType: " & LineType.ToString & " giá caricata , Canali: " & String.Join("-", rCanaliOrdinata.Select(Function(x) x.ChannelId).ToList))
          Exit Sub
        End If
      End If
    End If

    Try
      Dim CanaliStampati As New List(Of clsChannel2020)
      Yassi = New AxisCollection()
      Xassi = New AxisCollection()
      pYassiOrg = New AxisCollection()
      _GroupLines.Clear()
      CanaleAscissa = Nothing
      CanaliOrdinata = New ObservableCollection(Of clsChannel2020)(rCanaliOrdinata)
      If Annotazioni Is Nothing Then Annotazioni = New SciChart.Charting.Visuals.Annotations.AnnotationCollection

      If CanaliOrdinata Is Nothing Then Exit Sub
      If CanaliOrdinata.Count = 0 Then Exit Sub

      Dim ID As Integer = 0
      Dim PrimoCanaleCompletato As Boolean = False
      If Not _Annotazioni Is Nothing Then
        For Each Annotazione As SciChart.Charting.Visuals.Annotations.IAnnotation In _Annotazioni
          If TypeOf (Annotazione) Is SciChart.Charting.Visuals.Annotations.BoxAnnotation Then
            If DirectCast(DirectCast(Annotazione, SciChart.Charting.Visuals.Annotations.BoxAnnotation).Tag, eGruppoAnnotazione) = eGruppoAnnotazione.eSfondo Then
              PrimoCanaleCompletato = True
              'se giá esiste l annotazione con colori tack in background non la rifa'
              Exit For
            End If
          End If
        Next
      End If

      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
      Dim CanaleTack As clsChannel2020 = CanaleTackDefault()

      Dim ListaTarget As New List(Of String)
      Dim tgtIsVmg As Boolean = False


      For Each CanaleOrdinata As clsChannel2020 In CanaliOrdinata
        CanaleOrdinata = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId) ' verifica che i dati del canale siano caricati

        If Not CanaleOrdinata Is Nothing AndAlso Not CanaleRipetuto(CanaliStampati, CanaleOrdinata) Then


          Dim OrdinataIsStbd As Boolean = True
          Dim Omologo As clsChannel2020 = CanaleOrdinata

          If CanaleOrdinata.ChannelId.IndexOf("Port") > -1 Then ', 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
            Omologo = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Port", "Stbd"))
            If Omologo Is Nothing Then Omologo = CanaleOrdinata
            OrdinataIsStbd = False
          ElseIf CanaleOrdinata.ChannelId.IndexOf("Stbd") > -1 Then ', 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
            Omologo = DataProvider2020.CanaleDbl(CanaleOrdinata.ChannelId.Replace("Stbd", "Port"))
            If Omologo Is Nothing Then Omologo = CanaleOrdinata
            OrdinataIsStbd = True
          End If

          Dim Tgt As clsTgt = TgtManager.Tgt

          Dim DataSeriesTarget As XyDataSeries(Of DateTime, Double) = Nothing
          Dim LineaTarget As FastLineRenderableSeries = Nothing

          Dim Colore As Color = ColoriDifferenziati(ID)

          Dim CanaleTgt As clsChannel2020 = Nothing
          If _ShowTargetIfAvailable Then
            If Reset OrElse _GroupLines.Where(Function(x) Not x.Tgt Is Nothing).Count = 0 Then
              If Not Tgt Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chTws Is Nothing Then
                If ListaTarget.Where(Function(x) x = CanaleOrdinata.PolarHeader).FirstOrDefault Is Nothing Then 'verifica che non sia stato gia'aggiunto (esempio canali port e stbd di una stessa funzione)
                  ListaTarget.Add(CanaleOrdinata.PolarHeader)
                  CanaleTgt = DataProvider2020.VerificaCanaleTarget(CanaleOrdinata) 'carica i valori target del canale
                  If Tgt.PolareDisponibile(CanaleOrdinata.PolarHeader.ToLower) Then
                    DataSeriesTarget = New XyDataSeries(Of DateTime, Double)
                    DataSeriesTarget.SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinata.LongName & " Target"
                    DataSeriesTarget.AcceptsUnsortedData = False
                    LineaTarget = New FastLineRenderableSeries
                    LineaTarget.XAxisId = "DefaultAxisId" ''Xasse.Id
                    LineaTarget.StrokeThickness = 4
                    If CanaliOrdinata.Count = 1 Then
                      LineaTarget.Stroke = Color.FromArgb(30, 0, 0, 255)
                    Else
                      LineaTarget.Stroke = Color.FromArgb(30, Colore.R, Colore.G, Colore.B)
                    End If
                    If CanaleOrdinata.CanaleChiave = clsChannels2020.eCanaliChiave.eVMG Then
                      tgtIsVmg = True
                    End If
                  End If
                ElseIf CanaleOrdinata.CanaleChiave = clsChannels2020.eCanaliChiave.eVMG Then
                  If ListaTarget.Where(Function(x) x = "Vmg").FirstOrDefault Is Nothing Then
                    ListaTarget.Add("Vmg")
                    If Tgt.PolareDisponibile("Vmg") Then
                      tgtIsVmg = True
                      DataSeriesTarget = New XyDataSeries(Of DateTime, Double)
                      DataSeriesTarget.SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinata.LongName & " Target"
                      DataSeriesTarget.AcceptsUnsortedData = False
                      LineaTarget = New FastLineRenderableSeries
                      LineaTarget.XAxisId = "DefaultAxisId" ''Xasse.Id
                      LineaTarget.Stroke = Color.FromArgb(30, Colore.R, Colore.G, Colore.B)
                      LineaTarget.StrokeThickness = 4
                      'LineaTarget.StrokeDashArray = {4, 4}
                    End If
                  End If
                End If
              End If
            End If
          End If


          CanaliStampati.Add(CanaleOrdinata)

          Dim DataSeriesValsOrStbd As New XyDataSeries(Of DateTime, Double)
          DataSeriesValsOrStbd.SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinata.LongName
          DataSeriesValsOrStbd.AcceptsUnsortedData = False

          Dim DataSeriesPort As New XyDataSeries(Of DateTime, Double)
          DataSeriesPort.SeriesName = TimeRangeToShow.StringaPeriodo & " " & CanaleOrdinata.LongName & " Port"
          DataSeriesPort.AcceptsUnsortedData = False

          ' per ogni canale tre linee
          ' valori normali, soli valori positivi e soli valori negativi
          Dim LineaValuesOrStbd As New FastLineRenderableSeries
          Dim LineaPort As New FastLineRenderableSeries

          Dim Yasse As New SciChart.Charting.Visuals.Axes.NumericAxis
          LineaValuesOrStbd.Stroke = Colore
          CanaleOrdinata.PrintedColor = LineaValuesOrStbd.Stroke

          If CanaleOrdinata.DataType = clsChannel2020.eDataType.e360 Then
            Yasse.LabelProvider = New cls360LabelProvider()
            CanaleOrdinata.ValoriIntervallo.AggiornaValori(TimeRangeToShow, CanaleOrdinata.DataType = clsChannel2020.eDataType.e180)
          End If


          LineaValuesOrStbd.XAxisId = "DefaultAxisId" ''Xasse.Id
          LineaPort.XAxisId = "DefaultAxisId" ''Xasse.Id
          LineaPort.Stroke = Colore

          Yasse.AxisTitle = CanaleOrdinata.LongName
          Yasse.AutoRange = SciChart.Charting.Visuals.Axes.AutoRange.Once
          Yasse.GrowBy = New DoubleRange(0.1, 0.1)

          Dim suka As New SciChart.Charting.Visuals.Axes.LabelProviders.DefaultTickLabel
          Dim LabelStyle As New Style(suka.GetType())
          LabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(Colore)))
          Yasse.TickLabelStyle = LabelStyle
          Dim suka2 As New AxisTitle()
          Dim TitleLabelStyle As New Style(suka2.GetType)
          TitleLabelStyle.Setters.Add(New Setter(Label.ForegroundProperty, New SolidColorBrush(Colore)))
          Yasse.TitleStyle = TitleLabelStyle


          If CanaleOrdinata Is CanaliOrdinata.First Then
            Dim Xasse As New SciChart.Charting.Visuals.Axes.DateTimeAxis
            Xasse.AxisTitle = ""
            Xasse.Id = "DefaultAxisId"
            Xasse.SubDayTextFormatting = "HH:mm:ss.fF"
            Dim VBR As New Binding("SharedXVisibleRange")
            VBR.Source = pObjChartSyncManager
            VBR.Mode = BindingMode.TwoWay
            Xasse.SetBinding(SciChart.Charting.Visuals.Axes.AxisCore.VisibleRangeProperty, VBR)
            Dim DR As New DateRange(DataProvider2020.TimeRange.Start, DataProvider2020.TimeRange.Finish)
            Xasse.VisibleRangeLimit = DR
            DR = New DateRange(TimeRangeToShow.Start, TimeRangeToShow.Finish)
            Xassi.Add(Xasse)
            Yasse.Id = NomeAsse(Yassi, "DefaultAxisId")
            pYasseTitle = Yasse.AxisTitle
          Else
            Yasse.Id = NomeAsse(Yassi, "Channel" & CanaleOrdinata.ChannelId)
          End If

          LineaValuesOrStbd.YAxisId = Yasse.Id
          LineaPort.YAxisId = Yasse.Id
          LineaValuesOrStbd.XAxisId = "DefaultAxisId"
          LineaPort.XAxisId = LineaValuesOrStbd.XAxisId
          If Not DataSeriesTarget Is Nothing Then
            LineaTarget.YAxisId = Yasse.Id
            LineaTarget.XAxisId = LineaValuesOrStbd.XAxisId
          End If


          Yassi.Add(Yasse)
          pYassiOrg.Add(Yasse)
          Dim Yprev As Double = 0
          Dim Avg As Double = CanaleOrdinata.ValoriIntervallo.Avg
          Dim PrevTwaMode As eTwaMode = eTwaMode.eHeadDeadToWind
          If Not CanaleOrdinata.Valori Is Nothing AndAlso CanaleOrdinata.Valori.Count > 0 Then
            Dim Xiniziale As DateTime = Nothing 'Intervallo.IdRigaIniziale)
            Dim LastValidData As DateTime = DataProvider2020.TimeStamps(0)
            Dim objDerivata As New clsDerivata(DataProvider2020.Hz, CanaleOrdinata.DataType = clsChannel2020.eDataType.e360, LastValidData)
            Dim objWwlLwdDeriv As New clsDerivata(DataProvider2020.Hz, CanaleOrdinata.DataType = clsChannel2020.eDataType.e360, LastValidData)


            Dim ValoriX As New List(Of DateTime)
            Dim ValoriYorStbd As New List(Of Double)
            Dim ValoriYport As New List(Of Double)
            Dim ValoriYtgt As New List(Of Double)

            For Indice As Integer = 0 To System.Math.Min(CanaleOrdinata.Valori.Count, DataProvider2020.TimeStamps.Count) - 1
              Dim X As DateTime = DataProvider2020.TimeStamps(Indice)
              Dim Y As Double = CanaleOrdinata.Valori(Indice)
              'Dim yWL As Double = Y
              If Double.IsNaN(Y) Then

              ElseIf X = Nothing Then

              ElseIf ValoriX.Count > 0 AndAlso X <= ValoriX.Last Then
                '              Stop
              Else
                LastValidData = X
                If Xiniziale = Nothing Then Xiniziale = X

                CurrentTwaMode = TwaMode(CanaleTack, Indice, PrevTwaMode)

                ValoriX.Add(X)


                If Not CanaleTgt Is Nothing Then
                  ValoriYtgt.Add(CanaleTgt.Valori(Indice))
                End If

                Select Case LineType
                  Case clsGroupLines.eLineType.eValue
                    ' plotta il canale così come da parquet senza applicare le regole del segno
                    ValoriYorStbd.Add(Y)
                  Case clsGroupLines.eLineType.eDataTypeSigned, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
                    Select Case CanaleOrdinata.DataType
                      Case clsChannel2020.eDataType.e360
                        'qui va gestito meglio il 360 gradi
                        If Avg > 340 OrElse Avg < 20 Then
                          If Y > 270 Then Y -= 360
                        End If
                        ValoriYorStbd.Add(Y)
                        ValoriYport.Add(Double.NaN)
                      Case clsChannel2020.eDataType.e180
                        Dim absY As Double = System.Math.Abs(Y)
                        ValoriYorStbd.Add(Y)
                        If Y < 0 Then
                          ValoriYport.Add(absY)
                        Else
                          ValoriYport.Add(Double.NaN)
                        End If
                      Case clsChannel2020.eDataType.eAbs180
                        Dim absY As Double = System.Math.Abs(Y)
                        ValoriYorStbd.Add(absY)
                        If Y < 0 Then
                          ValoriYport.Add(absY)
                        Else
                          ValoriYport.Add(Double.NaN)
                        End If
                      Case clsChannel2020.eDataType.eTack
                        ValoriYorStbd.Add(Y)
                        If CurrentTwaMode = eTwaMode.eStbd Then
                          ValoriYport.Add(Y)
                        ElseIf CurrentTwaMode = eTwaMode.ePort Then
                          ValoriYport.Add(-Y)
                        Else
                          ValoriYport.Add(Double.NaN)
                        End If
                      Case clsChannel2020.eDataType.eTackReversed
                        ValoriYorStbd.Add(Y)
                        If CurrentTwaMode = eTwaMode.eStbd Then
                          ValoriYport.Add(-Y)
                        ElseIf CurrentTwaMode = eTwaMode.ePort Then
                          ValoriYport.Add(Y)
                        Else
                          ValoriYport.Add(Double.NaN)
                        End If
                      Case Else
                        ValoriYorStbd.Add(Y)
                        If CurrentTwaMode = eTwaMode.eStbd Then
                          ValoriYport.Add(Double.NaN)
                        ElseIf CurrentTwaMode = eTwaMode.ePort Then
                          ValoriYport.Add(Y)
                        Else
                          ValoriYport.Add(Double.NaN)
                        End If
                    End Select
                  Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
                    ' se il canale ha Port nel nome stampa sempre il lato sottovento
                    ' se il canale ha Stbd nel nome stampa sempre il lato sopravento
                    ' se non c'è port o stbd stampa 
                    ' se un  canale è di tipo absolute, 180,  tacksigned o reversed tack signed applica la convenzione
                    If Not DataProvider2020.IsStbd(Indice) Then
                      Y = Omologo.Valori(Indice)
                    End If
                    Select Case CanaleOrdinata.DataType
                      Case clsChannel2020.eDataType.e360
                        'qui va gestito meglio il 360 gradi
                        If Avg > 340 OrElse Avg < 20 Then
                          If Y > 270 Then Y -= 360
                        End If
                        ValoriYorStbd.Add(Y)
                      Case clsChannel2020.eDataType.e180
                        Dim absY As Double = System.Math.Abs(Y)
                        ValoriYorStbd.Add(Y)
                      Case clsChannel2020.eDataType.eAbs180
                        Dim absY As Double = System.Math.Abs(Y)
                        ValoriYorStbd.Add(absY)
                      Case clsChannel2020.eDataType.eTack
                        ValoriYorStbd.Add(Y)
                      Case clsChannel2020.eDataType.eTackReversed
                        ValoriYorStbd.Add(Y)
                      Case Else
                        ValoriYorStbd.Add(Y)
                    End Select

                  Case clsGroupLines.eLineType.eValueDeriv
                    ' derivata del canale prendendo i valori da value
                    If Not DataProvider2020.IsStbd(Indice) Then
                      Y = Omologo.Valori(Indice)
                    End If
                    If Not Double.IsNaN(Y) Then
                      If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
                      Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
                      objDerivata.MediaMobile.AggiornaMedia(Derivata)
                      ValoriYorStbd.Add(objDerivata.MediaMobile.Valore)
                      objDerivata.AggiornaValoriPrev(Y, X)
                    End If
                  Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
                    ' stampa la derivata dei valori Windward Leeward
                    If Not Double.IsNaN(Y) Then
                      If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
                      Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
                      objDerivata.MediaMobile.AggiornaMedia(Derivata)
                      ValoriYorStbd.Add(objDerivata.MediaMobile.Valore)
                      objDerivata.AggiornaValoriPrev(Y, X)
                    End If
                End Select

                'annotazioni sfondo
                If Not PrimoCanaleCompletato Then
                  If Not CurrentTwaMode = PrevTwaMode Then
                    Dim AnBA As New SciChart.Charting.Visuals.Annotations.BoxAnnotation
                    AnBA.X1 = Xiniziale
                    AnBA.X2 = X
                    AnBA.Y1 = 0
                    AnBA.Y2 = 1
                    If PrevTwaMode = eTwaMode.eStbd Then
                      AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 0, 255, 0))
                    ElseIf PrevTwaMode = eTwaMode.ePort Then
                      AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 255, 0, 0))
                    Else
                      AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
                    End If
                    AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
                    AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
                    AnBA.IsHidden = Not _ShowPortStbdBackGround
                    AnBA.Tag = eGruppoAnnotazione.eSfondo
                    Annotazioni.Add(AnBA)
                    Xiniziale = X
                  End If
                  PrevTwaMode = CurrentTwaMode
                End If
              End If
            Next
            DataSeriesValsOrStbd.Append(ValoriX.ToArray, ValoriYorStbd.ToArray)
            If ValoriYport.Count = ValoriX.Count Then
              DataSeriesPort.Append(ValoriX.ToArray, ValoriYport.ToArray)
            End If
            If Not DataSeriesTarget Is Nothing AndAlso ValoriYtgt.Count = ValoriX.Count Then
              DataSeriesTarget.Append(ValoriX.ToArray, ValoriYtgt.ToArray)
            End If
            If Not PrimoCanaleCompletato Then
              Dim AnBA As New SciChart.Charting.Visuals.Annotations.BoxAnnotation
              AnBA.X1 = Xiniziale
              AnBA.X2 = TimeRangeToShow.Finish
              AnBA.Y1 = 0
              AnBA.Y2 = 1
              If PrevTwaMode = eTwaMode.eStbd Then
                AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 0, 255, 0))
              ElseIf PrevTwaMode = eTwaMode.ePort Then
                AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 255, 0, 0))
              Else
                AnBA.Background = New SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
              End If
              AnBA.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.RelativeY
              AnBA.IsHidden = Not _ShowPortStbdBackGround
              AnBA.Tag = eGruppoAnnotazione.eSfondo
              AnBA.AnnotationCanvas = AnnotationCanvas.BelowChart
              Annotazioni.Add(AnBA)
            End If



            ' vengono associati i dati a ciscuna linea
            LineaValuesOrStbd.DataSeries = DataSeriesValsOrStbd
            LineaValuesOrStbd.ResamplingMode = SciChart.Data.Numerics.ResamplingMode.Auto ' MinMax
            LineaPort.DataSeries = DataSeriesPort

            ' il ChartSeriesViewModel con i valori normali viene creato e subito associato al series source
            Dim LineeDelCanale = _GroupLines.Where(Function(z) z.Canale Is CanaleOrdinata).FirstOrDefault
            If LineeDelCanale Is Nothing Then
              LineeDelCanale = New clsGroupLines(CanaleOrdinata)
              _GroupLines.Add(LineeDelCanale)
            End If
            Select Case LineType
              Case clsGroupLines.eLineType.eValue
                ' plotta il canale così come da parquet senza applicare le regole del segno
                Dim csvmValori As New ChartSeriesViewModel(DataSeriesValsOrStbd, LineaValuesOrStbd)
                LineeDelCanale.Main = csvmValori
              Case clsGroupLines.eLineType.eDataTypeSigned
                ' se un  canale è di tipo absolute, 180,  tacksigned o reversed tack signed applica la convenzione
                Dim csvmStbd As New ChartSeriesViewModel(DataSeriesValsOrStbd, LineaValuesOrStbd)
                Dim csvmPort = New ChartSeriesViewModel(DataSeriesPort, LineaPort)
                LineeDelCanale.Stbd = csvmStbd
                LineeDelCanale.Port = csvmPort
              Case clsGroupLines.eLineType.eValueDeriv
                ' derivata del canale prendendo i valori da value
                Dim csvmValori As New ChartSeriesViewModel(DataSeriesValsOrStbd, LineaValuesOrStbd)
                LineeDelCanale.Deriv = csvmValori
              Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd
                ' se il canale ha Port nel nome stampa sempre il lato sottovento
                ' se il canale ha Stbd nel nome stampa sempre il lato sopravento
                ' se non c'è port o stbd stampa 
                Dim csvmValori As New ChartSeriesViewModel(DataSeriesValsOrStbd, LineaValuesOrStbd)
                LineeDelCanale.WwdLwd = csvmValori
              Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
                ' stampa la derivata dei valori Windward Leeward
                Dim csvmValori As New ChartSeriesViewModel(DataSeriesValsOrStbd, LineaValuesOrStbd)
                LineeDelCanale.WwdLwdDeriv = csvmValori
            End Select

            Dim cstgt As ChartSeriesViewModel = Nothing
            If Not DataSeriesTarget Is Nothing Then
              cstgt = New ChartSeriesViewModel(DataSeriesTarget, LineaTarget)
              LineeDelCanale.Tgt = cstgt
            End If

            PrimoCanaleCompletato = True
            ID += 1

          End If
        End If
      Next

      ImpostaAnnotazioni(False)
      UpdateSeriesSource()

    Catch ex As Exception
      Stop
    End Try

    ObjContaTempo.StampaMillisecondiTrascorsi("   FINE TimePlot, Canali: " & String.Join("-", rCanaliOrdinata.Select(Function(x) x.ChannelId).ToList))



  End Sub

  Private Sub ImpostaAnnotazioni(Reset As Boolean)
    If Not Reset Then
      If Not Annotazioni Is Nothing Then Exit Sub
    End If

    Annotazioni = New SciChart.Charting.Visuals.Annotations.AnnotationCollection

    Dim An As New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
    An.X1 = 7
    An.Y1 = 0
    An.Stroke = New SolidColorBrush(Colors.DarkOrange)
    An.StrokeThickness = 2
    Dim a As New DoubleCollection
    a.Add(2)
    a.Add(2)
    An.StrokeDashArray = a
    An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
    An.IsHidden = False
    An.Tag = eGruppoAnnotazione.eGruppo1
    Annotazioni.Add(An)

    If OneIsPercentage() Then
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 100
      An.Stroke = New SolidColorBrush(Colors.Green)
      An.StrokeThickness = 3
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 90
      An.Stroke = New SolidColorBrush(Colors.DarkRed)
      An.StrokeThickness = 2
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
      An = New SciChart.Charting.Visuals.Annotations.HorizontalLineAnnotation
      An.X1 = 7
      An.Y1 = 110
      An.Stroke = New SolidColorBrush(Colors.DarkGreen)
      An.StrokeThickness = 2
      An.StrokeDashArray = a
      An.CoordinateMode = SciChart.Charting.Visuals.Annotations.AnnotationCoordinateMode.Absolute
      An.BindingGroup = Nothing
      An.IsHidden = False
      An.Tag = eGruppoAnnotazione.eGruppo1
      Annotazioni.Add(An)
    End If

    Dim AnVL As New SciChart.Charting.Visuals.Annotations.VerticalLineAnnotation
    AnVL.X1 = ObjChartSyncManager.CurrentPosition
    AnVL.Stroke = New SolidColorBrush(Colors.OrangeRed)
    AnVL.StrokeThickness = 1
    AnVL.BindingGroup = Nothing
    AnVL.IsHidden = False
    AnVL.IsEditable = False
    AnVL.Tag = eGruppoAnnotazione.eGruppo2
    Annotazioni.Add(AnVL)

    Dim VbrVL = New Binding("CurrentPosition")
    VbrVL.Source = pObjChartSyncManager
    VbrVL.Mode = BindingMode.TwoWay
    AnVL.SetBinding(SciChart.Charting.Visuals.Annotations.AxisMarkerAnnotationForMvvm.X1Property, VbrVL)
  End Sub

  Private Function OneIsPercentage() As Boolean
    For Each Ch In pCanaliOrdinata
      If Not Ch Is Nothing Then
        If Ch.DataType = clsChannel2020.eDataType.ePercentage Then
          Return True
        End If
      End If
    Next
    Return False
  End Function

  Private Function IsStbd(CanaleTack As clsChannel2020, Indice As Integer, ValoreIfNaN As Boolean) As Boolean
    If CanaleTack Is Nothing Then Return ValoreIfNaN
    If Double.IsNaN(CanaleTack.Valori(Indice)) Then Return ValoreIfNaN
    Return CanaleTack.Valori(Indice) >= 0
  End Function

  Private Function TwaMode(CanaleTack As clsChannel2020, Indice As Integer, ValoreIfNaN As eTwaMode) As eTwaMode
    'If DataProvider2020.IsStbd(Indice) Then
    '  Return eTwaMode.eStbd
    'Else
    '  Return eTwaMode.ePort
    'End If

    If CanaleTack Is Nothing Then Return ValoreIfNaN
    If Double.IsNaN(CanaleTack.Valori(Indice)) Then Return ValoreIfNaN
    Dim Valore As Double = CanaleTack.Valori(Indice)
    If System.Math.Abs(Valore) < 5 OrElse System.Math.Abs(Valore) > 175 Then
      Return eTwaMode.eHeadDeadToWind
    Else
      If Valore > 0 Then
        Return eTwaMode.eStbd
      Else
        Return eTwaMode.ePort
      End If
    End If
  End Function

  Private Sub pObjChartSyncManager_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pObjChartSyncManager.PropertyChanged
    Select Case e.PropertyName
      Case "SharedXVisibleRange"
        AggiornaDisplayValori()
      Case "CurrentPosition"
        AggiornaCurrentValues()
    End Select

  End Sub

  Public Sub AggiornaCurrentValues()
    If pCanaliOrdinata Is Nothing Then Exit Sub
    For Each Canale In pCanaliOrdinata
      If Not Canale Is Nothing Then
        If Not Canale.Valori Is Nothing Then
          If Canale.Valori.Count > 0 AndAlso Canale.Valori.Count > pObjChartSyncManager.CurrentRow Then
            Canale.CurrentValue = Canale.Valori(pObjChartSyncManager.CurrentRow)
          End If
        End If
      End If
    Next
  End Sub

  Public Sub AggiornaDisplayValori()
    AggiornaDisplayValori(pObjChartSyncManager.VisibleRange)
  End Sub

  Public Sub AggiornaDisplayValori(Intervallo As clsTimeRange)
    If pCanaliOrdinata Is Nothing Then Exit Sub
    For Each Canale As clsChannel2020 In pCanaliOrdinata
      If Not Canale Is Nothing Then
        Canale.ValoriIntervallo.AggiornaValori(Intervallo, False)
      End If
    Next
  End Sub


  Public Sub SelezionaCanaliGrafico()
    ImpostaCanaliAsIsSelected(pCanaliOrdinata)
    Dim WPFpup As New UserControlSelChannel(UserControlSelChannel.eLoadedChannels.eLoaded, Nothing, DataProvider2020, True)
    If WPFpup.ShowDialog() Then
      Dim CM As clsChannel2020 = WPFpup.CanaleAscissa
      Dim CS As List(Of clsChannel2020) = WPFpup.Canali
      'DrawTimePlot(DataProvider2020.TimeStamps, CS)
      LoadTimePlotData(False, CS)
      AggiornaDisplayValori()
      pListaParent.SalvaImpostazione()
    End If
    ImpostaCanaliAsIsSelected(Nothing)
  End Sub

End Class

