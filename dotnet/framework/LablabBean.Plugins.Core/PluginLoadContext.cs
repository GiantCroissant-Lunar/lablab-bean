namespace LablabBean.Plugins.Core;

using System;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System.Linq;

/// <summary>
/// AssemblyLoadContext for plugin isolation with optional collectible support for hot reload.
/// </summary>
public sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath, bool isCollectible = false)
        : base(name: pluginPath, isCollectible: isCollectible)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Share contracts assembly between plugin and host (avoid ALC boundary issues)
        if (assemblyName.Name == "LablabBean.Plugins.Contracts"
            || assemblyName.Name == "LablabBean.Rendering.Contracts"
            || assemblyName.Name == "LablabBean.Contracts.UI"
            || assemblyName.Name == "LablabBean.Contracts.Game"
            || assemblyName.Name == "LablabBean.Contracts.Game.UI"
            || assemblyName.Name == "Terminal.Gui")
        {
            // First check if already loaded in Default ALC
            var already = AssemblyLoadContext.Default.Assemblies
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
            if (already != null)
            {
                return already;
            }

            // Try to preload from app base directory into Default ALC
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var candidate = Path.Combine(baseDir, assemblyName.Name + ".dll");
                if (File.Exists(candidate))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate);
                }
            }
            catch
            {
                // Fallback: let Default ALC resolve it
            }
            return null; // Bind to Default ALC
        }

        // Share Microsoft.Extensions.* assemblies (logging, DI, config abstractions)
        if (assemblyName.Name?.StartsWith("Microsoft.Extensions.") == true)
        {
            return null; // Let the default ALC handle it
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
