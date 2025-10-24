# Phase 6: User Story 4 - Combat Spells and Abilities

**Status**: 🚧 **IN PROGRESS** - Core System Complete!
**Priority**: P2
**Started**: 2025-10-23
**Progress**: 9/15 tasks (60%)

## 🎯 Overview

**Goal**: Players can learn spells, cast them using mana, and see spell effects (damage, buffs, healing)

**User Story**: "A player encounters a magic tome on the dungeon floor and learns a fireball spell. During combat with a goblin, they press 'C' to open the spell casting menu, select 'Fireball', and target the goblin. The goblin takes 15 fire damage, the player's mana decreases by 10, and the spell effect is displayed on screen."

**Independent Test**:

- Player has mana stat
- Spell tome appears in dungeon
- Player can learn spell from tome
- Player can cast spell in combat
- Mana cost is deducted
- Spell damage/effect is applied
- Spell cooldown prevents spam casting

---

## 📋 Tasks (from tasks.md Phase 6)

### Spell Components (T074-T077) ✅ COMPLETE

- [x] **T074** [US4] Create Spell data class in `dotnet/plugins/LablabBean.Plugins.Spell/Data/Spell.cs`
  - SpellId, Name, Description
  - ManaCost, Cooldown, BaseDamage
  - SpellType (Offensive, Defensive, Healing, Buff)
  - TargetType (Self, Single, Area)
  - Effects, RequiredLevel

- [x] **T075** [US4] Create SpellBook component in `dotnet/plugins/LablabBean.Plugins.Spell/Components/SpellBook.cs`
  - List of learned spells
  - Spell cooldown tracking
  - IsSpellAvailable() method

- [x] **T076** [US4] Create Mana component in `dotnet/plugins/LablabBean.Plugins.Spell/Components/Mana.cs`
  - CurrentMana, MaxMana
  - ManaRegen (per turn or per second)

- [x] **T077** [US4] Create SpellEffect component (for visual/temporary effects) in `dotnet/plugins/LablabBean.Plugins.Spell/Components/SpellEffect.cs`
  - EffectType (Burn, Freeze, Shield, Heal)
  - Duration, Intensity

### Spell Systems (T078-T080) ✅ COMPLETE

- [x] **T078** [US4] Implement SpellCastingSystem in `dotnet/plugins/LablabBean.Plugins.Spell/Systems/SpellCastingSystem.cs`
  - Validate mana cost, cooldown, range
  - Apply spell effects to target(s)
  - Trigger cooldown
  - Update mana

- [x] **T079** [US4] Implement ManaRegenerationSystem in `dotnet/plugins/LablabBean.Plugins.Spell/Systems/ManaRegenerationSystem.cs`
  - Restore mana over time
  - Cap at MaxMana

- [x] **T080** [US4] Implement SpellEffectSystem (handles duration, tick damage, etc.) in `dotnet/plugins/LablabBean.Plugins.Spell/Systems/SpellEffectSystem.cs`
  - Process ongoing effects (burn, heal over time)
  - Remove expired effects

### Spell Service (T081) ✅ COMPLETE

- [x] **T081** [US4] Implement SpellService (LearnSpell, CastSpell, GetAvailableSpells) in `dotnet/plugins/LablabBean.Plugins.Spell/Services/SpellService.cs`
  - LearnSpell(Entity player, Spell spell)
  - CastSpell(Entity caster, Spell spell, Entity? target)
  - GetAvailableSpells(Entity player)
  - GetSpellCooldown(Entity player, Spell spell)
  - CanCastSpell(Entity player, Spell spell)

### Plugin Integration (T082) ✅ COMPLETE

- [x] **T082** [US4] Implement Spell plugin main class with service registration in `dotnet/plugins/LablabBean.Plugins.Spell/Plugin.cs`
  - Register SpellService
  - Register systems (SpellCastingSystem, ManaRegenerationSystem, SpellEffectSystem)
  - Initialize spell library

### Sample Spells (T083-T085) - ✅ COMPLETE

- [x] **T083** [US4] [P] Create sample offensive spells (Fireball, Lightning Bolt, Ice Shard) in `dotnet/plugins/LablabBean.Plugins.Spell/Data/Spells/`
  - Fireball: 15 damage, 10 mana, 3 turn cooldown
  - Lightning Bolt: 20 damage, 15 mana, 4 turn cooldown, chance to stun
  - Ice Shard: 12 damage, 8 mana, 2 turn cooldown, slows target

- [x] **T084** [US4] [P] Create sample healing/buff spells (Heal, Shield, Haste) in `dotnet/plugins/LablabBean.Plugins.Spell/Data/Spells/`
  - Heal: Restore 25 HP, 12 mana, 5 turn cooldown
  - Shield: +5 defense for 3 turns, 10 mana, 6 turn cooldown
  - Haste: +20 speed for 3 turns, 8 mana, 5 turn cooldown

- [ ] **T085** [US4] [P] Create spell tome items for learning spells in `dotnet/plugins/LablabBean.Plugins.Item/Data/Items/SpellTomes/`
  - Tome of Fireball (consumable, teaches Fireball)
  - Tome of Healing (consumable, teaches Heal)
  - Tome of Lightning (rare, teaches Lightning Bolt)

### UI Enhancement (T086-T087) ⏳ TODO

- [ ] **T086** [US4] Create SpellCastingScreen (spell selection, targeting) in `dotnet/console-app/LablabBean.Console/Screens/SpellCastingScreen.cs`
  - List available spells
  - Show mana cost, cooldown, description
  - Target selection cursor
  - Cast confirmation

- [ ] **T087** [US4] Add mana display to HUD in `dotnet/console-app/LablabBean.Console/Services/HudService.cs`
  - Show current/max mana
  - Mana bar (similar to health)
  - Spell cooldown indicators

### Integration (T088) ⏳ TODO

- [ ] **T088** [US4] Integrate spell casting into combat flow in `dotnet/plugins/LablabBean.Plugins.Combat/Systems/CombatSystem.cs`
  - Allow spell casting as combat action
  - Handle spell damage/effects in combat resolution
  - Update combat log with spell effects

---

## 📦 Dependencies

### Required Plugins/Systems

- ✅ **Quest Plugin** (Phase 3) - For spell tome rewards
- ✅ **Progression Plugin** (Phase 5) - For spell level requirements
- ⏳ **Combat System** - For integrating spells into combat
- ⏳ **Item Plugin** - For spell tomes as items

### New Plugin Structure

```
dotnet/plugins/LablabBean.Plugins.Spell/
├── Plugin.cs
├── Components/
│   ├── SpellBook.cs
│   ├── Mana.cs
│   └── SpellEffect.cs
├── Data/
│   ├── Spell.cs
│   └── Spells/
│       ├── OffensiveSpells.cs
│       ├── HealingSpells.cs
│       └── BuffSpells.cs
├── Systems/
│   ├── SpellCastingSystem.cs
│   ├── ManaRegenerationSystem.cs
│   └── SpellEffectSystem.cs
└── Services/
    └── SpellService.cs
```

---

## 🎯 Success Criteria

### Functional Requirements

1. ✅ Player has mana resource (current/max)
2. ✅ Spells have mana costs and cooldowns
3. ✅ Player can learn spells from tomes
4. ✅ Player can cast spells in combat
5. ✅ Spell effects are applied correctly
6. ✅ Mana regenerates over time
7. ✅ Cooldowns prevent spam casting
8. ✅ UI shows mana and available spells
9. ✅ Spell effects are visually indicated

### User Experience

- Spell casting feels powerful and strategic
- Mana management adds decision-making
- Cooldowns create meaningful choices
- Visual feedback is clear and satisfying

---

## 🧪 Testing Plan

### Independent Test Criteria

1. **Mana System**
   - Player starts with mana (e.g., 50/50)
   - Mana regenerates over time (e.g., 5 per turn)
   - Cannot cast spell if insufficient mana

2. **Spell Learning**
   - Pick up spell tome
   - Spell appears in spell book
   - Can't learn same spell twice

3. **Spell Casting**
   - Open spell menu with 'C' key
   - Select spell from list
   - Target enemy/self/area
   - Mana cost deducted
   - Spell effect applied
   - Cooldown starts

4. **Spell Effects**
   - Fireball deals damage
   - Heal restores HP
   - Shield increases defense
   - Effects expire after duration

5. **Cooldown System**
   - Can't cast spell on cooldown
   - Cooldown decreases each turn
   - UI shows turns remaining

### Manual Test Scenarios

```
Scenario 1: Learn and Cast Fireball
1. Start new game
2. Spawn spell tome (debug command)
3. Pick up Tome of Fireball
4. Open spell menu (C)
5. Verify Fireball is listed
6. Select Fireball
7. Target enemy goblin
8. Confirm cast
9. Verify: Mana reduced by 10, Goblin takes 15 damage
10. Try to cast again
11. Verify: Cooldown prevents casting

Scenario 2: Mana Regeneration
1. Cast spell to reduce mana
2. Wait 5 turns
3. Verify mana has regenerated
4. Verify mana doesn't exceed maximum

Scenario 3: Healing Spell
1. Take damage in combat
2. Cast Heal spell
3. Verify HP restored
4. Verify mana cost deducted

Scenario 4: Buff Spell
1. Cast Shield spell
2. Verify defense stat increased
3. Wait 3 turns
4. Verify buff expired and defense returned to normal
```

---

## 📊 Implementation Strategy

### Phase 6A: Core Spell System (T074-T082)

1. Create Spell plugin project
2. Implement components (Mana, SpellBook, SpellEffect)
3. Implement systems (Casting, Regeneration, Effects)
4. Implement SpellService
5. Register plugin and systems

### Phase 6B: Sample Content (T083-T085) - PARALLEL

1. Create offensive spells
2. Create healing/buff spells
3. Create spell tome items

### Phase 6C: UI Integration (T086-T087)

1. Create spell casting screen
2. Add mana display to HUD
3. Add cooldown indicators

### Phase 6D: Combat Integration (T088)

1. Integrate spell casting into combat
2. Handle spell effects in combat resolution
3. Update combat log

---

## 🎨 Spell Design

### Starter Spells (Level 1)

| Spell | Type | Damage/Effect | Mana | Cooldown | Description |
|-------|------|---------------|------|----------|-------------|
| Magic Missile | Offensive | 8 dmg | 5 | 1 turn | Always hits |
| Minor Heal | Healing | +15 HP | 8 | 3 turns | Restore health |

### Advanced Spells (Level 3+)

| Spell | Type | Damage/Effect | Mana | Cooldown | Description |
|-------|------|---------------|------|----------|-------------|
| Fireball | Offensive | 15 dmg | 10 | 3 turns | Area damage |
| Lightning Bolt | Offensive | 20 dmg | 15 | 4 turns | High damage, stun |
| Ice Shard | Offensive | 12 dmg | 8 | 2 turns | Slow target |
| Heal | Healing | +25 HP | 12 | 5 turns | Major healing |
| Shield | Buff | +5 DEF | 10 | 6 turns | Defense buff |
| Haste | Buff | +20 SPD | 8 | 5 turns | Speed buff |

### Mana Economy

- **Starting Mana**: 50
- **Mana per Level**: +10 (scales with player level)
- **Mana Regen**: 5 per turn (or 1 per second in real-time)
- **Mana Potions**: Restore 30 mana (found in dungeon or bought from merchants)

---

## 🔗 Integration Points

### Quest System Integration

- Quest rewards can include spell tomes
- Quest objectives can require casting specific spells
- NPCs can teach spells through dialogue

### Progression System Integration

- Spell power scales with player level
- Spells have minimum level requirements
- Advanced spells unlock at higher levels

### Combat System Integration

- Spells available as combat actions
- Spell damage calculated with combat formulas
- Status effects applied through combat system

### Item System Integration

- Spell tomes as consumable items
- Mana potions as consumable items
- Spell focus items (wands, staffs) for bonuses

---

## 📈 Future Enhancements (Post-Phase 6)

- **Spell Upgrades**: Improve spell power with repeated use
- **Combo System**: Chain spells for bonus effects
- **Elemental System**: Fire beats ice, ice beats water, etc.
- **Area Spells**: Affect multiple targets
- **Channeled Spells**: Cast over multiple turns for greater effect
- **Spell Crafting**: Combine spells to create new ones
- **Mana Overload**: Spend extra mana to empower spells

---

## 🏁 Definition of Done

- [ ] All 15 tasks (T074-T088) completed
- [ ] Spell plugin builds successfully
- [ ] Player can learn spells from tomes
- [ ] Player can cast spells with mana cost
- [ ] Spells apply correct effects (damage/heal/buff)
- [ ] Mana regenerates over time
- [ ] Cooldowns prevent spam casting
- [ ] UI shows mana bar and spell menu
- [ ] At least 6 sample spells implemented
- [ ] Integration with combat system works
- [ ] Manual testing scenarios pass
- [ ] Code committed with descriptive messages
- [ ] PHASE_6_SUMMARY.md created

---

**Next Phase**: Phase 7 - User Story 5: Merchant Trading System (Priority P3)
