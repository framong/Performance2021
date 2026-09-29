Imports SciChart.Charting.Visuals

Class Application
	Public Shared LF As LoadingForm

	Public Sub New()

		'If Not clsEzriz.ValidLicense() Then
		'	'Clipboard.SetText(clsEzriz.GetHdwId)
		'	'MsgBox("This Software needs a valid license to run, please use this code: " & clsEzriz.GetHdwId & " to request a valid one." & vbCrLf & "Screenshots will be ignored, please send the code as text" & vbCrLf & vbCrLf & "OK to close and copy it into the Clipboard", vbOKOnly)
		'	End
		'End If

		LF = New LoadingForm
		LF.Topmost = True
		LF.Show()
	End Sub

	Public Shared Sub CloseLoadingForm()
		LF.Close()
	End Sub

	Private Sub Application_Startup(sender As Object, e As StartupEventArgs)
		'SciChartSurface.SetRuntimeLicenseKey("LEAjrfDhFZkTFljHEuXoHCQ1jak3G9CGu+P+UWmhu9BgzlehDKBILz8SXu7tM6ZjDfRjO7ZesGYFV9uBcc4BRlYLidWF7vKUhsvvsFJAgQ71JhU/R2fLSX7OxMf2i6yqoQ4GwO8Skk7uhX4KmbYvxquOiKycNOoy3a9Lnd7VLaRFW7h/1+d/eHCmeSsE7Hx1hpmmgKeWKs2zlr9RkCGNIYhV/vCxgnbA/+Wcjctjnt8Svt9dIe84Uvwgx5NEhGG+AkKsqRDIKnO4VYCE+nQKL5r12Cuh5Gre9gb0pxUe/es/7K+rR3WRa0ij1BZGd7dgjcaJvCeUeJwRKYa1jjOiRUnyRG5NLLsl0dP68TgqjK/bDeveTvl8n29OR/m4mSQkLG94wAVdbGkNUEWJdo5lUhr3OwY9KMab6lx6H60/AfPI1rkJyVFfWM94TZe34lvs7iq00Y5p4YOgwx9cWmV2PfDHcoT6tGbcKXm+KDUmtkq1KJOcq2Ch")
	End Sub

	' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
	' can be handled in this file.


	Protected Overrides Sub OnStartup(e As StartupEventArgs)
		MyBase.OnStartup(e)
		'	'  SciChartSurface.SetRuntimeLicenseKey("<LicenseContract>
		'	'  <Customer>Paranna ditta Individuale</Customer>
		'	'  <OrderId>ABT190510-9577-91118</OrderId>
		'	'  <LicenseCount>1</LicenseCount>
		'	'  <IsTrialLicense>false</IsTrialLicense>
		'	'  <SupportExpires>01/14/2020 00:00:00</SupportExpires>
		'	'  <ProductCode>SC-WPF-2D-PRO</ProductCode>
		'	'  <KeyCode>lwAAAQEAAADTxU++BFTVAXkAQ3VzdG9tZXI9UGFyYW5uYSBkaXR0YSBJbmRpdmlkdWFsZTtPcmRlcklkPUFCVDE5MDUxMC05NTc3LTkxMTE4O1N1YnNjcmlwdGlvblZhbGlkVG89MTQtSmFuLTIwMjA7UHJvZHVjdENvZGU9U0MtV1BGLTJELVBSTx0CmPNDFghy262Kg1C7gMDa8RLxPAeoMnwKow/foHxE4C6Sut5xxf+6zyvTiIsEyw==</KeyCode>
		'	'</LicenseContract>")


		'	'// Set this code once in App.xaml.cs Or application startup
		'	'SciChartSurface.SetRuntimeLicenseKey("j5fmkHqdmPCDWgZaIr9tvAlj0agnnF/4fkuG5rf0DJRdcMO11iINVJyM9TzKiIRUNEwycyyfthPHExspCjIdBvJGuVnkxD/IxuRMoykqOAuKnekrguvdKoYAYXSEfXcdFlRRjqv07PeYygOdr28nwanZibDbSLTqXf8CizHk/w85477qisGlCV93MYE9GAQXR8U3dZxKKecT+R4IpXjyp+v7kpeJsYPgSVuFTZTi5XcYKxF9zurkE1ZJz8vbcdCBKO42uR6FuHuL4S5ppT+rwyUuwwos8smCdMKFc8uzLUusYN78vhc6s3K6ntpNtP/12KQbJ52qX8lXmtnrDhCCJfaeKfJb6/rE9mjvKocBqv+MpJBzYxgQe+5qj8M/qV8cH9ZYUrTyY3kSoqferG/i2FJppdMpSEUI57jTG9esx18RQAyo46+FBet3HK4tS36WUrOtrcuzE5dkJrp5J7v2rlUYtLatOd+DRFi+wFZzB8tUxtu1gj9p")


		'	'Set this code once in App.xaml.cs Or application startup
		'	'SciChartSurface.SetRuntimeLicenseKey("Owm7oHVhb0NgCXdK310qygZr0+T5ztX0R9QOyDq6UG5xPEqjPNj4RtXy+w5NfPR/1twwLPqR2K+hjm8n744JBf0Kg7HgdOKRwVU8cUuGvFWKhqIo0FS6/L9qpqC4L8d61jYCOLlF9+KprKiAz3VQpbcrT41AYuECPQPA0yB762I31FP7xOuhxAdHk5veRftfJs0brdzmcIb0KEQ/jyBR++452x5MZXGBLflUh6BAXISR8fka9SzWIdG2xAZGM6bz6Id4k33xQdCeFmYBnrYGdD5kuPTZUXCS7JEpGGk1+JDafz9k6kSVWR3wJ3wPx7N1YhzzGUTXhEok6aD9sFylRSztVIsrkwvCTeUuQFO6NDKNF0wSPoLIRhol7r8hWZqVgn7c7cFQGk6Muy5S9WQWNyhLzC7NTrAkqUFy6RWzAJnkhoR6P1F4YwshFE4oFFwc1r7uQYz+PfN5swf7Ftq6aKaoGbm44+XYA0bTYaBE4YDNueWzAz66")

		'	''// Set this code once in App.xaml.cs Or application startup
		'	'SciChartSurface.SetRuntimeLicenseKey("eIR2xbqY5el6SqWzQmWeR495efv5UK4B1XUDR4waQ2YhHZJWFhDyZCq5JepGHFhHNtVTUXoQ2ARB3uUoyZ4dAqL9oWDeRbgjG80q7JlORzOytZk2ts69bWrpg0vEU2DOdnRgML4LeLnGWyC1n++VqdLkyMtvJOeW4OXTQQTj3bVKPqCf522TtBK/adT5zk6GbA/O7uUOTU8ZXA8j50CoHb1FqmQ2dC9CUIOCcp7VF6a0p61/KIRApRgdvr0KgnRV9zYanptOIUIwsFjYecncVYrzVzpHgllSuzpglfJUfMnQ2t1hZuzYVqyIfwJI7qhvZYOY6w8g6dRe2mc97emx0VEJcMmt6mEloIrFjImGe4FceafkmeWA0/Qrc2DPmhABED/F0OUsokrv/SZ8c/9GnvNjw4NDFzjlkMFZzzTq7Eq2LM6U7h84AiuM9umc0kL/0GrA2iSka3kE7YtV7hZ3o7dwmsJKiGLd6RJ5et8GdSfnlN4/appj")


		'// Set this code once in App.xaml.cs Or application startup
		'SciChartSurface.SetRuntimeLicenseKey("LEAjrfDhFZkTFljHEuXoHCQ1jak3G9CGu+P+UWmhu9BgzlehDKBILz8SXu7tM6ZjDfRjO7ZesGYFV9uBcc4BRlYLidWF7vKUhsvvsFJAgQ71JhU/R2fLSX7OxMf2i6yqoQ4GwO8Skk7uhX4KmbYvxquOiKycNOoy3a9Lnd7VLaRFW7h/1+d/eHCmeSsE7Hx1hpmmgKeWKs2zlr9RkCGNIYhV/vCxgnbA/+Wcjctjnt8Svt9dIe84Uvwgx5NEhGG+AkKsqRDIKnO4VYCE+nQKL5r12Cuh5Gre9gb0pxUe/es/7K+rR3WRa0ij1BZGd7dgjcaJvCeUeJwRKYa1jjOiRUnyRG5NLLsl0dP68TgqjK/bDeveTvl8n29OR/m4mSQkLG94wAVdbGkNUEWJdo5lUhr3OwY9KMab6lx6H60/AfPI1rkJyVFfWM94TZe34lvs7iq00Y5p4YOgwx9cWmV2PfDHcoT6tGbcKXm+KDUmtkq1KJOcq2Ch")


		'// Set this code once in App.xaml.cs Or application startup
		'//		SciChartSurface.SetRuntimeLicenseKey("eIR2xbqY5el6SqWzQmWeR495efv5UK4B1XUDR4waQ2YhHZJWFhDyZCq5JepGHFhHNtVTUXoQ2ARB3uUoyZ4dAqL9oWDeRbgjG80q7JlORzOytZk2ts69bWrpg0vEU2DOdnRgML4LeLnGWyC1n++VqdLkyMtvJOeW4OXTQQTj3bVKPqCf522TtBK/adT5zk6GbA/O7uUOTU8ZXA8j50CoHb1FqmQ2dC9CUIOCcp7VF6a0p61/KIRApRgdvr0KgnRV9zYanptOIUIwsFjYecncVYrzVzpHgllSuzpglfJUfMnQ2t1hZuzYVqyIfwJI7qhvZYOY6w8g6dRe2mc97emx0VEJcMmt6mEloIrFjImGe4FceafkmeWA0/Qrc2DPmhABED/F0OUsokrv/SZ8c/9GnvNjw4NDFzjlkMFZzzTq7Eq2LM6U7h84AiuM9umc0kL/0GrA2iSka3kE7YtV7hZ3o7dwmsJKiGLd6RJ5et8GdSfnlN4/appj")
		' // Set this code once in App.xaml.cs Or application startup
		'SciChartSurface.SetRuntimeLicenseKey("OS8MbtLCraKrtoAZGV0elSXt5gYFjBV+0iPXr9X53JzQwwQWX72JtnXTocLZHtoXd7D8VbzFalfIGe4VkDTuNxp4pdQ7bHm+k8G3J+57+QJ99Tdjfn74Oy9Lu/Kls0CrvLm0NJxyzq6pxmF102ZOPGpumvLHvWev9aRwPVExzaw8B1v2o4sFWgjQ2OpT36g9cxsfdSOk6gtTCk2pEcFTIRZubAiHskd3PwupOP3kgcOat41qmqwWbSYe2zbESi69zoSkpboxa2A1qgPSpv5goNn9M6c4huEYXlF8VqJkROE5/imaCpgjQp6dBYTRGnCEzEdN3O0+G90/YBSyD+/H1LuqN7d4WmOmLLQBmcmcoqITsRjQ0lvdQ8/ChIgUMrnIojnqb3JQqHC40HIm/e6lPv+S+h0mwFh8mVDGf4BXLnCOJYuBZP4jaGdx2sX7qZ93dYvXVarcIcJSpdCN39ycy/tyDr4jmx1MVP2WGr3bsv6v0qRFurRQ")
		'SciChartSurface.SetRuntimeLicenseKey("gea9msxRSwbtq9EYy/amB6s+moRwOLC3HiYNLOHs5K7YdMy/NeDHvM+bEnDboiTCMDTYGePlf7WmNDXq0QjaVc0YiU6CwLg7KLsbcbJ3YqkEdWQY0pTKGGhoUqWyOkO/jrFjHs1TR37n9zo4hyTZC2Rn8tGwxMpa9U2iB/C4g5DxSgfJY2KhUq3NK4TiBmdnP/OJd3p75eY/7krUZcstZIvk0OSixQMKGTRmH7AdsOkOI9sWvF+vsoVG+iNEfesUPZPlhn3OJZfUOOkcj4k+yBqFnuJIMpIl9HWxpfvTx0x3X2hUMyq6lIH/sjnMszkPCllXHJnt/OoXLIuFVFKo21njc2bgnks+TKVwd4JBKE29E4tRTIVGtwNzu/3PkwntPgYZwzbJNHdbkOTBYkznSIdviHocsUHHr3N0/7dvTkM4E3SQOwVYCdCclAQUAfqqDIUH8qDH9sa0mnz6Xla9xYYbqP/XMBXKArEmEXR9statWl/cJUsA")
		'SciChartSurface.SetRuntimeLicenseKey("795tuKbqrzdN/tHiMeEfpYjOd/f1mz0/TK2O2E4FLM3ivcm8yvWVnczmxUIBF56PvR70vnRVORajoIY9islGvqDIqfD7KyZGlPAw3PFK8bhU8UcREqiacSZhPM/fDuXPVMOZulGe/EN/+Kk2QBDqZmH2gdlS04FpTdWhPCLld83Ke5WMyZmQIFDm10sBVRKpqSX6+5K7QZ3jcKVPpJGZc+ys2uqQ7bz1ZNypG4HtyycL3rVdEKc2K6ehvNrGuOFCp7Igs1UotQvrn1LdQjuS2bWC7Nu/Ch13S7FViUQoD5xNcIoX39x0102Yq8LUo6VcjTi/B5JjH5WB8SHxAg0/QR1waq92bJR+ZgL8/SLidGBer4AgZw54CqFQiCfTeiCF+dOeD8vYUf6L3xng51pst2MX4DWlHdmXt1KlhNpHwDRSagM+/EhkEQbThMQxHr8NvQCj13NFTWs0/xdUQKyPJnhmsiT4R2ngPxLtisGWuMeUL52+vERQsQ6oQukb/Z53pPpy7B8w/ktvxBifaQUuDDwzCzBkgch5WqSuknO0tBWubAlcbaNJ4CgyeodG2DzjGEiOgQaN")

		'// Set this code once in App.xaml.cs Or application startup
		SciChartSurface.SetRuntimeLicenseKey("aodaxhfFj36xEX2rV6jR2VPj6hmE528WAy5lgjSSL+DA/0fu2IjnFSeWRwqrU0ZB+56il6MfiU0QPL/U7A9sAfGezQc49VJ/sEA5H89Elu6qg8krdlbtLEwQ2svcFq43i6P5qifzOU0o4z758869MtPME6O/QFmdf4Wqfk1ZF+oSI/hqkTvji8oKTyRAP1Re9lA8Eftmx7YRZBq2l1A9rHwLB22sGnS26uMM4Nia/UaAIRz7RDJhlSI6FwzqV/vU4NsxMcDtOIXn/F6w6ADlc7SxCCS6ctuRLzxeXW0ihRYnIpKCBlD9wELjoUqeVYtYkYBt/lp5zTfUytTEj7rnmMATXaplADpsjSj8FWso0XdP7Tf2+87hiimSovF82NbqTNzGf0f7A13+UwzsPFfewvdoFeM/xUppveoKiHVnBPKTLmvS3ez4XMwrNkWLbRJBxSUhh5H/OzNnFX2stv/5v3LM8j3lsPD4IPyHGqS4vGOrTokRqnfS")

		Ab3d.Licensing.PowerToys.LicenseHelper.SetLicense("Francesco Mongelli", "SingleDeveloperLicense", "70F1-B320-6CB5-92E0-0E61-B63B-39C2-3A5A-FAB7-094D-B9D1-182E-9BB9-BFDF-EE60-F286")

	End Sub


End Class
