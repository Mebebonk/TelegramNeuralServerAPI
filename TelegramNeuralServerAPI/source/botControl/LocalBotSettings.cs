using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class LocalBotSettings
	{
		public NeuralProcess[] processes;

	}

	public enum NeuralProcess
	{
		Face_Detector,
		Fitter,
		Age_Estimator,
		Gender_Estimaror,
		Emotion_Estimator

	}
}
