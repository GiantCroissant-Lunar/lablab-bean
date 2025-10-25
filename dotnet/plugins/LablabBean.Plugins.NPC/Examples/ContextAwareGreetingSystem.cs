using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Examples;

/// <summary>
/// Context-aware greeting system that uses memory to create personalized NPC greetings
/// </summary>
public class ContextAwareGreetingSystem
{
    private readonly MemoryEnhancedNPCService _npcService;
    private readonly ILogger<ContextAwareGreetingSystem> _logger;

    public ContextAwareGreetingSystem(
        MemoryEnhancedNPCService npcService,
        ILogger<ContextAwareGreetingSystem> logger)
    {
        _npcService = npcService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a context-aware greeting based on player history
    /// </summary>
    public async Task<GreetingContext> GenerateGreetingAsync(
        string playerId,
        string npcId,
        string npcName,
        string npcRole = "merchant")
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
        var recentMemories = await _npcService.GetRecentDialogueHistoryAsync(playerId, limit: 5);

        var greeting = new StringBuilder();
        var context = new GreetingContext
        {
            PlayerId = playerId,
            NpcId = npcId,
            NpcName = npcName,
            RelationshipLevel = insights.RelationshipLevel,
            InteractionCount = insights.InteractionCount
        };

        // Base greeting by relationship
        greeting.Append(GetBaseGreeting(npcName, npcRole, insights.RelationshipLevel));

        // Add contextual information
        if (recentMemories.Count > 0)
        {
            var lastMemory = recentMemories.First();
            var timeSinceLastInteraction = DateTime.UtcNow - lastMemory.Memory.Timestamp;

            // Time-based context
            if (timeSinceLastInteraction.TotalMinutes < 10)
            {
                greeting.Append(" Still looking around?");
                context.ContextType = GreetingContextType.RecentInteraction;
            }
            else if (timeSinceLastInteraction.TotalHours < 1)
            {
                greeting.Append(" Back so soon?");
                context.ContextType = GreetingContextType.SameSession;
            }
            else if (timeSinceLastInteraction.TotalDays > 7)
            {
                greeting.Append(" It's been quite a while! I wondered if I'd see you again.");
                context.ContextType = GreetingContextType.LongAbsence;
            }

            // Topic-based context from recent memories
            if (recentMemories.Count >= 2)
            {
                var topics = recentMemories
                    .Select(m => ExtractTopic(m.Memory.Content))
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToList();

                if (topics.Count > 0)
                {
                    greeting.Append($" Still interested in {topics.First()}?");
                    context.RecentTopics.AddRange(topics);
                }
            }
        }
        else if (insights.InteractionCount == 0)
        {
            // First time meeting
            greeting.Append(" I don't believe we've met before.");
            context.ContextType = GreetingContextType.FirstMeeting;
        }

        // Special relationship messages
        if (insights.RelationshipLevel >= RelationshipLevel.GoodFriend)
        {
            greeting.Append(" You know you're always welcome here.");
        }

        context.GreetingText = greeting.ToString();
        context.Timestamp = DateTime.UtcNow;

        _logger.LogInformation(
            "Generated greeting for {PlayerId} with {NpcName}: {Greeting} (Level: {Level}, Interactions: {Count})",
            playerId, npcName, context.GreetingText, insights.RelationshipLevel, insights.InteractionCount);

        return context;
    }

    /// <summary>
    /// Gets base greeting based on relationship level
    /// </summary>
    private string GetBaseGreeting(string npcName, string npcRole, RelationshipLevel level)
    {
        return level switch
        {
            RelationshipLevel.Stranger => $"Greetings, traveler. I am {npcName}, {npcRole} of this establishment.",
            RelationshipLevel.Acquaintance => $"Hello there. {npcName} here.",
            RelationshipLevel.Friend => $"Ah, good to see you again, friend!",
            RelationshipLevel.GoodFriend => $"Welcome, my friend! How have you been?",
            RelationshipLevel.CloseFriend => $"My dear friend! Always a joy to see you!",
            RelationshipLevel.TrustedFriend => $"Ah, my most trusted companion! Come, come!",
            _ => $"Hello. {npcName} at your service."
        };
    }

    /// <summary>
    /// Extracts topic from memory content (simplified)
    /// </summary>
    private string ExtractTopic(string content)
    {
        // Simple keyword extraction - in production, use NLP
        if (content.Contains("weapon", StringComparison.OrdinalIgnoreCase))
            return "weapons";
        if (content.Contains("armor", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("shield", StringComparison.OrdinalIgnoreCase))
            return "armor";
        if (content.Contains("potion", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("health", StringComparison.OrdinalIgnoreCase))
            return "potions";
        if (content.Contains("quest", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("mission", StringComparison.OrdinalIgnoreCase))
            return "quests";

        return string.Empty;
    }

    /// <summary>
    /// Generates a farewell message based on context
    /// </summary>
    public async Task<string> GenerateFarewellAsync(
        string playerId,
        string npcId,
        bool purchaseMade = false)
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);

        var farewell = insights.RelationshipLevel switch
        {
            RelationshipLevel.Stranger => "Safe travels, stranger.",
            RelationshipLevel.Acquaintance => "Until next time.",
            RelationshipLevel.Friend => "Take care, friend!",
            RelationshipLevel.GoodFriend => "Stay safe out there, my friend!",
            RelationshipLevel.CloseFriend => "May fortune smile upon you, dear friend!",
            RelationshipLevel.TrustedFriend => "Until we meet again, trusted companion!",
            _ => "Farewell."
        };

        if (purchaseMade && insights.RelationshipLevel >= RelationshipLevel.Friend)
        {
            farewell += " Thank you for your patronage!";
        }

        return farewell;
    }
}

/// <summary>
/// Context information for a greeting
/// </summary>
public class GreetingContext
{
    public required string PlayerId { get; init; }
    public required string NpcId { get; init; }
    public required string NpcName { get; init; }
    public required RelationshipLevel RelationshipLevel { get; init; }
    public required int InteractionCount { get; init; }
    public string GreetingText { get; set; } = string.Empty;
    public GreetingContextType ContextType { get; set; } = GreetingContextType.Standard;
    public System.Collections.Generic.List<string> RecentTopics { get; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Types of greeting contexts
/// </summary>
public enum GreetingContextType
{
    Standard,
    FirstMeeting,
    RecentInteraction,
    SameSession,
    LongAbsence,
    RepeatVisitor
}
