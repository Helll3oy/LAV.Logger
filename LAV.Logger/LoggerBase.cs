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

        private volatile bool _useAnsiColors = true;
        private volatile LogLevel _level;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly object _sync = new object();

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
                    Warn(null, $"Уровень логирования изменен на {_level}.");
                }
                finally
                {
                    _lockSlim.ExitWriteLock();
                }
            }
        }

        public virtual bool UseAnsiColors
        {
            get
            {
                return _useAnsiColors;
            }
            set
            {
                lock (_sync)
                {
                    _useAnsiColors = value;
                    Warn(null, $"Режим Ansi для консоли {(_useAnsiColors ? "включен" : "выключен")}.");
                }
            }
        }

        protected LoggerBase() { }

        protected LoggerBase(IJsonSerializer jsonSerializer)
            : this(jsonSerializer, LogLevel.Trace)
        {
        }

        protected LoggerBase(IJsonSerializer jsonSerializer, LogLevel level)
        {
            UseAnsiColors = true;
            Level = level;

            _jsonSerializer = jsonSerializer;

#if WINDOWS
            if (_useAnsiColors) AnsiCodeEnabler.Enable();
#endif
        }

        public virtual IDisposable StartScope<TState>(TState state) where TState : class => null;

        public virtual void Stop() { }

        protected abstract void LogInternal(LogLevel level, Exception exception, string message, params object[] data);

        public void Log(LogLevel level, string message, params object[] data) => Log(level, null, message, data);
        public void Log(LogLevel level, Exception exception, string message, params object[] data)
        {
            if (_level > level) return;

            LogInternal(level, exception, message, data);
        }

        protected abstract Task LogInternalAsync(LogLevel level, Exception exception, string message, params object[] data);
        public Task LogAsync(LogLevel level, string message, params object[] data) => LogAsync(level, null, message, data);
        public async Task LogAsync(LogLevel level, Exception exception, string message, params object[] data)
        {
            if (_level > level) return;

            await _semaphore.WaitAsync();
            try
            {
                await LogInternalAsync(level, exception, message, data);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Trace(string message, params object[] data) => Trace(null, message, data);
        public virtual void Trace(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Trace, exception, message, data);
        }

        public Task TraceAsync(string message, params object[] data) => TraceAsync(null, message, data);
        public virtual async Task TraceAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Trace, exception, message, data);
        }

        public void Debug(string message, params object[] data) => Debug(null, message, data);
        public virtual void Debug(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Debug, exception, message, data);
        }

        public Task DebugAsync(string message, params object[] data) => DebugAsync(null, message, data);
        public virtual async Task DebugAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Debug, exception, message, data);
        }

        public void Info(string message, params object[] data) => Info(null, message, data);
        public virtual void Info(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Info, exception, message, data);
        }

        public Task InfoAsync(string message, params object[] data) => InfoAsync(null, message, data);
        public virtual async Task InfoAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Info, exception, message, data);
        }

        public void Warn(string message, params object[] data) => Warn(null, message, data);
        public virtual void Warn(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Warn, exception, message, data);
        }

        public Task WarnAsync(string message, params object[] data) => WarnAsync(null, message, data);
        public virtual async Task WarnAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Warn, exception, message, data);
        }

        public void Error(string message, params object[] data) => Error(null, message, data);
        public virtual void Error(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Error, exception, message, data);
        }

        public Task ErrorAsync(string message, params object[] data) => ErrorAsync(null, message, data);
        public virtual async Task ErrorAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Error, exception, message, data);
        }

        public void Fatal(string message, params object[] data) => Fatal(null, message, data);
        public virtual void Fatal(Exception exception, string message, params object[] data)
        {
            Log(LogLevel.Fatal, exception, message, data);
        }

        public Task FatalAsync(string message, params object[] data) => FatalAsync(null, message, data);
        public virtual async Task FatalAsync(Exception exception, string message, params object[] data)
        {
            await LogAsync(LogLevel.Fatal, exception, message, data);
        }
    }
}
