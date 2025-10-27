# LablabBean.Framework.Generated

This project contains automatically generated code from external sources:

## 📁 Structure

```
LablabBean.Framework.Generated/
├── ExternalApis/        # quicktype-generated models from JSON schemas/samples
│   └── *.g.cs           # Auto-generated C# models for external APIs
├── Mappers/             # Mapperly object mappers
│   └── *Mapper.cs       # Compile-time generated mappers
└── README.md
```

## 🔧 Tools Used

### quicktype - JSON to Strongly-Typed Models

- **Purpose**: Generate C# models from JSON schemas or samples
- **Location**: `ExternalApis/`
- **Naming**: `*.g.cs` (generated)
- **Generation**: Run `nuke GenerateApiTypes` from build directory
- **Source**: JSON samples in `schemas/` directory (root)

### Mapperly - Object Mapping

- **Purpose**: Zero-overhead compile-time object mapping
- **Location**: `Mappers/`
- **Usage**: Define partial classes with `[Mapper]` attribute
- **Generation**: Automatic during build via source generator

## 🚀 Usage

### Using Generated API Models

```csharp
using LablabBean.Framework.Generated.ExternalApis;

// Parse JSON response
var response = QdrantPointResponse.FromJson(jsonString);

// Access properties
Console.WriteLine($"Point ID: {response.Id}");
Console.WriteLine($"Vector length: {response.Vector.Count}");
```

### Creating Mapperly Mappers

```csharp
using Riok.Mapperly.Abstractions;

namespace LablabBean.Framework.Generated.Mappers;

[Mapper]
public partial class MyDataMapper
{
    // Mapperly generates the implementation at compile-time
    public partial DestinationModel MapToDestination(SourceModel source);

    // Lists are mapped automatically
    public partial List<DestinationModel> MapList(List<SourceModel> sources);

    // Update existing object
    public partial void UpdateDestination(SourceModel source, DestinationModel dest);
}
```

### Advanced Mapperly Features

```csharp
[Mapper]
public partial class AdvancedMapper
{
    // Property name mapping
    [MapProperty(nameof(Source.OldName), nameof(Dest.NewName))]
    public partial Dest Map(Source source);

    // Ignore properties
    [MapperIgnoreSource(nameof(Source.IgnoreMe))]
    public partial Dest MapWithIgnore(Source source);

    // Custom value converter
    private string FormatName(string name) => name.ToUpper();

    [MapProperty(nameof(Source.Name), nameof(Dest.FormattedName), Use = nameof(FormatName))]
    public partial Dest MapWithConverter(Source source);
}
```

## 🔄 Regenerating Code

### API Models (quicktype)

```bash
# From build/nuke directory
nuke GenerateApiTypes
```

This processes all `*-sample.json` files in `schemas/` and generates C# models.

### Mappers (Mapperly)

Mappers are regenerated automatically during build. No manual action needed.

## 📝 Adding New Schemas

1. Create JSON sample: `schemas/qdrant/my-type-sample.json`
2. Run: `nuke GenerateApiTypes`
3. Generated file: `ExternalApis/MyType.g.cs`
4. Commit both schema and generated code

## 📚 References

- **Mapperly**: <https://mapperly.riok.app/>
- **quicktype**: <https://quicktype.io/>
- **Package Versions**: See `Directory.Packages.props`

## ⚠️ Important Notes

- **DO NOT** edit `*.g.cs` files manually - they will be overwritten
- **DO** commit generated code to git for transparency
- **DO** test mappers with unit tests
- Mapperly has zero runtime cost - all code is generated at compile-time
- quicktype supports 20+ programming languages if needed for other projects

---

**Generated Code**: This project follows the convention that generated files use `.g.cs` suffix.
