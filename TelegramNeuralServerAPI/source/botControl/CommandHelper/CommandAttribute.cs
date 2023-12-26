using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
	internal class CommandAttribute(string command, string description) : Attribute
	{
		public string Command { get; set; } = command;
		public string Description { get; set; } = description;

	}
}
