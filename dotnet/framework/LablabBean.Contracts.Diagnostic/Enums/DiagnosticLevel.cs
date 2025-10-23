namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Diagnostic severity levels.
/// </summary>
public enum DiagnosticLevel
{
    /// <summary>
    /// Trace level - detailed debugging information
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Debug level - debugging information
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Information level - general information
    /// </summary>
    Information = 2,

    /// <summary>
    /// Warning level - potentially harmful situations
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error level - error events but application may continue
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical level - very severe error events
    /// </summary>
    Critical = 5
}
