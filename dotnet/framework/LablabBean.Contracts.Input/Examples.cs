using LablabBean.Contracts.Input.ActionMap;

namespace LablabBean.Contracts.Input;

/// <summary>
/// Example usage of Unity-inspired input system.
/// This class demonstrates how to create action maps, control schemes, and handle input.
/// </summary>
public static class Examples
{
    /// <summary>
    /// Create a complete input asset for a game.
    /// </summary>
    public static InputAsset CreateGameInputAsset()
    {
        // Create Player action map
        var playerActions = new[]
        {
            // Movement using WASD composite
            InputActionDefinition.Composite("Move",
                InputBinding.CompositeAxis("w", "up"),
                InputBinding.CompositeAxis("s", "down"),
                InputBinding.CompositeAxis("a", "left"),
                InputBinding.CompositeAxis("d", "right")
            ),

            // Simple button actions
            InputActionDefinition.Button("Jump", "space"),
            InputActionDefinition.Button("Attack", "leftButton"), // Mouse
            InputActionDefinition.Button("Block", "rightButton"), // Mouse
            InputActionDefinition.Button("Interact", "e"),

            // Actions with modifiers
            InputActionDefinition.Button("Sprint", "leftShift"),
            InputActionDefinition.Button("Crouch", "leftCtrl"),
        };

        // Create UI action map
        var uiActions = new[]
        {
            InputActionDefinition.Button("Submit", "enter"),
            InputActionDefinition.Button("Cancel", "escape"),
            InputActionDefinition.Button("Navigate", "tab"),
            InputActionDefinition.Button("OpenInventory", "i"),
            InputActionDefinition.Button("OpenMap", "m"),
            InputActionDefinition.Button("Pause", "escape"),
        };

        // Create Menu action map
        var menuActions = new[]
        {
            InputActionDefinition.Button("Select", "enter"),
            InputActionDefinition.Button("Back", "escape"),
            InputActionDefinition.Button("Up", "w"),
            InputActionDefinition.Button("Down", "s"),
            InputActionDefinition.Button("Left", "a"),
            InputActionDefinition.Button("Right", "d"),
        };

        // Create action maps
        var actionMaps = new[]
        {
            InputActionMap.Create("Player", playerActions, isEnabled: true),
            InputActionMap.Create("UI", uiActions, isEnabled: false),
            InputActionMap.Create("Menu", menuActions, isEnabled: false),
        };

        // Create control schemes
        var controlSchemes = new[]
        {
            ControlScheme.KeyboardMouse("Keyboard & Mouse"),
            ControlScheme.Keyboard("Keyboard Only"),
            ControlScheme.Gamepad("Gamepad"),
        };

        return InputAsset.Create("GameInput", actionMaps, controlSchemes);
    }

    /// <summary>
    /// Example of how to use the action map service.
    /// </summary>
    public static async Task ExampleUsage(ActionMap.IService actionMapService)
    {
        // Load the input asset
        var inputAsset = CreateGameInputAsset();
        await actionMapService.LoadAssetAsync(inputAsset);

        // Switch to keyboard & mouse control scheme
        actionMapService.SwitchControlScheme("Keyboard & Mouse");

        // Register callbacks for player actions
        var moveCallback = actionMapService.RegisterActionCallback("Move", context =>
        {
            Console.WriteLine($"Move action: {context.Phase} at {context.Timestamp}");
            // Handle movement logic here
        });

        var jumpCallback = actionMapService.RegisterActionCallback("Jump", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
            {
                Console.WriteLine("Player jumped!");
                // Handle jump logic here
            }
        });

        // Switch to UI mode (disable player, enable UI)
        actionMapService.DisableActionMap("Player");
        actionMapService.EnableActionMap("UI");

        // Register UI callbacks
        var pauseCallback = actionMapService.RegisterActionCallback("UI", "Pause", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
            {
                Console.WriteLine("Game paused!");
                // Switch back to player controls
                actionMapService.DisableActionMap("UI");
                actionMapService.EnableActionMap("Player");
            }
        });

        // Clean up callbacks when done
        // moveCallback.Dispose();
        // jumpCallback.Dispose();
        // pauseCallback.Dispose();
    }

    /// <summary>
    /// Example of creating a simple input configuration.
    /// </summary>
    public static InputAsset CreateSimpleInputAsset()
    {
        var actions = new[]
        {
            InputActionDefinition.Button("Confirm", "enter"),
            InputActionDefinition.Button("Cancel", "escape"),
            InputActionDefinition.Button("Up", "w"),
            InputActionDefinition.Button("Down", "s"),
            InputActionDefinition.Button("Left", "a"),
            InputActionDefinition.Button("Right", "d"),
        };

        var actionMap = InputActionMap.Create("Default", actions);
        var controlScheme = ControlScheme.Keyboard();

        return InputAsset.Create("SimpleInput", [actionMap], [controlScheme]);
    }

    /// <summary>
    /// Example of creating complex composite bindings.
    /// </summary>
    public static InputActionDefinition CreateComplexMovement()
    {
        // Create a 2D movement action with multiple binding options
        var bindings = new[]
        {
            // WASD bindings
            InputBinding.CompositeAxis("w", "up"),
            InputBinding.CompositeAxis("s", "down"),
            InputBinding.CompositeAxis("a", "left"),
            InputBinding.CompositeAxis("d", "right"),

            // Arrow key bindings (alternative)
            InputBinding.CompositeAxis("uparrow", "up"),
            InputBinding.CompositeAxis("downarrow", "down"),
            InputBinding.CompositeAxis("leftarrow", "left"),
            InputBinding.CompositeAxis("rightarrow", "right"),
        };

        return new InputActionDefinition("Movement", InputActionType.Value, bindings);
    }
}
