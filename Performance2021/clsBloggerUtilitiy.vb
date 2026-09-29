
Public Class clsBloggerUtility

  Private Sub ImportaDaBloggerCommentsOnly()
    'Dim AggiungiPeriodi As Boolean = False
    Dim ssOffset As Double = 18.5
    Dim PathFileCommenti As String
    Dim PathFileTest As String
    For Each Fl In DataProvider2020.Files.First.Directory.GetFiles
      'If Fl.Name.StartsWith("Book1") Then
      If Fl.Name.StartsWith("comments") Then
        PathFileCommenti = Fl.FullName
      End If
      If Fl.Name.StartsWith("Test_") Then
        PathFileTest = Fl.FullName
      End If
    Next
    Dim ListaTest As List(Of String) = Nothing
    If System.IO.File.Exists(PathFileTest) Then
      ListaTest = System.IO.File.ReadAllLines(PathFileTest).ToList
    End If
    Dim BloggerTestsCheck As New List(Of clsBloggerTest)
    If Not ListaTest Is Nothing Then
      For Each Test In ListaTest
        If Not Test.StartsWith("DBNameDate") Then
          BloggerTestsCheck.Add(New clsBloggerTest(Test))
        End If
      Next
    End If

    Dim ListaCommenti As List(Of String) = Nothing
    If System.IO.File.Exists(PathFileCommenti) Then
      ListaCommenti = System.IO.File.ReadAllLines(PathFileCommenti).ToList
    End If
    Dim BloggerTests As New List(Of clsBloggerTest)
    Dim BloggerComments As New List(Of clsBloggerCommento)

    If Not ListaCommenti Is Nothing Then
      Dim TR As New clsTimeRange
      For Each comm In ListaCommenti
        Dim Commento As New clsBloggerCommento(comm)
        If Commento.Commento.StartsWith("Test_start") Then
          TR.Start = Commento.Momento
        ElseIf Commento.Commento.StartsWith("Test_stop") Then
          TR.Finish = Commento.Momento
          Dim Test As New clsBloggerTest(TR.Clone)
          Test.VerificaCommenti(BloggerComments)
          BloggerTests.Add(Test)
        Else
          BloggerComments.Add(Commento)
        End If
      Next
      'Dim MinDel As Double = 999
      'For Each bT In BloggerTests
      '  If dataProvider2020.TimeRange.IsFullyOverlapped(bT.TR) Then
      '    For Each Tc In BloggerTestsCheck
      '      If dataProvider2020.TimeRange.IsFullyOverlapped(Tc.TR) Then
      '        MinDel = System.Math.Min(MinDel, Tc.TR.Inizio.Subtract(bT.TR.Inizio).TotalSeconds)
      '      End If
      '    Next
      '  End If
      'Next
      'If Not MinDel = 999 Then ssOffset = MinDel

      For Each Btest In BloggerTests
        Dim NuovoPeriodo As New clsPeriod2020(Btest.TR.Clone, clsPeriod2020.ePeriodType.eUndefined, PeriodsManager.IndiceNuovoPeriodo)
        If Not ssOffset = 0 Then
          NuovoPeriodo.TimeRange.Start = NuovoPeriodo.TimeRange.Start.AddSeconds(ssOffset)
          NuovoPeriodo.TimeRange.Finish = NuovoPeriodo.TimeRange.Finish.AddSeconds(ssOffset)
        End If
        If Btest.Commenti.Count > 0 Then
          NuovoPeriodo.ShortDescription = Btest.Commenti.First.Commento
          NuovoPeriodo.ExtendedDescription = Btest.StringCommenti
        End If
        If DataProvider2020.TimeRange.IsFullyOverlapped(NuovoPeriodo.TimeRange) Then
          Dim Aggiungi As Boolean = True
          For Each periodo In PeriodsManager.ListaPeriodi
            ' vengono aggiunti solo i periodi nell intervallo del file e che già non esistono
            If periodo.TimeRange.IsSameRange(NuovoPeriodo.TimeRange) Then
              Aggiungi = False
              Exit For
            End If
          Next
          If Aggiungi Then PeriodsManager.AggiungiNuovoPeriodo(NuovoPeriodo)
        End If
      Next
      'GraficoEventiViewModel.AggiornaPeriodi()

      Dim testoTmp As String = ""
      For Each Tst In BloggerTests
        testoTmp &= Tst.TR.StringaPeriodo
        If Tst.Commenti.Count > 0 Then
          testoTmp &= vbTab & Tst.Commenti.First.Commento & vbCrLf
          For Each Commento In Tst.Commenti
            testoTmp &= "" & vbTab & Commento.Momento.ToLongTimeString & " : " & Commento.Commento & " (" & Commento.Source & ")" & vbCrLf
          Next
        End If
        testoTmp &= vbCrLf
      Next
      Clipboard.SetText(testoTmp)
      testoTmp = ""
      For Each Test In BloggerTestsCheck
        testoTmp &= Test.TR.StringaPeriodo & vbCrLf
      Next
      Clipboard.SetText(testoTmp)
      testoTmp = ""
      For Each commento In BloggerComments
        testoTmp &= commento.Momento.ToLongTimeString & vbTab & commento.Commento & vbTab & commento.Source & vbCrLf
      Next
      Clipboard.SetText(testoTmp)
    End If

  End Sub


  Private Sub ImportaDaBlogger()
    Dim PathFileTest As String
    Dim PathFileCommenti As String
    For Each Fl In DataProvider2020.Files.First.Directory.GetFiles
      If Fl.Name.StartsWith("Test_") Then
        PathFileTest = Fl.FullName
      End If
      If Fl.Name.StartsWith("comments") Then
        PathFileCommenti = Fl.FullName
      End If
    Next
    If System.IO.File.Exists(PathFileTest) Then
      Dim ListTest As List(Of String) = System.IO.File.ReadAllLines(PathFileTest).ToList
      Dim ListaCommenti As List(Of String) = Nothing
      If System.IO.File.Exists(PathFileCommenti) Then
        ListaCommenti = System.IO.File.ReadAllLines(PathFileCommenti).ToList
      End If
      Dim BloggerTests As New List(Of clsBloggerTest)
      Dim BloggerComments As New List(Of clsBloggerCommento)
      If Not ListaCommenti Is Nothing Then
        For Each comm In ListaCommenti
          BloggerComments.Add(New clsBloggerCommento(comm))
        Next
      End If
      If Not ListTest Is Nothing Then
        For Each tst In ListTest
          If Not tst.StartsWith("DBNameDate") Then
            BloggerTests.Add(New clsBloggerTest(tst))
            BloggerTests.Last.VerificaCommenti(BloggerComments)
            Dim NuovoPeriodo As New clsPeriod2020(BloggerTests.Last.TR.Clone, clsPeriod2020.ePeriodType.eUndefined, PeriodsManager.IndiceNuovoPeriodo)
            If BloggerTests.Last.Commenti.Count > 0 Then
              NuovoPeriodo.ShortDescription = BloggerTests.Last.Commenti.First.Commento
              NuovoPeriodo.ExtendedDescription = BloggerTests.Last.StringCommenti
            End If
            PeriodsManager.AggiungiNuovoPeriodo(NuovoPeriodo)
          End If
        Next
        'GraficoEventiViewModel.AggiornaPeriodi()
        Dim testoTmp As String = ""
        For Each Tst In BloggerTests
          testoTmp &= Tst.TR.StringaPeriodo
          If Tst.Commenti.Count > 0 Then
            testoTmp &= vbTab & Tst.Commenti.First.Commento & vbCrLf
            For Each Commento In Tst.Commenti
              testoTmp &= "" & vbTab & Tst.Commenti.First.Momento.ToLongTimeString & " : " & Tst.Commenti.First.Commento & " (" & Tst.Commenti.First.Source & ")" & vbCrLf
            Next
          End If
          testoTmp &= vbCrLf
        Next
        Clipboard.SetText(testoTmp)
        testoTmp = ""
        For Each commento In BloggerComments
          testoTmp &= commento.Momento.ToLongTimeString & vbTab & commento.Commento & vbTab & commento.Source & vbCrLf
        Next
        Clipboard.SetText(testoTmp)
      End If

    End If


  End Sub


End Class

Public Class clsBloggerTest
  Dim pTR As clsTimeRange
  Dim pCommenti As New List(Of clsBloggerCommento)

  Public Sub New(TestoRiga As String)
    Dim Valori As String() = TestoRiga.Split(",")
    Dim Giorno As New DateTime(Valori(0).Substring(0, 4), Valori(0).Substring(4, 2), Valori(0).Substring(6, 2), 0, 0, 0)
    pTR = New clsTimeRange(Giorno.AddSeconds(CDbl(Valori(2))), Giorno.AddSeconds(CDbl(Valori(3))))
  End Sub

  Public Sub New(TR As clsTimeRange)
    pTR = TR
  End Sub

  Public Property TR As clsTimeRange
    Get
      Return pTR
    End Get
    Set(value As clsTimeRange)
      pTR = value
    End Set
  End Property

  Public Property Commenti As List(Of clsBloggerCommento)
    Get
      Return pCommenti
    End Get
    Set(value As List(Of clsBloggerCommento))
      pCommenti = value
    End Set
  End Property

  Public ReadOnly Property StringCommenti As String
    Get
      Dim strTmp As String = ""
      For Each commento In pCommenti
        strTmp = commento.Momento.ToLongTimeString & " : " & commento.Commento & " (" & commento.Source & ")" & vbCrLf
      Next
      Return strTmp.TrimEnd(vbCrLf)
    End Get
  End Property


  Public Sub VerificaCommento(Commento As clsBloggerCommento)
    If pTR.IsInRange(Commento.Momento, True, True) Then
      pCommenti.Add(Commento)
    End If
  End Sub

  Public Sub VerificaCommenti(Commenti As List(Of clsBloggerCommento))
    If Commenti.Count = 0 Then Exit Sub
    For Each Commento In Commenti
      If Not Commento Is Commenti.First Then
        If pTR.IsInRange(Commento.Momento, True, True) Then
          pCommenti.Add(Commento)
        End If
      End If
    Next
  End Sub

End Class

Public Class clsBloggerCommento
  Dim pMomento As DateTime
  Dim pSource As String
  Dim pCommento As String

  Public Sub New(TestoRiga As String)
    Dim Valori As String() = TestoRiga.Split(",")
    Dim Tm As String = If(Valori(1).IndexOf(".") > 0, Valori(1).Split(".")(0), Valori(0))
    Dim Ora As Integer = Tm.Substring(0, If(Tm.Length = 5, 1, 2))
    Dim Mn As Integer = Tm.Substring(If(Tm.Length = 5, 1, 2), 2)
    Dim Ss As Integer = Tm.Substring(If(Tm.Length = 5, 3, 4), 2)
    'Dim Giorno As New DateTime(Valori(0).Substring(0, 4), Valori(0).Substring(4, 2), Valori(0).Substring(6, 2), Ora, min, Ss)
    pMomento = New DateTime(Valori(0).Substring(0, 4), Valori(0).Substring(4, 2), Valori(0).Substring(6, 2), Ora, Mn, Ss)
    'pMomento = Giorno.AddSeconds(CDbl(Valori(1))).AddHours(-12)
    pSource = Valori(2).Replace("""", "").Replace("src", "").Replace(":", "").Replace("{", "")
    pCommento = Valori(3).Replace("""", "").Replace("msg", "").Replace(":", "").Replace("}", "")
  End Sub

  Public Property Momento As DateTime
    Get
      Return pMomento
    End Get
    Set(value As Date)
      pMomento = value
    End Set
  End Property

  Public Property Commento As String
    Get
      Return pCommento
    End Get
    Set(value As String)
      pCommento = value
    End Set
  End Property

  Public Property Source As String
    Get
      Return pSource
    End Get
    Set(value As String)
      pSource = value
    End Set
  End Property
End Class
