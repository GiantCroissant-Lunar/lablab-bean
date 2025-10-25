using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Hazards;

/// <summary>
/// Environmental Hazards plugin - adds traps, lava, and other dangerous terrain
/// </summary>
public class HazardsPlugin : IPlugin
{
    public string Id => "hazards";
    public string Name => "Environmental Hazards System";
    public string Version => "1.0.0";
    public string Description => "Adds environmental hazards like traps, lava, poison gas, and other dangerous terrain elements";

    private IPluginContext? _context;

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _context = context;

        context.Logger.LogInformation("Environmental Hazards plugin initialized");
        context.Logger.LogInformation("  - 10 hazard types available");
        context.Logger.LogInformation("  - Detection and disarming system ready");
        context.Logger.LogInformation("  - Ongoing effects (burning, poison) enabled");
        context.Logger.LogInformation("  - Components: Hazard, HazardEffect, HazardTrigger, HazardResistance");
        context.Logger.LogInformation("  - Systems: HazardSystem, HazardDetectionSystem");
        context.Logger.LogInformation("  - Services: HazardService, HazardFactory");

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Environmental Hazards plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Environmental Hazards plugin stopped");
        return Task.CompletedTask;
    }
}
