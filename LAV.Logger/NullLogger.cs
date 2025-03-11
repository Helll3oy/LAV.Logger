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

        public override void Trace(Exception exception, string message, params object[] data) { }
        public override Task TraceAsync(Exception exception, string message, params object[] data) => default;

        public override void Debug(Exception exception, string message, params object[] data) { }
        public override Task DebugAsync(Exception exception, string message, params object[] data) => default;

        public override void Info(Exception exception, string message, params object[] data) { }
        public override Task InfoAsync(Exception exception, string message, params object[] data) => default;

        public override void Warn(Exception exception, string message, params object[] data) { }
        public override Task WarnAsync(Exception exception, string message, params object[] data) => default;

        public override void Fatal(Exception exception, string message, params object[] data) { }
        public override Task FatalAsync(Exception exception, string message, params object[] data) => default;

        public override void Error(Exception exception, string message, params object[] data) { }
        public override Task ErrorAsync(Exception exception, string message, params object[] data) => default;

        protected override void LogInternal(LogLevel level, Exception exception, string message, params object[] data) { }
        protected override Task LogInternalAsync(LogLevel level, Exception exception, string message, params object[] data) => default;
    }
}
