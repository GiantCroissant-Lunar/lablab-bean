# Tasks: Core Gameplay Systems

**Input**: Design documents from `/specs/015-gameplay-systems/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md, quickstart.md

**Tests**: Tests are NOT explicitly requested in the spec - focusing on implementation tasks only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md structure:

- **Plugins**: `dotnet/plugins/LablabBean.Plugins.{SystemName}/`
- **Framework**: `dotnet/framework/LablabBean.Game.Core/`
- **Tests**: `dotnet/tests/LablabBean.Plugins.{SystemName}.Tests/`

---

## Phase 1: Setup (Shared Infrastructure) ✅ COMPLETE

**Purpose**: Project initialization and basic structure for 7 new plugins

- [x] T001 Create plugin project structure for LablabBean.Plugins.Quest in dotnet/plugins/
- [x] T002 Create plugin project structure for LablabBean.Plugins.NPC in dotnet/plugins/
- [x] T003 Create plugin project structure for LablabBean.Plugins.Progression in dotnet/plugins/
- [x] T004 Create plugin project structure for LablabBean.Plugins.Spells in dotnet/plugins/
- [x] T005 Create plugin project structure for LablabBean.Plugins.Merchant in dotnet/plugins/
- [x] T006 Create plugin project structure for LablabBean.Plugins.Boss in dotnet/plugins/
- [x] T007 Create plugin project structure for LablabBean.Plugins.Hazards in dotnet/plugins/
- [x] T008 [P] Add package references (Arch, Arch.System, GoRogue) to all plugin projects
- [x] T009 [P] Create plugin manifest files (plugin.json) for all 7 plugins
- [x] T010 [P] Setup test projects for all 7 plugins in dotnet/tests/
- [x] T011 [P] Add XML documentation configuration to all plugin .csproj files

---

## Phase 2: Foundational (Blocking Prerequisites) ✅ COMPLETE

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T012 Create event system events (QuestCompletedEvent, PlayerLevelUpEvent, NPCInteractionEvent, etc.) in dotnet/framework/LablabBean.Game.Core/Events/
- [x] T013 [P] Create IQuestService contract in dotnet/specs/015-gameplay-systems/contracts/IQuestService.cs
- [x] T014 [P] Create INPCService contract in dotnet/specs/015-gameplay-systems/contracts/INPCService.cs
- [x] T015 [P] Create IProgressionService contract in dotnet/specs/015-gameplay-systems/contracts/IProgressionService.cs
- [x] T016 [P] Create ISpellService contract in dotnet/specs/015-gameplay-systems/contracts/ISpellService.cs
- [x] T017 [P] Create IMerchantService contract in dotnet/specs/015-gameplay-systems/contracts/IMerchantService.cs
- [x] T018 [P] Create IBossService contract in dotnet/specs/015-gameplay-systems/contracts/IBossService.cs
- [x] T019 [P] Create IHazardService contract in dotnet/specs/015-gameplay-systems/contracts/IHazardService.cs
- [x] T020 Setup JSON data directories for quests, dialogue, spells, merchants, bosses in dotnet/plugins/{PluginName}/Data/
- [x] T021 [P] Create shared utility classes for skill checks and dice rolls in dotnet/framework/LablabBean.Game.Core/Utilities/
- [x] T022 [P] Enhance existing Gold component for merchant system (if not exists) in dotnet/framework/LablabBean.Game.Core/Components/Gold.cs

**Checkpoint**: ✅ Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Quest-Driven Exploration (Priority: P1) 🎯 MVP

**Goal**: Players can accept quests from NPCs, track objectives, and receive rewards upon completion

**Independent Test**: Create one NPC with one quest ("retrieve artifact from level 5"), accept it, complete objectives, return to NPC, and receive rewards (gold, XP, item)

### Implementation for User Story 1

#### Quest Components

- [ ] T023 [P] [US1] Create Quest component in dotnet/plugins/LablabBean.Plugins.Quest/Components/Quest.cs
- [ ] T024 [P] [US1] Create QuestObjective component in dotnet/plugins/LablabBean.Plugins.Quest/Components/QuestObjective.cs
- [ ] T025 [P] [US1] Create QuestLog component in dotnet/plugins/LablabBean.Plugins.Quest/Components/QuestLog.cs
- [ ] T026 [P] [US1] Create QuestRewards struct in dotnet/plugins/LablabBean.Plugins.Quest/Components/QuestRewards.cs
- [ ] T027 [P] [US1] Create QuestPrerequisites struct in dotnet/plugins/LablabBean.Plugins.Quest/Components/QuestPrerequisites.cs

#### NPC Components (needed for quest-givers)

- [ ] T028 [P] [US1] Create NPC component in dotnet/plugins/LablabBean.Plugins.NPC/Components/NPC.cs
- [ ] T029 [P] [US1] Create DialogueState component in dotnet/plugins/LablabBean.Plugins.NPC/Components/DialogueState.cs

#### Quest Systems

- [ ] T030 [US1] Implement QuestSystem (processes quest entities) in dotnet/plugins/LablabBean.Plugins.Quest/Systems/QuestSystem.cs
- [ ] T031 [US1] Implement QuestProgressSystem (event-driven objective tracking) in dotnet/plugins/LablabBean.Plugins.Quest/Systems/QuestProgressSystem.cs
- [ ] T032 [US1] Implement QuestRewardSystem (grants rewards on completion) in dotnet/plugins/LablabBean.Plugins.Quest/Systems/QuestRewardSystem.cs

#### Quest Service

- [ ] T033 [US1] Implement QuestService (API for quest management) in dotnet/plugins/LablabBean.Plugins.Quest/Services/QuestService.cs

#### NPC Interaction (minimal for quest-giving)

- [ ] T034 [US1] Implement basic NPCSystem (spawning, interaction checks) in dotnet/plugins/LablabBean.Plugins.NPC/Systems/NPCSystem.cs
- [ ] T035 [US1] Implement basic DialogueSystem (quest acceptance only) in dotnet/plugins/LablabBean.Plugins.NPC/Systems/DialogueSystem.cs
- [ ] T036 [US1] Implement minimal NPCService (StartDialogue, quest actions) in dotnet/plugins/LablabBean.Plugins.NPC/Services/NPCService.cs

#### Plugin Integration

- [ ] T037 [US1] Implement Quest plugin main class (Plugin.cs) with service registration in dotnet/plugins/LablabBean.Plugins.Quest/Plugin.cs
- [ ] T038 [US1] Implement NPC plugin main class (Plugin.cs) with service registration in dotnet/plugins/LablabBean.Plugins.NPC/Plugin.cs

#### Data and Configuration

- [ ] T039 [P] [US1] Create sample quest JSON (retrieve-artifact.json) in dotnet/plugins/LablabBean.Plugins.Quest/Data/Quests/
- [ ] T040 [P] [US1] Create sample NPC JSON (eldrin-questgiver.json) with minimal dialogue in dotnet/plugins/LablabBean.Plugins.NPC/Data/NPCs/
- [ ] T041 [P] [US1] Create sample dialogue tree JSON (eldrin-dialogue.json) in dotnet/plugins/LablabBean.Plugins.NPC/Data/Dialogue/

#### Event Integration

- [ ] T042 [US1] Subscribe QuestProgressSystem to game events (OnEnemyKilled, OnItemCollected, OnLocationReached) in QuestProgressSystem.cs
- [ ] T043 [US1] Add quest reward event publishing (OnQuestCompleted) in QuestRewardSystem.cs

#### UI Integration

- [ ] T044 [US1] Add quest log UI screen in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/QuestLogScreen.cs
- [ ] T045 [US1] Add NPC interaction prompt in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/DialogueScreen.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - player can accept quest, track progress, complete objectives, and receive rewards

---

## Phase 4: User Story 3 - NPC Interactions and Dialogue (Priority: P1)

**Goal**: Players can interact with NPCs, navigate branching dialogue trees, and trigger various actions (quests, trades, lore)

**Independent Test**: Place one NPC with a multi-branch dialogue tree, interact, select different choices, and verify branching works correctly

**Note**: US3 is implemented before US2 because it's also P1 and completes the NPC/dialogue system started in US1

### Implementation for User Story 3

#### Dialogue Components

- [ ] T046 [P] [US3] Create DialogueTree data class in dotnet/plugins/LablabBean.Plugins.NPC/Data/DialogueTree.cs
- [ ] T047 [P] [US3] Create DialogueNode data class in dotnet/plugins/LablabBean.Plugins.NPC/Data/DialogueNode.cs
- [ ] T048 [P] [US3] Create DialogueChoice data class in dotnet/plugins/LablabBean.Plugins.NPC/Data/DialogueChoice.cs
- [ ] T049 [P] [US3] Create DialogueAction data class in dotnet/plugins/LablabBean.Plugins.NPC/Data/DialogueAction.cs

#### Enhanced Dialogue System

- [ ] T050 [US3] Enhance DialogueSystem with condition evaluation (level checks, item checks, quest checks) in dotnet/plugins/LablabBean.Plugins.NPC/Systems/DialogueSystem.cs
- [ ] T051 [US3] Implement dialogue action execution (AcceptQuest, CompleteQuest, OpenTrade, SetNPCState) in DialogueSystem.cs
- [ ] T052 [US3] Add NPC state persistence to DialogueSystem (remembers previous choices) in DialogueSystem.cs

#### Enhanced NPC Service

- [ ] T053 [US3] Enhance NPCService with full dialogue tree navigation (SelectChoice, EndDialogue) in dotnet/plugins/LablabBean.Plugins.NPC/Services/NPCService.cs
- [ ] T054 [US3] Implement NPC state management (SetNPCState, GetNPCState, HasNPCState) in NPCService.cs
- [ ] T055 [US3] Add dialogue condition parser (simple DSL: "level >= 5 AND hasItem('key')") in NPCService.cs

#### Data and Configuration

- [ ] T056 [P] [US3] Create complex dialogue tree samples (multi-branch, conditional) in dotnet/plugins/LablabBean.Plugins.NPC/Data/Dialogue/
- [ ] T057 [P] [US3] Create lore NPC samples in dotnet/plugins/LablabBean.Plugins.NPC/Data/NPCs/
- [ ] T058 [P] [US3] Create merchant dialogue templates in dotnet/plugins/LablabBean.Plugins.NPC/Data/Dialogue/

#### UI Enhancements

- [ ] T059 [US3] Enhance DialogueScreen with choice filtering (hide unavailable choices) in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/DialogueScreen.cs
- [ ] T060 [US3] Add dialogue history display in DialogueScreen.cs
- [ ] T061 [US3] Add NPC type indicators (quest, merchant, lore) in game UI

**Checkpoint**: At this point, User Stories 1 AND 3 should both work independently - complete quest + dialogue systems

---

## Phase 5: User Story 2 - Character Growth Through Leveling (Priority: P2)

**Goal**: Players gain XP from combat/quests, level up, receive stat increases, and see progression clearly displayed

**Independent Test**: Award XP manually, trigger level-up, verify stat increases applied, and UI shows current level and XP progress

### Implementation for User Story 2

#### Progression Components

- [ ] T062 [P] [US2] Create Experience component in dotnet/plugins/LablabBean.Plugins.Progression/Components/Experience.cs
- [ ] T063 [P] [US2] Create LevelUpStats struct in dotnet/plugins/LablabBean.Plugins.Progression/Components/LevelUpStats.cs

#### Progression Systems

- [ ] T064 [US2] Implement ExperienceSystem (tracks XP, detects level-up threshold) in dotnet/plugins/LablabBean.Plugins.Progression/Systems/ExperienceSystem.cs
- [ ] T065 [US2] Implement LevelingSystem (applies stat increases, publishes OnPlayerLevelUp event) in dotnet/plugins/LablabBean.Plugins.Progression/Systems/LevelingSystem.cs

#### Progression Service

- [ ] T066 [US2] Implement ProgressionService (AwardExperience, GetExperience, CalculateXPRequired) in dotnet/plugins/LablabBean.Plugins.Progression/Services/ProgressionService.cs

#### Plugin Integration

- [ ] T067 [US2] Implement Progression plugin main class with service registration in dotnet/plugins/LablabBean.Plugins.Progression/Plugin.cs

#### XP Awarding Integration

- [ ] T068 [US2] Integrate XP awards into CombatSystem (OnEnemyKilled) in dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs
- [ ] T069 [US2] Integrate XP awards into QuestRewardSystem (quest completion) in dotnet/plugins/LablabBean.Plugins.Quest/Systems/QuestRewardSystem.cs

#### Stat Application

- [ ] T070 [US2] Subscribe to OnPlayerLevelUp event to apply stat increases (Health, Attack, Defense) in dotnet/framework/LablabBean.Game.Core/Systems/StatSystem.cs (create if needed)

#### UI Integration

- [ ] T071 [US2] Add character screen showing level, XP, and stats in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/CharacterScreen.cs
- [ ] T072 [US2] Add level-up notification popup in dotnet/console-app/LablabBean.Game.TerminalUI/UI/LevelUpNotification.cs
- [ ] T073 [US2] Add XP bar to main HUD in dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Combat Spells and Abilities (Priority: P2)

**Goal**: Players can learn spells, cast them using mana, and see spell effects (damage, buffs, healing)

**Independent Test**: Implement one spell (fireball), give player mana, cast spell, verify mana consumption and damage dealt

### Implementation for User Story 4

#### Spell Components

- [ ] T074 [P] [US4] Create Mana component in dotnet/plugins/LablabBean.Plugins.Spells/Components/Mana.cs
- [ ] T075 [P] [US4] Create SpellBook component in dotnet/plugins/LablabBean.Plugins.Spells/Components/SpellBook.cs
- [ ] T076 [P] [US4] Create SpellCooldown component in dotnet/plugins/LablabBean.Plugins.Spells/Components/SpellCooldown.cs

#### Spell Data Structures

- [ ] T077 [P] [US4] Create Spell data class in dotnet/plugins/LablabBean.Plugins.Spells/Data/Spell.cs
- [ ] T078 [P] [US4] Create SpellEffect data class in dotnet/plugins/LablabBean.Plugins.Spells/Data/SpellEffect.cs

#### Spell Systems

- [ ] T079 [US4] Implement ManaSystem (regeneration, consumption) in dotnet/plugins/LablabBean.Plugins.Spells/Systems/ManaSystem.cs
- [ ] T080 [US4] Implement SpellCastingSystem (validates mana, cooldowns, targeting) in dotnet/plugins/LablabBean.Plugins.Spells/Systems/SpellCastingSystem.cs
- [ ] T081 [US4] Implement SpellEffectSystem (applies damage, buffs, debuffs, healing) in dotnet/plugins/LablabBean.Plugins.Spells/Systems/SpellEffectSystem.cs
- [ ] T082 [US4] Implement SpellCooldownSystem (tracks and decrements cooldowns) in dotnet/plugins/LablabBean.Plugins.Spells/Systems/SpellCooldownSystem.cs

#### Spell Service

- [ ] T083 [US4] Implement SpellService (CastSpell, LearnSpell, EquipSpell, GetMana) in dotnet/plugins/LablabBean.Plugins.Spells/Services/SpellService.cs

#### Plugin Integration

- [ ] T084 [US4] Implement Spells plugin main class with service registration in dotnet/plugins/LablabBean.Plugins.Spells/Plugin.cs

#### Spell Integration with Existing Systems

- [ ] T085 [US4] Integrate spell damage with CombatSystem in dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs
- [ ] T086 [US4] Integrate spell buffs/debuffs with StatusEffectSystem (reuse existing) in dotnet/plugins/LablabBean.Plugins.StatusEffects/
- [ ] T087 [US4] Add spell learning on level-up in dotnet/plugins/LablabBean.Plugins.Progression/Systems/LevelingSystem.cs

#### Data and Configuration

- [ ] T088 [P] [US4] Create spell definitions JSON (fireball, heal, shield) in dotnet/plugins/LablabBean.Plugins.Spells/Data/Spells/
- [ ] T089 [P] [US4] Create spell unlock table (level requirements) in dotnet/plugins/LablabBean.Plugins.Spells/Data/SpellUnlocks.json

#### UI Integration

- [ ] T090 [US4] Add spellbook screen in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/SpellbookScreen.cs
- [ ] T091 [US4] Add mana bar to HUD in dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs
- [ ] T092 [US4] Add spell casting UI (target selection) in dotnet/console-app/LablabBean.Game.TerminalUI/UI/SpellCastingUI.cs

**Checkpoint**: User Stories 1-4 should all work independently

---

## Phase 7: User Story 5 - Merchant Trading System (Priority: P3)

**Goal**: Players can buy/sell items from merchant NPCs using gold currency

**Independent Test**: Create one merchant NPC with fixed inventory, buy an item (gold deducted), sell an item (gold added)

### Implementation for User Story 5

#### Merchant Components

- [ ] T093 [P] [US5] Enhance Gold component (if needed) in dotnet/framework/LablabBean.Game.Core/Components/Gold.cs
- [ ] T094 [P] [US5] Create MerchantInventory component in dotnet/plugins/LablabBean.Plugins.Merchant/Components/MerchantInventory.cs
- [ ] T095 [P] [US5] Create TradeState component in dotnet/plugins/LablabBean.Plugins.Merchant/Components/TradeState.cs

#### Merchant Data Structures

- [ ] T096 [P] [US5] Create MerchantItem data class in dotnet/plugins/LablabBean.Plugins.Merchant/Data/MerchantItem.cs

#### Merchant Systems

- [ ] T097 [US5] Implement TradingSystem (buy/sell logic, gold transactions) in dotnet/plugins/LablabBean.Plugins.Merchant/Systems/TradingSystem.cs
- [ ] T098 [US5] Implement MerchantSystem (stock management, refresh) in dotnet/plugins/LablabBean.Plugins.Merchant/Systems/MerchantSystem.cs

#### Merchant Service

- [ ] T099 [US5] Implement MerchantService (StartTrade, BuyItem, SellItem, GetGold) in dotnet/plugins/LablabBean.Plugins.Merchant/Services/MerchantService.cs

#### Plugin Integration

- [ ] T100 [US5] Implement Merchant plugin main class with service registration in dotnet/plugins/LablabBean.Plugins.Merchant/Plugin.cs

#### Integration with Inventory

- [ ] T101 [US5] Integrate with existing Inventory plugin (add/remove items during trade) in dotnet/plugins/LablabBean.Plugins.Inventory/

#### Integration with NPC

- [ ] T102 [US5] Add merchant dialogue actions (OpenTrade) to DialogueSystem in dotnet/plugins/LablabBean.Plugins.NPC/Systems/DialogueSystem.cs

#### Data and Configuration

- [ ] T103 [P] [US5] Create merchant inventory JSON files (blacksmith, general store) in dotnet/plugins/LablabBean.Plugins.Merchant/Data/Merchants/
- [ ] T104 [P] [US5] Create gold loot drop configurations in dotnet/framework/LablabBean.Game.Core/Data/

#### UI Integration

- [ ] T105 [US5] Add merchant trade screen in dotnet/console-app/LablabBean.Game.TerminalUI/Screens/TradeScreen.cs
- [ ] T106 [US5] Add gold display to HUD in dotnet/console-app/LablabBean.Game.TerminalUI/UI/HUD.cs

**Checkpoint**: User Stories 1-5 should all work independently

---

## Phase 8: User Story 6 - Boss Encounters (Priority: P3)

**Goal**: Players encounter powerful bosses on designated levels with unique mechanics and guaranteed loot

**Independent Test**: Spawn one boss on level 5, engage in combat, trigger phase transition at 50% health, defeat boss, verify loot drops

### Implementation for User Story 6

#### Boss Components

- [ ] T107 [P] [US6] Create Boss component in dotnet/plugins/LablabBean.Plugins.Boss/Components/Boss.cs
- [ ] T108 [P] [US6] Create BossPhase data class in dotnet/plugins/LablabBean.Plugins.Boss/Data/BossPhase.cs
- [ ] T109 [P] [US6] Create BossAbility data class in dotnet/plugins/LablabBean.Plugins.Boss/Data/BossAbility.cs

#### Boss Systems

- [ ] T110 [US6] Implement BossSystem (phase transitions, defeat tracking) in dotnet/plugins/LablabBean.Plugins.Boss/Systems/BossSystem.cs
- [ ] T111 [US6] Implement BossAISystem (special abilities, phase-based behavior) in dotnet/plugins/LablabBean.Plugins.Boss/Systems/BossAISystem.cs
- [ ] T112 [US6] Implement BossAbilitySystem (executes boss special attacks) in dotnet/plugins/LablabBean.Plugins.Boss/Systems/BossAbilitySystem.cs

#### Boss Service

- [ ] T113 [US6] Implement BossService (SpawnBoss, CheckPhaseTransition, TryUseAbility) in dotnet/plugins/LablabBean.Plugins.Boss/Services/BossService.cs

#### Plugin Integration

- [ ] T114 [US6] Implement Boss plugin main class with service registration in dotnet/plugins/LablabBean.Plugins.Boss/Plugin.cs

#### Boss Integration with Game Systems

- [ ] T115 [US6] Integrate boss spawning into LevelManager (spawn on levels 5, 10, 15, 20) in dotnet/framework/LablabBean.Game.Core/Maps/LevelManager.cs
- [ ] T116 [US6] Integrate phase checks with CombatSystem (trigger on health change) in dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs
- [ ] T117 [US6] Enhance MapGenerator to create boss rooms (larger, no other enemies) in dotnet/framework/LablabBean.Game.Core/Maps/MapGenerator.cs

#### Data and Configuration

- [ ] T118 [P] [US6] Create boss definition JSON files (dragon-lord, lich-king) in dotnet/plugins/LablabBean.Plugins.Boss/Data/Bosses/
- [ ] T119 [P] [US6] Define boss loot tables in dotnet/plugins/LablabBean.Plugins.Boss/Data/BossLoot.json

#### UI Integration

- [ ] T120 [US6] Add boss health bar display in dotnet/console-app/LablabBean.Game.TerminalUI/UI/BossHealthBar.cs
- [ ] T121 [US6] Add phase transition notification in dotnet/console-app/LablabBean.Game.TerminalUI/UI/PhaseTransitionNotification.cs

**Checkpoint**: User Stories 1-6 should all work independently

---

## Phase 9: User Story 7 - Environmental Hazards and Traps (Priority: P4)

**Goal**: Players encounter traps and environmental hazards that can be detected, disarmed, or triggered

**Independent Test**: Place one spike trap on a tile, walk onto it (triggers), reload level, detect trap with high perception, disarm successfully

### Implementation for User Story 7

#### Hazard Components

- [ ] T122 [P] [US7] Create Trap component in dotnet/plugins/LablabBean.Plugins.Hazards/Components/Trap.cs
- [ ] T123 [P] [US7] Create EnvironmentalHazard component in dotnet/plugins/LablabBean.Plugins.Hazards/Components/EnvironmentalHazard.cs
- [ ] T124 [P] [US7] Create TrapEffect data class in dotnet/plugins/LablabBean.Plugins.Hazards/Data/TrapEffect.cs

#### Hazard Systems

- [ ] T125 [US7] Implement TrapSystem (detection, disarming, triggering) in dotnet/plugins/LablabBean.Plugins.Hazards/Systems/TrapSystem.cs
- [ ] T126 [US7] Implement HazardSystem (applies damage per turn) in dotnet/plugins/LablabBean.Plugins.Hazards/Systems/HazardSystem.cs
- [ ] T127 [US7] Implement TrapDetectionSystem (perception-based checks on movement) in dotnet/plugins/LablabBean.Plugins.Hazards/Systems/TrapDetectionSystem.cs

#### Hazard Service

- [ ] T128 [US7] Implement HazardService (PlaceTrap, DetectTrap, DisarmTrap, PlaceHazard) in dotnet/plugins/LablabBean.Plugins.Hazards/Services/HazardService.cs

#### Plugin Integration

- [ ] T129 [US7] Implement Hazards plugin main class with service registration in dotnet/plugins/LablabBean.Plugins.Hazards/Plugin.cs

#### Hazard Integration with Game Systems

- [ ] T130 [US7] Integrate trap triggering with MovementSystem in dotnet/framework/LablabBean.Game.Core/Systems/MovementSystem.cs
- [ ] T131 [US7] Integrate hazard damage with CombatSystem in dotnet/framework/LablabBean.Game.Core/Systems/CombatSystem.cs
- [ ] T132 [US7] Enhance MapGenerator to place traps during level generation in dotnet/framework/LablabBean.Game.Core/Maps/MapGenerator.cs

#### Trap Integration with StatusEffects

- [ ] T133 [US7] Integrate poison gas traps with StatusEffectSystem (apply Poison status) in dotnet/plugins/LablabBean.Plugins.StatusEffects/

#### Data and Configuration

- [ ] T134 [P] [US7] Create trap placement configurations (density, difficulty) in dotnet/plugins/LablabBean.Plugins.Hazards/Data/TrapConfig.json

#### UI Integration

- [ ] T135 [US7] Add trap state visual indicators (hidden, detected, triggered, disarmed) in dotnet/console-app/LablabBean.Game.TerminalUI/Rendering/TrapRenderer.cs
- [ ] T136 [US7] Add trap detection notification in dotnet/console-app/LablabBean.Game.TerminalUI/UI/TrapDetectionNotification.cs

**Checkpoint**: All 7 user stories should now be independently functional

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T137 [P] Add XML documentation to all public service APIs across all 7 plugins
- [ ] T138 [P] Create README.md for each plugin (Quest, NPC, Progression, Spells, Merchant, Boss, Hazards) in dotnet/plugins/
- [ ] T139 [P] Add logging to all service methods using Microsoft.Extensions.Logging
- [ ] T140 Code cleanup and refactoring across all plugins
- [ ] T141 [P] Optimize ECS queries across all systems (review for N² operations)
- [ ] T142 [P] Add persistence support for all new components to JSON save system in dotnet/plugins/LablabBean.Plugins.PersistentStorage.Json/
- [ ] T143 [P] Create integration tests for cross-plugin interactions (Quest + NPC, Spell + StatusEffect, etc.) in dotnet/tests/Integration/
- [ ] T144 Performance profiling across all 7 systems (ensure <50ms turn processing)
- [ ] T145 [P] Update main game README with new gameplay features in dotnet/README.md
- [ ] T146 Validate against quickstart.md examples (run all code samples)
- [ ] T147 [P] Create data migration scripts for existing save files (add new components)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - **US1 (Quest, P1)**: Can start after Foundational - includes minimal NPC for quest-giving
  - **US3 (NPC, P1)**: Can start after Foundational - enhances NPC system from US1
  - **US2 (Progression, P2)**: Can start after Foundational - independent of US1/US3
  - **US4 (Spells, P2)**: Can start after Foundational - integrates with US2 (spell unlocks on level-up)
  - **US5 (Merchant, P3)**: Depends on US3 completion (needs full NPC dialogue for OpenTrade action)
  - **US6 (Boss, P3)**: Can start after Foundational - independent
  - **US7 (Hazards, P4)**: Can start after Foundational - independent
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies - foundational quest + minimal NPC
- **User Story 3 (P1)**: No hard dependency on US1, but enhances NPC system started there
- **User Story 2 (P2)**: No dependencies - independent progression system
- **User Story 4 (P2)**: Soft dependency on US2 (spell unlocks on level-up) - can work without it
- **User Story 5 (P3)**: Depends on US3 (needs dialogue actions for merchant)
- **User Story 6 (P3)**: No dependencies - independent boss system
- **User Story 7 (P4)**: No dependencies - independent hazard system

### Within Each User Story

- Components before systems (systems reference components)
- Systems before services (services call systems)
- Plugin main class after services (registers services)
- Data files can be created in parallel with components
- UI integration after core implementation

### Parallel Opportunities

**Setup Phase (Phase 1)**: All tasks T001-T011 can run in parallel (different projects/files)

**Foundational Phase (Phase 2)**: Tasks T013-T019 (service contracts) and T020-T022 can run in parallel

**User Story Phases**: After Foundational completes, these can proceed in parallel:

- US1 + US3 (together, as US3 enhances US1's NPC system)
- US2 (fully parallel with US1/US3)
- US4 (parallel if US2 spell unlock integration deferred)
- US6 (fully parallel with all others)
- US7 (fully parallel with all others)

**Component Creation**: Within each story, all [P] component tasks can run in parallel

**Data Files**: All [P] data/config file creation can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 components together:
Task T023: "Create Quest component"
Task T024: "Create QuestObjective component"
Task T025: "Create QuestLog component"
Task T026: "Create QuestRewards struct"
Task T027: "Create QuestPrerequisites struct"
Task T028: "Create NPC component"
Task T029: "Create DialogueState component"

# Then launch all US1 data files together:
Task T039: "Create sample quest JSON (retrieve-artifact.json)"
Task T040: "Create sample NPC JSON (eldrin-questgiver.json)"
Task T041: "Create sample dialogue tree JSON (eldrin-dialogue.json)"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 3 Only - Complete Quest & NPC)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Quest-Driven Exploration)
4. Complete Phase 4: User Story 3 (NPC Interactions and Dialogue)
5. **STOP and VALIDATE**: Test quest acceptance, dialogue branching, quest completion
6. Deploy/demo if ready

**MVP Value**: Players can receive quests from NPCs, complete objectives, and interact with rich dialogue

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 + US3 → Test quest + dialogue → Deploy/Demo (MVP!)
3. Add US2 → Test progression → Deploy/Demo (now with leveling)
4. Add US4 → Test spells → Deploy/Demo (now with magic combat)
5. Add US5 → Test trading → Deploy/Demo (now with economy)
6. Add US6 → Test bosses → Deploy/Demo (now with boss fights)
7. Add US7 → Test hazards → Deploy/Demo (complete gameplay)
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With 3 developers after Foundational phase completes:

**Sprint 1** (P1 stories):

- Developer A: User Story 1 (Quest)
- Developer B: User Story 3 (NPC/Dialogue)
- Developer C: User Story 2 (Progression)

**Sprint 2** (P2 + P3 stories):

- Developer A: User Story 4 (Spells)
- Developer B: User Story 5 (Merchant) - needs US3 done
- Developer C: User Story 6 (Boss)

**Sprint 3** (P4 + Polish):

- Developer A: User Story 7 (Hazards)
- Developers B & C: Polish & integration testing

---

## Notes

- **[P] tasks**: Different files, no dependencies - safe to parallelize
- **[Story] label**: Maps task to specific user story (US1-US7) for traceability
- **Each user story**: Independently completable and testable
- **Component IDs**: Use Guid for all entity/component references (Quest.Id, NPC.Id, Spell.Id)
- **Event system**: Heavy use of events for decoupling (OnQuestComplete, OnLevelUp, OnSpellCast)
- **JSON data**: All configuration externalized (quests, dialogue, spells, merchants, bosses)
- **Plugin pattern**: Each system follows existing Inventory/StatusEffects plugin structure
- **Commit strategy**: Commit after each task or logical group (per user story phase)
- **Checkpoints**: Stop at each phase checkpoint to validate story independently
- **Testing strategy**: Manual testing per user story + integration tests in Phase 10
- **Avoid**: Cross-story dependencies that break independence, shared file conflicts, monolithic components

---

**Total Tasks**: 147
**User Story Breakdown**:

- Setup: 11 tasks
- Foundational: 11 tasks
- US1 (Quest + minimal NPC): 23 tasks
- US3 (Full NPC/Dialogue): 16 tasks
- US2 (Progression): 12 tasks
- US4 (Spells): 19 tasks
- US5 (Merchant): 14 tasks
- US6 (Boss): 15 tasks
- US7 (Hazards): 15 tasks
- Polish: 11 tasks

**Parallel Opportunities**: 60+ tasks marked [P] across all phases

**MVP Scope**: Phases 1-4 (US1 + US3) = 61 tasks for complete quest & NPC system

**Independent Testing**: Each user story has clear test criteria and can be validated independently
