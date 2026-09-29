Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.Http.Formatting

Public Class MilfDataProvider

End Class

Public Class LoadChannelsListDTO

  Public Property TimeChannelName As String = "SystemTime_DaySeconds"
  Public Property IdSourceName As String
  Public Property ChanneslName As List(Of String)
  Public Property StartSeconds As Double
  Public Property EndSeconds As Double
End Class
