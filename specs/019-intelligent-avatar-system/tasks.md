# Tasks: Intelligent Avatar System

**Input**: Design documents from `/specs/019-intelligent-avatar-system/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅

**Tests**: Not explicitly requested in spec - focusing on implementation tasks only

**Organization**: Tasks grouped by user story (P1-P4) to enable independent implementation and testing

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label (US1, US2, US3, US4)
- All paths relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and framework library structure

- [ ] T001 Add Akka.NET packages to dotnet/Directory.Packages.props (Akka 1.5.31, Akka.Hosting, Akka.Persistence.Sql)
- [ ] T002 [P] Add Semantic Kernel packages to dotnet/Directory.Packages.props (Microsoft.SemanticKernel 1.25.0, Agents.Core, Connectors.OpenAI)
- [ ] T003 [P] Add test framework packages to dotnet/Directory.Packages.props (xUnit 2.5.3, NSubstitute 5.1.0, FluentAssertions 6.12.0, BenchmarkDotNet 0.13.12)
- [ ] T004 Create LablabBean.AI.Core project in dotnet/framework/LablabBean.AI.Core/LablabBean.AI.Core.csproj
- [ ] T005 [P] Create LablabBean.AI.Actors project in dotnet/framework/LablabBean.AI.Actors/LablabBean.AI.Actors.csproj
- [ ] T006 [P] Create LablabBean.AI.Agents project in dotnet/framework/LablabBean.AI.Agents/LablabBean.AI.Agents.csproj
- [ ] T007 [P] Create test project LablabBean.AI.Core.Tests in dotnet/framework/tests/LablabBean.AI.Core.Tests/
- [ ] T008 [P] Create test project LablabBean.AI.Actors.Tests in dotnet/framework/tests/LablabBean.AI.Actors.Tests/
- [ ] T009 [P] Create test project LablabBean.AI.Agents.Tests in dotnet/framework/tests/LablabBean.AI.Agents.Tests/
- [ ] T010 [P] Add project references: AI.Actors → AI.Core, AI.Agents → AI.Core
- [ ] T011 [P] Add project references: AI.Actors → Akka.NET packages, AI.Agents → SemanticKernel packages
- [ ] T012 Create personalities directory at repository root with .gitkeep
- [ ] T013 [P] Create appsettings.Development.json template at repository root (gitignored, with placeholder API keys)
- [ ] T014 [P] Add appsettings.Development.json to .gitignore if not already present

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and bridge components that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Core Components (Layer 1: ECS Bridge)

- [ ] T015 [P] Create AkkaActorRef component struct in dotnet/framework/LablabBean.AI.Core/Components/AkkaActorRef.cs
- [ ] T016 [P] Create SemanticAgent component struct in dotnet/framework/LablabBean.AI.Core/Components/SemanticAgent.cs
- [ ] T017 [P] Create IntelligentAI component struct with AICapability flags in dotnet/framework/LablabBean.AI.Core/Components/IntelligentAI.cs
- [ ] T018 [P] Create AICapability enum in dotnet/framework/LablabBean.AI.Core/Components/IntelligentAI.cs
- [ ] T019 [P] Create AgentType enum in dotnet/framework/LablabBean.AI.Core/Components/SemanticAgent.cs

### Models & State

- [ ] T020 [P] Create AvatarContext model in dotnet/framework/LablabBean.AI.Core/Models/AvatarContext.cs
- [ ] T021 [P] Create AvatarState model in dotnet/framework/LablabBean.AI.Core/Models/AvatarState.cs
- [ ] T022 [P] Create AvatarMemory model in dotnet/framework/LablabBean.AI.Core/Models/AvatarMemory.cs
- [ ] T023 [P] Create AvatarRelationship model in dotnet/framework/LablabBean.AI.Core/Models/AvatarRelationship.cs
- [ ] T024 [P] Create AIDecision model in dotnet/framework/LablabBean.AI.Core/Models/AIDecision.cs
- [ ] T025 [P] Create DialogueContext model in dotnet/framework/LablabBean.AI.Core/Models/DialogueContext.cs

### Events

- [ ] T026 [P] Create AIThoughtEvent in dotnet/framework/LablabBean.AI.Core/Events/AIThoughtEvent.cs
- [ ] T027 [P] Create AIBehaviorChangedEvent in dotnet/framework/LablabBean.AI.Core/Events/AIBehaviorChangedEvent.cs
- [ ] T028 [P] Create NPCDialogueEvent in dotnet/framework/LablabBean.AI.Core/Events/NPCDialogueEvent.cs
- [ ] T029 [P] Create ActorStoppedEvent in dotnet/framework/LablabBean.AI.Core/Events/ActorStoppedEvent.cs

### Interfaces

- [ ] T030 [P] Create IAvatarActor interface in dotnet/framework/LablabBean.AI.Core/Interfaces/IAvatarActor.cs
- [ ] T031 [P] Create IIntelligenceAgent interface in dotnet/framework/LablabBean.AI.Core/Interfaces/IIntelligenceAgent.cs

### Actor Messages (Layer 2: Akka.NET)

- [ ] T032 [P] Create TakeDamageMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/TakeDamageMessage.cs
- [ ] T033 [P] Create PlayerNearbyMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/PlayerNearbyMessage.cs
- [ ] T034 [P] Create DialogueRequestMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/DialogueRequestMessage.cs
- [ ] T035 [P] Create AIDecisionMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/AIDecisionMessage.cs
- [ ] T036 [P] Create DialogueResponseMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/DialogueResponseMessage.cs
- [ ] T037 [P] Create PublishGameEvent record in dotnet/framework/LablabBean.AI.Actors/Messages/PublishGameEvent.cs
- [ ] T038 [P] Create SaveSnapshotCommand record in dotnet/framework/LablabBean.AI.Actors/Messages/SaveSnapshotCommand.cs
- [ ] T039 [P] Create GetAIDecisionRequest record in dotnet/framework/LablabBean.AI.Actors/Messages/GetAIDecisionRequest.cs
- [ ] T040 [P] Create GetDialogueRequest record in dotnet/framework/LablabBean.AI.Actors/Messages/GetDialogueRequest.cs

### Infrastructure

- [ ] T041 Create EventBusAkkaAdapter actor in dotnet/framework/LablabBean.AI.Actors/Bridges/EventBusAkkaAdapter.cs
- [ ] T042 Create AvatarStateSerializer in dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs
- [ ] T043 Create SemanticKernelOptions config class in dotnet/framework/LablabBean.AI.Agents/Configuration/SemanticKernelOptions.cs
- [ ] T044 Add Akka.NET DI registration extension in dotnet/framework/LablabBean.AI.Actors/Extensions/ServiceCollectionExtensions.cs
- [ ] T045 Add Semantic Kernel DI registration extension in dotnet/framework/LablabBean.AI.Agents/Extensions/ServiceCollectionExtensions.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Boss with Personality and Memory (Priority: P1) 🎯 MVP

**Goal**: Implement intelligent boss with personality-driven AI decisions, memory, and adaptive tactics

**Independent Test**: Spawn boss entity with personality "Ancient Dragon", engage in combat, verify personality-appropriate behavior and memory of past actions

### Layer 3: Semantic Kernel Agent

- [ ] T046 [P] [US1] Create NpcIntelligenceAgent class in dotnet/framework/LablabBean.AI.Agents/Agents/NpcIntelligenceAgent.cs
- [ ] T047 [P] [US1] Create DecisionPrompts static class in dotnet/framework/LablabBean.AI.Agents/Prompts/DecisionPrompts.cs
- [ ] T048 [P] [US1] Create DecisionParser class in dotnet/framework/LablabBean.AI.Agents/Parsers/DecisionParser.cs
- [ ] T049 [US1] Implement DecideActionAsync method in NpcIntelligenceAgent (AI decision logic with personality, emotional state, memories)
- [ ] T050 [US1] Implement LRU cache for decision caching in NpcIntelligenceAgent (100 entry capacity)
- [ ] T051 [US1] Implement Polly circuit breaker policy in NpcIntelligenceAgent (3 failures, 30s break)
- [ ] T052 [US1] Implement fallback decision logic for timeout/failure in NpcIntelligenceAgent

### Layer 2: Akka.NET Actors

- [ ] T053 [US1] Create SemanticKernelBridgeActor in dotnet/framework/LablabBean.AI.Actors/Bridges/SemanticKernelBridgeActor.cs
- [ ] T054 [US1] Implement agent management (GetOrCreateAgent) in SemanticKernelBridgeActor
- [ ] T055 [US1] Implement GetAIDecisionRequest handler in SemanticKernelBridgeActor
- [ ] T056 [US1] Create AvatarActor base class in dotnet/framework/LablabBean.AI.Actors/Actors/AvatarActor.cs
- [ ] T057 [US1] Implement PlayerNearbyMessage handler in AvatarActor (query SK for decision)
- [ ] T058 [US1] Implement AIDecisionMessage handler in AvatarActor (update ECS components)
- [ ] T059 [US1] Implement TakeDamageMessage handler in AvatarActor (update state, memory)
- [ ] T060 [US1] Create BossActor subclass in dotnet/framework/LablabBean.AI.Actors/Actors/BossActor.cs
- [ ] T061 [US1] Implement boss-specific behaviors in BossActor (aggressive tactics, boss memory)
- [ ] T062 [US1] Create AvatarSupervisor actor in dotnet/framework/LablabBean.AI.Actors/Actors/AvatarSupervisor.cs
- [ ] T063 [US1] Implement OneForOneStrategy supervision in AvatarSupervisor (restart on LLM failures)
- [ ] T064 [US1] Implement Akka.Persistence recovery in AvatarActor (SnapshotOffer handling)
- [ ] T065 [US1] Implement snapshot saving logic in AvatarActor (every 10 events, on game save)

### Layer 1: ECS Integration

- [ ] T066 [US1] Create IntelligentAISystem in dotnet/framework/LablabBean.Game.Core/Systems/IntelligentAISystem.cs
- [ ] T067 [US1] Implement entity query for intelligent avatars in IntelligentAISystem (IntelligentAI + AkkaActorRef)
- [ ] T068 [US1] Implement PlayerNearby event forwarding to actors in IntelligentAISystem
- [ ] T069 [US1] Implement CreateIntelligentBoss helper method in IntelligentAISystem
- [ ] T070 [US1] Add IntelligentAI component to boss entities on spawn
- [ ] T071 [US1] Add AkkaActorRef component and create BossActor instance

### Configuration & Hosting

- [ ] T072 [US1] Update appsettings.json in dotnet/console-app/LablabBean.Console/appsettings.json (add SemanticKernel and Akka config sections)
- [ ] T073 [US1] Register Akka.NET hosting in dotnet/console-app/LablabBean.Console/Program.cs (AddAkka with actor system name)
- [ ] T074 [US1] Register Semantic Kernel in Program.cs (Kernel.CreateBuilder with OpenAI config)
- [ ] T075 [US1] Start SemanticKernelBridgeActor in Program.cs actor startup
- [ ] T076 [US1] Start AvatarSupervisor in Program.cs actor startup
- [ ] T077 [US1] Register EventBusAkkaAdapter and connect to IEventBus

### Personality Configuration

- [ ] T078 [US1] Create ancient-dragon.json personality file in personalities/ancient-dragon.json
- [ ] T079 [US1] Load personality from JSON on boss spawn in CreateIntelligentBoss method

### Integration

- [ ] T080 [US1] Wire IntelligentAISystem into game loop in existing AISystem coordination
- [ ] T081 [US1] Test boss spawn with personality "Ancient Dragon, proud and cunning"
- [ ] T082 [US1] Test boss AI decision making (aggressive when confident, defensive when low health)
- [ ] T083 [US1] Test boss memory (remembers fleeing player, past hits)
- [ ] T084 [US1] Test fallback behavior on LLM timeout (falls back to Chase)
- [ ] T085 [US1] Test actor supervision (restart on failure, preserve state)

**Checkpoint**: Boss with personality and memory is fully functional and testable independently

---

## Phase 4: User Story 2 - NPC with Dialogue Generation (Priority: P2)

**Goal**: Implement NPC dialogue generation with personality, emotional state, and conversation memory

**Independent Test**: Interact with merchant NPC, verify personality-consistent responses and memory of past conversations

### Layer 3: Dialogue Agent

- [ ] T086 [P] [US2] Create DialogueAgent class in dotnet/framework/LablabBean.AI.Agents/Agents/DialogueAgent.cs
- [ ] T087 [P] [US2] Create DialoguePrompts static class in dotnet/framework/LablabBean.AI.Agents/Prompts/DialoguePrompts.cs
- [ ] T088 [P] [US2] Create DialogueParser class in dotnet/framework/LablabBean.AI.Agents/Parsers/DialogueParser.cs
- [ ] T089 [US2] Implement GenerateDialogueAsync method in NpcIntelligenceAgent (extend existing agent)
- [ ] T090 [US2] Implement ChatHistory management in NpcIntelligenceAgent (track last 10 messages)
- [ ] T091 [US2] Implement dialogue fallback on timeout in NpcIntelligenceAgent

### Layer 2: Dialogue Actor Messages

- [ ] T092 [US2] Implement DialogueRequestMessage handler in AvatarActor (forward to SK bridge)
- [ ] T093 [US2] Implement DialogueResponseMessage handler in AvatarActor (publish to event bus)
- [ ] T094 [US2] Create NpcActor subclass in dotnet/framework/LablabBean.AI.Actors/Actors/NpcActor.cs
- [ ] T095 [US2] Implement NPC-specific dialogue handling in NpcActor (merchant personality, emotional state changes)
- [ ] T096 [US2] Implement GetDialogueRequest handler in SemanticKernelBridgeActor

### Layer 1: NPC Integration

- [ ] T097 [US2] Create CreateIntelligentNPC helper in IntelligentAISystem
- [ ] T098 [US2] Add dialogue capability to IntelligentAI component (AICapability.Dialogue flag)
- [ ] T099 [US2] Implement player-to-NPC dialogue interaction in console UI (extend Terminal.Gui views)
- [ ] T100 [US2] Wire NPCDialogueEvent to activity log display

### Personality Configuration

- [ ] T101 [US2] Create cautious-merchant.json personality in personalities/cautious-merchant.json

### Integration

- [ ] T102 [US2] Test merchant NPC spawn with dialogue capability
- [ ] T103 [US2] Test dialogue generation (personality-appropriate responses)
- [ ] T104 [US2] Test conversation memory (remembers past transactions)
- [ ] T105 [US2] Test emotional state changes (insult → "Offended" → colder responses)
- [ ] T106 [US2] Test dialogue fallback on timeout (generic response without crash)

**Checkpoint**: NPC dialogue system is fully functional and testable independently

---

## Phase 5: User Story 3 - Adaptive Enemy Tactics (Priority: P3)

**Goal**: Implement enemy learning from player behavior with tactical adaptation

**Independent Test**: Fight same enemy multiple times with different tactics, verify enemy adapts (e.g., closes distance against ranged attacks)

### Layer 3: Tactics Agent

- [ ] T107 [P] [US3] Create TacticsAgent class in dotnet/framework/LablabBean.AI.Agents/Agents/TacticsAgent.cs
- [ ] T108 [P] [US3] Create TacticsPrompts static class in dotnet/framework/LablabBean.AI.Agents/Prompts/TacticsPrompts.cs
- [ ] T109 [US3] Implement CreateTacticalPlanAsync method in NpcIntelligenceAgent (extend existing agent)
- [ ] T110 [US3] Implement player behavior tracking in AvatarState (detect patterns: ranged, hit-and-run, healing)

### Layer 2: Tactics Messages

- [ ] T111 [P] [US3] Create TacticalPlanMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/TacticalPlanMessage.cs
- [ ] T112 [P] [US3] Create GetTacticalPlanRequest record in dotnet/framework/LablabBean.AI.Actors/Messages/GetTacticalPlanRequest.cs
- [ ] T113 [US3] Implement GetTacticalPlanRequest handler in SemanticKernelBridgeActor
- [ ] T114 [US3] Implement TacticalPlanMessage handler in AvatarActor (update combat strategy)
- [ ] T115 [US3] Implement PlayerBehaviorObservedEvent in dotnet/framework/LablabBean.AI.Core/Events/PlayerBehaviorObservedEvent.cs
- [ ] T116 [US3] Subscribe to PlayerBehaviorObservedEvent in AvatarActor (update learning state)

### Layer 1: Learning Integration

- [ ] T117 [US3] Add AICapability.Learning flag to IntelligentAI component
- [ ] T118 [US3] Implement player behavior detection in CombatSystem (ranged vs melee, hit-and-run, healing frequency)
- [ ] T119 [US3] Publish PlayerBehaviorObservedEvent from CombatSystem
- [ ] T120 [US3] Implement tactical adaptation in AIBehavior selection (enemies with Learning capability query tactics)

### Multi-Enemy Coordination

- [ ] T121 [US3] Implement coordinated tactics for multiple intelligent enemies (flanking detection, focus fire logic)
- [ ] T122 [US3] Add group coordination messages between enemy actors

### Integration

- [ ] T123 [US3] Test enemy adapts to ranged attacks (closes distance quickly)
- [ ] T124 [US3] Test enemy adapts to hit-and-run (cuts off escape routes)
- [ ] T125 [US3] Test enemy adapts to healing (aggressive pressure)
- [ ] T126 [US3] Test multi-enemy coordination (flanking, focus fire)

**Checkpoint**: Adaptive enemy tactics are fully functional and testable independently

---

## Phase 6: User Story 4 - Quest Giver with Context Awareness (Priority: P4)

**Goal**: Implement quest giver NPCs that adapt quests to player progress, choices, and reputation

**Independent Test**: Interact with quest giver at different game states (early, mid, late), verify quest offerings adapt

### Layer 3: Quest Agent

- [ ] T127 [P] [US4] Create QuestAgent class in dotnet/framework/LablabBean.AI.Agents/Agents/QuestAgent.cs
- [ ] T128 [P] [US4] Create QuestPrompts static class in dotnet/framework/LablabBean.AI.Agents/Prompts/QuestPrompts.cs
- [ ] T129 [US4] Implement quest offering logic in QuestAgent (adapt to player reputation, past quests)
- [ ] T130 [US4] Implement quest context tracking in AvatarState (completed quests, refused quests)

### Layer 2: Quest Messages

- [ ] T131 [P] [US4] Create QuestOfferMessage record in dotnet/framework/LablabBean.AI.Actors/Messages/QuestOfferMessage.cs
- [ ] T132 [P] [US4] Create GetQuestOffersRequest record in dotnet/framework/LablabBean.AI.Actors/Messages/GetQuestOffersRequest.cs
- [ ] T133 [US4] Implement GetQuestOffersRequest handler in SemanticKernelBridgeActor
- [ ] T134 [US4] Implement QuestOfferMessage handler in NpcActor (present quests to player)

### Layer 1: Quest Integration

- [ ] T135 [US4] Add AICapability.Planning flag to IntelligentAI component (for quest logic)
- [ ] T136 [US4] Extend AvatarRelationship model with reputation score
- [ ] T137 [US4] Track player reputation in relationship system (heroic quests → higher reputation)
- [ ] T138 [US4] Implement quest giver UI in console app (display quest offerings, accept/refuse)

### Personality Configuration

- [ ] T139 [US4] Create wise-quest-giver.json personality in personalities/wise-quest-giver.json

### Integration

- [ ] T140 [US4] Test quest giver acknowledges past completed quests
- [ ] T141 [US4] Test quest difficulty adapts to player reputation
- [ ] T142 [US4] Test quest giver attitude reflects refused quests
- [ ] T143 [US4] Test quest explanations contextualized to player knowledge

**Checkpoint**: Context-aware quest system is fully functional and testable independently

---

## Phase 7: Save/Load Integration

**Purpose**: Persist NPC state across save/load operations

- [ ] T144 Configure Akka.Persistence.Sql with SQLite in appsettings.json
- [ ] T145 [P] Create database initialization for Akka persistence (avatars.db)
- [ ] T146 [P] Implement coordinated save flow in GameStateManager (ECS save → actor snapshots → metadata)
- [ ] T147 Implement coordinated load flow in GameStateManager (ECS load → recreate actors → restore snapshots)
- [ ] T148 [P] Add save metadata tracking (version, timestamp, avatar count) in new SaveMetadata model
- [ ] T149 Test save preserves NPC memories and relationships
- [ ] T150 Test load restores NPC state and conversations resume
- [ ] T151 Test actor recreation and snapshot recovery on load

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories

### Performance Optimization

- [ ] T152 [P] Run BenchmarkDotNet performance tests in dotnet/framework/tests/LablabBean.AI.Agents.Tests/Performance/LatencyBenchmarks.cs
- [ ] T153 Optimize LRU cache hit rate (profile and tune cache key generation)
- [ ] T154 Optimize prompt token usage (compress contexts, abbreviate where possible)
- [ ] T155 Profile actor message throughput (ensure <10ms roundtrip)
- [ ] T156 Profile ECS query performance with intelligent entities (ensure <1ms for 10K entities)

### Logging & Observability

- [ ] T157 [P] Add structured logging for AI decisions (log reasoning, emotional state)
- [ ] T158 [P] Add actor lifecycle tracing (start, stop, restart events)
- [ ] T159 [P] Add circuit breaker state logging (open, half-open, closed transitions)
- [ ] T160 Add performance metrics logging (latency, cache hit rate, fallback frequency)

### Error Handling

- [ ] T161 [P] Add comprehensive exception handling for LLM API failures
- [ ] T162 [P] Add validation for personality JSON files (schema validation on load)
- [ ] T163 Add graceful degradation UI indicators (show when NPC using fallback AI)

### Documentation

- [ ] T164 [P] Create developer quickstart guide in specs/019-intelligent-avatar-system/quickstart.md
- [ ] T165 [P] Document architecture in docs/_inbox/2025-10-24-intelligent-avatar-architecture.md (with YAML front-matter)
- [ ] T166 [P] Update docs/index/registry.json with new documentation entries
- [ ] T167 Document API key setup for development (OpenAI, Ollama alternatives)
- [ ] T168 Document personality JSON schema and examples
- [ ] T169 Document troubleshooting guide (common issues, LLM timeouts, actor failures)

### Testing & Validation

- [ ] T170 Run integration test: Boss with personality and memory (US1 validation)
- [ ] T171 Run integration test: NPC dialogue generation (US2 validation)
- [ ] T172 Run integration test: Adaptive enemy tactics (US3 validation)
- [ ] T173 Run integration test: Quest giver context awareness (US4 validation)
- [ ] T174 Run save/load integration test (verify state persistence)
- [ ] T175 Run performance validation (verify <2s AI decisions, <3s dialogue, 30+ FPS)
- [ ] T176 Run fallback behavior test (simulate LLM outage, verify graceful degradation)

### Code Quality

- [ ] T177 [P] Run code formatter on all new files (dotnet format)
- [ ] T178 [P] Review and document non-obvious code with comments
- [ ] T179 Validate no hardcoded secrets (API keys in config only)
- [ ] T180 Validate all paths are relative (no absolute Windows/Unix paths)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion - MVP target
- **User Story 2 (Phase 4)**: Depends on Foundational completion - Can run parallel to US1 if staffed
- **User Story 3 (Phase 5)**: Depends on Foundational completion - Can run parallel to US1/US2 if staffed
- **User Story 4 (Phase 6)**: Depends on Foundational + US2 (dialogue system) completion
- **Save/Load (Phase 7)**: Depends on at least US1 completion (needs actors to persist)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Boss)**: Independent - only depends on Foundational phase
- **US2 (NPC Dialogue)**: Independent - only depends on Foundational phase
- **US3 (Adaptive Tactics)**: Independent - only depends on Foundational phase, builds on US1 concepts but can work standalone
- **US4 (Quest Giver)**: Depends on US2 (uses dialogue system)

### Within Each User Story

- Layer 3 (SK Agents) before Layer 2 (Akka Actors) before Layer 1 (ECS Integration)
- Models and messages can be created in parallel [P]
- Configuration and hosting after actor implementation
- Integration and testing after all implementation complete

### Parallel Opportunities

**Setup Phase** (can run in parallel):

- T002 (SK packages) + T003 (test packages)
- T005 (AI.Actors project) + T006 (AI.Agents project)
- T007 (Core tests) + T008 (Actors tests) + T009 (Agents tests)
- T010 (project refs) + T011 (package refs)
- T013 (appsettings) + T014 (.gitignore)

**Foundational Phase** (can run in parallel):

- All component creation (T015-T019)
- All model creation (T020-T025)
- All event creation (T026-T029)
- All interface creation (T030-T031)
- All message creation (T032-T040)

**User Stories** (can run in parallel with sufficient team):

- US1 (Phase 3), US2 (Phase 4), US3 (Phase 5) can start simultaneously after Foundational
- US4 (Phase 6) must wait for US2 dialogue system

**Within US1**:

- T046-T048 (agent, prompts, parser) can run in parallel
- T053-T055 (SK bridge) and T056-T065 (avatar actors) can run in parallel

---

## Parallel Example: User Story 1 (Boss)

```bash
# After Foundational phase complete, launch US1 tasks in parallel:

# Parallel Group 1: Layer 3 components
Task: "Create NpcIntelligenceAgent class" (T046)
Task: "Create DecisionPrompts static class" (T047)
Task: "Create DecisionParser class" (T048)

# Sequential: Implement agent logic (depends on T046-T048)
Task: "Implement DecideActionAsync method" (T049)
Task: "Implement LRU cache" (T050)
Task: "Implement circuit breaker" (T051)
Task: "Implement fallback logic" (T052)

# Parallel Group 2: Layer 2 actors (can overlap with agent implementation)
Task: "Create SemanticKernelBridgeActor" (T053)
Task: "Create AvatarActor base class" (T056)
Task: "Create BossActor subclass" (T060)
Task: "Create AvatarSupervisor" (T062)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T014) → ~30 min
2. Complete Phase 2: Foundational (T015-T045) → ~2-3 hours
3. Complete Phase 3: User Story 1 (T046-T085) → ~6-8 hours
4. **STOP and VALIDATE**: Test boss with personality independently
5. Demo intelligent boss to stakeholders

**MVP Delivers**: Single intelligent boss with personality, memory, and adaptive behavior

### Incremental Delivery

1. MVP (US1) → Deploy/Demo (1 week)
2. Add US2 (NPC Dialogue) → Deploy/Demo (1 week)
3. Add US3 (Adaptive Tactics) → Deploy/Demo (1 week)
4. Add US4 (Quest Giver) → Deploy/Demo (1 week)
5. Add Save/Load → Deploy/Demo (1 week)
6. Polish & Optimize → Final release (1 week)

**Total**: 6 weeks for full feature set, 1 week for MVP

### Parallel Team Strategy

With 3 developers:

1. **Week 1**: All devs complete Setup + Foundational together
2. **Week 2+**:
   - Dev A: User Story 1 (Boss)
   - Dev B: User Story 2 (NPC Dialogue)
   - Dev C: User Story 3 (Adaptive Tactics)
3. **Week 3**: Integrate and test all stories
4. **Week 4**: Dev A works on US4, Devs B+C work on Save/Load
5. **Week 5-6**: All devs work on Polish & Optimization

---

## Task Summary

**Total Tasks**: 180

- **Setup**: 14 tasks
- **Foundational**: 31 tasks (BLOCKING)
- **User Story 1 (Boss)**: 40 tasks
- **User Story 2 (NPC Dialogue)**: 21 tasks
- **User Story 3 (Adaptive Tactics)**: 20 tasks
- **User Story 4 (Quest Giver)**: 17 tasks
- **Save/Load**: 8 tasks
- **Polish**: 29 tasks

**Parallel Opportunities**: 71 tasks marked [P] can run concurrently

**Independent Test Criteria**:

- US1: Spawn boss, engage combat, verify personality and memory
- US2: Talk to merchant, verify dialogue and conversation memory
- US3: Fight enemy repeatedly, verify tactical adaptation
- US4: Interact with quest giver at different stages, verify context-aware quests

**Suggested MVP Scope**: Phase 1-3 only (Setup + Foundational + US1) = 85 tasks

---

## Notes

- [P] tasks = different files, no dependencies, safe to parallelize
- [Story] label maps task to specific user story for traceability
- Each user story independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All file paths relative to repository root (follows R-CODE-004)
- API keys in configuration only (follows R-CODE-001)
