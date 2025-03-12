using System;
using System.Collections.Generic;
using System.Text;

namespace LAV.Logger
{
    public sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}
