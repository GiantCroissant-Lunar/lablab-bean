namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a trade is completed with a merchant
/// </summary>
public record TradeCompletedEvent(int PlayerEntityId, int MerchantEntityId, int GoldSpent);
