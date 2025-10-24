# Tasks: Hierarchical Dependency Injection Container System

**Input**: Design documents from `/specs/018-hierarchical-di-container/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md structure:

- **Framework Library**: `dotnet/framework/LablabBean.DependencyInjection/`
- **Tests**: `dotnet/tests/LablabBean.DependencyInjection.Tests/`
- **Solution**: `dotnet/LablabBean.sln`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create LablabBean.DependencyInjection project in dotnet/framework/LablabBean.DependencyInjection/LablabBean.DependencyInjection.csproj
- [ ] T002 Create LablabBean.DependencyInjection.Tests project in dotnet/tests/LablabBean.DependencyInjection.Tests/LablabBean.DependencyInjection.Tests.csproj
- [ ] T003 Add Microsoft.Extensions.DependencyInjection.Abstractions 8.0+ to Directory.Packages.props
- [ ] T004 [P] Add xUnit, FluentAssertions, NSubstitute test dependencies to Directory.Packages.props
- [ ] T005 [P] Add BenchmarkDotNet dependency for performance tests to Directory.Packages.props
- [ ] T006 Add both projects to dotnet/LablabBean.sln
- [ ] T007 Create Exceptions/ directory in dotnet/framework/LablabBean.DependencyInjection/Exceptions/
- [ ] T008 [P] Create Unit/ test directory in dotnet/tests/LablabBean.DependencyInjection.Tests/Unit/
- [ ] T009 [P] Create Integration/ test directory in dotnet/tests/LablabBean.DependencyInjection.Tests/Integration/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure and exceptions that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T010 [P] Create ServiceResolutionException in dotnet/framework/LablabBean.DependencyInjection/Exceptions/ServiceResolutionException.cs
- [ ] T011 [P] Create ContainerDisposedException in dotnet/framework/LablabBean.DependencyInjection/Exceptions/ContainerDisposedException.cs
- [ ] T012 Create IHierarchicalServiceProvider interface in dotnet/framework/LablabBean.DependencyInjection/IHierarchicalServiceProvider.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Global Services Accessible Across All Scenes (Priority: P1) 🎯 MVP

**Goal**: Enable global container creation and basic service resolution without hierarchy

**Independent Test**: Register a save system service in a global container, resolve it, and verify the same singleton instance is returned on multiple calls

**Acceptance Scenarios**:

1. Register save system in global container → save system available as singleton
2. Global container services → same instance provided on each request
3. Multiple resolves of audio manager → all return same instance

### Implementation for User Story 1

- [ ] T013 [US1] Create HierarchicalServiceProvider class skeleton in dotnet/framework/LablabBean.DependencyInjection/HierarchicalServiceProvider.cs (implement IHierarchicalServiceProvider, IServiceProvider, IDisposable)
- [ ] T014 [US1] Implement constructor that accepts IServiceCollection and optional name in HierarchicalServiceProvider.cs
- [ ] T015 [US1] Implement Name, Parent, Children, Depth, IsDisposed properties in HierarchicalServiceProvider.cs (Parent=null, Children=empty, Depth=0 for root)
- [ ] T016 [US1] Create inner ServiceProvider from IServiceCollection in HierarchicalServiceProvider.cs constructor
- [ ] T017 [US1] Implement GetService(Type serviceType) to delegate to inner ServiceProvider in HierarchicalServiceProvider.cs
- [ ] T018 [US1] Implement ISupportRequiredService.GetRequiredService with clear error messages in HierarchicalServiceProvider.cs
- [ ] T019 [US1] Implement basic Dispose() method disposing inner ServiceProvider in HierarchicalServiceProvider.cs
- [ ] T020 [US1] Add ObjectDisposedException guard to GetService when IsDisposed=true in HierarchicalServiceProvider.cs
- [ ] T021 [US1] Implement GetHierarchyPath() method returning container name in HierarchicalServiceProvider.cs
- [ ] T022 [US1] Create ServiceCollectionExtensions class in dotnet/framework/LablabBean.DependencyInjection/ServiceCollectionExtensions.cs
- [ ] T023 [US1] Implement BuildHierarchicalServiceProvider extension method in ServiceCollectionExtensions.cs

### Tests for User Story 1

- [ ] T024 [P] [US1] Create HierarchicalServiceProviderTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Unit/HierarchicalServiceProviderTests.cs
- [ ] T025 [P] [US1] Test: Creating root container with services returns singleton instances in HierarchicalServiceProviderTests.cs
- [ ] T026 [P] [US1] Test: Multiple GetService calls return same singleton instance in HierarchicalServiceProviderTests.cs
- [ ] T027 [P] [US1] Test: GetService on disposed container throws ObjectDisposedException in HierarchicalServiceProviderTests.cs
- [ ] T028 [P] [US1] Test: GetRequiredService throws with clear message when service not found in HierarchicalServiceProviderTests.cs
- [ ] T029 [P] [US1] Test: BuildHierarchicalServiceProvider extension method creates valid container in HierarchicalServiceProviderTests.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - global container creation and service resolution works

---

## Phase 4: User Story 2 - Scene-Specific Services with Parent Access (Priority: P2)

**Goal**: Enable child container creation with automatic parent service resolution fallback

**Independent Test**: Create global container with save system, create child dungeon container with combat system, verify combat system resolves locally and save system resolves from parent

**Acceptance Scenarios**:

1. Child container can resolve both local and parent services
2. Service not in child → checks parent automatically
3. Two sibling containers with same interface → each gets own implementation (no conflicts)

### Implementation for User Story 2

- [ ] T030 [US2] Add children field (List<HierarchicalServiceProvider>) to HierarchicalServiceProvider.cs
- [ ] T031 [US2] Implement CreateChildContainer(Action<IServiceCollection>, string?) method in HierarchicalServiceProvider.cs
- [ ] T032 [US2] Update constructor to accept optional parent parameter in HierarchicalServiceProvider.cs
- [ ] T033 [US2] Calculate Depth property from parent (parent.Depth + 1) in HierarchicalServiceProvider.cs
- [ ] T034 [US2] Update GetService to check local ServiceProvider first, then parent recursively in HierarchicalServiceProvider.cs
- [ ] T035 [US2] Add ObjectDisposedException guard to CreateChildContainer in HierarchicalServiceProvider.cs
- [ ] T036 [US2] Update GetHierarchyPath() to build full path (e.g., "Global → Dungeon") in HierarchicalServiceProvider.cs
- [ ] T037 [US2] Track created children in children list in CreateChildContainer in HierarchicalServiceProvider.cs

### Tests for User Story 2

- [ ] T038 [P] [US2] Create ServiceResolutionTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Unit/ServiceResolutionTests.cs
- [ ] T039 [P] [US2] Test: Child container can resolve service from parent in ServiceResolutionTests.cs
- [ ] T040 [P] [US2] Test: Child container prioritizes local service over parent in ServiceResolutionTests.cs (shadowing)
- [ ] T041 [P] [US2] Test: Service not found in child or parent returns null in ServiceResolutionTests.cs
- [ ] T042 [P] [US2] Test: Two sibling containers with same interface get different instances in ServiceResolutionTests.cs
- [ ] T043 [P] [US2] Test: CreateChildContainer increments depth correctly in ServiceResolutionTests.cs
- [ ] T044 [P] [US2] Test: GetHierarchyPath returns correct full path in ServiceResolutionTests.cs
- [ ] T045 [P] [US2] Test: Cannot create child from disposed container in ServiceResolutionTests.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - hierarchical resolution works

---

## Phase 5: User Story 3 - Multi-Level Hierarchies for Complex Scenes (Priority: P3)

**Goal**: Enable deep hierarchies (Global → Dungeon → Floor) with cascading disposal

**Independent Test**: Create 3-level hierarchy (Global → Dungeon → Floor1), verify Floor1 can access all parent services, dispose Dungeon and verify both Dungeon and Floor1 are disposed

**Acceptance Scenarios**:

1. Floor1 requests service → searches Floor1 → Dungeon → Global in order
2. Unload dungeon container → all child floors automatically disposed
3. Floor1 requests Floor2 service → fails (no cross-sibling access)

### Implementation for User Story 3

- [ ] T046 [US3] Update Dispose() to recursively dispose all children in HierarchicalServiceProvider.cs
- [ ] T047 [US3] Clear children list after disposal in HierarchicalServiceProvider.cs
- [ ] T048 [US3] Make Dispose() idempotent (safe to call multiple times) in HierarchicalServiceProvider.cs
- [ ] T049 [US3] Add depth limit validation (default max depth = 10) in CreateChildContainer in HierarchicalServiceProvider.cs
- [ ] T050 [US3] Prevent circular parent references in CreateChildContainer in HierarchicalServiceProvider.cs

### Tests for User Story 3

- [ ] T051 [P] [US3] Create DisposalTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Unit/DisposalTests.cs
- [ ] T052 [P] [US3] Test: Disposing parent disposes all children recursively in DisposalTests.cs
- [ ] T053 [P] [US3] Test: Disposing child does not affect parent or siblings in DisposalTests.cs
- [ ] T054 [P] [US3] Test: Dispose is idempotent (multiple calls safe) in DisposalTests.cs
- [ ] T055 [P] [US3] Test: Deep hierarchy (4+ levels) resolves services correctly in DisposalTests.cs
- [ ] T056 [P] [US3] Test: CreateChildContainer throws when depth exceeds max limit in DisposalTests.cs
- [ ] T057 [P] [US3] Create MultiLevelHierarchyTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Integration/MultiLevelHierarchyTests.cs
- [ ] T058 [US3] Integration test: Global → Dungeon → Floor hierarchy scenario in MultiLevelHierarchyTests.cs
- [ ] T059 [US3] Integration test: Cascading disposal in multi-level hierarchy in MultiLevelHierarchyTests.cs

**Checkpoint**: All core hierarchical functionality is complete - deep hierarchies with disposal work

---

## Phase 6: User Story 4 - MSDI Compatibility for Existing Code (Priority: P2)

**Goal**: Full MSDI interface compatibility with service lifetimes (Singleton, Scoped, Transient)

**Independent Test**: Register services with different lifetimes, build container, verify all MSDI lifetime semantics are preserved

**Acceptance Scenarios**:

1. Build from IServiceCollection → implements IServiceProvider correctly
2. Third-party library receives container → works without knowing about hierarchy
3. Singleton, Scoped, Transient lifetimes → respected per MSDI semantics

### Implementation for User Story 4

- [ ] T060 [US4] Implement IServiceScope interface in HierarchicalServiceScope.cs in dotnet/framework/LablabBean.DependencyInjection/HierarchicalServiceScope.cs
- [ ] T061 [US4] Implement IServiceScopeFactory in HierarchicalServiceProvider.cs
- [ ] T062 [US4] Implement CreateScope() method returning HierarchicalServiceScope in HierarchicalServiceProvider.cs
- [ ] T063 [US4] Update HierarchicalServiceScope to delegate scope to inner ServiceProvider in HierarchicalServiceScope.cs
- [ ] T064 [US4] Implement HierarchicalServiceProviderFactory class in dotnet/framework/LablabBean.DependencyInjection/HierarchicalServiceProviderFactory.cs
- [ ] T065 [US4] Implement IServiceProviderFactory<IServiceCollection> in HierarchicalServiceProviderFactory.cs
- [ ] T066 [US4] Add BuildHierarchicalServiceProvider overload with validateScopes parameter in ServiceCollectionExtensions.cs
- [ ] T067 [US4] Update inner ServiceProvider creation to support ServiceProviderOptions in HierarchicalServiceProvider.cs

### Tests for User Story 4

- [ ] T068 [P] [US4] Create ServiceLifetimeTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Unit/ServiceLifetimeTests.cs
- [ ] T069 [P] [US4] Test: Singleton services return same instance across hierarchy in ServiceLifetimeTests.cs
- [ ] T070 [P] [US4] Test: Scoped services are scoped to container level in ServiceLifetimeTests.cs
- [ ] T071 [P] [US4] Test: Transient services return new instance every time in ServiceLifetimeTests.cs
- [ ] T072 [P] [US4] Test: CreateScope returns valid IServiceScope in ServiceLifetimeTests.cs
- [ ] T073 [P] [US4] Test: Scoped service disposal when scope disposed in ServiceLifetimeTests.cs
- [ ] T074 [P] [US4] Test: IServiceProviderFactory creates valid containers in ServiceLifetimeTests.cs
- [ ] T075 [P] [US4] Test: Factory-based registration (ImplementationFactory) works correctly in ServiceLifetimeTests.cs
- [ ] T076 [P] [US4] Test: Instance-based registration (ImplementationInstance) works correctly in ServiceLifetimeTests.cs
- [ ] T077 [P] [US4] Test: Type-based registration (ImplementationType) works correctly in ServiceLifetimeTests.cs

**Checkpoint**: Full MSDI compatibility achieved - all lifetime semantics work correctly

---

## Phase 7: Scene Container Management (Additional Feature)

**Goal**: High-level scene container manager for easy scene lifecycle management

**Independent Test**: Initialize global container, create/retrieve/unload scene containers by name

### Implementation

- [ ] T078 [P] Create ISceneContainerManager interface in dotnet/framework/LablabBean.DependencyInjection/ISceneContainerManager.cs
- [ ] T079 Create SceneContainerManager class in dotnet/framework/LablabBean.DependencyInjection/SceneContainerManager.cs
- [ ] T080 Implement GlobalContainer property and IsInitialized in SceneContainerManager.cs
- [ ] T081 Implement InitializeGlobalContainer method in SceneContainerManager.cs
- [ ] T082 Implement CreateSceneContainer method with parent scene name support in SceneContainerManager.cs
- [ ] T083 Implement GetSceneContainer method in SceneContainerManager.cs
- [ ] T084 Implement UnloadScene method with disposal in SceneContainerManager.cs
- [ ] T085 Implement GetSceneNames method in SceneContainerManager.cs
- [ ] T086 Add thread-safe scene registry (ConcurrentDictionary or locks) in SceneContainerManager.cs
- [ ] T087 Add validation: prevent duplicate scene names in SceneContainerManager.cs
- [ ] T088 Add validation: verify parent scene exists when specified in SceneContainerManager.cs

### Tests

- [ ] T089 [P] Create SceneContainerManagerTests.cs in dotnet/tests/LablabBean.DependencyInjection.Tests/Integration/SceneContainerManagerTests.cs
- [ ] T090 [P] Test: InitializeGlobalContainer creates global container in SceneContainerManagerTests.cs
- [ ] T091 [P] Test: CreateSceneContainer registers scene container in SceneContainerManagerTests.cs
- [ ] T092 [P] Test: CreateSceneContainer with parent name creates hierarchy in SceneContainerManagerTests.cs
- [ ] T093 [P] Test: GetSceneContainer retrieves registered container in SceneContainerManagerTests.cs
- [ ] T094 [P] Test: UnloadScene disposes and removes container in SceneContainerManagerTests.cs
- [ ] T095 [P] Test: Duplicate scene name throws InvalidOperationException in SceneContainerManagerTests.cs
- [ ] T096 [P] Test: Invalid parent scene name throws InvalidOperationException in SceneContainerManagerTests.cs
- [ ] T097 Test: Complete game scenario (global → multiple scenes) in SceneContainerManagerTests.cs

**Checkpoint**: Scene management API complete and tested

---

## Phase 8: Performance & Polish

**Purpose**: Performance validation, optimization, and final polish

- [ ] T098 [P] Create PerformanceTests.cs with BenchmarkDotNet in dotnet/tests/LablabBean.DependencyInjection.Tests/Integration/PerformanceTests.cs
- [ ] T099 [P] Benchmark: Service resolution overhead (flat vs 1-level vs 3-level hierarchy) in PerformanceTests.cs
- [ ] T100 [P] Benchmark: Container creation and disposal (measure for 1000 cycles) in PerformanceTests.cs
- [ ] T101 [P] Benchmark: Scene transition timing (< 16ms target) in PerformanceTests.cs
- [ ] T102 [P] Add XML documentation comments to all public APIs in HierarchicalServiceProvider.cs
- [ ] T103 [P] Add XML documentation comments to all public APIs in SceneContainerManager.cs
- [ ] T104 [P] Add XML documentation comments to all public APIs in ServiceCollectionExtensions.cs
- [ ] T105 [P] Add XML documentation comments to exception classes in Exceptions/
- [ ] T106 Create README.md for library in dotnet/framework/LablabBean.DependencyInjection/README.md
- [ ] T107 Verify all tests pass and coverage is adequate
- [ ] T108 Run performance benchmarks and verify goals met (< 1μs overhead, < 16ms disposal)
- [ ] T109 Update quickstart.md examples to match final API
- [ ] T110 Create example integration in dotnet/examples/HierarchicalDI/ demonstrating usage

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2)
- **User Story 2 (Phase 4)**: Depends on User Story 1 (Phase 3) - builds on core implementation
- **User Story 3 (Phase 5)**: Depends on User Story 2 (Phase 4) - adds cascading disposal
- **User Story 4 (Phase 6)**: Depends on User Story 1 (Phase 3) - can run in parallel with US2/US3
- **Scene Manager (Phase 7)**: Depends on User Stories 1-3 (Phases 3-5)
- **Polish (Phase 8)**: Depends on all previous phases

### User Story Dependencies

- **User Story 1 (P1)**: Foundation - global container and basic resolution
- **User Story 2 (P2)**: Builds on US1 - adds child creation and parent fallback
- **User Story 3 (P3)**: Builds on US2 - adds cascading disposal
- **User Story 4 (P2)**: Builds on US1 - can proceed in parallel with US2/US3 after US1

### Within Each User Story

- Implementation tasks before test tasks (TDD optional for this feature)
- Core implementation before integration tests
- Unit tests can run in parallel (marked [P])
- Integration tests after unit tests

### Parallel Opportunities

**Phase 1 - Setup**: T003, T004, T005, T008, T009 can run in parallel

**Phase 2 - Foundational**: T010, T011 can run in parallel

**Phase 3 - US1 Tests**: T024-T029 can run in parallel after T023 completes

**Phase 4 - US2 Tests**: T038-T045 can run in parallel after T037 completes

**Phase 5 - US3 Tests**: T051-T056 can run in parallel after T050 completes

**Phase 6 - US4 Tests**: T068-T077 can run in parallel after T067 completes

**Phase 7 - Scene Manager**: T078 and T079-T088 implementation can overlap partially

**Phase 7 - Scene Manager Tests**: T089-T096 can run in parallel after T088 completes

**Phase 8 - Polish**: T098-T105 can run in parallel

---

## Parallel Example: User Story 1

```bash
# After completing T023, launch all US1 tests together:
Task: T024 - Create HierarchicalServiceProviderTests.cs
Task: T025 - Test singleton instance returns
Task: T026 - Test multiple GetService calls
Task: T027 - Test ObjectDisposedException
Task: T028 - Test GetRequiredService error message
Task: T029 - Test BuildHierarchicalServiceProvider extension
```

---

## Parallel Example: User Story 4

```bash
# After completing T067, launch all US4 lifetime tests together:
Task: T069 - Test Singleton lifetime
Task: T070 - Test Scoped lifetime
Task: T071 - Test Transient lifetime
Task: T072 - Test CreateScope
Task: T073 - Test scoped disposal
Task: T074 - Test IServiceProviderFactory
Task: T075 - Test factory-based registration
Task: T076 - Test instance-based registration
Task: T077 - Test type-based registration
```

---

## Implementation Strategy

### MVP First (User Stories 1-2 Only)

1. Complete Phase 1: Setup (T001-T009)
2. Complete Phase 2: Foundational (T010-T012) - CRITICAL
3. Complete Phase 3: User Story 1 (T013-T029) - Global container
4. **STOP and VALIDATE**: Test US1 independently
5. Complete Phase 4: User Story 2 (T030-T045) - Child containers
6. **STOP and VALIDATE**: Test US1 + US2 together
7. Deploy/demo MVP (global + scene containers working)

### Full Feature Delivery

1. Complete MVP (Phases 1-4)
2. Add Phase 5: User Story 3 (T046-T059) - Deep hierarchies
3. Add Phase 6: User Story 4 (T060-T077) - MSDI compatibility
4. Add Phase 7: Scene Manager (T078-T097) - High-level API
5. Complete Phase 8: Polish (T098-T110) - Performance & docs

### Parallel Team Strategy

With 2-3 developers after Foundational phase:

1. Team completes Setup + Foundational together
2. Once Foundational (Phase 2) is done:
   - **Developer A**: User Story 1 (Phase 3) → Core implementation
   - **Developer B**: User Story 4 (Phase 6) → MSDI interfaces (can start after US1 core)
3. After US1 complete:
   - **Developer A**: User Story 2 (Phase 4) → Child containers
   - **Developer B**: Continue User Story 4
4. After US2 complete:
   - **Developer A**: User Story 3 (Phase 5) → Cascading disposal
   - **Developer B**: Scene Manager (Phase 7, after US3 available)
5. **Both**: Phase 8 Polish (parallel tasks)

---

## Notes

- [P] tasks = different files, no dependencies - can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently testable at its checkpoint
- Tests are included but not TDD style - implementation first approach
- Stop at any checkpoint to validate story independently before proceeding
- Performance benchmarks (Phase 8) validate goals: < 1μs overhead, < 16ms disposal, 1000+ cycles
- XML documentation required for public API (Phase 8)
- Example integration recommended for demonstrating usage patterns

---

## Success Criteria Validation

After Phase 8 completion, verify:

- **SC-001**: ✓ Global container accessible from child (US1, US2)
- **SC-002**: ✓ 3-level hierarchy performance validated (US3, Phase 8)
- **SC-003**: ✓ 1000+ create/dispose cycles benchmarked (Phase 8)
- **SC-004**: ✓ IServiceProvider compatibility tested (US4)
- **SC-005**: ✓ Scene transition < 16ms validated (Phase 8)
- **SC-006**: ✓ All MSDI lifetimes tested (US4)
- **SC-007**: ✓ Third-party library integration tested (US4)
- **SC-008**: ✓ IServiceCollection syntax unchanged (US1, US4)
