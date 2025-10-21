// Contract: IPluginRegistry
// Purpose: Registry for querying plugin descriptors and states
// Location: LablabBean.Plugins.Contracts/IPluginRegistry.cs

namespace LablabBean.Plugins.Contracts;

public interface IPluginRegistry
{
    IReadOnlyCollection<PluginDescriptor> GetAll();
    PluginDescriptor? GetById(string id);
}

public enum PluginState { Created, Initialized, Started, Failed, Stopped, Unloaded }

public sealed class PluginDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required PluginState State { get; set; }
    public required PluginManifest Manifest { get; init; }
    public string? FailureReason { get; set; }
}
