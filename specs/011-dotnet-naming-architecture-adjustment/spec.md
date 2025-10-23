# Feature Specification: .NET Project Naming and Architecture Adjustment

**Feature Branch**: `011-dotnet-naming-architecture-adjustment`
**Created**: 2025-10-22
**Status**: Draft
**Input**: Architecture spec `docs/_inbox/2025-10-22-dotnet-naming-architecture-adjustment--DOC-2025-00042.md`

## User Scenarios & Testing (mandatory)

### User Story 1 - Developer standardizes naming across reporting (Priority: P1)

As a developer, I need consistent project and namespace naming (Contracts, SourceGenerators, Plugins) so the codebase is predictable and discoverable.

- Acceptance Scenarios:
  1. Given existing Reporting projects, when the rename is applied, then all references build successfully without namespace errors.
  2. Given centralized naming rules, when a new project is created, then it follows the Contracts/SourceGenerators/Plugins classification.
  3. Given CI, when the solution builds, then all renamed projects compile and all tests pass.

---

### User Story 2 - Contract projects expose generated proxies (Priority: P1)

As a developer, I want proxy services to live in Contract projects and be generated via attributes so consumers can use typed proxies without referencing the registry directly.

- Acceptance Scenarios:
  1. Given an interface in a Contract project, when a partial class is annotated with `[RealizeService]`, then the proxy methods are generated and compile.
  2. Given a proxy class with `IRegistry` field, when code calls the proxy, then calls delegate to the registered implementation resolved by the registry.
  3. Given Selection Strategy attributes, when multiple implementations exist, then the selected implementation follows the strategy.

---

### User Story 3 - Reporting renderers become replaceable plugins (Priority: P2)

As a maintainer, I want report renderers (Csv/Html/FastReport) to be plugins discovered via the registry so the system is extensible.

- Acceptance Scenarios:
  1. Given renderer plugins registered via `IPlugin`, when the reporting service requests a format, then the correct renderer is resolved via registry.
  2. Given a missing renderer for a requested format, when a report is requested, then a clear error explains no renderer supports that format.
  3. Given renderer plugins moved from framework to plugins, when tests run, then end-to-end report generation still succeeds.

---

### User Story 4 - Prepare for platform-agnostic plugin loading (Priority: P3/Future)

As an architect, I want clear loader abstractions so alternate loaders (ALC, HybridCLR) can be swapped without touching core plugin contracts.

- Acceptance Scenarios:
  1. Given `IPluginLoader` abstraction, when a .NET ALC loader is provided, then plugins load as they did before.
  2. Given a future HybridCLR loader, when plugged in, then plugin discovery continues to work without changing core contracts.

## Requirements (mandatory)

### Naming & Structure

- **FR-001**: Use the prefix `LablabBean` for all .NET projects and namespaces.
- **FR-002**: Use "Contracts" for interface projects (e.g., `LablabBean.Reporting.Contracts`).
- **FR-003**: Unify reporting source generators under `LablabBean.SourceGenerators.Reporting`.
- **FR-004**: Classify renderer implementations as plugins under `LablabBean.Plugins.Reporting.*`.

### Contract Proxies

- **FR-005**: Contract projects MUST contain proxy partial classes generated via `[RealizeService]`.
- **FR-006**: Proxies MUST delegate calls to `IRegistry`-resolved implementations.
- **FR-007**: Proxies SHOULD support selection strategies where applicable.
- **FR-008**: Contract projects MUST reference `LablabBean.SourceGenerators.Proxy` as an analyzer.

### Reporting Renderers as Plugins

- **FR-009**: Csv and Html renderers MUST be plugins with `IPlugin` entry points and manifests.
- **FR-010**: Reporting service MUST discover renderers dynamically via `IRegistry.GetAll<IReportRenderer>()`.
- **FR-011**: Renderer plugins MUST declare supported formats and provide render methods.

### Build & Verification

- **FR-012**: All renames MUST preserve build success for the entire solution.
- **FR-013**: Tests MUST be updated/moved accordingly and pass after refactor.
- **FR-014**: CI scripts MUST be updated to new paths/names.

### Platform Abstraction (Future)

- **FR-015**: Define `IPluginLoader` abstraction (future) decoupled from ALC specifics.
- **FR-016**: Provide a .NET ALC loader implementation under `LablabBean.Plugins.Loader.ALC`.

## Key Entities (include if feature involves data)

- **IReportRenderer**: Declares supported formats and render entry points.
- **IReportingService**: Orchestrates renderer discovery and report generation.
- **ServiceMetadata**: Name, Version, Priority for registry entries.
- **Proxy partial classes**: Generated via `[RealizeService]` in Contract projects.
- **IPluginLoader (future)**: Abstraction for platform-specific plugin loading.

## Success Criteria (mandatory)

- **SC-001**: All renamed projects build and tests pass with no namespace errors.
- **SC-002**: At least one Contract project demonstrates working generated proxies.
- **SC-003**: Renderer plugins (Csv/Html and/or FastReport) load and are discovered dynamically.
- **SC-004**: Reporting service resolves renderers by requested format and generates output.
- **SC-005**: CI builds succeed post-refactor with no path breakages.

## Assumptions

- Existing plugin system (`LablabBean.Plugins.Core`, `LablabBean.Plugins.Contracts`) remains stable.
- `LablabBean.SourceGenerators.Proxy` is already available and tested (SPEC-009).
- The refactor focuses on reporting-related naming and pluginization first.

## Out of Scope

- Runtime game feature changes unrelated to reporting or contracts.
- Non-reporting refactors beyond naming rules and proxies noted here.
- Immediate implementation of HybridCLR loader (planned as future).

## Dependencies

- `LablabBean.SourceGenerators.Proxy` (for proxies in Contracts).
- Plugin system and registry (`IRegistry`, `IPlugin`).
- Existing reporting components to be relocated/renamed.

## Notes

- Follow the migration guidance and verification steps in `DOC-2025-00042` for safe, incremental rollout.
- Prefer `git mv` for renames to preserve history; update solution and references incrementally.
