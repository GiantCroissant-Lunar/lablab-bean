using Arch.Core;
using Microsoft.Extensions.Logging;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Quest.Services;
using LablabBean.Plugins.Quest.Systems;

namespace LablabBean.Plugins.Quest;

/// <summary>
/// Quest plugin - provides quest-driven exploration system
/// </summary>
public class QuestPlugin : IPlugin
{
    private World? _world;
    private QuestSystem? _questSystem;
    private QuestProgressSystem? _questProgressSystem;
    private QuestRewardSystem? _questRewardSystem;
    private QuestService? _questService;

    public string Id => "quest";
    public string Name => "Quest System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        // Initialize systems
        _questSystem = new QuestSystem(_world);
        _questProgressSystem = new QuestProgressSystem(_world);
        _questRewardSystem = new QuestRewardSystem(_world);

        // Initialize service
        _questService = new QuestService(_world, _questSystem, _questProgressSystem, _questRewardSystem);

        // Register service in plugin context
        context.Registry.Register<QuestService>(_questService, priority: 100);

        // Subscribe to game events
        SubscribeToEvents(context);

        context.Logger.LogInformation("Quest plugin initialized");
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

    private void SubscribeToEvents(IPluginContext context)
    {
        // Subscribe to game events for quest progress tracking
        // Register systems for external access
        if (_questProgressSystem != null)
            context.Registry.Register<QuestProgressSystem>(_questProgressSystem, priority: 100);
        if (_questRewardSystem != null)
            context.Registry.Register<QuestRewardSystem>(_questRewardSystem, priority: 100);
    }
}
