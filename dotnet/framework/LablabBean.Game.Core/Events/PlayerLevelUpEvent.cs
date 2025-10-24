namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a player levels up
/// </summary>
public record PlayerLevelUpEvent(int EntityId, int NewLevel, int OldLevel);
