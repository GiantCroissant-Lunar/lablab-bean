# UI Architecture – Implementation Status

**Status**: Draft
**Last Updated**: 2025-10-27

## Checklist

- [x] Phase 1 – Contracts
  - [x] LablabBean.Rendering.Contracts created
  - [x] ISceneRenderer + DTOs defined
  - [x] LablabBean.Contracts.Game.UI created
  - [x] IDungeonCrawlerUI defined
  - [x] IActivityLog moved from Contracts.UI

- [x] Phase 2 – Terminal Stack
  - [x] Rendering.Terminal plugin implemented
  - [x] LablabBean.Game.TerminalUI adapter (IUiService + IDungeonCrawlerUI)
  - [x] UI.Terminal plugin composes real UI and registers services
  - [ ] Console legacy TUI files removed/archived

- [ ] Phase 3 – Terminal.Gui v2 API Compatibility
  - [x] Updated HudService for Terminal.Gui v2
  - [x] Updated WorldViewService for Terminal.Gui v2
  - [x] Updated ActivityLogView for Terminal.Gui v2
  - [x] Updated MapView for Terminal.Gui v2
  - [x] Wired services into TerminalUiAdapter
  - [ ] Integration testing with console app
  - [ ] DialogueView and QuestLogView (optional/deferred)

- [ ] Phase 4 – Windows/SadConsole Stack
  - [ ] Rendering.SadConsole plugin implemented
  - [ ] LablabBean.Game.SadConsole adapter (IUiService + IDungeonCrawlerUI)
  - [ ] UI.SadConsole plugin composes GameScreen/HUD and registers services
  - [ ] Windows app loads plugins only

- [ ] Phase 5 – Selection + Validation
  - [ ] Capability policy enforced (single UI + single renderer)
  - [ ] Config switches wired

- [ ] Phase 6 – Hardening
  - [ ] Lifecycle tested
  - [ ] Input routing tested
  - [ ] Viewport events verified
  - [ ] Docs/quickstarts updated

## Phase 3 Notes

**Terminal.Gui v2 API Migration Complete! ✅**

Successfully updated all Terminal UI components for Terminal.Gui v2 compatibility:

- **API Changes Applied:**
  - FrameView constructor now sets title as property
  - ListView constructor simplified
  - SetNeedsDisplay() → SetNeedsDraw()
  - Bounds → Frame property
  - Removed LayoutComplete event handler
  - Color.Brown → Color.DarkGray mapping

- **Re-enabled Components:**
  - HudService - Full stats/health/inventory display
  - WorldViewService - Map rendering with FOV
  - ActivityLogView - Event log display
  - MapView - Custom character rendering

- **TerminalUiAdapter:** Now has full HUD + WorldView + ActivityLog wired up

**Next:** Integration testing with actual console app to verify runtime behavior.
