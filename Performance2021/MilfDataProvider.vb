Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.Http.Formatting

Public Class MilfDataProvider

  Public Async Function GetChannelsValues(request As LoadChannelsListDTO) As Task(Of Dictionary(Of String, Double()))
    Dim _baseURL As String = "http://10.0.90.170:5050/"


    Using Client As New HttpClient
      Client.BaseAddress = New Uri(_baseURL)
      Client.DefaultRequestHeaders.Accept.Clear()
      Client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/octet-stream"))
      Client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
      Dim requestUri As String = $"{_baseURL}v1/middleware/channelsValuesInterval"

      Try
        Dim getTask = Await Client.PostAsJsonAsync(Of LoadChannelsListDTO)(requestUri, request)
        If getTask.IsSuccessStatusCode Then
          Return Await getTask.Content.ReadAsAsync(Of Dictionary(Of String, Double()))
        Else
          Return Nothing
        End If
      Catch ex As Exception
        Return Nothing
      End Try
    End Using
  End Function

End Class

Public Class LoadChannelsListDTO

  Public Property TimeChannelName As String = "SystemTime_DaySeconds"
  Public Property IdSourceName As String
  Public Property ChanneslName As List(Of String)
  Public Property StartSeconds As Double
  Public Property EndSeconds As Double
End Class
