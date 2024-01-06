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
					if (user.images.Count == 0) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "No images found!", cancellationToken: cancellationToken); return; }
					string response = "";

					{
						List<LocalImage> localImages = [];
						List<KeyValuePair<string, Image<Rgb, byte>>> images = [];

						foreach (var image in user.images)
						{
							Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

							using MemoryStream stream = new();
							await botClient.DownloadFileAsync(file.FilePath!, stream);

							using Mat mat = new();
							CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

							Image<Rgb, byte> img = mat.ToImage<Rgb, byte>();						

							//TODO: ext
							//string ext = file.FilePath!.Split(".").Last();
							//MemoryStream ms = new(CvInvoke.Imencode(ext, img));							

							localImages.Add(new((ushort)img.Width, (ushort)img.Height, (byte)img.NumberOfChannels, Convert.ToBase64String(img.Bytes)));
							images.Add(new(file.FilePath!.Split("/").Last(), img));
						}

						//response = await requestHandler.LaunchProcess(new([.. localImages], ProcessConverter.ConvertBytesToStrings(user.simpleProcessess)));
						response = await requestHandler.LaunchProcess(new([.. images.Select((a)=> LocalImage.LocalFromImage(a.Value))], ProcessConverter.ConvertBytesToStrings(user.simpleProcessess)));
					}
					JsonDocument json = JsonDocument.Parse(response);

					var jsonRoot = json.RootElement;
					var result = jsonRoot.GetProperty("result");
					var array = result.EnumerateArray().Last();
					var data = array.GetProperty("data");
					
					var parsedArray = data.Deserialize<Dictionary<string, object>[]>()!;

					{
						using FileStream file = new("response.txt", FileMode.Create);
						using StreamWriter writer = new(file);
						writer.Write(response);
					}

					//TODO: botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(), caption: , cancellationToken: cancellationToken);
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
