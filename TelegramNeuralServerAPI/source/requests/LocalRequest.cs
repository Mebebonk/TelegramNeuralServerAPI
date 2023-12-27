using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalRequest(LocalImage[] images, string[] unitTypes)
	{
		public LocalImage[] images = images;
		public string[] unitTypes = unitTypes; 
	}
}
