Imports System.ComponentModel
Imports Newtonsoft.Json
Imports PropertyChanged

Public Class UserControlParquetFinderFilter

  'Dim _ListaCanali As List(Of String)
  'Dim _VM As New clsFiltroParquetFinder
  Dim _VM As clsFiltroParquetFinder


  Public Property VM As clsFiltroParquetFinder
    Get
      Return _VM
    End Get
    Set(value As clsFiltroParquetFinder)
      _VM = value
    End Set
  End Property

  'Public Property ListaCanali As List(Of String)
  '  Get
  '    Return _ListaCanali
  '  End Get
  '  Set(value As List(Of String))
  '    _ListaCanali = value
  '  End Set
  'End Property

  'Public Sub New(Parent As clsParquetFinder, ListaCanali As List(Of String))
  Public Sub New(VM As clsFiltroParquetFinder)
    _VM = VM
    Me.DataContext = _VM
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    'Parent As clsParquetFinder, ListaCanali As List(Of String)
    '_VM.Parent = Parent
    '_VM.TipoFiltro = _VM.TipiFiltro.
    '_ListaCanali = VM.Parent.ListaCanali  'ListaCanali
  End Sub


  Private Sub ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
    _VM.VerificaNomeCanele2Enabled()
  End Sub

  Private Sub SelezionaCanale1(sender As Object, e As RoutedEventArgs)
    Dim selc As New List(Of String)
    selc.Add(VM.NomeCanale)
    Dim res As List(Of String) = GestisciListaCanali("Select Channel", VM.Parent.ListaCanali, selc, eSelectChannelType.eSingle)
    If res.Count > 0 Then
      _VM.NomeCanale = res.First
    End If

    'Dim CanaleDaSelezionare As String = VM.NomeCanale
    'Dim SelFromList As New UserControlSelFromList("Select Channel", VM.Parent.ListaCanali, CanaleDaSelezionare)
    'SelFromList.ShowDialog()
    'If SelFromList.DialogResult Then
    '  _VM.NomeCanale = SelFromList.CanaleSelezionato
    'End If
  End Sub

  Private Sub SelezionaCanale2(sender As Object, e As RoutedEventArgs)
    Dim selc As New List(Of String)
    selc.Add(VM.NomeCanale2)
    Dim res As List(Of String) = GestisciListaCanali("Select Channel", VM.Parent.ListaCanali, selc, eSelectChannelType.eSingle)
    If res.Count > 0 Then
      _VM.NomeCanale2 = res.First
    End If
    'Dim SelFromList As New UserControlSelFromList("Select Channel", VM.Parent.ListaCanali, _VM.NomeCanale2)
    'SelFromList.ShowDialog()
    'If SelFromList.DialogResult Then
    '  _VM.NomeCanale2 = SelFromList.CanaleSelezionato
    'End If
  End Sub

  'Public Sub ImpostaFiltro(NomeCanale As String, Parquet As clsParquetFile, TipoFiltro As clsFiltroParquetFinder.eTipoFiltro, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean) ', ListaCanali As List(Of String), Parent As clsParquetFinder)
  '  '_ListaCanali = ListaCanali
  '  _VM.ImpostaFiltro(NomeCanale, Parquet, TipoFiltro, LowerLimit, UpperLimit, AbsVal, VM.Parent)
  'End Sub

  'Public Sub ImpostaFiltro(NomeCanale As String, NomeCanale2 As String, Parquet As clsParquetFile, TipoFiltro As clsFiltroParquetFinder.eTipoFiltro, RelazioneTraCampi As clsFiltroParquetFinder.eRelazioneTraCampi, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean, AbsVal2 As Boolean) ', ListaCanali As List(Of String), Parent As clsParquetFinder)
  '  '_ListaCanali = ListaCanali
  '  _VM.ImpostaFiltro(NomeCanale, NomeCanale2, Parquet, TipoFiltro, RelazioneTraCampi, LowerLimit, UpperLimit, AbsVal, VM.Parent)
  'End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    If MsgBox("current filter will be removed, do you confirm it?", MsgBoxStyle.YesNo, "Filter") = MsgBoxResult.Yes Then
      _VM.EliminaDaListaParent()
    End If
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsFiltroParquetFinder
  'Implements INotifyPropertyChanged

  <JsonIgnore>
  Public Property Parent As clsParquetFinder
  Public Property NomeCanale As String
  Public Property ValoriCanale As Double()
  Public Property NomeCanale2 As String
  Public Property ValoriCanale2 As Double()
  <JsonIgnore>
  Public Property TipoFiltro As eTipoFiltro = eTipoFiltro.eNone
  Public Property TipoFiltroStringa As String
  <JsonIgnore>
  Public Property RelazioneTraCampi As eRelazioneTraCampi = eRelazioneTraCampi.eNone
  Public Property RelazioneTraCampiStringa As String
  Public Property UpperLimit As Double
  Public Property LowerLimit As Double
  Public Property Parquet As clsParquetFile
  <JsonIgnore>
  Public Property ValidFilter As Boolean = True
  Public Property AbsVal As Boolean
  <JsonIgnore>
  Public Property HeaderVisibility As Visibility = Visibility.Visible


  'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  'Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
  '  RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  'End Sub

  '<JsonIgnore>
  'Public Property ValidFilter As Boolean
  '  Get
  '    Return _ValidFilter
  '  End Get
  '  Set(value As Boolean)
  '    _ValidFilter = value
  '    OnPropertyChanged("ValidFilter")
  '  End Set
  'End Property

  'Public Property NomeCanale As String
  '  Get
  '    Return _NomeCanale
  '  End Get
  '  Set(value As String)
  '    _NomeCanale = value
  '    OnPropertyChanged("NomeCanale")
  '  End Set
  'End Property

  'Public Property NomeCanale2 As String
  '  Get
  '    Return _NomeCanale2
  '  End Get
  '  Set(value As String)
  '    _NomeCanale2 = value
  '    OnPropertyChanged("NomeCanale2")
  '  End Set
  'End Property

  'Public Property TipoFiltro As eTipoFiltro
  '  Get
  '    Return _TipoFiltro
  '  End Get
  '  Set(value As eTipoFiltro)
  '    _TipoFiltro = value

  '    'OnPropertyChanged("TipoFiltro")
  '    'OnPropertyChanged("TipoFiltroStringa")
  '    'OnPropertyChanged("LowerLimitAbilitato")
  '    'OnPropertyChanged("UpperLimitAbilitato")
  '  End Set
  'End Property

  'Public Property RelazioneTraCampi As eRelazioneTraCampi
  '  Get
  '    Return _RelazioneTraCampi
  '  End Get
  '  Set(value As eRelazioneTraCampi)
  '    _RelazioneTraCampi = value
  '    OnPropertyChanged("RelazioneTraCampi")
  '  End Set
  'End Property

  'Public Property UpperLimit As Double
  '  Get
  '    Return _UpperLimit
  '  End Get
  '  Set(value As Double)
  '    _UpperLimit = value
  '    OnPropertyChanged("UpperLimit")
  '    If LowerLimit > UpperLimit Then LowerLimit = UpperLimit
  '  End Set
  'End Property

  'Public Property LowerLimit As Double
  '  Get
  '    Return _LowerLimit
  '  End Get
  '  Set(value As Double)
  '    _LowerLimit = value
  '    OnPropertyChanged("LowerLimit")
  '    If LowerLimit > UpperLimit Then UpperLimit = LowerLimit
  '  End Set
  'End Property

  'Public Property AbsVal As Boolean
  '  Get
  '    Return _AbsVal
  '  End Get
  '  Set(value As Boolean)
  '    _AbsVal = value
  '    OnPropertyChanged("AbsVal")
  '  End Set
  'End Property

  <JsonIgnore>
  Public ReadOnly Property NomeCanele2Enabled As Boolean
    Get
      Return Not _RelazioneTraCampi = eRelazioneTraCampi.eNone ' OrElse Not _RelazioneTraCampi = Nothing
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property UpperLimitAbilitato As Boolean
    Get
      Return TipoFiltro = eTipoFiltro.eBetween OrElse TipoFiltro = eTipoFiltro.eMax
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property LowerLimitAbilitato As Boolean
    Get
      Return TipoFiltro = eTipoFiltro.eBetween OrElse TipoFiltro = eTipoFiltro.eMin
    End Get
  End Property

  Public Sub VerificaNomeCanele2Enabled()
    'OnPropertyChanged("NomeCanele2Enabled")
  End Sub

  Public Enum eTipoFiltro
    eBetween
    eMin
    eMax
    eNone
  End Enum

  Public Enum eRelazioneTraCampi
    eBoth
    eAtLeastOne
    eOneOnly
    eSum
    eSubtract
    eDivide
    eMultiply
    eNone
  End Enum

  '<JsonIgnore>
  'Public ReadOnly Property TipiFiltro
  '  Get
  '    Dim enumType As Type = GetType(eTipoFiltro)
  '    Dim Nomi = [Enum].GetNames(enumType)
  '    Return Nomi.ToList
  '  End Get
  'End Property

  '<JsonIgnore>
  'Public ReadOnly Property RelazioniTraCampi
  '  Get
  '    Dim enumType As Type = GetType(eRelazioneTraCampi)
  '    Dim Nomi = [Enum].GetNames(enumType)
  '    Return Nomi.ToList
  '  End Get
  'End Property

  '<JsonIgnore>
  'Public Property Parent As clsParquetFinder
  '  Get
  '    Return _Parent
  '  End Get
  '  Set(value As clsParquetFinder)
  '    _Parent = value
  '  End Set
  'End Property

  '<JsonIgnore>
  'Public Property HeaderVisibility As Visibility
  '  Get
  '    Return _HeaderVisibility
  '  End Get
  '  Set(value As Visibility)
  '    _HeaderVisibility = value
  '    OnPropertyChanged("HeaderVisibility")
  '  End Set
  'End Property

  'Public Property TipoFiltroStringa As String
  '  Get
  '    Return _TipoFiltroStringa
  '  End Get
  '  Set(value As String)
  '    _TipoFiltroStringa = value
  '    Dim et = GetType(eTipoFiltro)
  '    TipoFiltro = [Enum].Parse(et, value)

  '    'OnPropertyChanged("TipoFiltro")
  '    'OnPropertyChanged("TipoFiltroStringa")
  '    'OnPropertyChanged("LowerLimitAbilitato")
  '    'OnPropertyChanged("UpperLimitAbilitato")
  '  End Set
  'End Property

  'Public Property RelazioneTraCampiStringa As String
  '  Get
  '    Return _RelazioneTraCampiStringa
  '  End Get
  '  Set(value As String)
  '    If value = Nothing Then Exit Property
  '    _RelazioneTraCampiStringa = value
  '    'Dim et = GetType(eRelazioneTraCampi)
  '    '_RelazioneTraCampi = [Enum].Parse(et, value)
  '    'OnPropertyChanged("RelazioneTraCampi")
  '    'OnPropertyChanged("RelazioneTraCampiStringa")

  '  End Set
  'End Property

  Public Sub New() '(Parent As clsParquetFinder)
    'AggiornaAbilitazioni()
    'OnPropertyChanged("TipiFiltro")
    'OnPropertyChanged("RelazioniTraCampi")
  End Sub

  Public Sub New(NomeCanale As String, Parquet As clsParquetFile, TipoFiltro As eTipoFiltro, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean, Parent As clsParquetFinder)
    ImpostaFiltro(NomeCanale, Parquet, TipoFiltro, LowerLimit, UpperLimit, AbsVal, Parent)
    'AggiornaAbilitazioni()
  End Sub

  Public Sub New(NomeCanale As String, NomeCanale2 As String, Parquet As clsParquetFile, TipoFiltro As eTipoFiltro, RelazioneTraCampi As eRelazioneTraCampi, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean, Parent As clsParquetFinder)
    ImpostaFiltro(NomeCanale, NomeCanale2, Parquet, TipoFiltro, RelazioneTraCampi, LowerLimit, UpperLimit, AbsVal, Parent)
    'AggiornaAbilitazioni()
  End Sub

  Public Sub ImpostaFiltro(NomeCanale As String, Parquet As clsParquetFile, TipoFiltro As eTipoFiltro, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean, Parent As clsParquetFinder)
    _Parent = Parent
    _AbsVal = AbsVal
    _Parquet = Parquet
    _NomeCanale = NomeCanale
    _TipoFiltro = TipoFiltro
    _LowerLimit = LowerLimit
    _UpperLimit = UpperLimit
    _ValoriCanale = Parquet.FileParquet.CaricaValoriCanale(NomeCanale, Parquet.ParquetReader)
    If _ValoriCanale Is Nothing Then _ValidFilter = False
    If TipoFiltro = eTipoFiltro.eNone Then _ValidFilter = False
    'AggiornaAbilitazioni()
  End Sub

  Public Sub ImpostaFiltro(NomeCanale As String, NomeCanale2 As String, Parquet As clsParquetFile, TipoFiltro As eTipoFiltro, RelazioneTraCampi As eRelazioneTraCampi, LowerLimit As Double, UpperLimit As Double, AbsVal As Boolean, Parent As clsParquetFinder)
    _Parent = Parent
    _AbsVal = AbsVal
    _Parquet = Parquet
    _NomeCanale = NomeCanale
    _NomeCanale2 = NomeCanale2
    _TipoFiltro = TipoFiltro
    _RelazioneTraCampi = RelazioneTraCampi
    _LowerLimit = LowerLimit
    _UpperLimit = UpperLimit
    _ValoriCanale = Parquet.FileParquet.CaricaValoriCanale(NomeCanale, Parquet.ParquetReader)
    _ValoriCanale2 = Parquet.FileParquet.CaricaValoriCanale(NomeCanale2, Parquet.ParquetReader)
    If _ValoriCanale Is Nothing Then _ValidFilter = False
    If _ValoriCanale2 Is Nothing Then _ValidFilter = False
    If TipoFiltro = eTipoFiltro.eNone Then _ValidFilter = False
    'AggiornaAbilitazioni()
  End Sub

  Public Sub CaricaValoriCanali(Parquet As clsParquetFile)
    _ValoriCanale = Parquet.FileParquet.CaricaValoriCanale(NomeCanale, Parquet.ParquetReader)
    If _ValoriCanale Is Nothing Then _ValidFilter = False
    If Not _NomeCanale2 Is Nothing Then
      If Not RelazioneTraCampi = eRelazioneTraCampi.eNone Then
        _ValoriCanale2 = Parquet.FileParquet.CaricaValoriCanale(NomeCanale2, Parquet.ParquetReader)
        If _ValoriCanale2 Is Nothing Then _ValidFilter = False
      End If
    End If

  End Sub

  'Public Sub ScatenaOnPropertyChanged()
  '  OnPropertyChanged("NomeCanale")
  '  OnPropertyChanged("RelazioniTraCampi")
  '  OnPropertyChanged("NomeCanale2")
  '  OnPropertyChanged("TipoFiltro")
  '  OnPropertyChanged("UpperLimit")
  '  OnPropertyChanged("LowerLimit")
  '  OnPropertyChanged("AbsVal")
  '  OnPropertyChanged("LowerLimitAbilitato")
  '  OnPropertyChanged("UpperLimitAbilitato")
  '  OnPropertyChanged("NomeCanele2Enabled")
  '  OnPropertyChanged("LowerLimitAbilitato")
  '  OnPropertyChanged("UpperLimitAbilitato")
  '  OnPropertyChanged("ValidFilter")
  'End Sub


  Public Function IsValid(Indice As Integer) As Boolean
    If Not _NomeCanale2 = "" Then Return IsValidMultiChannel(Indice)
    Return Is1Valid(Indice)
  End Function

  Private Function Is1Valid(Indice As Integer) As Boolean
    If _AbsVal Then Return IsValidAbs(Indice)
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return Valore(Indice) <= _UpperLimit AndAlso Valore(Indice) >= _LowerLimit
      Case eTipoFiltro.eMax
        Return Valore(Indice) <= _UpperLimit
      Case eTipoFiltro.eMin
        Return Valore(Indice) >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Private Function IsValidCombined(Valore As Double) As Boolean
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return Valore <= _UpperLimit AndAlso Valore >= _LowerLimit
      Case eTipoFiltro.eMax
        Return Valore <= _UpperLimit
      Case eTipoFiltro.eMin
        Return Valore >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Private Function Is2Valid(Indice As Integer) As Boolean
    If _AbsVal Then Return Is2ValidAbs(Indice)
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return Valore2(Indice) <= _UpperLimit AndAlso Valore2(Indice) >= _LowerLimit
      Case eTipoFiltro.eMax
        Return Valore2(Indice) <= _UpperLimit
      Case eTipoFiltro.eMin
        Return Valore2(Indice) >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Private Function IsValidAbs(Indice As Integer) As Boolean
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return System.Math.Abs(Valore(Indice)) <= _UpperLimit AndAlso Valore(Indice) >= _LowerLimit
      Case eTipoFiltro.eMax
        Return System.Math.Abs(Valore(Indice)) <= _UpperLimit
      Case eTipoFiltro.eMin
        Return System.Math.Abs(Valore(Indice)) >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Private Function Is2ValidAbs(Indice As Integer) As Boolean
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        Return System.Math.Abs(Valore2(Indice)) <= _UpperLimit AndAlso Valore2(Indice) >= _LowerLimit
      Case eTipoFiltro.eMax
        Return System.Math.Abs(Valore2(Indice)) <= _UpperLimit
      Case eTipoFiltro.eMin
        Return System.Math.Abs(Valore2(Indice)) >= _LowerLimit
      Case Else
        Return True
    End Select
  End Function

  Private Function IsValidMultiChannel(Indice As Integer) As Boolean
    Select Case _RelazioneTraCampi
      Case eRelazioneTraCampi.eAtLeastOne
        Dim V1 As Boolean = Is1Valid(Indice)
        Dim V2 As Boolean = Is2Valid(Indice)
        Return V1 OrElse V2
      Case eRelazioneTraCampi.eBoth
        Dim V1 As Boolean = Is1Valid(Indice)
        Dim V2 As Boolean = Is2Valid(Indice)
        Return V1 AndAlso V2
      Case eRelazioneTraCampi.eDivide
        Return IsValidCombined(ValoreCombinato(Indice))
      Case eRelazioneTraCampi.eMultiply
        Return IsValidCombined(ValoreCombinato(Indice))
      Case eRelazioneTraCampi.eOneOnly
        Dim V1 As Boolean = Is1Valid(Indice)
        Dim V2 As Boolean = Is2Valid(Indice)
        Return V1 Xor V2
      Case eRelazioneTraCampi.eSubtract
        Return IsValidCombined(ValoreCombinato(Indice))
      Case eRelazioneTraCampi.eSum
        Return IsValidCombined(ValoreCombinato(Indice))
      Case Else
        Return False
    End Select
  End Function

  Private Function Valore(Indice As Integer) As Double
    Return _ValoriCanale(Indice)
  End Function

  Private Function Valore2(Indice As Integer) As Double
    Return _ValoriCanale2(Indice)
  End Function

  Private Function ValoreCombinato(Indice As Integer) As Double
    Dim V1 As Double = Valore(Indice)
    Dim V2 As Double = Valore2(Indice)
    If _AbsVal Then
      V1 = System.Math.Abs(V1)
      V2 = System.Math.Abs(V2)
    End If
    Select Case _RelazioneTraCampi
      Case eRelazioneTraCampi.eDivide
        Return V1 / V2
      Case eRelazioneTraCampi.eMultiply
        Return V1 * V2
      Case eRelazioneTraCampi.eSubtract
        Return V1 - V2
      Case eRelazioneTraCampi.eSum
        Return V1 + V2
      Case Else
        Return 0
    End Select
  End Function

  Public Function DescrizioneFiltro(Separatore As String) As String
    If Not _NomeCanale2 = "" AndAlso Not _RelazioneTraCampi = eRelazioneTraCampi.eNone Then Return DescrizioneFiltroDoppioCanale(Separatore)
    Dim strTmp As String = _NomeCanale & Separatore
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        strTmp &= "Between" & _LowerLimit.ToString("F0") & "And" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMax
        strTmp &= "Max" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMin
        strTmp &= "Min" & _LowerLimit.ToString("F0") '.Replace(".", "d")
      Case Else
        strTmp = ""
    End Select
    Return strTmp
  End Function

  Public Function DescrizioneFiltroDoppioCanale(Separatore As String) As String
    Dim strTmp As String = ""
    Select Case _RelazioneTraCampi
      Case eRelazioneTraCampi.eAtLeastOne
        strTmp = "AtLeastOneBetween" & _NomeCanale & "And" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eBoth
        strTmp = "Both" & _NomeCanale & "And" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eDivide
        strTmp = _NomeCanale & "DividedBy" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eMultiply
        strTmp = _NomeCanale & "MultipBy" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eOneOnly
        strTmp = "OneOnlyBetween" & _NomeCanale & "And" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eSubtract
        strTmp = _NomeCanale & "Minus" & _NomeCanale2 & "Is"
      Case eRelazioneTraCampi.eSum
        strTmp = _NomeCanale & "Plus" & _NomeCanale2 & "Is"
      Case Else
    End Select
    strTmp &= Separatore
    Select Case _TipoFiltro
      Case eTipoFiltro.eBetween
        strTmp &= "Between" & _LowerLimit.ToString("F0") & "And" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMax
        strTmp &= "Max" & _UpperLimit.ToString("F0") '.Replace(".", "d")
      Case eTipoFiltro.eMin
        strTmp &= "Min" & _LowerLimit.ToString("F0") '.Replace(".", "d")
      Case Else
        strTmp = ""
    End Select
    Return strTmp
  End Function

  Public Sub EliminaDaListaParent()
    _Parent.EliminaDaListaFiltri(Me)
  End Sub

End Class
