Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged

Public Class UserControlPolarSelect
  Dim VM As New clsPolariDisponibiliVM
  Dim pNomePolare As String = ""

  Public Sub New(CanaleAttuale As String)
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    For Each itm In cmb_Polari.Items
      Dim i = DirectCast(itm, String)
      If i = CanaleAttuale Then
        cmb_Polari.SelectedItem = itm
        Exit For
      End If
    Next

  End Sub

  Public Property NomePolare As String
    Get
      Return pNomePolare
    End Get
    Set(value As String)
      pNomePolare = value
    End Set
  End Property

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    Dim i = DirectCast(cmb_Polari.SelectedItem, String)
    If i Is Nothing Then
      Me.DialogResult = False
      pNomePolare = ""
    Else
      Me.DialogResult = True
      pNomePolare = i
    End If
    Me.Close()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    pNomePolare = ""
    Me.DialogResult = False
    Me.Close()
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    pNomePolare = ""
    Me.DialogResult = True
    Me.Close()
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsPolariDisponibiliVM
  'Implements INotifyPropertyChanged
  Public Property ListaPolariDisponibili As New List(Of String)
  Dim pCanalAttuale As String

  Public Sub New()
    If Not TgtManager.Tgt Is Nothing Then
      ListaPolariDisponibili = TgtManager.Tgt.PolariDisponibili
    End If
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  'Public Property ListaPolariDisponibili As List(Of String)
  '  Get
  '    Return pListaPolariDisponibili
  '  End Get
  '  Set(value As List(Of String))
  '    pListaPolariDisponibili = value
  '    OnPropertyChanged("ListaPolariDisponibili")
  '  End Set
  'End Property


End Class