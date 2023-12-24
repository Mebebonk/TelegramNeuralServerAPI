using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramNeuralServerAPI;

namespace CLIServer
{
	internal class CLIAPIHelper : IAPIHelper
	{
		public void InformNoToken()
		{
			Console.WriteLine("No token found!");
			Console.Read();
		}
	}
}
