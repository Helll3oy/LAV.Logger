using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace LAV.Logger.Transports
{
    public abstract class TransportChunkedBase : TransportBase, ITransportChunk
    {
        private const int MAX_CHUNK_SIZE = 1412;
        private readonly int _chunkSize;

        public int ChunkSize => _chunkSize;

        protected TransportChunkedBase(ITransportOptions options, int chunkSize = MAX_CHUNK_SIZE) : base(options)
        {
            _chunkSize = chunkSize;
        }

        public virtual void SendChunked(ReadOnlySpan<byte> data)
        {
            var messageId = Guid.NewGuid().ToByteArray();
            var chunkCount = (int)Math.Ceiling(data.Length / (double)_chunkSize);
            var chunks = new List<byte[]>(chunkCount);

            for (int i = 0; i < chunkCount; i++)
            {
                var offset = i * _chunkSize;
                var length = Math.Min(_chunkSize, data.Length - offset);
                //var chunkData = new byte[length];
                //Array.Copy(data, offset, chunkData, 0, length);

                var chunk = CreateChunk(messageId, i, chunkCount, data.Slice(offset, length).ToArray());
                chunks.Add(chunk);
            }

            foreach (var chunk in chunks)
            {
                Send(chunk);
            }
        }

        public virtual async Task SendChunkedAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
        {
            var messageId = Guid.NewGuid().ToByteArray();
            var chunkCount = (int)Math.Ceiling(data.Length / (double)_chunkSize);
            var chunks = new List<ReadOnlyMemory<byte>>(chunkCount);

            // Prepare chunks
            for (int i = 0; i < chunkCount; i++)
            {
                ct.ThrowIfCancellationRequested();

                var offset = i * _chunkSize;
                var length = Math.Min(_chunkSize, data.Length - offset);
                
                //var chunkData = new byte[length];
                //Array.Copy(data, offset, chunkData, 0, length);

                var chunk = CreateChunk(messageId, i, chunkCount, data.Slice(offset, length).ToArray());
                chunks.Add(chunk);
            }

            // Send chunks in parallel
            var tasks = chunks.Select(chunk => SendAsync(chunk, ct)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static byte[] CreateChunk(byte[] messageId, int index, int totalChunks, byte[] data)
        {
            var chunk = new byte[12 + data.Length];
            Buffer.BlockCopy(new byte[] { 0x1e, 0x0f }, 0, chunk, 0, 2);
            Buffer.BlockCopy(messageId, 0, chunk, 2, 8);
            chunk[10] = (byte)index;
            chunk[11] = (byte)totalChunks;
            Buffer.BlockCopy(data, 0, chunk, 12, data.Length);
            return chunk;
        }
    }
}
