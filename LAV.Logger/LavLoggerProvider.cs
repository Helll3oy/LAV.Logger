using System;
using System.Collections.Concurrent;
using LAV.Logger.Formatters;
using LAV.Logger.Transports;


#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
#endif
using Microsoft.Extensions.Logging;

namespace LAV.Logger
{
    public sealed class LavLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        private readonly ITransport[] _transports;
        private readonly ConcurrentDictionary<string, LavLogger> _loggers = new();

#if NET452
        private readonly CustomScopeProvider _scopeProvider = new CustomScopeProvider();
#else
        private readonly Microsoft.Extensions.Logging.IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
#endif

        public LavLoggerProvider(LogFormat format, LogFormatOptions options, params ITransport[] transports)
        {
            //_formatter = format switch
            //{
            //    LogFormat.Json => new JsonFormatter(),
            //    LogFormat.Logfmt => new LogfmtFormatter(),
            //    LogFormat.Cef => new CefFormatter(options.Vendor, options.Product, options.Version),
            //    _ => new AnsiFormatter()
            //};

            _transports = transports;
        }

        public LavLoggerProvider(params ITransport[] transports)
        {
            _transports = transports;
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new LavLogger(name, _scopeProvider, _transports));
        }

        public void Dispose() => _loggers.Clear();
    }
}
