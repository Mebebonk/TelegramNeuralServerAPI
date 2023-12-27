using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalRequest(LocalImage[] images, string[] unitTypes)
	{
		[JsonInclude]
		public LocalImage[] images = images;

		[JsonInclude]
		public string[] unitTypes = unitTypes;
	}
}
