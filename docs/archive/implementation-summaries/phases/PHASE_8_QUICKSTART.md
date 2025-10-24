# Phase 8: Complete Spell System - Quick Start Guide

**Current Status**: 🎯 60% Complete (9/15 tasks done)

## ✅ What's Already Working

The spell system core is **fully functional**:
- ✅ Mana management (Mana component)
- ✅ SpellBook tracking learned spells
- ✅ Spell casting validation (mana cost, cooldowns)
- ✅ Spell effects (buffs, debuffs, DoT)
- ✅ Mana regeneration system
- ✅ 10 sample spells created
- ✅ SpellService API ready
- ✅ Plugin properly registered

## 🎯 What We Need to Add (40% remaining)

### Simplified 3-Step Plan

Instead of 6 complex tasks, we'll do **3 focused additions**:

#### **Step 1: Add Spell Tomes as Items** (30 min)
**What**: Make spells learnable from items found in dungeon

**Changes**:
1. Add `SpellTome` to `ItemType` enum
2. Create spell tome component with SpellId
3. Update item usage system to handle spell learning

**Files**:
- `dotnet/framework/LablabBean.Game.Core/Components/Item.cs` - Add ItemType.SpellTome
- `dotnet/framework/LablabBean.Game.Core/Components/SpellTomeData.cs` - New component
- `dotnet/framework/LablabBean.Game.Core/Systems/ItemSpawnSystem.cs` - Add spell tome spawning

#### **Step 2: Combat Integration** (45 min)
**What**: Allow spell casting during combat

**Changes**:
1. Add spell casting to CombatSystem
2. Add mana checks before casting
3. Apply spell damage/healing/buffs
4. Trigger spell cooldowns

**Files**:
- `dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs` - Add CastSpell method
- `dotnet/framework/LablabBean.Game.Core/Systems/TurnSystem.cs` - Add mana regen on turn end

#### **Step 3: Basic UI** (45 min)
**What**: Display mana on HUD and allow spell selection

**Changes**:
1. Add mana bar to HUD
2. Simple spell menu (list available spells)
3. Keybind to open spell menu (`C` key)

**Files**:
- `dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs` - Add mana display
- `dotnet/console-app/LablabBean.Game.TerminalUI/Screens/SpellMenuScreen.cs` - New simple menu

**Total Time**: ~2 hours

---

## 🚀 Implementation Order

### Priority 1: Step 2 (Combat)
**Why**: Makes spells actually usable in gameplay
- **Skip spell tomes for now** - just give player starting spells
- Focus on combat casting working end-to-end

### Priority 2: Step 3 (UI)
**Why**: Player needs to see mana and cast spells
- Minimal UI - just show mana and spell list

### Priority 3: Step 1 (Items)  
**Why**: Nice-to-have for spell discovery
- Can be added later as polish

---

## 📝 Minimal Viable Spell System (MVP)

**Goal**: Player can cast ONE spell (Fireball) in combat using mana

**Implementation**:

### 1. Give Player Starting Spell (5 min)
```csharp
// In player initialization
var spellBook = new SpellBook();
spellBook.LearnedSpells.Add("fireball");
playerEntity.Add(spellBook);

var mana = new Mana { Current = 100, Max = 100, RegenRate = 5 };
playerEntity.Add(mana);
```

### 2. Add Combat Spell Cast (15 min)
```csharp
// In CombatSystem.cs
public bool CastSpell(World world, Entity caster, Entity target, string spellId)
{
    // Get spell from SpellService
    // Check mana
    // Apply damage/effect
    // Consume mana
    // Trigger cooldown
}
```

### 3. Add Mana to HUD (10 min)
```csharp
// In HUD.cs
public void RenderMana(Mana mana)
{
    // Show: "Mana: 80/100"
    // Blue bar below HP
}
```

### 4. Add Spell Hotkey (20 min)
```csharp
// In combat input handling
if (key == 'C')
{
    // Show simple spell menu
    // Let player select spell
    // Let player select target
    // Call CombatSystem.CastSpell()
}
```

**Total MVP Time**: ~50 minutes

---

## 🎮 Testing Checklist

After MVP implementation:

1. **Mana Basics**:
   - [ ] Player starts with 100/100 mana
   - [ ] Mana bar visible on HUD
   - [ ] Mana regenerates each turn

2. **Spell Casting**:
   - [ ] Press 'C' in combat opens spell menu
   - [ ] Fireball spell shows in menu
   - [ ] Select fireball, select enemy target
   - [ ] Spell casts, damage applied, mana consumed

3. **Validation**:
   - [ ] Cannot cast with insufficient mana
   - [ ] Cooldown prevents immediate re-cast
   - [ ] Mana regen works over turns

---

## ✨ What This Gets Us

**After MVP (50 min)**:
- ✅ Working spell casting in combat
- ✅ Mana resource management
- ✅ One functional spell (Fireball)
- ✅ Basic UI for spells
- **System 70% complete**

**After Full Implementation (2 hrs)**:
- ✅ All 10 spells working
- ✅ Better UI with spell details
- ✅ Spell tomes for discovery
- **System 100% complete** 🎉

---

## 🛠️ Technical Notes

**Spell Data Location**:
- Spells defined in: `dotnet/plugins/LablabBean.Plugins.Spell/Data/Spells/`
- Current spells: fireball, heal, lightning-bolt, shield, etc.

**Service Access**:
- SpellService already registered in SpellPlugin
- Access via: `_serviceProvider.GetService<ISpellService>()`

**Integration Points**:
- CombatSystem: Add spell damage calculation
- TurnSystem: Add mana regeneration hook
- Player Init: Add Mana & SpellBook components

---

## 🎯 Recommended Approach

**Option A: MVP First** (50 min) ⭐ RECOMMENDED
- Get spells working quickly
- Test gameplay feel
- Decide if worth expanding

**Option B: Full Implementation** (2 hrs)
- Complete all features
- Polish UI
- Full spell discovery loop

**Option C: Defer for Now**
- Core systems done
- Can revisit after other features
- Spells not critical for MVP gameplay

---

**Let's start with Option A (MVP)?** 🚀

It gets us working spells fastest, then we can decide next steps based on how it feels in gameplay.

---

**Version**: 1.0.0  
**Created**: 2025-10-23  
**Estimated Time**: MVP = 50 min, Full = 2 hrs