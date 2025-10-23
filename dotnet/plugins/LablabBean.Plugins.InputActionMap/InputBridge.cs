using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.ActionMap;
using LablabBean.Contracts.Input.Mapper;
using LablabBean.Contracts.Input.Router;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.InputActionMap;

/// <summary>
/// Bridge that connects the ActionMap service with the existing InputMapper service.
/// Forwards raw input from the mapper to the action map system.
/// </summary>
public class InputBridge : IDisposable
{
    private readonly ActionMapService _actionMapService;
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;
    private bool _disposed;

    public InputBridge(ActionMapService actionMapService, IEventBus eventBus, ILogger logger)
    {
        _actionMapService = actionMapService ?? throw new ArgumentNullException(nameof(actionMapService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to input action triggered events to forward to action map system
        _eventBus.Subscribe<InputActionTriggeredEvent>(OnInputActionTriggered);

        _logger.LogDebug("Input bridge initialized");
    }

    /// <summary>
    /// Process raw input and forward to action map service.
    /// This method should be called by the input mapper when raw input is received.
    /// </summary>
    /// <param name="rawInput">Raw input event.</param>
    /// <param name="phase">Input phase (default: Performed).</param>
    public void ProcessRawInput(RawKeyEvent rawInput, InputActionPhase phase = InputActionPhase.Performed)
    {
        if (_disposed) return;

        try
        {
            _actionMapService.ProcessInput(rawInput, phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing raw input in action map bridge");
        }
    }

    /// <summary>
    /// Create an input scope that forwards input to the action map system.
    /// This can be used with the existing Router service.
    /// </summary>
    /// <param name="scopeName">Name of the input scope.</param>
    /// <returns>Input scope that bridges to action map system.</returns>
    public IInputScope<InputEvent> CreateActionMapScope(string scopeName)
    {
        return new ActionMapInputScope(scopeName, this, _logger);
    }

    private async Task OnInputActionTriggered(InputActionTriggeredEvent evt)
    {
        _logger.LogTrace("Input action triggered: {ActionName} at {Timestamp}", evt.ActionName, evt.Timestamp);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Note: IEventBus doesn't support unsubscribe in current version
            // Subscribers live for the lifetime of the application
            _disposed = true;
        }
    }

    /// <summary>
    /// Input scope implementation that bridges InputEvent to ActionMap system.
    /// </summary>
    private class ActionMapInputScope : IInputScope<InputEvent>
    {
        private readonly InputBridge _bridge;
        private readonly ILogger _logger;

        public string Name { get; }

        public ActionMapInputScope(string name, InputBridge bridge, ILogger logger)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(InputEvent inputEvent)
        {
            if (inputEvent?.Command == null) return;

            try
            {
                // Convert InputEvent to RawKeyEvent for action map processing
                var rawKey = new RawKeyEvent(inputEvent.Command.Key, ""); // Modifiers could be extracted from metadata
                _bridge.ProcessRawInput(rawKey, InputActionPhase.Performed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling input event in action map scope: {ScopeName}", Name);
            }

            await Task.CompletedTask;
        }
    }
}
