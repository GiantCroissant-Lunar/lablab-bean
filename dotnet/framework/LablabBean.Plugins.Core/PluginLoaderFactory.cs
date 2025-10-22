namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Factory for creating platform-specific plugin loaders.
/// </summary>
public static class PluginLoaderFactory
{
    /// <summary>
    /// Creates a plugin loader for the current platform.
    /// Currently uses AssemblyLoadContext for .NET environments.
    /// Future: Can detect platform and return HybridCLR loader for Unity, etc.
    /// </summary>
    public static IPluginLoader Create(
        ILogger<PluginLoader> logger,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IServiceProvider services,
        PluginRegistry pluginRegistry,
        ServiceRegistry serviceRegistry,
        bool enableHotReload = false,
        string profile = "dotnet.console",
        PluginSystemMetrics? metrics = null)
    {
        // For .NET environments, use AssemblyLoadContext-based loader
        return new PluginLoader(
            logger,
            loggerFactory,
            configuration,
            services,
            pluginRegistry,
            serviceRegistry,
            enableHotReload,
            profile,
            metrics);
    }
}
