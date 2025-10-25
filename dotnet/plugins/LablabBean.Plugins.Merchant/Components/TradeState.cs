namespace LablabBean.Plugins.Merchant.Components;

/// <summary>
/// Component tracking active trade session state.
/// </summary>
public class TradeState
{
    public Guid PlayerId { get; set; }
    public Guid MerchantId { get; set; }
    public DateTime StartTime { get; set; }
    public bool IsActive { get; set; }

    public TradeState()
    {
        IsActive = false;
    }

    public void StartTrade(Guid playerId, Guid merchantId)
    {
        PlayerId = playerId;
        MerchantId = merchantId;
        StartTime = DateTime.UtcNow;
        IsActive = true;
    }

    public void EndTrade()
    {
        IsActive = false;
    }
}
