using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal static class ProcessConverter
	{

		public static readonly string[] simplePollAnswers =
			[
				"FACE_DETECTOR",
				"FITTER",
				"AGE_ESTIMATOR",
				"GENDER_ESTIMATOR",
				"EMOTION_ESTIMATOR",
				"MASK_ESTIMATOR",
				"EYE_OPENNESS_ESTIMATOR",
				"LIVENESS_ESTIMATOR",
				"GLASSES_ESTIMATOR"				
			];

		public static short ConvertPollToBytes(int[] answers)
		{
			short tmp = 0b_0000_0000_0000_0011;
			foreach (int answer in answers)
			{
				tmp |= (short)(1 << answer);
			}

			return tmp;
		}

		public static string[] ConvertBytesToStrings(short binary)
		{
			List<string> strings = [];
			short bytes = binary;

			for (short i = 0; i < simplePollAnswers.Length; i++)
			{
				if ((bytes & 1) == 1)
				{
					strings.Add(simplePollAnswers[i]);
				}
				bytes >>= 1;
			}

			return [.. strings];
		}
	}
}
