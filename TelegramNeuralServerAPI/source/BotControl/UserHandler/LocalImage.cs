using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalImage(byte width, byte height, byte channels, string data)
	{
		public byte width = width;
		public byte height = height;
		public byte channels = channels;
		public string data = data;
		public string depthType = "uint8_t";
	}
}
