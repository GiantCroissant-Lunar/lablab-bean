# Implementation Plan: Intelligent Avatar System

**Branch**: `019-intelligent-avatar-system` | **Date**: 2025-10-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/019-intelligent-avatar-system/spec.md`

## Summary

Implement a three-layer intelligent avatar architecture combining Arch ECS (entity data storage), Akka.NET (actor lifecycle management), and Semantic Kernel (AI reasoning) to create NPCs and enemies with personality, memory, emotional states, and adaptive behavior. The system enables context-aware AI decisions, natural language dialogue generation, and tactical adaptation while maintaining fault tolerance through actor supervision and graceful degradation to simple AI when LLM services are unavailable.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**:

- Arch ECS 2.0.0 (entity-component system)
- Akka.NET 1.5.31 (actor system, hosting, persistence)
- Microsoft.SemanticKernel 1.25.0 (AI orchestration)
- Microsoft.SemanticKernel.Agents.Core 1.25.0-alpha (agent framework)
- Microsoft.SemanticKernel.Connectors.OpenAI 1.25.0 (LLM integration)
- Microsoft.Extensions.* (DI, logging, configuration)
- Serilog (structured logging)
- System.Text.Json (serialization)

**Storage**:

- Akka.Persistence with SQLite (actor state persistence)
- JSON files (NPC personality configurations)
- In-memory (ECS world state, chat histories)

**Testing**:

- xUnit 2.5.3 (unit + integration tests)
- NSubstitute 5.1.0 (mocking)
- FluentAssertions 6.12.0 (assertions)
- BenchmarkDotNet 0.13.12 (performance testing)

**Target Platform**: Windows/Linux/macOS desktop (.NET 8 cross-platform)

**Project Type**: Multi-project .NET solution with framework libraries and console application host

**Performance Goals**:

- AI decisions: <2s (95th percentile), <5s timeout with fallback
- Dialogue generation: <3s (90th percentile)
- Event bus throughput: maintain existing 1.1M+ events/sec
- ECS queries: <1ms for 10,000 entities
- Actor message roundtrip: <10ms
- Support 10 concurrent intelligent avatars at 30+ FPS

**Constraints**:

- LLM API latency: 100-2000ms typical, up to 5000ms timeout
- Memory per NPC: max 10 recent interactions (~5KB per NPC)
- No blocking operations on game loop thread
- Graceful degradation when AI services unavailable
- Save/load must preserve complete NPC state
- Must integrate with existing event bus (IEventBus)

**Scale/Scope**:

- 5-10 intelligent avatars (bosses, NPCs, quest givers) per game session
- Hundreds of simple AI enemies (using existing enum-based behaviors)
- Max 10 memories per NPC (circular buffer)
- Relationship tracking for 1 player initially (future: multiplayer)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Project constitution is currently a template. Applying general .NET best practices and existing project conventions from docs/ARCHITECTURE.md and .agent/base/ rules.

### Applied Principles

✅ **Library-First**: Intelligent avatar functionality will be implemented as standalone framework libraries in `dotnet/framework/`:

- `LablabBean.AI.Core` (bridge components, interfaces)
- `LablabBean.AI.Actors` (Akka.NET actor implementations)
- `LablabBean.AI.Agents` (Semantic Kernel agent logic)

✅ **Test-First**: All new components will have corresponding test projects with contract, integration, and unit tests following existing pattern in `dotnet/framework/tests/`

✅ **Integration with Existing Systems**: Must integrate cleanly with:

- Existing Arch ECS world (`LablabBean.Game.Core`)
- Existing event bus (`LablabBean.Plugins.Core/EventBus.cs`)
- Existing DI container (Microsoft.Extensions.DependencyInjection)
- Existing console host (`LablabBean.Console`)

✅ **Documentation Standards**: Follow R-DOC rules from `.agent/base/20-rules.md`:

- New docs to `docs/_inbox/`
- YAML front-matter required
- Update registry.json

✅ **Code Quality**: Follow R-CODE rules:

- No hardcoded secrets (API keys in configuration)
- Meaningful names
- Comment non-obvious code
- Relative paths only

✅ **Observability**: Structured logging with Serilog, event tracing for AI decisions and actor lifecycle

### Constitution Compliance: PASS ✅

- No violations requiring justification
- Follows existing project structure conventions
- Integrates with established patterns (ECS, event bus, DI, reactive extensions)
- Adds three new framework libraries (reasonable for three-layer architecture)

## Project Structure

### Documentation (this feature)

```
specs/019-intelligent-avatar-system/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0: Technology decisions and patterns
├── data-model.md        # Phase 1: Entity and component designs
├── quickstart.md        # Phase 1: Developer quickstart guide
├── contracts/           # Phase 1: Message and interface contracts
│   ├── messages.md      # Actor messages and events
│   ├── components.md    # ECS bridge components
│   └── agents.md        # SK agent interfaces
├── checklists/
│   └── requirements.md  # Quality validation (complete)
└── tasks.md             # Phase 2: Implementation tasks (/speckit.tasks - not created yet)
```

### Source Code (repository root)

```
dotnet/
├── LablabBean.sln
├── Directory.Packages.props          # Add Akka.NET + Semantic Kernel packages
│
├── framework/
│   ├── LablabBean.AI.Core/          # NEW: Core AI abstractions and bridge components
│   │   ├── Components/               # ECS bridge components
│   │   │   ├── AkkaActorRef.cs      # Links entity to actor
│   │   │   ├── SemanticAgent.cs     # Links entity to SK agent
│   │   │   └── IntelligentAI.cs     # Enhanced AI component
│   │   ├── Models/
│   │   │   ├── AvatarContext.cs     # Context for AI decisions
│   │   │   ├── AvatarState.cs       # Actor persistent state
│   │   │   └── AIDecision.cs        # Decision result
│   │   ├── Interfaces/
│   │   │   ├── IAvatarActor.cs      # Actor contract
│   │   │   └── IIntelligenceAgent.cs # SK agent contract
│   │   └── Events/
│   │       ├── AIThoughtEvent.cs
│   │       ├── AIBehaviorChangedEvent.cs
│   │       └── NPCDialogueEvent.cs
│   │
│   ├── LablabBean.AI.Actors/        # NEW: Akka.NET actor implementations
│   │   ├── Actors/
│   │   │   ├── AvatarActor.cs       # Base avatar actor
│   │   │   ├── BossActor.cs         # Boss-specific behavior
│   │   │   ├── NpcActor.cs          # NPC-specific behavior
│   │   │   └── AvatarSupervisor.cs  # Supervision strategy
│   │   ├── Bridges/
│   │   │   ├── EventBusAkkaAdapter.cs    # Event bus ↔ Akka bridge
│   │   │   └── SemanticKernelBridge.cs   # Actor → SK bridge
│   │   ├── Messages/
│   │   │   ├── TakeDamageMessage.cs
│   │   │   ├── PlayerNearbyMessage.cs
│   │   │   ├── DialogueRequestMessage.cs
│   │   │   ├── AIDecisionMessage.cs
│   │   │   └── [... other messages]
│   │   ├── Persistence/
│   │   │   └── AvatarStateSerializer.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs  # DI registration
│   │
│   ├── LablabBean.AI.Agents/        # NEW: Semantic Kernel agent logic
│   │   ├── Agents/
│   │   │   ├── NpcIntelligenceAgent.cs    # Core AI agent
│   │   │   ├── DialogueAgent.cs           # Dialogue generation
│   │   │   ├── TacticsAgent.cs            # Combat tactics
│   │   │   └── QuestAgent.cs              # Quest logic
│   │   ├── Prompts/
│   │   │   ├── DecisionPrompts.cs         # AI decision prompts
│   │   │   ├── DialoguePrompts.cs         # Dialogue prompts
│   │   │   └── TacticsPrompts.cs          # Tactics prompts
│   │   ├── Parsers/
│   │   │   ├── DecisionParser.cs          # Parse SK JSON responses
│   │   │   └── DialogueParser.cs
│   │   ├── Configuration/
│   │   │   └── SemanticKernelOptions.cs   # Config model
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── LablabBean.Game.Core/        # EXISTING: Extend with new systems
│   │   └── Systems/
│   │       └── IntelligentAISystem.cs     # NEW: Intelligent AI coordinator
│   │
│   └── LablabBean.Plugins.Core/     # EXISTING: Extend event bus
│       └── Events/                   # Add new event types (see AI.Core/Events)
│
├── framework/tests/                  # NEW: Test projects
│   ├── LablabBean.AI.Core.Tests/
│   │   ├── Components/               # Component tests
│   │   ├── Models/                   # Model tests
│   │   └── Events/                   # Event tests
│   │
│   ├── LablabBean.AI.Actors.Tests/
│   │   ├── Unit/                     # Unit tests (mocked dependencies)
│   │   │   ├── AvatarActorTests.cs
│   │   │   ├── BossActorTests.cs
│   │   │   └── SupervisorTests.cs
│   │   ├── Integration/              # Integration tests (real Akka system)
│   │   │   ├── ActorLifecycleTests.cs
│   │   │   ├── EventBusBridgeTests.cs
│   │   │   └── PersistenceTests.cs
│   │   └── Contract/                 # Message contract tests
│   │       └── MessageSerializationTests.cs
│   │
│   └── LablabBean.AI.Agents.Tests/
│       ├── Unit/
│       │   ├── NpcIntelligenceAgentTests.cs
│       │   ├── DialogueAgentTests.cs
│       │   └── ParserTests.cs
│       ├── Integration/              # Integration tests (mocked LLM)
│       │   ├── AgentWorkflowTests.cs
│       │   └── FallbackTests.cs
│       └── Performance/              # Performance benchmarks
│           └── LatencyBenchmarks.cs
│
└── console-app/
    └── LablabBean.Console/
        ├── appsettings.json          # EXTEND: Add SK and Akka config
        └── Program.cs                 # EXTEND: Register AI services

# Config files (repository root)
├── appsettings.Development.json      # NEW: Dev LLM API keys (gitignored)
├── appsettings.Production.json       # NEW: Prod LLM config
└── personalities/                     # NEW: NPC personality definitions
    ├── ancient-dragon.json
    ├── cautious-merchant.json
    └── wise-quest-giver.json
```

**Structure Decision**: Chose multi-project structure to maintain separation of concerns across the three layers:

1. **AI.Core**: Shared abstractions, bridge components, models (no dependencies on Akka or SK)
2. **AI.Actors**: Akka.NET-specific implementations (depends on AI.Core + Akka.NET)
3. **AI.Agents**: Semantic Kernel-specific implementations (depends on AI.Core + Semantic Kernel)

This allows:

- Testing each layer independently
- Future swapping of actor system or LLM provider
- Clear dependency graph: Core ← Actors/Agents ← Game.Core ← Console
- Follows existing project convention (`LablabBean.*.Core`, `LablabBean.*.Tests`)

## Complexity Tracking

*No constitution violations requiring justification. This section intentionally left empty.*

---

## Phase 0: Research & Technology Decisions

*Output: research.md*

Research tasks identified from Technical Context:

1. **Akka.NET Best Practices** for .NET 8
   - Hosting integration with Microsoft.Extensions.Hosting
   - Actor supervision strategies for AI/LLM failures
   - Persistence options (SQLite vs in-memory for dev)
   - Message serialization (System.Text.Json vs Hyperion)

2. **Semantic Kernel Integration Patterns**
   - Agent orchestration vs single kernel
   - Prompt engineering for game AI decisions
   - Parsing structured JSON responses from LLMs
   - Timeout and retry strategies
   - Cost optimization (caching, prompt compression)

3. **ECS-Actor Bridge Patterns**
   - Component vs tag for actor references
   - Entity lifecycle coordination (ECS destroy → Actor stop)
   - Query performance with actor-enabled entities
   - World serialization with actor state

4. **Event Bus ↔ Actor Communication**
   - Bidirectional event forwarding patterns
   - Actor selection vs direct references
   - Event ordering guarantees
   - Performance impact of bridging

5. **Fallback AI Strategies**
   - Timeout detection and fallback triggers
   - Circuit breaker pattern for LLM calls
   - Caching decisions for common scenarios
   - Graceful degradation UX patterns

6. **Save/Load with Actor State**
   - Akka.Persistence snapshot strategies
   - Coordinating ECS save with actor persistence
   - Actor recovery on game load
   - Migration strategies for schema changes

7. **LLM API Integration**
   - OpenAI API best practices (rate limits, quotas)
   - Local LLM alternatives (Ollama, LM Studio) for dev/testing
   - Prompt token limits and chunking strategies
   - Structured output (JSON mode) reliability

---

## Phase 1: Design & Contracts

*Output: data-model.md, contracts/*, quickstart.md, updated agent context*

### Prerequisites

- research.md complete with all technology decisions made

### Deliverables

1. **data-model.md**: Entity and component designs
   - ECS bridge components (AkkaActorRef, SemanticAgent, IntelligentAI)
   - Actor state models (AvatarState, AvatarMemory, AvatarRelationship)
   - Decision models (AIDecision, DialogueContext, TacticalPlan)
   - Event models (all new event types)
   - Validation rules and state transitions

2. **contracts/messages.md**: Actor message contracts
   - All Akka.NET message types with schemas
   - Serialization format (JSON)
   - Message routing patterns

3. **contracts/components.md**: ECS component contracts
   - Component struct definitions
   - Query patterns for intelligent entities
   - Component lifecycle (add/remove/update)

4. **contracts/agents.md**: Semantic Kernel agent contracts
   - Agent interface definitions (IIntelligenceAgent)
   - Context structures (AvatarContext, DialogueContext)
   - Response formats (JSON schemas for LLM outputs)
   - Prompt templates

5. **quickstart.md**: Developer quickstart guide
   - Environment setup (API keys, dependencies)
   - Creating your first intelligent boss
   - Testing an NPC dialogue
   - Running benchmarks
   - Troubleshooting guide

6. **Updated agent context**: Technology additions to `.agent/adapters/claude.md`
   - Akka.NET actor system patterns
   - Semantic Kernel agent usage
   - Three-layer integration points

---

## Phase 2: Task Breakdown

*Not performed by /speckit.plan command. Use /speckit.tasks after completing Phase 1.*

The `/speckit.tasks` command will generate `tasks.md` with:

- Dependency-ordered implementation tasks
- Test-first development tasks
- Integration milestones
- Performance validation tasks

---

## Next Steps

1. ✅ **Phase 0**: Execute research tasks and document decisions in `research.md`
2. ⏭️ **Phase 1**: Design data models and generate contracts
3. ⏭️ **Phase 2**: Run `/speckit.tasks` to generate implementation task breakdown
4. ⏭️ **Implementation**: Execute via `/speckit.implement` or manual development

---

**Plan Status**: Phase 0 (Research) ready to begin
**Last Updated**: 2025-10-24
