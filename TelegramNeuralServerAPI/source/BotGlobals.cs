﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal static class BotGlobals
	{
		public const string launchfaceProcessCommandName = "launch_face_process";
		public const string faceProcessSettingsCommandName = "face_process_settings";

		public const string launchReIdCommandName = "launch_re_id";
		public const string launchReIdTestCommandName = "launch_re_id_test";

		public const string launchRecognizeCommandName = "launch_recognize";

		public const string flushCommandName = "flush";
		public const string showQueueCommandName = "show_queue";

		public const string helpCommandName = "help";
		public const string helpText = $"1 - Send image files you want to process\n2 - Select processess \"/{faceProcessSettingsCommandName}\"\n 3 - /{launchfaceProcessCommandName} process and wait\nOnly selected options will show up\nSettings saved automaticaly (per bot session)";
	}
}

