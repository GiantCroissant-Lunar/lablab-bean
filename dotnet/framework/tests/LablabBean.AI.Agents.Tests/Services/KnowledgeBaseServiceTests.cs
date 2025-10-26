using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Moq;
using Xunit;

namespace LablabBean.AI.Agents.Tests.Services;

public class KnowledgeBaseServiceTests
{
    private readonly Mock<IKernelMemory> _mockKernelMemory;
    private readonly Mock<ILogger<RagService>> _mockLogger;
    private readonly IRagService _service;

    public KnowledgeBaseServiceTests()
    {
        _mockKernelMemory = new Mock<IKernelMemory>();
        _mockLogger = new Mock<ILogger<RagService>>();
        _service = new RagService(_mockKernelMemory.Object, _mockLogger.Object);
    }

    #region IndexDocumentAsync Tests

    [Fact]
    public async Task IndexDocumentAsync_WithValidDocument_IndexesSuccessfully()
    {
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "handbook-001",
            Title = "Employee Handbook",
            Content = "Customer service policy: Always be polite and helpful.",
            Category = "handbook",
            Role = "employee",
            Tags = new Dictionary<string, string> { { "type", "knowledge" } }
        };

        _mockKernelMemory
            .Setup(km => km.ImportTextAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TagCollection>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("import-123");

        await _service.IndexDocumentAsync(document);

        _mockKernelMemory.Verify(
            km => km.ImportTextAsync(
                It.Is<string>(t => t.Contains("polite and helpful")),
                It.Is<string>(id => id == "handbook-001"),
                It.Is<TagCollection>(tags =>
                    tags.ContainsKey("document_id") &&
                    tags.ContainsKey("title") &&
                    tags.ContainsKey("category") &&
                    tags.ContainsKey("role") &&
                    tags.ContainsKey("indexed_at") &&
                    tags.ContainsKey("type")),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IndexDocumentAsync_WhenFails_Throws()
    {
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "test-001",
            Title = "Test Doc",
            Content = "Content",
            Category = "test",
            Role = "all"
        };

        _mockKernelMemory
            .Setup(km => km.ImportTextAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TagCollection>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Import failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IndexDocumentAsync(document));
    }

    #endregion

    #region QueryKnowledgeBaseAsync Tests

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithValidQuery_ReturnsAnswerWithCitations()
    {
        var query = "How should I handle an angry customer?";

        var searchAnswer = new MemoryAnswer
        {
            Question = query,
            Results = new List<Citation>
            {
                new Citation
                {
                    DocumentId = "handbook-001",
                    SourceName = "Employee Handbook",
                    Link = "handbook-001/part-01",
                    Partitions = new List<Citation.Partition>
                    {
                        new Citation.Partition
                        {
                            Text = "Always be polite and helpful when handling complaints.",
                            Relevance = 0.85f,
                            PartitionNumber = 1,
                            Tags = new Dictionary<string, List<string>>()
                        }
                    }
                }
            }
        };

        _mockKernelMemory
            .Setup(km => km.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter?>(),
                It.IsAny<double?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchAnswer);

        var result = await _service.QueryKnowledgeBaseAsync(query, role: "employee");

        result.Should().NotBeNull();
        result.Query.Should().Be(query);
        result.Answer.Should().NotBeEmpty();
        result.Citations.Should().NotBeEmpty();
        result.IsGrounded.Should().BeTrue();
    }

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithNoCitations_ReturnsUngrounded()
    {
        var query = "Unknown topic";
        var answer = new MemoryAnswer { Question = query, Results = new List<Citation>() };

        _mockKernelMemory
            .Setup(km => km.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter?>(),
                It.IsAny<double?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);

        var result = await _service.QueryKnowledgeBaseAsync(query);

        result.IsGrounded.Should().BeFalse();
        result.Citations.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithMaxCitations_LimitsCitations()
    {
        var query = "Customer service guidelines";
        var citations = new List<Citation>();
        for (int i = 0; i < 5; i++)
        {
            citations.Add(new Citation
            {
                DocumentId = $"doc-{i}",
                SourceName = $"Doc {i}",
                Partitions = new List<Citation.Partition>
                {
                    new Citation.Partition { Text = $"Guideline {i}", Relevance = 0.8f - i * 0.1f, PartitionNumber = 1, Tags = new Dictionary<string, List<string>>() }
                }
            });
        }

        var answer = new MemoryAnswer { Question = query, Results = citations };

        _mockKernelMemory
            .Setup(km => km.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter?>(),
                It.IsAny<double?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);

        var result = await _service.QueryKnowledgeBaseAsync(query, maxCitations: 2);

        result.Citations.Should().HaveCount(2);
    }

    #endregion

    #region Health and Deletion

    [Fact]
    public async Task IsHealthyAsync_WhenSearchSucceeds_ReturnsTrue()
    {
        _mockKernelMemory
            .Setup(km => km.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter?>(),
                It.IsAny<double?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryAnswer());

        var result = await _service.IsHealthyAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDocumentAsync_InvokesKernelDelete()
    {
        var docId = "handbook-001";
        _mockKernelMemory
            .Setup(km => km.DeleteDocumentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteDocumentAsync(docId);

        _mockKernelMemory.Verify(km => km.DeleteDocumentAsync(
            docId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
