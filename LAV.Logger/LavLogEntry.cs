#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MessagePack;
using MessagePack.Formatters;
using System.Text;
using System.Buffers;
using System.Linq;

namespace LAV.Logger
{
    [MessagePackObject]
    public sealed partial record LavLogEntry(
        [property: Key(0)] DateTime Timestamp,
        [property: Key(1)] Microsoft.Extensions.Logging.LogLevel Level,
        [property: Key(2)] Microsoft.Extensions.Logging.EventId EventId,
        [property: Key(3)] string Category,
        [property: Key(4)] List<string> Scopes,
        [property: Key(5)] string Message,
        [property: Key(6)] Exception Exception,
        [property: Key(7)] IReadOnlyList<KeyValuePair<string, object>> State);

    [MessagePackObject]
    public sealed class SerializableEventId
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public string? Name { get; set; }
    }

#pragma warning disable S2166
    [MessagePackObject]
    public sealed class SerializableException
    {
        [Key(0)]
        public string? Message { get; set; }

        [Key(1)]
        public string? StackTrace { get; set; }

        [Key(2)]
        public Dictionary<string, object>? Data { get; set; }
    }
#pragma warning restore S2166

#if !NET452
    internal sealed class EventIdFormatter : IMessagePackFormatter<Microsoft.Extensions.Logging.EventId>
    {
        public void Serialize(ref MessagePackWriter writer, Microsoft.Extensions.Logging.EventId value,
            MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.WriteInt32(value.Id);
            writer.WriteString(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(value.Name ?? string.Empty)));
        }

        public Microsoft.Extensions.Logging.EventId Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.ReadArrayHeader() != 2)
                throw new MessagePackSerializationException("Invalid EventId format");

            int id = reader.ReadInt32();
            string? name = reader.ReadString();
            return new Microsoft.Extensions.Logging.EventId(id, name);
        }
    }

    internal sealed class LogLevelFormatter : IMessagePackFormatter<Microsoft.Extensions.Logging.LogLevel>
    {
        public void Serialize(ref MessagePackWriter writer, Microsoft.Extensions.Logging.LogLevel value, MessagePackSerializerOptions options)
        {
            writer.WriteInt32((int)value);
        }

        public Microsoft.Extensions.Logging.LogLevel Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (Microsoft.Extensions.Logging.LogLevel)reader.ReadInt32();
        }
    }

    internal sealed class ExceptionFormatter : IMessagePackFormatter<Exception?>
    {
        public void Serialize(ref MessagePackWriter writer, Exception? value, MessagePackSerializerOptions options)
        {
            if (value == null)
            { 
                writer.WriteNil();
                return;
            }

            var serializable = new SerializableException
            {
                Message = value.Message,
                StackTrace = value.StackTrace ?? string.Empty,
                Data = value.Data.Keys.Cast<string>().ToDictionary(k => k, k => value.Data[k]!)
            };

            MessagePackSerializer.Serialize(ref writer, serializable, options);
        }

        public Exception? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return null;

            var serializable = MessagePackSerializer.Deserialize<SerializableException>(ref reader, options);
            var ex = new Exception(serializable.Message);

            if (serializable.Data != null)
            {
                foreach (var kv in serializable.Data)
                    ex.Data[kv.Key] = kv.Value;
            }

            return ex;
        }
    }
#else
    internal sealed class EventIdFormatter : IMessagePackFormatter<Microsoft.Extensions.Logging.EventId>
    {
        public int Serialize(ref byte[] bytes, int offset, Microsoft.Extensions.Logging.EventId value, IFormatterResolver formatterResolver)
    {
        var startOffset = offset;
        
        offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2); // [Id, Name]
        offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Id);
        offset += MessagePackBinary.WriteString(ref bytes, offset, value.Name ?? string.Empty);
        
        return offset - startOffset;
    }

    public Microsoft.Extensions.Logging.EventId Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
    {
        var startOffset = offset;
        
        var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
        offset += readSize;
        
        if (length != 2)
            //throw new MessagePackSerializationException("Invalid EventId format");
            throw new Exception("Invalid EventId format");
        
        var id = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        offset += readSize;
        
        var name = MessagePackBinary.ReadString(bytes, offset, out readSize);
        offset += readSize;
        
        readSize = offset - startOffset;
        return new Microsoft.Extensions.Logging.EventId(id, name);
    }
}
#endif


}
