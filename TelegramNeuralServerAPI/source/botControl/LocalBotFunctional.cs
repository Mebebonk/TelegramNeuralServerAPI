using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Emgu.CV.XFeatures2D;

namespace TelegramNeuralServerAPI
{
	internal class LocalBotFunctional
	{

		public static async Task CreateCommands(ITelegramBotClient botClient, CancellationToken cancellationToken)
		{
			BotCommand[] commands =
				[
					new() { Command = "settings", Description = "View list of availiable processess" },
					new() { Command = "launch", Description = "Launch process(ess)" },
					new() { Command = "help", Description = "Get bot help" }
				];


			await botClient.SetMyCommandsAsync(commands, cancellationToken: cancellationToken);
		}

	}
}
