# Data Model: Tiered Plugin Architecture

## Interfaces & Models (Contracts)

### Core Plugin Contract (Updated)

```csharp
namespace LablabBean.Plugins.Contracts;

public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    // Initialize with runtime context (isolates ALC boundary)
    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);

    // Called after host is built, to start async work
    Task StartAsync(CancellationToken ct = default);

    // Called before unload/shutdown
    Task StopAsync(CancellationToken ct = default);
}

public interface IPluginContext
{
    IRegistry Registry { get; }           // Cross-ALC service registration
    IConfiguration Configuration { get; } // Host configuration
    ILogger Logger { get; }               // Logger for this plugin
    IPluginHost Host { get; }             // Host services
}

public interface IPluginHost
{
    ILogger CreateLogger(string categoryName);
    IServiceProvider Services { get; }
    void PublishEvent<T>(T evt);
}

public interface IRegistry
{
    void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class;
    void Register<TService>(TService implementation, int priority = 100) where TService : class;

    TService Get<TService>(SelectionMode mode = SelectionMode.HighestPriority) where TService : class;
    IEnumerable<TService> GetAll<TService>() where TService : class;

    bool IsRegistered<TService>() where TService : class;
    bool Unregister<TService>(TService implementation) where TService : class;
}

public enum SelectionMode { One, HighestPriority, All }

public class ServiceMetadata
{
    public int Priority { get; set; } = 100;
    public string? Name { get; set; }
    public string? Version { get; set; }
}

public interface IPluginRegistry
{
    IReadOnlyCollection<PluginDescriptor> GetAll();
    PluginDescriptor? GetById(string id);
}

public enum PluginState { Created, Initialized, Started, Failed, Stopped, Unloaded }

public sealed class PluginDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required PluginState State { get; set; }
    public required PluginManifest Manifest { get; init; }
    public string? FailureReason { get; set; }
}

public sealed class PluginManifest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public string? EntryAssembly { get; init; } // e.g., Plugin DLL name
    public string? EntryType { get; init; }     // e.g., FullName of IPlugin
    public List<PluginDependency> Dependencies { get; init; } = new();
}

public sealed class PluginDependency
{
    public required string Id { get; init; }
    public string? VersionRange { get; init; } // e.g., ">=1.0.0 <2.0.0"
    public bool Optional { get; init; }
}
```

## Manifest JSON Schema (MVP)

```json
{
  "id": "lablab.dungeongame",
  "name": "Dungeon Game",
  "version": "0.1.0",
  "entryAssembly": "LablabBean.Plugins.DungeonGame.dll",
  "entryType": "LablabBean.Plugins.DungeonGame.DungeonGamePlugin",
  "dependencies": [
    { "id": "lablab.arch", "versionRange": ">=0.1.0", "optional": false }
  ]
}
```

## Lifecycle & State Machine

```
Create → Initialize → Start → (Stop → Unload) → (Reload path re-enters at Create)
          │            └── Failure → Failed (isolated; others continue)
```

## Dependency Resolution

- Build graph from manifests; validate no cycles.
- Respect hard/soft deps; topo-sort load/start order.

## Observability

- Logs: lifecycle, dependency outcomes, load time.
- Metrics: plugin_load_seconds (histogram), plugin_failures_total, plugin_reload_total.
