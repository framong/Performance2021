
'Public Class clsTabellePolari
'  Dim pTabelle As New List(Of clsTabella)
'  Dim pTipo As clsTabella.eTipoPolare = clsTabella.eTipoPolare.eNonRiconosciuto
'  Dim pRigaTgtUp As clsRighe
'  Dim pRigaTgtDn As clsRighe
'  Dim pDefSplineType As clsRighe.eSplineType = clsRighe.eSplineType.eCubic

'  Public Property Tabelle As List(Of clsTabella)
'    Get
'      Return pTabelle
'    End Get
'    Set(value As List(Of clsTabella))
'      pTabelle = value
'    End Set
'  End Property

'  Public Property Tipo As clsTabella.eTipoPolare
'    Get
'      Return pTipo
'    End Get
'    Set(value As clsTabella.eTipoPolare)
'      pTipo = value
'    End Set
'  End Property

'  Public ReadOnly Property GetTabella(NomeCanale As String) As clsTabella
'    Get
'      For Each Tabella As clsTabella In pTabelle
'        If Tabella.NomeCanaleValori.ToLower = NomeCanale.ToLower Then
'          Return Tabella
'        End If
'      Next
'      Return Nothing
'    End Get
'  End Property

'  Public Property RigaTgtUp As clsRighe
'    Get
'      Return pRigaTgtUp
'    End Get
'    Set(value As clsRighe)
'      pRigaTgtUp = value
'    End Set
'  End Property

'  Public Property RigaTgtDn As clsRighe
'    Get
'      Return pRigaTgtDn
'    End Get
'    Set(value As clsRighe)
'      pRigaTgtDn = value
'    End Set
'  End Property

'  Public Function AggiungiDaFilePolareSP(PathFile As String) As Boolean
'    'qui quando gli viene passato il file polare XML giá creato
'    pTipo = clsTabella.eTipoPolare.ePolareSP
'    Dim PolareCompleta As New clsPolareCompleta
'    PolareCompleta.CaricaFilePolare(PathFile)

'    Dim Kpoppa As Double = 0.3
'    pTabelle.Clear()
'    Dim DictTgtUp As New Dictionary(Of Double, Double)
'    Dim DictTgtDn As New Dictionary(Of Double, Double)
'    For Each Polare In PolareCompleta.PolariCanali
'      Dim VerificaVmg As Boolean = Polare.NomeCanaleValore.ToLower = "bs"
'      Dim TabellaTmp As New clsTabella(PathFile, clsTabella.eTipoPolare.ePolareSP, PolareCompleta.NomeCanaleAscissa, PolareCompleta.NomeCanaleOrdinata, Polare.NomeCanaleValore, VerificaVmg, pDefSplineType)
'      Dim CoppieUp As New List(Of clsCoppiaIndiceValore)
'      Dim CoppieDn As New List(Of clsCoppiaIndiceValore)
'      For IndiceAscissa As Integer = 0 To PolareCompleta.AscissaOriginalValues.Count - 1
'        If PolareCompleta.OrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues.Count = Polare.OriginalValues(IndiceAscissa).ChannelOriginalValues.Count Then
'          Dim Coppie As New List(Of clsCoppiaIndiceValore)
'          Coppie.Add(New clsCoppiaIndiceValore(0, 0, False, False)) 'aggiunge la colonna 0,0
'          For IndiceOrdinata As Integer = 0 To PolareCompleta.OrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues.Count - 1
'            Coppie.Add(New clsCoppiaIndiceValore(PolareCompleta.OrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues(IndiceOrdinata), Polare.OriginalValues(IndiceAscissa).ChannelOriginalValues(IndiceOrdinata), PolareCompleta.IDcolonnaBestVmgUpwind = IndiceOrdinata, PolareCompleta.IDcolonnaBestVmgDownwind = IndiceOrdinata))
'          Next
'          If PolareCompleta.NomeCanaleOrdinata = "twa" Then
'            Dim UltimoValore As Double = Coppie.Last.Valore
'            Coppie.Add(New clsCoppiaIndiceValore(180, UltimoValore * Kpoppa, False, False)) 'aggiunge la colonna 180
'          End If
'          TabellaTmp.AggiungiRiga(PolareCompleta.AscissaOriginalValues(IndiceAscissa), Coppie)
'          If VerificaVmg Then
'            DictTgtUp.Add(PolareCompleta.AscissaOriginalValues(IndiceAscissa), TabellaTmp.Righe.Last.IndiceTargetUp)
'            DictTgtDn.Add(PolareCompleta.AscissaOriginalValues(IndiceAscissa), TabellaTmp.Righe.Last.IndiceTargetDn)
'            CoppieUp.Add(New clsCoppiaIndiceValore(PolareCompleta.AscissaOriginalValues(IndiceAscissa), TabellaTmp.Righe(IndiceAscissa).CoppieIndiceValore(TabellaTmp.Righe.Last.IndiceTargetUp).Indice, True, False))
'            CoppieDn.Add(New clsCoppiaIndiceValore(PolareCompleta.AscissaOriginalValues(IndiceAscissa), TabellaTmp.Righe(IndiceAscissa).CoppieIndiceValore(TabellaTmp.Righe.Last.IndiceTargetDn).Indice, False, True))
'            'Else
'            '  'mi sa che non serve piú....
'            '  TabellaTmp.Righe.Last.IndiceTargetUp = DictTgtUp(IndiceAscissa)
'            '  TabellaTmp.Righe.Last.IndiceTargetDn = DictTgtDn(IndiceAscissa)
'          End If
'        End If
'      Next
'      If VerificaVmg Then
'        RigaTgtUp = New clsRighe(-1, CoppieUp, False, pDefSplineType)
'        RigaTgtDn = New clsRighe(-1, CoppieDn, False, pDefSplineType)
'      End If
'      If Not TabellaTmp.Righe Is Nothing Then
'        TabellaTmp.ImpostaTargetUpDn(DictTgtUp, DictTgtDn)
'        pTabelle.Add(TabellaTmp)
'      End If
'    Next

'    'Dim TestTmp As String = ""
'    'For Each riga In pTabelle.First.Righe
'    '  TestTmp &= riga.ValoreRiga & vbTab
'    '  For Each coppia In riga.CoppieIndiceValore
'    '    TestTmp &= coppia.Indice & vbTab & coppia.Valore & vbTab
'    '  Next
'    '  TestTmp = TestTmp.TrimEnd(vbTab) & vbCrLf
'    'Next
'    'Clipboard.SetText(TestTmp)
'    Return True
'  End Function


'  Public Function AggiungiFile(PathFile As String, MultiSpline As Boolean) As Boolean


'    Dim Righe As List(Of String) = ObjFiles.TestoInLista(PathFile)
'    Dim Righe2 As List(Of String) = Nothing
'    Dim CampoAscissa As String = "tws" 'DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
'    Dim CampoOrdinata As String = "twa" 'DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
'    Dim CampoValori As String = "sow" 'DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'    Dim IsVMG As Boolean = True
'    If Righe(1).IndexOf("Performance Software Polar File") > -1 Then
'      Return AggiungiDaFilePolareSP(PathFile)
'    ElseIf Righe.First.StartsWith("!Expedition") Then
'      pTipo = clsTabella.eTipoPolare.eExpedition

'    ElseIf IsNumeric(Righe.First.Split(vbTab)(1)) Then
'      pTipo = clsTabella.eTipoPolare.eFaroRowTwsColsTwa
'      Dim FI As New System.IO.FileInfo(PathFile)
'      Dim Nome As String = FI.Name
'      If String.Compare(Nome.Substring(0, 3), "TWA", True) > -1 Then
'        Nome = FI.Name.Replace("TWA", "Bs")
'        Dim path2 As String = FI.Directory.FullName & "\" & Nome
'        If System.IO.File.Exists(path2) Then
'          Righe2 = Righe
'          Righe = ObjFiles.TestoInLista(path2)
'        Else
'          Return False
'        End If
'      ElseIf String.Compare(Nome.Substring(0, 2), "BS", True) > -1 Then
'        Nome = FI.Name.Replace("Bs", "TWA")
'        Dim path2 As String = FI.Directory.FullName & "\" & Nome
'        If System.IO.File.Exists(path2) Then
'          Righe2 = ObjFiles.TestoInLista(path2)
'        Else
'          Return False
'        End If
'      Else
'        Return False
'      End If
'      If Righe2 Is Nothing Then Return False

'    ElseIf Not IsNumeric(Righe.First.Split(vbTab)(1)) AndAlso Righe.First.Split(vbTab)(1).LastIndexOf("v0") > -1 Then
'      pTipo = clsTabella.eTipoPolare.eFaRoRowTwsColsBsTwa
'    Else
'      Return False
'    End If

'    Dim ArraySpline As New List(Of clsRighe.eSplineType)
'    ArraySpline.Add(clsRighe.eSplineType.eCubic)
'    If MultiSpline Then
'      ArraySpline.Add(clsRighe.eSplineType.eAkima)
'      ArraySpline.Add(clsRighe.eSplineType.eCatmullRom)
'      ArraySpline.Add(clsRighe.eSplineType.eLinear)
'      ArraySpline.Add(clsRighe.eSplineType.eMonotone)
'    End If
'    pTabelle.Clear()

'    Dim Kpoppa As Double = 0.5

'    For Each TipoSpline As clsRighe.eSplineType In ArraySpline
'      pTabelle.Add(New clsTabella(PathFile, pTipo, CampoAscissa, CampoOrdinata, CampoValori, IsVMG, TipoSpline))

'      Select Case pTipo
'        Case clsTabella.eTipoPolare.eExpedition
'          For R As Integer = 1 To Righe.Count - 1
'            Dim ArrayValori As String() = Righe(R).Split(vbTab)
'            Dim Coppie As New List(Of clsCoppiaIndiceValore)
'            For C As Integer = 1 To ArrayValori.Length - 1 Step 2
'              Coppie.Add(New clsCoppiaIndiceValore(ArrayValori(C), ArrayValori(C + 1), False, False))
'            Next
'            Dim ValoreRiga As Double = ArrayValori(0)
'            pTabelle.Last.AggiungiRiga(ValoreRiga, Coppie)
'          Next
'          pTabelle.Last.VerificaPrimaEdUltimaRiga(Kpoppa)
'        Case clsTabella.eTipoPolare.eFaRoRowTwsColsBsTwa
'          For R As Integer = 2 To Righe.Count - 1
'            Dim ArrayValori As String() = Righe(R).Split(vbTab)

'            Dim Coppie As New List(Of clsCoppiaIndiceValore)
'            For C As Integer = 1 To ArrayValori.Length - 1 Step 2
'              Coppie.Add(New clsCoppiaIndiceValore(ArrayValori(C), ArrayValori(C + 1), False, False))
'            Next
'            Dim ValoreRiga As Double = ArrayValori(0)
'            pTabelle.Last.AggiungiRiga(ValoreRiga, Coppie)
'          Next
'          pTabelle.Last.VerificaPrimaEdUltimaRiga(Kpoppa)
'        Case clsTabella.eTipoPolare.eFaroRowTwsColsTwa
'          Dim ArrayValoriRiga As String() = Righe.First.Split(vbTab)
'          For ii As Integer = 1 To Righe.First.Split(vbTab).Count - 1
'            'looppa tra le colonne
'            Dim Coppie As New List(Of clsCoppiaIndiceValore)
'            For i As Integer = 2 To Righe.Count - 1 Step 2
'              Dim ArrayValoriBS As String() = Righe(i).Split(vbTab)
'              Dim ArrayValoriTWA As String() = Righe2(i).Split(vbTab)
'              Coppie.Add(New clsCoppiaIndiceValore(ArrayValoriTWA(ii), ArrayValoriBS(ii), False, False))
'            Next
'            Dim ValoreRiga As Double = ArrayValoriRiga(ii)
'            pTabelle.Last.AggiungiRiga(ValoreRiga, Coppie)
'          Next
'          pTabelle.Last.VerificaPrimaEdUltimaRiga(Kpoppa)
'        Case Else
'      End Select
'    Next
'    Return True
'  End Function



'End Class

'Public Class clsTabella
'  Dim pPathFile As String
'  Dim pFI As System.IO.FileInfo
'  Dim pRighe As List(Of clsRighe)
'  Dim pNomeCanaleAscissa As String
'  Dim pNomeCanaleOrdinata As String
'  Dim pNomeCanaleValori As String
'  'Dim pCanaleAscissa As clsChannel2020
'  'Dim pCanaleOrdinata As clsChannel2020
'  'Dim pCanaleValori As clsChannel2020
'  Dim pTargetUpwind As clsRighe
'  Dim pTargetDownwind As clsRighe
'  Dim pCheckVMG As Boolean
'  Dim pTipoPolare As eTipoPolare
'  Dim pSplineType As clsRighe.eSplineType

'  Public ReadOnly Property SplineType As clsRighe.eSplineType
'    Get
'      Return pSplineType
'    End Get
'  End Property

'  Public Enum eTipoPolare
'    eNonRiconosciuto = 0
'    eFaroRowTwsColsTwa = 1
'    eFaRoRowTwsColsBsTwa = 2
'    eExpedition = 3
'    eVpp = 4
'    ePolareSP = 5
'  End Enum

'  Public ReadOnly Property PathFile As String
'    Get
'      Return pPathFile
'    End Get
'  End Property

'  Public ReadOnly Property TipoPolare As eTipoPolare
'    Get
'      Return pTipoPolare
'    End Get
'  End Property

'  Public ReadOnly Property FI As System.IO.FileInfo
'    Get
'      Return pFI
'    End Get
'  End Property

'  Public ReadOnly Property Righe As List(Of clsRighe)
'    Get
'      Return pRighe
'    End Get
'  End Property

'  Public ReadOnly Property CheckVMG As Boolean
'    Get
'      Return pCheckVMG
'    End Get
'  End Property

'  Public ReadOnly Property NomeCanaleAscissa As String
'    Get
'      Return pNomeCanaleAscissa
'    End Get
'  End Property

'  Public ReadOnly Property NomeCanaleOrdinata As String
'    Get
'      Return pNomeCanaleOrdinata
'    End Get
'  End Property

'  Public ReadOnly Property NomeCanaleValori As String
'    Get
'      Return pNomeCanaleValori
'    End Get
'  End Property

'  'Public ReadOnly Property CanaleAscissa As clsChannel2020
'  '	Get
'  '		Return pCanaleAscissa
'  '	End Get
'  'End Property

'  'Public ReadOnly Property CanaleOrdinata As clsChannel2020
'  '	Get
'  '		Return pCanaleOrdinata
'  '	End Get
'  'End Property

'  'Public ReadOnly Property CanaleValori As clsChannel2020
'  '	Get
'  '		Return pCanaleValori
'  '	End Get
'  'End Property

'  'Public Sub New(PathFile As String, TipoPolare As eTipoPolare, CanaleAscissa As clsChannel2020, CanaleOrdinata As clsChannel2020, CanaleValori As clsChannel2020, CheckVMG As Boolean, SplineType As clsRighe.eSplineType)
'  Public Sub New(PathFile As String, TipoPolare As eTipoPolare, NomeCanaleAscissa As String, NomeCanaleOrdinata As String, NomeCanaleValori As String, CheckVMG As Boolean, SplineType As clsRighe.eSplineType)
'    pPathFile = PathFile
'    pFI = New System.IO.FileInfo(pPathFile)
'    pTipoPolare = TipoPolare
'    pNomeCanaleAscissa = NomeCanaleAscissa
'    pNomeCanaleOrdinata = NomeCanaleOrdinata
'    pNomeCanaleValori = NomeCanaleValori
'    pCheckVMG = CheckVMG
'    pSplineType = SplineType
'  End Sub

'  'Public Sub AggiungiRiga(DescrizioneRiga As Double, Indici As List(Of Double), Valori As List(Of Double), IndicetargetUp As Integer, IndiceTargetDn As Integer)
'  Public Sub AggiungiRiga(DescrizioneRiga As Double, Coppie As List(Of clsCoppiaIndiceValore))
'    ' esempio con polare BS
'    'viene aggiunta una nuova riga per ogni TWS (DescrizioneRiga), ogni riga ha la lista degli Indici presa dalla TWA e quella dei valori dalla BS per il relativo TWA
'    'le due matrice Indici e Valori devono avere la stessa dimensione
'    'i due indici target indicano l'indice nella lista del valore da considerare target upwind e downwind
'    If pRighe Is Nothing Then pRighe = New List(Of clsRighe)
'    'pRighe.Add(New clsRighe(DescrizioneRiga, Indici, Valori, IndicetargetUp, IndiceTargetDn, pCheckVMG, pSplineType))
'    pRighe.Add(New clsRighe(DescrizioneRiga, Coppie, pCheckVMG, pSplineType))
'  End Sub

'  Public Sub VerificaPrimaEdUltimaRiga(Kpoppa As Double)
'    'Exit Sub
'    Dim BS As Double
'    Dim TWA As Double
'    Dim VMG As Double
'    Dim Ktwa As Double
'    Dim Kbs As Double
'    Dim NewTWA As Double
'    Dim NewBS As Double
'    Dim Esp As Double = 1.3
'    If pRighe.First.CoppieIndiceValore.First.Indice > 0 Then
'      ' il primo twa non è 0
'      Dim BestVMGidx As Integer = -1
'      Dim BestVMG As Double = 0
'      For i As Integer = 0 To pRighe.First.CoppieIndiceValore.Count - 1
'        Dim VMGtmp As Double = pRighe.First.CoppieIndiceValore(i).Valore * System.Math.Cos(Radians(pRighe.First.CoppieIndiceValore(i).Indice))
'        If VMGtmp > BestVMG Then
'          BestVMG = VMGtmp
'          BestVMGidx = i
'        End If
'      Next
'      'Se il primo twa è anche quello vmg upwind allora aggiunge due colonne oltre quella iniziale a 0twa
'      ' la prima al 90% della TWA con una BS corrispondente al 98% della VMG (target) a quell'angolo
'      ' l'altra ad 1/3 dei gradi di twa con BS corrispondente del 20% 
'      ' questo per dare morbidità alla salita della curva della BS
'      For Each Riga As clsRighe In pRighe
'        If BestVMGidx = 0 Then
'          BS = Riga.CoppieIndiceValore(0).Valore
'          TWA = Riga.CoppieIndiceValore(0).Indice
'          VMG = BS * System.Math.Cos(Radians(TWA))
'          Ktwa = 0.9
'          Kbs = System.Math.Sin(Radians(90 * Ktwa ^ Esp)) ' 0.98
'          NewTWA = CInt(TWA * Ktwa)
'          NewBS = VMG * Kbs / System.Math.Cos(Radians(NewTWA))
'          Riga.CoppieIndiceValore.Insert(0, New clsCoppiaIndiceValore(NewTWA, System.Math.Round(NewBS, 1), False, False))
'          Ktwa = 0.3
'          Kbs = System.Math.Sin(Radians(90 * Ktwa ^ Esp)) ' 0.2
'          NewTWA = CInt(TWA * Ktwa)
'          NewBS = VMG * Kbs / System.Math.Cos(Radians(NewTWA))
'          Riga.CoppieIndiceValore.Insert(0, New clsCoppiaIndiceValore(NewTWA, System.Math.Round(NewBS, 1), False, False))
'        End If
'        Riga.CoppieIndiceValore.Insert(0, New clsCoppiaIndiceValore(0, 0, False, False))
'        Riga.AggiornaInterpolatoreEtTarget()
'      Next

'    End If
'    If pRighe.Last.CoppieIndiceValore.Last.Indice < 180 Then

'      Dim BestVMGidx As Integer = -1
'      Dim BestVMG As Double = 0 ' in poppa valori negativi
'      For i As Integer = 0 To pRighe.First.CoppieIndiceValore.Count - 1
'        Dim VMGtmp As Double = pRighe.First.CoppieIndiceValore(i).Valore * System.Math.Cos(Radians(pRighe.First.CoppieIndiceValore(i).Indice))
'        If VMGtmp < BestVMG Then
'          BestVMG = VMGtmp
'          BestVMGidx = i
'        End If
'      Next
'      'Esp = 4
'      Dim DimensioneArrayAnte As Integer = pRighe.First.CoppieIndiceValore.Count - 1
'      For Each Riga As clsRighe In pRighe
'        Dim lastVMG As Double = Riga.CoppieIndiceValore.Last.Valore * System.Math.Abs(System.Math.Cos(Radians(Riga.CoppieIndiceValore.Last.Indice)))
'        Dim BSp As Double = lastVMG * Kpoppa
'        If BestVMGidx = DimensioneArrayAnte Then
'          BS = Riga.CoppieIndiceValore(DimensioneArrayAnte).Valore
'          TWA = Riga.CoppieIndiceValore(DimensioneArrayAnte).Indice
'          VMG = (BS - BSp) * System.Math.Cos(Radians(TWA))
'          Ktwa = 0.9
'          Kbs = System.Math.Sin(Radians(90 * Ktwa ^ Esp)) ' 0.98
'          NewTWA = 180 - CInt((180 - TWA) * Ktwa)
'          NewBS = BSp + VMG * Kbs / System.Math.Cos(Radians(NewTWA))
'          Riga.CoppieIndiceValore.Add(New clsCoppiaIndiceValore(NewTWA, System.Math.Round(NewBS, 1), False, False))
'          Ktwa = 0.3
'          Kbs = System.Math.Sin(Radians(90 * Ktwa ^ Esp)) ' 0.2
'          NewTWA = 180 - CInt((180 - TWA) * Ktwa)
'          NewBS = BSp + VMG * Kbs / System.Math.Cos(Radians(NewTWA))
'          Riga.CoppieIndiceValore.Add(New clsCoppiaIndiceValore(NewTWA, System.Math.Round(NewBS, 1), False, False))
'        End If
'        Riga.CoppieIndiceValore.Add(New clsCoppiaIndiceValore(180, System.Math.Round(BSp, 1), False, False))
'        Riga.AggiornaInterpolatoreEtTarget()
'      Next
'    End If

'  End Sub


'  ' le righe contengono per ciascun valore di canale principale (TWS esempio classico) 
'  ' le coppie canale secondario (TWA in genere) con valore canale target (esempio BS)

'  Public Sub ImpostaTargetUpDn(IndiciTargetUp As Dictionary(Of Double, Double), IndiciTargetDn As Dictionary(Of Double, Double))
'    'IndiciTargetUp (TWS, IndiceColonna target) deve avere la stessa dimensione della lista pRighe
'    'crea una riga con coppie TWS valore corrispondente a quella colonna
'    Dim CoppieUp As New List(Of clsCoppiaIndiceValore)
'    Dim CoppieDn As New List(Of clsCoppiaIndiceValore)
'    For IndiceRiga As Integer = 0 To pRighe.Count - 1
'      CoppieUp.Add(New clsCoppiaIndiceValore(IndiciTargetUp.Keys(IndiceRiga), pRighe(IndiceRiga).Valore(IndiciTargetUp.Valori(IndiceRiga)), False, False))
'      CoppieDn.Add(New clsCoppiaIndiceValore(IndiciTargetDn.Keys(IndiceRiga), pRighe(IndiceRiga).Valore(IndiciTargetDn.Valori(IndiceRiga)), False, False))
'    Next
'    pTargetUpwind = New clsRighe(-1, CoppieUp, pCheckVMG, pSplineType)
'    pTargetDownwind = New clsRighe(-1, CoppieDn, pCheckVMG, pSplineType)
'  End Sub


'  Public Function ValoreTarget(ValoreRiga As Double, Upwind As Boolean) As Double
'    Dim ColonnaTMP As clsRighe
'    If Upwind Then
'      ColonnaTMP = pTargetUpwind
'    Else
'      ColonnaTMP = pTargetDownwind
'    End If
'    If ColonnaTMP Is Nothing Then
'      Stop
'      'Dim Coppie As New List(Of clsCoppiaIndiceValore)
'      'Dim IndiceRiga As Integer = 0
'      'For Each rigaTMP As clsRighe In pRighe
'      '  'PolareCompleta.IDcolonnaBestVmgUpwind = IndiceOrdinata, PolareCompleta.IDcolonnaBestVmgDownwind = IndiceOrdinata
'      '  Coppie.Add(New clsCoppiaIndiceValore(rigaTMP.ValoreIndiceTarget(Upwind), rigaTMP.ValoreTarget(Upwind), pRighe(IndiceRiga).IndiceTargetUp = IndiceRiga, pRighe(IndiceRiga).IndiceTargetDn = IndiceRiga))
'      '  IndiceRiga += 1
'      'Next
'      '' é un oggetto riga che include tutti i valori target ovvero nel caso classico le coppie TWA BS per tutte le intensitá di vento
'      'ColonnaTMP = New clsRighe(-1, Coppie, pCheckVMG, pSplineType)
'    End If
'    'da quella colonna restituisce il valore interpolato per il ValoreRiga ovvero lo specifico TWS
'    Return ColonnaTMP.Valore(ValoreRiga)
'  End Function

'  'Public Function ValoreIndiceTarget(ValoreRiga As Double, Upwind As Boolean) As Double
'  '	Dim ColonnaTMP As clsRighe
'  '	If Upwind Then
'  '		ColonnaTMP = pTargetUpwind
'  '	Else
'  '		ColonnaTMP = pTargetDownwind
'  '	End If
'  '	If ColonnaTMP Is Nothing Then
'  '		Dim Coppie As New List(Of clsCoppiaIndiceValore)
'  '		For Each rigaTMP As clsRighe In pRighe
'  '			Coppie.Add(New clsCoppiaIndiceValore(rigaTMP.ValoreIndiceTarget(Upwind), rigaTMP.ValoreTarget(Upwind), False, False))
'  '		Next
'  '		' é un oggetto riga che include tutti i valori target ovvero nel caso classico le coppie TWA BS per tutte le intensitá di vento
'  '		ColonnaTMP = New clsRighe(-1, Coppie, pCheckVMG, pSplineType)
'  '	End If
'  '	'da quella colonna restituisce il valore interpolato per il ValoreRiga ovvero lo specifico TWS
'  '	Return ColonnaTMP.ValoreIndiceTarget(ValoreRiga)
'  'End Function

'  Public Function Valore(ValoreRiga As Double, Indice As Double) As Double
'    Dim Coppie As New List(Of clsCoppiaIndiceValore)
'    For Each rigaTMP As clsRighe In pRighe
'      Coppie.Add(New clsCoppiaIndiceValore(rigaTMP.ValoreRiga, rigaTMP.Valore(Indice), False, False))
'    Next
'    ' é un oggetto riga che include tutti i valori target ovvero nel caso classico le coppie TWA BS per tutte le intensitá di vento
'    Dim ColonnaTMP As New clsRighe(-1, Coppie, False, pSplineType)

'    Return ColonnaTMP.Valore(ValoreRiga)
'  End Function

'End Class

'Public Class clsRighe
'  Dim pValoreRiga As Double
'  Dim pCoppieIndiceValore As List(Of clsCoppiaIndiceValore)
'  Dim pIndiceTargetUp As Integer
'  Dim pIndiceTargetDn As Integer
'  Dim pCheckVMG As Boolean 'proprietá con Valore True per polari BS in cui gli indici di colonna indicati come target sono quelli che producono il miglior VMG
'  Dim pObjInterpolatore As New alglib.spline1dinterpolant 'richiede Alglib2 www.alglib.net
'  Dim pSplineType As eSplineType

'  Public Enum eSplineType
'    eLinear = 0
'    eCubic = 1
'    'eHermite = 2
'    eAkima = 3
'    eCatmullRom = 4
'    eMonotone = 5
'  End Enum

'  Public ReadOnly Property SplineType As eSplineType
'    Get
'      Return pSplineType
'    End Get
'  End Property

'  Public ReadOnly Property ValoreRiga As Double
'    Get
'      Return pValoreRiga
'    End Get
'  End Property

'  Public Property IndiceTargetUp As Integer
'    Get
'      Return pIndiceTargetUp
'    End Get
'    Set(value As Integer)
'      pIndiceTargetUp = value
'    End Set
'  End Property

'  Public Property IndiceTargetDn As Integer
'    Get
'      Return pIndiceTargetDn
'    End Get
'    Set(value As Integer)
'      pIndiceTargetDn = value
'    End Set
'  End Property

'  Public Property CoppieIndiceValore As List(Of clsCoppiaIndiceValore)
'    Get
'      Return pCoppieIndiceValore
'    End Get
'    Set(value As List(Of clsCoppiaIndiceValore))
'      pCoppieIndiceValore = value
'    End Set
'  End Property

'  Public Sub New(ValoreRiga As Double, CoppieIndiceValore As List(Of clsCoppiaIndiceValore), CheckVMG As Boolean, SplineType As eSplineType)
'    pValoreRiga = ValoreRiga
'    pCheckVMG = CheckVMG
'    pSplineType = SplineType
'    ' nel caso della TWA come indice mette in ordine di TWA crescente
'    pIndiceTargetUp = -1
'    pIndiceTargetDn = -1
'    pCoppieIndiceValore = CoppieIndiceValore.OrderBy(Function(x) x.Indice).ToList
'    For i As Integer = 0 To pCoppieIndiceValore.Count - 1
'      If pCoppieIndiceValore(i).IsUpTg Then pIndiceTargetUp = i
'      If pCoppieIndiceValore(i).IsDnTg Then pIndiceTargetDn = i
'    Next
'    AggiornaInterpolatore()
'  End Sub

'  Public Sub AggiornaInterpolatoreEtTarget()
'    pIndiceTargetUp = -1
'    pIndiceTargetDn = -1
'    AggiornaInterpolatore()
'  End Sub

'  Public Sub AggiornaInterpolatore()
'    If pCheckVMG Then
'      Dim BestVMG As Double = 0
'      If pIndiceTargetDn = -1 Then
'        For i As Integer = 0 To pCoppieIndiceValore.Count - 1
'          If pCoppieIndiceValore(i).Indice > 90 Then
'            Dim VMG As Double = System.Math.Abs(pCoppieIndiceValore(i).Valore * System.Math.Cos(Radians(pCoppieIndiceValore(i).Indice)))
'            If VMG > BestVMG Then
'              pIndiceTargetDn = i
'              BestVMG = VMG
'            End If
'          End If
'        Next
'      End If
'      If pIndiceTargetUp = -1 Then
'        BestVMG = 0
'        For i As Integer = 0 To pCoppieIndiceValore.Count - 1
'          If pCoppieIndiceValore(i).Indice < 90 Then ' TWA < 90
'            Dim VMG As Double = System.Math.Abs(pCoppieIndiceValore(i).Valore * System.Math.Cos(Radians(pCoppieIndiceValore(i).Indice)))
'            If VMG > BestVMG Then
'              pIndiceTargetUp = i
'              BestVMG = VMG
'            End If
'          End If
'        Next
'      End If
'    End If

'    Dim pIndici As New List(Of Double)
'    Dim pValori As New List(Of Double)
'    For Each Coppia In pCoppieIndiceValore
'      pIndici.Add(Coppia.Indice)
'      pValori.Add(Coppia.Valore)
'    Next

'    Select Case pSplineType
'      Case eSplineType.eAkima
'        alglib.spline1dbuildakima(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eCatmullRom
'        alglib.spline1dbuildcatmullrom(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eCubic
'        alglib.spline1dbuildcubic(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'            'Case eSplineType.eHermite
'            '    alglib.spline1dbuildhermite(Indici.ToArray, Valori.ToArray, pObjInterpolatore)
'      Case eSplineType.eLinear
'        alglib.spline1dbuildlinear(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'      Case eSplineType.eMonotone
'        alglib.spline1dbuildmonotone(pIndici.ToArray, pValori.ToArray, pObjInterpolatore)
'    End Select
'  End Sub


'  Public Function ValoreTarget(Upwind As Boolean) As Double
'    If Upwind Then
'      Return ValoreDaIndice(pIndiceTargetUp)
'    Else
'      Return ValoreDaIndice(pIndiceTargetDn)
'    End If
'  End Function

'  Public Function ValoreIndiceTarget(Upwind As Boolean) As Double
'    If Upwind Then
'      Return pCoppieIndiceValore(pIndiceTargetUp).Indice
'    Else
'      Return pCoppieIndiceValore(pIndiceTargetDn).Indice
'    End If
'  End Function

'  Public Function Valore(Indice As Double) As Double
'    'indice nel caso di BS é il TWA
'    Return ValoreDaIndice(Indice)
'  End Function

'  Private Function ValoreDaIndice(Indice As Double) As Double
'    Dim Valore As Double = ValoreInterpolato(Indice)

'    If pCheckVMG Then
'      'applica il controllo di VMG
'      'ovvero restituisce al massimo una BS tale da eguagliare il VMG target
'      Dim TwaTg As Double = ValoreIndiceTarget(Indice < 90)
'      Dim BsTg As Double = ValoreInterpolato(TwaTg)
'      Dim VMGmax As Double = System.Math.Abs(BsTg * System.Math.Cos(Radians(TwaTg)))
'      Valore = System.Math.Min(Valore, VMGmax / System.Math.Abs(System.Math.Cos(Radians(Indice))))
'      If Valore < 0 Then
'        Valore = 0
'      End If
'    End If
'    Return Valore
'  End Function

'  Private Function ValoreInterpolato(Indice As Double) As Double
'    'legge il valore dall'interpolatore
'    Return alglib.spline1dcalc(pObjInterpolatore, Indice)
'  End Function


'End Class

'Public Class clsCoppiaIndiceValore
'  Dim pIndice As Double
'  Dim pValore As Double
'  Dim pIsUpTg As Boolean
'  Dim pIsDnTg As Boolean

'  Public Sub New(Indice As Double, Valore As Double, IsUpTg As Boolean, IsDnTg As Boolean)
'    pIndice = Indice
'    pValore = Valore
'    pIsUpTg = IsUpTg
'    pIsDnTg = IsDnTg
'  End Sub

'  Public Property Indice As Double
'    Get
'      Return pIndice
'    End Get
'    Set(value As Double)
'      pIndice = value
'    End Set
'  End Property

'  Public Property Valore As Double
'    Get
'      Return pValore
'    End Get
'    Set(value As Double)
'      pValore = value
'    End Set
'  End Property

'  Public Property IsUpTg As Boolean
'    Get
'      Return pIsUpTg
'    End Get
'    Set(value As Boolean)
'      pIsUpTg = value
'    End Set
'  End Property

'  Public Property IsDnTg As Boolean
'    Get
'      Return pIsDnTg
'    End Get
'    Set(value As Boolean)
'      pIsDnTg = value
'    End Set
'  End Property
'End Class

'Public Class clsPolareCompleta
'  'Dim pCanaleAscissa As clsChannel2020 'normalmente TWS
'  'Dim pCanaleOrdinata As clsChannel2020 'normalmente TWA
'  Dim pNomeCanaleAscissa As String 'normalmente TWS
'  Dim pNomeCanaleOrdinata As String 'normalmente TWA
'  Dim pPolariCanali As New List(Of clsPolareCanale) ' BS il grande classico, poi via via le altre
'  'Dim pBestUpwind As New List(Of Integer) 'lista (per riga di ascissa) dei valori di ordinata cui corrisponde il best value upwind
'  'Dim pBestDownwind As New List(Of Integer) 'lista (per riga di ascissa) dei valori di ordinata cui corrisponde il best value upwind
'  Dim pNomePolare As String
'  Dim pFilePolare As System.IO.FileInfo
'  Dim pFileXML As clsSettings
'  Dim pAscissaOriginalValues As New List(Of Double)
'  Dim pOrdinataOriginalValues As New List(Of clsOriginalValues)
'  Dim pIDcolonnaBestVmgUpwind As Integer = 0
'  Dim pIDcolonnaBestVmgDownwind As Integer = 0

'  Dim pFileSource As String
'  Dim pDescription As String

'  Public Property PolariCanali As List(Of clsPolareCanale)
'    Get
'      Return pPolariCanali
'    End Get
'    Set(value As List(Of clsPolareCanale))
'      pPolariCanali = value
'    End Set
'  End Property

'  Public Property NomeCanaleOrdinata As String
'    Get
'      Return pNomeCanaleOrdinata
'    End Get
'    Set(value As String)
'      pNomeCanaleOrdinata = value
'    End Set
'  End Property

'  Public Property NomeCanaleAscissa As String
'    Get
'      Return pNomeCanaleAscissa
'    End Get
'    Set(value As String)
'      pNomeCanaleAscissa = value
'    End Set
'  End Property

'  'Public Property CanaleOrdinata As clsChannel2020
'  '	Get
'  '		Return pCanaleOrdinata
'  '	End Get
'  '	Set(value As clsChannel2020)
'  '		pCanaleOrdinata = value
'  '	End Set
'  'End Property

'  'Public Property CanaleAscissa As clsChannel2020
'  '	Get
'  '		Return pCanaleAscissa
'  '	End Get
'  '	Set(value As clsChannel2020)
'  '		pCanaleAscissa = value
'  '	End Set
'  'End Property

'  Public Property NomePolare As String
'    Get
'      Return pNomePolare
'    End Get
'    Set(value As String)
'      pNomePolare = value
'    End Set
'  End Property

'  Public Property FilePolare As FileInfo
'    Get
'      Return pFilePolare
'    End Get
'    Set(value As FileInfo)
'      pFilePolare = value
'    End Set
'  End Property

'  Public Property AscissaOriginalValues As List(Of Double)
'    Get
'      Return pAscissaOriginalValues
'    End Get
'    Set(value As List(Of Double))
'      pAscissaOriginalValues = value
'    End Set
'  End Property

'  Public Property OrdinataOriginalValues As List(Of clsOriginalValues)
'    Get
'      Return pOrdinataOriginalValues
'    End Get
'    Set(value As List(Of clsOriginalValues))
'      pOrdinataOriginalValues = value
'    End Set
'  End Property

'  Public Property IDcolonnaBestVmgUpwind As Integer
'    Get
'      Return pIDcolonnaBestVmgUpwind
'    End Get
'    Set(value As Integer)
'      pIDcolonnaBestVmgUpwind = value
'    End Set
'  End Property

'  Public Property IDcolonnaBestVmgDownwind As Integer
'    Get
'      Return pIDcolonnaBestVmgDownwind
'    End Get
'    Set(value As Integer)
'      pIDcolonnaBestVmgDownwind = value
'    End Set
'  End Property

'  Public Property FileSource As String
'    Get
'      Return pFileSource
'    End Get
'    Set(value As String)
'      pFileSource = value
'    End Set
'  End Property

'  Public Property Description As String
'    Get
'      Return pDescription
'    End Get
'    Set(value As String)
'      pDescription = value
'    End Set
'  End Property

'  Public Sub New()

'  End Sub

'  Public Sub CaricaUltimoUtilizzato()
'    Dim PathFiledaCaricare As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "TargetFiles", "LastLoaded", "", True, True)
'    ImpostazioniIniziali(PathFiledaCaricare)
'  End Sub

'  Public Sub CaricaFilePolare(PathFiledaCaricare As String)
'    ImpostazioniIniziali(PathFiledaCaricare)
'  End Sub

'  Public Sub ImportaVppExcel()
'    Dim objXls As New clsExcelVpp
'    pFilePolare = objXls.FilePolare
'  End Sub

'  Private Sub ImpostazioniIniziali(PathFiledaCaricare As String)
'    If System.IO.File.Exists(PathFiledaCaricare) Then
'      pFilePolare = New System.IO.FileInfo(PathFiledaCaricare)
'      CaricaPolareXML()
'    Else
'      CreaFilePolareTest()
'    End If
'  End Sub


'  Private Sub CreaFilePolareTest()
'    pNomePolare = "PolarTest"
'    creaPathCompleto(pNomePolare, Nothing)
'    CreaPolareTest()
'  End Sub

'  Private Sub creaPathCompleto(NomeFile As String, FolderPath As String)
'    Dim dirPath As String = AppConfig.ApplicationDataFolder
'    Dim sb As New System.Text.StringBuilder
'    Dim cSep As String = "\"
'    If FolderPath Is Nothing Then
'      sb.Append(dirPath)
'      If dirPath.IndexOf("/") > -1 Then
'        cSep = "/"
'      End If
'      sb.Append(cSep)
'      sb.Append("Polars")
'    Else
'      sb.Append(FolderPath)
'      If dirPath.IndexOf("/") > -1 Then
'        cSep = "/"
'      End If
'      sb.Append(cSep)
'    End If

'    If Not IO.Directory.Exists(sb.ToString) Then
'      Try
'        IO.Directory.CreateDirectory(sb.ToString)
'      Catch ex As Exception
'        MsgBox("The Folder you're running the application hasn't writing permissions for the current user, please change user or folder", MsgBoxStyle.Critical)
'        End
'      End Try
'    End If

'    sb.Append(cSep)
'    sb.Append(NomeFile)
'    sb.Append(".xml")
'    pFilePolare = New System.IO.FileInfo(sb.ToString)
'  End Sub



'  Public Sub SalvaSuXml(PathFile As String)
'    If System.IO.File.Exists(pFilePolare.FullName) Then
'      System.IO.File.Delete(pFilePolare.FullName)
'    End If
'    If pFileXML Is Nothing Then
'      pFileXML = New clsSettings(PathFile, "LR2021", "Performance Software Polar File")
'    Else
'      pFileXML.EliminaNodo("PerformanceTable", clsSettings.eNodoSTD.eChannels, "", True)
'    End If
'    For IndiceAscissa As Integer = 0 To pAscissaOriginalValues.Count - 1
'      Dim stringaAscissa As String = pNomeCanaleAscissa & "_" & pAscissaOriginalValues(IndiceAscissa).ToString.Replace(".", "d")
'      For indiceOrdinata As Integer = 0 To pOrdinataOriginalValues.First.ChannelOriginalValues.Count - 1
'        Dim stringaOrdinata As String = pNomeCanaleOrdinata & "_" & pOrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata).ToString.Replace(".", "d")
'        For IndicePolare As Integer = 0 To pPolariCanali.Count - 1
'          pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, pPolariCanali(IndicePolare).NomeCanaleValore, pPolariCanali(IndicePolare).OriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata), True, False)
'        Next
'      Next
'    Next
'    'For IndiceAscissa As Integer = 0 To pAscissaOriginalValues.Count - 1
'    '	Dim stringaAscissa As String = pCanaleAscissa.PolarHeader & "_" & pAscissaOriginalValues(IndiceAscissa).ToString.Replace(".", "d")
'    '	For indiceOrdinata As Integer = 0 To pOrdinataOriginalValues.First.ChannelOriginalValues.Count - 1
'    '		Dim stringaOrdinata As String = pCanaleOrdinata.PolarHeader & "_" & pOrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata).ToString.Replace(".", "d")
'    '		For IndicePolare As Integer = 0 To pPolariCanali.Count - 1
'    '			pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaAscissa, stringaOrdinata, pPolariCanali(IndicePolare).CanaleValore.PolarHeader, pPolariCanali(IndicePolare).OriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata), True, False)
'    '		Next
'    '	Next
'    'Next
'    pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Upwind", pIDcolonnaBestVmgUpwind, True, False)
'    pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Downwind", pIDcolonnaBestVmgDownwind, True, False)

'    pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Annotations", "description", pDescription, True, False)
'    pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Annotations", "filesource", pFileSource, True, False)
'    pFileXML.SalvaFileXML()

'    AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "TargetFiles", "LastLoaded", pFilePolare.FullName, True, True)
'  End Sub

'  'Public Sub SalvaSuXmlOld(PathFile As String)
'  '  If System.IO.File.Exists(pFilePolare.FullName) Then
'  '    System.IO.File.Delete(pFilePolare.FullName)
'  '  End If
'  '  If pFileXML Is Nothing Then
'  '    pFileXML = New clsSettings(PathFile, "LR2021", "Performance Software Polar File")
'  '  Else
'  '    pFileXML.EliminaNodo("PerformanceTable", clsSettings.eNodoSTD.eChannels, "", True)
'  '  End If
'  '  'Dim NomeFile As String = ObjFiles.PulisciCaratteriBastardi(pNomePolare)
'  '  For IndiceAscissa As Integer = 0 To pAscissaOriginalValues.Count - 1
'  '    For indiceOrdinata As Integer = 0 To pOrdinataOriginalValues.First.ChannelOriginalValues.Count - 1
'  '      Dim stringaBase As String = pCanaleAscissa.PolarHeader & "_" & IndiceAscissa.ToString.PadLeft(2, "0")
'  '      stringaBase &= "_" & pCanaleOrdinata.PolarHeader & "_" & indiceOrdinata.ToString.PadLeft(2, "0")
'  '      pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaBase, pCanaleAscissa.PolarHeader, pAscissaOriginalValues(IndiceAscissa), True, False)
'  '      pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaBase, pCanaleOrdinata.PolarHeader, pOrdinataOriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata), True, False)
'  '      For IndicePolare As Integer = 0 To pPolariCanali.Count - 1
'  '        pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "Table", stringaBase, pPolariCanali(IndicePolare).CanaleValore.PolarHeader, pPolariCanali(IndicePolare).OriginalValues(IndiceAscissa).ChannelOriginalValues(indiceOrdinata), True, False)
'  '      Next
'  '    Next
'  '  Next
'  '  pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Upwind", IDcolonnaBestVmgUpwind, True, False)
'  '  pFileXML.SalvaValoreInnerText("PerformanceTable", clsSettings.eNodoSTD.eChannels, "TargetColumns", "Downwind", IDcolonnaBestVmgDownwind, True, False)
'  '  pFileXML.SalvaFileXML()
'  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "TargetFiles", "LastLoaded", pFilePolare.FullName, True, True)
'  'End Sub


'  Public Sub CaricaPolareXML()
'    pNomeCanaleAscissa = ""
'    pNomeCanaleOrdinata = "" ' Nothing
'    pAscissaOriginalValues.Clear()
'    pOrdinataOriginalValues.Clear()
'    pPolariCanali.Clear()
'    pFileXML = New clsSettings(pFilePolare.FullName, "LR2021", "Performance Software Polar File")
'    Dim NodoPolare As Xml.XmlNode = pFileXML.CercaNodo("PerformanceTable", clsSettings.eNodoSTD.eChannels, True)
'    If Not NodoPolare Is Nothing Then
'      For Each Nodo As Xml.XmlNode In NodoPolare
'        Select Case Nodo.Name
'          Case "Table"
'            For Each NodoAscissa As Xml.XmlNode In Nodo
'              If pNomeCanaleAscissa = "" Then ' Is Nothing Then
'                pNomeCanaleAscissa = NodoAscissa.Name.Split("_")(0).ToLower ' dataProvider2020.Channels.TrovaCanale(NodoAscissa.Name.Split("_")(0))
'              End If
'              pAscissaOriginalValues.Add(CDbl(NodoAscissa.Name.Split("_")(1).Replace("d", ".")))

'              pOrdinataOriginalValues.Add(New clsOriginalValues(Nothing))
'              If NodoAscissa.HasChildNodes Then
'                For Each NodoOrdinata As Xml.XmlNode In NodoAscissa.ChildNodes
'                  If pNomeCanaleOrdinata = "" Then ' Is Nothing Then
'                    pNomeCanaleOrdinata = NodoOrdinata.Name.Split("_")(0).ToLower ' dataProvider2020.Channels.TrovaCanale(NodoOrdinata.Name.Split("_")(0))
'                  End If
'                  pOrdinataOriginalValues.Last.ChannelOriginalValues.Add(CDbl(NodoOrdinata.Name.Split("_")(1).Replace("d", ".")))
'                  If NodoOrdinata.HasChildNodes Then
'                    If NodoOrdinata Is NodoAscissa.FirstChild Then
'                      If NodoAscissa Is Nodo.FirstChild Then
'                        For Each Canale As Xml.XmlNode In NodoOrdinata.ChildNodes
'                          'Dim CanaleTmp As clsChannel2020 = dataProvider2020.Channels.TrovaCanale(Canale.Name)
'                          Dim CanaleTmp As String = Canale.Name
'                          pPolariCanali.Add(New clsPolareCanale(CanaleTmp))
'                        Next
'                      End If
'                      For i As Integer = 0 To NodoOrdinata.ChildNodes.Count - 1
'                        pPolariCanali(i).OriginalValues.Add(New clsOriginalValues(Nothing))
'                      Next
'                    End If
'                    For i As Integer = 0 To NodoOrdinata.ChildNodes.Count - 1
'                      pPolariCanali(i).OriginalValues.Last.ChannelOriginalValues.Add(NodoOrdinata.ChildNodes(i).InnerText)
'                    Next
'                  End If
'                Next
'              End If
'            Next
'          Case "TargetColumns"
'            For Each Colonna As Xml.XmlNode In Nodo
'              Select Case Colonna.Name.ToLower
'                Case "upwind"
'                  pIDcolonnaBestVmgUpwind = Colonna.InnerText
'                Case "downwind"
'                  pIDcolonnaBestVmgDownwind = Colonna.InnerText
'              End Select
'            Next
'          Case "Annotations"
'            For Each Sottononno As Xml.XmlNode In Nodo
'              Select Case Sottononno.Name.ToLower
'                Case "description"
'                  pDescription = Sottononno.InnerText
'                Case "filesource"
'                  pFileSource = Sottononno.InnerText
'              End Select
'            Next
'        End Select
'      Next
'    End If






'  End Sub

'  Public Function CartellaPolari() As String
'    Dim dirPath As String = AppConfig.ApplicationDataFolder
'    Dim sb As New System.Text.StringBuilder
'    Dim cSep As String = "\"
'    sb.Append(dirPath)
'    If dirPath.IndexOf("/") > -1 Then
'      cSep = "/"
'    End If
'    sb.Append(cSep)
'    sb.Append("Polars")
'    Return sb.ToString
'  End Function

'  Private Sub CreaPolareTest()

'    pPolariCanali.Clear()

'    'pCanaleAscissa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
'    'pCanaleOrdinata = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)

'    'CreaPolareCanale(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW))
'    'CreaPolareCanale(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHEEL))
'    'CreaPolareCanale(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTRIM))
'    'CreaPolareCanale(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRideHeight))

'    pNomeCanaleAscissa = "tws"
'    pNomeCanaleOrdinata = "twa"

'    CreaPolareCanale("sow")
'    CreaPolareCanale("heel")
'    CreaPolareCanale("trim")
'    CreaPolareCanale("rideheight")

'    SalvaSuXml(pFilePolare.FullName)

'  End Sub


'  'Private Sub CreaPolareCanale(Canale As clsChannel2020)
'  Private Sub CreaPolareCanale(Canale As String)
'    Dim PC As New clsPolareCanale(Canale)
'    Dim x, y, z As Double
'    pAscissaOriginalValues.Clear()
'    pOrdinataOriginalValues.Clear()
'    IDcolonnaBestVmgUpwind = 1
'    IDcolonnaBestVmgDownwind = 5

'    Dim BaseTWA As Double() = {25, 55, 70, 90, 110, 125, 160}

'    For i As Integer = 4 To 30 Step 2
'      pAscissaOriginalValues.Add(i)
'      Dim ValoriOrdinataTmp As New List(Of Double)
'      Dim ValoriTmp As New List(Of Double)
'      Dim idcolonna As Integer = 0
'      For Each TWA In BaseTWA
'        If idcolonna = IDcolonnaBestVmgUpwind Then
'          ValoriOrdinataTmp.Add(TWA - (i / 4))
'        ElseIf idcolonna = IDcolonnaBestVmgDownwind Then
'          ValoriOrdinataTmp.Add(TWA + (i * 0.8))
'        Else
'          ValoriOrdinataTmp.Add(TWA)
'        End If
'        Select Case Canale.ToLower
'          Case "bs"
'            x = -0.0000105
'            y = 0.0011
'            z = 0.1661
'            Dim k As Double = 0.0337
'            ValoriTmp.Add(i * 0.1 * (x * TWA ^ 3 + y * TWA ^ 2 + z * TWA + k))
'          Case "heel"
'            x = 0.1
'            y = 0.05
'            z = 0.0
'            ValoriTmp.Add(i * x + TWA * y + z)
'          Case "trim"
'            x = 0.001
'            y = 0.0
'            z = 0.0
'            ValoriTmp.Add(i * x + TWA * y + z)
'          Case "rideheight"
'            x = 0.0
'            y = 0.0
'            z = 0.1
'            ValoriTmp.Add(i * x + TWA * y + z)
'          Case Else
'            Stop
'        End Select
'        idcolonna += 1
'      Next
'      pOrdinataOriginalValues.Add(New clsOriginalValues(ValoriOrdinataTmp))
'      PC.AggiungiValoriOriginali(ValoriTmp)
'    Next
'    PC.ImpostaTabellaCompleta(pAscissaOriginalValues, pOrdinataOriginalValues)
'    pPolariCanali.Add(PC)

'  End Sub


'End Class

'Public Class clsPolareCanale
'  'Dim pCanaleValore As clsChannel2020 ' BS grande classico....
'  Dim pNomeCanaleValore As String ' BS grande classico....
'  Dim pOriginalValues As New List(Of clsOriginalValues)
'  Dim pTabellaCompleta As Double(,)

'  Public Sub New(NomeCanaleValore As String)
'    pNomeCanaleValore = NomeCanaleValore
'  End Sub

'  Public Property NomeCanaleValore As String
'    Get
'      Return pNomeCanaleValore
'    End Get
'    Set(value As String)
'      pNomeCanaleValore = value
'    End Set
'  End Property

'  'Public Sub New(CanaleValore As clsChannel2020)
'  '	pCanaleValore = CanaleValore
'  'End Sub

'  'Public Property CanaleValore As clsChannel2020
'  '	Get
'  '		Return pCanaleValore
'  '	End Get
'  '	Set(value As clsChannel2020)
'  '		pCanaleValore = value
'  '	End Set
'  'End Property


'  Public Property TabellaCompleta As Double(,)
'    Get
'      Return pTabellaCompleta
'    End Get
'    Set(value As Double(,))
'      pTabellaCompleta = value
'    End Set
'  End Property

'  Public ReadOnly Property Valore(IndiceAscissa As Integer, IndiceOrdinata As Integer) As Double
'    Get
'      Return pTabellaCompleta(IndiceAscissa, IndiceOrdinata)
'    End Get
'  End Property

'  Public Property OriginalValues As List(Of clsOriginalValues)
'    Get
'      Return pOriginalValues
'    End Get
'    Set(value As List(Of clsOriginalValues))
'      pOriginalValues = value
'    End Set
'  End Property

'  Public Sub AggiungiValoriOriginali(Valori As List(Of Double))
'    pOriginalValues.Add(New clsOriginalValues(Valori))
'  End Sub

'  Public Sub ImpostaTabellaCompleta(AscissaOriginalValues As List(Of Double), OrdinataOriginalValues As List(Of clsOriginalValues))
'    Dim TabellaTemp(AscissaOriginalValues.Count - 1, OrdinataOriginalValues.First.ChannelOriginalValues.Count - 1) As Double

'    For i As Integer = 0 To AscissaOriginalValues.Count - 1
'      For ii As Integer = 0 To OrdinataOriginalValues.First.ChannelOriginalValues.Count - 1
'        TabellaTemp(i, ii) = pOriginalValues(i).ChannelOriginalValues(ii)
'      Next
'    Next
'    pTabellaCompleta = TabellaTemp
'  End Sub

'End Class

'Public Class clsOriginalValues
'  Dim pChannelOriginalValues As List(Of Double)

'  Public Sub New(Valori As List(Of Double))
'    If Valori Is Nothing Then
'      pChannelOriginalValues = New List(Of Double)
'    Else
'      pChannelOriginalValues = Valori
'    End If
'  End Sub

'  Public Property ChannelOriginalValues As List(Of Double)
'    Get
'      Return pChannelOriginalValues
'    End Get
'    Set(value As List(Of Double))
'      pChannelOriginalValues = value
'    End Set
'  End Property


'End Class



'Public Class clsPavarot_Old
'  Implements INotifyPropertyChanged

'  Dim pIDriga As Long 'riga della pavarot ovvero quella in cui avviene il cambio mura
'  Dim pIsUpwind As Boolean

'  Dim pIsValid As Boolean = False

'  Dim pIsStbdToPort As Boolean = True

'  'Dim pColore As System.Windows.Media.Color

'  Dim pSecsTableStart As Integer
'  Dim pIDPrimaRigaTabella As Long 'riga iniziale della pavarot ovvero riga della pavarot meno i secondi prima che si vuole analizzare
'  Dim pIDUltimaRigaTabella As Long 'riga finale della pavarot

'  Dim pSecondiPrimaRigaAntePavarot As Long
'  Dim pSecondiUltimaRigaPostPavarot As Long

'  Dim pIDrigaInizioCampionamentoAntePavarot As Long
'  Dim pIDrigaFineCampionamentoPostPavarot As Long
'  Dim pIDrigaInizioPavarot As Long
'  Dim pIDrigaFinePavarot As Long


'  Dim pMediana As Double = 0
'  Dim pMedianaRec As Double = 0

'  Dim pMedieAnte As clsMediePeriodo
'  Dim pMediePost As clsMediePeriodo
'  Dim pMediePavarot As clsMediePeriodo


'  Dim pMatriceGeoPos As New List(Of clsGeograficPosition)
'  Dim pMatriceGeoPosRelToPavarotPointTWD As New List(Of PointF)
'  Dim pMatriceGeoPosRelToPavarotPointMediana As New List(Of PointF)
'  Dim pMatriceGeoPosRelToPavarotPointMedianaRec As New List(Of PointF)

'  Dim pPuntoPavarot As PointF
'  Dim pPuntoAntePavarotInizioCampionamento As PointF
'  Dim pPuntoAntePavarot As PointF
'  Dim pPuntoPostPavarot As PointF
'  Dim pPuntoPostPavarotFineCampionamento As PointF


'  Dim pTotalPavarotGainLostTWD As Double = 0
'  Dim pTotalPavarotGainLostMediana As Double = 0
'  Dim pTotalPavarotGainLostMedianaRec As Double = 0
'  'Dim pTotalPavarotGainLostPortals As Double = 0
'  Dim pPuntoIntersezione As clsGeograficPosition
'  'Dim pTimeRangePavarot As clsTimeRange
'  Dim pSecondiAnte As Double = 0
'  Dim pSecondiPost As Double = 0

'  Dim pFinalTWAfromInstruments As Double = 0
'  Dim pFinalTWAfromMediana As Double = 0
'  Dim pFinalTWAfromMedianaRec As Double = 0

'  Dim pSecondiInizioCampionamentoAnte As Integer
'  Dim pSecondiFineCampionamentoAnte As Integer
'  Dim pSecondiInizioCampionamentoPost As Integer
'  Dim pSecondiFineCampionamentoPost As Integer

'  Dim pAsseAttivo As eAxisRef = eAxisRef.eTWD
'  Dim pPeriodo As clsPeriod2020

'  'Dim pLossGainTableTWD As New Dictionary(Of Double, Double)
'  'Dim pLossGainTableMediana As New Dictionary(Of Double, Double)
'  'Dim pLossGainTableMedianaRec As New Dictionary(Of Double, Double)

'  Dim pLossGainProgressionTableTwd As New Dictionary(Of Double, Double)
'  Dim pLossGainProgressionTableMediana As New Dictionary(Of Double, Double)
'  Dim pLossGainProgressionTableMedianaRec As New Dictionary(Of Double, Double)

'  Dim pLossGainTableTwdPost As New Dictionary(Of Double, Double)
'  Dim pLossGainTableMedianaPost As New Dictionary(Of Double, Double)
'  Dim pLossGainTableMedianaRecPost As New Dictionary(Of Double, Double)

'  Dim pPostPavarotSlopeTWD As Double
'  Dim pPostPavarotSlopeMediana As Double
'  Dim pPostPavarotSlopeMedianaRec As Double

'  Dim pDescrizionePavarot As String

'  Dim pIdRigaVmgStabile As Integer


'  Public Enum eAxisRef
'    eTWD = 0
'    eMediana = 1
'    eMedianaRec = 2
'  End Enum

'  Public Property AsseAttivo As eAxisRef
'    Get
'      Return pAsseAttivo
'    End Get
'    Set(value As eAxisRef)
'      pAsseAttivo = value
'    End Set
'  End Property

'  Public Property DescrizionePavarot As String
'    Get
'      Return pDescrizionePavarot
'    End Get
'    Set(value As String)
'      pDescrizionePavarot = value
'      OnPropertyChanged("DescrizionePavarot")
'    End Set
'  End Property

'  Public ReadOnly Property DescrizionePavarotDetails As String
'    Get
'      Return MomentoChiave.ToShortDateString & " " & MomentoChiave.ToLongTimeString & " " & pPeriodo.ShortDescription ' stringaGainLost(eAxisRef.eTWD) & ""
'    End Get
'  End Property



'  Public ReadOnly Property SecsTableStart As Integer
'    Get
'      Return pSecsTableStart
'    End Get
'  End Property


'  Public ReadOnly Property PuntoIntersezione As clsGeograficPosition
'    Get
'      Return pPuntoIntersezione
'    End Get
'  End Property


'  Public ReadOnly Property SecondiInizioCampionamentoAnte As Integer
'    Get
'      Return pSecondiInizioCampionamentoAnte
'    End Get
'  End Property

'  Public ReadOnly Property SecondiFineCampionamentoAnte As Integer
'    Get
'      Return pSecondiFineCampionamentoAnte
'    End Get
'  End Property

'  Public ReadOnly Property SecondiInizioCampionamentoPost As Integer
'    Get
'      Return pSecondiInizioCampionamentoPost
'    End Get
'  End Property

'  Public ReadOnly Property SecondiFineCampionamentoPost As Integer
'    Get
'      Return pSecondiFineCampionamentoPost
'    End Get
'  End Property

'  Public ReadOnly Property SecondiAnte As Double
'    Get
'      Return pSecondiAnte
'    End Get
'  End Property

'  Public ReadOnly Property SecondiPost As Double
'    Get
'      Return pSecondiPost
'    End Get
'  End Property


'  Public ReadOnly Property PavarotAxis(AxisRef As eAxisRef) As Double
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pMediana
'        Case eAxisRef.eMedianaRec
'          Return pMedianaRec
'        Case Else
'          Return pMediePavarot.TWDMedia
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property MatriceGeoPos As List(Of clsGeograficPosition)
'    Get
'      Return pMatriceGeoPos
'    End Get
'  End Property

'  Public ReadOnly Property MatriceGeoPosRelToPavarotPoint(AxisRef As eAxisRef) As List(Of PointF)
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pMatriceGeoPosRelToPavarotPointMediana
'        Case eAxisRef.eMedianaRec
'          Return pMatriceGeoPosRelToPavarotPointMedianaRec
'        Case Else
'          Return pMatriceGeoPosRelToPavarotPointTWD
'      End Select
'    End Get
'  End Property


'  Public ReadOnly Property PuntoPavarot As PointF
'    Get
'      Return pPuntoPavarot
'    End Get
'  End Property

'  Public ReadOnly Property PuntoAntePavarotInizioCampionamento As PointF
'    Get
'      Return pPuntoAntePavarotInizioCampionamento
'    End Get
'  End Property

'  Public ReadOnly Property PuntoAntePavarot As PointF
'    Get
'      Return pPuntoAntePavarot
'    End Get
'  End Property

'  Public ReadOnly Property PuntoPostPavarot As PointF
'    Get
'      Return pPuntoPostPavarot
'    End Get
'  End Property

'  Public ReadOnly Property PuntoPostPavarotFineCampionamento As PointF
'    Get
'      Return pPuntoPostPavarotFineCampionamento
'    End Get
'  End Property

'  Public ReadOnly Property MatriceTWA(AxisRef As eAxisRef) As List(Of Double)
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pMediePavarot.MatriceTWAmediana
'        Case eAxisRef.eMedianaRec
'          Return pMediePavarot.MatriceTWAmedianaRec
'        Case Else
'          Return pMediePavarot.MatriceTWAtwd
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property MatriceMsVMG(AxisRef As eAxisRef) As List(Of Double)
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pMediePavarot.MatriceMsVMGmediana
'        Case eAxisRef.eMedianaRec
'          Return pMediePavarot.MatriceMsVMGmedianaRec
'        Case Else
'          Return pMediePavarot.MatriceMsVMGtwd
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property MatriceNodiVMG(AxisRef As eAxisRef) As List(Of Double)
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pMediePavarot.MatriceNodiVMGmediana
'        Case eAxisRef.eMedianaRec
'          Return pMediePavarot.MatriceNodiVMGmedianaRec
'        Case Else
'          Return pMediePavarot.MatriceNodiVMGtwd
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property VMGkts(AxisRef As eAxisRef, Ante As Boolean) As Double
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          If Ante Then
'            Return pMedieAnte.VMGmedia(pMediana)
'          Else
'            Return pMediePost.VMGmedia(pMediana)
'          End If
'        Case eAxisRef.eMedianaRec
'          'ante e post devono essere uguali per definizione di medianarec
'          Return pMedieAnte.VMGmedia(pMedianaRec)
'          Return pMediePost.VMGmedia(pMedianaRec)
'        Case Else
'          If Ante Then
'            Return pMedieAnte.VMGmedia(TWD)
'          Else
'            Return pMediePost.VMGmedia(TWD)
'          End If
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property VMGms(AxisRef As eAxisRef, Ante As Boolean) As Double
'    Get
'      Return KtsToMS(VMGkts(AxisRef, Ante))
'    End Get
'  End Property

'  Public ReadOnly Property TotalPavarotLostGainVMGmeters(AxisRef As eAxisRef) As Double
'    Get
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return pTotalPavarotGainLostMediana
'        Case eAxisRef.eMedianaRec
'          Return pTotalPavarotGainLostMedianaRec
'        Case Else
'          Return pTotalPavarotGainLostTWD
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property TotalPavarotLostGainInlineMeters(AxisRef As eAxisRef) As Double
'    Get
'      Dim VMGlost As Double = 0
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          VMGlost = pTotalPavarotGainLostMediana
'        Case eAxisRef.eMedianaRec
'          VMGlost = pTotalPavarotGainLostMedianaRec
'        Case Else
'          VMGlost = pTotalPavarotGainLostTWD
'      End Select
'      Dim TWAavg As Double = (TWA(True, AxisRef) + TWA(True, AxisRef)) / 2
'      Return VMGlost / System.Math.Abs(System.Math.Cos(Radians(TWAavg)))
'    End Get
'  End Property

'  Public ReadOnly Property TWA(Ante As Boolean, AxisRef As eAxisRef) As Double
'    Get
'      Dim Rtmp As Double = 0
'      Select Case AxisRef
'        Case eAxisRef.eMediana
'          Return DifferenzaAssolutaTraAngoli360(pMediana, IIf(Ante, pMedieAnte.RottaMedia, pMediePost.RottaMedia), True)
'        Case eAxisRef.eMedianaRec
'          Return DifferenzaAssolutaTraAngoli360(pMedianaRec, IIf(Ante, pMedieAnte.RottaMedia, pMediePost.RottaMedia), True)
'        Case Else
'          Return DifferenzaAssolutaTraAngoli360(pMediePavarot.TWDMedia, IIf(Ante, pMedieAnte.RottaMedia, pMediePost.RottaMedia), True)
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property Mediana As Double
'    Get
'      Return pMediana
'    End Get
'  End Property

'  Public ReadOnly Property TWD As Double
'    Get
'      Return pMediePavarot.TWDMedia
'    End Get
'  End Property

'  Public ReadOnly Property MedianaRec As Double
'    Get
'      Return pMedianaRec
'    End Get
'  End Property

'  Public ReadOnly Property IdPrimaRigaTabella As Long
'    Get
'      Return pIDPrimaRigaTabella
'    End Get
'  End Property

'  Public ReadOnly Property IdUltimaRigaTabella As Long
'    Get
'      Return pIDUltimaRigaTabella
'    End Get
'  End Property

'  Public ReadOnly Property SecondiPrimaRigaAntePavarot As Long
'    Get
'      Return pSecondiPrimaRigaAntePavarot
'    End Get
'  End Property

'  Public ReadOnly Property SecondiUltimaRigaPostPavarot As Long
'    Get
'      Return pSecondiUltimaRigaPostPavarot
'    End Get
'  End Property

'  Public ReadOnly Property IdRigaInizioPavarot As Long
'    Get
'      Return pIDrigaInizioPavarot
'    End Get
'  End Property

'  Public ReadOnly Property IdRigaInizioPavarotCampionamento As Long
'    Get
'      Return pIDrigaInizioCampionamentoAntePavarot
'    End Get
'  End Property

'  Public ReadOnly Property IdRigaFinePavarot As Long
'    Get
'      Return pIDrigaFinePavarot
'    End Get
'  End Property

'  Public ReadOnly Property IdRigaFinePavarotCampionamento As Long
'    Get
'      Return pIDrigaFineCampionamentoPostPavarot
'    End Get
'  End Property


'  Public ReadOnly Property IDriga As Long
'    Get
'      Return pIDriga
'    End Get
'  End Property

'  Public ReadOnly Property MomentoChiave As DateTime
'    Get
'      Return Periodo.KeyMoment
'    End Get
'  End Property

'  Public ReadOnly Property IsUpwind As Boolean
'    Get
'      Return pIsUpwind
'    End Get
'  End Property

'  Public ReadOnly Property IsStbdToPort As Boolean
'    Get
'      Return pIsStbdToPort
'    End Get
'  End Property

'  Public ReadOnly Property IsValid As Boolean
'    Get
'      Return pIsValid
'    End Get
'  End Property

'  Public Property Periodo As clsPeriod2020
'    Get
'      Return pPeriodo
'    End Get
'    Set(value As clsPeriod2020)
'      pPeriodo = value
'    End Set
'  End Property

'  Public Property MedieAnte As clsMediePeriodo
'    Get
'      Return pMedieAnte
'    End Get
'    Set(value As clsMediePeriodo)
'      pMedieAnte = value
'    End Set
'  End Property

'  Public Property MediePost As clsMediePeriodo
'    Get
'      Return pMediePost
'    End Get
'    Set(value As clsMediePeriodo)
'      pMediePost = value
'    End Set
'  End Property

'  Public Property MediePavarot As clsMediePeriodo
'    Get
'      Return pMediePavarot
'    End Get
'    Set(value As clsMediePeriodo)
'      pMediePavarot = value
'    End Set
'  End Property


'  Public ReadOnly Property LossGainProgressionTable(AsseRiferimento As eAxisRef) As Dictionary(Of Double, Double)
'    Get
'      Select Case AsseRiferimento
'        Case eAxisRef.eMediana
'          Return pLossGainProgressionTableMediana
'        Case eAxisRef.eMedianaRec
'          Return pLossGainProgressionTableMedianaRec
'        Case Else
'          Return pLossGainProgressionTableTwd
'      End Select
'    End Get
'  End Property

'  Public ReadOnly Property PostPavarotSlope(AsseRiferimento As eAxisRef) As Double
'    Get
'      Select Case AsseRiferimento
'        Case eAxisRef.eMediana
'          Return pPostPavarotSlopeMediana
'        Case eAxisRef.eMedianaRec
'          Return pPostPavarotSlopeMedianaRec
'        Case Else
'          Return pPostPavarotSlopeTWD
'      End Select
'    End Get
'  End Property



'  Public ReadOnly Property PostPavarotSlopeTWD As Double
'    'questa Slope deriva dal punto di fine pavarot ed il relativo fien campionamento successivo
'    Get
'      Return pPostPavarotSlopeTWD
'    End Get
'  End Property

'  Public ReadOnly Property PostPavarotSlopeMediana As Double
'    'questa Slope deriva dal punto di fine pavarot ed il relativo fien campionamento successivo
'    Get
'      Return pPostPavarotSlopeMediana
'    End Get
'  End Property

'  Public ReadOnly Property PostPavarotSlopeMedianaRec As Double
'    'questa Slope deriva dal punto di fine pavarot ed il relativo fien campionamento successivo
'    Get
'      Return pPostPavarotSlopeMedianaRec
'    End Get
'  End Property

'  Public ReadOnly Property stringaGainLost(AsseRiferimento As eAxisRef) As String
'    Get
'      Dim valori As Integer() = {0, 5, 10, 15, 20, 25, 30}
'      Dim strTMP As String = "("
'      Select Case AsseRiferimento
'        Case eAxisRef.eMediana
'          For Each valore In valori
'            Dim LG As Double = pLossGainProgressionTableMediana.Valori(valore)
'            strTMP &= Format(LG, "F1") & ","
'          Next
'          strTMP = strTMP.TrimEnd(",") & " - " & Format(pPostPavarotSlopeMediana, "F1") & ")"
'        Case eAxisRef.eMedianaRec
'          For Each valore In valori
'            Dim LG As Double = pLossGainProgressionTableMedianaRec.Valori(valore)
'            strTMP &= Format(LG, "F1") & ","
'          Next
'          strTMP = strTMP.TrimEnd(",") & " - " & Format(pPostPavarotSlopeMedianaRec, "F1") & ")"
'        Case Else
'          For Each valore In valori
'            Dim LG As Double = pLossGainProgressionTableTwd.Valori(valore)
'            strTMP &= Format(LG, "F1") & ","
'          Next
'          strTMP = strTMP.TrimEnd(",") & " - " & Format(pPostPavarotSlopeTWD, "F1") & ")"
'      End Select
'      Return strTMP
'    End Get
'  End Property

'  Public ReadOnly Property IdRigaVmgStabile As Integer
'    Get
'      Return pIdRigaVmgStabile
'    End Get
'  End Property

'  Public Sub New(Periodo As clsPeriod2020)
'    pPeriodo = Periodo
'    pIDriga = dataProvider2020.TrovaIndice(Periodo.KeyMoment)
'    pIsStbdToPort = Periodo.TWA > 0
'    pIsUpwind = (Periodo.PeriodType = clsPeriod2020.ePeriodType.eTack)
'  End Sub

'  Private Function Mure() As String
'    If pIsStbdToPort Then
'      Return "StP"
'    Else
'      Return "PtS"
'    End If
'  End Function

'  Private Function UpDn() As String
'    If pIsUpwind Then
'      Return "Up"
'    Else
'      Return "Dn"
'    End If
'  End Function

'  Private Function Valid() As String
'    If pIsValid Then
'      Return "Valid"
'    Else
'      Return "Unvalid"
'    End If
'  End Function

'  Private Function AverageBsAntePost() As Double
'    Dim A As Double = pMedieAnte.SpeedMedia
'    Dim B As Double = pMedieAnte.SpeedMedia
'    Return (A + B) / 2
'  End Function

'  Private Function AverageTwaAntePost() As Double
'    Dim A As Double = TWA(True, pAsseAttivo)
'    Dim B As Double = TWA(False, pAsseAttivo)
'    Return (A + B) / 2
'  End Function

'  Public Sub AggiornaDescrizione()
'    Dim Gain As Double = TotalPavarotLostGainVMGmeters(pAsseAttivo) 'pTotalPavarotGainLostMedianaRec
'    Dim GainInLine As Double = TotalPavarotLostGainInlineMeters(pAsseAttivo) 'pTotalPavarotGainLostMedianaRec
'    Dim strGain As String = Format(System.Math.Abs(Gain), "F0").PadLeft(2, "0")
'    Dim strGainInLine As String = Format(System.Math.Abs(GainInLine), "F0").PadLeft(2, "0")
'    If Periodo.TimeRange Is Nothing Then
'      DescrizionePavarot = MomentoChiave.ToLongTimeString
'    Else
'      If Gain < 0 Then 'pMedieAnte.RottaMedia, pMediePost.RottaMedia
'        DescrizionePavarot = MomentoChiave.ToLongTimeString & " TWS:" & IIf(pMediePavarot.TWSMedia >= 10, "", " ") & Format(pMediePavarot.TWSMedia, "F0") & "kts, lenght:" & Format(Periodo.TimeRange.Durata.TotalSeconds, "F0") & "ss [-" & strGain & "m,-" & Format(System.Math.Abs(Gain) / NodiToMetriSec(AverageBsAntePost), "0") & "s]"
'      Else
'        DescrizionePavarot = MomentoChiave.ToLongTimeString & " TWS:" & IIf(pMediePavarot.TWSMedia >= 10, "", " ") & Format(pMediePavarot.TWSMedia, "F0") & "kts, lenght:" & Format(Periodo.TimeRange.Durata.TotalSeconds, "F0") & "ss [ " & strGain & "m, " & Format(System.Math.Abs(Gain) / NodiToMetriSec(AverageBsAntePost), "0") & "s]"
'      End If
'      'If Gain < 0 Then 'pMedieAnte.RottaMedia, pMediePost.RottaMedia
'      '  DescrizionePavarot = MomentoChiave.ToLongTimeString & " TWS:" & IIf(pMediePavarot.TWSMedia >= 10, "", " ") & Format(pMediePavarot.TWSMedia, "F0") & "kts, lenght:" & Format(Periodo.TimeRange.Durata.TotalSeconds, "F0") & "ss [-" & strGainInLine & "m,-" & Format(System.Math.Abs(Gain) / NodiToMetriSec(AverageBsAntePost), "0") & "s]"
'      'Else
'      '  DescrizionePavarot = MomentoChiave.ToLongTimeString & " TWS:" & IIf(pMediePavarot.TWSMedia >= 10, "", " ") & Format(pMediePavarot.TWSMedia, "F0") & "kts, lenght:" & Format(Periodo.TimeRange.Durata.TotalSeconds, "F0") & "ss [ " & strGainInLine & "m, " & Format(System.Math.Abs(Gain) / NodiToMetriSec(AverageBsAntePost), "0") & "s]"
'      'End If
'    End If
'  End Sub

'  Public Function ImpostaDatiPavarot_test(SecsTableStart As Integer, SecsAnte As Integer, MaxSecsPost As Integer, AvgLenghtSecs As Integer, DeltaRotta As Integer, MinVmgPerc As Double, UsaTarget As Boolean) As Boolean ', CurrRate As Double, CurrDir As Double)

'    ' SecsTableStart: secondi prima della pavarot per la creazione della tabella
'    ' SecsAnte: secondi prima della pavarot imposti come inizio della stessa (un migliore algoritmo potrebbe avere anche questo punto individuato quale l'inizio della rotazione e non un tempo costante
'    ' MaxSecsPost: secondi dopo la pavarot per la creazione della tabella, nonche' termine massimo perché si verifichino le condizioni per considerare valida la stessa
'    ' AvgLenghtSecs: secondi prima del momento di inizio pavarot e dopo il momento individuato quale fine della stessa usati per calcolare i valori medi ante e post
'    ' DeltaRotta: variazione in gradi della rotta quale limite al secondo per considerare la rotta stabile
'    ' MinVmgPerc: minima vmg percentuale' per considerare la pavarot potenzialmente conclusa, valori da 0 a 1. 1 significa minimo stessa vmg che nel periodo ante

'    '- 30 inizio tabella
'    ' -(SecsAnte+AvgLenghtSecs) inizio calcoli status ante pavarot
'    ' -SecsAnte inizio pavarot nonché fine calcoli status ante pavarot
'    '  0  momento pavarot
'    ' +?  fine pavarot calcolato nonché inizio calcoli status post pavarot
'    ' +?+AvgLenghtSecs fine calcoli status post pavarot 
'    ' +MaxSecsPost fine tabella

'    ' devo provare con il delta posizione al posto del cog e del sog
'    ' versione della funzione che riempie le matrici nell'intorno di 30 secondi 

'    pSecsTableStart = SecsTableStart
'    pIsValid = True


'    pIDPrimaRigaTabella = dataProvider2020.TrovaIndice(MomentoChiave.AddSeconds(-SecsTableStart))
'    'Dim Prova As DateTime = dataProvider2020.TimeStamps(pIDPrimaRigaTabella)
'    If pIDPrimaRigaTabella < 0 Then
'      pIDPrimaRigaTabella = 0
'      pSecondiPrimaRigaAntePavarot = MomentoChiave.Subtract(dataProvider2020.TimeRange.Inizio).TotalSeconds
'      pIsValid = False
'      Return False
'    End If
'    Dim PrimaRigaTabella As DateTime = dataProvider2020.TimeStamps(pIDPrimaRigaTabella)
'    pSecondiPrimaRigaAntePavarot = MomentoChiave.Subtract(PrimaRigaTabella).TotalSeconds


'    pIDUltimaRigaTabella = System.Math.Min(dataProvider2020.TimeRange.IdRigaFinale, dataProvider2020.TrovaIndice(MomentoChiave.AddSeconds(MaxSecsPost)))
'    Dim UltimaRigaTabella As DateTime = dataProvider2020.TimeStamps(pIDUltimaRigaTabella)
'    pSecondiUltimaRigaPostPavarot = UltimaRigaTabella.Subtract(MomentoChiave).TotalSeconds

'    Dim InizioPavarot As DateTime = MomentoChiave.AddSeconds(-SecsAnte)
'    pIDrigaInizioPavarot = dataProvider2020.TrovaIndice(InizioPavarot) 'inizio della pavarot


'    pSecondiInizioCampionamentoAnte = (pIDrigaInizioPavarot - pIDriga) / dataProvider2020.RawFileHz
'    pSecondiFineCampionamentoAnte = (pSecondiInizioCampionamentoAnte - AvgLenghtSecs)
'    Dim pSecondiInizioCampionamentoPost As Integer
'    Dim pSecondiFineCampionamentoPost As Integer



'    Dim InizioCampionamentoAntePavarot As DateTime = InizioPavarot.AddSeconds(-AvgLenghtSecs)
'    pIDrigaInizioCampionamentoAntePavarot = dataProvider2020.TrovaIndice(InizioCampionamentoAntePavarot) 'inizio della pavarot

'    pMedieAnte = New clsMediePeriodo(New clsTimeRange(InizioCampionamentoAntePavarot, InizioPavarot))
'    pMediePavarot = New clsMediePeriodo(New clsTimeRange(PrimaRigaTabella, UltimaRigaTabella))

'    pIDrigaFinePavarot = pIDUltimaRigaTabella
'    Dim MediaMobileLunga As New clsMediaMobile360(5 * dataProvider2020.RawFileHz)
'    Dim MediaMobileBreve As New clsMediaMobile360(3 * dataProvider2020.RawFileHz)
'    Dim cRotta As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
'    Dim cVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    UsaTarget = UsaTarget And Not Targets Is Nothing
'    Dim VmgTwdAnte As Double = pMedieAnte.VMGmedia(TWD)

'    'Dim IsUp As Boolean = pMedieAnte.TWAmedia <= 90
'    Dim TabellaPolare As clsPolare2019CanaleValori = Nothing
'    If UsaTarget Then
'      TabellaPolare = Targets.GetPolare("bs")
'      UsaTarget = UsaTarget And Not TabellaPolare Is Nothing
'      If UsaTarget Then
'        Dim BSt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUp(pMedieAnte.TWSMedia), TabellaPolare.ValoreTargetDn(pMedieAnte.TWSMedia))
'        Dim TWAt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUpReferece(pMedieAnte.TWSMedia), TabellaPolare.ValoreTargetDnReferece(pMedieAnte.TWSMedia))
'        Dim VMGt As Double = System.Math.Abs(BSt * System.Math.Cos(Radians(TWAt)))
'        VmgTwdAnte = VMGt
'      End If

'    End If

'    For Riga As Integer = pIDriga To pIDUltimaRigaTabella
'      MediaMobileBreve.AggiornaMedia(cRotta.Valori(Riga))
'      MediaMobileLunga.AggiornaMedia(cRotta.Valori(Riga))
'      If MediaMobileLunga.PrimoGiroCompletato Then
'        If (cVmg.Valori(Riga) / VmgTwdAnte) > MinVmgPerc Then
'          'la VMG é superiore alla minima richiesta per considerare la pavarot conclusa
'          If DifferenzaAssolutaTraAngoli360(MediaMobileBreve.Valore, MediaMobileLunga.Valore, True) < DeltaRotta Then
'            'la rotta é stabile ovvero la media a 3 e 5 secondi é minore uguale al DeltaRotta
'            If pIsStbdToPort Then
'              If pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileBreve.Valore, MediaMobileLunga.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              ElseIf Not pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileLunga.Valore, MediaMobileBreve.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              End If
'            Else
'              If pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileLunga.Valore, MediaMobileBreve.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              ElseIf Not pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileBreve.Valore, MediaMobileLunga.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              End If
'            End If
'          End If
'        End If
'      End If
'    Next

'    Dim FinePavarot As DateTime = dataProvider2020.TimeStamps(pIDrigaFinePavarot)
'    Dim FineCampionamentoPostPavarot As DateTime = FinePavarot.AddSeconds(AvgLenghtSecs)
'    If pIDrigaFinePavarot = pIDUltimaRigaTabella Then
'      pIDrigaFinePavarot = pIDUltimaRigaTabella - (AvgLenghtSecs * dataProvider2020.RawFileHz)
'      FinePavarot = dataProvider2020.TimeStamps(pIDrigaFinePavarot)
'      FineCampionamentoPostPavarot = FinePavarot.AddSeconds(AvgLenghtSecs)
'    End If

'    'pMediePost = New clsMediePeriodo(New clsTimeRange(FinePavarot, FineCampionamentoPostPavarot))
'    Dim FineTabellaPavarot As DateTime = dataProvider2020.TimeStamps(pIDUltimaRigaTabella)

'    pMediePost = New clsMediePeriodo(New clsTimeRange(FineTabellaPavarot.AddSeconds(-SecsAnte), FineTabellaPavarot))

'    pIDrigaFineCampionamentoPostPavarot = pIDrigaFinePavarot + (AvgLenghtSecs * dataProvider2020.RawFileHz)

'    pSecondiInizioCampionamentoPost = (pIDrigaFinePavarot - pIDriga) / dataProvider2020.RawFileHz
'    pSecondiFineCampionamentoPost = (pSecondiInizioCampionamentoPost + AvgLenghtSecs)


'    'vengono impostati i due assi calcolati, quello mediano che rappresenta l'asse tra la rotta ante e la rotta post
'    'quello mediano ricalcolato in modo da avere stesso VMG tra ante e post pavarot, se sog ante e post sono uguali corrisponde a quello mediano
'    pMediana = SommaAngolo180adAngolo360((DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMedieAnte.RottaMedia, pMediePost.RottaMedia) / 2), pMedieAnte.RottaMedia)
'    If Not pIsUpwind Then pMediana = SommaAngolo180adAngolo360(180, pMediana)

'    Dim pTWDaxis As Double = TWD
'    If Not pIsUpwind Then pTWDaxis = SommaAngolo180adAngolo360(180, TWD)

'    If DifferenzaAssolutaTraAngoli360(pMedieAnte.RottaMedia, pTWDaxis, True) < 10 OrElse DifferenzaAssolutaTraAngoli360(pMediePost.RottaMedia, pTWDaxis, True) < 10 Then
'      'se prima o dopo la virata la rotta é men di 10 gradi dalla TWD la considera non valida
'      pIsValid = False
'    End If

'    If DifferenzaAssolutaTraAngoli360(pMediana, TWD, True) > 20 Then
'      'se la mediana si discosta piu'di 20 gradi dall' asse del vento la pavarot non é valida
'      pIsValid = False
'    End If

'    pMedianaRec = MadianToAvenAntePost(pMedieAnte, pMediePost)


'    Dim MediaVMGanteTWD As Double = pMedieAnte.VMGmediaMS(TWD)
'    If UsaTarget Then MediaVMGanteTWD = KtsToMS(VmgTwdAnte)
'    Dim MediaVMGanteMediana As Double = pMedieAnte.VMGmediaMS(pMediana)
'    Dim MediaVMGanteMedianaRec As Double = pMedieAnte.VMGmediaMS(pMedianaRec)
'    Dim MediaVMGpostTWD As Double = pMediePost.VMGmediaMS(TWD)
'    If UsaTarget Then
'      'Dim BSt As Double = TabellaPolare.ValoreTarget(pMediePost.TWSMedia, IsUp)
'      'Dim TWAt As Double = IIf(IsUp, Polari.RigaTgtUp.Valore(pMediePost.TWSMedia), Polari.RigaTgtDn.Valore(pMediePost.TWSMedia))
'      Dim BSt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUp(pMediePost.TWSMedia), TabellaPolare.ValoreTargetDn(pMediePost.TWSMedia))
'      Dim TWAt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUpReferece(pMediePost.TWSMedia), TabellaPolare.ValoreTargetDnReferece(pMediePost.TWSMedia))
'      Dim VMGt As Double = System.Math.Abs(BSt * System.Math.Cos(Radians(TWAt)))
'      MediaVMGpostTWD = KtsToMS(VMGt)
'    End If
'    Dim MediaVMGpostMediana As Double = pMediePost.VMGmediaMS(pMediana)
'    Dim MediaVMGpostMedianaRec As Double = pMediePost.VMGmediaMS(pMedianaRec)


'    pTotalPavarotGainLostTWD = 0
'    pTotalPavarotGainLostMediana = 0
'    pTotalPavarotGainLostMedianaRec = 0
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eTWD, TWD, pIsUpwind)
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eMediana, pMediana, pIsUpwind)
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eMedianaRec, pMedianaRec, pIsUpwind)
'    'pLossGainTableTWD.Clear()
'    'pLossGainTableMediana.Clear()
'    'pLossGainTableMedianaRec.Clear()
'    pLossGainProgressionTableTwd.Clear()
'    pLossGainProgressionTableMediana.Clear()
'    pLossGainProgressionTableMedianaRec.Clear()


'    Dim VmgMsTwd As Double = System.Math.Max(MediaVMGanteTWD, MediaVMGpostTWD) ' MediaVMGanteTWD ' (MediaVMGanteTWD + MediaVMGpostTWD) / 2
'    Dim VmgMsMediana As Double = System.Math.Max(MediaVMGanteMediana, MediaVMGpostMediana) ' MediaVMGanteMediana ' (MediaVMGanteMediana + MediaVMGpostMediana) / 2
'    Dim VmgMsMedianaRec As Double = System.Math.Max(MediaVMGanteMedianaRec, MediaVMGpostMedianaRec) ' MediaVMGanteMedianaRec ' (MediaVMGanteMedianaRec + MediaVMGpostMedianaRec) / 2


'    Dim pLossGainTableTwdTmp As New Dictionary(Of Double, Double)
'    Dim pLossGainTableMedianaTmp As New Dictionary(Of Double, Double)
'    Dim pLossGainTableMedianaRecTmp As New Dictionary(Of Double, Double)
'    Dim A As Double = 0
'    Dim B As Double = 0
'    Dim C As Double = 0

'    'il conteggio inizia da quando inizia la pavarot
'    Dim SecFromPavarot As Integer
'    'For riga As Integer = pIDrigaInizioPavarot To System.Math.Min(pIDUltimaRigaTabella, pIDPrimaRigaTabella + pMediePavarot.MatriceMsVMGtwd.Count - 1)
'    Dim SecondiProiezioneVmgFinale As Integer = 10
'    Dim CampoVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    For riga As Integer = pIDrigaInizioPavarot To pIDUltimaRigaTabella + (SecondiProiezioneVmgFinale * dataProvider2020.RawFileHz)
'      Dim RigaRel As Integer = riga - pIDPrimaRigaTabella
'      'VmgMsTwd = If(riga <= IDriga, MediaVMGanteTWD, MediaVMGpostTWD)
'      If riga > pIDPrimaRigaTabella + pMediePavarot.MatriceMsVMGtwd.Count - 1 OrElse riga > pIDUltimaRigaTabella Then
'        Dim VmgMs As Double = KtsToMS(CampoVmg.Valori(riga))
'        A += (VmgMs - VmgMsTwd) / dataProvider2020.RawFileHz
'        ' vanno mesi i vmg relativi agli assi
'        B += (VmgMs - VmgMsMediana) / dataProvider2020.RawFileHz
'        C += (VmgMs - VmgMsMedianaRec) / dataProvider2020.RawFileHz
'      Else
'        A += (pMediePavarot.MatriceMsVMGtwd(RigaRel) - VmgMsTwd) / dataProvider2020.RawFileHz
'        B += (pMediePavarot.MatriceMsVMGmediana(RigaRel) - VmgMsMediana) / dataProvider2020.RawFileHz
'        C += (pMediePavarot.MatriceMsVMGmedianaRec(RigaRel) - VmgMsMedianaRec) / dataProvider2020.RawFileHz
'      End If
'      SecFromPavarot = CInt((riga - pIDriga) / dataProvider2020.RawFileHz)
'      If Not pLossGainProgressionTableTwd.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableTwd.Add(SecFromPavarot, A)
'      If Not pLossGainProgressionTableMediana.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableMediana.Add(SecFromPavarot, B)
'      If Not pLossGainProgressionTableMedianaRec.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableMedianaRec.Add(SecFromPavarot, C)


'      If riga <= pIDrigaFinePavarot Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come pavarot terminata
'        pTotalPavarotGainLostTWD = A '+= (pMediePavarot.MatriceMetriVMGtwd(RigaRel) - ((MediaVMGanteTWD + MediaVMGpostTWD) / 2)) / dataProvider2020.RawFileHz
'        pTotalPavarotGainLostMediana = B '+= (pMediePavarot.MatriceMetriVMGmediana(RigaRel) - ((MediaVMGanteMediana + MediaVMGpostMediana) / 2)) / dataProvider2020.RawFileHz
'        pTotalPavarotGainLostMedianaRec = C '+= (pMediePavarot.MatriceMetriVMGmedianaRec(RigaRel) - ((MediaVMGanteMedianaRec + MediaVMGpostMedianaRec) / 2)) / dataProvider2020.RawFileHz

'      End If
'      If riga = System.Math.Min(pIDrigaFinePavarot, pIDUltimaRigaTabella - AvgLenghtSecs * dataProvider2020.RawFileHz) Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come pavarot terminata oppure X secondi prima della fine della tabella
'        pLossGainTableTwdTmp.Add(riga, A)
'        pLossGainTableMedianaTmp.Add(riga, B)
'        pLossGainTableMedianaRecTmp.Add(riga, C)
'      ElseIf riga = System.Math.Min(pIDrigaFineCampionamentoPostPavarot, IdUltimaRigaTabella) Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come campionamento pavarot terminato oppure alla fine della tabella
'        pLossGainTableTwdTmp.Add(riga, A)
'        pLossGainTableMedianaTmp.Add(riga, B)
'        pLossGainTableMedianaRecTmp.Add(riga, C)
'      End If

'      'Dim RigaDaPavarot As Integer = (riga - pIDriga) / dataProvider2020.RawFileHz
'      'Select Case SecFromPavarot
'      '  Case 0, 5, 10, 20, 30
'      '    'vengono memorizzati i metri persi/guadagnati a determinati intervalli
'      '    'viene memorizzzato solo al primo valore del secondo
'      '    If Not pLossGainTableTWD.ContainsKey(SecFromPavarot) Then pLossGainTableTWD.Add(SecFromPavarot, A)
'      '    If Not pLossGainTableMediana.ContainsKey(SecFromPavarot) Then pLossGainTableMediana.Add(SecFromPavarot, B)
'      '    If Not pLossGainTableMedianaRec.ContainsKey(SecFromPavarot) Then pLossGainTableMedianaRec.Add(SecFromPavarot, C)
'      'End Select

'    Next
'    'SecFromPavarot += 1
'    'pLossGainProgressionTableTwd.Add(SecFromPavarot, A)
'    'pLossGainProgressionTableMediana.Add(SecFromPavarot, B)
'    'pLossGainProgressionTableMedianaRec.Add(SecFromPavarot, C)

'    pLossGainTableTwdPost.Clear()
'    pLossGainTableMedianaPost.Clear()
'    pLossGainTableMedianaRecPost.Clear()

'    Dim SecFromPavarotA As Double = (pIDrigaFinePavarot - pIDriga) / dataProvider2020.RawFileHz
'    Dim SecFromPavarotB As Double = (pIDrigaFineCampionamentoPostPavarot - pIDriga) / dataProvider2020.RawFileHz
'    pLossGainTableTwdPost.Add(SecFromPavarotA, pLossGainTableTwdTmp.Valori(0))
'    pLossGainTableMedianaPost.Add(SecFromPavarotA, pLossGainTableMedianaTmp.Valori(0))
'    pLossGainTableMedianaRecPost.Add(SecFromPavarotA, pLossGainTableMedianaRecTmp.Valori(0))
'    pLossGainTableTwdPost.Add(SecFromPavarotB, pLossGainTableTwdTmp.Valori(1))
'    pLossGainTableMedianaPost.Add(SecFromPavarotB, pLossGainTableMedianaTmp.Valori(1))
'    pLossGainTableMedianaRecPost.Add(SecFromPavarotB, pLossGainTableMedianaRecTmp.Valori(1))

'    pPostPavarotSlopeTWD = (pLossGainTableTwdTmp.Valori(1) - pLossGainTableTwdTmp.Valori(0)) / (pLossGainTableTwdTmp.Keys(1) - pLossGainTableTwdTmp.Keys(0))
'    pPostPavarotSlopeMediana = (pLossGainTableMedianaTmp.Valori(1) - pLossGainTableMedianaTmp.Valori(0)) / (pLossGainTableMedianaTmp.Keys(1) - pLossGainTableMedianaTmp.Keys(0))
'    pPostPavarotSlopeMedianaRec = (pLossGainTableMedianaRecTmp.Valori(1) - pLossGainTableMedianaRecTmp.Valori(0)) / (pLossGainTableMedianaRecTmp.Keys(1) - pLossGainTableMedianaRecTmp.Keys(0))


'    ' CalcoliGeoPoints
'    Dim CG As New clsCalcoliDuePuntiGeo()
'    Dim Lat As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDriga)
'    Dim Lon As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDriga)
'    Dim PuntoRef As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'    pMatriceGeoPos.Clear()
'    pMatriceGeoPosRelToPavarotPointTWD.Clear()
'    pMatriceGeoPosRelToPavarotPointMediana.Clear()
'    pMatriceGeoPosRelToPavarotPointMedianaRec.Clear()

'    For Riga As Integer = pIDPrimaRigaTabella To pIDUltimaRigaTabella
'      ' CalcoliGeoPoints
'      Try
'        Lat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(Riga)
'        Lon = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(Riga)
'        Dim GeoPoint As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'        pMatriceGeoPos.Add(GeoPoint)

'        CG.CalcolaDistanzaAndRotta(PuntoRef, GeoPoint)
'        Dim RNG As Double = CG.Distanza
'        Dim BRG As Double = CG.Rotta
'        Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TWD, BRG)

'        Dim BRGc As Double = BRGinCartesiano(BRGoc)
'        Dim X As Double = RNG * System.Math.Cos(Radians(BRGc))
'        Dim Y As Double = RNG * System.Math.Sin(Radians(BRGc))
'        Dim PuntoTraccia As New PointF(X, Y)

'        Select Case Riga
'          Case pIDriga
'            pPuntoPavarot = PuntoTraccia
'          Case pIDrigaInizioCampionamentoAntePavarot
'            pPuntoAntePavarotInizioCampionamento = PuntoTraccia
'          Case pIDrigaInizioPavarot
'            pPuntoAntePavarot = PuntoTraccia
'          Case pIDrigaFinePavarot
'            pPuntoPostPavarot = PuntoTraccia
'          Case pIDrigaFineCampionamentoPostPavarot
'            pPuntoPostPavarotFineCampionamento = PuntoTraccia
'          Case Else
'        End Select

'        pMatriceGeoPosRelToPavarotPointTWD.Add(PuntoTraccia)

'        BRGoc = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMediana, BRG)
'        BRGc = BRGinCartesiano(BRGoc)
'        X = RNG * System.Math.Cos(Radians(BRGc))
'        Y = RNG * System.Math.Sin(Radians(BRGc))
'        PuntoTraccia = New PointF(X, Y)
'        pMatriceGeoPosRelToPavarotPointMediana.Add(PuntoTraccia)

'        BRGoc = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMedianaRec, BRG)
'        BRGc = BRGinCartesiano(BRGoc)
'        X = RNG * System.Math.Cos(Radians(BRGc))
'        Y = RNG * System.Math.Sin(Radians(BRGc))
'        PuntoTraccia = New PointF(X, Y)
'        pMatriceGeoPosRelToPavarotPointMedianaRec.Add(PuntoTraccia)

'      Catch ex As Exception
'        Stop
'      End Try
'    Next

'    Dim SecFromInizio As Double = 20
'    pTotalPavarotGainLostTWD = 0
'    For ii As Integer = 0 To pLossGainProgressionTableTwd.Count - 1
'      If pLossGainProgressionTableTwd.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostTWD = pLossGainProgressionTableTwd.Valori(ii)
'    Next
'    pTotalPavarotGainLostMediana = 0
'    For ii As Integer = 0 To pLossGainProgressionTableMediana.Count - 1
'      If pLossGainProgressionTableMediana.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostMediana = pLossGainProgressionTableMediana.Valori(ii)
'    Next
'    pTotalPavarotGainLostMedianaRec = 0
'    For ii As Integer = 0 To pLossGainProgressionTableMedianaRec.Count - 1
'      If pLossGainProgressionTableMedianaRec.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostMedianaRec = pLossGainProgressionTableMedianaRec.Valori(ii)
'    Next





'    'Dim CampoVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    'Dim mVmgAnte As New clsMediaMobile(3 * dataProvider2020.RawFileHz)
'    'Dim mVmg As New clsMediaMobile(1 * dataProvider2020.RawFileHz)
'    'Dim Tolleranza As Double = 0.01
'    'pIdRigaVmgStabile = pIDrigaFinePavarot
'    'Dim RisalitaVmg As Boolean = False
'    'For i As Integer = 0 To MaxSecsPost * dataProvider2020.RawFileHz
'    '  Dim Riga As Integer = pIDriga + i
'    '  Dim Vmg As Double = CampoVmg.Valori(Riga)
'    '  mVmgAnte.AggiornaMedia(Vmg)
'    '  mVmg.AggiornaMedia(Vmg)
'    '  If i > 2 * dataProvider2020.RawFileHz AndAlso mVmg.Valore > mVmgAnte.Valore Then
'    '    If Not RisalitaVmg AndAlso System.Math.Abs(mVmg.Valore - mVmgAnte.Valore) / mVmg.Valore <= Tolleranza Then

'    '    Else
'    '      RisalitaVmg = True
'    '    End If
'    '    If RisalitaVmg AndAlso System.Math.Abs(mVmg.Valore - mVmgAnte.Valore) / mVmg.Valore <= Tolleranza Then
'    '      pIdRigaVmgStabile = Riga
'    '      SecFromInizio = i / dataProvider2020.RawFileHz
'    '      pTotalPavarotGainLostTWD = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableTwd.Count - 1
'    '        If pLossGainProgressionTableTwd.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostTWD = pLossGainProgressionTableTwd.Valori(ii)
'    '      Next
'    '      pTotalPavarotGainLostMediana = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableMediana.Count - 1
'    '        If pLossGainProgressionTableMediana.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostMediana = pLossGainProgressionTableMediana.Valori(ii)
'    '      Next
'    '      pTotalPavarotGainLostMedianaRec = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableMedianaRec.Count - 1
'    '        If pLossGainProgressionTableMedianaRec.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostMedianaRec = pLossGainProgressionTableMedianaRec.Valori(ii)
'    '      Next
'    '      Exit For
'    '    End If
'    '  End If
'    'Next


'    AggiornaDescrizione()
'    pPeriodo.IsChecked = pIsValid
'    Return True
'  End Function


'  Public Function ImpostaDatiPavarot(SecsTableStart As Integer, SecsAnte As Integer, MaxSecsPost As Integer, AvgLenghtSecs As Integer, DeltaRotta As Integer, MinVmgPerc As Double, UsaTarget As Boolean) As Boolean ', CurrRate As Double, CurrDir As Double)

'    ' SecsTableStart: secondi prima della pavarot per la creazione della tabella
'    ' SecsAnte: secondi prima della pavarot imposti come inizio della stessa (un migliore algoritmo potrebbe avere anche questo punto individuato quale l'inizio della rotazione e non un tempo costante
'    ' MaxSecsPost: secondi dopo la pavarot per la creazione della tabella, nonche' termine massimo perché si verifichino le condizioni per considerare valida la stessa
'    ' AvgLenghtSecs: secondi prima del momento di inizio pavarot e dopo il momento individuato quale fine della stessa usati per calcolare i valori medi ante e post
'    ' DeltaRotta: variazione in gradi della rotta quale limite al secondo per considerare la rotta stabile
'    ' MinVmgPerc: minima vmg percentuale' per considerare la pavarot potenzialmente conclusa, valori da 0 a 1. 1 significa minimo stessa vmg che nel periodo ante

'    '- 30 inizio tabella
'    ' -(SecsAnte+AvgLenghtSecs) inizio calcoli status ante pavarot
'    ' -SecsAnte inizio pavarot nonché fine calcoli status ante pavarot
'    '  0  momento pavarot
'    ' +?  fine pavarot calcolato nonché inizio calcoli status post pavarot
'    ' +?+AvgLenghtSecs fine calcoli status post pavarot 
'    ' +MaxSecsPost fine tabella

'    ' devo provare con il delta posizione al posto del cog e del sog
'    ' versione della funzione che riempie le matrici nell'intorno di 30 secondi 

'    pSecsTableStart = SecsTableStart
'    pIsValid = True


'    pIDPrimaRigaTabella = dataProvider2020.TrovaIndice(MomentoChiave.AddSeconds(-SecsTableStart))
'    'Dim Prova As DateTime = dataProvider2020.TimeStamps(pIDPrimaRigaTabella)
'    If pIDPrimaRigaTabella < 0 Then
'      pIDPrimaRigaTabella = 0
'      pSecondiPrimaRigaAntePavarot = MomentoChiave.Subtract(dataProvider2020.TimeRange.Inizio).TotalSeconds
'      pIsValid = False
'      Return False
'    End If
'    Dim PrimaRigaTabella As DateTime = dataProvider2020.TimeStamps(pIDPrimaRigaTabella)
'    pSecondiPrimaRigaAntePavarot = MomentoChiave.Subtract(PrimaRigaTabella).TotalSeconds


'    pIDUltimaRigaTabella = System.Math.Min(dataProvider2020.TimeRange.IdRigaFinale, dataProvider2020.TrovaIndice(MomentoChiave.AddSeconds(MaxSecsPost)))
'    Dim UltimaRigaTabella As DateTime = dataProvider2020.TimeStamps(pIDUltimaRigaTabella)
'    pSecondiUltimaRigaPostPavarot = UltimaRigaTabella.Subtract(MomentoChiave).TotalSeconds

'    Dim InizioPavarot As DateTime = MomentoChiave.AddSeconds(-SecsAnte)
'    pIDrigaInizioPavarot = dataProvider2020.TrovaIndice(InizioPavarot) 'inizio della pavarot


'    pSecondiInizioCampionamentoAnte = (pIDrigaInizioPavarot - pIDriga) / dataProvider2020.RawFileHz
'    pSecondiFineCampionamentoAnte = (pSecondiInizioCampionamentoAnte - AvgLenghtSecs)
'    Dim pSecondiInizioCampionamentoPost As Integer
'    Dim pSecondiFineCampionamentoPost As Integer



'    Dim InizioCampionamentoAntePavarot As DateTime = InizioPavarot.AddSeconds(-AvgLenghtSecs)
'    pIDrigaInizioCampionamentoAntePavarot = dataProvider2020.TrovaIndice(InizioCampionamentoAntePavarot) 'inizio della pavarot

'    pMedieAnte = New clsMediePeriodo(New clsTimeRange(InizioCampionamentoAntePavarot, InizioPavarot))
'    pMediePavarot = New clsMediePeriodo(New clsTimeRange(PrimaRigaTabella, UltimaRigaTabella))

'    pIDrigaFinePavarot = pIDUltimaRigaTabella
'    Dim MediaMobileLunga As New clsMediaMobile360(5 * dataProvider2020.RawFileHz)
'    Dim MediaMobileBreve As New clsMediaMobile360(3 * dataProvider2020.RawFileHz)
'    Dim cRotta As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
'    Dim cVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    UsaTarget = UsaTarget And Not Targets Is Nothing
'    Dim VmgTwdAnte As Double = pMedieAnte.VMGmedia(TWD)

'    'Dim IsUp As Boolean = pMedieAnte.TWAmedia <= 90
'    Dim TabellaPolare As clsPolare2019CanaleValori = Nothing
'    If UsaTarget Then
'      TabellaPolare = Targets.GetPolare("bs")
'      UsaTarget = UsaTarget And Not TabellaPolare Is Nothing
'      If UsaTarget Then
'        Dim BSt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUp(pMedieAnte.TWSMedia), TabellaPolare.ValoreTargetDn(pMedieAnte.TWSMedia))
'        Dim TWAt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUpReferece(pMedieAnte.TWSMedia), TabellaPolare.ValoreTargetDnReferece(pMedieAnte.TWSMedia))
'        Dim VMGt As Double = System.Math.Abs(BSt * System.Math.Cos(Radians(TWAt)))
'        VmgTwdAnte = VMGt
'      End If

'    End If

'    For Riga As Integer = pIDriga To pIDUltimaRigaTabella
'      MediaMobileBreve.AggiornaMedia(cRotta.Valori(Riga))
'      MediaMobileLunga.AggiornaMedia(cRotta.Valori(Riga))
'      If MediaMobileLunga.PrimoGiroCompletato Then
'        If (cVmg.Valori(Riga) / VmgTwdAnte) > MinVmgPerc Then
'          'la VMG é superiore alla minima richiesta per considerare la pavarot conclusa
'          If DifferenzaAssolutaTraAngoli360(MediaMobileBreve.Valore, MediaMobileLunga.Valore, True) < DeltaRotta Then
'            'la rotta é stabile ovvero la media a 3 e 5 secondi é minore uguale al DeltaRotta
'            If pIsStbdToPort Then
'              If pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileBreve.Valore, MediaMobileLunga.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              ElseIf Not pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileLunga.Valore, MediaMobileBreve.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              End If
'            Else
'              If pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileLunga.Valore, MediaMobileBreve.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              ElseIf Not pIsUpwind AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(MediaMobileBreve.Valore, MediaMobileLunga.Valore) >= 0 Then
'                pIDrigaFinePavarot = Riga
'                Exit For
'              End If
'            End If
'          End If
'        End If
'      End If
'    Next

'    Dim FinePavarot As DateTime = dataProvider2020.TimeStamps(pIDrigaFinePavarot)
'    Dim FineCampionamentoPostPavarot As DateTime = FinePavarot.AddSeconds(AvgLenghtSecs)
'    If pIDrigaFinePavarot = pIDUltimaRigaTabella Then
'      pIDrigaFinePavarot = pIDUltimaRigaTabella - (AvgLenghtSecs * dataProvider2020.RawFileHz)
'      FinePavarot = dataProvider2020.TimeStamps(pIDrigaFinePavarot)
'      FineCampionamentoPostPavarot = FinePavarot.AddSeconds(AvgLenghtSecs)
'    End If

'    pMediePost = New clsMediePeriodo(New clsTimeRange(FinePavarot, FineCampionamentoPostPavarot))

'    pIDrigaFineCampionamentoPostPavarot = pIDrigaFinePavarot + (AvgLenghtSecs * dataProvider2020.RawFileHz)

'    pSecondiInizioCampionamentoPost = (pIDrigaFinePavarot - pIDriga) / dataProvider2020.RawFileHz
'    pSecondiFineCampionamentoPost = (pSecondiInizioCampionamentoPost + AvgLenghtSecs)


'    'vengono impostati i due assi calcolati, quello mediano che rappresenta l'asse tra la rotta ante e la rotta post
'    'quello mediano ricalcolato in modo da avere stesso VMG tra ante e post pavarot, se sog ante e post sono uguali corrisponde a quello mediano
'    pMediana = SommaAngolo180adAngolo360((DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMedieAnte.RottaMedia, pMediePost.RottaMedia) / 2), pMedieAnte.RottaMedia)
'    If Not pIsUpwind Then pMediana = SommaAngolo180adAngolo360(180, pMediana)

'    Dim pTWDaxis As Double = TWD
'    If Not pIsUpwind Then pTWDaxis = SommaAngolo180adAngolo360(180, TWD)

'    If DifferenzaAssolutaTraAngoli360(pMedieAnte.RottaMedia, pTWDaxis, True) < 10 OrElse DifferenzaAssolutaTraAngoli360(pMediePost.RottaMedia, pTWDaxis, True) < 10 Then
'      'se prima o dopo la virata la rotta é men di 10 gradi dalla TWD la considera non valida
'      pIsValid = False
'    End If

'    If DifferenzaAssolutaTraAngoli360(pMediana, TWD, True) > 20 Then
'      'se la mediana si discosta piu'di 20 gradi dall' asse del vento la pavarot non é valida
'      pIsValid = False
'    End If

'    pMedianaRec = MadianToAvenAntePost(pMedieAnte, pMediePost)


'    Dim MediaVMGanteTWD As Double = pMedieAnte.VMGmediaMS(TWD)
'    If UsaTarget Then MediaVMGanteTWD = KtsToMS(VmgTwdAnte)
'    Dim MediaVMGanteMediana As Double = pMedieAnte.VMGmediaMS(pMediana)
'    Dim MediaVMGanteMedianaRec As Double = pMedieAnte.VMGmediaMS(pMedianaRec)
'    Dim MediaVMGpostTWD As Double = pMediePost.VMGmediaMS(TWD)
'    If UsaTarget Then
'      'Dim BSt As Double = TabellaPolare.ValoreTarget(pMediePost.TWSMedia, IsUp)
'      'Dim TWAt As Double = IIf(IsUp, Polari.RigaTgtUp.Valore(pMediePost.TWSMedia), Polari.RigaTgtDn.Valore(pMediePost.TWSMedia))
'      Dim BSt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUp(pMediePost.TWSMedia), TabellaPolare.ValoreTargetDn(pMediePost.TWSMedia))
'      Dim TWAt As Double = IIf(pIsUpwind, TabellaPolare.ValoreTargetUpReferece(pMediePost.TWSMedia), TabellaPolare.ValoreTargetDnReferece(pMediePost.TWSMedia))
'      Dim VMGt As Double = System.Math.Abs(BSt * System.Math.Cos(Radians(TWAt)))
'      MediaVMGpostTWD = KtsToMS(VMGt)
'    End If
'    Dim MediaVMGpostMediana As Double = pMediePost.VMGmediaMS(pMediana)
'    Dim MediaVMGpostMedianaRec As Double = pMediePost.VMGmediaMS(pMedianaRec)


'    pTotalPavarotGainLostTWD = 0
'    pTotalPavarotGainLostMediana = 0
'    pTotalPavarotGainLostMedianaRec = 0
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eTWD, TWD, pIsUpwind)
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eMediana, pMediana, pIsUpwind)
'    pMediePavarot.ImpostaMatricTWAandeMetriVMG(eAxisRef.eMedianaRec, pMedianaRec, pIsUpwind)
'    'pLossGainTableTWD.Clear()
'    'pLossGainTableMediana.Clear()
'    'pLossGainTableMedianaRec.Clear()
'    pLossGainProgressionTableTwd.Clear()
'    pLossGainProgressionTableMediana.Clear()
'    pLossGainProgressionTableMedianaRec.Clear()


'    Dim VmgMsTwd As Double = MediaVMGanteTWD ' (MediaVMGanteTWD + MediaVMGpostTWD) / 2
'    Dim VmgMsMediana As Double = MediaVMGanteMediana ' (MediaVMGanteMediana + MediaVMGpostMediana) / 2
'    Dim VmgMsMedianaRec As Double = MediaVMGanteMedianaRec ' (MediaVMGanteMedianaRec + MediaVMGpostMedianaRec) / 2


'    Dim pLossGainTableTwdTmp As New Dictionary(Of Double, Double)
'    Dim pLossGainTableMedianaTmp As New Dictionary(Of Double, Double)
'    Dim pLossGainTableMedianaRecTmp As New Dictionary(Of Double, Double)
'    Dim A As Double = 0
'    Dim B As Double = 0
'    Dim C As Double = 0

'    'il conteggio inizia da quando inizia la pavarot
'    Dim SecFromPavarot As Integer
'    'For riga As Integer = pIDrigaInizioPavarot To System.Math.Min(pIDUltimaRigaTabella, pIDPrimaRigaTabella + pMediePavarot.MatriceMsVMGtwd.Count - 1)
'    Dim SecondiProiezioneVmgFinale As Integer = 10
'    Dim CampoVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    For riga As Integer = pIDrigaInizioPavarot To pIDUltimaRigaTabella + (SecondiProiezioneVmgFinale * dataProvider2020.RawFileHz)
'      Dim RigaRel As Integer = riga - pIDPrimaRigaTabella
'      If riga > pIDPrimaRigaTabella + pMediePavarot.MatriceMsVMGtwd.Count - 1 OrElse riga > pIDUltimaRigaTabella Then
'        Dim VmgMs As Double = KtsToMS(CampoVmg.Valori(riga))

'        A += (VmgMs - VmgMsTwd) / dataProvider2020.RawFileHz
'        ' vanno mesi i vmg relativi agli assi
'        B += (VmgMs - VmgMsMediana) / dataProvider2020.RawFileHz
'        C += (VmgMs - VmgMsMedianaRec) / dataProvider2020.RawFileHz
'      Else
'        A += (pMediePavarot.MatriceMsVMGtwd(RigaRel) - VmgMsTwd) / dataProvider2020.RawFileHz
'        B += (pMediePavarot.MatriceMsVMGmediana(RigaRel) - VmgMsMediana) / dataProvider2020.RawFileHz
'        C += (pMediePavarot.MatriceMsVMGmedianaRec(RigaRel) - VmgMsMedianaRec) / dataProvider2020.RawFileHz
'      End If
'      SecFromPavarot = CInt((riga - pIDriga) / dataProvider2020.RawFileHz)
'      If Not pLossGainProgressionTableTwd.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableTwd.Add(SecFromPavarot, A)
'      If Not pLossGainProgressionTableMediana.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableMediana.Add(SecFromPavarot, B)
'      If Not pLossGainProgressionTableMedianaRec.ContainsKey(SecFromPavarot) Then pLossGainProgressionTableMedianaRec.Add(SecFromPavarot, C)


'      If riga <= pIDrigaFinePavarot Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come pavarot terminata
'        pTotalPavarotGainLostTWD = A '+= (pMediePavarot.MatriceMetriVMGtwd(RigaRel) - ((MediaVMGanteTWD + MediaVMGpostTWD) / 2)) / dataProvider2020.RawFileHz
'        pTotalPavarotGainLostMediana = B '+= (pMediePavarot.MatriceMetriVMGmediana(RigaRel) - ((MediaVMGanteMediana + MediaVMGpostMediana) / 2)) / dataProvider2020.RawFileHz
'        pTotalPavarotGainLostMedianaRec = C '+= (pMediePavarot.MatriceMetriVMGmedianaRec(RigaRel) - ((MediaVMGanteMedianaRec + MediaVMGpostMedianaRec) / 2)) / dataProvider2020.RawFileHz

'      End If
'      If riga = System.Math.Min(pIDrigaFinePavarot, pIDUltimaRigaTabella - AvgLenghtSecs * dataProvider2020.RawFileHz) Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come pavarot terminata oppure X secondi prima della fine della tabella
'        pLossGainTableTwdTmp.Add(riga, A)
'        pLossGainTableMedianaTmp.Add(riga, B)
'        pLossGainTableMedianaRecTmp.Add(riga, C)
'      ElseIf riga = System.Math.Min(pIDrigaFineCampionamentoPostPavarot, IdUltimaRigaTabella) Then
'        'vengono memorizzati i metri guadagnato/persi nel momento individuato come campionamento pavarot terminato oppure alla fine della tabella
'        pLossGainTableTwdTmp.Add(riga, A)
'        pLossGainTableMedianaTmp.Add(riga, B)
'        pLossGainTableMedianaRecTmp.Add(riga, C)
'      End If

'      'Dim RigaDaPavarot As Integer = (riga - pIDriga) / dataProvider2020.RawFileHz
'      'Select Case SecFromPavarot
'      '  Case 0, 5, 10, 20, 30
'      '    'vengono memorizzati i metri persi/guadagnati a determinati intervalli
'      '    'viene memorizzzato solo al primo valore del secondo
'      '    If Not pLossGainTableTWD.ContainsKey(SecFromPavarot) Then pLossGainTableTWD.Add(SecFromPavarot, A)
'      '    If Not pLossGainTableMediana.ContainsKey(SecFromPavarot) Then pLossGainTableMediana.Add(SecFromPavarot, B)
'      '    If Not pLossGainTableMedianaRec.ContainsKey(SecFromPavarot) Then pLossGainTableMedianaRec.Add(SecFromPavarot, C)
'      'End Select

'    Next
'    'SecFromPavarot += 1
'    'pLossGainProgressionTableTwd.Add(SecFromPavarot, A)
'    'pLossGainProgressionTableMediana.Add(SecFromPavarot, B)
'    'pLossGainProgressionTableMedianaRec.Add(SecFromPavarot, C)

'    pLossGainTableTwdPost.Clear()
'    pLossGainTableMedianaPost.Clear()
'    pLossGainTableMedianaRecPost.Clear()

'    Dim SecFromPavarotA As Double = (pIDrigaFinePavarot - pIDriga) / dataProvider2020.RawFileHz
'    Dim SecFromPavarotB As Double = (pIDrigaFineCampionamentoPostPavarot - pIDriga) / dataProvider2020.RawFileHz
'    pLossGainTableTwdPost.Add(SecFromPavarotA, pLossGainTableTwdTmp.Valori(0))
'    pLossGainTableMedianaPost.Add(SecFromPavarotA, pLossGainTableMedianaTmp.Valori(0))
'    pLossGainTableMedianaRecPost.Add(SecFromPavarotA, pLossGainTableMedianaRecTmp.Valori(0))
'    pLossGainTableTwdPost.Add(SecFromPavarotB, pLossGainTableTwdTmp.Valori(1))
'    pLossGainTableMedianaPost.Add(SecFromPavarotB, pLossGainTableMedianaTmp.Valori(1))
'    pLossGainTableMedianaRecPost.Add(SecFromPavarotB, pLossGainTableMedianaRecTmp.Valori(1))

'    pPostPavarotSlopeTWD = (pLossGainTableTwdTmp.Valori(1) - pLossGainTableTwdTmp.Valori(0)) / (pLossGainTableTwdTmp.Keys(1) - pLossGainTableTwdTmp.Keys(0))
'    pPostPavarotSlopeMediana = (pLossGainTableMedianaTmp.Valori(1) - pLossGainTableMedianaTmp.Valori(0)) / (pLossGainTableMedianaTmp.Keys(1) - pLossGainTableMedianaTmp.Keys(0))
'    pPostPavarotSlopeMedianaRec = (pLossGainTableMedianaRecTmp.Valori(1) - pLossGainTableMedianaRecTmp.Valori(0)) / (pLossGainTableMedianaRecTmp.Keys(1) - pLossGainTableMedianaRecTmp.Keys(0))


'    ' CalcoliGeoPoints
'    Dim CG As New clsCalcoliDuePuntiGeo()
'    Dim Lat As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDriga)
'    Dim Lon As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDriga)
'    Dim PuntoRef As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'    pMatriceGeoPos.Clear()
'    pMatriceGeoPosRelToPavarotPointTWD.Clear()
'    pMatriceGeoPosRelToPavarotPointMediana.Clear()
'    pMatriceGeoPosRelToPavarotPointMedianaRec.Clear()

'    For Riga As Integer = pIDPrimaRigaTabella To pIDUltimaRigaTabella
'      ' CalcoliGeoPoints
'      Try
'        Lat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(Riga)
'        Lon = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(Riga)
'        Dim GeoPoint As New clsGeograficPosition(Lat, Lon, Not (Lat = Nothing OrElse Lon = Nothing))
'        pMatriceGeoPos.Add(GeoPoint)

'        CG.CalcolaDistanzaAndRotta(PuntoRef, GeoPoint)
'        Dim RNG As Double = CG.Distanza
'        Dim BRG As Double = CG.Rotta
'        Dim BRGoc As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TWD, BRG)

'        Dim BRGc As Double = BRGinCartesiano(BRGoc)
'        Dim X As Double = RNG * System.Math.Cos(Radians(BRGc))
'        Dim Y As Double = RNG * System.Math.Sin(Radians(BRGc))
'        Dim PuntoTraccia As New PointF(X, Y)

'        Select Case Riga
'          Case pIDriga
'            pPuntoPavarot = PuntoTraccia
'          Case pIDrigaInizioCampionamentoAntePavarot
'            pPuntoAntePavarotInizioCampionamento = PuntoTraccia
'          Case pIDrigaInizioPavarot
'            pPuntoAntePavarot = PuntoTraccia
'          Case pIDrigaFinePavarot
'            pPuntoPostPavarot = PuntoTraccia
'          Case pIDrigaFineCampionamentoPostPavarot
'            pPuntoPostPavarotFineCampionamento = PuntoTraccia
'          Case Else
'        End Select

'        pMatriceGeoPosRelToPavarotPointTWD.Add(PuntoTraccia)

'        BRGoc = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMediana, BRG)
'        BRGc = BRGinCartesiano(BRGoc)
'        X = RNG * System.Math.Cos(Radians(BRGc))
'        Y = RNG * System.Math.Sin(Radians(BRGc))
'        PuntoTraccia = New PointF(X, Y)
'        pMatriceGeoPosRelToPavarotPointMediana.Add(PuntoTraccia)

'        BRGoc = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(pMedianaRec, BRG)
'        BRGc = BRGinCartesiano(BRGoc)
'        X = RNG * System.Math.Cos(Radians(BRGc))
'        Y = RNG * System.Math.Sin(Radians(BRGc))
'        PuntoTraccia = New PointF(X, Y)
'        pMatriceGeoPosRelToPavarotPointMedianaRec.Add(PuntoTraccia)

'      Catch ex As Exception
'        Stop
'      End Try
'    Next

'    Dim SecFromInizio As Double = 20
'    pTotalPavarotGainLostTWD = 0
'    For ii As Integer = 0 To pLossGainProgressionTableTwd.Count - 1
'      If pLossGainProgressionTableTwd.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostTWD = pLossGainProgressionTableTwd.Valori(ii)
'    Next
'    pTotalPavarotGainLostMediana = 0
'    For ii As Integer = 0 To pLossGainProgressionTableMediana.Count - 1
'      If pLossGainProgressionTableMediana.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostMediana = pLossGainProgressionTableMediana.Valori(ii)
'    Next
'    pTotalPavarotGainLostMedianaRec = 0
'    For ii As Integer = 0 To pLossGainProgressionTableMedianaRec.Count - 1
'      If pLossGainProgressionTableMedianaRec.Keys(ii) > SecFromInizio Then Exit For
'      pTotalPavarotGainLostMedianaRec = pLossGainProgressionTableMedianaRec.Valori(ii)
'    Next





'    'Dim CampoVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'    'Dim mVmgAnte As New clsMediaMobile(3 * dataProvider2020.RawFileHz)
'    'Dim mVmg As New clsMediaMobile(1 * dataProvider2020.RawFileHz)
'    'Dim Tolleranza As Double = 0.01
'    'pIdRigaVmgStabile = pIDrigaFinePavarot
'    'Dim RisalitaVmg As Boolean = False
'    'For i As Integer = 0 To MaxSecsPost * dataProvider2020.RawFileHz
'    '  Dim Riga As Integer = pIDriga + i
'    '  Dim Vmg As Double = CampoVmg.Valori(Riga)
'    '  mVmgAnte.AggiornaMedia(Vmg)
'    '  mVmg.AggiornaMedia(Vmg)
'    '  If i > 2 * dataProvider2020.RawFileHz AndAlso mVmg.Valore > mVmgAnte.Valore Then
'    '    If Not RisalitaVmg AndAlso System.Math.Abs(mVmg.Valore - mVmgAnte.Valore) / mVmg.Valore <= Tolleranza Then

'    '    Else
'    '      RisalitaVmg = True
'    '    End If
'    '    If RisalitaVmg AndAlso System.Math.Abs(mVmg.Valore - mVmgAnte.Valore) / mVmg.Valore <= Tolleranza Then
'    '      pIdRigaVmgStabile = Riga
'    '      SecFromInizio = i / dataProvider2020.RawFileHz
'    '      pTotalPavarotGainLostTWD = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableTwd.Count - 1
'    '        If pLossGainProgressionTableTwd.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostTWD = pLossGainProgressionTableTwd.Valori(ii)
'    '      Next
'    '      pTotalPavarotGainLostMediana = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableMediana.Count - 1
'    '        If pLossGainProgressionTableMediana.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostMediana = pLossGainProgressionTableMediana.Valori(ii)
'    '      Next
'    '      pTotalPavarotGainLostMedianaRec = 0
'    '      For ii As Integer = 0 To pLossGainProgressionTableMedianaRec.Count - 1
'    '        If pLossGainProgressionTableMedianaRec.Keys(ii) > SecFromInizio Then Exit For
'    '        pTotalPavarotGainLostMedianaRec = pLossGainProgressionTableMedianaRec.Valori(ii)
'    '      Next
'    '      Exit For
'    '    End If
'    '  End If
'    'Next


'    AggiornaDescrizione()
'    pPeriodo.IsCheckedSenzaEvento = pIsValid
'    Return True
'  End Function


'  Private Function MadianToAvenAntePost(MedieAnte As clsMediePeriodo, MediePost As clsMediePeriodo) As Double
'    Dim Xa As Double = MedieAnte.SpeedMedia * System.Math.Cos(Radians(MedieAnte.RottaMedia))
'    Dim Ya As Double = MedieAnte.SpeedMedia * System.Math.Sin(Radians(MedieAnte.RottaMedia))
'    Dim Xb As Double = MediePost.SpeedMedia * System.Math.Cos(Radians(MediePost.RottaMedia))
'    Dim Yb As Double = MediePost.SpeedMedia * System.Math.Sin(Radians(MediePost.RottaMedia))

'    Dim CoeffAng As Double = (Yb - Ya) / (Xb - Xa)
'    Dim aTG As Double = System.Math.Atan(CoeffAng)
'    Dim Degr As Double = Degrees(aTG)
'    Degr = SommaAngolo180adAngolo360(90, Degr)
'    If DifferenzaAssolutaTraAngoli360(Degr, TWD, True) > 45 Then
'      Degr = SommaAngolo180adAngolo360(180, Degr)
'    End If
'    If Degr < 0 Then Degr += 360
'    If Degr > 360 Then Degr -= 360
'    Return Degr
'  End Function


'  Private Function CalcolaLostGainMetodoPortals() As Double
'    ' Questo approccio si basa sul confronto tra la variazione di posizione tra i due punti noti ante e post pavarot e la posizione teorica determinata per lo stesso reale tempo trascorso eliminando rotazione ed accelerazione/decelerazione.
'    ' il punto di pavarot é dunque indivituato da quello di intersezione tra le rette del moto ante e post.
'    Dim LatTmp As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDriga)
'    Dim LonTmp As Double = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDriga)
'    Dim PuntoGeoRef As New clsGeograficPosition(LatTmp, LonTmp)

'    LatTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDrigaInizioCampionamentoAntePavarot)
'    LonTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDrigaInizioCampionamentoAntePavarot)
'    Dim PuntoGeoAnteA As New clsGeograficPosition(LatTmp, LonTmp)
'    LatTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDrigaInizioPavarot)
'    LonTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDrigaInizioPavarot)
'    Dim PuntoGeoAnteB As New clsGeograficPosition(LatTmp, LonTmp)

'    LatTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDrigaFinePavarot)
'    LonTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDrigaFinePavarot)
'    Dim PuntoGeoPostA As New clsGeograficPosition(LatTmp, LonTmp)
'    LatTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).Valori(pIDrigaFineCampionamentoPostPavarot)
'    LonTmp = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).Valori(pIDrigaFineCampionamentoPostPavarot)
'    Dim PuntoGeoPostB As New clsGeograficPosition(LatTmp, LonTmp)

'    Dim SegmentoAnte As New clsCourseLeg(New clsCourseMark(PuntoGeoAnteA, "Ante", 0), New clsCourseMark(PuntoGeoAnteB, "Post", 0))
'    Dim SegmentoPost As New clsCourseLeg(New clsCourseMark(PuntoGeoPostA, "Post", 0), New clsCourseMark(PuntoGeoPostB, "Ante", 0))

'    Dim AntePavarot As New clsRettaPerDuePunti(PuntoGeoAnteA, PuntoGeoAnteB, PuntoGeoRef)
'    Dim PostPavarot As New clsRettaPerDuePunti(PuntoGeoPostA, PuntoGeoPostB, PuntoGeoRef)



'    pPuntoIntersezione = CalcolaPuntoIntersezione(AntePavarot, PostPavarot, PuntoGeoRef)

'    SegmentoAnte = New clsCourseLeg(New clsCourseMark(PuntoGeoAnteB, "Ante", 0), New clsCourseMark(PuntoIntersezione, "Intersection", 0))
'    SegmentoPost = New clsCourseLeg(New clsCourseMark(PuntoIntersezione, "Intersezione", 0), New clsCourseMark(PuntoGeoPostA, "Post", 0))

'    pSecondiAnte = SegmentoAnte.LegRangeMeters / pMedieAnte.SpeedMediaMS

'    Dim pTimeRangePavarot As New clsTimeRange(dataProvider2020.TimeStamps(pIDrigaInizioPavarot), dataProvider2020.TimeStamps(pIDrigaFinePavarot))
'    Dim DurataPavarotSS As Double = pTimeRangePavarot.Durata.TotalMilliseconds / 1000

'    If DifferenzaAssolutaTraAngoli360(SegmentoAnte.LegBearingTrue, pMedieAnte.RottaMedia, True) < 90 Then
'      pSecondiPost = DurataPavarotSS - pSecondiAnte
'    Else
'      pSecondiPost = DurataPavarotSS + pSecondiAnte
'    End If

'    Dim DistanzaPostAtPostSpeed As Double = pMediePost.SpeedMediaMS * pSecondiPost

'    Dim Segno As Integer = -1

'    If DifferenzaAssolutaTraAngoli360(SegmentoPost.LegBearingTrue, pMediePost.RottaMedia, True) > 90 Then
'      Segno = 1
'    End If

'    ' differenza tra la distanza percorsa dal punto di intersezione al punto di fine pavarot e la distanza che si sarebbe percorsa nel tempo della pavarot meno il tempo prima del punto di intersezione alla velocitá post pavarot!
'    Return Segno * (DistanzaPostAtPostSpeed - SegmentoPost.LegRangeMeters)

'  End Function


'  Private Function CalcolaPuntoIntersezione(SegmentoAnte As clsRettaPerDuePunti, SegmentoPost As clsRettaPerDuePunti, PuntoGeoRef As clsGeograficPosition) As clsGeograficPosition
'    Dim pLatCart As Double
'    ' Y = A1*X + B1 ' LinePartenza
'    ' Y = A2*X + B2 ' BarcaBoa
'    ' A1*X + B1 = A2*X + B2 --> X=(B1-B2)/(A2-A1)
'    Dim pLngCart As Double = (SegmentoAnte.OrdOrig - SegmentoPost.OrdOrig) / (SegmentoPost.CoeffAng - SegmentoAnte.CoeffAng)
'    pLatCart = SegmentoAnte.CoeffAng * pLngCart + SegmentoAnte.OrdOrig
'    pLatCart = SegmentoPost.CoeffAng * pLngCart + SegmentoPost.OrdOrig
'    Dim pPuntoTmp As New clsCartesianRelativePosition(pLatCart, pLngCart, PuntoGeoRef)
'    Return pPuntoTmp.PuntoGeo

'    ''Exit Sub
'    'pBoatToIntersectionPoint.MarkTo.MarkPosition = pIntersectionPoint
'    'pBoatToIntersectionPoint.AggiornaRngBrg()
'    'pI_BoatToLineIntersection.AggiornaLegInfo(True)

'  End Function

'  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

'  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
'    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
'  End Sub


'End Class

