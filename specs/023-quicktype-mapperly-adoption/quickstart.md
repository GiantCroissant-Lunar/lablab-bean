# Quickstart Guide: quicktype and Mapperly

**Feature**: 023-quicktype-mapperly-adoption
**Date**: 2025-10-27
**Audience**: Developers integrating external APIs or adding internal mappers
**Related**: [plan.md](./plan.md) | [data-model.md](./data-model.md) | [research.md](./research.md)

## Overview

This guide helps developers use and extend the code generation infrastructure for external API models (quicktype) and internal object mapping (Mapperly). After completing this feature, the Lablab-Bean project will have automated type-safe code generation instead of manual JSON parsing.

**Quick Links**:

- [Using Generated Qdrant Models](#1-using-generated-qdrant-models)
- [Adding New External APIs](#2-adding-new-external-api-integrations)
- [When NOT to Use Code Generation](#3-when-not-to-use-code-generation)
- [Troubleshooting](#4-troubleshooting)

---

## 1. Using Generated Qdrant Models

### Before: Manual JSON Parsing (REMOVED)

The old approach used manual `JsonDocument` parsing with error-prone string-based property access:

```csharp
using var stream = await resp.Content.ReadAsStreamAsync(ct);
using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
var root = doc.RootElement;

if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Array)
    return Array.Empty<VectorSearchResult>();

var results = new List<VectorSearchResult>();
foreach (var item in result.EnumerateArray())
{
    string idStr = item.TryGetProperty("id", out var idEl)
        ? idEl.ValueKind switch
        {
            JsonValueKind.String => idEl.GetString()!,
            JsonValueKind.Number => idEl.GetRawText(),
            _ => Guid.NewGuid().ToString()
        }
        : Guid.NewGuid().ToString();

    float score = item.TryGetProperty("score", out var scoreEl) && scoreEl.TryGetSingle(out var s)
        ? s : 0f;

    // ... more manual property extraction
}
```

### After: Type-Safe Generated Models (PRODUCTION)

Now using quicktype-generated models with compile-time type safety:

```csharp
using LablabBean.Framework.Generated.Models.Qdrant;

// Create strongly-typed request
var request = new QdrantSearchRequest
{
    Vector = vector.Select(v => (double)v).ToList(),
    Limit = topK,
    WithPayload = true
};

using var resp = await _http.PostAsJsonAsync($"/collections/{collection}/points/search", request, ct);
resp.EnsureSuccessStatusCode();

using var stream = await resp.Content.ReadAsStreamAsync(ct);
var searchResponse = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(stream, cancellationToken: ct);

// Type-safe access to results
return searchResponse.Result.Select(point => new VectorSearchResult
{
    Id = point.Id.String ?? point.Id.Integer?.ToString() ?? Guid.NewGuid().ToString(),
    Score = (float)point.Score,
    Payload = ConvertPayload(point.Payload)
}).ToList();
```

**Benefits**:

- ✅ IntelliSense support for all properties
- ✅ Compile-time errors for property mismatches
- ✅ Handles union types (id as string|integer)
- ✅ Automatic null handling with nullable reference types
- ✅ 40+ lines of manual parsing reduced to 10 lines

```

**Problems**:
- ❌ No compile-time type safety
- ❌ Typos in property names cause runtime failures
- ❌ No IntelliSense support
- ❌ Verbose and error-prone
- ❌ Hard to maintain when API changes

### After: Type-Safe Generated Models

The new approach uses strongly-typed models with automatic serialization:

```csharp
using LablabBean.Framework.Generated.ExternalApis.Qdrant;
using System.Net.Http.Json;

// Deserialize directly to typed model
using var stream = await resp.Content.ReadAsStreamAsync(ct);
var searchResponse = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(
    stream,
    cancellationToken: ct
);

// Access properties with IntelliSense and type safety
var results = searchResponse.Result.Select(point => new VectorSearchResult
{
    Id = point.Id,  // Type-safe: already a string
    Score = point.Score,  // Type-safe: already a float
    Payload = point.Payload?.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.ToString()
    ) ?? new Dictionary<string, string>()
}).ToList();
```

**Benefits**:

- ✅ Compile-time type checking
- ✅ IntelliSense support for all properties
- ✅ Automatic null handling for optional properties
- ✅ Cleaner, more readable code
- ✅ API changes caught at compile time

### Step-by-Step: Update Existing Code

1. **Add Project Reference**:

   ```xml
   <!-- In your plugin's .csproj file -->
   <ItemGroup>
     <ProjectReference Include="..\..\framework\LablabBean.Framework.Generated\LablabBean.Framework.Generated.csproj" />
   </ItemGroup>
   ```

2. **Add Using Statement**:

   ```csharp
   using LablabBean.Framework.Generated.ExternalApis.Qdrant;
   ```

3. **Replace Manual Parsing**:

   ```csharp
   // OLD: Manual JsonDocument parsing
   // using var doc = await JsonDocument.ParseAsync(stream, ...);
   // ... lots of TryGetProperty calls

   // NEW: Strongly-typed deserialization
   var response = await JsonSerializer.DeserializeAsync<QdrantSearchResponse>(
       stream,
       cancellationToken: ct
   );
   ```

4. **Use Typed Properties**:

   ```csharp
   foreach (var point in response.Result)
   {
       Console.WriteLine($"ID: {point.Id}");  // IntelliSense works!
       Console.WriteLine($"Score: {point.Score:F2}");

       if (point.Payload != null)
       {
           foreach (var kvp in point.Payload)
           {
               Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
           }
       }
   }
   ```

---

## 2. Adding New External API Integrations

### Workflow Overview

```mermaid
graph LR
    A[Get API Contract] --> B[Create JSON Schema]
    B --> C[Run Build/Generate]
    C --> D[Add Project Reference]
    D --> E[Use Generated Models]
```

### Step 1: Obtain API Contract

**Option A: OpenAPI/Swagger Specification** (RECOMMENDED)

If the external API provides OpenAPI spec:

```bash
# Download the API specification
curl -o schemas/api-name/openapi.json https://api.example.com/openapi.json

# Or for specific version
curl -o schemas/api-name/openapi.json \
  https://raw.githubusercontent.com/vendor/api/v2.0.0/openapi.json
```

**Option B: Capture Sample Responses**

If no OpenAPI spec is available:

```csharp
// Capture real API response
var response = await httpClient.GetAsync("https://api.example.com/endpoint");
var json = await response.Content.ReadAsStringAsync();
File.WriteAllText("schemas/api-name/endpoint-response-sample.json", json);
```

**Option C: Find Existing Documentation**

Check vendor documentation for JSON Schema or TypeScript definitions.

### Step 2: Create JSON Schema

Place JSON Schema files in `schemas/{api-name}/`:

```
schemas/
└── api-name/
    ├── request.schema.json
    ├── response.schema.json
    └── README.md  (document API version, source, update process)
```

**JSON Schema Template**:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "https://lablab-bean.local/schemas/api-name/entity.json",
  "title": "EntityName",
  "description": "Description of this entity",
  "type": "object",
  "required": ["requiredField1", "requiredField2"],
  "properties": {
    "requiredField1": {
      "description": "Field description",
      "type": "string"
    },
    "optionalField": {
      "description": "Optional field (can be null)",
      "type": ["string", "null"]
    },
    "arrayField": {
      "description": "Array of items",
      "type": "array",
      "items": {"type": "number"}
    }
  },
  "additionalProperties": false
}
```

**Key Points**:

- Mark required fields in `"required"` array
- Use `"type": ["string", "null"]` for nullable fields
- Set `"additionalProperties": false` to catch unexpected properties

### Step 3: Run Code Generation

**Option A: Run NUKE Build** (generates all schemas):

```bash
cd build/nuke
nuke GenerateApiTypes
```

**Option B: Run quicktype Manually** (for single schema):

```bash
npx quicktype \
  --src schemas/api-name/entity.schema.json \
  --src-lang schema \
  --lang csharp \
  --framework SystemTextJson \
  --csharp-version 8 \
  --check-required \
  --namespace LablabBean.Framework.Generated.ExternalApis.ApiName \
  --top-level EntityName \
  --out dotnet/framework/LablabBean.Framework.Generated/ExternalApis/ApiName/EntityName.g.cs
```

**Verify Generation**:

```bash
# Check generated file was created
ls dotnet/framework/LablabBean.Framework.Generated/ExternalApis/

# Check for proper attributes
grep "JsonPropertyName" dotnet/framework/LablabBean.Framework.Generated/ExternalApis/**/*.g.cs

# Check for nullable annotations
grep "public.*? " dotnet/framework/LablabBean.Framework.Generated/ExternalApis/**/*.g.cs
```

### Step 4: Add Project Reference

In your plugin or service `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\framework\LablabBean.Framework.Generated\LablabBean.Framework.Generated.csproj" />
</ItemGroup>
```

### Step 5: Use Generated Models

```csharp
using LablabBean.Framework.Generated.ExternalApis.ApiName;
using System.Text.Json;

// Deserialize API response
var entity = await JsonSerializer.DeserializeAsync<EntityName>(stream);

// Access properties with IntelliSense
Console.WriteLine(entity.RequiredField1);

if (entity.OptionalField != null)
{
    Console.WriteLine(entity.OptionalField);
}
```

### Example: Adding Firebase API Models

```bash
# Step 1: Get Firebase Admin SDK OpenAPI spec (if available)
# Or capture sample responses:
curl -H "Authorization: Bearer $TOKEN" \
  https://identitytoolkit.googleapis.com/v1/accounts:lookup \
  > schemas/firebase/user-lookup-response-sample.json

# Step 2: Create schema (or use sample directly)
cat > schemas/firebase/user-lookup-response.schema.json <<'EOF'
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "FirebaseUserLookupResponse",
  "type": "object",
  "required": ["users"],
  "properties": {
    "users": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "localId": {"type": "string"},
          "email": {"type": ["string", "null"]},
          "displayName": {"type": ["string", "null"]}
        }
      }
    }
  }
}
EOF

# Step 3: Generate
nuke GenerateApiTypes

# Step 4 & 5: Add reference and use
# (See examples above)
```

---

## 3. When NOT to Use Code Generation

### Inappropriate Use Cases

**❌ DO NOT use quicktype for**:

1. **Serialization to JSON strings**:

   ```csharp
   // ❌ Wrong tool for this job
   string stateJson = quicktypeModel.ToJson();  // This is serialization, not parsing

   // ✅ Use System.Text.Json directly
   string stateJson = JsonSerializer.Serialize(state);
   ```

2. **Dictionary<string, object> payloads**:

   ```csharp
   // ❌ quicktype can't handle dynamic object values well
   public Dictionary<string, object> Metadata { get; set; }

   // ✅ Use Dictionary<string, JsonElement> or serialize entire object
   public Dictionary<string, JsonElement> Metadata { get; set; }
   // OR
   string metadataJson = JsonSerializer.Serialize(metadata);
   ```

3. **Internal domain models**:

   ```csharp
   // ❌ Don't generate your domain models from schemas
   // Domain models should be handcrafted with business logic

   // ✅ Generate only external API DTOs
   // Then map them to your domain models manually or with Mapperly
   ```

**❌ DO NOT use Mapperly for**:

1. **Object-to-JSON-string transformations**:

   ```csharp
   // ❌ Mapperly is for object-to-object mapping
   [Mapper]
   public partial interface IStateMapper
   {
       string ToJson(AvatarState state);  // Mapperly can't do this
   }

   // ✅ Use System.Text.Json
   string json = JsonSerializer.Serialize(avatarState);
   ```

2. **Dictionary<string, object> mappings**:

   ```csharp
   // ❌ Not supported by Mapperly (tracked in issue #1309)
   public Dictionary<string, object> Metadata { get; set; }

   // ✅ Use JSON serialization or manual copying
   ```

3. **Multi-source mappings**:

   ```csharp
   // ❌ Mapperly maps single source to single target
   [Mapper]
   public partial interface ISnapshotMapper
   {
       Snapshot Create(AvatarState state, AvatarMemory memory);  // Two sources!
   }

   // ✅ Use factory method
   public static Snapshot Create(AvatarState state, AvatarMemory memory)
   {
       return new Snapshot
       {
           EntityId = state.EntityId,
           StateJson = JsonSerializer.Serialize(state),
           MemoryJson = JsonSerializer.Serialize(memory),
           SnapshotTime = DateTime.UtcNow
       };
   }
   ```

4. **Generated or computed values**:

   ```csharp
   // ❌ Mapperly maps properties, doesn't generate values
   [Mapper]
   public partial interface IMapper
   {
       Target Map(Source source);  // Where does SnapshotTime come from?
   }

   // ✅ Set generated values manually or in factory method
   var target = mapper.Map(source);
   target.CreatedAt = DateTime.UtcNow;
   target.Id = Guid.NewGuid();
   ```

### When to Use Mapperly (Future)

**✅ Good use cases for Mapperly**:

1. **Simple DTO-to-Domain mapping**:

   ```csharp
   [Mapper(UseDeepCloning = true)]
   public partial interface IUserMapper
   {
       DomainUser FromDto(UserDto dto);
       List<DomainUser> FromDtos(List<UserDto> dtos);
   }
   ```

2. **Object cloning with collections**:

   ```csharp
   [Mapper(UseDeepCloning = true)]
   public partial interface IEntityMapper
   {
       Entity Clone(Entity source);
   }
   ```

3. **Property renaming/flattening**:

   ```csharp
   [Mapper]
   public partial interface IAddressMapper
   {
       [MapProperty(nameof(Source.Address.City), nameof(Target.City))]
       [MapProperty(nameof(Source.Address.State), nameof(Target.State))]
       Target Flatten(Source source);
   }
   ```

4. **Computed properties ignored**:

   ```csharp
   [Mapper]
   public partial interface IEntityMapper
   {
       [MapperIgnoreTarget(nameof(Entity.FullName))]  // Computed property
       [MapperIgnoreTarget(nameof(Entity.IsActive))]  // Computed property
       Entity FromDto(EntityDto dto);
   }
   ```

---

## 4. Troubleshooting

### Build Errors

#### "Type 'QdrantSearchResponse' could not be found"

**Problem**: Generated code isn't being built or project reference is missing.

**Solution**:

```bash
# 1. Regenerate types
nuke GenerateApiTypes

# 2. Check generated file exists
ls dotnet/framework/LablabBean.Framework.Generated/ExternalApis/

# 3. Add project reference if missing
# (See Step 4 in "Adding New External API Integrations")

# 4. Rebuild solution
dotnet build
```

#### "Nullable object must have a value"

**Problem**: Generated code doesn't have nullable annotations for optional properties.

**Solution**:

```bash
# Check quicktype flags include:
# --csharp-version 8 --framework SystemTextJson --check-required

# Verify JSON schema marks optional fields:
cat schemas/api-name/entity.schema.json | grep -A2 '"required"'

# Regenerate with correct flags
nuke GenerateApiTypes
```

#### "Could not find a part of the path 'schemas'"

**Problem**: Schemas directory doesn't exist or is empty.

**Solution**:

```bash
# Create schemas directory
mkdir -p schemas/api-name

# Verify files exist
ls schemas/**/*.schema.json

# The build target will warn but not fail if directory is missing
```

### Runtime Errors

#### "Deserialization failed: JSON value could not be converted"

**Problem**: JSON structure doesn't match schema.

**Solution**:

```csharp
// 1. Capture actual API response
var json = await response.Content.ReadAsStringAsync();
File.WriteAllText("debug-response.json", json);

// 2. Compare with schema
// 3. Update schema to match reality
// 4. Regenerate models
```

#### "Object reference not set to an instance of an object" (NullReferenceException)

**Problem**: Optional property accessed without null check.

**Solution**:

```csharp
// ❌ Don't assume optional properties are present
Console.WriteLine(point.Payload.Count);

// ✅ Check for null first
if (point.Payload != null)
{
    Console.WriteLine(point.Payload.Count);
}

// ✅ Or use null-conditional operator
Console.WriteLine(point.Payload?.Count ?? 0);
```

### Generation Issues

#### "npx: command not found"

**Problem**: Node.js or npm not installed.

**Solution**:

```bash
# Install Node.js from https://nodejs.org/
# Or use package manager:
winget install OpenJS.NodeJS  # Windows
brew install node  # macOS
apt install nodejs npm  # Linux

# Verify installation
npx --version
```

#### "quicktype: schema parse error"

**Problem**: Invalid JSON Schema syntax.

**Solution**:

```bash
# Validate JSON Schema
npx ajv-cli validate -s schemas/api-name/entity.schema.json

# Common issues:
# - Missing commas
# - Incorrect $ref paths
# - Invalid type values

# Fix schema and regenerate
```

#### "Generated file has warnings"

**Problem**: Generated code doesn't compile cleanly.

**Solution**:

```bash
# Check for specific warnings
dotnet build | grep "warning CS"

# Common fixes:
# 1. Add nullable annotations (--csharp-version 8)
# 2. Remove Newtonsoft.Json (--framework SystemTextJson)
# 3. Check required array in schema (--check-required)

# Regenerate with correct flags
```

---

## 5. Best Practices

### Schema Organization

```
schemas/
├── qdrant/              # Group by external service
│   ├── search-request.schema.json
│   ├── search-response.schema.json
│   ├── scored-point.schema.json
│   └── README.md        # Document API version, update process
├── firebase/
│   ├── user.schema.json
│   └── README.md
└── README.md            # Overall schemas documentation
```

### Schema Documentation

Always include in `schemas/{api-name}/README.md`:

- API vendor and service name
- API version (e.g., "Qdrant v1.7.0")
- Source of schemas (OpenAPI URL, documentation link)
- Update process (how to refresh schemas)
- Last updated date

**Template**:

```markdown
# {API Name} Schemas

**Vendor**: {Vendor Name}
**API Version**: {Version}
**Last Updated**: {Date}

## Source

Schemas extracted from: {URL or description}

## Update Process

```bash
# Download latest OpenAPI spec
curl -o openapi.json {URL}

# Extract relevant schemas
# ... (specific commands)
```

## Endpoints Covered

- `POST /endpoint` - Description (schema: `file.schema.json`)

```

### Naming Conventions

**Schema Files**:
- Use kebab-case: `search-request.schema.json`
- Include `.schema` suffix for JSON Schema files
- Use `-sample` suffix for sample JSON files

**Generated Classes**:
- PascalCase: `SearchRequest.g.cs`
- `.g.cs` suffix for generated files
- Match schema file name (kebab → Pascal)

**Namespaces**:
- `LablabBean.Framework.Generated.ExternalApis.{ApiName}`
- Example: `LablabBean.Framework.Generated.ExternalApis.Qdrant`

### Version Control

**Commit Generated Code**: Yes, commit `.g.cs` files to version control
- Ensures reproducible builds
- Allows code review of generated changes
- Prevents build-time failures if generation fails

**Schema Updates**:
```bash
# Update schema
vi schemas/qdrant/search-request.schema.json

# Regenerate
nuke GenerateApiTypes

# Review changes
git diff dotnet/framework/LablabBean.Framework.Generated/

# Commit both schema and generated code
git add schemas/qdrant/search-request.schema.json
git add dotnet/framework/LablabBean.Framework.Generated/ExternalApis/Qdrant/*.g.cs
git commit -m "feat(api): update Qdrant search request schema

- Add score_threshold parameter
- Update API version to 1.8.0"
```

---

## 6. Common Patterns

### Pattern: Handling Union Types

**Problem**: API returns ID as either string or number.

**Schema**:

```json
{
  "properties": {
    "id": {
      "oneOf": [
        {"type": "string"},
        {"type": "integer"}
      ]
    }
  }
}
```

**Generated Code** (quicktype handles this):

```csharp
public string Id { get; set; }  // Converts to string
```

**Usage**:

```csharp
// quicktype automatically converts numbers to strings
var id = point.Id;  // Works for both "abc123" and 12345
```

### Pattern: Dynamic Payload Processing

**Problem**: Payload has unknown structure.

**Schema**:

```json
{
  "properties": {
    "payload": {
      "type": ["object", "null"],
      "additionalProperties": true
    }
  }
}
```

**Generated Code**:

```csharp
public Dictionary<string, JsonElement>? Payload { get; set; }
```

**Usage**:

```csharp
if (point.Payload != null)
{
    foreach (var kvp in point.Payload)
    {
        // Handle different types
        switch (kvp.Value.ValueKind)
        {
            case JsonValueKind.String:
                var str = kvp.Value.GetString();
                break;
            case JsonValueKind.Number:
                var num = kvp.Value.GetDouble();
                break;
            case JsonValueKind.Object:
                var nested = kvp.Value;
                break;
        }
    }
}
```

### Pattern: Pagination

**Problem**: API returns paginated results.

**Schema**:

```json
{
  "properties": {
    "result": {"type": "array", "items": {...}},
    "offset": {"type": "integer"},
    "limit": {"type": "integer"},
    "total": {"type": "integer"}
  }
}
```

**Usage**:

```csharp
async Task<List<Item>> FetchAllPages()
{
    var allItems = new List<Item>();
    int offset = 0;
    const int limit = 100;

    while (true)
    {
        var response = await FetchPage(offset, limit);
        allItems.AddRange(response.Result);

        if (response.Result.Count < limit)
            break;  // Last page

        offset += limit;
    }

    return allItems;
}
```

---

## 7. Quick Reference

### quicktype Flags

| Flag | Purpose | Example |
|------|---------|---------|
| `--src` | Input file | `--src schema.json` |
| `--src-lang` | Input format | `--src-lang schema` (JSON Schema) or `json` (sample) |
| `--lang` | Output language | `--lang csharp` |
| `--framework` | Serialization library | `--framework SystemTextJson` |
| `--csharp-version` | C# language version | `--csharp-version 8` (nullable refs) |
| `--check-required` | Respect `required` array | `--check-required` |
| `--namespace` | C# namespace | `--namespace My.Namespace` |
| `--top-level` | Root class name | `--top-level EntityName` |
| `--out` | Output file | `--out Entity.g.cs` |
| `--array-type` | Array type | `--array-type list` (use `List<T>`) |

### NUKE Build Commands

```bash
# Generate all API types from schemas
nuke GenerateApiTypes

# Clean and regenerate
nuke Clean GenerateApiTypes

# Full build with generation
nuke Compile  # Runs GenerateApiTypes automatically
```

### System.Text.Json Attributes

```csharp
using System.Text.Json.Serialization;

[JsonPropertyName("snake_case_name")]  // Map to different JSON name
public string PascalCaseName { get; set; }

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  // Omit if null
public string? OptionalField { get; set; }

[JsonConverter(typeof(CustomConverter))]  // Custom serialization
public ComplexType Field { get; set; }
```

---

## 8. Additional Resources

### Documentation

- **quicktype Documentation**: <https://quicktype.io/>
- **System.Text.Json Guide**: <https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview>
- **Mapperly Documentation**: <https://mapperly.riok.app/>
- **JSON Schema Specification**: <https://json-schema.org/>

### Internal Documentation

- **Plan**: [plan.md](./plan.md) - Implementation plan and phases
- **Data Model**: [data-model.md](./data-model.md) - Entity definitions and mappings
- **Research**: [research.md](./research.md) - Technology decisions and rationale
- **Spec**: [spec.md](./spec.md) - Feature specification and requirements

### Getting Help

- Check [Troubleshooting](#4-troubleshooting) section above
- Review [research.md](./research.md) for design decisions
- Consult [data-model.md](./data-model.md) for entity structures
- Ask in team channel with error details and schema

---

**Document Version**: 1.0
**Last Updated**: 2025-10-27
**Status**: Ready for developer use after feature implementation
