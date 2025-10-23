using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.ServiceHealth.Services;

public interface IService
{
    Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName, CancellationToken cancellationToken);
    Task<SystemHealthReport> CheckSystemHealthAsync(CancellationToken cancellationToken);
    void RegisterHealthCheck(string serviceName, Func<CancellationToken, Task<HealthCheckResult>> healthCheck, HealthCheckConfig? config);
    void UnregisterHealthCheck(string serviceName);
    IEnumerable<string> GetRegisteredServices();
    HealthCheckConfig GetHealthCheckConfig(string serviceName);
    void UpdateHealthCheckConfig(string serviceName, HealthCheckConfig config);
    Task StartMonitoringAsync(CancellationToken cancellationToken);
    Task StopMonitoringAsync();
    bool IsMonitoring { get; }
    event EventHandler<HealthCheckResult>? HealthCheckCompleted;
    event EventHandler<SystemHealthReport>? SystemHealthReportGenerated;
}
