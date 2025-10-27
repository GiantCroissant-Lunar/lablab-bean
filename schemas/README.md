# JSON Schemas for quicktype Code Generation

This directory contains JSON samples used to generate strongly-typed C# models via quicktype.

## Directory Structure

```
schemas/
├── qdrant/                     # Qdrant vector database API models
│   ├── point-response-sample.json
│   └── point-response.json     # (Draft schema - not used yet)
└── README.md
```

## Usage

Schemas are automatically processed during the build:

```bash
# From build/nuke directory
nuke GenerateApiTypes
```

Generated files are placed in `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/`.

## Adding New Schemas

1. Create a JSON sample file ending with `-sample.json` in the appropriate subdirectory
2. Use realistic sample data that represents the API response
3. Run `nuke GenerateApiTypes` to generate C# models
4. The generated class name is derived from the filename (kebab-case → PascalCase)
5. Commit both the sample and generated code

## Sample File Guidelines

- **Naming**: Use kebab-case with `-sample.json` suffix (e.g., `user-profile-sample.json`)
- **Output**: Generates PascalCase class (e.g., `UserProfile`)
- **Content**: Use realistic sample data, not schema definitions
- **Location**: Organize by API/service (e.g., `qdrant/`, `firebase/`)

## Example Sample File

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2025-01-01T00:00:00Z",
  "tags": ["user", "active"],
  "metadata": {
    "source": "api",
    "version": "1.0"
  }
}
```

This generates a `UserProfile` class with properly typed properties.

## Why Samples Instead of Schemas?

quicktype works best with actual JSON samples:

- ✅ Easier to create (just copy API response)
- ✅ More accurate type inference
- ✅ Handles nested objects automatically
- ✅ Better array type detection

JSON Schema is also supported but requires more manual work.

## Generated Code Features

The generated C# models include:

- Property validation
- JSON serialization/deserialization methods
- Proper nullable handling
- Type-safe collections
- Extension methods for JSON parsing

## Tips

- Use **real API responses** as samples when possible
- Include **all optional fields** in samples for complete models
- Use **representative data types** (strings, numbers, booleans, dates)
- For arrays, include at least one item to infer element type
- For polymorphic types, create separate samples

## Build Integration

The `GenerateApiTypes` target in `Build.cs`:

1. Scans `schemas/**/*-sample.json`
2. Extracts type name from filename
3. Runs quicktype with appropriate options
4. Outputs to `ExternalApis/*.g.cs`

See [quicktype documentation](https://quicktype.io/docs) for advanced usage.
