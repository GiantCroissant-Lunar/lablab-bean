using System;

namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Metadata about a report provider.
/// </summary>
public class ReportMetadata
{
    /// <summary>
    /// Provider name (matches ReportProviderAttribute.Name).
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Category (Build, Analytics, Diagnostics).
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Supported output formats.
    /// </summary>
    public ReportFormat[] SupportedFormats { get; set; } = Array.Empty<ReportFormat>();
    
    /// <summary>
    /// Expected data source pattern (e.g., "*.xml", "*.jsonl").
    /// </summary>
    public string? DataSourcePattern { get; set; }
}
