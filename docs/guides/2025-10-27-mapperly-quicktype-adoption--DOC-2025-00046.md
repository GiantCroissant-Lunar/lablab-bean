---
doc_id: DOC-2025-00046
title: Mapperly and quicktype Adoption Guide
doc_type: guide
status: draft
canonical: false
created: 2025-10-27
tags: [code-generation, tooling, mapperly, quicktype, object-mapping, json-serialization, nuke-build]
summary: Comprehensive guide for adopting Mapperly (object mapping) and quicktype (JSON type generation) to replace manual mapping and JSON parsing patterns in the codebase.
source:
  author: agent
  agent: claude
  model: sonnet-4.5
related: [DOC-2025-00018, DOC-2025-00029]
---

# Mapperly and quicktype Adoption Guide

## Executive Summary

This guide documents the decision and implementation plan for adopting two battle-tested third-party tools to improve code quality, reduce boilerplate, and enhance type safety:

- **Mapperly**: Compile-time object mapper (replaces manual property mapping)
- **quicktype**: JSON-to-strongly-typed-model generator (replaces manual JSON parsing)

**Status**: Ready for implementation after SPEC-022 completion

**Decision Rationale**: Use robust, well-maintained tools instead of reinventing the wheel with custom mapping and parsing code.

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Why Adopt These Tools](#why-adopt-these-tools)
3. [Mapperly Integration](#mapperly-integration)
4. [quicktype Integration](#quicktype-integration)
5. [NUKE Build Integration](#nuke-build-integration)
6. [Migration Strategy](#migration-strategy)
7. [Usage Examples](#usage-examples)
8. [Best Practices](#best-practices)
9. [References](#references)

---

## Current State Analysis

### Existing Patterns

#### 1. Manual Object Mapping

**Location**: Throughout codebase, especially in:

- `dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs`
- Entity snapshot creation
- Analytics data transformations

**Current Pattern**:

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

**Issues**:

- ❌ Verbose and repetitive
- ❌ Error-prone (typos not caught until runtime)
- ❌ Manual maintenance when models change
- ❌ No automatic null handling
- ❌ Every mapper requires custom unit tests

#### 2. Manual JSON Parsing

**Location**:

- `dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs`
- Firebase integrations
- External API responses

**Current Pattern**:

```csharp
using var doc = await JsonDocument.ParseAsync(stream);
var root = doc.RootElement;
if (!root.TryGetProperty("result", out var result))
    return Array.Empty<VectorSearchResult>();

foreach (var item in result.EnumerateArray())
{
    var id = item.GetProperty("id").GetString();
    var score = item.GetProperty("score").GetDouble();
    // ... more manual extraction
}
```

**Issues**:

- ❌ No IntelliSense for API response properties
- ❌ Property name typos only caught at runtime
- ❌ Difficult to refactor when APIs change
- ❌ No compile-time validation
- ❌ Unclear API structure from code alone

### Code Generation Already in Use

The project successfully uses source generators for:

- `ProxyServiceGenerator` (service proxy implementations)
- `ReportProviderGenerator` (report provider registry)
- Arch ECS (entity-component-system boilerplate)
- ReactiveUI.Fody (reactive property weaving)

**Insight**: Team is already comfortable with source generators. Mapperly fits this pattern perfectly.

---

## Why Adopt These Tools

### Mapperly Benefits

| Benefit | Impact | Example |
|---------|--------|---------|
| **Zero runtime cost** | Compile-time generation, no reflection | Generated code is as fast as hand-written |
| **Type safety** | Compiler catches mapping errors | Rename property → build fails until mapper updated |
| **Consistency** | All mappings follow same pattern | Easier code reviews, onboarding |
| **Maintainability** | Models change → mappers auto-update | 70% less maintenance overhead |
| **Testing** | Less custom code to test | Focus tests on business logic, not plumbing |
| **Null safety** | Automatic null handling with C# 11+ | Fewer NullReferenceExceptions |

**Community Validation**:

- 1.5M+ NuGet downloads
- Active development (latest: v3.6.0, Oct 2024)
- Used by major .NET projects
- Strong GitHub community (1.2k+ stars)

### quicktype Benefits

| Benefit | Impact | Example |
|---------|--------|---------|
| **Strongly-typed models** | IntelliSense for API responses | `response.Result.Id` (not string lookups) |
| **Compile-time validation** | Typos caught at build time | `reponse.Reslt` → compiler error |
| **Self-documenting** | Models show API structure clearly | New devs understand API instantly |
| **Refactoring safety** | Find all usages of API properties | "Find References" works for API fields |
| **Multi-language** | Generate TypeScript for frontend too | Share types across .NET + Node.js |
| **API evolution tracking** | Regenerate when APIs change | Version control shows API diffs |

**Community Validation**:

- 12k+ GitHub stars
- Supports 20+ languages
- Industry standard for API type generation
- Active maintenance

---

## Mapperly Integration

### Step 1: Add Package

**Update `Directory.Packages.props`**:

```xml
<ItemGroup>
  <!-- Existing packages -->
  <PackageVersion Include="Riok.Mapperly" Version="3.6.0" />
</ItemGroup>
```

**Add to projects** (e.g., `LablabBean.AI.Actors.csproj`):

```xml
<ItemGroup>
  <PackageReference Include="Riok.Mapperly" />
</ItemGroup>
```

### Step 2: Create Mappers

**Convention**: Place mappers in `Mapping/` subdirectories within each project.

**Example 1: Avatar State Mapper**

```csharp
// dotnet/framework/LablabBean.AI.Actors/Mapping/AvatarMapper.cs

using Riok.Mapperly.Abstractions;
using LablabBean.AI.Actors.Models;
using LablabBean.AI.Actors.Persistence;

namespace LablabBean.AI.Actors.Mapping;

/// <summary>
/// Mapper for avatar-related entities.
/// Generated at compile time by Mapperly.
/// </summary>
[Mapper]
public partial class AvatarMapper
{
    /// <summary>
    /// Maps AvatarState to AvatarStateSnapshot for persistence.
    /// </summary>
    public partial AvatarStateSnapshot ToSnapshot(AvatarState state);

    /// <summary>
    /// Maps AvatarState to AvatarStateDto for API responses.
    /// </summary>
    [MapProperty(nameof(AvatarState.EntityId), nameof(AvatarStateDto.Id))]
    public partial AvatarStateDto ToDto(AvatarState state);

    /// <summary>
    /// Custom serialization for complex properties.
    /// </summary>
    [MapProperty(nameof(AvatarMemory), nameof(AvatarStateSnapshot.MemoryJson),
                 Use = nameof(SerializeMemory))]
    private string SerializeMemory(AvatarMemory memory)
        => JsonSerializer.Serialize(memory, JsonOptions.Default);
}
```

**What Mapperly Generates**:

```csharp
// Generated code (example - actual output may vary)
public partial class AvatarMapper
{
    public partial AvatarStateSnapshot ToSnapshot(AvatarState state)
    {
        var target = new AvatarStateSnapshot();
        target.EntityId = state.EntityId;
        target.StateJson = AvatarStateSerializer.Serialize(state);
        target.MemoryJson = SerializeMemory(state.Memory);
        target.SnapshotTime = DateTime.UtcNow;
        return target;
    }
}
```

**Example 2: Entity Mapper**

```csharp
// dotnet/framework/LablabBean.Core/Mapping/EntityMapper.cs

using Riok.Mapperly.Abstractions;

namespace LablabBean.Core.Mapping;

[Mapper]
public partial class EntityMapper
{
    /// <summary>
    /// Map game entity to display model for UI.
    /// </summary>
    public partial EntityDisplayModel ToDisplayModel(GameEntity entity);

    /// <summary>
    /// Map collection of entities (Mapperly handles IEnumerable automatically).
    /// </summary>
    public partial List<EntityDisplayModel> ToDisplayModels(IEnumerable<GameEntity> entities);
}
```

### Step 3: Register Mappers for DI (Optional)

```csharp
// In your DI container registration
services.AddSingleton<AvatarMapper>();
services.AddSingleton<EntityMapper>();
```

**Or use directly** (no state, so static usage is fine):

```csharp
var mapper = new AvatarMapper();
var snapshot = mapper.ToSnapshot(avatarState);
```

### Advanced Mapperly Features

#### Flattening/Unflattening

```csharp
[Mapper]
public partial class OrderMapper
{
    // Flattening: Order.Customer.Name → OrderDto.CustomerName
    [MapProperty(nameof(Order.Customer.Name), nameof(OrderDto.CustomerName))]
    public partial OrderDto ToDto(Order order);
}
```

#### Custom Value Converters

```csharp
[Mapper]
public partial class DateMapper
{
    public partial EventDto ToDto(Event evt);

    [MapProperty(nameof(Event.Timestamp), nameof(EventDto.TimestampIso),
                 Use = nameof(ToIso8601))]
    private string ToIso8601(DateTimeOffset timestamp) => timestamp.ToString("O");
}
```

#### Ignore Properties

```csharp
[Mapper]
public partial class UserMapper
{
    [MapperIgnore(nameof(User.PasswordHash))]
    public partial UserDto ToDto(User user);
}
```

---

## quicktype Integration

### Step 1: Install quicktype

**Global installation** (requires Node.js):

```bash
npm install -g quicktype
```

**Or use npx** (no installation needed):

```bash
npx quicktype --help
```

### Step 2: Prepare JSON Schemas/Examples

**Option A: Create JSON Schema**

```json
// schemas/qdrant-api.json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "definitions": {
    "SearchResponse": {
      "type": "object",
      "required": ["result"],
      "properties": {
        "result": {
          "type": "array",
          "items": { "$ref": "#/definitions/SearchResult" }
        }
      }
    },
    "SearchResult": {
      "type": "object",
      "required": ["id", "score"],
      "properties": {
        "id": { "type": "string" },
        "score": { "type": "number" },
        "payload": { "type": "object" },
        "vector": {
          "type": "array",
          "items": { "type": "number" }
        }
      }
    }
  }
}
```

**Option B: Use Sample JSON** (quicktype infers schema):

```json
// schemas/qdrant-sample.json
{
  "result": [
    {
      "id": "abc-123",
      "score": 0.95,
      "payload": { "category": "dungeon" },
      "vector": [0.1, 0.2, 0.3]
    }
  ]
}
```

### Step 3: Generate C# Models

**Manual generation**:

```bash
quicktype schemas/qdrant-api.json \
  --lang csharp \
  --namespace LablabBean.Plugins.VectorStore.Qdrant.Models \
  --out dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/Models/QdrantModels.g.cs \
  --features just-types \
  --csharp-version 11
```

**Output example** (QdrantModels.g.cs):

```csharp
// <auto-generated />
// Generated by quicktype

namespace LablabBean.Plugins.VectorStore.Qdrant.Models;

using System.Text.Json.Serialization;

public partial class SearchResponse
{
    [JsonPropertyName("result")]
    public SearchResult[] Result { get; set; } = Array.Empty<SearchResult>();
}

public partial class SearchResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("payload")]
    public Dictionary<string, object>? Payload { get; set; }

    [JsonPropertyName("vector")]
    public double[]? Vector { get; set; }
}
```

---

## NUKE Build Integration

### Add Code Generation Target

**Update `build/nuke/Build.cs`**:

```csharp
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build : NukeBuild
{
    /// <summary>
    /// Directory containing JSON schemas for quicktype.
    /// </summary>
    AbsolutePath SchemasDirectory => RootDirectory / "schemas";

    /// <summary>
    /// Generate strongly-typed models from JSON schemas/samples.
    /// </summary>
    Target GenerateApiTypes => _ => _
        .Description("Generate C# models from JSON schemas using quicktype")
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information("Generating API types from JSON schemas...");

            // Ensure quicktype is available
            try
            {
                ProcessTasks.StartProcess("quicktype", "--version")
                    .AssertZeroExitCode();
            }
            catch
            {
                Log.Warning("quicktype not found. Installing globally...");
                NpmInstall(s => s
                    .SetGlobal(true)
                    .AddPackages("quicktype"));
            }

            // Generate Qdrant API models
            GenerateQdrantModels();

            // Generate Firebase models (if needed)
            // GenerateFirebaseModels();

            Log.Information("✓ API type generation complete");
        });

    void GenerateQdrantModels()
    {
        var schemaFile = SchemasDirectory / "qdrant-api.json";
        var outputFile = RootDirectory / "dotnet" / "plugins" /
                        "LablabBean.Plugins.VectorStore.Qdrant" /
                        "Models" / "QdrantModels.g.cs";

        if (!FileExists(schemaFile))
        {
            Log.Warning($"Schema file not found: {schemaFile}");
            return;
        }

        Log.Information($"Generating Qdrant models from {schemaFile}...");

        ProcessTasks.StartProcess(
            "quicktype",
            $"--src \"{schemaFile}\" " +
            $"--lang csharp " +
            $"--namespace LablabBean.Plugins.VectorStore.Qdrant.Models " +
            $"--out \"{outputFile}\" " +
            $"--features just-types " +
            $"--csharp-version 11 " +
            $"--array-type array"
        ).AssertZeroExitCode();

        Log.Information($"✓ Generated: {outputFile}");
    }

    /// <summary>
    /// Update Compile target to depend on code generation.
    /// </summary>
    Target Compile => _ => _
        .DependsOn(GenerateApiTypes)
        .DependsOn(Restore)
        .Executes(() =>
        {
            // Existing compile logic
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
}
```

### Directory Structure

```
lablab-bean/
├── schemas/                          # JSON schemas/samples for quicktype
│   ├── qdrant-api.json
│   ├── qdrant-sample.json
│   ├── firebase-admin-api.json
│   └── README.md                     # Schema documentation
├── dotnet/
│   └── plugins/
│       └── LablabBean.Plugins.VectorStore.Qdrant/
│           └── Models/
│               ├── QdrantModels.g.cs  # Generated (do not edit)
│               └── QdrantExtensions.cs # Manual extensions
└── build/
    └── nuke/
        └── Build.cs
```

### Git Configuration

**Add to `.gitignore`**:

```gitignore
# Generated code (regenerate via build)
**/*.g.cs
```

**Alternative: Check in generated code** (recommended for transparency):

```gitignore
# Do NOT ignore generated code
# Allow inspection of generated output
```

---

## Migration Strategy

### Phase 1: Parallel Implementation (No Breaking Changes)

**Timeline**: 1-2 weeks

1. **Add Mapperly to select projects**
   - Start with `LablabBean.AI.Actors` (avatar mapping)
   - Create mappers alongside existing manual code
   - Use new mappers in new features only

2. **Set up quicktype in NUKE build**
   - Create `schemas/` directory
   - Add Qdrant schema/sample
   - Run generation manually first
   - Integrate into build pipeline

3. **Validate generated code**
   - Review generated models
   - Add unit tests for mappers
   - Compare output with manual code

### Phase 2: Gradual Replacement

**Timeline**: 2-4 weeks

1. **Replace manual mapping (low-risk areas first)**
   - Analytics transformations
   - Report generation
   - Display model mapping

2. **Replace manual JSON parsing**
   - Qdrant integration
   - Firebase responses
   - Any other external APIs

3. **Update documentation**
   - Add usage examples to `docs/guides/`
   - Update CONTRIBUTING.md with mapper guidelines

### Phase 3: Full Adoption

**Timeline**: 1-2 weeks

1. **Migrate all remaining manual mappings**
2. **Remove deprecated manual mapping code**
3. **Add pre-commit hooks** (optional: enforce mapper usage)
4. **Team training/knowledge sharing**

### Coordination with SPEC-022

**Current Status**: Another agent is implementing SPEC-022

**Recommended Approach**:

1. ✅ **Create this documentation first** (current step)
2. ⏸️ **Wait for SPEC-022 completion**
3. ▶️ **Begin Phase 1 integration** (no conflicts with SPEC-022)
4. 🔄 **Coordinate with other agent** if SPEC-022 adds new mapping code

---

## Usage Examples

### Example 1: Replace Avatar Serialization

**Before** (manual):

```csharp
// dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs

public class AvatarStatePersistence
{
    public AvatarStateSnapshot CreateSnapshot(AvatarState state, AvatarMemory memory)
    {
        return new AvatarStateSnapshot
        {
            EntityId = state.EntityId,
            StateJson = AvatarStateSerializer.Serialize(state),
            MemoryJson = JsonSerializer.Serialize(memory),
            SnapshotTime = DateTime.UtcNow,
            Version = state.Version
        };
    }
}
```

**After** (with Mapperly):

```csharp
// dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStatePersistence.cs

public class AvatarStatePersistence
{
    private readonly AvatarMapper _mapper = new();

    public AvatarStateSnapshot CreateSnapshot(AvatarState state, AvatarMemory memory)
    {
        // Mapperly generates this method at compile time
        return _mapper.ToSnapshot(state, memory);
    }
}
```

### Example 2: Replace Qdrant JSON Parsing

**Before** (manual):

```csharp
// dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs

public async Task<IEnumerable<VectorSearchResult>> SearchAsync(
    string collectionName,
    float[] queryVector,
    int topK,
    CancellationToken ct)
{
    var response = await _httpClient.PostAsJsonAsync(
        $"/collections/{collectionName}/points/search",
        new { vector = queryVector, top = topK },
        ct);

    response.EnsureSuccessStatusCode();

    using var stream = await response.Content.ReadAsStreamAsync(ct);
    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

    var root = doc.RootElement;
    if (!root.TryGetProperty("result", out var result))
        return Array.Empty<VectorSearchResult>();

    var results = new List<VectorSearchResult>();
    foreach (var item in result.EnumerateArray())
    {
        results.Add(new VectorSearchResult
        {
            Id = item.GetProperty("id").GetString() ?? string.Empty,
            Score = item.GetProperty("score").GetDouble(),
            Payload = item.TryGetProperty("payload", out var payload)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText())
                : null
        });
    }

    return results;
}
```

**After** (with quicktype):

```csharp
// dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs

using LablabBean.Plugins.VectorStore.Qdrant.Models; // Generated by quicktype

public async Task<IEnumerable<VectorSearchResult>> SearchAsync(
    string collectionName,
    float[] queryVector,
    int topK,
    CancellationToken ct)
{
    var response = await _httpClient.PostAsJsonAsync(
        $"/collections/{collectionName}/points/search",
        new { vector = queryVector, top = topK },
        ct);

    response.EnsureSuccessStatusCode();

    // Strongly-typed deserialization with IntelliSense
    var qdrantResponse = await response.Content
        .ReadFromJsonAsync<SearchResponse>(cancellationToken: ct)
        ?? throw new InvalidOperationException("Invalid response from Qdrant");

    // Type-safe mapping with compile-time validation
    return qdrantResponse.Result.Select(r => new VectorSearchResult
    {
        Id = r.Id,           // IntelliSense works!
        Score = r.Score,     // Compiler validates types
        Payload = r.Payload  // Null-safe access
    });
}
```

### Example 3: Complex Mapping with Custom Logic

```csharp
[Mapper]
public partial class GameStateMapper
{
    public partial GameStateDto ToDto(GameState state);

    // Custom mapping for computed properties
    [MapProperty(nameof(GameState.Entities), nameof(GameStateDto.EntityCount),
                 Use = nameof(CountEntities))]
    private int CountEntities(IEnumerable<GameEntity> entities)
        => entities.Count();

    // Conditional mapping
    [MapProperty(nameof(GameState.CurrentLevel), nameof(GameStateDto.LevelDisplay),
                 Use = nameof(FormatLevel))]
    private string FormatLevel(int level)
        => level > 0 ? $"Level {level}" : "Main Menu";
}
```

---

## Best Practices

### Mapperly Guidelines

1. **One mapper per domain aggregate**
   - ✅ `AvatarMapper` for all avatar-related mappings
   - ❌ Not `AvatarToSnapshotMapper`, `AvatarToDtoMapper`, etc.

2. **Place mappers in `Mapping/` directories**

   ```
   LablabBean.AI.Actors/
   ├── Models/
   ├── Mapping/
   │   └── AvatarMapper.cs
   └── Services/
   ```

3. **Use XML doc comments**

   ```csharp
   /// <summary>
   /// Maps <see cref="AvatarState"/> to persistent snapshot format.
   /// Generated at compile time by Mapperly.
   /// </summary>
   public partial AvatarStateSnapshot ToSnapshot(AvatarState state);
   ```

4. **Keep custom logic minimal**
   - ✅ Simple conversions (date formatting, string concatenation)
   - ❌ Complex business logic (belongs in services)

5. **Test generated mappers**

   ```csharp
   [Fact]
   public void ToSnapshot_MapsAllRequiredProperties()
   {
       var state = CreateTestAvatarState();
       var mapper = new AvatarMapper();

       var snapshot = mapper.ToSnapshot(state);

       snapshot.EntityId.Should().Be(state.EntityId);
       snapshot.Version.Should().Be(state.Version);
       // ... validate all critical properties
   }
   ```

### quicktype Guidelines

1. **Prefer JSON Schema over samples** (more precise)
   - ✅ Explicit nullability, required fields, types
   - ⚠️ Samples might miss edge cases

2. **Use semantic versioning for schemas**

   ```
   schemas/
   ├── qdrant-api-v1.json
   └── qdrant-api-v2.json
   ```

3. **Add schema documentation**

   ```json
   {
     "$schema": "http://json-schema.org/draft-07/schema#",
     "title": "Qdrant Search API v1",
     "description": "Response format for Qdrant vector search endpoint",
     "definitions": { ... }
   }
   ```

4. **Don't edit generated files**
   - ❌ Never modify `*.g.cs` files manually
   - ✅ Use partial classes for extensions

5. **Create extension methods for convenience**

   ```csharp
   // QdrantModels.g.cs (generated)
   public partial class SearchResult { ... }

   // QdrantExtensions.cs (manual)
   public static class SearchResultExtensions
   {
       public static VectorSearchResult ToVectorSearchResult(this SearchResult result)
       {
           return new VectorSearchResult
           {
               Id = result.Id,
               Score = result.Score,
               Payload = result.Payload
           };
       }
   }
   ```

### General Guidelines

1. **Run code generation before compile**
   - NUKE build already handles this: `GenerateApiTypes` → `Compile`

2. **Check generated code into source control** (recommended)
   - Transparency: Reviewers see generated code changes
   - Reproducibility: No build-time dependencies on quicktype
   - Alternative: Add to CI/CD pipeline, ignore locally

3. **Version control schema changes**
   - Track JSON schema evolution in git
   - Use `git diff` to see API changes over time

4. **Document deviations**
   - If generated code needs customization, document why
   - Consider filing issues with quicktype/Mapperly

---

## References

### Official Documentation

- **Mapperly**: <https://mapperly.riok.app/>
- **quicktype**: <https://quicktype.io/>
- **NUKE Build**: <https://nuke.build/>

### Related Project Documentation

- [Project Architecture](../ARCHITECTURE.md) - DOC-2025-00018
- [Project Setup Summary](project-setup.md) - DOC-2025-00029
- [NUKE Build Configuration](..\..\build\nuke\Build.cs)

### Package References

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Riok.Mapperly" Version="3.6.0" />
```

### CLI Commands

```bash
# Install quicktype globally
npm install -g quicktype

# Generate C# models from JSON schema
quicktype schemas/api.json --lang csharp --out Models.g.cs

# Run NUKE build with code generation
nuke GenerateApiTypes

# Compile with generated types
nuke Compile
```

---

## Next Steps

1. **Review and approve this document**
2. **Wait for SPEC-022 completion** (avoid merge conflicts)
3. **Create `schemas/` directory** with Qdrant schema
4. **Phase 1: Add Mapperly to one project** (e.g., LablabBean.AI.Actors)
5. **Phase 1: Set up quicktype in NUKE build**
6. **Validate generated code** with unit tests
7. **Phase 2: Gradual migration** of existing code
8. **Phase 3: Full adoption** across all projects

---

**Document Status**: Draft - Awaiting review and SPEC-022 completion

**Implementation Timeline**: 4-7 weeks total (after SPEC-022)

**Risk Level**: Low (parallel implementation, no breaking changes)

**Dependencies**:

- Node.js (for quicktype)
- NUKE build system (already in place)
- SPEC-022 completion

**Contact**: See CONTRIBUTING.md for questions or clarifications
