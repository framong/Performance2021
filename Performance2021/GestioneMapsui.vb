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
    Dim Nomi As String() = {"Selezione", "Traccia", "Glifi", "Vettori", "Cursore"}
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
    _LayerGlifi = New Layer With {.Name = "Glifi"}
    ' senza questo lo stile di default del layer disegna un pallino bianco dietro a ogni simbolo
    _LayerGlifi.Style = New VectorStyle With {.Enabled = False}
    MyMapControl.Map.Layers.Add(_LayerGlifi)
    ImpostaGlifi()
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
    Return Nothing
  End Function

  Private Function Gold() As Color
    Return New Color(255, 215, 0, 255)
  End Function

#Region "Colorazione della traccia"

  ' 0 andatura, 1 Vmg%, 2 Bs%, 3 Current rate, 4 Tws, 5 Twd (scostamento dalla media + barbe), 6 Current dir (frecce)
  Dim _ModoColore As Integer = 0
  Dim _RampTipo As Integer
  Dim _RampMin As Double
  Dim _RampMax As Double
  Dim _LayerGlifi As Mapsui.Layers.Layer
  Dim _MediaTwd As Double

  Public Event LegendaCambiata()
  Public LegendaTesto As String = ""
  Public LegendaColori As New List(Of Color)

  Dim _SoloSelezione As Boolean = False

  ''' <summary>True = la traccia (e i glifi) copre solo il periodo visualizzato, False = tutto il file.</summary>
  Public Property SoloSelezione As Boolean
    Get
      Return _SoloSelezione
    End Get
    Set(value As Boolean)
      _SoloSelezione = value
      AggiornaColoreTraccia()
      ZoomToAll()
    End Set
  End Property

  Private Sub LimitiTraccia(ByRef Da As Integer, ByRef A As Integer)
    Da = 0
    A = DataProvider2020.TimeStamps.Count - 1
    If _SoloSelezione AndAlso TRselezione IsNot Nothing Then
      Da = System.Math.Max(0, System.Math.Min(A, TRselezione.IdRigaIniziale))
      A = System.Math.Max(Da, System.Math.Min(A, TRselezione.IdRigaFinale))
    End If
  End Sub

  Public Property ModoColore As Integer
    Get
      Return _ModoColore
    End Get
    Set(value As Integer)
      _ModoColore = value
    End Set
  End Property

  ''' <summary>Ridisegna solo colori e glifi della traccia, senza spostare la mappa.</summary>
  Public Sub AggiornaColoreTraccia()
    If MyMapControl Is Nothing OrElse MyMapControl.Map Is Nothing Then Exit Sub
    If DataProvider2020 Is Nothing OrElse chLat Is Nothing OrElse _MinHz = 0 Then Exit Sub
    Dim LayerTraccia As Mapsui.Layers.Layer = TrovaLayer("Traccia")
    If LayerTraccia Is Nothing Then Exit Sub
    LayerTraccia.DataSource = New MemoryProvider(Traccia(_MinHz))
    LayerTraccia.DataHasChanged()
    ImpostaGlifi()
    MyMapControl.Refresh()
  End Sub

  Private Sub ImpostaGlifi()
    If _LayerGlifi Is Nothing Then Exit Sub
    Dim Lista = CreaGlifi()
    If Lista.Count = 0 Then
      _LayerGlifi.Enabled = False
    Else
      _LayerGlifi.DataSource = New MemoryProvider(Lista)
      _LayerGlifi.Enabled = True
      _LayerGlifi.DataHasChanged()
    End If
  End Sub

  Private Shared Function ChiaveColore(c As Color) As Integer
    Return (CInt(c.R) << 16) Or (CInt(c.G) << 8) Or CInt(c.B)
  End Function

  Private Function ValoriCanale(Chiave As clsChannels2020.eCanaliChiave) As Double()
    Dim ch = DataProvider2020.CanaleDbl(Chiave)
    If ch Is Nothing OrElse ch.Valori Is Nothing Then Return Nothing
    If ch.Valori.Length <> DataProvider2020.TimeStamps.Count Then Return Nothing
    Return ch.Valori
  End Function

  Private Sub PercentiliValidi(v As Double(), ByRef Basso As Double, ByRef Alto As Double)
    Dim ok = v.Where(Function(x) Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)).OrderBy(Function(x) x).ToArray
    If ok.Length = 0 Then
      Basso = 0 : Alto = 1
      Exit Sub
    End If
    Basso = ok(CInt((ok.Length - 1) * 0.05))
    Alto = ok(CInt((ok.Length - 1) * 0.95))
    If Alto - Basso < 0.01 Then Alto = Basso + 0.01
  End Sub

  ''' <summary>Valori da mappare in colore per il modo scelto (Nothing = andatura o dato non disponibile) e relativa legenda.</summary>
  Private Function PreparaValoriColore() As Double()
    LegendaTesto = ""
    LegendaColori = New List(Of Color)
    Dim v As Double() = Nothing
    Dim Testo As String = ""
    Select Case _ModoColore
      Case 0
        RaiseEvent LegendaCambiata()
        Return Nothing
      Case 1
        v = ValoriCanale(clsChannels2020.eCanaliChiave.eVMGp)
        _RampTipo = 1 : _RampMin = 80 : _RampMax = 100
        Testo = "80 → 100 % Vmg"
      Case 2
        v = ValoriCanale(clsChannels2020.eCanaliChiave.eBSPp)
        _RampTipo = 1 : _RampMin = 80 : _RampMax = 100
        Testo = "80 → 100 % Bs"
      Case 3, 6
        v = ValoriCanale(clsChannels2020.eCanaliChiave.eCurrRateRec)
        _RampTipo = 0
        If v IsNot Nothing Then
          PercentiliValidi(v, _RampMin, _RampMax)
          Testo = _RampMin.ToString("F2") & " → " & _RampMax.ToString("F2") & " kts current"
        End If
      Case 4
        v = ValoriCanale(clsChannels2020.eCanaliChiave.eTWS)
        _RampTipo = 0
        If v IsNot Nothing Then
          PercentiliValidi(v, _RampMin, _RampMax)
          Testo = _RampMin.ToString("F1") & " → " & _RampMax.ToString("F1") & " kts Tws"
        End If
      Case 5
        Dim twd = ValoriCanale(clsChannels2020.eCanaliChiave.eTWD)
        If twd IsNot Nothing Then
          Dim ss As Double = 0, cc As Double = 0
          For Each x In twd
            If Not Double.IsNaN(x) Then
              ss += System.Math.Sin(Radians(x))
              cc += System.Math.Cos(Radians(x))
            End If
          Next
          _MediaTwd = Degrees(System.Math.Atan2(ss, cc))
          If _MediaTwd < 0 Then _MediaTwd += 360
          v = New Double(twd.Length - 1) {}
          For i As Integer = 0 To twd.Length - 1
            v(i) = If(Double.IsNaN(twd(i)), Double.NaN, DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(_MediaTwd, twd(i)))
          Next
          Dim Basso, Alto As Double
          PercentiliValidi(v.Select(Function(x) System.Math.Abs(x)).ToArray, Basso, Alto)
          Dim R As Double = System.Math.Max(5, System.Math.Ceiling(Alto))
          _RampTipo = 2 : _RampMin = -R : _RampMax = R
          Testo = "-" & R.ToString("F0") & "° ← " & _MediaTwd.ToString("F0") & "° → +" & R.ToString("F0") & "° Twd"
        End If
    End Select
    If v Is Nothing Then
      LegendaTesto = "n/a"
    Else
      For k As Integer = 0 To 4
        LegendaColori.Add(ColoreDaRamp(k / 4.0))
      Next
      LegendaTesto = Testo
    End If
    RaiseEvent LegendaCambiata()
    Return v
  End Function

  Private Function ColoreDaValore(v As Double) As Color
    If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return Color.Gray
    Dim t As Double = (v - _RampMin) / (_RampMax - _RampMin)
    Return ColoreDaRamp(t)
  End Function

  Private Function Mescola(a As Color, b As Color, t As Double) As Color
    Return New Color(CInt(a.R + (b.R - a.R) * t), CInt(a.G + (b.G - a.G) * t), CInt(a.B + (b.B - a.B) * t), 255)
  End Function

  ''' <summary>t tra 0 e 1, quantizzato in 24 gradini per non frammentare troppo la traccia.</summary>
  Private Function ColoreDaRamp(t As Double) As Color
    t = System.Math.Max(0, System.Math.Min(1, t))
    t = System.Math.Round(t * 24) / 24
    Select Case _RampTipo
      Case 1 ' rosso - giallo - verde
        If t < 0.5 Then Return Mescola(New Color(215, 40, 40, 255), New Color(240, 200, 0, 255), t * 2)
        Return Mescola(New Color(240, 200, 0, 255), New Color(30, 160, 60, 255), (t - 0.5) * 2)
      Case 2 ' blu - grigio - rosso
        If t < 0.5 Then Return Mescola(New Color(30, 80, 220, 255), New Color(210, 210, 210, 255), t * 2)
        Return Mescola(New Color(210, 210, 210, 255), New Color(220, 40, 30, 255), (t - 0.5) * 2)
      Case Else ' blu - azzurro - verde - giallo - rosso
        Dim Stops As Color() = {New Color(30, 40, 230, 255), New Color(0, 200, 230, 255), New Color(40, 190, 60, 255), New Color(250, 220, 0, 255), New Color(230, 30, 30, 255)}
        Dim x As Double = t * 4
        Dim k As Integer = System.Math.Min(3, CInt(System.Math.Floor(x)))
        Return Mescola(Stops(k), Stops(k + 1), x - k)
    End Select
  End Function

  ' Glifi sulla traccia: barbe del vento (modo Twd) o frecce di corrente (modo Current dir).
  ' Sono simboli bitmap di dimensione fissa in pixel, ruotati sulla direzione, cosi' restano leggibili a ogni zoom.
  Shared _BarbeIds As New Dictionary(Of Integer, Integer)
  Shared _FrecciaId As Integer = -1

  Private Shared Function RegistraBitmap(bmp As System.Drawing.Bitmap) As Integer
    Dim ms As New System.IO.MemoryStream
    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
    ms.Position = 0
    Return BitmapRegistry.Instance.Register(ms)
  End Function

  Private Shared Function IdBarba(Tws As Double) As Integer
    Dim Classe As Integer = CInt(System.Math.Min(60, System.Math.Round(Tws / 5) * 5))
    If _BarbeIds.ContainsKey(Classe) Then Return _BarbeIds(Classe)
    Dim Id As Integer
    Using bmp As New System.Drawing.Bitmap(64, 64, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
      Using g = System.Drawing.Graphics.FromImage(bmp)
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        For Passo As Integer = 0 To 1
          Dim Spessore As Single = If(Passo = 0, 5.0F, 2.2F)
          Using pen As New System.Drawing.Pen(If(Passo = 0, System.Drawing.Color.White, System.Drawing.Color.Black), Spessore)
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round
            If Classe = 0 Then
              g.DrawEllipse(pen, 26, 26, 12, 12)
            Else
              g.DrawLine(pen, 32, 62, 32, 2)
              Dim Y As Single = 4
              For k As Integer = 1 To Classe \ 10
                g.DrawLine(pen, 32, Y, 48, Y + 7)
                Y += 7
              Next
              If Classe Mod 10 = 5 Then
                If Classe = 5 Then Y = 11
                g.DrawLine(pen, 32, Y, 40, Y + 3.5F)
              End If
            End If
          End Using
        Next
      End Using
      Id = RegistraBitmap(bmp)
    End Using
    _BarbeIds(Classe) = Id
    Return Id
  End Function

  Private Shared Function IdFreccia() As Integer
    If _FrecciaId >= 0 Then Return _FrecciaId
    Using bmp As New System.Drawing.Bitmap(64, 64, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
      Using g = System.Drawing.Graphics.FromImage(bmp)
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        Dim Testa As System.Drawing.PointF() = {New System.Drawing.PointF(32, 2), New System.Drawing.PointF(19, 24), New System.Drawing.PointF(45, 24)}
        Using bianco As New System.Drawing.Pen(System.Drawing.Color.White, 7)
          bianco.LineJoin = System.Drawing.Drawing2D.LineJoin.Round
          g.DrawLine(bianco, 32, 60, 32, 20)
          g.DrawPolygon(bianco, Testa)
        End Using
        Using blu As New System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 60, 190), 3)
          g.DrawLine(blu, 32, 60, 32, 20)
        End Using
        Using br As New System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 0, 60, 190))
          g.FillPolygon(br, Testa)
        End Using
      End Using
      _FrecciaId = RegistraBitmap(bmp)
    End Using
    Return _FrecciaId
  End Function

  Private Function CreaGlifi() As List(Of Feature)
    Dim Lista As New List(Of Feature)
    If _ModoColore <> 5 AndAlso _ModoColore <> 6 Then Return Lista
    If DataProvider2020 Is Nothing OrElse chLat Is Nothing OrElse chLat.Valori.Count = 0 Then Return Lista
    Dim n As Integer = DataProvider2020.TimeStamps.Count
    Dim Rate As Double() = Nothing, Dir As Double() = Nothing
    If _ModoColore = 6 Then
      Rate = ValoriCanale(clsChannels2020.eCanaliChiave.eCurrRateRec)
      Dir = ValoriCanale(clsChannels2020.eCanaliChiave.eCurrDirRec)
      If Rate Is Nothing OrElse Dir Is Nothing Then Return Lista
    ElseIf chTws Is Nothing OrElse chTwd Is Nothing Then
      Return Lista
    End If
    ' circa 40 glifi lungo tutto il file, mai piu' fitti di uno ogni 30 s
    Dim iDa As Integer, iA As Integer
    LimitiTraccia(iDa, iA)
    Dim Inizio As DateTime = If(iDa = 0, DataProvider2020.TimeRange.Start, DataProvider2020.TimeStamps(iDa))
    Dim Fine As DateTime = If(iA = n - 1, DataProvider2020.TimeRange.Finish, DataProvider2020.TimeStamps(iA))
    Dim Passo As Double = System.Math.Max(10, Fine.Subtract(Inizio).TotalSeconds / 40)
    Dim Prossimo As DateTime = Inizio
    For i As Integer = iDa To iA
      Dim m = DataProvider2020.TimeStamps(i)
      If m = Nothing OrElse m < Prossimo Then Continue For
      Dim pos As New clsGeographicPosition(chLat.Valori(i), chLng.Valori(i))
      If Double.IsNaN(pos.LatDec) OrElse pos.LatDec = 0 OrElse pos.LatDec = pos.LngDec Then Continue For
      Dim Ss As New SymbolStyle
      Ss.UnitType = UnitType.Pixel
      Ss.SymbolOffset = New Offset(0, 0, True)
      If _ModoColore = 6 Then
        If Double.IsNaN(Rate(i)) OrElse Double.IsNaN(Dir(i)) OrElse Rate(i) < 0.05 Then Continue For
        Ss.BitmapId = IdFreccia()
        Ss.SymbolScale = 0.35 + 0.35 * System.Math.Min(Rate(i), 3)
        Ss.SymbolRotation = Dir(i)
      Else
        If Double.IsNaN(chTws.Valori(i)) OrElse Double.IsNaN(chTwd.Valori(i)) Then Continue For
        Ss.BitmapId = IdBarba(chTws.Valori(i))
        Ss.SymbolScale = 1
        Ss.SymbolRotation = MagneticToTrue(pos, m, chTwd.Valori(i))
      End If
      Dim F As New Feature
      F.Geometry = Mapsui.Projection.SphericalMercator.FromLonLat(pos.LngDec, pos.LatDec)
      F.Styles.Add(Ss)
      Lista.Add(F)
      Prossimo = m.AddSeconds(Passo)
    Next
    Return Lista
  End Function

#End Region

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
    Dim LastKey As Integer = ChiaveColore(ColoreDaSailingMode(LastSailingMode))
    Dim UltimoVertice As Mapsui.Geometries.Point = Nothing
    Dim ValoriColore As Double() = PreparaValoriColore()

    Dim lastvalidgeopoint As clsGeographicPosition = Nothing
    Dim lastvalidmoment As DateTime = DataProvider2020.TimeRange.Start
    Dim d As New clsCalcoliDuePuntiGeo()
    Dim maxbsms As Double = 50
    Dim iDa As Integer, iA As Integer
    LimitiTraccia(iDa, iA)
    ' TimeStamps(0) puo' essere vuoto (DateTime.MinValue): AddSeconds(-1) solleverebbe un'eccezione
    Dim mprev As DateTime = If(iDa = 0, DataProvider2020.TimeRange.Start, DataProvider2020.TimeStamps(iDa)).AddSeconds(-1)
    For i As Integer = iDa To iA
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
          Dim ActualColor As Color
          If _ModoColore = 0 OrElse ValoriColore Is Nothing Then
            If _ModoColore = 0 Then
              ActualColor = ColoreDaSailingMode(SailingMode(chTwa.Valori(i), chBs.Valori(i)))
            Else
              ActualColor = Color.Gray
            End If
          Else
            ActualColor = ColoreDaValore(ValoriColore(i))
          End If
          Dim ActualKey As Integer = ChiaveColore(ActualColor)
          If Not ActualKey = LastKey Then
            TracciaTmp.Add(FcTmp)
            FcTmp = New Feature
            linea = New LineString
            FcTmp.Geometry = linea
            ' con i colori continui il nuovo tratto riparte dall'ultimo punto, per non lasciare buchi
            If _ModoColore <> 0 AndAlso UltimoVertice IsNot Nothing Then linea.Vertices.Add(UltimoVertice)
            St = New VectorStyle
            St.Line = New Pen(ActualColor, 2)
            'St.Line = New Pen(Color.Red, 2)
            FcTmp.Styles.Add(St)
            LastKey = ActualKey
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
              UltimoVertice = punto
              lastvalidgeopoint = geopoint
              lastvalidmoment = DataProvider2020.TimeStamps(i)
            End If
          End If
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

  Public Sub AggiornaSelezione(TimeRange As clsTimeRange)
    TRselezione = TimeRange
    ' l'evidenziazione dorata e' stata sostituita dal toggle Sel (traccia del solo periodo visualizzato)
    pLayerSelezione.Enabled = False
    If _SoloSelezione Then AggiornaColoreTraccia()
  End Sub

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