---
doc_id: DOC-2025-00047
title: Code Generation Quick Start (quicktype & Mapperly)
doc_type: guide
status: active
canonical: true
created: 2025-10-27
tags: [code-generation, quicktype, mapperly, quickstart, tooling]
summary: Quick start guide for using quicktype and Mapperly to eliminate boilerplate in object mapping and JSON deserialization.
source:
  author: agent
related: [DOC-2025-00046, DOC-2025-00048, DOC-2025-00029]
---

# quicktype & Mapperly Quick Start

This project uses **quicktype** and **Mapperly** to eliminate boilerplate code.

## 🎯 What & Why

### quicktype

- **What**: Generates C# models from JSON samples
- **Why**: Avoid manual typing of API response models
- **Example**: Qdrant API responses → Strongly-typed C# classes

### Mapperly

- **What**: Compile-time object mapper (like AutoMapper but faster)
- **Why**: Zero-runtime-cost mapping between objects
- **Example**: Map Firebase DTOs to domain models

## ⚡ Quick Usage

### 1. Generate API Models

```bash
# Add JSON sample to schemas/
cat > schemas/myapi/user-sample.json << EOF
{
  "id": "123",
  "name": "John Doe",
  "email": "john@example.com"
}
EOF

# Generate C# models
cd build/nuke
nuke GenerateApiTypes

# Use generated model
using LablabBean.Framework.Generated.ExternalApis;
var user = User.FromJson(jsonString);
```

### 2. Create Object Mapper

```csharp
using Riok.Mapperly.Abstractions;

namespace LablabBean.Framework.Generated.Mappers;

[Mapper]
public partial class UserMapper
{
    // Mapperly generates the code at compile-time
    public partial DomainUser MapToDomain(ApiUser apiUser);
}

// Usage
var mapper = new UserMapper();
var domainUser = mapper.MapToDomain(apiUser);
```

## 📁 File Locations

- **JSON Samples**: `schemas/**/*-sample.json`
- **Generated Models**: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/*.g.cs`
- **Mappers**: `dotnet/framework/LablabBean.Framework.Generated/Mappers/*Mapper.cs`

## 🔧 Commands

```bash
# Generate all API types from JSON samples
cd build/nuke && nuke GenerateApiTypes

# Build project (regenerates Mapperly mappers automatically)
cd dotnet && dotnet build

# Add to another project
dotnet add reference ../framework/LablabBean.Framework.Generated/LablabBean.Framework.Generated.csproj
```

## 📚 Full Documentation

- **Project README**: `dotnet/framework/LablabBean.Framework.Generated/README.md`
- **Schema Guide**: `schemas/README.md`
- **Mapperly Docs**: <https://mapperly.riok.app/>
- **quicktype Docs**: <https://quicktype.io/>

## 💡 Examples

### Before: Manual Mapping

```csharp
var destination = new DestinationModel
{
    Id = source.Id,
    Name = source.Name,
    Email = source.Email,
    CreatedAt = source.CreatedAt,
    UpdatedAt = source.UpdatedAt,
    // ... 20 more properties
};
```

### After: With Mapperly

```csharp
[Mapper]
public partial class MyMapper
{
    public partial DestinationModel Map(SourceModel source);
}

var destination = mapper.Map(source); // One line!
```

### Before: Manual JSON Parsing

```csharp
var json = JsonDocument.Parse(jsonString);
var id = json.RootElement.GetProperty("id").GetString();
var name = json.RootElement.GetProperty("name").GetString();
// ... error-prone and tedious
```

### After: With quicktype

```csharp
var response = QdrantPointResponse.FromJson(jsonString);
Console.WriteLine($"ID: {response.Id}, Name: {response.Name}");
```

## 🎯 Benefits

- ✅ **Type Safety**: Catch errors at compile-time
- ✅ **Zero Runtime Cost**: Mapperly generates code at compile-time
- ✅ **Less Boilerplate**: Focus on business logic, not data mapping
- ✅ **Maintainability**: Change once in sample, regenerate everywhere
- ✅ **IDE Support**: Full IntelliSense and refactoring support

---

**Status**: ✅ Integrated and ready to use
**Version**: Mapperly 4.1.0, quicktype (latest via npx)
**Last Updated**: 2025-10-27
