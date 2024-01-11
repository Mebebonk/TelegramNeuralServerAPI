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
using TelegramNeuralServerAPI;
using System.Drawing;

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

				if (photo != null) { _ = RealisePhoto(update.Message!.Photo!.Last().FileId); }

				bool? mimeImg = message.Document?.MimeType?.Contains("image");

				if (mimeImg != null && (bool)mimeImg) { _ = RealisePhoto(message.Document!.FileId); }

				return;
			}
			catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
		}

		public async Task RealiseVote()
		{
			try
			{
				LocalUserConfig user = await userCfg;
				if (user.lastPollMessageId == null) { return; }

				var poll = await botClient.StopPollAsync(update.PollAnswer!.User.Id, (int)user.lastPollMessageId!);

				user.faceProcessess = ProcessConverter.ConvertPollToFace(update.PollAnswer!.OptionIds);

				user.lastPollMessageId = null;
			}
			catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
		}
		private async Task RealisePhoto(string fileId)
		{
			var user = await userCfg;
			user.images.Add(fileId);
		}
		private async Task RealiseCommand()
		{
			string newCommand = update.Message!.Text!.Replace("/", "");

			switch (newCommand)
			{
				case BotGlobals.launchCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count == 0) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "No images found!", cancellationToken: cancellationToken); return; }

						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						string response = await requestHandler.LaunchProcess(new InferRequest([.. images.Select((a) => new LocalImage(a.Value))], ProcessConverter.ConvertFaceToStrings(user.faceProcessess)));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						if (array.Count() != images.Count) { throw new("count missmatch"); }
						BuildFaceProcessInfo(user, images, array);

						await ThrowImages(user, images);

						user.images.Clear();
					}
					return;

				case BotGlobals.launchReIdCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }

						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						string responseBodyDetector = await requestHandler.LaunchProcess(new InferRequest([.. images.Select((a) => new LocalImage(a.Value))], ["HUMAN_BODY_DETECTOR"]));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(responseBodyDetector).RootElement.GetProperty("result").EnumerateArray();
						BuildFaceProcessInfo(user, images, array);

						string response = await requestHandler.LaunchProcess(new ReIdRequest([.. images.TakeLast(images.Count - 1).Select((a) => new LocalImage(a.Value))], new(images.First().Value)));

					}
					return;
				case BotGlobals.launchRecognizeCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }
						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						string response = await requestHandler.LaunchProcess(new RecognizeRequest([.. images.TakeLast(images.Count - 1).Select((a) => new LocalImage(a.Value))], new(images.First().Value)));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

					}
					return;

				case BotGlobals.faceProcessSettingsCommandName:
					{
						var user = await userCfg;
						if (user.lastPollMessageId != null) { await botClient.StopPollAsync(update.Message.From!.Id, (int)user.lastPollMessageId); }

						var poll = await botClient.SendPollAsync(update.Message.From!.Id, "Choose face processess:", ProcessConverter.facePollAnswersHR, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);
						user.lastPollMessageId = poll.MessageId;
					}
					return;

				case BotGlobals.flushCommandName:
					{
						LocalUserConfig user = await userCfg;
						user.images.Clear();
						_ = botClient.SendTextMessageAsync(user.UserId, "Success!", cancellationToken: cancellationToken);
					}

					return;

				case BotGlobals.helpCommandName:

					_ = botClient.SendTextMessageAsync(update.Message.From!.Id, BotGlobals.helpText);

					return;

			}

		}
		private async Task PrepareImages(LocalUserConfig user, Dictionary<ImageInfo, Image<Rgb, byte>> images)
		{
			foreach (var image in user.images)
			{
				Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

				using MemoryStream stream = new();
				await botClient.DownloadFileAsync(file.FilePath!, stream);

				using Mat mat = new();

				CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

				Image<Rgb, byte> img = mat.ToImage<Rgb, byte>();

				images.Add(new(file.FilePath!.Split("/").Last()), img);
			}

		}
		private async Task ThrowImages(LocalUserConfig user, Dictionary<ImageInfo, Image<Rgb, byte>> images)
		{
			foreach (var img in images)
			{
				MemoryStream imgMs;

				try
				{
					imgMs = new(CvInvoke.Imencode("." + img.Key.Name.Split(".").Last(), img.Value));
				}
				catch (Emgu.CV.Util.CvException)
				{
					imgMs = new(CvInvoke.Imencode(".png", img.Value));
				}

				if (img.Key.Description?.Length > 1023)
				{
					await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), cancellationToken: cancellationToken);
					using MemoryStream stream = new(Encoding.ASCII.GetBytes(img.Key.RawDescription!));

					await botClient.SendDocumentAsync(user.UserId, InputFile.FromStream(stream, "result.txt"));
				}
				else
				{
					await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), caption: img.Key.Description, cancellationToken: cancellationToken, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
				}

				img.Value.Dispose();
				imgMs.Dispose();
			}
		}
		private static void BuildFaceProcessInfo(LocalUserConfig user, Dictionary<ImageInfo, Image<Rgb, byte>> images, JsonElement.ArrayEnumerator array)
		{
			int imageNumber = 0;
			foreach (JsonElement imageData in array)
			{
				int personNumber = 0;
				JsonElement data = imageData.GetProperty("data");
				var peopleData = data.EnumerateArray();
				if (!peopleData.Any()) { throw new("no data found!"); }

				foreach (JsonElement personData in peopleData)
				{
					PersonProcess person = personData.Deserialize<PersonProcess>() ?? throw new("how?..");

					Image<Rgb, byte> currentImage = images.Values.ToArray()[imageNumber];
					ImageInfo currentInfo = images.Keys.ToArray()[imageNumber];
					if (person.IsFilled())
					{
						currentInfo.TryAdd($"Person id: {personNumber}");
						person.WrappDescription(currentInfo);
					}


					int width = Math.Abs(person.faceDetector.topLeft.x - person.faceDetector.bottomRight.x);
					int height = Math.Abs(person.faceDetector.topLeft.y - person.faceDetector.bottomRight.y);

					int thickness = (int)Math.Ceiling(Math.Min(width, height) * 0.01);
					int borderThickness = (int)Math.Ceiling(thickness * 1.5);

					Rgb personColor = new(System.Drawing.Color.Yellow);
					Rgb borderColor = new(System.Drawing.Color.Black);

					int textX = person.faceDetector.topLeft.x + thickness * 2;
					int textY = person.faceDetector.bottomRight.y - thickness * 2;
					Point textPoint = new(textX, textY);

					if ((user.faceProcessess & 1) == 1) { DrawBox(person.faceDetector.topLeft, currentImage, width, height, thickness, borderThickness, personColor, borderColor); }
					if (person.fitter is not null && (user.faceProcessess & 2) == 2)
					{
						foreach (var point in person.fitter.Value.keypoints)
						{
							currentImage.Draw(new CircleF(new(point.x, point.y), thickness), new(System.Drawing.Color.Red));
						}
						currentImage.Draw(new CircleF(new(person.fitter.Value.mouth.x, person.fitter.Value.mouth.y), thickness), personColor);
						currentImage.Draw(new CircleF(new(person.fitter.Value.leftEye.x, person.fitter.Value.leftEye.y), thickness), personColor);
						currentImage.Draw(new CircleF(new(person.fitter.Value.rightEye.x, person.fitter.Value.rightEye.y), thickness), personColor);
					}

					DrawText(personNumber.ToString(), currentImage, thickness, borderThickness, personColor, borderColor, textPoint);

					personNumber++;
				}

				imageNumber++;
			}
		}

		private static void DrawText(string text, Image<Rgb, byte> currentImage, int thickness, int borderThickness, Rgb personColor, Rgb borderColor, Point point)
		{
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, borderColor, borderThickness);
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, personColor, thickness);
		}

		private static void DrawBox(Coordinate coord, Image<Rgb, byte> currentImage, int width, int height, int thickness, int borderThickness, Rgb personColor, Rgb borderColor)
		{
			currentImage.Draw(rect: new(coord.x, coord.y, width, height), borderColor, borderThickness);
			currentImage.Draw(rect: new(coord.x, coord.y, width, height), personColor, thickness);
		}
	}
}
