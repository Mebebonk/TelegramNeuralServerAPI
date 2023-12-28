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

				PhotoSize? photo = message.Photo?.Last();

				if (photo == null) { return; }

				await RealisePhoto();

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
		private async Task RealisePhoto()
		{
			try
			{
				string fileId = update.Message!.Photo!.Last().FileId;

				var user = await userCfg;
				user.images.Add(fileId);
			}
			catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
		}
		private async Task RealiseCommand()
		{
			string newCommand = update.Message!.Text!.Replace("/", "");

			switch (newCommand)
			{
				case "launch":

					LocalUserConfig user = await userCfg;
					List<LocalImage> localImages = [];

					foreach (var image in user.images)
					{
						using MemoryStream stream = new();
						await botClient.DownloadFileAsync(image, stream);

						using Mat mat = new();
						CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);
						
						using Image<Rgb, byte> img = mat.ToImage<Rgb, byte>();						
						localImages.Add(new((ushort)img.Width, (ushort)img.Height, (byte)img.NumberOfChannels, Convert.ToBase64String(img.Bytes)));
						
					}

					string a = await requestHandler.LaunchProcess(new([.. localImages], ProcessConverter.ConvertBytesToStrings(user.simpleProcessess)));

					await botClient.SendTextMessageAsync(update.Message.From!.Id, a, cancellationToken: cancellationToken);

					user.images.Clear();
					return;

				case "settings":
					await botClient.SendPollAsync(update.Message.From!.Id, "Choose processess:", ProcessConverter.simplePollAnswers, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);

					return;

				case "help":
					//TODO: SUDU
					return;

			}

		}

	}
}
