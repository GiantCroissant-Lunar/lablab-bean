using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Resilience;
using LablabBean.Contracts.Resilience.Interfaces;
using LablabBean.Contracts.Resilience.Services;
using LablabBean.Plugins.Resilience.Polly.Providers;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ContractIRetryPolicy = LablabBean.Contracts.Resilience.Interfaces.IRetryPolicy;

namespace LablabBean.Plugins.Resilience.Polly.Services;

public class ResilienceService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, PollyCircuitBreaker> _circuitBreakers = new();
    private int _totalRetries;
    private int _successfulRetries;
    private int _failedRetries;
    private DateTime _lastActivity = DateTime.UtcNow;

    public event Action<string, CircuitBreakerState>? CircuitBreakerStateChanged;
    public event Action<string, Exception, int>? OperationRetrying;
    public event Action<string>? OperationSucceededAfterRetry;
    public event Action<string, Exception>? OperationFailed;

    public ResilienceService(ILogger logger)
    {
        _logger = logger;
    }

    public ICircuitBreaker CreateCircuitBreaker(string operationKey, int failureThreshold, TimeSpan? timeout)
    {
        var cb = new PollyCircuitBreaker(
            operationKey,
            failureThreshold,
            timeout ?? TimeSpan.FromSeconds(30),
            _logger
        );

        cb.StateChanged += (state) =>
        {
            CircuitBreakerStateChanged?.Invoke(operationKey, state);
        };

        _circuitBreakers[operationKey] = cb;
        _logger.LogInformation("Created circuit breaker for {OperationKey}", operationKey);
        return cb;
    }

    public ICircuitBreaker GetCircuitBreaker(string operationKey)
    {
        if (_circuitBreakers.TryGetValue(operationKey, out var cb))
        {
            return cb;
        }
        throw new InvalidOperationException($"Circuit breaker not found: {operationKey}");
    }

    public bool RemoveCircuitBreaker(string operationKey)
    {
        if (_circuitBreakers.TryRemove(operationKey, out var cb))
        {
            cb.Dispose();
            _logger.LogInformation("Removed circuit breaker for {OperationKey}", operationKey);
            return true;
        }
        return false;
    }

    public ContractIRetryPolicy CreateRetryPolicy(int maxAttempts, TimeSpan? baseDelay, TimeSpan? maxDelay)
    {
        return new PollyRetryPolicy(
            maxAttempts,
            baseDelay ?? TimeSpan.FromMilliseconds(100),
            maxDelay ?? TimeSpan.FromSeconds(10)
        );
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, ContractIRetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        var policy = retryPolicy as PollyRetryPolicy ?? CreateDefaultRetryPolicy();
        var attemptNumber = 0;

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = policy.MaxAttempts,
                Delay = policy.BaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    attemptNumber++;
                    Interlocked.Increment(ref _totalRetries);
                    _lastActivity = DateTime.UtcNow;
                    OperationRetrying?.Invoke("operation", args.Outcome.Exception!, attemptNumber);
                    _logger.LogWarning("Retry attempt {Attempt} after {Delay}ms", attemptNumber, args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            await pipeline.ExecuteAsync(async ct => await operation(), cancellationToken);
            
            if (attemptNumber > 0)
            {
                Interlocked.Increment(ref _successfulRetries);
                OperationSucceededAfterRetry?.Invoke("operation");
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedRetries);
            OperationFailed?.Invoke("operation", ex);
            throw;
        }
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, ContractIRetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        var policy = retryPolicy as PollyRetryPolicy ?? CreateDefaultRetryPolicy();
        var attemptNumber = 0;

        var pipeline = new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = policy.MaxAttempts,
                Delay = policy.BaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    attemptNumber++;
                    Interlocked.Increment(ref _totalRetries);
                    _lastActivity = DateTime.UtcNow;
                    OperationRetrying?.Invoke("operation", args.Outcome.Exception!, attemptNumber);
                    _logger.LogWarning("Retry attempt {Attempt} after {Delay}ms", attemptNumber, args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        try
        {
            var result = await pipeline.ExecuteAsync(async ct => await operation(), cancellationToken);
            
            if (attemptNumber > 0)
            {
                Interlocked.Increment(ref _successfulRetries);
                OperationSucceededAfterRetry?.Invoke("operation");
            }
            
            return result!;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedRetries);
            OperationFailed?.Invoke("operation", ex);
            throw;
        }
    }

    public async Task ExecuteWithResilienceAsync(string operationKey, Func<Task> operation, ContractIRetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        if (!_circuitBreakers.TryGetValue(operationKey, out var cb))
        {
            throw new InvalidOperationException($"Circuit breaker not found: {operationKey}");
        }

        if (!cb.IsOperationAllowed())
        {
            throw new InvalidOperationException($"Circuit breaker is open for: {operationKey}");
        }

        try
        {
            await ExecuteWithRetryAsync(operation, retryPolicy, cancellationToken);
            cb.RecordSuccess();
        }
        catch (Exception ex)
        {
            cb.RecordFailure(ex);
            throw;
        }
    }

    public async Task<T> ExecuteWithResilienceAsync<T>(string operationKey, Func<Task<T>> operation, ContractIRetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        if (!_circuitBreakers.TryGetValue(operationKey, out var cb))
        {
            throw new InvalidOperationException($"Circuit breaker not found: {operationKey}");
        }

        if (!cb.IsOperationAllowed())
        {
            throw new InvalidOperationException($"Circuit breaker is open for: {operationKey}");
        }

        try
        {
            var result = await ExecuteWithRetryAsync(operation, retryPolicy, cancellationToken);
            cb.RecordSuccess();
            return result;
        }
        catch (Exception ex)
        {
            cb.RecordFailure(ex);
            throw;
        }
    }

    public ResilienceHealthInfo GetHealthStatus()
    {
        var openCount = 0;
        var halfOpenCount = 0;

        foreach (var cb in _circuitBreakers.Values)
        {
            if (cb.State == CircuitBreakerState.Open) openCount++;
            else if (cb.State == CircuitBreakerState.HalfOpen) halfOpenCount++;
        }

        return new ResilienceHealthInfo
        {
            IsHealthy = openCount == 0,
            TotalCircuitBreakers = _circuitBreakers.Count,
            OpenCircuitBreakers = openCount,
            HalfOpenCircuitBreakers = halfOpenCount,
            LastHealthCheck = DateTime.UtcNow
        };
    }

    public ResilienceDebugInfo GetDebugInfo()
    {
        return new ResilienceDebugInfo
        {
            ActiveCircuitBreakers = _circuitBreakers.Count,
            TotalRetries = _totalRetries,
            SuccessfulRetries = _successfulRetries,
            FailedRetries = _failedRetries,
            LastActivity = _lastActivity
        };
    }

    public void ResetAllCircuitBreakers()
    {
        foreach (var cb in _circuitBreakers.Values)
        {
            cb.Reset();
        }
        _logger.LogInformation("Reset all circuit breakers");
    }

    private PollyRetryPolicy CreateDefaultRetryPolicy()
    {
        return new PollyRetryPolicy(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        foreach (var cb in _circuitBreakers.Values)
        {
            cb.Dispose();
        }
        _circuitBreakers.Clear();
    }
}
