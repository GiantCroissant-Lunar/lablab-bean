using System;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Examples;

/// <summary>
/// Example merchant NPC with relationship-based pricing
/// Demonstrates how to use memory system for dynamic pricing
/// </summary>
public class RelationshipBasedMerchant
{
    private readonly MemoryEnhancedNPCService _npcService;
    private readonly ILogger<RelationshipBasedMerchant> _logger;

    public RelationshipBasedMerchant(
        MemoryEnhancedNPCService npcService,
        ILogger<RelationshipBasedMerchant> logger)
    {
        _npcService = npcService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates item price based on relationship level
    /// </summary>
    public async Task<int> GetPriceAsync(
        string playerId,
        string npcId,
        int basePrice)
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);

        var discount = insights.RelationshipLevel switch
        {
            RelationshipLevel.Stranger => 0.0,
            RelationshipLevel.Acquaintance => 0.05,    // 5% discount
            RelationshipLevel.Friend => 0.10,          // 10% discount
            RelationshipLevel.GoodFriend => 0.15,      // 15% discount
            RelationshipLevel.CloseFriend => 0.20,     // 20% discount
            RelationshipLevel.TrustedFriend => 0.25,   // 25% discount
            _ => 0.0
        };

        var finalPrice = (int)(basePrice * (1.0 - discount));

        _logger.LogInformation(
            "Price for player {PlayerId} with {NpcId}: {BasePrice} -> {FinalPrice} ({Discount}% off, {Level})",
            playerId, npcId, basePrice, finalPrice, (int)(discount * 100), insights.RelationshipLevel);

        return finalPrice;
    }

    /// <summary>
    /// Determines if NPC will offer exclusive items
    /// </summary>
    public async Task<bool> CanAccessExclusiveItemsAsync(
        string playerId,
        string npcId,
        RelationshipLevel minimumLevel = RelationshipLevel.GoodFriend)
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
        return insights.RelationshipLevel >= minimumLevel;
    }

    /// <summary>
    /// Gets a personalized greeting based on relationship
    /// </summary>
    public async Task<string> GetPersonalizedGreetingAsync(
        string playerId,
        string npcId,
        string npcName)
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
        var recentMemories = await _npcService.GetRecentDialogueHistoryAsync(playerId, limit: 1);

        var greeting = insights.RelationshipLevel switch
        {
            RelationshipLevel.Stranger => $"Welcome, traveler. I'm {npcName}.",
            RelationshipLevel.Acquaintance => $"Oh, hello again. Back so soon?",
            RelationshipLevel.Friend => $"Good to see you, friend! How can I help?",
            RelationshipLevel.GoodFriend => $"Hey there! Always a pleasure!",
            RelationshipLevel.CloseFriend => $"My friend! Come in, come in!",
            RelationshipLevel.TrustedFriend => $"Ah, my most trusted friend! For you, anything!",
            _ => $"Hello."
        };

        // Add context from recent interactions
        if (recentMemories.Count > 0 && insights.InteractionCount > 3)
        {
            var lastInteraction = recentMemories[0];
            var timeSince = DateTime.UtcNow - lastInteraction.Memory.Timestamp;

            if (timeSince.TotalMinutes < 30)
            {
                greeting += " Back already?";
            }
            else if (timeSince.TotalDays > 7)
            {
                greeting += " It's been a while!";
            }
        }

        return greeting;
    }

    /// <summary>
    /// Example: Complete merchant interaction flow
    /// </summary>
    public async Task<MerchantTransaction> HandlePurchaseAsync(
        Entity player,
        Entity merchant,
        string playerId,
        string itemName,
        int basePrice)
    {
        var npc = merchant.Get<Components.NPC>();

        // Get relationship insights
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npc.Id);

        // Calculate price
        var finalPrice = await GetPriceAsync(playerId, npc.Id, basePrice);

        // Get greeting
        var greeting = await GetPersonalizedGreetingAsync(playerId, npc.Id, npc.Name);

        return new MerchantTransaction
        {
            ItemName = itemName,
            BasePrice = basePrice,
            FinalPrice = finalPrice,
            DiscountPercent = (int)(((basePrice - finalPrice) / (double)basePrice) * 100),
            RelationshipLevel = insights.RelationshipLevel,
            InteractionCount = insights.InteractionCount,
            Greeting = greeting
        };
    }
}

/// <summary>
/// Result of a merchant transaction
/// </summary>
public record MerchantTransaction
{
    public required string ItemName { get; init; }
    public required int BasePrice { get; init; }
    public required int FinalPrice { get; init; }
    public required int DiscountPercent { get; init; }
    public required RelationshipLevel RelationshipLevel { get; init; }
    public required int InteractionCount { get; init; }
    public required string Greeting { get; init; }
}
