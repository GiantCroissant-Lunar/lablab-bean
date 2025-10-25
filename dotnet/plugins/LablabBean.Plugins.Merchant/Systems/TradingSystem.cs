using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Merchant.Components;
using LablabBean.Plugins.Merchant.Data;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.Merchant.Systems;

/// <summary>
/// System for handling buy/sell transactions between players and merchants.
/// </summary>
public class TradingSystem
{
    private readonly World _world;
    private readonly ILogger _logger;
    private readonly MerchantSystem _merchantSystem;

    public TradingSystem(World world, ILogger logger, MerchantSystem merchantSystem)
    {
        _world = world;
        _logger = logger;
        _merchantSystem = merchantSystem;
    }

    public TradeResult BuyItem(Entity player, Entity merchant, Guid itemId, int quantity = 1)
    {
        // Validate entities
        if (!player.Has<Gold>())
            return new TradeResult(false, "Player has no gold");

        if (!merchant.Has<MerchantInventory>())
            return new TradeResult(false, "Merchant has no inventory");

        ref var playerGold = ref player.Get<Gold>();
        ref var merchantInventory = ref merchant.Get<MerchantInventory>();

        // Check stock
        if (!merchantInventory.HasItem(itemId, quantity))
            return new TradeResult(false, "Item not in stock");

        var stockItem = merchantInventory.Stock[itemId];

        // Calculate price
        var totalPrice = _merchantSystem.CalculateSellPrice(merchant, stockItem.BasePrice) * quantity;

        // Check if player can afford
        if (!playerGold.Has(totalPrice))
            return new TradeResult(false, $"Insufficient gold (need {totalPrice})");

        // Process transaction
        if (!playerGold.TryRemove(totalPrice))
            return new TradeResult(false, "Failed to remove gold");

        if (!merchantInventory.RemoveStock(itemId, quantity))
        {
            // Refund on failure
            playerGold.Add(totalPrice);
            return new TradeResult(false, "Failed to remove from stock");
        }

        // Add item to player inventory
        // Note: This would integrate with Inventory plugin
        // For now, we just track the transaction

        _logger.LogInformation(
            "Player purchased {Quantity}x item {ItemId} for {Price} gold",
            quantity, itemId, totalPrice);

        return new TradeResult(
            true,
            GoldSpent: totalPrice,
            ItemsTraded: new List<Guid> { itemId }
        );
    }

    public TradeResult SellItem(Entity player, Entity merchant, Guid itemId, int quantity = 1)
    {
        // Validate entities
        if (!player.Has<Gold>())
            return new TradeResult(false, "Player has no gold component");

        if (!merchant.Has<MerchantInventory>())
            return new TradeResult(false, "Merchant has no inventory");

        // Note: Would check player inventory here
        // For now, assume player has the item

        ref var playerGold = ref player.Get<Gold>();
        var merchantInventory = merchant.Get<MerchantInventory>();

        // Calculate sell price (merchant buys at lower price)
        var basePrice = 100; // Would get from item definition
        var totalPrice = _merchantSystem.CalculateBuyPrice(merchant, basePrice) * quantity;

        // Process transaction
        playerGold.Add(totalPrice);

        // Remove from player inventory
        // Note: This would integrate with Inventory plugin

        // Optionally add to merchant stock
        // merchantInventory.AddStock(itemId, basePrice, quantity);

        _logger.LogInformation(
            "Player sold {Quantity}x item {ItemId} for {Price} gold",
            quantity, itemId, totalPrice);

        return new TradeResult(
            true,
            GoldEarned: totalPrice,
            ItemsTraded: new List<Guid> { itemId }
        );
    }

    public bool CanAffordPurchase(Entity player, int cost)
    {
        if (!player.Has<Gold>())
            return false;

        var gold = player.Get<Gold>();
        return gold.Has(cost);
    }

    public int GetPlayerGold(Entity player)
    {
        if (!player.Has<Gold>())
            return 0;

        var gold = player.Get<Gold>();
        return gold.Amount;
    }

    public void AddGold(Entity entity, int amount)
    {
        if (!entity.Has<Gold>())
        {
            entity.Add(new Gold(amount));
            return;
        }

        ref var gold = ref entity.Get<Gold>();
        gold.Add(amount);

        _logger.LogDebug("Added {Amount} gold to entity", amount);
    }

    public bool RemoveGold(Entity entity, int amount)
    {
        if (!entity.Has<Gold>())
            return false;

        ref var gold = ref entity.Get<Gold>();
        var removed = gold.TryRemove(amount);

        if (removed)
        {
            _logger.LogDebug("Removed {Amount} gold from entity", amount);
        }

        return removed;
    }
}
