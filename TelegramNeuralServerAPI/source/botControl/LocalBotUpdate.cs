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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TelegramNeuralServerAPI
{
	internal class LocalBotUpdate(ITelegramBotClient botClient, Update update, Task<LocalUserConfig> userCfg, HttpRequestHandler requestHandler, CancellationToken cancellationToken)
	{
		static readonly JsonSerializerOptions personOptions = new() { IncludeFields = true };
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
					Dictionary<string, Image<Rgb, byte>> images = [];

					await PrepareImages(user, images);
					response = await requestHandler.LaunchProcess(new([.. images.Select((a) => new LocalImage(a.Value))], ProcessConverter.ConvertBytesToStrings(user.simpleProcessess)));

					JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();
					if (array.Count() != images.Count) { throw new("count missmatch"); }

					DrawProcessInfo(images, array);

					await ThrowImages(user, images);

					user.images.Clear();
					return;

				case "settings":
					_ = botClient.SendPollAsync(update.Message.From!.Id, "Choose processess:", ProcessConverter.simplePollAnswersHR, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);

					return;

				case "help":
					//TODO: SUDU
					return;

			}

		}
		private async Task PrepareImages(LocalUserConfig user, Dictionary<string, Image<Rgb, byte>> images)
		{
			foreach (var image in user.images)
			{
				Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

				using MemoryStream stream = new();
				await botClient.DownloadFileAsync(file.FilePath!, stream);

				using Mat mat = new();
				CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

				Image<Rgb, byte> img = mat.ToImage<Rgb, byte>();

				images.Add(file.FilePath!.Split("/").Last(), img);
			}

		}
		private async Task ThrowImages(LocalUserConfig user, Dictionary<string, Image<Rgb, byte>> images)
		{
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
		}
		private static void DrawProcessInfo(Dictionary<string, Image<Rgb, byte>> images, JsonElement.ArrayEnumerator array)
		{
			int i = 0;
			foreach (JsonElement imageData in array)
			{
				int personNumber = 0;
				JsonElement data = imageData.GetProperty("data");
				var peopleData = data.EnumerateArray();
				if (!peopleData.Any()) { throw new("no data found!"); }

				foreach (JsonElement personData in peopleData)
				{
					Image<Rgb, byte> currentImage = images.Values.ToArray()[i];
					Person person = personData.Deserialize<Person>(personOptions) ?? throw new("how?..");

					int width = Math.Abs(person.faceDetector.topLeft.x - person.faceDetector.bottomRight.x);
					int height = Math.Abs(person.faceDetector.topLeft.y - person.faceDetector.bottomRight.y);

					int thickness = (int)Math.Ceiling(Math.Min(width, height) * 0.01);
					int borderThickness = (int)Math.Ceiling(thickness * 1.5);

					Rgb personColor = new(System.Drawing.Color.Yellow);
					Rgb borderColor = new(System.Drawing.Color.Black);

					currentImage.Draw(rect: new(person.faceDetector.topLeft.x, person.faceDetector.topLeft.y, width, height), borderColor, borderThickness);
					currentImage.Draw(rect: new(person.faceDetector.topLeft.x, person.faceDetector.topLeft.y, width, height), personColor, thickness);

					currentImage.Draw(personNumber.ToString(), new System.Drawing.Point(person.faceDetector.topLeft.x + thickness * 2, person.faceDetector.bottomRight.y - thickness * 2), Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, borderColor, borderThickness);
					currentImage.Draw(personNumber.ToString(), new System.Drawing.Point(person.faceDetector.topLeft.x + thickness * 2, person.faceDetector.bottomRight.y - thickness * 2), Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, personColor, thickness);

					personNumber++;
				}

				i++;
			}
		}

	}
}
