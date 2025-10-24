using Arch.Core;
using Microsoft.Extensions.Logging;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;

namespace LablabBean.Plugins.NPC;

/// <summary>
/// NPC plugin - provides NPC interaction and dialogue system
/// </summary>
public class NPCPlugin : IPlugin
{
    private World? _world;
    private NPCSystem? _npcSystem;
    private DialogueSystem? _dialogueSystem;
    private NPCService? _npcService;

    public string Id => "npc";
    public string Name => "NPC System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        // Initialize systems
        _npcSystem = new NPCSystem(_world);
        _dialogueSystem = new DialogueSystem(_world);

        // Initialize service
        _npcService = new NPCService(_world, _npcSystem, _dialogueSystem);

        // Register service in plugin context
        context.Registry.Register<NPCService>(_npcService, priority: 100);

        context.Logger.LogInformation("NPC plugin initialized");
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
