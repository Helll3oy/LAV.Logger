using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

#if !NET452
using System.Threading.Channels;
#endif

namespace LAV.Logger.ErrorHandlingStrategies
{
    /// <summary>
    /// Dead-Letter Queue (DLQ)
    /// </summary>
    public sealed class DeadLetterQueue
    {
        private readonly DlqOptions _options;
#if !NET452
        private readonly Channel<DlqEntry> _channel;
#else
        private readonly CustomChannel<DlqEntry> _channel;
#endif

        public DeadLetterQueue(DlqOptions options)
        {
            _options = options;

#if !NET452
            _channel = Channel.CreateBounded<DlqEntry>(
                new BoundedChannelOptions(options.MaxInMemoryEntries)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleWriter = false,
                    SingleReader = false
                });
#else
            _channel = new CustomChannel<DlqEntry>();
#endif
        }

        public async ValueTask StoreAsync(LavLogEntry entry, Exception error)
        {
            var dlqEntry = new DlqEntry(
                Guid.NewGuid(),
                entry,
                error,
                DateTime.UtcNow);

            if (_options.PersistentStorage != null)
            {
                await _options.PersistentStorage.StoreAsync(dlqEntry);
            }

            await _channel.Writer.WriteAsync(dlqEntry);
        }

#if !NET452
        public async IAsyncEnumerable<DlqEntry> GetEntriesAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            if (_options.PersistentStorage != null)
            {
                await foreach (var entry in _options.PersistentStorage.RetrieveAsync(ct))
                {
                    yield return entry;
                }
            }

            await foreach (var entry in _channel.Reader.ReadAllAsync(ct))
            {
                yield return entry;
            }
        }
#endif

        public async Task AcknowledgeAsync(Guid entryId)
        {
            if (_options.PersistentStorage != null)
            {
                await _options.PersistentStorage.RemoveAsync(entryId);
            }
        }
    }
}
