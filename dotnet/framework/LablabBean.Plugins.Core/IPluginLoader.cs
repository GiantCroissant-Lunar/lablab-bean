namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Platform-agnostic abstraction for loading and managing plugins.
/// Implementations handle platform-specific loading mechanisms (ALC, HybridCLR, etc.).
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Gets the plugin registry containing all discovered and loaded plugins.
    /// </summary>
    IPluginRegistry PluginRegistry { get; }

    /// <summary>
    /// Gets the service registry for plugin service registration.
    /// </summary>
    IRegistry ServiceRegistry { get; }

    /// <summary>
    /// Discover and load plugins from the specified directories.
    /// </summary>
    /// <param name="pluginPaths">Directories to scan for plugins.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of plugins successfully loaded.</returns>
    Task<int> DiscoverAndLoadAsync(IEnumerable<string> pluginPaths, CancellationToken ct = default);

    /// <summary>
    /// Unload a specific plugin.
    /// </summary>
    /// <param name="pluginId">Plugin identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UnloadPluginAsync(string pluginId, CancellationToken ct = default);

    /// <summary>
    /// Stop and unload all plugins.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task UnloadAllAsync(CancellationToken ct = default);
}
