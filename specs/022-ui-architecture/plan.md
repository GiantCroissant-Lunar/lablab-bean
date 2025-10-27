# UI System Architecture Consolidation – Implementation Plan

**Status**: Draft
**Last Updated**: 2025-10-27

## Phases

Phase 1 – Contracts (General + Game)

- Create framework package: LablabBean.Rendering.Contracts
  - ISceneRenderer (minimal surface first)
  - DTOs: Color, Glyph, Tile, TileBuffer, Palette
- Create framework package: LablabBean.Contracts.Game.UI
  - IDungeonCrawlerUI (HUD/dialogue/quest/inventory/camera follow)
  - IActivityLog (game log semantics)
- Adjust LablabBean.Contracts.UI
  - Keep IUiService, ViewportBounds, InputCommand
  - Add a generic Feed adapter concept (for list-like panels) if needed
- Migrate Activity Log contract from Contracts.UI to Contracts.Game.UI

Phase 2 – Terminal Stack

- Rendering.Terminal plugin
  - Implement ISceneRenderer using Terminal.Gui v2
  - Provide TileBuffer→glyph grid mapping, palette support
  - plugin.json: capabilities ["renderer", "renderer:terminal"]
- LablabBean.Game.TerminalUI adapter
  - Add adapter implementing IUiService + IDungeonCrawlerUI
  - Compose HudService, WorldViewService, ActivityLogView
- UI.Terminal plugin
  - Reference LablabBean.Game.TerminalUI and Rendering.Terminal
  - Compose actual UI, register services in IRegistry (IUiService, IDungeonCrawlerUI)
  - Replace placeholder window
- Cleanup
  - Remove (or archive) console TUI legacy files and views

Phase 3 – Windows/SadConsole Stack

- Rendering.SadConsole plugin
  - Implement ISceneRenderer using SadConsole surfaces
  - plugin.json: capabilities ["renderer", "renderer:windows"]
- LablabBean.Game.SadConsole adapter
  - Add adapter implementing IUiService + IDungeonCrawlerUI (wrap GameScreen/HUD renderers)
- UI.SadConsole plugin
  - Compose GameScreen and HUD, register services in IRegistry
  - Update LablabBean.Windows to be a thin host that loads plugins

Phase 4 – Selection + Validation

- Capability policy: exactly one UI and one Renderer active
- Add validation to PluginLoader to enforce capability constraints
- Config switches to prefer terminal vs windows

Phase 5 – Hardening

- Graceful lifecycle start/stop (init, run, shutdown)
- Input routing coverage (keyboard/mouse for windows; keyboard for terminal)
- Viewport events and performance checks
- Documentation and quickstarts for both stacks

Milestones

- M1: Contracts present, build passes
- M2: Terminal stack functional via plugin; console legacy removed
- M3: Windows stack functional via plugin; LablabBean.Windows loads UI via plugin
- M4: Capability enforcement + docs

Acceptance Criteria

- Console uses only UI.Terminal
- Windows uses only UI.SadConsole
- ActivityLog interface resides under Contracts.Game.UI
- Rendering and UI separable for tests

Notes

- Start with a minimal ISceneRenderer and TileBuffer; evolve with usage to avoid over-design.
