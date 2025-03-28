using System;
using System.Threading;
using System.Threading.Tasks;
using LAV.Logger.Formatters;

namespace LAV.Logger.Transports
{
    public interface ITransportOptions
    {
        ILogFormatter[] Formatters { get; }
        bool EnableCompression { get; }
        TimeSpan Timeout { get; }
    }

    public interface IRemoteTransportOptions : ITransportOptions
    {
        string ServerHost { get; }
        int ServerPort { get; }
    }

    public interface ITransport
    {
        ITransportOptions Options { get; }
        void Send(ReadOnlySpan<byte> data);
        Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct);
    }

    public interface ITransportChunk : ITransport
    {
        int ChunkSize { get; }
        void SendChunked(ReadOnlySpan<byte> data);
        Task SendChunkedAsync(ReadOnlyMemory<byte> data, CancellationToken ct);
    }
}