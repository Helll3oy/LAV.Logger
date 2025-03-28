using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace LAV.Logger.Transports
{
    public abstract class TransportBase : IDisposable, ITransport
    {
        protected readonly ITransportOptions _options;
        protected bool disposedValue;

        public virtual ITransportOptions Options => _options;

        private TransportBase() { }
        protected TransportBase(ITransportOptions options)
        {
            _options = options;
        }

        public abstract void Send(byte[] data);
        public abstract void Send(ReadOnlySpan<byte> data);

        public abstract Task SendAsync(byte[] data, CancellationToken ct);
        public abstract Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct);

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            disposedValue = true;
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
