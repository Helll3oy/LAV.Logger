using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LAV.Logger
{
    public interface ILogger
    {
        void Log(LogLevel level, EventId eventId, Exception exception, string message, params object[] data);
        Task LogAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data);

        //void Log(LogLevel level, string message, Exception exception);
        //Task<bool> LogAsync(LogLevel level, string message, Exception exception = null, IReadOnlyDictionary<string, object> properties = null);

        void LogTrace(Exception exception, string message, params object[] data);
        void LogTrace(EventId eventId, Exception exception, string message, params object[] data);
        Task LogTraceAsync(Exception exception, string message, params object[] data);
        Task LogTraceAsync(EventId eventId, Exception exception, string message, params object[] data);

        void LogDebug(Exception exception, string message, params object[] data);
        void LogDebug(EventId eventId, Exception exception, string message, params object[] data);
        Task LogDebugAsync(Exception exception, string message, params object[] data);
        Task LogDebugAsync(EventId eventId, Exception exception, string message, params object[] data);

        void LogInformation(Exception exception, string message, params object[] data);
        void LogInformation(EventId eventId, Exception exception, string message, params object[] data);
        Task LogInformationAsync(Exception exception, string message, params object[] data);
        Task LogInformationAsync(EventId eventId, Exception exception, string message, params object[] data);

        void LogWarning(Exception exception, string message, params object[] data);
        void LogWarning(EventId eventId, Exception exception, string message, params object[] data);
        Task LogWarningAsync(Exception exception, string message, params object[] data);
        Task LogWarningAsync(EventId eventId, Exception exception, string message, params object[] data);

        void LogError(Exception exception, string message, params object[] data);
        void LogError(EventId eventId, Exception exception, string message, params object[] data);
        Task LogErrorAsync(Exception exception, string message, params object[] data);
        Task LogErrorAsync(EventId eventId, Exception exception, string message, params object[] data);

        void LogCritical(Exception exception, string message, params object[] data);
        void LogCritical(EventId eventId, Exception exception, string message, params object[] data);
        Task LogCriticalAsync(Exception exception, string message, params object[] data);
        Task LogCriticalAsync(EventId eventId, Exception exception, string message, params object[] data);
    }
}
