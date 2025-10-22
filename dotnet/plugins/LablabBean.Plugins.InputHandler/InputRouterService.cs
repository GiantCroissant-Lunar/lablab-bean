using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.Router;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.InputHandler;

/// <summary>
/// Input router service with scope stack.
/// </summary>
public class InputRouterService<TInputEvent> : IService<TInputEvent> where TInputEvent : class
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Stack<IInputScope<TInputEvent>> _scopeStack = new();

    public IInputScope<TInputEvent>? Top => _scopeStack.Count > 0 ? _scopeStack.Peek() : null;

    public InputRouterService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDisposable PushScope(IInputScope<TInputEvent> scope)
    {
        if (scope == null) throw new ArgumentNullException(nameof(scope));

        _scopeStack.Push(scope);
        _logger.LogDebug("Pushed input scope: {ScopeName} (depth: {Depth})", 
            scope.Name, _scopeStack.Count);

        // Publish event
        _eventBus.PublishAsync(new InputScopePushedEvent(scope.Name)).GetAwaiter().GetResult();

        return new ScopeDisposer(this, scope);
    }

    public void Dispatch(TInputEvent inputEvent)
    {
        if (inputEvent == null) throw new ArgumentNullException(nameof(inputEvent));

        var scope = Top;
        if (scope != null)
        {
            _logger.LogDebug("Dispatching input to scope: {ScopeName}", scope.Name);
            scope.HandleAsync(inputEvent).GetAwaiter().GetResult();
        }
        else
        {
            _logger.LogWarning("No input scope to handle event");
        }
    }

    private void PopScope(IInputScope<TInputEvent> expectedScope)
    {
        if (_scopeStack.Count == 0)
        {
            _logger.LogWarning("Attempted to pop scope but stack is empty");
            return;
        }

        var actualScope = _scopeStack.Pop();
        if (!ReferenceEquals(actualScope, expectedScope))
        {
            _logger.LogWarning("Scope mismatch during pop. Expected: {Expected}, Actual: {Actual}",
                expectedScope.Name, actualScope.Name);
        }

        _logger.LogDebug("Popped input scope: {ScopeName} (depth: {Depth})", 
            actualScope.Name, _scopeStack.Count);

        // Publish event
        _eventBus.PublishAsync(new InputScopePoppedEvent(actualScope.Name)).GetAwaiter().GetResult();
    }

    private class ScopeDisposer : IDisposable
    {
        private readonly InputRouterService<TInputEvent> _service;
        private readonly IInputScope<TInputEvent> _scope;
        private bool _disposed;

        public ScopeDisposer(InputRouterService<TInputEvent> service, IInputScope<TInputEvent> scope)
        {
            _service = service;
            _scope = scope;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _service.PopScope(_scope);
                _disposed = true;
            }
        }
    }
}
