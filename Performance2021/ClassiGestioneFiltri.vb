Imports Newtonsoft.Json

''' <summary>Tipo di nodo del filtro: una condizione su un canale oppure un gruppo di nodi legati da AND / OR.</summary>
Public Enum eFiltroTipoNodo
  Condizione = 0
  Gruppo = 1
End Enum

''' <summary>Operatore con cui un gruppo combina i suoi figli.</summary>
Public Enum eFiltroOperatoreGruppo
  OpAnd = 0
  OpOr = 1
End Enum

''' <summary>
''' Nodo dell'albero di un filtro. Una sola classe (senza ereditarieta') per avere un JSON semplice nel profilo:
''' Tipo = Condizione usa Canale / AbsVal / UsaMin / UsaMax / Min / Max, Tipo = Gruppo usa Operatore e Figli.
''' Attivo = False esclude il nodo (e i suoi figli) senza cancellarlo; Negato inverte il risultato del nodo.
''' </summary>
Public Class clsFiltroNodo
  Public Property Tipo As eFiltroTipoNodo = eFiltroTipoNodo.Condizione
  Public Property Attivo As Boolean = True
  Public Property Negato As Boolean = False

  ' --- condizione
  Public Property ChannelId As String = ""
  Public Property AbsVal As Boolean = False
  Public Property UsaMin As Boolean = False
  Public Property UsaMax As Boolean = False
  Public Property Min As Double = 0
  Public Property Max As Double = 0

  ' --- gruppo
  Public Property Operatore As eFiltroOperatoreGruppo = eFiltroOperatoreGruppo.OpAnd
  Public Property Figli As New List(Of clsFiltroNodo)

  Public Shared Function NuovoGruppo(Op As eFiltroOperatoreGruppo) As clsFiltroNodo
    Return New clsFiltroNodo With {.Tipo = eFiltroTipoNodo.Gruppo, .Operatore = Op}
  End Function

  Public Shared Function NuovaCondizione(ChannelId As String, AbsVal As Boolean, UsaMin As Boolean, UsaMax As Boolean, Min As Double, Max As Double) As clsFiltroNodo
    Return New clsFiltroNodo With {.Tipo = eFiltroTipoNodo.Condizione, .ChannelId = ChannelId, .AbsVal = AbsVal, .UsaMin = UsaMin, .UsaMax = UsaMax, .Min = Min, .Max = Max}
  End Function

  Public Function Clona() As clsFiltroNodo
    Dim N As clsFiltroNodo = DirectCast(Me.MemberwiseClone(), clsFiltroNodo)
    N.Figli = New List(Of clsFiltroNodo)(Figli.Select(Function(x) x.Clona()))
    Return N
  End Function
End Class

''' <summary>Filtro con un nome, salvato nel profilo e riutilizzabile. Radice e' sempre un gruppo.</summary>
Public Class clsFiltroNominato
  Public Property Nome As String = ""
  Public Property Note As String = ""
  Public Property Radice As clsFiltroNodo = clsFiltroNodo.NuovoGruppo(eFiltroOperatoreGruppo.OpAnd)

  Public Function Clona() As clsFiltroNominato
    Return New clsFiltroNominato With {.Nome = Nome, .Note = Note, .Radice = Radice.Clona()}
  End Function

  Public Overrides Function ToString() As String
    Return Nome
  End Function
End Class

''' <summary>Risultato della compilazione di un filtro sul dataset caricato.</summary>
Public Class clsFiltroCompilato
  ''' <summary>Dice se la riga passa il filtro. Nothing = nessuna condizione attiva (tutti i campioni validi).</summary>
  Public Property Maschera As Func(Of Integer, Boolean)
  Public Property Descrizione As String = ""
  ''' <summary>Firma esatta del filtro (valori non arrotondati) per capire se e' cambiato.</summary>
  Public Property Firma As String = ""
  ''' <summary>Canali delle condizioni ignorate perche' assenti nel dataset caricato.</summary>
  Public Property Ignorate As New List(Of String)
End Class

''' <summary>
''' Compilazione e libreria dei filtri. La regola sul singolo valore e' sempre clsFiltroCampioniXY.ValoreValido (unica per l'applicazione).
''' Condizioni senza min/max, senza canale o con canale assente nel dataset sono ignorate (come nel filtro XY Plots);
''' un gruppo senza figli utili non vincola nulla.
''' La compilazione legge i canali: va eseguita sul thread UI (lettura lazy dei Parquet).
''' </summary>
Public Class clsFiltri

  Public Const NomeFiltroXyMigrato As String = "XY Plots (profile)"

  ' ---------------------------------------------------------------- libreria nel profilo

  ''' <summary>Elenco dei filtri salvati del profilo attivo. Se non esiste ancora (profili precedenti) viene creato con i filtri attuali di XY Plots.</summary>
  Public Shared Function Elenco() As List(Of clsFiltroNominato)
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return New List(Of clsFiltroNominato)
    Dim P As clsProfile2021 = AppConfig.ActiveProfile
    If P.FiltriSalvati Is Nothing Then
      P.FiltriSalvati = New List(Of clsFiltroNominato)
      Dim Migrato As clsFiltroNominato = DaImpostazioniXY(P.XYPlotSettings)
      If Migrato IsNot Nothing Then
        P.FiltriSalvati.Add(Migrato)
        ' il filtro migrato diventa quello scelto; i due filtri classici restano nel profilo ma non sono piu' applicati (fallback nascosto)
        If P.XYPlotSettings IsNot Nothing AndAlso String.IsNullOrWhiteSpace(P.XYPlotSettings.NamedFilter) Then
          P.XYPlotSettings.NamedFilter = Migrato.Nome
          P.XYPlotSettings.ApplyFilter1 = False
          P.XYPlotSettings.ApplyFilter2 = False
        End If
      End If
    End If
    Return P.FiltriSalvati
  End Function

  Public Shared Function Trova(Nome As String) As clsFiltroNominato
    Return Elenco().FirstOrDefault(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase))
  End Function

  ''' <summary>Aggiunge o sostituisce (stesso nome, senza distinguere maiuscole) il filtro nella libreria del profilo. Non salva il profilo su disco.</summary>
  Public Shared Sub Salva(F As clsFiltroNominato)
    If F Is Nothing OrElse String.IsNullOrWhiteSpace(F.Nome) Then Exit Sub
    Dim L As List(Of clsFiltroNominato) = Elenco()
    Dim Idx As Integer = L.FindIndex(Function(x) String.Equals(x.Nome, F.Nome, StringComparison.OrdinalIgnoreCase))
    If Idx >= 0 Then L(Idx) = F.Clona() Else L.Add(F.Clona())
  End Sub

  Public Shared Sub Elimina(Nome As String)
    Elenco().RemoveAll(Function(x) String.Equals(x.Nome, Nome, StringComparison.OrdinalIgnoreCase))
  End Sub

  ''' <summary>Converte i due filtri in AND del tab XY Plots in un filtro nominato. Nothing se non c'e' nessun filtro con canale e min/max.</summary>
  Public Shared Function DaImpostazioniXY(S As clsXYPlotSettings) As clsFiltroNominato
    If S Is Nothing Then Return Nothing
    Dim F As New clsFiltroNominato With {.Nome = NomeFiltroXyMigrato}
    Dim Id1 As String = If(S.FilterChannel IsNot Nothing, S.FilterChannel.ChannelId, S.FilterChannelName)
    Dim Id2 As String = If(S.Filter2Channel IsNot Nothing, S.Filter2Channel.ChannelId, S.Filter2ChannelName)
    If S.ApplyFilter1 AndAlso (S.ApplyFilterMin OrElse S.ApplyFilterMax) AndAlso Not String.IsNullOrWhiteSpace(Id1) Then
      F.Radice.Figli.Add(clsFiltroNodo.NuovaCondizione(Id1, S.ApplyFilterAbsVal, S.ApplyFilterMin, S.ApplyFilterMax, S.FilterValueMin, S.FilterValueMax))
    End If
    If S.ApplyFilter2 AndAlso (S.ApplyFilter2Min OrElse S.ApplyFilter2Max) AndAlso Not String.IsNullOrWhiteSpace(Id2) Then
      F.Radice.Figli.Add(clsFiltroNodo.NuovaCondizione(Id2, S.ApplyFilter2AbsVal, S.ApplyFilter2Min, S.ApplyFilter2Max, S.Filter2ValueMin, S.Filter2ValueMax))
    End If
    If F.Radice.Figli.Count = 0 Then Return Nothing
    Return F
  End Function

  ''' <summary>
  ''' Compila il filtro con nome scelto nelle impostazioni XY (NamedFilter). Nothing se non ne e' scelto nessuno o se non esiste piu'
  ''' nel profilo: in quel caso valgono i due filtri classici. Thread UI (legge i canali).
  ''' </summary>
  Public Shared Function CompilaPerImpostazioni(S As clsXYPlotSettings) As clsFiltroCompilato
    Dim F As clsFiltroNominato = RisolviPerImpostazioni(S)
    If F Is Nothing Then Return Nothing
    Return Compila(F)
  End Function

  ''' <summary>
  ''' Filtro con nome da usare per le impostazioni: la copia (snapshot) del plot di report se c'e' ed e' dello stesso nome, altrimenti quello
  ''' del profilo. Se la scelta nella combo e' cambiata la copia viene rifatta dalla libreria. Nothing se nessun filtro e' scelto o non esiste.
  ''' </summary>
  Public Shared Function RisolviPerImpostazioni(S As clsXYPlotSettings) As clsFiltroNominato
    If S Is Nothing Then Return Nothing
    Return Risolvi(S.NamedFilter, S.NamedFilterSnapshot)
  End Function

  ''' <summary>
  ''' Filtro da usare dato il nome scelto e la copia (snapshot) salvata con il report: la copia se e' dello stesso nome, altrimenti quello del profilo
  ''' (e la copia viene rifatta da questo). Nothing se il nome e' vuoto o il filtro non esiste.
  ''' </summary>
  Public Shared Function Risolvi(Nome As String, ByRef Snapshot As clsFiltroNominato) As clsFiltroNominato
    Elenco() ' garantisce la migrazione dei filtri classici nei profili precedenti
    If String.IsNullOrWhiteSpace(Nome) Then Return Nothing
    If Snapshot IsNot Nothing Then
      If String.Equals(Snapshot.Nome, Nome, StringComparison.OrdinalIgnoreCase) Then Return Snapshot
      Dim Nuovo As clsFiltroNominato = Trova(Nome)
      Snapshot = If(Nuovo Is Nothing, Nothing, Nuovo.Clona())
      Return Nuovo
    End If
    Return Trova(Nome)
  End Function

  ''' <summary>Descrizione testuale del filtro per titoli e report (non legge i dati, usa il nome breve dei canali se il dataset e' caricato).</summary>
  Public Shared Function DescrizioneBreve(F As clsFiltroNominato) As String
    If F Is Nothing Then Return ""
    Dim T As String = TestoNodo(F.Radice, True)
    If String.IsNullOrEmpty(T) Then Return "Filter '" & F.Nome & "': none"
    Return "Filter '" & F.Nome & "': " & T
  End Function

  Private Shared Function TestoNodo(N As clsFiltroNodo, Radice As Boolean) As String
    If N Is Nothing OrElse Not N.Attivo Then Return ""
    Dim T As String
    If N.Tipo = eFiltroTipoNodo.Condizione Then
      If Not (N.UsaMin OrElse N.UsaMax) OrElse String.IsNullOrWhiteSpace(N.ChannelId) Then Return ""
      Dim Ch As clsChannel2020 = Nothing
      If DataProvider2020 IsNot Nothing AndAlso DataProvider2020.Channels IsNot Nothing Then Ch = DataProvider2020.Channels.Canale(N.ChannelId)
      Dim Nome As String = If(Ch Is Nothing, N.ChannelId, Ch.ShortName)
      Dim F As String = "F" & If(Ch Is Nothing, 2, System.Math.Max(0, Ch.Decimals)).ToString
      T = If(N.AbsVal, "abs ", "") & Nome & " "
      If N.UsaMin AndAlso N.UsaMax Then
        T &= "between " & N.Min.ToString(F) & " and " & N.Max.ToString(F)
      ElseIf N.UsaMin Then
        T &= ">= " & N.Min.ToString(F)
      Else
        T &= "<= " & N.Max.ToString(F)
      End If
    Else
      Dim Parti As List(Of String) = N.Figli.Select(Function(x) TestoNodo(x, False)).Where(Function(x) x <> "").ToList
      If Parti.Count = 0 Then Return ""
      T = String.Join(If(N.Operatore = eFiltroOperatoreGruppo.OpAnd, " and ", " or "), Parti)
      If Not Radice AndAlso Parti.Count > 1 Then T = "(" & T & ")"
    End If
    If N.Negato Then T = "not (" & T & ")"
    Return T
  End Function

  ' ---------------------------------------------------------------- compilazione

  ''' <summary>Compila il filtro sul dataset caricato. Maschera = Nothing se non c'e' nessuna condizione attiva.</summary>
  Public Shared Function Compila(F As clsFiltroNominato) As clsFiltroCompilato
    Dim R As New clsFiltroCompilato
    If F Is Nothing OrElse F.Radice Is Nothing OrElse DataProvider2020 Is Nothing Then
      R.Descrizione = "No filter is set: all the samples are used."
      Return R
    End If
    Dim Testo As String = Nothing
    Dim Sig As String = Nothing
    R.Maschera = CompilaNodo(F.Radice, R.Ignorate, Testo, Sig, True)
    R.Firma = If(Sig, "") & "#" & String.Join(",", R.Ignorate)
    If R.Maschera Is Nothing Then
      R.Descrizione = "Filter '" & F.Nome & "' has no active condition: all the samples are used."
    Else
      R.Descrizione = "Filter '" & F.Nome & "': only the samples with " & Testo & "."
    End If
    If R.Ignorate.Count > 0 Then R.Descrizione &= " Ignored (channel not available in this dataset): " & String.Join(", ", R.Ignorate) & "."
    Return R
  End Function

  Private Shared Function CompilaNodo(N As clsFiltroNodo, Ignorate As List(Of String), ByRef Testo As String, ByRef Firma As String, Radice As Boolean) As Func(Of Integer, Boolean)
    Testo = Nothing
    Firma = Nothing
    If N Is Nothing OrElse Not N.Attivo Then Return Nothing

    Dim Funz As Func(Of Integer, Boolean) = Nothing
    If N.Tipo = eFiltroTipoNodo.Condizione Then
      If Not (N.UsaMin OrElse N.UsaMax) OrElse String.IsNullOrWhiteSpace(N.ChannelId) Then Return Nothing
      Dim Ch As clsChannel2020 = DataProvider2020.CanaleDbl(N.ChannelId)
      If Ch Is Nothing OrElse Ch.Valori Is Nothing OrElse Ch.Valori.Length = 0 Then
        Ignorate.Add("'" & N.ChannelId & "'")
        Return Nothing
      End If
      Dim Vals As Double() = Ch.Valori
      Dim A As Boolean = N.AbsVal, UMin As Boolean = N.UsaMin, UMax As Boolean = N.UsaMax, Mn As Double = N.Min, Mx As Double = N.Max
      Funz = Function(i As Integer) As Boolean
               If i < 0 OrElse i >= Vals.Length Then Return False
               Return clsFiltroCampioniXY.ValoreValido(Vals(i), A, UMin, UMax, Mn, Mx)
             End Function
      Dim F As String = "F" & System.Math.Max(0, Ch.Decimals).ToString
      Testo = If(N.AbsVal, "abs value of ", "") & "'" & Ch.ShortName & "' "
      If N.UsaMin AndAlso N.UsaMax Then
        Testo &= "between " & N.Min.ToString(F) & " and " & N.Max.ToString(F)
      ElseIf N.UsaMin Then
        Testo &= ">= " & N.Min.ToString(F)
      Else
        Testo &= "<= " & N.Max.ToString(F)
      End If
      Firma = Ch.ChannelId & ";" & N.AbsVal & ";" & N.UsaMin & ";" & N.UsaMax & ";" &
              N.Min.ToString("R", Globalization.CultureInfo.InvariantCulture) & ";" & N.Max.ToString("R", Globalization.CultureInfo.InvariantCulture)
    Else
      Dim Fn As New List(Of Func(Of Integer, Boolean))
      Dim Tx As New List(Of String)
      Dim Sg As New List(Of String)
      For Each Fi As clsFiltroNodo In N.Figli
        Dim t As String = Nothing, s As String = Nothing
        Dim f As Func(Of Integer, Boolean) = CompilaNodo(Fi, Ignorate, t, s, False)
        If f IsNot Nothing Then
          Fn.Add(f)
          Tx.Add(t)
          Sg.Add(s)
        End If
      Next
      If Fn.Count = 0 Then Return Nothing
      Dim Arr As Func(Of Integer, Boolean)() = Fn.ToArray
      Dim Sep As String = If(N.Operatore = eFiltroOperatoreGruppo.OpAnd, " and ", " or ")
      If N.Operatore = eFiltroOperatoreGruppo.OpAnd Then
        Funz = Function(i As Integer) As Boolean
                 For k As Integer = 0 To Arr.Length - 1
                   If Not Arr(k)(i) Then Return False
                 Next
                 Return True
               End Function
      Else
        Funz = Function(i As Integer) As Boolean
                 For k As Integer = 0 To Arr.Length - 1
                   If Arr(k)(i) Then Return True
                 Next
                 Return False
               End Function
      End If
      Testo = String.Join(Sep, Tx)
      ' le parentesi servono solo quando il gruppo e' annidato e ha piu' di un termine
      If Not Radice AndAlso Tx.Count > 1 Then Testo = "(" & Testo & ")"
      Firma = If(N.Operatore = eFiltroOperatoreGruppo.OpAnd, "A", "O") & "(" & String.Join(",", Sg) & ")"
    End If

    If N.Negato Then
      Dim Interno As Func(Of Integer, Boolean) = Funz
      Funz = Function(i As Integer) As Boolean
               Return Not Interno(i)
             End Function
      Testo = "not (" & Testo & ")"
      Firma = "N" & Firma
    End If
    Return Funz
  End Function

End Class
