Imports System.ComponentModel
Imports PropertyChanged

<AddINotifyPropertyChangedInterface>
Public Class UserControlPortStbd
  'Implements INotifyPropertyChanged
  Dim EventiAbilitati As Boolean = False

  Public Property CheckStatusChanged As clsSailingState.eTack
  '	Get
  '		Return GetTack
  '	End Get
  '	Set(value As clsSailingState.eTack)
  '		OnPropertyChanged("CheckStatusChanged")
  '	End Set
  'End Property

  Public Property StbdText As String
  '	Get
  '		Return lblStbd.Content
  '	End Get
  '	Set(value As String)
  '		lblStbd.Content = value
  '		OnPropertyChanged("StbdText")
  '	End Set
  'End Property

  Public Property StbdToolTip As String
  '	Get
  '		Return Stbd.ToolTip
  '	End Get
  '	Set(value As String)
  '		Stbd.ToolTip = value
  '		OnPropertyChanged("StbdToolTip")
  '	End Set
  'End Property

  Public Property PortText As String
  '	Get
  '		Return lblPort.Content
  '	End Get
  '	Set(value As String)
  '		lblPort.Content = value
  '		OnPropertyChanged("PortText")
  '	End Set
  'End Property

  Public Property PortToolTip As String
  '	Get
  '		Return Port.ToolTip
  '	End Get
  '	Set(value As String)
  '		Port.ToolTip = value
  '		OnPropertyChanged("PortToolTip")
  '	End Set
  'End Property

  Public Property SailingState As clsSailingState.eTack
  '	Get
  '		Return GetTack
  '	End Get
  '	Set(value As clsSailingState.eTack)
  '		SetTack(value)
  '	End Set
  'End Property

  Public ReadOnly Property GetTack As clsSailingState.eTack
    Get
      If Port.IsChecked Then
        If Stbd.IsChecked Then
          Return clsSailingState.eTack.eBoth
        Else
          Return clsSailingState.eTack.ePort
        End If
      ElseIf Stbd.IsChecked Then
        Return clsSailingState.eTack.eStbd
      Else
        Return clsSailingState.eTack.eNone
      End If
    End Get
  End Property

  Public Sub SetTack(Tack As clsSailingState.eTack)
    Port.IsChecked = (Tack = clsSailingState.eTack.eBoth OrElse Tack = clsSailingState.eTack.ePort)
    Stbd.IsChecked = (Tack = clsSailingState.eTack.eBoth OrElse Tack = clsSailingState.eTack.eStbd)
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '	If EventiAbilitati Then RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Private Sub Stbd_Checked(sender As Object, e As RoutedEventArgs) Handles Stbd.Checked
    CheckStatusChanged = clsSailingState.eTack.eStbd
  End Sub

  Private Sub Port_Checked(sender As Object, e As RoutedEventArgs) Handles Port.Checked
    CheckStatusChanged = clsSailingState.eTack.ePort
  End Sub

  Private Sub Port_Unchecked(sender As Object, e As RoutedEventArgs) Handles Port.Unchecked
    CheckStatusChanged = clsSailingState.eTack.ePort
  End Sub

  Private Sub Stbd_Unchecked(sender As Object, e As RoutedEventArgs) Handles Stbd.Unchecked
    CheckStatusChanged = clsSailingState.eTack.eStbd
  End Sub

  Private Sub UserControlPortStbd_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
    EventiAbilitati = True
  End Sub

  Public ReadOnly Property IsSelected(CurrentTack As clsSailingState.eTack) As Boolean
    Get
      Dim Selezione As clsSailingState.eTack = GetTack
      Return CurrentTack = Selezione OrElse CurrentTack = clsSailingState.eTack.eBoth
    End Get
  End Property

  Public ReadOnly Property IsSelected(IsStbd As Boolean) As Boolean
    Get
      Dim Selezione As clsSailingState.eTack = GetTack

      If Selezione = clsSailingState.eTack.eBoth Then Return True
      If IsStbd And Selezione = clsSailingState.eTack.eStbd Then Return True
      If Not IsStbd And Selezione = clsSailingState.eTack.ePort Then Return True
      Return False
    End Get
  End Property
End Class
