# Kernel Memory Integration: Resilience, Retention, and RAG

This document explains how the Kernel Memory-based NPC memory system is wired, how to enable resilience (retry/backoff), how retention works, and how to use the Memory and RAG services.

## Overview

- Semantic memory storage and retrieval via Microsoft Kernel Memory
- Knowledge-grounded answers (RAG) with citations
- Resilience hooks for retry/backoff and circuit breaking
- Per-entity write serialization to avoid races
- Time-based retention cleanup to prevent unbounded growth

Key services:

- `IMemoryService` (NPC memories): dotnet/framework/LablabBean.AI.Agents/Services/MemoryService.cs
- `IRagService` (KB queries with citations): dotnet/framework/LablabBean.Contracts.AI/Memory/RagService.cs

## Tag Schema (Canonical)

- `entity_id`: NPC/entity identifier
- `memory_type`: category (e.g., interaction, observation, tactical, relationship)
- `importance`: numeric (0.0–1.0)
- `timestamp`: ISO 8601 string (UTC)
- user tags: any additional keys (stored as-is)

The reader is backward-compatible with older keys `entity` and `type`.

## Configuration

Use the Agents configuration class `LablabBean.AI.Agents.Configuration.KernelMemoryOptions` (duplicate removed):

```
KernelMemory:
  Storage:
    Provider: "Volatile" | "Qdrant" | "AzureAISearch"
    ConnectionString: "http://localhost:6333"   # for Qdrant
    CollectionName: "game_memories"
  Embedding:
    Provider: "OpenAI"
    ModelName: "text-embedding-3-small"
    MaxTokens: 8191
  TextGeneration:
    Provider: "OpenAI"
    ModelName: "gpt-4"
    MaxTokens: 4096
```

DI registration:

```csharp
// Registers IKernelMemory, IMemoryService, IRagService
services.AddKernelMemoryService(configuration);
```

## Enabling Resilience (Retry/Backoff)

The memory and RAG services accept an optional `LablabBean.Contracts.Resilience.Services.IService` for resilience (Polly-based). If registered, Kernel Memory calls run with retry/backoff; otherwise they execute directly.

Example wiring using the Resilience Polly plugin:

```csharp
using LablabBean.Contracts.Resilience.Services;
using LablabBean.Plugins.Resilience.Polly.Services;

// Register resilience service
services.AddSingleton<IService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ResilienceService>>();
    return new ResilienceService(logger);
});

// Then add Kernel Memory services (they’ll pick up IService automatically)
services.AddKernelMemoryService(configuration);
```

## Per-Entity Concurrency Guards

`IMemoryService` serializes writes per `entity_id` to avoid race conditions. This is implemented with `SemaphoreSlim` keyed by entity.

## Retention Policy (Time-Based Cleanup)

Use `CleanOldMemoriesAsync(entityId, olderThan)` to delete memories older than a cutoff. This:

- Searches by `entity_id`
- Filters by `timestamp <= (UtcNow - olderThan)`
- Deletes matching documents (with resilience if enabled)

Example:

```csharp
var deleted = await memoryService.CleanOldMemoriesAsync("npc_001", TimeSpan.FromDays(30));
logger.LogInformation("Deleted {Count} old memories", deleted);
```

## Using IMemoryService

```csharp
// Store
await memoryService.StoreMemoryAsync(new MemoryEntry
{
  Id = Guid.NewGuid().ToString("N"),
  EntityId = "npc_001",
  Content = "Handled a difficult customer politely and offered a refund",
  MemoryType = "interaction",
  Importance = 0.8,
  Timestamp = DateTimeOffset.UtcNow,
  Tags = new Dictionary<string, string> { ["scenario"] = "customer_service" }
});

// Retrieve
var results = await memoryService.RetrieveRelevantMemoriesAsync(
  "how to handle angry customers",
  new MemoryRetrievalOptions { EntityId = "npc_001", MinRelevanceScore = 0.7, Limit = 5 });
```

## Using IRagService (Answers + Citations)

```csharp
// Index a KB document
await ragService.IndexDocumentAsync(new KnowledgeBaseDocument
{
  DocumentId = "employee-handbook-001",
  Title = "Employee Customer Service Handbook",
  Content = "...",
  Category = "handbook",
  Role = "employee"
});

// Query
var answer = await ragService.QueryKnowledgeBaseAsync(
  "How should I handle an angry customer?",
  role: "employee",
  category: "handbook",
  maxCitations: 3);

if (answer.IsGrounded)
{
  foreach (var c in answer.Citations)
  {
    logger.LogInformation("{Doc} (score {Score:F2})", c.DocumentTitle, c.RelevanceScore);
  }
}
```

## Notes

- Kernel Memory ID lookup is not direct; retrieval is done via search + partition parsing.
- If resilience isn’t registered in DI, memory operations run without retries.
- Tag schema is now canonical across writer and reader paths.
