using System.Text;
using LAV.Logger.Formatters;

#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
#endif

namespace LAV.Logger
{
    // logfmt Formatter
    public sealed class LogfmtFormatter : ILogFormatter
    {
        private const int BUFFER_SIZE = 1420;

        public string Format(LavLogEntry entry)
        {
            var builder = InnerCreateMessage(entry, BUFFER_SIZE * 4);

            return builder.ToString();
        }

        private static string Escape(string value) => value?.Replace("\"", "\\\"") ?? "";

        byte[] ILogFormatter.CreateMessage(LavLogEntry entry, bool? indented, int? bufferSize)
        {
            var builder = InnerCreateMessage(entry, bufferSize.Value * 4);

            char[] dest = new char[builder.Length];
            builder.CopyTo(0, dest, 0, builder.Length);

            return Encoding.UTF8.GetBytes(dest);
        }

        private static StringBuilder InnerCreateMessage(LavLogEntry entry, int bufferSize)
        {
            var builder = new StringBuilder(bufferSize)
                .Append($"ts={entry.Timestamp:o}")
                .Append($" level={entry.Level.ToString().ToLowerInvariant()}")
                .Append($" event_id={entry.EventId.Id}")
                .Append($" category={entry.Category}");

            if (entry.Scopes.Count > 0)
                builder.Append($" scopes=\"{Escape(string.Join(" => ", entry.Scopes))}\"");

            builder.Append($" msg=\"{Escape(entry.Message)}\"");

            if (entry.Exception != null)
                builder.Append($" exception=\"{Escape(entry.Exception.ToString())}\"");

            foreach (var prop in entry.State)
                builder.Append($" {prop.Key}=\"{Escape(prop.Value?.ToString())}\"");

            return builder;
        }
    }
}
