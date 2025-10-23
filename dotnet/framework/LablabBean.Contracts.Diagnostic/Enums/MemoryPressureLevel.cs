namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Memory pressure level enumeration.
/// </summary>
public enum MemoryPressureLevel
{
    /// <summary>
    /// Normal memory pressure.
    /// </summary>
    Normal,

    /// <summary>
    /// Low memory pressure.
    /// </summary>
    Low,

    /// <summary>
    /// Medium memory pressure.
    /// </summary>
    Medium,

    /// <summary>
    /// High memory pressure.
    /// </summary>
    High,

    /// <summary>
    /// Critical memory pressure.
    /// </summary>
    Critical
}
