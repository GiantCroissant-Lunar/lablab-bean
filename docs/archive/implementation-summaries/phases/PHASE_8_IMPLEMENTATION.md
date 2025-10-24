# Phase 8: Complete Spell System - Implementation Plan

**Status**: 🚀 **IN PROGRESS**  
**Priority**: P2  
**Started**: 2025-10-23  
**Target Completion**: 2025-10-23  
**Current Progress**: 9/15 tasks (60% - Core systems complete, UI & integration pending)

## 🎯 Goal

Complete the spell system by implementing spell tomes, UI components, and combat integration to achieve a fully functional magic system.

## 📊 Current Status

### ✅ Already Implemented (Phase 6 - 9/15 tasks)

**Core Components**:
- ✅ T074: Mana.cs - Resource management
- ✅ T075: SpellBook.cs - Spell tracking & cooldowns
- ✅ T076: SpellEffect.cs - Buff/DoT tracking

**Data Structures**:
- ✅ T077: Spell.cs - Spell data model
- ✅ T078: Sample spells created (10 spells)

**Systems**:
- ✅ T079: ManaRegenerationSystem.cs - Automatic mana restore
- ✅ T080: SpellCastingSystem.cs - Casting with validation
- ✅ T081: SpellEffectSystem.cs - Effect processing

**Services**:
- ✅ T083: SpellService.cs - Service API
- ✅ T084: SpellPlugin.cs - Plugin registration

### 🔄 Remaining Tasks (6 tasks)

**T085**: Spell Tome Items (Item Integration)
- Create spell tome item type
- Integrate with inventory plugin
- Add spell learning on item use
- **Depends on**: Inventory plugin completion

**T086**: Spell Casting Menu UI
- Create spell selection interface
- Target selection UI
- Spell info display
- **Depends on**: Terminal.Gui UI framework

**T087**: Mana HUD Display
- Mana bar visualization
- Current/max mana text
- Mana regeneration indicator
- **Depends on**: HUD implementation

**T088**: Combat System Integration
- Integrate spell casting in combat
- Add spell damage calculation
- Hook spell effects to combat
- **Depends on**: CombatSystem access

**T089**: Spell Learning System
- Level-up spell unlocks
- Spell discovery mechanics
- Spell prerequisite checking
- **Depends on**: Progression plugin

**T090**: Polish & Testing
- End-to-end spell testing
- Balance tuning
- Bug fixes

## 📋 Detailed Task List

### Task T085: Spell Tome Items 📚

**Goal**: Allow players to learn spells from consumable spell tome items

**Files to Create**:
```
dotnet/plugins/LablabBean.Plugins.Inventory/Items/SpellTome.cs
dotnet/plugins/LablabBean.Plugins.Inventory/Data/Items/spell-tomes.json
```

**Implementation**:
1. Create SpellTome item type extending base Item
2. Add SpellId property linking to spell
3. Implement OnUse() to call SpellService.LearnSpell()
4. Create JSON data for spell tomes:
   - fireball-tome.json
   - heal-tome.json
   - shield-tome.json
   - (10 spell tomes total)

**Integration**:
- ItemSpawnSystem: Add spell tome spawning (5% chance in treasure rooms)
- ItemUsageSystem: Handle spell tome consumption
- SpellService: Verify LearnSpell() method exists

**Acceptance Criteria**:
- [ ] Player can find spell tome items
- [ ] Using spell tome learns the spell
- [ ] Spell tome is consumed after use
- [ ] SpellBook component updated correctly

---

### Task T086: Spell Casting Menu UI 🎨

**Goal**: Create UI for selecting and casting spells

**Files to Create**:
```
dotnet/console-app/LablabBean.Game.TerminalUI/Screens/SpellCastingScreen.cs
dotnet/console-app/LablabBean.Game.TerminalUI/UI/SpellSelectionList.cs
dotnet/console-app/LablabBean.Game.TerminalUI/UI/SpellInfoPanel.cs
```

**Implementation**:
1. **SpellCastingScreen.cs**:
   - Display learned spells from SpellBook
   - Show spell details (mana cost, cooldown, description)
   - Filter available spells (has mana, not on cooldown)
   - Handle spell selection
   - Target selection for offensive spells

2. **SpellSelectionList.cs**:
   - ListView of available spells
   - Color coding: green (available), red (insufficient mana), gray (on cooldown)
   - Keyboard navigation

3. **SpellInfoPanel.cs**:
   - Spell name and description
   - Mana cost
   - Cooldown remaining
   - Effect details

**Key Bindings**:
- `C` or `M` - Open spell casting menu
- Arrow keys - Navigate spells
- Enter - Select spell
- Esc - Cancel

**Acceptance Criteria**:
- [ ] Player can open spell menu with hotkey
- [ ] Spells displayed with correct availability status
- [ ] Selecting spell enters target mode (if needed)
- [ ] Casting spell consumes mana and applies effect
- [ ] UI closes after successful cast

---

### Task T087: Mana HUD Display 💙

**Goal**: Show mana status on main HUD

**Files to Modify**:
```
dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs
```

**Implementation**:
1. Add mana bar below health bar
2. Format: `Mana: [████████░░] 80/100 (+5/turn)`
3. Color coding:
   - Blue: >50% mana
   - Yellow: 25-50% mana
   - Red: <25% mana
4. Show mana regen rate

**Layout Example**:
```
╔══════════════════════════════════╗
║ HP:   [████████░░] 80/100        ║
║ Mana: [████████░░] 80/100 (+5)   ║
║ Lvl 5 | XP: 450/500               ║
╚══════════════════════════════════╝
```

**Acceptance Criteria**:
- [ ] Mana bar visible on HUD
- [ ] Updates in real-time
- [ ] Color changes based on mana %
- [ ] Regen rate displayed

---

### Task T088: Combat System Integration ⚔️

**Goal**: Enable spell casting during combat

**Files to Modify**:
```
dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs
dotnet/framework/LablabBean.Game.Core/Systems/TurnSystem.cs
```

**Implementation**:
1. **CombatSystem.cs enhancements**:
   - Add HandleSpellCast() method
   - Calculate spell damage using spell stats
   - Apply spell effects to targets
   - Trigger OnSpellCast event

2. **TurnSystem.cs enhancements**:
   - Add "Cast Spell" action option
   - Process mana regeneration on turn end
   - Decrement spell cooldowns

3. **Combat Flow**:
   ```
   Player Turn Options:
   1. Attack (melee)
   2. Cast Spell (if has spells)
   3. Use Item
   4. Defend
   ```

**Spell Damage Calculation**:
```csharp
int totalDamage = spell.BaseDamage + (playerIntelligence / 2);
// Apply to target
target.TakeDamage(totalDamage);
```

**Acceptance Criteria**:
- [ ] "Cast Spell" option appears in combat
- [ ] Spell damage calculated correctly
- [ ] Spell effects apply to targets
- [ ] Mana consumed on cast
- [ ] Cooldowns tracked properly

---

### Task T089: Spell Learning System 📖

**Goal**: Unlock spells through level progression

**Files to Create**:
```
dotnet/plugins/LablabBean.Plugins.Spell/Data/SpellUnlocks.json
dotnet/plugins/LablabBean.Plugins.Spell/Systems/SpellUnlockSystem.cs
```

**Files to Modify**:
```
dotnet/plugins/LablabBean.Plugins.Progression/Systems/LevelingSystem.cs (if exists)
```

**Implementation**:
1. **SpellUnlocks.json**:
```json
{
  "unlocks": [
    { "level": 1, "spellId": "minor-heal" },
    { "level": 3, "spellId": "fireball" },
    { "level": 5, "spellId": "shield" },
    { "level": 7, "spellId": "lightning-bolt" },
    { "level": 10, "spellId": "greater-heal" }
  ]
}
```

2. **SpellUnlockSystem.cs**:
   - Subscribe to OnPlayerLevelUp event
   - Check SpellUnlocks.json for level
   - Automatically learn spell
   - Show notification

3. **Progression Integration**:
   - If progression plugin exists, integrate
   - Otherwise, create simple level tracking

**Acceptance Criteria**:
- [ ] Spells unlock at specified levels
- [ ] Player notified of new spell
- [ ] Spell added to SpellBook automatically

---

### Task T090: Polish & Testing ✨

**Goal**: Ensure spell system is bug-free and balanced

**Testing Checklist**:
1. **Mana System**:
   - [ ] Mana regenerates correctly per turn
   - [ ] Mana consumption works
   - [ ] Cannot cast with insufficient mana
   - [ ] MaxMana respected

2. **Spell Casting**:
   - [ ] All 10 spells cast successfully
   - [ ] Damage spells hurt enemies
   - [ ] Healing spells restore HP
   - [ ] Buff spells apply effects
   - [ ] Cooldowns prevent spam

3. **Spell Effects**:
   - [ ] DoT effects tick correctly
   - [ ] Buffs expire after duration
   - [ ] Shield reduces incoming damage
   - [ ] Haste increases speed

4. **UI**:
   - [ ] Spell menu navigable
   - [ ] Mana bar accurate
   - [ ] Cooldown indicators correct

5. **Integration**:
   - [ ] Spell tomes learnable
   - [ ] Combat casting functional
   - [ ] Level unlocks working

**Balance Tuning**:
- Mana costs appropriate?
- Cooldowns too long/short?
- Spell damage balanced vs melee?
- Mana regen rate fair?

**Acceptance Criteria**:
- [ ] All tests pass
- [ ] No critical bugs
- [ ] Gameplay feels balanced

---

## 🔄 Implementation Order

### Phase 1: Item Integration (T085) - 1 hour
✅ **Start Here**: Spell tomes enable spell discovery

### Phase 2: Combat Integration (T088) - 1 hour
⚔️ Makes spells usable in gameplay

### Phase 3: UI Components (T086, T087) - 2 hours
🎨 Player-facing interfaces

### Phase 4: Progression (T089) - 30 minutes
📖 Spell unlocks

### Phase 5: Polish (T090) - 1 hour
✨ Testing and tuning

**Total Estimated Time**: 5-6 hours

---

## 🎮 Gameplay Flow (Complete System)

### Discovery
1. Player finds spell tome in dungeon
2. Uses spell tome from inventory
3. Spell learned and added to SpellBook

### Casting
1. Player presses `C` to open spell menu
2. Selects available spell
3. Chooses target (if needed)
4. Spell casts, mana consumed, cooldown starts

### Combat
1. Enemy turn ends
2. Player turn begins
3. Options: Attack, Cast Spell, Use Item, Defend
4. Selects "Cast Spell"
5. Spell menu opens in combat context
6. Casts fireball at enemy
7. Enemy takes damage, spell effect applies

### Progression
1. Player levels up to level 5
2. Notification: "New spell unlocked: Shield!"
3. Shield automatically added to SpellBook
4. Player can now cast Shield

---

## 📊 Success Metrics

**Completion Criteria**:
- ✅ All 15 tasks complete (100%)
- ✅ 10 spells fully functional
- ✅ Spell tomes spawn and work
- ✅ UI fully integrated
- ✅ Combat casting operational
- ✅ No critical bugs

**User Experience Goals**:
- Spell discovery feels rewarding
- Casting is intuitive
- Mana management adds strategy
- Spells provide meaningful alternatives to melee

---

## 🔗 Dependencies

**Requires**:
- ✅ Inventory plugin (for spell tomes)
- ✅ Combat system (for spell casting in battle)
- ⏳ Progression plugin (optional - for level unlocks)
- ⏳ Terminal.Gui UI (for screens)

**Provides**:
- Complete spell casting system
- Mana resource management
- Spell effect framework
- Magic-based gameplay

---

## 🚀 Quick Start

**For Developers**:
```bash
# Check existing spell implementation
cd dotnet/plugins/LablabBean.Plugins.Spell
ls Components/  # Mana, SpellBook, SpellEffect
ls Systems/     # SpellCastingSystem, ManaRegenerationSystem, SpellEffectSystem
ls Services/    # SpellService

# Start with T085: Spell Tomes
cd ../LablabBean.Plugins.Inventory
# Create Items/SpellTome.cs

# Then T088: Combat Integration
cd ../../framework/LablabBean.Game.Core
# Modify Systems/CombatSystem.cs

# Then UI tasks (T086, T087)
cd ../../console-app/LablabBean.Game.TerminalUI
# Create spell UI components
```

---

**Version**: 1.0.0  
**Last Updated**: 2025-10-23  
**Estimated Completion**: 5-6 hours  
**Status**: Ready to implement - core systems complete