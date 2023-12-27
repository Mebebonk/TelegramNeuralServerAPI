using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramNeuralServerAPI
{
	internal class LocalBotUpdate(ITelegramBotClient botClient, Update update, Task<LocalUserConfig> userCfg, HttpRequestHandler requestHandler, CancellationToken cancellationToken)
	{
		
		public async Task RealiseMessage()
		{
			try
			{
				Message message = update.Message!;
				
				if (message.Text?.First() == '/') { await RealiseCommand(); return; }

				//TODO: edgecase: photo comentary '/'!!!

				var photos = message.Photo;
				if (photos == null) { return; }

				var photo = photos.Last();
				if (photo == null) { return; }

				return;
			}
			catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
		}

		private async Task RealiseCommand()
		{
			string newCommand = update.Message.Text.Replace("/","");

			switch (newCommand)
			{
				case "launch":
					{
						//TODO: check current settings and launch
						return;
					}
				case "settings":
					{
						//TODO: set per-user launch settings through poll
						return;
					}
				case "help":
					{
						//TODO: SUDU
						return;
					}
			}
		
		}

	}
}
