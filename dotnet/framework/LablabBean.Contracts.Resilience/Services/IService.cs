using System;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Contracts.Resilience.Interfaces;

namespace LablabBean.Contracts.Resilience.Services;

public interface IService
{
    ICircuitBreaker CreateCircuitBreaker(string operationKey, int failureThreshold, TimeSpan? timeout);
    ICircuitBreaker GetCircuitBreaker(string operationKey);
    bool RemoveCircuitBreaker(string operationKey);
    IRetryPolicy CreateRetryPolicy(int maxAttempts, TimeSpan? baseDelay, TimeSpan? maxDelay);
    Task ExecuteWithRetryAsync(Func<Task> operation, IRetryPolicy? retryPolicy, CancellationToken cancellationToken);
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, IRetryPolicy? retryPolicy, CancellationToken cancellationToken);
    Task ExecuteWithResilienceAsync(string operationKey, Func<Task> operation, IRetryPolicy? retryPolicy, CancellationToken cancellationToken);
    Task<T> ExecuteWithResilienceAsync<T>(string operationKey, Func<Task<T>> operation, IRetryPolicy? retryPolicy, CancellationToken cancellationToken);
    ResilienceHealthInfo GetHealthStatus();
    ResilienceDebugInfo GetDebugInfo();
    void ResetAllCircuitBreakers();
    event Action<string, CircuitBreakerState>? CircuitBreakerStateChanged;
    event Action<string, Exception, int>? OperationRetrying;
    event Action<string>? OperationSucceededAfterRetry;
    event Action<string, Exception>? OperationFailed;
}
