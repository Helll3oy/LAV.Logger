// See https://aka.ms/new-console-template for more information
using LAV.Logger;
using Microsoft.Extensions.Logging;

LAV.Logger.ILogger logger = new AnsiConsoleLogger();

await logger.LogInformationAsync(null, "This is an informational message.");
await logger.LogErrorAsync(new Exception("Something went wrong!"), "An error occurred.");
await logger.LogWarningAsync(null, "---------------------- WARNING ---------------------- {0}", "TEST");
await logger.LogDebugAsync(null, "---------------------- DEBUG ----------------------");
await logger.LogTraceAsync(null, "---------------------- TRACE ----------------------");
await logger.LogCriticalAsync(null, "---------------------- FATAL ----------------------");

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
});

var microsoftLogger = loggerFactory.CreateLogger<Program>();
logger = new MicrosoftExtensionsLoggerAdapter(microsoftLogger);

await logger.LogInformationAsync(new LAV.Logger.EventId(), null, "This is an informational message.");
await logger.LogErrorAsync(new LAV.Logger.EventId(), new Exception("Something went wrong!"), "An error occurred.");
await logger.LogWarningAsync(new LAV.Logger.EventId(), null, "---------------------- WARNING ----------------------");
await logger.LogDebugAsync(new LAV.Logger.EventId(), null, "---------------------- DEBUG ---------------------- {Text}", "TEST");
await logger.LogTraceAsync(new LAV.Logger.EventId(), null, "---------------------- TRACE ----------------------");
await logger.LogCriticalAsync(new LAV.Logger.EventId(), null, "---------------------- FATAL ----------------------");

Console.ReadLine();