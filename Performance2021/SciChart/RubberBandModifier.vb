Imports SciChart.Charting.ChartModifiers
Imports SciChart.Core.Utility.Mouse
Imports SciChart.Data.Model

Public Class clsRubberBandModifier
  Inherits RubberBandXyZoomModifier
  Dim pTempRange As IRange

  Public Overrides Sub OnModifierMouseUp(e As ModifierMouseArgs)
    MyBase.OnModifierMouseUp(e)
    If e.IsMaster Then
      e.Handled = True
      'Dim TPVMtmp As clsTimePlotViewModel = DirectCast(Me.DataContext, clsTimePlotViewModel)
      'TPVMtmp.ObjChartSyncManager.SetCurrentVisibleRange()
    End If
  End Sub


  Public Overrides Sub OnModifierMouseDown(e As ModifierMouseArgs)
    MyBase.OnModifierMouseDown(e)
    If e.IsMaster Then
      pTempRange = Me.XAxis.VisibleRange
      e.Handled = True
    End If
  End Sub

End Class




'Public Class clsZoomExtentsModifier
'  Inherits ZoomExtentsModifier

'  Public Overrides Sub OnModifierDoubleClick(e As ModifierMouseArgs)
'    If e.IsMaster Then
'      Dim TPVMtmp As clsTimePlotViewModel = DirectCast(Me.DataContext, clsTimePlotViewModel)
'      Select Case TPVMtmp.GruppoGrafici
'        Case clsTimePlotViewModel.eGruppoGrafici.eBasic, clsTimePlotViewModel.eGruppoGrafici.eNone
'          MyBase.OnModifierDoubleClick(e)
'        Case clsTimePlotViewModel.eGruppoGrafici.eDetails
'          e.Handled = True
'          Dim BasicSync As clsTimePlotViewModel = DirectCast(MatriceControlliBase.MatriceControlli.First.DataContext, clsTimePlotViewModel)
'          TPVMtmp.ObjChartSyncManager.SharedXVisibleRange = BasicSync.ObjChartSyncManager.SharedXVisibleRange
'      End Select
'    End If
'  End Sub


'End Class