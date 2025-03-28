// See https://aka.ms/new-console-template for more information
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using LAV.AnsiConsole;
using LAV.Logger;
using LAV.Logger.Formatters;
using Microsoft.Extensions.Logging;
using LAV.Logger.Transports;
using Microsoft.Extensions.Logging.Console;

//LAV.Logger.ILogger logger = new AnsiConsoleLogger();

//await logger.LogInformationAsync(null, "This is an informational message.");
//await logger.LogErrorAsync(new Exception("Something went wrong!"), "An error occurred.");
//await logger.LogWarningAsync(null, "---------------------- WARNING ---------------------- {0}", "TEST");
//await logger.LogDebugAsync(null, "---------------------- DEBUG ----------------------");
//await logger.LogTraceAsync(null, "---------------------- TRACE ----------------------");
//await logger.LogCriticalAsync(null, "---------------------- FATAL ----------------------");

//using var loggerFactory = LoggerFactory.Create(builder =>
//{
//    builder.AddConsole();
//    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
//});

//var microsoftLogger = loggerFactory.CreateLogger<Program>();
//logger = new MicrosoftExtensionsLoggerAdapter(microsoftLogger);

//await logger.LogInformationAsync(new LAV.Logger.EventId(), null, "This is an informational message.");
//await logger.LogErrorAsync(new LAV.Logger.EventId(), new Exception("Something went wrong!"), "An error occurred.");
//await logger.LogWarningAsync(new LAV.Logger.EventId(), null, "---------------------- WARNING --------------------");
//await logger.LogDebugAsync(new LAV.Logger.EventId(), null, "---------------------- DEBUG ---------------------- {Text}", "TEST");
//await logger.LogTraceAsync(new LAV.Logger.EventId(), null, "---------------------- TRACE ----------------------");
//await logger.LogCriticalAsync(new LAV.Logger.EventId(), null, "---------------------- FATAL ----------------------");


//var gelpOptions = GelfFormatterOptions.Default;

// Start TCP and UDP servers in parallel
//var tcpServer = StartTcpServer(gelpOptions.ServerPort);
//var udpServer = StartUdpServer(gelpOptions.Port);

//Console.WriteLine($"Servers running on port {gelpOptions.ServerPort}");

using var lavLoggerFactory = LoggerFactory.Create(builder =>
{
    ILogFormatter gelfFormatter = new GelfFormatter();

    builder.AddProvider(new LavLoggerProvider(
        new StreamTransport(new StreamTransportOptions
        {
            Formatters = [
                new AnsiFormatter(), 
                new CefFormatter(CefFormatterOptions.Default), 
                new LogfmtFormatter(), 
                new JsonFormatter()],
            EnableCompression = false,
            Timeout = Timeout.InfiniteTimeSpan
        }),
        new UdpTransport(new UdpTransportOptions
        {
            Formatters = [gelfFormatter],
            ServerHost = "tools-01.foms.local",
            ServerPort = 1514,
            EnableCompression = false,
            Timeout = Timeout.InfiniteTimeSpan
        }),
        new TcpTransport(new TcpTransportOptions
        {
            Formatters = [gelfFormatter],
            ServerHost = "tools-01.foms.local",
            ServerPort = 1514,
            EnableCompression = false,
            Timeout = Timeout.InfiniteTimeSpan
        })
        ,
        new WindowsEventTransport(new WindowsEventTransportOptions
        {
            Timeout = Timeout.InfiniteTimeSpan
        })
    ));

    //builder.AddProvider(new LavLoggerProvider(
    //    format: LogFormat.Gelf,
    //    options: new LogFormatOptions
    //    {
    //        Product = "TestProduct",
    //        Vendor = "TestVendor",
    //        Version = "1.0.0",
    //        Host = "api-server-01",
    //        Source = "PaymentGateway",
    //        EnableCompression = true
    //    }));
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
});

//EventLogInitializer.RegisterEventSource();

var lavLogger = lavLoggerFactory.CreateLogger<Program>();

//// Usage with scopes
//using (lavLogger.BeginScope("OUTER SCOPE"))
//{
//    lavLogger.LogInformation("This is an info message");

//    using (lavLogger.BeginScope("INNER SCOPE"))
//    {
//        lavLogger.LogWarning("This is a warning!");
//    }
//}

//Action<Microsoft.Extensions.Logging.ILogger, string, Exception> _apiError =
//        LoggerMessage.Define<string>(
//            Microsoft.Extensions.Logging.LogLevel.Critical,
//            new Microsoft.Extensions.Logging.EventId(100, "ApiError"),
//            AnsiConsole.ApplyStyle(s => s.ApplyForegroundColor(Color.Red)).AddText("API Error: {Endpoint}").GetAnsiString());

try
{
    throw new ApplicationException("Application throw this error!");
}
catch (Exception ex)
{
    lavLogger.LogError(LavLogEvents.Critical, ex, "Critical error occurred");
}

lavLogger.LogTrace(LavLogEvents.Info, "---------------------- TRACE ----------------------");
lavLogger.LogDebug(LavLogEvents.Debug, "---------------------- DEBUG ---------------------- ");

//_apiError(lavLogger, "endpoint", new Exception("test"));

using (lavLogger.BeginScope("Transaction:1234"))
{
    lavLogger.LogInformation(LavLogEvents.Info, "Processing payment for {Amount}", 99.99m);
}

Console.ReadLine();

//static async Task StartTcpServer(int port)
//{
//    var listener = new TcpListener(IPAddress.Any, port);
//    listener.Start();

//    try
//    {
//        while (true)
//        {
//            var client = await listener.AcceptTcpClientAsync();
//            _ = HandleTcpClientAsync(client); // Handle client in separate task
//        }
//    }
//    finally
//    {
//        listener.Stop();
//    }
//}

//static async Task HandleTcpClientAsync(TcpClient client)
//{
//    using (client)
//    using (var stream = client.GetStream())
//    {
//        var buffer = new byte[1024];

//        try
//        {
//            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
//            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//            Console.WriteLine($"TCP Received: {request}");

//            string response = "Hello from TCP server!";
//            byte[] responseData = Encoding.UTF8.GetBytes(response);
//            await stream.WriteAsync(responseData, 0, responseData.Length);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"TCP Error: {ex.Message}");
//        }
//    }
//}

//static async Task StartUdpServer(int port)
//{
//    using (var udpClient = new UdpClient(port))
//    {
//        try
//        {
//            while (true)
//            {
//                var result = await udpClient.ReceiveAsync();
//                string request = Encoding.ASCII.GetString(result.Buffer);
//                Console.WriteLine($"UDP Received: {request} from {result.RemoteEndPoint}");

//                string response = "Hello from UDP server!";
//                byte[] responseData = Encoding.ASCII.GetBytes(response);
//                await udpClient.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"UDP Error: {ex.Message}");
//        }
//    }
//}

[EventLogSource()]
public enum ApplicationCategories
{
    [EventCategory(1001, "Application Startup")]
    Startup,

    [EventCategory(1002, "Security Operations")]
    SecurityEvent,

    [EventCategory(1003, "Database Transactions")]
    DatabaseOperation
}