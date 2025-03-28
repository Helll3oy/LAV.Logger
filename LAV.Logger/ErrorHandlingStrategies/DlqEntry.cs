using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace LAV.Logger.ErrorHandlingStrategies
{
    public record DlqEntry(
        Guid Id,
        LavLogEntry LogEntry,
        Exception Error,
        DateTime CreatedAt);
}