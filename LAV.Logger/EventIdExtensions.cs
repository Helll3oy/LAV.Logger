namespace LAV.Logger
{
    public static class EventIdExtensions
    {
        public static Microsoft.Extensions.Logging.EventId ToMicrosoftEventId(this EventId eventId)
        {
            return new Microsoft.Extensions.Logging.EventId(eventId.Id, eventId.Name);
        }
    }
}
