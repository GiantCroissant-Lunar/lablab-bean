# Feature Specification: Hierarchical Dependency Injection Container System

**Feature Branch**: `018-hierarchical-di-container`
**Created**: 2025-10-24
**Status**: Draft
**Input**: User description: "Create a hierarchical dependency injection container system with parent-child relationships, compatible with Microsoft.Extensions.DependencyInjection interface, supporting multi-scene game development patterns where child containers can access parent container services"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Global Services Accessible Across All Scenes (Priority: P1)

As a game developer, I want to register core services (like save system, audio manager, event bus) in a global container so that all game scenes can access these shared services without duplication.

**Why this priority**: This is the foundation of the hierarchical DI system. Without global services, there's no parent container to build upon, making all other features impossible.

**Independent Test**: Can be fully tested by registering a save system in the global container, loading any scene, and verifying that scene can resolve and use the save system service. Delivers immediate value by establishing centralized service management.

**Acceptance Scenarios**:

1. **Given** the game has initialized, **When** I register a save system service in the global container, **Then** the save system is available as a singleton instance
2. **Given** the global container has registered services, **When** any scene requests a global service, **Then** the same instance is provided across all scenes
3. **Given** multiple scenes are loaded, **When** each scene accesses the audio manager from the global container, **Then** all scenes share the same audio manager instance

---

### User Story 2 - Scene-Specific Services with Parent Access (Priority: P2)

As a game developer, I want to create scene-specific containers that can register local services while still accessing global services, so that each scene can manage its own dependencies without polluting the global scope.

**Why this priority**: This enables proper service scoping and isolation between scenes, which is critical for multi-scene games where scenes load/unload dynamically.

**Independent Test**: Can be tested by creating a dungeon scene container with a dungeon-specific combat system, verifying the combat system can access global services (event bus) while remaining isolated from other scenes. Delivers value by enabling clean scene boundaries.

**Acceptance Scenarios**:

1. **Given** a global container with a save system, **When** I create a dungeon scene container with a combat system, **Then** the combat system can resolve both itself and the global save system
2. **Given** a scene container with scene-specific services, **When** I request a service not registered in the scene, **Then** the container checks the parent (global) container automatically
3. **Given** two separate scene containers (MainMenu and Dungeon), **When** each registers services with the same interface but different implementations, **Then** each scene receives its own implementation without conflicts

---

### User Story 3 - Multi-Level Hierarchies for Complex Scenes (Priority: P3)

As a game developer, I want to create multi-level container hierarchies (e.g., Global → Dungeon → DungeonFloor1) so that I can organize services at different granularity levels and unload entire sub-hierarchies when transitioning between game areas.

**Why this priority**: This addresses advanced multi-scene scenarios like procedural dungeons with multiple floors, where each floor needs its own services but shares dungeon-wide state.

**Independent Test**: Can be tested by creating a Global → Dungeon → Floor1 hierarchy, verifying Floor1 can access both Dungeon and Global services, and confirming that unloading Dungeon disposes both Dungeon and Floor1 containers. Delivers value for complex scene management.

**Acceptance Scenarios**:

1. **Given** a Global → Dungeon → Floor1 hierarchy, **When** Floor1 requests a service, **Then** the container searches Floor1 → Dungeon → Global in order
2. **Given** a dungeon with multiple floor containers, **When** I unload the dungeon container, **Then** all child floor containers are automatically disposed
3. **Given** Floor1 and Floor2 as siblings under Dungeon, **When** Floor1 requests a Floor2-specific service, **Then** the request fails (no cross-sibling access)

---

### User Story 4 - MSDI Compatibility for Existing Code (Priority: P2)

As a game developer using existing .NET libraries that depend on Microsoft.Extensions.DependencyInjection, I want the hierarchical container to implement standard MSDI interfaces so that I can integrate third-party libraries without modification.

**Why this priority**: This ensures the custom DI system doesn't create a walled garden and allows leveraging the .NET ecosystem.

**Independent Test**: Can be tested by registering services using IServiceCollection, building the container, and resolving services through IServiceProvider. Delivers value by ensuring compatibility with existing .NET code patterns.

**Acceptance Scenarios**:

1. **Given** an IServiceCollection with registered services, **When** I build a hierarchical service provider, **Then** the provider implements IServiceProvider and resolves services correctly
2. **Given** a third-party library expecting IServiceProvider, **When** I pass the hierarchical container, **Then** the library functions normally without knowing about the hierarchy
3. **Given** service registrations with Singleton, Scoped, and Transient lifetimes, **When** I resolve services from the hierarchical container, **Then** lifetimes are respected according to MSDI semantics

---

### Edge Cases

- What happens when a child container requests a service that exists in multiple parent levels (e.g., both Dungeon and Global register ICombatSystem)? The closest parent wins (Dungeon's implementation is returned).
- How does the system handle circular dependencies across container boundaries (e.g., Global service depends on Scene service)? This is prevented - child containers can only depend on parent services, not vice versa.
- What happens when a scene container is disposed while child containers still exist? All child containers are automatically disposed in a cascading fashion.
- How are scoped services handled across container boundaries? Scoped services are scoped to their container level - a scoped service in Global is different from a scoped service in Scene.
- What happens when requesting IServiceProvider from within the container? The container returns itself (the current container level's IServiceProvider).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST implement IServiceProvider interface for compatibility with Microsoft.Extensions.DependencyInjection
- **FR-002**: System MUST support creating child containers from a parent container
- **FR-003**: Child containers MUST be able to resolve services from their parent container when not found locally
- **FR-004**: System MUST support multi-level hierarchies (parent → child → grandchild → etc.)
- **FR-005**: Parent containers MUST NOT be able to access child container services (one-way dependency)
- **FR-006**: System MUST automatically dispose child containers when parent is disposed
- **FR-007**: System MUST support all three MSDI service lifetimes: Singleton, Scoped, and Transient
- **FR-008**: Singleton services registered in a parent MUST be shared across all child containers
- **FR-009**: Scoped services MUST be scoped to their container level, not shared across hierarchy
- **FR-010**: System MUST allow registering services via IServiceCollection
- **FR-011**: System MUST support factory-based service registration (descriptor.ImplementationFactory)
- **FR-012**: System MUST support instance-based service registration (descriptor.ImplementationInstance)
- **FR-013**: System MUST support type-based service registration (descriptor.ImplementationType)
- **FR-014**: System MUST support open generic type registrations (e.g., IRepository<>)
- **FR-015**: Child containers MUST be independently disposable without affecting parent or siblings
- **FR-016**: System MUST provide a mechanism to identify and retrieve containers by name/identifier for scene management
- **FR-017**: System MUST support creating child containers with explicit parent reference
- **FR-018**: Service resolution MUST follow a clear resolution order: current container → parent → grandparent → etc.
- **FR-019**: System MUST prevent duplicate service disposal when hierarchies overlap
- **FR-020**: System MUST provide clear error messages when services cannot be resolved at any hierarchy level

### Key Entities

- **HierarchicalServiceProvider**: The container that implements IServiceProvider and maintains parent-child relationships. Contains references to parent container and collection of child containers. Handles service resolution with fallback to parent.

- **SceneContainerManager**: Manages container lifecycle across game scenes. Tracks containers by scene name, handles scene loading/unloading, and maintains the container hierarchy structure.

- **ServiceDescriptor**: Represents a service registration (from MSDI). Contains service type, implementation type/factory/instance, and lifetime information.

- **ContainerRegistry**: Maps scene names to their corresponding containers. Enables lookup of containers for scene-based operations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can create a global container and access its services from any child container without additional configuration
- **SC-002**: Service resolution through a 3-level hierarchy (Global → Dungeon → Floor) completes without performance degradation compared to flat resolution
- **SC-003**: Child containers can be created and disposed 1000+ times during a game session without memory leaks
- **SC-004**: Existing code using IServiceProvider works without modification when passed a hierarchical container
- **SC-005**: Scene transitions that dispose old containers and create new ones complete within acceptable frame time budgets (< 16ms for 60fps)
- **SC-006**: 100% of MSDI service lifetimes (Singleton, Scoped, Transient) behave identically to standard Microsoft.Extensions.DependencyInjection behavior
- **SC-007**: Developers can successfully integrate third-party libraries expecting IServiceProvider without custom adapter code
- **SC-008**: Service registration using IServiceCollection syntax requires zero learning curve for developers familiar with MSDI

## Assumptions *(optional)*

- The game uses a scene-based architecture where scenes load and unload dynamically
- Services are primarily registered at startup or scene initialization, not frequently during gameplay
- The majority of services follow a parent-child access pattern (child accesses parent) rather than complex cross-cutting concerns
- Developers are familiar with Microsoft.Extensions.DependencyInjection patterns and conventions
- The system will be used primarily with .NET 8 and later versions
- Performance requirements align with typical game loop constraints (60fps target)
- Scene containers are relatively short-lived compared to the global container
- The primary use case is multi-scene Unity-style development patterns adapted to .NET

## Dependencies *(optional)*

- Microsoft.Extensions.DependencyInjection.Abstractions package (for IServiceProvider, IServiceCollection interfaces)
- Existing game framework must support scene lifecycle events (load, unload)
- Game architecture must support dependency injection pattern (constructor or property injection)

## Out of Scope *(optional)*

- Integration with specific game engines (Unity, Godot, etc.) - this feature provides the DI abstraction only
- Compile-time DI optimization (Pure.DI integration) - this is runtime-based initially
- Advanced DI features like interceptors, decorators, or AOP (aspect-oriented programming)
- GUI tools for visualizing container hierarchies
- Automatic service discovery or assembly scanning
- Property injection or method injection - only constructor injection is supported
- Validation of service registrations at build time
- Circular dependency detection and resolution
- Named service registrations (multiple implementations of same interface)
- Conditional service registration based on runtime state
