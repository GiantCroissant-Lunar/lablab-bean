# Quick Start: Hierarchical Dependency Injection Container

**Feature**: 018-hierarchical-di-container
**Audience**: Game developers using LablabBean framework

## Overview

This guide shows how to use the hierarchical DI container system for multi-scene game development. You'll learn how to set up global services, create scene-specific containers, and manage service lifetimes across scene transitions.

## Prerequisites

- .NET 8 SDK installed
- LablabBean.DependencyInjection package referenced in your project
- Basic understanding of dependency injection concepts

## Installation

Add the package reference to your project:

```xml
<PackageReference Include="LablabBean.DependencyInjection" Version="1.0.0" />
```

Or using the .NET CLI:

```bash
dotnet add package LablabBean.DependencyInjection
```

## Basic Usage

### 1. Create a Global Container

The global container holds services that persist across all scenes (save system, audio, events, etc.):

```csharp
using LablabBean.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Create service collection
var services = new ServiceCollection();

// Register global services
services.AddSingleton<ISaveSystem, SaveSystem>();
services.AddSingleton<IAudioManager, AudioManager>();
services.AddSingleton<IEventBus, EventBus>();

// Build hierarchical service provider
var globalContainer = services.BuildHierarchicalServiceProvider("Global");
```

### 2. Create Scene Containers

Scene containers hold services specific to a scene, while still accessing global services:

```csharp
// Main Menu scene
var mainMenuContainer = globalContainer.CreateChildContainer(services =>
{
    services.AddSingleton<IMainMenuController, MainMenuController>();
    services.AddSingleton<IProfileManager, ProfileManager>();
}, "MainMenu");

// Main menu services can access global services
var saveSystem = mainMenuContainer.GetService<ISaveSystem>(); // From global ✓
var menuController = mainMenuContainer.GetService<IMainMenuController>(); // From scene ✓
```

### 3. Multi-Level Hierarchies

Create nested hierarchies for complex scenes like dungeons with multiple floors:

```csharp
// Dungeon scene (child of global)
var dungeonContainer = globalContainer.CreateChildContainer(services =>
{
    services.AddSingleton<IDungeonState, DungeonState>();
    services.AddScoped<ICombatSystem, CombatSystem>();
}, "Dungeon");

// Floor 1 (child of dungeon)
var floor1Container = dungeonContainer.CreateChildContainer(services =>
{
    services.AddScoped<IFloorGenerator, Floor1Generator>();
}, "DungeonFloor1");

// Floor 1 can access:
// - Global services (ISaveSystem, IAudioManager)
// - Dungeon services (IDungeonState, ICombatSystem)
// - Floor-specific services (Floor1Generator)
var dungeonState = floor1Container.GetService<IDungeonState>(); // From parent ✓
var floorGen = floor1Container.GetService<IFloorGenerator>(); // From self ✓
```

### 4. Clean Up on Scene Unload

Dispose containers when scenes unload to free resources:

```csharp
// Unload main menu
mainMenuContainer.Dispose(); // Also disposes any child containers

// Unload dungeon (automatically disposes floor1 and floor2)
dungeonContainer.Dispose();
```

## Using SceneContainerManager

For larger games, use `SceneContainerManager` to centrally manage scene containers:

### Bootstrap Setup

```csharp
using LablabBean.DependencyInjection;

public class GameBootstrap
{
    private static SceneContainerManager? _containerManager;

    public static void Initialize()
    {
        _containerManager = new SceneContainerManager();

        // Initialize global container
        _containerManager.InitializeGlobalContainer(services =>
        {
            services.AddSingleton<ISaveSystem, SaveSystem>();
            services.AddSingleton<IAudioManager, AudioManager>();
            services.AddSingleton<IEventBus, EventBus>();
        });
    }

    public static ISceneContainerManager GetContainerManager()
    {
        if (_containerManager == null)
            throw new InvalidOperationException("GameBootstrap not initialized");

        return _containerManager;
    }
}
```

### Scene Loading

```csharp
public class SceneLoader
{
    private readonly ISceneContainerManager _containerManager;

    public SceneLoader()
    {
        _containerManager = GameBootstrap.GetContainerManager();
    }

    public void LoadMainMenu()
    {
        // Create main menu container
        var container = _containerManager.CreateSceneContainer("MainMenu", services =>
        {
            services.AddSingleton<IMainMenuController, MainMenuController>();
            services.AddSingleton<IProfileManager, ProfileManager>();
        });

        // Use container for scene services
        var menuController = container.GetService<IMainMenuController>();
        menuController?.Initialize();
    }

    public void LoadDungeon()
    {
        // Create dungeon container
        var dungeonContainer = _containerManager.CreateSceneContainer("Dungeon", services =>
        {
            services.AddSingleton<IDungeonState, DungeonState>();
            services.AddScoped<ICombatSystem, CombatSystem>();
        });

        // Create first floor as child of dungeon
        var floor1Container = _containerManager.CreateSceneContainer("DungeonFloor1",
            services =>
            {
                services.AddScoped<IFloorGenerator, Floor1Generator>();
            },
            parentSceneName: "Dungeon"); // Explicit parent

        // Initialize dungeon
        var dungeonState = dungeonContainer.GetService<IDungeonState>();
        dungeonState?.Initialize();
    }

    public void UnloadScene(string sceneName)
    {
        // Dispose and remove from manager
        if (_containerManager.UnloadScene(sceneName))
        {
            Console.WriteLine($"Scene '{sceneName}' unloaded successfully");
        }
    }
}
```

### Scene Transition

```csharp
public class SceneTransitionService
{
    private readonly ISceneContainerManager _containerManager;

    public SceneTransitionService(ISceneContainerManager containerManager)
    {
        _containerManager = containerManager;
    }

    public async Task TransitionAsync(string fromScene, string toScene)
    {
        // Unload current scene
        _containerManager.UnloadScene(fromScene);

        // Wait for cleanup (if needed)
        await Task.Delay(100);

        // Load new scene (implementation depends on your game)
        LoadScene(toScene);
    }

    private void LoadScene(string sceneName)
    {
        // Scene-specific loading logic
        switch (sceneName)
        {
            case "MainMenu":
                LoadMainMenu();
                break;
            case "Dungeon":
                LoadDungeon();
                break;
            default:
                throw new ArgumentException($"Unknown scene: {sceneName}");
        }
    }

    private void LoadMainMenu()
    {
        var container = _containerManager.CreateSceneContainer("MainMenu", services =>
        {
            services.AddSingleton<IMainMenuController, MainMenuController>();
        });
    }

    private void LoadDungeon()
    {
        var container = _containerManager.CreateSceneContainer("Dungeon", services =>
        {
            services.AddSingleton<IDungeonState, DungeonState>();
        });
    }
}
```

## Service Lifetime Best Practices

### Singleton

Use for services that live for the entire application/scene lifetime:

```csharp
// Global singletons (persist across all scenes)
globalServices.AddSingleton<ISaveSystem, SaveSystem>();
globalServices.AddSingleton<IAudioManager, AudioManager>();

// Scene singletons (persist while scene is loaded)
sceneServices.AddSingleton<IDungeonState, DungeonState>();
```

### Scoped

Use for services that need a fresh instance per operation/request:

```csharp
// Combat session scope
dungeonServices.AddScoped<ICombatSystem, CombatSystem>();

// Floor generation scope
floorServices.AddScoped<IFloorGenerator, FloorGenerator>();
```

**Note**: Create scopes explicitly for scoped services:

```csharp
using (var scope = container.CreateScope())
{
    var combat = scope.ServiceProvider.GetService<ICombatSystem>();
    combat?.StartCombat();
} // ICombatSystem disposed here
```

### Transient

Use for lightweight, stateless services:

```csharp
// Factory services
services.AddTransient<IItemFactory, ItemFactory>();

// Validators
services.AddTransient<IInputValidator, InputValidator>();
```

## Common Patterns

### Pattern 1: Global Services

```csharp
// Services that ALL scenes need
globalContainer:
├── ISaveSystem (persists game state)
├── IAudioManager (plays sounds)
├── IEventBus (cross-scene events)
└── IInputManager (handles input)
```

### Pattern 2: Scene Hierarchy

```csharp
// Multi-level game structure
Global
├── MainMenu (ephemeral UI services)
└── Dungeon (persistent during dungeon run)
    ├── Floor1 (floor-specific enemies, loot)
    ├── Floor2 (floor-specific enemies, loot)
    └── Floor3 (floor-specific enemies, loot)
```

### Pattern 3: Service Shadowing (Override)

Child containers can override parent services:

```csharp
// Global: Default combat system
globalServices.AddSingleton<ICombatSystem, BasicCombatSystem>();

// Dungeon: Override with advanced combat
dungeonServices.AddSingleton<ICombatSystem, AdvancedCombatSystem>();

// Floor 1 will use AdvancedCombatSystem (from dungeon)
// Not BasicCombatSystem (from global) - closer parent wins
```

## Troubleshooting

### Service Not Found

**Problem**: `GetService<T>()` returns null

**Solution**: Check the hierarchy path

```csharp
// Use GetHierarchyPath() for diagnostics
Console.WriteLine(floor1Container.GetHierarchyPath());
// Output: "Global → Dungeon → DungeonFloor1"

// Check if service is registered in any parent
var saveSystem = floor1Container.GetService<ISaveSystem>();
if (saveSystem == null)
{
    Console.WriteLine("ISaveSystem not found in hierarchy");
    // Register in global or parent container
}
```

### ObjectDisposedException

**Problem**: `GetService()` throws `ObjectDisposedException`

**Solution**: Don't access disposed containers

```csharp
// BAD: Accessing container after disposal
dungeonContainer.Dispose();
var state = dungeonContainer.GetService<IDungeonState>(); // ❌ Throws!

// GOOD: Check IsDisposed before access
if (!dungeonContainer.IsDisposed)
{
    var state = dungeonContainer.GetService<IDungeonState>(); // ✓
}
```

### Memory Leaks

**Problem**: Containers not being disposed

**Solution**: Always dispose scene containers on unload

```csharp
// Use using statement for auto-disposal
using (var tempContainer = parentContainer.CreateChildContainer(s => { }))
{
    // Use container
} // Automatically disposed

// Or dispose explicitly
var container = CreateContainer();
try
{
    // Use container
}
finally
{
    container.Dispose(); // Guaranteed disposal
}
```

## Performance Tips

1. **Minimize hierarchy depth**: Keep hierarchies shallow (< 5 levels) for faster resolution
2. **Cache frequently-used services**: Store references instead of calling `GetService()` repeatedly
3. **Use singletons wisely**: Singletons are faster than scoped/transient but live longer
4. **Dispose promptly**: Dispose containers as soon as scenes unload to free memory

## Next Steps

- Read the [Data Model](./data-model.md) for architecture details
- Review [Research](./research.md) for design decisions
- See [Contracts](./contracts/) for full API reference
- Check [Tasks](./tasks.md) for implementation roadmap (after `/speckit.tasks`)

## Support

- GitHub Issues: [lablab-bean/issues](https://github.com/your-org/lablab-bean/issues)
- Documentation: [docs/](../../docs/)
- Examples: [dotnet/examples/](../../dotnet/examples/)
