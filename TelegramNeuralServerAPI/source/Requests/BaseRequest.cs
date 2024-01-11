using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	abstract internal class BaseRequest(LocalImage[] images, string urlMod)
	{
		[JsonInclude]
		public LocalImage[] images = images;

		public string urlMod = urlMod;
	}
}
