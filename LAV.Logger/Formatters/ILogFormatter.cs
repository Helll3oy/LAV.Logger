namespace LAV.Logger.Formatters
{
    public interface ILogFormatter
    {
        string Format(LavLogEntry entry);

        byte[] CreateMessage(LavLogEntry entry, bool? indented = true, int? bufferSize = 1420);
    }
}
