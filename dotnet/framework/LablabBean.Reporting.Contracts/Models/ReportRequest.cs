using System.Collections.Generic;

namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Request parameters for report generation.
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// Desired output format.
    /// </summary>
    public ReportFormat Format { get; set; } = ReportFormat.HTML;

    /// <summary>
    /// Output file path (including extension).
    /// If null, generates default path in artifacts/reports/.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Path to data source (e.g., "TestResults/*.xml", "logs/analytics/session-123.jsonl").
    /// Provider determines how to interpret this.
    /// </summary>
    public string? DataPath { get; set; }

    /// <summary>
    /// Path to custom template file (.frx for FastReport).
    /// If null, uses embedded default template.
    /// </summary>
    public string? TemplatePath { get; set; }

    /// <summary>
    /// Additional provider-specific filters or options.
    /// </summary>
    public Dictionary<string, string> Options { get; set; } = new();
}
