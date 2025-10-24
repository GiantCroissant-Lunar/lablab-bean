# Implementation Plan: Core Gameplay Systems

**Branch**: `015-gameplay-systems` | **Date**: 2025-10-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/015-gameplay-systems/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This implementation adds seven critical gameplay systems to the Lablab-Bean dungeon crawler: Quest System with objectives and rewards, NPC/Dialogue System with branching conversations, Character Progression with experience and leveling, Spell/Ability System with mana-based magic, Merchant Trading System with gold economy, Boss Encounters with unique mechanics, and Environmental Hazards/Traps. These systems build on the existing ECS architecture (Arch.Extended) and plugin framework to create a complete RPG gameplay experience.

## Technical Context

**Language/Version**: C# (latest), .NET 8.0
**Primary Dependencies**: Arch (ECS), Arch.System, GoRogue (roguelike framework), TheSadRogue.Primitives, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging
**Storage**: JSON file-based persistence via LablabBean.Plugins.PersistentStorage.Json for game saves, level caching, quest state, NPC dialogue state
**Testing**: xUnit (existing project standard), integration tests for plugin contracts, ECS system tests
**Target Platform**: Windows/Linux/macOS desktop (cross-platform .NET), Terminal UI (Terminal.Gui) and Windows UI (SadConsole)
**Project Type**: Desktop game application with modular plugin architecture
**Performance Goals**: 60 FPS gameplay, <50ms turn processing, support 1000+ entities per level, instant UI responsiveness
**Constraints**: Turn-based gameplay (no real-time requirements), offline-only (no multiplayer), memory footprint <500MB, deterministic gameplay for save/load consistency
**Scale/Scope**: 7 new gameplay systems, ~50 new components, ~20 new ECS systems, support for 100+ concurrent quests, 200+ NPCs per game session, 20+ dungeon levels

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Project constitution file is a template. Using existing project conventions as guidance.

### Architecture Consistency

- ✅ **ECS Pattern**: All gameplay systems will use existing Arch ECS framework with Components and Systems
- ✅ **Plugin Architecture**: New systems (Quest, NPC, Progression, Spell, Merchant, Boss, Traps) will be implemented as plugins following existing patterns (Inventory, StatusEffects)
- ✅ **Service-Based Design**: Systems registered via DependencyInjection, accessed through IPluginContext
- ✅ **Event-Driven**: Use existing event system for inter-system communication (OnQuestComplete, OnLevelUp, OnNPCInteraction)

### Code Quality Gates

- ✅ **Component Separation**: Each gameplay system gets dedicated components (no monolithic components)
- ✅ **System Independence**: Systems operate independently with clear boundaries (Quest system doesn't directly reference Spell system)
- ✅ **Testability**: All systems will have unit tests for core logic, integration tests for plugin contracts
- ✅ **Documentation**: XML documentation for public APIs, README for each plugin

### Performance Gates

- ✅ **Turn-Based Processing**: No performance concerns for turn-based gameplay, systems process on-demand
- ✅ **ECS Queries**: Use Arch query system efficiently (avoid N² operations, use entity references)
- ✅ **Persistence**: Quest/NPC state persisted via existing JSON plugin, no new storage mechanism needed

### Complexity Justification

- ✅ **Plugin Count**: Adding 7 new plugins is justified as each represents an independent gameplay system with distinct responsibilities
- ✅ **Component Count**: ~50 new components across 7 systems averages ~7 components per system (reasonable for domain complexity)
- ✅ **No New Frameworks**: Reusing existing Arch ECS, GoRogue, DI patterns - no additional architectural complexity

**Status**: ✅ **PASSED** - All gates satisfied, proceeding to Phase 0 research

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/
├── framework/
│   ├── LablabBean.Game.Core/           # Existing game framework
│   │   ├── Components/                 # (Enhanced with new components)
│   │   ├── Systems/                    # (Enhanced with new systems)
│   │   └── Maps/                       # (Enhanced with boss/trap support)
│   ├── LablabBean.Plugins.Contracts/   # Existing plugin contracts
│   └── LablabBean.Plugins.Core/        # Existing plugin infrastructure
│
├── plugins/
│   ├── LablabBean.Plugins.Quest/       # NEW: Quest system plugin
│   │   ├── Components/                 # Quest, QuestObjective, QuestLog
│   │   ├── Systems/                    # QuestSystem, QuestProgressSystem
│   │   ├── Services/                   # QuestService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.NPC/         # NEW: NPC & Dialogue plugin
│   │   ├── Components/                 # NPC, DialogueTree, DialogueState
│   │   ├── Systems/                    # NPCSystem, DialogueSystem
│   │   ├── Services/                   # DialogueService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Progression/ # NEW: Character progression plugin
│   │   ├── Components/                 # Experience, Level
│   │   ├── Systems/                    # ExperienceSystem, LevelingSystem
│   │   ├── Services/                   # ProgressionService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Spells/      # NEW: Spell & ability plugin
│   │   ├── Components/                 # Mana, Spell, SpellBook, SpellCooldown
│   │   ├── Systems/                    # ManaSystem, SpellCastingSystem
│   │   ├── Services/                   # SpellService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Merchant/    # NEW: Trading system plugin
│   │   ├── Components/                 # Gold, MerchantInventory, TradeState
│   │   ├── Systems/                    # TradingSystem, MerchantSystem
│   │   ├── Services/                   # MerchantService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Boss/        # NEW: Boss encounter plugin
│   │   ├── Components/                 # Boss, BossPhase, BossAbility
│   │   ├── Systems/                    # BossSystem, BossAISystem
│   │   ├── Services/                   # BossService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Hazards/     # NEW: Traps & hazards plugin
│   │   ├── Components/                 # Trap, EnvironmentalHazard, TrapState
│   │   ├── Systems/                    # TrapSystem, HazardSystem
│   │   ├── Services/                   # HazardService
│   │   └── plugin.json
│   │
│   ├── LablabBean.Plugins.Inventory/   # EXISTING: Enhanced for trading
│   └── LablabBean.Plugins.StatusEffects/ # EXISTING: Used by spells/traps
│
├── console-app/
│   └── LablabBean.Game.TerminalUI/     # Terminal UI (enhanced with new interfaces)
│
└── windows-app/
    └── LablabBean.Game.SadConsole/     # Windows UI (enhanced with new interfaces)

tests/
├── LablabBean.Plugins.Quest.Tests/     # Unit + integration tests per plugin
├── LablabBean.Plugins.NPC.Tests/
├── LablabBean.Plugins.Progression.Tests/
├── LablabBean.Plugins.Spells.Tests/
├── LablabBean.Plugins.Merchant.Tests/
├── LablabBean.Plugins.Boss.Tests/
└── LablabBean.Plugins.Hazards.Tests/
```

**Structure Decision**: Plugin-based architecture with 7 new plugins (Quest, NPC, Progression, Spells, Merchant, Boss, Hazards). Each plugin follows the established pattern from existing Inventory and StatusEffects plugins: Components define data, Systems process ECS queries, Services provide high-level APIs exposed via IPluginContext. Core framework (LablabBean.Game.Core) is enhanced with shared components but heavy logic stays in plugins for modularity. This structure enables independent development, testing, and potential reuse across projects.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

N/A - No constitution violations. All complexity justified in Constitution Check section.
