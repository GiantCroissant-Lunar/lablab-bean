namespace LablabBean.Contracts.Input;

/// <summary>
/// Event published when an input action is triggered.
/// </summary>
/// <param name="ActionName">Name of the triggered action.</param>
/// <param name="Timestamp">When the action was triggered.</param>
public record InputActionTriggeredEvent(string ActionName, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="actionName">Name of the triggered action.</param>
    public InputActionTriggeredEvent(string actionName)
        : this(actionName, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when an input scope is pushed onto the stack.
/// </summary>
/// <param name="ScopeName">Name of the pushed scope.</param>
/// <param name="Timestamp">When the scope was pushed.</param>
public record InputScopePushedEvent(string ScopeName, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="scopeName">Name of the pushed scope.</param>
    public InputScopePushedEvent(string scopeName)
        : this(scopeName, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Event published when an input scope is popped from the stack.
/// </summary>
/// <param name="ScopeName">Name of the popped scope.</param>
/// <param name="Timestamp">When the scope was popped.</param>
public record InputScopePoppedEvent(string ScopeName, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="scopeName">Name of the popped scope.</param>
    public InputScopePoppedEvent(string scopeName)
        : this(scopeName, DateTimeOffset.UtcNow)
    {
    }
}
