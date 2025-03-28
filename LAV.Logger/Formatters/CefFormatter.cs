using System.Collections.Generic;

using LAV;
using LAV.Logger;
using LAV.Logger.Formatters;
using System.Text;


#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
#endif

namespace LAV.Logger.Formatters
{
    public sealed class CefFormatterOptions
    {
        public static CefFormatterOptions Default => new CefFormatterOptions();

        public string Version { get; set; }
        public string Vendor { get; set; }
        public string Product { get; set; }
    }

    // CEF Formatter
    public sealed class CefFormatter : ILogFormatter
    {
        private const int BUFFER_SIZE = 1420;
        private readonly CefFormatterOptions _options;

        public CefFormatter(CefFormatterOptions options)
        {
            _options = options;
        }

        public string Format(LavLogEntry entry)
        {
            var builder = InnerCreateMessage(entry, BUFFER_SIZE * 4);

            return builder.ToString();
        }

        private int GetCefSeverity(Microsoft.Extensions.Logging.LogLevel level) => level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => 1,
            Microsoft.Extensions.Logging.LogLevel.Debug => 3,
            Microsoft.Extensions.Logging.LogLevel.Information => 5,
            Microsoft.Extensions.Logging.LogLevel.Warning => 7,
            Microsoft.Extensions.Logging.LogLevel.Error => 9,
            Microsoft.Extensions.Logging.LogLevel.Critical => 10,
            _ => 0
        };

        private static string Escape(string value) => value?.Replace("|", "\\|") ?? "";

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = 1420)
        {
            var builder = InnerCreateMessage(entry, bufferSize.Value * 4);

            char[] dest = new char[builder.Length];
            builder.CopyTo(0, dest, 0, builder.Length);

            return Encoding.UTF8.GetBytes(dest);
        }

        private StringBuilder InnerCreateMessage(LavLogEntry entry, int bufferSize)
        {
            var extensions = new List<string>
            {
                $"cat={entry.Category}",
                $"scopes={string.Join("|", entry.Scopes)}",
                $"msg={Escape(entry.Message)}"
            };

            foreach (var prop in entry.State)
                extensions.Add($"{prop.Key}={Escape(prop.Value?.ToString())}");

            if (entry.Exception != null)
                extensions.Add($"exception={Escape(entry.Exception.ToString())}");

            return new StringBuilder(string.Format(
                "CEF:0|{0}|{1}|{2}|{3}|{4}|{5}| {6}",
                _options.Vendor,
                _options.Product,
                _options.Version,
                entry.EventId.Id,
                entry.EventId.Name,
                GetCefSeverity(entry.Level),
                string.Join(" ", extensions)));
        }
    }
}
