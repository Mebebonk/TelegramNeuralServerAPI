using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal static class EnumExtender
	{
		private static CommandAttribute GetCommandAttribute(this Enum value)
		{
			var attributes = value.GetType().GetField(value.ToString())!.GetCustomAttributes(typeof(CommandAttribute), false);
			if (attributes.Length != 0)
				return (attributes.First() as CommandAttribute)!;

			throw new Exception("No CommandAttribute found");
		}

		public static string GetCommand(this Enum value)
		{
			return value.GetCommandAttribute().Command;
		}

		public static string GetDescription(this Enum value)
		{
			return value.GetCommandAttribute().Description;
		}
		public static string GetCommandText(this Enum value)
		{
			return "/" + value.GetCommandAttribute().Command;
		}
	}
}
