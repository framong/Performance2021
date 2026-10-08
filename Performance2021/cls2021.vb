Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Net.Security
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Xml.Schema
Imports Ab3d.Common.Cameras
Imports BruTile.Wmts.Generated
Imports CsvHelper
Imports GeoTimeZone
Imports MathNet.Numerics.Statistics
Imports Newtonsoft.Json
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.Database
Imports OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime
Imports PropertyChanged
Imports SciChart.Charting.Model.ChartSeries
Imports SciChart.Charting.Model.DataSeries
Imports SciChart.Charting.Visuals.PointMarkers
Imports SciChart.Charting.Visuals.RenderableSeries
Imports Thrift.Protocol
Imports TimeZoneConverter
'Imports License


<AddINotifyPropertyChangedInterface>
Public Class clsEzriz

  Public Property HdwID As String


  Public Sub New()
    HdwID = GetHdwId()
  End Sub



  'Public Shared Function GetExpirationPeriod() As String
  '    Return Now.AddDays(License.Status.Evaluation_Time).AddDays(-License.Status.Evaluation_Time_Current).ToString("dd MMM yyyy")
  'End Function

  Public Shared Function GetExpirationDate() As String
    Dim ET = Now.AddDays(License.Status.Evaluation_Time).AddDays(-License.Status.Evaluation_Time_Current)
    Dim ED = License.Status.Expiration_Date
    If (ET < ED) Then
      Return ET.ToString("dd MMM yyyy")
    Else
      Return ED.ToString("dd MMM yyyy")
    End If
  End Function

  ' L'hardware ID interroga WMI (CPU, disco, MAC): e' l'operazione piu' lenta dell'avvio
  ' e veniva calcolata piu' volte. Il valore non cambia durante l'esecuzione.
  Private Shared _HdwIdCache As String = Nothing

  Public Shared Function GetHdwId() As String
    If _HdwIdCache Is Nothing Then
      _HdwIdCache = License.Status.GetHardwareID(True, True, True, False)
    End If
    Return _HdwIdCache
    'Return License.Status.License_HardwareID
    'Return License.Status.HardwareID
  End Function

  Public Shared Function ValidLicense() As Boolean
    Dim dbg As Boolean = My.Computer.Keyboard.ShiftKeyDown
    dbg = dbg OrElse System.IO.File.Exists(System.IO.Path.Combine(My.Application.Info.DirectoryPath, "dbg"))
    If (dbg) Then MsgBox("DBG Mode, you should now release the Shift key")
    'If (dbg) Then MsgBox("Pre License Check")
    If ValidLicense(dbg) Then
      If (dbg) Then MsgBox("The License is valid and active")
      Return True
    Else
      Clipboard.SetText(GetHdwId)
      MsgBox("This Software needs a valid license to run, please use this code: " & GetHdwId() & " to request a valid one." & vbCrLf & "Screenshots will be ignored, please send the code as text" & vbCrLf & vbCrLf & "OK to close and copy it into the Clipboard", vbOKOnly)
      Return False
    End If
  End Function

  Public Shared Property ExpirationDate As DateTime = Nothing



  Public Shared Function ValidLicense(bdg As Boolean) As Boolean
    'GetNTP()

    Dim hdwid = GetHdwId()
    'If (bdg) Then MsgBox("DBG Checking License")
    If (bdg) Then MsgBox("DBG: Computer Hardware ID " & hdwid)
    If hdwid = "####-####-####-####-####" Then
      If (bdg) Then MsgBox("DBG: License File Not Needed")
      Return True
    Else
      Dim licPath As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "SP2021.license")
      'licPath = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "test.license")
      If (System.IO.File.Exists(licPath)) Then
        If (bdg) Then MsgBox("Valid Computer Hardware ID and License File Found")
        License.Status.LoadLicense(licPath)
        Dim l = License.Status.License
        If l.Count > 0 Then
          If (bdg) Then MsgBox("License: " & licPath & vbCrLf & "License file matching the application requirements")
          If License.Status.License_HardwareID = hdwid Then
            Dim Expired As Boolean = True
            If License.Status.Evaluation_Lock_Enabled Then
              If (bdg) Then MsgBox("DBG: Evaluation Period Enabled" & vbCrLf & "Evaluation Time: " & License.Status.Evaluation_Time & vbCrLf & "Elapsed: " & License.Status.Evaluation_Time_Current)
              Expired = License.Status.Evaluation_Time < License.Status.Evaluation_Time_Current
            Else
              Expired = False
            End If
            If License.Status.Expiration_Date_Lock_Enable Then
              If (bdg) Then MsgBox("DBG: Expiration Date Enabled" & vbCrLf & "Expiration Date: " & License.Status.Expiration_Date)
              Expired = Expired OrElse License.Status.Expiration_Date < CurrentToday()
              ExpirationDate = License.Status.Expiration_Date.AddDays(1)
            End If
            If Expired Then
              If (bdg) Then MsgBox("DBG: License Expired")
              Return False
            Else
              Return True
            End If
          Else
            If (bdg) Then MsgBox("DBG: Computer Hardware ID not matching License Hardware ID")
            Return False
          End If
        Else
          If (bdg) Then MsgBox("License: " & licPath & vbCrLf & "License file not matching the application requirements")
          Return False
        End If
      Else
        If (bdg) Then MsgBox("DBG License file not found")
        Return False
      End If
    End If
  End Function


  ' Il risultato non cambia durante l'esecuzione: si calcola una volta sola.
  Private Shared _CurrentTodayCache As Date? = Nothing

  Shared Function CurrentToday() As DateTime

    If _CurrentTodayCache.HasValue Then Return _CurrentTodayCache.Value

    Dim d As New Dictionary(Of Date, Integer)

    ' NOTA: il codice precedente scandiva questa stessa cartella DUE volte (la seconda
    ' variabile puntava a Prefetch ma veniva poi riusata la prima). Ogni conteggio
    ' risultava semplicemente raddoppiato, quindi il massimo - e il risultato della
    ' funzione - restano identici eliminando la scansione duplicata.
    Try
      Dim o As New DirectoryInfo("C:\Windows\System32\winevt\Logs\")
      Dim Oggi As DateTime = DateTime.Today
      ' EnumerateFiles non materializza l'intero elenco prima di filtrare
      For Each f In o.EnumerateFiles()
        If f.LastWriteTime >= Oggi Then
          Dim k As Date = f.LastWriteTime.Date
          If d.ContainsKey(k) Then
            d(k) = d(k) + 1
          Else
            d.Add(k, 1)
          End If
        End If
      Next
    Catch ex As Exception
      ' cartella non accessibile: si ricade sulla data di sistema
    End Try

    If d.Count > 0 Then
      Dim cd As Date = DateTime.Today
      Dim m As Integer = 0
      For Each df In d
        If df.Value > m Then
          cd = df.Key
          m = df.Value
        End If
      Next
      _CurrentTodayCache = cd.AddDays(1)
    Else
      _CurrentTodayCache = DateTime.Today.AddDays(1)
    End If

    Return _CurrentTodayCache.Value

  End Function


End Class



<AddINotifyPropertyChangedInterface>
Public Class clsChStats
  Public Tws As New clsArrayStats(False)
  Public Twd As New clsArrayStats(True)
  Public Bs As New clsArrayStats(False)
  Public RH As New clsArrayStats(False)
  Public ECCTest As New clsArrayStats(False)
  Public Vmg As New clsArrayStats(False)

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsQualityChannelSetting

  ''' <summary>Canale chiave a cui si riferisce il settaggio. Non modificabile dall'utente.</summary>
  Public Property CanaleChiave As clsChannels2020.eCanaliChiave

  ''' <summary>Indice di qualita' a cui il canale contribuisce.</summary>
  Public Property Indice As eIndiceQualita

  ''' <summary>True se la deviazione va rapportata alla media del canale invece che alle sue unita'.</summary>
  Public Property UsePercent As Boolean = False

  ''' <summary>
  ''' True se la variazione tollerata si allarga con lo stato del mare.
  ''' Fa parte della struttura dell'algoritmo definita in clsQualitySettings.CanaliStandard.
  ''' </summary>
  Public Property SeaDriven As Boolean = False

  ''' <summary>
  ''' Esponente con cui il canale reagisce al mare: piu' e' basso, meno la soglia si allarga.
  ''' TrimNorm, HeelNorm e BS usano 1.25, il TWA molto meno perche' deve restare significativo
  ''' anche con onda. Struttura dell'algoritmo, non esposto all'utente.
  ''' </summary>
  Public Property SeaExp As Double = 1.25

  ''' <summary>Variazione (SD) tollerata con mare calmo, per ciascuna andatura.</summary>
  Public Property DeltaEquivalenteUpwind As Double = 2
  Public Property DeltaEquivalenteReaching As Double = 2
  Public Property DeltaEquivalenteDownwind As Double = 2

  <JsonIgnore>
  Public ReadOnly Property NomeCanale As String
    Get
      Return CanaleChiave.ToString.TrimStart("e"c)
    End Get
  End Property

  ''' <summary>Descrizione sintetica di come va letto il delta di questo canale.</summary>
  <JsonIgnore>
  Public ReadOnly Property Note As String
    Get
      Dim n As String = "degrees"
      If UsePercent Then n = "percent"
      If SeaDriven Then n &= ", sea driven"
      Return n
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DescrizioneIndice As String
    Get
      If Indice = eIndiceQualita.eEnvironment Then Return "Environment"
      Return "Attitude"
    End Get
  End Property

  Public Enum eIndiceQualita
    eEnvironment = 0
    eAttitude = 1
  End Enum

  Public Sub New()
    ' richiesto dalla deserializzazione Json
  End Sub

  Public Sub New(canaleChiave As clsChannels2020.eCanaliChiave, indice As eIndiceQualita,
                 dUp As Double, dReach As Double, dDown As Double,
                 usePercent As Boolean, seaDriven As Boolean, seaExp As Double)
    Me.CanaleChiave = canaleChiave
    Me.Indice = indice
    Me.DeltaEquivalenteUpwind = dUp
    Me.DeltaEquivalenteReaching = dReach
    Me.DeltaEquivalenteDownwind = dDown
    Me.UsePercent = usePercent
    Me.SeaDriven = seaDriven
    Me.SeaExp = seaExp
  End Sub

  ''' <summary>Copia nel settaggio corrente i soli valori tarabili dall'utente.</summary>
  Public Sub CopiaValoriTarabiliDa(Altro As clsQualityChannelSetting)
    If Altro Is Nothing Then Exit Sub
    If Altro.DeltaEquivalenteUpwind > 0 Then DeltaEquivalenteUpwind = Altro.DeltaEquivalenteUpwind
    If Altro.DeltaEquivalenteReaching > 0 Then DeltaEquivalenteReaching = Altro.DeltaEquivalenteReaching
    If Altro.DeltaEquivalenteDownwind > 0 Then DeltaEquivalenteDownwind = Altro.DeltaEquivalenteDownwind
  End Sub

  ''' <summary>Costruisce l'oggetto di calcolo runtime. Nothing se il canale non e' nel file caricato.</summary>
  Public Function CreaRuntime(Canale As clsChannel2020, Samples As Integer, SeaRef As Double) As clsChDataQuality
    If Canale Is Nothing Then Return Nothing
    Return New clsChDataQuality(Me, Canale, Samples, SeaRef)
  End Function

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsChDataQuality

  ''' <summary>Configurazione da cui derivano soglie e pesi.</summary>
  Public Property Setting As clsQualityChannelSetting
  Public Property Channel As clsChannel2020

  ''' <summary>SeaState al quale la variazione tollerata e' esattamente il delta impostato.</summary>
  Public Property SeaRef As Double = 0.3

  ''' <summary>Buffer circolare della finestra mobile.</summary>
  Public Property Values As Double()
  ''' <summary>True per gli slot del buffer gia' riempiti con dati reali.</summary>
  Public Property Riempito As Boolean()

  Public Sub New(setting As clsQualityChannelSetting, channel As clsChannel2020, samples As Integer, seaRef As Double)
    Me.Setting = setting
    Me.Channel = channel
    Me.SeaRef = seaRef
    ReDim _Values(samples - 1)
    ReDim _Riempito(samples - 1)
  End Sub

  <JsonIgnore>
  Public ReadOnly Property Indice As clsQualityChannelSetting.eIndiceQualita
    Get
      Return Setting.Indice
    End Get
  End Property

  ''' <summary>Inserisce un campione nella finestra mobile.</summary>
  Public Sub Accumula(Posizione As Integer, Valore As Double)
    Values(Posizione) = Valore
    Riempito(Posizione) = True
  End Sub

  ''' <summary>
  ''' Deviazione standard campionaria sui soli slot riempiti, senza allocare array: stessa ricorrenza usata da
  ''' MathNet (ArrayStatistics.Variance) sugli stessi valori, nello stesso ordine. Chiamata ~5 milioni di volte
  ''' nel calcolo della qualita': la versione con array temporaneo + MathNet costava circa 1,8 s.
  ''' </summary>
  Public Function SD() As Double
    Dim n As Integer = 0
    Dim t As Double = 0
    Dim Varianza As Double = 0
    Dim Vals As Double() = Values
    Dim Pieni As Boolean() = Riempito
    For i As Integer = 0 To Pieni.Length - 1
      If Pieni(i) Then
        Dim x As Double = Vals(i)
        n += 1
        t += x
        If n > 1 Then
          Dim diff As Double = (n * x) - t
          Varianza += (diff * diff) / (n * (n - 1.0))
        End If
      End If
    Next
    If n < 2 Then Return Double.NaN
    Return Math.Abs(Math.Sqrt(Varianza / (n - 1)))
  End Function

  ''' <summary>Versione precedente (array temporaneo + MathNet): serve solo alla verifica a campione dell'equivalenza.</summary>
  Public Function SDClassica() As Double
    Dim n As Integer = 0
    For i As Integer = 0 To Riempito.Count - 1
      If Riempito(i) Then n += 1
    Next
    If n < 2 Then Return Double.NaN

    Dim v(n - 1) As Double
    Dim j As Integer = 0
    For i As Integer = 0 To Riempito.Count - 1
      If Riempito(i) Then
        v(j) = Values(i)
        j += 1
      End If
    Next
    Return Math.Abs(MathNet.Numerics.Statistics.Statistics.StandardDeviation(v))
  End Function

  Public Function Avg() As Double
    Dim n As Integer = 0
    Dim tot As Double = 0
    For i As Integer = 0 To Riempito.Count - 1
      If Riempito(i) Then
        tot += Values(i)
        n += 1
      End If
    Next
    If n = 0 Then Return 0
    Return Math.Abs(tot / n)
  End Function

  ''' <summary>
  ''' Variazione tollerata per il campione corrente.
  ''' Per i canali legati al mare il delta e' quello valido con mare calmo e viene moltiplicato
  ''' quando il SeaState supera il riferimento: Delta * (SeaState / SeaRef) ^ SeaExp.
  ''' Il moltiplicatore non scende mai sotto 1, quindi il delta e' sempre il minimo tollerato.
  ''' </summary>
  Public Function Soglia(Andatura As eAndatura, SeaState As Double) As Double
    Dim Base As Double = DeltaAndatura(Andatura)
    If Not Setting.SeaDriven Then Return Base
    If SeaRef <= 0 Then Return Base
    If Double.IsNaN(SeaState) OrElse SeaState <= 0 Then Return Base
    Return Base * Math.Max(1, Math.Pow(SeaState / SeaRef, Setting.SeaExp))
  End Function

  ''' <summary>
  ''' Tolleranza consumata dal canale, in percentuale: 100 significa che la variazione osservata
  ''' e' esattamente quella tollerata. NaN se la finestra non ha ancora dati sufficienti.
  ''' </summary>
  ' verifica a campione (1 chiamata su 16384) della SD veloce contro quella classica, riportata nel log dei tempi
  Public Shared VerificaSdChiamate As Long = 0
  Public Shared VerificaSdControlli As Long = 0
  Public Shared VerificaSdDiffMax As Double = 0

  Public Function TolleranzaUsata(Andatura As eAndatura, SeaState As Double) As Double
    Dim s As Double = SD()
    VerificaSdChiamate += 1
    If (VerificaSdChiamate And 16383) = 0 Then
      Dim sc As Double = SDClassica()
      If Not Double.IsNaN(s) AndAlso Not Double.IsNaN(sc) Then
        VerificaSdControlli += 1
        VerificaSdDiffMax = Math.Max(VerificaSdDiffMax, Math.Abs(s - sc))
      ElseIf Double.IsNaN(s) <> Double.IsNaN(sc) Then
        VerificaSdDiffMax = Double.PositiveInfinity ' una e' NaN e l'altra no: non equivalenti
      End If
    End If
    If Double.IsNaN(s) Then Return Double.NaN

    If Setting.UsePercent Then
      Dim a As Double = Avg()
      If a = 0 Then Return Double.NaN
      s = s / a * 100
    End If

    Dim t As Double = Soglia(Andatura, SeaState)
    If t <= 0 Then Return Double.NaN
    Return s / t * 100
  End Function

  Private Function DeltaAndatura(Andatura As eAndatura) As Double
    Select Case Andatura
      Case eAndatura.eUpwind
        Return Setting.DeltaEquivalenteUpwind
      Case eAndatura.eReaching
        Return Setting.DeltaEquivalenteReaching
      Case Else
        Return Setting.DeltaEquivalenteDownwind
    End Select
  End Function

  Public Enum eAndatura
    eUpwind = 0
    eReaching = 1
    eDownwind = 2
  End Enum

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsQualitySettings

#Region "Parametri generali"
  ''' <summary>Ampiezza in secondi della finestra mobile su cui si calcolano le deviazioni standard.</summary>
  Public Property SecondiSampling As Integer = 15

  ''' <summary>
  ''' SeaState al quale i canali legati al mare tollerano
  ''' esattamente il loro delta. Sopra questo valore la tolleranza si allarga.
  ''' </summary>
  Public Property SeaRef As Double = 0.3
#End Region

#Region "Soglie di tolleranza usata (%)"
  ' 100 = il canale medio ha oscillato esattamente quanto gli era concesso dal suo delta.
  ' Soglie crescenti: vince il voto piu' alto la cui soglia non e' superata.
  Public Property MaxTollEnvQ5 As Double = 50
  Public Property MaxTollEnvQ4 As Double = 75
  Public Property MaxTollEnvQ3 As Double = 100
  Public Property MaxTollEnvQ2 As Double = 150
  Public Property MaxTollEnvQ1 As Double = 200

  Public Property MaxTollAttQ5 As Double = 50
  Public Property MaxTollAttQ4 As Double = 75
  Public Property MaxTollAttQ3 As Double = 100
  Public Property MaxTollAttQ2 As Double = 150
  Public Property MaxTollAttQ1 As Double = 200
#End Region

#Region "Performance"
  ''' <summary>Semiampiezza in minuti dell'intorno su cui si calcola il rendimento di riferimento.</summary>
  Public Property RefWindowMinutes As Double = 12
  ''' <summary>Voto minimo di Environment perche' un campione entri nel riferimento.</summary>
  Public Property MinEnvQPerRiferimento As Integer = 3
  ''' <summary>Voto minimo di Attitude perche' un campione entri nel riferimento.</summary>
  Public Property MinAttQPerRiferimento As Integer = 3

  ' Scostamento ASSOLUTO di BSp dal riferimento locale, in punti percentuali di target.
  ' Anche andare molto piu' forte del rendimento tipico e' un'anomalia: puo' voler dire
  ' raffica, onda favorevole o dato sporco, non necessariamente una prestazione reale.
  ' Soglie crescenti: vince il voto piu' alto il cui limite non e' superato.
  Public Property MaxAbsDeltaPerf5 As Double = 1
  Public Property MaxAbsDeltaPerf4 As Double = 2
  Public Property MaxAbsDeltaPerf3 As Double = 4
  Public Property MaxAbsDeltaPerf2 As Double = 10
  Public Property MaxAbsDeltaPerf1 As Double = 20
#End Region

#Region "Canali"
  ''' <summary>
  ''' Valori tarabili dei canali. La struttura (quali canali, a quale indice appartengono,
  ''' se sono percentuali o legati al mare) e' fissata nel codice da CanaliStandard:
  ''' dal profilo si recuperano solo i tre delta.
  ''' </summary>
  Public Property ChannelSettings As New ObservableCollection(Of clsQualityChannelSetting)
#End Region

  Public Sub New()
    ' richiesto dalla deserializzazione Json
  End Sub

  ''' <summary>
  ''' Definizione di fabbrica dei canali. E' la struttura dell'algoritmo, non una preferenza:
  ''' cambiare quali canali entrano in quale indice significa cambiare cosa l'indice misura,
  ''' percio' non e' esposta all'utente ne' salvata nel profilo.
  ''' </summary>
  Public Shared Function CanaliStandard() As List(Of clsQualityChannelSetting)
    Dim L As New List(Of clsQualityChannelSetting)
    Dim eEnv = clsQualityChannelSetting.eIndiceQualita.eEnvironment
    Dim eAtt = clsQualityChannelSetting.eIndiceQualita.eAttitude

    ' --- Environment: stabilita' della condizione di vento ---
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eTWS, eEnv, 3, 3, 2, True, False, 1.25))
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eTWD, eEnv, 3, 3, 3, False, False, 1.25))
    ' il TWA oscilla anche per effetto del beccheggio: tollera il mare, ma con una curva molto
    ' piu' piatta degli altri canali marini (3 gradi a SeaState 0.5, 10 gradi a SeaState 7)
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eTWA, eEnv, 2.4, 2.4, 2.8, False, True, 0.46))

    ' --- Attitude: stabilita' della condotta ---
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eHDG, eAtt, 1.5, 1.5, 2, False, False, 1.25))
    ' accelerazioni e decelerazioni: con onda sono in buona parte imposte dal mare
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eSOW, eAtt, 2, 2, 2, True, True, 1.25))
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eHeelNorm, eAtt, 2, 2, 2, False, True, 1.25))
    L.Add(New clsQualityChannelSetting(clsChannels2020.eCanaliChiave.eTrimNorm, eAtt, 0.1, 0.1, 0.1, False, True, 1.25))

    Return L
  End Function

  ''' <summary>Imposta l'intera configurazione ai valori di fabbrica.</summary>
  Public Sub CaricaValoriDefault()
    SecondiSampling = 15
    SeaRef = 0.3

    MaxTollEnvQ5 = 50 : MaxTollEnvQ4 = 75 : MaxTollEnvQ3 = 100 : MaxTollEnvQ2 = 150 : MaxTollEnvQ1 = 200
    MaxTollAttQ5 = 50 : MaxTollAttQ4 = 75 : MaxTollAttQ3 = 100 : MaxTollAttQ2 = 150 : MaxTollAttQ1 = 200

    RefWindowMinutes = 12
    MinEnvQPerRiferimento = 3
    MinAttQPerRiferimento = 3
    MaxAbsDeltaPerf5 = 1 : MaxAbsDeltaPerf4 = 2 : MaxAbsDeltaPerf3 = 4 : MaxAbsDeltaPerf2 = 10 : MaxAbsDeltaPerf1 = 20

    CaricaCanaliDefault()
  End Sub

  ''' <summary>Ripristina i soli valori tarabili dei canali.</summary>
  Public Sub CaricaCanaliDefault()
    ChannelSettings = New ObservableCollection(Of clsQualityChannelSetting)
    For Each c As clsQualityChannelSetting In CanaliStandard()
      ChannelSettings.Add(c)
    Next
  End Sub

  ''' <summary>
  ''' Riallinea la lista alla struttura di fabbrica conservando i delta tarati dall'utente,
  ''' e riempie i parametri lasciati vuoti da profili salvati con versioni precedenti.
  ''' </summary>
  Public Sub NormalizzaValoriMancanti()
    If SecondiSampling <= 0 Then SecondiSampling = 15
    If SeaRef <= 0 Then SeaRef = 0.3

    If MaxTollEnvQ5 <= 0 Then MaxTollEnvQ5 = 50
    If MaxTollEnvQ4 <= 0 Then MaxTollEnvQ4 = 75
    If MaxTollEnvQ3 <= 0 Then MaxTollEnvQ3 = 100
    If MaxTollEnvQ2 <= 0 Then MaxTollEnvQ2 = 150
    If MaxTollEnvQ1 <= 0 Then MaxTollEnvQ1 = 200

    If MaxTollAttQ5 <= 0 Then MaxTollAttQ5 = 50
    If MaxTollAttQ4 <= 0 Then MaxTollAttQ4 = 75
    If MaxTollAttQ3 <= 0 Then MaxTollAttQ3 = 100
    If MaxTollAttQ2 <= 0 Then MaxTollAttQ2 = 150
    If MaxTollAttQ1 <= 0 Then MaxTollAttQ1 = 200

    If MaxAbsDeltaPerf5 <= 0 Then MaxAbsDeltaPerf5 = 1
    If MaxAbsDeltaPerf4 <= 0 Then MaxAbsDeltaPerf4 = 2
    If MaxAbsDeltaPerf3 <= 0 Then MaxAbsDeltaPerf3 = 4
    If MaxAbsDeltaPerf2 <= 0 Then MaxAbsDeltaPerf2 = 10
    If MaxAbsDeltaPerf1 <= 0 Then MaxAbsDeltaPerf1 = 20

    If RefWindowMinutes <= 0 Then RefWindowMinutes = 12
    If MinEnvQPerRiferimento < 0 OrElse MinEnvQPerRiferimento > 5 Then MinEnvQPerRiferimento = 3
    If MinAttQPerRiferimento < 0 OrElse MinAttQPerRiferimento > 5 Then MinAttQPerRiferimento = 3

    ' la struttura viene sempre da CanaliStandard: dal profilo si recuperano solo i delta
    Dim Salvati As ObservableCollection(Of clsQualityChannelSetting) = ChannelSettings
    ChannelSettings = New ObservableCollection(Of clsQualityChannelSetting)
    For Each c As clsQualityChannelSetting In CanaliStandard()
      If Not Salvati Is Nothing Then
        Dim v As clsQualityChannelSetting = Salvati.FirstOrDefault(Function(x) x.CanaleChiave = c.CanaleChiave)
        c.CopiaValoriTarabiliDa(v)
      End If
      ChannelSettings.Add(c)
    Next
  End Sub

  ''' <summary>
  ''' Costruisce la lista di calcolo agganciando ogni settaggio al canale reale del file caricato.
  ''' I canali assenti dal file vengono saltati: la media si fa sui presenti.
  ''' </summary>
  Public Function CostruisciListaRuntime(Samples As Integer, RisolviCanale As Func(Of clsChannels2020.eCanaliChiave, clsChannel2020)) As List(Of clsChDataQuality)
    Dim Risultato As New List(Of clsChDataQuality)
    If ChannelSettings Is Nothing Then Return Risultato
    For Each s As clsQualityChannelSetting In ChannelSettings
      Dim r As clsChDataQuality = s.CreaRuntime(RisolviCanale(s.CanaleChiave), Samples, SeaRef)
      If Not r Is Nothing Then Risultato.Add(r)
    Next
    Return Risultato
  End Function

  ''' <summary>Voto 0-5 di Environment a partire dalla tolleranza media usata, in percentuale.</summary>
  Public Function VotoEnvironment(TolleranzaUsata As Double) As Double
    If Double.IsNaN(TolleranzaUsata) Then Return Double.NaN
    If TolleranzaUsata <= MaxTollEnvQ5 Then Return 5
    If TolleranzaUsata <= MaxTollEnvQ4 Then Return 4
    If TolleranzaUsata <= MaxTollEnvQ3 Then Return 3
    If TolleranzaUsata <= MaxTollEnvQ2 Then Return 2
    If TolleranzaUsata <= MaxTollEnvQ1 Then Return 1
    Return 0
  End Function

  ''' <summary>Voto 0-5 di Attitude a partire dalla tolleranza media usata, in percentuale.</summary>
  Public Function VotoAttitude(TolleranzaUsata As Double) As Double
    If Double.IsNaN(TolleranzaUsata) Then Return Double.NaN
    If TolleranzaUsata <= MaxTollAttQ5 Then Return 5
    If TolleranzaUsata <= MaxTollAttQ4 Then Return 4
    If TolleranzaUsata <= MaxTollAttQ3 Then Return 3
    If TolleranzaUsata <= MaxTollAttQ2 Then Return 2
    If TolleranzaUsata <= MaxTollAttQ1 Then Return 1
    Return 0
  End Function

  ''' <summary>
  ''' Voto 0-5 di performance a partire dallo scostamento dal riferimento locale, in punti di
  ''' target. Conta il valore assoluto: allontanarsi dal rendimento tipico e' un'anomalia in
  ''' entrambe le direzioni.
  ''' </summary>
  Public Function VotoPerformance(DeltaPunti As Double) As Double
    If Double.IsNaN(DeltaPunti) Then Return Double.NaN
    Dim d As Double = Math.Abs(DeltaPunti)
    If d <= MaxAbsDeltaPerf5 Then Return 5
    If d <= MaxAbsDeltaPerf4 Then Return 4
    If d <= MaxAbsDeltaPerf3 Then Return 3
    If d <= MaxAbsDeltaPerf2 Then Return 2
    If d <= MaxAbsDeltaPerf1 Then Return 1
    Return 0
  End Function

  ''' <summary>Copia profonda, usata dalla finestra di setup per poter annullare le modifiche.</summary>
  Public Function Clona() As clsQualitySettings
    Return JsonConvert.DeserializeObject(Of clsQualitySettings)(JsonConvert.SerializeObject(Me))
  End Function

End Class




<AddINotifyPropertyChangedInterface>
Public Class clsBasicChartSettings
  Public Property YaxisChannels As List(Of String)
  Public Property LineType As clsGroupLines.eLineType
  Public Property CommonYAxis As Boolean
  Public Property ShowPortStbdBackground As Boolean
  Public Property ShowTargetIfAvailable As Boolean
  Public Property YMinEnabled As Boolean = False ' se attivo il minimo dell'asse Y e' vincolato a YMin dopo gli zoom automatici
  Public Property YMin As Double = 0
  Public Property YMaxEnabled As Boolean = False ' idem per il massimo
  Public Property YMax As Double = 0

  Public Sub New()

  End Sub

  Public Sub New(YaxisChannels As List(Of String), LineType As clsGroupLines.eLineType, CommonYAxis As Boolean, ShowPortStbdBackground As Boolean, ShowTargetIfAvailable As Boolean)
    Me.YaxisChannels = YaxisChannels
    Me.LineType = LineType
    Me.CommonYAxis = CommonYAxis
    Me.ShowPortStbdBackground = ShowPortStbdBackground
    Me.ShowTargetIfAvailable = ShowPortStbdBackground
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotChartSettings
  Public Property YaxisChannel As String
  Public Property MathChannel As UserControlPavarotPlotViewModelMathPlots.eCanaleCustom
  Public Property ReverseSign As Boolean

  Public Sub New()

  End Sub

  Public Sub New(YaxisChannel As String, MathChannel As UserControlPavarotPlotViewModelMathPlots.eCanaleCustom, ReverseSign As Boolean)
    Me.YaxisChannel = YaxisChannel
    Me.MathChannel = MathChannel
    Me.ReverseSign = ReverseSign
  End Sub

End Class



''' <summary>Sezioni del report PDF delle straight line e creazione del csv della tabella dati.</summary>
<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineReportOptions
  Public Property PrintCharts As Boolean = True
  Public Property PrintTable As Boolean = True
  ''' <summary>L'elenco dei periodi e' sempre l'ultima sezione del report.</summary>
  Public Property PrintPeriods As Boolean = True
  ''' <summary>Crea anche un csv con la tabella dati completa, accanto al pdf.</summary>
  Public Property CreateTableCsv As Boolean = False
  ''' <summary>Ampiezza in gradi delle fasce di TWA della tabella per TWS e TWA (solo reaching).</summary>
  Public Property TwaBinDegrees As Integer = 20
  ''' <summary>True: a ogni pdf/html si apre l'elenco con le caselle per scegliere quali canali stampare (proprieta' Export).</summary>
  Public Property AskChannels As Boolean = False
  ''' <summary>Ampiezza in % della banda centrale usata per Min e Max dei valori nei report: 90 = P5 e P95, 100 = minimo e massimo veri. I grafici mostrano sempre tutto.</summary>
  Public Property RangeBandPercent As Integer = 90

  Public Function Copia() As clsStraightLineReportOptions
    Return New clsStraightLineReportOptions With {.PrintCharts = PrintCharts, .PrintTable = PrintTable, .PrintPeriods = PrintPeriods,
      .CreateTableCsv = CreateTableCsv, .TwaBinDegrees = TwaBinDegrees, .AskChannels = AskChannels, .RangeBandPercent = RangeBandPercent}
  End Function
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStraightLineChartSettings
  Public Property ChannelName As String
  ''' <summary>Se il canale va nel report pdf/html. Ogni andatura ha la sua lista, quindi la scelta e' per andatura.</summary>
  Public Property Export As Boolean = True

  Public Sub New()

  End Sub

  Public Sub New(ChannelName As String)
    Me.ChannelName = ChannelName
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class ChannelMaxMin
  Public Channel As clsChannel2020
  Public Property XMax As Double = Double.NaN
  Public Property XMin As Double = Double.NaN
  Public Property XStart As Double
  Public Property XStep As Double
  Public Property StepDecimals As Integer = 0

End Class



<AddINotifyPropertyChangedInterface>
Public Class XYplotDataTables
  Public Shared Property Tables As New List(Of XYplotDataTable)
  Public Shared Property ChannelsMaxMin As New List(Of ChannelMaxMin)
  'Public Shared Property XMax As Double
  'Public Shared Property XMin As Double
  'Public Shared Property XStart As Double
  'Public Shared Property XStep As Double
  Public Shared Property Name As String
  Public Shared Property Counter As Integer = -1

  Public Shared Property CsvTable As String

  Public Shared Sub Reset()
    Tables.Clear()
    ChannelsMaxMin.Clear()
    Name = String.Empty
  End Sub

  Shared Sub SetMaxMin()

    For Each t As XYplotDataTable In Tables
      If ChannelsMaxMin.Where(Function(x) x.Channel Is t.Xchannel).Count = 0 Then
        Dim x As New ChannelMaxMin
        x.Channel = t.Xchannel
        ChannelsMaxMin.Add(x)
      End If
    Next


    For Each t As XYplotDataTable In Tables
      Dim c = ChannelsMaxMin.Where(Function(x) x.Channel Is t.Xchannel).FirstOrDefault

      If Double.IsNaN(c.XMax) Then
        c.XMax = MathNet.Numerics.Statistics.Statistics.Percentile(t.XValues, 98)
      Else
        c.XMax = Math.Max(c.XMax, MathNet.Numerics.Statistics.Statistics.Percentile(t.XValues, 98))
      End If
    Next
    For Each t As XYplotDataTable In Tables
      Dim c = ChannelsMaxMin.Where(Function(x) x.Channel Is t.Xchannel).FirstOrDefault
      If Double.IsNaN(c.XMin) Then
        c.XMin = MathNet.Numerics.Statistics.Statistics.Percentile(t.XValues, 2)
      Else
        c.XMin = Math.Min(c.XMin, MathNet.Numerics.Statistics.Statistics.Percentile(t.XValues, 2))
        c.XMin = Math.Min(c.XMin, MathNet.Numerics.Statistics.Statistics.Percentile(t.XValues, 2))
      End If
    Next

    For Each c In ChannelsMaxMin
      If Not (Double.IsNaN(c.XMax) OrElse Double.IsNaN(c.XMin)) Then
        c.XStart = Math.Floor(c.XMin)
        c.XStep = CInt((c.XMax - c.XMin) / 10)
        If c.XStep = 0 Then
          c.XStep = CInt((c.XMax - c.XMin) * 10) / 10
          If c.XStep = 0 Then
            c.XStep = CInt((c.XMax - c.XMin) * 100) / 100
            If c.XStep = 0 Then
              c.XStep = CInt((c.XMax - c.XMin) * 1000) / 1000
            Else
              c.XStep = 0.00001
            End If
          End If
        End If
        If c.XStep >= 1 Then
          If (c.XStep / 2 = CInt(c.XStep / 2)) Then
            If (c.XStart / 2 = CInt(c.XStart / 2)) Then
              c.XStart -= 1
            End If
          End If
        End If
      End If
    Next


  End Sub

  Public Shared Sub SetCsvTable()


    Dim Rows As New List(Of String)
    Dim Row As String = Name & vbCrLf & vbCrLf
    Dim tt = Tables.OrderBy(Function(x) x.Xchannel.ActualLogHeader).ToList
    'Dim tt = Tables.OrderBy(Function(x) x.Order).OrderBy(Function(x) x.Xchannel.ActualLogHeader).ToList
    'Dim tt = Tables.ToList
    Dim prevx As String = ""
    Dim prevy As String = ""

    SetMaxMin()

    For Each t As XYplotDataTable In tt
      Dim c = ChannelsMaxMin.Where(Function(x) x.Channel Is t.Xchannel).FirstOrDefault
      Dim i As Double = c.XStart
      If prevx = "" OrElse prevx <> t.Xchannel.Name Then
        Row = "-------" & vbTab & "-------" & vbTab & t.Xchannel.Name
        Do
          Row &= vbTab & (i + (c.XStep / 2)).ToString("F" & c.StepDecimals.ToString)
          If c.XStep = 0 Then Exit Do
          If i > c.XMax Then Exit Do
          i += c.XStep
        Loop
        If Rows.Count > 0 Then Rows.Add("")
        Rows.Add(Row)
      End If
      i = c.XStart
      Dim RowAvg, RowMax, RowMin, RowSD, RowSpl As String
      Dim RowTgtUp As String = ""
      Dim RowTgtDn As String = ""
      Dim Tgt As clsChannelTarget = Nothing
      If prevy = "" OrElse prevy <> t.Ychannel.Name Then
        If t.Xchannel.CanaleChiave = clsChannels2020.eCanaliChiave.eTWS Then
          If t.ShowTgtUp OrElse t.ShowTgtDn Then
            Tgt = TgtManager.Tgt.Polare(t.Ychannel.PolarHeader)
          End If
        End If
      End If

      Dim T1 As String = ""
      Dim T2 As String = ""
      Dim T3 As String = ""
      Dim T4 As String = ""
      Dim T5 As String = ""

      Dim vv = t.Name.Split(" ")
      'If vv.Count = 1 Then
      '  T1 = vv(0)
      'ElseIf vv.Count = 2 Then
      '  T1 = vv(0)
      '  T2 = vv(1)
      'ElseIf vv.Count = 3 Then
      '  T1 = vv(0)
      '  T2 = vv(1)
      '  T3 = vv(2)
      'ElseIf vv.Count = 4 Then
      '  T1 = vv(0)
      '  T2 = vv(1)
      '  T3 = vv(2)
      '  T4 = vv(3)
      'ElseIf vv.Count = 5 Then
      '  T1 = vv(0)
      '  T2 = vv(1)
      '  T3 = vv(2)
      '  T4 = vv(3)
      '  T5 = vv(4)
      'Else
      If t.Name.Length < 60 Then
        Dim r(4) As String
        For i = 0 To 4
          r(i) = ""
        Next
        Dim ri As Integer = 0
        For Each v In vv
          If (r(ri) & v).Length > 15 Then
            ri += 1
          End If
          r(ri) += " " & v
        Next
        T1 = r(0)
        T2 = r(1)
        T3 = r(2)
        T4 = r(3)
        T5 = r(4)
      Else
        If (t.Name.Length < 15) Then
          T1 = t.Name
        ElseIf (t.Name.Length < 25) Then
          T1 = t.Name.Substring(0, 10)
          T2 = t.Name.Substring(10)
        ElseIf (t.Name.Length < 35) Then
          T1 = t.Name.Substring(0, 10)
          T2 = t.Name.Substring(10, 10)
          T3 = t.Name.Substring(20)
        ElseIf (t.Name.Length < 45) Then
          T1 = t.Name.Substring(0, 10)
          T2 = t.Name.Substring(10, 10)
          T3 = t.Name.Substring(20, 10)
          T4 = t.Name.Substring(30)
        Else
          T1 = t.Name.Substring(0, 10)
          T2 = t.Name.Substring(10, 10)
          T3 = t.Name.Substring(20, 10)
          T4 = t.Name.Substring(30, 10)
          T5 = t.Name.Substring(40)
        End If
      End If



      If Not Tgt Is Nothing Then
        If t.ShowTgtUp Then
          RowTgtUp = "" & vbTab & t.Ychannel.Name & vbTab & "TgtUp"
        End If
        If t.ShowTgtDn Then
          RowTgtDn = "" & vbTab & t.Ychannel.Name & vbTab & "TgtDn"
        End If
        RowAvg = T1 & vbTab & "" & vbTab & "Avg"
        'RowAvg = "" & vbTab & t.Name & vbTab & "Avg"
      Else
        RowTgtUp = ""
        RowTgtDn = ""
        If prevy = t.Ychannel.Name AndAlso prevx = t.Xchannel.Name Then
          'RowAvg = "" & vbTab & t.Name & vbTab & "Avg"
          RowAvg = T1 & vbTab & "" & vbTab & "Avg"
        Else
          'RowAvg = t.Ychannel.Name & vbTab & t.Name & vbTab & "Avg"
          RowAvg = T1 & vbTab & t.Ychannel.Name & vbTab & "Avg"
        End If
      End If
      'RowAvg = t.Ychannel.Name & vbTab & t.Name & vbTab & "Avg"

      RowMax = T2 & vbTab & "" & vbTab & "Max"
      RowMin = T3 & vbTab & "" & vbTab & "Min"
      RowSD = T4 & vbTab & "" & vbTab & "SD"
      RowSpl = T5 & vbTab & "" & vbTab & "Samples"
      Do
        If Not Tgt Is Nothing Then
          If t.ShowTgtUp Then
            Dim v = Tgt.TargetValue(i + (c.XStep / 2), True)
            RowTgtUp &= vbTab & v.ToString("F1")
          End If
          If t.ShowTgtDn Then
            Dim v = Tgt.TargetValue(i + (c.XStep / 2), False)
            RowTgtDn &= vbTab & v.ToString("F1")
          End If
        End If
        t.FilterData(i, i + c.XStep)
        RowAvg &= vbTab & t.Avg.ToString("F" & t.Ychannel.Decimals.ToString)
        RowMax &= vbTab & t.YPercMax.ToString("F" & t.Ychannel.Decimals.ToString)
        RowMin &= vbTab & t.YPercMin.ToString("F" & t.Ychannel.Decimals.ToString)
        RowSD &= vbTab & t.SD.ToString("F" & t.Ychannel.Decimals.ToString)
        RowSpl &= vbTab & t.Samples.ToString("F0")
        If c.XStep = 0 Then Exit Do
        If i > c.XMax Then Exit Do
        i += c.XStep
      Loop
      If Not RowTgtUp = "" Then Rows.Add(RowTgtUp)
      If Not RowTgtDn = "" Then Rows.Add(RowTgtDn)
      Rows.Add(RowAvg)
      Rows.Add(RowMax)
      Rows.Add(RowMin)
      Rows.Add(RowSD)
      Rows.Add(RowSpl)

      prevx = t.Xchannel.Name
      prevy = t.Ychannel.Name
    Next


    CsvTable = String.Join(vbCrLf, Rows.ToArray)

  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class XYplotDataTable
  Public Xchannel As clsChannel2020
  Public Ychannel As clsChannel2020
  Public Property XValues As List(Of Double)
  Public Property YValues As List(Of Double)
  Public Property Name As String
  Public Property Order As Integer
  Public Property ShowTgtUp As Boolean
  Public Property ShowTgtDn As Boolean
  Property YValuesFilt As New List(Of Double)

  Public Sub FilterData(Xmin As Double, Xmax As Double)
    Dim XY As New List(Of clsXYpoint)
    YValuesFilt.Clear()
    For i As Integer = 0 To XValues.Count - 1
      If XValues(i) >= Xmin AndAlso XValues(i) < Xmax Then
        YValuesFilt.Add(YValues(i))
      End If
    Next
  End Sub


  '  'Percentile1 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 1)
  '  'Percentile5 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 5)
  '  'Percentile95 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 95)
  '  'Percentile99 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 99)

  Public Function Ymin() As Double
    If Samples() = 0 Then Return 0
    Return YValuesFilt.Min
  End Function

  Public Function Ymax() As Double
    If Samples() = 0 Then Return 0
    Return YValuesFilt.Max
  End Function

  Public Function YPercMax() As Double
    If Samples() = 0 Then Return 0
    Return MathNet.Numerics.Statistics.Statistics.Percentile(YValuesFilt, 98)
  End Function

  Public Function YPercMin() As Double
    If Samples() = 0 Then Return 0
    Return MathNet.Numerics.Statistics.Statistics.Percentile(YValuesFilt, 2)
  End Function

  Public Function Samples() As Double
    Return YValuesFilt.Count
  End Function

  Public Function Avg() As Double
    If Samples() = 0 Then Return 0
    Return YValuesFilt.Average
  End Function

  Public Function Modes() As Double()
    If YValuesFilt Is Nothing OrElse YValuesFilt.Count = 0 Then
      Return Nothing
    Else
      Dim a = YValuesFilt.ToLookup(Function(x) x >= x)
      Dim b = a.Max(Function(x) x.Count)
      Dim c = a.Where(Function(x) x.Count = b)
      Return c
    End If
  End Function

  Public Function SD() As Double
    If Samples() = 0 Then Return 0
    Return YValuesFilt.StandardDeviation
  End Function

End Class



'<AddINotifyPropertyChangedInterface>
'Public Class clsCurrentOptimizer

'  Public Class NavSample
'    Public Property BS As Double     ' Boat speed (log)
'    Public Property CSE As Double    ' Heading (°)
'    Public Property SOG As Double    ' Speed over ground
'    Public Property COG As Double    ' Course over ground (°)
'  End Class

'  Public Class CalibrationResult
'    Public Property LogFactor As Double
'    Public Property CompassOffset As Double

'    Public Property CurrentSpeed As Double
'    Public Property CurrentDirection As Double

'    Public Property ErrorValue As Double
'  End Class

'  Public Class NavigationCalibration

'    Private Const DegToRad As Double = Math.PI / 180.0
'    Private Const RadToDeg As Double = 180.0 / Math.PI

'    Private Function Normalize(angle As Double) As Double
'      angle = angle Mod 360.0
'      If angle < 0 Then angle += 360.0
'      Return angle
'    End Function

'    Private Sub PolarToXY(speed As Double,
'                          course As Double,
'                          ByRef x As Double,
'                          ByRef y As Double)

'      Dim a = course * DegToRad

'      x = speed * Math.Sin(a)   'Est
'      y = speed * Math.Cos(a)   'Nord

'    End Sub

'    Public Function Optimize(samples As IEnumerable(Of NavSample)) As CalibrationResult

'      Dim data = samples.ToList()

'      If data.Count < 2 Then
'        Throw New Exception("Occorrono almeno due campioni.")
'      End If

'      Dim best As New CalibrationResult
'      best.ErrorValue = Double.MaxValue

'      'ricerca
'      For k As Double = 0.95 To 1.05 Step 0.0005

'        For delta As Double = -5 To 5 Step 0.02

'          Dim currents As New List(Of Tuple(Of Double, Double))

'          For Each s In data

'            Dim wx, wy As Double
'            PolarToXY(s.BS * k,
'                      Normalize(s.CSE + delta),
'                      wx, wy)

'            Dim gx, gy As Double
'            PolarToXY(s.SOG,
'                      s.COG,
'                      gx, gy)

'            currents.Add(
'                Tuple.Create(gx - wx,
'                             gy - wy))
'          Next

'          Dim avgX = currents.Average(Function(c) c.Item1)
'          Dim avgY = currents.Average(Function(c) c.Item2)

'          Dim err As Double = 0

'          For Each c In currents

'            Dim dx = c.Item1 - avgX
'            Dim dy = c.Item2 - avgY

'            err += dx * dx + dy * dy

'          Next

'          If err < best.ErrorValue Then

'            best.ErrorValue = err
'            best.LogFactor = k
'            best.CompassOffset = delta

'            best.CurrentSpeed =
'                Math.Sqrt(avgX * avgX + avgY * avgY)

'            best.CurrentDirection =
'                Normalize(Math.Atan2(avgX, avgY) * RadToDeg)

'          End If

'        Next
'      Next

'      Return best

'    End Function

'  End Class

'End Class

<AddINotifyPropertyChangedInterface>
Public Class clsXYPlotSettings

  <JsonIgnore>
  Dim _AvailableChannels As New ObservableCollection(Of clsChannel2020)


  Public Property DistributionIntervals As Integer
  <JsonIgnore>
  Public Property XAxisChannel As clsChannel2020
  Public Property XAxisChannelName As String
  Public Property XAxisDerivative As Boolean

  <JsonIgnore>
  Public Property YAxisChannel As clsChannel2020
  Public Property YAxisChannelName As String
  Public Property YAxisDerivative As Boolean

  <JsonIgnore>
  Public Property ZAxisChannel As clsChannel2020
  Public Property ZAxisChannelName As String = ""
  Public Property ZAxisDerivative As Boolean

  <JsonIgnore>
  Public Property Export360check As Boolean = False


  Public Property GroupingType As eGroupingType = eGroupingType.eSinglePeriod

  '<JsonIgnore>
  'Dim _SorgenteDati As eDataSource = eDataSource.eNone
  ''<JsonIgnore>
  Public Property SorgenteDati As eDataSource
  '  Get
  '    Return _SorgenteDati
  '  End Get
  '  Set(value As eDataSource)
  '    _SorgenteDati = value
  '    VerificaImpostaListe()
  '  End Set
  'End Property


  Public Property ManAndAccVisibleRange As Boolean
  Public Property PlotXDistribution As Boolean
  Public Property PlotYDistribution As Boolean
  Public Property ShowUpwindVmg As Boolean
  Public Property ShowDownwindVmg As Boolean
  Public Property ShowNotVmg As Boolean
  Public Property ShowStbd As Boolean
  Public Property ShowPort As Boolean
  Public Property ShowTargetIfAvailable As Boolean
  Public Property ShowBenchmarksIfAvailable As Boolean
  Public Property VmcAutoFilter As Boolean = True
  Public Property VmcAutoFilterRange As Integer = 10



  <JsonIgnore>
  Public Property FilterChannel As clsChannel2020
  'Dim _FilterChannelName As String
  <JsonIgnore>
  Public Property ChannelListUpdated As Boolean
  Public Property ApplyFilter1 As Boolean
  Public Property ApplyFilterMin As Boolean
  Public Property ApplyFilterMax As Boolean
  Public Property ApplyFilterAbsVal As Boolean
  'Public Property YaxisZeroIsNanThen As Boolean
  Public Property FilterValueMin As Double ' CurrentPlotSettings.FilterValueMin
  Public Property FilterValueMax As Double

  <JsonIgnore>
  Public Property Filter2Channel As clsChannel2020
  'Dim _Filter2ChannelName As String
  Public Property ApplyFilter2 As Boolean
  Public Property ApplyFilter2Min As Boolean
  Public Property ApplyFilter2Max As Boolean
  Public Property ApplyFilter2AbsVal As Boolean
  Public Property YaxisZeroIsNan As Boolean = True
  Public Property Filter2ValueMin As Double
  Public Property Filter2ValueMax As Double

  Public Property ApplyMinY As Boolean = False
  Public Property MinY As Double = 0
  Public Property ApplyMinX As Boolean = False
  Public Property MinX As Double = 0

  Public Property ApplyMaxY As Boolean = False
  Public Property MaxY As Double = 0
  Public Property ApplyMaxX As Boolean = False
  Public Property MaxX As Double = 0



  Public Property ApplyPercentileFilter As Boolean = True
  Public Property PercentileFilter As Integer = 3

  Public Property ApplyGroupFilterPerc As Boolean = True
  Public Property GroupFilterPerc As Integer = 5

  <JsonIgnore>
  Public Property SailingStateChannel As clsChannel2020
  'Public Property SailingStateChannelName As String
  Public Property SailingState As clsPeriodsManager2021.eRowType
  Public Property ApplyFilterSailingState As Boolean

  <JsonIgnore>
  Public Property ColorChannel As clsChannel2020
  'Public Property ColorChannelName As String
  Public Property ColorChannelIntervals As Integer

  Public Property DataPointSize As Integer
  Public Property DataPointOpacity As Integer
  Public Property DistributionOpacity As Integer
  Public Property DistributionBarWidth As Integer
  Public Property WindwardLeewardFunction As Boolean
  Public Property StampaTrendLines As Boolean
  Public Property StampaMeanAverages As Boolean
  Public Property StampaTabellaDati As Boolean = True
  Public Property TrendLinesStrokeThickness As Integer = 6
  Public Property TrendLinesStrokeArray As Double() = {3, 3}
  Public Property GradoTrendLines As Integer
  Public Property UseAbsValFor180 As Boolean

  <JsonIgnore>
  Dim _HLineCustom1Show As Boolean = False
  Public Property HLineCustom1Show As Boolean
    Set(value As Boolean)
      _HLineCustom1Show = value
      HLineCustom1Visibility = IIf(value, Visibility.Visible, Visibility.Hidden)
      HLineCustom1IsHidden = Not value
    End Set
    Get
      Return _HLineCustom1Show
    End Get
  End Property

  <JsonIgnore>
  Dim _HLineCustom2Show As Boolean = False
  Public Property HLineCustom2Show As Boolean
    Set(value As Boolean)
      _HLineCustom2Show = value
      HLineCustom2Visibility = IIf(value, Visibility.Visible, Visibility.Hidden)
      HLineCustom2IsHidden = Not value
    End Set
    Get
      Return _HLineCustom2Show
    End Get
  End Property

  <JsonIgnore>
  Dim _VLineCustom1Show As Boolean = False
  Public Property VLineCustom1Show As Boolean
    Set(value As Boolean)
      _VLineCustom1Show = value
      VLineCustom1Visibility = IIf(value, Visibility.Visible, Visibility.Hidden)
      VLineCustom1IsHidden = Not value
    End Set
    Get
      Return _VLineCustom1Show
    End Get
  End Property

  <JsonIgnore>
  Dim _VLineCustom2Show As Boolean = False
  Public Property VLineCustom2Show As Boolean
    Set(value As Boolean)
      _VLineCustom2Show = value
      VLineCustom2Visibility = IIf(value, Visibility.Visible, Visibility.Hidden)
      VLineCustom2IsHidden = Not value
    End Set
    Get
      Return _VLineCustom2Show
    End Get
  End Property

  ' nascoste di default: Visibility vale Visible a zero e, se il setter di Show non viene chiamato
  ' (es. impostazioni nuove o senza la chiave nel json), le linee comparirebbero anche non flaggate
  <JsonIgnore>
  Public Property HLineCustom1Visibility As Visibility = Visibility.Hidden
  <JsonIgnore>
  Public Property HLineCustom2Visibility As Visibility = Visibility.Hidden
  <JsonIgnore>
  Public Property VLineCustom1Visibility As Visibility = Visibility.Hidden
  <JsonIgnore>
  Public Property VLineCustom2Visibility As Visibility = Visibility.Hidden

  ' le annotazioni SciChart gestiscono da sole Visibility (la riscrivono, e con il binding TwoWay la riportano
  ' qui come Visible), quindi per nasconderle si usa IsHidden, che SciChart rispetta
  <JsonIgnore>
  Public Property HLineCustom1IsHidden As Boolean = True
  <JsonIgnore>
  Public Property HLineCustom2IsHidden As Boolean = True
  <JsonIgnore>
  Public Property VLineCustom1IsHidden As Boolean = True
  <JsonIgnore>
  Public Property VLineCustom2IsHidden As Boolean = True

  Public Property HLineCustom1Value As Double = 0
  Public Property HLineCustom2Value As Double = 0
  Public Property VLineCustom1Value As Double = 0
  Public Property VLineCustom2Value As Double = 0

  Public Property HLine0Visibility As Visibility
  Public Property HLine1Visibility As Visibility
  Public Property HLine100Visibility As Visibility
  Public Property MinSamples As Integer = 20




  Dim _ForceShowLegend As Boolean
  Public Property ForceShowLegend As Boolean
    Get
      Return _ForceShowLegend
    End Get
    Set(value As Boolean)
      _ForceShowLegend = value
    End Set
  End Property

  ' dimensioni dei testi sul grafico (nei profili vecchi la chiave manca e restano questi valori, uguali a quelli di prima)
  Public Property ChartTitleFontSize As Integer = 30 ' titolo (il sottotitolo e' circa la sua meta')
  Public Property ChartLabelsFontSize As Integer = 40 ' nomi dei canali X e Y scritti sul grafico
  Public Property AxisNumbersFontSize As Integer = 12 ' numeri sugli assi
  Public Property LegendFontSize As Integer = 12 ' legenda (nomi delle serie)

  ' valori effettivi usati dal grafico: mai sotto i 6 punti (un FontSize 0 o negativo non e' valido)
  <JsonIgnore>
  Public ReadOnly Property ChartTitleFontSizeEff As Double
    Get
      Return Math.Max(6, ChartTitleFontSize)
    End Get
  End Property
  <JsonIgnore>
  Public ReadOnly Property ChartSubTitleFontSizeEff As Double
    Get
      Return Math.Max(6, Math.Round(Math.Max(6, ChartTitleFontSize) * 16 / 30))
    End Get
  End Property
  <JsonIgnore>
  Public ReadOnly Property ChartLabelsFontSizeEff As Double
    Get
      Return Math.Max(6, ChartLabelsFontSize)
    End Get
  End Property
  <JsonIgnore>
  Public ReadOnly Property LegendFontSizeEff As Double
    Get
      Return Math.Max(6, LegendFontSize)
    End Get
  End Property
  <JsonIgnore>
  Public ReadOnly Property AxisNumbersFontSizeEff As Double
    Get
      Return Math.Max(6, AxisNumbersFontSize)
    End Get
  End Property

  Dim _LegendIsVisible As Boolean
  Public Property LegendIsVisible As Boolean
    Get
      Return _LegendIsVisible
    End Get
    Set(value As Boolean)
      _LegendIsVisible = value
    End Set
  End Property

  ''' <summary>
  ''' La legenda si vede solo con "Show Legend" spuntato. Prima valeva ForceShowLegend OrElse LegendIsVisible, e
  ''' LegendIsVisible viene messo a True a ogni disegno del grafico: la legenda c'era sempre e la casella poteva
  ''' solo "forzarla", mai nasconderla.
  ''' </summary>
  Public ReadOnly Property ShowLegend As Boolean
    Get
      Return ForceShowLegend
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DescriptionShort As String
    Get
      Dim n As String = ""
      If Not YAxisChannel Is Nothing Then n = " [" & YAxisChannel.ShortName & "]"
      Return SorgenteDati.ToString.TrimStart("e") & ", X: '" & XAxisChannelName & "', Y: '" & YAxisChannelName & n & "'" & ColoredBy() & " [" & StringaFiltroBase() & "]"
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DescriptionExtended As String
    Get
      Return DescriptionShort
    End Get
  End Property

  Private Function StringaFiltroBase() As String
    Dim strtmp As String = FiltroAndatura()
    strtmp &= FiltroSailingState()
    strtmp &= TipoFiltro() & TipoFiltro2()
    Return strtmp
  End Function

  Public Function ColoredBy() As String
    Return ", ColoredBy: " & GroupingType.ToString.TrimStart("e")
  End Function

  Public Function FiltroAndatura() As String
    Dim txttmp As String = ""
    If ShowUpwindVmg And ShowDownwindVmg And ShowNotVmg Then
      If Not (ShowPort And ShowStbd) Then
        txttmp &= IIf(ShowStbd, "Stbd", "") & "" & IIf(ShowPort, "Port", "")
      End If
    ElseIf ShowUpwindVmg And ShowDownwindVmg Then
      txttmp &= "Vmg Only"
      If Not (ShowPort And ShowStbd) Then
        txttmp &= IIf(ShowStbd, " Stbd", "") & "" & IIf(ShowPort, " Port", "")
      End If
    ElseIf ShowUpwindVmg Then
      txttmp &= "Upwind"
      If Not (ShowPort And ShowStbd) Then
        txttmp &= IIf(ShowStbd, " Stbd", "") & "" & IIf(ShowPort, " Port", "")
      End If
    ElseIf ShowNotVmg Then
      txttmp &= "Not Vmg Angles"
      If Not (ShowPort And ShowStbd) Then
        txttmp &= IIf(ShowStbd, " Stbd", "") & "" & IIf(ShowPort, " Port", "")
      End If
    ElseIf ShowDownwindVmg Then
      txttmp &= "Downwind"
      If Not (ShowPort And ShowStbd) Then
        txttmp &= IIf(ShowStbd, " Stbd", "") & "" & IIf(ShowPort, " Port", "")
      End If
    End If
    Return txttmp
  End Function

  Private Function FiltroSailingState() As String
    If ApplyFilterSailingState Then
      Return ", " & SailingState.ToString.TrimStart("e")
    End If
    Return ""
  End Function


  Private Function TipoFiltro() As String
    Dim txttmp As String = ""
    If SorgenteDati = clsXYPlotSettings.eDataSource.eCurrentVisibleRange Then Return ""
    If Not ApplyFilter1 Then Return ""
    If Not FilterChannel Is Nothing Then
      txttmp &= " Filtered by:" & IIf(ApplyFilterAbsVal, " AbsVal of", "") & " '" & FilterChannel.ShortName & "' "
      If ApplyFilterMin And ApplyFilterMax Then
        txttmp &= "between " & FilterValueMin.ToString("F" & FilterChannel.Decimals.ToString) & " and " & _FilterValueMax.ToString("F" & FilterChannel.Decimals.ToString) & ""
      ElseIf ApplyFilterMin Then
        txttmp &= "min " & FilterValueMin.ToString("F" & FilterChannel.Decimals.ToString)
      ElseIf ApplyFilterMax Then
        txttmp &= "max " & FilterValueMax.ToString("F" & FilterChannel.Decimals.ToString)
      End If
    End If
    Return txttmp
  End Function

  Private Function TipoFiltro2() As String
    If SorgenteDati = clsXYPlotSettings.eDataSource.eCurrentVisibleRange Then Return ""
    If Not ApplyFilter2 Then Return ""
    If Filter2Channel Is Nothing Then Return ""
    Dim txttmp As String = " and:" & IIf(ApplyFilter2AbsVal, " AbsVal of", "") & " '" & Filter2Channel.ShortName & "' "
    If ApplyFilter2Min And ApplyFilter2Max Then
      txttmp &= "between " & Filter2ValueMin.ToString("F" & Filter2Channel.Decimals.ToString) & " and " & Filter2ValueMax.ToString("F" & Filter2Channel.Decimals.ToString) & ""
    ElseIf ApplyFilter2Min Then
      txttmp &= "min " & Filter2ValueMin.ToString("F" & Filter2Channel.Decimals.ToString)
    ElseIf ApplyFilter2Max Then
      txttmp &= "max " & Filter2ValueMax.ToString("F" & Filter2Channel.Decimals.ToString)
    End If
    Return txttmp
  End Function

  Public Sub AggiornaAvailableChannels()
    ' la lista e' agganciata a molte combo: si prepara per intero e la si sostituisce in un colpo solo,
    ' invece di fare Clear e un Add per canale (ogni Add rielabora tutte le combo collegate)
    Using clsLogTempi.Misura("AggiornaAvailableChannels: ordinamento canali")
      Dim Ordinati As List(Of clsChannel2020) = DataProvider2020.Channels.ListaCanali.OrderBy(Function(x) x.LongName).ToList
      clsLogTempi.Scrivi("AggiornaAvailableChannels: canali = " & Ordinati.Count)
      Using clsLogTempi.Misura("AggiornaAvailableChannels: sostituzione lista (bind combo)")
        AvailableChannels = New ObservableCollection(Of clsChannel2020)(Ordinati)
      End Using
    End Using
    If ZAxisChannelName Is Nothing Then ZAxisChannelName = ""
    Using clsLogTempi.Misura("AggiornaAvailableChannels: assegnazione canali X/Y/Z/Color/Filter")
    Using clsLogTempi.Misura("  X = " & XAxisChannelName) : XAxisChannel = DataProvider2020.Channels.Canale(XAxisChannelName) : End Using
    Using clsLogTempi.Misura("  Y = " & YAxisChannelName) : YAxisChannel = DataProvider2020.Channels.Canale(YAxisChannelName) : End Using
    Using clsLogTempi.Misura("  Z = " & ZAxisChannelName) : ZAxisChannel = DataProvider2020.Channels.Canale(ZAxisChannelName) : End Using
    Using clsLogTempi.Misura("  Color = " & ColorChannelName) : ColorChannel = DataProvider2020.Channels.Canale(ColorChannelName) : End Using
    Using clsLogTempi.Misura("  Filter = " & FilterChannelName) : FilterChannel = DataProvider2020.Channels.Canale(FilterChannelName) : End Using
    Using clsLogTempi.Misura("  Filter2 = " & Filter2ChannelName) : Filter2Channel = DataProvider2020.Channels.Canale(Filter2ChannelName) : End Using
    Using clsLogTempi.Misura("  SailingState = " & SailingStateChannelName) : SailingStateChannel = DataProvider2020.Channels.Canale(SailingStateChannelName) : End Using
    End Using
  End Sub

  Public Sub VerificaImpostaListe()
    If DataProvider2020 Is Nothing Then Exit Sub
    If AvailableChannels.Count = 0 OrElse AppConfig.ActiveProfile.XYPlotSettings.ChannelListUpdated Then
      AppConfig.ActiveProfile.XYPlotSettings.ChannelListUpdated = False
      AggiornaAvailableChannels()
      'For Each c In DataProvider2020.Channels.ListaCanali.OrderBy(Function(x) x.LongName).ToList
      '    AvailableChannels.Add(c)
      '    Select Case c.ChannelId
      '        Case XAxisChannelName
      '            XAxisChannel = c ' DataProvider2020.CanaleDbl(XAxisChannelName)
      '        Case YAxisChannelName
      '            YAxisChannel = c ' DataProvider2020.CanaleDbl(YAxisChannelName)
      '        Case FilterChannelName
      '            FilterChannel = c ' DataProvider2020.CanaleDbl(FilterChannelName)
      '        Case Filter2ChannelName
      '            Filter2Channel = c ' DataProvider2020.CanaleDbl(Filter2ChannelName)
      '        Case ColorChannelName
      '            ColorChannel = c ' DataProvider2020.CanaleDbl(ColorChannelName)
      '        Case SailingStateChannelName
      '            SailingStateChannel = c ' DataProvider2020.CanaleDbl(SailingStateChannelName)
      '    End Select
      'Next
      'If ZAxisChannelName Is Nothing Then ZAxisChannelName = ""
      'XAxisChannel = DataProvider2020.Channels.Canale(XAxisChannelName)
      'YAxisChannel = DataProvider2020.Channels.Canale(YAxisChannelName)
      'ZAxisChannel = DataProvider2020.Channels.Canale(ZAxisChannelName)
      'ColorChannel = DataProvider2020.Channels.Canale(ColorChannelName)
      'FilterChannel = DataProvider2020.Channels.Canale(FilterChannelName)
      'Filter2Channel = DataProvider2020.Channels.Canale(Filter2ChannelName)
      'SailingStateChannel = DataProvider2020.Channels.Canale(SailingStateChannelName)
    End If
  End Sub

  Public Sub ImpostaCanaliDaNomi()
    If DataProvider2020 Is Nothing Then Exit Sub
    XAxisChannel = DataProvider2020.Channels.Canale(XAxisChannelName)
    YAxisChannel = DataProvider2020.Channels.Canale(YAxisChannelName)
    ZAxisChannel = DataProvider2020.Channels.Canale(ZAxisChannelName)
    ColorChannel = DataProvider2020.Channels.Canale(ColorChannelName)
    FilterChannel = DataProvider2020.Channels.Canale(FilterChannelName)
    Filter2Channel = DataProvider2020.Channels.Canale(Filter2ChannelName)
    SailingStateChannel = DataProvider2020.Channels.Canale(SailingStateChannelName)
  End Sub


  <JsonIgnore>
  Public Property AvailableChannels As ObservableCollection(Of clsChannel2020)
    Get
      Return _AvailableChannels
    End Get
    Set(value As ObservableCollection(Of clsChannel2020))
      _AvailableChannels = value
    End Set
  End Property


  Public Property FilterChannelName As String
  Public Property Filter2ChannelName As String
  Public Property SailingStateChannelName As String
  Public Property ColorChannelName As String


  Public Sub SalvaNomiCanali()
    If Not XAxisChannel Is Nothing Then
      XAxisChannelName = XAxisChannel.ChannelId
    End If
    If Not YAxisChannel Is Nothing Then
      YAxisChannelName = YAxisChannel.ChannelId
    End If
    If Not ZAxisChannel Is Nothing Then
      ZAxisChannelName = ZAxisChannel.ChannelId
    End If
    If Not ColorChannel Is Nothing Then
      ColorChannelName = ColorChannel.ChannelId
    End If
    If Not FilterChannel Is Nothing Then
      FilterChannelName = FilterChannel.ChannelId
    End If
    If Not Filter2Channel Is Nothing Then
      Filter2ChannelName = Filter2Channel.ChannelId
    End If
    If Not SailingStateChannel Is Nothing Then
      SailingStateChannelName = SailingStateChannel.ChannelId
    End If
  End Sub


  Public Enum eGroupingType
    eSinglePeriod = 0
    eTackOnly = 1
    eUpDnOnly = 2
    eTackAndUpDown = 3
    eAllTogether = 4
    eColorBins = 5
    eDayBins = 6
    eMonthBins = 7
    eKeyBins = 8
    eRaceBins = 12
    eRaceLegBins = 13
    eYearBins = 9
    eValueBins = 10
    e360checks = 11
  End Enum

  Public Enum eDataSource
    eNone = 0
    eCurrentVisibleRange = 1
    eCurrentVisibleRangeFiltered = 2
    eAllStraightLinesVmg = 3
    eSelectedStraightLinesVmg = 4
    eAllStraightLinesReaching = 5
    eSelectedStraightLinesReaching = 6
    eAllStraightLines = 7
    eSelectedStraightLines = 8
    eSelectedManoeuversEntryToExit = 9
    eSelectedManoeuversVisibleRange = 10
    eCurrVisRngFiltVsSelPeriods = 11
    eAllStraightLinesVsSelected = 12
    eAllStraightLinesVmgVsSelected = 13
    eAllStraightLinesReachingVsSelected = 14
    eCurrVisRngVsFilter = 15
    eSelectedStrLinesVsFilter = 16
    eSelectedStrLinesVmgVsFilter = 17
    eSelectedStrLinesReachingVsFilter = 18
  End Enum

  Public Sub New()

  End Sub

  Public Sub ImpostaValoriDefault()
    DistributionIntervals = 10
    XAxisChannelName = ""
    XAxisDerivative = False
    YAxisChannelName = ""
    YAxisDerivative = False
    ZAxisChannelName = ""
    ZAxisDerivative = False
    GroupingType = eGroupingType.eSinglePeriod
    SorgenteDati = eDataSource.eNone
    ManAndAccVisibleRange = True
    PlotXDistribution = False
    PlotYDistribution = False
    LegendIsVisible = True
    ShowUpwindVmg = True
    ShowDownwindVmg = True
    ShowNotVmg = True
    ShowStbd = True
    ShowPort = True
    ShowTargetIfAvailable = True
    ShowBenchmarksIfAvailable = True
    FilterChannelName = ""
    ApplyFilter1 = False
    ApplyFilterMin = False
    ApplyFilterMax = False
    ApplyFilterAbsVal = False
    YaxisZeroIsNan = False
    FilterValueMin = 0
    FilterValueMax = 0

    Filter2ChannelName = ""
    ApplyFilter2 = False
    ApplyFilter2Min = False
    ApplyFilter2Max = False
    ApplyFilter2AbsVal = False
    Filter2ValueMin = 0
    Filter2ValueMax = 0

    SailingState = clsPeriodsManager2021.eRowType.eStraightLineVmg
    ApplyFilterSailingState = False
    SailingStateChannelName = "SailingState"

    ColorChannelName = ""
    ColorChannelIntervals = 6


    DataPointSize = 6
    DataPointOpacity = 150
    DistributionOpacity = 100
    DistributionBarWidth = 50


    WindwardLeewardFunction = True


    StampaTrendLines = False
    GradoTrendLines = 4
    UseAbsValFor180 = True


    HLine0Visibility = Visibility.Hidden
    HLine1Visibility = Visibility.Hidden
    HLine100Visibility = Visibility.Hidden

  End Sub


  Public Function Clone() As clsXYPlotSettings
    Return Me.MemberwiseClone
  End Function

End Class




<AddINotifyPropertyChangedInterface>
Public Class clsSettings2021
  Dim DataPathFileName As String = "DataPath.txt"
  Public Property ActiveProfile As clsProfile2021
  <JsonIgnore>
  Public Property ProfileLoaded As Boolean = False
  <JsonIgnore>
  Public ReadOnly Property ApplicationDataFolder As String = My.Application.Info.DirectoryPath

  Public ReadOnly Property DataPathFile() As String
    Get
      Return System.IO.Path.Combine(ApplicationDataFolder, DataPathFileName)
    End Get
  End Property

  Public Sub New()
    'ProfileLoaded = LoadProfile()
  End Sub

  'Private Sub LoadSelectedProfile(SelectedPath As String)
  '    Dim ActiveProfileFile As String = ""
  '    If Not System.IO.File.Exists(SelectedPath) Then
  '        'Dim DataPathFile As String = System.IO.Path.Combine(ApplicationDataFolder, DataPathFileName)
  '        Dim file As System.IO.StreamWriter
  '        file = My.Computer.FileSystem.OpenTextFileWriter(DataPathFile, True)
  '        file.WriteLine("C:\Performance2021\Profiles\Default\Default.prf")
  '        file.Close()
  '    End If
  '    ActiveProfileFile = SelectedPath

  '    ActiveProfile = clsKillerSeriale.LoadConfigurationGeneric(Of clsProfile2021)(ActiveProfileFile)

  '    If ActiveProfile Is Nothing Then
  '        If Not System.IO.File.Exists(ActiveProfileFile) Then
  '            Dim fd As String = System.IO.Path.GetDirectoryName(ActiveProfileFile)
  '            If Not System.IO.Directory.Exists(fd) Then
  '                System.IO.Directory.CreateDirectory(fd)
  '            End If
  '        End If
  '        ActiveProfile = New clsProfile2021(System.IO.Path.GetFileNameWithoutExtension(ActiveProfileFile), ActiveProfileFile)
  '        Salva()
  '    Else
  '        ActiveProfile.ActiveProfileFilePath = ActiveProfileFile
  '    End If
  '    ProfileLoaded = True
  'End Sub


  'Private Function LoadProfileNew() As Boolean
  '    Dim DataPathFile As String = System.IO.Path.Combine(ApplicationDataFolder, DataPathFileName)
  '    If Not System.IO.File.Exists(DataPathFile) Then
  '        ' il DataPathFile non esiste, lo crea mettendo dentro il path di un profilo di default
  '        Dim file As System.IO.StreamWriter
  '        file = My.Computer.FileSystem.OpenTextFileWriter(DataPathFile, True)
  '        file.WriteLine("C:\Performance2021\Profiles\MyBoat\Default.prf")
  '        file.Close()
  '    End If

  '    Dim Contenuto As String = My.Computer.FileSystem.ReadAllText(DataPathFile).Trim
  '    Dim files As String() = Contenuto.Split(vbCrLf)
  '    Dim ActiveProfileFile As String = ""
  '    If files.Count = 0 Then
  '        Dim file As System.IO.StreamWriter
  '        file = My.Computer.FileSystem.OpenTextFileWriter(DataPathFile, True)
  '        file.WriteLine("C:\Performance2021\Profiles\Default\Default.prf")
  '        file.Close()
  '    ElseIf files.Count = 1 Then
  '        ActiveProfileFile = My.Computer.FileSystem.ReadAllText(DataPathFile).Trim.TrimEnd("*")
  '    Else
  '        Dim tmp As String = files.Where(Function(x) x.ToString.EndsWith("*")).FirstOrDefault()
  '        If tmp = Nothing Then
  '            Stop
  '            End
  '        Else
  '            ActiveProfileFile = tmp.Trim.TrimEnd("*")
  '        End If
  '    End If
  '    'Dim ActiveProfileFile As String = My.Computer.FileSystem.ReadAllText(DataPathFile).Trim
  '    ActiveProfile = clsKillerSeriale.LoadConfigurationGeneric(Of clsProfile2021)(ActiveProfileFile)

  '    If ActiveProfile Is Nothing Then
  '        If Not System.IO.File.Exists(ActiveProfileFile) Then
  '            Dim fd As String = System.IO.Path.GetDirectoryName(ActiveProfileFile)
  '            If Not System.IO.Directory.Exists(fd) Then
  '                System.IO.Directory.CreateDirectory(fd)
  '            End If
  '            'System.IO.File.Create(ActiveProfileFile)
  '        End If
  '        ActiveProfile = New clsProfile2021(System.IO.Path.GetFileNameWithoutExtension(ActiveProfileFile), ActiveProfileFile)
  '        Salva()
  '    Else
  '        'ActiveProfile.ProfileName = System.IO.Path.GetFileNameWithoutExtension(ActiveProfileFile)
  '        'ActiveProfile.ProfileFolder = System.IO.Path.GetDirectoryName(ActiveProfileFile)
  '        ActiveProfile.ActiveProfileFilePath = ActiveProfileFile
  '    End If
  '    Return True
  'End Function


  Public Sub Salva()
    If ActiveProfile Is Nothing Then Exit Sub

    ' i path interni all'albero del profilo vengono scritti in forma relativa, cosi' la
    ' cartella resta spostabile; subito dopo si ritorna agli assoluti usati a runtime
    Try
      ActiveProfile.ConvertiPathInRelativi()
      clsKillerSeriale.SaveConfigurationGeneric(Of clsProfile2021)(ActiveProfile, ActiveProfile.ProfileFilePath)
    Finally
      ActiveProfile.RisolviPathAssoluti()
    End Try
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPeriodsFinderSettings
  Public Property ValidRotationMinAngle As Integer
  Public Property RotationYrt As Double
  Public Property RotationMovingAverageSeconds As Double
  Public Property SecAnteTack As Integer
  Public Property SecPostTack As Integer
  Public Property SecAnteGybe As Integer
  Public Property SecPostGybe As Integer
  Public Property SecAnteRot As Integer
  Public Property SecPostRot As Integer
  ''' <summary>Voto minimo di PerformanceQuality perche' una riga sia valida. 0 = filtro disattivato.</summary>
  Public Property MinDataQualityIndex As Integer = 3
  ''' <summary>Voto minimo di EnvironmentQuality perche' una riga sia valida. 0 = filtro disattivato.</summary>
  Public Property MinEnvironmentQualityIndex As Integer = 3
  ''' <summary>Voto minimo di AttitudeQuality perche' una riga sia valida. 0 = filtro disattivato.</summary>
  Public Property MinAttitudeQualityIndex As Integer = 3
  ''' <summary>Durata minima di un periodo in secondi: sotto questa soglia il tratto viene scartato.</summary>
  Public Property MinPeriodSeconds As Integer = 15
  ''' <summary>Durata massima di un periodo in secondi: oltre, il tratto viene spezzato per continuita'.</summary>
  Public Property MaxPeriodSeconds As Integer = 300
  Public Property MaxTwaDeltaUpDn As Integer
  Public Property MaxBsTgtPercDelta As Integer

  Public Sub New()

  End Sub

  ''' <summary>Riempie i parametri assenti nei profili salvati con versioni precedenti.</summary>
  Public Sub NormalizzaValoriMancanti()
    If MinEnvironmentQualityIndex < 0 OrElse MinEnvironmentQualityIndex > 5 Then MinEnvironmentQualityIndex = 3
    If MinAttitudeQualityIndex < 0 OrElse MinAttitudeQualityIndex > 5 Then MinAttitudeQualityIndex = 3
    If MinDataQualityIndex < 0 OrElse MinDataQualityIndex > 5 Then MinDataQualityIndex = 3
    If MinPeriodSeconds <= 0 Then MinPeriodSeconds = 15
    If MaxPeriodSeconds <= 0 Then MaxPeriodSeconds = 300
    If MaxPeriodSeconds < MinPeriodSeconds Then MaxPeriodSeconds = MinPeriodSeconds * 2
  End Sub

  Public Sub ImpostaValoriDefault()
    ' 60, 6, 15, 25, 15, 25, 5, 10, 3, 10, 15
    ValidRotationMinAngle = 60
    RotationYrt = 2
    RotationMovingAverageSeconds = 10
    SecAnteTack = 10
    SecPostTack = 30
    SecAnteGybe = 10
    SecPostGybe = 30
    SecAnteRot = 5
    SecPostRot = 10
    MinDataQualityIndex = 3
    MinEnvironmentQualityIndex = 3
    MinAttitudeQualityIndex = 3
    MinPeriodSeconds = 15
    MaxPeriodSeconds = 300
    MaxTwaDeltaUpDn = 20
    MaxBsTgtPercDelta = 20
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotSettings
  'Public Property PavarotStandardRange As Integer = 30
  Public Property GybeSecAnte As Integer = 10
  Public Property GybeSecPost As Integer = 30
  Public Property TackSecAnte As Integer = 10
  Public Property TackSecPost As Integer = 30
  ' loss sulla traccia nell'acqua: stabilizzazione dell'uscita e recupero del vmg della mura d'uscita
  Public Property RecoveryVmgPerc As Double = 95   ' % del vmg di riferimento della mura d'uscita da raggiungere
  Public Property StableBsTolKts As Double = 0.2   ' variazione massima di velocita' (kts) nella finestra di stabilita'
  Public Property StableCseTolDeg As Double = 3    ' variazione massima di rotta sull'acqua (gradi) nella finestra di stabilita'
  Public Property StableWindowSec As Integer = 5   ' durata della finestra di stabilita' e delle medie d'entrata/uscita (s)

  Public Sub ImpostazioniDefault()
    'PavarotStandardRange = 30
    GybeSecAnte = 10
    GybeSecPost = 30
    TackSecAnte = 10
    TackSecPost = 30
    RecoveryVmgPerc = 95
    StableBsTolKts = 0.2
    StableCseTolDeg = 3
    StableWindowSec = 5
  End Sub

  ''' <summary>Riempie i parametri del loss assenti o non validi (profili salvati con versioni precedenti).</summary>
  Public Sub NormalizzaParametriLoss()
    If RecoveryVmgPerc <= 0 OrElse RecoveryVmgPerc > 200 Then RecoveryVmgPerc = 95
    If StableBsTolKts <= 0 Then StableBsTolKts = 0.2
    If StableCseTolDeg <= 0 Then StableCseTolDeg = 3
    If StableWindowSec <= 0 Then StableWindowSec = 5
  End Sub

End Class
<AddINotifyPropertyChangedInterface>
Public Class clsStartReportSettings
  Public Property CustomChannel1 As String = "TtkOnStbd"
  Public Property CustomChannel2 As String = "TtkOnPort"
  Public Property CustomChannel3 As String = "TtkRecoRc"
  Public Property CustomChannel4 As String = "TtkRecoPin"
  Public Property CustomChannel5 As String = "TtkPin"
  Public Property CustomChannel6 As String = "TtkRc"
  Public Property CustomChannel7 As String = "ProjHdg"
  Public Property CustomChannel8 As String = "ProjTgt"
  Public Property CustomChannel9 As String = "TtkTrigger"

End Class
<AddINotifyPropertyChangedInterface>
Public Class clsRaceReportSettings
  Public Property ExtraChannelsUpwind As List(Of clsChannel2020)
  Public Property ExtraChannelsDownwind As List(Of clsChannel2020)
  Public Property ExtraChannelsReaching As List(Of clsChannel2020)

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsMailingListSettings
  Public Property SmtpServerAddress As String
  Public Property SmtpServerPort As Integer
  Public Property SmtpAccount As String
  Public Property SmtpPassword As String
  Public Property MailSender As String
  Public Property MailingList As List(Of String)


  Public Sub SetDefaultForTesting()
    SmtpServerAddress = "smtp.gmail.com"
    SmtpServerPort = 465
    SmtpAccount = "sailingperformer@gmail.com"
    SmtpPassword = "haidettosandro"
    MailSender = "sailingperformer@gmail.com"
    MailingList = New List(Of String)
    MailingList.Add("fm.mongelli@gmail.com")
    MailingList.Add("office@paranna.it")

    'Dim mail As MailMessage = New MailMessage()
    'Dim SmtpServer As SmtpClient = New SmtpClient("smtp.gmail.com")
    'mail.From = New MailAddress("your mail@gmail.com")
    'mail.[To].Add("to_mail@gmail.com")
    'mail.Subject = "Test Mail - 1"
    'mail.Body = "mail with attachment"
    'Dim attachment As System.Net.Mail.Attachment
    'attachment = New System.Net.Mail.Attachment("c:/textfile.txt")
    'mail.Attachments.Add(attachment)
    'SmtpServer.Port = 587
    'SmtpServer.Credentials = New System.Net.NetworkCredential("your mail@gmail.com", "your password")
    'SmtpServer.EnableSsl = True
    'SmtpServer.Send(mail)


  End Sub


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsProfile2021
  '<JsonIgnore>
  'Public Property ProfileFolder As String
  <JsonIgnore>
  Public Property ActiveProfileFilePath As String
  'Public Property SettingsFolder As String
  Public Property ProfileName As String
  Public Property ProfileDescription As String
  Public Property ReportFolder As String
  Public Property LastFaroToParquet As String
  Public Property LastImpFolder As String
  Public Property LastPpfFolder As String
  Public Property TargetFile As String
  'Public Property LocalNavMarine As String
  Public Property LogHasReversedRudder As Boolean = False
  Public Property RudderIsAlreadyNormalized As Boolean = False
  Public Property RudderIsReversed As Boolean = True
  Public Property RudderNormFromPortStbd As Boolean = False
  Public Property ForceLeewayNormToAbsolute As Boolean = False
  Public Property LogHasReversedLeeway As Boolean = False
  Public Property ReverseTrim As Boolean = False
  'Public Property LocalNavSonic As String
  ''' <summary>Nome dell'ultimo layer mappa usato, per ripristinarlo al riavvio.</summary>
  Public Property LastMapLayer As String = ""
  Public Property ChannelConfigFileName As String
  Public Property BenchmarksFileName As String
  'Public Property PeriodsSettings As clsPeriodsSettings2021
  Public Property BasicChartSettings As List(Of clsBasicChartSettings)
  Public Property StraightLineChartSettingsAll As List(Of clsStraightLineChartSettings)
  Public Property StraightLineChartSettingsVmgUp As List(Of clsStraightLineChartSettings)
  Public Property StraightLineChartSettingsVmgDn As List(Of clsStraightLineChartSettings)
  Public Property StraightLineChartSettingsReaching As List(Of clsStraightLineChartSettings)
  Public Property PavarotChartSettings As List(Of clsPavarotChartSettings)
  Public Property XYPlotSettings As clsXYPlotSettings
  Public Property StartReportSettings As clsStartReportSettings
  Public Property RaceReportSettings As clsRaceReportSettings
  Public Property PeriodsFinderSettings As clsPeriodsFinderSettings
  Public Property CrossoverSettings As clsCrossoverSettings
  Public Property DataQualitySettings As clsQualitySettings
  Public Property MySongLeewaySettings As clsMySongLeewaySettings
  ''' <summary>Coefficienti e impostazioni del modello teorico del leeway (vedi LeewayModel.vb). Nothing nei profili precedenti: si usano i default.</summary>
  Public Property LeewayModel As clsLeewayModelSettings
  ''' <summary>Tabella TWS / SeaState atteso per il canale SeaStateNorm. Nothing nei profili precedenti: si usa quella di default.</summary>
  Public Property SeaStateNormTable As List(Of clsSeaStateNormPoint)
  ''' <summary>Cosa includere nel report PDF delle straight line (menu Reports dei tab upwind, downwind, reaching). Nothing = valori di default.</summary>
  Public Property StraightLineReportOptions As clsStraightLineReportOptions
  ''' <summary>Opzioni di report separate per tab (prima erano una sola, condivisa). Nothing = alla prima lettura si parte da una copia di StraightLineReportOptions.</summary>
  Public Property StraightLineReportOptionsAll As clsStraightLineReportOptions
  Public Property StraightLineReportOptionsVmgUp As clsStraightLineReportOptions
  Public Property StraightLineReportOptionsVmgDn As clsStraightLineReportOptions
  Public Property StraightLineReportOptionsReaching As clsStraightLineReportOptions
  Public Property BoatLenghtInMeters As Double
  Public Property InertialSowSeconds As Double = 5
  Public Property WindGradientAlpha As Double = 0.1
  Public Property WindGradientAlphaMin As Double = 0.15
  Public Property WindGradientAlphaMax As Double = 0.05
  Public Property MhuHeight As Double = 29
  Public Property SailFilesOffset As TimeSpan = New TimeSpan(0, 2, 0, 0, 0)
  Public Property MinVmgPerformanceValue As Double = 85
  Public Property MaxLeewayForRecalc As Double = 20
  Public Property MaxVmgPerformenceValue As Double = 110
  Public Property PavarotSettings As clsPavarotSettings
  Public Property MailingListSettings As clsMailingListSettings
  Public Property SummarySettings As clsSummarySettings
  Public Property SummaryHighlightSettings As clsSummaryHighlightSettings
  Public Property LiftAndDragSettings As List(Of clsLiftAndDragSettings)
  Public Property StripeHeightJib25 As Double = 6
  Public Property StripeHeightJib50 As Double = 11
  Public Property StripeHeightJib75 As Double = 16
  Public Property StripeHeightMain25 As Double = 9
  Public Property StripeHeightMain50 As Double = 15
  Public Property StripeHeightMain75 As Double = 21
  Public Property StripeHeightMain87 As Double = 24
  Public Property MastHeight As Double = 39
  Public Property MastHeightAlfa As Double = 0.1

  ' da mettere valori default
  Public Property CanaliDaEsportareNelCsv As List(Of String)

  Public Property XyReports As clsSciChartXyReports

  Public Sub New()
    'Stop
    ' bisogna verificare che esistano tutte le proprieta' ed i settaggi in modo che se ne aggiungo di nuove sul codice vengano messi i valori di default se ancora mai impostati
  End Sub

  Public Function ProfileFilePath() As String
    'Return IO.Path.Combine(ProfileFolder, ProfileName & ".prf")
    Return ActiveProfileFilePath
  End Function

  Public Function SettingsFolder() As String
    If String.IsNullOrWhiteSpace(ProfileFilePath()) Then Return ""
    Return IO.Path.Combine(System.IO.Path.GetDirectoryName(ProfileFilePath), "Settings")
  End Function

  Public Sub New(ProfileName As String, ProfileFilePath As String)
    ' se passa da qui vuol dire che il file del profilo non esiste
    Me.ProfileName = ProfileName
    Me.ActiveProfileFilePath = ProfileFilePath
    'Me.ProfileFolder = System.IO.Path.GetDirectoryName(ProfileFilePath)
    'PeriodsSettings = New clsPeriodsSettings2021(10, 30)
    'SettingsFolder = IO.Path.Combine(System.IO.Path.GetDirectoryName(ProfileFilePath), "Settings")
    If Not System.IO.Directory.Exists(SettingsFolder) Then
      System.IO.Directory.CreateDirectory(SettingsFolder)
    End If
    ChannelConfigFileName = "Channels.json"
    BenchmarksFileName = "Benchmarks.json"
    'ManoeuversBenchmarksFileName = "ManoeuversConfigFile.json"
    'AccelerationsBenchmarksFileName = "AccelerationsConfigFile.json"
    BoatLenghtInMeters = 15.85

    ImpostaChartBaseDefault()
    ImpostaStraightLineChartSettings()
    ImpostaChartPavarotDefault()
    ImpostaXYPlotSettings()
    ImpostaCrossoverSettings()

    PeriodsFinderSettings = New clsPeriodsFinderSettings
    PeriodsFinderSettings.ImpostaValoriDefault()
  End Sub

  ''' <summary>
  ''' Reimposta i valori di default per tutte le proprieta' non valorizzate.
  ''' Va chiamato SEMPRE dopo la creazione o il caricamento di un profilo, cosi' i
  ''' profili creati col costruttore vuoto o deserializzati da .prf piu' vecchi
  ''' risultano comunque completi.
  ''' </summary>
  Public Sub ImpostaValoriDefaultMancanti()
    If String.IsNullOrWhiteSpace(ChannelConfigFileName) Then ChannelConfigFileName = "Channels.json"
    If String.IsNullOrWhiteSpace(BenchmarksFileName) Then BenchmarksFileName = "Benchmarks.json"
    If BoatLenghtInMeters <= 0 Then BoatLenghtInMeters = 15.85

    If BasicChartSettings Is Nothing OrElse BasicChartSettings.Count = 0 Then ImpostaChartBaseDefault()
    If StraightLineChartSettingsAll Is Nothing OrElse StraightLineChartSettingsAll.Count = 0 Then ImpostaStraightLineChartSettings()
    If PavarotChartSettings Is Nothing OrElse PavarotChartSettings.Count = 0 Then ImpostaChartPavarotDefault()

    ImpostaXYPlotSettings()
    ImpostaCrossoverSettings()

    If PeriodsFinderSettings Is Nothing Then
      PeriodsFinderSettings = New clsPeriodsFinderSettings
      PeriodsFinderSettings.ImpostaValoriDefault()
    End If
    PeriodsFinderSettings.NormalizzaValoriMancanti()

    If DataQualitySettings Is Nothing Then
      DataQualitySettings = New clsQualitySettings
      DataQualitySettings.CaricaValoriDefault()
    Else
      DataQualitySettings.NormalizzaValoriMancanti()
    End If

    ' cartelle standard e path: possibile solo quando si sa dove sta il profilo
    If Not String.IsNullOrWhiteSpace(ActiveProfileFilePath) Then
      CreaCartelleStandard()
      RisolviPathAssoluti()
      ImpostaPathDefault()
    End If
  End Sub

#Region "Cartelle standard e path relativi"

  ''' <summary>Cartella che contiene il file di profilo. Base di tutti i path relativi.</summary>
  Public Function ProfileFolder() As String
    If String.IsNullOrWhiteSpace(ProfileFilePath()) Then Return ""
    Return System.IO.Path.GetDirectoryName(ProfileFilePath)
  End Function

  ''' <summary>Cartella dei dati: file da importare e .ppf gia' creati.</summary>
  Public Function DataFolder() As String
    If String.IsNullOrWhiteSpace(ProfileFolder()) Then Return ""
    Return IO.Path.Combine(ProfileFolder(), "Data")
  End Function

  ''' <summary>Cartella dei file target (polari).</summary>
  Public Function TargetFolder() As String
    If String.IsNullOrWhiteSpace(ProfileFolder()) Then Return ""
    Return IO.Path.Combine(ProfileFolder(), "Target")
  End Function

  ''' <summary>Crea le cartelle standard del profilo se non esistono gia'.</summary>
  Public Sub CreaCartelleStandard()
    CreaSeMancante(SettingsFolder())
    CreaSeMancante(DataFolder())
    CreaSeMancante(TargetFolder())
  End Sub

  Private Sub CreaSeMancante(Cartella As String)
    If String.IsNullOrWhiteSpace(Cartella) Then Exit Sub
    Try
      If Not System.IO.Directory.Exists(Cartella) Then System.IO.Directory.CreateDirectory(Cartella)
    Catch ex As Exception
      ' cartella non creabile: si prosegue, i dialoghi si apriranno altrove
    End Try
  End Sub

  ''' <summary>
  ''' Punta i path non ancora valorizzati alle cartelle standard, cosi' alla prima
  ''' installazione i dialoghi di apertura partono dal posto giusto invece che a caso.
  ''' </summary>
  Public Sub ImpostaPathDefault()
    If String.IsNullOrWhiteSpace(LastImpFolder) OrElse Not System.IO.Directory.Exists(LastImpFolder) Then LastImpFolder = DataFolder()
    If String.IsNullOrWhiteSpace(LastPpfFolder) OrElse Not System.IO.Directory.Exists(LastPpfFolder) Then LastPpfFolder = DataFolder()
    If String.IsNullOrWhiteSpace(LastFaroToParquet) OrElse Not System.IO.Directory.Exists(LastFaroToParquet) Then LastFaroToParquet = DataFolder()
    If String.IsNullOrWhiteSpace(ReportFolder) OrElse Not System.IO.Directory.Exists(ReportFolder) Then ReportFolder = DataFolder()

    ' il target e' un file, non una cartella: se non c'e' si punta comunque alla sua cartella
    If String.IsNullOrWhiteSpace(TargetFile) OrElse Not System.IO.File.Exists(TargetFile) Then
      Dim Trovato As String = PrimoFileTarget()
      If Not String.IsNullOrWhiteSpace(Trovato) Then TargetFile = Trovato
    End If
  End Sub

  ''' <summary>Primo file json presente nella cartella Target, se ce n'e' uno solo.</summary>
  Private Function PrimoFileTarget() As String
    Try
      Dim tf As String = TargetFolder()
      If String.IsNullOrWhiteSpace(tf) Then Return ""
      If Not System.IO.Directory.Exists(tf) Then Return ""
      Dim Files As String() = System.IO.Directory.GetFiles(tf, "*.json")
      If Files.Count = 1 Then Return Files(0)
    Catch ex As Exception
    End Try
    Return ""
  End Function

  ''' <summary>
  ''' Converte in assoluti i path scritti nel profilo in forma relativa alla cartella
  ''' del profilo stesso. Da chiamare al caricamento, cosi' il resto del codice lavora
  ''' sempre con path assoluti senza doversene preoccupare.
  ''' </summary>
  Public Sub RisolviPathAssoluti()
    LastImpFolder = PathAssoluto(LastImpFolder)
    LastPpfFolder = PathAssoluto(LastPpfFolder)
    LastFaroToParquet = PathAssoluto(LastFaroToParquet)
    ReportFolder = PathAssoluto(ReportFolder)
    TargetFile = PathAssoluto(TargetFile)
  End Sub

  ''' <summary>
  ''' Converte in relativi i path che stanno dentro l'albero del profilo, cosi' il file
  ''' salvato resta valido anche spostando la cartella o cambiando macchina.
  ''' Da chiamare subito prima di serializzare.
  ''' </summary>
  Public Sub ConvertiPathInRelativi()
    LastImpFolder = PathRelativo(LastImpFolder)
    LastPpfFolder = PathRelativo(LastPpfFolder)
    LastFaroToParquet = PathRelativo(LastFaroToParquet)
    ReportFolder = PathRelativo(ReportFolder)
    TargetFile = PathRelativo(TargetFile)
  End Sub

  ''' <summary>Rende assoluto un path relativo alla cartella del profilo.</summary>
  Public Function PathAssoluto(Percorso As String) As String
    If String.IsNullOrWhiteSpace(Percorso) Then Return Percorso
    Try
      If System.IO.Path.IsPathRooted(Percorso) Then Return Percorso
      Dim pf As String = ProfileFolder()
      If String.IsNullOrWhiteSpace(pf) Then Return Percorso
      Return System.IO.Path.GetFullPath(System.IO.Path.Combine(pf, Percorso))
    Catch ex As Exception
      Return Percorso
    End Try
  End Function

  ''' <summary>
  ''' Rende relativo un path che si trova sotto la cartella del profilo.
  ''' Quelli esterni restano assoluti: non avrebbe senso legarli al profilo.
  ''' </summary>
  Public Function PathRelativo(Percorso As String) As String
    If String.IsNullOrWhiteSpace(Percorso) Then Return Percorso
    Try
      If Not System.IO.Path.IsPathRooted(Percorso) Then Return Percorso
      Dim pf As String = ProfileFolder()
      If String.IsNullOrWhiteSpace(pf) Then Return Percorso

      Dim Radice As String = System.IO.Path.GetFullPath(pf).TrimEnd(System.IO.Path.DirectorySeparatorChar) & System.IO.Path.DirectorySeparatorChar
      Dim Completo As String = System.IO.Path.GetFullPath(Percorso)
      If Not Completo.StartsWith(Radice, StringComparison.CurrentCultureIgnoreCase) Then Return Percorso
      Return Completo.Substring(Radice.Length)
    Catch ex As Exception
      Return Percorso
    End Try
  End Function

#End Region


  'Public Sub ImpostazioniPostFileCaricato()
  '  ImpostaChartBaseDefault()
  '  ImpostaStraightLineChartSettings()
  '  ImpostaChartPavarotDefault()
  '  ImpostaXYPlotSettings()
  'End Sub


  Private Sub ImpostaXYPlotSettings()

    If XYPlotSettings Is Nothing Then
      XYPlotSettings = New clsXYPlotSettings
      XYPlotSettings.ImpostaValoriDefault()
    End If
  End Sub

  Private Sub ImpostaCrossoverSettings()

    If CrossoverSettings Is Nothing Then
      CrossoverSettings = New clsCrossoverSettings
      CrossoverSettings.ImpostaValoriDefault()
    End If
  End Sub

  Private Sub ImpostaStraightLineChartSettings()
    StraightLineChartSettingsAll = New List(Of clsStraightLineChartSettings)
    ImpostaStraightLineChartSettings(StraightLineChartSettingsAll, CanaliStraightLineDefault)
    StraightLineChartSettingsVmgUp = New List(Of clsStraightLineChartSettings)
    ImpostaStraightLineChartSettings(StraightLineChartSettingsVmgUp, CanaliStraightLineDefault)
    StraightLineChartSettingsVmgDn = New List(Of clsStraightLineChartSettings)
    ImpostaStraightLineChartSettings(StraightLineChartSettingsVmgDn, CanaliStraightLineDefault)
    StraightLineChartSettingsReaching = New List(Of clsStraightLineChartSettings)
    ImpostaStraightLineChartSettings(StraightLineChartSettingsReaching, CanaliStraightLineDefault)
  End Sub

  Public Sub ImpostaStraightLineChartSettings(ByRef Lista As List(Of clsStraightLineChartSettings), ListaHeadersCanali As List(Of String))
    ' chi era escluso dal report resta escluso se il canale e' ancora in elenco
    Dim Esclusi As New HashSet(Of String)(Lista.Where(Function(x) Not x.Export).Select(Function(x) x.ChannelName))
    Lista.Clear()
    For Each c In ListaHeadersCanali
      Lista.Add(New clsStraightLineChartSettings(c) With {.Export = Not Esclusi.Contains(c)})
    Next
  End Sub

  Public Sub ImpostaStraightLineChartSettingsDefault(ByRef Lista As List(Of clsStraightLineChartSettings))
    Lista = New List(Of clsStraightLineChartSettings)
    For Each c In CanaliStraightLineDefault()
      Lista.Add(New clsStraightLineChartSettings(c))
    Next
  End Sub

  Private Function CanaliStraightLineDefault() As List(Of String)
    Dim l As New List(Of String)
    l.Add("Tws")
    l.Add("VMGp")
    l.Add("BSTp")
    l.Add("TWAd")
    l.Add("Sow")
    l.Add("Twa")
    l.Add("Awa")
    l.Add("HeelNorm")
    l.Add("Trim")
    l.Add("RudderNorm")
    Return l
  End Function



  Private Sub ImpostaChartBaseDefault()
    BasicChartSettings = New List(Of clsBasicChartSettings)
    BasicChartSettings.Add(New clsBasicChartSettings({"Tws"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Twd"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Sow"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Sog"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Twa"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Awa"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Heel"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))
    BasicChartSettings.Add(New clsBasicChartSettings({"Rudder"}.ToList, clsGroupLines.eLineType.eDataTypeSigned, False, True, True))


  End Sub


  Private Sub ImpostaChartPavarotDefault()
    Me.PavarotChartSettings = New List(Of clsPavarotChartSettings)
    ImpostaChartPavarot(CanaliPavarotStandard)
  End Sub

  Public Sub ImpostaChartPavarot(ListaHeadersCanali As List(Of String))
    ' aggiunge i canali math delle pavarot
    Me.PavarotChartSettings.Clear()
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eProgressionLoss, False))
    For Each c In ChartPavarotLossAcqua()
      Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", c, False))
    Next
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eTwdVariation, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eTwdDeltaMinusTwaToTgtDelta, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eTwaVariation, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eCogVariation, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eHdgVariation, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eCogTwaDelta, False))
    Me.PavarotChartSettings.Add(New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eHdgTwaDelta, False))
    ' aggiunge i canali standard delle pavarot
    For Each c In ListaHeadersCanali
      Me.PavarotChartSettings.Add(New clsPavarotChartSettings(c, Nothing, ReversedSign(c)))
    Next
  End Sub

  Private Shared Function ChartPavarotLossAcqua() As List(Of UserControlPavarotPlotViewModelMathPlots.eCanaleCustom)
    Return New List(Of UserControlPavarotPlotViewModelMathPlots.eCanaleCustom) From {
      UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eLossAcquaBisettrice}
  End Function

  ''' <summary>Aggiunge ai profili salvati con versioni precedenti il grafico del loss sulla traccia, subito dopo Loss Progression, e toglie quelli non piu' previsti.</summary>
  Public Sub AggiungiChartPavarotMancanti()
    If PavarotChartSettings Is Nothing Then Exit Sub
    PavarotChartSettings.RemoveAll(Function(x) x.MathChannel = UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eVmgRecupero OrElse x.MathChannel = UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eLossAcquaTwd OrElse x.MathChannel = UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eLossGpsTwd)
    Dim Posizione As Integer = PavarotChartSettings.FindIndex(Function(x) x.MathChannel = UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eProgressionLoss) + 1
    If Not PavarotChartSettings.Any(Function(x) x.MathChannel = UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eProgressionLoss) Then
      PavarotChartSettings.Insert(0, New clsPavarotChartSettings("", UserControlPavarotPlotViewModelMathPlots.eCanaleCustom.eProgressionLoss, False))
      Posizione = 1
    End If
    For Each c In ChartPavarotLossAcqua()
      If PavarotChartSettings.Any(Function(x) x.MathChannel = c) Then Continue For
      PavarotChartSettings.Insert(Math.Min(Posizione, PavarotChartSettings.Count), New clsPavarotChartSettings("", c, False))
      Posizione += 1
    Next
  End Sub

  Private Function ReversedSign(IdCanale As String) As Boolean
    Select Case IdCanale.ToLower
      Case "awa", "twa", "heel", "rdr", "rdrangle"
        Return True
      Case Else
        Return False
    End Select
  End Function

  Private Function CanaliPavarotStandard() As List(Of String)
    Dim l As New List(Of String)
    l.Add("Tws")
    l.Add("Vmg")
    l.Add("Sow")
    l.Add("Twa")
    l.Add("Awa")
    l.Add("Aws")
    l.Add("Rudder")
    l.Add("Yrt")
    Return l
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsRaceReport
  Public Property RaceTimeRange As clsTimeRange
  Public Property RaceStart As clsExpeditionStart
  Public Property Periods As New List(Of clsPeriod2021)

  Public Sub New(RaceTimeRange As clsTimeRange)
    Me.RaceTimeRange = RaceTimeRange
    ImpostaPeriodi()
  End Sub

  Private Sub ImpostaPeriodi()
    If ExpStarts.StartsList.Count = 0 Then
      ExpStarts.CercaPartenze()
    End If
    ' seleziono la partenza piu' vicina al time range
    RaceStart = ExpStarts.StartsList.OrderBy(Function(x) Math.Abs(x.StartTime.Subtract(RaceTimeRange.Start).TotalSeconds)).FirstOrDefault
    Periods.Clear()
    ' creo i periodi della regata
    Dim chSailState = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSailingState)
    If Not chSailState Is Nothing Then
      Dim PrevSailingState As clsPeriodsManager2021.eRowType = chSailState.Valori(RaceTimeRange.IdRigaIniziale)
      Dim CurrentTR As New clsTimeRange(RaceStart.StartTime, RaceTimeRange.Finish)
      For i As Integer = DataProvider2020.TrovaIndice(RaceStart.StartTime) To RaceTimeRange.IdRigaFinale
        Dim SailingState As clsPeriodsManager2021.eRowType = chSailState.Valori(i)
        If Not SailingState = PrevSailingState Then
          CurrentTR.Finish = DataProvider2020.Momento(i)
          Periods.Add(New clsPeriod2021(CurrentTR.Clone, SeaStateToPeriodType(SailingState)))
          CurrentTR.Start = DataProvider2020.Momento(i + 1)
        End If
        PrevSailingState = SailingState
      Next
    End If
    Stop
  End Sub

  Private Function SeaStateToPeriodType(SeaState As clsPeriodsManager2021.eRowType) As clsPeriod2021.ePeriodType
    Select Case SeaState
      Case clsPeriodsManager2021.eRowType.eUndefined
        Return clsPeriod2021.ePeriodType.eUndefined
      Case clsPeriodsManager2021.eRowType.eStraightLineVmg
        Return clsPeriod2021.ePeriodType.eStraightLineVmg
      Case clsPeriodsManager2021.eRowType.eStraightLineReaching
        Return clsPeriod2021.ePeriodType.eStraightLineReaching
      Case clsPeriodsManager2021.eRowType.eTack
        Return clsPeriod2021.ePeriodType.eTack
      Case clsPeriodsManager2021.eRowType.eGybe
        Return clsPeriod2021.ePeriodType.eGybe
      Case clsPeriodsManager2021.eRowType.eTackAndBearAway
        Return clsPeriod2021.ePeriodType.eTack
      Case clsPeriodsManager2021.eRowType.eGybeAndRoundUp
        Return clsPeriod2021.ePeriodType.eGybe
      Case clsPeriodsManager2021.eRowType.eDownwindTack
        Return clsPeriod2021.ePeriodType.eTack
      Case clsPeriodsManager2021.eRowType.eUpwindGybe
        Return clsPeriod2021.ePeriodType.eGybe
      Case clsPeriodsManager2021.eRowType.eRoundUp
        Return clsPeriod2021.ePeriodType.eRoundUp
      Case clsPeriodsManager2021.eRowType.eBearAway
        Return clsPeriod2021.ePeriodType.eBearAway
      Case clsPeriodsManager2021.eRowType.eRoundUpAndTack
        Return clsPeriod2021.ePeriodType.eRoundUp
      Case clsPeriodsManager2021.eRowType.eBearAwayAndGybe
        Return clsPeriod2021.ePeriodType.eBearAway
      Case clsPeriodsManager2021.eRowType.eNotValidManoeuver
        Return clsPeriod2021.ePeriodType.eUndefined
      Case clsPeriodsManager2021.eRowType.eHeadingInstability
        Return clsPeriod2021.ePeriodType.eUndefined
      Case Else
        Return clsPeriod2021.ePeriodType.eTack
    End Select
  End Function


End Class


<AddINotifyPropertyChangedInterface>
Public Class clsPeriodsManager2021
  Public Periods As New clsPeriods2021
  Public TempPeriods As New clsPeriods2021
  Public FileFullName As String

  Public Event SelectionChanged(SelectedPeriods As List(Of clsPeriods2021))

  Public Enum eRowType
    eUndefined = 0
    eStraightLineVmg = 20
    eStraightLineReaching = 18
    eTack = 15
    eGybe = 16
    eTackAndBearAway = 14
    eGybeAndRoundUp = 17
    eDownwindTack = 18
    eUpwindGybe = 13
    eRoundUp = 10
    eBearAway = 9
    eRoundUpAndTack = 11
    eBearAwayAndGybe = 12
    eNotValidManoeuver = 2
    eHeadingInstability = 1
  End Enum

  Public ReadOnly Property SelectedPeriod As clsPeriod2021
    Get
      For Each Periodo In Periods.Lista
        If Periodo.IsSelected Then Return Periodo
      Next
      Return Nothing
    End Get
  End Property

  Public ReadOnly Property SelectedPeriods As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.IsSelected = True).ToList
    End Get
  End Property

  Public ReadOnly Property SelectedPeriodsTimeRange As clsTimeRange
    Get
      Dim SP = SelectedPeriods
      If SP Is Nothing Then Return Nothing
      If SP.Count = 0 Then Return Nothing
      Return New clsTimeRange(SP.Select(Function(x) x.TR.Start).Min, SP.Select(Function(x) x.TR.Finish).Max)
    End Get
  End Property

  Public ReadOnly Property ListaPavarot As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe OrElse x.PeriodType = clsPeriod2021.ePeriodType.eTack).ToList
    End Get
  End Property

  Public ReadOnly Property ListaStraightLine As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg OrElse x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).ToList
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineVmg As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).ToList
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineVmgUpwind As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg AndAlso x.IsUpwindVmgRange).ToList
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineVmgDownwind As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg AndAlso x.IsDownwindVmgRange).ToList
    End Get
  End Property

  Public ReadOnly Property ListaStraightLineReaching As List(Of clsPeriod2021)
    Get
      Return Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).ToList
    End Get
  End Property

  Dim _CollectionStraightLineVmg As New ObservableCollection(Of clsPeriod2021)

  Public Property CollectionStraightLineVmg As ObservableCollection(Of clsPeriod2021)
    Get
      _CollectionStraightLineVmg.Clear()
      'Dim L = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start).ToList
      Dim L = Periods.CollectionStraightLineAll.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start).ToList
      For Each p In L
        _CollectionStraightLineVmg.Add(p)
      Next
      Return _CollectionStraightLineVmg
      'Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg))

      'Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg)
      'If r.ToList.Count = 0 Then Return Nothing
      'Return r
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      _CollectionStraightLineVmg = value
    End Set
  End Property

  Public ReadOnly Property CollectionStraightLineVmgSelected As ObservableCollection(Of clsPeriod2021)
    Get
      _CollectionStraightLineVmg.Clear()
      'Dim L = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start).ToList
      Dim L = Periods.CollectionStraightLineVmgUp.Where(Function(x) x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
      For Each p In L
        'For Each p In Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg AndAlso x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
        _CollectionStraightLineVmg.Add(p)
      Next
      L = Periods.CollectionStraightLineVmgDn.Where(Function(x) x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
      For Each p In L
        'For Each p In Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg AndAlso x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
        _CollectionStraightLineVmg.Add(p)
      Next
      Return _CollectionStraightLineVmg
    End Get
  End Property

  Dim _CollectionStraightLine As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionStraightLine As ObservableCollection(Of clsPeriod2021)
    Get
      _CollectionStraightLine.Clear()
      'Dim L = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start).ToList
      Dim L = Periods.CollectionStraightLineAll.OrderBy(Function(x) x.TR.Start).ToList
      For Each p In L
        _CollectionStraightLine.Add(p)
      Next
      Return _CollectionStraightLine
      'Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg))

      'Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg)
      'If r.ToList.Count = 0 Then Return Nothing
      'Return r
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      _CollectionStraightLine = value
    End Set
  End Property

  Public ReadOnly Property CollectionStraightLineSelected As ObservableCollection(Of clsPeriod2021)
    Get
      _CollectionStraightLine.Clear()
      'Dim L = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start).ToList
      Dim L = Periods.CollectionStraightLineAll.Where(Function(x) x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
      For Each p In L
        'For Each p In Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg AndAlso x.IsChecked = True).OrderBy(Function(x) x.TR.Start).ToList
        _CollectionStraightLine.Add(p)
      Next
      Return _CollectionStraightLine
    End Get
  End Property

  Public ReadOnly Property CollectionStraightLineReaching As ObservableCollection(Of clsPeriod2021)
    Get
      Return New ObservableCollection(Of clsPeriod2021)(Periods.CollectionStraightLineAll.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching))
      'Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching))
      'Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).AsQueryable
      'If r.ToList.Count = 0 Then Return Nothing
      'Return r.AsEnumerable
    End Get
  End Property

  Public ReadOnly Property CollectionStraightLineReachingSelected As ObservableCollection(Of clsPeriod2021)
    Get
      Return New ObservableCollection(Of clsPeriod2021)(Periods.CollectionStraightLineReaching.Where(Function(x) x.IsChecked = True))
      'Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching AndAlso x.IsChecked = True))
      'Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).AsQueryable
      'If r.ToList.Count = 0 Then Return Nothing
      'Return r.AsEnumerable
    End Get
  End Property

  Public ReadOnly Property CollectionPavarot As ObservableCollection(Of clsPeriod2021)
    Get
      Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe OrElse x.PeriodType = clsPeriod2021.ePeriodType.eTack))
      Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe OrElse x.PeriodType = clsPeriod2021.ePeriodType.eTack).AsQueryable
      If r.ToList.Count = 0 Then Return Nothing
      Return r.AsEnumerable
    End Get
  End Property

  Public ReadOnly Property CollectionTack As ObservableCollection(Of clsPeriod2021)
    Get
      Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack))
      Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).AsQueryable
      If r.ToList.Count = 0 Then Return Nothing
      Return r.AsEnumerable
    End Get
  End Property

  Public ReadOnly Property CollectionGybe As ObservableCollection(Of clsPeriod2021)
    Get
      Return New ObservableCollection(Of clsPeriod2021)(Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe))
      Dim r = Periods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe).AsQueryable
      If r.ToList.Count = 0 Then Return Nothing
      Return r.AsEnumerable
    End Get
  End Property

  Public ReadOnly Property PeriodId(Periodo As clsPeriod2021) As Integer
    Get
      Return Periods.Lista.IndexOf(Periodo)
    End Get
  End Property

  Public Property VmgPercentage As clsVmgPercentage


  Public Sub New()

  End Sub

  'Public Sub New(Folder As String)
  '  ReloadPeriods(Folder)
  'End Sub


  Public Sub ImportaPeriodiDaTestFile()
    Dim UltimoPath As String = AppConfig.ActiveProfile.LastPpfFolder
    Dim SelFileName As String
    Dim SelectedFile As String = ObjFiles.SelezionaFile(UltimoPath, "Select test File", "Test File|*.tst|Test Files|*.tst|All Files|*.*", "tst", SelFileName)


    If (Not SelectedFile Is Nothing AndAlso System.IO.File.Exists(SelectedFile) AndAlso (SelectedFile.Contains("Test") OrElse SelectedFile.Contains("test"))) Then
      Dim fi = New System.IO.FileInfo(SelectedFile)
      Dim tt = ObjFiles.TestoInLista(SelectedFile)
      Dim strD As String = fi.Name.Replace("Test", "").Replace("test", "").Replace(fi.Extension, "")
      If strD.Length = 8 Then
        Dim Data As DateTime = New DateTime(strD.Substring(0, 4), strD.Substring(4, 2), strD.Substring(6, 2), 0, 0, 0)
        For Each t As String In tt
          If Not t.Trim = "" Then
            Dim msg As String() = t.Split(" ")
            Dim TestFrom As DateTime
            Dim TestTo As DateTime
            If msg(0).Length = 17 Then
              TestFrom = New DateTime(Data.Year, Data.Month, Data.Day, msg(0).Split("-")(0).Split(":")(0), msg(0).Split("-")(0).Split(":")(1), msg(0).Split("-")(0).Split(":")(2))
              TestTo = New DateTime(Data.Year, Data.Month, Data.Day, msg(0).Split("-")(1).Split(":")(0), msg(0).Split("-")(1).Split(":")(1), msg(0).Split("-")(1).Split(":")(2))
            ElseIf msg(0).Length = 11 Then
              TestFrom = New DateTime(Data.Year, Data.Month, Data.Day, msg(0).Split("-")(0).Split(":")(0), msg(0).Split("-")(0).Split(":")(1), 0)
              TestTo = New DateTime(Data.Year, Data.Month, Data.Day, msg(0).Split("-")(1).Split(":")(0), msg(0).Split("-")(1).Split(":")(1), 0)
            Else
              Exit For
            End If
            Dim TR = New clsTimeRange(TestFrom, TestTo)
            Dim TestName As String = ""
            For i = 1 To msg.Length - 1
              TestName &= msg(i) & " "
            Next
            TestName = TestName.TrimEnd(" ")
            Dim cTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
            Dim Intv = DataProvider2020.ValoriIntervallo(cTwa, TR, Nothing, Nothing, True, True)
            If Not Intv Is Nothing Then
              Dim AvgTwa As Double = Intv.Where(Function(x) Not Double.IsNaN(x)).Average
              Dim PT As clsPeriod2021.ePeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg
              If AvgTwa > 60 AndAlso AvgTwa < 1120 Then
                PT = clsPeriod2021.ePeriodType.eStraightLineReaching
              End If
              Dim np = New clsPeriod2021(TR, PT)
              np.Keys = TestName
              np.UpdateDetails()
              Periods.Lista.Add(np)
            End If
          End If
        Next
        Periods.AggiornaCollections()
      End If
    End If


  End Sub




  'Public Sub ReloadPeriods(Folder As String)
  '  Periods.Lista.Clear()
  '  AddNewPeriods(Folder)
  '  Periods.AggiornaCollections()
  'End Sub

  'Public Sub AddNewPeriods(Folder As String)
  '  If System.IO.Directory.Exists(Folder) Then
  '    Dim d As New System.IO.DirectoryInfo(Folder)
  '    FileFullName = System.IO.Path.Combine(Folder, d.Name & "_Prd.json")
  '    For Each fl In d.GetFiles
  '      If fl.Extension = ".json" Then
  '        Dim ptmp = clsKillerSeriale.LoadConfigurationGeneric(Of clsPeriods2021)(fl.FullName)
  '        If Not ptmp Is Nothing Then
  '          If ptmp.Type = "PeriodsV2" Then
  '            For Each p In ptmp.Lista
  '              AddIfNew(p, False, False) ', True)
  '            Next
  '          End If
  '        End If
  '      End If
  '    Next
  '  End If

  'End Sub

  'Public Sub AddIfNew(Period As clsPeriod2021, AggiornaCollections As Boolean, AggiornaDettagli As Boolean) ', VerificaTimeRange As Boolean)
  '  If Periods.Lista.Where(Function(x) x.TR.StringaPeriodo = Period.TR.StringaPeriodo).Count = 0 Then
  '    If AggiornaDettagli Then Period.UpdateDetails()
  '    Periods.Lista.Add(Period)
  '    If AggiornaCollections Then Periods.AggiornaCollections()
  '  End If
  'End Sub

  'aaaaaaaaaaaaaaaaaaaa


  Public Sub ReloadPeriods(folder As String)
    Periods.Lista.Clear()
    AddNewPeriods(folder)
    Periods.AggiornaCollections()
  End Sub

  Public Sub AddNewPeriods(folder As String)
    If Not System.IO.Directory.Exists(folder) Then Exit Sub

    Dim d As New System.IO.DirectoryInfo(folder)
    FileFullName = System.IO.Path.Combine(folder, d.Name & "_Prd.json")

    ' Precarica le chiavi già presenti per evitare O(n^2)
    Dim existing As New HashSet(Of String)(
        Periods.Lista.
            Where(Function(x) x IsNot Nothing AndAlso x.TR IsNot Nothing).
            Select(Function(x) x.TR.StringaPeriodo),
        StringComparer.OrdinalIgnoreCase
    )

    For Each path In System.IO.Directory.EnumerateFiles(folder, "*.json", System.IO.SearchOption.TopDirectoryOnly)
      'Dim ptmp = clsKillerSeriale.LoadConfigurationGeneric(Of clsPeriods2021)(path)
      Dim ptmp As clsPeriods2021
      clsKillerSeriale.TryLoadConfiguration(Of clsPeriods2021)(path, ptmp)
      If ptmp Is Nothing Then Continue For
      If Not String.Equals(ptmp.Type, "PeriodsV2", StringComparison.OrdinalIgnoreCase) Then Continue For
      If ptmp.Lista Is Nothing Then Continue For

      For Each p In ptmp.Lista
        AddIfNewFast(p, existing) ' no UpdateDetails, no AggiornaCollections (bulk)
      Next
    Next
  End Sub

  ' Mantieni la tua API pubblica
  Public Sub AddIfNew(period As clsPeriod2021, aggiornaCollections As Boolean, aggiornaDettagli As Boolean)
    Dim key = GetPeriodKey(period)
    If key Is Nothing Then Exit Sub

    If Not Periods.Lista.Any(Function(x) x IsNot Nothing AndAlso x.TR IsNot Nothing AndAlso
                                  String.Equals(x.TR.StringaPeriodo, key, StringComparison.OrdinalIgnoreCase)) Then
      If aggiornaDettagli Then period.UpdateDetails()
      Periods.Lista.Add(period)
      If aggiornaCollections Then Periods.AggiornaCollections()
    End If
  End Sub

  ' --- Helpers privati ---

  Private Sub AddIfNewFast(period As clsPeriod2021, existing As HashSet(Of String))
    Dim key = GetPeriodKey(period)
    If key Is Nothing Then Exit Sub
    If existing.Add(key) Then
      Periods.Lista.Add(period)
    End If
  End Sub

  Private Function GetPeriodKey(period As clsPeriod2021) As String
    If period Is Nothing OrElse period.TR Is Nothing Then Return Nothing
    Return period.TR.StringaPeriodo
  End Function


  'fffffffffffffff

  Public Sub AddIfNew(Period As clsPeriod2021)
    AddIfNew(Period, True, True) ', True)
  End Sub


  Public Sub AddToTempIfNew(Period As clsPeriod2021)
    AddToTempIfNew(Period, False)
  End Sub

  ' chiavi (StringaPeriodo) dei periodi gia' in TempPeriods: evita la scansione lineare con formattazione di
  ' una stringa per ogni elemento a ogni inserimento (quadratica, pesantissima con migliaia di periodi).
  ' Si ricostruisce da sola se la lista e' stata modificata altrove (Clear/Remove/Add diretti): cambia il conteggio.
  Private _ChiaviTemp As HashSet(Of String) = Nothing

  Public Sub AddToTempIfNew(Period As clsPeriod2021, AggiornaCollections As Boolean)
    If _ChiaviTemp Is Nothing OrElse _ChiaviTemp.Count <> TempPeriods.Lista.Count Then
      _ChiaviTemp = New HashSet(Of String)
      For Each p In TempPeriods.Lista
        _ChiaviTemp.Add(p.TR.StringaPeriodo)
      Next
    End If
    Dim Chiave As String = Period.TR.StringaPeriodo
    If Not _ChiaviTemp.Contains(Chiave) Then
      'Period.UpdateDetails()
      TempPeriods.Lista.Add(Period)
      _ChiaviTemp.Add(Chiave)
      If AggiornaCollections Then TempPeriods.AggiornaCollections()
    End If
  End Sub

  Public Sub AggiornaCollections()
    TempPeriods.AggiornaCollections()
  End Sub

  Public Sub Elimina(Periodo As clsPeriod2021, SalvaJson As Boolean)
    Periods.Lista.Remove(Periodo)
    Periods.AggiornaCollections()
    If SalvaJson Then SalvaPeriodiJsonFile()
  End Sub

  Public Sub SalvaPeriodiJsonFile()
    clsKillerSeriale.SaveConfigurationGeneric(Of clsPeriods2021)(Periods, FileFullName)
    SpostaJsonOriginali()
  End Sub

  ' salva solo il file dei periodi (senza spostare gli altri json): serve a rendere permanenti i dettagli ricalcolati
  ' si serializza prima di scrivere: SaveConfigurationGeneric svuota il file all'apertura e, se la serializzazione fallisce, lo lascia vuoto
  Public Function SalvaDettagliPeriodi() As Boolean
    If String.IsNullOrEmpty(FileFullName) Then Return False
    Try
      Dim Setting As New Newtonsoft.Json.JsonSerializerSettings
      Setting.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
      Dim Json As String = Newtonsoft.Json.JsonConvert.SerializeObject(Periods, Newtonsoft.Json.Formatting.Indented, Setting)
      System.IO.File.WriteAllText(FileFullName, Json)
      Return True
    Catch ex As Exception
      Return False
    End Try
  End Function

  Public Sub SpostaJsonOriginali()
    Dim cartella = New System.IO.FileInfo(FileFullName).Directory
    For Each fl In cartella.GetFiles.ToList()
      If Not fl.FullName = FileFullName Then
        Dim f = New System.IO.FileInfo(fl.FullName)
        If f.Extension = ".json" Then
          Dim newFld = System.IO.Path.Combine(f.Directory.FullName, "JsonOrg")
          If Not System.IO.Directory.Exists(newFld) Then
            System.IO.Directory.CreateDirectory(newFld)
          End If
          Dim newPos = System.IO.Path.Combine(newFld, fl.Name)
          System.IO.File.Move(fl.FullName, newPos)
        End If
      End If
    Next
  End Sub


  Public Function CalculatesSailingState() As Double()
    Return CalculatesSailingState(DataProvider2020.TimeRange)
  End Function

  ''' <summary>
  ''' True se la riga supera tutte e tre le soglie minime di qualita'. Soglia 0 = filtro spento.
  ''' </summary>
  Private Function RigaSuperaSoglieQualita(Riga As Integer, ChEnvQ As clsChannel2020, ChAttQ As clsChannel2020, ChPerfQ As clsChannel2020,
                                           MinEnvQ As Integer, MinAttQ As Integer, MinPerfQ As Integer) As Boolean
    If MinEnvQ > 0 Then
      If ChEnvQ Is Nothing Then Return False
      Dim v As Double = ChEnvQ.Valori(Riga)
      If Double.IsNaN(v) OrElse v < MinEnvQ Then Return False
    End If
    If MinAttQ > 0 Then
      If ChAttQ Is Nothing Then Return False
      Dim v As Double = ChAttQ.Valori(Riga)
      If Double.IsNaN(v) OrElse v < MinAttQ Then Return False
    End If
    If MinPerfQ > 0 Then
      If ChPerfQ Is Nothing Then Return False
      Dim v As Double = ChPerfQ.Valori(Riga)
      If Double.IsNaN(v) OrElse v < MinPerfQ Then Return False
    End If
    Return True
  End Function

  ''' <summary>
  ''' Aggiunge il periodo rispettando durata minima e massima.
  ''' Sotto la minima il tratto viene scartato. Sopra la massima viene spezzato in tronconi
  ''' contigui, evitando pero' code piu' corte della durata minima: se il residuo non
  ''' raggiungerebbe il minimo, il periodo si chiude allungato fino alla fine del tratto.
  ''' </summary>
  Private Sub AggiungiPeriodoConDurata(TR As clsTimeRange, Tipo As clsPeriod2021.ePeriodType)
    Dim PF As clsPeriodsFinderSettings = AppConfig.ActiveProfile.PeriodsFinderSettings
    PF.NormalizzaValoriMancanti()
    Dim MinSec As Integer = PF.MinPeriodSeconds
    Dim MaxSec As Integer = PF.MaxPeriodSeconds

    Dim Durata As Double = TR.Finish.Subtract(TR.Start).TotalSeconds
    If Durata < MinSec Then Exit Sub

    Dim Inizio As DateTime = TR.Start
    Do
      Dim Residuo As Double = TR.Finish.Subtract(Inizio).TotalSeconds

      If Residuo <= MaxSec + MinSec Then
        ' ci sta tutto, oppure tagliando a MaxSec resterebbe una coda sotto il minimo:
        ' meglio un periodo un po' piu' lungo del massimo che una coda da scartare
        AddToTempIfNew(New clsPeriod2021(New clsTimeRange(Inizio, TR.Finish), Tipo))
        Exit Do
      End If

      Dim Taglio As DateTime = Inizio.AddSeconds(MaxSec)
      AddToTempIfNew(New clsPeriod2021(New clsTimeRange(Inizio, Taglio), Tipo))
      Inizio = Taglio
    Loop
  End Sub

  Public Function CalculatesSailingState(TR As clsTimeRange) As Double()
    Dim ValidRotationMinAngle As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.ValidRotationMinAngle ' 60
    Dim RotationYrt As Double = AppConfig.ActiveProfile.PeriodsFinderSettings.RotationYrt ' 6
    Dim RotationMovingAverageSeconds As Double = AppConfig.ActiveProfile.PeriodsFinderSettings.RotationMovingAverageSeconds ' 6
    Dim SecAnteTack As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteTack ' 15
    Dim SecPostTack As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostTack ' 30
    Dim SecAnteGybe As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteGybe ' 15
    Dim SecPostGybe As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostGybe ' 30
    Dim SecAnteRot As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecAnteRot ' 5
    Dim SecPostRot As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.SecPostRot '10
    Dim MinDataQualityIndex As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.MinDataQualityIndex ' 3
    Dim MaxTwaDeltaUpDn As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.MaxTwaDeltaUpDn '10
    Dim MaxBsPolPercDelta As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.MaxBsTgtPercDelta ' 15
    Return CalculatesSailingState(TR, ValidRotationMinAngle, RotationYrt, SecAnteTack, SecPostTack, SecAnteGybe, SecPostGybe, SecAnteRot, SecPostRot, MinDataQualityIndex, MaxTwaDeltaUpDn, MaxBsPolPercDelta)
  End Function

  Public Function CalculatesSailingState(TR As clsTimeRange, ValidRotationMinAngle As Integer, RotationYrt As Double, SecAnteTack As Integer, SecPostTack As Integer, SecAnteGybe As Integer, SecPostGybe As Integer, SecAnteRot As Integer, SecPostRot As Integer, MinDataQualityIndex As Integer, MaxTwaDeltaUpDn As Integer, MaxBsPolPercDelta As Integer) As Double()

    ' marca le righe come in rotazione se yrt superiore al minimo
    ' delle righe a yrt > minimo segna come in rotazione anche le x precedenti e le y successive


    ' individua i cambi di mura
    ' valide se delta hdg sopra al valore minimo, le marca come tack o gybes, il resto rimane undefined o rotazione

    'rimangono ora le righe undefined alle quali viene assegnato un valore
    ' StarightLineVmg se hanno dataqualityindex >= 3
    ' StraightLineReaching se < 3 Bs PolarPer > minimo et < massimo


    Dim SwSS As Stopwatch = Stopwatch.StartNew()
    clsLogTempi.Scrivi("  SailingState: inizio (canali TWA/YRT)")
    Dim IdInizio As Integer = DataProvider2020.TrovaIndice(TR.Start)
    Dim IdFine As Integer = DataProvider2020.TrovaIndice(TR.Finish)
    Dim Valori(DataProvider2020.TimeStamps.Count - 1) As eRowType
    Dim g As Integer = 0
    Dim t As Integer = 0
    Dim TwaChannel As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim WasStbd As Boolean = TwaChannel.PrimoValoreNotNan(IdInizio)
    Dim YrtChannel As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim MmYrt As New clsMediaMobile(10, False)
    Dim prevIsRot As Boolean = False
    If YrtChannel Is Nothing Then
      Dim v(DataProvider2020.TimeStamps.Count - 1) As Double
      Return v
    End If
    clsLogTempi.Scrivi("  SailingState: canali pronti, " & SwSS.ElapsedMilliseconds & " ms; inizio ciclo rotazioni")
    For i As Integer = IdInizio To IdFine
      ' ogni volta che la barca supera il valore di yrt marco le x righe precedenti
      Dim Twa As Double = TwaChannel.Valori(i)
      Dim yr As Double = MmYrt.SetAndGet(YrtChannel.Valori(i))
      If Not Double.IsNaN(yr) AndAlso DataProvider2020.TrovaIndice(DataProvider2020.Momento(i)) > 0 Then
        ' gli indici di inizio/fine servono solo nelle rotazioni e subito dopo: si calcolano a richiesta
        ' (prima erano due ricerche binarie su 750mila righe per ogni riga, anche quando non servivano)
        Dim inizioTmp As Integer = -1
        Dim fineTmp As Integer = -1
        If Math.Abs(yr) > RotationYrt OrElse prevIsRot Then
          inizioTmp = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(-SecAnteRot)) '' SecAnteRot))
          fineTmp = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(SecPostRot))
        End If
        If Math.Abs(yr) > RotationYrt Then
          Dim IsRU As Boolean = Math.Abs(TwaChannel.PrimoValoreNotNan(inizioTmp)) > Math.Abs(TwaChannel.PrimoValoreNotNan(fineTmp))
          If IsRU Then
            Valori(i) = eRowType.eRoundUp
          Else
            Valori(i) = eRowType.eBearAway
          End If
          For ii As Integer = inizioTmp To i - 1
            Valori(ii) = Valori(i)
          Next
          prevIsRot = True
        Else
          If prevIsRot Then
            ' entra qui se non e' piu' oltre il limite yrt ma lo era
            ' quindi marca le x righe successive come in rotazione
            Dim RT As eRowType = Valori(i - 1)
            For ii As Integer = i To fineTmp
              Valori(ii) = RT
            Next
          End If
          Valori(i) = eRowType.eUndefined
          prevIsRot = False
        End If
      Else
        Valori(i) = eRowType.eUndefined
      End If
    Next

    'Dim chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    'If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    'If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    'Dim Hz As Integer = 1
    'Dim Samples As Integer = 10 * Hz
    'For i As Integer = Math.Max(Samples / 2, IdInizio) To Math.Max(IdFine, -Samples

    'Next



    'Clipboard.SetText(String.Join(vbCrLf, Valori))
    ' quando cambiano le mura sovrascive la rotazione con la strambata o virata se esistono
    clsLogTempi.Scrivi("  SailingState: ciclo rotazioni finito, " & SwSS.ElapsedMilliseconds & " ms; inizio ciclo virate/strambate")
    For i As Integer = IdInizio To IdFine
      Dim Twa As Double = TwaChannel.Valori(i)
      If Not Double.IsNaN(Twa) Then ' AndAlso Not Double.IsNaN(BsTp) Then
        Dim IsStbd As Boolean = Twa >= 0
        If Not IsStbd = WasStbd Then
          If Math.Abs(TwaChannel.PrimoValoreNotNan(i)) > 90 Then
            ' strambata
            Dim inizioTmp As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(-SecAnteGybe))
            Dim fineTmp As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(SecPostGybe))
            For ii As Integer = inizioTmp To fineTmp
              Valori(ii) = eRowType.eGybe
            Next
            g += 1
          Else
            ' virata
            Dim inizioTmp As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(-SecAnteTack))
            Dim fineTmp As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(SecPostTack))
            For ii As Integer = inizioTmp To fineTmp
              Valori(ii) = eRowType.eTack
            Next
            t += 1
          End If
        End If
        WasStbd = Twa >= 0
      End If
    Next

    'Clipboard.SetText(String.Join(vbCrLf, Valori))

    ' i tre indici di qualita' filtrano la singola riga, ciascuno con la propria soglia minima.
    ' Soglia 0 = filtro disattivato.
    clsLogTempi.Scrivi("  SailingState: ciclo virate/strambate finito, " & SwSS.ElapsedMilliseconds & " ms; inizio canali qualita'")
    AppConfig.ActiveProfile.PeriodsFinderSettings.NormalizzaValoriMancanti()
    Dim MinEnvQ As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.MinEnvironmentQualityIndex
    Dim MinAttQ As Integer = AppConfig.ActiveProfile.PeriodsFinderSettings.MinAttitudeQualityIndex

    Dim ChDq As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eDataQuality)
    Dim ChEnvQ As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
    Dim ChAttQ As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAttitudeQuality)
    Dim chBsPerc = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
    Dim chTwaD = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)
    clsLogTempi.Scrivi("  SailingState: canali qualita' pronti, " & SwSS.ElapsedMilliseconds & " ms; inizio assegnazione righe")
    For i As Integer = IdInizio To IdFine
      If Valori(i) = eRowType.eUndefined Then ' la riga non e' ancora stata definita, ovvero non e' ne una virata, ne una strambata, ne un round up, ne un bear away
        Dim Bspp As Double = chBsPerc.Valori(i)
        Dim Twa As Double = TwaChannel.Valori(i)
        Dim TwaD As Double = chTwaD.Valori(i)
        If Not Double.IsNaN(Bspp) AndAlso Not Double.IsNaN(Twa) AndAlso Not Double.IsNaN(Bspp) Then
          If Math.Abs(TwaD) < MaxTwaDeltaUpDn Then
            If RigaSuperaSoglieQualita(i, ChEnvQ, ChAttQ, ChDq, MinEnvQ, MinAttQ, MinDataQualityIndex) Then
              Valori(i) = eRowType.eStraightLineVmg
            Else
              Valori(i) = eRowType.eUndefined
            End If
          Else
            If Bspp > (100 - MaxBsPolPercDelta) AndAlso Bspp < (100 + MaxBsPolPercDelta) Then ' AndAlso Math.Abs(Twa) > 50 AndAlso Math.Abs(Twa) < 130 Then
              Valori(i) = eRowType.eStraightLineReaching
            Else
              Valori(i) = eRowType.eUndefined
            End If
          End If

        End If

        'Dim v As Double = ChDq.Valori(i)
        'If Not Double.IsNaN(v) Then

        '    Dim Bspp As Double = chBsPerc.Valori(i)
        '    Dim Twa As Double = TwaChannel.Valori(i)
        '    Dim TwaD As Double = chTwaD.Valori(i)
        '    Dim IsValidVmg As Boolean = v >= MinDataQualityIndex ' il quality index dipende dalla vmgp quindi esclude i periodi non optimal vmg
        '    If IsValidVmg Then
        '        Valori(i) = eRowType.eStraightLineVmg
        '    ElseIf Not Double.IsNaN(Bspp) AndAlso Bspp > (100 - MaxBsPolPercDelta) AndAlso Bspp < (100 + MaxBsPolPercDelta) Then ' AndAlso Twa > 50 AndAlso Twa < 130 AndAlso Math.Abs(TwaD) > MaxTwaDeltaUpDn Then
        '        Valori(i) = eRowType.eStraightLineReaching
        '    Else
        '        Valori(i) = eRowType.eUndefined
        '    End If
        'End If
      End If
    Next

    'Clipboard.SetText(String.Join(vbCrLf, Valori))

    clsLogTempi.Scrivi("  SailingState: assegnazione righe finita, " & SwSS.ElapsedMilliseconds & " ms; inizio creazione periodi")
    TempPeriods.Lista.Clear()

    ' manca la deselezione delle manovre finte

    Dim LastRT As eRowType = eRowType.eUndefined
    Dim Inizio As Integer = IdInizio
    For i As Integer = IdInizio To IdFine
      Dim CurrentRT As eRowType = Valori(i)
      If Not CurrentRT = LastRT Then
        Dim TRtmp As New clsTimeRange(DataProvider2020.Momento(Inizio), DataProvider2020.Momento(i - 1))
        Select Case LastRT
          Case eRowType.eUndefined
            ' non salva nulla
          Case eRowType.eTack
            Dim NotValid As Boolean = Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) > 90 OrElse Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) > 90
            If NotValid Then
              Dim ert As eRowType = eRowType.eNotValidManoeuver
              If Math.Abs(chTwaD.PrimoValoreNotNan(Inizio)) < MaxTwaDeltaUpDn AndAlso (Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) <= 90) AndAlso (chTwaD.PrimoValoreNotNan(i - 1) > MaxTwaDeltaUpDn) Then
                ' entrata in target bolina uscita bolina larga traverso
                ert = eRowType.eTackAndBearAway
              ElseIf Math.Abs(chTwaD.PrimoValoreNotNan(Inizio)) < MaxTwaDeltaUpDn AndAlso (Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) > 90) Then
                ' entrata in target bolina uscita traverso/poppa
                ert = eRowType.eTackAndBearAway
              ElseIf Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) > 90 OrElse Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) > 90 Then
                ' downwind tack
                ert = eRowType.eDownwindTack
              End If
              For ii As Integer = Inizio To i - 1
                Valori(ii) = ert
              Next
            Else
              Dim a As DateTime = DataProvider2020.Momento(Inizio).AddSeconds(SecAnteTack)
              Dim b As DateTime = DataProvider2020.Momento(i - 1).AddSeconds(-SecPostTack)
              Dim c As Double = b.Subtract(a).TotalSeconds
              Dim km As Date = a.AddSeconds(c / 2)
              AddToTempIfNew(New clsPeriod2021(TRtmp, clsPeriod2021.ePeriodType.eTack, km))
            End If
          Case eRowType.eStraightLineReaching
            AggiungiPeriodoConDurata(TRtmp, clsPeriod2021.ePeriodType.eStraightLineReaching)
          Case eRowType.eStraightLineVmg
            Try
              Dim mi As DateTime = DataProvider2020.Momento(Inizio)
              Dim mf As DateTime = DataProvider2020.Momento(i - 1)
              If mf.Subtract(mi).TotalSeconds < AppConfig.ActiveProfile.PeriodsFinderSettings.MinPeriodSeconds Then
                For ii As Integer = Inizio To i - 1
                  Valori(ii) = eRowType.eUndefined
                Next
              Else
                ' media dei valori non NaN su [Inizio, i-2]. In .NET Framework Skip() scorre tutti gli elementi
                ' precedenti anche su un array (O(n) per periodo): ciclo diretto, stesso risultato.
                ' Se non c'e' nessun valore valido Average() lanciava un'eccezione: si salta il periodo come prima.
                Dim SommaTwaD As Double = 0
                Dim NTwaD As Integer = 0
                Dim UltimoTwaD As Integer = Math.Min(i - 2, chTwaD.Valori.Length - 1)
                For k As Integer = Inizio To UltimoTwaD
                  Dim vD As Double = chTwaD.Valori(k)
                  If Not Double.IsNaN(vD) Then
                    SommaTwaD += vD
                    NTwaD += 1
                  End If
                Next
                If NTwaD = 0 Then Throw New InvalidOperationException("Nessun valore TWAd valido nel periodo")
                Dim DeltaTwaAvg As Double = SommaTwaD / NTwaD
                If Math.Abs(DeltaTwaAvg) < MaxTwaDeltaUpDn Then
                  AggiungiPeriodoConDurata(TRtmp, clsPeriod2021.ePeriodType.eStraightLineVmg)
                Else
                  For ii As Integer = Inizio To i - 1
                    Valori(ii) = eRowType.eStraightLineReaching
                  Next
                  AggiungiPeriodoConDurata(TRtmp, clsPeriod2021.ePeriodType.eStraightLineReaching)
                End If
              End If
            Catch ex As Exception
              Stop
            End Try
          Case eRowType.eRoundUp
            AddToTempIfNew(New clsPeriod2021(TRtmp, clsPeriod2021.ePeriodType.eRoundUp))
          Case eRowType.eGybe
            Dim NotValid As Boolean = Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) < 90 OrElse Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) < 90
            If NotValid Then
              Dim ert As eRowType = eRowType.eNotValidManoeuver
              If Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) <= 90 AndAlso (chTwaD.PrimoValoreNotNan(Inizio) > MaxTwaDeltaUpDn) AndAlso (Math.Abs(chTwaD.PrimoValoreNotNan(i - 1)) < MaxTwaDeltaUpDn) Then
                ' entrata bolina larga/traverso uscita in target
                ert = eRowType.eBearAwayAndGybe
              ElseIf Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) > 90 AndAlso (chTwaD.PrimoValoreNotNan(Inizio) < -MaxTwaDeltaUpDn) AndAlso (Math.Abs(chTwaD.PrimoValoreNotNan(i - 1)) < MaxTwaDeltaUpDn) Then
                ' entrata poppa stretta/traverso uscita in target
                ert = eRowType.eBearAwayAndGybe
              ElseIf Math.Abs(chTwaD.PrimoValoreNotNan(Inizio)) < MaxTwaDeltaUpDn AndAlso Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) <= 90 AndAlso (Math.Abs(chTwaD.PrimoValoreNotNan(i - 1)) < MaxTwaDeltaUpDn) Then
                ' entrata in target uscita bolina target
                ert = eRowType.eGybeAndRoundUp
              ElseIf Math.Abs(TwaChannel.PrimoValoreNotNan(Inizio)) < 90 OrElse Math.Abs(TwaChannel.PrimoValoreNotNan(i - 1)) < 90 Then
                ' upwind gybe
                ert = eRowType.eUpwindGybe
              End If
              For ii As Integer = Inizio To i - 1
                Valori(ii) = ert
              Next
            Else
              Dim a As DateTime = DataProvider2020.Momento(Inizio).AddSeconds(SecAnteGybe)
              Dim b As DateTime = DataProvider2020.Momento(i - 1).AddSeconds(-SecPostGybe)
              Dim c As Double = b.Subtract(a).TotalSeconds
              Dim km As Date = a.AddSeconds(c / 2)
              AddToTempIfNew(New clsPeriod2021(TRtmp, clsPeriod2021.ePeriodType.eGybe, km))
            End If
          Case eRowType.eBearAway
            AddToTempIfNew(New clsPeriod2021(TRtmp, clsPeriod2021.ePeriodType.eBearAway))
        End Select
        Inizio = i
      End If
      LastRT = CurrentRT
    Next
    clsLogTempi.Scrivi("  SailingState: periodi creati (" & TempPeriods.Lista.Count & "), " & SwSS.ElapsedMilliseconds & " ms; aggiorna collections")
    AggiornaCollections()
    clsLogTempi.Scrivi("  SailingState: collections aggiornate, " & SwSS.ElapsedMilliseconds & " ms; aggrega reaching")
    'aggrega reaching brevi
    Dim ptd As New List(Of clsPeriod2021)
    Dim ptadd As New List(Of clsPeriod2021)
    Dim ListaTmp = TempPeriods.Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).OrderBy(Function(x) x.TR.Start).ToList
    If ListaTmp.Count > 0 Then
      Dim PrdTmp As New clsPeriod2021(ListaTmp.First.TR, clsPeriod2021.ePeriodType.eStraightLineReaching)
      For Each p In ListaTmp
        ptd.Add(p) ' mette il periodo nella lista di quelli da eliminare
        Dim ss = p.TR.Start.Subtract(PrdTmp.TR.Finish).TotalSeconds
        If ss > 0 AndAlso ss < 2 Then
          ' tra il periodo precedente e quello attuale ci sono meno di due secondi, aggrega i periodi
          PrdTmp.TR.Finish = p.TR.Finish
        Else
          ' mette il periodo a se stante o aggregato nella lista di quelli da aggiungere se supera i 10 secondi
          If PrdTmp.TR.Durata.TotalSeconds > 10 Then ptadd.Add(PrdTmp)
          PrdTmp = New clsPeriod2021(p.TR, clsPeriod2021.ePeriodType.eStraightLineReaching)
        End If
      Next
      If PrdTmp.TR.Durata.TotalSeconds > 10 Then ptadd.Add(PrdTmp)
      For Each p In ptd
        TempPeriods.Lista.Remove(p)
      Next
      For Each p In ptadd
        TempPeriods.Lista.Add(p)
      Next
    End If

    Dim ValoriCanale(Valori.Count - 1) As Double
    For i As Integer = 0 To Valori.Count - 1
      ValoriCanale(i) = Valori(i)
    Next

    AggiornaCollections()
    clsLogTempi.Scrivi("  SailingState: fine, " & SwSS.ElapsedMilliseconds & " ms")
    Return ValoriCanale

  End Function

  Public Sub AssignKeysToPeriodsInTimeRange(PeriodsType As clsPeriod2021.ePeriodType)
    'quelle selezionate del tipo corrente
    Dim Key As String = InputBox("Keys text", "Keys")
    AssignKeysToPeriodsInTimeRange(Nothing, PeriodsType, Key)
  End Sub

  Public Function DeleteVisibleRangePeriods(TimeRange As clsTimeRange) As Boolean
    Dim pp = PeriodsManager.Periods.Lista.Where(Function(x) TimeRange.IsFullyOverlapped(x.TR)).ToList
    If pp.Count > 0 Then
      If MsgBox("Do you whant to delete all the " & pp.Count & " periods of the selected time range?", vbYesNo) = MsgBoxResult.Yes Then
        For Each p In pp
          PeriodsManager.Periods.Lista.Remove(p)
        Next
        Return True
      End If
    End If
    Return False
  End Function

  Public Sub AssignLegNameToPeriodsInTimeRange(TimeRange As clsTimeRange)
    'tutte quelle nel time range
    Dim Key As String = InputBox("Leg Name", "Leg Name")
    AssignKeysToPeriodsInTimeRange(TimeRange, Nothing, Key)
  End Sub
  Public Sub AssignRaceNameToPeriodsInTimeRange(TimeRange As clsTimeRange)
    'tutte quelle nel time range
    Dim Key As String = InputBox("Race Name", "Race Name")
    AssignKeysToPeriodsInTimeRange(TimeRange, Nothing, Key)
  End Sub
  Public Sub AssignKeysToPeriodsInTimeRange(TimeRange As clsTimeRange)
    'tutte quelle nel time range
    Dim Key As String = InputBox("Keys text", "Keys")
    AssignKeysToPeriodsInTimeRange(TimeRange, Nothing, Key)
  End Sub

  ''' <summary>Assegna Keys ai periodi indicati e salva il file dei periodi.</summary>
  Public Sub AssegnaKeys(ListaPeriodi As IEnumerable(Of clsPeriod2021), Keys As String)
    For Each P In ListaPeriodi
      P.Keys = Keys
    Next
    SalvaPeriodiJsonFile()
  End Sub

  Public Sub AssignKeysToPeriodsInTimeRange(TimeRange As clsTimeRange, PeriodsType As clsPeriod2021.ePeriodType, Keys As String)
    For Each P In Periods.Lista
      If TimeRange Is Nothing Then
        If P.IsChecked Then
          If PeriodsType = P.PeriodType Then
            P.Keys = Keys
          End If
        End If
      Else
        If P.TR.IsOverlapped(TimeRange) Then
          P.Keys = Keys
        End If
      End If
    Next
    SalvaPeriodiJsonFile()
  End Sub

  Public Sub AssignRaceNameToPeriodsInTimeRange(TimeRange As clsTimeRange, RaceName As String)
    If TimeRange Is Nothing Then Exit Sub
    For Each P In Periods.Lista
      If P.TR.IsOverlapped(TimeRange) Then
        P.RaceName = RaceName
      End If
    Next
    SalvaPeriodiJsonFile()
  End Sub

  Public Sub AssignLegNameToPeriodsInTimeRange(TimeRange As clsTimeRange, LegName As String)
    If TimeRange Is Nothing Then Exit Sub
    For Each P In Periods.Lista
      If P.TR.IsOverlapped(TimeRange) Then
        P.LegName = LegName
      End If
    Next
    SalvaPeriodiJsonFile()
  End Sub

  Public Sub SplitKeysToRaceLegToPeriodsInTimeRange(TimeRange As clsTimeRange)
    If TimeRange Is Nothing Then Exit Sub
    For Each P In Periods.Lista
      If P.TR.IsOverlapped(TimeRange) Then
        P.SplitKeysToRaceAndLeg()
      End If
    Next
    SalvaPeriodiJsonFile()
  End Sub

  'Public Sub AssignKeysToPeriodsInTimeRangeCustom(TimeRange As clsTimeRange)
  '  Dim PeriodsType As clsPeriod2021.ePeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg
  '  For Each P In Periods.Lista
  '    If PeriodsType = P.PeriodType Then
  '      If P.TR.IsOverlapped(TimeRange) Then
  '        If P.ShortDescription.IndexOf("FTD", 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
  '          P.Keys = "Ftd"
  '        ElseIf P.ShortDescription.IndexOf("Camera", 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
  '          P.Keys = "Camera"
  '        ElseIf P.ShortDescription.IndexOf("Visual", 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
  '          P.Keys = "Visual"
  '        End If
  '      End If
  '    End If
  '  Next
  'End Sub

  'Public Sub AssignKeysToPeriodsInTimeRangeSelectiveChannel(TimeRange As clsTimeRange, Channel As clsChannel2020)
  '  '  Dim Channel As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eStbdCantAngle)
  '  Dim PeriodsType As clsPeriod2021.ePeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg
  '  For Each P In Periods.Lista
  '    If PeriodsType = P.PeriodType Then
  '      If P.TR.IsOverlapped(TimeRange) Then
  '        Dim Vcp As New clsValoriPeriodoCanale2020(Channel, TimeRange, False)
  '        P.Keys = Channel.ShortName & " " & CInt(Vcp.Avg / 2) * 2.ToString("F0")
  '      End If
  '    End If
  '  Next
  'End Sub


  Dim _VmgPercentageCheckedOnly As Boolean
  Public Property VmgPercentageCheckedOnly As Boolean
    Get
      Return _VmgPercentageCheckedOnly
    End Get
    Set(value As Boolean)
      _VmgPercentageCheckedOnly = value
      AggiornaDailyVmgPerc()
    End Set
  End Property

  Public Sub AggiornaDailyVmgPerc()
    If DataProvider2020 Is Nothing Then Exit Sub
    Dim ChTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim ChVmgTp As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    Dim Lista = ListaStraightLineVmg.Where(Function(x) x.IsChecked)
    If Not _VmgPercentageCheckedOnly OrElse Lista.Count = 0 Then
      Lista = ListaStraightLineVmg
    End If
    If Not ChVmgTp Is Nothing AndAlso Not ChTws Is Nothing Then
      If VmgPercentage Is Nothing Then VmgPercentage = New clsVmgPercentage
      ' PeriodsManager.TwsEquivalentAtNorma
      VmgPercentage.AzzeraValori()
      If Not ChVmgTp.Valori Is Nothing AndAlso Not ChTws.Valori Is Nothing Then
        Dim Ultima As Integer = Math.Min(ChTws.Valori.Length, ChVmgTp.Valori.Length) - 1
        For Each periodo In Lista
          ' l'andatura si decide sul valore assoluto: AvgTwa e' negativo con le mure a sinistra e senza il valore
          ' assoluto una poppa con mure a sinistra (es. -150) veniva contata come bolina
          Dim IsUpwind As Boolean = Math.Abs(periodo.AvgTwa) < 90
          For i As Integer = Math.Max(0, periodo.TR.IdRigaIniziale) To Math.Min(Ultima, periodo.TR.IdRigaFinale)
            Dim Tws As Double = ChTws.Valori(i)
            Dim Vmgp As Double = ChVmgTp.Valori(i)
            ' i valori non validi non entrano nelle medie: un solo NaN nella lista rendeva NaN la media del Tws
            If Not Double.IsNaN(Tws) AndAlso Not Double.IsNaN(Vmgp) AndAlso Not Double.IsInfinity(Vmgp) Then
              VmgPercentage.AggiungiCoppia(Tws, Vmgp, IsUpwind, False)
            End If
          Next
        Next
      End If
      ' la descrizione si calcola una sola volta, non a ogni coppia aggiunta
      VmgPercentage.AggiornaDescrizione()
    End If
  End Sub


End Class


'<AddINotifyPropertyChangedInterface>
'Public Class clsPeriodsSettings2021
'    'Public Property SecsInvestment As Double = 30
'    Public Property SecsAnteManoeuver As Double = 10
'    Public Property SecsPostManoeuver As Double = 30
'    'Public Property SamplingSeconds As Double = 5
'    'Public Property Suffisso As String

'    Public Sub New()

'    End Sub

'    'Public Sub New(SecsAnte As Double, SecsPost As Double, SamplingSeconds As Double, SecsInvestment As Double)
'    Public Sub New(SecsAnte As Double, SecsPost As Double)
'        Me.SecsAnteManoeuver = SecsAnte
'        Me.SecsPostManoeuver = SecsPost
'        'Me.SamplingSeconds = SamplingSeconds
'        'Me.SecsInvestment = SecsInvestment
'    End Sub

'End Class



<AddINotifyPropertyChangedInterface>
Public Class clsPeriods2021
  Public Type As String = "PeriodsV2"

  Dim _Lista As New ObservableCollection(Of clsPeriod2021)
  Public Property ListaOrdinata As New ObservableCollection(Of clsPeriod2021)

  Public Property Lista As ObservableCollection(Of clsPeriod2021)
    Get
      Return _Lista
    End Get
    Set(value As ObservableCollection(Of clsPeriod2021))
      _Lista = value
      AggiornaCollections()
    End Set
  End Property

  Public Property CollectionStraightLineAll As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionStraightLineVmgUp As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionStraightLineVmgDn As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionStraightLineReaching As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionPavarot As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionTack As New ObservableCollection(Of clsPeriod2021)
  Public Property CollectionGybe As New ObservableCollection(Of clsPeriod2021)


  Public Sub AggiornaCollections()
    ListaOrdinata.Clear()
    CollectionStraightLineAll.Clear()
    CollectionStraightLineVmgUp.Clear()
    CollectionStraightLineVmgDn.Clear()
    CollectionStraightLineReaching.Clear()
    CollectionPavarot.Clear()
    CollectionTack.Clear()
    CollectionGybe.Clear()
    Dim i As Integer = 0
    For Each p In Lista.OrderBy(Function(x) x.TR.Start)
      If DataProvider2020.IsTimeRangeLoaded(p.TR) Then
        p.Colore = ColoriDifferenziati(i)
        i += 1
        ListaOrdinata.Add(p)
        Select Case p.PeriodType
          Case clsPeriod2021.ePeriodType.eStraightLineVmg
            CollectionStraightLineAll.Add(p)
            If p.IsUpwindVmgRange Then
              CollectionStraightLineVmgUp.Add(p)
            ElseIf p.IsDownwindVmgRange Then
              CollectionStraightLineVmgDn.Add(p)
            End If
          Case clsPeriod2021.ePeriodType.eStraightLineReaching
            CollectionStraightLineAll.Add(p)
            CollectionStraightLineReaching.Add(p)
          Case clsPeriod2021.ePeriodType.eTack
            CollectionPavarot.Add(p)
            CollectionTack.Add(p)
          Case clsPeriod2021.ePeriodType.eGybe
            CollectionPavarot.Add(p)
            CollectionGybe.Add(p)
        End Select
      End If

    Next
    'CollectionStraightLineVmg = Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg).OrderBy(Function(x) x.TR.Start)
    'CollectionStraightLineReaching = Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching).OrderBy(Function(x) x.TR.Start)
    'CollectionPavarot = Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack OrElse x.PeriodType = clsPeriod2021.ePeriodType.eGybe).OrderBy(Function(x) x.TR.Start)
    'CollectionTack = Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eTack).OrderBy(Function(x) x.TR.Start)
    'CollectionGybe = Lista.Where(Function(x) x.PeriodType = clsPeriod2021.ePeriodType.eGybe).OrderBy(Function(x) x.TR.Start)
  End Sub

End Class


<AddINotifyPropertyChangedInterface>
Public Class clsPeriod2021

  '  Dim pIsSelected As Boolean = False
  '  Dim _IsHighlighted As Boolean = False
  '  Dim pIsChecked As Boolean = True

  ' TempVal / StrTempVal / RigheValide erano campi pubblici: WPF puo' fare binding solo
  ' su PROPRIETA', quindi producevano un Data Error 40 per ogni periodo e il valore non
  ' arrivava mai a video.
  <JsonIgnore>
  Public Property TempVal As Double
  <JsonIgnore>
  Public Property StrTempVal As String
  <JsonIgnore>
  Public Property RigheValide As New Dictionary(Of Integer, Boolean)

  Public Property ShortDescription As String = ""
  Public Property ExtendedDescription As String = ""
  Public Property Keys As String = ""

  Public Property RaceName As String = ""
  Public Property LegName As String = ""

  Public Property TR As clsTimeRange
  Public Property KeyMoment As DateTime = Nothing
  Public Property PeriodType As ePeriodType
  Public Property TechnicalOrSystems As Boolean = False
  Public Property IsStbd As Boolean ' stbd entry per tacks and gybes
  Public Property AvgTwa As Double

  '<JsonIgnore>
  'Public Property IsSelected As Boolean = False
  <JsonIgnore>
  Public Property IsHighlighted As Boolean = False
  '<JsonIgnore>
  'Public Property IsChecked As Boolean = True

  Public Property Colore As Color = Colors.Gray
  Public Property ColoreSfondo As Color = Colors.White

  Public Property TwsDetails As New clsChannelStats2021
  Public Property TwdDetails As New clsChannelStats2021
  ''' <summary>Statistiche del TWA sull'intero periodo. AvgTwa invece campiona solo l'inizio.</summary>
  Public Property TwaDetails As New clsChannelStats2021
  ''' <summary>Statistiche della velocita' barca (SOW) sull'intero periodo.</summary>
  Public Property BsDetails As New clsChannelStats2021

  Public Property SeaStateDetails As New clsChannelStats2021

  Public Property StraightLineVmgDetails As clsStarightLineVmgDetails2021
  Public Property StraightLineReachingDetails As clsStarightLineReachingDetails2021
  Public Property PavarotDetails As clsPavarotDetails2021
  Public Property BearAwayDetails As clsBearAwayDetails2021
  Public Property RoundUpDetails As clsRoundUpDetails2021
  Public Property AccelerationDetails As clsAccelerationDetails2021

  'Public Property PavarotStandardRange As Integer = 30
  'Public Property GybeSecAnte As Integer = 10
  'Public Property GybeSecPost As Integer = 30
  'Public Property TackSecAnte As Integer = 10
  'Public Property TackSecPost As Integer = 30

  Dim _IsSelected As Boolean = True
  <JsonIgnore>
  Public Property IsSelected As Boolean
    Get
      Return _IsSelected
    End Get
    Set(value As Boolean)
      _IsSelected = value
    End Set
  End Property

  <JsonIgnore>
  Public ReadOnly Property RaceLegName As String
    Get
      Return RaceName & " " & LegName
    End Get
  End Property

  Dim _IsChecked As Boolean = True
  <JsonIgnore>
  Public Property IsChecked As Boolean
    Get
      Return _IsChecked
    End Get
    Set(value As Boolean)
      _IsChecked = value
    End Set
  End Property

  <JsonIgnore>
  Public Property PavSettings As clsPavarotSettings
    Get
      Return AppConfig.ActiveProfile.PavarotSettings
    End Get
    Set(value As clsPavarotSettings)
      AppConfig.ActiveProfile.PavarotSettings = value
    End Set
  End Property


  Dim _TimeRangeStandardPavarot As New clsTimeRange

  Dim _PeriodTgt As clsValoriPuntoPolare
  <JsonIgnore>
  Public Property PeriodTgt As clsValoriPuntoPolare
    Get
      If _PeriodTgt Is Nothing Then
        If Not TgtManager Is Nothing Then
          If Not TgtManager.Tgt Is Nothing Then
            _PeriodTgt = TgtManager.Tgt.ValoreTgt(Math.Abs(AvgTwa) <= 90, TwsDetails.AvgVal, "bs")
          End If
        End If
      End If
      Return _PeriodTgt
    End Get
    Set(value As clsValoriPuntoPolare)
      _PeriodTgt = value
    End Set
  End Property

  'Public Function StraightLineAvgTwa() As Double
  '  If Not StraightLineVmgDetails Is Nothing Then Return StraightLineVmgDetails.Twa.AvgVal
  '  If Not StraightLineReachingDetails Is Nothing Then Return StraightLineReachingDetails.Twa.AvgVal
  '  Return 0
  'End Function

  Public Function MomentoGpsHegHeadToWind() As DateTime
    Dim chHdgGps = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.ePositionHeading)
    'chHdgGps = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    Dim td = TwdDetails.AvgVal
    If Math.Abs(AvgTwa) > 90 Then td = SommaAngolo180adAngolo360(180, td)
    Dim mt As DateTime = KeyMoment
    If Not chHdgGps Is Nothing AndAlso Not chHdgGps.Valori.Count = 0 Then
      Dim md As Double = DifferenzaAssolutaTraAngoli360(chHdgGps.Valori(IdRigaKeyMoment), td)
      For i As Integer = TR.IdRigaIniziale To TR.IdRigaFinale
        Dim h = chHdgGps.Valori(i)
        If Not h = Nothing Then
          Dim dt = DifferenzaAssolutaTraAngoli360(h, td)
          If dt <= md Then
            md = dt
            mt = DataProvider2020.Momento(i)
          End If
        End If
      Next
    End If
    Return mt
  End Function


  Public Sub SplitKeysToRaceAndLeg()
    ' Usa una regex per riconoscere il formato opzionale + RXXLX
    ' ^\w{0,2}R(\d{2})L(\d)$
    Dim pattern As String = "^(.{0,2})R(\d{2})L(\d)$"
    Dim m As Match = Regex.Match(Keys, pattern)

    If Not m.Success Then
      Throw New ArgumentException("Stringa non valida: non rispetta il formato previsto.")
      Exit Sub
    End If

    ' Ricostruisci le due parti
    RaceName = m.Groups(1).Value & "R" & m.Groups(2).Value
    LegName = "L" & m.Groups(3).Value

  End Sub

  Public ReadOnly Property TimeRangeStandardPavarot As clsTimeRange
    Get
      If PeriodType = ePeriodType.eGybe Then
        _TimeRangeStandardPavarot.Start = KeyMoment.AddSeconds(-PavSettings.GybeSecAnte)
        _TimeRangeStandardPavarot.Finish = KeyMoment.AddSeconds(PavSettings.GybeSecPost)
      ElseIf PeriodType = ePeriodType.eTack Then
        _TimeRangeStandardPavarot.Start = KeyMoment.AddSeconds(-PavSettings.TackSecAnte)
        _TimeRangeStandardPavarot.Finish = KeyMoment.AddSeconds(PavSettings.TackSecPost)
      Else
        Return Nothing
      End If
      Return _TimeRangeStandardPavarot
    End Get
  End Property

  Public ReadOnly Property PavarotEntry As DateTime
    Get
      If PeriodType = ePeriodType.eGybe Then
        Return KeyMoment.AddSeconds(-PavSettings.GybeSecAnte)
      ElseIf PeriodType = ePeriodType.eTack Then
        Return KeyMoment.AddSeconds(-PavSettings.TackSecAnte)
      Else
        Return Nothing
      End If
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IdRigaPavarotEntry As Integer
    Get
      Return DataProvider2020.TrovaIndice(PavarotEntry)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IdRigaPavarotExit As Integer
    Get
      Return DataProvider2020.TrovaIndice(PavarotExit)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IdRigaKeyMoment As Integer
    Get
      Return DataProvider2020.TrovaIndice(KeyMoment)
    End Get
  End Property

  Public ReadOnly Property PavarotExit As DateTime
    Get
      If PeriodType = ePeriodType.eGybe Then
        Return KeyMoment.AddSeconds(PavSettings.GybeSecPost)
      ElseIf PeriodType = ePeriodType.eTack Then
        Return KeyMoment.AddSeconds(PavSettings.TackSecPost)
      Else
        Return Nothing
      End If
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsVmgRange() As Boolean
    Get
      Return IsUpwindVmgRange OrElse IsDownwindVmgRange
    End Get
  End Property
  <JsonIgnore>
  Public ReadOnly Property IsUpwindVmgRange() As Boolean ' andrebbe rivisto quando esiste il target
    Get
      Return (System.Math.Abs(AvgTwa) >= 30 And System.Math.Abs(AvgTwa) <= 65)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsDownwindVmgRange() As Boolean
    Get
      Return (System.Math.Abs(AvgTwa) >= 120 And System.Math.Abs(AvgTwa) <= 165)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property TwsGroup(KnotsRangeOfGroups As Integer) As Double
    Get
      Return System.Math.Round(TwsDetails.AvgVal / KnotsRangeOfGroups, 0) * KnotsRangeOfGroups
    End Get
  End Property

  Public Property KeyMomentChecked As Boolean

  Public Enum ePeriodType
    eUndefined = 0
    eStraightLineVmg = 1
    eStraightLineReaching = 2
    eTack = 3
    eGybe = 4
    eRoundUp = 5
    eBearAway = 6
    eAcceleration = 7
  End Enum

  Public Sub New()
    If AppConfig.ActiveProfile.PavarotSettings Is Nothing Then
      AppConfig.ActiveProfile.PavarotSettings = New clsPavarotSettings
      AppConfig.ActiveProfile.PavarotSettings.ImpostazioniDefault()
      AppConfig.Salva()
    End If
    'PavarotStandardRange = AppConfig.ActiveProfile.PavarotSettings.PavarotStandardRange ' 30
    'GybeSecAnte = AppConfig.ActiveProfile.PavarotSettings.GybeSecAnte ' 10
    'GybeSecPost = AppConfig.ActiveProfile.PavarotSettings.GybeSecPost ' 30
    'TackSecAnte = AppConfig.ActiveProfile.PavarotSettings.TackSecAnte ' 10
    'TackSecPost = AppConfig.ActiveProfile.PavarotSettings.TackSecPost ' 30
  End Sub

  Public Sub New(tr As clsTimeRange, periodtype As ePeriodType)
    Me.TR = tr
    Me.PeriodType = periodtype
  End Sub

  Public Sub New(tr As clsTimeRange, periodtype As ePeriodType, keymoment As DateTime)
    Me.TR = tr
    Me.PeriodType = periodtype
    Me.KeyMoment = keymoment
  End Sub

  Private Function ArrayAvg(Canale As clsChannel2020, idiniziale As Integer, samples As Integer) As Double
    Dim Tmp = Canale.Valori.Skip(idiniziale).Take(samples).Where(Function(x) Not Double.IsNaN(x))
    If Tmp Is Nothing Then Return 0
    If Tmp.Count = 0 Then Return 0
    Return Tmp.Average
  End Function

  Public Sub UpdateDetails()
    Dim i As Integer = TR.IdRigaIniziale
    Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
    Dim SecCampione As Integer = 5
    Dim idfinecampionamento As Integer = DataProvider2020.TrovaIndice(TR.Start.AddSeconds(SecCampione))

    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    AvgTwa = ArrayAvg(chTwa, i, idfinecampionamento - i)
    Dim idi As Integer = i
    'If Not KeyMoment = Nothing Then
    '  idi = DataProvider2020.TrovaIndice(KeyMoment.AddSeconds(-10))
    'End If
    IsStbd = ArrayAvg(chTwa, idi, 5 * DataProvider2020.Hz) >= 0

    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chSeaState As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSeaState)
    Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    TwsDetails.AggiornaStats(chTws, i, l)
    ' medie sull'intero periodo, quelle mostrate nella finestra di gestione del periodo
    TwaDetails.Is360 = False
    TwaDetails.AggiornaStats(chTwa, i, l)
    If Not chBs Is Nothing Then BsDetails.AggiornaStats(chBs, i, l)
    If Not chTwd Is Nothing Then
      TwdDetails.AggiornaStats(chTwd, i, l)
    End If
    If Not chSeaState Is Nothing Then
      SeaStateDetails.AggiornaStats(chSeaState, i, l)
    End If

    If Not TgtManager Is Nothing Then
      If Not TgtManager.Tgt Is Nothing Then
        PeriodTgt = TgtManager.Tgt.ValoreTgt(Math.Abs(AvgTwa) <= 90, TwsDetails.AvgVal, "bs")
      End If
    End If

    StraightLineVmgDetails = Nothing
    StraightLineReachingDetails = Nothing
    PavarotDetails = Nothing
    BearAwayDetails = Nothing
    RoundUpDetails = Nothing
    AccelerationDetails = Nothing
    Select Case PeriodType
      Case ePeriodType.eAcceleration
        AccelerationDetails = New clsAccelerationDetails2021
      Case ePeriodType.eBearAway
        BearAwayDetails = New clsBearAwayDetails2021
      Case ePeriodType.eGybe
        If PavSettings Is Nothing Then PavSettings = New clsPavarotSettings()
        PavarotDetails = New clsPavarotDetails2021(PavSettings.GybeSecAnte, PavSettings.GybeSecPost, KeyMoment, False)
        'PavarotDetails.UpdatePavarotData(GybeSecAnte, GybeSecPost, KeyMoment, False)
      Case ePeriodType.eRoundUp
        RoundUpDetails = New clsRoundUpDetails2021
      Case ePeriodType.eStraightLineReaching
        StraightLineReachingDetails = New clsStarightLineReachingDetails2021(TR)
        'StraightLineReachingDetails.CaricaValoriBase(TR)
      Case ePeriodType.eStraightLineVmg
        StraightLineVmgDetails = New clsStarightLineVmgDetails2021(TR)
        'StraightLineVmgDetails.CaricaValoriBase(TR)
      Case ePeriodType.eTack
        If PavSettings Is Nothing Then PavSettings = New clsPavarotSettings()
        PavarotDetails = New clsPavarotDetails2021(PavSettings.TackSecAnte, PavSettings.TackSecPost, KeyMoment, True)
        'PavarotDetails.UpdatePavarotData(TackSecAnte, TackSecPost, KeyMoment, True)
      Case ePeriodType.eUndefined
    End Select


  End Sub


  <JsonIgnore>
  Public ReadOnly Property DescrizioneMultiriga As String
    Get
      Dim strTmp As String = ""
      Select Case PeriodType
        Case ePeriodType.eAcceleration
          'If pDettagliAcceleration Is Nothing Then
          '  strTmp = pPeriodType.ToString.TrimStart("e") & " " & StringaDataTime(_TimeRange.Start) & ", Tws: " & pValoriCanaleTWS.AvgString & ", (TkOf: in progress..) " & pShortDescription
          'Else
          '  strTmp = pPeriodType.ToString.TrimStart("e") & " " & StringaDataTime(_TimeRange.Start) & " " & If(IsStbd, "Stbd", "Port") & vbCrLf
          '  strTmp &= "Tws: " & pValoriCanaleTWS.AvgString & vbCrLf
          '  strTmp &= If(pKeys.Trim = "", "", "Keys: " & pKeys & vbCrLf)
          '  strTmp &= pShortDescription & vbCrLf
          '  strTmp &= "Take Off Time: " & Format(pDettagliAcceleration.TimeToTakeOffSpeed.TotalSeconds, "F0") & " ss" & vbCrLf
          '  strTmp &= "Bs Tgt Time: " & Format(pDettagliAcceleration.MomentOfBSatTgt.Momento.Subtract(pDettagliAcceleration.MomentOfInizioAccelerazione.Momento).TotalSeconds, "F0") & " ss" & vbCrLf
          '  strTmp &= "Vmg Tgt Time: " & Format(pDettagliAcceleration.MomentOfVMGatTgt.Momento.Subtract(pDettagliAcceleration.MomentOfInizioAccelerazione.Momento).TotalSeconds, "F0") & " ss" & vbCrLf
          'End If
        Case ePeriodType.eTack
          strTmp = "Tack " & StringaDataTime(KeyMoment) & " " & If(IsStbd, "Stbd Entry", "Port Entry") & vbCrLf
          strTmp &= "Tws: " & TwsDetails.AvgVal.ToString("f1") & vbCrLf
          strTmp &= "Gain/Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & vbCrLf
          strTmp &= If(Keys.Trim = "", "", "Keys: " & Keys & vbCrLf)
          strTmp &= ShortDescription
        Case ePeriodType.eGybe
          strTmp = "Gybe " & StringaDataTime(KeyMoment) & " " & If(IsStbd, "Stbd Entry", "Port Entry") & vbCrLf
          strTmp &= "Tws: " & TwsDetails.AvgVal.ToString("f1") & vbCrLf
          strTmp &= "Gain/Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & vbCrLf
          strTmp &= If(Keys.Trim = "", "", "Keys: " & Keys & vbCrLf)
          strTmp &= ShortDescription
        Case ePeriodType.eStraightLineVmg
          strTmp = "StraightLineVmg " & StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & If(IsStbd, "Stbd", "Port") & vbCrLf
          strTmp &= "Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0") & vbCrLf
          strTmp &= "VmgP: " & StraightLineVmgDetails.VmgPerc.AvgVal.ToString("F0") & "%, Bs: " & StraightLineVmgDetails.Sow.AvgVal.ToString("F1") & ", Twa: " & StraightLineVmgDetails.Twa.AvgVal.ToString("F0") & vbCrLf
          strTmp &= If(Keys.Trim = "", "", "Keys: " & Keys & vbCrLf)
          strTmp &= ShortDescription & vbCrLf
        Case ePeriodType.eStraightLineReaching
          strTmp = "StraightLineReaching " & StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & If(IsStbd, "Stbd", "Port") & vbCrLf
          strTmp &= "Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0") & vbCrLf
          strTmp &= "Pol: " & StraightLineReachingDetails.PolarPerc.AvgVal.ToString("F0") & "%, Bs: " & StraightLineReachingDetails.Sow.AvgVal.ToString("F1") & ", Twa: " & StraightLineReachingDetails.Twa.AvgVal.ToString("F0") & vbCrLf
          strTmp &= If(Keys.Trim = "", "", "Keys: " & Keys & vbCrLf)
          strTmp &= ShortDescription & vbCrLf
        Case Else
          strTmp = PeriodType.ToString.TrimStart("e") & " " & StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & vbCrLf
          strTmp &= ShortDescription & vbCrLf
      End Select
      Return strTmp
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property ColoreTipoPeriodo As Color ' DisplayMemberPath="Descrizione" 
    Get
      Return ColoreDaTipo(PeriodType, 255)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property SfondoTipoPeriodo As Color ' DisplayMemberPath="Descrizione" 
    Get
      Return ColoreDaTipo(PeriodType, 60)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property Descrizione As String
    Get
      'Try
      Select Case PeriodType
          'Case ePeriodType.eAcceleration
          'If pDettagliAcceleration Is Nothing Then
          '  Return StringaDataTime(_TimeRange.Start) & ", (TkOf: in progress..) " & ", Tws: " & pValoriCanaleTWS.AvgString & IIf(pShortDescription.Trim = "", "", ", " & pShortDescription)
          '  Return "Acceleration external to actual loaded files"
          'Else
          '  Return StringaDataTime(_TimeRange.Start) & ", (TkOf: " & Format(pDettagliAcceleration.TimeToTakeOffSpeed.TotalSeconds, "F0") & "ss) " & ", Tws: " & pValoriCanaleTWS.AvgString & IIf(pShortDescription.Trim = "", "", ", " & pShortDescription)
          'End If
        Case ePeriodType.eGybe
          If PavarotDetails Is Nothing Then Return "Gybe external to actual loaded files"
          Return "Gybe " & StringaDataTime(KeyMoment) & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & If(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys)
        Case ePeriodType.eTack
          If PavarotDetails Is Nothing Then Return "Tack external to actual loaded files"
          Return "Tack " & StringaDataTime(KeyMoment) & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & If(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys)
        Case ePeriodType.eStraightLineVmg
          If StraightLineVmgDetails Is Nothing Then Return "Straight Line external to actual loaded files"
          Return "Vmg " & StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys) & ", Twa: " & StraightLineVmgDetails.Twa.AvgVal.ToString("F0") & ", Bs: " & StraightLineVmgDetails.Sow.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0")
        Case ePeriodType.eStraightLineReaching
          If StraightLineReachingDetails Is Nothing Then Return "Straight Line external to actual loaded files"
          Return "Reaching " & StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys) & ", Twa: " & StraightLineReachingDetails.Twa.AvgVal.ToString("F0") & ", Bs: " & StraightLineReachingDetails.Sow.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0")
        Case Else
          Return PeriodType.ToString.TrimStart("e") & " " & TR.StringaPeriodo & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription)
      End Select
      'Catch ex As Exception
      '  Stop
      '  Return PeriodType.ToString.TrimStart("e") & " " & TR.StringaPeriodo & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription)
      'End Try
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property StringaPeriodoXYplot() As String
    Get
      Return TR.Start.ToString("yyyy MMM dd HH:mm:ss") & "-" & TR.Finish.ToString("HH:mm:ss") & " " & Keys
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DescrizioneBreve
    Get
      Select Case PeriodType
        Case ePeriodType.eAcceleration
          'If pDettagliAcceleration Is Nothing Then
          '  Return StringaDataTime(_TimeRange.Start) & ", (TkOf: in progress..) " & ", Tws: " & pValoriCanaleTWS.AvgString & IIf(pShortDescription.Trim = "", "", ", " & pShortDescription)
          '  Return "Acceleration external to actual loaded files"
          'Else
          '  Return StringaDataTime(_TimeRange.Start) & ", (TkOf: " & Format(pDettagliAcceleration.TimeToTakeOffSpeed.TotalSeconds, "F0") & "ss) " & ", Tws: " & pValoriCanaleTWS.AvgString & IIf(pShortDescription.Trim = "", "", ", " & pShortDescription)
          'End If
          Return Nothing
        Case ePeriodType.eGybe
          If PavarotDetails Is Nothing Then Return "Gybe external to actual loaded files"
          Return "G " & StringaDataTime(KeyMoment) & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & If(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys)
        Case ePeriodType.eTack
          If PavarotDetails Is Nothing Then Return "Tack external to actual loaded files"
          Return "T " & StringaDataTime(KeyMoment) & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & ", Loss: " & PavarotDetails.VmgTgtLossMt.ToString("F0") & If(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys)
        Case ePeriodType.eStraightLineVmg
          If StraightLineVmgDetails Is Nothing Then Return "Straight Line external to actual loaded files"
          StraightLineVmgDetails.CaricaValoriBase(TR)
          Return StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys) & ", Twa: " & StraightLineVmgDetails.Twa.AvgVal.ToString("F0") & ", Bs: " & StraightLineVmgDetails.Sow.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0")
        Case ePeriodType.eStraightLineReaching
          If StraightLineReachingDetails Is Nothing Then Return "Straight Line external to actual loaded files"
          StraightLineReachingDetails.CaricaValoriBase(TR)
          Return StringaDataTime(TR.Start) & " (" & TR.DurataInStringaConSeparatore & ") " & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription) & If(Keys.Trim = "", "", ", " & Keys) & ", Twa: " & StraightLineReachingDetails.Twa.AvgVal.ToString("F0") & ", Bs: " & StraightLineReachingDetails.Sow.AvgVal.ToString("F1") & ", Twd: " & TwdDetails.AvgVal.ToString("F0")
        Case Else
          Return PeriodType.ToString.TrimStart("e") & " " & TR.StringaPeriodo & ", Tws: " & TwsDetails.AvgVal.ToString("F1") & IIf(ShortDescription.Trim = "", "", ", " & ShortDescription)
      End Select
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property Mure As Color
    Get
      If IsStbd Then
        Return Colors.SpringGreen
      Else
        Return Colors.OrangeRed
      End If
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property MureStraightLineVmg As Color
    Get
      If StraightLineVmgDetails.Twa.AvgVal >= 0 Then
        Return Colors.SpringGreen
      Else
        Return Colors.OrangeRed
      End If
    End Get
  End Property

  Private Function StringaDataTime(momento As DateTime) As String
    If DataProvider2020.TimeRange.Durata.TotalDays > 1 Then
      Return momento.ToShortDateString & " " & momento.ToLongTimeString
    Else
      Return momento.ToLongTimeString
    End If
  End Function


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsChannelStats2021
  <JsonIgnore>
  Public Channel As clsChannel2020
  <JsonIgnore>
  Public ValoriRaw As Double()
  <JsonIgnore>
  Public ValoriNotNan As Double()
  <JsonIgnore>
  Public ValoriNotNanSin As Double()
  <JsonIgnore>
  Public ValoriNotNanCos As Double()
  <JsonIgnore>
  Public Distribuzione As Distribuzione = Nothing
  <JsonIgnore>
  Public Derivative As clsChannelStats2021

  Public Is360 As Boolean
  Public AvgVal As Double
  Public Percentile5 As Double
  Public Percentile95 As Double
  Public Percentile1 As Double
  Public Percentile99 As Double
  Public MaxVal As Double ' Max Right if Is360
  Public MinVal As Double ' Max Left if Is360
  Public MaxScale As Double
  Public MinScale As Double
  Public StdDev As Double

  Public Sub New(channel As clsChannel2020)
    Me.Channel = channel
    Is360 = channel.DataType = clsChannel2020.eDataType.e360
  End Sub

  Public Sub New()
  End Sub

  'Public Sub SetChannel(channel As clsChannel2020)
  '  Me.Channel = channel
  '  Is360 = channel.DataType = clsChannel2020.eDataType.e360
  'End Sub

  Public Sub AggiornaStats(channel As clsChannel2020, idiniziale As Integer, samples As Integer)
    Dim Valori As Double() = channel.Valori.Skip(idiniziale).Take(samples).ToArray
    AggiornaStats(Valori, 20, -1, -1)
  End Sub

  Public Sub AggiornaStats(Valori As Double())
    AggiornaStats(Valori, 20, -1, -1)
  End Sub

  Public Sub AggiornaStats(Valori As Double(), intervalli As Integer, maxScale As Double, minScale As Double)
    ValoriRaw = Valori
    If (Valori.Count = 0) Then
      AvgVal = 0
      MaxVal = 0
      MinVal = 0
      StdDev = 0
      Distribuzione = Nothing
      Exit Sub
    ElseIf (Valori.Count = 1) Then
      AvgVal = Valori(0)
      MaxVal = Valori(0)
      MinVal = Valori(0)
      StdDev = 0
      Distribuzione = Nothing
      Exit Sub
    End If

    ValoriNotNan = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray


    If ValoriNotNan.Count = 0 Then
      AvgVal = 0
      MaxVal = 0
      MinVal = 0
      StdDev = 0
      Distribuzione = Nothing
      Exit Sub
    End If
    Percentile1 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 1)
    Percentile5 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 5)
    Percentile95 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 95)
    Percentile99 = MathNet.Numerics.Statistics.Statistics.Percentile(ValoriNotNan, 99)

    AvgVal = ValoriNotNan.Average
    MaxVal = ValoriNotNan.Max()
    MinVal = ValoriNotNan.Min()
    Dim BandWidth As Double = 10
    If Is360 Then
      ValoriNotNanSin = ValoriNotNan.Select(Function(x) System.Math.Sin(Radians(x))).ToArray
      ValoriNotNanCos = ValoriNotNan.Select(Function(x) System.Math.Cos(Radians(x))).ToArray
      Dim s As Double = ValoriNotNanSin.Sum
      Dim c As Double = ValoriNotNanCos.Sum
      AvgVal = Degrees(System.Math.Atan2(s, c))
      If AvgVal < 0 Then
        AvgVal += 360
      End If
      If AvgVal > 0 Then
        AvgVal -= 360
      End If
      Dim ValoriNotNanRad = ValoriNotNan.Select(Function(x) Radians(x)).ToArray
      StdDev = Degrees(Accord.Statistics.Circular.StandardDeviation(ValoriNotNanRad))
      Dim sr As New List(Of Double)
      Dim sl As New List(Of Double)
      For Each d In ValoriNotNan
        Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(AvgVal, d)
        If delta > 0 Then
          sr.Add(delta)
        Else
          sl.Add(delta)
        End If
      Next
      MaxVal = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(sr.ToArray(), 95), AvgVal)
      MinVal = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(sl.ToArray(), 95), AvgVal)
      If minScale = -1 Then
        minScale = 0
      End If
      If maxScale = -1 Then
        maxScale = 360
      End If
      BandWidth = 360 / intervalli
    Else
      StdDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(ValoriNotNan)
      BandWidth = Math.Abs(Percentile99 - Percentile1) / intervalli
      If minScale = -1 Then
        minScale = Percentile1
      End If
      If maxScale = -1 Then
        maxScale = Percentile99
      End If
      BandWidth = Math.Abs(maxScale - minScale) / intervalli
    End If

    Me.MaxScale = maxScale
    Me.MinScale = minScale

    Dim xx(intervalli - 1) As Double
    Dim yy(intervalli - 1) As Double
    Dim Totale As Double = 0

    For i As Integer = 0 To intervalli - 1
      Dim x As Double = minScale + (i * BandWidth)
      xx(i) = x
      If BandWidth = 0 Then
        yy(i) = 100 / intervalli
      Else
        Dim kde = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, BandWidth, ValoriNotNan)
        yy(i) = kde
      End If
      Totale += yy(i)
    Next
    For i As Integer = 0 To intervalli - 1
      yy(i) = yy(i) / Totale * 100
    Next
    Distribuzione = New Distribuzione(xx, yy)
  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class Distribuzione
  Public ValoriX As Double()
  Public ValoriY As Double()

  Public Sub New(xx As Double(), yy As Double())
    ValoriX = xx
    ValoriY = yy
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStarightLineVmgDetails2021
  Public Twa As clsChannelStats2021
  Public Sow As clsChannelStats2021
  Public VmgPerc As clsChannelStats2021
  Public ChannelsStats As List(Of clsChannelStats2021)

  Public Sub New()

  End Sub

  Public Sub New(TR As clsTimeRange)
    CaricaValoriBase(TR)
  End Sub

  Public Sub CaricaValoriBase(TR As clsTimeRange)
    Dim i As Integer = TR.IdRigaIniziale
    Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
    If Twa Is Nothing Then
      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Twa = New clsChannelStats2021(chTwa)
      Twa.AggiornaStats(chTwa.Valori.Skip(i).Take(l).ToArray)
    End If
    If Sow Is Nothing Then
      Dim chSow As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
      Sow = New clsChannelStats2021(chSow)
      Sow.AggiornaStats(chSow.Valori.Skip(i).Take(l).ToArray)
    End If
    If VmgPerc Is Nothing Then
      Dim chVmgP As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
      VmgPerc = New clsChannelStats2021(chVmgP)
      VmgPerc.AggiornaStats(chVmgP.Valori.Skip(i).Take(l).ToArray)
    End If
  End Sub


  Public Function ValoriCanale(Canale As clsChannel2020, TR As clsTimeRange) As clsChannelStats2021
    Dim c As clsChannelStats2021 = ChannelsStats.Where(Function(x) x.Channel Is Canale)
    If c Is Nothing Then
      c = New clsChannelStats2021(Canale)
      Dim i As Integer = TR.IdRigaIniziale
      Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
      c.AggiornaStats(Canale.Valori.Skip(i).Take(l).ToArray)
      ChannelsStats.Add(c)
    End If
    Return c
  End Function

  <JsonIgnore>
  Public ReadOnly Property IsTWAinRange(Range As clsDoubleRange) As Boolean
    Get
      If Range Is Nothing Then
        Return True
      Else
        Return (System.Math.Abs(Twa.AvgVal) >= Range.Min And System.Math.Abs(Twa.AvgVal) <= Range.Max)
      End If
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsTWAinRange(Range As clsSailingState.eAndatura) As Boolean
    Get
      Select Case Range
        Case clsSailingState.eAndatura.eUpwind
          Return (System.Math.Abs(Twa.AvgVal) >= 10 And System.Math.Abs(Twa.AvgVal) <= 70)
        Case clsSailingState.eAndatura.eDownWind
          Return (System.Math.Abs(Twa.AvgVal) >= 120 And System.Math.Abs(Twa.AvgVal) <= 170)
        Case clsSailingState.eAndatura.eReaching
          Return (System.Math.Abs(Twa.AvgVal) >= 70 And System.Math.Abs(Twa.AvgVal) <= 120)
        Case Else
          Return True
      End Select
    End Get
  End Property


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStarightLineReachingDetails2021
  Public Twa As clsChannelStats2021
  Public Sow As clsChannelStats2021
  Public PolarPerc As clsChannelStats2021
  Public VmcTable As New Dictionary(Of Integer, Double)
  Public ChannelsStats As List(Of clsChannelStats2021)


  Public Sub New()

  End Sub

  Public Sub New(TR As clsTimeRange)
    CaricaValoriBase(TR)
  End Sub

  Public Sub CaricaValoriBase(TR As clsTimeRange)
    Dim i As Integer = TR.IdRigaIniziale
    Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
    If Twa Is Nothing Then
      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Twa = New clsChannelStats2021(chTwa)
      Twa.AggiornaStats(chTwa.Valori.Skip(i).Take(l).ToArray)
    End If
    If Sow Is Nothing Then
      Dim chSow As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
      Sow = New clsChannelStats2021(chSow)
      Sow.AggiornaStats(chSow.Valori.Skip(i).Take(l).ToArray)
    End If
    If PolarPerc Is Nothing Then
      Dim chPol As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
      PolarPerc = New clsChannelStats2021(chPol)
      PolarPerc.AggiornaStats(chPol.Valori.Skip(i).Take(l).ToArray)
    End If
    If VmcTable.Count = 0 Then
      FillVmcTable()
    End If
  End Sub

  Private Sub FillVmcTable()
    For i As Integer = 10 To 180 Step 5
      Dim Delta As Double = Math.Abs(Twa.AvgVal) - i
      VmcTable.Add(i, Sow.AvgVal * Math.Cos(Radians(Delta)))
    Next
  End Sub

  Public Function ValoriCanale(Canale As clsChannel2020, TR As clsTimeRange) As clsChannelStats2021
    Dim c As clsChannelStats2021 = ChannelsStats.Where(Function(x) x.Channel Is Canale)
    If c Is Nothing Then
      c = New clsChannelStats2021(Canale)
      Dim i As Integer = TR.IdRigaIniziale
      Dim l As Integer = TR.IdRigaFinale - TR.IdRigaIniziale
      c.AggiornaStats(Canale.Valori.Skip(i).Take(l))
      ChannelsStats.Add(c)
    End If
    Return c
  End Function


End Class

'Public Class clsLatLonXY
'  '<JsonIgnore>
'  'Public LatLongArray(40, 1) As Double ' id 0 = 10 secondi prima del keymoment, keymoment ha id 10, id 40 = 30 secondi dopo il keymoment
'  Public XY_OverGround(40, 1) As Double
'  Public XY_OverWater(40, 1) As Double
'  Public Property HeadToWindMoment As DateTime
'  Public Property XY_TimeRange As clsTimeRange


'  'Public Sub CalcolaXY(KeyMoment As DateTime, CurrRate As Double, CurrDir As Double, AsseFromNorth As Double, SecAnte As Integer, SecPost As Integer, SecStep As Integer)
'  '  ReDim XY_OverGround((SecAnte + SecPost) / SecStep, 1)
'  '  ReDim XY_OverWater((SecAnte + SecPost) / SecStep, 1)
'  '  Dim MomentoCorrente As DateTime = KeyMoment.AddSeconds(-SecAnte)
'  '  Dim MomentoFinale As DateTime = KeyMoment.AddSeconds(SecPost)
'  '  Dim IdKeyMoment As Integer = DataProvider2020.TrovaIndice(KeyMoment)
'  '  Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'  '  Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'  '  Dim PuntoRef As New clsGeograficPosition(chLat.PrimoValoreNotNan(IdKeyMoment), chLon.PrimoValoreNotNan(IdKeyMoment))
'  '  Dim i As Integer = 0
'  '  Do While MomentoCorrente <= MomentoFinale
'  '    Dim IdRiga As Integer = DataProvider2020.TrovaIndice(MomentoCorrente)
'  '    Dim Lat As Double = chLat.Valori(IdRiga)
'  '    Dim Lon As Double = chLon.Valori(IdRiga)
'  '    If Not (Double.IsNaN(Lat) OrElse Double.IsNaN(Lon)) Then
'  '      Dim puntoCorrente As New clsGeograficPosition(Lat, Lon)
'  '      Dim RangeMt As Double = clsGeoCalculations.DistanceMeters(PuntoRef, puntoCorrente)
'  '      Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(PuntoRef, puntoCorrente)

'  '      'calcoli per determinare X ed Y over ground
'  '      'Dim RelativeBearing As Double = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'  '      Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(AsseFromNorth, BrgDeg)
'  '      Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'  '      Dim Xoverground As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'  '      Dim Yoverground As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'  '      Dim SecFromKM As Double = MomentoCorrente.Subtract(KeyMoment).TotalSeconds
'  '      Dim PuntoOverWater As clsGeograficPosition = clsGeoCalculations.PuntoDestinazione(puntoCorrente, CurrRate * SecFromKM, CurrDir)
'  '      RangeMt = clsGeoCalculations.DistanceMeters(PuntoRef, PuntoOverWater)
'  '      BrgDeg = clsGeoCalculations.BearingDegrees(PuntoRef, PuntoOverWater)
'  '      RelativeBearing = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'  '      CartBearing = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'  '      Dim Xoverwater As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'  '      Dim Yoverwater As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'  '      XY_OverGround(i, 0) = Xoverground
'  '      XY_OverGround(i, 1) = Yoverground
'  '      XY_OverWater(i, 0) = Xoverwater
'  '      XY_OverWater(i, 1) = Yoverwater
'  '    Else
'  '      XY_OverGround(i, 0) = Double.NaN
'  '      XY_OverGround(i, 1) = Double.NaN
'  '      XY_OverWater(i, 0) = Double.NaN
'  '      XY_OverWater(i, 1) = Double.NaN
'  '    End If

'  '    MomentoCorrente = MomentoCorrente.AddSeconds(SecStep)
'  '    i += 1
'  '  Loop



'  'End Sub

'  'Public Sub CalcolaXYzeroFromHdg(KeyMoment As DateTime, CurrRate As Double, CurrDir As Double, AsseFromNorth As Double, SecAnte As Integer, SecPost As Integer, SecStep As Integer)
'  '  ReDim XY_OverGround((SecAnte + SecPost) / SecStep, 1)
'  '  ReDim XY_OverWater((SecAnte + SecPost) / SecStep, 1)
'  '  Dim IdKeyMoment As Integer = DataProvider2020.TrovaIndice(KeyMoment)
'  '  Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'  '  Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'  '  Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)

'  '  Dim MinDelta As Double = DifferenzaAssolutaTraAngoli360(chHdg.PrimoValoreNotNan(IdKeyMoment), AsseFromNorth)
'  '  For ii As Integer = -10 To 10
'  '    Dim DeltaTmp As Double = DifferenzaAssolutaTraAngoli360(chHdg.PrimoValoreNotNan(IdKeyMoment + ii), AsseFromNorth)
'  '    If DeltaTmp < MinDelta Then
'  '      MinDelta = DeltaTmp
'  '      IdKeyMoment = IdKeyMoment + ii
'  '    End If
'  '  Next

'  '  HeadToWindMoment = DataProvider2020.Momento(IdKeyMoment)
'  '  Dim MomentoCorrente As DateTime = HeadToWindMoment.AddSeconds(-SecAnte)
'  '  Dim MomentoFinale As DateTime = HeadToWindMoment.AddSeconds(SecPost)
'  '  XY_TimeRange = New clsTimeRange(MomentoCorrente, MomentoFinale)
'  '  Dim PuntoRef As New clsGeograficPosition(chLat.PrimoValoreNotNan(IdKeyMoment), chLon.PrimoValoreNotNan(IdKeyMoment))
'  '  Dim i As Integer = 0
'  '  Do While MomentoCorrente <= MomentoFinale
'  '    Dim IdRiga As Integer = DataProvider2020.TrovaIndice(MomentoCorrente)
'  '    Dim Lat As Double = chLat.Valori(IdRiga)
'  '    Dim Lon As Double = chLon.Valori(IdRiga)
'  '    If Not (Double.IsNaN(Lat) OrElse Double.IsNaN(Lon)) Then
'  '      Dim puntoCorrente As New clsGeograficPosition(Lat, Lon)
'  '      Dim RangeMt As Double = clsGeoCalculations.DistanceMeters(PuntoRef, puntoCorrente)
'  '      Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(PuntoRef, puntoCorrente)

'  '      'calcoli per determinare X ed Y over ground
'  '      'Dim RelativeBearing As Double = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'  '      Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(AsseFromNorth, BrgDeg)
'  '      Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'  '      Dim Xoverground As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'  '      Dim Yoverground As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'  '      Dim SecFromKM As Double = MomentoCorrente.Subtract(KeyMoment).TotalSeconds
'  '      Dim PuntoOverWater As clsGeograficPosition = clsGeoCalculations.PuntoDestinazione(puntoCorrente, CurrRate * SecFromKM, CurrDir)
'  '      RangeMt = clsGeoCalculations.DistanceMeters(PuntoRef, PuntoOverWater)
'  '      BrgDeg = clsGeoCalculations.BearingDegrees(PuntoRef, PuntoOverWater)
'  '      RelativeBearing = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'  '      CartBearing = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'  '      Dim Xoverwater As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'  '      Dim Yoverwater As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'  '      XY_OverGround(i, 0) = Xoverground
'  '      XY_OverGround(i, 1) = Yoverground
'  '      XY_OverWater(i, 0) = Xoverwater
'  '      XY_OverWater(i, 1) = Yoverwater
'  '    Else
'  '      XY_OverGround(i, 0) = Double.NaN
'  '      XY_OverGround(i, 1) = Double.NaN
'  '      XY_OverWater(i, 0) = Double.NaN
'  '      XY_OverWater(i, 1) = Double.NaN
'  '    End If

'  '    MomentoCorrente = MomentoCorrente.AddSeconds(SecStep)
'  '    i += 1
'  '  Loop



'  'End Sub


'  Public Function MetriGhostVmgDaPosizione(SecondiDaEntry As Double, idattuale As Integer, PosizioneRef As clsGeograficPosition, IsGybe As Boolean, TgtVmg As Double, TwdAxis As Double) As Double
'    Dim MtGhost As Double = KtsToMS(TgtVmg) * SecondiDaEntry
'    Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'    Dim PuntoAttuale As New clsGeograficPosition(chLat.PrimoValoreNotNan(idattuale), chLng.PrimoValoreNotNan(idattuale))
'    Dim brg As Integer = clsGeoCalculations.BearingDegrees(PosizioneRef, PuntoAttuale)
'    Dim rng As Integer = clsGeoCalculations.DistanceMeters(PosizioneRef, PuntoAttuale)
'    Dim a As Double = DifferenzaAssolutaTraAngoli360(TwdAxis, brg)
'    Dim MtVmg As Double = rng * Math.Cos(Radians(a))
'    If IsGybe Then
'      MtVmg *= -1
'    End If
'    Return MtVmg - MtGhost





'  End Function



'  Public Sub CalcolaXYzeroFromPosition(KeyMoment As DateTime, CurrRate As Double, CurrDir As Double, AsseFromNorth As Double, SecAnte As Integer, SecPost As Integer, SecStep As Integer, IsGybe As Boolean)
'    ReDim XY_OverGround((SecAnte + SecPost) / SecStep, 1)
'    ReDim XY_OverWater((SecAnte + SecPost) / SecStep, 1)
'    Dim IdKeyMoment As Integer = DataProvider2020.TrovaIndice(KeyMoment)
'    Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)

'    Dim Brg As Double = chHdg.PrimoValoreNotNan(IdKeyMoment)
'    If IsGybe Then Brg = SommaAngolo180adAngolo360(180, Brg)
'    Dim MinDelta As Double = DifferenzaAssolutaTraAngoli360(Brg, AsseFromNorth)
'    For ii As Integer = -10 To 10
'      Dim p1 As New clsGeograficPosition(chLat.PrimoValoreNotNan(IdKeyMoment + ii - 1), chLon.PrimoValoreNotNan(IdKeyMoment + ii - 1))
'      Dim p2 As New clsGeograficPosition(chLat.PrimoValoreNotNan(IdKeyMoment + ii), chLon.PrimoValoreNotNan(IdKeyMoment + ii))
'      Brg = clsGeoCalculations.BearingDegrees(p1, p2)
'      If IsGybe Then Brg = SommaAngolo180adAngolo360(180, Brg)
'      Dim DeltaTmp As Double = DifferenzaAssolutaTraAngoli360(Brg, AsseFromNorth)
'      If DeltaTmp < MinDelta Then
'        MinDelta = DeltaTmp
'        IdKeyMoment = IdKeyMoment + ii
'      End If
'    Next

'    HeadToWindMoment = DataProvider2020.Momento(IdKeyMoment)
'    Dim MomentoCorrente As DateTime = HeadToWindMoment.AddSeconds(-SecAnte)
'    Dim MomentoFinale As DateTime = HeadToWindMoment.AddSeconds(SecPost)
'    XY_TimeRange = New clsTimeRange(MomentoCorrente, MomentoFinale)

'    Dim PuntoRef As New clsGeograficPosition(chLat.PrimoValoreNotNan(IdKeyMoment), chLon.PrimoValoreNotNan(IdKeyMoment))
'    Dim i As Integer = 0
'    Do While MomentoCorrente <= MomentoFinale
'      Dim IdRiga As Integer = DataProvider2020.TrovaIndice(MomentoCorrente)
'      Dim Lat As Double = chLat.Valori(IdRiga)
'      Dim Lon As Double = chLon.Valori(IdRiga)
'      If Not (Double.IsNaN(Lat) OrElse Double.IsNaN(Lon)) Then
'        Dim puntoCorrente As New clsGeograficPosition(Lat, Lon)
'        Dim RangeMt As Double = clsGeoCalculations.DistanceMeters(PuntoRef, puntoCorrente)
'        Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(PuntoRef, puntoCorrente)

'        'calcoli per determinare X ed Y over ground
'        'Dim RelativeBearing As Double = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'        Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(AsseFromNorth, BrgDeg)
'        Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'        Dim Xoverground As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'        Dim Yoverground As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'        Dim SecFromKM As Double = MomentoCorrente.Subtract(KeyMoment).TotalSeconds
'        Dim PuntoOverWater As clsGeograficPosition = clsGeoCalculations.PuntoDestinazione(puntoCorrente, CurrRate * SecFromKM, CurrDir)
'        RangeMt = clsGeoCalculations.DistanceMeters(PuntoRef, PuntoOverWater)
'        BrgDeg = clsGeoCalculations.BearingDegrees(PuntoRef, PuntoOverWater)
'        RelativeBearing = SommaAngolo180adAngolo360(AsseFromNorth, BrgDeg)
'        CartBearing = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
'        Dim Xoverwater As Double = RangeMt * System.Math.Cos(Radians(CartBearing))
'        Dim Yoverwater As Double = RangeMt * System.Math.Sin(Radians(CartBearing))

'        XY_OverGround(i, 0) = Xoverground
'        XY_OverGround(i, 1) = Yoverground
'        XY_OverWater(i, 0) = Xoverwater
'        XY_OverWater(i, 1) = Yoverwater
'      Else
'        XY_OverGround(i, 0) = Double.NaN
'        XY_OverGround(i, 1) = Double.NaN
'        XY_OverWater(i, 0) = Double.NaN
'        XY_OverWater(i, 1) = Double.NaN
'      End If

'      MomentoCorrente = MomentoCorrente.AddSeconds(SecStep)
'      i += 1
'    Loop



'  End Sub


'End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPavarotDetails2021
  Public Property TwdAxis As Double
  Public Property IsValid As Boolean = False
  Public Property EntryTwa As Double
  Public Property ExitTwa As Double
  Public Property BottomSpeed As Double
  Public Property RotPerc95 As Double
  Public Property SecAnteCalc As Integer ' finestra (secondi prima/dopo il key moment) usata nell ultimo calcolo dei dettagli; 0/0 nei periodi salvati prima di questo campo
  Public Property SecPostCalc As Integer
  ' loss sulla traccia nell'acqua (vedi CalcolaLossAcqua)
  Public Const VersioneDettagliCorrente As Integer = 4 ' da incrementare quando cambia il calcolo dei dettagli: le manovre piu' vecchie vanno ricalcolate
  Public Property VersioneDettagli As Integer           ' 0 nei periodi salvati prima di questo campo
  Public Property IsTackCalc As Boolean
  Public Property CseEntrata As Double = Double.NaN      ' rotta sull'acqua media dei primi secondi della finestra
  Public Property BsEntrata As Double = Double.NaN       ' velocita' media dei primi secondi della finestra (kts)
  Public Property CseUscita As Double = Double.NaN       ' rotta sull'acqua media dopo la stabilizzazione
  Public Property BsUscita As Double = Double.NaN        ' velocita' media degli ultimi secondi della finestra (kts): velocita' di regime d'uscita del ghost
  Public Property CseUscitaFine As Double = Double.NaN   ' rotta sull'acqua media degli ultimi secondi della finestra: rotta di regime d'uscita del ghost
  Public Property KeyMomentLoss As DateTime              ' key moment usato nel calcolo del loss (istante in cui il ghost vira)
  Public Property AsseBisettrice As Double = Double.NaN  ' bisettrice tra rotta d'entrata e d'uscita: asse su cui si misura il progresso
  Public Property WaterLossMt As Double = Double.NaN     ' metri persi sull'asse rispetto al ghost alla velocita' d'entrata (positivo = perdita)
  Public Property StableTimeSec As Double = Double.NaN   ' secondi dal key moment alla stabilizzazione di velocita' e rotta
  Public Property RecoveryTimeSec As Double = Double.NaN ' secondi dal key moment al recupero del vmg della mura d'uscita
  Public Property ExitRefVmg As Double = Double.NaN      ' vmg di riferimento della mura d'uscita (kts)
  Public Property ExitRefFromTarget As Boolean           ' True se il riferimento viene dal target perche' manca un tratto dritto utile
  Public Property ExitVmgRatio As Double = Double.NaN    ' vmg d'uscita stabilizzato / vmg di riferimento della mura d'uscita (%)
  Public Property EntryBs As Double
  Public Property ExitBs As Double
  Public Property EntryExitDeltaHdg As Double
  Public Property EntryExitDeltaCse As Double
  Public Property EntryExitDeltaCog As Double
  Public Property EntryExitDeltaTwd As Double ' valori positivi significa ha dato buono
  Public Property EntryExitDeltaTws As Double

  Public Property EntryExitDeltaTwaCog As Double
  Public Property EntryExitDeltaTwaCse As Double
  Public Property EntryExitDeltaTwaHdg As Double
  Public Property EntryExitDeltaTwaTgt As Double

  Public Property VmgPercAvg As Double ' Perdita in vmg rispetto al target
  Public Property VmgAvg As Double ' Perdita in vmg rispetto al target
  Public Property VmgTgtLossMt As Double ' Perdita in vmg rispetto al target
  Public Property InlineTgtLossMt As Double ' Perdita inline rispetto al target
  Public Property VmgEntryLossMt As Double ' Perdita in vmg rispetto al vmg medio da inizio - seconds ad inizio
  Public Property VmgExitLossMt As Double ' Perdita in vmg rispetto al vmg medio da fine a fine + seconds

  Public Property cRate As Double
  Public Property cDir As Double

  Public ReadOnly Property InlineTgtLossBL As Double
    Get
      Return InlineTgtLossMt / AppConfig.ActiveProfile.BoatLenghtInMeters
    End Get
  End Property

  'Public Property LatLonXY As New clsLatLonXY

  Public ReadOnly Property EntryExitDeltaTwa As Double
    Get
      If Math.Abs(EntryTwa) < 90 Then
        Return Math.Abs(EntryTwa) + Math.Abs(ExitTwa)
      Else
        Return (360 - (Math.Abs(EntryTwa) + Math.Abs(ExitTwa)))
      End If
    End Get
  End Property

  Private Sub CalcolaVmgLoss(idiniziale As Integer, idfinale As Integer, seconds As Double, EntrySecs As Integer)
    Dim chVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
    VmgAvg = chVmg.Valori.Skip(idiniziale).Take(idfinale - idiniziale).Where(Function(x) Not Double.IsNaN(x)).Average ' Perdita in vmg rispetto al target

    If TgtManager.Tgt Is Nothing Then
      Dim CanaleTgt As clsChannel2020 = DataProvider2020.VerificaCanaleTarget(chVmg)
      If CanaleTgt Is Nothing Then
        VmgTgtLossMt = 0 ' Perdita in vmg rispetto al target
      Else
        Dim avgVmgTgt = CanaleTgt.Valori.Where(Function(x) Not Double.IsNaN(x)).Skip(idiniziale).Take(idfinale - idiniziale).Average ' Perdita in vmg rispetto al target
        VmgPercAvg = VmgAvg / avgVmgTgt * 100
        VmgTgtLossMt = (avgVmgTgt - VmgAvg) * seconds / 3600 * 1852
        InlineTgtLossMt = VmgTgtLossMt / Math.Abs(Math.Cos(Radians(EntryTwa)))
      End If
    Else
      Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
      Dim tws = chTws.Valori.Skip(idiniziale).Take(idfinale - idiniziale).Where(Function(x) Not Double.IsNaN(x)).Average
      Dim Tgt = TgtManager.Tgt.ValoreTgt(Math.Abs(EntryTwa) <= 90, tws, "bs")
      VmgPercAvg = VmgAvg / Tgt.Vmg * 100
      VmgTgtLossMt = (Tgt.Vmg - VmgAvg) * seconds / 3600 * 1852
      InlineTgtLossMt = VmgTgtLossMt / Math.Abs(Math.Cos(Radians(Tgt.Twa)))
    End If
    Dim idante As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(idiniziale).AddSeconds(-EntrySecs))
    Dim entryVmg = chVmg.Valori.Where(Function(x) Not Double.IsNaN(x)).Skip(idante).Take(idiniziale - idante).Average
    VmgEntryLossMt = (entryVmg - VmgAvg) * seconds / 3600 * 1852

    Dim idpost As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(idfinale).AddSeconds(EntrySecs))
    Dim exitVmg = chVmg.Valori.Where(Function(x) Not Double.IsNaN(x)).Skip(idfinale).Take(idante - idfinale).Average ' Perdita in vmg rispetto al target
    VmgExitLossMt = (exitVmg - VmgAvg) * seconds / 3600 * 1852

  End Sub

  Public Function MetriGhostVmgDaEntry(SecondiDaEntry As Double, TgtVmg As Double, identry As Integer, idattuale As Integer) As Double
    Dim MtGhost As Double = KtsToMS(TgtVmg) * SecondiDaEntry
    Dim chVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
    Dim MtVmg As Double = -KtsToMS(chVmg.PrimoValoreNotNan(identry))
    For i As Integer = identry To idattuale
      If Not Double.IsNaN(chVmg.Valori(i)) Then
        MtVmg += KtsToMS(chVmg.Valori(i))
      End If
    Next
    'Dim VmgFromEntry = chVmg.Valori.Where(Function(x) Not Double.IsNaN(x)).Skip(identry).Take(idattuale - identry).Average
    'Dim MtVmg As Double = KtsToMS(VmgFromEntry) * SecondiDaEntry
    Return MtVmg - MtGhost
  End Function

  'Public Function PosizioneHeadToWind() As clsGeograficPosition
  '  Dim Id As Integer = DataProvider2020.TrovaIndice(LatLonXY.HeadToWindMoment)
  '  Return New clsGeograficPosition(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).PrimoValoreNotNan(Id), DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).PrimoValoreNotNan(Id))
  'End Function

  Public Function PosizioneAtTime(Momento As DateTime) As clsGeographicPosition
    Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
    Return New clsGeographicPosition(DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat).PrimoValoreNotNan(Id), DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng).PrimoValoreNotNan(Id))
  End Function

  Public Function PosizioneRelativa_XY(IdAttuale As Integer, PosizioneRef As clsGeographicPosition, IsGybe As Boolean, IsStbdEntry As Boolean) As clsDoubleXY
    Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    Dim PuntoAttuale As New clsGeographicPosition(chLat.PrimoValoreNotNan(IdAttuale), chLng.PrimoValoreNotNan(IdAttuale))
    Dim brg As Integer = clsGeoCalculations.BearingDegrees(PosizioneRef, PuntoAttuale)
    Dim rng As Integer = clsGeoCalculations.DistanceMeters(PosizioneRef, PuntoAttuale)
    Dim a As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TwdAxis, brg)
    Dim Y As Double = rng * Math.Cos(Radians(a))
    Dim X As Double = -rng * Math.Sin(Radians(a))
    If IsGybe Then
      Y *= -1
    End If
    If IsStbdEntry Then
      X *= -1
    End If
    Return New clsDoubleXY(X, Y)
  End Function

  Public Function MetriGhostVmgDaPosizione(SecondiDaEntry As Double, idattuale As Integer, PosizioneRef As clsGeographicPosition, IsGybe As Boolean, TgtVmg As Double) As Double
    Dim MtGhost As Double = KtsToMS(TgtVmg) * SecondiDaEntry
    Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    Dim PuntoAttuale As New clsGeographicPosition(chLat.PrimoValoreNotNan(idattuale), chLng.PrimoValoreNotNan(idattuale))
    Dim brg As Integer = clsGeoCalculations.BearingDegrees(PosizioneRef, PuntoAttuale)
    Dim rng As Integer = clsGeoCalculations.DistanceMeters(PosizioneRef, PuntoAttuale)
    Dim a As Double = DifferenzaAssolutaTraAngoli360(TwdAxis, brg)
    Dim MtVmg As Double = rng * Math.Cos(Radians(a))
    If IsGybe Then
      MtVmg *= -1
    End If
    Return MtVmg - MtGhost

  End Function

  Public Sub New()

  End Sub

  ' ================= loss sulla traccia nell'acqua =================
  ' Il ghost continua alla velocita' d'entrata; il progresso reale e' la traccia nell'acqua (velocita' x rotta sull'acqua)
  ' proiettata su un asse. Nell'acqua una corrente uniforme sposta allo stesso modo barca e ghost e quindi si annulla.

  Public Enum eVarianteLoss
    eAcquaBisettrice = 0 ' traccia nell'acqua, asse = bisettrice tra rotta d'entrata e d'uscita
    eAcquaTwd = 1        ' traccia nell'acqua, asse = twd media della finestra
    eGpsTwd = 2          ' traccia sul fondo (lat/lon), asse = twd media della finestra
  End Enum

  Private Const SecRiferimento As Double = 120    ' secondi massimi di tratto dritto usati come riferimento della mura d'uscita
  Private Const SecRicerca As Double = 600        ' distanza massima a cui cercare il tratto dritto sulla mura d'uscita
  Private Const SecMinRiferimento As Double = 20  ' secondi minimi validi, altrimenti si usa il target
  Private Const SecVicinoManovra As Double = 20   ' secondi scartati vicino alle manovre adiacenti
  Private Const YrtDritto As Double = 3           ' yaw rate massimo (gradi/s) di un campione considerato dritto
  Private Const SecTenutaRecupero As Double = 3   ' il vmg deve restare sopra soglia per almeno questi secondi
  Private Const DtMassimo As Double = 5           ' passi di tempo piu' lunghi sono buchi nei dati e non si integrano

  Private Shared Function ImpostazioniLoss() As clsPavarotSettings
    Dim S As clsPavarotSettings = AppConfig.ActiveProfile.PavarotSettings
    If S Is Nothing Then
      S = New clsPavarotSettings
      AppConfig.ActiveProfile.PavarotSettings = S
    End If
    S.NormalizzaParametriLoss()
    Return S
  End Function

  Private Shared Function MediaNaN(Canale As clsChannel2020, Da As Integer, A As Integer) As Double
    If Canale Is Nothing Then Return Double.NaN
    Dim Somma As Double = 0
    Dim N As Integer = 0
    For i As Integer = Math.Max(0, Da) To Math.Min(A, Canale.Valori.Count - 1)
      Dim v As Double = Canale.Valori(i)
      If Not Double.IsNaN(v) Then
        Somma += v
        N += 1
      End If
    Next
    If N = 0 Then Return Double.NaN
    Return Somma / N
  End Function

  Private Shared Function MediaCircolareNaN(Canale As clsChannel2020, Da As Integer, A As Integer) As Double
    If Canale Is Nothing Then Return Double.NaN
    Dim Seni As Double = 0
    Dim Coseni As Double = 0
    Dim N As Integer = 0
    For i As Integer = Math.Max(0, Da) To Math.Min(A, Canale.Valori.Count - 1)
      Dim v As Double = Canale.Valori(i)
      If Not Double.IsNaN(v) Then
        Seni += Math.Sin(Radians(v))
        Coseni += Math.Cos(Radians(v))
        N += 1
      End If
    Next
    If N = 0 Then Return Double.NaN
    Dim m As Double = Degrees(Math.Atan2(Seni, Coseni))
    If m < 0 Then m += 360
    Return m
  End Function

  ' escursione (max - min) dei valori; per gli angoli rispetto al primo valore valido
  Private Shared Function EscursioneNaN(Canale As clsChannel2020, Da As Integer, A As Integer, Circolare As Boolean) As Double
    If Canale Is Nothing Then Return Double.NaN
    Dim Rif As Double = Double.NaN
    Dim MinV As Double = Double.MaxValue
    Dim MaxV As Double = Double.MinValue
    For i As Integer = Math.Max(0, Da) To Math.Min(A, Canale.Valori.Count - 1)
      Dim v As Double = Canale.Valori(i)
      If Double.IsNaN(v) Then Continue For
      If Circolare Then
        If Double.IsNaN(Rif) Then Rif = v
        v = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Rif, v)
      End If
      MinV = Math.Min(MinV, v)
      MaxV = Math.Max(MaxV, v)
    Next
    If MaxV < MinV Then Return Double.NaN
    Return MaxV - MinV
  End Function

  Private Sub CalcolaLossAcqua(IdAnte As Integer, IdKm As Integer, IdPost As Integer, IsTack As Boolean, chBs As clsChannel2020, chCse As clsChannel2020, chTwa As clsChannel2020, chTws As clsChannel2020)
    Dim S As clsPavarotSettings = ImpostazioniLoss()
    IsTackCalc = IsTack
    Dim Finestra As Double = S.StableWindowSec

    ' entrata: medie dei primi secondi della finestra
    Dim t0 As DateTime = DataProvider2020.Momento(IdAnte)
    Dim IdEntrataFine As Integer = DataProvider2020.TrovaIndice(t0.AddSeconds(Finestra))
    CseEntrata = MediaCircolareNaN(chCse, IdAnte, IdEntrataFine)
    BsEntrata = MediaNaN(chBs, IdAnte, IdEntrataFine)

    ' stabilizzazione: primo istante dopo il key moment in cui velocita' e rotta restano entro le tolleranze per tutta la finestra
    Dim tKm As DateTime = DataProvider2020.Momento(IdKm)
    Dim IdStabile As Integer = -1
    For i As Integer = IdKm To IdPost
      Dim iFine As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(i).AddSeconds(Finestra))
      If iFine > IdPost OrElse iFine <= i Then Exit For
      If EscursioneNaN(chBs, i, iFine, False) <= S.StableBsTolKts AndAlso EscursioneNaN(chCse, i, iFine, True) <= S.StableCseTolDeg Then
        IdStabile = i
        Exit For
      End If
    Next
    Dim IdUscitaIni As Integer
    Dim IdUscitaFine As Integer
    If IdStabile >= 0 Then
      StableTimeSec = DataProvider2020.Momento(IdStabile).Subtract(tKm).TotalSeconds
      IdUscitaIni = IdStabile
      IdUscitaFine = DataProvider2020.TrovaIndice(DataProvider2020.Momento(IdStabile).AddSeconds(Finestra))
    Else
      ' mai stabile nella finestra: si usa la fine della finestra, il valore resta segnalato da StableTimeSec = NaN
      StableTimeSec = Double.NaN
      IdUscitaIni = DataProvider2020.TrovaIndice(DataProvider2020.Momento(IdPost).AddSeconds(-Finestra))
      IdUscitaFine = IdPost
    End If
    CseUscita = MediaCircolareNaN(chCse, IdUscitaIni, IdUscitaFine)
    ' regime d'uscita del ghost: ultimi secondi della finestra, quando la barca ha finito di accelerare (subito dopo la stabilizzazione e' ancora troppo presto)
    Dim IdRegimeIni As Integer = DataProvider2020.TrovaIndice(DataProvider2020.Momento(IdPost).AddSeconds(-Finestra))
    BsUscita = MediaNaN(chBs, IdRegimeIni, IdPost)
    CseUscitaFine = MediaCircolareNaN(chCse, IdRegimeIni, IdPost)
    KeyMomentLoss = tKm
    If Double.IsNaN(CseEntrata) OrElse Double.IsNaN(CseUscita) Then
      AsseBisettrice = Double.NaN
    Else
      AsseBisettrice = Media360(CseEntrata, CseUscita)
    End If

    ' loss finale sulla bisettrice (positivo = perdita)
    Dim Serie As List(Of Double) = SerieLoss(eVarianteLoss.eAcquaBisettrice, IdAnte, IdPost)
    WaterLossMt = Double.NaN
    For i As Integer = Serie.Count - 1 To 0 Step -1
      If Not Double.IsNaN(Serie(i)) Then
        WaterLossMt = -Serie(i)
        Exit For
      End If
    Next

    ' riferimento della mura d'uscita, rapporto d'uscita e tempo di recupero
    Dim chVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
    Dim chYrt As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim UscitaStbd As Boolean = Not (MediaNaN(chTwa, IdAnte, IdEntrataFine) >= 0)
    ExitRefVmg = VmgRiferimentoMuraUscita(IdAnte, IdPost, IsTack, UscitaStbd, chVmg, chTwa, chYrt)
    ExitRefFromTarget = Double.IsNaN(ExitRefVmg)
    If ExitRefFromTarget Then
      Dim Tws As Double = MediaNaN(chTws, IdAnte, IdPost)
      If Not TgtManager Is Nothing AndAlso Not TgtManager.Tgt Is Nothing AndAlso Not Double.IsNaN(Tws) Then
        ExitRefVmg = TgtManager.Tgt.ValoreTgt(IsTack, Tws, "bs").Vmg
      End If
    End If
    Dim VmgUscita As Double = MediaNaN(chVmg, IdUscitaIni, IdUscitaFine)
    ExitVmgRatio = If(ExitRefVmg > 0, VmgUscita / ExitRefVmg * 100, Double.NaN)
    RecoveryTimeSec = TempoRecupero(IdKm, IdPost, UscitaStbd, chVmg, chTwa, chYrt, ExitRefVmg * S.RecoveryVmgPerc / 100)
  End Sub

  ' vmg medio reale sulla mura d'uscita: prima il bordo che segue la manovra, poi l'ultimo bordo su quella mura che la precede
  Private Function VmgRiferimentoMuraUscita(IdAnte As Integer, IdPost As Integer, IsTack As Boolean, UscitaStbd As Boolean, chVmg As clsChannel2020, chTwa As clsChannel2020, chYrt As clsChannel2020) As Double
    If chVmg Is Nothing OrElse chTwa Is Nothing Then Return Double.NaN
    Dim Campioni = CampioniMura(IdPost + 1, 1, IsTack, UscitaStbd, False, chVmg, chTwa, chYrt)
    If Campioni.Count / Math.Max(1, DataProvider2020.Hz) < SecMinRiferimento Then
      Campioni = CampioniMura(IdAnte - 1, -1, IsTack, UscitaStbd, True, chVmg, chTwa, chYrt)
    End If
    If Campioni.Count / Math.Max(1, DataProvider2020.Hz) < SecMinRiferimento Then Return Double.NaN
    Return Campioni.Average(Function(c) c.Value)
  End Function

  ' campioni dritti sulla mura cercata a partire da Da (Passo +1 avanti, -1 indietro), in ordine di distanza dalla manovra.
  ' Si scartano i secondi vicini alle manovre adiacenti e si tengono al massimo SecRiferimento secondi.
  Private Function CampioniMura(Da As Integer, Passo As Integer, IsTack As Boolean, MuraStbd As Boolean, SaltaMuraOpposta As Boolean, chVmg As clsChannel2020, chTwa As clsChannel2020, chYrt As clsChannel2020) As List(Of KeyValuePair(Of DateTime, Double))
    Dim Grezzi As New List(Of KeyValuePair(Of DateTime, Double))
    Dim n As Integer = chTwa.Valori.Count
    If Da < 0 OrElse Da >= n Then Return Grezzi
    Dim tInizio As DateTime = DataProvider2020.Momento(Da)
    Dim InMura As Boolean = False
    Dim i As Integer = Da
    Do While i >= 0 AndAlso i < n
      Dim m As DateTime = DataProvider2020.Momento(i)
      If m.ToOADate > 0 Then
        If Math.Abs(m.Subtract(tInizio).TotalSeconds) > SecRicerca Then Exit Do
        Dim twa As Double = chTwa.Valori(i)
        If Not Double.IsNaN(twa) Then
          If (twa >= 0) = MuraStbd Then
            InMura = True
            Dim vmg As Double = chVmg.Valori(i)
            Dim yrt As Double = If(chYrt Is Nothing, 0, chYrt.Valori(i))
            Dim AndaturaOk As Boolean = (Math.Abs(twa) < 90) = IsTack
            If AndaturaOk AndAlso Not Double.IsNaN(vmg) AndAlso (Double.IsNaN(yrt) OrElse Math.Abs(yrt) < YrtDritto) Then
              Grezzi.Add(New KeyValuePair(Of DateTime, Double)(m, vmg))
            End If
          ElseIf InMura OrElse Not SaltaMuraOpposta Then
            Exit Do ' finito il bordo sulla mura cercata
          End If
        End If
      End If
      i += Passo
    Loop
    If Grezzi.Count = 0 Then Return Grezzi
    ' avanti il bordo finisce con la manovra successiva; indietro e' compreso tra due manovre
    Dim tPrimo As DateTime = Grezzi.First.Key
    Dim tUltimo As DateTime = Grezzi.Last.Key
    Dim Ris As New List(Of KeyValuePair(Of DateTime, Double))
    For Each c In Grezzi
      If Math.Abs(c.Key.Subtract(tUltimo).TotalSeconds) < SecVicinoManovra Then Continue For
      If SaltaMuraOpposta AndAlso Math.Abs(c.Key.Subtract(tPrimo).TotalSeconds) < SecVicinoManovra Then Continue For
      Ris.Add(c)
    Next
    If Ris.Count = 0 Then Return Ris
    Dim tVicino As DateTime = Ris.First.Key
    Return Ris.Where(Function(c) Math.Abs(c.Key.Subtract(tVicino).TotalSeconds) <= SecRiferimento).ToList
  End Function

  ' secondi dal key moment al primo istante in cui, sulla mura d'uscita e senza ruotare, il vmg (media di +-1 s) resta sopra soglia per SecTenutaRecupero
  Private Function TempoRecupero(IdKm As Integer, IdPost As Integer, UscitaStbd As Boolean, chVmg As clsChannel2020, chTwa As clsChannel2020, chYrt As clsChannel2020, Soglia As Double) As Double
    If Double.IsNaN(Soglia) OrElse chVmg Is Nothing OrElse chTwa Is Nothing Then Return Double.NaN
    Dim tKm As DateTime = DataProvider2020.Momento(IdKm)
    Dim Meta As Integer = Math.Max(1, DataProvider2020.Hz)
    Dim tTenuta As DateTime = Nothing
    Dim InTenuta As Boolean = False
    For i As Integer = IdKm To IdPost
      Dim m As DateTime = DataProvider2020.Momento(i)
      If m.ToOADate <= 0 Then Continue For
      Dim twa As Double = chTwa.Valori(i)
      Dim yrt As Double = If(chYrt Is Nothing, 0, chYrt.Valori(i))
      Dim vmg As Double = MediaNaN(chVmg, i - Meta, i + Meta)
      Dim Ok As Boolean = Not Double.IsNaN(twa) AndAlso (twa >= 0) = UscitaStbd AndAlso (Double.IsNaN(yrt) OrElse Math.Abs(yrt) < YrtDritto) AndAlso vmg >= Soglia
      If Ok Then
        If Not InTenuta Then
          InTenuta = True
          tTenuta = m
        End If
        If m.Subtract(tTenuta).TotalSeconds >= SecTenutaRecupero Then Return tTenuta.Subtract(tKm).TotalSeconds
      Else
        InTenuta = False
      End If
    Next
    Return Double.NaN
  End Function

  ' progresso sull'asse meno quello del ghost (positivo = guadagno), una voce per ogni riga da IdIniziale a IdFinale
  Public Function SerieLoss(Variante As eVarianteLoss, IdIniziale As Integer, IdFinale As Integer) As List(Of Double)
    Dim Ris As New List(Of Double)
    For i As Integer = IdIniziale To IdFinale
      Ris.Add(Double.NaN)
    Next
    If IdFinale <= IdIniziale Then Return Ris
    Dim Asse As Double
    If Variante = eVarianteLoss.eAcquaBisettrice Then
      Asse = AsseBisettrice
    ElseIf IsTackCalc Then
      Asse = TwdAxis
    Else
      Asse = SommaAngolo180adAngolo360(180, TwdAxis) ' in poppa si avanza sottovento
    End If
    If Double.IsNaN(Asse) Then Return Ris
    Dim S As clsPavarotSettings = ImpostazioniLoss()
    Dim t0 As DateTime = DataProvider2020.Momento(IdIniziale)

    If Variante = eVarianteLoss.eGpsTwd Then
      Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
      Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
      If chLat Is Nothing OrElse chLng Is Nothing Then Return Ris
      Dim P0 As New clsGeographicPosition(chLat.PrimoValoreNotNan(IdIniziale), chLng.PrimoValoreNotNan(IdIniziale))
      Dim IdE As Integer = DataProvider2020.TrovaIndice(t0.AddSeconds(S.StableWindowSec))
      Dim dtE As Double = DataProvider2020.Momento(IdE).Subtract(t0).TotalSeconds
      If dtE <= 0 Then Return Ris
      Dim PE As New clsGeographicPosition(chLat.PrimoValoreNotNan(IdE), chLng.PrimoValoreNotNan(IdE))
      Dim VGhost As Double = ProiezioneSuAsse(P0, PE, Asse) / dtE
      For i As Integer = IdIniziale To IdFinale
        Dim m As DateTime = DataProvider2020.Momento(i)
        If m.ToOADate <= 0 Then Continue For
        Dim lat As Double = chLat.Valori(i)
        Dim lng As Double = chLng.Valori(i)
        If Double.IsNaN(lat) OrElse Double.IsNaN(lng) Then Continue For
        Ris(i - IdIniziale) = ProiezioneSuAsse(P0, New clsGeographicPosition(lat, lng), Asse) - VGhost * m.Subtract(t0).TotalSeconds
      Next
    Else
      Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
      Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
      If chCse Is Nothing Then chCse = DataProvider2020.CseFromHdgAndLeeway()
      If chBs Is Nothing OrElse chCse Is Nothing Then Return Ris
      ' ghost sulla bisettrice: vira istantaneamente al key moment, prima rotta e velocita' d'entrata, poi quelle d'uscita
      Dim VGhost As Double = KtsToMS(BsEntrata) * Math.Cos(Radians(DifferenzaAssolutaTraAngoli360(CseEntrata, Asse)))
      If Double.IsNaN(VGhost) Then Return Ris
      Dim VGhostUscita As Double = VGhost
      Dim DoppioGhost As Boolean = Variante = eVarianteLoss.eAcquaBisettrice AndAlso Not Double.IsNaN(BsUscita) AndAlso Not Double.IsNaN(CseUscitaFine) AndAlso KeyMomentLoss.ToOADate > 0
      If DoppioGhost Then VGhostUscita = KtsToMS(BsUscita) * Math.Cos(Radians(DifferenzaAssolutaTraAngoli360(CseUscitaFine, Asse)))
      Dim Progresso As Double = 0
      Dim iPrec As Integer = -1
      Dim mPrec As DateTime
      For i As Integer = IdIniziale To IdFinale
        Dim m As DateTime = DataProvider2020.Momento(i)
        If m.ToOADate <= 0 Then Continue For
        If iPrec >= 0 Then
          Dim dt As Double = m.Subtract(mPrec).TotalSeconds
          Dim bs As Double = chBs.Valori(iPrec)
          Dim cse As Double = chCse.Valori(iPrec)
          If dt > 0 AndAlso dt <= DtMassimo AndAlso Not Double.IsNaN(bs) AndAlso Not Double.IsNaN(cse) Then
            Progresso += KtsToMS(bs) * dt * Math.Cos(Radians(DifferenzaAssolutaTraAngoli360(cse, Asse)))
          End If
        End If
        Dim Ghost As Double
        If DoppioGhost AndAlso m > KeyMomentLoss Then
          Ghost = VGhost * KeyMomentLoss.Subtract(t0).TotalSeconds + VGhostUscita * m.Subtract(KeyMomentLoss).TotalSeconds
        Else
          Ghost = VGhost * m.Subtract(t0).TotalSeconds
        End If
        Ris(i - IdIniziale) = Progresso - Ghost
        iPrec = i
        mPrec = m
      Next
    End If
    Return Ris
  End Function

  Private Shared Function ProiezioneSuAsse(Da As clsGeographicPosition, A As clsGeographicPosition, Asse As Double) As Double
    Dim d As Double = clsGeoCalculations.DistanceMeters(Da, A)
    If Double.IsNaN(d) OrElse d = 0 Then Return 0
    Dim brg As Double = clsGeoCalculations.BearingDegrees(Da, A)
    Return d * Math.Cos(Radians(DifferenzaAssolutaTraAngoli360(brg, Asse)))
  End Function

  ' vmg (media di +-1 s) in percentuale del riferimento della mura d'uscita, una voce per riga
  Public Function SerieVmgRecupero(IdIniziale As Integer, IdFinale As Integer) As List(Of Double)
    Dim Ris As New List(Of Double)
    Dim chVmg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
    Dim Meta As Integer = Math.Max(1, DataProvider2020.Hz)
    For i As Integer = IdIniziale To IdFinale
      If chVmg Is Nothing OrElse Not ExitRefVmg > 0 Then
        Ris.Add(Double.NaN)
      Else
        Ris.Add(MediaNaN(chVmg, i - Meta, i + Meta) / ExitRefVmg * 100)
      End If
    Next
    Return Ris
  End Function

  Public Sub New(SecAnte As Integer, SecPost As Integer, KeyMoment As DateTime, IsTack As Boolean)
    UpdatePavarotData(SecAnte, SecPost, KeyMoment, IsTack)
  End Sub

  Private Function AverageValue(Channel As clsChannel2020, IdAnte As Integer, IdPost As Integer, Is360 As Boolean)
    Dim tmp = Channel.Valori.Skip(IdAnte).Take(IdPost - IdAnte).ToArray
    If Is360 Then
      Return Media360(tmp.ToArray)
    Else
      Return Media(tmp.ToArray)
    End If
  End Function

  Public Sub UpdatePavarotData(SecAnte As Integer, SecPost As Integer, KeyMoment As DateTime, IsTack As Boolean)

    SecAnteCalc = SecAnte
    SecPostCalc = SecPost
    VersioneDettagli = VersioneDettagliCorrente
    Try
      Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
      Dim IdAnte As Integer = DataProvider2020.TrovaIndice(KeyMoment.AddSeconds(-SecAnte))
      'Dim IdAnteFine As Integer = DataProvider2020.TrovaIndice(KeyMoment.AddSeconds(-SecAnte - 5))
      Dim IdKm As Integer = DataProvider2020.TrovaIndice(KeyMoment)
      Dim IdPost As Integer = DataProvider2020.TrovaIndice(KeyMoment.AddSeconds(SecPost))
      Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
      Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
      Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
      Dim chCse As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
      Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
      Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
      Dim chLwyN As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLwyNorm)
      Dim chYawRate As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)

      If chCse Is Nothing Then
        chCse = DataProvider2020.CseFromHdgAndLeeway()
      End If

      ' percentile 95 del valore assoluto dello yaw rate nella finestra della manovra (prima si prende la finestra, poi si scartano i NaN)
      Dim r As Double() = Nothing
      If Not chYawRate Is Nothing Then
        r = chYawRate.Valori.Skip(IdAnte).Take(IdPost - IdAnte).Where(Function(x) Not Double.IsNaN(x)).Select(Function(x) Math.Abs(x)).ToArray
      End If
      If Not r Is Nothing AndAlso r.Count > 0 Then
        RotPerc95 = MathNet.Numerics.Statistics.Statistics.Percentile(r, 95)
      Else
        RotPerc95 = Double.NaN
      End If

      Dim Samples As Integer = 5 * DataProvider2020.Hz


      BottomSpeed = chBs.Valori.Skip(IdAnte).Take(IdPost - IdAnte).Where(Function(x) Not Double.IsNaN(x)).Min
      EntryBs = AverageValue(chBs, IdAnte, IdAnte + Samples, False)
      'EntryBs = chBs.PrimoValoreNotNan(IdAnteInizio)
      ExitBs = AverageValue(chBs, IdPost - Samples, IdPost, False)
      'ExitBs = chBs.PrimoValoreNotNan(IdPostFine)

      EntryTwa = AverageValue(chTwa, IdAnte, IdAnte + Samples, False)
      ExitTwa = AverageValue(chTwa, IdPost - Samples, IdPost, False)

      Dim a As Double = AverageValue(chTws, IdAnte, IdAnte + Samples, False)
      Dim p As Double = AverageValue(chTws, IdPost - Samples, IdPost, False)



      EntryExitDeltaTws = p - a

      a = AverageValue(chTwd, IdAnte, IdAnte + Samples, True)
      p = AverageValue(chTwd, IdPost - Samples, IdPost, True)
      If EntryTwa <= 0 Then
        EntryExitDeltaTwd = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(a, p) ' entrando mure a sinistra twd positiva se in uscita mure a dritta il valore e' piu' alto
      Else
        EntryExitDeltaTwd = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(p, a)
      End If

      a = AverageValue(chHdg, IdAnte, IdAnte + Samples, True)
      p = AverageValue(chHdg, IdPost - Samples, IdPost, True)
      EntryExitDeltaHdg = DifferenzaAssolutaTraAngoli360(a, p, True)

      If chCse Is Nothing Then
        If EntryTwa <= 0 Then
          a = SommaAngolo180adAngolo360(chLwyN.PrimoValoreNotNan(IdAnte), a)
          p = SommaAngolo180adAngolo360(-chLwyN.PrimoValoreNotNan(IdPost), p)
        Else
          a = SommaAngolo180adAngolo360(-chLwyN.PrimoValoreNotNan(IdAnte), a)
          p = SommaAngolo180adAngolo360(chLwyN.PrimoValoreNotNan(IdPost), p)
        End If
        EntryExitDeltaCse = DifferenzaAssolutaTraAngoli360(a, p, True)
      Else
        a = AverageValue(chCse, IdAnte, IdAnte + Samples, True)
        p = AverageValue(chCse, IdPost - Samples, IdPost, True)
        EntryExitDeltaCse = DifferenzaAssolutaTraAngoli360(a, p, True)
      End If

      a = AverageValue(chCog, IdAnte, IdAnte + Samples, True)
      p = AverageValue(chCog, IdPost - Samples, IdPost, True)
      EntryExitDeltaCog = DifferenzaAssolutaTraAngoli360(a, p, True)

      Dim PavarotTwa = EntryExitDeltaTwa

      If Not Double.IsNaN(PavarotTwa) AndAlso Not Double.IsNaN(EntryExitDeltaCog) Then
        EntryExitDeltaTwaCog = PavarotTwa - EntryExitDeltaCog
      Else
        EntryExitDeltaTwaCog = Double.NaN
      End If

      If Not Double.IsNaN(PavarotTwa) AndAlso Not Double.IsNaN(EntryExitDeltaCse) Then
        EntryExitDeltaTwaCse = PavarotTwa - EntryExitDeltaCse
      Else
        EntryExitDeltaTwaCse = Double.NaN
      End If
      If Not Double.IsNaN(PavarotTwa) AndAlso Not Double.IsNaN(EntryExitDeltaHdg) Then
        EntryExitDeltaTwaHdg = PavarotTwa - EntryExitDeltaHdg
      Else
        EntryExitDeltaTwaHdg = Double.NaN
      End If

      'If EntryExitDeltaTwaCse > 40 Then Stop

      If Not TgtManager Is Nothing Then
        If Not TgtManager.Tgt Is Nothing Then
          Dim TwsAvg = Media(chTws.Valori.Skip(IdAnte).Take(IdPost - IdAnte).Where(Function(x) Not Double.IsNaN(x)).ToArray)
          Dim TwaTgt As Double = TgtManager.Tgt.ValoreTgt(IsTack, TwsAvg, "bs").Twa
          If Not IsTack Then
            TwaTgt = 180 - TwaTgt
          End If
          EntryExitDeltaTwaTgt = EntryExitDeltaTwa - (2 * TwaTgt)
        End If
      End If






      TwdAxis = Media360(chTwd.Valori.Skip(IdAnte).Take(IdPost - IdAnte).Where(Function(x) Not Double.IsNaN(x)).ToArray)

      If Not chCse Is Nothing Then CalculateCurrent(chCse, chBs, IdAnte, IdPost)

      'LatLonXY.CalcolaXY(KeyMoment, cRate, cDir, axis, SecAnte, SecPost, 1)
      'LatLonXY.CalcolaXYzeroFromHdg(KeyMoment, cRate, cDir, axis, SecPost, SecPost, 1)
      'LatLonXY.CalcolaXYzeroFromPosition(KeyMoment, cRate, cDir, TwdAxis, SecPost, SecPost, 1, Not IsTack)
      'LatLonXY.CalcolaXYzeroFromHdg(KeyMoment, cRate, cDir, axis, SecAnte, SecPost, 1)

      Try
        CalcolaLossAcqua(IdAnte, IdKm, IdPost, IsTack, chBs, chCse, chTwa, chTws)
      Catch ex As Exception
        ' i valori restano NaN: non deve impedire il resto del calcolo
      End Try

      CalcolaVmgLoss(IdAnte, IdPost, SecAnte + SecPost, 5)
      IsValid = True
    Catch ex As Exception
      IsValid = False
    End Try
  End Sub


  Private Sub CalculateCurrent(chCse As clsChannel2020, chBs As clsChannel2020, idInizale As Integer, idFinale As Integer)
    Try
      Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
      Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
      Dim PIOG As New clsGeographicPosition(chLat.PrimoValoreNotNan(idInizale), chLon.PrimoValoreNotNan(idInizale))
      Dim pOverWater As clsGeographicPosition = PIOG
      For i As Integer = idInizale To idFinale
        pOverWater = clsGeoCalculations.PuntoDestinazione(pOverWater, chBs.PrimoValoreNotNan(i) * 1852 / 3600, chCse.PrimoValoreNotNan(i))
        'pOverWater = pTmp
      Next
      Dim PFOG As New clsGeographicPosition(chLat.PrimoValoreNotNan(idFinale), chLon.PrimoValoreNotNan(idFinale))
      cDir = clsGeoCalculations.BearingDegrees(PFOG, pOverWater)
      Dim t As TimeSpan = DataProvider2020.Momento(idFinale).Subtract(DataProvider2020.Momento(idInizale))
      cRate = clsGeoCalculations.DistanceMeters(PFOG, pOverWater) / t.TotalSeconds * 3600 / 1852
    Catch ex As Exception
      cRate = 0
      cDir = 0
    End Try
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBearAwayDetails2021

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsRoundUpDetails2021

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsAccelerationDetails2021

End Class




<AddINotifyPropertyChangedInterface>
Public Class clsFiltroDati
  Public TipoFiltro As eTipoFiltro
  Public AbsVal As Boolean
  Public MinVal As Double
  Public MaxVal As Double

  Public Enum eTipoFiltro
    eBetween
    eMin
    eMax
    eNone
  End Enum

  Public Sub New(Max As Double, Min As Double, Abs As Boolean)
    ImpostaTipoFiltro(Max, Min, Abs)
  End Sub

  Public Sub ImpostaTipoFiltro(Max As Double, Min As Double, Abs As Boolean)
    MaxVal = Max
    MinVal = Min
    AbsVal = Abs
    If Not Double.IsNaN(Min) Then
      If Not Double.IsNaN(Max) Then
        TipoFiltro = eTipoFiltro.eBetween
      Else
        TipoFiltro = eTipoFiltro.eMin
      End If
    ElseIf Not Double.IsNaN(Max) Then
      TipoFiltro = eTipoFiltro.eMax
    Else
      TipoFiltro = eTipoFiltro.eNone
    End If
  End Sub

  Public Function DatoValido(Valore As Double) As Boolean
    If Double.IsNaN(Valore) Then
      Return False
    Else
      If AbsVal Then Valore = System.Math.Abs(Valore)
      Select Case TipoFiltro
        Case eTipoFiltro.eBetween
          Return Valore >= MinVal And Valore <= MaxVal
        Case eTipoFiltro.eMax
          Return Valore <= MaxVal
        Case eTipoFiltro.eMin
          Return Valore >= MinVal
        Case eTipoFiltro.eNone
          Return True
      End Select
    End If
    Return True
  End Function

End Class


'Public Class BlackBoxDataProvider
'  Public DataSourceName As String
'  Public LastTimeStampFromFiles As DateTime
'  Public TimeStamps As DateTime()
'  Public ListaCanali As New List(Of ParquetChannel)
'  Public ReadOnly Property TimeRange
'    Get
'      If TimeStamps.Count = 0 Then Return Nothing
'      Return New clsTimeRange(Inizio, Fine)
'    End Get
'  End Property

'  Public ReadOnly Property Inizio As DateTime
'    Get
'      If TimeStamps.Count = 0 Then Return Nothing
'      Return TimeStamps.Where(Function(x) Not x = Nothing).Min
'    End Get
'  End Property
'  Public ReadOnly Property Fine As DateTime
'    Get
'      If TimeStamps.Count = 0 Then Return Nothing
'      Return TimeStamps.Where(Function(x) Not x = Nothing).Max
'    End Get
'  End Property
'  Public ReadOnly Property LatestDateTimeAvailable As DateTime
'    Get
'      If TimeStamps Is Nothing Then Return LastTimeStampFromFiles
'      Return TimeStamps.Where(Function(x) Not x = Nothing).Max
'    End Get
'  End Property

'  Public Sub New(DataSource As String)
'    DataSourceName = DataSource
'  End Sub


'  Public Sub AggiornaValori(Risultato As Dictionary(Of String, Double()))
'    Dim TS = Risultato.Where(Function(x) x.Key = "SystemTime_DaySeconds").FirstOrDefault
'    Dim TSS = TS.Value.Select(Function(x) Today.AddSeconds(x)).ToArray
'    Dim Altri = Risultato.Where(Function(x) Not x.Key = "SystemTime_DaySeconds").ToList
'    If TimeStamps Is Nothing Then
'      TimeStamps = TSS
'      For Each Canale In Altri
'        Dim c = ListaCanali.Where(Function(x) x.DataSourceHeader = Canale.Key).FirstOrDefault
'        If Not c Is Nothing Then
'          c.Valori = Canale.Value
'        End If
'      Next
'    Else
'      Stop
'      Dim TmpDT As New List(Of DateTime)
'      TmpDT.AddRange(TimeStamps)
'      TmpDT.AddRange(TSS)
'      TimeStamps = TmpDT.ToArray
'      For Each Canale In Altri
'        Dim c = ListaCanali.Where(Function(x) x.DataSourceHeader = Canale.Key).FirstOrDefault
'        If Not c Is Nothing Then
'          Dim TmpBdl As New List(Of Double)
'          TmpBdl.AddRange(c.Valori)
'          TmpBdl.AddRange(Canale.Value)
'          c.Valori = TmpBdl.ToArray
'        End If
'      Next
'    End If
'  End Sub



'End Class


<AddINotifyPropertyChangedInterface>
Public Class clsArrayStats
  Public ValoriRaw As Double()
  Public ValoriNotNan As Double()
  Public ValoriNotNanSin As Double()
  Public ValoriNotNanCos As Double()
  Public Is360 As Boolean
  Public Property Avg As Double
  Public Property Max As Double 'Max Right is 360
  Public Property Min As Double 'Max Left is 360
  Public Property StdDev As Double
  Public Property Distribuzione As Tuple(Of Double(), Double())

  Public Sub AggiornaValori(Valori As Double())
    ValoriRaw = Valori
    If Valori.Count = 0 Then
      Avg = 0
      Max = 0
      Min = 0
      Distribuzione = New Tuple(Of Double(), Double())({0}, {0})
      Exit Sub
    ElseIf Valori.Count = 1 Then
      Avg = Valori(0)
      Max = Valori(0)
      Min = Valori(0)
      Distribuzione = New Tuple(Of Double(), Double())({0}, {0})
      Exit Sub
    End If
    ValoriNotNan = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    Avg = ValoriNotNan.Average
    Max = ValoriNotNan.Max
    Min = ValoriNotNan.Min
    Dim Intervalli As Integer = 10
    Dim BandWidth As Integer = 10
    If Is360 Then
      ValoriNotNanSin = ValoriNotNan.Select(Function(x) System.Math.Sin(Radians(x))).ToArray
      ValoriNotNanCos = ValoriNotNan.Select(Function(x) System.Math.Cos(Radians(x))).ToArray
      Dim s As Double = ValoriNotNanSin.Sum
      Dim c As Double = ValoriNotNanCos.Sum
      Avg = Degrees(System.Math.Atan2(s, c))
      Dim ValoriNotNanRad = ValoriNotNan.Select(Function(x) Radians(x)).ToArray
      StdDev = Degrees(Accord.Statistics.Circular.StandardDeviation(ValoriNotNanRad))
      Dim sr As New List(Of Double)
      Dim sl As New List(Of Double)
      For Each d As Double In ValoriNotNan
        Dim delta As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Avg, d)
        If delta > 0 Then
          sr.Add(Radians(delta))
        Else
          sl.Add(Radians(delta))
        End If
      Next
      Max = SommaAngolo180adAngolo360(Degrees(Accord.Statistics.Circular.StandardDeviation(sr.ToArray)), Avg)
      Min = SommaAngolo180adAngolo360(-Degrees(Accord.Statistics.Circular.StandardDeviation(sl.ToArray)), Avg)

      'impostazioni per la distribuzione
      BandWidth = 10
      Intervalli = 360 / BandWidth
    Else
      alglib.basestat.sampleadev(ValoriNotNan, ValoriNotNan.Count, StdDev)
      BandWidth = CInt(Math.Abs(Max - Min) / Intervalli)
    End If
    Dim xx(Intervalli - 1) As Double
    Dim yy(Intervalli - 1) As Double
    Dim Totale As Double = 0
    For i As Integer = 0 To Intervalli - 1
      Dim x As Double = ((i * BandWidth) + (BandWidth / 2))
      Dim kde = MathNet.Numerics.Statistics.KernelDensity.EstimateGaussian(x, BandWidth, ValoriNotNan)
      xx(i) = x
      yy(i) = kde
      Totale += yy(i)
    Next
    For i As Integer = 0 To Intervalli - 1
      yy(i) = yy(i) / Totale * 100
    Next
    Distribuzione = New Tuple(Of Double(), Double())(xx, yy)
  End Sub


  Public Sub New(TypeIs360 As Boolean)
    Is360 = TypeIs360
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class ParquetDataSource
  Public Valid As Boolean
  Public FileInfo As System.IO.FileInfo
  Public TimeStamps As DateTime()
  Public ListaCanali As New List(Of ParquetChannel)
  Public ParquetReader As Parquet.ParquetReader
  Dim ParquetRowGroupReader As Parquet.ParquetRowGroupReader
  Dim FsTmp As System.IO.StreamReader

  Public ReadOnly Property DataSourceName As String
    Get
      Return FileInfo.Name.Substring(0, 4)
    End Get
  End Property

  Public ReadOnly Property CanaleDaHeader(Header As String) As ParquetChannel
    Get
      Return ListaCanali.Where(Function(x) x.DataSourceHeader = Header).FirstOrDefault
    End Get
  End Property

  Public ReadOnly Property TimeRange
    Get
      If TimeStamps.Count = 0 Then Return Nothing
      Return New clsTimeRange(Inizio, Fine)
    End Get
  End Property

  Public ReadOnly Property Inizio As DateTime
    Get
      If TimeStamps.Count = 0 Then Return Nothing
      Return TimeStamps.Where(Function(x) Not x = Nothing).Min
    End Get
  End Property
  Public ReadOnly Property Fine As DateTime
    Get
      If TimeStamps.Count = 0 Then Return Nothing
      Return TimeStamps.Where(Function(x) Not x = Nothing).Max
    End Get
  End Property

  Public Sub New(FilePath As String)
    FileInfo = New System.IO.FileInfo(FilePath)
    ImpostazioniIniziali()
  End Sub

  Private Sub ImpostazioniIniziali()
    FsTmp = New System.IO.StreamReader(FileInfo.FullName)
    ParquetReader = New Parquet.ParquetReader(FsTmp.BaseStream)
    Dim PqDataFileds As List(Of Parquet.Data.DataField) = ParquetReader.Schema.GetDataFields.ToList
    Dim IntestazioniTmp = ParquetReader.Schema.GetDataFields.Select(Function(x) x.Name).ToList
    ParquetRowGroupReader = ParquetReader.OpenRowGroupReader(0)
    For Each intestazione In IntestazioniTmp
      Dim Header As String = intestazione.Replace(vbNullChar, "").Replace(" ", "_")
      Dim PC As ParquetChannel = ParquetChannels.TrovaCanale(Header)
      If PC Is Nothing Then PC = New ParquetChannel(Header, DataSourceName) ' viene creato assegnando valori di default
      ListaCanali.Add(PC)
    Next
    ImpostaParquet()
  End Sub

  Public Function ValoriDbl(Canale As ParquetChannel) As Double()
    If Canale.Valori Is Nothing Then
      ImpostaValoriCanale(Canale)
    End If
    Return Canale.Valori
  End Function

  Private Sub ImpostaValoriCanale(Canale As ParquetChannel)
    If Canale.ChannelDetails.IsMath Then
      Canale.Valori = SailingChannels.ValoriCanaleMath(Canale.DataSourceHeader, Me)
    Else
      Dim ParquetDataField As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = Canale.DataSourceHeader).FirstOrDefault
      Dim ParquetRowGroupReader = ParquetReader.OpenRowGroupReader(0)
      Dim DatiCanale = ParquetRowGroupReader.ReadColumn(ParquetDataField).Data
      Canale.Valori = DatiCanale
    End If

  End Sub

  Private Sub ImpostaParquet()
    Dim HeaderCanaleSecFromMidnight As String = "SystemTime_DaySeconds"
    Dim HeaderCanaleData As String = "SystemTime_Date"
    Dim CanaleSecFromMidnight = ListaCanali.Where(Function(x) x.DataSourceHeader = HeaderCanaleSecFromMidnight)
    Dim CanaleData = ListaCanali.Where(Function(x) x.DataSourceHeader = HeaderCanaleData)

    Dim ParquetSecFromMidnightDataField As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = HeaderCanaleSecFromMidnight).FirstOrDefault
    If Not ParquetSecFromMidnightDataField Is Nothing Then
      Dim SecFromMidnight = ParquetRowGroupReader.ReadColumn(ParquetSecFromMidnightDataField).Data
      Dim ParquetDateDataField As Parquet.Data.DataField = ParquetReader.Schema.GetDataFields.Where(Function(x) x.Name = HeaderCanaleData).FirstOrDefault
      Dim FileDate As DateTime
      If ParquetDateDataField Is Nothing Then
        'cerca dal nome del file
        Dim Parent As String = FileInfo.Directory.Name
        If IsNumeric(Parent) Then
          'il file é contenuto in una cartella il cui nome é la data
          FileDate = New DateTime(Parent.Substring(0, 4), Parent.Substring(4, 2), Parent.Substring(6, 2), 0, 0, 0)
        Else
          'cerca la data nel nome del file
          For Each Str As String In FileInfo.Name.Split("_")
            If IsNumeric(Str) Then
              FileDate = New DateTime(Str.Substring(0, 4), Str.Substring(4, 2), Str.Substring(6, 2), 0, 0, 0)
              Exit For
            End If
          Next
        End If
      Else
        'lo ricava dal primo valore del campo data
        Dim DataSourceDate = ParquetRowGroupReader.ReadColumn(ParquetDateDataField).Data
        Dim tmp = DirectCast(DataSourceDate, Array)(0)
        FileDate = New DateTime(tmp.ToString.Substring(0, 4), tmp.ToString.Substring(4, 2), tmp.ToString.Substring(6, 2), 0, 0, 0)
      End If
      ReDim TimeStamps(SecFromMidnight.Length - 1)
      For i As Integer = 0 To SecFromMidnight.Length - 1
        If Double.IsNaN(SecFromMidnight(i)) Then
          TimeStamps(i) = Nothing
        Else
          TimeStamps(i) = FileDate.AddSeconds(SecFromMidnight(i))
        End If
      Next
      Valid = True
    Else
      Valid = False
    End If

  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class ParquetChannels
  Public Shared Property PathFile As String
  Public Shared KnownChannels As List(Of ParquetChannel)

  Public Shared Function TrovaCanale(Intestazione As String) As ParquetChannel
    If KnownChannels Is Nothing Then Return Nothing
    Return KnownChannels.Where(Function(x) x.DataSourceHeader = Intestazione)
  End Function

  Public Sub New(PathFileJson As String, ListaKnownChannels As List(Of ParquetChannel))
    PathFile = PathFileJson
    KnownChannels = ListaKnownChannels
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class ParquetChannel
  Public DataSourceName As String
  Public DataSourceHeader As String
  Public SailingChannel As SailingChannel
  Public ChannelDetails As ChannelDetails

  <JsonIgnore>
  Public Valori As Double()

  Public Sub New()
    'passa da qui quando lo carica da json
  End Sub

  Public Sub New(Intestazione As String, SourceName As String)
    ' se passa da qui significa che il canale non è noto
    DataSourceHeader = Intestazione
    DataSourceName = SourceName
    ' cerca tra i sailing channels
    SailingChannel = SailingChannels.TrovaCanaleDaIntestazioneNota(Intestazione)
    If SailingChannel Is Nothing Then
      ' gli associa valori di default dal nome dell intestazione
      ChannelDetails = SailingChannels.ValoriDefaultDaIntestazione(Intestazione)
    Else
      ' gli assegna valori di dettaglio del sailing channel
      ChannelDetails = SailingChannel.DefaultChannelDetails
    End If
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class ChannelDetails
  Public ShortName As String
  Public LongName As String
  Public ShortUM As String
  Public LongUM As String
  Public DataType As eDataType
  Public PolarHeader As String
  Public MathInputDataSource As String
  Public Decimals As Integer

  Public ReadOnly Property IsMath As Boolean
    Get
      Return Not (MathInputDataSource.Trim = "")
    End Get
  End Property

  Public Enum eDataType
    eLinear = 0
    e180 = 1
    e360 = 2
    eAbsLinear = 3
    eAbs180 = 4
    eTack = 5
    eTackReversed = 6
    eBoolean = 7
    ePercentage = 8
    eDataTime = 9
  End Enum

End Class

<AddINotifyPropertyChangedInterface>
Public Class SailingChannel
  Public SailingChannelName As String
  Public DefaultChannelDetails As New ChannelDetails
  Public KnownHeaders As New List(Of String)

End Class

<AddINotifyPropertyChangedInterface>
Public Class SailingChannels
  Public Shared TimeStamps As DateTime()
  Public Shared ListaCanaliSailingAndMath As New List(Of SailingChannel)

  Public Shared ReadOnly Property SailingChannels As List(Of SailingChannel)
    Get
      Return ListaCanaliSailingAndMath.Where(Function(x) x.DefaultChannelDetails.IsMath = False).ToList
    End Get
  End Property

  Public Shared ReadOnly Property MathChannels As List(Of SailingChannel)
    Get
      Return ListaCanaliSailingAndMath.Where(Function(x) x.DefaultChannelDetails.IsMath = True).ToList
    End Get
  End Property

  Public Shared Function ValoriCanaleMath(Intestazione As String, DataSource As ParquetDataSource) As Double()
    Select Case Intestazione
      Case "Vmg_Math"
        Dim TR As clsTimeRange = Nothing
        Dim Bs As Double() = DataSource.ValoriDbl(DataSource.CanaleDaHeader("Bs"))
        Dim Twa As Double() = DataSource.ValoriDbl(DataSource.CanaleDaHeader("Twa"))
        Dim vmg(Bs.Count - 1) As Double
        For i As Integer = 0 To Bs.Count - 1
          vmg(i) = Bs(i) * Math.Abs(Math.Cos(Radians(Twa(i))))
        Next
        Return vmg
      Case Else
        Return Nothing
    End Select
    Return Nothing
  End Function

  Public Shared Function TrovaCanaleDaIntestazioneNota(Intestazione As String) As SailingChannel
    Dim lc As New List(Of SailingChannel)
    For Each c In SailingChannels
      If c.KnownHeaders.Where(Function(x) x.ToLower = Intestazione.ToLower).Count > 0 Then
        lc.Add(c)
      End If
    Next
    If lc.Count = 0 Then
      Return Nothing
    ElseIf lc.Count = 1 Then
      Return lc.First
    Else
      Stop
      Return lc.First
    End If

  End Function


  Public Shared Function Contiene(NomeSp As String(), sigla As String, CaseSensitive As Boolean) As Boolean
    If CaseSensitive Then
      Return NomeSp.Where(Function(x) x = sigla).Count > 0
    Else
      Return NomeSp.Where(Function(x) x.ToLower = sigla.ToLower).Count > 0
    End If
  End Function

  Public Shared Function ValoriDefaultDaIntestazione(Intestazione As String) As ChannelDetails
    Dim NomeSp As String() = Intestazione.Split("_")
    Dim Details As New ChannelDetails
    Dim IsAp As Boolean = Contiene(NomeSp, "AP", True)

    Details.ShortName = NomeSp(0)
    Details.LongName = NomeSp(0)
    Details.MathInputDataSource = ""
    Details.PolarHeader = ""
    Details.Decimals = 1
    Details.DataType = ChannelDetails.eDataType.eLinear


    If Contiene(NomeSp, "ang", False) Then
      Details.DataType = ChannelDetails.eDataType.e180
      Details.ShortUM = "°"
      Details.LongUM = "deg"
    ElseIf Contiene(NomeSp, "pos", False) Then
      Details.ShortUM = "mm"
      Details.LongUM = "mm"
    ElseIf Contiene(NomeSp, "press", False) Then
      Details.ShortUM = "b"
      Details.LongUM = "bar"
    ElseIf Contiene(NomeSp, "deg", False) Then
      Details.DataType = ChannelDetails.eDataType.e180
      Details.ShortUM = "°"
      Details.LongUM = "deg"
    ElseIf Contiene(NomeSp, "load", False) Then
      Details.ShortUM = "t"
      Details.LongUM = "ton"
    ElseIf Contiene(NomeSp, "strain", False) Then
      Details.ShortUM = "n"
      Details.LongUM = "nstr"
    ElseIf Contiene(NomeSp, "m", False) Then
      Details.ShortUM = "m"
      Details.LongUM = "mt"
    ElseIf Contiene(NomeSp, "kts", False) Then
      Details.ShortUM = "k"
      Details.LongUM = "kts"
    ElseIf Contiene(NomeSp, "SystemTime", False) Then
      Details.ShortName = NomeSp(1)
      Details.LongName = Intestazione
      Details.MathInputDataSource = ""
      Details.PolarHeader = ""
      Details.Decimals = 0
      Details.DataType = ChannelDetails.eDataType.eDataTime
      Details.ShortUM = ""
      Details.LongUM = ""
    Else
      Details.ShortUM = ""
      Details.LongUM = ""
    End If
    Dim trisixti As String() = {"hdg", "cog", "cse", "twd", "gwd"}
    For Each t As String In trisixti
      If Contiene(NomeSp, t, False) Then
        Details.DataType = ChannelDetails.eDataType.e360
        Exit For
      End If
    Next
    Dim Coordinate As String() = {"lat", "lon", "lng", "latbow", "lonbow", "lngbow"}
    For Each t As String In Coordinate
      If Contiene(NomeSp, t, False) Then
        Details.Decimals = 8
        Details.LongUM = "deg"
        Details.ShortUM = "°"
        Details.DataType = ChannelDetails.eDataType.eLinear
        Exit For
      End If
    Next
    Return Details
  End Function

End Class

'Public Class clsBoatConfigML
'  Public ConfigValidFrom As DateTime
'  Public ConfigValidTo As DateTime
'  Public Boat As String
'  Public RudderConfig As String
'  Public RudderName As String
'  Public PortArmConfig As String
'  Public PortArmName As String
'  Public PortWingConfig As String
'  Public PortWingName As String
'  Public StbdArmConfig As String
'  Public StbdArmName As String
'  Public StbdWingConfig As String
'  Public StbdWingName As String
'End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBoatConfigs
  Dim _ListaBoatConfigs As New List(Of clsBoatConfig)

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsBoatConfig
  Dim _DescrizioneConfig As String
  Dim _IdConfig As Integer
  Dim _Inizio As DateTime
  Dim _Fine As DateTime
  Dim _Boat As New clsItemConfig(clsItemConfig.eConfigGroup.eBoat)
  Dim _BoatDetail As New clsItemConfig(clsItemConfig.eConfigGroup.eBoatDetail)
  Dim _Rig As New clsItemConfig(clsItemConfig.eConfigGroup.eRig)
  Dim _Rudder As New clsItemConfig(clsItemConfig.eConfigGroup.eRudder)
  Dim _PortArm As New clsItemConfig(clsItemConfig.eConfigGroup.eArm)
  Dim _PortWing As New clsItemConfig(clsItemConfig.eConfigGroup.eWing)
  Dim _StbdArm As New clsItemConfig(clsItemConfig.eConfigGroup.eArm)
  Dim _StbdWing As New clsItemConfig(clsItemConfig.eConfigGroup.eWing)
  Dim _Logic As New clsItemConfig(clsItemConfig.eConfigGroup.eLogic)

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsItemConfig
  Dim _Group As eConfigGroup
  Dim _IdConfig As Integer
  Dim _Description As String

  Public Sub New(Group As eConfigGroup)
    _Group = Group
  End Sub

  Public Enum eConfigGroup
    eBoat = 0
    eBoatDetail = 1
    eRig = 2
    eRudder = 3
    eArm = 4
    eWing = 5
    eLogic = 6
  End Enum

End Class


'<AddINotifyPropertyChangedInterface>
'Public Class clsRaceLeg
'  Public TR As clsTimeRange
'  Public IsUpwind As Boolean
'  Public TacksStbdToPort As New List(Of clsTimeRange)
'  Public GybesStbdToPort As New List(Of clsTimeRange)
'  Public TacksPortToStbd As New List(Of clsTimeRange)
'  Public GybesPortToStbd As New List(Of clsTimeRange)
'  Public StraightLinesStbd As New List(Of clsTimeRange)
'  Public StraightLinesPort As New List(Of clsTimeRange)
'  Public TopMarkRounding As New List(Of clsTimeRange)
'  Public BottomMarkRounding As New List(Of clsTimeRange)


'End Class

'<AddINotifyPropertyChangedInterface>
'Public Class clsRaceAnalyser
'  Public TR As clsTimeRange
'  Public Legs As New List(Of clsRaceLeg)
'  Public Tacks As New List(Of clsTimeRange)
'  Public Gybes As New List(Of clsTimeRange)
'  Public RoundUps As New List(Of clsTimeRange)
'  Public BearAways As New List(Of clsTimeRange)



'  Public Sub New(TimeRange As clsTimeRange)
'    TR = TimeRange

'  End Sub

'  Private Enum eAndatura
'    eNan = -1
'    eTack = 0
'    eUpwind = 1
'    eReaching = 2
'    eDownWind = 3
'    eGybing = 4
'  End Enum

'  Private Function AndaturaDaTwa(Twa As Double) As eAndatura
'    If Double.IsNaN(Twa) Then Return eAndatura.eNan
'    Select Case Math.Abs(Twa)
'      Case 0 - 15
'        Return eAndatura.eTack
'      Case 15 - 65
'        Return eAndatura.eUpwind
'      Case 65 - 120
'        Return eAndatura.eReaching
'      Case 120 - 165
'        Return eAndatura.eDownWind
'      Case Else
'        Return eAndatura.eGybing
'    End Select
'  End Function

'  Private Sub TrovaKeyMoments()
'    Dim IdIniziale As Integer = DataProvider2020.TrovaIndice(TR.Start)
'    Dim IdFinale As Integer = DataProvider2020.TrovaIndice(TR.Finish)
'    Dim ChTWA As clsChannel2020 = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eTWA)

'    Dim IndiceAndatura As New Dictionary(Of DateTime, eAndatura)

'    For i As Integer = IdIniziale To IdFinale
'      IndiceAndatura.Add(DataProvider2020.Momento(i), AndaturaDaTwa(ChTWA.Valori(i)))
'    Next

'    Dim AndaturaPrev As eAndatura = IndiceAndatura.First.Value
'    For Each e In IndiceAndatura
'      Dim Andatura As eAndatura = e.Value
'      If Not Andatura = AndaturaPrev Then
'        Select Case Andatura
'          Case eAndatura.eNan
'          Case eAndatura.eTack
'            If Tacks.Count = 0 Then
'              Tacks.Add(New clsTimeRange(e.Key.AddSeconds(-5), Nothing))
'            End If
'        End Select
'      End If

'      AndaturaPrev = Andatura
'    Next


'  End Sub


'End Class


<AddINotifyPropertyChangedInterface>
Public Class clsMarkPosition

  Public Property Positions As New Dictionary(Of DateTime, clsGeographicPosition)

  Public Sub AddIfNew(DT As DateTime, Position As clsGeographicPosition)
    If Not Positions.ContainsKey(DT) Then
      If Positions.Where(Function(x) x.Value.LatDec = Position.LatDec And x.Value.LngDec = Position.LngDec).Count = 0 Then
        Positions.Add(DT, Position)
      End If
    End If
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsExpeditionStarts
  Public Property StartsList As New ObservableCollection(Of clsExpeditionStart)
  Dim _SelectedStart As clsExpeditionStart = Nothing
  Public Property VisibleRangeReport As New SummaryReport

  Public Property VisibleRange As SciChart.Data.Model.DoubleRange = New SciChart.Data.Model.DoubleRange(-500, 500)


  Dim _StartPlotHeight As Double
  Public Property StartPlotHeight As Double
    Get
      Return _StartPlotHeight
    End Get
    Set(value As Double)
      _StartPlotHeight = value
    End Set
  End Property

  Public Property BoatLenghtInMeters As Double
    Get
      Return AppConfig.ActiveProfile.BoatLenghtInMeters
    End Get
    Set(value As Double)
      AppConfig.ActiveProfile.BoatLenghtInMeters = value
    End Set
  End Property

  Public Property SelectedStart As clsExpeditionStart
    Get
      Return _SelectedStart
    End Get
    Set(value As clsExpeditionStart)
      _SelectedStart = value
      VisibleRange.Min = SelectedStartMinValue() * 1.1
      VisibleRange.Max = SelectedStartMaxValue() * 1.1
      If Not SelectedStart Is Nothing Then SelectedStart.CurrentBoatData = SelectedStart.BoatDataAtZero
    End Set
  End Property

  Private Function SelectedStartMinValue() As Double
    If SelectedStart Is Nothing Then Return -1000
    Dim v = Math.Min(SelectedStart.PrestartBoatData.Min(Function(x) x.RelativePositionXY.X), SelectedStart.PrestartBoatData.Min(Function(x) x.RelativePositionXY.Y))
    v = Math.Min(v, SelectedStart.PrestartBoatData.Min(Function(x) x.PinPllX))
    Return Math.Min(0, v)
  End Function

  Private Function SelectedStartMaxValue() As Double
    If SelectedStart Is Nothing Then Return 1000
    Dim v = Math.Max(SelectedStart.PrestartBoatData.Max(Function(x) x.RelativePositionXY.X), SelectedStart.PrestartBoatData.Max(Function(x) x.RelativePositionXY.Y))
    v = Math.Max(v, SelectedStart.PrestartBoatData.Max(Function(x) x.RcSllX))
    v = Math.Max(v, SelectedStart.PrestartBoatData.Max(Function(x) x.RcPllY))
    v = Math.Max(v, SelectedStart.PrestartBoatData.Max(Function(x) x.RcSllY))
    v = Math.Max(v, SelectedStart.PrestartBoatData.Max(Function(x) x.PinPllY))
    v = Math.Max(v, SelectedStart.PrestartBoatData.Max(Function(x) x.PinSllY))
    Return Math.Max(SelectedStart.LineRcX, v)
  End Function

  Public Sub CercaPartenze()
    If DataProvider2020 Is Nothing Then Exit Sub
    If DataProvider2020.ValoriCaricati = False Then Exit Sub
    Dim StartTimes As New List(Of DateTime)
    Dim RcPositions As clsMarkPosition
    Dim PinPositions As clsMarkPosition
    Dim chTimeToGun As clsChannel2020 = DataProvider2020.CanaleDbl("TmToGun")
    StartTimes.Clear()
    SelectedStart = Nothing
    If chTimeToGun Is Nothing Then Exit Sub
    If chTimeToGun.Valori Is Nothing Then Exit Sub
    If chTimeToGun.Valori.Count = 0 Then Exit Sub
    For i As Integer = 0 To chTimeToGun.Valori.Count - 2
      Dim Tm As Double = chTimeToGun.Valori(i)
      Dim TmNext As Double = chTimeToGun.Valori(i + 1)
      If Tm = 0 Then
        StartTimes.Add(DataProvider2020.Momento(i))
        'ElseIf Tm > 0 AndAlso TmNext < 0 Then
        '  ' (x-x1)/(x2-x1)=(y-y1)/(y2-y1)
        '  'y-y1=(y2-y1)*(x-x1)/(x2-x1)
        '  'y-0=(1-0)*(0-Tm)/(TmNext-Tm)
        '  'y=-Tm/(TmNext-Tm)
        '  Dim v As Double = -Tm * (TmNext - Tm)
        '  StartTimes.Add(DataProvider2020.Momento(i).AddDays(v))
      ElseIf Tm > 0 AndAlso TmNext < 0 Then
        ' Tm is the time to the gun at sample i, expressed in days.
        ' The gun therefore falls at Momento(i) + Tm, with no interpolation needed:
        ' the sign change only tells us the crossing happens within this interval.
        Dim tt As Double = Tm * 24 * 3600
        If tt < 10 Then
          StartTimes.Add(DataProvider2020.Momento(i).AddSeconds(tt))
        End If
      ElseIf Tm > 0 AndAlso Double.IsNaN(TmNext) Then
        ' (x-x1)/(x2-x1)=(y-y1)/(y2-y1)
        'y-y1=(y2-y1)*(x-x1)/(x2-x1)
        'y-0=(1-0)*(0-Tm)/(TmNext-Tm)
        'y=-Tm/(TmNext-Tm)
        Dim tt = Tm * 24 * 3600
        Dim v As Double = -Tm * (TmNext - Tm)
        If tt < 10 Then
          StartTimes.Add(DataProvider2020.Momento(i).AddSeconds(tt))
        End If
      ElseIf Tm > 0 And TmNext > Tm And TmNext > (1 / 24 / 60 / 60 * 100) Then 'per le rolling starts
        Dim tt = Tm * 24 * 3600
        If tt < 10 Then
          StartTimes.Add(DataProvider2020.Momento(i).AddSeconds(tt))
        End If

        Dim a As Integer = 0
        'Stop
      End If
    Next

    RcPositions = PosizioniBoa("Stbd lat", "Stbd lon")
    PinPositions = PosizioniBoa("Port lat", "Port lon")
    'RcPositions.Positions.Remove(RcPositions.Positions.Last.Key)

    StartsList.Clear()

    For Each St In StartTimes
      If RcPositions.Positions.Where(Function(x) x.Key <= St).Count > 0 Then
        If PinPositions.Positions.Where(Function(x) x.Key <= St).Count > 0 Then
          Dim se As clsGeographicPosition = RcPositions.Positions.Where(Function(x) x.Key <= St).OrderBy(Function(x) x.Key).Last.Value
          Dim pe As clsGeographicPosition = PinPositions.Positions.Where(Function(x) x.Key <= St).OrderBy(Function(x) x.Key).Last.Value
          If DataProvider2020.Hz = 1 AndAlso St.Millisecond > 500 Then
            Dim NS = New clsExpeditionStart(St.AddMilliseconds(1000 - St.Millisecond), se, pe)
            If NS.IsValid Then StartsList.Add(NS)
          Else
            Dim NS = New clsExpeditionStart(St, se, pe)
            If NS.IsValid Then StartsList.Add(NS)
          End If
        End If
      End If
    Next
    If StartsList.Count > 0 Then
      SelectedStart = StartsList.First
    Else
      SelectedStart = Nothing
    End If
  End Sub

  Private Function PosizioniBoa(ChannelNameLat As String, ChannelNameLon As String) As clsMarkPosition
    Dim ChannelLat As clsChannel2020 = DataProvider2020.CanaleDbl(ChannelNameLat)
    Dim ChannelLon As clsChannel2020 = DataProvider2020.CanaleDbl(ChannelNameLon)
    Dim MP As New clsMarkPosition
    For i As Integer = 0 To ChannelLat.Valori.Count - 2
      Dim lt As Double = ChannelLat.Valori(i)
      Dim ln As Double = ChannelLon.Valori(i)
      If Not Double.IsNaN(lt) AndAlso Not Double.IsNaN(ln) Then
        If IsNumeric(lt) AndAlso IsNumeric(ln) Then
          If Not lt = ln Then
            MP.AddIfNew(DataProvider2020.Momento(i), New clsGeographicPosition(lt, ln))
            'MP.Positions.Add(DataProvider2020.Momento(i), New clsGeograficPosition(lt, ln))
          End If
        End If
      End If
    Next
    Return MP
  End Function


  Public Sub CsvPartenzeToClipboard()
    'Dim t As String = ""
    'For Each P In StartsList
    '  t &= P.EsportaCsvPartenza & vbCrLf & vbCrLf & vbCrLf & vbCrLf
    'Next
    'Clipboard.SetText(t)


    Dim ts As New List(Of String)
    For Each P In StartsList
      Dim nr = P.StartRow(ts.Count = 0)
      If nr.Count > 0 Then ts.AddRange(nr)
    Next


    Clipboard.SetText(String.Join(vbCrLf, ts))
  End Sub

  Dim LastPdfCreatedPath As String
  Public Sub SelectedStartPdfReport(Surface As SciChart.Charting.Visuals.SciChartSurface)
    Dim pdf As New clsPdf
    LastPdfCreatedPath = pdf.StampaReportPartenza(Surface.ExportToStream(SciChart.Core.ExportType.Bmp, False), SelectedStart)
  End Sub


  Public Sub RaceReportPdf(Surface As SciChart.Charting.Visuals.SciChartSurface)
    Dim pdf As New clsPdf
    VisibleRangeReport.setWLRows(DataPlotSync.VisibleRange, SelectedStart.StartTime, 5, 10, 10, 30, 3)
    LastPdfCreatedPath = pdf.StampaReportPartenza(Surface.ExportToStream(SciChart.Core.ExportType.Bmp, False), SelectedStart, VisibleRangeReport)
  End Sub

  Public Sub SendEmailToMailingList()
    If System.IO.File.Exists(LastPdfCreatedPath) Then
      clsEmail.SendEmail(LastPdfCreatedPath)
    End If
  End Sub


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsPrestartBoatData
  Public Property Momento As DateTime
  Public Property Id As Integer
  Public Property SecondsToStart As Integer
  Public Property BoatPosition As clsGeographicPosition
  Public Property GroupDescription As String
  Public Property RelativePositionXY As clsXYpoint
  Public Property Tws As Double
  Public Property Twd As Double
  Public Property Bs As Double
  Public Property BsDerivative As Double
  Public Property AccKtsMin As Double
  Public Property Twa As Double
  Public Property VmgP As Double
  Public Property BstP As Double
  Public Property TwaD As Double
  Public Property Heel As Double
  Public Property Rdr As Double
  Public Property Vdist As Double
  Public Property VdistBL As Double
  Public Property ExpVdist As Double
  Public Property ExpTimeToBurn As Double
  Public Property FinalShut As Double
  Public Property FinalShutMt As Double
  Public Property CustomData1 As Double
  Public Property CustomData2 As Double
  Public Property CustomData3 As Double
  Public Property CustomData4 As Double
  Public Property CustomData5 As Double
  Public Property CustomData6 As Double
  Public Property CustomData7 As Double
  Public Property CustomData8 As Double
  Public Property CustomData9 As Double
  Public Property CustomData1h As String
  Public Property CustomData2h As String
  Public Property CustomData3h As String
  Public Property CustomData4h As String
  Public Property CustomData5h As String
  Public Property CustomData6h As String
  Public Property CustomData7h As String
  Public Property CustomData8h As String
  Public Property CustomData9h As String
  Public Property ExpTimeToLine As Double
  Public Property TtlAtTgtStbd As Double
  Public Property TtlAtTgtPort As Double
  Public Property Sog As Double
  Public Property Cog As Double
  Public Property Hdg As Double
  Public Property YRT As Double
  Public Property HeaderVisible As Visibility = Visibility.Collapsed


  Public Property PinSllX As Integer
  Public Property PinSllY As Integer
  Public Property PinPllX As Integer
  Public Property PinPllY As Integer
  Public Property RcSllX As Integer
  Public Property RcSllY As Integer
  Public Property RcPllX As Integer
  Public Property RcPllY As Integer

  Public Property BoatStbd1mX As Integer
  Public Property BoatStbd1mY As Integer
  Public Property BoatPort1mX As Integer
  Public Property BoatPort1mY As Integer

  Public Property BoatStbdStsX As Integer
  Public Property BoatStbdStsY As Integer
  Public Property BoatPortStsX As Integer
  Public Property BoatPortStsY As Integer

  Public Property BoatPositionX As Integer
  Public Property BoatPositionY As Integer
  Public Property BoatGpsStsX As Integer
  Public Property BoatGpsStsY As Integer

  Public Property BoatSternX As Integer
  Public Property BoatSternY As Integer

  Public Property StbdDist As Double
  Public Property StbdTime As Double

  Public Property StbdTtlWithAcc As Double
  Public Property CalculateAccelerationLoss As Double

  Dim BoatLenghtInMeters As Double



  Public Property CustomData1v As Visibility
  Public Property CustomData2v As Visibility
  Public Property CustomData3v As Visibility
  Public Property CustomData4v As Visibility
  Public Property CustomData5v As Visibility
  Public Property CustomData6v As Visibility
  Public Property CustomData7v As Visibility
  Public Property CustomData8v As Visibility
  Public Property CustomData9v As Visibility

  Public Sub New(Momento As DateTime, Id As Integer, SecondsToStart As Double, BoatLenghtInMeters As Double)
    Me.Momento = Momento
    Me.Id = Id
    Me.SecondsToStart = SecondsToStart
    Me.BoatLenghtInMeters = BoatLenghtInMeters
  End Sub

  Public ReadOnly Property SecondsToGun As String
    Get
      If SecondsToStart = 0 Then
        Return "@Gun " & VdistToLine & " (" & (Vdist / BoatLenghtInMeters).ToString("F1") & ")"
      ElseIf SecondsToStart > 0 Then
        Return "Gun + " & Today.AddSeconds(Math.Abs(SecondsToStart)).ToString("mm:ss") & ", " & VdistToLine & " (" & (Vdist / BoatLenghtInMeters).ToString("F1") & ")"
      Else
        Return "" & Today.AddSeconds(Math.Abs(SecondsToStart)).ToString("mm:ss") & " to start, " & VdistToLine & " (" & (Vdist / BoatLenghtInMeters).ToString("F1") & ")"
      End If
    End Get
  End Property

  Public ReadOnly Property VdistToLine As String
    Get
      Return "Vdist: " & Vdist.ToString("F0") & "m"
    End Get
  End Property


  Public Sub UpdateRelativePosition(RefPosition As clsGeographicPosition, Axis As Double)
    Dim C As Color = Colors.Black
    Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(RefPosition, BoatPosition)
    Dim DistMt As Double = clsGeoCalculations.DistanceMeters(RefPosition, BoatPosition)
    If BstP < 20 Then
      GroupDescription = "BsTgt < 20%"
      C = Colors.Fuchsia
    ElseIf BstP < 40 Then
      C = Colors.DarkRed
      GroupDescription = "BsTgt 20-40%"
    ElseIf BstP < 60 Then
      C = Colors.Red
      GroupDescription = "BsTgt 40-60%"
    ElseIf BstP < 80 Then
      C = Colors.Orange
      GroupDescription = "BsTgt 60-80%"
    ElseIf BstP < 90 Then
      C = Colors.GreenYellow
      GroupDescription = "BsTgt 80-90%"
    Else
      C = Colors.DarkGreen
      GroupDescription = "BsTgt > 90%"
    End If
    'Dim RelativeBearing As Double = SommaAngolo180adAngolo360(Axis, BrgDeg)
    Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Axis, BrgDeg)
    Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
    Dim X As Double = DistMt * System.Math.Cos(Radians(CartBearing))
    Dim Y As Double = DistMt * System.Math.Sin(Radians(CartBearing))
    RelativePositionXY = New clsXYpoint(X, Y, C)
    BoatPositionX = RelativePositionXY.X
    BoatPositionY = RelativePositionXY.Y

  End Sub

  Public Sub UpdateLayLines(PinPosition As clsGeographicPosition, RcPosition As clsGeographicPosition, Axis As Double, TwaTgt As Double, BsTgt As Double, BoatLenghtInMeters As Double)

    Dim tmpP = clsGeoCalculations.PuntoDestinazione(RcPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(180, SommaAngolo180adAngolo360(-TwaTgt, Twd))) ' punto ad un minuto a bstgt lungo la layline 
    Dim tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Green) ' Rc Stbd
    RcSllX = tmp.X
    RcSllY = tmp.Y
    tmpP = clsGeoCalculations.PuntoDestinazione(RcPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(180, SommaAngolo180adAngolo360(TwaTgt, Twd))) ' punto ad un minuto a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Red) ' Rc Port
    RcPllX = tmp.X
    RcPllY = tmp.Y

    tmpP = clsGeoCalculations.PuntoDestinazione(PinPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(180, SommaAngolo180adAngolo360(-TwaTgt, Twd))) ' punto ad un minuto a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Green) ' Pin Stbd
    PinSllX = tmp.X
    PinSllY = tmp.Y
    tmpP = clsGeoCalculations.PuntoDestinazione(PinPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(180, SommaAngolo180adAngolo360(TwaTgt, Twd))) ' punto ad un minuto a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Red) ' Pin Port
    PinPllX = tmp.X
    PinPllY = tmp.Y

    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(-TwaTgt, Twd)) ' punto ad un minuto a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Green) ' boat Stbd 1m
    BoatStbd1mX = tmp.X
    BoatStbd1mY = tmp.Y
    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, KtsToMS(BsTgt) * 60, SommaAngolo180adAngolo360(TwaTgt, Twd)) ' punto ad un minuto a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Red) ' boat Port 1m
    BoatPort1mX = tmp.X
    BoatPort1mY = tmp.Y

    Dim secs As Double = SecondsToStart
    If SecondsToStart > 0 Then secs = 60

    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, KtsToMS(BsTgt) * Math.Abs(secs), SommaAngolo180adAngolo360(-TwaTgt, Twd)) ' punto a secondstostart a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Green) ' boat Stbd Sts
    BoatStbdStsX = tmp.X
    BoatStbdStsY = tmp.Y

    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, KtsToMS(BsTgt) * Math.Abs(secs), SommaAngolo180adAngolo360(TwaTgt, Twd)) ' punto a secondstostart a bstgt lungo la layline 
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Red) ' boat Port sts
    BoatPortStsX = tmp.X
    BoatPortStsY = tmp.Y

    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, KtsToMS(Sog) * Math.Abs(secs), Hdg) ' punto a secondstostart a dati gps corrente
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Red) ' boat projection at sts
    BoatGpsStsX = tmp.X
    BoatGpsStsY = tmp.Y

    tmpP = clsGeoCalculations.PuntoDestinazione(BoatPosition, BoatLenghtInMeters, SommaAngolo180adAngolo360(180, Hdg)) ' posizione poppa
    tmp = CalculatesRelativePositionXY(tmpP, PinPosition, Axis, Colors.Black) ' boat projection at sts
    BoatSternX = tmp.X
    BoatSternY = tmp.Y

  End Sub

  'Public Sub UpdateSafeToLine(PinPos As clsGeograficPosition, RcToPinBrg As Double)
  '  Dim PosTmp As clsGeograficPosition = BoatPosition
  '  Dim LLbrg As Double = SommaAngolo180adAngolo360(-(Twa - TwaD), Twd)
  '  StbdTime = 0
  '  StbdDist = 0
  '  Do
  '    Dim dtmp As Double = Math.Max(KtsToMS(Bs), KtsToMS(Bs / (BstP / 100)))
  '    PosTmp = clsGeoCalculations.PuntoDestinazione(PosTmp, dtmp, LLbrg)
  '    Dim BrgToPin As Double = clsGeoCalculations.BearingDegrees(PosTmp, PinPos)
  '    If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(RcToPinBrg, BrgToPin) <= 0 Then Exit Do
  '    StbdTime += 1
  '    StbdDist += dtmp
  '  Loop


  'End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsExpeditionStartFinalLaunch
  Public Property BottomSpeed As Double
  Public Property BottomSpeedSecToGun As Double
  Public Property AccStartBs As Double
  Public Property AccStartSecToGun As Double
  Public Property BsTgt90SecToGun As Double
  Public Property BsAtBsTgt90 As Double
  Public Property AccBsDelta As Double ' bottom speed to at least 90 of BsTgt
  Public Property AccAvgKtsSec As Double
  Public Property AccMaxKtsSec As Double
  Public Property AccAvgTwa As Double
  Public Property AccAvgTws As Double
  Public Property AccAvgTwd As Double
  Public Property AccMaxTwa As Double
  Public Property VdistAtAccStart As Double
  Public Property BsAtMinus10 As Double
  Public Property dTwaAtMinus10 As Double
  Public Property VdistAtMinus10 As Double
  Public Property BsAtMinus5 As Double
  Public Property dTwaAtMinus5 As Double
  Public Property VdistAtMinus5 As Double

  Public ReadOnly Property AccAvgKtsPerMinute As Double
    Get
      Return AccAvgKtsSec * 60
    End Get
  End Property

  Public ReadOnly Property AccMaxKtsPerMinute As Double
    Get
      Return AccMaxKtsSec * 60
    End Get
  End Property


  Public Sub CalcolaAccelerazione(PrestartBoatData As List(Of clsPrestartBoatData), FromSecToStart As Integer, ToSecToStart As Integer)
    Dim BoatData As New List(Of clsPrestartBoatData)
    Dim BeforeGunBoatData As New List(Of clsPrestartBoatData)
    For Each bd In PrestartBoatData
      If bd.SecondsToStart >= FromSecToStart AndAlso bd.SecondsToStart <= ToSecToStart Then
        BoatData.Add(bd)
      End If
      If bd.SecondsToStart >= FromSecToStart AndAlso bd.SecondsToStart <= 0 Then
        BeforeGunBoatData.Add(bd)
      End If
    Next
    Dim a = BoatData.Select(Function(x) x.Bs).ToList()
    Dim b = BeforeGunBoatData.Select(Function(x) x.Bs).ToList()

    BottomSpeed = BeforeGunBoatData.Min(Function(x) x.Bs)
    BottomSpeedSecToGun = BeforeGunBoatData.Where(Function(x) x.Bs = BottomSpeed).OrderBy(Function(x) x.SecondsToStart).First.SecondsToStart
    Dim BspAtBottomSpeed = BeforeGunBoatData.Where(Function(x) x.Bs = BottomSpeed).OrderBy(Function(x) x.SecondsToStart).First.BstP
    AccStartSecToGun = BottomSpeedSecToGun
    AccStartBs = BottomSpeed
    BsTgt90SecToGun = BoatData.Last.SecondsToStart
    BsAtBsTgt90 = BoatData.Last.Bs
    For Each bd In BoatData
      If bd.BstP < (BspAtBottomSpeed * 0.9) Then
        AccStartSecToGun = bd.SecondsToStart
        AccStartBs = bd.Bs
        BspAtBottomSpeed = bd.BstP
      End If
      If bd.SecondsToStart > BottomSpeedSecToGun Then
        If bd.BstP >= 90 Then
          BsTgt90SecToGun = bd.SecondsToStart
          BsAtBsTgt90 = bd.Bs
          Exit For
        End If
      End If
      BspAtBottomSpeed = Math.Max(bd.BstP, BspAtBottomSpeed)
    Next
    AccBsDelta = BsAtBsTgt90 - AccStartBs
    AccAvgKtsSec = AccBsDelta / (BsTgt90SecToGun - AccStartSecToGun)
    AccMaxKtsSec = AccAvgKtsSec
    Dim Acc As New List(Of clsPrestartBoatData)
    Dim BsPrev As Double = BoatData.First.Bs
    Dim SecPrev As Double = BoatData.First.SecondsToStart - 1
    For Each bd In BoatData
      If bd.SecondsToStart >= AccStartSecToGun AndAlso bd.SecondsToStart <= BsTgt90SecToGun Then
        Acc.Add(bd)
        AccMaxKtsSec = Math.Max(AccMaxKtsSec, (bd.Bs - BsPrev) / (bd.SecondsToStart - SecPrev))
        BsPrev = bd.Bs
        SecPrev = bd.SecondsToStart
      End If
    Next
    'AccMaxKtsSec = Acc.Max(Function(x) x.BsDerivative)
    AccAvgTwa = Acc.Average(Function(x) Math.Abs(x.Twa))
    AccAvgTws = Acc.Average(Function(x) x.Tws)
    AccAvgTwd = Media360(Acc.Select(Function(x) x.Twd).ToArray)
    AccMaxTwa = Acc.Max(Function(x) Math.Abs(x.Twa))

    Dim m10 = BoatData.Where(Function(x) x.SecondsToStart = -10).FirstOrDefault
    Dim m5 = BoatData.Where(Function(x) x.SecondsToStart = -5).FirstOrDefault
    If Not m10 Is Nothing Then
      BsAtMinus10 = m10.Bs
      dTwaAtMinus10 = m10.TwaD
      VdistAtMinus10 = m10.VdistBL
    End If
    If Not m5 Is Nothing Then
      BsAtMinus5 = m5.Bs
      dTwaAtMinus5 = m5.TwaD
      VdistAtMinus5 = m5.VdistBL
    End If

    For Each bd In PrestartBoatData
      If bd.SecondsToStart = AccStartSecToGun Then
        VdistAtAccStart = bd.Vdist / AppConfig.ActiveProfile.BoatLenghtInMeters
        Exit For
      End If
    Next

  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsExpeditionStart
  ' Port lat	Port lon	Stbd lat	Stbd lon
  Public IsValid As Boolean

  Dim StbdEnd As clsGeographicPosition
  Dim PortEnd As clsGeographicPosition
  Dim VirtualPortEnd As clsGeographicPosition
  Dim VirtualStbdEnd As clsGeographicPosition
  Public Property CrossingSeconds As Double
  Public Property StartTime As DateTime
  Public Property GpsStartTime As DateTime
  Dim BoatPositionXYtoPin As New ObservableCollection(Of clsXYpoint)
  Dim RcPositionXYtoPin As clsXYpoint

  Dim twds As clsChannelStats
  Dim twss As clsChannelStats
  Public Property RcToPin As Double
  Public Property LineTwd As Double
  Public Property LineLenght As Double
  Public Property LineBiasDeg As Double
  Public Property LineBiasMt As Double
  Public Property LineBiasSec As Double
  Public Property ZoneAtStart As Double
  Public Const SecondiPrimaDellaPartenza As Integer = 300 ' i dati prestart partono da -5 minuti
  Dim SecondsArray As Integer() = {-60, -50, -40, -30, -26, -24, -22, -20, -18, -16, -15, -14, -13, -12, -11, -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 10, 15, 30, 60}
  Public Property BoatDataTable As New ObservableCollection(Of clsPrestartBoatData) ' sottoinsieme ai soli secondi indicati nel secondsarray
  Public Property PrestartBoatData As New ObservableCollection(Of clsPrestartBoatData) ' dati da -300 a +60 intorno alla partenza
  Public Property BoatDataAtZero As clsPrestartBoatData
  Public Property BoatDataAtPlusTwo As clsPrestartBoatData
  Public Property First30secAverages As clsPrestartBoatData
  Public Property FirstMinuteAverages As clsPrestartBoatData
  Public Property BoatDataAtLineCrossing As clsPrestartBoatData
  Public Property CurrentBoatData As clsPrestartBoatData

  Public Property LinePinX As Integer
  Public Property LineRcX As Integer
  Public Property LineY As Integer
  Public Property LineYminus1bl As Integer
  Public Property LineYminus2bl As Integer
  Public Property LineYminus3bl As Integer
  Public Property LineYminus5bl As Integer
  Public Property LineYminus10bl As Integer
  'Public Property BoatLenghtInMeters As Double

  Dim LogLatLonIsTheBow As Boolean = False

  Public Property SeriesSource As New ObservableCollection(Of IChartSeriesViewModel)

  Public Property FinalLaunch As New clsExpeditionStartFinalLaunch

  Public Property AP As New clsAccelerationPath

  Public Sub SetCurrentPoint(SecToStart As Integer)
    CurrentBoatData = PrestartBoatData(SecToStart + SecondiPrimaDellaPartenza)
    DrawPolarProjection()
  End Sub

  Public Sub SetAtGun()
    CurrentBoatData = BoatDataAtZero
    DrawPolarProjection()
  End Sub

  Public Sub SetAtLineCrossing()
    CurrentBoatData = BoatDataAtLineCrossing
    DrawPolarProjection()
  End Sub

  Public Sub GoNext()
    Dim i = Math.Min(60, CurrentBoatData.SecondsToStart + 1)
    SetCurrentPoint(i)
    'DrawPolarProjection()
  End Sub

  Public Sub GoPrevious()
    Dim i = Math.Max(-SecondiPrimaDellaPartenza, CurrentBoatData.SecondsToStart - 1)
    SetCurrentPoint(i)
    'DrawPolarProjection()
  End Sub

  Public ReadOnly Property StartDescription As String
    Get
      Return GpsStartTime.ToString("yyyy MM dd HH:mm:ss")
    End Get
  End Property

  Public ReadOnly Property TTKstbdAtTgt As String
    Get
      If CurrentBoatData Is Nothing Then Return ""
      Return -(CurrentBoatData.TtlAtTgtStbd + CurrentBoatData.SecondsToStart).ToString("F0")
    End Get
  End Property

  Public ReadOnly Property TTKportAtTgt As String
    Get
      If CurrentBoatData Is Nothing Then Return ""
      Return -(CurrentBoatData.TtlAtTgtPort + CurrentBoatData.SecondsToStart).ToString("F0")
    End Get
  End Property

  Public ReadOnly Property TTKstbdAcc As String
    Get
      If CurrentBoatData Is Nothing Then Return ""
      Return -(AP.TimeToLine + CurrentBoatData.SecondsToStart).ToString("F0")
    End Get
  End Property

  Public ReadOnly Property StartDetails As String
    Get
      If BoatDataAtLineCrossing Is Nothing Then Return ""
      If BoatDataAtLineCrossing.SecondsToStart < 0 Then
        Return BoatDataAtZero.SecondsToGun & ", OVER by " & Math.Abs(BoatDataAtLineCrossing.SecondsToStart).ToString("F0") & "ss"
      Else
        Return BoatDataAtZero.SecondsToGun & ", line crossed @ " & CrossingSeconds.ToString("F1") & "ss"
      End If
    End Get
  End Property

  Public ReadOnly Property LineGeometry As String
    Get
      Return "LineTwd: " & TrueToMagnetic(PortEnd, StartTime, LineTwd).ToString("000") & "°, length: " & LineLenght.ToString("F0") & "m"
    End Get
  End Property

  Public ReadOnly Property BasicDataAtGun As String
    Get
      Return "@Gun: VmgTgt " & BoatDataAtZero.VmgP.ToString("F0") & "%, BsTgt " & BoatDataAtZero.BstP.ToString("F0") & "%,  Twa D " & BoatDataAtZero.TwaD.ToString("F0") & "°"
    End Get
  End Property

  Public ReadOnly Property BasicDataAtPlusTwo As String
    Get
      Return "Gun+2: VmgTgt " & BoatDataAtPlusTwo.VmgP.ToString("F0") & "%, BsTgt " & BoatDataAtPlusTwo.BstP.ToString("F0") & "%,  Twa D " & BoatDataAtPlusTwo.TwaD.ToString("F0") & "°"
    End Get
  End Property

  Public ReadOnly Property StartBias As String
    Get
      If LineBiasDeg > 0 Then
        Return "Rc fav. by " & LineBiasDeg.ToString("F0") & "° (" & LineBiasMt.ToString("F0") & "mt " & LineBiasSec.ToString("F0") & "ss)" &
          " [x: " & (LineBiasMt * ZoneAtStart).ToString("F0") & "mt " & (LineBiasSec * ZoneAtStart).ToString("F0") & "ss]"
      Else
        Return "Pin fav. by " & LineBiasDeg.ToString("F0") & "° (" & LineBiasMt.ToString("F0") & "mt " & LineBiasSec.ToString("F0") & "ss)" &
          " [x: " & (LineBiasMt * (1 - ZoneAtStart)).ToString("F0") & "mt " & (LineBiasSec * (1 - ZoneAtStart)).ToString("F0") & "ss]"
      End If
    End Get
  End Property

  Public ReadOnly Property StartBiasSimple As String
    Get
      If LineBiasDeg > 0 Then
        Return "RcFavBy " & LineBiasDeg.ToString("F0") & "° (" & LineBiasMt.ToString("F0") & "m " & LineBiasSec.ToString("F0") & "ss)"
      Else
        Return "PinFavBy " & -LineBiasDeg.ToString("F0") & "° (" & LineBiasMt.ToString("F0") & "m " & LineBiasSec.ToString("F0") & "ss)"
      End If
    End Get
  End Property

  Public ReadOnly Property ZoneBiasSimple As String
    Get
      If LineBiasDeg > 0 Then
        Return "Zone" & (ZoneAtStart * 10).ToString("F0") & ", " & (LineBiasMt * ZoneAtStart).ToString("F0") & "m, " & (LineBiasSec * ZoneAtStart).ToString("F0") & "ss"
      Else
        Return "Zone" & (ZoneAtStart * 10).ToString("F0") & ", " & (LineBiasMt * (1 - ZoneAtStart)).ToString("F0") & "m, " & (LineBiasSec * (1 - ZoneAtStart)).ToString("F0") & "ss"
      End If
    End Get
  End Property

  Public ReadOnly Property AccelerationLine1 As String
    Get
      Return "Last 40ss Bottom Speed: " & FinalLaunch.BottomSpeed.ToString("F1") & ", @" & FinalLaunch.BottomSpeedSecToGun.ToString("F0") & "ss"
    End Get
  End Property

  Public ReadOnly Property AccelerationLine2 As String
    Get
      Return "Final Launch starting Boat Speed " & FinalLaunch.AccStartBs.ToString("F1") & "kts, @ " & FinalLaunch.AccStartSecToGun.ToString("F0") & "ss"
    End Get
  End Property

  Public ReadOnly Property AccelerationLine3 As String
    Get
      Return "Avg Tws: " & FinalLaunch.AccAvgTws.ToString("F1") & ", Acc " & FinalLaunch.AccAvgKtsPerMinute.ToString("F1") & " k/m" & ", 90% of Tgt@" & FinalLaunch.BsTgt90SecToGun.ToString("F0") & "ss"
    End Get
  End Property

  Public ReadOnly Property AccelerationLine4 As String
    Get
      Return "Avg Twa " & FinalLaunch.AccAvgTwa.ToString("F0") & "°, Lower Twa " & FinalLaunch.AccMaxTwa.ToString("F0") & "°"
    End Get
  End Property
  Public ReadOnly Property AccelerationLine5 As String
    Get
      Return "30ssVmg:" & First30secAverages.VmgP.ToString("F0") & "%,Bs" & First30secAverages.BstP.ToString("F0") & "%. 1minVmg:" & FirstMinuteAverages.VmgP.ToString("F0") & "%,Bs" & FirstMinuteAverages.BstP.ToString("F0") & "%"
    End Get
  End Property


  Public ReadOnly Property PinX1 As String
    Get
      Return LinePinX - 8
    End Get
  End Property

  Public ReadOnly Property PinX2 As String
    Get
      Return LinePinX + 8
    End Get
  End Property

  Public ReadOnly Property RcX1 As String
    Get
      Return LineRcX - 8
    End Get
  End Property

  Public ReadOnly Property RcX2 As String
    Get
      Return LineRcX + 8
    End Get
  End Property

  Public ReadOnly Property LineY1 As String
    Get
      Return LineY - 8
    End Get
  End Property

  Public ReadOnly Property LineY2 As String
    Get
      Return LineY + 8
    End Get
  End Property

  Private Function PosizioneBarca(SecToStart As Integer) As clsGeographicPosition
    Dim Id As Integer = SecToStart + SecondiPrimaDellaPartenza
    If Id < 0 Then Id = 0
    If Id > PrestartBoatData.Count - 1 Then Id = PrestartBoatData.Count - 1
    Return PrestartBoatData(Id).BoatPosition
  End Function

  Private Function CogTraDuePunti(SecToStartFinale As Integer) As Double

    Dim p1 As clsGeographicPosition = PosizioneBarca(SecToStartFinale - 1)
    Dim p2 As clsGeographicPosition = PosizioneBarca(SecToStartFinale)
    For i As Integer = 1 To 10
      If Not p1.PositionString = p2.PositionString Then
        Exit For
      End If
      p1 = PosizioneBarca(SecToStartFinale - 1 - i)
    Next
    Return clsGeoCalculations.BearingDegrees(p1, p2)
  End Function

  Public Function PrevHdgVal(ActualSecondsToStart As Integer) As Double
    Dim Id As Integer = ActualSecondsToStart + SecondiPrimaDellaPartenza
    If Id < 0 Then Id = 0
    If Id > PrestartBoatData.Count - 1 Then Id = PrestartBoatData.Count - 1
    Return PrestartBoatData(Id).Hdg
  End Function

  Public Function PrevSowVal(ActualSecondsToStart As Integer) As Double
    Dim Id As Integer = ActualSecondsToStart + SecondiPrimaDellaPartenza
    If Id < 0 Then Id = 0
    If Id > PrestartBoatData.Count - 1 Then Id = PrestartBoatData.Count - 1
    Return PrestartBoatData(Id).Bs
  End Function


  Public Sub DrawAccelerationPath()
    Dim DataSeries As XyDataSeries(Of Double, Double)
    Dim s = SeriesSource.Where(Function(x) x.DataSeries.SeriesName = "AccelerationPath")

    If s.Count = 0 Then
      DataSeries = New XyDataSeries(Of Double, Double)
      Dim RenderableSeries As New FastLineRenderableSeries
      RenderableSeries.XAxisId = "DefaultAxisId"
      RenderableSeries.YAxisId = "DefaultAxisId"
      RenderableSeries.Stroke = Colors.Green
      RenderableSeries.StrokeThickness = 1
      DataSeries.AcceptsUnsortedData = True
      DataSeries.SeriesName = "AccelerationPath"
      RenderableSeries.DataSeries = DataSeries
      Dim CSVMstbd As New ChartSeriesViewModel(DataSeries, RenderableSeries)
      SeriesSource.Add(CSVMstbd)
    Else
      For Each ss In SeriesSource
        If ss.DataSeries.SeriesName = "AccelerationPath" Then
          DataSeries = ss.DataSeries
          DataSeries.Clear()
          Exit For
        End If
      Next
    End If
    If CurrentBoatData.SecondsToStart > 0 Then Exit Sub
    RcToPin = SommaAngolo180adAngolo360(-90, LineTwd)

    'If AP Is Nothing Then AP = New clsAccelerationPath
    Dim PosTmp As clsGeographicPosition = CurrentBoatData.BoatPosition
    Dim PosXY = CalculatesRelativePositionXY(PosTmp, PortEnd, LineTwd, Colors.Black)
    DataSeries.Append(PosXY.X, PosXY.Y, New clsPuntoMetadata(False))


    Dim StepSec As Double = 1
    Dim Sow As Double = CurrentBoatData.Bs
    Dim PrevSow As Double = PrevSowVal(CurrentBoatData.SecondsToStart)
    Dim Twa As Double = CurrentBoatData.Twa
    Dim Cog As Double = CurrentBoatData.Cog
    Dim Hdg As Double = CurrentBoatData.Hdg
    Dim PrevHdg As Double = PrevHdgVal(CurrentBoatData.SecondsToStart)
    Dim Cow As Double = CogTraDuePunti(CurrentBoatData.SecondsToStart) ' clsGeoCalculations.BearingDegrees(CurrentBoatData.BoatPosition, PrestartBoatData(CurrentBoatData.SecondsToStart + 181).BoatPosition)   'CurrentBoatData.Cog ' SommaAngolo180adAngolo360(-Twa, CurrentBoatData.Twd)
    Dim PrevCow As Double = CogTraDuePunti(CurrentBoatData.SecondsToStart - 1)
    Dim PrevRot As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Hdg, PrevHdg)
    Dim Rot As Double = CurrentBoatData.YRT
    Dim Acc As Double = (Sow - PrevSow) / StepSec
    'Console.Write("Sow: " & Sow)
    'Console.Write(" ,Twa: " & Twa)
    'Console.Write(" ,Cog: " & Cog)
    'Console.Write(" ,Hdg: " & Hdg)
    'Console.Write(" ,Rot: " & Rot)
    'Console.Write(" ,PrevHdg: " & PrevHdg)
    'Console.Write(" ,Cow: " & Cow)
    'Console.Write(" ,PrevCow: " & PrevCow)
    'Console.WriteLine(" ,PrevRot: " & PrevRot)
    AP.TimeToLine = -1
    Dim LoopTo As Integer = -CurrentBoatData.SecondsToStart + 100
    For i As Integer = 0 To LoopTo
      Dim UPT = TgtManager.Tgt.ValoreTgtUp(CurrentBoatData.Tws, "bs")
      Dim VPP = TgtManager.Tgt.Valore(CurrentBoatData.Tws, Twa, "bs")
      Dim NextStep = AP.Calcola(CurrentBoatData.SecondsToStart + i, PosTmp, Sow, CurrentBoatData.Tws, Twa, CurrentBoatData.Twd, Cow, StepSec, VPP, UPT, PrevRot, Acc)
      PosTmp = NextStep.Posizione
      Sow = NextStep.Sow
      Cow = NextStep.Cow
      PrevRot = NextStep.ROT
      Acc = NextStep.Acc
      Twa = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Cow, CurrentBoatData.Twd)
      If AP.TimeToLine = -1 Then
        If AP.LineOrOver(NextStep.Posizione, PortEnd, RcToPin) Then
          AP.TimeToLine = i
        End If
      End If
      PosXY = CalculatesRelativePositionXY(NextStep.Posizione, PortEnd, LineTwd, Colors.Black)
      DataSeries.Append(PosXY.X, PosXY.Y, New clsPuntoMetadata(False))
      If i >= -CurrentBoatData.SecondsToStart AndAlso AP.TimeToLine > -1 Then Exit For
    Next
    CurrentBoatData.StbdTtlWithAcc = AP.TimeToLine
    CurrentBoatData.CalculateAccelerationLoss = (AP.TimeToLine - CurrentBoatData.TtlAtTgtStbd)


    'Console.WriteLine("Time To Line: " & AP.TimeToLine)
  End Sub


  Public Sub DrawPolarProjection()
    Dim DataSeries As XyDataSeries(Of Double, Double)
    Dim s = SeriesSource.Where(Function(x) x.DataSeries.SeriesName = "PolarProjection")

    If s.Count = 0 Then
      DataSeries = New XyDataSeries(Of Double, Double)
      Dim RenderableSeries As New FastLineRenderableSeries
      RenderableSeries.XAxisId = "DefaultAxisId"
      RenderableSeries.YAxisId = "DefaultAxisId"
      RenderableSeries.Stroke = Colors.Black
      RenderableSeries.StrokeThickness = 1
      DataSeries.AcceptsUnsortedData = True
      DataSeries.SeriesName = "PolarProjection"
      RenderableSeries.DataSeries = DataSeries
      Dim CSVMstbd As New ChartSeriesViewModel(DataSeries, RenderableSeries)
      SeriesSource.Add(CSVMstbd)
    Else
      For Each ss In SeriesSource
        If ss.DataSeries.SeriesName = "PolarProjection" Then
          DataSeries = ss.DataSeries
          DataSeries.Clear()
          Exit For
        End If
      Next
    End If

    If CurrentBoatData.SecondsToStart > 0 Then Exit Sub
    'Dim RcToPin = SommaAngolo180adAngolo360(-90, LineTwd)

    Dim LineTwdMag As Double = TrueToMagnetic(PortEnd, StartTime, LineTwd)

    Dim tmp As New List(Of clsXYpoint)
    For i As Integer = 20 To 100
      Dim LineTwa As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(CurrentBoatData.Twd, LineTwdMag) + i
      If LineTwa < 90 Then
        Dim UPT = TgtManager.Tgt.ValoreTgtUp(CurrentBoatData.Tws, "bs")
        Dim VPP = TgtManager.Tgt.Valore(CurrentBoatData.Tws, i, "bs")
        Dim PolarSpeed As Double = Math.Max(VPP.Valore, UPT.Bs)
        PolarSpeed = Math.Max(PolarSpeed, CurrentBoatData.Bs)
        Dim p As clsGeographicPosition = clsGeoCalculations.PuntoDestinazione(CurrentBoatData.BoatPosition, KtsToMS(PolarSpeed) * Math.Abs(CurrentBoatData.SecondsToStart), SommaAngolo180adAngolo360(-i, CurrentBoatData.Twd))
        Dim PolarToPin As Double = clsGeoCalculations.BearingDegrees(p, PortEnd)
        If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(RcToPin, PolarToPin) Then
          ' qui serve ad intercettare quando la curva interseca la linea di partenza
        End If
        tmp.Add(CalculatesRelativePositionXY(p, PortEnd, LineTwd, Colors.Black))
        DataSeries.Append(tmp.Last.X, tmp.Last.Y, New clsPuntoMetadata(False))
      End If
    Next

    DrawAccelerationPath()

  End Sub

  Private Function GetValue(Channel As clsChannel2020, Id As Integer) As Double
    If Channel Is Nothing Then Return Double.NaN
    Return Channel.PrimoValoreNotNan(Id)
  End Function

  Public Sub New(startTime As DateTime, stbdEnd As clsGeographicPosition, portEnd As clsGeographicPosition)
    IsValid = CreateStart(startTime, stbdEnd, portEnd)
  End Sub

  Sub SetCustomData(Channel As clsChannel2020, ByRef Header As String, ByRef Value As Double, ByRef Vis As Visibility, idm As Integer)
    If Channel Is Nothing Then
      Header = ""
      Value = Double.NaN
      Vis = Visibility.Collapsed
    Else
      Header = Channel.ShortName
      If Channel.DataType = clsChannel2020.eDataType.eDays Then
        Value = GetValue(Channel, idm) * 24 * 3600
      Else
        Value = GetValue(Channel, idm)
      End If
      Vis = Visibility.Visible
    End If

  End Sub

  Private Function CreateStart(startTime As DateTime, stbdEnd As clsGeographicPosition, portEnd As clsGeographicPosition) As Double

    Me.StartTime = startTime
    LogLatLonIsTheBow = PosIsTheBow(startTime)
    Me.StbdEnd = stbdEnd
    Me.PortEnd = portEnd
    'BoatLenghtInMeters = AppConfig.CercaValoreInnerText(DataProvider2020.SuffissoFileType, clsSettings.eNodoSTD.eStartUp, "BoatSettings", "BoatLenghtInMeters", 16, True, True)
    Dim BoatLenghtInMeters As Double = AppConfig.ActiveProfile.BoatLenghtInMeters
    RcToPin = clsGeoCalculations.BearingDegrees(stbdEnd, portEnd)
    LineLenght = clsGeoCalculations.DistanceMeters(stbdEnd, portEnd)
    Me.VirtualPortEnd = clsGeoCalculations.PuntoDestinazione(stbdEnd, LineLenght + 1000, RcToPin)
    Me.VirtualStbdEnd = clsGeoCalculations.PuntoDestinazione(stbdEnd, 1000, SommaAngolo180adAngolo360(180, RcToPin))


    Dim chTws As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chBs As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    Dim chTwa As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    Dim chVmgP As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
    Dim chBstP As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSTp)
    Dim chTwaD As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWAd)
    Dim chHeel As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHeelNorm)
    Dim chRdr As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eRdrNorm)
    Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim chLon As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    Dim chCog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    Dim chSog As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOG)
    Dim chHdg As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)

    Dim chYrt As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eYRT)
    Dim chExpVdist As clsChannel2020 = DataProvider2020.CanaleDbl("BelowLn")
    Dim chExpTTK As clsChannel2020 = DataProvider2020.CanaleDbl("TmToBurn")
    Dim chFinalShut As clsChannel2020 = DataProvider2020.CanaleDbl("FinalShut")
    Dim chFinalShutMt As clsChannel2020 = DataProvider2020.CanaleDbl("BackStay")
    Dim chExpTTL As clsChannel2020 = DataProvider2020.CanaleDbl("TmToLn")
    If chExpTTK Is Nothing Then
      chExpTTK = DataProvider2020.CanaleDbl("Burn")
    End If
    If AppConfig.ActiveProfile.StartReportSettings Is Nothing Then
      AppConfig.ActiveProfile.StartReportSettings = New clsStartReportSettings
      AppConfig.Salva()
    End If


    Dim chCD1 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel1)
    Dim chCD2 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel2)
    Dim chCD3 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel3)
    Dim chCD4 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel4)
    Dim chCD5 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel5)
    Dim chCD6 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel6)
    Dim chCD7 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel7)
    Dim chCD8 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel8)
    Dim chCD9 As clsChannel2020 = DataProvider2020.CanaleDbl(AppConfig.ActiveProfile.StartReportSettings.CustomChannel9)




    Dim idm As Integer = DataProvider2020.TrovaIndice(startTime)
    Dim chGpsTime As clsChannel2020 = DataProvider2020.CanaleDbl("GPS time")
    If Not chGpsTime Is Nothing Then
      If chGpsTime.HasNotNanValues AndAlso DateTime.FromOADate(chGpsTime.PrimoValoreNotNan(idm)).Year = DateTime.FromOADate(0.5).Year Then
        ' faro
        'GpsStartTime = ConvertToLocalTime(chLat.PrimoValoreNotNan(idm), chLon.PrimoValoreNotNan(idm), New Date(startTime.Year, startTime.Month, startTime.Day).AddDays(chGpsTime.PrimoValoreNotNan(idm)))
      Else
        ' bravo (secondi)
        GpsStartTime = startTime
      End If
    End If
    GpsStartTime = startTime


    LineTwd = SommaAngolo180adAngolo360(90, RcToPin)
    Dim mLineTwd As Double = TrueToMagnetic(portEnd, startTime, LineTwd)
    'LineLenght = clsGeoCalculations.DistanceMeters(stbdEnd, portEnd)
    twds = New clsChannelStats(chTwd, New clsTimeRange(startTime.AddSeconds(-30), startTime.AddSeconds(30)))
    twss = New clsChannelStats(chTws, New clsTimeRange(startTime.AddSeconds(-30), startTime.AddSeconds(30)))
    If Double.IsNaN(twds.Avg) OrElse Double.IsNaN(twss.Avg) Then
      Return False
    End If

    LineBiasDeg = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(mLineTwd, twds.Avg)

    RcPositionXYtoPin = New clsXYpoint(LineLenght, 0, Colors.Black)

    LinePinX = 0
    LineRcX = LineLenght
    LineY = 0
    LineYminus1bl = -BoatLenghtInMeters
    LineYminus2bl = -2 * BoatLenghtInMeters
    LineYminus3bl = -3 * BoatLenghtInMeters
    LineYminus5bl = -5 * BoatLenghtInMeters
    LineYminus10bl = -10 * BoatLenghtInMeters



    Dim PinToLineTwdDist As Double = LineLenght * Math.Sin(Radians(LineBiasDeg))
    Dim deltaTwd As Double = LineBiasDeg
    If TgtManager.Tgt Is Nothing Then
      MsgBox("Polar File needed!", MsgBoxStyle.OkOnly, "Notice")
      Return False
    End If
    Dim UpPol As clsValoriPuntoPolare = TgtManager.Tgt.ValoreTgtUp(twss.Avg, "bs")
    Dim BsTgt As Double = UpPol.Bs
    Dim TwaTgt As Double = UpPol.Twa
    LineBiasMt = PinToLineTwdDist / Math.Cos(Radians(TwaTgt))
    LineBiasSec = LineBiasMt / KtsToMS(BsTgt)
    Dim Cbase As Boolean = True
    Dim CA As Color = Colors.WhiteSmoke
    Dim CB As Color = Colors.LightYellow
    Dim C0 As Color = Colors.Yellow
    Dim BsPrev As Double = 0
    Dim StsPrev As Double = -SecondiPrimaDellaPartenza - 1
    Dim momento As DateTime

    Dim VmgP30sec As New List(Of Double)
    Dim BsP30sec As New List(Of Double)
    Dim TwaD30sec As New List(Of Double)

    Dim VmgP1min As New List(Of Double)
    Dim BsP1min As New List(Of Double)
    Dim TwaD1min As New List(Of Double)


    For s As Integer = -SecondiPrimaDellaPartenza To 60 ' SecondsArray.First To SecondsArray.Last
      'If s = -20 Then Stop
      momento = startTime.AddSeconds(s)
      idm = DataProvider2020.TrovaIndice(momento)
      Dim BoatDataTmp As New clsPrestartBoatData(momento, idm, s, BoatLenghtInMeters)
      BoatDataTmp.Hdg = GetValue(chHdg, idm)
      Dim BP = BowPosition(New clsGeographicPosition(GetValue(chLat, idm), GetValue(chLon, idm)), BoatDataTmp.Hdg)
      BoatDataTmp.BoatPosition = BP
      BoatDataTmp.Bs = GetValue(chBs, idm)
      BoatDataTmp.BsDerivative = (BoatDataTmp.Bs - BsPrev) / (BoatDataTmp.SecondsToStart - StsPrev)
      BoatDataTmp.AccKtsMin = BoatDataTmp.BsDerivative * 60
      BsPrev = BoatDataTmp.Bs
      StsPrev = BoatDataTmp.SecondsToStart
      BoatDataTmp.BstP = GetValue(chBstP, idm)
      BoatDataTmp.Heel = GetValue(chHeel, idm)
      BoatDataTmp.Rdr = GetValue(chRdr, idm)
      BoatDataTmp.Twa = GetValue(chTwa, idm)
      BoatDataTmp.TwaD = GetValue(chTwaD, idm)
      BoatDataTmp.Twd = GetValue(chTwd, idm)
      BoatDataTmp.Tws = GetValue(chTws, idm)
      BoatDataTmp.VmgP = GetValue(chVmgP, idm)
      BoatDataTmp.YRT = GetValue(chYrt, idm)
      BoatDataTmp.Cog = GetValue(chCog, idm)
      BoatDataTmp.Sog = GetValue(chSog, idm)
      If s = SecondsArray.First Then BoatDataTmp.HeaderVisible = Visibility.Visible
      Dim BoatToPinDeg As Double = clsGeoCalculations.BearingDegrees(BP, VirtualPortEnd)
      Dim BoatToPinMt As Double = clsGeoCalculations.DistanceMeters(BP, VirtualPortEnd)
      Dim A As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(RcToPin, BoatToPinDeg)
      'Dim BoatToPinDeg As Double = clsGeoCalculations.BearingDegrees(BP, portEnd)
      'Dim BoatToPinMt As Double = clsGeoCalculations.DistanceMeters(BP, portEnd)
      'Dim A As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(RcToPin, BoatToPinDeg)
      BoatDataTmp.Vdist = Math.Sin(Radians(A)) * BoatToPinMt
      BoatDataTmp.VdistBL = BoatDataTmp.Vdist / BoatLenghtInMeters
      BoatDataTmp.ExpVdist = GetValue(chExpVdist, idm) * 1852
      BoatDataTmp.ExpTimeToLine = GetValue(chExpTTL, idm) * 24 * 3600
      BoatDataTmp.ExpTimeToBurn = GetValue(chExpTTK, idm) * 24 * 3600
      BoatDataTmp.FinalShut = GetValue(chFinalShut, idm) * 24 * 3600
      BoatDataTmp.FinalShutMt = GetValue(chFinalShutMt, idm)


      SetCustomData(chCD1, BoatDataTmp.CustomData1h, BoatDataTmp.CustomData1, BoatDataTmp.CustomData1v, idm)
      SetCustomData(chCD2, BoatDataTmp.CustomData2h, BoatDataTmp.CustomData2, BoatDataTmp.CustomData2v, idm)
      SetCustomData(chCD3, BoatDataTmp.CustomData3h, BoatDataTmp.CustomData3, BoatDataTmp.CustomData3v, idm)
      SetCustomData(chCD4, BoatDataTmp.CustomData4h, BoatDataTmp.CustomData4, BoatDataTmp.CustomData4v, idm)
      SetCustomData(chCD5, BoatDataTmp.CustomData5h, BoatDataTmp.CustomData5, BoatDataTmp.CustomData5v, idm)
      SetCustomData(chCD6, BoatDataTmp.CustomData6h, BoatDataTmp.CustomData6, BoatDataTmp.CustomData6v, idm)
      SetCustomData(chCD7, BoatDataTmp.CustomData7h, BoatDataTmp.CustomData7, BoatDataTmp.CustomData7v, idm)
      SetCustomData(chCD8, BoatDataTmp.CustomData8h, BoatDataTmp.CustomData8, BoatDataTmp.CustomData8v, idm)
      SetCustomData(chCD9, BoatDataTmp.CustomData9h, BoatDataTmp.CustomData9, BoatDataTmp.CustomData9v, idm)

      'If chCD1 Is Nothing Then
      '  BoatDataTmp.CustomData1h = ""
      '  BoatDataTmp.CustomData1 = Double.NaN
      'Else
      '  BoatDataTmp.CustomData1h = chCD1.ShortName
      '  If chCD1.DataType = clsChannel2020.eDataType.eDays Then
      '    BoatDataTmp.CustomData1 = GetValue(chCD1, idm) * 24 * 3600
      '  Else
      '    BoatDataTmp.CustomData1 = GetValue(chCD1, idm)
      '  End If
      'End If



      deltaTwd = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(LineTwd, BoatDataTmp.Twd)
      UpPol = TgtManager.Tgt.ValoreTgtUp(BoatDataTmp.Tws, "bs")
      BsTgt = UpPol.Bs
      TwaTgt = UpPol.Twa
      Dim DistToLine As Double = BoatDataTmp.Vdist / Math.Cos(Radians(TwaTgt - deltaTwd))
      BoatDataTmp.TtlAtTgtStbd = DistToLine / KtsToMS(BsTgt)
      DistToLine = BoatDataTmp.Vdist / Math.Cos(Radians(TwaTgt + deltaTwd))
      BoatDataTmp.TtlAtTgtPort = DistToLine / KtsToMS(BsTgt)
      BoatDataTmp.UpdateRelativePosition(portEnd, LineTwd)
      BoatDataTmp.UpdateLayLines(portEnd, stbdEnd, LineTwd, UpPol.Twa, UpPol.Bs, BoatLenghtInMeters)

      PrestartBoatData.Add(BoatDataTmp)

      If SecondsArray.Contains(BoatDataTmp.SecondsToStart) Then
        If Cbase Then
          BoatDataTmp.RelativePositionXY.RowColor = CA
        Else
          BoatDataTmp.RelativePositionXY.RowColor = CB
        End If
        BoatDataTable.Add(BoatDataTmp)
        Cbase = Not Cbase
      End If
      If s = 0 Then
        BoatDataAtZero = BoatDataTmp
        BoatDataTmp.RelativePositionXY.RowColor = C0
      ElseIf s = 2 Then
        BoatDataAtPlusTwo = BoatDataTmp
      End If
      If s < 20 AndAlso s > -20 AndAlso BoatDataAtLineCrossing Is Nothing AndAlso BoatDataTmp.Vdist < 0 Then
        CrossingSeconds = InterpolazioneLineare(0, PrestartBoatData(PrestartBoatData.Count - 2).Vdist, PrestartBoatData(PrestartBoatData.Count - 2).SecondsToStart, BoatDataTmp.Vdist, BoatDataTmp.SecondsToStart)
        BoatDataAtLineCrossing = BoatDataTmp
      End If
      If s > 0 Then
        If s <= 30 Then
          VmgP30sec.Add(BoatDataTmp.VmgP)
          BsP30sec.Add(BoatDataTmp.BstP)
          TwaD30sec.Add(BoatDataTmp.TwaD)
          VmgP1min.Add(BoatDataTmp.VmgP)
          BsP1min.Add(BoatDataTmp.BstP)
          TwaD1min.Add(BoatDataTmp.TwaD)
        End If
        If s <= 60 Then
          VmgP1min.Add(BoatDataTmp.VmgP)
          BsP1min.Add(BoatDataTmp.BstP)
          TwaD1min.Add(BoatDataTmp.TwaD)
        End If
      End If
    Next

    If BoatDataAtLineCrossing Is Nothing Then
      Dim BoatToPinMt As Double = clsGeoCalculations.DistanceMeters(BoatDataAtZero.BoatPosition, portEnd)
      Dim BoatToRcMt As Double = clsGeoCalculations.DistanceMeters(BoatDataAtZero.BoatPosition, stbdEnd)
      ZoneAtStart = BoatToRcMt / (BoatToRcMt + BoatToPinMt) ' 0 Rc , 1 Pin
    Else
      Dim BoatToPinMt As Double = clsGeoCalculations.DistanceMeters(BoatDataAtLineCrossing.BoatPosition, portEnd)
      Dim BoatToRcMt As Double = clsGeoCalculations.DistanceMeters(BoatDataAtLineCrossing.BoatPosition, stbdEnd)
      ZoneAtStart = BoatToRcMt / (BoatToRcMt + BoatToPinMt) ' 0 Rc , 1 Pin
    End If

    FinalLaunch.CalcolaAccelerazione(PrestartBoatData.ToList, -40, 60)

    momento = startTime.AddSeconds(30)
    idm = DataProvider2020.TrovaIndice(momento)
    First30secAverages = New clsPrestartBoatData(momento, idm, 30, BoatLenghtInMeters)
    First30secAverages.BstP = BsP30sec.Average
    First30secAverages.TwaD = TwaD30sec.Average
    First30secAverages.VmgP = VmgP30sec.Average

    momento = startTime.AddSeconds(60)
    idm = DataProvider2020.TrovaIndice(momento)
    FirstMinuteAverages = New clsPrestartBoatData(momento, idm, 60, BoatLenghtInMeters)
    FirstMinuteAverages.BstP = BsP1min.Average
    FirstMinuteAverages.TwaD = TwaD1min.Average
    FirstMinuteAverages.VmgP = VmgP1min.Average


    DrawBoatTrack()

    Return True


  End Function

  Private Function PosIsTheBow(StartTime As DateTime) As Boolean
    For Each f In DataProvider2020.ParquetFiles
      If f.TimeRangeRealDateTime.IsInRange(StartTime, True, True) Then
        Return f.LatLonPositionIsTheBow
      End If
    Next
    Return False
  End Function


  Private Function BowPosition(LogFilePosition As clsGeographicPosition, Hdg As Double) As clsGeographicPosition
    If LogLatLonIsTheBow Then
      Return LogFilePosition
    Else
      Return clsGeoCalculations.PuntoDestinazione(LogFilePosition, AppConfig.ActiveProfile.BoatLenghtInMeters, Hdg)
    End If
  End Function

  Private Sub DrawBoatTrack()
    'SeriesSource = New ObservableCollection(Of IChartSeriesViewModel)

    SeriesSource.Clear()


    Dim DataSeriesStbd As New XyDataSeries(Of Double, Double)
    Dim RenderableSerieStbd As New XyScatterRenderableSeries
    RenderableSerieStbd.XAxisId = "DefaultAxisId"
    RenderableSerieStbd.YAxisId = "DefaultAxisId"
    RenderableSerieStbd.PointMarker = New EllipsePointMarker
    RenderableSerieStbd.PointMarker.Fill = Colors.Transparent
    RenderableSerieStbd.PointMarker.Stroke = Colors.DarkGreen
    RenderableSerieStbd.PointMarker.Height = 9
    RenderableSerieStbd.PointMarker.Width = 9
    RenderableSerieStbd.PointMarker.StrokeThickness = 2
    DataSeriesStbd.AcceptsUnsortedData = True
    DataSeriesStbd.SeriesName = "Stbd"
    RenderableSerieStbd.DataSeries = DataSeriesStbd
    Dim CSVMstbd As New ChartSeriesViewModel(DataSeriesStbd, RenderableSerieStbd)
    SeriesSource.Add(CSVMstbd)

    Dim DataSeriesPort As New XyDataSeries(Of Double, Double)
    Dim RenderableSeriePort As New XyScatterRenderableSeries
    RenderableSeriePort.XAxisId = "DefaultAxisId"
    RenderableSeriePort.YAxisId = "DefaultAxisId"
    RenderableSeriePort.PointMarker = New EllipsePointMarker
    RenderableSeriePort.PointMarker.Fill = Colors.Transparent
    RenderableSeriePort.PointMarker.Stroke = Colors.DarkRed
    RenderableSeriePort.PointMarker.Height = 9
    RenderableSeriePort.PointMarker.Width = 9
    RenderableSeriePort.PointMarker.StrokeThickness = 2
    DataSeriesPort.AcceptsUnsortedData = True
    DataSeriesPort.SeriesName = "Port"

    RenderableSeriePort.DataSeries = DataSeriesPort
    Dim CSVMPort As New ChartSeriesViewModel(DataSeriesPort, RenderableSeriePort)
    SeriesSource.Add(CSVMPort)


    Dim gruppicolore = PrestartBoatData.GroupBy(Function(x) x.RelativePositionXY.Color).ToArray
    For Each gruppo In gruppicolore
      Dim DataSeries As New XyDataSeries(Of Double, Double)
      Dim RenderableSerie As New XyScatterRenderableSeries
      RenderableSerie.XAxisId = "DefaultAxisId"
      RenderableSerie.YAxisId = "DefaultAxisId"
      RenderableSerie.PointMarker = New EllipsePointMarker
      RenderableSerie.PointMarker.Fill = gruppo.First.RelativePositionXY.Color
      RenderableSerie.PointMarker.Stroke = Colors.Transparent
      RenderableSerie.PointMarker.Height = 7
      RenderableSerie.PointMarker.Width = 7
      RenderableSerie.PointMarker.StrokeThickness = 0
      DataSeries.AcceptsUnsortedData = True
      DataSeries.SeriesName = gruppo.First.GroupDescription
      For Each pbd In gruppo
        DataSeries.Append(pbd.RelativePositionXY.X, pbd.RelativePositionXY.Y, New clsPuntoMetadata(False))
        If pbd.Twa >= 0 Then
          DataSeriesStbd.Append(pbd.RelativePositionXY.X, pbd.RelativePositionXY.Y, New clsPuntoMetadata(False))
        Else
          DataSeriesPort.Append(pbd.RelativePositionXY.X, pbd.RelativePositionXY.Y, New clsPuntoMetadata(False))
        End If
      Next
      RenderableSerie.DataSeries = DataSeries
      Dim CSVMtmp As New ChartSeriesViewModel(DataSeries, RenderableSerie)
      SeriesSource.Add(CSVMtmp)
    Next

    Dim DataSeriesGun As New XyDataSeries(Of Double, Double)
    Dim RenderableSerieGun As New XyScatterRenderableSeries
    RenderableSerieGun.XAxisId = "DefaultAxisId"
    RenderableSerieGun.YAxisId = "DefaultAxisId"
    RenderableSerieGun.PointMarker = New EllipsePointMarker
    RenderableSerieGun.PointMarker.Fill = Colors.Transparent
    RenderableSerieGun.PointMarker.Stroke = Colors.Yellow
    RenderableSerieGun.PointMarker.Height = 12
    RenderableSerieGun.PointMarker.Width = 12
    RenderableSerieGun.PointMarker.StrokeThickness = 3
    DataSeriesGun.AcceptsUnsortedData = True
    DataSeriesGun.SeriesName = "Gun"
    DataSeriesGun.Append(BoatDataAtZero.RelativePositionXY.X, BoatDataAtZero.RelativePositionXY.Y, New clsPuntoMetadata(False))
    RenderableSerieGun.DataSeries = DataSeriesGun
    Dim CSVMtmpGun As New ChartSeriesViewModel(DataSeriesGun, RenderableSerieGun)
    SeriesSource.Add(CSVMtmpGun)


  End Sub

  Public Function StartRow(Header As Boolean) As List(Of String)
    Dim Righe As New List(Of String)
    If BoatDataAtLineCrossing Is Nothing Then Return Righe
    Dim riga As String = ""
    If Header Then
      riga = "Start Line" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "@Gun" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "Gun+2" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "Gun +30" & vbTab
      riga &= "" & vbTab
      riga &= "Gun+60" & vbTab
      riga &= "" & vbTab
      riga &= "Launch" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "Gun-10" & vbTab
      riga &= "" & vbTab
      riga &= "" & vbTab
      riga &= "Gun-5" & vbTab
      riga &= "" & vbTab
      riga &= ""
      Righe.Add(riga)

      riga = "Time" & vbTab
      riga &= "LnTwd" & vbTab
      riga &= "LnLenght" & vbTab
      riga &= "LnBias" & vbTab
      riga &= "Vdist" & vbTab
      riga &= "Cross" & vbTab
      riga &= "Pos" & vbTab
      riga &= "BstLss" & vbTab 'loss from best poosition
      riga &= "Bs%" & vbTab
      riga &= "Vmg%" & vbTab
      riga &= "TwaD" & vbTab
      riga &= "Bs%" & vbTab
      riga &= "Vmg%" & vbTab
      riga &= "Bs%" & vbTab
      riga &= "Vmg%" & vbTab
      riga &= "ss2gun" & vbTab
      riga &= "Tws" & vbTab
      riga &= "Twd" & vbTab
      riga &= "BtmBs" & vbTab
      riga &= "LVdist" & vbTab
      riga &= "Acc" & vbTab
      riga &= "TwaAvg" & vbTab
      riga &= "TwaMax" & vbTab
      riga &= "90ofBsT" & vbTab
      riga &= "Bs" & vbTab
      riga &= "dTwa" & vbTab
      riga &= "vDist" & vbTab
      riga &= "Bs" & vbTab
      riga &= "dTwa" & vbTab
      riga &= "vDist"
      Righe.Add(riga)
    End If
    riga = StartTime.ToString("yyyy MM dd HH:mm:ss") & vbTab
    riga &= LineTwd.ToString("F0") & vbTab
    riga &= LineLenght.ToString("F0") & vbTab
    riga &= LineBiasDeg.ToString("F0") & vbTab
    riga &= BoatDataAtZero.VdistBL.ToString("F1") & vbTab
    riga &= BoatDataAtLineCrossing.SecondsToStart.ToString("F1") & vbTab
    riga &= CInt((ZoneAtStart * 10)).ToString("F0") & vbTab 'da 0 (RC) a 10 (pin)
    'If ZoneAtStart < 0.3 Then
    '  riga &= ZoneAtStart & vbTab
    '  'riga = "Rc area"
    'ElseIf ZoneAtStart <= 0.7 Then
    '  riga &= "Pos" & vbTab
    '  'riga = "Mid line"
    'Else
    '  riga &= "Pos" & vbTab
    '  'riga = "Pin area"
    'End If
    If LineBiasDeg > 0 Then
      riga &= (LineBiasSec * ZoneAtStart).ToString("F0") & vbTab ' perdita rispetto alla potenziale miglior posizione sulla linea
      'riga &= ((LineBiasMt * ZoneAtStart) / MetriPerBoatLenght).ToString("F0") & vbTab ' perdita rispetto alla potenziale miglior posizione sulla linea
    Else
      riga &= (-LineBiasSec * ZoneAtStart).ToString("F0") & vbTab ' perdita rispetto alla potenziale miglior posizione sulla linea
      'riga &= -((LineBiasMt * ZoneAtStart) / MetriPerBoatLenght).ToString("F0") & vbTab ' perdita rispetto alla potenziale miglior posizione sulla linea
    End If
    riga &= BoatDataAtPlusTwo.BstP.ToString("F0") & vbTab
    riga &= BoatDataAtPlusTwo.VmgP.ToString("F0") & vbTab
    riga &= BoatDataAtPlusTwo.TwaD.ToString("F0") & vbTab
    riga &= First30secAverages.BstP.ToString("F0") & vbTab
    riga &= First30secAverages.VmgP.ToString("F0") & vbTab
    riga &= FirstMinuteAverages.BstP.ToString("F0") & vbTab
    riga &= FirstMinuteAverages.VmgP.ToString("F0") & vbTab
    riga &= -FinalLaunch.AccStartSecToGun.ToString("F0") & vbTab 'momento del lancio
    riga &= FinalLaunch.AccAvgTws.ToString("F1") & vbTab
    riga &= FinalLaunch.AccAvgTwd.ToString("F0") & vbTab
    riga &= FinalLaunch.BottomSpeed.ToString("F1") & vbTab
    riga &= FinalLaunch.VdistAtAccStart.ToString("F1") & vbTab
    riga &= FinalLaunch.AccAvgKtsPerMinute.ToString("F1") & vbTab
    riga &= FinalLaunch.AccAvgTwa.ToString("F0") & vbTab
    riga &= FinalLaunch.AccMaxTwa.ToString("F0") & vbTab
    riga &= -FinalLaunch.BsTgt90SecToGun.ToString("F0") & vbTab

    riga &= FinalLaunch.BsAtMinus10.ToString("F1") & vbTab
    riga &= FinalLaunch.dTwaAtMinus10.ToString("F0") & vbTab
    riga &= FinalLaunch.VdistAtMinus10.ToString("F1") & vbTab
    riga &= FinalLaunch.BsAtMinus5.ToString("F1") & vbTab
    riga &= FinalLaunch.dTwaAtMinus5.ToString("F0") & vbTab
    riga &= FinalLaunch.VdistAtMinus5.ToString("F1")
    Righe.Add(riga)

    Return Righe
  End Function




End Class

Public Class clsTwinDoubleArrays
  Public Property X As New List(Of Double)
  Public Property Y As New List(Of Double)

  Public Sub AddValues(X As Double, Y As Double)
    Me.X.Add(X)
    Me.Y.Add(Y)
  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsAccelerationPath
  Public Property RateOfTurn As New clsTwinDoubleArrays
  Public Property PowerCurve As New clsTwinDoubleArrays
  Public Property AccelerationKtsSec As New clsTwinDoubleArrays
  Public Property AccCurve As New clsTwinDoubleArrays
  Public Property LuffingBrake As New clsTwinDoubleArrays
  Public Property BestAccTwa As New clsTwinDoubleArrays
  Public Property DeltaSpeedKts As Double = 5
  Public Property TwsDeltaSpeed As Double = 10
  Public Property SecondsToAccelerateDeltaSpeed As Double = 20
  Public Property Coeffbase As Double
  Public Property TimeToLine As Double
  Dim RotInterpol As MathNet.Numerics.Interpolation.IInterpolation
  Dim AccKtsSecInterpol As MathNet.Numerics.Interpolation.IInterpolation
  Dim AccCurveInterpol As MathNet.Numerics.Interpolation.IInterpolation
  Dim BrakeInterpol As MathNet.Numerics.Interpolation.IInterpolation
  Dim BestTwaInterpol As MathNet.Numerics.Interpolation.IInterpolation

  Public Sub New()
    ImpostaTabelle()
  End Sub

  Public Sub ImpostaTabelle()
    Dim Xv As Double() = {0, 1, 4, 6, 8, 10, 12, 15, 20, 30, 40} ' Tws
    'Dim Yv As Double() = {2, 7, 10, 12, 14, 15, 15, 14, 13, 12, 10} ' Degrees per second
    Dim Yv As Double() = {2, 7, 12, 15, 17, 18, 18, 17, 15, 14, 12} ' Degrees per second
    For i As Integer = 0 To Xv.Count - 1
      RateOfTurn.AddValues(Xv(i), Yv(i))
    Next
    RotInterpol = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(RateOfTurn.X.ToArray, RateOfTurn.Y.ToArray)

    Xv = {0, 4, 6, 8, 10, 12, 14, 16, 18, 20, 25, 30, 40} ' Tws
    Yv = {0.0, 0.4, 0.6, 0.8, 1.0, 1.2, 1.4, 1.6, 1.8, 2.0, 2.5, 3.0, 4.0} ' Power Index ricavato dalle polari, 1 'e relativo al lift a 10 nodi
    For i As Integer = 0 To Xv.Count - 1
      PowerCurve.AddValues(Xv(i), Yv(i))
    Next

    'Public Property DeltaSpeedKts As Double = 5
    'Public Property TwsDeltaSpeed As Double = 10
    'Public Property SecondsToAccelerateDeltaSpeed As Double = 20
    ' ovvero ci vogliono 20 secondi per accelerare di 5 nodi con 10 di tws

    DeltaSpeedKts = 5
    TwsDeltaSpeed = 16
    SecondsToAccelerateDeltaSpeed = 10
    Dim Interpolatore As MathNet.Numerics.Interpolation.IInterpolation = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(PowerCurve.X.ToArray, PowerCurve.Y.ToArray)
    Coeffbase = SecondsToAccelerateDeltaSpeed * Interpolatore.Interpolate(TwsDeltaSpeed)

    For i As Integer = 0 To Xv.Count - 1
      AccelerationKtsSec.AddValues(Xv(i), DeltaSpeedKts / (Coeffbase / Yv(i))) ' Accelerazione in nodi al secondo
    Next
    AccKtsSecInterpol = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(AccelerationKtsSec.X.ToArray, AccelerationKtsSec.Y.ToArray)

    Xv = {0, 10, 20, 30, 40, 70, 80, 90, 95, 98, 101} ' Percentuale della Bs Polare
    Yv = {0.3, 0.7, 0.9, 1, 1, 1, 0.9, 0.6, 0.2, 0.05, 0} ' Efficenza accelerazione
    For i As Integer = 0 To Xv.Count - 1
      AccCurve.AddValues(Xv(i), Yv(i))
    Next
    AccCurveInterpol = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(AccCurve.X.ToArray, AccCurve.Y.ToArray)

    Xv = {0, 5, 10, 20, 30, 40} ' deg per second
    Yv = {0, 2, 4, 8, 12, 15} ' perdita di bs percentuale al secondo
    For i As Integer = 0 To Xv.Count - 1
      LuffingBrake.AddValues(Xv(i), Yv(i))
    Next
    BrakeInterpol = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(LuffingBrake.X.ToArray, LuffingBrake.Y.ToArray)

    Xv = {0, 4, 6, 8, 10, 12, 14, 16, 18, 20, 25, 30, 40} ' Tws
    'Yv = {70, 70, 75, 75, 70, 65, 60, 60, 55, 55, 50, 45, 45} ' Optimal Acceleration Twa
    Yv = {70, 70, 75, 80, 75, 70, 65, 60, 55, 55, 50, 45, 45} ' Optimal Acceleration Twa
    For i As Integer = 0 To Xv.Count - 1
      BestAccTwa.AddValues(Xv(i), Yv(i))
    Next
    BestTwaInterpol = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(BestAccTwa.X.ToArray, BestAccTwa.Y.ToArray)

  End Sub

  Private Function RotDegrees(Tws As Double, Seconds As Double) As Double
    Dim degpersec As Double = RotInterpol.Interpolate(Tws)
    Return degpersec * Seconds
  End Function

  Public Function Calcola(SecToStart As Integer, Position As clsGeographicPosition, Bs As Double, Tws As Double, Twa As Double, Twd As Double, Cse As Double, StepSeconds As Double, Polare As clsValoriPuntoPolare, UpTgt As clsValoriPuntoPolare, PrevRotaton As Double, Acc As Double) As clsVettoreBarca
    ' va implementato il k per quando la barca sta rallentando
    ' va implementato il k per sea state
    Dim Rotation As Double = RotDegrees(Tws, StepSeconds) ' esce sempre positiva
    Dim ApplyBrake As Boolean = False
    Dim TgtBrg As Double = SommaAngolo180adAngolo360(-UpTgt.Twa, Twd) ' BRG ad angolo Tgt mure a dritta
    Dim BestAccBrg As Double = SommaAngolo180adAngolo360(-BestTwaInterpol.Interpolate(Tws), Twd) ' BRG ad angolo best acceleration mure a dritta
    Dim NonPiuBasso As Boolean = False
    Dim NonPiuAlto As Boolean = False
    Dim RotK As Double = 1
    Dim Stbd As Boolean = True
    If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Twd, Cse) > 0 Then
      ' siamo ancora mure a sinistra
      Stbd = False
      ApplyBrake = True
      NonPiuBasso = True
    Else
      If Bs > UpTgt.Bs * 0.9 Then ' se mure a dritta con Bs > di quella target punta l angolo target
        TgtBrg = SommaAngolo180adAngolo360(-UpTgt.Twa, Twd) ' BRG ad angolo Tgt mure a dritta
      ElseIf Bs < UpTgt.Bs * 0.6 Then ' se mure a dritta con Bs > di quella target punta l angolo target
        TgtBrg = BestAccBrg ' BRG ad angolo Tgt mure a dritta
      Else
        TgtBrg = InterpolazioneLineare((Bs / UpTgt.Bs), 0.6, BestAccBrg, 0.9, TgtBrg)
      End If
      If DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Cse, TgtBrg) > 0 Then
        ' se mure a dritta sono piu' basso dell angolo dove devo andare
        ' devo orzare
        Rotation *= -1 ' inverto la direzione della rotazione
        NonPiuAlto = True
        RotK = 0.7
        'ApplyBrake = True
      Else
        NonPiuBasso = True
        ' devo poggiare
      End If
    End If
    Rotation = (Rotation + PrevRotaton) / 2 * RotK
    'If Math.Abs(Rotation) < 5 Then ApplyBrake = False
    Dim FinalCse As Double = SommaAngolo180adAngolo360(-Rotation, Cse)
    If NonPiuBasso Then
      If Stbd AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(FinalCse, TgtBrg) > 0 Then
        FinalCse = TgtBrg
      End If
    Else
      If NonPiuAlto Then
        If Stbd AndAlso DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(TgtBrg, FinalCse) > 0 Then
          FinalCse = TgtBrg
        End If
      End If
    End If
    ' adesso devo calcolare la distanza percorsa
    Dim A As Double = 0
    If Not ApplyBrake Then
      A = AccKtsSecInterpol.Interpolate(Tws)
      Dim KA As Double = AccCurveInterpol.Interpolate(Math.Max(0, Math.Min(100, Bs / Polare.Bs * 100)))
      A *= KA
    End If
    A *= (1 + (Acc / Bs)) ' moltiplica per l accelerazione reale della barca (serve a migliorare o peggiorere l accelerazione se si sta accelerando o decelerando)

    Dim Sow As Double = Math.Max(0, Bs + A * StepSeconds)
    Acc = (Sow - Bs) / StepSeconds ' nodi al secondo
    Dim BsLoss As Double = 0
    If ApplyBrake Then
      BsLoss = (100 - (BrakeInterpol.Interpolate(Math.Abs(Rotation)) * StepSeconds)) / 100
      Sow = Sow * BsLoss
    End If
    Dim Distanza As Double = KtsToMS(Bs) * StepSeconds + (0.5 * A * Math.Pow(StepSeconds, 2))      ' V0*T+0.5*A*T^2

    'Console.Write("SecToStart: " & SecToStart)
    'Console.Write(", ApplyBrake: " & ApplyBrake)
    'Console.Write(", A: " & A)
    'Console.Write(", BsLoss: " & BsLoss)
    'Console.Write(", BS0: " & Bs)
    'Console.Write(", BST: " & Sow)
    'Console.Write(", CSE: " & FinalCse)
    'Console.WriteLine(", Dist: " & Distanza)
    Return New clsVettoreBarca(clsGeoCalculations.PuntoDestinazione(Position, Distanza, FinalCse), Sow, FinalCse, Rotation, Acc)
  End Function

  Public Function LineOrOver(PosizioneCorrente As clsGeographicPosition, PinEnd As clsGeographicPosition, RcToPinBearing As Double) As Boolean
    Return DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(clsGeoCalculations.BearingDegrees(PosizioneCorrente, PinEnd), RcToPinBearing) >= 0
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsVettoreBarca
  Public Property Posizione As clsGeographicPosition
  Public Property Sow As Double
  Public Property Cow As Double
  Public Property ROT As Double
  Public Property Acc As Double

  Public Sub New(Posizione As clsGeographicPosition, Sow As Double, Cow As Double, Rot As Double, Acc As Double)
    Me.Posizione = Posizione
    Me.Sow = Sow
    Me.Cow = Cow
    Me.ROT = Rot
    Me.Acc = Acc
  End Sub
End Class

<AddINotifyPropertyChangedInterface>
Public Class clsStartRotation
  Public Property ActualTwaToBestTwaAcc As Double
  Public Property FinalHeading As Double
  Public Property FinalPosition As clsGeographicPosition
  Public Property RotationTime As Double

  Public Sub Calcola(MaxRotationDegPerSec As Double, ActualTwa As Double, FinalTwa As Double, ActualHdg As Double, ActualPosition As clsGeographicPosition)
    ActualTwaToBestTwaAcc = FinalTwa - ActualTwa
    FinalHeading = SommaAngolo180adAngolo360(ActualTwaToBestTwaAcc, ActualHdg)
    Dim FullSpeedRotationTime As Double = (ActualTwaToBestTwaAcc / MaxRotationDegPerSec)
    RotationTime = FullSpeedRotationTime * 1.3


  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsDfwUtilities
  Public Shared Property HasStartColumns As Boolean


  Public Shared Function SelezionaFileDeckmanAndMakeParquet() As String
    Dim n As String = ""
    'Dim PathExpDataFolder As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
    Dim PathDfwDataFolder As String = AppConfig.ActiveProfile.LastImpFolder '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
    Dim pf As String = ObjFiles.SelezionaFile(PathDfwDataFolder, "", "", "", n)
    If Not System.IO.File.Exists(pf) Then Return ""
    Dim fi As New System.IO.FileInfo(pf)
    PathDfwDataFolder = fi.Directory.FullName
    AppConfig.ActiveProfile.LastImpFolder = PathDfwDataFolder
    AppConfig.Salva()

    Dim d As Dictionary(Of String, List(Of String)) = LeggiFileLog(pf)

    Dim DfwStarts As List(Of clsDfwStart) = Nothing

    If Not HasStartColumns Then
      DfwStarts = AggiungiPartenze(PathDfwDataFolder)
    End If

    Dim FileCreato As String = DeckmanToParquet(pf, PathDfwDataFolder, d, True, True, DfwStarts)


    Return FileCreato

  End Function


  Public Shared LogFileTmOffset As TimeSpan

  Public Shared Function SelezionaFilesDeckmanAndMakeParquet() As String
    Dim n As New List(Of String)
    Dim PathDfwDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathDfwDataFolder, "", "", "", n)
    If pf Is Nothing Then Return ""
    Dim ft As New System.IO.FileInfo(pf.First)
    PathDfwDataFolder = ft.Directory.FullName
    AppConfig.ActiveProfile.LastImpFolder = PathDfwDataFolder
    AppConfig.Salva()
    Console.WriteLine(pf.Count)
    For Each fl In pf
      Dim fi As New System.IO.FileInfo(fl)
      Dim d As Dictionary(Of String, List(Of String)) = LeggiFileLog(fl)
      Console.WriteLine(fl)
      Dim DfwStarts As List(Of clsDfwStart) = AggiungiPartenze(fi.Directory.FullName)
      DeckmanToParquet(fl, PathDfwDataFolder, d, True, False, DfwStarts)
    Next
    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
    MsgBox("Done!")
    Return "" ' System.IO.Path.GetDirectoryName(pf.First)
  End Function





  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of String))
    Dim DictionaryCanali As New Dictionary(Of String, List(Of String))
    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    c.MissingFieldFound = Nothing
    Dim fi As New System.IO.FileInfo(PathLogFile)
    c.Delimiter = SeparatoreCampi(PathLogFile)
    Using reader As New StreamReader(PathLogFile)
      Using csv As New CsvReader(reader, c)
        Using dr As New CsvDataReader(csv)
          Dim dt As New System.Data.DataTable
          dt.Load(dr)
          For col As Integer = 0 To dt.Columns.Count - 2 ' l 'ultima colonna e' dovuta al fatto che nell esportare dfw lascia un tab di troppo
            DictionaryCanali.Add(RimappaNome(dt.Columns(col).ColumnName), New List(Of String))
            For row As Integer = 0 To dt.Rows.Count - 1
              Dim v As String = dt.Rows(row)(col).ToString
              Dim hdr As String = dt.Rows(row)(0).ToString
              'If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
              If IsNumeric(hdr) Then
                DictionaryCanali.Last.Value.Add(v)
              Else
                Dim a As Integer = 0
              End If
              'Else
              '  DictionaryCanali.Last.Value.Add(Double.NaN)
              'End If
            Next
          Next
        End Using
      End Using
    End Using

    Dim FakeChannels As String() ' = {"TmToGun", "BelowLn", "TmToBurn", "TmToLn", "Burn", "GPS time", "Port lat", "Port lon", "Stbd lat", "Stbd lon"}
    If HasStartColumns Then
      FakeChannels = {"BelowLn", "TmToBurn", "TmToLn", "Burn"}
    Else
      FakeChannels = {"TmToGun", "BelowLn", "TmToBurn", "TmToLn", "Burn", "GPS time", "Port lat", "Port lon", "Stbd lat", "Stbd lon"}
    End If
    For Each h In FakeChannels
      CreaNuovoCanaleVuoto(DictionaryCanali, h)
    Next

    Return DictionaryCanali
  End Function


  Private Shared Sub CreaNuovoCanaleVuoto(DictionaryCanali As Dictionary(Of String, List(Of String)), Header As String)
    DictionaryCanali.Add(Header, New List(Of String))
    For i As Integer = 0 To DictionaryCanali.First.Value.Count - 1
      DictionaryCanali.Last.Value.Add(Nothing)
    Next
  End Sub

  Private Shared Sub CreaNuovoCanaleConValoreFisso(DictionaryCanali As Dictionary(Of String, List(Of String)), Header As String, Valore As String)
    DictionaryCanali.Add(Header, New List(Of String))
    For i As Integer = 0 To DictionaryCanali.First.Value.Count - 1
      DictionaryCanali.Last.Value.Add(Valore)
    Next
  End Sub


  Private Shared Function RimappaNome(NomeColonna As String) As String
    Select Case NomeColonna
      Case "PF_Lat"
        Return "Lat"
      Case "PF_Lng"
        Return "Lon"
      Case "AW_angle"
        Return "Awa"
      Case "AW_speed"
        Return "Aws"
      Case "TW_angle"
        Return "Twa"
      Case "TW_speed"
        Return "Tws"
      Case "TW_Dirn"
        Return "Twd"
      Case "Ext_SOG"
        Return "Sog"
      Case "Ext_COG"
        Return "Cog"
      Case "Boatspeed"
        Return "Bsp"
      Case "Course"
        Return "Cse"
      Case "Heading"
        Return "Hdg"
      Case "RUDDER"
        Return "Rudder"
      Case "Deflect"
        Return "DeflectorP"
      Case "V1Port"
        Return "V1 P"
      Case "V1Stbd"
        Return "V1 S"
      Case "PitchRate"
        Return "TrimRate"
      Case "YawRate"
        Return "ROT"
' RC_LAT;RC_LON;PIN_LAT;PIN_LON;TIMER;GGAUTC
'     Dim FakeChannels As String() = {"TmToGun", "BelowLn", "TmToBurn", "TmToLn", "Burn", "GPS time", "Port lat", "Port lon", "Stbd lat", "Stbd lon"}
      Case "TIMER"
        HasStartColumns = True
        Return "TmToGun"
      Case "RC_LAT"
        HasStartColumns = True
        Return "Stbd lat"
      Case "RC_LON"
        HasStartColumns = True
        Return "Stbd lon"
      Case "PIN_LAT"
        HasStartColumns = True
        Return "Port lat"
      Case "PIN_LON"
        HasStartColumns = True
        Return "Port lon"
      Case "GGAUTC"
        HasStartColumns = True
        Return "GPS time"
      Case Else
        Return NomeColonna
    End Select
  End Function

  Private Shared Function SeparatoreCampi(PathFile As String)
    Using sr As New StreamReader(PathFile)
      Dim Line As String = Nothing
      Do
        Line = sr.ReadLine
        If Not Line Is Nothing Then
          If Not Line.Trim = "" Then
            If Line.Contains(vbTab) Then Return vbTab
            If Line.Contains(",") Then Return ","
            If Line.Contains(";") Then Return ";"
          End If
        End If
      Loop While Not Line Is Nothing
      Return ","
    End Using

  End Function


  Public Shared Function DeckmanToParquet(SourcePathFile As String, StorageFolder As String, DictionaryCanali As Dictionary(Of String, List(Of String)), ImportaSoloInMovimento As Boolean, OpenFolder As Boolean, Starts As List(Of clsDfwStart)) As String
    Try
      Dim IdInizioImportazione As Integer
      Dim IdFineImportazione As Integer
      Dim Samples As Integer

      Dim FolderDate As String = ""
      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
      Dim ds As New List(Of Double)
      Dim DataDouble As New List(Of Double)

      'Dim ChDfwTm = DictionaryCanali.Where(Function(x) x.Key.ToLower = "Time").FirstOrDefault
      'Dim ChDfwDt = DictionaryCanali.Where(Function(x) x.Key.ToLower = "Date").FirstOrDefault

      Dim TimeHeader, DateHeader As String
      If DictionaryCanali.ContainsKey("Time") Then
        TimeHeader = "Time"
      ElseIf DictionaryCanali.ContainsKey("hhmmss") Then
        TimeHeader = "hhmmss"
      Else
        Stop
      End If
      If DictionaryCanali.ContainsKey("Date") Then
        DateHeader = "Date"
      ElseIf DictionaryCanali.ContainsKey("dd/mm/yy") Then
        DateHeader = "dd/mm/yy"
      Else
        Stop
      End If

      If DictionaryCanali.ContainsKey(TimeHeader) AndAlso DictionaryCanali.ContainsKey(DateHeader) Then
        Dim DfwTm = DictionaryCanali(TimeHeader).ToArray
        Dim DfwDt = DictionaryCanali(DateHeader).ToArray
        Dim dt As New List(Of DateTime)
        Dim dtnn As New List(Of DateTime)
        Dim FormatiDfw As String() = {"dd/MM/yyyy HH:mm:ss", "dd-MM-yy HH:mm:ss", "dd/MM/yy HH:mm:ss"}
        For i As Integer = 0 To DfwTm.Count - 1
          Dim dttmp As DateTime
          If DateTime.TryParseExact(DfwDt(i) & " " & DfwTm(i), FormatiDfw, Nothing, Globalization.DateTimeStyles.None, dttmp) Then
            Dim dttmpoff = dttmp.Add(LogFileTmOffset)
            dt.Add(dttmpoff)
            dtnn.Add(dttmpoff)
            DataDouble.Add(dttmpoff.ToOADate)
          Else
            dt.Add(Nothing)
            DataDouble.Add(Double.NaN)
          End If
        Next
        Dim day As Date = DateTime.FromOADate(Int(dtnn.Select(Function(x) x.ToOADate).Average))
        FolderDate = day.ToString("yyyyMMdd")
        ds.Clear()
        For Each d In dt
          If d = Nothing Then
            ds.Add(Double.NaN)
          Else
            If d < day Then
              ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
            Else
              ds.Add(d.TimeOfDay.TotalSeconds)
            End If
          End If

        Next
      End If
      Dim b = ds.Min
      Dim a = ds.Max

      If Not Starts Is Nothing Then
        Dim tzIana As String = TimeZoneLookup.GetTimeZone(Starts.First.StbdEnd.LatDec, Starts.First.StbdEnd.LngDec).Result
        Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
        Dim lastindex As Integer = -1
        For Each s In Starts
          'Dim tzIana As String = TimeZoneLookup.GetTimeZone(s.StbdEnd.LatDec, s.StbdEnd.LngDec).Result
          'Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
          Dim t As DateTime = s.StartTime
          Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)

          'se e'nello stesso giorno
          'Dim aa = 1
          'Dim idx = ds.IndexOf(CInt(convertedTime.LocalDateTime.AddMinutes(-5).TimeOfDay.TotalSeconds))
          Dim std As Double = t.ToOADate
          If std >= DataDouble.First AndAlso std <= DataDouble.Last Then
            'Dim idx = ds.IndexOf(CInt(t.AddMinutes(-5).TimeOfDay.TotalSeconds))
            Dim idx = ds.IndexOf(CInt(t.TimeOfDay.TotalSeconds))
            'Dim dd = DataDouble(idx)
            If idx > -1 Then
              DictionaryCanali("Stbd lat")(idx - 100) = s.StbdEnd.LatDec
              DictionaryCanali("Stbd lon")(idx - 100) = s.StbdEnd.LngDec
              DictionaryCanali("Port lat")(idx - 150) = s.PortEnd.LatDec
              DictionaryCanali("Port lon")(idx - 150) = s.PortEnd.LngDec
              Dim idxfrom As Integer = 300
              If lastindex > -1 Then
                idxfrom = Math.Min(idx - lastindex - 5, 300)
              End If
              For i As Integer = idxfrom To -10 Step -1
                DictionaryCanali("TmToGun")(idx - i) = (i / (24 * 3600))
              Next
              lastindex = idx
            End If
          End If

        Next
      End If
      ' rimuove i canali non double
      DictionaryCanali.Remove(TimeHeader)
      DictionaryCanali.Remove(DateHeader)
      DictionaryCanali.Remove("DR_Lat")
      DictionaryCanali.Remove("DR_Lng")
      'If DictionaryCanali.ContainsKey("Lat") AndAlso DictionaryCanali.ContainsKey("Lon") Then
      If DictionaryCanali.ContainsKey("Lat") AndAlso DictionaryCanali.ContainsKey("Long") Then
        'For i As Integer = 0 To DictionaryCanali("Lat").Count - 1
        '  DictionaryCanali("Lat")(i) = clsGeoCalculations.StrLatLonDfwToDeg(DictionaryCanali("Lat")(i))
        '  'DictionaryCanali("Lon")(i) = clsGeoCalculations.StrLatLonDfwToDeg(DictionaryCanali("Lon")(i))
        '  DictionaryCanali("Long")(i) = clsGeoCalculations.StrLatLonDfwToDeg(DictionaryCanali("Long")(i))
        'Next
      Else
        ' a volte ci si puo' scordare di esportare il campo del gps....
        CreaNuovoCanaleConValoreFisso(DictionaryCanali, "Lat", 0)
        CreaNuovoCanaleConValoreFisso(DictionaryCanali, "Lon", 0)
      End If

      If ImportaSoloInMovimento Then
        Dim bs As String()
        Dim bsok As Boolean = True
        bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
        For c As Integer = 0 To bs.Count - 1
          If bs(c) > 1 Then
            IdInizioImportazione = c
            Exit For
          End If
        Next
        For c As Integer = bs.Count - 1 To 0 Step -1
          If bs(c) > 1 Then
            IdFineImportazione = c
            Exit For
          End If
        Next
      Else
        IdInizioImportazione = 0
        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
      End If
      Samples = IdFineImportazione - IdInizioImportazione

      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

      Dim Hz As Integer = clsExpeditionUtilities.VerificaCanaleHz(DictionaryCanali, ds.ToArray)
      clsExpeditionUtilities.VerificaCanaleROT(DictionaryCanali, ds, "ROT")
      Dim HeadersRate As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT", "YawR", "PitchR", "RollR"}
      Dim HeadersTrim As String() = {"Pitch", "Trim", "Trm"}
      clsExpeditionUtilities.VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


      For Each Canale In DictionaryCanali
        Select Case Canale.Key
          Case "TrimRate", "HeelRate", "Pitch rate", "Roll Rate", "Pitch rate", "TrimRT", "YawR", "PitchR", "RollR"
            If clsExpeditionUtilities.HasValidData(Canale.Value) Then
              DF = New Parquet.Data.DataField(Canale.Key, Parquet.Data.DataType.Double, False)
              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).Select(Function(x) ChckDbl(x)).ToArray))
            End If
          Case Else
            DF = New Parquet.Data.DataField(Canale.Key, Parquet.Data.DataType.Double, False)
            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).Select(Function(x) ChckDbl(x)).ToArray))
        End Select
      Next
      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
      If Not System.IO.Directory.Exists(StorageFolder) Then
        System.IO.Directory.CreateDirectory(StorageFolder)
      End If

      'Dim twss As String() = DictionaryCanali.Where(Function(x) x.Key.ToLower = "tws").FirstOrDefault.Value.ToArray
      Dim fi As New System.IO.FileInfo(SourcePathFile)
      Dim FileName As String = ("" + fi.Name).Replace(fi.Extension, ".ppf") ' parquet performance file

      Dim SW As New System.IO.StreamWriter(System.IO.Path.Combine(StorageFolder, FileName))
      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
      For Each C In CanaliPq
        PGR.WriteColumn(C)
      Next
      PW.Dispose()
      SW.Close()
      SW.Dispose()

      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

      Return System.IO.Path.Combine(StorageFolder, FileName)
    Catch ex As Exception
      Return ""
    End Try
  End Function

  Private Shared Function ChckDbl(Valore As String) As Double
    If Valore = "" Then
      Return Double.NaN
    Else
      Return CDbl(Valore)
    End If
  End Function


  Private Shared Function AggiungiPartenze(Folder As String) As List(Of clsDfwStart)
    Dim DfwStarts As New List(Of clsDfwStart)
    DfwStarts.Clear()

    'Dim prova = New Date(1970, 1, 1, 0, 0, 0).AddSeconds(1632755403)

    For Each f In System.IO.Directory.GetFiles(Folder)
      Dim fi As New IO.FileInfo(f)
      If fi.Extension = ".d" Then
        Dim Righe As List(Of String) = ObjFiles.TestoInLista(f)
        If Righe.Count = 6 Then
          Dim st As DateTime
          Dim dfwstart As clsDfwStart
          If Righe(5).StartsWith("L") Then
            Dim ldt = Righe(5).Split("-")
            st = New DateTime(ldt(1), ldt(2), ldt(3), ldt(4), ldt(5), ldt(6))
            dfwstart = New clsDfwStart(New clsGeographicPosition(Righe(2), Righe(3)), New clsGeographicPosition(Righe(0), Righe(1)), st)
          Else
            st = New DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Righe(5))
            dfwstart = New clsDfwStart(New clsGeographicPosition(Righe(2), Righe(3)), New clsGeographicPosition(Righe(0), Righe(1)), st.Add(LogFileTmOffset))
          End If

          If DfwStarts.Where(Function(x) x.StbdEnd.PositionString = dfwstart.StbdEnd.PositionString AndAlso x.PortEnd.PositionString = dfwstart.PortEnd.PositionString AndAlso x.StartTime = dfwstart.StartTime).Count = 0 Then
            DfwStarts.Add(dfwstart)
          End If
        End If
      End If
    Next
    Return DfwStarts
  End Function

End Class

Public Class clsDfwStart
  Public Property StbdEnd As clsGeographicPosition
  Public Property PortEnd As clsGeographicPosition
  Public Property StartTime As DateTime

  Public Sub New(StbdEnd As clsGeographicPosition, PortEnd As clsGeographicPosition, StartTime As DateTime)
    Me.StbdEnd = StbdEnd
    Me.PortEnd = PortEnd
    Me.StartTime = StartTime
  End Sub

End Class


'<AddINotifyPropertyChangedInterface>
'Public Class clsExpeditionUtilities

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As Double
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Average)
'  End Function

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As String
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Select(Function(x) CDbl(x)).Average)
'  End Function

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of Double)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          'Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of String)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub


'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of Double) = Nothing
'      'Dim headers As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'          Exit For
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'headers = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          Stop
'        Else
'          Dim WH(Valori.Count - 1) As Double
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As Double
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(Valori(i)))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of String) = Nothing
'      ' HeadersRate = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) Then
'          If HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'HeadersTrim = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          Stop
'        Else
'          Dim WH(Valori.Count - 1) As String
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As String
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(CDbl(Valori(i))))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Function NomeCanaleStandardizzato(Header As String) As String
'    Select Case Header
'      Case "AWA"
'        Return "Awa"
'      Case "AWS"
'        Return "Aws"
'      Case "TWA"
'        Return "Twa"
'      Case "TWS"
'        Return "Tws"
'      Case "HDG"
'        Return "Hdg"
'      Case "COG"
'        Return "Cog"
'      Case "SOG"
'        Return "Sog"
'      Case "TWD"
'        Return "Twd"
'      Case "BSP"
'        Return "Bs"
'      Case "Bsp"
'        Return "Bs"
'      Case "Pitch rate"
'        Return "PitchRate"
'      Case "Heel Rate"
'        Return "HeelRate"
'      Case "Roll Rate"
'        Return "HeelRate"
'      Case "TrimRate"
'        Return "PitchRate"
'      Case "Lat"
'        Return "LatBow"
'      Case "Lon"
'        Return "LonBow"
'      Case Else
'        Return Header
'    End Select
'  End Function


'  Class ExpNewFormat
'    Public Import As Boolean = False
'    Public Header As String
'    Public Values As New List(Of Double)
'  End Class

'  Public Shared Function ReadExpeditionLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    Dim inizio As DateTime = Now
'    Dim p As New Microsoft.VisualBasic.FileIO.TextFieldParser(PathLogFile)
'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(400) As ExpNewFormat
'    For i As Integer = 0 To 400
'      ENF(i) = New ExpNewFormat
'    Next

'    p.SetDelimiters(",")
'    'Dim MappingIdCanali As New Dictionary(Of Integer, String)
'    Do While Not p.EndOfData
'      Dim r As String() = p.ReadFields
'      If (r(0) = "!Boat") Then
'        t = 2
'        Dim r1 = p.ReadFields
'        For c As Integer = 1 To r.Count - 1
'          ENF(r1(c)).Header = r(c)
'        Next

'        Dim r2 = p.ReadFields
'      ElseIf (r(0) = "Boat") Then
'        t = 1
'        For c As Integer = 0 To r.Count - 1
'          DictionaryCanali.Add(r(c), New List(Of Double))
'        Next

'      Else
'        t = -1
'      End If
'      Exit Do
'    Loop


'    Dim ValidBoat As Integer = 0
'    If t = 1 Then ' formato ante 2022
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        For c As Integer = 0 To r.Count - 1
'          Dim v As Double = Double.NaN
'          If Double.TryParse(r(c).ToString, v) Then
'            DictionaryCanali.Last.Value.Add(v)
'          Else
'            DictionaryCanali.Last.Value.Add(Double.NaN)
'          End If
'        Next
'      Loop
'      p.Close()
'    ElseIf t = 2 Then
'      Dim nr As Integer = 0 ' serve per rimpire le righe precedenti dei canali aggiunti a file creato
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        Dim boat As Integer
'        If Integer.TryParse(r(0), boat) Then
'          If (boat = ValidBoat) Then
'            For Each ch As ExpNewFormat In ENF
'              ch.Values.Add(Double.NaN)
'            Next
'            Dim v As Double = Double.NaN
'            If Double.TryParse(r(1), v) Then
'              ENF(0).Import = True ' marca come da importare il canale se c'e' almeno un numero
'              ENF(0).Values(nr) = v
'            End If
'            For c As Integer = 2 To r.Count - 1 Step 2
'              Dim id As Integer
'              If Integer.TryParse(r(c), id) Then
'                v = Double.NaN
'                If Double.TryParse(r(c + 1), v) Then
'                  ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                  ENF(id).Values(nr) = v
'                End If
'              End If
'            Next
'            nr += 1
'          ElseIf r(0) = "!Boat" Then
'            Dim r1 = p.ReadFields ' riga con gli id
'            For c As Integer = 1 To r.Count - 1
'              ENF(r1(c)).Header = r(c)
'            Next
'            Dim r2 = p.ReadFields ' riga con la versione
'          End If
'        End If


'      Loop
'      For Each ch As ExpNewFormat In ENF
'        If ch.Import Then
'          DictionaryCanali.Add(ch.Header, ch.Values)
'        End If
'      Next
'      p.Close()
'    End If

'    Console.WriteLine("ReadExpeditionLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function

'  Public Shared Function ReadExpeditionLog2(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    'Dim inizio As DateTime = Now
'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(500) As ExpNewFormat
'    For i As Integer = 0 To 500
'      ENF(i) = New ExpNewFormat
'    Next


'    Using sr As New StreamReader(PathLogFile)
'      Dim Line As String = Nothing
'      Do
'        Line = sr.ReadLine
'        If Not Line Is Nothing Then
'          Dim r As String() = Line.Split(",")
'          Dim r1 = sr.ReadLine.Split(",")
'          If (r(0) = "!Boat" AndAlso r1(0) = "!boat") Then
'            t = 2
'            'Dim r1 = sr.ReadLine.Split(",")
'            For c As Integer = 1 To r.Count - 1
'              ENF(r1(c)).Header = r(c)
'            Next

'            Dim r2 = sr.ReadLine.Split(",")
'          ElseIf (r(0) = "Boat" OrElse r(0) = "!Boat") Then
'            t = 1
'            For c As Integer = 0 To r.Count - 1
'              DictionaryCanali.Add(r(c), New List(Of Double))
'            Next

'          ElseIf (r(0) = "UTC") Then
'            t = 1
'            For c As Integer = 0 To r.Count - 1
'              DictionaryCanali.Add(r(c), New List(Of Double))
'            Next

'          Else
'            t = -1
'          End If
'          Exit Do
'        End If
'      Loop While Not Line Is Nothing

'      Dim ValidBoat As Integer = 0
'      If t = 1 Then ' formato ante 2022 o esportato
'        Do
'          Line = sr.ReadLine
'          If Not Line Is Nothing Then
'            Dim r As String() = Line.Split(",")
'            For c As Integer = 0 To DictionaryCanali.Count - 1
'              Dim v As Double = Double.NaN
'              If (c > r.Count() - 1) Then
'                'Stop
'                DictionaryCanali.Values(c).Add(Double.NaN)
'              Else
'                If Double.TryParse(r(c).ToString, v) Then
'                  DictionaryCanali.Values(c).Add(v)
'                Else
'                  DictionaryCanali.Values(c).Add(Double.NaN)
'                End If
'              End If
'            Next
'          End If
'        Loop While Not Line Is Nothing
'      ElseIf t = 2 Then
'        Dim nr As Integer = 0 ' serve per rimpire le righe precedenti dei canali aggiunti a file creato
'        Do
'          Line = sr.ReadLine
'          If Not Line Is Nothing Then
'            Dim CheckRow As Boolean = True
'            Dim r As String() = Line.Split(",")
'            If Line.Contains("!Boat") AndAlso Not r(0) = "!Boat" Then
'              CheckRow = False
'            ElseIf Line.Contains("!boat") AndAlso Not r(0) = "!boat" Then
'              CheckRow = False
'            End If
'            Dim boat As Integer
'            If CheckRow AndAlso Integer.TryParse(r(0), boat) Then
'              If (boat = ValidBoat) Then
'                For Each ch As ExpNewFormat In ENF
'                  ch.Values.Add(Double.NaN)
'                Next
'                Dim v As Double = Double.NaN
'                If Double.TryParse(r(1), v) Then
'                  ENF(0).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                  ENF(0).Values(nr) = v
'                End If
'                For c As Integer = 2 To r.Count - 1 Step 2
'                  Dim id As Integer
'                  If Integer.TryParse(r(c), id) Then
'                    v = Double.NaN
'                    If c + 1 < r.Count AndAlso CheckDouble(r(c + 1), v) Then
'                      ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                      ENF(id).Values(nr) = v
'                    End If
'                    'If c + 1 < r.Count AndAlso Not r(c + 1) = "" AndAlso Not r(c + 1) = "nan" AndAlso Not r(c + 1) = "-nan" AndAlso Not r(c + 1) = "-nan(ind)" AndAlso Not r(c + 1) = "nan(ind)" AndAlso Not r(c + 1) = "-" AndAlso Not Double.IsNaN(r(c + 1)) Then
'                    '  If Double.TryParse(r(c + 1), v) Then
'                    '    ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                    '    ENF(id).Values(nr) = v
'                    '  End If
'                    'End If
'                  End If
'                Next
'                nr += 1
'              ElseIf r(0) = "!Boat" Then
'                Dim r1 = sr.ReadLine.Split(",")
'                For c As Integer = 1 To r.Count - 1
'                  ENF(r1(c)).Header = r(c)
'                Next
'                Dim r2 = sr.ReadLine.Split(",")
'              End If
'            End If
'          End If
'        Loop While Not Line Is Nothing
'        For Each ch As ExpNewFormat In ENF
'          If ch.Import AndAlso Not ch.Header Is Nothing Then
'            DictionaryCanali.Add(ch.Header, ch.Values)
'          End If
'        Next
'      End If
'    End Using

'    'Console.WriteLine("ReadExpeditionLog2: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function

'  Public Shared Function CheckDouble(ValueString As String, ByRef Value As Double) As Boolean
'    Dim valid As Boolean = False
'    Value = Double.NaN
'    '"nan" AndAlso Not r(c + 1) = "-nan" AndAlso Not r(c + 1) = "-nan(ind)" AndAlso Not r(c + 1) = "nan(ind)" AndAlso Not r(c + 1) = "-" AndAlso Not Double.IsNaN(r(c + 1)) Then
'    Select Case ValueString
'      Case ""
'      Case "nan"
'      Case "-nan"
'      Case "nan(ind)"
'      Case "-nan(ind)"
'      Case "-"
'      Case "."
'      Case Else
'        valid = Double.TryParse(ValueString, Value)
'    End Select
'    Return valid
'  End Function


'  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))
'    Dim inizio As DateTime = Now
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
'    c.MissingFieldFound = Nothing
'    Dim fi As New System.IO.FileInfo(PathLogFile)
'    If fi.Extension = ".gz" Then
'      Stop
'      ' va gestito il formato faro compresso
'    End If
'    c.Delimiter = SeparatoreCampi(PathLogFile)
'    Using reader As New StreamReader(PathLogFile)
'      Using csv As New CsvReader(reader, c)
'        Using dr As New CsvDataReader(csv)
'          Dim dt As New System.Data.DataTable
'          dt.Load(dr)
'          If Is2022expFormat(dt.Columns(0).ColumnName) Then
'            ' formato 2022
'            Dim MappingIdCanali As New Dictionary(Of Integer, String)
'            DictionaryCanali.Add(dt.Columns(0).ColumnName, New List(Of Double))
'            MappingIdCanali.Add(-1, dt.Columns(0).ColumnName)
'            For col As Integer = 1 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              MappingIdCanali.Add(dt.Rows(0)(col), dt.Columns(col).ColumnName)
'            Next
'            For row As Integer = 0 To dt.Rows.Count - 1
'              If IsNumeric(dt.Rows(row)(0).ToString) Then
'                For Each Canale In DictionaryCanali
'                  Canale.Value.Add(Double.NaN)
'                Next
'                For col As Integer = 0 To dt.Columns.Count - 1 Step 2
'                  If Not IsDBNull(dt.Rows(row)(col)) Then
'                    Try
'                      Dim Id As Integer = dt.Rows(row)(col)
'                      Dim v As Double = Double.NaN
'                      If Double.TryParse(dt.Rows(row)(col + 1).ToString, v) Then
'                        Dim Canale = DictionaryCanali(MappingIdCanali(Id))
'                        Canale(Canale.Count - 1) = v
'                      End If
'                    Catch ex As Exception

'                    End Try
'                  End If

'                Next
'              End If
'            Next
'          Else
'            ' formato ante 2022
'            For col As Integer = 0 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              For row As Integer = 0 To dt.Rows.Count - 1
'                Dim v As Double = Double.NaN
'                If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
'                  DictionaryCanali.Last.Value.Add(v)
'                Else
'                  DictionaryCanali.Last.Value.Add(Double.NaN)
'                End If
'              Next
'            Next
'          End If
'        End Using
'      End Using
'    End Using

'    Console.WriteLine("LeggiFileLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function

'  Private Shared Function Is2022expFormat(FirstChannelName As String) As Boolean
'    Return FirstChannelName = "!Boat"
'  End Function


'  Private Shared Function SeparatoreCampi(PathFile As String)
'    Using sr As New StreamReader(PathFile)
'      Dim Line As String = Nothing
'      Do
'        Line = sr.ReadLine
'        If Not Line Is Nothing Then
'          If Not Line.Trim = "" Then
'            If Line.Contains(vbTab) Then Return vbTab
'            If Line.Contains(",") Then Return ","
'          End If
'        End If
'      Loop While Not Line Is Nothing
'      Return ","
'    End Using

'  End Function


'  Public Shared Function SelezionaFileExpeditionAndMakeParquet() As String
'    Dim n As String = ""
'    'Dim PathExpDataFolder As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder '  AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
'    Dim pf As String = ObjFiles.SelezionaFile(PathExpDataFolder, "", "", "", n)
'    If Not System.IO.File.Exists(pf) Then Return ""
'    Dim fi As New System.IO.FileInfo(pf)
'    PathExpDataFolder = fi.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()
'    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", PathExpDataFolder, True, True)

'    'Dim d2 As Dictionary(Of String, List(Of Double)) = LeggiFileLog(pf)

'    'Dim d1 As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog(pf)

'    Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(pf)

'    'If True Then
'    '    If d.ContainsKey("BSP") AndAlso d.ContainsKey("SOG") Then
'    '        d("BSP") = d("SOG")
'    '    End If
'    'End If


'    Return ExpeditionToParquet(pf, PathExpDataFolder, d, True, True)


'  End Function


'  Public Shared Function SelezionaFilesExpeditionAndMakeParquet() As String
'    Dim n As New List(Of String)
'    'Dim PathExpDataFolder As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

'    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathExpDataFolder, "", "", "", n)
'    If pf Is Nothing Then Return ""
'    Dim ft As New System.IO.FileInfo(pf.First)
'    PathExpDataFolder = ft.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()
'    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", PathExpDataFolder, True, True)
'    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", System.IO.Path.GetDirectoryName(pf.First), True, True)
'    Console.WriteLine(pf.Count)
'    For Each fl In pf
'      Dim fi As New System.IO.FileInfo(fl)
'      Dim d As Dictionary(Of String, List(Of Double)) = LeggiFileLog(fl)
'      Console.WriteLine(fl)
'      ExpeditionToParquet(fl, PathExpDataFolder, d, True, False)
'    Next
'    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
'    MsgBox("Done!")
'    Return "" ' System.IO.Path.GetDirectoryName(pf.First)
'  End Function




'  Public Shared Function ExpeditionToParquet(SourcePathFile As String, StorageFolder As String, DictionaryCanali As Dictionary(Of String, List(Of Double)), ImportaSoloInMovimento As Boolean, OpenFolder As Boolean) As String
'    Try

'      Dim IdInizioImportazione As Integer = -1
'      Dim IdFineImportazione As Integer = -1
'      Dim Samples As Integer

'      Dim FolderDate As String = ""
'      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

'      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
'      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
'      Dim ds As New List(Of Double)
'      Dim DataDouble As New List(Of Double)

'      If Not DictionaryCanali.Where(Function(x) x.Key.ToLower = "utc") Is Nothing Then
'        Dim lats = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault().Value.ToArray
'        Dim lngs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault().Value.ToArray
'        Dim lat As Double = lats.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
'        Dim lng As Double = lngs.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
'        Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
'        Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
'        Dim utc = DictionaryCanali.Where(Function(x) x.Key.ToLower = "utc").FirstOrDefault().Value.ToArray
'        Dim dt As New List(Of DateTime)
'        Dim validdt As New List(Of Double)
'        For Each ut In utc
'          If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
'            Dim t As DateTime = DateTime.FromOADate(ut)
'            Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
'            dt.Add(convertedTime.LocalDateTime)
'            DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
'            validdt.Add(DataDouble.Last)
'          Else
'            dt.Add(Nothing)
'            DataDouble.Add(Double.NaN)
'          End If
'        Next
'        Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
'        FolderDate = day.ToString("yyyyMMdd")
'        ds.Clear()
'        For Each d In dt
'          If d = Nothing Then
'            ds.Add(Double.NaN)
'          Else
'            If d < day Then
'              ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
'            Else
'              ds.Add(d.TimeOfDay.TotalSeconds)
'            End If
'          End If

'        Next





'        'Dim dsa As Double() = ds.ToArray
'        'CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
'        'CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.ToArray))

'        'Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, dsa)
'        'VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", "PitchRate", "TrimRate", Hz)
'      Else
'        FolderDate = DateTime.Today.ToString("yyyyMMdd")
'      End If

'      If ImportaSoloInMovimento Then
'        Dim bs As Double()
'        Dim bsok As Boolean = True
'        'Dim Sog = DictionaryCanali.Where(Function(x) x.Key.ToLower = "sog").FirstOrDefault().Value.ToArray
'        If DictionaryCanali.ContainsKey("Bs") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bs").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("Bsp") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("BSP") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        Else
'          bsok = False
'          IdInizioImportazione = 0
'          IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'        End If
'        If bsok Then
'          For c As Integer = 0 To bs.Count - 1
'            If bs(c) > 1 Then
'              IdInizioImportazione = c
'              Exit For
'            End If
'          Next
'          For c As Integer = bs.Count - 1 To 0 Step -1
'            If bs(c) > 1 Then
'              IdFineImportazione = c
'              Exit For
'            End If
'          Next
'        End If
'      Else
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If
'      If ImportaSoloInMovimento AndAlso (IdInizioImportazione = -1 OrElse IdFineImportazione) OrElse IdInizioImportazione = IdFineImportazione Then
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If
'      Samples = IdFineImportazione - IdInizioImportazione

'      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
'      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
'      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

'      Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, ds.ToArray)
'      VerificaCanaleROT(DictionaryCanali, ds, "ROT")
'      'VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", "PitchRate", "TrimRate", Hz)
'      Dim HeadersRate As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      Dim HeadersTrim As String() = {"Pitch", "Trim", "Trm"}
'      VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


'      For Each Canale In DictionaryCanali
'        'If Canale.Key = "ROT" Then Stop
'        Select Case Canale.Key
'          Case "TrimRate", "HeelRate", "Pitch rate", "Roll Rate", "HeelRT"
'            If HasValidData(Canale.Value) Then
'              DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'            End If

'          Case Else
'            DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'        End Select





'      Next

'      Dim strnfilepath As String = System.IO.Path.Combine(StorageFolder, "strn.txt")

'      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
'      If Not System.IO.Directory.Exists(StorageFolder) Then
'        System.IO.Directory.CreateDirectory(StorageFolder)
'      End If

'      If System.IO.File.Exists(strnfilepath) Then
'        System.IO.File.Copy(strnfilepath, System.IO.Path.Combine(StorageFolder, "strn.txt"))
'      End If


'      'LatLonPositionIsTheBow = Not System.IO.File.Exists(pFileInfo.FullName.Replace(pFileInfo.Name, "strn.txt"))

'      Dim twss As Double() = DictionaryCanali.Where(Function(x) x.Key.ToLower = "tws").FirstOrDefault.Value.ToArray
'      Dim fi As New System.IO.FileInfo(SourcePathFile)
'      Dim FileName As String = ("" + fi.Name).Replace(fi.Extension, ".ppf") ' parquet performance file

'      Dim SW As New System.IO.StreamWriter(System.IO.Path.Combine(StorageFolder, FileName))
'      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
'      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
'      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
'      For Each C In CanaliPq
'        PGR.WriteColumn(C)
'      Next
'      PW.Dispose()
'      SW.Close()
'      SW.Dispose()

'      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

'      Return System.IO.Path.Combine(StorageFolder, FileName)
'    Catch ex As Exception
'      Return ""
'    End Try

'  End Function

'  Public Shared Function HasValidData(Valori As List(Of Double)) As Boolean
'    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
'  End Function

'  Public Shared Function HasValidData(Valori As List(Of String)) As Boolean
'    Dim a = Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
'    Return a.Count > 0
'  End Function


'  'bool ExpeditionToParquet(String SourcePathFile, string StorageFolder, Dictionary<String, List<Double>> DictionaryCanali)
'  '{
'  '  Try
'  '  {
'  '    Double[] twss = DictionaryCanali.Where(x => x.Key == "TWS").FirstOrDefault().Value.ToArray();
'  '    Clipboard.SetText(string.Join("\n", twss));
'  '    System.IO.FileInfo fi = New System.IO.FileInfo(SourcePathFile);
'  '    String FileName = ("CSV0_" + fi.Name).Replace(fi.Extension, ".parquet");
'  '    String FolderDate = "";
'  '    List<Parquet.Data.DataColumn> CanaliPq = New List<Parquet.Data.DataColumn>();
'  '    If (DictionaryCanali.Where(x >= x.Key == "Utc")!= null)
'  '    {
'  '      Double[] lats = DictionaryCanali.Where(x => x.Key == "Lat").FirstOrDefault().Value.ToArray();
'  '      Double[] lngs = DictionaryCanali.Where(x => x.Key == "Lon").FirstOrDefault().Value.ToArray();
'  '      Double lat = lats.Where(x >= !double.IsNaN(x) && x!= 0).Average();
'  '      Double lng = lngs.Where(x >= !double.IsNaN(x) && x!= 0).Average();
'  '      String tzIana = TimeZoneLookup.GetTimeZone(lat, lng).Result;
'  '      TimeZoneInfo tzInfo = TZConvert.GetTimeZoneInfo(tzIana);
'  '      //DateTimeOffset convertedTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzInfo);

'  '      Parquet.Data.DataField DF = New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, false);

'  '      Double[] utc = DictionaryCanali.Where(x => x.Key == "Utc").FirstOrDefault().Value.ToArray();
'  '      List<double> ds = New List<double>();
'  '      foreach (var ut in utc)
'  '      {
'  '        If (ut!= 0 && !double.IsNaN(ut))
'  '        {
'  '          DateTime t = DateTime.FromOADate(ut);
'  '          DateTimeOffset convertedTime = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo);
'  '          ds.Add(convertedTime.TimeOfDay.TotalSeconds);
'  '          If (FolderDate == "")
'  '          {
'  '            If (convertedTime.LocalDateTime!= DateTime.MinValue)
'  '            {
'  '              FolderDate = convertedTime.LocalDateTime.ToString("yyyMMdd");
'  '            }
'  '          }
'  '        }
'  '      }
'  '      Double[] dsa = ds.ToArray();
'  '      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa));

'  '      int Hz = VerificaCanaleHz(ref DictionaryCanali, dsa);
'  '      VerificaCanaleSeaStateIndex(ref DictionaryCanali, "SeaStateIndex", "PitchRate", "TrimRate", Hz);
'  '    }
'  '    Else
'  '    {
'  '      FolderDate = DateTime.Today.ToString("yyyMMdd");
'  '    }


'  '    foreach (var Canale in DictionaryCanali)
'  '    {
'  '      Parquet.Data.DataField DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key,LogFileSource.Expedition), Parquet.Data.DataType.Double, false);
'  '      CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.ToArray()));
'  '    }

'  '    StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate);
'  '    If (!System.IO.Directory.Exists(StorageFolder))
'  '    {
'  '      System.IO.Directory.CreateDirectory(StorageFolder);
'  '    }
'  '    StreamWriter SW = New StreamWriter(System.IO.Path.Combine(StorageFolder, FileName));
'  '    Parquet.Data.Schema PS = New Parquet.Data.Schema(CanaliPq.Select(x => x.Field).ToList());
'  '    Parquet.ParquetWriter PW = New Parquet.ParquetWriter(PS, SW.BaseStream);
'  '    Parquet.ParquetRowGroupWriter PGR = PW.CreateRowGroup();

'  '    foreach (var C in CanaliPq)
'  '    {
'  '      PGR.WriteColumn(C);
'  '    }
'  '    PW.Dispose();
'  '    SW.Close();
'  '    SW.Dispose();

'  '    System.Diagnostics.Process.Start("explorer.exe", StorageFolder);

'  '    Return True;
'  '  }
'  '  Catch
'  '  {
'  '    Return False;
'  '  }
'  '}



'  '  int VerificaCanaleHz(ref Dictionary<string, List<Double>> DictionaryCanali, Double[] DaySeconds)
'  '{
'  '  Double[] WH = New Double[DaySeconds.Count()];
'  '  For (int i = 1; i <= DaySeconds.Count() - 1; i++)
'  '  {
'  '    WH[i] = Convert.ToInt32(1 / (DaySeconds[i]- DaySeconds[i-1]));
'  '  }
'  '  DictionaryCanali.Add("LogHz", WH.ToList());
'  '  Return Convert.ToInt32(WH.Average());
'  '}

'  'void VerificaCanaleSeaStateIndex(ref Dictionary<string, List<Double>> DictionaryCanali, String HeaderCanaleSeaStateIndex, String HeaderCanalePitchRate, String HeaderCanaleTrimRate, int Hz)
'  '{

'  '  If (!DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex))
'  '  {
'  '    bool HasPR = DictionaryCanali.ContainsKey(HeaderCanalePitchRate);
'  '    bool HasTR = DictionaryCanali.ContainsKey(HeaderCanaleTrimRate);
'  '    Double[] WF = null;
'  '    If (HasPR && HasTR)
'  '    {
'  '      Double[] tPR = DictionaryCanali[HeaderCanalePitchRate].ToArray();
'  '      Double[] tTR = DictionaryCanali[HeaderCanaleTrimRate].ToArray();
'  '      If (tPR.Max() > 0)
'  '      {
'  '        WF = tPR;
'  '      }
'  '      Else { WF = tTR; }
'  '    }
'  '    ElseIf (HasPR)
'  '    {
'  '      WF = DictionaryCanali[HeaderCanalePitchRate].ToArray();
'  '    }
'  '    ElseIf (HasTR)
'  '    {
'  '      WF = DictionaryCanali[HeaderCanaleTrimRate].ToArray();
'  '    }
'  '    Else
'  '    {
'  '      WF = null;
'  '    }
'  '    If (WF!= null)
'  '    {
'  '      //double[] WF = DictionaryCanali[HeaderCanalePitchRate].ToArray();
'  '      Double[] WH = New Double[WF.Count()];
'  '      MediaMobile MM = New MediaMobile(30 * Hz);
'  '      //CodaMobile CMSL = New CodaMobile(60*Hz);
'  '      For (int i = 0; i <= WF.Count() - 1; i++)
'  '      {
'  '        WH[i] = MM.Aggiorna(Math.Abs(WF[i]));
'  '        //WH[i] = CMSL.BazzoMax(Math.Abs(WF[i]));
'  '      }
'  '      DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList());
'  '    }
'  '  }
'  '}


'  'String NomeCanaleStandardizzato(String Header, LogFileSource Source)
'  '  {
'  '    switch (Source)
'  '    {
'  '      Case LogFileSource.Expedition : 
'  '        switch (Header)
'  '        {
'  '          Case "TWS"
'  'Return "Tws";
'  '          Case "TWD"
'  'Return "Twd";
'  '          Case "BSP"
'  'Return "Bs";
'  '          Case "TrimRate"
'  'Return "PitchRate";
'  '          Case "Lat"
'  'Return "LatBow";
'  '          Case "Lon"
'  'Return "LonBow";
'  '          Default
'  '            Return Header;
'  '        }
'  '      Case LogFileSource.FaroGzTxt : 
'  '        switch (Header)
'  '        {
'  '          Case "BS"
'  'Return "Bs";
'  '          Case "TWS"
'  'Return "Tws";
'  '          Case "TWD"
'  'Return "Twd";
'  '          Case "Gyro1_Y"
'  'Return "PitchRate";
'  '          Default
'  '            Return Header;
'  '        }
'  '      Default
'  '        Return Header;
'  '    }
'  '  }

'End Class


'<AddINotifyPropertyChangedInterface>
'Public Class clsExpeditionUtilities

'#Region " Import diagnostics "

'  ''' <summary>
'  ''' Warnings collected during the last import. Populated by ReadExpeditionLog2
'  ''' and ExpeditionToParquet. Cleared at the start of every read.
'  ''' </summary>
'  Public Shared ReadOnly Property ImportWarnings As New List(Of String)

'  Private Shared MaxImportWarnings As Integer = 100

'  Private Shared Sub AggiungiWarning(testo As String)
'    If ImportWarnings.Count < MaxImportWarnings Then
'      ImportWarnings.Add(testo)
'    ElseIf ImportWarnings.Count = MaxImportWarnings Then
'      ImportWarnings.Add("... further warnings suppressed.")
'    End If
'  End Sub

'  Public Shared Function ImportWarningsText() As String
'    If ImportWarnings.Count = 0 Then Return ""
'    Return String.Join(Environment.NewLine, ImportWarnings)
'  End Function

'#End Region

'#Region " Timestamp normalisation "

'  Public Enum eFormatoTimestamp
'    Sconosciuto = 0
'    OleDate = 1
'    UnixSeconds = 2
'    UnixMilliseconds = 3
'    FileTime = 4
'    NetTicks = 5
'  End Enum

'  ''' <summary>
'  ''' Converts a raw Utc token into an OLE Automation date, auto-detecting the source
'  ''' format by magnitude. Expedition switched from OLE date (log v2) to Windows
'  ''' FILETIME (log v3, from v12.9) and the change can occur in the middle of a file
'  ''' when several logging sessions are concatenated.
'  ''' Returns False if the value cannot be interpreted as a plausible date.
'  ''' </summary>
'  Public Shared Function NormalizzaTimestampUtc(Valore As Double,
'                                                ByRef OleDate As Double,
'                                                ByRef Formato As eFormatoTimestamp) As Boolean
'    OleDate = Double.NaN
'    Formato = eFormatoTimestamp.Sconosciuto

'    If Double.IsNaN(Valore) OrElse Double.IsInfinity(Valore) OrElse Valore <= 0 Then Return False

'    Dim dt As DateTime

'    Try
'      If Valore < 100000.0 Then
'        ' OLE Automation date. Around 46267 for September 2026.
'        Formato = eFormatoTimestamp.OleDate
'        dt = DateTime.FromOADate(Valore)

'      ElseIf Valore >= 1000000000.0 AndAlso Valore < 100000000000.0 Then
'        ' Unix seconds.
'        Formato = eFormatoTimestamp.UnixSeconds
'        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Valore)

'      ElseIf Valore >= 1000000000000.0 AndAlso Valore < 1.0E+15 Then
'        ' Unix milliseconds.
'        Formato = eFormatoTimestamp.UnixMilliseconds
'        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Valore)

'      ElseIf Valore >= 1.0E+15 AndAlso Valore < 5.0E+17 Then
'        ' Windows FILETIME: 100 ns units since 1601-01-01. Expedition log v3.
'        Formato = eFormatoTimestamp.FileTime
'        dt = DateTime.FromFileTimeUtc(CLng(Valore))

'      ElseIf Valore >= 5.0E+17 AndAlso Valore <= 3.15E+18 Then
'        ' .NET ticks: 100 ns units since 0001-01-01.
'        Formato = eFormatoTimestamp.NetTicks
'        dt = New DateTime(CLng(Valore), DateTimeKind.Utc)

'      Else
'        Return False
'      End If
'    Catch
'      Formato = eFormatoTimestamp.Sconosciuto
'      Return False
'    End Try

'    ' Sanity window: anything outside is garbage, not a format we failed to guess.
'    If dt < New DateTime(1990, 1, 1) OrElse dt > New DateTime(2100, 1, 1) Then
'      Formato = eFormatoTimestamp.Sconosciuto
'      Return False
'    End If

'    OleDate = dt.ToOADate()
'    Return True
'  End Function

'  ''' <summary>
'  ''' Rewrites the Utc channel in place so that every sample is an OLE Automation date,
'  ''' regardless of the format each logging session used. Samples that cannot be
'  ''' interpreted are set to NaN. Applied to both the sparse and the rectangular format
'  ''' so that the rest of the pipeline only ever sees OLE dates.
'  ''' </summary>
'  Public Shared Sub NormalizzaCanaleUtc(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)))
'    Dim chiave As String = Nothing
'    For Each k In DictionaryCanali.Keys
'      If String.Equals(k, "Utc", StringComparison.OrdinalIgnoreCase) Then
'        chiave = k
'        Exit For
'      End If
'    Next
'    If chiave Is Nothing Then Exit Sub

'    Dim valori = DictionaryCanali(chiave)
'    Dim formatiVisti As New HashSet(Of eFormatoTimestamp)
'    Dim convertiti As Integer = 0
'    Dim invalidi As Integer = 0

'    For i As Integer = 0 To valori.Count - 1
'      Dim v As Double = valori(i)
'      If Double.IsNaN(v) OrElse v = 0 Then Continue For

'      Dim ole As Double
'      Dim fmt As eFormatoTimestamp
'      If NormalizzaTimestampUtc(v, ole, fmt) Then
'        formatiVisti.Add(fmt)
'        If fmt <> eFormatoTimestamp.OleDate Then
'          valori(i) = ole
'          convertiti += 1
'        End If
'      Else
'        valori(i) = Double.NaN
'        invalidi += 1
'      End If
'    Next

'    If formatiVisti.Count > 1 Then
'      AggiungiWarning("Utc channel contains mixed time formats: " &
'                      String.Join(", ", formatiVisti.Select(Function(f) f.ToString())) &
'                      ". " & convertiti.ToString() & " samples converted to OLE date.")
'    ElseIf convertiti > 0 Then
'      AggiungiWarning("Utc channel converted from " &
'                      formatiVisti.First().ToString() & " to OLE date (" &
'                      convertiti.ToString() & " samples).")
'    End If

'    If invalidi > 0 Then
'      AggiungiWarning(invalidi.ToString() & " Utc samples were unreadable and set to NaN.")
'    End If
'  End Sub



'  ''' <summary>
'  ''' Expedition log v3 writes time-to-mark channels in 100 ns units instead of the
'  ''' fractional days used by v2. Rescale them so the rest of the application keeps
'  ''' working in days. Detected by magnitude: a countdown of more than ~10 days is
'  ''' not a countdown.
'  ''' </summary>
'  Public Shared Sub NormalizzaCanaliTempo(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)))
'    Const TicksPerDay As Double = 864000000000.0
'    Dim canali As String() = {"TmToGun", "TmToLn", "RchTmToLn", "TmToPort", "TmToStbd", "TmToLay"}

'    For Each nome In canali
'      If Not DictionaryCanali.ContainsKey(nome) Then Continue For
'      Dim v = DictionaryCanali(nome)
'      Dim massimo As Double = 0
'      For Each x In v
'        If Not Double.IsNaN(x) Then massimo = Math.Max(massimo, Math.Abs(x))
'      Next
'      If massimo > 10.0 Then ' impossible in days: the channel is in 100 ns ticks
'        For i As Integer = 0 To v.Count - 1
'          If Not Double.IsNaN(v(i)) Then v(i) = v(i) / TicksPerDay
'        Next
'        AggiungiWarning("Channel '" & nome & "' rescaled from 100 ns ticks to days.")
'      End If
'    Next
'  End Sub


'#End Region

'#Region " Channel derivation helpers "

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As Double
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Average)
'  End Function

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As String
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Select(Function(x) CDbl(x)).Average)
'  End Function

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of Double)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          'Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of String)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub


'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of Double) = Nothing
'      'Dim headers As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'          Exit For
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'headers = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
'        Else
'          Dim WH(Valori.Count - 1) As Double
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As Double
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(Valori(i)))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of String) = Nothing
'      ' HeadersRate = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) Then
'          If HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'HeadersTrim = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
'        Else
'          Dim WH(Valori.Count - 1) As String
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As String
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(CDbl(Valori(i))))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Function NomeCanaleStandardizzato(Header As String) As String
'    Select Case Header
'      Case "AWA"
'        Return "Awa"
'      Case "AWS"
'        Return "Aws"
'      Case "TWA"
'        Return "Twa"
'      Case "TWS"
'        Return "Tws"
'      Case "HDG"
'        Return "Hdg"
'      Case "COG"
'        Return "Cog"
'      Case "SOG"
'        Return "Sog"
'      Case "TWD"
'        Return "Twd"
'      Case "BSP"
'        Return "Bs"
'      Case "Bsp"
'        Return "Bs"
'      Case "Pitch rate"
'        Return "PitchRate"
'      Case "Heel Rate"
'        Return "HeelRate"
'      Case "Roll Rate"
'        Return "HeelRate"
'      Case "TrimRate"
'        Return "PitchRate"
'      Case "Lat"
'        Return "LatBow"
'      Case "Lon"
'        Return "LonBow"
'      Case Else
'        Return Header
'    End Select
'  End Function

'#End Region

'#Region " Log readers "

'  Class ExpNewFormat
'    Public Import As Boolean = False
'    Public Header As String
'    Public Values As New List(Of Double)
'  End Class

'  Public Shared Function ReadExpeditionLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    Dim inizio As DateTime = Now
'    Dim p As New Microsoft.VisualBasic.FileIO.TextFieldParser(PathLogFile)
'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(400) As ExpNewFormat
'    For i As Integer = 0 To 400
'      ENF(i) = New ExpNewFormat
'    Next

'    p.SetDelimiters(",")
'    'Dim MappingIdCanali As New Dictionary(Of Integer, String)
'    Do While Not p.EndOfData
'      Dim r As String() = p.ReadFields
'      If (r(0) = "!Boat") Then
'        t = 2
'        Dim r1 = p.ReadFields
'        For c As Integer = 1 To r.Count - 1
'          ENF(r1(c)).Header = r(c)
'        Next

'        Dim r2 = p.ReadFields
'      ElseIf (r(0) = "Boat") Then
'        t = 1
'        For c As Integer = 0 To r.Count - 1
'          DictionaryCanali.Add(r(c), New List(Of Double))
'        Next

'      Else
'        t = -1
'      End If
'      Exit Do
'    Loop


'    Dim ValidBoat As Integer = 0
'    If t = 1 Then ' formato ante 2022
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        For c As Integer = 0 To r.Count - 1
'          Dim v As Double = Double.NaN
'          If Double.TryParse(r(c).ToString, v) Then
'            DictionaryCanali.Last.Value.Add(v)
'          Else
'            DictionaryCanali.Last.Value.Add(Double.NaN)
'          End If
'        Next
'      Loop
'      p.Close()
'    ElseIf t = 2 Then
'      Dim nr As Integer = 0 ' serve per rimpire le righe precedenti dei canali aggiunti a file creato
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        Dim boat As Integer
'        If Integer.TryParse(r(0), boat) Then
'          If (boat = ValidBoat) Then
'            For Each ch As ExpNewFormat In ENF
'              ch.Values.Add(Double.NaN)
'            Next
'            Dim v As Double = Double.NaN
'            If Double.TryParse(r(1), v) Then
'              ENF(0).Import = True ' marca come da importare il canale se c'e' almeno un numero
'              ENF(0).Values(nr) = v
'            End If
'            For c As Integer = 2 To r.Count - 1 Step 2
'              Dim id As Integer
'              If Integer.TryParse(r(c), id) Then
'                v = Double.NaN
'                If Double.TryParse(r(c + 1), v) Then
'                  ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                  ENF(id).Values(nr) = v
'                End If
'              End If
'            Next
'            nr += 1
'          ElseIf r(0) = "!Boat" Then
'            Dim r1 = p.ReadFields ' riga con gli id
'            For c As Integer = 1 To r.Count - 1
'              ENF(r1(c)).Header = r(c)
'            Next
'            Dim r2 = p.ReadFields ' riga con la versione
'          End If
'        End If


'      Loop
'      For Each ch As ExpNewFormat In ENF
'        If ch.Import Then
'          DictionaryCanali.Add(ch.Header, ch.Values)
'        End If
'      Next
'      p.Close()
'    End If

'    NormalizzaCanaleUtc(DictionaryCanali)

'    NormalizzaCanaliTempo(DictionaryCanali)

'    Console.WriteLine("ReadExpeditionLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function


'  ''' <summary>
'  ''' Main reader. Handles both the pre-2022 rectangular format and the sparse
'  ''' channel/value format, including files where several logging sessions have been
'  ''' concatenated: header blocks may appear at any point, the channel map may grow
'  ''' between sessions, and the Utc format may change mid-file.
'  ''' </summary>
'  Public Shared Function ReadExpeditionLog2(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    ImportWarnings.Clear()

'    Const MaxChannelId As Integer = 600

'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(MaxChannelId) As ExpNewFormat
'    For i As Integer = 0 To MaxChannelId
'      ENF(i) = New ExpNewFormat
'    Next

'    ' Names line ("!Boat,Utc,BSP,...") and indices line ("!boat,0,1,2,...") are two
'    ' separate lines that must be paired positionally. Keep the names line pending
'    ' until its indices line arrives, so that an orphan "!boat" block does not
'    ' corrupt the map already built.
'    Dim PendingNames As String() = Nothing
'    Dim HeaderBlocks As Integer = 0
'    Dim NuoviCanaliDaHeaderSuccessivi As Integer = 0

'    Using sr As New StreamReader(PathLogFile)
'      Dim Line As String = Nothing

'      ' --- format detection on the first meaningful line -------------------------
'      Do
'        Line = sr.ReadLine
'        If Line Is Nothing Then Exit Do
'        If Line.Trim = "" Then Continue Do

'        Dim r As String() = Line.Split(","c)

'        If r(0) = "!Boat" Then
'          ' Peek the next line: sparse format if it is the indices line.
'          Dim Line1 As String = sr.ReadLine
'          Dim r1 As String() = If(Line1 Is Nothing, New String() {""}, Line1.Split(","c))
'          If r1(0) = "!boat" Then
'            t = 2
'            HeaderBlocks += 1
'            NuoviCanaliDaHeaderSuccessivi += ApplicaMappaCanali(ENF, r, r1, MaxChannelId)
'            Dim r2 = sr.ReadLine ' version line, e.g. "!v12.9.2"
'          Else
'            ' "!Boat" header without an indices line: rectangular layout.
'            t = 1
'            For c As Integer = 0 To r.Count - 1
'              Dim nome As String = r(c).TrimStart("!"c)
'              If Not DictionaryCanali.ContainsKey(nome) Then DictionaryCanali.Add(nome, New List(Of Double))
'            Next
'          End If
'        ElseIf r(0) = "Boat" OrElse r(0) = "UTC" Then
'          t = 1
'          For c As Integer = 0 To r.Count - 1
'            If Not DictionaryCanali.ContainsKey(r(c)) Then DictionaryCanali.Add(r(c), New List(Of Double))
'          Next
'        Else
'          t = -1
'        End If
'        Exit Do
'      Loop While Not Line Is Nothing

'      If t = -1 Then
'        AggiungiWarning("Unrecognised log header: the file does not look like an Expedition log.")
'        Return DictionaryCanali
'      End If

'      Dim ValidBoat As Integer = 0

'      ' --- rectangular format ----------------------------------------------------
'      If t = 1 Then
'        Do
'          Line = sr.ReadLine
'          If Line Is Nothing Then Exit Do
'          If Line.Trim = "" Then Continue Do
'          If Line.StartsWith("!") Then Continue Do ' repeated header inside the file

'          Dim r As String() = Line.Split(","c)
'          For c As Integer = 0 To DictionaryCanali.Count - 1
'            Dim v As Double = Double.NaN
'            If (c > r.Count() - 1) Then
'              DictionaryCanali.Values(c).Add(Double.NaN)
'            Else
'              If CheckDouble(r(c), v) Then
'                DictionaryCanali.Values(c).Add(v)
'              Else
'                DictionaryCanali.Values(c).Add(Double.NaN)
'              End If
'            End If
'          Next
'        Loop While Not Line Is Nothing

'        ' --- sparse format -------------------------------------------------------
'      ElseIf t = 2 Then
'        Dim nr As Integer = 0 ' index of the row being filled, used to back-fill channels appearing later
'        Dim RigheScartate As Integer = 0

'        Do
'          Line = sr.ReadLine
'          If Line Is Nothing Then Exit Do
'          If Line.Trim = "" Then Continue Do

'          ' Header lines are handled BEFORE any attempt to parse a boat number.
'          ' In the previous version this branch sat inside the Integer.TryParse test
'          ' and was therefore unreachable, so mid-file header blocks were dropped
'          ' together with every channel they introduced.
'          If Line(0) = "!"c Then
'            Dim rh As String() = Line.Split(","c)
'            If rh(0) = "!Boat" Then
'              PendingNames = rh
'              HeaderBlocks += 1
'            ElseIf rh(0) = "!boat" Then
'              If PendingNames IsNot Nothing Then
'                NuoviCanaliDaHeaderSuccessivi += ApplicaMappaCanali(ENF, PendingNames, rh, MaxChannelId)
'                PendingNames = Nothing
'              Else
'                ' Orphan indices line (seen when a session restarts without re-emitting
'                ' the names line). Keep the existing map rather than losing it.
'                AggiungiWarning("Header block with indices but no names line: existing channel map kept.")
'              End If
'            End If
'            Continue Do
'          End If

'          Dim r As String() = Line.Split(","c)
'          Dim boat As Integer
'          If Not Integer.TryParse(r(0), boat) Then
'            RigheScartate += 1
'            Continue Do
'          End If
'          If boat <> ValidBoat Then Continue Do
'          If r.Count < 2 Then
'            RigheScartate += 1
'            Continue Do
'          End If

'          For Each ch As ExpNewFormat In ENF
'            ch.Values.Add(Double.NaN)
'          Next

'          Dim v As Double = Double.NaN
'          If CheckDouble(r(1), v) Then
'            ENF(0).Import = True ' Utc
'            ENF(0).Values(nr) = v
'          End If

'          ' Remaining tokens are id/value pairs. An odd count means a truncated line
'          ' (power loss mid-write); keep whatever parsed cleanly.
'          Dim ultimoIndice As Integer = r.Count - 2
'          For c As Integer = 2 To ultimoIndice Step 2
'            Dim id As Integer
'            If Not Integer.TryParse(r(c), id) Then Continue For
'            If id < 0 OrElse id > MaxChannelId Then
'              AggiungiWarning("Channel id out of range ignored: " & id.ToString())
'              Continue For
'            End If
'            v = Double.NaN
'            If CheckDouble(r(c + 1), v) Then
'              ENF(id).Import = True
'              ENF(id).Values(nr) = v
'            End If
'          Next

'          nr += 1
'        Loop While Not Line Is Nothing

'        If RigheScartate > 0 Then
'          AggiungiWarning(RigheScartate.ToString() & " malformed data rows skipped.")
'        End If

'        For Each ch As ExpNewFormat In ENF
'          If ch.Import Then
'            If ch.Header Is Nothing Then
'              AggiungiWarning("Channel with data but no name in any header block: dropped.")
'            ElseIf DictionaryCanali.ContainsKey(ch.Header) Then
'              AggiungiWarning("Duplicate channel name '" & ch.Header & "': second occurrence dropped.")
'            Else
'              DictionaryCanali.Add(ch.Header, ch.Values)
'            End If
'          End If
'        Next
'      End If
'    End Using

'    If HeaderBlocks > 1 Then
'      AggiungiWarning(HeaderBlocks.ToString() & " header blocks found: the file contains concatenated logging sessions.")
'    End If
'    If NuoviCanaliDaHeaderSuccessivi > 0 Then
'      AggiungiWarning(NuoviCanaliDaHeaderSuccessivi.ToString() & " channels were introduced by later header blocks.")
'    End If

'    ' Single point of truth for time: everything downstream sees OLE dates only.
'    NormalizzaCanaleUtc(DictionaryCanali)

'    Return DictionaryCanali
'  End Function

'  ''' <summary>
'  ''' Merges one header block into the channel map. Names already assigned are kept:
'  ''' a later session must be able to ADD channels without silently redefining the
'  ''' ones already carrying data. Returns the number of channels newly named.
'  ''' </summary>
'  Private Shared Function ApplicaMappaCanali(ENF As ExpNewFormat(),
'                                             RigaNomi As String(),
'                                             RigaIndici As String(),
'                                             MaxChannelId As Integer) As Integer
'    Dim n As Integer = Math.Min(RigaNomi.Count, RigaIndici.Count)
'    If RigaNomi.Count <> RigaIndici.Count Then
'      AggiungiWarning("Header block mismatch: " & RigaNomi.Count.ToString() & " names vs " &
'                      RigaIndici.Count.ToString() & " indices. Using the first " & n.ToString() & ".")
'    End If

'    Dim nuovi As Integer = 0
'    ' Element 0 is the "!Boat"/"!boat" label itself.
'    For c As Integer = 1 To n - 1
'      Dim id As Integer
'      If Not Integer.TryParse(RigaIndici(c).Trim, id) Then Continue For
'      If id < 0 OrElse id > MaxChannelId Then
'        AggiungiWarning("Header channel id out of range ignored: " & RigaIndici(c))
'        Continue For
'      End If
'      Dim nome As String = RigaNomi(c).Trim
'      If nome = "" Then Continue For

'      If ENF(id).Header Is Nothing Then
'        ENF(id).Header = nome
'        nuovi += 1
'      ElseIf Not String.Equals(ENF(id).Header, nome, StringComparison.Ordinal) Then
'        AggiungiWarning("Channel " & id.ToString() & " redefined: '" & ENF(id).Header &
'                        "' -> '" & nome & "'. Keeping the first definition.")
'      End If
'    Next
'    Return nuovi
'  End Function

'  Public Shared Function CheckDouble(ValueString As String, ByRef Value As Double) As Boolean
'    Dim valid As Boolean = False
'    Value = Double.NaN
'    If ValueString Is Nothing Then Return False
'    Select Case ValueString.Trim
'      Case ""
'      Case "nan"
'      Case "-nan"
'      Case "nan(ind)"
'      Case "-nan(ind)"
'      Case "-"
'      Case "."
'      Case Else
'        valid = Double.TryParse(ValueString, Value)
'    End Select
'    If Not valid Then Value = Double.NaN
'    Return valid
'  End Function


'  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))
'    Dim inizio As DateTime = Now
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
'    c.MissingFieldFound = Nothing
'    Dim fi As New System.IO.FileInfo(PathLogFile)
'    If fi.Extension = ".gz" Then
'      Stop
'      ' va gestito il formato faro compresso
'    End If
'    c.Delimiter = SeparatoreCampi(PathLogFile)
'    Using reader As New StreamReader(PathLogFile)
'      Using csv As New CsvReader(reader, c)
'        Using dr As New CsvDataReader(csv)
'          Dim dt As New System.Data.DataTable
'          dt.Load(dr)
'          If Is2022expFormat(dt.Columns(0).ColumnName) Then
'            ' formato 2022
'            Dim MappingIdCanali As New Dictionary(Of Integer, String)
'            DictionaryCanali.Add(dt.Columns(0).ColumnName, New List(Of Double))
'            MappingIdCanali.Add(-1, dt.Columns(0).ColumnName)
'            For col As Integer = 1 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              MappingIdCanali.Add(dt.Rows(0)(col), dt.Columns(col).ColumnName)
'            Next
'            For row As Integer = 0 To dt.Rows.Count - 1
'              If IsNumeric(dt.Rows(row)(0).ToString) Then
'                For Each Canale In DictionaryCanali
'                  Canale.Value.Add(Double.NaN)
'                Next
'                For col As Integer = 0 To dt.Columns.Count - 1 Step 2
'                  If Not IsDBNull(dt.Rows(row)(col)) Then
'                    Try
'                      Dim Id As Integer = dt.Rows(row)(col)
'                      Dim v As Double = Double.NaN
'                      If Double.TryParse(dt.Rows(row)(col + 1).ToString, v) Then
'                        Dim Canale = DictionaryCanali(MappingIdCanali(Id))
'                        Canale(Canale.Count - 1) = v
'                      End If
'                    Catch ex As Exception

'                    End Try
'                  End If

'                Next
'              End If
'            Next
'          Else
'            ' formato ante 2022
'            For col As Integer = 0 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              For row As Integer = 0 To dt.Rows.Count - 1
'                Dim v As Double = Double.NaN
'                If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
'                  DictionaryCanali.Last.Value.Add(v)
'                Else
'                  DictionaryCanali.Last.Value.Add(Double.NaN)
'                End If
'              Next
'            Next
'          End If
'        End Using
'      End Using
'    End Using

'    NormalizzaCanaleUtc(DictionaryCanali)

'    Console.WriteLine("LeggiFileLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function

'  Private Shared Function Is2022expFormat(FirstChannelName As String) As Boolean
'    Return FirstChannelName = "!Boat"
'  End Function


'  Private Shared Function SeparatoreCampi(PathFile As String)
'    Using sr As New StreamReader(PathFile)
'      Dim Line As String = Nothing
'      Do
'        Line = sr.ReadLine
'        If Not Line Is Nothing Then
'          If Not Line.Trim = "" Then
'            If Line.Contains(vbTab) Then Return vbTab
'            If Line.Contains(",") Then Return ","
'          End If
'        End If
'      Loop While Not Line Is Nothing
'      Return ","
'    End Using

'  End Function

'#End Region

'#Region " Import entry points "

'  Public Shared Function SelezionaFileExpeditionAndMakeParquet() As String
'    Dim n As String = ""
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder
'    Dim pf As String = ObjFiles.SelezionaFile(PathExpDataFolder, "", "", "", n)
'    If Not System.IO.File.Exists(pf) Then Return ""
'    Dim fi As New System.IO.FileInfo(pf)
'    PathExpDataFolder = fi.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()

'    Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(pf)

'    If d Is Nothing OrElse d.Count = 0 Then
'      MsgBox("Import failed: no channels could be read from the log file." & vbCrLf & vbCrLf &
'             ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
'      Return ""
'    End If

'    Dim risultato As String = ExpeditionToParquet(pf, PathExpDataFolder, d, True, True)

'    If risultato = "" Then
'      MsgBox("Import failed." & vbCrLf & vbCrLf & ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
'    ElseIf ImportWarnings.Count > 0 Then
'      ' The file was imported but something in it was irregular: worth telling the user.
'      MsgBox("Import completed with warnings:" & vbCrLf & vbCrLf & ImportWarningsText(),
'             MsgBoxStyle.Information, "Import")
'    End If

'    Return risultato
'  End Function


'  Public Shared Function SelezionaFilesExpeditionAndMakeParquet() As String
'    Dim n As New List(Of String)
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

'    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathExpDataFolder, "", "", "", n)
'    If pf Is Nothing Then Return ""
'    Dim ft As New System.IO.FileInfo(pf.First)
'    PathExpDataFolder = ft.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()

'    Console.WriteLine(pf.Count)
'    Dim Falliti As New List(Of String)
'    For Each fl In pf
'      Dim fi As New System.IO.FileInfo(fl)
'      ' Use the same reader as the single-file path so that both routes benefit from
'      ' the header and timestamp handling.
'      Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(fl)
'      Console.WriteLine(fl)
'      If d Is Nothing OrElse d.Count = 0 OrElse ExpeditionToParquet(fl, PathExpDataFolder, d, True, False) = "" Then
'        Falliti.Add(fi.Name)
'      End If
'    Next
'    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
'    If Falliti.Count > 0 Then
'      MsgBox("Done, but these files could not be imported:" & vbCrLf & String.Join(vbCrLf, Falliti),
'             MsgBoxStyle.Exclamation, "Import")
'    Else
'      MsgBox("Done!")
'    End If
'    Return ""
'  End Function




'  Public Shared Function ExpeditionToParquet(SourcePathFile As String, StorageFolder As String, DictionaryCanali As Dictionary(Of String, List(Of Double)), ImportaSoloInMovimento As Boolean, OpenFolder As Boolean) As String
'    Try

'      Dim IdInizioImportazione As Integer = -1
'      Dim IdFineImportazione As Integer = -1
'      Dim Samples As Integer

'      Dim FolderDate As String = ""
'      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

'      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
'      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
'      Dim ds As New List(Of Double)
'      Dim DataDouble As New List(Of Double)

'      Dim CanaleUtc = DictionaryCanali.Where(Function(x) x.Key.ToLower = "utc").FirstOrDefault()
'      Dim CanaleLat = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault()
'      Dim CanaleLon = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault()

'      ' Original code tested "Where(...) Is Nothing", which is never true for a LINQ
'      ' query, so the branch was always taken and a missing Lat/Lon threw inside the
'      ' blanket Catch below, producing a silent failure.
'      If CanaleUtc.Value IsNot Nothing AndAlso CanaleLat.Value IsNot Nothing AndAlso CanaleLon.Value IsNot Nothing Then
'        Dim lats = CanaleLat.Value.ToArray
'        Dim lngs = CanaleLon.Value.ToArray
'        Dim latsValide = lats.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
'        Dim lngsValide = lngs.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray

'        If latsValide.Count = 0 OrElse lngsValide.Count = 0 Then
'          AggiungiWarning("No valid Lat/Lon samples: time zone cannot be resolved, UTC used as local time.")
'          FolderDate = DateTime.Today.ToString("yyyyMMdd")
'        Else
'          Dim lat As Double = latsValide.Average
'          Dim lng As Double = lngsValide.Average
'          Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
'          Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
'          Dim utc = CanaleUtc.Value.ToArray
'          Dim dt As New List(Of DateTime)
'          Dim validdt As New List(Of Double)
'          For Each ut In utc
'            If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
'              Dim t As DateTime = DateTime.FromOADate(ut)
'              Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
'              dt.Add(convertedTime.LocalDateTime)
'              DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
'              validdt.Add(DataDouble.Last)
'            Else
'              dt.Add(Nothing)
'              DataDouble.Add(Double.NaN)
'            End If
'          Next

'          If validdt.Count = 0 Then
'            AggiungiWarning("Utc channel contains no usable samples.")
'            Return ""
'          End If

'          Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
'          FolderDate = day.ToString("yyyyMMdd")
'          ds.Clear()
'          For Each d In dt
'            If d = Nothing Then
'              ds.Add(Double.NaN)
'            Else
'              If d < day Then
'                ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
'              Else
'                ds.Add(d.TimeOfDay.TotalSeconds)
'              End If
'            End If
'          Next
'        End If
'      Else
'        AggiungiWarning("Utc, Lat or Lon channel missing: local time not computed.")
'        FolderDate = DateTime.Today.ToString("yyyyMMdd")
'      End If

'      If ImportaSoloInMovimento Then
'        Dim bs As Double() = Nothing
'        Dim bsok As Boolean = True
'        If DictionaryCanali.ContainsKey("Bs") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bs").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("Bsp") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("BSP") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        Else
'          bsok = False
'          IdInizioImportazione = 0
'          IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'        End If
'        If bsok Then
'          For c As Integer = 0 To bs.Count - 1
'            If bs(c) > 1 Then
'              IdInizioImportazione = c
'              Exit For
'            End If
'          Next
'          For c As Integer = bs.Count - 1 To 0 Step -1
'            If bs(c) > 1 Then
'              IdFineImportazione = c
'              Exit For
'            End If
'          Next
'        End If
'      Else
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If

'      ' The original condition read "(IdInizioImportazione = -1 OrElse IdFineImportazione)":
'      ' the second operand was a Double evaluated as a Boolean, which only compiled
'      ' because of Option Strict Off and never tested what it meant to test.
'      If (ImportaSoloInMovimento AndAlso (IdInizioImportazione = -1 OrElse IdFineImportazione = -1)) _
'         OrElse IdInizioImportazione = IdFineImportazione Then
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If
'      Samples = IdFineImportazione - IdInizioImportazione

'      If Samples <= 0 Then
'        AggiungiWarning("No samples left after range selection.")
'        Return ""
'      End If

'      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
'      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
'      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

'      Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, ds.ToArray)
'      VerificaCanaleROT(DictionaryCanali, ds, "ROT")
'      Dim HeadersRate As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      Dim HeadersTrim As String() = {"Pitch", "Trim", "Trm"}
'      VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


'      For Each Canale In DictionaryCanali
'        Select Case Canale.Key
'          Case "TrimRate", "HeelRate", "Pitch rate", "Roll Rate", "HeelRT"
'            If HasValidData(Canale.Value) Then
'              DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'            End If

'          Case Else
'            DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'        End Select
'      Next

'      Dim strnfilepath As String = System.IO.Path.Combine(StorageFolder, "strn.txt")

'      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
'      If Not System.IO.Directory.Exists(StorageFolder) Then
'        System.IO.Directory.CreateDirectory(StorageFolder)
'      End If

'      If System.IO.File.Exists(strnfilepath) Then
'        Dim strndest As String = System.IO.Path.Combine(StorageFolder, "strn.txt")
'        If Not System.IO.File.Exists(strndest) Then
'          System.IO.File.Copy(strnfilepath, strndest)
'        End If
'      End If

'      Dim fi As New System.IO.FileInfo(SourcePathFile)
'      Dim FileName As String = ("" + fi.Name).Replace(fi.Extension, ".ppf") ' parquet performance file

'      Dim SW As New System.IO.StreamWriter(System.IO.Path.Combine(StorageFolder, FileName))
'      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
'      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
'      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
'      For Each C In CanaliPq
'        PGR.WriteColumn(C)
'      Next
'      PW.Dispose()
'      SW.Close()
'      SW.Dispose()

'      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

'      Return System.IO.Path.Combine(StorageFolder, FileName)
'    Catch ex As Exception
'      ' Never swallow the reason again: a silent "" here is what made a mid-file
'      ' timestamp format change look like "the import just does not work".
'      AggiungiWarning("ExpeditionToParquet failed: " & ex.Message)
'      Console.WriteLine("ExpeditionToParquet failed: " & ex.ToString)
'      Return ""
'    End Try

'  End Function

'#End Region

'#Region " Misc "

'  Public Shared Function HasValidData(Valori As List(Of Double)) As Boolean
'    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
'  End Function

'  Public Shared Function HasValidData(Valori As List(Of String)) As Boolean
'    Dim a = Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
'    Return a.Count > 0
'  End Function

'#End Region

'End Class


'<AddINotifyPropertyChangedInterface>
'Public Class clsExpeditionUtilities

'#Region " Import diagnostics "

'  ''' <summary>
'  ''' Warnings collected during the last import. Populated by ReadExpeditionLog2
'  ''' and ExpeditionToParquet. Cleared at the start of every read.
'  ''' </summary>
'  Public Shared ReadOnly Property ImportWarnings As New List(Of String)

'  Private Shared MaxImportWarnings As Integer = 100

'  Private Shared Sub AggiungiWarning(testo As String)
'    If ImportWarnings.Count < MaxImportWarnings Then
'      ImportWarnings.Add(testo)
'    ElseIf ImportWarnings.Count = MaxImportWarnings Then
'      ImportWarnings.Add("... further warnings suppressed.")
'    End If
'  End Sub

'  Public Shared Function ImportWarningsText() As String
'    If ImportWarnings.Count = 0 Then Return ""
'    Return String.Join(Environment.NewLine, ImportWarnings)
'  End Function

'#End Region

'#Region " Timestamp normalisation "

'  Public Enum eFormatoTimestamp
'    Sconosciuto = 0
'    OleDate = 1
'    UnixSeconds = 2
'    UnixMilliseconds = 3
'    FileTime = 4
'    NetTicks = 5
'  End Enum

'  ''' <summary>
'  ''' Converts a raw Utc token into an OLE Automation date, auto-detecting the source
'  ''' format by magnitude. Expedition switched from OLE date (log v2) to Windows
'  ''' FILETIME (log v3, from v12.9) and the change can occur in the middle of a file
'  ''' when several logging sessions are concatenated.
'  ''' Returns False if the value cannot be interpreted as a plausible date.
'  ''' </summary>
'  Public Shared Function NormalizzaTimestampUtc(Valore As Double,
'                                                ByRef OleDate As Double,
'                                                ByRef Formato As eFormatoTimestamp) As Boolean
'    OleDate = Double.NaN
'    Formato = eFormatoTimestamp.Sconosciuto

'    If Double.IsNaN(Valore) OrElse Double.IsInfinity(Valore) OrElse Valore <= 0 Then Return False

'    Dim dt As DateTime

'    Try
'      If Valore < 100000.0 Then
'        ' OLE Automation date. Around 46267 for September 2026.
'        Formato = eFormatoTimestamp.OleDate
'        dt = DateTime.FromOADate(Valore)

'      ElseIf Valore >= 1000000000.0 AndAlso Valore < 100000000000.0 Then
'        ' Unix seconds.
'        Formato = eFormatoTimestamp.UnixSeconds
'        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Valore)

'      ElseIf Valore >= 1000000000000.0 AndAlso Valore < 1.0E+15 Then
'        ' Unix milliseconds.
'        Formato = eFormatoTimestamp.UnixMilliseconds
'        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Valore)

'      ElseIf Valore >= 1.0E+15 AndAlso Valore < 5.0E+17 Then
'        ' Windows FILETIME: 100 ns units since 1601-01-01. Expedition log v3.
'        Formato = eFormatoTimestamp.FileTime
'        dt = DateTime.FromFileTimeUtc(CLng(Valore))

'      ElseIf Valore >= 5.0E+17 AndAlso Valore <= 3.15E+18 Then
'        ' .NET ticks: 100 ns units since 0001-01-01.
'        Formato = eFormatoTimestamp.NetTicks
'        dt = New DateTime(CLng(Valore), DateTimeKind.Utc)

'      Else
'        Return False
'      End If
'    Catch
'      Formato = eFormatoTimestamp.Sconosciuto
'      Return False
'    End Try

'    ' Sanity window: anything outside is garbage, not a format we failed to guess.
'    If dt < New DateTime(1990, 1, 1) OrElse dt > New DateTime(2100, 1, 1) Then
'      Formato = eFormatoTimestamp.Sconosciuto
'      Return False
'    End If

'    OleDate = dt.ToOADate()
'    Return True
'  End Function

'  ''' <summary>
'  ''' Rewrites the Utc channel in place so that every sample is an OLE Automation date,
'  ''' regardless of the format each logging session used. Samples that cannot be
'  ''' interpreted are set to NaN. Applied to both the sparse and the rectangular format
'  ''' so that the rest of the pipeline only ever sees OLE dates.
'  ''' </summary>
'  Public Shared Sub NormalizzaCanaleUtc(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)))
'    Dim chiave As String = Nothing
'    For Each k In DictionaryCanali.Keys
'      If String.Equals(k, "Utc", StringComparison.OrdinalIgnoreCase) Then
'        chiave = k
'        Exit For
'      End If
'    Next
'    If chiave Is Nothing Then Exit Sub

'    Dim valori = DictionaryCanali(chiave)
'    Dim formatiVisti As New HashSet(Of eFormatoTimestamp)
'    Dim convertiti As Integer = 0
'    Dim invalidi As Integer = 0

'    For i As Integer = 0 To valori.Count - 1
'      Dim v As Double = valori(i)
'      If Double.IsNaN(v) OrElse v = 0 Then Continue For

'      Dim ole As Double
'      Dim fmt As eFormatoTimestamp
'      If NormalizzaTimestampUtc(v, ole, fmt) Then
'        formatiVisti.Add(fmt)
'        If fmt <> eFormatoTimestamp.OleDate Then
'          valori(i) = ole
'          convertiti += 1
'        End If
'      Else
'        valori(i) = Double.NaN
'        invalidi += 1
'      End If
'    Next

'    If formatiVisti.Count > 1 Then
'      AggiungiWarning("Utc channel contains mixed time formats: " &
'                      String.Join(", ", formatiVisti.Select(Function(f) f.ToString())) &
'                      ". " & convertiti.ToString() & " samples converted to OLE date.")
'    ElseIf convertiti > 0 Then
'      AggiungiWarning("Utc channel converted from " &
'                      formatiVisti.First().ToString() & " to OLE date (" &
'                      convertiti.ToString() & " samples).")
'    End If

'    If invalidi > 0 Then
'      AggiungiWarning(invalidi.ToString() & " Utc samples were unreadable and set to NaN.")
'    End If
'  End Sub

'#End Region

'#Region " Channel derivation helpers "

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As Double
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Average)
'  End Function

'  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), DaySeconds As Double()) As Integer
'    Dim WH(DaySeconds.Count - 1) As String
'    For i As Integer = 1 To DaySeconds.Count - 1
'      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
'      If d > 0 Then
'        WH(i) = Convert.ToInt32(1 / d)
'      End If
'    Next
'    WH(0) = WH(1)
'    DictionaryCanali.Add("LogHz", WH.ToList)
'    Return Convert.ToInt32(WH.Select(Function(x) CDbl(x)).Average)
'  End Function

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of Double)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          'Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub

'  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), TimeDbl As List(Of Double), RotChannelName As String)
'    If DictionaryCanali.ContainsKey(RotChannelName) Then
'      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
'        Dim hdg As New List(Of String)
'        If DictionaryCanali.TryGetValue("HDG", hdg) Then
'          Stop
'        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
'          Stop
'        Else
'          hdg = Nothing
'        End If
'        If Not hdg Is Nothing Then
'          Dim t1 As Double = TimeDbl(0)
'          Dim v1 As Double = hdg(0)
'          For i As Integer = 1 To hdg.Count - 1
'            Dim t2 As Double = TimeDbl(i)
'            Dim v2 As Double = hdg(i)
'            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
'            t1 = t2
'            v1 = v2
'          Next

'        End If
'      End If
'    Else

'    End If


'  End Sub


'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of Double) = Nothing
'      'Dim headers As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'          Exit For
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'headers = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
'        Else
'          Dim WH(Valori.Count - 1) As Double
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As Double
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(Valori(i)))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
'    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
'      Dim Valori As List(Of String) = Nothing
'      ' HeadersRate = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate"}
'      For Each hdr In HeadersRate
'        If DictionaryCanali.TryGetValue(hdr, Valori) Then
'          If HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        End If
'      Next
'      If Valori Is Nothing Then
'        ' non esiste un canale ptch rate o simile lo crea dal pitch
'        'HeadersTrim = {"Pitch", "Trim", "Trm"}
'        For Each hdr In HeadersTrim
'          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
'            Exit For
'          End If
'        Next
'        If Valori Is Nothing Then
'          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
'        Else
'          Dim WH(Valori.Count - 1) As String
'          Dim MM As New clsMediaMobile(30 * Hz, False)
'          Dim v1 As Double = Valori(0)
'          For i As Integer = 1 To Valori.Count - 1
'            Dim v2 As Double = Valori(i)
'            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
'          Next
'          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'        End If
'      Else
'        Dim WH(Valori.Count - 1) As String
'        Dim MM As New clsMediaMobile(30 * Hz, False)
'        For i As Integer = 0 To Valori.Count - 1
'          WH(i) = MM.SetAndGet(Math.Abs(CDbl(Valori(i))))
'        Next
'        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
'      End If
'    End If
'  End Sub

'  Public Shared Function NomeCanaleStandardizzato(Header As String) As String
'    Select Case Header
'      Case "AWA"
'        Return "Awa"
'      Case "AWS"
'        Return "Aws"
'      Case "TWA"
'        Return "Twa"
'      Case "TWS"
'        Return "Tws"
'      Case "HDG"
'        Return "Hdg"
'      Case "COG"
'        Return "Cog"
'      Case "SOG"
'        Return "Sog"
'      Case "TWD"
'        Return "Twd"
'      Case "BSP"
'        Return "Bs"
'      Case "Bsp"
'        Return "Bs"
'      Case "Pitch rate"
'        Return "PitchRate"
'      Case "Heel Rate"
'        Return "HeelRate"
'      Case "Roll Rate"
'        Return "HeelRate"
'      Case "TrimRate"
'        Return "PitchRate"
'      Case "Lat"
'        Return "LatBow"
'      Case "Lon"
'        Return "LonBow"
'      Case Else
'        Return Header
'    End Select
'  End Function

'#End Region

'#Region " Log readers "

'  Class ExpNewFormat
'    Public Import As Boolean = False
'    Public Header As String
'    Public Values As New List(Of Double)
'  End Class

'  Public Shared Function ReadExpeditionLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    Dim inizio As DateTime = Now
'    Dim p As New Microsoft.VisualBasic.FileIO.TextFieldParser(PathLogFile)
'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(400) As ExpNewFormat
'    For i As Integer = 0 To 400
'      ENF(i) = New ExpNewFormat
'    Next

'    p.SetDelimiters(",")
'    'Dim MappingIdCanali As New Dictionary(Of Integer, String)
'    Do While Not p.EndOfData
'      Dim r As String() = p.ReadFields
'      If (r(0) = "!Boat") Then
'        t = 2
'        Dim r1 = p.ReadFields
'        For c As Integer = 1 To r.Count - 1
'          ENF(r1(c)).Header = r(c)
'        Next

'        Dim r2 = p.ReadFields
'      ElseIf (r(0) = "Boat") Then
'        t = 1
'        For c As Integer = 0 To r.Count - 1
'          DictionaryCanali.Add(r(c), New List(Of Double))
'        Next

'      Else
'        t = -1
'      End If
'      Exit Do
'    Loop


'    Dim ValidBoat As Integer = 0
'    If t = 1 Then ' formato ante 2022
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        For c As Integer = 0 To r.Count - 1
'          Dim v As Double = Double.NaN
'          If Double.TryParse(r(c).ToString, v) Then
'            DictionaryCanali.Last.Value.Add(v)
'          Else
'            DictionaryCanali.Last.Value.Add(Double.NaN)
'          End If
'        Next
'      Loop
'      p.Close()
'    ElseIf t = 2 Then
'      Dim nr As Integer = 0 ' serve per rimpire le righe precedenti dei canali aggiunti a file creato
'      Do While Not p.EndOfData
'        Dim r As String() = p.ReadFields
'        Dim boat As Integer
'        If Integer.TryParse(r(0), boat) Then
'          If (boat = ValidBoat) Then
'            For Each ch As ExpNewFormat In ENF
'              ch.Values.Add(Double.NaN)
'            Next
'            Dim v As Double = Double.NaN
'            If Double.TryParse(r(1), v) Then
'              ENF(0).Import = True ' marca come da importare il canale se c'e' almeno un numero
'              ENF(0).Values(nr) = v
'            End If
'            For c As Integer = 2 To r.Count - 1 Step 2
'              Dim id As Integer
'              If Integer.TryParse(r(c), id) Then
'                v = Double.NaN
'                If Double.TryParse(r(c + 1), v) Then
'                  ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
'                  ENF(id).Values(nr) = v
'                End If
'              End If
'            Next
'            nr += 1
'          ElseIf r(0) = "!Boat" Then
'            Dim r1 = p.ReadFields ' riga con gli id
'            For c As Integer = 1 To r.Count - 1
'              ENF(r1(c)).Header = r(c)
'            Next
'            Dim r2 = p.ReadFields ' riga con la versione
'          End If
'        End If


'      Loop
'      For Each ch As ExpNewFormat In ENF
'        If ch.Import Then
'          DictionaryCanali.Add(ch.Header, ch.Values)
'        End If
'      Next
'      p.Close()
'    End If

'    NormalizzaCanaleUtc(DictionaryCanali)

'    Console.WriteLine("ReadExpeditionLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function


'  ''' <summary>
'  ''' Main reader. Handles both the pre-2022 rectangular format and the sparse
'  ''' channel/value format, including files where several logging sessions have been
'  ''' concatenated: header blocks may appear at any point, the channel map may grow
'  ''' between sessions, and the Utc format may change mid-file.
'  ''' </summary>
'  Public Shared Function ReadExpeditionLog2(PathLogFile As String) As Dictionary(Of String, List(Of Double))

'    ImportWarnings.Clear()

'    Const MaxChannelId As Integer = 600

'    Dim t As Integer = -1
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim ENF(MaxChannelId) As ExpNewFormat
'    For i As Integer = 0 To MaxChannelId
'      ENF(i) = New ExpNewFormat
'    Next

'    ' Names line ("!Boat,Utc,BSP,...") and indices line ("!boat,0,1,2,...") are two
'    ' separate lines that must be paired positionally. Keep the names line pending
'    ' until its indices line arrives, so that an orphan "!boat" block does not
'    ' corrupt the map already built.
'    Dim PendingNames As String() = Nothing
'    Dim HeaderBlocks As Integer = 0
'    Dim NuoviCanaliDaHeaderSuccessivi As Integer = 0

'    Using sr As New StreamReader(PathLogFile)
'      Dim Line As String = Nothing

'      ' --- format detection on the first meaningful line -------------------------
'      Do
'        Line = sr.ReadLine
'        If Line Is Nothing Then Exit Do
'        If Line.Trim = "" Then Continue Do

'        Dim r As String() = Line.Split(","c)

'        If r(0) = "!Boat" Then
'          ' Peek the next line: sparse format if it is the indices line.
'          Dim Line1 As String = sr.ReadLine
'          Dim r1 As String() = If(Line1 Is Nothing, New String() {""}, Line1.Split(","c))
'          If r1(0) = "!boat" Then
'            t = 2
'            HeaderBlocks += 1
'            NuoviCanaliDaHeaderSuccessivi += ApplicaMappaCanali(ENF, r, r1, MaxChannelId)
'            Dim r2 = sr.ReadLine ' version line, e.g. "!v12.9.2"
'          Else
'            ' "!Boat" header without an indices line: rectangular layout.
'            t = 1
'            For c As Integer = 0 To r.Count - 1
'              Dim nome As String = r(c).TrimStart("!"c)
'              If Not DictionaryCanali.ContainsKey(nome) Then DictionaryCanali.Add(nome, New List(Of Double))
'            Next
'          End If
'        ElseIf r(0) = "Boat" OrElse r(0) = "UTC" Then
'          t = 1
'          For c As Integer = 0 To r.Count - 1
'            If Not DictionaryCanali.ContainsKey(r(c)) Then DictionaryCanali.Add(r(c), New List(Of Double))
'          Next
'        Else
'          t = -1
'        End If
'        Exit Do
'      Loop While Not Line Is Nothing

'      If t = -1 Then
'        AggiungiWarning("Unrecognised log header: the file does not look like an Expedition log.")
'        Return DictionaryCanali
'      End If

'      Dim ValidBoat As Integer = 0

'      ' --- rectangular format ----------------------------------------------------
'      If t = 1 Then
'        Do
'          Line = sr.ReadLine
'          If Line Is Nothing Then Exit Do
'          If Line.Trim = "" Then Continue Do
'          If Line.StartsWith("!") Then Continue Do ' repeated header inside the file

'          Dim r As String() = Line.Split(","c)
'          For c As Integer = 0 To DictionaryCanali.Count - 1
'            Dim v As Double = Double.NaN
'            If (c > r.Count() - 1) Then
'              DictionaryCanali.Values(c).Add(Double.NaN)
'            Else
'              If CheckDouble(r(c), v) Then
'                DictionaryCanali.Values(c).Add(v)
'              Else
'                DictionaryCanali.Values(c).Add(Double.NaN)
'              End If
'            End If
'          Next
'        Loop While Not Line Is Nothing

'        ' --- sparse format -------------------------------------------------------
'      ElseIf t = 2 Then
'        Dim nr As Integer = 0 ' index of the row being filled, used to back-fill channels appearing later
'        Dim RigheScartate As Integer = 0

'        Do
'          Line = sr.ReadLine
'          If Line Is Nothing Then Exit Do
'          If Line.Trim = "" Then Continue Do

'          ' Header lines are handled BEFORE any attempt to parse a boat number.
'          ' In the previous version this branch sat inside the Integer.TryParse test
'          ' and was therefore unreachable, so mid-file header blocks were dropped
'          ' together with every channel they introduced.
'          If Line(0) = "!"c Then
'            Dim rh As String() = Line.Split(","c)
'            If rh(0) = "!Boat" Then
'              PendingNames = rh
'              HeaderBlocks += 1
'            ElseIf rh(0) = "!boat" Then
'              If PendingNames IsNot Nothing Then
'                NuoviCanaliDaHeaderSuccessivi += ApplicaMappaCanali(ENF, PendingNames, rh, MaxChannelId)
'                PendingNames = Nothing
'              Else
'                ' Orphan indices line (seen when a session restarts without re-emitting
'                ' the names line). Keep the existing map rather than losing it.
'                AggiungiWarning("Header block with indices but no names line: existing channel map kept.")
'              End If
'            End If
'            Continue Do
'          End If

'          Dim r As String() = Line.Split(","c)
'          Dim boat As Integer
'          If Not Integer.TryParse(r(0), boat) Then
'            RigheScartate += 1
'            Continue Do
'          End If
'          If boat <> ValidBoat Then Continue Do
'          If r.Count < 2 Then
'            RigheScartate += 1
'            Continue Do
'          End If

'          For Each ch As ExpNewFormat In ENF
'            ch.Values.Add(Double.NaN)
'          Next

'          Dim v As Double = Double.NaN
'          If CheckDouble(r(1), v) Then
'            ENF(0).Import = True ' Utc
'            ENF(0).Values(nr) = v
'          End If

'          ' Remaining tokens are id/value pairs. An odd count means a truncated line
'          ' (power loss mid-write); keep whatever parsed cleanly.
'          Dim ultimoIndice As Integer = r.Count - 2
'          For c As Integer = 2 To ultimoIndice Step 2
'            Dim id As Integer
'            If Not Integer.TryParse(r(c), id) Then Continue For
'            If id < 0 OrElse id > MaxChannelId Then
'              AggiungiWarning("Channel id out of range ignored: " & id.ToString())
'              Continue For
'            End If
'            v = Double.NaN
'            If CheckDouble(r(c + 1), v) Then
'              ENF(id).Import = True
'              ENF(id).Values(nr) = v
'            End If
'          Next

'          nr += 1
'        Loop While Not Line Is Nothing

'        If RigheScartate > 0 Then
'          AggiungiWarning(RigheScartate.ToString() & " malformed data rows skipped.")
'        End If

'        For Each ch As ExpNewFormat In ENF
'          If ch.Import Then
'            If ch.Header Is Nothing Then
'              AggiungiWarning("Channel with data but no name in any header block: dropped.")
'            ElseIf DictionaryCanali.ContainsKey(ch.Header) Then
'              AggiungiWarning("Duplicate channel name '" & ch.Header & "': second occurrence dropped.")
'            Else
'              DictionaryCanali.Add(ch.Header, ch.Values)
'            End If
'          End If
'        Next
'      End If
'    End Using

'    If HeaderBlocks > 1 Then
'      AggiungiWarning(HeaderBlocks.ToString() & " header blocks found: the file contains concatenated logging sessions.")
'    End If
'    If NuoviCanaliDaHeaderSuccessivi > 0 Then
'      AggiungiWarning(NuoviCanaliDaHeaderSuccessivi.ToString() & " channels were introduced by later header blocks.")
'    End If

'    ' Single point of truth for time: everything downstream sees OLE dates only.
'    NormalizzaCanaleUtc(DictionaryCanali)

'    Return DictionaryCanali
'  End Function

'  ''' <summary>
'  ''' Merges one header block into the channel map. Names already assigned are kept:
'  ''' a later session must be able to ADD channels without silently redefining the
'  ''' ones already carrying data. Returns the number of channels newly named.
'  ''' </summary>
'  Private Shared Function ApplicaMappaCanali(ENF As ExpNewFormat(),
'                                             RigaNomi As String(),
'                                             RigaIndici As String(),
'                                             MaxChannelId As Integer) As Integer
'    Dim n As Integer = Math.Min(RigaNomi.Count, RigaIndici.Count)
'    If RigaNomi.Count <> RigaIndici.Count Then
'      AggiungiWarning("Header block mismatch: " & RigaNomi.Count.ToString() & " names vs " &
'                      RigaIndici.Count.ToString() & " indices. Using the first " & n.ToString() & ".")
'    End If

'    Dim nuovi As Integer = 0
'    ' Element 0 is the "!Boat"/"!boat" label itself.
'    For c As Integer = 1 To n - 1
'      Dim id As Integer
'      If Not Integer.TryParse(RigaIndici(c).Trim, id) Then Continue For
'      If id < 0 OrElse id > MaxChannelId Then
'        AggiungiWarning("Header channel id out of range ignored: " & RigaIndici(c))
'        Continue For
'      End If
'      Dim nome As String = RigaNomi(c).Trim
'      If nome = "" Then Continue For

'      If ENF(id).Header Is Nothing Then
'        ENF(id).Header = nome
'        nuovi += 1
'      ElseIf Not String.Equals(ENF(id).Header, nome, StringComparison.Ordinal) Then
'        AggiungiWarning("Channel " & id.ToString() & " redefined: '" & ENF(id).Header &
'                        "' -> '" & nome & "'. Keeping the first definition.")
'      End If
'    Next
'    Return nuovi
'  End Function

'  Public Shared Function CheckDouble(ValueString As String, ByRef Value As Double) As Boolean
'    Dim valid As Boolean = False
'    Value = Double.NaN
'    If ValueString Is Nothing Then Return False
'    Select Case ValueString.Trim
'      Case ""
'      Case "nan"
'      Case "-nan"
'      Case "nan(ind)"
'      Case "-nan(ind)"
'      Case "-"
'      Case "."
'      Case Else
'        valid = Double.TryParse(ValueString, Value)
'    End Select
'    If Not valid Then Value = Double.NaN
'    Return valid
'  End Function


'  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))
'    Dim inizio As DateTime = Now
'    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
'    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
'    c.MissingFieldFound = Nothing
'    Dim fi As New System.IO.FileInfo(PathLogFile)
'    If fi.Extension = ".gz" Then
'      Stop
'      ' va gestito il formato faro compresso
'    End If
'    c.Delimiter = SeparatoreCampi(PathLogFile)
'    Using reader As New StreamReader(PathLogFile)
'      Using csv As New CsvReader(reader, c)
'        Using dr As New CsvDataReader(csv)
'          Dim dt As New System.Data.DataTable
'          dt.Load(dr)
'          If Is2022expFormat(dt.Columns(0).ColumnName) Then
'            ' formato 2022
'            Dim MappingIdCanali As New Dictionary(Of Integer, String)
'            DictionaryCanali.Add(dt.Columns(0).ColumnName, New List(Of Double))
'            MappingIdCanali.Add(-1, dt.Columns(0).ColumnName)
'            For col As Integer = 1 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              MappingIdCanali.Add(dt.Rows(0)(col), dt.Columns(col).ColumnName)
'            Next
'            For row As Integer = 0 To dt.Rows.Count - 1
'              If IsNumeric(dt.Rows(row)(0).ToString) Then
'                For Each Canale In DictionaryCanali
'                  Canale.Value.Add(Double.NaN)
'                Next
'                For col As Integer = 0 To dt.Columns.Count - 1 Step 2
'                  If Not IsDBNull(dt.Rows(row)(col)) Then
'                    Try
'                      Dim Id As Integer = dt.Rows(row)(col)
'                      Dim v As Double = Double.NaN
'                      If Double.TryParse(dt.Rows(row)(col + 1).ToString, v) Then
'                        Dim Canale = DictionaryCanali(MappingIdCanali(Id))
'                        Canale(Canale.Count - 1) = v
'                      End If
'                    Catch ex As Exception

'                    End Try
'                  End If

'                Next
'              End If
'            Next
'          Else
'            ' formato ante 2022
'            For col As Integer = 0 To dt.Columns.Count - 1
'              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
'              For row As Integer = 0 To dt.Rows.Count - 1
'                Dim v As Double = Double.NaN
'                If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
'                  DictionaryCanali.Last.Value.Add(v)
'                Else
'                  DictionaryCanali.Last.Value.Add(Double.NaN)
'                End If
'              Next
'            Next
'          End If
'        End Using
'      End Using
'    End Using

'    NormalizzaCanaleUtc(DictionaryCanali)

'    Console.WriteLine("LeggiFileLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
'    Return DictionaryCanali
'  End Function

'  Private Shared Function Is2022expFormat(FirstChannelName As String) As Boolean
'    Return FirstChannelName = "!Boat"
'  End Function


'  Private Shared Function SeparatoreCampi(PathFile As String)
'    Using sr As New StreamReader(PathFile)
'      Dim Line As String = Nothing
'      Do
'        Line = sr.ReadLine
'        If Not Line Is Nothing Then
'          If Not Line.Trim = "" Then
'            If Line.Contains(vbTab) Then Return vbTab
'            If Line.Contains(",") Then Return ","
'          End If
'        End If
'      Loop While Not Line Is Nothing
'      Return ","
'    End Using

'  End Function

'#End Region

'#Region " Import entry points "

'  Public Shared Function SelezionaFileExpeditionAndMakeParquet() As String
'    Dim n As String = ""
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder
'    Dim pf As String = ObjFiles.SelezionaFile(PathExpDataFolder, "", "", "", n)
'    If Not System.IO.File.Exists(pf) Then Return ""
'    Dim fi As New System.IO.FileInfo(pf)
'    PathExpDataFolder = fi.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()

'    Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(pf)

'    If d Is Nothing OrElse d.Count = 0 Then
'      MsgBox("Import failed: no channels could be read from the log file." & vbCrLf & vbCrLf &
'             ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
'      Return ""
'    End If

'    Dim risultato As String = ExpeditionToParquet(pf, PathExpDataFolder, d, True, True)

'    If risultato = "" Then
'      MsgBox("Import failed." & vbCrLf & vbCrLf & ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
'    ElseIf ImportWarnings.Count > 0 Then
'      ' The file was imported but something in it was irregular: worth telling the user.
'      MsgBox("Import completed with warnings:" & vbCrLf & vbCrLf & ImportWarningsText(),
'             MsgBoxStyle.Information, "Import")
'    End If

'    Return risultato
'  End Function


'  Public Shared Function SelezionaFilesExpeditionAndMakeParquet() As String
'    Dim n As New List(Of String)
'    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

'    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathExpDataFolder, "", "", "", n)
'    If pf Is Nothing Then Return ""
'    Dim ft As New System.IO.FileInfo(pf.First)
'    PathExpDataFolder = ft.Directory.FullName
'    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
'    AppConfig.Salva()

'    Console.WriteLine(pf.Count)
'    Dim Falliti As New List(Of String)
'    For Each fl In pf
'      Dim fi As New System.IO.FileInfo(fl)
'      ' Use the same reader as the single-file path so that both routes benefit from
'      ' the header and timestamp handling.
'      Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(fl)
'      Console.WriteLine(fl)
'      If d Is Nothing OrElse d.Count = 0 OrElse ExpeditionToParquet(fl, PathExpDataFolder, d, True, False) = "" Then
'        Falliti.Add(fi.Name)
'      End If
'    Next
'    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
'    If Falliti.Count > 0 Then
'      MsgBox("Done, but these files could not be imported:" & vbCrLf & String.Join(vbCrLf, Falliti),
'             MsgBoxStyle.Exclamation, "Import")
'    Else
'      MsgBox("Done!")
'    End If
'    Return ""
'  End Function




'  Public Shared Function ExpeditionToParquet(SourcePathFile As String, StorageFolder As String, DictionaryCanali As Dictionary(Of String, List(Of Double)), ImportaSoloInMovimento As Boolean, OpenFolder As Boolean) As String
'    Try

'      Dim IdInizioImportazione As Integer = -1
'      Dim IdFineImportazione As Integer = -1
'      Dim Samples As Integer

'      Dim FolderDate As String = ""
'      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

'      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
'      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
'      Dim ds As New List(Of Double)
'      Dim DataDouble As New List(Of Double)

'      Dim CanaleUtc = DictionaryCanali.Where(Function(x) x.Key.ToLower = "utc").FirstOrDefault()
'      Dim CanaleLat = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault()
'      Dim CanaleLon = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault()

'      ' Original code tested "Where(...) Is Nothing", which is never true for a LINQ
'      ' query, so the branch was always taken and a missing Lat/Lon threw inside the
'      ' blanket Catch below, producing a silent failure.
'      If CanaleUtc.Value IsNot Nothing AndAlso CanaleLat.Value IsNot Nothing AndAlso CanaleLon.Value IsNot Nothing Then
'        Dim lats = CanaleLat.Value.ToArray
'        Dim lngs = CanaleLon.Value.ToArray
'        Dim latsValide = lats.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
'        Dim lngsValide = lngs.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray

'        If latsValide.Count = 0 OrElse lngsValide.Count = 0 Then
'          AggiungiWarning("No valid Lat/Lon samples: time zone cannot be resolved, UTC used as local time.")
'          FolderDate = DateTime.Today.ToString("yyyyMMdd")
'        Else
'          Dim lat As Double = latsValide.Average
'          Dim lng As Double = lngsValide.Average
'          Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
'          Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
'          Dim utc = CanaleUtc.Value.ToArray
'          Dim dt As New List(Of DateTime)
'          Dim validdt As New List(Of Double)
'          For Each ut In utc
'            If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
'              Dim t As DateTime = DateTime.FromOADate(ut)
'              Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
'              dt.Add(convertedTime.LocalDateTime)
'              DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
'              validdt.Add(DataDouble.Last)
'            Else
'              dt.Add(Nothing)
'              DataDouble.Add(Double.NaN)
'            End If
'          Next

'          If validdt.Count = 0 Then
'            AggiungiWarning("Utc channel contains no usable samples.")
'            Return ""
'          End If

'          Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
'          FolderDate = day.ToString("yyyyMMdd")
'          ds.Clear()
'          For Each d In dt
'            If d = Nothing Then
'              ds.Add(Double.NaN)
'            Else
'              If d < day Then
'                ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
'              Else
'                ds.Add(d.TimeOfDay.TotalSeconds)
'              End If
'            End If
'          Next
'        End If
'      Else
'        AggiungiWarning("Utc, Lat or Lon channel missing: local time not computed.")
'        FolderDate = DateTime.Today.ToString("yyyyMMdd")
'      End If

'      If ImportaSoloInMovimento Then
'        Dim bs As Double() = Nothing
'        Dim bsok As Boolean = True
'        If DictionaryCanali.ContainsKey("Bs") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bs").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("Bsp") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        ElseIf DictionaryCanali.ContainsKey("BSP") Then
'          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
'        Else
'          bsok = False
'          IdInizioImportazione = 0
'          IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'        End If
'        If bsok Then
'          For c As Integer = 0 To bs.Count - 1
'            If bs(c) > 1 Then
'              IdInizioImportazione = c
'              Exit For
'            End If
'          Next
'          For c As Integer = bs.Count - 1 To 0 Step -1
'            If bs(c) > 1 Then
'              IdFineImportazione = c
'              Exit For
'            End If
'          Next
'        End If
'      Else
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If

'      ' The original condition read "(IdInizioImportazione = -1 OrElse IdFineImportazione)":
'      ' the second operand was a Double evaluated as a Boolean, which only compiled
'      ' because of Option Strict Off and never tested what it meant to test.
'      If (ImportaSoloInMovimento AndAlso (IdInizioImportazione = -1 OrElse IdFineImportazione = -1)) _
'         OrElse IdInizioImportazione = IdFineImportazione Then
'        IdInizioImportazione = 0
'        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
'      End If
'      Samples = IdFineImportazione - IdInizioImportazione

'      If Samples <= 0 Then
'        AggiungiWarning("No samples left after range selection.")
'        Return ""
'      End If

'      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
'      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
'      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

'      Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, ds.ToArray)
'      VerificaCanaleROT(DictionaryCanali, ds, "ROT")
'      Dim HeadersRate As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
'      Dim HeadersTrim As String() = {"Pitch", "Trim", "Trm"}
'      VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


'      For Each Canale In DictionaryCanali
'        Select Case Canale.Key
'          Case "TrimRate", "HeelRate", "Pitch rate", "Roll Rate", "HeelRT"
'            If HasValidData(Canale.Value) Then
'              DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'            End If

'          Case Else
'            DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
'            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
'        End Select
'      Next

'      Dim strnfilepath As String = System.IO.Path.Combine(StorageFolder, "strn.txt")

'      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
'      If Not System.IO.Directory.Exists(StorageFolder) Then
'        System.IO.Directory.CreateDirectory(StorageFolder)
'      End If

'      If System.IO.File.Exists(strnfilepath) Then
'        Dim strndest As String = System.IO.Path.Combine(StorageFolder, "strn.txt")
'        If Not System.IO.File.Exists(strndest) Then
'          System.IO.File.Copy(strnfilepath, strndest)
'        End If
'      End If

'      Dim fi As New System.IO.FileInfo(SourcePathFile)
'      Dim FileName As String = ("" + fi.Name).Replace(fi.Extension, ".ppf") ' parquet performance file

'      Dim SW As New System.IO.StreamWriter(System.IO.Path.Combine(StorageFolder, FileName))
'      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
'      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
'      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
'      For Each C In CanaliPq
'        PGR.WriteColumn(C)
'      Next
'      PW.Dispose()
'      SW.Close()
'      SW.Dispose()

'      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

'      Return System.IO.Path.Combine(StorageFolder, FileName)
'    Catch ex As Exception
'      ' Never swallow the reason again: a silent "" here is what made a mid-file
'      ' timestamp format change look like "the import just does not work".
'      AggiungiWarning("ExpeditionToParquet failed: " & ex.Message)
'      Console.WriteLine("ExpeditionToParquet failed: " & ex.ToString)
'      Return ""
'    End Try

'  End Function

'#End Region

'#Region " Misc "

'  Public Shared Function HasValidData(Valori As List(Of Double)) As Boolean
'    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
'  End Function

'  Public Shared Function HasValidData(Valori As List(Of String)) As Boolean
'    Dim a = Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
'    Return a.Count > 0
'  End Function

'#End Region

'End Class


<AddINotifyPropertyChangedInterface>
Public Class clsExpeditionUtilities

#Region " Import diagnostics "

  ''' <summary>
  ''' Warnings collected during the last import. Populated by ReadExpeditionLog2
  ''' and ExpeditionToParquet. Cleared at the start of every read.
  ''' </summary>
  Public Shared ReadOnly Property ImportWarnings As New List(Of String)

  Private Shared MaxImportWarnings As Integer = 100

  Private Shared Sub AggiungiWarning(testo As String)
    If ImportWarnings.Count < MaxImportWarnings Then
      ImportWarnings.Add(testo)
    ElseIf ImportWarnings.Count = MaxImportWarnings Then
      ImportWarnings.Add("... further warnings suppressed.")
    End If
  End Sub

  Public Shared Function ImportWarningsText() As String
    If ImportWarnings.Count = 0 Then Return ""
    Return String.Join(Environment.NewLine, ImportWarnings)
  End Function

#End Region

#Region " Timestamp normalisation "

  Public Enum eFormatoTimestamp
    Sconosciuto = 0
    OleDate = 1
    UnixSeconds = 2
    UnixMilliseconds = 3
    FileTime = 4
    NetTicks = 5
  End Enum

  ''' <summary>
  ''' Converts a raw Utc token into an OLE Automation date, auto-detecting the source
  ''' format by magnitude. Expedition switched from OLE date (log v2) to Windows
  ''' FILETIME (log v3, from v12.9) and the change can occur in the middle of a file
  ''' when several logging sessions are concatenated.
  ''' Returns False if the value cannot be interpreted as a plausible date.
  ''' </summary>
  Public Shared Function NormalizzaTimestampUtc(Valore As Double,
                                                ByRef OleDate As Double,
                                                ByRef Formato As eFormatoTimestamp) As Boolean
    OleDate = Double.NaN
    Formato = eFormatoTimestamp.Sconosciuto

    If Double.IsNaN(Valore) OrElse Double.IsInfinity(Valore) OrElse Valore <= 0 Then Return False

    Dim dt As DateTime

    Try
      If Valore < 100000.0 Then
        ' OLE Automation date. Around 46267 for September 2026.
        Formato = eFormatoTimestamp.OleDate
        dt = DateTime.FromOADate(Valore)

      ElseIf Valore >= 1000000000.0 AndAlso Valore < 100000000000.0 Then
        ' Unix seconds.
        Formato = eFormatoTimestamp.UnixSeconds
        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Valore)

      ElseIf Valore >= 1000000000000.0 AndAlso Valore < 1.0E+15 Then
        ' Unix milliseconds.
        Formato = eFormatoTimestamp.UnixMilliseconds
        dt = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Valore)

      ElseIf Valore >= 1.0E+15 AndAlso Valore < 5.0E+17 Then
        ' Windows FILETIME: 100 ns units since 1601-01-01. Expedition log v3.
        Formato = eFormatoTimestamp.FileTime
        dt = DateTime.FromFileTimeUtc(CLng(Valore))

      ElseIf Valore >= 5.0E+17 AndAlso Valore <= 3.15E+18 Then
        ' .NET ticks: 100 ns units since 0001-01-01.
        Formato = eFormatoTimestamp.NetTicks
        dt = New DateTime(CLng(Valore), DateTimeKind.Utc)

      Else
        Return False
      End If
    Catch
      Formato = eFormatoTimestamp.Sconosciuto
      Return False
    End Try

    ' Sanity window: anything outside is garbage, not a format we failed to guess.
    If dt < New DateTime(1990, 1, 1) OrElse dt > New DateTime(2100, 1, 1) Then
      Formato = eFormatoTimestamp.Sconosciuto
      Return False
    End If

    OleDate = dt.ToOADate()
    Return True
  End Function

  ''' <summary>
  ''' Rewrites the Utc channel in place so that every sample is an OLE Automation date,
  ''' regardless of the format each logging session used. Samples that cannot be
  ''' interpreted are set to NaN. Applied to both the sparse and the rectangular format
  ''' so that the rest of the pipeline only ever sees OLE dates.
  ''' </summary>
  Public Shared Sub NormalizzaCanaleUtc(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)))
    Dim chiave As String = Nothing
    For Each k In DictionaryCanali.Keys
      If String.Equals(k, "Utc", StringComparison.OrdinalIgnoreCase) Then
        chiave = k
        Exit For
      End If
    Next
    If chiave Is Nothing Then Exit Sub

    Dim valori = DictionaryCanali(chiave)
    Dim formatiVisti As New HashSet(Of eFormatoTimestamp)
    Dim convertiti As Integer = 0
    Dim invalidi As Integer = 0

    For i As Integer = 0 To valori.Count - 1
      Dim v As Double = valori(i)
      If Double.IsNaN(v) OrElse v = 0 Then Continue For

      Dim ole As Double
      Dim fmt As eFormatoTimestamp
      If NormalizzaTimestampUtc(v, ole, fmt) Then
        formatiVisti.Add(fmt)
        If fmt <> eFormatoTimestamp.OleDate Then
          valori(i) = ole
          convertiti += 1
        End If
      Else
        valori(i) = Double.NaN
        invalidi += 1
      End If
    Next

    If formatiVisti.Count > 1 Then
      AggiungiWarning("Utc channel contains mixed time formats: " &
                      String.Join(", ", formatiVisti.Select(Function(f) f.ToString())) &
                      ". " & convertiti.ToString() & " samples converted to OLE date.")
    ElseIf convertiti > 0 Then
      AggiungiWarning("Utc channel converted from " &
                      formatiVisti.First().ToString() & " to OLE date (" &
                      convertiti.ToString() & " samples).")
    End If

    If invalidi > 0 Then
      AggiungiWarning(invalidi.ToString() & " Utc samples were unreadable and set to NaN.")
    End If
  End Sub

#End Region

#Region " Channel scale normalisation "

  ''' <summary>
  ''' How a channel encodes its value in Expedition log v3 compared to v2.
  ''' </summary>
  Public Enum eConvenzioneCanale
    ''' <summary>Duration written in 100 ns ticks; v2 wrote fractional days.</summary>
    DurataTicks = 1
    ''' <summary>OLE Automation date multiplied by the number of ticks in a day.</summary>
    DataOleScalata = 2
    ''' <summary>Windows FILETIME (100 ns since 1601-01-01) to be read as an OLE date.</summary>
    FileTime = 3
  End Enum

  ''' <summary>
  ''' Channels whose scale changed with Expedition log v3, with the conversion each one
  ''' needs. The list is explicit on purpose: several healthy channels (Depth, Drift)
  ''' carry out-of-range sentinel values for "no data", so a rule based on magnitude
  ''' alone would corrupt them. Only channels listed here are ever touched.
  ''' </summary>
  Private Shared ReadOnly ConvenzioniCanali As New Dictionary(Of String, eConvenzioneCanale)(StringComparer.OrdinalIgnoreCase) From {
    {"TmToGun", eConvenzioneCanale.DurataTicks},
    {"TmToLn", eConvenzioneCanale.DurataTicks},
    {"RchTmToLn", eConvenzioneCanale.DurataTicks},
    {"GPS tOffset", eConvenzioneCanale.DataOleScalata},
    {"GPS time", eConvenzioneCanale.FileTime}
  }

  Private Const TicksPerDay As Double = 864000000000.0

  ''' <summary>
  ''' Rescales the channels listed in ConvenzioniCanali so that every sample uses the
  ''' v2 convention (fractional days / OLE dates) that the rest of the application
  ''' expects. Samples already in v2 scale are left untouched, so files mixing two
  ''' logging sessions are handled correctly.
  ''' </summary>
  Public Shared Sub NormalizzaCanaliTempo(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)))
    Const SogliaScala As Double = 1000000.0

    For Each voce In ConvenzioniCanali
      Dim valori As List(Of Double) = Nothing
      If Not DictionaryCanali.TryGetValue(voce.Key, valori) Then Continue For
      If valori Is Nothing Then Continue For

      Dim convertiti As Integer = 0
      Dim falliti As Integer = 0

      For i As Integer = 0 To valori.Count - 1
        Dim x As Double = valori(i)
        If Double.IsNaN(x) OrElse Math.Abs(x) <= SogliaScala Then Continue For

        Select Case voce.Value
          Case eConvenzioneCanale.DurataTicks, eConvenzioneCanale.DataOleScalata
            valori(i) = x / TicksPerDay
            convertiti += 1

          Case eConvenzioneCanale.FileTime
            If x > 0 AndAlso x < 5.0E+17 Then
              Try
                valori(i) = DateTime.FromFileTimeUtc(CLng(x)).ToOADate()
                convertiti += 1
              Catch
                valori(i) = Double.NaN
                falliti += 1
              End Try
            Else
              valori(i) = Double.NaN
              falliti += 1
            End If
        End Select
      Next

      If convertiti > 0 Then
        AggiungiWarning("Channel '" & voce.Key & "': " & convertiti.ToString() &
                        " samples rescaled from the log v3 convention (" & voce.Value.ToString() & ").")
      End If
      If falliti > 0 Then
        AggiungiWarning("Channel '" & voce.Key & "': " & falliti.ToString() &
                        " samples could not be rescaled and were set to NaN.")
      End If
    Next
  End Sub

#End Region

#Region " Channel derivation helpers "

  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), DaySeconds As Double()) As Integer
    Dim WH(DaySeconds.Count - 1) As Double
    For i As Integer = 1 To DaySeconds.Count - 1
      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
      If d > 0 Then
        WH(i) = Convert.ToInt32(1 / d)
      End If
    Next
    WH(0) = WH(1)
    DictionaryCanali.Add("LogHz", WH.ToList)
    Return Convert.ToInt32(WH.Average)
  End Function

  Public Shared Function VerificaCanaleHz(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), DaySeconds As Double()) As Integer
    Dim WH(DaySeconds.Count - 1) As String
    For i As Integer = 1 To DaySeconds.Count - 1
      Dim d As Double = DaySeconds(i) - DaySeconds(i - 1)
      If d > 0 Then
        WH(i) = Convert.ToInt32(1 / d)
      End If
    Next
    WH(0) = WH(1)
    DictionaryCanali.Add("LogHz", WH.ToList)
    Return Convert.ToInt32(WH.Select(Function(x) CDbl(x)).Average)
  End Function

  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), TimeDbl As List(Of Double), RotChannelName As String)
    If DictionaryCanali.ContainsKey(RotChannelName) Then
      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
        Dim hdg As New List(Of Double)
        If DictionaryCanali.TryGetValue("HDG", hdg) Then
          'Stop
        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
          Stop
        Else
          hdg = Nothing
        End If
        If Not hdg Is Nothing Then
          Dim t1 As Double = TimeDbl(0)
          Dim v1 As Double = hdg(0)
          For i As Integer = 1 To hdg.Count - 1
            Dim t2 As Double = TimeDbl(i)
            Dim v2 As Double = hdg(i)
            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
            t1 = t2
            v1 = v2
          Next

        End If
      End If
    Else

    End If


  End Sub

  Public Shared Sub VerificaCanaleROT(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), TimeDbl As List(Of Double), RotChannelName As String)
    If DictionaryCanali.ContainsKey(RotChannelName) Then
      If Not HasValidData(DictionaryCanali(RotChannelName)) Then
        Dim hdg As New List(Of String)
        If DictionaryCanali.TryGetValue("HDG", hdg) Then
          Stop
        ElseIf DictionaryCanali.TryGetValue("Hdg", hdg) Then
          Stop
        Else
          hdg = Nothing
        End If
        If Not hdg Is Nothing Then
          Dim t1 As Double = TimeDbl(0)
          Dim v1 As Double = hdg(0)
          For i As Integer = 1 To hdg.Count - 1
            Dim t2 As Double = TimeDbl(i)
            Dim v2 As Double = hdg(i)
            DictionaryCanali(RotChannelName)(i) = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(v1, v2) / (t2 - t1)
            t1 = t2
            v1 = v2
          Next

        End If
      End If
    Else

    End If


  End Sub


  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of Double)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
      Dim Valori As List(Of Double) = Nothing
      'Dim headers As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
      For Each hdr In HeadersRate
        If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
          Exit For
        End If
      Next
      If Valori Is Nothing Then
        ' non esiste un canale ptch rate o simile lo crea dal pitch
        'headers = {"Pitch", "Trim", "Trm"}
        For Each hdr In HeadersTrim
          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
            Exit For
          End If
        Next
        If Valori Is Nothing Then
          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
        Else
          Dim WH(Valori.Count - 1) As Double
          Dim MM As New clsMediaMobile(30 * Hz, False)
          Dim v1 As Double = Valori(0)
          For i As Integer = 1 To Valori.Count - 1
            Dim v2 As Double = Valori(i)
            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
          Next
          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
        End If
      Else
        Dim WH(Valori.Count - 1) As Double
        Dim MM As New clsMediaMobile(30 * Hz, False)
        For i As Integer = 0 To Valori.Count - 1
          WH(i) = MM.SetAndGet(Math.Abs(Valori(i)))
        Next
        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
      End If
    End If
  End Sub

  Public Shared Sub VerificaCanaleSeaStateIndex(ByRef DictionaryCanali As Dictionary(Of String, List(Of String)), HeaderCanaleSeaStateIndex As String, Hz As Integer, HeadersRate As String(), HeadersTrim As String())
    If Not DictionaryCanali.ContainsKey(HeaderCanaleSeaStateIndex) Then
      Dim Valori As List(Of String) = Nothing
      ' HeadersRate = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate"}
      For Each hdr In HeadersRate
        If DictionaryCanali.TryGetValue(hdr, Valori) Then
          If HasValidData(DictionaryCanali(hdr)) Then
            Exit For
          End If
        End If
      Next
      If Valori Is Nothing Then
        ' non esiste un canale ptch rate o simile lo crea dal pitch
        'HeadersTrim = {"Pitch", "Trim", "Trm"}
        For Each hdr In HeadersTrim
          If DictionaryCanali.TryGetValue(hdr, Valori) AndAlso HasValidData(DictionaryCanali(hdr)) Then
            Exit For
          End If
        Next
        If Valori Is Nothing Then
          AggiungiWarning("SeaStateIndex not created: no pitch/trim rate channel available.")
        Else
          Dim WH(Valori.Count - 1) As String
          Dim MM As New clsMediaMobile(30 * Hz, False)
          Dim v1 As Double = Valori(0)
          For i As Integer = 1 To Valori.Count - 1
            Dim v2 As Double = Valori(i)
            WH(i) = MM.SetAndGet(Math.Abs(v2 - v1))
          Next
          DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
        End If
      Else
        Dim WH(Valori.Count - 1) As String
        Dim MM As New clsMediaMobile(30 * Hz, False)
        For i As Integer = 0 To Valori.Count - 1
          WH(i) = MM.SetAndGet(Math.Abs(CDbl(Valori(i))))
        Next
        DictionaryCanali.Add(HeaderCanaleSeaStateIndex, WH.ToList)
      End If
    End If
  End Sub

  Public Shared Function NomeCanaleStandardizzato(Header As String) As String
    Select Case Header
      Case "AWA"
        Return "Awa"
      Case "AWS"
        Return "Aws"
      Case "TWA"
        Return "Twa"
      Case "TWS"
        Return "Tws"
      Case "HDG"
        Return "Hdg"
      Case "COG"
        Return "Cog"
      Case "SOG"
        Return "Sog"
      Case "TWD"
        Return "Twd"
      Case "BSP"
        Return "Bs"
      Case "Bsp"
        Return "Bs"
      Case "Pitch rate"
        Return "PitchRate"
      Case "Heel Rate"
        Return "HeelRate"
      Case "Roll Rate"
        Return "HeelRate"
      Case "TrimRate"
        Return "PitchRate"
      Case "Lat"
        Return "LatBow"
      Case "Lon"
        Return "LonBow"
      Case Else
        Return Header
    End Select
  End Function

#End Region

#Region " Log readers "

  Class ExpNewFormat
    Public Import As Boolean = False
    Public Header As String
    Public Values As New List(Of Double)
  End Class

  Public Shared Function ReadExpeditionLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))

    Dim inizio As DateTime = Now
    Dim p As New Microsoft.VisualBasic.FileIO.TextFieldParser(PathLogFile)
    Dim t As Integer = -1
    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
    Dim ENF(400) As ExpNewFormat
    For i As Integer = 0 To 400
      ENF(i) = New ExpNewFormat
    Next

    p.SetDelimiters(",")
    'Dim MappingIdCanali As New Dictionary(Of Integer, String)
    Do While Not p.EndOfData
      Dim r As String() = p.ReadFields
      If (r(0) = "!Boat") Then
        t = 2
        Dim r1 = p.ReadFields
        For c As Integer = 1 To r.Count - 1
          ENF(r1(c)).Header = r(c)
        Next

        Dim r2 = p.ReadFields
      ElseIf (r(0) = "Boat") Then
        t = 1
        For c As Integer = 0 To r.Count - 1
          DictionaryCanali.Add(r(c), New List(Of Double))
        Next

      Else
        t = -1
      End If
      Exit Do
    Loop


    Dim ValidBoat As Integer = 0
    If t = 1 Then ' formato ante 2022
      Do While Not p.EndOfData
        Dim r As String() = p.ReadFields
        For c As Integer = 0 To r.Count - 1
          Dim v As Double = Double.NaN
          If Double.TryParse(r(c).ToString, v) Then
            DictionaryCanali.Last.Value.Add(v)
          Else
            DictionaryCanali.Last.Value.Add(Double.NaN)
          End If
        Next
      Loop
      p.Close()
    ElseIf t = 2 Then
      Dim nr As Integer = 0 ' serve per rimpire le righe precedenti dei canali aggiunti a file creato
      Do While Not p.EndOfData
        Dim r As String() = p.ReadFields
        Dim boat As Integer
        If Integer.TryParse(r(0), boat) Then
          If (boat = ValidBoat) Then
            For Each ch As ExpNewFormat In ENF
              ch.Values.Add(Double.NaN)
            Next
            Dim v As Double = Double.NaN
            If Double.TryParse(r(1), v) Then
              ENF(0).Import = True ' marca come da importare il canale se c'e' almeno un numero
              ENF(0).Values(nr) = v
            End If
            For c As Integer = 2 To r.Count - 1 Step 2
              Dim id As Integer
              If Integer.TryParse(r(c), id) Then
                v = Double.NaN
                If Double.TryParse(r(c + 1), v) Then
                  ENF(id).Import = True ' marca come da importare il canale se c'e' almeno un numero
                  ENF(id).Values(nr) = v
                End If
              End If
            Next
            nr += 1
          ElseIf r(0) = "!Boat" Then
            Dim r1 = p.ReadFields ' riga con gli id
            For c As Integer = 1 To r.Count - 1
              ENF(r1(c)).Header = r(c)
            Next
            Dim r2 = p.ReadFields ' riga con la versione
          End If
        End If


      Loop
      For Each ch As ExpNewFormat In ENF
        If ch.Import Then
          DictionaryCanali.Add(ch.Header, ch.Values)
        End If
      Next
      p.Close()
    End If

    NormalizzaCanaleUtc(DictionaryCanali)
    NormalizzaCanaliTempo(DictionaryCanali)

    Console.WriteLine("ReadExpeditionLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
    Return DictionaryCanali
  End Function


  ''' <summary>
  ''' Main reader. Handles both the pre-2022 rectangular format and the sparse
  ''' channel/value format, including files where several logging sessions have been
  ''' concatenated: header blocks may appear at any point, the channel map may grow
  ''' between sessions, and the Utc format may change mid-file.
  ''' </summary>
  Public Shared Function ReadExpeditionLog2(PathLogFile As String) As Dictionary(Of String, List(Of Double))

    ImportWarnings.Clear()

    Const MaxChannelId As Integer = 600

    Dim t As Integer = -1
    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
    Dim ENF(MaxChannelId) As ExpNewFormat
    For i As Integer = 0 To MaxChannelId
      ENF(i) = New ExpNewFormat
    Next

    ' Names line ("!Boat,Utc,BSP,...") and indices line ("!boat,0,1,2,...") are two
    ' separate lines that must be paired positionally. Keep the names line pending
    ' until its indices line arrives, so that an orphan "!boat" block does not
    ' corrupt the map already built.
    Dim PendingNames As String() = Nothing
    Dim HeaderBlocks As Integer = 0
    Dim NuoviCanaliDaHeaderSuccessivi As Integer = 0

    Using sr As New StreamReader(PathLogFile)
      Dim Line As String = Nothing

      ' --- format detection on the first meaningful line -------------------------
      Do
        Line = sr.ReadLine
        If Line Is Nothing Then Exit Do
        If Line.Trim = "" Then Continue Do

        Dim r As String() = Line.Split(","c)

        If r(0) = "!Boat" Then
          ' Peek the next line: sparse format if it is the indices line.
          Dim Line1 As String = sr.ReadLine
          Dim r1 As String() = If(Line1 Is Nothing, New String() {""}, Line1.Split(","c))
          If r1(0) = "!boat" Then
            t = 2
            HeaderBlocks += 1
            ' The first header block defines the baseline map: it is not a "later"
            ' block, so its channels must not be counted as added by one.
            ApplicaMappaCanali(ENF, r, r1, MaxChannelId)
            Dim r2 = sr.ReadLine ' version line, e.g. "!v12.9.2"
          Else
            ' "!Boat" header without an indices line: rectangular layout.
            t = 1
            For c As Integer = 0 To r.Count - 1
              Dim nome As String = r(c).TrimStart("!"c)
              If Not DictionaryCanali.ContainsKey(nome) Then DictionaryCanali.Add(nome, New List(Of Double))
            Next
          End If
        ElseIf r(0) = "Boat" OrElse r(0) = "UTC" Then
          t = 1
          For c As Integer = 0 To r.Count - 1
            If Not DictionaryCanali.ContainsKey(r(c)) Then DictionaryCanali.Add(r(c), New List(Of Double))
          Next
        Else
          t = -1
        End If
        Exit Do
      Loop While Not Line Is Nothing

      If t = -1 Then
        AggiungiWarning("Unrecognised log header: the file does not look like an Expedition log.")
        Return DictionaryCanali
      End If

      Dim ValidBoat As Integer = 0

      ' --- rectangular format ----------------------------------------------------
      If t = 1 Then
        Do
          Line = sr.ReadLine
          If Line Is Nothing Then Exit Do
          If Line.Trim = "" Then Continue Do
          If Line.StartsWith("!") Then Continue Do ' repeated header inside the file

          Dim r As String() = Line.Split(","c)
          For c As Integer = 0 To DictionaryCanali.Count - 1
            Dim v As Double = Double.NaN
            If (c > r.Count() - 1) Then
              DictionaryCanali.Values(c).Add(Double.NaN)
            Else
              If CheckDouble(r(c), v) Then
                DictionaryCanali.Values(c).Add(v)
              Else
                DictionaryCanali.Values(c).Add(Double.NaN)
              End If
            End If
          Next
        Loop While Not Line Is Nothing

        ' --- sparse format -------------------------------------------------------
      ElseIf t = 2 Then
        Dim nr As Integer = 0 ' index of the row being filled, used to back-fill channels appearing later
        Dim RigheScartate As Integer = 0

        Do
          Line = sr.ReadLine
          If Line Is Nothing Then Exit Do
          If Line.Trim = "" Then Continue Do

          ' Header lines are handled BEFORE any attempt to parse a boat number.
          ' In the previous version this branch sat inside the Integer.TryParse test
          ' and was therefore unreachable, so mid-file header blocks were dropped
          ' together with every channel they introduced.
          If Line(0) = "!"c Then
            Dim rh As String() = Line.Split(","c)
            If rh(0) = "!Boat" Then
              PendingNames = rh
              HeaderBlocks += 1
            ElseIf rh(0) = "!boat" Then
              If PendingNames IsNot Nothing Then
                NuoviCanaliDaHeaderSuccessivi += ApplicaMappaCanali(ENF, PendingNames, rh, MaxChannelId)
                PendingNames = Nothing
              Else
                ' Orphan indices line (seen when a session restarts without re-emitting
                ' the names line). Keep the existing map rather than losing it.
                AggiungiWarning("Header block with indices but no names line: existing channel map kept.")
              End If
            End If
            Continue Do
          End If

          Dim r As String() = Line.Split(","c)
          Dim boat As Integer
          If Not Integer.TryParse(r(0), boat) Then
            RigheScartate += 1
            Continue Do
          End If
          If boat <> ValidBoat Then Continue Do
          If r.Count < 2 Then
            RigheScartate += 1
            Continue Do
          End If

          For Each ch As ExpNewFormat In ENF
            ch.Values.Add(Double.NaN)
          Next

          Dim v As Double = Double.NaN
          If CheckDouble(r(1), v) Then
            ENF(0).Import = True ' Utc
            ENF(0).Values(nr) = v
          End If

          ' Remaining tokens are id/value pairs. An odd count means a truncated line
          ' (power loss mid-write); keep whatever parsed cleanly.
          Dim ultimoIndice As Integer = r.Count - 2
          For c As Integer = 2 To ultimoIndice Step 2
            Dim id As Integer
            If Not Integer.TryParse(r(c), id) Then Continue For
            If id < 0 OrElse id > MaxChannelId Then
              AggiungiWarning("Channel id out of range ignored: " & id.ToString())
              Continue For
            End If
            v = Double.NaN
            If CheckDouble(r(c + 1), v) Then
              ENF(id).Import = True
              ENF(id).Values(nr) = v
            End If
          Next

          nr += 1
        Loop While Not Line Is Nothing

        If RigheScartate > 0 Then
          AggiungiWarning(RigheScartate.ToString() & " malformed data rows skipped.")
        End If

        For Each ch As ExpNewFormat In ENF
          If ch.Import Then
            If ch.Header Is Nothing Then
              AggiungiWarning("Channel with data but no name in any header block: dropped.")
            ElseIf DictionaryCanali.ContainsKey(ch.Header) Then
              AggiungiWarning("Duplicate channel name '" & ch.Header & "': second occurrence dropped.")
            Else
              DictionaryCanali.Add(ch.Header, ch.Values)
            End If
          End If
        Next
      End If
    End Using

    If HeaderBlocks > 1 Then
      AggiungiWarning(HeaderBlocks.ToString() & " header blocks found: the file contains concatenated logging sessions.")
    End If
    If NuoviCanaliDaHeaderSuccessivi > 0 Then
      AggiungiWarning(NuoviCanaliDaHeaderSuccessivi.ToString() & " channels were introduced by later header blocks.")
    End If

    ' Single point of truth for time: everything downstream sees the v2 conventions
    ' only, whatever the logging sessions in the file actually used.
    NormalizzaCanaleUtc(DictionaryCanali)
    NormalizzaCanaliTempo(DictionaryCanali)

    Return DictionaryCanali
  End Function

  ''' <summary>
  ''' Merges one header block into the channel map. Names already assigned are kept:
  ''' a later session must be able to ADD channels without silently redefining the
  ''' ones already carrying data. Returns the number of channels newly named.
  ''' </summary>
  Private Shared Function ApplicaMappaCanali(ENF As ExpNewFormat(),
                                             RigaNomi As String(),
                                             RigaIndici As String(),
                                             MaxChannelId As Integer) As Integer
    Dim n As Integer = Math.Min(RigaNomi.Count, RigaIndici.Count)
    If RigaNomi.Count <> RigaIndici.Count Then
      AggiungiWarning("Header block mismatch: " & RigaNomi.Count.ToString() & " names vs " &
                      RigaIndici.Count.ToString() & " indices. Using the first " & n.ToString() & ".")
    End If

    Dim nuovi As Integer = 0
    ' Element 0 is the "!Boat"/"!boat" label itself.
    For c As Integer = 1 To n - 1
      Dim id As Integer
      If Not Integer.TryParse(RigaIndici(c).Trim, id) Then Continue For
      If id < 0 OrElse id > MaxChannelId Then
        AggiungiWarning("Header channel id out of range ignored: " & RigaIndici(c))
        Continue For
      End If
      Dim nome As String = RigaNomi(c).Trim
      If nome = "" Then Continue For

      If ENF(id).Header Is Nothing Then
        ENF(id).Header = nome
        nuovi += 1
      ElseIf Not String.Equals(ENF(id).Header, nome, StringComparison.Ordinal) Then
        AggiungiWarning("Channel " & id.ToString() & " redefined: '" & ENF(id).Header &
                        "' -> '" & nome & "'. Keeping the first definition.")
      End If
    Next
    Return nuovi
  End Function

  Public Shared Function CheckDouble(ValueString As String, ByRef Value As Double) As Boolean
    Dim valid As Boolean = False
    Value = Double.NaN
    If ValueString Is Nothing Then Return False
    Select Case ValueString.Trim
      Case ""
      Case "nan"
      Case "-nan"
      Case "nan(ind)"
      Case "-nan(ind)"
      Case "-"
      Case "."
      Case Else
        valid = Double.TryParse(ValueString, Value)
    End Select
    If Not valid Then Value = Double.NaN
    Return valid
  End Function


  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))
    Dim inizio As DateTime = Now
    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    c.MissingFieldFound = Nothing
    Dim fi As New System.IO.FileInfo(PathLogFile)
    If fi.Extension = ".gz" Then
      Stop
      ' va gestito il formato faro compresso
    End If
    c.Delimiter = SeparatoreCampi(PathLogFile)
    Using reader As New StreamReader(PathLogFile)
      Using csv As New CsvReader(reader, c)
        Using dr As New CsvDataReader(csv)
          Dim dt As New System.Data.DataTable
          dt.Load(dr)
          If Is2022expFormat(dt.Columns(0).ColumnName) Then
            ' formato 2022
            Dim MappingIdCanali As New Dictionary(Of Integer, String)
            DictionaryCanali.Add(dt.Columns(0).ColumnName, New List(Of Double))
            MappingIdCanali.Add(-1, dt.Columns(0).ColumnName)
            For col As Integer = 1 To dt.Columns.Count - 1
              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
              MappingIdCanali.Add(dt.Rows(0)(col), dt.Columns(col).ColumnName)
            Next
            For row As Integer = 0 To dt.Rows.Count - 1
              If IsNumeric(dt.Rows(row)(0).ToString) Then
                For Each Canale In DictionaryCanali
                  Canale.Value.Add(Double.NaN)
                Next
                For col As Integer = 0 To dt.Columns.Count - 1 Step 2
                  If Not IsDBNull(dt.Rows(row)(col)) Then
                    Try
                      Dim Id As Integer = dt.Rows(row)(col)
                      Dim v As Double = Double.NaN
                      If Double.TryParse(dt.Rows(row)(col + 1).ToString, v) Then
                        Dim Canale = DictionaryCanali(MappingIdCanali(Id))
                        Canale(Canale.Count - 1) = v
                      End If
                    Catch ex As Exception

                    End Try
                  End If

                Next
              End If
            Next
          Else
            ' formato ante 2022
            For col As Integer = 0 To dt.Columns.Count - 1
              DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
              For row As Integer = 0 To dt.Rows.Count - 1
                Dim v As Double = Double.NaN
                If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
                  DictionaryCanali.Last.Value.Add(v)
                Else
                  DictionaryCanali.Last.Value.Add(Double.NaN)
                End If
              Next
            Next
          End If
        End Using
      End Using
    End Using

    NormalizzaCanaleUtc(DictionaryCanali)
    NormalizzaCanaliTempo(DictionaryCanali)

    Console.WriteLine("LeggiFileLog: " & Now.Subtract(inizio).TotalSeconds.ToString("F3"))
    Return DictionaryCanali
  End Function

  Private Shared Function Is2022expFormat(FirstChannelName As String) As Boolean
    Return FirstChannelName = "!Boat"
  End Function


  Private Shared Function SeparatoreCampi(PathFile As String)
    Using sr As New StreamReader(PathFile)
      Dim Line As String = Nothing
      Do
        Line = sr.ReadLine
        If Not Line Is Nothing Then
          If Not Line.Trim = "" Then
            If Line.Contains(vbTab) Then Return vbTab
            If Line.Contains(",") Then Return ","
          End If
        End If
      Loop While Not Line Is Nothing
      Return ","
    End Using

  End Function

#End Region

#Region " Import entry points "

  Public Shared Function SelezionaFileExpeditionAndMakeParquet() As String
    Dim n As String = ""
    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder
    Dim pf As String = ObjFiles.SelezionaFile(PathExpDataFolder, "", "", "", n)
    If Not System.IO.File.Exists(pf) Then Return ""
    Dim fi As New System.IO.FileInfo(pf)
    PathExpDataFolder = fi.Directory.FullName
    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
    AppConfig.Salva()

    Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(pf)

    If d Is Nothing OrElse d.Count = 0 Then
      MsgBox("Import failed: no channels could be read from the log file." & vbCrLf & vbCrLf &
             ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
      Return ""
    End If

    Dim risultato As String = ExpeditionToParquet(pf, PathExpDataFolder, d, True, True)

    If risultato = "" Then
      MsgBox("Import failed." & vbCrLf & vbCrLf & ImportWarningsText(), MsgBoxStyle.Exclamation, "Import")
    ElseIf ImportWarnings.Count > 0 Then
      ' The file was imported but something in it was irregular: worth telling the user.
      MsgBox("Import completed with warnings:" & vbCrLf & vbCrLf & ImportWarningsText(),
             MsgBoxStyle.Information, "Import")
    End If

    Return risultato
  End Function


  Public Shared Function SelezionaFilesExpeditionAndMakeParquet() As String
    Dim n As New List(Of String)
    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathExpDataFolder, "", "", "", n)
    If pf Is Nothing Then Return ""
    Dim ft As New System.IO.FileInfo(pf.First)
    PathExpDataFolder = ft.Directory.FullName
    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
    AppConfig.Salva()

    Console.WriteLine(pf.Count)
    Dim Falliti As New List(Of String)
    For Each fl In pf
      Dim fi As New System.IO.FileInfo(fl)
      ' Use the same reader as the single-file path so that both routes benefit from
      ' the header and timestamp handling.
      Dim d As Dictionary(Of String, List(Of Double)) = ReadExpeditionLog2(fl)
      Console.WriteLine(fl)
      If d Is Nothing OrElse d.Count = 0 OrElse ExpeditionToParquet(fl, PathExpDataFolder, d, True, False) = "" Then
        Falliti.Add(fi.Name)
      End If
    Next
    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
    If Falliti.Count > 0 Then
      MsgBox("Done, but these files could not be imported:" & vbCrLf & String.Join(vbCrLf, Falliti),
             MsgBoxStyle.Exclamation, "Import")
    Else
      MsgBox("Done!")
    End If
    Return ""
  End Function




  Public Shared Function ExpeditionToParquet(SourcePathFile As String, StorageFolder As String, DictionaryCanali As Dictionary(Of String, List(Of Double)), ImportaSoloInMovimento As Boolean, OpenFolder As Boolean) As String
    Try

      Dim IdInizioImportazione As Integer = -1
      Dim IdFineImportazione As Integer = -1
      Dim Samples As Integer

      Dim FolderDate As String = ""
      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
      Dim ds As New List(Of Double)
      Dim DataDouble As New List(Of Double)

      Dim CanaleUtc = DictionaryCanali.Where(Function(x) x.Key.ToLower = "utc").FirstOrDefault()
      Dim CanaleLat = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault()
      Dim CanaleLon = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault()

      ' Original code tested "Where(...) Is Nothing", which is never true for a LINQ
      ' query, so the branch was always taken and a missing Lat/Lon threw inside the
      ' blanket Catch below, producing a silent failure.
      If CanaleUtc.Value IsNot Nothing AndAlso CanaleLat.Value IsNot Nothing AndAlso CanaleLon.Value IsNot Nothing Then
        Dim lats = CanaleLat.Value.ToArray
        Dim lngs = CanaleLon.Value.ToArray
        Dim latsValide = lats.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
        Dim lngsValide = lngs.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray

        If latsValide.Count = 0 OrElse lngsValide.Count = 0 Then
          AggiungiWarning("No valid Lat/Lon samples: time zone cannot be resolved, UTC used as local time.")
          FolderDate = DateTime.Today.ToString("yyyyMMdd")
        Else
          ' Use the FIRST valid fix, not the average of all of them. An average of
          ' coordinates is not a point on the track: it moves when the file is
          ' truncated differently, and can land in a different time zone. That is how
          ' the same log imported twice ended up with local times one hour apart.
          Dim lat As Double = Double.NaN
          Dim lng As Double = Double.NaN
          For iFix As Integer = 0 To Math.Min(lats.Count, lngs.Count) - 1
            If Not Double.IsNaN(lats(iFix)) AndAlso lats(iFix) <> 0 _
               AndAlso Not Double.IsNaN(lngs(iFix)) AndAlso lngs(iFix) <> 0 Then
              lat = lats(iFix)
              lng = lngs(iFix)
              Exit For
            End If
          Next
          Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
          Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
          Dim utc = CanaleUtc.Value.ToArray
          Dim dt As New List(Of DateTime)
          Dim validdt As New List(Of Double)
          For Each ut In utc
            If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
              Dim t As DateTime = DateTime.FromOADate(ut)
              Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
              dt.Add(convertedTime.LocalDateTime)
              DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
              validdt.Add(DataDouble.Last)
            Else
              dt.Add(Nothing)
              DataDouble.Add(Double.NaN)
            End If
          Next

          If validdt.Count = 0 Then
            AggiungiWarning("Utc channel contains no usable samples.")
            Return ""
          End If

          Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
          FolderDate = day.ToString("yyyyMMdd")
          ds.Clear()
          For Each d In dt
            If d = Nothing Then
              ds.Add(Double.NaN)
            Else
              If d < day Then
                ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
              Else
                ds.Add(d.TimeOfDay.TotalSeconds)
              End If
            End If
          Next
        End If
      Else
        AggiungiWarning("Utc, Lat or Lon channel missing: local time not computed.")
        FolderDate = DateTime.Today.ToString("yyyyMMdd")
      End If

      If ImportaSoloInMovimento Then
        Dim bs As Double() = Nothing
        Dim bsok As Boolean = True
        If DictionaryCanali.ContainsKey("Bs") Then
          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bs").FirstOrDefault().Value.ToArray
        ElseIf DictionaryCanali.ContainsKey("Bsp") Then
          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
        ElseIf DictionaryCanali.ContainsKey("BSP") Then
          bs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
        Else
          bsok = False
          IdInizioImportazione = 0
          IdFineImportazione = DictionaryCanali.First.Value.Count - 1
        End If
        If bsok Then
          For c As Integer = 0 To bs.Count - 1
            If bs(c) > 1 Then
              IdInizioImportazione = c
              Exit For
            End If
          Next
          For c As Integer = bs.Count - 1 To 0 Step -1
            If bs(c) > 1 Then
              IdFineImportazione = c
              Exit For
            End If
          Next
        End If
      Else
        IdInizioImportazione = 0
        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
      End If

      ' The original condition read "(IdInizioImportazione = -1 OrElse IdFineImportazione)":
      ' the second operand was a Double evaluated as a Boolean, which only compiled
      ' because of Option Strict Off and never tested what it meant to test.
      If (ImportaSoloInMovimento AndAlso (IdInizioImportazione = -1 OrElse IdFineImportazione = -1)) _
         OrElse IdInizioImportazione = IdFineImportazione Then
        IdInizioImportazione = 0
        IdFineImportazione = DictionaryCanali.First.Value.Count - 1
      End If
      Samples = IdFineImportazione - IdInizioImportazione

      If Samples <= 0 Then
        AggiungiWarning("No samples left after range selection.")
        Return ""
      End If

      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

      Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, ds.ToArray)
      VerificaCanaleROT(DictionaryCanali, ds, "ROT")
      Dim HeadersRate As String() = {"PitchRate", "Pitch Rate", "TrimRate", "Trim Rate", "Pitch rate", "TrimRT"}
      Dim HeadersTrim As String() = {"Pitch", "Trim", "Trm"}
      VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


      For Each Canale In DictionaryCanali
        Select Case Canale.Key
          Case "TrimRate", "HeelRate", "Pitch rate", "Roll Rate", "HeelRT"
            If HasValidData(Canale.Value) Then
              DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
            End If

          Case Else
            DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
        End Select
      Next

      Dim strnfilepath As String = System.IO.Path.Combine(StorageFolder, "strn.txt")

      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
      If Not System.IO.Directory.Exists(StorageFolder) Then
        System.IO.Directory.CreateDirectory(StorageFolder)
      End If

      If System.IO.File.Exists(strnfilepath) Then
        Dim strndest As String = System.IO.Path.Combine(StorageFolder, "strn.txt")
        If Not System.IO.File.Exists(strndest) Then
          System.IO.File.Copy(strnfilepath, strndest)
        End If
      End If

      Dim fi As New System.IO.FileInfo(SourcePathFile)
      Dim FileName As String = ("" + fi.Name).Replace(fi.Extension, ".ppf") ' parquet performance file

      Dim SW As New System.IO.StreamWriter(System.IO.Path.Combine(StorageFolder, FileName))
      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
      For Each C In CanaliPq
        PGR.WriteColumn(C)
      Next
      PW.Dispose()
      SW.Close()
      SW.Dispose()

      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

      Return System.IO.Path.Combine(StorageFolder, FileName)
    Catch ex As Exception
      ' Never swallow the reason again: a silent "" here is what made a mid-file
      ' timestamp format change look like "the import just does not work".
      AggiungiWarning("ExpeditionToParquet failed: " & ex.Message)
      Console.WriteLine("ExpeditionToParquet failed: " & ex.ToString)
      Return ""
    End Try

  End Function

#End Region

#Region " Misc "

  Public Shared Function HasValidData(Valori As List(Of Double)) As Boolean
    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
  End Function

  Public Shared Function HasValidData(Valori As List(Of String)) As Boolean
    Dim a = Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).ToArray
    Return a.Count > 0
  End Function

#End Region

End Class

Public Class clsFastSkipperUtilities


  Public Shared Function SelezionaFilesLogFastSkipperAndMakeParquet() As String
    Dim n As New List(Of String)
    'Dim PathExpDataFolder As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", AppConfig.ApplicationDataFolder, True, True)
    Dim PathExpDataFolder As String = AppConfig.ActiveProfile.LastImpFolder

    Dim pf As List(Of String) = ObjFiles.SelezionaFiles(PathExpDataFolder, "", "", "", n)
    If pf Is Nothing Then Return ""
    Dim ft As New System.IO.FileInfo(pf.First)
    PathExpDataFolder = ft.Directory.FullName
    AppConfig.ActiveProfile.LastImpFolder = PathExpDataFolder
    AppConfig.Salva()
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", PathExpDataFolder, True, True)
    'AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExpDataFolder", System.IO.Path.GetDirectoryName(pf.First), True, True)
    Console.WriteLine(pf.Count)
    Dim Boats As New List(Of clsFastSkipperBoat)
    For Each fl In pf
      Dim fi As New System.IO.FileInfo(fl)
      Dim BN As String = BoatName(fi.Name)
      Dim CurrentBoat = Boats.Where(Function(x) x.BoatName = BN).FirstOrDefault ' New clsFastSkipperBoat(BN)
      If CurrentBoat Is Nothing Then
        CurrentBoat = New clsFastSkipperBoat(BN)
        Boats.Add(CurrentBoat)
      End If

      Dim d As Dictionary(Of String, List(Of Double)) = LeggiFileLog(fl)
      CurrentBoat.AggiungiNuoviValori(d, "ISODateTimeUTC")
      Console.WriteLine(fl)

    Next

    Dim LastParquetFullPath As String = ""
    For Each Boat In Boats
      LastParquetFullPath = FastSkipperToParquet(Boat, PathExpDataFolder, True, False)
    Next

    System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(pf.First))
    MsgBox("Done!")
    Return LastParquetFullPath ' System.IO.Path.GetDirectoryName(pf.First)
  End Function

  Public Shared Function BoatName(FileName As String) As String
    Dim s = FileName.Split(".")
    Return s(0) & s(1)
  End Function


  Public Shared Function NomeCanaleStandardizzato(Header As String) As String


    ' ISODateTimeUTC,LAT,LON,SOG,COG,THDG,HDG,HEEL,PITCH,AWA,AWS,AWA1,AWS1,AWA2,AWS2,AWA3,AWS3,AWA4,AWS4,RDR,BPM1,BPM2,BPM3,BPM4,TRIM1,TRIM2,TRIM3,TRIM4,COURSE,AWD,TWA,TWS,TWD,BS,VMG,DMG,VMC,DR,DRA,VMGTgt,BSTgt,TWATgt,TWDAvg,TWDMin,TWDMax,TWSAvg,TWSMin,TWSMax,TWAAvg,TWAMin,TWAMax,AWDAvg,AWDMin,AWDMax,AWAAvg,AWAMin,AWAMax,AWSAvg,AWSMin,AWSMax,SOGAvg,BSMin,BSMax,BATT,CELL,XRR,YRR,ZRR,XACC,YACC,ZACC,XMAG,YMAG,ZMAG,AWAC,AWSC,SOG_m,TWD_m,ANGL,HEEL_A,HEEL_FREQ

    ' ISODateTimeUTC,THDG,AWA1,AWS1,AWA2,AWS2,AWA3,AWS3,AWA4,AWS4,RDR,BPM1,BPM2,BPM3,BPM4,TRIM1,TRIM2,TRIM3,TRIM4,COURSE,AWD,VMG,DMG,VMC,DR,DRA,VMGTgt,BSTgt,TWATgt,TWDAvg,TWDMin,TWDMax,TWSAvg,TWSMin,TWSMax,TWAAvg,TWAMin,TWAMax,AWDAvg,AWDMin,AWDMax,AWAAvg,AWAMin,AWAMax,AWSAvg,AWSMin,AWSMax,SOGAvg,BSMin,BSMax,BATT,CELL,XACC,YACC,ZACC,XMAG,YMAG,ZMAG,AWAC,AWSC,SOG_m,TWD_m,ANGL,HEEL_A,HEEL_FREQ

    Select Case Header
      Case "AWA"
        Return "Awa"
      Case "AWS"
        Return "Aws"
      Case "TWA"
        Return "Twa"
      Case "TWS"
        Return "Tws"
      Case "HDG"
        Return "Hdg"
      Case "COG"
        Return "Cog"
      Case "SOG"
        Return "Sog"
      Case "TWD"
        Return "Twd"
      Case "BS"
        Return "Bs"
      Case "HEEL"
        Return "Heel"
      Case "PITCH"
        Return "Pitch"
      Case "YRR"
        Return "PitchRate"
      Case "XRR"
        Return "RollRate"
      Case "ZRR"
        Return "YawRate"
      Case "LAT"
        Return "Latitude"
      Case "LON"
        Return "Longitude"
      Case "Lat"
        Return "LatBow"
      Case "Lon"
        Return "LonBow"
      Case Else
        Return Header
    End Select
  End Function

  Public Shared Function LeggiFileLog(PathLogFile As String) As Dictionary(Of String, List(Of Double))
    Dim DictionaryCanali As New Dictionary(Of String, List(Of Double))
    Dim c As New CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    c.MissingFieldFound = Nothing
    Dim fi As New System.IO.FileInfo(PathLogFile)
    c.Delimiter = SeparatoreCampi(PathLogFile)
    Using reader As New StreamReader(PathLogFile)
      Using csv As New CsvReader(reader, c)
        Using dr As New CsvDataReader(csv)
          Dim dt As New System.Data.DataTable
          dt.Load(dr)
          For col As Integer = 0 To dt.Columns.Count - 1
            DictionaryCanali.Add(dt.Columns(col).ColumnName, New List(Of Double))
            For row As Integer = 0 To dt.Rows.Count - 1
              Dim v As Double = Double.NaN
              If Double.TryParse(dt.Rows(row)(col).ToString, v) Then
                DictionaryCanali.Last.Value.Add(v)
              Else
                Dim vDT As DateTime ' "W3C" 
                'Console.WriteLine(Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"))
                If DateTime.TryParseExact(dt.Rows(row)(col).ToString, "yyyy-MM-ddTHH:mm:ss.fffzzz", Nothing, Globalization.DateTimeStyles.None, vDT) Then
                  DictionaryCanali.Last.Value.Add(vDT.ToOADate)
                Else
                  DictionaryCanali.Last.Value.Add(Double.NaN)
                End If

              End If
            Next
          Next
        End Using
      End Using
    End Using


    Return DictionaryCanali
  End Function

  Private Shared Function SeparatoreCampi(PathFile As String)
    Using sr As New StreamReader(PathFile)
      Dim Line As String = Nothing
      Do
        Line = sr.ReadLine
        If Not Line Is Nothing Then
          If Not Line.Trim = "" Then
            If Line.Contains(vbTab) Then Return vbTab
            If Line.Contains(",") Then Return ","
          End If
        End If
      Loop While Not Line Is Nothing
      Return ","
    End Using

  End Function

  Public Shared Function FastSkipperToParquet(Boat As clsFastSkipperBoat, StorageFolder As String, ImportaSoloInMovimento As Boolean, OpenFolder As Boolean) As String
    Try

      Dim IdInizioImportazione As Integer = -1
      Dim IdFineImportazione As Integer = -1
      Dim Samples As Integer

      Dim FolderDate As String = ""
      Dim CanaliPq As New List(Of Parquet.Data.DataColumn)

      Dim DF As New Parquet.Data.DataField("SystemTime_DaySeconds", Parquet.Data.DataType.Double, False)
      Dim Data As New Parquet.Data.DataField("SystemTime_DateTimeLocal", Parquet.Data.DataType.Double, False)
      Dim ds As New List(Of Double)
      Dim DataDouble As New List(Of Double)

      If Not Boat.Channels.Where(Function(x) x.Key.ToLower = "ISODateTimeUTC") Is Nothing Then
        Dim lats = Boat.Channels.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault().Value.ToArray
        Dim lngs = Boat.Channels.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault().Value.ToArray
        Dim lat As Double = lats.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
        Dim lng As Double = lngs.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
        Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
        Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
        Dim utc = Boat.Channels.Where(Function(x) x.Key.ToLower = "isodatetimeutc").FirstOrDefault().Value.ToArray
        Dim dt As New List(Of DateTime)
        Dim validdt As New List(Of Double)
        For Each ut In utc
          If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
            Dim t As DateTime = DateTime.FromOADate(ut)
            Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
            dt.Add(convertedTime.LocalDateTime)
            DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
            validdt.Add(DataDouble.Last)
          Else
            dt.Add(Nothing)
            DataDouble.Add(Double.NaN)
          End If
        Next
        Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
        FolderDate = day.ToString("yyyyMMdd")
        ds.Clear()
        For Each d In dt
          If d = Nothing Then
            ds.Add(Double.NaN)
          Else
            If d < day Then
              ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
            Else
              ds.Add(d.TimeOfDay.TotalSeconds)
            End If
          End If

        Next





        'Dim dsa As Double() = ds.ToArray
        'CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
        'CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.ToArray))

        'Dim Hz As Integer = VerificaCanaleHz(DictionaryCanali, dsa)
        'VerificaCanaleSeaStateIndex(DictionaryCanali, "SeaStateIndex", "PitchRate", "TrimRate", Hz)
      Else
        FolderDate = DateTime.Today.ToString("yyyyMMdd")
      End If

      If ImportaSoloInMovimento Then
        Dim bs As Double()
        Dim bsok As Boolean = True
        'Dim Sog = DictionaryCanali.Where(Function(x) x.Key.ToLower = "sog").FirstOrDefault().Value.ToArray
        If Boat.Channels.ContainsKey("BS") Then
          bs = Boat.Channels.Where(Function(x) x.Key.ToLower = "bs").FirstOrDefault().Value.ToArray
          'ElseIf Boat.Channels.ContainsKey("Bsp") Then
          '    bs = Boat.Channels.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
          'ElseIf Boat.Channels.ContainsKey("BSP") Then
          '    bs = Boat.Channels.Where(Function(x) x.Key.ToLower = "bsp").FirstOrDefault().Value.ToArray
        Else
          bsok = False
          IdInizioImportazione = 0
          IdFineImportazione = Boat.Channels.First.Value.Count - 1
        End If
        If bsok Then
          For c As Integer = 0 To bs.Count - 1
            If bs(c) > 1 Then
              IdInizioImportazione = c
              Exit For
            End If
          Next
          For c As Integer = bs.Count - 1 To 0 Step -1
            If bs(c) > 1 Then
              IdFineImportazione = c
              Exit For
            End If
          Next
        End If
      Else
        IdInizioImportazione = 0
        IdFineImportazione = Boat.Channels.First.Value.Count - 1
      End If
      If ImportaSoloInMovimento AndAlso (IdInizioImportazione = -1 OrElse IdFineImportazione) OrElse IdInizioImportazione = IdFineImportazione Then
        IdInizioImportazione = 0
        IdFineImportazione = Boat.Channels.First.Value.Count - 1
      End If
      Samples = IdFineImportazione - IdInizioImportazione

      Dim dsa As Double() = ds.Skip(IdInizioImportazione).Take(Samples).ToArray
      CanaliPq.Add(New Parquet.Data.DataColumn(DF, dsa))
      CanaliPq.Add(New Parquet.Data.DataColumn(Data, DataDouble.Skip(IdInizioImportazione).Take(Samples).ToArray))

      Dim Hz As Integer = clsExpeditionUtilities.VerificaCanaleHz(Boat.Channels, ds.ToArray)
      clsExpeditionUtilities.VerificaCanaleROT(Boat.Channels, ds, "ZRR")
      Dim HeadersRate As String() = {"YRR"}
      Dim HeadersTrim As String() = {"PITCH"}
      clsExpeditionUtilities.VerificaCanaleSeaStateIndex(Boat.Channels, "SeaStateIndex", Hz, HeadersRate, HeadersTrim)


      For Each Canale In Boat.Channels
        Select Case Canale.Key
          Case "XRR", "YRR", "ZRR"
            If HasValidData(Canale.Value) Then
              DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
              CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
            End If
          Case Else
            DF = New Parquet.Data.DataField(NomeCanaleStandardizzato(Canale.Key), Parquet.Data.DataType.Double, False)
            CanaliPq.Add(New Parquet.Data.DataColumn(DF, Canale.Value.Skip(IdInizioImportazione).Take(Samples).ToArray))
        End Select





      Next

      Dim strnfilepath As String = System.IO.Path.Combine(StorageFolder, "strn.txt")

      StorageFolder = System.IO.Path.Combine(StorageFolder, FolderDate)
      If Not System.IO.Directory.Exists(StorageFolder) Then
        System.IO.Directory.CreateDirectory(StorageFolder)
      End If

      If System.IO.File.Exists(strnfilepath) Then
        System.IO.File.Copy(strnfilepath, System.IO.Path.Combine(StorageFolder, "strn.txt"))
      End If


      'LatLonPositionIsTheBow = Not System.IO.File.Exists(pFileInfo.FullName.Replace(pFileInfo.Name, "strn.txt"))

      'Dim twss As Double() = Boat.Channels.Where(Function(x) x.Key.ToLower = "tws").FirstOrDefault.Value.ToArray
      'Dim Boat.BoatName & ".ppf" As String = System.IO.Path.Combine(StorageFolder, Boat.BoatName) & ".ppf" ' parquet performance file

      Dim ParquetFileFullPath As String = System.IO.Path.Combine(StorageFolder, Boat.BoatName & "_" & FolderDate & ".ppf")

      Dim SW As New System.IO.StreamWriter(ParquetFileFullPath)
      Dim PS As New Parquet.Data.Schema(CanaliPq.Select(Function(x) x.Field).ToList)
      Dim PW As New Parquet.ParquetWriter(PS, SW.BaseStream)
      Dim PGR As Parquet.ParquetRowGroupWriter = PW.CreateRowGroup
      For Each C In CanaliPq
        PGR.WriteColumn(C)
      Next
      PW.Dispose()
      SW.Close()
      SW.Dispose()

      If OpenFolder Then System.Diagnostics.Process.Start("explorer.exe", StorageFolder)

      Return ParquetFileFullPath
    Catch ex As Exception
      Return ""
    End Try

  End Function

  Public Shared Function HasValidData(Valori As List(Of Double)) As Boolean
    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
  End Function

  Public Shared Function HasValidData(Valori As List(Of String)) As Boolean
    Return Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Count > 0
  End Function




End Class

Public Class clsFastSkipperBoat
  Public BoatName As String
  Public LocalFromUtc As New TimeSpan(0)
  Public ChannelDT As New List(Of DateTime)
  Public Channels As New Dictionary(Of String, List(Of Double))

  Public Sub New(Boat As String)
    BoatName = Boat
  End Sub

  Public Sub AggiungiNuoviValori(d As Dictionary(Of String, List(Of Double)), TimeChannelName As String)
    If Channels.ContainsKey(TimeChannelName) Then
      Dim ValoriDTesistenti = Channels(TimeChannelName)
      If d.ContainsKey(TimeChannelName) Then
        Dim ValoriDT = d(TimeChannelName)
        For i As Integer = 0 To ValoriDT.Count - 1
          Dim DT As DateTime = DateTime.FromOADate(ValoriDT(i))

          If DT < ChannelDT.First Then
            ' mette all inizio
            ChannelDT.Insert(0, DT)
            For Each k In d.Keys
              If Not Channels.ContainsKey(k) Then
                Dim vv As New List(Of Double)
                For Each vl In ValoriDT
                  vv.Add(Double.NaN)
                Next
                Channels.Add(k, vv)
              End If
              Channels(k).Insert(0, d(k)(i))
            Next
          ElseIf DT > ChannelDT.Last Then
            ' accoda
            ChannelDT.Add(DT)
            For Each k In d.Keys
              If Not Channels.ContainsKey(k) Then
                Dim vv As New List(Of Double)
                For Each vl In ValoriDT
                  vv.Add(Double.NaN)
                Next
                Channels.Add(k, vv)
              End If
              Channels(k).Add(d(k)(i))
            Next
          ElseIf ChannelDT.IndexOf(DT) > -1 Then
            ' il valore esiste non aggiunge la riga
            'Stop
          Else
            ' mette dopo l indice trovato
            Dim idx As Integer = TrovaIndicePrecedente(DT, ChannelDT.ToArray)
            ChannelDT.Insert(idx + 1, DT)
            For Each k In d.Keys
              If Not Channels.ContainsKey(k) Then
                Dim vv As New List(Of Double)
                For Each vl In ValoriDT
                  vv.Add(Double.NaN)
                Next
                Channels.Add(k, vv)
              End If
              Channels(k).Insert(idx + 1, d(k)(i))
            Next
          End If
        Next


      End If
    Else
      If d.ContainsKey(TimeChannelName) Then
        Dim ValoriDT = d(TimeChannelName)
        Channels = d
        For i As Integer = 0 To ValoriDT.Count - 1
          ChannelDT.Add(DateTime.FromOADate(ValoriDT(i)))
        Next
      End If
    End If



  End Sub


End Class
