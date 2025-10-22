using System;
using System.Collections.Generic;

namespace LablabBean.Reporting.Contracts.Models;

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
    public string State { get; set; } = string.Empty;
    
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
