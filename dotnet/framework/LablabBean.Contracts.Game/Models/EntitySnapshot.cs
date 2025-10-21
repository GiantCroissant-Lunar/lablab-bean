namespace LablabBean.Contracts.Game.Models;

/// <summary>
/// Immutable snapshot of an entity's state.
/// </summary>
public record EntitySnapshot(
    Guid Id,
    string Type,
    Position Position,
    int Health,
    int MaxHealth,
    IReadOnlyDictionary<string, object> Properties
);
