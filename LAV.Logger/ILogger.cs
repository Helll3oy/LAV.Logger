using System;
using System.Threading.Tasks;

namespace LAV.Logger
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        None,
    }

    public interface ILogger
    {
        void Log(LogLevel level, Exception exception, string message, params object[] data);
        Task LogAsync(LogLevel level, Exception exception, string message, params object[] data);

        //void Log(LogLevel level, string message, Exception exception);
        //Task<bool> LogAsync(LogLevel level, string message, Exception exception = null, IReadOnlyDictionary<string, object> properties = null);

        void Trace(Exception exception, string message, params object[] data);
        Task TraceAsync(Exception exception, string message, params object[] data);

        void Debug(Exception exception, string message, params object[] data);
        Task DebugAsync(Exception exception, string message, params object[] data);

        void Info(Exception exception, string message, params object[] data);
        Task InfoAsync(Exception exception, string message, params object[] data);

        void Warn(Exception exception, string message, params object[] data);
        Task WarnAsync(Exception exception, string message, params object[] data);

        void Error(Exception exception, string message, params object[] data);
        Task ErrorAsync(Exception exception, string message, params object[] data);

        void Fatal(Exception exception, string message, params object[] data);
        Task FatalAsync(Exception exception, string message, params object[] data);
    }
}
