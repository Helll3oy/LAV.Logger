using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LAV.Logger
{
    public sealed class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }

        private static ConsoleColor GetForegroundColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => ConsoleColor.Yellow,
                LogLevel.Debug => ConsoleColor.DarkCyan,
                LogLevel.Info => ConsoleColor.Green,
                LogLevel.Warn => ConsoleColor.DarkYellow,
                LogLevel.Error => ConsoleColor.DarkRed,
                LogLevel.Fatal => ConsoleColor.Black,
                _ => ConsoleColor.White
            };
        }

        private static ConsoleColor GetBackgroundColor(LogLevel level)
        {
            return level switch
            {
                //LogLevel.Trace => ConsoleColor.Yellow,
                //LogLevel.Debug => ConsoleColor.DarkBlue,
                //LogLevel.Info => ConsoleColor.Green,
                //LogLevel.Warn => ConsoleColor.DarkYellow,
                //LogLevel.Error => ConsoleColor.Yellow,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.Black
            };
        }

        protected override void LogInternal(LogLevel level, Exception exception, string message, params object[] data)
        {
            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(" [");

            Console.ForegroundColor = GetForegroundColor(level);
            Console.BackgroundColor = GetBackgroundColor(level);
            Console.Write($"{level}");

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine($"] {message}");

            if (data?.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.WriteLine("\t{0}", data);

                //var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(
                //            JsonSerializer.SerializeObject(data));

                //Console.WriteLine();
                //foreach (var item in properties)
                //{
                //    Console.WriteLine($"\t{item.Key}: {item.Value}");
                //}
            }

            if (exception != null)
            {
                Console.ForegroundColor = GetForegroundColor(LogLevel.Error);
                Console.BackgroundColor = GetBackgroundColor(LogLevel.Error);
                Console.WriteLine(exception);
            }

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        protected override async Task LogInternalAsync(LogLevel level, Exception exception, string message, params object[] data)
        {
            await Task.Run(() => LogInternal(level, exception, message, data));
        }
    }
}
