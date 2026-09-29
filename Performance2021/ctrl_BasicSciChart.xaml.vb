Imports System.ComponentModel


Public Class ctrl_BasicSciChart
  Implements INotifyPropertyChanged

  Dim pID As Integer
  Dim pCanaleAscissa As clsChannel
  Dim pCanaliOrdinata As List(Of clsChannel)
  'Dim pZoomMode As SciChart.Charting.XyDirection
  Dim pRubberMode As eRubberMode
  Dim pMouseWheelZoomMode As eMouseWheelZoomMode
  Dim pTackOutput As clsStatistic.eOutput = clsStatistic.eOutput.eNormal
  Dim Xiniziale, Yiniziale As Double
  Dim Xfinale, Yfinale As Double
  Dim pObjSciChart As clsSciChart
  Dim pValoreAssoluto As Boolean

  Public Enum eMouseWheelZoomMode
    eDisabled = 0
    eXY = 1
    eXonly = 2
    eYonly = 3
  End Enum

  Public Enum eRubberMode
    eMousePan = 0
    eXonly = 1
    eXY = 2
    eSelection = 3
  End Enum

  Public Enum eOperazioni
    eSelezionaCanale = 0
    eToggleZoomMode = 1
    eToggleTackMode = 2
  End Enum

  Public ReadOnly Property ValoreAssoluto As Boolean
    Get
      Return pValoreAssoluto
    End Get
  End Property

  'Public Property ZoomMode As SciChart.Charting.XyDirection
  '  Get
  '    Return pZoomMode
  '  End Get
  '  Set(value As SciChart.Charting.XyDirection)
  '    If pZoomMode = value Then Exit Property
  '    pZoomMode = value
  '    '' OnPropertyChanged("ZoomMode")
  '  End Set
  'End Property

  'Public Property ObjSciChart As clsSciChart
  '	Get
  '		Return pObjSciChart
  '	End Get
  '	Set(value As clsSciChart)
  '		pObjSciChart = value
  '	End Set
  'End Property

  Public Property TackOutput As clsStatistic.eOutput
    Get
      Return pTackOutput
    End Get
    Set(value As clsStatistic.eOutput)
      pTackOutput = value
    End Set
  End Property

  Public Property CanaleAscissa As clsChannel
    Get
      Return pCanaleAscissa
    End Get
    Set(value As clsChannel)
      pCanaleAscissa = value
    End Set
  End Property

  Public Property ID As Integer
    Get
      Return pID
    End Get
    Set(value As Integer)
      pID = value
    End Set
  End Property

  Public Property CanaliOrdinata As List(Of clsChannel)
    Get
      Return pCanaliOrdinata
    End Get
    Set(value As List(Of clsChannel))
      pCanaliOrdinata = value
    End Set
  End Property


  Public Sub New(ID As Integer, CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel))

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    pID = ID
    pCanaleAscissa = CanaleAscissa
    pCanaliOrdinata = CanaliOrdinata

    ScMouseWheelZoomModifier.IsEnabled = False
    pMouseWheelZoomMode = eMouseWheelZoomMode.eDisabled

    'pRubberMode = eRubberMode.eMousePan
    'ScRubberBandXyZoomModifier.IsEnabled = False
    'ScZoomPanModifier.IsEnabled = True
    pRubberMode = eRubberMode.eXonly
    ScRubberBandXyZoomModifier.IsEnabled = True
    ScRubberBandXyZoomModifier.IsXAxisOnly = True
    ScZoomPanModifier.IsEnabled = False

    pTackOutput = clsStatistic.eOutput.eNormal
    abilitaTackOutputMode()

    pObjSciChart = New clsSciChart(SCsurface)

  End Sub

  Private Sub abilitaTackOutputMode()
    For Each canale As clsChannel In pCanaliOrdinata
      If canale.DataType = clsChannel.eDataType.e180 Then
        btn_180mode.IsEnabled = True
        Exit Sub
      End If
    Next
    btn_180mode.IsEnabled = False
  End Sub

  Public Sub DrawTimePlot(CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel), Intervallo As clsTimeRange, objSMACVM As clsSynchronizeMouseAcrossChartsViewModel)
    pValoreAssoluto = False
    For Each canale As clsChannel In pCanaliOrdinata
      If canale.DataType = clsChannel.eDataType.e180 Then
        pValoreAssoluto = True
        Exit For
      End If
    Next
    DrawTimePlot(CanaleAscissa, CanaliOrdinata, Intervallo, objSMACVM, pValoreAssoluto)
  End Sub

  Public Sub DrawTimePlot(CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel), Intervallo As clsTimeRange, objSMACVM As clsSynchronizeMouseAcrossChartsViewModel, ValoreAssoluto As Boolean)
    pCanaleAscissa = CanaleAscissa
    pCanaliOrdinata = CanaliOrdinata
    pValoreAssoluto = ValoreAssoluto
    Dim Intervalli As New List(Of clsTimeRange)
    Intervalli.Add(Intervallo)
    For Each canale As clsChannel In pCanaliOrdinata
      If canale.CanaleChiave = clsChannels.eChannels.eTWA Then
        pObjSciChart.DrawTimePlotForceAbsolute(pCanaleAscissa, pCanaliOrdinata, Intervallo, objSMACVM)
        Exit Sub
      End If
    Next
    pObjSciChart.DrawTimePlot(pCanaleAscissa, pCanaliOrdinata, Intervalli, objSMACVM, False, False, pValoreAssoluto)
  End Sub


  'Public Sub New(ID As Integer)

  '	' This call is required by the designer.
  '	InitializeComponent()

  '	' Add any initialization after the InitializeComponent() call.
  '	pID = ID
  '	pZoomMode = SciChart.Charting.XyDirection.XDirection
  '	pRubberMode = eRubberMode.eMouseDrag
  '	ScRubberBandXyZoomModifier.IsEnabled = False
  'End Sub

  Public Event MouseUppato(sender As ctrl_BasicSciChart)

  'Public Event ZoomHistory(sender As ctrl_BasicSciChart, Back As Boolean)

  Public Event BottoneSchiacciato(Operazione As eOperazioni, sender As ctrl_BasicSciChart)

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Private Sub SplitGraficiBasic_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs) Handles SplitGraficiBasic.DragCompleted
    Dim Delta As Double = e.VerticalChange
    Dim H As Double = SCsurface.Height
    ''SCsurface.Height += Delta
    If Double.IsNaN(gridChart.Height) Then
      gridChart.Height = gridChart.ActualHeight
    End If
    gridChart.Height += Delta


  End Sub

  Public Property RubberMode As eRubberMode
    Get
      Return pRubberMode
    End Get
    Set(value As eRubberMode)
      pRubberMode = value
    End Set
  End Property

  Public ReadOnly Property RubberModeToolTip As String
    Get
      Dim strTmp As String = "X"
      Select Case pRubberMode
        Case eRubberMode.eSelection
          Return "Mouse Mode Selection, Click to Mouse Pan"
        Case eRubberMode.eMousePan
          Return "Mouse Pan Mode, Click to Selection Mode ZoomXY"
        Case eRubberMode.eXY
          Return "Selection Mode ZoomXY, Click to Selection Mode ZoomX Only"
        Case eRubberMode.eXonly
          Return "Selection Mode ZoomX Only, Click to Selection Mode"
      End Select
      Return strTmp
    End Get
  End Property

  Public Property MouseWheelZoomMode As eRubberMode
    Get
      Return pMouseWheelZoomMode
    End Get
    Set(value As eRubberMode)
      pMouseWheelZoomMode = value
    End Set
  End Property

  Public ReadOnly Property MouseWheelZoomModeToolTip As String
    Get
      Dim strTmp As String = "X"
      Select Case pMouseWheelZoomMode
        Case eMouseWheelZoomMode.eDisabled
          strTmp = "Mouse Wheel Mode Disabled, Click to Zoom XY"
        Case eMouseWheelZoomMode.eXY
          strTmp = "Mouse Wheel Mode ZoomXY, Click to Zoom X Only"
        Case eMouseWheelZoomMode.eXonly
          strTmp = "mouse Wheel Mode Zoom X only , Click to Zoom Y Only"
        Case eMouseWheelZoomMode.eYonly
          strTmp = "Mouse Wheel Mode Zoom Y only , Click to Disable it"
      End Select
      Return strTmp
    End Get
  End Property


  Private Sub btn_SelectChannels_Click(sender As Object, e As RoutedEventArgs) Handles btn_SelectChannels.Click
    RaiseEvent BottoneSchiacciato(eOperazioni.eSelezionaCanale, Me)
  End Sub

  Public Function ToggleZoomMode() As String
    Dim strTmp As String = "X"
    Select Case pMouseWheelZoomMode
      Case eMouseWheelZoomMode.eDisabled
        ScMouseWheelZoomModifier.IsEnabled = True
        ScMouseWheelZoomModifier.XyDirection = SciChart.Charting.XyDirection.XYDirection
        pMouseWheelZoomMode = eMouseWheelZoomMode.eXY
        strTmp = "ZoomXY"
      Case eMouseWheelZoomMode.eXY
        ScMouseWheelZoomModifier.IsEnabled = True
        ScMouseWheelZoomModifier.XyDirection = SciChart.Charting.XyDirection.XDirection
        pMouseWheelZoomMode = eMouseWheelZoomMode.eXonly
        strTmp = "ZoomX"
      Case eMouseWheelZoomMode.eXonly
        ScMouseWheelZoomModifier.IsEnabled = True
        ScMouseWheelZoomModifier.XyDirection = SciChart.Charting.XyDirection.YDirection
        pMouseWheelZoomMode = eMouseWheelZoomMode.eYonly
        strTmp = "ZoomY"
      Case eMouseWheelZoomMode.eYonly
        ScMouseWheelZoomModifier.IsEnabled = False
        pMouseWheelZoomMode = eMouseWheelZoomMode.eDisabled
        strTmp = "Off"
    End Select
    Return strTmp
  End Function

  'Private Sub btn_ToggleZoomMode_Click(sender As Object, e As RoutedEventArgs) Handles btn_ToggleZoomMode.Click
  '	ToggleZoomMode()
  '	'RaiseEvent BottoneSchiacciato(eOperazioni.eToggleZoomMode, Me)
  'End Sub

  Private Sub MouseWheelZoomModifier_MouseWheel(sender As Object, e As MouseWheelEventArgs)
    Stop
  End Sub

  Public Function ToggleRubberMode() As String
    Select Case pRubberMode
      Case eRubberMode.eSelection
        pRubberMode = eRubberMode.eMousePan
        ScRubberBandXyZoomModifier.IsEnabled = False
        ScZoomPanModifier.IsEnabled = True
        Return "Pan"
      Case eRubberMode.eMousePan
        pRubberMode = eRubberMode.eXY
        ScRubberBandXyZoomModifier.IsEnabled = True
        ScRubberBandXyZoomModifier.IsXAxisOnly = False
        ScZoomPanModifier.IsEnabled = False
        Return "ZoomXY"
      Case eRubberMode.eXY
        pRubberMode = eRubberMode.eXonly
        ScRubberBandXyZoomModifier.IsEnabled = True
        ScRubberBandXyZoomModifier.IsXAxisOnly = True
        ScZoomPanModifier.IsEnabled = False
        Return "ZoomX"
      Case eRubberMode.eXonly
        pRubberMode = eRubberMode.eSelection
        ScRubberBandXyZoomModifier.IsEnabled = False
        ScZoomPanModifier.IsEnabled = False
        Return "Sel"
    End Select

    Return "X"

  End Function

  'Private Sub btn_ToggleRubberMode_Click(sender As Object, e As RoutedEventArgs) Handles btn_ToggleRubberMode.Click
  '	ToggleRubberMode()
  'End Sub

  Private Sub SCsurface_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseDown
    Stop
  End Sub

  Private Sub SCsurface_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseUp
    'Stop
    RaiseEvent MouseUppato(Me)
  End Sub

  ' Private Sub btn_ZoomBack_Click(sender As Object, e As RoutedEventArgs) Handles btn_ZoomBack.Click
  '   RaiseEvent ZoomHistory(Me, True)
  ' End Sub

  'Private Sub btn_ZoomFwd_Click(sender As Object, e As RoutedEventArgs) Handles btn_ZoomFwd.Click
  '	RaiseEvent ZoomHistory(Me, False)
  'End Sub

  Public Sub ImpostaAltezza(Altezza As Integer)
    gridChart.Height = Altezza
  End Sub

  Private Sub Btn_180mode_Click(sender As Object, e As RoutedEventArgs) Handles btn_180mode.Click
    btn_180mode.Content = ToggleNormalAbsoluteTack()
  End Sub

  Public Function ToggleNormalAbsoluteTack() As String
    Dim strTmp As String = "X"
    Select Case pTackOutput
      Case clsStatistic.eOutput.eNormal
        pTackOutput = clsStatistic.eOutput.eAbsolute
        strTmp = "Absolute"
      Case clsStatistic.eOutput.eAbsolute
        pTackOutput = clsStatistic.eOutput.eZeroAndPositives
        strTmp = "Stbd"
      Case clsStatistic.eOutput.eZeroAndPositives
        pTackOutput = clsStatistic.eOutput.eNegatives
        strTmp = "Port"
      Case clsStatistic.eOutput.eNegatives
        pTackOutput = clsStatistic.eOutput.eNormal
        strTmp = "Normal"
    End Select
    RaiseEvent BottoneSchiacciato(eOperazioni.eToggleTackMode, Me)
    Return strTmp
  End Function


End Class

