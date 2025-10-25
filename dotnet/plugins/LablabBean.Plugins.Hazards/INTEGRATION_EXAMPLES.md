# Hazards System Integration Examples

## 🎮 Game Loop Integration

### Complete Turn Processing

```csharp
public class GameManager
{
    private readonly HazardService _hazardService;
    private readonly MovementSystem _movementSystem;

    public void ProcessTurn()
    {
        // 1. Handle player input
        var action = GetPlayerAction();

        // 2. Execute player movement
        if (action is MoveAction move)
        {
            var newPos = CalculateNewPosition(player, move.Direction);

            // Move player
            _movementSystem.Move(player, newPos);

            // Check hazards at new position
            _hazardService.OnEntityMove(player, newPos.x, newPos.y);
        }

        // 3. Process AI turns
        ProcessEnemies();

        // 4. Update all hazards
        _hazardService.UpdateHazards();
        // - Periodic hazards tick
        // - Ongoing effects apply damage
        // - Cooldowns update

        // 5. Check win/lose conditions
        CheckGameState();
    }
}
```

## 🗺️ Map Generation Integration

### Adding Hazards to Rooms

```csharp
public class DungeonGenerator
{
    private readonly HazardFactory _hazardFactory;
    private readonly HazardService _hazardService;

    public void GenerateDungeon()
    {
        var rooms = GenerateRooms();
        var corridors = ConnectRooms(rooms);

        foreach (var room in rooms)
        {
            DecorateRoom(room);
        }

        foreach (var corridor in corridors)
        {
            DecorateCorridor(corridor);
        }
    }

    private void DecorateRoom(Room room)
    {
        var roomType = DetermineRoomType(room);

        switch (roomType)
        {
            case RoomType.TreasureRoom:
                AddTreasureRoomHazards(room);
                break;

            case RoomType.LavaRoom:
                AddLavaRoom(room);
                break;

            case RoomType.TrapRoom:
                AddTrapRoom(room);
                break;

            case RoomType.PoisonRoom:
                AddPoisonRoom(room);
                break;
        }
    }

    private void AddTreasureRoomHazards(Room room)
    {
        // Chest in center
        var centerX = room.X + room.Width / 2;
        var centerY = room.Y + room.Height / 2;
        PlaceChest(centerX, centerY);

        // Hidden traps around chest
        _hazardFactory.CreateBearTrap(centerX - 1, centerY, hidden: true);
        _hazardFactory.CreateSpikeTrap(centerX + 1, centerY, hidden: true);
        _hazardFactory.CreateArrowTrap(centerX, centerY - 1, hidden: true);
        _hazardFactory.CreatePitfall(centerX, centerY + 1, hidden: true);
    }

    private void AddLavaRoom(Room room)
    {
        // Lava border
        for (int x = room.X + 1; x < room.X + room.Width - 1; x++)
        {
            _hazardFactory.CreateLava(x, room.Y + 1);
            _hazardFactory.CreateLava(x, room.Y + room.Height - 2);
        }

        for (int y = room.Y + 2; y < room.Y + room.Height - 2; y++)
        {
            _hazardFactory.CreateLava(room.X + 1, y);
            _hazardFactory.CreateLava(room.X + room.Width - 2, y);
        }

        // Safe path through middle
        var centerX = room.X + room.Width / 2;
        for (int y = room.Y + 1; y < room.Y + room.Height - 1; y++)
        {
            ClearTile(centerX, y);
        }
    }

    private void AddTrapRoom(Room room)
    {
        // Random traps throughout
        _hazardService.SpawnHazardsInArea(
            room.X + 1,
            room.Y + 1,
            room.Width - 2,
            room.Height - 2,
            hazardCount: 5
        );
    }

    private void AddPoisonRoom(Room room)
    {
        // Poison gas clouds
        var cloudCount = Random.Shared.Next(3, 6);
        for (int i = 0; i < cloudCount; i++)
        {
            var x = Random.Shared.Next(room.X + 1, room.X + room.Width - 1);
            var y = Random.Shared.Next(room.Y + 1, room.Y + room.Height - 1);
            _hazardFactory.CreatePoisonGas(x, y);
        }
    }

    private void DecorateCorridor(Corridor corridor)
    {
        // 20% chance of hazards in corridor
        if (Random.Shared.NextDouble() < 0.2)
        {
            var midpoint = corridor.Length / 2;
            var x = corridor.StartX + (corridor.IsHorizontal ? midpoint : 0);
            var y = corridor.StartY + (corridor.IsHorizontal ? 0 : midpoint);

            // Random trap type
            var trapType = Random.Shared.Next(4);
            switch (trapType)
            {
                case 0:
                    _hazardFactory.CreateSpikeTrap(x, y, hidden: true);
                    break;
                case 1:
                    _hazardFactory.CreateArrowTrap(x, y, hidden: true);
                    break;
                case 2:
                    _hazardFactory.CreateFallingRocks(x, y, hidden: true);
                    break;
                case 3:
                    _hazardFactory.CreatePitfall(x, y, hidden: true);
                    break;
            }
        }
    }
}
```

## 🎯 Player Actions Integration

### Detection Action

```csharp
public class PlayerController
{
    private readonly HazardService _hazardService;
    private readonly Entity _player;

    public void SearchForTraps()
    {
        var playerPos = GetPlayerPosition();
        var perceptionSkill = GetPlayerStat("Perception");

        var detected = _hazardService.DetectHazards(
            playerPos.x,
            playerPos.y,
            range: 3,
            skill: perceptionSkill
        );

        if (detected.Count == 0)
        {
            MessageLog.Add("You don't find any traps.");
        }
        else
        {
            MessageLog.Add($"You detected {detected.Count} trap(s)!");

            foreach (var hazard in detected)
            {
                var info = _hazardService.GetHazardInfo(hazard);
                MessageLog.Add($"  - {info}");

                // Mark on map
                MarkHazardOnMap(hazard);
            }

            GainXP(10 * detected.Count);
        }

        // Searching takes a turn
        AdvanceTurn();
    }

    public void DisarmTrap(Entity trapEntity)
    {
        var disarmSkill = GetPlayerStat("Disable Device");

        MessageLog.Add("You carefully attempt to disarm the trap...");

        var success = _hazardService.DisarmHazard(trapEntity, disarmSkill);

        if (success)
        {
            MessageLog.Add("Success! You disarmed the trap.", Color.Green);
            GainXP(50);

            // Maybe get components
            if (Random.Shared.NextDouble() < 0.3)
            {
                AddItem("trap_components", 1);
                MessageLog.Add("You salvaged some trap components.");
            }
        }
        else
        {
            MessageLog.Add("You failed to disarm the trap.", Color.Yellow);

            // Check if trap triggered (on critical failure)
            var hazard = trapEntity.Get<Hazard>();
            if (hazard.State == HazardState.Triggered)
            {
                MessageLog.Add("The trap triggers!", Color.Red);
                // Damage already applied by system
            }
        }

        AdvanceTurn();
    }
}
```

## 🛡️ Equipment Integration

### Hazard Protection Items

```csharp
public class EquipmentManager
{
    private readonly HazardService _hazardService;

    public void OnItemEquipped(Entity player, Item item)
    {
        switch (item.Id)
        {
            case "fireproof_boots":
                _hazardService.AddResistance(player, HazardType.Fire, 0.75f);
                _hazardService.AddResistance(player, HazardType.Lava, 0.75f);
                break;

            case "acid_resistant_armor":
                _hazardService.AddResistance(player, HazardType.AcidPool, 0.5f);
                break;

            case "insulated_gloves":
                _hazardService.AddResistance(player, HazardType.ElectricFloor, 0.6f);
                break;

            case "thieves_tools":
                // Bonus to disarming
                player.Get<Skills>().DisableDevice += 5;
                break;

            case "trap_detector_ring":
                // Passive detection bonus
                player.Get<Skills>().Perception += 3;
                break;
        }
    }

    public void OnItemUnequipped(Entity player, Item item)
    {
        switch (item.Id)
        {
            case "fireproof_boots":
                _hazardService.AddResistance(player, HazardType.Fire, 0f);
                _hazardService.AddResistance(player, HazardType.Lava, 0f);
                break;

            // ... remove other resistances
        }
    }
}
```

## 🧪 Consumable Items Integration

### Hazard-Related Potions

```csharp
public class ConsumableManager
{
    private readonly HazardService _hazardService;

    public void UsePotion(Entity player, string potionId)
    {
        switch (potionId)
        {
            case "antidote":
                // Remove poison effect
                if (player.Has<HazardEffect>())
                {
                    var effect = player.Get<HazardEffect>();
                    if (effect.SourceType == HazardType.PoisonGas)
                    {
                        _hazardService.RemoveHazardEffect(player);
                        MessageLog.Add("The poison leaves your system.");
                    }
                }
                break;

            case "fire_resistance_potion":
                _hazardService.AddResistance(player, HazardType.Fire, 0.75f);
                _hazardService.AddResistance(player, HazardType.Lava, 0.75f);
                AddTimedEffect(player, "Fire Resistance", 100);
                break;

            case "universal_antidote":
                // Remove all hazard effects
                _hazardService.RemoveHazardEffect(player);
                MessageLog.Add("You feel cleansed of all ailments.");
                break;

            case "healing_salve":
                // Also clears burning
                if (player.Has<HazardEffect>())
                {
                    var effect = player.Get<HazardEffect>();
                    if (effect.EffectName == "Burning")
                    {
                        _hazardService.RemoveHazardEffect(player);
                        MessageLog.Add("The salve soothes your burns.");
                    }
                }
                Heal(player, 10);
                break;
        }
    }
}
```

## ⚔️ Combat Integration

### Environmental Combat

```csharp
public class CombatManager
{
    private readonly HazardService _hazardService;
    private readonly HazardFactory _hazardFactory;

    // Push enemy into hazard
    public void ExecuteShoveAttack(Entity attacker, Entity target, Direction direction)
    {
        var targetPos = GetPosition(target);
        var newPos = Move(targetPos, direction);

        // Check if shove succeeds
        var attackerStr = GetStat(attacker, "Strength");
        var targetStr = GetStat(target, "Strength");

        if (attackerStr + Random.Shared.Next(1, 21) > targetStr + 10)
        {
            MessageLog.Add($"{GetName(attacker)} shoves {GetName(target)}!");

            // Move target
            SetPosition(target, newPos);

            // Check for hazards
            _hazardService.OnEntityMove(target, newPos.x, newPos.y);

            var hazards = _hazardService.GetHazardsAtPosition(newPos.x, newPos.y);
            if (hazards.Any())
            {
                MessageLog.Add($"{GetName(target)} is pushed into a hazard!", Color.Orange);
            }
        }
    }

    // Fire spell creates hazards
    public void CastFirestorm(int centerX, int centerY, int radius)
    {
        // Deal damage to all in radius
        var targets = GetEntitiesInRadius(centerX, centerY, radius);
        foreach (var target in targets)
        {
            DealDamage(target, 20, DamageType.Fire);
        }

        // Create fire hazards
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    var x = centerX + dx;
                    var y = centerY + dy;

                    if (IsTileWalkable(x, y) && Random.Shared.NextDouble() < 0.4)
                    {
                        _hazardFactory.CreateFire(x, y);
                    }
                }
            }
        }

        MessageLog.Add("The area is engulfed in flames!");
    }

    // Ice spell disables hazards
    public void CastFrostNova(int centerX, int centerY, int radius)
    {
        var hazards = _hazardService.GetAllHazards();

        foreach (var (entity, hazard) in hazards)
        {
            if (!entity.Has<Transform>())
                continue;

            var transform = entity.Get<Transform>();
            var distance = CalculateDistance(
                centerX, centerY,
                transform.X, transform.Y
            );

            if (distance <= radius)
            {
                // Freeze fire hazards (remove them)
                if (hazard.Type == HazardType.Fire)
                {
                    entity.Destroy();
                    MessageLog.Add("Fire extinguished by frost!");
                }

                // Disable traps temporarily
                if (hazard.RequiresDetection)
                {
                    var newHazard = hazard;
                    newHazard.State = HazardState.Disabled;
                    entity.Set(newHazard);
                }
            }
        }
    }
}
```

## 📊 UI Integration

### Status Display

```csharp
public class HUDManager
{
    private readonly HazardService _hazardService;

    public void RenderPlayerStatus(Entity player)
    {
        // ... health, mana, etc.

        // Show hazard effects
        if (player.Has<HazardEffect>())
        {
            var effect = player.Get<HazardEffect>();
            var icon = GetEffectIcon(effect.EffectName);
            var color = GetEffectColor(effect.SourceType);

            Console.ForegroundColor = color;
            Console.Write($"{icon} {effect.EffectName} ");
            Console.Write($"({effect.DamagePerTurn} dmg/turn, {effect.RemainingTurns} turns)");
            Console.ResetColor();
        }

        // Show resistances
        if (player.Has<HazardResistance>())
        {
            var resistance = player.Get<HazardResistance>();
            Console.Write("Resistances: ");

            foreach (var (type, value) in resistance.Resistances)
            {
                if (value > 0)
                {
                    Console.Write($"{type}: {value * 100:F0}% ");
                }
            }
        }
    }

    public void RenderTileInfo(int x, int y)
    {
        var hazards = _hazardService.GetHazardsAtPosition(x, y);

        if (hazards.Count > 0)
        {
            foreach (var hazard in hazards)
            {
                var info = _hazardService.GetHazardInfo(hazard);
                Console.WriteLine($"⚠ {info}");
            }
        }
    }
}
```

---

## 🎯 Complete Example: Hazardous Boss Room

```csharp
public class BossRoomGenerator
{
    private readonly HazardFactory _hazardFactory;
    private readonly HazardService _hazardService;

    public void CreateBossRoom(Room room, string bossType)
    {
        var centerX = room.X + room.Width / 2;
        var centerY = room.Y + room.Height / 2;

        // Place boss
        var boss = CreateBoss(bossType, centerX, centerY);

        // Boss-specific hazards
        switch (bossType)
        {
            case "FireDragon":
                CreateFireDragonRoom(room, boss);
                break;

            case "TrapMaster":
                CreateTrapMasterRoom(room, boss);
                break;

            case "PoisonLich":
                CreatePoisonLichRoom(room, boss);
                break;
        }
    }

    private void CreateFireDragonRoom(Room room, Entity boss)
    {
        // Lava pools around the edges
        CreateLavaPerimeter(room);

        // Boss immune to fire
        _hazardService.AddResistance(boss, HazardType.Fire, 1.0f);
        _hazardService.AddResistance(boss, HazardType.Lava, 1.0f);

        // Boss ability: Breath fire (creates hazards)
        boss.Get<AI>().OnSpecialAbility = () =>
        {
            var bossPos = GetPosition(boss);
            var playerPos = GetPlayerPosition();
            var direction = GetDirection(bossPos, playerPos);

            // Line of fire hazards
            for (int i = 1; i <= 5; i++)
            {
                var x = bossPos.x + direction.x * i;
                var y = bossPos.y + direction.y * i;
                _hazardFactory.CreateFire(x, y);
            }

            MessageLog.Add("The dragon breathes fire!", Color.Red);
        };
    }

    private void CreateTrapMasterRoom(Room room, Entity boss)
    {
        // Hidden traps everywhere
        for (int i = 0; i < 15; i++)
        {
            var x = Random.Shared.Next(room.X + 1, room.X + room.Width - 1);
            var y = Random.Shared.Next(room.Y + 1, room.Y + room.Height - 1);

            var trapType = Random.Shared.Next(5);
            switch (trapType)
            {
                case 0: _hazardFactory.CreateSpikeTrap(x, y, true); break;
                case 1: _hazardFactory.CreateBearTrap(x, y, true); break;
                case 2: _hazardFactory.CreateArrowTrap(x, y, true); break;
                case 3: _hazardFactory.CreatePitfall(x, y, true); break;
                case 4: _hazardFactory.CreateFallingRocks(x, y, true); break;
            }
        }

        // Boss periodically spawns new traps
        boss.Get<AI>().OnSpecialAbility = () =>
        {
            var playerPos = GetPlayerPosition();
            _hazardFactory.CreateBearTrap(playerPos.x, playerPos.y, false);
            MessageLog.Add("A trap appears beneath your feet!", Color.Red);
        };
    }

    private void CreatePoisonLichRoom(Room room, Entity boss)
    {
        // Poison gas clouds
        for (int i = 0; i < 8; i++)
        {
            var x = Random.Shared.Next(room.X + 1, room.X + room.Width - 1);
            var y = Random.Shared.Next(room.Y + 1, room.Y + room.Height - 1);
            _hazardFactory.CreatePoisonGas(x, y);
        }

        // Boss immune to poison
        _hazardService.AddResistance(boss, HazardType.PoisonGas, 1.0f);

        // Boss spreads poison
        boss.Get<AI>().OnSpecialAbility = () =>
        {
            var bossPos = GetPosition(boss);

            // Create poison in 3x3 area
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    _hazardFactory.CreatePoisonGas(
                        bossPos.x + dx,
                        bossPos.y + dy
                    );
                }
            }

            MessageLog.Add("Toxic gas spreads from the lich!", Color.Green);
        };
    }
}
```

This integration creates dynamic, challenging encounters where environmental hazards play a key role in combat strategy!
