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
/*              var photoId = photo.FileId;
				var filePath = await botClient.GetFileAsync(photoId, cancellationToken);

				MemoryStream stream = new();

				await botClient.DownloadFileAsync(filePath.FilePath!, stream, cancellationToken);

				Mat mat = new();
				CvInvoke.Imdecode(stream.ToArray(), Emgu.CV.CvEnum.ImreadModes.Color, mat);

				var img = mat.ToImage<Rgb, byte>();
				img.Draw(new Rectangle(0, 0, img.Size.Width / 2, img.Size.Height / 2), new Rgb(System.Drawing.Color.Aqua));

				FileStream file = new("testLul.txt", FileMode.Create);

				StreamWriter stream1 = new(file);
				stream1.Write(Convert.ToBase64String(stream.ToArray()));
				var success = CvInvoke.Imwrite("test.png", img);
*/