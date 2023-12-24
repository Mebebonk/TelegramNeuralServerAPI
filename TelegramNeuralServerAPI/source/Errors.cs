using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramNeuralServerAPI
{
	internal static class Errors
	{
		static public async void NoPhotoFoundError(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) { await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "No photo(s) was found", cancellationToken: cancellationToken); }
	}
}
