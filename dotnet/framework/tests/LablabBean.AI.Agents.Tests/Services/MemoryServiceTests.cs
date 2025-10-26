using FluentAssertions;
using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using NSubstitute;
using Xunit;

namespace LablabBean.AI.Agents.Tests.Services;

public class MemoryServiceTests : IDisposable
{
    private readonly ILogger<LablabBean.AI.Agents.Services.MemoryService> _logger;
    private readonly IOptions<KernelMemoryOptions> _options;
    private readonly IKernelMemory _kernelMemory;
    private readonly LablabBean.AI.Agents.Services.MemoryService _sut;

    public MemoryServiceTests()
    {
        _logger = Substitute.For<ILogger<LablabBean.AI.Agents.Services.MemoryService>>();
        _kernelMemory = Substitute.For<IKernelMemory>();

        var memoryOptions = new KernelMemoryOptions
        {
            Storage = new StorageOptions
            {
                Provider = "Volatile",
                CollectionName = "test-memories"
            }
        };

        _options = Options.Create(memoryOptions);
        _sut = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory, _options);
    }

    public void Dispose() => GC.SuppressFinalize(this);

    [Fact]
    public async Task StoreMemoryAsync_ValidMemory_CallsKernelAndReturnsId()
    {
        var memory = new MemoryEntry
        {
            Id = "test-memory-001",
            Content = "Handled angry customer",
            EntityId = "employee_001",
            MemoryType = "interaction",
            Importance = 0.8,
            Timestamp = DateTimeOffset.UtcNow
        };

        _kernelMemory
            .ImportTextAsync(memory.Content, memory.Id, Arg.Any<TagCollection>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(memory.Id));

        var result = await _sut.StoreMemoryAsync(memory);

        result.Should().Be(memory.Id);
        await _kernelMemory.Received(1).ImportTextAsync(
            memory.Content,
            memory.Id,
            Arg.Any<TagCollection>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetrieveRelevantMemoriesAsync_EmptyQuery_Throws()
    {
        var options = new MemoryRetrievalOptions { EntityId = "e1" };
        var act = () => _sut.RetrieveRelevantMemoriesAsync("", options);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteMemoryAsync_CallsKernelDelete()
    {
        var id = "mem-123";
        await _sut.DeleteMemoryAsync(id);
        await _kernelMemory.Received(1).DeleteDocumentAsync(id, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsHealthyAsync_WhenSearchDoesNotThrow_ReturnsTrue()
    {
        // Return default MemoryAnswer; not used by the method
        await _kernelMemory.SearchAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<MemoryFilter?>(), Arg.Any<double?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        var healthy = await _sut.IsHealthyAsync();
        healthy.Should().BeTrue();
    }
}
