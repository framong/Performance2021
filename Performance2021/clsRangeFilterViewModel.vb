Imports System.ComponentModel
Imports PropertyChanged

<AddINotifyPropertyChangedInterface>
Public Class clsRangeFilterViewModel
  'Implements INotifyPropertyChanged
  Public Property ApplyIsChecked As Boolean
  Dim ApplyCheckedAndNotCustom As Boolean
  Public Property CustomValueEnabled As Boolean
  Public Property IsCustomValue As Boolean = False

  'Public Property ApplyIsChecked As Boolean
  '  Get
  '    Return pApplyIsChecked
  '  End Get
  '  Set(value As Boolean)
  '    pApplyIsChecked = value
  '    OnPropertyChanged("ApplyIsChecked")
  '    CustomValueEnabled = pApplyIsChecked And pIsCustomValue
  '  End Set
  'End Property

  'Public Property CustomValueEnabled As Boolean
  '  Get
  '    Return pCustomValueEnabled
  '  End Get
  '  Set(value As Boolean)
  '    pCustomValueEnabled = value
  '    OnPropertyChanged("CustomValueEnabled")
  '  End Set
  'End Property

  'Public Property IsCustomValue As Boolean
  '  Get
  '    Return pIsCustomValue
  '  End Get
  '  Set(value As Boolean)
  '    pIsCustomValue = value
  '    CustomValueEnabled = ApplyIsChecked And IsCustomValue
  '  End Set
  'End Property


  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

End Class
