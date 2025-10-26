using FluentAssertions;
using LablabBean.AI.Agents.Services.KnowledgeBase;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LablabBean.AI.Agents.Tests.Services;

public class PromptAugmentationServiceTests
{
    private readonly ILogger<PromptAugmentationService> _mockLogger;
    private readonly IKnowledgeBaseService _mockKbService;
    private readonly PromptAugmentationService _service;

    public PromptAugmentationServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<PromptAugmentationService>>();
        _mockKbService = Substitute.For<IKnowledgeBaseService>();
        _service = new PromptAugmentationService(_mockLogger, _mockKbService);
    }

    [Fact]
    public async Task AugmentQueryAsync_WithRelevantResults_ReturnsRagContext()
    {
        // Arrange
        var query = "Tell me about dragons";
        var searchResults = new List<KnowledgeSearchResult>
        {
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "chunk-1",
                    DocumentId = "doc-1",
                    Title = "Dragon Lore",
                    Content = "Dragons are ancient powerful creatures.",
                    Category = "lore",
                    Source = "dragons.md",
                    ChunkIndex = 0,
                    TotalChunks = 1
                },
                Score = 0.95f,
                Distance = 0.05f
            },
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "chunk-2",
                    DocumentId = "doc-2",
                    Title = "Dragon Types",
                    Content = "There are many types of dragons including fire, ice, and thunder dragons.",
                    Category = "lore",
                    Source = "dragon-types.md",
                    ChunkIndex = 0,
                    TotalChunks = 1
                },
                Score = 0.88f,
                Distance = 0.12f
            }
        };

        _mockKbService.SearchAsync(query, 5, null, null, Arg.Any<CancellationToken>())
            .Returns(searchResults);

        // Act
        var context = await _service.AugmentQueryAsync(query, topK: 5);

        // Assert
        context.Should().NotBeNull();
        context.Query.Should().Be(query);
        context.RetrievedDocuments.Should().HaveCount(2);
        context.RetrievedDocuments[0].Chunk.Content.Should().Contain("ancient powerful creatures");
        context.RetrievedDocuments[1].Chunk.Content.Should().Contain("fire, ice, and thunder");
    }

    [Fact]
    public async Task AugmentQueryAsync_WithNoResults_ReturnsEmptyContext()
    {
        // Arrange
        var query = "unknown topic";
        _mockKbService.SearchAsync(query, Arg.Any<int>(), null, null, Arg.Any<CancellationToken>())
            .Returns(new List<KnowledgeSearchResult>());

        // Act
        var context = await _service.AugmentQueryAsync(query);

        // Assert
        context.Should().NotBeNull();
        context.Query.Should().Be(query);
        context.RetrievedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task AugmentQueryAsync_WithCategoryFilter_PassesCategoryToSearch()
    {
        // Arrange
        var query = "ancient ruins";
        var category = "location";

        _mockKbService.SearchAsync(query, Arg.Any<int>(), category, null, Arg.Any<CancellationToken>())
            .Returns(new List<KnowledgeSearchResult>());

        // Act
        await _service.AugmentQueryAsync(query, category: category);

        // Assert
        await _mockKbService.Received(1).SearchAsync(
            query,
            Arg.Any<int>(),
            category,
            Arg.Any<List<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AugmentQueryAsync_WithTopKLimit_ReturnsLimitedResults()
    {
        // Arrange
        var query = "test query";
        var topK = 2;
        var searchResults = Enumerable.Range(1, 5).Select(i => new KnowledgeSearchResult
        {
            Chunk = new DocumentChunk
            {
                Id = $"chunk-{i}",
                Content = $"Content {i}",
                DocumentId = $"doc-{i}",
                Title = $"Doc {i}"
            },
            Score = 1.0f - (i * 0.1f)
        }).ToList();

        _mockKbService.SearchAsync(query, topK, null, null, Arg.Any<CancellationToken>())
            .Returns(searchResults.Take(topK).ToList());

        // Act
        var context = await _service.AugmentQueryAsync(query, topK: topK);

        // Assert
        context.RetrievedDocuments.Should().HaveCount(topK);
    }

    [Fact]
    public void BuildAugmentedPrompt_WithContext_IncludesContextInPrompt()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";
        var userQuery = "Tell me about dragons.";
        var ragContext = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>
            {
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Content = "Dragons are magical creatures.",
                        Title = "Dragon Basics",
                        Source = "dragons.md",
                        DocumentId = "doc1",
                        Id = "chunk1"
                    },
                    Score = 0.9f
                }
            }
        };

        // Act
        var augmentedPrompt = _service.BuildAugmentedPrompt(systemPrompt, userQuery, ragContext);

        // Assert
        augmentedPrompt.Should().Contain(systemPrompt);
        augmentedPrompt.Should().Contain(userQuery);
        augmentedPrompt.Should().Contain("Dragons are magical creatures");
        augmentedPrompt.Should().Contain("Dragon Basics");
        augmentedPrompt.Should().Contain("dragons.md");
    }

    [Fact]
    public void BuildAugmentedPrompt_WithNoContext_ReturnsBasicPrompt()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";
        var userQuery = "Hello";
        var ragContext = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>()
        };

        // Act
        var augmentedPrompt = _service.BuildAugmentedPrompt(systemPrompt, userQuery, ragContext);

        // Assert
        augmentedPrompt.Should().Contain(systemPrompt);
        augmentedPrompt.Should().Contain(userQuery);
    }

    [Fact]
    public void BuildAugmentedPrompt_WithMultipleChunks_IncludesAllChunks()
    {
        // Arrange
        var systemPrompt = "Assistant prompt";
        var userQuery = "Query";
        var ragContext = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>
            {
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Content = "Chunk 1 content",
                        Title = "Doc 1",
                        Source = "doc1.md",
                        DocumentId = "d1",
                        Id = "c1"
                    },
                    Score = 0.9f
                },
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Content = "Chunk 2 content",
                        Title = "Doc 2",
                        Source = "doc2.md",
                        DocumentId = "d2",
                        Id = "c2"
                    },
                    Score = 0.8f
                },
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Content = "Chunk 3 content",
                        Title = "Doc 3",
                        Source = "doc3.md",
                        DocumentId = "d3",
                        Id = "c3"
                    },
                    Score = 0.7f
                }
            }
        };

        // Act
        var augmentedPrompt = _service.BuildAugmentedPrompt(systemPrompt, userQuery, ragContext);

        // Assert
        augmentedPrompt.Should().Contain("Chunk 1 content");
        augmentedPrompt.Should().Contain("Chunk 2 content");
        augmentedPrompt.Should().Contain("Chunk 3 content");
        augmentedPrompt.Should().Contain("doc1.md");
        augmentedPrompt.Should().Contain("doc2.md");
        augmentedPrompt.Should().Contain("doc3.md");
    }

    [Fact]
    public void BuildAugmentedPrompt_IncludesRAGGuidelines()
    {
        // Arrange
        var systemPrompt = "System";
        var userQuery = "Query";
        var ragContext = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>
            {
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Content = "Test",
                        Title = "Test",
                        Source = "test.md",
                        DocumentId = "test",
                        Id = "test"
                    },
                    Score = 0.9f
                }
            }
        };

        // Act
        var augmentedPrompt = _service.BuildAugmentedPrompt(systemPrompt, userQuery, ragContext);

        // Assert
        augmentedPrompt.Should().Contain("context");
    }

    [Fact]
    public async Task AugmentQueryAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var query = "test";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockKbService.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<List<KnowledgeSearchResult>>(cts.Token));

        // Act & Assert
        var act = async () => await _service.AugmentQueryAsync(query, cancellationToken: cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task AugmentQueryAsync_WithInvalidQuery_ThrowsArgumentException(string? query)
    {
        // Act & Assert
        var act = async () => await _service.AugmentQueryAsync(query!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AugmentQueryAsync_WithZeroTopK_ThrowsArgumentException()
    {
        // Arrange
        var query = "test";

        // Act & Assert
        var act = async () => await _service.AugmentQueryAsync(query, topK: 0);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AugmentQueryAsync_WithNegativeTopK_ThrowsArgumentException()
    {
        // Arrange
        var query = "test";

        // Act & Assert
        var act = async () => await _service.AugmentQueryAsync(query, topK: -5);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void BuildAugmentedPrompt_WithNullSystemPrompt_ThrowsArgumentException()
    {
        // Arrange
        var ragContext = new RagContext { Query = "test", RetrievedDocuments = new List<KnowledgeSearchResult>() };

        // Act & Assert
        var act = () => _service.BuildAugmentedPrompt(null!, "query", ragContext);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BuildAugmentedPrompt_WithNullUserQuery_ThrowsArgumentException()
    {
        // Arrange
        var ragContext = new RagContext { Query = "test", RetrievedDocuments = new List<KnowledgeSearchResult>() };

        // Act & Assert
        var act = () => _service.BuildAugmentedPrompt("system", null!, ragContext);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BuildAugmentedPrompt_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.BuildAugmentedPrompt("system", "query", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AugmentQueryAsync_SortsByRelevanceScore()
    {
        // Arrange
        var query = "test";
        var searchResults = new List<KnowledgeSearchResult>
        {
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "1",
                    Content = "High score",
                    DocumentId = "doc1",
                    Title = "Doc 1"
                },
                Score = 0.95f
            },
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "2",
                    Content = "Medium score",
                    DocumentId = "doc2",
                    Title = "Doc 2"
                },
                Score = 0.75f
            },
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "3",
                    Content = "Low score",
                    DocumentId = "doc3",
                    Title = "Doc 3"
                },
                Score = 0.5f
            }
        };

        _mockKbService.SearchAsync(query, Arg.Any<int>(), null, null, Arg.Any<CancellationToken>())
            .Returns(searchResults.OrderByDescending(r => r.Score).ToList());

        // Act
        var context = await _service.AugmentQueryAsync(query);

        // Assert
        context.RetrievedDocuments[0].Chunk.Content.Should().Be("High score");
        context.RetrievedDocuments[1].Chunk.Content.Should().Be("Medium score");
        context.RetrievedDocuments[2].Chunk.Content.Should().Be("Low score");
    }
}
