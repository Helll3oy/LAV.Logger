namespace LAV.Logger
{
    internal static class LogEvents
    {
        public static readonly EventId UseAnsiColors = new EventId(1001, nameof(UseAnsiColors));
        public static readonly EventId LogLevelChanged = new EventId(1002, nameof(LogLevelChanged));
    }
}
