//##ScriptHttpMidd.cs
System.Diagnostics.Stopwatch stopwatch = new  System.Diagnostics.Stopwatch();
stopwatch.Start();
LoadChannelsListDTO ch = new LoadChannelsListDTO()
{ IdSourceName ="ILS0",
  ChanneslName= new List<string>(){"Bs","Tws","Awa","Twd","Twa","AccX","AccY","AccZ","Aws","Hdg","Heel","Lwy","LonBow","LonStern","LatBow","LatStern","FCS_PortCant_Ang","FCS_StbdCant_Ang"},
  StartSeconds = 55000,
  EndSeconds=64000
};

string result=string.Empty;
           var data1 =await  GetChanneslValues(ch);
           result+= $"  {data1.Values.Count} {System.Environment.NewLine}";
           
           stopwatch.Stop();
           return data1["Bs"].Length +"  IN " + stopwatch.ElapsedMilliseconds;
           
           
   