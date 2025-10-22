# Implementation Plan: .NET Project Naming and Architecture Adjustment

**Branch**: `011-dotnet-naming-architecture-adjustment` | **Date**: 2025-10-22 | **Spec**: `specs/011-dotnet-naming-architecture-adjustment/spec.md`
**Input**: Architecture specification `docs/_inbox/2025-10-22-dotnet-naming-architecture-adjustment--DOC-2025-00042.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan implements DOC-2025-00042 to standardize naming, introduce contract-level proxy services, convert reporting renderers to plugins, and prepare a platform-agnostic loader abstraction.

Phases:
- Phase 1: Simple renames (low risk)
  - `LablabBean.Reporting.Abstractions` → `LablabBean.Reporting.Contracts`
  - `LablabBean.Reporting.SourceGen` → `LablabBean.SourceGenerators.Reporting`
- Phase 2: Contract proxies
  - Add `[RealizeService]`-based proxy partials in Contract projects
  - Ensure analyzer reference to `LablabBean.SourceGenerators.Proxy`
- Phase 3: Renderer plugins
  - Move Csv/Html renderers to `LablabBean.Plugins.Reporting.*`
  - Update `IReportingService` to discover `IReportRenderer` via registry
- Phase 4 (Future): Platform-agnostic loader
  - Extract `IPluginLoader` and move ALC-based loader to `LablabBean.Plugins.Loader.ALC`

## Technical Context

**Language/Version**: .NET 8 (C# 12)  
**Primary Dependencies**: Microsoft.CodeAnalysis 4.9.x (incremental generators), Microsoft.CodeAnalysis.Analyzers, Microsoft.Extensions.*; existing plugin system (`LablabBean.Plugins.Core`, `LablabBean.Plugins.Contracts`)  
**Storage**: N/A  
**Testing**: xUnit, FluentAssertions; source generator tests as needed  
**Target Platform**: Windows/Linux (CI), .NET SDK 8.x
**Project Type**: Multi-project library + plugins  
**Performance Goals**: No regression in build time; generator functions under typical solution load  
**Constraints**: Zero breaking namespace errors; all solution tests pass post-rename  
**Scale/Scope**: Repository-wide naming for reporting assemblies and related plugins

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)

```
specs/011-dotnet-naming-architecture-adjustment/
├── plan.md              # This file
├── spec.md              # WHAT/WHY (already created)
├── tasks.md             # Generated next
└── (optional) quickstart.md, checklists/
```

### Source Code (repository root after refactor)

```
dotnet/
├── framework/
│   ├── LablabBean.Core
│   ├── LablabBean.Infrastructure
│   ├── LablabBean.Reactive
│   ├── LablabBean.Game.Core
│   │
│   ├── Contracts (Interfaces + Proxies)
│   ├── LablabBean.Contracts.*               # existing domains
│   ├── LablabBean.Plugins.Contracts
│   ├── LablabBean.Reporting.Contracts       # ← Renamed from Reporting.Abstractions
│   │
│   ├── Source Generators
│   ├── LablabBean.SourceGenerators.Proxy
│   ├── LablabBean.SourceGenerators.Reporting # ← Renamed from Reporting.SourceGen
│   │
│   ├── Plugin Infrastructure
│   ├── LablabBean.Plugins.Core
│   │
│   └── Reporting Core
│       ├── LablabBean.Reporting.Analytics
│       └── LablabBean.Reporting.Providers.Build
│
├── plugins/
│   └── Infrastructure Plugins
│       ├── LablabBean.Plugins.Reporting.Csv   # ← Moved from framework
│       ├── LablabBean.Plugins.Reporting.Html  # ← Moved from framework
│       └── (Future) LablabBean.Plugins.Loader.ALC
│
└── tests/
    ├── LablabBean.Contracts.*.Tests
    ├── LablabBean.Plugins.*.Tests
    ├── LablabBean.Plugins.Reporting.Csv.Tests
    ├── LablabBean.Plugins.Reporting.Html.Tests
    └── LablabBean.Reporting.*.Tests
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

- Adopt Contracts/SourceGenerators/Plugins classification per DOC-2025-00042.
- Keep Reporting core libraries under `framework/`; renderers under `plugins/`.
- Maintain existing test projects; move renderer tests alongside plugins.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

