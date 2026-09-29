
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

  Private Class TabellaPivottata
    'Public acceleration As clsAcceleration
    Public canali As New List(Of String)
    Public valori As New List(Of Double)
    Public id As Integer
  End Class



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

