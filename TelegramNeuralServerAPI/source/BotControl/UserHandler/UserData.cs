using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class UserData()
	{
		private readonly SemaphoreSlim _locker = new(1);
		private readonly List<LocalUserConfig> _users = [];

		public async Task<LocalUserConfig> GetUser(long userId)
		{
			await _locker.WaitAsync();
			try
			{
				LocalUserConfig? user = _users.Find((a) => a.UserId == userId);

				if (user == null)
				{
					user = new(userId);
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