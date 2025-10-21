namespace LablabBean.Plugins.Contracts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Plugin lifecycle contract. Plugins implement this interface to integrate with the host.
/// </summary>
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    /// <summary>
    /// Initialize plugin with runtime context. Use context.Registry to register services.
    /// </summary>
    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);

    /// <summary>
    /// Start async background work (e.g., hosted services, event subscriptions).
    /// </summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stop async work and release resources before unload.
    /// </summary>
    Task StopAsync(CancellationToken ct = default);
}

/// <summary>
/// Plugin initialization context provided by host at runtime.
/// Isolates ALC boundary by not exposing IServiceCollection directly.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Service registry for cross-ALC service registration (priority-based, runtime type matching).
    /// </summary>
    IRegistry Registry { get; }

    /// <summary>
    /// Host configuration (read-only).
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Logger instance for this plugin (category = plugin ID).
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Host services surface (events, logging, service provider for host-provided services).
    /// </summary>
    IPluginHost Host { get; }
}
