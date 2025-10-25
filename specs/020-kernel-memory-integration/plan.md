# Implementation Plan: Kernel Memory Integration for NPC Intelligence

**Branch**: `020-kernel-memory-integration` | **Date**: 2025-10-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/020-kernel-memory-integration/spec.md`

## Summary

This feature integrates Microsoft Kernel Memory to provide semantic search, RAG (Retrieval Augmented Generation), and persistent memory capabilities for NPC intelligence. The primary requirement is to replace the current chronological memory retrieval (last 5 memories) with semantic similarity-based retrieval, enabling NPCs to make contextually relevant decisions. The technical approach uses Kernel Memory as an embedded service with in-memory storage for development and configurable persistent storage (Qdrant recommended) for production.

## Technical Context

**Language/Version**: C# / .NET 8.0 (based on existing project structure)
**Primary Dependencies**:

- Microsoft.SemanticKernel (already integrated)
- Microsoft.KernelMemory.Core
- Microsoft.KernelMemory.SemanticKernelPlugin
- Microsoft.KernelMemory.MemoryDb.Qdrant (Phase 3)
- Akka.NET (existing - for actor system)
- Arch.Core (existing - for ECS)

**Storage**:

- Development: In-memory SimpleVectorDb
- Production: Qdrant (vector database) or PostgreSQL with pgvector
- Content Storage: SimpleFileStorage (development) → Azure Blobs or file system (production)

**Testing**: xUnit (existing .NET test framework based on project structure)

**Target Platform**:

- Windows, Linux, macOS (cross-platform .NET console/desktop application)
- Game runtime environment with Akka.NET actors

**Project Type**: Multi-project .NET solution with contracts-based architecture

**Performance Goals**:

- Memory retrieval: <200ms p95 latency
- Semantic search relevance: >0.7 average score
- Embedding generation: Handle 100+ memories/second with queuing
- Cross-session persistence: 100% memory retention

**Constraints**:

- Must integrate with existing Semantic Kernel agents without major rewrites
- Backward compatibility with existing AvatarMemory data structures during migration
- OpenAI API rate limits for embedding generation
- Memory storage growth must be bounded (configurable retention policies)
- Graceful degradation when persistence layer unavailable

**Scale/Scope**:

- 100+ concurrent NPCs (employees, bosses, tactical enemies)
- 50-100 memories per NPC
- 10,000+ total memory entries in production
- Knowledge bases: 5-10 documents (personality files, policies, lore)
- Session duration: Hours (memories must persist across application restarts)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: PASS (No constitution violations detected)

The project constitution template has not been customized. Based on standard .NET best practices and existing project architecture:

✅ **Contracts-First Architecture**: Feature will follow existing pattern of defining interfaces in `LablabBean.Contracts.*` projects before implementation
✅ **Testability**: xUnit tests will be created for all new services (MemoryService, configuration, integration)
✅ **Dependency Injection**: All services registered via `IServiceCollection` extensions following existing patterns in `ServiceCollectionExtensions.cs`
✅ **Configuration-Driven**: All settings exposed via `appsettings.json` and strongly-typed options classes
✅ **Graceful Degradation**: Memory-only fallback mode when persistence unavailable

**No violations to justify**

## Project Structure

### Documentation (this feature)

```
specs/020-kernel-memory-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (technology decisions, best practices)
├── data-model.md        # Phase 1 output (memory entities, relationships)
├── quickstart.md        # Phase 1 output (developer setup guide)
├── contracts/           # Phase 1 output (service interfaces)
│   ├── IMemoryService.cs
│   ├── IKnowledgeBaseService.cs
│   └── DTOs.cs
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/framework/
├── LablabBean.AI.Agents/                    # EXISTING - Modify
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs   # Add Kernel Memory registration
│   ├── Services/                            # NEW
│   │   ├── MemoryService.cs
│   │   ├── KnowledgeBaseService.cs
│   │   └── TacticalMemoryService.cs
│   ├── Configuration/
│   │   ├── SemanticKernelOptions.cs         # EXISTING
│   │   └── KernelMemoryOptions.cs           # NEW
│   ├── Models/                              # NEW
│   │   ├── MemoryResult.cs
│   │   ├── TacticalMemory.cs
│   │   └── RelationshipEvent.cs
│   ├── EmployeeIntelligenceAgent.cs         # MODIFY (update memory retrieval)
│   ├── BossIntelligenceAgent.cs             # MODIFY (update memory retrieval)
│   └── TacticsAgent.cs                      # MODIFY (add tactical memory storage/retrieval)
│
├── LablabBean.Contracts.AI/                 # NEW PROJECT (if not exists)
│   └── Memory/
│       ├── IMemoryService.cs
│       ├── IKnowledgeBaseService.cs
│       └── DTOs/
│           ├── MemoryEntry.cs
│           ├── MemoryFilter.cs
│           └── RetrievalOptions.cs
│
├── LablabBean.Core/                         # EXISTING
│   └── Models/
│       └── AvatarMemory.cs                  # MODIFY (add migration helpers)
│
└── tests/
    └── LablabBean.AI.Agents.Tests/          # NEW (if not exists)
        ├── Services/
        │   ├── MemoryServiceTests.cs
        │   └── KnowledgeBaseServiceTests.cs
        └── Integration/
            └── KernelMemoryIntegrationTests.cs

dotnet/console-app/
└── LablabBean.Console/
    └── Program.cs                            # MODIFY (register new services)

appsettings.Development.json                  # MODIFY (add KernelMemory config)
```

**Structure Decision**:

This is a multi-project .NET solution following a contracts-based architecture. The implementation follows existing patterns:

1. **Contracts Project**: Define interfaces in a new `LablabBean.Contracts.AI` project (or add to existing contracts if one exists for AI)
2. **Implementation**: Add services to existing `LablabBean.AI.Agents` project
3. **Testing**: Create comprehensive unit and integration tests in `tests/LablabBean.AI.Agents.Tests`
4. **Configuration**: Extend existing configuration patterns with `KernelMemoryOptions`
5. **Integration Points**: Modify existing agents (`EmployeeIntelligenceAgent`, `BossIntelligenceAgent`, `TacticsAgent`) to use new memory service

This approach minimizes disruption to existing architecture while adding the new capability as a clean extension.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

N/A - No constitutional violations detected. The implementation follows established project patterns.
