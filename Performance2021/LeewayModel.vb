Imports PropertyChanged
Imports Newtonsoft.Json

' ================= Modello teorico del leeway =================
' Il leeway e' l'angolo di incidenza con cui gli appendici producono la forza laterale che equilibra quella delle vele:
'
'     lee = K * Kc * M / (V^2 * S)  -  (Ab * g(s) * fB / S) * (Gd * angolo_canard + D0)
'
'   M   momento sbandante (in gradi equivalenti): sin(heel) + R * sin(heel + cant chiglia)
'         R = momento raddrizzante della chiglia basculante rispetto a scafo+equipaggio
'   S   rigidezza laterale degli appendici (chiglia = 1): fK + Ab * g(s) * fB + Ar
'         fK = cos(heel + cant)^N   proiezione laterale della chiglia
'         fB = cos(heel)^N          proiezione laterale del canard
'         g(s) = a(s) / a(sRef)     area portante del canard in funzione dell'immersione,
'                                   a(s) = s / (1 + 2*c / (KAr*s))  (linea portante, allungamento = KAr*s/c)
'   Kc  correzione empirica per TWA, TWS e stato del mare
'   Il secondo termine e' l'effetto dell'angolo del canard: ha un'incidenza propria (angolo + leeway),
'   quindi sposta il leeway di equilibrio in proporzione al suo peso nella rigidezza totale.
'
' Con un solo appendice a rigidezza costante e M = heel si ricade nella formula classica k * heel / V^2.
' Tutti i coefficienti sono parametri salvati nel profilo (clsProfile2021.LeewayModel) e si possono adattare
' automaticamente a una serie di riferimento (LeewayModelForm).

''' <summary>Un coefficiente del modello: valore corrente, limiti e se partecipa all'adattamento automatico.</summary>
<AddINotifyPropertyChangedInterface>
Public Class clsLeewayParametro
  Public Property Nome As String
  Public Property Descrizione As String
  Public Property Valore As Double
  Public Property Minimo As Double
  Public Property Massimo As Double
  Public Property Predefinito As Double
  Public Property DaAdattare As Boolean

  Public Sub New()

  End Sub

  Public Sub New(Nome As String, Descrizione As String, Predefinito As Double, Minimo As Double, Massimo As Double, DaAdattare As Boolean)
    Me.Nome = Nome
    Me.Descrizione = Descrizione
    Me.Valore = Predefinito
    Me.Predefinito = Predefinito
    Me.Minimo = Minimo
    Me.Massimo = Massimo
    Me.DaAdattare = DaAdattare
  End Sub
End Class

''' <summary>Coefficienti del modello e impostazioni dei canali di ingresso, salvati nel profilo.</summary>
Public Class clsLeewayModelSettings
  ''' <summary>Nothing finche' non si chiama NormalizzaValoriMancanti (evita che il deserializzatore accodi ai default).</summary>
  Public Property Parametri As List(Of clsLeewayParametro)

  ''' <summary>Nome esatto del canale; vuoto = ricerca automatica (Canard/Dagger/Board + Angle).</summary>
  Public Property CanardAngleChannel As String = ""
  Public Property CanardImmersionChannel As String = ""
  ''' <summary>Il cant della chiglia non ha una ricerca automatica sicura: se vuoto si provano pochi nomi noti.</summary>
  Public Property KeelCantChannel As String = ""
  ''' <summary>True se il canale e' positivo a dritta (positivo sopravvento solo con TWA positivo). Il cant (KeelAng) dei log di questa barca e' gia' positivo sopravvento su entrambe le mure.</summary>
  Public Property CanardAngleSignedByTack As Boolean = False
  Public Property KeelCantSignedByTack As Boolean = False
  ''' <summary>Inverte il segno del cant dopo l'eventuale normalizzazione sulla mura (positivo = verso sopravvento).</summary>
  Public Property KeelCantInverted As Boolean = False
  ''' <summary>Serie di riferimento: 0 = Leeway Norm (loggato), 1 = Leeway Recalc (cog - hdg), 2 = Recalc 3s, 3 = Recalc 30s.</summary>
  Public Property Riferimento As Integer = 0
  ''' <summary>True = l'adattamento usa solo il range temporale visibile nei plot, False = tutto il file.</summary>
  Public Property SoloRangeVisibile As Boolean = True
  ''' <summary>Versione dei valori di partenza: se piu' vecchia di quella del codice i coefficienti tornano ai default aggiornati.</summary>
  Public Property VersioneParametri As Integer = 0
  Public Const VersioneParametriCorrente As Integer = 2

  Public Shared Function ParametriDefault() As List(Of clsLeewayParametro)
    Return New List(Of clsLeewayParametro) From {
      New clsLeewayParametro("K", "Overall gain (deg*kn^2). With R=0 and Ab=Ar=0 it is the classic k in k*heel/V^2 (7-15)", 20, 0.5, 40, True),
      New clsLeewayParametro("R", "Keel moment ratio: righting moment of the canted keel relative to hull + crew (0 = keel cant ignored; not supported by the 27-28 Sep 2026 data)", 0, -3, 3, False),
      New clsLeewayParametro("Ab", "Canard lateral stiffness relative to the keel (keel = 1) at the reference immersion", 0.6, 0, 3, True),
      New clsLeewayParametro("Ar", "Rudder lateral stiffness relative to the keel (kept constant)", 0.1, 0, 1, False),
      New clsLeewayParametro("Chord", "Canard chord, same unit as the immersion channel x ImmScale", 0.4, 0.05, 3, False),
      New clsLeewayParametro("SRef", "Canard reference immersion (immersion where g = 1), same unit as Chord. With the immersion in % and ImmScale 0.02, 2 = fully down", 2, 0.05, 10, False),
      New clsLeewayParametro("KAr", "Free surface effect on the canard aspect ratio (1 = no effect, 2 = full mirror effect)", 1, 0.5, 2.5, False),
      New clsLeewayParametro("N", "Exponent of the cosine projection of keel and canard on the heel angle", 1, 0, 3, False),
      New clsLeewayParametro("Gd", "Effectiveness of the canard angle channel as angle of attack (1 = the channel is the angle of attack offset; negative if the channel has the opposite sign)", 1, -2, 2, False),
      New clsLeewayParametro("D0", "Canard angle offset (deg): angle of attack with the channel at zero. Not identifiable from a constant bias: fit it only with data from a known leeway", 0, -5, 5, False),
      New clsLeewayParametro("CTwa", "Correction per 45 deg of |TWA| away from 45 deg: Kc = 1 + CTwa*(|TWA|-45)/45 + ...", 0, -2, 2, False),
      New clsLeewayParametro("CTws", "Correction per 12 kn of TWS away from 12 kn", 0, -2, 2, False),
      New clsLeewayParametro("CSea", "Correction per unit of the SeaState channel", 0, -1, 1, False),
      New clsLeewayParametro("ImmScale", "Scale applied to the canard immersion channel to bring it to the unit of Chord (the Board channel is in %: 0.02 makes 100% = 2 length units)", 0.02, 0.001, 1000, False),
      New clsLeewayParametro("VMin", "Minimum boat speed (kn): below it the model returns no value", 3, 0, 10, False),
      New clsLeewayParametro("LMax", "Maximum absolute leeway (deg) returned and used in the fit", 15, 1, 30, False)
    }
  End Function

  Public Sub CaricaValoriDefault()
    Parametri = ParametriDefault()
  End Sub

  ''' <summary>Aggiunge i coefficienti mancanti (profili salvati prima di una nuova versione) e riallinea le descrizioni.</summary>
  Public Sub NormalizzaValoriMancanti()
    If Parametri Is Nothing Then Parametri = New List(Of clsLeewayParametro)
    If VersioneParametri < VersioneParametriCorrente Then
      ' v2: unita' dell'immersione (Board e' in %), cant gia' normalizzato, fit limitato ai coefficienti supportati dai dati
      Parametri = ParametriDefault()
      KeelCantSignedByTack = False
      VersioneParametri = VersioneParametriCorrente
    End If
    For Each Def In ParametriDefault()
      Dim Esistente = Parametri.FirstOrDefault(Function(x) x.Nome = Def.Nome)
      If Esistente Is Nothing Then
        Parametri.Add(Def)
      Else
        Esistente.Descrizione = Def.Descrizione
        Esistente.Predefinito = Def.Predefinito
      End If
    Next
    ' ordine stabile come nei default
    Dim Ordinati = ParametriDefault().Select(Function(d) Parametri.First(Function(x) x.Nome = d.Nome)).ToList
    Parametri = Ordinati
  End Sub

  Public Function Clona() As clsLeewayModelSettings
    Dim Copia = JsonConvert.DeserializeObject(Of clsLeewayModelSettings)(JsonConvert.SerializeObject(Me))
    Copia.NormalizzaValoriMancanti()
    Return Copia
  End Function

  Public Function Coefficienti() As clsLeewayCoeff
    Return clsLeewayCoeff.Da(Parametri.ToDictionary(Function(p) p.Nome, Function(p) p.Valore))
  End Function
End Class

''' <summary>Coefficienti come campi numerici: la valutazione per campione non deve cercare nei dizionari.</summary>
Public Class clsLeewayCoeff
  Public K, R, Ab, Ar, Chord, SRef, KAr, N, Gd, D0, CTwa, CTws, CSea, ImmScale, VMin, LMax As Double

  Public Shared Function Da(P As Dictionary(Of String, Double)) As clsLeewayCoeff
    Dim C As New clsLeewayCoeff
    C.K = P("K") : C.R = P("R") : C.Ab = P("Ab") : C.Ar = P("Ar")
    C.Chord = P("Chord") : C.SRef = P("SRef") : C.KAr = P("KAr") : C.N = P("N")
    C.Gd = P("Gd") : C.D0 = P("D0")
    C.CTwa = P("CTwa") : C.CTws = P("CTws") : C.CSea = P("CSea")
    C.ImmScale = P("ImmScale") : C.VMin = P("VMin") : C.LMax = P("LMax")
    Return C
  End Function

  Private Shared Function AreaPortante(s As Double, c As Double, kAr As Double) As Double
    If s <= 0 Then Return 0
    Return s / (1 + 2 * c / (kAr * s))
  End Function

  ''' <summary>
  ''' Leeway teorico di un campione (gradi, positivo sottovento). Sow in kn, heel e cant in gradi (heel positivo sottovento,
  ''' cant positivo verso sopravvento), twa/tws/sea per le correzioni, imm = immersione del canard, ang = suo angolo.
  ''' I valori NaN di cant, imm, ang, twa, tws, sea non bloccano il calcolo: diventano il valore neutro.
  ''' </summary>
  Public Function Calcola(Sow As Double, Heel As Double, Twa As Double, Tws As Double, Sea As Double, Cant As Double, Imm As Double, Ang As Double) As Double
    If Double.IsNaN(Sow) OrElse Double.IsNaN(Heel) OrElse Sow < VMin OrElse Sow <= 0 Then Return Double.NaN
    Dim Phi As Double = Radians(Heel)
    Dim Th As Double = If(Double.IsNaN(Cant), 0, Radians(Cant))
    Dim M As Double = Degrees(Math.Sin(Phi) + R * Math.Sin(Phi + Th))

    Dim fK As Double = Math.Pow(Math.Max(0.05, Math.Cos(Phi + Th)), N)
    Dim fB As Double = Math.Pow(Math.Max(0.05, Math.Cos(Phi)), N)
    Dim a0 As Double = AreaPortante(SRef, Chord, KAr)
    Dim g As Double = 1
    If Not Double.IsNaN(Imm) AndAlso a0 > 0 Then g = AreaPortante(Imm * ImmScale, Chord, KAr) / a0
    Dim WB As Double = Ab * g * fB
    Dim S As Double = fK + WB + Ar
    If S <= 0.000001 Then Return Double.NaN

    Dim Kc As Double = 1
    If Not Double.IsNaN(Twa) Then Kc += CTwa * (Math.Abs(Twa) - 45) / 45
    If Not Double.IsNaN(Tws) Then Kc += CTws * (Tws - 12) / 12
    If Not Double.IsNaN(Sea) Then Kc += CSea * Sea
    Kc = Math.Max(0.1, Kc)

    Dim Lee As Double = K * Kc * M / (Sow * Sow * S)
    If Not Double.IsNaN(Ang) Then Lee -= (WB / S) * (Gd * Ang + D0)
    Return Math.Min(LMax, Math.Max(-LMax, Lee))
  End Function
End Class

''' <summary>Serie di ingresso del modello (una voce per riga del file) e, se richiesta, la serie di riferimento.</summary>
Public Class clsLeewayIngressi
  Public N As Integer
  Public Sow, Heel, Twa, Tws, Sea, Cant, Imm, Ang, Yrt, Rif As Double()
  ''' <summary>Quali canali sono stati usati o mancano, mostrato nel report.</summary>
  Public Note As String = ""

  Public Function CalcolaTutti(C As clsLeewayCoeff) As Double()
    Dim Ris(N - 1) As Double
    For i As Integer = 0 To N - 1
      Ris(i) = C.Calcola(Sow(i), Heel(i), Twa(i), Tws(i), Sea(i), Cant(i), Imm(i), Ang(i))
    Next
    Return Ris
  End Function
End Class

Public Class clsLeewayFitResult
  Public Report As String = ""
  Public NuoviValori As Dictionary(Of String, Double)
  Public Rmse0 As Double = Double.NaN
  Public Rmse1 As Double = Double.NaN
End Class

Public Class clsLeewayModel

  Public Shared Function ImpostazioniCorrenti() As clsLeewayModelSettings
    Dim Profilo = AppConfig.ActiveProfile
    If Profilo.LeewayModel Is Nothing Then Profilo.LeewayModel = New clsLeewayModelSettings
    Profilo.LeewayModel.NormalizzaValoriMancanti()
    Return Profilo.LeewayModel
  End Function

  ''' <summary>Canale math Leeway Model: da richiamare sul thread UI (legge canali dal file).</summary>
  Public Shared Function CalcolaSerie(Provider As clsDataProvider2020) As Double()
    Dim S = ImpostazioniCorrenti()
    Dim Ing = CaricaIngressi(Provider, S, False)
    If Ing Is Nothing Then Return New Double() {}
    Return Ing.CalcolaTutti(S.Coefficienti())
  End Function

  Private Shared Function Valori(Ch As clsChannel2020, N As Integer) As Double()
    Dim Ris(N - 1) As Double
    If Ch Is Nothing OrElse Ch.Valori Is Nothing OrElse Ch.Valori.Count <> N Then
      For i As Integer = 0 To N - 1
        Ris(i) = Double.NaN
      Next
    Else
      Array.Copy(Ch.Valori, Ris, N)
    End If
    Return Ris
  End Function

  ''' <summary>
  ''' Cerca il canale per nome esatto (Override), poi per i nomi noti, poi per parti del nome (es. Canard/Dagger/Board + Angle).
  ''' Il caso delle parti del nome e' disattivabile: per il cant della chiglia sarebbe ambiguo.
  ''' </summary>
  Private Shared Function CercaPerNome(Provider As clsDataProvider2020, Nome As String) As clsChannel2020
    ' nome del canale nel file (ChannelId), poi nome lungo o breve (es. RudderFwd si chiama anche DaggerAngle)
    Dim c = Provider.CanaleDbl(Nome)
    If Not c Is Nothing Then Return c
    For Each ch In Provider.Channels.ListaCanali
      If ch Is Nothing OrElse ch.IsMath OrElse ch.ChannelId Is Nothing Then Continue For
      If String.Equals(ch.LongName, Nome, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(ch.ShortName, Nome, StringComparison.OrdinalIgnoreCase) Then
        Return Provider.CanaleDbl(ch.ChannelId)
      End If
    Next
    Return Nothing
  End Function

  Private Shared Function TrovaCanale(Provider As clsDataProvider2020, Override As String, NomiNoti As String(), Prefissi As String(), Suffissi As String()) As clsChannel2020
    If Not String.IsNullOrWhiteSpace(Override) Then Return CercaPerNome(Provider, Override.Trim)
    For Each Nome In NomiNoti
      Dim c = CercaPerNome(Provider, Nome)
      If Not c Is Nothing Then Return c
    Next
    If Prefissi Is Nothing Then Return Nothing
    For Each ch In Provider.Channels.ListaCanali
      If ch Is Nothing OrElse ch.IsMath OrElse ch.ChannelId Is Nothing Then Continue For
      Dim Id As String = ch.ChannelId.ToLower
      If Id.EndsWith("_tgt") OrElse Id.EndsWith("_pol") Then Continue For
      Dim Nomi As String = Id & "|" & If(ch.LongName, "").ToLower
      If Prefissi.Any(Function(p) Nomi.Contains(p.ToLower)) AndAlso Suffissi.Any(Function(s) Nomi.Contains(s.ToLower)) Then
        Return Provider.CanaleDbl(ch.ChannelId)
      End If
    Next
    Return Nothing
  End Function

  Private Shared Function NomeCanale(Ch As clsChannel2020) As String
    If Ch Is Nothing Then Return "NOT FOUND"
    Return Ch.ChannelId
  End Function

  Public Shared Function CaricaIngressi(Provider As clsDataProvider2020, S As clsLeewayModelSettings, ConRiferimento As Boolean) As clsLeewayIngressi
    If Provider Is Nothing OrElse Not Provider.ValoriCaricati Then Return Nothing
    Dim Ts = Provider.TimeStamps
    If Ts Is Nothing OrElse Ts.Count = 0 Then Return Nothing
    Dim Ing As New clsLeewayIngressi
    Ing.N = Ts.Count
    Dim N As Integer = Ing.N
    Dim C As clsLeewayCoeff = S.Coefficienti()

    Dim chSow = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chHeel = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
    Dim chTwa = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chYrt = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim chSea As clsChannel2020 = Nothing
    If C.CSea <> 0 Then chSea = Provider.CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)

    Dim chAng = TrovaCanale(Provider, S.CanardAngleChannel, {"CanardAngle", "DaggerAngle", "BoardAngle", "RudderFwd"}, {"Canard", "Dagger", "Board"}, {"Angle"})
    Dim chImm = TrovaCanale(Provider, S.CanardImmersionChannel, {"CanardImmersion", "DaggerImmersion", "BoardImmersion", "Board"}, {"Canard", "Dagger", "Board"}, {"Immersion"})
    Dim chCant = TrovaCanale(Provider, S.KeelCantChannel, {"KeelCant", "KeelAng", "KeelAngle", "CantAngle"}, Nothing, Nothing)

    Ing.Sow = Valori(chSow, N)
    Ing.Heel = Valori(chHeel, N)
    Ing.Twa = Valori(chTwa, N)
    Ing.Tws = Valori(chTws, N)
    Ing.Yrt = Valori(chYrt, N)
    Ing.Sea = Valori(chSea, N)
    Ing.Imm = Valori(chImm, N)
    Ing.Ang = Valori(chAng, N)
    Ing.Cant = Valori(chCant, N)

    ' normalizzazione sulla mura: positivo = sopravvento
    For i As Integer = 0 To N - 1
      Dim Lato As Double = If(Double.IsNaN(Ing.Twa(i)) OrElse Ing.Twa(i) >= 0, 1, -1)
      If S.CanardAngleSignedByTack Then Ing.Ang(i) *= Lato
      If S.KeelCantSignedByTack Then Ing.Cant(i) *= Lato
      If S.KeelCantInverted Then Ing.Cant(i) = -Ing.Cant(i)
    Next

    Ing.Note = "Channels: SOW=" & NomeCanale(chSow) & "  HeelNorm=" & NomeCanale(chHeel) & "  TWA=" & NomeCanale(chTwa) &
               "  CanardAngle=" & NomeCanale(chAng) & "  CanardImmersion=" & NomeCanale(chImm) & "  KeelCant=" & NomeCanale(chCant)

    If ConRiferimento Then
      Dim Chiave As clsChannels2020.eCanaliChiave
      Select Case S.Riferimento
        Case 1 : Chiave = clsChannels2020.eCanaliChiave.eLeewayRecalc
        Case 2 : Chiave = clsChannels2020.eCanaliChiave.eLwyRec3s
        Case 3 : Chiave = clsChannels2020.eCanaliChiave.eLwyRec30s
        Case Else : Chiave = clsChannels2020.eCanaliChiave.eLwyNorm
      End Select
      Dim chRif = Provider.CanaleDbl(Chiave)
      Ing.Rif = Valori(chRif, N)
      Ing.Note &= "  Reference=" & NomeCanale(chRif)
    End If
    Return Ing
  End Function

#Region "Adattamento dei coefficienti"

  ''' <summary>Campioni validi per l'adattamento: navigazione stabile, riferimento e ingressi presenti, senza rotazioni.</summary>
  Private Shared Function CampioniValidi(Ing As clsLeewayIngressi, C As clsLeewayCoeff, IdIni As Integer, IdFin As Integer) As Integer()
    Dim Idx As New List(Of Integer)
    For i As Integer = Math.Max(0, IdIni) To Math.Min(Ing.N - 1, IdFin)
      If Double.IsNaN(Ing.Rif(i)) OrElse Double.IsNaN(Ing.Sow(i)) OrElse Double.IsNaN(Ing.Heel(i)) Then Continue For
      If Ing.Sow(i) < C.VMin Then Continue For
      If Math.Abs(Ing.Rif(i)) > C.LMax Then Continue For
      If Not Double.IsNaN(Ing.Yrt(i)) AndAlso Math.Abs(Ing.Yrt(i)) > 3 Then Continue For ' fuori dalle manovre
      Idx.Add(i)
    Next
    Return Idx.ToArray
  End Function

  ''' <summary>Errore del modello rispetto al riferimento sui campioni: rmse, bias (modello - riferimento) e campioni senza valore.</summary>
  Private Shared Sub Errore(Ing As clsLeewayIngressi, C As clsLeewayCoeff, Campioni As Integer(), ByRef Rmse As Double, ByRef Bias As Double, ByRef Senza As Integer)
    Dim Somma2 As Double = 0
    Dim Somma As Double = 0
    Dim N As Integer = 0
    Senza = 0
    For Each i In Campioni
      Dim v As Double = C.Calcola(Ing.Sow(i), Ing.Heel(i), Ing.Twa(i), Ing.Tws(i), Ing.Sea(i), Ing.Cant(i), Ing.Imm(i), Ing.Ang(i))
      If Double.IsNaN(v) Then
        Senza += 1
      Else
        Dim r As Double = v - Ing.Rif(i)
        Somma += r
        Somma2 += r * r
        N += 1
      End If
    Next
    If N = 0 Then
      Rmse = Double.NaN
      Bias = Double.NaN
    Else
      Rmse = Math.Sqrt(Somma2 / N)
      Bias = Somma / N
    End If
  End Sub

  ''' <summary>Valuta il modello con i coefficienti correnti (senza adattare) e restituisce un report.</summary>
  Public Shared Function Valuta(Ing As clsLeewayIngressi, S As clsLeewayModelSettings, IdIni As Integer, IdFin As Integer) As String
    Dim C = S.Coefficienti()
    Dim Campioni = CampioniValidi(Ing, C, IdIni, IdFin)
    If Campioni.Length = 0 Then Return "No valid samples in the selected range." & vbCrLf & Ing.Note
    Dim Rmse, Bias As Double
    Dim Senza As Integer
    Errore(Ing, C, Campioni, Rmse, Bias, Senza)
    Return "Samples: " & Campioni.Length & "   RMSE: " & Rmse.ToString("F3") & " deg   Bias (model - reference): " & Bias.ToString("F3") & " deg   Samples without model value: " & Senza & vbCrLf & Ing.Note
  End Function

  ''' <summary>
  ''' Adatta i coefficienti marcati "Fit" in modo che il modello segua la serie di riferimento.
  ''' Perdita di Huber (1 grado) per non farsi trascinare dai punti anomali, piu' un termine di regolarizzazione verso i valori
  ''' di partenza perche' i coefficienti non sono tutti indipendenti. Nelder-Mead sui coefficienti riportati a [1,2]
  ''' (la simplex di partenza e' proporzionale al valore, quindi non deve partire da zero).
  ''' Lavora solo su array gia' caricati: puo' girare in background.
  ''' </summary>
  Public Shared Function Adatta(Ing As clsLeewayIngressi, S As clsLeewayModelSettings, IdIni As Integer, IdFin As Integer) As clsLeewayFitResult
    Dim Ris As New clsLeewayFitResult
    Dim Parametri As List(Of clsLeewayParametro) = S.Parametri
    Dim ValoriIniziali As Dictionary(Of String, Double) = Parametri.ToDictionary(Function(p) p.Nome, Function(p) p.Valore)
    Dim C0 As clsLeewayCoeff = clsLeewayCoeff.Da(ValoriIniziali)

    Dim Tutti As Integer() = CampioniValidi(Ing, C0, IdIni, IdFin)
    If Tutti.Length < 50 Then
      Ris.Report = "Too few valid samples (" & Tutti.Length & ") in the selected range: at least 50 are needed." & vbCrLf & Ing.Note
      Return Ris
    End If
    Dim Libere As List(Of clsLeewayParametro) = Parametri.Where(Function(p) p.DaAdattare AndAlso p.Massimo > p.Minimo).ToList
    If Libere.Count = 0 Then
      Ris.Report = "No coefficient is marked for fitting."
      Return Ris
    End If

    ' sottocampionamento regolare: la funzione obiettivo viene valutata migliaia di volte
    Dim Passo As Integer = Math.Max(1, Tutti.Length \ 3000)
    Dim Campioni As Integer() = Tutti.Where(Function(x, k) k Mod Passo = 0).ToArray

    Dim Rmse0, Bias0 As Double
    Dim Senza0 As Integer
    Errore(Ing, C0, Campioni, Rmse0, Bias0, Senza0)

    Dim Corrente As Dictionary(Of String, Double) = New Dictionary(Of String, Double)(ValoriIniziali)
    Dim Fase As Integer = 0
    Do
      Fase += 1
      ' fase 1: solo K (se libero), fase 2: tutti i coefficienti liberi
      Dim DaOttimizzare As List(Of clsLeewayParametro) = Libere
      If Fase = 1 Then
        DaOttimizzare = Libere.Where(Function(p) p.Nome = "K").ToList
        If DaOttimizzare.Count = 0 Then Continue Do
      End If
      Dim Base As Dictionary(Of String, Double) = New Dictionary(Of String, Double)(Corrente)
      Dim U0(DaOttimizzare.Count - 1) As Double
      For j As Integer = 0 To DaOttimizzare.Count - 1
        Dim p = DaOttimizzare(j)
        U0(j) = 1 + (Base(p.Nome) - p.Minimo) / (p.Massimo - p.Minimo)
      Next

      Dim Obiettivo As Func(Of MathNet.Numerics.LinearAlgebra.Vector(Of Double), Double) =
        Function(V As MathNet.Numerics.LinearAlgebra.Vector(Of Double)) As Double
          Dim Prova As Dictionary(Of String, Double) = New Dictionary(Of String, Double)(Base)
          Dim Penale As Double = 0
          For j As Integer = 0 To DaOttimizzare.Count - 1
            Dim p = DaOttimizzare(j)
            Dim u As Double = V(j)
            Dim uc As Double = Math.Min(2, Math.Max(1, u))
            Penale += (u - uc) * (u - uc)
            Prova(p.Nome) = p.Minimo + (uc - 1) * (p.Massimo - p.Minimo)
          Next
          Dim C As clsLeewayCoeff = clsLeewayCoeff.Da(Prova)
          Dim Somma As Double = 0
          Dim N As Integer = 0
          For Each i In Campioni
            Dim v1 As Double = C.Calcola(Ing.Sow(i), Ing.Heel(i), Ing.Twa(i), Ing.Tws(i), Ing.Sea(i), Ing.Cant(i), Ing.Imm(i), Ing.Ang(i))
            If Double.IsNaN(v1) Then
              Somma += 20 ' nessun valore del modello: peggio di qualsiasi errore ammissibile
            Else
              Dim a As Double = Math.Abs(v1 - Ing.Rif(i))
              Somma += If(a <= 1, 0.5 * a * a, a - 0.5)
            End If
            N += 1
          Next
          Dim Ridge As Double = 0
          For j As Integer = 0 To DaOttimizzare.Count - 1
            Ridge += (V(j) - U0(j)) * (V(j) - U0(j))
          Next
          Return Math.Sqrt(2 * Somma / Math.Max(1, N)) + 0.001 * Ridge + 100 * Penale
        End Function

      Dim Punto0 = MathNet.Numerics.LinearAlgebra.Vector(Of Double).Build.DenseOfArray(U0)
      Dim Trovato = MathNet.Numerics.Optimization.NelderMeadSimplex.Minimum(MathNet.Numerics.Optimization.ObjectiveFunction.Value(Obiettivo), Punto0, 0.000001, 3000)
      For j As Integer = 0 To DaOttimizzare.Count - 1
        Dim p = DaOttimizzare(j)
        Dim uc As Double = Math.Min(2, Math.Max(1, Trovato.MinimizingPoint(j)))
        Corrente(p.Nome) = p.Minimo + (uc - 1) * (p.Massimo - p.Minimo)
      Next
    Loop While Fase < 2

    Dim C1 As clsLeewayCoeff = clsLeewayCoeff.Da(Corrente)
    Dim Rmse1, Bias1 As Double
    Dim Senza1 As Integer
    Errore(Ing, C1, Campioni, Rmse1, Bias1, Senza1)
    Ris.NuoviValori = Corrente
    Ris.Rmse0 = Rmse0
    Ris.Rmse1 = Rmse1

    Dim Sb As New System.Text.StringBuilder
    Sb.AppendLine("Samples used: " & Campioni.Length & " (of " & Tutti.Length & " valid, 1 every " & Passo & ")")
    Sb.AppendLine("RMSE: " & Rmse0.ToString("F3") & " -> " & Rmse1.ToString("F3") & " deg    Bias (model - reference): " & Bias0.ToString("F3") & " -> " & Bias1.ToString("F3") & " deg")
    Sb.AppendLine()
    For Each p In Libere
      Dim Vecchio As Double = ValoriIniziali(p.Nome)
      Dim Nuovo As Double = Corrente(p.Nome)
      Dim Nota As String = ""
      Dim Fascia As Double = (p.Massimo - p.Minimo) * 0.01
      If Nuovo - p.Minimo < Fascia Then Nota = "   <- at the lower limit"
      If p.Massimo - Nuovo < Fascia Then Nota = "   <- at the upper limit"
      Sb.AppendLine(p.Nome.PadRight(9) & Vecchio.ToString("0.####").PadLeft(10) & " -> " & Nuovo.ToString("0.####").PadRight(10) & Nota)
    Next
    Sb.AppendLine()
    Sb.AppendLine(Ing.Note)
    Ris.Report = Sb.ToString
    Return Ris
  End Function

#End Region

End Class
