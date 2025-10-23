# LablabBean.Plugins.InputActionMap

Unity-inspired Action Map input system for LablabBean framework.

## Overview

This plugin provides a comprehensive input management system inspired by Unity's Input System, featuring:

- **Action Maps**: Group related actions (Player, UI, Menu)
- **Input Bindings**: Flexible key-to-action mapping with composite support
- **Control Schemes**: Device-specific configurations (Keyboard, Mouse, Gamepad)
- **Input Assets**: Top-level configuration containers
- **Event-Driven Callbacks**: Clean callback system for action handling

## Architecture

```
InputAsset
├── ActionMaps (Player, UI, Menu)
│   └── Actions (Move, Jump, Attack)
│       └── Bindings (WASD, Space, Mouse)
└── ControlSchemes (Keyboard, Gamepad)
    └── DeviceRequirements (Required/Optional)
```

## Key Features

### 1. Unity JSON Compatibility

Load and save Unity Input System `.inputactions` files:

```csharp
// Load Unity-created input configuration
await actionMapService.LoadAssetFromJsonAsync("GameInput.inputactions");

// Save current configuration as Unity-compatible JSON
await actionMapService.SaveAssetToJsonAsync("output.inputactions");
```

### 2. Action Maps

Group related actions that can be enabled/disabled together:

```csharp
var playerActions = new[]
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

var playerMap = InputActionMap.Create("Player", playerActions);
```

### 3. Composite Actions

Support for complex input combinations like WASD movement:

```csharp
var moveAction = InputActionDefinition.Composite("Move",
    InputBinding.CompositeAxis("w", "up"),
    InputBinding.CompositeAxis("s", "down"),
    InputBinding.CompositeAxis("a", "left"),
    InputBinding.CompositeAxis("d", "right")
);
```

### 4. Control Schemes

Define device requirements for different input methods:

```csharp
var keyboardMouse = ControlScheme.KeyboardMouse();
var gamepad = ControlScheme.Gamepad();
var keyboardOnly = ControlScheme.Keyboard();
```

### 5. Context Switching

Easy switching between input contexts:

```csharp
// Switch from gameplay to UI
actionMapService.DisableActionMap("Player");
actionMapService.EnableActionMap("UI");

// Switch control schemes
actionMapService.SwitchControlScheme("Gamepad");
```

## Usage

### 1. JSON Configuration (Recommended)

Create input configuration using Unity's Input Actions editor or manually:

```json
{
  "name": "GameInput",
  "maps": [
    {
      "name": "Player",
      "actions": [
        { "name": "Jump", "type": "Button" },
        { "name": "Move", "type": "Value" }
      ],
      "bindings": [
        { "path": "<Keyboard>/space", "action": "Jump" },
        { "name": "Axis2D", "action": "Move", "isComposite": true },
        { "name": "up", "path": "<Keyboard>/w", "action": "Move", "isPartOfComposite": true }
      ]
    }
  ]
}
```

Load in your application:

```csharp
await actionMapService.LoadAssetFromJsonAsync("GameInput.inputactions");
actionMapService.SwitchControlScheme("Keyboard & Mouse");
```

### 2. Register the Plugin

The plugin automatically registers the `ActionMap.IService` when loaded:

```csharp
// Plugin registration happens automatically
// Service is available as ActionMap.IService
```

### 3. Programmatic Configuration

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

### 4. Runtime Usage

```csharp
// Load configuration
await actionMapService.LoadAssetAsync(asset);
actionMapService.SwitchControlScheme("Keyboard & Mouse");

// Register callbacks
var jumpCallback = actionMapService.RegisterActionCallback("Jump", context =>
{
    if (context.Phase == InputActionPhase.Performed)
    {
        Console.WriteLine("Player jumped!");
    }
});

// Process raw input (typically called by input system)
actionMapService.ProcessInput(new RawKeyEvent("space", ""), InputActionPhase.Performed);
```

## Integration with Existing System

This plugin extends the existing LablabBean input system:

### Preserved Components

- `RawKeyEvent` - Raw input capture
- `InputEvent` - Processed input events
- `InputCommand` - Command pattern for input
- `Mapper.IService` - Key-to-action mapping
- `Router.IService` - Scope-based routing

### New Components

- `InputAsset` - Configuration container
- `InputActionMap` - Action grouping
- `InputActionDefinition` - Action definitions
- `InputBinding` - Flexible binding system
- `ControlScheme` - Device management
- `ActionMap.IService` - High-level management

## Examples

See `dotnet/examples/ActionMapDemo/` for a complete working example that demonstrates:

- Creating action maps for Player, UI, and Menu contexts
- Registering action callbacks
- Context switching between input modes
- Processing simulated input events

## Benefits

1. **Familiar Patterns**: Developers familiar with Unity will recognize the structure
2. **Flexible Binding**: Support for composite actions (WASD movement)
3. **Context Switching**: Easy switching between input contexts (Player/UI/Menu)
4. **Device Management**: Control schemes handle different input devices
5. **Scalable**: Easy to add new actions, maps, and control schemes
6. **Event-Driven**: Clean callback system for action handling
7. **Platform Independent**: No Unity dependencies, works anywhere

## Service Registration

The plugin registers:

- `ActionMap.IService` - Main action map management service
- Priority: 300 (higher than basic input handler)
- Name: "ActionMapService"
- Version: "1.0.0"

## Dependencies

- `LablabBean.Plugins.Contracts` - Core plugin framework
- `LablabBean.Contracts.Input` - Input system contracts

## Future Enhancements

- JSON/XML configuration loading
- Builder pattern for easier setup
- Advanced composite binding types
- Input validation and error handling
- Performance optimizations for high-frequency input
- Integration with existing InputHandler plugin
