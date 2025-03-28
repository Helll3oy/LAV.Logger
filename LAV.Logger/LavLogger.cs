using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
using System.Text.Json;
#endif
using System.Threading;
using LAV.Logger.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using LAV.Logger.Transports;
using System.Threading.Tasks;

namespace LAV.Logger
{
    public sealed class LavLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly string _categoryName;
        private readonly Dictionary<ILogFormatter, HashSet<ITransport>> _senders = [];

#if NET452
        private readonly CustomScopeProvider _scopeProvider;

        public LavLogger(string categoryName, CustomScopeProvider scopeProvider, params ITransport[] transports)
        {
            _categoryName = categoryName;
            _scopeProvider = scopeProvider;

            if(transports.Length > 0)
            {
                foreach(var transport in transports)
                {
                    foreach(var formatter in  transport.Options.Formatters)
                    {
                        if(!_senders.TryGetValue(formatter, out var senders))
                        {
                            _senders.Add(formatter, new HashSet<ITransport>());
                        }

                        senders.Add(transport);
                    }
                }
            }
            else
            {
                _senders.Add(new AnsiFormatter(), [new StreamTransport(new StreamTransportOptions
                {
                    EnableCompression = false,
                    Timeout = Timeout.InfiniteTimeSpan
                })]);
            }
        }
#else
        private readonly Microsoft.Extensions.Logging.IExternalScopeProvider _scopeProvider;

        public LavLogger(string categoryName, IExternalScopeProvider scopeProvider)
            : this(categoryName, scopeProvider, null) { }
        public LavLogger(string categoryName, IExternalScopeProvider scopeProvider, 
            params ITransport[] transports)
        {
            _categoryName = categoryName;
            _scopeProvider = scopeProvider;

            if(transports.Length > 0)
            {
                foreach(var transport in transports)
                {
                    foreach(var formatter in  transport.Options.Formatters)
                    {
                        if(!_senders.TryGetValue(formatter, out var senders))
                        {
                            senders = new HashSet<ITransport>();
                            _senders.Add(formatter, senders);
                        }

                        senders.Add(transport);
                    }
                }
            }
            else
            {
                _senders.Add(new AnsiFormatter(), [new StreamTransport(new StreamTransportOptions
                {
                    EnableCompression = false,
                    Timeout = Timeout.InfiniteTimeSpan
                })]);
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => _scopeProvider.Push(state);

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            => logLevel != Microsoft.Extensions.Logging.LogLevel.None;

        public void Send(LavLogEntry entry)
        {
            foreach(var senders in _senders)
            {
                var message = senders.Key.CreateMessage(entry, false);

                Send(message, senders.Value);
            }
        }

        private static void Send(ReadOnlySpan<byte> data, HashSet<ITransport> transports)
        {
            foreach (var transport in transports)
            {
                if (transport is ITransportChunk chunkedTransport && data.Length > chunkedTransport.ChunkSize)
                {
                    chunkedTransport.SendChunked(data);
                }
                else
                {
                    transport.Send(data.ToArray());
                }
            }
        }

        public async Task SendAsync(LavLogEntry entry, CancellationToken ct = default)
        {
            List<Task> tasks = [];
            foreach (var senders in _senders)
            {
                var message = senders.Key.CreateMessage(entry, false);

                tasks.Add(SendAsync(message, senders.Value, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static async Task SendAsync(ReadOnlyMemory<byte> data, HashSet<ITransport> transports, CancellationToken ct = default)
        {
#if NET6_0_OR_GREATER
            await Parallel.ForEachAsync(transports, async (transport, _) => {
                if (transport is ITransportChunk chunkedTransport && data.Length > chunkedTransport.ChunkSize)
                {
                    await chunkedTransport.SendChunkedAsync(data, ct).ConfigureAwait(false);
                }
                else
                {
                    await transport.SendAsync(data, ct).ConfigureAwait(false);
                }
            });
#else
            await Task.WhenAll(transports.Select(transport =>
            {
                if (transport is ITransportChunk chunkedTransport && data.Length > chunkedTransport.ChunkSize)
                {
                    return chunkedTransport.SendChunkedAsync(data, ct);
                }
                else
                {
                    return transport.SendAsync(data, ct);
                }
            })).ConfigureAwait(false);
#endif
        }

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter) where TState : notnull
        {
            if (!IsEnabled(logLevel)) return;

            var entry = new LavLogEntry(
                DateTime.UtcNow,
                logLevel,
                eventId,
                _categoryName,
                GetScopes(state),
                formatter(state, exception),
                exception,
                GetStateProperties(state));

            Send(entry);
        }

        private static IReadOnlyList<KeyValuePair<string, object>> GetStateProperties<TState>(TState state) where TState : notnull
            => state as IReadOnlyList<KeyValuePair<string, object>>
                ?? Enumerable.Empty<KeyValuePair<string, object>>().ToArray();

        private List<string> GetScopes<TState>(TState state) where TState : notnull
        {
            var scopes = new List<string>();
            _scopeProvider.ForEachScope<TState>((scope, _) => scopes.Add(scope.ToString()), state);
            //scopes.Reverse();
            return scopes;
        }
    }
}
