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

        protected override void LogInternal(LogLevel level, Exception exception, string message, params object[] data)
        {
            EventId eventId = new EventId();

            if (data?.Length > 0)
            {
                message += "\r\n\t{data}";
                switch (level)
                {
                    case LogLevel.Trace:
                        _logger.LogTrace(eventId, exception, message, data);
                        break;
                    case LogLevel.Debug:
                        _logger.LogDebug(eventId, exception, message, data);
                        break;
                    case LogLevel.Information:
                        _logger.LogInformation(eventId, exception, message, data);
                        break;
                    case LogLevel.Warning:
                        _logger.LogWarning(eventId, exception, message, data);
                        break;
                    case LogLevel.Error:
                        _logger.LogError(eventId, exception, message, data);
                        break;
                    case LogLevel.Critical:
                        _logger.LogCritical(eventId, exception, message, data);
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        break;
                }

                //_logger.Log(level.ToMicrosoftLogLevel(), eventId, exception, message, data);
            }
            else
            {
                switch (level)
                {
                    case LogLevel.Trace:
                        _logger.LogTrace(eventId, exception, message);
                        break;
                    case LogLevel.Debug:
                        _logger.LogDebug(eventId, exception, message);
                        break;
                    case LogLevel.Information:
                        _logger.LogInformation(eventId, exception, message);
                        break;
                    case LogLevel.Warning:
                        _logger.LogWarning(eventId, exception, message);
                        break;
                    case LogLevel.Error:
                        _logger.LogError(eventId, exception, message);
                        break;
                    case LogLevel.Critical:
                        _logger.LogCritical(eventId, exception, message);
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        break;
                }
                //_logger.Log(level.ToMicrosoftLogLevel(), eventId, exception, message);
            }
        }

        protected override async Task LogInternalAsync(LogLevel level, Exception exception, string message, params object[] data)
        {
            await Task.Run(() => LogInternal(level, exception, message, data)).ConfigureAwait(false);
        }
    }
}
