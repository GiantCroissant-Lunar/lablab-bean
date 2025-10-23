using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.ServiceHealth;

public class HealthCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public Exception? Exception { get; set; }
}

public class SystemHealthReport
{
    public HealthStatus OverallStatus { get; set; }
    public DateTime ReportedAt { get; set; }
    public TimeSpan TotalCheckDuration { get; set; }
    public List<HealthCheckResult> ServiceChecks { get; set; } = new();
    public Dictionary<string, object> SystemMetrics { get; set; } = new();
}

public class HealthCheckConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool Enabled { get; set; } = true;
    public HealthCheckType CheckType { get; set; } = HealthCheckType.Readiness;
}
