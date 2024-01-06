using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Dai;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Emgu.CV.Features2D;

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

		private async Task BuildImages(LocalUserConfig user, Dictionary<string, Image<Rgb, byte>> images)
		{
			foreach (var image in user.images)
			{
				Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

				using MemoryStream stream = new();
				await botClient.DownloadFileAsync(file.FilePath!, stream);

				using Mat mat = new();
				CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

				Image<Rgb, byte> img = mat.ToImage<Rgb, byte>();

				images.Add(new(file.FilePath!.Split("/").Last(), img));
			}

		}
		private async Task RealiseCommand()
		{
			string newCommand = update.Message!.Text!.Replace("/", "");

			switch (newCommand)
			{
				case "launch":

					LocalUserConfig user = await userCfg;

					if (user.images.Count == 0) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "No images found!", cancellationToken: cancellationToken); return; }

					string response = "";
					Dictionary<string, Image<Rgb, byte>> images = [];

					await BuildImages(user, images);
					response = await requestHandler.LaunchProcess(new([.. images.Select((a) => new LocalImage(a.Value))], ProcessConverter.ConvertBytesToStrings(user.simpleProcessess)));
					

					JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

					if (array.Count() != images.Count) { throw new("count missmatch"); }

					foreach (JsonElement imageData in array)
					{
						JsonElement data = imageData.GetProperty("data");
						var parsedArray = data.Deserialize<Dictionary<string, object>[]>()!;
					}

					//{
					//	using FileStream file = new("response.txt", FileMode.Create);
					//	using StreamWriter writer = new(file);
					//	writer.Write(response);
					//}

					foreach (var img in images)
					{
						MemoryStream imgMs;

						try
						{
							imgMs = new(CvInvoke.Imencode("." + img.Key.Split(".").Last(), img.Value));
						}
						catch (Emgu.CV.Util.CvException)
						{
							imgMs = new(CvInvoke.Imencode(".png", img.Value));
						}
						await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), caption: "caption", cancellationToken: cancellationToken);

						img.Value.Dispose();
						imgMs.Dispose();
					}

					user.images.Clear();
					return;

				case "settings":
					await botClient.SendPollAsync(update.Message.From!.Id, "Choose processess:", ProcessConverter.simplePollAnswersHR, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);

					return;

				case "help":
					//TODO: SUDU
					return;

			}

		}

	}
}
