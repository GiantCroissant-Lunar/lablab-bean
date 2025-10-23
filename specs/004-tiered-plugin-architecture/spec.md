# Feature Specification: Tiered Plugin Architecture Adoption

**Feature Branch**: `004-tiered-plugin-architecture`
**Created**: 2025-10-21
**Status**: Draft
**Input**: "Adopt a tiered design with plugin usage in lablab-bean, leveraging prior art from CrossMilo (contracts), PluginManoi (loader/registry), and Winged Bean (plugin migration and hosts)."

## User Scenarios & Testing (mandatory)

### Scenario 1 - Discover and Load Plugins (Priority: P1)

A console or windows host scans a `plugins/` directory, reads plugin manifests, resolves dependencies, loads assemblies via AssemblyLoadContext, and starts each plugin. Host logs successful discovery and initialization.

- Acceptance:
  - Given a manifest-compliant plugin in `plugins/`, when the host starts, then the plugin is discovered, dependency-checked, loaded, and `StartAsync()` is invoked.
  - Given a missing hard dependency, when loading, then loading fails for that plugin with a clear error and other plugins continue.

### Scenario 2 - Hard vs Soft Dependencies (Priority: P1)

A plugin declares hard deps (must exist) and soft deps (optional). Loader enforces hard deps and warns on soft deps absence.

- Acceptance:
  - Hard dep missing → plugin not started; registry shows Failed state with reason.
  - Soft dep missing → plugin started; warning logged; capability that requires soft dep is disabled.

### Scenario 3 - Hot Reload (Priority: P2)

Replace a plugin assembly on disk; host triggers unload and reload using collectible ALC.

- Acceptance:
  - On file change, loader stops plugin, unloads ALC, reloads updated assembly, re-registers services.
  - No memory growth after 3 reload cycles (±10%).

### Scenario 4 - Tier Compliance (Priority: P2)

Contracts target `netstandard2.1` for cross-profile, while hosts/plugins target `net8.0`.

- Acceptance:
  - Build succeeds with contracts referenced by both console and windows hosts.
  - Plugins compile against contracts without referencing host-specific assemblies.

### Scenario 5 - Game Plugin Boot (Priority: P1)

`DungeonGame` plugin boots and exposes services (e.g., map gen, systems) to the host via DI.

- Acceptance:
  - Host resolves a game service from plugin (`IGameLoop`), runs a tick, and receives events/logs.

---

## Requirements (mandatory)

### Functional Requirements

- FR-001: Define plugin manifest schema with multi-profile support (id, name, version, entryPoint dictionary, dependencies, capabilities, priority, loadStrategy).
- FR-002: Implement plugin lifecycle: InitializeAsync(IPluginContext), StartAsync, StopAsync. Use IPluginContext to isolate ALC boundary (no direct IServiceCollection exposure).
- FR-003: Provide dependency resolution using **Kahn's topological sort algorithm**:
  - **Hard dependencies**: Missing → plugin excluded from load order, ERROR logged, FailureReason set
  - **Soft dependencies**: Missing → WARNING logged, plugin loaded with reduced features
  - **Cycle detection**: If `sorted.Count != pluginCount`, throw `InvalidOperationException` with cycle details
  - **Version selection**: Highest semantic version per plugin ID (NuGet.Versioning library)
- FR-004: Implement AssemblyLoadContext-based loader with isolation and optional hot reload (collectible ALC, GC forcing, file watcher).
- FR-005: Provide `IPluginRegistry` to query plugin state (Created/Initialized/Started/Failed/Stopped/Unloaded).
- FR-006: Provide `IRegistry` for cross-ALC service registration (priority-based, runtime type matching, no compile-time coupling).
- FR-007: Configurable plugin probing paths via appsettings.json or env vars (multi-path support).
- FR-008: Structured logging for all lifecycle events (Microsoft.Extensions.Logging with plugin ID category).
- FR-009: Metrics (optional P2): load time histogram, failure counter, reload counter (OpenTelemetry recommended).
- FR-010: Tests: integration tests for lifecycle, dependency resolution, unload/reload, and cross-ALC service access.

### Key Entities

- IPlugin: plugin contract surface (InitializeAsync, StartAsync, StopAsync).
- IPluginContext: initialization context passed to plugin (Registry, Configuration, Logger, Host).
- IRegistry: cross-ALC service registry (priority-based, runtime type matching).
- IPluginRegistry: runtime registry of plugin descriptors and states.
- IPluginHost: host surface available to plugins (logger factory, event bus, services).
- PluginManifest: metadata + dependency list (hard/soft, version ranges, multi-profile entry points).
- PluginDependency: id + version range + optional flag.
- PluginState: Created, Initialized, Started, Failed, Stopped, Unloaded.
- PluginLoader: scanning, validation, load/unload, lifecycle orchestration.
- ServiceMetadata: priority, name, version for service registration conflict resolution.

## Success Criteria

- SC-001: Host starts and loads 1+ plugin from disk within 1000ms per plugin on dev machine.
- SC-002: Hard dep failure is isolated; other plugins start successfully 100% of time.
- SC-003: At least one plugin can be hot reloaded 3× in a row without memory growth >10%.
- SC-004: Contracts package compiles under netstandard2.1 and is referenced by both hosts.
- SC-005: E2E demo: DungeonGame plugin renders first frame in console host.

## Assumptions

1. .NET 8 for hosts/plugins; contracts on netstandard2.1.
2. DI via Microsoft.Extensions.* is already available in hosts.
3. File-based plugin discovery is sufficient for MVP.
4. No sandboxing/security isolation required beyond ALC (future work).

## Dependencies

- CrossMilo: contracts packaging pattern (`ref-projects/cross-milo/...`).
- PluginManoi: loader/registry designs (`ref-projects/plugin-manoi/...`).
- Winged Bean: multi-profile hosts and plugin migration (`ref-projects/winged-bean/...`).

## Out of Scope

- Plugin signing/verification.
- Network-downloaded plugin distribution.
- Cross-version side-by-side plugin instances.
