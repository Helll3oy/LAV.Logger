// See https://aka.ms/new-console-template for more information
using LAV.Logger;
using Microsoft.Extensions.Logging;

LAV.Logger.ILogger logger = new ConsoleLogger();

logger.Log(LAV.Logger.LogLevel.Information, null, "This is an informational message.");
logger.Log(LAV.Logger.LogLevel.Error, new Exception("Something went wrong!"), "An error occurred.");


var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var microsoftLogger = loggerFactory.CreateLogger<Program>();
logger = new MicrosoftExtensionsLoggerAdapter(microsoftLogger);

logger.Log(LAV.Logger.LogLevel.Information, null, "This is an informational message.");
logger.Log(LAV.Logger.LogLevel.Error, new Exception("Something went wrong!"), "An error occurred.");

Console.ReadLine();