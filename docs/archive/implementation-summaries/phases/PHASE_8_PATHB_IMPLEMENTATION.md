# Phase 8: Path B Implementation - Service-Based Integration

**Started**: 2025-10-23
**Approach**: Proper Architecture via IRegistry
**Estimated Time**: 3 hours

## 🎯 Architecture Overview

### Current State
```
Spell Plugin (Isolated)
├── Components: Mana, SpellBook, SpellEffect
├── Systems: SpellCastingSystem, ManaRegenerationSystem, SpellEffectSystem
├── Service: SpellService ← Registered in IRegistry
└── Plugin: SpellPlugin ← Initializes and registers service

Game.Core (Framework)
├── GameStateManager ← Needs to use SpellService
├── CombatSystem ← Needs to use SpellService
└── No direct plugin references ✅
```

### Integration Pattern
```
GameStateManager/CombatSystem
    ↓ (accesses via)
IRegistry.Get<SpellService>()
    ↓ (returns)
SpellService (from Spell Plugin)
    ↓ (manages)
Mana, SpellBook components
```

**Key**: Use IRegistry as the bridge, no direct component references!

---

## 📋 Implementation Steps

### Step 1: Add IRegistry to GameStateManager (30 min)

**File**: `dotnet/framework/LablabBean.Game.Core/Services/GameStateManager.cs`

**Changes**:
1. Add `IRegistry` field
2. Update constructor to accept IRegistry
3. Add method: `InitializePlayerSpells(Entity player)`
4. Call SpellService via Registry in InitializePlayWorld()

**Code**:
```csharp
private readonly IRegistry _registry;

public GameStateManager(
    // ...existing params
    IRegistry registry)
{
    // ...existing assignments
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
}

private void InitializePlayerSpells(Entity player)
{
    // Access SpellService from registry
    var spellService = _registry.Get<SpellService>();
    if (spellService != null)
    {
        // Initialize mana
        spellService.InitializeMana(player, maxMana: 100, regen: 5);
        
        // Learn starting spell
        spellService.LearnSpell(player, "fireball");
        
        _logger.LogInformation("Player initialized with spell system");
    }
    else
    {
        _logger.LogWarning("SpellService not available - spell system disabled");
    }
}
```

5. Call in InitializePlayWorld() after player creation:
```csharp
// Create the player
var player = world.Create(/*...components...*/);

// Initialize spells if available
InitializePlayerSpells(player);
```

**Testing**:
- [ ] GameStateManager compiles
- [ ] Player entity has Mana component
- [ ] Player entity has SpellBook component
- [ ] Player knows "fireball" spell

---

### Step 2: Add IRegistry to CombatSystem (30 min)

**File**: `dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs`

**Changes**:
1. Add `IRegistry` field
2. Update constructor to accept IRegistry
3. Add method: `CastSpell(Entity caster, Entity target, string spellId)`

**Code**:
```csharp
private readonly IRegistry? _registry;

public CombatSystem(
    ILogger<CombatSystem> logger, 
    ItemSpawnSystem? itemSpawnSystem = null,
    IRegistry? registry = null)
{
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _random = new Random();
    _itemSpawnSystem = itemSpawnSystem;
    _registry = registry;
}

/// <summary>
/// Cast a spell from caster to target using SpellService.
/// </summary>
public bool CastSpell(World world, Entity caster, Entity target, string spellId)
{
    if (_registry == null)
    {
        _logger.LogWarning("Cannot cast spell - registry not available");
        return false;
    }

    var spellService = _registry.Get<SpellService>();
    if (spellService == null)
    {
        _logger.LogWarning("Cannot cast spell - SpellService not registered");
        return false;
    }

    // Validate and cast through service
    if (!spellService.CanCastSpell(caster, spellId))
    {
        _logger.LogDebug("Cannot cast {SpellId} - not ready", spellId);
        return false;
    }

    var success = spellService.CastSpell(caster, spellId, target);
    
    if (success)
    {
        var spell = spellService.GetSpell(spellId);
        _logger.LogInformation("{Caster} cast {SpellName} on {Target}!", 
            GetEntityName(caster), spell?.Name ?? spellId, GetEntityName(target));
        OnSpellCast?.Invoke(caster, target, spellId);
    }

    return success;
}

/// <summary>
/// Event triggered when a spell is successfully cast.
/// </summary>
public event Action<Entity, Entity, string>? OnSpellCast;
```

**Testing**:
- [ ] CombatSystem compiles
- [ ] CastSpell() callable
- [ ] Spell casting validates mana
- [ ] Damage applied to target

---

### Step 3: Update System Initialization (45 min)

**Files**:
- Where GameStateManager is constructed
- Where CombatSystem is constructed

**Need to find**: Dependency injection setup or manual construction sites

**Changes**: Pass `IRegistry` instance to both constructors

**Tasks**:
- [ ] Find GameStateManager construction site
- [ ] Add IRegistry parameter
- [ ] Find CombatSystem construction site  
- [ ] Add IRegistry parameter
- [ ] Verify registry contains SpellService

---

### Step 4: Add Mana Display to HUD (45 min)

**File**: `dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs` (if exists)

**Changes**:
1. Add `RenderMana()` method
2. Access player's Mana component
3. Display mana bar below health

**Code**:
```csharp
private void RenderMana(Entity player)
{
    if (!player.Has<Mana>())
        return;

    var mana = player.Get<Mana>();
    var percentage = mana.GetPercentage();
    
    // Render mana bar (similar to health)
    var barLength = 20;
    var filled = (int)(percentage * barLength);
    
    var color = percentage > 0.5f ? Color.Blue : 
                percentage > 0.25f ? Color.Yellow : 
                Color.Red;
    
    // Draw: "Mana: [████████░░] 80/100 (+5)"
    Console.Write("Mana: [");
    Console.ForegroundColor = color;
    Console.Write(new string('█', filled));
    Console.ForegroundColor = Color.Gray;
    Console.Write(new string('░', barLength - filled));
    Console.ResetColor();
    Console.WriteLine($"] {mana.Current}/{mana.Max} (+{mana.Regen})");
}
```

**Integration**: Call from main HUD render method

**Testing**:
- [ ] Mana bar visible
- [ ] Updates in real-time
- [ ] Color coding works
- [ ] Regen rate shown

---

### Step 5: Add Spell Casting UI (45 min)

**File**: `dotnet/console-app/LablabBean.Game.TerminalUI/Screens/SpellCastingScreen.cs` (new)

**Purpose**: Simple menu to select spell and target

**Code Structure**:
```csharp
public class SpellCastingScreen
{
    private readonly IRegistry _registry;
    private readonly World _world;
    
    public void Show(Entity caster)
    {
        var spellService = _registry.Get<SpellService>();
        if (spellService == null) return;
        
        var available = spellService.GetAvailableSpells(caster);
        
        // Display list
        for (int i = 0; i < available.Count; i++)
        {
            var spell = available[i];
            Console.WriteLine($"{i + 1}. {spell.Name} - {spell.ManaCost} mana");
        }
        
        // Get selection
        var choice = GetUserChoice();
        if (choice < 0 || choice >= available.Count)
            return;
            
        var selected = available[choice];
        
        // Get target (if needed)
        Entity? target = null;
        if (selected.TargetType == TargetType.Single)
        {
            target = SelectTarget();
        }
        
        // Cast spell
        var combatSystem = _registry.Get<CombatSystem>();
        combatSystem?.CastSpell(_world, caster, target, selected.SpellId);
    }
}
```

**Testing**:
- [ ] Spell menu opens
- [ ] Lists available spells
- [ ] Filters by mana/cooldown
- [ ] Target selection works
- [ ] Casting triggers

---

### Step 6: Add Hotkey Integration (30 min)

**File**: Input handling for combat/exploration

**Changes**: Add 'C' key to open spell casting menu

**Code**:
```csharp
if (key == ConsoleKey.C)
{
    var spellScreen = new SpellCastingScreen(_registry, _world);
    spellScreen.Show(playerEntity);
}
```

**Testing**:
- [ ] 'C' key opens spell menu
- [ ] ESC closes menu
- [ ] Returns to game after cast

---

### Step 7: Add Mana Regeneration Integration (15 min)

**File**: Turn processing system

**Changes**: Call ManaRegenerationSystem after each turn

**Code**:
```csharp
// At end of player turn
var manaRegenSystem = _registry.Get<ManaRegenerationSystem>();
if (manaRegenSystem != null)
{
    manaRegenSystem.RegenerateMana(playerEntity);
}
```

**Testing**:
- [ ] Mana regenerates each turn
- [ ] Regen amount correct (+5)
- [ ] Doesn't exceed max

---

### Step 8: Integration Testing (30 min)

**Full Gameplay Test**:

1. **Start Game**
   - [ ] Player has 100/100 mana
   - [ ] Mana bar visible on HUD
   - [ ] Player knows Fireball spell

2. **Enter Combat**
   - [ ] Enemy spawned
   - [ ] Combat options available

3. **Cast Spell**
   - [ ] Press 'C' opens spell menu
   - [ ] Fireball listed
   - [ ] Shows "15 damage, 10 mana, 3 turn CD"
   - [ ] Select Fireball
   - [ ] Select enemy target
   - [ ] Fireball casts

4. **Verify Effects**
   - [ ] Enemy takes 15 damage
   - [ ] Player mana: 100 → 90
   - [ ] Cooldown starts (3 turns)
   - [ ] Mana bar updates

5. **Wait for Regen**
   - [ ] Turn 1: 90 → 95 mana
   - [ ] Turn 2: 95 → 100 mana
   - [ ] Turn 3: 100 mana (capped)
   - [ ] Cooldown: 3 → 2 → 1 → 0

6. **Cast Again**
   - [ ] Fireball available after 3 turns
   - [ ] Can cast again
   - [ ] Cooldown resets

**Pass Criteria**: All checkboxes above must pass ✅

---

## 🔧 Technical Details

### IRegistry Usage Pattern

**Get Service (Safe)**:
```csharp
var service = _registry.Get<ServiceType>();
if (service != null)
{
    service.DoSomething();
}
```

**Get Service (Required)**:
```csharp
var service = _registry.Get<ServiceType>() 
    ?? throw new InvalidOperationException("ServiceType not found");
```

**Check Availability**:
```csharp
if (_registry.IsRegistered<SpellService>())
{
    // Service available
}
```

### Component Access via Service

**Never do this** ❌:
```csharp
var mana = entity.Get<Mana>(); // Direct component access from Core
```

**Always do this** ✅:
```csharp
var spellService = _registry.Get<SpellService>();
var canCast = spellService.CanCastSpell(entity, spellId); // Service handles it
```

### Spell System Methods

**SpellService API**:
- `InitializeMana(entity, maxMana, regen)` - Add mana component
- `LearnSpell(entity, spellId)` - Teach spell
- `CastSpell(caster, spellId, target)` - Cast spell
- `CanCastSpell(entity, spell)` - Check if castable
- `GetAvailableSpells(entity)` - Get ready spells
- `GetLearnedSpells(entity)` - Get all known spells
- `GetSpellCooldown(entity, spellId)` - Check cooldown

---

## 🎯 Success Criteria

### Code Quality
- [ ] No direct component references from Core to Plugin
- [ ] All access via IRegistry
- [ ] Proper null checking
- [ ] Logging at key points
- [ ] Error handling

### Functionality
- [ ] Player can cast spells
- [ ] Mana consumption works
- [ ] Cooldowns prevent spam
- [ ] Mana regeneration functional
- [ ] UI responsive

### Architecture
- [ ] Plugin isolation maintained
- [ ] Service-based integration
- [ ] Extensible for future spells
- [ ] No technical debt

---

## 📊 Progress Tracker

### Phase 1: Core Integration (1.5 hrs)
- [ ] Step 1: IRegistry to GameStateManager (30 min)
- [ ] Step 2: IRegistry to CombatSystem (30 min)
- [ ] Step 3: Update initialization (30 min)

### Phase 2: UI Integration (1.5 hrs)
- [ ] Step 4: Mana HUD display (45 min)
- [ ] Step 5: Spell casting UI (45 min)
- [ ] Step 6: Hotkey integration (15 min)
- [ ] Step 7: Mana regen hook (15 min)

### Phase 3: Testing (30 min)
- [ ] Step 8: Full integration test (30 min)

**Total Time**: 3 hours

---

## 🚀 Let's Begin!

Starting with **Step 1**: Adding IRegistry to GameStateManager...

**Next**: Ready to implement?