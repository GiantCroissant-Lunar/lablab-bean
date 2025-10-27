# UI System Architecture – Task Backlog

**Status**: Draft
**Last Updated**: 2025-10-27

## Phase 1 – Contracts

- Create framework project: dotnet/framework/LablabBean.Rendering.Contracts
  - Define ISceneRenderer
  - Define DTOs: Color, Glyph, Tile, TileBuffer, Palette
- Create framework project: dotnet/framework/LablabBean.Contracts.Game.UI
  - Define IDungeonCrawlerUI
  - Define IActivityLog (migrate from Contracts.UI)
- Update references where ActivityLog moved
- Document generic Feed rendering pattern in Contracts.UI README (optional)

Phase 2 – Terminal Stack

- New plugin: dotnet/plugins/LablabBean.Plugins.Rendering.Terminal
  - Implement ISceneRenderer via Terminal.Gui v2 glyph grid
  - Add plugin.json with capabilities ["renderer", "renderer:terminal"]
- Update library: dotnet/console-app/LablabBean.Game.TerminalUI
  - Add adapter implementing IUiService + IDungeonCrawlerUI; wire HudService, WorldViewService, ActivityLogView
- Update plugin: dotnet/plugins/LablabBean.Plugins.UI.Terminal
  - Reference LablabBean.Game.TerminalUI + Rendering.Terminal
  - Replace placeholder with composed UI
  - Register IUiService + IDungeonCrawlerUI via IRegistry
- Cleanup legacy
  - Remove or move: dotnet/console-app/LablabBean.Console/Services/TerminalGuiService.cs
  - Remove or move: dotnet/console-app/LablabBean.Console/Services/DungeonCrawlerService.cs
  - Remove or move: dotnet/console-app/LablabBean.Console/Views/*

Phase 3 – Windows/SadConsole Stack

- New plugin: dotnet/plugins/LablabBean.Plugins.Rendering.SadConsole
  - Implement ISceneRenderer via SadConsole
  - plugin.json with capabilities ["renderer", "renderer:windows"]
- Update library: dotnet/windows-app/LablabBean.Game.SadConsole
  - Add adapter implementing IUiService + IDungeonCrawlerUI (wrap GameScreen/HUD)
- New plugin: dotnet/plugins/LablabBean.Plugins.UI.SadConsole
  - Compose GameScreen/HUD
  - Register IUiService + IDungeonCrawlerUI via IRegistry
- Host changes
  - Update dotnet/windows-app/LablabBean.Windows to load plugins only

Phase 4 – Selection + Validation

- Extend PluginLoader validation to enforce single UI + single renderer
- Add configuration switches for preferred UI/renderer

Phase 5 – Hardening + Docs

- Add lifecycle tests for plugin start/stop
- Add input routing tests/mocks
- Add performance checks for viewport updates
- Add quickstarts for terminal/windows
- Update README with new architecture diagram

Out of Scope (nice-to-have later)

- Unity adapters (Rendering.Unity, UI.Unity)
- Lighting/FOV advanced DTOs (layers, z-order)
