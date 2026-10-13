Imports System.IO
Imports System.IO.Compression
Imports Newtonsoft.Json

''' <summary>
''' Esporta i "risultati di riferimento" per la migrazione a Performance2027 (C#, .NET 10): per il dataset caricato con il profilo attivo,
''' tutti i canali (grezzi e matematici, calcolati qui con le impostazioni correnti), gli istanti, i periodi e i risultati del Feed Forward.
''' La nuova applicazione deve ricalcolare gli stessi numeri: questi file sono il criterio di verifica del porting.
''' Formato (una cartella per esportazione):
'''  - meta.json: versione, profilo, file, intervallo esportato, impostazioni che cambiano i calcoli, elenco dei canali nell'ordine dei dati;
'''  - timestamps.bin.gz: per ogni riga esportata un Int64 (DateTime.Ticks), little endian;
'''  - channels.bin.gz: per ogni canale di meta.json, nell'ordine, le righe esportate come Double little endian (NaN dove manca il dato);
'''  - periods.json: straight line, virate, strambate e manovre Pavarot;
'''  - feedforward.json: risultati del motore Feed Forward e della sensitivita' con il report di lavoro, su tutte le righe e senza filtro.
''' </summary>
Public Class clsEsportaRiferimenti

  ''' <summary>Canali con il calcolo non piu' usato (conversione radianti con Stop nel codice): non si forzano.</summary>
  Private Shared ReadOnly DaNonForzare As clsChannels2020.eCanaliChiave() = {
    clsChannels2020.eCanaliChiave.ePRT, clsChannels2020.eCanaliChiave.eYRT, clsChannels2020.eCanaliChiave.eRRT}

  Private Class clsCanaleEsportato
    Public Property ChannelId As String
    Public Property CanaleChiave As String
    Public Property ShortName As String
    Public Property LongName As String
    Public Property ShortUM As String
    Public Property Decimals As Integer
    Public Property DataType As String
    Public Property IsMath As Boolean
    ''' <summary>ok, missing (nessun valore), error: messaggio.</summary>
    Public Property Stato As String = "ok"
    Public Property ValoriValidi As Integer
  End Class

  ''' <summary>Esegue l'esportazione e restituisce la cartella creata (Nothing se annullata o impossibile).</summary>
  Public Shared Function Esporta(Versione As String, SoloVisibile As Boolean) As String
    If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati OrElse DataProvider2020.TimeStamps Is Nothing Then Return Nothing
    Dim Ts() As DateTime = DataProvider2020.TimeStamps
    Dim N As Integer = Ts.Length
    Dim Da As Integer = 0, A As Integer = N - 1
    If SoloVisibile AndAlso DataPlotSync IsNot Nothing AndAlso DataPlotSync.VisibleRange IsNot Nothing Then
      Da = Math.Max(0, DataPlotSync.VisibleRange.IdRigaIniziale)
      A = Math.Min(N - 1, DataPlotSync.VisibleRange.IdRigaFinale)
    End If
    If A < Da Then Return Nothing

    Dim Base As String = DataProvider2020.Files(0).Directory.FullName
    Dim Cartella As String = Path.Combine(Base, "Performance2027_reference", Now.ToString("yyyyMMdd_HHmmss") & "_" & Path.GetFileNameWithoutExtension(DataProvider2020.Files(0).Name))
    Directory.CreateDirectory(Cartella)

    ' 1. tutti i canali: grezzi caricati e matematici calcolati; i nuovi canali creati durante il calcolo (target "_Tgt", "_Pol") entrano al giro dopo
    Dim Fatti As New HashSet(Of clsChannel2020)
    Dim Elenco As New List(Of clsCanaleEsportato)
    Dim Valori As New List(Of Double())
    For Giro As Integer = 1 To 3
      Dim Nuovi As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.Where(Function(c) c IsNot Nothing AndAlso Not Fatti.Contains(c)).ToList
      If Nuovi.Count = 0 Then Exit For
      For Each Ch As clsChannel2020 In Nuovi
        Fatti.Add(Ch)
        Dim E As New clsCanaleEsportato With {.ChannelId = Ch.ChannelId, .CanaleChiave = Ch.CanaleChiave.ToString, .ShortName = Ch.ShortName, .LongName = Ch.LongName,
                                              .ShortUM = Ch.ShortUM, .Decimals = Ch.Decimals, .DataType = Ch.DataType.ToString, .IsMath = Ch.IsMath}
        Dim V() As Double = Nothing
        If DaNonForzare.Contains(Ch.CanaleChiave) Then
          E.Stato = "skipped (obsolete calculation)"
        Else
          Try
            Dim C As clsChannel2020 = DataProvider2020.CanaleDbl(Ch)
            V = If(C Is Nothing, Nothing, C.Valori)
            If V Is Nothing OrElse V.Length <= 1 Then E.Stato = "missing"
          Catch ex As Exception
            E.Stato = "error: " & ex.Message
            V = Nothing
          End Try
        End If
        Dim Riga(A - Da) As Double
        Dim Validi As Integer = 0
        For i As Integer = Da To A
          Dim x As Double = If(V IsNot Nothing AndAlso V.Length > 1 AndAlso i < V.Length, V(i), Double.NaN)
          Riga(i - Da) = x
          If Not Double.IsNaN(x) Then Validi += 1
        Next
        E.ValoriValidi = Validi
        Elenco.Add(E)
        Valori.Add(Riga)
      Next
    Next

    ' 2. istanti e valori, compressi
    Using Fs As New FileStream(Path.Combine(Cartella, "timestamps.bin.gz"), FileMode.Create), Gz As New GZipStream(Fs, CompressionLevel.Optimal), Bw As New BinaryWriter(Gz)
      For i As Integer = Da To A
        Bw.Write(Ts(i).Ticks)
      Next
    End Using
    Using Fs As New FileStream(Path.Combine(Cartella, "channels.bin.gz"), FileMode.Create), Gz As New GZipStream(Fs, CompressionLevel.Optimal), Bw As New BinaryWriter(Gz)
      For Each Riga() As Double In Valori
        For Each x As Double In Riga
          Bw.Write(x)
        Next
      Next
    End Using

    ' 3. descrizione
    Dim P As clsProfile2021 = AppConfig.ActiveProfile
    Dim Meta As New Dictionary(Of String, Object) From {
      {"Format", "Performance2021 reference v1"},
      {"AppVersion", Versione},
      {"ExportedAt", Now.ToString("yyyy-MM-dd HH:mm:ss")},
      {"Profile", If(P Is Nothing, "", P.ProfileName)},
      {"Files", DataProvider2020.Files.Select(Function(f) f.FullName).ToList},
      {"Hz", DataProvider2020.Hz},
      {"TotalRows", N},
      {"FirstRow", Da},
      {"LastRow", A},
      {"ExportedRows", A - Da + 1},
      {"FirstTime", Ts(Da).ToString("yyyy-MM-dd HH:mm:ss.fff")},
      {"LastTime", Ts(A).ToString("yyyy-MM-dd HH:mm:ss.fff")},
      {"InertialLegacyWindows", P IsNot Nothing AndAlso P.InertialLegacyWindows},
      {"InertialSowSeconds", If(P Is Nothing, Double.NaN, P.InertialSowSeconds)},
      {"InertiaTauSec", clsInerzia.TauCorrente()},
      {"MaxLeewayForRecalc", If(P Is Nothing, Double.NaN, P.MaxLeewayForRecalc)},
      {"TargetLoaded", TgtManager IsNot Nothing AndAlso TgtManager.Tgt IsNot Nothing},
      {"Channels", Elenco}}
    File.WriteAllText(Path.Combine(Cartella, "meta.json"), JsonConvert.SerializeObject(Meta, Formatting.Indented))

    ' 4. periodi
    Dim Periodi As New Dictionary(Of String, Object)
    If PeriodsManager IsNot Nothing Then
      Periodi("StraightLine") = DescriviPeriodi(PeriodsManager.CollectionStraightLine)
      Periodi("StraightLineVmg") = DescriviPeriodi(PeriodsManager.CollectionStraightLineVmg)
      Periodi("StraightLineReaching") = DescriviPeriodi(PeriodsManager.CollectionStraightLineReaching)
      Periodi("Pavarot") = DescriviPeriodi(PeriodsManager.CollectionPavarot)
      Periodi("Tack") = DescriviPeriodi(PeriodsManager.CollectionTack)
      Periodi("Gybe") = DescriviPeriodi(PeriodsManager.CollectionGybe)
    End If
    File.WriteAllText(Path.Combine(Cartella, "periods.json"), JsonConvert.SerializeObject(Periodi, Formatting.Indented))

    ' 5. Feed Forward e sensitivita' con il report di lavoro (tutte le righe, nessun filtro: verifica del motore)
    File.WriteAllText(Path.Combine(Cartella, "feedforward.json"), JsonConvert.SerializeObject(FeedForward(), Formatting.Indented))
    Return Cartella
  End Function

  Private Shared Function DescriviPeriodi(Lista As IEnumerable(Of clsPeriod2021)) As List(Of Object)
    Dim Out As New List(Of Object)
    If Lista Is Nothing Then Return Out
    For Each Pd As clsPeriod2021 In Lista
      If Pd Is Nothing OrElse Pd.TR Is Nothing Then Continue For
      Out.Add(New Dictionary(Of String, Object) From {
        {"Start", Pd.TR.Start.ToString("yyyy-MM-dd HH:mm:ss.fff")},
        {"Finish", Pd.TR.Finish.ToString("yyyy-MM-dd HH:mm:ss.fff")},
        {"KeyMoment", If(Pd.KeyMoment.Ticks = 0, "", Pd.KeyMoment.ToString("yyyy-MM-dd HH:mm:ss.fff"))},
        {"Type", Pd.PeriodType.ToString},
        {"IsStbd", Pd.IsStbd},
        {"AvgTwa", Pd.AvgTwa},
        {"Description", Pd.ShortDescription}})
    Next
    Return Out
  End Function

  Private Shared Function FeedForward() As Object
    Dim Out As New Dictionary(Of String, Object)
    Dim S As clsFeedForwardReport = If(AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing, Nothing, AppConfig.ActiveProfile.FeedForwardSettings)
    If S Is Nothing OrElse String.IsNullOrEmpty(S.XChannelId) OrElse String.IsNullOrEmpty(S.TwsChannelId) Then
      Out("Note") = "no working Feed Forward report in the profile"
      Return Out
    End If
    Dim Cx As clsChannel2020 = DataProvider2020.CanaleDbl(S.XChannelId)
    Dim Ct As clsChannel2020 = DataProvider2020.CanaleDbl(S.TwsChannelId)
    If Cx Is Nothing OrElse Ct Is Nothing OrElse Cx.Valori Is Nothing OrElse Ct.Valori Is Nothing Then
      Out("Note") = "X or band channel not available"
      Return Out
    End If
    Out("Parameters") = S.Parametri
    Out("XChannelId") = S.XChannelId
    Out("BandChannelId") = S.TwsChannelId
    Out("Rows") = "all rows, no filter"
    Dim Canali As New List(Of Object)
    For Each Id As String In S.YChannelIds
      Dim Cy As clsChannel2020 = DataProvider2020.CanaleDbl(Id)
      If Cy Is Nothing OrElse Cy.Valori Is Nothing Then Continue For
      Dim N As Integer = Math.Min(Cx.Valori.Length, Math.Min(Cy.Valori.Length, Ct.Valori.Length))
      Dim Righe As IEnumerable(Of Integer) = Enumerable.Range(0, N)
      Dim Fasce As List(Of clsFfFascia) = clsFeedForward.Calcola(Cx.Valori, Cy.Valori, Ct.Valori, Righe, Nothing, S.Parametri)
      Dim Sens As List(Of clsFfSensitivita) = clsFeedForward.Sensitivita(Cx.Valori, Cy.Valori, Ct.Valori, Righe, Nothing, S.Parametri)
      Canali.Add(New Dictionary(Of String, Object) From {
        {"YChannelId", Id},
        {"Bands", Fasce.Select(Function(f) New Dictionary(Of String, Object) From {
            {"TwsLow", f.TwsLow}, {"TwsHigh", f.TwsHigh}, {"Samples", f.Campioni}, {"Message", f.Messaggio},
            {"Bins", f.Bins},
            {"Optimum", f.Ottimo},
            {"CurveX", f.CurvaX}, {"CurveY", f.CurvaY}}).ToList},
        {"Sensitivity", Sens.Select(Function(q) New Dictionary(Of String, Object) From {
            {"TwsLow", q.TwsLow}, {"Samples", q.Campioni}, {"TypicalChange", q.VariazioneTipica}, {"Sensitivity", q.Sensitivita},
            {"ExplainedShare", q.QuotaSpiegata}, {"Best", q.ValoreMigliore}, {"BestGain", q.GuadagnoMigliore}, {"BestAtEdge", q.MiglioreAlBordo},
            {"Message", q.Messaggio}}).ToList}})
    Next
    Out("Channels") = Canali
    Return Out
  End Function

End Class
