Public Class UserControlStraightLinePlotContainer

	Public Sub New()

		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.

	End Sub

  Public Sub New(ControlHeight As Double)

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    GrigliaGrafico.Height = ControlHeight

  End Sub

  Private Sub SplitGrafici_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
    If ViewModel.Periodo Is Nothing Then Exit Sub
    Dim Delta As Double = e.VerticalChange
    'Dim H As Double = SCsurface.Height
    If Double.IsNaN(GrigliaGrafico.Height) Then
      GrigliaGrafico.Height = GrigliaGrafico.ActualHeight
    End If
    If GrigliaGrafico.Height + Delta < 5 Then
      GrigliaGrafico.Height = 5
    Else
      GrigliaGrafico.Height += Delta
    End If


	End Sub

	Public ReadOnly Property ViewModel As clsStraightLineContainerViewModel
    Get
      Return Me.DataContext
    End Get
  End Property


End Class
