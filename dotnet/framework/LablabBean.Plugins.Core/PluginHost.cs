namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Default implementation of IPluginHost.
/// </summary>
public sealed class PluginHost : IPluginHost
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _services;

    public PluginHost(ILoggerFactory loggerFactory, IServiceProvider services)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggerFactory.CreateLogger(categoryName);
    }

    public IServiceProvider Services => _services;

    public void PublishEvent<T>(T evt)
    {
        // TODO: Implement event bus integration when MessagePipe or similar is added
        // For now, just log the event
        var logger = _loggerFactory.CreateLogger<PluginHost>();
        logger.LogDebug("Event published: {EventType}", typeof(T).Name);
    }
}
