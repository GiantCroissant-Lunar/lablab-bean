# Tasks: Tiered Contract Architecture

**Input**: Design documents from `/specs/007-tiered-contract-architecture/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Tests are included per spec requirements (FR-033 through FR-037, SC-003, SC-004, SC-008)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- Plugin framework architecture (multi-assembly)
- Paths: `dotnet/framework/`, `dotnet/tests/`, `plugins/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for new contract assemblies

- [ ] T001 Create `LablabBean.Contracts.Game` project structure in `dotnet/framework/LablabBean.Contracts.Game/`
- [ ] T002 Create `LablabBean.Contracts.UI` project structure in `dotnet/framework/LablabBean.Contracts.UI/`
- [ ] T003 [P] Create test project `LablabBean.Contracts.Game.Tests` in `dotnet/tests/LablabBean.Contracts.Game.Tests/`
- [ ] T004 [P] Create test project `LablabBean.Contracts.UI.Tests` in `dotnet/tests/LablabBean.Contracts.UI.Tests/`
- [ ] T005 [P] Configure project references (contracts → `LablabBean.Plugins.Contracts` only)
- [ ] T006 [P] Add projects to solution file and build configuration

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 Add `IEventBus` interface to `dotnet/framework/LablabBean.Plugins.Contracts/IEventBus.cs`
- [ ] T008 Implement `EventBus` class in `dotnet/framework/LablabBean.Plugins.Core/EventBus.cs`
- [ ] T009 Register `EventBus` as singleton in `dotnet/framework/LablabBean.Plugins.Core/ServiceCollectionExtensions.cs`
- [ ] T010 [P] Write unit test: Subscribe and publish single event in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- [ ] T011 [P] Write unit test: Multiple subscribers receive same event in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- [ ] T012 [P] Write unit test: Subscriber exception doesn't affect others in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- [ ] T013 [P] Write unit test: No subscribers completes successfully in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- [ ] T014 [P] Write unit test: Concurrent publishing from multiple threads in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusTests.cs`
- [ ] T015 Run all EventBus tests and verify they pass

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Event-Driven Analytics Plugin (Priority: P1) 🎯 MVP

**Goal**: Enable plugin developers to create analytics plugins that track game events without direct dependency on game plugin

**Independent Test**: Create a simple event publisher in one plugin, a subscriber in another plugin, and verify the event is received without any direct dependency between the plugins

**Success Criteria** (from spec.md):
- Event bus publishes events to all subscribers asynchronously
- Analytics plugin receives game events without referencing game plugin assembly
- Multiple subscribers receive events without errors from one subscriber affecting others

### Game Event Definitions for User Story 1

- [ ] T016 [P] [US1] Create `Position` record in `dotnet/framework/LablabBean.Contracts.Game/Models/Position.cs`
- [ ] T017 [P] [US1] Create `EntitySpawnedEvent` record in `dotnet/framework/LablabBean.Contracts.Game/Events/EntitySpawnedEvent.cs`
- [ ] T018 [P] [US1] Create `EntityMovedEvent` record in `dotnet/framework/LablabBean.Contracts.Game/Events/EntityMovedEvent.cs`
- [ ] T019 [P] [US1] Create `CombatEvent` record in `dotnet/framework/LablabBean.Contracts.Game/Events/CombatEvent.cs`

### Analytics Plugin Implementation for User Story 1

- [ ] T020 [US1] Create analytics plugin project in `plugins/LablabBean.Plugins.Analytics/`
- [ ] T021 [US1] Add project references (Analytics → `LablabBean.Plugins.Contracts`, `LablabBean.Contracts.Game`)
- [ ] T022 [US1] Implement `AnalyticsPlugin` class with event subscriptions in `plugins/LablabBean.Plugins.Analytics/AnalyticsPlugin.cs`
- [ ] T023 [US1] Subscribe to `EntitySpawnedEvent` and track spawn count
- [ ] T024 [US1] Subscribe to `CombatEvent` and track combat count
- [ ] T025 [US1] Add logging for analytics tracking

### Integration Test for User Story 1

- [ ] T026 [US1] Create integration test: Publisher plugin publishes event, analytics plugin receives it in `dotnet/tests/LablabBean.Plugins.Core.Tests/AnalyticsPluginIntegrationTests.cs`
- [ ] T027 [US1] Verify analytics plugin tracks events without direct game plugin dependency
- [ ] T028 [US1] Verify multiple subscribers receive events without interference

**Checkpoint**: At this point, User Story 1 should be fully functional - analytics plugin can track game events via event bus

---

## Phase 4: User Story 2 - Game Service Contract (Priority: P2)

**Goal**: Enable plugin developers to define game service interfaces in contract assemblies separate from implementations, allowing multiple platform-specific implementations

**Independent Test**: Create the `LablabBean.Contracts.Game` assembly with service interfaces, implement a simple mock service and register it via `IRegistry`, and verify it can be retrieved by other plugins

**Success Criteria** (from spec.md):
- Game service interface defined in contract assembly
- Multiple implementations can be registered with different priorities
- Plugins can retrieve service using `SelectionMode.HighestPriority`

### Game Service Contract for User Story 2

- [ ] T029 [P] [US2] Create `GameState` record in `dotnet/framework/LablabBean.Contracts.Game/Models/GameState.cs`
- [ ] T030 [P] [US2] Create `GameStateType` enum in `dotnet/framework/LablabBean.Contracts.Game/Models/GameState.cs`
- [ ] T031 [P] [US2] Create `EntitySnapshot` record in `dotnet/framework/LablabBean.Contracts.Game/Models/EntitySnapshot.cs`
- [ ] T032 [P] [US2] Create `CombatResult` record in `dotnet/framework/LablabBean.Contracts.Game/Models/CombatResult.cs`
- [ ] T033 [P] [US2] Create `GameStartOptions` record in `dotnet/framework/LablabBean.Contracts.Game/Models/GameStartOptions.cs`
- [ ] T034 [P] [US2] Create `GameStateChangedEvent` record in `dotnet/framework/LablabBean.Contracts.Game/Events/GameStateChangedEvent.cs`
- [ ] T035 [US2] Create game service interface `IService` in `dotnet/framework/LablabBean.Contracts.Game/Services/IService.cs`
- [ ] T036 [US2] Add XML documentation to all game service methods

### Mock Game Service Implementation for User Story 2

- [ ] T037 [US2] Create mock game service plugin project in `plugins/LablabBean.Plugins.MockGame/`
- [ ] T038 [US2] Add project references (MockGame → contracts)
- [ ] T039 [US2] Implement `MockGameService` class in `plugins/LablabBean.Plugins.MockGame/MockGameService.cs`
- [ ] T040 [US2] Implement `StartGameAsync` with event publishing
- [ ] T041 [US2] Implement `SpawnEntityAsync` with event publishing
- [ ] T042 [US2] Implement `MoveEntityAsync` with event publishing
- [ ] T043 [US2] Implement `AttackAsync` with event publishing
- [ ] T044 [US2] Implement `GetGameState` and `GetEntities` methods
- [ ] T045 [US2] Register service via `IRegistry` with priority 200 in plugin's `InitializeAsync`

### Contract Validation Tests for User Story 2

- [ ] T046 [P] [US2] Write contract test: Verify service interface follows naming conventions in `dotnet/tests/LablabBean.Contracts.Game.Tests/GameServiceContractTests.cs`
- [ ] T047 [P] [US2] Write contract test: Verify all methods are async where appropriate in `dotnet/tests/LablabBean.Contracts.Game.Tests/GameServiceContractTests.cs`
- [ ] T048 [P] [US2] Write contract test: Verify events follow record pattern with timestamp in `dotnet/tests/LablabBean.Contracts.Game.Tests/GameServiceContractTests.cs`
- [ ] T049 [US2] Write integration test: Register service and retrieve via `IRegistry.Get<IService>()` in `dotnet/tests/LablabBean.Plugins.Core.Tests/GameServiceIntegrationTests.cs`
- [ ] T050 [US2] Write integration test: Multiple implementations with priority selection in `dotnet/tests/LablabBean.Plugins.Core.Tests/GameServiceIntegrationTests.cs`

**Checkpoint**: At this point, User Story 2 should be fully functional - game service contract is defined and mock implementation works via IRegistry

---

## Phase 5: User Story 3 - UI Plugin with Events (Priority: P2)

**Goal**: Enable plugin developers to create UI rendering plugins that automatically update when game state changes via event subscriptions

**Independent Test**: Publish game state change events from a mock game service, have the UI plugin subscribe to those events, and verify the UI updates are triggered without the UI polling the game state

**Success Criteria** (from spec.md):
- UI plugin subscribes to game events and updates display reactively
- UI handles multiple events asynchronously without blocking game loop
- System handles missing subscriber gracefully when UI plugin is unloaded

### UI Service Contract for User Story 3

- [ ] T051 [P] [US3] Create `InputCommand` record in `dotnet/framework/LablabBean.Contracts.UI/Models/InputCommand.cs`
- [ ] T052 [P] [US3] Create `InputType` enum in `dotnet/framework/LablabBean.Contracts.UI/Models/InputCommand.cs`
- [ ] T053 [P] [US3] Create `ViewportBounds` record in `dotnet/framework/LablabBean.Contracts.UI/Models/ViewportBounds.cs`
- [ ] T054 [P] [US3] Create `UIInitOptions` record in `dotnet/framework/LablabBean.Contracts.UI/Models/UIInitOptions.cs`
- [ ] T055 [P] [US3] Create `InputReceivedEvent` record in `dotnet/framework/LablabBean.Contracts.UI/Events/InputReceivedEvent.cs`
- [ ] T056 [P] [US3] Create `ViewportChangedEvent` record in `dotnet/framework/LablabBean.Contracts.UI/Events/ViewportChangedEvent.cs`
- [ ] T057 [US3] Create UI service interface `IService` in `dotnet/framework/LablabBean.Contracts.UI/Services/IService.cs`
- [ ] T058 [US3] Add XML documentation to all UI service methods

### Reactive UI Plugin Implementation for User Story 3

- [ ] T059 [US3] Create reactive UI plugin project in `plugins/LablabBean.Plugins.ReactiveUI/`
- [ ] T060 [US3] Add project references (ReactiveUI → contracts)
- [ ] T061 [US3] Implement `ReactiveUIService` class in `plugins/LablabBean.Plugins.ReactiveUI/ReactiveUIService.cs`
- [ ] T062 [US3] Subscribe to `EntityMovedEvent` and mark display for redraw
- [ ] T063 [US3] Subscribe to `EntitySpawnedEvent` and mark display for redraw
- [ ] T064 [US3] Subscribe to `CombatEvent` and mark display for redraw
- [ ] T065 [US3] Implement `RenderViewportAsync` method
- [ ] T066 [US3] Implement `UpdateDisplayAsync` method with redraw logic
- [ ] T067 [US3] Implement `HandleInputAsync` with event publishing
- [ ] T068 [US3] Register service via `IRegistry` with priority 100 in plugin's `InitializeAsync`

### Contract Validation Tests for User Story 3

- [ ] T069 [P] [US3] Write contract test: Verify UI service interface follows naming conventions in `dotnet/tests/LablabBean.Contracts.UI.Tests/UIServiceContractTests.cs`
- [ ] T070 [P] [US3] Write contract test: Verify all methods are async where appropriate in `dotnet/tests/LablabBean.Contracts.UI.Tests/UIServiceContractTests.cs`
- [ ] T071 [US3] Write integration test: UI plugin receives game events and updates display in `dotnet/tests/LablabBean.Plugins.Core.Tests/ReactiveUIIntegrationTests.cs`
- [ ] T072 [US3] Write integration test: UI handles rapid event succession without blocking in `dotnet/tests/LablabBean.Plugins.Core.Tests/ReactiveUIIntegrationTests.cs`
- [ ] T073 [US3] Write integration test: System handles missing UI subscriber gracefully in `dotnet/tests/LablabBean.Plugins.Core.Tests/ReactiveUIIntegrationTests.cs`

**Checkpoint**: At this point, User Story 3 should be fully functional - UI plugin reactively updates based on game events

---

## Phase 6: User Story 4 - Scene Management Contracts (Priority: P3)

**Goal**: Enable plugin developers to create scene/level management systems with contracts for loading dungeons, managing cameras, and handling transitions

**Independent Test**: Create the `LablabBean.Contracts.Scene` assembly, implement a basic scene loader, and verify scene load/unload operations work through the contract interface

**Note**: Per spec.md "Out of Scope", Scene contract assembly is deferred to future iteration. This phase is included for completeness but marked as optional.

**Status**: ⚠️ DEFERRED - Not part of initial implementation per spec.md

- [ ] T074 [US4] [DEFERRED] Create `LablabBean.Contracts.Scene` project structure (future iteration)
- [ ] T075 [US4] [DEFERRED] Define scene service interface (future iteration)
- [ ] T076 [US4] [DEFERRED] Define scene events (SceneLoadedEvent, SceneUnloadedEvent) (future iteration)

**Checkpoint**: User Story 4 is deferred to future iteration per spec.md

---

## Phase 7: Performance Validation & Testing

**Purpose**: Validate performance requirements from spec.md success criteria

**Success Criteria**:
- SC-003: Event publishing completes in under 10ms for events with up to 10 subscribers
- SC-004: Event bus handles at least 1000 events per second without blocking the game loop

- [ ] T077 [P] Write performance test: Measure event publishing latency with 10 subscribers in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusPerformanceTests.cs`
- [ ] T078 [P] Write performance test: Measure event throughput (events/second) in `dotnet/tests/LablabBean.Plugins.Core.Tests/EventBusPerformanceTests.cs`
- [ ] T079 Run performance tests and verify SC-003 (<10ms) and SC-004 (1000 events/sec) are met
- [ ] T080 If performance targets not met, implement object pooling for high-frequency events in `dotnet/framework/LablabBean.Plugins.Core/EventBus.cs`
- [ ] T081 Re-run performance tests after optimization

---

## Phase 8: Documentation & Developer Experience

**Purpose**: Complete developer documentation and ensure SC-008 (plugin creation in <30 minutes)

**Success Criteria**:
- SC-007: Documentation includes at least 3 complete examples of event-driven plugin patterns
- SC-008: A developer unfamiliar with the codebase can create a working event-subscribing plugin in under 30 minutes using the documentation

- [ ] T082 [P] Copy quickstart.md examples to main documentation in `docs/plugins/event-driven-development.md`
- [ ] T083 [P] Create example plugin: Analytics plugin (from quickstart) in `plugins/examples/LablabBean.Plugins.Analytics.Example/`
- [ ] T084 [P] Create example plugin: Custom game service (from quickstart) in `plugins/examples/LablabBean.Plugins.CustomGame.Example/`
- [ ] T085 [P] Create example plugin: Reactive UI (from quickstart) in `plugins/examples/LablabBean.Plugins.ReactiveUI.Example/`
- [ ] T086 Update main README.md with link to event-driven development guide
- [ ] T087 Add plugin manifest examples to documentation
- [ ] T088 Validate quickstart.md by following it step-by-step (time yourself, should be <30 minutes)

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [ ] T089 [P] Add XML documentation comments to all public APIs in contract assemblies
- [ ] T090 [P] Run code formatter and linter across all new code
- [ ] T091 Verify backward compatibility: Run existing plugin tests to ensure no breaking changes
- [ ] T092 [P] Update CHANGELOG.md with new features and breaking changes (should be none)
- [ ] T093 [P] Create migration guide for existing plugins to adopt event-driven patterns in `docs/plugins/migration-to-events.md`
- [ ] T094 Review all error messages for clarity and actionability
- [ ] T095 Add logging for event bus operations (subscribe, publish, error)
- [ ] T096 Run full test suite across all assemblies
- [ ] T097 Validate all success criteria from spec.md (SC-001 through SC-008)
- [ ] T098 Final review: Ensure all FR requirements (FR-001 through FR-037) are implemented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User Story 1 (Analytics Plugin): Can start after Foundational - No dependencies on other stories
  - User Story 2 (Game Service Contract): Can start after Foundational - No dependencies on other stories
  - User Story 3 (UI Plugin with Events): Can start after Foundational - May use events from US1, but independently testable
  - User Story 4 (Scene Management): DEFERRED to future iteration
- **Performance Validation (Phase 7)**: Depends on Foundational (EventBus implementation)
- **Documentation (Phase 8)**: Can start after any user story is complete, but best after US1-US3
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - ✅ No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - ✅ No dependencies on other stories
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - ✅ Uses events from US1 but independently testable
- **User Story 4 (P3)**: DEFERRED - Not part of initial implementation

### Within Each User Story

- Event/model definitions before service implementations
- Service implementations before plugin implementations
- Plugin implementations before integration tests
- Tests should pass before moving to next story

### Parallel Opportunities

**Phase 1 (Setup)**:
- T003 and T004 (test project creation) can run in parallel
- T005 and T006 (configuration) can run in parallel

**Phase 2 (Foundational)**:
- T010-T014 (all EventBus unit tests) can run in parallel after T008 is complete

**Phase 3 (User Story 1)**:
- T016-T019 (all event definitions) can run in parallel
- T026-T028 (all integration tests) can run in parallel after implementation

**Phase 4 (User Story 2)**:
- T029-T034 (all model/event definitions) can run in parallel
- T046-T048 (all contract tests) can run in parallel

**Phase 5 (User Story 3)**:
- T051-T056 (all model/event definitions) can run in parallel
- T069-T070 (contract tests) can run in parallel

**Phase 7 (Performance)**:
- T077 and T078 (performance tests) can run in parallel

**Phase 8 (Documentation)**:
- T082-T085 (all example plugins) can run in parallel

**Phase 9 (Polish)**:
- T089, T090, T092, T093 (documentation tasks) can run in parallel

**Cross-Story Parallelism**:
- After Foundational phase completes, User Stories 1, 2, and 3 can all be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all event definitions for User Story 1 together:
Task T016: "Create Position record in dotnet/framework/LablabBean.Contracts.Game/Models/Position.cs"
Task T017: "Create EntitySpawnedEvent record in dotnet/framework/LablabBean.Contracts.Game/Events/EntitySpawnedEvent.cs"
Task T018: "Create EntityMovedEvent record in dotnet/framework/LablabBean.Contracts.Game/Events/EntityMovedEvent.cs"
Task T019: "Create CombatEvent record in dotnet/framework/LablabBean.Contracts.Game/Events/CombatEvent.cs"

# Then launch all integration tests together after implementation:
Task T026: "Create integration test: Publisher plugin publishes event, analytics plugin receives it"
Task T027: "Verify analytics plugin tracks events without direct game plugin dependency"
Task T028: "Verify multiple subscribers receive events without interference"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (6 tasks)
2. Complete Phase 2: Foundational (9 tasks) - CRITICAL - blocks all stories
3. Complete Phase 3: User Story 1 (13 tasks)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Demo analytics plugin tracking game events
6. **Decision Point**: Ship MVP or continue to User Story 2

**MVP Scope**: 28 tasks (Setup + Foundational + US1)  
**Estimated Time**: 1-2 days for experienced developer

### Incremental Delivery

1. **Foundation** (Phases 1-2): Setup + EventBus → Foundation ready (15 tasks)
2. **MVP** (Phase 3): Add User Story 1 → Test independently → Demo analytics plugin (13 tasks)
3. **Iteration 2** (Phase 4): Add User Story 2 → Test independently → Demo game service contracts (22 tasks)
4. **Iteration 3** (Phase 5): Add User Story 3 → Test independently → Demo reactive UI (25 tasks)
5. **Polish** (Phases 7-9): Performance + Documentation + Polish (20 tasks)

Each iteration adds value without breaking previous stories.

### Parallel Team Strategy

With multiple developers:

1. **Team completes Setup + Foundational together** (Phases 1-2)
2. **Once Foundational is done, parallelize**:
   - Developer A: User Story 1 (Analytics Plugin)
   - Developer B: User Story 2 (Game Service Contract)
   - Developer C: User Story 3 (UI Plugin with Events)
3. **Reconvene for integration**: Verify all stories work together
4. **Final push**: Performance + Documentation + Polish

---

## Task Summary

### Total Tasks: 98 tasks
- **Phase 1 (Setup)**: 6 tasks
- **Phase 2 (Foundational)**: 9 tasks ⚠️ BLOCKS all user stories
- **Phase 3 (User Story 1 - Analytics)**: 13 tasks 🎯 MVP
- **Phase 4 (User Story 2 - Game Service)**: 22 tasks
- **Phase 5 (User Story 3 - UI Plugin)**: 25 tasks
- **Phase 6 (User Story 4 - Scene)**: 3 tasks (DEFERRED)
- **Phase 7 (Performance)**: 5 tasks
- **Phase 8 (Documentation)**: 7 tasks
- **Phase 9 (Polish)**: 10 tasks

### Tasks by User Story
- **US1 (Analytics Plugin)**: 13 tasks
- **US2 (Game Service Contract)**: 22 tasks
- **US3 (UI Plugin with Events)**: 25 tasks
- **US4 (Scene Management)**: 3 tasks (DEFERRED)

### Parallel Opportunities Identified
- **Setup phase**: 4 tasks can run in parallel
- **Foundational phase**: 5 tests can run in parallel
- **User Story 1**: 4 event definitions + 3 tests can run in parallel
- **User Story 2**: 6 models/events + 3 contract tests can run in parallel
- **User Story 3**: 6 models/events + 2 contract tests can run in parallel
- **Cross-story**: All 3 user stories can be worked on in parallel after Foundational phase

### Independent Test Criteria
- **US1**: Create event publisher and subscriber in separate plugins, verify event delivery without direct dependency
- **US2**: Register service via IRegistry, retrieve via Get<IService>(), verify priority-based selection works
- **US3**: Publish game events, verify UI plugin receives them and updates display without polling

### Suggested MVP Scope
**Phases 1-3 only** (Setup + Foundational + User Story 1)
- **28 tasks total**
- **Delivers**: Event bus + Analytics plugin tracking game events
- **Validates**: Core event-driven architecture works
- **Time estimate**: 1-2 days

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Foundational phase (Phase 2) is CRITICAL - blocks all user stories
- User Story 4 (Scene Management) is DEFERRED per spec.md "Out of Scope"
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Performance targets: <10ms event publishing, 1000 events/second (SC-003, SC-004)
- Documentation target: Plugin creation in <30 minutes (SC-008)

---

**Generated**: 2025-10-21  
**Command**: `/speckit.tasks`  
**Based on**: spec.md (4 user stories), plan.md, research.md, data-model.md, contracts/
