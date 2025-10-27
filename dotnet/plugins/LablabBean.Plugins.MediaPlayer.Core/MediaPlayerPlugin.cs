using LablabBean.Contracts.Media;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.MediaPlayer.Core.Detectors;
using LablabBean.Plugins.MediaPlayer.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.MediaPlayer.Core;

/// <summary>
/// Plugin registration for core media player services
/// </summary>
public class MediaPlayerPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "media-player-core";
    public string Name => "Media Player Core";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Media Player Core plugin");

        // Register services
        RegisterServices(context);

        _logger.LogInformation("Media Player Core plugin initialized successfully");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Media Player Core plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Media Player Core plugin stopped");
        return Task.CompletedTask;
    }

    private  void RegisterServices(IPluginContext context)
    {
        // Register terminal capability detector
        // Create a typed logger wrapper using the plugin's logger
        var detectorLogger = new TypedLoggerWrapper<TerminalCapabilityDetector>(context.Logger);
        var capabilityDetector = new TerminalCapabilityDetector(detectorLogger);
        context.Registry.Register<ITerminalCapabilityDetector>(capabilityDetector, priority: 100);

        // Note: MediaService will be registered by the host application after all engines and renderers
        // are discovered from other plugins. This plugin only provides the core infrastructure.
        _logger?.LogInformation("Registered ITerminalCapabilityDetector");
        _logger?.LogInformation("Media player infrastructure ready (engines and renderers from other plugins)");
    }
}

/// <summary>
/// Simple wrapper to adapt ILogger to ILogger&lt;T&gt;
/// </summary>
internal class TypedLoggerWrapper<T> : ILogger<T>
{
    private readonly ILogger _logger;

    public TypedLoggerWrapper(ILogger logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _logger.Log(logLevel, eventId, state, exception, formatter);
}
