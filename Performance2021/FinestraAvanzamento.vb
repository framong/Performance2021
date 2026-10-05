Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Threading

''' <summary>
''' Finestra di avanzamento non modale per i report: la creazione gira sul thread UI (le letture Parquet e l'esportazione dei
''' grafici non si possono spostare), quindi dopo ogni passo si cede il controllo alla UI per ridisegnare la barra e leggere Cancel.
''' </summary>
Public Class FinestraAvanzamento
  Inherits Window

  Private ReadOnly Testo As New TextBlock With {.Margin = New Thickness(0, 0, 0, 8), .TextWrapping = TextWrapping.Wrap}
  Private ReadOnly Barra As New ProgressBar With {.Minimum = 0, .Maximum = 100, .Height = 18}
  Private ReadOnly Tempo As New TextBlock With {.Margin = New Thickness(0, 6, 0, 0), .Foreground = System.Windows.Media.Brushes.Gray}
  Private ReadOnly Avvio As DateTime = Now
  Private UltimoAggiornamento As DateTime = DateTime.MinValue
  Private FaseDa As Double = 0
  Private FaseA As Double = 100
  Private FaseTitolo As String = ""

  ''' <summary>True se l'utente ha premuto Cancel o chiuso la finestra.</summary>
  Public Property Annullato As Boolean = False

  Public Sub New(Titolo As String)
    Me.Title = Titolo
    Width = 420
    SizeToContent = SizeToContent.Height
    ResizeMode = ResizeMode.NoResize
    WindowStartupLocation = WindowStartupLocation.CenterOwner
    If Not System.Windows.Application.Current Is Nothing AndAlso Not System.Windows.Application.Current.MainWindow Is Nothing AndAlso System.Windows.Application.Current.MainWindow.IsLoaded Then Owner = System.Windows.Application.Current.MainWindow

    Dim Pannello As New StackPanel With {.Margin = New Thickness(14)}
    Pannello.Children.Add(Testo)
    Pannello.Children.Add(Barra)
    Pannello.Children.Add(Tempo)
    Dim Annulla As New Button With {.Content = "Cancel", .Width = 80, .HorizontalAlignment = HorizontalAlignment.Right, .Margin = New Thickness(0, 10, 0, 0)}
    AddHandler Annulla.Click, Sub()
                                Annullato = True
                                Testo.Text = "Canceling..."
                              End Sub
    Pannello.Children.Add(Annulla)
    Content = Pannello
    AddHandler Closing, Sub() Annullato = True
  End Sub

  ''' <summary>Inizia una fase che occupa la parte di barra tra Da e A (percentuali) e ne mostra il titolo.</summary>
  Public Sub Fase(Titolo As String, Da As Double, A As Double)
    FaseTitolo = Titolo
    FaseDa = Da
    FaseA = A
    Passo(0, 0, "")
  End Sub

  ''' <summary>Avanzamento dentro la fase: i di n, con un dettaglio facoltativo (per esempio il nome del canale).</summary>
  Public Sub Passo(i As Integer, n As Integer, Dettaglio As String)
    Dim Frazione As Double = If(n <= 0, 0, Math.Min(1, Math.Max(0, i / n)))
    Barra.Value = FaseDa + (FaseA - FaseDa) * Frazione
    Testo.Text = FaseTitolo & If(n > 0, " (" & i & " / " & n & ")", "") & If(Dettaglio = "", "", vbCrLf & Dettaglio)
    Tempo.Text = "Elapsed: " & (Now - Avvio).ToString("mm\:ss")
    ' cede la UI al massimo ogni 100 ms: ridisegna la barra e gestisce il clic su Cancel
    If (Now - UltimoAggiornamento).TotalMilliseconds >= 100 Then
      UltimoAggiornamento = Now
      Dispatcher.Invoke(Sub()
                        End Sub, DispatcherPriority.Background)
    End If
    If Annullato Then Throw New OperationCanceledException
  End Sub

End Class
