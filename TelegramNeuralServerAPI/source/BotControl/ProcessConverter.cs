using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal static class ProcessConverter
	{

		public static readonly string[] facePollAnswers =
			[
				"FACE_DETECTOR",
				"FITTER",
				"AGE_ESTIMATOR",
				"GENDER_ESTIMATOR",
				"EMOTION_ESTIMATOR",
				"MASK_ESTIMATOR",
				"EYE_OPENNESS_ESTIMATOR",
				"LIVENESS_ESTIMATOR",
				"GLASSES_ESTIMATOR"	,
				"HUMAN_BODY_DETECTOR"
			];
		public static readonly string[] facePollAnswersHR =
			[
				"Face detector",
				"Fitter (face mesh)",
				"Age",
				"Gender",
				"Emotion",
				"Mask",
				"Eye openness",
				"Liveness (real human or not)",
				"Glasses",
				"Human body detector"
			]; 

		public static short ConvertPollToFace(int[] answers)
		{
			short tmp = 0b0000_0000_0000_0000;
			foreach (int answer in answers)
			{
				tmp |= (short)(1 << answer);
			}

			return tmp;
		}

		public static string[] ConvertFaceToStrings(short binary)
		{
			List<string> strings = [];
			short bytes = binary;

			for (short i = 0; i < facePollAnswers.Length; i++)
			{
				if ((bytes & 1) == 1)
				{
					strings.Add(facePollAnswers[i]);
				}
				bytes >>= 1;
			}
			if (strings.Contains(facePollAnswers[6]) && !strings.Contains(facePollAnswers[1])) { strings.Add(facePollAnswers[1]); }
			if (!strings.Contains(facePollAnswers[0])) { strings.Add(facePollAnswers[0]); }

			return [.. strings];
		}
	}
}
