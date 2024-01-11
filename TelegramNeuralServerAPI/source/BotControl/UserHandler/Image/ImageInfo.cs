using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class ImageInfo(string name, string? description = null)
	{
		public readonly string Name = name;
		public string? Description = description;
		public string? RawDescription { get => Description?.Replace("<pre>", "").Replace("</pre>", ""); }

		public void TryAdd(string? newString)
		{
			if (newString != null) { Description += $"{newString}\n\n"; }
		}
	}
}
