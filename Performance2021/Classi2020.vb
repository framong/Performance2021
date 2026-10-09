Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Composition

'Imports System.Data.SQLite
Imports System.IO
Imports System.Net.Security
Imports System.Runtime.Remoting.Channels
Imports System.Windows.Interop
Imports BruTile.Wmts.Generated

'Imports alglib
Imports Newtonsoft.Json
Imports Parquet
Imports PdfSharp.Pdf.Content.Objects
Imports PdfSharp.Pdf.IO
Imports PropertyChanged
Imports SciChart.Charting.Visuals
Imports SciChart.Charting2D.Interop
Imports SciChart.Core.Extensions
Imports SPwpf
Imports Thrift.Protocol
'Imports SQLitePCL

'Public Class clsPeriodsTrigger
'  Implements INotifyPropertyChanged
'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Dim pListaInAggiornamento As Boolean = True

'  Public Property ListaInAggiornamento As Boolean
'    Get
'      Return pListaInAggiornamento
'    End Get
'    Set(value As Boolean)
'      pListaInAggiornamento = value
'      'OnPropertyChanged("ListaInAggiornamento")
'    End Set
'  End Property

'  Public Sub AggiornaGrafici()
'    OnPropertyChanged("PeriodiAggiornati")
'  End Sub

'  Public ReadOnly Property PeriodiAggiornati As Boolean
'    Get
'      Return True
'    End Get
'  End Property

'End Class

<AddINotifyPropertyChangedInterface>
Public Class clsVmgPercentage
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Dim pUpwindVmgAvg As New List(Of Double)
  Dim pDownwindVmgAvg As New List(Of Double)
  Dim pUpwindTwsAvg As New List(Of Double)
  Dim pDownwindTwsAvg As New List(Of Double)
  Dim ValoriUp As New clsValoriAggregati()
  Dim ValoriDn As New clsValoriAggregati()
  Dim _NormaVmgPerc As Double


  Public ReadOnly Property Valore(IsUpwind As Boolean, Tws As Double) As Double
    Get
      If IsUpwind Then
        If ValoriUp.Dizionario.Count = 0 Then Return 100
        If ValoriUp.Dizionario.ContainsKey(CInt(Tws)) Then
          Return ValoriUp.Dizionario(CInt(Tws)).ValoriY.Average
        Else
          If CInt(Tws) > ValoriUp.Dizionario.Keys.Max Then
            Return ValoriUp.Dizionario(ValoriUp.Dizionario.Keys.Max).ValoriY.Average
          ElseIf CInt(Tws) < ValoriUp.Dizionario.Keys.Min Then
            Return ValoriUp.Dizionario(ValoriUp.Dizionario.Keys.Min).ValoriY.Average
          Else
            ' passa da qui se viene richiesto un coefficiente per un vento che non si é visto negli starightlines ma solo nella manovra
            Dim a = ValoriUp.Dizionario.Where(Function(x) x.Key < CInt(Tws)).OrderByDescending(Function(x) x.Key).FirstOrDefault
            Dim b = ValoriUp.Dizionario.Where(Function(x) x.Key > CInt(Tws)).OrderBy(Function(x) x.Key).FirstOrDefault
            '(tws-a.k)/(b.k-a.k)=(x-a.v)/(b.v-a.v)
            '(x-a.v) = (tws-a.k)/(b.k-a.k)*(b.v-a.v)
            'x=a.v+((tws-a.k)/(b.k-a.k)*(b.v-a.v))
            Return a.Value.ValoriY.Average + ((Tws - a.Key) / (b.Key - a.Key) * (b.Value.ValoriY.Average - a.Value.ValoriY.Average))
          End If
        End If
      Else
        If ValoriDn.Dizionario.Count = 0 Then Return 100
        If ValoriDn.Dizionario.ContainsKey(CInt(Tws)) Then
          Return ValoriDn.Dizionario(CInt(Tws)).ValoriY.Average
        Else
          If CInt(Tws) > ValoriDn.Dizionario.Keys.Max Then
            Return ValoriDn.Dizionario(ValoriDn.Dizionario.Keys.Max).ValoriY.Average
          ElseIf CInt(Tws) < ValoriDn.Dizionario.Keys.Min Then
            Return ValoriDn.Dizionario(ValoriDn.Dizionario.Keys.Min).ValoriY.Average
          Else
            ' passa da qui se viene richiesto un coefficiente per un vento che non si é visto negli starightlines ma solo nella manovra
            Dim a = ValoriDn.Dizionario.Where(Function(x) x.Key < CInt(Tws)).OrderByDescending(Function(x) x.Key).FirstOrDefault
            Dim b = ValoriDn.Dizionario.Where(Function(x) x.Key > CInt(Tws)).OrderBy(Function(x) x.Key).FirstOrDefault
            Return a.Value.ValoriY.Average + ((Tws - a.Key) / (b.Key - a.Key) * (b.Value.ValoriY.Average - a.Value.ValoriY.Average))
          End If
        End If
      End If
    End Get
  End Property

  Public ReadOnly Property ValoreMainChannelEquivalentTargetUp() As Double
    Get
      If TgtManager.Tgt Is Nothing Then Return Double.NaN
      Return TgtManager.Tgt.ValoreMainChannelEquivalentTargetUp(UpwindTwsAvg, UpwindVmgAvg, "bs")
      'Return Targets.GetPolare("bs").ValoreMainChannelEquivalentTargetUp(UpwindTwsAvg, UpwindVmgAvg)
    End Get
  End Property

  Public ReadOnly Property ValoreMainChannelEquivalentTargetDn() As Double
    Get
      If TgtManager.Tgt Is Nothing Then Return Double.NaN
      Return TgtManager.Tgt.ValoreMainChannelEquivalentTargetDn(DownwindTwsAvg, DownwindVmgAvg, "bs")
      ''Return Targets.GetPolare("bs").ValoreMainChannelEquivalentTargetDn(DownwindTwsAvg, DownwindVmgAvg)
    End Get
  End Property

  Public ReadOnly Property ValoreMainChannelEquivalentTarget(IsUpwind As Boolean) As Double
    Get
      If IsUpwind Then
        Return ValoreMainChannelEquivalentTargetUp
      Else
        Return ValoreMainChannelEquivalentTargetDn
      End If
      ''Return Targets.GetPolare("bs").ValoreMainChannelEquivalentTargetDn(DownwindTwsAvg, DownwindVmgAvg)
    End Get
  End Property

  Public ReadOnly Property UpwindVmgAvg As Double
    Get
      If pUpwindTwsAvg.Count = 0 Then Return 0
      Return pUpwindVmgAvg.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property DownwindVmgAvg As Double
    Get
      If pDownwindTwsAvg.Count = 0 Then Return 0
      Return pDownwindVmgAvg.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property UpwindTwsAvg As Double
    Get
      If pUpwindTwsAvg.Count = 0 Then Return 0
      Return pUpwindTwsAvg.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public ReadOnly Property DownwindTwsAvg As Double
    Get
      If pDownwindTwsAvg.Count = 0 Then Return 0
      Return pDownwindTwsAvg.Where(Function(x) Not Double.IsNaN(x)).Average
    End Get
  End Property

  Public Property Descrizione As String

  ''' <summary>Tws equivalente formattato; "n/a" se non calcolabile (polare assente, valore fuori tabella...).</summary>
  Private Shared Function FormatoTwsEquivalente(Valore As Double) As String
    If Double.IsNaN(Valore) OrElse Double.IsInfinity(Valore) Then Return "n/a"
    Return Valore.ToString("F1")
  End Function

  ''' <summary>
  ''' Riscrive la descrizione mostrata sul pulsante: "Up: 95%(12.3k) @12.1k, Dn: ...", cioe' Vmg% medio (Tws equivalente
  ''' al quale il target darebbe quella percentuale) alla Tws media. Non lancia mai eccezioni: un errore nel calcolo del
  ''' Tws equivalente lasciava la descrizione vuota.
  ''' </summary>
  Public Sub AggiornaDescrizione()
    Dim strTmp As String = "Up: "
    Try
      If pUpwindTwsAvg.Count > 0 Then
        strTmp &= Format(UpwindVmgAvg, "F0")
        strTmp &= "%("
        strTmp &= FormatoTwsEquivalente(ValoreMainChannelEquivalentTargetUp)
        strTmp &= "k) @"
        strTmp &= Format(UpwindTwsAvg, "F1")
        strTmp &= "k"
      Else
        strTmp &= "no data"
      End If
    Catch ex As Exception
      strTmp &= "n/a"
    End Try
    strTmp &= ", Dn: "
    Try
      If pDownwindTwsAvg.Count > 0 Then
        strTmp &= Format(DownwindVmgAvg, "F0")
        strTmp &= "%("
        strTmp &= FormatoTwsEquivalente(ValoreMainChannelEquivalentTargetDn)
        strTmp &= "k) @"
        strTmp &= Format(DownwindTwsAvg, "F1")
        strTmp &= "k"
      Else
        strTmp &= "no data"
      End If
    Catch ex As Exception
      strTmp &= "n/a"
    End Try
    Descrizione = strTmp
  End Sub

  Public Property NormaVmgPerc As Double
    Get
      Return _NormaVmgPerc
    End Get
    Set(value As Double)
      _NormaVmgPerc = value
    End Set
  End Property

  ''' <param name="AggiornaDescr">False quando si aggiungono molte coppie di seguito: la descrizione (che ricalcola medie e Tws
  ''' equivalente su tutti i dati) va poi aggiornata una sola volta con AggiornaDescrizione.</param>
  Public Sub AggiungiCoppia(ValoreX As Double, ValoreY As Double, IsUpwind As Boolean, Optional AggiornaDescr As Boolean = True)
    If IsUpwind Then
      ValoriUp.AggiungiCoppia(CInt(ValoreX), ValoreY)
      pUpwindTwsAvg.Add(ValoreX)
      pUpwindVmgAvg.Add(ValoreY)
    Else
      ValoriDn.AggiungiCoppia(CInt(ValoreX), ValoreY)
      pDownwindTwsAvg.Add(ValoreX)
      pDownwindVmgAvg.Add(ValoreY)
    End If
    If AggiornaDescr Then AggiornaDescrizione()
  End Sub

  Public Sub AzzeraValori()
    pUpwindVmgAvg.Clear()
    pDownwindVmgAvg.Clear()
    pUpwindTwsAvg.Clear()
    pDownwindTwsAvg.Clear()
    ValoriUp = New clsValoriAggregati()
    ValoriDn = New clsValoriAggregati()
    AggiornaDescrizione()
  End Sub

  'Public Sub AggiornaDescrizione()
  '  'Dim TwsEquiv As Double = ValoreMainChannelEquivalentTargetUp
  '  OnPropertyChanged("Descrizione")
  'End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPeriodsJson
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property ListaPeriodi As New ObservableCollection(Of clsPeriod2021) ' il primo è il test che contiene i commenti generici

  'Public Property ListaPeriodi As ObservableCollection(Of clsPeriod2021)
  '  Get
  '    Return _ListaPeriodi
  '  End Get
  '  Set(value As ObservableCollection(Of clsPeriod2021))
  '    _ListaPeriodi = value
  '    OnPropertyChanged("ListaPeriodi")
  '  End Set
  'End Property

  Public Sub New()

  End Sub

End Class

Public Class clsValoriNotNanPortStbd
  Dim _Canale As clsChannel2020
  Dim _ValoriNotNan As New List(Of Double)
  Dim _ValoriNotNanPort As New List(Of Double)
  Dim _ValoriNotNanStbd As New List(Of Double)

  Public Sub New(Canale As clsChannel2020)
    _Canale = Canale
  End Sub

  Public Property ValoriNotNan As List(Of Double)
    Get
      Return _ValoriNotNan
    End Get
    Set(value As List(Of Double))
      _ValoriNotNan = value
    End Set
  End Property

  Public Property ValoriNotNanPort As List(Of Double)
    Get
      Return _ValoriNotNanPort
    End Get
    Set(value As List(Of Double))
      _ValoriNotNanPort = value
    End Set
  End Property

  Public Property ValoriNotNanStbd As List(Of Double)
    Get
      Return _ValoriNotNanStbd
    End Get
    Set(value As List(Of Double))
      _ValoriNotNanStbd = value
    End Set
  End Property

  Public Property Canale As clsChannel2020
    Get
      Return _Canale
    End Get
    Set(value As clsChannel2020)
      _Canale = value
    End Set
  End Property

End Class

'Public Class clsPavarotAdvanced
'  Dim _ListaPavarots As List(Of clsPeriod2020)

'  Public Enum eTipoDato
'    eEnrtyExitLossSecs
'    eEnrtyExitLossMeters
'    eTws

'  End Enum

'  Public Sub New(ListaPavarots As List(Of clsPeriod2020))
'    _ListaPavarots = ListaPavarots
'  End Sub

'  Public Property ListaPavarots As List(Of clsPeriod2020)
'    Get
'      Return _ListaPavarots
'    End Get
'    Set(value As List(Of clsPeriod2020))
'      _ListaPavarots = value
'    End Set
'  End Property


'  'Public ReadOnly Property Lista(Tacks As Boolean, KindOf As clsPavarotPhases.eKindOfPavarot) As List(Of clsPeriod2020)
'  '  Get
'  '    If Tacks Then
'  '      Return ListaPavarots.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eTack).Where(Function(x) x.DettagliPavarot.PavarotData.PavarotPhases.KindOfPavarot = KindOf).ToList
'  '    Else
'  '      Return ListaPavarots.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eGybe).Where(Function(x) x.DettagliPavarot.PavarotData.PavarotPhases.KindOfPavarot = KindOf).ToList
'  '    End If
'  '  End Get
'  'End Property

'  Public ReadOnly Property Lista(Tacks As Boolean) As List(Of clsPeriod2020)
'    Get
'      If Tacks Then
'        Return ListaPavarots.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eTack).ToList
'      Else
'        Return ListaPavarots.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eGybe).ToList
'      End If
'    End Get
'  End Property


'  Public ReadOnly Property StringaValore(Lista As List(Of clsPeriod2020), TipoDato As eTipoDato, DailyVmgCoeff As Double, Decimali As Integer) As String
'    Get
'      Return ""
'      'Dim v As Double = Valore(Lista, TipoDato, DailyVmgCoeff)
'      'If Double.IsNaN(v) Then
'      '  Return ""
'      'Else
'      '  Return v.ToString("F" & Decimali.ToString)
'      'End If
'    End Get
'  End Property

'  'Public ReadOnly Property Valore(Lista As List(Of clsPeriod2020), TipoDato As eTipoDato, DailyVmgCoeff As Double) As Double
'  '  Get
'  '    Dim v As New List(Of Double)
'  '    For Each p In Lista
'  '      Dim Tws As Double = p.DettagliPavarot.MedieStandardPavarot.TWSMedia
'  '      Select Case TipoDato
'  '        Case eTipoDato.eEnrtyExitLossSecs, eTipoDato.eEnrtyExitLossMeters
'  '          Dim Vmg As Double = p.DettagliPavarot.VmgGainLossTotalAvgKts(p.DettagliPavarot.TimeRangeEntry.IdRigaFinale, p.DettagliPavarot.TimeRangeExit.IdRigaIniziale)
'  '          Dim VmgT As Double = 0
'  '          If TgtManager.Tgt Is Nothing Then
'  '            VmgT = p.DettagliPavarot.PavarotData.PavarotPhases.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Vmg).Avg
'  '          Else
'  '            Dim vb = TgtManager.Tgt.ValoreTgt(p.DettagliPavarot.IsTack, Tws, "bs")
'  '            VmgT = vb.Vmg
'  '            'If ApplyDailyVmgCoeff Then DailyVmgCoeff = PeriodsManager.VmgPercentage.Valore(p.DettagliPavarot.IsTack, Tws) / 100
'  '            VmgT *= DailyVmgCoeff
'  '          End If
'  '          Dim EntryToExitVmgLossInMetri As Double = KtsToMS(Vmg - VmgT) * p.DettagliPavarot.TimeRangeExit.Start.Subtract(p.DettagliPavarot.TimeRangeEntry.Finish).TotalSeconds
'  '          If TipoDato = eTipoDato.eEnrtyExitLossSecs Then
'  '            v.Add(EntryToExitVmgLossInMetri / VmgT)
'  '          Else
'  '            v.Add(EntryToExitVmgLossInMetri)
'  '          End If
'  '        Case eTipoDato.eTws
'  '          v.Add(p.ValoriCanaleTWS.Avg)
'  '      End Select
'  '    Next
'  '    If v.Count = 0 Then Return Double.NaN
'  '    Return v.Average
'  '  End Get
'  'End Property

'End Class

Public Class clsFaseLeg
  Dim _IdRiga As Integer
  Dim _Type As eType
  Dim _IsStbd As Boolean
  Dim _IsStbdEntry As Boolean
  Dim _IsInvestment As Boolean

  Public Enum eType
    eAllTogether = -1
    eStraightLine = 0
    eManoeuver = 1
    eInvestment = 2
  End Enum

  Public Property IdRiga As Integer
    Get
      Return _IdRiga
    End Get
    Set(value As Integer)
      _IdRiga = value
    End Set
  End Property

  Public Property IsStbd As Boolean
    Get
      Return _IsStbd
    End Get
    Set(value As Boolean)
      _IsStbd = value
    End Set
  End Property

  Public Property IsStbdEntry As Boolean
    Get
      Return _IsStbdEntry
    End Get
    Set(value As Boolean)
      _IsStbdEntry = value
    End Set
  End Property

  Public Property Type As eType
    Get
      Return _Type
    End Get
    Set(value As eType)
      _Type = value
    End Set
  End Property
End Class

Public Class clsLeg
  Public Property TR As clsTimeRange
  Public Property Manovre As List(Of clsPeriod2021)
  Public Property StraightLines As List(Of clsPeriod2021)
  Public Property SecInvestimento As Integer = 25
  Public Property SecAnte As Integer = 7
  Public Property SecPost As Integer = 25
  Public Property SecAntePostRounding As Integer = 20
  Public Property MomentiLeg As New List(Of clsFaseLeg)
  Public Property EscludiEstremi As Boolean = True
  Public Property IsUpwind As Boolean

  'Public Sub New(TR As clsTimeRange, Manovre As List(Of clsPeriod2020))
  '  _TR = TR
  '  _Manovre = Manovre.Where(Function(x) x.KeyMoment >= TR.Start AndAlso x.KeyMoment <= TR.Finish).ToList
  '  If _Manovre.Count > 0 Then
  '    If _EscludiEstremi Then
  '      If TR.Finish.Subtract(_Manovre.Last.KeyMoment).TotalSeconds < 30 Then
  '        _TR.Finish = _Manovre.Last.KeyMoment.AddSeconds(_SecPost)
  '      Else
  '        _TR.Finish = TR.Finish.AddSeconds(-30)
  '      End If
  '    End If
  '  End If
  '  ImpostaRighe()
  'End Sub

  Public Sub New(TR As clsTimeRange, Manovre As List(Of clsPeriod2021), StraightLine As List(Of clsPeriod2021))
    _TR = TR
    _Manovre = Manovre.Where(Function(x) x.KeyMoment >= TR.Start AndAlso x.KeyMoment <= TR.Finish).ToList
    _StraightLines = StraightLine.Where(Function(x) x.TR.Start >= TR.Start AndAlso x.TR.Finish <= TR.Finish).ToList
    If _Manovre.Count > 0 Then
      If _EscludiEstremi Then
        ' vanno esclusi tutti i periodi SL che iniziano a meno di 30 secondi dall inizio del lato
        _StraightLines = _StraightLines.Where(Function(x) x.TR.Start >= TR.Start.AddSeconds(30)).ToList
        ' vanno esclusi tutti i periodi SL che finiscono a meno di 30 secondi dalla fine del lato
        _StraightLines = _StraightLines.Where(Function(x) x.TR.Finish <= TR.Finish.AddSeconds(-30)).ToList
      End If
    End If
    ImpostaRigheDaPeriodi()
  End Sub

  'Public Property TR As clsTimeRange
  '  Get
  '    Return _TR
  '  End Get
  '  Set(value As clsTimeRange)
  '    _TR = value
  '  End Set
  'End Property

  'Public Property MomentiLeg As List(Of clsFaseLeg)
  '  Get
  '    Return _MomentiLeg
  '  End Get
  '  Set(value As List(Of clsFaseLeg))
  '    _MomentiLeg = value
  '  End Set
  'End Property

  'Public Property Manovre As List(Of clsPeriod2021)
  '  Get
  '    Return _Manovre
  '  End Get
  '  Set(value As List(Of clsPeriod2021))
  '    _Manovre = value
  '  End Set
  'End Property

  'Public Property EscludiEstremi As Boolean
  '  Get
  '    Return _EscludiEstremi
  '  End Get
  '  Set(value As Boolean)
  '    _EscludiEstremi = value
  '  End Set
  'End Property

  'Public Property SecAnte As Integer
  '  Get
  '    Return _SecAnte
  '  End Get
  '  Set(value As Integer)
  '    _SecAnte = value
  '  End Set
  'End Property

  'Public Property IsUpwind As Boolean
  '  Get
  '    Return _IsUpwind
  '  End Get
  '  Set(value As Boolean)
  '    _IsUpwind = value
  '  End Set
  'End Property

  'Public Property SecPost As Integer
  '  Get
  '    Return _SecPost
  '  End Get
  '  Set(value As Integer)
  '    _SecPost = value
  '  End Set
  'End Property

  'Public Property SecInvestimento As Integer
  '  Get
  '    Return _SecInvestimento
  '  End Get
  '  Set(value As Integer)
  '    _SecInvestimento = value
  '  End Set
  'End Property

  Private Sub ImpostaRigheDaPeriodi()
    Dim cTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim VnotNan As Double() = DataProvider2020.ValoriIntervallo(cTwa, TR, Nothing, Nothing, True, True)
    _IsUpwind = VnotNan.Average <= 90
    For Each sl In _StraightLines
      For i As Integer = sl.TR.IdRigaIniziale To sl.TR.IdRigaFinale
        Dim m As New clsFaseLeg
        m.IdRiga = i
        m.IsStbd = sl.IsStbd
        m.Type = clsFaseLeg.eType.eStraightLine
        _MomentiLeg.Add(m)
      Next
    Next
    For Each manovra In _Manovre
      Dim InizioInvestimento As Integer = DataProvider2020.TrovaIndice(manovra.KeyMoment.AddSeconds(-_SecInvestimento))
      Dim Inizio As Integer = DataProvider2020.TrovaIndice(manovra.KeyMoment.AddSeconds(-_SecAnte))
      Dim Fine As Integer = DataProvider2020.TrovaIndice(manovra.KeyMoment.AddSeconds(_SecPost))
      For i As Integer = InizioInvestimento To Fine
        Dim m As New clsFaseLeg
        m.IdRiga = i
        m.IsStbd = manovra.IsStbd
        If i >= Inizio Then
          m.Type = clsFaseLeg.eType.eManoeuver
        ElseIf i >= InizioInvestimento Then
          m.Type = clsFaseLeg.eType.eInvestment
        End If
        m.IsStbdEntry = manovra.IsStbd
        _MomentiLeg.Add(m)
      Next
    Next
  End Sub

  Public Function ValoriTotaliStbd(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eStraightLine AndAlso x.IsStbd).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next

    Indici = _MomentiLeg.Where(Function(x) Not x.Type = clsFaseLeg.eType.eStraightLine AndAlso x.IsStbdEntry).ToList
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriTotaliPort(Canale As clsChannel2020) As List(Of Double)
    'Dim Indici = _MomentiLeg.Where(Function(x) Not x.IsStbd).ToList
    'Dim Valori As New List(Of Double)
    'For Each indice In Indici
    '  Valori.Add(Canale.Valori(indice.IdRiga))
    'Next
    ''Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    'Return Valori
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eStraightLine AndAlso Not x.IsStbd).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next

    Indici = _MomentiLeg.Where(Function(x) Not x.Type = clsFaseLeg.eType.eStraightLine AndAlso Not x.IsStbdEntry).ToList
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriStraightLinePort(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eStraightLine AndAlso Not x.IsStbd).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriStraightLineStbd(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eStraightLine AndAlso x.IsStbd).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next

    'Dim v As New List(Of Integer)
    'Dim vs As New List(Of Integer)
    'Dim vp As New List(Of Integer)
    'For Each momento In _MomentiLeg
    '  If momento.Type = clsMomentoLeg.eType.eStraightLine Then
    '    v.Add(momento.IdRiga)
    '    If momento.IsStbd Then
    '      vs.Add(momento.IdRiga)
    '    Else
    '      vp.Add(momento.IdRiga)
    '    End If
    '  End If
    'Next

    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori



  End Function




  Public Function ValoriManoeuversPortEntry(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eManoeuver AndAlso Not x.IsStbdEntry).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriManoeuversStbdEntry(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eManoeuver AndAlso x.IsStbdEntry).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriInvestmentsPortEntry(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eInvestment AndAlso Not x.IsStbdEntry).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function

  Public Function ValoriInvestmentsStbdEntry(Canale As clsChannel2020) As List(Of Double)
    Dim Indici = _MomentiLeg.Where(Function(x) x.Type = clsFaseLeg.eType.eInvestment AndAlso x.IsStbdEntry).ToList
    Dim Valori As New List(Of Double)
    For Each indice In Indici
      Valori.Add(Canale.Valori(indice.IdRiga))
    Next
    'Return Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
    Return Valori
  End Function


End Class

Public Class clsLegs
  Public Property Legs As New List(Of clsLeg)
  Public Property TR As clsTimeRange
  Public Property Manoeuvers As List(Of clsPeriod2021)
  Public Property MarkRoundings As List(Of DateTime)
  Public Property StraightLineChannels As List(Of clsChannel2020)
  Public Property DescriptionChannels As List(Of clsChannel2020)

  Public Enum eLossUM
    eNone = 0
    eMinute = 1
    e30mins = 2
  End Enum


  Public Sub New(TR As clsTimeRange, Manoeuvers As List(Of clsPeriod2021), MarkRoundings As List(Of DateTime), StraightLines As List(Of clsPeriod2021), StraightLineChannels As List(Of clsChannel2020), DescriptionChannels As List(Of clsChannel2020))
    Me.TR = TR
    Me.Manoeuvers = Manoeuvers
    Me.MarkRoundings = MarkRoundings
    Me.StraightLineChannels = StraightLineChannels
    Me.DescriptionChannels = DescriptionChannels
    Me.Legs.Clear()

    Dim i As Integer = 0
    For Each MR In MarkRoundings
      If MR = MarkRoundings.First Then
        _Legs.Add(New clsLeg(New clsTimeRange(TR.Start, MR), Manoeuvers, StraightLines))
      End If

      If MR = MarkRoundings.Last Then
        _Legs.Add(New clsLeg(New clsTimeRange(MR, TR.Finish), Manoeuvers, StraightLines))
        Exit For
      End If

      _Legs.Add(New clsLeg(New clsTimeRange(MR, MarkRoundings(i + 1)), Manoeuvers, StraightLines))
      i += 1
    Next
  End Sub

  'Public Property Legs As List(Of clsLeg)
  '  Get
  '    Return _Legs
  '  End Get
  '  Set(value As List(Of clsLeg))
  '    _Legs = value
  '  End Set
  'End Property

  'Public Property TR As clsTimeRange
  '  Get
  '    Return _TR
  '  End Get
  '  Set(value As clsTimeRange)
  '    _TR = value
  '  End Set
  'End Property

  Private Function StringaLoss(LossUM As eLossUM) As String
    Select Case LossUM
      Case eLossUM.e30mins
        Return "per 30 minutes"
      Case eLossUM.eMinute
        Return "per Minute"
      Case Else
        Return ""
    End Select
  End Function

  Private Function Media(Valori As List(Of Double)) As Double
    If Valori.Count > 0 Then Return Valori.Where(Function(x) Not Double.IsNaN(x)).Average
    Return 0
  End Function


  Private Function Configurazione(TR As clsTimeRange) As String
    Dim txt As String = ""
    If _DescriptionChannels.Count = 0 Then Return ""
    For Each c In _DescriptionChannels
      Dim vs As New List(Of Double)
      For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
        Dim v As Double = c.Valori(i)
        If Not Double.IsNaN(v) Then
          vs.Add(v)
        End If
      Next
      txt &= " " & c.ShortName & ": " & vs.Average.ToString("F2") & ","
    Next
    Return txt.TrimEnd(",") & ""
  End Function

End Class

Public Class clsLegData
  Dim _Leg As clsLeg
  Dim _IsStbd As Boolean
  Dim _Valori As New Dictionary(Of clsChannel2020, List(Of Double))
  'Dim _ValoriPort As New Dictionary(Of clsChannel2020, List(Of Double))
  'Dim _ValoriStbd As New Dictionary(Of clsChannel2020, List(Of Double))
  Dim _Type As clsFaseLeg.eType
  Dim _Channels As List(Of clsChannel2020)

  Dim _Seconds As Double
  'Dim _SecondsPort As Double
  'Dim _SecondsStbd As Double

  'Public Sub New(Leg As clsLeg, Channels As List(Of clsChannel2020), Type As clsFaseLeg.eType)
  Public Sub New(Leg As clsLeg, IsStbd As Boolean, Channels As List(Of clsChannel2020), Type As clsFaseLeg.eType)
    _Leg = Leg
    _IsStbd = IsStbd
    _Channels = Channels
    _Type = Type
    RiempiValori()
  End Sub

  Public ReadOnly Property Valori As Dictionary(Of clsChannel2020, List(Of Double))
    Get
      Return _Valori
    End Get
  End Property

  'Public ReadOnly Property ValoriPort As Dictionary(Of clsChannel2020, List(Of Double))
  '  Get
  '    Return _ValoriPort
  '  End Get
  'End Property

  'Public ReadOnly Property ValoriStbd As Dictionary(Of clsChannel2020, List(Of Double))
  '  Get
  '    Return _ValoriStbd
  '  End Get
  'End Property

  Public Property Type As clsFaseLeg.eType
    Get
      Return _Type
    End Get
    Set(value As clsFaseLeg.eType)
      _Type = value
    End Set
  End Property

  Public Property Channels As List(Of clsChannel2020)
    Get
      Return _Channels
    End Get
    Set(value As List(Of clsChannel2020))
      _Channels = value
    End Set
  End Property

  Public ReadOnly Property IsStbd As Boolean
    Get
      Return _IsStbd
    End Get
  End Property

  Public ReadOnly Property Seconds As Double
    Get
      Return _Seconds
    End Get
  End Property

  'Public ReadOnly Property SecondsStbd As Double
  '  Get
  '    Return _SecondsStbd
  '  End Get
  'End Property


  'Public ReadOnly Property SecondsPort As Double
  '  Get
  '    Return _SecondsPort
  '  End Get
  'End Property
  Public Sub AzzeraValori()
    _Valori.Clear()
    _Seconds = 0
    For Each canale In _Channels
      _Valori.Add(canale, New List(Of Double))
    Next

  End Sub

  Private Sub RiempiValori()
    Dim _v As List(Of Double)
    _Valori.Clear()

    For Each canale In _Channels
      Select Case _Type
        Case clsFaseLeg.eType.eInvestment
          If IsStbd Then
            _v = _Leg.ValoriInvestmentsStbdEntry(canale)
          Else
            _v = _Leg.ValoriInvestmentsPortEntry(canale)
          End If
        Case clsFaseLeg.eType.eManoeuver
          If IsStbd Then
            _v = _Leg.ValoriManoeuversStbdEntry(canale)
          Else
            _v = _Leg.ValoriManoeuversPortEntry(canale)
          End If
        Case clsFaseLeg.eType.eStraightLine
          If IsStbd Then
            _v = _Leg.ValoriStraightLineStbd(canale)
          Else
            _v = _Leg.ValoriStraightLinePort(canale)
          End If
        Case Else ' clsMomentoLeg.eType.eAllTogether
          If IsStbd Then
            _v = _Leg.ValoriTotaliStbd(canale)
          Else
            _v = _Leg.ValoriTotaliPort(canale)
          End If
      End Select
      _Valori.Add(canale, _v.Where(Function(x) Not Double.IsNaN(x)).ToList)
    Next
    _Seconds = _Valori.Values.First.Count / DataProvider2020.Hz

  End Sub

  'Private Sub RiempiValori()
  '  Dim _v, _vs, _vp As List(Of Double)
  '  _Valori.Clear()
  '  _ValoriStbd.Clear()
  '  _ValoriPort.Clear()

  '  For Each canale In _Channels
  '    Select Case _Type
  '      Case clsFaseLeg.eType.eInvestment
  '        _v = _Leg.ValoriInvestments(canale)
  '        _vs = _Leg.ValoriInvestmentsStbdEntry(canale)
  '        _vp = _Leg.ValoriInvestmentsPortEntry(canale)
  '      Case clsFaseLeg.eType.eManoeuver
  '        _v = _Leg.ValoriManoeuvers(canale)
  '        _vs = _Leg.ValoriManoeuversStbdEntry(canale)
  '        _vp = _Leg.ValoriManoeuversPortEntry(canale)
  '      Case clsFaseLeg.eType.eStraightLine
  '        _v = _Leg.ValoriStraightLine(canale)
  '        _vs = _Leg.ValoriStraightLineStbd(canale)
  '        _vp = _Leg.ValoriStraightLinePort(canale)
  '      Case Else ' clsMomentoLeg.eType.eAllTogether
  '        _v = _Leg.ValoriTotali(canale)
  '        _vs = _Leg.ValoriTotaliStbd(canale)
  '        _vp = _Leg.ValoriTotaliPort(canale)
  '    End Select
  '    _Valori.Add(canale, _v.Where(Function(x) Not Double.IsNaN(x)).ToList)
  '    _ValoriPort.Add(canale, _vp.Where(Function(x) Not Double.IsNaN(x)).ToList)
  '    _ValoriStbd.Add(canale, _vs.Where(Function(x) Not Double.IsNaN(x)).ToList)
  '  Next
  '  '_Seconds = _Valori.Values.First.Count / DataProvider2020.Hz
  '  _SecondsPort = _ValoriPort.Values.First.Count / DataProvider2020.Hz
  '  _SecondsStbd = _ValoriStbd.Values.First.Count / DataProvider2020.Hz
  '  _Seconds = _SecondsStbd + _SecondsPort

  'End Sub

End Class

Public Class clsTratto
  Public Inizio As DateTime
  Public Fine As DateTime
  Public Andatura As SummaryReport.Andatura

  Public Sub New(Start As DateTime, Angle As SummaryReport.Andatura)
    Inizio = Start
    Andatura = Angle
  End Sub

  Public Property Durata As TimeSpan = Fine.Subtract(Inizio)



End Class

Public Class clsSegmento
  'Dim _Seconds As Double
  Dim _IsUp As Boolean
  Dim _IsStbd As Boolean
  Dim _NrManovre As Integer

  Dim _SecondsLossPerManoeuver As Double
  Dim _InlineLossPerManoeuver As Double
  Dim _SecondsLossPerInvestment As Double
  Dim _InvestmentDeltaPercFromStraightLine As Double

  Dim _LegData As clsLegData
  Dim _LegDataStraightLine As clsLegData
  Dim _LegDataInvestment As clsLegData
  Dim _LegDataManoeuver As clsLegData

  Dim _Tws As Double
  Dim _TwsEq As Double

  Dim _NormaVmgPerc As Double

  Public Sub New(IsStbd As Boolean, Lato As clsLeg, Canali As List(Of clsChannel2020))


    _IsUp = Lato.IsUpwind
    _IsStbd = IsStbd


    _LegData = New clsLegData(Lato, _IsStbd, Canali, clsFaseLeg.eType.eAllTogether)
    _LegDataStraightLine = New clsLegData(Lato, _IsStbd, Canali, clsFaseLeg.eType.eStraightLine)
    _LegDataInvestment = New clsLegData(Lato, _IsStbd, Canali, clsFaseLeg.eType.eInvestment)
    _LegDataManoeuver = New clsLegData(Lato, _IsStbd, Canali, clsFaseLeg.eType.eManoeuver)

    '_Seconds = _LegData.Seconds

    'Dim tgt As clsVmgVals = Nothing
    'If Not TgtManager Is Nothing Then
    '  If Not TgtManager.Tgt Is Nothing Then
    '    tgt = TgtManager.Tgt.ValoreTgt(Lato.IsUpwind, Media(ldT.Last.Valori(CTws)), "bs")
    '  End If
    'End If
    '_LegData.CalcolaMetriLoss(CVmg, CVmgP, tgt)
    '_LegDataStraightLine.CalcolaMetriLoss(CVmg, CVmgP, tgt)
    '_LegDataInvestment.CalcolaMetriLoss(CVmg, CVmgP, tgt)
    '_LegDataManoeuver.CalcolaMetriLoss(CVmg, CVmgP, tgt)

  End Sub

  Public Sub AzzeraValori()
    _LegData.AzzeraValori()
    _LegDataStraightLine.AzzeraValori()
    _LegDataInvestment.AzzeraValori()
    _LegDataManoeuver.AzzeraValori()
  End Sub

  'Public ReadOnly Property Seconds As Double
  '  Get
  '    Return _Seconds
  '  End Get
  'End Property

  Public ReadOnly Property IsUp As Boolean
    Get
      Return _IsUp
    End Get
  End Property

  Public ReadOnly Property IsStbd As Boolean
    Get
      Return _IsStbd
    End Get
  End Property

  Public ReadOnly Property LegData As clsLegData
    Get
      Return _LegData
    End Get
  End Property

  Public ReadOnly Property LegDataStraightLine As clsLegData
    Get
      Return _LegDataStraightLine
    End Get
  End Property

  Public ReadOnly Property LegDataInvestment As clsLegData
    Get
      Return _LegDataInvestment
    End Get
  End Property

  Public ReadOnly Property LegDataManoeuver As clsLegData
    Get
      Return _LegDataManoeuver
    End Get
  End Property

  Public Property SecondsLossPerManoeuver As Double
    Get
      Return _SecondsLossPerManoeuver
    End Get
    Set(value As Double)
      _SecondsLossPerManoeuver = value
    End Set
  End Property

  Public Property InlineLossPerManoeuver As Double
    Get
      Return _InlineLossPerManoeuver
    End Get
    Set(value As Double)
      _InlineLossPerManoeuver = value
    End Set
  End Property

  Public Property SecondsLossPerInvestment As Double
    Get
      Return _SecondsLossPerInvestment
    End Get
    Set(value As Double)
      _SecondsLossPerInvestment = value
    End Set
  End Property

  Public Property InvestmentDeltaPercFromStraightLine As Double
    Get
      Return _InvestmentDeltaPercFromStraightLine
    End Get
    Set(value As Double)
      _InvestmentDeltaPercFromStraightLine = value
    End Set
  End Property

  Public Property TwsEq As Double
    Get
      Return _TwsEq
    End Get
    Set(value As Double)
      _TwsEq = value
    End Set
  End Property

  Public Property Tws As Double
    Get
      Return _Tws
    End Get
    Set(value As Double)
      _Tws = value
    End Set
  End Property

  Public Property NrManovre As Integer
    Get
      Return _NrManovre
    End Get
    Set(value As Integer)
      _NrManovre = value
    End Set
  End Property

  Public Property NormaVmgPerc As Double
    Get
      Return _NormaVmgPerc
    End Get
    Set(value As Double)
      _NormaVmgPerc = value
    End Set
  End Property
End Class

Public Class clsDeltas
  Dim _MtTot As Double
  Dim _MtSl As Double
  Dim _MtInv As Double
  Dim _MtMan As Double
  Dim _SecTot As Double
  Dim _SecSl As Double
  Dim _SecInv As Double
  Dim _SecMan As Double
  Dim _Scenario As eScenario
  Dim _StringaScenario As String

  Dim _InvSec As Integer = 25
  Dim _ManPre As Integer = 7
  Dim _ManPost As Integer = 25

  Public Enum eScenario
    eLight = 0
    eMid = 1
    eStrong = 2
  End Enum

  Public Sub New(Scenario As eScenario, IsUpwind As Boolean, GhostVmg As Double, SlVmg As Double, InvVmg As Double, ManVmg As Double, HasStraightLine As Boolean, NrManovre As Integer)
    CalcolaTempi(Scenario, IsUpwind, GhostVmg, SlVmg, InvVmg, ManVmg, HasStraightLine, NrManovre)
  End Sub

  Private Sub CalcolaTempi(Scenario As eScenario, IsUpwind As Boolean, GhostVmg As Double, SlVmg As Double, InvVmg As Double, ManVmg As Double, HasStraightLine As Boolean, NrManovre As Integer)

    _Scenario = Scenario
    _StringaScenario = ScenarioDescription()
    Dim SlDeltaMeters = KtsToMS(SlVmg - GhostVmg) ' metri al secondo persi/guadagnati in straight line rispetto al gost     * SlSeconds  'metri persi tra vmg sl e gost
    Dim InvDeltaMetersGhost As Double = KtsToMS(InvVmg - GhostVmg) ' metri al secondo persi/guadagnati in investment rispetto al gost  * InvTime
    Dim ManDeltaMetersGhost As Double = KtsToMS(ManVmg - GhostVmg) ' metri al secondo persi/guadagnati in manovra rispetto al gost  * ManTime
    Dim InvDeltaMeters As Double = KtsToMS(InvVmg - SlVmg) ' metri al secondo persi/guadagnati in investment rispetto al gost  * InvTime
    Dim ManDeltaMeters As Double = KtsToMS(ManVmg - SlVmg) ' metri al secondo persi/guadagnati in manovra rispetto al gost  * ManTime
    Dim TgtMan As Integer = ScenarioManoeuvers(IsUpwind)
    Dim TgtTime As Integer = ScenarioLegSeconds(IsUpwind)
    Dim TgtInvTime As Integer = TgtMan * (_InvSec - _ManPre)
    Dim TgtManTime As Integer = TgtMan * (_ManPre + _ManPost)
    Dim TgtSlTime As Integer = TgtTime - TgtInvTime - TgtManTime

    If HasStraightLine Then
      If NrManovre = 0 Then
        InvDeltaMeters = 0 ' metri al secondo persi/guadagnati in investment rispetto al gost  * InvTime
        ManDeltaMeters = 0 ' metri al secondo persi/guadagnati in manovra rispetto al gost  * ManTime
        InvDeltaMetersGhost = 0 ' metri al secondo persi/guadagnati in investment rispetto al gost  * InvTime
        ManDeltaMetersGhost = 0 ' metri al secondo persi/guadagnati in manovra rispetto al gost  * ManTime
      End If



      ' delta metri al secondo calcolati dal persorso reale riportati sul percorso scenario
      _MtSl = SlDeltaMeters * TgtSlTime
      _MtInv = InvDeltaMeters * TgtInvTime
      _MtMan = ManDeltaMeters * TgtManTime
      Dim MtInvGhost As Double = InvDeltaMetersGhost * TgtInvTime
      Dim MtManGhost As Double = ManDeltaMetersGhost * TgtManTime

      _MtTot = _MtSl + MtInvGhost + MtManGhost

      ' metri trasformati in secondi alla velocità del ghost
      _SecTot = _MtTot / KtsToMS(GhostVmg)
      _SecSl = _MtSl / KtsToMS(GhostVmg)
      _SecInv = _MtInv / KtsToMS(GhostVmg)
      _SecMan = _MtMan / KtsToMS(GhostVmg)
    Else
      'Stop
      ' andrebbe fatto sull investment ma se l investment non è buono i dati fanno schifo
      ' provo mettendo sl al 100% e calcolo il resto
      If NrManovre = 0 Then
        _MtSl = 0
        _MtInv = 0
        _MtMan = 0
        _MtTot = 0
        _SecTot = 0
        _SecSl = 0
        _SecInv = 0
        _SecMan = 0
      Else
        SlVmg = GhostVmg 'se non c'e' straight line si fa rispetto al target
        InvDeltaMeters = KtsToMS(InvVmg - SlVmg) ' metri al secondo persi/guadagnati in investment rispetto al gost  * InvTime
        ManDeltaMeters = KtsToMS(ManVmg - SlVmg) ' metri al secondo persi/guadagnati in manovra rispetto al gost  * ManTime

        _MtSl = 0 * TgtSlTime
        _MtInv = InvDeltaMeters * TgtInvTime
        _MtMan = ManDeltaMeters * TgtManTime
        Dim MtInvGhost As Double = InvDeltaMetersGhost * TgtInvTime
        Dim MtManGhost As Double = ManDeltaMetersGhost * TgtManTime
        _MtTot = _MtSl + MtInvGhost + MtManGhost

        _SecTot = _MtTot / KtsToMS(GhostVmg)
        _SecSl = _MtSl / KtsToMS(GhostVmg)
        _SecInv = _MtInv / KtsToMS(GhostVmg)
        _SecMan = _MtMan / KtsToMS(GhostVmg)
      End If
    End If


  End Sub

  Private Function ScenarioManoeuvers(IsUpwind As Boolean) As Integer
    If IsUpwind Then
      Select Case _Scenario
        Case eScenario.eLight
          Return 6
        Case eScenario.eMid
          Return 3
        Case Else
          Return 3
      End Select
    Else
      Select Case _Scenario
        Case eScenario.eLight
          Return 5
        Case eScenario.eMid
          Return 3
        Case Else
          Return 2
      End Select
    End If
  End Function

  Private Function ScenarioLegSeconds(IsUpwind As Boolean) As Integer
    If IsUpwind Then
      Select Case _Scenario
        Case eScenario.eLight
          Return 10.7 * 60
        Case eScenario.eMid
          Return 6.7 * 60
        Case Else
          Return 7.1 * 60
      End Select
    Else
      Select Case _Scenario
        Case eScenario.eLight
          Return 7.2 * 60
        Case eScenario.eMid
          Return 5 * 60
        Case Else
          Return 4.8 * 40
      End Select
    End If
  End Function

  Private Function ScenarioLaps() As Integer
    Select Case _Scenario
      Case eScenario.eLight
        Return 2
      Case eScenario.eMid
        Return 2
      Case Else
        Return 3
    End Select
  End Function

  Public ReadOnly Property StringaScenario As String
    Get
      Return _StringaScenario
    End Get
  End Property


  Public Function ScenarioDescription() As String
    Dim txtTmp As String = _Scenario.ToString.Substring(1) & " wind scenario, " & ScenarioLaps.ToString("F0") & " laps. For each lap: "
    Dim oraTmp As New DateTime(0)
    oraTmp = oraTmp.AddSeconds(ScenarioLegSeconds(True))
    txtTmp &= "Upwind time: " & oraTmp.ToString("mm:ss") & ", #Tacks: " & ScenarioManoeuvers(True)
    oraTmp = New DateTime(0)
    oraTmp = oraTmp.AddSeconds(ScenarioLegSeconds(False))
    txtTmp &= ". Downwind time: " & oraTmp.ToString("mm:ss") & ", #Gybes: " & ScenarioManoeuvers(True)
    Return txtTmp
  End Function

  Public Property MtTot As Double
    Get
      Return _MtTot
    End Get
    Set(value As Double)
      _MtTot = value
    End Set
  End Property

  Public Property MtSl As Double
    Get
      Return _MtSl
    End Get
    Set(value As Double)
      _MtSl = value
    End Set
  End Property

  Public Property MtInv As Double
    Get
      Return _MtInv
    End Get
    Set(value As Double)
      _MtInv = value
    End Set
  End Property

  Public Property MtMan As Double
    Get
      Return _MtMan
    End Get
    Set(value As Double)
      _MtMan = value
    End Set
  End Property

  Public Property SecTot As Double
    Get
      Return _SecTot
    End Get
    Set(value As Double)
      _SecTot = value
    End Set
  End Property

  Public Property SecSl As Double
    Get
      Return _SecSl
    End Get
    Set(value As Double)
      _SecSl = value
    End Set
  End Property

  Public Property SecInv As Double
    Get
      Return _SecInv
    End Get
    Set(value As Double)
      _SecInv = value
    End Set
  End Property

  Public Property SecMan As Double
    Get
      Return _SecMan
    End Get
    Set(value As Double)
      _SecMan = value
    End Set
  End Property
End Class

Public Class clsStatVals
  Dim _IdAndatura As eAndatura
  Dim _Valori As New List(Of Double)
  Dim _Valori2 As New List(Of Double)

  Public Property IdAndatura As eAndatura
    Get
      Return _IdAndatura
    End Get
    Set(value As eAndatura)
      _IdAndatura = value
    End Set
  End Property

  Public Property Valori As List(Of Double)
    Get
      Return _Valori
    End Get
    Set(value As List(Of Double))
      _Valori = value
    End Set
  End Property

  Public Property Valori2 As List(Of Double)
    Get
      Return _Valori2
    End Get
    Set(value As List(Of Double))
      _Valori2 = value
    End Set
  End Property

  Public Enum eAndatura
    eUp = 0
    eDn = 1
    eRc = 2
    eAll = 3
  End Enum

  Public Sub AggiungiCoppia(v1 As Double, v2 As Double)
    _Valori.Add(v1)
    _Valori2.Add(v2)
  End Sub

End Class

Public Class clsSailingFunctionDetails
  Dim _Id As String
  Dim _Descrizione As String
  Dim _Seconds As Double
  Dim _EnergyJoules As Double

  Public Sub New(Id As Integer, Descrizione As String)
    _Id = Id
    _Descrizione = Descrizione
  End Sub

  Public Property Id As String
    Get
      Return _Id
    End Get
    Set(value As String)
      _Id = value
    End Set
  End Property

  Public Property Descrizione As String
    Get
      Return _Descrizione
    End Get
    Set(value As String)
      _Descrizione = value
    End Set
  End Property

  Public Property Seconds As Double
    Get
      Return _Seconds
    End Get
    Set(value As Double)
      _Seconds = value
    End Set
  End Property

  Public Property EnergyJoules As Double
    Get
      Return _EnergyJoules
    End Get
    Set(value As Double)
      _EnergyJoules = value
    End Set
  End Property

  Public ReadOnly Property PowerWatt As Double
    Get
      Return _EnergyJoules / _Seconds
    End Get
  End Property

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStatisticheIntervallo

  'Dim _CanaleParent As clsChannel2020
  Public ChannelId As String

  Dim _TimeRange As clsTimeRange
  Public Property Avg As Double
  Public Property AvgPort As Double
  Public Property AvgStbd As Double
  Public Property Min As Double
  Public Property Max As Double
  Public Property Sd As Double

  Public Sub New(CanaleParent As clsChannel2020)
    '_CanaleParent = CanaleParent
    ChannelId = CanaleParent.ChannelId
  End Sub

  Public Sub New(CanaleParent As clsChannel2020, TimeRange As clsTimeRange)
    '_CanaleParent = CanaleParent
    ChannelId = CanaleParent.ChannelId
    _TimeRange = TimeRange
    AggiornaIntervallo(_TimeRange, clsGroupLines.eLineType.eDataTypeSigned, CanaleParent.ChannelId)
  End Sub

  Public Sub New(CanaleParent As clsChannel2020, TimeRange As clsTimeRange, DataType As clsGroupLines.eLineType)
    '_CanaleParent = CanaleParent
    ChannelId = CanaleParent.ChannelId
    _TimeRange = TimeRange
    AggiornaIntervallo(_TimeRange, DataType, CanaleParent.ChannelId)
  End Sub

  Private _ChannelCache As clsChannel2020

  Public ReadOnly Property Channel As clsChannel2020
    Get
      ' la lookup per nome veniva rifatta a ogni accesso, anche dai binding della UI
      ' (AvgString, MinString, MaxString... la invocano due volte ciascuna)
      Dim c As clsChannel2020 = _ChannelCache
      If Not c Is Nothing Then
        If String.Equals(c.ChannelId, ChannelId, StringComparison.OrdinalIgnoreCase) Then Return c
      End If
      If DataProvider2020 Is Nothing Then Return Nothing
      _ChannelCache = DataProvider2020.Channels.Canale(ChannelId)
      Return _ChannelCache
    End Get
    'Set(value As clsChannel2020)
    '    _CanaleParent = value
    'End Set
  End Property

  Public Property TimeRange As clsTimeRange
    Get
      Return _TimeRange
    End Get
    Set(value As clsTimeRange)
      _TimeRange = value
    End Set
  End Property

  Public ReadOnly Property AvgString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      'If Avg < 0 Then Stop
      Return Avg.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property AvgPortString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      Return AvgPort.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property AvgStbdString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      Return AvgStbd.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property MinString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      Return Min.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property MaxString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      Return Max.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property SdString As String
    Get
      If Channel Is Nothing Then Return "-"
      If TimeRange Is Nothing Then Return "-"
      Return Sd.ToString("F" & Channel.Decimals.ToString)
    End Get
  End Property

  Public Sub AggiornaIntervallo(TimeRange As clsTimeRange, ValuesType As clsGroupLines.eLineType, ChannelIdCheck As String)
    _TimeRange = TimeRange
    Dim Inizio As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
    Dim Fine As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)
    Dim _CanaleParent As clsChannel2020 = Channel
    If _CanaleParent Is Nothing Then
      ChannelId = ChannelIdCheck
      If _CanaleParent Is Nothing Then
        Exit Sub
      End If
    End If
    'ReDim _ValoriIsStbd(Fine - Inizio)
    Dim CanaleTack As clsChannel2020 = CanaleTackDefault() ' DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eIsStbd)
    Dim LastValidData As DateTime = DataProvider2020.TimeStamps(0)
    Dim objDerivata As New clsDerivata(DataProvider2020.Hz, _CanaleParent.DataType = clsChannel2020.eDataType.e360, LastValidData)

    Dim Omologo As clsChannel2020 = _CanaleParent
    If _CanaleParent.ChannelId.IndexOf("Port") > -1 Then ', 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
      Omologo = DataProvider2020.CanaleDbl(_CanaleParent.ChannelId.Replace("Port", "Stbd"))
      If Omologo Is Nothing Then Omologo = _CanaleParent
    ElseIf _CanaleParent.ChannelId.IndexOf("Stbd") > -1 Then ', 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
      Omologo = DataProvider2020.CanaleDbl(_CanaleParent.ChannelId.Replace("Stbd", "Port"))
      If Omologo Is Nothing Then Omologo = _CanaleParent
    End If

    Dim CompleteTmp As New List(Of Double)
    Dim NotNanTmp As New List(Of Double)
    Dim NotNanPortTmp As New List(Of Double)
    Dim NotNanStbdTmp As New List(Of Double)

    If _CanaleParent.Valori Is Nothing Then
      '_CanaleParent.Valori = Nothing
      Dim c As clsChannel2020 = DataProvider2020.CanaleDbl(_CanaleParent.ChannelId)
    End If
    Try
      Dim TimeStampsLoc As DateTime() = DataProvider2020.TimeStamps
      Dim ValoriLoc As Double() = _CanaleParent.Valori
      Dim ValoriTackLoc As Double() = CanaleTack.Valori
      Dim ValoriOmologoLoc As Double() = Omologo.Valori
      If ValoriLoc Is Nothing OrElse ValoriTackLoc Is Nothing Then Exit Try
      Dim Limite As Integer = Math.Min(Math.Min(TimeStampsLoc.Length, ValoriLoc.Length), ValoriTackLoc.Length) - 1
      If Fine > Limite Then Fine = Limite
      If Inizio < 0 Then Inizio = 0

      For i As Integer = Inizio To Fine
        Dim X As DateTime = TimeStampsLoc(i)
        Dim Y As Double = ValoriLoc(i)
        Dim ValoreTack As Double = ValoriTackLoc(i)
        Dim IsStbd As Boolean = ValoreTack > 0
        If Not Double.IsNaN(Y) AndAlso Not Double.IsNaN(ValoreTack) Then
          Select Case ValuesType
            Case clsGroupLines.eLineType.eRawValue, clsGroupLines.eLineType.eRawValueDeriv
              ' plotta il canale così come da parquet senza applicare le regole del segno
              If ValuesType = clsGroupLines.eLineType.eRawValueDeriv Then
                'Y = ValoriY.Last
                ' derivata del canale prendendo i valori da value
                If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
                Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
                objDerivata.MediaMobile.AggiornaMedia(Derivata)
                Y = objDerivata.MediaMobile.Valore
                objDerivata.AggiornaValoriPrev(Y, X)
              End If
            Case clsGroupLines.eLineType.eDataTypeSigned, clsGroupLines.eLineType.eDataTypeSignedDeriv
              Select Case _CanaleParent.DataType
                Case clsChannel2020.eDataType.eTack
                  If Not IsStbd Then
                    Y *= -1
                  End If
                Case clsChannel2020.eDataType.eTackReversed
                  If IsStbd Then
                    Y *= -1
                  End If
                Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
                  Y = System.Math.Abs(Y)
              End Select
              If ValuesType = clsGroupLines.eLineType.eDataTypeSignedDeriv Then
                If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
                Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
                objDerivata.MediaMobile.AggiornaMedia(Derivata)
                Y = objDerivata.MediaMobile.Valore
                objDerivata.AggiornaValoriPrev(Y, X)
              End If
            Case clsGroupLines.eLineType.eDataTypeSignedAndWwdLwd, clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv
              ' se il canale ha Port nel nome stampa sempre il lato sottovento
              ' se il canale ha Stbd nel nome stampa sempre il lato sopravento
              ' se non c'è port o stbd stampa 
              ' se un  canale è di tipo absolute, 180,  tacksigned o reversed tack signed applica la convenzione
              If Not IsStbd Then
                If Not ValoriOmologoLoc Is Nothing AndAlso i < ValoriOmologoLoc.Length Then Y = ValoriOmologoLoc(i)
              End If
              If Not Double.IsNaN(Y) Then
                Select Case _CanaleParent.DataType
                  Case clsChannel2020.eDataType.e180, clsChannel2020.eDataType.eAbs180
                    Y = System.Math.Abs(Y)
                  Case clsChannel2020.eDataType.eTack
                    If Not IsStbd Then
                      Y *= -1
                    End If
                  Case clsChannel2020.eDataType.eTackReversed
                    If IsStbd Then
                      Y *= -1
                    End If
                End Select
                If ValuesType = clsGroupLines.eLineType.eDataTypeSignedAndWwdLwdDeriv Then
                  ' stampa la derivata dei valori Windward Leeward
                  If Double.IsNaN(objDerivata.ValoreRigaPrev) Then objDerivata.ValoreRigaPrev = Y
                  Dim Derivata As Double = (Y - objDerivata.ValoreRigaPrev) / X.Subtract(objDerivata.MomentoPrev).TotalSeconds
                  objDerivata.MediaMobile.AggiornaMedia(Derivata)
                  Y = objDerivata.MediaMobile.Valore
                  objDerivata.AggiornaValoriPrev(Y, X)
                End If
              Else
                'l'omologo é nan mette nan ad y
                'Stop
                Y = Double.NaN
              End If
          End Select
          '_ValoriIsStbd(i - Inizio) = New clsValoreIsStbd(Y, IsStbd)
          CompleteTmp.Add(Y)
          If Not Double.IsNaN(Y) AndAlso Not Double.IsInfinity(Y) Then
            NotNanTmp.Add(Y)
            If IsStbd Then
              NotNanStbdTmp.Add(Y)
            Else
              NotNanPortTmp.Add(Y)
            End If
          End If
        End If
      Next
      ImpostaValori(NotNanTmp.ToArray)
      AvgPort = CalcolaMedia(NotNanPortTmp.ToArray)
      AvgStbd = CalcolaMedia(NotNanStbdTmp.ToArray)
      Sd = CalcolaSd(NotNanTmp.ToArray)
    Catch ex As Exception
      Stop
    End Try

    'AggiornaBinding()
  End Sub

  Private Function CalcolaMedia(Valori As Double()) As Double
    If Valori.Count = 0 Then Return Double.NaN
    If Channel.DataType = clsChannel2020.eDataType.e360 Then
      Dim s = Valori.Select(Function(x) System.Math.Sin(Radians(x))).Average
      Dim c = Valori.Select(Function(x) System.Math.Cos(Radians(x))).Average
      Dim MediaTmp As Double = Degrees(System.Math.Atan2(s, c))
      If MediaTmp < 0 Then MediaTmp += 360
      Return MediaTmp
    Else
      Return Valori.Average
    End If
  End Function

  Private Sub ImpostaValori(Valori As Double())
    If Valori.Count = 0 Then
      Min = Double.NaN
      Max = Double.NaN
    End If
    If Valori.Count = 0 Then
      Avg = 0
      Min = 0
      Max = 0
    ElseIf Channel.DataType = clsChannel2020.eDataType.e360 Then
      Dim s = Valori.Select(Function(x) System.Math.Sin(Radians(x))).Average
      Dim c = Valori.Select(Function(x) System.Math.Cos(Radians(x))).Average
      Avg = Degrees(System.Math.Atan2(s, c))
      If Avg < 0 Then Avg += 360
      Dim Delta = Valori.Select(Function(x) DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Avg, x)).ToArray
      Max = SommaAngolo180adAngolo360(Delta.Max, Avg) 'max right
      Min = SommaAngolo180adAngolo360(Delta.Min, Avg) 'max left
    Else
      Avg = Valori.Average
      Min = Valori.Min
      Max = Valori.Max
    End If
  End Sub

  Private Function CalcolaSd(Valori As Double()) As Double
    Dim SdTmp As Double
    Try
      alglib.basestat.sampleadev(Valori.ToArray, Valori.Count, SdTmp)
      Return SdTmp
    Catch ex As Exception
      'Stop
      Return Double.NaN
    End Try
  End Function


End Class

Public Class clsSpeedTest


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsCoppieValoriTwsVsCanale
  Public Property Canale As clsChannel2020
  Public Property CoppieValori As New List(Of clsDoubleXY) ' X:tws y: canale


  Public Sub New(Canale As clsChannel2020)
    Me.Canale = Canale
  End Sub

  ' campioni per fascia di Tws intera (Tws arrotondato), costruiti alla prima richiesta: i report chiedono migliaia di
  ' volte la stessa fascia e scorrere ogni volta tutte le coppie diventava lentissimo con molti dati
  Dim _PerFascia As Dictionary(Of Integer, List(Of Double)) = Nothing

  Private Function PerFascia() As Dictionary(Of Integer, List(Of Double))
    If _PerFascia Is Nothing Then
      _PerFascia = New Dictionary(Of Integer, List(Of Double))
      For Each cv In CoppieValori
        Dim k As Integer = CInt(Math.Floor(cv.X + 0.5))
        Dim l As List(Of Double) = Nothing
        If Not _PerFascia.TryGetValue(k, l) Then
          l = New List(Of Double)
          _PerFascia.Add(k, l)
        End If
        l.Add(cv.Y)
      Next
    End If
    Return _PerFascia
  End Function

  ''' <summary>True se l'intervallo [MinTws, MaxTws) e' una fascia intera [t-0.5, t+0.5): in quel caso si usa PerFascia.</summary>
  Private Shared Function FasciaIntera(MinTws As Double, MaxTws As Double, ByRef Fascia As Integer) As Boolean
    If Math.Abs((MaxTws - MinTws) - 1) > 0.000001 Then Return False
    Dim t As Double = MinTws + 0.5
    If Math.Abs(t - Math.Round(t)) > 0.000001 Then Return False
    Fascia = CInt(Math.Round(t))
    Return True
  End Function

  Public Sub AccodaDati(TR As clsTimeRange, Optional Maschera As Func(Of Integer, Boolean) = Nothing)
    _PerFascia = Nothing
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
      If Not Maschera Is Nothing AndAlso Not Maschera(i) Then Continue For
      Dim tws = chTws.Valori(i)
      Dim val = Canale.Valori(i)
      If Canale.DataType = clsChannel2020.eDataType.e180 Then
        val = Math.Abs(Canale.Valori(i))
      End If
      If Not Double.IsNaN(tws) AndAlso Not Double.IsNaN(val) Then
        CoppieValori.Add(New clsDoubleXY(tws, val))
      End If
    Next
  End Sub

  Public ReadOnly Property MinTws As Double
    Get
      If CoppieValori.Count = 0 Then Return 0
      Return CoppieValori.Min(Function(x) x.X)
    End Get
  End Property

  Public ReadOnly Property MaxTws As Double
    Get
      If CoppieValori.Count = 0 Then Return 0
      Return CoppieValori.Max(Function(x) x.X)
    End Get
  End Property

  ''' <summary>Statistiche dei campioni nella fascia di Tws. BandaPercentuale &lt; 100: Min e Max sono i percentili della banda centrale.</summary>
  Public Function Valori(MinTws As Double, MaxTws As Double, Optional BandaPercentuale As Double = 100) As clsValoriBase
    Dim Fascia As Integer
    If FasciaIntera(MinTws, MaxTws, Fascia) Then
      Dim l As List(Of Double) = Nothing
      If Not PerFascia().TryGetValue(Fascia, l) Then l = New List(Of Double)
      Return New clsValoriBase(l.ToArray, Canale.DataType, BandaPercentuale)
    End If
    Dim cv = CoppieValori.Where(Function(x) x.X >= MinTws AndAlso x.X < MaxTws)
    If cv Is Nothing Then Return Nothing
    Return New clsValoriBase(cv.Select(Function(x) x.Y).ToArray, Canale.DataType, BandaPercentuale)
  End Function

  ''' <summary>Numero di campioni validi (uno per riga del file) nella fascia di Tws.</summary>
  Public Function Conteggio(MinTws As Double, MaxTws As Double) As Integer
    Dim Fascia As Integer
    If FasciaIntera(MinTws, MaxTws, Fascia) Then
      Dim l As List(Of Double) = Nothing
      Return If(PerFascia().TryGetValue(Fascia, l), l.Count, 0)
    End If
    Return CoppieValori.Where(Function(x) x.X >= MinTws AndAlso x.X < MaxTws).Count
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsValoriPeriodoCanale2020
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Dim pCanale As clsChannel2020
  Dim pTimeRange As clsTimeRange
  Dim pAvgOrg As Double
  Dim pAvg As Double
  Dim pAvgPort As Double
  Dim pAvgStbd As Double
  Dim pMin As Double
  Dim pMax As Double
  Dim pDs As Double
  Dim pForceAbsVal As Boolean = False

  Public ReadOnly Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
  End Property

  Public ReadOnly Property TimeRange As clsTimeRange
    Get
      Return pTimeRange
    End Get
  End Property

  Public ReadOnly Property AvgOrg As Double
    Get
      Return pAvgOrg
    End Get
  End Property

  Public ReadOnly Property Avg As Double
    Get
      Return pAvg
    End Get
  End Property

  Public ReadOnly Property Min As Double
    Get
      Return pMin
    End Get
  End Property

  Public ReadOnly Property Max As Double
    Get
      Return pMax
    End Get
  End Property

  Public ReadOnly Property Ds As Double
    Get
      Return pDs
    End Get
  End Property

  Public ReadOnly Property AvgString As Double
    Get
      Return Format(pAvg, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property AvgOrgString As Double
    Get
      Return Format(pAvgOrg, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property AvgPortString As Double
    Get
      Return Format(pAvgPort, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property AvgStbdString As Double
    Get
      Return Format(pAvgStbd, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property MinString As Double
    Get
      Return Format(pMin, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property MaxString As Double
    Get
      Return Format(pMax, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property DsString As Double
    Get
      Return Format(pDs, "F" & pCanale.Decimals.ToString)
    End Get
  End Property

  Public ReadOnly Property ValoriNotNan As Double()
    Get
      Dim VnotNanP, VnotNanS As Double()
      Dim VnotNan As Double() = DataProvider2020.ValoriIntervallo(pCanale, pTimeRange, VnotNanP, VnotNanS, True, False)
      Return VnotNan
    End Get
  End Property

  Public Sub New(Canale As clsChannel2020, TimeRange As clsTimeRange, ForceAbsVal As Boolean)
    pCanale = Canale
    pTimeRange = TimeRange
    pForceAbsVal = ForceAbsVal
    'AggiornaBinding()
  End Sub

  'Private Sub AggiornaBinding()
  '  OnPropertyChanged("Canale")
  '  OnPropertyChanged("TimeRange")
  '  OnPropertyChanged("Avg")
  '  OnPropertyChanged("Min")
  '  OnPropertyChanged("Max")
  '  OnPropertyChanged("Ds")
  '  OnPropertyChanged("AvgString")
  '  OnPropertyChanged("AvgPortString")
  '  OnPropertyChanged("AvgStbdString")
  '  OnPropertyChanged("MinString")
  '  OnPropertyChanged("MaxString")
  '  OnPropertyChanged("DsString")
  'End Sub

  Public Function AggiornaValori(TimeRange As clsTimeRange, ForzaAbsolute As Boolean) As Double()
    pTimeRange = TimeRange
    Return AggiornaValori(ForzaAbsolute)
  End Function

  Public Function AggiornaValori(ForzaAbsolute As Boolean) As Double()
    Dim VnotNanP As Double() = Nothing
    Dim VnotNanS As Double() = Nothing
    Dim VnotNan As Double() = DataProvider2020.ValoriIntervallo(pCanale, pTimeRange, VnotNanP, VnotNanS, True, ForzaAbsolute, Maschera)
    Return AggiornaValori(VnotNan, VnotNanP, VnotNanS)
  End Function

  ''' <summary>Filtro opzionale sulle righe (indice del campione): se impostato le statistiche usano solo le righe per cui e' True.</summary>
  Public Property Maschera As Func(Of Integer, Boolean) = Nothing

  ''' <summary>False se dopo l'ultimo AggiornaValori non c'era nessun campione valido (per esempio tutti scartati dal filtro).</summary>
  Public ReadOnly Property HaDati As Boolean
    Get
      Return pHaDati
    End Get
  End Property
  Dim pHaDati As Boolean = True

  Public Function AggiornaValori(VnotNan As Double(), VnotNanP As Double(), VnotNanS As Double()) As Double()
    pHaDati = Not (VnotNan Is Nothing OrElse VnotNan.Count = 0)
    If pCanale.Valori Is Nothing OrElse pCanale.Valori.Count = 0 Then
      pAvg = 0
      pAvgPort = 0
      pAvgStbd = 0
      pMin = 0
      pMax = 0
      pDs = 0
    Else
      'Dim VnotNanP, VnotNanS As Double()
      'Dim VnotNan As Double() = DataProvider2020.ValoriIntervallo(pCanale.Valori, pTimeRange, VnotNanP, VnotNanS, True)
      If VnotNan Is Nothing OrElse VnotNan.Count = 0 Then
        pAvg = 0
        pAvgOrg = 0
        pAvgPort = 0
        pAvgStbd = 0
        pMin = 0
        pMax = 0
        pDs = 0
      Else
        If pCanale.DataType = clsChannel2020.eDataType.e360 Then
          Dim Sin = VnotNan.Select(Function(x) System.Math.Sin(Radians(x)))
          Dim Cos = VnotNan.Select(Function(x) System.Math.Cos(Radians(x)))
          pAvg = Degrees(System.Math.Atan2(Sin.Sum / Sin.Count, Cos.Sum / Cos.Count))

          If VnotNanP.Count > 0 Then
            Sin = VnotNanP.Select(Function(x) System.Math.Sin(Radians(x)))
            Cos = VnotNanP.Select(Function(x) System.Math.Cos(Radians(x)))
            pAvgPort = Degrees(System.Math.Atan2(Sin.Sum / Sin.Count, Cos.Sum / Cos.Count))
          Else
            pAvgPort = 0
          End If

          If VnotNanS.Count > 0 Then
            Sin = VnotNanS.Select(Function(x) System.Math.Sin(Radians(x)))
            Cos = VnotNanS.Select(Function(x) System.Math.Cos(Radians(x)))
            pAvgStbd = Degrees(System.Math.Atan2(Sin.Sum / Sin.Count, Cos.Sum / Cos.Count))
          Else
            pAvgStbd = 0
          End If
          If pAvg < 0 Then pAvg += 360
          pAvgOrg = pAvg
          If pAvgPort < 0 Then pAvgPort += 360
          If pAvgStbd < 0 Then pAvgStbd += 360
          'If pAvg < 360 Then pAvg += 360
          'If pAvgPort < 360 Then pAvgPort += 360
          'If pAvgStbd < 360 Then pAvgStbd += 360
          Dim Delta = VnotNan.Select(Function(x) DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pAvg, x))
          pMax = SommaAngolo180adAngolo360(Delta.Max, pAvg) 'max right
          pMin = SommaAngolo180adAngolo360(Delta.Min, pAvg) 'max left
        Else
          If pForceAbsVal OrElse pCanale.DataType = clsChannel2020.eDataType.eAbs180 Then
            pAvgOrg = VnotNan.Average
            VnotNan = VnotNan.Select(Function(x) System.Math.Abs(x)).ToArray
            VnotNanS = VnotNanS.Select(Function(x) System.Math.Abs(x)).ToArray
            VnotNanP = VnotNanP.Select(Function(x) System.Math.Abs(x)).ToArray
          End If

          pAvg = VnotNan.Average
          pAvgOrg = pAvg

          If VnotNanS.Count > 0 Then
            pAvgStbd = VnotNanS.Average
          Else
            pAvgStbd = 0
          End If
          If VnotNanP.Count > 0 Then
            pAvgPort = VnotNanP.Average
          Else
            pAvgPort = 0
          End If
          pMax = VnotNan.Max
          pMin = VnotNan.Min
        End If
        alglib.basestat.sampleadev(VnotNan.ToArray, VnotNan.Count, pDs)
      End If
      'AggiornaBinding()
      Return VnotNan
    End If
    'AggiornaBinding()
    Return pCanale.Valori
  End Function

End Class

Public Class clsValoriBase
  Dim pAvg As Double
  Dim pMin As Double
  Dim pMax As Double
  Dim pDs As Double

  Public ReadOnly Property Avg As Double
    Get
      Return pAvg
    End Get
  End Property

  Public ReadOnly Property Min As Double
    Get
      Return pMin
    End Get
  End Property

  Public ReadOnly Property Max As Double
    Get
      Return pMax
    End Get
  End Property

  Public ReadOnly Property Ds As Double
    Get
      Return pDs
    End Get
  End Property

  ''' <summary>Percentile P (0-100) con interpolazione lineare su valori gia' ordinati in modo crescente.</summary>
  Public Shared Function Percentile(Ordinati As Double(), P As Double) As Double
    If Ordinati Is Nothing OrElse Ordinati.Length = 0 Then Return Double.NaN
    If Ordinati.Length = 1 Then Return Ordinati(0)
    Dim Pos As Double = Math.Max(0, Math.Min(100, P)) / 100.0 * (Ordinati.Length - 1)
    Dim i As Integer = CInt(Math.Floor(Pos))
    If i >= Ordinati.Length - 1 Then Return Ordinati(Ordinati.Length - 1)
    Return Ordinati(i) + (Pos - i) * (Ordinati(i + 1) - Ordinati(i))
  End Function

  ''' <summary>
  ''' Min e Max della serie. Con BandaPercentuale &lt; 100 sono i percentili agli estremi della banda centrale
  ''' (90 = P5 e P95); con 100 sono il minimo e il massimo veri.
  ''' </summary>
  Public Sub New(Valori As Double(), DataType As clsChannel2020.eDataType, Optional BandaPercentuale As Double = 100)
    If Valori.Count = 0 Then
      pAvg = 0
      pMin = 0
      pMax = 0
      pDs = 0
    Else
      Dim VnotNan = Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)).ToList
      If VnotNan.Count = 0 Then
        pAvg = 0
        pMin = 0
        pMax = 0
        pDs = 0
      ElseIf DataType = clsChannel2020.eDataType.e360 Then
        Dim Sin = VnotNan.Select(Function(x) System.Math.Sin(Radians(x)))
        Dim Cos = VnotNan.Select(Function(x) System.Math.Cos(Radians(x)))
        pAvg = Degrees(System.Math.Atan2(Sin.Sum / Sin.Count, Cos.Sum / Cos.Count))
        If pAvg < 0 Then pAvg += 360
        Dim Delta = VnotNan.Select(Function(x) DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pAvg, x)).ToArray
        If BandaPercentuale < 100 Then
          Dim DeltaOrdinati As Double() = Delta.OrderBy(Function(x) x).ToArray
          Dim Basso As Double = (100 - BandaPercentuale) / 2
          pMax = SommaAngolo180adAngolo360(Percentile(DeltaOrdinati, 100 - Basso), pAvg)
          pMin = SommaAngolo180adAngolo360(Percentile(DeltaOrdinati, Basso), pAvg)
        Else
          pMax = SommaAngolo180adAngolo360(Delta.Max, pAvg) 'max right
          pMin = SommaAngolo180adAngolo360(Delta.Min, pAvg) 'max left
        End If
        alglib.basestat.sampleadev(Delta.ToArray, Delta.Count, pDs)
      Else
        pAvg = VnotNan.Average
        If BandaPercentuale < 100 Then
          Dim Ordinati As Double() = VnotNan.OrderBy(Function(x) x).ToArray
          Dim Basso As Double = (100 - BandaPercentuale) / 2
          pMax = Percentile(Ordinati, 100 - Basso)
          pMin = Percentile(Ordinati, Basso)
        Else
          pMax = VnotNan.Max
          pMin = VnotNan.Min
        End If
        alglib.basestat.sampleadev(VnotNan.ToArray, VnotNan.Count, pDs)
      End If
    End If
  End Sub

End Class

Public Class clsFiles2020

  Public Function SelezionaFiles(pathIniziale As String, intestazioneFinestra As String, stringaFiltro As String, EstensioneDefault As String, ByRef NomiFileSelezionati As List(Of String)) As List(Of String)
    Dim FD As New Microsoft.Win32.OpenFileDialog

    If Not EstensioneDefault = "" Then
      FD.DefaultExt = EstensioneDefault
      FD.AddExtension = True
    End If
    FD.CheckFileExists = True
    FD.CheckPathExists = True
    FD.Filter = stringaFiltro
    FD.InitialDirectory = pathIniziale
    FD.Multiselect = True
    FD.Title = intestazioneFinestra
    If FD.ShowDialog Then
      NomiFileSelezionati = FD.SafeFileNames.ToList
      Return FD.FileNames.ToList
    Else
      NomiFileSelezionati = Nothing
      Return Nothing
    End If
  End Function

  Public Function SelezionaFileDati() As List(Of String)
    Dim UltimoPath As String = AppConfig.ActiveProfile.LastPpfFolder

    'Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
    'Dim LastExt As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", "txt", True, True)
    If System.IO.Directory.Exists(UltimoPath) Then
      UltimoPath = UltimoPath
    ElseIf System.IO.File.Exists(UltimoPath) Then
      UltimoPath = System.IO.Path.GetDirectoryName(UltimoPath)
    Else
      ' i ppf e i file da importare stanno in Data, non in Settings
      UltimoPath = AppConfig.ActiveProfile.DataFolder
      If String.IsNullOrWhiteSpace(UltimoPath) OrElse Not System.IO.Directory.Exists(UltimoPath) Then UltimoPath = AppConfig.ActiveProfile.ProfileFolder
    End If
    Dim SelFileNames As New List(Of String)
    Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Performance File|*.ppf|Parquet Files|*.parquet|All Files|*.*", "ppf", SelFileNames)
    If SelectedFiles Is Nothing Then Return Nothing
    AppConfig.ActiveProfile.LastPpfFolder = GetFileInfo(SelectedFiles.First).Directory.FullName
    AppConfig.Salva()

    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", GetFileInfo(SelectedFiles.First).Directory.FullName, True, True)
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", GetFileInfo(SelectedFiles.First).Extension, True, True)
    Return SelectedFiles
  End Function

  Public Sub SalvaImpostazioniFileCreato(SelectedFile As String)
    'Stop
    AppConfig.ActiveProfile.LastPpfFolder = GetFileInfo(SelectedFile).Directory.FullName
    AppConfig.Salva()

    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", GetFileInfo(SelectedFile).Directory.FullName, True, True)
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", GetFileInfo(SelectedFile).Extension, True, True)
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsMySongLeeway
  Dim TwaTable As clsTable1D
  Dim TwsTable As clsTable1D
  Dim SeaStateTable As clsTable1D
  Dim KeelAngleTable As clsTable2D
  Dim DaggerImmersionTable As clsTable1D
  Dim DaggerAngleTable As clsTable1D
  'legge le tabelle usate nel faro

  Public Function KtwaFromTable(twa As Double) As Double
    Return 1
  End Function
  Public Function KtwsFromTable(tws As Double) As Double
    Return 1
  End Function
  Public Function KSeaStateFromTable(SeaState As Double) As Double
    Return 1
  End Function


  Public Function CalculateNormalizedLeeway(HeelingNormalized As Double, BoatSpeed As Double, Twa As Double, Tws As Double, SeaState As Double, KeelAngle As Double, DaggerImmersion As Double, DaggerAngle As Double) As Double

    Dim Ktwa As Double = KtwaFromTable(Twa)
    Dim Ktws As Double = KtwsFromTable(Tws)
    Dim KSeaState As Double = KSeaStateFromTable(SeaState)
    Dim KeelArea As Double = 1 ' AppConfig.ActiveProfile.
    Dim DaggerArea As Double = 1
    Dim DaggerOffset As Double = 0

    'aaaaaaaaaaaaaaaaa
    Return 0

  End Function


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsIntervalloTack
  Public Property X1 As DateTime
  Public Property X2 As DateTime
  ''' <summary>0 = Stbd, 1 = Port, 2 = HeadDeadToWind</summary>
  Public Property Modo As Byte

  Public Sub New(X1 As DateTime, X2 As DateTime, Modo As Byte)
    Me.X1 = X1
    Me.X2 = X2
    Me.Modo = Modo
  End Sub
End Class

Public Class clsDataProvider2020
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  'viene creato un data provider per barca
  Public Property Channels As New clsChannels2020(Me)
  Dim pFileType As eFileType
  Dim pSuffissoFileType As String
  Dim pIntestazioni As New List(Of String)
  Dim pFiles As New List(Of System.IO.FileInfo)
  Public ParquetFiles As New List(Of clsFileParquet2020)
  Dim pFaRoBinFiles As New List(Of clsFaRoBin)
  'Dim pSqLiteGombocFiles As New List(Of clsFileSqLite2020)
  Dim pTimeRange As clsTimeRange
  Dim _TimeStamps As DateTime()
  Public Property ValoriCaricati As Boolean = False
  ' --- parametri (fissi) delle statistiche riepilogative del singolo file ---
  ''' <summary>Velocita' minima assoluta perche' si possa parlare di navigazione.</summary>
  Dim pSailingTimeBsMin As Double = 3
  ''' <summary>Frazione della velocita' di riferimento del file usata come soglia di navigazione.</summary>
  Dim pSailingTimeBsPerc As Double = 0.4
  ''' <summary>Velocita' oltre la quale la barca e' considerata partita (esclude l'ormeggio iniziale).</summary>
  Dim pMovimentoBs As Double = 2
  Const PercentileLow As Integer = 5
  Const PercentileHigh As Integer = 95
  Const PercentileTopSpeed As Integer = 99

  ''' <summary>Percentile robusto su un vettore di valori gia' privi di NaN.</summary>
  Private Function Percentile(Valori As Double(), P As Integer) As Double
    If Valori Is Nothing OrElse Valori.Count = 0 Then Return Double.NaN
    If Valori.Count = 1 Then Return Valori(0)
    Return MathNet.Numerics.Statistics.Statistics.Percentile(Valori, P)
  End Function

  Private Function FormatGradi(Valore As Double) As String
    Dim v As Integer = CInt(Math.Round(Valore / 10)) * 10
    v = ((v Mod 360) + 360) Mod 360
    Return Format(v, "F0").PadLeft(3, "0") & "°"
  End Function

  ''' <summary>
  ''' Range percentile di un canale circolare. I valori vengono srotolati attorno alla media
  ''' prima di essere ordinati, altrimenti a cavallo dei 360 gradi il risultato non ha senso.
  ''' </summary>
  Private Sub RangePercentileAngolare(Valori As Double(), Media As Double, PLow As Integer, PHigh As Integer, ByRef Basso As Double, ByRef Alto As Double)
    Dim Srotolati(Valori.Count - 1) As Double
    For i As Integer = 0 To Valori.Count - 1
      Dim d As Double = Valori(i) - Media
      While d > 180
        d -= 360
      End While
      While d < -180
        d += 360
      End While
      Srotolati(i) = d
    Next
    Basso = Media + Percentile(Srotolati, PLow)
    Alto = Media + Percentile(Srotolati, PHigh)
  End Sub

  Dim _Hz As Integer
  Dim _CanaleTack As clsChannel2020

#Region "Cache TwaMode / intervalli di mure"

  ''' <summary>
  ''' Modo di navigazione per ogni campione, precalcolato una sola volta per caricamento.
  ''' 0 = Stbd, 1 = Port, 2 = HeadDeadToWind. Prima veniva ricalcolato campione per campione
  ''' dentro ogni plot (44 volte lo stesso risultato).
  ''' </summary>
  Private _TwaModes As Byte() = Nothing

  Private _IntervalliTack As List(Of clsIntervalloTack) = Nothing

  Public Const TwaModeStbd As Byte = 0
  Public Const TwaModePort As Byte = 1
  Public Const TwaModeHeadDead As Byte = 2

  ''' <summary>Azzera le cache derivate dai valori dei canali. Va chiamata a ogni ricaricamento.</summary>
  Public Sub InvalidaCacheDerivate()
    _TwaModes = Nothing
    _IntervalliTack = Nothing
  End Sub

  Public ReadOnly Property TwaModes As Byte()
    Get
      If _TwaModes Is Nothing Then CostruisciTwaModes()
      Return _TwaModes
    End Get
  End Property

  Private Sub CostruisciTwaModes()
    Dim n As Integer = 0
    If Not _TimeStamps Is Nothing Then n = _TimeStamps.Length
    Dim Risultato(Math.Max(n - 1, 0)) As Byte

    Dim CanaleTack As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    If CanaleTack Is Nothing OrElse CanaleTack.Valori Is Nothing Then
      For i As Integer = 0 To n - 1
        Risultato(i) = TwaModeHeadDead
      Next
      _TwaModes = Risultato
      Exit Sub
    End If

    Dim Valori As Double() = CanaleTack.Valori
    Dim nMax As Integer = Math.Min(n, Valori.Length)
    For i As Integer = 0 To nMax - 1
      Dim v As Double = Valori(i)
      If Double.IsNaN(v) Then
        Risultato(i) = TwaModeHeadDead
      Else
        Dim a As Double = System.Math.Abs(v)
        If a < 5 OrElse a > 175 Then
          Risultato(i) = TwaModeHeadDead
        ElseIf v > 0 Then
          Risultato(i) = TwaModeStbd
        Else
          Risultato(i) = TwaModePort
        End If
      End If
    Next
    For i As Integer = nMax To n - 1
      Risultato(i) = TwaModeHeadDead
    Next
    _TwaModes = Risultato
  End Sub

  ''' <summary>
  ''' Intervalli omogenei di mura, calcolati una sola volta e riusati da tutti i plot
  ''' per disegnare lo sfondo Port/Stbd.
  ''' </summary>
  Public ReadOnly Property IntervalliTack As List(Of clsIntervalloTack)
    Get
      If _IntervalliTack Is Nothing Then CostruisciIntervalliTack()
      Return _IntervalliTack
    End Get
  End Property

  Private Sub CostruisciIntervalliTack()
    Dim Risultato As New List(Of clsIntervalloTack)
    Dim Modi As Byte() = TwaModes
    If _TimeStamps Is Nothing OrElse _TimeStamps.Length = 0 OrElse Modi Is Nothing OrElse Modi.Length = 0 Then
      _IntervalliTack = Risultato
      Exit Sub
    End If

    Dim n As Integer = Math.Min(_TimeStamps.Length, Modi.Length)
    Dim ModoCorrente As Byte = Modi(0)
    Dim Xiniziale As DateTime = _TimeStamps(0)

    For i As Integer = 1 To n - 1
      If Not Modi(i) = ModoCorrente Then
        Dim X As DateTime = _TimeStamps(i)
        If (X - Xiniziale).TotalSeconds >= 1 Then
          Risultato.Add(New clsIntervalloTack(Xiniziale, X, ModoCorrente))
        End If
        Xiniziale = X
        ModoCorrente = Modi(i)
      End If
    Next
    ' ultimo intervallo aperto fino alla fine dei dati
    Risultato.Add(New clsIntervalloTack(Xiniziale, _TimeStamps(n - 1), ModoCorrente))

    _IntervalliTack = Risultato
  End Sub

#End Region
  Dim _DampSec As Double = 1
  'Dim _Periods2021 As New clsPeriods2021
  'Dim _PeriodsManager2021 As clsPeriodsManager2021

  Public Property KnownSails As New List(Of clsKnownSail)
  Public Property SailSet As New List(Of String)



  Public Enum eFileType
    eUnknown = -1
    eParquet = 0
    eFaRoBin = 1
    eGombocSqlLite = 2
    eFaRoCsv = 3
    eExpLog = 4
    eDfwLog = 5
  End Enum

  Public ReadOnly Property IsSimulFile As Boolean
    Get
      Return pIntestazioni(1).StartsWith("Block_")
    End Get
  End Property

  Public ReadOnly Property SelectedFilesList() As String
    Get
      'Stop
      Dim strTmp As String = ""
      Select Case pFileType
        Case eFileType.eParquet
          Dim lista = ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
          For Each file In lista.ToList
            strTmp &= file.TimeRangeRealDateTime.InizioFormattato(clsTimeRange.eTipoFormatoData.eDateTimeEsteso) & " (" & file.TimeRangeRealDateTime.DurataInStringaConSeparatore & ")"
            Dim BsNotNaN As List(Of Double?) = file.BoatSpeedNotNan
            If Not BsNotNaN Is Nothing AndAlso BsNotNaN.Count > 0 Then
              Dim vBs As Double() = BsNotNaN.Select(Function(x) x.Value).ToArray

              ' velocita' di riferimento robusta: il massimo assoluto e' quasi sempre uno spike di sensore
              Dim TopSpeed As Double = Percentile(vBs, PercentileTopSpeed)

              ' la soglia di "in navigazione" e' relativa alla barca e alle condizioni:
              ' una soglia fissa a 10 nodi azzera il valore su qualsiasi giornata di poco vento
              Dim SogliaSailing As Double = Math.Max(pSailingTimeBsMin, TopSpeed * pSailingTimeBsPerc)

              ' il conteggio parte dal primo movimento reale, cosi' l'ormeggio iniziale non falsa la percentuale
              Dim IdPrimoMovimento As Integer = 0
              For i As Integer = 0 To vBs.Count - 1
                If vBs(i) > pMovimentoBs Then
                  IdPrimoMovimento = i
                  Exit For
                End If
              Next
              Dim Navigazione As Double() = vBs.Skip(IdPrimoMovimento).ToArray
              If Navigazione.Count > 0 Then
                Dim Sailing As Integer = Navigazione.Where(Function(x) x >= SogliaSailing).Count
                strTmp &= ", SailingTime: " & Format(Sailing / Navigazione.Count * 100, "F0") & "%"
              End If

              Dim TwsNotNan As List(Of Double?) = file.TrueWindSpeedNotNan
              If Not TwsNotNan Is Nothing AndAlso TwsNotNan.Count > 0 Then
                Dim vTws As Double() = TwsNotNan.Select(Function(x) x.Value).ToArray
                ' mediana come vento rappresentativo, percentile alto come raffica: niente picchi da sensore
                strTmp &= ", Tws: " & Format(Percentile(vTws, 50), "F1") & " (" & Format(Percentile(vTws, PercentileHigh), "F0") & ")"
              End If

              Dim TwdNotNan As List(Of Double?) = file.TrueWindDirNotNan
              If Not TwdNotNan Is Nothing AndAlso TwdNotNan.Count > 0 Then
                Dim vTwd As Double() = TwdNotNan.Select(Function(x) x.Value).ToArray
                Dim Twd As New clsValoriBase(vTwd, clsChannel2020.eDataType.e360)
                Dim TwdLo As Double, TwdHi As Double
                RangePercentileAngolare(vTwd, Twd.Avg, PercentileLow, PercentileHigh, TwdLo, TwdHi)
                strTmp &= ", Twd: " & FormatGradi(Twd.Avg) & " (" & FormatGradi(TwdLo) & "-" & FormatGradi(TwdHi) & ")"
              End If

              strTmp &= ", TopSpeed: " & Format(TopSpeed, "F1")
              strTmp &= ", " & file.FileInfo.Name
              strTmp &= vbCrLf
            End If
          Next
        Case Else
          Stop
      End Select
      Return strTmp
    End Get
  End Property

  Public Function GetSailNameBySailCode(SailCode As Double) As String
    If KnownSails Is Nothing Then Return ""
    If KnownSails.Count = 0 Then Return ""
    Dim Sail = KnownSails.Where(Function(x) x.SailCode = SailCode).FirstOrDefault
    If Sail Is Nothing Then Return ""
    Return Sail.AliasName
  End Function

  Public ReadOnly Property SuffissoFileType As String
    Get
      Return System.Enum.GetName(GetType(eFileType), pFileType).TrimStart("e") & "2020"
    End Get
  End Property

  Public ReadOnly Property IsStbd(Indice As Integer) As Boolean
    Get
      Return _CanaleTack.Valori(Indice) > 0
    End Get
  End Property
  'Public Property Channels As clsChannels2020
  '  Get
  '    Return pChannels
  '  End Get
  '  Set(value As clsChannels2020)
  '    pChannels = value
  '  End Set
  'End Property

  Public ReadOnly Property TimeStamps As DateTime()
    Get
      Return _TimeStamps
    End Get
  End Property

  Public ReadOnly Property VerificaCanaleTarget(CanaleRef As clsChannel2020) As clsChannel2020
    Get
      If VerificaCanaleTgtPol(CanaleRef, False) Then
        Return CanaleDbl(CanaleRef.PolarHeader & "_Tgt")
      Else
        Return Nothing
      End If
    End Get
  End Property

  Public ReadOnly Property VerificaCanalePolar(CanaleRef As clsChannel2020) As clsChannel2020
    Get
      If VerificaCanaleTgtPol(CanaleRef, False) Then
        Return CanaleDbl(CanaleRef.PolarHeader & "_Pol")
      Else
        Return Nothing
      End If
    End Get
  End Property

  Public ReadOnly Property CanaleDbl(CanaleChiave As clsChannels2020.eCanaliChiave) As clsChannel2020
    Get
      Dim c As clsChannel2020 = Channels.Canale(CanaleChiave)
      If c Is Nothing Then Return Nothing
      'If c.ChannelId = "AccX" Then Stop
      CaricaValoriParquet(c)
      Return c
    End Get
  End Property
  Public ReadOnly Property CanaleDbl(NomeChiave As String) As clsChannel2020
    Get
      'Stop
      Dim c As clsChannel2020 = Channels.Canale(NomeChiave)
      If c Is Nothing Then Return Nothing
      'If c.ChannelId = "AccX" Then Stop
      CaricaValoriParquet(c)
      Return c
    End Get
  End Property

  Public ReadOnly Property CanaleDbl(Canale As clsChannel2020) As clsChannel2020
    Get
      'Stop
      If Canale Is Nothing Then Return Nothing
      'If c.ChannelId = "AccX" Then Stop
      CaricaValoriParquet(Canale)
      Return Canale
    End Get
  End Property

  'Public ReadOnly Property CanaleConVerificaArrayValori(CanaleChiave As clsChannels2020.eCanaliChiave) As clsChannel2020
  '  Get
  '    Return Channels.CanaleConVerificaArrayValori(CanaleChiave)
  '  End Get
  'End Property

  'Public ReadOnly Property CanaleDblIsLoaded(CanaleChiave As clsChannels2020.eCanaliChiave) As Boolean
  '  Get
  '    Return Channels.CanaleIsLoaded(CanaleChiave)
  '  End Get
  'End Property


  Public ReadOnly Property Momento(Riga As Integer) As DateTime
    Get
      If TimeStamps(Riga) = Nothing Then
        For i As Integer = 0 To 100
          Dim ii As Integer = Math.Min(TimeStamps.Count - 1, Riga + i)
          If Not TimeStamps(ii) = Nothing Then
            Return TimeStamps(ii)
          End If
        Next
      End If
      Return TimeStamps(Riga)
    End Get
  End Property

  Public Property FileType As eFileType
    Get
      Return pFileType
    End Get
    Set(value As eFileType)
      pFileType = value
    End Set
  End Property

  Public ReadOnly Property Intestazioni As List(Of String)
    Get
      Return pIntestazioni
    End Get
  End Property

  Public Property TimeRange As clsTimeRange
    Get
      Return pTimeRange
    End Get
    Set(value As clsTimeRange)
      pTimeRange = value
    End Set
  End Property

  Public Property Files As List(Of FileInfo)
    Get
      Return pFiles
    End Get
    Set(value As List(Of FileInfo))
      pFiles = value
    End Set
  End Property

  Public Sub New(PathFiles As List(Of String))
    CaricaIntestazioni(PathFiles)
  End Sub

  Public ReadOnly Property Hz As Integer
    Get
      Return _Hz
      'If pChannels.CanaleDT Is Nothing Then Return 0
      'Dim pHz As Integer = pTimeStamps.Count / pTimeRange.Durata.TotalSeconds
      'NormalizzaHz(pHz)
      'Return pHz
    End Get
  End Property

  Public Function IsTimeRangeLoaded(TR As clsTimeRange) As Boolean
    For Each fp In ParquetFiles
      If fp.TimeRangeRealDateTime.IsFullyOverlapped(TR) Then Return True
      'If TR.IsFullyOverlapped(fp.TimeRangeRealDateTime) Then Return True
    Next
    Return False
  End Function

  Public Function TrovaIndice(Momento As DateTime) As Integer
    'Return TrovaIndice(Momento, pTimeStamps)
    Return TrovaIndice(Momento, TimeStamps)
  End Function

  Public Function TrovaIndiceDaCanaleDT(Momento As DateTime) As Integer
    ' il CanaleDT contiene i valori reali del tempo, quello performance quelli fittizzi che servono quando i file sono multipli ovvero risultato del parquet finder
    ' questo va usato quando il TR viene preso dal visiblerange dell asse X dei grafici
    Return TrovaIndice(Momento, TimeStamps)
    'Return TrovaIndice(Momento, TimeStamp)
  End Function

  Public Function TrovaIndice(Momento As DateTime, ValoriDT As Date()) As Integer
    If Momento = Nothing Then Return -1
    Dim L As Integer = 0
    Dim U As Integer = ValoriDT.Count - 1
    Dim DTtmp As DateTime
    Dim PrevIndex As Integer = 0
    Dim IndexTmp As Integer
    'Dim delta As Integer = -1

    Do While U - L > 1 '  AndAlso PrevIndex = IndexTmp
      IndexTmp = CInt((U + L) / 2)
      DTtmp = ValoriDT(IndexTmp)


      Do While IndexTmp > L AndAlso IndexTmp < U
        If ValoriDT(IndexTmp).ToOADate = 0 Then
          IndexTmp += 1
          DTtmp = ValoriDT(IndexTmp)
        Else
          Exit Do
        End If
      Loop
      If Momento = DTtmp Then
        Return IndexTmp
      ElseIf Momento > DTtmp Then
        L = IndexTmp
      Else
        U = IndexTmp
      End If

      If PrevIndex = IndexTmp Then Exit Do
      PrevIndex = IndexTmp
    Loop

    Dim UpTmp As Integer = L + 1
    If UpTmp >= ValoriDT.Count - 1 Then

    Else
      Do
        If ValoriDT(UpTmp).ToOADate = 0 Then
          UpTmp += 1
          If UpTmp >= ValoriDT.Count - 1 Then
            Exit Do
          End If
        Else
          Exit Do
        End If
      Loop
    End If

    U = Math.Min(UpTmp, ValoriDT.Count - 1)
    If System.Math.Abs(Momento.Subtract(ValoriDT(L)).TotalSeconds) < System.Math.Abs(Momento.Subtract(ValoriDT(U)).TotalSeconds) Then
      IndexTmp = L
    Else
      IndexTmp = U
    End If
    Dim d As TimeSpan = Momento.Subtract(ValoriDT(IndexTmp))
    If System.Math.Abs(d.TotalSeconds) > 1 Then
      'Dim Minimatrix = ValoriDT.Skip(L - 100).Take(200).ToList
      'Console.WriteLine("Delta Mills:" & d.TotalMilliseconds & " Search:" & Momento.ToString("dd/MM/yyyy HH:mm:ss.fff") & " Find:" & ValoriDT(IndexTmp).ToString("dd/MM/yyyy HH:mm:ss.fff"))
    Else
      'Console.WriteLine(d.TotalMilliseconds)
    End If
    Return IndexTmp

  End Function

  Public Function ValoriIntervallo(ArrayCompleto As Double(), IndiceIniziale As Integer, IndiceFinale As Integer) As Double()
    Dim Risultato(IndiceFinale - IndiceIniziale - 1) As Double
    Array.Copy(ArrayCompleto, IndiceIniziale, Risultato, 0, IndiceFinale - IndiceIniziale)
    Return Risultato
  End Function

  Public Function ValoriIntervallo(ArrayValori As Double(), MomentoIniziale As DateTime, MomentoFinale As DateTime) As Double()
    Dim IndiceIniziale As Integer = TrovaIndice(MomentoIniziale)
    Dim IndiceFinale As Integer = TrovaIndice(MomentoFinale)
    Return ValoriIntervallo(ArrayValori, IndiceIniziale, IndiceFinale)
  End Function

  Public Function ValoriIntervallo(ArrayValori As Double(), TimeRange As clsTimeRange) As Double()
    Dim IndiceIniziale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Start), True)
    Dim IndiceFinale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Finish), False)
    Return ValoriIntervallo(ArrayValori, IndiceIniziale, IndiceFinale)
  End Function


  ''' <summary>Come l'altra versione per array, ma tiene solo le righe per cui Maschera(indice) e' True (Nothing = tutte).</summary>
  Public Function ValoriIntervallo(ArrayValori As Double(), TimeRange As clsTimeRange, Maschera As Func(Of Integer, Boolean)) As Double()
    If Maschera Is Nothing Then Return ValoriIntervallo(ArrayValori, TimeRange)
    Dim IndiceIniziale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Start), True)
    Dim IndiceFinale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Finish), False)
    Dim V As New List(Of Double)
    For i As Integer = IndiceIniziale To IndiceFinale - 1
      If Maschera(i) Then V.Add(ArrayValori(i))
    Next
    Return V.ToArray
  End Function

  Public Function ValoriIntervallo(Canale As clsChannel2020, TimeRange As clsTimeRange, ByRef SoloPort As Double(), ByRef SoloStbd As Double(), SoloNotNan As Boolean, ForceAbsolute As Boolean, Optional Maschera As Func(Of Integer, Boolean) = Nothing) As Double()
    If Canale Is Nothing Then Return Nothing
    If TimeRange Is Nothing Then Return Nothing
    If Canale.Valori Is Nothing Then Return Nothing
    Try
      Dim ArrayValori As Double() = Canale.Valori
      Dim IndiceIniziale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Start), True)
      Dim IndiceFinale As Integer = VerificaNotNothing(TrovaIndice(TimeRange.Finish), False)
      'IndiceFinale = System.Math.Min(IndiceFinale, ArrayValori.Length) - 1
      'Dim Vtmp As Double() = ValoriIntervallo(ArrayValori, IndiceIniziale, IndiceFinale)
      Dim V As New List(Of Double)
      Dim P As New List(Of Double)
      Dim S As New List(Of Double)
      'Dim CanaleTack As clsChannel2020 = CanaleTackDefault()

      If Not _CanaleTack Is Nothing Then
        Select Case Canale.DataType
          Case clsChannel2020.eDataType.eAbs180, clsChannel2020.eDataType.eAbsLinear
            For i As Integer = IndiceIniziale To IndiceFinale - 1
              If Not Maschera Is Nothing AndAlso Not Maschera(i) Then Continue For
              Dim vl = ArrayValori(i)
              If Not Double.IsInfinity(vl) Then
                If SoloNotNan Then
                  If Not Double.IsNaN(vl) Then
                    vl = System.Math.Abs(vl)
                    V.Add(vl)
                    If _CanaleTack.Valori(i) > 0 Then
                      S.Add(vl)
                    Else
                      P.Add(vl)
                    End If
                  End If
                Else
                  If Not Double.IsNaN(vl) Then vl = System.Math.Abs(vl)
                  V.Add(vl)
                  If _CanaleTack.Valori(i) > 0 Then
                    S.Add(vl)
                  Else
                    P.Add(vl)
                  End If
                End If
              End If
            Next
          Case Else
            For i As Integer = IndiceIniziale To IndiceFinale - 1
              If Not Maschera Is Nothing AndAlso Not Maschera(i) Then Continue For
              Dim vl = ArrayValori(i)
              If Not Double.IsInfinity(vl) Then
                If ForceAbsolute Then vl = System.Math.Abs(vl)
                If SoloNotNan Then
                  If Not Double.IsNaN(vl) Then
                    V.Add(vl)
                    If _CanaleTack.Valori(i) > 0 Then
                      S.Add(vl)
                    Else
                      P.Add(vl)
                    End If
                  End If
                Else
                  V.Add(vl)
                  If _CanaleTack.Valori(i) > 0 Then
                    S.Add(vl)
                  Else
                    P.Add(vl)
                  End If
                End If
              End If
            Next
        End Select
      End If
      'If SoloNotNan Then
      '  Vtmp = Vtmp.Where(Function(x) Not Double.IsNaN(x)).ToArray
      'End If
      SoloPort = P.ToArray
      SoloStbd = S.ToArray
      Return V.ToArray
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public Function VerificaNotNothing(Indice As Integer, CercaSuccessivo As Boolean) As Integer
    Do
      'If pTimeStamps(Indice) = Nothing Then
      If TimeStamps(Indice) = Nothing Then
        If CercaSuccessivo Then
          Indice += 1
        Else
          Indice -= 1
        End If
        If Indice >= TimeStamps.Count Then Return -1
      Else
        Return Indice
      End If
    Loop
    Return -1
  End Function

  Public ReadOnly Property SourceFilesTimeRanges As List(Of clsTimeRange)
    Get
      Dim tmp As New List(Of clsTimeRange)
      Select Case pFileType
        Case eFileType.eParquet
          For Each File In ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
            tmp.Add(File.TimeRangeRealDateTime)
          Next
        Case Else
          Stop
      End Select
      Return tmp
    End Get
  End Property

  'Public Property PeriodsManager2021 As clsPeriodsManager2021
  '  Get
  '    Return _PeriodsManager2021
  '  End Get
  '  Set(value As clsPeriodsManager2021)
  '    _PeriodsManager2021 = value
  '  End Set
  'End Property

  'Public Property Periods2021 As clsPeriods2021
  '  Get
  '    Return _Periods2021
  '  End Get
  '  Set(value As clsPeriods2021)
  '    _Periods2021 = value
  '  End Set
  'End Property

  Private Sub CaricaIntestazioni(PathFiles As List(Of String))
    pFiles.Clear()
    pIntestazioni.Clear()
    For Each Path In PathFiles
      Dim Fi As New System.IO.FileInfo(Path)
      pFiles.Add(Fi)
      ' carica le intestazioni del file parquet
      Select Case pFiles.First.Extension
        Case ".parquet", ".ppf"
          pFileType = eFileType.eParquet
          Dim npf = New clsFileParquet2020(Fi)
          If npf.IsValid Then
            ParquetFiles.Add(npf)
            'pParquetFiles.Last.DisposeReader()
            AggiungiIntestazioniUnivoche(ParquetFiles.Last.Intestazioni)
          End If
        'Case ".db"
        '  pFileType = eFileType.eGombocSqlLite
        '  pSqLiteGombocFiles.Add(New clsFileSqLite2020(Fi, False))
        '  AggiungiIntestazioniUnivoche(pSqLiteGombocFiles.Last.Intestazioni)
        Case ".bin"
          pFileType = eFileType.eFaRoBin
          pFaRoBinFiles.Add(New clsFaRoBin(Fi))
          AggiungiIntestazioniUnivoche(pFaRoBinFiles.Last.Intestazioni)
        Case Else
          Stop
      End Select
    Next

    Select Case pFiles.First.Extension
      Case ".parquet", ".ppf"
        'VerificaIntestazioniComuniFileParquetCaricati()
      Case ".db"
        Stop
      Case ".bin"
        Stop
      Case Else
        Stop
    End Select

    Channels.MappaCanali(pIntestazioni)
    ValoriCaricati = False
    'OnPropertyChanged("SelectedFilesList")
  End Sub


  Private Sub AggiungiIntestazioniUnivoche(Intestazioni As List(Of String))
    For Each Intestazione In Intestazioni
      If pIntestazioni.Where(Function(x) x.ToLower = Intestazione.ToLower).Count = 0 Then
        pIntestazioni.Add(Intestazione)
      End If
    Next
  End Sub


  Public Sub OrcRaceReplay()
    Dim Testo As String = clsRatingUtilities.RaceReplay()
    Clipboard.SetText(Testo)
  End Sub

  Public Sub FunzioneCustom()
    Dim tv As New SailsTable

    tv.TabellaVele()

    Stop


    Dim c As clsChannel2020 = CanaleDbl("Board")
    If Not c Is Nothing Then

      Dim Miniz As Integer = TrovaIndiceDaCanaleDT(New Date(2023, 10, 14, 12, 0, 0))
      Dim Mfin As Integer = TrovaIndiceDaCanaleDT(New Date(2023, 10, 14, 12, 30, 0))
      If Miniz > -1 AndAlso Mfin > -1 Then
        For i As Integer = Miniz To Mfin
          c.Valori(i) = -30
        Next
      End If
      Miniz = TrovaIndiceDaCanaleDT(New Date(2023, 10, 14, 14, 0, 0))
      Mfin = TrovaIndiceDaCanaleDT(New Date(2023, 10, 14, 15, 0, 0))
      If Miniz > -1 AndAlso Mfin > -1 Then
        For i As Integer = Miniz To Mfin
          c.Valori(i) = -30
        Next
      End If

      Miniz = TrovaIndiceDaCanaleDT(New Date(2023, 10, 17, 10, 0, 0))
      Mfin = TrovaIndiceDaCanaleDT(New Date(2023, 10, 17, 12, 50, 0))
      If Miniz > -1 AndAlso Mfin > -1 Then
        For i As Integer = Miniz To Mfin
          c.Valori(i) = -30
        Next
      End If

    End If


  End Sub

  Public Sub CaricaValoriCanaliSelezionati()
    Select Case pFileType
      Case eFileType.eParquet
        AzzeraValoriCanali()
        Console.WriteLine("AA " & Now.ToString("HHmmss.fff"))
        CaricaValoriParquet()
        Console.WriteLine("AB " & Now.ToString("HHmmss.fff"))
        AggiornaTimeStamps()
        Console.WriteLine("AC " & Now.ToString("HHmmss.fff"))
        'AggiornaListaCanali()
        'Console.WriteLine("AD " & Now.ToString("HHmmss.fff"))
        'CaricaCanaliMathParquet()
        'Console.WriteLine("AE " & Now.ToString("HHmmss.fff"))
      Case eFileType.eFaRoBin
        Stop
        'CaricaValoriFaRoBin()
        'AggiornaListaCanali()
        'CaricaCanaliMathFaRoBin()
      Case eFileType.eGombocSqlLite
        Stop
        'CaricaValoriGomboc()
        'AggiornaListaCanali()
        'CaricaCanaliMathGomboc()
      Case Else
        Stop
        ValoriCaricati = False
        Exit Sub
    End Select
    'AggiornaListaCanali()
    ValoriCaricati = True
    'Dim c = Channels.Canale(clsChannels2020.eCanaliChiave.eImuTopMastTwist)
    Console.WriteLine("AX " & Now.ToString("HHmmss.fff"))
    _CanaleTack = CanaleTackDefault()
    ImpostaHz()
    Console.WriteLine("AY " & Now.ToString("HHmmss.fff"))
  End Sub

  Private Sub ImpostaHz()
    Try
      Dim Test As Integer = 0
      Dim Indice As Integer = 0
      Dim HzPrev(2) As Integer
      For i As Integer = 0 To HzPrev.Count - 1
        HzPrev(i) = -1
      Next
      Do
        Dim Inizio As DateTime = Momento(Test)
        Dim Fine As DateTime = Inizio.AddSeconds(10)
        Dim HzTmp As Integer = (TrovaIndice(Fine) - TrovaIndice(Inizio)) / 10
        NormalizzaHz(HzTmp)
        HzPrev(Indice) = HzTmp
        Dim TuttiUguali As Boolean = True
        For i As Integer = 1 To HzPrev.Count - 1
          If Not HzPrev(i - 1) = HzPrev(i) Then
            TuttiUguali = False
            Exit For
          End If
        Next
        If TuttiUguali Then
          _Hz = HzPrev(0)
          Exit Sub
        End If
        Dim td As Integer = Math.Min(100, Int(TimeStamps.Count / 3))
        Test += td
        Test = Math.Min(Test, TimeStamps.Count - 3)
        Indice += 1
        If Indice >= HzPrev.Count Then Indice = 0
      Loop
      Stop
      _Hz = 1
    Catch ex As Exception
      Stop
      _Hz = 1
    End Try
  End Sub

  Private Sub AggiornaTimeStamps()
    Dim TStmp As New List(Of DateTime)
    For Each FP In ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start).ToList
      For Each Ts In FP.TimeStamps
        TStmp.Add(Ts)
      Next
    Next
    _TimeStamps = TStmp.ToArray
    InvalidaCacheDerivate()
  End Sub

  'Private Sub AggiornaListaCanali()

  '  'Channels.ListaCanali.Clear()
  '  'For Each canale In Channels.ListaCanaliFileCorrente.Where(Function(x) x.Importa = True OrElse x.IsMath = True).ToList
  '  '  Channels.ListaCanali.Add(canale)
  '  'Next


  '  Dim chTmp = Channels.ListaCanali.Where(Function(x) x.CanaleChiave = clsChannels2020.eCanaliChiave.eTimeOnly).FirstOrDefault
  '  If Not chTmp Is Nothing Then
  '    Channels.CanaleDT = chTmp
  '  Else
  '    For Each canale In Channels.ListaCanali
  '      If Not canale.ValoriDT Is Nothing Then
  '        Channels.CanaleDT = canale
  '        Exit Sub
  '      End If
  '    Next
  '  End If
  '  Dim chTmpDtPerf = Channels.ListaCanali.Where(Function(x) x.ChannelId = "SystemTime_Performance").FirstOrDefault
  '  If chTmpDtPerf Is Nothing Then
  '    Channels.CanaleDtPerformance = chTmp
  '  Else
  '    Channels.CanaleDtPerformance = chTmpDtPerf
  '    ReDim TimeStamp(Channels.CanaleDtPerformance.Valori.Count - 1)
  '    For i As Integer = 0 To Channels.CanaleDtPerformance.Valori.Count - 1
  '      TimeStamp(i) = DateTime.FromOADate(Channels.CanaleDtPerformance.Valori(i))
  '    Next
  '  End If


  'End Sub

  'Public Sub AggiungiCanali()
  '  Dim Lista As New List(Of clsChannel2020)
  '  For Each canale In Channels.ListaCanali
  '    If Channels.ListaCanali.Where(Function(x) x Is canale).Count = 0 Then
  '      ' canale marcato come da importare ma dati non caricati
  '      Lista.Add(canale)
  '    End If
  '  Next
  '  If Lista.Count > 0 Then
  '    For Each File In pParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
  '      File.CaricaValoriCanali(Lista)
  '    Next
  '    AggiornaListaCanali()
  '    'CaricaCanaliMathParquet()
  '  End If
  'End Sub


  Private Sub AzzeraValoriCanali()
    Dim TStmp As New List(Of DateTime)
    pTimeRange = New clsTimeRange(Nothing, Nothing)
    InvalidaCacheDerivate()
    ' rilascia gli handle dei file del set precedente
    For Each pf In ParquetFiles
      Try
        pf.ChiudiReader()
      Catch ex As Exception
      End Try
    Next
    For Each Canale In Channels.ListaCanali
      Canale.Valori = Nothing
      Canale.ValoriDT = Nothing
    Next
  End Sub
  Private Sub CaricaValoriParquet()
    Dim TStmp As New List(Of DateTime)
    pTimeRange = New clsTimeRange(Nothing, Nothing)
    For Each FP In ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
      FP.CaricaValoriCanaliTipoTime(Channels.ListaCanali.ToList) ' carica solo i canali time
      pTimeRange.VerificaInizio(FP.TimeRangeRealDateTime.Start)
      pTimeRange.VerificaFine(FP.TimeRangeRealDateTime.Finish)
      For Each Ts In FP.TimeStamps
        TStmp.Add(Ts)
      Next
    Next
    _TimeStamps = TStmp.ToArray
  End Sub


  Public Sub VerificaSeValoriCaricati(Canale As clsChannel2020, ExportMath As Boolean)
    If Canale.Valori Is Nothing OrElse Canale.Valori.Count < 2 Then
      'If Not HasValidValues(Canale) Then
      Canale.Valori = Nothing
      For Each File In ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
        If ExportMath AndAlso Canale.IsMath Then
          'VerificaCanaleMathsParquet(Canale)
        Else
          File.CaricaValoriCanale(Canale)
        End If
      Next
    End If
  End Sub

  Private Sub CaricaValoriParquet(Canale As clsChannel2020)
    'Console.WriteLine("A CaricaValoriParquet Channel:" & Canale.ChannelId & " ")
    'If Canale.ChannelId = "AccX" Then Stop
    If Not Canale.Valori Is Nothing Then
      If Canale.Valori.Count > 1 Then Exit Sub
    End If

    Dim adesso As DateTime = Now
    If Canale.IsMath Then
      'Console.WriteLine("B CaricaValoriParquet - Load Math data, Polar " & Canale.PolarHeader & " Channel:" & Canale.ChannelId & " ") 
      VerificaCanaleMathsParquet(Canale)
      'qui non occorre girare tra tutti i files perché se dovesse mancare uno dei canali ai quali il canale math fa riferimento verrebbe integralmente caricato
    Else
      'Console.WriteLine("B CaricaValoriParquet - Loading data, Channel:" & Canale.ChannelId & " ")
      Canale.Valori = Nothing
      For Each File In ParquetFiles.OrderBy(Function(x) x.TimeRangeRealDateTime.Start)
        File.CaricaValoriCanale(Canale)
      Next
    End If
    'Console.WriteLine("C CaricaValoriParquet - Loaded " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
  End Sub


  Public Function VerificaCanaleTgtPol(CanaleRef As clsChannel2020, RicaricaValori As Boolean) As Boolean
    Dim NomeCanaleTarget As String = CanaleRef.PolarHeader.ToLower
    If NomeCanaleTarget.Trim = "" Then Return False
    'If NomeCanaleTarget = "awa" Then Stop
    'If NomeCanaleTarget = "aws" Then Stop
    'If NomeCanaleTarget = "vmg" Then Stop
    If TgtManager.Tgt Is Nothing Then Return False
    If Not TgtManager.Tgt.ListaCanaliValori.Contains(NomeCanaleTarget, StringComparer.CurrentCultureIgnoreCase) Then Return False

    Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)


    Dim CanaleTgt As clsChannel2020 = CanaleDbl(NomeCanaleTarget & "_Tgt")
    Dim CanalePol As clsChannel2020 = CanaleDbl(NomeCanaleTarget & "_Pol")
    Dim adesso As DateTime = Now
    If CanaleTgt Is Nothing OrElse CanaleTgt.Valori.Sum = 0 Then
      ' crea il canale target
      'Console.WriteLine("VerificaCanaleTgtPol - Creating Target Channel: " & NomeCanaleTarget & "_Tgt and " & NomeCanaleTarget & "_Pol")
      CanaleTgt = New clsChannel2020(NomeCanaleTarget & "_Tgt", clsChannels2020.eCanaliChiave.eNone, NomeCanaleTarget & "_Tgt", NomeCanaleTarget & "_Tgt", CanaleRef.ShortUM, CanaleRef.LongUM, CanaleRef.DataType, "", Nothing, NomeCanaleTarget, CanaleRef.BenchmarkHeader, True, CanaleRef.Decimals)
      Channels.ListaCanali.Add(CanaleTgt)
      CanalePol = New clsChannel2020(NomeCanaleTarget & "_Pol", clsChannels2020.eCanaliChiave.eNone, NomeCanaleTarget & "_Pol", NomeCanaleTarget & "_Pol", CanaleRef.ShortUM, CanaleRef.LongUM, CanaleRef.DataType, "", Nothing, NomeCanaleTarget, CanaleRef.BenchmarkHeader, True, CanaleRef.Decimals)
      Channels.ListaCanali.Add(CanalePol)
      'Console.WriteLine("VerificaCanaleTgtPol - Channels created " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    End If
    If RicaricaValori OrElse CanaleTgt.Valori Is Nothing OrElse CanaleTgt.Valori.Count = 0 Then
      ReDim CanaleTgt.Valori(chTws.Valori.Count - 1)
      ReDim CanalePol.Valori(chTws.Valori.Count - 1)
    Else
      If CanaleTgt.Valori.Count > 0 Then Return True
    End If

    adesso = Now
    Dim SwTgt As Stopwatch = Stopwatch.StartNew()

    Dim Polare As clsChannelTarget = TgtManager.Tgt.Polare(NomeCanaleTarget)
    If Polare Is Nothing Then Return False

    'ObjContaTempo.StampaMillisecondiTrascorsi("   Target inizio loop ")

    ' calcolo veloce (interpolatori di riga costruiti una volta sola); se la tabella e' vuota si usa il ciclo classico
    If Polare.CalcolaSerie(chTws.Valori, chTwa.Valori, CanaleTgt.Valori, CanalePol.Valori) Then
      ' verifica a campione contro il calcolo classico, riportata nel log dei tempi (differenza massima assoluta)
      Try
        Dim Passo As Integer = Math.Max(1, chTws.Valori.Length \ 300)
        Dim MaxDiff As Double = 0
        Dim Controllate As Integer = 0
        For i As Integer = 0 To chTws.Valori.Length - 1 Step Passo
          Dim TWS As Double = chTws.Valori(i)
          If Double.IsNaN(TWS) Then Continue For
          Dim TWA As Double = System.Math.Abs(chTwa.Valori(i))
          Dim dp As Double = Math.Abs(Polare.PolarValue(TWS, TWA) - CanalePol.Valori(i))
          Dim dt As Double = Math.Abs(Polare.TargetValue(TWS, TWA <= 90) - CanaleTgt.Valori(i))
          If Not Double.IsNaN(dp) Then MaxDiff = Math.Max(MaxDiff, dp)
          If Not Double.IsNaN(dt) Then MaxDiff = Math.Max(MaxDiff, dt)
          Controllate += 1
        Next
        clsLogTempi.Scrivi("  target/polare " & NomeCanaleTarget & ": verifica a campione su " & Controllate & " righe, differenza massima " & MaxDiff.ToString("E2"))
      Catch ex As Exception
        clsLogTempi.Scrivi("  target/polare " & NomeCanaleTarget & ": verifica a campione fallita: " & ex.Message)
      End Try
    Else
      For i As Long = 0 To chTws.Valori.Count - 1
        Dim TWS As Double = chTws.Valori(i)
        If Not Double.IsNaN(TWS) Then
          Dim TWA As Double = System.Math.Abs(chTwa.Valori(i))
          Dim pol As Double = Polare.PolarValue(TWS, TWA)
          Dim tgt As Double = Polare.TargetValue(TWS, TWA <= 90)
          CanaleTgt.Valori(i) = tgt
          CanalePol.Valori(i) = pol
        Else
          CanaleTgt.Valori(i) = Double.NaN
          CanalePol.Valori(i) = Double.NaN
        End If
      Next
    End If
    clsLogTempi.Scrivi("  target/polare " & NomeCanaleTarget & ": serie calcolate in " & SwTgt.ElapsedMilliseconds & " ms")
    'Console.WriteLine("VerificaCanaleTgtPol - Channels Data Loaded " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    'ObjContaTempo.StampaMillisecondiTrascorsi("   Target fine loop ")

    Return CanaleTgt.Valori.Count > 0

  End Function

  Public Function CseFromHdgAndLeeway() As clsChannel2020

    Dim chCse = New clsChannel2020("Cse", clsChannels2020.eCanaliChiave.eCSE, "Cse", "Course", "d", "deg", clsChannel2020.eDataType.e360, "", Nothing, "", "", False, 1) ', Nothing)
    Channels.ListaCanali.Add(chCse)

    Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chHdg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim chLwyN As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
    ReDim chCse.Valori(chHdg.Valori.Count - 1)
    For i As Long = 0 To chTwa.Valori.Count - 1
      Dim Twa As Double = chTwa.Valori(i)
      If Double.IsNaN(Twa) Then
        chCse.Valori(i) = Double.NaN
      Else
        If Twa >= 0 Then
          chCse.Valori(i) = SommaAngolo180adAngolo360(-chLwyN.Valori(i), chHdg.Valori(i))
        Else
          chCse.Valori(i) = SommaAngolo180adAngolo360(chLwyN.Valori(i), chHdg.Valori(i))
        End If
      End If
    Next
    Return chCse
  End Function


  ''' <summary>Calcola il canale math se non ancora calcolato; scrive nel log dei tempi quelli che richiedono un calcolo vero.</summary>
  Public Sub VerificaCanaleMathsParquet(Canale As clsChannel2020)
    If Not Canale.Valori Is Nothing AndAlso Canale.Valori.Count > 1 Then Exit Sub
    Using clsLogTempi.Misura("calcolo math " & Canale.ChannelId & " (righe " & If(TimeStamps Is Nothing, 0, TimeStamps.Count) & ")")
      VerificaCanaleMathsParquetInterno(Canale)
    End Using
  End Sub

  Private Sub VerificaCanaleMathsParquetInterno(Canale As clsChannel2020)
    Dim adesso As DateTime = Now
    'Console.WriteLine("VerificaCanaleMathsParquet - Loading Math Channel: " & Canale.ChannelId)
    If Canale.Valori Is Nothing Then
      ReDim Canale.Valori(TimeStamps.Count - 1)
    Else
      If Canale.Valori.Count > 1 Then Exit Sub
    End If
    Select Case Canale.CanaleChiave
      Case clsChannels2020.eCanaliChiave.eVMG 'ricalcola sempre il vmg
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        ReDim Canale.Valori(chSow.Valori.Count - 1)
        For i As Long = 0 To chSow.Valori.Count - 1
          Dim Sow As Double = chSow.Valori(i)
          Dim Twa As Double = System.Math.Abs(chTwa.Valori(i))
          If Double.IsNaN(Sow) OrElse Double.IsNaN(Twa) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = Sow * System.Math.Abs(System.Math.Cos(Radians(Twa)))
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eVMGp
        If Not TgtManager.Tgt Is Nothing Then
          Dim chVmg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVmg)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chVmg.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Vmg As Double = chVmg.Valori(i)
            If Double.IsNaN(Vmg) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = Vmg / CanaleTgt.Valori(i) * 100
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eBSTp
        If Not TgtManager.Tgt Is Nothing Then
          Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
          Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chSow)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chSow.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Sow As Double = chSow.Valori(i)
            Dim Twa As Double = chTwa.Valori(i)
            If Double.IsNaN(Sow) OrElse Double.IsNaN(Twa) Then
              Canale.Valori(i) = Double.NaN
            Else
              If Math.Abs(Twa) > 80 AndAlso Math.Abs(Twa) < 100 Then
                Canale.Valori(i) = Double.NaN
              Else
                Dim R = Sow / CanaleTgt.Valori(i) * 100
                If R > 150 OrElse R < 0 Then
                  Canale.Valori(i) = Double.NaN
                Else
                  Canale.Valori(i) = R
                End If
              End If
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eBSPp
        If Not TgtManager.Tgt Is Nothing Then
          Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanalePolar(chSow)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chSow.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Sow As Double = chSow.Valori(i)
            If Double.IsNaN(Sow) Then
              Canale.Valori(i) = Double.NaN
            Else
              Dim R = Sow / CanaleTgt.Valori(i) * 100
              If R > 150 OrElse R < 0 Then
                Canale.Valori(i) = Double.NaN
              Else
                Canale.Valori(i) = R
              End If
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eFlyingStatus
        Dim chRH As clsChannel2020 = CanaleDbl("FCS_RideHeight")
        Dim chCantPort As clsChannel2020 = CanaleDbl("FCS_PortCant_Ang")
        Dim chCantStbd As clsChannel2020 = CanaleDbl("FCS_StbdCant_Ang")
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        'Dim CanaleFS As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eFlyingStatus)
        If Not (chRH Is Nothing OrElse chCantPort Is Nothing OrElse chCantStbd Is Nothing OrElse chSow Is Nothing) Then
          ReDim Canale.Valori(chSow.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Sow As Double = chSow.Valori(i)
            If Double.IsNaN(Sow) Then
              Canale.Valori(i) = Double.NaN
            Else
              Dim cs As Double = chCantStbd.Valori(i)
              Dim cp As Double = chCantPort.Valori(i)
              Dim rh As Double = chRH.Valori(i)
              If Not (Double.IsNaN(cs) OrElse Double.IsNaN(cp) OrElse Double.IsNaN(rh)) Then
                If Sow > 16 Then
                  If (chCantPort.Valori(i) > 30 And chCantStbd.Valori(i) < 30) OrElse (chCantPort.Valori(i) < 30 And chCantStbd.Valori(i) > 30) Then
                    ' un solo arm in acqua velocita' superiore ai 17 nodi 
                    If chRH.Valori(i) > 0.2 Then
                      Canale.Valori(i) = 10 ' volo normale
                    ElseIf chRH.Valori(i) > -0.1 Then
                      Canale.Valori(i) = 8 ' volo skimming
                    Else
                      Canale.Valori(i) = 6 ' volo bassissimo
                    End If
                  ElseIf chCantPort.Valori(i) < 30 And chCantStbd.Valori(i) < 30 Then
                    Canale.Valori(i) = 2 ' due boards in acqua manovra o towing
                  End If
                ElseIf Sow > 12 Then
                  Canale.Valori(i) = 4 ' accelerazione displacement
                Else
                  Canale.Valori(i) = 0 ' volo non volo
                End If
              Else
                Canale.Valori(i) = Double.NaN
              End If
            End If
          Next
        End If

      Case clsChannels2020.eCanaliChiave.eTWAd
        If Not TgtManager.Tgt Is Nothing Then
          Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chTwa)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chTwa.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Twa As Double = chTwa.Valori(i)
            If Double.IsNaN(Twa) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = System.Math.Abs(Twa) - CanaleTgt.Valori(i)
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eAwaWithLwy
        If Not TgtManager.Tgt Is Nothing Then
          Dim chAwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eAbsAwa)
          Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
          ReDim Canale.Valori(chAwa.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Twa As Double = chAwa.Valori(i)
            Dim Lwy As Double = chLwy.Valori(i)
            If Double.IsNaN(Twa) OrElse Double.IsNaN(Lwy) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = Twa + Lwy
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eTwaNoLeeway
        If Not TgtManager.Tgt Is Nothing Then
          Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
          Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
          ReDim Canale.Valori(chTwa.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Twa As Double = chTwa.Valori(i)
            Dim Lwy As Double = chLwy.Valori(i)
            If Double.IsNaN(Twa) OrElse Double.IsNaN(Lwy) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = System.Math.Abs(Twa) - Lwy
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eAwaRecLwy, clsChannels2020.eCanaliChiave.eAwsRecLwy, clsChannels2020.eCanaliChiave.eAwaRecNoLwy
        Dim chTwaRecLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaRecLeeway)
        Dim chTws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chBs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chLwy3s As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyK3s)

        If Not chTwaRecLwy Is Nothing OrElse Not chTws Is Nothing OrElse Not chBs Is Nothing OrElse Not chLwy3s Is Nothing Then
          Dim chAwaRecLwy = Channels.Canale(clsChannels2020.eCanaliChiave.eAwaRecLwy)
          Dim chAwsRecLwy = Channels.Canale(clsChannels2020.eCanaliChiave.eAwsRecLwy)
          Dim chAwaRecNoLwy = Channels.Canale(clsChannels2020.eCanaliChiave.eAwaRecNoLwy)

          ReDim chAwaRecLwy.Valori(chTwaRecLwy.Valori.Count - 1)
          ReDim chAwsRecLwy.Valori(chTwaRecLwy.Valori.Count - 1)
          ReDim chAwaRecNoLwy.Valori(chTwaRecLwy.Valori.Count - 1)

          For i As Long = 0 To Canale.Valori.Count - 1
            Dim awa, aws As Double
            Dim Twa As Double = chTwaRecLwy.Valori(i)
            Dim Tws As Double = chTws.Valori(i)
            Dim Bs As Double = chBs.Valori(i)
            ApparentFromTrue(awa, aws, Twa, Tws, Bs)
            If Double.IsNaN(Twa) OrElse Double.IsNaN(Tws) OrElse Double.IsNaN(Bs) Then
              chAwaRecLwy.Valori(i) = Double.NaN
              chAwsRecLwy.Valori(i) = Double.NaN
              chAwaRecNoLwy.Valori(i) = Double.NaN
            Else
              chAwaRecLwy.Valori(i) = awa
              chAwsRecLwy.Valori(i) = aws
              Dim Lwy As Double = chLwy3s.Valori(i)
              If Double.IsNaN(Lwy) Then
                chAwaRecNoLwy.Valori(i) = Double.NaN
              Else
                If Twa = 0 Then
                  chAwaRecNoLwy.Valori(i) = awa - Lwy
                Else
                  Dim sign As Integer = Twa / Math.Abs(Twa)
                  chAwaRecNoLwy.Valori(i) = awa - (sign * Math.Min(15, Lwy))
                End If
              End If
            End If
          Next
        End If


      Case clsChannels2020.eCanaliChiave.eTwaRecLeeway
        Dim chTwa0L As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaNoLeeway)
        Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLeewayRecalc)
        If chLwy.Valori.Count <> chTwa0L.Valori.Count Then
          chLwy.Valori = Nothing
          chLwy = CanaleDbl(clsChannels2020.eCanaliChiave.eLeewayRecalc)
        End If
        ReDim Canale.Valori(chTwa0L.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwa0L.Valori(i)
          Dim Lwy As Double = chLwy.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Lwy) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = System.Math.Abs(Twa) + Lwy
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eHeadstayTotalLoad
        Dim chFsy As clsChannel2020 = CanaleDbl("Forestay")
        Dim chTack As clsChannel2020 = CanaleDbl("Tack")
        If Not chFsy Is Nothing AndAlso Not chTack Is Nothing Then
          ReDim Canale.Valori(chFsy.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim Fsy As Double = chFsy.Valori(i)
            Dim Tack As Double = chTack.Valori(i)
            If Double.IsNaN(Fsy) OrElse Double.IsNaN(Tack) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = Fsy + (Tack / 1000)
            End If
          Next
        End If


      Case clsChannels2020.eCanaliChiave.eVmgShearPerc, clsChannels2020.eCanaliChiave.eVmgShear
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chTwaTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chTwa)
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chSowTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chSow)
        Dim chVmgSh As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgShear)
        Dim chVmgShP As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgShearPerc)
        ReDim chVmgSh.Valori(chTwa.Valori.Count - 1)
        ReDim chVmgShP.Valori(chTwa.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwaTgt.Valori(i)
          Dim Sow As Double = chSow.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Sow) Then
            chVmgSh.Valori(i) = Double.NaN
            chVmgShP.Valori(i) = Double.NaN
          Else
            chVmgSh.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(Twa)))
            chVmgShP.Valori(i) = chVmgSh.Valori(i) / chSowTgt.Valori(i) * 100
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eVmgRecLwy
        Dim chTwaRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaRecLeeway)
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        ReDim Canale.Valori(chTwaRL.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwaRL.Valori(i)
          Dim Sow As Double = chSow.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Sow) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(Twa)))
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eVmgPercRecLwy
        If Not TgtManager.Tgt Is Nothing Then
          Dim chVmg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVmg)
          Dim chVmgRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVmgRecLwy)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chVmgRL.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim VmgRL As Double = chVmgRL.Valori(i)
            If Double.IsNaN(VmgRL) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = VmgRL / CanaleTgt.Valori(i) * 100
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eVmgInertial, clsChannels2020.eCanaliChiave.eVmgInertialRecLeeway
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertial)
        Dim chTwaRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway)
        'Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSowInertial)
        Dim chVmgInr As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgInertial)
        Dim chVmgInrRL As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgInertialRecLeeway)
        ReDim chVmgInr.Valori(chTwaRL.Valori.Count - 1)
        ReDim chVmgInrRL.Valori(chTwaRL.Valori.Count - 1)
        For i As Long = 0 To chTwa.Valori.Count - 1
          Dim Twa As Double = chTwa.Valori(i)
          Dim TwaRL As Double = chTwaRL.Valori(i)
          Dim Sow As Double = chSow.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(TwaRL) OrElse Double.IsNaN(Sow) Then
            chVmgInr.Valori(i) = Double.NaN
            chVmgInr.Valori(i) = Double.NaN
          Else
            chVmgInr.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(Twa)))
            chVmgInrRL.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(TwaRL)))
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eVmc50, clsChannels2020.eCanaliChiave.eVmc60, clsChannels2020.eCanaliChiave.eVmc70,
           clsChannels2020.eCanaliChiave.eVmc80, clsChannels2020.eCanaliChiave.eVmc90, clsChannels2020.eCanaliChiave.eVmc100,
           clsChannels2020.eCanaliChiave.eVmc110, clsChannels2020.eCanaliChiave.eVmc120, clsChannels2020.eCanaliChiave.eVmc130,
           clsChannels2020.eCanaliChiave.eVmc140, clsChannels2020.eCanaliChiave.eVmc150
        Dim VmcTwa = 0
        Dim chVmcInr As clsChannel2020
        Select Case Canale.CanaleChiave
          Case clsChannels2020.eCanaliChiave.eVmc40
            VmcTwa = 40
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc40)
          Case clsChannels2020.eCanaliChiave.eVmc50
            VmcTwa = 50
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc50)
          Case clsChannels2020.eCanaliChiave.eVmc60
            VmcTwa = 60
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc60)
          Case clsChannels2020.eCanaliChiave.eVmc70
            VmcTwa = 70
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc70)
          Case clsChannels2020.eCanaliChiave.eVmc80
            VmcTwa = 80
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc80)
          Case clsChannels2020.eCanaliChiave.eVmc90
            VmcTwa = 90
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc90)
          Case clsChannels2020.eCanaliChiave.eVmc100
            VmcTwa = 100
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc100)
          Case clsChannels2020.eCanaliChiave.eVmc110
            VmcTwa = 110
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc110)
          Case clsChannels2020.eCanaliChiave.eVmc120
            VmcTwa = 120
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc120)
          Case clsChannels2020.eCanaliChiave.eVmc130
            VmcTwa = 130
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc130)
          Case clsChannels2020.eCanaliChiave.eVmc140
            VmcTwa = 140
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc140)
          Case clsChannels2020.eCanaliChiave.eVmc150
            VmcTwa = 150
            chVmcInr = Channels.Canale(clsChannels2020.eCanaliChiave.eVmc150)
        End Select
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway)
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        ReDim chVmcInr.Valori(chTwa.Valori.Count - 1)
        For i As Long = 0 To chTwa.Valori.Count - 1
          Dim Twa As Double = chTwa.Valori(i)
          Dim Sow As Double = chSow.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Sow) Then
            chVmcInr.Valori(i) = Double.NaN
          Else
            Dim d = Math.Abs(Twa - VmcTwa)
            chVmcInr.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(d)))
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eVmgPercInertial, clsChannels2020.eCanaliChiave.eVmgPercInertialRecLeeway
        If Not TgtManager.Tgt Is Nothing Then
          Dim chVmgInr As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVmgInertial)
          Dim chVmgInrRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVmgInertialRecLeeway)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVmgInr)
          Dim chVmgPercInr As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgPercInertial)
          Dim chVmgPercInrRL As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eVmgPercInertialRecLeeway)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim chVmgPercInr.Valori(chVmgInr.Valori.Count - 1)
          ReDim chVmgPercInrRL.Valori(chVmgInr.Valori.Count - 1)
          For i As Long = 0 To chVmgInr.Valori.Count - 1
            Dim VmgInr As Double = chVmgInr.Valori(i)
            Dim VmgInrRL As Double = chVmgInr.Valori(i)
            If Double.IsNaN(VmgInr) Then
              chVmgPercInr.Valori(i) = Double.NaN
              chVmgPercInrRL.Valori(i) = Double.NaN
            Else
              chVmgPercInr.Valori(i) = VmgInr / CanaleTgt.Valori(i) * 100
              chVmgPercInrRL.Valori(i) = VmgInrRL / CanaleTgt.Valori(i) * 100
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eSailSet

        Dim chMS As clsChannel2020 = Channels.Canale("MainSail")
        Dim chHS As clsChannel2020 = Channels.Canale("HeadSail")
        Dim chPS As clsChannel2020 = Channels.Canale("ProdSail")
        Dim chSS As clsChannel2020 = Channels.Canale("StaySail")

        If Not chMS Is Nothing Then
          Dim c = chMS.Valori.Where(Function(x) Not Double.IsNaN(x)).ToList
          If c.Count = 0 Then
            Dim o As New clsSailUsage()
            o.UpdateSailUsageFromTextFile(False, False)
          End If
        End If


        If Not chMS Is Nothing AndAlso Not chHS Is Nothing AndAlso Not chPS Is Nothing AndAlso Not chSS Is Nothing Then

          Dim chSailSet As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSailSet)
          ReDim chSailSet.Valori(chMS.Valori.Count - 1)
          DataProvider2020.SailSet.Clear()

          For i As Long = 0 To chMS.Valori.Count - 1
            Dim ms As Double = chMS.Valori(i)
            Dim hs As Double = chHS.Valori(i)
            Dim ps As Double = chPS.Valori(i)
            Dim ss As Double = chSS.Valori(i)
            Dim SailSetName As String = ""
            'If Not Double.IsNaN(ms) Then
            '  Stop
            'End If
            If Not Double.IsNaN(ms) Then
              Dim ks = DataProvider2020.KnownSails.Where(Function(x) x.SailType = clsKnownSail.eSailType.eMainSail AndAlso x.SailCode = ms).FirstOrDefault
              If Not ks Is Nothing Then
                SailSetName = ks.AliasName
              End If
            End If
            If Not Double.IsNaN(hs) Then
              Dim ks = DataProvider2020.KnownSails.Where(Function(x) x.SailType = clsKnownSail.eSailType.eHeadSail AndAlso x.SailCode = hs).FirstOrDefault
              If Not ks Is Nothing Then
                SailSetName &= " " & ks.AliasName
              End If
            End If
            If Not Double.IsNaN(ss) Then
              Dim ks = DataProvider2020.KnownSails.Where(Function(x) x.SailType = clsKnownSail.eSailType.eStaySail AndAlso x.SailCode = ss).FirstOrDefault
              If Not ks Is Nothing Then
                SailSetName &= " " & ks.AliasName
              End If
            End If
            If Not Double.IsNaN(ps) Then
              Dim ks = DataProvider2020.KnownSails.Where(Function(x) x.SailType = clsKnownSail.eSailType.eProdSail AndAlso x.SailCode = ps).FirstOrDefault
              If Not ks Is Nothing Then
                SailSetName &= " " & ks.AliasName
              End If
            End If
            If SailSetName.Trim = "" Then
              chSailSet.Valori(i) = Double.NaN
            ElseIf DataProvider2020.SailSet.Where(Function(x) x = SailSetName).Count > 0 Then
              chSailSet.Valori(i) = DataProvider2020.SailSet.IndexOf(SailSetName)
            Else
              DataProvider2020.SailSet.Add(SailSetName)
              chSailSet.Valori(i) = DataProvider2020.SailSet.IndexOf(SailSetName)
            End If
          Next
        End If

      Case clsChannels2020.eCanaliChiave.eUpDnNetPress
        Dim chOutDn As clsChannel2020 = CanaleDbl("PLC_P100_Board_UpDw_OUT_Bar")
        Dim chInUp As clsChannel2020 = CanaleDbl("PLC_P102_Board_UpDw_IN_Bar")
        If Not chOutDn Is Nothing AndAlso Not chInUp Is Nothing Then
          If chOutDn.Valori.Count > 0 AndAlso chInUp.Valori.Count > 0 Then
            ReDim Canale.Valori(chOutDn.Valori.Count - 1)
            For i As Long = 0 To Canale.Valori.Count - 1
              Dim Up As Double = chInUp.Valori(i)
              Dim Dn As Double = chOutDn.Valori(i)
              If Double.IsNaN(Up) OrElse Double.IsNaN(Dn) Then
                Canale.Valori(i) = Double.NaN
              Else
                Canale.Valori(i) = Up - Dn
              End If
            Next
          End If
        End If
      Case clsChannels2020.eCanaliChiave.eMsLwy
        Dim objMsLwy As New clsMySongLeeway
        Dim chTwa0L As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaNoLeeway)
        Dim chHeelN As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
        Dim chBs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chTws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chSeaState As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chKeelAngle As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chDaggerImmersion As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim chDaggerAngle As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        ReDim Canale.Valori(chTwa0L.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwa0L.Valori(i)
          Dim Tws As Double = chTws.Valori(i)
          Dim HeelN As Double = chHeelN.Valori(i)
          Dim Bs As Double = chBs.Valori(i)
          Dim SeaState As Double = chSeaState.Valori(i)
          Dim DaggerImmersion As Double = chDaggerImmersion.Valori(i)
          Dim KeelAngle As Double = chKeelAngle.Valori(i)
          Dim DaggerAngle As Double = chDaggerAngle.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Tws) OrElse Double.IsNaN(HeelN) OrElse Double.IsNaN(Bs) OrElse Double.IsNaN(SeaState) OrElse Double.IsNaN(DaggerImmersion) OrElse Double.IsNaN(KeelAngle) OrElse Double.IsNaN(DaggerAngle) Then
            Canale.Valori(i) = Double.NaN
          Else
            Dim Lwy As Double = objMsLwy.CalculateNormalizedLeeway(HeelN, Bs, Twa, Tws, SeaState, KeelAngle, DaggerImmersion, DaggerAngle)
            Canale.Valori(i) = System.Math.Abs(Twa) + Lwy
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eMsTwa
        Dim chTwa0L As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaNoLeeway)
        Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eMsLwy)
        ReDim Canale.Valori(chTwa0L.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwa0L.Valori(i)
          Dim Lwy As Double = chLwy.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Lwy) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = System.Math.Abs(Twa) + Lwy
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eMsVmg
        Dim chTwaRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eMsTwa)
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        ReDim Canale.Valori(chTwaRL.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim Twa As Double = chTwaRL.Valori(i)
          Dim Sow As Double = chSow.Valori(i)
          If Double.IsNaN(Twa) OrElse Double.IsNaN(Sow) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = System.Math.Abs(Sow * Math.Cos(Radians(Twa)))
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eMsVmgPerc
        If Not TgtManager.Tgt Is Nothing Then
          Dim chVmg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
          Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVmg)
          Dim chVmgRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eMsVmg)
          If CanaleTgt Is Nothing Then Exit Sub
          ReDim Canale.Valori(chVmgRL.Valori.Count - 1)
          For i As Long = 0 To Canale.Valori.Count - 1
            Dim VmgRL As Double = chVmgRL.Valori(i)
            If Double.IsNaN(VmgRL) Then
              Canale.Valori(i) = Double.NaN
            Else
              Canale.Valori(i) = VmgRL / CanaleTgt.Valori(i) * 100
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eDaggerAoA
        Dim chDaggerAngle As clsChannel2020 = CanaleDbl("RudderFwd")
        Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
        If chDaggerAngle Is Nothing Then Exit Sub
        If chLwy Is Nothing Then Exit Sub
        ReDim Canale.Valori(chLwy.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim DA As Double = chDaggerAngle.Valori(i)
          Dim Lwy As Double = chLwy.Valori(i)
          If Double.IsNaN(DA) OrElse Double.IsNaN(Lwy) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = DA + Lwy
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eDaggerAoARL
        Dim chDaggerAngle As clsChannel2020 = CanaleDbl("RudderFwd")
        Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLeewayRecalc)
        If chDaggerAngle Is Nothing Then Exit Sub
        If chLwy Is Nothing Then Exit Sub
        ReDim Canale.Valori(chLwy.Valori.Count - 1)
        For i As Long = 0 To Canale.Valori.Count - 1
          Dim DA As Double = chDaggerAngle.Valori(i)
          Dim Lwy As Double = chLwy.Valori(i)
          If Double.IsNaN(DA) OrElse Double.IsNaN(Lwy) Then
            Canale.Valori(i) = Double.NaN
          Else
            Canale.Valori(i) = DA + Lwy
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eLeewayRecalc, clsChannels2020.eCanaliChiave.eLwyK, clsChannels2020.eCanaliChiave.eLwyK3s, clsChannels2020.eCanaliChiave.eLwyK30s, clsChannels2020.eCanaliChiave.eLwyRec3s, clsChannels2020.eCanaliChiave.eLwyRec30s

        'Dim mm As New clsMediaMobile(10, False)
        'Dim mm360 As New clsMediaMobile(10, True)
        'Dim testo As String = ""
        'For i As Integer = 0 To 15
        '  Dim v = 10 + i
        '  testo &= i & vbTab & v & vbTab & mm.SetAndGet(v).ToString("F1")
        '  testo &= vbTab
        '  Dim v360 As Double = (90 + (i * 10)) Mod 360
        '  testo &= v360 & vbTab & mm360.SetAndGet(v360).ToString("F1") & vbCrLf
        'Next
        'testo &= vbCrLf
        'testo &= vbCrLf
        'testo &= vbCrLf
        'mm.Azzera()
        'mm360.Azzera()
        'For i As Integer = 0 To 15
        '  Dim v = 10 + i
        '  testo &= i & vbTab & v & vbTab & mm.SetAndGet(v).ToString("F1")
        '  testo &= vbTab
        '  Dim v360 As Double = (90 + (i * 10)) Mod 360
        '  testo &= v360 & vbTab & mm360.SetAndGet(v360).ToString("F1") & vbCrLf
        'Next
        'Clipboard.SetText(testo)

        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chHeel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chHdg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
        Dim chCog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
        Dim chLeewayRecalc As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLeewayRecalc)
        Dim chLwyRec3s As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyRec3s)
        Dim chLwyRec30s As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyRec30s)
        Dim chLeewayK As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyK) ' il K di BnG ricalcolato secono la formula Lwy = K*H/Bs^2
        Dim chLwyK3s As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyK3s) ' il K di BnG ricalcolato secono la formula Lwy = K*H/Bs^2
        Dim chLwyK30s As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyK30s) ' il K di BnG ricalcolato secono la formula Lwy = K*H/Bs^2
        Dim MaxLeewayForRecalc As Double = AppConfig.ActiveProfile.MaxLeewayForRecalc
        If Not chHdg Is Nothing AndAlso Not chTwa Is Nothing AndAlso Not chCog Is Nothing Then
          ReDim chLeewayRecalc.Valori(chHdg.Valori.Count - 1)
          ReDim chLwyRec3s.Valori(chHdg.Valori.Count - 1)
          ReDim chLwyRec30s.Valori(chHdg.Valori.Count - 1)
          ReDim chLeewayK.Valori(chHdg.Valori.Count - 1)
          ReDim chLwyK3s.Valori(chHdg.Valori.Count - 1)
          ReDim chLwyK30s.Valori(chHdg.Valori.Count - 1)
          Dim isStbd As Boolean = True
          Dim wasStbd As Boolean = True
          Dim mmSow3s As New clsMediaMobile(3 * DataProvider2020.Hz, False)
          Dim mmSow30s As New clsMediaMobile(30 * DataProvider2020.Hz, False)
          Dim mmHeel3s As New clsMediaMobile(3 * DataProvider2020.Hz, False)
          Dim mmHeel30s As New clsMediaMobile(30 * DataProvider2020.Hz, False)
          Dim mmHdg3s As New clsMediaMobile(3 * DataProvider2020.Hz, True)
          Dim mmHdg30s As New clsMediaMobile(30 * DataProvider2020.Hz, True)
          Dim mmCog3s As New clsMediaMobile(3 * DataProvider2020.Hz, True)
          Dim mmCog30s As New clsMediaMobile(30 * DataProvider2020.Hz, True)

          For i As Integer = 0 To _TimeStamps.Count - 1
            Dim Twa As Double = chTwa.Valori(i)
            Dim Hdg As Double = chHdg.Valori(i)
            Dim Cog As Double = chCog.Valori(i)
            If Double.IsNaN(Twa) OrElse Double.IsNaN(Hdg) OrElse Double.IsNaN(Cog) Then
              chLeewayRecalc.Valori(i) = Double.NaN
              chLwyRec3s.Valori(i) = Double.NaN
              chLwyRec30s.Valori(i) = Double.NaN
              chLeewayK.Valori(i) = Double.NaN
              chLwyK3s.Valori(i) = Double.NaN
              chLwyK30s.Valori(i) = Double.NaN
            Else
              isStbd = Twa >= 0
              If Not isStbd = wasStbd Then
                mmSow3s.Azzera()
                mmSow30s.Azzera()
                mmHeel3s.Azzera()
                mmHeel30s.Azzera()
                mmHdg3s.Azzera()
                mmHdg30s.Azzera()
                mmCog3s.Azzera()
                mmCog30s.Azzera()
              End If
              If isStbd Then
                chLeewayRecalc.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Cog, Hdg)))
                chLwyRec3s.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(mmCog3s.SetAndGet(chCog.Valori(i)), mmHdg3s.SetAndGet(chHdg.Valori(i)))))
                chLwyRec30s.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(mmCog30s.SetAndGet(chCog.Valori(i)), mmHdg30s.SetAndGet(chHdg.Valori(i)))))
              Else
                chLeewayRecalc.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Hdg, Cog)))
                chLwyRec3s.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(mmHdg3s.SetAndGet(chHdg.Valori(i)), mmCog3s.SetAndGet(chCog.Valori(i)))))
                chLwyRec30s.Valori(i) = Math.Min(MaxLeewayForRecalc, Math.Max(-MaxLeewayForRecalc, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(mmHdg30s.SetAndGet(chHdg.Valori(i)), mmCog30s.SetAndGet(chCog.Valori(i)))))
              End If
              chLeewayK.Valori(i) = chLeewayRecalc.Valori(i) * Math.Pow(chSow.Valori(i), 2) / Math.Abs(chHeel.Valori(i))
              chLwyK3s.Valori(i) = chLwyRec3s.Valori(i) * Math.Pow(mmSow3s.SetAndGet(chSow.Valori(i)), 2) / Math.Abs(mmHeel3s.SetAndGet(chHeel.Valori(i)))
              chLwyK30s.Valori(i) = chLwyRec30s.Valori(i) * Math.Pow(mmSow30s.SetAndGet(chSow.Valori(i)), 2) / Math.Abs(mmHeel30s.SetAndGet(chHeel.Valori(i)))

            End If
            wasStbd = isStbd
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eLeewayModel
        ' leeway teorico: coefficienti nel profilo (AppConfig.ActiveProfile.LeewayModel), vedi LeewayModel.vb
        Canale.Valori = clsLeewayModel.CalcolaSerie(Me)
      Case clsChannels2020.eCanaliChiave.eSeaStateNorm, clsChannels2020.eCanaliChiave.eSeaStateNormDelta
        ' SeaState atteso dalla TWS (tabella nel profilo) e differenza rispetto al SeaState misurato
        Dim chTwsSS As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        If chTwsSS Is Nothing OrElse chTwsSS.Valori Is Nothing Then Exit Select
        Dim Norm() As Double = clsSeaStateNorm.CalcolaSerie(chTwsSS.Valori)
        If Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eSeaStateNorm Then
          Canale.Valori = Norm
        Else
          Dim chMis As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)
          If chMis Is Nothing OrElse chMis.Valori Is Nothing Then Exit Select
          Dim Delta(Norm.Length - 1) As Double
          For i As Integer = 0 To Norm.Length - 1
            If i < chMis.Valori.Length Then Delta(i) = chMis.Valori(i) - Norm(i) Else Delta(i) = Double.NaN
          Next
          Canale.Valori = Delta
        End If
      Case clsChannels2020.eCanaliChiave.eHeelDelta, clsChannels2020.eCanaliChiave.eTrimDelta, clsChannels2020.eCanaliChiave.eRudderDelta
        If Not TgtManager.Tgt Is Nothing Then
          Dim chVal As clsChannel2020 = Nothing
          Select Case Canale.CanaleChiave
            Case clsChannels2020.eCanaliChiave.eHeelDelta
              chVal = CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
            Case clsChannels2020.eCanaliChiave.eTrimDelta
              chVal = CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm)
            Case clsChannels2020.eCanaliChiave.eRudderDelta
              chVal = CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
          End Select
          If Not chVal Is Nothing Then
            Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVal)
            If CanaleTgt Is Nothing Then Exit Sub
            ReDim Canale.Valori(chVal.Valori.Count - 1)
            For i As Long = 0 To Canale.Valori.Count - 1
              Dim Valore As Double = chVal.Valori(i)
              If Double.IsNaN(Valore) Then
                Canale.Valori(i) = Double.NaN
              Else
                Canale.Valori(i) = Valore - CanaleTgt.Valori(i)
              End If
            Next
          End If
        End If
      Case clsChannels2020.eCanaliChiave.eSowSogDelta, clsChannels2020.eCanaliChiave.eSowSogK
        Dim chSow As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chSog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
        If Not chSow Is Nothing AndAlso Not chSog Is Nothing Then
          Dim chSogBsDelta As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSowSogDelta)
          Dim chSogBsK As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSowSogK)
          ReDim chSogBsDelta.Valori(chSow.Valori.Count - 1)
          ReDim chSogBsK.Valori(chSow.Valori.Count - 1)
          For i As Long = 0 To chSow.Valori.Count - 1
            Dim Valore As Double = chSow.Valori(i)
            If Double.IsNaN(Valore) Then
              chSogBsDelta.Valori(i) = Double.NaN
              chSogBsK.Valori(i) = Double.NaN
            Else
              chSogBsDelta.Valori(i) = Valore - chSog.Valori(i)
              chSogBsK.Valori(i) = Valore / chSog.Valori(i) * 100
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eTRIM 'se trim e' segnato come math channel viene ricalcolato dal pitch invertendo il segno
        Dim chTrim As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM)
        If chTrim Is Nothing Then
          chTrim = CanaleDbl("Trim")
        End If
        If Not chTrim Is Nothing Then
          chTrim.Valori = Nothing
          Canale.IsMath = False
          CaricaValoriParquet(chTrim)
          'chTrim = CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM)
          If AppConfig.ActiveProfile.ReverseTrim Then
            For i As Long = 0 To chTrim.Valori.Count - 1
              chTrim.Valori(i) = chTrim.Valori(i) * -1
            Next
            'Else
            '    chTrim = chTrim
          End If
          Canale.IsMath = True
        Else
          Dim chPitch As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.ePitch)
          If Not chPitch Is Nothing Then
            ReDim chTrim.Valori(chPitch.Valori.Count - 1)
            For i As Long = 0 To chPitch.Valori.Count - 1
              chTrim.Valori(i) = chPitch.Valori(i) * -1
            Next
          End If
        End If
      Case clsChannels2020.eCanaliChiave.eTrimNorm 'viene ricalcolato dal pitch invertendo il segno
        Dim chTrim As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM)
        If chTrim Is Nothing Then
          chTrim = CanaleDbl(clsChannels2020.eCanaliChiave.ePitch)
        End If
        If chTrim Is Nothing Then Exit Select
        If chTrim.Valori Is Nothing Then Exit Select
        If chTrim.Valori.Count = 0 Then Exit Select
        Dim chBoatTrim As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTrimNorm)
        ReDim chBoatTrim.Valori(chTrim.Valori.Count - 1)
        If AppConfig.ActiveProfile.ReverseTrim Then
          For i As Long = 0 To chTrim.Valori.Count - 1
            chBoatTrim.Valori(i) = chTrim.Valori(i) * -1
          Next
        Else
          For i As Long = 0 To chTrim.Valori.Count - 1
            chBoatTrim.Valori(i) = chTrim.Valori(i)
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eTravNorm
        Dim chTrav As clsChannel2020 = CanaleDbl("Trav")
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If chTwa Is Nothing Then Exit Select
        If chTrav Is Nothing Then Exit Select
        If chTrav.Valori Is Nothing Then Exit Select
        If chTrav.Valori.Count = 0 Then Exit Select
        Dim chTravNorm As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTravNorm)
        ReDim chTravNorm.Valori(chTrav.Valori.Count - 1)
        For i As Long = 0 To chTrav.Valori.Count - 1
          If chTwa.Valori(i) >= 0 Then
            chTravNorm.Valori(i) = chTrav.Valori(i) * -1
          Else
            chTravNorm.Valori(i) = chTrav.Valori(i)
          End If
        Next
      Case clsChannels2020.eCanaliChiave.ePRT 'converte da radianti in gradi
        Stop
        For i As Long = 0 To Channels.Canale(clsChannels2020.eCanaliChiave.ePRT).Valori.Count - 1
          Channels.Canale(clsChannels2020.eCanaliChiave.ePRT).Valori(i) = Channels.Canale(clsChannels2020.eCanaliChiave.ePRT).Valori(i) / System.Math.PI * 360
        Next
      Case clsChannels2020.eCanaliChiave.eYRT 'converte da radianti in gradi
        Stop
        For i As Long = 0 To Channels.Canale(clsChannels2020.eCanaliChiave.eYRT).Valori.Count - 1
          Channels.Canale(clsChannels2020.eCanaliChiave.eYRT).Valori(i) = Channels.Canale(clsChannels2020.eCanaliChiave.eYRT).Valori(i) / System.Math.PI * 360
        Next
      Case clsChannels2020.eCanaliChiave.eRRT 'converte da radianti in gradi
        Stop
        For i As Long = 0 To Channels.Canale(clsChannels2020.eCanaliChiave.eRRT).Valori.Count - 1
          Channels.Canale(clsChannels2020.eCanaliChiave.eRRT).Valori(i) = Channels.Canale(clsChannels2020.eCanaliChiave.eRRT).Valori(i) / System.Math.PI * 360
        Next
      Case clsChannels2020.eCanaliChiave.eIsStbd
        Dim PrevStbd As Boolean = True
        Dim chTwa As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTWA)
        Dim chIsStbd As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eIsStbd)
        ReDim chIsStbd.Valori(chTwa.Valori.Count - 1)
        For i As Integer = 0 To Channels.Canale(clsChannels2020.eCanaliChiave.eTWA).Valori.Count - 1
          Dim Valore = Channels.Canale(clsChannels2020.eCanaliChiave.eTWA).Valori(i)
          If Double.IsNaN(Valore) Then
            chIsStbd.Valori(i) = PrevStbd
          Else
            Dim IsStbd As Boolean = Valore > 0
            chIsStbd.Valori(i) = IsStbd
            PrevStbd = IsStbd
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eAbsTwa
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chTwaAbs As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAbsTwa)
        If Not chTwa Is Nothing Then
          ReDim chTwaAbs.Valori(chTwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            chTwaAbs.Valori(i) = Math.Abs(chTwa.Valori(i))
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eAbsAwa
        Dim chAwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
        Dim chAwaAbs As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAbsAwa)
        If Not chAwa Is Nothing Then
          ReDim chAwaAbs.Valori(chAwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            chAwaAbs.Valori(i) = Math.Abs(chAwa.Valori(i))
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eRdrNorm

        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chRdrAng As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eRdrAngle)
        'Dim chRdrAng As clsChannel2020 = CanaleDbl("Rudder")
        Dim chRdrN As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eRdrNorm)
        If Not AppConfig.ActiveProfile.RudderNormFromPortStbd AndAlso Not chRdrAng Is Nothing AndAlso Not chTwa Is Nothing Then
          ReDim chRdrN.Valori(chTwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            If (AppConfig.ActiveProfile.RudderIsAlreadyNormalized) Then
              If AppConfig.ActiveProfile.LogHasReversedRudder Then
                chRdrN.Valori(i) = -chRdrAng.Valori(i)
              Else
                chRdrN.Valori(i) = chRdrAng.Valori(i)
              End If
            Else
              Dim isStbd As Boolean = chTwa.Valori(i) >= 0
              If isStbd Then
                If AppConfig.ActiveProfile.LogHasReversedRudder Then
                  chRdrN.Valori(i) = -chRdrAng.Valori(i)
                Else
                  chRdrN.Valori(i) = chRdrAng.Valori(i)
                End If
              Else
                If AppConfig.ActiveProfile.LogHasReversedRudder Then
                  chRdrN.Valori(i) = chRdrAng.Valori(i)
                Else
                  chRdrN.Valori(i) = -chRdrAng.Valori(i)
                End If
              End If
            End If
          Next
        Else
          ReDim chRdrN.Valori(chTwa.Valori.Count - 1)
          Dim chRdrP = CanaleDbl("RudderP")
          Dim chRdrS = CanaleDbl("RudderS")
          If Not chRdrP Is Nothing AndAlso Not chRdrS Is Nothing Then
            For i As Integer = 0 To _TimeStamps.Count - 1
              If AppConfig.ActiveProfile.LogHasReversedRudder Then
                If chTwa.Valori(i) < 0 Then
                  chRdrN.Valori(i) = -chRdrS.Valori(i)
                Else
                  chRdrN.Valori(i) = -chRdrP.Valori(i)
                End If
              Else
                If chTwa.Valori(i) < 0 Then
                  chRdrN.Valori(i) = chRdrS.Valori(i)
                Else
                  chRdrN.Valori(i) = chRdrP.Valori(i)
                End If
              End If
            Next
            'Else
            'For i As Integer = 0 To _TimeStamps.Count - 1
            '  If (AppConfig.ActiveProfile.RudderIsAlreadyNormalized) Then
            '    If AppConfig.ActiveProfile.LogHasReversedRudder Then
            '      chRdrN.Valori(i) = -chRdrAng.Valori(i)
            '    Else
            '      chRdrN.Valori(i) = chRdrAng.Valori(i)
            '    End If
            '  Else
            '    Dim isStbd As Boolean = chTwa.Valori(i) >= 0
            '    If isStbd Then
            '      If AppConfig.ActiveProfile.LogHasReversedRudder Then
            '        chRdrN.Valori(i) = -chRdrAng.Valori(i)
            '      Else
            '        chRdrN.Valori(i) = chRdrAng.Valori(i)
            '      End If
            '    Else
            '      If AppConfig.ActiveProfile.LogHasReversedRudder Then
            '        chRdrN.Valori(i) = chRdrAng.Valori(i)
            '      Else
            '        chRdrN.Valori(i) = -chRdrAng.Valori(i)
            '      End If
            '    End If
            '  End If
            'Next
          End If
        End If
      Case clsChannels2020.eCanaliChiave.eLwyNorm
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLWY)
        Dim chLwyN As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eLwyNorm)
        If Not chTwa Is Nothing AndAlso Not chLwy Is Nothing Then
          ReDim chLwyN.Valori(chTwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            If AppConfig.ActiveProfile.ForceLeewayNormToAbsolute Then
              chLwyN.Valori(i) = Math.Abs(chLwy.Valori(i))
            Else
              Dim isStbd As Boolean = chTwa.Valori(i) >= 0
              If AppConfig.ActiveProfile.LogHasReversedLeeway Then ' Reversed significa leeway negativo mure a dritta
                If isStbd Then
                  chLwyN.Valori(i) = -chLwy.Valori(i) 'normalizzato significa avere leeway positivo quando la barca scarroccia sottovento
                Else
                  chLwyN.Valori(i) = chLwy.Valori(i)
                End If
              Else
                If isStbd Then
                  chLwyN.Valori(i) = chLwy.Valori(i)
                Else
                  chLwyN.Valori(i) = -chLwy.Valori(i)
                End If
              End If
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eHeelNorm
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chHeel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim RevHeel As Boolean = ReverseHeelSign()
        Dim chHeelN As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eHeelNorm)
        If Not chHeel Is Nothing AndAlso Not chTwa Is Nothing Then
          ReDim chHeelN.Valori(chTwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            Dim isStbd As Boolean = chTwa.Valori(i) >= 0
            If isStbd Then
              If RevHeel Then
                chHeelN.Valori(i) = -chHeel.Valori(i)
              Else
                chHeelN.Valori(i) = chHeel.Valori(i)
              End If
            Else
              If RevHeel Then
                chHeelN.Valori(i) = chHeel.Valori(i)
              Else
                chHeelN.Valori(i) = -chHeel.Valori(i)
              End If
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eMwaNorm
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chMwa As clsChannel2020 = CanaleDbl("Mwa")
        Dim chMwaN As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eMwaNorm)
        If Not chTwa Is Nothing AndAlso Not chMwa Is Nothing Then
          ReDim chMwaN.Valori(chTwa.Valori.Count - 1)
          For i As Integer = 0 To _TimeStamps.Count - 1
            Dim isStbd As Boolean = chTwa.Valori(i) >= 0
            If isStbd Then
              chMwaN.Valori(i) = chMwa.Valori(i)
            Else
              chMwaN.Valori(i) = -chMwa.Valori(i)
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.ePositionHeading
        Dim chLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
        Dim chLng = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
        Dim chPH = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.ePositionHeading)
        If Not chLat Is Nothing AndAlso Not chLng Is Nothing Then
          ReDim chPH.Valori(chLat.Valori.Count - 1)
          Dim P0 As New clsGeographicPosition(0, 0)
          Dim P1 As New clsGeographicPosition(0, 0)
          For i As Integer = 0 To chLat.Valori.Count - 2
            Dim lt0 = chLat.Valori(i)
            Dim ln0 = chLng.Valori(i)
            Dim lt1 = chLat.Valori(i + 1)
            Dim ln1 = chLng.Valori(i + 1)
            If Not Double.IsNaN(lt0) AndAlso Not Double.IsNaN(ln0) AndAlso Not Double.IsNaN(lt1) AndAlso Not Double.IsNaN(ln1) Then
              P0 = New clsGeographicPosition(lt0, ln0)
              P1 = New clsGeographicPosition(lt1, ln1)
              Dim h = clsGeoCalculations.BearingDegrees(P0, P1)
              chPH.Valori(i) = h
            Else
              chPH.Valori(i) = Double.NaN
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eSeaState
        Dim chBowRT As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.ePRT)
        Dim chSS As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSeaState)
        If chBowRT Is Nothing Then
          'chBowRT = CanaleDbl(clsChannels2020.eCanaliChiave.eTrimRT)
          chBowRT = New clsChannel2020() '
          chBowRT.ChannelId = "PRT"
          chBowRT.CanaleChiave = clsChannels2020.eCanaliChiave.ePRT
          chBowRT.ShortName = "PtcRt"
          chBowRT.LongName = "Pitch Rate"
          chBowRT.ShortUM = "°/S"
          chBowRT.LongUM = "Deg/Sec"
          chBowRT.DataType = clsChannel2020.eDataType.eLinear
          chBowRT.IsMath = True
          chBowRT.Decimals = 1
          Dim chTrim As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM)
          Dim v(DataProvider2020.TimeStamps.Count - 1) As Double
          chBowRT.Valori = v

          'Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pIsMath, pDecimals) ' , Me)
          If Not chTrim Is Nothing Then
            'Dim v(DataProvider2020.TimeStamps.Count - 1) As Double
            'chBowRT.Valori = v
            'For Each Val As Double In cprt.Valori
            '  Val = 0
            'Next
            Dim pt As DateTime = DataProvider2020.TimeStamps(0)
            Dim pv As Double = chTrim.Valori(0)
            chBowRT.Valori(0) = 0
            For i As Integer = 1 To chBowRT.Valori.Count - 1
              Dim ct As DateTime = DataProvider2020.TimeStamps(i)
              Dim cv As Double = chTrim.Valori(i)
              If (Double.IsNaN(cv)) Then
                cv = pv
              End If
              chBowRT.Valori(i) = (cv - pv) / (ct.Subtract(pt).TotalSeconds)
              pv = cv
              pt = ct
            Next

          Else
            For Each Val As Double In chBowRT.Valori
              Val = 0
            Next
          End If
          DataProvider2020.Channels.ListaCanali.Add(chBowRT)

          'Dim chPRT As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.ePRT)

          'DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.ePRT)

        End If
        If Not chBowRT Is Nothing Then

          ReDim chSS.Valori(chBowRT.Valori.Count - 1)
          Dim Samples As Integer = 30 * Hz
          Dim mm As New clsMediaMobile(Samples, False)
          For i As Integer = 0 To chBowRT.Valori.Count - 1
            chSS.Valori(i) = mm.SetAndGet(Math.Abs(chBowRT.Valori(i)))
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eSailingState
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chSailingState As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSailingState)
        ReDim chSailingState.Valori(chTwa.Valori.Count - 1)
        'Dim Valori As Double() = PeriodsManager.PeriodsFinder(DataProvider2020.TimeRange, 60, 6, 15, 25, 15, 25, 5, 10, 3, 10, 15)
        Dim Valori As Double() = PeriodsManager.CalculatesSailingState()
        chSailingState.Valori = Valori
      ' ============================================================================================
      ' VERSIONE PRECEDENTE DEL CALCOLO, conservata come riferimento storico.
      ' Basata su: somma non normalizzata delle SD pesate, riferimento al percentile 95 dell'intero
      ' file, DataQuality come media di Attitude, Environment e voto BSp sul target assoluto.
      ' Sostituita perche' il riferimento relativo rendeva i voti non confrontabili tra file e
      ' tra momenti diversi dello stesso file, e la SD del trim misurava il mare, non la condotta.
      ' --------------------------------------------------------------------------------------------
'      Case clsChannels2020.eCanaliChiave.eDataQuality, clsChannels2020.eCanaliChiave.eEnvironmentQuality, clsChannels2020.eCanaliChiave.eAttitudeQuality
'        'ValoriCaricati diversi per up e down
'        ' Definisco la SD standard di ogni canale
'        ' creo un canale che vale 100 meno somma delle standard deviation dei canali presi in considerazione
'        ' Vmg SD
'        ' Tws SD 
'        ' Twa SD 
'        ' Trim SD
'        ' Hdg SD
'        ' Bs SD
'        ' Heeling SD
'        ' se la vmgp , lo yawrate e il delta heel sono fuori dai limiti dimezza il data quality value calcolato con le deviazioni standard
'
'        'Stop
'
'
'        If AppConfig.ActiveProfile.DataQualitySettings Is Nothing Then
'          AppConfig.ActiveProfile.DataQualitySettings = New clsQualitySettings
'          AppConfig.ActiveProfile.DataQualitySettings.CaricaValoriDefault()
'          AppConfig.Salva()
'        Else
'          AppConfig.ActiveProfile.DataQualitySettings.VerificaCanaliEsistenti()
'        End If
'
'        Dim samples As Integer = AppConfig.ActiveProfile.DataQualitySettings.SecondiSampling
'        Dim lh As clsChannel2020 = CanaleDbl("LogHz")
'        If Not lh Is Nothing Then
'          Dim hz = lh.Valori.Where(Function(x) Not Double.IsNaN(x)).Average
'          samples = CInt(hz) * AppConfig.ActiveProfile.DataQualitySettings.SecondiSampling
'        End If
'
'        Dim chTwa As New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eTWA), 2, 2, 3, samples, True, False, False, False)
'
'        Dim chVmgP As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
'        Dim MinVmgP As Integer = AppConfig.ActiveProfile.DataQualitySettings.MinVmgPerc
'        Dim chYawRate As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eYRT) ' CanaleDbl("ROT")
'        If chYawRate Is Nothing Then
'
'          Dim c As New clsChannel2020()
'          c.ChannelId = "YawRateFromHeading"
'          c.ActualLogHeader = c.ChannelId
'          c.DataType = clsChannel2020.eDataType.eLinear
'          c.CanaleChiave = clsChannels2020.eCanaliChiave.eYRT
'          c.Decimals = 1
'          c.IsMath = True
'          c.Export = False
'          c.LongName = "YawRateFromHeading"
'          c.LongUM = "Deg/Sec"
'          c.PolarHeader = ""
'          c.BenchmarkHeader = ""
'          c.ShortName = "YRT"
'          c.ShortUM = "d/s"
'          ReDim c.Valori(_TimeStamps.Count - 1)
'          Channels.ListaCanali.Add(c)
'
'        End If
'
'
'        If Not chYawRate.HasNotNanValuesOrZero Then ' verifica se il campo ROT ha valori validi, se non li ha crea lo yaw dalla variazione di heading
'          Dim chHdg = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
'          Dim t1 As DateTime = _TimeStamps(0)
'          Dim v1 As Double = chHdg.Valori(0)
'          For i As Integer = 1 To _TimeStamps.Count - 1
'            Dim t2 As DateTime = _TimeStamps(i)
'            Dim v2 As Double = chHdg.Valori(i)
'            chYawRate.Valori(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2.Subtract(t1).TotalSeconds)
'            t1 = t2
'            v1 = v2
'          Next
'        End If
'
'
'        'Dim MaxYawRate As Double = 4
'        Dim chHeelTgtDelta As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHeelDelta)
'        'Dim MaxHeelTgtDelta As Double = 4
'        Dim chBSpp As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
'
'        ' il coefficiente serve a bilanciare il peso delle variazioni se ad esempio in termini di instabilita', in bolina, 3 % di vmg equivale a 5 gradi di twd i coefficienti saranno 3 e 5  
'        ' attenzione se il numero 'e in valore reale o percentuale
'        Dim Lista As New List(Of clsChDataQuality)
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp), 2, 2, 3, samples, True, False, False, True)) '*
'        'Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp), 2,2, 3, samples, True, False, False, True)) '*
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eHDG), 1.5, 1.5, 2, samples, True, False, True, False))
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eSOW), 3, 3, 4, samples, True, False, False, True)) '*
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eTWS), 3, 3, 2, samples, True, True, False, True))
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL), 2, 2, 2, samples, True, False, True, False))
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eTrimNorm), 0.2, 0.2, 0.3, samples, True, False, True, False))
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eTWD), 3, 3, 3, samples, True, True, False, False))
'        Lista.Add(New clsChDataQuality(CanaleDbl(clsChannels2020.eCanaliChiave.eAWA), 1.5, 1.5, 1.5, samples, True, False, True, False))
'
'        ' canali environment = TWS, TWD
'        ' canali attitude = HDG, SOW, HEEL, TRIM, AWA
'        ' canale filtro quality BSPP non piu' VMGp
'
'        ' avendo messo il valore reale del delta equivalente qui rimodula i coefficienti in modo che quando la sd di quel canale equivale alla variazione ammissibile la % tolta dalla qualita' totale sia 1
'        'implementata nell istanza
'        'chTwa.UpwindK = 1 / chTwa.UpwindK
'        'chTwa.DownwindK = 1 / chTwa.DownwindK
'        'For Each L In Lista
'        '  L.UpwindK = 1 / L.UpwindK
'        '  L.DownwindK = 1 / L.DownwindK
'        'Next
'
'
'        Dim ListaR As New List(Of clsChDataQuality)
'
'        For Each c As clsChDataQuality In Lista
'          If c.Channel Is Nothing Then
'            ListaR.Add(c)
'          End If
'        Next
'        For Each c As clsChDataQuality In ListaR
'          Lista.Remove(c)
'        Next
'
'
'        Dim chDataQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eDataQuality)
'        Dim chEnvQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
'        Dim chAttQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAttitudeQuality)
'        Dim idx As Integer = 0
'
'        ReDim chDataQ.Valori(chTwa.Channel.Valori.Count - 1)
'        ReDim chEnvQ.Valori(chTwa.Channel.Valori.Count - 1)
'        ReDim chAttQ.Valori(chTwa.Channel.Valori.Count - 1)
'        Dim DataQtmp(chTwa.Channel.Valori.Count - 1) As Double
'        Dim EnvQtmp(chTwa.Channel.Valori.Count - 1) As Double
'        Dim AttQtmp(chTwa.Channel.Valori.Count - 1) As Double
'
'        Dim ND As Integer = Lista.Where(Function(x) x.DataQuality = True).Count
'        Dim NA As Integer = Lista.Where(Function(x) x.AttitudeQuality = True).Count
'        Dim NE As Integer = Lista.Where(Function(x) x.EnvironmentQuality = True).Count
'
'        Dim vTmp As Double = 0
'        Dim eTmp As Double = 0
'        Dim aTmp As Double = 0
'
'        Dim maxvmgpk, maxheelk, maxyawk As Double
'
'        For i As Integer = 0 To _TimeStamps.Count - 1
'          'If Momento(i).Year = 2019 Then Stop
'          'If Momento(i) > New DateTime(2021, 7, 16, 14, 31, 35) AndAlso Momento(i) < New DateTime(2021, 7, 16, 14, 31, 50) Then Stop
'          Dim Twa As Double = chTwa.Channel.Valori(i)
'          If Not Double.IsNaN(Twa) Then
'            Try
'
'              Dim Andatura As clsChDataQuality.eAndatura = clsChDataQuality.eAndatura.eUpwind
'              If Math.Abs(Twa) > 120 Then
'                Andatura = clsChDataQuality.eAndatura.eDownwind
'              ElseIf Math.Abs(Twa) > 65 Then
'                Andatura = clsChDataQuality.eAndatura.eReaching
'              End If
'              chTwa.Values(idx) = Twa
'              vTmp = chTwa.WeightedSD(Andatura) ' chTwa.SD * chTwa.Coeff(isUp)
'              aTmp = 0
'              eTmp = 0
'              For Each c As clsChDataQuality In Lista
'                Dim v As Double = c.Channel.Valori(i)
'                If Not Double.IsNaN(v) Then
'                  c.Values(idx) = v
'                  If c.DataQuality Then
'                    vTmp += c.WeightedSD(Andatura) ' .SD * c.Coeff(isUp)
'                  End If
'                  If c.EnvironmentQuality Then
'                    eTmp += c.WeightedSD(Andatura) ' .SD * c.Coeff(isUp)
'                  End If
'                  If c.AttitudeQuality Then
'                    aTmp += c.WeightedSD(Andatura) '.SD * c.Coeff(isUp)
'                  End If
'                End If
'              Next
'              'If (vTmp / ND) > 20 Then Stop
'              idx += 1
'              If idx >= samples Then idx = 0
'              DataQtmp(i) = Math.Max(0, 100 - vTmp) ' (vTmp / ND))
'              EnvQtmp(i) = Math.Max(0, 100 - eTmp) '  (eTmp / NE))
'              AttQtmp(i) = Math.Max(0, 100 - aTmp) '  (aTmp / NA))
'
'
'              Dim dAbs As Double
'              Dim K As Double
'              If Not chVmgP Is Nothing Then
'                'K = Math.Max(0.5, chVmgP.Valori(i) / MinVmgP) ' 30/90 = 0.333 =>0.5  90/90=1 =>1 180/90=2 =>1 
'                K = Math.Max(AppConfig.ActiveProfile.DataQualitySettings.MinCoeffK, chVmgP.Valori(i) / MinVmgP) ' 30/90 = 0.333 =>0.7  90/90=1 =>1 180/90=2 =>1 
'                K = Math.Min(1, K)
'                'If K > 1 Then Stop
'                DataQtmp(i) = DataQtmp(i) * K
'                maxvmgpk = Math.Max(maxvmgpk, K)
'              End If
'              If Not chYawRate Is Nothing Then
'                dAbs = Math.Abs(chYawRate.Valori(i))
'                If Not Double.IsNaN(dAbs) Then
'                  K = Math.Max(AppConfig.ActiveProfile.DataQualitySettings.MinCoeffK, AppConfig.ActiveProfile.DataQualitySettings.MaxYawRate / dAbs) ' 3/6 = 0.5 =>0.7  3/3=1 =>1 3/1=3 =>1
'                  K = Math.Min(1, K)
'                  'If K > 1 Then Stop
'                  DataQtmp(i) = DataQtmp(i) * K
'                  maxyawk = Math.Max(maxyawk, K)
'                End If
'              End If
'              If Not chHeelTgtDelta Is Nothing Then
'                dAbs = Math.Abs(chHeelTgtDelta.Valori(i))
'                If Not Double.IsNaN(dAbs) Then
'                  K = Math.Max(AppConfig.ActiveProfile.DataQualitySettings.MinCoeffK, AppConfig.ActiveProfile.DataQualitySettings.MaxHeelTgtDelta / dAbs) ' 3/6 = 0.5 =>0.7  3/3=1 =>1 3/1=3 =>1
'                  K = Math.Min(1, K)
'                  'If K > 1 Then Stop
'                  DataQtmp(i) = DataQtmp(i) * K
'                  maxheelk = Math.Max(maxheelk, K)
'                End If
'              End If
'            Catch ex As Exception
'              Stop
'            End Try
'          Else
'            DataQtmp(i) = Double.NaN
'            EnvQtmp(i) = Double.NaN
'            AttQtmp(i) = Double.NaN
'          End If
'        Next
'        ' l'array temporaneo 'e pieno, adesso assegna il punteggio da 0 a 5
'        ' il valoe topscorer 'e quello del percentile 95
'        ' i singoli valori prendono un valore percentuale rispetto al topscorer
'        Dim TopScorer = MathNet.Numerics.Statistics.Statistics.Percentile(DataQtmp.Where(Function(x) Not Double.IsNaN(x)), 95)
'        Dim TopScorerE = MathNet.Numerics.Statistics.Statistics.Percentile(EnvQtmp.Where(Function(x) Not Double.IsNaN(x)), 95)
'        Dim TopScorerA = MathNet.Numerics.Statistics.Statistics.Percentile(AttQtmp.Where(Function(x) Not Double.IsNaN(x)), 95)
'        'TopScorer = 100
'        For i As Integer = 0 To DataQtmp.Count - 1
'          Dim v As Double = DataQtmp(i)
'          Dim e As Double = EnvQtmp(i)
'          Dim a As Double = AttQtmp(i)
'
'          If Double.IsNaN(e) Then
'            chEnvQ.Valori(i) = Double.NaN
'          Else
'            Dim eP As Double = e / TopScorerE * 100
'            If eP > AppConfig.ActiveProfile.DataQualitySettings.MinPercEnvQ5 Then
'              chEnvQ.Valori(i) = 5
'            ElseIf eP > AppConfig.ActiveProfile.DataQualitySettings.MinPercEnvQ4 Then
'              chEnvQ.Valori(i) = 4
'            ElseIf eP > AppConfig.ActiveProfile.DataQualitySettings.MinPercEnvQ3 Then
'              chEnvQ.Valori(i) = 3
'            ElseIf eP > AppConfig.ActiveProfile.DataQualitySettings.MinPercEnvQ2 Then
'              chEnvQ.Valori(i) = 2
'            ElseIf eP > AppConfig.ActiveProfile.DataQualitySettings.MinPercEnvQ1 Then
'              chEnvQ.Valori(i) = 1
'            Else
'              chEnvQ.Valori(i) = 0
'            End If
'          End If
'
'
'          If Double.IsNaN(a) Then
'            chAttQ.Valori(i) = Double.NaN
'          Else
'            Dim aP As Double = a / TopScorerA * 100
'            If aP > AppConfig.ActiveProfile.DataQualitySettings.MinPercAttQ5 Then
'              chAttQ.Valori(i) = 5
'            ElseIf aP > AppConfig.ActiveProfile.DataQualitySettings.MinPercAttQ4 Then
'              chAttQ.Valori(i) = 4
'            ElseIf aP > AppConfig.ActiveProfile.DataQualitySettings.MinPercAttQ3 Then
'              chAttQ.Valori(i) = 3
'            ElseIf aP > AppConfig.ActiveProfile.DataQualitySettings.MinPercAttQ2 Then
'              chAttQ.Valori(i) = 2
'            ElseIf aP > AppConfig.ActiveProfile.DataQualitySettings.MinPercAttQ1 Then
'              chAttQ.Valori(i) = 1
'            Else
'              chAttQ.Valori(i) = 0
'            End If
'          End If
'
'          If Double.IsNaN(e) OrElse Double.IsNaN(a) Then
'            chDataQ.Valori(i) = Double.NaN
'          Else
'            Dim Qtmp = (chAttQ.Valori(i) + chEnvQ.Valori(i)) / 2
'            If Not chBSpp Is Nothing Then
'              Dim BspK As Double = chBSpp.Valori(i)
'              If Not Double.IsNaN(BspK) Then
'                Dim QI As Integer = 0
'                If BspK > AppConfig.ActiveProfile.DataQualitySettings.MinPercPerf5 Then
'                  QI = 5
'                ElseIf BspK > AppConfig.ActiveProfile.DataQualitySettings.MinPercPerf4 Then
'                  QI = 4
'                ElseIf BspK > AppConfig.ActiveProfile.DataQualitySettings.MinPercPerf3 Then
'                  QI = 3
'                ElseIf BspK > AppConfig.ActiveProfile.DataQualitySettings.MinPercPerf2 Then
'                  QI = 2
'                ElseIf BspK > AppConfig.ActiveProfile.DataQualitySettings.MinPercPerf1 Then
'                  QI = 1
'                Else
'                  QI = 0
'                End If
'                chDataQ.Valori(i) = (chAttQ.Valori(i) + chEnvQ.Valori(i) + QI) / 3
'              Else
'                chDataQ.Valori(i) = (chAttQ.Valori(i) + chEnvQ.Valori(i)) / 2
'              End If
'            Else
'              chDataQ.Valori(i) = (chAttQ.Valori(i) + chEnvQ.Valori(i)) / 2
'            End If
'          End If
'        Next
'        'Stop
'
'
      ' ============================================================================================

      Case clsChannels2020.eCanaliChiave.eDataQuality, clsChannels2020.eCanaliChiave.eEnvironmentQuality, clsChannels2020.eCanaliChiave.eAttitudeQuality

        ' I tre indici sono indipendenti e rispondono a domande diverse:
        '   EnvironmentQuality : il vento era stabile?           -> il dato e' riproducibile
        '   AttitudeQuality    : la barca era condotta stabile?  -> il dato e' riproducibile
        '   DataQuality        : andavamo forte?                 -> il dato e' buono (performance pura)
        ' I primi due sono misure ASSOLUTE di instabilita': per ogni canale si rapporta la deviazione
        ' standard nella finestra mobile alla variazione tollerata, e si fa la media sui canali
        ' presenti. 1 significa "in media ogni canale ha oscillato esattamente quanto ammesso".
        ' Il terzo confronta BSp con il rendimento tipico del suo intorno temporale, a parita' di
        ' andatura e sui soli campioni gia' giudicati validi dai primi due indici.

        If AppConfig.ActiveProfile.DataQualitySettings Is Nothing Then
          AppConfig.ActiveProfile.DataQualitySettings = New clsQualitySettings
          AppConfig.ActiveProfile.DataQualitySettings.CaricaValoriDefault()
          AppConfig.Salva()
        End If
        Dim QS As clsQualitySettings = AppConfig.ActiveProfile.DataQualitySettings
        QS.NormalizzaValoriMancanti()

        ' ampiezza della finestra mobile: SecondiSampling secondi reali, non righe di log
        Dim HzMedio As Double = 1
        Dim samples As Integer = QS.SecondiSampling
        Dim lh As clsChannel2020 = CanaleDbl("LogHz")
        If Not lh Is Nothing Then
          HzMedio = lh.Valori.Where(Function(x) Not Double.IsNaN(x)).Average
          If HzMedio <= 0 Then HzMedio = 1
          samples = CInt(HzMedio) * QS.SecondiSampling
        End If
        If samples < 2 Then samples = 2

        ' il TWA serve a stabilire l'andatura, quindi quale dei tre delta applicare
        Dim chTwaVal As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        If chTwaVal Is Nothing Then
          ' senza TWA non e' possibile determinare l'andatura: i tre canali restano non calcolati
          Exit Select
        End If

        ' stato del mare: media mobile a 30 secondi del valore assoluto del pitch rate, in deg/s.
        ' CanaleDbl lo calcola qui se non e' ancora stato generato.
        Dim chSeaState As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)

        ' NB: il canale YawRate non entra piu' nel calcolo della qualita', ma questo blocco lo crea
        ' e lo deriva dall'heading quando manca o e' vuoto. E' un canale usato altrove,
        ' quindi va mantenuto anche se qui non serve.
        Dim chYawRate As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eYRT) ' CanaleDbl("ROT")
        If chYawRate Is Nothing Then

          Dim c As New clsChannel2020()
          c.ChannelId = "YawRateFromHeading"
          c.ActualLogHeader = c.ChannelId
          c.DataType = clsChannel2020.eDataType.eLinear
          c.CanaleChiave = clsChannels2020.eCanaliChiave.eYRT
          c.Decimals = 1
          c.IsMath = True
          c.Export = False
          c.LongName = "YawRateFromHeading"
          c.LongUM = "Deg/Sec"
          c.PolarHeader = ""
          c.BenchmarkHeader = ""
          c.ShortName = "YRT"
          c.ShortUM = "d/s"
          ReDim c.Valori(_TimeStamps.Count - 1)
          Channels.ListaCanali.Add(c)

        End If

        If Not chYawRate.HasNotNanValuesOrZero Then ' se ROT non ha valori validi crea lo yaw dalla variazione di heading
          Dim chHdg = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
          Dim t1 As DateTime = _TimeStamps(0)
          Dim v1 As Double = chHdg.Valori(0)
          For i As Integer = 1 To _TimeStamps.Count - 1
            Dim t2 As DateTime = _TimeStamps(i)
            Dim v2 As Double = chHdg.Valori(i)
            chYawRate.Valori(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2.Subtract(t1).TotalSeconds)
            t1 = t2
            v1 = v2
          Next
        End If

        ' velocita' in percentuale sul target polare: e' la materia prima del DataQuality
        Dim chBSpp As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)

        ' i canali e la loro appartenenza sono fissati in clsQualitySettings.CanaliStandard,
        ' dal profilo arrivano solo i delta tarati. I canali assenti dal file vengono scartati.
        Dim Lista As List(Of clsChDataQuality) = QS.CostruisciListaRuntime(samples, Function(k) CanaleDbl(k))
        Dim ListaEnv As List(Of clsChDataQuality) = Lista.Where(Function(x) x.Indice = clsQualityChannelSetting.eIndiceQualita.eEnvironment).ToList
        Dim ListaAtt As List(Of clsChDataQuality) = Lista.Where(Function(x) x.Indice = clsQualityChannelSetting.eIndiceQualita.eAttitude).ToList

        Dim chDataQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eDataQuality)
        Dim chEnvQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
        Dim chAttQ As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAttitudeQuality)

        Dim N As Integer = chTwaVal.Valori.Count
        ReDim chDataQ.Valori(N - 1)
        ReDim chEnvQ.Valori(N - 1)
        ReDim chAttQ.Valori(N - 1)

        ' andatura di ogni riga, riusata nella seconda passata per il riferimento di performance
        Dim Andature(N - 1) As clsChDataQuality.eAndatura
        Dim AndaturaValida(N - 1) As Boolean

        Dim idx As Integer = 0
        Dim SwDq As Stopwatch = Stopwatch.StartNew()
        clsLogTempi.Scrivi("  DataQuality: preparazione finita, inizio passata 1 (" & Lista.Count & " canali, finestra " & samples & " righe)")

        ' ---------------------------------------------------------------------------------
        ' PASSATA 1: instabilita' assoluta di Environment e Attitude
        ' ---------------------------------------------------------------------------------
        For i As Integer = 0 To N - 1
          Dim Twa As Double = chTwaVal.Valori(i)
          If Double.IsNaN(Twa) Then
            chEnvQ.Valori(i) = Double.NaN
            chAttQ.Valori(i) = Double.NaN
            AndaturaValida(i) = False
            Continue For
          End If

          Try
            Dim Andatura As clsChDataQuality.eAndatura = clsChDataQuality.eAndatura.eUpwind
            If Math.Abs(Twa) > 120 Then
              Andatura = clsChDataQuality.eAndatura.eDownwind
            ElseIf Math.Abs(Twa) > 65 Then
              Andatura = clsChDataQuality.eAndatura.eReaching
            End If
            Andature(i) = Andatura
            AndaturaValida(i) = True

            Dim SeaState As Double = Double.NaN
            If Not chSeaState Is Nothing Then SeaState = chSeaState.Valori(i)

            ' aggiorna le finestre mobili
            For Each c As clsChDataQuality In Lista
              Dim v As Double = c.Channel.Valori(i)
              If Not Double.IsNaN(v) Then c.Accumula(idx, v)
            Next
            idx += 1
            If idx >= samples Then idx = 0

            ' media delle tolleranze usate sui soli canali che hanno prodotto un valore
            Dim SommaE As Double = 0
            Dim NE As Integer = 0
            For Each c As clsChDataQuality In ListaEnv
              Dim ins As Double = c.TolleranzaUsata(Andatura, SeaState)
              If Not Double.IsNaN(ins) Then
                SommaE += ins
                NE += 1
              End If
            Next

            Dim SommaA As Double = 0
            Dim NA As Integer = 0
            For Each c As clsChDataQuality In ListaAtt
              Dim ins As Double = c.TolleranzaUsata(Andatura, SeaState)
              If Not Double.IsNaN(ins) Then
                SommaA += ins
                NA += 1
              End If
            Next

            If NE > 0 Then
              chEnvQ.Valori(i) = QS.VotoEnvironment(SommaE / NE)
            Else
              chEnvQ.Valori(i) = Double.NaN
            End If

            If NA > 0 Then
              chAttQ.Valori(i) = QS.VotoAttitude(SommaA / NA)
            Else
              chAttQ.Valori(i) = Double.NaN
            End If

          Catch ex As Exception
            chEnvQ.Valori(i) = Double.NaN
            chAttQ.Valori(i) = Double.NaN
            AndaturaValida(i) = False
          End Try
        Next

        ' ---------------------------------------------------------------------------------
        ' PASSATA 2: performance relativa al rendimento tipico dell'intorno
        clsLogTempi.Scrivi("  DataQuality: passata 1 finita in " & SwDq.ElapsedMilliseconds & " ms, inizio passata 2" &
                           " (verifica SD veloce vs classica: " & clsChDataQuality.VerificaSdControlli & " controlli, differenza massima " & clsChDataQuality.VerificaSdDiffMax.ToString("E2") & ")")
        ' Il riferimento e' la mediana di BSp calcolata a blocchi di un minuto sull'intorno di
        ' +/- RefWindowMinutes, separatamente per andatura e sui soli campioni gia' giudicati
        ' validi dai due indici di stabilita'. Tra i centri dei blocchi si interpola.
        ' ---------------------------------------------------------------------------------
        If chBSpp Is Nothing Then
          For i As Integer = 0 To N - 1
            chDataQ.Valori(i) = Double.NaN
          Next
        Else
          Dim RighePerBlocco As Integer = CInt(Math.Max(1, HzMedio * 60))
          Dim BloccoPerLato As Integer = CInt(Math.Max(1, Math.Round(QS.RefWindowMinutes)))
          Dim NumBlocchi As Integer = CInt(Math.Ceiling(N / RighePerBlocco))

          ' un riferimento per blocco e per andatura
          Dim Riferimento(NumBlocchi - 1, 2) As Double
          For b As Integer = 0 To NumBlocchi - 1
            For a As Integer = 0 To 2
              Riferimento(b, a) = Double.NaN
            Next
          Next

          For b As Integer = 0 To NumBlocchi - 1
            Dim Da As Integer = Math.Max(0, (b - BloccoPerLato) * RighePerBlocco)
            Dim Fino As Integer = Math.Min(N - 1, (b + BloccoPerLato + 1) * RighePerBlocco - 1)

            Dim Campioni(2) As List(Of Double)
            For a As Integer = 0 To 2
              Campioni(a) = New List(Of Double)
            Next

            For i As Integer = Da To Fino
              If Not AndaturaValida(i) Then Continue For
              Dim bs As Double = chBSpp.Valori(i)
              If Double.IsNaN(bs) Then Continue For
              Dim eq As Double = chEnvQ.Valori(i)
              Dim aq As Double = chAttQ.Valori(i)
              If Double.IsNaN(eq) OrElse Double.IsNaN(aq) Then Continue For
              If eq < QS.MinEnvQPerRiferimento OrElse aq < QS.MinAttQPerRiferimento Then Continue For
              Campioni(CInt(Andature(i))).Add(bs)
            Next

            For a As Integer = 0 To 2
              ' sotto una decina di campioni la mediana non e' significativa
              If Campioni(a).Count >= 10 Then
                Riferimento(b, a) = MathNet.Numerics.Statistics.Statistics.Median(Campioni(a))
              End If
            Next
          Next

          For i As Integer = 0 To N - 1
            If Not AndaturaValida(i) Then
              chDataQ.Valori(i) = Double.NaN
              Continue For
            End If

            Dim bs As Double = chBSpp.Valori(i)
            If Double.IsNaN(bs) Then
              chDataQ.Valori(i) = Double.NaN
              Continue For
            End If

            Dim a As Integer = CInt(Andature(i))

            ' interpolazione lineare tra i centri dei due blocchi adiacenti
            Dim Pos As Double = (i / RighePerBlocco) - 0.5
            Dim b1 As Integer = CInt(Math.Floor(Pos))
            Dim b2 As Integer = b1 + 1
            Dim Frazione As Double = Pos - b1
            If b1 < 0 Then
              b1 = 0
              b2 = 0
              Frazione = 0
            End If
            If b2 > NumBlocchi - 1 Then
              b2 = NumBlocchi - 1
              If b1 > NumBlocchi - 1 Then b1 = NumBlocchi - 1
            End If

            Dim r1 As Double = Riferimento(b1, a)
            Dim r2 As Double = Riferimento(b2, a)
            Dim Rif As Double
            If Double.IsNaN(r1) AndAlso Double.IsNaN(r2) Then
              Rif = Double.NaN
            ElseIf Double.IsNaN(r1) Then
              Rif = r2
            ElseIf Double.IsNaN(r2) Then
              Rif = r1
            Else
              Rif = r1 + (r2 - r1) * Frazione
            End If

            If Double.IsNaN(Rif) Then
              ' nessun riferimento affidabile nell'intorno: nessun giudizio di performance
              chDataQ.Valori(i) = Double.NaN
            Else
              chDataQ.Valori(i) = QS.VotoPerformance(bs - Rif)
            End If
          Next
        End If

      Case clsChannels2020.eCanaliChiave.eRudderAoA, clsChannels2020.eCanaliChiave.eRdrRecLwyAoA
        'Stop
        Dim chBs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim chTwa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim chRdrAngNorm As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
        Dim chLwyNorm As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
        Dim chRecLwy As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eLeewayRecalc)
        Dim chYawRate As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)

        Dim ApplyYaw As Boolean = Not chYawRate Is Nothing AndAlso Not chBs Is Nothing AndAlso Not chTwa Is Nothing

        Dim chRdrAoA As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eRudderAoA)
        Dim chRdrRecLwyAoA As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eRdrRecLwyAoA)
        If Not chRdrAngNorm Is Nothing AndAlso Not chRdrAoA Is Nothing Then
          ReDim chRdrAoA.Valori(chRdrAngNorm.Valori.Count - 1)
          ReDim chRdrRecLwyAoA.Valori(chRdrAngNorm.Valori.Count - 1)
          Dim Dist As Double = 6
          For i As Integer = 0 To _TimeStamps.Count - 1
            Dim RdrLeeway As Double = 0
            Dim RdrLeewayR As Double = 0
            If ApplyYaw Then
              Dim isStbd As Boolean = chTwa.Valori(i) >= 0
              Dim Lx As Double = chBs.Valori(i) * System.Math.Cos(Radians(chLwyNorm.Valori(i)))
              Dim Ly As Double = chBs.Valori(i) * System.Math.Sin(Radians(chLwyNorm.Valori(i)))
              Dim LxR As Double = chBs.Valori(i) * System.Math.Cos(Radians(chRecLwy.Valori(i)))
              Dim LyR As Double = chBs.Valori(i) * System.Math.Sin(Radians(chRecLwy.Valori(i)))
              Dim yYaw As Double = Radians(chYawRate.Valori(i)) * Dist
              If Not isStbd Then
                yYaw *= -1 ' mure a sinistra inverto la yYaw in quanto lo yawrate è l'unico non normalizzato
              End If
              Dim RdrY As Double = Ly + yYaw
              Dim RdrYR As Double = LyR + yYaw
              RdrLeeway = Degrees(System.Math.Atan2(RdrY, Lx))
              RdrLeewayR = Degrees(System.Math.Atan2(RdrYR, LxR))
            Else
              RdrLeeway = chLwyNorm.Valori(i)
              RdrLeewayR = chRecLwy.Valori(i)
            End If
            chRdrAoA.Valori(i) = (chRdrAngNorm.Valori(i) + RdrLeeway)
            chRdrRecLwyAoA.Valori(i) = (chRdrAngNorm.Valori(i) + RdrLeewayR)
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eWindRatio
        Dim chTwsTopMast As clsChannel2020 = CanaleDbl("TwsAltBowRefHeightOrg")
        Dim chTwsBow As clsChannel2020 = CanaleDbl("TwsAltMastOrg")
        If Not chTwsTopMast Is Nothing AndAlso Not chTwsBow Is Nothing Then
          Dim chWindRatio As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eWindRatio)
          ReDim chWindRatio.Valori(_TimeStamps.Count - 1)
          For i As Long = 0 To _TimeStamps.Count - 1
            If Not Double.IsNaN(chTwsTopMast.Valori(i)) Then
              If Not Double.IsNaN(chTwsBow.Valori(i)) Then
                chWindRatio.Valori(i) = chTwsTopMast.Valori(i) / chTwsBow.Valori(i) * 100
              Else
                chWindRatio.Valori(i) = Double.NaN
              End If
            Else
              chWindRatio.Valori(i) = Double.NaN
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eRdrAngle
        Dim RdrYA As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
        Dim RdrA As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eRdrAngle)
        Dim CT As clsChannel2020 = CanaleTackDefault()
        ReDim RdrA.Valori(_TimeStamps.Count - 1)
        For i As Long = 0 To RdrYA.Valori.Count - 1
          If CT.Valori(i) >= 0 Then
            RdrA.Valori(i) = -RdrYA.Valori(i)
          Else
            RdrA.Valori(i) = RdrYA.Valori(i)
          End If
        Next

      Case clsChannels2020.eCanaliChiave.eTravellerTack
        Dim TravCounter As clsChannel2020 = CanaleDbl("Traveller_Ang")
        If Not TravCounter Is Nothing Then
          Dim TravellerTack As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTravellerTack)
          Dim CT As clsChannel2020 = CanaleTackDefault()
          ReDim TravellerTack.Valori(_TimeStamps.Count - 1)
          For i As Long = 0 To TravCounter.Valori.Count - 1
            If CT.Valori(i) >= 0 Then
              TravellerTack.Valori(i) = TravCounter.Valori(i)
            Else
              TravellerTack.Valori(i) = -TravCounter.Valori(i)
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eTp52MainLeechLoad
        Dim NewCh As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTp52MainLeechLoad)
        Dim MslCh As clsChannel2020 = CanaleDbl("Main")
        Dim VlCh As clsChannel2020 = CanaleDbl("SeaTemp") ' e'il vang
        If Not MslCh Is Nothing AndAlso Not VlCh Is Nothing Then
          ReDim NewCh.Valori(_TimeStamps.Count - 1)
          For i As Long = 0 To NewCh.Valori.Count - 1
            Dim MSL As Double = MslCh.Valori(i)
            Dim VL As Double = VlCh.Valori(i)
            If Not Double.IsNaN(MSL) AndAlso Not Double.IsNaN(VL) Then
              NewCh.Valori(i) = ((1.03 * MSL) + (0.15 * VL)) / 1000
            Else
              NewCh.Valori(i) = Double.NaN
            End If
          Next
        End If
      Case clsChannels2020.eCanaliChiave.eTp52MainLockLoad
        Dim NewCh As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTp52MainLockLoad)
        Dim MslCh As clsChannel2020 = CanaleDbl("Main")
        Dim VlCh As clsChannel2020 = CanaleDbl("SeaTemp") ' e'il vang
        Dim McCh As clsChannel2020 = CanaleDbl("JibUpDnCm")
        If Not MslCh Is Nothing AndAlso Not VlCh Is Nothing AndAlso Not McCh Is Nothing Then
          ReDim NewCh.Valori(_TimeStamps.Count - 1)
          For i As Long = 0 To NewCh.Valori.Count - 1
            Dim MSL As Double = MslCh.Valori(i)
            Dim VL As Double = VlCh.Valori(i)
            Dim MC As Double = McCh.Valori(i)
            If Not Double.IsNaN(MSL) AndAlso Not Double.IsNaN(VL) AndAlso Not Double.IsNaN(MC) Then
              NewCh.Valori(i) = ((0.7 * MSL) + (0.1 * VL / 1000) + MC) / 1000
            Else
              NewCh.Valori(i) = Double.NaN
            End If
          Next
        End If

      Case clsChannels2020.eCanaliChiave.eTws10mAlfaConst, clsChannels2020.eCanaliChiave.eTws10mAlfaVar
        Dim chTws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim TwsAlfaK As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTws10mAlfaConst)
        Dim TwsAlfaV As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTws10mAlfaVar)
        'If Not AwaTopMast.Valori.Where(Function(x) x = 0).Count = AwaTopMast.Valori.Count Then Exit Select
        ReDim TwsAlfaK.Valori(chTws.Valori.Count - 1)
        ReDim TwsAlfaV.Valori(chTws.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To chTws.Valori.Count - 1
          Dim Tws = chTws.Valori(i)
          If Double.IsNaN(Tws) Then
            TwsAlfaK.Valori(i) = Double.NaN
            TwsAlfaV.Valori(i) = Double.NaN
          Else
            TwsAlfaK.Valori(i) = ObjWind.WindAtHeight(chTws.Valori(i), AppConfig.ActiveProfile.MhuHeight, 10)
            TwsAlfaV.Valori(i) = ObjWind.WindAtHeightAlphaVariable(chTws.Valori(i), AppConfig.ActiveProfile.MhuHeight, 10)
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eAwaTopMast, clsChannels2020.eCanaliChiave.eAwsTopMast
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaTopMast As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwaTopMast)
        Dim AwsTopMast As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwsTopMast)
        'If Not AwaTopMast.Valori.Where(Function(x) x = 0).Count = AwaTopMast.Valori.Count Then Exit Select
        ReDim AwaTopMast.Valori(Twa.Valori.Count - 1)
        ReDim AwsTopMast.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ApparentFromTrue(AwaTmp, AwsTmp, Twa.Valori(i), Tws.Valori(i), Bs.Valori(i))
          AwaTopMast.Valori(i) = AwaTmp
          AwsTopMast.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa2m, clsChannels2020.eCanaliChiave.eAws2m
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim Awa2m As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa2m)
        Dim Aws2m As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws2m)
        ReDim Awa2m.Valori(Twa.Valori.Count - 1)
        ReDim Aws2m.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, 2, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          Awa2m.Valori(i) = AwaTmp
          Aws2m.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_J25, clsChannels2020.eCanaliChiave.eAws_J25
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_J25)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_J25)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightJib25, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_J50, clsChannels2020.eCanaliChiave.eAws_J50
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_J50)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_J50)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightJib50, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_J75, clsChannels2020.eCanaliChiave.eAws_J75
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_J75)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_J75)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightJib75, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_M25, clsChannels2020.eCanaliChiave.eAws_M25
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_M25)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_M25)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightMain25, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_M50, clsChannels2020.eCanaliChiave.eAws_M50
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_M50)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_M50)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightMain50, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_M75, clsChannels2020.eCanaliChiave.eAws_M75
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_M75)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_M75)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightMain75, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eAwa_M87, clsChannels2020.eCanaliChiave.eAws_M87
        Dim Heel As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Tws As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
        Dim AwaH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAwa_M87)
        Dim AwsH As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eAws_M87)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim AwaH.Valori(Twa.Valori.Count - 1)
        ReDim AwsH.Valori(Twa.Valori.Count - 1)
        Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Twa.Valori.Count - 1
          Dim AwaTmp, AwsTmp As Double
          ObjWind.ApparentAtHeight(Tws.Valori(i), AppConfig.ActiveProfile.MhuHeight, AppConfig.ActiveProfile.StripeHeightMain87, Bs.Valori(i), Twa.Valori(i), Heel.Valori(i), AwaTmp, AwsTmp)
          AwaH.Valori(i) = AwaTmp
          AwsH.Valori(i) = AwsTmp
        Next
      Case clsChannels2020.eCanaliChiave.eRtCurrDir, clsChannels2020.eCanaliChiave.eRtCurrRate
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Cse As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
        If Cse Is Nothing Then
          Cse = DataProvider2020.CseFromHdgAndLeeway()
        End If
        Dim Sog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
        Dim Cog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
        Dim Dir As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eRtCurrDir)
        Dim Rate As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eRtCurrRate)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim Dir.Valori(Bs.Valori.Count - 1)
        ReDim Rate.Valori(Bs.Valori.Count - 1)
        'Dim ObjWind As New clsWindCalc
        For i As Long = 0 To Bs.Valori.Count - 1
          Dim v = clsCurrentCalcs.CalcolaCorrente(Bs.Valori(i), Cse.Valori(i), Sog.Valori(i), Cog.Valori(i))
          Dir.Valori(i) = v.BRGdeg
          Rate.Valori(i) = v.SPD
        Next
      Case clsChannels2020.eCanaliChiave.eCurrDirRec, clsChannels2020.eCanaliChiave.eCurrRateRec
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim Cse As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
        Dim YR As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
        If YR Is Nothing Then
          If YRFromHdg() Is Nothing Then
            Exit Sub
          End If
          'Dim chHdg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
          'If chHdg Is Nothing Then Exit Function
          'YR = Channels.Canale(clsChannels2020.eCanaliChiave.eYRT)
          'ReDim YR.Valori(chHdg.Valori.Count - 1)
          'Dim hdgprev As Double = chHdg.Valori.First
          'For i As Long = 1 To chHdg.Valori.Count - 1
          '  Dim hdg = chHdg.Valori(i)
          '  If Double.IsNaN(hdgprev) OrElse Double.IsNaN(hdg) Then
          '    YR.Valori(i) = Double.NaN
          '  Else
          '    YR.Valori(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(hdgprev, hdg) * Hz
          '    hdgprev = hdg
          '  End If
          'Next
        End If
        Dim AvgSec As Integer = 60
        Dim YawMAx As Double = 2
        Dim YawInertia As Double = 10
        If Cse Is Nothing Then
          Cse = DataProvider2020.CseFromHdgAndLeeway()
        End If
        Dim Sog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
        Dim Cog As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
        Dim Dir As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eCurrDirRec)
        Dim Rate As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eCurrRateRec)
        'If Not AwaH.Valori.Where(Function(x) x = 0).Count = AwaH.Valori.Count Then Exit Select
        ReDim Dir.Valori(Bs.Valori.Count - 1)
        ReDim Rate.Valori(Bs.Valori.Count - 1)
        'Dim ObjWind As New clsWindCalc
        Dim ActualIdx As Long = -1
        Dim LastDir As Double = 0
        Dim LastRt As Double = 0
        Dim MmDir = New clsMediaMobile(AvgSec * Hz, True)
        Dim MmRt = New clsMediaMobile(AvgSec * Hz, False)
        For i As Long = 0 To Bs.Valori.Count - 1
          Dim AbsYR = Math.Abs(YR.Valori(i))
          If AbsYR > YawMAx Then
            ActualIdx = i + YawInertia * Hz
          End If
          If i > ActualIdx Then
            Dim v = clsCurrentCalcs.CalcolaCorrente(Bs.Valori(i), Cse.Valori(i), Sog.Valori(i), Cog.Valori(i))
            LastDir = MmDir.SetAndGet(v.BRGdeg)
            LastRt = MmRt.SetAndGet(v.SPD)
          End If
          Dir.Valori(i) = LastDir
          Rate.Valori(i) = LastRt
        Next
      Case clsChannels2020.eCanaliChiave.eSowInertial
        Dim Bs As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
        Dim AvgSec As Integer = AppConfig.ActiveProfile.InertialSowSeconds
        Dim BsInr As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eSowInertial)
        ReDim BsInr.Valori(Bs.Valori.Count - 1)
        Dim LastBs As Double = 0
        Dim MmBs = New clsMediaMobile(AvgSec * Hz, False)
        Dim MaxIndex As Integer = Bs.Valori.Count - (AvgSec + 1)
        'Console.WriteLine(Bs.Valori.Count)
        For i As Long = 0 To Bs.Valori.Count - 1
          'If i = 26058 Then Stop
          If i > MaxIndex Then
            LastBs = Bs.Valori(i)
          Else
            LastBs = MmBs.SetAndGet(Bs.Valori(i + AvgSec))
          End If
          BsInr.Valori(i) = LastBs
          'Console.WriteLine(i)
        Next
        'Stop
      Case clsChannels2020.eCanaliChiave.eTwaInertial, clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway
        Dim Twa As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
        Dim TwaRL As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eTwaRecLeeway)
        Dim AvgSec As Integer = 10
        Dim Delta As Integer = 5 * Hz 'i secondi prima della media mobile da ignorare
        Dim TwaInr As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTwaInertial)
        Dim TwaInrRL As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eTwaInertialRecLeeway)
        ReDim TwaInr.Valori(Twa.Valori.Count - 1)
        ReDim TwaInrRL.Valori(Twa.Valori.Count - 1)
        Dim ActualIdx As Long = -1
        Dim LastTwa As Double = 0
        Dim LastTwaRL As Double = 0
        Dim MmTwa = New clsMediaMobile(AvgSec * Hz, False)
        Dim MmTwaRL = New clsMediaMobile(AvgSec * Hz, False)
        For i As Long = 0 To Twa.Valori.Count - 1
          If i > Delta Then
            If i > ActualIdx Then
              LastTwa = MmTwa.SetAndGet(Twa.Valori(i - Delta))
              LastTwaRL = MmTwaRL.SetAndGet(TwaRL.Valori(i - Delta))
            End If
            TwaInr.Valori(i) = LastTwa
            TwaInrRL.Valori(i) = LastTwaRL
          Else
            TwaInr.Valori(i) = Double.NaN
            TwaInrRL.Valori(i) = Double.NaN
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eJibTwistFromFoot_25
        ' Vela & "_" & Stripe.StripePercentageHeight & "_" & NomeCanale
        Dim TwistChannel As clsChannel2020 = CanaleDbl("Jib_25_dTwist")
        Dim JibInOut As clsChannel2020 = CanaleDbl("JibLwdIo")
        If TwistChannel Is Nothing Then Exit Select
        If JibInOut Is Nothing Then Exit Select
        If TwistChannel.Valori Is Nothing OrElse TwistChannel.Valori.Count = 0 Then Exit Select
        If JibInOut.Valori Is Nothing OrElse JibInOut.Valori.Count = 0 Then Exit Select
        Dim JibTwistFromJibFoot As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_25)
        ReDim JibTwistFromJibFoot.Valori(TwistChannel.Valori.Count - 1)
        For i As Long = 0 To TwistChannel.Valori.Count - 1
          If TwistChannel.Valori(i) = Nothing OrElse TwistChannel.Valori(i) = 0 Then
            JibTwistFromJibFoot.Valori(i) = Double.NaN
          Else
            JibTwistFromJibFoot.Valori(i) = TwistChannel.Valori(i) - JibInOut.Valori(i)
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eJibTwistFromFoot_50
        ' Vela & "_" & Stripe.StripePercentageHeight & "_" & NomeCanale
        Dim TwistChannel As clsChannel2020 = CanaleDbl("Jib_50_dTwist")
        Dim JibInOut As clsChannel2020 = CanaleDbl("JibLwdIo")
        If TwistChannel Is Nothing Then Exit Select
        If JibInOut Is Nothing Then Exit Select
        If TwistChannel.Valori Is Nothing OrElse TwistChannel.Valori.Count = 0 Then Exit Select
        If JibInOut.Valori Is Nothing OrElse JibInOut.Valori.Count = 0 Then Exit Select
        Dim JibTwistFromJibFoot As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_50)
        ReDim JibTwistFromJibFoot.Valori(TwistChannel.Valori.Count - 1)
        For i As Long = 0 To TwistChannel.Valori.Count - 1
          If TwistChannel.Valori(i) = Nothing OrElse TwistChannel.Valori(i) = 0 Then
            JibTwistFromJibFoot.Valori(i) = Double.NaN
          Else
            JibTwistFromJibFoot.Valori(i) = TwistChannel.Valori(i) - JibInOut.Valori(i)
          End If
        Next
      Case clsChannels2020.eCanaliChiave.eJibTwistFromFoot_75
        ' Vela & "_" & Stripe.StripePercentageHeight & "_" & NomeCanale
        Dim TwistChannel As clsChannel2020 = CanaleDbl("Jib_75_dTwist")
        Dim JibInOut As clsChannel2020 = CanaleDbl("JibLwdIo")
        If TwistChannel Is Nothing Then Exit Select
        If JibInOut Is Nothing Then Exit Select
        If JibInOut.Valori Is Nothing OrElse TwistChannel.Valori.Count = 0 Then Exit Select
        If JibInOut.Valori Is Nothing OrElse JibInOut.Valori.Count = 0 Then Exit Select
        Dim JibTwistFromJibFoot As clsChannel2020 = Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_75)
        ReDim JibTwistFromJibFoot.Valori(TwistChannel.Valori.Count - 1)
        For i As Long = 0 To TwistChannel.Valori.Count - 1
          If TwistChannel.Valori(i) = Nothing OrElse TwistChannel.Valori(i) = 0 Then
            JibTwistFromJibFoot.Valori(i) = Double.NaN
          Else
            JibTwistFromJibFoot.Valori(i) = TwistChannel.Valori(i) - JibInOut.Valori(i)
          End If
        Next
      Case Else
        'Stop
    End Select
    'Console.WriteLine("VerificaCanaleMathsParquet - Channels created " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))

  End Sub


  Private Function YRFromHdg() As clsChannel2020
    Dim chHdg As clsChannel2020 = CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    If chHdg Is Nothing Then Return Nothing
    Dim YR = New clsChannel2020("YRT", clsChannels2020.eCanaliChiave.eYRT, "YawRt", "Yaw Rate", "°/S", "Deg/Sec", clsChannel2020.eDataType.eLinear, "", Nothing, "", "", False, 1) ', Nothing)
    ReDim YR.Valori(chHdg.Valori.Count - 1)
    Dim hdgprev As Double = chHdg.Valori.First
    For i As Long = 1 To chHdg.Valori.Count - 1
      Dim hdg = chHdg.Valori(i)
      If Double.IsNaN(hdgprev) OrElse Double.IsNaN(hdg) Then
        YR.Valori(i) = Double.NaN
      Else
        YR.Valori(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(hdgprev, hdg) * Hz
        hdgprev = hdg
      End If
    Next
    Return YR
  End Function

  Private Function ReverseHeelSign() As Boolean
    Dim Twa = CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim Heel = CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL)
    Dim HeelStbd As New List(Of Double)
    For i As Integer = 0 To Twa.Valori.Count - 1
      If Not Double.IsNaN(Twa.Valori(i)) AndAlso Not Double.IsNaN(Heel.Valori(i)) Then
        If Twa.Valori(i) > 20 AndAlso Twa.Valori(i) < 60 Then
          If Math.Abs(Heel.Valori(i)) > 15 Then
            HeelStbd.Add(Heel.Valori(i))
          End If
        End If
      End If
    Next
    If HeelStbd.Count = 0 Then Return False
    Return HeelStbd.Average < 0
  End Function

  Private Sub AggiungiValore(CanaleSource As clsChannel2020, ByRef CanaleDestinazione As clsChannel2020, Indice As Integer)
    If Not CanaleSource Is Nothing AndAlso CanaleSource.Valori.Count > 0 AndAlso CanaleSource.Valori.Count > Indice Then
      CanaleDestinazione.Valori(Indice) = CanaleSource.Valori(Indice)
    Else
      CanaleDestinazione.Valori(Indice) = 0
    End If
  End Sub

  Private Sub AggiungiValore(CanaleSource As clsChannel2020, ByRef CanaleDestinazione As clsChannel2020, Moltiplicatore As Integer, Indice As Integer)
    If Not CanaleSource Is Nothing AndAlso CanaleSource.Valori.Count > 0 AndAlso CanaleSource.Valori.Count > Indice Then
      CanaleDestinazione.Valori(Indice) = CanaleSource.Valori(Indice) * Moltiplicatore
      'Else
      '  CanaleDestinazione.Valori.Add(0)
    End If
  End Sub

  Public Enum eZonaVerifica
    eSoloPrima
    eSoloDopo
    eClosest
  End Enum


  Public Sub ExportInExpeditionFormat()
    ' Boat,Utc,BSP,AWA,AWS,TWA,TWS,TWD,RudderFwd,Leeway,Set,Drift,HDG,AirTemp,SeaTemp,Baro,Depth,Heel,Trim,Rudder,Tab,Forestay,Downhaul,MastAng,FStayLen,MastButt,Load S,Load P,Rake,Volts,ROT,GpQual,PDOP,GpsNum,GpsAge,Altitude,GeoSep,GpsMode,Lat,Lon,COG,SOG,DiffStn,Error,RunnerS,RunnerP,Vang,Trav,Main,KeelAng,KeelHt,Board,EngOilPres,RPM 1,RPM 2,Board P,Board S,DistToLn,RchTmToLn,RchDtToLn,GPS time,TWD+90,TWD-90,Downhaul2,Mk Lat,Mk Lon,Port lat,Port lon,Stbd lat,Stbd lon,HPE,RH,Lead P,Lead S,BackStay,user0,JibTackLoad,JibInOutLoad,user3,Tack load T,Rake mm,Checkstay Load,MWS,MWA,SecToLineAtVmgT,SecPerBlAtVmgTg,mhuAWASumphase,TgBs,TgTwa,Tg PolBs,user15,Hdg 2,user17,Checksty_pc,MButt mm,MainCunnLoad,user21,LeeCap,V1StbdBridge,V1PortBridge,mast shim,MOB CSE,MOB DST,MOB LAT,MOB LON,MOB STATE,user31,TmToGun,TmToLn,Burn,BelowLn,GunBlwLn,WvSigHt,WvSigPd,WvMaxHt,WvMaxPd,Slam,Heave,MWA,MWS,Boom,Twist,TackLossT,TackLossD,TrimRate,HeelRate,DeflectorP,RudderP,RudderS,RudderToe,BspTr,FStayInner,DeflectorS,Bobstay,Outhaul,D0 P,D0 S,D1 P,D1 S,V0 P,V0 S,V1 P,V1 S,BoomAng,Cunningham,FStayInHal,JibFurl,JibH,MastCant,J1,J2,J3,J4,Foil P,Foil S,Reacher,Blade,Staysail,Solent,Tack,TackP,TackS,DeflectU,DeflectL,WinchP,WinchS,SpinP,SpinS,MainH,Mast2,DepthAft,Burn%,GunBspTarg%,GunBspPol%,EngTemp,EngOilTemp,TranOilTemp,TranOilPres,FuelLevel,Amps,Charge%,GPS tOffset

    'Dim Canali = GestisciListaCanali("Channels to be exported in the expedition format csv file", Channels.ListaCanali.ToList, AppConfig.ActiveProfile.CanaliDaEsportareNelCsv, eSelectChannelType.eTwin)
    Dim Canali = Channels.ListaCanali.Where(Function(x) x.IsMath = False AndAlso Not x.ActualLogHeader = "").ToList
    If Canali.Count > 0 Then
      AppConfig.ActiveProfile.CanaliDaEsportareNelCsv = Canali.Select(Function(x) x.ActualLogHeader).ToList
      AppConfig.Salva()

      Dim Righe As New List(Of String)
      Dim Riga As String = "Utc"
      For Each Channel In Canali
        Riga &= "," & RimappaNomiPerExpedition(Channel.ActualLogHeader)
        Dim c = Me.CanaleDbl(Channel)
      Next
      Righe.Add(Riga)
      For i As Integer = 0 To TimeStamps.Count - 1
        If Not TimeStamps(i) = DateTime.MinValue Then
          Riga = TimeStamps(i).AddHours(-2).ToOADate
          For Each Channel In Canali
            Dim c = Me.CanaleDbl(Channel)
            If Channel.ActualLogHeader = "TmToGun" Then
              If c.Valori(i) = Double.NaN Then
                Riga &= "," & c.Valori(i)
              Else
                Riga &= "," & c.Valori(i) / (24 * 3600)
              End If
            Else
              Riga &= "," & c.Valori(i)
            End If
          Next
          Righe.Add(Riga)
        End If
      Next

      ObjFiles.SalvaNuovoFileSostituendoContenuto(String.Join(vbCrLf, Righe), Me.Files.First.FullName.Replace(".ppf", ".csv"))
      ApriExplorer(Me.Files.First.FullName.Replace(".txt", "csv"))
    End If
  End Sub

  Private Function RimappaNomiPerExpedition(Canale As String) As String
    Select Case Canale.ToLower
      Case "lng"
        Return "lon"
      Case "sow"
        Return "Bsp"
      Case "yrt"
        Return "ROT"
      Case "TrimRT"
        Return "Trim Rate"
      Case "HeelRT"
        Return "Heel Rate"
      Case Else
        Return Canale
    End Select
  End Function


End Class

Public Class clsMySongLeewaySettings
  Public Property LwyTwsTableName As String
  Public Property LwyTwaTableName As String
  Public Property LwySeaStateTableName As String
  Public Property DaggerMinAreaKeelRatio As Double
  Public Property DaggerMaxAreaKeelRatio As Double
  Public Property LwyDaggerBs2dTableName As String

End Class

' clsQualitySettings e' stata spostata e riorganizzata in cls2021.vb, accanto a clsChDataQuality.


Public Class clsChannels2020
  Dim pParentDataProvider As clsDataProvider2020
  'Dim _CanaleDT As clsChannel2020
  'Dim _CanaleDtPerformance As clsChannel2020
  'Dim pCanaleValidRows As New clsValidRows
  Dim pListaCanaliConosciuti As New List(Of clsChannel2020)
  Dim pListaCanaliChiave As New List(Of clsChannel2020)
  'Dim _ListaCanaliFileCorrente As New List(Of clsChannel2020)
  Dim _ListaCanali As New ObservableCollection(Of clsChannel2020)
  Dim pToggleImporta As eToggleImporta = eToggleImporta.eKeyChannelsOnly


  Public Sub New(ParentDataProvider As clsDataProvider2020)
    pParentDataProvider = ParentDataProvider
    CaricaCanaliChiave()
    CaricaCanaliConosciutiJson()
    'pCanaleValidRows.CanaleNVR = Canale(eCanaliChiave.eCanaleNVR)
  End Sub

  Public Enum eToggleImporta
    'eDefault = 0
    eKeyChannelsOnly = 1
    eAllChannels = 2
    eNone = 3
  End Enum

  Public Enum eCanaliChiave
    'NON CAMBIARE ID!!!!!! accodarne solamente di nuovi
    eCanaleNVR = -3
    eCanaleDT = -2
    eNone = -1

    eDateTime = 0
    eDateOnly = 1
    eTimeOnly = 2
    eSOW = 3
    eHDG = 4
    eSOG = 5
    eCOG = 6
    eLat = 7
    eLng = 8
    eLWY = 9
    eHEEL = 10

    eTRIM = 11
    eYRT = 12
    ePRT = 13
    eRRT = 14
    eTWA = 15
    eTWS = 16
    eTWD = 17
    eAWA = 18
    eAWS = 19
    eRdrAngle = 20

    eRdrRake = 21
    eFlyingStatus = 22
    eTrimNorm = 23 ' Normalized trim, needed if trim is pitch signed or reversed
    eTravNorm = 24 ' Normalized trim, needed if trim is pitch signed or reversed
    'eVmgShear = 23
    'eVmgShearPerc = 24
    'eStbdCantAngle = 25
    eAbsTwa = 26
    ePositionHeading = 27
    eIsStbd = 28
    eVMG = 29
    eVMGp = 30

    eBSTp = 31
    eBSPp = 32
    eTWAd = 33
    eTwaNoLeeway = 34
    eTwaRecLeeway = 35
    ePitch = 36
    eVmgRecLwy = 37
    eVmgPercRecLwy = 38

    eMsLwy = 39
    eMsTwa = 40
    eMsVmg = 41
    eMsVmgPerc = 42

    eHeadstayTotalLoad = 43
    eAwaWithLwy = 44

    eAwaRecLwy = 45
    eAwsRecLwy = 46
    eAwaRecNoLwy = 47


    eUpDnNetPress = 48
    'eStbdFoilOutFlap2Angle = 46

    'eLwdCantAngle = 47
    'eWwdCantAngle = 48

    'eLwdFoilOutFlap2Angle = 49
    'eLwdFoilOutFlap1Angle = 50
    'eLwdFoilInFlap1Angle = 51
    'eLwdFoilInFlap2Angle = 52

    'eWwdFoilInFlap2Angle = 53
    'eWwdFoilInFlap1Angle = 54
    'eWwdFoilOutFlap1Angle = 55
    'eWwdFoilOutFlap2Angle = 56

    eCSE = 57

    'ePortCantAngleEffective = 58
    'eStbdCantAngleEffective = 59
    'eLwdCantAngleEffective = 60
    'eWwdCantAngleEffective = 61
    'eRdrRakeEffective = 62

    eRudderAoA = 63
    eRdrRecLwyAoA = 64

    eTrimRT = 65
    eHeelRT = 66

    eTwaInertial = 67
    eTwaInertialRecLeeway = 68
    eVmgInertial = 69
    eVmgInertialRecLeeway = 70
    eVmgPercInertial = 71
    eVmgPercInertialRecLeeway = 72


    eVmc50 = 73
    eVmc60 = 74
    eVmc70 = 75
    eVmc80 = 76
    eVmc90 = 77
    eVmc100 = 78
    eVmc110 = 79
    eVmc120 = 80
    eVmc130 = 81
    eVmc140 = 82
    eVmc150 = 83
    eVmc40 = 84

    eSailSet = 85
    eSowInertial = 86

    'ePortWingInnerProjPercLift = 67
    'ePortWingInnerProjPercLwy = 68
    'ePortWingOuterProjPercLift = 69
    'ePortWingOuterProjPercLwy = 70
    'eStbdWingInnerProjPercLift = 71
    'eStbdWingInnerProjPercLwy = 72
    'eStbdWingOuterProjPercLift = 73
    'eStbdWingOuterProjPercLwy = 74
    'eUnderwaterWingWindwardProjPercLift = 75
    'eUnderwaterWingWindwardProjPercLwy = 76
    'eUnderwaterWingLeewardProjPercLift = 77
    'eUnderwaterWingLeewardProjPercLwy = 78
    'eUnderwaterWingGlobalProjPercLift = 75
    'eUnderwaterWingGlobalProjPercLwy = 76

    'ePortV1 = 67
    'ePortD1 = 68
    'ePortRunner = 69
    'ePortJibSheet = 70
    'ePortJibUpDn = 71

    'eStbdV1 = 72
    'eStbdD1 = 73
    'eStbdRunner = 74
    'eStbdJibSheet = 75
    'eStbdJibUpDn = 76

    'eLwdV1 = 77
    'eLwdD1 = 78
    'eLwdRunner = 79
    'eLwdJibSheet = 80
    'eLwdJibUpDn = 81

    'eWwdV1 = 82
    'eWwdD1 = 83
    'eWwdRunner = 84
    'eWwdJibSheet = 85
    'eWwdJibUpDn = 86

    'eSpannerCounter = 87
    'eTopCtrlArmCounter = 88
    'eTravellerCounter = 89

    'eSpannerTack = 90
    'eTopCtrlArm = 91
    eTp52MainLeechLoad = 90
    eTp52MainLockLoad = 91
    eTravellerTack = 92
    'eTopTwist = 93

    eRoll = 94

    'eAoaMainFoot = 95
    'eAoaMainTop = 96

    eAwa2m = 97
    eAws2m = 98

    eAwaTopMast = 99
    eAwsTopMast = 100

    eDaggerAoA = 101
    eDaggerAoARL = 102

    'ePortJibCarAngle = 102
    'eStbdJibCarAngle = 103

    eRdrNorm = 104

    eWindRatio = 105

    'Tws(h) = TWS(ref)*(h/href)^0.1

    eTws10mAlfaConst = 106
    eTws10mAlfaVar = 107

    'eRdrLwd = 106
    'eTwaTgtTable = 107
    'eVmgTgtTable = 108
    'eBsPolTable = 109

    'eStbdOutFlapAngDamped = 110
    'ePortOutFlapAngDamped = 111
    'eStbdInFlapAngDamped = 112
    'ePortInFlapAngDamped = 113

    'eCurrRateDll = 114
    'eCurrDirDll = 115

    'eGlobalStabilityIndex = 116
    'eGlobalWeightedStabilityIndex = 117
    'eGlobalStabilityToolsIndex = 118
    'eGlobalWeightedStabilityToolsIndex = 119


    eLwyK3s = 116
    eLwyK30s = 117
    eLwyRec3s = 118
    eLwyRec30s = 119

    eLwyNorm = 120
    eHeelNorm = 121
    eDataQuality = 122


    eLeewayRecalc = 123
    eSowSogK = 124
    eSowSogDelta = 125
    eHeelDelta = 126
    eTrimDelta = 127
    eRudderDelta = 128

    eEnvironmentQuality = 129
    eAttitudeQuality = 130

    eSailingState = 131
    eLwyK = 132

    eSeaState = 133

    eMwaNorm = 134
    eAbsAwa = 135

    eAwa_J25 = 136
    eAws_J25 = 137
    eAwa_J50 = 138
    eAws_J50 = 139
    eAwa_J75 = 140
    eAws_J75 = 141

    eAwa_M25 = 142
    eAws_M25 = 143
    eAwa_M50 = 144
    eAws_M50 = 145
    eAwa_M75 = 146
    eAws_M75 = 147
    eAwa_M87 = 148
    eAws_M87 = 149

    eRtCurrDir = 150
    eRtCurrRate = 151

    eJibTwistFromFoot_25 = 152
    eJibTwistFromFoot_50 = 153
    eJibTwistFromFoot_75 = 154

    eCurrDirRec = 155
    eCurrRateRec = 156

    eVmgShear = 157
    eVmgShearPerc = 158

    eLeewayModel = 159 ' leeway teorico dal modello a coefficienti (clsLeewayModel)

    eSeaStateNorm = 160 ' SeaState atteso per la TWS corrente, da tabella del profilo (clsSeaStateNorm)
    eSeaStateNormDelta = 161 ' SeaState misurato meno SeaState normalizzato


  End Enum

  'Public Property CanaleDT As clsChannel2020
  '  Get
  '    Return _CanaleDT
  '  End Get
  '  Set(value As clsChannel2020)
  '    _CanaleDT = value
  '  End Set
  'End Property

  Public Property ListaCanali As ObservableCollection(Of clsChannel2020)
    Get
      Return _ListaCanali
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      _ListaCanali = value
    End Set
  End Property

  'Public ReadOnly Property ListaCanaliMath As List(Of clsChannel2020)
  '  Get
  '    Return pListaCanali.Where(Function(x) x.IsMath).ToList
  '  End Get
  'End Property

  'Public Property ListaCanaliFileCorrente As List(Of clsChannel2020)
  '  Get
  '    Return _ListaCanaliFileCorrente
  '  End Get
  '  Set(value As List(Of clsChannel2020))
  '    _ListaCanaliFileCorrente = value
  '  End Set
  'End Property

  Dim pDictionaryCanali As New Dictionary(Of eCanaliChiave, clsChannel2020)

  Public ReadOnly Property Canale(CanaleChiave As eCanaliChiave) As clsChannel2020
    Get
      Dim Risultato As clsChannel2020 = Nothing
      If Not pDictionaryCanali.TryGetValue(CanaleChiave, Risultato) Then
        Risultato = _ListaCanali.Where(Function(x) x.CanaleChiave = CanaleChiave).FirstOrDefault
        If Not Risultato Is Nothing Then pDictionaryCanali(CanaleChiave) = Risultato
      End If
      'If Not Risultato Is Nothing Then
      '  If Risultato.Valori Is Nothing OrElse Risultato.Valori.Count = 0 Then DataProvider2020.VerificaCanaleMathsParquet(Risultato)
      'End If
      Return Risultato
    End Get
  End Property

  Public ReadOnly Property Canale(NomeCanale As String) As clsChannel2020
    Get
      If NomeCanale = Nothing Then Return Nothing
      Dim Risultato As clsChannel2020 = Nothing
      ' lookup O(1). In caso di miss si ricostruisce l'indice: copre l'aggiunta a runtime
      ' dei canali _Tgt / _Pol e dei canali math creati su richiesta.
      ' SyncLock: il calcolo delle serie dei plot gira su thread di background e puo'
      ' interrogare l'indice mentre la UI lo sta ricostruendo.
      SyncLock pLockDizionario
        If Not pDictionaryCanaliPerNome.TryGetValue(NomeCanale, Risultato) Then
          RicostruisciIndiceNomi()
          pDictionaryCanaliPerNome.TryGetValue(NomeCanale, Risultato)
        ElseIf Risultato IsNot Nothing Then
          ' l'indice potrebbe essere stale se la lista e' stata rigenerata
          If Not _ListaCanali.Contains(Risultato) Then
            RicostruisciIndiceNomi()
            Risultato = Nothing
            pDictionaryCanaliPerNome.TryGetValue(NomeCanale, Risultato)
          End If
        End If
      End SyncLock
      If Not Risultato Is Nothing Then
        If Risultato.Valori Is Nothing OrElse Risultato.Valori.Count = 0 Then
          If Risultato.IsMath Then
            DataProvider2020.VerificaCanaleMathsParquet(Risultato)
          ElseIf Risultato.CanaleChiave = eCanaliChiave.eSeaState Then
            DataProvider2020.VerificaCanaleMathsParquet(Risultato)
          End If
        End If
      End If
      Return Risultato
    End Get
  End Property

  ''' <summary>
  ''' Indice nome canale -> canale, case insensitive. Sostituisce la scansione lineare
  ''' con doppio ToLower che veniva eseguita a ogni accesso (anche dai binding della UI).
  ''' </summary>
  Private pDictionaryCanaliPerNome As New Dictionary(Of String, clsChannel2020)(StringComparer.OrdinalIgnoreCase)
  Private ReadOnly pLockDizionario As New Object

  ''' <summary>Da chiamare gia' dentro SyncLock pLockDizionario.</summary>
  Public Sub RicostruisciIndiceNomi()
    pDictionaryCanaliPerNome.Clear()
    For Each c In _ListaCanali
      If c Is Nothing Then Continue For
      If c.ChannelId Is Nothing Then Continue For
      pDictionaryCanaliPerNome(c.ChannelId) = c
    Next
  End Sub

  'Public ReadOnly Property CanaleConVerificaArrayValori(CanaleChiave As eCanaliChiave) As clsChannel2020
  '  Get
  '    Dim Tmp As clsChannel2020 = pListaCanali.Where(Function(x) x.CanaleChiave = CanaleChiave).FirstOrDefault
  '    If Not Tmp Is Nothing Then
  '      If Tmp.Valori Is Nothing Then ReDim Tmp.Valori(_CanaleDT.ValoriDT.Count - 1)
  '    End If
  '    Return Tmp
  '  End Get
  'End Property

  'Public ReadOnly Property CanaleIsLoaded(CanaleChiave As eCanaliChiave) As Boolean
  '  Get
  '    Return Not pListaCanali.Where(Function(x) x.CanaleChiave = CanaleChiave).FirstOrDefault Is Nothing
  '  End Get
  'End Property

  'Public Property CanaleValidRows As clsValidRows
  '  Get
  '    Return pCanaleValidRows
  '  End Get
  '  Set(value As clsValidRows)
  '    pCanaleValidRows = value
  '  End Set
  'End Property

  'Public Property CanaleDtPerformance As clsChannel2020
  '  Get
  '    Return _CanaleDtPerformance
  '  End Get
  '  Set(value As clsChannel2020)
  '    _CanaleDtPerformance = value
  '  End Set
  'End Property

  Private Function KnownHeaders(CanaleChiave As eCanaliChiave, DefaultValue As String) As List(Of String)
    Select Case pParentDataProvider.FileType
      Case clsDataProvider2020.eFileType.eGombocSqlLite
        Stop
        'Select Case CanaleChiave
        '  Case eCanaliChiave.eDateOnly
        '    Return New List(Of String)(New String() {"SystemTime_Date"})
        '  Case eCanaliChiave.eTimeOnly
        '    Return New List(Of String)(New String() {"SystemTime_Local"})
        '  Case eCanaliChiave.eSOW
        '    Return New List(Of String)(New String() {"Bs", "Bsp"})
        '  Case eCanaliChiave.eHDG
        '    Return New List(Of String)(New String() {"Hdg"})
        '  Case eCanaliChiave.eSOG
        '    Return New List(Of String)(New String() {"Sog"})
        '  Case eCanaliChiave.eCOG
        '    Return New List(Of String)(New String() {"Cog"})
        '  Case eCanaliChiave.eLat
        '    Return New List(Of String)(New String() {"LatBow"})
        '  Case eCanaliChiave.eLng
        '    Return New List(Of String)(New String() {"LonBow"})
        '  Case eCanaliChiave.eLWY
        '    Return New List(Of String)(New String() {"Lwy"})
        '  Case eCanaliChiave.eHEEL
        '    Return New List(Of String)(New String() {"Heel"})
        '  Case eCanaliChiave.ePitch
        '    Return New List(Of String)(New String() {"Trim"})
        '  Case eCanaliChiave.eYRT
        '    Return New List(Of String)(New String() {"YawRate"})
        '  Case eCanaliChiave.ePRT
        '    Return New List(Of String)(New String() {"PitchRate"})
        '  Case eCanaliChiave.eRRT
        '    Return New List(Of String)(New String() {"RollRate"})
        '  Case eCanaliChiave.eTWA
        '    Return New List(Of String)(New String() {"Twa"})
        '  Case eCanaliChiave.eTWS
        '    Return New List(Of String)(New String() {"Tws"})
        '  Case eCanaliChiave.eTWD
        '    Return New List(Of String)(New String() {"Twd"})
        '  Case eCanaliChiave.eAWA
        '    Return New List(Of String)(New String() {"Awa"})
        '  Case eCanaliChiave.eAWS
        '    Return New List(Of String)(New String() {"Aws"})
        '  'Case eCanaliChiave.ePortCantAngle
        '  '  Return New List(Of String)(New String() {"FCS_PortCant_Ang"})
        '  'Case eCanaliChiave.eStbdCantAngle
        '  '  Return New List(Of String)(New String() {"FCS_StbdCant_Ang"})
        '  Case eCanaliChiave.eRdrAngle
        '    Return New List(Of String)(New String() {"Rudder_Ang"})
        '  'Case eCanaliChiave.eRdrNorm
        '  '  Return New List(Of String)(New String() {"RudderNorm_Ang"})
        '  'Case eCanaliChiave.eLwyNorm
        '  '  Return New List(Of String)(New String() {"LeewayNorm"})
        '  'Case eCanaliChiave.eHeelNorm
        '  '  Return New List(Of String)(New String() {"HeelingNorm"})
        '  'Case eCanaliChiave.eDataQuality
        '  '  Return New List(Of String)(New String() {"DataQuality"})
        '  'Case eCanaliChiave.eRdrRake
        '  '  Return New List(Of String)(New String() {"RudderRake_Ang"})
        '  'Case eCanaliChiave.ePortFoilOutFlap2Angle
        '  '  Return New List(Of String)(New String() {"PortOutFlap2_Ang"})
        '  'Case eCanaliChiave.ePortFoilOutFlap1Angle
        '  '  Return New List(Of String)(New String() {"PortOutFlap1_Ang"})
        '  'Case eCanaliChiave.ePortFoilInFlap1Angle
        '  '  Return New List(Of String)(New String() {"PortInFlap1_Ang"})
        '  'Case eCanaliChiave.ePortFoilInFlap2Angle
        '  '  Return New List(Of String)(New String() {"PortOutFlap2_Ang"})
        '  'Case eCanaliChiave.eStbdFoilInFlap2Angle
        '  '  Return New List(Of String)(New String() {"StbdOutFlap2_Ang"})
        '  'Case eCanaliChiave.eStbdFoilInFlap1Angle
        '  '  Return New List(Of String)(New String() {"StbdOutFlap1_Ang"})
        '  'Case eCanaliChiave.eStbdFoilOutFlap1Angle
        '  '  Return New List(Of String)(New String() {"StbdInFlap1_Ang"})
        '  'Case eCanaliChiave.eStbdFoilOutFlap2Angle
        '  '  Return New List(Of String)(New String() {"StbdInFlap2_Ang"})
        '  Case eCanaliChiave.eTrimRT
        '    Return New List(Of String)(New String() {"TrimRate"})
        '  Case eCanaliChiave.eHeelRT
        '    Return New List(Of String)(New String() {"HeelRate"})
        '  'Case eCanaliChiave.ePortV1
        '  '  Return New List(Of String)(New String() {"PortV1Pin_Load"})
        '  'Case eCanaliChiave.ePortD1
        '  '  Return New List(Of String)(New String() {"PortD1Pin_Load"})
        '  'Case eCanaliChiave.ePortRunner
        '  '  Return New List(Of String)(New String() {"PortRunnerRam_Load"})
        '  'Case eCanaliChiave.ePortJibSheet
        '  '  Return New List(Of String)(New String() {"PortJibSheetRam_Load"})
        '  'Case eCanaliChiave.ePortJibUpDn
        '  '  Return New List(Of String)(New String() {"PortJibUpDwRam_Load"})
        '  'Case eCanaliChiave.eStbdV1
        '  '  Return New List(Of String)(New String() {"StbdV1Pin_Load"})
        '  'Case eCanaliChiave.eStbdD1
        '  '  Return New List(Of String)(New String() {"StbdD1Pin_Load"})
        '  'Case eCanaliChiave.eStbdRunner
        '  '  Return New List(Of String)(New String() {"StbdRunnerRam_Load"})
        '  'Case eCanaliChiave.eStbdJibSheet
        '  '  Return New List(Of String)(New String() {"StbdJibSheetRam_Load"})
        '  'Case eCanaliChiave.eStbdJibUpDn
        '  '  Return New List(Of String)(New String() {"StbdJibUpDwRam_Load"})
        '  'Case eCanaliChiave.eSpannerCounter
        '  '  Return New List(Of String)(New String() {"Spanner_Ang"})
        '  'Case eCanaliChiave.eTopCtrlArmCounter
        '  '  Return New List(Of String)(New String() {"TopCtrlArm_Ang"})
        '  'Case eCanaliChiave.eTravellerCounter
        '  '  Return New List(Of String)(New String() {"Traveller_Ang"})
        '  Case eCanaliChiave.eRoll
        '    Return New List(Of String)(New String() {"Roll"})
        '    Return New List(Of String)(New String() {"Pitch"})
        '    'Case eCanaliChiave.eTRIM
        '  Case Else
        '    Return New List(Of String)(New String() {DefaultValue})
        'End Select
      Case clsDataProvider2020.eFileType.eFaRoBin
        Stop
      Case clsDataProvider2020.eFileType.eDfwLog
        Stop
      Case clsDataProvider2020.eFileType.eExpLog
        Stop
      Case clsDataProvider2020.eFileType.eFaRoCsv
        Stop
      Case clsDataProvider2020.eFileType.eParquet
        Select Case CanaleChiave
          Case eCanaliChiave.eDateOnly
            Return New List(Of String)(New String() {"SystemTime_Date"})
          Case eCanaliChiave.eTimeOnly
            Return New List(Of String)(New String() {"SystemTime_Local"})
          Case eCanaliChiave.eSOW
            Return New List(Of String)(New String() {"Bs", "Bsp"})
          Case eCanaliChiave.eHDG
            Return New List(Of String)(New String() {"Hdg", "Heading", "HDG"})
          Case eCanaliChiave.eSOG
            Return New List(Of String)(New String() {"Sog", "SOG"})
          Case eCanaliChiave.eCOG
            Return New List(Of String)(New String() {"Cog", "COG"})
          Case eCanaliChiave.eLat
            Return New List(Of String)(New String() {"LatBow", "Latitude", "Lat"})
          Case eCanaliChiave.eLng
            Return New List(Of String)(New String() {"LonBow", "Longitude", "Lon", "Lng"})
          Case eCanaliChiave.eLWY
            Return New List(Of String)(New String() {"Lwy", "Leeway"})
          Case eCanaliChiave.eHEEL
            Return New List(Of String)(New String() {"Heel"})
          Case eCanaliChiave.eTRIM
            Return New List(Of String)(New String() {"Trim", "Trm"})
          Case eCanaliChiave.ePitch
            Return New List(Of String)(New String() {"Pitch", "Ptc"})
          Case eCanaliChiave.eYRT
            Return New List(Of String)(New String() {"YawRate", "Yrt", "ROT"})
          Case eCanaliChiave.ePRT
            Return New List(Of String)(New String() {"PitchRate", "Prt"})
          Case eCanaliChiave.eRRT
            Return New List(Of String)(New String() {"RollRate", "Rrt"})
          Case eCanaliChiave.eTWA
            Return New List(Of String)(New String() {"Twa", "TrueWA"})
          Case eCanaliChiave.eTWS
            Return New List(Of String)(New String() {"Tws", "TrueWS"})
          Case eCanaliChiave.eTWD
            Return New List(Of String)(New String() {"Twd", "TrueWD"})
          Case eCanaliChiave.eAWA
            Return New List(Of String)(New String() {"Awa", "AppWA"})
          Case eCanaliChiave.eAWS
            Return New List(Of String)(New String() {"Aws", "AppWS"})
          Case eCanaliChiave.eRdrAngle
            Return New List(Of String)(New String() {"Rudder_Ang", "Rdr", "RdrAng", "Rudder"})
          Case eCanaliChiave.eTrimRT
            Return New List(Of String)(New String() {"TrimRate", "Trt"})
          Case eCanaliChiave.eHeelRT
            Return New List(Of String)(New String() {"HeelRate", "Hrt"})
          Case eCanaliChiave.eRoll
            Return New List(Of String)(New String() {"Roll", "Rll"})
          Case Else
            Return New List(Of String)(New String() {DefaultValue})
        End Select
      Case Else
        Stop
        Return New List(Of String)(New String() {DefaultValue})
    End Select
    ' formati non gestiti (Gomboc, FaRoBin, Dfw, Exp, FaRoCsv): nessuna intestazione nota
    Return Nothing

  End Function

  Private Sub CaricaCanaliChiave()
    pListaCanaliChiave.Clear()
    Dim NomiCanaliChiave As String() = System.Enum.GetNames(GetType(eCanaliChiave))
    Dim ChiaviCanaliChiave As eCanaliChiave() = System.Enum.GetValues(GetType(eCanaliChiave))



    For Each CanaleChiave In ChiaviCanaliChiave
      Dim pChannelId As String = System.Enum.GetName(GetType(eCanaliChiave), CanaleChiave).TrimStart("e") '   NomiCanaliChiave(ChiaviCanaliChiave.Select(Function(x) x.GetValues(CanaleChiave) ).TrimStart("e")
      Dim pCanaleChiave As clsChannels2020.eCanaliChiave = CanaleChiave
      Dim pShortName As String = pChannelId
      Dim pLongName As String = pChannelId
      Dim pShortUM As String = "°"
      Dim pLongUM As String = "Deg"
      Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
      'Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
      Dim pKnownHeaders As List(Of String) = KnownHeaders(CanaleChiave, pChannelId)
      Dim pActualLogHeader As String = pKnownHeaders.First
      Dim pPolarHeader As String = ""
      Dim pBenchmarkHeader As String = ""
      Dim pIsMath As Boolean = False
      Dim pDecimals As Integer = 1
      Select Case CanaleChiave
        Case eCanaliChiave.eCanaleNVR
          pIsMath = True
        Case eCanaliChiave.eCanaleDT
          pIsMath = True
        Case eCanaliChiave.eNone
        Case eCanaliChiave.eDateTime
          pShortName = "DT"
          pLongName = "Date Time"
          pShortUM = ""
          pLongUM = ""
          pDataType = clsChannel2020.eDataType.eDateTime
          'Select Case pParentDataProvider.FileType
          '  Case clsDataProvider2020.eFileType.eParquet
          '  Case clsDataProvider2020.eFileType.eGombocSqlLite
          '  Case clsDataProvider2020.eFileType.eFaRoBin
          '  Case clsDataProvider2020.eFileType.eFaRoCsv
          'End Select
        Case eCanaliChiave.eDateOnly
          pShortName = "Dt"
          pLongName = "Date"
          pShortUM = ""
          pLongUM = ""
          pDataType = clsChannel2020.eDataType.eDateOnly
        Case eCanaliChiave.eTimeOnly
          pShortName = "Tm"
          pLongName = "Time"
          pShortUM = ""
          pLongUM = ""
          pDataType = clsChannel2020.eDataType.eTimeOnly
        Case eCanaliChiave.eSOW
          pShortName = "Sow"
          pLongName = "Speed Over Water"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Bs"
          pBenchmarkHeader = "Bs"
        Case eCanaliChiave.eHDG
          pShortName = "Hdg"
          pLongName = "Heading"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eSOG
          pShortName = "Sog"
          pLongName = "Speed Over Ground"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Bs"
          pBenchmarkHeader = "Bs"
        Case eCanaliChiave.eCOG
          pShortName = "Cog"
          pLongName = "Course Over Ground"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eLat
          pShortName = "Lat"
          pLongName = "Latitude"
          pDecimals = 6
        Case eCanaliChiave.eLng
          pShortName = "Long"
          pLongName = "Longitude"
          pDecimals = 6
        Case eCanaliChiave.eLWY
          pShortName = "Lwy"
          pLongName = "Leeway"
        Case eCanaliChiave.eHEEL
          pShortName = "Heel"
          pLongName = "Heel"
          pPolarHeader = "Heel"
        Case eCanaliChiave.eTRIM
          pShortName = "Trm"
          pLongName = "Trim"
          pPolarHeader = "Trim"
          pIsMath = False
        Case eCanaliChiave.eTrimNorm
          pShortName = "TrmN"
          pLongName = "TrimNorm"
          pPolarHeader = "Trim"
          pIsMath = True
        Case eCanaliChiave.eTravNorm
          pShortName = "TravN"
          pLongName = "TravNorm"
          pPolarHeader = "Trav"
          pIsMath = True
        Case eCanaliChiave.eYRT
          pShortName = "YawRt"
          pLongName = "Yaw Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
        Case eCanaliChiave.ePRT
          pShortName = "PtcRt"
          pLongName = "Pitch Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
        Case eCanaliChiave.eRRT
          pShortName = "RollRt"
          pLongName = "Roll Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
        Case eCanaliChiave.eTWA
          pShortName = "Twa"
          pLongName = "True Wind Angle"
          pDataType = clsChannel2020.eDataType.e180
          pPolarHeader = "Twa"
          pBenchmarkHeader = "Twa"
        Case eCanaliChiave.eTWS
          pShortName = "Tws"
          pLongName = "True Wind Speed"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Tws"
          pBenchmarkHeader = "Tws"
        Case eCanaliChiave.eTws10mAlfaConst
          pShortName = "Tws@10K"
          pLongName = "True Wind Speed @10K"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Tws"
          pBenchmarkHeader = "Tws"
          pIsMath = True
        Case eCanaliChiave.eTws10mAlfaVar
          pShortName = "Tws@10V"
          pLongName = "True Wind Speed @10V"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Tws"
          pBenchmarkHeader = "Tws"
          pIsMath = True
        Case eCanaliChiave.eTWD
          pShortName = "Twd"
          pLongName = "True Wind Direction"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eAWA
          pShortName = "Awa"
          pLongName = "Apparent Wind Angle"
          pDataType = clsChannel2020.eDataType.e180
        Case eCanaliChiave.eAWS
          pShortName = "Aws"
          pLongName = "Apparent Wind Speed"
          pShortUM = "K"
          pLongUM = "Kts"
        Case eCanaliChiave.eRdrAngle
          pShortName = "Rdr"
          pLongName = "Rudder Angle"
          pPolarHeader = "Rudder"
        Case eCanaliChiave.eRdrNorm
          pIsMath = True
          pShortName = "RdrN"
          pLongName = "Rudder Normalized"
          pPolarHeader = "Rudder"
        Case eCanaliChiave.eLwyNorm
          pIsMath = True
          pShortName = "LwyN"
          pLongName = "Leeway Normalized"
          pPolarHeader = "Leeway"
        Case eCanaliChiave.eHeelNorm
          pIsMath = True
          pShortName = "HeelN"
          pLongName = "Heeling Normalized"
          pPolarHeader = "Heel"
        Case eCanaliChiave.eDataQuality
          ' il membro enum resta eDataQuality per non invalidare i .ppf e le configurazioni
          ' gia' salvate, che usano ChannelId: qui cambiano solo le etichette visualizzate
          pIsMath = True
          pShortName = "PerfQ"
          pLongName = "Performance Quality"
        Case eCanaliChiave.eEnvironmentQuality
          pIsMath = True
          pShortName = "EnvQ"
          pLongName = "Environment Quality"
        Case eCanaliChiave.ePositionHeading
          pIsMath = True
          pShortName = "HdgByPos"
          pLongName = "Position Heading"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eAttitudeQuality
          pIsMath = True
          pShortName = "AttQ"
          pLongName = "Attitude Quality"
        Case eCanaliChiave.eSailingState
          pIsMath = True
          pShortName = "SailingState"
          pLongName = "Sailing State"
        Case eCanaliChiave.eRudderDelta
          pIsMath = True
          pShortName = "RdrTd"
          pLongName = "Rudder Tgt Delta"
        Case eCanaliChiave.eTrimDelta
          pIsMath = True
          pShortName = "TrmTd"
          pLongName = "Trim Tgt Delta"
        Case eCanaliChiave.eRdrRake
          pIsMath = False
          pShortName = "RdrRk"
          pLongName = "Rudder Rake"
        Case eCanaliChiave.eFlyingStatus
          pIsMath = True
          pShortName = "FyS"
          pLongName = "Flying Status"
        Case eCanaliChiave.eHeelDelta
          pIsMath = True
          pShortName = "HeelTd"
          pLongName = "Heel Tgt Delta"
        Case eCanaliChiave.eSowSogDelta
          pIsMath = True
          pShortName = "SowSogD"
          pLongName = "Sow Sog Delta"
        Case eCanaliChiave.eSowSogK
          pIsMath = True
          pShortName = "SowSogK"
          pLongName = "Sow Sog K"
        Case eCanaliChiave.eLeewayRecalc
          pIsMath = True
          pShortName = "LwyRec"
          pLongName = "Leeway Recalc"
          pPolarHeader = "Leeway"
        Case eCanaliChiave.eLwyRec3s
          pIsMath = True
          pShortName = "LwyRec3s"
          pLongName = "Leeway Recalc 3s"
          pPolarHeader = "Leeway"
        Case eCanaliChiave.eLwyRec30s
          pIsMath = True
          pShortName = "LwyRec30s"
          pLongName = "Leeway Recalc 30s"
          pPolarHeader = "Leeway"
        Case eCanaliChiave.eDaggerAoA
          pIsMath = True
          pShortName = "DaggerAoA"
          pLongName = "DaggerBoard AoA"
          pPolarHeader = ""
        Case eCanaliChiave.eDaggerAoARL
          pIsMath = True
          pShortName = "DaggerAoARL"
          pLongName = "DaggerBoard AoA Rec Lwy"
          pPolarHeader = ""
        Case eCanaliChiave.eLwyK
          pIsMath = True
          pShortName = "LwyK"
          pLongName = "Leeway K"
        Case eCanaliChiave.eLwyK3s
          pIsMath = True
          pShortName = "LwyK3s"
          pLongName = "Leeway K 3s"
        Case eCanaliChiave.eLwyK30s
          pIsMath = True
          pShortName = "LwyK30s"
          pLongName = "Leeway K 30s"
        Case eCanaliChiave.eSeaState
          pShortName = "SeaState"
          pLongName = "Sea State"
          pKnownHeaders = New List(Of String)(New String() {"SeaStateIndex"})
          pDecimals = 1
        Case eCanaliChiave.eMwaNorm
          pIsMath = True
          pShortName = "MwaNorm"
          pLongName = "Mwa Normalized"
          pDecimals = 1
        Case eCanaliChiave.eAbsTwa
          pIsMath = True
          pShortName = "TwaAbs"
          pLongName = "TWA Abs value"
          pDecimals = 1
          pPolarHeader = "Twa"
          pBenchmarkHeader = "Twa"
        Case eCanaliChiave.eAbsAwa
          pIsMath = True
          pShortName = "AwaAbs"
          pLongName = "AWA Abs value"
          pDecimals = 1
        Case eCanaliChiave.eIsStbd
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eBoolean
        Case eCanaliChiave.eVMG
          pIsMath = True
          pShortName = "Vmg"
          pLongName = "Velocity Made Good"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Vmg"
          pBenchmarkHeader = "Vmg"
        Case eCanaliChiave.eVMGp
          pIsMath = True
          pShortName = "Vmg%"
          pLongName = "Velocity Made Good Perc"
          pShortUM = "%"
          pLongUM = "Perc"
          pDataType = clsChannel2020.eDataType.ePercentage
        Case eCanaliChiave.eBSTp
          pIsMath = True
          pShortName = "BsT%"
          pLongName = "Boat Speed Target Perc"
          pShortUM = "%"
          pLongUM = "Perc"
          pDataType = clsChannel2020.eDataType.ePercentage
        Case eCanaliChiave.eBSPp
          pIsMath = True
          pShortName = "Pol%"
          pLongName = "Polar Boat Speed Perc"
          pShortUM = "%"
          pLongUM = "Perc"
          pDataType = clsChannel2020.eDataType.ePercentage
        Case eCanaliChiave.eTWAd
          pIsMath = True
          pShortName = "TwaTd"
          pLongName = "True Wind Angle Target Delta"
        Case eCanaliChiave.eWindRatio
          pIsMath = True
          pShortName = "WindRatio"
          pLongName = "Bow TopMast Wind Ration"
          pShortUM = "%"
          pLongUM = "Perc"
          pDecimals = 0
        Case eCanaliChiave.eTwaNoLeeway
          pShortName = "Twa0L"
          pLongName = "Twa No Leeway"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
        Case eCanaliChiave.eTwaRecLeeway
          pShortName = "TwaRecLwy"
          pLongName = "Twa Rec Leeway"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
          pPolarHeader = "Twa"
          pBenchmarkHeader = "Twa"
        Case eCanaliChiave.eAwaRecLwy
          pShortName = "AwaRecLwy"
          pLongName = "Awa Rec Leeway"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
          pPolarHeader = "Awa"
          pBenchmarkHeader = "Awa"
        Case eCanaliChiave.eAwsRecLwy
          pShortName = "AwsRecLwy"
          pLongName = "Aws Rec Leeway"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
          pPolarHeader = "Aws"
          pBenchmarkHeader = "Aws"
        Case eCanaliChiave.eAwaRecNoLwy
          pShortName = "AwaRecNoLwy"
          pLongName = "Awa Rec No Leeway"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
          pPolarHeader = "Awa"
          pBenchmarkHeader = "Awa"
        Case eCanaliChiave.eHeadstayTotalLoad
          pShortName = "Headstay"
          pLongName = "Headstay Load"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "T"
          pLongUM = "Tons"
          pIsMath = True
        Case eCanaliChiave.eVmgRecLwy
          pShortName = "VmgRL"
          pLongName = "Vmg Rec Lwy"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
          pPolarHeader = "Vmg"
          pBenchmarkHeader = "Vmg"
        Case eCanaliChiave.eVmgShear
          pShortName = "VmgSh"
          pLongName = "Vmg Shear"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
          pPolarHeader = "Vmg"
          pBenchmarkHeader = "Vmg"
        Case eCanaliChiave.eLeewayModel
          pShortName = "LwyMdl"
          pLongName = "Leeway Model"
          pIsMath = True
          pPolarHeader = "Leeway"
        Case eCanaliChiave.eSeaStateNorm
          pShortName = "SeaStateNorm"
          pLongName = "Sea State Norm"
          pShortUM = ""
          pLongUM = ""
          pIsMath = True
          pDecimals = 2
        Case eCanaliChiave.eSeaStateNormDelta
          pShortName = "SeaStateNormDelta"
          pLongName = "Sea State Norm Delta"
          pShortUM = ""
          pLongUM = ""
          pIsMath = True
          pDecimals = 2
        Case eCanaliChiave.eVmgShearPerc
          pShortName = "VmgShP"
          pLongName = "Vmg Shear Perc"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
          pPolarHeader = "Vmg"
          pBenchmarkHeader = "Vmg"
        Case eCanaliChiave.eVmgPercRecLwy
          pShortName = "VmgPercRL"
          pLongName = "Vmg Perc Rec Lwy"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
        Case eCanaliChiave.eMsLwy
          pShortName = "MSlwy"
          pLongName = "MS Lwy"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
        Case eCanaliChiave.eMsTwa
          pShortName = "MsTwa"
          pLongName = "MS Twa"
          pDataType = clsChannel2020.eDataType.e180
          pIsMath = True
        Case eCanaliChiave.eMsVmg
          pShortName = "MsVmg"
          pLongName = "MS Vmg"
          pDataType = clsChannel2020.eDataType.eLinear
          pPolarHeader = "vmg"
          pBenchmarkHeader = "Vmg"
          pIsMath = True
        Case eCanaliChiave.eMsVmgPerc
          pShortName = "MsVmgPerc"
          pLongName = "MS Vmg Perc"
          pDataType = clsChannel2020.eDataType.e360
          pIsMath = True
        Case eCanaliChiave.eUpDnNetPress
          pShortName = "UpDnNetPress"
          pLongName = "UpDn Net Press"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
        Case eCanaliChiave.eTwaInertial
          pShortName = "TwaInr"
          pLongName = "Twa Inertial"
          pDataType = clsChannel2020.eDataType.e180
          pPolarHeader = "twa"
          pBenchmarkHeader = "Twa"
          pIsMath = True
        Case eCanaliChiave.eSowInertial
          pShortName = "SowInr"
          pLongName = "Sow Inertial"
          pDataType = clsChannel2020.eDataType.e180
          pPolarHeader = "bs"
          pBenchmarkHeader = "Bs"
          pIsMath = True
        Case eCanaliChiave.eTwaInertialRecLeeway
          pShortName = "TwaInrRL"
          pLongName = "Twa Inertial RecLeew"
          pDataType = clsChannel2020.eDataType.e180
          pBenchmarkHeader = "Twa"
          pPolarHeader = "twa"
          pIsMath = True
        Case eCanaliChiave.eVmgInertial
          pShortName = "VmgInr"
          pLongName = "Vmg Intertial"
          pDataType = clsChannel2020.eDataType.eLinear
          pPolarHeader = "vmg"
          pBenchmarkHeader = "Vmg"
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmgInertialRecLeeway
          pShortName = "VmgInrRL"
          pLongName = "Vmg Intertial RecLeew"
          pDataType = clsChannel2020.eDataType.eLinear
          pPolarHeader = "vmg"
          pBenchmarkHeader = "Vmg"
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmgPercInertial
          pShortName = "VmgInrPerc"
          pLongName = "VmgPerc Intertial"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
        Case eCanaliChiave.eVmgPercInertialRecLeeway
          pShortName = "VmgInrPercRL"
          pLongName = "VmgPerc Intertial RecLeew"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
        Case eCanaliChiave.eSailSet
          pShortName = "SailSet"
          pLongName = "SailSet"
          pDataType = clsChannel2020.eDataType.eLinear
          pIsMath = True
          pShortUM = ""
          pLongUM = ""
        Case eCanaliChiave.eVmc40
          pShortName = "Vmc40"
          pLongName = "Vmc 40 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc50
          pShortName = "Vmc50"
          pLongName = "Vmc 50 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc60
          pShortName = "Vmc60"
          pLongName = "Vmc 60 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc70
          pShortName = "Vmc70"
          pLongName = "Vmc 70 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pBenchmarkHeader = "Twa70"
          pIsMath = True
        Case eCanaliChiave.eVmc80
          pShortName = "Vmc80"
          pLongName = "Vmc 80 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc90
          pShortName = "Vmc90"
          pLongName = "Vmc 90 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pBenchmarkHeader = "Twa90"
          pIsMath = True
        Case eCanaliChiave.eVmc100
          pShortName = "Vmc100"
          pLongName = "Vmc 100 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc110
          pShortName = "Vmc110"
          pLongName = "Vmc 110 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pBenchmarkHeader = "Twa110"
          pIsMath = True
        Case eCanaliChiave.eVmc120
          pShortName = "Vmc120"
          pLongName = "Vmc 120 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc130
          pShortName = "Vmc130"
          pLongName = "Vmc 130 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc140
          pShortName = "Vmc140"
          pLongName = "Vmc 140 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eVmc150
          pShortName = "Vmc150"
          pLongName = "Vmc 150 Twa"
          pDataType = clsChannel2020.eDataType.eLinear
          pShortUM = "K"
          pLongUM = "Kts"
          pIsMath = True
        Case eCanaliChiave.eCSE
          pShortName = "Cse"
          pLongName = "Course Over Water"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eRudderAoA
          pIsMath = True
          pShortName = "RudderAoA"
          pLongName = "Rudder Angle of Attack"
        Case eCanaliChiave.eRdrRecLwyAoA
          pIsMath = True
          pShortName = "RudderAoARecLwy"
          pLongName = "Rudder Angle of Attack RecLwy"
        Case eCanaliChiave.eAwaWithLwy
          pIsMath = True
          pShortName = "AwaLwy"
          pLongName = "Apparent wind angle with lwy"
          pIsMath = True
        Case eCanaliChiave.eTrimRT
          pShortName = "TrmRt"
          pLongName = "Trim Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
        Case eCanaliChiave.eHeelRT
          pShortName = "HeelRt"
          pLongName = "Heel Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
        Case eCanaliChiave.eTravellerTack
          pIsMath = True
          pPolarHeader = "TravellerTack"
          pLongName = "TravT"
        Case eCanaliChiave.eTp52MainLeechLoad
          pIsMath = True
          pPolarHeader = ""
          pLongName = "Tp52 MainSail Leech Load"
          pShortName = "MsLeechLd"
          pDecimals = 1
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eTp52MainLockLoad
          pIsMath = True
          pPolarHeader = ""
          pLongName = "Tp52 MainSail Lock Load"
          pShortName = "MsLockLd"
          pDecimals = 1
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eAwaTopMast
          pIsMath = True
          pPolarHeader = "AwaTopMast"
          pDataType = clsChannel2020.eDataType.eAbs180
        Case eCanaliChiave.eAwsTopMast
          pIsMath = True
          pPolarHeader = "AwsTopMast"
        Case eCanaliChiave.eAwa2m
          pIsMath = True
          pPolarHeader = "Awa2m"
          pDataType = clsChannel2020.eDataType.eAbs180
        Case eCanaliChiave.eAws2m
          pIsMath = True
          pPolarHeader = "Aws2m"
        Case eCanaliChiave.eAwa_J25
          pIsMath = True
          pPolarHeader = "eAwa_J25"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_J25
          pIsMath = True
          pPolarHeader = "eAws_J25"
        Case eCanaliChiave.eAwa_J50
          pIsMath = True
          pPolarHeader = "eAwa_J50"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_J50
          pIsMath = True
          pPolarHeader = "eAws_J50"
        Case eCanaliChiave.eAwa_J75
          pIsMath = True
          pPolarHeader = "eAwa_J75"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_J75
          pIsMath = True
          pPolarHeader = "eAws_J75"
        Case eCanaliChiave.eAwa_M25
          pIsMath = True
          pPolarHeader = "eAwa_M25"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_M25
          pIsMath = True
          pPolarHeader = "eAws_M25"
        Case eCanaliChiave.eAwa_M50
          pIsMath = True
          pPolarHeader = "eAwa_M50"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_M50
          pIsMath = True
          pPolarHeader = "eAws_M50"
        Case eCanaliChiave.eAwa_M75
          pIsMath = True
          pPolarHeader = "eAwa_M75"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_M75
          pIsMath = True
          pPolarHeader = "eAws_M75"
        Case eCanaliChiave.eAwa_M87
          pIsMath = True
          pPolarHeader = "eAwa_M87"
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eAws_M87
          pIsMath = True
          pPolarHeader = "eAws_M87"
        Case eCanaliChiave.eRoll
        Case eCanaliChiave.ePitch
          pShortName = "Pitch"
          pLongName = "Pitch"
          pPolarHeader = "Pitch"
        Case eCanaliChiave.eRtCurrDir
          pShortName = "RtCurrDir"
          pLongName = "RT Current Direction"
          pShortUM = "°"
          pLongUM = "Deg"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eRtCurrRate
          pShortName = "RtCurrRate"
          pLongName = "RT Current Rate"
          pShortUM = "k"
          pLongUM = "Kts"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eCurrDirRec
          pShortName = "CurrDirRec"
          pLongName = "Current Direction Rec"
          pShortUM = "°"
          pLongUM = "Deg"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.eCurrRateRec
          pShortName = "CurrRateRec"
          pLongName = "Current Rate Rec"
          pShortUM = "k"
          pLongUM = "Kts"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eJibTwistFromFoot_25
          pShortName = "Jib_25_TwistFromFoot"
          pLongName = "Jib_25_TwistFromFoot"
          pShortUM = "°"
          pLongUM = "Deg"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eJibTwistFromFoot_50
          pShortName = "Jib_50_TwistFromFoot"
          pLongName = "Jib_50_TwistFromFoot"
          pShortUM = "°"
          pLongUM = "Deg"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eLinear
        Case eCanaliChiave.eJibTwistFromFoot_75
          pShortName = "Jib_75_TwistFromFoot"
          pLongName = "Jib_75_TwistFromFoot"
          pShortUM = "°"
          pLongUM = "Deg"
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eLinear
        Case Else
          Stop
      End Select

      Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pBenchmarkHeader, pIsMath, pDecimals) ' , Me)
      pListaCanaliChiave.Add(CanaleTmp)
    Next


  End Sub

  Private Sub CaricaCanaliConosciutiJson()
    Dim CanaliConosciutiJson = clsKillerSeriale.LoadConfigurationGeneric(Of List(Of clsChannel2020))(IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.ChannelConfigFileName))
    If CanaliConosciutiJson Is Nothing Then
      ' serviva nella migrazione da xml a json
      'CaricaCanaliConosciuti()
      'If Not IO.Directory.Exists(_PathConfigChannel) Then
      '  IO.Directory.CreateDirectory(_PathConfigChannel)
      'End If
      'SalvaCanaliJson()
    Else
      pListaCanaliConosciuti = CanaliConosciutiJson
    End If
  End Sub

  'Private Sub CaricaCanaliConosciuti()
  '  Dim NodoCanali As Xml.XmlNode = AppConfig.CercaNodo(pParentDataProvider.SuffissoFileType, clsSettings.eNodoSTD.eChannels, True)
  '  pListaCanaliConosciuti.Clear()

  '  If Not NodoCanali Is Nothing Then
  '    For Each Nodo As Xml.XmlNode In NodoCanali
  '      Dim pChannelId As String = Nodo.Name.Substring(8) 'i nodi dei canali iniziano con "Channel_"
  '      Dim CanaleChiave As clsChannels2020.eCanaliChiave = eCanaliChiave.eNone
  '      Dim pShortName As String = pChannelId
  '      Dim pLongName As String = pChannelId
  '      Dim pShortUM As String = "°"
  '      Dim pLongUM As String = "Deg"
  '      Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
  '      Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
  '      Dim pActualLogHeader As String = pKnownHeaders.First
  '      Dim pPolarHeader As String = ""
  '      Dim pImporta As Boolean = False
  '      Dim pIsMath As Boolean = False
  '      Dim pDecimals As Integer = 1
  '      For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
  '        Select Case SottoNodo.Name
  '          Case "DataType"
  '            pDataType = CInt(SottoNodo.InnerText)
  '          Case "LongName"
  '            pLongName = SottoNodo.InnerText
  '          Case "ShortName"
  '            pShortName = SottoNodo.InnerText
  '          Case "LongUM"
  '            pLongUM = SottoNodo.InnerText
  '          Case "ShortUM"
  '            pShortUM = SottoNodo.InnerText
  '          Case "Load"
  '            pImporta = SottoNodo.InnerText
  '          Case "Decimals"
  '            pDecimals = CInt(SottoNodo.InnerText)
  '          Case "KeyChannel"
  '            CanaleChiave = CInt(SottoNodo.InnerText.Trim)
  '          Case "Headers"
  '            pKnownHeaders = HeadersDaStringa(SottoNodo.InnerText.Trim)
  '          Case "PolarHeader"
  '            pPolarHeader = SottoNodo.InnerText
  '          Case "IsMath"
  '            pIsMath = CBool(SottoNodo.InnerText)
  '          Case Else
  '            Stop
  '        End Select
  '      Next
  '      Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pIsMath, pDecimals) ', Me)
  '      'CanaleTmp.Importa = pImporta
  '      pListaCanaliConosciuti.Add(CanaleTmp)
  '    Next
  '  End If
  'End Sub

  'Private Function HeadersDaStringa(StringaHeaders As String) As List(Of String)
  '  If StringaHeaders.IndexOf(",") > -1 Then
  '    Return StringaHeaders.Split(",").ToList
  '  Else
  '    Return New List(Of String)({StringaHeaders})
  '  End If
  'End Function

  Public Sub MappaCanali(Intestazioni As List(Of String))
    'cerca l 'intestazione nei canali di default
    'se non trova l 'intestazione carica dei valori dei default
    ' cerca l 'intestazione nei canali conosciuti
    'se non la trova lascia le impostazioni di default e carica il canale
    _ListaCanali.Clear()
    SyncLock pLockDizionario
      pDictionaryCanaliPerNome.Clear()
    End SyncLock
    pDictionaryCanali.Clear()
    Dim NuoviCanali As Integer = 0
    For Each Intestazione In Intestazioni
      'cerca nei canali di default disponibili solo lato codice
      Dim CanaleTmp As clsChannel2020 = pListaCanaliChiave.Where(Function(x) x.VerificaKnownHeader(Intestazione)).FirstOrDefault
      'sovrascrive i settaggi se messi nel file di configurazione
      Dim CanaleConosciuto = pListaCanaliConosciuti.Where(Function(x) x.VerificaKnownHeader(Intestazione)).FirstOrDefault
      If CanaleConosciuto Is Nothing Then
        'il canale non é mai stato mappato prima
        NuoviCanali += 1
        If CanaleTmp Is Nothing Then
          CanaleTmp = CreaNuovoCanaleJson(Intestazione)
          pListaCanaliConosciuti.Add(CanaleTmp)
        Else
          'Stop
          pListaCanaliConosciuti.Add(CanaleTmp)
          'CanaleTmp.Importa = Not CanaleTmp.CanaleChiave = eCanaliChiave.eNone
          'CanaleTmp.SalvaSuXML(pParentDataProvider.SuffissoFileType, False)
        End If
      Else
        CanaleTmp = CanaleConosciuto 'in ogni caso se é stato mappato si prende la sua configurazione
      End If
      CanaleTmp.ActualLogHeader = Intestazione
      _ListaCanali.Add(CanaleTmp)
    Next
    For Each Ch In pListaCanaliChiave.Where(Function(x) x.IsMath = True).ToList
      If Intestazioni.Where(Function(x) x.ToLower = Ch.ChannelId.ToLower).FirstOrDefault = Nothing Then
        _ListaCanali.Add(Ch)
      End If
    Next
    If NuoviCanali > 0 Then
      SalvaCanaliJson()

      'AppConfig.SalvaFileXML()
    End If
  End Sub

  'Private Function CreaNuovoCanale(Intestazione As String) As clsChannel2020
  '  Dim pChannelId As String = Intestazione
  '  Dim CanaleChiave As clsChannels2020.eCanaliChiave = eCanaliChiave.eNone
  '  Dim pShortName As String = pChannelId
  '  Dim pLongName As String = pChannelId
  '  Dim pShortUM As String = "°"
  '  Dim pLongUM As String = "Deg"
  '  Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
  '  Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
  '  Dim pActualLogHeader As String = pKnownHeaders.First
  '  Dim pPolarHeader As String = ""
  '  Dim pIsMath As Boolean = False
  '  Dim pDecimals As Integer = 1
  '  Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pIsMath, pDecimals) ', Me)
  '  'CanaleTmp.Importa = False 'Not CanaleChiave = eCanaliChiave.eNone
  '  CanaleTmp.SalvaSuXML(pParentDataProvider.SuffissoFileType, False)
  '  Return CanaleTmp
  'End Function

  Private Function CreaNuovoCanaleJson(Intestazione As String) As clsChannel2020
    Dim pChannelId As String = Intestazione
    Dim CanaleChiave As clsChannels2020.eCanaliChiave = eCanaliChiave.eNone
    Dim pShortName As String = pChannelId
    Dim pLongName As String = pChannelId
    Dim pShortUM As String = "°"
    Dim pLongUM As String = "Deg"
    Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
    Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
    Dim pActualLogHeader As String = pKnownHeaders.First
    Dim pPolarHeader As String = ""
    Dim pBenchmarkHeader As String = ""
    Dim pIsMath As Boolean = False
    Dim pDecimals As Integer = 1
    Dim CanaleTmp As New clsChannel2020(pChannelId, CanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pBenchmarkHeader, pIsMath, pDecimals) ', Me)
    'CanaleTmp.Importa = False 'Not CanaleChiave = eCanaliChiave.eNone
    Return CanaleTmp
  End Function

  Public Sub SalvaCanaliJson()
    clsKillerSeriale.SaveConfigurationGeneric(Of List(Of clsChannel2020))(pListaCanaliConosciuti, IO.Path.Combine(AppConfig.ActiveProfile.SettingsFolder, AppConfig.ActiveProfile.ChannelConfigFileName))
  End Sub


  'Public Sub SalvaCanale(Canale As clsChannel2020, SalvaXml As Boolean)
  '  If SalvaXml Then
  '    Canale.SalvaSuXML(pParentDataProvider.SuffissoFileType, SalvaXml)
  '  End If
  'End Sub

  'Public Function ToggleCanaliDaImportare() As String
  '  Dim strTmp As String = ""
  '  Select Case pToggleImporta
  '    'Case eToggleImporta.eDefault
  '    '  pToggleImporta = eToggleImporta.eKeyChannelsOnly
  '    '  strTmp = "Toggle Channels (Key)"
  '    Case eToggleImporta.eKeyChannelsOnly
  '      pToggleImporta = eToggleImporta.eAllChannels
  '      strTmp = "Toggle Channels (All)"
  '    Case eToggleImporta.eAllChannels
  '      pToggleImporta = eToggleImporta.eNone
  '      strTmp = "Toggle Channels (None)"
  '    Case eToggleImporta.eNone
  '      pToggleImporta = eToggleImporta.eKeyChannelsOnly
  '      strTmp = "Toggle Channels (Dflt)"
  '  End Select
  '  SetCanaliDaImportare()
  '  Return strTmp
  'End Function

  'Public Sub SetCanaliDaImportare()
  '  For Each Canale As clsChannel2020 In _ListaCanaliFileCorrente
  '    Select Case pToggleImporta
  '      'Case eToggleImporta.eDefault
  '      '  Canale.Importa = DefaultValueOfImportaCanale(Canale.IdIntestazione)
  '      Case eToggleImporta.eKeyChannelsOnly
  '        Canale.Importa = Not (Canale.CanaleChiave = eCanaliChiave.eNone)
  '      Case eToggleImporta.eAllChannels
  '        Canale.Importa = True
  '      Case eToggleImporta.eNone
  '        Canale.Importa = False
  '    End Select
  '  Next

  'End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsDataCorrections

  'clsDataCorrections.ApplicaCorrezioni
  Public Shared Sub ApplicaCorrezioni(TimeRange As clsTimeRange)
    'ApplyTrimSignInversion(TimeRange)
    'Exit Sub


    'ApplyTrimOffset(TimeRange, -1.31)
    'Exit Sub

    Dim BsCoeffPort As Double = 0.99
    Dim BsCoeffStbd As Double = 1.01
    Dim AwsCoeff As Double = 1
    Dim AwaOffset As Double = -1.5
    Dim HdgOffset As Double = 0
    Dim TwaUpwashUp As Double = 0.0
    Dim TwaUpwashRc As Double = 0
    Dim TwaUpwashDn As Double = 0.0
    ApplyCalibs(TimeRange, BsCoeffPort, BsCoeffStbd, AwsCoeff, AwaOffset, HdgOffset, TwaUpwashUp, TwaUpwashRc, TwaUpwashDn)
  End Sub

  Public Shared Sub ApplyCalibs(TimeRange As clsTimeRange, BsCoeffPort As Double, BsCoeffStbd As Double, AwsCoeff As Double, AwaOffset As Double, HdgOffset As Double, TwaUpwashUp As Double, TwaUpwashRc As Double, TwaUpwashDn As Double)
    Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chAws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWS)
    Dim chAwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chCse Is Nothing Then
      chCse = DataProvider2020.CseFromHdgAndLeeway()
    End If
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)

    Dim Twa, Tws, Twd As Double
    Dim TackSign = 1
    For i As Long = TimeRange.IdRigaIniziale To TimeRange.IdRigaFinale
      chAws.Valori(i) *= AwsCoeff
      chAwa.Valori(i) += AwaOffset

      TackSign = 1
      If (chAwa.Valori(i) < 0) Then TackSign = -1
      If (TackSign > 0) Then
        chBs.Valori(i) *= BsCoeffStbd
      Else
        chBs.Valori(i) *= BsCoeffPort
      End If

      chHdg.Valori(i) = SommaAngolo180adAngolo360(HdgOffset, chHdg.Valori(i))
      chCse.Valori(i) = SommaAngolo180adAngolo360(HdgOffset, chCse.Valori(i))
      If AwaOffset <> 0 OrElse AwsCoeff <> 1 Then
        TrueFromApparent(Twa, Tws, chAwa.Valori(i), chAws.Valori(i), chBs.Valori(i))
        Twa = TackSign * Math.Abs(Twa)
      Else
        Twa = chTwa.Valori(i)
        Tws = chTws.Valori(i)
      End If
      If Math.Abs(Twa) > 130 Then
        Twa += TackSign * TwaUpwashDn
        Twd = SommaAngolo180adAngolo360(TackSign * TwaUpwashDn, chTwd.Valori(i))
      ElseIf Math.Abs(Twa) > 60 Then
        Twa += TackSign * TwaUpwashRc
        Twd = SommaAngolo180adAngolo360(TackSign * TwaUpwashRc, chTwd.Valori(i))
      Else
        Twa += TackSign * TwaUpwashUp
        Twd = SommaAngolo180adAngolo360(TackSign * TwaUpwashUp, chTwd.Valori(i))
      End If
      chTwa.Valori(i) = Twa
      chTws.Valori(i) = Tws
      chTwd.Valori(i) = Twd
    Next



    ClearChannelValues(clsChannels2020.eCanaliChiave.eVMG)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eVMGp)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eVmgShear)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eVmgShearPerc)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eSowSogDelta)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eSowSogK)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eLeewayRecalc)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eVmgRecLwy)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eVmgPercRecLwy)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eTWAd)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eTwaNoLeeway)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eTwaRecLeeway)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAbsAwa)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAbsTwa)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwaRecLwy)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwsRecLwy)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwaRecNoLwy)

    ClearChannelValues(clsChannels2020.eCanaliChiave.eCurrRateRec)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eCurrDirRec)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eRtCurrRate)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eRtCurrDir)



    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa2m)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwaTopMast)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwaWithLwy)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_J25)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_J50)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_J75)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_M25)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_M50)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_M75)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwa_M87)

    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws2m)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAwsTopMast)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_J25)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_J50)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_J75)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_M25)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_M50)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_M75)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eAws_M87)

    ClearChannelValues(clsChannels2020.eCanaliChiave.eTws10mAlfaConst)
    ClearChannelValues(clsChannels2020.eCanaliChiave.eTws10mAlfaVar)





  End Sub


  Public Shared Sub ClearChannelValues(CanaleChiave As clsChannels2020.eCanaliChiave)
    Dim ch As clsChannel2020 = DataProvider2020.CanaleDbl(CanaleChiave)
    If Not ch Is Nothing Then
      If Not ch.Valori Is Nothing Then
        If ch.Valori.Count > 0 Then
          ReDim ch.Valori(0)
          ch = DataProvider2020.CanaleDbl(CanaleChiave)
        End If
      End If
    End If
  End Sub


  ''' <summary>
  ''' Invalida i tre canali di qualita' e li fa ricalcolare con le impostazioni correnti.
  ''' Stesso meccanismo delle correzioni: si svuota l'array dei valori, cosi' il successivo
  ''' accesso via CanaleDbl trova il canale scarico e ne richiama il calcolo math.
  ''' </summary>
  Public Shared Sub RicalcolaCanaliQualita()
    If DataProvider2020 Is Nothing Then Exit Sub

    ' il SeaState alimenta le soglie dei canali legati al mare: va invalidato per primo,
    ' altrimenti resterebbe quello calcolato con i parametri precedenti
    '    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eSeaState)

    ' i tre canali vanno svuotati tutti prima di toccarne uno: il calcolo li produce
    ' insieme, e un ClearChannelValues per volta li farebbe rigenerare tre volte
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eAttitudeQuality)
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eDataQuality)

    ' un solo accesso rigenera tutti e tre gli array
    Dim chTmp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eDataQuality)
  End Sub

  ''' <summary>
  ''' Svuota l'array di un canale senza richiamarne subito il ricalcolo, cosi' da poter
  ''' invalidare piu' canali prodotti dallo stesso calcolo prima di rigenerarli una volta sola.
  ''' </summary>
  Private Shared Sub SvuotaSenzaRicalcolo(CanaleChiave As clsChannels2020.eCanaliChiave)
    SvuotaCanale(DataProvider2020.Channels.Canale(CanaleChiave))
  End Sub

  ''' <summary>
  ''' Invalida e ricalcola tutti i canali che dipendono dalla polare, da usare dopo il
  ''' caricamento di un target file diverso.
  ''' Comprende i canali generati "_Tgt" e "_Pol", i math che li consumano (BSp, VMGp) e,
  ''' a cascata, i tre indici di qualita' che si basano sulla velocita' in percentuale.
  ''' </summary>
  Public Shared Sub RicalcolaCanaliPolare()
    If DataProvider2020 Is Nothing Then Exit Sub
    If DataProvider2020.Channels Is Nothing Then Exit Sub
    If DataProvider2020.Channels.ListaCanali Is Nothing Then Exit Sub

    ' i canali target e polare sono generati con questi suffissi a partire dal PolarHeader
    For Each ch As clsChannel2020 In DataProvider2020.Channels.ListaCanali
      If ch Is Nothing Then Continue For
      If ch.ChannelId Is Nothing Then Continue For
      If ch.ChannelId.EndsWith("_Tgt", StringComparison.CurrentCultureIgnoreCase) OrElse
         ch.ChannelId.EndsWith("_Pol", StringComparison.CurrentCultureIgnoreCase) Then
        SvuotaCanale(ch)
      End If
    Next

    ' math che leggono i canali target
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eBSPp)
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eVMGp)

    ' la performance quality si basa sul BSp: va rifatta anche lei
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eAttitudeQuality)
    SvuotaSenzaRicalcolo(clsChannels2020.eCanaliChiave.eDataQuality)

    ' un accesso per famiglia rigenera i valori con la nuova polare
    Dim chTmp As clsChannel2020
    chTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
    chTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    chTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eDataQuality)
  End Sub

  Private Shared Sub SvuotaCanale(ch As clsChannel2020)
    If ch Is Nothing Then Exit Sub
    If ch.Valori Is Nothing Then Exit Sub
    If ch.Valori.Count = 0 Then Exit Sub
    ReDim ch.Valori(0)
  End Sub

  Public Shared Sub SalvaCopiaParquet()
    LoadingProgressVisualizza()
    For Each f In DataProvider2020.ParquetFiles
      Dim N As New clsParquetCopy(f.FileInfo.FullName, "Corrected")
    Next
    LoadingProgressNascondi()
    ApriExplorer(DataProvider2020.ParquetFiles(0).FileInfo.Directory.FullName)

  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsChannel2020
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property IdIntestazione As Integer
  Public Property ChannelId As String
  Public Property CanaleChiave As clsChannels2020.eCanaliChiave
  ' Nomi: quelli memorizzati (e salvati nel json dei canali) restano "puri", senza suffisso.
  ' ShortName / LongName sono i nomi VISUALIZZATI: per i canali math aggiungono "_m" solo in lettura.
  Public Const SuffissoMath As String = "_m"
  Dim _ShortName As String
  Dim _LongName As String

  ''' <summary>Nome breve memorizzato, senza il suffisso _m. E' quello che finisce nel json (chiave "ShortName", come prima).</summary>
  <JsonProperty("ShortName")>
  Public Property ShortNameMemorizzato As String
    Get
      Return _ShortName
    End Get
    Set(value As String)
      _ShortName = value
    End Set
  End Property

  ''' <summary>Nome lungo memorizzato, senza il suffisso _m. E' quello che finisce nel json (chiave "LongName", come prima).</summary>
  <JsonProperty("LongName")>
  Public Property LongNameMemorizzato As String
    Get
      Return _LongName
    End Get
    Set(value As String)
      _LongName = value
    End Set
  End Property

  ''' <summary>Nome breve visualizzato (con _m se il canale e' math).</summary>
  <JsonIgnore>
  Public Property ShortName As String
    Get
      Return ConSuffissoMath(_ShortName)
    End Get
    Set(value As String)
      _ShortName = SenzaSuffissoMath(value)
    End Set
  End Property

  ''' <summary>Nome lungo visualizzato (con _m se il canale e' math).</summary>
  <JsonIgnore>
  Public Property LongName As String
    Get
      Return ConSuffissoMath(_LongName)
    End Get
    Set(value As String)
      _LongName = SenzaSuffissoMath(value)
    End Set
  End Property

  Private Function ConSuffissoMath(Nome As String) As String
    If Nome Is Nothing OrElse Not IsMath OrElse Nome.EndsWith(SuffissoMath) Then Return Nome
    Return Nome & SuffissoMath
  End Function

  ' chi modifica il nome a mano parte da quello visualizzato: il suffisso non va memorizzato
  Private Function SenzaSuffissoMath(Nome As String) As String
    If Nome Is Nothing OrElse Not IsMath OrElse Not Nome.EndsWith(SuffissoMath) Then Return Nome
    Return Nome.Substring(0, Nome.Length - SuffissoMath.Length)
  End Function

  Public Property ShortUM As String
  Public Property LongUM As String
  Public Property DataType As eDataType
  Public Property ActualLogHeader As String
  Public Property KnownHeaders As List(Of String)
  Public Property PolarHeader As String
  Public Property BenchmarkHeader As String
  Public Property IsMath As Boolean
  Public Property Export As Boolean = False
  Public Property Foreground As New SolidColorBrush(Colors.Black)



  Dim _PrintedColor As Color
  Public Property PrintedColor As Color
    Get
      Return _PrintedColor
    End Get
    Set(value As Color)
      _PrintedColor = value
      Foreground = New SolidColorBrush(_PrintedColor)
    End Set
  End Property

  Public Property MinVal As Double = Double.NaN
  Public Property MaxVal As Double = Double.NaN
  Public Property AlertDelta As Double = Double.NaN

  Public Property Decimals As Integer
  Public Property IsSelected As Boolean
  <JsonIgnore>
  Public Property ValoriDT As DateTime()
  <JsonIgnore>
  Public Property Valori As Double() = Nothing

  '<JsonIgnore>
  'Dim pListaParentChannels As clsChannels2020

  'Dim ValoriIntervallo As New clsValoriPeriodoCanale2020(Me, Nothing, False)

  <JsonIgnore>
  Public Property StatisticheIntervallo As New clsStatisticheIntervallo(Me)
  <JsonIgnore>
  Public Property StraightLineChartSync As clsStraightLineChartSync
  <JsonIgnore>
  Public Property UseShort As Boolean = True
  <JsonIgnore>
  Public Property CurrentValue As Double
  <JsonIgnore>
  Public Property StripeParameter As String
  <JsonIgnore>
  Public Property StripeHeight As String
  <JsonIgnore>
  Public Property SailName As String

  Public Property Visibility As Visibility = Visibility.Visible

  Public Sub New(ChannelId As String, CanaleChiave As clsChannels2020.eCanaliChiave, ShortName As String, LongName As String,
        ShortUM As String, LongUM As String, DataType As eDataType, ActualLogHeader As String, KnownHeaders As List(Of String),
        PolarHeader As String, BenchmarkHeader As String, IsMath As Boolean, Decimals As Integer) ' , ListaParentChannels As clsChannels2020)

    Me.ChannelId = ChannelId
    StatisticheIntervallo.ChannelId = ChannelId
    Me.CanaleChiave = CanaleChiave
    ' nomi memorizzati puri: il suffisso _m dei canali math si aggiunge solo in lettura (ShortName / LongName)
    Me._ShortName = ShortName
    Me._LongName = LongName
    Me.ShortUM = ShortUM
    Me.LongUM = LongUM
    Me.DataType = DataType
    Me.ActualLogHeader = ActualLogHeader
    Me.KnownHeaders = KnownHeaders
    Me.PolarHeader = PolarHeader
    Me.BenchmarkHeader = BenchmarkHeader
    Me.IsMath = IsMath
    Me.Decimals = Decimals
    'pImporta = False
    Me.IsSelected = False
    Me.Export = False
    'pListaParentChannels = ListaParentChannels
  End Sub

  Public Sub New()
    Me.Valori = Nothing
  End Sub

  Public Enum eDataType
    eLinear = 0
    e180 = 1
    e360 = 2
    eAbsLinear = 3
    eAbs180 = 4
    eTack = 5
    eBoolean = 6
    ePercentage = 7
    eDateTime = 8
    eDateOnly = 9
    eTimeOnly = 10
    eTackReversed = 11
    eDaySeconds = 12
    eDays = 13
  End Enum

  'Dim _StrCurrentValue As String
  Public ReadOnly Property StrCurrentValue As String
    Get
      'Return _StrCurrentValue
      Return CurrentValue.ToString("F" & Decimals)
    End Get
    'Set(value As String)
    '  _StrCurrentValue = value
    'End Set
  End Property

  'Public Property DataType As eDataType
  '  Get
  '    Return pDataType
  '  End Get
  '  Set(value As eDataType)
  '    pDataType = value
  '    OnPropertyChanged("DataType")
  '  End Set
  'End Property

  '<JsonIgnore>
  'Public Property ValoriDT As Date()
  '  Get
  '    Return ValoriDT
  '  End Get
  '  Set(value As Date())
  '    ValoriDT = value
  '  End Set
  'End Property

  Public ReadOnly Property FirstNotNan(Indice As Integer) As Double
    Get
      Dim v = Valori(Indice)
      If Double.IsNaN(v) Then
        Dim i As Integer = 1
        Do
          v = Valori(Indice + i)
          If Not Double.IsNaN(v) Then
            Return v
          End If
          i += 1
        Loop While i + Indice < Valori.Count - 1
        Return v
      Else
        Return v
      End If
    End Get
  End Property

  '<JsonIgnore>
  'Public Property Valori As Double()
  '  Get
  '    'If ChannelId = "AccX" Then Stop
  '    Return Valori
  '  End Get
  '  Set(value As Double())
  '    'If ChannelId = "AccX" Then Stop
  '    Valori = value
  '  End Set
  'End Property

  Public Function HasNotNanValues() As Boolean
    Return Valori.Where(Function(x) Not Double.IsNaN(x)).Count > 0
  End Function

  Public Function HasNotNanValuesOrZero() As Boolean
    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
  End Function

  Public Function PrimoValoreNotNan(Indice As Double) As Double
    Dim v As Double = Valori(Indice)
    If Double.IsNaN(v) Then
      For i As Integer = Indice + 1 To Math.Min(Indice + 1000, Valori.Count - 1)
        v = Valori(i)
        If Not Double.IsNaN(v) Then Return v
      Next
      ' se arriva qui non ha trovato valori successivi validi, fa il giro a ritroso
      For i As Integer = Valori.Count - 1 To 0 Step -1
        v = Valori(i)
        If Not Double.IsNaN(v) Then Return v
      Next
      Return Double.NaN
    Else
      Return v
    End If
  End Function

  Public ReadOnly Property HasValues As Boolean
    Get
      If Not Valori Is Nothing Then Return Valori.Count > 0
      If Not ValoriDT Is Nothing Then Return ValoriDT.Count > 0
      'Return (Valori.Count + ValoriDT.Count) > 0
      Return False
    End Get
  End Property

  'Public ReadOnly Property AvgTest As String
  '  Get
  '    Return ValoriIntervallo.AvgString
  '  End Get
  'End Property

  'Public Property ChannelId As String
  '  Get
  '    Return pChannelId
  '  End Get
  '  Set(value As String)
  '    pChannelId = value
  '    OnPropertyChanged("ChannelId")
  '  End Set
  'End Property

  'Public Property CanaleChiave As clsChannels2020.eCanaliChiave
  '  Get
  '    Return CanaleChiave
  '  End Get
  '  Set(value As clsChannels2020.eCanaliChiave)
  '    CanaleChiave = value
  '    OnPropertyChanged("CanaleChiave")
  '    OnPropertyChanged("CanaleChiaveStringa")
  '  End Set
  'End Property

  Public ReadOnly Property CanaleChiaveStringa As String
    Get
      'If CanaleChiave.ToString = "58" Then Stop
      If CanaleChiave = clsChannels2020.eCanaliChiave.eNone Then
        Return "-"
      Else
        Return CanaleChiave.ToString.TrimStart("e")
      End If
    End Get
  End Property

  'Public Property ShortName As String
  '  Get
  '    Return pShortName
  '  End Get
  '  Set(value As String)
  '    pShortName = value
  '    OnPropertyChanged("ShortName")
  '    OnPropertyChanged("Name")
  '  End Set
  'End Property

  'Public Property LongName As String
  '  Get
  '    Return pLongName
  '  End Get
  '  Set(value As String)
  '    pLongName = value
  '    OnPropertyChanged("LongName")
  '    OnPropertyChanged("LongNameNoSpaces")
  '    OnPropertyChanged("Name")
  '  End Set
  'End Property

  'Public ReadOnly Property LongNameNoSpaces As String
  '  Get
  '    Return pLongName.Replace(" ", "")
  '  End Get
  'End Property

  Public ReadOnly Property Name As String
    Get
      If UseShort Then
        Return ShortName
      Else
        Return LongName
      End If
    End Get
  End Property

  Public ReadOnly Property UM As String
    Get
      If UseShort Then
        Return ShortUM
      Else
        Return LongUM
      End If
    End Get
  End Property

  'Public Property ShortUM As String
  '  Get
  '    Return pShortUM
  '  End Get
  '  Set(value As String)
  '    pShortUM = value
  '    'OnPropertyChanged("ShortUM")
  '    'OnPropertyChanged("UM")
  '  End Set
  'End Property

  'Public Property LongUM As String
  '  Get
  '    Return pLongUM
  '  End Get
  '  Set(value As String)
  '    pLongUM = value
  '    'OnPropertyChanged("LongUM")
  '    'OnPropertyChanged("UM")
  '  End Set
  'End Property

  'Public Property ActualLogHeader As String
  '  Get
  '    Return pActualLogHeader
  '  End Get
  '  Set(value As String)
  '    pActualLogHeader = value
  '    OnPropertyChanged("ActualLogHeader")
  '  End Set
  'End Property

  'Public Property KnownHeaders As List(Of String)
  '  Get
  '    Return pKnownHeaders
  '  End Get
  '  Set(value As List(Of String))
  '    pKnownHeaders = value
  '    OnPropertyChanged("KnownHeaders")
  '  End Set
  'End Property

  Public ReadOnly Property VerificaKnownHeader(Header As String) As Boolean
    Get
      For Each Hedr In KnownHeaders
        If Hedr.Trim.ToLower = Header.Trim.ToLower Then Return True
      Next
      Return False
    End Get
  End Property

  'Public Property PolarHeader As String
  '  Get
  '    Return pPolarHeader
  '  End Get
  '  Set(value As String)
  '    pPolarHeader = value
  '    OnPropertyChanged("PolarHeader")
  '  End Set
  'End Property

  'Public Property IsMath As Boolean
  '  Get
  '    Return pIsMath
  '  End Get
  '  Set(value As Boolean)
  '    pIsMath = value
  '    OnPropertyChanged("IsMath")
  '  End Set
  'End Property

  'Public Property Importa As Boolean
  '  Get
  '    Return pImporta
  '  End Get
  '  Set(value As Boolean)
  '    pImporta = value
  '    OnPropertyChanged("Importa")
  '  End Set
  'End Property

  'Public Property Foreground As Brush
  '  Get
  '    Return pForeground
  '  End Get
  '  Set(value As Brush)
  '    pForeground = value
  '    OnPropertyChanged("Foreground")
  '  End Set
  'End Property

  'Public Property Decimals As Integer
  '  Get
  '    Return pDecimals
  '  End Get
  '  Set(value As Integer)
  '    pDecimals = value
  '    OnPropertyChanged("Decimals")
  '  End Set
  'End Property

  'Public Property IsSelected As Boolean
  '  Get
  '    Return pIsSelected
  '  End Get
  '  Set(value As Boolean)
  '    pIsSelected = value
  '    OnPropertyChanged("IsTemporarySelected")
  '  End Set
  'End Property

  'Public Property ListaParentChannels As clsChannels2020
  '  Get
  '    Return pListaParentChannels
  '  End Get
  '  Set(value As clsChannels2020)
  '    pListaParentChannels = value
  '    OnPropertyChanged("ListaParentChannels")
  '  End Set
  'End Property

  'Public Property IdIntestazione As Integer
  '  Get
  '    Return pIdIntestazione
  '  End Get
  '  Set(value As Integer)
  '    pIdIntestazione = value
  '    OnPropertyChanged("IdIntestazione")
  '  End Set
  'End Property

  'Public Property ValoriIntervallo As clsValoriPeriodoCanale2020
  '  Get
  '    Return ValoriIntervallo
  '  End Get
  '  Set(value As clsValoriPeriodoCanale2020)
  '    ValoriIntervallo = value
  '    OnPropertyChanged("ValoriIntervallo")
  '  End Set
  'End Property

  Public ReadOnly Property ValoriIntervallo(IdIniziale As Integer, IdFinale As Integer, SoloNotNan As Boolean) As Double()
    Get
      Dim v = Valori.Skip(IdIniziale).Take(IdFinale - IdIniziale + 1).ToArray
      If SoloNotNan Then
        Return v.Where(Function(x) Not Double.IsNaN(x)).ToArray
      Else
        Return v
      End If
    End Get
  End Property

  Public ReadOnly Property ValoriIntervallo(TimeRange As clsTimeRange, SoloNotNan As Boolean) As Double()
    Get
      Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TimeRange.Start)
      Dim IdFinale As Integer = DataProvider2020.TrovaIndice(TimeRange.Finish)
      Return ValoriIntervallo(IdIniziale, IdFinale, SoloNotNan)
    End Get
  End Property

  Public ReadOnly Property IsNumeric As Boolean
    Get
      Select Case DataType
        Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly
          Return False
        Case Else
          Return True
      End Select
    End Get
  End Property

  Public ReadOnly Property SqlDbName As String
    Get
      If CanaleChiave = clsChannels2020.eCanaliChiave.eNone Then
        Return ChannelId
      Else
        Return "_" ' & System.Enum.GetName(GetType(clsChannels2020.eCanaliChiave), CanaleChiave).TrimStart("e")
      End If
    End Get
  End Property

  'Public Property StraightLineChartSync As clsStraightLineChartSync
  '  Get
  '    Return pStraightLineChartSync
  '  End Get
  '  Set(value As clsStraightLineChartSync)
  '    pStraightLineChartSync = value
  '  End Set
  'End Property

  'Public Property CurrentValue As String
  '  Get
  '    Return Format(pCurrentValue, "F" & pDecimals.ToString)
  '  End Get
  '  Set(value As String)
  '    pCurrentValue = value
  '    OnPropertyChanged("CurrentValue")
  '  End Set
  'End Property

  'Public Property Visibility As Visibility
  '  Get
  '    Return pVisibility
  '  End Get
  '  Set(value As Visibility)
  '    pVisibility = value
  '    OnPropertyChanged("Visibility")
  '  End Set
  'End Property

  'Public Sub ScatenaEventoStatisticheIntervallo()
  '  OnPropertyChanged("StatisticheIntervallo")
  'End Sub

  '<JsonIgnore>
  'Public Property StatisticheIntervallo As clsStatisticheIntervallo
  '  Get
  '    Return _StatisticheIntervallo
  '  End Get
  '  Set(value As clsStatisticheIntervallo)
  '    _StatisticheIntervallo = value
  '    OnPropertyChanged("StatisticheIntervallo")
  '  End Set
  'End Property

  'Public Sub ToggleNomi()
  '  pShort = Not pShort
  '  OnPropertyChanged("Name")
  '  OnPropertyChanged("UM")
  'End Sub

  'Public Function ValorePerSql(Riga As Integer) As Nullable(Of Double)
  '  Select Case pDataType
  '    Case eDataType.eBoolean
  '      If Valori.Count = 0 Then Return Nothing
  '      If Valori(Riga) Then
  '        Return 0
  '      Else
  '        Return 1
  '      End If
  '      'Return IIf(pValuesBl(Riga), 1, 0)
  '    Case eDataType.eDateOnly, eDataType.eDateTime, eDataType.eTimeOnly
  '      If ValoriDT.Count = 0 Then Return Nothing
  '      Return ValoriDT(Riga).ToOADate
  '    Case Else
  '      If Valori.Count = 0 Then Return Nothing
  '      Return Valori(Riga)
  '  End Select

  'End Function

  'Public Sub SalvaSuXML(Suffisso As String, SalvaSuFile As Boolean)
  '  ''Dim pChannelId As String
  '  ''Dim CanaleChiave As clsChannels2020.eCanaliChiave
  '  ''Dim pShortName As String
  '  ''Dim pLongName As String
  '  ''Dim pShortUM As String
  '  ''Dim pLongUM As String
  '  ''Dim pDataType As eDataType
  '  'Dim pActualLogHeader As String
  '  ''Dim pKnownHeaders As List(Of String)
  '  ''Dim pPolarHeader As String
  '  ''Dim pIsMath As Boolean
  '  ''Dim pImporta As Boolean
  '  'Dim pPrintedColor As Color
  '  'Dim pDecimals As Integer
  '  'Dim pIsSelected As Boolean
  '  'Dim ValoriDT As DateTime()
  '  'Dim Valori As Double()
  '  'Dim pListaParentChannels As clsChannels2020
  '  Dim ChannelNode As String = "Channel_" & pChannelId
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "KeyChannel", CanaleChiave, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "ShortName", pShortName, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongName", pLongName, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "ShortUM", pShortUM, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "LongUM", pLongUM, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "DataType", pDataType, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "PolarHeader", pPolarHeader, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "IsMath", pIsMath, True, False)
  '  'AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "Load", pImporta, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "Decimals", pDecimals, True, False)
  '  AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eChannels, ChannelNode, "Headers", String.Join(",", pKnownHeaders), True, False)
  '  If SalvaSuFile Then AppConfig.SalvaFileXML()
  'End Sub

End Class


Public Class clsFileParquet2020
  Dim pFileInfo As FileInfo
  Dim pIntestazioni As New List(Of String)
  Dim _TimeRangeRealDateTime As clsTimeRange
  Dim pBoatSpeed As New List(Of Double?)
  Dim pTrueWindSpeed As New List(Of Double?)
  Dim pTrueWindDir As New List(Of Double?)
  Dim _TimeStamps As DateTime()
  Dim _ParquetReader As Parquet.ParquetReader
  Dim _IdCampoSecFromMidnight As Integer = -1
  Dim _IdCampoLocalDateTime As Integer = -1
  Dim _IdCampoLocalTime As Integer = -1
  Dim _IdCampoData As Integer = -1
  Dim _IdCampoDTPerformance As Integer = -1 'potrebbe anche essere il vecchio campo SystemTime_LocalTime
  Dim _IdCampoExpUtc As Integer = -1 'cmpo utc dei file expedition per ricostruire la data
  Dim _ApplicaHeelCheck As Boolean = True
  Dim _ApplicaRdrCheck As Boolean = True
  Public Property LatLonPositionIsTheBow As Boolean = True


  Public Property TimeRangeRealDateTime As clsTimeRange
    Get
      Return _TimeRangeRealDateTime
    End Get
    Set(value As clsTimeRange)
      _TimeRangeRealDateTime = value
    End Set
  End Property

  'Public Property FileDate As Date
  '  Get
  '    Return _FileDate
  '  End Get
  '  Set(value As Date)
  '    _FileDate = value
  '  End Set
  'End Property

  Public Property Intestazioni As List(Of String)
    Get
      Return pIntestazioni
    End Get
    Set(value As List(Of String))
      pIntestazioni = value
    End Set
  End Property

  Public Property TimeStamps As DateTime()
    Get
      Return _TimeStamps
    End Get
    Set(value As DateTime())
      _TimeStamps = value
    End Set
  End Property

  Public Property FileInfo As FileInfo
    Get
      Return pFileInfo
    End Get
    Set(value As FileInfo)
      pFileInfo = value
    End Set
  End Property

  Public Property BoatSpeed As List(Of Double?)
    Get
      Return pBoatSpeed
    End Get
    Set(value As List(Of Double?))
      pBoatSpeed = value
    End Set
  End Property

  Public Property TrueWindSpeed As List(Of Double?)
    Get
      Return pTrueWindSpeed
    End Get
    Set(value As List(Of Double?))
      pTrueWindSpeed = value
    End Set
  End Property

  Public ReadOnly Property TrueWindSpeedNotNan As List(Of Double?)
    Get
      If pTrueWindSpeed Is Nothing Then Return Nothing
      Return pTrueWindSpeed.Where(Function(x) x.HasValue AndAlso Not Double.IsNaN(x.Value)).ToList
    End Get
  End Property

  Public ReadOnly Property TrueWindDirNotNan As List(Of Double?)
    Get
      If pTrueWindDir Is Nothing Then Return Nothing
      Return pTrueWindDir.Where(Function(x) x.HasValue AndAlso Not Double.IsNaN(x.Value)).ToList
    End Get
  End Property

  Public ReadOnly Property BoatSpeedNotNan As List(Of Double?)
    Get
      If pBoatSpeed Is Nothing Then Return Nothing
      Return pBoatSpeed.Where(Function(x) x.HasValue AndAlso Not Double.IsNaN(x.Value)).ToList
    End Get
  End Property

  Public Property TrueWindDir As List(Of Double?)
    Get
      Return pTrueWindDir
    End Get
    Set(value As List(Of Double?))
      pTrueWindDir = value
    End Set
  End Property

  Public Property ParquetReader As ParquetReader
    Get
      Return _ParquetReader
    End Get
    Set(value As ParquetReader)
      _ParquetReader = value
    End Set
  End Property

  Public Property IdCampoLocalDateTime As Integer
    Get
      Return _IdCampoLocalDateTime
    End Get
    Set(value As Integer)
      _IdCampoLocalDateTime = value
    End Set
  End Property

  Public Property ApplicaHeelCheck As Boolean
    Get
      Return _ApplicaHeelCheck
    End Get
    Set(value As Boolean)
      _ApplicaHeelCheck = value
    End Set
  End Property

  Public Property ApplicaRdrCheck As Boolean
    Get
      Return _ApplicaRdrCheck
    End Get
    Set(value As Boolean)
      _ApplicaRdrCheck = value
    End Set
  End Property

  Public Property IsValid As Boolean = True

  'Public Property MomentiRealDT As Date()
  '  Get
  '    Return _MomentiRealDT
  '  End Get
  '  Set(value As Date())
  '    _MomentiRealDT = value
  '  End Set
  'End Property

  Public Sub New(FileInfo As System.IO.FileInfo)
    ImpostazioniIniziali(FileInfo, True)
  End Sub

  Public Sub New(FileInfo As System.IO.FileInfo, DisposeReader As Boolean)
    ImpostazioniIniziali(FileInfo, DisposeReader)
  End Sub

  Public Sub ImpostazioniIniziali(FileInfo As System.IO.FileInfo, DisposeReader As Boolean)
    pFileInfo = FileInfo
    Dim fsTmp As New System.IO.FileStream(pFileInfo.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 65536)
    _ParquetReader = New Parquet.ParquetReader(fsTmp)
    Dim PqDataFileds As List(Of Parquet.Data.DataField) = _ParquetReader.Schema.GetDataFields.ToList
    Dim IntestazioniTmp = _ParquetReader.Schema.GetDataFields.Select(Function(x) x.Name).ToList
    pIntestazioni.Clear()
    'Dim IntTmp As New List(Of String)
    For Each intestazione In IntestazioniTmp
      If intestazione.IndexOf("{") = -1 Then
        'pIntestazioni.Add(intestazione.Replace(vbNullChar, "").Replace(" ", "_"))
        pIntestazioni.Add(intestazione.Replace(vbNullChar, ""))
        'IntTmp.Add(intestazione)
      End If
      'Dim nomeTmp As String = intestazione.Replace("{", "_ogr_").Replace("}", "_cgr_")
    Next
    'Clipboard.SetText(String.Join(vbCrLf, IntTmp))

    If pIntestazioni(1).StartsWith("Block_") Then
      ImpostaParquetSimul()
    Else
      ImpostaParquet()
    End If

    If DisposeReader Then
      _ParquetReader.Dispose()
      fsTmp.Close()
      fsTmp.Dispose()
    End If

    LatLonPositionIsTheBow = Not System.IO.File.Exists(pFileInfo.FullName.Replace(pFileInfo.Name, "strn.txt"))
  End Sub

  Public Sub DisposeReader()
    _ParquetReader.Dispose()
  End Sub

  Private Sub ImpostaParquet()
    _IdCampoSecFromMidnight = pIntestazioni.FindIndex(Function(x) x = "SystemTime_DaySeconds")
    _IdCampoLocalDateTime = pIntestazioni.FindIndex(Function(x) x = "SystemTime_DateTimeLocal")
    _IdCampoExpUtc = pIntestazioni.FindIndex(Function(x) x = "Utc")
    _IdCampoData = pIntestazioni.FindIndex(Function(x) x = "SystemTime_Date")
    _IdCampoDTPerformance = pIntestazioni.FindIndex(Function(x) x = "SystemTime_Performance") 'potrebbe anche essere il vecchio campo SystemTime_LocalTime
    _IdCampoLocalTime = pIntestazioni.FindIndex(Function(x) x = "SystemTime_Local") 'potrebbe anche essere il vecchio campo SystemTime_LocalTime


    Dim PqDataField As Parquet.Data.DataField ' = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoSecFromMidnight).ToLower).FirstOrDefault
    Dim StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)

    Dim StaminchiaDiDati '= StaminchiaRaw.ReadColumn(PqDataField).Data
    Dim ArrayDiQuelParaculoDiDenis As Double() '(StaminchiaDiDati.Length - 1) As Double



    If _IdCampoSecFromMidnight = -1 AndAlso _IdCampoExpUtc > -1 Then
      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoExpUtc).ToLower).FirstOrDefault
      Dim PqDataFieldUtc As Parquet.Data.DataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoExpUtc).ToLower).FirstOrDefault
      Dim StaminchiaRawUtc = _ParquetReader.OpenRowGroupReader(0)
      Dim StaminchiaDiDatiUtc = StaminchiaRawUtc.ReadColumn(PqDataFieldUtc).Data
      'Dim DTtmp As Date = DateTime.FromOADate(StaminchiaDiDatiUtc(100))
      '_FileDate = New DateTime(DTtmp.Year, DTtmp.Month, DTtmp.Day, 0, 0, 0)

      ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDatiUtc.Length - 1)
      Array.Copy(StaminchiaDiDatiUtc, ArrayDiQuelParaculoDiDenis, StaminchiaDiDatiUtc.Length - 1)
      Dim sfm = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToArray

      'Dim tmp = DirectCast(StaminchiaDiDatiUtc, Array)(0)
      'Dim dtt As DateTime = DateTime.FromOADate(StaminchiaDiDatiUtc(0))
      'Dim _FileDate As New DateTime(dtt.Year, dtt.Month, dtt.da, 0, 0, 0)

      ReDim _TimeStamps(sfm.Count - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To sfm.Count - 1
        If sfm(i) > sfm(i - 1) OrElse Double.IsNaN(sfm(i - 1)) Then
          'aggiunge solo i momenti con secondi dalla mezzanotte successivi allúltimo messo e non nulli
          _TimeStamps(i) = DateTime.FromOADate(sfm(i))
        Else
          _TimeStamps(i) = Nothing
        End If
      Next


    ElseIf _IdCampoLocalDateTime > -1 Then
      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoLocalDateTime).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data


      ReDim _TimeStamps(StaminchiaDiDati.length - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To StaminchiaDiDati.length - 1
        If Double.IsNaN(StaminchiaDiDati(i)) Then
          _TimeStamps(i) = Nothing
        Else
          If StaminchiaDiDati(i) > StaminchiaDiDati(i - 1) OrElse Double.IsNaN(StaminchiaDiDati(i - 1)) Then
            _TimeStamps(i) = DateTime.FromOADate(StaminchiaDiDati(i))
          Else
            _TimeStamps(i) = Nothing
          End If
        End If
      Next
    ElseIf _IdCampoDTPerformance > -1 Then
      'da verificare
      'esiste il campo performance ovvero quello con la data nel formato completo prende la data da qui
      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoDTPerformance).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data


      ReDim _TimeStamps(StaminchiaDiDati.length - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To StaminchiaDiDati.length - 1
        If Double.IsNaN(StaminchiaDiDati(i)) Then
          _TimeStamps(i) = Nothing
        Else
          If StaminchiaDiDati(i) > StaminchiaDiDati(i - 1) OrElse Double.IsNaN(StaminchiaDiDati(i - 1)) Then
            'aggiunge solo i momenti con secondi dalla mezzanotte successivi allúltimo messo e non nulli
            _TimeStamps(i) = DateTime.FromOADate(StaminchiaDiDati(i))
          Else
            _TimeStamps(i) = Nothing
          End If
        End If
      Next
      'Stop
      '_Momenti = StaminchiaDiDati
    ElseIf _IdCampoData = -1 Then
      Dim _FileDate As Date
      If _IdCampoData = -1 Then
        Dim Parent As String = pFileInfo.Directory.Name
        If IsNumeric(Parent) Then
          'il file é contenuto in una cartella il cui nome é la data
          _FileDate = New DateTime(Parent.Substring(0, 4), Parent.Substring(4, 2), Parent.Substring(6, 2), 0, 0, 0)
        Else
          'Stop
          'cerca la data nel nome del file
          If _IdCampoExpUtc > -1 Then
            ' prende la tada dal campo utc
            'Stop
            Dim PqDataFieldUtc As Parquet.Data.DataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoExpUtc).ToLower).FirstOrDefault
            Dim StaminchiaRawUtc = _ParquetReader.OpenRowGroupReader(0)
            Dim StaminchiaDiDatiUtc = StaminchiaRawUtc.ReadColumn(PqDataFieldUtc).Data
            Dim DTtmp As Date = DateTime.FromOADate(StaminchiaDiDatiUtc(100))
            _FileDate = New DateTime(DTtmp.Year, DTtmp.Month, DTtmp.Day, 0, 0, 0)

            'Stop
          Else
            For Each Str As String In pFileInfo.Name.Split("_")
              If IsNumeric(Str) Then
                _FileDate = New DateTime(Str.Substring(0, 4), Str.Substring(4, 2), Str.Substring(6, 2), 0, 0, 0)
                Exit For
              End If
            Next
          End If
        End If
      End If

      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoSecFromMidnight).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data
      ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1)
      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
      Dim sfm = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToArray

      ReDim _TimeStamps(sfm.Count - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To sfm.Count - 1
        If Double.IsNaN(sfm(i)) Then
          _TimeStamps(i) = Nothing
        Else
          If sfm(i) > sfm(i - 1) OrElse Double.IsNaN(sfm(i - 1)) Then
            'aggiunge solo i momenti con secondi dalla mezzanotte successivi allúltimo messo e non nulli
            _TimeStamps(i) = _FileDate.AddSeconds(sfm(i))
          Else
            _TimeStamps(i) = Nothing
          End If
        End If
      Next

      'Stop
    ElseIf _IdCampoData > -1 AndAlso _IdCampoLocalTime > -1 Then
      'esiste il campo data e quello day secnds
      'il campodatetime viene riempito dalla combinazione campo data campo day seconds
      'Stop

      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoLocalTime).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      'If StaminchiaRaw.RowCount = 0 Then Exit Sub

      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data

      If StaminchiaDiDati.Length = 0 Then
        IsValid = False
        Exit Sub
      End If
      Dim ArrayDiQuelParaculoDiDenisNullable As Double() '(StaminchiaDiDati.Length - 1) As Double
      ReDim ArrayDiQuelParaculoDiDenisNullable(StaminchiaDiDati.Length - 1)

      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenisNullable, StaminchiaDiDati.Length - 1)
      Dim sfm = ArrayDiQuelParaculoDiDenisNullable.ToArray '.Cast(Of Double?).ToArray

      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoData).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data

      Dim ArrayDiQuelParaculoDiDenisNullable2 As Double() '(StaminchiaDiDati.Length - 1) As Double
      ReDim ArrayDiQuelParaculoDiDenisNullable2(StaminchiaDiDati.Length - 1)

      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenisNullable2, StaminchiaDiDati.Length - 1)
      Dim tmp = ArrayDiQuelParaculoDiDenisNullable2.ToArray '.Cast(Of Double?).ToArray


      'Dim tmp = DirectCast(StaminchiaDiDati, Array)(0)
      'Dim _FileDate As New DateTime(tmp.ToString.Substring(0, 4), tmp.ToString.Substring(4, 2), tmp.ToString.Substring(6, 2), 0, 0, 0)

      ReDim _TimeStamps(sfm.Count - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To sfm.Count - 1
        If sfm(i) > sfm(i - 1) OrElse Double.IsNaN(sfm(i - 1)) Then
          'aggiunge solo i momenti con secondi dalla mezzanotte successivi allúltimo messo e non nulli
          Dim yy As Integer = tmp(i).ToString.Substring(0, 4)
          Dim mm As Integer = tmp(i).ToString.Substring(4, 2)
          Dim dd As Integer = tmp(i).ToString.Substring(6, 2)
          Dim tm = sfm(i).ToString.Split(".")(0).PadLeft(6, "0")
          Dim tmd = 0
          If sfm(i).ToString.Split(".").Length > 1 Then
            tmd = sfm(i).ToString.Split(".")(1).PadRight(3, "0")
          End If
          Dim hh As Integer = tm.ToString.Substring(0, 2)
          Dim mmm As Integer = tm.ToString.Substring(2, 2)
          Dim ss As Integer = tm.ToString.Substring(4, 2)
          Dim sss As Integer = tmd
          _TimeStamps(i) = New DateTime(yy, mm, dd, hh, mmm, ss)
          _TimeStamps(i) = _TimeStamps(i).AddMilliseconds(sss)
        Else
          _TimeStamps(i) = Nothing
        End If
      Next


    Else
      'esiste il campo data e quello day secnds
      'il campodatetime viene riempito dalla combinazione campo data campo day seconds
      'Stop

      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoSecFromMidnight).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data

      Dim ArrayDiQuelParaculoDiDenisNullable As Double() '(StaminchiaDiDati.Length - 1) As Double
      ReDim ArrayDiQuelParaculoDiDenisNullable(StaminchiaDiDati.Length - 1)

      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenisNullable, StaminchiaDiDati.Length - 1)
      Dim sfm = ArrayDiQuelParaculoDiDenisNullable.ToArray '.Cast(Of Double?).ToArray

      PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoData).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data
      Dim tmp = DirectCast(StaminchiaDiDati, Array)(0)
      Dim _FileDate As New DateTime(tmp.ToString.Substring(0, 4), tmp.ToString.Substring(4, 2), tmp.ToString.Substring(6, 2), 0, 0, 0)

      ReDim _TimeStamps(sfm.Count - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To sfm.Count - 1
        If sfm(i) > sfm(i - 1) OrElse Double.IsNaN(sfm(i - 1)) Then
          'aggiunge solo i momenti con secondi dalla mezzanotte successivi allúltimo messo e non nulli
          _TimeStamps(i) = _FileDate.AddSeconds(sfm(i))
        Else
          _TimeStamps(i) = Nothing
        End If
      Next

      'PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoSecFromMidnight).ToLower).FirstOrDefault
      'StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      'StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data
      'ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1)
      'Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
      'Dim sfm = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToArray

      'PqDataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(_IdCampoData).ToLower).FirstOrDefault
      'StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      'StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataField).Data
      'ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1)
      'Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
      'Dim dt = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToArray

      'Stop
      'va rivisto il formato con il quel arriva e riempito l elenco dei momenti

    End If

    _TimeRangeRealDateTime = New clsTimeRange(PrimoNotNothing(_TimeStamps), UltimoNotNothing(_TimeStamps))


    pBoatSpeed = RiempiCanale("Bs", _ParquetReader, PqDataField)
    If pBoatSpeed Is Nothing Then pBoatSpeed = RiempiCanale("Bsp", _ParquetReader, PqDataField)
    If pBoatSpeed Is Nothing Then pBoatSpeed = RiempiCanale("BS", _ParquetReader, PqDataField)
    If pBoatSpeed Is Nothing Then pBoatSpeed = RiempiCanale("SOG", _ParquetReader, PqDataField)
    If pBoatSpeed Is Nothing Then pBoatSpeed = RiempiCanale("Sog", _ParquetReader, PqDataField)
    pTrueWindSpeed = RiempiCanale("Tws", _ParquetReader, PqDataField)
    If pTrueWindSpeed Is Nothing Then pTrueWindSpeed = RiempiCanale("TWS", _ParquetReader, PqDataField)
    pTrueWindDir = RiempiCanale("Twd", _ParquetReader, PqDataField)
    If pTrueWindDir Is Nothing Then pTrueWindDir = RiempiCanale("TWD", _ParquetReader, PqDataField)
    IsValid = True
  End Sub

  Private Sub ImpostaParquetSimul()
    Dim IdCampoSecFromMidnight As Integer = pIntestazioni.FindIndex(Function(x) x = "Block_Clock_WallFM") '"SystemTime_DaySeconds")
    Dim IdCampoData As Integer = pIntestazioni.FindIndex(Function(x) x = "Block_Clock_WallDate") '"SystemTime_Date")

    Dim _FileDate As Date


    Dim PqDataFiled As Parquet.Data.DataField = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(IdCampoSecFromMidnight).ToLower).FirstOrDefault
    Dim StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)

    Dim StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
    Dim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1) As Double?
    Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
    Dim SecsFromMidnight = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToList

    Dim IdPrimaRigaValida As Integer = 0
    Dim IdUltimaRigaValida As Integer = 0
    For i As Integer = 0 To ArrayDiQuelParaculoDiDenis.Length - 1
      If Not Double.IsNaN(SecsFromMidnight(i)) Then
        IdPrimaRigaValida = i
        Exit For
      End If
    Next
    IdUltimaRigaValida = ArrayDiQuelParaculoDiDenis.Length - 1
    For i As Integer = ArrayDiQuelParaculoDiDenis.Length - 1 To 0 Step -1
      If Not IsNothing(SecsFromMidnight(i)) Then
        If SecsFromMidnight(i).Value > 0 Then
          IdUltimaRigaValida = i
          Exit For
        End If
      End If
    Next



    If IdCampoData = -1 Then
      Dim FI As New System.IO.FileInfo(pFileInfo.FullName)
      Dim NomeFile As String = FI.Name
      Dim Sezioni As String() = NomeFile.Split("_")
      For Each Sezione In Sezioni
        Try
          If IsNumeric(Sezione) Then
            Dim DataTmp As DateTime = DateTime.ParseExact(Sezione, "yyyyMMdd", meCultureInfo)
            _FileDate = DataTmp
            Exit For
          End If
        Catch ex As Exception

        End Try
      Next
      If _FileDate = Nothing Then _FileDate = New DateTime(FI.LastWriteTime.Year, FI.LastWriteTime.Month, FI.LastWriteTime.Day, 0, 0, 0)
    Else
      PqDataFiled = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(IdCampoData).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
      ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1)
      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
      Dim dt = ArrayDiQuelParaculoDiDenis.Cast(Of Double?).ToList
      _FileDate = New DateTime(dt(IdPrimaRigaValida).ToString.Substring(0, 4), dt(IdPrimaRigaValida).ToString.Substring(4, 2), dt(IdPrimaRigaValida).ToString.Substring(6, 2), 0, 0, 0)
    End If

    If IdCampoSecFromMidnight = -1 Then
      Stop
    Else
      PqDataFiled = _ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = pIntestazioni(IdCampoSecFromMidnight).ToLower).FirstOrDefault
      StaminchiaRaw = _ParquetReader.OpenRowGroupReader(0)
      StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
      ReDim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1)
      Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
      'Dim SFromMid = ArrayDiQuelParaculoDiDenis.Cast(Of Double)
      ReDim _TimeStamps(SecsFromMidnight.Count - 1)
      _TimeStamps(0) = Nothing
      For i As Integer = 1 To SecsFromMidnight.Count - 1
        If IsNothing(SecsFromMidnight(i)) Then
          _TimeStamps(i) = Nothing
        Else
          _TimeStamps(i) = _FileDate.AddSeconds(SecsFromMidnight(i))
        End If
      Next
    End If

    _TimeRangeRealDateTime = New clsTimeRange(PrimoNotNothing(_TimeStamps), UltimoNotNothing(_TimeStamps))

    pBoatSpeed = RiempiCanaleSimul("Block_Boat_Speed_kts", _ParquetReader, PqDataFiled)
    pTrueWindSpeed = RiempiCanaleSimul("Block_Boat_TWS_kts", _ParquetReader, PqDataFiled)
    pTrueWindDir = RiempiCanaleSimul("Block_Boat_TWD", _ParquetReader, PqDataFiled)

  End Sub


  Private Function RiempiCanale(NomeCanale As String, ParquetReader As Parquet.ParquetReader, PqDataFiled As Parquet.Data.DataField) As List(Of Double?)
    Dim IdCampo As Integer = pIntestazioni.FindIndex(Function(x) x = NomeCanale)
    If IdCampo = -1 Then
      'questo canale non esiste nel file corrente
      Return Nothing
    Else
      PqDataFiled = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = NomeCanale.ToLower).FirstOrDefault
      Dim Temp1 = ParquetReader.OpenRowGroupReader(0)
      Dim Temp2 = Temp1.ReadColumn(PqDataFiled).Data
      Dim Temp3(Temp2.Length - 1)
      Array.Copy(Temp2, Temp3, Temp2.Length - 1)
      Return Temp3.Cast(Of Double?).ToList
    End If
  End Function

  Private Function RiempiCanaleSimul(NomeCanale As String, ParquetReader As Parquet.ParquetReader, PqDataFiled As Parquet.Data.DataField) As List(Of Double?)
    Dim IdCampo As Integer = pIntestazioni.FindIndex(Function(x) x = NomeCanale)
    If IdCampo = -1 Then
      'questo canale non esiste nel file corrente
      Return Nothing
    Else
      PqDataFiled = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.ToLower = NomeCanale.ToLower).FirstOrDefault
      Dim Temp1 = ParquetReader.OpenRowGroupReader(0)
      Dim Temp2 = Temp1.ReadColumn(PqDataFiled).Data
      Dim Temp3(Temp2.Length - 1) As Double?
      For i As Integer = 0 To Temp2.Length - 1
        If IsNothing(Temp2(i)) Then
          Temp3(i) = Nothing
        Else
          Temp3(i) = CDbl(Temp2(i))
        End If
      Next
      Return Temp3.ToList
    End If
  End Function

  Private Function PrimoNotNothing(Valori As DateTime()) As DateTime
    For Each valore In Valori
      If Not valore = Nothing Then Return valore
    Next
    Return Nothing
  End Function

  Private Function UltimoNotNothing(Valori As DateTime()) As DateTime
    For i As Integer = Valori.Count - 4 To 0 Step -1
      If Not Valori(i) = Nothing Then Return Valori(i)
    Next
    Return Nothing
  End Function

  Private Function IsLoaded(Canale As clsChannel2020) As Boolean
    Return DataProvider2020.Channels.ListaCanali.Where(Function(x) x.ChannelId = Canale.ChannelId).Count > 0
  End Function

  Public Sub CaricaValoriCanaliTipoTime(ListaCanali As List(Of clsChannel2020))
    If ListaCanali.First.ActualLogHeader.StartsWith("Block_") Then
      CaricaValoriCanaliSimul(ListaCanali)
      Exit Sub
    End If

    'Dim i As Integer = 0
    For Each Canale In ListaCanali
      'If Not IsLoaded(Canale) Then
      Select Case Canale.DataType
        Case clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eTimeOnly, clsChannel2020.eDataType.eDaySeconds
          ' va gestito l'eventuale errore di formato del record specifico
          If Canale.ValoriDT Is Nothing Then
            Canale.ValoriDT = _TimeStamps.Cast(Of DateTime) ' è il primo file, riempie con i momenti creati in selezione file
          Else
            Dim l1 As Integer = Canale.ValoriDT.Count ' accoda i valori a quelli dei file precedenti
            ReDim Preserve Canale.ValoriDT(Canale.ValoriDT.Count + _TimeStamps.Count - 1)
            Array.Copy(_TimeStamps, 0, Canale.ValoriDT, l1, _TimeStamps.Count)
          End If
          If Canale.DataType = clsChannel2020.eDataType.eDaySeconds Then
            Canale.DataType = clsChannel2020.eDataType.eTimeOnly
            Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eTimeOnly
          End If
        Case Else
          ' blocco disattivato: i valori vengono caricati da CaricaValoriCanale
      End Select
      'Else
      '  Stop
      'End If
      'i += 1
    Next

  End Sub

#Region "Cache reader / schema parquet"

  ' Prima ogni singolo canale riapriva il file, riparsava il footer e riscansionava lo schema
  ' con LINQ + ToLower: con ~150 canali x 7 file erano oltre mille aperture e scansioni.
  Private _ReaderCache As Parquet.ParquetReader
  Private _StreamCache As System.IO.FileStream
  Private _RowGroupCache As ParquetRowGroupReader
  Private _CampiPerNome As Dictionary(Of String, Parquet.Data.DataField)

  ''' <summary>
  ''' Apre (una sola volta) reader, primo row group e indice dei campi per nome.
  ''' </summary>
  Private Sub AssicuraReader()
    If Not _ReaderCache Is Nothing AndAlso Not _CampiPerNome Is Nothing Then Exit Sub
    ChiudiReader()
    _StreamCache = New System.IO.FileStream(pFileInfo.FullName, System.IO.FileMode.Open,
                                            System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 65536)
    _ReaderCache = New Parquet.ParquetReader(_StreamCache)
    _RowGroupCache = _ReaderCache.OpenRowGroupReader(0)
    _CampiPerNome = New Dictionary(Of String, Parquet.Data.DataField)(StringComparer.OrdinalIgnoreCase)
    For Each Campo In _ReaderCache.Schema.GetDataFields
      Dim Nome As String = Campo.Name.Replace(vbNullChar, "")
      If Not _CampiPerNome.ContainsKey(Nome) Then _CampiPerNome(Nome) = Campo
    Next
  End Sub

  ''' <summary>Rilascia il file. Va chiamata quando si abbandona il set di dati corrente.</summary>
  Public Sub ChiudiReader()
    Try
      If Not _ReaderCache Is Nothing Then _ReaderCache.Dispose()
    Catch ex As Exception
    End Try
    Try
      If Not _StreamCache Is Nothing Then _StreamCache.Dispose()
    Catch ex As Exception
    End Try
    _ReaderCache = Nothing
    _StreamCache = Nothing
    _RowGroupCache = Nothing
    _CampiPerNome = Nothing
  End Sub

  ''' <summary>Lookup O(1) del campo parquet per nome, case insensitive.</summary>
  Private Function CampoPerNome(Nome As String) As Parquet.Data.DataField
    If Nome Is Nothing Then Return Nothing
    AssicuraReader()
    Dim Campo As Parquet.Data.DataField = Nothing
    _CampiPerNome.TryGetValue(Nome.Replace(vbNullChar, ""), Campo)
    Return Campo
  End Function

  ''' <summary>
  ''' Legge una colonna come Double(). Evita .Cast(Of Double).Count, che enumerava e
  ''' faceva unboxing di tutti i valori solo per contarli, anche due o tre volte per append.
  ''' </summary>
  Private Function LeggiColonnaDouble(Campo As Parquet.Data.DataField) As Double()
    If Campo Is Nothing Then Return Nothing
    AssicuraReader()
    Dim Dati As Array = _RowGroupCache.ReadColumn(Campo).Data
    If Dati Is Nothing Then Return Nothing
    Dim Tipizzato As Double() = TryCast(Dati, Double())
    If Not Tipizzato Is Nothing Then Return Tipizzato
    ' fallback per colonne nullable o di tipo diverso
    Dim Risultato(Dati.Length - 1) As Double
    For i As Integer = 0 To Dati.Length - 1
      Dim v As Object = Dati.GetValue(i)
      If v Is Nothing Then
        Risultato(i) = Double.NaN
      Else
        Risultato(i) = Convert.ToDouble(v)
      End If
    Next
    Return Risultato
  End Function

  ''' <summary>Accoda i valori al canale preallocando una sola volta.</summary>
  Private Sub AccodaValori(Canale As clsChannel2020, Dati As Double(), Segno As Double)
    If Dati Is Nothing Then Exit Sub
    If Canale.Valori Is Nothing Then
      ReDim Canale.Valori(Dati.Length - 1)
      If Segno = 1 Then
        Array.Copy(Dati, 0, Canale.Valori, 0, Dati.Length)
      Else
        For i As Integer = 0 To Dati.Length - 1
          Canale.Valori(i) = Segno * Dati(i)
        Next
      End If
    Else
      Dim l1 As Integer = Canale.Valori.Length
      ReDim Preserve Canale.Valori(l1 + Dati.Length - 1)
      If Segno = 1 Then
        Array.Copy(Dati, 0, Canale.Valori, l1, Dati.Length)
      Else
        For i As Integer = 0 To Dati.Length - 1
          Canale.Valori(l1 + i) = Segno * Dati(i)
        Next
      End If
    End If
  End Sub

#End Region

  Public Sub CaricaValoriCanale(Canale As clsChannel2020)
    If Canale.ActualLogHeader.StartsWith("Block_") Then
      Exit Sub
    End If

    Select Case Canale.DataType
      Case clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eTimeOnly, clsChannel2020.eDataType.eDaySeconds
        ' va gestito l'eventuale errore di formato del record specifico
        If Canale.ValoriDT Is Nothing Then
          Canale.ValoriDT = _TimeStamps.Cast(Of DateTime) ' è il primo file, riempie con i momenti creati in selezione file
        Else
          Dim l1 As Integer = Canale.ValoriDT.Count ' accoda i valori a quelli dei file precedenti
          ReDim Preserve Canale.ValoriDT(Canale.ValoriDT.Count + _TimeStamps.Count - 1)
          Array.Copy(_TimeStamps, 0, Canale.ValoriDT, l1, _TimeStamps.Count)
        End If
        If Canale.DataType = clsChannel2020.eDataType.eDaySeconds Then
          Canale.DataType = clsChannel2020.eDataType.eTimeOnly
          Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eTimeOnly
        End If

      Case Else
        AssicuraReader()
        Dim PqDataFiled As Parquet.Data.DataField = CampoPerNome(Canale.ActualLogHeader)

        If PqDataFiled Is Nothing Then
          ' non ha trovato il canale: puo' accadere caricando file con headers diversi.
          ' si riempie con NaN usando la lunghezza del primo campo disponibile
          Dim CampoRif As Parquet.Data.DataField = _ReaderCache.Schema.GetDataFields.First
          Dim Lunghezza As Integer = _RowGroupCache.ReadColumn(CampoRif).Data.Length
          Dim l1 As Integer = 0
          If Canale.Valori Is Nothing Then
            ReDim Canale.Valori(Lunghezza - 1)
          Else
            l1 = Canale.Valori.Length
            ReDim Preserve Canale.Valori(l1 + Lunghezza - 1)
          End If
          For ii As Integer = l1 To Canale.Valori.Length - 1
            Canale.Valori(ii) = Double.NaN
          Next
        Else
          Dim Dati As Double() = LeggiColonnaDouble(PqDataFiled)
          If _ApplicaHeelCheck AndAlso Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eHEEL AndAlso RevHeelSign("Heel", "Twa", _RowGroupCache) Then
            AccodaValori(Canale, Dati, -1)
          ElseIf _ApplicaRdrCheck AndAlso Canale.CanaleChiave = clsChannels2020.eCanaliChiave.eRdrAngle AndAlso RevRdrSign("Rudder", "Hdg", _RowGroupCache) Then
            AccodaValori(Canale, Dati, -1)
          Else
            AccodaValori(Canale, Dati, 1)
          End If
        End If
    End Select

  End Sub

  Private _RevHeelCache As Boolean? = Nothing
  Private _RevRdrCache As Boolean? = Nothing

  Private Function RevHeelSign(HeelHeader As String, TwaHeader As String, StaminchiaRaw As ParquetRowGroupReader) As Boolean
    ' il risultato dipende solo dal file: si calcola una volta sola invece che
    ' a ogni caricamento di un canale di heel
    If _RevHeelCache.HasValue Then Return _RevHeelCache.Value

    Dim HeelDataFiled As Parquet.Data.DataField = CampoPerNome(HeelHeader)
    Dim TwaDataFiled As Parquet.Data.DataField = CampoPerNome(TwaHeader)
    If HeelDataFiled Is Nothing OrElse TwaDataFiled Is Nothing Then
      _RevHeelCache = False
      Return False
    End If

    Dim DatiHeel As Double() = LeggiColonnaDouble(HeelDataFiled)
    Dim DatiTwa As Double() = LeggiColonnaDouble(TwaDataFiled)
    If DatiHeel Is Nothing OrElse DatiTwa Is Nothing Then
      _RevHeelCache = False
      Return False
    End If

    Dim n As Integer = Math.Min(DatiHeel.Length, DatiTwa.Length)
    Dim Somma As Double = 1 ' serve nel caso in cui non si verifichi mai l evento di heel maggiore del limite
    Dim Conteggio As Integer = 1
    For i As Integer = 0 To n - 1
      Dim h As Double = DatiHeel(i)
      If Not Double.IsNaN(h) Then
        If Math.Abs(h) > 15 Then
          If (DatiTwa(i) > 0) = (h > 0) Then
            Somma += 1
          Else
            Somma -= 1
          End If
          Conteggio += 1
        End If
      End If
    Next
    _RevHeelCache = (Somma / Conteggio) < 0
    Return _RevHeelCache.Value

  End Function

  Private Function RevRdrSign(RdrHeader As String, HdgHeader As String, StaminchiaRaw As ParquetRowGroupReader) As Boolean
    If _RevRdrCache.HasValue Then Return _RevRdrCache.Value

    Dim RdrDataFiled As Parquet.Data.DataField = CampoPerNome(RdrHeader)
    Dim HdgDataFiled As Parquet.Data.DataField = CampoPerNome(HdgHeader)
    If RdrDataFiled Is Nothing OrElse HdgDataFiled Is Nothing Then
      _RevRdrCache = False
      Return False
    End If

    Dim DatiRdr As Double() = LeggiColonnaDouble(RdrDataFiled)
    Dim DatiHdg As Double() = LeggiColonnaDouble(HdgDataFiled)
    If DatiRdr Is Nothing OrElse DatiHdg Is Nothing Then
      _RevRdrCache = False
      Return False
    End If

    Dim RotOraria As New List(Of clsDoubleXY)
    Dim RotAnti As New List(Of clsDoubleXY)
    RotOraria.Add(New clsDoubleXY(3, -1)) ' serve nel caso in cui non si verifichi mai l evento di heel maggiore del limite
    RotAnti.Add(New clsDoubleXY(-3, 1)) ' serve nel caso in cui non si verifichi mai l evento di heel maggiore del limite
    Dim n As Integer = Math.Min(DatiRdr.Length, DatiHdg.Length)
    Dim v1 As Double = DatiHdg(0)
    For i As Integer = 0 To n - 1
      Dim v2 As Double = DatiHdg(i)
      If Not Double.IsNaN(v2) Then
        Dim d As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2)
        If Math.Abs(d) > 4 Then
          If d > 0 Then
            ' rotazione oraria
            RotOraria.Add(New clsDoubleXY(d, DatiRdr(i)))
          Else
            RotAnti.Add(New clsDoubleXY(d, DatiRdr(i)))
          End If
        End If
        v1 = v2
      End If
    Next
    Dim ROd As Double = RotOraria.Select(Function(x) x.X).Average
    Dim RAd As Double = RotAnti.Select(Function(x) x.X).Average
    Dim ROavg As Double = MathNet.Numerics.Statistics.Statistics.Median(RotOraria.Select(Function(x) x.Y))
    Dim RAavg As Double = MathNet.Numerics.Statistics.Statistics.Median(RotAnti.Select(Function(x) x.Y))

    _RevRdrCache = ROavg > RAavg
    Return _RevRdrCache.Value

  End Function

  Public Sub CaricaValoriCanaliSimul(ByRef ListaCanali As List(Of clsChannel2020))
    Dim fsTmp As New System.IO.FileStream(pFileInfo.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 65536)
    Dim ParquetReader As Parquet.ParquetReader = New Parquet.ParquetReader(fsTmp)

    Dim i As Integer = 0
    For Each Canale In ListaCanali
      If Not IsLoaded(Canale) Then
        Select Case Canale.DataType
          Case clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eTimeOnly
            ' va gestito l'eventuale errore di formato del record specifico
            If Canale.ValoriDT Is Nothing Then
              ' primo file
              Canale.ValoriDT = _TimeStamps.Cast(Of DateTime)
            Else
              ' files successivi
              Dim l1 As Integer = Canale.ValoriDT.Count
              ReDim Preserve Canale.ValoriDT(Canale.ValoriDT.Count + _TimeStamps.Count - 1)
              Array.Copy(_TimeStamps, 0, Canale.ValoriDT, l1, _TimeStamps.Count)
            End If
          Case Else
            Dim PqDataFiled As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.Replace(vbNullChar, "").ToLower = Canale.ActualLogHeader.ToLower).FirstOrDefault
            If PqDataFiled Is Nothing Then
              Stop
            Else
              Dim StaminchiaRaw = ParquetReader.OpenRowGroupReader(0)
              Dim StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
              If Canale.Valori Is Nothing Then
                ' primo file
                Dim Temp3(StaminchiaDiDati.Length - 1) As Double
                For ii As Integer = 0 To StaminchiaDiDati.Length - 1
                  If IsNothing(StaminchiaDiDati(ii)) Then
                    Temp3(ii) = Double.NaN
                  Else
                    Temp3(ii) = CDbl(StaminchiaDiDati(ii))
                  End If
                Next
                'Canale.Valori = StaminchiaDiDati.Cast(Of Single)
                Canale.Valori = Temp3.ToArray
              Else
                ' files successivi
                Dim l1 As Integer = Canale.Valori.Count
                ReDim Preserve Canale.Valori(Canale.Valori.Count + StaminchiaDiDati.Length - 1)
                For ii As Integer = 0 To StaminchiaDiDati.Length - 1
                  If IsNothing(StaminchiaDiDati(ii)) Then
                    Canale.Valori(l1 + ii) = Double.NaN
                  Else
                    Canale.Valori(l1 + ii) = CDbl(StaminchiaDiDati(ii))
                  End If
                Next
                'Array.Copy(StaminchiaDiDati, 0, Canale.Valori, l1, StaminchiaDiDati.Cast(Of Double).Count)
              End If
            End If
        End Select
      Else
        Stop
      End If
      i += 1
    Next
    ParquetReader.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()

  End Sub

  Public Function CaricaValoriCanale(Canale As String) As Double()
    Dim fsTmp As New System.IO.FileStream(pFileInfo.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 65536)
    Dim ParquetReader As Parquet.ParquetReader = New Parquet.ParquetReader(fsTmp)
    Dim PqDataFiled As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.Replace(vbNullChar, "").ToLower = Canale.ToLower).FirstOrDefault
    Dim StaminchiaRaw = ParquetReader.OpenRowGroupReader(0)
    Dim StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
    Dim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1) As Double
    Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
    ParquetReader.Dispose()
    fsTmp.Close()
    fsTmp.Dispose()
    Return ArrayDiQuelParaculoDiDenis.Cast(Of Double)

  End Function

  Public Function CaricaValoriCanale(Canale As String, ParquetReader As Parquet.ParquetReader) As Double()
    Dim PqDataFiled As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name.Replace(vbNullChar, "").ToLower = Canale.ToLower).FirstOrDefault
    If PqDataFiled Is Nothing Then Return Nothing
    Dim StaminchiaRaw = ParquetReader.OpenRowGroupReader(0)
    Dim StaminchiaDiDati = StaminchiaRaw.ReadColumn(PqDataFiled).Data
    Dim ArrayDiQuelParaculoDiDenis(StaminchiaDiDati.Length - 1) As Double
    Array.Copy(StaminchiaDiDati, ArrayDiQuelParaculoDiDenis, StaminchiaDiDati.Length - 1)
    Return ArrayDiQuelParaculoDiDenis.Cast(Of Double)
  End Function

End Class



Public Class clsFaroBinRecord2020
  Public Property Valori As String()
  Public Property ValoriDbl As Double()
  Dim pMomentoDT As DateTime

  Public Sub New(NumeroValori As Integer)
    ReDim Valori(NumeroValori)
    ReDim ValoriDbl(NumeroValori)
  End Sub

  'Public Property Valori As String()
  '  Get
  '    Return Valori
  '  End Get
  '  Set(value As String())
  '    Valori = value
  '  End Set
  'End Property

  'Public Property ValoriDbl As Double()
  '  Get
  '    Return ValoriDbl
  '  End Get
  '  Set(value As Double())
  '    ValoriDbl = value
  '  End Set
  'End Property

  Public Property MomentoDT As Date
    Get
      Return pMomentoDT
    End Get
    Set(value As Date)
      pMomentoDT = value
    End Set
  End Property

  Public Sub AggiornaTime(IdCampoSecFromMidnight As Integer, IdCampoData As Integer)
    Dim pSecFromMidnight As Double = ValoriDbl(IdCampoSecFromMidnight)
    pMomentoDT = New Date(ValoriDbl(IdCampoData).ToString.Substring(0, 4), ValoriDbl(IdCampoData).ToString.Substring(4, 2), ValoriDbl(IdCampoData).ToString.Substring(6, 2), 0, 0, 0)
    'pMomentoDT = CDate(ValoriDbl(IdCampoData).ToString)
    pMomentoDT = pMomentoDT.AddSeconds(pSecFromMidnight)
  End Sub

End Class


Public Class clsFaRoBin
  Dim pFileInfo As FileInfo
  Dim pIntestazioni As New List(Of String)
  Dim pSource As String = ""
  Dim pRecords As New List(Of clsFaroBinRecord2020)
  Dim pTimeRange As clsTimeRange

  Public Sub New(FileInfo As FileInfo)
    'C:\DiscoDati\progettidotnet\SPwpf\20190625\ILS0Log_20190625_114314_395.bin
    pFileInfo = FileInfo
    If System.IO.File.Exists(pFileInfo.FullName) Then
      LeggiFile(True)
    End If
  End Sub

  Public Sub New(FileInfo As FileInfo, LeggiSoloIntestazioni As Boolean)
    pFileInfo = FileInfo
    If System.IO.File.Exists(FileInfo.FullName) Then
      LeggiFile(LeggiSoloIntestazioni)
    End If
  End Sub

  Public Property TimeRange As clsTimeRange
    Get
      Return pTimeRange
    End Get
    Set(value As clsTimeRange)
      pTimeRange = value
    End Set
  End Property

  Public Property Records As List(Of clsFaroBinRecord2020)
    Get
      Return pRecords
    End Get
    Set(value As List(Of clsFaroBinRecord2020))
      pRecords = value
    End Set
  End Property

  Public Property Intestazioni As List(Of String)
    Get
      Return pIntestazioni
    End Get
    Set(value As List(Of String))
      pIntestazioni = value
    End Set
  End Property

  Public Property FileInfo As FileInfo
    Get
      Return pFileInfo
    End Get
    Set(value As FileInfo)
      pFileInfo = value
    End Set
  End Property

  Public Sub CaricaValoriCanali(ListaCanali As List(Of clsChannel2020))
    LeggiFile(False)
    CaricaValori(ListaCanali)
  End Sub

  Private Sub LeggiFile(SoloIntestazioni As Boolean)
    'Dim Adesso As DateTime = Now
    Dim BinFile = IO.File.Open(pFileInfo.FullName, IO.FileMode.Open)
    Dim Buffer1byte(0) As Byte
    Dim Buffer5bytes(4) As Byte
    Dim Bafferone(20) As Byte

    Dim strTmp As String = ""
    Dim Carattere As Char

    Dim FrameID As Integer
    Dim nDouble As Integer = 0
    Dim nFloat As Integer = 0
    Dim nBool As Integer = 0

    Dim IdCampoData As Integer = 2
    If pFileInfo.FullName.IndexOf("SensorsLog") > -1 Then IdCampoData = 1

    pRecords.Clear()

    For i As Integer = 0 To BinFile.Length - 1
      If BinFile.Position >= BinFile.Length Then Exit For
      Dim LatByte As Integer = BinFile.Read(Buffer1byte, 0, 1)
      If Buffer1byte.First = 36 Then '$
        BinFile.Read(Buffer5bytes, 0, 5)
        strTmp = System.Text.Encoding.Default.GetString(Buffer5bytes)
        Select Case strTmp
          Case "FRLSN"
            'inizio riga intestazioni
            'legge tutta la riga fino a quando non trova il $ successivo
            If pIntestazioni.Count = 0 Then
              Dim strHeaders As String = ""
              Do
                BinFile.Read(Buffer1byte, 0, 1)
                Carattere = System.Text.Encoding.Default.GetString(Buffer1byte)
                strHeaders &= Carattere
              Loop Until Carattere = "$"
              strHeaders = strHeaders.Replace(vbNullChar, "")
              pIntestazioni = strHeaders.Replace(" ", "_").TrimEnd("$").Split(",").ToList
              pSource = pIntestazioni(1).Clone
              pIntestazioni.RemoveAt(0)
              pIntestazioni.RemoveAt(0)
              pRecords.Insert(0, New clsFaroBinRecord2020(pIntestazioni.Count - 1))
              pRecords(0).Valori = pIntestazioni.ToArray
              If SoloIntestazioni Then
                pRecords.RemoveRange(1, pRecords.Count - 1)
                BinFile.Close()
                BinFile.Dispose()
                pTimeRange = Nothing
                'Console.WriteLine(Now.Subtract(Adesso).TotalMilliseconds / 1000)
                Exit Sub
              End If
            End If
          Case "FRLSB"
            'inizio riga valori
            BinFile.Read(Bafferone, 0, 15)
            FrameID = BitConverter.ToInt32(Bafferone, 5)
            nDouble = BitConverter.ToInt16(Bafferone, 9)
            nFloat = BitConverter.ToInt16(Bafferone, 11)
            nBool = BitConverter.ToInt16(Bafferone, 13)
            Dim totBytesDouble As Integer = nDouble * 8
            Dim totBytesFloat As Integer = nFloat * 4
            Dim totBytesBool As Integer = System.Math.Ceiling(CDbl(nBool) / 8) 'Int((nBool - 1) / 8) + 1

            Dim Bafferissimo(totBytesDouble + totBytesFloat + totBytesBool - 1) As Byte
            BinFile.Read(Bafferissimo, 0, totBytesDouble + totBytesFloat + totBytesBool)

            pRecords.Add(New clsFaroBinRecord2020(nDouble + nFloat + nBool))
            Dim Contatore As Integer = 0
            For ii As Integer = 0 To nDouble - 1
              pRecords.Last.ValoriDbl(Contatore) = BitConverter.ToDouble(Bafferissimo, ii * 8)
              Contatore += 1
            Next
            For ii As Integer = 0 To nFloat - 1
              pRecords.Last.ValoriDbl(Contatore) = BitConverter.ToSingle(Bafferissimo, totBytesDouble + (ii * 4))
              Contatore += 1
            Next
            For ii As Integer = 0 To nBool - 1
              Dim idByte As Integer = ii / 8
              Dim Indice As Integer = totBytesDouble + totBytesFloat + idByte
              Dim Val = IsBitSet(Bafferissimo(Indice), ii)
              pRecords.Last.ValoriDbl(Contatore) = CDbl(Val)
              Contatore += 1
            Next
            pRecords.Last.AggiornaTime(0, IdCampoData)
          Case Else
            'normale valore
            'non essendo anticipato da intestazione di riga riconosciuta non fa nulla
        End Select


      Else

      End If
    Next
    If pRecords.Count > 0 Then
      pTimeRange = New clsTimeRange(pRecords(1).MomentoDT, pRecords.Last.MomentoDT)
    End If
    'Console.WriteLine(Now.Subtract(Adesso).TotalMilliseconds / 1000)
  End Sub

  Public Function IsBitSet(InByte As Byte, Bit As Byte) As Boolean
    'Is het n'de bit van InByte gezet of niet?
    IsBitSet = ((InByte And (2 ^ Bit)) > 0)
  End Function


  Private Sub CaricaValori(ListaCanali As List(Of clsChannel2020))
    For Each Canale In ListaCanali
      Select Case Canale.DataType
        Case clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eTimeOnly
          ReDim Canale.ValoriDT(Records.Count - 1)
        Case Else
          ReDim Canale.Valori(Records.Count - 1)
      End Select
      Canale.IdIntestazione = pIntestazioni.IndexOf(Canale.ActualLogHeader)
    Next

    For i As Integer = 0 To pRecords.Count - 1
      For Each Canale In ListaCanali
        Select Case Canale.DataType
          Case clsChannel2020.eDataType.eDateTime, clsChannel2020.eDataType.eDateOnly, clsChannel2020.eDataType.eTimeOnly
            ' va gestito l'eventuale errore di formato del record specifico
            Canale.ValoriDT(i) = pRecords(i).MomentoDT
          Case Else
            Canale.Valori(i) = pRecords(i).ValoriDbl(Canale.IdIntestazione)
        End Select
      Next
    Next


  End Sub


End Class

Public Class clsXdeltaYValue
  Dim pXmin As Double
  Dim pXmax As Double
  Dim pY As Double = 0

  Public Sub New(Xmin As Double, Xmax As Double)
    pXmin = Xmin
    pXmax = Xmax
  End Sub

  Public Property Xmin As Double
    Get
      Return pXmin
    End Get
    Set(value As Double)
      pXmin = value
    End Set
  End Property

  Public Property Xmax As Double
    Get
      Return pXmax
    End Get
    Set(value As Double)
      pXmax = value
    End Set
  End Property

  Public Property Y As Double
    Get
      Return pY
    End Get
    Set(value As Double)
      pY = value
    End Set
  End Property
End Class

Public Class clsXYdouble
  Dim pX As Double
  Dim pY As Double = 0

  Public Sub New(X As Double)
    pX = X
  End Sub

  Public Sub New(X As Double, Y As Double)
    pX = X
    pY = Y
  End Sub

  Public Property X As Double
    Get
      Return pX
    End Get
    Set(value As Double)
      pX = value
    End Set
  End Property

  Public Property Y As Double
    Get
      Return pY
    End Get
    Set(value As Double)
      pY = value
    End Set
  End Property

End Class

Public Class clsIdXY
  Dim pId As Integer
  Dim pX As Double
  Dim pY As Double = 0


  Public Sub New(Id As Integer, X As Double, Y As Double)
    pId = Id
    pX = X
    pY = Y
  End Sub

  Public Property Id As Double
    Get
      Return pId
    End Get
    Set(value As Double)
      pId = value
    End Set
  End Property

  Public Property X As Double
    Get
      Return pX
    End Get
    Set(value As Double)
      pX = value
    End Set
  End Property

  Public Property Y As Double
    Get
      Return pY
    End Get
    Set(value As Double)
      pY = value
    End Set
  End Property

End Class

Public Class clsViolin
  Public Property Valori As Double() 'X é il valore , Y i samples
  Public Property ValoriXY As clsXYdouble() 'X é il valore , Y i samples
  Public Property Intervalli As Integer
  Public Property Poligon As clsXYdouble() 'parte dal primo e fa un giro chiuso 

  'Public Property Valori As Double()
  '  Get
  '    Return Valori
  '  End Get
  '  Set(value As Double())
  '    Valori = value
  '  End Set
  'End Property

  'Public Property ValoriXY As clsXYdouble()
  '  Get
  '    Return ValoriXY
  '  End Get
  '  Set(value As clsXYdouble())
  '    ValoriXY = value
  '  End Set
  'End Property

  'Public Property Poligon As clsXYdouble()
  '  Get
  '    Return pPoligon
  '  End Get
  '  Set(value As clsXYdouble())
  '    pPoligon = value
  '  End Set
  'End Property

  Public Sub AggiornaValori()
    Dim V = Valori.Where(Function(x) Not Double.IsNaN(x))
    Dim Range As Double = (V.Max - V.Min)
    Dim SingleGap As Double = Range / Intervalli
    ReDim Valori(Intervalli)

    For i As Integer = 0 To Valori.Count - 1
      ValoriXY(i) = New clsXYdouble(V.Min + i * SingleGap)
    Next
    For Each Valore In Valori
      Dim X As Integer = Valore
      Dim Gruppo As Integer = System.Math.Floor((X - V.Min) / SingleGap)
      Gruppo = System.Math.Min(Gruppo, Intervalli)
      ValoriXY(Gruppo).Y += 1
    Next

    ReDim Poligon(ValoriXY.Count * 2)
    For i As Integer = 0 To ValoriXY.Count
      Poligon(i) = New clsXYdouble(-ValoriXY(i).Y / 2, ValoriXY(i).X)
      Poligon(Poligon.Count - 1 - i) = New clsXYdouble(ValoriXY(i).Y / 2, ValoriXY(i).X)
    Next

  End Sub

End Class

'Public Class clsConsistencyDetails
'  Implements INotifyPropertyChanged
'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub

'  Dim pConsistencyDetailsSync As clsConsistencyDetailsSync
'  Dim pChannel As clsChannel2020
'  Dim Valori As clsXdeltaYValue()
'  Dim ValoriPort As clsXdeltaYValue()
'  Dim ValoriStbd As clsXdeltaYValue()

'  Public Sub New(ConsistencyDetailsSync As clsConsistencyDetailsSync, Channel As clsChannel2020)
'    pConsistencyDetailsSync = ConsistencyDetailsSync
'    pChannel = Channel
'  End Sub

'  Public ReadOnly Property ConsistencyDetailsSync As clsConsistencyDetailsSync
'    Get
'      Return pConsistencyDetailsSync
'    End Get
'  End Property

'  Public ReadOnly Property Channel As clsChannel2020
'    Get
'      Return pChannel
'    End Get
'  End Property

'  Public ReadOnly Property Valori As clsXdeltaYValue()
'    Get
'      Return Valori
'    End Get
'  End Property

'  Public ReadOnly Property ValoriPort As clsXdeltaYValue()
'    Get
'      Return ValoriPort
'    End Get
'  End Property

'  Public ReadOnly Property ValoriStbd As clsXdeltaYValue()
'    Get
'      Return ValoriStbd
'    End Get
'  End Property

'  Public Sub AggiornaValori()
'    Dim Vmain = DataProvider2020.ValoriIntervallo(pChannel.Valori, pConsistencyDetailsSync.TR)
'    Vmain = Vmain.Where(Function(x) Not Double.IsNaN(x))
'    Dim Range As Double = (Vmain.Max - Vmain.Min)
'    Dim SingleGap As Double = Range / pConsistencyDetailsSync.Intervalli
'    ReDim Valori(pConsistencyDetailsSync.Intervalli)
'    ReDim ValoriPort(pConsistencyDetailsSync.Intervalli)
'    ReDim ValoriStbd(pConsistencyDetailsSync.Intervalli)
'    For i As Integer = 0 To Valori.Count - 1
'      Valori(i) = New clsXdeltaYValue(Vmain.Min + i * SingleGap, Vmain.Min + (i + 1) * SingleGap)
'      ValoriPort(i) = New clsXdeltaYValue(Vmain.Min + i * SingleGap, Vmain.Min + (i + 1) * SingleGap)
'      ValoriStbd(i) = New clsXdeltaYValue(Vmain.Min + i * SingleGap, Vmain.Min + (i + 1) * SingleGap)
'    Next
'    Dim ChTack As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
'    For i As Integer = pConsistencyDetailsSync.TR.IdRigaIniziale To pConsistencyDetailsSync.TR.IdRigaFinale
'      Dim X As Integer = pChannel.Valori(i)
'      Dim Gruppo As Integer = System.Math.Floor((X - Vmain.Min) / SingleGap)
'      Gruppo = System.Math.Min(Gruppo, pConsistencyDetailsSync.Intervalli)
'      Valori(Gruppo).Y += 1
'      If ChTack.Valori(i) > 0 Then
'        ValoriStbd(Gruppo).Y += 1
'      Else
'        ValoriPort(Gruppo).Y += 1
'      End If
'    Next
'  End Sub


'End Class


<AddINotifyPropertyChangedInterface>
Public Class clsTgtManager
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property Tgt As clsTgt

  Public Sub New()

    'CreaFileTargetTest()
  End Sub

  'Public Property Tgt As clsTgt
  '  Get
  '    Return _tgt
  '  End Get
  '  Set(value As clsTgt)
  '    _tgt = value
  '    OnPropertyChanged("Tgt")
  '  End Set
  'End Property


  Public Sub SalvaJsonTgtFile(TgtFile As clsTgt)
    _Tgt = TgtFile
    SalvaJsonTgtFile()
  End Sub

  Private Sub SalvaJsonTgtFile()
    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
    Dim NomeFile As String = UltimoPath
    If Not UltimoPath.EndsWith(".json") Then
      NomeFile = UltimoPath.Replace(".", "_") & ".json"
    End If
    Dim fi As New System.IO.FileInfo(NomeFile)
    _Tgt.Fi = fi
    clsKillerSeriale.SaveConfigurationGeneric(Of clsTgt)(_Tgt, NomeFile)
    ApriExplorer(fi.FullName)
  End Sub



  Public Function CaricaLastJsonTgtFile() As clsTgt
    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile ' AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
    Return CaricaJsonTgtFile(UltimoPath)
  End Function

  Public Function CaricaJsonTgtFile(FilePath As String) As clsTgt
    If System.IO.File.Exists(FilePath) Then
      Dim fi As New System.IO.FileInfo(FilePath)
      If fi.Extension = ".json" Then
        _Tgt = clsKillerSeriale.LoadConfigurationGeneric(Of clsTgt)(FilePath)
        _Tgt.AggiornaTgtValues()
        _Tgt.Fi = fi
      Else
        _Tgt = Nothing
      End If
    Else
      _Tgt = Nothing
    End If
    Return _Tgt
  End Function

  Public Function CaricaJsonTgtFile() As clsTgt
    Dim UltimoPath As String = AppConfig.ActiveProfile.TargetFile ' AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
    Dim fd As String
    If System.IO.File.Exists(UltimoPath) Then
      fd = System.IO.Path.GetDirectoryName(UltimoPath)
    Else
      ' le polari stanno in Target, non in Settings
      fd = AppConfig.ActiveProfile.TargetFolder
      If String.IsNullOrWhiteSpace(fd) OrElse Not System.IO.Directory.Exists(fd) Then fd = AppConfig.ActiveProfile.ProfileFolder
    End If
    Dim SelectedFile As String = ObjFiles.SelezionaFile(fd, "Select Polar File", "Json Polar File|*.Json|All Files|*.*", ".Json", "")
    If SelectedFile = "" Then Return Nothing
    Dim Tgt = CaricaJsonTgtFile(SelectedFile)
    If Not Tgt Is Nothing Then
      AppConfig.ActiveProfile.TargetFile = SelectedFile
      AppConfig.Salva()
      'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", SelectedFile, True, True)
    End If
    Return Tgt
  End Function

End Class


Public Class clsChannelTarget
  Dim _TgtName As String
  Dim _Rows As New List(Of clsTgtRow)
  Dim _RowsChannelName As String
  Dim _ColumnsChannelName As String
  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation
  Dim _RigaAnte As clsTgtRow
  Dim _RigaPost As clsTgtRow

  Public Property TgtName As String
    Get
      Return _TgtName
    End Get
    Set(value As String)
      _TgtName = value
    End Set
  End Property

  Public Property Rows As List(Of clsTgtRow)
    Get
      Return _Rows
    End Get
    Set(value As List(Of clsTgtRow))
      _Rows = value
      VerificaOrdineEtAggiornaArrays()
    End Set
  End Property

  Public Sub AccodaRiga(Riga As clsTgtRow)
    _Rows.Add(Riga)
    VerificaOrdineEtAggiornaArrays()
  End Sub
  Public Property RowsChannelName As String
    Get
      Return _RowsChannelName
    End Get
    Set(value As String)
      _RowsChannelName = value
    End Set
  End Property

  Public Property ColumnsChannelName As String
    Get
      Return _ColumnsChannelName
    End Get
    Set(value As String)
      _ColumnsChannelName = value
    End Set
  End Property

  Public Sub New(TgtName As String, RowsChannelName As String, ColumnsChannelName As String)
    _TgtName = TgtName
    _RowsChannelName = RowsChannelName
    _ColumnsChannelName = ColumnsChannelName
  End Sub

  Public Function PolarValue(RowValue As Double, ColumnValue As Double) As Double
    ImpostaRigaAntePost(RowValue)
    Dim ValAnte As Double = GetPolarValue(ColumnValue, _RigaAnte.ColumnsValues, _RigaAnte.TgtValues)
    If _RigaPost Is Nothing OrElse _RigaAnte Is _RigaPost Then
      Return ValAnte
    Else
      Dim ValPost As Double = GetPolarValue(ColumnValue, _RigaPost.ColumnsValues, _RigaPost.TgtValues)
      Dim rv As Double() = {_RigaAnte.RowValue, _RigaPost.RowValue}
      Dim cv As Double() = {ValAnte, ValPost}
      Return ValoreInterpolazioneLineare(RowValue, rv, cv)
    End If

  End Function

  Public Function TargetValue(RowValue As Double, Upwind As Boolean) As Double
    ImpostaRigaAntePost(RowValue)
    Dim ValAnte As Double = GetTargetValue(_RigaAnte, Upwind)
    If _RigaPost Is Nothing Then
      Return ValAnte
    ElseIf _RigaAnte Is _RigaPost Then
      Return ValAnte
    Else
      Dim ValPost As Double = GetTargetValue(_RigaPost, Upwind)
      Dim rv As Double() = {_RigaAnte.RowValue, _RigaPost.RowValue}
      Dim cv As Double() = {ValAnte, ValPost}
      Return ValoreInterpolazioneLineare(RowValue, rv, cv)
    End If

  End Function


  ''' <summary>
  ''' Calcola in un colpo solo le serie target e polare per tutte le righe (TWS = righe della tabella, TWA = colonne),
  ''' con lo stesso risultato di chiamare PolarValue / TargetValue riga per riga. Il costo del metodo per riga
  ''' era la ricostruzione, a ogni chiamata, di due spline (Akima + lineare): qui l'interpolatore di ogni riga della
  ''' tabella si costruisce una sola volta, al primo uso. Restituisce False se la tabella e' vuota (si usa il percorso classico).
  ''' Non usa i campi di stato condivisi (_RigaAnte, _Interpolatore): non interferisce con le chiamate classiche.
  ''' </summary>
  Public Function CalcolaSerie(Tws As Double(), Twa As Double(), Tgt As Double(), Pol As Double()) As Boolean
    If _Rows Is Nothing OrElse _Rows.Count = 0 Then Return False
    Dim n As Integer = _Rows.Count
    Dim FunzPolar(n - 1) As Func(Of Double, Double)
    Dim TgtUp(n - 1) As Double
    Dim TgtDn(n - 1) As Double
    Dim TgtUpPronto(n - 1) As Boolean
    Dim TgtDnPronto(n - 1) As Boolean

    Dim PolarRiga As Func(Of Integer, Double, Double) =
      Function(r As Integer, c As Double) As Double
        If FunzPolar(r) Is Nothing Then
          Dim cv As Double() = _Rows(r).ColumnsValues
          Dim tv As Double() = _Rows(r).TgtValues
          If cv.Count = 2 Then
            Dim ca As Double = (tv(0) - tv(1)) / (cv(0) - cv(1))
            Dim org As Double = tv(0) - (((tv(0) - tv(1)) / (cv(0) - cv(1))) * cv(0))
            FunzPolar(r) = Function(x As Double) org + ca * x
          Else
            Dim ip = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(cv, tv)
            FunzPolar(r) = Function(x As Double) ip.Interpolate(x)
          End If
        End If
        Return FunzPolar(r)(c)
      End Function

    Dim TargetRiga As Func(Of Integer, Boolean, Double) =
      Function(r As Integer, up As Boolean) As Double
        If up Then
          If Not TgtUpPronto(r) Then
            TgtUp(r) = GetTargetValue(_Rows(r), True)
            TgtUpPronto(r) = True
          End If
          Return TgtUp(r)
        Else
          If Not TgtDnPronto(r) Then
            TgtDn(r) = GetTargetValue(_Rows(r), False)
            TgtDnPronto(r) = True
          End If
          Return TgtDn(r)
        End If
      End Function

    ' interpolazione lineare tra due righe, come LinearSpline.Interpolate: y0 + (y1 - y0) / (x1 - x0) * (x - x0)
    Dim Lineare As Func(Of Double, Double, Double, Double, Double, Double) =
      Function(x As Double, x0 As Double, x1 As Double, y0 As Double, y1 As Double) As Double
        If x1 <= x0 Then
          Return ValoreInterpolazioneLineare(x, New Double() {x0, x1}, New Double() {y0, y1}) ' righe non crescenti: comportamento classico
        End If
        Return y0 + ((y1 - y0) / (x1 - x0)) * (x - x0)
      End Function

    For i As Integer = 0 To Tws.Length - 1
      Dim TWS_ As Double = Tws(i)
      If Double.IsNaN(TWS_) Then
        Tgt(i) = Double.NaN
        Pol(i) = Double.NaN
        Continue For
      End If
      Dim TWA_ As Double = System.Math.Abs(Twa(i))
      Dim Upwind As Boolean = TWA_ <= 90

      ' righe che racchiudono il valore (stessa logica di ImpostaRigaAntePost)
      Dim IdAnte As Integer = -1
      Dim IdPost As Integer = -1
      For r As Integer = 0 To n - 1
        If _Rows(r).RowValue <= TWS_ Then IdAnte = r
        If _Rows(r).RowValue >= TWS_ Then
          IdPost = r
          Exit For
        End If
      Next
      Dim Ante As Integer, Post As Integer
      If IdAnte = -1 Then
        Ante = 0 : Post = -1
      ElseIf IdPost = -1 Then
        Ante = n - 1 : Post = -1
      Else
        Ante = IdAnte : Post = IdPost
      End If

      If Post = -1 OrElse Ante = Post Then
        Pol(i) = PolarRiga(Ante, TWA_)
        Tgt(i) = TargetRiga(Ante, Upwind)
      Else
        Dim x0 As Double = _Rows(Ante).RowValue
        Dim x1 As Double = _Rows(Post).RowValue
        Pol(i) = Lineare(TWS_, x0, x1, PolarRiga(Ante, TWA_), PolarRiga(Post, TWA_))
        Tgt(i) = Lineare(TWS_, x0, x1, TargetRiga(Ante, Upwind), TargetRiga(Post, Upwind))
      End If
    Next
    Return True
  End Function

  Private Sub ImpostaRigaAntePost(RowValue As Double)
    Dim _IdRigaAnte As Integer = -1
    Dim _IdRigaPost As Integer = -1
    For i As Integer = 0 To _Rows.Count - 1
      If _Rows(i).RowValue <= RowValue Then _IdRigaAnte = i
      If _Rows(i).RowValue >= RowValue Then
        _IdRigaPost = i
        Exit For
      End If
    Next
    If _IdRigaAnte = -1 Then ' rowvalue minore della prima riga
      _RigaAnte = Rows.First
      _RigaPost = Nothing
    ElseIf _IdRigaPost = -1 Then ' rowvalue maggiore dell ultima riga
      _RigaAnte = Rows.Last
      _RigaPost = Nothing
    Else ' rowvalue all interno delle righe
      _RigaAnte = Rows(_IdRigaAnte)
      _RigaPost = Rows(_IdRigaPost)
    End If

  End Sub

  Private Function ValoreInterpolazioneLineare(RowValue As Double, RowValues As Double(), TgtValues As Double()) As Double
    _Interpolatore = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(RowValues, TgtValues)
    Dim Int1 As Double = _Interpolatore.Interpolate(RowValue)
    Return Int1
  End Function

  Private Function GetPolarValue(ColumnValue As Double, ColumnValues As Double(), TgtValues As Double()) As Double
    If ColumnValues.Count >= 5 Then
      _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(ColumnValues, TgtValues)
      Dim Int1 As Double = _Interpolatore.Interpolate(ColumnValue)
      Return Int1
    ElseIf ColumnValues.Count = 2 Then

      Dim ca = (TgtValues(0) - TgtValues(1)) / (ColumnValues(0) - ColumnValues(1))
      Dim org = TgtValues(0) - (((TgtValues(0) - TgtValues(1)) / (ColumnValues(0) - ColumnValues(1))) * ColumnValues(0))
      ' y = q + mX
      Return org + ca * ColumnValue
    Else
      _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(ColumnValues, TgtValues)
      Dim Int1 As Double = _Interpolatore.Interpolate(ColumnValue)
      Return Int1
    End If
  End Function

  Private Function GetTargetValue(Riga As clsTgtRow, Upwind As Boolean) As Double
    If Upwind Then
      Dim Ris = Riga.ColumnTgtPoints.Where(Function(x) x.IsTgtUp = True).FirstOrDefault
      If Ris Is Nothing Then Return Double.NaN
      Return Ris.TgtValue
    Else
      Dim Ris = Riga.ColumnTgtPoints.Where(Function(x) x.IsTgtDn = True).FirstOrDefault
      If Ris Is Nothing Then Return Double.NaN
      Return Ris.TgtValue
    End If
  End Function



  Private Sub VerificaOrdineEtAggiornaArrays()
    Dim ltmp = Rows.OrderBy(Function(x) x.RowValue).ToList
    _Rows = ltmp
  End Sub


End Class

Public Class clsTgtRow
  Dim _RowValue As Double
  Dim _ColumnTgtPoints As New List(Of clsTgtPunti2D)
  Dim _ColumnsValues As Double()
  Dim _TgtValues As Double()

  Public Property RowValue As Double
    Get
      Return _RowValue
    End Get
    Set(value As Double)
      _RowValue = value
    End Set
  End Property

  Public Property ColumnTgtPoints As List(Of clsTgtPunti2D)
    Get
      Return _ColumnTgtPoints
    End Get
    Set(value As List(Of clsTgtPunti2D))
      _ColumnTgtPoints = value
      VerificaOrdineEtAggiornaArrays()
    End Set
  End Property

  Public Sub AccodaPunto(Punto As clsTgtPunti2D)
    _ColumnTgtPoints.Add(Punto)
    VerificaOrdineEtAggiornaArrays()
  End Sub

  Public Property ColumnsValues As Double()
    Get
      Return _ColumnsValues
    End Get
    Set(value As Double())
      _ColumnsValues = value
    End Set
  End Property

  Public Property TgtValues As Double()
    Get
      Return _TgtValues
    End Get
    Set(value As Double())
      _TgtValues = value
    End Set
  End Property

  Public Function TargetValue(Upwind As Boolean) As Double
    Try
      If Upwind Then
        Return _ColumnTgtPoints.Where(Function(x) x.IsTgtUp = True).FirstOrDefault.TgtValue
      Else
        Return _ColumnTgtPoints.Where(Function(x) x.IsTgtDn = True).FirstOrDefault.TgtValue
      End If
    Catch ex As Exception
      Return Double.NaN
    End Try
  End Function

  Public Sub New(RowValue As Double)
    _RowValue = RowValue
  End Sub

  Private Sub VerificaOrdineEtAggiornaArrays()
    Dim ltmp = _ColumnTgtPoints.OrderBy(Function(x) x.ColumnValue).ToList
    _ColumnTgtPoints = ltmp
    ReDim _ColumnsValues(_ColumnTgtPoints.Count - 1)
    ReDim _TgtValues(_ColumnTgtPoints.Count - 1)
    For i As Integer = 0 To _ColumnTgtPoints.Count - 1
      _ColumnsValues(i) = _ColumnTgtPoints(i).ColumnValue
      _TgtValues(i) = _ColumnTgtPoints(i).TgtValue
    Next
  End Sub

End Class

Public Class clsTgtPunti2D
  Dim _TgType As eTgType
  Dim _ColumnValue As Double
  Dim _TgtValue As Double

  Public Enum eTgType
    eNone = 0
    eUp = 1
    eDn = 2
  End Enum

  Public Sub New(ColumnValue As Double, TgtValue As Double, TgType As eTgType)
    _ColumnValue = ColumnValue
    _TgtValue = TgtValue
    _TgType = TgType
  End Sub

  Public Property TgtValue As Double
    Get
      Return _TgtValue
    End Get
    Set(value As Double)
      _TgtValue = value
    End Set
  End Property

  Public ReadOnly Property IsTgtUp As Boolean
    Get
      Return _TgType = eTgType.eUp
    End Get
  End Property

  Public ReadOnly Property IsTgtDn As Boolean
    Get
      Return _TgType = eTgType.eDn
    End Get
  End Property

  Public Property ColumnValue As Double
    Get
      Return _ColumnValue
    End Get
    Set(value As Double)
      _ColumnValue = value
    End Set
  End Property

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsIsolinea
  Public Property RefVal As Double
  Public Property ValoriTwaTws As New List(Of clsDoubleXY)

  Public Sub New(RefVal As Double)
    Me.RefVal = RefVal
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsTgt

  <JsonIgnore>
  Public Property Fi As System.IO.FileInfo
  Public Property MainVarName As String
  Public Property TgtRowValues As New List(Of clsTgtRowValues)
  <JsonIgnore>
  Public Property ValoriMainChannel As New List(Of Double)
  <JsonIgnore>
  Public Property ListaCanaliValori As New List(Of String)

  <JsonIgnore>
  Public Property AwaIsolines As New List(Of clsIsolinea)
  <JsonIgnore>
  Public Property AwsIsolines As New List(Of clsIsolinea)

  Dim _ChannelsTarget As New List(Of clsChannelTarget)


  Public Sub AggiornaTgtValues()
    ValoriMainChannel.Clear()
    For Each riga In _TgtRowValues
      riga.ImpostaValori()
      ValoriMainChannel.Add(riga.Value)
    Next
    ListaCanaliValori = _TgtRowValues.First.TgtPoints.First.Values.Keys.ToList
    ImpostaListePolareAndTarget()
  End Sub

  Private Sub ImpostaListePolareAndTarget()
    _ChannelsTarget.Clear()
    For Each Channel In TgtRowValues.First.TgtPoints.First.Values
      'If Not Channel.Key = "twa" Then
      Dim ChannelTarget As New clsChannelTarget(Channel.Key, "tws", "twa")
      For Each tws In _TgtRowValues
        Dim Row As New clsTgtRow(tws.Value)
        Dim canale = tws.TgtValues.Where(Function(x) x.Nome = Channel.Key).FirstOrDefault
        Dim canaleTwa = tws.TgtValues.Where(Function(x) x.Nome = "twa").FirstOrDefault
        If Not canale Is Nothing Then
          For i As Integer = 0 To canale.Valori.Count - 1
            Dim tgType As clsTgtPunti2D.eTgType = SPwpf.clsTgtPunti2D.eTgType.eNone
            If canale.IndiceTgtUp = i Then tgType = clsTgtPunti2D.eTgType.eUp
            If canale.IndiceTgtDn = i Then tgType = clsTgtPunti2D.eTgType.eDn
            Dim punto As New clsTgtPunti2D(canaleTwa.Valori(i), canale.Valori(i), tgType)
            Row.AccodaPunto(punto)
          Next
          ChannelTarget.AccodaRiga(Row)
        Else
          ChannelTarget = Nothing
        End If
      Next
      If Not ChannelTarget Is Nothing Then _ChannelsTarget.Add(ChannelTarget)
      'End If
    Next

    'Stop
  End Sub

  <JsonIgnore>
  Public ReadOnly Property PolareDisponibile(NomeCanale As String) As Boolean
    Get
      Return PolariDisponibili.Where(Function(x) x.ToLower = NomeCanale.ToLower).Count > 0
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property PolariDisponibili As List(Of String)
    Get
      Return _ChannelsTarget.Select(Function(x) x.TgtName).ToList
    End Get
  End Property

  Private Sub ImpostaTwsAntePost(ByRef RigaAnte As clsTgtRowValues, ByRef RigaPost As clsTgtRowValues, Tws As Double)
    RigaAnte = TgtRowValues.First
    For Each v In TgtRowValues
      If v.Value <= Tws Then
        RigaAnte = v
      End If
      If v.Value >= Tws Then
        RigaPost = v
        Exit For
      End If
    Next
    If RigaPost Is Nothing Then RigaPost = TgtRowValues.Last
  End Sub

  Private Function ValoreInterpolato(ByRef RigaAnte As clsTgtRowValues, ByRef RigaPost As clsTgtRowValues, Tws As Double, Twa As Double, Canale As String) As Double
    ImpostaTwsAntePost(RigaAnte, RigaPost, Tws)
    Return InterpolazioneLineare(RigaAnte, RigaPost, Tws, Twa, Canale)
  End Function


  Private Function InterpolazioneLineare(RigaAnte As clsTgtRowValues, RigaPost As clsTgtRowValues, Tws As Double, Twa As Double, Canale As String) As Double
    Dim V1 As Double = RigaAnte.GetValue(Canale, Twa)
    Dim V2 As Double = RigaPost.GetValue(Canale, Twa)
    Return InterpolazioneLineare(RigaAnte.Value, RigaPost.Value, Tws, V1, V2)
  End Function

  Private Function InterpolazioneLineare(LowerTws As Double, HigherTws As Double, Tws As Double, HigherTwsVal As Double, LowerTwsVal As Double) As Double
    Return LowerTwsVal + ((HigherTws - Tws) / (HigherTws - LowerTws) * (HigherTwsVal - LowerTwsVal))
  End Function

  Private Function InterpolazioneLineare(K As Double, Tws As Double, HigherTwsVal As Double, LowerTwsVal As Double) As Double
    Return LowerTwsVal + (K * (HigherTwsVal - LowerTwsVal))
  End Function

  Private Function IsTgtPoint(IdTgtUp As Integer, IdTgtDn As Integer, ActualId As Integer) As Boolean
    If IdTgtUp = ActualId Then Return True
    If IdTgtDn = ActualId Then Return True
    Return False
  End Function

  Public Function PuntiPolare(Tws As Double, CanaleX As String, CanaleY As String) As List(Of clsPuntoPolare)
    Dim TgtCalc As eTgtCalc = eTgtCalc.eNone
    Select Case CanaleY.ToLower
      Case "cwa"
        Return Nothing
      Case "awa"
        TgtCalc = eTgtCalc.eAwa
        CanaleY = "bs"
      Case "vmg"
        TgtCalc = eTgtCalc.eVmg
        CanaleY = "bs"
      Case "aws"
        TgtCalc = eTgtCalc.eAws
        CanaleY = "bs"
      Case Else
        CanaleY = CanaleY.ToLower
    End Select

    Dim Tws1 = TgtRowValues.Where(Function(x) x.Value = Tws).FirstOrDefault
    Dim ListaTmp As New List(Of clsPuntoPolare)
    If Tws1 Is Nothing Then
      'Trova la riga precedente e quella successiva e le interpola
      Dim Tws2 As clsTgtRowValues = Nothing
      ImpostaTwsAntePost(Tws1, Tws2, Tws)

      Dim vX1 = Tws1.TgtValues.Where(Function(x) x.Nome = CanaleX).FirstOrDefault
      Dim vY1 = Tws1.TgtValues.Where(Function(x) x.Nome = CanaleY).FirstOrDefault
      Dim vX2 = Tws2.TgtValues.Where(Function(x) x.Nome = CanaleX).FirstOrDefault
      Dim vY2 = Tws2.TgtValues.Where(Function(x) x.Nome = CanaleY).FirstOrDefault
      Dim V1ok As Boolean = Not vX1 Is Nothing AndAlso Not vY1 Is Nothing
      Dim V2ok As Boolean = Not vX2 Is Nothing AndAlso Not vY2 Is Nothing
      If V1ok Then
        If V2ok Then
          ' entrambi ok
          Dim K As Double = (Tws2.Value - Tws) / (Tws2.Value - Tws1.Value)
          If Not Double.IsInfinity(K) AndAlso Not K = 0 Then
            Dim awa, aws As Double
            For i As Integer = 0 To vX1.Valori.Count - 1
              Dim X As Double = InterpolazioneLineare(K, Tws, vX1.Valori(i), vX2.Valori(i))
              Dim Y As Double = InterpolazioneLineare(K, Tws, vY1.Valori(i), vY2.Valori(i))
              Select Case TgtCalc
                Case eTgtCalc.eAwa
                  ApparentFromTrue(awa, aws, X, Tws, Y)
                  Y = awa
                Case eTgtCalc.eAws
                  ApparentFromTrue(awa, aws, X, Tws, Y)
                  Y = aws
                Case eTgtCalc.eVmg
                  Y = Vmg(Y, X)
                Case Else
              End Select
              ListaTmp.Add(New clsPuntoPolare(X, Y, IsTgtPoint(vX1.IndiceTgtUp, vX1.IndiceTgtDn, i) OrElse IsTgtPoint(vX2.IndiceTgtUp, vX2.IndiceTgtDn, i)))
            Next
          End If
        Else
          ' solo v1 ok
          Dim awa, aws, X, Y As Double
          For i As Integer = 0 To vX1.Valori.Count - 1
            X = vX1.Valori(i)
            Y = vY1.Valori(i)
            Select Case TgtCalc
              Case eTgtCalc.eAwa
                ApparentFromTrue(awa, aws, X, Tws, Y)
                Y = awa
              Case eTgtCalc.eAws
                ApparentFromTrue(awa, aws, X, Tws, Y)
                Y = aws
              Case eTgtCalc.eVmg
                Y = Vmg(Y, X)
              Case Else
            End Select
            ListaTmp.Add(New clsPuntoPolare(X, Y, IsTgtPoint(vX1.IndiceTgtUp, vX1.IndiceTgtDn, i)))
          Next
        End If
      Else
        If V2ok Then
          'solo v2 ok
          Dim awa, aws, X, Y As Double
          For i As Integer = 0 To vX2.Valori.Count - 1
            X = vX2.Valori(i)
            Y = vY2.Valori(i)
            Select Case TgtCalc
              Case eTgtCalc.eAwa
                ApparentFromTrue(awa, aws, X, Tws, Y)
                Y = awa
              Case eTgtCalc.eAws
                ApparentFromTrue(awa, aws, X, Tws, Y)
                Y = aws
              Case eTgtCalc.eVmg
                Y = Vmg(Y, X)
              Case Else
            End Select
            ListaTmp.Add(New clsPuntoPolare(X, Y, IsTgtPoint(vX2.IndiceTgtUp, vX2.IndiceTgtDn, i)))
          Next
        End If
      End If
    Else
      ' tws preciso dulla riga esistente
      Dim vX = Tws1.TgtValues.Where(Function(x) x.Nome = CanaleX).FirstOrDefault
      Dim vY = Tws1.TgtValues.Where(Function(x) x.Nome = CanaleY).FirstOrDefault
      If Not vX Is Nothing AndAlso Not vY Is Nothing Then
        Dim awa, aws, X, Y As Double
        For i As Integer = 0 To System.Math.Min(vY.Valori.Count, vX.Valori.Count) - 1
          X = vX.Valori(i)
          Y = vY.Valori(i)
          Select Case TgtCalc
            Case eTgtCalc.eAwa
              ApparentFromTrue(awa, aws, X, Tws, Y)
              Y = awa
            Case eTgtCalc.eAws
              ApparentFromTrue(awa, aws, X, Tws, Y)
              Y = aws
            Case eTgtCalc.eVmg
              Y = Vmg(Y, X)
            Case Else
          End Select
          ListaTmp.Add(New clsPuntoPolare(X, Y, IsTgtPoint(vX.IndiceTgtUp, vX.IndiceTgtDn, i)))
        Next
      End If
    End If
    Return ListaTmp
  End Function

  Private Function Vmg(Bs As Double, Twa As Double) As Double
    Return Bs * System.Math.Abs(System.Math.Cos(Radians(Twa)))
  End Function

  Public Function Valore(Tws As Double, Twa As Double, Canale As String) As clsValoriPuntoPolare
    Tws = System.Math.Max(Tws, TgtRowValues.First.Value)
    Tws = System.Math.Min(Tws, TgtRowValues.Last.Value)
    Dim Tws1 = TgtRowValues.Where(Function(x) x.Value = Tws).FirstOrDefault
    Dim V As Double = 0
    If Tws1 Is Nothing Then
      'il tentativo di trovare una tws precisa é fallito, cerca la riga precedente e quella successiva e le interpola
      Dim Tws2 As clsTgtRowValues = Nothing
      V = ValoreInterpolato(Tws1, Tws2, Tws, Twa, Canale)
    Else
      V = Tws1.GetValue(Canale, Twa)
    End If
    Return New clsValoriPuntoPolare(Tws, Twa, System.Math.Max(0.001, V), Canale = "bs")
  End Function

  Public Function ValoreTgt(IsUp As Boolean, ByRef tws As Double, Canale As String) As clsValoriPuntoPolare
    If IsUp Then Return ValoreTgtUp(tws, Canale)
    Return ValoreTgtDn(tws, Canale)
  End Function

  Public Function Polare(NomePolare As String) As clsChannelTarget
    Return _ChannelsTarget.Where(Function(x) x.TgtName.ToLower = NomePolare.ToLower).FirstOrDefault
  End Function

  Public Function Valore(Tws As Double, Twa As Double, Polare As clsChannelTarget) As Double
    Return Polare.PolarValue(Tws, Twa)
  End Function

  Public Function ValoreTgt(IsUp As Boolean, ByRef tws As Double, Polare As clsChannelTarget) As Double
    Return Polare.TargetValue(tws, IsUp)
  End Function


  Public Function LiftEquivalent(Tws As Double, DeltaTws As Double, Upwind As Boolean) As Double
    Dim tgt = ValoreTgt(Upwind, Tws, "bs")
    Dim tgt1 = ValoreTgt(Upwind, Tws + DeltaTws, "bs")
    Dim vmg As Double = tgt.Vmg ' vmg benchmark
    Dim cosBeta As Double = vmg / tgt1.Bs ' cos(TwaR=Vmg/Bs)
    Dim Beta As Double = Degrees(Math.Acos(cosBeta)) ' EqTwaD = Degrees(Acos(vmg/Bs))
    Return IIf(Upwind, Beta, 180 - Beta) - tgt.Twa ' Delta Twa = EqTwaD - TwaD
  End Function

  Public Function TwsTimeEquivalent(Tws As Double, DeltaTws As Double, Upwind As Boolean, LossMtVmg As Double) As TimeSpan
    ' LossMt deve arrivare in metri Vmg non inline
    Dim tgt = ValoreTgt(Upwind, Tws, "bs")
    Dim tgt1 = ValoreTgt(Upwind, Tws + DeltaTws, "bs")
    Dim vmgLoss As Double = KtsToMS(tgt.Vmg - tgt1.Vmg) ' differenza di vmg in metri tra tws attuale ed il suo delta passato come variabile
    If (vmgLoss = 0) Then Return New TimeSpan(0)
    Return TimeSpan.FromSeconds(LossMtVmg / vmgLoss) ' restituisce i secondi che servono per evere una perdita in vmg uguale a quella LossMtVmg
  End Function

  Public Function TwaTimeEquivalent(Tws As Double, DeltaTwa As Double, Upwind As Boolean, LossMtVmg As Double) As TimeSpan
    ' LossMt deve arrivare in metri Vmg non inline
    Dim tgt = ValoreTgt(Upwind, Tws, "bs")
    Dim twa = tgt.Twa
    If Not Upwind Then
      twa = 180 - tgt.Twa
    End If
    Dim Vmg1 As Double = tgt.Bs * Math.Cos(Radians(twa + DeltaTwa))
    ' vmg con l angolo twa ottenuto con il delta da quello attuale
    Dim vmgLoss As Double = KtsToMS(tgt.Vmg - Vmg1)
    ' differenza di vmg in metri tra twa attuale ed il suo delta passato come variabile
    If (vmgLoss = 0) Then Return New TimeSpan(0)
    Return TimeSpan.FromSeconds(LossMtVmg / vmgLoss) ' restituisce i secondi che servono per evere una perdita in vmg uguale a quella LossMtVmg
  End Function



  Public Function ValoreTgt(tgtType As clsTgtColonna.eTgtType, tws As Double, Canale As String, ByRef RigaAnte As clsTgtRowValues, ByRef RigaPost As clsTgtRowValues) As clsValoriPuntoPolare
    Dim TgtCalc As eTgtCalc = eTgtCalc.eNone
    Select Case Canale.ToLower
      Case "cwa"
        Return Nothing
      Case "awa"
        TgtCalc = eTgtCalc.eAwa
        Canale = "bs"
      Case "vmg"
        TgtCalc = eTgtCalc.eVmg
        Canale = "bs"
      Case "aws"
        TgtCalc = eTgtCalc.eAws
        Canale = "bs"
      Case Else
        Canale = Canale.ToLower
    End Select
    Dim P1 = RigaAnte.TgtPoints.Where(Function(x) x.TgtType = tgtType).FirstOrDefault
    Dim P2 = RigaPost.TgtPoints.Where(Function(x) x.TgtType = tgtType).FirstOrDefault
    If Not P1 Is Nothing AndAlso Not P2 Is Nothing Then
      Dim K As Double = (RigaPost.Value - tws) / (RigaPost.Value - RigaAnte.Value)
      If Not Double.IsInfinity(K) AndAlso Not K = 0 Then
        Dim X As Double = InterpolazioneLineare(K, tws, P1.Values("twa"), P2.Values("twa"))
        Dim Y As Double = InterpolazioneLineare(K, tws, P1.Values(Canale), P2.Values(Canale))
        Dim awa, aws As Double
        Select Case TgtCalc
          Case eTgtCalc.eAwa
            ApparentFromTrue(awa, aws, X, tws, Y)
            Return New clsValoriPuntoPolare(tws, X, awa, False)
          Case eTgtCalc.eAws
            ApparentFromTrue(awa, aws, X, tws, Y)
            Return New clsValoriPuntoPolare(tws, X, aws, False)
          Case eTgtCalc.eVmg
            Return New clsValoriPuntoPolare(tws, X, Vmg(Y, X), False)
          Case Else
            Return New clsValoriPuntoPolare(tws, X, Y, Canale = "bs")
        End Select
      End If
    End If
    Return Nothing
  End Function

  Private Enum eTgtCalc
    eNone
    eVmg
    eAwa
    eAws
  End Enum


  Public Function ValoreTgtUp(tws As Double, Canale As String) As clsValoriPuntoPolare
    If Double.IsNaN(tws) Then Return Nothing
    tws = System.Math.Max(tws, TgtRowValues.First.Value)
    tws = System.Math.Min(tws, TgtRowValues.Last.Value)
    Dim Tws1 = TgtRowValues.Where(Function(x) x.Value = tws).FirstOrDefault
    If Tws1 Is Nothing Then
      'Trova la riga precedente e quella successiva e le interpola
      Dim Tws2 As clsTgtRowValues = Nothing
      ImpostaTwsAntePost(Tws1, Tws2, tws)
      Return ValoreTgt(clsTgtColonna.eTgtType.Upwind, tws, Canale, Tws1, Tws2)
    Else
      If Not Tws1.TgtPoints.First.Values.ContainsKey(Canale) Then Return Nothing
      Dim twa = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Upwind).FirstOrDefault.Values("twa")
      Dim valore = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Upwind).FirstOrDefault.Values(Canale)
      Dim P1 = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Upwind).FirstOrDefault
      Return New clsValoriPuntoPolare(tws, P1.Values("twa"), P1.Values(Canale), Canale = "bs")
    End If
    Stop
  End Function

  Public Function ValoreTgtDn(tws As Double, Canale As String) As clsValoriPuntoPolare
    If Double.IsNaN(tws) Then Return Nothing
    tws = System.Math.Max(tws, TgtRowValues.First.Value)
    tws = System.Math.Min(tws, TgtRowValues.Last.Value)
    Dim Tws1 = TgtRowValues.Where(Function(x) x.Value = tws).FirstOrDefault
    If Tws1 Is Nothing Then
      'Trova la riga precedente e quella successiva e le interpola
      Dim Tws2 As clsTgtRowValues = Nothing
      ImpostaTwsAntePost(Tws1, Tws2, tws)
      Return ValoreTgt(clsTgtColonna.eTgtType.Downwind, tws, Canale, Tws1, Tws2)
    Else
      If Not Tws1.TgtPoints.First.Values.ContainsKey(Canale) Then Return Nothing
      Dim twa = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Downwind).FirstOrDefault.Values("twa")
      Dim valore = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Downwind).FirstOrDefault.Values(Canale)
      Dim P1 = Tws1.TgtPoints.Where(Function(x) x.TgtType = clsTgtColonna.eTgtType.Downwind).FirstOrDefault
      Return New clsValoriPuntoPolare(tws, P1.Values("twa"), P1.Values(Canale), Canale = "bs")
    End If
    Return Nothing
  End Function

  ''' <summary>
  ''' Tws al quale il target della barca vale quanto il target alla Tws MainChannelValue moltiplicato per Coefficient/100
  ''' (es. 95% del target a 12 nodi = target a circa 11,2 nodi). Si scende (Coefficient &lt; 100) o si sale (&gt; 100) a passi
  ''' di un nodo finche' il target attraversa il valore cercato, poi si interpola linearmente TRA I DUE PUNTI CHE LO
  ''' RACCHIUDONO. NaN se non e' calcolabile (polare assente, valore fuori dalla tabella).
  ''' Prima si interpolava tra il punto trovato e la Tws di partenza anche saltando nodi intermedi, e quando il
  ''' target non era raggiungibile si restituiva 0 (mostrato come "0.0k"), o si andava in errore.
  ''' </summary>
  Private Function TwsEquivalente(MainChannelValue As Double, Coefficient As Double, Canale As String, Upwind As Boolean) As Double
    If Double.IsNaN(MainChannelValue) OrElse Double.IsNaN(Coefficient) OrElse Coefficient <= 0 Then Return Double.NaN
    Dim Target As Func(Of Double, clsValoriPuntoPolare) = Function(t As Double) If(Upwind, ValoreTgtUp(t, Canale), ValoreTgtDn(t, Canale))

    Dim P0 As clsValoriPuntoPolare = Target(MainChannelValue)
    If P0 Is Nothing Then Return Double.NaN
    Dim BsTgt As Double = P0.Bs
    Dim BsEq As Double = BsTgt * Coefficient / 100
    If Coefficient = 100 Then Return MainChannelValue

    Dim PrecTws As Double = MainChannelValue
    Dim PrecBs As Double = BsTgt
    If Coefficient < 100 Then
      For i As Integer = CInt(Math.Ceiling(MainChannelValue)) - 1 To 0 Step -1
        Dim Tg As clsValoriPuntoPolare = Target(i)
        If Not Tg Is Nothing Then
          If Tg.Bs < BsEq Then
            Dim dBs As Double = PrecBs - Tg.Bs
            If dBs <= 0 Then Return Double.NaN
            Return i + (BsEq - Tg.Bs) / dBs * (PrecTws - i)
          End If
          PrecTws = i
          PrecBs = Tg.Bs
        End If
      Next
    Else
      For i As Integer = CInt(Math.Floor(MainChannelValue)) + 1 To 30
        Dim Tg As clsValoriPuntoPolare = Target(i)
        If Not Tg Is Nothing Then
          If Tg.Bs > BsEq Then
            Dim dBs As Double = Tg.Bs - PrecBs
            If dBs <= 0 Then Return Double.NaN
            Return PrecTws + (BsEq - PrecBs) / dBs * (i - PrecTws)
          End If
          PrecTws = i
          PrecBs = Tg.Bs
        End If
      Next
    End If
    Return Double.NaN
  End Function

  Public ReadOnly Property ValoreMainChannelEquivalentTargetUp(MainChannelValue As Double, Coefficient As Double, Canale As String) As Double
    Get
      Return TwsEquivalente(MainChannelValue, Coefficient, Canale, True)
    End Get
  End Property

  Public ReadOnly Property ValoreMainChannelEquivalentTargetDn(MainChannelValue As Double, Coefficient As Double, Canale As String) As Double
    Get
      Return TwsEquivalente(MainChannelValue, Coefficient, Canale, False)
    End Get
  End Property

  Public Sub ImpostaAwaAwsIsolines()
    Dim minTws As Double = TgtRowValues.Min(Function(x) x.Value)
    Dim maxTws As Double = TgtRowValues.Max(Function(x) x.Value)
    Dim awas As New List(Of clsXYZ)
    Dim awss As New List(Of clsXYZ)
    'Dim txtAwa As New List(Of String)
    'Dim txtAws As New List(Of String)
    For tws As Double = minTws To maxTws Step 1
      'Dim RigaAwa As String = tws
      'Dim RigaAws As String = tws
      For twa As Double = 0 To 180 Step 1
        Dim polspd = Polare("bs").PolarValue(tws, twa)
        Dim a As Double
        Dim s As Double
        ApparentFromTrue(a, s, twa, tws, polspd)
        awas.Add(New clsXYZ(twa, tws, a, Colors.Black))
        awss.Add(New clsXYZ(twa, tws, s, Colors.Black))
        'RigaAwa &= vbTab & a
        'RigaAws &= vbTab & s
      Next
      'txtAwa.Add(RigaAwa)
      'txtAws.Add(RigaAws)
    Next

    'Clipboard.SetText(String.Join(vbCrLf, txtAwa.ToArray))
    'Clipboard.SetText(String.Join(vbCrLf, txtAws.ToArray))

    'MetodoInterpolatore(awas, awss, minTws, maxTws)
    MetodoTrendLine(awas, awss, minTws, maxTws)
  End Sub

  Public Sub MetodoTrendLine(awas As List(Of clsXYZ), awss As List(Of clsXYZ), minTws As Double, maxTws As Double)

    Dim AwaIsoTmp As New List(Of clsIsolinea)
    Dim AwsIsoTmp As New List(Of clsIsolinea)
    Dim stp As Double = 5
    Dim Ordine As Integer = 8
    Dim awarange As Double = 1
    Dim awsrange As Double = 0.3
    For awa As Double = 5 To 160 Step stp
      Dim mn As Double = awa - awarange
      Dim mx As Double = awa + awarange
      Dim r = awas.Where(Function(x) x.Z >= mn AndAlso x.Z <= mx).ToList ' righe con quell awa
      Dim o = r.OrderBy(Function(x) x.X).ToList ' ordinate per twa
      Dim il = New clsIsolinea(awa)
      If o.Count <= Ordine + 1 Then
        For Each oo In o
          il.ValoriTwaTws.Add(New clsDoubleXY(oo.X, oo.Y))
        Next
      Else
        Dim twa = o.Select(Function(x) x.X).ToArray
        Dim tws = o.Select(Function(x) x.Y).ToArray
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(twa, tws, Ordine) ', MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquation)
        For i As Double = 30 To 175 Step 5
          Dim ii As Double = p.Coefficients(0)
          For g As Integer = 1 To Ordine
            ii += p.Coefficients(g) * i ^ g
          Next
          il.ValoriTwaTws.Add(New clsDoubleXY(i, ii))
        Next
        AwaIsoTmp.Add(il)
      End If
    Next

    For aws As Double = 5 To 70 Step 2
      Dim mn As Double = aws - awsrange
      Dim mx As Double = aws + awsrange
      Dim r = awss.Where(Function(x) x.Z >= mn AndAlso x.Z <= mx).ToList ' righe con quell awa
      Dim o = r.OrderBy(Function(x) x.Y).ToList ' ordinate per tws
      Dim il = New clsIsolinea(aws)
      If o.Count <= Ordine + 1 Then
        For Each oo In o
          il.ValoriTwaTws.Add(New clsDoubleXY(oo.X, oo.Y))
        Next
      Else
        Dim twa = o.Select(Function(x) x.X).ToArray
        Dim tws = o.Select(Function(x) x.Y).ToArray
        Dim p As MathNet.Numerics.Polynomial = MathNet.Numerics.Polynomial.Fit(twa, tws, Ordine) ', MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquation)
        For i As Double = 30 To 175 Step 5
          Dim ii As Double = p.Coefficients(0)
          For g As Integer = 1 To Ordine
            ii += p.Coefficients(g) * i ^ g
          Next
          il.ValoriTwaTws.Add(New clsDoubleXY(i, ii))
        Next
        AwsIsoTmp.Add(il)
      End If
    Next

    AwaIsolines.Clear()
    AwsIsolines.Clear()
    For Each i In AwaIsoTmp
      Dim o = New clsIsolinea(i.RefVal)
      o.ValoriTwaTws = i.ValoriTwaTws.OrderBy(Function(x) x.X).ToList
      AwaIsolines.Add(o)
    Next
    For Each i In AwsIsoTmp
      Dim o = New clsIsolinea(i.RefVal)
      o.ValoriTwaTws = i.ValoriTwaTws.OrderBy(Function(x) x.X).ToList
      AwsIsolines.Add(o)
    Next


  End Sub


End Class

Public Class clsPuntoPolare
  Dim _PuntoX As Double
  Dim _PuntoY As Double
  Dim _IsTgt As Boolean

  Public Sub New(PuntoX As Double, PuntoY As Double, IsTgt As Boolean)
    _PuntoX = PuntoX
    _PuntoY = PuntoY
    _IsTgt = IsTgt
  End Sub

  Public Property PuntoX As Double
    Get
      Return _PuntoX
    End Get
    Set(value As Double)
      _PuntoX = value
    End Set
  End Property

  Public Property PuntoY As Double
    Get
      Return _PuntoY
    End Get
    Set(value As Double)
      _PuntoY = value
    End Set
  End Property

  Public Property IsTgt As Boolean
    Get
      Return _IsTgt
    End Get
    Set(value As Boolean)
      _IsTgt = value
    End Set
  End Property

End Class

Public Class clsValoriPuntoPolare
  Dim _Tws As Double
  Dim _Twa As Double
  Dim _Bs As Double
  Dim _Vmg As Double
  Dim _Aws As Double
  Dim _Awa As Double


  Public Sub New(Tws As Double, Twa As Double, Bs As Double, Calcola As Boolean)
    _Tws = Tws
    _Twa = Twa
    _Bs = Bs
    If Calcola Then
      _Vmg = Bs * System.Math.Abs(System.Math.Cos(Radians(Twa)))
      ApparentFromTrue(_Awa, _Aws, _Twa, _Tws, _Bs)
    End If
  End Sub

  Public ReadOnly Property Tws As Double
    Get
      Return _Tws
    End Get
  End Property

  Public ReadOnly Property Twa As Double
    Get
      Return _Twa
    End Get
  End Property

  Public ReadOnly Property Bs As Double
    Get
      Return _Bs
    End Get
  End Property

  Public ReadOnly Property Valore As Double
    Get
      Return _Bs
    End Get
  End Property

  Public ReadOnly Property Vmg As Double
    Get
      Return _Vmg
    End Get
  End Property

  Public ReadOnly Property Aws As Double
    Get
      Return _Aws
    End Get
  End Property

  Public ReadOnly Property Awa As Double
    Get
      Return _Awa
    End Get
  End Property

End Class

Public Class clsTgtRowValues
  Dim _Parent As clsTgt
  Dim _Value As Double
  Dim _TgtPoints As New List(Of clsTgtColonna) 'un punto per ciascuna Twa per esempio
  Dim _TgtValues As New List(Of clsTgtValues)
  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation

  Public Sub New(Value As Double, Parent As clsTgt)
    _Value = Value
    _Parent = Parent
  End Sub

  Public Property Value As Double
    Get
      Return _Value
    End Get
    Set(value As Double)
      _Value = value
    End Set
  End Property

  Public Property TgtPoints As List(Of clsTgtColonna)
    Get
      Return _TgtPoints
    End Get
    Set(value As List(Of clsTgtColonna))
      _TgtPoints = value
    End Set
  End Property

  <JsonIgnore>
  Public Property Parent As clsTgt
    Get
      Return _Parent
    End Get
    Set(value As clsTgt)
      _Parent = value
    End Set
  End Property

  <JsonIgnore>
  Public Property TgtValues As List(Of clsTgtValues)
    Get
      Return _TgtValues
    End Get
    Set(value As List(Of clsTgtValues))
      _TgtValues = value
    End Set
  End Property

  '  'Dim a As MathNet.Numerics.Interpolation.IInterpolation
  '  'Dim X As Double()
  '  'Dim Y As Double()
  '  'a = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(X, Y)
  '  'Dim z As Double = a.Interpolate(3)

  Public Sub ImpostaValori()
    TgtValues.Clear()
    Dim Contapunto As Integer = 0
    For Each Colonna In TgtPoints
      For i As Integer = 0 To Colonna.Values.Count - 1
        Dim Nome As String = Colonna.Values.Keys(i)
        Dim Valore As Double = Colonna.Values(Nome)
        Dim TgtValue = _TgtValues.Where(Function(x) x.Nome = Nome).FirstOrDefault
        If TgtValue Is Nothing Then
          TgtValue = New clsTgtValues(Nome, Me)
          TgtValues.Add(TgtValue)
        End If
        TgtValue.Valori.Add(Valore)
        If Colonna.TgtType = clsTgtColonna.eTgtType.Upwind Then
          TgtValue.IndiceTgtUp = Contapunto
        ElseIf Colonna.TgtType = clsTgtColonna.eTgtType.Downwind Then
          TgtValue.IndiceTgtDn = Contapunto
        End If
      Next
      Contapunto += 1
    Next

    If TgtPoints.Count = 0 Then Exit Sub
    If Not TgtPoints.First.Values.ContainsKey("awa") Then
      Dim Tws As Double = _Value
      For Each P In TgtPoints
        Dim Bs As Double = P.Values("bs")
        Dim Twa As Double = P.Values("twa")
        Dim awa, aws, vmg As Double
        vmg = Bs * System.Math.Abs(System.Math.Cos(Radians(Twa)))
        ApparentFromTrue(awa, aws, Twa, Tws, Bs)
        P.Values.Add("awa", awa)
        P.Values.Add("aws", aws)
        P.Values.Add("vmg", vmg)
      Next
      Dim CampiExtra As String() = {"awa", "aws", "vmg"}
      For Each CampoExtra In CampiExtra
        Dim TgtValue As New clsTgtValues(CampoExtra, Me)
        TgtValue.IndiceTgtUp = TgtValues.First.IndiceTgtUp
        TgtValue.IndiceTgtDn = TgtValues.First.IndiceTgtDn
        For Each P In TgtPoints
          Dim Valore As Double = P.Values(CampoExtra)
          TgtValue.Valori.Add(Valore)
        Next
        TgtValues.Add(TgtValue)
      Next
    End If


  End Sub

  Public Function GetTgtValues(NomeCanale As String) As clsTgtValues
    Return _TgtValues.Where(Function(x) x.Nome.ToLower = NomeCanale.ToLower).FirstOrDefault
  End Function

  Public Function GetValue(NomeCanale As String, Twa As Double) As Double
    Dim X As Double() = GetTgtValues("twa").Valori.ToArray
    Dim yTgtVals As clsTgtValues = GetTgtValues(NomeCanale)
    Dim Y As Double()
    If yTgtVals Is Nothing Then
      Return Double.NaN
    Else
      Y = yTgtVals.Valori.ToArray
    End If
    If Not X.Count = Y.Count Then Return Double.NaN
    Return GetValue(Twa, X, Y)
  End Function

  Public Function GetValue(Twa As Double, X As Double(), Y As Double()) As Double
    'Return 0
    _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(X, Y)
    Dim Int1 As Double = _Interpolatore.Interpolate(Twa)
    Return Int1
  End Function




End Class

Public Class clsCoppia
  Dim _A As Double
  Dim _B As Double

  Public Sub New(A As Double, B As Double)
    _A = A
    _B = B
  End Sub

  Public Property A As Double
    Get
      Return _A
    End Get
    Set(value As Double)
      _A = value
    End Set
  End Property

  Public Property B As Double
    Get
      Return _B
    End Get
    Set(value As Double)
      _B = value
    End Set
  End Property
End Class

Public Class clsTgtValues
  Dim _Nome As String
  Dim _Valori As New List(Of Double)
  Dim _Parent As clsTgtRowValues
  Dim _IndiceTgtUp As Integer
  Dim _IndiceTgtDn As Integer

  Public Sub New(Nome As String, Parent As clsTgtRowValues)
    _Nome = Nome
    _Parent = Parent
  End Sub

  Public Property Nome As String
    Get
      Return _Nome
    End Get
    Set(value As String)
      _Nome = value
    End Set
  End Property

  Public Property Valori As List(Of Double)
    Get
      Return _Valori
    End Get
    Set(value As List(Of Double))
      _Valori = value
    End Set
  End Property

  <JsonIgnore>
  Public Property Parent As clsTgtRowValues
    Get
      Return _Parent
    End Get
    Set(value As clsTgtRowValues)
      _Parent = value
    End Set
  End Property

  Public Property IndiceTgtUp As Integer
    Get
      Return _IndiceTgtUp
    End Get
    Set(value As Integer)
      _IndiceTgtUp = value
    End Set
  End Property

  Public Property IndiceTgtDn As Integer
    Get
      Return _IndiceTgtDn
    End Get
    Set(value As Integer)
      _IndiceTgtDn = value
    End Set
  End Property
End Class


Public Class clsTgtColonna
  Dim _TgtType As eTgtType = eTgtType.None
  Dim _Values As New Dictionary(Of String, Double)  'ex twa:35, bs:10.2 , lwy:2.5 etc etc
  Dim _Parent As clsTgtRowValues

  Public Enum eTgtType
    None
    Upwind
    Downwind
  End Enum

  Public Sub New(TgtType As eTgtType, Parent As clsTgtRowValues)
    _TgtType = TgtType
    _Parent = Parent
  End Sub

  Public Property TgtType As eTgtType
    Get
      Return _TgtType
    End Get
    Set(value As eTgtType)
      _TgtType = value
    End Set
  End Property

  Public Property Values As Dictionary(Of String, Double)
    Get
      Return _Values
    End Get
    Set(value As Dictionary(Of String, Double))
      _Values = value
    End Set
  End Property

  <JsonIgnore>
  Public Property Parent As clsTgtRowValues
    Get
      Return _Parent
    End Get
    Set(value As clsTgtRowValues)
      _Parent = value
    End Set
  End Property
End Class

Public Class SailTable
  Public SailCode As String
  Public SecondsTable(30, 180) As Double

  Public Sub New(_SailCode As String)
    SailCode = _SailCode
  End Sub

  Public Sub AddOneSecond(Tws As Double, Twa As Integer)
    SecondsTable(Math.Min(30, CInt(Tws)), CInt(Twa)) += 1
  End Sub

  Public Function GetSeconds(FromTws As Double, ToTws As Double, FromTwa As Integer, ToTwa As Integer)
    Dim tot As Integer = 0
    For tws As Integer = 0 To SecondsTable.GetUpperBound(0)
      For twa As Integer = 0 To SecondsTable.GetUpperBound(1)
        If tws >= FromTws AndAlso tws < ToTws AndAlso twa >= FromTwa AndAlso twa < ToTwa Then
          tot += SecondsTable(tws, twa)
        End If
      Next
    Next
    Return tot
  End Function

End Class

Public Class SailsTable

  Public Sub TabellaVele()
    ' filtra per bsp > 80, bolina heel > 7 e 
    Dim Miniz As Integer = DataPlotSync.VisibleRange.IdRigaIniziale
    Dim Mfin As Integer = DataPlotSync.VisibleRange.IdRigaFinale
    Dim bstp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    Dim tws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim twa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAbsTwa)
    Dim MainSail As clsChannel2020 = DataProvider2020.CanaleDbl("J1")
    Dim Gennaker As clsChannel2020 = DataProvider2020.CanaleDbl("J3")
    Dim Jib As clsChannel2020 = DataProvider2020.CanaleDbl("J4")

    Dim mains As New List(Of SailTable)
    Dim gennakers As New List(Of SailTable)
    Dim jibs As New List(Of SailTable)


    For i As Integer = Miniz To Mfin
      Dim prf = bstp.Valori(i)
      If prf > 80 Then
        Dim mn = MainSail.Valori(i)
        If Not mn = Nothing Then
          Dim mnc = mn.ToString("F2")
          Dim cmn = mains.Where(Function(x) x.SailCode = mnc).FirstOrDefault()
          If cmn Is Nothing Then
            cmn = New SailTable(mnc)
            mains.Add(cmn)
          End If
          cmn.AddOneSecond(tws.Valori(i), twa.Valori(i))
        End If
        Dim jb = Jib.Valori(i)
        If Not jb = Nothing Then
          Dim jbc = jb.ToString("F2")
          Dim jbn = jibs.Where(Function(x) x.SailCode = jbc).FirstOrDefault()
          If jbn Is Nothing Then
            jbn = New SailTable(jbc)
            jibs.Add(jbn)
          End If
          jbn.AddOneSecond(tws.Valori(i), twa.Valori(i))
        End If
        Dim gn = Gennaker.Valori(i)
        If Not gn = Nothing Then
          Dim gnc = gn.ToString("F2")
          Dim cgn = gennakers.Where(Function(x) x.SailCode = gnc).FirstOrDefault()
          If cgn Is Nothing Then
            cgn = New SailTable(gnc)
            gennakers.Add(cgn)
          End If
          cgn.AddOneSecond(tws.Valori(i), twa.Valori(i))
        End If
      End If
    Next

    Dim righe As New List(Of String)

    Dim rtmp As String = "Mainsails"
    righe.Add(rtmp)
    Dim twsstp As Integer = 2
    For Each m In mains.OrderBy(Function(x) x.SailCode).ToList()
      rtmp = m.SailCode
      righe.Add(rtmp)
      Dim Tot As Double = 0
      For twsr As Integer = 0 To 30 Step twsstp
        rtmp = vbTab & "Tws: " & twsr & "-" & twsr + twsstp & vbTab
        Dim ss = m.GetSeconds(twsr, twsr + twsstp, 30, 160)
        rtmp &= CDbl(ss / 3600).ToString("F1")
        righe.Add(rtmp)
        Tot += ss
      Next
      righe.Add(vbTab & "Tot: " & vbTab & (Tot / 3600).ToString("F1"))
      righe.Add("")
    Next

    righe.Add("")
    righe.Add("")
    rtmp = "Jibs"
    righe.Add(rtmp)
    For Each j In jibs.OrderBy(Function(x) x.SailCode).ToList()
      rtmp = j.SailCode
      righe.Add(rtmp)
      Dim Tot As Double = 0
      For twsr As Integer = 0 To 30 Step twsstp
        rtmp = vbTab & "Tws: " & twsr & "-" & twsr + twsstp & vbTab
        Dim ss = j.GetSeconds(twsr, twsr + twsstp, 30, 70)
        rtmp &= CDbl(ss / 3600).ToString("F1")
        righe.Add(rtmp)
        Tot += ss
      Next
      righe.Add(vbTab & "Tot: " & vbTab & (Tot / 3600).ToString("F1"))
      righe.Add("")
    Next


    righe.Add("")
    righe.Add("")
    rtmp = "Gennakers"
    righe.Add(rtmp)
    For Each g In gennakers.OrderBy(Function(x) x.SailCode).ToList()
      rtmp = g.SailCode
      righe.Add(rtmp)
      Dim Tot As Double = 0
      For twsr As Integer = 0 To 30 Step twsstp
        rtmp = vbTab & "Tws: " & twsr & "-" & twsr + twsstp & vbTab
        Dim ss = g.GetSeconds(twsr, twsr + twsstp, 120, 160)
        rtmp &= CDbl(ss / 3600).ToString("F1")
        righe.Add(rtmp)
        Tot += ss
      Next
      righe.Add(vbTab & "Tot: " & vbTab & (Tot / 3600).ToString("F1"))
      righe.Add("")
    Next
    Clipboard.SetText(String.Join(vbCrLf, righe))

  End Sub


End Class