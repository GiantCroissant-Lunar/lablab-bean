// Contract: IPluginHost
// Purpose: Host surface available to plugins for logging, events, and services
// Location: LablabBean.Plugins.Contracts/IPluginHost.cs

namespace LablabBean.Plugins.Contracts;

using Microsoft.Extensions.Logging;

public interface IPluginHost
{
    ILogger CreateLogger(string categoryName);
    IServiceProvider Services { get; }
    void PublishEvent<T>(T evt);
}
