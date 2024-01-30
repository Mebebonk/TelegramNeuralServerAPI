using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalImage(ushort width, ushort height, byte channels, string data)
	{
		[JsonInclude]
		public ushort width = width;
		[JsonInclude]
		public ushort height = height;
		[JsonInclude]
		public byte channels = channels;
		[JsonInclude]
		public string data = data;
		[JsonInclude]
		public string depthType = "uint8_t";

		public LocalImage(Image<Rgb, byte> img) : this((ushort)img.Width, (ushort)img.Height, (byte)img.NumberOfChannels, Convert.ToBase64String(img.Mat.GetRawData())) { }

	}
}
