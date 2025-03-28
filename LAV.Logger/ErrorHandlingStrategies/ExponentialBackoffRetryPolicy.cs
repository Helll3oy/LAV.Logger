using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LAV.Logger.ErrorHandlingStrategies
{
    /// <summary>
    /// Exponential Backoff Retry
    /// </summary>
    public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly TimeSpan _baseDelay;

        public ExponentialBackoffRetryPolicy(int maxAttempts, TimeSpan baseDelay)
        {
            _maxAttempts = maxAttempts;
            _baseDelay = baseDelay;
        }

        public async Task ExecuteAsync(
            Func<int, Task> action,
            CancellationToken ct)
        {
            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    await action(attempt);
                    return;
                }
                catch when (attempt == _maxAttempts)
                {
                    throw;
                }
                catch (Exception) when (!ct.IsCancellationRequested)
                {
                    var delay = CalculateDelay(attempt);
                    await Task.Delay(delay, ct);
                }
            }
        }

        private TimeSpan CalculateDelay(int attempt)
        {
            var jitter = new Random().NextDouble() * 0.2 + 0.9;
            return TimeSpan.FromTicks((long)(
                _baseDelay.Ticks * Math.Pow(2, attempt - 1) * jitter));
        }
    }
}
