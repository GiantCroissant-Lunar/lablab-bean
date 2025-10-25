---
doc_id: DOC-2025-00039
title: Plugin Manifest Schema Reference
doc_type: reference
status: draft
canonical: false
created: 2025-10-21
tags: [plugins, manifest, schema, configuration, json]
summary: >
  Complete reference for plugin.json manifest format including all fields, validation rules, and examples
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Plugin Manifest Schema Reference

Complete reference for `plugin.json` manifest format.

## Overview

Every plugin **must** include a `plugin.json` manifest file in its output directory. The plugin loader discovers plugins by scanning for this file.

**Location**: Root of plugin output directory (e.g., `plugins/inventory/plugin.json`)

**Format**: JSON (no comments, strict syntax)

**Encoding**: UTF-8

**Validation**: Performed by `PluginLoader` during discovery phase

## Minimal Example

```json
{
  "id": "my-plugin",
  "name": "My Plugin",
  "version": "1.0.0",
  "description": "A simple plugin",
  "author": "Your Name",
  "entryPoint": {
    "dotnet.console": "MyPlugin.dll,Namespace.MyPlugin"
  },
  "dependencies": [],
  "capabilities": [],
  "priority": 100
}
```

## Complete Schema

```json
{
  "id": "string (required)",
  "name": "string (required)",
  "version": "string (required)",
  "description": "string (optional)",
  "author": "string (optional)",
  "license": "string (optional)",
  "homepage": "string (optional)",
  "repository": "string (optional)",
  "tags": ["string"],
  "entryPoint": {
    "dotnet.console": "string (required)",
    "dotnet.sadconsole": "string (optional)",
    "unity": "string (optional)"
  },
  "dependencies": [
    {
      "id": "string (required)",
      "versionRange": "string (optional)",
      "optional": false
    }
  ],
  "capabilities": ["string"],
  "priority": 100,
  "security": {
    "permissions": ["string"],
    "sandboxed": false
  },
  "metadata": {
    "key": "value"
  }
}
```

## Field Reference

### Required Fields

#### `id` (string, required)

Unique plugin identifier.

**Constraints**:

- Must be unique across all plugins
- Lowercase recommended
- Alphanumeric + hyphens only (`[a-z0-9-]+`)
- Kebab-case preferred

**Examples**:

```json
"id": "inventory"
"id": "status-effects"
"id": "advanced-combat-system"
```

**Validation**:

- Must not be null or empty
- Must not contain spaces or special characters
- Should match `IPlugin.Id` property

**Location**: Used for:

- Dependency resolution
- Service registry metadata
- Logger category naming
- Configuration section naming

---

#### `name` (string, required)

Human-readable plugin display name.

**Constraints**:

- Any valid string
- No length limit (reasonable limit: 50 chars)

**Examples**:

```json
"name": "Inventory System"
"name": "Advanced Combat Mechanics"
```

**Location**: Used for:

- Log messages
- UI display (if applicable)
- Error reporting

---

#### `version` (string, required)

Plugin version (semantic versioning recommended).

**Format**: `MAJOR.MINOR.PATCH` (e.g., `1.0.0`)

**Constraints**:

- Must not be null or empty
- Semantic versioning recommended but not enforced

**Examples**:

```json
"version": "1.0.0"
"version": "2.1.3-beta"
```

**Location**: Used for:

- Dependency version resolution
- Service metadata
- Compatibility checking

---

#### `entryPoint` (object, required)

Profile-specific entry points for multi-targeting.

**Format**: `{ "profile": "assembly,type" }`

**Required profiles**:

- `dotnet.console` - Terminal.Gui console application

**Optional profiles**:

- `dotnet.sadconsole` - SadConsole UI
- `unity` - Unity game engine (future)

**Assembly format**: `AssemblyName.dll,Fully.Qualified.TypeName`

**Examples**:

Single profile:

```json
"entryPoint": {
  "dotnet.console": "LablabBean.Plugins.Inventory.dll,LablabBean.Plugins.Inventory.InventoryPlugin"
}
```

Multi-profile:

```json
"entryPoint": {
  "dotnet.console": "MyPlugin.dll,MyPlugin.ConsolePlugin",
  "dotnet.sadconsole": "MyPlugin.dll,MyPlugin.SadConsolePlugin"
}
```

**Validation**:

- At least one profile required
- Assembly file must exist in plugin directory
- Type must implement `IPlugin` interface
- Format: `{assembly},{namespace}.{className}`

**Active profile**: Set in `appsettings.json`:

```json
{
  "Plugins": {
    "Profile": "dotnet.console"
  }
}
```

---

### Optional Fields

#### `description` (string, optional)

Brief description of plugin functionality.

**Example**:

```json
"description": "Provides inventory management for items, equipment, and resources"
```

**Recommended length**: 1-2 sentences

---

#### `author` (string, optional)

Plugin author name or organization.

**Example**:

```json
"author": "Lablab Bean Team"
"author": "John Doe <john@example.com>"
```

---

#### `license` (string, optional)

SPDX license identifier or custom license.

**Examples**:

```json
"license": "MIT"
"license": "Apache-2.0"
"license": "Proprietary"
```

---

#### `homepage` (string, optional)

Plugin homepage or documentation URL.

**Example**:

```json
"homepage": "https://github.com/user/plugin"
```

---

#### `repository` (string, optional)

Source repository URL.

**Example**:

```json
"repository": "https://github.com/user/plugin.git"
```

---

#### `tags` (array of strings, optional)

Searchable tags for plugin categorization.

**Examples**:

```json
"tags": ["inventory", "items", "equipment"]
"tags": ["combat", "gameplay", "mechanics"]
```

---

#### `dependencies` (array of objects, optional)

Plugin dependencies (loaded before this plugin).

**Default**: `[]` (no dependencies)

**Format**:

```json
"dependencies": [
  {
    "id": "other-plugin",
    "versionRange": ">=1.0.0 <2.0.0",
    "optional": false
  }
]
```

**Fields**:

- `id` (string, required) - Dependency plugin ID
- `versionRange` (string, optional) - Semantic version range (default: `*`)
- `optional` (boolean, optional) - Allow missing dependency (default: `false`)

**Version range syntax**:

- `*` - Any version
- `1.0.0` - Exact version
- `>=1.0.0` - Minimum version
- `>=1.0.0 <2.0.0` - Range (recommended)
- `^1.0.0` - Compatible with 1.x.x
- `~1.2.0` - Compatible with 1.2.x

**Examples**:

Required dependency:

```json
"dependencies": [
  {
    "id": "inventory",
    "versionRange": ">=1.0.0 <2.0.0",
    "optional": false
  }
]
```

Optional dependency:

```json
"dependencies": [
  {
    "id": "audio-system",
    "optional": true
  }
]
```

**Validation**:

- Circular dependencies detected and rejected
- Missing required dependencies prevent plugin load
- Load order: dependencies loaded first (topological sort)

---

#### `capabilities` (array of strings, optional)

Plugin capabilities for discovery and filtering.

**Default**: `[]`

**Purpose**: Searchable metadata for plugin marketplace/catalog

**Examples**:

```json
"capabilities": ["gameplay", "inventory"]
"capabilities": ["ui", "rendering", "terminal"]
"capabilities": ["demo", "test"]
```

**Common capability tags**:

- `gameplay` - Game mechanics
- `ui` - User interface
- `rendering` - Graphics/rendering
- `audio` - Sound/music
- `network` - Networking/multiplayer
- `persistence` - Save/load
- `debug` - Development tools
- `demo` - Example/demo plugin
- `test` - Testing utilities

---

#### `priority` (integer, optional)

Plugin load priority. Higher = loaded later.

**Default**: `100`

**Range**: 0-1000 (recommended)

**Purpose**: Determines load order for plugins with same dependency level

**Guidelines**:

- Core framework plugins: 1000+
- Game logic plugins: 100-500
- UI plugins: 50-99
- Test/demo plugins: 0-49

**Examples**:

```json
"priority": 100  // Default
"priority": 200  // Load after default plugins
"priority": 50   // Load before default plugins
```

**Note**: Dependencies always load before dependents, regardless of priority.

---

#### `security` (object, optional)

Security configuration (future enhancement).

**Format**:

```json
"security": {
  "permissions": ["FileSystem.Read", "Network.Connect"],
  "sandboxed": true
}
```

**Fields**:

- `permissions` (array of strings) - Required permissions
- `sandboxed` (boolean) - Run in restricted sandbox

**Status**: Not implemented yet (Phase 5+ feature)

---

#### `metadata` (object, optional)

Custom metadata for plugin-specific configuration.

**Format**: Key-value pairs (string keys, any JSON value)

**Examples**:

```json
"metadata": {
  "minGameVersion": "1.0.0",
  "maxPlayers": 4,
  "experimental": true
}
```

**Purpose**: Plugin-specific data (not used by plugin loader)

---

## Validation Rules

The plugin loader validates manifests during discovery phase. Validation failures prevent plugin load.

### Required Field Validation

| Field | Validation |
|-------|------------|
| `id` | Not null/empty, no spaces, alphanumeric + hyphens |
| `name` | Not null/empty |
| `version` | Not null/empty |
| `entryPoint` | At least one profile, assembly exists, type implements IPlugin |

### Entry Point Validation

```json
"entryPoint": {
  "dotnet.console": "Assembly.dll,Namespace.Type"
}
```

**Checks**:

1. `Assembly.dll` exists in plugin directory
2. Format is `{assembly},{fullTypeName}`
3. Type exists in assembly (checked during load)
4. Type implements `IPlugin` interface

**Error examples**:

- ❌ `Assembly.dll` - Missing type name
- ❌ `Namespace.Type` - Missing assembly name
- ❌ `Assembly.dll,Type` - Type name not fully qualified
- ✅ `Assembly.dll,Namespace.Type` - Valid

### Dependency Validation

```json
"dependencies": [
  {
    "id": "other-plugin",
    "versionRange": ">=1.0.0 <2.0.0",
    "optional": false
  }
]
```

**Checks**:

1. No circular dependencies (A → B → A)
2. Required dependencies exist and loaded
3. Version ranges satisfied
4. Load order respects dependency graph

**Error examples**:

- ❌ Plugin A depends on B, B depends on A (circular)
- ❌ Plugin A requires B, but B not installed (missing required dependency)
- ❌ Plugin A requires B v2.0, but B v1.0 installed (version mismatch)
- ✅ Plugin A depends on B, B loaded first

---

## Complete Examples

### Example 1: Minimal Plugin

```json
{
  "id": "hello-world",
  "name": "Hello World Plugin",
  "version": "1.0.0",
  "description": "A simple hello world plugin",
  "author": "Developer",
  "entryPoint": {
    "dotnet.console": "HelloWorld.dll,HelloWorld.HelloWorldPlugin"
  },
  "dependencies": [],
  "capabilities": ["demo"],
  "priority": 100
}
```

### Example 2: Plugin with Dependencies

```json
{
  "id": "advanced-inventory",
  "name": "Advanced Inventory System",
  "version": "2.0.0",
  "description": "Enhanced inventory with crafting and storage",
  "author": "Lablab Bean Team",
  "license": "MIT",
  "entryPoint": {
    "dotnet.console": "AdvancedInventory.dll,AdvancedInventory.InventoryPlugin"
  },
  "dependencies": [
    {
      "id": "inventory",
      "versionRange": ">=1.0.0 <2.0.0",
      "optional": false
    },
    {
      "id": "crafting-system",
      "versionRange": ">=1.5.0",
      "optional": true
    }
  ],
  "capabilities": ["gameplay", "inventory", "crafting"],
  "priority": 150
}
```

### Example 3: Multi-Profile Plugin

```json
{
  "id": "universal-renderer",
  "name": "Universal Renderer",
  "version": "1.0.0",
  "description": "Cross-platform rendering engine",
  "author": "Rendering Team",
  "entryPoint": {
    "dotnet.console": "Renderer.dll,Renderer.TerminalGuiRenderer",
    "dotnet.sadconsole": "Renderer.dll,Renderer.SadConsoleRenderer"
  },
  "dependencies": [],
  "capabilities": ["rendering", "ui"],
  "priority": 200,
  "metadata": {
    "gpuAccelerated": false,
    "maxResolution": "1920x1080"
  }
}
```

### Example 4: Framework Plugin

```json
{
  "id": "core-logging",
  "name": "Core Logging System",
  "version": "3.0.0",
  "description": "Centralized logging infrastructure",
  "author": "Framework Team",
  "license": "Apache-2.0",
  "entryPoint": {
    "dotnet.console": "CoreLogging.dll,CoreLogging.LoggingPlugin"
  },
  "dependencies": [],
  "capabilities": ["framework", "logging"],
  "priority": 1000,
  "security": {
    "permissions": ["FileSystem.Write"],
    "sandboxed": false
  }
}
```

---

## File Placement

### Development

During development, `plugin.json` lives in project root:

```
dotnet/plugins/MyPlugin/
├── MyPlugin.cs
├── MyPlugin.csproj
└── plugin.json  ← Source file
```

### Build Output

After build, `plugin.json` must be copied to output directory:

**.csproj configuration**:

```xml
<ItemGroup>
  <None Update="plugin.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Output structure**:

```
plugins/my-plugin/
├── MyPlugin.dll
├── MyPlugin.deps.json
└── plugin.json  ← Loader discovers this file
```

---

## Discovery Process

The plugin loader discovers plugins using this algorithm:

1. **Scan paths**: Read `Plugins:Paths` from `appsettings.json`

   ```json
   {
     "Plugins": {
       "Paths": ["plugins", "../../../plugins"]
     }
   }
   ```

2. **Find manifests**: Search for `plugin.json` files recursively

3. **Parse manifests**: Deserialize JSON and validate required fields

4. **Resolve dependencies**: Build dependency graph (topological sort)

5. **Load plugins**: Load in dependency order, respecting priority

**Loader implementation**: `dotnet/framework/LablabBean.Plugins.Core/PluginLoader.cs:157-261`

---

## Configuration Integration

Plugin manifests integrate with host configuration:

### appsettings.json

```json
{
  "Plugins": {
    "Paths": ["plugins"],
    "Profile": "dotnet.console",
    "HotReload": false,
    "AllowedPlugins": ["inventory", "status-effects"],
    "BlockedPlugins": []
  },
  "Inventory": {
    "MaxItems": 100
  }
}
```

**Plugin-specific config**: Use plugin ID as section name (`Inventory`).

**Access in plugin**:

```csharp
var config = context.Configuration.GetSection("Inventory");
var maxItems = config.GetValue<int>("MaxItems", 100);
```

---

## Error Handling

### Common Validation Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Plugin manifest not found` | No `plugin.json` in plugin directory | Add manifest file |
| `Invalid entry point format` | Wrong format in `entryPoint` | Use `Assembly.dll,Namespace.Type` |
| `Assembly not found` | Assembly doesn't exist | Check assembly name and build output |
| `Type does not implement IPlugin` | Entry point class doesn't implement interface | Add `: IPlugin` to class |
| `Circular dependency detected` | A→B→A dependency cycle | Remove circular reference |
| `Required dependency missing` | Dependency not installed | Install dependency or mark as optional |
| `Version mismatch` | Dependency version out of range | Update dependency or version range |

### Debugging Tips

**Enable verbose logging**:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "LablabBean.Plugins": "Debug"
      }
    }
  }
}
```

**Check discovery output**:

```
[Debug] Discovered plugin manifest: plugins/inventory/plugin.json
[Debug] Validating plugin: inventory v1.0.0
[Debug] Resolving dependencies for: inventory
[Information] Loading plugin: inventory
```

---

## JSON Schema (Future)

JSON Schema validation will be available in future versions:

```json
{
  "$schema": "https://lablab-bean.dev/schemas/plugin.schema.json",
  "id": "my-plugin",
  ...
}
```

**Schema file**: `dotnet/framework/LablabBean.Plugins.Contracts/plugin.schema.json` (to be created)

**Validation**: Use JSON schema validator in IDE or CI/CD pipeline

---

## Related Documentation

- **Quick-Start Guide**: `docs/_inbox/2025-10-21-plugin-development-quickstart--DOC-2025-00037.md`
- **Contracts API**: `docs/_inbox/2025-10-21-plugin-contracts-api--DOC-2025-00038.md`
- **Architecture**: `specs/004-tiered-plugin-architecture/spec.md`
- **Loader Implementation**: `dotnet/framework/LablabBean.Plugins.Core/PluginLoader.cs`

---

**Version**: 1.0.0
**Created**: 2025-10-21
**Author**: Claude (Sonnet 4.5)
