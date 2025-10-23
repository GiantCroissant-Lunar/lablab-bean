using Arch.Core;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Spell.Data.Spells;
using LablabBean.Plugins.Spell.Services;
using LablabBean.Plugins.Spell.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spell;

/// <summary>
/// Plugin for spell casting and magic system.
/// </summary>
public class SpellPlugin : IPlugin
{
    private World? _world;
    private SpellCastingSystem? _castingSystem;
    private ManaRegenerationSystem? _manaRegenSystem;
    private SpellEffectSystem? _effectSystem;
    private SpellService? _spellService;

    public string Id => "spell";
    public string Name => "Spell System";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        var logger = context.Logger;
        logger.LogInformation("Initializing {PluginName} plugin v{Version}", Name, Version);

        _castingSystem = new SpellCastingSystem(_world, logger);
        _manaRegenSystem = new ManaRegenerationSystem(_world, logger);
        _effectSystem = new SpellEffectSystem(_world, logger);
        _spellService = new SpellService(logger, _castingSystem);

        context.Registry.Register<SpellService>(_spellService, priority: 100);
        context.Registry.Register<SpellCastingSystem>(_castingSystem, priority: 100);
        context.Registry.Register<ManaRegenerationSystem>(_manaRegenSystem, priority: 100);
        context.Registry.Register<SpellEffectSystem>(_effectSystem, priority: 100);

        RegisterSampleSpells();

        logger.LogInformation("{PluginName} plugin initialized successfully", Name);
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

    private void RegisterSampleSpells()
    {
        if (_spellService == null) return;

        foreach (var spell in OffensiveSpells.GetAll())
        {
            _spellService.RegisterSpell(spell);
        }

        foreach (var spell in HealingSpells.GetAll())
        {
            _spellService.RegisterSpell(spell);
        }

        foreach (var spell in BuffSpells.GetAll())
        {
            _spellService.RegisterSpell(spell);
        }
    }
}
