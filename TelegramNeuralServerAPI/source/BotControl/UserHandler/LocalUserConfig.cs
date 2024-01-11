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
		public List<string> images = [];
		public short faceProcessess = 0b_0000_0000_0000_0000;
		public byte bodyProcessess = 0b_0000_0000;		
		public int? lastPollMessageId = null;
	}
}
