namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// System health status enumeration.
/// </summary>
public enum SystemHealth
{
    /// <summary>
    /// Health status is unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// System is healthy and operating normally
    /// </summary>
    Healthy = 1,

    /// <summary>
    /// System has warnings but is still functional
    /// </summary>
    Warning = 2,

    /// <summary>
    /// System is in a degraded state
    /// </summary>
    Degraded = 3,

    /// <summary>
    /// System is in critical state
    /// </summary>
    Critical = 4,

    /// <summary>
    /// System is offline or unreachable
    /// </summary>
    Offline = 5
}
