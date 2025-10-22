using System;
using System.Collections.Generic;
using System.Linq;

namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Result of report generation.
/// </summary>
public class ReportResult
{
    /// <summary>
    /// True if report was generated successfully.
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Path to generated report file.
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Time taken to generate report.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Errors encountered during generation.
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Warnings (non-fatal issues).
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    public static ReportResult Success(string outputPath, long fileSizeBytes = 0, TimeSpan duration = default)
        => new()
        {
            IsSuccess = true,
            OutputPath = outputPath,
            FileSizeBytes = fileSizeBytes,
            Duration = duration
        };
    
    public static ReportResult Failure(params string[] errors)
        => new()
        {
            IsSuccess = false,
            Errors = errors.ToList()
        };
}
