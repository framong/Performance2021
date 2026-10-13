Imports System.Globalization

''' <summary>
''' Finestra modale dell'inerzia della barca (profilo attivo): riferimenti per il Tau di default, Tau in uso (modificabile a mano o ripristinabile
''' al default) e stima dal recupero di velocita' dopo virate e strambate (clsInerzia.Stima). Lavora su una copia fino a "Save and Refresh".
''' </summary>
Public Class InerziaForm

  Private _S As clsInerziaSettings
  Private _Caricando As Boolean = True
  Private _Stime As New List(Of clsFitManovra)

  Public Sub New()
    InitializeComponent()
    If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) Then Exit Sub
    _S = clsInerzia.Impostazioni().Clona()
    txt_Disl.Text = Testo(_S.DislocamentoKg)
    txt_Vref.Text = Testo(_S.VelocitaRiferimentoKn)
    txt_Drag.Text = Testo(_S.ResistenzaN)
    txt_Tau.Text = _S.TauSec.ToString("0.0", CultureInfo.InvariantCulture)
    _Caricando = False
    AggiornaDefault()
    txt_Origine.Text = "Origin: " & _S.Origine
  End Sub

  Private Shared Function Testo(V As Double) As String
    Return V.ToString("0.###", CultureInfo.InvariantCulture)
  End Function

  Private Shared Function Numero(T As String, Predefinito As Double) As Double
    Dim V As Double
    If Double.TryParse(T, NumberStyles.Float, CultureInfo.InvariantCulture, V) Then Return V
    Return Predefinito
  End Function

  Private Sub LeggiRiferimenti()
    _S.DislocamentoKg = Math.Max(0, Numero(txt_Disl.Text, _S.DislocamentoKg))
    _S.VelocitaRiferimentoKn = Math.Max(0, Numero(txt_Vref.Text, _S.VelocitaRiferimentoKn))
    _S.ResistenzaN = Math.Max(0, Numero(txt_Drag.Text, _S.ResistenzaN))
  End Sub

  Private Sub AggiornaDefault()
    LeggiRiferimenti()
    txt_TauDefault.Text = clsInerzia.TauDefault(_S).ToString("0.0", CultureInfo.InvariantCulture)
  End Sub

  ''' <summary>Riferimenti cambiati: si aggiorna il default; se il Tau in uso e' quello di default lo segue.</summary>
  Private Sub Riferimenti_Cambiati(sender As Object, e As TextChangedEventArgs)
    If _Caricando Then Exit Sub
    AggiornaDefault()
    If _S.Origine = clsInerzia.OrigineDefault Then ImpostaTau(clsInerzia.TauDefault(_S), clsInerzia.OrigineDefault)
  End Sub

  Private Sub ImpostaTau(Tau As Double, Origine As String)
    _Caricando = True
    _S.TauSec = Tau
    _S.Origine = Origine
    txt_Tau.Text = Tau.ToString("0.0", CultureInfo.InvariantCulture)
    txt_Origine.Text = "Origin: " & Origine
    _Caricando = False
  End Sub

  ''' <summary>Tau scritto a mano: diventa una correzione manuale.</summary>
  Private Sub Tau_Cambiato(sender As Object, e As TextChangedEventArgs)
    If _Caricando Then Exit Sub
    Dim V As Double = Numero(txt_Tau.Text, Double.NaN)
    If Double.IsNaN(V) OrElse V <= 0 Then Exit Sub
    _S.TauSec = V
    _S.Origine = "manual"
    txt_Origine.Text = "Origin: manual"
  End Sub

  Private Sub btn_Default_Click(sender As Object, e As RoutedEventArgs)
    AggiornaDefault()
    ImpostaTau(clsInerzia.TauDefault(_S), clsInerzia.OrigineDefault)
  End Sub

  ''' <summary>Stima dal dataset caricato: Sow lisciata su 1 s, key moment delle virate e strambate del Pavarot, recupero fino alla manovra successiva.</summary>
  Private Sub btn_Stima_Click(sender As Object, e As RoutedEventArgs)
    btn_Usa.IsEnabled = False
    grd_Manovre.ItemsSource = Nothing
    txt_Riepilogo.Text = ""
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati OrElse DataProvider2020.TimeStamps Is Nothing Then
      MsgBox("Load a dataset first.", MsgBoxStyle.Exclamation, "Boat Inertia")
      Exit Sub
    End If
    If PeriodsManager Is Nothing OrElse (PeriodsManager.CollectionTack.Count = 0 AndAlso PeriodsManager.CollectionGybe.Count = 0) Then
      MsgBox("There are no tacks or gybes: find the manoeuvres first (Auto Periods / Pavarot).", MsgBoxStyle.Exclamation, "Boat Inertia")
      Exit Sub
    End If
    Dim chSow As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    If chSow Is Nothing OrElse chSow.Valori Is Nothing Then
      MsgBox("The boat speed channel is not available.", MsgBoxStyle.Exclamation, "Boat Inertia")
      Exit Sub
    End If
    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)

    Dim Cursore As Input.Cursor = Input.Mouse.OverrideCursor
    Input.Mouse.OverrideCursor = Input.Cursors.Wait
    Try
      Dim Ts() As DateTime = DataProvider2020.TimeStamps
      Dim T0 As DateTime = Ts(0)
      Dim T(Ts.Length - 1) As Double
      For i As Integer = 0 To Ts.Length - 1
        T(i) = If(Ts(i).Ticks = 0, Double.NaN, (Ts(i) - T0).TotalSeconds)
      Next
      Dim V() As Double = DataProvider2020.MediaFinestraTemporale(chSow.Valori, -0.5, 0.5, False)

      Dim Tutte As List(Of KeyValuePair(Of DateTime, Boolean)) =
        PeriodsManager.CollectionTack.Select(Function(p) New KeyValuePair(Of DateTime, Boolean)(p.KeyMoment, True)).
        Concat(PeriodsManager.CollectionGybe.Select(Function(p) New KeyValuePair(Of DateTime, Boolean)(p.KeyMoment, False))).
        Where(Function(q) q.Key.Ticks > 0).OrderBy(Function(q) q.Key).ToList
      _Stime = New List(Of clsFitManovra)
      For j As Integer = 0 To Tutte.Count - 1
        Dim Km As DateTime = Tutte(j).Key
        Dim Idx As Integer = DataProvider2020.TrovaIndice(Km)
        Dim M As New clsFitManovra With {.Momento = Km, .IsTack = Tutte(j).Value, .IndiceKeyMoment = Idx}
        If j < Tutte.Count - 1 Then M.TempoLimite = (Tutte(j + 1).Key - T0).TotalSeconds - 5
        If chTws IsNot Nothing AndAlso chTws.Valori IsNot Nothing AndAlso Idx >= 0 AndAlso Idx < chTws.Valori.Length Then M.Tws = chTws.Valori(Idx)
        _Stime.Add(M)
      Next
      clsInerzia.Stima(T, V, _Stime)
      grd_Manovre.ItemsSource = _Stime
      MostraRiepilogo()
    Catch ex As Exception
      MsgBox("Error while estimating: " & ex.Message, MsgBoxStyle.Exclamation, "Boat Inertia")
    Finally
      Input.Mouse.OverrideCursor = Cursore
    End Try
  End Sub

  Private Function Parte(Nome As String, L As List(Of clsFitManovra), Tot As Integer) As String
    If L.Count = 0 Then Return Nome & ": none accepted of " & Tot
    Dim Ci As CultureInfo = CultureInfo.InvariantCulture
    Dim Taus As IEnumerable(Of Double) = L.Select(Function(q) q.Tau)
    Return Nome & ": tau " & clsInerzia.Mediana(Taus).ToString("0.0", Ci) & " s (typical " & clsInerzia.Quantile(Taus, 0.25).ToString("0.0", Ci) & " - " &
           clsInerzia.Quantile(Taus, 0.75).ToString("0.0", Ci) & ", " & L.Count & " of " & Tot & ")"
  End Function

  Private Sub MostraRiepilogo()
    Dim Ok As List(Of clsFitManovra) = _Stime.Where(Function(q) q.Accettata).ToList
    txt_Riepilogo.Text = Parte("Tacks", Ok.Where(Function(q) q.IsTack).ToList, _Stime.Where(Function(q) q.IsTack).Count) & "     " &
                         Parte("Gybes", Ok.Where(Function(q) Not q.IsTack).ToList, _Stime.Where(Function(q) Not q.IsTack).Count) & vbCrLf &
                         Parte("All", Ok, _Stime.Count)
    btn_Usa.IsEnabled = Ok.Count > 0
  End Sub

  Private Sub btn_Usa_Click(sender As Object, e As RoutedEventArgs)
    Dim Ok As List(Of clsFitManovra) = _Stime.Where(Function(q) q.Accettata).ToList
    If Ok.Count = 0 Then Exit Sub
    Dim Tau As Double = Math.Round(clsInerzia.Mediana(Ok.Select(Function(q) q.Tau)), 1)
    ImpostaTau(Tau, "estimated from " & Ok.Count & " manoeuvres (" & Ok.Where(Function(q) q.IsTack).Count & " tacks, " & Ok.Where(Function(q) Not q.IsTack).Count & " gybes) on " & Now.ToString("yyyy-MM-dd"))
  End Sub

  Private Sub btn_Save_Click(sender As Object, e As RoutedEventArgs)
    LeggiRiferimenti()
    Dim V As Double = Numero(txt_Tau.Text, Double.NaN)
    If Double.IsNaN(V) OrElse V <= 0 OrElse V > 120 Then
      MsgBox("Tau must be a number of seconds between 0 and 120.", MsgBoxStyle.Exclamation, "Boat Inertia")
      Exit Sub
    End If
    _S.TauSec = V
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Exit Sub
    AppConfig.ActiveProfile.Inerzia = _S
    AppConfig.Salva()
    Me.DialogResult = True
  End Sub

  Private Sub btn_Cancel_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
  End Sub

End Class
