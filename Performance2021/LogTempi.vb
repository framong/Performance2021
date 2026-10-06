Imports System.Diagnostics

''' <summary>
''' Log dei tempi per studiare i rallentamenti (apertura tab XY Plots con molti file, prefetch dei Data Plots).
''' Scrive su file (Performance2021_Tempi.log nella cartella temporanea di Windows, azzerato a ogni avvio)
''' e su Console. Ogni riga: ore, ms dall'avvio, id thread, messaggio. Thread-safe.
''' Include un rilevatore di blocchi della UI: se il thread UI non risponde per piu' di SogliaBloccoMs lo scrive.
''' </summary>
Public Class clsLogTempi

  Private Shared ReadOnly _Lock As New Object
  Private Shared ReadOnly _Cronometro As Stopwatch = Stopwatch.StartNew()
  Private Shared _Percorso As String = Nothing
  Private Shared _Timer As System.Windows.Threading.DispatcherTimer
  Private Shared _UltimoTick As Long
  Private Const IntervalloMs As Integer = 100
  Private Const SogliaBloccoMs As Integer = 300

  Public Shared ReadOnly Property Percorso As String
    Get
      If _Percorso Is Nothing Then _Percorso = IO.Path.Combine(IO.Path.GetTempPath(), "Performance2021_Tempi.log")
      Return _Percorso
    End Get
  End Property

  ''' <summary>Azzera il file e avvia il rilevatore di blocchi UI. Da chiamare una volta, sul thread UI, all'avvio.</summary>
  Public Shared Sub Avvia()
    Try
      SyncLock _Lock
        IO.File.WriteAllText(Percorso, "")
      End SyncLock
    Catch
    End Try
    Scrivi("=== avvio log, file: " & Percorso)
    _UltimoTick = _Cronometro.ElapsedMilliseconds
    _Timer = New System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal)
    _Timer.Interval = TimeSpan.FromMilliseconds(IntervalloMs)
    AddHandler _Timer.Tick, AddressOf Timer_Tick
    _Timer.Start()
  End Sub

  Private Shared Sub Timer_Tick(sender As Object, e As EventArgs)
    Dim Adesso As Long = _Cronometro.ElapsedMilliseconds
    Dim Ritardo As Long = Adesso - _UltimoTick - IntervalloMs
    _UltimoTick = Adesso
    If Ritardo > SogliaBloccoMs Then Scrivi("UI BLOCCATA per circa " & Ritardo & " ms (fino a ora)")
  End Sub

  Public Shared Sub Scrivi(Messaggio As String)
    Try
      Dim Riga As String = Now.ToString("HH:mm:ss.fff") & " +" & _Cronometro.ElapsedMilliseconds.ToString.PadLeft(8) & "ms T" &
                           Threading.Thread.CurrentThread.ManagedThreadId.ToString.PadLeft(2) & " " & Messaggio
      Console.WriteLine(Riga)
      SyncLock _Lock
        IO.File.AppendAllText(Percorso, Riga & vbCrLf)
      End SyncLock
    Catch
    End Try
  End Sub

  ''' <summary>Misura un blocco di codice: using (clsLogTempi.Misura("nome")) ... scrive inizio e durata.</summary>
  Public Shared Function Misura(Nome As String) As IDisposable
    Return New MisuraTempo(Nome)
  End Function

  Private Class MisuraTempo
    Implements IDisposable
    Private ReadOnly _Nome As String
    Private ReadOnly _Sw As Stopwatch = Stopwatch.StartNew()

    Public Sub New(Nome As String)
      _Nome = Nome
      Scrivi("> " & Nome)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
      Scrivi("< " & _Nome & " : " & _Sw.ElapsedMilliseconds & " ms")
    End Sub
  End Class

End Class
