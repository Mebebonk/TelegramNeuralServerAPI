using CustomSettingsGenerator;
using SettingsGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	[CustomJsonSettings("RequestSettings")]
	internal class HttpRequestHandler
	{
		static readonly HttpClient client = new();
		[SaveLoad]
		private readonly string _url = "";
		public HttpRequestHandler(IAPIHelper helper)
		{
			this.LoadSettings();
			if (String.IsNullOrEmpty(_url)) { helper?.InformNoUrl(); Environment.Exit(0); }
		}

		public string LaunchProcess(LocalRequest localRequest)
		{
			using HttpRequestMessage request = new(HttpMethod.Post, _url + "/infer");

			string parsedImageList = JsonSerializer.Serialize(localRequest);
			request.Content = new StringContent(parsedImageList);

			HttpResponseMessage response = client.Send(request);
			string? returnString = response.Content.ToString();

			return returnString == null ? "response fail" : returnString!;
		}
	}
}
