using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class RecognizeProcess(BoundingBox boundingBox, float distance, bool verdict)
	{
		[JsonInclude]
		[JsonPropertyName("boundingBox")]
		public BoundingBox boundingBox = boundingBox;
		[JsonInclude]
		[JsonPropertyName("distance")]
		public float distance = distance;
		[JsonInclude]
		[JsonPropertyName("verdict")]
		public bool verdict = verdict;
	}
}
