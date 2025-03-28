namespace LAV.Logger
{
    public static class LogLevelExtensions
    {
        public static int ToSyslogLevel(this Microsoft.Extensions.Logging.LogLevel level) => level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => 7,
            Microsoft.Extensions.Logging.LogLevel.Debug => 7,
            Microsoft.Extensions.Logging.LogLevel.Information => 6,
            Microsoft.Extensions.Logging.LogLevel.Warning => 4,
            Microsoft.Extensions.Logging.LogLevel.Error => 3,
            Microsoft.Extensions.Logging.LogLevel.Critical => 2,
            _ => 6
        };

        public static int ToSyslogLevel(this LogLevel level) => level switch
        {
            LogLevel.Trace => 7,
            LogLevel.Debug => 7,
            LogLevel.Information => 6,
            LogLevel.Warning => 4,
            LogLevel.Error => 3,
            LogLevel.Critical => 2,
            _ => 6
        };

        public static Microsoft.Extensions.Logging.LogLevel ToMicrosoftLogLevel(this LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
                LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
                LogLevel.None => Microsoft.Extensions.Logging.LogLevel.None,
                _ => Microsoft.Extensions.Logging.LogLevel.None
            };
        }

        public static string ToDisplayText(this Microsoft.Extensions.Logging.LogLevel level)
        {
            return level switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => "TRCE",
                Microsoft.Extensions.Logging.LogLevel.Debug => "DBUG",
                Microsoft.Extensions.Logging.LogLevel.Information => "INFO",
                Microsoft.Extensions.Logging.LogLevel.Warning => "WARN",
                Microsoft.Extensions.Logging.LogLevel.Error => "FAIL",
                Microsoft.Extensions.Logging.LogLevel.Critical => "CRIT",
                Microsoft.Extensions.Logging.LogLevel.None => "NONE",
                _ => "UNKN"
            };
        }
    }
}
