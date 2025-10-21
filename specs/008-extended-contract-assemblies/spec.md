# Feature Specification: Extended Contract Assemblies

**Feature Branch**: `008-extended-contract-assemblies`
**Created**: 2025-10-21
**Completed**: 2025-10-22
**Status**: Complete
**Prerequisites**: Spec 007 (Tiered Contract Architecture) ✅ Complete
**Input**: Gap analysis from cross-milo reference architecture showing 4 critical missing contract domains

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scene Management for Dungeon Loading (Priority: P1)

A plugin developer wants to create a dungeon scene loader that manages level transitions, camera positioning, and world entity updates. They need to load/unload scenes, control the camera viewport, and receive events when scenes change without coupling to specific rendering engines.

**Why this priority**: Scene management is fundamental for dungeon crawlers. Without scene contracts, level loading, camera control, and world rendering are tightly coupled to specific UI implementations, preventing platform independence.

**Independent Test**: Can be fully tested by creating a scene loader plugin that implements `IService`, loading a test scene, verifying camera positioning works, and confirming `SceneLoadedEvent` is published without any dependency on Terminal.Gui or SadConsole.

**Acceptance Scenarios**:

1. **Given** a scene service is registered, **When** a plugin calls `LoadSceneAsync("dungeon-1")`, **Then** the scene loads successfully and `SceneLoadedEvent` is published with scene metadata
2. **Given** a scene is loaded, **When** the camera position is updated via `SetCamera()`, **Then** the viewport reflects the new camera position and entities are rendered relative to it
3. **Given** game entities move, **When** `UpdateWorld()` is called with entity snapshots, **Then** the scene updates entity positions and the display refreshes accordingly

---

### User Story 2 - Input Routing for Modal Dialogs (Priority: P1)

A plugin developer wants to create a modal inventory screen that captures all keyboard input while open, then returns input control to the game when closed. They need scope-based input routing where the topmost scope receives input first, with automatic cleanup when scopes are popped.

**Why this priority**: Modal UI (inventory, menus, dialogs) requires input scope management. Without input contracts, every UI plugin must implement its own input routing, leading to conflicts and bugs when multiple plugins try to handle input simultaneously.

**Independent Test**: Can be fully tested by pushing an input scope for a modal dialog, dispatching keyboard events, verifying only the modal scope receives them, then popping the scope and confirming input returns to the game scope.

**Acceptance Scenarios**:

1. **Given** the game is running, **When** a plugin pushes an inventory input scope, **Then** all subsequent input events are routed to the inventory scope only
2. **Given** multiple input scopes are pushed (game → menu → dialog), **When** input is dispatched, **Then** only the topmost scope (dialog) receives the input
3. **Given** an input scope is popped, **When** the scope's `IDisposable` is disposed, **Then** input automatically routes to the next scope in the stack

---

### User Story 3 - Configuration Management with Change Notifications (Priority: P1)

A plugin developer wants to create a settings plugin that allows players to change game configuration (difficulty, keybindings, graphics) at runtime. They need to get/set config values, receive events when config changes, and support hierarchical configuration sections.

**Why this priority**: Game settings are essential for player experience. Without config contracts, each plugin implements its own configuration storage, leading to inconsistent settings management and no way to react to config changes across plugins.

**Independent Test**: Can be fully tested by setting a config value via `Set()`, retrieving it via `Get<T>()`, subscribing to `ConfigChangedEvent`, changing the value again, and verifying the event is published with old/new values.

**Acceptance Scenarios**:

1. **Given** a config service is registered, **When** a plugin calls `Set("difficulty", "hard")`, **Then** the value is stored and `ConfigChangedEvent` is published
2. **Given** config values exist, **When** a plugin calls `Get<string>("difficulty")`, **Then** the typed value is returned with automatic type conversion
3. **Given** a plugin subscribes to config changes, **When** any config value changes, **Then** the subscriber receives `ConfigChangedEvent` with the key, old value, and new value

---

### User Story 4 - Async Resource Loading with Caching (Priority: P1)

A plugin developer wants to create an asset loader that loads dungeon data files, sprite sheets, and tile maps asynchronously with caching. They need to load resources on-demand, preload resources for performance, and receive events when loading succeeds or fails.

**Why this priority**: Asset management is critical for performance and user experience. Without resource contracts, each plugin implements its own loading/caching logic, leading to duplicate assets in memory and inconsistent loading patterns.

**Independent Test**: Can be fully tested by calling `LoadAsync<T>("dungeon-data")`, verifying the resource is loaded and cached, calling `LoadAsync` again and confirming it returns from cache, then calling `Unload()` and verifying the resource is removed from cache.

**Acceptance Scenarios**:

1. **Given** a resource service is registered, **When** a plugin calls `LoadAsync<DungeonData>("level-1")`, **Then** the resource is loaded asynchronously and `ResourceLoadedEvent` is published
2. **Given** a resource is already loaded, **When** `LoadAsync` is called again for the same resource, **Then** the cached resource is returned immediately without reloading
3. **Given** multiple resources need preloading, **When** `PreloadAsync()` is called with a list of resource IDs, **Then** all resources are loaded concurrently and events are published for each

---

### Edge Cases

- What happens when a scene is loaded but the scene file doesn't exist? (Should publish `SceneLoadFailedEvent` with error details)
- How does the system handle input scope disposal if an exception occurs? (Should use `IDisposable` pattern to ensure cleanup even on exceptions)
- What happens when config is reloaded while a plugin is reading values? (Should use thread-safe access, config snapshot pattern)
- How does the system handle circular resource dependencies (Resource A loads Resource B which loads Resource A)? (Should detect cycles and throw `InvalidOperationException` with clear error message)
- What happens when multiple plugins try to load the same resource concurrently? (Should deduplicate requests and return the same Task to all callers)
- How does the system handle scene transitions during active gameplay? (Should unload old scene, publish `SceneUnloadedEvent`, load new scene, publish `SceneLoadedEvent` in correct order)

## Requirements *(mandatory)*

### Functional Requirements

#### Scene Contract (Tier 1)

- **FR-001**: System MUST provide a `LablabBean.Contracts.Scene` assembly for scene/level management contracts
- **FR-002**: Scene service interface MUST define `LoadSceneAsync(string sceneId)` method that returns `Task`
- **FR-003**: Scene service interface MUST define `UnloadSceneAsync(string sceneId)` method that returns `Task`
- **FR-004**: Scene service interface MUST define `GetViewport()` method that returns current viewport dimensions
- **FR-005**: Scene service interface MUST define `GetCameraViewport()` method that returns camera position and viewport
- **FR-006**: Scene service interface MUST define `SetCamera(Camera camera)` method for camera positioning
- **FR-007**: Scene service interface MUST define `UpdateWorld(IReadOnlyList<EntitySnapshot> snapshots)` method for entity updates
- **FR-008**: Scene contract MUST define `SceneLoadedEvent` with scene ID and timestamp
- **FR-009**: Scene contract MUST define `SceneUnloadedEvent` with scene ID and timestamp
- **FR-010**: Scene contract MUST define `SceneLoadFailedEvent` with scene ID, error, and timestamp
- **FR-011**: Scene contract MUST define `Camera` record with Position and Zoom properties
- **FR-012**: Scene contract MUST define `Viewport` record with Width and Height properties
- **FR-013**: Scene contract MUST define `CameraViewport` record combining Camera and Viewport
- **FR-014**: Scene loading MUST complete in under 100ms for small scenes (< 100 entities)
- **FR-015**: Scene transitions MUST publish events in correct order: UnloadedEvent then LoadedEvent

#### Input Contract (Tier 1)

- **FR-016**: System MUST provide a `LablabBean.Contracts.Input` assembly for input routing and mapping contracts
- **FR-017**: Input router service MUST define `PushScope(IInputScope scope)` method that returns `IDisposable`
- **FR-018**: Input router service MUST define `Dispatch(InputEvent evt)` method for routing input to topmost scope
- **FR-019**: Input router service MUST define `Top` property that returns the current topmost scope or null
- **FR-020**: Input router MUST route input to topmost scope only (stack-based routing)
- **FR-021**: Input scope disposal MUST automatically pop the scope from the stack
- **FR-022**: Input mapper service MUST define `Map(RawKeyEvent rawKey)` method for action mapping
- **FR-023**: Input mapper service MUST define `TryGetAction(string actionName, out InputAction action)` method
- **FR-024**: Input contract MUST define `RawKeyEvent` record with Key and Modifiers properties
- **FR-025**: Input contract MUST define `InputEvent` record with Command and Timestamp properties
- **FR-026**: Input contract MUST define `InputActionTriggeredEvent` with action name and timestamp
- **FR-027**: Input scope stack MUST handle exceptions during scope disposal gracefully
- **FR-028**: Input scope stack MUST not leak memory when scopes are pushed/popped repeatedly

#### Config Contract (Tier 1)

- **FR-029**: System MUST provide a `LablabBean.Contracts.Config` assembly for configuration management contracts
- **FR-030**: Config service MUST define `Get(string key)` method that returns string value or null
- **FR-031**: Config service MUST define `Get<T>(string key)` method with automatic type conversion
- **FR-032**: Config service MUST define `Set(string key, string value)` method for updating config
- **FR-033**: Config service MUST define `GetSection(string key)` method that returns `IConfigSection`
- **FR-034**: Config service MUST define `Exists(string key)` method to check if key exists
- **FR-035**: Config service MUST define `ReloadAsync()` method for reloading config from source
- **FR-036**: Config service MUST define `ConfigChanged` event that fires when any value changes
- **FR-037**: Config contract MUST define `ConfigChangedEvent` with Key, OldValue, NewValue, and Timestamp
- **FR-038**: Config contract MUST define `IConfigSection` interface for hierarchical config
- **FR-039**: Config access MUST be thread-safe for concurrent reads and writes
- **FR-040**: Config change events MUST be published within 10ms of value change
- **FR-041**: Config service MUST support hierarchical keys using colon separator (e.g., "game:difficulty")

#### Resource Contract (Tier 1)

- **FR-042**: System MUST provide a `LablabBean.Contracts.Resource` assembly for resource loading contracts
- **FR-043**: Resource service MUST define `LoadAsync<T>(string resourceId)` method that returns `Task<T>`
- **FR-044**: Resource service MUST define `Unload(string resourceId)` method for cache eviction
- **FR-045**: Resource service MUST define `IsLoaded(string resourceId)` method to check cache status
- **FR-046**: Resource service MUST define `PreloadAsync(IEnumerable<string> resourceIds)` method for batch loading
- **FR-047**: Resource contract MUST define `ResourceLoadedEvent` with resource ID and timestamp
- **FR-048**: Resource contract MUST define `ResourceLoadFailedEvent` with resource ID, error, and timestamp
- **FR-049**: Resource loading MUST cache loaded resources to avoid redundant I/O
- **FR-050**: Resource loading MUST deduplicate concurrent requests for the same resource
- **FR-051**: Resource service MUST support concurrent loading of different resources (50+ simultaneous)
- **FR-052**: Resource cache MUST achieve >90% hit rate for frequently accessed resources
- **FR-053**: Resource service MUST detect and prevent circular dependencies

#### Event Definitions (Tier 1)

- **FR-054**: All events MUST be defined as immutable `record` types
- **FR-055**: Event naming MUST follow the pattern: `{Subject}{Action}Event`
- **FR-056**: Events MUST include a `DateTimeOffset Timestamp` property
- **FR-057**: Events MUST provide a convenience constructor that automatically sets timestamp to `DateTimeOffset.UtcNow`
- **FR-058**: All event properties MUST be read-only (record pattern enforces this)

#### Integration with Existing Architecture

- **FR-059**: New contract assemblies MUST only reference `LablabBean.Plugins.Contracts` and standard .NET libraries
- **FR-060**: New contract assemblies MUST follow namespace pattern: `LablabBean.Contracts.{Domain}.Services`
- **FR-061**: Service interfaces MUST be named `IService` within their domain namespace
- **FR-062**: New contracts MUST integrate with existing `IEventBus` for event publishing
- **FR-063**: New contracts MUST integrate with existing `IRegistry` for service registration
- **FR-064**: Service implementations MUST be registered via `IRegistry.Register<T>()` with priority metadata
- **FR-065**: Multiple implementations of the same service contract MUST be supported via priority-based selection

#### Migration & Compatibility

- **FR-066**: Existing plugins from Spec 007 MUST continue to work without modification
- **FR-067**: New contract assemblies MUST be added without breaking existing `IEventBus` or `IRegistry`
- **FR-068**: Documentation MUST be updated to include examples for all 4 new contract domains
- **FR-069**: Developer guide MUST include complete workflow example using Scene + Input + Config + Resource together

### Key Entities *(include if feature involves data)*

- **Scene**: A level or dungeon instance with entities, camera, and viewport. Managed by scene service.
- **Camera**: Position and zoom level for viewport rendering. Controlled via scene service.
- **Viewport**: Display dimensions (width, height) for rendering visible area.
- **Input Scope**: A layer in the input routing stack that handles input events. Implements `IInputScope`.
- **Input Action**: A logical game action (e.g., "MoveNorth", "OpenInventory") mapped from raw keyboard input.
- **Config Section**: A hierarchical group of configuration values (e.g., "game:graphics:resolution").
- **Resource**: An asset (data file, sprite, tile map) loaded from disk and cached in memory.
- **Resource Cache**: In-memory storage for loaded resources to avoid redundant I/O.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Plugin developers can create a scene loader that loads dungeons without any reference to Terminal.Gui or SadConsole assemblies
- **SC-002**: Plugin developers can create modal UI (inventory, menu) that correctly captures input without conflicts from other plugins
- **SC-003**: Scene loading completes in under 100ms for scenes with up to 100 entities
- **SC-004**: Input scope stack does not leak memory after 1,000 push/pop cycles
- **SC-005**: Config change events are published within 10ms of value changes
- **SC-006**: Resource service handles 50+ concurrent load requests without deadlocks
- **SC-007**: Resource cache achieves >90% hit rate for frequently accessed resources in typical gameplay
- **SC-008**: A developer can implement a complete dungeon loading workflow (scene + input + config + resource) in under 2 hours using the documentation

## Assumptions

- Spec 007 (Tiered Contract Architecture) is complete with `IEventBus` and `IRegistry` fully implemented
- Plugin loading occurs sequentially during host startup (plugins can register services during `InitializeAsync`)
- Scene loading is asynchronous but scene transitions are sequential (one scene at a time)
- Input scopes are managed by a single thread (no concurrent scope push/pop)
- Config values are stored as strings and converted to typed values on retrieval
- Resource loading supports any serializable type via generic `LoadAsync<T>()`
- Resource IDs are unique strings (e.g., "dungeon-1", "sprite-player", "tilemap-cave")
- Camera position is in world coordinates, viewport is in screen coordinates
- Input events are dispatched synchronously to the topmost scope
- Config reload is an explicit operation (not automatic file watching)

## Out of Scope

- Audio contract assembly (`LablabBean.Contracts.Audio`) - deferred to Spec 009
- Analytics contract assembly (`LablabBean.Contracts.Analytics`) - deferred to Spec 009
- Diagnostics contract assembly (`LablabBean.Contracts.Diagnostics`) - deferred to Spec 009
- Source generators for automatic proxy service generation - deferred to Spec 010
- Scene prefab system or scene composition - basic scene loading only
- Input replay/recording - basic input routing only
- Config schema validation or type safety - string-based config only
- Resource compression or encryption - basic file loading only
- Resource streaming for large assets - full load into memory only
- Automatic config file watching and reload - explicit reload only
- Input action rebinding UI - action mapping only, no UI
- Scene background loading or progressive loading - full scene load only

## Dependencies

- Spec 007 (Tiered Contract Architecture) MUST be complete
- `IEventBus` interface and `EventBus` implementation from Spec 007
- `IRegistry` interface and `ServiceRegistry` implementation from Spec 007
- .NET 8 runtime
- Existing plugin loading infrastructure in `LablabBean.Plugins.Core`

## Risks

- **Scene loading performance**: Large dungeons (1000+ entities) may exceed 100ms load time. **Mitigation**: Document scene size limits, consider progressive loading in future spec.
- **Input scope complexity**: Nested scopes (game → menu → submenu → dialog) could become hard to debug. **Mitigation**: Add logging for scope push/pop, provide debug visualization tool.
- **Config thread safety**: Concurrent reads during config reload could return inconsistent values. **Mitigation**: Use snapshot pattern (copy-on-write) for config reload.
- **Resource memory usage**: Loading many large resources could exhaust memory. **Mitigation**: Document resource size limits, add cache eviction policy (LRU) in future spec.
- **Circular resource dependencies**: Resources that depend on each other could cause infinite loops. **Mitigation**: Implement cycle detection with clear error messages.
- **Documentation complexity**: 4 new contract domains add significant documentation burden. **Mitigation**: Provide complete working example that uses all 4 domains together.

## Notes

- This specification builds on Spec 007 (Tiered Contract Architecture) which established `IEventBus` and the first two contract assemblies (Game, UI)
- The 4 new contract domains (Scene, Input, Config, Resource) were identified via gap analysis with cross-milo reference architecture
- These contracts are essential for a functional dungeon crawler and should be implemented together as a cohesive unit
- Scene contract enables level loading and camera control (critical for dungeon navigation)
- Input contract enables modal UI and action mapping (critical for inventory, menus, keybindings)
- Config contract enables game settings and runtime configuration (critical for player preferences)
- Resource contract enables asset loading and caching (critical for performance and memory management)
- All 4 contracts follow the same patterns established in Spec 007 (immutable events, `IService` naming, priority-based selection)
- Source generators (used in cross-milo for proxy services) are intentionally deferred to reduce initial complexity
- The specification is technology-agnostic per the template guidelines, focusing on "what" not "how"
