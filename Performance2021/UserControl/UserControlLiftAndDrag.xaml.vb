Imports System.Numerics
Imports PropertyChanged

Public Class UserControlLiftAndDrag
  Public Property VM As New clsLiftAndDragCompare


  Public Sub New()
    Me.DataContext = VM
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

  End Sub


  Private Sub NumericOnly(sender As Object, e As TextCompositionEventArgs)
    If Not IsNumeric(e.Text) Then
      e.Handled = True
    End If
  End Sub

  Private Function IsNumeric(testo As String) As Boolean
    Dim objRegExp As New System.Text.RegularExpressions.Regex("[0-9.-]") 'digits only  [^0-9.-]+   ^\d+$
    Dim match As System.Text.RegularExpressions.Match = objRegExp.Match(testo)
    If match.Success Then ' OrElse testo = "."  Then
      Return True
    End If
    Return False
  End Function

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    VM.ImpostazioniIniziali()
  End Sub

  Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
    VM.LiftAndDragBaseLine.CalcolaTutto()
    VM.LiftAndDragChanges.CalcolaTutto()
    VM.TWSeq = VM.LiftAndDragBaseLine.TwsEquivalent(VM.LiftAndDragChanges, VM.Twa, VM.Tws)
  End Sub
End Class


<AddINotifyPropertyChangedInterface>
Public Class clsLiftAndDragSettings
  Public Property Name As String ' in Celsius degrees
  Public Property AirT As Double ' in Celsius degrees
  Public Property AirP As Double ' in Millibar/hPa
  Public Property RelHumidity As Double ' 0-100 %
  Public Property AirFlowSpeed As Double ' in m/s
  Public Property Leeway As Double ' in degrees
  Public Property WingSpan As Double ' in m
  Public Property WingCamber As Double ' % of Chord
  Public Property WingChord As Double ' in m
  Public Property K As Double ' ratio between Aoa and Cl, depends on the section shape
  Public Property AirFlowAngleOfAttack As Double ' Aoa in degrees
  Public Property DragK As Double ' Drag Coefficent
  Public Property WingEfficencyFactor As Double ' Drag Coefficent
  Public Property WingSettingAngle As Double


  Public Sub ImpostazioniDefault(Name As String)
    Me.Name = Name
    AirT = 15 ' in Celsius degrees
    AirP = 1013 ' in Millibar/hPa
    RelHumidity = 50 ' 0-100 %
    AirFlowSpeed = 6 ' in m/s
    Leeway = 4 ' in m/s
    WingSpan = 30 ' in m
    WingCamber = 7 ' Chord %
    WingChord = 3.5 ' in m
    K = 0.2 ' ratio between Aoa and Cl, depends on the section shape
    AirFlowAngleOfAttack = 15 ' Aoa in degrees
    DragK = 0.2 ' Drag Coefficent
    WingEfficencyFactor = 0.3 ' Drag Coefficent
    WingSettingAngle = 5
  End Sub


  Public Function Clone() As clsLiftAndDragSettings
    Return Me.MemberwiseClone
  End Function

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsLiftAndDragCompare
  Public Property LiftAndDragBaseLine As New clsLiftAndDrag
  Public Property LiftAndDragChanges As New clsLiftAndDrag


  Public Property Tws As Double = 10
  Public Property Twa As Double = 45
  Public Property Leeway As Double = 4
  Public Property Aws As Double
  Public Property Awa As Double
  Public Property TWSeq As Double


  Public Sub ImpostazioniIniziali()

    If TgtManager Is Nothing Then Exit Sub
    If TgtManager.Tgt Is Nothing Then Exit Sub

    Dim p = TgtManager.Tgt.Valore(Tws, Twa, "bs")

    Awa = p.Awa
    Aws = p.Aws

    LiftAndDragBaseLine.Settings = New clsLiftAndDragSettings
    LiftAndDragBaseLine.Settings.ImpostazioniDefault("BaseLine")
    LiftAndDragBaseLine.Settings.AirFlowSpeed = Aws / 3600 * 1852
    LiftAndDragBaseLine.Settings.AirFlowAngleOfAttack = Awa
    LiftAndDragBaseLine.Settings.Leeway = Leeway
    LiftAndDragBaseLine.CalcolaTutto()

    LiftAndDragChanges = LiftAndDragBaseLine.Clone
    LiftAndDragChanges.Settings.Name = "Changes"
    LiftAndDragChanges.CalcolaTutto()

    TWSeq = LiftAndDragBaseLine.TwsEquivalent(LiftAndDragChanges, Twa, Tws)

  End Sub

End Class

<AddINotifyPropertyChangedInterface>
Public Class clsLiftAndDragDiff
  Public Property Settings As clsLiftAndDragSettings

  Public Property LiftVectorAngle As Double
  Public Property DragVectorAngle As Double


  Public Property Lift As Double
  Public Property CiEl As Double
  Public Property Rho As Double
  Public Property DewPoint As Double
  Public Property WingArea As Double
  Public Property TotalDrag As Double
  Public Property DragAtZeroLift As Double
  Public Property InducedDrag As Double
  Public Property AspectRatio As Double
  Public Property DrivingForce As Double
  Public Property LiftVectorBoatAxis As Vector2
  Public Property TotalDragVectorBoatAxis As Vector2


End Class

<AddINotifyPropertyChangedInterface>
Public Class clsLiftAndDragVectorTable
  Public Property LiftAndDragValues As clsLiftAndDrag(,)
  'Public Property LiftValues As Vector2(,)
  'Public Property DragValues As Vector2(,)
  Dim _Interpolatore As MathNet.Numerics.Interpolation.IInterpolation

  Public Sub New(Optional Crea As Boolean = False)
    If Crea Then
      Dim tmp = CreaConfigurazioneDefault()
      ImpostaTabelle(tmp)
    End If
  End Sub

  Public Function CreaConfigurazioneDefault() As clsLiftAndDrag
    Dim Ctmp As New clsLiftAndDrag

    If TgtManager Is Nothing Then Return Nothing
    If TgtManager.Tgt Is Nothing Then Return Nothing

    Dim p = TgtManager.Tgt.Valore(10, 45, "bs")

    Dim Awa = p.Awa
    Dim Aws = p.Aws

    Ctmp.Settings = New clsLiftAndDragSettings
    Ctmp.Settings.ImpostazioniDefault("BaseLine")
    Ctmp.Settings.AirFlowSpeed = Aws / 3600 * 1852
    Ctmp.Settings.AirFlowAngleOfAttack = Awa
    Ctmp.Settings.Leeway = 4
    Ctmp.CalcolaTutto()

    Return Ctmp

  End Function

  Public Sub ImpostaTabelle(Configurazione As clsLiftAndDrag)
    ReDim LiftAndDragValues(41, 181)
    'ReDim LiftValues(41, 181)
    'ReDim DragValues(41, 181)
    If TgtManager Is Nothing Then Exit Sub
    If TgtManager.Tgt Is Nothing Then Exit Sub

    For tws = 0 To 40
      For twa = 0 To 180
        Dim tgt = TgtManager.Tgt.Valore(tws, twa, "bs")
        Configurazione.Settings.AirFlowAngleOfAttack = tgt.Awa
        Configurazione.Settings.AirFlowSpeed = tgt.Aws / 3600 * 1852
        Configurazione.Settings.WingSettingAngle = WingSettingAngleFromApparent(tgt.Aws, tgt.Awa)
        Configurazione.Settings.Leeway = LeewayFromTrue(tws, twa)
        Configurazione.CalcolaTutto()
        LiftAndDragValues(tws, twa) = Configurazione.Clone
        'LiftValues(tws, twa) = Configurazione.LiftVectorBoatAxis
        'DragValues(tws, twa) = Configurazione.TotalDragVectorBoatAxis
      Next
    Next

  End Sub

  Private Function WingSettingAngleFromApparent(AwsKts As Double, Awa As Double) As Double
    Dim AngoliAwa As Double() = {0, 10, 20, 40, 70, 100, 120, 150, 180}
    Dim ValoriWsa As Double() = {0, 2, 5, 10, 25, 35, 50, 70, 90}
    Dim Vtmp = ValoreInterpolazioneLineare(Awa, AngoliAwa, ValoriWsa)

    Dim ValoriAws As Double() = {0, 20, 40, 60, 80, 100}
    Dim ValoriWsaAdd As Double() = {0, 0.5, 1, 2, 3, 5}
    Dim Addtmp = ValoreInterpolazioneLineare(AwsKts, ValoriAws, ValoriWsaAdd)
    Return Vtmp + Addtmp

  End Function

  Private Function LeewayFromTrue(TwsKts As Double, Twa As Double) As Double
    Dim AngoliTwa As Double() = {0, 30, 70, 100, 120, 180}
    Dim ValoriLwy As Double() = {0, 4, 4, 3, 2, 0}
    Dim Vtmp = ValoreInterpolazioneLineare(Twa, AngoliTwa, ValoriLwy)

    Dim ValoriTws As Double() = {0, 10, 20, 30, 50}
    Dim ValoriLwyAdd As Double() = {0, 0, 0.5, 1, 2}
    Dim Addtmp = ValoreInterpolazioneLineare(TwsKts, ValoriTws, ValoriLwyAdd)
    Return Vtmp + Addtmp
  End Function

  Private Function ValoreInterpolazioneLineare(AwValue As Double, AwValues As Double(), WsaValues As Double()) As Double
    _Interpolatore = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(AwValues, WsaValues)
    Dim Int1 As Double = _Interpolatore.Interpolate(AwValue)
    Return Int1
  End Function


End Class


<AddINotifyPropertyChangedInterface>
  Public Class clsLiftAndDrag
    Public Property Settings As clsLiftAndDragSettings

    Public Property LiftVectorAngle As Double
    Public Property DragVectorAngle As Double


    Public Property Lift As Double
    Public Property CiEl As Double
    Public Property Rho As Double
    Public Property DewPoint As Double
    Public Property WingArea As Double
    Public Property TotalDrag As Double
    Public Property DragAtZeroLift As Double
    Public Property InducedDrag As Double
    Public Property AspectRatio As Double
  Public Property DrivingForce As Double
  Public Property LateralForce As Double
  Public Property LiftVectorBoatAxis As Vector2
    Public Property TotalDragVectorBoatAxis As Vector2

    Public ReadOnly Property LiftVectorBoatAxisX As Double
      Get
        Return LiftVectorBoatAxis.X
      End Get
    End Property

    Public ReadOnly Property LiftVectorBoatAxisY As Double
      Get
        Return LiftVectorBoatAxis.Y
      End Get
    End Property

    Public ReadOnly Property TotalDragVectorBoatAxisX As Double
      Get
        Return TotalDragVectorBoatAxis.X
      End Get
    End Property

    Public ReadOnly Property TotalDragVectorBoatAxisY As Double
      Get
        Return TotalDragVectorBoatAxis.Y
      End Get
    End Property

    Public Sub CalcolaTutto(Optional SettingsName As String = "")
        Try
            If Not SettingsName = "" Then
                If AppConfig.ActiveProfile.LiftAndDragSettings Is Nothing Then
                    Settings = New clsLiftAndDragSettings
                    Settings.ImpostazioniDefault(SettingsName)
                    AppConfig.ActiveProfile.LiftAndDragSettings.Add(Settings)
                    AppConfig.Salva()
                Else
                    Settings = AppConfig.ActiveProfile.LiftAndDragSettings.Where(Function(x) x.Name = SettingsName).FirstOrDefault
                    If Settings Is Nothing Then
                        Settings = New clsLiftAndDragSettings
                        Settings.ImpostazioniDefault(SettingsName)
                        AppConfig.ActiveProfile.LiftAndDragSettings.Add(Settings)
                        AppConfig.Salva()
                    End If
                End If
            End If


            CalcolaRho()
            CalcolaCiEl()

            CalcolaWingArea()

            CalcolaLift()

            CalcolaLiftVectorBoatAxis()
            CalcolaDragAtZeroLift()

            CalcolaAspectRatio()
            CalcolaInducedDrag()
            CalcolaTotalDrag()
            CalcolaTotalDragVectorBoatAxis()
            CalcolaDrivingForce()
            CalcolaLateralForce()
        Catch ex As Exception

        End Try

    End Sub

    Public Function Clone() As clsLiftAndDrag
      Dim t As clsLiftAndDrag
      t = Me.MemberwiseClone
      t.Settings = Settings.Clone
      Return t
    End Function


    ' Lift = 1/2*V^2*Rho*A*Cl(=K*AoA)

    ' AoA = Lift/(1/2*V^2*Rho*A*K)

    Private Sub CalcolaLift()
      Lift = 0.5 * Math.Pow(Settings.AirFlowSpeed, 2) * CiEl() * Rho() * WingArea
    End Sub

    Private Sub CalcolaCiEl()
      CiEl = Settings.K * (Settings.AirFlowAngleOfAttack - Settings.WingSettingAngle)
    End Sub

    Private Function Kelvin(Celsius As Double) As Double
      Return Celsius + 273.15
    End Function

    Private Function Celsius(Kelvin As Double) As Double
      Return Kelvin - 273.15
    End Function

    Private Sub CalcolaRho()
        Try
            Dim elev = (7.5 * Settings.AirT) / Kelvin(Settings.AirT)
            Dim p1 = 6.1078 * Math.Pow(10, elev)
            Dim VapP As Double = p1 * Settings.RelHumidity
            Dim DryAirP = (Settings.AirP * 100) - VapP
            Dim rd As Double = 287.058
            Dim rv As Double = 461.495
            Rho = (DryAirP / (rd * Kelvin(Settings.AirT))) + (VapP / (rv * Kelvin(Settings.AirT)))

            Dim alpha As Double = Math.Log(Settings.RelHumidity / 100) + (17.62 * Settings.AirT / (243.12 + Settings.AirT))
            DewPoint = (243.12 * alpha) / (17.62 - alpha)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub CalcolaWingArea()
      WingArea = Settings.WingSpan * Settings.WingChord
    End Sub

    Private Sub CalcolaTotalDrag()
      TotalDrag = InducedDrag() + DragAtZeroLift()
    End Sub

    Private Sub CalcolaDragAtZeroLift()
      'Dim Fa = Settings.WingCamber / 100 * Settings.WingChord * Math.Cos(Radians(Settings.AirFlowAngleOfAttack)) + (Settings.WingSpan * Settings.WingChord * Math.Sin(Radians(Settings.AirFlowAngleOfAttack)))
      ''Fa = Settings.WingFrontFace

      'Fa = (Settings.WingCamber / 100 * Settings.WingChord * Math.Cos(Radians(Settings.AirFlowAngleOfAttack)) + Settings.WingChord * Math.Sin(Radians(Settings.AirFlowAngleOfAttack))) * Settings.WingSpan
      DragAtZeroLift = 0.5 * Settings.DragK * WingArea * Rho() * Math.Pow(Settings.AirFlowSpeed, 2)
    End Sub

    Private Sub CalcolaInducedDrag()
      InducedDrag = Math.Pow(CiEl, 2) / (Math.PI * AspectRatio() * Settings.WingEfficencyFactor)
    End Sub

    Private Sub CalcolaAspectRatio()
      AspectRatio = Math.Pow(Settings.WingSpan, 2) / WingArea
    End Sub

  Private Sub CalcolaDrivingForce()
    DrivingForce = LiftVectorBoatAxis.X + TotalDragVectorBoatAxis.X
  End Sub

  Private Sub CalcolaLateralForce()
    LateralForce = LiftVectorBoatAxis.Y + TotalDragVectorBoatAxis.Y
  End Sub

  Private Sub CalcolaLiftVectorBoatAxis()
      LiftVectorAngle = 90 - (Settings.WingSettingAngle + Settings.Leeway)
      Dim L As Double = Lift()
      Dim x As Double = L * Math.Cos(Radians(LiftVectorAngle))
      Dim y As Double = L * Math.Sin(Radians(LiftVectorAngle))
      LiftVectorBoatAxis = New Vector2(x, y)
    End Sub


    Private Sub CalcolaTotalDragVectorBoatAxis()
      DragVectorAngle = 180 - Settings.WingSettingAngle
      Dim TD As Double = TotalDrag()
      Dim x As Double = TD * Math.Cos(Radians(DragVectorAngle))
      Dim y As Double = TD * Math.Sin(Radians(DragVectorAngle))
      TotalDragVectorBoatAxis = New Vector2(x, y)
    End Sub

  'Public Function TwsEquivalent(DeltaAirTemp As Double, DeltaPressure As Double, DeltaRelHumidity As Double, UpwindVmg As Boolean) As Double
  'Dim NewLift As clsLiftAndDrag = Me.Clone
  'NewLift.Settings.AirT += DeltaAirTemp
  '  NewLift.Settings.AirP += DeltaPressure
  '  NewLift.Settings.RelHumidity += DeltaRelHumidity
  '  NewLift.CalcolaTutto()
  '  Return TwsEquivalent(NewLift, UpwindVmg)
  'End Function

  'Public Function TwsEquivalent(NewLift As clsLiftAndDrag, UpwindVmg As Boolean) As Double
  '  ' cerca sul tgt up o down
  '  If TgtManager Is Nothing Then Return 0
  '  If TgtManager.Tgt Is Nothing Then Return 0
  '  Dim Delta As Double = 1000
  'Dim LaDtmp As clsLiftAndDrag = Me.Clone
  'Dim TwsTmp As Double = 4
  '  For i As Double = 4 To 40 Step 0.1
  '    Dim tgt = TgtManager.Tgt.ValoreTgt(UpwindVmg, i, "bs")
  '    LaDtmp.Settings.AirFlowAngleOfAttack = tgt.Awa
  '    LaDtmp.Settings.AirFlowSpeed = tgt.Aws
  '    LaDtmp.CalcolaTutto()
  '    Dim LaDlift = LaDtmp.DrivingForce
  '    If Math.Abs(LaDlift - NewLift.DrivingForce) < Delta Then
  '      Delta = LaDlift - NewLift.DrivingForce
  '      TwsTmp = i
  '    End If
  '  Next
  '  Return TwsTmp
  'End Function

  Public Function TwsEquivalent(DeltaAirTemp As Double, DeltaPressure As Double, DeltaRelHumidity As Double, Twa As Double, ActualTws As Double) As Double
    Dim NewLift As clsLiftAndDrag = Me.Clone
    NewLift.Settings.AirT += DeltaAirTemp
    NewLift.Settings.AirP += DeltaPressure
    NewLift.Settings.RelHumidity += DeltaRelHumidity
    Return TwsEquivalent(NewLift, Twa, ActualTws)
  End Function

  Public Function TwsEquivalent(NewLift As clsLiftAndDrag, Twa As Double, ActualTws As Double) As Double

    If TgtManager Is Nothing Then Return ActualTws
    If TgtManager.Tgt Is Nothing Then Return ActualTws
    Dim Delta As Double = Math.Abs(DrivingForce - NewLift.DrivingForce)
    If Delta = 0 Then Return ActualTws
    Dim steppi = 0.05
    Dim TwsTo = 40
    If DrivingForce < NewLift.DrivingForce Then
      steppi *= -1
      TwsTo = 0
    End If

    Dim TwsTmp As Double = ActualTws
    Dim LaDtmp As clsLiftAndDrag = NewLift.Clone
    For i As Double = ActualTws To TwsTo Step steppi
      Dim tgt = TgtManager.Tgt.Valore(i, Twa, "bs")
      LaDtmp.Settings.AirFlowAngleOfAttack = tgt.Awa
      LaDtmp.Settings.AirFlowSpeed = tgt.Aws / 3600 * 1852
      LaDtmp.CalcolaTutto()
      Dim DeltaTmp As Double = Math.Abs(DrivingForce - LaDtmp.DrivingForce)
      If DeltaTmp < Delta Then
        Delta = DeltaTmp
        TwsTmp = i
      End If
    Next
    Return TwsTmp


    '' cerca nella polare
    ''Dim Delta As Double = 100
    ''Dim LaDtmp As clsLiftAndDrag = Me.Clone
    ''Dim TwsTmp As Double = 4
    'For i As Double = 4 To 40 Step 0.1
    '  Dim tgt = TgtManager.Tgt.Valore(i, Twa, "bs")
    '  LaDtmp.Settings.AirFlowAngleOfAttack = tgt.Awa
    '  LaDtmp.Settings.AirFlowSpeed = tgt.Aws
    '  LaDtmp.CalcolaTutto()
    '  Dim LaDlift = LaDtmp.DrivingForce
    '  If Math.Abs(LaDlift - NewLift.DrivingForce) < Delta Then
    '    Delta = LaDlift - NewLift.DrivingForce
    '    TwsTmp = i
    '  End If
    'Next
    'Return TwsTmp
  End Function

End Class
