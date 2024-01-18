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
using Emgu.CV.Ocl;
using static Emgu.CV.Dai.OpenVino;
using SettingsGenerator;
using Emgu.CV.Util;

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

						List<ExtendedImage> images = [];
						await PrepareImages(user, images);
						string response = await requestHandler.LaunchProcess(new InferRequest([.. images.Select((a) => new LocalImage(a))], ProcessConverter.ConvertFaceToStrings(user.faceProcessess)));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						if (array.Count() != images.Count) { throw new("count missmatch"); }

						ActOnArray<PersonProcess>(array, (process, imN, perN) => FaceProcess(process, imN, perN, user, images), true);

						await ThrowImages(user, images);
						user.images.Clear();
					}
					return;
				//body reidentification
				case BotGlobals.launchReIdCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }
						List<ExtendedImage> images = [];

						await PrepareImages(user, images);
						string responseBodyDetector = await requestHandler.LaunchProcess(new InferRequest([.. images.Select((a) => new LocalImage(a))], ["HUMAN_BODY_DETECTOR"]));
						JsonElement.ArrayEnumerator bodyDetectorArray = JsonDocument.Parse(responseBodyDetector).RootElement.GetProperty("result").EnumerateArray();
						if (bodyDetectorArray.Count() != images.Count) { throw new("count missmatch"); }

						List<Image<Rgb, byte>> croppedImages = [];

						ActOnArray<PersonProcess>(bodyDetectorArray, (process, imageN, personN) => CropProcess(process, imageN, personN, croppedImages, images), true);

						string response = await requestHandler.LaunchProcess(new ReIdRequest([.. croppedImages.TakeLast(images.Count - 1).Select((a) => new LocalImage(a))], new(croppedImages.First())));
						DisposeEnumerable(croppedImages);
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						ActOnArray<ReIdProcess>(array, (process, imageN) => ReIdProcess(process, imageN, images.TakeLast(images.Count - 1).ToList()));

						await ThrowImages(user, images);
						user.images.Clear();
					}
					return;
				//recognize
				case BotGlobals.launchRecognizeCommandName:
					{
						LocalUserConfig user = await userCfg;

						if (user.images.Count < 2) { _ = botClient.SendTextMessageAsync(update.Message.From!.Id, "Not enough images! Min: 2", cancellationToken: cancellationToken); return; }
						List<ExtendedImage> images = [];

						await PrepareImages(user, images);
						string response = await requestHandler.LaunchProcess(new RecognizeRequest([.. images.TakeLast(images.Count - 1).Select((a) => new LocalImage(a))], new(images.First())));
						JsonElement.ArrayEnumerator array = JsonDocument.Parse(response).RootElement.GetProperty("result").EnumerateArray();

						ActOnArray<RecognizeProcess>(array, (process, imN, perN) => RecognizeProcess(process, imN, perN, images.TakeLast(images.Count - 1).ToList()));

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

						List<ExtendedImage> images = [];
						await PrepareImages(user, images);
						if (images.Count > 1)
						{
							for (var i = 0; i <= images.Count / 10; i++)
							{
								var tmp = images.Take(new System.Range(i * 10, (i + 1) * 10));
								if (!tmp.Any()) { return; }

								await botClient.SendMediaGroupAsync(user.UserId, tmp.Select((a, it) => new InputMediaPhoto(InputFile.FromStream(TryEncodeImage(images[i + it])))));
							}
						}
						else
						{
							MemoryStream imgMs = TryEncodeImage(images[0]);

							await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), cancellationToken: cancellationToken);
							imgMs.Dispose();
						}
						DisposeEnumerable(images);
					}
					return;
				//help
				case BotGlobals.helpCommandName:

					_ = botClient.SendTextMessageAsync(update.Message.From!.Id, BotGlobals.helpText);

					return;

			}

		}


		private async Task PrepareImages(LocalUserConfig user, List<ExtendedImage> images)
		{
			List<Task<ExtendedImage>> tasks = [];
			foreach (var image in user.images)
			{
				tasks.Add(PrepareImageAsync(image));
			}
			await Task.WhenAll(tasks);

			foreach (var task in tasks)
			{
				images.Add(task.Result);
			}
		}
		private async Task<ExtendedImage> PrepareImageAsync(string image)
		{
			Telegram.Bot.Types.File file = await botClient.GetFileAsync(image, cancellationToken);

			using MemoryStream stream = new();
			await botClient.DownloadFileAsync(file.FilePath!, stream);

			using Mat mat = new();
			using Mat matRgb = new();

			CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);
			CvInvoke.CvtColor(mat, matRgb, Emgu.CV.CvEnum.ColorConversion.Rgb2Bgr);

			return new(matRgb, new(file.FilePath!.Split("/").Last()));
		}
		private async Task ThrowImages(LocalUserConfig user, List<ExtendedImage> images)
		{
			foreach (var img in images)
			{
				MemoryStream imgMs = TryEncodeImage(img);

				if (img.ImageInfo.Description?.Length > 1023)
				{
					await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), cancellationToken: cancellationToken);
					using MemoryStream stream = new(Encoding.ASCII.GetBytes(img.ImageInfo.RawDescription!));

					await botClient.SendDocumentAsync(user.UserId, InputFile.FromStream(stream, "result.txt"));
				}
				else
				{
					await botClient.SendPhotoAsync(user.UserId, InputFile.FromStream(imgMs), caption: img.ImageInfo.Description, cancellationToken: cancellationToken, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
				}

				img.Dispose();
				imgMs.Dispose();
			}
		}
		private static MemoryStream TryEncodeImage(ExtendedImage img)
		{
			MemoryStream imgMs;
			try
			{
				imgMs = new(CvInvoke.Imencode("." + img.ImageInfo.Name.Split(".").Last(), img));
			}
			catch (Emgu.CV.Util.CvException)
			{
				imgMs = new(CvInvoke.Imencode(".png", img));
			}

			return imgMs;
		}

		private static void ActOnArray<T>(JsonElement.ArrayEnumerator array, Action<T, int, int> callback, bool hasData = false)
		{
			int imageNumber = 0;
			if (!array.Any()) { throw new("no data found!"); }

			foreach (JsonElement imageData in array)
			{
				int personNumber = 0;

				JsonElement data = hasData ? imageData.GetProperty("data") : imageData;
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

		private static void FaceProcess(PersonProcess person, int imageNumber, int personNumber, LocalUserConfig user, List<ExtendedImage> images)
		{
			ExtendedImage currentImage = images[imageNumber];
			ImageInfo currentInfo = currentImage.ImageInfo;
			if (person.IsFilled())
			{
				currentInfo.TryAdd($"Person id: {personNumber}");
				person.WrappDescription(currentInfo);
			}

			ProcessAssistant assistant = new(person.boundingBox.topLeft, person.boundingBox.bottomRight);

			if ((user.faceProcessess & 1) == 1)
			{
				DrawBox(assistant, currentImage);
			}
			if (person.fitter is not null && (user.faceProcessess & 2) == 2)
			{
				foreach (var point in person.fitter.Value.keypoints)
				{
					currentImage.Draw(new CircleF(new(point.x, point.y), assistant.thickness), new(System.Drawing.Color.Red));
				}
				currentImage.Draw(new CircleF(new(person.fitter.Value.mouth.x, person.fitter.Value.mouth.y), assistant.thickness), personColor);
				currentImage.Draw(new CircleF(new(person.fitter.Value.leftEye.x, person.fitter.Value.leftEye.y), assistant.thickness), personColor);
				currentImage.Draw(new CircleF(new(person.fitter.Value.rightEye.x, person.fitter.Value.rightEye.y), assistant.thickness), personColor);
			}

			DrawText(personNumber.ToString(), currentImage, assistant.thickness, assistant.borderThickness, assistant.textPoint);
		}
		private static void ReIdProcess(ReIdProcess person, int imageNumber, List<ExtendedImage> images)
		{
			ExtendedImage currentImage = images.Find((a) => a.ImageInfo.derivedImages.ContainsKey(imageNumber))!;
			PersonProcess currentPerson = currentImage.ImageInfo.derivedImages[imageNumber];
			int i = currentImage.ImageInfo.derivedImages.Values.ToList().IndexOf(currentPerson);
			ProcessAssistant assistant = new(currentPerson.boundingBox);

			if (person.verdict)
			{
				DrawBox(assistant, currentImage);
			}

			DrawText(i.ToString(), currentImage, assistant.thickness, assistant.borderThickness, assistant.textPoint);
		}
		private static void RecognizeProcess(RecognizeProcess person, int imageNumber, int personNumber, List<ExtendedImage> images)
		{
			ExtendedImage currentImage = images[imageNumber];
			ProcessAssistant assistant = new(person.boundingBox.topLeft, person.boundingBox.bottomRight);
			if (person.verdict)
			{
				DrawBox(assistant, currentImage);
			}

			DrawText(personNumber.ToString(), currentImage, assistant.thickness, assistant.borderThickness, assistant.textPoint);
		}
		private static void CropProcess(PersonProcess person, int imageNumber, int personNumber, List<Image<Rgb, byte>> list, List<ExtendedImage> images)
		{
			ProcessAssistant assistant = new(person.boundingBox.topLeft, person.boundingBox.bottomRight);
			images[imageNumber].ImageInfo.derivedImages.Add(list.Count, person);

			using Mat mat = new(images[imageNumber].Mat, new Rectangle(assistant.topLeft.x, assistant.topLeft.y, assistant.width, assistant.height));
			list.Add(mat.ToImage<Rgb, byte>());
		}

		private static void DrawText(string text, Image<Rgb, byte> currentImage, int thickness, int borderThickness, Point point)
		{
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, borderColor, borderThickness);
			currentImage.Draw(text, point, Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0, personColor, thickness);
		}
		private static void DrawBox(ProcessAssistant assistant, Image<Rgb, byte> currentImage)
		{
			DrawBox(assistant, currentImage, personColor);
		}
		private static void DrawBox(ProcessAssistant assistant, Image<Rgb, byte> currentImage, Rgb personColor)
		{
			currentImage.Draw(rect: new(assistant.topLeft.x, assistant.topLeft.y, assistant.width, assistant.height), borderColor, assistant.borderThickness);
			currentImage.Draw(rect: new(assistant.topLeft.x, assistant.topLeft.y, assistant.width, assistant.height), personColor, assistant.thickness);
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
