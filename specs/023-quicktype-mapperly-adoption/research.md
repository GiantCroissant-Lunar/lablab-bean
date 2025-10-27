# Research Findings: quicktype and Mapperly Production Adoption

**Feature**: 023-quicktype-mapperly-adoption
**Date**: 2025-10-27
**Status**: Complete
**Related**: [plan.md](./plan.md) | [spec.md](./spec.md)

## Executive Summary

This document consolidates research findings for five critical technical decisions required to migrate from manual JSON parsing and object mapping to automated code generation using quicktype and Mapperly. All research questions have been answered with concrete implementation guidance.

**Key Decisions**:

1. ✅ **API Contract Source**: Use Qdrant's official OpenAPI specification
2. ✅ **JSON Serialization**: Use System.Text.Json (matches 93% of codebase)
3. ✅ **Avatar Mapping**: Keep current implementation - Mapperly not suitable for this case
4. ✅ **Build Integration**: Enhance existing NUKE target with error handling
5. ✅ **Nullable Handling**: Use nullable reference types for optional properties

---

## Research Question 1: Qdrant API Contract Capture

### Question

How do we obtain accurate JSON schemas for Qdrant's search API (request/response structures)?

### Options Evaluated

- **Option A**: Use Qdrant's OpenAPI/Swagger specification
- **Option B**: Capture live API responses and infer schema with quicktype
- **Option C**: Manually write JSON Schema based on API documentation

### Current State Analysis

**Existing API Integration** (`dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs`):

- **Endpoints Used**:
  - `PUT /collections/{collection}` - Create/ensure collection exists
  - `POST /collections/{collection}/points` - Upsert points
  - `POST /collections/{collection}/points/search` - Search for similar vectors (PRIMARY)
  - `POST /collections/{collection}/points/delete` - Delete points by ID

**Search Request Structure** (lines 55-60):

```json
{
  "vector": [0.1, 0.2, ...],
  "limit": 10,
  "with_payload": true
}
```

**Search Response Structure** (manually parsed at lines 64-94):

```json
{
  "result": [
    {
      "id": "string|number",
      "score": 0.81,
      "payload": { "text": "...", "metadata": {...} }
    }
  ],
  "status": "ok",
  "time": 0.001
}
```

**Existing Schema Files**:

- ✅ `schemas/qdrant/point-response.json` - Draft JSON Schema (not actively used)
- ✅ `schemas/qdrant/point-response-sample.json` - Sample JSON (CURRENTLY USED)
- ✅ Generated: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/QdrantPointResponse.g.cs`
- ❌ **Issue**: Sample is too specific (hardcoded Guid type, specific payload structure)

### Decision: **Option A - Use Qdrant's OpenAPI Specification**

### Rationale

1. **OpenAPI Specification Available**: Qdrant maintains official OpenAPI spec at:
   - Primary source: `https://github.com/qdrant/qdrant/blob/master/docs/redoc/master/openapi.json`
   - Interactive docs: `https://api.qdrant.tech/api-reference`
   - Swagger UI: `https://ui.qdrant.tech/`

2. **Accuracy & Completeness**:
   - Official specs include all request/response structures
   - Documents optional parameters and edge cases
   - Maintained by Qdrant team, synchronized with API changes
   - Includes proper union types (`id` as string|integer)

3. **Build Pipeline Compatibility**:
   - quicktype natively supports OpenAPI/JSON Schema input (`--src-lang schema`)
   - Current build already uses quicktype via `GenerateApiTypes` target
   - Minimal changes to existing workflow

4. **Type Safety**:
   - OpenAPI schemas define exact types, nullability, and constraints
   - Better than manual JSON samples which may miss edge cases
   - Current generated code has issues (hardcoded Guid instead of flexible id type)

5. **Maintenance**:
   - No need to capture/update samples manually
   - Can periodically update from upstream OpenAPI spec
   - Clear versioning strategy (spec is versioned with Qdrant releases)

### Alternatives Considered

- **Option B (Live Capture)**:
  - ❌ Only captures what you test
  - ❌ May miss optional fields or edge cases
  - ❌ Requires running Qdrant instance and writing test scenarios
  - ❌ Manual maintenance burden

- **Option C (Manual JSON Schema)**:
  - ❌ Error-prone and time-consuming
  - ❌ May drift from actual API
  - ❌ Already exists as Option A (official spec)

### Implementation Guidance

#### Step 1: Download Qdrant OpenAPI Specification

```bash
# Download the official OpenAPI spec
curl -o schemas/qdrant/openapi.json https://raw.githubusercontent.com/qdrant/qdrant/master/docs/redoc/master/openapi.json
```

**Version pinning recommendation**: Use specific release tag (e.g., `v1.7.0`) instead of `master` for stability.

#### Step 2: Extract Relevant Schemas

The OpenAPI spec is comprehensive (all endpoints). Extract only needed schemas:

**Required Schemas**:

1. **SearchRequest** - Request body for `POST /collections/{collection_name}/points/search`
2. **SearchResponse** - Response wrapper containing result array
3. **ScoredPoint** - Individual search result structure
4. **Record** - Point with full payload

**Extraction Method**:

**Option 2a**: Use quicktype with OpenAPI directly

```bash
npx quicktype \
  --src schemas/qdrant/openapi.json \
  --src-lang schema \
  --lang csharp \
  --namespace LablabBean.Framework.Generated.ExternalApis.Qdrant \
  --top-level SearchRequest,SearchResponse \
  --out dotnet/framework/LablabBean.Framework.Generated/ExternalApis/QdrantSearch.g.cs
```

**Option 2b**: Extract specific schemas to separate JSON Schema files (RECOMMENDED)

- Extract `#/components/schemas/SearchRequest` → `schemas/qdrant/search-request.schema.json`
- Extract `#/components/schemas/SearchResponse` → `schemas/qdrant/search-response.schema.json`
- Use existing build pipeline with `--src-lang schema` flag

#### Step 3: Update Build.cs GenerateApiTypes Target

Modify `build/nuke/Build.cs` to handle both sample JSON and JSON Schema files:

```csharp
// Add schema file detection (line ~103)
var schemaFiles = SchemasDirectory.GlobFiles("**/*.schema.json");
var sampleFiles = SchemasDirectory.GlobFiles("**/*-sample.json");

// Process schema files with --src-lang schema
foreach (var schemaFile in schemaFiles) {
    var typeName = KebabToPascalCase(
        Path.GetFileNameWithoutExtension(schemaFile).Replace(".schema", "")
    );
    var outputFile = GeneratedDirectory / "ExternalApis" / $"{typeName}.g.cs";

    ProcessTasks.StartProcess(
        "npx",
        $"quicktype --src \"{schemaFile}\" --src-lang schema --lang csharp " +
        $"--namespace LablabBean.Framework.Generated.ExternalApis " +
        $"--array-type list --csharp-version 8 --framework SystemTextJson " +
        $"--check-required --top-level {typeName} --out \"{outputFile}\"",
        RootDirectory,
        timeout: TimeSpan.FromMinutes(2)
    ).AssertZeroExitCode();
}
```

#### Step 4: Replace Manual Parsing in QdrantVectorStore.cs

Once generated types are available:

```csharp
using LablabBean.Framework.Generated.ExternalApis;

// In SearchAsync method:
using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
var searchResponse = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(
    stream,
    cancellationToken: ct
).ConfigureAwait(false);

return searchResponse.Result.Select(point => new VectorSearchResult
{
    Id = point.Id.ToString(),  // Handle union type properly
    Score = point.Score,
    Payload = point.Payload ?? new Dictionary<string, string>()
}).ToList();
```

#### Step 5: Validation

1. Run build: `nuke GenerateApiTypes`
2. Verify generated code in `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/`
3. Check union types: Ensure `id` field supports both string and number
4. Test parsing: Run integration tests against actual Qdrant instance
5. Compare behavior: Ensure new typed parsing matches existing manual parsing

#### Step 6: Documentation

Update `schemas/README.md` to document:

- How to update from upstream OpenAPI spec
- Which endpoints are currently integrated
- Version tracking (which Qdrant version the spec comes from)

---

## Research Question 2: JSON Serialization Choice

### Question

Should we use System.Text.Json or Newtonsoft.Json for deserialization with quicktype-generated models?

### Options Evaluated

- **System.Text.Json**: Native .NET, better performance, already in use
- **Newtonsoft.Json**: More features, flexible, already in dependencies

### Current State Analysis

**Qdrant Plugin** (`dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs`):

- Uses `System.Text.Json` (line 5)
- Uses `System.Net.Http.Json` extensions (line 4)
- Uses `JsonDocument.ParseAsync()` for manual parsing (line 64)
- Uses `HttpClient.PostAsJsonAsync()` and `ReadFromJsonAsync()` helpers

**AI.Actors Persistence** (`dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs`):

- Uses `System.Text.Json` (line 1)
- Uses `JsonSerializer.Serialize()` and `JsonSerializer.Deserialize()`
- Custom `JsonSerializerOptions` with `WriteIndented: true` and `PropertyNameCaseInsensitive: true`

**Codebase Statistics**:

- **30 files** use System.Text.Json (93% of JSON-related code)
- **2 files** use Newtonsoft.Json (only generated example + one legacy file)
- **10 projects** reference System.Text.Json in .csproj
- **1 project** (Framework.Generated) references Newtonsoft.Json

**quicktype Default**: Generates Newtonsoft.Json code by default

- Current generated file uses `using Newtonsoft.Json` (line 15)
- Uses `[JsonProperty]` attributes (Newtonsoft convention)
- Helper methods use `JsonConvert.DeserializeObject()` (Newtonsoft API)
- NUKE build does NOT specify `--framework`, so defaults to Newtonsoft

### Decision: **System.Text.Json**

### Rationale

1. **Consistency with Existing Code**: 93% of the codebase uses System.Text.Json. Introducing Newtonsoft.Json would create inconsistency and cognitive overhead for developers.

2. **Performance**: System.Text.Json is significantly faster than Newtonsoft.Json:
   - Project includes a `SerializationBenchmark` demonstrating STJ performance
   - Microsoft's official guidance recommends STJ for .NET 5+ applications
   - Critical for Qdrant plugin handling high-frequency vector search operations

3. **Native .NET Integration**: System.Text.Json is built into .NET 8 runtime with zero external dependencies. Qdrant plugin already uses `System.Net.Http.Json` extensions designed for System.Text.Json.

4. **Modern .NET Standards**: System.Text.Json is the official Microsoft-recommended serializer for .NET Core 3.0+ and is actively maintained as part of the .NET runtime.

5. **Existing Patterns Match**: Current Qdrant plugin and AI.Actors code already use System.Text.Json patterns (nullable annotations, case-insensitive property matching, indented output).

### Alternatives Considered

**Newtonsoft.Json** - Rejected for:

- **Inconsistency**: Would introduce second JSON serialization library to standardized codebase
- **Performance Cost**: Slower than System.Text.Json for high-frequency operations
- **External Dependency**: Requires NuGet package for capability built into .NET runtime
- **Migration Path**: Would create future technical debt
- **Quick Fix Trap**: Only reason would be because quicktype defaults to it

### Implementation Guidance

#### Configure quicktype for System.Text.Json

Update `build/nuke/Build.cs` line 125:

**Before** (defaults to Newtonsoft.Json):

```bash
quicktype --src "{schemaFile}" --src-lang json --lang csharp
  --namespace LablabBean.Framework.Generated.ExternalApis
  --array-type list
  --features complete
  --top-level {typeName}
  --out "{outputFile}"
```

**After** (explicit System.Text.Json):

```bash
quicktype --src "{schemaFile}" --src-lang json --lang csharp
  --framework SystemTextJson
  --namespace LablabBean.Framework.Generated.ExternalApis
  --array-type list
  --csharp-version 8
  --check-required
  --top-level {typeName}
  --out "{outputFile}"
```

**Key changes**:

- Add `--framework SystemTextJson` flag
- Add `--csharp-version 8` for nullable reference types
- Add `--check-required` for proper nullable handling
- Remove `--features complete` (too aggressive)

#### Remove Newtonsoft.Json Dependency

Update `dotnet/framework/LablabBean.Framework.Generated/LablabBean.Framework.Generated.csproj`:

```xml
<!-- BEFORE -->
<ItemGroup>
  <PackageReference Include="Riok.Mapperly" />
  <PackageReference Include="System.Text.Json" />
  <PackageReference Include="Newtonsoft.Json" />  <!-- REMOVE -->
</ItemGroup>

<!-- AFTER -->
<ItemGroup>
  <PackageReference Include="Riok.Mapperly" />
  <PackageReference Include="System.Text.Json" />
</ItemGroup>
```

#### Regenerate Generated Files

Delete current Newtonsoft-based example:

```bash
Remove-Item "dotnet/framework/LablabBean.Framework.Generated/ExternalApis/QdrantPointResponse.g.cs"
```

Regenerate with updated build command to create System.Text.Json-based models.

#### Verify Generated Code

Generated code should use:

- `using System.Text.Json.Serialization;` (NOT `using Newtonsoft.Json;`)
- `[JsonPropertyName("property")]` attributes (NOT `[JsonProperty("property")]`)
- Compatible with `JsonSerializer.Deserialize<T>()` and `JsonSerializer.Serialize<T>()`

---

## Research Question 3: Mapperly Configuration for Avatar State Mapping

### Question

How do we handle edge cases in avatar state mapping with Mapperly?

### Current State Analysis

**Source Type: AvatarState** (`dotnet/framework/LablabBean.AI.Core/Models/AvatarState.cs`):

- `string EntityId` - Simple property
- `float Health`, `float MaxHealth` - Primitives
- `string CurrentBehavior`, `string EmotionalState` - Strings with defaults
- `Dictionary<string, float> Stats` - **EDGE CASE**: Dictionary collection
- `List<string> ActiveEffects` - **EDGE CASE**: List collection
- `DateTime LastUpdated` - DateTime with default
- `float HealthPercentage` (read-only computed) - **EDGE CASE**: Computed property
- `bool IsAlive` (read-only computed) - **EDGE CASE**: Computed property

**Target Type: AvatarStateSnapshot** (`dotnet/framework/LablabBean.AI.Actors/Persistence/AvatarStateSerializer.cs`):

- `string EntityId` - Maps from AvatarState.EntityId
- `string StateJson` - **EDGE CASE**: Requires serialization of entire AvatarState
- `string MemoryJson` - **EDGE CASE**: Requires serialization of separate AvatarMemory object
- `DateTime SnapshotTime` - **EDGE CASE**: Generated value, not from source

**Related Type: AvatarMemory** (`dotnet/framework/LablabBean.AI.Core/Models/AvatarMemory.cs`):

- `List<MemoryEntry> ShortTermMemory` - List of complex objects
- `List<MemoryEntry> LongTermMemory` - List of complex objects
- `Dictionary<string, int> InteractionCounts` - Dictionary collection

**MemoryEntry Structure**:

- `Dictionary<string, object> Metadata` - **CRITICAL EDGE CASE**: Dictionary with object values

### Edge Cases Identified

1. **Computed/Read-Only Properties**: Cannot be mapped (no setters)
2. **Dictionary Collections**: Need deep cloning decision
3. **Dictionary<string, object>**: Mapperly has limited support (issue #1309)
4. **Nested Complex Objects in Collections**: Deep cloning affects nested structures
5. **JSON Serialization Mapping**: Object-to-JSON-string conversion
6. **Generated/Timestamp Properties**: Value generated at creation time
7. **Multiple Source Objects**: AvatarStateSnapshot needs both AvatarState AND AvatarMemory

### Decision: **Keep Current Implementation - Do NOT Use Mapperly for Avatar State**

### Rationale

The existing `AvatarStateSerializer` and `AvatarStateSnapshot.Create()` method should **NOT** be replaced with Mapperly because:

1. **Serialization is not mapping**: Converting objects to JSON strings is a serialization concern, not property mapping
2. **Dictionary<string, object>**: Mapperly cannot handle this reliably (tracked in issue #1309)
3. **Multi-source mapping**: Requires both AvatarState and AvatarMemory
4. **Generated values**: SnapshotTime is timestamp, not mapped data
5. **Current implementation is optimal**: Uses appropriate patterns for the use case

### Current Code is Correct - Keep It

```csharp
// ✅ KEEP THIS - DO NOT REPLACE WITH MAPPERLY
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

### Mapperly Features Documented (For Other Use Cases)

When Mapperly IS appropriate (simple object-to-object mapping):

#### 1. **MapperIgnoreTarget Attribute**

```csharp
[MapperIgnoreTarget(nameof(AvatarState.HealthPercentage))]
[MapperIgnoreTarget(nameof(AvatarState.IsAlive))]
```

Use for computed read-only properties that have no setters.

#### 2. **UseDeepCloning Configuration**

```csharp
[Mapper(UseDeepCloning = true)]
```

Controls whether collections and nested objects are deep copied or referenced.

#### 3. **Custom Mapping Methods**

```csharp
private string SerializeToJson(AvatarState state)
{
    return JsonSerializer.Serialize(state);
}
```

Mapperly auto-detects and uses custom methods with matching signatures.

### When to USE Mapperly

**Appropriate scenarios**:

- Simple DTO-to-Domain mapping when shapes align
- Cloning objects with simple properties
- Mapping between similar object structures
- Collections of primitive or simple types

**Configuration example**:

```csharp
[Mapper(UseDeepCloning = true)]
public partial class SimpleMapper
{
    [MapperIgnoreTarget(nameof(Entity.ComputedField))]
    public partial DomainEntity FromDto(EntityDto dto);
}
```

### When NOT to use Mapperly

**Inappropriate scenarios** (like Avatar State):

- Object-to-JSON-string serialization
- `Dictionary<string, object>` (not supported)
- Multi-source mappings (requires orchestration)
- Generated/computed values (not from source)
- Complex business logic in transformation

### Alternatives Considered

**Option: Use Mapperly with workarounds** - Rejected because:

- Workarounds would be more complex than current simple code
- Dictionary<string, object> has no reliable Mapperly solution
- Multi-source mapping requires manual orchestration anyway
- Current factory method pattern is clearer and more maintainable

### Implementation Guidance

**For Avatar State**: NO CHANGES NEEDED. Current implementation is correct.

**For Future Simple Mappings**: Use Mapperly when appropriate:

1. Identify DTO-to-Domain mapping needs
2. Verify no edge cases (Dictionary<string, object>, multi-source, serialization)
3. Create partial interface with `[Mapper]` attribute
4. Configure `UseDeepCloning = true` for snapshot scenarios
5. Use `[MapperIgnoreTarget]` for computed properties

---

## Research Question 4: Build Integration Details

### Question

How does the existing NUKE GenerateApiTypes target work and what modifications are needed?

### Current Implementation Analysis

**Location**: `build/nuke/Build.cs` (lines 95-133)

**What it currently does**:

1. Checks if `schemas/` directory exists
2. Creates output directory `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/`
3. Scans for `schemas/**/*-sample.json` files
4. For each file:
   - Converts kebab-case filename to PascalCase type name
   - Removes `-sample` suffix
   - Invokes `npx quicktype` with specific options
5. Logs processing count

**Build Integration**:

- Runs **before** `Restore` target (line 142)
- Build order: `GenerateApiTypes` → `Restore` → `Compile` → `Test`
- Already integrated into build pipeline!
- Invoked automatically when building

**quicktype Command**:

```bash
npx quicktype --src "{schemaFile}" --src-lang json --lang csharp
  --namespace LablabBean.Framework.Generated.ExternalApis
  --array-type list
  --features complete
  --top-level {typeName}
  --out "{outputFile}"
```

### Issues Identified

1. **Example/Demo Code Present**: Test files meant for demonstration remain in production project
2. **No Error Handling**: If one schema fails, entire target fails
3. **No Incremental Generation**: Regenerates all files every time
4. **No Schema Validation**: No pre-validation that JSON is well-formed
5. **Hardcoded Namespace**: Cannot organize into sub-namespaces
6. **No Cleanup of Stale Files**: Deleted schemas leave orphaned generated files
7. **Limited Logging**: Minimal success/failure reporting
8. **No Configuration Options**: quicktype options are hardcoded

### Decision: **Minimal Critical Fixes + Important Enhancements**

### Critical Changes (Must-Have)

1. **Remove example/demo code** from LablabBean.Framework.Generated:
   - Delete `Tests/QuicktypeGenerationExamples.cs`
   - Delete `Tests/MapperlyExamples.cs`
   - Delete `Mappers/ExampleMapper.cs`

2. **Add per-file error handling**:

   ```csharp
   foreach (var schemaFile in schemaFiles)
   {
       try
       {
           // ... generation logic
           successCount++;
       }
       catch (Exception ex)
       {
           Serilog.Log.Error("✗ Failed: {Schema}: {Error}", schemaFile, ex.Message);
           failureCount++;
       }
   }
   ```

3. **Enhanced logging**:
   - Log success/failure per file
   - Report summary at end
   - Show output location

### Important Changes (Should-Have)

4. **Clean up stale generated files**:

   ```csharp
   if (Directory.Exists(outputDir))
   {
       var existingFiles = outputDir.GlobFiles("*.g.cs");
       foreach (var file in existingFiles) File.Delete(file);
   }
   ```

5. **Add JSON validation**:

   ```csharp
   static bool IsValidJson(string filePath)
   {
       try
       {
           JsonDocument.Parse(File.ReadAllText(filePath));
           return true;
       }
       catch { return false; }
   }
   ```

6. **Better error messages**: Show which schema failed and why

### Rationale

**Why minimal changes?**

- Current implementation already works successfully
- Already integrated into build pipeline
- Good patterns in place (glob, directory creation, graceful degradation)
- Generated `QdrantPointResponse.g.cs` proves correct output

**Why these specific changes?**

- **Remove examples**: Plan explicitly states this (Phase 5: "Remove example/demo code")
- **Per-file error handling**: Production resilience - one bad schema shouldn't break all generation
- **Better logging**: Essential for debugging production issues
- **Stale file cleanup**: Prevents confusion from orphaned generated code
- **JSON validation**: Catch errors early with clear messages

**Why defer optional features?**

- Incremental generation: Premature optimization (currently only 1 schema)
- Configurable namespaces: Add when multiple APIs integrated
- Build configuration: Add when patterns emerge

### Implementation Guidance

#### Files to Delete

```
dotnet/framework/LablabBean.Framework.Generated/
├── Tests/QuicktypeGenerationExamples.cs  ❌ DELETE
├── Tests/MapperlyExamples.cs             ❌ DELETE
└── Mappers/ExampleMapper.cs              ❌ DELETE
```

#### Code Changes to Build.cs

See detailed implementation in plan.md Phase 0 section for complete code examples including:

- Try-catch wrapper for each schema
- Success/failure counters
- JSON validation helper
- Stale file cleanup logic
- Enhanced logging statements

---

## Research Question 5: Nullable Property Handling

### Question

How should generated code handle missing/null JSON properties to match existing behavior?

### Current State Analysis

**Nullable Reference Types**: Enabled project-wide

- `dotnet/Directory.Build.props`: `<Nullable>enable</Nullable>`
- All projects inherit this setting
- Nullable warnings treated as errors: `<WarningsAsErrors>nullable</WarningsAsErrors>`

**Current Qdrant Parsing** (`QdrantVectorStore.cs` lines 66-92):

```csharp
// Defensive parsing with null-coalescing
if (!root.TryGetProperty("result", out var result))
    return Array.Empty<VectorSearchResult>();

string idStr = item.TryGetProperty("id", out var idEl)
    ? idEl.ValueKind switch { /* ... */ }
    : Guid.NewGuid().ToString();

float score = item.TryGetProperty("score", out var scoreEl)
    ? scoreEl.TryGetSingle(out var s) ? s : 0f
    : 0f;
```

**Existing Generated Code** (`QdrantPointResponse.g.cs`):

```csharp
[JsonProperty("id")]
public Guid Id { get; set; }  // ❌ NOT NULLABLE

[JsonProperty("payload")]
public Payload Payload { get; set; }  // ❌ NOT NULLABLE
```

**Issues**:

- No nullable annotations despite nullable warnings being errors
- Uses Newtonsoft.Json instead of System.Text.Json
- Doesn't match schema (only `id` is required)
- Will cause compiler warnings

**Qdrant API Contract** (`schemas/qdrant/point-response.json`):

```json
{
  "required": ["id"],
  "properties": {
    "id": { "oneOf": [{"type": "string"}, {"type": "integer"}] },
    "payload": { "type": "object" },  // OPTIONAL
    "vector": { "oneOf": [...] }      // OPTIONAL
  }
}
```

**API Behavior**:

- `id`: Always present (required)
- `payload`: Optional (can be omitted with `with_payload=false`)
- `vector`: Optional (can be omitted with `with_vectors=false`)

### Decision: **Use Nullable Reference Types for Optional Fields**

Generated code should:

1. Mark optional properties as nullable (`string?`, `List<T>?`, `Payload?`)
2. Required properties remain non-nullable (e.g., `id`)
3. Use System.Text.Json (`JsonPropertyName`)
4. Enable nullable reference types
5. Add `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` for optional properties

**Recommended Generated Code**:

```csharp
using System.Text.Json.Serialization;

public partial class QdrantPointResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;  // Required, never null

    [JsonPropertyName("payload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Payload? Payload { get; set; }  // Optional, can be null

    [JsonPropertyName("vector")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double>? Vector { get; set; }  // Optional, can be null
}
```

### Rationale

1. **Consistency with Codebase Standards**: Nullable reference types enforced project-wide
2. **API Contract Accuracy**: Reflects that payload and vector can be omitted
3. **Alignment with Existing Patterns**: Manual parsing already handles missing properties
4. **Type Safety**: Forces consumers to check for null before accessing
5. **System.Text.Json Migration**: Consistent with 93% of codebase

### Alternatives Considered

**Non-nullable with default initializers** - Rejected because:

- Doesn't accurately represent API contract
- Can hide bugs when properties are unexpectedly missing
- Not semantically correct (optional ≠ has default value)

**Newtonsoft.Json** - Rejected because:

- Inconsistent with 93% of codebase using System.Text.Json
- Performance cost for high-frequency operations
- Creates unnecessary external dependency

### Implementation Guidance

#### quicktype Flags

Update `build/nuke/Build.cs`:

```bash
quicktype --src "{schemaFile}" --src-lang json --lang csharp
  --namespace LablabBean.Framework.Generated.ExternalApis
  --array-type list
  --csharp-version 8            # ← Enable nullable reference types
  --framework SystemTextJson     # ← Use System.Text.Json
  --check-required              # ← Generate nullable for optional properties
  --top-level {typeName}
  --out "{outputFile}"
```

**Flag Explanations**:

- `--csharp-version 8`: Enables nullable reference types (C# 8+)
- `--framework SystemTextJson`: Use `JsonPropertyName` instead of `JsonProperty`
- `--check-required`: Generates nullable types for optional properties based on schema `required` array

#### JSON Schema Updates

Ensure schemas explicitly mark required fields:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "required": ["id"],  // ✅ Only id is required
  "properties": {
    "id": { "oneOf": [{"type": "string"}, {"type": "integer"}] },
    "payload": { "type": ["object", "null"] },  // ✅ Explicitly nullable
    "vector": { "oneOf": [
      {"type": "array", "items": {"type": "number"}},
      {"type": "null"}  // ✅ Explicitly nullable
    ]}
  }
}
```

#### Post-Generation Validation

Verify generated code:

1. **Check nullable annotations**:

   ```bash
   grep -n "public.*? " dotnet/framework/LablabBean.Framework.Generated/ExternalApis/*.g.cs
   ```

2. **Check System.Text.Json attributes**:

   ```bash
   grep -n "JsonPropertyName" dotnet/framework/LablabBean.Framework.Generated/ExternalApis/*.g.cs
   ```

3. **Check compilation**:

   ```bash
   cd dotnet && dotnet build --no-restore
   ```

#### Testing Strategy

Test deserialization with missing properties:

```csharp
[Fact]
public void QdrantPointResponse_DeserializesWithMissingPayload()
{
    var json = """{"id": "123", "vector": [0.1, 0.2]}""";  // No payload
    var response = JsonSerializer.Deserialize<QdrantPointResponse>(json);

    Assert.NotNull(response);
    Assert.Equal("123", response.Id);
    Assert.Null(response.Payload);  // ✅ Should be null, not throw
}
```

---

## Summary of Decisions

| Question | Decision | Rationale |
|----------|----------|-----------|
| **API Contract Source** | Use Qdrant OpenAPI spec | Official, accurate, maintainable |
| **JSON Serialization** | System.Text.Json | 93% codebase consistency, performance, native .NET |
| **Avatar Mapping** | Keep current code (no Mapperly) | Serialization concern, Dictionary<string, object>, multi-source |
| **Build Integration** | Enhance with error handling | Current works, needs production robustness |
| **Nullable Handling** | Use nullable reference types | Matches codebase standards, API accuracy |

## Implementation Priority

### Phase 1: Critical Changes

1. ✅ Update quicktype flags: `--framework SystemTextJson --csharp-version 8 --check-required`
2. ✅ Remove example/demo code from Framework.Generated
3. ✅ Add per-file error handling to Build.cs
4. ✅ Download Qdrant OpenAPI specification

### Phase 2: Important Enhancements

5. ✅ Extract Qdrant schemas from OpenAPI spec
6. ✅ Add JSON validation to build target
7. ✅ Add stale file cleanup
8. ✅ Enhanced logging

### Phase 3: Production Migration

9. ✅ Generate Qdrant models with new configuration
10. ✅ Add Framework.Generated project reference to Qdrant plugin
11. ✅ Replace manual parsing with generated models
12. ✅ Run integration tests

## Next Steps

Continue to Phase 1 of `/speckit.plan`:

1. Generate `data-model.md` with entity definitions
2. Create `contracts/` directory with JSON schemas
3. Generate `quickstart.md` developer guide
4. Update agent context with new technologies

**Research Phase**: ✅ COMPLETE

---

**Document Version**: 1.0
**Last Updated**: 2025-10-27
**Status**: All research questions answered with implementation guidance
