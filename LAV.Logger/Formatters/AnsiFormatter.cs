using System;
using System.Drawing;
using System.Text;

namespace LAV.Logger.Formatters
{
    // ANSI Formatter (Default)
    public sealed class AnsiFormatter : ILogFormatter
    {
        private const int BUFFER_SIZE = 1420;

        private static Color GetForegroundColor(Microsoft.Extensions.Logging.LogLevel level)
        {
            return level switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => Color.Cyan,
                Microsoft.Extensions.Logging.LogLevel.Debug => Color.DarkCyan,
                Microsoft.Extensions.Logging.LogLevel.Information => Color.Green,
                Microsoft.Extensions.Logging.LogLevel.Warning => Color.Yellow,
                Microsoft.Extensions.Logging.LogLevel.Error => Color.Black,
                Microsoft.Extensions.Logging.LogLevel.Critical => Color.White,
                _ => Color.White
            };
        }

        private static Color GetBackgroundColor(Microsoft.Extensions.Logging.LogLevel level)
        {
            return level switch
            {
                //LogLevel.Trace => Color.Yellow,
                //LogLevel.Debug => Color.DarkBlue,
                //LogLevel.Information => Color.Green,
                //LogLevel.Warning => Color.YellowGreen,
                Microsoft.Extensions.Logging.LogLevel.Error => Color.DarkRed,
                Microsoft.Extensions.Logging.LogLevel.Critical => Color.DarkRed,
                _ => ConsoleColorHelper.GetActualConsoleBackgroundColorRgb() ??
                    Console.BackgroundColor.ToColor()
            };
        }

        public string Format(LavLogEntry entry)
        {
            var builder = InnerCreateMessage(entry, BUFFER_SIZE * 4);
            return builder.ToString();           
        }

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = BUFFER_SIZE)
        {
            var builder = InnerCreateMessage(entry, bufferSize.Value * 4);

            char[] dest = new char[builder.Length];
            builder.CopyTo(0, dest, 0, builder.Length);

            return Encoding.UTF8.GetBytes(dest);
        }

        private static StringBuilder InnerCreateMessage(LavLogEntry entry, int bufferSize)
        {
            var output =
                AnsiConsole.AnsiConsole
                    .ApplyStyle(s =>
                    {
                        s.ApplyBackgroundColor(GetBackgroundColor(entry.Level));
                        s.ApplyForegroundColor(GetForegroundColor(entry.Level));
                    })
                    .AddText($"[{entry.Level.ToDisplayText()}] {{0:yyyy-MM-ddThh:mm:ss.fff}}Z")
                    .ApplyStyle(s =>
                    {
                        s.ApplyBackgroundColor(GetBackgroundColor(entry.Level));
                        s.ApplyForegroundColor(GetForegroundColor(entry.Level).DecreaseBrightness(0.75f));
                    })
                    .AddText(" [{1}]")
                    .ApplyStyle(s =>
                    {
                        s.ApplyForegroundColor(Color.DarkGray.DecreaseBrightness(0.75f));
                    })
                    .AddText("{2}")
                    .AddSimpleText(" {3}")
                    .ApplyStyle(s =>
                    {
                        s.ApplyBackgroundColor(GetForegroundColor(Microsoft.Extensions.Logging.LogLevel.Error));
                        s.ApplyForegroundColor(GetBackgroundColor(Microsoft.Extensions.Logging.LogLevel.Error));
                    })
                    .AddText("{4}{5}")
                    .GetAnsiString();

            return new StringBuilder(string.Format(format: output,
                    entry.Timestamp.ToUniversalTime(),
                    entry.Category,
                    entry.Scopes.Count > 0 ? " [" + string.Join(" → ", entry.Scopes) + "]" : "",
                    entry.Message,
                    entry.Exception != null ? Environment.NewLine : "",
                    entry.Exception
                    ));
        }
    }
}
