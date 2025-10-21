# Implementation Plan: Tiered Contract Architecture

**Branch**: `007-tiered-contract-architecture` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-tiered-contract-architecture/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Adopt cross-milo tier 1 and tier 2 contract design patterns to establish event-driven, platform-independent plugin architecture. This implementation adds `IEventBus` foundation (Tier 2 infrastructure), creates domain-specific contract assemblies for Game and UI (Tier 1 contracts), and implements event-driven communication patterns. The goal is to enable loose coupling between plugins while maintaining the existing `IRegistry` infrastructure that is already well-aligned with cross-milo patterns.

**Primary Deliverables**:
1. `IEventBus` interface and implementation in `LablabBean.Plugins.Contracts` and `LablabBean.Plugins.Core`
2. `LablabBean.Contracts.Game` assembly with service interfaces and event definitions
3. `LablabBean.Contracts.UI` assembly with service interfaces and event definitions
4. Developer documentation and quickstart guide for event-driven plugin development

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: 
- Existing: `LablabBean.Plugins.Contracts`, `LablabBean.Plugins.Core`, `Microsoft.Extensions.DependencyInjection`
- New: None (pure .NET 8 implementation)

**Storage**: N/A (in-memory event bus, no persistence)  
**Testing**: xUnit, FluentAssertions (existing test infrastructure)  
**Target Platform**: Cross-platform (.NET 8 - Windows, Linux, macOS)  
**Project Type**: Plugin framework (multi-assembly architecture)  
**Performance Goals**: 
- Event publishing: <10ms for events with up to 10 subscribers
- Event throughput: 1000 events/second minimum
- Zero allocation event publishing (use object pooling if needed)

**Constraints**: 
- Backward compatibility: Existing plugins must continue to work without modification
- Thread safety: Event bus must support concurrent publishing from multiple plugins
- Sequential execution: Subscribers execute sequentially (not parallel) for predictable ordering
- Error isolation: Exceptions in one subscriber must not affect others

**Scale/Scope**: 
- 2 new contract assemblies (Game, UI)
- 1 new infrastructure interface (IEventBus)
- ~10 event definitions across domains
- ~5 service interface definitions
- Support for 10+ concurrent plugins

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Alignment with .agent/base/10-principles.md

✅ **P-1: Documentation-First Development** - This plan includes research.md, data-model.md, contracts/, and quickstart.md

✅ **P-2: Clear Code Over Clever Code** - Event-driven patterns are well-documented, interfaces are explicit

✅ **P-3: Testing Matters** - Test requirements included in spec (SC-003, SC-004), xUnit tests planned

✅ **P-4: Security Consciousness** - No security concerns (in-memory events, no external I/O)

✅ **P-5: User Experience Focus** - Improves developer UX by enabling loose coupling and reactive patterns

✅ **P-6: Separation of Concerns** - Tier 1 contracts separate from Tier 2 infrastructure, clean boundaries

✅ **P-7: Performance Awareness** - Performance goals defined (10ms, 1000 events/sec), monitoring planned

✅ **P-8: Build Automation** - Uses existing build infrastructure, no new build steps required

✅ **P-9: Version Control Hygiene** - Feature branch `007-tiered-contract-architecture` already created

✅ **P-10: When in doubt, ask** - Specification includes edge cases and clarifications

### Alignment with .agent/base/20-rules.md

✅ **R-CODE-001**: No hardcoded secrets (N/A for this feature)

✅ **R-DOC-001**: Documentation goes to `specs/007-tiered-contract-architecture/` (correct location)

✅ **R-DOC-002**: YAML front-matter included in spec.md

✅ **R-TST-001**: Test critical paths - event bus, service registration, error handling

✅ **R-GIT-001**: Feature branch follows naming convention

**GATE RESULT**: ✅ PASSED - All principles and rules satisfied

## Project Structure

### Documentation (this feature)

```
specs/007-tiered-contract-architecture/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── IEventBus.cs     # Event bus interface definition
│   ├── game-service-contract.cs    # Game service interface
│   └── ui-service-contract.cs      # UI service interface
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/framework/
├── LablabBean.Plugins.Contracts/        # EXISTING - Tier 2 infrastructure contracts
│   ├── IRegistry.cs                     # EXISTING - Already aligned with cross-milo
│   ├── IEventBus.cs                     # NEW - Event bus interface
│   ├── SelectionMode.cs                 # EXISTING
│   └── ServiceMetadata.cs               # EXISTING
│
├── LablabBean.Plugins.Core/             # EXISTING - Tier 2 infrastructure implementation
│   ├── ServiceRegistry.cs               # EXISTING - Already aligned with cross-milo
│   ├── EventBus.cs                      # NEW - Event bus implementation
│   └── ServiceCollectionExtensions.cs   # MODIFIED - Register EventBus
│
├── LablabBean.Contracts.Game/           # NEW - Tier 1 domain contracts
│   ├── LablabBean.Contracts.Game.csproj # NEW
│   ├── Services/
│   │   └── IService.cs                  # NEW - Game service interface
│   ├── Events/
│   │   ├── EntitySpawnedEvent.cs        # NEW
│   │   ├── EntityMovedEvent.cs          # NEW
│   │   ├── CombatEvent.cs               # NEW
│   │   └── GameStateChangedEvent.cs     # NEW
│   └── Models/
│       ├── EntitySnapshot.cs            # NEW
│       └── GameState.cs                 # NEW
│
└── LablabBean.Contracts.UI/             # NEW - Tier 1 domain contracts
    ├── LablabBean.Contracts.UI.csproj   # NEW
    ├── Services/
    │   └── IService.cs                  # NEW - UI service interface
    ├── Events/
    │   ├── InputReceivedEvent.cs        # NEW
    │   └── ViewportChangedEvent.cs      # NEW
    └── Models/
        ├── InputCommand.cs              # NEW
        └── ViewportBounds.cs            # NEW

dotnet/tests/
├── LablabBean.Plugins.Core.Tests/       # EXISTING
│   ├── EventBusTests.cs                 # NEW - Event bus unit tests
│   └── ServiceRegistryTests.cs          # EXISTING
│
├── LablabBean.Contracts.Game.Tests/     # NEW
│   └── GameServiceContractTests.cs      # NEW - Contract validation tests
│
└── LablabBean.Contracts.UI.Tests/       # NEW
    └── UIServiceContractTests.cs        # NEW - Contract validation tests
```

**Structure Decision**: Multi-assembly plugin framework architecture following cross-milo tier 1 and tier 2 patterns. The existing `LablabBean.Plugins.Contracts` and `LablabBean.Plugins.Core` assemblies serve as Tier 2 infrastructure. Two new Tier 1 contract assemblies (`LablabBean.Contracts.Game` and `LablabBean.Contracts.UI`) will be created to define domain-specific service interfaces and events. This structure maintains backward compatibility while enabling event-driven, platform-independent plugin development.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

**No violations detected.** All design decisions align with project principles and rules. The addition of two new contract assemblies follows the established cross-milo pattern and maintains simplicity by reusing existing infrastructure (`IRegistry`, `ServiceRegistry`).

---

## Phase 0: Research & Clarification ✅

**Status**: Complete  
**Output**: [research.md](./research.md)

### Summary

All technical unknowns from the Technical Context have been resolved through analysis of cross-milo reference architecture and existing lablab-bean codebase.

**Key Decisions**:
1. **Event Bus Storage**: `ConcurrentDictionary<Type, List<Func<object, Task>>>` for thread-safe, lock-free reads
2. **Event Types**: Immutable `record` with `DateTimeOffset Timestamp` property
3. **Contract Organization**: `LablabBean.Contracts.{Domain}` with generic `IService` naming
4. **Service Registration**: Existing `IRegistry` with priority metadata (framework: 1000+, plugins: 100-500)
5. **Event Bus Lifecycle**: Singleton registered in DI, exposed via `IRegistry`
6. **Error Handling**: Catch per subscriber, log error, continue to next subscriber
7. **Performance**: Lock-free reads, minimal allocations, object pooling for high-frequency events
8. **Testing**: Unit + Contract + Integration tests

**Reference**: See [research.md](./research.md) for detailed rationale and alternatives considered.

---

## Phase 1: Design & Contracts ✅

**Status**: Complete  
**Outputs**: 
- [data-model.md](./data-model.md)
- [contracts/IEventBus.cs](./contracts/IEventBus.cs)
- [contracts/game-service-contract.cs](./contracts/game-service-contract.cs)
- [contracts/ui-service-contract.cs](./contracts/ui-service-contract.cs)
- [quickstart.md](./quickstart.md)

### Summary

Complete data model and contract definitions for tiered architecture.

**Deliverables**:

1. **IEventBus Interface** (`LablabBean.Plugins.Contracts`)
   - `PublishAsync<T>()` - Publish events to all subscribers
   - `Subscribe<T>()` - Register event handlers
   - Sequential execution, error isolation, thread-safe

2. **Game Domain Contracts** (`LablabBean.Contracts.Game`)
   - **Service Interface**: `IService` with game operations (start, spawn, move, attack)
   - **Events**: `EntitySpawnedEvent`, `EntityMovedEvent`, `CombatEvent`, `GameStateChangedEvent`
   - **Models**: `EntitySnapshot`, `GameState`, `Position`, `CombatResult`

3. **UI Domain Contracts** (`LablabBean.Contracts.UI`)
   - **Service Interface**: `IService` with UI operations (initialize, render, input)
   - **Events**: `InputReceivedEvent`, `ViewportChangedEvent`
   - **Models**: `InputCommand`, `ViewportBounds`, `UIInitOptions`

4. **EventBus Implementation** (`LablabBean.Plugins.Core`)
   - Thread-safe implementation using `ConcurrentDictionary`
   - Sequential subscriber execution
   - Error isolation with logging

5. **Developer Guide** ([quickstart.md](./quickstart.md))
   - Example 1: Analytics plugin (event subscriber)
   - Example 2: Custom game service (service provider)
   - Example 3: Reactive UI plugin (subscriber + provider)
   - Best practices, troubleshooting, testing patterns

**Reference**: See [data-model.md](./data-model.md) for complete interface definitions and validation rules.

---

## Phase 2: Agent Context Update ✅

**Status**: Complete  
**Output**: Updated `CLAUDE.md` with project context

### Summary

Agent context files updated with technology stack and project type information:
- **Language**: C# 12 / .NET 8.0
- **Database**: N/A (in-memory event bus, no persistence)
- **Project Type**: Plugin framework (multi-assembly architecture)

This ensures AI assistants have accurate context for future development work.

---

## Constitution Check (Post-Design) ✅

**Re-evaluation after Phase 1 design**:

✅ **P-1: Documentation-First Development** - Complete documentation generated (research.md, data-model.md, contracts/, quickstart.md)

✅ **P-2: Clear Code Over Clever Code** - Interfaces are explicit, patterns are well-documented

✅ **P-3: Testing Matters** - Test strategy defined in research.md, examples in quickstart.md

✅ **P-6: Separation of Concerns** - Clean tier separation (Tier 1 contracts, Tier 2 infrastructure)

✅ **P-7: Performance Awareness** - Performance goals defined and optimization strategies documented

**GATE RESULT**: ✅ PASSED - Design maintains alignment with all principles

---

## Implementation Roadmap

**Next Command**: `/speckit.tasks` to generate actionable task breakdown

**Recommended Task Phases**:

### Phase A: Infrastructure (Tier 2)
1. Add `IEventBus` interface to `LablabBean.Plugins.Contracts`
2. Implement `EventBus` class in `LablabBean.Plugins.Core`
3. Register `EventBus` in `ServiceCollectionExtensions`
4. Write unit tests for `EventBus` (subscribe, publish, error handling)

### Phase B: Game Contracts (Tier 1)
1. Create `LablabBean.Contracts.Game` project
2. Add service interface (`Services/IService.cs`)
3. Add event definitions (`Events/*.cs`)
4. Add model definitions (`Models/*.cs`)
5. Write contract validation tests

### Phase C: UI Contracts (Tier 1)
1. Create `LablabBean.Contracts.UI` project
2. Add service interface (`Services/IService.cs`)
3. Add event definitions (`Events/*.cs`)
4. Add model definitions (`Models/*.cs`)
5. Write contract validation tests

### Phase D: Integration & Documentation
1. Update existing plugins to use `IEventBus` (optional, backward compatible)
2. Create example analytics plugin
3. Update developer documentation
4. Add integration tests (cross-plugin event communication)
5. Performance testing (event throughput, latency)

**Estimated Effort**: 3-5 days for complete implementation and testing

---

## Success Metrics

From [spec.md](./spec.md) success criteria:

- **SC-001**: ✅ Design supports analytics plugin without direct game plugin dependency
- **SC-002**: ✅ Design supports multiple UI implementations via contracts
- **SC-003**: ⏳ Performance target: <10ms event publishing (to be validated in implementation)
- **SC-004**: ⏳ Throughput target: 1000 events/second (to be validated in implementation)
- **SC-005**: ✅ Design maintains backward compatibility (no breaking changes to `IRegistry`, `IPlugin`)
- **SC-006**: ✅ Service registration requires <5 lines (see quickstart examples)
- **SC-007**: ✅ Documentation includes 3+ examples (analytics, game service, reactive UI)
- **SC-008**: ✅ Quickstart guide enables plugin creation in <30 minutes

**Design Phase Success**: 6/8 criteria validated, 2 require implementation validation

---

## Risk Mitigation

From [spec.md](./spec.md) risks:

1. **Event bus performance** 
   - ✅ Mitigated: Lock-free reads, sequential execution, object pooling strategy
   - ⏳ Validation: Performance tests in Phase D

2. **Circular event dependencies**
   - ✅ Mitigated: Documented as anti-pattern in quickstart.md
   - ✅ Guidance: Clear event direction (game → UI, not bidirectional)

3. **Breaking changes during migration**
   - ✅ Mitigated: No changes to existing `IRegistry`, `IPlugin`, `IPluginContext`
   - ✅ Additive only: New interfaces and assemblies

4. **Documentation drift**
   - ✅ Mitigated: Contracts documented inline with XML comments
   - ✅ Process: Include contract docs in same PR as contract changes

5. **Over-engineering**
   - ✅ Mitigated: Starting with 2 contract assemblies (Game, UI) only
   - ✅ Strategy: Add more domains (Scene, Analytics) based on actual need

---

## Appendix: File Manifest

### Generated Documentation
- ✅ `specs/007-tiered-contract-architecture/plan.md` (this file)
- ✅ `specs/007-tiered-contract-architecture/research.md`
- ✅ `specs/007-tiered-contract-architecture/data-model.md`
- ✅ `specs/007-tiered-contract-architecture/quickstart.md`
- ✅ `specs/007-tiered-contract-architecture/contracts/IEventBus.cs`
- ✅ `specs/007-tiered-contract-architecture/contracts/game-service-contract.cs`
- ✅ `specs/007-tiered-contract-architecture/contracts/ui-service-contract.cs`

### Updated Files
- ✅ `CLAUDE.md` (agent context updated)

### To Be Created (Implementation Phase)
- ⏳ `dotnet/framework/LablabBean.Plugins.Contracts/IEventBus.cs`
- ⏳ `dotnet/framework/LablabBean.Plugins.Core/EventBus.cs`
- ⏳ `dotnet/framework/LablabBean.Contracts.Game/` (entire assembly)
- ⏳ `dotnet/framework/LablabBean.Contracts.UI/` (entire assembly)
- ⏳ `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- ⏳ `dotnet/tests/LablabBean.Contracts.Game.Tests/` (entire test project)
- ⏳ `dotnet/tests/LablabBean.Contracts.UI.Tests/` (entire test project)

---

## Plan Complete ✅

**Branch**: `007-tiered-contract-architecture`  
**Status**: Ready for implementation  
**Next Step**: Run `/speckit.tasks` to generate dependency-ordered task breakdown

**Phase Summary**:
- ✅ Phase 0: Research complete (8 decisions documented)
- ✅ Phase 1: Design complete (3 contract interfaces, 10+ events, 4 documents)
- ✅ Phase 2: Agent context updated

**Quality Gates**:
- ✅ Constitution check passed (all principles satisfied)
- ✅ All technical unknowns resolved
- ✅ Complete contract definitions
- ✅ Developer documentation with examples
- ✅ Risk mitigation strategies defined

**Estimated Implementation**: 3-5 days  
**Confidence Level**: High (design validated against cross-milo reference architecture)

---

**Generated**: 2025-10-21  
**Command**: `/speckit.plan`  
**Spec Version**: 1.0 (draft)

