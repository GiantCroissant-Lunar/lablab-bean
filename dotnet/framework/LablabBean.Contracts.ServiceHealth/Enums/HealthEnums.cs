namespace LablabBean.Contracts.ServiceHealth;

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}

public enum HealthCheckType
{
    Liveness,
    Readiness,
    Startup
}
