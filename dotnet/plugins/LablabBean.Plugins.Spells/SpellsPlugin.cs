using Arch.Core;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Spells.Services;
using LablabBean.Plugins.Spells.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spells;

/// <summary>
/// Spells & Abilities plugin - provides magic combat system with mana management.
/// </summary>
public class SpellsPlugin : IPlugin
{
    private IPluginContext? _context;
    private World? _world;
    private SpellDatabase? _spellDatabase;
    private ManaSystem? _manaSystem;
    private SpellCooldownSystem? _cooldownSystem;
    private SpellEffectSystem? _effectSystem;
    private SpellCastingSystem? _castingSystem;
    private SpellService? _spellService;

    public string Id => "spells";
    public string Name => "Spells System";
    public string Version => "1.0.0";

    public async Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _context = context;
        _context.Logger.LogInformation("Initializing Spells plugin...");

        if (!context.Registry.IsRegistered<World>())
        {
            _context.Logger.LogWarning("Spells plugin: World service not found; initializing in passive mode.");
            return;
        }

        var worldService = context.Registry.Get<World>();
        _world = worldService ?? throw new InvalidOperationException("World service not found");

        _spellDatabase = new SpellDatabase(_context.Logger);

        var pluginDir = Path.GetDirectoryName(typeof(SpellsPlugin).Assembly.Location);
        var spellsDir = Path.Combine(pluginDir ?? ".", "Data", "Spells");
        var unlocksFile = Path.Combine(pluginDir ?? ".", "Data", "SpellUnlocks.json");

        await _spellDatabase.LoadSpellsAsync(spellsDir);
        await _spellDatabase.LoadSpellUnlocksAsync(unlocksFile);

        _manaSystem = new ManaSystem(_world, _context.Logger);
        _cooldownSystem = new SpellCooldownSystem(_world, _context.Logger);
        _effectSystem = new SpellEffectSystem(_world, _context.Logger);
        _castingSystem = new SpellCastingSystem(
            _world,
            _context.Logger,
            _manaSystem,
            _cooldownSystem,
            _effectSystem,
            _spellDatabase.Spells);

        _spellService = new SpellService(
            _world,
            _context.Logger,
            _spellDatabase.Spells,
            _manaSystem,
            _cooldownSystem,
            _castingSystem);

        context.Registry.Register(_spellService);
        context.Registry.Register(_spellDatabase);

        _context.Logger.LogInformation(
            "Spells plugin initialized with {SpellCount} spells",
            _spellDatabase.Spells.Count);
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Spells plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _context?.Logger.LogInformation("Spells plugin stopped");
        return Task.CompletedTask;
    }

    public void UpdateCooldowns()
    {
        _cooldownSystem?.UpdateCooldowns();
    }

    public void RegenerateMana(bool inCombat)
    {
        _manaSystem?.RegenerateMana(inCombat);
    }
}
