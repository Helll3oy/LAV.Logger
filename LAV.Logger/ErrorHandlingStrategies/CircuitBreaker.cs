using System;
using System.Collections.Generic;
using System.Text;

namespace LAV.Logger.ErrorHandlingStrategies
{
    /// <summary>
    /// Circuit Breaker
    /// </summary>
    public sealed class CircuitBreaker
    {
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private int _failureCount;
        private DateTime _lastFailureTime;
        private bool _isTripped;

        public CircuitBreaker(int failureThreshold, TimeSpan resetTimeout)
        {
            _failureThreshold = failureThreshold;
            _resetTimeout = resetTimeout;
        }

        public bool IsTripped
        {
            get
            {
                if (_isTripped &&
                   (DateTime.UtcNow - _lastFailureTime) > _resetTimeout)
                {
                    _isTripped = false;
                    _failureCount = 0;
                }
                return _isTripped;
            }
        }

        public void RecordFailure()
        {
            if (++_failureCount >= _failureThreshold)
            {
                Trip();
            }
        }

        public void Reset() => _failureCount = 0;

        private void Trip()
        {
            _isTripped = true;
            _lastFailureTime = DateTime.UtcNow;
        }
    }
}
