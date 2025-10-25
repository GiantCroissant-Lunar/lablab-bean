using Arch.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using LablabBean.Plugins.NPC.Agents;
using LablabBean.Plugins.NPC.Factories;

namespace LablabBean.Plugins.NPC;

/// <summary>
/// NPC plugin - provides NPC interaction and dialogue system with optional LLM dialogue generation
/// </summary>
public class NPCPlugin : IPlugin
{
    private World? _world;
    private NPCSystem? _npcSystem;
    private DialogueSystem? _dialogueSystem;
    private NPCService? _npcService;
    private DialogueGeneratorAgent? _dialogueGenerator;
    private NPCFactory? _npcFactory;

    public string Id => "npc";
    public string Name => "NPC System";
    public string Version => "1.1.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        // Gracefully handle missing World service in host
        if (!context.Registry.IsRegistered<World>())
        {
            context.Logger.LogWarning("World service not found; NPC plugin will initialize in passive mode.");
            return Task.CompletedTask;
        }

        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        // Try to get IChatClient for LLM dialogue generation
        var chatClient = context.Registry.Get<IChatClient>();
        if (chatClient != null)
        {
            _dialogueGenerator = new DialogueGeneratorAgent(chatClient);
            context.Logger.LogInformation("NPC plugin: LLM dialogue generation enabled");
        }
        else
        {
            context.Logger.LogInformation("NPC plugin: LLM dialogue generation disabled (no IChatClient available)");
        }

        // Initialize NPC factory with optional LLM generator
        _npcFactory = new NPCFactory(_world, _dialogueGenerator);

        // Initialize systems
        _npcSystem = new NPCSystem(_world);
        _dialogueSystem = new DialogueSystem(_world);

        // Initialize service
        _npcService = new NPCService(_world, _npcSystem, _dialogueSystem);

        // Register services in plugin context
        context.Registry.Register<NPCService>(_npcService, priority: 100);
        context.Registry.Register<NPCFactory>(_npcFactory, priority: 100);

        if (_dialogueGenerator != null)
            context.Registry.Register<DialogueGeneratorAgent>(_dialogueGenerator, priority: 100);

        context.Logger.LogInformation("NPC plugin initialized (LLM: {HasLLM})", _dialogueGenerator != null);
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
