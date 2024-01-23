using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class ReIdRequest(LocalImage[] images, LocalImage referenceImage, float verifyThreshold = 80f, bool allowMultipleBodies = true) : RecognizeRequest(images, referenceImage, verifyThreshold, "/body_reidentify")
	{	
		[JsonInclude]
		public bool allowMultipleBodies = allowMultipleBodies;			
	}
}
