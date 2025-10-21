# Implementation Plan: Tiered Plugin Architecture

**Branch**: `004-tiered-plugin-architecture` | **Spec**: `./spec.md`

## Summary
Adopt a tiered plugin architecture modeled after Winged Bean and PluginManoi: contracts in Tier 1/2 (netstandard2.1), loader/registry core also netstandard2.1 where possible, with host-specific loading in Tier 3/4 (net8.0). Provide a DungeonGame plugin as first consumer and prepare migrations for Inventory and Status Effects.

## Target Architecture (Tiers)
- Tier 1: Contracts (netstandard2.1)
  - `LablabBean.Plugins.Contracts`
  - Interfaces: `IPlugin`, `IPluginRegistry`, `IPluginHost`, manifest models.
- Tier 2: Core (netstandard2.1)
  - `LablabBean.Plugins.Core`
  - Registry impl, manifest parsing, dependency resolution.
- Tier 3: Host Integrations (net8.0)
  - Console host: integrate loader hosted service.
  - Windows host: integrate loader hosted service.
- Tier 4: Game Plugins (net8.0)
  - `LablabBean.Plugins.DungeonGame` (initial demo plugin)
  - Future: Inventory, StatusEffects as separate plugins.

## Proposed Repository Layout
```
dotnet/framework/
├── LablabBean.Plugins.Contracts/        # netstandard2.1
├── LablabBean.Plugins.Core/             # netstandard2.1
└── LablabBean.Game.Core/                # existing (will consume contracts)

dotnet/console-app/
└── LablabBean.Console/                  # add PluginLoaderHostedService

dotnet/windows-app/
└── LablabBean.Windows/                  # add PluginLoaderHostedService

plugins/                                  # development plugins (optional)
└── LablabBean.Plugins.DungeonGame/      # net8.0 demo plugin
```

## Phase Breakdown
- Phase 0: Research & Constitution Check
  - Review `.agent/base/*` and Spec-Kit conventions.
  - Map prior art: CrossMilo contracts, PluginManoi loader/registry, Winged Bean hosts.
- Phase 1: Contracts & Data Model
  - Define `IPlugin`, manifest models, dependency entity, `IPluginRegistry`, `PluginState`.
  - Validate contract API against Unity/Godot future (keep netstandard2.1 friendly).
- Phase 2: Core & Loader
  - Implement registry and dependency resolver (acyclic, topo sort, fail early).
  - Implement loader (ALC) in host with hot reload toggle.
  - Structured logging + metrics.
- Phase 3: Host Integrations
  - Console: background hosted service to scan, load, start.
  - Windows: same flow; verify no UI thread blocking.
- Phase 4: Demo Plugin & E2E
  - Create `DungeonGame` plugin (service add + one tick pipeline).
  - E2E test: load, start, stop; verify metrics & logs.
- Phase 5: Migrations (Specs 005, 006)
  - Publish specs for Inventory and Status Effects pluginization.

## Risks & Mitigations
- ALC unload complexity → keep references inside ALC; add unload tests.
- Dependency cycles → explicit cycle detection; clear diagnostics.
- DI collisions → plugin-only service namespaces; validate service keys.

## Project Integration Points
- `dotnet/framework/LablabBean.Game.Core` will depend on contracts (for plugin-exposed services).
- Hosts (`LablabBean.Console`, `LablabBean.Windows`) add loader hosted service.

## Deliverables
- Contracts, Core libs, host integration, and demo plugin scaffolding.
- Documentation: quickstart, research, tasks.

