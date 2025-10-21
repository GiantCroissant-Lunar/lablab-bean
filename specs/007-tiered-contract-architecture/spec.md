# Feature Specification: Tiered Contract Architecture

**Feature Branch**: `007-tiered-contract-architecture`
**Created**: 2025-10-21
**Status**: Draft (Expanded)
**Input**: User description: "Adopt cross-milo tier 1 and tier 2 contract design patterns: Add IEventBus foundation, create domain-specific contract assemblies (Game, UI, Scene, Input, Config, Resource), implement event-driven communication, and establish comprehensive platform-independent plugin architecture"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plugin Developer Creates Analytics Plugin Using Events (Priority: P1)

A plugin developer wants to create an analytics plugin that tracks game events (entity spawns, combat, level ups) without directly depending on the game plugin. They need to subscribe to game events and react to them asynchronously.

**Why this priority**: This is the foundation of event-driven architecture. Without the event bus, plugins cannot communicate loosely. This delivers immediate value by enabling the first cross-plugin integration pattern.

**Independent Test**: Can be fully tested by creating a simple event publisher in one plugin, a subscriber in another plugin, and verifying the event is received without any direct dependency between the plugins.

**Acceptance Scenarios**:

1. **Given** the event bus is registered in the plugin host, **When** a plugin publishes an event using `PublishAsync<T>()`, **Then** all subscribers to that event type receive the event asynchronously
2. **Given** an analytics plugin subscribes to `EntitySpawnedEvent`, **When** the game plugin spawns an entity and publishes the event, **Then** the analytics plugin's handler is invoked with the event data
3. **Given** multiple plugins subscribe to the same event type, **When** an event is published, **Then** all subscribers receive the event without errors from one subscriber affecting others

---

### User Story 2 - Plugin Developer Defines Game Service Contract (Priority: P2)

A plugin developer wants to create a game service that implements dungeon crawler mechanics using well-defined contracts. They need to define the service interface in a contract assembly separate from the implementation, allowing multiple platform-specific implementations.

**Why this priority**: Domain contracts establish the "what" without the "how", enabling platform independence. This is essential for supporting multiple UI frameworks (Terminal.Gui, SadConsole, Unity).

**Independent Test**: Can be fully tested by creating the `LablabBean.Contracts.Game` assembly with service interfaces, then implementing a simple mock service and registering it via `IRegistry`, and verifying it can be retrieved by other plugins.

**Acceptance Scenarios**:

1. **Given** a `LablabBean.Contracts.Game` assembly exists with `IService` interface, **When** a plugin implements the interface and registers it via `IRegistry`, **Then** other plugins can retrieve the service using `Get<IService>()`
2. **Given** multiple implementations of `IService` are registered with different priorities, **When** a plugin requests the service using `SelectionMode.HighestPriority`, **Then** the implementation with the highest priority is returned
3. **Given** a game service interface defines methods like `ProcessTurnAsync()`, **When** a plugin calls the method, **Then** the concrete implementation executes without the caller knowing the platform-specific details

---

### User Story 3 - Plugin Developer Creates UI Plugin with Events (Priority: P2)

A plugin developer wants to create a UI rendering plugin that automatically updates when game state changes. They need to subscribe to game events (entity moved, combat occurred) and update the display without polling for changes.

**Why this priority**: This demonstrates the full event-driven pattern in action - loose coupling between game logic and UI rendering. This is critical for supporting multiple UI frameworks on the same game core.

**Independent Test**: Can be fully tested by publishing game state change events from a mock game service, having the UI plugin subscribe to those events, and verifying the UI updates are triggered without the UI polling the game state.

**Acceptance Scenarios**:

1. **Given** a UI plugin subscribes to `EntityMovedEvent`, **When** the game service moves an entity and publishes the event, **Then** the UI plugin's display updates to reflect the new position
2. **Given** a UI plugin subscribes to multiple game events, **When** events are published in rapid succession, **Then** the UI handles all events asynchronously without blocking the game loop
3. **Given** the UI plugin is unloaded, **When** game events are published, **Then** the system handles the missing subscriber gracefully without errors

---

### User Story 4 - Plugin Developer Creates Scene Management Contract (Priority: P1)

A plugin developer wants to create a scene/level management system with contracts for loading dungeons, managing cameras, and handling transitions. They need domain-specific contracts for scene operations that work across different rendering engines.

**Why this priority**: Scene management is critical for dungeon crawlers - it handles level loading, camera control, viewport management, and world rendering coordination. Without scene contracts, the game cannot properly manage dungeons or coordinate rendering across different UI frameworks.

**Independent Test**: Can be fully tested by creating the `LablabBean.Contracts.Scene` assembly, implementing a basic scene loader, and verifying scene load/unload operations work through the contract interface without depending on specific rendering implementations.

**Acceptance Scenarios**:

1. **Given** a `LablabBean.Contracts.Scene` assembly with `IService` interface, **When** a plugin loads a scene via `LoadSceneAsync()`, **Then** the scene data is loaded and a `SceneLoadedEvent` is published
2. **Given** multiple scene implementations (procedural dungeon, static level), **When** the game requests scene loading via the contract, **Then** the appropriate implementation is selected based on priority
3. **Given** a scene is active, **When** the camera position is updated, **Then** the viewport reflects the new camera position and all subscribers receive camera update events
4. **Given** a scene transition is requested, **When** the transition completes, **Then** both `SceneUnloadedEvent` and `SceneLoadedEvent` are published in correct order

---

### User Story 5 - Plugin Developer Implements Input Handling Contract (Priority: P1)

A plugin developer wants to implement input handling for dungeon navigation using scope-based input routing. They need to handle keyboard input that behaves differently depending on context (in-game movement, menu navigation, dialog responses) without complex conditional logic.

**Why this priority**: Input handling is essential for player interaction. The scope-based pattern (pushing/popping input contexts) is critical for modal UI elements like menus, dialogs, and inventory screens that temporarily override game controls.

**Independent Test**: Can be fully tested by creating the `LablabBean.Contracts.Input` assembly, implementing an input router with scope stacking, pushing a menu scope, and verifying that input events are routed to the menu scope instead of the game scope.

**Acceptance Scenarios**:

1. **Given** an input router is active with a game input scope, **When** a modal menu scope is pushed, **Then** subsequent input events are routed to the menu scope only
2. **Given** a menu scope is active, **When** the menu scope is disposed (popped), **Then** input events are routed back to the game scope
3. **Given** an input mapper is configured with action bindings, **When** raw keyboard input is received, **Then** the corresponding game action is triggered and an `InputActionTriggeredEvent` is published
4. **Given** multiple input scopes are stacked, **When** input is received, **Then** only the top scope receives the input

---

### User Story 6 - Plugin Developer Implements Configuration Contract (Priority: P1)

A plugin developer wants to manage game configuration (difficulty settings, keybindings, graphics options) with a contract that supports both reading and writing configuration values, with automatic change notifications.

**Why this priority**: Configuration management is essential for game settings, plugin preferences, and runtime customization. The event-driven notification pattern ensures UI and gameplay systems stay synchronized when settings change.

**Independent Test**: Can be fully tested by creating the `LablabBean.Contracts.Config` assembly, implementing a configuration service, subscribing to config change events, updating a config value, and verifying subscribers are notified.

**Acceptance Scenarios**:

1. **Given** a configuration service is registered, **When** a plugin reads a config value via `Get<T>(key)`, **Then** the typed value is returned correctly
2. **Given** a plugin subscribes to `ConfigChangedEvent`, **When** a config value is updated via `Set(key, value)`, **Then** the subscriber receives the event with old and new values
3. **Given** configuration exists in sections, **When** a plugin requests a section via `GetSection(key)`, **Then** all values under that section are accessible
4. **Given** configuration is loaded from a file, **When** `ReloadAsync()` is called, **Then** the configuration is refreshed and change events are published for modified values

---

### User Story 7 - Plugin Developer Implements Resource Loading Contract (Priority: P1)

A plugin developer wants to load game assets (dungeon map data, sprite sheets, tile definitions) asynchronously with a contract that supports caching, preloading, and error handling across different platforms.

**Why this priority**: Resource management is critical for loading dungeon data, assets, and game content. Asynchronous loading prevents UI blocking, caching improves performance, and platform-agnostic contracts enable the same game content to work across Console, Windows, and future Unity implementations.

**Independent Test**: Can be fully tested by creating the `LablabBean.Contracts.Resource` assembly, implementing a resource loader, loading a test resource asynchronously, and verifying the resource is cached and subsequent loads return the cached instance.

**Acceptance Scenarios**:

1. **Given** a resource service is registered, **When** a plugin loads a resource via `LoadAsync<T>(resourceId)`, **Then** the resource is loaded asynchronously and a `ResourceLoadedEvent` is published
2. **Given** a resource is already loaded, **When** the same resource is requested again, **Then** the cached instance is returned immediately without reloading
3. **Given** a plugin requests preloading of multiple resources, **When** `PreloadAsync(resourceIds)` is called, **Then** all resources are loaded in the background without blocking
4. **Given** a resource fails to load, **When** `LoadAsync<T>(resourceId)` is called, **Then** a `ResourceLoadFailedEvent` is published with error details

---

### Edge Cases

**Event Bus**:
- What happens when a plugin publishes an event but no subscribers exist? (Should complete successfully without errors)
- How does the system handle a subscriber that throws an exception? (Error should be logged but other subscribers should continue to receive the event)
- How does the system handle circular event dependencies (Plugin A publishes event that triggers Plugin B to publish event that Plugin A subscribes to)? (Should handle gracefully with proper async execution, but document as anti-pattern)
- How does the system handle plugin unload when event subscriptions still exist? (Should handle gracefully, document subscription lifecycle)

**Service Registry**:
- What happens when a plugin tries to retrieve a service that has no registered implementation? (Should throw `InvalidOperationException` with clear error message)
- What happens when multiple implementations of a service have the same priority? (Should return one deterministically, document priority tie-breaking strategy)

**Scene Contract**:
- What happens when a scene transition is requested while another transition is in progress? (Should queue or reject with clear error)
- How does the system handle scene loading failures? (Should publish `SceneLoadFailedEvent` and maintain previous scene if possible)
- What happens when camera boundaries exceed scene bounds? (Should clamp to valid viewport or throw descriptive error)

**Input Contract**:
- What happens when an input scope is pushed but never popped? (Should handle gracefully, document scope lifecycle best practices)
- How does the system handle input when no scopes are registered? (Should throw clear error or route to default scope)
- What happens when an input action has no mapped key binding? (Should return false from `TryGetAction`, document fallback patterns)

**Config Contract**:
- What happens when a config key doesn't exist? (Should return null or default value, not throw)
- How does the system handle type mismatches in `Get<T>(key)`? (Should throw with clear error message indicating expected vs actual type)
- What happens when config file is corrupted or missing? (Should use default values and log warning)

**Resource Contract**:
- What happens when a resource fails to load? (Should publish `ResourceLoadFailedEvent` and throw exception from `LoadAsync`)
- How does the system handle concurrent loads of the same resource? (Should deduplicate requests and return same cached instance)
- What happens when a resource is unloaded while still in use? (Document lifecycle management, consider ref counting)

## Requirements *(mandatory)*

### Functional Requirements

#### Event Bus Foundation (Tier 2)

- **FR-001**: System MUST provide an `IEventBus` interface in `LablabBean.Plugins.Contracts` with `PublishAsync<T>()` and `Subscribe<T>()` methods
- **FR-002**: Event bus MUST support asynchronous event publishing where all registered subscribers are notified
- **FR-003**: Event bus MUST execute subscribers sequentially (not in parallel) to maintain predictable ordering
- **FR-004**: Event bus MUST catch exceptions from individual subscribers to prevent one failing subscriber from breaking others
- **FR-005**: Event bus MUST log errors when a subscriber throws an exception
- **FR-006**: Event bus MUST be registered as a singleton in the plugin host and available via `IRegistry`
- **FR-007**: Plugins MUST be able to subscribe to events during their initialization phase
- **FR-008**: Event publishing MUST complete successfully even when no subscribers exist for the event type

#### Domain Contract Assemblies (Tier 1)

- **FR-009**: System MUST provide a `LablabBean.Contracts.Game` assembly for game service contracts
- **FR-010**: System MUST provide a `LablabBean.Contracts.UI` assembly for UI rendering and input contracts
- **FR-011**: System MUST provide a `LablabBean.Contracts.Scene` assembly for scene/level management contracts
- **FR-012**: System MUST provide a `LablabBean.Contracts.Input` assembly for input handling contracts
- **FR-013**: System MUST provide a `LablabBean.Contracts.Config` assembly for configuration management contracts
- **FR-014**: System MUST provide a `LablabBean.Contracts.Resource` assembly for resource loading contracts
- **FR-015**: Each contract assembly MUST follow the namespace pattern: `LablabBean.Contracts.{Domain}.Services`
- **FR-016**: Each contract assembly MUST define service interfaces named `IService` within their domain namespace
- **FR-017**: Contract assemblies MUST only reference `LablabBean.Plugins.Contracts` and standard .NET libraries (no implementation dependencies)
- **FR-018**: Contract assemblies MUST define supporting types (events, models, options) in the root namespace of the assembly

#### Event Definitions (Tier 1)

- **FR-019**: All events MUST be defined as immutable `record` types
- **FR-020**: Event naming MUST follow the pattern: `{Subject}{Action}Event` (e.g., `EntitySpawnedEvent`, `ConfigChangedEvent`)
- **FR-021**: Events MUST include a `DateTimeOffset Timestamp` property
- **FR-022**: Events MUST provide a convenience constructor that automatically sets the timestamp to `DateTimeOffset.UtcNow`
- **FR-023**: Game contract MUST define events: `EntitySpawnedEvent`, `EntityMovedEvent`, `CombatEvent`, `GameStateChangedEvent`
- **FR-024**: UI contract MUST define events for viewport changes and rendering updates
- **FR-025**: Scene contract MUST define events: `SceneLoadedEvent`, `SceneUnloadedEvent`, `CameraUpdatedEvent`, `SceneTransitionEvent`
- **FR-026**: Input contract MUST define events: `RawInputReceivedEvent`, `InputActionTriggeredEvent`, `InputScopeChangedEvent`
- **FR-027**: Config contract MUST define events: `ConfigChangedEvent`, `ConfigReloadedEvent`
- **FR-028**: Resource contract MUST define events: `ResourceLoadedEvent`, `ResourceLoadFailedEvent`, `ResourceUnloadedEvent`
- **FR-029**: All event properties MUST be read-only (record pattern enforces this)

#### Service Contract Patterns (Tier 1)

- **FR-030**: Game service contract MUST define methods for: starting game, processing turns, spawning entities, moving entities, attacking
- **FR-031**: Game service contract MUST define method to retrieve current game state
- **FR-032**: Game service contract MUST define method to retrieve all entities as snapshots
- **FR-033**: UI service contract MUST define methods for: rendering viewport, updating display
- **FR-034**: Service interfaces MUST be technology-agnostic (no references to specific UI frameworks or platforms)
- **FR-035**: Service interfaces MUST support async operations where I/O or long-running work is expected

#### Scene Contract (Tier 1)

- **FR-036**: Scene service contract MUST define method for loading scenes asynchronously: `LoadSceneAsync(sceneId, ct)`
- **FR-037**: Scene service contract MUST define method for unloading scenes: `UnloadSceneAsync(sceneId, ct)`
- **FR-038**: Scene service contract MUST define method for getting current viewport: `GetViewport()`
- **FR-039**: Scene service contract MUST define method for setting camera position: `SetCamera(camera)`
- **FR-040**: Scene service contract MUST define method for updating world entities: `UpdateWorld(entitySnapshots)`
- **FR-041**: Scene service contract MUST support camera tracking (following player entity)
- **FR-042**: Scene contract MUST define supporting types: `Camera`, `Viewport`, `CameraViewport`

#### Input Contract (Tier 1)

- **FR-043**: Input router service MUST define method for pushing input scopes: `PushScope(scope)` returning `IDisposable`
- **FR-044**: Input router service MUST define method for dispatching input: `Dispatch(inputEvent)`
- **FR-045**: Input router service MUST define property for getting current top scope: `Top`
- **FR-046**: Input router MUST route input only to the topmost scope when multiple scopes are stacked
- **FR-047**: Input router MUST support disposing scopes to pop them from the stack
- **FR-048**: Input mapper service MUST define method for mapping raw input to actions: `Map(rawInputEvent)`
- **FR-049**: Input mapper service MUST define method for getting actions: `TryGetAction(actionName, out action)`
- **FR-050**: Input contract MUST define supporting types: `RawKeyEvent`, `InputScope`, `InputAction`

#### Config Contract (Tier 1)

- **FR-051**: Config service contract MUST define method for getting config values: `Get(key)` and `Get<T>(key)`
- **FR-052**: Config service contract MUST define method for setting config values: `Set(key, value)`
- **FR-053**: Config service contract MUST define method for getting config sections: `GetSection(key)`
- **FR-054**: Config service contract MUST define method for checking key existence: `Exists(key)`
- **FR-055**: Config service contract MUST define method for reloading config: `ReloadAsync(ct)`
- **FR-056**: Config service contract MUST support typed value retrieval with automatic conversion
- **FR-057**: Config service contract MUST publish `ConfigChangedEvent` when values are updated
- **FR-058**: Config contract MUST define supporting types: `IConfigSection`, `ConfigChangedEventArgs`

#### Resource Contract (Tier 1)

- **FR-059**: Resource service contract MUST define method for loading resources: `LoadAsync<T>(resourceId, ct)`
- **FR-060**: Resource service contract MUST define method for unloading resources: `Unload(resourceId)`
- **FR-061**: Resource service contract MUST define method for checking if resource is loaded: `IsLoaded(resourceId)`
- **FR-062**: Resource service contract MUST define method for preloading multiple resources: `PreloadAsync(resourceIds, ct)`
- **FR-063**: Resource service MUST cache loaded resources and return cached instances for subsequent requests
- **FR-064**: Resource service MUST publish `ResourceLoadedEvent` on successful load
- **FR-065**: Resource service MUST publish `ResourceLoadFailedEvent` on load failure
- **FR-066**: Resource contract MUST define supporting types: `ResourceMetadata`, `ResourceLoadOptions`

#### Registry Integration (Tier 2)

- **FR-067**: Existing `IRegistry` interface MUST continue to support priority-based service selection
- **FR-068**: Existing `IRegistry` interface MUST continue to support `SelectionMode.HighestPriority` for retrieving services
- **FR-069**: Event bus MUST be retrievable from `IRegistry` using `Get<IEventBus>()`
- **FR-070**: Service implementations MUST be registered via `IRegistry.Register<T>()` with priority metadata
- **FR-071**: Multiple implementations of the same service contract MUST be supported via priority-based selection

#### Migration & Compatibility

- **FR-072**: Existing plugins MUST continue to work during and after the transition (no breaking changes to `IRegistry`, `IPlugin`, `IPluginContext`)
- **FR-073**: New contract assemblies MUST be added without removing existing plugin infrastructure
- **FR-074**: Documentation MUST be updated to explain event-driven patterns and domain contracts
- **FR-075**: Developer guide MUST include examples of creating plugins with event subscriptions
- **FR-076**: Developer guide MUST include examples of implementing service contracts
- **FR-077**: Developer guide MUST include examples of using Scene, Input, Config, and Resource contracts

### Key Entities *(include if feature involves data)*

- **Event**: An immutable record representing something that happened, with timestamp and relevant data. Events are published via `IEventBus` and consumed by subscribers.
- **Service Contract**: An interface defining operations for a specific domain (Game, UI, Scene, Input, Config, Resource). Contracts are platform-agnostic and implemented by plugins.
- **Domain Contract Assembly**: A .NET assembly containing service interfaces, event definitions, and supporting types for a specific domain (e.g., `LablabBean.Contracts.Game`, `LablabBean.Contracts.Scene`).
- **Event Subscription**: A registered handler function that receives events of a specific type when published.
- **Service Implementation**: A concrete class in a plugin that implements a service contract interface and is registered via `IRegistry`.
- **Event Bus**: The infrastructure service (Tier 2) that routes events from publishers to subscribers.
- **Scene**: A level or dungeon instance with associated camera, viewport, and entity collection.
- **Input Scope**: A context for input handling that can be pushed onto a stack for modal behavior (e.g., menu, dialog, in-game).
- **Input Action**: A logical game action (e.g., "MoveNorth", "Attack") mapped from raw input events (e.g., arrow key, spacebar).
- **Config Section**: A hierarchical grouping of configuration values (e.g., "Graphics.Resolution", "Gameplay.Difficulty").
- **Resource**: A game asset (sprite, tile set, dungeon map) that can be loaded asynchronously and cached.

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Event Bus Foundation**:
- **SC-001**: Plugin developers can create an analytics plugin that tracks game events without any direct reference to the game plugin assembly
- **SC-002**: Event publishing completes in under 10ms for events with up to 10 subscribers
- **SC-003**: The event bus handles at least 1000 events per second without blocking the game loop
- **SC-004**: All existing plugins continue to function without modification after the event bus is added

**Domain Contracts**:
- **SC-005**: Plugin developers can create two different UI implementations (e.g., Terminal.Gui and SadConsole) that both use the same game service contract
- **SC-006**: Plugin developers can implement a new service contract with less than 5 lines of registration code
- **SC-007**: A scene implementation can load a dungeon level and update the camera position in under 100ms
- **SC-008**: Input scopes can be pushed and popped without memory leaks or orphaned subscriptions

**Configuration & Resources**:
- **SC-009**: Configuration changes trigger events within 10ms and all subscribers are notified
- **SC-010**: Resource loading supports at least 50 concurrent async operations without deadlocks
- **SC-011**: Resource cache hit rate exceeds 90% for repeated resource requests

**Developer Experience**:
- **SC-012**: Documentation includes at least 6 complete examples covering all contract domains (Game, UI, Scene, Input, Config, Resource)
- **SC-013**: A developer unfamiliar with the codebase can create a working event-subscribing plugin in under 30 minutes using the documentation
- **SC-014**: A developer can implement a complete dungeon loading workflow (scene + resources + config) in under 2 hours using the contracts

## Assumptions

- The existing `IRegistry` implementation is thread-safe and supports concurrent service registration (this is already implemented in `ServiceRegistry.cs`)
- Plugin loading occurs sequentially during host startup (plugins can subscribe to events during `InitializeAsync`)
- Event bus does not require persistence or durability (in-memory only, events are not stored)
- Event subscribers are expected to live for the lifetime of the application (no unsubscribe mechanism required initially)
- Plugin developers understand async/await patterns in C#
- The game loop runs on a single thread (event publishing is async but subscribers execute sequentially)
- Cross-platform support includes Console (.NET), Windows (SadConsole), and potentially Unity/Godot (future)
- Default priority for services is 100, with framework services at 1000+ and game plugins at 100-500
- Scene implementations handle their own rendering coordination (contracts define data flow, not rendering details)
- Input handling is primarily keyboard-based for dungeon crawler (mouse/gamepad can be added later via same contracts)
- Configuration is text-based (JSON, YAML, or similar) with automatic type conversion
- Resource loading supports common formats (JSON for data, PNG for sprites) but specific formats are implementation details
- Input scopes are stack-based (LIFO) - most recently pushed scope receives input first
- Camera viewport is 2D (X, Y coordinates) - 3D support is out of scope

## Out of Scope

**Deferred to Future Iterations**:
- Source generators for automatic proxy service generation (Spec 008)
- Audio contract assembly (`LablabBean.Contracts.Audio`)
- Analytics contract assembly (`LablabBean.Contracts.Analytics`)
- Diagnostics contract assembly (`LablabBean.Contracts.Diagnostics`)
- Resilience contract assembly (`LablabBean.Contracts.Resilience`)
- Advanced contracts (GOAP, Recorder, Capability, Hosting, Terminal)

**Not Planned**:
- Event bus persistence or durability guarantees (in-memory only)
- Event bus parallel subscriber execution (sequential only for predictable ordering)
- Unsubscribe mechanism for events (subscribers live for app lifetime)
- Priority tie-breaking strategy beyond deterministic selection
- Event versioning or schema evolution
- Cross-process event publishing (single process only)
- 3D camera/viewport support (2D dungeon crawler only)
- Mouse or gamepad input contracts (keyboard-focused initially)
- Binary resource formats (text/JSON/PNG initially)
- Resource compression or encryption
- Configuration encryption or secure storage
- Multi-language localization in config system
- Input macro or combo system
- Scene prefab or template system

## Dependencies

- Existing `IRegistry` interface and `ServiceRegistry` implementation must remain stable
- Existing `IPlugin` and `IPluginContext` interfaces must remain stable
- .NET 8 runtime
- Existing plugin loading infrastructure in `LablabBean.Plugins.Core`

## Risks

- **Event bus performance**: If subscribers perform heavy synchronous work, the event bus could become a bottleneck. **Mitigation**: Document best practices for async handlers, consider adding performance monitoring.
- **Circular event dependencies**: Plugins could create circular dependencies via events (A publishes event that triggers B to publish event that triggers A). **Mitigation**: Document as anti-pattern, provide guidance on event design.
- **Breaking changes during migration**: Adding new contracts could inadvertently break existing plugins. **Mitigation**: Phased rollout, maintain backward compatibility with existing `IRegistry`, thorough testing.
- **Documentation drift**: Event contracts and patterns could become outdated as code evolves. **Mitigation**: Include contract documentation in same PR as contract changes, add documentation validation to CI.
- **Scope creep**: Six contract assemblies (Game, UI, Scene, Input, Config, Resource) is a large initial scope. **Mitigation**: Implement in phases - EventBus first, then Game/UI, then Scene/Input/Config/Resource. All contracts defined upfront but implemented incrementally.
- **Input scope memory leaks**: If input scopes are not properly disposed, they could accumulate and cause memory issues. **Mitigation**: Use `IDisposable` pattern, document proper usage with `using` statements, add unit tests for scope cleanup.
- **Resource cache unbounded growth**: If resources are never unloaded, cache could consume excessive memory. **Mitigation**: Document resource lifecycle, consider adding cache size limits or LRU eviction in implementation.
- **Config reload race conditions**: If config is reloaded while plugins are reading values, inconsistent state could occur. **Mitigation**: Document reload semantics, consider copy-on-write or read-write locking in implementation.
- **Scene transition edge cases**: Complex scene transitions (loading while unloading, rapid scene changes) could cause state corruption. **Mitigation**: Define clear transition semantics in contract, implement state machine for scene lifecycle.

## Notes

- This specification focuses on Tier 1 (contracts) and Tier 2 (infrastructure) based on the cross-milo reference architecture
- The cross-milo project provides 15 contract assemblies; lablab-bean implements 6 core contracts (Game, UI, Scene, Input, Config, Resource) essential for dungeon crawlers
- The existing `IRegistry` implementation in lablab-bean is already well-aligned with cross-milo patterns
- Event-driven communication is the key differentiator from the current architecture
- Source generators (used in cross-milo for proxy services) are deferred to Spec 008 to reduce initial complexity
- The specification is technology-agnostic per the template guidelines, focusing on "what" not "how"
- This spec expands the original scope from 2 contracts (Game, UI) to 6 contracts based on gap analysis with cross-milo
- Scene, Input, Config, and Resource contracts are essential for a functional dungeon crawler and were missing from the original spec
- All contracts follow cross-milo naming conventions: generic `IService` within domain namespace (e.g., `LablabBean.Contracts.Scene.Services.IService`)
- Implementation can be phased: Phase 1 (EventBus), Phase 2 (Game/UI events), Phase 3 (Scene/Input), Phase 4 (Config/Resource)
- Total functional requirements: 77 (vs 37 in original spec) covering all 6 contract domains comprehensively
