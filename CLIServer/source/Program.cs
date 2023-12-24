using CLIServer;
using TelegramNeuralServerAPI;


var a = new TelegramApi(new CLIAPIHelper());

a.LaunchBot().Wait();
