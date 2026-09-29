Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.IO
Imports System.Windows
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports Microsoft.Win32
Imports System.Windows.Controls

Public Class UserControlExportHtml
  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.

    cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(Function(f) f.Source)
    cmbFontSize.ItemsSource = New List(Of Double) From {8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}
  End Sub

  Private Sub rtbEditor_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles rtbEditor.SelectionChanged
    'Dim temp As Object = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty)
    'btnBold.IsChecked = (Not temp = DependencyProperty.UnsetValue) And (temp.Equals(FontWeights.Bold))
    'temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty)
    'btnItalic.IsChecked = (Not temp = DependencyProperty.UnsetValue) And (temp.Equals(FontStyles.Italic))
    'temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty)
    'btnUnderline.IsChecked = (Not temp = DependencyProperty.UnsetValue) And (temp.Equals(TextDecorations.Underline))
    'temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty)
    'cmbFontFamily.SelectedItem = temp
    'temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty)
    'cmbFontSize.Text = temp.ToString()
  End Sub

  Private Sub cmbFontFamily_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbFontFamily.SelectionChanged
    If Not IsDBNull(cmbFontFamily.SelectedItem) Then
      rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem)
    End If
  End Sub

  'Private Sub cmbFontSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbFontSize.SelectionChanged
  '  rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text)
  'End Sub

  'Private Sub cmbFontSize_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles cmbFontSize.SizeChanged
  '  rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text)
  'End Sub

  Private Sub CmbFontSize_TextChanged(sender As Object, e As TextChangedEventArgs)
    rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text)
  End Sub

  Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

    Dim txtRange As New TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd)
    Dim mStr As New MemoryStream
    txtRange.Save(mStr, "yy/MM/dd")

  End Sub

End Class




'Namespace WpfTutorialSamples.Rich_text_controls
'{
'	Partial Public Class RichTextEditorSample :  Window
'	{
'		Public RichTextEditorSample()
'		{
'			InitializeComponent();
'			cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
'			cmbFontSize.ItemsSource = New List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
'		}

'		Private void rtbEditor_SelectionChanged(Object sender, RoutedEventArgs e)
'		{
'			Object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
'			btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
'			temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
'			btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
'			temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
'			btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

'			temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
'			cmbFontFamily.SelectedItem = temp;
'			temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
'			cmbFontSize.Text = temp.ToString();
'		}

'		Private void Open_Executed(Object sender, ExecutedRoutedEventArgs e)
'		{
'			OpenFileDialog dlg = New OpenFileDialog();
'			dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
'			If (dlg.ShowDialog() == True)
'			{
'				FileStream fileStream = New FileStream(dlg.FileName, FileMode.Open);
'				TextRange range = New TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
'				range.Load(fileStream, DataFormats.Rtf);
'			}
'		}

'		Private void Save_Executed(Object sender, ExecutedRoutedEventArgs e)
'		{
'			SaveFileDialog dlg = New SaveFileDialog();
'			dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
'			If (dlg.ShowDialog() == True)
'			{
'				FileStream fileStream = New FileStream(dlg.FileName, FileMode.Create);
'				TextRange range = New TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
'				range.Save(fileStream, DataFormats.Rtf);
'			}
'		}

'		Private void cmbFontFamily_SelectionChanged(Object sender, SelectionChangedEventArgs e)
'		{
'			If (cmbFontFamily.SelectedItem!= null)
'				rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
'		}

'		Private void cmbFontSize_TextChanged(Object sender, TextChangedEventArgs e)
'		{
'			rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
'		}
'	}
'}