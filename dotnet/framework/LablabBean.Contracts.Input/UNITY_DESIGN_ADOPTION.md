# Unity Input System Design Adoption

This document outlines how LablabBean.Contracts.Input adopts Unity Input System's design patterns while maintaining platform independence.

## Architecture Overview

```
InputAsset
├── ActionMaps (Player, UI, Menu)
│   └── Actions (Move, Jump, Attack)
│       └── Bindings (WASD, Space, Mouse)
└── ControlSchemes (Keyboard, Gamepad)
    └── DeviceRequirements (Required/Optional)
```

## Key Components

### 1. Input Asset (`InputAsset`)

Top-level container for all input configuration, similar to Unity's Input Action Asset.

```csharp
var asset = InputAsset.Create("GameInput", actionMaps, controlSchemes);
```

### 2. Action Maps (`InputActionMap`)

Groups of related actions that can be enabled/disabled together.

```csharp
var playerMap = InputActionMap.Create("Player", playerActions, isEnabled: true);
var uiMap = InputActionMap.Create("UI", uiActions, isEnabled: false);
```

### 3. Actions (`InputActionDefinition`)

Individual input actions with their bindings and behavior.

```csharp
// Simple button action
var jump = InputActionDefinition.Button("Jump", "space");

// Composite action (WASD movement)
var move = InputActionDefinition.Composite("Move",
    InputBinding.CompositeAxis("w", "up"),
    InputBinding.CompositeAxis("s", "down"),
    InputBinding.CompositeAxis("a", "left"),
    InputBinding.CompositeAxis("d", "right")
);
```

### 4. Bindings (`InputBinding`)

Map physical input to actions with support for modifiers and composites.

```csharp
// Simple key binding
var keyBinding = InputBinding.Key("w");

// Key with modifiers
var modifiedBinding = InputBinding.Key("c", "ctrl");

// Mouse binding
var mouseBinding = InputBinding.Mouse("leftButton");

// Composite part
var compositeBinding = InputBinding.CompositeAxis("w", "up");
```

### 5. Control Schemes (`ControlScheme`)

Define device requirements for different input methods.

```csharp
var keyboardMouse = ControlScheme.KeyboardMouse();
var gamepad = ControlScheme.Gamepad();
var keyboardOnly = ControlScheme.Keyboard();
```

## Action Map Service (`ActionMap.IService`)

Manages action maps and provides callback registration:

```csharp
// Load input configuration
await actionMapService.LoadAssetAsync(inputAsset);

// Switch control schemes
actionMapService.SwitchControlScheme("Keyboard & Mouse");

// Enable/disable action maps
actionMapService.EnableActionMap("Player");
actionMapService.DisableActionMap("UI");

// Register action callbacks
var callback = actionMapService.RegisterActionCallback("Jump", context =>
{
    if (context.Phase == InputActionPhase.Performed)
    {
        // Handle jump
    }
});
```

## Integration with Existing System

The Unity-inspired design extends the existing LablabBean input system:

### Existing Components (Preserved)

- `RawKeyEvent` - Raw input capture
- `InputEvent` - Processed input events
- `InputCommand` - Command pattern for input
- `Mapper.IService` - Key-to-action mapping
- `Router.IService` - Scope-based routing

### New Components (Unity-Inspired)

- `InputAsset` - Configuration container
- `InputActionMap` - Action grouping
- `InputActionDefinition` - Action definitions
- `InputBinding` - Flexible binding system
- `ControlScheme` - Device management
- `ActionMap.IService` - High-level management

## Benefits of Unity Design Adoption

1. **Familiar Patterns**: Developers familiar with Unity will recognize the structure
2. **Flexible Binding**: Support for composite actions (WASD movement)
3. **Context Switching**: Easy switching between input contexts (Player/UI/Menu)
4. **Device Management**: Control schemes handle different input devices
5. **Scalable**: Easy to add new actions, maps, and control schemes
6. **Event-Driven**: Clean callback system for action handling

## Usage Examples

### Basic Setup

```csharp
// Create actions
var actions = new[]
{
    InputActionDefinition.Button("Jump", "space"),
    InputActionDefinition.Button("Attack", "leftButton"),
    InputActionDefinition.Composite("Move",
        InputBinding.CompositeAxis("w", "up"),
        InputBinding.CompositeAxis("s", "down"),
        InputBinding.CompositeAxis("a", "left"),
        InputBinding.CompositeAxis("d", "right")
    )
};

// Create action map
var playerMap = InputActionMap.Create("Player", actions);

// Create control scheme
var scheme = ControlScheme.KeyboardMouse();

// Create input asset
var asset = InputAsset.Create("Game", [playerMap], [scheme]);
```

### Runtime Management

```csharp
// Load and activate
await actionMapService.LoadAssetAsync(asset);
actionMapService.SwitchControlScheme("Keyboard & Mouse");

// Register callbacks
actionMapService.RegisterActionCallback("Jump", context =>
{
    Console.WriteLine($"Jump {context.Phase}!");
});

// Context switching
actionMapService.DisableActionMap("Player");
actionMapService.EnableActionMap("UI");
```

## Implementation Status

✅ **Completed**:

- Core data structures (InputAsset, ActionMap, etc.)
- Action and binding definitions
- Control scheme system
- Service interface design
- Usage examples and documentation

🔄 **Next Steps**:

- Implement ActionMap.IService in a plugin
- Integrate with existing Mapper/Router services
- Add configuration loading (JSON/XML)
- Create builder pattern for easier setup
- Add validation and error handling

This design provides a solid foundation for Unity-style input management while remaining platform-independent and integrating seamlessly with the existing LablabBean architecture.
