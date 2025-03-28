#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
#endif
namespace LAV.Logger
{
    public enum LogFormat
    {
        Ansi,
        Json,
        Logfmt,
        Cef,
        Gelf
    }
}
