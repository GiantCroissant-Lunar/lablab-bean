# Tasks: Kernel Memory Integration for NPC Intelligence

**Input**: Design documents from `/specs/020-kernel-memory-integration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Test tasks are included based on the .NET testing framework (xUnit). Tests follow TDD principles.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a multi-project .NET solution with contracts-based architecture:

- **Contracts**: `dotnet/framework/LablabBean.Contracts.AI/`
- **Implementation**: `dotnet/framework/LablabBean.AI.Agents/`
- **Core Models**: `dotnet/framework/LablabBean.Core/`
- **Tests**: `dotnet/framework/tests/LablabBean.AI.Agents.Tests/`
- **Console App**: `dotnet/console-app/LablabBean.Console/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, NuGet packages, and basic configuration

- [x] T001 Create LablabBean.Contracts.AI project if not exists in dotnet/framework/LablabBean.Contracts.AI/
- [x] T002 Add Microsoft.KernelMemory.Core NuGet package to dotnet/framework/LablabBean.AI.Agents/
- [x] T003 [P] Add Microsoft.KernelMemory.SemanticKernelPlugin NuGet package to dotnet/framework/LablabBean.AI.Agents/
- [x] T004 [P] Create LablabBean.AI.Agents.Tests project if not exists in dotnet/framework/tests/LablabBean.AI.Agents.Tests/
- [x] T005 [P] Add KernelMemory configuration section to dotnet/console-app/LablabBean.Console/appsettings.json
- [x] T006 [P] Create Services directory in dotnet/framework/LablabBean.AI.Agents/Services/
- [x] T007 [P] Create Models directory in dotnet/framework/LablabBean.AI.Agents/Models/
- [x] T008 [P] Create Memory subdirectory in dotnet/framework/LablabBean.Contracts.AI/Memory/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core contracts and DTOs that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T009 [P] Create MemoryEntry DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [x] T010 [P] Create MemoryRetrievalOptions DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [x] T011 [P] Create MemoryResult DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [x] T012 [P] Create PlayerBehavior enum in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [x] T013 [P] Create OutcomeType enum in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [x] T014 Create IMemoryService interface in dotnet/framework/LablabBean.Contracts.AI/Memory/IMemoryService.cs
- [x] T015 Create KernelMemoryOptions configuration class in dotnet/framework/LablabBean.AI.Agents/Configuration/KernelMemoryOptions.cs
- [x] T016 Add KernelMemory DI registration method to dotnet/framework/LablabBean.AI.Agents/Extensions/ServiceCollectionExtensions.cs
- [x] T017 Create MemoryService base class in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs with IMemoryService implementation skeleton

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Contextually Relevant NPC Decisions (Priority: P1) 🎯 MVP

**Goal**: Replace chronological memory retrieval (last 5 memories) with semantic similarity-based retrieval so NPCs make contextually appropriate decisions based on relevant past experiences.

**Independent Test**: Trigger an NPC decision scenario where relevant memories exist from earlier time periods (not in the last 5 chronological entries) and verify the NPC's decision reflects those relevant memories rather than recent but irrelevant ones.

### Tests for User Story 1

**NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T018 [P] [US1] Unit test for MemoryService.StoreMemoryAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/MemoryServiceTests.cs
- [x] T019 [P] [US1] Unit test for MemoryService.RetrieveRelevantMemoriesAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/MemoryServiceTests.cs
- [x] T020 [P] [US1] Integration test for semantic memory retrieval in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/SemanticRetrievalTests.cs

### Implementation for User Story 1

- [x] T021 [US1] Implement MemoryService.StoreMemoryAsync with embedding generation and tagging in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [x] T022 [US1] Implement MemoryService.RetrieveRelevantMemoriesAsync with semantic search and filtering in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [x] T023 [US1] Implement memory tagging strategy (entity, type, importance, timestamp) in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [x] T024 [US1] Update EmployeeIntelligenceAgent.GetDecisionAsync to use IMemoryService.RetrieveRelevantMemoriesAsync in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [x] T025 [US1] Update BossIntelligenceAgent.GetDecisionAsync to use IMemoryService.RetrieveRelevantMemoriesAsync in dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs
- [x] T026 [US1] Add dual-write logic to store memories in both legacy AvatarMemory and new MemoryService in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [x] T027 [US1] Add dual-write logic to store memories in both legacy AvatarMemory and new MemoryService in dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs
- [x] T028 [US1] Add error handling and fallback to legacy memory retrieval if MemoryService unavailable in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [x] T029 [US1] Add logging for memory retrieval operations (relevance scores, count) in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - NPCs retrieve semantically relevant memories instead of just recent ones, testable by comparing decision quality before/after

---

## Phase 4: User Story 2 - Persistent Cross-Session Memory (Priority: P2)

**Goal**: Enable NPCs to retain memories across application restarts, creating continuity and long-term relationship building.

**Independent Test**: Record NPC interactions in one game session, restart the application, and verify NPCs can recall and act upon memories from the previous session.

### Tests for User Story 2

- [ ] T030 [P] [US2] Integration test for memory persistence across restarts in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/MemoryPersistenceTests.cs
- [ ] T031 [P] [US2] Unit test for Qdrant configuration validation in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Configuration/KernelMemoryOptionsTests.cs

### Implementation for User Story 2

- [ ] T032 [US2] Add Microsoft.KernelMemory.MemoryDb.Qdrant NuGet package to dotnet/framework/LablabBean.AI.Agents/
- [ ] T033 [US2] Add Qdrant configuration options to dotnet/framework/LablabBean.AI.Agents/Configuration/KernelMemoryOptions.cs
- [ ] T034 [US2] Update ServiceCollectionExtensions to configure Qdrant when VectorDbType is "Qdrant" in dotnet/framework/LablabBean.AI.Agents/Extensions/ServiceCollectionExtensions.cs
- [ ] T035 [US2] Add Qdrant configuration to dotnet/console-app/LablabBean.Console/appsettings.Production.json
- [ ] T036 [US2] Create docker-compose.yml with Qdrant service definition in repository root
- [ ] T037 [US2] Implement graceful degradation to in-memory mode if Qdrant unavailable in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T038 [US2] Add connection health check for Qdrant on startup in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T039 [US2] Implement MemoryService.MigrateLegacyMemoriesAsync for one-time migration in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T040 [US2] Add logging for persistence operations (storage backend, migration status) in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - semantic retrieval works with in-memory storage, and memories persist across restarts when Qdrant is configured

---

## Phase 5: User Story 3 - Knowledge-Grounded NPC Behavior (Priority: P3)

**Goal**: Enable NPCs to query knowledge bases (personality documents, policies, lore) to ground their responses in consistent documented information.

**Independent Test**: Create a knowledge base document (e.g., "Employee Handbook"), trigger an NPC decision scenario covered by the handbook, and verify the NPC's response aligns with the handbook guidance.

### Tests for User Story 3

- [ ] T041 [P] [US3] Unit test for KnowledgeBaseService.IndexDocumentAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/KnowledgeBaseServiceTests.cs
- [ ] T042 [P] [US3] Unit test for KnowledgeBaseService.QueryKnowledgeBaseAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/KnowledgeBaseServiceTests.cs
- [ ] T043 [P] [US3] Integration test for RAG workflow (index → query → citation) in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/KnowledgeBaseRAGTests.cs

### Implementation for User Story 3

- [ ] T044 [P] [US3] Create KnowledgeBaseDocument DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [ ] T045 [P] [US3] Create KnowledgeBaseAnswer DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [ ] T046 [P] [US3] Create Citation DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs
- [ ] T047 [US3] Create IKnowledgeBaseService interface in dotnet/framework/LablabBean.Contracts.AI/Memory/IKnowledgeBaseService.cs
- [ ] T048 [US3] Implement KnowledgeBaseService class in dotnet/framework/LablabBean.AI.Agents/Services/KnowledgeBaseService.cs
- [ ] T049 [US3] Implement KnowledgeBaseService.IndexDocumentAsync with role-based tagging in dotnet/framework/LablabBean.AI.Agents/Services/KnowledgeBaseService.cs
- [ ] T050 [US3] Implement KnowledgeBaseService.QueryKnowledgeBaseAsync with RAG in dotnet/framework/LablabBean.AI.Agents/Services/KnowledgeBaseService.cs
- [ ] T051 [US3] Add IKnowledgeBaseService to DI container in dotnet/framework/LablabBean.AI.Agents/Extensions/ServiceCollectionExtensions.cs
- [ ] T052 [US3] Update EmployeeIntelligenceAgent to query knowledge base for customer service scenarios in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [ ] T053 [US3] Update BossIntelligenceAgent to query knowledge base for management decisions in dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs
- [ ] T054 [US3] Create sample knowledge base documents (employee_handbook.txt, boss_policies.txt) in dotnet/console-app/LablabBean.Console/knowledge/
- [ ] T055 [US3] Add knowledge base indexing on application startup in dotnet/console-app/LablabBean.Console/Program.cs

**Checkpoint**: All three user stories (semantic retrieval, persistence, knowledge base RAG) should now be independently functional and testable

---

## Phase 6: User Story 4 - Adaptive Tactical Enemy Behavior (Priority: P4)

**Goal**: Enable tactical enemies to analyze and learn from player combat behavior patterns, adapting strategies based on observed tendencies.

**Independent Test**: Have a player exhibit consistent combat behavior patterns across multiple encounters, then verify tactical enemies employ counter-strategies specific to those observed patterns.

### Tests for User Story 4

- [ ] T056 [P] [US4] Unit test for MemoryService.StoreTacticalObservationAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs
- [ ] T057 [P] [US4] Unit test for MemoryService.RetrieveSimilarTacticsAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/TacticalMemoryTests.cs
- [ ] T058 [P] [US4] Integration test for tactical learning loop in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/TacticalLearningTests.cs

### Implementation for User Story 4

- [ ] T059 [P] [US4] Create TacticalObservation DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs (already in foundational, verify completeness)
- [ ] T060 [US4] Implement MemoryService.StoreTacticalObservationAsync with behavior-specific tagging in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T061 [US4] Implement MemoryService.RetrieveSimilarTacticsAsync with behavior filtering in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T062 [US4] Update TacticsAgent.CreateTacticalPlanAsync to store observations after encounters in dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs
- [ ] T063 [US4] Update TacticsAgent.CreateTacticalPlanAsync to retrieve similar past encounters before planning in dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs
- [ ] T064 [US4] Implement counter-tactic selection based on effectiveness ratings from retrieved observations in dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs
- [ ] T065 [US4] Add aggregation logic for identifying dominant player behavior patterns in dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs
- [ ] T066 [US4] Add logging for tactical observations and counter-strategy selection in dotnet/framework/LablabBean.AI.Agents/TacticsAgent.cs

**Checkpoint**: User Story 4 complete - tactical enemies learn from player patterns and adapt strategies, testable by observing enemy behavior changes after repeated player tactics

---

## Phase 7: User Story 5 - Semantic Relationship Memory (Priority: P5)

**Goal**: Enable NPCs to maintain rich, semantically searchable relationship histories for nuanced interaction dynamics.

**Independent Test**: Create a history of varied interactions between two NPCs, trigger a new interaction similar to an older (not recent) interaction, and verify the NPC recalls the contextually relevant past event.

### Tests for User Story 5

- [ ] T067 [P] [US5] Unit test for MemoryService.StoreRelationshipEventAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/RelationshipMemoryTests.cs
- [ ] T068 [P] [US5] Unit test for MemoryService.GetRelationshipHistoryAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/RelationshipMemoryTests.cs
- [ ] T069 [P] [US5] Integration test for relationship-aware dialogue in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/RelationshipDialogueTests.cs

### Implementation for User Story 5

- [ ] T070 [P] [US5] Create RelationshipEvent DTO in dotnet/framework/LablabBean.Contracts.AI/Memory/DTOs.cs (already in foundational, verify completeness)
- [ ] T071 [US5] Implement MemoryService.StoreRelationshipEventAsync with participant tagging in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T072 [US5] Implement MemoryService.GetRelationshipHistoryAsync with bidirectional filtering in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T073 [US5] Update EmployeeIntelligenceAgent.GenerateDialogueAsync to retrieve relationship history before dialogue generation in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [ ] T074 [US5] Update BossIntelligenceAgent.GenerateDialogueAsync to retrieve relationship history before dialogue generation in dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs
- [ ] T075 [US5] Store relationship events after NPC-NPC interactions in dotnet/framework/LablabBean.AI.Agents/EmployeeIntelligenceAgent.cs
- [ ] T076 [US5] Store relationship events after NPC-player interactions in dotnet/framework/LablabBean.AI.Agents/BossIntelligenceAgent.cs
- [ ] T077 [US5] Add emotional impact tracking to relationship events in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T078 [US5] Add logging for relationship event storage and retrieval in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs

**Checkpoint**: All five user stories are now complete and independently functional - semantic retrieval, persistence, knowledge base RAG, tactical learning, and relationship memory all working together

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements, documentation, and optimizations that affect multiple user stories

- [ ] T079 [P] Update existing documentation with Kernel Memory integration in docs/findings/kernel-memory-integration-analysis.md
- [ ] T080 [P] Add migration guide for existing AvatarMemory data in specs/020-kernel-memory-integration/quickstart.md
- [ ] T081 [P] Create performance benchmarks for memory retrieval latency in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Benchmarks/MemoryServiceBenchmarks.cs
- [ ] T082 Implement memory retention policies (configurable pruning) in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T083 Add rate limit handling with exponential backoff for embedding API in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T084 Implement MemoryService.DeleteEntityMemoriesAsync for cleanup in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T085 [P] Add comprehensive XML documentation comments to IMemoryService in dotnet/framework/LablabBean.Contracts.AI/Memory/IMemoryService.cs
- [ ] T086 [P] Add comprehensive XML documentation comments to IKnowledgeBaseService in dotnet/framework/LablabBean.Contracts.AI/Memory/IKnowledgeBaseService.cs
- [ ] T087 Code cleanup: Remove legacy memory retrieval code after migration period in dotnet/framework/LablabBean.AI.Agents/
- [ ] T088 Security review: Validate memory access controls (entity isolation) in dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- [ ] T089 Run quickstart.md validation: Verify 15-minute setup time in specs/020-kernel-memory-integration/quickstart.md
- [ ] T090 Create troubleshooting runbook for common issues (API keys, rate limits, persistence) in specs/020-kernel-memory-integration/troubleshooting.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) - Can start after foundation ready
- **User Story 2 (Phase 4)**: Depends on Foundational (Phase 2) and User Story 1 implementation - Builds on semantic retrieval
- **User Story 3 (Phase 5)**: Depends on Foundational (Phase 2) - Can start in parallel with US1/US2
- **User Story 4 (Phase 6)**: Depends on Foundational (Phase 2) and User Story 1 - Builds on semantic retrieval
- **User Story 5 (Phase 7)**: Depends on Foundational (Phase 2) and User Story 1 - Builds on semantic retrieval
- **Polish (Phase 8)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - **No dependencies on other stories**
- **User Story 2 (P2)**: Requires User Story 1 core MemoryService implementation - Adds persistence layer
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - **Independent** (can run parallel with US1/US2 if capacity allows)
- **User Story 4 (P4)**: Requires User Story 1 core MemoryService - Uses semantic retrieval infrastructure
- **User Story 5 (P5)**: Requires User Story 1 core MemoryService - Uses semantic retrieval infrastructure

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- DTOs and interfaces before services
- Service implementation before agent integration
- Core functionality before integration points
- Logging and error handling after core logic
- Story complete before moving to next priority

### Parallel Opportunities

**Setup Phase (all can run in parallel):**

- T003, T004, T005, T006, T007, T008 all touch different files

**Foundational Phase (most can run in parallel):**

- T009-T013 (DTOs) can run in parallel
- T015 (config) can run in parallel with DTOs
- T014, T016, T017 must run sequentially (interface → DI → implementation skeleton)

**User Story 1:**

- T018, T019, T020 (tests) can run in parallel
- T024, T025 (agent updates) can run in parallel after T021-T023 complete
- T026, T027 (dual-write) can run in parallel

**User Story 2:**

- T030, T031 (tests) can run in parallel
- T032, T033 can run in parallel

**User Story 3:**

- T041, T042, T043 (tests) can run in parallel
- T044, T045, T046 (DTOs) can run in parallel
- T052, T053 (agent updates) can run in parallel

**User Story 4:**

- T056, T057, T058 (tests) can run in parallel

**User Story 5:**

- T067, T068, T069 (tests) can run in parallel
- T073, T074 (dialogue updates) can run in parallel
- T075, T076 (event storage) can run in parallel

**Polish Phase:**

- T079, T080, T081, T085, T086 can all run in parallel (different files)

**Multiple User Stories in Parallel** (with team capacity):

- After Foundational complete: US1 + US3 can run in parallel (independent)
- After US1 complete: US2, US4, US5 can run in parallel (all depend on US1 foundation)

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit test for MemoryService.StoreMemoryAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/MemoryServiceTests.cs"
Task: "Unit test for MemoryService.RetrieveRelevantMemoriesAsync in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Services/MemoryServiceTests.cs"
Task: "Integration test for semantic memory retrieval in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Integration/SemanticRetrievalTests.cs"

# After core MemoryService implementation (T021-T023), launch agent updates in parallel:
Task: "Update EmployeeIntelligenceAgent.GetDecisionAsync to use IMemoryService.RetrieveRelevantMemoriesAsync"
Task: "Update BossIntelligenceAgent.GetDecisionAsync to use IMemoryService.RetrieveRelevantMemoriesAsync"

# Launch dual-write implementations in parallel:
Task: "Add dual-write logic in EmployeeIntelligenceAgent.cs"
Task: "Add dual-write logic in BossIntelligenceAgent.cs"
```

---

## Parallel Example: After Foundational Complete

```bash
# With 3 developers, after Phase 2 completion:

Developer A (US1 - Semantic Retrieval):
- T018-T020 (tests)
- T021-T029 (implementation)

Developer B (US3 - Knowledge Base RAG):
- T041-T043 (tests)
- T044-T055 (implementation)

Developer C (Setup for US2):
- T032-T035 (Qdrant packages and config)
- T036 (docker-compose)

# Then after US1 completes, Developer A moves to US2 implementation
# Or Developer A picks up US4/US5 if US3 is higher priority than US2 persistence
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. **Complete Phase 1: Setup** (T001-T008)
   - Project structure, NuGet packages, directories
   - ~2 hours
2. **Complete Phase 2: Foundational** (T009-T017)
   - Contracts, DTOs, base service structure
   - CRITICAL - blocks all stories
   - ~4 hours
3. **Complete Phase 3: User Story 1** (T018-T029)
   - Semantic memory retrieval
   - ~2-3 days
4. **STOP and VALIDATE**:
   - Test: Trigger employee decision with relevant old memories
   - Verify: Retrieves contextually relevant memories (not just last 5)
   - Measure: Relevance scores > 0.7
   - Deploy/demo if ready
5. **Estimated MVP Timeline**: ~3-4 days for a single developer

### Incremental Delivery

1. **Setup + Foundational** (1 day) → Foundation ready
2. **Add User Story 1** (3 days) → Test independently → **Deploy/Demo (MVP!)**
   - NPCs now make contextually relevant decisions
   - Measurable improvement in NPC intelligence
3. **Add User Story 2** (2 days) → Test independently → Deploy/Demo
   - Memories persist across sessions
   - Long-term character development enabled
4. **Add User Story 3** (2 days) → Test independently → Deploy/Demo
   - NPCs grounded in knowledge bases
   - Consistent behavior based on policies/lore
5. **Add User Story 4** (2 days) → Test independently → Deploy/Demo
   - Tactical enemies adapt to player patterns
   - Dynamic combat challenge
6. **Add User Story 5** (2 days) → Test independently → Deploy/Demo
   - Rich relationship dynamics
   - Context-aware dialogue
7. **Polish** (1-2 days) → Final validation → Production release

**Total Estimated Timeline**: ~2-3 weeks for all user stories (single developer, sequential)

### Parallel Team Strategy

With 3 developers after Foundational phase:

1. **Week 1**:
   - Dev A: User Story 1 (semantic retrieval) - **MVP**
   - Dev B: User Story 3 (knowledge base RAG) - independent
   - Dev C: Setup for User Story 2 (Qdrant config/Docker)
2. **Week 2**:
   - Dev A: User Story 2 implementation (builds on US1)
   - Dev B: User Story 4 (tactical memory, builds on US1)
   - Dev C: User Story 5 (relationship memory, builds on US1)
3. **Week 3**:
   - All: Polish, documentation, optimization
   - Integration testing across all stories
   - Production deployment

**Parallel Timeline**: ~2 weeks with 3 developers

---

## Task Summary

| Phase | Task Count | Can Parallelize | Description |
|-------|------------|-----------------|-------------|
| Setup | 8 | 6 tasks | Project structure, packages, directories |
| Foundational | 9 | 5 tasks | Contracts, DTOs, base services (BLOCKS all stories) |
| User Story 1 (P1) | 12 | 4 tasks | Semantic memory retrieval (MVP) |
| User Story 2 (P2) | 11 | 2 tasks | Persistence with Qdrant |
| User Story 3 (P3) | 15 | 6 tasks | Knowledge base RAG |
| User Story 4 (P4) | 11 | 3 tasks | Tactical learning |
| User Story 5 (P5) | 12 | 5 tasks | Relationship memory |
| Polish | 12 | 5 tasks | Documentation, optimization, cleanup |
| **Total** | **90 tasks** | **36 parallel** | Complete feature implementation |

**Parallel Opportunities**: 40% of tasks can run in parallel (36 of 90)

**Independent Test Criteria per Story**:

- **US1**: NPC retrieves old relevant memories instead of recent irrelevant ones
- **US2**: Memories survive application restart
- **US3**: NPC behavior aligns with indexed knowledge base
- **US4**: Enemy tactics adapt after observing player patterns
- **US5**: Dialogue reflects contextually relevant relationship history

**Suggested MVP Scope**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (User Story 1)

- **Estimated effort**: 3-4 days single developer
- **Deliverable**: NPCs make contextually relevant decisions using semantic memory retrieval
- **Value**: Immediate, noticeable improvement in NPC intelligence

---

## Notes

- **[P] tasks**: Different files, no dependencies - safe to parallelize
- **[Story] label**: Maps task to specific user story for traceability and independent testing
- **Tests**: Follow TDD - write tests first, verify they fail, then implement
- **Checkpoints**: Stop after each user story phase to validate independently
- **Commits**: Commit after each task or logical group for rollback safety
- **Migration**: Dual-write approach (Phase 3) ensures zero data loss during transition
- **Storage**: Start with SimpleVectorDb (in-memory) for Phases 1-3, add Qdrant in Phase 4
- **User Story Independence**: US1 and US3 are fully independent; US2, US4, US5 build on US1 foundation

**Success Metrics**:

- Memory retrieval relevance score: >0.7 average (US1)
- Memory persistence: 100% across restarts (US2)
- Knowledge base answers: 95% grounded in citations (US3)
- Tactical adaptation: 50% of enemies employ counter-tactics after 5+ observations (US4)
- Relationship-aware dialogue: 80% of interactions reflect relevant history (US5)
