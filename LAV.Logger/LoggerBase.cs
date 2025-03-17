using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LAV.Logger
{
    public abstract class LoggerBase : ILogger
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        protected volatile bool _useAnsiColors = true;
        protected volatile LogLevel _level;
        private readonly IJsonSerializer _jsonSerializer;

        public IJsonSerializer JsonSerializer => _jsonSerializer;

        public virtual LogLevel Level
        {
            get
            {
                _lockSlim.EnterReadLock();
                try
                {
                    return _level;
                }
                finally
                {
                    _lockSlim.ExitReadLock();
                }
            }
            set
            {
                _lockSlim.EnterWriteLock();
                try
                {
                    _level = value;
                    LogWarning(LogEvents.LogLevelChanged, null, $"Уровень логирования изменен на {_level}.");
                }
                finally
                {
                    _lockSlim.ExitWriteLock();
                }
            }
        }

        protected LoggerBase() { }

        protected LoggerBase(IJsonSerializer jsonSerializer)
            : this(jsonSerializer, LogLevel.Trace) { }

        protected LoggerBase(IJsonSerializer jsonSerializer, LogLevel level)
        {
            _level = level;
            _jsonSerializer = jsonSerializer;
        }

        public virtual IDisposable StartScope<TState>(TState state) where TState : class => null;

        public virtual void Stop() { }

        protected abstract void LogInternal(LogLevel level, EventId eventId, Exception exception, string message, params object[] data);

        public void Log(LogLevel level, EventId eventId, string message, params object[] data) => Log(level, eventId, null, message, data);
        public void Log(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            if (_level > level) return;

            LogInternal(level, eventId, exception, message, data);
        }

        protected abstract Task LogInternalAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data);
        public Task LogAsync(LogLevel level, EventId eventId, string message, params object[] data) => LogAsync(level, eventId, null, message, data);
        public async Task LogAsync(LogLevel level, EventId eventId, Exception exception, string message, params object[] data)
        {
            if (_level > level) return;

            await _semaphore.WaitAsync();
            try
            {
                await LogInternalAsync(level, eventId, exception, message, data).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void LogTrace(string message, params object[] data) => LogTrace(new EventId(), null, message, data);
        public void LogTrace(Exception exception, string message, params object[] data) => LogTrace(new EventId(), exception, message, data);
        public void LogTrace(EventId eventId, string message, params object[] data) => LogTrace(eventId, null, message, data);
        public virtual void LogTrace(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Trace, eventId, exception, message, data);
        }

        public Task LogTraceAsync(string message, params object[] data) => LogTraceAsync(new EventId(), null, message, data);
        public Task LogTraceAsync(Exception exception, string message, params object[] data) => LogTraceAsync(new EventId(), exception, message, data);
        public Task LogTraceAsync(EventId eventId, string message, params object[] data) => LogTraceAsync(eventId, null, message, data);
        public virtual async Task LogTraceAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Trace, eventId, exception, message, data).ConfigureAwait(false);
        }

        public void LogDebug(string message, params object[] data) => LogDebug(new EventId(), null, message, data);
        public void LogDebug(Exception exception, string message, params object[] data) => LogDebug(new EventId(), exception, message, data);
        public void LogDebug(EventId eventId, string message, params object[] data) => LogDebug(eventId, null, message, data);
        public virtual void LogDebug(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Debug, eventId, exception, message, data);
        }

        public Task LogDebugAsync(string message, params object[] data) => LogDebugAsync(new EventId(), null, message, data);
        public Task LogDebugAsync(Exception exception, string message, params object[] data) => LogDebugAsync(new EventId(), exception, message, data);
        public Task LogDebugAsync(EventId eventId, string message, params object[] data) => LogDebugAsync(eventId, null, message, data);
        public virtual async Task LogDebugAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Debug, eventId, exception, message, data).ConfigureAwait(false);
        }

        public void LogInformation(string message, params object[] data) => LogInformation(new EventId(), null, message, data);
        public void LogInformation(Exception exception, string message, params object[] data) => LogInformation(new EventId(), exception, message, data);
        public void LogInformation(EventId eventId, string message, params object[] data) => LogInformation(eventId, null, message, data);
        public virtual void LogInformation(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Information, eventId, exception, message, data);
        }

        public Task LogInformationAsync(string message, params object[] data) => LogInformationAsync(new EventId(), null, message, data);
        public Task LogInformationAsync(Exception exception, string message, params object[] data) => LogInformationAsync(new EventId(), exception, message, data);
        public Task LogInformationAsync(EventId eventId, string message, params object[] data) => LogInformationAsync(eventId, null, message, data);
        public virtual async Task LogInformationAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Information, eventId, exception, message, data).ConfigureAwait(false);
        }

        public void LogWarning(string message, params object[] data) => LogWarning(new EventId(), null, message, data);
        public void LogWarning(Exception exception, string message, params object[] data) => LogWarning(new EventId(), exception, message, data);
        public void LogWarning(EventId eventId, string message, params object[] data) => LogWarning(eventId, null, message, data);
        public virtual void LogWarning(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Warning, eventId, exception, message, data);
        }

        public Task LogWarningAsync(string message, params object[] data) => LogWarningAsync(new EventId(), null, message, data);
        public Task LogWarningAsync(Exception exception, string message, params object[] data) => LogWarningAsync(new EventId(), exception, message, data);
        public Task LogWarningAsync(EventId eventId, string message, params object[] data) => LogWarningAsync(eventId, null, message, data);
        public virtual async Task LogWarningAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Warning, eventId, exception, message, data).ConfigureAwait(false);
        }

        public void LogError(string message, params object[] data) => LogError(new EventId(), null, message, data);
        public void LogError(Exception exception, string message, params object[] data) => LogError(new EventId(), exception, message, data);
        public void LogError(EventId eventId, string message, params object[] data) => LogError(eventId, null, message, data);
        public virtual void LogError(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Error, eventId, exception, message, data);
        }

        public Task LogErrorAsync(string message, params object[] data) => LogErrorAsync(new EventId(), null, message, data);
        public Task LogErrorAsync(Exception exception, string message, params object[] data) => LogErrorAsync(new EventId(), exception, message, data);
        public Task LogErrorAsync(EventId eventId, string message, params object[] data) => LogErrorAsync(eventId, null, message, data);
        public virtual async Task LogErrorAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Error, eventId, exception, message, data).ConfigureAwait(false);
        }

        public void LogCritical(string message, params object[] data) => LogCritical(new EventId(), null, message, data);
        public void LogCritical(Exception exception, string message, params object[] data) => LogCritical(new EventId(), exception, message, data);
        public void LogCritical(EventId eventId, string message, params object[] data) => LogCritical(eventId, null, message, data);
        public virtual void LogCritical(EventId eventId, Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Critical, eventId, exception, message, data);
        }

        public Task LogCriticalAsync(string message, params object[] data) => LogCriticalAsync(new EventId(), null, message, data);
        public Task LogCriticalAsync(Exception exception, string message, params object[] data) => LogCriticalAsync(new EventId(), exception, message, data);
        public Task LogCriticalAsync(EventId eventId, string message, params object[] data) => LogCriticalAsync(eventId, null, message, data);
        public virtual async Task LogCriticalAsync(EventId eventId, Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Critical, eventId, exception, message, data).ConfigureAwait(false);
        }
    }
}
