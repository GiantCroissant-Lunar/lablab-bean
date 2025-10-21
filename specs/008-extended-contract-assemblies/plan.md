# Implementation Plan: Extended Contract Assemblies

**Spec**: 008-extended-contract-assemblies  
**Created**: 2025-10-22  
**Status**: Draft  
**Prerequisites**: Spec 007 (Tiered Contract Architecture) - Complete ✅

## Executive Summary

This plan outlines the implementation of 4 additional contract assemblies (Scene, Input, Config, Resource) that extend the event-driven plugin architecture from Spec 007. The implementation is organized into 4 sequential phases, each delivering a complete contract assembly with tests and documentation.

**Total Estimated Duration**: 7-11 days  
**Team Size**: 1-2 developers  
**Risk Level**: Medium

## Architecture Overview

### System Context

```
┌─────────────────────────────────────────────────────────────┐
│                    Lablab-Bean Application                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Spec 007 Foundation (Complete)                │  │
│  │  ┌──────────────┐  ┌──────────────┐                  │  │
│  │  │  IEventBus   │  │  IRegistry   │                  │  │
│  │  └──────────────┘  └──────────────┘                  │  │
│  │  ┌──────────────┐  ┌──────────────┐                  │  │
│  │  │ Game Contract│  │ UI Contract  │                  │  │
│  │  └──────────────┘  └──────────────┘                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Spec 008 Extensions (This Plan)               │  │
│  │  ┌──────────────┐  ┌──────────────┐                  │  │
│  │  │Scene Contract│  │Input Contract│                  │  │
│  │  └──────────────┘  └──────────────┘                  │  │
│  │  ┌──────────────┐  ┌──────────────┐                  │  │
│  │  │Config Contract│ │Resource Cont.│                  │  │
│  │  └──────────────┘  └──────────────┘                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Plugin Implementations                    │  │
│  │  Scene Loader │ Input Handler │ Config Mgr │ Res Mgr │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Component Dependencies

```
LablabBean.Plugins.Contracts (Tier 2)
├── IEventBus
├── IRegistry
└── ServiceMetadata

LablabBean.Contracts.Scene (Tier 1)
├── Depends on: Plugins.Contracts
├── Services/IService
├── Events: SceneLoadedEvent, SceneUnloadedEvent, SceneLoadFailedEvent
└── Models: Camera, Viewport, CameraViewport

LablabBean.Contracts.Input (Tier 1)
├── Depends on: Plugins.Contracts
├── Router/IService<TInputEvent>
├── Mapper/IService
├── Events: InputActionTriggeredEvent
└── Models: RawKeyEvent, InputEvent, InputAction

LablabBean.Contracts.Config (Tier 1)
├── Depends on: Plugins.Contracts
├── Services/IService
├── Events: ConfigChangedEvent
└── Models: IConfigSection

LablabBean.Contracts.Resource (Tier 1)
├── Depends on: Plugins.Contracts
├── Services/IService
├── Events: ResourceLoadedEvent, ResourceLoadFailedEvent
└── Models: (generic resource types)
```

## Design Decisions

### 1. Contract Assembly Structure

**Decision**: Follow the same structure as Spec 007 contracts

**Structure**:
```
LablabBean.Contracts.{Domain}/
├── LablabBean.Contracts.{Domain}.csproj
├── Events.cs                    # All event definitions
├── Models.cs                    # Supporting types
└── Services/
    └── IService.cs              # Service interface
```

**Rationale**:
- Consistency with existing contracts
- Simple and predictable structure
- Easy to navigate and understand

### 2. Event Naming Convention

**Decision**: Use `{Subject}{Action}Event` pattern from Spec 007

**Examples**:
- `SceneLoadedEvent`
- `InputActionTriggeredEvent`
- `ConfigChangedEvent`
- `ResourceLoadedEvent`

**Rationale**:
- Consistent with Spec 007
- Clear and descriptive
- Follows C# naming conventions

### 3. Service Interface Naming

**Decision**: Use generic `IService` name within domain namespace

**Examples**:
- `LablabBean.Contracts.Scene.Services.IService`
- `LablabBean.Contracts.Input.Router.IService<TInputEvent>`
- `LablabBean.Contracts.Config.Services.IService`
- `LablabBean.Contracts.Resource.Services.IService`

**Rationale**:
- Consistent with cross-milo reference architecture
- Namespace provides context
- Simplifies naming within domain

### 4. Implementation Strategy

**Decision**: Implement contracts sequentially (Scene → Input → Config → Resource)

**Rationale**:
- Scene is most complex, sets patterns for others
- Input depends on understanding scene context
- Config and Resource are simpler, can be done quickly
- Sequential reduces integration complexity

### 5. Testing Strategy

**Decision**: Unit tests for each contract, integration tests for cross-contract scenarios

**Test Coverage**:
- Unit tests: Event definitions, model validation
- Integration tests: Service registration, event publishing, cross-plugin communication
- Performance tests: Scene loading time, resource cache hit rate

**Rationale**:
- Ensures each contract works independently
- Validates contracts work together
- Meets success criteria requirements

## Implementation Phases

### Phase 1: Scene Contract (2-3 days)

**Goal**: Enable scene/level loading with camera and viewport management

**Deliverables**:
1. `LablabBean.Contracts.Scene` assembly
2. Service interface for scene management
3. Events: `SceneLoadedEvent`, `SceneUnloadedEvent`, `SceneLoadFailedEvent`
4. Models: `Camera`, `Viewport`, `CameraViewport`
5. Unit tests (10+ tests)
6. Example scene loader plugin
7. Documentation update

**Success Criteria**:
- ✅ Scene loading completes in <100ms (up to 100 entities)
- ✅ Camera positioning works correctly
- ✅ Scene transition events published in correct order

**Key Tasks**:
1. Create project structure
2. Define service interface
3. Define events and models
4. Write unit tests
5. Create example plugin
6. Update documentation

---

### Phase 2: Input Contract (2-3 days)

**Goal**: Enable scope-based input routing and action mapping

**Deliverables**:
1. `LablabBean.Contracts.Input` assembly
2. Router service interface (`IService<TInputEvent>`)
3. Mapper service interface (`IService`)
4. Events: `InputActionTriggeredEvent`
5. Models: `RawKeyEvent`, `InputEvent`, `InputAction`, `IInputScope`
6. Unit tests (15+ tests)
7. Example modal input plugin
8. Documentation update

**Success Criteria**:
- ✅ Input scope stack no memory leaks (1,000 cycles)
- ✅ Modal UI correctly captures input
- ✅ Action mapping works correctly

**Key Tasks**:
1. Create project structure
2. Define router and mapper interfaces
3. Define events and models
4. Write unit tests (including memory leak tests)
5. Create example modal plugin
6. Update documentation

---

### Phase 3: Config Contract (1-2 days)

**Goal**: Enable configuration management with change notifications

**Deliverables**:
1. `LablabBean.Contracts.Config` assembly
2. Service interface for config operations
3. Events: `ConfigChangedEvent`
4. Models: `IConfigSection`
5. Unit tests (10+ tests)
6. Example config plugin
7. Documentation update

**Success Criteria**:
- ✅ Config change events <10ms latency
- ✅ Typed value retrieval works correctly
- ✅ Hierarchical sections work correctly

**Key Tasks**:
1. Create project structure
2. Define service interface
3. Define events and models
4. Write unit tests (including latency tests)
5. Create example config plugin
6. Update documentation

---

### Phase 4: Resource Contract (2-3 days)

**Goal**: Enable async resource loading with caching

**Deliverables**:
1. `LablabBean.Contracts.Resource` assembly
2. Service interface for resource operations
3. Events: `ResourceLoadedEvent`, `ResourceLoadFailedEvent`
4. Unit tests (15+ tests)
5. Example resource loader plugin
6. Documentation update

**Success Criteria**:
- ✅ Resource service handles 50+ concurrent loads
- ✅ Resource cache >90% hit rate
- ✅ Circular dependency detection works

**Key Tasks**:
1. Create project structure
2. Define service interface
3. Define events
4. Write unit tests (including concurrency and cache tests)
5. Create example resource plugin
6. Update documentation

---

## Cross-Cutting Concerns

### Documentation

**Developer Guide Updates**:
- Add section for each new contract
- Provide complete working example using all 4 contracts
- Update quickstart guide
- Add troubleshooting section

**API Documentation**:
- XML documentation for all public APIs
- Code examples in XML docs
- Link to related contracts

### Testing

**Test Organization**:
```
dotnet/tests/
├── LablabBean.Contracts.Scene.Tests/
│   ├── ServiceInterfaceTests.cs
│   ├── EventTests.cs
│   └── ModelTests.cs
├── LablabBean.Contracts.Input.Tests/
│   ├── RouterTests.cs
│   ├── MapperTests.cs
│   ├── MemoryLeakTests.cs
│   └── EventTests.cs
├── LablabBean.Contracts.Config.Tests/
│   ├── ServiceTests.cs
│   ├── LatencyTests.cs
│   └── EventTests.cs
└── LablabBean.Contracts.Resource.Tests/
    ├── ServiceTests.cs
    ├── ConcurrencyTests.cs
    ├── CacheTests.cs
    └── EventTests.cs
```

**Test Coverage Goals**:
- Unit tests: 80%+ code coverage
- Integration tests: All cross-contract scenarios
- Performance tests: All success criteria validated

### Performance Validation

**Benchmarks**:
1. Scene loading time (target: <100ms)
2. Input scope memory usage (target: no leaks)
3. Config change latency (target: <10ms)
4. Resource concurrent loads (target: 50+ without deadlocks)
5. Resource cache hit rate (target: >90%)

**Tools**:
- BenchmarkDotNet for performance tests
- dotMemory for memory leak detection
- xUnit for unit/integration tests

## Risk Mitigation

### Risk 1: Scene Loading Performance

**Risk**: Large dungeons may exceed 100ms load time

**Mitigation**:
- Document scene size limits in spec
- Implement progressive loading in future spec if needed
- Provide performance profiling tools

**Contingency**:
- Adjust success criteria to 200ms for large scenes
- Add warning in documentation about scene complexity

### Risk 2: Input Scope Complexity

**Risk**: Nested scopes could become hard to debug

**Mitigation**:
- Add logging for scope push/pop operations
- Provide debug visualization tool
- Include scope stack inspection API

**Contingency**:
- Limit scope depth to 10 levels
- Add validation to prevent excessive nesting

### Risk 3: Config Thread Safety

**Risk**: Concurrent reads during reload could return inconsistent values

**Mitigation**:
- Use snapshot pattern (copy-on-write)
- Document thread safety guarantees
- Provide thread-safe config section implementation

**Contingency**:
- Add locking if snapshot pattern insufficient
- Document performance impact of locking

### Risk 4: Resource Memory Usage

**Risk**: Loading many large resources could exhaust memory

**Mitigation**:
- Document resource size limits
- Implement LRU cache eviction policy
- Provide memory usage monitoring

**Contingency**:
- Add configurable cache size limit
- Implement aggressive eviction for low-memory scenarios

### Risk 5: Circular Resource Dependencies

**Risk**: Resources that depend on each other could cause infinite loops

**Mitigation**:
- Implement cycle detection algorithm
- Throw clear exception with dependency chain
- Document best practices for resource dependencies

**Contingency**:
- Add timeout for resource loading
- Provide dependency graph visualization tool

## Integration Points

### With Spec 007 Components

**IEventBus Integration**:
- All 4 contracts publish events via `IEventBus`
- Events follow same pattern as Spec 007
- Subscribers can listen to events from any contract

**IRegistry Integration**:
- All service implementations registered via `IRegistry`
- Priority-based selection works across all contracts
- Multiple implementations supported per contract

**Existing Plugins**:
- Analytics plugin can subscribe to new events
- MockGame plugin can use scene/input contracts
- ReactiveUI plugin can use config change events

### With Existing Game Systems

**Scene + Game Loop**:
- Scene service coordinates with game service
- Entity updates flow through scene service
- Camera follows player entity

**Input + UI**:
- Input router coordinates with UI service
- Modal UI pushes input scopes
- Game receives input when no modal active

**Config + All Systems**:
- Game difficulty from config
- Keybindings from config
- Graphics settings from config

**Resource + All Systems**:
- Scene loads dungeon data via resource service
- UI loads sprites via resource service
- Game loads entity definitions via resource service

## Validation Checklist

### Pre-Implementation
- [x] Spec 007 is complete
- [x] Gap analysis reviewed
- [x] Design decisions documented
- [x] Risk mitigation planned

### Per Phase
- [ ] Project structure created
- [ ] Service interface defined
- [ ] Events and models defined
- [ ] Unit tests written (80%+ coverage)
- [ ] Example plugin created
- [ ] Documentation updated
- [ ] Success criteria validated

### Post-Implementation
- [ ] All 4 contracts complete
- [ ] Integration tests passing
- [ ] Performance benchmarks met
- [ ] Documentation complete
- [ ] Code review completed
- [ ] Ready for production

## Timeline

### Week 1
- **Days 1-2**: Phase 1 (Scene Contract)
- **Days 3-4**: Phase 2 (Input Contract)
- **Day 5**: Phase 3 (Config Contract)

### Week 2
- **Days 1-2**: Phase 4 (Resource Contract)
- **Days 3-4**: Integration testing and documentation
- **Day 5**: Final validation and polish

**Total**: 7-11 days (1.5-2 weeks)

## Success Metrics

### Functional Metrics
- ✅ All 69 functional requirements implemented
- ✅ All 8 success criteria validated
- ✅ All edge cases handled

### Quality Metrics
- ✅ 80%+ unit test coverage
- ✅ 100% integration test coverage
- ✅ 0 critical bugs
- ✅ All performance benchmarks met

### Documentation Metrics
- ✅ Complete API documentation (XML docs)
- ✅ Developer guide updated
- ✅ Working example for all 4 contracts
- ✅ Troubleshooting guide complete

## Appendix

### File Structure

```
dotnet/
├── framework/
│   ├── LablabBean.Contracts.Scene/
│   │   ├── LablabBean.Contracts.Scene.csproj
│   │   ├── Events.cs
│   │   ├── Models.cs
│   │   └── Services/
│   │       └── IService.cs
│   ├── LablabBean.Contracts.Input/
│   │   ├── LablabBean.Contracts.Input.csproj
│   │   ├── Events.cs
│   │   ├── Models.cs
│   │   ├── Router/
│   │   │   └── IService.cs
│   │   └── Mapper/
│   │       └── IService.cs
│   ├── LablabBean.Contracts.Config/
│   │   ├── LablabBean.Contracts.Config.csproj
│   │   ├── Events.cs
│   │   ├── Models.cs
│   │   └── Services/
│   │       └── IService.cs
│   └── LablabBean.Contracts.Resource/
│       ├── LablabBean.Contracts.Resource.csproj
│       ├── Events.cs
│       └── Services/
│           └── IService.cs
├── tests/
│   ├── LablabBean.Contracts.Scene.Tests/
│   ├── LablabBean.Contracts.Input.Tests/
│   ├── LablabBean.Contracts.Config.Tests/
│   └── LablabBean.Contracts.Resource.Tests/
└── plugins/
    ├── LablabBean.Plugins.SceneLoader/
    ├── LablabBean.Plugins.InputHandler/
    ├── LablabBean.Plugins.ConfigManager/
    └── LablabBean.Plugins.ResourceLoader/
```

### References

- **Spec 007**: [007-tiered-contract-architecture/spec.md](../007-tiered-contract-architecture/spec.md)
- **Gap Analysis**: [docs/_inbox/contract-gap-analysis.md](../../docs/_inbox/contract-gap-analysis.md)
- **Cross-Milo Reference**: `ref-projects/cross-milo/`

---

**Version**: 1.0.0  
**Status**: Draft  
**Last Updated**: 2025-10-22  
**Next Step**: Generate tasks.md
