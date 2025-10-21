namespace LablabBean.Plugins.Contracts;

using System.Collections.Generic;

/// <summary>
/// Registry for querying plugin descriptors and states.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// Get all registered plugin descriptors.
    /// </summary>
    IReadOnlyCollection<PluginDescriptor> GetAll();
    
    /// <summary>
    /// Get a plugin descriptor by ID.
    /// </summary>
    PluginDescriptor? GetById(string id);
}

/// <summary>
/// Plugin lifecycle state.
/// </summary>
public enum PluginState
{
    Created,
    Initialized,
    Started,
    Failed,
    Stopped,
    Unloaded
}

/// <summary>
/// Plugin descriptor with state and metadata.
/// </summary>
public sealed class PluginDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required PluginState State { get; set; }
    public required PluginManifest Manifest { get; init; }
    public string? FailureReason { get; set; }
}
