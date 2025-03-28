using System;
using System.Threading.Tasks;
using System.Threading;

namespace LAV.Logger.ErrorHandlingStrategies
{
    public interface IRetryPolicy
    {
        Task ExecuteAsync(Func<int, Task> action, CancellationToken ct);
    }
}