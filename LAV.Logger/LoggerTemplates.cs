using System.Drawing;


namespace LAV.Logger
{
    public static partial class LoggerTemplates
    {
        public const string TRACE_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [TRCE] {1}";
        public const string TRACE_FORMAT_ANSI = "[48;2;0;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;0;0;0;38;2;0;255;255mTRCE[0m] ";
        //public static readonly string TRACE_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Trace));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Trace));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Trace));
        //    })
        //    .AddText($"{LogLevel.Trace}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();

        public const string DEBUG_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [DBUG] {1}";
        public const string DEBUG_FORMAT_ANSI = "[48;2;0;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;0;0;0;38;2;0;139;139mDBUG[0m] ";
        //public static readonly string DEBUG_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Debug));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Debug));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Debug));
        //    })
        //    .AddText($"{LogLevel.Debug}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();

        public const string INFO_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [INFO] {1}";
        public const string INFO_FORMAT_ANSI = "[48;2;0;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;0;0;0;38;2;0;128;0mINFO[0m] ";
        //public static readonly string INFO_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Information));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Information));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Information));
        //    })
        //    .AddText($"{LogLevel.Information}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();

        public const string WARN_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [WARN] {1}";
        public const string WARN_FORMAT_ANSI = "[48;2;0;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;0;0;0;38;2;255;255;0mWARN[0m] ";
        //public static readonly string WARN_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Warning));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Warning));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Warning));
        //    })
        //    .AddText($"{LogLevel.Warning}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();

        public const string ERROR_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [FAIL] {1}";
        public const string ERROR_FORMAT_ANSI = "[48;2;139;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;139;0;0;38;2;0;0;0mFAIL[0m] ";
        //public static readonly string ERROR_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Error));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Error));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Error));
        //    })
        //    .AddText($"{LogLevel.Error}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();

        public const string FATAL_FORMAT = "[{0:yyyy-MM-dd HH:mm:ss.fff}] [CRIT] {1}";
        public const string FATAL_FORMAT_ANSI = "[48;2;139;0;0;38;2;147;112;219m[{0:yyyy-MM-dd HH:mm:ss.fff}][0m [[48;2;139;0;0;38;2;255;255;255mCRIT[0m] ";
        //public static readonly string FATAL_FORMAT_ANSI = LAV.AnsiConsole.AnsiConsole
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Critical));
        //        a.ApplyForegroundColor(Color.MediumPurple);
        //    })
        //    .AddText("[{0:yyyy-MM-dd HH:mm:ss.fff}]")
        //    .AddSimpleText(" [")
        //    .ApplyStyle(a =>
        //    {
        //        a.ApplyBackgroundColor(AnsiConsoleLogger.GetBackgroundColor(LogLevel.Critical));
        //        a.ApplyForegroundColor(AnsiConsoleLogger.GetForegroundColor(LogLevel.Critical));
        //    })
        //    .AddText($"{LogLevel.Critical}")
        //    .AddSimpleText("] {1}")
        //    .GetAnsiString();
    }
}