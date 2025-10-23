---
doc_id: DOC-2025-00035
title: Monster Template Example
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [templates, monsters, entities, code-generation]
summary: >
  Example guide for using monster template to generate new monster types
  with spec-kit.
---

# Monster Template Example

This example shows how to use the monster template to generate new monster types.

## Template Variables

The `templates/entity/monster.tmpl` template accepts these variables:

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| Name | string | C# class name | `Dragon` |
| DisplayName | string | In-game display name | `Red Dragon` |
| Glyph | char | ASCII character | `D` |
| Description | string | Monster description | `A fearsome red dragon` |
| MaxHealth | int | Maximum health points | `150` |
| Attack | int | Attack power | `25` |
| Defense | int | Defense rating | `15` |
| Speed | int | Movement speed | `5` |
| AiType | enum | AI behavior type | `Aggressive`, `Passive`, `Patrol` |
| AggroRange | int | Detection range in tiles | `10` |
| ExperienceValue | int | XP awarded on defeat | `500` |
| GoldDropMin | int | Minimum gold dropped | `50` |
| GoldDropMax | int | Maximum gold dropped | `150` |
| Author | string | Template author | `Your Name` |
| Timestamp | string | Generation timestamp | `2025-10-20 23:00:00` |

## Usage Example

### Using spec-kit (when fully implemented)

```bash
# Generate a Dragon monster
task speck-generate \
  TEMPLATE=entity/monster \
  OUTPUT=Dragon.cs \
  NAME=Dragon \
  DISPLAY_NAME="Red Dragon" \
  GLYPH=D \
  MAX_HEALTH=150 \
  ATTACK=25 \
  DEFENSE=15
```

### Manual Variable Replacement (temporary)

For now, manually copy the template and replace variables:

1. Copy `templates/entity/monster.tmpl`
2. Replace all `{{.VariableName}}` with actual values
3. Save to appropriate location

Example:

```csharp
// Before (template)
public class {{.Name}} : Monster
{
    Name = "{{.DisplayName}}";
    MaxHealth = {{.MaxHealth}};
}

// After (generated)
public class Dragon : Monster
{
    Name = "Red Dragon";
    MaxHealth = 150;
}
```

## Pre-defined Monster Examples

### Dragon

```yaml
Name: Dragon
DisplayName: "Red Dragon"
Glyph: 'D'
Description: "A fearsome red dragon with scales like molten steel"
MaxHealth: 150
Attack: 25
Defense: 15
Speed: 5
AiType: Aggressive
AggroRange: 12
ExperienceValue: 500
GoldDropMin: 50
GoldDropMax: 150
```

### Wraith

```yaml
Name: Wraith
DisplayName: "Shadow Wraith"
Glyph: 'W'
Description: "An ethereal undead creature that phases through walls"
MaxHealth: 60
Attack: 18
Defense: 5
Speed: 8
AiType: Patrol
AggroRange: 8
ExperienceValue: 200
GoldDropMin: 20
GoldDropMax: 60
```

### Slime

```yaml
Name: Slime
DisplayName: "Green Slime"
Glyph: 's'
Description: "A gelatinous blob that splits when damaged"
MaxHealth: 30
Attack: 5
Defense: 2
Speed: 3
AiType: Passive
AggroRange: 3
ExperienceValue: 50
GoldDropMin: 5
GoldDropMax: 15
```

### Mimic

```yaml
Name: Mimic
DisplayName: "Chest Mimic"
Glyph: 'M'
Description: "A shapeshifting creature disguised as treasure"
MaxHealth: 80
Attack: 20
Defense: 12
Speed: 4
AiType: Ambush
AggroRange: 1
ExperienceValue: 300
GoldDropMin: 100
GoldDropMax: 200
```

## Directory Structure

Generated monsters should be placed in:

```
dotnet/framework/LablabBean.Game.Core/
└── Monsters/
    ├── Dragon.cs
    ├── Wraith.cs
    ├── Slime.cs
    └── Mimic.cs
```

## Integration Steps

After generating a monster class:

1. **Add to project**:

   ```bash
   # Monster class is automatically included in .csproj
   ```

2. **Register in MonsterFactory** (if exists):

   ```csharp
   MonsterFactory.Register("dragon", typeof(Dragon));
   ```

3. **Add to spawn tables**:

   ```csharp
   // In GameStateManager or similar
   var spawnTable = new Dictionary<string, int>
   {
       { "dragon", 5 },  // 5% spawn chance
       // ... other monsters
   };
   ```

4. **Test in game**:

   ```bash
   task dotnet-run-console
   ```

## Customization Points

The template includes TODO comments for customization:

### OnSpawn()

```csharp
public override void OnSpawn()
{
    base.OnSpawn();
    // TODO: Add custom spawn behavior
    // Examples:
    // - Spawn effect particles
    // - Play sound
    // - Modify nearby tiles
}
```

### OnDeath()

```csharp
public override void OnDeath()
{
    base.OnDeath();
    // TODO: Add custom death behavior
    // Examples:
    // - Drop special items
    // - Trigger events
    // - Spawn smaller enemies
}
```

### CalculateAttackDamage()

```csharp
public override int CalculateAttackDamage()
{
    var baseDamage = base.CalculateAttackDamage();
    // TODO: Add special attack modifiers
    // Examples:
    // - Critical hits
    // - Elemental damage
    // - Status effects
    return baseDamage;
}
```

## Future Improvements

When spec-kit is fully integrated:

1. **YAML-based Monster Definitions**:

   ```yaml
   # monsters/dragon.yaml
   monster:
     name: Dragon
     display_name: "Red Dragon"
     stats:
       health: 150
       attack: 25
     # ... other properties
   ```

2. **Batch Generation**:

   ```bash
   task gen-monsters-from-yaml FILE=monsters/*.yaml
   ```

3. **Validation**:

   ```bash
   task validate-monster-stats
   ```

4. **Documentation Generation**:

   ```bash
   task gen-monster-bestiary
   # Generates: docs/bestiary.md with all monsters
   ```

---

**See Also**:

- `docs/SPEC-KIT-UTILIZATION.md` - Full spec-kit guide
- `templates/entity/monster.tmpl` - Monster template
- `docs/specs/dungeon-generation-system.md` - Example specification
