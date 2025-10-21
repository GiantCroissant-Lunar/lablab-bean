# Implementation Plan: Status Effects System

**Branch**: `002-status-effects` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-status-effects/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a comprehensive status effects system that adds tactical depth to combat through temporary buffs, debuffs, and damage/healing over time effects. The system will:
- Support 8+ effect types (Poison, Regeneration, Strength, Weakness, Haste, Slow, Defense Boost, Defense Break)
- Track turn-based duration for each effect (1-99 turns)
- Apply damage/healing at turn start, modify combat stats dynamically
- Display active effects in HUD with remaining duration
- Support multiple simultaneous effects (up to 10 per entity)
- Enable consumable items and enemy attacks to apply effects

The system extends the existing ECS architecture with new components and systems, integrates with the Actor/Combat/Inventory systems, and follows established turn-based patterns.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 8  
**Primary Dependencies**: Arch ECS (1.3.3), GoRogue (3.0.0-beta09), Terminal.Gui (2.0.0-pre.2), SadConsole (10.0.3)  
**Storage**: In-memory ECS components (status effects stored per-entity)  
**Testing**: xUnit, FluentAssertions (existing test infrastructure)  
**Target Platform**: Cross-platform console (Windows/Linux/macOS) via Terminal.Gui and Windows GUI via SadConsole
**Project Type**: Desktop game application with dual rendering modes  
**Performance Goals**: 60 FPS rendering, <1ms per status effect tick, instant HUD updates  
**Constraints**: Turn-based gameplay, max 10 concurrent effects per entity, 99 turn maximum duration  
**Scale/Scope**: 10-30 entities with effects active simultaneously, 8+ effect types in MVP

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Constitution template is not yet filled. Proceeding with project-established principles from existing codebase:

✅ **ECS Architecture**: Extends existing Arch ECS framework (LablabBean.Game.Core)
✅ **Data-Driven Design**: Effect definitions use component-based approach
✅ **Separation of Concerns**: Game logic (Core) separate from rendering (Terminal/SadConsole)
✅ **Existing Patterns**: Follows established system/component patterns from combat and inventory
✅ **No Breaking Changes**: Additive only - integrates with existing Actor/Combat systems
✅ **Dependency on spec-001**: Builds on inventory system for consumable items

**Post-Design Re-check**: Will validate after Phase 1 that design maintains these principles.

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
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```
dotnet/framework/
├── LablabBean.Game.Core/              # Shared game framework (ECS)
│   ├── Components/
│   │   ├── StatusEffect.cs            # 🆕 NEW: Status effect components
│   │   ├── Actor.cs                   # ✨ EXTEND: Add StatusEffects component
│   │   └── ...
│   ├── Systems/
│   │   ├── StatusEffectSystem.cs      # 🆕 NEW: Effect application/tick logic
│   │   ├── ActorSystem.cs             # ✨ EXTEND: Integrate effect duration tracking
│   │   ├── CombatSystem.cs            # ✨ EXTEND: Apply stat modifiers from effects
│   │   ├── InventorySystem.cs         # ✨ EXTEND: Apply effects from consumables
│   │   └── ...
│   └── Services/
│       └── GameStateManager.cs        # ✨ EXTEND: Register StatusEffectSystem
│
├── LablabBean.Game.Terminal/          # Terminal.Gui rendering
│   └── Services/
│       └── HudService.cs              # ✨ EXTEND: Display active status effects
│
└── LablabBean.Game.SadConsole/        # SadConsole rendering
    └── Renderers/
        └── HudRenderer.cs             # ✨ EXTEND: Display active status effects

dotnet/console-app/
└── LablabBean.Console/
    └── Program.cs                     # ✨ EXTEND: Register StatusEffectSystem
```

**Structure Decision**: Extends existing ECS game framework. New status effect components and system go into `LablabBean.Game.Core`, rendering extensions go into platform-specific projects. Integrates with existing Actor, Combat, and Inventory systems.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

