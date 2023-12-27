using CustomSettingsGenerator;
using SettingsGenerator;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramNeuralServerAPI;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;


namespace TelegramNeuralServerAPI
{
	[CustomJsonSettings("settings")]
	public class TelegramApi
	{
		[SaveLoad]
		private readonly string _token = "";
		private readonly ITelegramBotClient _botClient;
		private readonly ReceiverOptions _receiverOptions;

		private readonly HttpRequestHandler _requestHandler;
		private readonly UserData _userData = new();

		public TelegramApi(IAPIHelper? helper = null)
		{
			this.LoadSettings();

			if (string.IsNullOrEmpty(_token)) { helper?.InformNoToken(); Environment.Exit(0); };
			_requestHandler = new(helper);

			_botClient = new TelegramBotClient(_token);
			_receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.Message], ThrowPendingUpdates = true };
		}

		public async Task LaunchBot()
		{
			using var cts = new CancellationTokenSource();


			_botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

			var me = await _botClient.GetMeAsync();
			await LocalBotFunctional.CreateCommands(_botClient, cts.Token);

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
							await new LocalBotUpdate(botClient, update, _userData.GetUser(update.Message!.From!.Id), _requestHandler, cancellationToken).RealiseMessage();

							return;
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