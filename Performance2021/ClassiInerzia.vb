''' <summary>
''' Inerzia della barca per i canali Inertial (metodo predefinito, InertialLegacyWindows = False): la velocita' tende a quella che le condizioni attuali possono sostenere con una costante di tempo
''' Tau (modello del primo ordine dV/dt = (Veq - V) / Tau), quindi la velocita' verso cui la barca sta andando e' Veq = V + Tau * dV/dt.
''' Un'orzata che per qualche secondo tiene la velocita' fa calare dV/dt e quindi Veq: la Vmg calcolata con Veq non premia piu' il guadagno fittizio.
''' Impostazioni nel profilo: dislocamento, velocita' e resistenza di riferimento (per il Tau di default), Tau in uso e sua origine.
''' </summary>
Public Class clsInerziaSettings
  ''' <summary>Dislocamento in kg.</summary>
  Public Property DislocamentoKg As Double = 10000
  ''' <summary>Velocita' tipica di bolina in nodi (riferimento per il Tau di default).</summary>
  Public Property VelocitaRiferimentoKn As Double = 7
  ''' <summary>Resistenza all'avanzamento alla velocita' di riferimento in N (dal VPP); 0 = stimata come RapportoResistenzaPeso del peso.</summary>
  Public Property ResistenzaN As Double = 0
  ''' <summary>Costante di tempo in uso (s).</summary>
  Public Property TauSec As Double = Double.NaN
  ''' <summary>Origine del Tau in uso: default dal dislocamento, stimato dalle manovre o corretto a mano (testo per l'utente).</summary>
  Public Property Origine As String = clsInerzia.OrigineDefault

  Public Function Clona() As clsInerziaSettings
    Return DirectCast(Me.MemberwiseClone(), clsInerziaSettings)
  End Function
End Class

''' <summary>Esito del fit del recupero di velocita' dopo una manovra (virata o strambata).</summary>
Public Class clsFitManovra
  Public Property Momento As DateTime
  Public Property IsTack As Boolean
  Public Property Tws As Double = Double.NaN
  ''' <summary>Indice della riga del key moment e tempo (s, stessa scala di T) oltre il quale il recupero non si considera (manovra successiva).</summary>
  Public Property IndiceKeyMoment As Integer
  Public Property TempoLimite As Double = Double.PositiveInfinity
  Public Property Tau As Double = Double.NaN
  ''' <summary>Velocita' al minimo dopo la manovra e velocita' a cui tende (dal fit), recupero = differenza.</summary>
  Public Property VMin As Double = Double.NaN
  Public Property VFinale As Double = Double.NaN
  Public Property Recupero As Double = Double.NaN
  Public Property R2 As Double = Double.NaN
  ''' <summary>Durata del tratto di recupero usato nel fit (s).</summary>
  Public Property Durata As Double
  Public Property Accettata As Boolean
  Public Property Motivo As String = ""

  Public ReadOnly Property Tipo As String
    Get
      Return If(IsTack, "Tack", "Gybe")
    End Get
  End Property
End Class

Public Class clsInerzia

  Public Const OrigineDefault As String = "default from displacement"
  ''' <summary>Resistenza / peso tipica di una barca a chiglia di bolina, usata se la resistenza non e' indicata.</summary>
  Public Const RapportoResistenzaPeso As Double = 0.025
  ''' <summary>Massa d'acqua trascinata, in frazione del dislocamento.</summary>
  Public Const MassaAggiunta As Double = 0.1
  ''' <summary>Mezza finestra (s) della pendenza della velocita' usata per dV/dt.</summary>
  Public Const MezzaFinestraDerivataSec As Double = 2
  Private Const G As Double = 9.80665
  Private Const NodiInMs As Double = 0.514444

  ''' <summary>Impostazioni del profilo attivo (create con il Tau di default se mancano).</summary>
  Public Shared Function Impostazioni() As clsInerziaSettings
    If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Return New clsInerziaSettings With {.TauSec = TauDefault(New clsInerziaSettings)}
    If AppConfig.ActiveProfile.Inerzia Is Nothing Then
      Dim S As New clsInerziaSettings
      S.TauSec = TauDefault(S)
      AppConfig.ActiveProfile.Inerzia = S
    End If
    If Double.IsNaN(AppConfig.ActiveProfile.Inerzia.TauSec) OrElse AppConfig.ActiveProfile.Inerzia.TauSec <= 0 Then
      AppConfig.ActiveProfile.Inerzia.TauSec = TauDefault(AppConfig.ActiveProfile.Inerzia)
      AppConfig.ActiveProfile.Inerzia.Origine = OrigineDefault
    End If
    Return AppConfig.ActiveProfile.Inerzia
  End Function

  Public Shared Function TauCorrente() As Double
    Return Impostazioni().TauSec
  End Function

  ''' <summary>
  ''' Tau di default: Tau = (1 + massa aggiunta) * m * V / (2 * D), dalla linearizzazione di una resistenza proporzionale a V^2 (dD/dV = 2D/V).
  ''' Senza resistenza indicata D = RapportoResistenzaPeso * m * g: allora il dislocamento si semplifica e Tau dipende solo dalla velocita'
  ''' (circa 1.15 s per nodo); con la resistenza del VPP il dislocamento conta davvero.
  ''' </summary>
  Public Shared Function TauDefault(S As clsInerziaSettings) As Double
    Dim m As Double = S.DislocamentoKg
    Dim V As Double = S.VelocitaRiferimentoKn * NodiInMs
    If m <= 0 OrElse V <= 0 Then Return 8
    Dim D As Double = If(S.ResistenzaN > 0, S.ResistenzaN, RapportoResistenzaPeso * m * G)
    Return (1 + MassaAggiunta) * m * V / (2 * D)
  End Function

  ''' <summary>
  ''' Fit del recupero di velocita' dopo ogni manovra: dal minimo di velocita' nei 20 s dopo il key moment, per al massimo 60 s
  ''' (o fino alla manovra successiva, o a un buco nei dati), V(t) = VFinale - Recupero * exp(-(t - t0) / Tau).
  ''' Tau cercato su una griglia (0.2 s), per ogni Tau VFinale e Recupero sono un fit lineare. T = secondi di ogni riga, V = velocita' (kn) lisciata.
  ''' La manovra e' accettata se il recupero e' visibile (almeno 0.3 kn e il 5% della velocita'), l'adattamento e' buono (R2 >= 0.5),
  ''' Tau non e' ai bordi della griglia e il tratto dura almeno 2 Tau (altrimenti la costante non si vede).
  ''' </summary>
  Public Shared Sub Stima(T() As Double, V() As Double, Manovre As List(Of clsFitManovra))
    Dim n As Integer = Math.Min(T.Length, V.Length)
    For Each M As clsFitManovra In Manovre
      Dim k0 As Integer = M.IndiceKeyMoment
      If k0 < 0 OrElse k0 >= n OrElse Double.IsNaN(T(k0)) Then
        M.Motivo = "no data at the key moment"
        Continue For
      End If
      ' minimo di velocita' nei 20 s dopo il key moment
      Dim iMin As Integer = -1
      Dim k As Integer = k0
      While k < n AndAlso Not Double.IsNaN(T(k)) AndAlso T(k) <= T(k0) + 20 AndAlso T(k) < M.TempoLimite
        If Not Double.IsNaN(V(k)) AndAlso (iMin < 0 OrElse V(k) < V(iMin)) Then iMin = k
        k += 1
      End While
      If iMin < 0 Then
        M.Motivo = "no speed after the manoeuvre"
        Continue For
      End If
      ' tratto di recupero: fino a 60 s, alla manovra successiva o a un buco di dati
      Dim t0 As Double = T(iMin)
      Dim Fine As Double = Math.Min(t0 + 60, M.TempoLimite)
      Dim Xs As New List(Of Double), Ys As New List(Of Double)
      k = iMin
      While k < n AndAlso Not Double.IsNaN(T(k)) AndAlso T(k) <= Fine
        If k > iMin AndAlso T(k) - T(k - 1) > 2 Then Exit While
        If Not Double.IsNaN(V(k)) Then
          Xs.Add(T(k) - t0)
          Ys.Add(V(k))
        End If
        k += 1
      End While
      M.Durata = If(Xs.Count > 0, Xs.Last, 0)
      If Xs.Count < 10 OrElse M.Durata < 15 Then
        M.Motivo = "recovery too short (" & M.Durata.ToString("0") & " s: next manoeuvre or data gap)"
        Continue For
      End If

      Dim Ym As Double = Ys.Average
      Dim Sst As Double = Ys.Sum(Function(q) (q - Ym) * (q - Ym))
      Dim MigliorSse As Double = Double.MaxValue, MigliorTau As Double = Double.NaN, MigliorA As Double = 0, MigliorB As Double = 0
      For Tau As Double = 1 To 40 Step 0.2
        ' V = a + b * x con x = 1 - exp(-t / Tau): minimi quadrati lineari
        Dim Sx As Double = 0, Sy As Double = 0, Sxx As Double = 0, Sxy As Double = 0
        Dim Nn As Integer = Xs.Count
        For j As Integer = 0 To Nn - 1
          Dim x As Double = 1 - Math.Exp(-Xs(j) / Tau)
          Sx += x
          Sy += Ys(j)
          Sxx += x * x
          Sxy += x * Ys(j)
        Next
        Dim Den As Double = Nn * Sxx - Sx * Sx
        If Math.Abs(Den) < 0.000000001 Then Continue For
        Dim b As Double = (Nn * Sxy - Sx * Sy) / Den
        Dim a As Double = (Sy - b * Sx) / Nn
        Dim Sse As Double = 0
        For j As Integer = 0 To Nn - 1
          Dim e As Double = Ys(j) - (a + b * (1 - Math.Exp(-Xs(j) / Tau)))
          Sse += e * e
        Next
        If Sse < MigliorSse Then
          MigliorSse = Sse
          MigliorTau = Tau
          MigliorA = a
          MigliorB = b
        End If
      Next
      If Double.IsNaN(MigliorTau) Then
        M.Motivo = "fit not possible"
        Continue For
      End If
      M.Tau = MigliorTau
      M.VMin = MigliorA
      M.Recupero = MigliorB
      M.VFinale = MigliorA + MigliorB
      M.R2 = If(Sst > 0, 1 - MigliorSse / Sst, Double.NaN)

      If MigliorB < Math.Max(0.3, 0.05 * M.VFinale) Then
        M.Motivo = "speed recovery too small (" & MigliorB.ToString("0.00") & " kn)"
      ElseIf Double.IsNaN(M.R2) OrElse M.R2 < 0.5 Then
        M.Motivo = "poor fit (R2 " & M.R2.ToString("0.00") & ")"
      ElseIf MigliorTau <= 1.5 OrElse MigliorTau >= 39 Then
        M.Motivo = "tau at the edge of the search range"
      ElseIf M.Durata < 2 * MigliorTau Then
        M.Motivo = "recovery shorter than 2 tau"
      Else
        M.Accettata = True
        M.Motivo = "ok"
      End If
    Next
  End Sub

  ''' <summary>Mediana (NaN se la lista e' vuota).</summary>
  Public Shared Function Mediana(Valori As IEnumerable(Of Double)) As Double
    Dim L As List(Of Double) = Valori.Where(Function(q) Not Double.IsNaN(q)).OrderBy(Function(q) q).ToList
    If L.Count = 0 Then Return Double.NaN
    If L.Count Mod 2 = 1 Then Return L(L.Count \ 2)
    Return (L(L.Count \ 2 - 1) + L(L.Count \ 2)) / 2
  End Function

  ''' <summary>Quantile semplice (interpolato) per l'intervallo tipico dei Tau stimati.</summary>
  Public Shared Function Quantile(Valori As IEnumerable(Of Double), Q As Double) As Double
    Dim L As List(Of Double) = Valori.Where(Function(v) Not Double.IsNaN(v)).OrderBy(Function(v) v).ToList
    If L.Count = 0 Then Return Double.NaN
    Dim Pos As Double = Q * (L.Count - 1)
    Dim i As Integer = CInt(Math.Floor(Pos))
    If i >= L.Count - 1 Then Return L.Last
    Return L(i) + (Pos - i) * (L(i + 1) - L(i))
  End Function

End Class
