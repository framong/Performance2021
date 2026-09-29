Imports System.ComponentModel
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Data.Model

Public Class clsGroupByPlotViewModel
	Implements INotifyPropertyChanged

	Dim pObjChartSyncManager As clsChartSyncManager
	Dim pSeriesSource As XyDataSeries(Of DateTime, Double)

	Dim pTestaDiCatreca As String

	Public Property ObjChartSyncManager As clsChartSyncManager
		Get
			Return pObjChartSyncManager
		End Get
		Set(value As clsChartSyncManager)
			pObjChartSyncManager = value
		End Set
	End Property

	Public Property SeriesSource As XyDataSeries(Of DateTime, Double)
		Get
			Return pSeriesSource
		End Get
		Set(value As XyDataSeries(Of DateTime, Double))
			If pSeriesSource Is value Then Exit Property
			pSeriesSource = value
			OnPropertyChanged("SeriesSource")
		End Set
	End Property

	Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

	Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
		RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
	End Sub

	Public Property TestaDiCatreca As String
		Get
			Return pTestaDiCatreca
		End Get
		Set(value As String)
			If pTestaDiCatreca = value Then Exit Property
			pTestaDiCatreca = value
			OnPropertyChanged("TestaDiCatreca")
		End Set
	End Property



End Class

