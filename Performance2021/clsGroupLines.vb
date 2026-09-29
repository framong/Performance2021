Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.RenderableSeries

Public Class clsGroupLines
  Dim _Canale As clsChannel2020

  Dim _RawValue As IChartSeriesViewModel
  Dim _RawValueDeriv As IChartSeriesViewModel
  Dim _Signed As IChartSeriesViewModel
  Dim _SignedDeriv As IChartSeriesViewModel
  Dim _WwdLwd As IChartSeriesViewModel
  Dim _WwdLwdDeriv As IChartSeriesViewModel

  Dim _Tgt As IChartSeriesViewModel

  Public Sub New(Canale As clsChannel2020)
    _Canale = Canale
  End Sub

  Public Enum eLineType
    eRawValue = 0
    eRawValueDeriv = 1
    eDataTypeSigned = 2
    eDataTypeSignedDeriv = 3
    eDataTypeSignedAndWwdLwd = 4
    eDataTypeSignedAndWwdLwdDeriv = 5
  End Enum

  'Public Property Calculated As List(Of ChartSeriesViewModel)
  '  Get
  '    Return _Calculated
  '  End Get
  '  Set(value As List(Of ChartSeriesViewModel))
  '    _Calculated = value
  '  End Set
  'End Property

  Public ReadOnly Property IsLoaded(LineType As eLineType) As Boolean
    Get
      Select Case LineType
        Case eLineType.eRawValueDeriv
          Return Not _RawValueDeriv Is Nothing
        Case eLineType.eDataTypeSigned
          Return Not _Signed Is Nothing
        Case eLineType.eRawValueDeriv
          Return Not _RawValueDeriv Is Nothing
        Case eLineType.eDataTypeSignedAndWwdLwd
          Return Not _WwdLwd Is Nothing
        Case eLineType.eDataTypeSignedAndWwdLwdDeriv
          Return Not _WwdLwdDeriv Is Nothing
        Case Else
          Return Not _RawValue Is Nothing
      End Select
    End Get
  End Property

  Public Property RawValue As IChartSeriesViewModel
    Get
      Return _RawValue
    End Get
    Set(value As IChartSeriesViewModel)
      _RawValue = value
    End Set
  End Property
  Public Property RawValueDeriv As IChartSeriesViewModel
    Get
      Return _RawValueDeriv
    End Get
    Set(value As IChartSeriesViewModel)
      _RawValueDeriv = value
    End Set
  End Property

  Public Property Signed As IChartSeriesViewModel
    Get
      Return _Signed
    End Get
    Set(value As IChartSeriesViewModel)
      _Signed = value
    End Set
  End Property

  Public Property SignedDeriv As IChartSeriesViewModel
    Get
      Return _SignedDeriv
    End Get
    Set(value As IChartSeriesViewModel)
      _SignedDeriv = value
    End Set
  End Property

  Public Property WwdLwd As IChartSeriesViewModel
    Get
      Return _WwdLwd
    End Get
    Set(value As IChartSeriesViewModel)
      _WwdLwd = value
    End Set
  End Property

  Public Property WwdLwdDeriv As IChartSeriesViewModel
    Get
      Return _WwdLwdDeriv
    End Get
    Set(value As IChartSeriesViewModel)
      _WwdLwdDeriv = value
    End Set
  End Property

  Public Property Canale As clsChannel2020
    Get
      Return _Canale
    End Get
    Set(value As clsChannel2020)
      _Canale = value
    End Set
  End Property

  Public Property Tgt As IChartSeriesViewModel
    Get
      Return _Tgt
    End Get
    Set(value As IChartSeriesViewModel)
      _Tgt = value
    End Set
  End Property


End Class

'Public Class clsGroupLinesGroupBy

'  Dim pBasic As IChartSeriesViewModel
'  Dim pBasicPort As IChartSeriesViewModel
'  Dim pBasicStbd As IChartSeriesViewModel
'  Dim pBasicStackedPort As IChartSeriesViewModel
'  Dim pBasicStackedStbd As IChartSeriesViewModel

'  Dim pNormal As IChartSeriesViewModel
'  Dim pAbsolute As IChartSeriesViewModel
'  Dim pPort As IChartSeriesViewModel
'  Dim pStbd As IChartSeriesViewModel

'  Dim pNormalCloud As IChartSeriesViewModel
'  Dim pAbsoluteCloud As IChartSeriesViewModel
'  Dim pPortCloud As IChartSeriesViewModel
'  Dim pStbdClod As IChartSeriesViewModel

'  Public Property Basic As IChartSeriesViewModel
'    Get
'      Return pBasic
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pBasic = value
'    End Set
'  End Property

'  Public Property BasicPort As IChartSeriesViewModel
'    Get
'      Return pBasicPort
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pBasicPort = value
'    End Set
'  End Property

'  Public Property BasicStbd As IChartSeriesViewModel
'    Get
'      Return pBasicStbd
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pBasicStbd = value
'    End Set
'  End Property

'  Public Property BasicStackedPort As IChartSeriesViewModel
'    Get
'      Return pBasicStackedPort
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pBasicStackedPort = value
'    End Set
'  End Property

'  Public Property BasicStackedStbd As IChartSeriesViewModel
'    Get
'      Return pBasicStackedStbd
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pBasicStackedStbd = value
'    End Set
'  End Property

'  Public Property Normal As IChartSeriesViewModel
'    Get
'      Return pNormal
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pNormal = value
'    End Set
'  End Property

'  Public Property Absolute As IChartSeriesViewModel
'    Get
'      Return pAbsolute
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pAbsolute = value
'    End Set
'  End Property

'  Public Property Port As IChartSeriesViewModel
'    Get
'      Return pPort
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pPort = value
'    End Set
'  End Property

'  Public Property Stbd As IChartSeriesViewModel
'    Get
'      Return pStbd
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pStbd = value
'    End Set
'  End Property

'  Public Property NormalCloud As IChartSeriesViewModel
'    Get
'      Return pNormalCloud
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pNormalCloud = value
'    End Set
'  End Property

'  Public Property AbsoluteCloud As IChartSeriesViewModel
'    Get
'      Return pAbsoluteCloud
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pAbsoluteCloud = value
'    End Set
'  End Property

'  Public Property PortCloud As IChartSeriesViewModel
'    Get
'      Return pPortCloud
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pPortCloud = value
'    End Set
'  End Property

'  Public Property StbdCloud As IChartSeriesViewModel
'    Get
'      Return pStbdClod
'    End Get
'    Set(value As IChartSeriesViewModel)
'      pStbdClod = value
'    End Set
'  End Property

'End Class


'Public Class clsSerie_LineaAndCloud

'  Dim pDatiNuvola As XyDataSeries(Of Double, Double)
'  Dim pNuvola As XyScatterRenderableSeries
'  Dim pDatiLinea As XyDataSeries(Of Double, Double)
'  Dim pLinea As FastLineRenderableSeries

'  Public Sub New()
'    pDatiNuvola = New XyDataSeries(Of Double, Double)
'    pNuvola = New XyScatterRenderableSeries

'    pDatiLinea = New XyDataSeries(Of Double, Double)
'    pLinea = New FastLineRenderableSeries
'  End Sub

'  Public Property DatiNuvola As XyDataSeries(Of Double, Double)
'    Get
'      Return pDatiNuvola
'    End Get
'    Set(value As XyDataSeries(Of Double, Double))
'      pDatiNuvola = value
'    End Set
'  End Property

'  Public Property Nuvola As XyScatterRenderableSeries
'    Get
'      Return pNuvola
'    End Get
'    Set(value As XyScatterRenderableSeries)
'      pNuvola = value
'    End Set
'  End Property

'  Public Property DatiLinea As XyDataSeries(Of Double, Double)
'    Get
'      Return pDatiLinea
'    End Get
'    Set(value As XyDataSeries(Of Double, Double))
'      pDatiLinea = value
'    End Set
'  End Property

'  Public Property Linea As FastLineRenderableSeries
'    Get
'      Return pLinea
'    End Get
'    Set(value As FastLineRenderableSeries)
'      pLinea = value
'    End Set
'  End Property



'End Class

