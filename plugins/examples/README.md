# Example Plugins

This directory contains example plugin implementations demonstrating the event-driven architecture patterns.

## Available Examples

### 1. Analytics Plugin
**Location**: `../LablabBean.Plugins.Analytics/`  
**Pattern**: Event Subscriber  
**Purpose**: Track game events without direct game plugin dependency

**Features**:
- Subscribes to `EntitySpawnedEvent`, `EntityMovedEvent`, `CombatEvent`
- Tracks counts and logs analytics
- Zero direct dependencies on game plugin

**Use Case**: Demonstrates loose coupling via events

### 2. Mock Game Service
**Location**: `../LablabBean.Plugins.MockGame/`  
**Pattern**: Service Provider + Event Publisher  
**Purpose**: Provide game service implementation with event publishing

**Features**:
- Implements `IService` contract
- Publishes events on game actions
- Priority-based registration (200)

**Use Case**: Demonstrates service contracts and event publishing

### 3. Reactive UI Service
**Location**: `../LablabBean.Plugins.ReactiveUI/`  
**Pattern**: Event Subscriber + Service Provider  
**Purpose**: Reactive UI that updates automatically on game events

**Features**:
- Subscribes to 4 game events
- Implements `IService` contract
- Marks display for redraw on events
- No polling required

**Use Case**: Demonstrates full reactive UI pattern

## Running Examples

### Build All Examples

```bash
dotnet build plugins/LablabBean.Plugins.Analytics
dotnet build plugins/LablabBean.Plugins.MockGame
dotnet build plugins/LablabBean.Plugins.ReactiveUI
```

### Test Examples

```bash
# Run integration tests that use these examples
dotnet test dotnet/tests/LablabBean.Plugins.Core.Tests
```

## Creating Your Own Plugin

See the [event-driven-development.md](../../docs/plugins/event-driven-development.md) guide for a step-by-step tutorial.

### Quick Template

```csharp
using LablabBean.Contracts.Game.Events;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

public class MyPlugin : IPlugin
{
    public string Id => "my-plugin-id";
    public string Name => "My Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        var eventBus = context.Registry.Get<IEventBus>();
        
        // Subscribe to events
        eventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
        
        context.Logger.LogInformation("Plugin initialized");
        return Task.CompletedTask;
    }

    private Task OnEntitySpawned(EntitySpawnedEvent evt)
    {
        // Handle event
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
```

## Performance

All example plugins have been tested and validated:
- **Event Latency**: 0.003ms average
- **Throughput**: 1.1M+ events/second
- **Memory**: ~239 bytes per event

See [performance-results.md](../../specs/007-tiered-contract-architecture/performance-results.md) for details.

## Documentation

- **Developer Guide**: [event-driven-development.md](../../docs/plugins/event-driven-development.md)
- **Quickstart**: [quickstart.md](../../specs/007-tiered-contract-architecture/quickstart.md)
- **Data Model**: [data-model.md](../../specs/007-tiered-contract-architecture/data-model.md)
- **Spec**: [spec.md](../../specs/007-tiered-contract-architecture/spec.md)
