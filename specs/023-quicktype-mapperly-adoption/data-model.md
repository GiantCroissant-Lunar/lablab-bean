# Data Model: quicktype and Mapperly Production Adoption

**Feature**: 023-quicktype-mapperly-adoption
**Date**: 2025-10-27
**Status**: Design Complete
**Related**: [plan.md](./plan.md) | [spec.md](./spec.md) | [research.md](./research.md)

## Overview

This document defines all entities, models, and mapping relationships involved in migrating from manual JSON parsing and object mapping to automated code generation using quicktype (for external APIs) and Mapperly (for internal object mapping).

**Key Insight from Research**: Mapperly is NOT appropriate for the Avatar State serialization use case. The current implementation should be kept as-is. This document focuses on where code generation WILL be used: Qdrant API models.

---

## Entity Categories

### 1. External API Models (quicktype-generated)

Models representing external API request/response structures, generated from JSON schemas.

### 2. Internal Domain Models (existing)

Domain entities that already exist in the codebase and will consume generated models.

### 3. Mapping Infrastructure (Mapperly - for future use)

Mapper interfaces for future simple object-to-object mappings (NOT for Avatar State).

---

## 1. External API Models (quicktype)

### 1.1 QdrantSearchRequest

**Purpose**: Request structure for Qdrant vector search API endpoint

**Source**: `schemas/qdrant/search-request.schema.json` (to be created from OpenAPI spec)

**API Endpoint**: `POST /collections/{collection_name}/points/search`

**Properties**:

| Property | Type | Required | Nullable | Description |
|----------|------|----------|----------|-------------|
| `vector` | `List<float>` | ✅ Yes | ❌ No | Query vector for similarity search |
| `limit` | `int` | ✅ Yes | ❌ No | Maximum number of results to return (topK) |
| `with_payload` | `bool` | ❌ No | ❌ No | Include payload in response (default: true) |
| `with_vector` | `bool` | ❌ No | ❌ No | Include vectors in response (default: false) |
| `filter` | `object?` | ❌ No | ✅ Yes | Filter conditions for search results |
| `score_threshold` | `float?` | ❌ No | ✅ Yes | Minimum similarity score filter |
| `offset` | `int?` | ❌ No | ✅ Yes | Pagination offset |

**Generated File**: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/Qdrant/QdrantSearchRequest.g.cs`

**Namespace**: `LablabBean.Framework.Generated.ExternalApis.Qdrant`

**Usage Example**:

```csharp
var request = new QdrantSearchRequest
{
    Vector = embedding.ToList(),
    Limit = 10,
    WithPayload = true,
    WithVector = false
};
```

**Current State**: Manual anonymous object creation in `QdrantVectorStore.cs:55-60`

---

### 1.2 QdrantSearchResponse

**Purpose**: Response wrapper for Qdrant search API containing result array

**Source**: `schemas/qdrant/search-response.schema.json` (to be created from OpenAPI spec)

**API Endpoint**: `POST /collections/{collection_name}/points/search` (response)

**Properties**:

| Property | Type | Required | Nullable | Description |
|----------|------|----------|----------|-------------|
| `result` | `List<QdrantScoredPoint>` | ✅ Yes | ❌ No | Array of search results ordered by score |
| `status` | `string` | ✅ Yes | ❌ No | Response status ("ok" or error message) |
| `time` | `float?` | ❌ No | ✅ Yes | Execution time in seconds |

**Generated File**: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/Qdrant/QdrantSearchResponse.g.cs`

**Namespace**: `LablabBean.Framework.Generated.ExternalApis.Qdrant`

**Relationships**:

- Contains array of `QdrantScoredPoint`
- Maps to `List<VectorSearchResult>` in domain model

**Usage Example**:

```csharp
using var stream = await response.Content.ReadAsStreamAsync(ct);
var searchResponse = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(stream, cancellationToken: ct);

foreach (var point in searchResponse.Result)
{
    Console.WriteLine($"ID: {point.Id}, Score: {point.Score}");
}
```

**Current State**: Manually parsed using `JsonDocument` in `QdrantVectorStore.cs:64-94`

---

### 1.3 QdrantScoredPoint

**Purpose**: Individual search result with score, ID, payload, and optional vector

**Source**: `schemas/qdrant/scored-point.schema.json` (to be created from OpenAPI spec)

**Properties**:

| Property | Type | Required | Nullable | Description |
|----------|------|----------|----------|-------------|
| `id` | `string` | ✅ Yes | ❌ No | Point ID (union type: string\|integer from API, converted to string) |
| `score` | `float` | ✅ Yes | ❌ No | Similarity score (0.0 to 1.0) |
| `payload` | `Dictionary<string, JsonElement>?` | ❌ No | ✅ Yes | Dynamic payload data (omitted if `with_payload=false`) |
| `vector` | `List<float>?` | ❌ No | ✅ Yes | Vector embeddings (omitted if `with_vector=false`) |

**Generated File**: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/Qdrant/QdrantScoredPoint.g.cs`

**Namespace**: `LablabBean.Framework.Generated.ExternalApis.Qdrant`

**Type Handling Notes**:

- **`id` field**: Qdrant API returns union type (string|integer). Generated code will use `string` representation.
- **`payload` field**: Dynamic JSON object. Using `Dictionary<string, JsonElement>` for maximum flexibility.
- **Nullable properties**: `payload` and `vector` can be absent from API response.

**Mapping to Domain Model**:

```csharp
// QdrantScoredPoint → VectorSearchResult
new VectorSearchResult
{
    Id = point.Id,  // Already string
    Score = point.Score,
    Payload = point.Payload?.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.ToString()
    ) ?? new Dictionary<string, string>()
};
```

**Current State**: Manually extracted from JSON with `TryGetProperty` in `QdrantVectorStore.cs:74-92`

---

### 1.4 QdrantPointRequest (Future)

**Purpose**: Request structure for upserting points to Qdrant collection

**Source**: `schemas/qdrant/point-request.schema.json` (to be created)

**API Endpoint**: `POST /collections/{collection_name}/points`

**Status**: Out of scope for initial migration (Phase 1 focuses on search only)

**Properties** (for reference):

| Property | Type | Required | Nullable | Description |
|----------|------|----------|----------|-------------|
| `points` | `List<QdrantPoint>` | ✅ Yes | ❌ No | Array of points to upsert |

---

## 2. Internal Domain Models (Existing)

### 2.1 VectorSearchResult

**Purpose**: Domain model representing a vector search result (abstraction over provider-specific formats)

**Location**: `dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs` (inline class, lines ~20-30)

**Properties**:

| Property | Type | Nullable | Description |
|----------|------|----------|-------------|
| `Id` | `string` | ❌ No | Search result ID |
| `Score` | `float` | ❌ No | Similarity score |
| `Payload` | `Dictionary<string, string>` | ❌ No | Metadata key-value pairs |

**Mapping from QdrantScoredPoint**:

```csharp
QdrantScoredPoint (external) → VectorSearchResult (domain)
- id (string) → Id (string) [direct copy]
- score (float) → Score (float) [direct copy]
- payload (Dictionary<string, JsonElement>?) → Payload (Dictionary<string, string>) [convert JsonElement to string]
```

**Transformation Logic**:

```csharp
public List<VectorSearchResult> MapSearchResults(List<QdrantScoredPoint> points)
{
    return points.Select(point => new VectorSearchResult
    {
        Id = point.Id,
        Score = point.Score,
        Payload = point.Payload?.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString()
        ) ?? new Dictionary<string, string>()
    }).ToList();
}
```

**Validation Rules**:

- `Id` must not be empty
- `Score` should be between 0.0 and 1.0 (but not enforced)
- `Payload` defaults to empty dictionary if null

---

### 2.2 AvatarState (NO CHANGES)

**Purpose**: Domain model representing avatar state in the game

**Location**: `dotnet/framework/LablabBean.AI.Core/Models/AvatarState.cs`

**Status**: **NO MAPPERLY MIGRATION** - Research determined current serialization approach is correct

**Properties**:

| Property | Type | Computed | Description |
|----------|------|----------|-------------|
| `EntityId` | `string` | ❌ | Avatar unique identifier |
| `Health` | `float` | ❌ | Current health points |
| `MaxHealth` | `float` | ❌ | Maximum health points |
| `CurrentBehavior` | `string` | ❌ | Current behavior state |
| `EmotionalState` | `string` | ❌ | Current emotional state |
| `Stats` | `Dictionary<string, float>` | ❌ | Dynamic stats dictionary |
| `ActiveEffects` | `List<string>` | ❌ | Active effect IDs |
| `LastUpdated` | `DateTime` | ❌ | Last state update timestamp |
| `HealthPercentage` | `float` | ✅ Yes | Computed: Health / MaxHealth |
| `IsAlive` | `bool` | ✅ Yes | Computed: Health > 0 |

**Why No Mapperly**:

- Target is `AvatarStateSnapshot` which requires JSON serialization, not property mapping
- Multi-source mapping (requires both `AvatarState` and `AvatarMemory`)
- Generated timestamp property (`SnapshotTime`)
- Contains `Dictionary<string, object>` in related `AvatarMemory` (not supported by Mapperly)

---

### 2.3 AvatarStateSnapshot (NO CHANGES)

**Purpose**: Persistence DTO for serialized avatar state and memory

**Location**: `dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs`

**Status**: **KEEP CURRENT FACTORY METHOD** - No Mapperly migration

**Properties**:

| Property | Type | Nullable | Description |
|----------|------|----------|-------------|
| `EntityId` | `string` | ❌ No | Avatar identifier |
| `StateJson` | `string` | ❌ No | Serialized AvatarState JSON |
| `MemoryJson` | `string` | ❌ No | Serialized AvatarMemory JSON |
| `SnapshotTime` | `DateTime` | ❌ No | Timestamp when snapshot was created |

**Current Creation Method** (KEEP THIS):

```csharp
public static AvatarStateSnapshot Create(AvatarState state, AvatarMemory memory)
{
    return new AvatarStateSnapshot
    {
        EntityId = state.EntityId,
        StateJson = AvatarStateSerializer.Serialize(state),
        MemoryJson = JsonSerializer.Serialize(memory),
        SnapshotTime = DateTime.UtcNow
    };
}
```

**Why This Is Correct**:

- Serialization is the correct semantic operation (not property mapping)
- Handles `Dictionary<string, object>` in `AvatarMemory` correctly via JSON serialization
- Multi-source orchestration is clear and explicit
- Generated timestamp is business logic, not mapping concern

---

## 3. Mapping Infrastructure (Mapperly - Future Use)

### 3.1 When to Use Mapperly

**Appropriate Scenarios** (for future implementation):

1. **DTO-to-Domain Mapping**:
   - External API DTO → Internal domain entity
   - Database entity → API response DTO
   - Form data → Command object

2. **Object Cloning**:
   - Deep copying domain objects
   - Creating snapshot copies for testing
   - Duplicating entities with modifications

3. **Simple Structure Transformations**:
   - Renaming properties (e.g., `FirstName` → `GivenName`)
   - Flattening nested objects (e.g., `Address.City` → `City`)
   - Combining related properties

**Configuration Template**:

```csharp
[Mapper(UseDeepCloning = true)]
public partial interface IExampleMapper
{
    // Ignore computed properties
    [MapperIgnoreTarget(nameof(Target.ComputedField))]
    Target Map(Source source);

    // Collection mapping
    List<Target> MapList(List<Source> sources);
}
```

---

### 3.2 When NOT to Use Mapperly

**Inappropriate Scenarios** (like Avatar State):

1. **Serialization/Deserialization**:
   - Object ↔ JSON string
   - Object ↔ XML string
   - Object ↔ Binary format

2. **Dictionary<string, object>**:
   - Dynamic metadata with unknown types
   - Polymorphic value types
   - Use JSON serialization instead

3. **Multi-Source Mapping**:
   - Combining data from multiple objects
   - Requires orchestration and business logic
   - Use factory methods or builders

4. **Generated/Computed Values**:
   - Timestamps (`DateTime.UtcNow`)
   - Generated IDs (`Guid.NewGuid()`)
   - Calculated aggregates

5. **Complex Business Logic**:
   - Conditional transformations
   - External service calls
   - Validation and error handling

**Example of Incorrect Use**:

```csharp
// ❌ DON'T DO THIS - Use JSON serialization
[Mapper]
public partial interface IAvatarSnapshotMapper
{
    // This won't work correctly for Dictionary<string, object>
    // and doesn't handle multi-source mapping
    AvatarStateSnapshot ToSnapshot(AvatarState state, AvatarMemory memory);
}
```

**Correct Approach**:

```csharp
// ✅ DO THIS - Use factory method with JSON serialization
public static AvatarStateSnapshot Create(AvatarState state, AvatarMemory memory)
{
    return new AvatarStateSnapshot
    {
        EntityId = state.EntityId,
        StateJson = JsonSerializer.Serialize(state),
        MemoryJson = JsonSerializer.Serialize(memory),
        SnapshotTime = DateTime.UtcNow
    };
}
```

---

## Entity Relationships

```
External API Layer:
┌─────────────────────────────────────┐
│  Qdrant API (HTTP/JSON)             │
└─────────────────────────────────────┘
                 │
                 │ POST /collections/{name}/points/search
                 │
                 ▼
┌─────────────────────────────────────┐
│  QdrantSearchResponse               │◄─── quicktype-generated
│  - result: List<QdrantScoredPoint>  │
│  - status: string                   │
│  - time: float?                     │
└─────────────────────────────────────┘
                 │
                 │ Contains
                 ▼
┌─────────────────────────────────────┐
│  QdrantScoredPoint                  │◄─── quicktype-generated
│  - id: string                       │
│  - score: float                     │
│  - payload: Dictionary?             │
│  - vector: List<float>?             │
└─────────────────────────────────────┘
                 │
                 │ Maps to (manual transformation)
                 ▼
Domain Layer:
┌─────────────────────────────────────┐
│  VectorSearchResult                 │◄─── Internal domain model
│  - Id: string                       │
│  - Score: float                     │
│  - Payload: Dictionary<string,str>  │
└─────────────────────────────────────┘
```

**No Mapperly Involvement**: The transformation from `QdrantScoredPoint` to `VectorSearchResult` is straightforward LINQ and doesn't require a mapper interface. Future complex transformations may use Mapperly.

---

## Validation Rules

### QdrantSearchRequest Validation

- `vector` must not be empty (length > 0)
- `limit` must be positive integer (> 0)
- `limit` typically capped at API maximum (e.g., 100)

### QdrantScoredPoint Validation

- `id` must not be empty or whitespace
- `score` should be in range [0.0, 1.0] (not enforced by model)
- `payload` can be null (when `with_payload=false`)
- `vector` can be null (when `with_vector=false`)

### VectorSearchResult Validation

- `Id` must not be empty
- `Score` no range validation (accept API values as-is)
- `Payload` never null (defaults to empty dictionary)

---

## Namespace Organization

```
LablabBean.Framework.Generated.ExternalApis.Qdrant/
├── QdrantSearchRequest.g.cs         (generated)
├── QdrantSearchResponse.g.cs        (generated)
├── QdrantScoredPoint.g.cs           (generated)
└── (future: QdrantPoint.g.cs, etc.)

LablabBean.Framework.Generated.Mappers/
└── (empty - ready for future mapper interfaces)

Domain Models (existing locations):
- VectorSearchResult: QdrantVectorStore.cs (inline)
- AvatarState: LablabBean.AI.Core/Models/
- AvatarStateSnapshot: LablabBean.AI.Actors/Persistence/
```

---

## Summary: What's Being Generated

| Entity | Tool | Source | Status |
|--------|------|--------|--------|
| `QdrantSearchRequest` | quicktype | OpenAPI schema | ✅ Will generate |
| `QdrantSearchResponse` | quicktype | OpenAPI schema | ✅ Will generate |
| `QdrantScoredPoint` | quicktype | OpenAPI schema | ✅ Will generate |
| `VectorSearchResult` | N/A | Manual code | ✅ Keep as-is (simple LINQ transformation) |
| `AvatarState` | N/A | Manual code | ✅ Keep as-is (no changes) |
| `AvatarStateSnapshot` | N/A | Manual code | ✅ Keep as-is (no Mapperly) |
| `AvatarMemory` | N/A | Manual code | ✅ Keep as-is (no changes) |

**Code Generation Scope**: Only Qdrant API models. No Mapperly usage in initial migration.

---

## Next Steps

1. ✅ Create JSON schemas in `contracts/` directory (extracted from Qdrant OpenAPI spec)
2. ✅ Update Build.cs with proper quicktype flags
3. ✅ Generate models using `nuke GenerateApiTypes`
4. ✅ Update `QdrantVectorStore.cs` to use generated models
5. ✅ Add project reference from Qdrant plugin to Framework.Generated
6. ✅ Run integration tests to validate behavior

---

**Document Version**: 1.0
**Last Updated**: 2025-10-27
**Status**: Design complete, ready for schema creation
