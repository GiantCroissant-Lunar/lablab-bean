namespace LablabBean.Plugins.Contracts;

using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Host surface available to plugins for logging, events, and services.
/// </summary>
public interface IPluginHost
{
    /// <summary>
    /// Create a logger with the specified category name.
    /// </summary>
    ILogger CreateLogger(string categoryName);
    
    /// <summary>
    /// Service provider for host-provided services.
    /// </summary>
    IServiceProvider Services { get; }
    
    /// <summary>
    /// Publish an event to the host event bus.
    /// </summary>
    void PublishEvent<T>(T evt);
}
