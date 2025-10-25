// Progression Plugin: Character leveling and experience system
using Arch.Core;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Progression.Services;
using LablabBean.Plugins.Progression.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Progression;

/// <summary>
/// Plugin for character progression (experience, leveling, stat growth).
/// Provides ProgressionService for XP management.
/// </summary>
public class ProgressionPlugin : IPlugin
{
    private World? _world;
    private ExperienceSystem? _experienceSystem;
    private LevelingSystem? _levelingSystem;
    private ProgressionService? _progressionService;

    public string Id => "progression";
    public string Name => "Progression System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        // Gracefully handle missing World service in host
        if (!context.Registry.IsRegistered<World>())
        {
            context.Logger.LogWarning("World service not found; Progression plugin will initialize in passive mode.");
            return Task.CompletedTask;
        }

        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        // Initialize systems
        _experienceSystem = new ExperienceSystem(_world);
        _levelingSystem = new LevelingSystem(_world);

        // Initialize service
        _progressionService = new ProgressionService(_world, _experienceSystem, _levelingSystem);

        // Register service in plugin context
        context.Registry.Register<ProgressionService>(_progressionService, priority: 100);

        // Register systems for external access
        if (_experienceSystem != null)
            context.Registry.Register<ExperienceSystem>(_experienceSystem, priority: 100);
        if (_levelingSystem != null)
            context.Registry.Register<LevelingSystem>(_levelingSystem, priority: 100);

        context.Logger.LogInformation("Progression plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
