using LablabBean.Contracts.UI.Models;

namespace LablabBean.Contracts.UI.Events;

/// <summary>
/// Published when user input is received.
/// </summary>
public record InputReceivedEvent(
    InputCommand Command,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public InputReceivedEvent(InputCommand command)
        : this(command, DateTimeOffset.UtcNow)
    {
    }
}
