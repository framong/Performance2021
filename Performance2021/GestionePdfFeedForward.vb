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
End Class

Public Class clsPdfFeedForward

  Private Const Margine As Double = 24
  Private Const Spazio As Double = 6
  Private Const AltezzaPiede As Double = 16
  Private Const AltezzaRiga As Double = 12

  Private Shared ReadOnly FTit As New XFont("Verdana", 14, XFontStyle.Bold)
  Private Shared ReadOnly FSez As New XFont("Verdana", 11, XFontStyle.Bold)
  Private Shared ReadOnly FTesto As New XFont("Verdana", 8)
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

      ' altezza di note e tabella, riservata sotto il grafico
      Dim T As clsFfPdfTabella = Pg.Tabella
      Dim RigheNote As New List(Of String)
      For Each R As String In Pg.Righe
        RigheNote.AddRange(ACapo(Gfx, R, FTesto, W))
      Next
      Dim Ht As Double = 11 * RigheNote.Count + 6
      If T IsNot Nothing Then
        Dim NNote As Integer = 0
        For Each Nota As String In T.Note.Where(Function(q) Not q.StartsWith("#"))
          NNote += ACapo(Gfx, Nota, FTesto, W).Count
        Next
        Ht += AltezzaRiga * ((If(T.Gruppi.Count > 0, 1, 0)) + 1 + T.Righe.Count) + 4 + 11 * NNote
      End If
      Dim Fondo As Double = Pagina.Height.Value - Margine * 0.5 - AltezzaPiede - 4
      Dim HGraf As Double = Fondo - Y - Ht - Spazio * 2

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

      ' tabella sotto il grafico, poi tutte le note (legenda della tabella, sorgente, filtro, impostazioni)
      If T IsNot Nothing Then Y = DisegnaTabellaSuPagina(Gfx, Y, W, Fondo, T)
      For Each R As String In RigheNote
        If Y + 11 > Fondo + AltezzaPiede Then Exit For
        Gfx.DrawString(R, FTesto, XBrushes.DimGray, New XRect(Margine, Y, W, 11), XStringFormats.CenterLeft)
        Y += 11
      Next
      Gfx.Dispose()
    Next
    NumeraPagine(Doc)
    Doc.Save(Percorso)
    Return True
  End Function

  ''' <summary>
  ''' Tabella compatta: griglia leggera, cornice marcata, righe alternate con sfondo, note sotto. Le righe che non entrano si perdono (la pagina e' dimensionata prima).
  ''' Restituisce la Y sotto l'ultima nota.
  ''' </summary>
  Private Shared Function DisegnaTabellaSuPagina(Gfx As XGraphics, Y As Double, W As Double, Fondo As Double, T As clsFfPdfTabella)
    Dim n As Integer = T.Intestazioni.Count
    If n = 0 Then Return Y
    Dim Largh(n - 1) As Double
    Dim Rel(n - 1) As Double
    For c As Integer = 0 To n - 1
      Rel(c) = If(T.Larghezze.Count = n, T.Larghezze(c), If(c = 0, 2.0, 1.0))
    Next
    For c As Integer = 0 To n - 1
      Largh(c) = W * Rel(c) / Rel.Sum
    Next
    Dim Y0 As Double = Y
    Dim PennaLeggera As New XPen(XColor.FromArgb(205, 205, 205), 0.3)

    ' gruppi (le linee verticali interne si limitano ai confini dei gruppi)
    Dim YGruppi As Double = -1
    If T.Gruppi.Count > 0 Then
      YGruppi = Y
      Dim Xg As Double = Margine
      Dim Col As Integer = 0
      For Each G As KeyValuePair(Of String, Integer) In T.Gruppi
        Dim Lg As Double = 0
        For k As Integer = Col To System.Math.Min(n - 1, Col + G.Value - 1)
          Lg += Largh(k)
        Next
        Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(214, 224, 238)), Xg, Y, Lg, AltezzaRiga)
        Gfx.DrawString(Tronca(Gfx, G.Key, FTabB, Lg - 3), FTabB, XBrushes.Black, New XRect(Xg, Y, Lg, AltezzaRiga), XStringFormats.Center)
        Gfx.DrawLine(PennaLeggera, Xg, Y, Xg, Y + AltezzaRiga)
        Xg += Lg
        Col += G.Value
      Next
      Y += AltezzaRiga
    End If

    ' intestazione
    Dim X As Double = Margine
    For c As Integer = 0 To n - 1
      Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(232, 232, 232)), X, Y, Largh(c), AltezzaRiga)
      Gfx.DrawString(Tronca(Gfx, T.Intestazioni(c), FTabB, Largh(c) - 2), FTabB, XBrushes.Black, New XRect(X, Y, Largh(c), AltezzaRiga), XStringFormats.Center)
      X += Largh(c)
    Next
    Y += AltezzaRiga
    Dim YCorpo As Double = Y

    ' corpo: righe alternate con sfondo
    Dim NVisibili As Integer = 0
    For r As Integer = 0 To T.Righe.Count - 1
      If Y + AltezzaRiga > Fondo Then Exit For
      Dim Riga() As String = T.Righe(r)
      Dim Xr As Double = Margine
      If r Mod 2 = 1 Then Gfx.DrawRectangle(New XSolidBrush(XColor.FromArgb(240, 242, 246)), Xr, Y, W, AltezzaRiga)
      For c As Integer = 0 To n - 1
        Dim Testo As String = If(c < Riga.Length, Riga(c), "")
        Dim F As XFont = If(r < T.RigheEvidenza OrElse T.CelleGrassetto.Contains(r & "," & c), FTabB, FTab)
        Dim Pennello As XBrush = If(T.CelleGrigie.Contains(r & "," & c), XBrushes.Gray, XBrushes.Black)
        Gfx.DrawString(Tronca(Gfx, Testo, F, Largh(c) - 2), F, Pennello, New XRect(Xr + If(c = 0, 3, 0), Y, Largh(c), AltezzaRiga), If(c = 0, XStringFormats.CenterLeft, XStringFormats.Center))
        Xr += Largh(c)
      Next
      Y += AltezzaRiga
      NVisibili += 1
    Next

    ' griglia leggera (verticali e orizzontali) e cornice marcata
    Dim Xv As Double = Margine
    For c As Integer = 1 To n - 1
      Xv += Largh(c - 1)
      Gfx.DrawLine(PennaLeggera, Xv, YCorpo - AltezzaRiga, Xv, Y)
    Next
    For r As Integer = 0 To NVisibili
      Gfx.DrawLine(PennaLeggera, Margine, YCorpo + r * AltezzaRiga, Margine + W, YCorpo + r * AltezzaRiga)
    Next
    Gfx.DrawLine(PennaLeggera, Margine, YCorpo - AltezzaRiga, Margine + W, YCorpo - AltezzaRiga)
    Gfx.DrawRectangle(New XPen(XColors.DimGray, 1.4), Margine, Y0, W, Y - Y0)

    Y += 4
    For Each Nota As String In T.Note.Where(Function(q) Not q.StartsWith("#"))
      For Each R As String In ACapo(Gfx, Nota, FTesto, W)
        If Y + 11 > Fondo + AltezzaPiede Then Exit For
        Gfx.DrawString(R, FTesto, XBrushes.DimGray, New XRect(Margine, Y, W, 11), XStringFormats.CenterLeft)
        Y += 11
      Next
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
