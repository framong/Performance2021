Imports System.ComponentModel
Imports System.Windows.Controls.Primitives
Imports SPwpf

Public Class UserControlDataDisplay
  Dim pCanale As clsChannel2020

  Public Property Canale As clsChannel2020
    Get
      Return pCanale
    End Get
    Set(value As clsChannel2020)
      pCanale = value
    End Set
  End Property

  Public Sub New(Canale As clsChannel2020)

    pCanale = Canale
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    ImpostaBinding()
  End Sub


  Private Sub ToggleNomi()
    pCanale.ToggleNomi()
  End Sub

  Private Sub ImpostaBinding()

    Dim BND As New Binding("Name")
    BND.Source = Canale
    lbl_Descrizione.SetBinding(Label.ContentProperty, BND)

    BND = New Binding("UM")
    BND.Source = Canale
    lbl_UM.SetBinding(Label.ContentProperty, BND)

    BND = New Binding("ValoriIntervallo.Max")
    BND.Source = Canale
    lbl_MaxVal.SetBinding(Label.ContentProperty, BND)

    BND = New Binding("ValoriIntervallo.Min")
    BND.Source = Canale
    lbl_MinVal.SetBinding(Label.ContentProperty, BND)

    BND = New Binding("ValoriIntervallo.Avg")
    BND.Source = Canale
    lbl_AvgVal.SetBinding(Label.ContentProperty, BND)

    BND = New Binding("ValoriIntervallo.Ds")
    BND.Source = Canale
    lbl_DsVal.SetBinding(Label.ContentProperty, BND)

    Dim CNV As New clsColorConverter
    BND = New Binding("PrintedColor")
    BND.Source = Canale
    BND.Converter = CNV
    OutBorder.SetBinding(Border.BorderBrushProperty, BND)

    BND = New Binding("DataBoxHeight")
    Me.SetBinding(MyBase.HeightProperty, BND)

  End Sub

  Private Sub SplitDati_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles SplitDati.DragCompleted
    Dim Delta As Double = e.VerticalChange
    Dim H As Double = Me.Height
    If Double.IsNaN(Me.Height) Then
      Me.Height = Me.ActualHeight
    End If
    If Me.Height + Delta < 5 Then
      Me.Height = 5
    Else
      Me.Height += Delta
    End If

  End Sub
End Class
