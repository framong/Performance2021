Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic   ' per CallByName (late binding sulle proprieta')

' =====================================================================================
'  Calibrazione log + bussola dalla coerenza della corrente su piu' tratti.
'
'  Idea: la corrente vera e' la stessa in tutti i tratti. Applicando un coefficiente k
'  al log (STW) e un offset delta alla bussola (HDG), si cercano k e delta che rendono
'  i vettori-corrente dei vari tratti il piu' possibile UGUALI tra loro.
'
'  Con 2 tratti la soluzione e' esatta; con N tratti e' un fit ai minimi quadrati che,
'  minimizzando la varianza dei vettori-corrente, ammette forma CHIUSA (fit di
'  similitudine rotazione+scala sui vettori centrati -> come Umeyama/Kabsch in 2D):
'
'      corrente_i = g_i - k * R(delta) * w_i          (GPS assunto esatto)
'      minimizza  Sum_i | corrente_i - corrente_media |^2
'
'   ->  delta = atan2(Q, P)          k = sqrt(P^2 + Q^2) / Sum|w_centrato|^2
'      con P = Sum (g' . w')  (dot)   Q = Sum (g'_E w'_N - g'_N w'_E)  (cross)
'      dove w' , g' sono i vettori (E,N) sull'acqua e al suolo centrati sulla media.
'
'  Convenzioni:
'    - E = v*sin(brg), N = v*cos(brg)   (rilevamento in gradi, orario da Nord)
'    - set = direzione VERSO cui scorre la corrente
'    - HDG_vero = cse + delta ;  STW_vera = bs * k
' =====================================================================================


''' <summary>Risultato della calibrazione.</summary>
Public Class CurrentCalibrationResult
  ''' <summary>Coefficiente log: STW_vera = bs * k.</summary>
  Public ReadOnly Property LogCoefficient As Double
  ''' <summary>Offset bussola in gradi: HDG_vero = cse + offset.</summary>
  Public ReadOnly Property CompassOffset As Double
  ''' <summary>Set della corrente comune [gradi], direzione VERSO cui scorre.</summary>
  Public ReadOnly Property CurrentSet As Double
  ''' <summary>Drift della corrente comune [nodi].</summary>
  Public ReadOnly Property CurrentDrift As Double
  ''' <summary>Scarto di ciascun tratto rispetto alla corrente comune [nodi].</summary>
  Public ReadOnly Property Residuals As Double()
  ''' <summary>RMS degli scarti [nodi]: qualita' del fit (0 = coerenza perfetta).</summary>
  Public ReadOnly Property Rms As Double

  Friend Sub New(k As Double, offsetDeg As Double, setDeg As Double,
                 drift As Double, res As Double(), rmsVal As Double)
    LogCoefficient = k
    CompassOffset = offsetDeg
    CurrentSet = setDeg
    CurrentDrift = drift
    Residuals = res
    Rms = rmsVal
  End Sub
End Class


Public Module CurrentSolver

  ''' <summary>
  ''' Calcola coefficiente log (k) e offset bussola (delta) che minimizzano la
  ''' differenza della corrente calcolata tra i tratti.
  ''' Ogni oggetto passato deve esporre le proprieta':
  '''   bs  = boat speed / STW (log)              [nodi]
  '''   sog = speed over ground (GPS)             [nodi]
  '''   cse = course/heading through water (bussola) [gradi]
  '''   cog = course over ground (GPS)            [gradi]
  ''' </summary>
  ''' <param name="legs">Matrice/collezione di oggetti con bs, sog, cse, cog.</param>
  Public Function SolveCurrentCalibration(legs As IEnumerable(Of Object)) As CurrentCalibrationResult
    Dim list As New List(Of Object)(legs)
    Dim n As Integer = list.Count
    If n < 2 Then Throw New ArgumentException("Servono almeno 2 tratti.")

    Const D2R As Double = Math.PI / 180.0
    Const R2D As Double = 180.0 / Math.PI

    Dim wE(n - 1), wN(n - 1), gE(n - 1), gN(n - 1) As Double
    Dim wmE, wmN, gmE, gmN As Double   ' medie (default 0)

    For i As Integer = 0 To n - 1
      Dim o As Object = list(i)
      Dim bs As Double = CDbl(CallByName(o, "bs", CallType.[Get]))
      Dim sog As Double = CDbl(CallByName(o, "sog", CallType.[Get]))
      Dim cse As Double = CDbl(CallByName(o, "cse", CallType.[Get]))
      Dim cog As Double = CDbl(CallByName(o, "cog", CallType.[Get]))

      wE(i) = bs * Math.Sin(cse * D2R) : wN(i) = bs * Math.Cos(cse * D2R)
      gE(i) = sog * Math.Sin(cog * D2R) : gN(i) = sog * Math.Cos(cog * D2R)
      wmE += wE(i) : wmN += wN(i) : gmE += gE(i) : gmN += gN(i)
    Next
    wmE /= n : wmN /= n : gmE /= n : gmN /= n

    ' P = somma dot(g',w') ; Q = somma cross(g',w') ; sww = somma |w'|^2
    Dim P, Q, sww As Double
    For i As Integer = 0 To n - 1
      Dim wcE As Double = wE(i) - wmE, wcN As Double = wN(i) - wmN
      Dim gcE As Double = gE(i) - gmE, gcN As Double = gN(i) - gmN
      P += gcE * wcE + gcN * wcN
      Q += gcE * wcN - gcN * wcE
      sww += wcE * wcE + wcN * wcN
    Next

    If sww < 0.000000001 Then _
        Throw New InvalidOperationException(
            "Tratti degeneri (rotte/velocita' sull'acqua troppo simili): " &
            "k e offset non sono separabili. Servono rotte diverse.")

    Dim deltaRad As Double = Math.Atan2(Q, P)
    Dim k As Double = Math.Sqrt(P * P + Q * Q) / sww
    Dim cosd As Double = Math.Cos(deltaRad), sind As Double = Math.Sin(deltaRad)

    ' corrente comune = media(ground) - k * R(delta) * media(water)
    ' R(delta): E' = E*cos + N*sin ; N' = -E*sin + N*cos
    Dim curE As Double = gmE - k * (wmE * cosd + wmN * sind)
    Dim curN As Double = gmN - k * (-wmE * sind + wmN * cosd)
    Dim setDeg As Double = (Math.Atan2(curE, curN) * R2D + 360.0) Mod 360.0
    Dim drift As Double = Math.Sqrt(curE * curE + curN * curN)

    ' scarti per tratto (distanza dal vettore-corrente comune)
    Dim res(n - 1) As Double
    Dim ss As Double
    For i As Integer = 0 To n - 1
      Dim ce As Double = wE(i) * cosd + wN(i) * sind
      Dim cn As Double = -wE(i) * sind + wN(i) * cosd
      Dim de As Double = (gE(i) - k * ce) - curE
      Dim dn As Double = (gN(i) - k * cn) - curN
      res(i) = Math.Sqrt(de * de + dn * dn)
      ss += de * de + dn * dn
    Next

    Return New CurrentCalibrationResult(k, deltaRad * R2D, setDeg, drift, res, Math.Sqrt(ss / n))
  End Function

End Module


''' <summary>Classe comoda se non hai gia' oggetti tuoi con quelle proprieta'.</summary>
Public Class NavLeg
  Public Property TR As clsTimeRange  ' STW (log)  [nodi]
  Public Property bs As Double     ' STW (log)  [nodi]
  Public Property sog As Double    ' SOG (GPS)  [nodi]
  Public Property cse As Double    ' HDG bussola [gradi]
  Public Property cog As Double    ' COG (GPS)  [gradi]

  Public Sub New()
  End Sub

  Public Sub New(bs As Double, sog As Double, cse As Double, cog As Double)
    Me.bs = bs : Me.sog = sog : Me.cse = cse : Me.cog = cog
  End Sub

  Public Sub New(tr As clsTimeRange, bs As Double, sog As Double, cse As Double, cog As Double)
    Me.TR = tr : Me.bs = bs : Me.sog = sog : Me.cse = cse : Me.cog = cog
  End Sub
End Class


' -------------------------------------------------------------------------------------
'  Esempio d'uso (rimuovere/spostare in un progetto Console per provarlo)
' -------------------------------------------------------------------------------------

Public Class CurrentCalibration
  Public Shared SegmentOne As NavLeg
  Public Shared SegmentTwo As NavLeg


  Public Shared Sub AddSegment(TR As clsTimeRange, first As Boolean)
    Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    Dim chSog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
    Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)

    Dim StatBs = New clsStatisticheIntervallo(chBs, TR)
    Dim StatCse = New clsStatisticheIntervallo(chCse, TR)
    Dim StatSog = New clsStatisticheIntervallo(chSog, TR)
    Dim StatCog = New clsStatisticheIntervallo(chCog, TR)


    If first Then
      SegmentOne = New NavLeg(TR, StatBs.Avg, StatSog.Avg, StatCse.Avg, StatCog.Avg)
    Else
      SegmentTwo = New NavLeg(TR, StatBs.Avg, StatSog.Avg, StatCse.Avg, StatCog.Avg)
    End If
  End Sub

  Public Shared Sub AddSegment(bs As Double, sog As Double, cse As Double, cog As Double, first As Boolean)
    If first Then
      SegmentOne = New NavLeg(bs, sog, cse, cog)
    Else
      SegmentTwo = New NavLeg(bs, sog, cse, cog)
    End If
  End Sub

  Public Shared Function GetResult() As String
    If SegmentOne Is Nothing OrElse SegmentTwo Is Nothing Then Throw New InvalidOperationException("2 segments needed.")
    Dim str As String = "Current Check"
    Dim S1 As String = SegmentOne.TR.StringaPeriodo & " " & SegmentOne.bs.ToString("F1") & " " & SegmentOne.sog.ToString("F1") & " " &
                       SegmentOne.cse.ToString("F1") & " " & SegmentOne.cog.ToString("F1")

    Dim S2 As String = SegmentTwo.TR.StringaPeriodo & " " & SegmentTwo.bs.ToString("F1") & " " & SegmentTwo.sog.ToString("F1") & " " &
                       SegmentTwo.cse.ToString("F1") & " " & SegmentTwo.cog.ToString("F1")

    str = str & vbCrLf & S1 & vbCrLf & S2

    Dim tratti As New List(Of Object) From {SegmentOne, SegmentTwo}

    Dim r As CurrentCalibrationResult = CurrentSolver.SolveCurrentCalibration(tratti)

    str = str & vbCrLf & "Speed K: " & r.LogCoefficient.ToString("F4") & vbCrLf
    str = str & vbCrLf & "Compass Offset: " & r.CompassOffset.ToString("F1") & vbCrLf
    str = str & vbCrLf & "Current dir/set: " & r.CurrentSet.ToString("F0") & vbCrLf
    str = str & vbCrLf & "Current rate/drift: " & r.CurrentDrift.ToString("F1") & vbCrLf

    Return str
  End Function


End Class


Module Demo
  Sub CurrentDemoMain()
    Dim tratti As New List(Of Object) From {
        New NavLeg(19.1, 18.7, 200.5, 202.8),
        New NavLeg(17.7, 17.4, 135.0, 136.8)
    }

    Dim r As CurrentCalibrationResult = CurrentSolver.SolveCurrentCalibration(tratti)

    Console.WriteLine($"Coeff. log (k)   = {r.LogCoefficient:F4}   -> Corrected Bs = bs * k")
    Console.WriteLine($"Compass Offset = {r.CompassOffset:+0.00;-0.00} deg  -> Corrected CSE = cse + offset")
    Console.WriteLine($"Calibrated Current  = dir/set {r.CurrentSet:F1} deg, rate/drift {r.CurrentDrift:F2} kn")
    Console.WriteLine($"RMS discards       = {r.Rms:F3} kn")
    For i As Integer = 0 To r.Residuals.Length - 1
      Console.WriteLine($"  segment {i + 1}: discard {r.Residuals(i):F3} kn")
    Next
    ' Atteso: k=0.9874, offset=+2.26, set 18.2, drift 0.16, RMS ~0
  End Sub
End Module