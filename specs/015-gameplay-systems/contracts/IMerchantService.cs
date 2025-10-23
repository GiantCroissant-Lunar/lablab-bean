// Plugin Service Contract: Merchant & Trading System
// Exposes trading, gold management, and merchant inventory via IPluginContext

using System;
using System.Collections.Generic;

namespace LablabBean.Plugins.Merchant.Contracts
{
    /// <summary>
    /// Service for managing trading and merchant interactions.
    /// Registered in IPluginContext as "MerchantService".
    /// </summary>
    public interface IMerchantService
    {
        // Trading

        /// <summary>
        /// Starts a trade session with a merchant NPC.
        /// </summary>
        void StartTrade(Guid playerId, Guid merchantId);

        /// <summary>
        /// Purchases an item from the merchant.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="merchantId">Merchant entity ID</param>
        /// <param name="itemId">Item to purchase</param>
        /// <param name="quantity">Number to buy</param>
        /// <returns>True if purchase successful</returns>
        bool BuyItem(Guid playerId, Guid merchantId, Guid itemId, int quantity = 1);

        /// <summary>
        /// Sells an item to the merchant.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="merchantId">Merchant entity ID</param>
        /// <param name="itemId">Item to sell</param>
        /// <param name="quantity">Number to sell</param>
        /// <returns>True if sale successful</returns>
        bool SellItem(Guid playerId, Guid merchantId, Guid itemId, int quantity = 1);

        /// <summary>
        /// Ends the current trade session.
        /// </summary>
        void EndTrade(Guid playerId);

        // Gold Management

        /// <summary>
        /// Gets player's current gold amount.
        /// </summary>
        int GetGold(Guid playerId);

        /// <summary>
        /// Adds gold to player inventory.
        /// </summary>
        void AddGold(Guid playerId, int amount);

        /// <summary>
        /// Removes gold from player inventory.
        /// </summary>
        /// <returns>True if player had enough gold</returns>
        bool RemoveGold(Guid playerId, int amount);

        /// <summary>
        /// Checks if player can afford a purchase.
        /// </summary>
        bool CanAfford(Guid playerId, int cost);

        // Merchant Inventory

        /// <summary>
        /// Gets all items available from a merchant.
        /// </summary>
        IEnumerable<MerchantItemInfo> GetMerchantInventory(Guid merchantId);

        /// <summary>
        /// Refreshes merchant stock (called on level change).
        /// </summary>
        void RefreshMerchantStock(Guid merchantId, int currentDungeonLevel);

        /// <summary>
        /// Calculates sell price for an item (merchant selling to player).
        /// </summary>
        int CalculateSellPrice(Guid merchantId, Guid itemId);

        /// <summary>
        /// Calculates buy price for an item (merchant buying from player).
        /// </summary>
        int CalculateBuyPrice(Guid merchantId, Guid itemId);

        // Queries

        /// <summary>
        /// Gets active trade state.
        /// </summary>
        TradeStateInfo? GetTradeState(Guid playerId);

        /// <summary>
        /// Checks if merchant has item in stock.
        /// </summary>
        bool HasItemInStock(Guid merchantId, Guid itemId, int quantity = 1);
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
}
