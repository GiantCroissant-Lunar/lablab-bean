namespace LablabBean.Plugins.Core;

using System;
using System.Reflection;
using System.Runtime.Loader;

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
        if (assemblyName.Name == "LablabBean.Plugins.Contracts")
        {
            return null; // Let the default ALC handle it
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
