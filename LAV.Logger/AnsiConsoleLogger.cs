using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using LAV.AnsiConsole;

namespace LAV.Logger
{
    public sealed class AnsiConsoleLogger : LoggerBase
    {
        public AnsiConsoleLogger(IJsonSerializer jsonSerializer = null) : base(jsonSerializer) { }

        internal static Color GetForegroundColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => Color.Cyan,
                LogLevel.Debug => Color.DarkCyan,
                LogLevel.Information => Color.Green,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Error => Color.Black,
                LogLevel.Critical => Color.White,
                _ => Color.White
            };
        }

        internal static Color GetBackgroundColor(LogLevel level)
        {
            return level switch
            {
                //LogLevel.Trace => Color.Yellow,
                //LogLevel.Debug => Color.DarkBlue,
                //LogLevel.Information => Color.Green,
                //LogLevel.Warning => Color.YellowGreen,
                LogLevel.Error => Color.DarkRed,
                LogLevel.Critical => Color.DarkRed,
                _ => Color.Black
            };
        }

        public override void LogTrace(EventId eventId, Exception exception, string message, params object[] data)
        {
            //base.LogTrace(eventId, exception, message, data);
            Console.WriteLine(LoggerTemplates.TRACE_FORMAT_ANSI, DateTime.Now, message);
        }

        protected override void LogInternal(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            //LAV.AnsiConsole.AnsiConsole
            //    .ApplyStyle(a =>
            //    {
            //        a.ApplyBackgroundColor(GetBackgroundColor(level));
            //        a.ApplyForegroundColor(Color.MediumPurple);
            //    })
            //    .AddText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]")
            //    .AddSimpleText(" [")
            //    .ApplyStyle(a =>
            //    {
            //        a.ApplyBackgroundColor(GetBackgroundColor(level));
            //        a.ApplyForegroundColor(GetForegroundColor(level));
            //    })
            //    .AddText($"{level}")
            //    .AddSimpleText("] ")
            //    .AddSimpleText(message)

            //    .WriteLine();

            switch (level)
            {
                case LogLevel.Trace:
                    Console.WriteLine(LoggerTemplates.TRACE_FORMAT_ANSI, DateTime.Now, message);
                    break;
                case LogLevel.Debug:
                    Console.WriteLine(LoggerTemplates.DEBUG_FORMAT_ANSI, DateTime.Now, message);
                    break;
                case LogLevel.Information:
                    Console.WriteLine(LoggerTemplates.INFO_FORMAT_ANSI, DateTime.Now, message);
                    break;
                case LogLevel.Warning:
                    Console.WriteLine(LoggerTemplates.WARN_FORMAT_ANSI, DateTime.Now, message);
                    break;
                case LogLevel.Error:
                    Console.WriteLine(LoggerTemplates.ERROR_FORMAT_ANSI, DateTime.Now, message);
                    break;
                case LogLevel.Critical:
                    Console.WriteLine(LoggerTemplates.FATAL_FORMAT_ANSI, DateTime.Now, message);
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }

            //if (data?.Length > 0)
            //{
            //    Console.ForegroundColor = ConsoleColor.DarkYellow;

            //    Console.WriteLine("\t{0}", data);

            //    //var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(
            //    //            JsonSerializer.SerializeObject(data));

            //    //Console.WriteLine();
            //    //foreach (var item in properties)
            //    //{
            //    //    Console.WriteLine($"\t{item.Key}: {item.Value}");
            //    //}
            //}

            //if (exception != null)
            //{
            //    Console.ForegroundColor = GetForegroundColor(LogLevel.Error);
            //    Console.BackgroundColor = GetBackgroundColor(LogLevel.Error);
            //    Console.WriteLine(exception);
            //}
        }

        protected override async Task LogInternalAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            await Task.Run(() => LogInternal(level, eventId, exception, message, data)).ConfigureAwait(false);
        }
    }
}
