using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramNeuralServerAPI
{
	internal class UserData()
	{
		private readonly SemaphoreSlim _locker = new(1);
		private readonly List<LocalUserConfig> _users = [];

		public async Task<LocalUserConfig> GetUser(Telegram.Bot.Types.User from)
		{
			await _locker.WaitAsync();
			try
			{
				LocalUserConfig? user = _users.Find((a) => a.UserId == from.Id);

				if (user == null)
				{
					user = new(from);
					_users.Add(user);
				}
				return user;
			}
			finally
			{
				_locker.Release();
			}
		}
	}
}