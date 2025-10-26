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
            Storage = new StorageOptions
            {
                Provider = "Volatile",
                CollectionName = "test-relationships"
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
        var relationshipMemory = new RelationshipMemory
        {
            Entity1Id = "alice_123",
            Entity2Id = "player_456",
            InteractionType = InteractionType.Conversation,
            Sentiment = "positive",
            Description = "Discussed project collaboration, both enthusiastic",
            Timestamp = DateTime.UtcNow
        };

        _kernelMemory.ImportTextAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<TagCollection>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns("memory_id");

        var result = await _memoryService.StoreRelationshipMemoryAsync(relationshipMemory);

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
}
