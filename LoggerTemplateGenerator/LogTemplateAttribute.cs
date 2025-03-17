namespace LAV.LoggerTemplateGenerator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class LogTemplateAttribute : Attribute
    {
        public string Message { get; }

        public LogTemplateAttribute(string message)
        {
            Message = message;
        }
    }
}
