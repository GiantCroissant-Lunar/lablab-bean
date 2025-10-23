using System;
using LablabBean.Contracts.Resilience.Interfaces;

namespace LablabBean.Plugins.Resilience.Polly.Providers;

public class PollyRetryPolicy : IRetryPolicy
{
    public int MaxAttempts { get; }
    public TimeSpan BaseDelay { get; }
    public TimeSpan MaxDelay { get; }

    public PollyRetryPolicy(int maxAttempts, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        MaxAttempts = maxAttempts;
        BaseDelay = baseDelay;
        MaxDelay = maxDelay;
    }

    public TimeSpan GetNextDelay(int attemptNumber)
    {
        var delay = TimeSpan.FromMilliseconds(
            Math.Min(
                BaseDelay.TotalMilliseconds * Math.Pow(2, attemptNumber - 1),
                MaxDelay.TotalMilliseconds
            )
        );
        return delay;
    }

    public bool ShouldRetry(int attemptNumber, Exception exception)
    {
        return attemptNumber < MaxAttempts;
    }
}
