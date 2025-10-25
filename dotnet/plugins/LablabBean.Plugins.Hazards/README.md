# Environmental Hazards System

**Plugin**: `LablabBean.Plugins.Hazards`
**Version**: 1.0.0
**Phase**: 9 of 10

## 🎯 Overview

Complete environmental hazards system featuring traps, environmental damage (lava, acid, poison gas), detection mechanics, and ongoing effects. Adds danger and strategic depth to dungeon exploration.

## 🏗️ Architecture

### Components

1. **Hazard** - Core hazard component
   - Type, damage, activation chance
   - Visibility and detection requirements
   - State management

2. **HazardEffect** - Ongoing damage effects
   - Duration-based effects (burning, poisoned)
   - Damage per turn
   - Effect stacking

3. **HazardTrigger** - Trigger configuration
   - OnEnter, OnExit, Periodic, Proximity, Manual
   - Retrigger settings
   - Cooldown management

4. **HazardResistance** - Damage mitigation
   - Type-specific resistances
   - Percentage-based reduction

### Systems

1. **HazardSystem** - Core hazard processing
   - Activation checks
   - Damage application
   - Effect management
   - Periodic and proximity triggers

2. **HazardDetectionSystem** - Detection/disarming
   - Skill-based detection rolls
   - Disarming mechanics
   - Critical failure handling

### Services

1. **HazardService** - High-level API
   - Hazard creation and management
   - Entity movement integration
   - Area spawning

2. **HazardFactory** - Entity creation
   - Convenient factory methods
   - Pre-configured hazards

### Data

1. **HazardDatabase** - Predefined hazards
   - 10 hazard definitions
   - Balanced stats
   - Visual glyphs

## 🎮 Hazard Types

### Traps (Hidden)

1. **Spike Trap** `^`
   - Damage: 10
   - Activation: 80%
   - Detection: DC 12
   - One-time trigger

2. **Bear Trap** `v`
   - Damage: 15
   - Activation: 100%
   - Detection: DC 15
   - One-time trigger

3. **Arrow Trap** `→`
   - Damage: 12
   - Range: 1 tile
   - Detection: DC 14
   - Retriggers after 3 turns

4. **Falling Rocks** `▼`
   - Damage: 25
   - Activation: 60%
   - Detection: DC 16
   - One-time trigger

5. **Pitfall** `O`
   - Damage: 20
   - Activation: 100%
   - Detection: DC 13
   - One-time trigger

### Environmental (Visible)

6. **Lava** `≈`
   - Damage: 20
   - Burning: 5 dmg/turn for 3 turns
   - Always visible

7. **Acid Pool** `~`
   - Damage: 15
   - Corroding: 3 dmg/turn for 5 turns
   - Always visible

8. **Fire** `♦`
   - Damage: 8
   - Burning: 2 dmg/turn for 5 turns
   - Always visible

9. **Poison Gas** `☁`
   - Damage: 5
   - Poisoned: 1 dmg/turn for 10 turns
   - Periodic: Every 2 turns

10. **Electric Floor** `▓`
    - Damage: 18
    - Periodic: Every 3 turns
    - Always visible

## 📖 Usage Examples

### Basic Hazard Creation

```csharp
// Using service
var hazardService = serviceProvider.GetService<HazardService>();
var spiketrap = hazardService.CreateHazard("spike_trap", 10, 15);

// Using factory
var factory = new HazardFactory(world);
var lava = factory.CreateLava(20, 25);
var bearTrap = factory.CreateBearTrap(15, 10, hidden: true);
```

### Integration with Movement

```csharp
// In your movement handler
public void OnPlayerMove(Entity player, int newX, int newY)
{
    // Check for hazards
    hazardService.OnEntityMove(player, newX, newY);

    // This automatically:
    // - Triggers OnEnter hazards
    // - Checks proximity triggers
    // - Applies damage and effects
}
```

### Detection and Disarming

```csharp
// Detect hidden traps
var detected = hazardService.DetectHazards(
    playerX,
    playerY,
    range: 3,
    skill: playerPerception
);

foreach (var hazard in detected)
{
    var info = hazardService.GetHazardInfo(hazard);
    MessageLog.Add($"You detected: {info}");
}

// Attempt to disarm
var success = hazardService.DisarmHazard(trapEntity, playerDisarmSkill);
if (success)
{
    MessageLog.Add("Trap disarmed!");
    GainXP(50);
}
```

### Adding Resistance

```csharp
// Fire resistance potion
hazardService.AddResistance(player, HazardType.Fire, 0.5f); // 50% reduction

// Acid-proof armor
hazardService.AddResistance(player, HazardType.AcidPool, 0.75f); // 75% reduction

// Immunity item
hazardService.AddResistance(player, HazardType.Lava, 1.0f); // 100% immune
```

### Game Turn Processing

```csharp
public void ProcessGameTurn()
{
    // Process player action
    HandlePlayerInput();

    // Update hazards
    hazardService.UpdateHazards();
    // - Processes periodic hazards
    // - Applies ongoing effects
    // - Updates cooldowns

    // Process enemies
    ProcessAI();
}
```

### Spawning Hazards in Rooms

```csharp
// Random hazards in a room
hazardService.SpawnHazardsInArea(
    room.X,
    room.Y,
    room.Width,
    room.Height,
    hazardCount: 3
);

// Lava corridor
for (int x = corridor.StartX; x <= corridor.EndX; x++)
{
    factory.CreateLava(x, corridor.Y);
}

// Trapped treasure room
factory.CreateBearTrap(treasureX - 1, treasureY, hidden: true);
factory.CreateArrowTrap(treasureX, treasureY - 1, hidden: true);
```

### Custom Hazard Creation

```csharp
// Create custom hazard
var customHazard = new Hazard(
    type: HazardType.SpikeTrap,
    damage: 30,
    activationChance: 1.0f,
    isVisible: false,
    requiresDetection: true,
    detectionDifficulty: 20
);

var trigger = new HazardTrigger(TriggerType.OnEnter, canRetrigger: false);

var entity = world.Create(customHazard, trigger);
entity.Add(new Transform { X = x, Y = y });
```

## 🔗 Integration Examples

### With Combat System

```csharp
// Enemy knockback into lava
public void KnockbackEnemy(Entity enemy, Direction direction)
{
    var newPos = CalculateKnockbackPosition(enemy, direction);
    MoveEntity(enemy, newPos.x, newPos.y);

    // Hazards automatically trigger
    hazardService.OnEntityMove(enemy, newPos.x, newPos.y);
}
```

### With Quest System

```csharp
// Quest: Disarm 5 traps
questService.OnHazardDisarmed += (entity, hazard) =>
{
    var quest = questService.GetActiveQuest("disarm_traps");
    if (quest != null)
    {
        quest.Progress++;
        if (quest.Progress >= 5)
        {
            questService.CompleteQuest("disarm_traps");
        }
    }
};
```

### With Spell System

```csharp
// Fireball creates fire hazards
public void CastFireball(int targetX, int targetY)
{
    // Deal damage...

    // Create fire hazards in explosion radius
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            var fx = targetX + dx;
            var fy = targetY + dy;

            if (Random.Shared.NextDouble() < 0.5)
            {
                factory.CreateFire(fx, fy);
            }
        }
    }
}
```

### With Merchant System

```csharp
// Buy trap detection items
merchantService.AddItem("trap_detector", new MerchantItem
{
    Name = "Trap Detector",
    Price = 200,
    OnUse = (player) =>
    {
        var detected = hazardService.DetectHazards(
            playerX, playerY, range: 10, skill: 20
        );
        MessageLog.Add($"Detected {detected.Count} hazards!");
    }
});

// Buy hazard resistance potions
merchantService.AddItem("fire_resist_potion", new MerchantItem
{
    Name = "Fire Resistance Potion",
    Price = 150,
    OnUse = (player) =>
    {
        hazardService.AddResistance(player, HazardType.Fire, 0.75f);
        AddTimedEffect(player, "Fire Resist", duration: 100);
    }
});
```

## 🎨 Visual Representation

### Rendering Hazards

```csharp
public char GetHazardGlyph(Entity hazardEntity)
{
    if (!hazardEntity.Has<Hazard>())
        return ' ';

    var hazard = hazardEntity.Get<Hazard>();

    // Hidden traps don't show unless detected
    if (hazard.RequiresDetection && !hazard.IsVisible)
        return '.'; // Normal floor

    return hazard.Type switch
    {
        HazardType.SpikeTrap => '^',
        HazardType.BearTrap => 'v',
        HazardType.ArrowTrap => '→',
        HazardType.Lava => '≈',
        HazardType.PoisonGas => '☁',
        HazardType.AcidPool => '~',
        HazardType.ElectricFloor => '▓',
        HazardType.FallingRocks => '▼',
        HazardType.Pitfall => 'O',
        HazardType.Fire => '♦',
        _ => '?'
    };
}

public Color GetHazardColor(HazardType type)
{
    return type switch
    {
        HazardType.Lava => Color.Red,
        HazardType.Fire => Color.Red,
        HazardType.PoisonGas => Color.Green,
        HazardType.AcidPool => Color.Yellow,
        HazardType.ElectricFloor => Color.Cyan,
        HazardType.SpikeTrap => Color.Gray,
        HazardType.BearTrap => Color.DarkGray,
        HazardType.ArrowTrap => Color.Brown,
        HazardType.FallingRocks => Color.Gray,
        HazardType.Pitfall => Color.Black,
        _ => Color.White
    };
}
```

### Status Effects Display

```csharp
public void RenderStatusEffects(Entity entity)
{
    if (!entity.Has<HazardEffect>())
        return;

    var effect = entity.Get<HazardEffect>();
    var icon = effect.EffectName switch
    {
        "Burning" => "🔥",
        "Poisoned" => "☠",
        "Corroding" => "💧",
        _ => "✖"
    };

    Console.Write($"{icon} {effect.EffectName} ({effect.RemainingTurns})");
}
```

## 📊 Statistics

### Code Metrics

- **Components**: 6 files (~200 lines)
- **Systems**: 2 files (~350 lines)
- **Services**: 1 file (~200 lines)
- **Factories**: 1 file (~180 lines)
- **Data**: 2 files (~250 lines)
- **Documentation**: This file
- **Total**: ~1,200+ lines of code

### Features

- ✅ 10 hazard types
- ✅ 5 trigger types
- ✅ Detection system
- ✅ Disarming mechanics
- ✅ Ongoing effects
- ✅ Resistance system
- ✅ Area spawning
- ✅ Integration hooks

## ✅ Build Status

**Status**: ✅ Ready to build

**Requirements**:

- Arch.Core (ECS)
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection

## 🚀 Next Steps

1. **Build Plugin**

   ```bash
   dotnet build
   ```

2. **Test Integration**
   - Add to dungeon generator
   - Test detection mechanics
   - Verify damage application

3. **Balance Tuning**
   - Adjust damage values
   - Fine-tune detection difficulties
   - Test resistance effectiveness

## 📝 Notes

- Hazards use existing Transform and Health components from game framework
- Detection uses D&D-style skill checks (d20 + skill)
- Critical failures on disarm attempts trigger the trap
- Ongoing effects stack (multiple burns = more damage)
- Resistance is multiplicative with damage reduction

---

**Phase 9 Complete!** 🎉

Ready for Phase 10: Polish & Integration
