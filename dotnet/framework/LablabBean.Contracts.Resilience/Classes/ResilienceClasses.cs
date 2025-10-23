using System;

namespace LablabBean.Contracts.Resilience;

public class ResilienceHealthInfo
{
    public bool IsHealthy { get; set; }
    public int TotalCircuitBreakers { get; set; }
    public int OpenCircuitBreakers { get; set; }
    public int HalfOpenCircuitBreakers { get; set; }
    public DateTime LastHealthCheck { get; set; }
}

public class ResilienceDebugInfo
{
    public int ActiveCircuitBreakers { get; set; }
    public int TotalRetries { get; set; }
    public int SuccessfulRetries { get; set; }
    public int FailedRetries { get; set; }
    public DateTime LastActivity { get; set; }
}
