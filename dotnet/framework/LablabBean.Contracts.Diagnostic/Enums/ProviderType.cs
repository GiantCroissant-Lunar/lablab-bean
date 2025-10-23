namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Types of diagnostic providers.
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// Unity built-in diagnostics
    /// </summary>
    Unity = 0,

    /// <summary>
    /// OpenTelemetry provider
    /// </summary>
    OpenTelemetry = 1,

    /// <summary>
    /// Sentry error tracking provider
    /// </summary>
    Sentry = 2,

    /// <summary>
    /// System monitoring provider
    /// </summary>
    System = 3,

    /// <summary>
    /// Performance monitoring provider
    /// </summary>
    Performance = 4,

    /// <summary>
    /// Memory monitoring provider
    /// </summary>
    Memory = 5,

    /// <summary>
    /// Custom diagnostic provider
    /// </summary>
    Custom = 6
}
