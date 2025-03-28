using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;
using LAV.Logger.Formatters;

namespace LAV.Logger.Transports
{
    public sealed class StreamTransportOptions : ITransportOptions
    {
        public ILogFormatter[] Formatters { get; set; }
        public bool EnableCompression { get; set; }
        public TimeSpan Timeout { get; set; }
    }

    public sealed class StreamTransport : TransportBase
    {

        public StreamTransport(StreamTransportOptions options) : base(options)
        {
        }

        public override void Send(byte[] data)
        {
            //if (logLevel is Microsoft.Extensions.Logging.LogLevel.Error or Microsoft.Extensions.Logging.LogLevel.Critical)
            //    Console.Error.WriteLine(data);
            //else
                //Console.WriteLine(data);

            Console.WriteLine(Encoding.UTF8.GetString(data));
        }

        public override void Send(ReadOnlySpan<byte> data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data.ToArray()));
            //Console.WriteLine(data.ToArray());
        }

        public override Task SendAsync(byte[] data, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
#if !NET452
                return Task.FromCanceled(ct);
#else
                return TaskHelpers.CreateCanceledTask();
#endif
            }

            Console.WriteLine(Encoding.UTF8.GetString(data));

#if !NET452
            return Task.CompletedTask;
#else
            return TaskHelpers.CompletedTask;
#endif
        }

        public override Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
#if !NET452
                return Task.FromCanceled(ct);
#else
                return TaskHelpers.CreateCanceledTask();
#endif
            }

            Console.WriteLine(Encoding.UTF8.GetString(data.ToArray()));

#if !NET452
            return Task.CompletedTask;
#else
            return TaskHelpers.CompletedTask;
#endif
        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);

        //    if (!disposing) return;
        //}
    }
}
