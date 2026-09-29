Imports System.ComponentModel
Imports System.Windows.Controls.Primitives

Public Class ctrl_DetailSciChart

	Dim pCanaleAscissa As clsChannel
	Dim pCanaliOrdinata As List(Of clsChannel)
	Dim pObjSciChart As clsSciChart
	WithEvents pObjSMACVM As clsSynchronizeMouseAcrossChartsViewModel
	Dim pIntervalli As List(Of clsTimeRange)
	Dim pAsseYunificato As Boolean
	Dim pRubberMode As eRubberMode
	Dim pMouseWheelZoomMode As eMouseWheelZoomMode
	Dim pTackOutput As clsStatistic.eOutput
	Dim LastDetailTR As clsTimeRange
	Dim pRisultato As clsRisultatoGroupBy
	Dim pFiltroGroupBy As clsFiltro


	Public Enum eOperazioni
		eSeleziona = 0
		eElimina = 1
		eMuoviSu = 2
		eMuoviGiu = 3
		eToggleAsseYunificato = 4
		eToggleTackMode = 5
	End Enum

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

	Public ReadOnly Property ValoreAssoluto As Boolean
		Get
			Return pTackOutput = clsStatistic.eOutput.eAbsolute
		End Get
	End Property

	Public Property TackOutput As clsStatistic.eOutput
		Get
			Return pTackOutput
		End Get
		Set(value As clsStatistic.eOutput)
			pTackOutput = value
		End Set
	End Property

	Public Property AsseYunificato As Boolean
		Get
			Return pAsseYunificato
		End Get
		Set(value As Boolean)
			pAsseYunificato = value
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

	Public Property CanaliOrdinata As List(Of clsChannel)
		Get
			Return pCanaliOrdinata
		End Get
		Set(value As List(Of clsChannel))
			pCanaliOrdinata = value
		End Set
	End Property

	Public Property RubberMode As eRubberMode
		Get
			Return pRubberMode
		End Get
		Set(value As eRubberMode)
			pRubberMode = value
		End Set
	End Property

	Public ReadOnly Property VisibleRange As clsTimeRange
		Get
			Dim inizio As DateTime = DirectCast(pObjSMACVM.SharedXVisibleRange.Min, DateTime)
			Dim fine As DateTime = DirectCast(pObjSMACVM.SharedXVisibleRange.Max, DateTime)
			Return New clsTimeRange(inizio, fine)
		End Get
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


	Public ReadOnly Property IsGraficoTimePlot As Boolean
		Get
			If pCanaleAscissa Is Nothing Then Return True
			If pCanaleAscissa.IdCanale = -1 Then Return True
			If pCanaleAscissa.DataType = clsChannel.eDataType.eTimeOnly Then Return True
			If pCanaleAscissa.DataType = clsChannel.eDataType.eDateTime Then Return True
			If pCanaleAscissa.DataType = clsChannel.eDataType.eDateOnly Then Return True
			Return False
		End Get
	End Property

	Public Sub New(CanaleAscissa As clsChannel, CanaliOridinata As List(Of clsChannel))

		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.
		pCanaleAscissa = CanaleAscissa
		pCanaliOrdinata = CanaliOridinata

		ScMouseWheelZoomModifier.IsEnabled = False
		pMouseWheelZoomMode = eMouseWheelZoomMode.eDisabled
		pRubberMode = eRubberMode.eXonly

		btn_ToggleYaxes.IsEnabled = pCanaliOrdinata.Count > 1

		'abilitaTackOutputMode()
		If IsGraficoTimePlot Then
			btn_180mode.Content = AggiornaInfosToggleAbsolute()
		Else
			btn_180mode.Content = AggiornaInfosToggleNormalAbsoluteTack()
		End If

		ScRubberBandXyZoomModifier.IsEnabled = True
		ScRubberBandXyZoomModifier.IsXAxisOnly = True
		ScZoomPanModifier.IsEnabled = False

		pObjSciChart = New clsSciChart(SCsurface)


	End Sub

	'Private Sub abilitaTackOutputMode()
	'	For Each canale As clsChannel In pCanaliOrdinata
	'		If Not canale Is Nothing Then
	'			If canale.DataType = clsChannel.eDataType.e180 Then
	'				btn_180mode.IsEnabled = True
	'				pTackOutput = clsStatistic.eOutput.eAbsolute
	'				Exit Sub
	'			End If
	'		End If
	'	Next
	'	btn_180mode.IsEnabled = False
	'	pTackOutput = clsStatistic.eOutput.eNormal
	'End Sub


	Public Event MouseUppato(sender As ctrl_DetailSciChart)

	Public Event MouseDoppioClick(sender As ctrl_DetailSciChart)

	Public Event BottoneSchiacciato(Operazione As eOperazioni, sender As ctrl_DetailSciChart)

	Private Sub SCsurface_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseUp
		RaiseEvent MouseUppato(Me)
	End Sub


	Private Sub Splittami_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles SplitGrafici.DragCompleted
		Dim Delta As Double = e.VerticalChange
		Dim H As Double = SCsurface.Height
		If Double.IsNaN(GrigliaGraficoDettaglio.Height) Then
			GrigliaGraficoDettaglio.Height = GrigliaGraficoDettaglio.ActualHeight
		End If
		GrigliaGraficoDettaglio.Height += Delta

		'SCsurface.Height += Delta
	End Sub

	Private Sub btn_Del_Click(sender As Object, e As RoutedEventArgs) Handles btn_Del.Click
		RaiseEvent BottoneSchiacciato(eOperazioni.eElimina, Me)
	End Sub

	Private Sub btn_Select_Click(sender As Object, e As RoutedEventArgs) Handles btn_Select.Click
		RaiseEvent BottoneSchiacciato(eOperazioni.eSeleziona, Me)
	End Sub

	Private Sub btn_MoveDn_Click(sender As Object, e As RoutedEventArgs) Handles btn_MoveDn.Click
		RaiseEvent BottoneSchiacciato(eOperazioni.eMuoviGiu, Me)
	End Sub

	Private Sub btn_MoveUp_Click(sender As Object, e As RoutedEventArgs) Handles btn_MoveUp.Click
		RaiseEvent BottoneSchiacciato(eOperazioni.eMuoviSu, Me)
	End Sub

	Public Sub ImpostaAltezza(Altezza As Integer)
		GrigliaGraficoDettaglio.Height = Altezza
	End Sub

	Private Sub btn_ToggleYaxes_Click(sender As Object, e As RoutedEventArgs) Handles btn_ToggleYaxes.Click
		RaiseEvent BottoneSchiacciato(eOperazioni.eToggleAsseYunificato, Me)
	End Sub

	Public Sub DrawTimePlot(CanaleAscissa As clsChannel, CanaliOrdinata As List(Of clsChannel), Intervallo As clsTimeRange, objSMACVM As clsSynchronizeMouseAcrossChartsViewModel, AsseYunificato As Boolean, AggiornaVisibleRangeX As Boolean)
		'pTackOutput = clsStatistic.eOutput.eNormal
		'For Each canale As clsChannel In pCanaliOrdinata
		'	If canale.DataType = clsChannel.eDataType.e180 Then
		'		pTackOutput = clsStatistic.eOutput.eAbsolute
		'		Exit For
		'	End If
		'Next
		DrawTimePlot(CanaleAscissa, CanaliOrdinata, Intervallo, objSMACVM, AsseYunificato, AggiornaVisibleRangeX, ValoreAssoluto)
	End Sub

  Public Sub DrawTimePlot(rCanaleAscissa As clsChannel, rCanaliOrdinata As List(Of clsChannel), Intervallo As clsTimeRange, objSMACVM As clsSynchronizeMouseAcrossChartsViewModel, AsseYunificato As Boolean, AggiornaVisibleRangeX As Boolean, UsaValoreAssoluto As Boolean)
    CanaleAscissa = rCanaleAscissa
    CanaliOrdinata = rCanaliOrdinata
    'ImpostaDisplaysValori()
    pObjSMACVM = objSMACVM
    pIntervalli = New List(Of clsTimeRange)
    pIntervalli.Add(Intervallo)
    pAsseYunificato = AsseYunificato
    'If UsaValoreAssoluto Then
    '	pTackOutput = clsStatistic.eOutput.eAbsolute
    'Else
    '	pTackOutput = clsStatistic.eOutput.eNormal
    'End If
    'For Each canale As clsChannel In pCanaliOrdinata
    '	If canale.CanaleChiave = clsChannels.eChannels.eTWA Then
    '		pObjSciChart.DrawTimePlotForceAbsolute(pCanaleAscissa, pCanaliOrdinata, Intervallo, objSMACVM)
    '		Exit Sub
    '	End If
    'Next
    btn_ToggleYaxes.IsEnabled = pCanaliOrdinata.Count > 1
    pObjSciChart.DrawTimePlot(pCanaleAscissa, pCanaliOrdinata, pIntervalli, pObjSMACVM, AsseYunificato, AggiornaVisibleRangeX, UsaValoreAssoluto)
  End Sub

  Public Sub DrawTimePlotToggleAsseYunificato()
		pAsseYunificato = Not pAsseYunificato
		btn_ToggleYaxes.IsEnabled = pCanaliOrdinata.Count > 1
		pObjSciChart.DrawTimePlot(pCanaleAscissa, pCanaliOrdinata, pIntervalli, pObjSMACVM, pAsseYunificato, False, ValoreAssoluto)
	End Sub

	Public Sub DrawConsistencyCloudAsseYunificato()
		pAsseYunificato = Not pAsseYunificato
		DrawConsistencyCloud(pRisultato, pFiltroGroupBy)
	End Sub

	Public Sub DrawConsistencyCloud(Risultato As clsRisultatoGroupBy, FiltroGroupBy As clsFiltro)
		pRisultato = Risultato
		pFiltroGroupBy = FiltroGroupBy
		btn_ToggleYaxes.IsEnabled = pCanaliOrdinata.Count > 1
		pObjSciChart.DrawConsistencyCloud(Risultato, FiltroGroupBy, pAsseYunificato)
    'ImpostaDisplaysValori()
  End Sub

	'Public Sub DrawConsistencyCandleStick(Risultato As clsRisultatoGroupBy, FiltroGroupBy As clsFiltro)
	'	pObjSciChart.DrawConsistencyCandleStick(Risultato, FiltroGroupBy)
	'	'pObjSciChart.DrawConsistencyCloud(Risultato, FiltroGroupBy)
	'	ImpostaDisplaysValori()
	'End Sub

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

	Private Sub Btn_Basic180mode_Click(sender As Object, e As RoutedEventArgs) Handles btn_180mode.Click
		CambiaModo180()
	End Sub

	Private Sub CambiaModo180()
		If IsGraficoTimePlot Then
			btn_180mode.Content = ToggleAbsolute()
		Else
			For Each canale As clsChannel In pCanaliOrdinata
				If canale.DataType = clsChannel.eDataType.e180 Then
					btn_180mode.Content = ToggleNormalAbsoluteTack()
					Exit Sub
				End If
			Next
		End If
	End Sub

	Private Function ToggleNormalAbsoluteTack() As String
		Dim strTmp As String = "X"
		Select Case pTackOutput
			Case clsStatistic.eOutput.eNormal
				pTackOutput = clsStatistic.eOutput.eAbsolute
			Case clsStatistic.eOutput.eAbsolute
				pTackOutput = clsStatistic.eOutput.eZeroAndPositives
			Case clsStatistic.eOutput.eZeroAndPositives
				pTackOutput = clsStatistic.eOutput.eNegatives
			Case clsStatistic.eOutput.eNegatives
				pTackOutput = clsStatistic.eOutput.eNormal
		End Select
		RaiseEvent BottoneSchiacciato(eOperazioni.eToggleTackMode, Me)
		Return AggiornaInfosToggleNormalAbsoluteTack()
	End Function

	Private Function AggiornaInfosToggleNormalAbsoluteTack() As String
		Dim strTmp As String = "X"
		Select Case pTackOutput
			Case clsStatistic.eOutput.eAbsolute
				strTmp = "Abs"
				btn_180mode.ToolTip = "Tack Mode Absolute, clik to Swap to Stbd Only"
			Case clsStatistic.eOutput.eZeroAndPositives
				strTmp = "Stb"
				btn_180mode.ToolTip = "Tack Mode Stbd Only, clik to Swap to Port Only"
			Case clsStatistic.eOutput.eNegatives
				strTmp = "Prt"
				btn_180mode.ToolTip = "Tack Mode Port Only, clik to Swap to Normal"
			Case clsStatistic.eOutput.eNormal
				strTmp = "Nrm"
				btn_180mode.ToolTip = "Tack Mode Normal, clik to Swap to Absolute"
		End Select
		Return strTmp
	End Function

	Private Function ToggleAbsolute() As String
		Dim strTmp As String = "X"
		Select Case pTackOutput
			Case clsStatistic.eOutput.eNormal
				pTackOutput = clsStatistic.eOutput.eAbsolute
				strTmp = "Abs"
				btn_180mode.ToolTip = "Tack Mode Absolute, clik to Swap to Normal"
			Case clsStatistic.eOutput.eAbsolute
				pTackOutput = clsStatistic.eOutput.eNormal
				strTmp = "Nrm"
				btn_180mode.ToolTip = "Tack Mode Normal, clik to Swap to Absolute"
		End Select
		RaiseEvent BottoneSchiacciato(eOperazioni.eToggleTackMode, Me)
		Return AggiornaInfosToggleAbsolute()
	End Function

	Private Function AggiornaInfosToggleAbsolute() As String
		Dim strTmp As String = "X"
		Select Case pTackOutput
			Case clsStatistic.eOutput.eAbsolute
				strTmp = "Abs"
				btn_180mode.ToolTip = "Tack Mode Absolute, clik to Swap to Normal"
			Case clsStatistic.eOutput.eNormal
				strTmp = "Nrm"
				btn_180mode.ToolTip = "Tack Mode Normal, clik to Swap to Absolute"
		End Select
		Return strTmp
	End Function

	'Public Sub AggiornaValoriDatiVisualizzati(Intervallo As clsTimeRange)
	'	txt_dati.Text = ""
	'	'If Not IsGraficoTimePlot Then Exit Sub

	'	Dim IndiceIniziale, IndiceFinale As Long
	'	IndiciIntervallo(Intervallo, IndiceIniziale, IndiceFinale)

	'	For Each canale As clsChannel In pCanaliOrdinata
	'		If Not canale Is Nothing Then
	'			txt_dati.Text &= canale.ShortName.ToUpper & vbCrLf
	'			Dim Mvalori = canale.Values.GetRange(IndiceIniziale, IndiceFinale - IndiceIniziale)
	'			txt_dati.Text &= "Max: " & Format(Mvalori.Max.Value, "F1") & vbCrLf
	'			txt_dati.Text &= "Avg: " & Format(Mvalori.Average.Value, "F1") & vbCrLf
	'			txt_dati.Text &= "Min: " & Format(Mvalori.Min.Value, "F1") & vbCrLf
	'		End If
	'	Next


	'End Sub

	Public Sub AggiornaDisplayValori()
		AggiornaDisplayValori(VisibleRange)
	End Sub

	Public Sub AggiornaDisplayValori(Intervallo As clsTimeRange)
		Dim IndiceIniziale, IndiceFinale As Long
		IndiciIntervallo(Intervallo, IndiceIniziale, IndiceFinale)
		For Each Canale As clsChannel In pCanaliOrdinata
			If Not Canale Is Nothing Then
				Canale.AggiornaSubSetTimeRange(IndiceIniziale, IndiceFinale, False)
			End If
		Next
	End Sub

  'Private Sub ImpostaDisplaysValori()

  '  'Dim syn As New clsChartSyncManager
  '  'syn.DataBoxHeight = 100

  '  ' Deve passare da qui ogni volta che c'è la possibilità che siano cambiati i canali in ordinata
  '  StackDisplays.Children.Clear()
  '  For Each Canale As clsChannel In pCanaliOrdinata
  '    If Not Canale Is Nothing Then
  '      StackDisplays.Children.Add(New ctrl_DataDisplay(Canale)) ', syn))
  '    End If
  '  Next
  'End Sub

  Private Sub pObjSMACVM_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pObjSMACVM.PropertyChanged
		Select Case e.PropertyName
			Case "SharedXVisibleRange"
				If LastDetailTR Is Nothing Then
					LastDetailTR = VisibleRange
					AggiornaDisplayValori()
				ElseIf VisibleRange.HasSameRange(LastDetailTR) Then

				Else
					LastDetailTR = VisibleRange
					AggiornaDisplayValori()
				End If
		End Select
	End Sub

  Private Sub SCsurface_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles SCsurface.MouseDoubleClick
    RaiseEvent MouseDoppioClick(Me)
  End Sub




End Class
