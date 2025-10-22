namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Supported report output formats.
/// </summary>
public enum ReportFormat
{
    /// <summary>HTML format (default)</summary>
    HTML,
    
    /// <summary>PDF format</summary>
    PDF,
    
    /// <summary>PNG image format</summary>
    PNG,
    
    /// <summary>CSV format (future)</summary>
    CSV
}
