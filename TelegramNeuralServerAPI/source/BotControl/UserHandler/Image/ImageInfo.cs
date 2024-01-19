using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class ImageInfo(string name, string? description = null)
	{
		public string Name = name;
		public string? Description = description;
		public string? RawDescription { get => Description?.Replace("<pre>", "").Replace("</pre>", ""); }
		public readonly Dictionary<int, PersonProcess> derivedImages = [];
		public bool IsValid { get => isValid; set => isValid = value; }
		private bool isValid = true;

		public void TryAdd(string? newString)
		{
			if (newString != null) { Description += $"{newString}\n\n"; }
		}
	}
}
