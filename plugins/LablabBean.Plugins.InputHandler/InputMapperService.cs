using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.Mapper;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.InputHandler;

/// <summary>
/// Input mapper service for action mapping.
/// </summary>
public class InputMapperService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, InputAction> _actions = new();
    private readonly Dictionary<string, string> _keyToAction = new();

    public InputMapperService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Map(RawKeyEvent rawKey)
    {
        if (rawKey == null) throw new ArgumentNullException(nameof(rawKey));

        var keyString = GetKeyString(rawKey);
        if (_keyToAction.TryGetValue(keyString, out var actionName))
        {
            _logger.LogDebug("Mapped key {Key} to action {Action}", keyString, actionName);
            
            // Publish event
            _eventBus.PublishAsync(new InputActionTriggeredEvent(actionName)).GetAwaiter().GetResult();
        }
        else
        {
            _logger.LogDebug("No mapping found for key: {Key}", keyString);
        }
    }

    public bool TryGetAction(string actionName, out InputAction action)
    {
        return _actions.TryGetValue(actionName, out action!);
    }

    public void RegisterMapping(string actionName, RawKeyEvent key)
    {
        if (string.IsNullOrEmpty(actionName)) 
            throw new ArgumentException("Action name cannot be null or empty", nameof(actionName));
        if (key == null) 
            throw new ArgumentNullException(nameof(key));

        var keyString = GetKeyString(key);
        var action = new InputAction(actionName);

        _actions[actionName] = action;
        _keyToAction[keyString] = actionName;

        _logger.LogInformation("Registered mapping: {Key} -> {Action}", keyString, actionName);
    }

    public void UnregisterMapping(string actionName)
    {
        if (_actions.Remove(actionName))
        {
            // Remove from key mappings
            var keysToRemove = _keyToAction
                .Where(kvp => kvp.Value == actionName)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _keyToAction.Remove(key);
            }

            _logger.LogInformation("Unregistered mapping: {Action}", actionName);
        }
    }

    public IReadOnlyCollection<string> GetActionNames()
    {
        return _actions.Keys.ToList().AsReadOnly();
    }

    private static string GetKeyString(RawKeyEvent key)
    {
        return string.IsNullOrEmpty(key.Modifiers) 
            ? key.Key 
            : $"{key.Modifiers}+{key.Key}";
    }
}
