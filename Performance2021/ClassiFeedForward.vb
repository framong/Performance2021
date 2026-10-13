Imports System.Collections.Generic
Imports System.Linq

''' <summary>Modello usato per descrivere come cambia Y al variare di X (medie dei bin).</summary>
Public Enum eFfModello
  ''' <summary>Retta pesata sul numero di campioni: il piu' prudente, l'ottimo e' sempre su un estremo.</summary>
  Lineare = 0
  ''' <summary>Parabola pesata: trova un massimo (o minimo) vero; se la curva e' ancora in salita il vertice e' limitato a poco oltre i dati.</summary>
  Quadratico = 1
  ''' <summary>Curva che sale fino a un picco e poi scende, senza altra forma assunta: l'ottimo e' il picco (inizio del plateau).</summary>
  LocaleUnimodale = 2
End Enum

''' <summary>Parametri del report feed forward: fasce di vento, bin della variabile da ottimizzare (X), modello.</summary>
Public Class clsFfParametri
  Public Property TwsMin As Double = 6
  Public Property TwsMax As Double = 24
  Public Property TwsStep As Double = 2
  ''' <summary>Limite inferiore del primo bin di X (per esempio 80 per Vmg% oltre l'80%).</summary>
  Public Property XBinStart As Double = 80
  ''' <summary>Limite superiore dei bin di X; se non e' maggiore di XBinStart non c'e' limite superiore.</summary>
  Public Property XBinEnd As Double = 0
  Public Property XBinWidth As Double = 5
  ''' <summary>Campioni minimi perche' un bin entri nel fit (minimo assoluto, insieme a MinQuotaBinPerc).</summary>
  Public Property MinCampioniBin As Integer = 20
  ''' <summary>
  ''' Quota minima dei campioni della fascia (%) perche' un bin sia "denso" ed entri nel fit e nel calcolo del bersaglio.
  ''' Con la percentuale la soglia si adatta da sola a fasce con pochi o tanti campioni; i bin radi restano visibili ma non si usano.
  ''' </summary>
  Public Property MinQuotaBinPerc As Double = 2
  ''' <summary>Bin utilizzabili minimi per calcolare il fit di una fascia.</summary>
  Public Property MinBinPerFit As Integer = 3
  Public Property Modello As eFfModello = eFfModello.Quadratico
  ''' <summary>True = si cerca il massimo di Y, False = il minimo.</summary>
  Public Property Massimizza As Boolean = True
  ''' <summary>
  ''' True = Y e' una regolazione (per esempio il trim) e X la prestazione: il bersaglio e' sempre il valore piu' alto di X (ultimo bin con dati piu' l'estrapolazione)
  ''' e Y atteso e' il valore del modello li'. Massimizza e' ignorato.
  ''' </summary>
  Public Property SpingiX As Boolean = True
  ''' <summary>0 = impostazioni salvate prima che "spingi X" diventasse il comportamento predefinito (vedi Aggiorna).</summary>
  Public Property VersioneObiettivo As Integer = 0
  ''' <summary>Di quanti bin oltre l'ultimo bin con dati l'ottimo puo' spingersi (estrapolazione).</summary>
  Public Property ExtrapolazioneBin As Double = 1

  Public Function Clona() As clsFfParametri
    Return DirectCast(Me.MemberwiseClone(), clsFfParametri)
  End Function

  ''' <summary>Porta le impostazioni salvate alla versione corrente: "spingi X al massimo" e' il comportamento predefinito (la richiesta originale del feed forward).</summary>
  Public Sub Aggiorna()
    If VersioneObiettivo < 1 Then
      SpingiX = True
      VersioneObiettivo = 1
    End If
  End Sub
End Class

Public Class clsFfBin
  ''' <summary>Numero d'ordine del bin a partire da XBinStart (0 = primo bin).</summary>
  Public Property Indice As Integer
  Public Property XLow As Double
  Public Property XHigh As Double
  ''' <summary>Media di X dei campioni del bin (posizione usata nel fit).</summary>
  Public Property XMedio As Double
  Public Property N As Integer
  Public Property YMedio As Double
  Public Property YDev As Double
  Public Property UsatoNelFit As Boolean
End Class

Public Class clsFfOttimo
  Public Property X As Double
  Public Property Y As Double
  ''' <summary>Errore standard del valore previsto in X (NaN per il modello locale).</summary>
  Public Property ErroreStd As Double = Double.NaN
  Public Property R2 As Double = Double.NaN
  Public Property Estrapolato As Boolean
  Public Property BinUsati As Integer
  Public Property CampioniUsati As Integer
  ''' <summary>Bin con la media migliore tra quelli osservati (per confronto con l'ottimo suggerito).</summary>
  Public Property XMigliorBin As Double
  Public Property YMigliorBin As Double
  Public Property Punteggio As Double
  Public Property Affidabilita As String = ""
End Class

Public Class clsFfFascia
  Public Property TwsLow As Double
  Public Property TwsHigh As Double
  Public Property Campioni As Integer
  Public Property Bins As New List(Of clsFfBin)
  ''' <summary>Campioni grezzi della fascia (nuvola): X, Y e indice del bin di ciascuno.</summary>
  Public Property PuntiX As New List(Of Double)
  Public Property PuntiY As New List(Of Double)
  Public Property PuntiBin As New List(Of Integer)
  ''' <summary>Curva del modello, da disegnare (puo' arrivare fino all'ottimo estrapolato).</summary>
  Public Property CurvaX As New List(Of Double)
  Public Property CurvaY As New List(Of Double)
  Public Property Ottimo As clsFfOttimo = Nothing
  ''' <summary>Motivo per cui non c'e' un ottimo (pochi bin, pochi campioni...).</summary>
  Public Property Messaggio As String = ""
End Class

''' <summary>Un bin (a quantili) del canale nel calcolo della sensitivita': valori medi del canale e della prestazione.</summary>
Public Class clsFfBinSensitivita
  Public Property CanaleMin As Double
  Public Property CanaleMax As Double
  Public Property CanaleMedio As Double
  Public Property PrestazioneMedia As Double
  Public Property N As Integer
End Class

''' <summary>
''' Sensitivita' della prestazione X a un canale Y in una fascia: di quanto cambia X quando Y si sposta della sua variazione tipica
''' (1 deviazione standard nella fascia), quanta parte della variazione di X si spiega con Y e il valore di Y con la prestazione migliore.
''' </summary>
Public Class clsFfSensitivita
  Public Property TwsLow As Double
  Public Property TwsHigh As Double
  Public Property Campioni As Integer
  ''' <summary>Variazione tipica del canale (deviazione standard dei campioni della fascia).</summary>
  Public Property VariazioneTipica As Double = Double.NaN
  ''' <summary>Variazione della prestazione per una variazione tipica del canale (unita' di X): pendenza della retta X su Y per la deviazione standard di Y.</summary>
  Public Property Sensitivita As Double = Double.NaN
  ''' <summary>Quota della varianza di X spiegata dai bin del canale (rapporto di correlazione eta^2, 0-1): coglie anche relazioni non lineari.</summary>
  Public Property QuotaSpiegata As Double = Double.NaN
  ''' <summary>Valore medio del canale nel bin con la prestazione migliore, guadagno rispetto alla prestazione media e se e' il primo o l'ultimo bin.</summary>
  Public Property ValoreMigliore As Double = Double.NaN
  Public Property GuadagnoMigliore As Double = Double.NaN
  Public Property MiglioreAlBordo As Boolean
  Public Property Bins As New List(Of clsFfBinSensitivita)
  Public Property Messaggio As String = ""
  Public ReadOnly Property Valida As Boolean
    Get
      Return Not Double.IsNaN(Sensitivita)
    End Get
  End Property
End Class

''' <summary>
''' Motore del report feed forward. Per ogni fascia di vento: i campioni validi sono divisi in bin della variabile X da ottimizzare,
''' di ogni bin si calcola la media di Y, poi un modello (retta, parabola o curva monotona) sulle medie dei bin, pesate per il numero di campioni,
''' dice dove sta l'ottimo e quanto vale Y la'. L'ottimo non va oltre ExtrapolazioneBin bin dall'ultimo bin con dati: oltre non si promette nulla.
''' Nessuna dipendenza dai dati dell'applicazione: riceve array e una maschera, quindi e' verificabile da solo.
''' </summary>
Public Class clsFeedForward

  ''' <summary>Curva polinomiale (grado 1 o 2) adattata con minimi quadrati pesati, con la matrice necessaria all'errore standard.</summary>
  Private Class clsPoli
    Public Grado As Integer
    Public Coef() As Double
    Public Xref As Double
    Public Scala As Double
    Public Inv(,) As Double
    Public S2 As Double = Double.NaN
    Public R2 As Double = Double.NaN
    Public Gdl As Integer

    Public Function Valore(x As Double) As Double
      Dim u As Double = (x - Xref) / Scala
      Dim y As Double = 0
      For k As Integer = Grado To 0 Step -1
        y = y * u + Coef(k)
      Next
      Return y
    End Function

    Public Function ErroreStd(x As Double) As Double
      If Double.IsNaN(S2) OrElse Inv Is Nothing Then Return Double.NaN
      Dim u As Double = (x - Xref) / Scala
      Dim v(Grado) As Double
      v(0) = 1
      For k As Integer = 1 To Grado
        v(k) = v(k - 1) * u
      Next
      Dim q As Double = 0
      For i As Integer = 0 To Grado
        For j As Integer = 0 To Grado
          q += v(i) * Inv(i, j) * v(j)
        Next
      Next
      Return System.Math.Sqrt(System.Math.Max(0, S2 * q))
    End Function
  End Class

  ' ---------------------------------------------------------------- calcolo principale

  ''' <summary>
  ''' Calcola tutte le fasce di vento. Righe = indici di riga da considerare (per esempio i periodi scelti), Maschera = filtro (Nothing = tutte valide).
  ''' Campioni con X, Y o TWS non validi (NaN) sono scartati.
  ''' </summary>
  Public Shared Function Calcola(X() As Double, Y() As Double, Tws() As Double, Righe As IEnumerable(Of Integer),
                                 Maschera As Func(Of Integer, Boolean), P As clsFfParametri) As List(Of clsFfFascia)
    Dim Fasce As New List(Of clsFfFascia)
    If X Is Nothing OrElse Y Is Nothing OrElse Tws Is Nothing OrElse P Is Nothing OrElse P.TwsStep <= 0 OrElse P.XBinWidth <= 0 Then Return Fasce

    Dim NFasce As Integer = System.Math.Max(1, CInt(System.Math.Ceiling((P.TwsMax - P.TwsMin) / P.TwsStep - 0.000000001)))
    For f As Integer = 0 To NFasce - 1
      Fasce.Add(New clsFfFascia With {.TwsLow = P.TwsMin + f * P.TwsStep, .TwsHigh = P.TwsMin + (f + 1) * P.TwsStep})
    Next

    Dim UsaLimiteX As Boolean = P.XBinEnd > P.XBinStart
    Dim NBin As Integer = If(UsaLimiteX, System.Math.Max(1, CInt(System.Math.Ceiling((P.XBinEnd - P.XBinStart) / P.XBinWidth - 0.000000001))), 0)

    ' accumulo per fascia e per bin (i bin si creano man mano se manca il limite superiore)
    Dim Somme As New List(Of Dictionary(Of Integer, Double()))
    For f As Integer = 0 To NFasce - 1
      Somme.Add(New Dictionary(Of Integer, Double()))
    Next

    Dim n As Integer = System.Math.Min(X.Length, System.Math.Min(Y.Length, Tws.Length))
    For Each i As Integer In Righe
      If i < 0 OrElse i >= n Then Continue For
      Dim x1 As Double = X(i), y1 As Double = Y(i), t As Double = Tws(i)
      If Double.IsNaN(x1) OrElse Double.IsNaN(y1) OrElse Double.IsNaN(t) Then Continue For
      If Maschera IsNot Nothing AndAlso Not Maschera(i) Then Continue For
      If t < P.TwsMin OrElse t >= P.TwsMax Then Continue For
      If x1 < P.XBinStart Then Continue For
      If UsaLimiteX AndAlso x1 > P.XBinEnd Then Continue For
      Dim b As Integer = CInt(System.Math.Floor((x1 - P.XBinStart) / P.XBinWidth))
      If UsaLimiteX AndAlso b >= NBin Then b = NBin - 1 ' il limite superiore e' incluso nell'ultimo bin
      Dim fa As Integer = CInt(System.Math.Floor((t - P.TwsMin) / P.TwsStep))
      If fa < 0 OrElse fa >= NFasce Then Continue For

      Dim Fascia As clsFfFascia = Fasce(fa)
      Fascia.Campioni += 1
      Fascia.PuntiX.Add(x1)
      Fascia.PuntiY.Add(y1)
      Fascia.PuntiBin.Add(b)
      Dim a() As Double = Nothing
      If Not Somme(fa).TryGetValue(b, a) Then
        a = New Double(4) {} ' n, somma x, somma y, somma y^2 (le 4 posizioni utili)
        Somme(fa)(b) = a
      End If
      a(0) += 1
      a(1) += x1
      a(2) += y1
      a(3) += y1 * y1
    Next

    For fa As Integer = 0 To NFasce - 1
      Dim Fascia As clsFfFascia = Fasce(fa)
      For Each kv As KeyValuePair(Of Integer, Double()) In Somme(fa).OrderBy(Function(q) q.Key)
        Dim a() As Double = kv.Value
        Dim Nn As Integer = CInt(a(0))
        Dim Media As Double = a(2) / Nn
        Dim Var As Double = If(Nn > 1, System.Math.Max(0, (a(3) - Nn * Media * Media) / (Nn - 1)), 0)
        Fascia.Bins.Add(New clsFfBin With {
          .Indice = kv.Key, .XLow = P.XBinStart + kv.Key * P.XBinWidth, .XHigh = P.XBinStart + (kv.Key + 1) * P.XBinWidth,
          .XMedio = a(1) / Nn, .N = Nn, .YMedio = Media, .YDev = System.Math.Sqrt(Var),
          .UsatoNelFit = Nn >= P.MinCampioniBin AndAlso Nn >= P.MinQuotaBinPerc / 100.0 * Fascia.Campioni})
      Next
      CalcolaOttimo(Fascia, P)
    Next

    Return Fasce
  End Function

  ' ---------------------------------------------------------------- ottimo di una fascia

  ''' <summary>Bin usati dal trend locale (modello locale con "spingi X"): gli ultimi, i piu' vicini al bersaglio.</summary>
  Private Const BinTrendLocale As Integer = 3

  Private Shared Sub CalcolaOttimo(F As clsFfFascia, P As clsFfParametri)
    Dim Usati As List(Of clsFfBin) = F.Bins.Where(Function(b) b.UsatoNelFit).OrderBy(Function(b) b.XMedio).ToList
    If F.Campioni = 0 Then
      F.Messaggio = "No samples"
      Exit Sub
    End If
    If Usati.Count < System.Math.Max(2, P.MinBinPerFit) Then
      F.Messaggio = "Not enough dense bins (at least " & P.MinCampioniBin & " samples and " & P.MinQuotaBinPerc.ToString("0.#") & "% of the band): " & Usati.Count & " of " & System.Math.Max(2, P.MinBinPerFit)
      Exit Sub
    End If

    Dim xs() As Double = Usati.Select(Function(b) b.XMedio).ToArray
    Dim ys() As Double = Usati.Select(Function(b) b.YMedio).ToArray
    Dim ws() As Double = Usati.Select(Function(b) CDbl(b.N)).ToArray
    Dim Segno As Double = If(P.Massimizza, 1.0, -1.0)
    Dim xLo As Double = xs(0)
    Dim xUltimo As Double = xs(xs.Length - 1)
    Dim xHi As Double = xUltimo + System.Math.Max(0, P.ExtrapolazioneBin) * P.XBinWidth
    ' spingendo X il bersaglio e' l'ultimo bin denso piu' ExtrapolazioneBin bin: i bin radi (pochi campioni isolati) non lo spostano

    Dim O As New clsFfOttimo With {.BinUsati = Usati.Count, .CampioniUsati = Usati.Sum(Function(b) b.N)}
    Dim Migliore As clsFfBin = If(P.SpingiX, Usati.Last, If(P.Massimizza, Usati.OrderByDescending(Function(b) b.YMedio).First, Usati.OrderBy(Function(b) b.YMedio).First))
    O.XMigliorBin = Migliore.XMedio
    O.YMigliorBin = Migliore.YMedio

    If P.Modello = eFfModello.LocaleUnimodale AndAlso Not P.SpingiX Then
      OttimoUnimodale(F, O, xs, ys, ws, Segno, xHi, P)
    Else
      Dim Grado As Integer = If(P.Modello = eFfModello.Lineare, 1, 2)
      ' spingendo X il modello locale e' il trend locale: retta pesata sugli ultimi bin usati (quelli piu' vicini al bersaglio)
      Dim xsF() As Double = xs, ysF() As Double = ys, wsF() As Double = ws
      If P.SpingiX AndAlso P.Modello = eFfModello.LocaleUnimodale Then
        Dim k As Integer = System.Math.Min(BinTrendLocale, xs.Length)
        xsF = xs.Skip(xs.Length - k).ToArray
        ysF = ys.Skip(ys.Length - k).ToArray
        wsF = ws.Skip(ws.Length - k).ToArray
        Grado = 1
      End If
      If xsF.Length <= Grado Then Grado = xsF.Length - 1
      Dim C As clsPoli = AdattaPoli(xsF, ysF, wsF, Grado, P.XBinWidth)
      If C Is Nothing Then
        F.Messaggio = "The fit is not possible with these bins"
        Exit Sub
      End If
      ' candidati: estremi dell'intervallo ammesso e vertice (se cade dentro); vince il migliore nel verso richiesto
      Dim Cand As New List(Of Double) From {xLo, xHi}
      If P.SpingiX Then Cand = New List(Of Double) From {xHi} ' solo il valore piu' alto di X
      If Not P.SpingiX AndAlso Grado = 2 AndAlso System.Math.Abs(C.Coef(2)) > 0.000000000001 Then
        Dim uv As Double = -C.Coef(1) / (2 * C.Coef(2))
        Dim xv As Double = C.Xref + uv * C.Scala
        If xv > xLo AndAlso xv < xHi Then Cand.Add(xv)
      End If
      Dim xBest As Double = Cand(0)
      For Each cx As Double In Cand
        If Segno * C.Valore(cx) > Segno * C.Valore(xBest) + 0.000000000001 Then xBest = cx
      Next
      O.X = xBest
      O.Y = C.Valore(xBest)
      O.ErroreStd = C.ErroreStd(xBest)
      O.R2 = C.R2
      O.Estrapolato = xBest > xUltimo + 0.000000001
      ' curva da disegnare: dal primo bin del fit all'ottimo o al limite estrapolato
      Dim xFine As Double = System.Math.Max(xHi, xBest)
      Dim xIni As Double = xsF(0)
      Const Passi As Integer = 40
      For k As Integer = 0 To Passi
        Dim x As Double = xIni + (xFine - xIni) * k / Passi
        F.CurvaX.Add(x)
        F.CurvaY.Add(C.Valore(x))
      Next
    End If

    ' affidabilita': tanti bin, tanti campioni, poi
    ' - cercando il massimo/minimo di Y: buon adattamento (R2) e nessuna estrapolazione, perche' conta la forma della curva
    ' - spingendo X: precisione del valore suggerito, cioe' 2 errori standard al bersaglio confrontati con la dispersione dei campioni
    '   dentro i bin; l'estrapolazione e' gia' nell'errore standard (cresce allontanandosi dai dati), quindi non si penalizza di nuovo
    Dim s As Double = System.Math.Min(1.0, O.BinUsati / 5.0) * System.Math.Min(1.0, O.CampioniUsati / 300.0)
    If P.SpingiX Then
      Dim Sigma As Double = DispersioneNeiBin(Usati)
      If Double.IsNaN(O.ErroreStd) OrElse Double.IsNaN(Sigma) OrElse Sigma <= 0 Then
        s *= 0.5
      Else
        Dim Rapporto As Double = 2 * O.ErroreStd / Sigma
        s *= 1 / (1 + Rapporto * Rapporto)
      End If
    Else
      s *= If(Double.IsNaN(O.R2), 0.7, System.Math.Max(0, System.Math.Min(1.0, O.R2)))
      If O.Estrapolato Then s *= 0.6
    End If
    O.Punteggio = s
    O.Affidabilita = If(s >= 0.55, "High", If(s >= 0.3, "Medium", "Low"))
    F.Ottimo = O
  End Sub

  ''' <summary>Deviazione standard dei campioni dentro i bin (combinata sui bin usati): la variabilita' naturale del canale Y.</summary>
  Private Shared Function DispersioneNeiBin(Usati As List(Of clsFfBin)) As Double
    Dim Num As Double = 0, Den As Double = 0
    For Each B As clsFfBin In Usati
      If B.N < 2 Then Continue For
      Num += (B.N - 1) * B.YDev * B.YDev
      Den += B.N - 1
    Next
    Return If(Den > 0, System.Math.Sqrt(Num / Den), Double.NaN)
  End Function

  ''' <summary>Regressione isotonica non decrescente (PAVA) con pesi.</summary>
  Private Shared Function Pava(y() As Double, w() As Double) As Double()
    Dim n As Integer = y.Length
    If n = 0 Then Return New Double() {}
    Dim Liv(n - 1) As Double
    Dim Pes(n - 1) As Double
    Dim Lungh(n - 1) As Integer
    Dim m As Integer = 0
    For i As Integer = 0 To n - 1
      Liv(m) = y(i)
      Pes(m) = w(i)
      Lungh(m) = 1
      m += 1
      While m > 1 AndAlso Liv(m - 2) > Liv(m - 1)
        Dim tot As Double = Pes(m - 2) + Pes(m - 1)
        Liv(m - 2) = (Liv(m - 2) * Pes(m - 2) + Liv(m - 1) * Pes(m - 1)) / tot
        Pes(m - 2) = tot
        Lungh(m - 2) += Lungh(m - 1)
        m -= 1
      End While
    Next
    Dim Fit(n - 1) As Double
    Dim k As Integer = 0
    For b As Integer = 0 To m - 1
      For j As Integer = 1 To Lungh(b)
        Fit(k) = Liv(b)
        k += 1
      Next
    Next
    Return Fit
  End Function

  ''' <summary>
  ''' Modello locale unimodale: la curva sale fino a un picco e poi scende (senza altra forma assunta). Per ogni possibile picco si adatta una curva
  ''' crescente prima e decrescente dopo; vince il picco con l'errore minore (a parita' il primo, cioe' l'inizio del plateau).
  ''' Se il picco e' l'ultimo bin e la curva sale ancora, si spinge di ExtrapolazioneBin bin con la pendenza dell'ultimo tratto.
  ''' Se si minimizza si ribalta il segno di Y e il picco diventa un minimo.
  ''' </summary>
  Private Shared Sub OttimoUnimodale(F As clsFfFascia, O As clsFfOttimo, xs() As Double, ys() As Double, ws() As Double,
                                     Segno As Double, xHi As Double, P As clsFfParametri)
    Dim n As Integer = xs.Length
    Dim z(n - 1) As Double
    For i As Integer = 0 To n - 1
      z(i) = Segno * ys(i)
    Next

    Dim MigliorFit() As Double = Nothing
    Dim MigliorM As Integer = -1
    Dim MigliorSse As Double = Double.MaxValue
    For m As Integer = 0 To n - 1
      ' parte crescente fino al picco (incluso)
      Dim zl(m) As Double, wl(m) As Double
      For i As Integer = 0 To m
        zl(i) = z(i)
        wl(i) = ws(i)
      Next
      Dim Sx() As Double = Pava(zl, wl)
      ' parte decrescente dopo il picco: PAVA sull'ordine inverso
      Dim nr As Integer = n - 1 - m
      Dim Dx() As Double = New Double() {}
      If nr > 0 Then
        Dim zr(nr - 1) As Double, wr(nr - 1) As Double
        For i As Integer = 0 To nr - 1
          zr(i) = z(n - 1 - i)
          wr(i) = ws(n - 1 - i)
        Next
        Dim Inv() As Double = Pava(zr, wr)
        ReDim Dx(nr - 1)
        For i As Integer = 0 To nr - 1
          Dx(i) = Inv(nr - 1 - i)
        Next
        ' deve restare unimodale: dopo il picco non si supera il valore del picco
        If Dx(0) > Sx(m) + 0.000000000001 Then Continue For
      End If
      Dim Fit(n - 1) As Double
      Dim Sse As Double = 0
      For i As Integer = 0 To n - 1
        Fit(i) = If(i <= m, Sx(i), Dx(i - m - 1))
        Sse += ws(i) * (z(i) - Fit(i)) ^ 2
      Next
      If Sse < MigliorSse - 0.000000000001 Then
        MigliorSse = Sse
        MigliorM = m
        MigliorFit = Fit
      End If
    Next

    ' R2 pesato
    Dim zMed As Double = 0, wTot As Double = 0
    For i As Integer = 0 To n - 1
      zMed += z(i) * ws(i)
      wTot += ws(i)
    Next
    zMed /= wTot
    Dim ssTot As Double = 0
    For i As Integer = 0 To n - 1
      ssTot += ws(i) * (z(i) - zMed) ^ 2
    Next
    O.R2 = If(ssTot > 0, 1 - MigliorSse / ssTot, Double.NaN)

    Dim xOpt As Double = xs(MigliorM)
    Dim yOpt As Double = Segno * MigliorFit(MigliorM)
    If MigliorM = n - 1 AndAlso n >= 2 AndAlso xs(n - 1) > xs(n - 2) Then
      Dim Pend As Double = (MigliorFit(n - 1) - MigliorFit(n - 2)) / (xs(n - 1) - xs(n - 2))
      If Pend > 0 AndAlso xHi > xs(n - 1) Then
        xOpt = xHi
        yOpt = Segno * (MigliorFit(n - 1) + Pend * (xHi - xs(n - 1)))
        O.Estrapolato = True
      End If
    End If
    O.X = xOpt
    O.Y = yOpt
    For i As Integer = 0 To n - 1
      F.CurvaX.Add(xs(i))
      F.CurvaY.Add(Segno * MigliorFit(i))
    Next
    If O.Estrapolato Then
      F.CurvaX.Add(xOpt)
      F.CurvaY.Add(yOpt)
    End If
  End Sub

  ' ---------------------------------------------------------------- sensitivita' della prestazione ai canali

  ''' <summary>Bin a quantili del canale usati per la quota spiegata e il valore migliore.</summary>
  Public Const BinSensitivita As Integer = 8
  ''' <summary>Sotto questa quota spiegata (eta^2) il canale praticamente non conta per la prestazione.</summary>
  Public Const QuotaSpiegataMinima As Double = 0.02

  ''' <summary>
  ''' Sensitivita' per fascia della prestazione X (per esempio VmgPerc) al canale Y: stessi campioni della sorgente, del filtro e della fascia
  ''' usati dal feed forward, ma con qualsiasi valore di X (limitare X per bin taglierebbe proprio la variabile che si misura).
  ''' Sensitivita' = pendenza della retta X su Y per la deviazione standard di Y = di quanto cambia X quando il canale si sposta della sua
  ''' variazione tipica. Quota spiegata: i campioni divisi in BinSensitivita bin a quantili di Y (stesso numero di campioni), varianza delle
  ''' medie di X tra i bin sulla varianza totale di X. Valore migliore: Y medio del bin con X medio piu' alto.
  ''' </summary>
  Public Shared Function Sensitivita(X() As Double, Y() As Double, Tws() As Double, Righe As IEnumerable(Of Integer),
                                     Maschera As Func(Of Integer, Boolean), P As clsFfParametri) As List(Of clsFfSensitivita)
    Dim Out As New List(Of clsFfSensitivita)
    If X Is Nothing OrElse Y Is Nothing OrElse Tws Is Nothing OrElse P Is Nothing OrElse P.TwsStep <= 0 Then Return Out
    Dim NFasce As Integer = System.Math.Max(1, CInt(System.Math.Ceiling((P.TwsMax - P.TwsMin) / P.TwsStep - 0.000000001)))
    Dim PerFascia As New List(Of List(Of Integer))
    For f As Integer = 0 To NFasce - 1
      Out.Add(New clsFfSensitivita With {.TwsLow = P.TwsMin + f * P.TwsStep, .TwsHigh = P.TwsMin + (f + 1) * P.TwsStep})
      PerFascia.Add(New List(Of Integer))
    Next
    Dim n As Integer = System.Math.Min(X.Length, System.Math.Min(Y.Length, Tws.Length))
    For Each i As Integer In Righe
      If i < 0 OrElse i >= n Then Continue For
      If Double.IsNaN(X(i)) OrElse Double.IsNaN(Y(i)) OrElse Double.IsNaN(Tws(i)) OrElse Double.IsInfinity(X(i)) OrElse Double.IsInfinity(Y(i)) Then Continue For
      If Maschera IsNot Nothing AndAlso Not Maschera(i) Then Continue For
      If Tws(i) < P.TwsMin OrElse Tws(i) >= P.TwsMax Then Continue For
      Dim fa As Integer = CInt(System.Math.Floor((Tws(i) - P.TwsMin) / P.TwsStep))
      If fa < 0 OrElse fa >= NFasce Then Continue For
      PerFascia(fa).Add(i)
    Next

    For fa As Integer = 0 To NFasce - 1
      Dim S As clsFfSensitivita = Out(fa)
      Dim Idx As List(Of Integer) = PerFascia(fa)
      S.Campioni = Idx.Count
      If Idx.Count < BinSensitivita * System.Math.Max(5, P.MinCampioniBin) Then
        S.Messaggio = If(Idx.Count = 0, "No samples", "Not enough samples (" & Idx.Count & ")")
        Continue For
      End If
      ' medie, varianze e covarianza sui campioni
      Dim Mx As Double = 0, My As Double = 0
      For Each i As Integer In Idx
        Mx += X(i)
        My += Y(i)
      Next
      Mx /= Idx.Count
      My /= Idx.Count
      Dim Vx As Double = 0, Vy As Double = 0, Cxy As Double = 0
      For Each i As Integer In Idx
        Vx += (X(i) - Mx) * (X(i) - Mx)
        Vy += (Y(i) - My) * (Y(i) - My)
        Cxy += (X(i) - Mx) * (Y(i) - My)
      Next
      If Vy <= 0 Then
        S.Messaggio = "The channel is constant"
        Continue For
      End If
      S.VariazioneTipica = System.Math.Sqrt(Vy / (Idx.Count - 1))
      S.Sensitivita = (Cxy / Vy) * S.VariazioneTipica

      ' bin a quantili del canale
      Dim Ord As List(Of Integer) = Idx.OrderBy(Function(q) Y(q)).ToList
      Dim Tra As Double = 0
      For b As Integer = 0 To BinSensitivita - 1
        Dim i0 As Integer = CInt(System.Math.Floor(b * Ord.Count / CDbl(BinSensitivita)))
        Dim i1 As Integer = CInt(System.Math.Floor((b + 1) * Ord.Count / CDbl(BinSensitivita))) - 1
        If i1 < i0 Then Continue For
        Dim Sx As Double = 0, Sy As Double = 0
        For k As Integer = i0 To i1
          Sx += X(Ord(k))
          Sy += Y(Ord(k))
        Next
        Dim Nb As Integer = i1 - i0 + 1
        Dim Bn As New clsFfBinSensitivita With {.CanaleMin = Y(Ord(i0)), .CanaleMax = Y(Ord(i1)), .CanaleMedio = Sy / Nb, .PrestazioneMedia = Sx / Nb, .N = Nb}
        S.Bins.Add(Bn)
        Tra += Nb * (Bn.PrestazioneMedia - Mx) * (Bn.PrestazioneMedia - Mx)
      Next
      S.QuotaSpiegata = If(Vx > 0, Tra / Vx, Double.NaN)
      If S.Bins.Count > 0 Then
        Dim Best As Integer = 0
        For b As Integer = 1 To S.Bins.Count - 1
          If S.Bins(b).PrestazioneMedia > S.Bins(Best).PrestazioneMedia Then Best = b
        Next
        S.ValoreMigliore = S.Bins(Best).CanaleMedio
        S.GuadagnoMigliore = S.Bins(Best).PrestazioneMedia - Mx
        S.MiglioreAlBordo = Best = 0 OrElse Best = S.Bins.Count - 1
      End If
    Next
    Return Out
  End Function

  ' ---------------------------------------------------------------- minimi quadrati pesati

  Private Shared Function AdattaPoli(xs() As Double, ys() As Double, ws() As Double, Grado As Integer, Scala As Double) As clsPoli
    Dim n As Integer = xs.Length
    If n <= Grado Then Return Nothing
    Dim p As Integer = Grado + 1
    Dim wTot As Double = ws.Sum
    Dim xref As Double = 0
    For i As Integer = 0 To n - 1
      xref += xs(i) * ws(i)
    Next
    xref /= wTot

    Dim A(p - 1, p - 1) As Double
    Dim B(p - 1) As Double
    For i As Integer = 0 To n - 1
      Dim u As Double = (xs(i) - xref) / Scala
      Dim v(p - 1) As Double
      v(0) = 1
      For k As Integer = 1 To p - 1
        v(k) = v(k - 1) * u
      Next
      For r As Integer = 0 To p - 1
        B(r) += ws(i) * v(r) * ys(i)
        For c As Integer = 0 To p - 1
          A(r, c) += ws(i) * v(r) * v(c)
        Next
      Next
    Next

    Dim Inv(,) As Double = Inverti(A, p)
    If Inv Is Nothing Then Return Nothing
    Dim Cf(p - 1) As Double
    For r As Integer = 0 To p - 1
      For c As Integer = 0 To p - 1
        Cf(r) += Inv(r, c) * B(c)
      Next
    Next

    Dim Cur As New clsPoli With {.Grado = Grado, .Coef = Cf, .Xref = xref, .Scala = Scala, .Inv = Inv, .Gdl = n - p}
    Dim yMed As Double = 0
    For i As Integer = 0 To n - 1
      yMed += ys(i) * ws(i)
    Next
    yMed /= wTot
    Dim ssRes As Double = 0, ssTot As Double = 0
    For i As Integer = 0 To n - 1
      ssRes += ws(i) * (ys(i) - Cur.Valore(xs(i))) ^ 2
      ssTot += ws(i) * (ys(i) - yMed) ^ 2
    Next
    If ssTot > 0 Then Cur.R2 = 1 - ssRes / ssTot
    If Cur.Gdl > 0 Then Cur.S2 = ssRes / Cur.Gdl
    Return Cur
  End Function

  ''' <summary>Inversa con Gauss-Jordan e pivot parziale; Nothing se la matrice e' singolare.</summary>
  Private Shared Function Inverti(A(,) As Double, n As Integer) As Double(,)
    Dim M(n - 1, 2 * n - 1) As Double
    For i As Integer = 0 To n - 1
      For j As Integer = 0 To n - 1
        M(i, j) = A(i, j)
      Next
      M(i, n + i) = 1
    Next
    For col As Integer = 0 To n - 1
      Dim piv As Integer = col
      For r As Integer = col + 1 To n - 1
        If System.Math.Abs(M(r, col)) > System.Math.Abs(M(piv, col)) Then piv = r
      Next
      If System.Math.Abs(M(piv, col)) < 0.000000000001 Then Return Nothing
      If piv <> col Then
        For j As Integer = 0 To 2 * n - 1
          Dim t As Double = M(col, j)
          M(col, j) = M(piv, j)
          M(piv, j) = t
        Next
      End If
      Dim d As Double = M(col, col)
      For j As Integer = 0 To 2 * n - 1
        M(col, j) /= d
      Next
      For r As Integer = 0 To n - 1
        If r <> col Then
          Dim f As Double = M(r, col)
          If f <> 0 Then
            For j As Integer = 0 To 2 * n - 1
              M(r, j) -= f * M(col, j)
            Next
          End If
        End If
      Next
    Next
    Dim Inv(n - 1, n - 1) As Double
    For i As Integer = 0 To n - 1
      For j As Integer = 0 To n - 1
        Inv(i, j) = M(i, n + j)
      Next
    Next
    Return Inv
  End Function

End Class
