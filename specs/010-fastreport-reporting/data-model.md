# Data Model: FastReport Reporting Infrastructure

**Spec**: [../spec.md](../spec.md) | **Plan**: [../plan.md](../plan.md) | **Research**: [../research.md](../research.md)  
**Date**: 2025-10-22  
**Status**: Draft

---

## Overview

This document defines the data models, interfaces, and contracts for the FastReport reporting infrastructure. All types are designed to support the three core report types (build metrics, session statistics, plugin health) with extensibility for future report types.

---

## Core Interfaces

### IReportProvider

Providers collect and prepare data for report generation. Each provider is marked with `[ReportProvider]` attribute for compile-time discovery.

```csharp
namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Provides data for report generation.
/// Implementations are discovered via [ReportProvider] attribute and source generator.
/// </summary>
public interface IReportProvider
{
    /// <summary>
    /// Retrieves report data based on the request parameters.
    /// </summary>
    /// <param name="request">Report request containing data path, filters, etc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report data object (typically BuildMetricsData, SessionStatisticsData, or PluginHealthData)</returns>
    Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets metadata about this provider (name, supported data sources, etc.)
    /// </summary>
    ReportMetadata GetMetadata();
}
```

**Usage Example**:
```csharp
[ReportProvider("BuildMetrics", "Build", priority: 0)]
public class BuildMetricsProvider : IReportProvider
{
    public async Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        var data = new BuildMetricsData();
        // Parse test results, coverage, build timing
        return data;
    }
    
    public ReportMetadata GetMetadata() => new()
    {
        Name = "BuildMetrics",
        Description = "Build metrics including tests, coverage, and timing",
        SupportedFormats = new[] { ReportFormat.HTML, ReportFormat.PDF }
    };
}
```

---

### IReportRenderer

Renderers transform data into output formats (HTML, PDF, PNG). FastReport plugin implements this interface.

```csharp
namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Renders report data to various output formats.
/// Typically implemented by reporting plugins (e.g., FastReportPlugin).
/// </summary>
public interface IReportRenderer
{
    /// <summary>
    /// Renders report data to the specified format.
    /// </summary>
    /// <param name="request">Report request containing format, output path, template, etc.</param>
    /// <param name="data">Report data from IReportProvider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing output file path and any errors</returns>
    Task<ReportResult> RenderAsync(ReportRequest request, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets supported output formats.
    /// </summary>
    IEnumerable<ReportFormat> SupportedFormats { get; }
}
```

**Usage Example**:
```csharp
public class FastReportRenderer : IReportRenderer
{
    public IEnumerable<ReportFormat> SupportedFormats => new[]
    {
        ReportFormat.HTML,
        ReportFormat.PDF,
        ReportFormat.PNG
    };
    
    public async Task<ReportResult> RenderAsync(ReportRequest request, object data, CancellationToken cancellationToken)
    {
        var report = new Report();
        report.Load(request.TemplatePath);
        report.RegisterData(data, "Data");
        report.Prepare();
        
        switch (request.Format)
        {
            case ReportFormat.PDF:
                report.Export(new PDFSimpleExport(), request.OutputPath);
                break;
            case ReportFormat.HTML:
                report.Export(new HTMLExport(), request.OutputPath);
                break;
        }
        
        return ReportResult.Success(request.OutputPath);
    }
}
```

---

### IReportingService

Orchestrator that coordinates providers and renderers. Resolves the correct provider, retrieves data, and invokes the renderer.

```csharp
namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Orchestrates report generation by coordinating providers and renderers.
/// Main entry point for report generation.
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates a report by resolving the provider, retrieving data, and rendering.
    /// </summary>
    /// <param name="providerName">Name of the provider (e.g., "BuildMetrics", "Session")</param>
    /// <param name="request">Report request with format, output path, etc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing output file path and any errors</returns>
    Task<ReportResult> GenerateReportAsync(string providerName, ReportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all available report providers.
    /// </summary>
    IEnumerable<ReportMetadata> GetAvailableProviders();
}
```

**Usage Example**:
```csharp
// From CLI command handler
var service = serviceProvider.GetRequiredService<IReportingService>();
var request = new ReportRequest
{
    Format = ReportFormat.HTML,
    OutputPath = "artifacts/reports/build-metrics.html",
    DataPath = "TestResults/*.xml"
};

var result = await service.GenerateReportAsync("BuildMetrics", request);
if (result.IsSuccess)
{
    Console.WriteLine($"Report generated: {result.OutputPath}");
}
```

---

## Attributes

### ReportProviderAttribute

Marks provider classes for compile-time discovery by the source generator.

```csharp
namespace LablabBean.Reporting.Abstractions.Attributes;

/// <summary>
/// Marks a class as a report provider for compile-time discovery.
/// Class must implement IReportProvider.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ReportProviderAttribute : Attribute
{
    /// <summary>
    /// Unique name of the provider (e.g., "BuildMetrics").
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Category for grouping providers (e.g., "Build", "Analytics", "Diagnostics").
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// Priority for provider selection when multiple providers exist in same category.
    /// Lower values = higher priority. Default is 0.
    /// </summary>
    public int Priority { get; }
    
    public ReportProviderAttribute(string name, string category, int priority = 0)
    {
        Name = name;
        Category = category;
        Priority = priority;
    }
}
```

**Categories**:
- `"Build"` - Build-time metrics (tests, coverage, timing)
- `"Analytics"` - Runtime analytics (session stats, player behavior)
- `"Diagnostics"` - System health (plugin status, performance)

---

## Data Models

### ReportRequest

Input parameters for report generation.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

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
```

---

### ReportResult

Output from report generation.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

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
```

---

### ReportMetadata

Provider metadata.

```csharp
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
```

---

### ReportFormat

Supported output formats.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

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
```

---

## Report-Specific Data Models

### BuildMetricsData

Build metrics including test results, code coverage, and build timing.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

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
    public List<FileCoverage> LowCoverageFiles { get; set; } = new(); // < 80%
    
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
    public string Result { get; set; } = string.Empty; // "Pass", "Fail", "Skip"
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
    public string FileName => Path.GetFileName(FilePath);
    public decimal CoveragePercentage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
}
```

**Mapping to Spec Requirements**:
- FR-020: Test counts (Total, Passed, Failed, Skipped)
- FR-021: Pass percentage calculation
- FR-022: Line/branch coverage, low-coverage file identification
- FR-023: Build duration, start/end timestamps
- FR-024: Build metadata (number, repo, branch, commit)
- FR-025: PDF export support

---

### SessionStatisticsData

Game session statistics including playtime, combat metrics, and progression.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Data model for game session statistics reports.
/// Maps to FR-026 through FR-032 (session statistics requirements).
/// </summary>
public class SessionStatisticsData
{
    // Session Metadata (FR-026)
    public string SessionId { get; set; } = string.Empty;
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public TimeSpan TotalPlaytime { get; set; }
    
    // Combat Statistics (FR-027, FR-028)
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public decimal KillDeathRatio { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public decimal AverageDamagePerKill { get; set; }
    
    // Progression (FR-029)
    public int ItemsCollected { get; set; }
    public int LevelsCompleted { get; set; }
    public int AchievementsUnlocked { get; set; }
    
    // Performance Metrics (FR-030)
    public int AverageFrameRate { get; set; }
    public TimeSpan TotalLoadTime { get; set; }
    
    // Event Timeline (FR-031)
    public List<SessionEvent> KeyEvents { get; set; } = new();
    
    // Metadata
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Significant event during session.
/// </summary>
public class SessionEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty; // "Kill", "Death", "LevelComplete", etc.
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

**Mapping to Spec Requirements**:
- FR-026: Playtime calculation
- FR-027: Kill/death tracking and K/D ratio
- FR-028: Damage dealt/taken, average damage
- FR-029: Item collection, level progression, achievements
- FR-030: Performance metrics (FPS, load times)
- FR-031: Key event timeline
- FR-032: CSV export support (deferred to future phase)

---

### PluginHealthData

Plugin system health including loaded plugins, memory usage, and load times.

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Data model for plugin system health reports.
/// Maps to FR-033 through FR-037 (plugin health requirements).
/// </summary>
public class PluginHealthData
{
    // Summary (FR-033)
    public int TotalPlugins { get; set; }
    public int RunningPlugins { get; set; }
    public int FailedPlugins { get; set; }
    public int DegradedPlugins { get; set; }
    public decimal SuccessRate { get; set; }
    
    // Individual Plugin Details (FR-034, FR-035, FR-036, FR-037)
    public List<PluginStatus> Plugins { get; set; } = new();
    
    // System Metrics
    public long TotalMemoryUsageMB { get; set; }
    public TimeSpan TotalLoadTime { get; set; }
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Status of an individual plugin.
/// </summary>
public class PluginStatus
{
    // Basic Info (FR-033)
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // "Running", "Failed", "Degraded", "Stopped"
    
    // Memory Usage (FR-035)
    public long MemoryUsageMB { get; set; }
    
    // Load Time (FR-036)
    public TimeSpan LoadDuration { get; set; }
    
    // Health Status (FR-034)
    public string? HealthStatusReason { get; set; }
    public DateTime? DegradedSince { get; set; }
    
    // Error Details (FR-033)
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}
```

**Mapping to Spec Requirements**:
- FR-033: Plugin state counts, failure reasons
- FR-034: Health status (Degraded state and reason)
- FR-035: Memory usage highlighting (>50 MB)
- FR-036: Load time tracking and sorting
- FR-037: Health state timestamp tracking

---

## Source Generator Output

The source generator creates a static registry based on discovered `[ReportProvider]` classes.

### Generated: ReportProviderRegistry.g.cs

```csharp
// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using LablabBean.Reporting.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Reporting.Generated
{
    /// <summary>
    /// Auto-generated registry of report providers discovered at compile time.
    /// </summary>
    public static class ReportProviderRegistry
    {
        /// <summary>
        /// All discovered report providers with metadata.
        /// </summary>
        public static IReadOnlyList<ProviderRegistration> Providers { get; } = new[]
        {
            new ProviderRegistration(typeof(LablabBean.Reporting.Build.BuildMetricsProvider), "BuildMetrics", "Build", 0),
            new ProviderRegistration(typeof(LablabBean.Reporting.Analytics.SessionStatisticsProvider), "Session", "Analytics", 0),
            new ProviderRegistration(typeof(LablabBean.Reporting.Analytics.PluginHealthProvider), "PluginHealth", "Diagnostics", 0)
        };
        
        /// <summary>
        /// Registers all discovered providers with the service collection.
        /// </summary>
        public static IServiceCollection AddReportProviders(this IServiceCollection services)
        {
            foreach (var provider in Providers)
            {
                services.AddTransient(typeof(IReportProvider), provider.Type);
            }
            return services;
        }
    }
    
    /// <summary>
    /// Registration information for a report provider.
    /// </summary>
    public record ProviderRegistration(Type Type, string Name, string Category, int Priority);
}
```

---

## Summary

This data model provides:

1. ✅ **Clear Contracts**: IReportProvider, IReportRenderer, IReportingService
2. ✅ **Attribute-Driven Discovery**: ReportProviderAttribute with name, category, priority
3. ✅ **Type-Safe Data Models**: BuildMetricsData, SessionStatisticsData, PluginHealthData
4. ✅ **Request/Result Pattern**: ReportRequest, ReportResult for orchestration
5. ✅ **Source Generator Output**: Static registry for zero-reflection lookup
6. ✅ **Spec Alignment**: All models map to FR-020 through FR-037

**Next Steps**: Implement Phase 2 (Abstractions library) using these contracts.
