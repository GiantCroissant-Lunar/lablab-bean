namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Export formats for diagnostic data.
/// </summary>
public enum DiagnosticExportFormat
{
    /// <summary>
    /// JSON format
    /// </summary>
    Json = 0,

    /// <summary>
    /// XML format
    /// </summary>
    Xml = 1,

    /// <summary>
    /// CSV format
    /// </summary>
    Csv = 2,

    /// <summary>
    /// Plain text format
    /// </summary>
    Text = 3,

    /// <summary>
    /// OpenTelemetry format
    /// </summary>
    OpenTelemetry = 4,

    /// <summary>
    /// Sentry format
    /// </summary>
    Sentry = 5
}
