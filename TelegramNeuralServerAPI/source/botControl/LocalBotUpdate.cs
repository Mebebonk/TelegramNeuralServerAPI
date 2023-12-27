using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Emgu.CV;
using Emgu.CV.Structure;

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

		public async Task RealiseVote()
		{
			try
			{				
				LocalUserConfig user = await userCfg;

				user.simpleProcessess = ProcessConverter.ConvertPollToBytes(update.PollAnswer!.OptionIds);
			}
			catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
		}

		private async Task RealiseCommand()
		{
			string newCommand = update.Message!.Text!.Replace("/", "");

			switch (newCommand)
			{
				case "launch":
					FileStream fs = new("ng.jpg", FileMode.Open);
					MemoryStream stream = new();
					fs.CopyTo(stream);


					Mat mat = new();
					CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

					var img = mat.ToImage<Rgb, byte>();

					LocalUserConfig config = await userCfg;
					string a = await requestHandler.LaunchProcess(new([new LocalImage((ushort)img.Width, (ushort)img.Height, (byte)img.NumberOfChannels, Convert.ToBase64String(img.Bytes))], ["AGE_ESTIMATOR"]));
					await botClient.SendTextMessageAsync(update.Message.From!.Id, a, cancellationToken: cancellationToken);
					fs.Dispose();
					return;

				case "settings":
					var message = await botClient.SendPollAsync(update.Message.From!.Id, "Choose processess:", ProcessConverter.simplePollAnswers, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);

					return;

				case "help":
					//TODO: SUDU
					return;

			}

		}

	}
}
