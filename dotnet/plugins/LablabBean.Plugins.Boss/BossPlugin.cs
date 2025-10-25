using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Boss;

/// <summary>
/// Minimal Boss plugin stub to satisfy manifest entry type.
/// </summary>
public class BossPlugin : IPlugin
{
    public string Id => "boss";
    public string Name => "Boss System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        context.Logger.LogInformation("Boss plugin initialized (stub)");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
