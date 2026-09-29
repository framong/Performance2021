Imports System.ComponentModel
Imports PropertyChanged
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class UserControlNumericUpDown
  'Implements INotifyPropertyChanged
  Dim pDecimals As Integer = 0

  Public Sub New()
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    txtValore.Text = 10.0
  End Sub

  Public Property Decimals As Integer
    Get
      Return pDecimals
    End Get
    Set(value As Integer)
      pDecimals = value
    End Set
  End Property

  Public Property Value As Double
    Get
      Return CDbl(txtValore.Text)
    End Get
    Set(value As Double)
      txtValore.Text = Format(value, "F" & pDecimals.ToString)
      'OnPropertyChanged("Value")
    End Set
  End Property

  Private Sub ButtonUp_Click(sender As Object, e As RoutedEventArgs)
    Value = CDbl(txtValore.Text) + (10 ^ -(pDecimals - 1)) / 2
  End Sub

  Private Sub ButtonDn_Click(sender As Object, e As RoutedEventArgs)
    Value = CDbl(txtValore.Text) - (10 ^ -(pDecimals - 1)) / 2
  End Sub

  Private Sub txtValore_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
    If Not IsNumeric(e.Text) Then
      e.Handled = True
    End If
  End Sub

  Private Function IsNumeric(testo As String) As Boolean
    Dim objRegExp As New System.Text.RegularExpressions.Regex("^\d+$") 'digits only
    Dim match As System.Text.RegularExpressions.Match = objRegExp.Match(testo)
    If match.Success Then
      Return True
    End If
    Return False
  End Function

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

End Class
