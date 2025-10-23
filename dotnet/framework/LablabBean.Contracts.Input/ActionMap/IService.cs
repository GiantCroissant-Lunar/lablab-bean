namespace LablabBean.Contracts.Input.ActionMap;

/// <summary>
/// Action map service for managing input action maps.
/// Inspired by Unity Input System's Action Map management.
/// </summary>
public interface IService
{
    /// <summary>
    /// Load an input asset containing action maps and control schemes.
    /// </summary>
    /// <param name="asset">Input asset to load.</param>
    Task LoadAssetAsync(InputAsset asset);

    /// <summary>
    /// Load input asset from Unity-compatible JSON file.
    /// </summary>
    /// <param name="jsonFilePath">Path to JSON file.</param>
    Task LoadAssetFromJsonAsync(string jsonFilePath);

    /// <summary>
    /// Load input asset from Unity-compatible JSON string.
    /// </summary>
    /// <param name="json">JSON string.</param>
    Task LoadAssetFromJsonStringAsync(string json);

    /// <summary>
    /// Save current input asset to Unity-compatible JSON file.
    /// </summary>
    /// <param name="jsonFilePath">Path to save JSON file.</param>
    Task SaveAssetToJsonAsync(string jsonFilePath);

    /// <summary>
    /// Enable an action map by name.
    /// </summary>
    /// <param name="mapName">Name of the action map to enable.</param>
    void EnableActionMap(string mapName);

    /// <summary>
    /// Disable an action map by name.
    /// </summary>
    /// <param name="mapName">Name of the action map to disable.</param>
    void DisableActionMap(string mapName);

    /// <summary>
    /// Switch to a different control scheme.
    /// </summary>
    /// <param name="schemeName">Name of the control scheme to activate.</param>
    void SwitchControlScheme(string schemeName);

    /// <summary>
    /// Get all currently enabled action maps.
    /// </summary>
    /// <returns>Collection of enabled action maps.</returns>
    IReadOnlyCollection<InputActionMap> GetEnabledActionMaps();

    /// <summary>
    /// Get the currently active control scheme.
    /// </summary>
    /// <returns>Active control scheme, or null if none active.</returns>
    ControlScheme? GetActiveControlScheme();

    /// <summary>
    /// Find an action across all enabled action maps.
    /// </summary>
    /// <param name="actionName">Name of the action to find.</param>
    /// <returns>Action definition if found, null otherwise.</returns>
    InputActionDefinition? FindAction(string actionName);

    /// <summary>
    /// Find an action in a specific action map.
    /// </summary>
    /// <param name="mapName">Name of the action map.</param>
    /// <param name="actionName">Name of the action.</param>
    /// <returns>Action definition if found, null otherwise.</returns>
    InputActionDefinition? FindAction(string mapName, string actionName);

    /// <summary>
    /// Register a callback for when an action is triggered.
    /// </summary>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="callback">Callback to invoke when action is triggered.</param>
    /// <returns>Disposable to unregister the callback.</returns>
    IDisposable RegisterActionCallback(string actionName, Action<InputActionContext> callback);

    /// <summary>
    /// Register a callback for when an action in a specific map is triggered.
    /// </summary>
    /// <param name="mapName">Name of the action map.</param>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="callback">Callback to invoke when action is triggered.</param>
    /// <returns>Disposable to unregister the callback.</returns>
    IDisposable RegisterActionCallback(string mapName, string actionName, Action<InputActionContext> callback);

    /// <summary>
    /// Event fired when an action map is enabled.
    /// </summary>
    event Action<string> ActionMapEnabled;

    /// <summary>
    /// Event fired when an action map is disabled.
    /// </summary>
    event Action<string> ActionMapDisabled;

    /// <summary>
    /// Event fired when the control scheme changes.
    /// </summary>
    event Action<ControlScheme> ControlSchemeChanged;
}

/// <summary>
/// Context information for an input action callback.
/// </summary>
/// <param name="Action">The action definition that was triggered.</param>
/// <param name="ActionMap">The action map containing the action.</param>
/// <param name="RawInput">The raw input that triggered the action.</param>
/// <param name="Phase">The phase of the action (Started, Performed, Canceled).</param>
/// <param name="Timestamp">When the action was triggered.</param>
public record InputActionContext(
    InputActionDefinition Action,
    InputActionMap ActionMap,
    RawKeyEvent RawInput,
    InputActionPhase Phase,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor with current timestamp.
    /// </summary>
    public InputActionContext(
        InputActionDefinition action,
        InputActionMap actionMap,
        RawKeyEvent rawInput,
        InputActionPhase phase)
        : this(action, actionMap, rawInput, phase, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Phase of an input action.
/// </summary>
public enum InputActionPhase
{
    /// <summary>
    /// Action has started (e.g., key pressed down).
    /// </summary>
    Started,

    /// <summary>
    /// Action is being performed (e.g., key held down or released).
    /// </summary>
    Performed,

    /// <summary>
    /// Action was canceled (e.g., focus lost while key held).
    /// </summary>
    Canceled
}
