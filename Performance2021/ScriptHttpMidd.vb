Imports System.Net.Http;
Imports System.Threading.Tasks;
Imports System.Net.Http.Headers;
Imports System.Collections.Generic

Namespace MilfWare
  Public Class MilfWareDataProvider

    Public Function GetChannelsValues() As Dictionary(Of String, Double())
      Dim _baseURL As String = "http://10.0.90.170:5050/"


      'Using (client = New HttpClient())
      '            {
      '                client.BaseAddress = New Uri(_baseURL);
      '                client.DefaultRequestHeaders.Accept.Clear();
      '                client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/octet-stream"));
      '                client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"));

      '                var requestUri = $"{_baseURL}v1/middleware/channelsValuesInterval";
      '                Try
      '                {
      '                    var getTask = await client.PostAsJsonAsync<ILoadChannelsListDTO>(requestUri, request);
      '                    If (getTask.IsSuccessStatusCode) Then
      '                                {
      '                        var data = await getTask.Content.ReadAsAsync<Dictionary<String, Double[]>>();

      '                        Return data;
      '                    }
      '                    Else
      '      Throw New ApplicationException(String.Format("Unable to GetChanneslValues Data {0}", getTask.ReasonPhrase));
      '                }
      '                Catch (HttpRequestException ex)
      '                {
      '                    If (System.Diagnostics.Debugger.IsAttached) Then
      '      System.Diagnostics.Debugger.Break();
      '                    Throw;
      '                }
      '            }


    End Function

  End Class

End Namespace


'Public async Task<double[]> GetChanneValues(string sourceName, string channelName)
'        {
'          var _baseURL="http://10.0.90.170:5050/";
'            try
'            {
'                double[] data = new double[11];

'                if (!string.IsNullOrEmpty(_baseURL))
'                    using (var client = new HttpClient())
'                    {
'                    data = new double[99];
'                        client.BaseAddress = new Uri(_baseURL);
'                        client.DefaultRequestHeaders.Accept.Clear();
'                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

'                        var requestUri = $"{_baseURL}v1/middleware/channelValues/{sourceName}/{channelName}";
'                        try
'                        {
'                            var getTask = await client.GetAsync(requestUri);
'                            if (getTask.IsSuccessStatusCode)
'                            {                                              
'                                data =  await getTask.Content.ReadAsAsync<double[]>();
'                            }
'                            else
'                                throw new ApplicationException(string.Format("Unable to get ChannelValues {0}, {1}", channelName, getTask.ReasonPhrase));
'                        }
'                        catch (Exception ex)
'                        {
'                            //_logger.Error(ex, "MiddlewareApiClient:GetChanneValues");
'                            if (System.Diagnostics.Debugger.IsAttached)
'                                System.Diagnostics.Debugger.Break();
'                            //if (Regex.IsMatch(failoverServerUrl, urlRegEx))
'                            //    logger.Error("Error retrieving license info from server {0}. Trying fail-over server {1}. Error: {2}", primaryServerUrl, failoverServerUrl, ex);
'                            //else
'                            data = new double[9];
'                            throw;
'                        }

'                    }
'                return data;
'            }
'            catch
'            {
'                return null;
'            }
'        }




'Public async Task<Dictionary<string, double[]>> GetChanneslValues(ILoadChannelsListDTO request)
'        {
'			 var _baseURL="http://10.0.90.170:5050/";
'			 //_baseURL="http://192.168.160.5:5050/";
'Using (var client = New HttpClient())
'            {
'                client.BaseAddress = New Uri(_baseURL);
'                client.DefaultRequestHeaders.Accept.Clear();
'                client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/octet-stream"));
'                client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"));

'                var requestUri = $"{_baseURL}v1/middleware/channelsValuesInterval";
'                Try
'                {
'                    var getTask = await client.PostAsJsonAsync<ILoadChannelsListDTO>(requestUri, request);
'                    If (getTask.IsSuccessStatusCode)
'                    {
'                        var data = await getTask.Content.ReadAsAsync<Dictionary<String, Double[]>>();

'                        Return data;
'                    }
'                    Else
'Throw New ApplicationException(String.Format("Unable to GetChanneslValues Data {0}", getTask.ReasonPhrase));
'                }
'                Catch (HttpRequestException ex)
'                {
'                    If (System.Diagnostics.Debugger.IsAttached)
'System.Diagnostics.Debugger.Break();
'                    Throw;
'                }
'            }
'        }