Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports PropertyChanged

Public Class UserControl_ImportaPeriodi
  Dim VM As New clsImportaPeriodiViewModel

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    Me.DataContext = VM


  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.CaricaPeriodi()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    Stop
    'If VM.AccodaPeriodiCheckati Then
    '  PeriodsManager.AggiornaColori()
    '  Me.Close()
    'End If
  End Sub

  Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
    Me.Close()
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsImportaPeriodiViewModel
  'Implements INotifyPropertyChanged

  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  Public Property ListaNuoviPeriodi As New ObservableCollection(Of clsPeriod2021)
  'Dim _ListaPeriodi As ObservableCollection(Of clsPeriod2021)

  'Public Property ListaNuoviPeriodi As ObservableCollection(Of clsPeriod2021)
  '  Get
  '    Return _ListaNuoviPeriodi
  '  End Get
  '  Set(value As ObservableCollection(Of clsPeriod2021))
  '    _ListaNuoviPeriodi = value
  '    OnPropertyChanged("ListaNuoviPeriodi")
  '  End Set
  'End Property

  Public ReadOnly Property ListaPeriodi As ObservableCollection(Of clsPeriod2021)
    Get
      Return PeriodsManager.Periods.Lista
    End Get
  End Property

  Public Sub CaricaPeriodi()
    Stop
    'Dim objF As New clsFiles2020
    'Dim fls = objF.SelezionaFiles(DataProvider2020.Files.First.FullName, "Import Periods", "Performance Period Files|*.prd;*.json;|All Files|*.*", ".prd", Nothing)
    '_ListaNuoviPeriodi.Clear()
    'For Each fl In fls
    '  Dim l = PeriodsManager.CercaPeriodiFile(fl)
    '  For Each p In l
    '    ListaNuoviPeriodi.Add(p)
    '  Next
    'Next
    'For Each prd In ListaNuoviPeriodi
    '  prd.IsChecked = False
    '  prd.Loaded = False
    '  For Each TR In DataProvider2020.SourceFilesTimeRanges
    '    If TR.IsFullyOverlapped(prd.TimeRange) Then
    '      prd.Loaded = True
    '      prd.IsChecked = NuovoPeriodo(prd)
    '      If prd.IsChecked Then Exit For
    '    End If
    '  Next
    '  If prd.Loaded Then prd.AggiornaValoriCanali()
    'Next
    'OnPropertyChanged("ListaNuoviPeriodi")
  End Sub

  'Private Sub ImpostaNuoviPeriodi(Periodi As List(Of clsPeriod2021))
  '  If Periodi Is Nothing Then Exit Sub
  '  Dim Contatore = PeriodsManager.IndiceNuovoPeriodo
  '  For Each Periodo In Periodi
  '    Periodo.Loaded = False
  '    For Each TRng In DataProvider2020.SourceFilesTimeRanges
  '      If TRng.IsInRange(Periodo.TimeRange.Start, True, True) AndAlso TRng.IsInRange(Periodo.TimeRange.Finish, True, True) Then
  '        Periodo.Loaded = True
  '        Exit For
  '      End If
  '    Next
  '    Periodo.Id = Contatore
  '    Periodo.AggiornaValoriCanali()
  '    ListaPeriodi.Add(Periodo)
  '    Contatore += 1
  '  Next
  'End Sub


  Private Function NuovoPeriodo(prd As clsPeriod2021) As Boolean
    Stop
    'For Each p In PeriodsManager.ListaPeriodi
    '  If p.TimeRange.HasSameRange(prd.TimeRange) Then
    '    Return False
    '  End If
    'Next
    'Return True
  End Function


  Public Function AccodaPeriodiCheckati() As Boolean
    Stop
    'Dim msg As String = ListaNuoviPeriodi.Where(Function(x) x.IsChecked).Count & " new periods selected (out of the " & ListaNuoviPeriodi.Count & " found)" & vbCrLf
    'msg &= ListaNuoviPeriodi.Where(Function(x) x.IsChecked).Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eAcceleration).Count & " Accelerations" & vbCrLf
    'msg &= ListaNuoviPeriodi.Where(Function(x) x.IsChecked).Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe).Count & " Gybes" & vbCrLf
    'msg &= ListaNuoviPeriodi.Where(Function(x) x.IsChecked).Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).Count & " Tacks" & vbCrLf
    'msg &= ListaNuoviPeriodi.Where(Function(x) x.IsChecked).Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLine).Count & " Straight Lines" & vbCrLf & vbCrLf
    'msg &= "Do you want to add all these selected to the " & PeriodsManager.ListaPeriodi.Count & " already existings?"
    'If MsgBox(msg, MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
    '  For Each p In ListaNuoviPeriodi
    '    If p.IsChecked Then
    '      PeriodsManager.ListaPeriodi.Add(p)
    '    End If
    '  Next
    '  PeriodsManager.SalvaPeriodiJsonFile()
    '  Return True
    'Else
    '  Return False
    'End If
  End Function

End Class