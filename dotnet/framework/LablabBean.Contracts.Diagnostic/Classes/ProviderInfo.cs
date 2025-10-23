using System;
using System.Collections.Generic;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Information about a diagnostic provider.
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Provider name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Provider type.
    /// </summary>
    public ProviderType Type { get; set; }

    /// <summary>
    /// Provider version.
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// Provider description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Whether the provider is currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Whether the provider is available/installed.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Provider health status.
    /// </summary>
    public SystemHealth Health { get; set; }

    /// <summary>
    /// Supported capabilities.
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Provider configuration options.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();

    /// <summary>
    /// Last health check time.
    /// </summary>
    public DateTime? LastHealthCheck { get; set; }

    /// <summary>
    /// Last error message (if any).
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Provider initialization time.
    /// </summary>
    public DateTime? InitializedAt { get; set; }

    /// <summary>
    /// Total number of data collections performed.
    /// </summary>
    public long CollectionCount { get; set; }

    /// <summary>
    /// Average collection time in milliseconds.
    /// </summary>
    public double AverageCollectionTime { get; set; }

    /// <summary>
    /// Last collection time.
    /// </summary>
    public DateTime? LastCollectionTime { get; set; }

    /// <summary>
    /// Provider-specific metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
