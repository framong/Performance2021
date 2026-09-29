Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text

' =====================================================================================
'  Analisi manovre + stima corrente con calibrazione log/bussola asimmetrica per mura.
'
'  Pipeline:
'   1) Dai canali (time, twa, yawrate, ...) trova i cambi di mura (cambio segno twa),
'      classifica bolina/poppa (|twa| < / > 90) e delimita la TRANSIZIONE della manovra
'      dal profilo di yawrate:
'         - inizio manovra = primo istante in cui |yawrate| >= soglia
'         - fine   manovra = ultimo istante in cui |yawrate| >= soglia
'         - periodo transitorio = [inizio - preBuffer , fine + postBuffer]
'      Il cambio mura e' accettato SOLO se attorno ad esso |yawrate| supera la soglia.
'   2) Per ogni manovra costruisce una coppia di finestre di misura, lunghe "period":
'         - BEFORE = [transStart - period , transStart]
'         - AFTER  = [transEnd            , transEnd + period]
'      In ciascuna finestra la barca va dritta: si media il vettore acqua (bs,cse) e il
'      vettore suolo (sog,cog) -> un "tratto" (leg) con la sua mura (segno twa).
'   3) Stima la corrente imponendo coerenza dei vettori-corrente. Estensione rispetto
'      all'algoritmo base: l'offset del cse puo' essere DIVERSO tra le due mure
'      (dPlus per twa>0, dMinus per twa<0), mentre il coefficiente log k e' condiviso.
'
'  IDENTIFICABILITA' (vedi note): con k, dPlus e dMinus liberi il modello e' identificabile
'  solo se c'e' diversita' di prua ENTRO ciascuna mura. Su un bordeggio a due sole prue
'  (caso tipico windward-leeward) k e gli offset si scambiano senza cambiare il residuo:
'  in quel caso si FISSA k (assumedLog, es. ricavato prima dall'algoritmo base su tratti
'  dritti) e dPlus/dMinus/corrente tornano identificabili. La modalita' Auto rileva la
'  degenerazione dallo spread di prua per mura e sceglie di conseguenza.
'
'  ORIZZONTE DI COERENZA: la corrente e' assunta costante entro "coherenceHorizon" secondi
'  (default: infinito = tutta la finestra d'analisi). Manovre oltre l'orizzonte formano
'  epoche separate con corrente propria. NB: imporre coerenza solo dentro-coppia (mure
'  opposte) NON basta a identificare gli offset per-mura.
'
'  Convenzioni: E = v*sin(brg), N = v*cos(brg); rilevamento orario da Nord in gradi.
'               set = direzione VERSO cui scorre la corrente. cse_vero = cse + offset(mura).
' =====================================================================================


''' <summary>Vettore 2D nel piano (Est, Nord).</summary>
Public Structure V2
  Public ReadOnly E As Double
  Public ReadOnly N As Double

  Public Sub New(east As Double, north As Double)
    E = east : N = north
  End Sub

  ''' <summary>Da velocita' e rilevamento (gradi).</summary>
  Public Shared Function FromPolar(speed As Double, bearingDeg As Double) As V2
    Dim r As Double = bearingDeg * Math.PI / 180.0
    Return New V2(speed * Math.Sin(r), speed * Math.Cos(r))
  End Function

  ''' <summary>Ruota il rilevamento di deltaDeg (prua += deltaDeg).</summary>
  Public Function Rotated(deltaDeg As Double) As V2
    Dim d As Double = deltaDeg * Math.PI / 180.0
    Dim c As Double = Math.Cos(d), s As Double = Math.Sin(d)
    Return New V2(E * c + N * s, -E * s + N * c)
  End Function

  Public ReadOnly Property Mag As Double
    Get
      Return Math.Sqrt(E * E + N * N)
    End Get
  End Property

  Public ReadOnly Property BearingDeg As Double
    Get
      Return (Math.Atan2(E, N) * 180.0 / Math.PI + 360.0) Mod 360.0
    End Get
  End Property

  Public Function Dot(o As V2) As Double
    Return E * o.E + N * o.N
  End Function

  Public Shared Operator +(a As V2, b As V2) As V2
    Return New V2(a.E + b.E, a.N + b.N)
  End Operator
  Public Shared Operator -(a As V2, b As V2) As V2
    Return New V2(a.E - b.E, a.N - b.N)
  End Operator
  Public Shared Operator *(a As V2, s As Double) As V2
    Return New V2(a.E * s, a.N * s)
  End Operator
End Structure


''' <summary>Canali di dati (array paralleli, stesso indice = stesso istante).</summary>
Public Class NavChannels
  Public Property Time As DateTime()
  Public Property Lat As Double()
  Public Property Lon As Double()      ' 'long' e' parola riservata -> Lon
  Public Property Bs As Double()       ' boat speed / STW (log)          [nodi]
  Public Property Sog As Double()      ' speed over ground (GPS)         [nodi]
  Public Property Cog As Double()      ' course over ground (GPS)        [gradi]
  Public Property Hdg As Double()      ' heading                         [gradi]
  Public Property Lwy As Double()      ' leeway                          [gradi]
  Public Property Cse As Double()      ' course through water            [gradi]
  Public Property Twa As Double()      ' true wind angle (segno = mura)  [gradi]
  Public Property YawRate As Double()  ' rate of turn                    [gradi/s]

  Public ReadOnly Property Count As Integer
    Get
      Return If(Time Is Nothing, 0, Time.Length)
    End Get
  End Property


  'Public Function SetChannels(startTimeIdx As Integer, endTimeIdx As Integer) As NavChannels

  '  Dim chTm As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eDateTime)
  '  Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
  '  Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
  '  Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
  '  Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
  '  Dim chSog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
  '  Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
  '  Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
  '  Dim chLwy As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLWY)
  '  Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  '  Dim chYRT As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)



  '  Time = chTm.ValoriDT.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Lat = chLat.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Lon = chLon.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Bs = chBs.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Sog = chSog.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Cog = chCog.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Hdg = chHdg.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Lwy = chLwy.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Cse = chCse.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  Twa = chTwa.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()
  '  YawRate = chYRT.Valori.Skip(startTimeIdx).Take(endTimeIdx - startTimeIdx + 1).ToArray()

  '  Return Me
  'End Function

  Public Function SetChannels(startTimeIdx As Integer, endTimeIdx As Integer) As NavChannels
    Dim count As Integer = endTimeIdx - startTimeIdx + 1
    If count <= 0 Then Throw New ArgumentException("Intervallo vuoto: endTimeIdx < startTimeIdx.")

    Time = Slice(DataProvider2020.TimeStamps, startTimeIdx, count)
    Lat = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori, startTimeIdx, count)
    Lon = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori, startTimeIdx, count)
    Bs = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW).Valori, startTimeIdx, count)
    Sog = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG).Valori, startTimeIdx, count)
    Cog = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG).Valori, startTimeIdx, count)
    Hdg = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG).Valori, startTimeIdx, count)
    Lwy = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLWY).Valori, startTimeIdx, count)
    Cse = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE).Valori, startTimeIdx, count)
    Twa = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA).Valori, startTimeIdx, count)
    YawRate = Slice(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT).Valori, startTimeIdx, count)
    Return Me
  End Function

  ' Slice veloce: Array.Copy per gli array, accesso indicizzato O(1) per IList;
  ' Skip solo come fallback (evita la scansione O(startTimeIdx) di Enumerable.Skip).
  Private Shared Function Slice(Of T)(src As IEnumerable(Of T), start As Integer, count As Integer) As T()
    Dim result(count - 1) As T
    Dim arr As T() = TryCast(src, T())
    If arr IsNot Nothing Then
      Array.Copy(arr, start, result, 0, count)
      Return result
    End If
    Dim lst As IList(Of T) = TryCast(src, IList(Of T))
    If lst IsNot Nothing Then
      For i As Integer = 0 To count - 1
        result(i) = lst(start + i)
      Next
      Return result
    End If
    Return src.Skip(start).Take(count).ToArray()   ' fallback generico
  End Function


End Class


''' <summary>Modalita' di calibrazione.</summary>
Public Enum CalibMode
  Auto        ' sceglie Full o FixedLog dallo spread di prua per mura
  Full        ' stima k, dPlus, dMinus (richiede diversita' di prua)
  FixedLog    ' fissa k = assumedLog, stima solo dPlus, dMinus (robusto su 2 prue)
End Enum


''' <summary>Risultato per singola manovra (coppia before/after).</summary>
Public Class ManeuverResult
  ' contesto temporale
  Public Property TackChangeTime As DateTime
  Public Property TransitionStart As DateTime
  Public Property TransitionEnd As DateTime
  Public Property BeforeStart As DateTime
  Public Property BeforeEnd As DateTime
  Public Property AfterStart As DateTime
  Public Property AfterEnd As DateTime
  ' tipo manovra
  Public Property IsUpwind As Boolean       ' True = bolina (tack), False = poppa (gybe)
  Public Property BeforeTackSign As Integer ' +1 (twa>0) / -1 (twa<0)
  Public Property AfterTackSign As Integer
  ' posizione media dell'area della manovra (BeforeStart..AfterEnd); NaN se lat/lon assenti
  Public Property MeanLat As Double
  Public Property MeanLon As Double
  ' corrente (dell'epoca cui appartiene la manovra)
  Public Property EpochId As Integer
  Public Property CurrentSet As Double      ' [gradi] verso cui scorre
  Public Property CurrentDrift As Double    ' [nodi]
  Public Property PairResidual As Double    ' |corrente_before - corrente_after| [nodi]
  ' calibrazione globale (uguale per tutti gli elementi)
  Public Property LogCoefficient As Double
  Public Property CompassOffsetStbd As Double   ' dPlus  (mura twa>0)  [gradi]
  Public Property CompassOffsetPort As Double   ' dMinus (mura twa<0)  [gradi]
  ' diagnostica
  Public Property ModeUsed As CalibMode
  Public Property UnderIdentified As Boolean    ' True se degenere (k fissato d'ufficio)
  Public Property HeadingSpreadStbd As Double   ' [gradi]
  Public Property HeadingSpreadPort As Double   ' [gradi]
  Public Property GlobalRms As Double           ' RMS residui su tutti i tratti [nodi]
End Class




Public Module ResultsClipboard

  ' Cultura per i numeri:
  '  - InvariantCulture -> separatore decimale '.'  (file di testo portabile)
  '  - Per incollare in Excel con locale IT, usa CultureInfo.GetCultureInfo("it-IT")
  '    cosi' i decimali usano la virgola (il TAB resta il separatore di colonna, nessuna ambiguita').
  Private ReadOnly Ci As CultureInfo = CultureInfo.InvariantCulture
  Private Const DtFmt As String = "yyyy-MM-dd HH:mm:ss.fff"

  ''' <summary>Costruisce il TSV (tab + CRLF) con tutte le proprieta' e lo mette negli appunti. Restituisce anche la stringa.</summary>
  Public Function CopyResultsToClipboard(results As ManeuverResult()) As String
    Dim sb As New StringBuilder()

    Dim headers As String() = {
            "TackChangeTime", "TransitionStart", "TransitionEnd", "BeforeStart", "BeforeEnd",
            "AfterStart", "AfterEnd", "IsUpwind", "BeforeTackSign", "AfterTackSign",
            "MeanLat", "MeanLon", "EpochId", "CurrentSet", "CurrentDrift", "PairResidual",
            "LogCoefficient", "CompassOffsetStbd", "CompassOffsetPort", "ModeUsed",
            "UnderIdentified", "HeadingSpreadStbd", "HeadingSpreadPort", "GlobalRms"}
    sb.Append(String.Join(vbTab, headers)).Append(vbCrLf)

    If results IsNot Nothing Then
      For Each r In results
        Dim cols As String() = {
                    r.TackChangeTime.ToString(DtFmt, Ci),
                    r.TransitionStart.ToString(DtFmt, Ci),
                    r.TransitionEnd.ToString(DtFmt, Ci),
                    r.BeforeStart.ToString(DtFmt, Ci),
                    r.BeforeEnd.ToString(DtFmt, Ci),
                    r.AfterStart.ToString(DtFmt, Ci),
                    r.AfterEnd.ToString(DtFmt, Ci),
                    r.IsUpwind.ToString(),
                    r.BeforeTackSign.ToString(Ci),
                    r.AfterTackSign.ToString(Ci),
                    r.MeanLat.ToString("0.000000", Ci),
                    r.MeanLon.ToString("0.000000", Ci),
                    r.EpochId.ToString(Ci),
                    r.CurrentSet.ToString("0.0", Ci),
                    r.CurrentDrift.ToString("0.000", Ci),
                    r.PairResidual.ToString("0.000", Ci),
                    r.LogCoefficient.ToString("0.0000", Ci),
                    r.CompassOffsetStbd.ToString("0.000", Ci),
                    r.CompassOffsetPort.ToString("0.000", Ci),
                    r.ModeUsed.ToString(),
                    r.UnderIdentified.ToString(),
                    r.HeadingSpreadStbd.ToString("0.0", Ci),
                    r.HeadingSpreadPort.ToString("0.0", Ci),
                    r.GlobalRms.ToString("0.000", Ci)}
        sb.Append(String.Join(vbTab, cols)).Append(vbCrLf)
      Next
    End If

    Dim text As String = sb.ToString()

    ' Deve girare su thread STA (il thread UI lo e'). SetDataObject(..., True)
    ' mantiene il contenuto negli appunti anche dopo la chiusura dell'app.
    Clipboard.SetDataObject(text, True)

    Return text
  End Function

End Module

Public Module ManeuverCurrentAnalyzer

  ' ---- tratto medio (leg) interno ----
  Private Class Leg
    Public Water As V2
    Public Ground As V2
    Public Tack As Integer
    Public Epoch As Integer
    Public HeadingDeg As Double
  End Class

  Private Class Pair
    Public Before As Leg
    Public After As Leg
    Public TackChange As DateTime
    Public IsUpwind As Boolean
    Public TransStart As DateTime
    Public TransEnd As DateTime
    Public BeforeStart As DateTime
    Public BeforeEnd As DateTime
    Public AfterStart As DateTime
    Public AfterEnd As DateTime
    Public Epoch As Integer
  End Class

  ''' <summary>
  ''' Analizza le manovre in [startTime, endTime] e stima la corrente per ciascuna.
  ''' </summary>
  ''' <param name="ch">Canali di dati.</param>
  ''' <param name="period">Durata finestre di misura before/after [s]. Default 40.</param>
  ''' <param name="yawThreshold">Soglia |yawrate| per "sta ruotando" [gradi/s]. Default 3.</param>
  ''' <param name="preBuffer">Anticipo transizione prima del superamento soglia [s]. Default 5.</param>
  ''' <param name="postBuffer">Coda transizione dopo il rientro sotto soglia [s]. Default 40.</param>
  ''' <param name="searchWindow">Semi-finestra di ricerca del picco yaw attorno al cambio mura [s]. Default 15.</param>
  ''' <param name="coherenceHorizon">Corrente costante entro questi [s]. Default infinito (una sola epoca).</param>
  ''' <param name="mode">Auto / Full / FixedLog.</param>
  ''' <param name="assumedLog">k da usare in FixedLog (es. ricavato dall'algoritmo base). Default 1.0.</param>
  ''' <param name="minHeadingSpreadDeg">Spread di prua minimo per considerare identificabile il modello pieno. Default 12.</param>
  Public Function Analyze(ch As NavChannels,
                          startTime As DateTime, endTime As DateTime,
                          Optional period As Double = 40.0,
                          Optional yawThreshold As Double = 3.0,
                          Optional preBuffer As Double = 5.0,
                          Optional postBuffer As Double = 40.0,
                          Optional searchWindow As Double = 15.0,
                          Optional coherenceHorizon As Double = Double.PositiveInfinity,
                          Optional mode As CalibMode = CalibMode.Auto,
                          Optional assumedLog As Double = 1.0,
                          Optional minHeadingSpreadDeg As Double = 12.0) As ManeuverResult()

    If ch Is Nothing OrElse ch.Count < 3 Then Return Array.Empty(Of ManeuverResult)()

    Dim t As DateTime() = ch.Time
    Dim twa As Double() = ch.Twa
    Dim yaw As Double() = ch.YawRate
    Dim nSamp As Integer = ch.Count

    ' -------- 1) segmentazione: trova coppie --------
    Dim pairs As New List(Of Pair)()
    Dim i As Integer = 0
    While i < nSamp - 1
      ' entro finestra temporale
      If t(i) < startTime OrElse t(i + 1) > endTime Then
        i += 1 : Continue While
      End If
      ' cambio mura = cambio segno twa
      If twa(i) * twa(i + 1) >= 0 Then
        i += 1 : Continue While
      End If

      Dim tChange As DateTime = t(i + 1)
      Dim upwind As Boolean = (Math.Abs(twa(i)) < 90.0)

      ' picco |yaw| attorno al cambio mura
      Dim pk As Integer = -1
      Dim pkVal As Double = -1.0
      For j As Integer = 0 To nSamp - 1
        If Math.Abs((t(j) - tChange).TotalSeconds) <= searchWindow Then
          If Math.Abs(yaw(j)) > pkVal Then pkVal = Math.Abs(yaw(j)) : pk = j
        End If
      Next
      If pk < 0 OrElse pkVal < yawThreshold Then
        ' non e' una vera manovra (nessuna rotazione): scarta
        i += 1 : Continue While
      End If

      ' estende il run contiguo sopra soglia
      Dim a As Integer = pk
      While a > 0 AndAlso Math.Abs(yaw(a - 1)) >= yawThreshold
        a -= 1
      End While
      Dim b As Integer = pk
      While b < nSamp - 1 AndAlso Math.Abs(yaw(b + 1)) >= yawThreshold
        b += 1
      End While

      Dim transStart As DateTime = t(a).AddSeconds(-preBuffer)
      Dim transEnd As DateTime = t(b).AddSeconds(postBuffer)
      Dim bStart As DateTime = transStart.AddSeconds(-period)
      Dim bEnd As DateTime = transStart
      Dim aStart As DateTime = transEnd
      Dim aEnd As DateTime = transEnd.AddSeconds(period)

      If bStart >= startTime AndAlso aEnd <= endTime Then
        Dim legB As Leg = MeanLeg(ch, bStart, bEnd)
        Dim legA As Leg = MeanLeg(ch, aStart, aEnd)
        If legB IsNot Nothing AndAlso legA IsNot Nothing Then
          pairs.Add(New Pair With {
              .Before = legB, .After = legA, .TackChange = tChange, .IsUpwind = upwind,
              .TransStart = transStart, .TransEnd = transEnd,
              .BeforeStart = bStart, .BeforeEnd = bEnd,
              .AfterStart = aStart, .AfterEnd = aEnd})
        End If
      End If

      ' salta oltre la fine del run per non ridetettare lo stesso cambio
      i = Math.Max(i + 1, b + 1)
    End While

    If pairs.Count = 0 Then Return Array.Empty(Of ManeuverResult)()

    ' -------- assegna epoche di coerenza --------
    pairs.Sort(Function(p1, p2) p1.TackChange.CompareTo(p2.TackChange))
    Dim epoch As Integer = 0
    Dim epochStart As DateTime = pairs(0).TackChange
    For Each p In pairs
      If (p.TackChange - epochStart).TotalSeconds > coherenceHorizon Then
        epoch += 1 : epochStart = p.TackChange
      End If
      p.Epoch = epoch
      p.Before.Epoch = epoch : p.After.Epoch = epoch
    Next
    Dim nEpochs As Integer = epoch + 1

    ' -------- tutti i tratti --------
    Dim legs As New List(Of Leg)()
    For Each p In pairs
      legs.Add(p.Before) : legs.Add(p.After)
    Next

    ' -------- diagnostica: spread di prua per mura --------
    Dim hPlus = legs.Where(Function(l) l.Tack > 0).Select(Function(l) l.HeadingDeg).ToList()
    Dim hMinus = legs.Where(Function(l) l.Tack < 0).Select(Function(l) l.HeadingDeg).ToList()
    Dim spreadPlus As Double = CircularSpreadDeg(hPlus)
    Dim spreadMinus As Double = CircularSpreadDeg(hMinus)

    ' -------- scelta modalita' --------
    Dim useFull As Boolean
    Select Case mode
      Case CalibMode.Full : useFull = True
      Case CalibMode.FixedLog : useFull = False
      Case Else ' Auto
        useFull = (hPlus.Count >= 2 AndAlso hMinus.Count >= 2 _
                   AndAlso spreadPlus >= minHeadingSpreadDeg _
                   AndAlso spreadMinus >= minHeadingSpreadDeg)
    End Select
    Dim underId As Boolean = Not useFull

    ' -------- stima (block coordinate descent, passi in forma chiusa) --------
    Dim k As Double, dPlus As Double, dMinus As Double
    Dim currents(nEpochs - 1) As V2
    Fit(legs, nEpochs, useFull, assumedLog, k, dPlus, dMinus, currents)

    ' RMS globale
    Dim ss As Double = 0.0
    For Each l In legs
      Dim off As Double = If(l.Tack > 0, dPlus, dMinus)
      Dim r As V2 = l.Ground - currents(l.Epoch) - (l.Water.Rotated(off) * k)
      ss += r.E * r.E + r.N * r.N
    Next
    Dim rms As Double = Math.Sqrt(ss / legs.Count)

    ' -------- costruisce risultati --------
    Dim res As New List(Of ManeuverResult)()
    For Each p In pairs
      Dim c As V2 = currents(p.Epoch)
      Dim offB As Double = If(p.Before.Tack > 0, dPlus, dMinus)
      Dim offA As Double = If(p.After.Tack > 0, dPlus, dMinus)
      Dim curB As V2 = p.Before.Ground - (p.Before.Water.Rotated(offB) * k)
      Dim curA As V2 = p.After.Ground - (p.After.Water.Rotated(offA) * k)
      Dim mLat As Double, mLon As Double
      MeanPosition(ch, p.BeforeStart, p.AfterEnd, mLat, mLon)
      res.Add(New ManeuverResult With {
          .TackChangeTime = p.TackChange,
          .TransitionStart = p.TransStart, .TransitionEnd = p.TransEnd,
          .BeforeStart = p.BeforeStart, .BeforeEnd = p.BeforeEnd,
          .AfterStart = p.AfterStart, .AfterEnd = p.AfterEnd,
          .IsUpwind = p.IsUpwind,
          .BeforeTackSign = p.Before.Tack, .AfterTackSign = p.After.Tack,
          .MeanLat = mLat, .MeanLon = mLon,
          .EpochId = p.Epoch, .CurrentSet = c.BearingDeg, .CurrentDrift = c.Mag,
          .PairResidual = (curB - curA).Mag,
          .LogCoefficient = k, .CompassOffsetStbd = dPlus, .CompassOffsetPort = dMinus,
          .ModeUsed = If(useFull, CalibMode.Full, CalibMode.FixedLog),
          .UnderIdentified = underId,
          .HeadingSpreadStbd = spreadPlus, .HeadingSpreadPort = spreadMinus,
          .GlobalRms = rms})
    Next

    Return res.ToArray()
  End Function


  ' ------------------------------------------------------------------ media di un tratto
  Private Function MeanLeg(ch As NavChannels, tA As DateTime, tB As DateTime) As Leg
    Dim sumW As New V2(0, 0)
    Dim sumG As New V2(0, 0)
    Dim sumTwa As Double = 0.0
    Dim cnt As Integer = 0
    For j As Integer = 0 To ch.Count - 1
      Dim tj As DateTime = ch.Time(j)
      If tj >= tA AndAlso tj <= tB Then
        sumW = sumW + V2.FromPolar(ch.Bs(j), ch.Cse(j))
        sumG = sumG + V2.FromPolar(ch.Sog(j), ch.Cog(j))
        sumTwa += ch.Twa(j)
        cnt += 1
      End If
    Next
    If cnt < 2 Then Return Nothing
    Dim w As V2 = sumW * (1.0 / cnt)
    Dim g As V2 = sumG * (1.0 / cnt)
    Dim tackSign As Integer = Math.Sign(sumTwa)
    If tackSign = 0 Then tackSign = 1
    Return New Leg With {.Water = w, .Ground = g, .Tack = tackSign, .HeadingDeg = w.BearingDeg}
  End Function


  ' ------------------------------------------------------------------ posizione media
  ' Media aritmetica di lat/lon sui campioni in [tA, tB] (l'area di una manovra e'
  ' piccola: la media semplice e' adeguata). Restituisce NaN se lat/lon non disponibili.
  Private Sub MeanPosition(ch As NavChannels, tA As DateTime, tB As DateTime,
                           ByRef latOut As Double, ByRef lonOut As Double)
    latOut = Double.NaN : lonOut = Double.NaN
    If ch.Lat Is Nothing OrElse ch.Lon Is Nothing Then Return
    Dim sLat As Double = 0.0, sLon As Double = 0.0, cnt As Integer = 0
    For j As Integer = 0 To ch.Count - 1
      Dim tj As DateTime = ch.Time(j)
      If tj >= tA AndAlso tj <= tB Then
        sLat += ch.Lat(j) : sLon += ch.Lon(j) : cnt += 1
      End If
    Next
    If cnt > 0 Then
      latOut = sLat / cnt
      lonOut = sLon / cnt
    End If
  End Sub


  ' ------------------------------------------------------------------ stima (block descent)
  Private Sub Fit(legs As List(Of Leg), nEpochs As Integer,
                  fitLog As Boolean, assumedLog As Double,
                  ByRef k As Double, ByRef dPlus As Double, ByRef dMinus As Double,
                  currents As V2())
    k = If(fitLog, 1.0, assumedLog)
    dPlus = 0.0 : dMinus = 0.0
    Dim prevJ As Double = Double.MaxValue

    For iter As Integer = 1 To 500
      ' corrente per epoca = media( ground - k*R(off)*water )
      Dim accE(nEpochs - 1), accN(nEpochs - 1) As Double
      Dim cntEp(nEpochs - 1) As Integer
      For Each l In legs
        Dim off As Double = If(l.Tack > 0, dPlus, dMinus)
        Dim cal As V2 = l.Ground - (l.Water.Rotated(off) * k)
        accE(l.Epoch) += cal.E : accN(l.Epoch) += cal.N : cntEp(l.Epoch) += 1
      Next
      For e As Integer = 0 To nEpochs - 1
        If cntEp(e) > 0 Then currents(e) = New V2(accE(e) / cntEp(e), accN(e) / cntEp(e))
      Next

      ' k (solo se richiesto)
      If fitLog Then
        Dim num As Double = 0.0, den As Double = 0.0
        For Each l In legs
          Dim off As Double = If(l.Tack > 0, dPlus, dMinus)
          Dim err As V2 = l.Ground - currents(l.Epoch)
          Dim rw As V2 = l.Water.Rotated(off)
          num += err.Dot(rw) : den += l.Water.Dot(l.Water)
        Next
        If den > 0.000000001 Then k = num / den
      End If

      ' dPlus (mura twa>0) : massimizza somma err . R(d) water  ->  d = atan2(Q,P)
      dPlus = UpdateOffset(legs, currents, +1, dPlus)
      ' dMinus (mura twa<0)
      dMinus = UpdateOffset(legs, currents, -1, dMinus)

      ' costo
      Dim jj As Double = 0.0
      For Each l In legs
        Dim off As Double = If(l.Tack > 0, dPlus, dMinus)
        Dim r As V2 = l.Ground - currents(l.Epoch) - (l.Water.Rotated(off) * k)
        jj += r.E * r.E + r.N * r.N
      Next
      If Math.Abs(prevJ - jj) < 0.000000000001 Then Exit For
      prevJ = jj
    Next
  End Sub

  Private Function UpdateOffset(legs As List(Of Leg), currents As V2(),
                                tackSel As Integer, current As Double) As Double
    Dim P As Double = 0.0, Q As Double = 0.0, cnt As Integer = 0
    For Each l In legs
      If Math.Sign(l.Tack) <> tackSel Then Continue For
      Dim err As V2 = l.Ground - currents(l.Epoch)
      Dim w As V2 = l.Water
      P += err.E * w.E + err.N * w.N
      Q += err.E * w.N - err.N * w.E
      cnt += 1
    Next
    If cnt = 0 OrElse (P * P + Q * Q) < 0.000000000001 Then Return current
    Return Math.Atan2(Q, P) * 180.0 / Math.PI
  End Function


  ' ------------------------------------------------------------------ spread circolare
  Private Function CircularSpreadDeg(headings As List(Of Double)) As Double
    If headings Is Nothing OrElse headings.Count < 2 Then Return 0.0
    Dim se As Double = 0.0, sn As Double = 0.0
    For Each h In headings
      Dim r As Double = h * Math.PI / 180.0
      se += Math.Sin(r) : sn += Math.Cos(r)
    Next
    Dim meanBrg As Double = Math.Atan2(se, sn) * 180.0 / Math.PI
    Dim maxDev As Double = Double.NegativeInfinity, minDev As Double = Double.PositiveInfinity
    For Each h In headings
      Dim dev As Double = ((h - meanBrg + 540.0) Mod 360.0) - 180.0  ' -> (-180,180]
      maxDev = Math.Max(maxDev, dev) : minDev = Math.Min(minDev, dev)
    Next
    Return maxDev - minDev
  End Function

End Module