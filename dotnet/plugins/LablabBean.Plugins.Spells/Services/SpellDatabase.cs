using System.Text.Json;
using LablabBean.Plugins.Spells.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Spells.Services;

/// <summary>
/// Loads spell definitions from JSON files.
/// </summary>
public class SpellDatabase
{
    private readonly Dictionary<Guid, Spell> _spells = new();
    private readonly Dictionary<int, List<Guid>> _spellUnlocks = new();
    private readonly ILogger _logger;

    public SpellDatabase(ILogger logger)
    {
        _logger = logger;
    }

    public Dictionary<Guid, Spell> Spells => _spells;
    public Dictionary<int, List<Guid>> SpellUnlocks => _spellUnlocks;

    public async Task LoadSpellsAsync(string spellsDirectory)
    {
        if (!Directory.Exists(spellsDirectory))
        {
            _logger.LogWarning("Spells directory not found: {Directory}", spellsDirectory);
            LoadDefaultSpells();
            return;
        }

        var jsonFiles = Directory.GetFiles(spellsDirectory, "*.json");
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var spell = JsonSerializer.Deserialize<Spell>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (spell != null)
                {
                    _spells[spell.Id] = spell;
                    _logger.LogDebug("Loaded spell: {SpellName}", spell.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load spell from {File}", file);
            }
        }

        _logger.LogInformation("Loaded {Count} spells", _spells.Count);
    }

    public async Task LoadSpellUnlocksAsync(string unlockFile)
    {
        if (!File.Exists(unlockFile))
        {
            _logger.LogWarning("Spell unlocks file not found: {File}", unlockFile);
            LoadDefaultUnlocks();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(unlockFile);
            var data = JsonSerializer.Deserialize<SpellUnlockData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.SpellUnlocks != null)
            {
                foreach (var kvp in data.SpellUnlocks)
                {
                    if (int.TryParse(kvp.Key, out var level))
                    {
                        _spellUnlocks[level] = kvp.Value;
                    }
                }
            }

            _logger.LogInformation("Loaded spell unlocks for {Count} levels", _spellUnlocks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load spell unlocks from {File}", unlockFile);
            LoadDefaultUnlocks();
        }
    }

    public List<Guid> GetSpellsForLevel(int level)
    {
        return _spellUnlocks.TryGetValue(level, out var spells) ? spells : new List<Guid>();
    }

    private void LoadDefaultSpells()
    {
        _spells.Add(
            Guid.Parse("d5e7f9a2-8b1c-7d3f-2e4b-6f8a1c3e5d7b"),
            new Spell
            {
                Id = Guid.Parse("d5e7f9a2-8b1c-7d3f-2e4b-6f8a1c3e5d7b"),
                Name = "Magic Missile",
                Description = "A simple offensive spell that never misses.",
                Type = SpellType.Offensive,
                Targeting = TargetingType.Single,
                ManaCost = 8,
                Cooldown = 0,
                Range = 4,
                MinLevel = 1,
                Effects = new List<SpellEffect>
                {
                    new() { Type = SpellEffectType.Damage, Value = 15 }
                }
            });

        _spells.Add(
            Guid.Parse("f7a9d8c5-3b12-4e8f-9c7a-1d5e6f8a9b2c"),
            new Spell
            {
                Id = Guid.Parse("f7a9d8c5-3b12-4e8f-9c7a-1d5e6f8a9b2c"),
                Name = "Fireball",
                Description = "Hurls a ball of fire at the target.",
                Type = SpellType.Offensive,
                Targeting = TargetingType.Single,
                ManaCost = 15,
                Cooldown = 2,
                Range = 5,
                MinLevel = 3,
                Effects = new List<SpellEffect>
                {
                    new() { Type = SpellEffectType.Damage, Value = 25 }
                }
            });

        _logger.LogInformation("Loaded {Count} default spells", _spells.Count);
    }

    private void LoadDefaultUnlocks()
    {
        _spellUnlocks[1] = new List<Guid> { Guid.Parse("d5e7f9a2-8b1c-7d3f-2e4b-6f8a1c3e5d7b") };
        _spellUnlocks[3] = new List<Guid> { Guid.Parse("f7a9d8c5-3b12-4e8f-9c7a-1d5e6f8a9b2c") };

        _logger.LogInformation("Loaded default spell unlocks");
    }

    private class SpellUnlockData
    {
        public Dictionary<string, List<Guid>> SpellUnlocks { get; set; } = new();
    }
}
