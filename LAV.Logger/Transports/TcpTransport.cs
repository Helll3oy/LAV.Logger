using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using LAV.Logger.Formatters;

namespace LAV.Logger.Transports
{
    public sealed class TcpTransportOptions : IRemoteTransportOptions
    {
        public ILogFormatter[] Formatters { get; set; }

        public string ServerHost { get; set; }

        public int ServerPort { get; set; }

        public bool EnableCompression { get; set; } 

        public TimeSpan Timeout { get; set; }
    }

    public sealed class TcpTransport : TransportBase
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _tcpStream;

        private readonly IRemoteTransportOptions _remoteOptions;

        public TcpTransport(TcpTransportOptions options) : base(options)
        {
            _remoteOptions = (IRemoteTransportOptions)options;

            _tcpClient = new TcpClient(_remoteOptions.ServerHost, _remoteOptions.ServerPort);
            _tcpStream = _tcpClient.GetStream();
        }

        public override void Send(byte[] data)
        {
            var framedData = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, framedData, 0, data.Length);

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            framedData[^1] = 0; // Null byte terminator
            _tcpStream.Write(framedData.AsSpan());
#else
            framedData[framedData.Length - 1] = 0; // Null byte terminator
            _tcpStream.Write(framedData, 0, framedData.Length);
#endif
        }

        public override void Send(ReadOnlySpan<byte> data)
        {
            var framedData = new byte[data.Length + 1];
            data.CopyTo(framedData);

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            framedData[^1] = 0; // Null byte terminator
            //_tcpStream.Write(framedData.AsSpan());
#else
            framedData[framedData.Length - 1] = 0; // Null byte terminator
            //_tcpStream.Write(framedData, 0, framedData.Length);
#endif

            if (!_tcpClient.Connected)
                _tcpClient.Connect(_remoteOptions.ServerHost, _remoteOptions.ServerPort);

            _tcpStream = _tcpClient.GetStream();
            _tcpStream.Write(framedData, 0, framedData.Length);
        }

        public override async Task SendAsync(byte[] data, CancellationToken ct)
        {
            var framedData = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, framedData, 0, data.Length);

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            framedData[^1] = 0; // Null byte terminator
            await _tcpStream.WriteAsync(framedData.AsMemory(), ct);
#else
            framedData[framedData.Length - 1] = 0; // Null byte terminator
            await _tcpStream.WriteAsync(framedData, 0, framedData.Length, ct);
#endif
        }

        public override async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
        {
            var framedData = new byte[data.Length + 1];
            data.CopyTo(framedData);
            //Buffer.BlockCopy(data.ToArray(), 0, framedData, 0, data.Length);

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            framedData[^1] = 0; // Null byte terminator
            await _tcpStream.WriteAsync(framedData, ct);
#else
            framedData[framedData.Length - 1] = 0; // Null byte terminator
            await _tcpStream.WriteAsync(framedData, 0, framedData.Length, ct);
#endif
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

#if NET452
            _tcpStream?.Close();
            _tcpClient?.Close();
#else
            _tcpStream?.Dispose();
            _tcpClient?.Dispose();
#endif
        }
    }
}
