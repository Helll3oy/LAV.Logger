#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LAV.Logger
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class EventLogSourceAttribute(string? logName = null) : Attribute
    {
        public string LogName { get; } = logName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EventCategoryAttribute(int id, string displayName) : Attribute
    {
        public int Id { get; } = id;
        public string DisplayName { get; } = displayName;
    }
}
