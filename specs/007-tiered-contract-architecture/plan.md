# Implementation Plan: Tiered Contract Architecture

**Branch**: `007-tiered-contract-architecture` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-tiered-contract-architecture/spec.md`

## Summary

Implement a comprehensive tiered contract architecture based on cross-milo patterns, including:

- **Tier 1 (Contracts)**: 6 domain-specific contract assemblies (Game, UI, Scene, Input, Config, Resource) with service interfaces, events, and supporting types
- **Tier 2 (Infrastructure)**: IEventBus + EventBus implementation, Roslyn source generator for automatic proxy service generation, and two attributes ([RealizeService], [SelectionStrategy])
- **93 functional requirements** covering event-driven communication, domain contracts, and developer tooling

**Technical Approach**: Extend existing plugin system (IRegistry already implemented) with event bus, create 6 contract assemblies following cross-milo naming conventions (generic `IService` per domain), implement Roslyn incremental source generator to eliminate proxy boilerplate, enable loose coupling via publish-subscribe events.

## Technical Context

**Language/Version**: C# / .NET 8
**Primary Dependencies**:

- Existing: LablabBean.Plugins.Contracts (IRegistry, ServiceMetadata, SelectionMode)
- New: Microsoft.CodeAnalysis.CSharp (Roslyn source generator)
- Existing: LablabBean.Plugins.Core (ServiceRegistry implementation)

**Storage**: N/A (contracts are interfaces, events are in-memory)
**Testing**: xUnit, NSubstitute for mocking
**Target Platform**: Cross-platform (.NET 8: Windows, Linux, macOS)
**Project Type**: Multi-assembly library architecture (6 contract assemblies + 1 source generator + 1 core infrastructure)

**Performance Goals**:

- Event publishing: <10ms for events with up to 10 subscribers
- Event bus throughput: ≥1000 events/second
- Source generator: compile-time generation (zero runtime cost)
- Resource cache hit rate: >90%

**Constraints**:

- No breaking changes to existing IRegistry, IPlugin, IPluginContext interfaces
- Technology-agnostic contracts (no platform-specific types)
- Source generator must support generic constraints, nullable annotations, ref/out parameters
- All contracts must work across Terminal.Gui, SadConsole, and future Unity implementations

**Scale/Scope**:

- 6 contract assemblies
- 1 source generator assembly
- 93 functional requirements
- 8 user stories
- Expected: 50-200 methods across all service interfaces (all auto-generated via source generator)

## Constitution Check

*GATE: Must pass before Phase 0 research.*

**Status**: ✅ **PASSED** (No constitution file exists - using project conventions from CLAUDE.md)

**Project Conventions Check**:

- ✅ **R-DOC**: Documentation will be created in `docs/_inbox/` with YAML front-matter
- ✅ **R-CODE**: No hardcoded secrets, meaningful names, commented non-obvious code
- ✅ **R-TST**: Critical functionality will be tested (event bus, source generator, service registration)
- ✅ **R-GIT**: Conventional commit format, no secrets

**Re-check after Phase 1**: Will verify test coverage for EventBus and source generator.

## Project Structure

### Documentation (this feature)

```
specs/007-tiered-contract-architecture/
├── spec.md                      # Feature specification (COMPLETE)
├── plan.md                      # This file
├── research.md                  # Phase 0 output (design decisions)
├── data-model.md                # Phase 1 output (entity definitions)
├── quickstart.md                # Phase 1 output (developer quick start)
├── contracts/                   # Phase 1 output (contract schemas)
│   ├── IEventBus.cs            # Event bus contract
│   ├── IService-Game.cs        # Game service contract
│   ├── IService-UI.cs          # UI service contract
│   ├── IService-Scene.cs       # Scene service contract
│   ├── IService-Input.cs       # Input service contract
│   ├── IService-Config.cs      # Config service contract
│   ├── IService-Resource.cs    # Resource service contract
│   ├── Events-Game.cs          # Game events
│   ├── Events-Scene.cs         # Scene events
│   ├── Events-Input.cs         # Input events
│   ├── Events-Config.cs        # Config events
│   └── Events-Resource.cs      # Resource events
├── checklists/
│   └── requirements.md          # Validation checklist (COMPLETE)
└── tasks.md                     # Phase 2 output (/speckit.tasks - NOT created by this command)
```

### Source Code (repository root)

```
dotnet/framework/
├── LablabBean.Plugins.Contracts/           # EXISTING - Tier 1 base
│   ├── IRegistry.cs                        # EXISTING
│   ├── ServiceMetadata.cs                  # EXISTING
│   ├── SelectionMode.cs                    # EXISTING
│   ├── IEventBus.cs                        # NEW (FR-001)
│   ├── RealizeServiceAttribute.cs          # NEW (FR-078)
│   └── SelectionStrategyAttribute.cs       # NEW (FR-079)
│
├── LablabBean.Contracts.Game/              # EXISTING (partial) - Tier 1
│   ├── Services/
│   │   └── Interfaces/
│   │       └── IService.cs                 # UPDATE with FR-030 to FR-032
│   ├── GameEvents.cs                       # NEW (FR-023: EntitySpawnedEvent, etc.)
│   └── GameModels.cs                       # UPDATE (EntitySnapshot, GameState, etc.)
│
├── LablabBean.Contracts.UI/                # EXISTING (partial) - Tier 1
│   ├── Services/
│   │   └── Interfaces/
│   │       └── IService.cs                 # UPDATE with FR-033
│   └── UIEvents.cs                         # NEW (FR-024)
│
├── LablabBean.Contracts.Scene/             # EXISTING (empty) - Tier 1
│   ├── Services/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (FR-036 to FR-041)
│   ├── SceneEvents.cs                      # NEW (FR-025: SceneLoadedEvent, etc.)
│   └── SceneModels.cs                      # NEW (FR-042: Camera, Viewport, etc.)
│
├── LablabBean.Contracts.Input/             # EXISTING (empty) - Tier 1
│   ├── Router/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (FR-043 to FR-047)
│   ├── Mapper/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (FR-048 to FR-049)
│   ├── Scope/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (Scope interface)
│   ├── InputEvents.cs                      # NEW (FR-026: RawInputReceivedEvent, etc.)
│   └── InputModels.cs                      # NEW (FR-050: RawKeyEvent, InputAction, etc.)
│
├── LablabBean.Contracts.Config/            # EXISTING (empty) - Tier 1
│   ├── Services/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (FR-051 to FR-057)
│   ├── ConfigEvents.cs                     # NEW (FR-027: ConfigChangedEvent, etc.)
│   └── ConfigModels.cs                     # NEW (FR-058: IConfigSection, etc.)
│
├── LablabBean.Contracts.Resource/          # EXISTING (empty) - Tier 1
│   ├── Services/
│   │   └── Interfaces/
│   │       └── IService.cs                 # NEW (FR-059 to FR-065)
│   ├── ResourceEvents.cs                   # NEW (FR-028: ResourceLoadedEvent, etc.)
│   └── ResourceModels.cs                   # NEW (FR-066: ResourceMetadata, etc.)
│
├── LablabBean.SourceGenerators.Proxy/      # NEW - Tier 2
│   ├── ProxyServiceGenerator.cs            # NEW (FR-067 to FR-076)
│   ├── ProxyServiceSyntaxReceiver.cs       # NEW (helper for source generator)
│   └── LablabBean.SourceGenerators.Proxy.csproj
│
└── LablabBean.Plugins.Core/                # EXISTING - Tier 2
    ├── ServiceRegistry.cs                  # EXISTING
    ├── EventBus.cs                         # NEW (FR-002 to FR-008)
    ├── PluginHost.cs                       # UPDATE (register EventBus as singleton)
    └── PluginContext.cs                    # EXISTING
```

**Structure Decision**: Multi-assembly architecture with clear tier separation:

- **Tier 1 Base** (`LablabBean.Plugins.Contracts`): Foundation contracts (IRegistry, IEventBus, attributes)
- **Tier 1 Domains** (6 assemblies): Domain-specific contracts (Game, UI, Scene, Input, Config, Resource)
- **Tier 2 Infrastructure** (`LablabBean.Plugins.Core` + `LablabBean.SourceGenerators.Proxy`): EventBus implementation + source generator
- **Tier 3+** (plugins): Implementations of contracts (not part of this spec)

## Complexity Tracking

*No violations - architecture aligns with existing plugin system patterns.*

## Phase 0: Research & Design Decisions

See [research.md](./research.md) for detailed research findings.

**Key Decisions**:

1. **Event Bus Pattern**: Publish-subscribe with sequential execution (not parallel) to maintain predictable ordering
2. **Source Generator Approach**: Roslyn incremental source generation (same as cross-milo)
3. **Namespace Convention**: Generic `IService` within domain namespace (e.g., `LablabBean.Contracts.Scene.Services.IService`)
4. **Event Naming**: `{Subject}{Action}Event` pattern with timestamp
5. **Proxy Service Pattern**: Partial class with `[RealizeService]` and `[SelectionStrategy]` attributes
6. **Input Scope Stack**: LIFO stack-based input routing for modal UI

## Phase 1: Contracts & Data Model

See [data-model.md](./data-model.md) for entity definitions and [contracts/](./contracts/) for service interface schemas.

**Deliverables**:

- IEventBus contract (Tier 1 base)
- 6 domain service contracts (Game, UI, Scene, Input, Config, Resource)
- Event definitions for all domains
- Supporting types (models) for all domains
- Source generator attributes ([RealizeService], [SelectionStrategy])

## Implementation Phases

### Phase 1: EventBus Foundation (Priority: P0)

**Goal**: Enable event-driven communication between plugins

**Tasks**:

1. Add `IEventBus` interface to `LablabBean.Plugins.Contracts` (FR-001)
2. Implement `EventBus` class in `LablabBean.Plugins.Core` (FR-002 to FR-008)
3. Register EventBus as singleton in `PluginHost` (FR-006)
4. Write unit tests for EventBus (subscribe, publish, error handling)
5. Write integration tests (multi-subscriber scenarios)

**Success Criteria**: SC-001 to SC-004

**Dependencies**: None (builds on existing IRegistry)

---

### Phase 2: Source Generator + Attributes (Priority: P0)

**Goal**: Eliminate proxy service boilerplate

**Tasks**:

1. Create `LablabBean.SourceGenerators.Proxy` project (FR-067)
2. Add `[RealizeService]` attribute to `LablabBean.Plugins.Contracts` (FR-078)
3. Add `[SelectionStrategy]` attribute to `LablabBean.Plugins.Contracts` (FR-079)
4. Implement `ProxyServiceGenerator` class (FR-068 to FR-076)
5. Implement `ProxyServiceSyntaxReceiver` class
6. Write unit tests for source generator (method/property/event generation)
7. Write integration tests (compile test projects with generated code)

**Success Criteria**: SC-012 to SC-014, SC-018

**Dependencies**: Phase 1 (needs IRegistry for generated delegation code)

---

### Phase 3: Game & UI Contract Events (Priority: P1)

**Goal**: Add event-driven patterns to existing contracts

**Tasks**:

1. Add event definitions to `LablabBean.Contracts.Game` (FR-023)
2. Add event definitions to `LablabBean.Contracts.UI` (FR-024)
3. Update Game `IService` interface (FR-030 to FR-032)
4. Update UI `IService` interface (FR-033)
5. Write example proxy service using source generator
6. Write unit tests for event publishing/subscribing
7. Update documentation with examples

**Success Criteria**: SC-005, SC-006

**Dependencies**: Phase 1 (EventBus), Phase 2 (source generator)

---

### Phase 4: Scene & Input Contracts (Priority: P1)

**Goal**: Enable dungeon/level management and input handling

**Tasks**:

1. Create `LablabBean.Contracts.Scene` service interface (FR-036 to FR-042)
2. Create Scene events (FR-025)
3. Create Scene models (Camera, Viewport, CameraViewport)
4. Create `LablabBean.Contracts.Input` router service (FR-043 to FR-047)
5. Create `LablabBean.Contracts.Input` mapper service (FR-048 to FR-049)
6. Create Input events (FR-026)
7. Create Input models (RawKeyEvent, InputAction, InputScope) (FR-050)
8. Write unit tests for scope stacking and action mapping
9. Update documentation with scene/input examples

**Success Criteria**: SC-007, SC-008

**Dependencies**: Phase 1 (EventBus), Phase 2 (source generator)

---

### Phase 5: Config & Resource Contracts (Priority: P1)

**Goal**: Enable configuration management and resource loading

**Tasks**:

1. Create `LablabBean.Contracts.Config` service interface (FR-051 to FR-058)
2. Create Config events (FR-027)
3. Create Config models (IConfigSection, ConfigChangedEventArgs)
4. Create `LablabBean.Contracts.Resource` service interface (FR-059 to FR-066)
5. Create Resource events (FR-028)
6. Create Resource models (ResourceMetadata, ResourceLoadOptions)
7. Write unit tests for config reload and resource caching
8. Update documentation with config/resource examples

**Success Criteria**: SC-009, SC-010, SC-011

**Dependencies**: Phase 1 (EventBus), Phase 2 (source generator)

---

### Phase 6: Documentation & Examples (Priority: P2)

**Goal**: Comprehensive developer documentation

**Tasks**:

1. Write developer guide with event subscription examples (FR-090)
2. Write developer guide with service contract examples (FR-091)
3. Write developer guide with Scene/Input/Config/Resource examples (FR-092)
4. Write developer guide with source generator usage (FR-093)
5. Create quickstart guide for plugin developers
6. Update CLAUDE.md with contract patterns
7. Validate documentation against SC-015, SC-016, SC-017

**Success Criteria**: SC-015, SC-016, SC-017

**Dependencies**: Phases 1-5 complete

---

## Testing Strategy

### Unit Tests

- EventBus: Subscribe, publish, error handling, no subscribers
- Source Generator: Method generation, property generation, event generation, generic constraints, nullable annotations
- Each contract: Event definitions, model validation

### Integration Tests

- Event bus with multiple subscribers across plugins
- Source generator compiling real projects
- Service registration and retrieval via IRegistry
- Input scope push/pop lifecycle
- Resource cache hit/miss scenarios

### Contract Tests

- Each service interface has a contract test verifying proxy generation works
- Event schemas validated (timestamp, immutability)

## Migration Strategy

### Backward Compatibility

- ✅ Existing `IRegistry`, `IPlugin`, `IPluginContext` interfaces unchanged (FR-087)
- ✅ Existing plugins continue to work without modification (FR-088)
- ✅ New contract assemblies added alongside existing code (FR-088)

### Phased Rollout

1. **Phase 1-2**: Infrastructure only (EventBus + source generator) - no existing code changes
2. **Phase 3**: Update existing Game/UI contracts with events - existing plugins unaffected
3. **Phase 4-5**: Add new Scene/Input/Config/Resource contracts - opt-in for plugins
4. **Phase 6**: Documentation - enables plugin developers to adopt patterns

### Risk Mitigation

- Each phase is independently testable and deployable
- Source generator failures fall back to compile errors (no runtime impact)
- Event bus errors are isolated per subscriber (FR-004)
- All new assemblies are additive (no deletions)

## Performance Considerations

### EventBus

- Sequential execution prevents race conditions
- Subscribers execute async (non-blocking)
- Error isolation (one failing subscriber doesn't break others)
- Target: <10ms publish time, ≥1000 events/second

### Source Generator

- Incremental compilation (only regenerates changed files)
- Compile-time generation (zero runtime cost)
- Target: <30 seconds for full rebuild with 50+ service methods

### Resource Caching

- LRU eviction strategy (to be implemented)
- Concurrent load deduplication (to be implemented)
- Target: >90% cache hit rate

## Open Questions

*All technical decisions resolved in Phase 0 research. No NEEDS CLARIFICATION items remain.*

## Next Steps

1. ✅ **Spec Complete**: 93 functional requirements documented
2. ✅ **Plan Complete**: This document
3. 🔄 **Phase 0**: Create research.md with design decisions
4. 🔄 **Phase 1**: Create data-model.md and contracts/
5. 🔄 **Phase 2**: Run `/speckit.tasks` to generate task list
6. ⏭️ **Implementation**: Execute tasks from tasks.md

---

**Plan Status**: ✅ READY FOR PHASE 0 (RESEARCH)
**Next Command**: Continue with research.md generation in this session
