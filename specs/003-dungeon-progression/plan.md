# Implementation Plan: Dungeon Progression System

**Branch**: `003-dungeon-progression` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-dungeon-progression/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a multi-level dungeon progression system that enables players to descend and ascend through procedurally generated levels with persistent state. The system will:

- Generate staircases (up/down) in dungeons for level transitions
- Persist dungeon state (map layout, enemies, items) when leaving a level
- Restore exact dungeon state when returning to a level
- Scale enemy difficulty exponentially with depth (stats × 1.12^level)
- Scale loot drop rates and quality with depth
- Track current depth and personal best depth
- Support victory condition (level 20) or endless mode

The system extends the existing dungeon generation and game state management, integrates with the enemy spawning and item systems, and maintains player progression across levels.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 8
**Primary Dependencies**: Arch ECS (1.3.3), GoRogue (3.0.0-beta09), Terminal.Gui (2.0.0-pre.2), SadConsole (10.0.3)
**Storage**: In-memory level state cache (Dictionary<int, DungeonLevel>), future: JSON serialization for save/load
**Testing**: xUnit, FluentAssertions (existing test infrastructure)
**Target Platform**: Cross-platform console (Windows/Linux/macOS) via Terminal.Gui and Windows GUI via SadConsole
**Project Type**: Desktop game application with dual rendering modes
**Performance Goals**: <500ms level generation, <100ms level transition, 60 FPS rendering
**Constraints**: Turn-based gameplay, max 30 cached levels in memory, level 30 difficulty cap
**Scale/Scope**: 20-30 dungeon levels (victory at 20, endless beyond), 50-100 rooms per level

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Constitution template is not yet filled. Proceeding with project-established principles from existing codebase:

✅ **ECS Architecture**: Extends existing Arch ECS framework (LablabBean.Game.Core)
✅ **Data-Driven Design**: Level state uses component-based serialization
✅ **Separation of Concerns**: Game logic (Core) separate from rendering (Terminal/SadConsole)
✅ **Existing Patterns**: Follows established dungeon generation and game state patterns
✅ **No Breaking Changes**: Additive only - extends MapGenerator and GameStateManager
✅ **Dependencies**: Builds on dungeon generation, enemy spawning, item systems

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
│   │   ├── Staircase.cs               # 🆕 NEW: Staircase component (up/down)
│   │   └── ...
│   ├── Maps/
│   │   ├── DungeonLevel.cs            # 🆕 NEW: Level state container
│   │   ├── LevelManager.cs            # 🆕 NEW: Level persistence and transitions
│   │   ├── MapGenerator.cs            # ✨ EXTEND: Add staircase placement
│   │   └── ...
│   ├── Systems/
│   │   ├── DifficultyScalingSystem.cs # 🆕 NEW: Enemy/loot scaling logic
│   │   ├── EnemySpawnSystem.cs        # ✨ EXTEND: Apply difficulty scaling
│   │   ├── ItemSpawnSystem.cs         # ✨ EXTEND: Apply loot scaling
│   │   └── ...
│   └── Services/
│       └── GameStateManager.cs        # ✨ EXTEND: Integrate LevelManager
│
├── LablabBean.Game.Terminal/          # Terminal.Gui rendering
│   └── Services/
│       └── HudService.cs              # ✨ EXTEND: Display dungeon level/depth
│
└── LablabBean.Game.SadConsole/        # SadConsole rendering
    └── Renderers/
        └── HudRenderer.cs             # ✨ EXTEND: Display dungeon level/depth

dotnet/console-app/
└── LablabBean.Console/
    └── Services/
        └── DungeonCrawlerService.cs   # ✨ EXTEND: Handle staircase interaction (>, < keys)
```

**Structure Decision**: Extends existing ECS game framework. New level management components and systems go into `LablabBean.Game.Core/Maps`, difficulty scaling into `Systems`. Integrates with existing MapGenerator, enemy/item spawn systems, and game state management.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
