using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class RecognizeRequest(LocalImage[] images, LocalImage referenceImage, float verifyThreshold = 100f, string urlMod = "/recogize") : BaseRequest(images, urlMod)
	{
		[JsonInclude]
		public LocalImage referenceImage = referenceImage;

		[JsonInclude]
		public float verifyThreshold = verifyThreshold;
	}
}
