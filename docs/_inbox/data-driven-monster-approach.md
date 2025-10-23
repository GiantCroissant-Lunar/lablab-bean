---
doc_id: DOC-2025-00034
title: Data-Driven Monster System (Better Alternative to Code Generation)
doc_type: proposal
status: draft
canonical: false
created: 2025-10-21
tags: [architecture, data-driven, monsters, ecs, design-patterns]
summary: >
  Proposal for data-driven monster system using JSON/YAML instead of
  generated C# classes. More flexible, maintainable, and modern.
---

# Data-Driven Monster System

**Alternative to Code Generation Approach**

## Problem with Current Approach

Generating a C# class for each monster type is:

- ❌ Old-style OOP inheritance pattern
- ❌ Requires recompilation for each change
- ❌ Hard to balance (code changes vs data changes)
- ❌ Not designer-friendly
- ❌ No hot-reload support
- ❌ Difficult to mod

## Proposed Data-Driven Approach

### Architecture

```
┌─────────────────┐
│ Monster Data    │  ← YAML/JSON files
│ (data/monsters/)│
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│ MonsterDatabase │  ← Load and parse at runtime
│ (singleton)     │
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│ Monster Factory │  ← Create instances from data
│                 │
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│ Monster Entity  │  ← Generic entity + components
│ + Components    │
└─────────────────┘
```

### Data Format

**File:** `data/monsters/dragon.yaml`

```yaml
id: ancient_dragon
display_name: "Ancient Dragon"
description: "A fearsome ancient dragon with devastating fire attacks"
glyph: "D"
color: red

# Base stats
stats:
  health: 200
  attack: 30
  defense: 20
  speed: 6

# AI behavior
ai:
  type: aggressive
  aggro_range: 15
  patrol_behavior: none

# Components (behavior composition)
components:
  - type: FireBreathing
    damage: 50
    cooldown: 3
  - type: Flying
    altitude: 2
  - type: TreasureHoarding
    gold_multiplier: 3

# Loot tables
loot:
  experience: 1000
  gold:
    min: 100
    max: 300
  drops:
    - item: dragon_scale
      chance: 0.5
      quantity: [1, 3]
    - item: fire_gem
      chance: 0.2
      quantity: 1

# Special abilities
abilities:
  - id: fire_breath
    name: "Fire Breath"
    damage: 75
    range: 5
    cooldown: 5
  - id: tail_sweep
    name: "Tail Sweep"
    damage: 40
    range: 2
    cooldown: 3
```

### C# Implementation

**Single Monster Class:**

```csharp
// Monster.cs
using System.Collections.Generic;
using LablabBean.Game.Core.Components;

namespace LablabBean.Game.Core.Entities
{
    public class Monster : Entity
    {
        // Data reference
        public MonsterDefinition Definition { get; private set; }

        // Component system
        public List<IMonsterComponent> Components { get; }

        // Runtime state
        public int CurrentHealth { get; set; }
        public Vector2 Position { get; set; }
        public MonsterState State { get; set; }

        public Monster(MonsterDefinition definition)
        {
            Definition = definition;
            Components = new List<IMonsterComponent>();
            CurrentHealth = definition.Stats.Health;

            // Initialize components from definition
            foreach (var componentDef in definition.Components)
            {
                var component = ComponentFactory.Create(componentDef);
                Components.Add(component);
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update all components
            foreach (var component in Components)
            {
                component.Update(this, gameTime);
            }

            // Execute AI
            Definition.Ai.Execute(this, gameTime);
        }
    }
}
```

**Monster Database:**

```csharp
// MonsterDatabase.cs
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace LablabBean.Game.Core.Data
{
    public class MonsterDatabase
    {
        private static MonsterDatabase _instance;
        public static MonsterDatabase Instance => _instance ??= new MonsterDatabase();

        private Dictionary<string, MonsterDefinition> _monsters;

        private MonsterDatabase()
        {
            _monsters = new Dictionary<string, MonsterDefinition>();
            LoadMonsters();
        }

        private void LoadMonsters()
        {
            var dataPath = "data/monsters/";
            var yamlFiles = Directory.GetFiles(dataPath, "*.yaml");

            var deserializer = new DeserializerBuilder().Build();

            foreach (var file in yamlFiles)
            {
                var yaml = File.ReadAllText(file);
                var definition = deserializer.Deserialize<MonsterDefinition>(yaml);
                _monsters[definition.Id] = definition;
            }
        }

        public MonsterDefinition Get(string monsterId)
        {
            if (_monsters.TryGetValue(monsterId, out var definition))
                return definition;

            throw new KeyNotFoundException($"Monster '{monsterId}' not found");
        }

        public IEnumerable<MonsterDefinition> GetAll() => _monsters.Values;

        // Hot reload support
        public void Reload()
        {
            _monsters.Clear();
            LoadMonsters();
        }
    }
}
```

**Monster Factory:**

```csharp
// MonsterFactory.cs
namespace LablabBean.Game.Core.Factories
{
    public static class MonsterFactory
    {
        public static Monster Create(string monsterId)
        {
            var definition = MonsterDatabase.Instance.Get(monsterId);
            return new Monster(definition);
        }

        public static Monster CreateAt(string monsterId, Vector2 position)
        {
            var monster = Create(monsterId);
            monster.Position = position;
            return monster;
        }
    }
}
```

**Usage in Game:**

```csharp
// DungeonGenerator.cs
public void SpawnMonsters()
{
    // Spawn a dragon
    var dragon = MonsterFactory.CreateAt("ancient_dragon", new Vector2(10, 10));
    _monsters.Add(dragon);

    // Spawn multiple goblins
    for (int i = 0; i < 5; i++)
    {
        var goblin = MonsterFactory.CreateAt("goblin", GetRandomPosition());
        _monsters.Add(goblin);
    }
}

// Game loop
public void Update(GameTime gameTime)
{
    foreach (var monster in _monsters)
    {
        monster.Update(gameTime);
    }
}
```

## Benefits

### ✅ Designer-Friendly

- Edit YAML files directly
- No C# knowledge needed
- Visual tools can edit YAML

### ✅ Hot-Reload

```csharp
// In development mode
if (DevMode && FileWatcher.Detected("data/monsters/"))
{
    MonsterDatabase.Instance.Reload();
    _logger.LogInformation("Monster data reloaded");
}
```

### ✅ Easy Balancing

- Change health: edit YAML, reload
- No recompilation
- Quick iteration

### ✅ Modding Support

```
game/data/monsters/      ← Base game monsters
mods/cool-mod/monsters/  ← Mod monsters
```

### ✅ Version Control Friendly

```diff
# Clear diff of balance changes
- health: 100
+ health: 120
```

### ✅ Component Composition

```yaml
# Mix and match behaviors
components:
  - FireBreathing
  - Flying
  - Regeneration
```

## Migration Path

### Phase 1: Create Data Schema

1. Define `MonsterDefinition` class
2. Create YAML schema
3. Add validation

### Phase 2: Convert Existing Monsters

```bash
# Use Spec-Kit to convert C# → YAML!
task speck-generate TEMPLATE=migration/cs-to-yaml \
     INPUT=Dragon.cs \
     OUTPUT=data/monsters/dragon.yaml
```

### Phase 3: Implement Data Loading

1. Add YamlDotNet package
2. Implement MonsterDatabase
3. Implement MonsterFactory

### Phase 4: Refactor Game Logic

1. Replace `new Dragon()` with `MonsterFactory.Create("dragon")`
2. Remove old C# monster classes
3. Test

### Phase 5: Add Hot-Reload (Optional)

1. Add FileSystemWatcher
2. Implement reload trigger
3. Update UI to show "Data Reloaded"

## Spec-Kit's New Role

Instead of generating **C# code**, use Spec-Kit to generate **data files**:

### Template: `templates/data/monster.tmpl`

```yaml
id: {{lower Name}}
display_name: "{{DisplayName}}"
description: "{{Description}}"
glyph: "{{Glyph}}"

stats:
  health: {{MaxHealth}}
  attack: {{Attack}}
  defense: {{Defense}}
  speed: {{Speed}}

ai:
  type: {{lower AiType}}
  aggro_range: {{AggroRange}}

loot:
  experience: {{ExperienceValue}}
  gold:
    min: {{GoldDropMin}}
    max: {{GoldDropMax}}
```

### Usage

```bash
# Generate YAML data (not C# code!)
task speck-generate \
     TEMPLATE=data/monster \
     OUTPUT=data/monsters/dragon.yaml \
     VARS=vars/dragon.yaml
```

### Output: `data/monsters/dragon.yaml` ✅

Now Spec-Kit generates **data**, not code!

## Dependencies

```xml
<!-- LablabBean.Game.Core.csproj -->
<ItemGroup>
  <PackageReference Include="YamlDotNet" Version="13.7.1" />
</ItemGroup>
```

## Example Full Implementation

See:

- `data/monsters/` - Monster data files
- `MonsterDefinition.cs` - Data schema
- `MonsterDatabase.cs` - Data loader
- `MonsterFactory.cs` - Instance creator
- `Monster.cs` - Generic entity

## Performance

**Loading Time:**

- Parse all YAML: ~50ms for 100 monsters
- Lazy loading: <1ms per monster
- In-memory cache: No runtime overhead

**Memory:**

- Shared definitions: 1 instance per monster type
- Runtime entities: Only spawned monsters

## Comparison

| Approach | Compilation | Hot-Reload | Designer-Friendly | Modding | Flexibility |
|----------|-------------|------------|-------------------|---------|-------------|
| **Generated C# Classes** | Required | ❌ No | ❌ No | ❌ Hard | ❌ Low |
| **Data-Driven YAML** | Not Required | ✅ Yes | ✅ Yes | ✅ Easy | ✅ High |

## Conclusion

**Recommendation:** Adopt data-driven approach for monsters.

**Use Spec-Kit for:** Generating YAML data files, documentation, and configs.

**Don't use Spec-Kit for:** Generating C# monster classes.

---

**Status:** Proposed
**Next Step:** Implement MonsterDefinition schema and database loader
