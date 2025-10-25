# Quickstart: Kernel Memory Integration

**Last Updated**: 2025-10-25
**Feature**: Kernel Memory Integration for NPC Intelligence
**Target Audience**: Developers setting up local development environment

## Overview

This guide walks through setting up the Kernel Memory integration for local development and testing. For production deployment, see the deployment guide (to be created in Phase 3).

---

## Prerequisites

### Required

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **OpenAI API Key** (for embeddings and chat completion)
  - Sign up at [platform.openai.com](https://platform.openai.com)
  - Minimum tier: Tier 1 (free tier may have rate limits)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** (for version control)

### Optional (for Phase 3+)

- **Docker Desktop** (for Qdrant vector database)
- **Qdrant** ([Installation](https://qdrant.tech/documentation/quick-start/))

---

## Quick Setup (15 minutes)

### Step 1: Clone and Checkout Feature Branch

```bash
cd lablab-bean
git checkout 020-kernel-memory-integration
```

### Step 2: Configure OpenAI API Key

1. Open `appsettings.Development.json` in the console app project:

   ```
   dotnet/console-app/LablabBean.Console/appsettings.Development.json
   ```

2. Add your OpenAI API key:

   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-YOUR-OPENAI-API-KEY-HERE",
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

3. **Important**: Never commit your API key! It's already in `.gitignore`.

### Step 3: Install NuGet Packages

```bash
cd dotnet/framework/LablabBean.AI.Agents

# Add Kernel Memory packages
dotnet add package Microsoft.KernelMemory.Core
dotnet add package Microsoft.KernelMemory.SemanticKernelPlugin

# Restore all dependencies
dotnet restore
```

### Step 4: Build the Solution

```bash
# From repository root
cd dotnet
dotnet build
```

**Expected Output**:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 5: Run Tests

```bash
# Run all tests
dotnet test

# Run only memory service tests
dotnet test --filter "FullyQualifiedName~MemoryService"
```

**Expected Output**:

```
Test Run Successful.
Total tests: 15
     Passed: 15
```

---

## Development Workflow

### Phase 1 (Current): In-Memory Development

**Storage**: `SimpleVectorDb` (in-memory, no persistence)
**Pros**: Zero setup, fast iteration
**Cons**: Data lost on restart

```csharp
// Memories stored in-memory only during application runtime
// Restart = fresh slate (good for testing)
```

### Phase 2: Semantic Search Integration

**Focus**: Replace chronological memory retrieval with semantic search

Before:

```csharp
// Old way: Get last 5 memories (may be irrelevant)
var memories = avatarMemory.ShortTermMemory.TakeLast(5);
```

After:

```csharp
// New way: Get 5 most relevant memories
var memories = await _memoryService.RetrieveRelevantMemoriesAsync(
    entityId: "employee_001",
    contextQuery: "How should I handle this angry customer?",
    options: new MemoryRetrievalOptions { Limit = 5 }
);
```

### Phase 3: Persistent Storage with Qdrant

**Setup Qdrant with Docker**:

1. Create `docker-compose.yml` in repository root:

   ```yaml
   version: '3.8'
   services:
     qdrant:
       image: qdrant/qdrant:latest
       ports:
         - "6333:6333"
       volumes:
         - ./qdrant_storage:/qdrant/storage
   ```

2. Start Qdrant:

   ```bash
   docker-compose up -d
   ```

3. Update `appsettings.Development.json`:

   ```json
   {
     "KernelMemory": {
       "VectorDbType": "Qdrant",
       "Qdrant": {
         "Endpoint": "http://localhost:6333",
         "ApiKey": ""
       }
     }
   }
   ```

4. Memories now persist across application restarts!

---

## Usage Examples

### Example 1: Store and Retrieve Employee Memory

```csharp
using LablabBean.Contracts.AI.Memory;

// 1. Store a memory
var memory = new MemoryEntry
{
    EntityId = "employee_001",
    EventType = "customer_service",
    Description = "Handled an angry customer complaint about delayed order. Remained calm, offered discount, customer left satisfied.",
    Timestamp = DateTime.UtcNow,
    Importance = 0.8f,
    Metadata = new Dictionary<string, object>
    {
        { "emotion", "stressed" },
        { "outcome", "positive" }
    }
};

await _memoryService.StoreMemoryAsync("employee_001", memory);

// 2. Later... retrieve relevant memories
var relevantMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
    entityId: "employee_001",
    contextQuery: "What should I do when a customer is upset?",
    options: new MemoryRetrievalOptions
    {
        MemoryTypes = new List<string> { "customer_service", "interaction" },
        MinRelevance = 0.7f,
        Limit = 3
    }
);

foreach (var result in relevantMemories)
{
    Console.WriteLine($"Relevance: {result.RelevanceScore:F2}");
    Console.WriteLine($"Memory: {result.Memory.Description}");
}
```

**Expected Output**:

```
Relevance: 0.89
Memory: Handled an angry customer complaint about delayed order...

Relevance: 0.76
Memory: Customer was frustrated about pricing...
```

### Example 2: Knowledge Base RAG

```csharp
// 1. Index a knowledge base document
var document = new KnowledgeBaseDocument
{
    DocumentId = "employee_handbook_v1",
    Title = "Employee Customer Service Guidelines",
    Content = @"
        ## Handling Angry Customers

        1. Listen actively without interrupting
        2. Acknowledge their frustration
        3. Offer a concrete solution
        4. Follow up to ensure satisfaction

        ## Escalation Policy

        If customer remains unsatisfied after three attempts,
        escalate to supervisor immediately.
    ",
    RoleTags = new List<string> { "employee" },
    CategoryTags = new List<string> { "policy", "customer_service" },
    LastUpdated = DateTime.UtcNow
};

await _knowledgeBaseService.IndexDocumentAsync(document);

// 2. Query the knowledge base
var answer = await _knowledgeBaseService.QueryKnowledgeBaseAsync(
    question: "What should I do if a customer is angry?",
    roleFilter: "employee",
    categoryFilter: "customer_service"
);

Console.WriteLine($"Answer: {answer.Answer}");
Console.WriteLine($"Source: {answer.Citations[0].SourceTitle}");
```

**Expected Output**:

```
Answer: When handling an angry customer, you should: 1) Listen actively without interrupting, 2) Acknowledge their frustration, 3) Offer a concrete solution, and 4) Follow up to ensure satisfaction.

Source: Employee Customer Service Guidelines (Section: Handling Angry Customers)
```

### Example 3: Tactical Enemy Learning

```csharp
// 1. Enemy observes player behavior
var observation = new TacticalObservation
{
    EnemyEntityId = "tactical_enemy_005",
    PlayerBehavior = PlayerBehavior.AggressiveRush,
    Situation = "Player charged directly through corridor toward 3 enemies",
    EffectivenessRating = 0.85f, // Worked well for player
    CounterTactic = "flanking",
    Timestamp = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        { "player_health", 0.75f },
        { "enemy_count", 3 }
    }
};

await _memoryService.StoreTacticalObservationAsync("tactical_enemy_005", observation);

// 2. Later... retrieve similar tactical situations
var similarTactics = await _memoryService.RetrieveSimilarTacticsAsync(
    currentSituation: "Player is charging toward us in narrow corridor",
    behaviorFilter: PlayerBehavior.AggressiveRush,
    limit: 5
);

// 3. Use learned tactics to adapt strategy
var counters = similarTactics
    .Where(t => t.EffectivenessRating > 0.7f)
    .Select(t => t.CounterTactic)
    .Distinct();

Console.WriteLine($"Recommended counter-tactics: {string.Join(", ", counters)}");
```

**Expected Output**:

```
Recommended counter-tactics: flanking, defensive_positioning, focus_fire
```

---

## Testing

### Unit Tests

```bash
# Test memory storage and retrieval
dotnet test --filter "FullyQualifiedName~MemoryServiceTests"

# Test knowledge base operations
dotnet test --filter "FullyQualifiedName~KnowledgeBaseServiceTests"
```

### Integration Tests

```bash
# Test full Kernel Memory integration
dotnet test --filter "FullyQualifiedName~KernelMemoryIntegrationTests"
```

### Manual Testing

1. Run the console application:

   ```bash
   cd dotnet/console-app/LablabBean.Console
   dotnet run
   ```

2. Trigger NPC decision-making scenarios
3. Observe console logs for memory retrieval:

   ```
   [MemoryService] Retrieved 3 relevant memories (avg relevance: 0.82)
   [EmployeeAgent] Making decision based on contextual memories...
   ```

---

## Troubleshooting

### Issue: "OpenAI API key not configured"

**Solution**: Ensure `appsettings.Development.json` has valid `OpenAI.ApiKey`

### Issue: "Embedding API rate limit exceeded"

**Solution**:

- Tier 1 accounts: 200 requests/minute (should be sufficient)
- Reduce memory creation frequency during testing
- Retry logic handles transient rate limits automatically

### Issue: "Memory retrieval returns no results"

**Possible causes**:

1. `MinRelevance` threshold too high (try lowering to 0.5)
2. No memories stored yet for that entity
3. Query context doesn't semantically match stored memories

**Debug**:

```csharp
// Lower relevance threshold temporarily
options.MinRelevance = 0.0f; // Returns all memories ranked by relevance

// Check if any memories exist
var allMemories = await _memoryService.RetrieveRelevantMemoriesAsync(
    entityId,
    "any",
    new MemoryRetrievalOptions { MinRelevance = 0.0f, Limit = 100 }
);
Console.WriteLine($"Total memories for {entityId}: {allMemories.Count()}");
```

### Issue: "Build error: Package not found"

**Solution**:

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

---

## Configuration Reference

### Development Configuration (In-Memory)

```json
{
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

**Use Case**: Local development, unit tests, fast iteration

### Production Configuration (Qdrant)

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

**Use Case**: Production deployment, cross-session persistence

---

## Next Steps

After completing local setup:

1. **Read the Spec**: Review `spec.md` for feature requirements
2. **Review Contracts**: Study `contracts/` for service interfaces
3. **Check Data Model**: Understand entities in `data-model.md`
4. **Run Tasks**: Use `/speckit.tasks` to generate implementation tasks (Phase 2)

---

## Support

- **Documentation**: `docs/findings/kernel-memory-integration-analysis.md`
- **Research**: `specs/020-kernel-memory-integration/research.md`
- **Issues**: GitHub Issues (tag: `kernel-memory`)

---

## Appendix: Useful Commands

```bash
# Check .NET version
dotnet --version

# List installed packages
dotnet list package

# Clean build artifacts
dotnet clean

# Watch for file changes and rebuild
dotnet watch run

# Generate code coverage report
dotnet test /p:CollectCoverage=true

# Benchmark memory service performance
dotnet run --project benchmarks/MemoryServiceBenchmarks
```

---

**Setup Time**: ~15 minutes for in-memory development
**Production Setup Time**: ~30 minutes (includes Docker/Qdrant)
