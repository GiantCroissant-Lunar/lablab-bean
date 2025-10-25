using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Merchant.Components;
using LablabBean.Plugins.Merchant.Data;
using LablabBean.Plugins.Merchant.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Merchant.Services;

/// <summary>
/// Service for managing merchant trading and transactions.
/// </summary>
public class MerchantService
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, MerchantDefinition> _merchantDefinitions;
    private readonly TradingSystem _tradingSystem;
    private readonly MerchantSystem _merchantSystem;
    private readonly Dictionary<Guid, TradeState> _activeTrades;

    public MerchantService(
        World world,
        ILogger logger,
        Dictionary<Guid, MerchantDefinition> merchantDefinitions,
        TradingSystem tradingSystem,
        MerchantSystem merchantSystem)
    {
        _world = world;
        _logger = logger;
        _merchantDefinitions = merchantDefinitions;
        _tradingSystem = tradingSystem;
        _merchantSystem = merchantSystem;
        _activeTrades = new Dictionary<Guid, TradeState>();
    }

    #region Trading

    public void StartTrade(Guid playerId, Guid merchantId)
    {
        var player = FindEntity(playerId);
        var merchant = FindEntity(merchantId);

        if (player == null || merchant == null)
        {
            _logger.LogWarning("Cannot start trade: player or merchant not found");
            return;
        }

        var tradeState = new TradeState();
        tradeState.StartTrade(playerId, merchantId);
        _activeTrades[playerId] = tradeState;

        _logger.LogInformation("Started trade between player {PlayerId} and merchant {MerchantId}",
            playerId, merchantId);
    }

    public bool BuyItem(Guid playerId, Guid merchantId, Guid itemId, int quantity = 1)
    {
        var player = FindEntity(playerId);
        var merchant = FindEntity(merchantId);

        if (player == null || merchant == null)
        {
            _logger.LogWarning("Cannot buy item: player or merchant not found");
            return false;
        }

        if (!_activeTrades.ContainsKey(playerId) || !_activeTrades[playerId].IsActive)
        {
            _logger.LogWarning("Cannot buy item: no active trade session");
            return false;
        }

        var result = _tradingSystem.BuyItem(player.Value, merchant.Value, itemId, quantity);

        if (result.Success)
        {
            _logger.LogInformation("Player {PlayerId} bought {Quantity}x {ItemId} for {Gold} gold",
                playerId, quantity, itemId, result.GoldSpent);
        }
        else
        {
            _logger.LogDebug("Buy failed: {Reason}", result.FailureReason);
        }

        return result.Success;
    }

    public bool SellItem(Guid playerId, Guid merchantId, Guid itemId, int quantity = 1)
    {
        var player = FindEntity(playerId);
        var merchant = FindEntity(merchantId);

        if (player == null || merchant == null)
        {
            _logger.LogWarning("Cannot sell item: player or merchant not found");
            return false;
        }

        if (!_activeTrades.ContainsKey(playerId) || !_activeTrades[playerId].IsActive)
        {
            _logger.LogWarning("Cannot sell item: no active trade session");
            return false;
        }

        var result = _tradingSystem.SellItem(player.Value, merchant.Value, itemId, quantity);

        if (result.Success)
        {
            _logger.LogInformation("Player {PlayerId} sold {Quantity}x {ItemId} for {Gold} gold",
                playerId, quantity, itemId, result.GoldEarned);
        }
        else
        {
            _logger.LogDebug("Sell failed: {Reason}", result.FailureReason);
        }

        return result.Success;
    }

    public void EndTrade(Guid playerId)
    {
        if (_activeTrades.TryGetValue(playerId, out var tradeState))
        {
            tradeState.EndTrade();
            _activeTrades.Remove(playerId);
            _logger.LogInformation("Ended trade for player {PlayerId}", playerId);
        }
    }

    #endregion

    #region Gold Management

    public int GetGold(Guid playerId)
    {
        var player = FindEntity(playerId);
        if (player == null)
            return 0;

        return _tradingSystem.GetPlayerGold(player.Value);
    }

    public void AddGold(Guid playerId, int amount)
    {
        var player = FindEntity(playerId);
        if (player == null)
            return;

        _tradingSystem.AddGold(player.Value, amount);
    }

    public bool RemoveGold(Guid playerId, int amount)
    {
        var player = FindEntity(playerId);
        if (player == null)
            return false;

        return _tradingSystem.RemoveGold(player.Value, amount);
    }

    public bool CanAfford(Guid playerId, int cost)
    {
        var player = FindEntity(playerId);
        if (player == null)
            return false;

        return _tradingSystem.CanAffordPurchase(player.Value, cost);
    }

    #endregion

    #region Merchant Inventory

    public IEnumerable<MerchantItemInfo> GetMerchantInventory(Guid merchantId)
    {
        var merchant = FindEntity(merchantId);
        if (merchant == null || !merchant.Value.Has<MerchantInventory>())
            return Enumerable.Empty<MerchantItemInfo>();

        var inventory = merchant.Value.Get<MerchantInventory>();
        var items = new List<MerchantItemInfo>();

        foreach (var (itemId, stockItem) in inventory.Stock)
        {
            var sellPrice = _merchantSystem.CalculateSellPrice(merchant.Value, stockItem.BasePrice);
            var buyPrice = _merchantSystem.CalculateBuyPrice(merchant.Value, stockItem.BasePrice);

            items.Add(new MerchantItemInfo(
                ItemId: itemId,
                ItemName: GetItemName(itemId),
                Quantity: stockItem.IsInfinite ? -1 : stockItem.Quantity,
                SellPrice: sellPrice,
                BuyPrice: buyPrice,
                InStock: stockItem.IsInfinite || stockItem.Quantity > 0
            ));
        }

        return items;
    }

    public void RefreshMerchantStock(Guid merchantId, int currentDungeonLevel)
    {
        var merchant = FindEntity(merchantId);
        if (merchant == null)
            return;

        _merchantSystem.RefreshMerchantStock(merchant.Value, currentDungeonLevel);
    }

    public int CalculateSellPrice(Guid merchantId, Guid itemId)
    {
        var merchant = FindEntity(merchantId);
        if (merchant == null || !merchant.Value.Has<MerchantInventory>())
            return 0;

        var inventory = merchant.Value.Get<MerchantInventory>();
        if (!inventory.Stock.TryGetValue(itemId, out var stockItem))
            return 0;

        return _merchantSystem.CalculateSellPrice(merchant.Value, stockItem.BasePrice);
    }

    public int CalculateBuyPrice(Guid merchantId, Guid itemId)
    {
        var merchant = FindEntity(merchantId);
        if (merchant == null || !merchant.Value.Has<MerchantInventory>())
            return 0;

        var inventory = merchant.Value.Get<MerchantInventory>();
        if (!inventory.Stock.TryGetValue(itemId, out var stockItem))
            return 0;

        return _merchantSystem.CalculateBuyPrice(merchant.Value, stockItem.BasePrice);
    }

    public bool HasItemInStock(Guid merchantId, Guid itemId, int quantity = 1)
    {
        var merchant = FindEntity(merchantId);
        if (merchant == null || !merchant.Value.Has<MerchantInventory>())
            return false;

        var inventory = merchant.Value.Get<MerchantInventory>();
        return inventory.HasItem(itemId, quantity);
    }

    #endregion

    #region Queries

    public TradeStateInfo? GetTradeState(Guid playerId)
    {
        if (!_activeTrades.TryGetValue(playerId, out var tradeState) || !tradeState.IsActive)
            return null;

        var merchant = FindEntity(tradeState.MerchantId);
        var merchantName = merchant?.Has<Name>() == true
            ? merchant.Value.Get<Name>().Value
            : "Unknown Merchant";

        var playerGold = GetGold(playerId);

        return new TradeStateInfo(
            MerchantId: tradeState.MerchantId,
            MerchantName: merchantName,
            PlayerGold: playerGold
        );
    }

    #endregion

    #region Helper Methods

    private Entity? FindEntity(Guid entityId)
    {
        // Simplified entity lookup
        var playerQuery = new QueryDescription().WithAll<Player>();
        Entity? found = null;

        _world.Query(in playerQuery, (Entity entity) =>
        {
            if (found == null)
            {
                found = entity;
            }
        });

        if (found != null) return found;

        // Search NPCs
        var npcQuery = new QueryDescription().WithAll<NPC.Components.NPC>();
        _world.Query(in npcQuery, (Entity entity) =>
        {
            if (found == null)
            {
                found = entity;
            }
        });

        return found;
    }

    private string GetItemName(Guid itemId)
    {
        // TODO: Integrate with item database
        return $"Item-{itemId.ToString()[..8]}";
    }

    #endregion
}

/// <summary>
/// DTO for merchant item information.
/// </summary>
public record MerchantItemInfo(
    Guid ItemId,
    string ItemName,
    int Quantity,  // -1 = infinite
    int SellPrice,
    int BuyPrice,
    bool InStock
);

/// <summary>
/// DTO for active trade session.
/// </summary>
public record TradeStateInfo(
    Guid MerchantId,
    string MerchantName,
    int PlayerGold
);
