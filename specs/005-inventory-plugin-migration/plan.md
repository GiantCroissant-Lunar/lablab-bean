# Plan: Inventory Plugin Migration

- Create `LablabBean.Plugins.Inventory` (net8.0)
- Define public DI surface `IInventoryService` and read models for HUD
- Move/duplicate ECS components into plugin namespace (or reference existing until refactor)
- Bridge events to host via `IPluginHost.PublishEvent`
- Write adapter in TerminalUI HUD to consume read model

Phases:

1) Boot empty plugin and DI service
2) Port systems (spawn, pickup, use, equip)
3) Wire HUD updates via events
4) Tests: unit + manual quickstart flows from `specs/001` quickstart
