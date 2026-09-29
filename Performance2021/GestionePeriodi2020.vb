Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports SPwpf

Public Class clsPeriodsManager2020
  Implements INotifyPropertyChanged
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim pPathFileXml As String
  Dim pAccelerationSettings2020 As New clsAccelerationSettings2020
  Dim pPavarotSettings2020 As New clsPavarotSettings2020
  Dim pListaPeriodi As New ObservableCollection(Of clsPeriod2020)
  Dim pListaCanaliValoriBase As New ObservableCollection(Of clsChannels.eChannels)

  Public Property AccelerationSettings2020 As clsAccelerationSettings2020
    Get
      Return pAccelerationSettings2020
    End Get
    Set(value As clsAccelerationSettings2020)
      pAccelerationSettings2020 = value
    End Set
  End Property

  Public Property PavarotSettings2020 As clsPavarotSettings2020
    Get
      Return pPavarotSettings2020
    End Get
    Set(value As clsPavarotSettings2020)
      pPavarotSettings2020 = value
    End Set
  End Property

  Public Property ListaPeriodi As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi
    End Get
    Set(value As ObservableCollection(Of clsPeriod2020))
      pListaPeriodi = value
      OnPropertyChanged("ListaPeriodi")
      OnPropertyChanged("ListaAccelerazioni")
      OnPropertyChanged("ListaPavarot")
      OnPropertyChanged("ListaStraightLine")
      OnPropertyChanged("ListaAccelerazioniCheckate")
      OnPropertyChanged("ListaPavarotCheckate")
      OnPropertyChanged("ListaStraightLineCheckate")
      OnPropertyChanged("ListaAccelerazioniSelezionate")
      OnPropertyChanged("ListaPavarotSelezionate")
      OnPropertyChanged("ListaStraightLineSelezionate")
    End Set
  End Property

  Public ReadOnly Property ListaAccelerazioni As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eAcceleration)
    End Get
  End Property

  Public ReadOnly Property ListaPavarot As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eGybe Or x.PeriodType = clsPeriod2020.ePeriodType.eTack)
    End Get
  End Property

  Public ReadOnly Property ListaStraightLine As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eStraightLine)
    End Get
  End Property

  Public ReadOnly Property ListaAccelerazioniCheckate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eAcceleration).Where(Function(x) x.IsChecked)
    End Get
  End Property

  Public ReadOnly Property ListaPavarotCheckate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eGybe Or x.PeriodType = clsPeriod2020.ePeriodType.eTack).Where(Function(x) x.IsChecked)
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineCheckate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eStraightLine).Where(Function(x) x.IsChecked)
    End Get
  End Property

  Public ReadOnly Property ListaAccelerazioniSelezionate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eAcceleration).Where(Function(x) x.IsSelected)
    End Get
  End Property

  Public ReadOnly Property ListaPavarotSelezionate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eGybe Or x.PeriodType = clsPeriod2020.ePeriodType.eTack).Where(Function(x) x.IsSelected)
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineSelezionate As ObservableCollection(Of clsPeriod2020)
    Get
      Return pListaPeriodi.Where(Function(x) x.PeriodType = clsPeriod2020.ePeriodType.eStraightLine).Where(Function(x) x.IsSelected)
    End Get
  End Property

  Public Property ListaCanaliValoriBase As ObservableCollection(Of clsChannels.eChannels)
    Get
      Return pListaCanaliValoriBase
    End Get
    Set(value As ObservableCollection(Of clsChannels.eChannels))
      pListaCanaliValoriBase = value
    End Set
  End Property

  Public Sub New()
    pListaCanaliValoriBase.Add(clsChannels.eChannels.eSOW)
    pListaCanaliValoriBase.Add(clsChannels.eChannels.eTWA)
    pListaCanaliValoriBase.Add(clsChannels.eChannels.eTWS)
    pListaCanaliValoriBase.Add(clsChannels.eChannels.eTWD)
    CaricaSettaggi()
  End Sub

  Private Sub CaricaSettaggi()
    AccelerationSettings2020.LeggiSettaggi(Nothing, "")
    PavarotSettings2020.LeggiSettaggi(Nothing, "")
  End Sub

  Private Sub CaricaPeriodiDaXml()
    Stop
  End Sub



End Class

Public Class clsAccelerationSettings2020
  Implements INotifyPropertyChanged
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim pBsLimite As Double ' BS superata la quale ha inizio l accelerazione, é lo zero dell'accelerazione
  Dim pTakeOffSpeed As Double ' BS necessaria al take off
  Dim pTestDuration As Double ' secondi dopo superata la bs limite per dove l accelerazione viene considerata conclusa


  Public Sub New()

  End Sub

  Public Sub New(BsLimite As Double, TakeOffSpeed As Double, TestDuration As Double)
    pBsLimite = BsLimite
    pTakeOffSpeed = TakeOffSpeed
    pTestDuration = TestDuration
  End Sub

  Public Property BsLimite As Double
    Get
      Return pBsLimite
    End Get
    Set(value As Double)
      pBsLimite = value
      OnPropertyChanged("BsLimite")
    End Set
  End Property

  Public Property TakeOffSpeed As Double
    Get
      Return pTakeOffSpeed
    End Get
    Set(value As Double)
      pTakeOffSpeed = value
      OnPropertyChanged("TakeOffSpeed")
    End Set
  End Property

  Public Property TestDuration As Double
    Get
      Return pTestDuration
    End Get
    Set(value As Double)
      pTestDuration = value
      OnPropertyChanged("TestDuration")
    End Set
  End Property

  Public Sub LeggiSettaggi(FileSettaggi As clsSettings, Suffisso As String)

  End Sub

  Public Sub SalvaSettaggi(FileSettaggi As clsSettings, Suffisso As String)

  End Sub


End Class

Public Class clsPavarotSettings2020
  Implements INotifyPropertyChanged
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim pSecsAnte As Double = 7
  Dim pSecsPost As Double = 20
  Dim pSamplingSeconds As Double = 5

  Public Sub New()

  End Sub

  Public Sub New(SecsAnte As Double, SecsPost As Double, SamplingSeconds As Double)
    pSecsAnte = SecsAnte
    pSecsPost = SecsPost
    pSamplingSeconds = SamplingSeconds
  End Sub

  Public Property SecsAnte As Double
    Get
      Return pSecsAnte
    End Get
    Set(value As Double)
      pSecsAnte = value
      OnPropertyChanged("SecsAnte")
    End Set
  End Property

  Public Property SecsPost As Double
    Get
      Return pSecsPost
    End Get
    Set(value As Double)
      pSecsPost = value
      OnPropertyChanged("SecsPost")
    End Set
  End Property

  Public Property SamplingSeconds As Double
    Get
      Return pSamplingSeconds
    End Get
    Set(value As Double)
      pSamplingSeconds = value
      OnPropertyChanged("SamplingSeconds")
    End Set
  End Property

  Public Sub LeggiSettaggi(FileSettaggi As clsSettings, Suffisso As String)

  End Sub

  Public Sub SalvaSettaggi(FileSettaggi As clsSettings, Suffisso As String)

  End Sub

End Class

Public Class clsPeriod2020
  Implements INotifyPropertyChanged
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim pTimeRange As clsTimeRange2020
  Dim pId As Integer
  Dim pSqlId As Integer = -1
  Dim pShortDescription As String = ""
  Dim pExtendedDescription As String = ""
  Dim pKeys As String = ""
  Dim pPeriodType As ePeriodType = ePeriodType.eUndefined
  Dim pKeyMoment As DateTime
  Dim pIsSelected As Boolean = False
  Dim pIsChecked As Boolean = True
  Dim pColore As Color = Colors.Gray
  Dim pColoreSfondo As Color = Colors.White
  Dim pMure As Color = Colors.SpringGreen

  Dim pSuffisso As String



  Public Property TimeRange As clsTimeRange2020
    Get
      Return pTimeRange
    End Get
    Set(value As clsTimeRange2020)
      pTimeRange = value
      OnPropertyChanged("TimeRange")
    End Set
  End Property

  Public Property Id As Integer
    Get
      Return pId
    End Get
    Set(value As Integer)
      pId = value
      OnPropertyChanged("Id")
    End Set
  End Property

  Public Property SqlId As Integer
    Get
      Return pSqlId
    End Get
    Set(value As Integer)
      pSqlId = value
      OnPropertyChanged("SqlId")
    End Set
  End Property

  Public Property ShortDescription As String
    Get
      Return pShortDescription
    End Get
    Set(value As String)
      pShortDescription = value
      OnPropertyChanged("ShortDescription")
    End Set
  End Property

  Public Property ExtendedDescription As String
    Get
      Return pExtendedDescription
    End Get
    Set(value As String)
      pExtendedDescription = value
      OnPropertyChanged("ExtendedDescription")
    End Set
  End Property

  Public Property Keys As String
    Get
      Return pKeys
    End Get
    Set(value As String)
      pKeys = value
      OnPropertyChanged("Keys")
    End Set
  End Property

  Public Property PeriodType As ePeriodType
    Get
      Return pPeriodType
    End Get
    Set(value As ePeriodType)
      pPeriodType = value
      OnPropertyChanged("PeriodType")
    End Set
  End Property

  Public Property KeyMoment As Date
    Get
      Return pKeyMoment
    End Get
    Set(value As Date)
      pKeyMoment = value
      OnPropertyChanged("KeyMoment")
    End Set
  End Property

  Public Property IsSelected As Boolean
    Get
      Return pIsSelected
    End Get
    Set(value As Boolean)
      pIsSelected = value
      OnPropertyChanged("IsSelected")
    End Set
  End Property

  Public Property IsChecked As Boolean
    Get
      Return pIsChecked
    End Get
    Set(value As Boolean)
      pIsChecked = value
      OnPropertyChanged("IsChecked")
    End Set
  End Property

  Public Property Colore As Color
    Get
      Return pColore
    End Get
    Set(value As Color)
      pColore = value
      OnPropertyChanged("Colore")
    End Set
  End Property

  Public Property ColoreSfondo As Color
    Get
      Return pColoreSfondo
    End Get
    Set(value As Color)
      pColoreSfondo = value
      OnPropertyChanged("ColoreSfondo")
    End Set
  End Property

  Public Property Mure As Color
    Get
      Return pMure
    End Get
    Set(value As Color)
      pMure = value
      OnPropertyChanged("Mure")
    End Set
  End Property

  Public Enum ePeriodType
    eUndefined = -1
    eStraightLine = 0
    eTack = 1
    eGybe = 2
    eRoundUp = 3
    eBrAway = 4
    eStart = 5
    eFinish = 6
    eAcceleration = 7
  End Enum




End Class

Public Class clsValoriPeriodoCanale2020
  Implements INotifyPropertyChanged
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub

  Dim pCanale As clsChannel2020
  Dim pPeriodo As clsPeriod2020
  Dim pAvg As Double
  Dim pMin As Double
  Dim pMax As Double
  Dim pDs As Double

  Public ReadOnly Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
  End Property

  Public ReadOnly Property Periodo As clsPeriod2020
    Get
      Return pPeriodo
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

  Public Sub New(Canale As clsChannel2020, Periodo As clsPeriod2020)
    pCanale = Canale
    pPeriodo = Periodo
  End Sub

  Public Sub AggiornaValori(Periodo As clsPeriod2020)
    pPeriodo = Periodo

  End Sub

  Private Sub AggiornaValori()
    If pCanale.Valori.Count = 0 Then
      pAvg = 0
      pMin = 0
      pMax = 0
      pDs = 0
    Else

      Dim DT2020 As New clsDataProvider2020
      Dim Valori As Double() = DT2020.ValoriIntervallo(DT2020.Channels.CanaleDT.ValoriDT, pCanale.Valori, pPeriodo.TimeRange)
      pAvg = Valori.Average
      pMax = Valori.Max
      pMin = Valori.Min
      alglib.basestat.sampleadev(Valori, Valori.Count, pDs)


      'Dim ValoriConIndici = pCanale.CanaleDT.ValoriDT.Select(Function(Valore, Indice) New With {.val = Valore, .index = Indice})
      '   Dim Inizio = ValoriConIndici.Where(Function(x) x.val >= pPeriodo.TimeRange.Inizio)
      '   Dim ID As Integer = Inizio.FirstOrDefault.index
      '   Dim Fine = ValoriConIndici.Where(Function(x) x.val >= pPeriodo.TimeRange.Fine)
      '   Dim IDf As Integer = Inizio.LastOrDefault.index

      '   ID = -1
      '   IDf = -1
      '   For i As Integer = 0 To pCanale.CanaleDT.ValoriDT.Count - 1
      '     If ID = -1 Then
      '       If pCanale.CanaleDT.ValoriDT(i) >= pPeriodo.TimeRange.Inizio Then
      '         ID = i
      '       End If
      '     End If
      '     If IDf = -1 Then
      '       If pCanale.CanaleDT.ValoriDT(i) >= pPeriodo.TimeRange.Fine Then
      '         IDf = i
      '         Exit Sub
      '       End If
      '     End If
      '   Next

      '   Dim L As Integer = 0
      '   Dim U As Integer = pCanale.CanaleDT.ValoriDT.Count - 1
      '   Dim DTtmp As DateTime
      '   Dim IndexTmp As Integer
      '   Do While U - L > 0
      '     IndexTmp = (U + L) / 2
      '     DTtmp = pCanale.CanaleDT.ValoriDT(IndexTmp)
      '     If pPeriodo.TimeRange.Inizio = DTtmp Then
      '       Exit Do
      '     ElseIf pPeriodo.TimeRange.Inizio > DTtmp Then
      '       L = IndexTmp
      '     Else
      '       U = IndexTmp
      '     End If

      '   Loop


      '   Dim Valori = pCanale.Valori.ToList.GetRange(1, 100)

    End If

    OnPropertyChanged("Periodo")
    OnPropertyChanged("Avg")
    OnPropertyChanged("Max")
    OnPropertyChanged("Min")
    OnPropertyChanged("Ds")
  End Sub

End Class

Public Class clsDataProviders2020

End Class

Public Class clsDataProvider2020
  'viene creato un data provider per barca
  Dim pChannels As New clsChannels2020(Me)
  Dim pFileType As eFileType

  Public Sub New(PathFiles As String())

  End Sub

  Public Enum eFileType
    eParquet = 0
    eFaRoBin = 1
    eGombocSqlLite = 2
    eFaRoCsv = 3
    eExplog = 4
    eDfwLog = 5
    eImoth = 6
  End Enum

  Public Property Channels As clsChannels2020
    Get
      Return pChannels
    End Get
    Set(value As clsChannels2020)
      pChannels = value
    End Set
  End Property

  Public Property FileType As eFileType
    Get
      Return pFileType
    End Get
    Set(value As eFileType)
      pFileType = value
    End Set
  End Property

  Public Function TrovaIndice(ArrayDT As DateTime(), Momento As DateTime) As Integer
    Dim L As Integer = 0
    Dim U As Integer = ArrayDT.Count - 1
    Dim DTtmp As DateTime
    Dim IndexTmp As Integer
    Do While U - L > 1
      IndexTmp = CInt((U + L) / 2)
      DTtmp = ArrayDT(IndexTmp)
      If Momento = DTtmp Then
        Exit Do
      ElseIf Momento > DTtmp Then
        L = IndexTmp
      Else
        U = IndexTmp
      End If
    Loop
    Return IndexTmp
  End Function

  Public Function ValoriIntervallo(ArrayCompleto As Double(), IndiceIniziale As Integer, IndiceFinale As Integer) As Double()
    Dim Risultato(IndiceFinale - IndiceIniziale) As Double
    Array.Copy(ArrayCompleto, IndiceIniziale, Risultato, 0, IndiceFinale - IndiceIniziale)
    Return Risultato
  End Function

  Public Function ValoriIntervallo(ArrayDT As DateTime(), ArrayValori As Double(), MomentoIniziale As DateTime, MomentoFinale As DateTime) As Double()
    Dim IndiceIniziale As Integer = TrovaIndice(ArrayDT, MomentoIniziale)
    Dim IndiceFinale As Integer = TrovaIndice(ArrayDT, MomentoFinale)
    Return ValoriIntervallo(ArrayValori, IndiceIniziale, IndiceFinale)
  End Function

  Public Function ValoriIntervallo(ArrayDT As DateTime(), ArrayValori As Double(), TimeRange As clsTimeRange2020) As Double()
    Dim IndiceIniziale As Integer = TrovaIndice(ArrayDT, TimeRange.Inizio)
    Dim IndiceFinale As Integer = TrovaIndice(ArrayDT, TimeRange.Fine)
    Return ValoriIntervallo(ArrayValori, IndiceIniziale, IndiceFinale)
  End Function

End Class

Public Class clsDefaultChannels2020

End Class

Public Class clsChannels2020
  Dim pCanaleDT As clsChannel2020
  Dim pListaCanaliConosciuti As List(Of clsChannel2020)
  Dim pListaCanaliDefault As List(Of clsChannel2020)
  Dim pListaCanali As ObservableCollection(Of clsChannel2020)
  Dim pParentDataProvider As clsDataProvider2020

  Public Sub New(ParentDataProvider As clsDataProvider2020)
    pParentDataProvider = ParentDataProvider

  End Sub

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
    ePortTipPiercingAngle = 22
    eStbdTipPiercingAngle = 23
    ePortCantAngle = 24
    eStbdCantAngle = 25
    eRideHeightPointX = 26
    eRideHeightPointY = 27
    eIsStbd = 28
    eVMG = 29
    eVMGp = 30

    eBSTp = 31
    eBSPp = 32
    eTWAd = 33
    eRideHeight = 34
    eLwdTipPiercingAngle = 35
    'eMWA = 36
    eSOGms = 37
    eIsPavarot = 38

    ePortFoilOutFlap2Angle = 39
    ePortFoilOutFlap1Angle = 40
    ePortFoilInFlap1Angle = 41
    ePortFoilInFlap2Angle = 42

    eStbdFoilInFlap2Angle = 43
    eStbdFoilInFlap1Angle = 44
    eStbdFoilOutFlap1Angle = 45
    eStbdFoilOutFlap2Angle = 46

    eLwdCantAngle = 47
    eWwdCantAngle = 48

    eLwdFoilOutFlap2Angle = 49
    eLwdFoilOutFlap1Angle = 50
    eLwdFoilInFlap1Angle = 51
    eLwdFoilInFlap2Angle = 52

    eWwdFoilInFlap2Angle = 53
    eWwdFoilInFlap1Angle = 54
    eWwdFoilOutFlap1Angle = 55
    eWwdFoilOutFlap2Angle = 56

    eCSE = 57

    ePortCantAngleEffective = 58
    eStbdCantAngleEffective = 59
    eLwdCantAngleEffective = 60
    eWwdCantAngleEffective = 61
    eRdrRakeEffective = 62

    eBsTwsRatio = 63
    eVmgTwsRatio = 64

    eTrimRT = 65
    eHeelRT = 66

    ePortV1 = 67
    ePortD1 = 68
    ePortRunner = 69
    ePortJibSheet = 70
    ePortJibUpDn = 71

    eStbdV1 = 72
    eStbdD1 = 73
    eStbdRunner = 74
    eStbdJibSheet = 75
    eStbdJibUpDn = 76

    eLwdV1 = 77
    eLwdD1 = 78
    eLwdRunner = 79
    eLwdJibSheet = 80
    eLwdJibUpDn = 81

    eWwdV1 = 82
    eWwdD1 = 83
    eWwdRunner = 84
    eWwdJibSheet = 85
    eWwdJibUpDn = 86

    eSpannerCounter = 87
    eTopCtrlArmCounter = 88
    eTravellerCounter = 89

    eSpanner = 90
    eTopCtrlArm = 91
    eTraveller = 92
    eTopTwist = 93

    eRoll = 94
  End Enum

  Public Property CanaleDT As clsChannel2020
    Get
      Return pCanaleDT
    End Get
    Set(value As clsChannel2020)
      pCanaleDT = value
    End Set
  End Property

  Public Property ListaCanali As ObservableCollection(Of clsChannel2020)
    Get
      Return pListaCanali
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      pListaCanali = value
    End Set
  End Property

  Public ReadOnly Property ParentDataProvider As clsDataProvider2020
    Get
      Return pParentDataProvider
    End Get
  End Property


  Private Sub CaricaCanaliDefault()
    pListaCanaliDefault.Clear()
    Dim NomiCanaliChiave As String() = System.Enum.GetNames(GetType(eCanaliChiave))
    Dim ChiaviCanaliChiave As eCanaliChiave() = System.Enum.GetValues(GetType(eCanaliChiave))



    For Each CanaleChiave In ChiaviCanaliChiave
      Dim pChannelId As String = NomiCanaliChiave(CanaleChiave).TrimStart("e")
      Dim pCanaleChiave As clsChannels2020.eCanaliChiave = CanaleChiave
      Dim pShortName As String = pChannelId
      Dim pLongName As String = pChannelId
      Dim pShortUM As String = "°"
      Dim pLongUM As String = "Degrees"
      Dim pDataType As clsChannel2020.eDataType = clsChannel2020.eDataType.eLinear
      Dim pActualLogHeader As String = "_" & pChannelId
      Dim pKnownHeaders As New List(Of String)(New String() {pChannelId})
      Dim pPolarHeader As String = ""
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
          pKnownHeaders = New List(Of String)(New String() {"SystemTime_Date"})
        Case eCanaliChiave.eTimeOnly
          pShortName = "Tm"
          pLongName = "Time"
          pShortUM = ""
          pLongUM = ""
          pDataType = clsChannel2020.eDataType.eTimeOnly
          pKnownHeaders = New List(Of String)(New String() {"SystemTime_Local"})
        Case eCanaliChiave.eSOW
          pShortName = "Sow"
          pLongName = "Speed Over Water"
          pShortUM = "K"
          pLongUM = "Kts"
          pPolarHeader = "Bs"
          pKnownHeaders = New List(Of String)(New String() {"Bs"})
        Case eCanaliChiave.eHDG
          pShortName = "Hdg"
          pLongName = "Heading"
          pDataType = clsChannel2020.eDataType.e360
          pKnownHeaders = New List(Of String)(New String() {"Hdg"})
        Case eCanaliChiave.eSOG
          pShortName = "Sog"
          pLongName = "Speed Over Ground"
          pShortUM = "K"
          pLongUM = "Kts"
          pKnownHeaders = New List(Of String)(New String() {"Sog"})
        Case eCanaliChiave.eCOG
          pShortName = "Cog"
          pLongName = "Course Over Ground"
          pDataType = clsChannel2020.eDataType.e360
          pKnownHeaders = New List(Of String)(New String() {"Cog"})
        Case eCanaliChiave.eLat
          pShortName = "Lat"
          pLongName = "Latitude"
          pKnownHeaders = New List(Of String)(New String() {"LatBow"})
          pDecimals = 6
        Case eCanaliChiave.eLng
          pShortName = "Long"
          pLongName = "Longitude"
          pKnownHeaders = New List(Of String)(New String() {"LonBow"})
          pDecimals = 6
        Case eCanaliChiave.eLWY
          pShortName = "Lwy"
          pLongName = "Leeway"
          pKnownHeaders = New List(Of String)(New String() {"Lwy"})
        Case eCanaliChiave.eHEEL
          pShortName = "Heel"
          pLongName = "Heel"
          pPolarHeader = "Heel"
          pKnownHeaders = New List(Of String)(New String() {"Heel"})
        Case eCanaliChiave.eTRIM
          pShortName = "Trm"
          pLongName = "Trim"
          pPolarHeader = "Trim"
          pKnownHeaders = New List(Of String)(New String() {"Trim"})
        Case eCanaliChiave.eYRT
          pShortName = "YawRt"
          pLongName = "Yaw Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
          pKnownHeaders = New List(Of String)(New String() {"YawRate"})
        Case eCanaliChiave.ePRT
          pShortName = "PtcRt"
          pLongName = "Pitch Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
          pKnownHeaders = New List(Of String)(New String() {"PitchRate"})
        Case eCanaliChiave.eRRT
          pShortName = "RollRt"
          pLongName = "Roll Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
          pKnownHeaders = New List(Of String)(New String() {"RollRate"})
        Case eCanaliChiave.eTWA
          pShortName = "Twa"
          pLongName = "True Wind Angle"
          pDataType = clsChannel2020.eDataType.eAbs180
          pPolarHeader = "Twa"
          pKnownHeaders = New List(Of String)(New String() {"Twa"})
        Case eCanaliChiave.eTWS
          pShortName = "Tws"
          pLongName = "True Wind Speed"
          pShortUM = "K"
          pLongUM = "Kts"
          pKnownHeaders = New List(Of String)(New String() {"Tws"})
        Case eCanaliChiave.eTWD
          pShortName = "Twd"
          pLongName = "True Wind Direction"
          pDataType = clsChannel2020.eDataType.e360
          pKnownHeaders = New List(Of String)(New String() {"Twd"})
        Case eCanaliChiave.eAWA
          pShortName = "Awa"
          pLongName = "Apparent Wind Angle"
          pDataType = clsChannel2020.eDataType.eAbs180
          pKnownHeaders = New List(Of String)(New String() {"Awa"})
        Case eCanaliChiave.eAWS
          pShortName = "Aws"
          pLongName = "Apparent Wind Speed"
          pShortUM = "K"
          pLongUM = "Kts"
          pKnownHeaders = New List(Of String)(New String() {"Aws"})
        Case eCanaliChiave.eRdrAngle
          pShortName = "Rdr"
          pLongName = "Rudder Angle"
          pKnownHeaders = New List(Of String)(New String() {"Rudder_Ang"})
        Case eCanaliChiave.eRdrRake
          pShortName = "RdrRk"
          pLongName = "Rudder Rake"
          pKnownHeaders = New List(Of String)(New String() {"RudderRake_Ang"})
        Case eCanaliChiave.ePortTipPiercingAngle
          pIsMath = True
          pShortName = "PortTip"
          pLongName = "Port Tip Piercing Angle"
        Case eCanaliChiave.eStbdTipPiercingAngle
          pIsMath = True
          pShortName = "StbdTip"
          pLongName = "Stbd Tip Piercing Angle"
        Case eCanaliChiave.ePortCantAngle
          pShortName = "PortCant"
          pLongName = "Port Cant Angle"
          pKnownHeaders = New List(Of String)(New String() {"FCS_PortCant_Ang"})
        Case eCanaliChiave.eStbdCantAngle
          pShortName = "StbdCant"
          pLongName = "Stbd Cant Angle"
          pKnownHeaders = New List(Of String)(New String() {"FCS_StbdCant_Ang"})
        Case eCanaliChiave.eRideHeightPointX
          pShortName = "RhX"
          pLongName = "Ride Height Point X"
          pShortUM = "M"
          pLongUM = "Meters"
        Case eCanaliChiave.eRideHeightPointY
          pShortName = "RhY"
          pLongName = "Ride Height Point Y"
          pShortUM = "M"
          pLongUM = "Meters"
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
        Case eCanaliChiave.eRideHeight
          pShortName = "Rh"
          pLongName = "Ride Height"
          pShortUM = "M"
          pLongUM = "Meters"
          pPolarHeader = "Rh"
        Case eCanaliChiave.eLwdTipPiercingAngle
          pIsMath = True
          pShortName = "PiercAng"
          pLongName = "Leeward Tip Piercing Angle"
        'Case eCanaliChiave.eMWA
        Case eCanaliChiave.eSOGms
          pIsMath = True
          pShortUM = "MS"
          pLongUM = "MetSec"
        Case eCanaliChiave.eIsPavarot
          pIsMath = True
          pDataType = clsChannel2020.eDataType.eBoolean
        Case eCanaliChiave.ePortFoilOutFlap2Angle
          pShortName = "PortOutFlap2"
          pLongName = "Port Outboard Foil Flap 2"
          pKnownHeaders = New List(Of String)(New String() {"PortOutFlap2_Ang"})
        Case eCanaliChiave.ePortFoilOutFlap1Angle
          pShortName = "PortOutFlap1"
          pLongName = "Port Outboard Foil Flap 1"
          pKnownHeaders = New List(Of String)(New String() {"PortOutFlap1_Ang"})
        Case eCanaliChiave.ePortFoilInFlap1Angle
          pShortName = "PortInFlap1"
          pLongName = "Port Inboard Foil Flap 1"
          pKnownHeaders = New List(Of String)(New String() {"PortInFlap1_Ang"})
        Case eCanaliChiave.ePortFoilInFlap2Angle
          pShortName = "PortInFlap2"
          pLongName = "Port Inboard Foil Flap 2"
          pKnownHeaders = New List(Of String)(New String() {"PortOutFlap2_Ang"})
        Case eCanaliChiave.eStbdFoilInFlap2Angle
          pShortName = "StbdOutFlap2"
          pLongName = "Stbd Outboard Foil Flap 2"
          pKnownHeaders = New List(Of String)(New String() {"StbdOutFlap2_Ang"})
        Case eCanaliChiave.eStbdFoilInFlap1Angle
          pShortName = "StbdOutFlap1"
          pLongName = "Stbd Outboard Foil Flap 1"
          pKnownHeaders = New List(Of String)(New String() {"StbdOutFlap1_Ang"})
        Case eCanaliChiave.eStbdFoilOutFlap1Angle
          pShortName = "StbdInFlap1"
          pLongName = "Stbd Inboard Foil Flap 1"
          pKnownHeaders = New List(Of String)(New String() {"StbdInFlap1_Ang"})
        Case eCanaliChiave.eStbdFoilOutFlap2Angle
          pShortName = "StbdInFlap2"
          pLongName = "Stbd Inboard Foil Flap 2"
          pKnownHeaders = New List(Of String)(New String() {"StbdInFlap2_Ang"})
        Case eCanaliChiave.eLwdCantAngle
          pIsMath = True
          pShortName = "LwdCant"
          pLongName = "Leeward Cant Angle"
          pPolarHeader = "Cant"
        Case eCanaliChiave.eWwdCantAngle
          pIsMath = True
          pShortName = "WwdCant"
          pLongName = "Windward Cant Angle"
        Case eCanaliChiave.eLwdFoilOutFlap2Angle
          pIsMath = True
          pShortName = "LwdOutFlap2"
          pLongName = "Leeward Outboard Foil Flap 2"
          pPolarHeader = "OutFoil"
        Case eCanaliChiave.eLwdFoilOutFlap1Angle
          pIsMath = True
          pShortName = "LwdOutFlap1"
          pLongName = "Leeward Outboard Foil Flap 1"
          pPolarHeader = "OutFoil"
        Case eCanaliChiave.eLwdFoilInFlap1Angle
          pIsMath = True
          pShortName = "LwdInFlap1"
          pLongName = "Leeward Inboard Foil Flap 1"
          pPolarHeader = "InFoil"
        Case eCanaliChiave.eLwdFoilInFlap2Angle
          pIsMath = True
          pShortName = "LwdInFlap2"
          pLongName = "Leeward Inboard Foil Flap 2"
          pPolarHeader = "InFoil"
        Case eCanaliChiave.eWwdFoilInFlap2Angle
          pIsMath = True
          pShortName = "WwdOutFlap2"
          pLongName = "Windward Outboard Foil Flap 2"
        Case eCanaliChiave.eWwdFoilInFlap1Angle
          pIsMath = True
          pShortName = "WwdOutFlap1"
          pLongName = "Windward Outboard Foil Flap 1"
        Case eCanaliChiave.eWwdFoilOutFlap1Angle
          pIsMath = True
          pShortName = "WwdInFlap1"
          pLongName = "Windward Inboard Foil Flap 1"
        Case eCanaliChiave.eWwdFoilOutFlap2Angle
          pIsMath = True
          pShortName = "WwdInFlap2"
          pLongName = "Windward Inboard Foil Flap 2"
        Case eCanaliChiave.eCSE
          pShortName = "Cse"
          pLongName = "Course Over Water"
          pDataType = clsChannel2020.eDataType.e360
        Case eCanaliChiave.ePortCantAngleEffective
          pIsMath = True
          pShortName = "EffPortCant"
          pLongName = "Port Cant Effective Angle"
        Case eCanaliChiave.eStbdCantAngleEffective
          pIsMath = True
          pShortName = "EffStbdCant"
          pLongName = "Stbd Cant Effective Angle"
        Case eCanaliChiave.eLwdCantAngleEffective
          pIsMath = True
          pShortName = "EffCant"
          pLongName = "Leeward Cant Effective Angle"
          pPolarHeader = "Cant"
        Case eCanaliChiave.eWwdCantAngleEffective
          pIsMath = True
          pShortName = "EffWwdCant"
          pLongName = "Windward Cant Effective Angle"
        Case eCanaliChiave.eRdrRakeEffective
          pIsMath = True
          pShortName = "EffRdrRk"
          pLongName = "Rudder Effective Rake Angle"
          pPolarHeader = "Rake"
        Case eCanaliChiave.eBsTwsRatio
          pIsMath = True
          pDataType = clsChannel2020.eDataType.ePercentage
        Case eCanaliChiave.eVmgTwsRatio
          pIsMath = True
          pDataType = clsChannel2020.eDataType.ePercentage
        Case eCanaliChiave.eTrimRT
          pShortName = "TrmRt"
          pLongName = "Trim Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
          pKnownHeaders = New List(Of String)(New String() {"TrimRate"})
        Case eCanaliChiave.eHeelRT
          pShortName = "HeelRt"
          pLongName = "Heel Rate"
          pShortUM = "°/S"
          pLongUM = "Deg/Sec"
          pKnownHeaders = New List(Of String)(New String() {"HeelRate"})
        Case eCanaliChiave.ePortV1
          pLongName = "Port V1"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"PortV1Pin_Load"})
        Case eCanaliChiave.ePortD1
          pLongName = "Port D1"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"PortD1Pin_Load"})
        Case eCanaliChiave.ePortRunner
          pLongName = "Port Runner"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"PortRunnerRam_Load"})
        Case eCanaliChiave.ePortJibSheet
          pLongName = "Port Jib Sheet"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"PortJibSheetRam_Load"})
        Case eCanaliChiave.ePortJibUpDn
          pLongName = "Port Jib Up Dn"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"PortJibUpDwRam_Load"})
        Case eCanaliChiave.eStbdV1
          pLongName = "Stbd V1"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"StbdV1Pin_Load"})
        Case eCanaliChiave.eStbdD1
          pLongName = "Stbd D1"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"StbdD1Pin_Load"})
        Case eCanaliChiave.eStbdRunner
          pLongName = "Stbd Runner"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"StbdRunnerRam_Load"})
        Case eCanaliChiave.eStbdJibSheet
          pLongName = "Stbd Jib Sheet"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"StbdJibSheetRam_Load"})
        Case eCanaliChiave.eStbdJibUpDn
          pLongName = "Stbd Jib Up Dn"
          pShortUM = "T"
          pLongUM = "Tons"
          pKnownHeaders = New List(Of String)(New String() {"StbdJibUpDwRam_Load"})
        Case eCanaliChiave.eLwdV1
          pIsMath = True
          pLongName = "Leeward V1"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eLwdD1
          pIsMath = True
          pLongName = "Leeward D1"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eLwdRunner
          pIsMath = True
          pLongName = "Leeward Runner"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eLwdJibSheet
          pIsMath = True
          pLongName = "Leeward Jib Sheet"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eLwdJibUpDn
          pIsMath = True
          pLongName = "Leeward Jib Up Dn"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eWwdV1
          pIsMath = True
          pLongName = "Windward V1"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eWwdD1
          pIsMath = True
          pLongName = "Windward D1"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eWwdRunner
          pIsMath = True
          pLongName = "Windward Runner"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eWwdJibSheet
          pIsMath = True
          pLongName = "Windward Jib Sheet"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eWwdJibUpDn
          pIsMath = True
          pLongName = "Windward Jib Up Dn"
          pShortUM = "T"
          pLongUM = "Tons"
        Case eCanaliChiave.eSpannerCounter
          pKnownHeaders = New List(Of String)(New String() {"Spanner_Ang"})
        Case eCanaliChiave.eTopCtrlArmCounter
          pKnownHeaders = New List(Of String)(New String() {"TopCtrlArm_Ang"})
        Case eCanaliChiave.eTravellerCounter
          pKnownHeaders = New List(Of String)(New String() {"Traveller_Ang"})
        Case eCanaliChiave.eSpanner
          pIsMath = True
          pPolarHeader = "Spanner"
        Case eCanaliChiave.eTopCtrlArm
          pIsMath = True
        Case eCanaliChiave.eTraveller
          pIsMath = True
        Case eCanaliChiave.eTopTwist
          pIsMath = True
          pPolarHeader = "Twist"
        Case eCanaliChiave.eRoll
          pKnownHeaders = New List(Of String)(New String() {"Roll"})
      End Select

      Dim CanaleTmp As New clsChannel2020(pChannelId, pCanaleChiave, pShortName, pLongName, pShortUM, pLongUM, pDataType, pActualLogHeader, pKnownHeaders, pPolarHeader, pIsMath, pDecimals)
      pListaCanaliDefault.Add(CanaleTmp)
    Next


  End Sub

  Private Sub CaricaCanaliConosciuti(FileType As clsDataProvider2020.eFileType)
    AppConfig.CercaNodo(ParentDataProvider.FileType)

  End Sub

  Private Sub MappaCanali(Intestazioni As List(Of String))
    'cerca l 'intestazione nei canali di default
    'se non trova l 'intestazione carica dei valori dei default
    ' cerca l 'intestazione nei canali conosciuti
    'se non la trova lascia le impostazioni di default e carica il canale

  End Sub


End Class

Public Class clsChannel2020
  Dim pChannelId As String
  Dim pCanaleChiave As clsChannels2020.eCanaliChiave
  Dim pShortName As String
  Dim pLongName As String
  Dim pShortUM As String
  Dim pLongUM As String
  Dim pDataType As eDataType
  Dim pActualLogHeader As String
  Dim pKnownHeaders As List(Of String)
  Dim pPolarHeader As String
  Dim pIsMath As Boolean
  Dim pImporta As Boolean
  Dim pPrintedColor As Color
  Dim pDecimals As Integer
  Dim pIsTemporarySelected As Boolean
  Dim pValoriDT As DateTime()
  Dim pValori As Double()

  Public Sub New(ChannelId As String, CanaleChiave As clsChannels2020.eCanaliChiave, ShortName As String, LongName As String,
      ShortUM As String, LongUM As String, DataType As eDataType, ActualLogHeader As String, KnownHeaders As List(Of String),
      PolarHeader As String, IsMath As Boolean, Decimals As Integer)

    pChannelId = ChannelId
    pCanaleChiave = CanaleChiave
    pShortName = ShortName
    pLongName = LongName
    pShortUM = ShortUM
    pLongUM = LongUM
    pDataType = DataType
    pActualLogHeader = ActualLogHeader
    pKnownHeaders = KnownHeaders
    pPolarHeader = PolarHeader
    pIsMath = IsMath
    pDecimals = Decimals
    pImporta = False
    pIsTemporarySelected = False

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
  End Enum


  Public Property ValoriDT As Date()
    Get
      Return pValoriDT
    End Get
    Set(value As Date())
      pValoriDT = value
    End Set
  End Property

  Public Property Valori As Double()
    Get
      Return pValori
    End Get
    Set(value As Double())
      pValori = value
    End Set
  End Property

  Public Property DataType As eDataType
    Get
      Return pDataType
    End Get
    Set(value As eDataType)
      pDataType = value
    End Set
  End Property

End Class


Public Class clsTimeRange2020
  Dim pInizio As DateTime
  Dim pFine As DateTime

  Public Property Inizio As Date
    Get
      Return pInizio
    End Get
    Set(value As Date)
      pInizio = value
    End Set
  End Property

  Public Property Fine As Date
    Get
      Return pFine
    End Get
    Set(value As Date)
      pFine = value
    End Set
  End Property




End Class
