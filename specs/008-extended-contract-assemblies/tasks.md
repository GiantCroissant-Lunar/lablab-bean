# Tasks: Extended Contract Assemblies

**Input**: Design documents from `/specs/008-extended-contract-assemblies/`
**Prerequisites**: Spec 007 (Tiered Contract Architecture) ✅ Complete

**Tests**: Tests are included per spec requirements (FR-054 through FR-069, SC-001 through SC-008)

**Organization**: Tasks are grouped by phase and contract domain to enable sequential implementation.

## Format: `[ID] [P?] [Phase] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Phase]**: Which phase this task belongs to (e.g., P1, P2, P3, P4)
- Include exact file paths in descriptions

## Path Conventions
- Contract assemblies: `dotnet/framework/LablabBean.Contracts.{Domain}/`
- Test projects: `dotnet/tests/LablabBean.Contracts.{Domain}.Tests/`
- Example plugins: `plugins/LablabBean.Plugins.{Name}/`

---

## Phase 0: Setup (Shared Infrastructure)

**Purpose**: Project initialization and solution configuration

- [ ] T001 Add `LablabBean.Contracts.Scene` project to solution file
- [ ] T002 Add `LablabBean.Contracts.Input` project to solution file
- [ ] T003 Add `LablabBean.Contracts.Config` project to solution file
- [ ] T004 Add `LablabBean.Contracts.Resource` project to solution file
- [ ] T005 [P] Add test projects to solution file (4 test projects)
- [ ] T006 [P] Add example plugin projects to solution file (4 plugins)
- [ ] T007 Configure build order and dependencies in solution

---

## Phase 1: Scene Contract (Priority: P1) 🎯

**Goal**: Enable scene/level loading with camera and viewport management

**Success Criteria**:
- Scene loading <100ms (up to 100 entities)
- Camera positioning works correctly
- Scene transition events published in correct order

### Project Setup

- [ ] T008 Create `LablabBean.Contracts.Scene` project in `dotnet/framework/LablabBean.Contracts.Scene/`
- [ ] T009 Add reference to `LablabBean.Plugins.Contracts`
- [ ] T010 Configure project properties (TargetFramework: net8.0, nullable: enable)
- [ ] T011 Create `Services/` directory

### Service Interface

- [ ] T012 Create `Services/IService.cs` with scene management methods
  - `Task LoadSceneAsync(string sceneId, CancellationToken ct = default)`
  - `Task UnloadSceneAsync(string sceneId, CancellationToken ct = default)`
  - `Viewport GetViewport()`
  - `CameraViewport GetCameraViewport()`
  - `void SetCamera(Camera camera)`
  - `void UpdateWorld(IReadOnlyList<EntitySnapshot> snapshots)`
  - `event EventHandler<SceneShutdownEventArgs>? Shutdown`

### Models

- [ ] T013 [P] Create `Models.cs` with supporting types
  - `Camera` record (Position, Zoom)
  - `Viewport` record (Width, Height)
  - `CameraViewport` record (Camera, Viewport)
  - `EntitySnapshot` record (if not in Game contract)

### Events

- [ ] T014 [P] Create `Events.cs` with event definitions
  - `SceneLoadedEvent` record (SceneId, Timestamp)
  - `SceneUnloadedEvent` record (SceneId, Timestamp)
  - `SceneLoadFailedEvent` record (SceneId, Error, Timestamp)

### Tests

- [ ] T015 Create test project `LablabBean.Contracts.Scene.Tests` in `dotnet/tests/`
- [ ] T016 [P] Write unit test: Scene service interface contract validation
- [ ] T017 [P] Write unit test: Event immutability and timestamp validation
- [ ] T018 [P] Write unit test: Model validation (Camera, Viewport)
- [ ] T019 [P] Write performance test: Scene loading time <100ms
- [ ] T020 Run all Scene contract tests and verify they pass

### Example Plugin

- [ ] T021 Create `LablabBean.Plugins.SceneLoader` project in `plugins/`
- [ ] T022 Implement basic scene loader service
- [ ] T023 Add scene loading logic with event publishing
- [ ] T024 Register service with priority in plugin initialization
- [ ] T025 Write integration test: Scene loader with event bus

### Documentation

- [ ] T026 Add XML documentation to all public APIs
- [ ] T027 Update developer guide with Scene contract section
- [ ] T028 Add code examples to documentation

**Checkpoint**: Scene contract complete - can load/unload scenes with camera control

---

## Phase 2: Input Contract (Priority: P1)

**Goal**: Enable scope-based input routing and action mapping

**Success Criteria**:
- Input scope stack no memory leaks (1,000 cycles)
- Modal UI correctly captures input
- Action mapping works correctly

### Project Setup

- [ ] T029 Create `LablabBean.Contracts.Input` project in `dotnet/framework/LablabBean.Contracts.Input/`
- [ ] T030 Add reference to `LablabBean.Plugins.Contracts`
- [ ] T031 Configure project properties
- [ ] T032 Create `Router/` and `Mapper/` directories

### Router Service Interface

- [ ] T033 Create `Router/IService.cs` with generic input routing
  - `IDisposable PushScope(IInputScope<TInputEvent> scope)`
  - `void Dispatch(TInputEvent inputEvent)`
  - `IInputScope<TInputEvent>? Top { get; }`

### Mapper Service Interface

- [ ] T034 Create `Mapper/IService.cs` with action mapping
  - `void Map(RawKeyEvent rawKey)`
  - `bool TryGetAction(string actionName, out InputAction action)`
  - `void RegisterMapping(string actionName, RawKeyEvent key)`

### Models

- [ ] T035 [P] Create `Models.cs` with supporting types
  - `RawKeyEvent` record (Key, Modifiers)
  - `InputEvent` record (Command, Timestamp)
  - `InputAction` record (Name, Metadata)
  - `IInputScope<TInputEvent>` interface (Handle method)

### Events

- [ ] T036 [P] Create `Events.cs` with event definitions
  - `InputActionTriggeredEvent` record (ActionName, Timestamp)
  - `InputScopePushedEvent` record (ScopeName, Timestamp)
  - `InputScopePoppedEvent` record (ScopeName, Timestamp)

### Tests

- [ ] T037 Create test project `LablabBean.Contracts.Input.Tests` in `dotnet/tests/`
- [ ] T038 [P] Write unit test: Router service interface contract validation
- [ ] T039 [P] Write unit test: Mapper service interface contract validation
- [ ] T040 [P] Write unit test: Input scope stack push/pop operations
- [ ] T041 [P] Write unit test: Topmost scope receives input
- [ ] T042 [P] Write memory leak test: 1,000 push/pop cycles (use dotMemory)
- [ ] T043 [P] Write unit test: IDisposable cleanup on exception
- [ ] T044 Run all Input contract tests and verify they pass

### Example Plugin

- [ ] T045 Create `LablabBean.Plugins.InputHandler` project in `plugins/`
- [ ] T046 Implement input router service with scope stack
- [ ] T047 Implement input mapper service with action mapping
- [ ] T048 Add example modal input scope (inventory screen)
- [ ] T049 Register services with priority in plugin initialization
- [ ] T050 Write integration test: Modal UI captures input correctly

### Documentation

- [ ] T051 Add XML documentation to all public APIs
- [ ] T052 Update developer guide with Input contract section
- [ ] T053 Add modal UI example to documentation

**Checkpoint**: Input contract complete - modal UI works with scope-based routing

---

## Phase 3: Config Contract (Priority: P1)

**Goal**: Enable configuration management with change notifications

**Success Criteria**:
- Config change events <10ms latency
- Typed value retrieval works correctly
- Hierarchical sections work correctly

### Project Setup

- [ ] T054 Create `LablabBean.Contracts.Config` project in `dotnet/framework/LablabBean.Contracts.Config/`
- [ ] T055 Add reference to `LablabBean.Plugins.Contracts`
- [ ] T056 Configure project properties
- [ ] T057 Create `Services/` directory

### Service Interface

- [ ] T058 Create `Services/IService.cs` with config operations
  - `string? Get(string key)`
  - `T? Get<T>(string key)`
  - `IConfigSection GetSection(string key)`
  - `void Set(string key, string value)`
  - `bool Exists(string key)`
  - `Task ReloadAsync(CancellationToken ct = default)`
  - `event EventHandler<ConfigChangedEventArgs>? ConfigChanged`

### Models

- [ ] T059 [P] Create `Models.cs` with supporting types
  - `IConfigSection` interface (Get, Set, GetSection methods)
  - `ConfigChangedEventArgs` class (Key, OldValue, NewValue)

### Events

- [ ] T060 [P] Create `Events.cs` with event definitions
  - `ConfigChangedEvent` record (Key, OldValue, NewValue, Timestamp)
  - `ConfigReloadedEvent` record (Timestamp)

### Tests

- [ ] T061 Create test project `LablabBean.Contracts.Config.Tests` in `dotnet/tests/`
- [ ] T062 [P] Write unit test: Config service interface contract validation
- [ ] T063 [P] Write unit test: Get/Set operations
- [ ] T064 [P] Write unit test: Typed value retrieval with conversion
- [ ] T065 [P] Write unit test: Hierarchical sections (colon separator)
- [ ] T066 [P] Write latency test: Config change events <10ms
- [ ] T067 [P] Write unit test: Thread-safe concurrent reads/writes
- [ ] T068 Run all Config contract tests and verify they pass

### Example Plugin

- [ ] T069 Create `LablabBean.Plugins.ConfigManager` project in `plugins/`
- [ ] T070 Implement config service with in-memory storage
- [ ] T071 Add hierarchical section support
- [ ] T072 Add change notification with event publishing
- [ ] T073 Register service with priority in plugin initialization
- [ ] T074 Write integration test: Config changes trigger events

### Documentation

- [ ] T075 Add XML documentation to all public APIs
- [ ] T076 Update developer guide with Config contract section
- [ ] T077 Add configuration examples to documentation

**Checkpoint**: Config contract complete - game settings work with change notifications

---

## Phase 4: Resource Contract (Priority: P1)

**Goal**: Enable async resource loading with caching

**Success Criteria**:
- Resource service handles 50+ concurrent loads
- Resource cache >90% hit rate
- Circular dependency detection works

### Project Setup

- [ ] T078 Create `LablabBean.Contracts.Resource` project in `dotnet/framework/LablabBean.Contracts.Resource/`
- [ ] T079 Add reference to `LablabBean.Plugins.Contracts`
- [ ] T080 Configure project properties
- [ ] T081 Create `Services/` directory

### Service Interface

- [ ] T082 Create `Services/IService.cs` with resource operations
  - `Task<T> LoadAsync<T>(string resourceId, CancellationToken ct = default)`
  - `void Unload(string resourceId)`
  - `bool IsLoaded(string resourceId)`
  - `Task PreloadAsync(IEnumerable<string> resourceIds, CancellationToken ct = default)`
  - `void ClearCache()`

### Events

- [ ] T083 [P] Create `Events.cs` with event definitions
  - `ResourceLoadedEvent` record (ResourceId, Timestamp)
  - `ResourceLoadFailedEvent` record (ResourceId, Error, Timestamp)
  - `ResourceUnloadedEvent` record (ResourceId, Timestamp)
  - `ResourceCacheClearedEvent` record (Timestamp)

### Tests

- [ ] T084 Create test project `LablabBean.Contracts.Resource.Tests` in `dotnet/tests/`
- [ ] T085 [P] Write unit test: Resource service interface contract validation
- [ ] T086 [P] Write unit test: LoadAsync returns cached resource on second call
- [ ] T087 [P] Write unit test: Unload removes resource from cache
- [ ] T088 [P] Write unit test: IsLoaded returns correct status
- [ ] T089 [P] Write concurrency test: 50+ concurrent LoadAsync calls
- [ ] T090 [P] Write unit test: Concurrent requests deduplicated (same Task returned)
- [ ] T091 [P] Write cache test: Hit rate >90% for repeated loads
- [ ] T092 [P] Write unit test: Circular dependency detection
- [ ] T093 [P] Write unit test: PreloadAsync loads multiple resources concurrently
- [ ] T094 Run all Resource contract tests and verify they pass

### Example Plugin

- [ ] T095 Create `LablabBean.Plugins.ResourceLoader` project in `plugins/`
- [ ] T096 Implement resource service with LRU cache
- [ ] T097 Add async loading with deduplication
- [ ] T098 Add circular dependency detection
- [ ] T099 Add event publishing for load success/failure
- [ ] T100 Register service with priority in plugin initialization
- [ ] T101 Write integration test: Resource loading with caching

### Documentation

- [ ] T102 Add XML documentation to all public APIs
- [ ] T103 Update developer guide with Resource contract section
- [ ] T104 Add resource loading examples to documentation

**Checkpoint**: Resource contract complete - assets load efficiently with caching

---

## Phase 5: Integration & Documentation (Final Polish)

**Goal**: Ensure all 4 contracts work together seamlessly

### Integration Tests

- [ ] T105 Create integration test project `LablabBean.Plugins.Core.Tests.Integration`
- [ ] T106 Write integration test: Scene + Input (camera follows player input)
- [ ] T107 Write integration test: Config + All contracts (config changes affect behavior)
- [ ] T108 Write integration test: Resource + Scene (scene loads dungeon data)
- [ ] T109 Write integration test: All 4 contracts together (complete dungeon workflow)
- [ ] T110 Run all integration tests and verify they pass

### Performance Validation

- [ ] T111 Run performance benchmarks for all success criteria
  - Scene loading <100ms ✅
  - Input scope no memory leaks ✅
  - Config change events <10ms ✅
  - Resource 50+ concurrent loads ✅
  - Resource cache >90% hit rate ✅
- [ ] T112 Document performance results in `performance-results.md`

### Documentation

- [ ] T113 Create comprehensive example using all 4 contracts
- [ ] T114 Update main README with new contracts section
- [ ] T115 Create quickstart guide for new contracts
- [ ] T116 Add troubleshooting section to developer guide
- [ ] T117 Update CHANGELOG with Spec 008 features
- [ ] T118 Create completion report (COMPLETION.md)

### Final Validation

- [ ] T119 Verify all 69 functional requirements implemented
- [ ] T120 Verify all 8 success criteria validated
- [ ] T121 Verify all edge cases handled
- [ ] T122 Run full test suite (all contracts + integration)
- [ ] T123 Code review and quality check
- [ ] T124 Update spec.md status to "Complete"

**Checkpoint**: All 4 contracts complete, tested, and documented

---

## Parallel Execution Strategy

### Phase 1 (Scene) - Sequential
- Must complete before other phases (establishes patterns)

### Phase 2 (Input) - After Phase 1
- Depends on understanding scene context
- Can start once Scene contract patterns are established

### Phase 3 (Config) - After Phase 1
- Independent of Input
- Can run in parallel with Phase 2 if multiple developers

### Phase 4 (Resource) - After Phase 1
- Independent of Input and Config
- Can run in parallel with Phases 2-3 if multiple developers

### Phase 5 (Integration) - After Phases 1-4
- Requires all 4 contracts complete
- Sequential (integration tests depend on all contracts)

---

## Task Summary

### Total Tasks: 124 tasks

**By Phase**:
- **Phase 0 (Setup)**: 7 tasks
- **Phase 1 (Scene)**: 21 tasks
- **Phase 2 (Input)**: 25 tasks
- **Phase 3 (Config)**: 24 tasks
- **Phase 4 (Resource)**: 27 tasks
- **Phase 5 (Integration)**: 20 tasks

**By Type**:
- **Project Setup**: 16 tasks
- **Implementation**: 40 tasks
- **Testing**: 48 tasks
- **Documentation**: 20 tasks

**Parallel Opportunities**:
- Phase 0: 3 tasks can run in parallel (test/plugin projects)
- Phase 1: 6 tasks can run in parallel (models, events, tests)
- Phase 2: 8 tasks can run in parallel (models, events, tests)
- Phase 3: 6 tasks can run in parallel (models, events, tests)
- Phase 4: 10 tasks can run in parallel (events, tests)
- **Total**: 33 tasks can run in parallel (27% of tasks)

### Estimated Duration

**Single Developer**:
- Phase 0: 0.5 days
- Phase 1: 2-3 days
- Phase 2: 2-3 days
- Phase 3: 1-2 days
- Phase 4: 2-3 days
- Phase 5: 1-2 days
- **Total**: 9-14 days

**Two Developers** (with parallelization):
- Phase 0: 0.5 days
- Phase 1: 2-3 days (sequential)
- Phases 2-4: 3-4 days (parallel)
- Phase 5: 1-2 days
- **Total**: 6.5-9.5 days

### Critical Path

```
Setup (0.5d) → Scene (2-3d) → Input (2-3d) → Integration (1-2d)
                              ↘ Config (1-2d) ↗
                              ↘ Resource (2-3d) ↗
```

---

## Notes

- **[P] tasks** = different files, no dependencies, can run in parallel
- **Phase labels** map tasks to implementation phases
- Each phase should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate phase independently
- Performance targets from success criteria must be validated in tests
- All contracts follow patterns established in Spec 007

---

**Generated**: 2025-10-22  
**Command**: Manual generation based on spec.md and plan.md  
**Based on**: spec.md (4 user stories, 69 requirements), plan.md (4 phases)
