'Imports System.Data.Odbc
Imports System.Collections.ObjectModel
Imports System.ComponentModel
'Imports System.Data.SQLite
Imports System.Data
Imports System.Data.Odbc
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Management
Imports System.Net.Mail
Imports System.Numerics
Imports System.Xml
Imports Accord.Math
Imports GeoTimeZone
Imports Mapsui.Providers
Imports Newtonsoft.Json
Imports PropertyChanged
Imports SciChart.Charting2D.Interop
Imports SPwpf
Imports TimeZoneConverter




Module GpxWriter
  ''' <summary>
  ''' Saves a single waypoint to a GPX 1.1 file.
  ''' </summary>
  Public Sub SaveGpxWaypoint(filePath As String, lat As Double, lon As Double, wpName As String)
    Dim settings As New XmlWriterSettings With {
            .Indent = True,
            .Encoding = System.Text.Encoding.UTF8,
            .NewLineOnAttributes = False
        }

    Using writer As XmlWriter = XmlWriter.Create(filePath, settings)
      Dim ns As String = "http://www.topografix.com/GPX/1/1"

      writer.WriteStartDocument()
      writer.WriteStartElement("gpx", ns)
      writer.WriteAttributeString("version", "1.1")
      writer.WriteAttributeString("creator", "YourAppName")
      writer.WriteAttributeString("xmlns", "xsi", Nothing, "http://www.w3.org/2001/XMLSchema-instance")
      writer.WriteAttributeString("xsi", "schemaLocation",
                "http://www.w3.org/2001/XMLSchema-instance",
                "http://www.topografix.com/GPX/1/1 " &
                "http://www.topografix.com/GPX/1/1/gpx.xsd")

      ' <wpt lat=".." lon="..">
      writer.WriteStartElement("wpt", ns)
      writer.WriteAttributeString("lat", lat.ToString(CultureInfo.InvariantCulture))
      writer.WriteAttributeString("lon", lon.ToString(CultureInfo.InvariantCulture))

      ' optional: time (UTC ISO 8601)
      writer.WriteElementString("time", ns, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))

      ' <name>..</name>
      If Not String.IsNullOrWhiteSpace(wpName) Then
        writer.WriteElementString("name", ns, wpName)
      End If

      writer.WriteEndElement() ' </wpt>
      writer.WriteEndElement() ' </gpx>
      writer.WriteEndDocument()
    End Using
  End Sub
End Module

Public Class clsTable1D
  Public Property Values As Dictionary(Of Double, Double)
  <JsonIgnore>
  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation

  Public Sub SetValues(RefChannelValues As Double(), ValChannelValues As Double())
    For i As Integer = 0 To RefChannelValues.Count - 1
      Values.Add(RefChannelValues(i), ValChannelValues(i))
    Next
  End Sub

  Public Function GetValue(RefChannelValue As Double) As Double
    _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(GetRefChannelArray(), GetValChannelArray())
    Return _Interpolatore.Interpolate(RefChannelValue)
  End Function

  Public Function GetRefChannelArray() As Double()
    Dim l As New List(Of Double)
    For Each k In Values.Keys
      l.Add(k)
    Next
    Return l.ToArray
  End Function

  Public Function GetValChannelArray() As Double()
    Dim l As New List(Of Double)
    For Each v In Values.Values
      l.Add(v)
    Next
    Return l.ToArray
  End Function


End Class

Public Class clsTable2D
  Public Property Values As Double(,) 'la prima riga sono i valori del canale x, la prima colonna sono i valori del canale y
  '0,0 e' dunque vuoto
  <JsonIgnore>
  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation
  <JsonIgnore>
  Dim XHeaders() As Double = Nothing
  <JsonIgnore>
  Dim YHeaders() As Double = Nothing

  Public Sub SetValues(Values As Double(,))
    'exempio tabella x:tws , y:twa , values:heel
    Me.Values = Values
    SetHeaders(True)
  End Sub

  Public Function GetXChannelArray() As Double()
    Return XHeaders
  End Function

  Public Function GetYChannelArray() As Double()
    Return YHeaders
  End Function

  Private Sub SetHeaders(Optional Force As Boolean = False)
    If XHeaders Is Nothing OrElse Force Then
      ReDim XHeaders(Values.GetUpperBound(0) - 2)
      ReDim YHeaders(Values.GetUpperBound(1) - 2)
      'mette in un array i valori dell intestazione y ed i intepolati di x
      For i As Integer = 1 To Values.GetUpperBound(0) - 1
        XHeaders(i - 1) = Values(i, 0)
      Next
      For i As Integer = 1 To Values.GetUpperBound(1) - 1
        YHeaders(i - 1) = Values(0, i)
      Next
    End If

  End Sub

  Public Function GetValue(XChannelValue As Double, YChannelValue As Double) As Double
    SetHeaders()

    Dim XTmp(Values.GetUpperBound(1) - 2) As Double
    Dim YTmp(Values.GetUpperBound(0) - 2) As Double
    For y As Integer = 1 To Values.GetUpperBound(1) - 1
      For x As Integer = 1 To Values.GetUpperBound(0) - 1
        XTmp(y - 1) = Values(0, y)
      Next
      _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(XHeaders, XTmp)
      YTmp(y - 1) = _Interpolatore.Interpolate(XChannelValue)
    Next
    _Interpolatore = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(YHeaders, YTmp)
    Return _Interpolatore.Interpolate(YChannelValue)
  End Function


End Class

Public Class clsGeoCalculations

  'Private Shared Function CalcolaDistanza(Point1 As clsGeograficPosition, Point2 As clsGeograficPosition) As Double
  '  Dim RT As Double = 6372795.477598 ' Metri
  '  Return RT * System.Math.Acos((System.Math.Sin(Point1.LatR) * System.Math.Sin(Point2.LatR)) + (System.Math.Cos(Point1.LatR) * System.Math.Cos(Point2.LatR) * System.Math.Cos(Point1.LngR - Point2.LngR)))
  'End Function


  Public Shared Function DistanceMeters(Point1 As clsGeographicPosition, Point2 As clsGeographicPosition) As Double
    Return DistanceMeters(Point1.LatDec, Point1.LngDec, Point2.LatDec, Point2.LngDec) * 1000
  End Function

  Public Shared Function DistanceMeters(ByVal lat1 As Double, ByVal lon1 As Double,
             ByVal lat2 As Double, ByVal lon2 As Double) As Double
    'Haversine
    Dim R As Double = 6372.795477598 'earth radius in km
    Dim dLat As Double
    Dim dLon As Double
    Dim a As Double
    Dim c As Double
    'Dim d As Double
    dLat = Radians(lat2 - lat1)
    dLon = Radians((lon2 - lon1))
    a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Radians(lat1)) *
      Math.Cos(Radians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))
    Return R * c
  End Function


  Public Shared Function BearingDegreesFast(Point1 As clsGeographicPosition, Point2 As clsGeographicPosition) As Double
    'CalcolaRottaOLD()
    'Exit Sub
    'Dim DeltaPhi As Double = System.Math.Log(System.Math.Tan(pPuntoB.LatR / 2 + System.Math.PI / 4) / System.Math.Tan(pPuntoA.LatR / 2 + System.Math.PI / 4))
    'Dim DeltaLon As Double = System.Math.Abs(pPuntoA.LngR - pPuntoB.LngR)
    'Dim RottaRad As Double = System.Math.Atan2(DeltaLon, DeltaPhi)
    'pRotta = pObjCalcoli.RadiantiToGradi(RottaRad)

    Dim pDlatMediaD As Double = (Point1.LatDec + Point2.LatDec) / 2
    Dim pDlatD As Double = Point2.LatDec - Point1.LatDec
    Dim pDlongD As Double = Point2.LngDec - Point1.LngDec
    If pDlatD = 0 Then
      If pDlongD > 0 Then
        Return 90
      ElseIf pDlongD < 0 Then
        Return 270
      Else
        Return 0
      End If
    ElseIf pDlongD = 0 Then
      If pDlatD > 0 Then
        Return 0
      ElseIf pDlatD < 0 Then
        Return 180
      Else
        Return 0
      End If
    Else

    End If

    Dim pDlatMedia As Double = Radians(pDlatMediaD)
    Dim pDlat As Double = Radians(pDlatD)
    Dim pDlong As Double = Radians(pDlongD)
    Dim pRt As Double = System.Math.Atan(pDlong * System.Math.Cos(pDlatMedia) / pDlat)
    ' Vale Nell'emisfero NORD va verificato con il sud
    If Point2.LatDec > Point1.LatDec Then
      If pRt < 0 Then pRt += System.Math.PI * 2
    Else
      pRt += System.Math.PI
    End If

    'If pRt < 0 Then pRt += System.Math.PI * 2
    Return Degrees(pRt)
  End Function


  Public Shared Function BearingDegrees(Point1 As clsGeographicPosition, Point2 As clsGeographicPosition) As Double

    Dim y As Double = System.Math.Sin(Point2.LngR - Point1.LngR) * System.Math.Cos(Point2.LatR)

    Dim x As Double = System.Math.Cos(Point1.LatR) * System.Math.Sin(Point2.LatR)
    x -= System.Math.Sin(Point1.LatR) * System.Math.Cos(Point2.LatR) * System.Math.Cos(Point2.LngR - Point1.LngR)


    Dim Theta As Double = System.Math.Atan2(y, x)
    If Theta < 0 Then Theta += 2 * System.Math.PI
    Return Degrees(Theta)
  End Function

  Public Shared Function IntestectionPoint(Point1 As clsGeographicPosition, Route1 As Double, Point2 As clsGeographicPosition, Route2 As Double) As clsGeographicPosition
    Dim Dist As Double = 100
    Dim Dest1 As clsGeographicPosition = PuntoDestinazione(Point1, Dist, Route1)
    Dim Point1XY As New clsXYdouble(0, 0) ' = Ref Point
    Dim Dest1XY As clsXYdouble = CalculatesRelativePositionXY(Dest1, Point1, 0)

    Dim Dest2 As clsGeographicPosition = PuntoDestinazione(Point2, Dist, Route2)
    Dim Point2XY As clsXYdouble = CalculatesRelativePositionXY(Point2, Point1, 0)
    Dim Dest2XY As clsXYdouble = CalculatesRelativePositionXY(Dest2, Point1, 0)

    Dim PuntoIntersezione As clsXYdouble = XYIntersectionPoint(Point1XY, Dest1XY, Point2XY, Dest2XY)
    If Double.IsNaN(PuntoIntersezione.X) Then Return Nothing

    Dist = Math.Sqrt(Math.Pow(PuntoIntersezione.X - Point1XY.X, 2) + Math.Pow(PuntoIntersezione.Y - Point1XY.Y, 2))
    Dim angR As Double = Math.Atan2(PuntoIntersezione.Y - Point1XY.Y, PuntoIntersezione.X - Point1XY.X)
    Dim Brg As Double = BearingToCartesianAndViceVersa(Degrees(angR))
    Return PuntoDestinazione(Point1, Dist, Brg)
  End Function



  Public Shared Function XYIntersectionPoint(s1 As clsXYdouble, e1 As clsXYdouble, s2 As clsXYdouble, e2 As clsXYdouble) As clsXYdouble
    Dim a1 = e1.Y - s1.Y
    Dim b1 = s1.X - e1.X
    Dim c1 = a1 * s1.X + b1 * s1.Y

    Dim a2 = e2.Y - s2.Y
    Dim b2 = s2.X - e2.X
    Dim c2 = a2 * s2.X + b2 * s2.Y

    Dim delta = a1 * b2 - a2 * b1

    'If lines are parallel, the result will be (NaN, NaN).
    Return If(delta = 0, New clsXYdouble(Single.NaN, Single.NaN), New clsXYdouble((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta))
  End Function


  Public Shared Function CalculatesRelativePositionXY(DestinationPosition As clsGeographicPosition, RefPosition As clsGeographicPosition, Axis As Double) As clsXYdouble
    Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(RefPosition, DestinationPosition)
    Dim DistMt As Double = clsGeoCalculations.DistanceMeters(RefPosition, DestinationPosition)
    Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Axis, BrgDeg)
    Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
    Dim X As Double = DistMt * System.Math.Cos(Radians(CartBearing))
    Dim Y As Double = DistMt * System.Math.Sin(Radians(CartBearing))
    Return New clsXYdouble(X, Y)
  End Function


  Public Shared Sub TestRnB()
    'Dim Centro As New clsGeograficPosition(-10, 10)
    'Dim Punto As clsGeograficPosition
    'Dim R1, B1, R2, B2 As Double
    'For i As Integer = 0 To 7
    '  Select Case i
    '    Case 0
    '      Punto = New clsGeograficPosition(-11, 10)
    '    Case 1
    '      Punto = New clsGeograficPosition(-11, 11)
    '    Case 2
    '      Punto = New clsGeograficPosition(-10, 11)
    '    Case 3
    '      Punto = New clsGeograficPosition(-9, 11)
    '    Case 4
    '      Punto = New clsGeograficPosition(-9, 10)
    '    Case 5
    '      Punto = New clsGeograficPosition(-9, 9)
    '    Case 6
    '      Punto = New clsGeograficPosition(-10, 9)
    '    Case 7
    '      Punto = New clsGeograficPosition(-11, 9)
    '  End Select
    '  R1 = CalcolaDistanza(Centro, Punto)
    '  B1 = BearingDegreesFast(Centro, Punto)

    '  R2 = DistanceMeters(Centro, Punto)
    '  B2 = BearingDegrees(Centro, Punto)

    '  Console.WriteLine(R1.ToString("F0") & " " & R2.ToString("F0") & " " & B1.ToString("F1") & " " & B2.ToString("F1"))
    '  'Stop
    'Next
    ''Stop
    'Dim adesso As DateTime = Now
    'For i As Integer = 0 To 4000000
    '  R1 = CalcolaDistanza(Punto, Centro)
    '  'B1 = BearingDegrees(Punto, Centro)
    'Next
    'Console.WriteLine("R1 " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    'adesso = Now
    'For i As Integer = 0 To 4000000
    '  R2 = DistanceMeters(Punto, Centro)
    '  'B2 = Bearing(Punto, Centro)
    'Next
    'Console.WriteLine("R2 " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    'adesso = Now
    'For i As Integer = 0 To 4000000
    '  B1 = BearingDegrees(Punto, Centro)
    'Next
    'Console.WriteLine("B1 " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    'adesso = Now
    'For i As Integer = 0 To 4000000
    '  B2 = Bearing(Punto, Centro)
    'Next
    'Console.WriteLine("B2 " & Now.Subtract(adesso).TotalSeconds.ToString("F3"))
    'Stop
  End Sub

  'Public Shared Sub ProvaBearingToCartesianAndViceversa()
  '  For i As Integer = 0 To 360 Step 10
  '    Console.WriteLine("Brg: " & i.ToString("F0").ToString.PadLeft(3, "0") & " Cart:" & BearingToCartesian(i).ToString("F0").ToString.PadLeft(3, "0"))
  '  Next
  '  Stop
  '  Console.WriteLine()
  '  Console.WriteLine()
  '  Console.WriteLine()
  '  For i As Integer = 0 To 360 Step 10
  '    Console.WriteLine("Cart: " & i.ToString("F0").ToString.PadLeft(3, "0") & " Brg:" & CartesianToBearing(i).ToString("F0").ToString.PadLeft(3, "0"))
  '  Next
  '  Stop
  'End Sub

  Public Shared Function StrLatLonDfwToDeg(Coordinates As String) As Double
    Dim dmPos As String = Coordinates.Split(".")(0)
    Dim mdPos As String = Coordinates.Split(".")(1)
    Dim sdm As Integer = 2
    If dmPos.Length = 5 Then
      sdm = 3
    End If
    Dim d As Double = Coordinates.Substring(0, sdm)
    Dim m As Double = Coordinates.Substring(sdm, 2)

    Dim s As Integer = 1
    If mdPos.EndsWith("S") Then
      s = -1
    ElseIf mdPos.EndsWith("W") Then
      s = -1
    End If
    mdPos = mdPos.Remove(mdPos.Length - 1)
    mdPos = "0." & mdPos
    Dim mm As Double = (m + CDbl(mdPos)) / 60
    Return s * (d + mm)
  End Function

  'Private Function ConvGradiGeografici(ByRef Gradi As String) As String
  '  ' questa funzione converte le coordinate Lat e Long da minuti a centesimi di Grado
  '  'Gradi = Gradi
  '  If InStr(Gradi, ".") = 5 Then ' è una longitudine 4 cifre
  '    Return CDbl(Left(CStr(Gradi), 2)) + CDbl(Mid(CStr(Gradi), 3) / 60)
  '  ElseIf InStr(Gradi, ".") = 6 Then ' è una latitudine 5 cifre
  '    Return CDbl(Left(CStr(Gradi), 3)) + CDbl(Mid(CStr(Gradi), 4) / 60)
  '  Else
  '    Return CDbl(0)
  '  End If
  'End Function


  Public Shared Function BearingToCartesianAndViceVersa(Degrees As Double) As Double
    'vale sia per la conversione da bearing a cartesiano che vice versa
    Dim BRGmod As Single = SommaAngolo180adAngolo360(-90, Degrees)
    BRGmod -= 360
    BRGmod *= -1
    Return BRGmod
  End Function


  Public Shared Function PuntoDestinazione(PuntoOrig As clsGeographicPosition, Distanza As Double, Rotta As Double) As clsGeographicPosition
    Dim RT As Double = 6372795.477598 ' Metri
    Dim LatDestR As Double
    Dim LongDestR As Double
    Dim RottaRad As Double = Radians(Rotta)
    LatDestR = System.Math.Asin(System.Math.Sin(PuntoOrig.LatR) * System.Math.Cos(Distanza / RT) + System.Math.Cos(PuntoOrig.LatR) * System.Math.Sin(Distanza / RT) * System.Math.Cos(RottaRad))
    LongDestR = PuntoOrig.LngR + System.Math.Atan2(System.Math.Sin(RottaRad) * System.Math.Sin(Distanza / RT) * System.Math.Cos(PuntoOrig.LatR), System.Math.Cos(Distanza / RT) - System.Math.Sin(PuntoOrig.LatR) * System.Math.Sin(LatDestR))
    Return New clsGeographicPosition(Degrees(LatDestR), Degrees(LongDestR), PuntoOrig.IsValid)
  End Function



  Public Shared Function PolygonAroundGeoPoint(point As clsGeographicPosition, polar As clsTgt, WindAxis As Double, Tws As Double, Seconds As Double)

    Dim polygon As New List(Of clsGeographicPosition)
    Dim StepDegrees As Integer = 5 ' Number of points to create a circle


    For i As Integer = 0 To 360 Step StepDegrees
      Dim Brg = SommaAngolo180adAngolo360(WindAxis, i)
      Dim Twa As Double = i
      If Twa > 180 Then
        Twa -= 360
      End If
      Dim Rng = 0
      Dim TwaTgtUp As Double = polar.ValoreTgtUp(Tws, "bs").Twa
      Dim TwaTgtDn As Double = polar.ValoreTgtDn(Tws, "bs").Twa
      If Math.Abs(Twa) <= TwaTgtUp Then
        Stop 'la velocita'e'il vmg non la bs piu'il costo di una manovra
      ElseIf Math.Abs(Twa) >= TwaTgtUp Then
        Stop 'la velocita'e'il vmg non la bs piu'il costo di una manovra
      Else
        Rng = polar.Polare("bs").PolarValue(Tws, Math.Abs(Twa))
      End If
      Rng = KtsToMS(Rng) * Seconds ' Convert to meters

      Dim PuntoTmp As clsGeographicPosition = clsGeoCalculations.PuntoDestinazione(point, Rng, Brg)
      polygon.Add(New clsGeographicPosition(PuntoTmp.LatDec, PuntoTmp.LngDec, True))
    Next
    Return polygon
  End Function



  Public Function IsPointInPolygon(point As clsGeographicPosition, polygon As List(Of clsGeographicPosition)) As Boolean
    Dim n As Integer = polygon.Count
    Dim inside As Boolean = False

    For i As Integer = 0 To n - 1
      Dim j As Integer = (i + n - 1) Mod n

      Dim xi As Double = polygon(i).LngDec
      Dim yi As Double = polygon(i).LatDec
      Dim xj As Double = polygon(j).LngDec
      Dim yj As Double = polygon(j).LatDec

      Dim intersect As Boolean = ((yi > point.LatDec) <> (yj > point.LngDec)) AndAlso
                (point.LngDec < (xj - xi) * (point.LatDec - yi) / ((yj - yi) + 0.0000000001) + xi)

      If intersect Then inside = Not inside
    Next

    Return inside
  End Function


End Class

Public Class clsKillerSeriale

  Public Shared Function SaveConfigurationGeneric(Of T)(Oggetto As T, FilePath As String) As Boolean
    Dim sw As System.IO.StreamWriter
    Try
      sw = System.IO.File.CreateText(FilePath)
      'Dim JS As New Newtonsoft.Json.JsonSerializer
      Dim Setting = New Newtonsoft.Json.JsonSerializerSettings
      Setting.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
      Dim serializzatore = Newtonsoft.Json.JsonConvert.SerializeObject(Oggetto, Newtonsoft.Json.Formatting.Indented, Setting)
      sw.Write(serializzatore)
      sw.Dispose()
      Return True
    Catch ex As Exception
      sw.Dispose()
      Return False
    End Try
  End Function

  Public Shared Function LoadConfigurationGeneric(Of T)(FilePath As String) As T
    Try
      If Not System.IO.File.Exists(FilePath) Then
        Return Nothing
      End If

      Dim T0 As DateTime = Now

      Dim Setting = New Newtonsoft.Json.JsonSerializerSettings
      Setting.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
      ' Formatting non ha effetto in lettura: rimosso

      ' Streaming: niente ReadToEnd, evita di materializzare l'intero file in una stringa
      Dim Risultato As T
      Using sr As New System.IO.StreamReader(FilePath, System.Text.Encoding.UTF8, True, 65536)
        Using jtr As New Newtonsoft.Json.JsonTextReader(sr)
          Dim Serializzatore = Newtonsoft.Json.JsonSerializer.Create(Setting)
          Risultato = Serializzatore.Deserialize(Of T)(jtr)
        End Using
      End Using

      If dbg Then
        Dim fi As New System.IO.FileInfo(FilePath)
        Console.WriteLine("LoadConfig " & fi.Name & " " & (fi.Length \ 1024) & "KB in " &
                          Now.Subtract(T0).TotalMilliseconds.ToString("F0") & " ms")
      End If

      Return Risultato
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  'Public Shared Function LoadConfigurationGeneric(Of T)(FilePath As String) As T
  '  Try
  '    If Not System.IO.File.Exists(FilePath) Then
  '      Return Nothing
  '    End If
  '    Dim sr As New System.IO.StreamReader(FilePath)
  '    'Dim JS As New Newtonsoft.Json.JsonSerializer
  '    Dim Setting = New Newtonsoft.Json.JsonSerializerSettings
  '    Setting.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
  '    Setting.Formatting = Newtonsoft.Json.Formatting.Indented

  '    Dim Stringa = sr.ReadToEnd
  '    Dim Risultato = Newtonsoft.Json.JsonConvert.DeserializeObject(Of T)(Stringa, Setting)
  '    sr.Close()
  '    Return Risultato
  '  Catch ex As Exception
  '    Return Nothing
  '  End Try
  'End Function

  Public Shared Function TryLoadConfiguration(Of T As Class)(
    filePath As String,
    ByRef result As T,
    Optional ByRef errorMessage As String = Nothing) As Boolean

    result = Nothing
    If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
      errorMessage = "File not found."
      Return False
    End If

    Dim settings As New JsonSerializerSettings With {
    .TypeNameHandling = TypeNameHandling.None,
    .MissingMemberHandling = MissingMemberHandling.Ignore
  }

    Try
      Using sr As New StreamReader(filePath, System.Text.Encoding.UTF8, True)
        Using jtr As New JsonTextReader(sr)
          Dim serializer = JsonSerializer.Create(settings)
          result = serializer.Deserialize(Of T)(jtr)
          Return result IsNot Nothing
        End Using
      End Using
    Catch ex As Exception
      errorMessage = ex.Message
      Return False
    End Try
  End Function


End Class

Public Class clsEmail

  Public Shared Function SendEmail(AttachPath As String) As Boolean
    Dim fi As New System.IO.FileInfo(AttachPath)

    If AppConfig.ActiveProfile.MailingListSettings Is Nothing Then
      AppConfig.ActiveProfile.MailingListSettings = New clsMailingListSettings
      AppConfig.ActiveProfile.MailingListSettings.SetDefaultForTesting()
      'AppConfig.ActiveProfile.MailingListSettings.SetDefault()
      'AppConfig.Salva()
    End If
    Dim Subject As String = "SailingPerformer " & fi.Name.Replace(fi.Extension, "")
    Dim Body As String = "Please find report pdf in attach"
    Dim MailSender As String = AppConfig.ActiveProfile.MailingListSettings.MailSender
    Dim MailList As List(Of String) = AppConfig.ActiveProfile.MailingListSettings.MailingList
    Dim SmtpServerAddress As String = AppConfig.ActiveProfile.MailingListSettings.SmtpServerAddress
    Dim SmtpPort As Integer = AppConfig.ActiveProfile.MailingListSettings.SmtpServerPort
    Dim SmtpAccount As String = AppConfig.ActiveProfile.MailingListSettings.SmtpAccount
    Dim SmtpPassword As String = AppConfig.ActiveProfile.MailingListSettings.SmtpPassword
    Return SendEmail(Subject, Body, AttachPath, MailSender, MailList, SmtpServerAddress, SmtpPort, SmtpAccount, SmtpPassword)
  End Function
  Public Shared Function SendEmail(Subject As String, Body As String, AttachPath As String, MailSender As String, MailList As List(Of String), SmtpServerAddress As String, SmtpPort As Integer, SmtpAccount As String, SmtpPassword As String) As Boolean
    Try
      Dim mail As MailMessage = New MailMessage()
      Dim SmtpServer As SmtpClient = New SmtpClient(SmtpServerAddress)
      mail.From = New MailAddress(MailSender)
      For Each d In MailList
        mail.[To].Add(d)
      Next
      mail.Subject = Subject
      mail.Body = Body
      Dim attachment As System.Net.Mail.Attachment
      attachment = New System.Net.Mail.Attachment(AttachPath)
      mail.Attachments.Add(attachment)
      SmtpServer.Port = SmtpPort
      SmtpServer.Credentials = New System.Net.NetworkCredential(SmtpAccount, SmtpPassword)
      SmtpServer.EnableSsl = True
      SmtpServer.Send(mail)
      Return True
    Catch ex As Exception
      Return False
    End Try
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
  End Function



End Class


Module mdlCommon
  'WithEvents pDataProvider As clsDataProvider2020

  Dim pObjFiles2020 As New clsFiles2020
  Dim _DataProvider2020 As clsDataProvider2020
  Dim _PeriodsManager As New clsPeriodsManager2021

  Public dbg As Boolean = True


  'Dim pObjSCpolar As clsSciChart
  'Dim pAppConfig As clsSettings
  'Dim pTargets As clsPolare2019
  Public Property TgtManager As clsTgtManager
  Public Property BenchManager As New clsBenchmarksManager
  'Dim _ManBenchManager As clsManoeuversBenchmarks
  'Dim _AccBenchManager As clsAccelerationBenchmarks

  'Dim pTrackSC As clsSciChart
  Dim pMatriceControlliBase As clsMatriceControlliSinglePeriod
  'Dim pMatriceControlliDettagli As clsMatriceControlliSinglePeriod
  Dim pSailingState As New clsSailingState


  Dim pMetriMiglioNautico As Double = 1852
  Dim pMetriPerBoatLenght As Double = 7.5
  Dim pUTCOffSet As Double = 2
  Dim pEpoch As New DateTime(1970, 1, 1)

  'Public Property BoatLengthMt As Double = 15.85

  'WithEvents pObjChartSyncManagerDetails As New clsChartSyncManager
  WithEvents _DataPlotSync As New clsChartSyncManager

  WithEvents pGraficoEventiViewModel As clsChartEventiViewModel ' New clsChartEventiViewModel

  'Dim pPeriodsManager As New clsPeriodsManager2020


  Public meCultureUIInfo As System.Globalization.CultureInfo
  Public meCultureInfo As System.Globalization.CultureInfo
  WithEvents pObjFiles As New clsFiles

  'WithEvents pMessaggioStatusBar As New clsMessaggio

  Public ObjContaTempo As New clsContaTempo

  Dim pFormatiFaro As String() = {"yyyyMMddHmmss.ffff", "yyyyMMddHmmss.fff", "yyyyMMddHmmss.ff", "yyyyMMddHmmss.f", "yyyyMMddHmmss"}

  Dim pListaColori As List(Of Color)
  Dim pStpW As New Stopwatch
  Dim pRigheStopWatch As New List(Of String)

  Dim pMapControl As clsGestioneMapsui
  'Dim pMapsuiUtilities As New clsMapsuiUtilities

  'Dim pPedestalControl As New clsCalcoliPedestals

  Dim _PavarotVisibleRange As New clsDoubleRange(0, 0)
  Dim _AccVisibleRange As New clsDoubleRange(0, 0)

  Dim _SelectedPoints As New List(Of DateTime)

  Dim _ExpStarts As New clsExpeditionStarts


  Public Property FormatiFaro As String()
    Get
      Return pFormatiFaro
    End Get
    Set(Value As String())
      pFormatiFaro = Value
    End Set
  End Property

  'Public Property MessaggioStatusBar As clsMessaggio
  '  Get
  '    Return pMessaggioStatusBar
  '  End Get
  '  Set(Value As clsMessaggio)
  '    pMessaggioStatusBar = Value
  '  End Set
  'End Property

  Public Property DataPlotSync As clsChartSyncManager
    Get
      Return _DataPlotSync
    End Get
    Set(Value As clsChartSyncManager)
      _DataPlotSync = Value
    End Set
  End Property

  Public Property ObjFiles As clsFiles
    Get
      Return pObjFiles
    End Get
    Set(Value As clsFiles)
      pObjFiles = Value
    End Set
  End Property


  Public Property AppConfig As clsSettings2021


  Public Property PeriodsManager As clsPeriodsManager2021
    Get
      Return _PeriodsManager
    End Get
    Set(value As clsPeriodsManager2021)
      _PeriodsManager = value
    End Set
  End Property


  Public Function CanaleTackDefault() As clsChannel2020
    Return DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
  End Function


  Public Sub LoadingProgressVisualizza()
    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait '// Set the cursor To loading spinner
  End Sub

  Public Sub LoadingProgressNascondi()
    Mouse.OverrideCursor = Nothing ' // Set the cursor back To arrow
  End Sub

  Public Sub AggiornaSelezionePeriodo(TR As clsTimeRange, IR As SciChart.Data.Model.DateRange)
    GraficoEventiViewModel.AggiornaSelezione(TR)
    DataPlotSync.SharedXVisibleRange = IR
    MapControl.AggiornaSelezione(TR)
    DataPlotSync.AggiornaYrange()
  End Sub


  Private Function ParteComuneDelNome(PathFiles As List(Of String)) As String
    Dim strTmp As String = ""
    Dim Nomi As New List(Of String)
    For Each fl In PathFiles
      Dim FI As New System.IO.FileInfo(fl)
      Nomi.Add(FI.Name)
    Next
    If Nomi.First.Length > 1 Then
      Dim subStr As String ' = Nomi.First.Substring(0, 1)
      For i As Integer = 1 To Nomi.First.Length - 1
        subStr = Nomi.First.Substring(0, i)
        Dim SubstringUguale As Boolean = True
        For Each Nome In Nomi
          If Not subStr = Nome.Substring(0, i) Then
            SubstringUguale = False
            Exit For
          End If
        Next
        If SubstringUguale Then
          strTmp = subStr
        Else
          Exit For
        End If
      Next
    End If
    If strTmp.Trim = "" Then Return TempoInStringaFormattata(Now, eFormatType.YYYYMMDDHHMMSS)
    Return strTmp.TrimEnd("_")
  End Function




  Public Property GraficoEventiViewModel As clsChartEventiViewModel

    Get
      Return pGraficoEventiViewModel
    End Get
    Set(value As clsChartEventiViewModel)
      pGraficoEventiViewModel = value
    End Set
  End Property


  Public Property MatriceControlliBase As clsMatriceControlliSinglePeriod
    Get
      Return pMatriceControlliBase
    End Get
    Set(value As clsMatriceControlliSinglePeriod)
      pMatriceControlliBase = value
    End Set
  End Property


  Public Property SailingState As clsSailingState
    Get
      Return pSailingState
    End Get
    Set(value As clsSailingState)
      pSailingState = value
    End Set
  End Property

  Public Enum eFormatType
    YYYYMMDDHHMMSS = 0
    Gomboc = 1
    YYYYMMDD = 2
  End Enum

  Public Property UTCOffSet() As Double
    Get
      Return pUTCOffSet
    End Get
    Set(value As Double)
      pUTCOffSet = value
    End Set
  End Property

  Public Function BRGinCartesiano(BRGbussola As Double) As Double
    Dim BRGmod As Single = SommaAngolo180adAngolo360(-90, BRGbussola)
    BRGmod -= 360
    BRGmod *= -1
    Return BRGmod
  End Function

  Public Property MetriPerBoatLenght As Double
    Get
      Return pMetriPerBoatLenght
    End Get
    Set(value As Double)
      pMetriPerBoatLenght = value
    End Set
  End Property

  Public ReadOnly Property MetriMiglioNautico As Double
    Get
      Return pMetriMiglioNautico
    End Get
  End Property

  Public Property Epoch As Date
    Get
      Return pEpoch
    End Get
    Set(value As Date)
      pEpoch = value
    End Set
  End Property

  Public Enum eUnitaMisura
    eMetri = 0
    eLunghezza = 1
    eMiglia = 2
    eGradiGeo = 3
  End Enum

  Public Function KtsToMS(SpeedKts As Double) As Double
    Return SpeedKts * 1852 / 3600
  End Function

  Public Function MsToKts(SpeedMS As Double) As Double
    Return SpeedMS / 1852 * 3600
  End Function

  Public Function FaroDateAndSecFromMidnightToSystemDT(Day As DateTime, StringaSecFromMidNight As String) As DateTime
    Dim secDbl As Double
    If Double.TryParse(StringaSecFromMidNight, secDbl) Then
      Return Day.AddSeconds(secDbl)
    End If
    Return Nothing
    'If Not IsNumeric(StringaSecFromMidNight) Then Return Nothing
    'Return Day.AddSeconds(CDbl(StringaSecFromMidNight))
  End Function


  Public Function FaroDateTimeToSystemDT(StringaData As String, StringaTime As String) As DateTime
    If Not IsNumeric(StringaData) Then Return Nothing
    If Not IsNumeric(StringaTime) Then Return Nothing
    Return DateTime.ParseExact(StringaData & StringaTime, FormatiFaro, Nothing, Globalization.DateTimeStyles.None)

    Dim hms As String = StringaTime.Split(".")(0)
    hms = hms.PadLeft(6, "0")
    If StringaTime.IndexOf(".") > -1 Then
      'Dim fff As String = StringaTime.Split(".")(1)
      'fff = fff.PadRight(3, "0")
      StringaTime = hms & "." & StringaTime.Split(".")(1)
    Else
      StringaTime = hms & ".000"
    End If
    'Dim dt As DateTime
    'DateTime.TryParseExact(StringaData & hms & "." & fff, "yyyyMMddHmmss.fff", meCultureInfo, Globalization.DateTimeStyles.None, dt)

    Return DateTime.ParseExact(StringaData & StringaTime, FormatiFaro, Nothing, Globalization.DateTimeStyles.None)



  End Function

  Public Function CalculatesRelativePositionXY(DestinationPosition As clsGeographicPosition, RefPosition As clsGeographicPosition, Axis As Double, Colore As Color) As clsXYpoint
    Dim BrgDeg As Double = clsGeoCalculations.BearingDegrees(RefPosition, DestinationPosition)
    Dim DistMt As Double = clsGeoCalculations.DistanceMeters(RefPosition, DestinationPosition)
    'Dim RelativeBearing As Double = SommaAngolo180adAngolo360(Axis, BrgDeg)
    Dim RelativeBearing As Double = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(Axis, BrgDeg)
    Dim CartBearing As Double = clsGeoCalculations.BearingToCartesianAndViceVersa(RelativeBearing)
    Dim X As Double = DistMt * System.Math.Cos(Radians(CartBearing))
    Dim Y As Double = DistMt * System.Math.Sin(Radians(CartBearing))
    Return New clsXYpoint(X, Y, Colore)
  End Function

  Public Function ConvertToLocalTime(lat As Double, lng As Double, UtcTime As DateTime) As DateTime
    Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
    Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
    Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(UtcTime, tzInfo)
    Return convertedTime.LocalDateTime
  End Function


  Public Function FaroTimeToSystemTime(Data As Date, StringaTime As String) As DateTime
    Dim HMS As String = StringaTime.Split(".")(0)
    Dim Mill As String = 0
    If StringaTime.IndexOf(".") > -1 Then Mill = StringaTime.Split(".")(1)
    Dim strTimef As String = StringaTime.Substring(0, HMS.Length - 4).PadLeft(2, "0") & ":" & StringaTime.Substring(HMS.Length - 4, 2) & ":" & StringaTime.Substring(HMS.Length - 2, 2) & "." & Mill.PadLeft(3, "0")
    Return DateTime.Parse(Data.ToLongDateString & " " & strTimef)
  End Function

  Public Function TempoInStringaFormattata(Momento As DateTime, FormatType As eFormatType) As String
    Dim strTMP As String
    Select Case FormatType

      Case eFormatType.YYYYMMDD
        strTMP = Momento.ToString("yyyyMMdd")
      Case eFormatType.YYYYMMDDHHMMSS
        strTMP = Momento.ToString("yyyyMMddHHmmss")
      Case eFormatType.Gomboc
        strTMP = Momento.ToString("yyyy-MM-dd HH:mm:ss")
      Case Else
        strTMP = Momento.ToString("yyyyMMddHHmmss")
    End Select
    Return strTMP
  End Function

  Public Function GetFileInfo(FullPath As String) As System.IO.FileInfo
    Return New System.IO.FileInfo(FullPath)
  End Function

  Public Function MagneticDeclination(Pos As clsGeographicPosition, Data As Date) As Double
    Return MagneticDeclination(Pos.LatDec, Pos.LngDec, Data)
  End Function

  Public Function MagneticDeclination(Lat As Double, Lng As Double, Data As Date) As Double
    Dim ngm As New NGeoMag.GeoMag
    Return ngm.GetDeclination(Lat, Lng, Data.Year, 0)
  End Function

  Public Function TrueToMagnetic(Pos As clsGeographicPosition, Data As Date, Bearing As Double) As Double
    Return SommaAngolo180adAngolo360(-MagneticDeclination(Pos.LatDec, Pos.LngDec, Data), Bearing)
  End Function

  Public Function MagneticToTrue(Pos As clsGeographicPosition, Data As Date, Bearing As Double) As Double
    Return SommaAngolo180adAngolo360(MagneticDeclination(Pos.LatDec, Pos.LngDec, Data), Bearing)
  End Function

  Public Enum eSelectChannelType
    eSingle = 0
    eMulti = 1
    eTwin = 2
  End Enum


  Public Function GestisciListaCanaliDaStringaToChannel(Titolo As String, ListaCanaliDisponibili As List(Of clsChannel2020), ListaCanaliSelezionati As List(Of String), SelectChannelType As eSelectChannelType) As List(Of clsChannel2020)
    Dim lstTmp As New List(Of clsChannel2020)
    If Not ListaCanaliSelezionati Is Nothing Then
      For Each cs In ListaCanaliSelezionati
        Dim c = DataProvider2020.CanaleDbl(cs)
        If Not c Is Nothing Then
          lstTmp.Add(c)
        End If
      Next
    End If
    Dim CanaliSelezionati As List(Of clsChannel2020) = GestisciListaCanali(Titolo, ListaCanaliDisponibili, lstTmp, SelectChannelType)
    If CanaliSelezionati Is Nothing Then Return Nothing
    Return CanaliSelezionati
  End Function

  Public Function GestisciListaCanali(Titolo As String, ListaCanaliDisponibili As List(Of clsChannel2020), ListaCanaliSelezionati As List(Of String), SelectChannelType As eSelectChannelType) As List(Of String)
    Dim lstTmp As New List(Of clsChannel2020)
    If Not ListaCanaliSelezionati Is Nothing Then
      For Each cs In ListaCanaliSelezionati
        Dim c = DataProvider2020.CanaleDbl(cs)
        If Not c Is Nothing Then
          lstTmp.Add(c)
        End If
      Next
    End If
    Dim CanaliSelezionati As List(Of clsChannel2020) = GestisciListaCanali(Titolo, ListaCanaliDisponibili, lstTmp, SelectChannelType)
    If CanaliSelezionati Is Nothing OrElse CanaliSelezionati.Count = 0 Then Return New List(Of String)
    Return CanaliSelezionati.Select(Function(x) x.ChannelId).ToList
  End Function

  Public Function GestisciListaCanali(Titolo As String, ListaCanaliDisponibili As List(Of clsChannel2020), ListaCanaliSelezionati As List(Of clsChannel2020), SelectChannelType As eSelectChannelType) As List(Of clsChannel2020)
    If SelectChannelType = eSelectChannelType.eTwin Then
      Dim frmSelChannel As New UserControlChannelSelectorTwin(Titolo, ListaCanaliDisponibili, ListaCanaliSelezionati)
      frmSelChannel.ShowDialog()
      Dim CanaliDaStampare As List(Of clsChannel2020)
      If frmSelChannel.Status = ChannelSelectorAndOrderer.eStatus.eSave Then
        'CanaliDaStampare = frmSelChannel.VM.CanaliSelezionati.Select(Function(x) x.Channel).ToList
        CanaliDaStampare = frmSelChannel.VM.CanaliSelezionati.ToList
        If CanaliDaStampare Is Nothing Then Return New List(Of clsChannel2020)
        If CanaliDaStampare.Count = 0 Then Return New List(Of clsChannel2020)
        Return CanaliDaStampare
      Else
        Return Nothing
        'Return New List(Of clsChannel2020)
      End If
    Else
      Dim sm As SelectionMode = SelectionMode.Single
      If SelectChannelType = eSelectChannelType.eMulti Then sm = SelectionMode.Multiple
      Dim SelFromList As New UserControlSelFromList(Titolo, ListaCanaliDisponibili, ListaCanaliSelezionati, sm)
      SelFromList.ShowDialog()
      If SelFromList.DialogResult Then
        Return SelFromList.CanaliSelezionati
      Else
        Return Nothing
        'Return New List(Of clsChannel2020)
      End If
    End If
  End Function

  Public Function GestisciListaCanali(Titolo As String, ListaCanaliDisponibili As List(Of String), ListaCanaliSelezionati As List(Of String), MultiSelect As Boolean) As List(Of String)
    Dim sm As SelectionMode = SelectionMode.Single
    If MultiSelect Then sm = SelectionMode.Multiple
    Dim SelFromList As New UserControlSelFromList(Titolo, ListaCanaliDisponibili, ListaCanaliSelezionati, sm)
    SelFromList.ShowDialog()
    If SelFromList.DialogResult Then
      Return SelFromList.CanaliSelezionatiString
    Else
      Return New List(Of String)
    End If
  End Function

  Public Sub CambiaLingua(ByVal nuovaLingua As String)
    Try
      If (nuovaLingua.Length > 0) Then
        'Create the culture to be used application wide.
        'The UI culture is the easy one to set.
        meCultureUIInfo = New System.Globalization.CultureInfo(nuovaLingua)
        System.Threading.Thread.CurrentThread.CurrentUICulture = meCultureUIInfo
        'The current culture requires that the string be
        'in the <language>-<country> code.
        If nuovaLingua.Length > 2 Then
          meCultureInfo = New System.Globalization.CultureInfo(nuovaLingua)
          System.Threading.Thread.CurrentThread.CurrentCulture = meCultureInfo
        Else
          'For the culture, use the default.
          meCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture
        End If
      Else
        'Use the defaults from what the user set up.
        meCultureUIInfo = System.Threading.Thread.CurrentThread.CurrentUICulture
        meCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture
      End If
      meCultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm:ss"
      meCultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss"
    Catch argEx As ArgumentException
      'objApplicazione.SalvaNelLog("frmMaster.CambiaLingua" & argEx.Message)
      MessageBox.Show(argEx.Message, "Application error...")
    Catch argEx As NotSupportedException
      'objApplicazione.SalvaNelLog("frmMaster.CambiaLingua" & argEx.Message)
      MessageBox.Show(argEx.Message, "Application error...")
    End Try

  End Sub

  Public Function ImpostaAggregazione(Aggregazione As Double, vMax As Double, vMin As Double, Intervalli As Integer) As Double
    If Aggregazione <= 0 Then
      Dim Valore As Double = (vMax - vMin) / Intervalli
      Dim Segno As Integer = If(Valore = 0, 1, System.Math.Abs(Valore) / Valore)
      Valore = System.Math.Abs(Valore)

      'Dim Test As Double() = {0.001, 0.1, 3.6, 16, 21.5, 55, 122, 240, 450, 550, 700, 995, 1300, 4000, 9000, 12000, 55000, 99000, 150000}
      'For Each Elemento As Double In Test
      '  Valore = Elemento / Intervalli
      Dim Esponente As Integer
      If Valore >= 1 Then
        For Esponente = 1 To 100
          Dim vTMp As Double = Valore / 10 ^ Esponente
          If vTMp <= 1 Then
            Esponente -= 1
            Exit For
          End If
        Next
      Else
        For Esponente = 0 To 100
          Dim vTMp As Double = Valore * 10 ^ Esponente
          If vTMp >= 1 Then
            'Esponente += 1
            Esponente *= -1
            Exit For
          End If
        Next
      End If

      Dim lAggregazione As Double = CInt(Valore / (10 ^ (Esponente))) * (10 ^ (Esponente))
      Dim lAggregazioneUp As Double = CInt(Valore / (10 ^ (Esponente + 1))) * (10 ^ (Esponente + 1))
      Dim lAggregazioneDn As Double = CInt(Valore / (10 ^ (Esponente - 1))) * (10 ^ (Esponente - 1))
      If System.Math.Abs(vMax - vMin) / lAggregazioneUp > 10 Then
        lAggregazione = lAggregazioneUp
      End If
      'Dim Prova As Double = CInt(Elemento / pAggregazione) * pAggregazione
      If lAggregazione = 0 Then lAggregazione = 1
      Return lAggregazione
      'Console.WriteLine(Elemento & " " & pAggregazione & " " & Elemento / pAggregazione)
      'Next

      Stop
    Else
      Return Aggregazione
    End If

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

  Public Function TrovaIndicePrecedente(Momento As DateTime, ValoriDT As Date()) As Integer
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

    'Dim a As New List(Of Double)
    'For i As Integer = 0 To 100
    '    a.Add(i)
    'Next

    'a.Insert(8, 8.5)
    'a.Insert(0, 0.5)


    Return L


    'Dim UpTmp As Integer = L + 1
    'If UpTmp >= ValoriDT.Count - 1 Then

    'Else
    '    Do
    '        If ValoriDT(UpTmp).ToOADate = 0 Then
    '            UpTmp += 1
    '            If UpTmp >= ValoriDT.Count - 1 Then
    '                Exit Do
    '            End If
    '        Else
    '            Exit Do
    '        End If
    '    Loop
    'End If

    'U = Math.Min(UpTmp, ValoriDT.Count - 1)
    'If System.Math.Abs(Momento.Subtract(ValoriDT(L)).TotalSeconds) < System.Math.Abs(Momento.Subtract(ValoriDT(U)).TotalSeconds) Then
    '    IndexTmp = L
    'Else
    '    IndexTmp = U
    'End If
    'Dim d As TimeSpan = Momento.Subtract(ValoriDT(IndexTmp))
    'If System.Math.Abs(d.TotalSeconds) > 1 Then
    '    'Dim Minimatrix = ValoriDT.Skip(L - 100).Take(200).ToList
    '    'Console.WriteLine("Delta Mills:" & d.TotalMilliseconds & " Search:" & Momento.ToString("dd/MM/yyyy HH:mm:ss.fff") & " Find:" & ValoriDT(IndexTmp).ToString("dd/MM/yyyy HH:mm:ss.fff"))
    'Else
    '    'Console.WriteLine(d.TotalMilliseconds)
    'End If
    'Return IndexTmp

  End Function


  'Public Sub IndiciIntervallo(Intervallo As clsTimeRange, ByRef RigaInizioIntervallo As Integer, ByRef RigaFineIntervallo As Integer)
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  Dim SecTmp As Double = Intervallo.Inizio.Subtract(dataProvider2020.TimeRange.Inizio).TotalSeconds
  '  RigaInizioIntervallo = System.Math.Max(0, SecTmp * dataProvider2020.RawFileHz)
  '  SecTmp = Intervallo.Fine.Subtract(dataProvider2020.TimeRange.Inizio).TotalSeconds
  '  RigaFineIntervallo = System.Math.Min(dataProvider2020.RigheDT.Length - 1, SecTmp * dataProvider2020.RawFileHz)
  'End Sub

  'Public Sub IndiciIntervallo(Intervallo As clsTimeRange, ByRef RigaInizioIntervallo As Integer, ByRef RigaFineIntervallo As Integer)
  '  If DataProvider2020 Is Nothing Then Exit Sub
  '  RigaInizioIntervallo = DataProvider2020.TrovaIndice(Intervallo.Inizio)
  '  RigaFineIntervallo = DataProvider2020.TrovaIndice(Intervallo.Fine)
  'End Sub

  'Public Sub ImpostaIndiciIntervallo(ByRef Intervallo As clsTimeRange)
  '	If DataProvider2020 Is Nothing Then Exit Sub
  '	Dim SecTmp As Double = Intervallo.Inizio.Subtract(dataProvider2020.MomentoPrimaRiga).TotalSeconds
  '   Intervallo.IdRigaIniziale = System.Math.Max(0, SecTmp * dataProvider2020.RawFileHz)
  '   SecTmp = Intervallo.Fine.Subtract(dataProvider2020.MomentoPrimaRiga).TotalSeconds
  '   Intervallo.IdRigaFinale = System.Math.Min(dataProvider2020.RigheDT.Length - 1, SecTmp * dataProvider2020.RawFileHz)
  ' End Sub

  Public Function Radians(Degrees As Double) As Double
    Return Degrees * System.Math.PI / 180
  End Function

  Public Function Degrees(Radians As Double) As Double
    Return Radians / System.Math.PI * 180
  End Function

  Public Function ColoreInScala(Coefficiente As Integer) As Color
    '0: blu RGB:0,0,255 
    '64: turchese RGB:0,255,255 
    '128: verde RGB:0,255,0
    '192: giallo RGB:255,255,0 
    '256: rosso RGB:255,0,0 
    'Coefficiente -= 1
    Dim R, G, B As Byte
    Select Case Coefficiente
      Case 0 To 64
        R = 0
        G = System.Math.Min(255, Coefficiente * 4)
        B = 255
      Case 65 To 128
        R = 0
        G = 255
        B = System.Math.Max(0, 255 - (Coefficiente - 64) * 4)
      Case 129 To 192
        R = 255 - (192 - Coefficiente) * 4
        G = 255
        B = 0
      Case 193 To 256
        R = 255
        G = System.Math.Max(0, 255 - (Coefficiente - 192) * 4)
        B = 0
        ' andrá aggiunto il rosso scuro
      Case Else
        Stop
    End Select
    Return Color.FromRgb(R, G, B)
  End Function

  'Public Function ColoreScuroInScala(Coefficiente As Integer) As Color
  '  '0: blu RGB:0,0,255 
  '  '64: turchese RGB:0,255,255 
  '  '128: verde RGB:0,255,0
  '  '192: giallo RGB:255,255,0 
  '  '256: rosso RGB:255,0,0 
  '  'Coefficiente -= 1
  '  'il verde non puó mai essere maggiore di 225/356
  '  Dim R, G, B As Byte
  '  Select Case Coefficiente
  '    Case 0 To 64
  '      R = 0
  '      G = System.Math.Min(225, Coefficiente * 4)
  '      B = 255
  '    Case 65 To 128
  '      R = 0
  '      G = 255
  '      B = System.Math.Max(0, 255 - (Coefficiente - 64) * 4)
  '    Case 129 To 192
  '      R = 255 - (192 - Coefficiente) * 4
  '      G = 255
  '      B = 0
  '    Case 193 To 256
  '      R = 255
  '      G = System.Math.Max(0, 255 - (Coefficiente - 192) * 4)
  '      B = 0
  '      ' andrá aggiunto il rosso scuro
  '    Case Else
  '      Stop
  '  End Select
  '  Return Color.FromRgb(R, G, B)
  'End Function

  Public Function ColoreBeneMale(AlfaChannel As Byte, Coefficiente As Double) As Color
    'Dim Green As Byte = System.Math.Min(255, Coefficiente * 511)
    'Dim Red As Byte = System.Math.Min(255, (1 - Coefficiente) * 511)
    'Console.WriteLine(Coefficiente & " R:" & Red & " G:" & Green)
    Try
      Coefficiente = System.Math.Min(1, System.Math.Max(0, Coefficiente))
      If Not Coefficiente = Double.NaN Then
        Return Color.FromArgb(AlfaChannel, System.Math.Min(255, (1 - Coefficiente) * 511), System.Math.Min(255, Coefficiente * 511), 0)
      End If
    Catch ex As Exception

    End Try
  End Function

  Public Function ScalaColoriSDC(NumeroValori As Integer) As List(Of System.Drawing.Color)
    If NumeroValori = 0 Then NumeroValori = 1
    Dim Steppi As Double = Int(256 / NumeroValori)
    Dim ListaColori As New List(Of System.Drawing.Color)
    For i As Integer = 0 To 255 Step Steppi
      Dim Ctmp As Color = ColoreInScala(i)
      ListaColori.Add(System.Drawing.Color.FromArgb(255, Ctmp.R, Ctmp.G, Ctmp.B))
    Next
    Return ListaColori
  End Function

  'Public Function ScalaColori(NumeroValori As Integer) As List(Of Color)
  '  If NumeroValori = 0 Then NumeroValori = 1
  '  Dim Steppi As Double = Int(256 / NumeroValori)
  '  Dim ListaColori As New List(Of Color)
  '  For i As Integer = 0 To 255 Step Steppi
  '    ListaColori.Add(ColoreInScala(i))
  '  Next
  '  Return ListaColori
  'End Function

  'Public Function ScalaColoriScuri(NumeroValori As Integer) As List(Of Color)
  '  If NumeroValori = 0 Then NumeroValori = 1
  '  Dim Steppi As Double = Int(256 / NumeroValori)
  '  Dim ListaColori As New List(Of Color)
  '  For i As Integer = 0 To 255 Step Steppi
  '    ListaColori.Add(ColoreScuroInScala(i))
  '  Next
  '  Return ListaColori
  'End Function

  Public Function TonalitaRosso(Indice As Integer) As Color
    Return TonalitaRosso(255, Indice)
  End Function

  Public Function TonalitaRosso(Trasparenza As Byte, Indice As Integer) As Color
    Dim idx As Integer = ((Indice / 4) - Int(Indice / 4)) * 4
    Select Case idx
      Case 0
        Return Color.FromArgb(Trasparenza, 255, 0, 0)
      Case 1
        Return Color.FromArgb(Trasparenza, 180, 0, 0)
      Case 2
        Return Color.FromArgb(Trasparenza, 255, 128, 0)
      Case 3
        Return Color.FromArgb(Trasparenza, 255, 0, 128)
    End Select

  End Function

  Public Function TonalitaVerde(Indice As Integer) As Color
    Return TonalitaVerde(255, Indice)
  End Function

  Public Function TonalitaVerde(Trasparenza As Byte, Indice As Integer) As Color
    Dim idx As Integer = ((Indice / 4) - Int(Indice / 4)) * 4
    Select Case idx
      Case 0
        Return Color.FromArgb(Trasparenza, 0, 255, 0)
      Case 1
        Return Color.FromArgb(Trasparenza, 0, 180, 0)
      Case 2
        Return Color.FromArgb(Trasparenza, 128, 255, 0)
      Case 3
        Return Color.FromArgb(Trasparenza, 60, 255, 180)
    End Select
  End Function

  Dim ListaChiavi As New Dictionary(Of String, System.Windows.Media.Color)

  Public Function ColoreDaOutputType(Periodo As clsPeriod2021, OutputType As clsStraightLineVM2020.eOutputType) As System.Windows.Media.Color
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eGroupByTack, clsStraightLineVM2020.eOutputType.eColorByTack
        If Periodo.IsStbd Then
          Return Colors.Green
        Else
          Return Colors.Red
        End If
      Case clsStraightLineVM2020.eOutputType.eColorByKey
        Return ColoreDaChiave(Periodo)
      Case clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc, clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
        Return ColoreDaPerformance(Periodo, OutputType)
      Case Else
        Return Periodo.Colore
    End Select
  End Function

  Public Function ColoreDaChiave(Period As clsPeriod2021) As System.Windows.Media.Color
    If ListaChiavi.ContainsKey(Period.Keys) Then Return ListaChiavi(Period.Keys)
    Return Colors.Black
  End Function


  Public Function ColoreDaPerformance(Period As clsPeriod2021, OutputType As clsStraightLineVM2020.eOutputType) As System.Windows.Media.Color
    Dim MinVal As Double = AppConfig.ActiveProfile.MinVmgPerformanceValue
    Dim MaxVal As Double = AppConfig.ActiveProfile.MaxVmgPerformenceValue
    Dim VmgP As Double
    Dim chPerf As clsChannel2020
    If Not Period.StraightLineVmgDetails Is Nothing Then
      VmgP = Period.StraightLineVmgDetails.VmgPerc.AvgVal
    ElseIf Not Period.StraightLineReachingDetails Is Nothing Then
      VmgP = Period.StraightLineReachingDetails.PolarPerc.AvgVal
    End If
    Select Case OutputType
      Case clsStraightLineVM2020.eOutputType.eColorByVmgTgtPerc
        chPerf = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMGp)
        If chPerf Is Nothing Then Return Colors.Black
        'VmgP = Period.StraightLineVmgDetails.VmgPerc.AvgVal
      Case clsStraightLineVM2020.eOutputType.eColorByBsPolarPerc
        chPerf = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eBSPp)
        If chPerf Is Nothing Then Return Colors.Black
        'VmgP = Period.StraightLineReachingDetails.PolarPerc.AvgVal
      Case Else
        Return Colors.DarkGray
    End Select
    VmgP -= MinVal
    VmgP *= (100 / (MaxVal - MinVal))
    If VmgP < 0 Then VmgP = 0
    If VmgP > 100 Then VmgP = 100
    Return ColoreBeneMale(255, VmgP / 100)
  End Function


  Public Function ColoreDaPerformance(PerfPerc As Double) As System.Windows.Media.Color
    If Double.IsNaN(PerfPerc) Then Return Colors.Transparent
    Dim MinVal As Double = AppConfig.ActiveProfile.MinVmgPerformanceValue
    Dim MaxVal As Double = AppConfig.ActiveProfile.MaxVmgPerformenceValue
    If PerfPerc <= MinVal Then
      Return ColoreBeneMale(255, 0)
    ElseIf PerfPerc >= MaxVal Then
      Return ColoreBeneMale(255, 1)
    Else
      Dim v = PerfPerc - MinVal
      v = CInt(v / (MaxVal - MinVal) * 10)
      Return ColoreBeneMale(255, v / 10)
    End If
  End Function

  Public Function ColoreDaPerformance(PerfPerc As Double, ByRef Name As String) As System.Windows.Media.Color
    If Double.IsNaN(PerfPerc) Then Return Colors.Transparent
    Dim MinVal As Double = AppConfig.ActiveProfile.MinVmgPerformanceValue
    Dim MaxVal As Double = AppConfig.ActiveProfile.MaxVmgPerformenceValue
    If PerfPerc <= MinVal Then
      Name = "<=" & CInt(MinVal)
      Return ColoreBeneMale(255, 0)
    ElseIf PerfPerc >= MaxVal Then
      Name = CInt(MaxVal) & " >"
      Return ColoreBeneMale(255, 1)
    Else
      Dim v = PerfPerc - MinVal
      v = CInt(v / (MaxVal - MinVal) * 10)
      Name = CInt(MinVal + ((MaxVal - MinVal) * (v / 10)))
      Return ColoreBeneMale(255, v / 10)
    End If
  End Function



  Public Function ColoriChiave(Id As Integer) As System.Windows.Media.Color
    Select Case Id
      Case 0
        Return Colors.Red
      Case 1
        Return Colors.Blue
      Case 2
        Return Colors.Green
      Case 3
        Return Colors.Magenta
      Case 4
        Return Colors.DarkOrange
      Case 5
        Return Colors.DarkKhaki
      Case 6
        Return Colors.Violet
      Case 7
        Return Colors.GreenYellow
      Case 8
        Return Colors.DarkGoldenrod
      Case 9
        Return Colors.DarkCyan
      Case Else
        Return Colors.Magenta
    End Select
  End Function

  Public Function AvgCanaleIntervallo(Canale As clsChannel2020, TR As clsTimeRange) As Double
    Dim c = DataProvider2020.CanaleDbl(Canale.ChannelId)
    Dim v = c.Valori.Skip(TR.IdRigaIniziale).Take(TR.IdRigaFinale - TR.IdRigaIniziale).Where(Function(x) Not Double.IsNaN(x))
    If v Is Nothing Then Return 0
    Return v.Average

  End Function

  Public Function ValoriCanaleIntervallo(Canale As clsChannel2020, TR As clsTimeRange) As Double()
    Dim c = DataProvider2020.CanaleDbl(Canale.ChannelId)
    Return c.Valori.Skip(TR.IdRigaIniziale).Take(TR.IdRigaFinale - TR.IdRigaIniziale).Where(Function(x) Not Double.IsNaN(x)).ToArray
  End Function

  Public Sub ImpostaListaChiavi(Lista As List(Of clsPeriod2021))
    Dim i As Integer = 0
    For Each p In Lista
      If Not ListaChiavi.ContainsKey(p.Keys) Then
        ListaChiavi.Add(p.Keys, ColoriChiave(i))
        i += 1
      End If
    Next
  End Sub

  Public Sub ImpostaListaChiaviSoloSelected(Lista As List(Of clsPeriod2021))
    Dim i As Integer = 0
    ListaChiavi.Clear()
    For Each p In Lista
      If p.IsChecked Then
        If Not ListaChiavi.ContainsKey(p.Keys) Then
          ListaChiavi.Add(p.Keys, ColoriChiave(i))
          i += 1
        End If
      End If
    Next
  End Sub


  Public Function LongProiezione(RawLat As Double, RawLong As Double) As Double
    If System.Math.Abs(RawLat) >= 90 Then Return 0
    Return RawLong * System.Math.Cos(Radians(RawLat))
  End Function

  Public Function GetValoriAbs(Valori As List(Of Double)) As List(Of Double)
    Dim lstTmp As New List(Of Double)
    For i As Integer = 0 To Valori.Count - 1
      lstTmp.Add(System.Math.Abs(Valori(i)))
    Next
    Return lstTmp
  End Function

  Public Function GetValoriAbs(Valori As List(Of Double?)) As List(Of Double?)
    Dim lstTmp As New List(Of Double?)
    For i As Integer = 0 To Valori.Count - 1
      lstTmp.Add(System.Math.Abs(Valori(i).Value))
    Next
    Return lstTmp
  End Function

  Public Function GetValoriTack(CurrentChannelPositiveStbd As Boolean, Valori As List(Of Double), ValoriTackChannel As List(Of Double)) As List(Of Double)
    If Not Valori.Count = ValoriTackChannel.Count Then Return Nothing
    Dim lstTmp As New List(Of Double)
    Dim Segno As Integer = If(CurrentChannelPositiveStbd, 1, -1)
    Dim SegnoTack As Integer = 1
    For i As Integer = 0 To Valori.Count - 1
      SegnoTack = If(ValoriTackChannel(i) < 0, -1, 1)
      lstTmp.Add(Segno * SegnoTack * Valori(i))
    Next
    Return lstTmp
  End Function

  Public Function GetValoriStbd(Valori As List(Of Double), ValoriTackChannel As List(Of Double)) As List(Of Double)
    If Not Valori.Count = ValoriTackChannel.Count Then Return Nothing
    Dim lstTmp As New List(Of Double)
    For i As Integer = 0 To Valori.Count - 1
      Dim vTack As Double = ValoriTackChannel(i)
      If vTack >= 0 Then
        lstTmp.Add(Valori(i))
      End If
    Next
    Return lstTmp
  End Function

  Public Sub GetValoriPortStbd(CampoValori As clsChannel2020, IdIniziale As Integer, IdFinale As Integer, ByRef ValoriPort As List(Of Double), ByRef ValoriStbd As List(Of Double), ByRef ValoriAbs As List(Of Double?))
    GetValoriPortStbd(CampoValori, IdIniziale, IdFinale, CanaleTackDefault, ValoriPort, ValoriStbd, ValoriAbs)
  End Sub

  Private Sub GetValoriPortStbd(CampoValori As clsChannel2020, IdIniziale As Integer, IdFinale As Integer, CampoTack As clsChannel2020, ByRef ValoriPort As List(Of Double), ByRef ValoriStbd As List(Of Double), ByRef ValoriAbs As List(Of Double?))
    For i As Integer = IdIniziale To IdFinale
      Dim Vtmp As Double = CampoValori.Valori(i)
      If Not Double.IsNaN(Vtmp) Then
        ValoriAbs.Add(Vtmp)
        If CampoTack.Valori(i) < 0 Then
          ValoriPort.Add(Vtmp)
        Else
          ValoriStbd.Add(Vtmp)
        End If
      End If
    Next
  End Sub

  Public Function GetValoriPort(Valori As List(Of Double), ValoriTackChannel As List(Of Double)) As List(Of Double)
    If Not Valori.Count = ValoriTackChannel.Count Then Return Nothing
    Dim lstTmp As New List(Of Double)
    For i As Integer = 0 To Valori.Count - 1
      Dim vTack As Double = ValoriTackChannel(i)
      If vTack < 0 Then
        lstTmp.Add(Valori(i))
      End If
    Next
    Return lstTmp
  End Function

  'Public Sub ImpostaCanaliAsIsSelected(CanaliOrdinata As IEnumerable(Of clsChannel2020))
  '  For Each Canale As clsChannel2020 In DataProvider2020.Channels.ListaCanali
  '    Canale.IsSelected = False
  '  Next
  '  If CanaliOrdinata Is Nothing Then
  '    Exit Sub
  '  Else
  '    Dim Canali As IEnumerable(Of clsChannel2020) = CanaliOrdinata
  '    For Each Canale As clsChannel2020 In Canali
  '      If Not Canale Is Nothing Then
  '        Canale.IsSelected = True
  '      End If
  '    Next
  '  End If

  'End Sub

  Public Function InterpolazioneLineare(X As Double, X1 As Double, Y1 As Double, X2 As Double, Y2 As Double) As Double
    Dim i = MathNet.Numerics.Interpolation.LinearSpline.Interpolate({X1, X2}, {Y1, Y2})
    Return i.Interpolate(X)
  End Function


  'Public Function IndiceControlloNellaLista(Lista As ObservableCollection(Of UserControlAccPlot), ControlloCorrente As UserControlAccPlot) As Integer
  '  For i As Integer = 0 To Lista.Count - 1
  '    If Lista(i) Is ControlloCorrente Then Return i
  '  Next
  '  Return -1
  'End Function

  Public Function StringaDistanza(Metri As Double) As String
    If Metri < pMetriPerBoatLenght Then
      Return Format(Metri, "F1") & "m"
    ElseIf Metri < pMetriPerBoatLenght * 20 Then
      Return Format(Metri / pMetriPerBoatLenght, "F1") & "bl"
    ElseIf Metri < pMetriMiglioNautico * 2 Then
      Return Format(Metri / pMetriMiglioNautico, "F2") & "nm"
    Else
      Return Format(Metri / pMetriMiglioNautico, "F1") & "nm"
    End If
  End Function

  Public Function Lunghezze(Distanza As Double, UnitaMisura As eUnitaMisura) As Double
    Select Case UnitaMisura
      Case eUnitaMisura.eGradiGeo
        Return Distanza * 60 * pMetriMiglioNautico / pMetriPerBoatLenght
      Case eUnitaMisura.eLunghezza
        Return Distanza
      Case eUnitaMisura.eMetri
        Return Distanza / pMetriPerBoatLenght
      Case eUnitaMisura.eMiglia
        Return Distanza * pMetriMiglioNautico / pMetriPerBoatLenght
    End Select
    Return Distanza
  End Function

  Public Function Metri(Distanza As Double, UnitaMisura As eUnitaMisura) As Double
    Select Case UnitaMisura
      Case eUnitaMisura.eGradiGeo
        Return Distanza * 60 * pMetriMiglioNautico
      Case eUnitaMisura.eLunghezza
        Return Distanza * pMetriPerBoatLenght
      Case eUnitaMisura.eMetri
        Return Distanza
      Case eUnitaMisura.eMiglia
        Return Distanza * pMetriMiglioNautico
    End Select
    Return Distanza
  End Function

  Public Function NodiToMetriSec(SpeedNodi As Double) As Double
    Return SpeedNodi * 1852 / 3600
  End Function

  Public Function MetriSecToNodi(SpeedMetriSecondo As Double) As Double
    Return SpeedMetriSecondo / 1852 * 3600
  End Function

  Public Function TwaPoppaPerFunzioneDelta(Twa As Double) As Double
    If Math.Abs(Twa) > 90 Then
      If Twa > 0 Then
        Return 180 - Twa
      Else
        Return -180 - Twa
      End If
    Else
      Return Twa
    End If
  End Function

  Public Function DifferenzaAssolutaTraAngoli360(ByVal AngoloA As Double, ByVal AngoloB As Double) As Double
    Return DifferenzaAssolutaTraAngoli360(AngoloA, AngoloB, True)
  End Function

  Public Function DifferenzaAssolutaTraAngoli360(ByVal AngoloA As Double, ByVal AngoloB As Double, ByVal minore As Boolean) As Double
    Dim DeltaTMP As Double = System.Math.Abs(AngoloA - AngoloB)
    Dim DeltaTMPINV As Double = 360 - DeltaTMP
    If minore Then
      Return System.Math.Min(DeltaTMP, DeltaTMPINV)
    Else
      Return System.Math.Max(DeltaTMP, DeltaTMPINV)
    End If
  End Function

  Public Function DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(ByVal PrimoAngolo As Double, ByVal SecondoAngolo As Double) As Double
    If Double.IsNaN(PrimoAngolo) Then Return 0
    If Double.IsNaN(SecondoAngolo) Then Return 0
    Dim Delta As Double = DifferenzaAssolutaTraAngoli360(PrimoAngolo, SecondoAngolo, True)
    If PrimoAngolo = 360 Then PrimoAngolo = 0
    Try
      If Math.Round(SommaAngolo180adAngolo360(Delta, SecondoAngolo), 2) = Math.Round(PrimoAngolo, 2) Then ' aggiunge sempre a destra
        Return -Delta
      Else
        Return Delta
      End If
    Catch ex As Exception
      Return -Delta
    End Try
  End Function

  Public Function SommaAngolo180adAngolo360(ByVal Angolo180 As Double, ByVal Angolo360 As Double) As Double
    Dim rTMP As Double = Angolo360 + Angolo180
    If rTMP < 0 Then
      rTMP += 360
    ElseIf rTMP > 360 Then
      rTMP -= 360
    End If
    If rTMP = 360 Then rTMP = 0
    Return rTMP
  End Function

  'Private Sub pGraficoEventiViewModel_SelectionChanged(ActualTimeRange As clsTimeRange) Handles pGraficoEventiViewModel.SelectionChanged
  '  ObjChartSyncManagerBasic.AggiornaVisibleRange(ActualTimeRange, True)
  'End Sub

  'Private Sub pGraficoEventiViewModel_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pGraficoEventiViewModel.PropertyChanged

  'End Sub



  'Private Sub pObjChartSyncManagerBasic_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pObjChartSyncManagerBasic.PropertyChanged
  '  Select Case e.PropertyName
  '    Case "SharedXVisibleRange"
  '      If Not ObjChartSyncManagerBasic.VisibleRange Is Nothing Then

  '        MapControl.AggiornaSelezione(ObjChartSyncManagerBasic.VisibleRange)
  '        GraficoEventiViewModel.AggiornaSelezione(ObjChartSyncManagerBasic.VisibleRange)
  '      End If
  '  End Select
  'End Sub

  Public Function ColoreDaTipo(Tipo As clsPeriod2021.ePeriodType, Trasparenza As Integer) As Color
    Dim Colore As Color
    Select Case Tipo
      Case clsPeriod2021.ePeriodType.eBearAway
        Colore = Colors.Orange
      'Case clsPeriod2021.ePeriodType.eFinish
      '  Colore = Colors.Yellow
      Case clsPeriod2021.ePeriodType.eGybe
        Colore = Colors.Red
      Case clsPeriod2021.ePeriodType.eStraightLineVmg
        Colore = Colors.Gold
      Case clsPeriod2021.ePeriodType.eStraightLineReaching
        Colore = Colors.GreenYellow
      Case clsPeriod2021.ePeriodType.eTack
        Colore = Colors.Blue
      Case clsPeriod2021.ePeriodType.eRoundUp
        Colore = Colors.Gainsboro
      Case clsPeriod2021.ePeriodType.eAcceleration
        Colore = Colors.Violet
      Case Else
        Colore = Colors.PaleVioletRed
    End Select
    Colore.A = Trasparenza
    Return Colore
  End Function

  'Private Sub pObjChartSyncManagerDetails_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles pObjChartSyncManagerDetails.PropertyChanged

  'End Sub

  Public Sub AggiungiDoubleToMatrice(ByRef Matrice As Double(), Valore As Double)
    If Matrice Is Nothing Then ' OrElse Matrice(0) = Nothing Then
      ReDim Matrice(0)
    Else
      ReDim Preserve Matrice(Matrice.Length)
    End If
    Matrice(Matrice.Length - 1) = Valore
  End Sub

  Public Sub AggiungiTRtoMatrice(ByRef Matrice As clsTimeRange(), TR As clsTimeRange)
    If IsNothing(Matrice) Then
      ReDim Matrice(0)
    Else
      ReDim Preserve Matrice(Matrice.Length)
    End If
    Matrice(Matrice.Length - 1) = TR
  End Sub

  'Public Function VisualizzazioneGroupByDefault(Canali As List(Of clsChannel2020)) As clsTimePlotViewModel.eVisualizzazioneGroupBy
  '  If Canali Is Nothing Then Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eNormale
  '  If Canali.Count = 0 Then Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eNormale
  '  Dim AtLeastOne180 As Boolean = False
  '  Dim AtLeastOneTack As Boolean = False
  '  Dim AtLeastOneTackRev As Boolean = False
  '  For Each Canale In Canali
  '    If Not Canale Is Nothing Then
  '      AtLeastOne180 = AtLeastOne180 OrElse Canale.DataType = clsChannel2020.eDataType.e180
  '      AtLeastOneTack = AtLeastOneTack OrElse Canale.DataType = clsChannel2020.eDataType.eTack
  '      AtLeastOneTackRev = AtLeastOneTackRev OrElse Canale.DataType = clsChannel2020.eDataType.eTackReversed
  '    End If
  '  Next
  '  If AtLeastOneTackRev Then Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eAssolutaTbT
  '  If AtLeastOneTack Then Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eAssolutaTbT
  '  If AtLeastOne180 Then Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eAssoluta
  '  Return clsTimePlotViewModel.eVisualizzazioneGroupBy.eNormale
  'End Function


  Public Function Media360(ValoreA As Double, ValoreB As Double) As Double
    Dim mVal() As Double = {ValoreA, ValoreB}
    Return Media360(mVal)
  End Function

  Public Function Media360(Valori As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim AR As Double = 0
    Dim S As Double = 0
    Dim C As Double = 0
    Dim sS, sC As Double

    For Each Valore As Double In v
      AR = Radians(Valore)
      S = System.Math.Sin(AR)
      C = System.Math.Cos(AR)
      sS += S
      sC += C
    Next
    sS /= v.Count
    sC /= v.Count
    AR = System.Math.Atan2(sS, sC)
    If AR < 0 Then AR += System.Math.PI * 2
    'Dim AR2 As Double = System.Math.Atan(sS / sC)
    'Dim ard As Double = RadiantiToGradi(AR)
    'Dim ard2 As Double = RadiantiToGradi(AR2)

    Return Degrees(AR)

  End Function

  Public Function Media360(Valori As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim AR As Double = 0
    Dim S As Double = 0
    Dim C As Double = 0
    Dim sS, sC As Double

    For Each Valore As Double In v
      AR = Radians(Valore)
      S = System.Math.Sin(AR)
      C = System.Math.Cos(AR)
      sS += S
      sC += C
    Next
    sS /= v.Count
    sC /= v.Count
    AR = System.Math.Atan2(sS, sC)
    If AR < 0 Then AR += System.Math.PI * 2
    'Dim AR2 As Double = System.Math.Atan(sS / sC)
    'Dim ard As Double = RadiantiToGradi(AR)
    'Dim ard2 As Double = RadiantiToGradi(AR2)

    Return Degrees(AR)

  End Function

  Public Function Media360(Valori As List(Of Double?)) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim AR As Double = 0
    Dim S As Double = 0
    Dim C As Double = 0
    Dim sS, sC As Double

    For Each Valore As Double In v
      AR = Radians(Valore)
      S = System.Math.Sin(AR)
      C = System.Math.Cos(AR)
      sS += S
      sC += C
    Next
    sS /= v.Count
    sC /= v.Count
    AR = System.Math.Atan2(sS, sC)
    If AR < 0 Then AR += System.Math.PI * 2
    'Dim AR2 As Double = System.Math.Atan(sS / sC)
    'Dim ard As Double = RadiantiToGradi(AR)
    'Dim ard2 As Double = RadiantiToGradi(AR2)

    Return Degrees(AR)

  End Function

  Public Function Media180(ValoreA As Double, ValoreB As Double) As Double
    Dim mVal() As Double = {ValoreA, ValoreB}
    Return Media180(mVal)
  End Function


  Public Function Media180(Valori As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim AR As Double = 0
    Dim S As Double = 0
    Dim C As Double = 0
    Dim sS, sC As Double

    For Each Valore As Double In v
      If Valore < 0 Then Valore += 360
      AR = Radians(Valore)
      S = System.Math.Sin(AR)
      C = System.Math.Cos(AR)
      sS += S
      sC += C
    Next
    sS /= v.Count
    sC /= v.Count
    AR = System.Math.Atan(sS / sC)
    If AR > System.Math.PI Then AR -= 2 * System.Math.PI
    Return Degrees(AR)

  End Function

  Public Function Media180(Valori As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim AR As Double = 0
    Dim S As Double = 0
    Dim C As Double = 0
    Dim sS, sC As Double

    For Each Valore As Double In v
      If Valore < 0 Then Valore += 360
      AR = Radians(Valore)
      S = System.Math.Sin(AR)
      C = System.Math.Cos(AR)
      sS += S
      sC += C
    Next
    sS /= v.Count
    sC /= v.Count
    AR = System.Math.Atan(sS / sC)
    If AR > System.Math.PI Then AR -= 2 * System.Math.PI
    Return Degrees(AR)

  End Function


  Public Function Media(ValoreA As Double, ValoreB As Double) As Double
    Dim mVal() As Double = {ValoreA, ValoreB}
    Return Media(mVal)
  End Function

  Public Function MediaAbs(Valori As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim Somma As Double = 0
    For Each Valore As Double In v
      Somma += Math.Abs(Valore)
    Next
    Return Somma / v.Count

  End Function

  Public Function MediaAbs(Valori As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim Somma As Double = 0
    For Each Valore As Double In v
      Somma += Math.Abs(Valore)
    Next
    Return Somma / v.Count

  End Function

  Public Function Media(Valori As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim Somma As Double = 0
    For Each Valore As Double In v
      Somma += Valore
    Next
    Return Somma / v.Count

  End Function

  Public Function Media(Valori As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim Somma As Double = 0
    For Each Valore As Double In v
      Somma += Valore
    Next
    Return Somma / v.Count

  End Function

  Public Function Media(Valori As List(Of Double?)) As Double
    If Valori Is Nothing Then Return 0
    Dim v = Valori.Where(Function(x) Not Double.IsNaN(x)).ToArray
    If v Is Nothing Then Return 0
    Dim Somma As Double = 0
    For Each Valore As Double In v
      Somma += Valore
    Next
    Return Somma / v.Count

  End Function

  Public Function MediaTack(Valori As Double?(), Tack As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim Somma As Double = 0
    Dim Counter As Integer = 0
    For i As Integer = 0 To Valori.Count - 1
      If Not Double.IsNaN(Valori(i)) AndAlso Not Double.IsNaN(Tack(i)) Then
        Somma += (Valori(i) * IIf(Tack(i) > 0, 1, -1))
        Counter += 1
      End If
    Next
    Return Somma / Counter
  End Function

  Public Function MediaTack(Valori As Double(), Tack As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim Somma As Double = 0
    Dim Counter As Integer = 0
    For i As Integer = 0 To Valori.Count - 1
      If Not Double.IsNaN(Valori(i)) AndAlso Not Double.IsNaN(Tack(i)) Then
        Somma += (Valori(i) * IIf(Tack(i) > 0, 1, -1))
        Counter += 1
      End If
    Next
    Return Somma / Counter
  End Function

  Public Function MediaTackReversed(Valori As Double?(), Tack As Double?()) As Double
    If Valori Is Nothing Then Return 0
    Dim Somma As Double = 0
    Dim Counter As Integer = 0
    For i As Integer = 0 To Valori.Count - 1
      If Not Double.IsNaN(Valori(i)) AndAlso Not Double.IsNaN(Tack(i)) Then
        Somma += (Valori(i) * IIf(Tack(i) > 0, -1, 1))
        Counter += 1
      End If
    Next
    Return Somma / Counter
  End Function

  Public Function MediaTackReversed(Valori As Double(), Tack As Double()) As Double
    If Valori Is Nothing Then Return 0
    Dim Somma As Double = 0
    Dim Counter As Integer = 0
    For i As Integer = 0 To Valori.Count - 1
      If Not Double.IsNaN(Valori(i)) AndAlso Not Double.IsNaN(Tack(i)) Then
        Somma += (Valori(i) * IIf(Tack(i) > 0, -1, 1))
        Counter += 1
      End If
    Next
    Return Somma / Counter
  End Function

  Public Sub PowerZoneReport()
    Dim Awa As Double
    Dim Aws As Double
    Dim AwX As Double
    Dim AwY As Double
    Dim txt As String = ""
    For tws As Integer = 6 To 30 Step 2
      txt &= "Tws" & vbTab & tws.ToString("F0") & vbCrLf
      txt &= "" & vbTab & "Twa" & vbCrLf
      txt &= ""
      For Twa As Integer = 40 To 140 Step 10
        txt &= vbTab & Twa & vbTab & vbTab & vbTab
      Next
      txt &= vbCrLf
      txt &= "Bs"
      For Twa As Integer = 4 To 140 Step 10
        txt &= vbTab & "Awa" & vbTab & "Aws" & vbTab & "AwX" & vbTab & "AwY"
      Next
      txt &= vbCrLf

      For Bs As Integer = 5 To 50 Step 5
        txt &= Bs.ToString("F0")
        For Twa As Integer = 40 To 140 Step 10
          ApparentFromTrue(Awa, Aws, Twa, tws, Bs)
          AwX = Aws * System.Math.Cos(Radians(Awa))
          AwY = Aws * System.Math.Sin(Radians(Awa))
          txt &= vbTab & Awa.ToString("F0") & vbTab & Aws.ToString("F0") & vbTab & AwX.ToString("F0") & vbTab & AwY.ToString("F1")
        Next
        txt &= vbCrLf
      Next
      txt &= vbCrLf
    Next
    Clipboard.SetText(txt)
    MessageBox.Show("PowerZoneReport done! in the clipboard", "", MessageBoxButton.OK)
  End Sub


  Public Sub ApparentFromTrue(ByRef AWA As Double, ByRef AWS As Double, TWA As Double, TWS As Double, BS As Double)
    AWS = (TWS ^ 2 + BS ^ 2 + 2 * TWS * BS * System.Math.Cos(Radians(TWA))) ^ (1 / 2)
    AWA = Degrees(System.Math.Acos((TWS * System.Math.Cos(Radians(TWA)) + BS) / AWS))

  End Sub

  'Public Sub ApparentFromTrue(ByRef AWA As Double, ByRef AWS As Double, TWA As Double, TWS As Double, BS As Double, Leeway As Double)
  '   If TWA = 0 Then
  '     AWA = -Leeway
  '     AWS = System.Math.Abs(TWS + BS)
  '   Else
  '     Dim AWx As Double = BS + Coseno(TWS, TWA)
  '     Dim AWy As Double = TWS * System.Math.Sin(Radians(TWA))
  '     AWS = System.Math.Sqrt(System.Math.Pow(AWx, 2) + System.Math.Pow(AWy, 2))
  '     If AWS = 0 OrElse AWx = 0 Then
  '       AWA = -Leeway
  '     Else
  '       Dim TGtwa As Double = AWy / AWx
  '       AWA = Degrees(System.Math.Atan(TGtwa)) - Leeway
  '     End If
  '   End If
  ' End Sub

  Public Function Coseno(ByVal Raggio As Double, ByVal AngoloGradi As Double) As Double
    If AngoloGradi = 0 Then
      Coseno = Raggio
    Else
      Coseno = Raggio * System.Math.Cos(Radians(System.Math.Abs(AngoloGradi)))
    End If
  End Function

  Public Sub TrueFromApparent(ByRef TWA As Double, ByRef TWS As Double, AWA As Double, AWS As Double, BS As Double)
    TWS = (AWS ^ 2 + BS ^ 2 - 2 * AWS * BS * System.Math.Cos(Radians(AWA))) ^ (1 / 2)
    TWA = Degrees(System.Math.Acos((AWS * System.Math.Cos(Radians(AWA)) - BS) / TWS))

    'If AWA = 0 OrElse AWS = 0 Then
    '	TWA = AWA
    '	TWS = AWS
    'Else
    '	Dim TWx As Double = Coseno(AWS, AWA) - BS
    '    Dim TWy As Double = AWS * System.Math.Sin(Radians(AWA))
    '    TWS = System.Math.Sqrt(System.Math.Pow(TWx, 2) + System.Math.Pow(TWy, 2))
    '    If TWS = 0 Then
    '      TWA = AWA
    '    Else
    '      Dim TGtwa As Double = TWy / TWx
    '      TWA = Degrees(System.Math.Atan(TGtwa))
    '      TGtwa = TWy / TWS
    '      TWA = Degrees(System.Math.Asin(TGtwa))
    '    End If
    '  End If
  End Sub

  'Public Sub TrueFromApparent(ByRef TWA As Double, ByRef TWS As Double, AWA As Double, AWS As Double, BS As Double, Leeway As Double)
  '  AWA += Leeway
  '  If AWA = 0 OrElse AWS = 0 Then
  '    TWA = AWA
  '    TWS = AWS
  '  Else
  '    Dim TWx As Double = Coseno(AWS, AWA) - BS
  '    Dim TWy As Double = AWS * System.Math.Sin(Radians(AWA))
  '    TWS = System.Math.Sqrt(System.Math.Pow(TWx, 2) + System.Math.Pow(TWy, 2))
  '    If TWS = 0 Then
  '      TWA = AWA
  '    Else
  '      Dim TGtwa As Double = TWy / TWx
  '      TWA = Degrees(System.Math.Atan(TGtwa))
  '      TGtwa = TWy / TWS
  '      TWA = Degrees(System.Math.Asin(TGtwa))
  '    End If
  '  End If
  'End Sub

  Public Sub NormalizzaHz(ByRef RawFileHz As Integer)
    Select Case RawFileHz
      Case 4
        RawFileHz = 5
      Case 6 To 9
        RawFileHz = 10
      Case 11 To 19
        RawFileHz = 20
      Case 21 To 49
        RawFileHz = 50
      Case 51 To 99
        RawFileHz = 100
      Case 101 To 199
        RawFileHz = 200
      Case 201 To 499
        RawFileHz = 500
      Case 501 To 999
        RawFileHz = 1000
      Case Else
        ' lascia invariato
    End Select
  End Sub

  Public Function coloriHtml(Indice As Integer)
    Dim ListaColori As New List(Of String)
    ListaColori.Add("Red")
    ListaColori.Add("Green")
    ListaColori.Add("Blue")
    ListaColori.Add("Dark Goldenrod2")
    ListaColori.Add("DarkGreen")
    ListaColori.Add("Orange")
    ListaColori.Add("Gold")
    ListaColori.Add("Marron")
    ListaColori.Add("Violet")
    ListaColori.Add("Cyan")
    ListaColori.Add("DarkBlue")


    '#a6cee3
    '#1F78b4
    '#b2df8a
    '#33a02c
    '#fb9a99
    '#e31a1c
    '#fdbf6f
    '#ff7f00
    '#cab2d6
    '#6a3d9a
    '#ffff99
    '#b15928

    If Indice >= ListaColori.Count Then

      'For i As Integer = 0 To 25
      '  Dim resto As Integer = i Mod ListaColori.Count
      'Next


      Indice = Indice Mod ListaColori.Count
      'Indice = System.Math.IEEERemainder(Indice, ListaColori.Count)
    End If

    Return ListaColori(Indice)
  End Function


  Private Sub ImpostaListaColori()
    pListaColori = New List(Of Color)
    pListaColori.Add(Colors.DarkRed)
    pListaColori.Add(Colors.Green)
    pListaColori.Add(Colors.Blue)
    pListaColori.Add(Colors.DarkGoldenrod)
    pListaColori.Add(Colors.DarkOrange)
    pListaColori.Add(Colors.DarkOrchid)
    pListaColori.Add(Colors.Gold)
    pListaColori.Add(Colors.Cyan)
    pListaColori.Add(Colors.Brown)
    pListaColori.Add(Colors.GreenYellow)
    pListaColori.Add(Colors.Fuchsia)
    pListaColori.Add(Colors.RoyalBlue)
    pListaColori.Add(Colors.Magenta)
    pListaColori.Add(Colors.CadetBlue)
    pListaColori.Add(Colors.DarkKhaki)
    pListaColori.Add(Colors.DarkGreen)
    pListaColori.Add(Colors.DarkViolet)
    pListaColori.Add(Colors.Red)
    pListaColori.Add(Colors.SteelBlue)
    pListaColori.Add(Colors.Teal)
    pListaColori.Add(Colors.Indigo)
    pListaColori.Add(Colors.Tomato)
    pListaColori.Add(Colors.DarkBlue)
    pListaColori.Add(Colors.SeaGreen)
  End Sub


  Public Function ColoriDifferenziati(Indice As Integer, GT As clsXYPlotSettings.eGroupingType, RstColors As Boolean) As Color
    Select Case GT
      Case clsXYPlotSettings.eGroupingType.eTackOnly
        If RstColors Then
          pListaColori = New List(Of Color)
          pListaColori.Add(Colors.Red)
          pListaColori.Add(Colors.Green)
        End If
      Case clsXYPlotSettings.eGroupingType.eTackOnly, clsXYPlotSettings.eGroupingType.eTackAndUpDown
        If RstColors Then
          pListaColori = New List(Of Color)
          pListaColori.Add(Colors.Red)
          pListaColori.Add(Colors.DarkRed)
          pListaColori.Add(Colors.DarkGreen)
          pListaColori.Add(Colors.LightGreen)
        End If

      Case clsXYPlotSettings.eGroupingType.eUpDnOnly
        If RstColors Then
          pListaColori = New List(Of Color)
          pListaColori.Add(Colors.Blue)
          pListaColori.Add(Colors.Yellow)
        End If
      Case Else
        If RstColors OrElse pListaColori Is Nothing OrElse pListaColori.Count < 5 Then ImpostaListaColori()
        If Indice >= pListaColori.Count Then
          Dim Resto = Indice Mod pListaColori.Count
          Indice = Resto
        End If
    End Select



    Return pListaColori(Indice)
  End Function

  Public Function ColoriDifferenziati(Valore As Double) As Color
    If Double.IsNaN(Valore) Then Return Colors.Transparent
    If pListaColori Is Nothing Then ImpostaListaColori()
    Dim Indice As Integer = CInt(Valore)
    If Indice >= pListaColori.Count Then
      Dim Resto = Indice Mod pListaColori.Count
      Indice = Resto
    End If
    Return pListaColori(Indice)
  End Function

  Public Enum eTipoFormatoData
    eDateTimeBreve = 0
    eDateTimeEsteso = 1
    eSoloDateBreve = 2
    eSoloDateEsteso = 3
    eSoloTime = 4
  End Enum

  Public ReadOnly Property TempoFormattato(Momento As DateTime, ByVal Formato As eTipoFormatoData) As String
    Get
      Select Case Formato
        Case eTipoFormatoData.eDateTimeBreve
          Return Format(Momento, "yyyy/MM/dd HH:mm:ss")
        Case eTipoFormatoData.eDateTimeEsteso
          Return Format(Momento, "yyyy MMM dd HH:mm:ss")
        Case eTipoFormatoData.eSoloDateBreve
          Return Format(Momento, "yyyy/MM/dd")
        Case eTipoFormatoData.eSoloDateEsteso
          Return Format(Momento, "yyyy MMM dd")
        Case eTipoFormatoData.eSoloTime
          Return Format(Momento, "HH:mm:ss")
        Case Else
          Return Momento
      End Select
    End Get
  End Property



  Public Property RigheStopWatch As List(Of String)
    Get
      Return pRigheStopWatch
    End Get
    Set(value As List(Of String))
      pRigheStopWatch = value
    End Set
  End Property

  Public Property StpW As Stopwatch
    Get
      Return pStpW
    End Get
    Set(value As Stopwatch)
      pStpW = value
    End Set
  End Property

  Public Property DataProvider2020 As clsDataProvider2020
    Get
      Return _DataProvider2020
    End Get
    Set(value As clsDataProvider2020)
      _DataProvider2020 = value
    End Set
  End Property

  Public Property ObjFiles2020 As clsFiles2020
    Get
      Return pObjFiles2020
    End Get
    Set(value As clsFiles2020)
      pObjFiles2020 = value
    End Set
  End Property

  Public Property MapControl As clsGestioneMapsui
    Get
      Return pMapControl
    End Get
    Set(value As clsGestioneMapsui)
      pMapControl = value
    End Set
  End Property

  'Public Property PedestalControl As clsCalcoliPedestals
  '  Get
  '    Return pPedestalControl
  '  End Get
  '  Set(value As clsCalcoliPedestals)
  '    pPedestalControl = value
  '  End Set
  'End Property

  'Public Property ManBenchManager As clsManoeuversBenchmarks
  '  Get
  '    Return _ManBenchManager
  '  End Get
  '  Set(value As clsManoeuversBenchmarks)
  '    _ManBenchManager = value
  '  End Set
  'End Property

  'Public Property AccBenchManager As clsAccelerationBenchmarks
  '  Get
  '    Return _AccBenchManager
  '  End Get
  '  Set(value As clsAccelerationBenchmarks)
  '    _AccBenchManager = value
  '  End Set
  'End Property

  'Public Property TgtManager As clsTgtManager
  '  Get
  '    Return _TgtManager
  '  End Get
  '  Set(value As clsTgtManager)
  '    _TgtManager = value
  '  End Set
  'End Property

  Public Property PavarotVisibleRange As clsDoubleRange
    Get
      Return _PavarotVisibleRange
    End Get
    Set(value As clsDoubleRange)
      _PavarotVisibleRange = value
    End Set
  End Property

  Public Property AccVisibleRange As clsDoubleRange
    Get
      Return _AccVisibleRange
    End Get
    Set(value As clsDoubleRange)
      _AccVisibleRange = value
    End Set
  End Property

  Public Property SelectedPoints As List(Of Date)
    Get
      Return _SelectedPoints
    End Get
    Set(value As List(Of Date))
      _SelectedPoints = value
    End Set
  End Property

  Public Property ExpStarts As clsExpeditionStarts
    Get
      Return _ExpStarts
    End Get
    Set(value As clsExpeditionStarts)
      _ExpStarts = value
    End Set
  End Property


  'Public Property MapsuiUtilities As clsMapsuiUtilities
  '  Get
  '    Return pMapsuiUtilities
  '  End Get
  '  Set(value As clsMapsuiUtilities)
  '    pMapsuiUtilities = value
  '  End Set
  'End Property

  Public Function SelectFolder(PathIniziale As String) As String
    Dim Fld As New System.Windows.Forms.FolderBrowserDialog
    Fld.SelectedPath = PathIniziale
    Dim Risultato As System.Windows.Forms.DialogResult = Fld.ShowDialog
    Select Case Risultato
      Case Forms.DialogResult.OK
        Return Fld.SelectedPath
      Case Forms.DialogResult.Yes
        Return Fld.SelectedPath
      Case Else
        Return ""
    End Select
  End Function

  Public Function SepDaPath(Path As String) As String
    If Path.IndexOf("/") > 0 Then Return "/"
    If Path.IndexOf("\") > 0 Then Return "\"
    Return ""
  End Function

  Public Sub ApriExplorer(Path As String)
    If System.IO.File.Exists(Path) Then
      Dim fi As New System.IO.FileInfo(Path)
      ApriFinestraExplorer(fi.Directory.FullName, fi.Name)
    ElseIf System.IO.Directory.Exists(Path) Then
      ApriFinestraExplorer(Path, "")
    Else
      ApriFinestraExplorer("", "")
    End If
  End Sub

  Sub ApriFinestraExplorer(FolderPath As String, FileName As String)
    If ExplorerAlreadyOpen(FolderPath) Then Exit Sub
    'Dim Explorer As System.Diagnostics.Process() = Process.GetProcessesByName("explorer")
    'If Explorer.Count > 0 Then
    '  For Each processo In Explorer
    '    If processo.MainWindowTitle = Folder Then
    '      SetForegroundWindow(processo.MainWindowHandle)
    '      Exit Sub
    '    End If
    '  Next
    'End If
    If System.IO.Directory.Exists(FolderPath) Then
      Process.Start("explorer.exe", FolderPath)
    Else
      Process.Start("explorer.exe", "/select,""" & System.IO.Path.Combine(FolderPath, FileName) & """")
    End If

  End Sub

  Function ExplorerAlreadyOpen(FileFolder As String) As Boolean
    For Each explorerProcess As Process In Process.GetProcessesByName("explorer").ToList
      Dim mwt As String = explorerProcess.MainWindowTitle
      If Not String.IsNullOrEmpty(mwt) Then
        Dim fl As New System.IO.DirectoryInfo(mwt.Replace(" - File Explorer", ""))
        If fl.FullName = FileFolder Then
          SetForegroundWindow(explorerProcess.MainWindowHandle)
          Return True
        End If
      End If

      'Dim folderPath As String = GetExplorerFolderPath(explorerProcess)
      'If Not String.IsNullOrEmpty(folderPath) Then
      '  If FileFolder = folderPath Then
      '    SetForegroundWindow(explorerProcess.MainWindowHandle)
      '    Return True
      '  End If
      'Console.WriteLine($"Explorer Process ID: {explorerProcess.Id}, Folder Path: {folderPath}")
      'Else
      '  Console.WriteLine($"Explorer Process ID: {explorerProcess.Id}, Folder Path: Not Available")
      'End If
    Next
    Return False
  End Function

  'Function GetExplorerFolderPath(ByVal explorerProcess As Process) As String
  '  Try
  '    ' Query WMI for the command line of the process
  '    Dim query As String = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {explorerProcess.Id}"
  '    Using searcher As New ManagementObjectSearcher(query)
  '      For Each obj As ManagementObject In searcher.Get()
  '        Dim commandLine As String = obj("CommandLine")?.ToString()
  '        ' Extract the folder path from the command line if available
  '        If Not String.IsNullOrEmpty(commandLine) Then
  '          Dim args As String() = commandLine.Split(""""c)
  '          If args.Length > 1 Then
  '            Return args(1) ' Folder path is typically the second element
  '          End If
  '        End If
  '      Next
  '    End Using
  '  Catch ex As Exception
  '    Console.WriteLine($"Error retrieving folder path: {ex.Message}")
  '  End Try
  '  Return Nothing
  'End Function




  Declare Function SetForegroundWindow Lib "user32.dll" (ByVal hwnd As Integer) As Integer


  Public Function stringaGombocResults(Colonne As List(Of String), LatiPerGiro As Integer, sqlWhere As String) As String
    ' "SELECT solution_id/50 as 'sex', [Block.Time], [Block.Boat.Speed_kts] FROM results WHERE solution_id % 50 == 0 and [Block.Boat.Overlays.LegTimes.Sum] >= 0 ORDER BY solution_id;"

    Dim sqlSelect As String = "SELECT [Block.Clock.Epoch] as 'time',"  ' formato Epoch;"
    sqlSelect &= "[Block.Clock.Epoch] as 'date',"
    sqlSelect &= "[Block.Boat.Latitude] as 'latitude',"
    sqlSelect &= "[Block.Boat.Longitude] as 'longitude',"
    sqlSelect &= "[Block.Boat.TWS_kts] as 'tws'," ' Block.Boat.TWS_kts TWS @ 10m
    sqlSelect &= "[Block.Boat.TWD] as 'twd',"
    sqlSelect &= "[Block.Boat.CWA] as 'twa',"
    sqlSelect &= "[Block.Boat.TWA] as 'hdgtwa',"
    sqlSelect &= "[Block.Boat.Speed_kts] as 'bs',"
    sqlSelect &= "[Block.Boat.Aero.ApparentWind.AWA] as 'awa'," ' Wind Sensor for sails
    sqlSelect &= "([Block.Boat.Aero.ApparentWind.AWS]*1.94384) as 'aws',"
    sqlSelect &= "[Block.Boat.Heel_n] as 'heel',"
    sqlSelect &= "[Block.Boat.Trim] as 'trim',"
    sqlSelect &= "([Block.Boat.q] * 57.2958) as 'ptchrt'," ' 
    sqlSelect &= "[Block.Boat.Sink_min] as 'rideheight',"
    sqlSelect &= "[Block.Boat.PortFoil.Cant] as 'cantport',"
    sqlSelect &= "[Block.Boat.StbdFoil.Cant] as 'cantstbd',"
    If VerificaCampo("Block.Boat.Aero.Traveler", Colonne) Then
      sqlSelect &= "[Block.Boat.Aero.Traveler] as 'maintrav',"
    End If
    If VerificaCampo("Block.Boat.Aero.Twist", Colonne) Then
      sqlSelect &= "[Block.Boat.Aero.Twist] as 'maintwist',"
    End If
    If VerificaCampo("Block.Boat.AP.TgtRide", Colonne) Then
      sqlSelect &= "[Block.Boat.AP.TgtRide] as 'rdetgt',"
    End If
    sqlSelect &= "[Block.Boat.Rudder.Yaw] as 'rdr',"
    sqlSelect &= "[Block.Boat.Rudder.Rake] as 'rdrrk',"
    sqlSelect &= "[Block.Boat.Leeway_n] as 'lwy',"
    sqlSelect &= "[Block.Boat.Heading] as 'hdg',"
    sqlSelect &= "[Block.Boat.COG] as 'cse',"
    sqlSelect &= "[Block.Boat.COG] as 'cog',"
    sqlSelect &= "[Block.Boat.Speed_kts] as 'sog',"  ' ?????
    sqlSelect &= "([Block.Boat.r] * 57.2958) as 'yawrt',"
    sqlSelect &= "([Block.Boat.p] * 57.2958) as 'rollrt',"
    sqlSelect &= "[Block.Boat.PortFoil.InboardFlap1] as 'PortFoilInFlap1Angle',"
    sqlSelect &= "[Block.Boat.PortFoil.InboardFlap2] as 'PortFoilInFlap2Angle',"
    sqlSelect &= "[Block.Boat.PortFoil.OutboardFlap1] as 'PortFoilOutFlap1Angle',"
    sqlSelect &= "[Block.Boat.PortFoil.OutboardFlap2] as 'PortFoilOutFlap2Angle',"
    sqlSelect &= "[Block.Boat.StbdFoil.InboardFlap1] as 'StbdFoilInFlap1Angle',"
    sqlSelect &= "[Block.Boat.StbdFoil.InboardFlap2] as 'StbdFoilInFlap2Angle',"
    sqlSelect &= "[Block.Boat.StbdFoil.OutboardFlap1] as 'StbdFoilOutFlap1Angle',"
    sqlSelect &= "[Block.Boat.StbdFoil.OutboardFlap2] as 'StbdFoilOutFlap2Angle',"
    sqlSelect &= "[Block.Boat.PortFoil.Cant] + [Block.Boat.PortFoil.Effective.Cant] - [Block.Boat.Heel] as 'PortCantAngleEffective',"
    sqlSelect &= "[Block.Boat.StbdFoil.Cant] + [Block.Boat.StbdFoil.Effective.Cant] + [Block.Boat.Heel] as 'StbdCantAngleEffective',"
    sqlSelect &= "([Block.Boat.Rudder.Rake]+[Block.Boat.Trim]) as 'RuddeRakeEffective',"
    sqlSelect &= "[Block.Boat.HullLiftFraction] as 'HullLift',"
    sqlSelect &= "[Block.Boat.RefBoatID] as 'BoatConfigId',"
    sqlSelect &= "[Block.Boat.ExternalAPs.DLL.waveCrest] as 'WaveCrest'"
    If LatiPerGiro > 0 Then
      For i As Integer = 1 To LatiPerGiro
        sqlSelect &= ", [Block.Boat.LegTimes.Leg" & i & "] as 'Leg" & i & "'"
      Next
      sqlSelect &= ", [Block.Boat.LegTimes.Sum] as 'LapTime'"
    End If

    sqlSelect &= " FROM results "
    'If pDownSampling Then
    '  sqlSelect &= sqlWere10Hz() '"WHERE solution_id % 5 == 0 and [Block.Boat.Overlays.LegTimes.Sum] >= 0 "
    'Else
    '  sqlSelect &= sqlWere() '"WHERE solution_id % 5 == 0 and [Block.Boat.Overlays.LegTimes.Sum] >= 0 "
    'End If
    sqlSelect &= sqlWhere
    'sqlSelect &= "WHERE solution_id % 50 == 0 and [Block.Boat.Overlays.LegTimes.Sum] >= 0 "
    sqlSelect &= "ORDER BY solution_id;"

    Return sqlSelect
  End Function

  Public Function stringaGombocResults(PathFileMapping As String, Colonne As List(Of String), LatiPerGiro As Integer, sqlWhere As String) As String
    Dim Righe As List(Of String) = System.IO.File.ReadAllLines(PathFileMapping).ToList
    Dim sqlSelect As String = "SELECT"
    For Each Riga As String In Righe
      Dim CanaliEsistenti As Boolean = VerificaCanali(Riga.Split(vbTab)(0), Colonne)
      If CanaliEsistenti Then
        sqlSelect &= " " & Riga.Split(vbTab)(0) & " as '" & Riga.Split(vbTab)(1) & "',"
      End If
    Next
    If LatiPerGiro > 0 Then
      For i As Integer = 1 To LatiPerGiro
        sqlSelect &= " [Block.Boat.LegTimes.Leg" & i & "] as 'Leg" & i & "',"
      Next
      sqlSelect &= " [Block.Boat.LegTimes.Sum] as 'LapTime'"
    Else
      sqlSelect = sqlSelect.TrimEnd(",")
    End If
    sqlSelect &= " FROM results "
    'If pDownSampling Then
    '  sqlSelect &= sqlWere10Hz(pLatiPerGiro)
    'Else
    '  sqlSelect &= sqlWhere(pLatiPerGiro)
    'End If
    sqlSelect &= sqlWhere
    sqlSelect &= "ORDER BY solution_id;"
    Return sqlSelect
  End Function

  Private Function VerificaCanali(sql As String, ListaCampi As List(Of String)) As Boolean
    Dim r As New Text.RegularExpressions.Regex("\[([^\]]*)\]")
    Dim Risultati As Text.RegularExpressions.MatchCollection = r.Matches(sql)

    For Each risultato In Risultati
      If ListaCampi.Find(Function(x) x = risultato.value.ToString.TrimEnd("]").TrimStart("[")) Is Nothing Then
        Return False
      End If
    Next
    Return True

  End Function

  Public Function VerificaCampo(Nome As String, ListaCampi As List(Of String)) As Boolean
    Return Not ListaCampi.Find(Function(x) x = Nome) Is Nothing
  End Function

  Public Function sqlWere10Hz(LatiPerGiro As Integer) As String
    Dim strTmp As String = "WHERE solution_id % 5 == 0 "
    If LatiPerGiro > 0 Then
      Return strTmp & " and [Block.Boat.LegTimes.Sum] >= 0 "
    Else
      Return strTmp
    End If
  End Function

  Public Function sqlWhere(LatiPerGiro As Integer) As String
    If LatiPerGiro > 0 Then
      Return "WHERE [Block.Boat.LegTimes.Sum] >= 0 "
    Else
      Return ""
    End If
  End Function

  Public Function JouleToKcal(Joules As Double) As Double
    Return Joules * 0.000239006
  End Function

  Public Function CartellaParent(FilePath As String, SeparatoreFinale As Boolean) As String
    Return CartellaParent(New System.IO.FileInfo(FilePath), SeparatoreFinale)
  End Function

  Public Function CartellaParent(FI As System.IO.FileInfo, SeparatoreFinale As Boolean) As String
    If SeparatoreFinale Then
      If FI.FullName.Split(System.IO.Path.DirectorySeparatorChar).Count > FI.FullName.Split(System.IO.Path.AltDirectorySeparatorChar).Count Then
        Return FI.DirectoryName & System.IO.Path.DirectorySeparatorChar
      Else
        Return FI.DirectoryName & System.IO.Path.AltDirectorySeparatorChar
      End If
    Else
      Return FI.DirectoryName
    End If
  End Function

  Public Function PathSepChar(FilePath As String) As String
    Return PathSepChar(New System.IO.FileInfo(FilePath))
  End Function

  Public Function PathSepChar(FI As System.IO.FileInfo) As String
    If FI.FullName.Split(System.IO.Path.DirectorySeparatorChar).Count > FI.FullName.Split(System.IO.Path.AltDirectorySeparatorChar).Count Then
      Return System.IO.Path.DirectorySeparatorChar
    Else
      Return System.IO.Path.AltDirectorySeparatorChar
    End If
  End Function

  'Public Sub polynomiale()
  '  Dim p As MathNet.Numerics.Polynomial
  '  Dim x As Double()
  '  Dim y As Double()
  '  p = MathNet.Numerics.Polynomial.Fit(x, y, 4, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations)

  'End Sub



End Module



'Imports System.ComponentModel
'Imports System.Windows

Module mdlDesignMode

  Private _isDesign As Boolean? = Nothing

  ''' <summary>
  ''' True quando il codice viene eseguito dal designer XAML di Visual Studio / Blend.
  ''' </summary>
  Public ReadOnly Property IsInDesignMode() As Boolean
    Get
      If Not _isDesign.HasValue Then
        _isDesign = DesignerProperties.GetIsInDesignMode(New DependencyObject())
        ' fallback: il designer gira dentro processi separati
        If Not _isDesign.Value Then
          Dim proc As String = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLowerInvariant()
          _isDesign = proc.Contains("devenv") OrElse
                                proc.Contains("xdesproc") OrElse
                                proc.Contains("blend")
        End If
      End If
      Return _isDesign.Value
    End Get
  End Property

End Module

Public Class cls360vector
  Dim pRNG As Double
  Dim pBRGdeg As Double
  Dim pDistUM As eDistUM

  Public ReadOnly Property RNG As Double
    Get
      Return pRNG
    End Get
  End Property

  Public ReadOnly Property RNG(OutputUM As eDistUM) As Double
    Get
      Select Case pDistUM
        Case eDistUM.eKnots, eDistUM.eNauticalMiles
          If OutputUM = eDistUM.eMeters Then Return pRNG * 1852
        Case eDistUM.eMeters
          If OutputUM = eDistUM.eNauticalMiles Then Return pRNG / 1852
          If OutputUM = eDistUM.eKnots Then Return pRNG / 1852
      End Select
      Return pRNG
    End Get
  End Property

  Public ReadOnly Property BRGdeg As Double
    Get
      Return pBRGdeg
    End Get
  End Property

  Public ReadOnly Property ReversedBRGdeg As Double
    Get
      Return SommaAngolo180adAngolo360(180, pBRGdeg)
    End Get
  End Property

  Public ReadOnly Property BRGdegInt As Integer
    Get
      Return CInt(pBRGdeg)
    End Get
  End Property

  Public ReadOnly Property DistUM As eDistUM
    Get
      Return pDistUM
    End Get
  End Property

  Public ReadOnly Property SPD As Double
    Get
      Return pRNG
    End Get
  End Property

  Public Sub AggiornaRangeAndBearing(newRange As Double, newBearing As Double)
    ' brg deve entrare normalizzato
    pRNG = newRange
    pBRGdeg = newBearing
  End Sub

  Public Sub AggiornaRange(newRange As Double)
    pRNG = newRange
  End Sub

  Public Sub AggiornaBearing(newBearing As Double)
    pBRGdeg = newBearing
  End Sub

  Public Sub ReverseBearing()
    pBRGdeg += 180
    If pBRGdeg > 360 Then pBRGdeg -= 360
  End Sub

  Public Sub New(Range As Double, Bearing As Double, DistUM As eDistUM)
    ' BRG deve entrare normalizzato
    pRNG = Range
    pBRGdeg = Bearing
    pDistUM = DistUM
  End Sub

  Public Enum eDistUM
    eMeters = 1
    eNauticalMiles = 2
    eKnots = 3
  End Enum

  'Public Sub New(PositionA As clsGeograficPosition, PositionB As clsGeograficPosition, DistUM As eDistUM)
  '    Dim pObjCalcoliDuePuntiGeo As New clsCalcoliDuePuntiGeo
  '    pObjCalcoliDuePuntiGeo.CalcolaDistanzaAndRotta(PositionA, PositionB)
  '    pDistUM = DistUM
  '    If pDistUM = eDistUM.eMeters Then
  '        pRNG = pObjCalcoliDuePuntiGeo.DistanzaMetri
  '    Else
  '        pRNG = pObjCalcoliDuePuntiGeo.DistanzaNM
  '    End If
  '    pBRGdeg = pObjCalcoliDuePuntiGeo.RottaTrue
  'End Sub

  Public Function VettoreSomma(Vettore As cls360vector) As cls360vector
    ' restituisce il vettore somma dei due vettori ovvero quello corrente e quello passato nella funzione
    ' esempio è il calcolo del vento sull'acqua partendo da vento al suolo e vettore inverso della corrente
    ' esempio è il calcolo del vento al suolo partendo da vento sull'acqua ed il vettore della corrente
    ' esempio è il calcolo di COG e SOG con noti CRS, BS ed il vettore della corrente
    ' esempio è il calcolo di CRS e BS con noti COG, SOG ed vettore inverso della corrente

    Dim Vme_O, Vme_V, VettoreO, VettoreV, Vres_O, Vres_V As Double
    Vme_O = pRNG * System.Math.Cos(GradiToRadianti(pBRGdeg))
    Vme_V = pRNG * System.Math.Sin(GradiToRadianti(pBRGdeg))
    VettoreO = DistanzaCorretta(Vettore) * System.Math.Cos(GradiToRadianti(Vettore.BRGdeg))
    VettoreV = DistanzaCorretta(Vettore) * System.Math.Sin(GradiToRadianti(Vettore.BRGdeg))
    Vres_O = VettoreO + Vme_O
    Vres_V = VettoreV + Vme_V
    Dim Versore As Double = System.Math.Sqrt(System.Math.Pow(Vres_O, 2) + System.Math.Pow(Vres_V, 2))
    If Versore = 0 Then
      Return New cls360vector(Versore, pBRGdeg, pDistUM)
    End If
    Dim Angolo As Double = RadiantiToGradi(System.Math.Atan2(Vres_V, Vres_O))
    If Angolo < 0 Then Angolo += 360
    Return New cls360vector(Versore, Angolo, pDistUM)
  End Function

  Public Function VettoreDifferenza(Vettore As cls360vector) As cls360vector
    ' restituisce il vettore che va dal vertice di quello corrente al vertice di quello passato nella funzione 
    ' come ad esempio nel caso del calcolo della corrente quando si passano i due vettori CRS_BS e COG_SOG

    Dim Vme_O, Vme_V, VettoreO, VettoreV, Vres_O, Vres_V As Double
    Vme_O = pRNG * System.Math.Cos(GradiToRadianti(pBRGdeg))
    Vme_V = pRNG * System.Math.Sin(GradiToRadianti(pBRGdeg))
    VettoreO = DistanzaCorretta(Vettore) * System.Math.Cos(GradiToRadianti(Vettore.BRGdeg))
    VettoreV = DistanzaCorretta(Vettore) * System.Math.Sin(GradiToRadianti(Vettore.BRGdeg))
    Vres_O = VettoreO - Vme_O
    Vres_V = VettoreV - Vme_V
    Dim Versore As Double = System.Math.Sqrt(System.Math.Pow(Vres_O, 2) + System.Math.Pow(Vres_V, 2))
    Dim Angolo As Double = RadiantiToGradi(System.Math.Atan2(Vres_V, Vres_O))
    If Angolo < 0 Then Angolo += 360
    Return New cls360vector(Versore, Angolo, pDistUM)
  End Function

  Public Function GradiToRadianti(AngoloGradi As Double) As Double
    Return AngoloGradi * System.Math.PI / 180
  End Function

  Public Function RadiantiToGradi(AngoloRadianti As Double) As Double
    Return AngoloRadianti / System.Math.PI * 180
  End Function

  Public Function DistanzaCorretta(Vettore As cls360vector) As Double
    If pDistUM = Vettore.pDistUM Then
      Return Vettore.RNG
    ElseIf pDistUM = eDistUM.eMeters Then
      Return Vettore.RNG * 1852
    Else
      Return Vettore.RNG / 1852
    End If
  End Function

End Class

Public Class clsCurrentCalcs

  Public Shared Function CalcolaCorrente(Bs As Double, Cse As Double, sog As Double, cog As Double) As cls360vector
    ' restituisce il vettore che va dal vertice di quello corrente al vertice di quello passato nella funzione 
    ' come ad esempio nel caso del calcolo della corrente quando si passano i due vettori CRS_BS e COG_SOG

    Dim OW_O, OW_V, OG_O, OG_V, Vres_O, Vres_V As Double
    OW_O = Bs * System.Math.Cos(Radians(Cse))
    OW_V = Bs * System.Math.Sin(Radians(Cse))
    OG_O = sog * System.Math.Cos(Radians(cog))
    OG_V = sog * System.Math.Sin(Radians(cog))
    Vres_O = OG_O - OW_O
    Vres_V = OG_V - OW_V
    Dim Versore As Double = System.Math.Sqrt(System.Math.Pow(Vres_O, 2) + System.Math.Pow(Vres_V, 2))
    Dim Angolo As Double = Degrees(System.Math.Atan2(Vres_V, Vres_O))
    If Angolo < 0 Then Angolo += 360
    Return New cls360vector(Versore, Angolo, cls360vector.eDistUM.eKnots)
  End Function


End Class

Public Class clsWindCalc
  Dim pAlfa As Double = AppConfig.ActiveProfile.WindGradientAlpha

  'Tws(h) = TWS(ref)*(h/href)^0.1


  Public Function WindAtHeight(Meas_Tws As Double, Meas_Height As Double, CalcTws_Height As Double) As Double
    Return Meas_Tws * (CalcTws_Height / Meas_Height) ^ pAlfa
  End Function

  Public Function WindAtHeightAlphaVariable(Meas_Tws As Double, Meas_Height As Double, CalcTws_Height As Double) As Double
    Dim Alpha As Double = InterpolazioneLineare(Meas_Tws, 4, AppConfig.ActiveProfile.WindGradientAlphaMin, 25, AppConfig.ActiveProfile.WindGradientAlphaMax)
    Return Meas_Tws * (CalcTws_Height / Meas_Height) ^ Alpha
  End Function


  Public Function ApparentAtHeight(TwsRef As Double, Href As Double, Hwind As Double, Bs As Double, TwaRef As Double, Heeling As Double, ByRef AwaHwind As Double, ByRef AwsHwind As Double) As Double

    Dim HrefHc As Double = Href * Math.Cos(Radians(Heeling))
    Dim HwindHc As Double = Hwind * Math.Cos(Radians(Heeling))
    Dim TwsWind As Double = WindAtHeight(TwsRef, HrefHc, HwindHc)
    'Dim TwsTest As Double = WindAtHeight(10, Href, Hwind)
    'Dim awa, aws As Double
    'Dim awat, awst As Double
    'ApparentFromTrue(awa, aws, 45, 10, 9)
    'ApparentFromTrue(awat, awst, 45, TwsTest, 9)
    ApparentFromTrue(AwaHwind, AwsHwind, TwaRef, TwsWind, Bs)

    Return TwsWind ' returns the wind at the requested height

    'Dim dAwa As Double = awa / AwaHwind
    'Dim dAws As Double = aws / AwsHwind
  End Function


End Class


'Public Class clsMessaggio
'  Dim pMessaggio As String
'  Public Event MessaggioAggiornato(Messaggio As String)


'  Public Property Messaggio As String
'    Get
'      Return pMessaggio
'    End Get
'    Set(value As String)
'      pMessaggio = value
'      RaiseEvent MessaggioAggiornato(pMessaggio)
'    End Set
'  End Property

'End Class


Public Class clsMatriceControlliSinglePeriod
  'Dim pMatriceControlli As New ObservableCollection(Of UserControlTimePlotView)
  Dim pMatriceControlli As New ObservableCollection(Of clsTimePlotViewModel)

  Dim pNomeMatrice As String

  Public Sub New(NomeMatrice As String)
    pNomeMatrice = NomeMatrice
  End Sub

  'Public Property MatriceControlli As ObservableCollection(Of UserControlTimePlotView)
  '  Get
  '    Return pMatriceControlli
  '  End Get
  '  Set(value As ObservableCollection(Of UserControlTimePlotView))
  '    pMatriceControlli = value
  '  End Set
  'End Property

  Public Property MatriceControlli As ObservableCollection(Of clsTimePlotViewModel)
    Get
      Return pMatriceControlli
    End Get
    Set(value As ObservableCollection(Of clsTimePlotViewModel))
      pMatriceControlli = value
    End Set
  End Property

  Private Function HeaderAscissa(CanaleAscissa As clsChannel2020) As String
    If CanaleAscissa Is Nothing Then Return ""
    Return CanaleAscissa.ChannelId
  End Function

  Public Sub SalvaImpostazione()
    Stop
    AppConfig.Salva()
    'Dim Suffisso As String = DataProvider2020.SuffissoFileType
    'AppConfig.EliminaNodo(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, True)
    'Dim ID As Integer = 0
    'If pMatriceControlli.Count > 0 Then
    '  For Each ctrl As UserControlTimePlotView In pMatriceControlli
    '    Dim TimePlotViewModel As clsTimePlotViewModel = DirectCast(ctrl.DataContext, clsTimePlotViewModel)
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Xaxis", HeaderAscissa(TimePlotViewModel.CanaleAscissa), True, False)
    '    Dim LH As New List(Of String)
    '    If Not TimePlotViewModel.CanaliOrdinata Is Nothing Then
    '      For Each canale As clsChannel2020 In TimePlotViewModel.CanaliOrdinata
    '        If Not canale Is Nothing Then
    '          LH.Add(canale.ChannelId)
    '        End If
    '      Next
    '    End If
    '    Dim strTMP As String = ""
    '    For Each Elemento As String In LH
    '      strTMP &= Elemento & ","
    '    Next
    '    strTMP = strTMP.TrimEnd(",")
    '    'Stop
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Yaxis", strTMP, True, False)
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "LineType", TimePlotViewModel.SelectedLineType.LineType, True, False)
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "ShowPortStbdBackground", TimePlotViewModel.ShowPortStbdBackGround, True, False)
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "ShowTargetIfAvailable", TimePlotViewModel.ShowTargetIfAvailable, True, False)
    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "CommonYAxis", TimePlotViewModel.CommonYaxis, True, False)
    '    'Dim LineType As clsGroupLines.eLineType = TrovaValore(Nodo, "LineType", clsGroupLines.eLineType.eDataTypeSigned)
    '    'Dim StampaSfondo As Boolean = TrovaValore(Nodo, "ShowPortStbdBackground", CInt(False))
    '    'Dim StampaTarget As Boolean = TrovaValore(Nodo, "ShowTargetIfAvailable", CInt(True))
    '    'Dim AsseYUnico As Boolean = TrovaValore(Nodo, "CommonYAxis", CInt(False))

    '    ID += 1
    '  Next
    'End If
    'AppConfig.SalvaFileXML()

  End Sub

End Class

'Public Class clsMatriceControlliMultiPeriod
'  Dim pMatriceControlli As New ObservableCollection(Of UserControlStraightLineStandardPlot)
'  Dim pNomeMatrice As String

'  Public Sub New(NomeMatrice As String)
'    pNomeMatrice = NomeMatrice
'  End Sub

'  Public Property MatriceControlli As ObservableCollection(Of UserControlStraightLineStandardPlot)
'    Get
'      Return pMatriceControlli
'    End Get
'    Set(value As ObservableCollection(Of UserControlStraightLineStandardPlot))
'      pMatriceControlli = value
'    End Set
'  End Property



'  Private Function Header(CanaleAscissa As clsChannel2020) As String
'    If CanaleAscissa Is Nothing Then Return ""
'    If CanaleAscissa.IsMath Then
'      Return CanaleAscissa.CanaleChiave.ToString.TrimStart("e")
'    Else
'      Return CanaleAscissa.ChannelId
'    End If
'  End Function

'  Public Sub SalvaImpostazione()
'    Stop
'    AppConfig.Salva()
'    'Dim Suffisso As String = DataProvider2020.SuffissoFileType
'    'AppConfig.EliminaNodo(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, True)
'    'Dim ID As Integer = 0
'    'If pMatriceControlli.Count > 0 Then
'    '  For Each ctrl As UserControlStraightLineStandardPlot In pMatriceControlli
'    '    Dim PeriodStandardPlotViewModel As clsStraightLineStandardPlotViewModel = DirectCast(ctrl.DataContext, clsStraightLineStandardPlotViewModel)
'    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Xaxis", Header(PeriodStandardPlotViewModel.CanaleAscissa), True, False)
'    '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eStartUp, pNomeMatrice, "Chart_" & ID, "Yaxis", Header(PeriodStandardPlotViewModel.CanaleOrdinata), True, False)
'    '    ID += 1
'    '  Next
'    '  AppConfig.SalvaFileXML()
'    'End If

'  End Sub

'End Class



Public Class clsFiles

  Public Event Avanzamento(Valore As Integer)
  Public Event MaxMin(vMin As Integer, vMax As Integer)


  'Public Function SelezionaFileDati(LeggiSoloIntestazioni As Boolean) As Boolean
  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
  '  Dim LastExt As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", "txt", True, True)
  '  If Not System.IO.Directory.Exists(UltimoPath) Then
  '    UltimoPath = AppConfig.ApplicationDataFolder
  '  End If
  '  Dim SelFileNames As New List(Of String)
  '  Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Source File |*.txt;*.csv;*.db;*.parquet;*.bin|All Files|*.*", LastExt, SelFileNames)
  '  If SelectedFiles Is Nothing Then Return False

  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", GetFileInfo(SelectedFiles.First).Directory.FullName, True, True)
  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", GetFileInfo(SelectedFiles.First).Extension, True, True)
  '  Return LeggiFileDati(SelectedFiles, LeggiSoloIntestazioni)
  'End Function

  'Public Function SelezionaFileDati() As Boolean
  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
  '  Dim LastExt As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", "txt", True, True)
  '  If Not System.IO.Directory.Exists(UltimoPath) Then
  '    UltimoPath = AppConfig.ApplicationDataFolder
  '  End If
  '  Dim SelFileNames As New List(Of String)
  '  Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Source File |*.txt;*.csv;*.db;*.parquet;*.bin|All Files|*.*", LastExt, SelFileNames)
  '  If SelectedFiles Is Nothing Then Return False

  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", GetFileInfo(SelectedFiles.First).Directory.FullName, True, True)
  '  AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastExt", GetFileInfo(SelectedFiles.First).Extension, True, True)
  '  Return LeggiFileDati(SelectedFiles, False)
  'End Function


  'Public Sub CompattaFileGomboc(ByRef ProgressBar As clsProgressBar)
  '  ProgressBar.Min = 0
  '  ProgressBar.Max = 1
  '  ProgressBar.Value = 0.1
  '  ProgressBar.ValuePerc = "0%"
  '  System.Windows.Forms.Application.DoEvents()

  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
  '  Dim LastExt As String = "db"
  '  If Not System.IO.Directory.Exists(UltimoPath) Then
  '    UltimoPath = AppConfig.ApplicationDataFolder
  '  End If
  '  Dim SelFileNames As New List(Of String)
  '  Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Gomboc Files |*.db|All Files|*.*", LastExt, SelFileNames)
  '  If SelectedFiles Is Nothing Then
  '    Exit Sub
  '  End If
  '  If SelectedFiles.Count > 0 Then
  '    Dim Progressivo As Integer = 1
  '    For Each Fl In SelectedFiles
  '      Dim Db As New clsSQLiteUtility2020(Fl)
  '      ProgressBar.Value = Progressivo / SelectedFiles.Count
  '      ProgressBar.ValuePerc = Format(Progressivo / SelectedFiles.Count * 100, "F0") & "%"
  '      System.Windows.Forms.Application.DoEvents()
  '      Progressivo += 1
  '    Next
  '    MsgBox(SelectedFiles.Count & " DataBases Compacted!")
  '  End If
  '  ProgressBar.Min = 0
  '  ProgressBar.Max = 0
  '  ProgressBar.Value = 0
  '  ProgressBar.ValuePerc = ""
  '  System.Windows.Forms.Application.DoEvents()

  'End Sub


  'Public Sub MergeParquetMultifiles()
  '  'MergeTextFiles()
  '  'Exit Sub
  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
  '  Dim LastExt As String = "parquet"
  '  If Not System.IO.Directory.Exists(UltimoPath) Then
  '    UltimoPath = AppConfig.ApplicationDataFolder
  '  End If
  '  Dim SelFileNames As New List(Of String)
  '  Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Parquet Files |*.ppf|All Files|*.*", LastExt, SelFileNames)
  '  If SelectedFiles Is Nothing Then Exit Sub
  '  If SelectedFiles.Count > 1 Then
  '    Dim ProvaMerge As New clsParquetMultiFiles(SelectedFiles)
  '    AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", GetFileInfo(ProvaMerge.PathFileSalvato).Directory.FullName, True, True)
  '  Else
  '    ' corrrege il singolo file parquet selezionato
  '  End If

  'End Sub

  'Public Sub MergeTextFiles()
  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "Directory", "LastUsed", AppConfig.ApplicationDataFolder, True, False)
  '  Dim LastExt As String = "parquet"
  '  If Not System.IO.Directory.Exists(UltimoPath) Then
  '    UltimoPath = AppConfig.ApplicationDataFolder
  '  End If
  '  Dim SelFileNames As New List(Of String)
  '  Dim SelectedFiles As List(Of String) = ObjFiles.SelezionaFiles(UltimoPath, "Select Source File", "Parquet Files |*.ppf|All Files|*.*", LastExt, SelFileNames)
  '  If SelectedFiles Is Nothing Then Exit Sub
  '  If SelectedFiles.Count > 1 Then
  '    Dim Righe As New List(Of String)
  '    Dim Fis As New List(Of System.IO.FileInfo)
  '    For Each File In SelectedFiles
  '      Fis.Add(New System.IO.FileInfo(File))
  '    Next
  '    Dim ListaOrdinata = Fis.OrderBy(Function(x) x.LastWriteTime)


  '    For Each File In ListaOrdinata
  '      Dim objReader As New System.IO.StreamReader(File.FullName)
  '      Dim Riga As String
  '      Do
  '        Riga = objReader.ReadLine
  '        Righe.Add(Riga)
  '      Loop Until Riga = Nothing
  '      objReader.Close()
  '    Next
  '    Dim objWriter As New System.IO.StreamWriter(ListaOrdinata.First.FullName.Replace(ListaOrdinata.First.Name, "MergedTxt.csv"), False)
  '    objWriter.Write(String.Join(vbCrLf, Righe))
  '    objWriter.Close()

  '    'ApriExplorer(Fis.First.FullName.Replace(Fis.First.Name, ""), Nothing)

  '  End If
  'End Sub

  'Public Function SelezionaFilePolare(MultiSpline As Boolean) As Boolean
  '  Try
  '    Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
  '    Dim SelectedFile As String = ObjFiles.SelezionaFile(UltimoPath, "Select Polar File", "", "", "")
  '    If SelectedFile = "" Then Return False
  '    AppConfig.SalvaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", SelectedFile, True, True)
  '    TgtManager.CaricaJsonTgtFile(SelectedFile)
  '    Return True
  '  Catch ex As Exception
  '    Return False
  '  End Try
  'End Function

  'Public Function CaricaUltimoFilePolare() As Boolean
  '  Dim UltimoPath As String = AppConfig.CercaValoreInnerText("", clsSettings.eNodoSTD.eStartUp, "PolarFiles", "LastLoaded", AppConfig.ApplicationDataFolder, True, True)
  '  If System.IO.File.Exists(UltimoPath) Then
  '    'Targets = New clsPolare2019(UltimoPath)
  '    TgtManager.CaricaJsonTgtFile(UltimoPath)
  '    Return True
  '  End If
  '  Return False
  'End Function


  Public Function SelezionaFile(pathIniziale As String, intestazioneFinestra As String, stringaFiltro As String, EstensioneDefault As String, ByRef NomeFileSelezionato As String) As String
    Dim FD As New Microsoft.Win32.OpenFileDialog

    If Not EstensioneDefault = "" Then
      FD.DefaultExt = EstensioneDefault
      FD.AddExtension = True
    End If
    FD.CheckFileExists = True
    FD.CheckPathExists = True
    FD.Filter = stringaFiltro
    If System.IO.File.Exists(pathIniziale) Then
      FD.InitialDirectory = System.IO.Path.GetDirectoryName(pathIniziale)
    ElseIf System.IO.Directory.Exists(pathIniziale) Then
      FD.InitialDirectory = pathIniziale
    Else
      FD.InitialDirectory = ""
    End If
    FD.Multiselect = False
    FD.Title = intestazioneFinestra
    If FD.ShowDialog Then
      NomeFileSelezionato = FD.SafeFileName
      Return FD.FileName
    Else
      NomeFileSelezionato = ""
      Return ""
    End If
  End Function

  Public Function SelezionaFiles(pathIniziale As String, intestazioneFinestra As String, stringaFiltro As String, EstensioneDefault As String, ByRef NomiFileSelezionati As List(Of String)) As List(Of String)
    Dim FD As New Microsoft.Win32.OpenFileDialog

    If Not EstensioneDefault = "" Then
      FD.DefaultExt = EstensioneDefault
      FD.AddExtension = True
    End If
    FD.CheckFileExists = True
    FD.CheckPathExists = True
    FD.Filter = stringaFiltro
    If System.IO.File.Exists(pathIniziale) Then
      FD.InitialDirectory = System.IO.Path.GetDirectoryName(pathIniziale)
    ElseIf System.IO.Directory.Exists(pathIniziale) Then
      FD.InitialDirectory = pathIniziale
    Else
      FD.InitialDirectory = ""
    End If
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

  Public Function LeggiTuttoFileTesto(pathFile As String, ByRef errDescription As String) As String
    Dim SR As New System.IO.StreamReader(pathFile)
    Try
      Return SR.ReadToEnd
    Catch ex As Exception
      errDescription = ex.Message
      Return ""
    End Try
  End Function

  Public Function TestoInLista(pathFile As String) As List(Of String)
    Return System.IO.File.ReadAllLines(pathFile).ToList
  End Function

  Public Function TestoInRighe(stringa As String, cSep As String) As String()
    If stringa.IndexOf(cSep) > -1 Then
      Return stringa.Split(cSep)
    End If
    Return Nothing
  End Function

  Public Function TestoInRighe(stringa As String) As String()
    ' verifica CrLf Cr Lf ed elimina quelli ripetuti
    Dim strTest As String() = {"!", "|", "#", "@"}
    ' se il testo ha uno di questi caratteri non si puo' fare la pulizia degli accapo ripetuti
    Dim IDtestChar As Integer = -1
    For i As Integer = 0 To strTest.Length - 1
      If stringa.IndexOf(strTest(i)) = -1 Then
        IDtestChar = i
        Exit For
      End If
    Next
    If IDtestChar > -1 Then
      Dim Carattere As String = strTest(IDtestChar)
      Dim strTMP As String = stringa.Replace(vbCrLf, Carattere)
      strTMP = strTMP.Replace(vbCr, Carattere)
      strTMP = strTMP.Replace(vbLf, Carattere)

      strTMP = strTMP.Replace(Carattere & Carattere & Carattere, Carattere)
      strTMP = strTMP.Replace(Carattere & Carattere, Carattere)
      Return strTMP.Split(Carattere)
    Else
      Return stringa.Split(vbCrLf)
    End If

    Return Nothing
  End Function

  Public Sub SalvaNuovoFileSostituendoContenuto(ByVal Contenuto As String, ByVal FileDaAggiornare As String)
    Try
      If Not System.IO.File.Exists(FileDaAggiornare) Then
        System.IO.File.Create(FileDaAggiornare).Close()
      End If
      Dim SR As New System.IO.StreamWriter(FileDaAggiornare)
      SR.Write(Contenuto)
      SR.Close()
      SR.Dispose()

      'System.IO.File.WriteAllText(FileDaAggiornare, Contenuto)
    Catch ex As Exception
      MsgBox("File looks like in use by another application, please close that application, changes have been not saved")
    End Try

  End Sub

  Public Sub SalvaNuovoFileSostituendoContenuto(ByVal Contenuto As List(Of String), ByVal FileDaAggiornare As String)
    Try
      If Not System.IO.File.Exists(FileDaAggiornare) Then
        System.IO.File.Create(FileDaAggiornare).Close()
      End If
      Dim SR As New System.IO.StreamWriter(FileDaAggiornare)
      SR.Write(String.Join(vbCrLf, Contenuto))
      SR.Close()
      SR.Dispose()

      'System.IO.File.WriteAllText(FileDaAggiornare, Contenuto)
    Catch ex As Exception
      MsgBox("File looks like in use by another application, please close that application, changes have been not saved")
    End Try

  End Sub

  'Public Sub ApriExplorer(ByVal Path As String)

  '  Dim runExplorer As New System.Diagnostics.ProcessStartInfo()
  '  runExplorer.FileName = "explorer.exe"

  '  If System.IO.Directory.Exists(Path) Then
  '    runExplorer.Arguments = CartellaParent(Path)
  '    System.Diagnostics.Process.Start(runExplorer)
  '    'Shell("Explorer.exe " & CartellaParent(Path), AppWinStyle.NormalFocus)
  '  ElseIf System.IO.File.Exists(Path) Then
  '    runExplorer.Arguments = "/select," & Path
  '    System.Diagnostics.Process.Start(runExplorer)
  '    'Shell("Explorer.exe /select," & Path, AppWinStyle.NormalFocus)
  '  Else
  '    MsgBox("File/Folder not found!", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation)
  '  End If
  'End Sub


  Public Function CartellaParent(ByVal FileOrCartella As String) As String
    Return CartellaParent(FileOrCartella, 0, False, False)
  End Function

  Public Function CartellaParent(ByVal FileOrCartella As String, ByVal SeparatoreFinale As Boolean) As String
    Return CartellaParent(FileOrCartella, 0, SeparatoreFinale, False)
  End Function

  Public Function CartellaParent(ByVal FileOrCartella As String, ByVal Livelli As Integer, ByVal SeparatoreFinale As Boolean) As String
    Return CartellaParent(FileOrCartella, Livelli, SeparatoreFinale, False)
  End Function

  Public Function CartellaParent(ByVal FileOrCartella As String, ByVal Livelli As Integer, ByVal SeparatoreFinale As Boolean, ByVal PrimaCartellaParentEsistente As Boolean) As String
    If FileOrCartella = "" Then Return ""
    Dim Csep As Char = trovaSeparatorePath(FileOrCartella)
    Dim strSPLIT As String() = FileOrCartella.TrimEnd(Csep).Split(Csep)
    If strSPLIT Is Nothing OrElse strSPLIT(0) = Nothing Then Return ""
    Dim CartellaTMP As String = ""
    Dim ValTMP As Integer = 1
    If Not EstensioneDelFile(FileOrCartella) = "" Then
      ValTMP = 2
    End If
    Dim strPrimaParentEsistente As String = ""
    For i As Integer = 0 To strSPLIT.Length - ValTMP - Livelli
      CartellaTMP &= strSPLIT(i) & Csep
      If System.IO.Directory.Exists(CartellaTMP) Then
        strPrimaParentEsistente = CartellaTMP
      End If
    Next
    If PrimaCartellaParentEsistente Then
      Return VerificaSeparatoreFinale(strPrimaParentEsistente, SeparatoreFinale)
    Else
      Return VerificaSeparatoreFinale(CartellaTMP, SeparatoreFinale)
    End If
  End Function

  Public Function EstensioneDelFile(ByVal FileDaAnalizzare As String) As String
    EstensioneDelFile = ""
    Dim cSep As String = trovaSeparatorePath(FileDaAnalizzare)
    Dim matrice As String() = FileDaAnalizzare.Split(cSep)
    If cSep = "" Then
      ReDim matrice(0)
      matrice(0) = FileDaAnalizzare
    Else
      FileDaAnalizzare = VerificaSeparatoreFinale(FileDaAnalizzare, False)
      matrice = FileDaAnalizzare.Split(cSep)
    End If
    If matrice(matrice.Length - 1).Contains(".") Then
      Try
        EstensioneDelFile = FileDaAnalizzare.Substring(FileDaAnalizzare.LastIndexOf(".") + 1)
        Return EstensioneDelFile
      Catch ex As Exception
        Return ""
      End Try
    Else
      Return ""
    End If
  End Function

  Public Function FI(PathFile As String) As System.IO.FileInfo
    If System.IO.File.Exists(PathFile) Then
      Return New System.IO.FileInfo(PathFile)
    Else
      Return Nothing
    End If
  End Function

  Public Function VerificaSeparatoreFinale(ByVal Cartella As String, ByVal Separatore As Boolean) As String
    Dim Sep As Char = trovaSeparatorePath(Cartella)
    Dim CartellaTMP As String = Cartella.TrimEnd(Sep)
    If Separatore Then
      Return CartellaTMP & Sep
    Else
      Return CartellaTMP
    End If
  End Function

  Public Function trovaSeparatorePath(ByVal File_o_Cartella As String) As String
    If File_o_Cartella = "" Then
      Return ""
    ElseIf File_o_Cartella.IndexOf("\") > -1 Then
      Return "\"
    ElseIf File_o_Cartella.IndexOf("/") > -1 Then
      Return "/"
    Else
      Return "\" ' serve ad individuare se il file o la cartella si trovano in un ambiente di rete o locale
    End If
  End Function

  Public Function CartellaParentFullPath(PathFileOrCartella As String, SeparatoreFinale As Boolean) As String
    Dim cSep As String = trovaSeparatorePath(PathFileOrCartella)
    If cSep.Length = 0 Then Return PathFileOrCartella

    If System.IO.File.Exists(PathFileOrCartella) Then
      Return VerificaSeparatoreFinale(PathFileOrCartella.Substring(0, PathFileOrCartella.LastIndexOf(cSep)), SeparatoreFinale)
    ElseIf System.IO.Directory.Exists(PathFileOrCartella.TrimEnd(cSep)) Then
      Dim fldTMP As String = PathFileOrCartella.TrimEnd(cSep)
      Return VerificaSeparatoreFinale(fldTMP.Substring(0, fldTMP.LastIndexOf(cSep)), SeparatoreFinale)
    Else
      Return PathFileOrCartella
    End If
  End Function

  Public Function CartelleParentMatriceNomi(PathFileOrCartella As String) As String()
    Dim fldTMP As String = CartellaParentFullPath(PathFileOrCartella, False)
    Dim cSep As String = trovaSeparatorePath(PathFileOrCartella)
    If fldTMP.IndexOf(cSep) = -1 Then
      Return {fldTMP}
    Else
      Return fldTMP.Split(cSep)
    End If

  End Function

  Public Function CartellaParentNome(PathFileOrCartella As String) As String
    Dim Nomi As String() = CartelleParentMatriceNomi(PathFileOrCartella)
    Return Nomi(Nomi.Length - 1)
  End Function

  Public Function PulisciCaratteriBastardi(stringa As String) As String
    Dim charBastards() As Char = {":", ".", " ", "/", "\", "*", "?", "$", "!", "'", ",", ";", "+", "-", "=", "#", "@", "(", ")"}
    Dim strTMP As String = stringa
    For Each Cr As Char In charBastards
      strTMP = strTMP.Replace(Cr, "_")
    Next
    If IsNumeric(strTMP.Substring(0, 1)) Then strTMP = "_" & strTMP
    Return strTMP
  End Function


  Public Shared Function NomeProgressivo(PathFile As String) As String
    Dim fi As New System.IO.FileInfo(PathFile)
    Dim ext As String = fi.Extension
    Dim FileNameOrg As String = fi.Name.Replace(ext, "")
    Dim FileName As String = FileNameOrg
    Dim Counter As Integer = 0
    Do
      Dim np As String = IO.Path.Combine(fi.Directory.FullName, FileName & ext)
      If System.IO.File.Exists(np) Then
        FileName = FileNameOrg & "_" & (Counter + 1).ToString.PadLeft(2, "0")
        Counter += 1
      Else
        Return np
      End If
    Loop
  End Function

End Class



<AddINotifyPropertyChangedInterface>
Public Class clsTimeRange
  Dim _Start As DateTime
  Dim _Finish As DateTime
  'Dim pDescrizione As String

  <JsonIgnore>
  Public ReadOnly Property Durata As TimeSpan
    Get
      Return _Finish.Subtract(Start)
    End Get
  End Property

  Public Sub VerificaInizio(Momento As DateTime)
    If _Start = Nothing Then
      _Start = Momento
    Else
      If Momento < _Start Then
        _Start = Momento
      End If
    End If
  End Sub

  Public Sub VerificaFine(Momento As DateTime)
    If _Finish = Nothing Then
      _Finish = Momento
    Else
      If Momento > _Finish Then
        _Finish = Momento
      End If
    End If
  End Sub

  Public Property Start() As DateTime
    Get
      Return _Start
    End Get
    Set(ByVal value As DateTime)
      _Start = value
    End Set
  End Property

  Public Property Finish() As DateTime
    Get
      Return _Finish
    End Get
    Set(ByVal value As DateTime)
      _Finish = value
    End Set
  End Property

  <JsonIgnore>
  Public ReadOnly Property IdRigaIniziale As Integer
    Get
      Return DataProvider2020.TrovaIndice(_Start)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IdRigaFinale As Integer
    Get
      Return DataProvider2020.TrovaIndice(_Finish)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property RigheIntervallo As Integer
    Get
      Return IdRigaFinale - IdRigaIniziale + 1
    End Get
  End Property

  Public Enum eTipoFormatoData
    eDateTimeBreve = 0
    eDateTimeEsteso = 1
    eSoloDateBreve = 2
    eSoloDateEsteso = 3
    eSoloTime = 4
  End Enum



  <JsonIgnore>
  Public ReadOnly Property InizioFormattato(ByVal Formato As eTipoFormatoData) As String
    Get
      Select Case Formato
        Case eTipoFormatoData.eDateTimeBreve
          Return Format(_Start, "yyyy/MM/dd HH:mm:ss")
        Case eTipoFormatoData.eDateTimeEsteso
          Return Format(_Start, "yyyy MMM dd HH:mm:ss")
        Case eTipoFormatoData.eSoloDateBreve
          Return Format(_Start, "yyyy/MM/dd")
        Case eTipoFormatoData.eSoloDateEsteso
          Return Format(_Start, "yyyy MMM dd")
        Case eTipoFormatoData.eSoloTime
          Return Format(_Start, "HH:mm:ss")
        Case Else
          Return _Start
      End Select
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property MomentoFormattato(Momento As DateTime, ByVal Formato As eTipoFormatoData) As String
    Get
      Select Case Formato
        Case eTipoFormatoData.eDateTimeBreve
          Return Format(Momento, "yyyy/MM/dd HH:mm:ss")
        Case eTipoFormatoData.eDateTimeEsteso
          Return Format(Momento, "yyyy MMM dd HH:mm:ss")
        Case eTipoFormatoData.eSoloDateBreve
          Return Format(Momento, "yyyy/MM/dd")
        Case eTipoFormatoData.eSoloDateEsteso
          Return Format(Momento, "yyyy MMM dd")
        Case eTipoFormatoData.eSoloTime
          Return Format(Momento, "HH:mm:ss")
        Case Else
          Return Momento
      End Select
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property FineFormattato(ByVal Formato As eTipoFormatoData) As String
    Get
      Select Case Formato
        Case eTipoFormatoData.eDateTimeBreve
          Return Format(_Finish, "yyyy/MM/dd HH:mm:ss")
        Case eTipoFormatoData.eDateTimeEsteso
          Return Format(_Finish, "yyyy MMM dd HH:mm:ss")
        Case eTipoFormatoData.eSoloDateBreve
          Return Format(_Finish, "yyyy/MM/dd")
        Case eTipoFormatoData.eSoloDateEsteso
          Return Format(_Finish, "yyyy MMM dd")
        Case eTipoFormatoData.eSoloTime
          Return Format(_Finish, "HH:mm:ss")
        Case Else
          Return _Finish
      End Select
    End Get
  End Property


  <JsonIgnore>
  Public ReadOnly Property IsAllBeforeOrAfter(TR As clsTimeRange) As Boolean
    Get
      If TR.Finish < Start Then
        Return True
      End If
      If TR.Start > Finish Then
        Return True
      End If
      Return False
    End Get
  End Property


  <JsonIgnore>
  Public ReadOnly Property OverlappedWith(TR As clsTimeRange) As Boolean
    Get
      If TR.Start >= _Start And TR.Start <= _Finish Then
        ' se l'inizio del TR da verificare cade nel TimeRange corrente
        Return True
      ElseIf TR.Finish >= _Start And TR.Finish <= _Finish Then
        ' se la fine del TR da verificare cade nel TimeRange corrente
        Return True
      ElseIf TR.Start <= _Start And TR.Finish >= _Finish Then
        ' se tutto il TR è esterno al TimeRange corrente
        Return True
      Else
        Return False
      End If
    End Get
  End Property


  Public Sub VerificaEdEstendiEstremi(TR As clsTimeRange)
    If TR.Start < _Start Then _Start = TR.Start
    If TR.Finish > _Finish Then _Finish = TR.Finish
  End Sub

  <JsonIgnore>
  Public ReadOnly Property IsInRange(MomentoToCheck As DateTime, InclusoInizio As Boolean, InclusaFine As Boolean) As Boolean
    Get
      Dim Incluso As Boolean = False
      If InclusoInizio Then
        Incluso = MomentoToCheck >= _Start
      Else
        Incluso = MomentoToCheck > _Start
      End If
      If Incluso Then
        If InclusaFine Then
          Incluso = MomentoToCheck <= _Finish
        Else
          Incluso = MomentoToCheck < _Finish
        End If
      End If
      Return Incluso
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsSameRange(DataRangeToCheck As clsTimeRange, IgnoraMillesimi As Boolean) As Boolean
    Get
      If DataRangeToCheck Is Nothing Then Return False
      If IgnoraMillesimi Then
        Dim MeTMP As clsTimeRange = New clsTimeRange(_Start.AddMilliseconds(-_Start.Millisecond), _Finish.AddMilliseconds(-_Finish.Millisecond))
        Dim ToCheckTMP As clsTimeRange = New clsTimeRange(DataRangeToCheck.Start.AddMilliseconds(-DataRangeToCheck.Start.Millisecond), DataRangeToCheck.Finish.AddMilliseconds(-DataRangeToCheck.Finish.Millisecond))
        Return DataRangeToCheck.Start = MeTMP.Start AndAlso DataRangeToCheck.Finish = MeTMP.Finish
      End If
      Return DataRangeToCheck.Start = _Start AndAlso DataRangeToCheck.Finish = _Finish
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsSameRange(DataRangeToCheck As clsTimeRange) As Boolean
    Get
      If DataRangeToCheck Is Nothing Then Return False
      Return System.Math.Abs(DataRangeToCheck.Start.Subtract(_Start).TotalSeconds + DataRangeToCheck.Finish.Subtract(_Finish).TotalSeconds) < 2
    End Get
  End Property


  <JsonIgnore>
  Public ReadOnly Property IsFullyOverlapped(DataRangeToCheck As clsTimeRange) As Boolean
    Get
      If IsInRange(DataRangeToCheck.Start, True, True) Then
        If IsInRange(DataRangeToCheck.Finish, True, True) Then
          Return True
        End If
      End If
      Return False
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property IsOverlapped(DataRangeToCheck As clsTimeRange) As Boolean
    Get
      If Finish >= DataRangeToCheck.Start Then
        If Start <= DataRangeToCheck.Finish Then
          Return True
        End If
      End If
      Return False
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DurataInStringa() As String
    Get
      Return pDurataInStringa(False)
    End Get
  End Property

  <JsonIgnore>
  Public ReadOnly Property DurataInStringa(ByVal conCommenti As Boolean) As String
    Get
      Return pDurataInStringa(conCommenti)
    End Get

  End Property

  Private Function pDurataInStringa(ByVal conCommenti As Boolean) As String
    Dim strTMP As String = ""
    If Durata.TotalDays > 0.8 Then
      If conCommenti Then
        strTMP &= "d"
      End If
      strTMP &= "" & Durata.Days
    End If
    If Durata.TotalHours > 0.8 Then
      If conCommenti Then
        strTMP &= " h"
      End If
      strTMP &= "" & Durata.Hours.ToString.PadLeft(2, "0")
    End If
    If Durata.TotalMinutes > 0.8 Then
      If conCommenti Then
        strTMP &= " m"
      End If
      strTMP &= "" & Durata.Minutes.ToString.PadLeft(2, "0")
    End If
    If conCommenti Then
      strTMP &= " s"
    End If
    strTMP &= "" & Durata.Seconds.ToString.PadLeft(2, "0")
    Return strTMP
  End Function

  Public Function DurataInStringaConSeparatore() As String
    Dim strTMP As String = ""
    If Durata.TotalDays > 0.8 Then
      strTMP &= Durata.Days
    End If
    If Durata.TotalHours > 0.8 Then
      If Not strTMP = "" Then strTMP &= "d"
      strTMP &= Durata.Hours.ToString.PadLeft(2, "0")
    End If
    If Durata.TotalMinutes > 0.8 Then
      If Not strTMP = "" Then strTMP &= ":"
      strTMP &= Durata.Minutes.ToString.PadLeft(2, "0")
    End If
    If strTMP = "" Then
      strTMP &= "00:"
    Else
      strTMP &= ":"
    End If
    strTMP &= "" & Durata.Seconds.ToString.PadLeft(2, "0")
    If Durata.TotalSeconds < 2 Then
      If strTMP = "" Then
        strTMP &= "0"
      End If
      strTMP &= "." & Durata.Milliseconds.ToString.PadLeft(3, "0")
    End If
    Return strTMP
  End Function

  Public Sub New()

  End Sub

  Public Sub New(ByVal Inizio As DateTime, ByVal Fine As DateTime)
    ImpostaInizioFine(Inizio, Fine)
  End Sub

  Private Sub ImpostaInizioFine(ByVal Inizio As DateTime, ByVal Fine As DateTime)
    If Inizio <= Fine Then
      _Start = Inizio
      _Finish = Fine
    Else
      _Start = Fine
      _Finish = Inizio
    End If
  End Sub


  'Public Property Descrizione() As String
  '  Get
  '    Return pDescrizione
  '  End Get
  '  Set(ByVal value As String)
  '    pDescrizione = value
  '  End Set
  'End Property


  Public Function DatatimePrecedente(data1 As DateTime, data2 As DateTime) As DateTime
    If data1.ToOADate <= data2.ToOADate Then
      Return data1
    Else
      Return data2
    End If
  End Function

  Public Function DatatimeSuccessivo(data1 As DateTime, data2 As DateTime) As DateTime
    If data1.ToOADate >= data2.ToOADate Then
      Return data1
    Else
      Return data2
    End If
  End Function

  Public Function HasSameRange(TR As clsTimeRange) As Boolean
    If TR Is Nothing Then Return False
    Return TR.Start = _Start And TR.Finish = _Finish
  End Function


  Public Function InRange(Momento As DateTime) As Boolean
    Return Momento >= _Start And Momento <= _Finish
  End Function

  Public Function InRange(IndiceRiga As Integer) As Boolean
    Return IndiceRiga >= IdRigaIniziale And IndiceRiga <= IdRigaFinale
  End Function

  Public Function Clone() As clsTimeRange

    Return DirectCast(Me.MemberwiseClone, clsTimeRange)


  End Function

  <JsonIgnore>
  Public ReadOnly Property StringaPeriodo() As String
    Get
      Return StringaPeriodo(False)
    End Get
  End Property


  <JsonIgnore>
  Public ReadOnly Property StringaPeriodo(ConData As Boolean) As String
    Get
      If ConData Then
        If Start.Date = Finish.Date Then
          Return Start.ToShortDateString & " " & Start.ToLongTimeString & " - " & Finish.ToLongTimeString
        Else
          Return Start.ToString("ddMMMyy HH:mm") & " - " & Finish.ToString("ddMMMyy HH:mm")
        End If
      Else
        Return Start.ToLongTimeString & " - " & Finish.ToLongTimeString
      End If
    End Get
  End Property

End Class



Public Class clsComando
  Implements ICommand

  Dim pAzione As Action

  Public Sub New(action As Action)
    pAzione = action

  End Sub

  Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

  Public Sub Execute(parameter As Object) Implements ICommand.Execute
    pAzione.Invoke()
  End Sub

  Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
    Return True
  End Function
End Class

Public Class clsColorConverter
  Implements IValueConverter

  Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
    Dim Colore As Color = DirectCast(value, Color)
    Return New SolidColorBrush(Colore)
    'Throw New NotImplementedException()
  End Function

  Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack

    'Throw New NotImplementedException()
  End Function
End Class


Public Class clsColorConverterLineUp
  Implements IValueConverter


  Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
    Dim dc = DirectCast(value, DataGridCell)
    Dim pa As clsPeriodoAdvanced = DirectCast(dc.DataContext, clsPeriodoAdvanced)
    Dim strCanale As String = DirectCast(DirectCast(dc.Column, System.Windows.Controls.DataGridBoundColumn).Binding, System.Windows.Data.Binding).Path.Path
    Dim idCanale As Integer = strCanale.Split("[")(1).Split("]")(0)
    If idCanale > -1 Then
      Return pa.ColoreBackGround(idCanale)
    Else
      Return Brushes.LightYellow
    End If
  End Function

  Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack

  End Function
End Class

Public Class clsDoubleRange
  Dim pMax As Double
  Dim pMin As Double

  Public Sub New(Max As Double, Min As Double)
    pMax = Max
    pMin = Min
  End Sub

  Public Property Max As Double
    Get
      Return pMax
    End Get
    Set(value As Double)
      pMax = value
    End Set
  End Property

  Public Property Min As Double
    Get
      Return pMin
    End Get
    Set(value As Double)
      pMin = value
    End Set
  End Property

  Public Sub SetRange(Max As Double, Min As Double)
    pMax = Max
    pMin = Min
  End Sub

End Class

Public Class clsIndexIndex
  Dim _Inizio As Integer
  Dim _Fine As Integer

  Public Sub New(Inizio As Integer, Fine As Integer)
    _Inizio = Inizio
    _Fine = Fine
  End Sub

  Public Property Inizio As Integer
    Get
      Return _Inizio
    End Get
    Set(value As Integer)
      _Inizio = value
    End Set
  End Property

  Public Property Fine As Integer
    Get
      Return _Fine
    End Get
    Set(value As Integer)
      _Fine = value
    End Set
  End Property
End Class


Public Class clsSailingState
  Dim UpL As Double = 65
  Dim DnL As Double = 125

  Public Enum eAndatura
    eUpwind = 0
    eReaching = 1
    eDownWind = 2
  End Enum

  Public Enum eTack
    eBoth = 0
    ePort = 1
    eStbd = 2
    eNone = 3
  End Enum

  Public Function AndaturaDaTwa(TWA As Double) As eAndatura
    If System.Math.Abs(TWA) > DnL Then
      Return eAndatura.eDownWind
    ElseIf System.Math.Abs(TWA) > UpL Then
      Return eAndatura.eReaching
    Else
      Return eAndatura.eUpwind
    End If
  End Function

  Public Function Tack(TWA As Double) As eTack
    If TWA >= 0 Then
      Return eTack.eStbd
    Else
      Return eTack.ePort
    End If
  End Function

End Class


Public Class clsMediaMobile
  Dim pMediaMobileLineare As clsMediaMobileLineare
  Dim pMediaMobile360 As clsMediaMobile360

  Public Sub New(Samples As Integer, Is360 As Boolean)
    If Is360 Then
      pMediaMobile360 = New clsMediaMobile360(Samples)
    Else
      pMediaMobileLineare = New clsMediaMobileLineare(Samples)
    End If
  End Sub

  Public Sub AggiornaMedia(Valore As Double) 'aggiorna
    If pMediaMobile360 Is Nothing Then
      pMediaMobileLineare.AggiornaMedia(Valore)
    Else
      pMediaMobile360.AggiornaMedia(Valore)
    End If
  End Sub

  Public Function SetAndGet(Valore As Double) As Double 'aggiorna e restituisce il valore
    If pMediaMobile360 Is Nothing Then
      pMediaMobileLineare.AggiornaMedia(Valore)
      Return pMediaMobileLineare.Valore
    Else
      pMediaMobile360.AggiornaMedia(Valore)
      Return pMediaMobile360.Valore
    End If
  End Function

  Public Function Valore() As Double 'restituisce il valore
    If pMediaMobile360 Is Nothing Then
      Return pMediaMobileLineare.Valore
    Else
      Return pMediaMobile360.Valore
    End If
  End Function


  Public Sub Azzera()
    If pMediaMobile360 Is Nothing Then
      pMediaMobileLineare.Azzera()
    Else
      pMediaMobile360.Azzera()
    End If
  End Sub

  Private Class clsMediaMobileLineare
    Dim pSamples As Integer
    Dim pMatrice() As Double
    Dim pIndice As Integer = 0
    Dim pPrimoGiro As Boolean = True
    Dim pTotale As Double


    Public Sub New(Samples As Integer)
      pSamples = Samples
      ReDim pMatrice(pSamples - 1)
      'pMatrice(0) = Double.NaN

    End Sub

    Public Sub AggiornaMedia(Valore As Double)
      'If pMatrice.Count > 0 AndAlso Double.IsNaN(pMatrice(0)) Then
      '  pTotale = Valore
      '  pMatrice(pIndice) = Valore
      '  pIndice += 1
      '  If pIndice >= pSamples Then
      '    pIndice = 0
      '    pPrimoGiro = False
      '  End If
      'Else
      If pMatrice.Count > 0 AndAlso (Not Double.IsNaN(Valore)) AndAlso (Not Double.IsInfinity(Valore)) Then
        If Not pPrimoGiro Then
          pTotale -= pMatrice(pIndice)
        End If
        pTotale += Valore
        If Double.IsNaN(pTotale) Then Stop
        If Double.IsInfinity(pTotale) Then Stop
        pMatrice(pIndice) = Valore
        pIndice += 1
        If pIndice >= pSamples Then
          pIndice = 0
          pPrimoGiro = False
        End If
      End If
    End Sub

    Public Function Valore() As Double
      If pPrimoGiro Then
        Return pTotale / pIndice
      Else
        Return pTotale / pSamples
      End If
    End Function

    Public Function UpdateAnDGetMovingAverage(NewValue As Double) As Double
      AggiornaMedia(NewValue)
      Return Valore()
    End Function

    Public Sub Azzera()
      ReDim pMatrice(pSamples - 1)
      'pMatrice(0) = Double.NaN
      pPrimoGiro = True
      pIndice = 0
      pTotale = 0
    End Sub


  End Class



  Private Class clsMediaMobile360
    Dim pSamples As Integer
    Dim pMatrice() As Double
    Dim pIndice As Integer = 0
    Dim pPrimoGiro As Boolean = True
    Dim pTotaleSin As Double
    Dim pTotaleCos As Double
    Dim kDegToRad As Double

    Public Sub New(Samples As Integer)
      pSamples = Samples
      ReDim pMatrice(pSamples - 1)
      'pMatrice(0) = Double.NaN
      kDegToRad = System.Math.PI / 180
    End Sub

    Public Sub AggiornaMedia(Valore As Double)
      'If pMatrice.Count > 0 AndAlso Double.IsNaN(pMatrice(0)) Then
      '  pMatrice(pIndice) = Valore
      '  pTotaleSin = System.Math.Sin(Valore * kDegToRad)
      '  pTotaleCos = System.Math.Cos(Valore * kDegToRad)
      '  pIndice += 1
      '  If pIndice >= pSamples Then
      '    pIndice = 0
      '    pPrimoGiro = False
      '  End If
      'Else
      'If pMatrice.Count > 0 AndAlso (Not Double.IsNaN(Valore)) AndAlso (Not Double.IsInfinity(Valore)) Then
      If pMatrice.Count > 0 AndAlso Not Double.IsNaN(Valore) Then
        If Not pPrimoGiro Then
          pTotaleSin -= System.Math.Sin(pMatrice(pIndice) * kDegToRad)
          pTotaleCos -= System.Math.Cos(pMatrice(pIndice) * kDegToRad)
        End If
        pTotaleSin += System.Math.Sin(Valore * kDegToRad)
        pTotaleCos += System.Math.Cos(Valore * kDegToRad)
        pMatrice(pIndice) = Valore

        pIndice += 1
        If pIndice >= pSamples Then
          pIndice = 0
          pPrimoGiro = False
        End If
      End If

    End Sub

    Public Function Valore() As Double
      Dim vTmp As Double
      If pPrimoGiro Then
        vTmp = System.Math.Atan2(pTotaleSin / pIndice, pTotaleCos / pIndice)
      Else
        vTmp = System.Math.Atan2(pTotaleSin / pSamples, pTotaleCos / pSamples)
      End If
      If vTmp < 0 Then
        vTmp += System.Math.PI * 2
      End If
      Return vTmp / kDegToRad
    End Function

    Public Sub Azzera()
      ReDim pMatrice(pSamples - 1)
      'pMatrice(0) = Double.NaN
      pPrimoGiro = True
      pIndice = 0
      pTotaleSin = 0
      pTotaleCos = 0
    End Sub

  End Class
End Class

Public Class clsContaTempo
  Dim pInizio As DateTime
  Dim pIntermedio As DateTime
  Dim pTesto As String

  Public Property Testo As String
    Get
      Return pTesto
    End Get
    Set(value As String)
      pTesto = value
    End Set
  End Property

  Public Sub AvviaContaTempo()
    pInizio = Now
    pIntermedio = pInizio
    'Console.WriteLine("Start @" & pInizio.ToLongTimeString & " ")
    pTesto = "Descr" & vbTab & "Total" & vbTab & "Partial" & vbCrLf
    pTesto = "Start @" & vbTab & pInizio.ToLongTimeString & vbTab & "" & vbCrLf

  End Sub

  Public Sub StampaMillisecondiTrascorsi(Intestazione As String)
    Dim Adesso As DateTime = Now
    Console.WriteLine(Intestazione & ": " & (Adesso.Subtract(pInizio).TotalMilliseconds / 1000).ToString("F3") & " (" & (Adesso.Subtract(pIntermedio).TotalMilliseconds / 1000).ToString("F3") & ")")
    pTesto &= Intestazione & vbTab & (Adesso.Subtract(pInizio).TotalMilliseconds / 1000).ToString("F3") & vbTab & (Adesso.Subtract(pIntermedio).TotalMilliseconds / 1000).ToString("F3") & vbCrLf
    pIntermedio = Adesso
  End Sub

End Class


Public Class clsInternetTime
  Dim _Client As New Net.Sockets.TcpClient("time.nist.gov", 13)
  Dim _LocalTime As DateTime

  Public Property LocalTime As Date
    Get
      Return _LocalTime
    End Get
    Set(value As Date)
      _LocalTime = value
    End Set
  End Property

  Public Sub VerificaTime()
    _LocalTime = Now
    Exit Sub
    Try
      Dim _SR As New IO.StreamReader(_Client.GetStream())
      Dim _Response = _SR.ReadToEnd
      Dim utcDateTimeString As String = _Response.Substring(7, 17)
      _LocalTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
    Catch ex As Exception
      _LocalTime = Now
    End Try
  End Sub

  '  var client = New TcpClient("time.nist.gov", 13);
  'Using (var streamReader = New StreamReader(client.GetStream()))
  '{
  '    var response = streamReader.ReadToEnd();
  '    var utcDateTimeString = response.Substring(7, 17);
  '    var localDateTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
  '}
End Class


Public Class clsCoppieTwsValori
  Dim _Tws As New List(Of Double)
  Dim _Valori As New List(Of Double)

  Public Sub New(Tws As Double, Valore As Double)
    _Tws.Add(Tws)
    _Valori.Add(Valore)
  End Sub

  Public Property Tws As List(Of Double)
    Get
      Return _Tws
    End Get
    Set(value As List(Of Double))
      _Tws = value
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

  Public Sub AggiungiCoppia(Tws As Double, Valore As Double)
    _Tws.Add(Tws)
    _Valori.Add(Valore)
  End Sub

End Class

Public Class clsGruppoPeriodi
  Dim _Chiave As String
  Dim _CoppieTwsValori As clsCoppieTwsValori
  Dim _ValoriAggregati As New clsValoriAggregati
  Public Property Colore As Color

  Dim _Mure As eMure
  Dim _IdGruppo As Integer

  Public Sub New(Tws As Double, Valore As Double, Mure As eMure, IdGruppo As Integer, Chiave As String, Colore As Color)
    _CoppieTwsValori = New clsCoppieTwsValori(Tws, Valore)
    _Mure = Mure
    _IdGruppo = IdGruppo
    _Chiave = Chiave
    Me.Colore = Colore
  End Sub

  Public Sub New(Seconds As Integer, Valore As Double, Mure As eMure, IdGruppo As Integer, Chiave As String, Colore As Color)
    _ValoriAggregati.AggiungiCoppia(Seconds, Valore)
    _Mure = Mure
    _IdGruppo = IdGruppo
    _Chiave = Chiave
    Me.Colore = Colore
  End Sub

  Public Property IdGruppo As Integer
    Get
      Return _IdGruppo
    End Get
    Set(value As Integer)
      _IdGruppo = value
    End Set
  End Property

  Public Property CoppieTwsValori As clsCoppieTwsValori
    Get
      Return _CoppieTwsValori
    End Get
    Set(value As clsCoppieTwsValori)
      _CoppieTwsValori = value
    End Set
  End Property

  Public Property Mure As eMure
    Get
      Return _Mure
    End Get
    Set(value As eMure)
      _Mure = value
    End Set
  End Property

  Public Property ValoriAggregati As clsValoriAggregati
    Get
      Return _ValoriAggregati
    End Get
    Set(value As clsValoriAggregati)
      _ValoriAggregati = value
    End Set
  End Property

  Public Property Chiave As String
    Get
      Return _Chiave
    End Get
    Set(value As String)
      _Chiave = value
    End Set
  End Property

  Public Enum eMure
    ePort = 0
    eStbd = 1
    eBoth = 2
  End Enum

  Public Sub AggiungiCoppia(Tws As Double, Valore As Double)
    _CoppieTwsValori.AggiungiCoppia(Tws, Valore)
  End Sub

  Public Sub AggiungiCoppia(Seconds As Integer, Valore As Double)
    _ValoriAggregati.AggiungiCoppia(Seconds, Valore)
  End Sub
End Class


Public Class clsDoubleXY
  Dim _X As Double
  Dim _Y As Double

  Public Sub New(X As Double, Y As Double)
    _X = X
    _Y = Y
  End Sub

  Public Property X As Double
    Get
      Return _X
    End Get
    Set(value As Double)
      _X = value
    End Set
  End Property

  Public Property Y As Double
    Get
      Return _Y
    End Get
    Set(value As Double)
      _Y = value
    End Set
  End Property
End Class

Public Class clsXYpoint
  Dim _X As Double
  Dim _Y As Double
  Dim _Color As Color
  Dim _RowColor As Color

  Public Sub New(X As Double, Y As Double, Color As Color)
    _X = X
    _Y = Y
    _Color = Color
    _RowColor = Color
  End Sub

  Public ReadOnly Property Brush As Brush
    Get
      Return New SolidColorBrush(_RowColor)
    End Get
  End Property

  Public Property X As Double
    Get
      Return _X
    End Get
    Set(value As Double)
      _X = value
    End Set
  End Property

  Public Property Y As Double
    Get
      Return _Y
    End Get
    Set(value As Double)
      _Y = value
    End Set
  End Property

  Public Property Color As Color
    Get
      Return _Color
    End Get
    Set(value As Color)
      _Color = value
    End Set
  End Property

  Public Property RowColor As Color
    Get
      Return _RowColor
    End Get
    Set(value As Color)
      _RowColor = value
    End Set
  End Property
End Class

Public Class clsXYZKpoint
  Public Property X As Double
  Public Property Y As Double
  Public Property Z As Double
  Public Property K As Double
  Public Property Color As Color

  Public Sub New(X As Double, Y As Double, Z As Double, K As Double, Color As Color)
    Me.X = X
    Me.Y = Y
    Me.Z = Z
    Me.K = K
    Me.Color = Color
  End Sub

End Class

Public Class clsXYZ
  Public Property X As Double
  Public Property Y As Double
  Public Property Z As Double
  Public Property Color As Color

  Public Sub New(X As Double, Y As Double, Z As Double, Color As Color)
    Me.X = X
    Me.Y = Y
    Me.Z = Z
    Me.Color = Color
  End Sub

End Class

Public Class clsMeteoReport

  Public Shared Sub CreateReport(TR As clsTimeRange, StepsInSeconds As Integer)

    'StepsInSeconds = 60

    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    Dim ChLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim ChLng = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    Dim ChTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim ChTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)

    Dim Lt As New List(Of Double)
    Dim Ln As New List(Of Double)
    Dim Tws As New List(Of Double)
    Dim Twd As New List(Of Double)


    'Dim kml As New SharpKml.Dom.Kml
    'Dim s As New SharpKml.Base.Serializer
    's.Serialize(kml)
    'Dim ps As New SharpKml.Base.Parser
    'ps.ParseString(s.Xml, True)
    'kml = CType(ps.Root, SharpKml.Dom.Kml)

    Dim kmlfld As New SharpKml.Dom.Folder
    kmlfld.Name = "Meteo Report " & TR.Start.ToString("yyyy MM dd")

    Dim Righe As New List(Of String)
    Dim Riga As String = TR.Start.ToString("yyyy MMM dd") & " Meteo Report" & vbCrLf
    Righe.Add(Riga)
    Riga = "Time" & vbTab & "Tws" & vbTab & "Range" & vbTab & "Twd" & vbTab & "Range" & vbTab & "Lat" & vbTab & "Long"
    Righe.Add(Riga)
    Dim c As Integer = 0
    Dim Start As DateTime = TR.Start
    For i As Integer = 0 To ChLat.Valori.Count - 10
      If Not Double.IsNaN(ChLat.Valori(i)) Then Lt.Add(ChLat.Valori(i))
      If Not Double.IsNaN(ChLng.Valori(i)) Then Ln.Add(ChLng.Valori(i))
      If Not Double.IsNaN(ChTws.Valori(i)) Then Tws.Add(ChTws.Valori(i))
      If Not Double.IsNaN(ChTwd.Valori(i)) Then Twd.Add(ChTwd.Valori(i))
      c += 1
      Dim adesso As DateTime = DataProvider2020.Momento(i)
      If adesso.Subtract(Start).TotalSeconds >= StepsInSeconds Then
        Riga = adesso.AddSeconds(-CInt(StepsInSeconds / 2)).ToString("HH:mm") & vbTab
        Riga &= Tws.Average.ToString("F1") & vbTab

        'Dim StdDev As Double
        'alglib.basestat.sampleadev(Tws.ToArray, Tws.Count, StdDev)
        'Riga &= StdDev.ToString("F1") & vbTab
        Riga &= Tws.Min.ToString("F1") & "-" & Tws.Max.ToString("F1") & vbTab

        Dim avg = Media360(Twd.ToArray)
        Dim r As New List(Of Double)
        Dim l As New List(Of Double)
        For Each e In Twd.ToArray
          Dim d = DifferenzaTraAngoli360_PositivoSeSecondoADestraDelPrimo(avg, e)
          If d >= 0 Then
            r.Add(d)
          Else
            l.Add(Math.Abs(d))
          End If
        Next
        Dim MaxR = SommaAngolo180adAngolo360(MathNet.Numerics.Statistics.Statistics.Percentile(r.ToArray(), 98), avg)
        Dim MaxL = SommaAngolo180adAngolo360(-MathNet.Numerics.Statistics.Statistics.Percentile(l.ToArray(), 98), avg)
        Riga &= avg.ToString("F0").PadLeft(3, "0") & vbTab
        Riga &= MaxL.ToString("F0").PadLeft(3, "0") & "-" & MaxR.ToString("F0").PadLeft(3, "0") & vbTab

        Riga &= Lt.Average.ToString("F8") & vbTab
        Riga &= Ln.Average.ToString("F8")
        Righe.Add(Riga)

        Dim pm As New SharpKml.Dom.Placemark
        Dim p As New SharpKml.Dom.Point
        p.Coordinate = New SharpKml.Base.Vector(Lt.Average, Ln.Average)
        p.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround
        pm.Name = adesso.AddSeconds(-CInt(StepsInSeconds / 2)).ToString("HH:mm") & " Tws: " & Tws.Average.ToString("F1") & ", Twd: " & Media360(Twd.ToArray).ToString("F0") & ""
        pm.Geometry = p

        'Dim mg As New SharpKml.Dom.MultipleGeometry

        kmlfld.AddFeature(pm)


        Start = DataProvider2020.Momento(i)
        c = 0
        Lt.Clear()
        Ln.Clear()
        Tws.Clear()
        Twd.Clear()


      End If
    Next

    Dim kml As New SharpKml.Dom.Kml
    kml.Feature = kmlfld
    Dim kmlfile = SharpKml.Engine.KmlFile.Create(kml, False)
    SalvaKml(TR, kmlfile)

    Dim objPdf As New clsPdf
    objPdf.StampaMeteoReport(Righe)

    SalvaCsv(TR, Righe)

    SalvaExpLikeLog(TR)

    Clipboard.SetText(String.Join(vbCrLf, Righe))


  End Sub



  Public Shared Sub SalvaExpLikeLog(TR As clsTimeRange)
    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = TR.Start.ToString("yyyyMMdd")
    If TR.Durata.TotalDays >= 1 Then
      pstringadata = TR.Start.ToString("yyyyMMdd") & "_" & TR.Finish.ToString("yyyyMMdd")
    End If
    Dim filename As String = IO.Path.Combine(path, "MeteoReportExpLike_" & pstringadata & ".txt")

    '    !Boat, Utc, BSP, AWA, AWS, TWA, TWS, TWD, RudderFwd, Course, Leeway,Set, Drift, HDG, AirTemp, SeaTemp, Baro, Depth, Heel, Trim, Rudder, Forestay, Downhaul, FStayLen, MastButt, Load S, Load P, Rake, Volts, ROT, Lat, Lon, COG, SOG, TargHeel,Error, VMG%, Vang, Trav, Main, Board, Board P, Board S, StTmToP, StTmToS, LnSqWind, DistToLn, RchTmToLn, RchDtToLn, RchBsToLn, MagVar, GWD, GWS, BlwLnStrn, GpsTmToLn, GpsTmToBn, GPS time, Downhaul2, Mk Lat, Mk Lon, Port lat, Port lon, Stbd lat, Stbd lon, Lead P, Lead S, BackStay, SeaState, MagicMoment, RdrTgtFaro, DeflectorTgtFar, FsyTgtFaro, JibTackTgtFaro, LeeCapTgtFaro, MainSheetTgtFar, MastButtTgtFaro, RakeTgtFaro, MastShimsTgtFar, TrimTgtFaro, VangTgtFaro, ProjHdg, ProjTgt, TtkRc, TtkPort, TtkAlfaRc, TtkBravoPin, TtkStbd, TtkPin, TtkTrigger, MastShimsMm, JibInOutLwd, JibUpDnCm, Hdg2, Roll2, Pitch2, BsTgtFaro, TwaTgtFaro, LeeCap, PerfPolFaro, TmToGun, TmToLn, Burn, BelowLn, GunBlwLn, dBspSog, dHdgCog, TWDPeriod, TWSPeriod, Targ Twa, Targ Bsp, WvSigHt, WvSigPd, WvMaxHt, WvMaxPd, GPS tOffset, Heave, MWA, MWS, Boom, StBsOnS, Twist, TWDTwisted, TackLossT, TackLossD, TrimRate, HeelRate, DeflectorP, RudderP, RudderS, RudderToe, BspTr, FStayInner, DeflectorS, Bobstay, Outhaul, D0 P, D0 S, D1 P, D1 S, V0 P, V0 S, V1 P, V1 S, BoomAng, Cunningham, FStayInHal, JibFurl, JibH, MastCant, J1, J2, J3, J4, Foil P, Foil S, Reacher, Blade, Staysail, Solent, Tack, TackP, TackS, DeflectU, DeflectL, WinchP, WinchS, SpinP, SpinS, MainH, Mast2, DepthAft, Burn%, GunBspTarg%, GunBspPol%, EngTemp, EngOilTemp, TranOilTemp, TranOilPres, FuelLev, Current, Charge%, TWG, TWDG, DewPt, FStay + Tack, Gradient, TWSGradient, Rud Ptch P, Rud Ptch S, WaterLev, WaterLev2, FuelLev2, CANLoad%, FastPkErr, Volt 1, Volt 2, Volt 3, Volt 4, Current1, Current2, Current3, Current4, Charge1%, Charge2%, Charge3%, Charge4%, CalROT, CalBrake, CalAccel, TWDmin, TWDmax, PredSeaTemp
    '!boat, 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 25, 26, 27, 28, 29, 30, 32, 48, 49, 50, 51, 56, 60, 66, 67, 68, 69, 73, 77, 78, 81, 82, 83, 84, 85, 86, 87, 93, 94, 95, 116, 142, 143, 146, 151, 161, 162, 163, 164, 165, 166, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 231, 232, 235, 236, 237, 238, 252, 253, 254, 255, 256, 257, 258, 259, 260, 267, 268, 272, 273, 274, 277, 278, 281, 282, 283, 284, 285, 286, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 317, 318, 319, 320, 321, 322, 323, 324, 325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 351, 352, 354, 355, 359, 360, 361, 362, 363, 364, 365, 368, 369, 370, 371, 372, 373, 374, 375, 379, 380, 381, 382, 383, 389, 390, 391, 392, 393, 394, 395, 396, 397, 398, 399, 400, 401, 402, 403, 411, 412, 413
    '!v120.6.9
    '0,46147.308486,1,0.000,2,5.5,3,4.62,4,5.0,5,4.42,6,168.2,7,6.31,9,163.1,10,0.00,11,356.6,12,0.11,13,163.1,14,0.00,15,1.30,17,42949672.00,18,0.37,19,-0.22,20,6.31,22,0.705,25,0.865,26,23.171,27,0.259,28,-0.200,29,4.683,32,-0.01,48,39.5313154,49,2.5662734,50,356.6,51,0.103,56,10.12,66,0.00,67,1.301,68,1.345,69,67.696,83,207.0,93,2.092,94,168.5,95,4.54,146,0.308494,163,39.5032600,164,2.6551980,165,39.5051540,166,2.6507880,169,164.221,170,-0.098,171,-0.370,172,0.072787,174,1.730000,175,100.000000,176,1.799651,177,0.000000,178,0.100000,179,0.000000,180,26.448696,181,5.500000,182,10.000000,183,-0.400000,184,0.000000,185,0.000000,186,0.000000,187,0.000000,188,0.000000,189,0.000000,190,0.000000,191,0.000000,192,0.000000,193,0.000000,194,-1.443979,195,2.767753,196,-25.651051,197,359.597656,198,0.000000,199,0.000000,200,5.487028,201,52.231888,202,3.529922,203,0.096222,207,-0.529285,231,-0.108,232,166.47,237,52.1,238,5.546,256,46146.999992,258,7.9,259,4.41,260,67.877,272,168.3,277,0.01,278,0.08,286,0.805,291,-0.067,292,0.050,293,0.199,294,7.000,298,3.531,299,3.501,300,3.530,301,3.498,323,10.250,324,10.250,325,10.240,326,10.230,333,22.302,336,1.385,369,163.1,371,0.727,373,4.43,379,-1.45,393,168.97,394,168.19,395,169.39,396,167.99,401,9.00,402,54.24,403,1.363
    '0,46147.308497,1,0.000,2,4.6,3,4.46,4,3.9,5,4.37,6,167.2,7,6.30,9,163.1,10,0.00,11,356.6,12,0.10,13,163.1,14,0.00,15,1.32,17,42949672.00,18,0.33,19,-0.20,20,6.31,22,0.705,25,0.866,26,23.172,27,0.106,28,-0.191,29,4.683,32,-0.01,48,39.5313155,49,2.5662734,50,356.6,51,0.105,56,10.11,66,0.00,67,1.321,68,1.345,69,67.736,83,207.0,93,2.092,94,167.8,95,4.49,146,0.308506,169,164.262,170,-0.080,171,-0.381,172,0.072640,174,1.730000,175,100.000000,176,1.799639,177,0.000000,178,0.100000,179,0.000000,180,26.448805,181,5.500000,182,10.000000,183,-0.400000,184,0.000000,185,0.000000,186,0.000000,187,0.000000,188,0.000000,189,0.000000,190,0.000000,191,0.000000,192,0.000000,193,0.000000,194,-1.445211,195,2.767739,196,-25.640318,197,359.597656,198,0.000000,199,0.000000,200,5.432306,201,52.417885,202,3.532681,203,0.061403,207,-0.529282,231,-0.101,232,166.45,237,52.3,238,5.502,256,46146.999992,258,5.6,259,4.27,260,67.921,272,167.6,277,0.04,278,0.05,286,0.805,291,0.025,292,-0.082,293,-0.264,294,7.000,298,3.540,299,3.488,300,3.533,301,3.494,323,10.250,324,10.250,325,10.240,326,10.230,333,22.320,336,1.380,369,163.1,371,0.727,373,4.39,379,-1.45,393,168.41,394,167.25,395,168.45,396,167.11,401,9.00,402,54.24,403,1.243

    Dim R1 As String = "!Boat, Utc, TWS, TWD, Lat, Lon"
    Dim R2 As String = "!boat, 0, 5, 6, 48, 49"
    Dim R3 As String = "!v120.6.9"
    Dim Rows As New List(Of String)
    Rows.Add(R1)
    Rows.Add(R2)
    Rows.Add(R3)


    'Dim chUtc = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eDateTime)
    Dim chTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)
    Dim chLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim chLon = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)



    'Dim lats = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lat").FirstOrDefault().Value.ToArray
    'Dim lngs = DictionaryCanali.Where(Function(x) x.Key.ToLower = "lon").FirstOrDefault().Value.ToArray
    Dim lat As Double = chLat.Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
    Dim lng As Double = chLon.Valori.Where(Function(x) Not Double.IsNaN(x) AndAlso Not x = 0).Average
    Dim tzIana As String = TimeZoneLookup.GetTimeZone(lat, lng).Result
    Dim tzInfo As TimeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana)
    'Dim dt As New List(Of DateTime)
    'Dim validdt As New List(Of Double)
    'For Each ut In utc
    '  If Not Double.IsNaN(ut) AndAlso Not ut = 0 Then
    '    Dim t As DateTime = DateTime.FromOADate(ut)
    '    Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(t, tzInfo)
    '    dt.Add(convertedTime.LocalDateTime)
    '    DataDouble.Add(convertedTime.LocalDateTime.ToOADate)
    '    validdt.Add(DataDouble.Last)
    '  Else
    '    dt.Add(Nothing)
    '    DataDouble.Add(Double.NaN)
    '  End If
    'Next
    'Dim day As Date = DateTime.FromOADate(Int(validdt.Select(Function(x) x).Average))
    'FolderDate = day.ToString("yyyyMMdd")
    'ds.Clear()
    'For Each d In dt
    '  If d = Nothing Then
    '    ds.Add(Double.NaN)
    '  Else
    '    If d < day Then
    '      ds.Add((d.TimeOfDay.TotalSeconds - 24 * 3600))
    '    Else
    '      ds.Add(d.TimeOfDay.TotalSeconds)
    '    End If
    '  End If

    'Next



    Dim idiniziale As Integer = TR.IdRigaIniziale
    Dim idfinale As Integer = TR.IdRigaFinale

    For i As Integer = idiniziale To idfinale
      Dim t As DateTime = DataProvider2020.TimeStamps(i)
      Dim convertedTime As DateTimeOffset = TimeZoneInfo.ConvertTimeToUtc(t, tzInfo)
      Dim riga As String = "0,"
      riga += convertedTime.UtcDateTime.ToOADate & ","
      riga += chTws.Valori(i).ToString & ","
      riga += chTwd.Valori(i).ToString & ","
      riga += chLat.Valori(i).ToString & ","
      riga += chLon.Valori(i).ToString
      Rows.Add(riga)
    Next

    ObjFiles.SalvaNuovoFileSostituendoContenuto(Rows, filename)
    ApriExplorer(filename)
  End Sub

  Public Shared Sub SalvaCsv(TR As clsTimeRange, Contenuto As List(Of String))
    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = TR.Start.ToString("yyyyMMdd")
    If TR.Durata.TotalDays >= 1 Then
      pstringadata = TR.Start.ToString("yyyyMMdd") & "_" & TR.Finish.ToString("yyyyMMdd")
    End If
    Dim filename As String = IO.Path.Combine(path, "MeteoReport_" & pstringadata & ".csv")
    ObjFiles.SalvaNuovoFileSostituendoContenuto(Contenuto, filename)
    ApriExplorer(filename)
  End Sub

  Public Shared Sub SalvaKml(TR As clsTimeRange, KmlFile As SharpKml.Engine.KmlFile)
    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = TR.Start.ToString("yyyyMMdd")
    If TR.Durata.TotalDays >= 1 Then
      pstringadata = TR.Start.ToString("yyyyMMdd") & "_" & TR.Finish.ToString("yyyyMMdd")
    End If
    Dim filename As String = IO.Path.Combine(path, "MeteoReport_" & pstringadata & ".kml")
    Dim stream = System.IO.File.OpenWrite(filename)
    KmlFile.Save(stream)
    ApriExplorer(filename)
  End Sub


  Sub Run()
    Console.WriteLine("Creating a point at 37.42052549 latitude and -122.0816695 longitude." & vbLf)
    'Dim point = New Point With {
    '      .Coordinate = New Vector(37.42052549, -122.0816695)
    '  }
    'Dim placemark = New Placemark With {
    '      .Name = "Cool Statue",
    '      .Geometry = point
    '  }
    'Dim kml = New Kml With {
    '      .Feature = placemark
    '  }
    'Dim serializer = New Serializer()
    'serializer.Serialize(kml)
    'Console.WriteLine(serializer.Xml)
    'Console.WriteLine(vbLf & "Reading Xml...")
    'Dim parser = New Parser()
    'parser.ParseString(serializer.Xml, True)
    'kml = CType(parser.Root, Kml)
    'placemark = CType(kml.Feature, Placemark)
    'point = CType(placemark.Geometry, Point)
    'Console.WriteLine("Latitude:{0} Longitude:{1}", point.Coordinate.Latitude, point.Coordinate.Longitude)
  End Sub

End Class

Public Class clsKML

  Public Shared Sub EsportaTracciaKml(TR As clsTimeRange, DataPointsStepsInSeconds As Integer)
    If DataProvider2020 Is Nothing Then Exit Sub
    If Not DataProvider2020.ValoriCaricati Then Exit Sub
    Dim ChLat = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLat)
    Dim ChLng = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eLng)
    Dim ChTws = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWS)
    Dim ChTwd = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWD)

    Dim Lt As New List(Of Double)
    Dim Ln As New List(Of Double)
    Dim Tws As New List(Of Double)
    Dim Twd As New List(Of Double)


    Dim kmlDayfld As New SharpKml.Dom.Folder
    kmlDayfld.Name = "" & TR.Start.ToString("yyyy MM dd")

    Dim kmlfldTrk As New SharpKml.Dom.Folder
    kmlfldTrk.Name = "Boat Track"

    Dim kmlfldDp As New SharpKml.Dom.Folder
    kmlfldDp.Name = "Data Points"

    kmlDayfld.AddFeature(kmlfldTrk)
    kmlDayfld.AddFeature(kmlfldDp)

    Dim Inizio As Integer = TR.IdRigaIniziale
    Dim Fine As Integer = TR.IdRigaFinale

    Dim c As Integer = 0
    Dim pmTrk As New SharpKml.Dom.Placemark
    Dim ls As New SharpKml.Dom.LineString
    ls.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround
    ls.Coordinates = New SharpKml.Dom.CoordinateCollection
    Dim Start As DateTime = TR.Start
    For i As Integer = Inizio To Fine
      If Not Double.IsNaN(ChLat.Valori(i)) Then Lt.Add(ChLat.Valori(i))
      If Not Double.IsNaN(ChLng.Valori(i)) Then Ln.Add(ChLng.Valori(i))
      If Not Double.IsNaN(ChTws.Valori(i)) Then Tws.Add(ChTws.Valori(i))
      If Not Double.IsNaN(ChTwd.Valori(i)) Then Twd.Add(ChTwd.Valori(i))
      c += 1
      Dim adesso As DateTime = DataProvider2020.Momento(i)

      If Not Double.IsNaN(ChLat.Valori(i)) Then
        Dim v As New SharpKml.Base.Vector(ChLat.Valori(i), ChLng.Valori(i))
        ls.Coordinates.Add(v)

        If adesso.Subtract(Start).TotalSeconds >= DataPointsStepsInSeconds Then
          Dim pmdp As New SharpKml.Dom.Placemark
          Dim p As New SharpKml.Dom.Point
          p.Coordinate = New SharpKml.Base.Vector(Lt.Average, Ln.Average)
          p.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround
          pmdp.Name = adesso.AddSeconds(-CInt(DataPointsStepsInSeconds / 2)).ToString("HH:mm") & " Tws: " & Tws.Average.ToString("F1") & ", Twd: " & Media360(Twd.ToArray).ToString("F0") & ""
          pmdp.Geometry = p
          kmlfldDp.AddFeature(pmdp)
          Start = DataProvider2020.Momento(i)
          c = 0
          Lt.Clear()
          Ln.Clear()
          Tws.Clear()
          Twd.Clear()
        End If
      End If
    Next
    pmTrk.Geometry = ls
    kmlfldTrk.AddFeature(pmTrk)

    Dim kml As New SharpKml.Dom.Kml
    kml.Feature = kmlDayfld
    Dim kmlfile = SharpKml.Engine.KmlFile.Create(kml, False)

    Dim path As String = DataProvider2020.Files.First.Directory.FullName
    Dim pstringadata As String = TR.Start.ToString("yyyyMMdd")
    If TR.Durata.TotalDays >= 1 Then
      pstringadata = TR.Start.ToString("yyyyMMdd") & "_" & TR.Finish.ToString("yyyyMMdd")
    End If
    Dim filename As String = IO.Path.Combine(path, "BoatTrack_" & pstringadata & ".kml")
    Dim stream = System.IO.File.OpenWrite(filename)
    kmlfile.Save(stream)
    ApriExplorer(filename)

  End Sub

End Class


Public Class clsVettori

  Public Shared Function ProdottoVettoriale(PrimoVettore As System.Numerics.Vector2, SecondoVettore As System.Numerics.Vector2) As Single
    Return System.Numerics.Vector2.Dot(PrimoVettore, SecondoVettore)
  End Function

  Public Shared Function DistanzaEuclidea(PrimoVettore As System.Numerics.Vector2, SecondoVettore As System.Numerics.Vector2) As Single
    Return System.Numerics.Vector2.Distance(PrimoVettore, SecondoVettore)
  End Function

  Public Shared Function VettoreUnitario(Vettore As System.Numerics.Vector2) As System.Numerics.Vector2
    Return System.Numerics.Vector2.Normalize(Vettore)
  End Function

  Public Shared Function VettoreSomma(PrimoVettore As System.Numerics.Vector2, SecondoVettore As System.Numerics.Vector2) As System.Numerics.Vector2
    Return System.Numerics.Vector2.Add(PrimoVettore, SecondoVettore)
  End Function

  Public Shared Function VettoreDifferenza(VettoreBase As System.Numerics.Vector2, VettoreDaSottrarre As System.Numerics.Vector2) As System.Numerics.Vector2
    Return System.Numerics.Vector2.Subtract(VettoreBase, VettoreDaSottrarre)
  End Function

  Public Shared Function VettoreSomma(PrimoVettore As System.Numerics.Vector3, SecondoVettore As System.Numerics.Vector3) As System.Numerics.Vector3
    Return System.Numerics.Vector3.Add(PrimoVettore, SecondoVettore)
  End Function

  Public Shared Function VettoreDifferenza(VettoreBase As System.Numerics.Vector3, VettoreDaSottrarre As System.Numerics.Vector3) As System.Numerics.Vector3
    Return System.Numerics.Vector3.Subtract(VettoreBase, VettoreDaSottrarre)
  End Function

  Public Shared Function DistanzaEuclidea(PrimoVettore As System.Numerics.Vector3, SecondoVettore As System.Numerics.Vector3) As Single
    Return System.Numerics.Vector3.Distance(PrimoVettore, SecondoVettore)
  End Function

  Public Shared Function RangeAndBearingToVector(Range As Double, Bearing As Double) As System.Numerics.Vector2
    Dim CartBrg As Double = BrgToAlfa(Bearing)
    Dim X As Double = Math.Cos(Radians(CartBrg))
    Dim Y As Double = Math.Sin(Radians(CartBrg))
    Return New System.Numerics.Vector2(X, Y)
  End Function

  Public Shared Function BrgToAlfa(Bearing As Double) As Double
    Dim sBrg As Double = Math.Sin(Radians(Bearing))
    Dim cBrg As Double = Math.Cos(Radians(Bearing))
    Dim tmp As Double = Degrees(Math.Atan2(sBrg, cBrg)) Mod 360
    If tmp < 0 Then
      Return 360 + tmp
    Else
      Return tmp
    End If
  End Function

  Public Shared Function AlfaToBrg(Alfa As Double) As Double
    Dim sAlfa As Double = Math.Sin(Radians(Alfa))
    Dim cAlfa As Double = Math.Cos(Radians(Alfa))
    Dim tmp As Double = Degrees(Math.Atan2(sAlfa, cAlfa)) Mod 360
    If tmp < 0 Then
      Return 360 + tmp
    Else
      Return tmp
    End If
  End Function

End Class


Public Class clsPuntoXYZ
  Public Property X As Double
  Public Property Y As Double
  Public Property Z As Double
  Public Shared Property Vettore3D As System.Numerics.Vector3

  Public Sub New(x As Double, y As Double, z As Double)
    Me.X = x
    Me.Y = y
    Me.Z = z
    Vettore3D = New System.Numerics.Vector3(x, y, z)
  End Sub

  Public Shared Function SommaVettore(VettoreDaAggiungere As System.Numerics.Vector3) As System.Numerics.Vector3
    Return clsVettori.VettoreSomma(Vettore3D, VettoreDaAggiungere)
  End Function

  Public Shared Function SottraiVettore(VettoreDaAggiungere As System.Numerics.Vector3) As System.Numerics.Vector3
    Return clsVettori.VettoreDifferenza(Vettore3D, VettoreDaAggiungere)
  End Function

End Class





