namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when an item is collected
/// </summary>
public record ItemCollectedEvent(int EntityId, string ItemId, int Quantity = 1);
