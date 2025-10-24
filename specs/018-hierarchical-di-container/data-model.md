# Data Model: Hierarchical Dependency Injection Container System

**Feature**: 018-hierarchical-di-container
**Date**: 2025-10-24
**Phase**: 1 - Design & Contracts

## Overview

This document defines the core entities and their relationships for the hierarchical DI container system.

## Entity Model

### 1. HierarchicalServiceProvider

**Purpose**: Core container that implements IServiceProvider and manages parent-child relationships.

**Attributes**:

- `Name`: string (optional) - Identifier for diagnostics and scene management
- `Parent`: HierarchicalServiceProvider? - Reference to parent container (null for root)
- `Children`: IReadOnlyList<HierarchicalServiceProvider> - Child containers created from this one
- `InnerServiceProvider`: IServiceProvider - MSDI ServiceProvider instance for this level
- `IsDisposed`: bool - Tracks disposal state
- `Depth`: int - Level in hierarchy (0 = root, 1 = first child, etc.)

**Relationships**:

- Has zero or one parent (null for root container)
- Has zero or more children
- Owns its inner ServiceProvider instance
- Owns its children (disposes them when disposed)

**Lifecycle**:

```
Created → Active → Disposed
           ↓
      CreateChild() → New HierarchicalServiceProvider
```

**Invariants**:

- Once disposed, cannot create children
- Once disposed, GetService throws ObjectDisposedException
- Children must be disposed before or with parent
- Parent reference is immutable after construction

**State Transitions**:

```
State: Active
  - GetService() → returns service or null
  - CreateChild() → new child added to Children
  - Dispose() → State: Disposed

State: Disposed
  - GetService() → throws ObjectDisposedException
  - CreateChild() → throws ObjectDisposedException
  - Dispose() → no-op (idempotent)
```

### 2. ContainerRegistration

**Purpose**: Represents a single container instance registered with SceneContainerManager.

**Attributes**:

- `SceneName`: string - Unique identifier for the scene
- `Container`: HierarchicalServiceProvider - The actual container instance
- `ParentSceneName`: string? - Name of parent scene (null for global)
- `CreatedAt`: DateTime - When the container was created
- `ChildScenes`: List<string> - Names of child scenes

**Relationships**:

- References one HierarchicalServiceProvider
- Optionally references parent ContainerRegistration by scene name
- Tracks child ContainerRegistrations by scene name

**Lifecycle**:

- Created when scene loads
- Disposed when scene unloads
- Removed from SceneContainerManager on disposal

### 3. ServiceLifetimeManager

**Purpose**: Internal helper to manage service lifetime tracking (conceptual - implementation detail).

**Attributes**:

- `Singletons`: Dictionary<Type, object> - Cached singleton instances at this level
- `ScopedInstances`: Dictionary<Type, object> - Scoped instances for current scope
- `DisposableServices`: List<IDisposable> - Services to dispose with container

**Note**: This may be entirely handled by inner MSDI ServiceProvider. Included here for completeness but might not exist as separate class.

### 4. SceneContainerManager

**Purpose**: Registry and factory for scene-based containers.

**Attributes**:

- `GlobalContainer`: HierarchicalServiceProvider - Root container for global services
- `SceneContainers`: Dictionary<string, ContainerRegistration> - Map of scene name to container
- `IsInitialized`: bool - Whether global container has been initialized

**Relationships**:

- Owns the global container
- Tracks all scene containers
- Does not own scene containers (they can be disposed independently)

**Operations**:

- `InitializeGlobalContainer(IServiceCollection)` - Create root container
- `CreateSceneContainer(string sceneName, Action<IServiceCollection>, string? parentSceneName)` - Create child
- `GetSceneContainer(string sceneName)` - Retrieve by name
- `UnloadScene(string sceneName)` - Dispose and remove

### 5. ServiceDescriptor (MSDI)

**Purpose**: Represents a service registration (from Microsoft.Extensions.DependencyInjection).

**Attributes** (from MSDI):

- `ServiceType`: Type - The interface or class being registered
- `ImplementationType`: Type? - Concrete type (for type-based registration)
- `ImplementationFactory`: Func<IServiceProvider, object>? - Factory function
- `ImplementationInstance`: object? - Pre-created instance
- `Lifetime`: ServiceLifetime - Singleton, Scoped, or Transient

**Note**: This is from MSDI, not created by us. Included for completeness.

## Entity Relationship Diagram

```
┌─────────────────────────────────┐
│  SceneContainerManager          │
│  ─────────────────────────────  │
│  + GlobalContainer              │────┐
│  + SceneContainers              │    │
│  + IsInitialized                │    │
└─────────────────────────────────┘    │
         │                              │
         │ manages                      │ owns
         ▼                              ▼
┌─────────────────────────────────┐  ┌─────────────────────────────────┐
│  ContainerRegistration          │  │  HierarchicalServiceProvider    │
│  ─────────────────────────────  │  │  ──────────────────────────────  │
│  + SceneName                    │  │  + Name                         │
│  + Container                    │──┤  + Parent                       │◄──┐
│  + ParentSceneName              │  │  + Children                     │───┘
│  + CreatedAt                    │  │  + InnerServiceProvider         │
│  + ChildScenes                  │  │  + IsDisposed                   │
└─────────────────────────────────┘  │  + Depth                        │
                                      └─────────────────────────────────┘
                                                │
                                                │ delegates to
                                                ▼
                                      ┌─────────────────────────────────┐
                                      │  IServiceProvider (MSDI)        │
                                      │  ──────────────────────────────  │
                                      │  + GetService(Type)             │
                                      └─────────────────────────────────┘
                                                │
                                                │ resolves from
                                                ▼
                                      ┌─────────────────────────────────┐
                                      │  ServiceDescriptor (MSDI)       │
                                      │  ──────────────────────────────  │
                                      │  + ServiceType                  │
                                      │  + Implementation*              │
                                      │  + Lifetime                     │
                                      └─────────────────────────────────┘
```

## Hierarchy Example

**Scenario**: Game with Global services, Main Menu, and Dungeon with two floors

```
[Global Container]
├── Name: "Global"
├── Parent: null
├── Services: ISaveSystem, IAudioManager, IEventBus
└── Children:
    │
    ├── [Main Menu Container]
    │   ├── Name: "MainMenu"
    │   ├── Parent: → Global
    │   ├── Services: IMainMenuController, IProfileManager
    │   └── Children: []
    │
    └── [Dungeon Container]
        ├── Name: "Dungeon"
        ├── Parent: → Global
        ├── Services: IDungeonState, ILootSystem
        └── Children:
            │
            ├── [Floor 1 Container]
            │   ├── Name: "DungeonFloor1"
            │   ├── Parent: → Dungeon
            │   ├── Services: IFloorGenerator (Floor1Generator)
            │   └── Children: []
            │
            └── [Floor 2 Container]
                ├── Name: "DungeonFloor2"
                ├── Parent: → Dungeon
                ├── Services: IFloorGenerator (Floor2Generator)
                └── Children: []
```

**Service Resolution Examples**:

1. **Floor1 requests ISaveSystem**:
   - Check Floor1 services → not found
   - Check Dungeon services → not found
   - Check Global services → found ✓
   - Return ISaveSystem from Global

2. **Floor1 requests IDungeonState**:
   - Check Floor1 services → not found
   - Check Dungeon services → found ✓
   - Return IDungeonState from Dungeon

3. **Floor1 requests IFloorGenerator**:
   - Check Floor1 services → found ✓
   - Return Floor1Generator from Floor1

4. **Floor2 requests IFloorGenerator**:
   - Check Floor2 services → found ✓
   - Return Floor2Generator from Floor2
   - (Different instance than Floor1, no cross-sibling access)

## Validation Rules

### HierarchicalServiceProvider

1. **Name Validation** (if provided):
   - Must not be null or whitespace
   - Recommended: alphanumeric with hyphens/underscores
   - Example valid names: "Global", "MainMenu", "Dungeon-Floor-1"

2. **Parent Validation**:
   - Parent must not be disposed
   - Parent must not be a descendant (prevent circular references)
   - Depth must be < MaxDepth (default: 10, configurable)

3. **Disposal Validation**:
   - Cannot call GetService after disposal
   - Cannot create children after disposal
   - Dispose is idempotent (safe to call multiple times)

### SceneContainerManager

1. **Scene Name Uniqueness**:
   - Scene names must be unique within manager
   - Creating scene with existing name throws InvalidOperationException

2. **Parent Scene Existence**:
   - If parentSceneName provided, parent must exist in SceneContainers
   - Exception: global container is always valid parent

3. **Initialization Order**:
   - InitializeGlobalContainer must be called before CreateSceneContainer
   - Calling twice throws InvalidOperationException

## Performance Characteristics

| Operation | Time Complexity | Notes |
|-----------|----------------|-------|
| GetService (local hit) | O(1) | Delegates to MSDI ServiceProvider |
| GetService (parent hit) | O(d) | d = depth in hierarchy, typically d ≤ 3 |
| CreateChildContainer | O(1) | Adds to children list |
| Dispose | O(n) | n = total descendants (recursive) |
| SceneContainerManager.GetSceneContainer | O(1) | Dictionary lookup |

**Memory**:

- Base overhead per container: ~200-500 bytes (object, references, list)
- Service instance overhead: Managed by MSDI
- Typical game scenario: ~10-20 containers max, negligible memory impact

## Thread Safety Guarantees

| Operation | Thread Safety | Notes |
|-----------|--------------|-------|
| GetService | ✅ Thread-safe | Delegates to thread-safe MSDI ServiceProvider |
| CreateChildContainer | ❌ Not thread-safe | Must be called from scene loading thread |
| Dispose | ✅ Thread-safe | Uses lock to prevent concurrent disposal |
| SceneContainerManager operations | ✅ Thread-safe | Uses ConcurrentDictionary or locks |

## Extension Points

Future extensions may include:

1. **Diagnostics**:
   - `IEnumerable<Type> GetRegisteredServices()` - List all services at this level
   - `string GetHierarchyPath()` - "Global → Dungeon → Floor1"
   - `IServiceProviderIsService` - Check if service exists without resolving

2. **Events**:
   - `ContainerCreated` event
   - `ContainerDisposed` event
   - `ServiceResolved` event (for debugging/telemetry)

3. **Middleware**:
   - Service resolution interceptors
   - Lifetime policy customization

These are **out of scope for v1** but documented here for future consideration.
