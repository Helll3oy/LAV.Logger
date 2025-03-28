using System.IO;
using System.Text;

using LAV.Logger.Formatters;

#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
using System.Text.Json;
#endif

namespace LAV.Logger.Formatters
{
    // JSON Formatter
    public sealed class JsonFormatter : ILogFormatter
    {
        private const int BUFFER_SIZE = 1420;

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = BUFFER_SIZE)
        {
            return Encoding.UTF8.GetBytes(Format(entry));
        }

#if !NET452
        public string Format(LavLogEntry entry)
        {
            var options = new JsonWriterOptions { Indented = true };

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            var bufferWriter = new ArrayBufferWriter<byte>(initialCapacity: 256);
#else
            using var bufferWriter = new MemoryStream();
#endif
            using var writer = new Utf8JsonWriter(bufferWriter, options);

            writer.WriteStartObject();
            writer.WriteString("timestamp", entry.Timestamp);
            writer.WriteString("level", entry.Level.ToString());
            writer.WriteNumber("eventId", entry.EventId.Id);
            writer.WriteString("eventName", entry.EventId.Name);
            writer.WriteString("category", entry.Category);

            writer.WriteStartArray("scopes");
            foreach (var scope in entry.Scopes) writer.WriteStringValue(scope);
            writer.WriteEndArray();

            writer.WriteString("message", entry.Message);
            if (entry.Exception != null)
                writer.WriteString("exception", entry.Exception.ToString());

            writer.WriteStartObject("properties");
            foreach (var prop in entry.State)
                writer.WriteString(prop.Key, prop.Value?.ToString());
            writer.WriteEndObject();

            writer.WriteEndObject();
            writer.Flush();

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
#else
            bufferWriter.Position = 0;
            using var reader = new StreamReader(bufferWriter);
            return reader.ReadToEnd();
#endif
        }

#else// NETSTANDARD2_0 || NET462_OR_GREATER
        public string Format(LavLogEntry entry)
        {
            using var bufferWriter = new MemoryStream();
            using var writer = new Utf8JsonWriter(bufferWriter, true);

            writer.WriteStartObject();
            writer.WriteString("timestamp", entry.Timestamp.ToString("o"));
            writer.WriteString("level", entry.Level.ToString());
            writer.WriteNumber("eventId", entry.EventId.Id);
            writer.WriteString("eventName", entry.EventId.Name);
            writer.WriteString("category", entry.Category);
            writer.WriteStartArray("scopes");
            foreach (var scope in entry.Scopes) writer.WriteStringValue(scope);
            writer.WriteEndArray();
            writer.WriteString("message", entry.Message);
            if (entry.Exception != null)
                writer.WriteString("exception", entry.Exception.ToString());
            writer.WriteStartObject("properties");
            foreach (var prop in entry.State)
                writer.WriteString(prop.Key, prop.Value?.ToString());
            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.Flush();

            bufferWriter.Position = 0;
            using var reader = new StreamReader(bufferWriter);
            return reader.ReadToEnd();
        }
#endif
    }
}
