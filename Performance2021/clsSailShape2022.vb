Imports OfficeOpenXml

Public Class clsSailShape2022
  Public PathFile As String
  Public FullListOfStripes As New List(Of SailShapeStripe)
  Dim Intestazioni As New Dictionary(Of Integer, String)
  Public Valid As Boolean = False

  Public Sub New(Folder As String, SalvaParquet As Boolean)
    Dim fld As New System.IO.DirectoryInfo(Folder)
    For Each f In fld.GetFiles
      If f.Extension = ".xlsx" Then
        Try
          PathFile = f.FullName
          LeggiFileSailShapeXls()
          Exit For
        Catch ex As Exception
          Valid = False
        End Try
      ElseIf f.Extension = ".csv" Then
        If f.Name.StartsWith("JIB") Then
          PathFile = f.FullName
          LeggiFileSailShapeCsv(True)
        ElseIf f.Name.StartsWith("MAIN") Then
          PathFile = f.FullName
          LeggiFileSailShapeCsv(False)
        End If
      End If
    Next

    Dim c25 = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_25)
    Dim c50 = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_50)
    Dim c75 = DataProvider2020.Channels.Canale(clsChannels2020.eCanaliChiave.eJibTwistFromFoot_75)
    ReDim c25.Valori(0)
    ReDim c50.Valori(0)
    ReDim c75.Valori(0)

    If SalvaParquet Then
      Dim ChTmp As clsChannel2020 = DataProvider2020.Channels.Canale("MainSail")
      Dim Name = ""
      If Not ChTmp Is Nothing Then Name = "WithSailsAnd"
      LoadingProgressVisualizza()
      For Each f In DataProvider2020.ParquetFiles
        Dim N As New clsParquetCopy(f.FileInfo.FullName, Name & "Shapes")
      Next
      LoadingProgressNascondi()
      ApriExplorer(DataProvider2020.ParquetFiles(0).FileInfo.Directory.FullName)
    End If


    Valid = True
  End Sub

  Private Sub LeggiFileSailShapeXls()

    Dim EP As New ExcelPackage(PathFile)
    Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets
    Dim newChannels As New List(Of clsChannel2020)
    For Each Foglio In Fogli
      Dim s = AddSail(Foglio)
      Dim SailName As String = Foglio.Name.Split("_")(0)
      newChannels.AddRange(CreaCanaliParquet(DataProvider2020, SailName, s))
    Next

    For Each NC As clsChannel2020 In newChannels.ToList
      If DataProvider2020.Channels.ListaCanali.Where(Function(x) x.LongName = NC.LongName).Count = 0 Then
        DataProvider2020.Channels.ListaCanali.Add(NC)
      End If

    Next

  End Sub

  Private Sub LeggiFileSailShapeCsv(IsHeadSail As Boolean)


    Dim cc As List(Of String) = ObjFiles.TestoInLista(PathFile)
    Dim SailName As String = IIf(IsHeadSail, "Jib", "Main")
    Dim s = AddSail(cc, SailName)
    Dim newChannels As New List(Of clsChannel2020)
    newChannels.AddRange(CreaCanaliParquet(DataProvider2020, SailName, s))

    'Dim EP As New ExcelPackage(PathFile)
    'Dim Fogli As ExcelWorksheets = EP.Workbook.Worksheets
    'For Each Foglio In Fogli
    '    Dim s = AddSail(Foglio)
    '    Dim SailName As String = Foglio.Name.Split("_")(0)
    '    newChannels.AddRange(CreaCanaliParquet(DataProvider2020, SailName, s))
    'Next

    For Each NC As clsChannel2020 In newChannels.ToList
      If DataProvider2020.Channels.ListaCanali.Where(Function(x) x.LongName = NC.LongName).Count = 0 Then
        DataProvider2020.Channels.ListaCanali.Add(NC)
      End If

    Next

  End Sub

  Private Function AddSail(FoglioExcel As ExcelWorksheet) As List(Of SailShapeStripe)
    Dim Stripes As New List(Of SailShapeStripe)
    Intestazioni.Clear()
    Dim SailName As String = FoglioExcel.Name.Split("_")(0)
    Dim CellaInBassoDestra As ExcelAddressBase = FoglioExcel.Dimension

    For IdColonna As Integer = 1 To CellaInBassoDestra.Columns ' ArrayDati.GetUpperBound(1)
      Dim C = FoglioExcel.Cells(1, IdColonna).Value
      Dim Cella As String = "" ' ArrayDati(IdRiga, IdColonna)
      If Not C Is Nothing Then
        Cella = C.ToString ' ArrayDati(IdRiga, IdColonna)
        Intestazioni.Add(IdColonna, Cella)
      End If
    Next
    For Each Intestazione As String In Intestazioni.Values
      Dim str As String = Intestazione.Substring(0, 2)
      If IsNumeric(str) Then
        Dim percH As Integer = CInt(str)
        If IsNewStripe(percH, Stripes) Then
          Stripes.Add(New SailShapeStripe(percH, SailName))
        End If
      End If
    Next

    For IdRiga As Integer = 2 To CellaInBassoDestra.Rows ' ArrayDati.GetUpperBound(0)
      For IdColonna As Integer = 1 To CellaInBassoDestra.Columns ' ArrayDati.GetUpperBound(1)
        Dim C = FoglioExcel.Cells(IdRiga, IdColonna)
        AggiungiValore(IdRiga, IdColonna, C, Stripes)
      Next

    Next
    FullListOfStripes.AddRange(Stripes)
    Return Stripes
  End Function

  Private Function AddSail(Righe As List(Of String), SailName As String) As List(Of SailShapeStripe)
    Dim Stripes As New List(Of SailShapeStripe)
    Intestazioni.Clear()


    Dim RigaSplittata = Righe.First.Split(";")
    For IdColonna As Integer = 0 To RigaSplittata.Count - 1 ' ArrayDati.GetUpperBound(1)
      Dim Cella As String = RigaSplittata(IdColonna) ' ArrayDati(IdRiga, IdColonna)
      Intestazioni.Add(IdColonna, Cella)
    Next
    For Each Intestazione As String In Intestazioni.Values
      Dim str As String = Intestazione.Substring(0, 2)
      If IsNumeric(str) Then
        Dim percH As Integer = CInt(str)
        If IsNewStripe(percH, Stripes) Then
          Stripes.Add(New SailShapeStripe(percH, SailName))
        End If
      End If
    Next

    For IdRiga As Integer = 1 To Righe.Count - 1 ' ArrayDati.GetUpperBound(0)
      RigaSplittata = Righe(IdRiga).Split(";")
      If Not RigaSplittata(0) = "" Then
        For IdColonna As Integer = 0 To RigaSplittata.Count - 1 ' ArrayDati.GetUpperBound(1)
          Dim Cella As String = RigaSplittata(IdColonna) ' ArrayDati(IdRiga, IdColonna)
          AggiungiValore(IdRiga, IdColonna, Cella, Stripes)
        Next
      End If
    Next
    FullListOfStripes.AddRange(Stripes)
    Return Stripes
  End Function

  Private Function IsNewStripe(Height As Integer, Stripes As List(Of SailShapeStripe)) As Boolean
    For Each Stripe As SailShapeStripe In Stripes
      If Stripe.StripePercentageHeight = Height Then
        Return False
      End If
    Next
    Return True
  End Function
  Private Function GetStripe(Height As String, Stripes As List(Of SailShapeStripe)) As SailShapeStripe
    For Each Stripe As SailShapeStripe In Stripes
      If Stripe.StripePercentageHeight = CInt(Height) Then
        Return Stripe
      End If
    Next
    Return Nothing
  End Function


  Dim CurrentDate As DateTime
  Dim CurrentTime As DateTime
  Dim LastPictureName As String

  Private Sub AggiungiValore(IdRiga As Integer, IdColonna As Integer, CellaValore As ExcelRange, Stripes As List(Of SailShapeStripe))
    If Intestazioni.ContainsKey(IdColonna) Then
      Dim Colonna As String = Intestazioni(IdColonna).ToLower
      Select Case Colonna
        Case "date"
          CurrentDate = CellaValore.Value
        Case "hour"
          CurrentTime = CellaValore.Value
        Case "filename"
          LastPictureName = CellaValore.Value.ToString
        Case Else
          If Not (IsNumeric(CellaValore.Value)) Then Exit Select
          Dim sH As String = Colonna.Substring(0, 2)
          If IsNumeric(sH) Then
            Dim Str = GetStripe(sH, Stripes)
            If Not Str Is Nothing Then
              Dim Canale As String = Colonna.Substring(3)
              Select Case Canale.ToLower
                Case "height"
                  Str.StripePercentageHeight = sH
                  Str.StripeShapeValues.Add(New SailShapeValue(Str))
                  Str.StripeShapeValues.Last().UpdatePictureData(CurrentDate, CurrentTime, LastPictureName)
                Case "camber"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eCamber, CellaValore.Value)
                Case "draft"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eDraft, CellaValore.Value)
                Case "dtwist"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eTwist, CellaValore.Value)
                Case "entry"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eEntry, CellaValore.Value)
                Case "exit"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eExit, CellaValore.Value)
                Case "front_cam"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eFrontCamber, CellaValore.Value)
                Case "back_cam"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eBackCamber, CellaValore.Value)
              End Select
            End If
          End If
      End Select

    End If

  End Sub

  Private Sub AggiungiValore(IdRiga As Integer, IdColonna As Integer, Valore As String, Stripes As List(Of SailShapeStripe))
    If Intestazioni.ContainsKey(IdColonna) Then
      Dim Colonna As String = Intestazioni(IdColonna).ToLower
      Select Case Colonna
        Case "date"
          'CurrentDate = Valore
          If Not DateTime.TryParseExact(Valore, "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, CurrentDate) Then
            Stop
          End If
        Case "hour"
          'CurrentTime = Valore
          If Not DateTime.TryParseExact(Valore, "HH:mm:ss", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, CurrentTime) Then
            Stop
          End If
        Case "filename"
          LastPictureName = Valore.ToString
        Case Else
          If Not (IsNumeric(Valore)) Then Exit Select
          Dim sH As String = Colonna.Substring(0, 2)
          If IsNumeric(sH) Then
            Dim Str = GetStripe(sH, Stripes)
            If Not Str Is Nothing Then
              Dim Canale As String = Colonna.Substring(3)
              Select Case Canale.ToLower
                Case "height"
                  Str.StripePercentageHeight = sH
                  Str.StripeShapeValues.Add(New SailShapeValue(Str))
                  Str.StripeShapeValues.Last().UpdatePictureData(CurrentDate, CurrentTime, LastPictureName)
                Case "camber"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eCamber, Valore)
                Case "draft"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eDraft, Valore)
                Case "dtwist"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eTwist, Valore)
                Case "entry"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eEntry, Valore)
                Case "exit"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eExit, Valore)
                Case "front_cam"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eFrontCamber, Valore)
                Case "back_cam"
                  Str.StripeShapeValues.Last().ParameterValue.Add(SailShapeValue.eSailShapeParameter.eBackCamber, Valore)
              End Select
            End If
          End If
      End Select

    End If
  End Sub


  Public Function CreaCanaliParquet(DP As clsDataProvider2020, SailName As String, Stripes As List(Of SailShapeStripe)) As List(Of clsChannel2020)
    Dim ctmp As New List(Of clsChannel2020)
    Dim DictTmp As New Dictionary(Of SailShapeStripe, List(Of clsChannel2020))

    For Each Str As SailShapeStripe In Stripes
      Dim lt As New List(Of clsChannel2020)
      ctmp.Add(CreaCanaleParquet(Str, "Camber", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "Draft", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "dTwist", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "Entry", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "Exit", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "FrontCamber", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      ctmp.Add(CreaCanaleParquet(Str, "BackCamber", SailName)) ' , Str.Values, DP))
      lt.Add(ctmp.Last)
      DictTmp.Add(Str, lt)
    Next

    For Each Stripe In DictTmp.Keys
      For Each Canale As clsChannel2020 In DictTmp(Stripe)
        RiempiCanaleParquet(Canale, Stripe, DP)
        'RiempiCanaleParquet(Canale, Stripe.Values, DP, Stripe.StripePercentageHeight.ToString)
      Next
    Next

    Return ctmp
  End Function

  Private Function CreaCanaleParquet(Stripe As SailShapeStripe, NomeCanale As String, Vela As String) As clsChannel2020
    Dim c As New clsChannel2020()
    c.ChannelId = Vela & "_" & Stripe.StripePercentageHeight & "_" & NomeCanale
    c.ActualLogHeader = c.ChannelId
    c.DataType = clsChannel2020.eDataType.ePercentage
    c.CanaleChiave = clsChannels2020.eCanaliChiave.eNone
    c.Decimals = 1
    c.IsMath = False
    c.LongName = Vela & "_" & Stripe.StripePercentageHeight & "_" & NomeCanale
    c.LongUM = "Perc"
    c.PolarHeader = c.ActualLogHeader & "_tgt"
    c.ShortName = c.LongName
    c.ShortUM = "%"
    c.SailName = Vela
    c.StripeHeight = Stripe.StripePercentageHeight
    c.StripeParameter = NomeCanale
    Return c
  End Function

  Private Sub RiempiCanaleParquet(Canale As clsChannel2020, Stripe As SailShapeStripe, DP As clsDataProvider2020)
    ReDim Canale.Valori(DP.TimeStamps.Count - 1)

    'Dim t As Nullable(Of Double)()
    'ReDim t(DP.TimeStamps.Count - 1)

    For Each valore In Canale.Valori
      valore = Double.NaN
    Next

    For Each StripeShape As SailShapeValue In Stripe.StripeShapeValues
      Dim IdAnte As Integer = DP.TrovaIndiceDaCanaleDT(StripeShape.DT.AddSeconds(-5))
      Dim IdPost As Integer = DP.TrovaIndiceDaCanaleDT(StripeShape.DT.AddSeconds(5))
      For Each Valore In StripeShape.ParameterValue
        If IdAnte > -1 AndAlso IdPost > -1 Then
          For i As Integer = IdAnte To IdPost
            Select Case Canale.StripeParameter.ToLower
              Case "camber"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eCamber)
              Case "draft"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eDraft)
              Case "dtwist"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eTwist)
              Case "entry"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eEntry)
              Case "exit"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eExit)
              Case "backcamber"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eBackCamber)
              Case "frontcamber"
                Canale.Valori(i) = StripeShape.ParameterValue(SailShapeValue.eSailShapeParameter.eFrontCamber)
            End Select
          Next
        End If
      Next
    Next



  End Sub



End Class




Public Class SailShapeStripe
  Public SailName As String
  Public StripePercentageHeight As Integer
  Public StripeShapeValues As New List(Of SailShapeValue)


  Public Sub New(PercentageHeight As Integer, Name As String)
    StripePercentageHeight = PercentageHeight
    SailName = Name
  End Sub

End Class

Public Class SailShapeValue
  Public ParameterValue As New Dictionary(Of eSailShapeParameter, Double)
  Public Parent As SailShapeStripe

  Dim _Data As String
  Dim _Ora As String
  Public PictureName As String
  Public DT As DateTime

  Public Sub New(ParentSailShapeStripe As SailShapeStripe)
    Parent = ParentSailShapeStripe
  End Sub


  Public Enum eSailShapeParameter
    eCamber
    eDraft
    eTwist
    eEntry
    eExit
    eFrontCamber
    eBackCamber
    eChord
    eFloorLenght
  End Enum


  Public Sub UpdatePictureData(Data As DateTime, Ora As DateTime, PicName As String)

    Dim Offset = AppConfig.ActiveProfile.SailFilesOffset


    _Data = Data.ToString("dd/MM/yyyy")
    _Ora = Ora.ToString("HH:mm:ss")
    PictureName = PicName

    If DateTime.TryParseExact(_Data & " " & _Ora, "dd/MM/yyyy HH:mm:ss", Nothing, Globalization.DateTimeStyles.None, DT) Then
      Dim LDT = DT.Add(Offset)
      _Data = LDT.ToString("dd/MM/yyyy")
      _Ora = LDT.ToString("HH:mm:ss")
      DT = LDT
    Else
      Stop
    End If
    'If DT = Nothing Then Stop


  End Sub

End Class



Public Class clsStripePanel
  Public Property UpperPanel As SailShapeValue
  Public Property LowerPanel As SailShapeValue

  Public Property ArmPercentageHeight As Double
  Public Property LiftForce As Double
  Public Property InducedDragForce As Double
  Public Property InducedDragLongitiudinal As Double
  Public Property DrivingForce As Double
  Public Property HeelingForce As Double


  Public Sub GetLift(UpPanel As SailShapeValue, LwPanel As SailShapeValue, Aws As Double, Awa As Double, TwistOffset As Double)
    UpperPanel = UpPanel
    LowerPanel = LwPanel
    Dim PanelHeight As Double
    Dim ro As Double = 1.255 '(Kg/m3)
    Dim e As Double = 0.9 '(Kg/m3)
    Dim AlfaZero As Double = 2 * Math.PI
    Dim UpChord As Double
    Dim UpArea As Double = UpChord * PanelHeight
    Dim UpAoARad As Double = Radians(Math.Abs(Awa) - (UpPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eTwist) + TwistOffset))
    Dim UpAR As Double = PanelHeight / UpArea
    Dim UpAlfaElleZero As Double = 2 * (UpPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eCamber) / 100)
    Dim UpCL2D As Double = AlfaZero * (UpAoARad - UpAlfaElleZero)

    Dim UpAlfa As Double = AlfaZero / (1 + AlfaZero / Math.PI * e * UpAR)
    Dim UpCL3D As Double = AlfaZero * (UpAoARad - UpAlfaElleZero)

    Dim LwChord As Double
    Dim LwArea As Double = LwChord * PanelHeight
    Dim LwAoARad As Double = Radians(Math.Abs(Awa) - (LwPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eTwist) + TwistOffset))
    Dim LwAR As Double = PanelHeight / LwArea
    Dim LwAlfaElleZero As Double = 2 * (UpPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eCamber) / 100)
    Dim LwCL2D As Double = 2 * Math.PI * (LwAoARad - LwAlfaElleZero)

    Dim LwAlfa As Double = AlfaZero / (1 + AlfaZero / Math.PI * e * LwAR)
    Dim LwCL3D As Double = AlfaZero * (LwAoARad - LwAlfaElleZero)

    Dim UpCl = UpCL2D
    Dim LwCl = LwCL2D

    If True Then
      UpCl = UpCL3D
      LwCl = LwCL3D
    End If

    Dim UpLift As Double = 0.5 * ro * Math.Pow(Aws, 2) * UpArea * UpCl
    Dim UpChordLiftX = UpLift * Math.Sin(Radians(UpAoARad))
    Dim UpChordLiftY = UpLift * Math.Cos(Radians(UpAoARad))


    Dim LwLift As Double = 0.5 * ro * Math.Pow(Aws, 2) * LwArea * LwCl
    Dim LwChordLiftX = LwLift * Math.Sin(Radians(LwAoARad))
    Dim LwChordLiftY = LwLift * Math.Cos(Radians(LwAoARad))

    Dim UpCD2D = Math.Pow(UpCl, 2) / (Math.PI * e * UpAR)
    Dim LwCD2D = Math.Pow(LwCl, 2) / (Math.PI * e * LwAR)

    Dim kH As Double = LwLift / (UpLift + LwLift)

    Dim dH = UpPanel.Parent.StripePercentageHeight - LwPanel.Parent.StripePercentageHeight
    Dim upC = UpPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eChord)
    Dim lwC = LwPanel.ParameterValue(SailShapeValue.eSailShapeParameter.eChord)
    Dim hs = dH / 3 * ((lwC + 2 * upC) / (lwC + upC))
    ArmPercentageHeight = LwPanel.Parent.StripePercentageHeight + hs



  End Sub


End Class

Public Class clsSailShapeUtilities

  Public Shared Sub ExportStraightLinesPeriods(Periods As List(Of clsPeriod2021), DP As clsDataProvider2020)
    Dim VmgUp As Boolean = True
    Dim VmgDn As Boolean = False
    Dim Reaching As Boolean = False
    Dim ReachingMinTwa As Integer = 70
    Dim ReachingMaxTwa As Integer = 120

    Dim chTws As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eTWS)
    Dim chTwa As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eTWA)
    Dim chVmgTgtP As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eVMGp)
    Dim chBsTgtP As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eBSTp)
    Dim chBsPolP As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eBSPp)
    Dim chEnv As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eEnvironmentQuality)
    Dim chAtt As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eAttitudeQuality)
    Dim chQI As clsChannel2020 = DP.Channels.Canale(clsChannels2020.eCanaliChiave.eDataQuality)

    chTwa.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    chTws.StatisticheIntervallo.ChannelId = chTws.ChannelId
    'chBsTgtP.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    'chBsPolP.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    'chVmgTgtP.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    'chEnv.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    'chAtt.StatisticheIntervallo.ChannelId = chTwa.ChannelId
    'chQI.StatisticheIntervallo.ChannelId = chTwa.ChannelId



    Dim Righe As New List(Of String)

    Dim riga As String = "From" & vbTab & "To" & vbTab & "Tack" & vbTab & "Tws" & vbTab & "Twa" & vbTab & "Vmg%" & vbTab & "BsTgt%" & vbTab & "BsPolar%" & vbTab & "DataQualityIndex" & vbTab & "EnvironmentQualityIndex" & vbTab & "AttitudeQualityIndex"
    Righe.Add(riga)
    For Each p As clsPeriod2021 In Periods
      If p.PeriodType = clsPeriod2021.ePeriodType.eStraightLineVmg Then
        If (VmgUp AndAlso p.IsUpwindVmgRange) OrElse (VmgDn AndAlso p.IsDownwindVmgRange) Then
          chTwa.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chTws.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chVmgTgtP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chBsTgtP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chBsPolP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chEnv.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chAtt.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chQI.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")

          riga = p.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eSoloTime) & vbTab
          riga &= p.TR.FineFormattato(clsTimeRange.eTipoFormatoData.eSoloTime) & vbTab
          riga &= IIf(p.IsStbd, "Stbd", "Port") & vbTab
          riga &= chTws.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chTwa.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= chVmgTgtP.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chBsTgtP.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chBsPolP.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chQI.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= chEnv.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= chAtt.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          Righe.Add(riga)
        End If
      ElseIf p.PeriodType = clsPeriod2021.ePeriodType.eStraightLineReaching Then
        If Reaching AndAlso p.StraightLineReachingDetails.Twa.AvgVal >= ReachingMinTwa And p.StraightLineReachingDetails.Twa.AvgVal <= ReachingMaxTwa Then
          chTwa.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chTws.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          'chVmgTgtP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue,"")
          'chBsTgtP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue,"")
          chBsPolP.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chEnv.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chAtt.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")
          chQI.StatisticheIntervallo.AggiornaIntervallo(p.TR, clsGroupLines.eLineType.eRawValue, "")

          riga = p.TR.InizioFormattato(clsTimeRange.eTipoFormatoData.eSoloTime) & vbTab
          riga &= p.TR.FineFormattato(clsTimeRange.eTipoFormatoData.eSoloTime) & vbTab
          riga &= IIf(p.IsStbd, "Stbd", "Port") & vbTab
          riga &= chTws.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chTwa.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= "N.A." & vbTab
          riga &= "N.A." & vbTab
          riga &= chBsPolP.StatisticheIntervallo.Avg.ToString("F1") & vbTab
          riga &= chQI.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= chEnv.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          riga &= chAtt.StatisticheIntervallo.Avg.ToString("F0") & vbTab
          Righe.Add(riga)
        End If
      End If
    Next

    Clipboard.SetText(String.Join(vbCrLf, Righe))

    MsgBox("Results to the Clipboard")

  End Sub

End Class
