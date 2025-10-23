using System;
using System.Collections.Generic;

namespace LablabBean.Reporting.Contracts.Models;

/// <summary>
/// Data model for build metrics reports.
/// Maps to FR-020 through FR-025 (build metrics requirements).
/// </summary>
public class BuildMetricsData
{
    // Test Results (FR-020, FR-021)
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public decimal PassPercentage { get; set; }
    public List<TestResult> FailedTestDetails { get; set; } = new();

    // Code Coverage (FR-022)
    public decimal LineCoveragePercentage { get; set; }
    public decimal BranchCoveragePercentage { get; set; }
    public List<FileCoverage> LowCoverageFiles { get; set; } = new();

    // Build Timing (FR-023)
    public TimeSpan BuildDuration { get; set; }
    public DateTime BuildStartTime { get; set; }
    public DateTime BuildEndTime { get; set; }

    // Metadata (FR-024, FR-025)
    public string BuildNumber { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual test result.
/// </summary>
public class TestResult
{
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}

/// <summary>
/// File-level code coverage.
/// </summary>
public class FileCoverage
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => System.IO.Path.GetFileName(FilePath);
    public decimal CoveragePercentage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
}
