using Arch.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Quest.Services;
using LablabBean.Plugins.Quest.Systems;
using LablabBean.Plugins.Quest.Agents;
using LablabBean.Plugins.Quest.Factories;
using LablabBean.Plugins.Progression.Services;

namespace LablabBean.Plugins.Quest;

/// <summary>
/// Quest plugin - provides quest-driven exploration system with optional LLM quest generation
/// </summary>
public class QuestPlugin : IPlugin
{
    private World? _world;
    private QuestSystem? _questSystem;
    private QuestProgressSystem? _questProgressSystem;
    private QuestRewardSystem? _questRewardSystem;
    private QuestService? _questService;
    private QuestGeneratorAgent? _questGenerator;
    private QuestFactory? _questFactory;

    public string Id => "quest";
    public string Name => "Quest System";
    public string Version => "1.1.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        if (!context.Registry.IsRegistered<World>())
        {
            context.Logger.LogWarning("Quest plugin: World service not found; initializing in passive mode.");
            return Task.CompletedTask;
        }

        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        // Try to get IChatClient for LLM quest generation
        var chatClient = context.Registry.Get<IChatClient>();
        if (chatClient != null)
        {
            _questGenerator = new QuestGeneratorAgent(chatClient);
            context.Logger.LogInformation("Quest plugin: LLM quest generation enabled");
        }
        else
        {
            context.Logger.LogInformation("Quest plugin: LLM quest generation disabled (no IChatClient available)");
        }

        // Initialize quest factory with optional LLM generator
        _questFactory = new QuestFactory(_world, _questGenerator);

        // Initialize systems
        _questSystem = new QuestSystem(_world);
        _questProgressSystem = new QuestProgressSystem(_world);
        _questRewardSystem = new QuestRewardSystem(_world);

        // Try to get ProgressionService and inject it
        var progressionService = context.Registry.Get<ProgressionService>();
        if (progressionService != null && _questRewardSystem != null)
        {
            _questRewardSystem.SetProgressionService(progressionService);
            context.Logger.LogInformation("Quest plugin: ProgressionService integrated for XP rewards");
        }
        else
        {
            context.Logger.LogWarning("Quest plugin: ProgressionService not found - XP rewards will not be granted");
        }

        // Initialize service
        _questService = new QuestService(_world, _questSystem, _questProgressSystem, _questRewardSystem!);

        // Register services in plugin context
        context.Registry.Register<QuestService>(_questService, priority: 100);
        context.Registry.Register<QuestFactory>(_questFactory, priority: 100);

        if (_questGenerator != null)
            context.Registry.Register<QuestGeneratorAgent>(_questGenerator, priority: 100);

        // Subscribe to game events
        SubscribeToEvents(context);

        context.Logger.LogInformation("Quest plugin initialized (LLM: {HasLLM})", _questGenerator != null);
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
