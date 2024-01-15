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
using Emgu.CV.ImgHash;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramNeuralServerAPI
{
	internal class LocalBotUpdate(ITelegramBotClient botClient, Update update, Task<LocalUserConfig> userCfg, HttpRequestHandler requestHandler, CancellationToken cancellationToken)
	{

		static Rgb personColor = new(System.Drawing.Color.Yellow);
		static Rgb borderColor = new(System.Drawing.Color.Black);

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
				//face processess
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
				//body reidentification
				case BotGlobals.launchReIdCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }

						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						string responseBodyDetector = await requestHandler.LaunchProcess(new InferRequest([.. images.Select((a) => new LocalImage(a.Value))], ["HUMAN_BODY_DETECTOR"]));
						JsonElement.ArrayEnumerator bodyDetectorArray = JsonDocument.Parse(responseBodyDetector).RootElement.GetProperty("result").EnumerateArray();
						if (bodyDetectorArray.Count() != images.Count) { throw new("count missmatch"); }

						List<Image<Rgb, byte>> croppedImages = CropImages(images, bodyDetectorArray);
						string response = await requestHandler.LaunchProcess(new ReIdRequest([.. croppedImages.TakeLast(images.Count - 1).Select((a) => new LocalImage(a))], new(croppedImages.First())));
						DisposeEnumerable(croppedImages);
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						BuildReIdProcessInfo(images, array);

						await ThrowImages(user, images);
						user.images.Clear();
					}
					return;
				//recognize
				case BotGlobals.launchRecognizeCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }
						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						string response = await requestHandler.LaunchProcess(new RecognizeRequest([.. images.TakeLast(images.Count - 1).Select((a) => new LocalImage(a.Value))], new(images.First().Value)));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						await ThrowImages(user, images);
						user.images.Clear();
					}
					return;
				//settings
				case BotGlobals.faceProcessSettingsCommandName:
					{
						var user = await userCfg;
						if (user.lastPollMessageId != null) { await botClient.StopPollAsync(update.Message.From!.Id, (int)user.lastPollMessageId); }

						var poll = await botClient.SendPollAsync(update.Message.From!.Id, "Choose face processess:", ProcessConverter.facePollAnswersHR, allowsMultipleAnswers: true, isAnonymous: false, cancellationToken: cancellationToken);
						user.lastPollMessageId = poll.MessageId;
					}
					return;
				//clear images
				case BotGlobals.flushCommandName:
					{
						LocalUserConfig user = await userCfg;
						user.images.Clear();
						_ = botClient.SendTextMessageAsync(user.UserId, "Success!", cancellationToken: cancellationToken);
					}

					return;
				//show queue
				case BotGlobals.showQueueCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count == 0) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "No images found!", cancellationToken: cancellationToken); return; }

						Dictionary<ImageInfo, Image<Rgb, byte>> images = [];
						await PrepareImages(user, images);
						if (images.Count > 1)
						{
							for (var i = 0; i <= images.Count / 10; i++)
							{
								var tmp = images.Take(new System.Range(i * 10, (i + 1) * 10));
								if (!tmp.Any()) { return; }

								await botClient.SendMediaGroupAsync(user.UserId, tmp.Select((a, it)
									=> new InputMediaPhoto(InputFile.FromStream(
										TryEncodeImage(new(images.Keys.ToArray()[i + it], images.Values.ToArray()[i + it])),
										images.Keys.ToArray()[i + it].Name))));
							}
						}
						else
						{
							MemoryStream imgMs = TryEncodeImage(new(images.Keys.ToArray()[0], images.Values.ToArray()[0]));

							await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), cancellationToken: cancellationToken);
							imgMs.Dispose();
						}
						DisposeEnumerable(images.Select((a) => a.Value));
					}
					return;
				//help
				case BotGlobals.helpCommandName:

					_ = botClient.SendTextMessageAsync(update.Message.From!.Id, BotGlobals.helpText);

					return;

			}

		}


		private async Task PrepareImages(LocalUserConfig user, Dictionary<ImageInfo, Image<Rgb, byte>> images)
		{
			List<Task<KeyValuePair<ImageInfo, Image<Rgb, byte>>>> tasks = [];
			foreach (var image in user.images)
			{
				tasks.Add(PrepareImageAsync(image));
			}
			await Task.WhenAll(tasks);

			foreach (var task in tasks)
			{
				images.Add(task.Result.Key, task.Result.Value);
			}
		}
		private async Task<KeyValuePair<ImageInfo, Image<Rgb, byte>>> PrepareImageAsync(string image)
		{
			Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

			using MemoryStream stream = new();
			await botClient.DownloadFileAsync(file.FilePath!, stream);

			using Mat mat = new();

			CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

			return new(new(file.FilePath!.Split("/").Last()), mat.ToImage<Rgb, byte>());
		}
		private async Task ThrowImages(LocalUserConfig user, Dictionary<ImageInfo, Image<Rgb, byte>> images)
		{
			foreach (var img in images)
			{
				MemoryStream imgMs = TryEncodeImage(img);

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
		private static void ActOnArray<T>(JsonElement.ArrayEnumerator array, Action<T, int, int> callback)
		{
			int imageNumber = 0;
			if (!array.Any()) { throw new("no data found!"); }

			foreach (JsonElement imageData in array)
			{
				int personNumber = 0;
				JsonElement data = imageData.GetProperty("data");
				var peopleData = data.EnumerateArray();
				if (!peopleData.Any()) { throw new("no data found!"); }

				foreach (JsonElement personData in peopleData)
				{
					T person = personData.Deserialize<T>() ?? throw new("how?..");
					callback(person, imageNumber, personNumber);
					personNumber++;
				}

				imageNumber++;
			}
		}
		private static void ActOnArray<T>(JsonElement.ArrayEnumerator array, Action<T, int> callback)
		{
			int imageNumber = 0;
			if (!array.Any()) { throw new("no data found!"); }

			foreach (JsonElement imageData in array)
			{
				T person = imageData.Deserialize<T>() ?? throw new("how?..");
				callback(person, imageNumber);

				imageNumber++;
			}
		}
		private static MemoryStream TryEncodeImage(KeyValuePair<ImageInfo, Image<Rgb, byte>> img)
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

			return imgMs;
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

					int textX = person.faceDetector.topLeft.x + thickness * 2;
					int textY = person.faceDetector.bottomRight.y - thickness * 2;
					Point textPoint = new(textX, textY);

					if ((user.faceProcessess & 1) == 1) { DrawBox(person.faceDetector.topLeft, currentImage, width, height, thickness, borderThickness); }
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

					DrawText(personNumber.ToString(), currentImage, thickness, borderThickness, textPoint);

					personNumber++;
				}

				imageNumber++;
			}
		}
		private static void BuildReIdProcessInfo(Dictionary<ImageInfo, Image<Rgb, byte>> images, JsonElement.ArrayEnumerator array)
		{
			ActOnArray
		}

		private static List<Image<Rgb, byte>> CropImages(Dictionary<ImageInfo, Image<Rgb, byte>> images, JsonElement.ArrayEnumerator array)
		{
			List<Image<Rgb, byte>> list = [];
			int imageNumber = 0;
			foreach (JsonElement imageData in array)
			{
				JsonElement data = imageData.GetProperty("data");
				var peopleData = data.EnumerateArray();
				foreach (var personData in peopleData)
				{
					RecognizeProcess person = personData.Deserialize<RecognizeProcess>() ?? throw new("how?..");

					int width = Math.Abs(person.boundingBox.topLeft.x - person.boundingBox.bottomRight.x);
					int height = Math.Abs(person.boundingBox.topLeft.y - person.boundingBox.bottomRight.y);

					using Mat mat = new(images.Values.ToArray()[imageNumber].Mat, new Rectangle(person.boundingBox.topLeft.x, person.boundingBox.topLeft.y, width, height));
					list.Add(mat.ToImage<Rgb, byte>());
				}
				imageNumber++;
			}

			return list;
		}

		private static void DrawText(string text, Image<Rgb, byte> currentImage, int thickness, int borderThickness, Point point)
		{
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, borderColor, borderThickness);
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, personColor, thickness);
		}
		private static void DrawBox(Coordinate topLeftCoord, Image<Rgb, byte> currentImage, int width, int height, int thickness, int borderThickness)
		{
			currentImage.Draw(rect: new(topLeftCoord.x, topLeftCoord.y, width, height), borderColor, borderThickness);
			currentImage.Draw(rect: new(topLeftCoord.x, topLeftCoord.y, width, height), personColor, thickness);
		}
		private static void DisposeEnumerable<T>(IEnumerable<T> values) where T : IDisposable
		{
			foreach (T value in values)
			{
				value.Dispose();
			}
		}
	}
}
