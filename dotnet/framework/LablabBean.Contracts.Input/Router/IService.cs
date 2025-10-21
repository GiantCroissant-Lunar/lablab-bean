namespace LablabBean.Contracts.Input.Router;

/// <summary>
/// Input router service for scope-based input routing.
/// Routes input events to the topmost scope in the stack.
/// </summary>
/// <typeparam name="TInputEvent">Type of input event to route.</typeparam>
public interface IService<TInputEvent> where TInputEvent : class
{
    /// <summary>
    /// Push a new input scope onto the stack.
    /// The scope will receive all input until it is popped.
    /// </summary>
    /// <param name="scope">Input scope to push.</param>
    /// <returns>Disposable that pops the scope when disposed.</returns>
    IDisposable PushScope(IInputScope<TInputEvent> scope);

    /// <summary>
    /// Dispatch an input event to the topmost scope.
    /// </summary>
    /// <param name="inputEvent">Input event to dispatch.</param>
    void Dispatch(TInputEvent inputEvent);

    /// <summary>
    /// Get the topmost scope in the stack, or null if stack is empty.
    /// </summary>
    IInputScope<TInputEvent>? Top { get; }
}

/// <summary>
/// Input scope that handles input events.
/// </summary>
/// <typeparam name="TInputEvent">Type of input event to handle.</typeparam>
public interface IInputScope<TInputEvent> where TInputEvent : class
{
    /// <summary>
    /// Handle an input event.
    /// </summary>
    /// <param name="inputEvent">Input event to handle.</param>
    /// <returns>Task that completes when handling is done.</returns>
    Task HandleAsync(TInputEvent inputEvent);

    /// <summary>
    /// Name of the scope for debugging.
    /// </summary>
    string Name { get; }
}
