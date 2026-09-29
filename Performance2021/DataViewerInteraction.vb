Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient


Public Interface IDataRepository
  Function SendRequestToDb(ByVal SqlString As String)
  Function CheckColumns(ByVal channelsName As String)
  'Function FindTests(column As String, ByVal WHERE As String) As List(Of Test)
  Function ExecuteInsert(ByVal sql As String) As Integer
End Interface

Public Class clsFiltroSql
  Dim pRefToAvgVal As Boolean
  Dim pStrCanale As String = ""
  Dim pRange As clsDoubleRange = Nothing
  Dim pValore As Double = Nothing
  Dim pStringaCustom As String = ""

  Public Sub New(RefToAvgVal As Boolean, Canale As clsChannel2020, Range As clsDoubleRange, Valore As Double)
    pRefToAvgVal = RefToAvgVal
    pStrCanale = Canale.SqlDbName
    pRange = Range
    pValore = Valore
  End Sub

  Public Sub New(RefToAvgVal As Boolean, StringaCanale As String, Range As clsDoubleRange, Valore As Double)
    pRefToAvgVal = RefToAvgVal
    pStrCanale = StringaCanale
    pRange = Range
    pValore = Valore
  End Sub

  Public Sub New(StringaCustom As String)
    pStringaCustom = StringaCustom
  End Sub

  Public Property StrCanale As String
    Get
      Return pStrCanale
    End Get
    Set(value As String)
      pStrCanale = value
    End Set
  End Property

  Public Property Range As clsDoubleRange
    Get
      Return pRange
    End Get
    Set(value As clsDoubleRange)
      pRange = value
    End Set
  End Property

  Public Property Valore As Double
    Get
      Return pValore
    End Get
    Set(value As Double)
      pValore = value
    End Set
  End Property

  Public Property RefToAvgVal As Boolean
    Get
      Return pRefToAvgVal
    End Get
    Set(value As Boolean)
      pRefToAvgVal = value
    End Set
  End Property

  Public Function stringaWhere() As String
    If Not pStringaCustom = "" Then Return pStringaCustom
    If pRefToAvgVal Then
      Return " Avg(" & pStrCanale & ")" & stringaIntervallo() & ""
    Else
      Return " " & pStrCanale & " " & stringaIntervallo() & ""
    End If

  End Function

  Private Function stringaIntervallo()
    If pRange Is Nothing Then
      Return " = " & pValore & ""
    Else
      Return " Between " & System.Math.Min(pRange.Min, pRange.Max) & " And " & System.Math.Max(pRange.Min, pRange.Max) & ""
    End If
  End Function

End Class


Public Class clsSqlServerDb
  Implements IDataRepository
  Implements INotifyPropertyChanged

  Private myConn As SqlConnection

  Dim pBoatConfig As Boolean

  Public Property BoatConfig As Boolean
    Get
      Return pBoatConfig
    End Get
    Set(value As Boolean)
      pBoatConfig = value
    End Set
  End Property

  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

  Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
  End Sub


  Public Function SendRequestToDb(SqlString As String) As Object Implements IDataRepository.SendRequestToDb
    Try
      Dim sqlCmd As SqlCommand
      Dim strSql As SqlParameter = New SqlParameter("sql", SqlString)
      sqlCmd = New SqlCommand("ExecuteReadOnlySQL", myConn)
      sqlCmd.CommandType = System.Data.CommandType.StoredProcedure
      sqlCmd.Parameters.Add(strSql)
      Dim myReader As SqlDataReader = sqlCmd.ExecuteReader()
      Return myReader
    Catch exSql As SqlException
      MsgBox("Db Search Error: " & exSql.Message)
      Return Nothing
    Catch ex As Exception
      MsgBox("Db Search Error: " & ex.Message)
      Return Nothing
    End Try
  End Function


  Public Function CheckColumns(channelsName As String) As Object Implements IDataRepository.CheckColumns
    Dim sqlCmd As SqlCommand
    'Dim connection = ConnectToDb()
    Dim columns As SqlParameter = New SqlParameter("ColumnsComma", channelsName)
    columns.Direction = System.Data.ParameterDirection.Input

    sqlCmd = New SqlCommand("AddMissingColumns", myConn)

    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure
    sqlCmd.Parameters.Add(columns)

    Dim myReader As SqlDataReader = sqlCmd.ExecuteReader()
    myReader.Close()
    'sqlCmd.Dispose()

  End Function


  Public Function GetDouble(StringaSql) As Double
    Try
      If myConn Is Nothing Then
        ConnectToDb()
      ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
        myConn.Open()
      End If
      Dim sqlCmd As SqlCommand
      sqlCmd = New SqlCommand(StringaSql, myConn)
      Return sqlCmd.ExecuteScalar
    Catch ex As Exception
      MsgBox("Query Error")
      Return Nothing
    End Try
  End Function

  Public Function GetSqlDataReader(StringaSql As String) As SqlDataReader
    Try
      If myConn Is Nothing Then
        ConnectToDb()
      ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
        myConn.Open()
      End If
      Dim sqlCmd As SqlCommand
      sqlCmd = New SqlCommand(StringaSql, myConn)
      Dim reader As SqlDataReader = sqlCmd.ExecuteReader()
      Return reader
    Catch ex As Exception
      MsgBox("Query Error")
      Return Nothing
    End Try

  End Function

  Public Function ExecuteInsert(sql As String) As Integer Implements IDataRepository.ExecuteInsert
    Dim sqlCmd As SqlCommand
    Dim connection = ConnectToDb()
    sqlCmd = New SqlCommand(sql, connection)
    Return sqlCmd.ExecuteNonQuery
  End Function

  Public Function ListaCanaliDb() As List(Of String)
    Dim ListaTmp As New List(Of String)
    If myConn Is Nothing Then
      ConnectToDb()
    ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
      myConn.Open()
    End If
    Dim strSql As String = "SELECT
	COLUMN_NAME
FROM
  	INFORMATION_SCHEMA.COLUMNS
WHERE
	TABLE_NAME = 'Data'"
    Dim sqlCmd As SqlCommand = New SqlCommand(strSql, myConn)
    Dim reader As SqlDataReader = sqlCmd.ExecuteReader()
    Dim Aggiungi As Boolean = False
    While reader.Read
      Dim Header As String = reader.GetString(0)
      If Aggiungi Then
        ListaTmp.Add(Header)
      Else
        Aggiungi = (Header = "Time") 'aggiunge alla lista solo le colonne dopo la colonna time
      End If
    End While
    reader.Close()
    myConn.Close()
    ListaTmp.Sort()
    Return ListaTmp
  End Function

  Public Sub EliminaDati(SqlPeriodi As String, SqlDati As String)
    Dim ListaTmp As New List(Of String)
    If myConn Is Nothing Then
      ConnectToDb()
    ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
      myConn.Open()
    End If
    Dim sqlCmd As New SqlCommand(SqlPeriodi, myConn)
    Dim RigheEliminate As Integer = sqlCmd.ExecuteNonQuery()
    'elimina tutti le righe relativi a periodi che esistono solo sul Db
    sqlCmd = New SqlCommand(SqlDati, myConn)
    RigheEliminate = sqlCmd.ExecuteNonQuery()
    myConn.Close()
  End Sub


  Private Function ConnectToDb() As SqlConnection
    Dim str As String = ConfigurationManager.ConnectionStrings("dbConnection").ToString()

    myConn = New SqlConnection(str)

    myConn.Open()
    Return myConn
  End Function

  Private Function SqlInstertInto() As String
    Dim strSql As String = "INSERT INTO dbo.Periods "
    'strSql &= "(Start,Finish,TypeId,IdUser,IdBoat,KeyMoment,ShortDescription,ExtendedDescription,Keys,SourceFileName) "
    'strSql &= "VALUES ('" & Periodo.TR.Inizio.ToString("yyyy/MM/dd HH:mm:ss.fff") & "',"
    'strSql &= "'" & Periodo.TR.Fine.ToString("yyyy/MM/dd HH:mm:ss.fff") & "',"
    'strSql &= "" & Periodo.Periodo.PeriodType & ","
    'strSql &= "" & IdUtente & ","
    'strSql &= "" & IdBarca & ","
    'strSql &= IIf(Periodo.Periodo.KeyMoment = Nothing, "NULL", "'" & Periodo.Periodo.KeyMoment.ToString("yyyy/MM/dd HH:mm:ss.fff") & "'") & ","
    'strSql &= "'" & Periodo.Periodo.ShortDescription & "',"
    'strSql &= "'" & Periodo.Periodo.ExtendedDescription & "',"
    'strSql &= "'" & Periodo.Periodo.Keys & "',"
    'strSql &= "'" & ObjDataProvider.TrovaFile(Periodo.TR.Fine).Name.Trim & "') "
    Return strSql
  End Function

  Private Function SqlUpdate() As String '(Periodo As clsPeriodoMod, IdUtente As Integer, IdBarca As Integer) As String
    Dim strSql As String = "UPDATE dbo.Periods "
    'strSql &= "SET Start='" & Periodo.TR.Inizio.ToString("yyyy/MM/dd HH:mm:ss.fff") & "',"
    'strSql &= "Finish='" & Periodo.TR.Fine.ToString("yyyy/MM/dd HH:mm:ss.fff") & "',"
    'strSql &= "IdUser=" & IdUtente & ","
    'strSql &= "IdBoat=" & IdBarca & ","
    'strSql &= "TypeId=" & Periodo.Periodo.PeriodType & ","
    'strSql &= "KeyMoment=" & IIf(Periodo.Periodo.KeyMoment = Nothing, "NULL", "'" & Periodo.Periodo.KeyMoment.ToString("yyyy/MM/dd HH:mm:ss.fff") & "'") & ","
    'strSql &= "ShortDescription='" & Periodo.Periodo.ShortDescription & "',"
    'strSql &= "ExtendedDescription='" & Periodo.Periodo.ExtendedDescription & "',"
    'strSql &= "Keys='" & Periodo.Periodo.Keys & "',"
    'strSql &= "SourceFileName='" & ObjDataProvider.TrovaFile(Periodo.TR.Fine).Name.Trim & "' "
    'strSql &= "WHERE Id=" & Periodo.Periodo.SqlId & " "
    Return strSql
  End Function


  Private Function StringaWhereIntervallo(TR As clsTimeRange) As String
    Return "Start between '" & TR.Start.ToString("yyyy/MM/dd HH:mm:ss") & "' and '" & TR.Finish.ToString("yyyy/MM/dd  HH:mm:ss") & "'"
  End Function

  'Public Sub SincronizzaPeriodiLocaliConDb(Periodi As List(Of clsPeriod), IdUtente As Integer, IdBarca As Integer, ByRef ProgressBar As clsProgressBar)

  '  Dim ContaOperazioni(3) As Integer


  '  ProgressBar.Min = 0
  '  ProgressBar.Max = 1
  '  ProgressBar.Value = 0.01
  '  ProgressBar.ValuePerc = "0%"
  '  System.Windows.Forms.Application.DoEvents()

  '  Dim ImportStep As Integer = 1
  '  Dim PrimoId As Integer = 500
  '  Dim Campioni As Integer = 1000
  '  Dim delta As Double = ObjDataProvider.ObjChannels.CanaleDateTime.ValuesDT(PrimoId + Campioni).Value.Subtract(ObjDataProvider.ObjChannels.CanaleDateTime.ValuesDT(PrimoId).Value).TotalSeconds
  '  Dim Hz As Integer = Campioni / delta
  '  ImportStep = System.Math.Max(1, Hz / 10)

  '  'Dim Totale As Double = 0
  '  'For Each periodo In PeriodsManager.Periods
  '  '  Totale += (periodo.TimeRange.IdRigaFinaleAssociata - periodo.TimeRange.IdRigaInizialeAssociata) / ImportStep
  '  'Next
  '  'tutti i periodi presenti nei file log caricati
  '  Dim ListaPeriodiDataProvider As New List(Of clsPeriodoMod)



  '  For Each Periodo In Periodi
  '    ListaPeriodiDataProvider.Add(New clsPeriodoMod(Periodo, clsPeriodoMod.eTipoPeriodo.eSoloLog))
  '  Next

  '  If myConn Is Nothing Then
  '    ConnectToDb()
  '  ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
  '    myConn.Open()
  '  End If
  '  Dim sqlCmd As SqlCommand
  '  Dim strSql As String

  '  'strSql = "SELECT Id FROM Periods WHERE IdUser = " & IdUtente & " And CONVERT(VARCHAR(10), Periods.Start, 111) = '" & Periodi.First.TimeRange.Inizio.ToString("yyyy/MM/dd") & "' "

  '  Dim ListaPeriodiDb As New List(Of clsPeriodoMod)

  '  For Each Fl In ObjDataProvider.FCs
  '    ' cerca i periodi nel db sqlserver contenuti nei file caricati
  '    'strSql = "SELECT Id, Start, Finish FROM Periods WHERE IdUser = " & IdUtente & " And " & StringaWhereIntervallo(Fl.RawTimeRange) & " "
  '    strSql = "SELECT Id, Start, Finish FROM Periods WHERE IdUser = " & IdUtente & " And SourceFileName = '" & Fl.FI.Name & "' "
  '    sqlCmd = New SqlCommand(strSql, myConn)
  '    Dim reader As SqlDataReader = sqlCmd.ExecuteReader()
  '    'cerca tutti i test presenti nel db fatti dall'utente relativi ai file log caricati
  '    While reader.Read
  '      Dim id = reader.GetInt32(reader.GetOrdinal("Id"))
  '      Dim St As DateTime = reader.GetDateTime(reader.GetOrdinal("Start"))
  '      Dim Fn As DateTime = reader.GetDateTime(reader.GetOrdinal("Finish"))
  '      Dim pm As New clsPeriodoMod(Nothing, clsPeriodoMod.eTipoPeriodo.eSoloDb)
  '      pm.Id = id
  '      pm.TR = New clsTimeRange(St, Fn)
  '      ListaPeriodiDb.Add(pm)
  '    End While
  '    reader.Close()
  '  Next

  '  'strSql = "SELECT Id, Start, Finish FROM Periods WHERE IdUser = " & IdUtente & " And " & StringaWhereIntervallo(Inizio, Fine) & " "
  '  'sqlCmd = New SqlCommand(strSql, myConn)
  '  'Dim reader As SqlDataReader = sqlCmd.ExecuteReader()
  '  'Dim ListaPeriodiDb As New List(Of clsPeriodoMod)
  '  ''cerca tutti i test presenti nel db fatti dall'utente relativi ai file log caricati
  '  'While reader.Read
  '  '  Dim id = reader.GetInt32(reader.GetOrdinal("Id"))
  '  '  Dim St As DateTime = reader.GetDateTime(reader.GetOrdinal("Start"))
  '  '  Dim Fn As DateTime = reader.GetDateTime(reader.GetOrdinal("Finish"))
  '  '  Dim pm As New clsPeriodoMod(Nothing, clsPeriodoMod.eTipoPeriodo.eSoloDb)
  '  '  pm.Id = id
  '  '  pm.TR = New clsTimeRange(St, Fn)
  '  '  ListaPeriodiDb.Add(pm)
  '  'End While
  '  'reader.Close()

  '  Dim ListaPeriodi As New List(Of clsPeriodoMod)
  '  'caratterizza nella lista completa i periodi nuovi e quelli giá presenti nel db
  '  Dim Totale As Double = 0
  '  For Each Elemento In ListaPeriodiDataProvider
  '    Totale += Elemento.TR.IdRigaFinaleAssociata - Elemento.TR.IdRigaInizialeAssociata
  '    ListaPeriodi.Add(CercaPeriodo(Elemento, ListaPeriodiDb))
  '  Next

  '  'aggiunge gli elementi presenti solo nel db (che andranno eliminati)
  '  Dim ListaIdSoloDb As New List(Of Integer)
  '  For Each Elemento In ListaPeriodiDb
  '    ListaIdSoloDb.Add(Elemento.Id)
  '    ListaPeriodi.Add(Elemento)
  '  Next

  '  Dim Progressivo As Double = 0
  '  Dim ColonneCambiate As Boolean = False
  '  For Each PeriodoMod In ListaPeriodi
  '    Dim Periodo As clsPeriod = PeriodoMod.Periodo
  '    If Not Periodo Is Nothing Then
  '      If Periodo Is Periodi.First Then
  '        'qui va aggiunto che se il numero di cmpi é cambiato vanno ricaricati tutti i dati
  '        Dim strConteggio As String = "select count(COLUMN_NAME) from INFORMATION_SCHEMA.COLUMNS where table_name = 'data'"
  '        sqlCmd = New SqlCommand(strConteggio, myConn)
  '        Dim ColonnePresenti As Integer = sqlCmd.ExecuteScalar

  '        'é il primo periodo, aggiunge le colonne mancanti
  '        Dim strCanali As String = ""
  '        For Each canale In ObjDataProvider.ObjChannels.CanaliCaricati
  '          If canale.IsNumericOrBoolean Then
  '            strCanali &= canale.SqlDbName & ";"
  '          End If
  '        Next
  '        CheckColumns(strCanali.TrimEnd(";"))

  '        sqlCmd = New SqlCommand(strConteggio, myConn)
  '        ColonneCambiate = Not (ColonnePresenti = sqlCmd.ExecuteScalar)
  '      End If
  '    End If


  '    Dim AccodaDati As Boolean = False
  '    'PeriodoMod.TipoPeriodo = clsPeriodoMod.eTipoPeriodo.eEntrambiTRdiverso
  '    Select Case PeriodoMod.TipoPeriodo
  '      Case clsPeriodoMod.eTipoPeriodo.eSoloDb
  '        ' elimina periodi e dati dal Db
  '        strSql = "DELETE FROM Periods WHERE IdUser = " & IdUtente & " And Id IN (" & String.Join(",", ListaIdSoloDb) & ")"
  '        sqlCmd = New SqlCommand(strSql, myConn)
  '        Dim RigheEliminate As Integer = sqlCmd.ExecuteNonQuery()
  '        'elimina tutti le righe relativi a periodi che esistono solo sul Db
  '        strSql = "DELETE FROM Data WHERE PeriodId IN (" & String.Join(",", ListaIdSoloDb) & ")"
  '        sqlCmd = New SqlCommand(strSql, myConn)
  '        RigheEliminate = sqlCmd.ExecuteNonQuery()
  '        AccodaDati = False
  '        ContaOperazioni(0) += 1
  '      Case clsPeriodoMod.eTipoPeriodo.eSoloLog
  '        'aggiunge il nuovo periodo ed aggiunge le righe
  '        strSql = SqlInstertInto(PeriodoMod, IdUtente, IdBarca)
  '        sqlCmd = New SqlCommand(strSql, myConn)
  '        Dim RigheSalvate As Integer = sqlCmd.ExecuteNonQuery()
  '        sqlCmd = New SqlCommand("SELECT IDENT_CURRENT('dbo.Periods')", myConn)
  '        Periodo.SqlId = sqlCmd.ExecuteScalar
  '        Periodo.SalvaInLocale()
  '        AccodaDati = True
  '        ContaOperazioni(1) += 1
  '      Case clsPeriodoMod.eTipoPeriodo.eEntrambiTRuguale
  '        'non fa nulla
  '        AccodaDati = False
  '        ContaOperazioni(2) += 1
  '      Case clsPeriodoMod.eTipoPeriodo.eEntrambiTRdiverso
  '        'aggiorna i dati del periodo
  '        strSql = SqlUpdate(PeriodoMod, IdUtente, IdBarca)
  '        sqlCmd = New SqlCommand(strSql, myConn)
  '        Dim RigheSalvate As Integer = sqlCmd.ExecuteNonQuery
  '        'elimina le righe precedenti
  '        strSql = "DELETE FROM Data WHERE PeriodId = " & Periodo.SqlId & ""
  '        sqlCmd = New SqlCommand(strSql, myConn)
  '        Dim RigheEliminate As Integer = sqlCmd.ExecuteNonQuery()
  '        AccodaDati = True
  '        ContaOperazioni(3) += 1
  '    End Select
  '    'Progressivo += (Periodo.TimeRange.IdRigaFinaleAssociata - Periodo.TimeRange.IdRigaInizialeAssociata)
  '    'ProgressBar.Value = Progressivo / Totale
  '    'ProgressBar.ValuePerc = Format(Progressivo / Totale * 100, "F0") & "%"
  '    'System.Windows.Forms.Application.DoEvents()

  '    If ColonneCambiate OrElse AccodaDati Then
  '      'salva una ad una tutte le righe del periodo
  '      Dim Mezzanotte As New DateTime(Periodo.TimeRange.Inizio.Year, Periodo.TimeRange.Inizio.Month, Periodo.TimeRange.Inizio.Day, 0, 0, 0, 0)
  '      Dim Contatore As Integer = 0
  '      Dim RigheFlush As Integer = 1000
  '      strSql = ""
  '      Dim IdRigaIniziale As Integer = PeriodoMod.TR.IdRigaInizialeAssociata
  '      Dim IdRigaFinale As Integer = PeriodoMod.TR.IdRigaFinaleAssociata

  '      For i As Integer = System.Math.Max(0, IdRigaIniziale) To System.Math.Min(ObjDataProvider.FileTimeRange.IdRigaFinaleAssociata, IdRigaFinale) Step ImportStep
  '        Dim Momento As DateTime = ObjDataProvider.ObjChannels.CanaleDateTime.ValuesDT(i)
  '        strSql &= "INSERT INTO dbo.Data "
  '        Dim strHeaders As String = "(PeriodId,Time,"
  '        Dim strValues As String = " VALUES (" & Periodo.SqlId & "," & Momento.Subtract(Mezzanotte).TotalSeconds & ","
  '        For Each canale In ObjDataProvider.ObjChannels.CanaliCaricati
  '          If canale.IsNumericOrBoolean Then
  '            'If canale.SqlDbName = "_TWD" Then Stop
  '            strHeaders &= canale.SqlDbName & ","
  '            Dim v As Nullable(Of Double) = canale.ValorePerSql(i)
  '            If Not v Is Nothing AndAlso Double.IsInfinity(v) Then v = Nothing
  '            strValues &= IIf(Not v.HasValue, "NULL", v) & ","
  '          End If
  '        Next
  '        strSql &= strHeaders.TrimEnd(",") & ") " & strValues.TrimEnd(",") & ")"
  '        strSql = strSql.Replace("NaN", "NULL")
  '        'sqlCmd = New SqlCommand(strSql, myConn)
  '        If Contatore >= RigheFlush Then
  '          sqlCmd = New SqlCommand(strSql, myConn)
  '          Dim RigheSalvate As Integer = sqlCmd.ExecuteNonQuery()
  '          Contatore = 0
  '          strSql = ""
  '          Progressivo += RigheFlush
  '          ProgressBar.Value = Progressivo / Totale
  '          ProgressBar.ValuePerc = Format(Progressivo / Totale * 100, "F0") & "%"
  '          System.Windows.Forms.Application.DoEvents()
  '        Else
  '          Contatore += 1
  '          strSql &= ";"
  '        End If
  '      Next
  '      If Contatore <= RigheFlush Then
  '        sqlCmd = New SqlCommand(strSql & ";", myConn)
  '        Dim RigheSalvate As Integer = sqlCmd.ExecuteNonQuery()
  '        Progressivo += Contatore
  '        ProgressBar.Value = Progressivo / Totale
  '        ProgressBar.ValuePerc = Format(Progressivo / Totale * 100, "F0") & "%"
  '        System.Windows.Forms.Application.DoEvents()
  '      End If
  '    End If
  '  Next
  '  myConn.Close()

  '  ProgressBar.Min = 0
  '  ProgressBar.Max = 0
  '  ProgressBar.Value = 0
  '  ProgressBar.ValuePerc = ""
  '  System.Windows.Forms.Application.DoEvents()
  '  Dim Test As String = "Total Periods: " & Periodi.Count & vbCrLf
  '  Test &= "StraightLines: " & PeriodsManager.StraightLines.ListaStraightLines.Count & vbCrLf
  '  Test &= "Tacks and Gybes: " & PeriodsManager.Pavarots.ListaPavarots2019.Count & vbCrLf
  '  Test &= "Accelerations: " & PeriodsManager.Accelerations.ListaAccelerazioni.Count & vbCrLf & vbCrLf
  '  Test &= "Elaborated: " & (ContaOperazioni(0) + ContaOperazioni(1) + ContaOperazioni(2) + ContaOperazioni(3)) & vbCrLf
  '  Test &= "New from log to Db: " & ContaOperazioni(1) & vbCrLf
  '  Test &= "Not on log anymore, Deleted from the Db: " & ContaOperazioni(0) & vbCrLf
  '  Test &= "Already fully synced: " & ContaOperazioni(2) & vbCrLf
  '  Test &= "Already present but time range changed so resynced: " & ContaOperazioni(3) & vbCrLf
  '  MsgBox(Test, MsgBoxStyle.OkOnly)

  'End Sub


  Public Function StringaQuerySql(ListaCanali As List(Of String), ListaFiltriAvg As List(Of clsFiltroSql), ListaFiltriValori As List(Of clsFiltroSql), StringaCases As String) As String
    'Dim strTmp As String = "Select TOP(100) PERCENT Data.PeriodId as SqlId, PeriodType.Description AS Type, Periods.SourceFileName as Source"
    pBoatConfig = Not ListaCanali.Find(Function(x) x = "BoatConfigId") Is Nothing
    If StringaCases.Trim = "" Then pBoatConfig = False
    Dim strTmp As String = "Select Data.PeriodId as SqlId, PeriodType.Description AS Type, Periods.SourceFileName as Source"
    strTmp &= ", CONVERT(VARCHAR(10), Periods.Start, 111) AS Day, CONVERT(VARCHAR(10), Periods.Start, 8) AS Start, CONVERT(VARCHAR(10), Periods.Finish-Periods.Start, 8) AS Duration"
    strTmp &= ", Periods.ShortDescription, COUNT(Data.Id) AS Samples"
    strTmp &= ", dbo.Periods.KeyMoment, dbo.Periods.TypeId, dbo.Periods.Finish as EndTime, dbo.Periods.Start AS StartTime"
    strTmp &= ", dbo.Periods.IdUser, dbo.Periods.ExtendedDescription, dbo.Periods.Keys"
    If pBoatConfig Then
      strTmp &= ", case "
      strTmp &= StringaCases
      strTmp &= " end as Competitor "
    End If
    For Each canale In ListaCanali
      strTmp &= ", AVG(Data." & canale & ") as AvgOf" & canale & ""
      strTmp &= ", MAX(Data." & canale & ") as MaxOf" & canale & ""
      strTmp &= ", MIN(Data." & canale & ") as MinOf" & canale & ""
      strTmp &= ", STDEV(Data." & canale & ") as StDevOf" & canale & ""
    Next
    strTmp &= " From Data INNER Join "
    strTmp &= "Periods ON Data.PeriodId = Periods.Id LEFT OUTER Join "
    strTmp &= "PeriodType ON Periods.TypeId = PeriodType.AppId "
    strTmp &= "GROUP BY Data.PeriodId, " & IIf(pBoatConfig, "Data.BoatConfigId ,", "") & " PeriodType.Description,Periods.SourceFileName, CONVERT(VARCHAR(10), Periods.Start, 111) , CONVERT(VARCHAR(10), Periods.Start, 8) , CONVERT(VARCHAR(10), Periods.Finish-Periods.Start, 8), Periods.ShortDescription, PeriodType.Description, "
    strTmp &= "dbo.Periods.KeyMoment, dbo.Periods.TypeId, dbo.Periods.Finish, dbo.Periods.Start, dbo.Periods.IdUser, dbo.Periods.ExtendedDescription, dbo.Periods.Keys "
    strTmp &= "HAVING ( "
    strTmp &= "Data.PeriodId IN ("

    strTmp &= "SELECT Id FROM Periods "
    If ListaFiltriValori.Count > 0 Then
      strTmp &= "WHERE "
      For Each Filtro In ListaFiltriValori
        strTmp &= "(" & Filtro.stringaWhere & ") "
        If Not Filtro Is ListaFiltriValori.Last Then
          strTmp &= "And "
        End If
      Next
    End If
    strTmp &= ")" 'IN
    If Not ListaFiltriAvg Is Nothing Then
      For Each Filtro In ListaFiltriAvg
        strTmp &= "And (" & Filtro.stringaWhere & ") "
      Next
    End If
    strTmp &= ") " 'Having
    strTmp &= "ORDER BY Periods.Start"
    Return strTmp
  End Function


  Public Function ListaPeriodiDb(StringaSql As String) As SqlDataReader
    Try
      If myConn Is Nothing Then
        ConnectToDb()
      ElseIf Not myConn.State = System.Data.ConnectionState.Open Then
        myConn.Open()
      End If

      Return SendRequestToDb(StringaSql)

      Dim sqlCmd As SqlCommand
      sqlCmd = New SqlCommand(StringaSql, myConn)
      Dim reader As SqlDataReader = sqlCmd.ExecuteReader()
      Return reader
    Catch ex As Exception
      MsgBox("Query Error")
      Return Nothing
    End Try

  End Function



  'Public Sub ListaPeriodiDb(reader As SqlDataReader, ListaCanali As List(Of String), Tipo As clsPeriod.ePeriodType)


  '  If ListaCanali Is Nothing Then Exit Sub
  '  RisultatoQuerySql.ListaPeriodi.Clear()
  '  RisultatoQuerySql.ListaPeriodiStraightLine.Clear()
  '  RisultatoQuerySql.ListaPeriodiTacks.Clear()
  '  RisultatoQuerySql.ListaPeriodiGybes.Clear()
  '  RisultatoQuerySql.ListaPeriodiAccelerations.Clear()

  '  RisultatoQuerySql.ListaControlliStraightLine.Clear()
  '  RisultatoQuerySql.ListaControlliTacks.Clear()
  '  RisultatoQuerySql.ListaControlliGybes.Clear()
  '  RisultatoQuerySql.ListaControlliAccelerations.Clear()
  '  'RisultatoQuerySql.Clear()
  '  Select Case Tipo
  '    Case clsPeriod.ePeriodType.eTack
  '      If pBoatConfig Then RisultatoQuerySql.AggiungiControlloTack("!competitor")
  '      RisultatoQuerySql.AggiungiControlloTack("!entrytack")
  '      RisultatoQuerySql.AggiungiControlloTack("!vmgtotalmeters")
  '      RisultatoQuerySql.AggiungiControlloTack("!@15")
  '      RisultatoQuerySql.AggiungiControlloTack("!@30")
  '      RisultatoQuerySql.AggiungiControlloTack("!vmgperc")
  '      'RisultatoQuerySql.AggiungiControlloTack("!vmgloss")
  '      'RisultatoQuerySql.AggiungiControlloTack("!inlineloss")
  '      RisultatoQuerySql.AggiungiControlloTack("!boarddown")
  '      RisultatoQuerySql.AggiungiControlloTack("!newcantdropangle")
  '      RisultatoQuerySql.AggiungiControlloTack("!newinnerflapangle")
  '      RisultatoQuerySql.AggiungiControlloTack("!newouterflapangle")
  '      RisultatoQuerySql.AggiungiControlloTack("!boarddropheel")
  '      RisultatoQuerySql.AggiungiControlloTack("!boarddroptrim")
  '      RisultatoQuerySql.AggiungiControlloTack("!rotationstart")
  '      RisultatoQuerySql.AggiungiControlloTack("!rotationduration")
  '      RisultatoQuerySql.AggiungiControlloTack("!minbs")
  '      RisultatoQuerySql.AggiungiControlloTack("!cogdelta")
  '      RisultatoQuerySql.AggiungiControlloTack("!yawrateavg")
  '      RisultatoQuerySql.AggiungiControlloTack("!rdrangavg")
  '      RisultatoQuerySql.AggiungiControlloTack("!rdrangmax")
  '      RisultatoQuerySql.AggiungiControlloTack("!lwymax")
  '      RisultatoQuerySql.AggiungiControlloTack("!minsinkavg")
  '      RisultatoQuerySql.AggiungiControlloTack("!travellerspeed")
  '      RisultatoQuerySql.AggiungiControlloTack("!twistspeed")
  '      RisultatoQuerySql.AggiungiControlloTack("!boardup")
  '      RisultatoQuerySql.AggiungiControlloTack("!entrybs")
  '      RisultatoQuerySql.AggiungiControlloTack("!entrytwa")
  '      RisultatoQuerySql.AggiungiControlloTack("!exitbs")
  '      RisultatoQuerySql.AggiungiControlloTack("!exittwa")
  '    Case clsPeriod.ePeriodType.eGybe
  '      If pBoatConfig Then RisultatoQuerySql.AggiungiControlloGybe("!competitor")
  '      RisultatoQuerySql.AggiungiControlloGybe("!entrytack")
  '      RisultatoQuerySql.AggiungiControlloGybe("!vmgtotalmeters")
  '      RisultatoQuerySql.AggiungiControlloGybe("!@15")
  '      RisultatoQuerySql.AggiungiControlloGybe("!@30")
  '      RisultatoQuerySql.AggiungiControlloGybe("!vmgperc")
  '      'RisultatoQuerySql.AggiungiControlloGybe("!vmgloss")
  '      'RisultatoQuerySql.AggiungiControlloGybe("!inlineloss")
  '      RisultatoQuerySql.AggiungiControlloGybe("!boarddown")
  '      RisultatoQuerySql.AggiungiControlloGybe("!newcantdropangle")
  '      RisultatoQuerySql.AggiungiControlloGybe("!newinnerflapangle")
  '      RisultatoQuerySql.AggiungiControlloGybe("!newouterflapangle")
  '      RisultatoQuerySql.AggiungiControlloGybe("!boarddropheel")
  '      RisultatoQuerySql.AggiungiControlloGybe("!boarddroptrim")
  '      RisultatoQuerySql.AggiungiControlloGybe("!rotationstart")
  '      RisultatoQuerySql.AggiungiControlloGybe("!rotationduration")
  '      RisultatoQuerySql.AggiungiControlloGybe("!minbs")
  '      RisultatoQuerySql.AggiungiControlloGybe("!cogdelta")
  '      RisultatoQuerySql.AggiungiControlloGybe("!yawrateavg")
  '      RisultatoQuerySql.AggiungiControlloGybe("!rdrangavg")
  '      RisultatoQuerySql.AggiungiControlloGybe("!rdrangmax")
  '      RisultatoQuerySql.AggiungiControlloGybe("!lwymax")
  '      RisultatoQuerySql.AggiungiControlloGybe("!minsinkavg")
  '      RisultatoQuerySql.AggiungiControlloGybe("!travellerspeed")
  '      RisultatoQuerySql.AggiungiControlloGybe("!twistspeed")
  '      RisultatoQuerySql.AggiungiControlloGybe("!boardup")
  '      RisultatoQuerySql.AggiungiControlloGybe("!entrybs")
  '      RisultatoQuerySql.AggiungiControlloGybe("!entrytwa")
  '      RisultatoQuerySql.AggiungiControlloGybe("!exitbs")
  '      RisultatoQuerySql.AggiungiControlloGybe("!exittwa")
  '    Case clsPeriod.ePeriodType.eAcceleration
  '      If pBoatConfig Then RisultatoQuerySql.AggiungiControlloAcceleration("!competitor")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!sectotakeoff")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!vmgmeters@takeoff")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!testduration")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!vmgmeters@exit")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!lateralmeters@exit")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!exitvmg")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!exitbs")
  '      RisultatoQuerySql.AggiungiControlloAcceleration("!exittwa")
  '    Case clsPeriod.ePeriodType.eStraightLine
  '      If pBoatConfig Then RisultatoQuerySql.AggiungiControlloStraightLine("!competitor")
  '      RisultatoQuerySql.AggiungiControlloStraightLine("!vmgmetersxmin")
  '      RisultatoQuerySql.AggiungiControlloStraightLine("!latmetersxmin")
  '      RisultatoQuerySql.AggiungiControlloStraightLine("!tack")
  '    Case Else
  '  End Select

  '  Dim IsPrimoPeriodo As Boolean = True

  '  While reader.Read
  '    Dim SqlId = reader.GetInt32(reader.GetOrdinal("SqlId"))
  '    Dim startDate = reader.GetDateTime(reader.GetOrdinal("StartTime"))
  '    Dim endDate = reader.GetDateTime(reader.GetOrdinal("EndTime"))
  '    Dim TimeRange As New clsTimeRange(startDate, endDate)
  '    Dim PeriodType = reader.GetInt32(reader.GetOrdinal("TypeId"))
  '    Dim KeyMoment As DateTime = Nothing
  '    If Not reader.IsDBNull(reader.GetOrdinal("KeyMoment")) Then
  '      KeyMoment = reader.GetDateTime(reader.GetOrdinal("KeyMoment"))
  '    End If
  '    Dim Keys As String = reader.GetString(reader.GetOrdinal("Keys"))
  '    Dim SourceFile As String = reader.GetString(reader.GetOrdinal("Source"))
  '    Dim ShortDescription As String = reader.GetString(reader.GetOrdinal("ShortDescription"))
  '    Dim ExtendedDescription As String = reader.GetString(reader.GetOrdinal("ExtendedDescription"))
  '    Dim NrCampioni As Integer = reader.GetInt32(reader.GetOrdinal("Samples"))
  '    Dim PeriodoTmp As New clsDbPeriod(SqlId, TimeRange, PeriodType, KeyMoment, Keys, ShortDescription, ExtendedDescription, NrCampioni, SourceFile)
  '    For Each Canale In ListaCanali
  '      Dim Avg As Double = Nothing
  '      Dim Max As Double = Nothing
  '      Dim Min As Double = Nothing
  '      Dim StdDev As Double = Nothing
  '      If (Not reader.IsDBNull(reader.GetOrdinal("AvgOf" & Canale & ""))) Then
  '        Avg = reader.GetDouble(reader.GetOrdinal("AvgOf" & Canale & ""))
  '      End If
  '      If (Not reader.IsDBNull(reader.GetOrdinal("MaxOf" & Canale & ""))) Then
  '        Max = reader.GetDouble(reader.GetOrdinal("MaxOf" & Canale & ""))
  '      End If
  '      If (Not reader.IsDBNull(reader.GetOrdinal("MinOf" & Canale & ""))) Then
  '        Min = reader.GetDouble(reader.GetOrdinal("MinOf" & Canale & ""))
  '      End If
  '      If (Not reader.IsDBNull(reader.GetOrdinal("StDevOf" & Canale & ""))) Then
  '        StdDev = reader.GetDouble(reader.GetOrdinal("StDevOf" & Canale & ""))
  '      End If
  '      Dim ChTmp As New clsValoriBaseCanale(Canale, Avg, Max, Min, StdDev, IsPrimoPeriodo)
  '      PeriodoTmp.ListaCanali.Add(ChTmp)
  '      If IsPrimoPeriodo Then
  '        If Not Canale Is ListaCanali.First Then
  '          If Canale Is Nothing Then Stop
  '          Select Case Tipo
  '            Case clsPeriod.ePeriodType.eStraightLine
  '              RisultatoQuerySql.AggiungiControlloStraightLine(Canale)
  '            Case clsPeriod.ePeriodType.eTack
  '              'RisultatoQuerySql.AggiungiControlloTack(Canale, True)
  '              'RisultatoQuerySql.AggiungiControlloTack(Canale, False)
  '            Case clsPeriod.ePeriodType.eGybe
  '              'RisultatoQuerySql.AggiungiControlloGybe(Canale, True)
  '              'RisultatoQuerySql.AggiungiControlloGybe(Canale, False)
  '            Case clsPeriod.ePeriodType.eAcceleration
  '              'RisultatoQuerySql.AggiungiControlloAcceleration(Canale)
  '            Case Else
  '          End Select
  '        End If
  '      End If
  '    Next
  '    If pBoatConfig Then
  '      Dim Competitor As Double
  '      If (Not reader.IsDBNull(reader.GetOrdinal("competitor"))) Then
  '        Competitor = reader.GetValue(reader.GetOrdinal("competitor"))
  '      End If
  '      Dim ChTmp As New clsValoriBaseCanale("competitor", Competitor, Competitor, Competitor, 0, IsPrimoPeriodo)
  '      PeriodoTmp.ListaCanali.Add(ChTmp)
  '    End If
  '    PeriodoTmp.AggiornaTimeRangeDescription()
  '    RisultatoQuerySql.ListaPeriodi.Add(PeriodoTmp)
  '    Select Case PeriodType
  '      Case clsPeriod.ePeriodType.eAcceleration
  '        RisultatoQuerySql.ListaPeriodiAccelerations.Add(PeriodoTmp)
  '      Case clsPeriod.ePeriodType.eGybe
  '        RisultatoQuerySql.ListaPeriodiGybes.Add(PeriodoTmp)
  '      Case clsPeriod.ePeriodType.eTack
  '        RisultatoQuerySql.ListaPeriodiTacks.Add(PeriodoTmp)
  '      Case clsPeriod.ePeriodType.eStraightLine
  '        RisultatoQuerySql.ListaPeriodiStraightLine.Add(PeriodoTmp)
  '    End Select
  '    'RisultatoQuerySql.Add(PeriodoTmp)
  '    IsPrimoPeriodo = False
  '  End While
  '  For i As Integer = 0 To RisultatoQuerySql.ListaPeriodi.Count - 1
  '    RisultatoQuerySql.ListaPeriodi(i).Colore = ColoriDifferenziati(i)
  '    'RisultatoQuerySql.ListaPeriodi(i).Colore = ColoreScuroInScala((i + 1) / RisultatoQuerySql.ListaPeriodi.Count * 256)
  '  Next

  '  reader.Close()
  '  myConn.Close()


  '  RisultatoQuerySql.ListaControlliStraightLineSync.ListaPeriodiDb = RisultatoQuerySql.ListaPeriodiStraightLine
  '  RisultatoQuerySql.ListaControlliTacksSync.ListaPeriodiDb = RisultatoQuerySql.ListaPeriodiTacks
  '  RisultatoQuerySql.ListaControlliGybesSync.ListaPeriodiDb = RisultatoQuerySql.ListaPeriodiGybes
  '  RisultatoQuerySql.ListaControlliAccelerationsSync.ListaPeriodiDb = RisultatoQuerySql.ListaPeriodiAccelerations


  '  Select Case Tipo
  '    Case clsPeriod.ePeriodType.eTack
  '      If RisultatoQuerySql.ListaPeriodiTacks.Count < 50 Then
  '        RisultatoQuerySql.RicalcolaTacks(Me)
  '      End If
  '    Case clsPeriod.ePeriodType.eGybe
  '      If RisultatoQuerySql.ListaPeriodiGybes.Count < 50 Then
  '        RisultatoQuerySql.RicalcolaGybes(Me)
  '      End If
  '    Case clsPeriod.ePeriodType.eStraightLine
  '      RisultatoQuerySql.AggiornaGraficiStraightLine()
  '    Case clsPeriod.ePeriodType.eAcceleration
  '      If RisultatoQuerySql.ListaPeriodiAccelerations.Count < 50 Then
  '        Dim BSlimit As Double = AppConfig.CercaValoreInnerText("SqlDbSettings", clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "BSlimit", 10, True, False)
  '        Dim TakeOffSpeed As Double = AppConfig.CercaValoreInnerText("SqlDbSettings", clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TakeOffSpeed", 14.5, True, False)
  '        Dim TestDuration As Double = AppConfig.CercaValoreInnerText("SqlDbSettings", clsSettings.eNodoSTD.eStartUp, "AccelerationSettings", "TestDuration", 60, True, True)
  '        RisultatoQuerySql.RicalcolaAccelerations(Me, BSlimit, TakeOffSpeed, TestDuration)
  '      End If
  '  End Select




  '  'RisultatoQuerySql.AggiornaGraficiTacks()
  '  'RisultatoQuerySql.AggiornaGraficiGybes()

  'End Sub

End Class
