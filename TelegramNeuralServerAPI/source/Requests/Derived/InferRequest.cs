using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	[method: JsonConstructor]
	internal class InferRequest(LocalImage[] images, string[] unitTypes) : BaseRequest(images, "/infer")
	{
		[JsonInclude]		
		public string[] unitTypes = unitTypes;
	}
}
