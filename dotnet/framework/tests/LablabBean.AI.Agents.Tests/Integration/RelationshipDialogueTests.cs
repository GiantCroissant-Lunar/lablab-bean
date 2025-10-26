using LablabBean.AI.Agents.Services;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Integration tests for relationship-aware dialogue scenarios.
/// Tests the full workflow: store interactions → retrieve context → influence dialogue.
/// </summary>
public class RelationshipDialogueTests : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private IMemoryService? _memoryService;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // Add Semantic Kernel with memory
        var kernelBuilder = Kernel.CreateBuilder();

        // Use in-memory configuration for testing
        kernelBuilder.Services.AddSingleton<IMemoryService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var logger = sp.GetRequiredService<ILogger<MemoryService>>();
            return new MemoryService(kernel, logger);
        });

        services.AddSingleton(kernelBuilder.Build());

        _serviceProvider = services.BuildServiceProvider();
        _memoryService = _serviceProvider.GetRequiredService<IMemoryService>();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Fact]
    public async Task RelationshipHistory_InfluencesNPCDialogue_WithMultipleInteractions()
    {
        // Arrange - Build a relationship history
        var playerId = "player_test_001";
        var npcId = "alice_npc_001";

        // Interaction 1: Positive collaboration
        await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Player helped Alice complete urgent project. Alice was grateful and impressed by player's skills.",
            Timestamp = DateTime.UtcNow.AddDays(-5)
        });

        // Interaction 2: Gift exchange
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Gift,
            Sentiment = "positive",
            Description = "Player gave Alice coffee beans from rare region. Alice loves coffee and was touched by thoughtfulness.",
            Timestamp = DateTime.UtcNow.AddDays(-3)
        });

        // Interaction 3: Quest collaboration
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Quest,
            Sentiment = "positive",
            Description = "Completed dangerous quest together. Alice trusts player completely after facing challenges together.",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        });

        // Act - Retrieve relationship context for new interaction
        var relationshipContext = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: npcId,
            entity2Id: playerId,
            query: "player asking for another favor",
            maxResults: 5
        );

        // Assert - Should retrieve relevant positive history
        Assert.NotEmpty(relationshipContext);
        Assert.True(relationshipContext.Count >= 2, $"Expected at least 2 memories, got {relationshipContext.Count}");

        // Verify context includes positive sentiment
        var contextText = string.Join(" ", relationshipContext.Select(r => r.Content));
        Assert.Contains("positive", contextText.ToLower());

        // Verify key relationship milestones present
        var hasCollaboration = relationshipContext.Any(r => r.Content.ToLower().Contains("project") || r.Content.ToLower().Contains("collaboration"));
        var hasGift = relationshipContext.Any(r => r.Content.ToLower().Contains("gift") || r.Content.ToLower().Contains("coffee"));
        var hasQuest = relationshipContext.Any(r => r.Content.ToLower().Contains("quest") || r.Content.ToLower().Contains("trust"));

        Assert.True(hasCollaboration || hasGift || hasQuest,
            "Relationship context should include at least one key interaction milestone");
    }

    [Fact]
    public async Task RelationshipHistory_DistinguishesBetweenDifferentNPCs()
    {
        // Arrange - Build relationships with two different NPCs
        var playerId = "player_test_002";
        var aliceId = "alice_npc_002";
        var bobId = "bob_npc_002";

        // Positive relationship with Alice
        await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = aliceId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Alice and player are great friends, always help each other",
            Timestamp = DateTime.UtcNow.AddDays(-2)
        });

        // Negative relationship with Bob
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = bobId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Betrayal,
            Sentiment = "negative",
            Description = "Bob feels betrayed by player, refuses to work together",
            Timestamp = DateTime.UtcNow.AddDays(-2)
        });

        // Act - Retrieve context for each NPC separately
        var aliceContext = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: aliceId,
            entity2Id: playerId,
            query: "interaction",
            maxResults: 5
        );

        var bobContext = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: bobId,
            entity2Id: playerId,
            query: "interaction",
            maxResults: 5
        );

        // Assert - Contexts should be different
        Assert.NotEmpty(aliceContext);
        Assert.NotEmpty(bobContext);

        var aliceText = string.Join(" ", aliceContext.Select(r => r.Content)).ToLower();
        var bobText = string.Join(" ", bobContext.Select(r => r.Content)).ToLower();

        // Alice context should be positive
        Assert.Contains("positive", aliceText);
        Assert.DoesNotContain("betrayal", aliceText);

        // Bob context should be negative
        Assert.Contains("negative", bobText);
        Assert.Contains("betrayal", bobText.ToLower());
    }

    [Fact]
    public async Task RelationshipHistory_FiltersBySentiment_ForContextualDialogue()
    {
        // Arrange - Build mixed sentiment history
        var playerId = "player_test_003";
        var npcId = "charlie_npc_003";

        await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Trade,
            Sentiment = "positive",
            Description = "Fair trade, both parties satisfied",
            Timestamp = DateTime.UtcNow.AddDays(-5)
        });

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Combat,
            Sentiment = "negative",
            Description = "Heated argument escalated to conflict",
            Timestamp = DateTime.UtcNow.AddDays(-3)
        });

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Conversation,
            Sentiment = "positive",
            Description = "Reconciliation conversation, apologies exchanged",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        });

        // Act - Retrieve only positive memories for friendly dialogue
        var positiveContext = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: npcId,
            entity2Id: playerId,
            query: "current relationship",
            maxResults: 5,
            sentiment: "positive"
        );

        // Assert - Should only include positive interactions
        Assert.NotEmpty(positiveContext);
        var contextText = string.Join(" ", positiveContext.Select(r => r.Content)).ToLower();
        Assert.Contains("positive", contextText);
        Assert.DoesNotContain("combat", contextText);
    }

    [Fact]
    public async Task RelationshipHistory_TracksEvolution_OverTime()
    {
        // Arrange - Simulate relationship evolution
        var playerId = "player_test_004";
        var npcId = "diana_npc_004";

        // Phase 1: Strangers (neutral)
        await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Conversation,
            Sentiment = "neutral",
            Description = "First meeting, polite small talk, no strong impression",
            Timestamp = DateTime.UtcNow.AddDays(-10)
        });

        // Phase 2: Building trust (positive)
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Quest,
            Sentiment = "positive",
            Description = "Player proved reliable in first quest together",
            Timestamp = DateTime.UtcNow.AddDays(-7)
        });

        // Phase 3: Close friends (very positive)
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = npcId,
            Entity2Id = playerId,
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Diana considers player a close friend, shares personal stories",
            Timestamp = DateTime.UtcNow.AddDays(-2)
        });

        // Act - Retrieve full history
        var history = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: npcId,
            entity2Id: playerId,
            query: "relationship development",
            maxResults: 10
        );

        // Assert - Should show progression
        Assert.NotEmpty(history);
        Assert.True(history.Count >= 2, "Should retrieve multiple stages of relationship");

        var combinedHistory = string.Join(" ", history.Select(r => r.Content)).ToLower();
        var hasEarlyStage = combinedHistory.Contains("first") || combinedHistory.Contains("meeting");
        var hasLaterStage = combinedHistory.Contains("friend") || combinedHistory.Contains("trust");

        Assert.True(hasEarlyStage || hasLaterStage, "History should reflect relationship stages");
    }

    [Fact]
    public async Task RelationshipHistory_HandlesBidirectionalRelationships()
    {
        // Arrange - Both entities initiate interactions
        var aliceId = "alice_npc_005";
        var bobId = "bob_npc_005";

        // Alice perspective
        await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = aliceId,
            Entity2Id = bobId,
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Alice asked Bob for help, Bob was supportive",
            Timestamp = DateTime.UtcNow.AddDays(-3)
        });

        // Bob perspective
        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = bobId,
            Entity2Id = aliceId,
            InteractionType = InteractionType.Gift,
            Sentiment = "positive",
            Description = "Bob gave Alice technical advice as gift",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        });

        // Act - Retrieve from both perspectives
        var alicePerspective = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: aliceId,
            entity2Id: bobId,
            query: "our relationship",
            maxResults: 5
        );

        var bobPerspective = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: bobId,
            entity2Id: aliceId,
            query: "our relationship",
            maxResults: 5
        );

        // Assert - Both should see shared relationship history
        Assert.NotEmpty(alicePerspective);
        Assert.NotEmpty(bobPerspective);

        // Both perspectives should contain information about the relationship
        var aliceText = string.Join(" ", alicePerspective.Select(r => r.Content)).ToLower();
        var bobText = string.Join(" ", bobPerspective.Select(r => r.Content)).ToLower();

        Assert.Contains("positive", aliceText);
        Assert.Contains("positive", bobText);
    }

    [Fact]
    public async Task RelationshipHistory_SupportsComplexInteractionTypes()
    {
        // Arrange - Test all interaction types
        var playerId = "player_test_006";
        var npcId = "eve_npc_006";

        var interactionTypes = new[]
        {
            (InteractionType.Conversation, "casual chat about weather"),
            (InteractionType.Trade, "traded rare items"),
            (InteractionType.Combat, "sparred in training"),
            (InteractionType.Collaboration, "worked on community project"),
            (InteractionType.Betrayal, "misunderstanding about resources"),
            (InteractionType.Gift, "exchanged holiday presents"),
            (InteractionType.Quest, "completed dungeon together")
        };

        foreach (var (type, description) in interactionTypes)
        {
            await _memoryService!.StoreRelationshipMemoryAsync(new RelationshipMemory
            {
                Entity1Id = npcId,
                Entity2Id = playerId,
                InteractionType = type,
                Sentiment = type == InteractionType.Betrayal ? "negative" : "positive",
                Description = description,
                Timestamp = DateTime.UtcNow.AddMinutes(-interactionTypes.ToList().IndexOf((type, description)))
            });
        }

        // Act - Retrieve history
        var history = await _memoryService!.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: npcId,
            entity2Id: playerId,
            query: "all our interactions",
            maxResults: 10
        );

        // Assert - Should retrieve diverse interaction types
        Assert.NotEmpty(history);
        Assert.True(history.Count >= 3, "Should retrieve multiple interaction types");

        var historyText = string.Join(" ", history.Select(r => r.Content)).ToLower();

        // Verify variety of interactions captured
        var capturedTypes = 0;
        if (historyText.Contains("conversation") || historyText.Contains("chat")) capturedTypes++;
        if (historyText.Contains("trade")) capturedTypes++;
        if (historyText.Contains("combat") || historyText.Contains("spar")) capturedTypes++;
        if (historyText.Contains("collaboration") || historyText.Contains("project")) capturedTypes++;
        if (historyText.Contains("gift") || historyText.Contains("present")) capturedTypes++;
        if (historyText.Contains("quest") || historyText.Contains("dungeon")) capturedTypes++;

        Assert.True(capturedTypes >= 2, $"Should capture multiple interaction types, found {capturedTypes}");
    }
}
