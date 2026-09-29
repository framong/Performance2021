Imports System.ComponentModel

Public Class clsDenisBinding
  Implements INotifyPropertyChanged

  Dim pZoomMode As SciChart.Charting.XyDirection
  Dim pItemHeight As Double

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub


  Public Property ZoomMode As SciChart.Charting.XyDirection
    Get
      Return pZoomMode
    End Get
    Set(value As SciChart.Charting.XyDirection)
      If pZoomMode = value Then Exit Property
      pZoomMode = value
      OnPropertyChanged("ZoomMode")
    End Set
  End Property

  Public Property ItemHeight As Double
    Get
      Return pItemHeight
    End Get
    Set(value As Double)
      If pItemHeight = value Then Exit Property
      pItemHeight = value
      OnPropertyChanged("ItemHeight")
    End Set
  End Property




End Class
