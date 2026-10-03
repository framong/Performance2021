''' <summary>
''' Finestra modale del modello teorico del leeway: modifica dei coefficienti, valutazione dell'errore rispetto
''' a una serie di riferimento e adattamento automatico dei coefficienti marcati "Fit".
''' Lavora su una copia: il profilo viene toccato solo con Save.
''' </summary>
Public Class LeewayModelForm

  Private _S As clsLeewayModelSettings

  ''' <summary>True se l'utente ha chiesto anche il ricalcolo del canale Leeway Model.</summary>
  Public Property RichiestoRefresh As Boolean = False

  Public Sub New()
    InitializeComponent()
    If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) Then Exit Sub

    cmb_Riferimento.Items.Add("Leeway Normalized (logged)")
    cmb_Riferimento.Items.Add("Leeway Recalc (COG - HDG)")
    cmb_Riferimento.Items.Add("Leeway Recalc 3s")
    cmb_Riferimento.Items.Add("Leeway Recalc 30s")

    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      _S = New clsLeewayModelSettings
      _S.NormalizzaValoriMancanti()
    Else
      _S = clsLeewayModel.ImpostazioniCorrenti().Clona()
    End If
    Mostra()
  End Sub

  Private Sub Mostra()
    grd_Parametri.ItemsSource = Nothing
    grd_Parametri.ItemsSource = _S.Parametri
    cmb_Riferimento.SelectedIndex = Math.Min(3, Math.Max(0, _S.Riferimento))
    rdb_Visibile.IsChecked = _S.SoloRangeVisibile
    rdb_Tutto.IsChecked = Not _S.SoloRangeVisibile
    txt_CanAngolo.Text = _S.CanardAngleChannel
    txt_CanImmersione.Text = _S.CanardImmersionChannel
    txt_Cant.Text = _S.KeelCantChannel
    chk_AngoloPerMura.IsChecked = _S.CanardAngleSignedByTack
    chk_CantPerMura.IsChecked = _S.KeelCantSignedByTack
    chk_CantInvertito.IsChecked = _S.KeelCantInverted
  End Sub

  ''' <summary>Dai controlli alla copia di lavoro.</summary>
  Private Sub Leggi()
    grd_Parametri.CommitEdit(DataGridEditingUnit.Row, True)
    _S.Riferimento = Math.Max(0, cmb_Riferimento.SelectedIndex)
    _S.SoloRangeVisibile = rdb_Visibile.IsChecked = True
    _S.CanardAngleChannel = txt_CanAngolo.Text.Trim
    _S.CanardImmersionChannel = txt_CanImmersione.Text.Trim
    _S.KeelCantChannel = txt_Cant.Text.Trim
    _S.CanardAngleSignedByTack = chk_AngoloPerMura.IsChecked = True
    _S.KeelCantSignedByTack = chk_CantPerMura.IsChecked = True
    _S.KeelCantInverted = chk_CantInvertito.IsChecked = True
  End Sub

  ''' <summary>Righe da usare nell'adattamento: range visibile nei plot oppure tutto il file.</summary>
  Private Sub Intervallo(ByRef IdIni As Integer, ByRef IdFin As Integer)
    IdIni = 0
    IdFin = Integer.MaxValue
    If Not _S.SoloRangeVisibile Then Exit Sub
    If DataPlotSync Is Nothing OrElse DataPlotSync.VisibleRange Is Nothing Then Exit Sub
    Dim TR As clsTimeRange = DataPlotSync.VisibleRange
    If TR.Start.ToOADate <= 0 OrElse TR.Finish <= TR.Start Then Exit Sub
    IdIni = DataProvider2020.TrovaIndice(TR.Start)
    IdFin = DataProvider2020.TrovaIndice(TR.Finish)
  End Sub

  Private Function PreparaIngressi() As clsLeewayIngressi
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then
      MsgBox("Load a file first.", MsgBoxStyle.Exclamation, "Leeway Model")
      Return Nothing
    End If
    ' lettura dei canali sul thread UI: la lettura Parquet non e' thread-safe
    Return clsLeewayModel.CaricaIngressi(DataProvider2020, _S, True)
  End Function

  Private Sub btn_Valuta_Click(sender As Object, e As RoutedEventArgs)
    Leggi()
    Dim Ing = PreparaIngressi()
    If Ing Is Nothing Then Exit Sub
    Dim IdIni, IdFin As Integer
    Intervallo(IdIni, IdFin)
    txt_Report.Text = clsLeewayModel.Valuta(Ing, _S, IdIni, IdFin)
  End Sub

  Private Async Sub btn_Fit_Click(sender As Object, e As RoutedEventArgs)
    Leggi()
    Dim Ing = PreparaIngressi()
    If Ing Is Nothing Then Exit Sub
    Dim IdIni, IdFin As Integer
    Intervallo(IdIni, IdFin)
    Dim Copia As clsLeewayModelSettings = _S.Clona()
    ImpostaOccupato(True)
    txt_Report.Text = "Fitting..."
    Try
      ' il calcolo lavora solo sugli array gia' caricati
      Dim Ris As clsLeewayFitResult = Await System.Threading.Tasks.Task.Run(Function() clsLeewayModel.Adatta(Ing, Copia, IdIni, IdFin))
      If Not Ris.NuoviValori Is Nothing Then
        For Each p In _S.Parametri
          If Ris.NuoviValori.ContainsKey(p.Nome) Then p.Valore = Ris.NuoviValori(p.Nome)
        Next
      End If
      txt_Report.Text = Ris.Report
    Catch ex As Exception
      txt_Report.Text = "Fit failed: " & ex.Message
    Finally
      ImpostaOccupato(False)
    End Try
  End Sub

  Private Sub ImpostaOccupato(Occupato As Boolean)
    btn_Fit.IsEnabled = Not Occupato
    btn_Valuta.IsEnabled = Not Occupato
    btn_Defaults.IsEnabled = Not Occupato
    btn_Save.IsEnabled = Not Occupato
    btn_SaveAndRefresh.IsEnabled = Not Occupato
    Mouse.OverrideCursor = If(Occupato, Cursors.Wait, Nothing)
  End Sub

  Private Sub btn_Defaults_Click(sender As Object, e As RoutedEventArgs)
    If MsgBox("Reset all the coefficients and the channel settings to their defaults?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Leeway Model") <> MsgBoxResult.Yes Then Exit Sub
    _S = New clsLeewayModelSettings
    _S.CaricaValoriDefault()
    Mostra()
    txt_Report.Text = ""
  End Sub

  Private Function Salva() As Boolean
    Leggi()
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return False
    AppConfig.ActiveProfile.LeewayModel = _S.Clona()
    AppConfig.Salva()
    Return True
  End Function

  Private Sub btn_Save_Click(sender As Object, e As RoutedEventArgs)
    If Salva() Then
      Me.DialogResult = True
    End If
  End Sub

  Private Sub btn_SaveAndRefresh_Click(sender As Object, e As RoutedEventArgs)
    If Salva() Then
      RichiestoRefresh = True
      Me.DialogResult = True
    End If
  End Sub

  Private Sub btn_Cancel_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
  End Sub

End Class
