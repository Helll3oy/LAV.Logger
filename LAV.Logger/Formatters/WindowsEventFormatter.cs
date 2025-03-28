using System;
using System.Drawing;
using System.Text;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace LAV.Logger.Formatters
{
    internal sealed class WindowsEventFormatter : ILogFormatter
    {
        private const int BUFFER_SIZE = 1420;

#if !NET452
        private readonly MessagePackSerializerOptions _options;
#else
        private readonly IFormatterResolver _resolver;
#endif

#if !NET452
        public WindowsEventFormatter(MessagePackSerializerOptions options)
        {
            _options = options;
        }
#else
        public WindowsEventFormatter(MessagePack.IFormatterResolver resolver)
        {
            _resolver = resolver;
        }
#endif

        public string Format(LavLogEntry entry)
        {
            throw new NotSupportedException();       
        }

        public byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = BUFFER_SIZE)
        {
#if !NET452
            return MessagePackSerializer.Serialize(entry, _options);
#else
            return MessagePackSerializer.Serialize(entry, _resolver);
#endif
        }
    }
}
