using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LAV.Logger
{
    /// <summary>
    /// Заглушка для отключения логирования
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", 
        Justification = "<Ожидание>")]
    public class NullLogger : LoggerBase
    {
        private static readonly Lazy<NullLogger> _instance = new Lazy<NullLogger>(() => new NullLogger());
        public static NullLogger Instance => _instance.Value;

        public NullLogger()
        {
        }

        public override void LogTrace(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogTraceAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        public override void LogDebug(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogDebugAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        public override void LogInformation(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogInformationAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        public override void LogWarning(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogWarningAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        public override void LogCritical(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogCriticalAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        public override void LogError(EventId eventId, Exception exception, string message, params object[] data) { }
        public override Task LogErrorAsync(EventId eventId, Exception exception, string message, params object[] data) => default;

        protected override void LogInternal(LogLevel level, EventId eventId, Exception exception, string message, params object[] data) { }
        protected override Task LogInternalAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data) => default;
    }
}
