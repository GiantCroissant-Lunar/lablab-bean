# LablabBean.Plugins.Core

## Overview

This library provides the core plugin system infrastructure, including:
- Plugin loading and lifecycle management
- Service registry and dependency injection integration
- Plugin discovery and dependency resolution
- Health monitoring and metrics

## Platform-Agnostic Architecture

The plugin system is designed to support multiple platforms through the `IPluginLoader` abstraction.

### Current Implementation

**AssemblyLoadContext (ALC) Loader**
- Implementation: `PluginLoader` class
- Platform: .NET 5+ (Windows, Linux, macOS)
- Features:
  - Plugin isolation via separate AssemblyLoadContexts
  - Hot reload support (collectible contexts)
  - Dependency resolution
  - Shared contract assemblies to avoid ALC boundary issues

### Factory Pattern

Use `PluginLoaderFactory` to create platform-appropriate loaders:

```csharp
var loader = PluginLoaderFactory.Create(
    logger,
    loggerFactory,
    configuration,
    services,
    pluginRegistry,
    serviceRegistry,
    enableHotReload: true,
    profile: "dotnet.console");

await loader.DiscoverAndLoadAsync(pluginPaths);
```

### Future Platform Support

The `IPluginLoader` interface enables support for additional platforms:

**HybridCLR (Unity)**
- Future implementation could load IL2CPP-compatible plugins
- Would implement `IPluginLoader` with Unity-specific loading

**WebAssembly**
- Future implementation for browser-based plugins
- Would implement `IPluginLoader` with WASM module loading

## Plugin Lifecycle

Plugins follow this lifecycle:

1. **Created** - Plugin manifest loaded, dependencies resolved
2. **Initialized** - `IPlugin.InitializeAsync()` called, services registered
3. **Started** - `IPlugin.StartAsync()` called, plugin begins operation
4. **Stopped** - `IPlugin.StopAsync()` called, cleanup initiated
5. **Failed** - Plugin encountered an error during any phase

## Architecture

### Key Components

- **IPluginLoader** - Platform-agnostic loader abstraction
- **PluginLoader** - ALC-based implementation for .NET
- **PluginLoadContext** - Custom AssemblyLoadContext for plugin isolation
- **PluginRegistry** - Tracks all discovered and loaded plugins
- **ServiceRegistry** - Plugin service registration and discovery
- **DependencyResolver** - Resolves plugin dependencies and load order
- **PluginHost** - Host services available to plugins

### Shared Assemblies

These assemblies are shared between host and plugins to avoid ALC boundary issues:

- `LablabBean.Plugins.Contracts` - Core plugin interfaces
- `Microsoft.Extensions.*` - DI, logging, configuration abstractions

## Usage

See the console application for a complete example:
- `dotnet/console-app/LablabBean.Console/`

Basic usage:

```csharp
// Setup registries
var pluginRegistry = new PluginRegistry();
var serviceRegistry = new ServiceRegistry(eventBus);

// Create loader
var loader = PluginLoaderFactory.Create(
    logger, loggerFactory, configuration, services,
    pluginRegistry, serviceRegistry);

// Load plugins
var pluginPaths = new[] { "./plugins" };
var count = await loader.DiscoverAndLoadAsync(pluginPaths);

// Use registered services
var service = serviceRegistry.Get<IMyService>("my-service");

// Cleanup
await loader.UnloadAllAsync();
```

## Extending with New Platforms

To add support for a new platform:

1. Create a new implementation of `IPluginLoader`
2. Implement platform-specific assembly loading
3. Ensure plugin lifecycle methods are called correctly
4. Update `PluginLoaderFactory` to detect and create your loader
5. Document platform-specific considerations

Example structure:

```csharp
public class HybridClrPluginLoader : IPluginLoader
{
    public IPluginRegistry PluginRegistry { get; }
    public IRegistry ServiceRegistry { get; }
    
    public Task<int> DiscoverAndLoadAsync(
        IEnumerable<string> pluginPaths, 
        CancellationToken ct = default)
    {
        // Platform-specific loading logic
    }
    
    public Task UnloadPluginAsync(string pluginId, CancellationToken ct = default)
    {
        // Platform-specific unload logic
    }
    
    public Task UnloadAllAsync(CancellationToken ct = default)
    {
        // Platform-specific cleanup
    }
}
```

## Testing

The plugin system includes comprehensive tests:

- Plugin lifecycle tests
- Service registration/resolution tests
- Dependency resolution tests
- Hot reload tests (when enabled)

Run tests:

```bash
dotnet test
```

## See Also

- [Plugin Contracts](../LablabBean.Plugins.Contracts/README.md)
- [Example Plugins](../../plugins/)
- [SPEC-011](../../../specs/011-dotnet-naming-architecture-adjustment/spec.md) - Architecture specification
