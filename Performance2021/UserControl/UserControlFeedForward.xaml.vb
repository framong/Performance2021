Imports System.Collections.ObjectModel
Imports System.Globalization
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.Annotations
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries

''' <summary>
''' Tab Feed Forward: per ogni fascia di vento si dividono in bin i campioni validi della variabile X da ottimizzare, di ogni bin si calcola la media
''' della variabile di efficienza Y e un modello (retta, parabola o curva unimodale) indica dove sta l'ottimo di X e quanto vale Y la'.
''' Un report ha impostazioni comuni e una lista di canali Y: il calcolo e il PDF (una serie di grafici per ogni Y) usano le stesse impostazioni.
''' Il calcolo e' in clsFeedForward (senza dipendenze dai dati); qui ci sono impostazioni, scelta dei campioni, grafico, tabella e stampa.
''' La lettura dei canali e' lazy e non thread-safe: tutto gira sul thread UI.
''' </summary>
Public Class UserControlFeedForward

  Private Class clsVoceFiltro
    Public Property Valore As String
    Public Property Testo As String
  End Class

  Private Class clsVoceY
    Public Property Id As String
    Public Property Nome As String
  End Class

  ''' <summary>Riga della tabella riassuntiva: una per fascia di vento.</summary>
  Public Class clsFfRiga
    Public Property Fascia As String
    Public Property Campioni As Integer
    Public Property Bin As String
    Public Property OttimoX As String
    Public Property OttimoY As String
    Public Property Errore As String
    Public Property MigliorX As String
    Public Property MigliorY As String
    Public Property Guadagno As String
    Public Property R2 As String
    Public Property Estrapolato As String
    Public Property Affidabilita As String
    Public Property Associazione As String
    Public Property Nota As String
    Public Property Dati As clsFfFascia
    ''' <summary>Se la fascia e' disegnata nel grafico (checkbox della tabella).</summary>
    Public Property Visibile As Boolean
    ''' <summary>Media di Y per bin (chiave = indice del bin; tra parentesi se il bin non e' nel fit).</summary>
    Public Property Medie As New Dictionary(Of Integer, String)
    ''' <summary>Per ogni bin: se la media e' entrata nel fit (altrimenti in tabella e' grigia).</summary>
    Public Property MedieUsate As New Dictionary(Of Integer, Boolean)
  End Class

  ''' <summary>Risultati dell'ultimo calcolo: per ogni canale Y (ChannelId) le fasce di vento.</summary>
  Private Risultati As New Dictionary(Of String, List(Of clsFfFascia))
  Private Righe As New List(Of clsFfRiga)
  ''' <summary>Fasce selezionate (checkbox Show) per ogni canale Y: ChannelId -> inizio fascia (TwsLow).</summary>
  Private Selezione As New Dictionary(Of String, HashSet(Of Double))
  Private CanaleX As clsChannel2020 = Nothing
  Private CanaliY As New Dictionary(Of String, clsChannel2020)
  Private CanaleYVista As clsChannel2020 = Nothing
  Private VociY As New ObservableCollection(Of clsVoceY)
  Private DescrizioneFiltro As String = ""
  Private Inizializzato As Boolean = False
  Private Caricando As Boolean = False

  Public Sub New()
    InitializeComponent()
    cmb_Sorgente.Items.Add("Current visible range")
    cmb_Sorgente.Items.Add("All straight lines")
    cmb_Sorgente.Items.Add("All straight lines (Vmg)")
    cmb_Sorgente.Items.Add("Selected straight lines")
    cmb_Sorgente.Items.Add("All data")
    cmb_Modello.Items.Add("Linear (weighted)")
    cmb_Modello.Items.Add("Quadratic (weighted)")
    cmb_Modello.Items.Add("Local (unimodal peak; with Push X: trend of the last 3 bins)")
    cmb_Obiettivo.Items.Add("Push X to its maximum (Y follows the trend)")
    cmb_Obiettivo.Items.Add("Maximize Y (Y is an efficiency)")
    cmb_Obiettivo.Items.Add("Minimize Y (Y is an efficiency)")
    lst_Y.ItemsSource = VociY
  End Sub

  ''' <summary>Obiettivo o modello cambiati: se ci sono risultati si ricalcola subito, cosi' la tabella non resta quella vecchia.</summary>
  Private Sub ModelloObiettivo_Cambiato(sender As Object, e As SelectionChangedEventArgs) Handles cmb_Obiettivo.SelectionChanged, cmb_Modello.SelectionChanged
    If Not Inizializzato OrElse Caricando OrElse Risultati.Count = 0 Then Exit Sub
    btn_Calcola_Click(Nothing, Nothing)
  End Sub

  Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
    If Inizializzato Then Exit Sub
    Inizializzato = True
    CaricaImpostazioni()
  End Sub

  ' ---------------------------------------------------------------- impostazioni del report

  ''' <summary>Report di lavoro (quello mostrato nei campi): sta nel profilo, i report salvati con un nome ne sono copie.</summary>
  Private Function Rep() As clsFeedForwardReport
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return New clsFeedForwardReport
    If AppConfig.ActiveProfile.FeedForwardSettings Is Nothing Then AppConfig.ActiveProfile.FeedForwardSettings = New clsFeedForwardReport
    Return AppConfig.ActiveProfile.FeedForwardSettings
  End Function

  Private Function Salvati() As List(Of clsFeedForwardReport)
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return New List(Of clsFeedForwardReport)
    If AppConfig.ActiveProfile.FeedForwardReports Is Nothing Then AppConfig.ActiveProfile.FeedForwardReports = New List(Of clsFeedForwardReport)
    Return AppConfig.ActiveProfile.FeedForwardReports
  End Function

  Private Shared Function Testo(V As Double) As String
    Return V.ToString("0.######", CultureInfo.InvariantCulture)
  End Function

  Private Shared Function Numero(T As String, Predefinito As Double) As Double
    Dim V As Double
    If Double.TryParse(T, NumberStyles.Float, CultureInfo.InvariantCulture, V) Then Return V
    Return Predefinito
  End Function

  Private Function CanaleDaId(Id As String) As clsChannel2020
    If String.IsNullOrWhiteSpace(Id) OrElse DataProvider2020 Is Nothing OrElse DataProvider2020.Channels Is Nothing Then Return Nothing
    Return DataProvider2020.Channels.Canale(Id)
  End Function

  Private Function NomeCanale(Id As String) As String
    Dim C As clsChannel2020 = CanaleDaId(Id)
    Return If(C Is Nothing, Id, C.LongName)
  End Function

  Private Sub CaricaImpostazioni()
    Caricando = True
    Dim S As clsFeedForwardReport = Rep()
    Dim P As clsFfParametri = S.Parametri
    P.Aggiorna()
    If DataProvider2020 IsNot Nothing AndAlso DataProvider2020.Channels IsNot Nothing Then
      Dim Elenco As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.OrderBy(Function(x) x.LongName).ToList
      pck_X.ItemsSource = Elenco
      pck_Tws.ItemsSource = Elenco
      pck_YNuovo.ItemsSource = Elenco
      pck_X.SelectedChannel = CanaleDaId(S.XChannelId)
      Dim Tws As clsChannel2020 = CanaleDaId(S.TwsChannelId)
      If Tws Is Nothing Then Tws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
      pck_Tws.SelectedChannel = Tws
    End If
    VociY.Clear()
    For Each Id As String In S.YChannelIds
      VociY.Add(New clsVoceY With {.Id = Id, .Nome = NomeCanale(Id)})
    Next
    cmb_Sorgente.SelectedIndex = CInt(S.Sorgente)
    cmb_Modello.SelectedIndex = CInt(P.Modello)
    cmb_Obiettivo.SelectedIndex = If(P.SpingiX, 0, If(P.Massimizza, 1, 2))
    txt_TwsMin.Text = Testo(P.TwsMin)
    txt_TwsMax.Text = Testo(P.TwsMax)
    txt_TwsStep.Text = Testo(P.TwsStep)
    txt_XStart.Text = Testo(P.XBinStart)
    txt_XEnd.Text = Testo(P.XBinEnd)
    txt_XWidth.Text = Testo(P.XBinWidth)
    txt_MinCamp.Text = P.MinCampioniBin.ToString
    txt_MinBin.Text = P.MinBinPerFit.ToString
    txt_Extra.Text = Testo(P.ExtrapolazioneBin)
    txt_XRif.Text = Testo(S.XRiferimento)
    txt_PuntiDim.Text = S.PuntiDimensione.ToString
    txt_PuntiOpacita.Text = S.PuntiOpacita.ToString
    txt_MedieDim.Text = S.MedieDimensione.ToString
    txt_RighePdf.Text = S.RighePagina.ToString
    txt_ColonnePdf.Text = S.ColonnePagina.ToString
    chk_PdfInsieme.IsChecked = S.PdfInsieme
    txt_NomiAssiDim.Text = S.NomiAssiDimensione.ToString
    txt_NomiAssiOpacita.Text = S.NomiAssiOpacita.ToString
    txt_ValoriAssiDim.Text = S.ValoriAssiDimensione.ToString
    txt_DecimaliAssi.Text = S.DecimaliAssiExtra.ToString
    txt_TitoloDim.Text = S.TitoloDimensione.ToString
    cmb_Filtro_DropDownOpened(Nothing, Nothing)
    cmb_Filtro.SelectedValue = If(S.NamedFilter, "")
    AggiornaElencoReport()
    Caricando = False
    If VociY.Count > 0 Then lst_Y.SelectedIndex = 0
  End Sub

  ''' <summary>Legge i campi nel report di lavoro (e salva il profilo).</summary>
  Private Sub LeggiImpostazioni()
    Dim S As clsFeedForwardReport = Rep()
    Dim P As clsFfParametri = S.Parametri
    S.Sorgente = CType(System.Math.Max(0, cmb_Sorgente.SelectedIndex), eFfSorgente)
    S.XChannelId = If(pck_X.SelectedChannel Is Nothing, "", pck_X.SelectedChannel.ChannelId)
    S.TwsChannelId = If(pck_Tws.SelectedChannel Is Nothing, "", pck_Tws.SelectedChannel.ChannelId)
    S.NamedFilter = If(cmb_Filtro.SelectedValue Is Nothing, "", CStr(cmb_Filtro.SelectedValue))
    S.YChannelIds = VociY.Select(Function(v) v.Id).ToList
    P.Modello = CType(System.Math.Max(0, cmb_Modello.SelectedIndex), eFfModello)
    P.SpingiX = cmb_Obiettivo.SelectedIndex <= 0
    P.Massimizza = cmb_Obiettivo.SelectedIndex <> 2
    P.TwsMin = Numero(txt_TwsMin.Text, P.TwsMin)
    P.TwsMax = Numero(txt_TwsMax.Text, P.TwsMax)
    P.TwsStep = Numero(txt_TwsStep.Text, P.TwsStep)
    P.XBinStart = Numero(txt_XStart.Text, P.XBinStart)
    P.XBinEnd = Numero(txt_XEnd.Text, P.XBinEnd)
    P.XBinWidth = Numero(txt_XWidth.Text, P.XBinWidth)
    P.MinCampioniBin = CInt(Numero(txt_MinCamp.Text, P.MinCampioniBin))
    P.MinBinPerFit = CInt(Numero(txt_MinBin.Text, P.MinBinPerFit))
    P.ExtrapolazioneBin = Numero(txt_Extra.Text, P.ExtrapolazioneBin)
    S.XRiferimento = Numero(txt_XRif.Text, S.XRiferimento)
    LeggiGrafica()
    S.PdfInsieme = chk_PdfInsieme.IsChecked = True
    SalvaProfilo()
  End Sub

  Private Sub chk_PdfInsieme_Click(sender As Object, e As RoutedEventArgs)
    If Not Inizializzato OrElse Caricando Then Exit Sub
    Rep().PdfInsieme = chk_PdfInsieme.IsChecked = True
    SalvaProfilo()
  End Sub

  Private Sub SalvaProfilo()
    Try
      AppConfig.Salva()
    Catch ex As Exception
      Console.WriteLine("FeedForward: salvataggio profilo: " & ex.Message)
    End Try
  End Sub

  ''' <summary>Legge dimensione e trasparenza dei pallini e il numero di grafici per pagina (con limiti sensati).</summary>
  Private Sub LeggiGrafica()
    Dim S As clsFeedForwardReport = Rep()
    S.PuntiDimensione = System.Math.Max(1, System.Math.Min(30, CInt(Numero(txt_PuntiDim.Text, S.PuntiDimensione))))
    S.PuntiOpacita = System.Math.Max(5, System.Math.Min(100, CInt(Numero(txt_PuntiOpacita.Text, S.PuntiOpacita))))
    S.MedieDimensione = System.Math.Max(1, System.Math.Min(40, CInt(Numero(txt_MedieDim.Text, S.MedieDimensione))))
    S.RighePagina = System.Math.Max(1, System.Math.Min(6, CInt(Numero(txt_RighePdf.Text, S.RighePagina))))
    S.ColonnePagina = System.Math.Max(1, System.Math.Min(6, CInt(Numero(txt_ColonnePdf.Text, S.ColonnePagina))))
    txt_PuntiDim.Text = S.PuntiDimensione.ToString
    txt_PuntiOpacita.Text = S.PuntiOpacita.ToString
    txt_MedieDim.Text = S.MedieDimensione.ToString
    txt_RighePdf.Text = S.RighePagina.ToString
    txt_ColonnePdf.Text = S.ColonnePagina.ToString
    S.NomiAssiDimensione = System.Math.Max(8, System.Math.Min(100, CInt(Numero(txt_NomiAssiDim.Text, S.NomiAssiDimensione))))
    S.NomiAssiOpacita = System.Math.Max(5, System.Math.Min(100, CInt(Numero(txt_NomiAssiOpacita.Text, S.NomiAssiOpacita))))
    S.ValoriAssiDimensione = System.Math.Max(6, System.Math.Min(40, CInt(Numero(txt_ValoriAssiDim.Text, S.ValoriAssiDimensione))))
    S.DecimaliAssiExtra = System.Math.Max(0, System.Math.Min(3, CInt(Numero(txt_DecimaliAssi.Text, S.DecimaliAssiExtra))))
    S.TitoloDimensione = System.Math.Max(6, System.Math.Min(30, CInt(Numero(txt_TitoloDim.Text, S.TitoloDimensione))))
    txt_NomiAssiDim.Text = S.NomiAssiDimensione.ToString
    txt_NomiAssiOpacita.Text = S.NomiAssiOpacita.ToString
    txt_ValoriAssiDim.Text = S.ValoriAssiDimensione.ToString
    txt_DecimaliAssi.Text = S.DecimaliAssiExtra.ToString
    txt_TitoloDim.Text = S.TitoloDimensione.ToString
  End Sub

  ''' <summary>Dimensione o trasparenza cambiate: si ridisegna la fascia senza ricalcolare.</summary>
  Private Sub Grafica_Cambiata(sender As Object, e As RoutedEventArgs)
    If Not Inizializzato OrElse Caricando Then Exit Sub
    LeggiGrafica()
    SalvaProfilo()
    RidisegnaVisibili()
  End Sub

  ' ---------------------------------------------------------------- report salvati

  Private Sub AggiornaElencoReport()
    Dim Nome As String = Rep().Nome
    Dim Prec As Boolean = Caricando
    Caricando = True
    cmb_Report.ItemsSource = Salvati().OrderBy(Function(x) x.Nome).Select(Function(x) x.Nome).ToList
    cmb_Report.SelectedItem = If(String.IsNullOrEmpty(Nome), Nothing, Nome)
    Caricando = Prec
  End Sub

  Private Sub cmb_Report_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    If Caricando OrElse cmb_Report.SelectedItem Is Nothing Then Exit Sub
    Dim Nome As String = CStr(cmb_Report.SelectedItem)
    Dim R As clsFeedForwardReport = Salvati().FirstOrDefault(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase))
    If R Is Nothing Then Exit Sub
    AppConfig.ActiveProfile.FeedForwardSettings = R.Clona()
    Risultati.Clear()
    Selezione.Clear()
    grd_Fasce.ItemsSource = Nothing
    DisegnaFascia(Nothing, Nothing)
    CaricaImpostazioni()
    txt_Stato.Text = "Report '" & Nome & "' loaded. Press Calculate."
  End Sub

  Private Sub SalvaReport(Nome As String)
    LeggiImpostazioni()
    Dim S As clsFeedForwardReport = Rep()
    S.Nome = Nome
    ' il filtro viene salvato anche come copia: il report non cambia se poi si modifica la libreria
    Dim Copia As clsFiltroNominato = Nothing
    If S.NamedFilter <> "" Then
      Dim F As clsFiltroNominato = clsFiltri.Trova(S.NamedFilter)
      If F IsNot Nothing Then Copia = F.Clona()
    End If
    S.FiltroSnapshot = Copia
    Dim L As List(Of clsFeedForwardReport) = Salvati()
    Dim Idx As Integer = L.FindIndex(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase))
    If Idx >= 0 Then L(Idx) = S.Clona() Else L.Add(S.Clona())
    SalvaProfilo()
    AggiornaElencoReport()
    txt_Stato.Text = "Report '" & Nome & "' saved."
  End Sub

  Private Function ChiediNome(Predefinito As String) As String
    Dim Input As New UserControl_InputBox(Predefinito, "Report name")
    Input.Owner = Window.GetWindow(Me)
    Input.ShowDialog()
    If Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True Then Return Input.Testo.Trim
    Return ""
  End Function

  Private Sub btn_SalvaReport_Click(sender As Object, e As RoutedEventArgs)
    Dim Nome As String = Rep().Nome
    If String.IsNullOrWhiteSpace(Nome) Then Nome = ChiediNome("New report")
    If Nome = "" Then Exit Sub
    SalvaReport(Nome)
  End Sub

  Private Sub btn_SalvaComeReport_Click(sender As Object, e As RoutedEventArgs)
    Dim Nome As String = ChiediNome(If(String.IsNullOrWhiteSpace(Rep().Nome), "New report", Rep().Nome))
    If Nome = "" Then Exit Sub
    If Salvati().Any(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase)) AndAlso Not String.Equals(Nome, Rep().Nome, StringComparison.OrdinalIgnoreCase) Then
      If MsgBox("A report named """ & Nome & """ already exists. Replace it?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Feed Forward") <> MsgBoxResult.Yes Then Exit Sub
    End If
    SalvaReport(Nome)
  End Sub

  Private Sub btn_EliminaReport_Click(sender As Object, e As RoutedEventArgs)
    Dim Nome As String = Rep().Nome
    If String.IsNullOrWhiteSpace(Nome) OrElse Not Salvati().Any(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase)) Then
      MsgBox("Select a saved report first.", MsgBoxStyle.Information, "Feed Forward")
      Exit Sub
    End If
    If MsgBox("Delete the report """ & Nome & """ from the profile?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Feed Forward") <> MsgBoxResult.Yes Then Exit Sub
    Salvati().RemoveAll(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase))
    Rep().Nome = ""
    SalvaProfilo()
    AggiornaElencoReport()
    txt_Stato.Text = "Report '" & Nome & "' deleted (the current settings stay in the fields)."
  End Sub

  ' ---------------------------------------------------------------- canali Y

  Private Sub btn_AggiungiY_Click(sender As Object, e As RoutedEventArgs)
    Dim C As clsChannel2020 = pck_YNuovo.SelectedChannel
    If C Is Nothing Then Exit Sub
    If VociY.Any(Function(v) v.Id = C.ChannelId) Then Exit Sub
    VociY.Add(New clsVoceY With {.Id = C.ChannelId, .Nome = C.LongName})
    lst_Y.SelectedIndex = VociY.Count - 1
    Rep().YChannelIds = VociY.Select(Function(v) v.Id).ToList
  End Sub

  Private Sub btn_RimuoviY_Click(sender As Object, e As RoutedEventArgs)
    Dim V As clsVoceY = TryCast(lst_Y.SelectedItem, clsVoceY)
    If V Is Nothing Then Exit Sub
    Dim Idx As Integer = lst_Y.SelectedIndex
    VociY.Remove(V)
    Risultati.Remove(V.Id)
    Rep().YChannelIds = VociY.Select(Function(x) x.Id).ToList
    If VociY.Count > 0 Then lst_Y.SelectedIndex = System.Math.Min(Idx, VociY.Count - 1)
  End Sub

  Private Sub SpostaY(Delta As Integer)
    Dim Idx As Integer = lst_Y.SelectedIndex
    Dim Nuovo As Integer = Idx + Delta
    If Idx < 0 OrElse Nuovo < 0 OrElse Nuovo >= VociY.Count Then Exit Sub
    VociY.Move(Idx, Nuovo)
    lst_Y.SelectedIndex = Nuovo
    Rep().YChannelIds = VociY.Select(Function(x) x.Id).ToList
  End Sub

  Private Sub btn_SuY_Click(sender As Object, e As RoutedEventArgs)
    SpostaY(-1)
  End Sub

  Private Sub btn_GiuY_Click(sender As Object, e As RoutedEventArgs)
    SpostaY(1)
  End Sub

  Private Sub lst_Y_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    If Caricando Then Exit Sub
    MostraY(TryCast(lst_Y.SelectedItem, clsVoceY))
  End Sub

  ' ---------------------------------------------------------------- filtri

  Private Sub cmb_Filtro_DropDownOpened(sender As Object, e As EventArgs)
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Exit Sub
    Dim Scelto As Object = cmb_Filtro.SelectedValue
    Dim L As New List(Of clsVoceFiltro)
    L.Add(New clsVoceFiltro With {.Valore = "", .Testo = "(none)"})
    For Each F As clsFiltroNominato In clsFiltri.Elenco().OrderBy(Function(x) x.Nome)
      L.Add(New clsVoceFiltro With {.Valore = F.Nome, .Testo = F.Nome})
    Next
    cmb_Filtro.ItemsSource = L
    If Scelto IsNot Nothing Then cmb_Filtro.SelectedValue = Scelto
  End Sub

  Private Sub btn_Filtri_Click(sender As Object, e As RoutedEventArgs)
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      MsgBox("No active profile.", MsgBoxStyle.Exclamation, "Filters")
      Exit Sub
    End If
    Dim Scelto As String = If(cmb_Filtro.SelectedValue Is Nothing, "", CStr(cmb_Filtro.SelectedValue))
    Dim f As New FiltriForm(Scelto)
    f.Owner = Window.GetWindow(Me)
    f.ShowDialog()
    ' il filtro puo' essere stato modificato: la copia del report si rifa' dalla libreria alla prossima registrazione
    Rep().FiltroSnapshot = Nothing
    cmb_Filtro_DropDownOpened(Nothing, Nothing)
    If clsFiltri.Trova(Scelto) IsNot Nothing Then cmb_Filtro.SelectedValue = Scelto
  End Sub

  ' ---------------------------------------------------------------- calcolo

  Private Function RigheDaPeriodi(Lista As IEnumerable(Of clsPeriod2021), N As Integer) As IEnumerable(Of Integer)
    Dim Out As New List(Of Integer)
    If Lista Is Nothing Then Return Out
    For Each p As clsPeriod2021 In Lista
      Dim i0 As Integer = System.Math.Max(0, p.TR.IdRigaIniziale)
      Dim i1 As Integer = System.Math.Min(N - 1, p.TR.IdRigaFinale)
      For i As Integer = i0 To i1
        Out.Add(i)
      Next
    Next
    Return Out
  End Function

  ''' <summary>Indici di riga dei campioni della sorgente scelta (un periodo che ne contiene un altro non conta due volte).</summary>
  Private Function RigheSorgente(S As eFfSorgente, N As Integer) As IEnumerable(Of Integer)
    Dim Out As IEnumerable(Of Integer)
    Select Case S
      Case eFfSorgente.VisibleRange
        Dim TR As clsTimeRange = DataPlotSync.VisibleRange
        If TR Is Nothing Then Return New List(Of Integer)
        Dim i0 As Integer = System.Math.Max(0, TR.IdRigaIniziale)
        Dim i1 As Integer = System.Math.Min(N - 1, TR.IdRigaFinale)
        Out = Enumerable.Range(i0, System.Math.Max(0, i1 - i0 + 1))
      Case eFfSorgente.AllStraightLines
        Out = RigheDaPeriodi(PeriodsManager.CollectionStraightLine, N)
      Case eFfSorgente.AllStraightLinesVmg
        Out = RigheDaPeriodi(PeriodsManager.CollectionStraightLineVmg, N)
      Case eFfSorgente.SelectedStraightLines
        Out = RigheDaPeriodi(PeriodsManager.CollectionStraightLineSelected, N)
      Case Else
        Out = Enumerable.Range(0, N)
    End Select
    Return Out.Distinct()
  End Function

  ''' <summary>Calcola tutti i canali Y del report. False (con messaggio) se non e' possibile.</summary>
  Private Function EseguiCalcolo() As Boolean
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then
      MsgBox("Load a dataset first.", MsgBoxStyle.Exclamation, "Feed Forward")
      Return False
    End If
    If pck_X.SelectedChannel Is Nothing OrElse pck_Tws.SelectedChannel Is Nothing Then
      MsgBox("Choose the X channel and the band channel.", MsgBoxStyle.Exclamation, "Feed Forward")
      Return False
    End If
    If VociY.Count = 0 Then
      MsgBox("Add at least one Y channel.", MsgBoxStyle.Exclamation, "Feed Forward")
      Return False
    End If
    LeggiImpostazioni()
    Dim S As clsFeedForwardReport = Rep()

    Try
      CanaleX = DataProvider2020.CanaleDbl(S.XChannelId)
      Dim ChTws As clsChannel2020 = DataProvider2020.CanaleDbl(S.TwsChannelId)
      If CanaleX Is Nothing OrElse ChTws Is Nothing OrElse CanaleX.Valori Is Nothing OrElse ChTws.Valori Is Nothing Then
        MsgBox("The X or the band channel has no data in this dataset.", MsgBoxStyle.Exclamation, "Feed Forward")
        Return False
      End If

      Dim Maschera As Func(Of Integer, Boolean) = Nothing
      DescrizioneFiltro = "no filter"
      If S.NamedFilter <> "" Then
        Dim Snap As clsFiltroNominato = S.FiltroSnapshot
        Dim F As clsFiltroNominato = clsFiltri.Risolvi(S.NamedFilter, Snap)
        S.FiltroSnapshot = Snap
        If F Is Nothing Then
          MsgBox("The filter '" & S.NamedFilter & "' is not in the profile anymore: no filter is applied.", MsgBoxStyle.Information, "Feed Forward")
        Else
          Dim C As clsFiltroCompilato = clsFiltri.Compila(F)
          Maschera = C.Maschera
          DescrizioneFiltro = clsFiltri.DescrizioneBreve(F)
          If C.Ignorate.Count > 0 Then DescrizioneFiltro &= " (ignored channels: " & String.Join(", ", C.Ignorate) & ")"
        End If
      End If

      Risultati.Clear()
      CanaliY.Clear()
      Dim Mancanti As New List(Of String)
      Dim RigheCamp As List(Of Integer) = Nothing
      For Each V As clsVoceY In VociY
        Dim ChY As clsChannel2020 = DataProvider2020.CanaleDbl(V.Id)
        If ChY Is Nothing OrElse ChY.Valori Is Nothing Then
          Mancanti.Add(V.Nome)
          Continue For
        End If
        Dim N As Integer = System.Math.Min(CanaleX.Valori.Length, System.Math.Min(ChY.Valori.Length, ChTws.Valori.Length))
        If RigheCamp Is Nothing Then RigheCamp = RigheSorgente(S.Sorgente, N).ToList
        CanaliY(V.Id) = ChY
        Risultati(V.Id) = clsFeedForward.Calcola(CanaleX.Valori, ChY.Valori, ChTws.Valori, RigheCamp, Maschera, S.Parametri)
        ' selezione delle fasce: si conserva quella esistente (solo fasce ancora valide), altrimenti la prima con ottimo
        Dim Sel As HashSet(Of Double) = Nothing
        If Selezione.TryGetValue(V.Id, Sel) Then
          Sel.RemoveWhere(Function(t) Not Risultati(V.Id).Any(Function(f) f.TwsLow = t AndAlso f.Ottimo IsNot Nothing))
        Else
          Sel = New HashSet(Of Double)
          Dim Pr As clsFfFascia = Risultati(V.Id).FirstOrDefault(Function(f) f.Ottimo IsNot Nothing)
          If Pr IsNot Nothing Then Sel.Add(Pr.TwsLow)
          Selezione(V.Id) = Sel
        End If
      Next
      txt_Stato.Text = If(RigheCamp Is Nothing, 0, RigheCamp.Count).ToString("N0") & " rows in the source, " & Risultati.Count & " Y channels; " & DescrizioneFiltro & ". Goal: " & cmb_Obiettivo.Text & "; model: " & cmb_Modello.Text & "." &
                       If(Mancanti.Count > 0, " No data for: " & String.Join(", ", Mancanti) & ".", "")
      Return Risultati.Count > 0
    Catch ex As Exception
      MsgBox("Error: " & ex.Message, MsgBoxStyle.Exclamation, "Feed Forward")
      Return False
    End Try
  End Function

  Private Sub btn_Calcola_Click(sender As Object, e As RoutedEventArgs)
    If Not EseguiCalcolo() Then Exit Sub
    If lst_Y.SelectedItem Is Nothing AndAlso VociY.Count > 0 Then
      lst_Y.SelectedIndex = 0 ' la selezione mostra il canale
    Else
      MostraY(TryCast(lst_Y.SelectedItem, clsVoceY))
    End If
  End Sub

  ' ---------------------------------------------------------------- tabella

  Private Function Dec(V As Double, Canale As clsChannel2020) As String
    If Double.IsNaN(V) Then Return ""
    ' i decimali del canale, come nel resto dell'applicazione
    Dim D As Integer = If(Canale Is Nothing, 2, System.Math.Max(0, System.Math.Min(6, Canale.Decimals)))
    Return V.ToString("F" & D)
  End Function

  ''' <summary>Mostra tabella e grafico del canale Y scelto (se calcolato).</summary>
  Private Sub MostraY(V As clsVoceY)
    Righe = New List(Of clsFfRiga)
    CanaleYVista = Nothing
    Dim Lista As List(Of clsFfFascia) = Nothing
    If V IsNot Nothing AndAlso Risultati.TryGetValue(V.Id, Lista) Then
      CanaleYVista = CanaliY(V.Id)
      For Each F As clsFfFascia In Lista
        Righe.Add(RigaTabella(F, CanaleYVista))
      Next
    End If
    AggiornaColonneBin(Lista)
    ' all'inizio e' visibile solo la prima fascia con ottimo (le altre si abilitano con la checkbox)
    Dim Prima As clsFfRiga = Righe.FirstOrDefault(Function(r) r.Dati.Ottimo IsNot Nothing)
    If Prima Is Nothing Then Prima = Righe.FirstOrDefault(Function(r) r.Campioni > 0)
    If Prima Is Nothing Then Prima = Righe.FirstOrDefault
    Dim SelY As HashSet(Of Double) = Nothing
    If V IsNot Nothing AndAlso Selezione.TryGetValue(V.Id, SelY) Then
      For Each R As clsFfRiga In Righe
        R.Visibile = SelY.Contains(R.Dati.TwsLow)
      Next
      If Not Righe.Any(Function(r) r.Visibile) AndAlso Prima IsNot Nothing Then Prima.Visibile = False
      Dim PrimaVis As clsFfRiga = Righe.FirstOrDefault(Function(r) r.Visibile)
      If PrimaVis IsNot Nothing Then Prima = PrimaVis
    ElseIf Prima IsNot Nothing Then
      Prima.Visibile = True
    End If
    grd_Fasce.ItemsSource = Righe
    grd_Fasce.SelectedItem = Prima
    RidisegnaVisibili()
  End Sub

  Private ColonneBin As New List(Of DataGridColumn)
  ''' <summary>Bin con colonna nella tabella: indice e intestazione (stesso ordine delle colonne).</summary>
  Private IntestazioniBin As New List(Of KeyValuePair(Of Integer, String))

  ''' <summary>Una colonna per ogni bin di X con la media di Y della fascia (prima della colonna Note); si ricostruiscono a ogni canale mostrato.</summary>
  Private Sub AggiornaColonneBin(Lista As List(Of clsFfFascia))
    For Each C As DataGridColumn In ColonneBin
      grd_Fasce.Columns.Remove(C)
    Next
    ColonneBin.Clear()
    IntestazioniBin.Clear()
    If Lista Is Nothing OrElse CanaleX Is Nothing OrElse CanaleYVista Is Nothing Then Exit Sub
    Dim Bins As New SortedDictionary(Of Integer, clsFfBin)
    For Each F As clsFfFascia In Lista
      For Each B As clsFfBin In F.Bins
        If B.N > 0 AndAlso Not Bins.ContainsKey(B.Indice) Then Bins(B.Indice) = B
      Next
    Next
    For Each R As clsFfRiga In Righe
      For Each Idx As Integer In Bins.Keys
        Dim Ix As Integer = Idx
        Dim B As clsFfBin = R.Dati.Bins.FirstOrDefault(Function(q) q.Indice = Ix AndAlso q.N > 0)
        R.Medie(Idx) = If(B Is Nothing, "", Dec(B.YMedio, CanaleYVista))
        R.MedieUsate(Idx) = B Is Nothing OrElse B.UsatoNelFit
      Next
    Next
    Dim Pos As Integer = System.Math.Max(0, grd_Fasce.Columns.Count - 1) ' prima di Note
    For Each kv As KeyValuePair(Of Integer, clsFfBin) In Bins
      Dim Col As New DataGridTextColumn With {.Header = "mean " & kv.Value.XLow.ToString("0.##") & "-" & kv.Value.XHigh.ToString("0.##"),
                                              .Binding = New Binding("Medie[" & kv.Key & "]")}
      Col.ElementStyle = New Style(GetType(TextBlock))
      Col.ElementStyle.Setters.Add(New Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right))
      ' valori non usati nel fit in grigio
      Dim Tr As New DataTrigger With {.Binding = New Binding("MedieUsate[" & kv.Key & "]"), .Value = False}
      Tr.Setters.Add(New Setter(TextBlock.ForegroundProperty, Brushes.Gray))
      Col.ElementStyle.Triggers.Add(Tr)
      grd_Fasce.Columns.Insert(Pos, Col)
      ColonneBin.Add(Col)
      IntestazioniBin.Add(New KeyValuePair(Of Integer, String)(kv.Key, CStr(Col.Header)))
      Pos += 1
    Next
  End Sub

  ''' <summary>Disegna le fasce con la checkbox attiva; il testo sotto il grafico riguarda la riga selezionata.</summary>
  Private Sub RidisegnaVisibili()
    Dim Sel As clsFfRiga = TryCast(grd_Fasce.SelectedItem, clsFfRiga)
    DisegnaFasce(Righe.Where(Function(r) r.Visibile).Select(Function(r) r.Dati).ToList, CanaleYVista, If(Sel Is Nothing, Nothing, Sel.Dati))
  End Sub

  ''' <summary>Riporta le checkbox della tabella nella selezione del canale Y mostrato.</summary>
  Private Sub SalvaSelezione()
    Dim V As clsVoceY = TryCast(lst_Y.SelectedItem, clsVoceY)
    If V Is Nothing Then Exit Sub
    Selezione(V.Id) = New HashSet(Of Double)(Righe.Where(Function(r) r.Visibile).Select(Function(r) r.Dati.TwsLow))
  End Sub

  Private Sub chk_Fascia_Click(sender As Object, e As RoutedEventArgs)
    SalvaSelezione()
    RidisegnaVisibili()
  End Sub

  Private Sub btn_TuttiCanali_Click(sender As Object, e As RoutedEventArgs)
    For Each kv As KeyValuePair(Of String, List(Of clsFfFascia)) In Risultati
      Selezione(kv.Key) = New HashSet(Of Double)(kv.Value.Where(Function(f) f.Ottimo IsNot Nothing).Select(Function(f) f.TwsLow))
    Next
    MostraY(TryCast(lst_Y.SelectedItem, clsVoceY))
  End Sub

  Private Sub btn_TutteFasce_Click(sender As Object, e As RoutedEventArgs)
    For Each R As clsFfRiga In Righe
      R.Visibile = R.Dati.Ottimo IsNot Nothing
    Next
    SalvaSelezione()
    grd_Fasce.Items.Refresh()
    RidisegnaVisibili()
  End Sub

  ''' <summary>Toglie la riga evidenziata: nel grafico tutte le fasce visibili hanno lo stesso peso.</summary>
  Private Sub btn_NoEvidenza_Click(sender As Object, e As RoutedEventArgs)
    grd_Fasce.UnselectAll()
    grd_Fasce.SelectedItem = Nothing
    RidisegnaVisibili()
  End Sub

  Private Sub btn_NessunaFascia_Click(sender As Object, e As RoutedEventArgs)
    For Each R As clsFfRiga In Righe
      R.Visibile = False
    Next
    SalvaSelezione()
    grd_Fasce.Items.Refresh()
    RidisegnaVisibili()
  End Sub

  Private Function RigaTabella(F As clsFfFascia, ChY As clsChannel2020) As clsFfRiga
    Dim R As New clsFfRiga With {.Fascia = F.TwsLow.ToString("0.#") & " - " & F.TwsHigh.ToString("0.#"), .Campioni = F.Campioni, .Dati = F}
    R.Bin = F.Bins.Where(Function(b) b.UsatoNelFit).Count & " / " & F.Bins.Count
    If F.Ottimo Is Nothing Then
      R.Nota = F.Messaggio
    Else
      Dim O As clsFfOttimo = F.Ottimo
      R.OttimoX = Dec(O.X, CanaleX)
      R.OttimoY = Dec(O.Y, ChY)
      R.Errore = If(Double.IsNaN(O.ErroreStd), "", Dec(2 * O.ErroreStd, ChY))
      R.MigliorX = Dec(O.XMigliorBin, CanaleX)
      R.MigliorY = Dec(O.YMigliorBin, ChY)
      R.Guadagno = Dec(O.Y - O.YMigliorBin, ChY)
      R.R2 = If(Double.IsNaN(O.R2), "", O.R2.ToString("0.00"))
      R.Estrapolato = If(O.Estrapolato, "yes", "")
      R.Affidabilita = O.Affidabilita
      R.Associazione = TestoAssoc(O.AssociazioneDelta, O.Associazione, ChY)
    End If
    Return R
  End Function

  Private Sub btn_Copia_Click(sender As Object, e As RoutedEventArgs)
    If Righe.Count = 0 Then Exit Sub
    Dim Sb As New System.Text.StringBuilder
    Sb.AppendLine(If(CanaleYVista Is Nothing, "", CanaleYVista.LongName))
    Dim Testa As New List(Of String) From {"Band", "Samples", "Bins", "Target X", "Expected Y", "+-2 SE", "Best bin X", "Best bin Y", "Gain vs best bin", "R2", "Extrapolated", "Reliability", "Association"}
    Testa.AddRange(IntestazioniBin.Select(Function(q) q.Value))
    Testa.Add("Note")
    Sb.AppendLine(String.Join(vbTab, Testa))
    For Each R As clsFfRiga In Righe
      Dim Cel As New List(Of String) From {R.Fascia, R.Campioni.ToString, R.Bin, R.OttimoX, R.OttimoY, R.Errore, R.MigliorX, R.MigliorY, R.Guadagno, R.R2, R.Estrapolato, R.Affidabilita, R.Associazione}
      For Each Kb As KeyValuePair(Of Integer, String) In IntestazioniBin
        Dim Mv As String = Nothing
        Dim Us As Boolean = True
        R.MedieUsate.TryGetValue(Kb.Key, Us)
        Cel.Add(If(R.Medie.TryGetValue(Kb.Key, Mv), If(Us OrElse Mv = "", Mv, "(" & Mv & ")"), "")) ' nel testo copiato non c'e' il grigio: i non usati sono tra parentesi
      Next
      Cel.Add(R.Nota)
      Sb.AppendLine(String.Join(vbTab, Cel))
    Next
    Try
      Clipboard.SetText(Sb.ToString)
      txt_Stato.Text = "Table copied to the clipboard."
    Catch ex As Exception
      txt_Stato.Text = "Cannot copy: " & ex.Message
    End Try
  End Sub

  Private Sub grd_Fasce_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    RidisegnaVisibili()
  End Sub

  ' ---------------------------------------------------------------- grafico

  ''' <summary>Colore della fascia (palette a colori distinti, ciclica): le fasce si distinguono per colore, i bin dalle bande verticali dell'asse.</summary>
  Private Shared Function ColoreFascia(Indice As Integer, Alpha As Byte) As Color
    Static Palette() As Color = {Color.FromRgb(31, 119, 180), Color.FromRgb(214, 39, 40), Color.FromRgb(44, 160, 44), Color.FromRgb(255, 127, 14), Color.FromRgb(148, 103, 189),
                                 Color.FromRgb(23, 190, 207), Color.FromRgb(188, 189, 34), Color.FromRgb(227, 119, 194), Color.FromRgb(140, 86, 75), Color.FromRgb(90, 90, 90)}
    Dim C As Color = Palette(System.Math.Abs(Indice) Mod Palette.Length)
    Return Color.FromArgb(Alpha, C.R, C.G, C.B)
  End Function

  Private Function MarkerPunto(Diametro As Integer, Riempimento As Color, Contorno As Color) As EllipsePointMarker
    Dim M As New EllipsePointMarker
    M.Width = Diametro
    M.Height = Diametro
    M.Fill = Riempimento
    M.Stroke = Contorno
    Return M
  End Function

  ''' <summary>Fasce del canale Y mostrato (tutte, anche quelle non visibili): servono per avere i colori dei bin comuni.</summary>
  Private Function TutteLeFasce(ChY As clsChannel2020) As List(Of clsFfFascia)
    For Each kv As KeyValuePair(Of String, clsChannel2020) In CanaliY
      If kv.Value Is ChY AndAlso Risultati.ContainsKey(kv.Key) Then Return Risultati(kv.Key)
    Next
    Return Nothing
  End Function

  ''' <summary>Disegna una sola fascia (anche per l'immagine del PDF).</summary>
  Private Sub DisegnaFascia(F As clsFfFascia, ChY As clsChannel2020)
    DisegnaFasce(If(F Is Nothing, New List(Of clsFfFascia), New List(Of clsFfFascia) From {F}), ChY, F)
  End Sub

  ''' <summary>Disegna insieme le fasce indicate: colori dei bin comuni a tutte; il testo sotto il grafico riguarda la fascia Dettaglio.</summary>
  Private Sub DisegnaFasce(Fasce As List(Of clsFfFascia), ChY As clsChannel2020, Dettaglio As clsFfFascia)
    Plot.RenderableSeries.Clear()
    Plot.Annotations.Clear()
    pnl_Legenda.Children.Clear()
    txt_Ottimo.Text = ""
    If Fasce Is Nothing OrElse CanaleX Is Nothing OrElse ChY Is Nothing Then Exit Sub
    Fasce = Fasce.Where(Function(f) f.Campioni > 0).ToList
    If Fasce.Count = 0 Then Exit Sub
    Dim S As clsFeedForwardReport = Rep()

    AsseX.AxisTitle = ""
    AsseY.AxisTitle = ""
    AsseX.TextFormatting = FormatoAsse(CanaleX, S.DecimaliAssiExtra)
    AsseY.TextFormatting = FormatoAsse(ChY, S.DecimaliAssiExtra)
    AsseX.FontSize = S.ValoriAssiDimensione
    AsseY.FontSize = S.ValoriAssiDimensione
    AggiungiNomiAssi(CanaleX, ChY)

    ' ogni fascia ha un suo colore, fisso (posizione nell'elenco di tutte le fasce del canale), anche se se ne mostrano solo alcune
    Dim Tutte As List(Of clsFfFascia) = TutteLeFasce(ChY)
    If Tutte Is Nothing Then Tutte = Fasce.ToList
    Dim Ordinate As List(Of clsFfFascia) = Fasce.ToList

    Const MaxPuntiPerBin As Integer = 2500
    ' con piu' fasce la fascia selezionata (Dettaglio) e' disegnata per ultima e in evidenza, le altre attenuate
    Dim Multi As Boolean = Fasce.Count > 1 AndAlso Dettaglio IsNot Nothing AndAlso Fasce.Contains(Dettaglio)
    If Multi Then Fasce = Fasce.Where(Function(f) f IsNot Dettaglio).Concat({Dettaglio}).ToList
    For Each F As clsFfFascia In Fasce
      Dim Ev As Boolean = Multi AndAlso F Is Dettaglio
      Dim Att As Boolean = Multi AndAlso Not Ev
      Dim Bi As Integer = Tutte.IndexOf(F)
      If Bi < 0 Then Bi = Ordinate.IndexOf(F)
      Dim ColPieno As Color = ColoreFascia(Bi, 255)
      ' nuvola: una serie per bin, decimata per tenere il disegno leggero
      Dim PerBin As New Dictionary(Of Integer, List(Of Integer))
      For i As Integer = 0 To F.PuntiBin.Count - 1
        Dim L As List(Of Integer) = Nothing
        If Not PerBin.TryGetValue(F.PuntiBin(i), L) Then
          L = New List(Of Integer)
          PerBin(F.PuntiBin(i)) = L
        End If
        L.Add(i)
      Next
      For Each b As clsFfBin In F.Bins
        Dim Idx As List(Of Integer) = Nothing
        If Not PerBin.TryGetValue(b.Indice, Idx) Then Continue For
        Dim Passo As Integer = System.Math.Max(1, CInt(System.Math.Ceiling(Idx.Count / CDbl(MaxPuntiPerBin))))
        Dim xs As New List(Of Double), ys As New List(Of Double)
        For k As Integer = 0 To Idx.Count - 1 Step Passo
          xs.Add(F.PuntiX(Idx(k)))
          ys.Add(F.PuntiY(Idx(k)))
        Next
        Dim Ds As New XyDataSeries(Of Double, Double) With {.AcceptsUnsortedData = True}
        Ds.Append(xs, ys)
        Dim Col As Color = ColoreFascia(Bi, CByte(255 * System.Math.Min(100, If(Ev, S.PuntiOpacita * 1.6, If(Att, S.PuntiOpacita * 0.45, S.PuntiOpacita))) / 100))
        Dim Sr As New XyScatterRenderableSeries
        Sr.DataSeries = Ds
        Sr.PointMarker = MarkerPunto(If(Ev, S.PuntiDimensione + 2, S.PuntiDimensione), Col, Col)
        Plot.RenderableSeries.Add(Sr)
      Next

      ' medie dei bin: piene se entrano nel fit, vuote se no
      For Each Usato As Boolean In {False, True}
        Dim Bs As List(Of clsFfBin) = F.Bins.Where(Function(b) b.UsatoNelFit = Usato).ToList
        If Bs.Count = 0 Then Continue For
        Dim Ds As New XyDataSeries(Of Double, Double) With {.AcceptsUnsortedData = True}
        Ds.Append(Bs.Select(Function(b) b.XMedio).ToList, Bs.Select(Function(b) b.YMedio).ToList)
        Dim Sr As New XyScatterRenderableSeries
        Sr.DataSeries = Ds
        Dim DimM As Integer = If(Ev, S.MedieDimensione + 4, If(Att, System.Math.Max(3, S.MedieDimensione - 3), S.MedieDimensione))
        Sr.PointMarker = If(Usato, MarkerPunto(DimM, ColPieno, Colors.Black), MarkerPunto(System.Math.Max(1, DimM - 2), Colors.Transparent, ColPieno))
        Plot.RenderableSeries.Add(Sr)
      Next

      ' curva del modello: la parte oltre l'ultimo bin usato (estrapolata) tratteggiata
      If F.CurvaX.Count > 1 AndAlso F.Bins.Any(Function(b) b.UsatoNelFit) Then
        Dim xUltimo As Double = F.Bins.Where(Function(b) b.UsatoNelFit).Max(Function(b) b.XMedio)
        Dim Dentro As New List(Of Integer), Fuori As New List(Of Integer)
        For i As Integer = 0 To F.CurvaX.Count - 1
          If F.CurvaX(i) <= xUltimo + 0.000000001 Then Dentro.Add(i) Else Fuori.Add(i)
        Next
        ' il tratto estrapolato riparte dall'ultimo punto interno, cosi' la linea e' continua
        If Fuori.Count > 0 AndAlso Dentro.Count > 0 Then Fuori.Insert(0, Dentro.Last)
        For Gruppo As Integer = 0 To 1
          Dim Ix As List(Of Integer) = If(Gruppo = 0, Dentro, Fuori)
          If Ix.Count < 2 Then Continue For
          Dim Ds As New XyDataSeries(Of Double, Double) With {.AcceptsUnsortedData = True}
          Ds.Append(Ix.Select(Function(i) F.CurvaX(i)).ToList, Ix.Select(Function(i) F.CurvaY(i)).ToList)
          Dim L As New FastLineRenderableSeries
          L.DataSeries = Ds
          L.Stroke = If(Att, ColoreFascia(Bi, 150), ColPieno)
          If Gruppo = 1 Then L.StrokeDashArray = New Double() {5, 4} ' tratto estrapolato tratteggiato
          L.StrokeThickness = If(Ev, 4, If(Att, 1, 2))
          Plot.RenderableSeries.Add(L)
        Next
      End If

      ' ottimo suggerito
      If F.Ottimo IsNot Nothing Then
        Dim Ds As New XyDataSeries(Of Double, Double) With {.AcceptsUnsortedData = True}
        Ds.Append(New List(Of Double) From {F.Ottimo.X}, New List(Of Double) From {F.Ottimo.Y})
        Dim Sr As New XyScatterRenderableSeries
        Sr.DataSeries = Ds
        Sr.PointMarker = MarkerPunto(If(Ev, 24, If(Att, 12, 18)), Colors.Gold, ColPieno)
        Plot.RenderableSeries.Add(Sr)
      End If
    Next

    ' legenda: una voce per fascia disegnata, col suo colore
    For Each F As clsFfFascia In Ordinate.OrderBy(Function(q) q.TwsLow)
      Dim Bi As Integer = Tutte.IndexOf(F)
      If Bi < 0 Then Bi = Ordinate.IndexOf(F)
      Dim Et As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 10, 0)}
      Et.Children.Add(New Border With {.Width = 12, .Height = 12, .Background = New SolidColorBrush(ColoreFascia(Bi, 255)), .Margin = New Thickness(0, 0, 3, 0)})
      Et.Children.Add(New TextBlock With {.Text = F.TwsLow.ToString("0.#") & "-" & F.TwsHigh.ToString("0.#") & " (n=" & F.Campioni.ToString("N0") & ")", .FontSize = 11})
      pnl_Legenda.Children.Add(Et)
    Next

    ' titolo e testo: una sola fascia = dettaglio dell'ottimo; piu' fasce = elenco delle fasce, il dettaglio e' quello della riga selezionata
    Dim Bande As String = String.Join(", ", Fasce.Select(Function(f) f.TwsLow.ToString("0.#") & "-" & f.TwsHigh.ToString("0.#")))
    Dim Titolo As String
    If Fasce.Count = 1 Then
      Titolo = ChY.ShortName & " - band " & Bande & "   (n=" & Fasce(0).Campioni.ToString("N0") & ")"
    Else
      Titolo = ChY.ShortName & " - bands " & Bande
    End If
    Dim D As clsFfFascia = If(Dettaglio IsNot Nothing, Dettaglio, Fasce(0))
    Dim TitoloD As String = ChY.ShortName & " - band " & D.TwsLow.ToString("0.#") & " - " & D.TwsHigh.ToString("0.#") & "   (n=" & D.Campioni.ToString("N0") & ")"
    If D.Ottimo IsNot Nothing Then
      Dim O As clsFfOttimo = D.Ottimo
      Dim Riga1 As String = "target " & CanaleX.ShortName & " = " & Dec(O.X, CanaleX) & "  ->  " & ChY.ShortName & " = " & Dec(O.Y, ChY) &
                            If(Double.IsNaN(O.ErroreStd), "", " (±" & Dec(2 * O.ErroreStd, ChY) & ")")
      Dim Riga2 As String = "best observed bin: " & Dec(O.XMigliorBin, CanaleX) & " -> " & Dec(O.YMigliorBin, ChY) &
                            "   reliability: " & O.Affidabilita & If(O.Estrapolato, " (extrapolated)", "")
      txt_Ottimo.Text = TitoloD & ":  " & Riga1 & "   " & Riga2
      If Fasce.Count = 1 Then Titolo &= vbCrLf & Riga1 & vbCrLf & Riga2
    Else
      txt_Ottimo.Text = TitoloD & ": " & D.Messaggio
      If Fasce.Count = 1 Then Titolo &= vbCrLf & D.Messaggio
    End If
    AggiungiTitolo(Titolo)
    Plot.ZoomExtents()
  End Sub

  ''' <summary>Titolo dentro il grafico (finisce anche nell'immagine del PDF).</summary>
  ''' <summary>Formato dei numeri dell'asse: i decimali noti del canale.</summary>
  Private Shared Function FormatoAsse(Ch As clsChannel2020, Extra As Integer) As String
    Dim D As Integer = If(Ch Is Nothing, 1, System.Math.Max(0, System.Math.Min(5, Ch.Decimals + Extra)))
    Return If(D = 0, "0", "0." & New String("0"c, D))
  End Function

  Private Shared Function NomeConUnita(Ch As clsChannel2020) As String
    Dim Um As String = If(String.IsNullOrWhiteSpace(Ch.ShortUM), If(String.IsNullOrWhiteSpace(Ch.LongUM), "", Ch.LongUM), Ch.ShortUM)
    Return Ch.LongName & If(Um = "", "", "  [" & Um & "]")
  End Function

  ''' <summary>Nomi degli assi in grande, semitrasparenti, sullo sfondo del grafico (sotto le serie): si capisce subito quale e' X e quale Y.</summary>
  Private Sub AggiungiNomiAssi(ChX As clsChannel2020, ChY As clsChannel2020)
    Try
      For Each Asse As Integer In {0, 1}
        Dim Tb As New TextBlock
        Tb.Text = NomeConUnita(If(Asse = 0, ChX, ChY))
        ' la dimensione si riduce se il nome non entra nel grafico (altrimenti verrebbe tagliato)
        Dim Lung As Double = If(Asse = 0, Plot.ActualWidth, Plot.ActualHeight) * 0.85
        Dim Dim0 As Double = Rep().NomiAssiDimensione
        If Lung > 50 AndAlso Tb.Text.Length > 0 Then Dim0 = System.Math.Min(Dim0, Lung / (Tb.Text.Length * 0.65))
        Tb.FontSize = System.Math.Max(8, Dim0)
        Tb.FontWeight = FontWeights.Bold
        Tb.Foreground = New SolidColorBrush(Color.FromArgb(CByte(255 * Rep().NomiAssiOpacita / 100), 70, 90, 120))
        Tb.IsHitTestVisible = False
        ' contenitore di dimensione esplicita: col solo testo ruotato l'ancoraggio usava la larghezza del testo non ruotato
        ' e il nome di Y finiva spostato verso il centro del grafico
        Tb.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
        Dim Lt As Double = Tb.DesiredSize.Width
        Dim At As Double = Tb.DesiredSize.Height
        Dim Contenitore As New Grid With {.IsHitTestVisible = False}
        If Asse = 1 Then
          Tb.LayoutTransform = New RotateTransform(-90)
          Contenitore.Width = At
          Contenitore.Height = Lt
        Else
          Contenitore.Width = Lt
          Contenitore.Height = At
        End If
        Contenitore.Children.Add(Tb)
        Dim A As New CustomAnnotation
        A.Content = Contenitore
        A.Width = Contenitore.Width
        A.Height = Contenitore.Height
        ' coordinate sui dati (non relative): la posizione si calcola dai bordi visibili degli assi, vedi PosizionaNomiAssi
        A.CoordinateMode = AnnotationCoordinateMode.Absolute
        A.X1 = 0.0
        A.Y1 = 0.0
        A.HorizontalAnchorPoint = If(Asse = 0, HorizontalAnchorPoint.Center, HorizontalAnchorPoint.Right)
        A.VerticalAnchorPoint = If(Asse = 0, VerticalAnchorPoint.Bottom, VerticalAnchorPoint.Center)
        A.AnnotationCanvas = AnnotationCanvas.BelowChart
        Plot.Annotations.Add(A)
        If Asse = 0 Then NomeAssiX = A Else NomeAssiY = A
      Next
      If Not NomiAssiAgganciati Then
        NomiAssiAgganciati = True
        AddHandler AsseX.VisibleRangeChanged, AddressOf RangeAssiCambiato
        AddHandler AsseY.VisibleRangeChanged, AddressOf RangeAssiCambiato
      End If
      PosizionaNomiAssi()
    Catch ex As Exception
      Console.WriteLine("FeedForward: nomi degli assi: " & ex.Message)
    End Try
  End Sub

  Private NomeAssiX As CustomAnnotation = Nothing
  Private NomeAssiY As CustomAnnotation = Nothing
  Private NomiAssiAgganciati As Boolean = False

  Private Sub RangeAssiCambiato(sender As Object, e As EventArgs)
    PosizionaNomiAssi()
  End Sub

  ''' <summary>Il nome di X sta al centro del bordo inferiore dell'area dati, quello di Y al centro del bordo destro (dove ci sono i suoi numeri), anche dopo zoom e pan.</summary>
  Private Sub PosizionaNomiAssi()
    Try
      If NomeAssiX Is Nothing OrElse NomeAssiY Is Nothing Then Exit Sub
      Dim Rx As SciChart.Data.Model.IRange = AsseX.VisibleRange
      Dim Ry As SciChart.Data.Model.IRange = AsseY.VisibleRange
      If Rx Is Nothing OrElse Ry Is Nothing Then Exit Sub
      Dim x0 As Double = CDbl(Rx.Min), x1 As Double = CDbl(Rx.Max)
      Dim y0 As Double = CDbl(Ry.Min), y1 As Double = CDbl(Ry.Max)
      NomeAssiX.X1 = (x0 + x1) / 2
      NomeAssiX.Y1 = y0
      NomeAssiY.X1 = x1
      NomeAssiY.Y1 = (y0 + y1) / 2
    Catch ex As Exception
      Console.WriteLine("FeedForward: posizione dei nomi degli assi: " & ex.Message)
    End Try
  End Sub

  Private Sub AggiungiTitolo(Testo As String)
    Try
      Dim T As New TextAnnotation
      T.Text = Testo
      T.CoordinateMode = AnnotationCoordinateMode.Relative
      T.X1 = 0.01
      T.Y1 = 0.01
      T.HorizontalAnchorPoint = HorizontalAnchorPoint.Left
      T.VerticalAnchorPoint = VerticalAnchorPoint.Top
      T.FontSize = Rep().TitoloDimensione
      T.Foreground = Brushes.Black
      Plot.Annotations.Add(T)
    Catch ex As Exception
      Console.WriteLine("FeedForward: titolo del grafico: " & ex.Message)
    End Try
  End Sub

  ' ---------------------------------------------------------------- tabelle del PDF (solo fasce con dati sufficienti)

  ''' <summary>Tabella riepilogativa: una riga per fascia di vento, per ogni canale Y target X, Y attesa e affidabilita'.</summary>
  Private Function TabellaRiepilogo(Riga1 As String, Riga2 As String) As clsFfPdfTabella
    Dim T As New clsFfPdfTabella With {.Titolo = ""}
    T.Note.Add("#" & Riga1)
    T.Note.Add("#" & Riga2)
    Dim Canali As List(Of clsVoceY) = VociY.Where(Function(v) Risultati.ContainsKey(v.Id)).ToList
    If Canali.Count = 0 Then Return T
    T.Intestazioni.Add("Band")
    T.Intestazioni.Add("Samples")
    T.Gruppi.Add(New KeyValuePair(Of String, Integer)("", 2))
    T.Larghezze.Add(1.4)
    T.Larghezze.Add(1.0)
    For Each V As clsVoceY In Canali
      Dim Ch As clsChannel2020 = CanaliY(V.Id)
      T.Gruppi.Add(New KeyValuePair(Of String, Integer)(Ch.ShortName, 3))
      T.Intestazioni.Add("Target " & CanaleX.ShortName)
      T.Intestazioni.Add("Exp. Y")
      T.Intestazioni.Add("Rel.")
      T.Larghezze.Add(1.3)
      T.Larghezze.Add(1.1)
      T.Larghezze.Add(0.6)
    Next
    Dim NFasce As Integer = Risultati(Canali(0).Id).Count
    For b As Integer = 0 To NFasce - 1
      Dim Fasce As List(Of clsFfFascia) = Canali.Select(Function(v) Risultati(v.Id)(b)).ToList
      If Not Fasce.Any(Function(f) f.Ottimo IsNot Nothing) Then Continue For
      Dim Cella As New List(Of String)
      Cella.Add(Fasce(0).TwsLow.ToString("0.#") & " - " & Fasce(0).TwsHigh.ToString("0.#"))
      Cella.Add(Fasce.Max(Function(f) f.Campioni).ToString("N0"))
      For i As Integer = 0 To Canali.Count - 1
        Dim O As clsFfOttimo = Fasce(i).Ottimo
        If O Is Nothing Then
          Cella.Add("-")
          Cella.Add("-")
          Cella.Add("-")
        Else
          Cella.Add(Dec(O.X, CanaleX) & If(O.Estrapolato, " *", ""))
          Cella.Add(Dec(O.Y, CanaliY(Canali(i).Id)))
          Cella.Add(O.Affidabilita.Substring(0, 1))
        End If
      Next
      T.Righe.Add(Cella.ToArray)
    Next
    T.Note.Add("* extrapolated beyond the last bin with data.   Rel.: reliability H = high, M = medium, L = low.   '-' = not enough data for that channel.")
    Return T
  End Function

  ''' <summary>
  ''' Riepilogo del PDF "insieme": una colonna per fascia (quelle stampate in almeno un canale); in alto il bersaglio di X e i campioni,
  ''' poi una riga per canale Y con il valore suggerito dal feed forward. Grassetto = affidabilita' alta, normale = media, grigio = bassa.
  ''' Se in una fascia il bersaglio di un canale e' diverso da quello della riga in alto (per esempio cercando il massimo di Y), il valore riporta "@ X".
  ''' </summary>
  ''' <summary>X di riferimento come testo, in percentuale (la X da spingere e' una prestazione percentuale, per esempio Vmg%).</summary>
  Private Function TestoRiferimento() As String
    Return Testo(Rep().XRiferimento) & "%"
  End Function

  ''' <summary>Media di Y osservata a X = Xr: interpolazione lineare tra le medie dei due bin usati nel fit che la racchiudono (NaN se Xr e' fuori dai bin usati).</summary>
  Private Shared Function MediaA(F As clsFfFascia, Xr As Double) As Double
    Dim Bs As List(Of clsFfBin) = F.Bins.Where(Function(b) b.UsatoNelFit).OrderBy(Function(b) b.XMedio).ToList
    If Bs.Count = 0 Then Return Double.NaN
    If Xr < Bs.First.XLow OrElse Xr > Bs.Last.XHigh Then Return Double.NaN
    If Xr <= Bs.First.XMedio Then Return Bs.First.YMedio
    If Xr >= Bs.Last.XMedio Then Return Bs.Last.YMedio
    For i As Integer = 0 To Bs.Count - 2
      If Xr >= Bs(i).XMedio AndAlso Xr <= Bs(i + 1).XMedio Then
        Dim t As Double = (Xr - Bs(i).XMedio) / (Bs(i + 1).XMedio - Bs(i).XMedio)
        Return Bs(i).YMedio + t * (Bs(i + 1).YMedio - Bs(i).YMedio)
      End If
    Next
    Return Double.NaN
  End Function

  ''' <summary>Associazione come testo: variazione del canale nelle sue unita' (con segno) e, tra parentesi, quanto e' netta.</summary>
  Private Function TestoAssoc(Delta As Double, A As Double, ChY As clsChannel2020) As String
    If Double.IsNaN(Delta) OrElse Double.IsNaN(A) Then Return ""
    Dim Um As String = If(String.IsNullOrWhiteSpace(ChY.ShortUM), "", " " & ChY.ShortUM)
    Return If(Delta >= 0, "+", "") & Dec(Delta, ChY) & Um & "  (" & clsFeedForward.ClasseAssociazione(A) & ")"
  End Function

  Private Function TabellaRiepilogoInsieme(Canali As List(Of KeyValuePair(Of clsChannel2020, List(Of clsFfFascia)))) As clsFfPdfTabella
    Dim T As New clsFfPdfTabella
    Dim ChBanda As clsChannel2020 = CanaleDaId(Rep().TwsChannelId)
    Dim NomeBanda As String = If(ChBanda Is Nothing, "Band", ChBanda.ShortName)
    ' fasce: unione di quelle stampate, in ordine
    Dim Bande As New SortedDictionary(Of Double, Double)
    For Each kv As KeyValuePair(Of clsChannel2020, List(Of clsFfFascia)) In Canali
      For Each F As clsFfFascia In kv.Value
        If Not Bande.ContainsKey(F.TwsLow) Then Bande(F.TwsLow) = F.TwsHigh
      Next
    Next

    T.Intestazioni.Add("")
    T.Larghezze.Add(2.6)
    For Each B As KeyValuePair(Of Double, Double) In Bande
      T.Intestazioni.Add(B.Key.ToString("0.#") & " - " & B.Value.ToString("0.#"))
      T.Larghezze.Add(1.0)
    Next
    T.Gruppi.Add(New KeyValuePair(Of String, Integer)("", 1))
    T.Gruppi.Add(New KeyValuePair(Of String, Integer)(NomeBanda & " bands", Bande.Count))

    ' bersaglio di X per fascia: il piu' frequente tra i canali (con "spingi X" e' lo stesso per tutti)
    Dim Bersaglio As New Dictionary(Of Double, Double)
    Dim Campioni As New Dictionary(Of Double, Integer)
    For Each B As Double In Bande.Keys
      Dim Bk As Double = B
      Dim Ottimi As List(Of clsFfFascia) = Canali.Select(Function(kv) kv.Value.FirstOrDefault(Function(f) f.TwsLow = Bk)).Where(Function(f) f IsNot Nothing AndAlso f.Ottimo IsNot Nothing).ToList
      If Ottimi.Count = 0 Then Continue For
      Bersaglio(B) = Ottimi.GroupBy(Function(f) Dec(f.Ottimo.X, CanaleX)).OrderByDescending(Function(g) g.Count).First.First.Ottimo.X
      Campioni(B) = Ottimi.Max(Function(f) f.Campioni)
    Next
    Dim Rt As New List(Of String) From {"FF target " & CanaleX.ShortName}
    Dim Rc As New List(Of String) From {"Samples"}
    For Each B As Double In Bande.Keys
      Rt.Add(If(Bersaglio.ContainsKey(B), Dec(Bersaglio(B), CanaleX) & "  (" & TestoRiferimento() & ")", ""))
      Rc.Add(If(Campioni.ContainsKey(B), Campioni(B).ToString("N0"), ""))
    Next
    T.Righe.Add(Rt.ToArray)
    T.Righe.Add(Rc.ToArray)
    T.RigheEvidenza = 1

    ' una riga per canale: lettera dell'affidabilita' e valore suggerito
    For Each kv As KeyValuePair(Of clsChannel2020, List(Of clsFfFascia)) In Canali
      Dim ChY As clsChannel2020 = kv.Key
      Dim Um As String = If(String.IsNullOrWhiteSpace(ChY.ShortUM), "", " [" & ChY.ShortUM & "]")
      Dim Cella As New List(Of String) From {"FF " & ChY.ShortName & Um}
      For Each B As Double In Bande.Keys
        Dim Bk As Double = B
        Dim F As clsFfFascia = kv.Value.FirstOrDefault(Function(q) q.TwsLow = Bk)
        If F Is Nothing OrElse F.Ottimo Is Nothing Then
          Cella.Add("")
          Continue For
        End If
        Dim O As clsFfOttimo = F.Ottimo
        Dim Testo As String = Dec(O.Y, ChY)
        If Bersaglio.ContainsKey(B) AndAlso Dec(O.X, CanaleX) <> Dec(Bersaglio(B), CanaleX) Then Testo &= " @ " & Dec(O.X, CanaleX)
        Dim Rif As Double = MediaA(F, Rep().XRiferimento)
        If Not Double.IsNaN(Rif) Then Testo &= "  (" & Dec(Rif, ChY) & ")"
        Cella.Add(O.Affidabilita.Substring(0, 1) & "   " & Testo)
      Next
      T.Righe.Add(Cella.ToArray)
    Next
    T.Note.Add("FF = feed forward: value suggested for each channel at the target " & CanaleX.ShortName & " of the band;  (value) = mean observed at the reference " & CanaleX.ShortName & " " & TestoRiferimento() & ";  '@ x' = the channel has a different target.")
    T.Note.Add("Letter before the value = reliability of the suggestion:  H = high,  M = medium,  L = low.  It measures how precise the value at the target is (2 standard errors) compared with the natural scatter of the channel inside the bins; with Maximize/Minimize Y it measures the quality of the fit (R2).")
    T.Note.Add("Details, charts and bins in the following pages, one per channel.")
    Return T
  End Function

  ''' <summary>
  ''' Tabella della pagina "insieme": una colonna per fascia; in alto il suggerimento feed forward (target X, Y attesa, affidabilita', campioni),
  ''' poi una riga per bin di X, dal piu' alto al piu' basso, con la media di Y.
  ''' </summary>
  Private Function TabellaCanaleInsieme(ChY As clsChannel2020, Scelte As List(Of clsFfFascia)) As clsFfPdfTabella
    Dim T As New clsFfPdfTabella
    Dim ChBanda As clsChannel2020 = CanaleDaId(Rep().TwsChannelId)
    Dim NomeBanda As String = If(ChBanda Is Nothing, "Band", ChBanda.ShortName)
    Dim Bins As New SortedDictionary(Of Integer, clsFfBin)
    For Each F As clsFfFascia In Scelte
      For Each B As clsFfBin In F.Bins
        If B.N > 0 AndAlso Not Bins.ContainsKey(B.Indice) Then Bins(B.Indice) = B
      Next
    Next

    T.Intestazioni.Add("")
    T.Larghezze.Add(2.4)
    For Each F As clsFfFascia In Scelte
      T.Intestazioni.Add(F.TwsLow.ToString("0.#") & " - " & F.TwsHigh.ToString("0.#"))
      T.Larghezze.Add(1.0)
    Next
    T.Gruppi.Add(New KeyValuePair(Of String, Integer)("", 1))
    T.Gruppi.Add(New KeyValuePair(Of String, Integer)(NomeBanda & " bands", Scelte.Count))

    ' suggerimento feed forward
    Dim Rt As New List(Of String) From {"FF target " & CanaleX.ShortName}
    Dim Re As New List(Of String) From {"FF expected " & ChY.ShortName}
    Dim Ra As New List(Of String) From {"FF reliability (H/M/L)"}
    Dim Rc As New List(Of String) From {"Samples"}
    For Each F As clsFfFascia In Scelte
      Dim O As clsFfOttimo = F.Ottimo
      Rt.Add(If(O Is Nothing, "", Dec(O.X, CanaleX) & If(O.Estrapolato, " *", "") & "  (" & TestoRiferimento() & ")"))
      Dim Rif As Double = MediaA(F, Rep().XRiferimento)
      Re.Add(If(O Is Nothing, "", Dec(O.Y, ChY) & If(Double.IsNaN(Rif), "", "  (" & Dec(Rif, ChY) & ")")))
      Ra.Add(If(O Is Nothing, "", O.Affidabilita.Substring(0, 1)))
      Rc.Add(F.Campioni.ToString("N0"))
    Next
    T.Righe.Add(Rt.ToArray)
    T.Righe.Add(Re.ToArray)
    T.Righe.Add(Ra.ToArray)
    T.Righe.Add(Rc.ToArray)
    T.RigheEvidenza = 3

    ' bin dal piu' alto al piu' basso
    For Each Idx As Integer In Bins.Keys.Reverse
      Dim Ix As Integer = Idx
      Dim Cella As New List(Of String)
      Cella.Add(CanaleX.ShortName & " " & Bins(Idx).XLow.ToString("0.##") & " - " & Bins(Idx).XHigh.ToString("0.##"))
      For Each F As clsFfFascia In Scelte
        Dim B As clsFfBin = F.Bins.FirstOrDefault(Function(q) q.Indice = Ix AndAlso q.N > 0)
        If B Is Nothing Then
          Cella.Add("")
        Else
          Cella.Add(Dec(B.YMedio, ChY))
          If Not B.UsatoNelFit Then T.CelleGrigie.Add(T.Righe.Count & "," & Cella.Count - 1) ' colonna = posizione della cella appena aggiunta
        End If
      Next
      T.Righe.Add(Cella.ToArray)
    Next
    T.Note.Add("Rows below 'Samples' = mean of " & ChY.ShortName & " in each " & CanaleX.ShortName & " bin;  gray = bin not used in the fit (too few samples).")
    T.Note.Add("FF = feed forward: suggested " & CanaleX.ShortName & ", expected " & ChY.ShortName & ";  (value) = reference " & CanaleX.ShortName & " " & TestoRiferimento() & " and mean " & ChY.ShortName & " observed there;  * = extrapolated;  H / M / L = reliability.")
    Return T
  End Function

  ''' <summary>Tabella dettagliata di un canale Y: le stesse colonne della tabella del tab, solo per le fasce con ottimo.</summary>
  Private Function TabellaDettaglio(ChY As clsChannel2020, Buone As List(Of clsFfFascia), Riga1 As String, Riga2 As String) As clsFfPdfTabella
    Dim T As New clsFfPdfTabella With {.Titolo = "optimum by band"}
    T.Note.Add("#" & Riga1)
    T.Note.Add("#" & Riga2)
    For Each H As String In {"Band", "Samples", "Bins", "Target " & CanaleX.ShortName, "Expected " & ChY.ShortName, "+-2 SE", "Best bin X", "Best bin Y", "Gain vs best bin", "R2", "Extrapolated", "Reliability"}
      T.Intestazioni.Add(H)
    Next
    For Each F As clsFfFascia In Buone
      Dim R As clsFfRiga = RigaTabella(F, ChY)
      T.Righe.Add({R.Fascia, R.Campioni.ToString("N0"), R.Bin, R.OttimoX, R.OttimoY, R.Errore, R.MigliorX, R.MigliorY, R.Guadagno, R.R2, R.Estrapolato, R.Affidabilita})
    Next
    Return T
  End Function

  ''' <summary>Dettaglio dei bin di ogni fascia con ottimo: campioni, medie e deviazione standard di Y, e se il bin e' entrato nel fit.</summary>
  Private Function TabellaBin(ChY As clsChannel2020, Buone As List(Of clsFfFascia)) As clsFfPdfTabella
    Dim T As New clsFfPdfTabella With {.Titolo = "bins by band"}
    For Each H As String In {"Band", CanaleX.ShortName & " bin", "Samples", "Mean " & CanaleX.ShortName, "Mean " & ChY.ShortName, "Std " & ChY.ShortName, "Used in fit"}
      T.Intestazioni.Add(H)
    Next
    For Each F As clsFfFascia In Buone
      Dim Banda As String = F.TwsLow.ToString("0.#") & " - " & F.TwsHigh.ToString("0.#")
      For Each B As clsFfBin In F.Bins
        T.Righe.Add({Banda, B.XLow.ToString("0.##") & " - " & B.XHigh.ToString("0.##"), B.N.ToString("N0"), Dec(B.XMedio, CanaleX), Dec(B.YMedio, ChY), Dec(B.YDev, ChY), If(B.UsatoNelFit, "yes", "no")})
      Next
    Next
    Return T
  End Function

  ' ---------------------------------------------------------------- PDF

  ''' <summary>Disegna la fascia, aspetta che SciChart l'abbia resa e ne esporta l'immagine.</summary>
  Private Async Function CatturaAsync(Fasce As List(Of clsFfFascia), ChY As clsChannel2020) As Threading.Tasks.Task(Of IO.Stream)
    Dim Fatto As New Threading.Tasks.TaskCompletionSource(Of Boolean)
    Dim Gestore As EventHandler(Of EventArgs) = Sub(s As Object, e As EventArgs) Fatto.TrySetResult(True)
    AddHandler Plot.Rendered, Gestore
    Try
      DisegnaFasce(Fasce, ChY, Nothing)
      Plot.InvalidateElement()
      Await Threading.Tasks.Task.WhenAny(Fatto.Task, Threading.Tasks.Task.Delay(4000))
    Finally
      RemoveHandler Plot.Rendered, Gestore
    End Try
    Await Threading.Tasks.Task.Delay(100)
    Return Plot.ExportToStream(SciChart.Core.ExportType.Bmp, False)
  End Function

  Private Async Sub btn_Pdf_Click(sender As Object, e As RoutedEventArgs)
    If Not EseguiCalcolo() Then Exit Sub
    Dim S As clsFeedForwardReport = Rep()
    Dim Nome As String = If(String.IsNullOrWhiteSpace(S.Nome), "FeedForward", S.Nome)
    Dim Cartella As String = DataProvider2020.Files.First.Directory.FullName
    Dim NomeBase As String = DataProvider2020.TimeRange.Start.ToString("yyyyMMdd") & "_FeedForward_" & Nome
    Dim Input As New UserControl_InputBox(NomeBase, "Pdf File Name")
    Input.Owner = Window.GetWindow(Me)
    Input.ShowDialog()
    If Not (Input.DialogResult.HasValue AndAlso Input.DialogResult.Value = True) Then Exit Sub
    Dim NomePdf As String = Input.Testo

    Dim SelPrima As Object = lst_Y.SelectedItem
    btn_Pdf.IsEnabled = False
    btn_Calcola.IsEnabled = False
    Try
      Dim Sezioni As New List(Of clsFfPdfSezione)
      Dim P As clsFfParametri = S.Parametri
      Dim n As Integer = 0
      Dim Saltate As Integer = 0
      Dim Riga1 As String = "Source: " & cmb_Sorgente.Text & "   |   " & DescrizioneFiltro
      Dim Riga2 As String = "Bands " & P.TwsMin.ToString("0.#") & " - " & P.TwsMax.ToString("0.#") & " step " & P.TwsStep.ToString("0.#") &
                            "   |   X bins from " & P.XBinStart.ToString("0.##") & If(P.XBinEnd > P.XBinStart, " to " & P.XBinEnd.ToString("0.##"), "") & " width " & P.XBinWidth.ToString("0.##") &
                            " (min " & P.MinCampioniBin & " samples)   |   Model: " & cmb_Modello.Text & ", " & cmb_Obiettivo.Text
      Dim Pagine As New List(Of clsFfPdfPagina)
      Dim PerRiepilogo As New List(Of KeyValuePair(Of clsChannel2020, List(Of clsFfFascia)))
      Dim Riepilogo As clsFfPdfTabella = TabellaRiepilogo(Riga1, Riga2)
      For Each V As clsVoceY In VociY
        Dim Lista As List(Of clsFfFascia) = Nothing
        If Not Risultati.TryGetValue(V.Id, Lista) Then Continue For
        Dim ChY As clsChannel2020 = CanaliY(V.Id)
        ' solo le fasce con dati sufficienti (un ottimo calcolato): le altre non si stampano ne' come grafico ne' come riga di tabella
        Dim Buone As List(Of clsFfFascia) = Lista.Where(Function(f) f.Ottimo IsNot Nothing).ToList
        Saltate += Lista.Where(Function(f) f.Campioni > 0 AndAlso f.Ottimo Is Nothing).Count
        If Buone.Count = 0 Then Continue For
        If S.PdfInsieme Then
          ' una pagina verticale per canale: grafico con le fasce selezionate sopra, tabella dei bin e del feed forward sotto
          Dim SelP As HashSet(Of Double) = Nothing
          Dim Scelte As List(Of clsFfFascia) = If(Selezione.TryGetValue(V.Id, SelP), Buone.Where(Function(f) SelP.Contains(f.TwsLow)).ToList, New List(Of clsFfFascia))
          If Scelte.Count = 0 Then Continue For
          txt_Stato.Text = "Printing " & ChY.ShortName & ", " & Scelte.Count & " bands together..."
          Dim Pg As New clsFfPdfPagina With {.Titolo = ChY.LongName & " " & CanaleX.LongName & " FeedForward Optimization"}
          Pg.Righe.Add(Riga1)
          Pg.Righe.Add(Riga2)
          Pg.Immagine = Await CatturaAsync(Scelte, ChY)
          Pg.Tabella = TabellaCanaleInsieme(ChY, Scelte)
          Pagine.Add(Pg)
          PerRiepilogo.Add(New KeyValuePair(Of clsChannel2020, List(Of clsFfFascia))(ChY, Scelte))
          n += 1
          Continue For
        End If
        Dim Sez As New clsFfPdfSezione With {.Titolo = "Y: " & ChY.LongName & "   -   X: " & CanaleX.LongName}
        Sez.Righe.Add(Riga1)
        Sez.Righe.Add(Riga2)
        Sez.Tabelle.Add(TabellaDettaglio(ChY, Buone, Riga1, Riga2))
        Sez.Tabelle.Add(TabellaBin(ChY, Buone))
        For Each F As clsFfFascia In Buone
          txt_Stato.Text = "Printing " & ChY.ShortName & ", band " & F.TwsLow.ToString("0.#") & "-" & F.TwsHigh.ToString("0.#") & "..."
          Sez.Immagini.Add(Await CatturaAsync(New List(Of clsFfFascia) From {F}, ChY))
          n += 1
        Next
        Sezioni.Add(Sez)
      Next

      Dim Percorso As String = IO.Path.Combine(Cartella, NomePdf & ".pdf")
      For i As Integer = 1 To 100
        If Not IO.File.Exists(Percorso) Then Exit For
        Percorso = IO.Path.Combine(Cartella, NomePdf & "_" & i & ".pdf")
      Next
      ' prima pagina: riepilogo con i valori suggeriti di tutti i canali, per fascia
      If S.PdfInsieme AndAlso PerRiepilogo.Count > 0 Then
        Dim PgR As New clsFfPdfPagina With {.Titolo = CanaleX.LongName & " FeedForward Summary"}
        PgR.Righe.Add(Riga1)
        PgR.Righe.Add(Riga2)
        PgR.Tabella = TabellaRiepilogoInsieme(PerRiepilogo)
        Pagine.Insert(0, PgR)
      End If
      Dim Creato As Boolean = If(S.PdfInsieme, clsPdfFeedForward.CreaPagine(Percorso, "Feed Forward - " & Nome, Pagine),
                                 clsPdfFeedForward.Crea(Percorso, "Feed Forward - " & Nome, Riepilogo, Sezioni, S.RighePagina, S.ColonnePagina))
      If Creato Then
        txt_Stato.Text = n & " charts printed" & If(Saltate > 0, " (" & Saltate & " bands skipped: not enough data)", "") & ": " & Percorso
        Process.Start(Percorso)
      Else
        txt_Stato.Text = "Nothing to print: no band has enough data."
      End If
    Catch ex As Exception
      MsgBox("Error creating the pdf: " & ex.Message, MsgBoxStyle.Exclamation, "Feed Forward")
    Finally
      btn_Pdf.IsEnabled = True
      btn_Calcola.IsEnabled = True
      MostraY(TryCast(SelPrima, clsVoceY))
    End Try
  End Sub

End Class
