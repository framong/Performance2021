Imports System.Text.RegularExpressions

''' <summary>
''' Immissione di numeri nelle TextBox, da usare con la proprieta' associata NumericInput.Kind:
'''   Decimal : numero con segno e parte decimale (accetta "." e ",", normalizzata in ".")
'''   Int     : intero senza segno
'''   Byte    : intero da 0 a 255
''' Durante la digitazione il testo resta quello scritto dall'utente (il punto compare subito, anche se non ci sono
''' ancora decimali); le sole correzioni sono "0" davanti a un punto iniziale. Alla fine (perdita del focus o Invio) si
''' completa con gli zeri mancanti: ".5" = "0.5", "5." = "5.0", "." = "0.0", vuoto = "0".
''' Il binding del testo deve aggiornare la sorgente alla perdita del focus (il valore predefinito): con
''' UpdateSourceTrigger=PropertyChanged il numero "5." diventa 5 e il punto sparisce prima di poter scrivere i decimali.
''' </summary>
Public Class NumericInput

  Public Shared ReadOnly KindProperty As DependencyProperty =
    DependencyProperty.RegisterAttached("Kind", GetType(String), GetType(NumericInput), New PropertyMetadata(Nothing, AddressOf KindChanged))

  Public Shared Function GetKind(d As DependencyObject) As String
    Return CStr(d.GetValue(KindProperty))
  End Function

  Public Shared Sub SetKind(d As DependencyObject, value As String)
    d.SetValue(KindProperty, value)
  End Sub

  Private Shared Sub KindChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
    Dim tb As TextBox = TryCast(d, TextBox)
    If tb Is Nothing Then Exit Sub
    RemoveHandler tb.PreviewTextInput, AddressOf OnPreviewTextInput
    RemoveHandler tb.PreviewKeyDown, AddressOf OnPreviewKeyDown
    RemoveHandler tb.LostFocus, AddressOf OnLostFocus
    DataObject.RemovePastingHandler(tb, AddressOf OnPaste)
    If e.NewValue Is Nothing Then Exit Sub
    AddHandler tb.PreviewTextInput, AddressOf OnPreviewTextInput
    AddHandler tb.PreviewKeyDown, AddressOf OnPreviewKeyDown
    AddHandler tb.LostFocus, AddressOf OnLostFocus
    DataObject.AddPastingHandler(tb, AddressOf OnPaste)
  End Sub

  Private Shared Function Valido(Kind As String, Testo As String) As Boolean
    Select Case Kind
      Case "Decimal"
        Return Regex.IsMatch(Testo, "^-?\d*\.?\d*$")
      Case "Byte"
        If Not Regex.IsMatch(Testo, "^\d{0,3}$") Then Return False
        Return Testo = "" OrElse CInt(Testo) <= 255
      Case Else ' Int
        Return Regex.IsMatch(Testo, "^\d*$")
    End Select
  End Function

  ''' <summary>Aggiunge lo zero davanti a un punto iniziale (".5" = "0.5", "-.5" = "-0.5").</summary>
  Private Shared Function ZeroDavanti(Testo As String) As String
    If Testo.StartsWith("-.") Then Return "-0" & Testo.Substring(1)
    If Testo.StartsWith(".") Then Return "0" & Testo
    Return Testo
  End Function

  ''' <summary>Inserisce Inserito al posto della selezione corrente; False se il risultato non e' un numero valido.</summary>
  Private Shared Function Inserisci(tb As TextBox, Inserito As String) As Boolean
    Dim Kind As String = GetKind(tb)
    Dim Prima As String = tb.Text.Substring(0, tb.SelectionStart)
    Dim Dopo As String = tb.Text.Substring(tb.SelectionStart + tb.SelectionLength)
    Dim Candidato As String = Prima & Inserito & Dopo
    If Not Valido(Kind, Candidato) Then Return False
    Dim Pos As Integer = Prima.Length + Inserito.Length
    If Kind = "Decimal" Then
      Dim ConZero As String = ZeroDavanti(Candidato)
      Pos += ConZero.Length - Candidato.Length
      Candidato = ConZero
    End If
    tb.Text = Candidato
    tb.SelectionStart = Pos
    tb.SelectionLength = 0
    Return True
  End Function

  Private Shared Sub OnPreviewTextInput(sender As Object, e As TextCompositionEventArgs)
    Dim tb As TextBox = DirectCast(sender, TextBox)
    Inserisci(tb, e.Text.Replace(","c, "."c))
    e.Handled = True ' il testo lo inserisce sempre il codice sopra (con la virgola trasformata in punto)
  End Sub

  Private Shared Sub OnPaste(sender As Object, e As DataObjectPastingEventArgs)
    Dim tb As TextBox = DirectCast(sender, TextBox)
    If e.DataObject.GetDataPresent(GetType(String)) Then
      Inserisci(tb, CStr(e.DataObject.GetData(GetType(String))).Trim().Replace(","c, "."c))
    End If
    e.CancelCommand()
  End Sub

  Private Shared Sub OnPreviewKeyDown(sender As Object, e As KeyEventArgs)
    If e.Key = Key.Space Then
      e.Handled = True
    ElseIf e.Key = Key.Enter Then
      Completa(DirectCast(sender, TextBox))
    End If
  End Sub

  Private Shared Sub OnLostFocus(sender As Object, e As RoutedEventArgs)
    Completa(DirectCast(sender, TextBox))
  End Sub

  ''' <summary>Completa il testo con gli zeri mancanti e aggiorna la sorgente del binding.</summary>
  Public Shared Sub Completa(tb As TextBox)
    Dim t As String = tb.Text.Trim()
    Select Case GetKind(tb)
      Case "Decimal"
        If t = "" OrElse t = "-" Then
          t = "0"
        Else
          t = ZeroDavanti(t)
          If t.EndsWith(".") Then t &= "0"
        End If
      Case "Byte"
        If t = "" Then t = "0"
        If CInt(t) > 255 Then t = "255"
      Case Else
        If t = "" Then t = "0"
    End Select
    If tb.Text <> t Then tb.Text = t
    Dim Binding As BindingExpression = tb.GetBindingExpression(TextBox.TextProperty)
    If Not Binding Is Nothing Then Binding.UpdateSource()
  End Sub

End Class
