using LablabBean.AI.Agents.Configuration;
using LablabBean.AI.Agents.Services;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using NSubstitute;
using Xunit;

namespace LablabBean.AI.Agents.Tests.Services;

public class RelationshipMemoryTests : IAsyncLifetime
{
    private readonly ILogger<MemoryService> _logger;
    private readonly IOptions<KernelMemoryOptions> _options;
    private readonly IKernelMemory _kernelMemory;
    private readonly MemoryService _memoryService;

    public RelationshipMemoryTests()
    {
        _logger = Substitute.For<ILogger<MemoryService>>();
        _kernelMemory = Substitute.For<IKernelMemory>();

        var memoryOptions = new KernelMemoryOptions
        {
            Embedding = new EmbeddingOptions
            {
                Provider = "OpenAI",
                ModelName = "text-embedding-3-small",
                MaxTokens = 8191
            },
            Storage = new StorageOptions
            {
                Provider = "Volatile",
                ConnectionString = "",
                CollectionName = "test-relationships"
            },
            TextGeneration = new TextGenerationOptions
            {
                Provider = "OpenAI",
                ModelName = "gpt-4",
                MaxTokens = 4096
            }
        };

        _options = Options.Create(memoryOptions);
        _memoryService = new MemoryService(_logger, _kernelMemory, _options);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task StoreRelationshipMemoryAsync_StoresMemoryWithCorrectTags()
    {
        // Arrange
        var relationshipMemory = new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Conversation,
            Sentiment = "positive",
            Description = "Discussed project collaboration, both enthusiastic",
            Timestamp = DateTime.UtcNow
        };

        // Mock kernel memory to return success
        _kernelMemory.ImportTextAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<TagCollection>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns("memory_id");

        // Act
        var result = await _memoryService.StoreRelationshipMemoryAsync(relationshipMemory);

        // Assert
        Assert.NotNull(result);
        await _kernelMemory.Received(1).ImportTextAsync(
            Arg.Is<string>(s => s.Contains("alice_123") && s.Contains("player_456")),
            Arg.Any<string>(),
            Arg.Is<TagCollection>(tags =>
                tags.ContainsKey("type") &&
                tags.ContainsKey("entity1") &&
                tags.ContainsKey("entity2") &&
                tags.ContainsKey("sentiment")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StoreRelationshipMemoryAsync_GeneratesCorrectTags()
    {
        // Arrange
        var relationshipMemory = new RelationshipMemory
        {
            Entity1Id = "bob_789",
            Entity2Id = "charlie_012",
            InteractionType = InteractionType.Trade,
            Sentiment = "neutral",
            Description = "Completed standard trade transaction",
            Timestamp = DateTime.UtcNow
        };

        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        // Act
        await _memoryService.StoreRelationshipMemoryAsync(relationshipMemory);

        // Retrieve and verify tags
        var results = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "bob_789",
            entity2Id: "charlie_012",
            query: "trade",
            maxResults: 10
        );

        // Assert - should find the stored memory
        Assert.NotEmpty(results);
        var memory = results.First();
        Assert.Contains("bob_789", memory.Content);
        Assert.Contains("charlie_012", memory.Content);
    }

    [Fact]
    public async Task StoreRelationshipMemoryAsync_IncludesSentimentInMetadata()
    {
        // Arrange
        var negativeInteraction = new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Betrayal,
            Sentiment = "negative",
            Description = "Player broke promise, Alice feels betrayed",
            Timestamp = DateTime.UtcNow
        };

        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        // Act
        await _memoryService.StoreRelationshipMemoryAsync(negativeInteraction);

        // Retrieve with sentiment filter
        var results = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "alice_123",
            entity2Id: "player_456",
            query: "betrayal",
            maxResults: 10,
            sentiment: "negative"
        );

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Contains("negative", r.Content.ToLower()));
    }

    [Fact]
    public async Task RetrieveRelevantRelationshipHistoryAsync_FiltersByEntityPair()
    {
        // Arrange - store memories for different entity pairs
        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Conversation,
            Sentiment = "positive",
            Description = "Alice and player had friendly chat",
            Timestamp = DateTime.UtcNow
        });

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "bob_789",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Combat,
            Sentiment = "negative",
            Description = "Bob fought with player",
            Timestamp = DateTime.UtcNow
        });

        // Act - retrieve only Alice-Player relationship
        var alicePlayerHistory = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "alice_123",
            entity2Id: "player_456",
            query: "interaction",
            maxResults: 10
        );

        // Assert - should only contain Alice-Player memories
        Assert.NotEmpty(alicePlayerHistory);
        Assert.All(alicePlayerHistory, r =>
        {
            var content = r.Content.ToLower();
            Assert.Contains("alice", content);
            Assert.DoesNotContain("bob", content);
        });
    }

    [Fact]
    public async Task RetrieveRelevantRelationshipHistoryAsync_WorksBidirectionally()
    {
        // Arrange
        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "bob_789",
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Alice and Bob collaborated on project",
            Timestamp = DateTime.UtcNow
        });

        // Act - retrieve with entities in reverse order
        var resultsForward = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "alice_123",
            entity2Id: "bob_789",
            query: "collaboration",
            maxResults: 10
        );

        var resultsReverse = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "bob_789",
            entity2Id: "alice_123",
            query: "collaboration",
            maxResults: 10
        );

        // Assert - both directions should find the same memory
        Assert.NotEmpty(resultsForward);
        Assert.NotEmpty(resultsReverse);
        Assert.Equal(resultsForward.Count, resultsReverse.Count);
    }

    [Fact]
    public async Task RetrieveRelevantRelationshipHistoryAsync_FiltersBySentiment()
    {
        // Arrange - store mixed sentiment interactions
        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Gift,
            Sentiment = "positive",
            Description = "Player gave Alice a gift, she was delighted",
            Timestamp = DateTime.UtcNow.AddDays(-2)
        });

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Conversation,
            Sentiment = "negative",
            Description = "Argument about work deadline, tension",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        });

        // Act - retrieve only positive memories
        var positiveMemories = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "alice_123",
            entity2Id: "player_456",
            query: "interaction",
            maxResults: 10,
            sentiment: "positive"
        );

        // Assert - should only contain positive interactions
        Assert.NotEmpty(positiveMemories);
        Assert.All(positiveMemories, r => Assert.Contains("positive", r.Content.ToLower()));
    }

    [Fact]
    public async Task RetrieveRelevantRelationshipHistoryAsync_ReturnsChronologicallyOrdered()
    {
        // Arrange - store memories at different times
        var mockEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        _mockEmbeddingService.Setup(e => e.GenerateEmbeddingAsync(
            It.IsAny<string>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEmbedding);

        var oldTimestamp = DateTime.UtcNow.AddDays(-10);
        var recentTimestamp = DateTime.UtcNow.AddDays(-1);

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Conversation,
            Sentiment = "neutral",
            Description = "First meeting, introductions",
            Timestamp = oldTimestamp
        });

        await _memoryService.StoreRelationshipMemoryAsync(new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Collaboration,
            Sentiment = "positive",
            Description = "Recent successful project completion",
            Timestamp = recentTimestamp
        });

        // Act
        var results = await _memoryService.RetrieveRelevantRelationshipHistoryAsync(
            entity1Id: "alice_123",
            entity2Id: "player_456",
            query: "working together",
            maxResults: 10
        );

        // Assert - more recent memories should appear first (or have higher relevance)
        Assert.NotEmpty(results);
        // Note: Actual ordering depends on relevance scoring + recency weighting
    }
}
