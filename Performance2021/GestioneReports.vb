Imports System.Collections.ObjectModel
Imports SPwpf

'Public Class clsReportStraightLine
'  Dim SelettoreCanali As New clsSelezionaCanali



'  Public Sub StampaTabella(SelezionaCanali As Boolean)
'    Dim CanaliDaStampare As List(Of clsChannel2020) = Nothing
'    If SelezionaCanali Then
'      CanaliDaStampare = SelettoreCanali.GestisciLista(clsSelezionaCanali.eTipo.eStraightLineAvgTableChannels, "Straight Line Report Channel Selector")
'    Else
'      CanaliDaStampare = SelettoreCanali.CaricaListaCorrente(clsSelezionaCanali.eTipo.eStraightLineAvgTableChannels)
'    End If
'    If CanaliDaStampare Is Nothing Then Exit Sub
'    If CanaliDaStampare.Count = 0 Then Exit Sub

'    Dim Periodi As New List(Of clsPeriod2021)
'    Dim PO = PeriodsManager.ListaStraightLineVmg.OrderBy(Function(x) x.TR.Start)
'    For Each SL In PO
'      If SL.IsChecked Then
'        Periodi.Add(SL)
'      End If
'    Next
'    'Dim PO = PeriodsManager.PeriodiSelezionati.OrderBy(Function(x) x.TimeRange.Inizio)
'    'For Each periodo In PO
'    '  Periodi.Add(periodo)
'    'Next
'    ' PeriodsManager.PeriodiSelezionati.ToList
'    Dim objTabelle As New clsTabellaPeriodiCanali("StraightLine", Periodi, CanaliDaStampare)
'    Dim Righe As New List(Of String)
'    Righe.Add("")
'    Righe.AddRange(objTabelle.TabellaPeriodi)
'    Righe.Add("")
'    Righe.Add("")
'    Righe.Add("")
'    Righe.AddRange(objTabelle.TabellaCanali)
'    Righe.Add("")
'    Righe.Add("")
'    Clipboard.SetText(String.Join(vbCrLf, Righe))
'    'MsgBox("StraightLine Table copied to the clipboard")

'    'Righe.Clear()
'    'Righe.AddRange(objTabelle.TabellaPeriodiCompleta)
'    'Dim strtmp As String = String.Join(vbCrLf, Righe)
'    'strtmp = strtmp.Replace(",", " ")
'    'Clipboard.SetText(strtmp.Replace(vbTab, ","))
'    'MsgBox("StraightLine Table CSV copied to the clipboard")

'  End Sub


'End Class

'Public Class clsSelezionaCanali
'  'Dim pTipo As eTipo
'  'Dim pSuffisso As String

'  'Public Property Suffisso As String
'  '  Get
'  '    Return pSuffisso
'  '  End Get
'  '  Set(value As String)
'  '    pSuffisso = value
'  '  End Set
'  'End Property

'  'Public Enum eTipo
'  '  eStraightLineDataGridTable = 0
'  '  eStraightLineAvgTableChannels = 1
'  '  eStraightLineChartChannels = 2
'  '  eStraightLineHtmlChannels = 3
'  '  ePavarotHtmlChannels = 4
'  '  eAccelerationsHtmlChannels = 5
'  '  eQuerySqlStraightLine = 6
'  '  eQuerySqlAccelerations = 7
'  '  eQuerySqlTacksAndGybes = 8
'  'End Enum

'  'Private Function TipoInStringa() As String
'  '  Return pTipo.ToString.TrimStart("e")
'  'End Function

'  'Private Function IntestazioniDefault() As List(Of String)
'  '  Dim lTmp As New List(Of String)
'  '  Select Case pTipo
'  '    Case eTipo.ePavarotHtmlChannels
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '    Case eTipo.eStraightLineHtmlChannels
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrAngle)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWD)
'  '    Case eTipo.eStraightLineAvgTableChannels
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrAngle)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWD)
'  '    Case eTipo.eStraightLineDataGridTable
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrAngle)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWD)
'  '    Case eTipo.eStraightLineChartChannels
'  '      '  CanaleOrdinata = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eVMG)
'  '      'Case 1
'  '      '  CanaleOrdinata = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eSOW)
'  '      'Case 2
'  '      '  CanaleOrdinata = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eTWA)
'  '      'Case Else
'  '      '  CanaleOrdinata = DataProvider2020.CanaleDbl(clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrAngle)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWD)
'  '    Case eTipo.eAccelerationsHtmlChannels
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '    Case eTipo.eQuerySqlStraightLine
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '    Case eTipo.eQuerySqlAccelerations
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '    Case eTipo.eQuerySqlTacksAndGybes
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWA)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eAWS)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eSOW)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eHEEL)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eTRIM)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eVMG)
'  '      AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eLWY)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRideHeight)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRake)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, clsChannels2020.eCanaliChiave.eRdrRakeEffective)
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortCantAngleEffective")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilOutFlap1Angle")
'  '      'AggiungiIntestazioneSeEsiste(lTmp, "PortFoilInFlap1Angle")
'  '  End Select
'  '  SalvaIntestazioniJson(lTmp)
'  '  Return lTmp
'  'End Function

'  'Private Sub AggiungiIntestazioneSeEsiste(ByRef lista As List(Of String), CanaleChiave As clsChannels2020.eCanaliChiave)
'  '  If DataProvider2020 Is Nothing OrElse Not DataProvider2020.ValoriCaricati Then
'  '    lista.Add("_" & System.Enum.GetName(GetType(clsChannels2020.eCanaliChiave), CanaleChiave).TrimStart("e"))
'  '  Else
'  '    If Not DataProvider2020.CanaleDbl(CanaleChiave) Is Nothing Then
'  '      lista.Add(DataProvider2020.CanaleDbl(CanaleChiave).ChannelId)
'  '    End If
'  '  End If
'  'End Sub

'  'Private Sub AggiungiIntestazioneSeEsiste(ByRef lista As List(Of String), ChannelId As String)
'  '  Dim Canale As clsChannel2020 = DataProvider2020.CanaleDbl(ChannelId)
'  '  If Not Canale Is Nothing Then
'  '    lista.Add(ChannelId)
'  '  End If
'  'End Sub




'  'Private Function CaricaListaIntestazioniDaJson() As List(Of String)
'  '  Dim ListaTmp As List(Of String) = Nothing
'  '  Select Case pTipo
'  '    Case eTipo.eAccelerationsHtmlChannels
'  '      Return AppConfig.ActiveProfile.AccelerationsHtmlChannels
'  '    Case eTipo.ePavarotHtmlChannels
'  '      Return AppConfig.ActiveProfile.PavarotHtmlChannels
'  '    Case eTipo.eQuerySqlAccelerations
'  '      Return AppConfig.ActiveProfile.QuerySqlAccelerations
'  '    Case eTipo.eQuerySqlStraightLine
'  '      Return AppConfig.ActiveProfile.QuerySqlStraightLine
'  '    Case eTipo.eQuerySqlTacksAndGybes
'  '      Return AppConfig.ActiveProfile.QuerySqlTacksAndGybes
'  '    Case eTipo.eStraightLineAvgTableChannels
'  '      Return AppConfig.ActiveProfile.StraightLineAvgTableChannels
'  '    Case eTipo.eStraightLineChartChannels
'  '      Return AppConfig.ActiveProfile.StraightLineChartChannels
'  '    Case eTipo.eStraightLineDataGridTable
'  '      Return AppConfig.ActiveProfile.StraightLineDataGridTable
'  '    Case eTipo.eStraightLineHtmlChannels
'  '      Return AppConfig.ActiveProfile.StraightLineHtmlChannels
'  '  End Select
'  '  If ListaTmp Is Nothing Then
'  '    ListaTmp = IntestazioniDefault()
'  '  End If
'  '  Return ListaTmp
'  'End Function

'  'Private Sub SalvaIntestazioniJson(Canali As List(Of clsChannel2020))
'  '  Dim ListaTmp As New List(Of String)
'  '  For Each canale In Canali
'  '    ListaTmp.Add(canale.ChannelId)
'  '  Next
'  '  SalvaIntestazioniJson(ListaTmp)
'  'End Sub

'  'Private Sub SalvaIntestazioniJson(ListaCanali As List(Of String))
'  '  Select Case pTipo
'  '    Case eTipo.eAccelerationsHtmlChannels
'  '      AppConfig.ActiveProfile.AccelerationsHtmlChannels = ListaCanali
'  '    Case eTipo.ePavarotHtmlChannels
'  '      AppConfig.ActiveProfile.PavarotHtmlChannels = ListaCanali
'  '    Case eTipo.eQuerySqlAccelerations
'  '      AppConfig.ActiveProfile.QuerySqlAccelerations = ListaCanali
'  '    Case eTipo.eQuerySqlStraightLine
'  '      AppConfig.ActiveProfile.QuerySqlStraightLine = ListaCanali
'  '    Case eTipo.eQuerySqlTacksAndGybes
'  '      AppConfig.ActiveProfile.QuerySqlTacksAndGybes = ListaCanali
'  '    Case eTipo.eStraightLineAvgTableChannels
'  '      AppConfig.ActiveProfile.StraightLineAvgTableChannels = ListaCanali
'  '    Case eTipo.eStraightLineChartChannels
'  '      AppConfig.ActiveProfile.StraightLineChartChannels = ListaCanali
'  '    Case eTipo.eStraightLineDataGridTable
'  '      AppConfig.ActiveProfile.StraightLineDataGridTable = ListaCanali
'  '    Case eTipo.eStraightLineHtmlChannels
'  '      AppConfig.ActiveProfile.StraightLineHtmlChannels = ListaCanali
'  '  End Select
'  'End Sub

'  'Private Function CaricaListaIntestazioniDaXml() As List(Of String)
'  '  Select Case pTipo
'  '    Case eTipo.eQuerySqlAccelerations, eTipo.eQuerySqlStraightLine, eTipo.eQuerySqlTacksAndGybes
'  '      Suffisso = "SqlDbSettings"
'  '    Case Else
'  '      Suffisso = DataProvider2020.SuffissoFileType
'  '  End Select

'  '  Dim NodoIntestazioni As Xml.XmlNode = AppConfig.CercaNodo(Suffisso, clsSettings.eNodoSTD.eReports, TipoInStringa, True)
'  '  If NodoIntestazioni Is Nothing Then
'  '    Return IntestazioniDefault()
'  '  Else
'  '    NodoIntestazioni = AppConfig.CercaNodo(Suffisso, clsSettings.eNodoSTD.eReports, True)
'  '    Dim lTmp As New List(Of String)
'  '    For Each Nodo As Xml.XmlNode In NodoIntestazioni
'  '      Select Case Nodo.Name
'  '        Case TipoInStringa()
'  '          For Each SottoNodo As Xml.XmlNode In Nodo.ChildNodes
'  '            lTmp.Add(SottoNodo.Name)
'  '          Next
'  '          Return lTmp
'  '        Case Else
'  '      End Select
'  '    Next
'  '  End If
'  '  Return Nothing
'  'End Function

'  'Private Sub SalvaListaIntestazioniDaXml(Canali As List(Of clsChannel2020))
'  '  AppConfig.EliminaNodo(Suffisso, clsSettings.eNodoSTD.eReports, TipoInStringa, True)
'  '  For Each Canale In Canali
'  '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eReports, TipoInStringa, Canale.ChannelId, "", True, False)
'  '  Next
'  '  AppConfig.SalvaFileXML()
'  'End Sub

'  'Private Sub SalvaListaIntestazioniDaXml(Canali As List(Of String))
'  '  AppConfig.EliminaNodo(Suffisso, clsSettings.eNodoSTD.eReports, TipoInStringa, True)
'  '  For Each Canale In Canali
'  '    AppConfig.SalvaValoreInnerText(Suffisso, clsSettings.eNodoSTD.eReports, TipoInStringa, Canale, "", True, False)
'  '  Next
'  '  AppConfig.SalvaFileXML()
'  'End Sub


'  'Public Function GestisciLista(Tipo As eTipo, Titolo As String) As List(Of clsChannel2020)
'  '  pTipo = Tipo
'  '  Dim strIntestazioni As List(Of String) = CaricaListaIntestazioniDaJson()
'  '  Dim frmSelChannel As New ChannelSelectorAndOrderer(DataProvider2020.Channels.ListaCanali, strIntestazioni, Titolo)
'  '  frmSelChannel.ShowDialog()
'  '  Dim CanaliDaStampare As List(Of clsChannel2020) = Nothing
'  '  If frmSelChannel.Status = ChannelSelectorAndOrderer.eStatus.eSave Then
'  '    CanaliDaStampare = frmSelChannel.AvailableChannels.GetListaCanaliSelezionati.ToList
'  '    If CanaliDaStampare Is Nothing Then Return Nothing
'  '    If CanaliDaStampare.Count = 0 Then Return Nothing
'  '    SalvaIntestazioniJson(CanaliDaStampare)
'  '    Return CanaliDaStampare
'  '  Else
'  '    Return Nothing
'  '  End If
'  'End Function

'  'Public Function GestisciListaStringa(ListaCanaliDisponibili As List(Of String), Tipo As eTipo) As List(Of String)
'  '  pTipo = Tipo
'  '  Dim strIntestazioni As List(Of String) = CaricaListaIntestazioniDaJson()
'  '  Dim frmSelChannel As New UserControlSqlChannelSelector(ListaCanaliDisponibili, strIntestazioni)
'  '  frmSelChannel.ShowDialog()
'  '  Dim CanaliDaStampare As List(Of String) = Nothing
'  '  If frmSelChannel.Status = ChannelSelectorAndOrderer.eStatus.eSave Then
'  '    CanaliDaStampare = frmSelChannel.AvailableChannels.ListaCanaliSelezionati
'  '    If CanaliDaStampare Is Nothing Then Return Nothing
'  '    If CanaliDaStampare.Count = 0 Then Return Nothing
'  '    SalvaIntestazioniJson(CanaliDaStampare)
'  '    Return CanaliDaStampare
'  '  Else
'  '    Return Nothing
'  '  End If
'  'End Function

'  Public Function GestisciListaStringa(ListaCanaliDisponibili As List(Of String), ListaCanaliSelezionati As List(Of String)) As List(Of String)
'    Dim frmSelChannel As New UserControlSqlChannelSelector(ListaCanaliDisponibili, ListaCanaliSelezionati)
'    frmSelChannel.ShowDialog()
'    Dim CanaliDaStampare As List(Of String) = Nothing
'    If frmSelChannel.Status = ChannelSelectorAndOrderer.eStatus.eSave Then
'      CanaliDaStampare = frmSelChannel.AvailableChannels.ListaCanaliSelezionati
'      If CanaliDaStampare Is Nothing Then Return Nothing
'      If CanaliDaStampare.Count = 0 Then Return Nothing
'      Return CanaliDaStampare
'    Else
'      Return Nothing
'    End If
'  End Function

'  'Public Function CaricaListaCorrente(Tipo As eTipo) As List(Of clsChannel2020)
'  '  pTipo = Tipo
'  '  Dim ListaTmp As New List(Of clsChannel2020)
'  '  For Each Canale In AppConfig.ActiveProfile.StraightLineChartSettings
'  '    Dim CanaleTmp As clsChannel2020 = DataProvider2020.CanaleDbl(Canale.ChannelName)
'  '    If Not CanaleTmp Is Nothing Then ListaTmp.Add(CanaleTmp)
'  '  Next
'  '  Return ListaTmp
'  'End Function

'  'Public Function CaricaListaCorrenteStringa(Tipo As eTipo) As List(Of String)
'  '  pTipo = Tipo
'  '  Return CaricaListaIntestazioniDaJson()
'  'End Function

'End Class

'Public Class clsReportPavarots
'  Dim pSecondiCampionamentoAntePost As Integer = 5

'  'Public Function TabelloneInStringa() As String

'  '  Dim pv As Integer = 0
'  '  Dim StringaClipboard As String = ""
'  '  For Each Pavarot In PeriodsManager.Pavarots.ListaPavarots2019
'  '    If Pavarot.Periodo.IsChecked Then
'  '      Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      'Dim eventi = Dettagli.ListaEventi
'  '      'For Each Momento In Dettagli.ListaEventi
'  '      '	Console.WriteLine(Momento.Descrizione & ": " & Momento.Momento.ToLongTimeString)
'  '      'Next
'  '      Dim strDescrizione As String = "Description"
'  '      Dim strPavarot As String = Pavarot.DescrizionePavarotDetails
'  '      'strDescrizione &= vbTab & "Duration"
'  '      'strPavarot &= vbTab & Pavarot.Periodo.TimeRange.DurataInStringaConSeparatore
'  '      strDescrizione &= vbTab & "Type"
'  '      strPavarot &= vbTab & If(Pavarot.IsTack, "Tack", "Gybe")
'  '      strDescrizione &= vbTab & "Tack"
'  '      strPavarot &= vbTab & If(Pavarot.StbdToPort, "Stbd", "Port")
'  '      strDescrizione &= vbTab & "Tws"
'  '      strPavarot &= vbTab & Format(Pavarot.Periodo.TWS, "F1")
'  '      strDescrizione &= vbTab & "InlineLoss"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriInLineLossGainAt(clsPavarot2019.eAxisRef.eTWD, Pavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '      strDescrizione &= vbTab & "VmgLoss"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, Pavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '      strDescrizione &= vbTab & "Loss@15"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, CDbl(15)), "F0")
'  '      strDescrizione &= vbTab & "Loss@30"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, CDbl(30)), "F0")
'  '      For Each Momento In Dettagli.ListaEventi
'  '        strDescrizione &= vbTab & Momento.ShortName
'  '        strPavarot &= vbTab & Format(Momento.Momento.Subtract(Pavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '      Next
'  '      strDescrizione &= vbTab & ""
'  '      strPavarot &= vbTab & ""
'  '      strDescrizione &= vbTab & "Cant@BrdDnFn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewCantRealAngle, "F1")
'  '      strDescrizione &= vbTab & "InFlap@BrdDn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInSetupAngle, "F1")
'  '      strDescrizione &= vbTab & "InFlap@BrdDnFn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInRealAngle, "F1")
'  '      strDescrizione &= vbTab & "OutFlap@BrdDn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutSetupAngle, "F1")
'  '      strDescrizione &= vbTab & "OutFlap@BrdDnFn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutRealAngle, "F1")
'  '      strDescrizione &= vbTab & "Trim@BrdDn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialTrim, "F1")
'  '      strDescrizione &= vbTab & "Trim@BrdDnFn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.FinalTrim, "F1")
'  '      strDescrizione &= vbTab & "Heel@BrdDn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialHeel, "F1")
'  '      strDescrizione &= vbTab & "Heel@BrdDnFn"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.FinalHeel, "F1")

'  '      strDescrizione &= vbTab & ""
'  '      strPavarot &= vbTab & ""
'  '      strDescrizione &= vbTab & "CogDelta"
'  '      strPavarot &= vbTab & Format(DifferenzaAssolutaTraAngoli360(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, True), "F0")
'  '      strDescrizione &= vbTab & "YawRateAvg"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Avg, "F0")
'  '      strDescrizione &= vbTab & "YawRateMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).MaxVal, "F0")
'  '      strDescrizione &= vbTab & "RdrAngleAvg"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "RdrAngleMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).MaxVal, "F0")
'  '      strDescrizione &= vbTab & "LeewayMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Leeway).MaxVal, "F0")


'  '      strDescrizione &= vbTab & ""
'  '      strPavarot &= vbTab & ""

'  '      strDescrizione &= vbTab & "BsAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")
'  '      strDescrizione &= vbTab & "BsPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")

'  '      strDescrizione &= vbTab & "TwaAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg, "F0")
'  '      strDescrizione &= vbTab & "TwaPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg, "F0")

'  '      strDescrizione &= vbTab & "TrimAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg, "F1")
'  '      strDescrizione &= vbTab & "TrimPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg, "F1")

'  '      strDescrizione &= vbTab & "HeelAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg, "F1")
'  '      strDescrizione &= vbTab & "HeelPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg, "F1")

'  '      strDescrizione &= vbTab & "MinSinkAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RideHeight).Avg, "F1")
'  '      strDescrizione &= vbTab & "MinSinkPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RideHeight).Avg, "F1")


'  '      strDescrizione &= vbTab & "CantAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "CantPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "EffCantAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngleEffective).Avg, "F0")
'  '      strDescrizione &= vbTab & "EffCantPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngleEffective).Avg, "F0")

'  '      strDescrizione &= vbTab & "RdrRakeAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg, "F1")
'  '      strDescrizione &= vbTab & "RdrRakePost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg, "F1")

'  '      strDescrizione &= vbTab & "RdrRakeEffAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRkEff).Avg, "F1")
'  '      strDescrizione &= vbTab & "RdrRakeEffPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRkEff).Avg, "F1")

'  '      strDescrizione &= vbTab & "InFlapAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilInFlap1Angle).Avg, "F1")
'  '      strDescrizione &= vbTab & "InFlapPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilInFlap1Angle).Avg, "F1")

'  '      strDescrizione &= vbTab & "OutFlapAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilOutFlap1Angle).Avg, "F1")
'  '      strDescrizione &= vbTab & "OutFlapPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilOutFlap1Angle).Avg, "F1")

'  '      If pv = 0 Then
'  '        'Console.WriteLine(strDescrizione)
'  '        StringaClipboard &= strDescrizione & vbCrLf
'  '      End If
'  '      'Console.WriteLine(strPavarot)
'  '      StringaClipboard &= strPavarot & vbCrLf
'  '      pv += 1
'  '    End If
'  '    'Stop
'  '  Next

'  '  Return StringaClipboard

'  'End Function

'  'Public Function TabelloneInRighe() As String

'  '  Dim pv As Integer = 0
'  '  Dim StringaClipboard As String = "Overview" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.Pavarots.ListaPavarots2019
'  '    If Pavarot.Periodo.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      Dim strDescrizione As String = "Description"
'  '      Dim strPavarot As String = Pavarot.DescrizionePavarotDetails
'  '      strDescrizione &= vbTab & "Type"
'  '      strPavarot &= vbTab & If(Pavarot.IsTack, "Tack", "Gybe")
'  '      strDescrizione &= vbTab & "Tack"
'  '      strPavarot &= vbTab & If(Pavarot.StbdToPort, "Stbd", "Port")
'  '      strDescrizione &= vbTab & "Tws"
'  '      strPavarot &= vbTab & Format(Pavarot.Periodo.TWS, "F1")

'  '      strDescrizione &= vbTab & "InlineLoss"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriInLineLossGainAt(clsPavarot2019.eAxisRef.eTWD, Pavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '      strDescrizione &= vbTab & "VmgLoss"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, Pavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '      strDescrizione &= vbTab & "Loss@15"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, CDbl(15)), "F0")
'  '      strDescrizione &= vbTab & "Loss@30"
'  '      strPavarot &= vbTab & Format(Pavarot.MetriLossGainAt(clsPavarot2019.eAxisRef.eTWD, CDbl(30)), "F0")
'  '      For i As Integer = 0 To Dettagli.ListaEventi.Count - 1 Step 2
'  '        strDescrizione &= vbTab & Dettagli.ListaEventi(i).ShortName
'  '        strPavarot &= vbTab & Format(Dettagli.ListaEventi(i).Momento.Subtract(Pavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '        strDescrizione &= vbTab & "Duration"
'  '        strPavarot &= vbTab & Format(Dettagli.ListaEventi(i + 1).Momento.Subtract(Dettagli.ListaEventi(i).Momento).TotalSeconds, "F1")
'  '      Next

'  '      'For Each Momento In Dettagli.ListaEventi
'  '      '  strDescrizione &= vbTab & Momento.ShortName
'  '      '  strPavarot &= vbTab & Format(Momento.Momento.Subtract(Pavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '      'Next
'  '      If pv = 0 Then
'  '        StringaClipboard &= strDescrizione & vbCrLf
'  '      End If
'  '      StringaClipboard &= strPavarot & vbCrLf
'  '      pv += 1
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Board Drop" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.Pavarots.ListaPavarots2019
'  '    If Pavarot.Periodo.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      Dim strDescrizione As String = "Description"
'  '      Dim strPavarot As String = Pavarot.DescrizionePavarotDetails
'  '      strDescrizione &= vbTab & "Cant@BoardDownCompleted"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewCantRealAngle, "F1")
'  '      strDescrizione &= vbTab & "InnerFlapPreset"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInSetupAngle, "F1")
'  '      strDescrizione &= vbTab & "InnerFlapChange"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInSetupAngle - Dettagli.PavarotDetailBoardDrop.NewFlapInRealAngle, "F1")
'  '      strDescrizione &= vbTab & "OuterFlapPreset"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutSetupAngle, "F1")
'  '      strDescrizione &= vbTab & "OuterFlapChange"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutSetupAngle - Dettagli.PavarotDetailBoardDrop.NewFlapOutRealAngle, "F1")
'  '      strDescrizione &= vbTab & "Trim@BoardDown"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialTrim, "F1")
'  '      strDescrizione &= vbTab & "TrimChange"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialTrim - Dettagli.PavarotDetailBoardDrop.FinalTrim, "F1")
'  '      strDescrizione &= vbTab & "Heel@BoardDown"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialHeel, "F1")
'  '      strDescrizione &= vbTab & "HeelChange"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialHeel - Dettagli.PavarotDetailBoardDrop.FinalHeel, "F1")
'  '      If pv = 0 Then
'  '        StringaClipboard &= strDescrizione & vbCrLf
'  '      End If
'  '      StringaClipboard &= strPavarot & vbCrLf
'  '      pv += 1
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Boat Rotation" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.Pavarots.ListaPavarots2019
'  '    If Pavarot.Periodo.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      Dim strDescrizione As String = "Description"
'  '      Dim strPavarot As String = Pavarot.DescrizionePavarotDetails
'  '      strDescrizione &= vbTab & "CogDelta"
'  '      strPavarot &= vbTab & Format(DifferenzaAssolutaTraAngoli360(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, True), "F0")
'  '      strDescrizione &= vbTab & "YawRateAvg"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Avg, "F0")
'  '      strDescrizione &= vbTab & "YawRateMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).MaxVal, "F0")
'  '      strDescrizione &= vbTab & "RdrAngleAvg"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "RdrAngleMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).MaxVal, "F0")
'  '      strDescrizione &= vbTab & "LeewayMax"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Leeway).MaxVal, "F0")
'  '      strDescrizione &= vbTab & "MinSink"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RideHeight).MaxVal, "F2")
'  '      If pv = 0 Then
'  '        StringaClipboard &= strDescrizione & vbCrLf
'  '      End If
'  '      StringaClipboard &= strPavarot & vbCrLf
'  '      pv += 1
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Boat performances before Board Drop and After Board Up" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.Pavarots.ListaPavarots2019
'  '    If Pavarot.Periodo.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      Dim strDescrizione As String = "Description"
'  '      Dim strPavarot As String = Pavarot.DescrizionePavarotDetails
'  '      strDescrizione &= vbTab & "BsAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")
'  '      strDescrizione &= vbTab & "BsPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")

'  '      strDescrizione &= vbTab & "TwaAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg, "F0")
'  '      strDescrizione &= vbTab & "TwaPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg, "F0")

'  '      strDescrizione &= vbTab & "TrimAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg, "F1")
'  '      strDescrizione &= vbTab & "TrimPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Trim).Avg, "F1")

'  '      strDescrizione &= vbTab & "HeelAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg, "F1")
'  '      strDescrizione &= vbTab & "HeelPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Heel).Avg, "F1")

'  '      strDescrizione &= vbTab & "MinSinkAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RideHeight).Avg, "F1")
'  '      strDescrizione &= vbTab & "MinSinkPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RideHeight).Avg, "F1")

'  '      strDescrizione &= vbTab & "CantAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "CantPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngle).Avg, "F0")
'  '      strDescrizione &= vbTab & "EffCantAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngleEffective).Avg, "F0")
'  '      strDescrizione &= vbTab & "EffCantPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdCantAngleEffective).Avg, "F0")

'  '      strDescrizione &= vbTab & "RdrRakeAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg, "F1")
'  '      strDescrizione &= vbTab & "RdrRakePost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRk).Avg, "F1")

'  '      strDescrizione &= vbTab & "RdrRakeEffAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRkEff).Avg, "F1")
'  '      strDescrizione &= vbTab & "RdrRakeEffPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.RdrRkEff).Avg, "F1")

'  '      strDescrizione &= vbTab & "InFlapAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilInFlap1Angle).Avg, "F1")
'  '      strDescrizione &= vbTab & "InFlapPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilInFlap1Angle).Avg, "F1")

'  '      strDescrizione &= vbTab & "OutFlapAnte"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilOutFlap1Angle).Avg, "F1")
'  '      strDescrizione &= vbTab & "OutFlapPost"
'  '      strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.LwdFoilOutFlap1Angle).Avg, "F1")

'  '      If pv = 0 Then
'  '        StringaClipboard &= strDescrizione & vbCrLf
'  '      End If
'  '      StringaClipboard &= strPavarot & vbCrLf
'  '      pv += 1
'  '    End If
'  '  Next

'  '  Return StringaClipboard


'  'End Function

'  'Public Function TabelloneInRighe(CanaliEntryExit As List(Of clsChannel2020)) As String

'  '  Dim pv As Integer = 0
'  '  Dim StringaClipboard As String = "Overview" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.CollectionPavarot
'  '    If Pavarot.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot.DettagliPavarot, Pavarot.DettagliPavarot.MomentoChiave.Subtract(Pavarot.DettagliPavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.DettagliPavarot.TimeRangeExit.Inizio.Subtract(Pavarot.DettagliPavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      'Dim Dettagli As New clsPavarotPhases(Pavarot.DettagliPavarot, PeriodsManager.PavarotSettings2020)
'  '      If False Then ' Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '        'Dim strDescrizione As String = "Description"
'  '        'Dim strPavarot As String = Pavarot.DettagliPavarot.DescrizionePavarotDetails
'  '        'strDescrizione &= vbTab & "Type"
'  '        'strPavarot &= vbTab & If(Pavarot.DettagliPavarot.IsTack, "Tack", "Gybe")
'  '        'strDescrizione &= vbTab & "Tack"
'  '        'strPavarot &= vbTab & If(Pavarot.DettagliPavarot.StbdToPort, "Stbd", "Port")
'  '        'strDescrizione &= vbTab & "Tws"
'  '        'strPavarot &= vbTab & Format(Pavarot.DettagliPavarot.Periodo.ValoriCanaleTWS.Avg, "F1")

'  '        'strDescrizione &= vbTab & "AvgBs"
'  '        'strPavarot &= vbTab & Format(Pavarot.DettagliPavarot.PavarotAvgBsEquivalent(Pavarot.DettagliPavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '        'strDescrizione &= vbTab & "AvgVmg"
'  '        'strPavarot &= vbTab & Format(Pavarot.DettagliPavarot.PavarotAvgVmgEquivalent(Pavarot.DettagliPavarot.TimeRangeExit.IdRigaIniziale), "F0")
'  '        'strDescrizione &= vbTab & "Loss@15"
'  '        'strPavarot &= vbTab & Format(Pavarot.DettagliPavarot.VmgMetersEntryToMoment(CDbl(15)), "F0")
'  '        'strDescrizione &= vbTab & "Loss@30"
'  '        'strPavarot &= vbTab & Format(Pavarot.DettagliPavarot.VmgMetersEntryToMoment(CDbl(30)), "F0")
'  '        ''For i As Integer = 0 To Dettagli.ListaEventi.Count - 1 Step 2
'  '        ''  strDescrizione &= vbTab & Dettagli.ListaEventi(i).ShortName
'  '        ''  strPavarot &= vbTab & Format(Dettagli.ListaEventi(i).Momento.Subtract(Pavarot.DettagliPavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '        ''  strDescrizione &= vbTab & "Duration"
'  '        ''  strPavarot &= vbTab & Format(Dettagli.ListaEventi(i + 1).Momento.Subtract(Dettagli.ListaEventi(i).Momento).TotalSeconds, "F1")
'  '        ''Next

'  '        ''For Each Momento In Dettagli.ListaEventi
'  '        ''  strDescrizione &= vbTab & Momento.ShortName
'  '        ''  strPavarot &= vbTab & Format(Momento.Momento.Subtract(Pavarot.DettagliPavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '        ''Next
'  '        'If pv = 0 Then
'  '        '  StringaClipboard &= strDescrizione & vbCrLf
'  '        'End If
'  '        'StringaClipboard &= strPavarot & vbCrLf
'  '        'pv += 1
'  '      End If
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Board Drop" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.CollectionPavarot
'  '    If Pavarot.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot.DettagliPavarot, Pavarot.DettagliPavarot.MomentoChiave.Subtract(Pavarot.DettagliPavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.DettagliPavarot.TimeRangeExit.Inizio.Subtract(Pavarot.DettagliPavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      'Dim Dettagli As New clsPavarotPhases(Pavarot.DettagliPavarot, PeriodsManager.PavarotSettings2020)
'  '      'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '      '  Dim strDescrizione As String = "Description"
'  '      '  Dim strPavarot As String = Pavarot.DettagliPavarot.DescrizionePavarotDetails
'  '      '  strDescrizione &= vbTab & "Cant@BoardDownCompleted"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewCantRealAngle, "F1")
'  '      '  strDescrizione &= vbTab & "InnerFlapPreset"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInSetupAngle, "F1")
'  '      '  strDescrizione &= vbTab & "InnerFlap@DropCompleted"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInRealAngle, "F1")
'  '      '  strDescrizione &= vbTab & "OuterFlapPreset"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutSetupAngle, "F1")
'  '      '  strDescrizione &= vbTab & "OuterFlap@DropCompleted"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutRealAngle, "F1")
'  '      '  strDescrizione &= vbTab & "Trim@Drop"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialTrim, "F1")
'  '      '  strDescrizione &= vbTab & "Trim@DropCompleted"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.FinalTrim, "F1")
'  '      '  strDescrizione &= vbTab & "Heel@Drop"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.InitialHeel, "F1")
'  '      '  strDescrizione &= vbTab & "Heel@DropCompleted"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailBoardDrop.FinalHeel, "F1")
'  '      '  If pv = 0 Then
'  '      '    StringaClipboard &= strDescrizione & vbCrLf
'  '      '  End If
'  '      '  StringaClipboard &= strPavarot & vbCrLf
'  '      '  pv += 1
'  '      'End If
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Boat Rotation" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.CollectionPavarot
'  '    If Pavarot.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot, 10, 10)
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot.DettagliPavarot, Pavarot.DettagliPavarot.MomentoChiave.Subtract(Pavarot.DettagliPavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.DettagliPavarot.TimeRangeExit.Inizio.Subtract(Pavarot.DettagliPavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      'Dim Dettagli As New clsPavarotPhases(Pavarot.DettagliPavarot, PeriodsManager.PavarotSettings2020)
'  '      'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '      '  Dim strDescrizione As String = "Description"
'  '      '  Dim strPavarot As String = Pavarot.DettagliPavarot.DescrizionePavarotDetails
'  '      '  strDescrizione &= vbTab & "CogDelta"
'  '      '  strPavarot &= vbTab & Format(DifferenzaAssolutaTraAngoli360(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, True), "F0")
'  '      '  strDescrizione &= vbTab & "YawRateAvg"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Avg, "F0")
'  '      '  strDescrizione &= vbTab & "YawRateMax"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Max, "F0")
'  '      '  strDescrizione &= vbTab & "RdrAngleAvg"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Avg, "F0")
'  '      '  strDescrizione &= vbTab & "RdrAngleMax"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Max, "F0")
'  '      '  strDescrizione &= vbTab & "LeewayMax"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Leeway).Max, "F0")
'  '      '  strDescrizione &= vbTab & "MinSink"
'  '      '  strPavarot &= vbTab & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RideHeight).Max, "F2")
'  '      '  If pv = 0 Then
'  '      '    StringaClipboard &= strDescrizione & vbCrLf
'  '      '  End If
'  '      '  StringaClipboard &= strPavarot & vbCrLf
'  '      '  pv += 1
'  '      'End If
'  '    End If
'  '  Next

'  '  pv = 0
'  '  StringaClipboard &= vbCrLf & vbCrLf
'  '  StringaClipboard &= "Boat performances @Entry and @Exit" & vbCrLf
'  '  For Each Pavarot In PeriodsManager.CollectionPavarot
'  '    If Pavarot.IsChecked Then
'  '      'Dim Dettagli As New clsPavarotDetails(Pavarot.DettagliPavarot, Pavarot.DettagliPavarot.MomentoChiave.Subtract(Pavarot.DettagliPavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.DettagliPavarot.TimeRangeExit.Inizio.Subtract(Pavarot.DettagliPavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '      'Dim Dettagli As New clsPavarotPhases(Pavarot.DettagliPavarot, PeriodsManager.PavarotSettings2020)
'  '      'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '      '  Dim strDescrizione As String = "Description"
'  '      '  Dim strPavarot As String = Pavarot.DettagliPavarot.DescrizionePavarotDetails
'  '      '  For Each canale In CanaliEntryExit
'  '      '    strDescrizione &= vbTab & canale.ShortName & "Entry"
'  '      '    strPavarot &= vbTab & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(canale).Avg, "F1")
'  '      '    strDescrizione &= vbTab & canale.ShortName & "Exit"
'  '      '    strPavarot &= vbTab & Format(Dettagli.PavarotDetailPost.ChannelSubSet(canale).Avg, "F1")
'  '      '  Next
'  '      '  If pv = 0 Then
'  '      '    StringaClipboard &= strDescrizione & vbCrLf
'  '      '  End If
'  '      '  StringaClipboard &= strPavarot & vbCrLf
'  '      '  pv += 1
'  '      'End If
'  '    End If
'  '  Next

'  '  Return StringaClipboard


'  'End Function

'  'Public Function TabelloneHtml(CanaliEntryExit As List(Of clsChannel2020), ListaPavarots As List(Of clsPeriod2021)) As String
'  '  Dim strDescrizione As String = ""
'  '  Dim strDescrizione2 As String = ""
'  '  Dim strPavarot As String = ""

'  '  Dim pv As Integer = 0
'  '  Dim StringaHtml As String = vbCrLf & "<table><tr><td>" 'Tabella Esterna

'  '  Dim Filtro As String = "Entry@ " & Format(ListaPavarots.First.PavarotEntry.Subtract(ListaPavarots.First.KeyMoment).TotalSeconds, "F0") & " ss, "
'  '  Filtro &= " Exit(End)@ +" & Format(ListaPavarots.First.PavarotExit.Subtract(ListaPavarots.First.KeyMoment).TotalSeconds, "F0") & " ss"

'  '  StringaHtml &= vbCrLf & "<table class=""tbtt"">" 'tabella A
'  '  StringaHtml &= "<tr class=""trtt""><td colspan=""19"" class=""TableHeader"">Overview <div style=""font-size:14px"">" & Filtro & "</div></td></tr>" 'Tabella Titolo Overview
'  '  For Each Pavarot In ListaPavarots
'  '    'Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '    'Dim Dettagli As New clsPavarotPhases(Pavarot, PeriodsManager.PavarotSettings2020)
'  '    'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then

'  '    '  Dim Classe As String = "tbtd"
'  '    '  If pv / 2 = CInt(pv / 2) Then Classe = "tbtp"
'  '    '  strDescrizione = "<td class=""Header"">Description</td>"
'  '    '  strPavarot = "<td class=""" & Classe & """>#" & pv + 1 & " " & Pavarot.MomentoChiave.ToLongTimeString & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">Type</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & If(Pavarot.IsTack, "Tack", "Gybe")
'  '    '  strDescrizione &= "<td Class=""Header"">Entry</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & If(Pavarot.StbdToPort, "Stbd", "Port")
'  '    '  strDescrizione &= "<td Class=""Header"">Tws</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.Periodo.ValoriCanaleTWS.Avg, "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">Vmg%</td>"
'  '    '  Dim ValoreTmp As Double = Pavarot.PavarotAvgVmgEquivalent(Pavarot.TimeRangeExit.Start.Subtract(Pavarot.MomentoChiave).TotalSeconds)
'  '    '  If Not TgtManager.Tgt Is Nothing Then
'  '    '    Dim Tws As Double = Pavarot.EntryExitInfos.Tws
'  '    '    'Dim bsTgt As Double = Targets.GetPolare("bs").ValoreTarget(Pavarot.IsTack, Tws)
'  '    '    'Dim twaTgt As Double = Targets.GetPolare("twa").ValoreTarget(Pavarot.IsTack, Tws)
'  '    '    'Dim vmgTgt As Double = System.Math.Abs(bsTgt * System.Math.Cos(Radians(twaTgt)))
'  '    '    Dim v = TgtManager.Tgt.ValoreTgt(Pavarot.IsTack, Tws, "bs")
'  '    '    ValoreTmp = If(v Is Nothing, 0, ValoreTmp / v.Vmg * 100) ' Bs * System.Math.Abs(System.Math.Cos(Radians(Twa)))
'  '    '  End If
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(ValoreTmp, "F1")
'  '    '  'strDescrizione &= "<td Class=""Header"">AvgBs</td>"
'  '    '  'strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.PavarotAvgBsEquivalent(clsPavarot2019.eAxisRef.eTWD, PeriodsManager.PavarotSettings2020.SecsPost), "F1")
'  '    '  'strDescrizione &= "<td Class=""Header"">AvgVmg</td>"
'  '    '  'strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.PavarotAvgVmgEquivalent(clsPavarot2019.eAxisRef.eTWD, PeriodsManager.PavarotSettings2020.SecsPost), "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">VmgMeters<br>@Exit</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.VmgMetersEntryToMoment(PeriodsManager.PavarotSettings2020.SecsPost), "F0")
'  '    '  strDescrizione &= "<td Class=""Header"">VmgMeters<br>@30</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.VmgMetersEntryToMoment(CDbl(30)), "F0")
'  '    '  strDescrizione &= "<td Class=""Header"">VmgMeters<br>@15</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.VmgMetersEntryToMoment(CDbl(15)), "F0")
'  '    '  For i As Integer = 0 To Dettagli.ListaEventi.Count - 1 Step 2
'  '    '    strDescrizione &= "<td Class=""Header"">" & Dettagli.ListaEventi(i).ShortName
'  '    '    strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.ListaEventi(i).Momento.Subtract(Pavarot.Periodo.KeyMoment).TotalSeconds, "F1")
'  '    '    strDescrizione &= "<td Class=""Header"">Duration</td>"
'  '    '    strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.ListaEventi(i + 1).Momento.Subtract(Dettagli.ListaEventi(i).Momento).TotalSeconds, "F1")
'  '    '  Next

'  '    '  strDescrizione &= "<td Class=""Header"">MinBs</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.MedieStandardPavarot.SpeedMinima, "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">Bs@<br>Entry</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">Twa@<br>Entry</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(System.Math.Abs(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg), "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">Bs@<br>Exit</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Bs).Avg, "F1")
'  '    '  strDescrizione &= "<td Class=""Header"">Twa@<br>Exit</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(System.Math.Abs(Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Twa).Avg), "F1")

'  '    '  If pv = 0 Then
'  '    '    'riga intestazioni alla prima pavarot
'  '    '    StringaHtml &= "<tr Class=""trtt"">" & strDescrizione & "</tr>"
'  '    '  End If
'  '    '  'riga valori per tutte le pavarot
'  '    '  StringaHtml &= "<tr Class=""trtt"">" & strPavarot & "</tr>"
'  '    '  pv += 1
'  '    'End If
'  '  Next
'  '  StringaHtml &= "</table>" 'chiudo la tabella A
'  '  StringaHtml &= "</td></tr><tr><td>"
'  '  pv = 0
'  '  StringaHtml &= vbCrLf & "<table Class=""tbtt"">" 'tabella B
'  '  StringaHtml &= "<tr><td colspan=""10"" Class=""TableHeader"">Board drop</td></tr>"
'  '  For Each Pavarot In ListaPavarots
'  '    'Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '    'Dim Dettagli As New clsPavarotPhases(Pavarot, PeriodsManager.PavarotSettings2020)
'  '    'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '    '  Dim Classe As String = "tbtd"
'  '    '  If pv / 2 = CInt(pv / 2) Then Classe = "tbtp"
'  '    '  strDescrizione = "<td Class=""Header"">Description</td>"
'  '    '  strPavarot = "<td class=""" & Classe & """>#" & pv + 1 & " " & Pavarot.MomentoChiave.ToLongTimeString & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Cant@Drop<br>Completed" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.NewCantRealAngle, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "InnerFlap<br>Preset" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInSetupAngle, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "InnerFlap<br>@Completed" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.NewFlapInRealAngle, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "OuterFlap<br>Preset" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutSetupAngle, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "OuterFlap<br>@Completed" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.NewFlapOutRealAngle, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Trim@Drop" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.InitialTrim, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Trim@Drop<br>Completed" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.FinalTrim, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Heel@Drop" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.InitialHeel, "F1") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Heel@Drop<br>Completed" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailBoardDrop.FinalHeel, "F1") & "</td>"
'  '    '  If pv = 0 Then
'  '    '    StringaHtml &= "<tr Class=""trtt"">" & strDescrizione & "</tr>"
'  '    '  End If
'  '    '  StringaHtml &= "<tr Class=""trtt"">" & strPavarot & "</tr>"
'  '    '  pv += 1
'  '    'End If
'  '  Next
'  '  StringaHtml &= "</table>" 'chiudo la tabella B
'  '  StringaHtml &= "</td></tr><tr><td>"
'  '  pv = 0
'  '  StringaHtml &= vbCrLf & "<table Class=""tbtt"">" 'tabella C
'  '  StringaHtml &= "<tr Class=""trtt""><td colspan=""11"" Class=""TableHeader"">Boat rotation</td></tr>" 'Tabella Titolo Boat Rotation
'  '  For Each Pavarot In ListaPavarots
'  '    'Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '    'Dim Dettagli As New clsPavarotPhases(Pavarot, PeriodsManager.PavarotSettings2020)
'  '    'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '    '  Dim Classe As String = "tbtd"
'  '    '  If pv / 2 = CInt(pv / 2) Then Classe = "tbtp"
'  '    '  strDescrizione = "<td Class=""Header"">Description</td>"
'  '    '  strPavarot = "<td Class=""" & Classe & """>#" & pv + 1 & " " & Pavarot.DescrizionePavarotDetails & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Tws" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Pavarot.EntryExitInfos.Tws, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "CogDelta" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(DifferenzaAssolutaTraAngoli360(Dettagli.PavarotDetailAnte.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, Dettagli.PavarotDetailPost.ChannelSubSet(clsPavarotDetailAntePost.eCanali.Cog).Avg, True), "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "YawRate Avg" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Avg, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "YawRate Max" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.YawRate).Max, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "RdrAngle Avg" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Avg, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "RdrAngle Max" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RdrAngle).Max, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "Leeway Max" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.Leeway).Max, "F0") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">" & "MinSink" & "</td>"
'  '    '  strPavarot &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.RideHeight).Max, "F2") & "</td>"
'  '    '  strDescrizione &= "<td Class=""Header"">MainTrav ds</td>"
'  '    '  If Not Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.MainTrav) Is Nothing Then
'  '    '    Dim Max As Double = Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.MainTrav).Max
'  '    '    Dim Min As Double = Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.MainTrav).Min
'  '    '    Dim Delta As Double = System.Math.Abs(Max - Min)
'  '    '    Dim Durata As Double = Dettagli.PavarotDetailTurning.TR.Durata.TotalSeconds
'  '    '    strPavarot &= "<td Class=""" & Classe & """>" & Format(Delta / Durata, "F1") & "</td>"
'  '    '    strDescrizione &= "<td Class=""Header"">MainTwist ds</td>"
'  '    '    Max = Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.MainTwist).Max
'  '    '    Min = Dettagli.PavarotDetailTurning.ChannelSubSet(clsPavarotDetailTurning.eCanali.MainTwist).Min
'  '    '    Delta = System.Math.Abs(Max - Min)
'  '    '    strPavarot &= "<td Class=""" & Classe & """>" & Format(Delta / Durata, "F1") & "</td>"
'  '    '  End If

'  '    '  If pv = 0 Then
'  '    '    StringaHtml &= "<tr Class=""trtt"">" & strDescrizione & "</tr>"
'  '    '  End If
'  '    '  StringaHtml &= "<tr Class=""trtt"">" & strPavarot & "</tr>"
'  '    '  pv += 1
'  '    'End If
'  '  Next
'  '  StringaHtml &= "</table>" 'chiudo la tabella C
'  '  StringaHtml &= "</td></tr><tr><td>"
'  '  pv = 0
'  '  StringaHtml &= vbCrLf & "<table Class=""tbtt"">" 'tabella D
'  '  Dim Cs As Integer = 21
'  '  If ListaPavarots.Count >= 8 Then Cs = 17
'  '  StringaHtml &= "<tr Class=""trtt""><td colspan=""" & Cs & """ Class=""TableHeader"">Boat performances @entry and @exit <div style=""font-size:14px"">" & Filtro & "</div></td></tr>" 'Tabella Titolo Performances Entry Exit

'  '  strDescrizione = ""
'  '  strDescrizione2 = ""


'  '  Dim strPavarots(CanaliEntryExit.Count - 1) As String
'  '  Dim IntestazioneCanale As Boolean = True
'  '  For pvrt As Integer = 0 To ListaPavarots.Count - 1
'  '    Dim Pavarot As clsPeriod2021 = ListaPavarots(pvrt)

'  '    If ListaPavarots.Count > 6 Then
'  '      strDescrizione &= "<td class=""Header"" colspan=""2"">#" & pv + 1 & " " & Pavarot.KeyMoment.ToLongTimeString & "</td>"
'  '    Else
'  '      strDescrizione &= "<td class=""Header"" colspan=""2"">#" & pv + 1 & " " & Pavarot.KeyMoment.ToLongTimeString & "</td>"
'  '      'strDescrizione &= "<td class=""Header"" colspan=""2"">#" & pv + 1 & " " & Pavarot.DescrizionePavarotDetails & "</td>"
'  '    End If
'  '    strDescrizione2 &= "<td Class=""Header"">Entry</td><td Class=""Header"">Exit</td>"
'  '    'Dim Dettagli As New clsPavarotDetails(Pavarot, Pavarot.MomentoChiave.Subtract(Pavarot.TimeRangeEntry.Fine).TotalSeconds, Pavarot.TimeRangeExit.Inizio.Subtract(Pavarot.MomentoChiave).TotalSeconds, pSecondiCampionamentoAntePost)  '10, 10)
'  '    'Dim Dettagli As New clsPavarotPhases(Pavarot, PeriodsManager.PavarotSettings2020)
'  '    'If Not Dettagli.KindOfPavarot = clsPavarotPhases.eKindOfPavarot.eNotValid Then
'  '    '  For i As Integer = 0 To CanaliEntryExit.Count - 1
'  '    '    Dim Classe As String = "tbtd"
'  '    '    If i / 2 = CInt(i / 2) Then Classe = "tbtp"
'  '    '    Dim canale As clsChannel2020 = CanaliEntryExit(i)
'  '    '    If IntestazioneCanale Then
'  '    '      strPavarots(i) = "<td Class=""" & Classe & """><b>" & canale.ShortName & "</b></td>"
'  '    '    End If
'  '    '    strPavarots(i) &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailAnte.ChannelSubSet(canale).Avg, "F1") & "</td>"
'  '    '    strPavarots(i) &= "<td Class=""" & Classe & """>" & Format(Dettagli.PavarotDetailPost.ChannelSubSet(canale).Avg, "F1") & "</td>"
'  '    '  Next
'  '    '  IntestazioneCanale = False
'  '    'pv += 1
'  '    '  If ListaPavarots.Count >= 0 AndAlso pvrt > 0 AndAlso (pvrt + 1) Mod 8 = 0 Then
'  '    '    StringaHtml &= "<tr Class=""trtt""><td Class=""Header""> </td>" & strDescrizione & "</tr>"
'  '    '    StringaHtml &= "<tr Class=""trtt""><td Class=""Header"">Channel</td>" & strDescrizione2 & "</tr>"
'  '    '    For Each StrTmp In strPavarots
'  '    '      StringaHtml &= "<tr Class=""trtt"">" & StrTmp & "</tr>"
'  '    '    Next
'  '    '    ReDim strPavarots(CanaliEntryExit.Count - 1)
'  '    '    strDescrizione = ""
'  '    '    strDescrizione2 = ""
'  '    '    IntestazioneCanale = True
'  '    '  End If
'  '    'End If

'  '  Next
'  '  If Not strDescrizione = "" Then
'  '    StringaHtml &= "<tr Class=""trtt""><td Class=""Header""> </td>" & strDescrizione & "</tr>"
'  '    StringaHtml &= "<tr Class=""trtt""><td Class=""Header"">Channel</td>" & strDescrizione2 & "</tr>"
'  '    For Each StrTmp In strPavarots
'  '      StringaHtml &= "<tr Class=""trtt"">" & StrTmp & "</tr>"
'  '    Next
'  '  End If


'  '  StringaHtml &= "</table>"

'  '  StringaHtml &= "</td></tr></table>" 'chiusura tabella esterna

'  '  Return StringaHtml


'  'End Function



'End Class


'Public Class clsReportStraightLineHtml
'  Dim pTestoCompleto As String
'  Dim pPathTemplate As String
'  Dim pListaCanali As List(Of clsChannel2020)
'  Dim pListaPeriodi As List(Of clsPeriod2021)
'  Dim SelettoreCanali As New clsSelezionaCanali

'  Public Sub New(TemplateFileName As String, SelezionaCanali As Boolean, Titolo As String, PiedePagina As String, UpDnRc As clsSailingState.eAndatura, PathFileReport As String) ', ListaCanali As List(Of clsChannel2020), ListaPeriodi As List(Of clsPeriod2020))
'    pPathTemplate = AppConfig.ApplicationDataFolder & "\config\" & TemplateFileName 'GenericReportTemplate.html"

'    If System.IO.File.Exists(pPathTemplate) Then
'      Dim CanaliDaStampare As List(Of clsChannel2020) = Nothing
'      If SelezionaCanali Then
'        CanaliDaStampare = SelettoreCanali.GestisciLista(clsSelezionaCanali.eTipo.eStraightLineHtmlChannels, "Straight Line Report Channel Selector")
'      Else
'        CanaliDaStampare = SelettoreCanali.CaricaListaCorrente(clsSelezionaCanali.eTipo.eStraightLineHtmlChannels)
'      End If
'      If CanaliDaStampare Is Nothing Then Exit Sub
'      If CanaliDaStampare.Count = 0 Then Exit Sub

'      Dim Periodi As New List(Of clsPeriod2021)
'      Dim PO = PeriodsManager.ListaStraightLineVmg.OrderBy(Function(x) CInt(x.TwsDetails.AvgVal)).ThenByDescending(Function(x) x.StraightLineVmgDetails.VmgPerc.AvgVal)
'      'Dim PO = PeriodsManager.StraightLines.ListaStraightLines.OrderByDescending(Function(x) x.Periodo.Vmg).OrderBy(Function(x) CInt(x.Periodo.TWS))
'      For Each SL In PO
'        If SL.StraightLineVmgDetails.IsTWAinRange(UpDnRc) Then
'          If SL.IsChecked Then
'            Periodi.Add(SL)
'          End If
'        End If
'      Next
'      pListaCanali = CanaliDaStampare
'      pListaPeriodi = Periodi
'      CreaFileHtml(Titolo, PiedePagina, PathFileReport)
'    Else
'      MsgBox("Template File Not Found!")
'    End If

'  End Sub

'  Private Sub CreaFileHtml(Titolo As String, PiedePagina As String, PathFileReport As String)
'    pTestoCompleto = IO.File.ReadAllText(pPathTemplate)
'    Clipboard.SetText(pTestoCompleto)
'    Dim SingleBoxSize As Integer = 400
'    Dim SingleBoxPadding As Integer = 50
'    Dim Colonne As Integer = System.Math.Min(3, Int(pListaCanali.Count / 6))
'    Dim Righe As Integer = Int(pListaCanali.Count / Colonne)
'    Dim Diametro As Integer = 5
'    Dim ColoreDefault As String = "blue"

'    Dim Dati As String = "["
'    Dim objTabelle As New clsTabellaPeriodiCanali("StraightLine", pListaPeriodi, pListaCanali)
'    Dim Contatore As Integer = 0
'    For Each Canale In pListaCanali
'      If Not Canale Is pListaCanali.First Then
'        Dim Riga As Integer = Int(Contatore / Colonne)
'        Dim Colonna As Integer = Contatore - Riga * Colonne
'        Dati &= "{ name:'" & Canale.ShortName & "', row:" & Riga & ",col:" & Colonna & ",values:" & vbCrLf & "["
'        Dim RighePunti As List(Of String) = objTabelle.TabellaCanaleHtml(pListaCanali.First, Canale)
'        For Each RigaPunto In RighePunti
'          Dati &= "" & RigaPunto & "," & vbCrLf
'        Next
'        Dati &= "" & vbCrLf & " ] }," & vbCrLf
'        Contatore += 1
'      End If
'    Next
'    Dati &= "]"

'    Dim RigheRisultato As List(Of String) = objTabelle.TabellaPeriodiHtml()

'    '0 Titolo
'    '1 PidePagina
'    '2 Size dei singoli box grafici
'    '3 Padding tra grafici
'    '4 Dati
'    '5 righe
'    '6 colonne
'    '7 diametro singolo punto
'    '8 colore di default
'    Dim Parametri(8)
'    Parametri(0) = Titolo
'    Parametri(1) = PiedePagina & "<br>" & String.Join(vbCrLf, RigheRisultato)
'    Parametri(2) = SingleBoxSize
'    Parametri(3) = SingleBoxPadding
'    Parametri(4) = Dati
'    Parametri(5) = Righe
'    Parametri(6) = Colonne
'    Parametri(7) = Diametro
'    Parametri(8) = ColoreDefault

'    Dim R1 As String = pTestoCompleto.Replace("{", "{{").Replace("}", "}}")
'    For i As Integer = 0 To Parametri.GetUpperBound(0)
'      R1 = R1.Replace("{{" & i.ToString & "}}", "{" & i.ToString & "}")
'    Next
'    Dim Risultato As String = String.Format(R1, Parametri)
'    'ReDim Parametri(2)
'    'Parametri(0) = "Suca"
'    'Parametri(1) = "Pezzeffang"
'    'Parametri(2) = "maledetti"
'    'Dim strTmp As String = "staminchia{0} hairottercazzo{1}" & vbCrLf & "{2}"
'    'Dim Risultato As String = String.Format(strTmp, Parametri)
'    'Clipboard.SetText(Risultato)

'    '    Dim h1 As String = "sono un fottuto genio e te non capisci un caz"
'    '    Dim tit1le As String = "mongelloo capisti?"
'    '    Dim den As String = $"body>
'    '<h2> {title}</h2>  
'    '<div id=chartArea> </div>
'    '<h1>
'    '   {h1}
'    '</h1>
'    '</body>"

'    '    Dim result = den

'    'Dim Risultato As String = String.Format(pTestoCompleto, Parametri)
'    Clipboard.SetText(Risultato)
'    System.IO.File.WriteAllText(PathFileReport, Risultato)
'  End Sub


'End Class

'Public Class clsPuntoHtml
'  Dim pStringaColore As String
'  Dim pX As Double
'  Dim pY As Double
'  Dim pDecimals As Integer

'  Public Sub New(StringaColore As String, x As Double, Y As Double, Decimals As Integer)

'  End Sub

'  Public ReadOnly Property StringaPunto As String
'    Get
'      Dim strTmp As String = "{x:{0},y:{1},color:{2}}"
'      String.Format(strTmp, pStringaColore, Format(pX, "F" & pDecimals.ToString), Format(pY, "F" & pDecimals.ToString))
'      Return strTmp
'    End Get
'  End Property

'End Class


'Public Class clsReportPavarotHtml
'  Dim pTestoCompleto As String
'  Dim pPathTemplate As String
'  Dim pListaCanali As List(Of clsChannel2020)
'  Dim pListaPavarots As List(Of clsPavarot2019)
'  Dim SelettoreCanali As New clsSelezionaCanali

'  Public Sub New(TemplateFileName As String, SelezionaCanali As Boolean, Titolo As String, Tipo As clsPeriod2020.ePeriodType, SoloChecked As Boolean, PathFileReport As String)
'    pPathTemplate = AppConfig.ApplicationDataFolder & "\config\" & TemplateFileName 'GenericReportTemplate.html"


'    If System.IO.File.Exists(pPathTemplate) Then
'      Dim CanaliDaStampare As List(Of clsChannel2020) = Nothing
'      If SelezionaCanali Then
'        Dim TitoloTmp As String = "Tacks Report Channel Selector"
'        If Tipo = clsPeriod2020.ePeriodType.eGybe Then TitoloTmp = "Gybes Report Channel Selector"
'        CanaliDaStampare = SelettoreCanali.GestisciLista(clsSelezionaCanali.eTipo.ePavarotHtmlChannels, TitoloTmp)
'      Else
'        CanaliDaStampare = SelettoreCanali.CaricaListaCorrente(clsSelezionaCanali.eTipo.ePavarotHtmlChannels)
'      End If
'      If CanaliDaStampare Is Nothing Then Exit Sub
'      If CanaliDaStampare.Count = 0 Then Exit Sub

'      Dim Pavarots As New List(Of clsPavarot2019)
'      'Dim PvOrdinate = PeriodsManager.Pavarots.ListaPavarots2019.OrderByDescending(Function(x) x.TotalVmgMetersEntryToExit(clsPavarot2019.eAxisRef.eTWD)).OrderBy(Function(x) CInt(x.TwsMedia / 1))
'      Dim PvOrdinate = PeriodsManager.ListaPavarot.OrderBy(Function(x) CInt(x.ValoriCanaleTWS.Avg)).ThenByDescending(Function(x) x.DettagliPavarot.EntryExitInfos.MetriVmgDelta)
'      For Each Pavarot In PvOrdinate ' PeriodsManager.Pavarots.ListaPavarots2019
'        If Pavarot.PeriodType = Tipo Then
'          If SoloChecked Then
'            If Pavarot.IsChecked Then
'              Pavarots.Add(Pavarot.DettagliPavarot)
'            End If
'          Else
'            Pavarots.Add(Pavarot.DettagliPavarot)
'          End If
'        End If
'      Next
'      pListaCanali = CanaliDaStampare
'      pListaPavarots = Pavarots
'      CreaFileHtml(Titolo, PathFileReport)
'    Else
'      MsgBox("Template File Not Found!")
'    End If

'  End Sub

'  Public Property ListaCanali As List(Of clsChannel2020)
'    Get
'      Return pListaCanali
'    End Get
'    Set(value As List(Of clsChannel2020))
'      pListaCanali = value
'    End Set
'  End Property

'  Private Sub CreaFileHtml(Titolo As String, PathFileReport As String)
'    If pListaPavarots.Count = 0 Then Exit Sub
'    pTestoCompleto = IO.File.ReadAllText(pPathTemplate)
'    Clipboard.SetText(pTestoCompleto)
'    Dim PavReport As New clsReportPavarots
'    Dim TabellaHtml As String = PavReport.TabelloneHtml(pListaCanali, pListaPavarots)

'    Dim Righe As New List(Of String)
'    Dim cSep As String = ","
'    Dim RigaIntestazione As String = ""

'    Dim RigheVmgAvgKtsVsTws As New List(Of String)
'    RigheVmgAvgKtsVsTws.Add("Description" & cSep & "Tws" & cSep & "AvgVmgKts")

'    Dim RigheTotalVmgLateralMeters As New List(Of String)
'    RigheTotalVmgLateralMeters.Add("Description" & cSep & "Lateral" & cSep & "Vmg")

'    Dim RigheLossProgression As New List(Of String)
'    Dim IntestazioneLossProgression As String
'    Dim RigaLossProgression As String


'    For Each Pavarot In pListaPavarots
'			'2 Dati per i grafici a linee
'			Dim IdIniziale As Integer = Pavarot.TimeRangeStandardPavarot.IdRigaIniziale
'			Dim IdFinale As Integer = Pavarot.TimeRangeStandardPavarot.IdRigaFinale
'      Dim IdKeyMoment As Integer = Pavarot.IdMomentoChiave
'      For IdRiga As Integer = IdIniziale To IdFinale
'        Dim Momento As DateTime = DataProvider2020.Momento(IdRiga)
'        Dim IdRigaRel As Integer = IdRiga - IdIniziale
'        RigaIntestazione = "Description" & cSep
'        Dim Riga As String = Pavarot.DescrizionePavarot & cSep
'        RigaIntestazione &= "SecFromKeyMoment" & cSep
'        Riga &= Momento.Subtract(Pavarot.MomentoChiave).TotalSeconds.ToString("F1") & cSep '  (IdRiga - IdKeyMoment) / DataProvider2020.RawFileHz & cSep
'        If Not Pavarot.PavarotData.ListaPuntiVmgVmbDaKeyMoment(IdRigaRel) Is Nothing Then
'          Dim PuntoCartesiano As System.Drawing.PointF = Pavarot.PavarotData.ListaPuntiVmgVmbDaKeyMoment(IdRigaRel).PuntoCartesianoDaRef.ToPointF
'          RigaIntestazione &= "X" & cSep
'          Riga &= PuntoCartesiano.X & cSep
'          RigaIntestazione &= "Y" & cSep
'          Riga &= PuntoCartesiano.Y & cSep
'          For Each Canale In pListaCanali
'            RigaIntestazione &= Canale.ShortName & cSep
'            Riga &= Format(Canale.Valori(IdRiga), "F" & Canale.Decimals.ToString) & cSep
'          Next
'          If Pavarot Is pListaPavarots.First AndAlso IdRiga = IdIniziale Then
'            Righe.Add(RigaIntestazione.TrimEnd(cSep))
'          End If
'          Righe.Add(Riga.TrimEnd(cSep))
'        End If
'      Next


'      '3 Dati per il grafico AvgVmgKts
'      Dim X As Double = Pavarot.MedieStandardPavarot.TWSMedia
'      Dim Y As Double = Pavarot.VmgGainLossTotalAvgKts(Pavarot.TimeRangeEntry.IdRigaFinale, Pavarot.TimeRangeExit.IdRigaIniziale)
'      RigheVmgAvgKtsVsTws.Add(Pavarot.DescrizionePavarot & cSep & Format(X, "F1") & cSep & Format(Y, "F2"))


'      '4 Dati per il grafico TotalVmgLateralMeters
'      X = Pavarot.LateralMetersEntryToExit()
'      Y = Pavarot.EntryExitInfos.MetriVmgDelta
'      RigheTotalVmgLateralMeters.Add(Pavarot.DescrizionePavarot & cSep & Format(X, "F1") & cSep & Format(Y, "F1"))

'      '5 Dati per il grafico RigheLossProgression
'      Dim ssIniziali As Double = Pavarot.TimeRangeEntry.Finish.Subtract(Pavarot.MomentoChiave).TotalSeconds
'      Dim ss As Double = Pavarot.TimeRangeExit.Start.Subtract(Pavarot.MomentoChiave).TotalSeconds
'      IntestazioneLossProgression = "Description" & cSep
'      RigaLossProgression = Pavarot.DescrizionePavarot & cSep
'      For valore As Integer = ssIniziali To ss Step 1
'        Dim GL As Double = Pavarot.VmgMetersEntryToMoment(CDbl(valore))
'        IntestazioneLossProgression &= valore & cSep
'        RigaLossProgression &= Format(GL, "F2") & cSep
'      Next

'      Dim SecsToAdd As Double = 10
'      Dim GLt As Double = Pavarot.VmgMetersEntryToMoment(ss)
'      'DataSeriesTMPt.Append(ss, GLt)
'      Dim BsEntry As Double = Pavarot.MedieEntry.SpeedMedia ' BsChannel.Valori(Pavarot.TimeRangeStandardPavarot.IdRigaFinale)
'      Dim BsExit As Double = Pavarot.MedieExit.SpeedMedia ' BsChannel.Valori(Pavarot.TimeRangeStandardPavarot.IdRigaFinale)
'      Dim TwaEntry As Double = DifferenzaAssolutaTraAngoli360(Pavarot.MedieEntry.TWDMedia, Pavarot.MedieEntry.RottaMedia, True)  'TwaChannel.Valori(Pavarot.TimeRangeEntry.IdRigaFinale)
'      Dim MetriVmgRef As Double = KtsToMS(BsEntry) * System.Math.Abs(System.Math.Cos(Radians(TwaEntry))) * SecsToAdd
'      Dim MetriVmgCalc As Double = KtsToMS(BsExit) * System.Math.Abs(System.Math.Cos(Radians(TwaEntry))) * SecsToAdd
'      Dim Delta As Double = MetriVmgCalc - MetriVmgRef

'      IntestazioneLossProgression &= ss + SecsToAdd & cSep
'      RigaLossProgression &= Format(GLt + Delta, "F2") & cSep
'      'DataSeriesTMPt.Append(ss + SecsToAdd, GLt + Delta)


'      If Pavarot Is pListaPavarots.First Then
'        RigheLossProgression.Add(IntestazioneLossProgression.TrimEnd(cSep))
'      End If
'      RigheLossProgression.Add(RigaLossProgression.TrimEnd(cSep))
'    Next

'    '0 Titolo
'    '1 tabella Dati
'    '2 Dati per i grafici a linee
'    '3 Dati per il grafico AvgVmgKts
'    '4 Dati per il grafico TotalVmgLateralMeters
'    '5 Dati per il grafico RigheLossProgression
'    Dim Parametri(5)
'    Parametri(0) = Titolo
'    Parametri(1) = TabellaHtml
'    Parametri(2) = String.Join(vbCrLf, Righe)
'    Parametri(3) = String.Join(vbCrLf, RigheVmgAvgKtsVsTws)
'    Parametri(4) = String.Join(vbCrLf, RigheTotalVmgLateralMeters)
'    Parametri(5) = String.Join(vbCrLf, RigheLossProgression)
'    Clipboard.SetText(Parametri(2) & vbCrLf & vbCrLf & Parametri(3) & vbCrLf & vbCrLf & Parametri(4) & vbCrLf & vbCrLf & Parametri(5))

'    Dim R1 As String = pTestoCompleto.Replace("{", "{{").Replace("}", "}}")
'    For i As Integer = 0 To Parametri.GetUpperBound(0)
'      R1 = R1.Replace("{{" & i.ToString & "}}", "{" & i.ToString & "}")
'    Next
'    Dim Risultato As String = String.Format(R1, Parametri)

'    System.IO.File.WriteAllText(PathFileReport, Risultato)



'  End Sub




'End Class

'Public Class clsReportAccelerationHtml
'  Dim pTestoCompleto As String
'  Dim pPathTemplate As String
'  Dim pListaCanali As List(Of clsChannel2020)
'  Dim pListaAccelerations As New List(Of clsAcceleration)
'  Dim SelettoreCanali As New clsSelezionaCanali

'  Public Sub New(TemplateFileName As String, SelezionaCanali As Boolean, Titolo As String, Tipo As clsPeriod2020.ePeriodType, SoloChecked As Boolean, PathFileReport As String)
'    pPathTemplate = AppConfig.ApplicationDataFolder & "\config\" & TemplateFileName 'GenericReportTemplate.html"


'    If System.IO.File.Exists(pPathTemplate) Then
'      Dim CanaliDaStampare As List(Of clsChannel2020) = Nothing
'      If SelezionaCanali Then
'        CanaliDaStampare = SelettoreCanali.GestisciLista(clsSelezionaCanali.eTipo.eAccelerationsHtmlChannels, "Accelerations Report Channel Selector")
'      Else
'        CanaliDaStampare = SelettoreCanali.CaricaListaCorrente(clsSelezionaCanali.eTipo.eAccelerationsHtmlChannels)
'      End If
'      If CanaliDaStampare Is Nothing Then Exit Sub
'      If CanaliDaStampare.Count = 0 Then Exit Sub


'      'Dim ListaOrdinata = PeriodsManager.Accelerations.ListaAccelerazioni.OrderByDescending(Function(x) x.MetriVmgFineTest).OrderBy(Function(x) CInt(x.Periodo.TWS))

'      ''Dim IdRigaTmp As Integer = Acc.MomentOfFineTestAccelerazione.IdRiga - Acc.Periodo.TimeRange.IdRigaIniziale
'      ''strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsUpwind, 1, -1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).Y, "F0") & "</td>"
'      ''strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsStbd, -1, 1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).X, "F0") & "</td>"

'      'Dim ListaOrdinata2 = PeriodsManager.Accelerations.ListaAccelerazioni.OrderBy(Function(x) CInt(x.Periodo.TWS)).OrderBy(Function(x) x.MetriVmgFineTest)

'      Dim ListaOrdinata = PeriodsManager.ListaAccelerazioni.OrderBy(Function(x) CInt(x.ValoriCanaleTWS.Avg)).ThenByDescending(Function(x) x.DettagliAcceleration.MetriVmgFineTest)

'      pListaAccelerations.Clear()
'      For Each Acc In ListaOrdinata
'        If Acc.PeriodType = Tipo Then
'          If SoloChecked Then
'            If Acc.IsChecked Then
'              pListaAccelerations.Add(Acc.DettagliAcceleration)
'            End If
'          Else
'            pListaAccelerations.Add(Acc.DettagliAcceleration)
'          End If
'        End If
'      Next
'      pListaCanali = CanaliDaStampare
'      CreaFileHtml(Titolo, PathFileReport)
'    Else
'      MsgBox("Template File Not Found!")
'    End If

'  End Sub

'  Public Property ListaCanali As List(Of clsChannel2020)
'    Get
'      Return pListaCanali
'    End Get
'    Set(value As List(Of clsChannel2020))
'      pListaCanali = value
'    End Set
'  End Property

'  Private Sub CreaFileHtml(Titolo As String, PathFileReport As String)
'    If pListaAccelerations.Count = 0 Then Exit Sub
'    pTestoCompleto = IO.File.ReadAllText(pPathTemplate)
'    Clipboard.SetText(pTestoCompleto)

'    Dim TabellaHtml As String = TabelloneHtml(pListaCanali, pListaAccelerations)

'    Dim Righe As New List(Of String)
'    Dim cSep As String = ","
'    Dim RigaIntestazione As String = ""

'    Dim RigheSecondsToTakeOffVsTws As New List(Of String)
'    RigheSecondsToTakeOffVsTws.Add("Description" & cSep & "Tws" & cSep & "SecToTakeOff")

'    For Each Accel In pListaAccelerations
'      '2 Dati per i grafici a linee
'      Dim RigaInizioCampionamento As Integer = DataProvider2020.TrovaIndice(Accel.MomentOfInizioAccelerazione.Momento.AddSeconds(-10))
'      Dim RigaInizioPeriodoAccelerazione As Integer = Accel.MomentOfInizioAccelerazione.IdRiga
'      Dim RigaInizioTabella As Integer = Accel.Periodo.TimeRange.IdRigaIniziale

'      For IdRiga As Integer = RigaInizioCampionamento To Accel.MomentOfFineTestAccelerazione.IdRiga
'        Dim rigaRaw As Integer = System.Math.Max(0, IdRiga - RigaInizioTabella)
'        If rigaRaw < Accel.MatriceGeoPosRelToPuntoIniziale.Count Then
'          Dim PuntoCartesiano As System.Drawing.PointF = Accel.MatriceGeoPosRelToPuntoIniziale(rigaRaw)
'          Dim SegnoStbd As Integer = IIf(Accel.IsStbd, -1, 1)
'          Dim SegnoUpwind As Integer = IIf(Accel.IsUpwind, 1, -1)

'          RigaIntestazione = "Description" & cSep
'          Dim Riga As String = Accel.MomentOfInizioAccelerazione.Momento.ToLongTimeString & cSep '  Pavarot.DescrizionePavarot & cSep

'          Dim SS As Double = DataProvider2020.Momento(IdRiga).Subtract(Accel.MomentOfInizioAccelerazione.Momento).TotalSeconds
'          'Dim SS As Double = (IdRiga - RigaInizioPeriodoAccelerazione) / DataProvider2020.RawFileHz
'          RigaIntestazione &= "SecFromAccMinSpeed" & cSep
'          Riga &= SS & cSep
'          RigaIntestazione &= "X" & cSep
'          Riga &= Format(SegnoStbd * PuntoCartesiano.X, "F2") & cSep
'          RigaIntestazione &= "Y" & cSep
'          Riga &= Format(PuntoCartesiano.Y, "F2") & cSep
'          RigaIntestazione &= "AbsY" & cSep
'          Riga &= Format(SegnoUpwind * PuntoCartesiano.Y, "F2") & cSep


'          For Each Canale In pListaCanali
'            RigaIntestazione &= Canale.ShortName & cSep
'            Riga &= Format(Canale.Valori(IdRiga), "F" & Canale.Decimals.ToString) & cSep
'          Next
'          If Accel Is pListaAccelerations.First AndAlso IdRiga = RigaInizioCampionamento Then
'            Righe.Add(RigaIntestazione.TrimEnd(cSep))
'          End If
'          Righe.Add(Riga.TrimEnd(cSep))
'        End If
'      Next


'      ''3 Dati per il grafico RigheSecondsToTakeOffVsTws
'      Dim X As Double = Accel.TimeToTakeOffSpeed.TotalSeconds
'      Dim Y As Double = Accel.Periodo.ValoriCanaleTWS.Avg
'      RigheSecondsToTakeOffVsTws.Add(Accel.MomentOfInizioAccelerazione.Momento.ToLongTimeString & cSep & Format(X, "F1") & cSep & Format(Y, "F0"))


'    Next

'    '0 Titolo
'    '1 tabella Dati
'    '2 Dati per i grafici a linee
'    '3 Dati per il grafico RigheSecondsToTakeOffVsTws
'    Dim Parametri(3)
'    Parametri(0) = Titolo
'    Parametri(1) = TabellaHtml
'    Parametri(2) = String.Join(vbCrLf, Righe)
'    Parametri(3) = String.Join(vbCrLf, RigheSecondsToTakeOffVsTws)
'    Clipboard.SetText(Parametri(2) & vbCrLf & vbCrLf & Parametri(3))

'    Dim R1 As String = pTestoCompleto.Replace("{", "{{").Replace("}", "}}")
'    For i As Integer = 0 To Parametri.GetUpperBound(0)
'      R1 = R1.Replace("{{" & i.ToString & "}}", "{" & i.ToString & "}")
'    Next
'    Dim Risultato As String = String.Format(R1, Parametri)

'    System.IO.File.WriteAllText(PathFileReport, Risultato)



'    ''0 Titolo
'    ''1 tabella Dati
'    'Dim Parametri(1)
'    'Parametri(0) = Titolo
'    'Parametri(1) = TabellaHtml

'    'Dim R1 As String = pTestoCompleto.Replace("{", "{{").Replace("}", "}}")
'    'For i As Integer = 0 To Parametri.GetUpperBound(0)
'    '  R1 = R1.Replace("{{" & i.ToString & "}}", "{" & i.ToString & "}")
'    'Next
'    'Dim Risultato As String = String.Format(R1, Parametri)
'    'Clipboard.SetText(Risultato)

'    'System.IO.File.WriteAllText(PathFileReport, Risultato)


'  End Sub



'  Public Function TabelloneHtml(CanaliEntryExit As List(Of clsChannel2020), ListaAccelerazioni As List(Of clsAcceleration)) As String

'    Dim strDescrizione As String = ""
'    Dim strDescrizione2 As String = ""
'    Dim strAcc As String = ""

'    Dim cnt As Integer = 0
'    Dim StringaHtml As String = ""
'    Dim Filtro As String = "Min Speed: " & Format(ListaAccelerazioni.First.MomentOfInizioAccelerazione.Valore, "F1") & " kts, "
'    Filtro &= " TakeOff Speed: " & Format(ListaAccelerazioni.First.MomentOfTakeOffSpeed.Valore, "F1") & " kts"

'    StringaHtml &= vbCrLf & "<table>"
'    StringaHtml &= "<tr class=""trtt""><td colspan=""13"" class=""TableHeader"">Overview</td></tr>" 'Tabella Titolo Overview


'    'StringaHtml &= "<table class=""tbtt""><tr class=""trtt""><td class=""TableHeader"">Overview</td></tr></table>"
'    'StringaHtml &= "<table class=""tbtt"">"

'    StringaHtml &= "<tr><td class=""TopHeader"" colspan=""4"">" & Filtro & "</td>"
'    StringaHtml &= "<td class=""TopHeader"" colspan=""5"">Data @ Acceleration's end</td><td class=""TopHeader""colspan=""4"">Data @ TakeOff</td></tr>"

'    StringaHtml &= "<tr><td class=""Header"">Start Time</td><td class=""Header"">UpDn</td><td class=""Header"">Tack</td><td class=""Header"">Tws</td>"
'    StringaHtml &= "<td class=""Header"">Test Duration sec</td><td class=""Header"">Vmg Meters</td><td class=""Header"">Lateral Meters</td><td class=""Header"">Final Bs</td><td class=""Header"">Final Twa</td>"
'    StringaHtml &= "<td class=""Header"">Sec from MinSpeed</td><td class=""Header"">Vmg Meters</td><td class=""Header"">Lateral Meters</td><td class=""Header"">Twa</td></tr>"

'    'Dim ListaOrdinata = ListaAccelerazioni.OrderBy(Function(x) x.Periodo.TWS)


'    For Each Acc In ListaAccelerazioni
'      Dim Classe As String = "tbtd"
'      If cnt / 2 = CInt(cnt / 2) Then Classe = "tbtp"
'      strDescrizione = "<td Class=""" & Classe & """>#" & cnt + 1 & " " & Acc.MomentOfInizioAccelerazione.Momento.ToLongTimeString & " " & Acc.Periodo.ShortDescription & "</td>"
'      strDescrizione &= "<td Class=""" & Classe & """>" & If(Acc.IsUpwind, "Upwind", "Downwind") & "</td>"
'      strDescrizione &= "<td Class=""" & Classe & """>" & If(Acc.IsStbd, "Stbd", "Port") & "</td>"
'      strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.Periodo.ValoriCanaleTWS.Avg, "F1") & "</td>"

'      strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.TestDurationInSeconds, "F0") & "</td>"
'      Dim IdRigaTmp As Integer = Acc.MomentOfFineTestAccelerazione.IdRiga - Acc.MomentOfInizioAccelerazione.IdRiga
'      If IdRigaTmp < Acc.MatriceGeoPosRelToPuntoIniziale.Count Then
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsUpwind, 1, -1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).Y, "F0") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsStbd, -1, 1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).X, "F0") & "</td>"
'        'strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalVmg, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalSpeed, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalAbsTWA, "F0") & "</td>"
'      Else
'        strDescrizione &= "<td Class=""" & Classe & """>-</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>-</td>"
'        'strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalVmg, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalSpeed, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.FinalAbsTWA, "F0") & "</td>"
'      End If

'      IdRigaTmp = Acc.MomentOfTakeOffSpeed.IdRiga - Acc.MomentOfInizioAccelerazione.IdRiga
'      If IdRigaTmp < Acc.MatriceGeoPosRelToPuntoIniziale.Count Then
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.TimeToTakeOffSpeed.TotalSeconds, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsUpwind, 1, -1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).Y, "F0") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(IIf(Acc.IsStbd, -1, 1) * Acc.MatriceGeoPosRelToPuntoIniziale(IdRigaTmp).X, "F0") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.TakeOffAbsTWA, "F0") & "</td>"
'      Else
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.TimeToTakeOffSpeed.TotalSeconds, "F1") & "</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>-</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>-</td>"
'        strDescrizione &= "<td Class=""" & Classe & """>" & Format(Acc.TakeOffAbsTWA, "F0") & "</td>"
'      End If



'      StringaHtml &= "<tr Class=""trtt"">" & strDescrizione & "</tr>"
'      cnt += 1
'    Next

'    StringaHtml &= "</table>"

'    StringaHtml &= "</br>"
'    cnt = 0
'    StringaHtml &= vbCrLf & "<table Class=""tbtt"">" 'tabella B
'    Dim cs As Integer = 1 + (ListaAccelerazioni.Count * 3)
'    If ListaAccelerazioni.Count > 10 Then cs = 1 + (8 * 3)
'    StringaHtml &= "<tr><td colspan=""" & 1 + (ListaAccelerazioni.Count * 3) & """ Class=""TableHeader"">Start, TakeOff and Acceleration's end Data Details</td></tr>"

'    strDescrizione = ""
'    strDescrizione2 = ""
'    strAcc = ""
'    Dim strAccs(CanaliEntryExit.Count - 1) As String
'    Dim IntestazioneCanale As Boolean = True
'    For Each Acc In ListaAccelerazioni
'      strDescrizione &= "<td class=""TopHeader"" colspan=""3"">#" & cnt + 1 & " " & Acc.MomentOfInizioAccelerazione.Momento.ToLongTimeString & "</td>"
'      strDescrizione2 &= "<td Class=""Header"">Start</td><td Class=""Header"">TakeOff</td><td Class=""Header"">Final</td>"
'      Dim IdRigaIniziale As Integer = Acc.MomentOfInizioAccelerazione.IdRiga
'      Dim IdRigaTakeOff As Integer = Acc.MomentOfTakeOffSpeed.IdRiga
'      Dim IdRigaFinale As Integer = Acc.MomentOfFineTestAccelerazione.IdRiga
'      For i As Integer = 0 To CanaliEntryExit.Count - 1
'        Dim Classe As String = "tbtd"
'        If i / 2 = CInt(i / 2) Then Classe = "tbtp"
'        Dim canale As clsChannel2020 = CanaliEntryExit(i)
'        If IntestazioneCanale Then
'          strAccs(i) = "<td Class=""" & Classe & """><b>" & canale.ShortName & "</b></td>"
'        End If
'        strAccs(i) &= "<td Class=""" & Classe & """>" & Format(canale.Valori(IdRigaIniziale), "F1") & "</td>"
'        strAccs(i) &= "<td Class=""" & Classe & """>" & Format(canale.Valori(IdRigaTakeOff), "F1") & "</td>"
'        strAccs(i) &= "<td Class=""" & Classe & """>" & Format(canale.Valori(IdRigaFinale), "F1") & "</td>"
'      Next
'      IntestazioneCanale = False
'      cnt += 1
'      If ListaAccelerazioni.Count >= 0 AndAlso cnt > 0 AndAlso (cnt + 1) Mod 8 = 0 Then
'        StringaHtml &= "<tr Class=""trtt""><td Class=""Header""> </td>" & strDescrizione & "</tr>"
'        StringaHtml &= "<tr Class=""trtt""><td Class=""Header"">Channel</td>" & strDescrizione2 & "</tr>"
'        For Each StrTmp In strAccs
'          StringaHtml &= "<tr Class=""trtt"">" & StrTmp & "</tr>"
'        Next
'        ReDim strAccs(CanaliEntryExit.Count - 1)
'        strDescrizione = ""
'        strDescrizione2 = ""
'        IntestazioneCanale = True
'      End If
'    Next

'    If Not strDescrizione = "" Then
'      StringaHtml &= "<tr Class=""trtt""><td Class=""Header""> </td>" & strDescrizione & "</tr>"
'      StringaHtml &= "<tr Class=""trtt""><td Class=""Header"">Channel</td>" & strDescrizione2 & "</tr>"
'      For Each StrTmp In strAccs
'        StringaHtml &= "<tr Class=""trtt"">" & StrTmp & "</tr>"
'      Next
'    End If

'    StringaHtml &= "</table>"

'    Return StringaHtml


'  End Function



'End Class
