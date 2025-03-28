namespace LAV.Logger.ErrorHandlingStrategies
{
    public class DlqOptions
    {
        public int MaxInMemoryEntries { get; set; } = 1000;
        public IDlqPersistentStorage PersistentStorage { get; set; }
    }
}