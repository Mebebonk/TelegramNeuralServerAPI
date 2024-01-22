using CustomSettingsGenerator;
using SettingsGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace TelegramNeuralServerAPI
{
	[CustomJsonSettings("RequestSettings")]
	internal class HttpRequestHandler
	{
		static readonly HttpClient client = new();
		static readonly JsonSerializerOptions options = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
		[SaveLoad]
		private readonly string _url = "";
		public HttpRequestHandler(IAPIHelper? helper = null)
		{
			this.LoadSettings();
			if (String.IsNullOrEmpty(_url)) { helper?.InformNoUrl(); Environment.Exit(0); }
		}

		public async Task<string> LaunchProcess<T>(T localRequest) where T : BaseRequest
		{
			using HttpRequestMessage request = new(HttpMethod.Post, _url + localRequest.urlMod);

			string parsedImageList = JsonSerializer.Serialize(localRequest, options);

			request.Content = new StringContent(parsedImageList, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

			HttpResponseMessage response = await client.SendAsync(request);
			string? returnString = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode) { throw new($"Response error: {returnString}"); }

			return returnString ?? throw new("response fail!");
		}
	}
}