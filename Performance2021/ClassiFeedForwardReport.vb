''' <summary>Da dove arrivano i campioni del report feed forward.</summary>
Public Enum eFfSorgente
  VisibleRange = 0
  AllStraightLines = 1
  AllStraightLinesVmg = 2
  SelectedStraightLines = 3
  AllData = 4
End Enum

''' <summary>
''' Report feed forward: impostazioni comuni (variabile X da ottimizzare, vento, filtro, fasce, bin, modello) e una lista di canali Y da analizzare.
''' Ogni canale Y ha la sua serie di grafici, uno per fascia di vento. Il filtro e' salvato anche come copia (FiltroSnapshot), come nei report XY:
''' modificare o cancellare il filtro nella libreria del profilo non cambia un report gia' salvato.
''' </summary>
Public Class clsFeedForwardReport
  Public Property Nome As String = ""
  Public Property Sorgente As eFfSorgente = eFfSorgente.AllStraightLinesVmg
  Public Property XChannelId As String = ""
  Public Property TwsChannelId As String = ""
  ''' <summary>Nome del filtro (libreria del profilo) che sceglie i campioni validi. Vuoto = nessun filtro.</summary>
  Public Property NamedFilter As String = ""
  ''' <summary>Copia del filtro presa quando il report viene salvato. Nothing = si usa quello della libreria.</summary>
  Public Property FiltroSnapshot As clsFiltroNominato = Nothing
  Public Property Parametri As New clsFfParametri
  ''' <summary>Canali Y da analizzare (ChannelId), nell'ordine di stampa.</summary>
  Public Property YChannelIds As New List(Of String)
  ''' <summary>Dimensione in pixel dei pallini della nuvola di campioni.</summary>
  Public Property PuntiDimensione As Integer = 4
  ''' <summary>Opacita' dei pallini della nuvola, da 5 (quasi trasparenti) a 100 (pieni).</summary>
  Public Property PuntiOpacita As Integer = 45
  ''' <summary>Dimensione in pixel dei pallini delle medie dei bin.</summary>
  Public Property MedieDimensione As Integer = 11
  ''' <summary>Grafici per pagina nel PDF.</summary>
  Public Property RighePagina As Integer = 2
  Public Property ColonnePagina As Integer = 2
  ''' <summary>PDF: True = un solo grafico per canale Y con le fasce selezionate insieme; False = un grafico per ogni fascia.</summary>
  Public Property PdfInsieme As Boolean = False
  ''' <summary>Mostra nel grafico (app e PDF) la nuvola e le medie dei bin radi, che non entrano nel calcolo.</summary>
  Public Property MostraBinRadi As Boolean = True
  ''' <summary>Evidenziazione del feed forward: guadagno potenziale (punti di X) da cui una cella e' "da intervenire" o "da considerare".</summary>
  Public Property GuadagnoForte As Double = 1
  Public Property GuadagnoDebole As Double = 0.5
  ''' <summary>Evidenziazione della sensitivita': quota spiegata (%) da cui un canale "conta" o "conta molto".</summary>
  Public Property QuotaMediaPerc As Double = 5
  Public Property QuotaFortePerc As Double = 15
  ''' <summary>Dimensione dei nomi degli assi disegnati sullo sfondo del grafico.</summary>
  Public Property NomiAssiDimensione As Integer = 40
  ''' <summary>Opacita' (5-100) dei nomi degli assi.</summary>
  Public Property NomiAssiOpacita As Integer = 27
  ''' <summary>Dimensione dei numeri sugli assi.</summary>
  Public Property ValoriAssiDimensione As Integer = 14
  ''' <summary>Decimali degli assi = decimali del canale + questo valore.</summary>
  Public Property DecimaliAssiExtra As Integer = 0
  ''' <summary>X di riferimento (per esempio Vmg% 100): nel riepilogo, accanto al valore suggerito, la media di Y osservata a questa X.</summary>
  Public Property XRiferimento As Double = 100
  ''' <summary>Dimensione del titolo dentro il grafico.</summary>
  Public Property TitoloDimensione As Integer = 11

  Public Function Clona() As clsFeedForwardReport
    Dim C As clsFeedForwardReport = DirectCast(Me.MemberwiseClone(), clsFeedForwardReport)
    C.Parametri = Parametri.Clona()
    C.YChannelIds = New List(Of String)(YChannelIds)
    C.FiltroSnapshot = If(FiltroSnapshot Is Nothing, Nothing, FiltroSnapshot.Clona())
    Return C
  End Function

  Public Overrides Function ToString() As String
    Return Nome
  End Function
End Class
