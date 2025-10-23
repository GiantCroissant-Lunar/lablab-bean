namespace LablabBean.Contracts.Input;

/// <summary>
/// Action Map containing a group of related input actions.
/// Inspired by Unity Input System's Action Map concept.
/// </summary>
/// <param name="Name">Unique name of the action map.</param>
/// <param name="Actions">Collection of actions in this map.</param>
/// <param name="IsEnabled">Whether this action map is currently active.</param>
public record InputActionMap(
    string Name,
    IReadOnlyDictionary<string, InputActionDefinition> Actions,
    bool IsEnabled = true
)
{
    /// <summary>
    /// Create an action map with actions from a collection.
    /// </summary>
    /// <param name="name">Name of the action map.</param>
    /// <param name="actions">Actions to include.</param>
    /// <param name="isEnabled">Whether the map starts enabled.</param>
    public static InputActionMap Create(string name, IEnumerable<InputActionDefinition> actions, bool isEnabled = true)
    {
        var actionDict = actions.ToDictionary(a => a.Name, a => a);
        return new InputActionMap(name, actionDict, isEnabled);
    }

    /// <summary>
    /// Get an action by name.
    /// </summary>
    /// <param name="actionName">Name of the action.</param>
    /// <returns>Action definition if found, null otherwise.</returns>
    public InputActionDefinition? GetAction(string actionName)
    {
        return Actions.TryGetValue(actionName, out var action) ? action : null;
    }
}

/// <summary>
/// Definition of an input action with its bindings.
/// </summary>
/// <param name="Name">Unique name of the action.</param>
/// <param name="ActionType">Type of action (Button, Value, PassThrough).</param>
/// <param name="Bindings">Input bindings for this action.</param>
/// <param name="IsEnabled">Whether this action is currently active.</param>
public record InputActionDefinition(
    string Name,
    InputActionType ActionType,
    IReadOnlyList<InputBinding> Bindings,
    bool IsEnabled = true
)
{
    /// <summary>
    /// Create a simple button action with a single key binding.
    /// </summary>
    /// <param name="name">Action name.</param>
    /// <param name="key">Key to bind.</param>
    /// <param name="modifiers">Optional modifiers.</param>
    public static InputActionDefinition Button(string name, string key, string modifiers = "")
    {
        var binding = InputBinding.Key(key, modifiers);
        return new InputActionDefinition(name, InputActionType.Button, [binding]);
    }

    /// <summary>
    /// Create a composite action (e.g., WASD movement).
    /// </summary>
    /// <param name="name">Action name.</param>
    /// <param name="bindings">Multiple bindings for the composite.</param>
    public static InputActionDefinition Composite(string name, params InputBinding[] bindings)
    {
        return new InputActionDefinition(name, InputActionType.Value, bindings);
    }
}

/// <summary>
/// Type of input action.
/// </summary>
public enum InputActionType
{
    /// <summary>
    /// Button press/release action.
    /// </summary>
    Button,

    /// <summary>
    /// Continuous value action (e.g., movement axis).
    /// </summary>
    Value,

    /// <summary>
    /// Pass-through action that forwards raw input.
    /// </summary>
    PassThrough
}

/// <summary>
/// Input binding that maps physical input to an action.
/// </summary>
/// <param name="Path">Input path (e.g., "Keyboard/w", "Mouse/leftButton").</param>
/// <param name="Modifiers">Required modifier keys.</param>
/// <param name="CompositeType">Type of composite binding if applicable.</param>
/// <param name="CompositePart">Part name for composite bindings (e.g., "up", "down").</param>
public record InputBinding(
    string Path,
    string Modifiers = "",
    CompositeType CompositeType = CompositeType.None,
    string CompositePart = ""
)
{
    /// <summary>
    /// Create a simple key binding.
    /// </summary>
    /// <param name="key">Key name (e.g., "w", "space", "escape").</param>
    /// <param name="modifiers">Modifier keys (e.g., "ctrl", "shift").</param>
    public static InputBinding Key(string key, string modifiers = "")
    {
        return new InputBinding($"Keyboard/{key}", modifiers);
    }

    /// <summary>
    /// Create a mouse button binding.
    /// </summary>
    /// <param name="button">Mouse button (e.g., "leftButton", "rightButton").</param>
    public static InputBinding Mouse(string button)
    {
        return new InputBinding($"Mouse/{button}");
    }

    /// <summary>
    /// Create a composite binding part (e.g., for WASD movement).
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="compositePart">Part name (e.g., "up", "down", "left", "right").</param>
    public static InputBinding CompositeAxis(string key, string compositePart)
    {
        return new InputBinding($"Keyboard/{key}", CompositePart: compositePart, CompositeType: CompositeType.Axis2D);
    }
}

/// <summary>
/// Type of composite binding.
/// </summary>
public enum CompositeType
{
    /// <summary>
    /// Not a composite binding.
    /// </summary>
    None,

    /// <summary>
    /// 1D axis composite (e.g., A/D for left/right).
    /// </summary>
    Axis,

    /// <summary>
    /// 2D axis composite (e.g., WASD for movement).
    /// </summary>
    Axis2D,

    /// <summary>
    /// Button with modifiers composite.
    /// </summary>
    ButtonWithModifiers
}
