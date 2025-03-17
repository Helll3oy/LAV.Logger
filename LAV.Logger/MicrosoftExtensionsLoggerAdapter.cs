using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LAV.Logger
{
    public sealed class MicrosoftExtensionsLoggerAdapter : LoggerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public MicrosoftExtensionsLoggerAdapter(Microsoft.Extensions.Logging.ILogger logger) : base()
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void LogInternal(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            Microsoft.Extensions.Logging.EventId microsoftEventId =
                new Microsoft.Extensions.Logging.EventId(eventId.Id, eventId.Name);

            switch (level)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.Error:
                    _logger.LogError(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(microsoftEventId, exception, message, data);
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }

            //_logger.Log(level.ToMicrosoftLogLevel(), eventId, exception, message, data);

        }

        protected override async Task LogInternalAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            await Task.Run(() => LogInternal(level, eventId, exception, message, data)).ConfigureAwait(false);
        }
    }
}
