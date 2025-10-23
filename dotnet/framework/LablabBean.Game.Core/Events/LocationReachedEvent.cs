namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a player reaches a specific location
/// </summary>
public record LocationReachedEvent(int EntityId, string LocationId, int X, int Y);
