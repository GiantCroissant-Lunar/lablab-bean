using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.ActionMap;
using LablabBean.Contracts.Input.Router;
using LablabBean.Contracts.Input.Serialization;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using ActionMapType = LablabBean.Contracts.Input.InputActionMap;

namespace LablabBean.Plugins.InputActionMap;

/// <summary>
/// Implementation of Unity-inspired Action Map service.
/// Manages input action maps, control schemes, and action callbacks.
/// </summary>
public class ActionMapService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, ActionMapType> _actionMaps = new();
    private readonly Dictionary<string, ControlScheme> _controlSchemes = new();
    private readonly Dictionary<string, List<ActionCallback>> _actionCallbacks = new();
    private readonly HashSet<string> _enabledMaps = new();

    private InputAsset? _currentAsset;
    private ControlScheme? _activeControlScheme;

    public ActionMapService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public event Action<string>? ActionMapEnabled;
    public event Action<string>? ActionMapDisabled;
    public event Action<ControlScheme>? ControlSchemeChanged;

    public async Task LoadAssetAsync(InputAsset asset)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));

        _logger.LogInformation("Loading input asset: {AssetName}", asset.Name);

        // Clear existing configuration
        _actionMaps.Clear();
        _controlSchemes.Clear();
        _enabledMaps.Clear();

        // Load action maps
        foreach (var (name, actionMap) in asset.ActionMaps)
        {
            _actionMaps[name] = actionMap;
            if (actionMap.IsEnabled)
            {
                _enabledMaps.Add(name);
            }
            _logger.LogDebug("Loaded action map: {MapName} (Enabled: {IsEnabled})", name, actionMap.IsEnabled);
        }

        // Load control schemes
        foreach (var (name, scheme) in asset.ControlSchemes)
        {
            _controlSchemes[name] = scheme;
            if (scheme.IsActive)
            {
                _activeControlScheme = scheme;
            }
            _logger.LogDebug("Loaded control scheme: {SchemeName} (Active: {IsActive})", name, scheme.IsActive);
        }

        _currentAsset = asset;
        _logger.LogInformation("Input asset loaded successfully with {MapCount} action maps and {SchemeCount} control schemes",
            asset.ActionMaps.Count, asset.ControlSchemes.Count);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Load input asset from Unity-compatible JSON file.
    /// </summary>
    /// <param name="jsonFilePath">Path to JSON file.</param>
    public async Task LoadAssetFromJsonAsync(string jsonFilePath)
    {
        if (string.IsNullOrEmpty(jsonFilePath))
            throw new ArgumentException("JSON file path cannot be null or empty", nameof(jsonFilePath));

        _logger.LogInformation("Loading input asset from JSON: {FilePath}", jsonFilePath);

        try
        {
            var asset = await InputAssetConverter.LoadFromFileAsync(jsonFilePath);
            await LoadAssetAsync(asset);

            _logger.LogInformation("Successfully loaded input asset from JSON file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load input asset from JSON file: {FilePath}", jsonFilePath);
            throw;
        }
    }

    /// <summary>
    /// Load input asset from Unity-compatible JSON string.
    /// </summary>
    /// <param name="json">JSON string.</param>
    public async Task LoadAssetFromJsonStringAsync(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("JSON string cannot be null or empty", nameof(json));

        _logger.LogInformation("Loading input asset from JSON string");

        try
        {
            var asset = InputAssetConverter.FromJson(json);
            await LoadAssetAsync(asset);

            _logger.LogInformation("Successfully loaded input asset from JSON string");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load input asset from JSON string");
            throw;
        }
    }

    /// <summary>
    /// Save current input asset to Unity-compatible JSON file.
    /// </summary>
    /// <param name="jsonFilePath">Path to save JSON file.</param>
    public async Task SaveAssetToJsonAsync(string jsonFilePath)
    {
        if (_currentAsset == null)
            throw new InvalidOperationException("No input asset loaded");

        if (string.IsNullOrEmpty(jsonFilePath))
            throw new ArgumentException("JSON file path cannot be null or empty", nameof(jsonFilePath));

        _logger.LogInformation("Saving input asset to JSON: {FilePath}", jsonFilePath);

        try
        {
            await InputAssetConverter.SaveToFileAsync(_currentAsset, jsonFilePath);
            _logger.LogInformation("Successfully saved input asset to JSON file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save input asset to JSON file: {FilePath}", jsonFilePath);
            throw;
        }
    }

    public void EnableActionMap(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
            throw new ArgumentException("Map name cannot be null or empty", nameof(mapName));

        if (!_actionMaps.ContainsKey(mapName))
        {
            _logger.LogWarning("Attempted to enable unknown action map: {MapName}", mapName);
            return;
        }

        if (_enabledMaps.Add(mapName))
        {
            _logger.LogInformation("Enabled action map: {MapName}", mapName);
            ActionMapEnabled?.Invoke(mapName);

            // Publish event
            _eventBus.PublishAsync(new InputScopePushedEvent(mapName)).GetAwaiter().GetResult();
        }
    }

    public void DisableActionMap(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
            throw new ArgumentException("Map name cannot be null or empty", nameof(mapName));

        if (_enabledMaps.Remove(mapName))
        {
            _logger.LogInformation("Disabled action map: {MapName}", mapName);
            ActionMapDisabled?.Invoke(mapName);

            // Publish event
            _eventBus.PublishAsync(new InputScopePoppedEvent(mapName)).GetAwaiter().GetResult();
        }
    }

    public void SwitchControlScheme(string schemeName)
    {
        if (string.IsNullOrEmpty(schemeName))
            throw new ArgumentException("Scheme name cannot be null or empty", nameof(schemeName));

        if (!_controlSchemes.TryGetValue(schemeName, out var scheme))
        {
            _logger.LogWarning("Attempted to switch to unknown control scheme: {SchemeName}", schemeName);
            return;
        }

        // Deactivate current scheme
        if (_activeControlScheme != null)
        {
            _controlSchemes[_activeControlScheme.Name] = _activeControlScheme with { IsActive = false };
        }

        // Activate new scheme
        _activeControlScheme = scheme with { IsActive = true };
        _controlSchemes[schemeName] = _activeControlScheme;

        _logger.LogInformation("Switched to control scheme: {SchemeName}", schemeName);
        ControlSchemeChanged?.Invoke(_activeControlScheme);
    }

    public IReadOnlyCollection<ActionMapType> GetEnabledActionMaps()
    {
        return _enabledMaps
            .Where(name => _actionMaps.ContainsKey(name))
            .Select(name => _actionMaps[name])
            .ToList()
            .AsReadOnly();
    }

    public ControlScheme? GetActiveControlScheme()
    {
        return _activeControlScheme;
    }

    public InputActionDefinition? FindAction(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            return null;

        // Search through enabled action maps
        foreach (var mapName in _enabledMaps)
        {
            if (_actionMaps.TryGetValue(mapName, out var actionMap))
            {
                var action = actionMap.GetAction(actionName);
                if (action != null)
                {
                    return action;
                }
            }
        }

        return null;
    }

    public InputActionDefinition? FindAction(string mapName, string actionName)
    {
        if (string.IsNullOrEmpty(mapName) || string.IsNullOrEmpty(actionName))
            return null;

        if (_actionMaps.TryGetValue(mapName, out var actionMap))
        {
            return actionMap.GetAction(actionName);
        }

        return null;
    }

    public IDisposable RegisterActionCallback(string actionName, Action<InputActionContext> callback)
    {
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentException("Action name cannot be null or empty", nameof(actionName));
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var actionCallback = new ActionCallback(actionName, null, callback);

        if (!_actionCallbacks.ContainsKey(actionName))
        {
            _actionCallbacks[actionName] = new List<ActionCallback>();
        }

        _actionCallbacks[actionName].Add(actionCallback);

        _logger.LogDebug("Registered callback for action: {ActionName}", actionName);

        return new CallbackDisposable(() => RemoveCallback(actionName, actionCallback));
    }

    public IDisposable RegisterActionCallback(string mapName, string actionName, Action<InputActionContext> callback)
    {
        if (string.IsNullOrEmpty(mapName))
            throw new ArgumentException("Map name cannot be null or empty", nameof(mapName));
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentException("Action name cannot be null or empty", nameof(actionName));
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var key = $"{mapName}.{actionName}";
        var actionCallback = new ActionCallback(actionName, mapName, callback);

        if (!_actionCallbacks.ContainsKey(key))
        {
            _actionCallbacks[key] = new List<ActionCallback>();
        }

        _actionCallbacks[key].Add(actionCallback);

        _logger.LogDebug("Registered callback for action: {MapName}.{ActionName}", mapName, actionName);

        return new CallbackDisposable(() => RemoveCallback(key, actionCallback));
    }

    /// <summary>
    /// Process a raw input event and trigger appropriate action callbacks.
    /// This method should be called by the input system when raw input is received.
    /// </summary>
    /// <param name="rawInput">Raw input event.</param>
    /// <param name="phase">Input action phase.</param>
    public void ProcessInput(RawKeyEvent rawInput, InputActionPhase phase = InputActionPhase.Performed)
    {
        if (rawInput == null) return;

        // Find matching actions in enabled action maps
        foreach (var mapName in _enabledMaps)
        {
            if (!_actionMaps.TryGetValue(mapName, out var actionMap)) continue;

            foreach (var (actionName, actionDef) in actionMap.Actions)
            {
                if (!actionDef.IsEnabled) continue;

                // Check if any binding matches the raw input
                if (DoesBindingMatch(actionDef.Bindings, rawInput))
                {
                    var context = new InputActionContext(actionDef, actionMap, rawInput, phase);
                    TriggerActionCallbacks(actionName, mapName, context);
                }
            }
        }
    }

    private bool DoesBindingMatch(IReadOnlyList<InputBinding> bindings, RawKeyEvent rawInput)
    {
        foreach (var binding in bindings)
        {
            if (IsBindingMatch(binding, rawInput))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBindingMatch(InputBinding binding, RawKeyEvent rawInput)
    {
        // Extract key from binding path (e.g., "Keyboard/w" -> "w")
        var bindingKey = ExtractKeyFromPath(binding.Path);
        if (bindingKey == null) return false;

        // Check key match
        if (!string.Equals(bindingKey, rawInput.Key, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check modifiers match
        return ModifiersMatch(binding.Modifiers, rawInput.Modifiers);
    }

    private string? ExtractKeyFromPath(string path)
    {
        // Handle paths like "Keyboard/w", "Mouse/leftButton"
        var parts = path.Split('/');
        return parts.Length >= 2 ? parts[1] : null;
    }

    private bool ModifiersMatch(string bindingModifiers, string inputModifiers)
    {
        // Simple modifier matching - can be enhanced for complex scenarios
        if (string.IsNullOrEmpty(bindingModifiers) && string.IsNullOrEmpty(inputModifiers))
            return true;

        return string.Equals(bindingModifiers, inputModifiers, StringComparison.OrdinalIgnoreCase);
    }

    private void TriggerActionCallbacks(string actionName, string mapName, InputActionContext context)
    {
        // Trigger global action callbacks
        if (_actionCallbacks.TryGetValue(actionName, out var globalCallbacks))
        {
            foreach (var callback in globalCallbacks)
            {
                try
                {
                    callback.Callback(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in action callback for {ActionName}", actionName);
                }
            }
        }

        // Trigger map-specific callbacks
        var mapKey = $"{mapName}.{actionName}";
        if (_actionCallbacks.TryGetValue(mapKey, out var mapCallbacks))
        {
            foreach (var callback in mapCallbacks)
            {
                try
                {
                    callback.Callback(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in action callback for {MapName}.{ActionName}", mapName, actionName);
                }
            }
        }

        // Publish action triggered event
        _eventBus.PublishAsync(new InputActionTriggeredEvent(actionName)).GetAwaiter().GetResult();
    }

    private void RemoveCallback(string key, ActionCallback callback)
    {
        if (_actionCallbacks.TryGetValue(key, out var callbacks))
        {
            callbacks.Remove(callback);
            if (callbacks.Count == 0)
            {
                _actionCallbacks.Remove(key);
            }
        }
    }

    private record ActionCallback(string ActionName, string? MapName, Action<InputActionContext> Callback);

    private class CallbackDisposable : IDisposable
    {
        private readonly Action _disposeAction;
        private bool _disposed;

        public CallbackDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposeAction();
                _disposed = true;
            }
        }
    }
}
