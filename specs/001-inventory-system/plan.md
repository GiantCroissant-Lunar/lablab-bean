# Implementation Plan: Inventory System with Item Pickup and Usage

**Branch**: `001-inventory-system` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-inventory-system/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a complete inventory system for the dungeon crawler game that enables players to:

- Pick up items from the dungeon floor (healing potions, weapons, armor)
- View and manage inventory through the HUD
- Consume healing potions to restore health
- Equip weapons and armor to modify combat stats

The system will extend the existing ECS architecture (Arch) with new components and systems, integrate with the Terminal.Gui HUD, and follow the data-driven design principles established in the game framework.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 8
**Primary Dependencies**: Arch ECS (1.3.3), GoRogue (3.0.0-beta09), Terminal.Gui (2.0.0-pre.2), SadConsole (10.0.3)
**Storage**: In-memory ECS components (future: JSON serialization for save/load)
**Testing**: xUnit, FluentAssertions (existing test infrastructure)
**Target Platform**: Cross-platform console (Windows/Linux/macOS) via Terminal.Gui and Windows GUI via SadConsole
**Project Type**: Desktop game application with dual rendering modes
**Performance Goals**: 60 FPS rendering, <16ms frame time, instant UI updates (<100ms)
**Constraints**: Turn-based gameplay (no real-time pressure), 20-item inventory limit, single-player only
**Scale/Scope**: Single dungeon level, 20-50 rooms, 10-30 enemies, 20-100 items per playthrough

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Constitution template is not yet filled. Proceeding with project-established principles from existing codebase:

✅ **ECS Architecture**: Extends existing Arch ECS framework (LablabBean.Game.Core)
✅ **Data-Driven Design**: Item definitions will use existing component-based approach
✅ **Separation of Concerns**: Game logic (Core) separate from rendering (Terminal/SadConsole)
✅ **Existing Patterns**: Follows established system/component patterns from combat and movement
✅ **No Breaking Changes**: Additive only - no modifications to existing game systems

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
│   │   ├── Item.cs                    # ✨ EXTEND: Add new item components
│   │   ├── Actor.cs                   # ✨ EXTEND: Inventory component already exists
│   │   └── ...
│   ├── Systems/
│   │   ├── InventorySystem.cs         # 🆕 NEW: Item pickup/usage logic
│   │   ├── ItemSpawnSystem.cs         # 🆕 NEW: Item generation
│   │   └── ...
│   └── Services/
│       └── GameStateManager.cs        # ✨ EXTEND: Integrate inventory systems
│
├── LablabBean.Game.Terminal/          # Terminal.Gui rendering
│   └── Services/
│       └── HudService.cs              # ✨ EXTEND: Add inventory display
│
└── LablabBean.Game.SadConsole/        # SadConsole rendering
    └── Renderers/
        └── HudRenderer.cs             # ✨ EXTEND: Add inventory display

dotnet/console-app/
└── LablabBean.Console/
    ├── Program.cs                     # ✨ EXTEND: Register new systems
    └── Services/
        └── DungeonCrawlerService.cs   # ✨ EXTEND: Add inventory input handling
```

**Structure Decision**: Extends existing ECS game framework. All new components go into `LablabBean.Game.Core`, rendering extensions go into platform-specific projects (`Game.Terminal`, `Game.SadConsole`). This maintains the established separation between game logic and rendering.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
