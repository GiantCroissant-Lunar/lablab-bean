# Research: Kernel Memory Integration

**Date**: 2025-10-25
**Feature**: Kernel Memory Integration for NPC Intelligence
**Phase**: 0 - Technology Research & Decision Documentation

## Purpose

This document resolves technical unknowns from the implementation plan and documents best practices for integrating Microsoft Kernel Memory into the existing Semantic Kernel-based NPC intelligence system.

---

## Research Areas

### 1. Kernel Memory Deployment Model

**Question**: Should we use Kernel Memory as embedded service, web service, or plugin?

**Research Findings**:

**Option A: Embedded Service (Serverless Mode)**

- Runs in-process with the application
- Direct library integration via NuGet packages
- No separate service to deploy/manage
- Suitable for single-instance applications

**Option B: Web Service**

- Runs as separate REST API service
- Supports multiple clients
- Adds deployment complexity
- Better for distributed systems

**Option C: Semantic Kernel Plugin**

- Integrates as SK plugin
- Limited API surface
- Best for simple RAG scenarios

**Decision**: **Embedded Service (Option A)**

**Rationale**:

1. The game is a single-instance console/desktop application, not a distributed system
2. In-process execution minimizes latency (critical for <200ms retrieval goal)
3. Simpler deployment - no separate service infrastructure needed
4. Full Kernel Memory API access for advanced features
5. Existing SK integration can coexist with embedded KM

**Alternatives Considered**:

- Web Service rejected: Adds unnecessary complexity and latency for single-instance game
- SK Plugin rejected: Too limited for tactical memory and relationship queries

---

### 2. Vector Database Selection

**Question**: Which vector database should we use for persistent memory storage?

**Research Findings**:

**Option A: Qdrant**

- Purpose-built vector database
- Excellent performance (millisecond queries)
- Docker deployment available
- Rich filtering capabilities
- Active development

**Option B: PostgreSQL with pgvector**

- Leverages existing RDBMS if present
- Mature, well-understood technology
- Good for hybrid relational + vector data
- Performance adequate for medium scale

**Option C: Redis with RediSearch**

- In-memory speed
- Simple deployment
- Limited persistence guarantees
- Good for caching layer

**Option D: Azure AI Search**

- Managed service (Azure only)
- Excellent scaling
- Requires Azure subscription
- Higher cost

**Decision**: **Qdrant (Option A)** for production, **SimpleVectorDb (in-memory)** for development

**Rationale**:

1. **Performance**: Qdrant optimized for vector similarity search, meeting <200ms latency goal
2. **Development Simplicity**: SimpleVectorDb (built into KM) requires zero setup for local dev
3. **Production Ready**: Qdrant Docker container easy to deploy
4. **Filtering**: Qdrant's payload filtering perfect for entity ID, type, importance filters
5. **Cost**: Self-hosted Qdrant has no per-query costs (unlike managed services)
6. **Cross-Platform**: Works on Windows, Linux, macOS

**Alternatives Considered**:

- PostgreSQL rejected: Project doesn't use Postgres currently; would add new dependency
- Redis rejected: Memory-only doesn't meet persistence requirements
- Azure AI Search rejected: Vendor lock-in, requires Azure subscription

**Migration Path**:

- Phase 1-2: SimpleVectorDb (in-memory) for development and testing
- Phase 3: Add Qdrant support with Docker Compose configuration
- Configuration-driven: Switch backends via `appsettings.json` without code changes

---

### 3. Embedding Strategy

**Question**: How should we manage embedding generation costs and rate limits?

**Research Findings**:

**Embedding Model Options**:

- `text-embedding-3-small`: 1536 dimensions, $0.00002/1K tokens, fast
- `text-embedding-3-large`: 3072 dimensions, $0.00013/1K tokens, higher quality
- `text-embedding-ada-002`: 1536 dimensions, legacy, similar cost to small

**Cost Analysis** (based on scale/scope):

- 10,000 memories × 50 tokens average = 500K tokens
- text-embedding-3-small: $0.01 for initial indexing
- Ongoing: ~1,000 new memories/day × 50 tokens = 50K tokens/day = $0.001/day
- **Annual cost**: ~$0.37 (negligible)

**Rate Limits** (OpenAI):

- Tier 1 (new accounts): 200 requests/minute, 1M tokens/minute
- At 50 tokens/memory: Can embed 20,000 memories/minute
- Project scale (100 memories/minute worst case): Well within limits

**Decision**: **text-embedding-3-small** with async queuing

**Rationale**:

1. **Cost**: Negligible ($0.37/year even with generous estimates)
2. **Quality**: Sufficient for memory similarity (tested extensively by Microsoft)
3. **Speed**: Faster than large model, meets latency requirements
4. **Compatibility**: Same model already configured in SemanticKernelOptions
5. **Rate Limits**: Project scale nowhere near limits; simple queue sufficient

**Implementation Strategy**:

```csharp
// Async queueing for non-blocking memory storage
public async Task StoreMemoryAsync(string entityId, MemoryEntry memory)
{
    // Queue for background processing
    await _memoryQueue.EnqueueAsync(new StorageTask
    {
        EntityId = entityId,
        Memory = memory
    });
}

// Background worker processes queue with retry logic
while (await _memoryQueue.DequeueAsync())
{
    try
    {
        await _kernelMemory.ImportTextAsync(...);
    }
    catch (RateLimitException ex)
    {
        // Exponential backoff retry
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
        _memoryQueue.Requeue(task);
    }
}
```

**Alternatives Considered**:

- text-embedding-3-large rejected: Overkill for use case, 6.5x more expensive, slower
- Batch embedding rejected: Adds complexity, unnecessary given rate limit headroom
- Local embedding models rejected: Adds deployment complexity, quality unknown

---

### 4. Memory Tagging Strategy

**Question**: What tagging scheme should we use for efficient memory filtering?

**Research Findings**:

**Kernel Memory Tagging**:

- Tags are key-value pairs attached to documents
- Used for filtering during search: `filter.ByTag("entity", "employee_001")`
- Efficient index-based filtering (not post-search filtering)
- Supports multiple tags per document

**Decision**: **Hierarchical tagging scheme**

**Tag Schema**:

```csharp
// Core tags (always present)
{
    "entity": "{entityId}",           // e.g., "employee_001", "boss_003"
    "type": "{memoryType}",           // e.g., "decision", "interaction", "event"
    "importance": "{0.0-1.0}",        // e.g., "0.85"
    "timestamp": "{ISO8601}",         // e.g., "2025-10-25T14:30:00Z"
}

// Type-specific tags (conditional)
{
    // For "interaction" type
    "interaction_type": "{type}",     // e.g., "customer_service", "conflict"
    "target_entity": "{entityId}",    // e.g., "customer_042"

    // For tactical memories
    "behavior": "{PlayerBehavior}",   // e.g., "aggressive_rush"
    "effectiveness": "{0.0-1.0}",     // e.g., "0.72"

    // For relationship events
    "participant_1": "{entityId}",
    "participant_2": "{entityId}",
    "outcome": "{positive|negative|neutral}"
}
```

**Rationale**:

1. **Entity Isolation**: `entity` tag ensures cross-entity memory privacy
2. **Type-Based Retrieval**: `type` tag enables filtering by memory category
3. **Importance Filtering**: `importance` tag supports "critical memories only" queries
4. **Time-Based Queries**: `timestamp` tag enables "recent memories" or "memories from session X"
5. **Extensibility**: Type-specific tags allow specialized queries without schema changes

**Query Examples**:

```csharp
// Employee decision-making: Get relevant interaction memories
var filter = new MemoryFilter()
    .ByTag("entity", employeeId)
    .ByTag("type", "interaction")
    .ByTag("importance", "0.7", MemoryFilter.Comparison.GreaterThan);

// Tactical enemy: Get aggressive rush counter-tactics
var filter = new MemoryFilter()
    .ByTag("type", "tactical")
    .ByTag("behavior", "aggressive_rush")
    .ByTag("effectiveness", "0.5", MemoryFilter.Comparison.GreaterThan);

// Relationship history: Get conflict events between two NPCs
var filter = new MemoryFilter()
    .ByTag("type", "relationship")
    .ByTag("participant_1", npcId)
    .ByTag("participant_2", targetId)
    .ByTag("outcome", "negative");
```

**Alternatives Considered**:

- Flat namespace rejected: Harder to evolve schema
- Document ID encoding rejected: Less flexible than tag-based filtering
- No tagging (query only) rejected: Performance penalty on large datasets

---

### 5. Migration Strategy

**Question**: How do we migrate existing AvatarMemory data to Kernel Memory?

**Research Findings**:

**Existing Data**:

- `AvatarMemory` class with ShortTermMemory and LongTermMemory lists
- In-memory only (lost on restart)
- Each `MemoryEntry` has: EventType, Description, Timestamp, Metadata, Importance

**Decision**: **One-time migration utility + Dual-write period**

**Migration Approach**:

**Phase 1: Dual-Write (Weeks 1-4)**

```csharp
// When new memory created
var memoryEntry = new MemoryEntry { ... };

// Write to legacy system
avatarMemory.ShortTermMemory.Add(memoryEntry);

// Write to new system (async, non-blocking)
await _memoryService.StoreMemoryAsync(entityId, memoryEntry);
```

**Phase 2: Dual-Read with KM Preference (Weeks 5-6)**

```csharp
// Try new system first
var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
    entityId, context, limit: 5);

// Fallback to legacy if KM unavailable
if (!memories.Any())
{
    memories = avatarMemory.ShortTermMemory.TakeLast(5);
}
```

**Phase 3: Migration Utility (Week 6)**

```csharp
// Migrate existing in-memory data on startup (optional)
public async Task MigrateLegacyMemories(string entityId, AvatarMemory legacy)
{
    foreach (var memory in legacy.ShortTermMemory.Concat(legacy.LongTermMemory))
    {
        await _memoryService.StoreMemoryAsync(entityId, memory);
    }
}
```

**Phase 4: KM Only (Week 7+)**

- Remove legacy memory retrieval code
- Keep `AvatarMemory` class for backward compatibility (empty implementation)

**Rationale**:

1. **Zero Downtime**: Dual-write ensures no data loss during migration
2. **Gradual Rollout**: Can test KM retrieval while legacy system still active
3. **Rollback Ready**: Can revert to legacy system if KM issues found
4. **Data Preservation**: Migration utility preserves historical memories (optional)
5. **Clean Cutover**: Clear phase 4 milestone for removing legacy code

**Alternatives Considered**:

- Big-bang migration rejected: Too risky, no rollback path
- Permanent dual-write rejected: Adds complexity indefinitely
- No migration rejected: Loses valuable historical context

---

### 6. Configuration Best Practices

**Question**: What configuration structure best supports development and production environments?

**Research Findings**:

**Kernel Memory Configuration Patterns**:

- Builder-based API for programmatic configuration
- Supports configuration file (JSON) binding
- Environment-specific overrides via `appsettings.{Environment}.json`

**Decision**: **Strongly-typed options with environment overrides**

**Configuration Structure**:

**`appsettings.Development.json`** (in-memory, no persistence):

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE",
    "EmbeddingModelId": "text-embedding-3-small",
    "ModelId": "gpt-4o"
  },
  "KernelMemory": {
    "VectorDbType": "SimpleVectorDb",
    "ContentStorageType": "SimpleFileStorage",
    "Retrieval": {
      "MinRelevanceThreshold": 0.7,
      "DefaultMaxResults": 5,
      "TimeoutSeconds": 30
    }
  }
}
```

**`appsettings.Production.json`** (Qdrant, persistent):

```json
{
  "KernelMemory": {
    "VectorDbType": "Qdrant",
    "ContentStorageType": "SimpleFileStorage",
    "Qdrant": {
      "Endpoint": "http://localhost:6333",
      "ApiKey": ""
    },
    "Retrieval": {
      "MinRelevanceThreshold": 0.75,
      "DefaultMaxResults": 10,
      "TimeoutSeconds": 60
    }
  }
}
```

**Strongly-Typed Options Class**:

```csharp
public class KernelMemoryOptions
{
    public string VectorDbType { get; set; } = "SimpleVectorDb";
    public string ContentStorageType { get; set; } = "SimpleFileStorage";
    public QdrantOptions Qdrant { get; set; } = new();
    public RetrievalOptions Retrieval { get; set; } = new();
}

public class RetrievalOptions
{
    public float MinRelevanceThreshold { get; set; } = 0.7f;
    public int DefaultMaxResults { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 30;
}
```

**Registration**:

```csharp
services.Configure<KernelMemoryOptions>(
    configuration.GetSection("KernelMemory"));
```

**Rationale**:

1. **Type Safety**: Compile-time checking of configuration properties
2. **IntelliSense**: IDE support for configuration discovery
3. **Validation**: Can use Data Annotations for validation
4. **Environment Separation**: Clear dev vs. prod configuration
5. **Testability**: Easy to mock/override in tests

**Alternatives Considered**:

- Magic strings rejected: Error-prone, no compile-time checking
- Environment variables only rejected: Harder to document, no structure
- Database configuration rejected: Overkill for static settings

---

## Summary of Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| **Deployment Model** | Embedded Service | In-process, low latency, simple deployment |
| **Vector Database** | Qdrant (prod) / SimpleVectorDb (dev) | Performance, ease of use, cost |
| **Embedding Model** | text-embedding-3-small | Cost-effective, sufficient quality, fast |
| **Tagging Scheme** | Hierarchical with core + type-specific tags | Flexible, efficient filtering, extensible |
| **Migration Strategy** | Dual-write → Dual-read → Migration utility → KM only | Zero downtime, gradual rollout, rollback ready |
| **Configuration** | Strongly-typed options with environment overrides | Type safety, validation, clear dev/prod separation |

---

## Next Steps

Phase 0 research complete. Proceed to **Phase 1: Design & Contracts**:

1. Create `data-model.md` with entity definitions
2. Generate service contracts in `contracts/` directory
3. Create `quickstart.md` for developer onboarding
4. Update agent context with new technology decisions
