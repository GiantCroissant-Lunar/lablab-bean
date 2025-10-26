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
    private readonly Mock<ILogger<KnowledgeBaseService>> _mockLogger;
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        _mockKernelMemory = new Mock<IKernelMemory>();
        _mockLogger = new Mock<ILogger<KnowledgeBaseService>>();
        _service = new KnowledgeBaseService(_mockKernelMemory.Object, _mockLogger.Object);
    }

    #region T041 - IndexDocumentAsync Tests

    [Fact]
    public async Task IndexDocumentAsync_WithValidDocument_IndexesSuccessfully()
    {
        // Arrange
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "handbook-001",
            Title = "Employee Handbook",
            Content = "Customer service policy: Always be polite and helpful. Conflict resolution: Escalate to management if needed.",
            Category = "handbook",
            Role = "employee",
            Tags = new Dictionary<string, string> { { "type", "knowledge" } }
        };

        _mockKernelMemory
            .Setup(km => km.ImportDocumentAsync(
                It.IsAny<Document>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("import-123");

        // Act
        await _service.IndexDocumentAsync(document);

        // Assert
        _mockKernelMemory.Verify(
            km => km.ImportDocumentAsync(
                It.Is<Document>(d =>
                    d.Id == "handbook-001" &&
                    d.Tags.Any(t => t.Name == "category" && t.Value == "handbook") &&
                    d.Tags.Any(t => t.Name == "role" && t.Value == "employee") &&
                    d.Tags.Any(t => t.Name == "type" && t.Value == "knowledge")),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IndexDocumentAsync_WithRoleBasedTags_AppliesCorrectTags()
    {
        // Arrange
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "policy-001",
            Title = "Boss Management Policy",
            Content = "Performance review guidelines...",
            Category = "policy",
            Role = "boss"
        };

        // Act
        await _service.IndexDocumentAsync(document);

        // Assert
        _mockKernelMemory.Verify(
            km => km.ImportDocumentAsync(
                It.Is<Document>(d =>
                    d.Tags.Any(t => t.Name == "role" && t.Value == "boss") &&
                    d.Tags.Any(t => t.Name == "category" && t.Value == "policy")),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IndexDocumentAsync_WhenFails_LogsErrorAndThrows()
    {
        // Arrange
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "test-001",
            Title = "Test Doc",
            Content = "Content",
            Category = "test",
            Role = "all"
        };

        _mockKernelMemory
            .Setup(km => km.ImportDocumentAsync(
                It.IsAny<Document>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Import failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.IndexDocumentAsync(document));
    }

    #endregion

    #region T042 - QueryKnowledgeBaseAsync Tests

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithValidQuery_ReturnsAnswerWithCitations()
    {
        // Arrange
        var query = "How should I handle an angry customer?";
        var memoryAnswer = new MemoryAnswer
        {
            Question = query,
            Result = "Based on customer service guidelines, you should remain calm, listen actively, and escalate if needed.",
            RelevantSources = new List<Citation>
            {
                new Citation
                {
                    SourceName = "handbook-001",
                    Link = "handbook-001/part-01",
                    Partitions = new List<string> { "part-01" },
                    SourceContentType = "text/plain",
                    SourceUrl = "",
                    Tags = new Dictionary<string, List<string>>
                    {
                        { "category", new List<string> { "handbook" } },
                        { "role", new List<string> { "employee" } }
                    }
                }
            }
        };

        memoryAnswer.RelevantSources[0].Partitions.Add(new Citation.Partition
        {
            Text = "Customer service policy: Always be polite and helpful.",
            Relevance = 0.85f,
            PartitionNumber = 1,
            SectionNumber = 1,
            LastUpdate = DateTimeOffset.UtcNow,
            Tags = new Dictionary<string, List<string>>()
        });

        _mockKernelMemory
            .Setup(km => km.AskAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter>(),
                It.IsAny<ICollection<MemoryFilter>>(),
                It.IsAny<double>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(memoryAnswer);

        // Act
        var result = await _service.QueryKnowledgeBaseAsync(query, role: "employee");

        // Assert
        result.Should().NotBeNull();
        result.Query.Should().Be(query);
        result.Answer.Should().NotBeEmpty();
        result.Citations.Should().NotBeEmpty();
        result.Citations.Should().HaveCountGreaterThan(0);
        result.IsGrounded.Should().BeTrue();
    }

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithRoleFilter_AppliesFilter()
    {
        // Arrange
        var query = "What are the management policies?";
        MemoryFilter? capturedFilter = null;

        _mockKernelMemory
            .Setup(km => km.AskAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter>(),
                It.IsAny<ICollection<MemoryFilter>>(),
                It.IsAny<double>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, MemoryFilter?, ICollection<MemoryFilter>?, double?, CancellationToken>(
                (q, idx, filter, filters, minRel, ct) => capturedFilter = filter)
            .ReturnsAsync(new MemoryAnswer
            {
                Question = query,
                Result = "Test answer",
                RelevantSources = new List<Citation>()
            });

        // Act
        await _service.QueryKnowledgeBaseAsync(query, role: "boss", category: "policy");

        // Assert
        _mockKernelMemory.Verify(
            km => km.AskAsync(
                query,
                It.IsAny<string>(),
                It.IsAny<MemoryFilter>(),
                It.IsAny<ICollection<MemoryFilter>>(),
                It.IsAny<double>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        capturedFilter.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithNoCitations_ReturnsUngroundedAnswer()
    {
        // Arrange
        var query = "Unknown topic";
        var memoryAnswer = new MemoryAnswer
        {
            Question = query,
            Result = "I don't have information about that.",
            RelevantSources = new List<Citation>()
        };

        _mockKernelMemory
            .Setup(km => km.AskAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter>(),
                It.IsAny<ICollection<MemoryFilter>>(),
                It.IsAny<double>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(memoryAnswer);

        // Act
        var result = await _service.QueryKnowledgeBaseAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Citations.Should().BeEmpty();
        result.IsGrounded.Should().BeFalse();
    }

    [Fact]
    public async Task QueryKnowledgeBaseAsync_WithMaxCitations_LimitsCitations()
    {
        // Arrange
        var query = "Customer service guidelines";
        var memoryAnswer = new MemoryAnswer
        {
            Question = query,
            Result = "Multiple guidelines apply...",
            RelevantSources = new List<Citation>()
        };

        // Add 5 sources but request max 2
        for (int i = 0; i < 5; i++)
        {
            var citation = new Citation
            {
                SourceName = $"doc-{i}",
                Link = $"doc-{i}/part-01",
                Partitions = new List<string>()
            };
            citation.Partitions.Add(new Citation.Partition
            {
                Text = $"Guideline {i}",
                Relevance = 0.8f - (i * 0.1f),
                PartitionNumber = 1,
                SectionNumber = 1,
                LastUpdate = DateTimeOffset.UtcNow,
                Tags = new Dictionary<string, List<string>>()
            });
            memoryAnswer.RelevantSources.Add(citation);
        }

        _mockKernelMemory
            .Setup(km => km.AskAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MemoryFilter>(),
                It.IsAny<ICollection<MemoryFilter>>(),
                It.IsAny<double>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(memoryAnswer);

        // Act
        var result = await _service.QueryKnowledgeBaseAsync(query, maxCitations: 2);

        // Assert
        result.Citations.Should().HaveCount(2);
        result.Citations[0].RelevanceScore.Should().BeGreaterThanOrEqualTo(result.Citations[1].RelevanceScore);
    }

    #endregion

    #region IsHealthyAsync Tests

    [Fact]
    public async Task IsHealthyAsync_WhenHealthy_ReturnsTrue()
    {
        // Arrange
        _mockKernelMemory
            .Setup(km => km.GetDocumentStatusAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataPipelineStatus());

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHealthyAsync_WhenUnhealthy_ReturnsFalse()
    {
        // Arrange
        _mockKernelMemory
            .Setup(km => km.GetDocumentStatusAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteDocumentAsync Tests

    [Fact]
    public async Task DeleteDocumentAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var documentId = "handbook-001";

        _mockKernelMemory
            .Setup(km => km.DeleteDocumentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteDocumentAsync(documentId);

        // Assert
        _mockKernelMemory.Verify(
            km => km.DeleteDocumentAsync(
                documentId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
