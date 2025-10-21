namespace LablabBean.Contracts.Input;

/// <summary>
/// Raw keyboard event from the platform.
/// </summary>
/// <param name="Key">Key code (e.g., "W", "ArrowUp", "Escape").</param>
/// <param name="Modifiers">Modifier keys (e.g., "Ctrl", "Shift", "Alt").</param>
public record RawKeyEvent(string Key, string Modifiers = "");

/// <summary>
/// Input event with command and metadata.
/// </summary>
/// <param name="Command">Input command with type and data.</param>
/// <param name="Timestamp">When the input occurred.</param>
public record InputEvent(InputCommand Command, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Convenience constructor that sets timestamp to current UTC time.
    /// </summary>
    /// <param name="command">Input command.</param>
    public InputEvent(InputCommand command)
        : this(command, DateTimeOffset.UtcNow)
    {
    }
}

/// <summary>
/// Input command with type and optional data.
/// </summary>
/// <param name="Type">Command type (e.g., "Move", "Attack", "OpenInventory").</param>
/// <param name="Key">Raw key that triggered the command.</param>
/// <param name="Metadata">Optional metadata dictionary.</param>
public record InputCommand(
    string Type,
    string Key,
    IReadOnlyDictionary<string, object>? Metadata = null
);

/// <summary>
/// Logical input action mapped from raw keys.
/// </summary>
/// <param name="Name">Action name (e.g., "MoveNorth", "Attack").</param>
/// <param name="Metadata">Optional metadata for the action.</param>
public record InputAction(
    string Name,
    IReadOnlyDictionary<string, object>? Metadata = null
);
