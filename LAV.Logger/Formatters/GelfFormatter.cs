using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;

using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;

using LAV.Logger.Transports;

#if NET452

#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Buffers;
#else
using System.Text.Json;
using System.Buffers;
#endif

namespace LAV.Logger.Formatters
{
    public sealed class GelfFormatterOptions
    {
        public static GelfFormatterOptions Default => new GelfFormatterOptions();

        public string Version { get; set; } = "1.1";
        public Dictionary<string, string> FieldMappings { get; set; } = new();
    }

    public sealed class GelfFormatter : ILogFormatter, IDisposable
    {
        private readonly GelfFormatterOptions _options;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Microsoft.Extensions.Logging.ILogger _errorLogger;
        private bool disposedValue;

        private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        public GelfFormatter() : this(GelfFormatterOptions.Default, null, null) { }

        public GelfFormatter(GelfFormatterOptions options, IJsonSerializer jsonSerializer = null, 
            Microsoft.Extensions.Logging.ILogger errorLogger = null)
        {
            _options = options;
            _jsonSerializer = jsonSerializer;

            _errorLogger = errorLogger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public string Format(LavLogEntry entry)
        {
            var message = CreateMessage(entry, true, null);
            return Encoding.UTF8.GetString(message);
        }

        public ReadOnlyMemory<char> FormatToMemory(LavLogEntry entry)
        {
            var message = CreateMessage(entry, true, null);
            return Encoding.UTF8.GetChars(message).AsMemory();
        }

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = 1420)
        {
            return CreateMessage(entry, indented, bufferSize, false);
        }

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = 1420, bool enableCompression = false)
        {
            var buffer = _bufferPool.Rent(bufferSize.Value * 4);
            try
            {
                using var output = new MemoryStream(buffer);
                using var gzip = enableCompression ? new GZipStream(output, CompressionLevel.Optimal) : null;

#if !NET452
                var options = new JsonWriterOptions { Indented = indented.Value };
                using var writer = gzip != null ? new Utf8JsonWriter(gzip) : new Utf8JsonWriter(output, options);
#else
                using var writer = gzip != null ? new Utf8JsonWriter(gzip) : new Utf8JsonWriter(output, indented.Value);
#endif
                writer.WriteStartObject();
                WriteBaseFields(writer, entry);
                WriteAdditionalFields(writer, entry);
                writer.WriteEndObject();
                writer.Flush();

                gzip?.Flush();

                var outMsg = new byte[output.Position];
                Buffer.BlockCopy(buffer, 0, outMsg, 0, (int)output.Position);

                return outMsg;
            }
            finally
            {
                _bufferPool.Return(buffer);
            }
        }

        private void WriteBaseFields(Utf8JsonWriter writer, LavLogEntry entry)
        {
            writer.WriteString("version", _options.Version);
            writer.WriteString("host", Environment.MachineName);
            writer.WriteString("short_message", entry.Message);
            writer.WriteNumber("timestamp", entry.Timestamp.ToUnixTimestamp());
            writer.WriteNumber("level", entry.Level.ToSyslogLevel());
        }

        private void WriteAdditionalFields(Utf8JsonWriter writer, LavLogEntry entry)
        {
            // Additional fields
            writer.WriteString("_category", entry.Category);

            if (!string.IsNullOrEmpty(entry.EventId.Name))
                writer.WriteString("_event", entry.EventId.Name);

            if(entry.EventId.Id != 0)
                writer.WriteNumber("_eventId", entry.EventId.Id);

            writer.WriteString("_source", entry.Category);
            writer.WriteString("_app_name", Assembly.GetEntryAssembly().GetName().Name);

            if (!string.IsNullOrWhiteSpace(Environment.UserDomainName))
                writer.WriteString("_user_id", $"{Environment.UserDomainName}/{Environment.UserName}");
            else
                writer.WriteString("_user_id", Environment.UserName);

            // Field mapping and custom fields
            foreach (var prop in entry.State)
            {
                var fieldName = _options.FieldMappings.TryGetValue(prop.Key, out var mapping)
                    ? mapping
                    : $"_{prop.Key}";

                writer.WritePropertyName(fieldName);
#if !NET452
                JsonSerializer.Serialize(writer, prop.Value);
#else
                writer.WriteStringValue(prop.Value.ToString());
#endif
            }

            // Add scopes as array
            if (entry.Scopes.Count > 0)
            {
                writer.WriteStartArray("_scopes");
                foreach (var scope in entry.Scopes)
                    writer.WriteStringValue(scope);
                writer.WriteEndArray();
            }

            // Exception details
            if (entry.Exception != null)
            {
                writer.WriteString("_exception", entry.Exception.Message);
                writer.WriteString("_exception_type", entry.Exception.GetType().Name);
                writer.WriteString("_exception_stack", entry.Exception.StackTrace);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposedValue) return;
            disposedValue = true;

            if (!disposing) return;
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~GelfFormatter()
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

        //        private static string Compress(string input)
        //        {
        //            using var output = new MemoryStream();
        //            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        //            {
        //                using var writer = new StreamWriter(gzip);
        //                writer.Write(input);
        //            }
        //            return Convert.ToBase64String(output.ToArray());
        //        }

        //        public string FormatToJson(LavLogEntry entry)
        //        {
        //#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        //            var bufferWriter = new ArrayBufferWriter<byte>();
        //            var options = new JsonWriterOptions { Indented = true };
        //            using var writer = new Utf8JsonWriter(bufferWriter, options);

        //            writer.WriteStartObject();

        //            // Required GELF fields
        //            writer.WriteString("version", _version);
        //            writer.WriteString("host", _host);
        //            writer.WriteString("short_message", entry.Message);
        //            writer.WriteNumber("timestamp", entry.Timestamp.ToUnixTimestamp());
        //            writer.WriteNumber("level", entry.Level.ToSyslogLevel());

        //            // Additional fields
        //            writer.WriteString("_category", entry.Category);
        //            writer.WriteString("_eventId", entry.EventId.Id.ToString());
        //            writer.WriteString("_eventName", entry.EventId.Name);
        //            writer.WriteString("_source", _source);

        //            // Scopes
        //            if (entry.Scopes.Count > 0)
        //            {
        //                writer.WriteStartArray("_scopes");
        //                foreach (var scope in entry.Scopes)
        //                    writer.WriteStringValue(scope);
        //                writer.WriteEndArray();
        //            }

        //            // Exception
        //            if (entry.Exception != null)
        //            {
        //                writer.WriteString("_exception", entry.Exception.ToString());
        //                writer.WriteString("_exceptionType", entry.Exception.GetType().Name);
        //            }

        //            // State properties
        //            foreach (var prop in entry.State)
        //            {
        //                writer.WritePropertyName($"_{prop.Key}");
        //                JsonSerializer.Serialize(writer, prop.Value);
        //            }

        //            writer.WriteEndObject();
        //            writer.Flush();

        //            return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);

        //#elif NETSTANDARD2_0 || NET462_OR_GREATER
        //            using var bufferWriter = new MemoryStream();
        //            var options = new JsonWriterOptions { Indented = true };
        //            using var writer = new Utf8JsonWriter(bufferWriter, options);

        //            writer.WriteStartObject();

        //            // Required GELF fields
        //            writer.WriteString("version", _version);
        //            writer.WriteString("host", _host);
        //            writer.WriteString("short_message", entry.Message);
        //            writer.WriteNumber("timestamp", entry.Timestamp.ToUnixTimestamp());
        //            writer.WriteNumber("level", entry.Level.ToSyslogLevel());

        //            // Additional fields
        //            writer.WriteString("_category", entry.Category);
        //            writer.WriteString("_eventId", entry.EventId.Id.ToString());
        //            writer.WriteString("_eventName", entry.EventId.Name);
        //            writer.WriteString("_source", _source);

        //            // Scopes
        //            if (entry.Scopes.Count > 0)
        //            {
        //                writer.WriteStartArray("_scopes");
        //                foreach (var scope in entry.Scopes)
        //                    writer.WriteStringValue(scope);
        //                writer.WriteEndArray();
        //            }

        //            // Exception
        //            if (entry.Exception != null)
        //            {
        //                writer.WriteString("_exception", entry.Exception.ToString());
        //                writer.WriteString("_exceptionType", entry.Exception.GetType().Name);
        //            }

        //            // State properties
        //            //foreach (var prop in entry.State)
        //            //{
        //            //    writer.WritePropertyName($"_{prop.Key}");
        //            //    JsonSerializer.Serialize(writer, prop.Value);
        //            //}
        //            foreach (var prop in entry.State)
        //                writer.WriteString($"_{prop.Key}", prop.Value?.ToString());

        //            writer.WriteEndObject();
        //            writer.Flush();

        //            bufferWriter.Position = 0;
        //            using var reader = new StreamReader(bufferWriter);
        //            return reader.ReadToEnd();

        //#else
        //            return string.Empty;
        //#endif
        //        }
    }
}
