#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
#endif
namespace LAV.Logger
{
    public class LogFormatOptions
    {
        public bool EnableCompression { get; set; } = false;
        //CEF
        public string Vendor { get; set; }
        public string Product { get; set; }
        public string Version { get; set; } //Gelf

        //Gelf
        public string Source { get; set; }
        public string Host { get; set; }
    }
}
