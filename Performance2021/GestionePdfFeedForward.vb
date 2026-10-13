Imports PdfSharp
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

''' <summary>Tabella del PDF feed forward: intestazioni (con eventuali gruppi sopra), righe di testo e note sotto il titolo.</summary>
Public Class clsFfPdfTabella
  Public Property Titolo As String = ""
  ''' <summary>Righe della tabella: un array di celle per riga.</summary>
  Public Property Righe As New List(Of String())
  Public Property Intestazioni As New List(Of String)
  ''' <summary>Gruppi di colonne sopra le intestazioni: testo e numero di colonne (la somma deve essere il numero di colonne). Vuota = nessun gruppo.</summary>
  Public Property Gruppi As New List(Of KeyValuePair(Of String, Integer))
  ''' <summary>Larghezze relative delle colonne; vuota = la prima il doppio delle altre.</summary>
  Public Property Larghezze As New List(Of Double)
  ''' <summary>Note sotto la tabella (legenda, asterischi).</summary>
  Public Property Note As New List(Of String)
  ''' <summary>Numero di righe iniziali (dopo l'intestazione) stampate in grassetto.</summary>
  Public Property RigheEvidenza As Integer = 0
  ''' <summary>Celle da stampare in grigio ("riga,colonna", indici da 0 sul corpo della tabella).</summary>
  Public Property CelleGrigie As New HashSet(Of String)
  ''' <summary>Celle da stampare in grassetto ("riga,colonna", come CelleGrigie).</summary>
  Public Property CelleGrassetto As New HashSet(Of String)
  ''' <summary>Sfondo di singole celle ("riga,colonna" come CelleGrigie): colore ARGB.</summary>
  Public Property Sfondi As New Dictionary(Of String, Integer)
  ''' <summary>Celle con il bordo marcato ("riga,colonna"), colore BordoColore.</summary>
  Public Property CelleBordo As New HashSet(Of String)
  Public Property BordoColore As Integer = &HFF2A7F72
End Class

''' <summary>Una sezione del PDF feed forward: un canale Y con le sue tabelle e i suoi grafici (uno per fascia di vento).</summary>
Public Class clsFfPdfSezione
  Public Property Titolo As String = ""
  ''' <summary>Righe di testo sotto il titolo (filtro, bin, modello...).</summary>
  Public Property Righe As New List(Of String)
  Public Property Tabelle As New List(Of clsFfPdfTabella)
  ''' <summary>Immagini dei grafici, nell'ordine di stampa.</summary>
  Public Property Immagini As New List(Of System.IO.Stream)
End Class

''' <summary>
''' Impaginazione del PDF del report feed forward: A4 orizzontale; prima la tabella riepilogativa per fascia di vento, poi una sezione per canale Y
''' (tabelle e griglia di RighePagina x ColonnePagina grafici per pagina). Intestazione ripetuta e numero di pagina su ogni pagina.
''' </summary>
Public Class clsFfPdfPagina
  Public Property Titolo As String = ""
  Public Property Righe As New List(Of String)
  Public Property Immagine As System.IO.Stream
  Public Property Tabella As clsFfPdfTabella
  ''' <summary>Seconda tabella opzionale sotto la prima (per esempio la sensitivita' nel riepilogo), con il suo titolo.</summary>
  Public Property Tabella2 As clsFfPdfTabella
  Public Property TitoloTabella2 As String = ""
End Class

Public Class clsPdfFeedForward

  Private Const Margine As Double = 24
  Private Const Spazio As Double = 6
  Private Const AltezzaPiede As Double = 16
  Private Const AltezzaRiga As Double = 12

  Private Shared ReadOnly FTit As New XFont("Verdana", 14, XFontStyle.Bold)
  Private Shared ReadOnly FSez As New XFont("Verdana", 11, XFontStyle.Bold)
  Private Shared ReadOnly FTesto As New XFont("Verdana", 8)
  ''' <summary>Note delle pagine verticali: carattere piccolo, in un riquadro in fondo alla pagina staccato dalla tabella.</summary>
  Private Shared ReadOnly FNote As New XFont("Verdana", 6.5)
  Private Const AltezzaRigaNote As Double = 9
  Private Const PaddingNote As Double = 5
  Private Const DistaccoNote As Double = 16
  Private Const DistaccoTabelle As Double = 18
  Private Const AltezzaTitoloTabella As Double = 16
  Private Shared ReadOnly FTab As New XFont("Verdana", 7.5)
  Private Shared ReadOnly FTabB As New XFont("Verdana", 7.5, XFontStyle.Bold)
  Private Shared ReadOnly FPiede As New XFont("Verdana", 7)
  Private Shared ReadOnly FPiedeB As New XFont("Verdana", 8, XFontStyle.Bold)
  Private Shared ReadOnly FTitPagina As New XFont("Verdana", 13, XFontStyle.Bold)

  ''' <summary>Crea il file PDF. False se non c'e' nulla da stampare.</summary>
  Public Shared Function Crea(Percorso As String, TitoloReport As String, Riepilogo As clsFfPdfTabella, Sezioni As List(Of clsFfPdfSezione),
                              RighePagina As Integer, ColonnePagina As Integer) As Boolean
    Dim ConRiepilogo As Boolean = Riepilogo IsNot Nothing AndAlso Riepilogo.Righe.Count > 0
    If Not ConRiepilogo AndAlso (Sezioni Is Nothing OrElse Sezioni.All(Function(s) s.Immagini.Count = 0 AndAlso s.Tabelle.All(Function(t) t.Righe.Count = 0))) Then Return False
    RighePagina = System.Math.Max(1, RighePagina)
    ColonnePagina = System.Math.Max(1, ColonnePagina)

    Dim Doc As New PdfDocument
    If ConRiepilogo Then DisegnaTabella(Doc, TitoloReport, "Summary by band", Riepilogo)

    For Each Sez As clsFfPdfSezione In Sezioni
      For Each T As clsFfPdfTabella In Sez.Tabelle
        DisegnaTabella(Doc, TitoloReport, Sez.Titolo, T)
      Next
      If Sez.Immagini.Count > 0 Then DisegnaGrafici(Doc, TitoloReport, Sez, RighePagina, ColonnePagina)
    Next

    NumeraPagine(Doc)

    Doc.Save(Percorso)
    Return True
  End Function

  Private Shared Function NuovaPagina(Doc As PdfDocument, Optional Verticale As Boolean = False) As PdfPage
    Dim P As PdfPage = Doc.AddPage()
    P.Size = PageSize.A4
    P.Orientation = If(Verticale, PageOrientation.Portrait, PageOrientation.Landscape)
    Return P
  End Function

  ''' <summary>Divide il testo in righe che stanno nella larghezza L (a capo tra le parole; una parola troppo lunga viene troncata).</summary>
  Private Shared Function ACapo(Gfx As XGraphics, Testo As String, F As XFont, L As Double) As List(Of String)
    Dim Righe As New List(Of String)
    If String.IsNullOrEmpty(Testo) Then
      Righe.Add("")
      Return Righe
    End If
    Dim Riga As String = ""
    For Each Parola As String In Testo.Split(" "c)
      Dim Prova As String = If(Riga = "", Parola, Riga & " " & Parola)
      If Riga <> "" AndAlso Gfx.MeasureString(Prova, F).Width > L Then
        Righe.Add(Tronca(Gfx, Riga, F, L))
        Riga = Parola
      Else
        Riga = Prova
      End If
    Next
    If Riga <> "" Then Righe.Add(Tronca(Gfx, Riga, F, L))
    Return Righe
  End Function

  Private Shared Function Tronca(Gfx As XGraphics, Testo As String, F As XFont, L As Double) As String
    If String.IsNullOrEmpty(Testo) OrElse Gfx.MeasureString(Testo, F).Width <= L Then Return Testo
    Dim T As String = Testo
    While T.Length > 1 AndAlso Gfx.MeasureString(T & "..", F).Width > L
      T = T.Substring(0, T.Length - 1)
    End While
    Return T & ".."
  End Function

  ' ---------------------------------------------------------------- tabelle

  Private Shared Sub DisegnaTabella(Doc As PdfDocument, TitoloReport As String, SottoTitolo As String, T As clsFfPdfTabella)
    If T.Righe.Count = 0 OrElse T.Intestazioni.Count = 0 Then Exit Sub
    Dim n As Integer = T.Intestazioni.Count
    Dim Pagina As PdfPage = Nothing
    Dim Gfx As XGraphics = Nothing
    Dim Y As Double = 0
    Dim Largh() As Double = Nothing
    Dim NumPag As Integer = 0

    For r As Integer = 0 To T.Righe.Count - 1
      Dim Limite As Double = If(Pagina Is Nothing, 0, Pagina.Height.Value - Margine * 0.5 - AltezzaPiede - 4)
      If Gfx Is Nothing OrElse Y + AltezzaRiga > Limite Then
        If Gfx IsNot Nothing Then Gfx.Dispose()
        Pagina = NuovaPagina(Doc)
        Gfx = XGraphics.FromPdfPage(Pagina)
        NumPag += 1
        Dim W As Double = Pagina.Width.Value - 2 * Margine
        Gfx.DrawString(TitoloReport, FTit, XBrushes.Black, New XRect(Margine, 8, W, 20), XStringFormats.CenterLeft)
        Gfx.DrawString(SottoTitolo & If(T.Titolo <> "", "  -  " & T.Titolo, "") & If(NumPag > 1, " (cont.)", ""), FSez, XBrushes.DarkBlue, New XRect(Margine, 28, W, 16), XStringFormats.CenterLeft)
        Y = 46
        If NumPag = 1 Then
          For Each Nota As String In T.Note.Where(Function(q) q.StartsWith("#"))
            Gfx.DrawString(Nota.Substring(1), FTesto, XBrushes.DimGray, New XRect(Margine, Y, W, 11), XStringFormats.CenterLeft)
            Y += 11
          Next
          Y += 4
        End If

        ' larghezze delle colonne
        ReDim Largh(n - 1)
        Dim Rel(n - 1) As Double
        For c As Integer = 0 To n - 1
          Rel(c) = If(T.Larghezze.Count = n, T.Larghezze(c), If(c = 0, 2.0, 1.0))
        Next
        Dim Tot As Double = Rel.Sum
        For c As Integer = 0 To n - 1
          Largh(c) = W * Rel(c) / Tot
        Next

        ' gruppi e intestazioni
        If T.Gruppi.Count > 0 Then
          Dim Xg As Double = Margine
          Dim Col As Integer = 0
          For Each G As KeyValuePair(Of String, Integer) In T.Gruppi
            Dim Lg As Double = 0
            For k As Integer = Col To System.Math.Min(n - 1, Col + G.Value - 1)
              Lg += Largh(k)
            Next
            Gfx.DrawRectangle(New XPen(XColors.Gray, 0.4), New XSolidBrush(XColor.FromArgb(214, 224, 238)), Xg, Y, Lg, AltezzaRiga)
            Gfx.DrawString(Tronca(Gfx, G.Key, FTabB, Lg - 3), FTabB, XBrushes.Black, New XRect(Xg, Y, Lg, AltezzaRiga), XStringFormats.Center)
            Xg += Lg
            Col += G.Value
          Next
          Y += AltezzaRiga
        End If
        Dim X As Double = Margine
        For c As Integer = 0 To n - 1
          Gfx.DrawRectangle(New XPen(XColors.Gray, 0.4), New XSolidBrush(XColor.FromArgb(232, 232, 232)), X, Y, Largh(c), AltezzaRiga)
          Gfx.DrawString(Tronca(Gfx, T.Intestazioni(c), FTabB, Largh(c) - 3), FTabB, XBrushes.Black, New XRect(X, Y, Largh(c), AltezzaRiga), XStringFormats.Center)
          X += Largh(c)
        Next
        Y += AltezzaRiga
      End If

      Dim Riga() As String = T.Righe(r)
      Dim Xr As Double = Margine
      If r Mod 2 = 1 Then Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(246, 246, 246)), Xr, Y, Largh.Sum, AltezzaRiga)
      For c As Integer = 0 To n - 1
        Dim Testo As String = If(c < Riga.Length, Riga(c), "")
        Dim Fmt As XStringFormat = If(c = 0, XStringFormats.CenterLeft, XStringFormats.Center)
        Gfx.DrawString(Tronca(Gfx, Testo, FTab, Largh(c) - 3), FTab, XBrushes.Black, New XRect(Xr + If(c = 0, 3, 0), Y, Largh(c), AltezzaRiga), Fmt)
        Xr += Largh(c)
      Next
      Gfx.DrawLine(New XPen(XColors.LightGray, 0.3), Margine, Y + AltezzaRiga, Margine + Largh.Sum, Y + AltezzaRiga)
      Y += AltezzaRiga
    Next

    ' note sotto la tabella (quelle senza # sono la legenda)
    If Gfx IsNot Nothing Then
      Y += 4
      For Each Nota As String In T.Note.Where(Function(q) Not q.StartsWith("#"))
        If Y + 11 > Pagina.Height.Value - Margine * 0.5 - AltezzaPiede - 4 Then Exit For
        Gfx.DrawString(Nota, FTesto, XBrushes.DimGray, New XRect(Margine, Y, Pagina.Width.Value - 2 * Margine, 11), XStringFormats.CenterLeft)
        Y += 11
      Next
      Gfx.Dispose()
    End If
  End Sub

  ' ---------------------------------------------------------------- grafici

  Private Shared Sub DisegnaGrafici(Doc As PdfDocument, TitoloReport As String, Sez As clsFfPdfSezione, RighePagina As Integer, ColonnePagina As Integer)
    Dim Pagina As PdfPage = Nothing
    Dim Gfx As XGraphics = Nothing
    Dim AreaTop As Double = 0
    Dim Cella As Integer = 0
    Dim PerPagina As Integer = RighePagina * ColonnePagina
    Dim NumPagina As Integer = 0

    For k As Integer = 0 To Sez.Immagini.Count - 1
      If Cella = 0 Then
        If Gfx IsNot Nothing Then Gfx.Dispose() ' PdfSharp: un solo XGraphics per pagina alla volta
        Pagina = NuovaPagina(Doc)
        Gfx = XGraphics.FromPdfPage(Pagina)
        NumPagina += 1
        Gfx.DrawString(TitoloReport, FTit, XBrushes.Black, New XRect(Margine, 8, Pagina.Width.Value - 2 * Margine, 20), XStringFormats.CenterLeft)
        Gfx.DrawString(Sez.Titolo & " - charts" & If(NumPagina > 1, " (cont.)", ""), FSez, XBrushes.DarkBlue, New XRect(Margine, 28, Pagina.Width.Value - 2 * Margine, 16), XStringFormats.CenterLeft)
        Dim Y As Double = 44
        If NumPagina = 1 Then
          For Each R As String In Sez.Righe
            Gfx.DrawString(R, FTesto, XBrushes.DimGray, New XRect(Margine, Y, Pagina.Width.Value - 2 * Margine, 11), XStringFormats.CenterLeft)
            Y += 11
          Next
        End If
        AreaTop = Y + 4
      End If

      Dim L As Double = (Pagina.Width.Value - 2 * Margine - Spazio * (ColonnePagina - 1)) / ColonnePagina
      Dim H As Double = (Pagina.Height.Value - AreaTop - AltezzaPiede - Margine * 0.5 - Spazio * (RighePagina - 1)) / RighePagina
      Dim Col As Integer = Cella Mod ColonnePagina
      Dim Rig As Integer = Cella \ ColonnePagina
      Dim X As Double = Margine + Col * (L + Spazio)
      Dim Yc As Double = AreaTop + Rig * (H + Spazio)
      Try
        Dim Flusso As System.IO.Stream = Sez.Immagini(k)
        If Flusso.CanSeek Then Flusso.Position = 0
        Dim Img As XImage = XImage.FromStream(Flusso)
        ' si adatta alla cella mantenendo le proporzioni
        Dim Sc As Double = System.Math.Min(L / Img.PointWidth, H / Img.PointHeight)
        Dim W As Double = Img.PointWidth * Sc
        Dim Hh As Double = Img.PointHeight * Sc
        Gfx.DrawImage(Img, New XRect(X + (L - W) / 2, Yc + (H - Hh) / 2, W, Hh))
      Catch ex As Exception
        Gfx.DrawRectangle(New XPen(XColors.LightGray, 0.5), X, Yc, L, H)
        Gfx.DrawString("chart not available", FTesto, XBrushes.Gray, New XRect(X, Yc, L, H), XStringFormats.Center)
      End Try

      Cella += 1
      If Cella >= PerPagina Then Cella = 0
    Next
    If Gfx IsNot Nothing Then Gfx.Dispose()
  End Sub

  ' ---------------------------------------------------------------- una pagina verticale per canale: titolo, grafico, note, tabella

  ''' <summary>Crea il file PDF con una pagina A4 verticale per ogni elemento: titolo centrato, grafico, note e tabella. False se non c'e' nulla da stampare.</summary>
  Public Shared Function CreaPagine(Percorso As String, TitoloReport As String, Pagine As List(Of clsFfPdfPagina)) As Boolean
    If Pagine Is Nothing OrElse Pagine.Count = 0 Then Return False
    Dim Doc As New PdfDocument
    For Each Pg As clsFfPdfPagina In Pagine
      Dim Pagina As PdfPage = NuovaPagina(Doc, True)
      Dim Gfx As XGraphics = XGraphics.FromPdfPage(Pagina)
      Dim W As Double = Pagina.Width.Value - 2 * Margine
      ' titolo su una sola riga al centro, poi un po' di spazio prima del grafico
      Gfx.DrawString(Tronca(Gfx, Pg.Titolo, FTitPagina, W), FTitPagina, XBrushes.Black, New XRect(Margine, 26, W, 22), XStringFormats.Center)
      Dim Y As Double = 62

      ' note (legenda della tabella, sorgente, filtro, impostazioni) in un riquadro con cornice, largo come la tabella, in fondo alla pagina
      Dim T As clsFfPdfTabella = Pg.Tabella
      Dim T2 As clsFfPdfTabella = Pg.Tabella2
      Dim RigheNote As New List(Of String)
      For Each Tx As clsFfPdfTabella In {T, T2}
        If Tx Is Nothing Then Continue For
        For Each Nota As String In Tx.Note.Where(Function(q) Not q.StartsWith("#"))
          RigheNote.AddRange(ACapo(Gfx, Nota, FNote, W - 2 * PaddingNote))
        Next
      Next
      For Each R As String In Pg.Righe
        RigheNote.AddRange(ACapo(Gfx, R, FNote, W - 2 * PaddingNote))
      Next
      Dim Fondo As Double = Pagina.Height.Value - Margine * 0.5 - AltezzaPiede - 4
      Dim HNote As Double = If(RigheNote.Count = 0, 0, RigheNote.Count * AltezzaRigaNote + 2 * PaddingNote)
      Dim YNote As Double = Fondo - HNote
      Dim Ht As Double = AltezzaTabella(Gfx, W, T) ' righe piu' alte dove il testo va su due righe
      If T2 IsNot Nothing Then Ht += DistaccoTabelle + AltezzaTitoloTabella + AltezzaTabella(Gfx, W, T2)
      ' il grafico prende lo spazio che resta tra il titolo e la tabella, che resta staccata dal riquadro delle note
      Dim HGraf As Double = YNote - DistaccoNote - Ht - Spazio - Y

      If Pg.Immagine IsNot Nothing AndAlso HGraf > 40 Then
        Try
          If Pg.Immagine.CanSeek Then Pg.Immagine.Position = 0
          Dim Img As XImage = XImage.FromStream(Pg.Immagine)
          Dim Sc As Double = System.Math.Min(W / Img.PointWidth, HGraf / Img.PointHeight)
          Dim Wi As Double = Img.PointWidth * Sc
          Dim Hi As Double = Img.PointHeight * Sc
          Gfx.DrawImage(Img, New XRect(Margine + (W - Wi) / 2, Y, Wi, Hi))
          Y += Hi + Spazio
        Catch ex As Exception
          Gfx.DrawString("chart not available", FTesto, XBrushes.Gray, New XRect(Margine, Y, W, 20), XStringFormats.Center)
          Y += 24
        End Try
      End If

      If T IsNot Nothing Then Y = DisegnaTabellaSuPagina(Gfx, Y, W, YNote - DistaccoNote, T)
      If T2 IsNot Nothing Then
        Y += DistaccoTabelle
        Gfx.DrawString(Tronca(Gfx, Pg.TitoloTabella2, FSez, W), FSez, XBrushes.DarkBlue, New XRect(Margine, Y, W, AltezzaTitoloTabella), XStringFormats.CenterLeft)
        Y += AltezzaTitoloTabella
        Y = DisegnaTabellaSuPagina(Gfx, Y, W, YNote - DistaccoNote, T2)
      End If
      If RigheNote.Count > 0 Then
        ' se la pagina e' piena il riquadro non sale sopra la tabella: le righe che non entrano si perdono
        Dim YBox As Double = Math.Max(YNote, Y + DistaccoNote)
        Dim HBox As Double = Math.Min(HNote, Fondo - YBox)
        Gfx.DrawRectangle(New XPen(XColors.Gray, 0.7), New XSolidBrush(XColor.FromArgb(250, 250, 252)), Margine, YBox, W, HBox)
        Dim Yr As Double = YBox + PaddingNote
        For Each R As String In RigheNote
          If Yr + AltezzaRigaNote > YBox + HBox - PaddingNote + 0.5 Then Exit For
          Gfx.DrawString(R, FNote, XBrushes.DimGray, New XRect(Margine + PaddingNote, Yr, W - 2 * PaddingNote, AltezzaRigaNote), XStringFormats.CenterLeft)
          Yr += AltezzaRigaNote
        Next
      End If
      Gfx.Dispose()
    Next
    NumeraPagine(Doc)
    Doc.Save(Percorso)
    Return True
  End Function

  ' ---------------------------------------------------------------- testo adattato alla cella

  ''' <summary>Corpo minimo del carattere nelle celle e passo con cui lo si riduce.</summary>
  Private Const CorpoMinimo As Double = 5.5
  Private Const PassoCorpo As Double = 0.5
  ''' <summary>Di quanto si riduce al massimo il carattere su una riga prima di passare a due righe (oltre il testo diventerebbe troppo piccolo).</summary>
  Private Const RiduzioneUnaRiga As Double = 1.5
  ''' <summary>Spazio libero ai lati del testo nella cella.</summary>
  Private Const MargineCella As Double = 2.5

  ''' <summary>Come sta un testo nella sua cella: carattere usato e righe (una o due; l'ultima eventualmente con i puntini).</summary>
  Private Class clsTestoCella
    Public Font As XFont
    Public Righe As New List(Of String)
    Public ReadOnly Property AltezzaRigaTesto As Double
      Get
        Return Font.Size * 1.22
      End Get
    End Property
    ''' <summary>Altezza minima della riga della tabella perche' il testo stia dentro.</summary>
    Public ReadOnly Property AltezzaNecessaria As Double
      Get
        Return If(Righe.Count <= 1, AltezzaRiga, Righe.Count * AltezzaRigaTesto + 3)
      End Get
    End Property
  End Class

  Private Shared ReadOnly CacheFont As New Dictionary(Of String, XFont)

  Private Shared Function FontCorpo(Base As XFont, Corpo As Double) As XFont
    Dim Chiave As String = Base.FontFamily.Name & "|" & Corpo.ToString("0.0", Globalization.CultureInfo.InvariantCulture) & "|" & CInt(Base.Style)
    Dim F As XFont = Nothing
    If Not CacheFont.TryGetValue(Chiave, F) Then
      F = New XFont(Base.FontFamily.Name, Corpo, Base.Style)
      CacheFont(Chiave) = F
    End If
    Return F
  End Function

  ''' <summary>
  ''' Adatta il testo alla larghezza L: prima il carattere normale, poi ridotto a passi di al massimo RiduzioneUnaRiga, poi su due righe
  ''' (al corpo piu' grande con cui entrambe stanno nella cella, a capo tra le parole), e solo nei casi estremi due righe al minimo con i puntini.
  ''' </summary>
  Private Shared Function Adatta(Gfx As XGraphics, Testo As String, Base As XFont, L As Double) As clsTestoCella
    Dim Ris As New clsTestoCella With {.Font = Base}
    If String.IsNullOrEmpty(Testo) Then
      Ris.Righe.Add("")
      Return Ris
    End If
    ' 1-2: una riga, carattere normale o poco ridotto
    Dim Corpo As Double = Base.Size
    Dim MinimoUnaRiga As Double = Math.Max(CorpoMinimo, Base.Size - RiduzioneUnaRiga)
    While Corpo >= MinimoUnaRiga - 0.001
      Dim F As XFont = FontCorpo(Base, Corpo)
      If Gfx.MeasureString(Testo, F).Width <= L Then
        Ris.Font = F
        Ris.Righe.Add(Testo)
        Return Ris
      End If
      Corpo -= PassoCorpo
    End While
    ' 3: due righe, a capo nello spazio che bilancia meglio le due righe
    Dim Parole() As String = Testo.Split(" "c)
    If Parole.Length > 1 Then
      Corpo = Base.Size
      While Corpo >= CorpoMinimo - 0.001
        Dim F As XFont = FontCorpo(Base, Corpo)
        Dim Migliore As Double = Double.MaxValue, R1 As String = Nothing, R2 As String = Nothing
        For k As Integer = 1 To Parole.Length - 1
          Dim A As String = String.Join(" ", Parole, 0, k).Trim
          Dim B As String = String.Join(" ", Parole, k, Parole.Length - k).Trim
          If A = "" OrElse B = "" Then Continue For
          Dim La As Double = Gfx.MeasureString(A, F).Width, Lb As Double = Gfx.MeasureString(B, F).Width
          If La <= L AndAlso Lb <= L AndAlso Math.Max(La, Lb) < Migliore Then
            Migliore = Math.Max(La, Lb)
            R1 = A
            R2 = B
          End If
        Next
        If R1 IsNot Nothing Then
          Ris.Font = F
          Ris.Righe.Add(R1)
          Ris.Righe.Add(R2)
          Return Ris
        End If
        Corpo -= PassoCorpo
      End While
    End If
    ' una sola parola troppo lunga: una riga al corpo minimo, se entra
    Dim Fm As XFont = FontCorpo(Base, CorpoMinimo)
    If Gfx.MeasureString(Testo, Fm).Width <= L Then
      Ris.Font = Fm
      Ris.Righe.Add(Testo)
      Return Ris
    End If
    ' 4: casi estremi, due righe al corpo minimo e puntini sull'ultima
    Ris.Font = Fm
    Dim Righe As List(Of String) = ACapo(Gfx, Testo, Fm, L)
    If Parole.Length <= 1 OrElse Righe.Count <= 1 Then
      Ris.Righe.Add(Tronca(Gfx, Testo, Fm, L))
    Else
      Ris.Righe.Add(Righe(0))
      Ris.Righe.Add(Tronca(Gfx, String.Join(" ", Righe.Skip(1)), Fm, L))
    End If
    Return Ris
  End Function

  ''' <summary>Disegna il testo adattato, centrato in verticale nella cella (a sinistra o al centro in orizzontale).</summary>
  Private Shared Sub DisegnaCella(Gfx As XGraphics, Tc As clsTestoCella, Pennello As XBrush, X As Double, Y As Double, L As Double, H As Double, ASinistra As Boolean)
    Dim Hl As Double = If(Tc.Righe.Count <= 1, H, Tc.AltezzaRigaTesto)
    Dim Y0 As Double = If(Tc.Righe.Count <= 1, Y, Y + (H - Tc.Righe.Count * Hl) / 2)
    For i As Integer = 0 To Tc.Righe.Count - 1
      Gfx.DrawString(Tc.Righe(i), Tc.Font, Pennello, New XRect(X + MargineCella, Y0 + i * Hl, L - 2 * MargineCella, Hl),
                     If(ASinistra, XStringFormats.CenterLeft, XStringFormats.Center))
    Next
  End Sub

  ''' <summary>Impaginazione di una tabella: larghezze delle colonne, testo adattato di ogni cella e altezza di ogni riga.</summary>
  Private Class clsLayoutTabella
    Public Largh() As Double
    Public Gruppi As New List(Of clsTestoCella)
    Public HGruppi As Double
    Public Intestazioni As New List(Of clsTestoCella)
    Public HIntestazione As Double
    Public Celle As New List(Of clsTestoCella())
    Public HRighe As New List(Of Double)
    Public ReadOnly Property Altezza As Double
      Get
        Return HGruppi + HIntestazione + HRighe.Sum
      End Get
    End Property
  End Class

  Private Shared Function Impagina(Gfx As XGraphics, W As Double, T As clsFfPdfTabella) As clsLayoutTabella
    Dim Lo As New clsLayoutTabella
    Dim n As Integer = T.Intestazioni.Count
    If n = 0 Then
      Lo.Largh = New Double() {}
      Return Lo
    End If
    ReDim Lo.Largh(n - 1)
    Dim Rel(n - 1) As Double
    For c As Integer = 0 To n - 1
      Rel(c) = If(T.Larghezze.Count = n, T.Larghezze(c), If(c = 0, 2.0, 1.0))
    Next
    For c As Integer = 0 To n - 1
      Lo.Largh(c) = W * Rel(c) / Rel.Sum
    Next
    If T.Gruppi.Count > 0 Then
      Dim Col As Integer = 0
      For Each G As KeyValuePair(Of String, Integer) In T.Gruppi
        Dim Lg As Double = 0
        For k As Integer = Col To Math.Min(n - 1, Col + G.Value - 1)
          Lg += Lo.Largh(k)
        Next
        Lo.Gruppi.Add(Adatta(Gfx, G.Key, FTabB, Lg - 2 * MargineCella))
        Col += G.Value
      Next
      Lo.HGruppi = Lo.Gruppi.Max(Function(q) q.AltezzaNecessaria)
    End If
    For c As Integer = 0 To n - 1
      Lo.Intestazioni.Add(Adatta(Gfx, T.Intestazioni(c), FTabB, Lo.Largh(c) - 2 * MargineCella))
    Next
    Lo.HIntestazione = Lo.Intestazioni.Max(Function(q) q.AltezzaNecessaria)
    For r As Integer = 0 To T.Righe.Count - 1
      Dim Riga() As String = T.Righe(r)
      Dim Celle(n - 1) As clsTestoCella
      For c As Integer = 0 To n - 1
        Dim F As XFont = If(r < T.RigheEvidenza OrElse T.CelleGrassetto.Contains(r & "," & c), FTabB, FTab)
        Celle(c) = Adatta(Gfx, If(c < Riga.Length, Riga(c), ""), F, Lo.Largh(c) - 2 * MargineCella)
      Next
      Lo.Celle.Add(Celle)
      Lo.HRighe.Add(Celle.Max(Function(q) q.AltezzaNecessaria))
    Next
    Return Lo
  End Function

  ''' <summary>Altezza che la tabella occupera' sulla pagina (stesso calcolo del disegno).</summary>
  Private Shared Function AltezzaTabella(Gfx As XGraphics, W As Double, T As clsFfPdfTabella) As Double
    If T Is Nothing OrElse T.Intestazioni.Count = 0 Then Return 0
    Return Impagina(Gfx, W, T).Altezza
  End Function

  ''' <summary>
  ''' Tabella compatta: griglia leggera, cornice marcata, righe alternate con sfondo. Il testo di ogni cella si adatta (carattere ridotto,
  ''' poi due righe con la riga piu' alta, poi puntini). Le righe che non entrano si perdono (la pagina e' dimensionata prima).
  ''' Restituisce la Y sotto la tabella (le note vanno nel riquadro in fondo alla pagina, vedi CreaPagine).
  ''' </summary>
  Private Shared Function DisegnaTabellaSuPagina(Gfx As XGraphics, Y As Double, W As Double, Fondo As Double, T As clsFfPdfTabella) As Double
    Dim n As Integer = T.Intestazioni.Count
    If n = 0 Then Return Y
    Dim Lo As clsLayoutTabella = Impagina(Gfx, W, T)
    Dim Y0 As Double = Y
    Dim PennaLeggera As New XPen(XColor.FromArgb(205, 205, 205), 0.3)

    ' gruppi (le linee verticali interne si limitano ai confini dei gruppi)
    If T.Gruppi.Count > 0 Then
      Dim Xg As Double = Margine
      Dim Col As Integer = 0
      For g As Integer = 0 To T.Gruppi.Count - 1
        Dim Lg As Double = 0
        For k As Integer = Col To Math.Min(n - 1, Col + T.Gruppi(g).Value - 1)
          Lg += Lo.Largh(k)
        Next
        Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(214, 224, 238)), Xg, Y, Lg, Lo.HGruppi)
        DisegnaCella(Gfx, Lo.Gruppi(g), XBrushes.Black, Xg, Y, Lg, Lo.HGruppi, False)
        Gfx.DrawLine(PennaLeggera, Xg, Y, Xg, Y + Lo.HGruppi)
        Xg += Lg
        Col += T.Gruppi(g).Value
      Next
      Y += Lo.HGruppi
    End If

    ' intestazione
    Dim YIntestazione As Double = Y
    Dim X As Double = Margine
    For c As Integer = 0 To n - 1
      Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(232, 232, 232)), X, Y, Lo.Largh(c), Lo.HIntestazione)
      DisegnaCella(Gfx, Lo.Intestazioni(c), XBrushes.Black, X, Y, Lo.Largh(c), Lo.HIntestazione, False)
      X += Lo.Largh(c)
    Next
    Y += Lo.HIntestazione
    Dim YCorpo As Double = Y

    ' corpo: righe alternate con sfondo, altezza secondo il testo
    Dim Bordi As New List(Of Double) From {YCorpo}
    Dim Riquadri As New List(Of XRect)
    For r As Integer = 0 To T.Righe.Count - 1
      Dim H As Double = Lo.HRighe(r)
      If Y + H > Fondo Then Exit For
      Dim Xr As Double = Margine
      If r Mod 2 = 1 Then Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(240, 242, 246)), Xr, Y, W, H)
      For c As Integer = 0 To n - 1
        Dim Chiave As String = r & "," & c
        Dim Argb As Integer
        If T.Sfondi.TryGetValue(Chiave, Argb) Then Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(Argb)), Xr, Y, Lo.Largh(c), H)
        If T.CelleBordo.Contains(Chiave) Then Riquadri.Add(New XRect(Xr + 0.8, Y + 0.8, Lo.Largh(c) - 1.6, H - 1.6))
        Dim Pennello As XBrush = If(T.CelleGrigie.Contains(Chiave), XBrushes.Gray, XBrushes.Black)
        DisegnaCella(Gfx, Lo.Celle(r)(c), Pennello, Xr, Y, Lo.Largh(c), H, c = 0)
        Xr += Lo.Largh(c)
      Next
      Y += H
      Bordi.Add(Y)
    Next

    ' griglia leggera (verticali e orizzontali) e cornice marcata
    Dim Xv As Double = Margine
    For c As Integer = 1 To n - 1
      Xv += Lo.Largh(c - 1)
      Gfx.DrawLine(PennaLeggera, Xv, YIntestazione, Xv, Y)
    Next
    For Each Yb As Double In Bordi
      Gfx.DrawLine(PennaLeggera, Margine, Yb, Margine + W, Yb)
    Next
    Gfx.DrawLine(PennaLeggera, Margine, YIntestazione, Margine + W, YIntestazione)
    Gfx.DrawRectangle(New XPen(XColors.DimGray, 1.4), Margine, Y0, W, Y - Y0)
    ' bordi marcati sopra la griglia
    Dim PennaBordo As New XPen(XColor.FromArgb(T.BordoColore), 1.6)
    For Each Rq As XRect In Riquadri
      Gfx.DrawRectangle(PennaBordo, Rq)
    Next
    Return Y
  End Function

  ''' <summary>Su ogni pagina: cornice marcata, numero di pagina in basso a sinistra e nome dell'applicazione e del tipo di grafico in basso a destra.</summary>
  Private Shared Sub NumeraPagine(Doc As PdfDocument)
    For i As Integer = 0 To Doc.PageCount - 1
      Dim P As PdfPage = Doc.Pages(i)
      Dim G As XGraphics = XGraphics.FromPdfPage(P)
      Dim W As Double = P.Width.Value
      Dim H As Double = P.Height.Value
      G.DrawRectangle(New XPen(XColor.FromArgb(40, 60, 90), 2.5), 12, 12, W - 24, H - 24)
      G.DrawString("Page " & (i + 1) & " / " & Doc.PageCount, FPiede, XBrushes.Gray, New XRect(Margine, H - 12 - AltezzaPiede - 2, 200, AltezzaPiede), XStringFormats.CenterLeft)
      G.DrawString("SailingPerformer  FeedForward Analysis", FPiedeB, XBrushes.DarkSlateGray, New XRect(W - Margine - 300, H - 12 - AltezzaPiede - 2, 300, AltezzaPiede), XStringFormats.CenterRight)
      G.Dispose()
    Next
  End Sub

End Class
