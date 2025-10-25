---
doc_id: DOC-2025-00038
title: Plugin Contracts API Reference
doc_type: reference
status: draft
canonical: false
created: 2025-10-21
tags: [plugins, api, contracts, interfaces, reference]
summary: >
  Complete API reference for plugin contract interfaces (IPlugin, IPluginContext, IRegistry, IPluginHost)
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Plugin Contracts API Reference

Complete reference for `LablabBean.Plugins.Contracts` (netstandard2.1).

## Overview

The plugin contracts library defines the interface between plugins and the host application. All types in this assembly are **shared across AssemblyLoadContext boundaries**, ensuring type compatibility between plugins and host.

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/`

**Framework**: netstandard2.1 (maximum compatibility)

**Purpose**: Minimal surface area for plugin integration without coupling to host implementation.

## Core Interfaces

### IPlugin

```csharp
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
```

**Purpose**: Plugin lifecycle contract. All plugins must implement this interface.

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/IPlugin.cs:9`

#### Properties

##### `string Id { get; }`

Unique plugin identifier. Must match `id` field in `plugin.json`.

**Constraints**:

- Lowercase recommended (enforced by loader)
- Kebab-case preferred (`inventory-system`, not `InventorySystem`)
- Globally unique within application

**Example**:

```csharp
public string Id => "inventory";
```

##### `string Name { get; }`

Human-readable plugin name.

**Example**:

```csharp
public string Name => "Inventory System";
```

##### `string Version { get; }`

Plugin version (semantic versioning recommended).

**Format**: `MAJOR.MINOR.PATCH`

**Example**:

```csharp
public string Version => "1.0.0";
```

#### Methods

##### `Task InitializeAsync(IPluginContext context, CancellationToken ct)`

**Location**: `IPlugin.cs:18`

Called once during plugin load. Use this to:

- Store context references (logger, host, configuration)
- Register services via `context.Registry`
- Initialize state (do NOT start background work here)

**Lifecycle stage**: Phase 4 (after dependency resolution, before StartAsync)

**Parameters**:

- `context` - Plugin initialization context (never null)
- `ct` - Cancellation token for graceful shutdown

**Returns**: Task (use `Task.CompletedTask` for synchronous initialization)

**Example**:

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    _logger = context.Logger;
    _config = context.Configuration.GetSection("Inventory");

    var service = new InventoryService(_logger);
    context.Registry.Register<IInventoryService>(service, priority: 100);

    return Task.CompletedTask;
}
```

**Error handling**: Exceptions thrown here prevent plugin load and log error.

##### `Task StartAsync(CancellationToken ct)`

**Location**: `IPlugin.cs:22`

Called after all plugins initialized. Use this to:

- Start background work (timers, hosted services)
- Subscribe to events
- Begin async operations

**Lifecycle stage**: Phase 5 (all plugins initialized, dependency graph resolved)

**Parameters**:

- `ct` - Cancellation token for graceful shutdown

**Returns**: Task (supports async operations)

**Example**:

```csharp
public Task StartAsync(CancellationToken ct)
{
    _logger?.LogInformation("Starting background processing");
    _timer = new Timer(ProcessQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    return Task.CompletedTask;
}
```

**Important**: This method may run concurrently for independent plugins.

##### `Task StopAsync(CancellationToken ct)`

**Location**: `IPlugin.cs:27`

Called during application shutdown. Use this to:

- Stop background work
- Unsubscribe from events
- Dispose resources
- Flush pending operations

**Lifecycle stage**: Shutdown (reverse load order)

**Parameters**:

- `ct` - Cancellation token with timeout (graceful shutdown period)

**Returns**: Task (supports async cleanup)

**Example**:

```csharp
public Task StopAsync(CancellationToken ct)
{
    _logger?.LogInformation("Stopping plugin");
    _timer?.Dispose();
    _subscription?.Dispose();
    return Task.CompletedTask;
}
```

---

### IPluginContext

```csharp
public interface IPluginContext
{
    IRegistry Registry { get; }
    IConfiguration Configuration { get; }
    ILogger Logger { get; }
    IPluginHost Host { get; }
}
```

**Purpose**: Plugin initialization context provided by host at runtime.

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/IPlugin.cs:35`

**Lifecycle**: Valid only during `InitializeAsync`. Store references to needed properties.

#### Properties

##### `IRegistry Registry { get; }`

**Location**: `IPlugin.cs:40`

Service registry for cross-ALC service registration.

**Purpose**: Register plugin services visible to host and other plugins.

**Usage**:

```csharp
context.Registry.Register<IInventoryService>(myService, priority: 100);
```

See [IRegistry](#iregistry) for details.

##### `IConfiguration Configuration { get; }`

**Location**: `IPlugin.cs:45`

Host configuration (Microsoft.Extensions.Configuration).

**Purpose**: Read-only access to `appsettings.json` and environment configuration.

**Usage**:

```csharp
var section = context.Configuration.GetSection("Inventory");
var maxItems = section.GetValue<int>("MaxItems", 100);
```

**Configuration structure**:

```json
{
  "Plugins": {
    "Paths": ["plugins"],
    "Profile": "dotnet.console"
  },
  "Inventory": {
    "MaxItems": 100
  }
}
```

##### `ILogger Logger { get; }`

**Location**: `IPlugin.cs:50`

Logger instance for this plugin (category = plugin ID).

**Purpose**: Structured logging via Microsoft.Extensions.Logging.

**Usage**:

```csharp
_logger.LogInformation("Inventory loaded with {ItemCount} items", items.Count);
_logger.LogError(ex, "Failed to load inventory data");
```

**Log levels** (configured in `appsettings.json`):

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "LablabBean.Plugins.Inventory": "Debug"
      }
    }
  }
}
```

##### `IPluginHost Host { get; }`

**Location**: `IPlugin.cs:55`

Host services surface (events, logging, service provider).

**Purpose**: Access host-provided services and event bus.

See [IPluginHost](#ipluginhost) for details.

---

### IPluginHost

```csharp
public interface IPluginHost
{
    ILogger CreateLogger(string categoryName);
    IServiceProvider Services { get; }
    void PublishEvent<T>(T evt);
}
```

**Purpose**: Host surface available to plugins for logging, events, and services.

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/IPluginHost.cs:9`

#### Methods

##### `ILogger CreateLogger(string categoryName)`

**Location**: `IPluginHost.cs:14`

Create a logger with custom category name.

**Parameters**:

- `categoryName` - Log category (e.g., "Inventory.Service")

**Returns**: ILogger instance

**Usage**:

```csharp
var logger = context.Host.CreateLogger("Inventory.DataLoader");
logger.LogDebug("Loading data from {Path}", path);
```

**Note**: Usually not needed; use `context.Logger` instead.

##### `IServiceProvider Services { get; }`

**Location**: `IPluginHost.cs:19`

Service provider for host-provided services (Microsoft.Extensions.DependencyInjection).

**Purpose**: Resolve services registered by host (not plugin services).

**Usage**:

```csharp
var config = context.Host.Services.GetService<IConfiguration>();
var logger = context.Host.Services.GetService<ILogger<MyClass>>();
```

**Warning**: Only use for host services. Plugin services use `IRegistry`.

##### `void PublishEvent<T>(T evt)`

**Location**: `IPluginHost.cs:24`

Publish an event to the host event bus.

**Type parameter**:

- `T` - Event type (any class)

**Parameters**:

- `evt` - Event data

**Usage**:

```csharp
context.Host.PublishEvent(new InventoryChangedEvent
{
    ItemId = "sword_01",
    Quantity = 5,
    Timestamp = DateTime.UtcNow
});
```

**Note**: Event delivery mechanism depends on host implementation (synchronous or async).

---

### IRegistry

```csharp
public interface IRegistry
{
    void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class;
    void Register<TService>(TService implementation, int priority = 100) where TService : class;

    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;

    bool IsRegistered<TService>() where TService : class;
    bool Unregister<TService>(TService implementation) where TService : class;
}
```

**Purpose**: Service registry for cross-ALC plugin communication.

**Location**: `dotnet/framework/LablabBean.Plugins.Contracts/IRegistry.cs:10`

**Key feature**: Uses runtime type matching (not compile-time references) to support plugin isolation.

#### Methods

##### `void Register<TService>(TService implementation, ServiceMetadata metadata)`

**Location**: `IRegistry.cs:15`

Register a service implementation with full metadata.

**Type parameter**:

- `TService` - Service interface type

**Parameters**:

- `implementation` - Service instance
- `metadata` - Registration metadata (priority, name, version)

**Usage**:

```csharp
context.Registry.Register<IInventoryService>(myService, new ServiceMetadata
{
    Priority = 200,
    Name = "MainInventory",
    Version = "1.0.0"
});
```

##### `void Register<TService>(TService implementation, int priority = 100)`

**Location**: `IRegistry.cs:20`

Register a service with priority (shorthand).

**Type parameter**:

- `TService` - Service interface type

**Parameters**:

- `implementation` - Service instance
- `priority` - Priority (default: 100)

**Usage**:

```csharp
context.Registry.Register<IInventoryService>(myService, priority: 150);
```

**Priority guidelines**:

- Framework: 1000+
- Game plugins: 100-500
- UI plugins: 50-99
- Default: 100

##### `TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority)`

**Location**: `IRegistry.cs:26`

Get a single service implementation using selection mode.

**Type parameter**:

- `TService` - Service interface type

**Parameters**:

- `mode` - Selection mode (default: HighestPriority)

**Returns**: Service implementation

**Throws**:

- `InvalidOperationException` - If mode is `One` and multiple or zero implementations exist

**Usage**:

```csharp
var service = context.Registry.Get<IInventoryService>();
var uniqueService = context.Registry.Get<IGameEngine>(SelectionMode.One);
```

**Selection modes**:

- `HighestPriority` - Return highest priority implementation (default)
- `One` - Require exactly one implementation (throws if 0 or 2+)
- `All` - Throws; use `GetAll()` instead

##### `IEnumerable<TService> GetAll<TService>()`

**Location**: `IRegistry.cs:31`

Get all registered implementations of a service.

**Type parameter**:

- `TService` - Service interface type

**Returns**: Enumerable of all implementations (ordered by priority descending)

**Usage**:

```csharp
var allRenderers = context.Registry.GetAll<IRenderer>();
foreach (var renderer in allRenderers)
{
    renderer.Render(scene);
}
```

##### `bool IsRegistered<TService>()`

**Location**: `IRegistry.cs:36`

Check if any implementation is registered.

**Type parameter**:

- `TService` - Service interface type

**Returns**: `true` if at least one implementation registered

**Usage**:

```csharp
if (context.Registry.IsRegistered<IInventoryService>())
{
    var service = context.Registry.Get<IInventoryService>();
}
```

##### `bool Unregister<TService>(TService implementation)`

**Location**: `IRegistry.cs:41`

Unregister a specific service implementation.

**Type parameter**:

- `TService` - Service interface type

**Parameters**:

- `implementation` - Service instance to remove

**Returns**: `true` if unregistered, `false` if not found

**Usage**:

```csharp
context.Registry.Unregister<IInventoryService>(myService);
```

**Note**: Typically called during plugin unload (`StopAsync`).

---

## Supporting Types

### SelectionMode

```csharp
public enum SelectionMode
{
    One,
    HighestPriority,
    All
}
```

**Purpose**: Selection mode for `Get<TService>` when multiple implementations exist.

**Location**: `IRegistry.cs:47`

#### Values

##### `One`

**Location**: `IRegistry.cs:52`

Exactly one implementation required. Throws `InvalidOperationException` if zero or multiple exist.

**Use case**: Critical services that must have exactly one provider.

```csharp
var engine = registry.Get<IGameEngine>(SelectionMode.One);
```

##### `HighestPriority`

**Location**: `IRegistry.cs:57`

Return the implementation with highest priority. Default behavior.

**Use case**: Services with fallback implementations.

```csharp
var renderer = registry.Get<IRenderer>(SelectionMode.HighestPriority);
// Returns highest priority renderer
```

##### `All`

**Location**: `IRegistry.cs:62`

Throws exception; caller should use `GetAll()` instead.

**Use case**: None (exists for API clarity).

---

### ServiceMetadata

```csharp
public class ServiceMetadata
{
    public int Priority { get; set; } = 100;
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? PluginId { get; set; }
}
```

**Purpose**: Metadata for service registration.

**Location**: `IRegistry.cs:68`

#### Properties

##### `int Priority { get; set; } = 100`

**Location**: `IRegistry.cs:74`

Priority for conflict resolution. Higher = preferred.

**Guidelines**:

- Framework services: 1000+
- Game plugins: 100-500
- UI plugins: 50-99
- Default: 100

##### `string? Name { get; set; }`

**Location**: `IRegistry.cs:79`

Optional name for multi-named services.

**Use case**: Multiple implementations with names (e.g., "TerminalUI", "SadConsole").

##### `string? Version { get; set; }`

**Location**: `IRegistry.cs:84`

Optional version for tracking service compatibility.

**Format**: Semantic version string (e.g., "1.0.0")

##### `string? PluginId { get; set; }`

**Location**: `IRegistry.cs:89`

Plugin ID that registered this service (set by registry automatically).

**Note**: Read-only from plugin perspective.

---

## Usage Patterns

### Pattern 1: Simple Service Registration

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    var service = new MyService(context.Logger);
    context.Registry.Register<IMyService>(service, priority: 100);
    return Task.CompletedTask;
}
```

### Pattern 2: Multi-Implementation Service

```csharp
// Plugin A
context.Registry.Register<IRenderer>(new TerminalRenderer(), priority: 100);

// Plugin B
context.Registry.Register<IRenderer>(new SadConsoleRenderer(), priority: 200);

// Host code
var renderer = registry.Get<IRenderer>(); // Gets SadConsoleRenderer (priority 200)
```

### Pattern 3: Service with Metadata

```csharp
context.Registry.Register<IUIProvider>(uiProvider, new ServiceMetadata
{
    Priority = 150,
    Name = "TerminalUI",
    Version = "2.0.0"
});
```

### Pattern 4: Conditional Service Consumption

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    if (context.Registry.IsRegistered<IInventoryService>())
    {
        _inventory = context.Registry.Get<IInventoryService>();
        _logger.LogInformation("Inventory service available");
    }
    else
    {
        _logger.LogWarning("Inventory service not available, using mock");
        _inventory = new MockInventoryService();
    }

    return Task.CompletedTask;
}
```

### Pattern 5: Multi-Service Pipeline

```csharp
var processors = context.Registry.GetAll<IEventProcessor>();
foreach (var processor in processors) // Ordered by priority
{
    await processor.ProcessAsync(evt);
}
```

---

## Assembly Load Context Isolation

The contracts assembly is **shared across ALC boundaries**, meaning:

1. **Type identity preserved**: `IPlugin` from host === `IPlugin` from plugin
2. **Interface matching works**: Plugins can implement interfaces without cast errors
3. **Service registration succeeds**: `Register<T>` uses runtime type matching

**Key files**:

- `PluginLoadContext.cs:26-29` - Shared assembly loading logic
- `PluginLoader.cs:157-261` - Plugin lifecycle implementation

**Shared assemblies**:

- `LablabBean.Plugins.Contracts`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection.Abstractions`

---

## Related Documentation

- **Quick-Start Guide**: `docs/_inbox/2025-10-21-plugin-development-quickstart--DOC-2025-00037.md`
- **Manifest Schema**: `docs/_inbox/2025-10-21-plugin-manifest-schema--DOC-2025-00039.md`
- **Architecture**: `specs/004-tiered-plugin-architecture/spec.md`
- **Examples**: `dotnet/examples/LablabBean.Plugin.Demo/`

---

**Version**: 1.0.0
**Created**: 2025-10-21
**Author**: Claude (Sonnet 4.5)
