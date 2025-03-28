using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
//using LAV.Logger.Formatters;

#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#endif

namespace LAV.Logger.Transports
{
    public sealed class GelfTransport : IDisposable, ITransport
    {
        private readonly UdpClient _udpClient;
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _tcpStream;
        private readonly GelfFormatterOptions _options;
        private bool disposedValue;

        public GelfTransport(GelfFormatterOptions options)
        {
            _options = options;

            switch (options.Transport)
            {
                case TransportProtocol.UDP:
                    _udpClient = new UdpClient(options.ServerHost, options.ServerPort);
                    break;
                case TransportProtocol.TCP:
                    _tcpClient = new TcpClient(options.ServerHost, options.ServerPort);
                    _tcpStream = _tcpClient.GetStream();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.Transport));
            }
        }

        public void Send(byte[] data)
        {
            switch (_options.Transport)
            {
                case TransportProtocol.UDP:
                    _udpClient.Send(data, data.Length);
                    break;
                case TransportProtocol.TCP:
                    var framedData = new byte[data.Length + 1];
                    Buffer.BlockCopy(data, 0, framedData, 0, data.Length);
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
                    framedData[^1] = 0; // Null byte terminator
#else
                    framedData[framedData.Length - 1] = 0; // Null byte terminator
#endif
                    _tcpStream.Write(framedData, 0, framedData.Length);
                    break;
            }
        }

        public async Task SendAsync(byte[] data, CancellationToken ct)
        {
            switch (_options.Transport)
            {
                case TransportProtocol.UDP:
                    //await _udpClient.SendAsync(data, data.Length, ct);
                    await _udpClient.SendAsync(data, data.Length);
                    break;
                case TransportProtocol.TCP:
                    var framedData = new byte[data.Length + 1];
                    Buffer.BlockCopy(data, 0, framedData, 0, data.Length);
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
                    framedData[^1] = 0; // Null byte terminator
#else
                    framedData[framedData.Length - 1] = 0; // Null byte terminator
#endif
                    await _tcpStream.WriteAsync(framedData, 0, framedData.Length, ct);
                    break;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposedValue) return;

            disposedValue = true;

            if (disposing)
            {
#if NET452
                _udpClient?.Close();
                _tcpStream?.Close();
                _tcpClient?.Close();
#else
                _udpClient?.Dispose();
                _tcpStream?.Dispose();
                _tcpClient?.Dispose();
#endif
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~GelfTransport()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
