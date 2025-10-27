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

- [ ] Phase 2 – Terminal Stack
  - [ ] Rendering.Terminal plugin implemented
  - [ ] LablabBean.Game.TerminalUI adapter (IUiService + IDungeonCrawlerUI)
  - [ ] UI.Terminal plugin composes real UI and registers services
  - [ ] Console legacy TUI files removed/archived

- [ ] Phase 3 – Windows/SadConsole Stack
  - [ ] Rendering.SadConsole plugin implemented
  - [ ] LablabBean.Game.SadConsole adapter (IUiService + IDungeonCrawlerUI)
  - [ ] UI.SadConsole plugin composes GameScreen/HUD and registers services
  - [ ] Windows app loads plugins only

- [ ] Phase 4 – Selection + Validation
  - [ ] Capability policy enforced (single UI + single renderer)
  - [ ] Config switches wired

- [ ] Phase 5 – Hardening
  - [ ] Lifecycle tested
  - [ ] Input routing tested
  - [ ] Viewport events verified
  - [ ] Docs/quickstarts updated

Notes

- Track blockers and decisions here during implementation.
