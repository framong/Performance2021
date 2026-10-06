Imports System.ComponentModel
Imports System.Globalization

''' <summary>Restituisce una vista propria (ListCollectionView) della lista: cosi' ogni combo la filtra senza toccare le altre che usano la stessa sorgente.</summary>
Public Class clsVistaPropriaConverter
  Implements IValueConverter

  Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
    If value Is Nothing Then Return Nothing
    Dim Lista As IList = TryCast(value, IList)
    If Lista Is Nothing Then Return value
    Return New ListCollectionView(Lista)
  End Function

  Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
    Throw New NotSupportedException
  End Function
End Class

''' <summary>
''' Rende una ComboBox di canali editabile con ricerca: scrivendo, la lista si restringe ai canali il cui
''' LongName o ShortName contiene tutte le parole digitate (senza distinguere maiuscole/minuscole).
''' Richiede che ItemsSource sia una ICollectionView propria della combo (vedi clsVistaPropriaConverter).
''' </summary>
Public Class clsComboFiltro

  Public Shared Sub Attiva(cmb As ComboBox)
    cmb.IsEditable = True
    cmb.IsTextSearchEnabled = False
    cmb.AddHandler(Keyboard.KeyUpEvent, New KeyEventHandler(AddressOf Combo_KeyUp), True)
    AddHandler cmb.DropDownClosed, AddressOf Combo_DropDownClosed
  End Sub

  Private Shared Function Corrisponde(o As Object, Parole() As String) As Boolean
    Dim c As clsChannel2020 = TryCast(o, clsChannel2020)
    If c Is Nothing Then Return False
    Dim Nomi As String = (c.LongName & " " & c.ShortName).ToLowerInvariant
    Return Parole.All(Function(p) Nomi.Contains(p))
  End Function

  Private Shared Sub Combo_KeyUp(sender As Object, e As KeyEventArgs)
    Dim cmb As ComboBox = TryCast(sender, ComboBox)
    Dim Vista As ICollectionView = TryCast(cmb?.ItemsSource, ICollectionView)
    If Vista Is Nothing Then Exit Sub

    Select Case e.Key
      Case Key.Up, Key.Down, Key.Left, Key.Right, Key.Tab, Key.Home, Key.End, Key.PageUp, Key.PageDown,
           Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt
        Exit Sub
      Case Key.Escape
        Vista.Filter = Nothing
        cmb.IsDropDownOpen = False
        RipristinaTesto(cmb)
        Exit Sub
    End Select

    Dim Parole() As String = cmb.Text.ToLowerInvariant.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)

    If e.Key = Key.Enter Then
      Dim Primo As Object = Vista.Cast(Of Object).FirstOrDefault(Function(o) Corrisponde(o, Parole))
      cmb.IsDropDownOpen = False
      If Not Primo Is Nothing Then cmb.SelectedItem = Primo
      Exit Sub
    End If

    ' il canale selezionato resta sempre in lista, altrimenti la combo azzererebbe la selezione
    Dim Selezionato As Object = cmb.SelectedItem
    If Parole.Length = 0 Then
      Vista.Filter = Nothing
    Else
      Vista.Filter = Function(o) Object.ReferenceEquals(o, Selezionato) OrElse Corrisponde(o, Parole)
    End If
    cmb.IsDropDownOpen = True
  End Sub

  Private Shared Sub Combo_DropDownClosed(sender As Object, e As EventArgs)
    Dim cmb As ComboBox = TryCast(sender, ComboBox)
    Dim Vista As ICollectionView = TryCast(cmb?.ItemsSource, ICollectionView)
    If Vista Is Nothing Then Exit Sub
    Vista.Filter = Nothing
    RipristinaTesto(cmb)
  End Sub

  Private Shared Sub RipristinaTesto(cmb As ComboBox)
    Dim c As clsChannel2020 = TryCast(cmb.SelectedItem, clsChannel2020)
    If Not c Is Nothing Then cmb.Text = c.LongName
  End Sub

End Class
