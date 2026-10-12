Imports System.Globalization

''' <summary>
''' Finestra modale "Data Source and Filter": elenco dei filtri con nome del profilo e costruttore dell'albero di condizioni
''' (gruppi AND / OR, NOT, condizioni su un canale con valore assoluto, min e max), con conteggio live dei campioni che passano.
''' Si lavora su una copia: le modifiche entrano nel profilo solo con Save.
''' </summary>
Public Class FiltriForm

  Private Canali As New List(Of clsChannel2020)
  Private Corrente As clsFiltroNominato = Nothing
  Private Caricando As Boolean = False
  Private Sporco As Boolean = False

  ''' <summary>Nome del filtro selezionato quando la finestra si chiude, per chi la apre.</summary>
  Public Property FiltroScelto As String = ""

  Public Sub New(Optional NomeIniziale As String = "")
    InitializeComponent()

    If DataProvider2020 IsNot Nothing AndAlso DataProvider2020.Channels IsNot Nothing Then
      Canali = DataProvider2020.Channels.ListaCanali.OrderBy(Function(x) x.LongName).ToList
    End If

    RicaricaElenco(NomeIniziale)
  End Sub

  ' ---------------------------------------------------------------- elenco filtri

  Private Sub RicaricaElenco(Selezione As String)
    Caricando = True
    lst_Filtri.Items.Clear()
    For Each F As clsFiltroNominato In clsFiltri.Elenco().OrderBy(Function(x) x.Nome)
      lst_Filtri.Items.Add(F.Nome)
    Next
    Caricando = False
    If Not String.IsNullOrEmpty(Selezione) AndAlso lst_Filtri.Items.Contains(Selezione) Then
      lst_Filtri.SelectedItem = Selezione
    ElseIf lst_Filtri.Items.Count > 0 Then
      lst_Filtri.SelectedIndex = 0
    Else
      CaricaFiltro(Nothing)
    End If
  End Sub

  Private Function ConfermaScartoModifiche() As Boolean
    If Not Sporco Then Return True
    Return MsgBox("The current filter has unsaved changes. Discard them?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Filter") = MsgBoxResult.Yes
  End Function

  Private Sub lst_Filtri_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    If Caricando Then Exit Sub
    If lst_Filtri.SelectedItem Is Nothing Then Exit Sub
    If Not ConfermaScartoModifiche() Then
      Caricando = True
      If Corrente IsNot Nothing Then lst_Filtri.SelectedItem = Corrente.Nome
      Caricando = False
      Exit Sub
    End If
    Dim Orig As clsFiltroNominato = clsFiltri.Trova(CStr(lst_Filtri.SelectedItem))
    CaricaFiltro(If(Orig Is Nothing, Nothing, Orig.Clona()))
  End Sub

  Private Sub CaricaFiltro(F As clsFiltroNominato)
    Caricando = True
    Corrente = F
    Sporco = False
    txt_Nome.Text = If(F Is Nothing, "", F.Nome)
    txt_Note.Text = If(F Is Nothing, "", F.Note)
    txt_Nome.IsEnabled = F IsNot Nothing
    txt_Note.IsEnabled = F IsNot Nothing
    btn_Salva.IsEnabled = F IsNot Nothing
    btn_Duplica.IsEnabled = F IsNot Nothing
    btn_Elimina.IsEnabled = F IsNot Nothing
    Caricando = False
    RicostruisciRighe()
  End Sub

  Private Sub btn_Nuovo_Click(sender As Object, e As RoutedEventArgs)
    If Not ConfermaScartoModifiche() Then Exit Sub
    Dim Nome As String = NomeLibero("New filter")
    Dim F As New clsFiltroNominato With {.Nome = Nome}
    clsFiltri.Salva(F)
    RicaricaElenco(Nome)
  End Sub

  Private Sub btn_Duplica_Click(sender As Object, e As RoutedEventArgs)
    If Corrente Is Nothing Then Exit Sub
    Dim Copia As clsFiltroNominato = Corrente.Clona()
    Copia.Nome = NomeLibero(Corrente.Nome & " copy")
    clsFiltri.Salva(Copia)
    Sporco = False
    RicaricaElenco(Copia.Nome)
  End Sub

  Private Sub btn_Elimina_Click(sender As Object, e As RoutedEventArgs)
    If lst_Filtri.SelectedItem Is Nothing Then Exit Sub
    Dim Nome As String = CStr(lst_Filtri.SelectedItem)
    If MsgBox("Delete the filter """ & Nome & """ from the profile?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Filter") <> MsgBoxResult.Yes Then Exit Sub
    clsFiltri.Elimina(Nome)
    AppConfig.Salva()
    Sporco = False
    RicaricaElenco("")
  End Sub

  Private Function NomeLibero(Base As String) As String
    Dim Nome As String = Base
    Dim n As Integer = 2
    While clsFiltri.Trova(Nome) IsNot Nothing
      Nome = Base & " " & n
      n += 1
    End While
    Return Nome
  End Function

  Private Sub txt_Nome_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txt_Nome.TextChanged
    If Caricando OrElse Corrente Is Nothing Then Exit Sub
    Sporco = True
  End Sub

  Private Sub txt_Note_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txt_Note.TextChanged
    If Caricando OrElse Corrente Is Nothing Then Exit Sub
    Sporco = True
  End Sub

  Private Sub btn_Salva_Click(sender As Object, e As RoutedEventArgs)
    If Corrente Is Nothing Then Exit Sub
    Dim Nome As String = txt_Nome.Text.Trim
    If Nome = "" Then
      MsgBox("The filter name cannot be empty.", MsgBoxStyle.Exclamation, "Filter")
      Exit Sub
    End If
    Dim NomeVecchio As String = If(lst_Filtri.SelectedItem Is Nothing, "", CStr(lst_Filtri.SelectedItem))
    Dim Esistente As clsFiltroNominato = clsFiltri.Trova(Nome)
    If Esistente IsNot Nothing AndAlso Not String.Equals(Nome, NomeVecchio, StringComparison.OrdinalIgnoreCase) Then
      If MsgBox("A filter named """ & Nome & """ already exists. Replace it?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Filter") <> MsgBoxResult.Yes Then Exit Sub
    End If
    If NomeVecchio <> "" AndAlso Not String.Equals(Nome, NomeVecchio, StringComparison.OrdinalIgnoreCase) Then clsFiltri.Elimina(NomeVecchio)
    Corrente.Nome = Nome
    Corrente.Note = txt_Note.Text.Trim
    clsFiltri.Salva(Corrente)
    AppConfig.Salva()
    Sporco = False
    FiltroScelto = Nome
    RicaricaElenco(Nome)
  End Sub

  Private Sub btn_Chiudi_Click(sender As Object, e As RoutedEventArgs)
    If Not ConfermaScartoModifiche() Then Exit Sub
    If lst_Filtri.SelectedItem IsNot Nothing Then FiltroScelto = CStr(lst_Filtri.SelectedItem)
    Sporco = False
    Me.DialogResult = True
    Me.Close()
  End Sub

  Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
    ' chiusura con la X: stessa conferma di Close, se ci sono modifiche non salvate
    If Sporco AndAlso Not ConfermaScartoModifiche() Then e.Cancel = True
    MyBase.OnClosing(e)
  End Sub

  ' ---------------------------------------------------------------- righe dell'albero

  Private Sub RicostruisciRighe()
    pnl_Righe.Children.Clear()
    If Corrente Is Nothing Then
      txt_Conteggio.Text = "No filter selected. Use New to create one."
      txt_Descrizione.Text = ""
      Exit Sub
    End If
    AggiungiRiga(Corrente.Radice, Nothing, 0)
    AggiornaConteggio()
  End Sub

  Private Sub Modificato()
    If Caricando Then Exit Sub
    Sporco = True
    AggiornaConteggio()
  End Sub

  Private Sub AggiungiRiga(N As clsFiltroNodo, Padre As clsFiltroNodo, Livello As Integer)
    Dim Riga As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(Livello * 28, 2, 0, 2)}

    Dim ChkAttivo As New CheckBox With {.IsChecked = N.Attivo, .ToolTip = "Enabled", .VerticalAlignment = VerticalAlignment.Center, .Margin = New Thickness(0, 0, 6, 0)}
    AddHandler ChkAttivo.Click, Sub()
                                  N.Attivo = (ChkAttivo.IsChecked = True)
                                  Modificato()
                                End Sub
    Riga.Children.Add(ChkAttivo)

    Dim ChkNot As New CheckBox With {.IsChecked = N.Negato, .Content = "NOT", .ToolTip = "Invert the result of this " & If(N.Tipo = eFiltroTipoNodo.Gruppo, "group", "condition"), .VerticalAlignment = VerticalAlignment.Center, .Margin = New Thickness(0, 0, 8, 0)}
    AddHandler ChkNot.Click, Sub()
                               N.Negato = (ChkNot.IsChecked = True)
                               Modificato()
                             End Sub
    Riga.Children.Add(ChkNot)

    If N.Tipo = eFiltroTipoNodo.Gruppo Then
      Dim CmbOp As New ComboBox With {.Width = 70, .FontWeight = FontWeights.Bold}
      CmbOp.Items.Add("AND")
      CmbOp.Items.Add("OR")
      CmbOp.SelectedIndex = If(N.Operatore = eFiltroOperatoreGruppo.OpAnd, 0, 1)
      CmbOp.ToolTip = "AND: all the items must be true. OR: at least one."
      AddHandler CmbOp.SelectionChanged, Sub()
                                           N.Operatore = If(CmbOp.SelectedIndex = 0, eFiltroOperatoreGruppo.OpAnd, eFiltroOperatoreGruppo.OpOr)
                                           Modificato()
                                         End Sub
      Riga.Children.Add(CmbOp)

      Dim BtnC As New Button With {.Content = "+ Condition", .Margin = New Thickness(10, 0, 0, 0), .Padding = New Thickness(8, 0, 8, 0)}
      AddHandler BtnC.Click, Sub()
                               N.Figli.Add(clsFiltroNodo.NuovaCondizione("", False, True, False, 0, 0))
                               Sporco = True
                               RicostruisciRighe()
                             End Sub
      Riga.Children.Add(BtnC)

      Dim BtnG As New Button With {.Content = "+ Group", .Margin = New Thickness(6, 0, 0, 0), .Padding = New Thickness(8, 0, 8, 0)}
      AddHandler BtnG.Click, Sub()
                               N.Figli.Add(clsFiltroNodo.NuovoGruppo(If(N.Operatore = eFiltroOperatoreGruppo.OpAnd, eFiltroOperatoreGruppo.OpOr, eFiltroOperatoreGruppo.OpAnd)))
                               Sporco = True
                               RicostruisciRighe()
                             End Sub
      Riga.Children.Add(BtnG)
    Else
      Riga.Children.Add(CreaComboCanale(N))

      Dim ChkAbs As New CheckBox With {.IsChecked = N.AbsVal, .Content = "Abs", .ToolTip = "Use the absolute value", .VerticalAlignment = VerticalAlignment.Center, .Margin = New Thickness(8, 0, 0, 0)}
      AddHandler ChkAbs.Click, Sub()
                               N.AbsVal = (ChkAbs.IsChecked = True)
                               Modificato()
                             End Sub
      Riga.Children.Add(ChkAbs)

      Riga.Children.Add(CreaLimite(N, True))
      Riga.Children.Add(CreaLimite(N, False))
    End If

    If Padre IsNot Nothing Then
      Dim BtnDel As New Button With {.Content = "✕", .ToolTip = "Remove", .Width = 24, .Margin = New Thickness(10, 0, 0, 0)}
      AddHandler BtnDel.Click, Sub()
                                 Padre.Figli.Remove(N)
                                 Sporco = True
                                 RicostruisciRighe()
                               End Sub
      Riga.Children.Add(BtnDel)
    End If

    pnl_Righe.Children.Add(Riga)

    If N.Tipo = eFiltroTipoNodo.Gruppo Then
      For Each Figlio As clsFiltroNodo In N.Figli.ToList
        AggiungiRiga(Figlio, N, Livello + 1)
      Next
    End If
  End Sub

  ''' <summary>Selettore di canale con ricerca "contiene" (lo stesso del tab XY Plots). Un canale non presente nel dataset resta nel filtro e viene segnalato in rosso.</summary>
  Private Function CreaComboCanale(N As clsFiltroNodo) As FrameworkElement
    Dim Pannello As New StackPanel With {.Orientation = Orientation.Horizontal}
    Dim Picker As New ChannelPicker With {.Width = 260, .ItemsSource = Canali}
    Dim Avviso As New TextBlock With {.Foreground = Brushes.Red, .VerticalAlignment = VerticalAlignment.Center, .Margin = New Thickness(4, 0, 0, 0)}

    Dim Sel As clsChannel2020 = Canali.FirstOrDefault(Function(x) x.ChannelId = N.ChannelId)
    Picker.SelectedChannel = Sel
    If Sel Is Nothing AndAlso N.ChannelId <> "" Then Avviso.Text = "'" & N.ChannelId & "' not in dataset"

    Dim Desc As ComponentModel.DependencyPropertyDescriptor = ComponentModel.DependencyPropertyDescriptor.FromProperty(ChannelPicker.SelectedChannelProperty, GetType(ChannelPicker))
    Desc.AddValueChanged(Picker, Sub()
                                   Dim C As clsChannel2020 = Picker.SelectedChannel
                                   If C Is Nothing Then Exit Sub
                                   N.ChannelId = C.ChannelId
                                   Avviso.Text = ""
                                   Modificato()
                                 End Sub)
    Pannello.Children.Add(Picker)
    Pannello.Children.Add(Avviso)
    Return Pannello
  End Function

  ''' <summary>Casella di controllo + campo numerico per il minimo o il massimo di una condizione.</summary>
  Private Function CreaLimite(N As clsFiltroNodo, EMinimo As Boolean) As StackPanel
    Dim P As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10, 0, 0, 0)}
    Dim Attivo As Boolean = If(EMinimo, N.UsaMin, N.UsaMax)
    Dim Valore As Double = If(EMinimo, N.Min, N.Max)
    Dim Chk As New CheckBox With {.IsChecked = Attivo, .Content = If(EMinimo, "Min", "Max"), .VerticalAlignment = VerticalAlignment.Center, .Width = 46}
    Dim Txt As New TextBox With {.Width = 70, .HorizontalContentAlignment = HorizontalAlignment.Right, .IsEnabled = Attivo,
                                 .Text = Valore.ToString("0.##########", CultureInfo.InvariantCulture)}
    NumericInput.SetKind(Txt, "Decimal")
    AddHandler Chk.Click, Sub()
                            Dim A As Boolean = Chk.IsChecked = True
                            If EMinimo Then N.UsaMin = A Else N.UsaMax = A
                            Txt.IsEnabled = A
                            Modificato()
                          End Sub
    AddHandler Txt.TextChanged, Sub()
                                  Dim V As Double
                                  If Double.TryParse(Txt.Text, NumberStyles.Float, CultureInfo.InvariantCulture, V) Then
                                    If EMinimo Then N.Min = V Else N.Max = V
                                    Modificato()
                                  End If
                                End Sub
    P.Children.Add(Chk)
    P.Children.Add(Txt)
    Return P
  End Function

  ' ---------------------------------------------------------------- conteggio live

  Private Sub AggiornaConteggio()
    If Corrente Is Nothing Then Exit Sub
    Try
      Dim Compilato As clsFiltroCompilato = clsFiltri.Compila(Corrente)
      txt_Descrizione.Text = Compilato.Descrizione

      Dim Totale As Integer = TotaleRighe(Corrente.Radice)
      If Compilato.Maschera Is Nothing Then
        txt_Conteggio.Text = If(Totale > 0, "No active condition: all " & Totale.ToString("N0") & " samples pass.", "No active condition.")
        Exit Sub
      End If
      If Totale <= 0 Then
        txt_Conteggio.Text = "No dataset loaded: the samples cannot be counted."
        Exit Sub
      End If
      Dim Validi As Integer = 0
      For i As Integer = 0 To Totale - 1
        If Compilato.Maschera(i) Then Validi += 1
      Next
      txt_Conteggio.Text = Validi.ToString("N0") & " / " & Totale.ToString("N0") & " samples pass (" & (100.0 * Validi / Totale).ToString("0.0") & "%)"
    Catch ex As Exception
      txt_Conteggio.Text = "Error evaluating the filter: " & ex.Message
    End Try
  End Sub

  ''' <summary>Numero di righe del dataset, preso dal primo canale usato dal filtro (0 se non ce ne sono nel dataset caricato).</summary>
  Private Function TotaleRighe(N As clsFiltroNodo) As Integer
    If DataProvider2020 Is Nothing OrElse N Is Nothing Then Return 0
    If N.Tipo = eFiltroTipoNodo.Condizione Then
      If String.IsNullOrWhiteSpace(N.ChannelId) Then Return 0
      Dim Ch As clsChannel2020 = DataProvider2020.CanaleDbl(N.ChannelId)
      If Ch Is Nothing OrElse Ch.Valori Is Nothing Then Return 0
      Return Ch.Valori.Length
    End If
    For Each F As clsFiltroNodo In N.Figli
      Dim T As Integer = TotaleRighe(F)
      If T > 0 Then Return T
    Next
    Return 0
  End Function

End Class
