''' <summary>Punto della tabella SeaState normalizzato: SeaState atteso per una data intensita' del vento.</summary>
Public Class clsSeaStateNormPoint
  Public Property Tws As Double
  Public Property SeaState As Double

  Public Sub New()
  End Sub

  Public Sub New(Tws As Double, SeaState As Double)
    Me.Tws = Tws
    Me.SeaState = SeaState
  End Sub
End Class

''' <summary>
''' SeaState "normale" in funzione della TWS, interpolato linearmente dalla tabella del profilo
''' (clsProfile2021.SeaStateNormTable). Fuori dal range della tabella si usa il valore del punto estremo.
''' </summary>
Public Class clsSeaStateNorm

  Public Shared Function TabellaDefault() As List(Of clsSeaStateNormPoint)
    Return New List(Of clsSeaStateNormPoint) From {
      New clsSeaStateNormPoint(0, 0), New clsSeaStateNormPoint(5, 0.1), New clsSeaStateNormPoint(10, 0.4),
      New clsSeaStateNormPoint(15, 0.9), New clsSeaStateNormPoint(20, 1.3), New clsSeaStateNormPoint(25, 1.8),
      New clsSeaStateNormPoint(30, 2.2)}
  End Function

  ''' <summary>Tabella del profilo attivo, ordinata per TWS; se manca o e' vuota si usa quella di default.</summary>
  Public Shared Function TabellaCorrente() As List(Of clsSeaStateNormPoint)
    Dim Tab As List(Of clsSeaStateNormPoint) = Nothing
    If Not AppConfig Is Nothing AndAlso Not AppConfig.ActiveProfile Is Nothing Then Tab = AppConfig.ActiveProfile.SeaStateNormTable
    If Tab Is Nothing OrElse Tab.Count = 0 Then Tab = TabellaDefault()
    Return Tab.OrderBy(Function(p) p.Tws).ToList
  End Function

  Public Shared Function Interpola(Tab As List(Of clsSeaStateNormPoint), Tws As Double) As Double
    If Double.IsNaN(Tws) Then Return Double.NaN
    If Tws <= Tab.First.Tws Then Return Tab.First.SeaState
    If Tws >= Tab.Last.Tws Then Return Tab.Last.SeaState
    For i As Integer = 1 To Tab.Count - 1
      If Tws <= Tab(i).Tws Then
        Dim dx As Double = Tab(i).Tws - Tab(i - 1).Tws
        If dx <= 0 Then Return Tab(i).SeaState
        Return Tab(i - 1).SeaState + (Tab(i).SeaState - Tab(i - 1).SeaState) * (Tws - Tab(i - 1).Tws) / dx
      End If
    Next
    Return Tab.Last.SeaState
  End Function

  Public Shared Function CalcolaSerie(Tws() As Double) As Double()
    Dim Tab As List(Of clsSeaStateNormPoint) = TabellaCorrente()
    Dim Ris(Tws.Length - 1) As Double
    For i As Integer = 0 To Tws.Length - 1
      Ris(i) = Interpola(Tab, Tws(i))
    Next
    Return Ris
  End Function

End Class
