Imports System.Windows.Media.Media3D
Imports Ab3d.Common.EventManager3D
Imports Ab3d.Common.Models
Imports Ab3d.Meshes
Imports Ab3d.Utilities
Imports Ab3d.Visuals

Public Class UserControlAb3D
  'Public Property VM As UserControlAb3DViewModel

  Private Property _allSpheresData As List(Of SphereDataView)

  Private Property _isSelecting As Boolean
  Private Property _isSelectStarting As Boolean
  Private Property _startSelectionPosition As Point

  Private Property _sampleData As List(Of SphereData)
  Private Property _xyDataRange As Rect

  Private Property _gradientColorsArray As Color()

  'Public Property XchannelName As String
  'Public Property YchannelName As String
  'Public Property ZchannelName As String

  'Public Property XchannelMainSep As Integer
  'Public Property YchannelMainSep As Integer
  'Public Property ZchannelMainSep As Integer


  Public Sub New()




    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.


    CameraControllerInfo.AddCustomInfoLine(0, Ab3d.Controls.MouseCameraController.MouseAndKeyboardConditions.LeftMouseButtonPressed, "Click to select sphere\r\nDrag for rectangular section")
    Dim LinearGradientBrush = CreateGradientBrush()
    _gradientColorsArray = HeightMapMesh3D.GetGradientColorsArray(LinearGradientBrush, 30)
    LegendRectangle.Fill = LinearGradientBrush

    _xyDataRange = New Rect(-10, -10, 20, 20)
    _sampleData = GenerateRandomData(_xyDataRange, 0.2, 20)


    'Setup axis limits And shown values
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.XAxis, 0, _sampleData.Count, 2, 0, True)
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.YAxis, _xyDataRange.Y, _xyDataRange.Y + _xyDataRange.Height, 5, 2.5, True)
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.ZAxis, _xyDataRange.X, _xyDataRange.X + _xyDataRange.Width, 5, 2.5, True)

    Dim displayedDataBounds As New Rect3D(AxesBox.CenterPosition.X - AxesBox.Size.X * 0.5,
                                                 AxesBox.CenterPosition.Y - AxesBox.Size.Y * 0.5,
                                                 AxesBox.CenterPosition.Z - AxesBox.Size.Z * 0.5,
                                                 AxesBox.Size.X,
                                                 AxesBox.Size.Y,
                                                 AxesBox.Size.Z)

    ShowData(_sampleData, displayedDataBounds, _xyDataRange)

    UpdateSelectedSpheresData()

    MinValueTextBlock.Text = String.Format("{0:0}", _xyDataRange.Y)
    MaxValueTextBlock.Text = String.Format("{0:0}", _xyDataRange.Y + _xyDataRange.Height)

    '// Subscribe mouse events that will be used to create selection rectangle
    'ViewportBorder.MouseLeftButtonDown += SelectionOverlayCanvasOnMouseLeftButtonDown
    'ViewportBorder.MouseMove += SelectionOverlayCanvasOnMouseMove
    'ViewportBorder.MouseLeftButtonUp += SelectionOverlayCanvasOnMouseLeftButtonUp


  End Sub


  Private Sub ExportToImage()
    Dim fileDialog = New Microsoft.Win32.SaveFileDialog With {
        .DefaultExt = "png",
        .Filter = "Png files(*.png)|*.png",
        .Title = "Select output png file",
        .FileName = "Plot3D.png"
    }

    If If(fileDialog.ShowDialog(), False) Then
      Dim plotImage As BitmapSource = Camera1.RenderToBitmap(1920, 1200, 4)

      If plotImage IsNot Nothing Then

        Using fileStream = New System.IO.FileStream(fileDialog.FileName, System.IO.FileMode.Create)
          Dim encoder As BitmapEncoder = New PngBitmapEncoder()
          encoder.Frames.Add(BitmapFrame.Create(plotImage))
          encoder.Save(fileStream)
        End Using

        Dim processStartInfo = New ProcessStartInfo With {
                .FileName = fileDialog.FileName,
                .UseShellExecute = True
            }
        Process.Start(processStartInfo)
      End If
    End If
  End Sub


  Private Function ResetBox() As Rect3D


    'Setup axis limits And shown values
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.XAxis, 0, _sampleData.Select(Function(X) X.Time).Max, 10, 5, True)
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.YAxis, _xyDataRange.Y, _xyDataRange.Y + _xyDataRange.Height, 10, 5, True)
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.ZAxis, _xyDataRange.X, _xyDataRange.X + _xyDataRange.Width, 10, 5, True)

    Dim displayedDataBounds As New Rect3D(AxesBox.CenterPosition.X - AxesBox.Size.X * 0.5,
                                                 AxesBox.CenterPosition.Y - AxesBox.Size.Y * 0.5,
                                                 AxesBox.CenterPosition.Z - AxesBox.Size.Z * 0.5,
                                                 AxesBox.Size.X,
                                                 AxesBox.Size.Y,
                                                 AxesBox.Size.Z)
    Return displayedDataBounds
  End Function

  Public Function ResetBoxGraficoXYZ(DataRange As Rect3D, Xsep As Integer, Ysep As Integer, Zsep As Integer) As Rect3D


    'Setup axis limits And shown values
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.XAxis, 0, DataRange.SizeX, Xsep, Xsep / 2, True) ' Twa
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.YAxis, 0, DataRange.SizeY, Ysep, Ysep / 2, True) ' Tws
    AxesBox.SetAxisDataRange(AxesBoxVisual3D.AxisTypes.ZAxis, 0, DataRange.SizeZ, Zsep, Zsep / 2, True) ' Channel Vmg%

    Dim displayedDataBounds As New Rect3D(0, 0, 0, AxesBox.Size.X, AxesBox.Size.Y, AxesBox.Size.Z)
    Return displayedDataBounds
  End Function

  Private Sub DataListBoxOnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    For Each addedItem In e.AddedItems
      CType(addedItem, SphereDataView).IsSelected = True
    Next

    For Each addedItem In e.RemovedItems
      CType(addedItem, SphereDataView).IsSelected = False
    Next
  End Sub

  Private Sub RecreateDataButtonOnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    '_xyDataRange = New Rect(-10, -10, 20, 20)
    '_sampleData = GenerateRandomData(_xyDataRange, 0.2, 1000)

    _sampleData.Clear()
    'Dim random As New Random
    'For twa As Integer = 0 To 180
    '  For tws As Integer = 0 To 35
    '    Dim v = random.Next(90, 110)
    '    Dim sd As New SphereData(twa, New Point(tws, v), 1)
    '    _sampleData.Add(sd)
    '  Next
    'Next

    '_xyDataRange = New Rect(0, 0, 10, 10)

    '_sampleData.Add(New SphereData(Twa, New Point(0, 0), 1))
    _sampleData.Add(New SphereData(1, New Point(0, 0), 1))
    '_sampleData.Add(New SphereData(1, New Point(0, 0), 2))
    '_sampleData.Add(New SphereData(0, New Point(1, 0), 3))
    '_sampleData.Add(New SphereData(0, New Point(0, 1), 4))
    _sampleData.Add(New SphereData(5, New Point(1, 1), 5))
    '_xyDataRange = New Rect(0, 0, 180, 35)

    Dim DataRange As New Rect3D(0, 0, 0, 180, 40, 120)
    Dim displayedDataBounds = ResetBoxGraficoXYZ(DataRange, 5, 5, 5) ' New Rect3D(AxesBox.CenterPosition.X - AxesBox.Size.X * 0.5, AxesBox.CenterPosition.Y - AxesBox.Size.Y * 0.5, AxesBox.CenterPosition.Z - AxesBox.Size.Z * 0.5, AxesBox.Size.X, AxesBox.Size.Y, AxesBox.Size.Z)
    'Dim displayedDataBounds = New Rect3D(AxesBox.CenterPosition.X - AxesBox.Size.X * 0.5, AxesBox.CenterPosition.Y - AxesBox.Size.Y * 0.5, AxesBox.CenterPosition.Z - AxesBox.Size.Z * 0.5, AxesBox.Size.X, AxesBox.Size.Y, AxesBox.Size.Z)
    'Dim displayedDataBounds = New Rect3D(0, 0, 0, 10, 20, 30)

    Dim p As New List(Of clsXYZColore)
    p.Add(New clsXYZColore(-1, 0, 0, 0, Colors.Black)) ' Twa, Tws. Vmg%
    p.Add(New clsXYZColore(-1, 90, 0, 0, Colors.Red))
    p.Add(New clsXYZColore(-1, 90, 20, 0, Colors.Green))
    p.Add(New clsXYZColore(-1, 90, 40, 10, Colors.Blue))
    p.Add(New clsXYZColore(-1, 180, 40, 100, Colors.Orange))
    PlottaGraficoXYZ(p, displayedDataBounds, DataRange, 1)

    'ShowData(_sampleData, displayedDataBounds, _xyDataRange)
  End Sub

  Private Sub ClearSelectionButtonOnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    For Each sphereDataView In _allSpheresData
      sphereDataView.IsSelected = False
    Next
  End Sub

  Private Sub SelectionOverlayCanvasOnMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
    _isSelectStarting = True
    Dim position As Point = e.GetPosition(SelectionOverlayCanvas)
    _startSelectionPosition = position

    For Each positionData In _allSpheresData
      positionData.ScreenPosition = Camera1.Point3DTo2D(positionData.Position)
    Next

    e.Handled = True
  End Sub

  Private Sub SelectionOverlayCanvasOnMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
    If Not _isSelecting AndAlso Not _isSelectStarting Then
      e.Handled = False
      Return
    End If

    Dim position As Point = e.GetPosition(SelectionOverlayCanvas)
    Dim dx As Double = position.X - _startSelectionPosition.X
    Dim dy As Double = position.Y - _startSelectionPosition.Y
    Dim width As Double = Math.Abs(dx)
    Dim height As Double = Math.Abs(dy)

    If _isSelectStarting Then

      If width > 2 OrElse height > 2 Then
        _isSelectStarting = False
        _isSelecting = True
        SelectionRectangle.Visibility = Visibility.Visible
      Else
        Return
      End If
    End If

    Dim x As Double = _startSelectionPosition.X
    Dim y As Double = _startSelectionPosition.Y
    If dx < 0 Then x += dx
    If dy < 0 Then y += dy
    Canvas.SetLeft(SelectionRectangle, x)
    Canvas.SetTop(SelectionRectangle, y)
    SelectionRectangle.Width = width
    SelectionRectangle.Height = height
    Dim selectionRect As Rect = New Rect(x, y, width, height)

    For Each positionData In _allSpheresData
      positionData.IsSelected = positionData.IsInScreenRectangle(selectionRect)
    Next

    e.Handled = True
  End Sub

  Private Sub SelectionOverlayCanvasOnMouseLeftButtonUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
    _isSelecting = False
    _isSelectStarting = False
    SelectionRectangle.Visibility = Visibility.Collapsed
    e.Handled = True
  End Sub

  Private Function CreateGradientBrush() As LinearGradientBrush
    Dim gradientStopCollection = New GradientStopCollection()
    gradientStopCollection.Add(New GradientStop(Colors.Red, 1))
    gradientStopCollection.Add(New GradientStop(Colors.Yellow, 0.5))
    gradientStopCollection.Add(New GradientStop(Colors.DodgerBlue, 0))
    Dim linearGradientBrush = New LinearGradientBrush(gradientStopCollection, New Point(0, 1), New Point(0, 0))
    Return linearGradientBrush
  End Function

  Public Shared Function GenerateRandomData(ByVal xyDataRange As Rect, ByVal relativeMargin As Double, ByVal dataCount As Integer) As List(Of SphereData)
    Dim originalData = New List(Of SphereData)()
    Dim minX = xyDataRange.X
    Dim minY = xyDataRange.Y
    Dim maxX = xyDataRange.X + xyDataRange.Width
    Dim maxY = xyDataRange.Y + xyDataRange.Height
    minX += relativeMargin * xyDataRange.Width * 0.5
    minY += relativeMargin * xyDataRange.Height * 0.5
    maxX -= relativeMargin * xyDataRange.Width * 0.5
    maxY -= relativeMargin * xyDataRange.Height * 0.5
    Dim x As Double = xyDataRange.X + xyDataRange.Width * 0.5
    Dim y As Double = xyDataRange.Y + xyDataRange.Height * 0.5
    Dim vx As Double = 0
    Dim vy As Double = 0
    Dim rnd = New Random()
    Dim randomizationFactor As Double = 0.1

    For i As Integer = 0 To dataCount - 1
      Dim dx As Double = xyDataRange.Width * randomizationFactor * (rnd.NextDouble() * 2 - 1)
      Dim dy As Double = xyDataRange.Height * randomizationFactor * (rnd.NextDouble() * 2 - 1)
      vx += dx
      vy += dy
      x += vx
      y += vy

      If x < minX Then
        x = minX + (minX - x)
        vx = -vx
      ElseIf x > maxX Then
        x = maxX - (x - maxX)
        vx = -vx
      End If

      If y < minY Then
        y = minY + (minY - y)
        vy = -vy
      ElseIf y > maxY Then
        y = maxY - (y - maxY)
        vy = -vy
      End If

      x = Math.Max(minX, Math.Min(maxX, x))
      y = Math.Max(minY, Math.Min(maxY, y))
      Dim sphereSize As Double = rnd.NextDouble() * 3 + 1
      Dim sphereData = New SphereData(time:=i + 1, location:=New Point(x, y), size:=sphereSize)
      originalData.Add(sphereData)
    Next

    Return originalData
  End Function


  Public Sub PlottaGraficoXYZ(Punti As List(Of clsXYZColore), displayedDataBounds As Rect3D, dataRange As Rect3D, PointSize As Double)
    _allSpheresData = New List(Of SphereDataView)(Punti.Count)
    For Each Punto In Punti
      Dim sphereDataView As SphereDataView = SphereDataView.Create(Punto, displayedDataBounds, dataRange, PointSize)
      'AddHandler sphereDataView.IsSelectedChanged, AddressOf sphereDataView_IsSelectedChanged
      _allSpheresData.Add(sphereDataView)
    Next
    DataListBox.ItemsSource = _allSpheresData
    CurveModelVisual.Content = Nothing
    SpheresModelVisual.Children.Clear()
    Dim eventManager3D = New Ab3d.Utilities.EventManager3D(MainViewport)

    For Each positionData In _allSpheresData
      SpheresModelVisual.Children.Add(positionData.ModelVisual3D)
      Dim visualEventSource3D = New VisualEventSource3D(positionData.ModelVisual3D)
      'AddHandler visualEventSource3D.MouseEnter, AddressOf visualEventSource3D_MouseEnter
      'AddHandler visualEventSource3D.MouseLeave, AddressOf visualEventSource3D_MouseLeave
      'AddHandler visualEventSource3D.MouseClick, AddressOf visualEventSource3D_MouseClick
      eventManager3D.RegisterEventSource3D(visualEventSource3D)
    Next
  End Sub


  Public Sub ShowData(ByVal originalData As List(Of SphereData), ByVal displayedDataBounds As Rect3D, ByVal xyDataRange As Rect)
    _allSpheresData = New List(Of SphereDataView)(originalData.Count)

    For Each originalSphereData In originalData
      Dim relativeY As Double = (originalSphereData.Location.Y - xyDataRange.Y) / xyDataRange.Height
      Dim colorArrayIndex As Integer = CInt((relativeY * (_gradientColorsArray.Length - 1)))
      Dim color As Color = _gradientColorsArray(Math.Max(0, Math.Min(_gradientColorsArray.Count - 1, colorArrayIndex)))
      Dim sphereDataView As SphereDataView = SphereDataView.Create(originalSphereData, displayedDataBounds, originalData.Select(Function(x) x.Time).Max, xyDataRange, color)
      'Dim sphereDataView As SphereDataView = SphereDataView.Create(originalSphereData, displayedDataBounds, originalData.Count, xyDataRange, color)

      AddHandler sphereDataView.IsSelectedChanged, AddressOf sphereDataView_IsSelectedChanged

      _allSpheresData.Add(sphereDataView)
    Next

    DataListBox.ItemsSource = _allSpheresData
    Dim allPositions As List(Of Point3D) = New List(Of Point3D)()

    For Each positionData In _allSpheresData
      allPositions.Add(positionData.Position)
    Next

    Dim bezierCurve As BezierCurve = Ab3d.Utilities.BezierCurve.CreateFromCurvePositions(allPositions)
    Dim curvePoints As Point3DCollection = bezierCurve.CreateBezierCurve(10)
    Dim curveModel As Model3D = Ab3d.Models.Line3DFactory.CreatePolyLine3D(curvePoints, 2, Colors.Blue, False, LineCap.Flat, LineCap.Flat, MainViewport)
    CurveModelVisual.Content = curveModel
    SpheresModelVisual.Children.Clear()
    Dim eventManager3D = New Ab3d.Utilities.EventManager3D(MainViewport)

    For Each positionData In _allSpheresData
      SpheresModelVisual.Children.Add(positionData.ModelVisual3D)
      Dim visualEventSource3D = New VisualEventSource3D(positionData.ModelVisual3D)

      AddHandler visualEventSource3D.MouseEnter, AddressOf visualEventSource3D_MouseEnter
      AddHandler visualEventSource3D.MouseLeave, AddressOf visualEventSource3D_MouseLeave
      AddHandler visualEventSource3D.MouseClick, AddressOf visualEventSource3D_MouseClick

      eventManager3D.RegisterEventSource3D(visualEventSource3D)
    Next
  End Sub

  Private Sub sphereDataView_IsSelectedChanged(ByVal sender As Object, ByVal args As EventArgs)
    Dim changedPositionDataView = CType(sender, SphereDataView)
    If changedPositionDataView.IsSelected Then
      DataListBox.SelectedItems.Add(changedPositionDataView)
    Else
      DataListBox.SelectedItems.Remove(changedPositionDataView)
    End If
    UpdateSelectedSpheresData()
  End Sub

  Private Sub visualEventSource3D_MouseEnter(ByVal sender As Object, ByVal e As Mouse3DEventArgs)
    If _isSelecting Then Exit Sub
    Mouse.OverrideCursor = Cursors.Hand
    Dim selectedPositionData = _allSpheresData.Where(Function(p) p.ModelVisual3D Is e.HitObject).FirstOrDefault
    SelectData(selectedPositionData, e.CurrentMousePosition)
  End Sub

  Private Sub visualEventSource3D_MouseLeave(ByVal sender As Object, ByVal e As Mouse3DEventArgs)
    If _isSelecting Then Exit Sub
    Mouse.OverrideCursor = Nothing
    DataToolTipBorder.Visibility = Visibility.Collapsed
    DataToolTipBorder.DataContext = Nothing
    SelectedSphereLinesVisual.Children.Clear()
  End Sub

  Private Sub visualEventSource3D_MouseClick(ByVal sender As Object, ByVal e As MouseButton3DEventArgs)
    Dim clickedPositionData = _allSpheresData.Where(Function(p) p.ModelVisual3D Is e.HitObject).FirstOrDefault
    If clickedPositionData IsNot Nothing Then clickedPositionData.IsSelected = Not clickedPositionData.IsSelected
  End Sub

  Public Sub SelectData(ByVal selectedPositionData As SphereDataView, ByVal mousePosition As Point)
    DataToolTipBorder.DataContext = selectedPositionData
    Canvas.SetLeft(DataToolTipBorder, mousePosition.X + 10)
    Canvas.SetTop(DataToolTipBorder, mousePosition.Y + 10)
    DataToolTipBorder.Visibility = Visibility.Visible
    Dim centerPosition = AxesBox.CenterPosition
    Dim size = AxesBox.Size
    Dim wireBoxBottom As Double = centerPosition.Y - size.Y * 0.5
    Dim verticalLineVisual3D = New Ab3d.Visuals.LineVisual3D() With {
        .StartPosition = selectedPositionData.Position,
        .EndPosition = New Point3D(selectedPositionData.Position.X, wireBoxBottom, selectedPositionData.Position.Z),
        .LineThickness = 2,
        .LineColor = Colors.Gray
    }
    SelectedSphereLinesVisual.Children.Add(verticalLineVisual3D)
    Dim x1 As Double = centerPosition.X - size.X * 0.5
    Dim x2 As Double = centerPosition.X + size.X * 0.5
    Dim xLineVisual3D = New Ab3d.Visuals.LineVisual3D() With {
        .StartPosition = New Point3D(x1, wireBoxBottom, selectedPositionData.Position.Z),
        .EndPosition = New Point3D(x2, wireBoxBottom, selectedPositionData.Position.Z),
        .LineThickness = 2,
        .LineColor = Colors.Gray
    }
    SelectedSphereLinesVisual.Children.Add(xLineVisual3D)
    Dim z1 As Double = centerPosition.Z - size.Z * 0.5
    Dim z2 As Double = centerPosition.Z + size.Z * 0.5
    Dim zLineVisual3D = New Ab3d.Visuals.LineVisual3D() With {
        .StartPosition = New Point3D(selectedPositionData.Position.X, wireBoxBottom, z1),
        .EndPosition = New Point3D(selectedPositionData.Position.X, wireBoxBottom, z2),
        .LineThickness = 2,
        .LineColor = Colors.Gray
    }
    SelectedSphereLinesVisual.Children.Add(zLineVisual3D)
  End Sub

  Private Sub UpdateSelectedSpheresData()
    If DataListBox.SelectedItems Is Nothing OrElse DataListBox.SelectedItems.Count = 0 Then
      SelectionDataTextBlock.Text = "No spheres selected" & vbCrLf
      ClearSelectionButton.IsEnabled = False
      Return
    End If

    Dim totalSize As Double = DataListBox.SelectedItems.OfType(Of SphereDataView)().Sum(Function(d) d.OriginalSphereData.Size)
    SelectionDataTextBlock.Text = String.Format("Count: {0}" & vbCrLf & "Total size: {1:0.0}", DataListBox.SelectedItems.Count, totalSize)
    ClearSelectionButton.IsEnabled = True
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
    ExportToImage()
  End Sub
End Class


Public Class UserControlAb3DViewModel



End Class


'SphereData contains raw data about each sampled data
Public Class SphereData
  Public Property Time As Double
  Public Property Location As Point
  Public Property Size As Double

  Public Sub New(ByVal time As Double, ByVal location As Point, ByVal size As Double)
    Me.Time = time
    Me.Location = location
    Me.Size = size
  End Sub
End Class



Public Class SphereDataView
  Private _isSelected As Boolean
  Private _sphere As SphereVisual3D
  Private _selectedMaterial As DiffuseMaterial = New DiffuseMaterial(Brushes.Red)
  Public Property OriginalSphereData As SphereData
  Public Property Position As Point3D
  Public Property Color As Color
  Public Property Radius As Double
  Public Property ScreenPosition As Point

  Public ReadOnly Property ModelVisual3D As ModelVisual3D
    Get
      If _sphere Is Nothing Then _sphere = CreateSphereVisual3D()
      Return _sphere
    End Get
  End Property

  Public Property IsSelected As Boolean
    Get
      Return _isSelected
    End Get
    Set(ByVal value As Boolean)
      If _isSelected = value Then Return
      _isSelected = value
      If _sphere IsNot Nothing Then _sphere.Material = GetCurrentSphereMaterial()
      OnIsSelectedChanged()
    End Set
  End Property

  Public Event IsSelectedChanged As EventHandler

  Public Sub New(ByVal originalSphereData As SphereData, ByVal position As Point3D, ByVal color As Color, ByVal radius As Double)
    Me.OriginalSphereData = originalSphereData
    Me.Position = position
    Me.Color = color
    Me.Radius = radius
    Me.ScreenPosition = New Point(Double.NaN, Double.NaN)
  End Sub

  Public Shared Function Create(ByVal originalSphereData As SphereData, ByVal displayedDataBounds As Rect3D, ByVal maxTime As Double, ByVal xyDataRange As Rect, ByVal color As Color) As SphereDataView
    Dim x As Double = originalSphereData.Time / maxTime
    Dim y As Double = (originalSphereData.Location.Y - xyDataRange.Y) / xyDataRange.Height
    Dim z As Double = (originalSphereData.Location.X - xyDataRange.X) / xyDataRange.Width
    x = (x * displayedDataBounds.SizeX) + displayedDataBounds.X
    y = (y * displayedDataBounds.SizeY) + displayedDataBounds.Y
    z = (z * displayedDataBounds.SizeZ) + displayedDataBounds.Z
    Dim position3D As Point3D = New Point3D(x, y, z)
    Dim sphereDataView = New SphereDataView(originalSphereData, position3D, color, originalSphereData.Size)
    Return sphereDataView
  End Function

  Public Shared Function Create(Punto As clsXYZColore, displayedDataBounds As Rect3D, dataRange As Rect3D, PointSize As Double) As SphereDataView
    'Dim position3D As Point3D = New Point3D(Punto.X, Punto.Z, Punto.Y)
    Dim x As Double = (Punto.X * displayedDataBounds.SizeX / dataRange.SizeX) - (displayedDataBounds.SizeX / 2)
    Dim y As Double = (Punto.Z * displayedDataBounds.SizeZ / dataRange.SizeZ) - (displayedDataBounds.SizeZ / 2)
    Dim z As Double = (displayedDataBounds.SizeY / 2) - (Punto.Y * displayedDataBounds.SizeY / dataRange.SizeY)
    Dim position3D As Point3D = New Point3D(x, y, z)
    Dim sphereDataView = New SphereDataView(New SphereData(Punto.X, New Point(Punto.Y, Punto.Z), PointSize), position3D, Punto.Colore, PointSize)
    Return sphereDataView
  End Function

  Public Function IsInScreenRectangle(ByVal selectionRectangle As Rect) As Boolean
    If Double.IsNaN(ScreenPosition.X) Then Return False
    Return selectionRectangle.X < ScreenPosition.X AndAlso ScreenPosition.X < (selectionRectangle.X + selectionRectangle.Width) AndAlso selectionRectangle.Y < ScreenPosition.Y AndAlso ScreenPosition.Y < (selectionRectangle.Y + selectionRectangle.Height)
  End Function

  Private Function CreateSphereVisual3D() As SphereVisual3D
    Dim sphereVisual3D = New SphereVisual3D() With {
            .CenterPosition = Position,
            .Material = GetCurrentSphereMaterial(),
            .Radius = Radius
        }
    Return sphereVisual3D
  End Function

  Private Function GetCurrentSphereMaterial() As Material
    Return If(IsSelected, _selectedMaterial, New DiffuseMaterial(New SolidColorBrush(Color)))
  End Function

  Protected Sub OnIsSelectedChanged()
    RaiseEvent IsSelectedChanged(Me, Nothing)
  End Sub
End Class

