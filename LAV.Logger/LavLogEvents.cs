namespace LAV.Logger
{
    public static class LavLogEvents
    {
        public static readonly Microsoft.Extensions.Logging.EventId Info = 
            new Microsoft.Extensions.Logging.EventId(1001, nameof(Info));
        public static readonly Microsoft.Extensions.Logging.EventId Debug = 
            new Microsoft.Extensions.Logging.EventId(1002, nameof(Debug));
        public static readonly Microsoft.Extensions.Logging.EventId Critical =
            new Microsoft.Extensions.Logging.EventId(1003, nameof(Critical));
    }
}
