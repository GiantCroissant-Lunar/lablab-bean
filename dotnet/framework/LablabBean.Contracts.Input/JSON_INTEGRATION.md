# Unity Input System JSON Integration

This document describes how LablabBean.Contracts.Input supports Unity Input System's JSON format for seamless interoperability.

## Overview

The LablabBean input system now supports loading and saving Unity-compatible `.inputactions` JSON files, enabling:

- **Unity Compatibility**: Use the same input configuration in Unity and LablabBean
- **Visual Editing**: Create input maps in Unity's Input Actions editor
- **Version Control**: Store input configuration as readable JSON files
- **Runtime Configuration**: Load different input setups dynamically

## JSON Format Support

### Supported Unity Features

✅ **Action Maps**: Groups of related actions
✅ **Actions**: Individual input actions with types (Button, Value, PassThrough)
✅ **Bindings**: Key-to-action mappings with composite support
✅ **Control Schemes**: Device-specific configurations
✅ **Composite Bindings**: Multi-key actions (WASD movement)
✅ **Device Requirements**: Required/optional device specifications

### Unity JSON Structure

```json
{
  "name": "GameInput",
  "maps": [
    {
      "name": "Player",
      "id": "unique-guid",
      "actions": [
        {
          "name": "Move",
          "type": "Value",
          "expectedControlType": "Vector2"
        }
      ],
      "bindings": [
        {
          "name": "Axis2D",
          "path": "",
          "action": "Move",
          "isComposite": true
        },
        {
          "name": "up",
          "path": "<Keyboard>/w",
          "action": "Move",
          "isPartOfComposite": true
        }
      ]
    }
  ],
  "controlSchemes": [
    {
      "name": "Keyboard & Mouse",
      "devices": [
        { "devicePath": "<Keyboard>", "isOptional": false },
        { "devicePath": "<Mouse>", "isOptional": false }
      ]
    }
  ]
}
```

## API Usage

### Loading from JSON

```csharp
// Load from file
await actionMapService.LoadAssetFromJsonAsync("GameInput.inputactions");

// Load from string
var json = File.ReadAllText("input.json");
await actionMapService.LoadAssetFromJsonStringAsync(json);

// Using converter directly
var asset = await InputAssetConverter.LoadFromFileAsync("input.json");
await actionMapService.LoadAssetAsync(asset);
```

### Saving to JSON

```csharp
// Save current configuration
await actionMapService.SaveAssetToJsonAsync("output.inputactions");

// Using converter directly
var json = InputAssetConverter.ToJson(inputAsset);
await File.WriteAllTextAsync("output.json", json);
```

### Programmatic Creation + JSON Export

```csharp
// Create input asset programmatically
var actions = new[]
{
    InputActionDefinition.Button("Jump", "space"),
    InputActionDefinition.Composite("Move",
        InputBinding.CompositeAxis("w", "up"),
        InputBinding.CompositeAxis("s", "down"),
        InputBinding.CompositeAxis("a", "left"),
        InputBinding.CompositeAxis("d", "right")
    )
};

var actionMap = InputActionMap.Create("Player", actions);
var controlScheme = ControlScheme.KeyboardMouse();
var asset = InputAsset.Create("Game", [actionMap], [controlScheme]);

// Save as Unity-compatible JSON
await InputAssetConverter.SaveToFileAsync(asset, "Generated.inputactions");
```

## Unity Workflow Integration

### 1. Create in Unity Editor

1. Open Unity Input Actions window
2. Create new Input Actions asset
3. Design your action maps, actions, and bindings
4. Save as `.inputactions` file

### 2. Use in LablabBean

```csharp
// Load the Unity-created file directly
await actionMapService.LoadAssetFromJsonAsync("UnityInput.inputactions");

// Switch control schemes
actionMapService.SwitchControlScheme("Keyboard & Mouse");

// Register callbacks
actionMapService.RegisterActionCallback("Jump", context => {
    if (context.Phase == InputActionPhase.Performed)
        HandleJump();
});
```

### 3. Bidirectional Workflow

```csharp
// Load Unity configuration
await actionMapService.LoadAssetFromJsonAsync("UnityInput.inputactions");

// Modify programmatically (add runtime actions)
var runtimeAction = InputActionDefinition.Button("DebugToggle", "f12");
// ... add to action map ...

// Save modified version
await actionMapService.SaveAssetToJsonAsync("ModifiedInput.inputactions");
```

## Path Mapping

### Unity → LablabBean

| Unity Path | LablabBean Equivalent | Description |
|------------|----------------------|-------------|
| `<Keyboard>/w` | `Keyboard/w` | Keyboard key |
| `<Mouse>/leftButton` | `Mouse/leftButton` | Mouse button |
| `<Gamepad>/buttonSouth` | `Gamepad/buttonSouth` | Gamepad button |

### Composite Bindings

Unity's composite bindings are fully supported:

```json
// Unity 2D Axis Composite
{
  "name": "Axis2D",
  "isComposite": true,
  "action": "Move"
},
{
  "name": "up",
  "path": "<Keyboard>/w",
  "isPartOfComposite": true,
  "action": "Move"
}
```

Maps to:

```csharp
InputActionDefinition.Composite("Move",
    InputBinding.CompositeAxis("w", "up"),
    InputBinding.CompositeAxis("s", "down"),
    InputBinding.CompositeAxis("a", "left"),
    InputBinding.CompositeAxis("d", "right")
)
```

## Example Files

### Basic Game Input

See `dotnet/examples/ActionMapDemo/GameInput.inputactions` for a complete example with:

- Player action map (Move, Jump, Attack, Block, Interact, Sprint)
- UI action map (Submit, Cancel, Navigate, OpenInventory, Pause)
- Menu action map (Select, Back, Up, Down)
- Keyboard & Mouse control scheme
- Keyboard Only control scheme

### Generated Output

The system generates Unity-compatible JSON with:

- Proper GUID generation for IDs
- Correct action types and expected control types
- Composite binding structure
- Device requirements

## Limitations & Differences

### Not Yet Supported

- **Processors**: Unity's input processors (normalize, scale, etc.)
- **Interactions**: Unity's interaction system (hold, tap, etc.)
- **Initial State Check**: Unity's initial state checking
- **Binding Groups**: Advanced binding group filtering

### LablabBean Extensions

- **Simplified Modifiers**: Basic modifier key support
- **Event Integration**: Seamless integration with LablabBean event bus
- **Scope Management**: Enhanced scope-based input routing

## Migration Guide

### From Unity Input System

1. Export your Unity Input Actions as JSON
2. Load directly in LablabBean:

   ```csharp
   await actionMapService.LoadAssetFromJsonAsync("YourUnityInput.inputactions");
   ```

3. Register callbacks for your actions
4. Handle input phases (Started, Performed, Canceled)

### To Unity Input System

1. Create input configuration in LablabBean
2. Save as JSON:

   ```csharp
   await actionMapService.SaveAssetToJsonAsync("ForUnity.inputactions");
   ```

3. Import the JSON file in Unity
4. Assign to PlayerInput component

## Best Practices

### File Organization

```
Assets/Input/
├── GameInput.inputactions          # Main game input
├── MenuInput.inputactions          # Menu-specific input
└── DebugInput.inputactions         # Debug/development input
```

### Naming Conventions

- **Action Maps**: PascalCase (Player, UI, Menu)
- **Actions**: PascalCase (Move, Jump, Attack)
- **Bindings**: Descriptive names for composites (Axis2D, ButtonWithModifiers)

### Version Control

- Store `.inputactions` files in version control
- Use meaningful commit messages for input changes
- Consider separate files for different contexts

### Runtime Configuration

```csharp
// Load different configurations based on context
var inputFile = isDebugMode ? "DebugInput.inputactions" : "GameInput.inputactions";
await actionMapService.LoadAssetFromJsonAsync(inputFile);
```

This JSON integration provides seamless interoperability with Unity while maintaining the flexibility and power of the LablabBean input system.
