using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalImage(byte width, byte height, byte channels, string data)
	{
		[JsonInclude]
		public byte width = width;
		[JsonInclude]
		public byte height = height;
		[JsonInclude]
		public byte channels = channels;
		[JsonInclude]
		public string data = data;
		[JsonInclude]
		public string depthType = "uint8_t";
	}
}
