Imports System.Windows
Imports System.Windows.Controls

''' <summary>Elenco con casella di spunta dei canali dei plot a video: sceglie quali stampare nel report pdf/html.</summary>
Public Class FinestraCanaliReport
  Inherits Window

  Private ReadOnly Voci As New List(Of CheckBox)

  Public Sub New(Canali As List(Of clsStraightLineChartSettings))
    Title = "Report channels"
    Width = 320
    Height = 460
    WindowStartupLocation = WindowStartupLocation.CenterOwner
    If Not Application.Current Is Nothing AndAlso Not Application.Current.MainWindow Is Nothing AndAlso Application.Current.MainWindow.IsLoaded Then Owner = Application.Current.MainWindow

    Dim Radice As New DockPanel With {.Margin = New Thickness(10)}

    Dim Pulsanti As New StackPanel With {.Orientation = Orientation.Horizontal, .HorizontalAlignment = HorizontalAlignment.Right, .Margin = New Thickness(0, 10, 0, 0)}
    Dim Ok As New Button With {.Content = "Make report", .Width = 90, .IsDefault = True, .Margin = New Thickness(0, 0, 8, 0)}
    Dim Annulla As New Button With {.Content = "Cancel", .Width = 70, .IsCancel = True}
    AddHandler Ok.Click, Sub() DialogResult = True
    Pulsanti.Children.Add(Ok)
    Pulsanti.Children.Add(Annulla)
    DockPanel.SetDock(Pulsanti, Dock.Bottom)
    Radice.Children.Add(Pulsanti)

    Dim Selezione As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 6)}
    Dim Tutti As New Button With {.Content = "All", .Width = 50, .Margin = New Thickness(0, 0, 6, 0)}
    Dim Nessuno As New Button With {.Content = "None", .Width = 50}
    AddHandler Tutti.Click, Sub() ImpostaTutti(True)
    AddHandler Nessuno.Click, Sub() ImpostaTutti(False)
    Selezione.Children.Add(Tutti)
    Selezione.Children.Add(Nessuno)
    DockPanel.SetDock(Selezione, Dock.Top)
    Radice.Children.Add(Selezione)

    Dim Lista As New StackPanel
    For Each cs In Canali
      Dim Ch = DataProvider2020.Channels.Canale(cs.ChannelName)
      Dim Testo As String = cs.ChannelName
      If Not Ch Is Nothing Then
        Testo = If(String.IsNullOrWhiteSpace(Ch.LongName), cs.ChannelName, Ch.LongName)
        If Not String.IsNullOrWhiteSpace(Ch.ShortUM) Then Testo &= " (" & Ch.ShortUM & ")"
      End If
      Dim Voce As New CheckBox With {.Content = Testo, .IsChecked = cs.Export, .Tag = cs, .Margin = New Thickness(2, 2, 2, 2)}
      Voci.Add(Voce)
      Lista.Children.Add(Voce)
    Next
    Radice.Children.Add(New ScrollViewer With {.Content = Lista, .VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .BorderThickness = New Thickness(1), .BorderBrush = System.Windows.Media.Brushes.Gray})
    Content = Radice
  End Sub

  Private Sub ImpostaTutti(Valore As Boolean)
    For Each v In Voci
      v.IsChecked = Valore
    Next
  End Sub

  ''' <summary>Riporta le spunte nella proprieta' Export dei canali.</summary>
  Public Sub Applica()
    For Each v In Voci
      DirectCast(v.Tag, clsStraightLineChartSettings).Export = v.IsChecked = True
    Next
  End Sub

End Class
