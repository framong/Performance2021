Imports System.Collections.ObjectModel

''' <summary>
''' Finestra modale di configurazione dei parametri usati per calcolare i tre indici di qualita'
''' e per la creazione automatica dei periodi.
''' Lavora su copie: il profilo viene toccato solo alla conferma.
''' La struttura dell'algoritmo (quali canali, a quale indice appartengono, quali sono legati
''' al mare) non e' modificabile: da qui si tarano solo i valori numerici.
''' </summary>
Public Class QualitySettingsForm

  ''' <summary>Copia di lavoro delle impostazioni di qualita'.</summary>
  Private _Settings As clsQualitySettings

  ''' <summary>Copia di lavoro dei parametri del periods finder toccati da questa finestra.</summary>
  Private _MinEnvQIndex As Integer = 3
  Private _MinAttQIndex As Integer = 3
  Private _MinPerfQIndex As Integer = 3
  Private _MinPeriodSeconds As Integer = 15
  Private _MaxPeriodSeconds As Integer = 300

  ''' <summary>True se l'utente ha chiesto anche il ricalcolo immediato dei canali.</summary>
  Public Property RichiestoRefresh As Boolean = False

  Public Sub New()
    InitializeComponent()

    If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) Then Exit Sub

    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then
      _Settings = New clsQualitySettings
      _Settings.CaricaValoriDefault()
    Else
      If AppConfig.ActiveProfile.DataQualitySettings Is Nothing Then
        AppConfig.ActiveProfile.DataQualitySettings = New clsQualitySettings
        AppConfig.ActiveProfile.DataQualitySettings.CaricaValoriDefault()
      End If
      AppConfig.ActiveProfile.DataQualitySettings.NormalizzaValoriMancanti()
      _Settings = AppConfig.ActiveProfile.DataQualitySettings.Clona()
      _Settings.NormalizzaValoriMancanti()

      If AppConfig.ActiveProfile.PeriodsFinderSettings Is Nothing Then
        AppConfig.ActiveProfile.PeriodsFinderSettings = New clsPeriodsFinderSettings
        AppConfig.ActiveProfile.PeriodsFinderSettings.ImpostaValoriDefault()
      End If
      AppConfig.ActiveProfile.PeriodsFinderSettings.NormalizzaValoriMancanti()

      Dim PF As clsPeriodsFinderSettings = AppConfig.ActiveProfile.PeriodsFinderSettings
      _MinEnvQIndex = PF.MinEnvironmentQualityIndex
      _MinAttQIndex = PF.MinAttitudeQualityIndex
      _MinPerfQIndex = PF.MinDataQualityIndex
      _MinPeriodSeconds = PF.MinPeriodSeconds
      _MaxPeriodSeconds = PF.MaxPeriodSeconds
    End If

    MostraSettings()
    MostraCanaliAssenti()
  End Sub

#Region "Da settings a controlli e viceversa"

  Private Sub MostraSettings()
    txt_SecondiSampling.Text = _Settings.SecondiSampling.ToString
    txt_SeaRef.Text = _Settings.SeaRef.ToString

    txt_RefWindowMinutes.Text = _Settings.RefWindowMinutes.ToString
    txt_MinEnvQPerRiferimento.Text = _Settings.MinEnvQPerRiferimento.ToString
    txt_MinAttQPerRiferimento.Text = _Settings.MinAttQPerRiferimento.ToString

    txt_MinEnvironmentQualityIndex.Text = _MinEnvQIndex.ToString
    txt_MinAttitudeQualityIndex.Text = _MinAttQIndex.ToString
    txt_MinDataQualityIndex.Text = _MinPerfQIndex.ToString
    txt_MinPeriodSeconds.Text = _MinPeriodSeconds.ToString
    txt_MaxPeriodSeconds.Text = _MaxPeriodSeconds.ToString

    txt_EnvQ5.Text = _Settings.MaxTollEnvQ5.ToString
    txt_EnvQ4.Text = _Settings.MaxTollEnvQ4.ToString
    txt_EnvQ3.Text = _Settings.MaxTollEnvQ3.ToString
    txt_EnvQ2.Text = _Settings.MaxTollEnvQ2.ToString
    txt_EnvQ1.Text = _Settings.MaxTollEnvQ1.ToString

    txt_AttQ5.Text = _Settings.MaxTollAttQ5.ToString
    txt_AttQ4.Text = _Settings.MaxTollAttQ4.ToString
    txt_AttQ3.Text = _Settings.MaxTollAttQ3.ToString
    txt_AttQ2.Text = _Settings.MaxTollAttQ2.ToString
    txt_AttQ1.Text = _Settings.MaxTollAttQ1.ToString

    txt_Perf5.Text = _Settings.MaxAbsDeltaPerf5.ToString
    txt_Perf4.Text = _Settings.MaxAbsDeltaPerf4.ToString
    txt_Perf3.Text = _Settings.MaxAbsDeltaPerf3.ToString
    txt_Perf2.Text = _Settings.MaxAbsDeltaPerf2.ToString
    txt_Perf1.Text = _Settings.MaxAbsDeltaPerf1.ToString

    grd_Channels.ItemsSource = _Settings.ChannelSettings
  End Sub

  ''' <summary>
  ''' Segnala quali canali previsti dall'algoritmo non sono presenti nel file caricato:
  ''' la media viene fatta solo sui canali disponibili.
  ''' </summary>
  Private Sub MostraCanaliAssenti()
    txt_CanaliAssenti.Text = ""
    If DataProvider2020 Is Nothing Then
      txt_CanaliAssenti.Text = "No file loaded: cannot check which channels are actually available."
      Exit Sub
    End If

    Dim Assenti As New List(Of String)
    For Each c As clsQualityChannelSetting In _Settings.ChannelSettings
      If DataProvider2020.CanaleDbl(c.CanaleChiave) Is Nothing Then Assenti.Add(c.NomeCanale)
    Next

    If Assenti.Count > 0 Then
      txt_CanaliAssenti.Text = "Not available in the loaded file, excluded from the calculation: " & String.Join(", ", Assenti)
    End If
  End Sub

  ''' <summary>
  ''' Legge i controlli nelle copie di lavoro. Restituisce False se qualche valore non e' valido:
  ''' in quel caso l'utente ha gia' visto il messaggio e non si deve chiudere la finestra.
  ''' </summary>
  Private Function LeggiSettings() As Boolean
    grd_Channels.CommitEdit(DataGridEditingUnit.Row, True)

    Dim v As Double

    If Not LeggiNumero(txt_SecondiSampling, "Sampling window", 1, 600, v) Then Return False
    _Settings.SecondiSampling = CInt(v)
    If Not LeggiNumero(txt_SeaRef, "Sea reference", 0.01, 20, v) Then Return False
    _Settings.SeaRef = v

    If Not LeggiNumero(txt_RefWindowMinutes, "Reference window", 1, 240, v) Then Return False
    _Settings.RefWindowMinutes = v
    If Not LeggiNumero(txt_MinEnvQPerRiferimento, "Min Env score for reference", 0, 5, v) Then Return False
    _Settings.MinEnvQPerRiferimento = CInt(v)
    If Not LeggiNumero(txt_MinAttQPerRiferimento, "Min Att score for reference", 0, 5, v) Then Return False
    _Settings.MinAttQPerRiferimento = CInt(v)

    If Not LeggiNumero(txt_MinEnvironmentQualityIndex, "Min EnvironmentQuality", 0, 5, v) Then Return False
    _MinEnvQIndex = CInt(v)
    If Not LeggiNumero(txt_MinAttitudeQualityIndex, "Min AttitudeQuality", 0, 5, v) Then Return False
    _MinAttQIndex = CInt(v)
    If Not LeggiNumero(txt_MinDataQualityIndex, "Min PerformanceQuality", 0, 5, v) Then Return False
    _MinPerfQIndex = CInt(v)

    If Not LeggiNumero(txt_MinPeriodSeconds, "Min period duration", 1, 3600, v) Then Return False
    _MinPeriodSeconds = CInt(v)
    If Not LeggiNumero(txt_MaxPeriodSeconds, "Max period duration", 1, 7200, v) Then Return False
    _MaxPeriodSeconds = CInt(v)
    If _MaxPeriodSeconds <= _MinPeriodSeconds Then
      MsgBox("The maximum period duration must be greater than the minimum.", MsgBoxStyle.Exclamation, "Quality Settings")
      Return False
    End If

    If Not LeggiNumero(txt_EnvQ5, "Environment Quality 5", 1, 1000, v) Then Return False
    _Settings.MaxTollEnvQ5 = v
    If Not LeggiNumero(txt_EnvQ4, "Environment Quality 4", 1, 1000, v) Then Return False
    _Settings.MaxTollEnvQ4 = v
    If Not LeggiNumero(txt_EnvQ3, "Environment Quality 3", 1, 1000, v) Then Return False
    _Settings.MaxTollEnvQ3 = v
    If Not LeggiNumero(txt_EnvQ2, "Environment Quality 2", 1, 1000, v) Then Return False
    _Settings.MaxTollEnvQ2 = v
    If Not LeggiNumero(txt_EnvQ1, "Environment Quality 1", 1, 1000, v) Then Return False
    _Settings.MaxTollEnvQ1 = v

    If Not LeggiNumero(txt_AttQ5, "Attitude Quality 5", 1, 1000, v) Then Return False
    _Settings.MaxTollAttQ5 = v
    If Not LeggiNumero(txt_AttQ4, "Attitude Quality 4", 1, 1000, v) Then Return False
    _Settings.MaxTollAttQ4 = v
    If Not LeggiNumero(txt_AttQ3, "Attitude Quality 3", 1, 1000, v) Then Return False
    _Settings.MaxTollAttQ3 = v
    If Not LeggiNumero(txt_AttQ2, "Attitude Quality 2", 1, 1000, v) Then Return False
    _Settings.MaxTollAttQ2 = v
    If Not LeggiNumero(txt_AttQ1, "Attitude Quality 1", 1, 1000, v) Then Return False
    _Settings.MaxTollAttQ1 = v

    If Not LeggiNumero(txt_Perf5, "Performance Quality 5", 0.01, 200, v) Then Return False
    _Settings.MaxAbsDeltaPerf5 = v
    If Not LeggiNumero(txt_Perf4, "Performance Quality 4", 0.01, 200, v) Then Return False
    _Settings.MaxAbsDeltaPerf4 = v
    If Not LeggiNumero(txt_Perf3, "Performance Quality 3", 0.01, 200, v) Then Return False
    _Settings.MaxAbsDeltaPerf3 = v
    If Not LeggiNumero(txt_Perf2, "Performance Quality 2", 0.01, 200, v) Then Return False
    _Settings.MaxAbsDeltaPerf2 = v
    If Not LeggiNumero(txt_Perf1, "Performance Quality 1", 0.01, 200, v) Then Return False
    _Settings.MaxAbsDeltaPerf1 = v

    For Each c As clsQualityChannelSetting In _Settings.ChannelSettings
      If c.DeltaEquivalenteUpwind <= 0 OrElse c.DeltaEquivalenteReaching <= 0 OrElse c.DeltaEquivalenteDownwind <= 0 Then
        MsgBox("Channel " & c.NomeCanale & ": the deltas must be greater than zero.", MsgBoxStyle.Exclamation, "Quality Settings")
        Return False
      End If
    Next

    ' la tolleranza ammessa cresce dal voto 5 al voto 1
    If Not SoglieCrescenti(_Settings.MaxTollEnvQ5, _Settings.MaxTollEnvQ4, _Settings.MaxTollEnvQ3, _Settings.MaxTollEnvQ2, _Settings.MaxTollEnvQ1, "Environment Quality") Then Return False
    If Not SoglieCrescenti(_Settings.MaxTollAttQ5, _Settings.MaxTollAttQ4, _Settings.MaxTollAttQ3, _Settings.MaxTollAttQ2, _Settings.MaxTollAttQ1, "Attitude Quality") Then Return False
    ' lo scostamento ammesso cresce dal voto 5 al voto 1
    If Not SoglieCrescenti(_Settings.MaxAbsDeltaPerf5, _Settings.MaxAbsDeltaPerf4, _Settings.MaxAbsDeltaPerf3, _Settings.MaxAbsDeltaPerf2, _Settings.MaxAbsDeltaPerf1, "Performance Quality") Then Return False

    Return True
  End Function

  Private Function LeggiNumero(Casella As TextBox, Descrizione As String, Minimo As Double, Massimo As Double, ByRef Valore As Double) As Boolean
    If Not Double.TryParse(Casella.Text.Trim.Replace(",", "."), Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, Valore) Then
      MsgBox(Descrizione & ": '" & Casella.Text & "' is not a valid number.", MsgBoxStyle.Exclamation, "Quality Settings")
      Casella.Focus()
      Casella.SelectAll()
      Return False
    End If
    If Valore < Minimo OrElse Valore > Massimo Then
      MsgBox(Descrizione & ": the value must be between " & Minimo.ToString & " and " & Massimo.ToString & ".", MsgBoxStyle.Exclamation, "Quality Settings")
      Casella.Focus()
      Casella.SelectAll()
      Return False
    End If
    Return True
  End Function

  Private Function SoglieCrescenti(v5 As Double, v4 As Double, v3 As Double, v2 As Double, v1 As Double, Descrizione As String) As Boolean
    If v5 < v4 AndAlso v4 < v3 AndAlso v3 < v2 AndAlso v2 < v1 Then Return True
    MsgBox(Descrizione & ": the thresholds must increase from 5 up to 1.", MsgBoxStyle.Exclamation, "Quality Settings")
    Return False
  End Function

#End Region

#Region "Pulsanti"

  Private Function Applica() As Boolean
    If Not LeggiSettings() Then Return False
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return False

    AppConfig.ActiveProfile.DataQualitySettings = _Settings

    If AppConfig.ActiveProfile.PeriodsFinderSettings Is Nothing Then
      AppConfig.ActiveProfile.PeriodsFinderSettings = New clsPeriodsFinderSettings
      AppConfig.ActiveProfile.PeriodsFinderSettings.ImpostaValoriDefault()
    End If

    With AppConfig.ActiveProfile.PeriodsFinderSettings
      .MinEnvironmentQualityIndex = _MinEnvQIndex
      .MinAttitudeQualityIndex = _MinAttQIndex
      .MinDataQualityIndex = _MinPerfQIndex
      .MinPeriodSeconds = _MinPeriodSeconds
      .MaxPeriodSeconds = _MaxPeriodSeconds
    End With

    AppConfig.Salva()
    Return True
  End Function

  Private Sub btn_Save_Click(sender As Object, e As RoutedEventArgs)
    If Not Applica() Then Exit Sub
    RichiestoRefresh = False
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub btn_SaveAndRefresh_Click(sender As Object, e As RoutedEventArgs)
    If Not Applica() Then Exit Sub
    RichiestoRefresh = True
    Me.DialogResult = True
    Me.Close()
  End Sub

  Private Sub btn_Defaults_Click(sender As Object, e As RoutedEventArgs)
    If MsgBox("Reset all quality settings to the factory values?", MsgBoxStyle.OkCancel Or MsgBoxStyle.Question, "Quality Settings") <> MsgBoxResult.Ok Then Exit Sub
    _Settings = New clsQualitySettings
    _Settings.CaricaValoriDefault()

    Dim pf As New clsPeriodsFinderSettings
    pf.ImpostaValoriDefault()
    _MinEnvQIndex = pf.MinEnvironmentQualityIndex
    _MinAttQIndex = pf.MinAttitudeQualityIndex
    _MinPerfQIndex = pf.MinDataQualityIndex
    _MinPeriodSeconds = pf.MinPeriodSeconds
    _MaxPeriodSeconds = pf.MaxPeriodSeconds

    MostraSettings()
    MostraCanaliAssenti()
  End Sub

  Private Sub btn_Cancel_Click(sender As Object, e As RoutedEventArgs)
    Me.DialogResult = False
    Me.Close()
  End Sub

#End Region

End Class


''' <summary>
''' Converter per le celle numeriche della griglia canali.
''' Accetta indifferentemente il punto e la virgola come separatore decimale: con il solo
''' binding standard la conversione segue la cultura di sistema e un valore come 0.3
''' digitato col punto verrebbe rifiutato senza alcun avviso.
''' </summary>
Public Class clsDecimalFlexConverter
  Implements IValueConverter

  Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
    If value Is Nothing Then Return ""
    Dim d As Double
    If Not Double.TryParse(value.ToString, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, d) Then
      If Not Double.TryParse(value.ToString, d) Then Return value.ToString
    End If
    Return d.ToString(Globalization.CultureInfo.CurrentCulture)
  End Function

  Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
    If value Is Nothing Then Return Binding.DoNothing
    Dim t As String = value.ToString.Trim
    If t = "" Then Return Binding.DoNothing

    Dim d As Double

    ' prima si prova con la cultura corrente, poi normalizzando il separatore
    If Double.TryParse(t, d) Then Return d

    Dim Sep As String = Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
    Dim Normalizzato As String = t.Replace(".", Sep).Replace(",", Sep)
    If Double.TryParse(Normalizzato, d) Then Return d

    If Double.TryParse(t.Replace(",", "."), Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, d) Then Return d

    ' valore non interpretabile: si lascia invariato quello precedente
    Return Binding.DoNothing
  End Function

End Class
