using System;
using System.Threading;
using LablabBean.Contracts.Resilience;
using LablabBean.Contracts.Resilience.Interfaces;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Resilience.Polly.Providers;

public class PollyCircuitBreaker : ICircuitBreaker
{
    private readonly string _operationKey;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount;
    private DateTime? _lastFailureTime;
    private DateTime? _openedAt;

    public event Action<CircuitBreakerState>? StateChanged;

    public string OperationKey => _operationKey;
    public CircuitBreakerState State => _state;
    public int FailureCount => _failureCount;
    public DateTime? LastFailureTime => _lastFailureTime;

    public PollyCircuitBreaker(string operationKey, int failureThreshold, TimeSpan timeout, ILogger logger)
    {
        _operationKey = operationKey;
        _failureThreshold = failureThreshold;
        _timeout = timeout;
        _logger = logger;
    }

    public void RecordSuccess()
    {
        if (_state == CircuitBreakerState.HalfOpen)
        {
            _logger.LogInformation("Circuit breaker {OperationKey} transitioning to Closed", _operationKey);
            TransitionTo(CircuitBreakerState.Closed);
            Interlocked.Exchange(ref _failureCount, 0);
            _lastFailureTime = null;
            _openedAt = null;
        }
        else if (_state == CircuitBreakerState.Closed)
        {
            Interlocked.Exchange(ref _failureCount, 0);
        }
    }

    public void RecordFailure(Exception exception)
    {
        _lastFailureTime = DateTime.UtcNow;
        var newCount = Interlocked.Increment(ref _failureCount);

        _logger.LogWarning(exception, "Circuit breaker {OperationKey} recorded failure ({Count}/{Threshold})",
            _operationKey, newCount, _failureThreshold);

        if (_state == CircuitBreakerState.HalfOpen)
        {
            _logger.LogWarning("Circuit breaker {OperationKey} transitioning back to Open", _operationKey);
            TransitionTo(CircuitBreakerState.Open);
            _openedAt = DateTime.UtcNow;
        }
        else if (_state == CircuitBreakerState.Closed && newCount >= _failureThreshold)
        {
            _logger.LogWarning("Circuit breaker {OperationKey} transitioning to Open", _operationKey);
            TransitionTo(CircuitBreakerState.Open);
            _openedAt = DateTime.UtcNow;
        }
    }

    public void Reset()
    {
        _logger.LogInformation("Circuit breaker {OperationKey} manually reset", _operationKey);
        Interlocked.Exchange(ref _failureCount, 0);
        _lastFailureTime = null;
        _openedAt = null;
        TransitionTo(CircuitBreakerState.Closed);
    }

    public bool IsOperationAllowed()
    {
        if (_state == CircuitBreakerState.Closed)
        {
            return true;
        }

        if (_state == CircuitBreakerState.Open)
        {
            if (_openedAt.HasValue && DateTime.UtcNow - _openedAt.Value >= _timeout)
            {
                _logger.LogInformation("Circuit breaker {OperationKey} transitioning to HalfOpen", _operationKey);
                TransitionTo(CircuitBreakerState.HalfOpen);
                return true;
            }
            return false;
        }

        return _state == CircuitBreakerState.HalfOpen;
    }

    private void TransitionTo(CircuitBreakerState newState)
    {
        var oldState = _state;
        _state = newState;
        if (oldState != newState)
        {
            StateChanged?.Invoke(newState);
        }
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
