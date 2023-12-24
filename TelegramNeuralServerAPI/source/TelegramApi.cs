using CustomSettingsGenerator;
using SettingsGenerator;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramNeuralServerAPI;


namespace TelegramNeuralServerAPI
{
	[CustomJsonSettings("settings")]
	public class TelegramApi
	{
		[SaveLoad]
		private readonly string _token = "";
		private readonly ITelegramBotClient _botClient;

		private readonly ReceiverOptions _receiverOptions;

		public TelegramApi(IAPIHelper? helper = null)
		{
			this.LoadSettings();

			if (string.IsNullOrEmpty(_token)) { helper?.InformNoToken(); Environment.Exit(0); };

			_botClient = new TelegramBotClient(_token);
			_receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.Message], ThrowPendingUpdates = true };
		}

		public async Task LaunchBot()
		{
			using var cts = new CancellationTokenSource();


			_botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

			var me = await _botClient.GetMeAsync();
			Console.WriteLine($"{me.FirstName} запущен!");

			await Task.Delay(-1);
		}

		private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{

			try
			{
				switch (update.Type)
				{
					case UpdateType.Message:
						{
							try
							{
								var msg = update.Message;
								var message = update.Message;

								var photos = message!.Photo;
								if (photos == null) { Errors.NoPhotoFoundError(botClient, update, cancellationToken); return; }

								var photo = photos.Last();
								if (photo == null) { Errors.NoPhotoFoundError(botClient, update, cancellationToken); return; }

								var photoId = photo.FileId;
								var filePath = await botClient.GetFileAsync(photoId, cancellationToken);

								MemoryStream stream = new();

								await botClient.DownloadFileAsync(filePath.FilePath!, stream, cancellationToken);

								return;
							}
							catch (NullReferenceException ex) { Console.WriteLine(ex.ToString()); return; }
						}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
		{

			var ErrorMessage = error switch
			{
				ApiRequestException apiRequestException
					=> $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => error.ToString()
			};

			Console.WriteLine(ErrorMessage);
			return Task.CompletedTask;
		}
	}
}