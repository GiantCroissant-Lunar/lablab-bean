namespace LablabBean.Plugins.Core;

using System.Collections.Generic;

/// <summary>
/// Configuration options for the plugin system.
/// </summary>
public sealed class PluginOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Plugins";

    /// <summary>
    /// Plugin search paths.
    /// </summary>
    public List<string> Paths { get; set; } = new();

    /// <summary>
    /// Default plugin path if Paths is empty.
    /// </summary>
    public string DefaultPath { get; set; } = "plugins";

    /// <summary>
    /// Enable hot reload support (collectible AssemblyLoadContext).
    /// </summary>
    public bool HotReload { get; set; } = false;

    /// <summary>
    /// Current profile (e.g., "dotnet.console", "dotnet.sadconsole", "unity").
    /// </summary>
    public string Profile { get; set; } = "dotnet.console";
}
