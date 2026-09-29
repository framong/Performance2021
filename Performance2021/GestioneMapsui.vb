Imports BruTile.Predefined
Imports Mapsui.Layers
Imports Mapsui.Geometries
Imports Mapsui.Providers
'Imports Mapsui.Samples.Common.Helpers
Imports Mapsui.Styles
Imports Mapsui.UI
Imports Mapsui.Utilities
Imports BruTile.Web
Imports Mapsui.UI.Wpf
Imports BruTile.MbTiles
Imports SQLite
Imports System.Reflection
Imports System.Net.Http
Imports BruTile

'Public Class clsMapsuiUtilities
'  Dim pPosizioneCorrente As New Feature
'  Dim pTimeRangeCorrente As clsTimeRange
'  Dim chLat As clsChannel2020
'  Dim chLng As clsChannel2020
'  Dim pLayerCursore As Mapsui.Layers.Layer


'  Public Sub ImpostaPuntoIniziale(Punto As Mapsui.Geometries.Point, LayerCursore As Mapsui.Layers.Layer)
'    chLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
'    chLng = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
'    pLayerCursore = LayerCursore
'    Dim SS As New SymbolStyle
'    SS.SymbolType = SymbolType.Ellipse
'    SS.SymbolScale = 0.3
'    PosizioneCorrente.Styles.Add(SS)
'    PosizioneCorrente.Geometry = Punto
'  End Sub

'  Public Property PosizioneCorrente As Feature
'    Get
'      Return pPosizioneCorrente
'    End Get
'    Set(value As Feature)
'      pPosizioneCorrente = value
'    End Set
'  End Property

'  Public Property TimeRangeCorrente As clsTimeRange
'    Get
'      Return pTimeRangeCorrente
'    End Get
'    Set(value As clsTimeRange)
'      pTimeRangeCorrente = value
'    End Set
'  End Property

'  Public Sub AggiornaPosizioneCorrente(Latitude As Double, Longitude As Double)
'    PosizioneCorrente.Geometry = Mapsui.Projection.SphericalMercator.FromLonLat(Latitude, Longitude)
'  End Sub

'  Public Sub AggiornaPosizioneCorrente(Momento As DateTime)
'    If DataProvider2020 Is Nothing Then Exit Sub
'    Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
'    PosizioneCorrente.Geometry = Mapsui.Projection.SphericalMercator.FromLonLat(chLng.Valori(Id), chLat.Valori(Id))
'    pLayerCursore.DataHasChanged()
'  End Sub

'End Class




Public Class clsGestioneMapsui
  WithEvents pMyMapControl As Mapsui.UI.Wpf.MapControl
  Dim chLat As clsChannel2020
  Dim chLng As clsChannel2020
  Dim chBs As clsChannel2020
  Dim chHdg As clsChannel2020
  Dim chTwa As clsChannel2020
  Dim chTws As clsChannel2020
  Dim chTwd As clsChannel2020
  'Dim chPortArm As clsChannel2020
  'Dim chStbdArm As clsChannel2020

  'Dim _PolareBs As clsPolare2019CanaleValori
  Dim _Tgt As clsTgt
  Dim _VettoreBarca As New clsVettoreMapsui("Boat", Color.Blue, 3)
  Dim _VettoreVento As New clsVettoreMapsui("Wind", Color.Orange, 3)
  Dim _VettoreLLp As New clsVettoreMapsui("PortLL", Color.Red, 1)
  Dim _VettoreLLs As New clsVettoreMapsui("StbdLL", Color.Green, 1)
  Dim _LayerVettori As New Mapsui.Layers.Layer


  'Dim _VettoreBarca As New LineString
  'Dim _VettoreVento As New LineString

  Dim pLayerCursore As Mapsui.Layers.WritableLayer
  Dim pPosizioneCorrente As New Feature
  Dim pStartingPoint As Point = Nothing
  Dim pLayerSelezione As New Mapsui.Layers.Layer
  Dim pLayerDisegno As New Mapsui.Layers.Layer
  Dim pPuntiLayerDisegno As New List(Of Point)
  Dim pSelezioneCorrente As New Feature
  Dim pTRselezione As clsTimeRange


  ' TileLayer non espone DataSource in Mapsui 2.0: si tiene traccia dell'esistenza
  ' effettiva dei file .mbtiles al momento della creazione dei layer

  Dim pLayerOpenTopoMap As Mapsui.Layers.TileLayer
  Dim pLayerEmodnet As Mapsui.Layers.TileLayer
  Dim pLayerOpenStreetMap As Mapsui.Layers.TileLayer
  Dim pLayerGoogleMaps As Mapsui.Layers.TileLayer
  Dim pLayerGoogleTerrain As Mapsui.Layers.TileLayer
  Dim pLayerGoogleSatellite As Mapsui.Layers.TileLayer

  Dim pLayersAttivi As New List(Of Mapsui.Layers.TileLayer)

  Dim _MinHz As Integer

  Dim pDescrizioneDisegno As String

  'Dim pActiveBackLayer As TileLayer

  Public Sub New(ByRef MapControl As Mapsui.UI.Wpf.MapControl)
    MyMapControl = MapControl
    pLayerDisegno.Name = "Disegno"
    ImpostaMapControl()
  End Sub


  Public Property MyMapControl As MapControl
    Get
      Return pMyMapControl
    End Get
    Set(value As MapControl)
      pMyMapControl = value
    End Set
  End Property

  Public Property PosizioneCorrente As Feature
    Get
      Return pPosizioneCorrente
    End Get
    Set(value As Feature)
      pPosizioneCorrente = value
    End Set
  End Property

  Public Property SelezioneCorrente As Feature
    Get
      Return pSelezioneCorrente
    End Get
    Set(value As Feature)
      pSelezioneCorrente = value
    End Set
  End Property

  Public Property TRselezione As clsTimeRange
    Get
      Return pTRselezione
    End Get
    Set(value As clsTimeRange)
      pTRselezione = value
    End Set
  End Property

  Public ReadOnly Property DescrizioneDisegno As String
    Get
      Return pDescrizioneDisegno
    End Get
  End Property

  Public Sub NascondiLayerDisegno()
    pDescrizioneDisegno = ""
    pPuntiLayerDisegno.Clear()
    pLayerDisegno.Enabled = False
  End Sub

  Public Sub AggiungiPuntoToLayerDisegno(Punto As Point)
    pLayerDisegno.Enabled = True
    pPuntiLayerDisegno.Add(Punto)
    AggiornaLayerDisegno()
  End Sub

  Private Sub AggiornaLayerDisegno()
    If pPuntiLayerDisegno.Count = 0 Then
      Exit Sub
    ElseIf pPuntiLayerDisegno.Count = 1 Then
      'disegna il punto iniziale
      Dim PuntoTmp As New Feature
      PuntoTmp.Geometry = pPuntiLayerDisegno.First
      Dim SS As New SymbolStyle
      SS.SymbolType = SymbolType.Ellipse
      SS.SymbolScale = 0.2
      SS.Fill = New Brush(New Color(255, 0, 0, 255))
      SS.Outline = Nothing
      SS.Line = New Pen(Color.Red, 4)
      'PosizioneCorrente.Styles.Add(SS)
      pLayerDisegno.Style = SS
      pLayerDisegno.DataSource = New MemoryProvider(PuntoTmp)
      Dim l = Mapsui.Projection.SphericalMercator.ToLonLat(pPuntiLayerDisegno.First.X, pPuntiLayerDisegno.First.Y)
      pDescrizioneDisegno = l.Y.ToString("F6") & " " & l.X.ToString("F6")
    Else
      'disegna la spezzata
      Dim FcTmp As New Feature
      Dim linea As New LineString
      FcTmp.Geometry = linea
      Dim St As New VectorStyle
      Dim LastSailingMode As eSailingMode = eSailingMode.eNotSureIsRacing
      St.Line = New Pen(Color.Red, 1)
      'St.Line.PenStrokeCap = PenStrokeCap.Butt
      FcTmp.Styles.Add(St)
      Dim puntoPrev As Point
      Dim BrgPrev As Double = Nothing
      Dim RB As New clsCalcoliDuePuntiGeo
      pDescrizioneDisegno = ""
      For Each punto In pPuntiLayerDisegno
        linea.Vertices.Add(punto)
        If Not punto Is pPuntiLayerDisegno.First Then
          Dim p1c = Mapsui.Projection.SphericalMercator.ToLonLat(puntoPrev.X, puntoPrev.Y)
          Dim p1 As New clsGeographicPosition(p1c.Y, p1c.X)
          Dim p2c = Mapsui.Projection.SphericalMercator.ToLonLat(punto.X, punto.Y)
          Dim p2 As New clsGeographicPosition(p2c.Y, p2c.X)
          RB.CalcolaDistanzaAndRotta(p1, p2)
          If Not BrgPrev = Nothing Then
            '  BrgPrev = RB.RottaTrue
            'Else
            Dim Angolo As Double = DifferenzaAssolutaTraAngoli360(RB.Rotta, SommaAngolo180adAngolo360(180, BrgPrev), True)
            'If Angolo > 90 Then Angolo = 180 - Angolo
            pDescrizioneDisegno &= " ( " & Angolo.ToString("F0") & "°) "
          End If
          BrgPrev = RB.RottaTrue
          If RB.DistanzaNM < 0.4 Then
            pDescrizioneDisegno &= "r:" & (RB.DistanzaNM * 1852).ToString("F0") & "m, b:" & RB.RottaTrue.ToString("F0").ToString.PadLeft(3, "0") & "°"
          Else
            pDescrizioneDisegno &= "r:" & RB.DistanzaNM.ToString("F2") & "Nm, b:" & RB.RottaTrue.ToString("F0").ToString.PadLeft(3, "0") & "°"
          End If
        End If
        puntoPrev = punto
      Next
      pLayerDisegno.DataSource = New MemoryProvider(FcTmp)
    End If

  End Sub

  Public Function GeographicPosition(Momento As DateTime) As clsGeographicPosition
    Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
    Return New clsGeographicPosition(chLat.Valori(Id), chLng.Valori(Id))
  End Function

  Public Sub AggiornaPosizioneCorrente(Momento As DateTime)
    If DataProvider2020 Is Nothing Then Exit Sub
    If chLat Is Nothing OrElse chLat.Valori.Count = 0 Then Exit Sub
    Dim Id As Integer = DataProvider2020.TrovaIndice(Momento)
    Dim PuntoBarca As New clsGeographicPosition(chLat.Valori(Id), chLng.Valori(Id))
    PosizioneCorrente.Geometry = Mapsui.Projection.SphericalMercator.FromLonLat(PuntoBarca.LngDec, PuntoBarca.LatDec)
    Dim Hdg As Double = MagneticToTrue(PuntoBarca, Momento, chHdg.Valori(Id))
    Dim Bs As Double = chBs.Valori(Id)
    Dim Tws As Double = chTws.Valori(Id)
    Dim Twa As Double = chTwa.Valori(Id)
    Dim Twd As Double = MagneticToTrue(PuntoBarca, Momento, chTwd.Valori(Id))
    Dim BsTg As Double = 0
    Dim TwaTg As Double = 0
    If Not TgtManager.Tgt Is Nothing Then
      Dim v = TgtManager.Tgt.ValoreTgt(System.Math.Abs(Twa) < 90, Tws, "bs")
      BsTg = If(v Is Nothing, Double.NaN, v.Bs)
      TwaTg = If(v Is Nothing, Double.NaN, v.Twa)
    End If

    'If Hdg > 180 Then Hdg -= 360


    Dim Secs As Double = 30
    Dim ST = DirectCast(PosizioneCorrente.Styles(0), SymbolStyle)
    ST.SymbolRotation = Hdg
    If MyMapControl.Viewport.Resolution > 50000 Then
      ST.SymbolScale = 2.0
      Secs = 600
    ElseIf MyMapControl.Viewport.Resolution > 60 Then
      ST.SymbolScale = 1.8
      Secs = 300
    ElseIf MyMapControl.Viewport.Resolution > 40 Then
      ST.SymbolScale = 1.7
      Secs = 150
    ElseIf MyMapControl.Viewport.Resolution > 30 Then
      ST.SymbolScale = 1.5
      Secs = 90
    ElseIf MyMapControl.Viewport.Resolution > 10 Then
      ST.SymbolScale = 1.35
      Secs = 40
    Else
      ST.SymbolScale = 1.2
      Secs = 10
    End If
    'Console.WriteLine(ST.SymbolScale & ": " & MyMapControl.Viewport.Resolution)
    'la freccia rapprenta Secs secondi minuto di navigazione
    'BS da nodi in metri al secondo e poi punto ad x distanza
    Dim LLSecs As Double = 120
    If _LayerVettori.Enabled Then
      _VettoreBarca.AggiornaVettore(PuntoBarca, KtsToMS(Bs) * Secs, Hdg)
      _VettoreVento.AggiornaVettore(PuntoBarca, KtsToMS(Tws) * Secs, Twd)
      _VettoreLLp.AggiornaVettore(PuntoBarca, KtsToMS(BsTg) * LLSecs, SommaAngolo180adAngolo360(TwaTg, Twd))
      _VettoreLLs.AggiornaVettore(PuntoBarca, KtsToMS(BsTg) * LLSecs, SommaAngolo180adAngolo360(-TwaTg, Twd))
    End If

    pLayerCursore.DataHasChanged()
  End Sub

  Public LayersString As String

  Private Sub ImpostaMapControl()
    MyMapControl.Map.Layers.Clear()

    pLayerOpenStreetMap = CreateOpenStreetMap()
    pLayerOpenStreetMap.Name = "OpenStreetMap"
    pLayerOpenStreetMap.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerOpenStreetMap)

    pLayerGoogleMaps = CreateGoogleMaps()
    pLayerGoogleMaps.Name = "GoogleMaps"
    pLayerGoogleMaps.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerGoogleMaps)

    ' Navionics rimosso completamente: il servizio online non e' piu' disponibile e
    ' le mappe locali .mbtiles sono state abbandonate. La disponibilita' offline e' ora
    ' garantita dalla cache su disco applicata a tutti i layer (vedi CacheDisco).



    pLayerGoogleTerrain = CreateGoogleTerrain()
    pLayerGoogleTerrain.Name = "GoogleTerrain"
    pLayerGoogleTerrain.Enabled = True
    MyMapControl.Map.Layers.Add(pLayerGoogleTerrain)

    pLayerGoogleSatellite = CreateGoogleSatellite()
    pLayerGoogleSatellite.Name = "GoogleSatellite"
    pLayerGoogleSatellite.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerGoogleSatellite)

    ' Alternativa a GoogleTerrain: quello usa un endpoint interno di Google, senza contratto,
    ' che puo' smettere di funzionare da un giorno all'altro come e' successo a Navionics.
    ' OpenTopoMap e' pubblica e con licenza esplicita (CC-BY-SA), rilievo e curve di livello.
    pLayerOpenTopoMap = CreateOpenTopoMap()
    pLayerOpenTopoMap.Name = "OpenTopoMap"
    pLayerOpenTopoMap.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerOpenTopoMap)

    ' EMODnet Bathymetry: batimetria europea ~125 m, GEBCO ~500 m altrove.
    ' Servizio pubblico, licenza CC-BY 4.0. NON utilizzabile per la navigazione.
    pLayerEmodnet = CreateEmodnetBathymetry()
    pLayerEmodnet.Name = "EMODnetBathymetry"
    pLayerEmodnet.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerEmodnet)

    _LayerVettori.Name = "LayerVettori"
    MyMapControl.Map.Layers.Add(_LayerVettori)

    pLayerDisegno.Enabled = False
    MyMapControl.Map.Layers.Add(pLayerDisegno)

    'pLayerCursore = CreaCursoreBarca()
    'pLayerCursore.Name = "Cursore"
    'MyMapControl.Map.Layers.Add(pLayerCursore)
    pLayersAttivi.Clear()
    pLayersAttivi.Add(pLayerGoogleTerrain)
    pLayersAttivi.Add(pLayerGoogleSatellite)
    pLayersAttivi.Add(pLayerGoogleMaps)
    pLayersAttivi.Add(pLayerOpenTopoMap)
    pLayersAttivi.Add(pLayerEmodnet)
    pLayersAttivi.Add(pLayerOpenStreetMap)

    SelezionaLayerIniziale()
    AggiornaLayersString()

  End Sub

#Region "Selezione della mappa iniziale"

  ''' <summary>
  ''' Attiva l'ultima mappa usata, o GoogleTerrain alla prima esecuzione.
  ''' Senza connessione i tile gia' visitati arrivano dalla cache su disco (vedi CacheDisco).
  ''' </summary>
  Private Sub SelezionaLayerIniziale()
    Dim Desiderato As ILayer = Nothing
    Dim NomeSalvato As String = ""
    Try
      If Not AppConfig Is Nothing AndAlso Not AppConfig.ActiveProfile Is Nothing Then
        NomeSalvato = AppConfig.ActiveProfile.LastMapLayer
      End If
    Catch ex As Exception
    End Try
    If Not NomeSalvato = "" Then Desiderato = TrovaLayer(NomeSalvato)
    If Desiderato Is Nothing Then Desiderato = pLayerGoogleTerrain

    ' un solo layer mappa attivo alla volta
    For Each Ly In MyMapControl.Map.Layers
      If TypeOf Ly Is TileLayer Then Ly.Enabled = Ly Is Desiderato
    Next
    If Not Desiderato Is Nothing Then Desiderato.Enabled = True
  End Sub

  ''' <summary>Aggiorna la descrizione della mappa attiva, sovrapposizione inclusa.</summary>
  Public Sub AggiornaLayersString()
    Dim Testo As String = ""
    For Each Layer As TileLayer In pLayersAttivi
      If Layer.Enabled Then Testo &= Layer.Name & " "
    Next
    If Testo = "" Then
      ' puo' essere attiva una mappa locale non presente in pLayersAttivi
      For Each Ly In MyMapControl.Map.Layers
        If TypeOf Ly Is TileLayer AndAlso Ly.Enabled Then
          Testo &= Ly.Name & " "
          Exit For
        End If
      Next
    End If
    LayersString = Testo.Trim()
  End Sub

  ''' <summary>Memorizza nel profilo il layer mappa attualmente attivo.</summary>
  Private Sub SalvaLayerCorrente()
    Try
      If AppConfig Is Nothing OrElse AppConfig.ActiveProfile Is Nothing Then Exit Sub
      Dim Attivo = pLayersAttivi.Where(Function(x) x.Enabled).FirstOrDefault
      Dim Nome As String = ""
      If Not Attivo Is Nothing Then
        Nome = Attivo.Name
      Else
        For Each Ly In MyMapControl.Map.Layers
          If TypeOf Ly Is TileLayer AndAlso Ly.Enabled Then
            Nome = Ly.Name
            Exit For
          End If
        Next
      End If
      If Not Nome = "" AndAlso Not Nome = AppConfig.ActiveProfile.LastMapLayer Then
        AppConfig.ActiveProfile.LastMapLayer = Nome
        AppConfig.Salva()
      End If
    Catch ex As Exception
    End Try
  End Sub

#End Region

  Public Sub TogglaMappa()
    Dim Active = pLayersAttivi.Where(Function(x) x.Enabled).FirstOrDefault
    Dim nxt As Integer = 0
    If Active Is Nothing Then
      Active = pLayersAttivi.First
    Else
      nxt = pLayersAttivi.IndexOf(Active) + 1
      If nxt >= pLayersAttivi.Count Then nxt = 0
    End If
    For i As Integer = 0 To pLayersAttivi.Count - 1
      pLayersAttivi(i).Enabled = i = nxt
    Next
    ZoomIn()
    ZoomOut()

    AggiornaLayersString()

    ' ricorda la scelta per il prossimo avvio
    SalvaLayerCorrente()

  End Sub

  ''' <summary>
  ''' HttpClient con User-Agent che identifica l'applicazione. La tile usage policy di
  ''' OpenStreetMap impone un User-Agent valido e univoco: senza, i server rispondono 403
  ''' con il tile "Access blocked". Vale per tutti i servizi derivati da OSM.
  ''' </summary>
  Private Shared _HttpClientOsm As HttpClient = Nothing

  ''' <summary>
  ''' Cache persistente su disco dei tile scaricati, una cartella per layer.
  ''' Sostituisce le mappe .mbtiles: le zone gia' visitate restano consultabili senza
  ''' connessione, per qualsiasi layer e senza doversi procurare file esterni.
  ''' I tile scadono dopo 90 giorni, cosi' la cartografia non resta indietro all'infinito.
  ''' </summary>
  Private Shared Function CacheDisco(NomeLayer As String) As BruTile.Cache.IPersistentCache(Of Byte())
    Try
      Dim Radice As String = System.IO.Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "SailingPerformer", "TileCache", NomeLayer)
      Return New clsCacheTilesSuDisco(Radice, 90)
    Catch ex As Exception
      ' senza cache si continua a lavorare, solo senza disponibilita' offline
      Return Nothing
    End Try
  End Function

  Private Shared Function ClientOsm() As HttpClient
    If _HttpClientOsm Is Nothing Then
      _HttpClientOsm = New HttpClient()
      _HttpClientOsm.DefaultRequestHeaders.Add("User-Agent", "SailingPerformer-DataAnalyzer/2026.08 (+https://www.sailingperformer.com)")
    End If
    Return _HttpClientOsm
  End Function

  ''' <summary>
  ''' OpenTopoMap: mappa topografica con rilievo e curve di livello, zoom max 17.
  ''' Licenza CC-BY-SA, richiede l'attribuzione a OpenStreetMap e SRTM.
  ''' </summary>
  Public Shared Function CreateOpenTopoMap() As TileLayer
    Return New TileLayer(New HttpClientTileSource(ClientOsm(), New GlobalSphericalMercator(0, 17), "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png", {"a", "b", "c"}, name:="OpenTopoMap", persistentCache:=CacheDisco("OpenTopoMap")))
  End Function

  ''' <summary>
  ''' EMODnet Bathymetry, layer mean_atlas_land: profondita' media con copertura terrestre.
  ''' Tile set web_mercator (EPSG:3857), zoom 0-15. Nel template WMTS {TileCol} e' x e
  ''' {TileRow} e' y, quindi l'ordine e' {z}/{x}/{y}.
  ''' </summary>
  Public Shared Function CreateEmodnetBathymetry() As TileLayer
    Return New TileLayer(New HttpTileSource(New GlobalSphericalMercator(0, 15), "https://tiles.emodnet-bathymetry.eu/latest/mean_atlas_land/web_mercator/{z}/{x}/{y}.png", Nothing, name:="EMODnetBathymetry", persistentCache:=CacheDisco("EMODnetBathymetry")))
  End Function

  Public Shared Function CreateOpenStreetMap() As TileLayer
    Return New TileLayer(New HttpClientTileSource(ClientOsm(), New GlobalSphericalMercator(0, 19), "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {"a", "b", "c"}, name:="OpenStreetMap", persistentCache:=CacheDisco("OpenStreetMap")))
  End Function

  Public Shared Function CreateGoogleTerrain() As TileLayer
    Return New TileLayer(New HttpTileSource(New GlobalSphericalMercator(0, 20), "http://mt{s}.google.com/vt/lyrs=t@125,r@130&hl=en&x={x}&y={y}&z={z}", {"0", "1", "2", "4"}, name:="GoogleTerrain", persistentCache:=CacheDisco("GoogleTerrain")))
  End Function

  Public Shared Function CreateGoogleSatellite() As TileLayer
    Return New TileLayer(New HttpTileSource(New GlobalSphericalMercator(0, 20), "http://mt{s}.google.com/vt/lyrs=s@125,r@130&hl=en&x={x}&y={y}&z={z}", {"0", "1", "2", "4"}, name:="GoogleSatellite", persistentCache:=CacheDisco("GoogleSatellite")))
  End Function

  Public Shared Function CreateGoogleMaps() As TileLayer
    Return New TileLayer(New HttpTileSource(New GlobalSphericalMercator(0, 20), "http://mt{s}.google.com/vt/lyrs=m@130&hl=en&x={x}&y={y}&z={z}", {"0", "1", "2", "4"}, name:="GoogleMaps", persistentCache:=CacheDisco("GoogleMaps")))
  End Function



  'h = roads only
  'm = standard roadmap
  'p = terrain
  'r = somehow altered roadmap
  's = satellite only
  't = terrain only
  'y = hybrid


  Public Shared Function CreateMbTilesLayer(ByVal path As String) As TileLayer
    If Not System.IO.File.Exists(path) Then Return New TileLayer
    Try
      Dim mbTilesTileSource = New MbTilesTileSource(New SQLiteConnectionString(path, True))
      Dim mbTilesLayer = New TileLayer(mbTilesTileSource)
      Return mbTilesLayer
    Catch ex As Exception
      Return New TileLayer
    End Try

  End Function

  Private Function TrovaLayer(Nome As String) As ILayer
    For Each Ly In MyMapControl.Map.Layers
      If Ly.Name = Nome Then Return Ly
    Next
    Return Nothing
  End Function

  ''' <summary>
  ''' Rimuove i layer derivati dai dati caricati. AggiornaTracciaBase li AGGIUNGE soltanto:
  ''' senza questa pulizia, al secondo caricamento la traccia precedente resterebbe disegnata.
  ''' </summary>
  Public Sub AzzeraTraccia()
    If MyMapControl Is Nothing Then Exit Sub
    If MyMapControl.Map Is Nothing Then Exit Sub
    Dim Nomi As String() = {"Selezione", "Traccia", "Vettori", "Cursore"}
    For Each Nome In Nomi
      Dim DaTogliere = MyMapControl.Map.Layers.Where(Function(x) x.Name = Nome).ToList
      For Each Ly In DaTogliere
        Try
          MyMapControl.Map.Layers.Remove(Ly)
        Catch ex As Exception
        End Try
      Next
    Next
    Try
      MyMapControl.Refresh()
    Catch ex As Exception
    End Try
  End Sub

  Public Sub AggiornaTracciaBase(MinHz As Integer)
    ' idempotente: toglie sempre i layer precedenti prima di ricrearli
    AzzeraTraccia()
    ImpostaCanali()
    If chLat Is Nothing OrElse chLat.Valori.Count = 0 Then Exit Sub
    MyMapControl.Map.Layers.Add(pLayerSelezione)
    MyMapControl.Map.Layers.Last.Name = "Selezione"
    Dim LayerTraccia = CreaLayerTraccia(MinHz)
    MyMapControl.Map.Layers.Add(LayerTraccia)
    MyMapControl.Map.Layers.Last.Name = "Traccia"
    MyMapControl.Navigator.CenterOn(LayerTraccia.Envelope.Centroid)
    MyMapControl.Navigator.ZoomTo(200)

    _LayerVettori = CreaLayerVettori()
    _LayerVettori.Name = "Vettori"
    MyMapControl.Map.Layers.Add(_LayerVettori)

    pLayerCursore = CreaCursoreBarca()
    pLayerCursore.Name = "Cursore"
    MyMapControl.Map.Layers.Add(pLayerCursore)
  End Sub


  Private Function CreaLayerVettori() As ILayer
    Dim ListaVettori As New List(Of Feature)
    ListaVettori.Add(_VettoreLLp.Feature)
    ListaVettori.Add(_VettoreLLs.Feature)
    ListaVettori.Add(_VettoreBarca.Feature)
    ListaVettori.Add(_VettoreVento.Feature)
    _VettoreLLp.ImpostazioneIniziale(pStartingPoint)
    _VettoreLLs.ImpostazioneIniziale(pStartingPoint)
    _VettoreBarca.ImpostazioneIniziale(pStartingPoint)
    _VettoreVento.ImpostazioneIniziale(pStartingPoint)
    Dim LY As New Layer
    LY.DataSource = New MemoryProvider(ListaVettori)
    Return LY
  End Function

  Private Function CreaCursoreBarca() As ILayer
    Dim LY As New WritableLayer
    'Dim LY As WritableLayer = CreateBoatLayer()
    'PosizioneCorrente.Geometry = New Point(9.2, 39)
    'Dim SS As New SymbolStyle
    'SS = CreaSymbolStyleBarcaSvg()
    'PosizioneCorrente.Styles.Add(SS)
    'LY.Style = SS
    'LY.DataSource = New MemoryProvider(PosizioneCorrente)

    'Dim ssb As New SymbolStyle
    'ssb.Line = New Pen(Color.Black, 2)
    'Dim fcb As New Feature
    'fcb.Geometry = _VettoreBarca
    'fcb.Styles.Add(ssb)
    'LY.Add(fcb)

    'Dim ssv As New SymbolStyle
    'ssv.Line = New Pen(Color.Orange, 2)
    'Dim fcv As New Feature
    'fcv.Geometry = _VettoreVento
    'fcv.Styles.Add(ssv)
    'LY.Add(fcv)

    Dim ss As New SymbolStyle
    ss.BitmapId = BoatImageId
    'ss.SymbolType = SymbolType.Svg
    ss.SymbolScale = 3
    ss.UnitType = UnitType.Pixel
    ss.SymbolRotation = 0
    ss.SymbolOffset = New Offset(0, 0, True)
    LY.Style = ss
    PosizioneCorrente.Styles.Add(ss)
    PosizioneCorrente.Geometry = pStartingPoint
    LY.Add(PosizioneCorrente)

    Return LY
  End Function

  'Private Function CreaLayerCursore(PuntoIniziale As Point) As ILayer
  '  Dim LY As New Layer
  '  PosizioneCorrente.Geometry = PuntoIniziale
  '  Dim SS As New SymbolStyle
  '  SS.SymbolType = SymbolType.Ellipse
  '  'SS = CreaSymbolStyleBarcaSvg()
  '  SS.SymbolScale = 0.5
  '  SS.Fill = New Brush(New Color(255, 0, 0, 150))
  '  SS.Outline = Nothing
  '  SS.Line = New Pen(Color.Red, 4)
  '  PosizioneCorrente.Styles.Add(SS)
  '  LY.Style = SS
  '  LY.DataSource = New MemoryProvider(PosizioneCorrente)
  '  Return LY
  'End Function

  'Private Function CreaSymbolStyleBarcaSvg() As SymbolStyle
  '  Dim pPathBin As String = My.Application.Info.DirectoryPath
  '  Dim ss As New SymbolStyle
  '  ss.SymbolType = SymbolType.Svg
  '  'ss.SymbolScale = 1
  '  ss.BitmapId = GetBitmapIdForEmbeddedResource(pPathBin & "\Boat.svg") ' "Mapsui.Samples.Common.Images.Pin.svg"
  '  Return ss
  'End Function


  Private Shared Function GetBitmapIdForEmbeddedResource(ByVal imagePath As String) As Integer
    Dim assembly = GetType(Point).GetTypeInfo().Assembly
    If System.IO.File.Exists(imagePath) Then
      'Dim image = assembly.GetManifestResourceStream(imagePath)
      Dim svg As String = System.IO.File.ReadAllText(imagePath)
      Return BitmapRegistry.Instance.Register(svg)
    End If
    Return 0
  End Function


  Shared _boatImageId As Integer = -1

  Public Shared ReadOnly Property BoatImageId As Integer
    Get

      If _boatImageId = -1 Then

        Try
          ' PNG e non piu' SVG: il rendering SVG passa da SkiaSharp.Extended.Svg, che e'
          ' compilato contro SkiaSharp 1.68 mentre il progetto usa la 3.119.1, e solleva
          ' MissingMethodException a ogni disegno del simbolo.
          Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
          Dim name = (From resName In assembly.GetManifestResourceNames() Where resName.EndsWith("boat.png") Select resName).FirstOrDefault()
          If name Is Nothing Then
            ' ripiego sull'SVG se il PNG non e' stato incluso tra le risorse incorporate
            name = (From resName In assembly.GetManifestResourceNames() Where resName.EndsWith("boat.svg") Select resName).FirstOrDefault()
          End If
          If Not name Is Nothing Then
            Dim image = assembly.GetManifestResourceStream(name)
            _boatImageId = BitmapRegistry.Instance.Register(image)
          End If
        Catch
        End Try
      End If
      Return _boatImageId
    End Get
  End Property

  Public Shared Function CreateBoatLayer() As WritableLayer
    Dim layer = New WritableLayer With {
            .Style = New SymbolStyle With {
                .BitmapId = BoatImageId,
                .SymbolScale = 1,
                .SymbolOffset = New Offset(0.0, 0.0, True)
            },
            .IsMapInfoLayer = False
        }
    Return layer
    '.SymbolType = SymbolType.Svg,

  End Function


  Private Function CreaLayerTraccia(MinHz As Integer) As ILayer
    Dim LY As New Layer
    Try
      LY.DataSource = New MemoryProvider(Traccia(MinHz))
    Catch ex As Exception
      Stop
    End Try
    Return LY

  End Function

  Private Sub ImpostaCanali()
    chLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    chLng = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    chBs = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
    chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eHDG)
    If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCSE)
    If chHdg Is Nothing Then chHdg = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eCOG)
    chTwa = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
    chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    'chPortArm = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.ePortCantAngle)
    'chStbdArm = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eStbdCantAngle)
  End Sub

  Private Function SailingMode(Twa As Double, Bs As Double) As eSailingMode
    If Bs < 4 Then Return eSailingMode.eNotSureIsRacing
    If System.Math.Abs(Twa) > 120 Then
      If Twa > 0 Then
        Return eSailingMode.eDnStbd
      Else
        Return eSailingMode.eDnPort
      End If
    ElseIf System.Math.Abs(Twa) > 70 Then
      If Twa > 0 Then
        Return eSailingMode.eRcStbd
      Else
        Return eSailingMode.eRcPort
      End If
    Else
      If Twa > 0 Then
        Return eSailingMode.eUpStbd
      Else
        Return eSailingMode.eUpPort
      End If
    End If
  End Function

  'Private Function SailingMode(Twa As Double, Bs As Double, PortArmCant As Double, StbdArmCant As Double) As eSailingMode
  '  If Bs < 15 Then Return eSailingMode.eNotFoiling
  '  If System.Math.Abs(PortArmCant - StbdArmCant) < 10 Then Return eSailingMode.eBothArmsInTheWater
  '  If System.Math.Abs(Twa) > 120 Then
  '    If Twa > 0 Then
  '      Return eSailingMode.eDnStbd
  '    Else
  '      Return eSailingMode.eDnPort
  '    End If
  '  ElseIf System.Math.Abs(Twa) > 70 Then
  '    If Twa > 0 Then
  '      Return eSailingMode.eRcStbd
  '    Else
  '      Return eSailingMode.eRcPort
  '    End If
  '  Else
  '    If Twa > 0 Then
  '      Return eSailingMode.eUpStbd
  '    Else
  '      Return eSailingMode.eUpPort
  '    End If
  '  End If
  'End Function

  Private Enum eSailingMode
    eNotSureIsRacing
    eUpStbd
    eUpPort
    eDnStbd
    eDnPort
    eRcStbd
    eRcPort
    eBothArmsInTheWater
  End Enum

  Private Function ColoreDaSailingMode(SailingMode As eSailingMode) As Color
    Select Case SailingMode
      Case eSailingMode.eNotSureIsRacing
        Return Color.Gray
      Case eSailingMode.eUpStbd
        Return Color.Green
      Case eSailingMode.eUpPort
        Return Color.Red
      Case eSailingMode.eDnStbd
        Return Color.Blue
      Case eSailingMode.eDnPort
        Return Color.Orange
      Case eSailingMode.eRcStbd
        Return Color.Cyan
      Case eSailingMode.eRcPort
        Return Color.Yellow
      Case eSailingMode.eBothArmsInTheWater
        Return Color.Violet
    End Select
  End Function

  Private Function Gold() As Color
    Return New Color(255, 215, 0, 255)
  End Function

  Private Function Traccia(MinHz As Integer) As List(Of Feature)
    _MinHz = MinHz
    'ImpostaCanali()
    Dim TracciaTmp As New List(Of Feature)
    Dim FcTmp As New Feature
    Dim linea As New LineString
    FcTmp.Geometry = linea
    Dim St As New VectorStyle
    Dim LastSailingMode As eSailingMode = eSailingMode.eNotSureIsRacing
    St.Line = New Pen(ColoreDaSailingMode(LastSailingMode), 2)
    'St.Line = New Pen(Color.Red, 2)
    FcTmp.Styles.Add(St)

    Dim lastvalidgeopoint As clsGeographicPosition = Nothing
    Dim lastvalidmoment As DateTime = DataProvider2020.TimeRange.Start
    Dim d As New clsCalcoliDuePuntiGeo()
    Dim maxbsms As Double = 50
    Dim mprev As DateTime = DataProvider2020.TimeRange.Start.AddSeconds(-1)
    For i As Integer = 0 To DataProvider2020.TimeStamps.Count - 1
      Dim m = DataProvider2020.TimeStamps(i)
      If Not m = Nothing AndAlso m.Subtract(mprev).TotalMilliseconds > (1000 / MinHz) Then
        Dim la, ln As Double
        If chLat Is Nothing OrElse chLat.Valori.Count = 0 Then
          la = 0
          ln = 0
        Else
          la = chLat.Valori(i)
          ln = chLng.Valori(i)
        End If
        Dim geopoint As New clsGeographicPosition(la, ln)
        'Console.WriteLine(geopoint.LatDec & " " & geopoint.LngDec)
        If Not Double.IsNaN(geopoint.LatDec) AndAlso Not (geopoint.LatDec = 0) AndAlso Not (geopoint.LatDec = geopoint.LngDec) Then
          'Dim ActualSailingMode As eSailingMode = SailingMode(chTwa.Valori(i), chBs.Valori(i), chPortArm.Valori(i), chStbdArm.Valori(i))
          Dim ActualSailingMode As eSailingMode = SailingMode(chTwa.Valori(i), chBs.Valori(i))
          If Not ActualSailingMode = LastSailingMode Then
            TracciaTmp.Add(FcTmp)
            FcTmp = New Feature
            linea = New LineString
            FcTmp.Geometry = linea
            St = New VectorStyle
            St.Line = New Pen(ColoreDaSailingMode(ActualSailingMode), 2)
            'St.Line = New Pen(Color.Red, 2)
            FcTmp.Styles.Add(St)
          End If
          If lastvalidgeopoint Is Nothing Then
            lastvalidgeopoint = geopoint
            lastvalidmoment = DataProvider2020.TimeStamps(i)
          Else
            d.CalcolaDistanzaAndRotta(geopoint, lastvalidgeopoint)
            Dim momento As DateTime = DataProvider2020.TimeStamps(i)
            Dim secs As Double = momento.Subtract(lastvalidmoment).TotalSeconds
            'If secs = 0 Then Stop
            If d.DistanzaMetri < maxbsms * secs Then
              Dim punto As Mapsui.Geometries.Point = Mapsui.Projection.SphericalMercator.FromLonLat(geopoint.LngDec, geopoint.LatDec)
              If pStartingPoint Is Nothing Then pStartingPoint = Mapsui.Projection.SphericalMercator.FromLonLat(geopoint.LngDec, geopoint.LatDec)
              linea.Vertices.Add(punto)
              lastvalidgeopoint = geopoint
              lastvalidmoment = DataProvider2020.TimeStamps(i)
            End If
          End If
          LastSailingMode = ActualSailingMode
        End If
        mprev = m
      End If
    Next
    TracciaTmp.Add(FcTmp)

    Dim TracciaNotEmpty As New List(Of Feature)
    For Each t In TracciaTmp
      If Not t.Geometry.IsEmpty Then
        TracciaNotEmpty.Add(t)
      End If
    Next

    Return TracciaNotEmpty
  End Function

  Public Sub ZoomToAll()
    Dim LayerTraccia As Layer = TrovaLayer("Traccia")
    If LayerTraccia Is Nothing Then Exit Sub
    MyMapControl.Navigator.NavigateTo(LayerTraccia.Envelope)
    ZoomOut()
  End Sub

  Public Sub ZoomToActualSelection()
    If pLayerSelezione Is Nothing Then Exit Sub
    MyMapControl.Navigator.NavigateTo(pLayerSelezione.Envelope)
    ZoomOut()
  End Sub

  Public Sub ZoomIn()
    MyMapControl.Navigator.ZoomIn()
  End Sub

  Public Sub ZoomOut()
    MyMapControl.Navigator.ZoomOut()
  End Sub

  Public Function AggiornaSelezione(TimeRange As clsTimeRange)
    TRselezione = TimeRange
    If TimeRange.HasSameRange(DataProvider2020.TimeRange) Then
      pLayerSelezione.Enabled = False
    Else
      pLayerSelezione.Enabled = True
      pLayerSelezione.DataSource = New MemoryProvider(Selezione(TimeRange.IdRigaIniziale, TimeRange.IdRigaFinale))
    End If
  End Function

  Private Function Selezione(IdIniziale As Integer, IdFinale As Integer) As Feature
    Dim FcTmp As New Feature
    Dim linea As New LineString
    FcTmp.Geometry = linea
    Dim St As New VectorStyle
    St.Line = New Pen(Gold, 8)
    FcTmp.Styles.Add(St)

    Dim lastvalidgeopoint As clsGeographicPosition = Nothing
    Dim lastvalidmoment As DateTime = DataProvider2020.TimeRange.Start
    Dim d As New clsCalcoliDuePuntiGeo()
    Dim maxbsms As Double = 50

    Dim mprev As DateTime = DataProvider2020.Momento(IdIniziale).AddSeconds(-1)
    For i As Integer = IdIniziale To IdFinale
      Dim m = DataProvider2020.TimeStamps(i)
      If Not m = Nothing AndAlso m.Subtract(mprev).TotalMilliseconds > (1000 / _MinHz) Then
        Dim geopoint As New clsGeographicPosition(chLat.Valori(i), chLng.Valori(i))
        If Not Double.IsNaN(geopoint.LatDec) Then
          'Dim ActualSailingMode As eSailingMode = SailingMode(chTwa.Valori(i), chBs.Valori(i), chPortArm.Valori(i), chStbdArm.Valori(i))
          Dim ActualSailingMode As eSailingMode = SailingMode(chTwa.Valori(i), chBs.Valori(i))
          If lastvalidgeopoint Is Nothing Then
            lastvalidgeopoint = geopoint
            lastvalidmoment = DataProvider2020.TimeStamps(i)
          Else
            d.CalcolaDistanzaAndRotta(geopoint, lastvalidgeopoint)
            Dim momento As DateTime = DataProvider2020.TimeStamps(i)
            Dim secs As Double = momento.Subtract(lastvalidmoment).TotalSeconds
            If d.DistanzaMetri < maxbsms * secs Then
              Dim punto As Mapsui.Geometries.Point = Mapsui.Projection.SphericalMercator.FromLonLat(geopoint.LngDec, geopoint.LatDec)
              If pStartingPoint Is Nothing Then pStartingPoint = Mapsui.Projection.SphericalMercator.FromLonLat(geopoint.LngDec, geopoint.LatDec)
              linea.Vertices.Add(punto)
              lastvalidgeopoint = geopoint
              lastvalidmoment = DataProvider2020.TimeStamps(i)
            End If
          End If
        End If
        mprev = m
      End If
    Next
    Return FcTmp
  End Function

  'Private Function TracciaMultipunto() As List(Of Point)
  '  Dim risultato As New List(Of Point)
  '  Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
  '  Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
  '  For i As Integer = 0 To DataProvider2020.TimeStamps.Count - 1
  '    Dim Lat As Double = chLat.Valori(i)
  '    Dim Lng As Double = chLng.Valori(i)
  '    If Not Double.IsNaN(Lng) Then
  '      Dim Punto As Mapsui.Geometries.Point = Mapsui.Projection.SphericalMercator.FromLonLat(Lng, Lat)
  '      risultato.Add(Punto)
  '    End If
  '  Next
  '  Return risultato
  'End Function


  'Private Function CreatePolygon() As List(Of Polygon)
  '  Dim risultato As New List(Of Polygon)
  '  Dim Traccia As New Polygon()
  '  Dim chLat As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
  '  Dim chLng As clsChannel2020 = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
  '  For i As Integer = 0 To DataProvider2020.TimeStamps.Count - 1
  '    Dim Lat As Double = chLat.Valori(i)
  '    Dim Lng As Double = chLng.Valori(i)
  '    If Not Double.IsNaN(Lng) Then
  '      Dim Punto As Mapsui.Geometries.Point = Mapsui.Projection.SphericalMercator.FromLonLat(Lng, Lat)
  '      Traccia.ExteriorRing.Vertices.Add(Punto)
  '    End If
  '  Next

  '  risultato.Add(Traccia)
  '  Return risultato
  'End Function


End Class

''' <summary>
''' Cache dei tile su disco. BruTile in questa versione non espone una FileCache, quindi
''' l'interfaccia IPersistentCache viene implementata qui: e' un archivio di PNG in
''' sottocartelle livello/colonna, con scadenza a giorni.
''' </summary>
Friend Class clsCacheTilesSuDisco
  Implements BruTile.Cache.IPersistentCache(Of Byte())

  Private ReadOnly _Radice As String
  Private ReadOnly _GiorniValidita As Integer

  Public Sub New(Radice As String, GiorniValidita As Integer)
    _Radice = Radice
    _GiorniValidita = GiorniValidita
    Try
      If Not System.IO.Directory.Exists(_Radice) Then System.IO.Directory.CreateDirectory(_Radice)
    Catch ex As Exception
    End Try
  End Sub

  Private Function PercorsoTile(Index As BruTile.TileIndex) As String
    Return System.IO.Path.Combine(_Radice, Index.Level.ToString(), Index.Col.ToString(), Index.Row.ToString() & ".png")
  End Function

  Public Sub Add(Index As BruTile.TileIndex, Tile As Byte()) Implements BruTile.Cache.IPersistentCache(Of Byte()).Add
    Try
      If Tile Is Nothing OrElse Tile.Length = 0 Then Exit Sub
      Dim Percorso As String = PercorsoTile(Index)
      Dim Cartella As String = System.IO.Path.GetDirectoryName(Percorso)
      If Not System.IO.Directory.Exists(Cartella) Then System.IO.Directory.CreateDirectory(Cartella)
      System.IO.File.WriteAllBytes(Percorso, Tile)
    Catch ex As Exception
      ' un tile non salvato non e' un problema: verra' riscaricato
    End Try
  End Sub

  Public Function Find(Index As BruTile.TileIndex) As Byte() Implements BruTile.Cache.IPersistentCache(Of Byte()).Find
    Try
      Dim Percorso As String = PercorsoTile(Index)
      If Not System.IO.File.Exists(Percorso) Then Return Nothing
      If _GiorniValidita > 0 Then
        If System.IO.File.GetLastWriteTimeUtc(Percorso) < DateTime.UtcNow.AddDays(-_GiorniValidita) Then
          Try
            System.IO.File.Delete(Percorso)
          Catch ex As Exception
          End Try
          Return Nothing
        End If
      End If
      Return System.IO.File.ReadAllBytes(Percorso)
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public Sub Remove(Index As BruTile.TileIndex) Implements BruTile.Cache.IPersistentCache(Of Byte()).Remove
    Try
      Dim Percorso As String = PercorsoTile(Index)
      If System.IO.File.Exists(Percorso) Then System.IO.File.Delete(Percorso)
    Catch ex As Exception
    End Try
  End Sub

End Class

Friend Class HttpClientTileSource
  Inherits HttpTileSource

  Public Sub New(ByVal httpClient As HttpClient, ByVal tileSchema As ITileSchema, ByVal urlFormatter As String, ByVal Optional serverNodes As IEnumerable(Of String) = Nothing, ByVal Optional apiKey As String = Nothing, ByVal Optional name As String = Nothing, ByVal Optional persistentCache As BruTile.Cache.IPersistentCache(Of Byte()) = Nothing, ByVal Optional attribution As Attribution = Nothing)
    MyBase.New(tileSchema, urlFormatter, serverNodes, apiKey, name, persistentCache, Function(uri) httpClient.GetByteArrayAsync(uri).ConfigureAwait(False).GetAwaiter().GetResult(), attribution)
    'Try
    'Catch ex As Exception

    'End Try
  End Sub
End Class



Public Class clsVettoreMapsui
  Dim CalcoliGeo As New clsCalcoliDuePuntiGeo

  Dim _Feature As New Feature
  Dim _Geometry As New LineString
  Dim _Stile As New SymbolStyle
  Dim _Nome As String

  Public Sub New(Nome As String, Colore As Color, Spessore As Integer)
    _Nome = Nome
    _Stile.Line = New Pen(Colore, Spessore)
    _Stile.Line.PenStrokeCap = PenStrokeCap.Round
    Feature.Geometry = _Geometry
    Feature.Styles.Add(_Stile)
  End Sub

  Public Property Feature As Feature
    Get
      Return _Feature
    End Get
    Set(value As Feature)
      _Feature = value
    End Set
  End Property

  'Public Property Geometry As LineString
  '  Get
  '    Return _Geometry
  '  End Get
  '  Set(value As LineString)
  '    _Geometry = value
  '  End Set
  'End Property

  'Public Property Stile As Style
  '  Get
  '    Return _Stile
  '  End Get
  '  Set(value As Style)
  '    _Stile = value
  '  End Set
  'End Property

  Public Property Nome As String
    Get
      Return _Nome
    End Get
    Set(value As String)
      _Nome = value
    End Set
  End Property

  Public Sub AggiornaVettore(PuntoIniziale As clsGeographicPosition, RangeMt As Double, bearing As Double)
    Dim PuntoFinale As clsGeographicPosition = CalcoliGeo.PuntoDestinazione(PuntoIniziale, RangeMt, bearing)
    _Geometry.Vertices.Clear()
    _Geometry.Vertices.Add(Mapsui.Projection.SphericalMercator.FromLonLat(PuntoIniziale.LngDec, PuntoIniziale.LatDec))
    _Geometry.Vertices.Add(Mapsui.Projection.SphericalMercator.FromLonLat(PuntoFinale.LngDec, PuntoFinale.LatDec))
  End Sub

  Public Sub ImpostazioneIniziale(PuntoIniziale As Point)
    _Geometry.Vertices.Clear()
    _Geometry.Vertices.Add(PuntoIniziale)
  End Sub

End Class