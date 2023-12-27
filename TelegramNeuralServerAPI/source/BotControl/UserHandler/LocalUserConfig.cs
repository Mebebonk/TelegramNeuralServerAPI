using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalUserConfig(long userId)
	{
		public long UserId { get; set; } = userId;
		public LocalImage[]? images;
		public short simpleProcessess = 0b_0000_0000_0000_0001;
		public byte specificProcessess = 0b_0000_0001;
	}
}
