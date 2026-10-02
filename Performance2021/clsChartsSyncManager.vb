Imports System.Collections.Specialized
Imports System.ComponentModel
Imports PropertyChanged
Imports SciChart.Charting.ChartModifiers
Imports SciChart.Charting.Visuals
Imports SciChart.Charting.Visuals.Axes
Imports SciChart.Core.Framework
Imports SciChart.Core.Utility
Imports SciChart.Core.Utility.Mouse
Imports SciChart.Data.Model

<AddINotifyPropertyChangedInterface>
Public Class clsChartSyncManager
  'Implements INotifyPropertyChanged

  Public Property MouseWheelEnabled As Boolean
  Public Property MouseWheelXYdirection As SciChart.Charting.XyDirection

  Public Property PanEnabled As Boolean

  Public Property RolloverEnabled As Boolean
  Public Property CursorEnabled As Boolean

  Public Property ZoomPanModifierIsEnabled As Boolean
  Public Property ZoomExtentsY As Boolean
  Public Property ZoomPanModifierXYdirection As SciChart.Charting.XyDirection

  Public Property RubberModeIsEnabled As Boolean
  Public Property RubberModeIsXAxisOnly As Boolean

  Dim _SharedXVisibleRange As IRange
  'Public Property SharedXVisibleRange As IRange
  Public Property VisibleRange As New clsTimeRange
  Public Property VerticalBarPosition As DateTime

  Dim _CurrentPosition As DateTime
  'Public Property CurrentRow As Integer

  'Public Property VisibleRanges(10) As IRange
  Dim VRinx As Integer = -1

  Public Property RubberMode As eRubberMode
  Public Property MouseWheelZoomMode As eMouseWheelZoomMode

  Public Property SetCurrentVisibleRange As Boolean = True

  Public Property DataBoxWidth As New GridLength(90, GridUnitType.Pixel)

  Public Event VisibleRangeChanged(TimeRange As clsTimeRange)
  Public Event MousePositionChanged(Position As DateTime)



  Public Enum eMouseWheelZoomMode
    eDisabled = 0
    eXY = 1
    eXonly = 2
    eYonly = 3
  End Enum

  Public Enum eRubberMode
    eXonly = 0
    eXY = 1
    'eSelection = 2
  End Enum

  Public Sub AggiornaVisibleRange(NuovoRange As clsTimeRange, ScatenaEvento As Boolean)
    VisibleRange.Start = NuovoRange.Start
    VisibleRange.Finish = NuovoRange.Finish
    If SharedXVisibleRange Is Nothing Then
      _SharedXVisibleRange = New DateRange(VisibleRange.Start, VisibleRange.Finish)
    End If
    If NuovoRange.Start > DirectCast(SharedXVisibleRange.Max, DateTime) Then
      SharedXVisibleRange.Max = VisibleRange.Finish
      SharedXVisibleRange.Min = VisibleRange.Start
    Else
      SharedXVisibleRange.Min = VisibleRange.Start
      SharedXVisibleRange.Max = VisibleRange.Finish
    End If
    RaiseEvent VisibleRangeChanged(VisibleRange)
    'If ScatenaEvento Then OnPropertyChanged("SharedXVisibleRange")
  End Sub

  'Public ReadOnly Property VisibleRange As clsTimeRange
  '  Get
  '    Return pVisibleRange
  '  End Get
  '  'Set(value As clsTimeRange)
  '  '  If pVisibleRange Is value Then Exit Property
  '  '  pVisibleRange = value
  '  '  OnPropertyChanged("VisibleRange")
  '  'End Set
  'End Property

  Public Property SharedXVisibleRange As IRange
    Get
      Return _SharedXVisibleRange
    End Get
    Set(value As IRange)
            If _SharedXVisibleRange Is value Then Exit Property
            If value Is Nothing Then
                _SharedXVisibleRange = Nothing
                Exit Property
            End If
            If Not value.Max.CompareTo(_SharedXVisibleRange.Max) = 0 OrElse Not value.Min.CompareTo(_SharedXVisibleRange.Min) = 0 Then
                If DirectCast(value.Max, DateTime) < DirectCast(value.Min, DateTime) Then
                    _SharedXVisibleRange.SetMinMax(value.Max, value.Min)
                Else
                    _SharedXVisibleRange = value
                End If
                _VisibleRange.Start = DirectCast(_SharedXVisibleRange.Min, DateTime)
                _VisibleRange.Finish = DirectCast(_SharedXVisibleRange.Max, DateTime)
                GraficoEventiViewModel.AggiornaSelezione(VisibleRange)
                RaiseEvent VisibleRangeChanged(VisibleRange)
                'AggiornaYrange()
            Else
                'Console.WriteLine("SAME RANGE")
            End If
    End Set
  End Property

  'Public Sub AggiornaYrange()
  '  'If Not Mouse.LeftButton = MouseButtonState.Pressed Then
  '  For Each controllo In MatriceControlliBase.MatriceControlli
  '    If Not controllo.SCsurface.YAxes Is Nothing Then
  '      If Not controllo.SCsurface.YAxes.Count = 0 Then
  '        If Not controllo.SCsurface.YAxes.First Is Nothing Then
  '          controllo.SCsurface.ZoomExtentsY()
  '        End If
  '      End If
  '    End If
  '  Next
  '  'End If

  'End Sub


  Public Sub AggiornaYrange()
    If MatriceControlliBase Is Nothing Then Exit Sub
    For Each Vm In MatriceControlliBase.MatriceControlli
      If Vm.Surface Is Nothing Then Continue For
      If Vm.Surface.YAxes Is Nothing Then Continue For
      If Vm.Surface.YAxes.Count = 0 Then Continue For
      If Vm.Surface.YAxes.First Is Nothing Then Continue For
      Vm.ZoomExtentsYConRange()
    Next
  End Sub

  Public Property CurrentPosition As Date
    Get
      Return _CurrentPosition
    End Get
    Set(value As Date)
      If _CurrentPosition = value Then Exit Property
      _CurrentPosition = value
      RaiseEvent MousePositionChanged(value)
    End Set
  End Property

  Public ReadOnly Property CurrentRow As Integer
    Get
      Return DataProvider2020.TrovaIndice(_CurrentPosition)
    End Get
  End Property

  'Public Property MouseWheelEnabled As Boolean
  '  Get
  '    Return pMouseWheelEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pMouseWheelEnabled = value Then Exit Property
  '    pMouseWheelEnabled = value
  '    OnPropertyChanged("MouseWheelEnabled")
  '  End Set
  'End Property

  'Public Property MouseWheelXYdirection As SciChart.Charting.XyDirection
  '  Get
  '    Return pMouseWheelXYdirection
  '  End Get
  '  Set(value As SciChart.Charting.XyDirection)
  '    If pMouseWheelXYdirection = value Then Exit Property
  '    pMouseWheelXYdirection = value
  '    ZoomExtentsY = (pMouseWheelXYdirection = SciChart.Charting.XyDirection.YDirection)
  '    OnPropertyChanged("MouseWheelXYdirection")
  '  End Set
  'End Property

  'Public Property ZoomPanModifierIsEnabled As Boolean
  '  Get
  '    Return pZoomPanModifierIsEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pZoomPanModifierIsEnabled = value Then Exit Property
  '    pZoomPanModifierIsEnabled = value
  '    OnPropertyChanged("ZoomPanModifierIsEnabled")
  '  End Set
  'End Property

  'Public Property ZoomExtentsY As Boolean
  '  Get
  '    Return pZoomExtentsY
  '  End Get
  '  Set(value As Boolean)
  '    If pZoomExtentsY = value Then Exit Property
  '    pZoomExtentsY = value
  '    OnPropertyChanged("ZoomExtentsY")
  '  End Set
  'End Property

  'Public Property ZoomPanModifierXYdirection As SciChart.Charting.XyDirection
  '  Get
  '    Return pZoomPanModifierXYdirection
  '  End Get
  '  Set(value As SciChart.Charting.XyDirection)
  '    If pZoomPanModifierXYdirection = value Then Exit Property
  '    pZoomPanModifierXYdirection = value
  '    OnPropertyChanged("ZoomPanModifierXYdirection")
  '  End Set
  'End Property

  'Public Property RubberModeIsEnabled As Boolean
  '  Get
  '    Return pRubberModeIsEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pRubberModeIsEnabled = value Then Exit Property
  '    pRubberModeIsEnabled = value
  '    OnPropertyChanged("RubberModeIsEnabled")
  '  End Set
  'End Property

  'Public Property RubberModeIsXAxisOnly As Boolean
  '  Get
  '    Return pRubberModeIsXAxisOnly
  '  End Get
  '  Set(value As Boolean)
  '    If pRubberModeIsXAxisOnly = value Then Exit Property
  '    pRubberModeIsXAxisOnly = value
  '    OnPropertyChanged("RubberModeIsXAxisOnly")
  '  End Set
  'End Property

  'Public Property CursorEnabled As Boolean
  '  Get
  '    Return pCursorEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pCursorEnabled = value Then Exit Property
  '    pCursorEnabled = value
  '    OnPropertyChanged("CursorEnabled")
  '  End Set
  'End Property

  'Public Property RolloverEnabled As Boolean
  '  Get
  '    Return pRolloverEnabled
  '  End Get
  '  Set(value As Boolean)
  '    If pRolloverEnabled = value Then Exit Property
  '    pRolloverEnabled = value
  '    OnPropertyChanged("RolloverEnabled")
  '  End Set
  'End Property

  'Public Property VerticalBarPosition As DateTime
  '  Get
  '    Return pVerticalBarPosition
  '  End Get
  '  Set(value As DateTime)
  '    If pVerticalBarPosition = value Then Exit Property
  '    pVerticalBarPosition = value
  '    OnPropertyChanged("VerticalBarPosition")
  '  End Set
  'End Property

  'Public Property DataBoxWidth As GridLength
  '  Get
  '    Return pDataBoxWidth
  '  End Get
  '  Set(value As GridLength)
  '    If pDataBoxWidth = value Then Exit Property
  '    pDataBoxWidth = value
  '    OnPropertyChanged("DataBoxWidth")
  '  End Set
  'End Property

  'Public Sub MouseUppato()
  '  AggiornaVisibleRanges(pSharedXVisibleRange)
  'End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)

  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub


  'Public Sub SetCurrentVisibleRange()
  '  ' imposta questo boleano in modo che al primo cambiamento dello shared visible range venga aggiornata la zoom history
  '  pSetCurrentVisibleRange = True
  'End Sub

  'Private Sub SetZoomHistory()
  '  If Not pSetCurrentVisibleRange Then Exit Sub
  '  pSetCurrentVisibleRange = False
  '  'il visible range memorizzato é quello precedente all'ultimo visualizzato 
  '  'questo perché viene gestito dal mouse up
  '  VRinx += 1
  '  If VRinx > pVisibleRanges.Count - 1 Then VRinx = 0
  '  pVisibleRanges(VRinx) = pSharedXVisibleRange.Clone
  'End Sub

  ''Public Sub SetVisibleRange(CurrentVisibleRange As IRange)
  ''  'il visible range memorizzato é quello precedente all'ultimo visualizzato 
  ''  'questo perché viene gestito dal mouse up
  ''  VRinx += 1
  ''  If VRinx > pVisibleRanges.Count - 1 Then VRinx = 0
  ''  pVisibleRanges(VRinx) = CurrentVisibleRange
  ''End Sub

  'Public Function GetVisibleRange(Prev As Boolean) As IRange
  '  If VRinx = -1 Then VRinx = 0
  '  Dim VRtmp As IRange = pVisibleRanges(VRinx)
  '  If Not Prev Then
  '    VRinx += 2
  '    If VRinx > pVisibleRanges.Count - 1 Then VRinx = (VRinx - pVisibleRanges.Count)
  '    VRtmp = pVisibleRanges(VRinx)
  '  End If
  '  VRinx -= 1
  '  If VRinx < 0 Then VRinx = pVisibleRanges.Count - 1
  '  Return VRtmp
  'End Function

  Public Function ToggleMouseWheel() As String
    Select Case MouseWheelZoomMode
      Case eMouseWheelZoomMode.eXY
        Return SetMouseWheelMode(eMouseWheelZoomMode.eXonly)
      Case eMouseWheelZoomMode.eXonly
        Return SetMouseWheelMode(eMouseWheelZoomMode.eYonly)
      Case eMouseWheelZoomMode.eYonly
        Return SetMouseWheelMode(eMouseWheelZoomMode.eXY)
    End Select
    Return SetMouseWheelMode(eMouseWheelZoomMode.eDisabled)

  End Function


  Public Function SetMouseWheelMode(MouseWheelZoomMode As eMouseWheelZoomMode) As String
    Select Case MouseWheelZoomMode
      Case eMouseWheelZoomMode.eXY
        MouseWheelXYdirection = SciChart.Charting.XyDirection.XYDirection
        MouseWheelZoomMode = eMouseWheelZoomMode.eXY
        Return "ZoomXY"
      Case eMouseWheelZoomMode.eXonly
        MouseWheelXYdirection = SciChart.Charting.XyDirection.XDirection
        MouseWheelZoomMode = eMouseWheelZoomMode.eXonly
        Return "ZoomX"
      Case eMouseWheelZoomMode.eYonly
        MouseWheelXYdirection = SciChart.Charting.XyDirection.YDirection
        MouseWheelZoomMode = eMouseWheelZoomMode.eYonly
        Return "ZoomY"
    End Select
    Return "Error"

  End Function


  Public Function TogglePanSelect() As String
    Select Case RubberMode
      'Case eRubberMode.eSelection
      '	pRubberMode = eRubberMode.eXonly
      '	RubberModeIsEnabled = True
      '	RubberModeIsXAxisOnly = True
      '	Return "ZoomX"
      Case eRubberMode.eXonly
        Return SetPanSelect(eRubberMode.eXY)
        'pRubberMode = eRubberMode.eXY
        'RubberModeIsEnabled = True
        'RubberModeIsXAxisOnly = False
        'Return "ZoomXY"
      Case eRubberMode.eXY
        Return SetPanSelect(eRubberMode.eXonly)
        'pRubberMode = eRubberMode.eSelection
        'RubberModeIsEnabled = False
        'Return "Sel"
    End Select

    Return "ErRor"

  End Function

  Public Function SetPanSelect(RubberMode As eRubberMode) As String
    Select Case RubberMode
      Case eRubberMode.eXonly
        RubberMode = eRubberMode.eXonly
        RubberModeIsXAxisOnly = True
        Return "ZoomX"
      Case eRubberMode.eXY
        RubberMode = eRubberMode.eXY
        RubberModeIsXAxisOnly = False
        Return "ZoomXY"
    End Select

    Return "Error"

  End Function


End Class

