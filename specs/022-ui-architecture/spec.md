# UI System Architecture Consolidation (Console + Windows)

**Version**: 0.1 (Draft)
**Owners**: UI, Plugins
**Status**: Proposed
**Last Updated**: 2025-10-27

## Problem Statement

- Terminal.Gui bootstrapping is duplicated across the console host and the UI plugin.
- Windows (SadConsole) UI is composed inside the app rather than via a plugin.
- UI and Rendering responsibilities are mixed; this complicates future engines (e.g., Unity) and testing.
- Game-specific Activity Log semantics live in general UI contracts, blurring concerns.

Goals

- Exactly one active UI plugin at runtime per platform (Terminal or Windows).
- Separate Rendering (low-level drawing) from UI (layout, input, orchestration).
- Keep general UI contracts tech-agnostic; add game-specific UI contracts for Dungeon Crawler features.
- Reuse existing composition libraries (TerminalUI, SadConsole) inside their platform plugins.
- Remove legacy/duplicate TUI hosting and views from the console app.

Non-Goals

- Comprehensive engine abstraction beyond UI/Rendering.
- Runtime hot-swap of UI tech.

Architecture

- Contracts (tier 1, general)
  - Rendering.Contracts
    - ISceneRenderer: draw world/scene into an abstract surface.
    - Minimal DTOs: Tile, TileBuffer, Glyph, Color, Palette, optional layers.
  - UI.Contracts
    - IUiService: Initialize, UpdateDisplay, HandleInput, Get/SetViewport.
    - ViewportBounds, InputCommand remain here.
- Contracts (tier 2, game-specific: Dungeon Crawler)
  - Game.UI.Contracts
    - IDungeonCrawlerUI: HUD toggles, dialogue/quest/inventory panels, camera follow.
    - IActivityLog: game-log semantics (timestamp, severity, category, text, tags, icon).
- Plugins (tier 3, adapters)
  - Rendering.Terminal: Terminal.Gui v2 ISceneRenderer (glyph grid).
  - UI.Terminal: IUiService + IDungeonCrawlerUI using LablabBean.Game.TerminalUI controls.
  - Rendering.SadConsole: SadConsole ISceneRenderer (tiles/layers).
  - UI.SadConsole: IUiService + IDungeonCrawlerUI using GameScreen/HUD renderers.
- Selection
  - Capability tags: "ui:terminal", "ui:windows" and "renderer:*".
  - Registry priority + config flags select exactly one UI and one Renderer.

Migration Plan (summary)

1) Contracts
   - Create Rendering.Contracts; move game ActivityLog to Game.UI.Contracts.
   - Keep generic feed/list UX in UI.Contracts; adapt ActivityLog rendering via a feed adapter.
2) Terminal
   - Add Rendering.Terminal plugin.
   - Add Terminal UI adapter in LablabBean.Game.TerminalUI implementing IUiService + IDungeonCrawlerUI.
   - Update UI.Terminal plugin to compose the real UI and register to IRegistry.
   - Remove legacy console TUI files and views.
3) Windows/SadConsole
   - Add Rendering.SadConsole plugin.
   - Add UI.SadConsole plugin composing GameScreen/HUD; make LablabBean.Windows load plugins.
4) Selection + validation
   - Enforce single UI + single renderer; validate via plugin loader capabilities/deps.
5) Hardening
   - Lifecycle, input routing, viewport events; docs/samples for both Terminal and Windows.

Acceptance Criteria

- Console host launches only UI.Terminal plugin for TUI.
- Windows host launches only UI.SadConsole plugin.
- IActivityLog exists under Game.UI.Contracts; generic list/feed UX remains in UI.Contracts.
- No Terminal.Gui hosting code remains in the console app.
- Rendering and UI swappable for tests (mock renderer).

Risks

- Over-abstraction: start with minimal ISceneRenderer + TileBuffer; evolve as needed.
- Plugin ordering: require capability tags and explicit dependencies in plugin.json.
- Cross-ALC types: keep contracts/DTOs in framework assemblies only; plugins pass only interfaces/DTOs.

Open Questions

- Scene DTO scope now (TileBuffer + palette) vs later (layers, lighting, FOV)?
- ActivityLog fields (keep current vs slim)? Tags for extensibility?
- Override policy for active UI/renderer (config vs strict capability policy)?

References

- Console host plugin load: dotnet/console-app/LablabBean.Console/Program.cs:149
- Console legacy TUI excluded: dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj:42
- Terminal plugin placeholder: dotnet/plugins/LablabBean.Plugins.UI.Terminal/TerminalUiPlugin.cs:1
- Terminal UI library: dotnet/console-app/LablabBean.Game.TerminalUI
- Windows composition: dotnet/windows-app/LablabBean.Windows/Program.cs:61
