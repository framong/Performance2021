
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Newtonsoft.Json
Imports PropertyChanged
Imports SPwpf

<AddINotifyPropertyChangedInterface>
Public Class clsPeriodManagerViewModel
  'Implements INotifyPropertyChanged

  Public Property SelectedPeriod As clsPeriod2021
  Public Property KeyMomentIsChecked As Boolean = False

  Public Sub New(SelectedPeriod As clsPeriod2021)
    Me.SelectedPeriod = SelectedPeriod
  End Sub

  'Public Property KeyMomentIsChecked As Boolean
  '  Get
  '    Return SelectedPeriod.KeyMomentChecked
  '  End Get
  '  Set(value As Boolean)
  '    pKeyMomentIsChecked = value
  '  End Set
  'End Property

  Public Property KeyMomentEnabled As Boolean
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return SelectedPeriod.PeriodType = clsPeriod2021.ePeriodType.eGybe OrElse SelectedPeriod.PeriodType = clsPeriod2021.ePeriodType.eTack
    End Get
    Set(value As Boolean)
      'OnPropertyChanged("KeyMomentEnabled")
    End Set
  End Property

  'Public Property SelectedPeriod As clsPeriod2021
  '  Get
  '    Return pSelectedPeriod
  '  End Get
  '  Set(value As clsPeriod2021)
  '    pSelectedPeriod = value
  '    OnPropertyChanged("SelectedPeriod")
  '  End Set
  'End Property

  Public Property KeyMoment As DateTime
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return SelectedPeriod.KeyMoment
    End Get
    Set(value As DateTime)
      SelectedPeriod.KeyMoment = value
      'OnPropertyChanged("KeyMoment")
    End Set
  End Property

  Public Property Inizio As DateTime
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return SelectedPeriod.TR.Start
    End Get
    Set(value As DateTime)
      SelectedPeriod.TR.Start = value
      AggiornaMedie()
      'OnPropertyChanged("Inizio")
    End Set
  End Property

  Public Property Fine As DateTime
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return SelectedPeriod.TR.Finish
    End Get
    Set(value As DateTime)
      SelectedPeriod.TR.Finish = value
      AggiornaMedie()
      'OnPropertyChanged("Fine")
    End Set
  End Property

  Public Property PeriodType As clsPeriod2021.ePeriodType
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return SelectedPeriod.PeriodType
    End Get
    Set(value As clsPeriod2021.ePeriodType)
      SelectedPeriod.PeriodType = value
      'OnPropertyChanged("PeriodType")
      'OnPropertyChanged("KeyMomentEnabled")
    End Set
  End Property

  Public ReadOnly Property Tack As String
    Get
      'Return Nothing
      If SelectedPeriod Is Nothing Then Return Nothing
      Select Case SelectedPeriod.PeriodType
        Case clsPeriod2021.ePeriodType.eGybe, clsPeriod2021.ePeriodType.eTack
          If SelectedPeriod.PavarotDetails Is Nothing Then
            Stop
            'pSelectedPeriod.AggiornaValoriCanali()
            SelectedPeriod.UpdateDetails()
          End If
          If SelectedPeriod.PavarotDetails Is Nothing Then
            Return IIf(SelectedPeriod.IsStbd, "Stbd", "Port")
          Else
            Return IIf(SelectedPeriod.IsStbd, "StP", "PtS")
          End If
        Case Else
          Return IIf(SelectedPeriod.IsStbd, "Stbd", "Port")
      End Select
    End Get
    'Set(value As String)
    '  OnPropertyChanged("fBS")
    'End Set
  End Property

  Public ReadOnly Property Brush As SolidColorBrush
    Get
      If SelectedPeriod Is Nothing Then Return Nothing
      Return IIf(Not SelectedPeriod.IsStbd, New SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), New SolidColorBrush(Color.FromArgb(100, 0, 255, 0)))
    End Get
    'Set(value As String)
    '  OnPropertyChanged("fBS")
    'End Set
  End Property


  Private Sub AggiornaMedie()
        'Stop
        'pSelectedPeriod.AggiornaValoriCanali()
    End Sub


  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub


End Class


Public Class clsTabellaPeriodiCanali
  Dim pIntestazione As String
  Dim pListaPeriodi As List(Of clsPeriod2021)
  Dim pListaCanali As List(Of clsChannel2020)

  Public Sub New(Intestazione As String, ListaPeriodi As List(Of clsPeriod2021), ListaCanali As List(Of clsChannel2020))
    pIntestazione = Intestazione
    pListaPeriodi = ListaPeriodi
    pListaCanali = ListaCanali
  End Sub

  Public Function TabellaPeriodi() As List(Of String)
    Dim Righe As New List(Of String)
    Dim RigaIntestazioni As String = "Period" & vbTab & "Tack" & vbTab
    For Each Periodo In pListaPeriodi
      Dim Riga As String = Periodo.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eDateTimeBreve) & " (" & Periodo.TR.DurataInStringaConSeparatore & ") " & Periodo.ShortDescription & " { " & Periodo.ExtendedDescription.Replace(vbCrLf, " ") & "}" & vbTab
      Riga &= IIf(Not Periodo.IsStbd, "Port", "Stbd") & vbTab
      For Each Canale In pListaCanali
        If Canale.HasValues > 0 Then
          Dim SubChannel As New clsValoriPeriodoCanale2020(Canale, Periodo.TR, False)
          'SubChannel.AggiornaSubSet(Periodo.TimeRange.IdRigaIniziale, Periodo.TimeRange.IdRigaFinale, False)
          If Periodo Is pListaPeriodi.First Then
            RigaIntestazioni &= Canale.ChannelId & vbTab
          End If
          Riga &= SubChannel.Avg & vbTab
        End If
      Next
      Righe.Add(Riga.TrimEnd(vbTab))
    Next
    Righe.Insert(0, RigaIntestazioni)
    Righe.Insert(0, pIntestazione & " periods averages")
    Return Righe

  End Function

  Public Function TabellaPeriodiHtml() As List(Of String)
    Dim Righe As New List(Of String)
    'Dim RigaIntestazioni As String = "<th class='Headers'>Period</th><th class='Headers'>Tack</th>"
    Dim RigaIntestazioni As String = "<th class='Headers'>Period</th>"
    Dim Contatore As Integer = 0
    For Each Periodo In pListaPeriodi
      Dim Riga As String = "<td class='Description'><a id=""" & Contatore & """></a><b>" & Periodo.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eDateTimeBreve) & " (" & Periodo.TR.DurataInStringaConSeparatore & ")</b><br>" & Periodo.ShortDescription & If(Periodo.ExtendedDescription.Trim.Length = 0, "</td>", "<br> " & Periodo.ExtendedDescription.Replace(vbCrLf, " ") & "</td>")
      'Riga &= "<td class='" & IIf(Periodo.TWA < 0, "Port", "Stbd") & "'>" & IIf(Periodo.TWA < 0, "Port", "Stbd") & "</td>"

      For Each Canale In pListaCanali
        If Canale.HasValues > 0 Then
          Dim SubChannel As New clsValoriPeriodoCanale2020(Canale, Periodo.TR, False)
          If Periodo Is pListaPeriodi.First Then
            RigaIntestazioni &= "<th class='Headers'>" & Canale.ShortName & "</th>"
          End If
          Riga &= "<td class='" & IIf(Periodo.IsStbd, "Stbd", "Port") & "'>" & SubChannel.Avg & "</td>"
          Riga &= SubChannel.Avg & vbTab
        End If

      Next
      Righe.Add("<tr>" & Riga & "</tr>")
      Contatore += 1
    Next
    Righe.Add("</Table>")
    Righe.Insert(0, "<tr>" & RigaIntestazioni & "</tr>")
    Righe.Insert(0, "<Table>")
    'Righe.Insert(0, pIntestazione & " periods averages")
    Return Righe

  End Function

  Private Class TabellaPivottata
    'Public acceleration As clsAcceleration
    Public canali As New List(Of String)
    Public valori As New List(Of Double)
    Public id As Integer
  End Class

  Public Function TabellaPeriodiCompleta() As List(Of String)
    Dim Righe As New List(Of String)
    Dim RigaIntestazioni As String = "Period" & vbTab & "Start" & vbTab & "Finish" & vbTab & "Tack" & vbTab
    For Each Periodo In pListaPeriodi
      Dim Riga As String = Periodo.ShortDescription & " { " & Periodo.ExtendedDescription.Replace(vbCrLf, " ") & "}" & vbTab
      Riga &= Periodo.TR.Start.ToShortDateString & " " & Periodo.TR.Start.ToShortTimeString & vbTab
      Riga &= Periodo.TR.Finish.ToShortDateString & " " & Periodo.TR.Finish.ToShortTimeString & vbTab
      Riga &= IIf(Periodo.IsStbd, "Stbd", "Port") & vbTab
      'Riga &= Periodo.StringaMure & vbTab
      For Each Canale In pListaCanali
        If Canale.HasValues > 0 Then
          Dim SubChannel As New clsValoriPeriodoCanale2020(Canale, Periodo.TR, False)
          If Periodo Is pListaPeriodi.First Then
            RigaIntestazioni &= Canale.ChannelId & "_Avg" & vbTab & Canale.ChannelId & "_Max" & vbTab & Canale.ChannelId & "_Min" & vbTab & Canale.ChannelId & "_Sd" & vbTab
          End If
          Riga &= SubChannel.Avg & vbTab & SubChannel.Max & vbTab & SubChannel.Min & vbTab & SubChannel.Ds & vbTab
          Riga &= SubChannel.Avg & vbTab
        End If
      Next
      Righe.Add(Riga.TrimEnd(vbTab))
    Next
    Righe.Insert(0, RigaIntestazioni)
    Return Righe

  End Function

  Public Function TabellaCanali() As List(Of String)
    Dim Righe As New List(Of String)
    For Each Canale In pListaCanali
      If Canale.HasValues > 0 Then
        Righe.Add("")
        Dim Riga As String = Canale.ChannelId & vbTab & "Tack" & vbTab & "Avg" & vbTab & "Max" & vbTab & "Min" & vbTab & "StDev"
        Righe.Add(Riga)
        For Each Periodo In pListaPeriodi
          Riga = Periodo.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eDateTimeBreve) & " (" & Periodo.TR.DurataInStringaConSeparatore & ") " & Periodo.ShortDescription & " { " & Periodo.ExtendedDescription.Replace(vbCrLf, " ") & "}" & vbTab
          Riga &= IIf(Periodo.IsStbd, "Stbd", "Port") & vbTab
          Dim SubChannel As New clsValoriPeriodoCanale2020(Canale, Periodo.TR, False)
          Riga &= SubChannel.Avg & vbTab
          Riga &= SubChannel.Max & vbTab
          Riga &= SubChannel.Min & vbTab
          Riga &= SubChannel.Ds & vbTab
          Righe.Add(Riga.TrimEnd(vbTab))
        Next
      End If
    Next
    Righe.Insert(0, pIntestazione & " Channels basic stats")
    Return Righe

  End Function


  Public Function TabellaCanaleHtml(CanaleAscissa As clsChannel2020, CanaleOrdinata As clsChannel2020) As List(Of String)
    Dim Righe As New List(Of String)
    If CanaleOrdinata.HasValues Then
      For Each Periodo In pListaPeriodi
        'Periodo.TimeRange.VerificaRigheAssociate()
        'Dim Vmg As Double = Periodo.Vmg
        Dim SubChannel As New clsValoriPeriodoCanale2020(CanaleOrdinata, Periodo.TR, False)
        Dim SubChannelAscissa As New clsValoriPeriodoCanale2020(CanaleAscissa, Periodo.TR, False)
        Dim X As String = SubChannelAscissa.Avg
        Dim Y As String = SubChannel.Avg
        If CanaleOrdinata.DataType = clsChannel2020.eDataType.e180 Then
          Y = System.Math.Abs(SubChannel.Avg)
        End If
        Dim Colore As String = IIf(Periodo.IsStbd, "green", "red")
        Dim ValoriAltriCanali As String = StringaMedieHtml(Periodo) ' "Twd:345<br/>Tws:12.3<br/>Awa:23.6"
        Dim strTmp As String = "{{x:{0},y:{1},color:'{2}',descr:'{3}'}}"
        Dim a As String = String.Format(strTmp, X, Y, Colore, ValoriAltriCanali)
        Righe.Add(a)
      Next
    End If
    Return Righe
  End Function


  Private Function StringaMedieHtml(Periodo As clsPeriod2021) As String
    Dim strTmp As String = "<table  class=""tbtt"">"
    strTmp &= "<tr class=""trtt""><td colspan=2><b>" & Periodo.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eSoloTime) & " (" & Periodo.TR.DurataInStringaConSeparatore & ")</b></td></tr>"
    For Each canale In pListaCanali
      Dim SubChannel As New clsValoriPeriodoCanale2020(canale, Periodo.TR, False)
      Dim Media As String = SubChannel.Avg
      Select Case canale.DataType
        Case clsChannel2020.eDataType.e180
          Media = Format(System.Math.Abs(SubChannel.Avg), "F" & canale.Decimals.ToString)
        Case Else
      End Select
      strTmp &= "<tr class=""trtt""><td><b>" & canale.ShortName & "</b>:</td><td class=""tdtt"">" & SubChannel.Avg & "</td></tr>" '  [" & SubChannel.SubSetMin & "#" & SubChannel.SubSetMax & " (" & SubChannel.SubSetDs & ")]<br/>"
    Next
    Return strTmp & "</table>"
  End Function

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsTests
  'Implements INotifyPropertyChanged
  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property ListaTests As New ObservableCollection(Of clsTest) ' il primo è il test che contiene i commenti generici
  <JsonIgnore>
  Public Property TestAttivo As clsTest

  Public Sub New()

  End Sub

  Public Sub CreaDailyTest()
    ListaTests.Add(New clsTest(Now, True))
    'pListaTests.Last.IsSelected = True
    ImpostaTestSelezionato(ListaTests.Last)
  End Sub

  'Public Property ListaTests As ObservableCollection(Of clsTest)
  '  Get
  '    Return pListaTests
  '  End Get
  '  Set(value As ObservableCollection(Of clsTest))
  '    pListaTests = value
  '    OnPropertyChanged("ListaTests")
  '    OnPropertyChanged("ListaTestsOrdinata")
  '  End Set
  'End Property


  Private Sub ImpostaTestSelezionato(TestDaSelezionare As clsTest)
    'For Each test In ListaTests
    '  test.IsSelected = (test Is TestDaSelezionare)
    'Next
    TestAttivo = TestDaSelezionare
    'OnPropertyChanged("TestAttivoOrPrimo")
  End Sub

  Public Sub AvviaNuovoTest()
    ListaTests.Add(New clsTest(Now, False))
    ListaTests.Last.Descrizione = "Running Test"
    ImpostaTestSelezionato(ListaTests.Last)
  End Sub

  Public Sub StoppaNuovoTest()
    ListaTests.Last.Fine = Now
    If ListaTests.Last.Descrizione = "Running Test" Then
      ListaTests.Last.Descrizione = "Not Defined Test"
    End If

    ImpostaTestSelezionato(ListaTests.First)
  End Sub

  Public Sub AnnullaNuovoTest()
    ListaTests.Remove(ListaTests.Last)
    ImpostaTestSelezionato(ListaTests.First)
  End Sub

  '<JsonIgnore>
  'Public Property TestAttivoOrPrimo() As clsTest
  '  Get
  '    Return pTestAttivo
  '  End Get
  '  Set(value As clsTest)
  '    ImpostaTestSelezionato(value)
  '  End Set
  'End Property
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsTest
  'Implements INotifyPropertyChanged
  Public Property Inizio As DateTime
  Public Property Fine As DateTime
  Public Property Descrizione As String
  Public Property ListaCommenti As New ObservableCollection(Of clsCommento)
  <JsonIgnore>
  Public Property CommentoCorrente As clsCommento = Nothing
  Public Property IsDailyTest As Boolean = False
  'Dim pIsSelected As Boolean = False

  Public Sub New(Inizio As DateTime, IsDailyTest As Boolean)
    Me.Inizio = Inizio
    Me.IsDailyTest = IsDailyTest
    If IsDailyTest Then
      Me.Descrizione = "General comments of the day"
    End If
    'AggiornaDescrizione()
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub


  'Public Property Inizio As Date
  '  Get
  '    Return pInizio
  '  End Get
  '  Set(value As Date)
  '    pInizio = value
  '    OnPropertyChanged("Inizio")
  '    OnPropertyChanged("DescrizioneTest")
  '  End Set
  'End Property

  'Public Property Fine As Date
  '  Get
  '    Return pFine
  '  End Get
  '  Set(value As Date)
  '    pFine = value
  '    OnPropertyChanged("Fine")
  '    OnPropertyChanged("DescrizioneTest")
  '  End Set
  'End Property

  'Public Property Descrizione As String
  '  Get
  '    Return pDescrizione
  '  End Get
  '  Set(value As String)
  '    pDescrizione = value
  '    OnPropertyChanged("Descrizione")
  '    OnPropertyChanged("DescrizioneTest")
  '  End Set
  'End Property

  'Public Property ListaCommenti As ObservableCollection(Of clsCommento)
  '  Get
  '    Return pListaCommenti
  '  End Get
  '  Set(value As ObservableCollection(Of clsCommento))
  '    pListaCommenti = value
  '    OnPropertyChanged("ListaCommenti")
  '    OnPropertyChanged("DescrizioneTest")
  '  End Set
  'End Property

  <JsonIgnore>
  Public ReadOnly Property DescrizioneTest As String
    Get
      If IsDailyTest Then
        Return Inizio.ToShortDateString & " " & Descrizione & " [" & ListaCommenti.Count & "]"
      Else
        Return Inizio.ToLongTimeString & " (" & Durata() & ") " & Descrizione & " [" & ListaCommenti.Count & "]"
      End If
    End Get
  End Property

  'Public Property IsDailyTest As Boolean
  '  Get
  '    Return pIsDailyTest
  '  End Get
  '  Set(value As Boolean)
  '    pIsDailyTest = value
  '    OnPropertyChanged("IsDailyTest")
  '  End Set
  'End Property

  '<JsonIgnore>
  'Public Property CommentoCorrente As clsCommento
  '  Get
  '    Return pCommentoCorrente
  '  End Get
  '  Set(value As clsCommento)
  '    'pCommentoCorrente = value
  '    ImpostaCommentoCorrente(value)
  '  End Set
  'End Property

  Private Sub ImpostaCommentoCorrente(commento As clsCommento)
    CommentoCorrente = commento
    'OnPropertyChanged("CommentoCorrente")
  End Sub

  Private Function Durata() As String
    If Fine = Nothing Then
      Return "Running..."
    Else
      Dim tmp As TimeSpan = Fine.Subtract(Inizio)
      Return tmp.Minutes.ToString.PadLeft(2, "0") & ":" & tmp.Seconds.ToString.PadLeft(2, "0")
      'Return Format(pFine.Subtract(pInizio).TotalSeconds, "F0")
    End If
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsCommento
  '  Implements INotifyPropertyChanged
  Public Property Momento As DateTime
  Public Property Descrizione As String
  Public Property Commento As String
  Public Property PathFileAudio As String
  Public Property FI As System.IO.FileInfo = Nothing
  'Dim pIsSelected As Boolean = False

  Public Sub New(nMomento As DateTime, nDescrizione As String, nPathFileAudio As String)
    Me.Momento = nMomento
    Me.Descrizione = nDescrizione
    Me.PathFileAudio = nPathFileAudio
  End Sub

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  'Public Property Momento As Date
  '  Get
  '    Return pMomento
  '  End Get
  '  Set(value As Date)
  '    pMomento = value
  '    OnPropertyChanged("DescrizioneEstesa")
  '    OnPropertyChanged("Momento")
  '  End Set
  'End Property

  'Public Property Descrizione As String
  '  Get
  '    Return pDescrizione
  '  End Get
  '  Set(value As String)
  '    pDescrizione = value
  '    OnPropertyChanged("DescrizioneEstesa")
  '    OnPropertyChanged("Descrizione")
  '  End Set
  'End Property

  <JsonIgnore>
  Public ReadOnly Property DescrizioneEstesa As String
    Get
      Return Momento.ToLongTimeString & " " & Descrizione
    End Get
  End Property

  'Public Property PathFileAudio As String
  '  Get
  '    Return pPathFileAudio
  '  End Get
  '  Set(value As String)
  '    pPathFileAudio = value
  '    If System.IO.File.Exists(PathFileAudio) Then
  '      pFI = New System.IO.FileInfo(PathFileAudio)
  '    End If
  '    OnPropertyChanged("PathFileAudio")
  '  End Set
  'End Property

  <JsonIgnore>
  Public ReadOnly Property AudioInfoFile As System.IO.FileInfo
    Get
      Return FI
    End Get
  End Property


  <JsonIgnore>
  Public ReadOnly Property AudioFileName As String
    Get
      If FI Is Nothing Then Return ""
      Return FI.Name
    End Get
  End Property

End Class

