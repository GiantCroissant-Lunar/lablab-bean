using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Container for diagnostic data collected from providers.
/// </summary>
public class DiagnosticData
{
    /// <summary>
    /// Provider that collected this data.
    /// </summary>
    public string ProviderName { get; set; } = "";

    /// <summary>
    /// Provider type.
    /// </summary>
    public ProviderType ProviderType { get; set; }

    /// <summary>
    /// When the data was collected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Data collection duration.
    /// </summary>
    public TimeSpan CollectionDuration { get; set; }

    /// <summary>
    /// Performance metrics.
    /// </summary>
    public PerformanceMetrics? Performance { get; set; }

    /// <summary>
    /// Memory information.
    /// </summary>
    public MemoryInfo? Memory { get; set; }

    /// <summary>
    /// CPU information.
    /// </summary>
    public CpuInfo? Cpu { get; set; }

    /// <summary>
    /// GPU information.
    /// </summary>
    public GpuInfo? Gpu { get; set; }

    /// <summary>
    /// Device information.
    /// </summary>
    public DeviceInfo? Device { get; set; }

    /// <summary>
    /// System health status.
    /// </summary>
    public SystemHealth Health { get; set; }

    /// <summary>
    /// Custom metrics from the provider.
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; set; } = new();

    /// <summary>
    /// Additional metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Provider-specific data.
    /// </summary>
    public object? ProviderSpecificData { get; set; }

    /// <summary>
    /// Errors encountered during collection.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Warnings encountered during collection.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
