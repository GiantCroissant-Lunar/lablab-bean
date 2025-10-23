using System;

namespace LablabBean.Contracts.Resilience.Interfaces;

public interface ICircuitBreaker : IDisposable
{
    string OperationKey { get; }
    CircuitBreakerState State { get; }
    int FailureCount { get; }
    DateTime? LastFailureTime { get; }
    void RecordSuccess();
    void RecordFailure(Exception exception);
    void Reset();
    bool IsOperationAllowed();
}

public interface IRetryPolicy
{
    int MaxAttempts { get; }
    TimeSpan GetNextDelay(int attemptNumber);
    bool ShouldRetry(int attemptNumber, Exception exception);
}
