namespace LablabBean.Contracts.UI.Models;

/// <summary>
/// User input command.
/// </summary>
public record InputCommand(
    InputType Type,
    string Key,
    IReadOnlyDictionary<string, object>? Metadata = null
);

/// <summary>
/// Input types.
/// </summary>
public enum InputType
{
    Movement,
    Action,
    Menu,
    System
}
