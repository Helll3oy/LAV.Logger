using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using LAV.Logger.Formatters;

namespace LAV.Logger.Transports
{
    public sealed class UdpTransportOptions : IRemoteTransportOptions
    {
        public ILogFormatter[] Formatters { get; set; }

        public string ServerHost { get; set; }

        public int ServerPort { get; set; }

        public bool EnableCompression { get; set; }

        public TimeSpan Timeout { get; set; }

        public int ChunkSize { get; } = 1412;
    }

    public sealed class UdpTransport : TransportChunkedBase
    {
        private readonly UdpClient _udpClient;
        private readonly IRemoteTransportOptions _remoteOptions;

        public UdpTransport(UdpTransportOptions options) : base(options, options.ChunkSize)
        {
            _remoteOptions = (IRemoteTransportOptions)options;
            _udpClient = new UdpClient(_remoteOptions.ServerHost, _remoteOptions.ServerPort);
        }

        public override void Send(byte[] data)
        {
            _udpClient.Send(data, data.Length);
        }

        public override void Send(ReadOnlySpan<byte> data)
        {
            _udpClient.Send(data.ToArray(), data.Length);
        }

        public override async Task SendAsync(byte[] data, CancellationToken ct)
        {
            await _udpClient.SendAsync(data, data.Length);
        }

        public override async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
        {
            await _udpClient.SendAsync(data.ToArray(), data.Length);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

#if NET452
            _udpClient?.Close();
#else
            _udpClient?.Dispose();
#endif
        }
    }
}
