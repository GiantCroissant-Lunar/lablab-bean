using FluentAssertions;
using LablabBean.AI.Agents.Services.KnowledgeBase;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Comprehensive end-to-end integration tests for the RAG (Retrieval-Augmented Generation) pipeline
/// Tests the complete workflow: Document Loading → Chunking → Indexing → Search → Prompt Augmentation
///
/// Test Coverage:
/// - Full RAG pipeline workflow
/// - Document loading from files
/// - Document chunking strategies
/// - Knowledge base search
/// - Prompt augmentation
/// - Category filtering
/// - Multi-document scenarios
/// - Error handling
/// </summary>
public class RagPipelineIntegrationTests : IDisposable
{
    private readonly ILogger<DocumentLoader> _loaderLogger;
    private readonly ILogger<PromptAugmentationService> _augLogger;
    private readonly IKnowledgeBaseService _knowledgeBase;
    private readonly IDocumentChunker _chunker;
    private readonly IDocumentLoader _loader;
    private readonly IPromptAugmentationService _promptAugmentation;
    private readonly string _testDataDir;

    public RagPipelineIntegrationTests()
    {
        _loaderLogger = Substitute.For<ILogger<DocumentLoader>>();
        _augLogger = Substitute.For<ILogger<PromptAugmentationService>>();

        // Setup test directory
        _testDataDir = Path.Combine(Path.GetTempPath(), $"rag-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDataDir);

        // Initialize real components
        _chunker = new DocumentChunker();
        _loader = new DocumentLoader(_loaderLogger);
        _knowledgeBase = Substitute.For<IKnowledgeBaseService>();
        _promptAugmentation = new PromptAugmentationService(_augLogger, _knowledgeBase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDir))
        {
            Directory.Delete(_testDataDir, recursive: true);
        }
    }

    #region End-to-End RAG Workflow Tests

    [Fact]
    public async Task EndToEndRagWorkflow_WithRealDocuments_CompletesSuccessfully()
    {
        // Arrange - Create test documents
        CreateTestMarkdownFile("world-history.md", "lore",
            """
            # World History

            ## The Age of Dragons
            Draconus the Eternal was the most powerful Ancient Dragon who ruled the world for millennia.
            His reign ended 1000 years ago when the hero Aethon the Bold defeated him in an epic battle.

            ## The Modern Era
            After the fall of Draconus, humanity flourished and built great kingdoms.
            """);

        CreateTestMarkdownFile("main-quest.md", "quest",
            """
            # Main Quest: The Dragon's Return

            Rumors suggest that Draconus may be returning. Investigate the ancient ruins.

            ## Objectives
            1. Find the Dragon Orb
            2. Speak to the Oracle
            3. Prepare for battle
            """);

        // Act 1: Load documents
        var documents = await _loader.LoadFromDirectoryAsync(_testDataDir);

        // Assert 1: Documents loaded
        documents.Should().HaveCount(2);
        documents.Should().Contain(d => d.Category == "lore");
        documents.Should().Contain(d => d.Category == "quest");

        // Act 2: Chunk documents
        var allChunks = new List<DocumentChunk>();
        foreach (var doc in documents)
        {
            var chunks = _chunker.ChunkDocument(doc, maxChunkSize: 500, overlapSize: 100);
            allChunks.AddRange(chunks);
        }

        // Assert 2: Chunks created
        allChunks.Should().HaveCountGreaterOrEqualTo(2);
        allChunks.Should().AllSatisfy(chunk =>
        {
            chunk.Content.Should().NotBeEmpty();
            chunk.DocumentId.Should().NotBeEmpty();
            chunk.Category.Should().NotBeEmpty();
        });

        // Act 3: Simulate indexing and search
        var loreChunk = allChunks.First(c => c.Category == "lore");
        var mockSearchResults = new List<KnowledgeSearchResult>
        {
            new KnowledgeSearchResult
            {
                Chunk = loreChunk,
                Score = 0.94f
            }
        };

        _knowledgeBase.SearchAsync(
            "Tell me about Draconus the Eternal",
            3,
            "lore",
            Arg.Any<List<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(mockSearchResults);

        // Act 4: Augment query
        var context = await _promptAugmentation.AugmentQueryAsync(
            query: "Tell me about Draconus the Eternal",
            topK: 3,
            category: "lore");

        // Assert 4: Context retrieved
        context.Should().NotBeNull();
        context.Query.Should().Be("Tell me about Draconus the Eternal");
        context.RetrievedDocuments.Should().HaveCount(1);
        context.RetrievedDocuments[0].Chunk.Category.Should().Be("lore");
        context.FormattedContext.Should().Contain("Draconus");

        // Act 5: Build augmented prompt
        var systemPrompt = "You are a knowledgeable game master.";
        var userQuery = "Tell me about Draconus the Eternal";
        var augmentedPrompt = _promptAugmentation.BuildAugmentedPrompt(systemPrompt, userQuery, context);

        // Assert 5: Prompt properly constructed
        augmentedPrompt.Should().Contain(systemPrompt);
        augmentedPrompt.Should().Contain(userQuery);
        augmentedPrompt.Should().Contain("Draconus");
    }

    [Fact]
    public async Task RagPipeline_WithCategoryFiltering_ReturnsOnlyRelevantCategory()
    {
        // Arrange
        var loreDoc = CreateTestDocument("history.md", "lore", "Ancient history of dragons and heroes");
        var questDoc = CreateTestDocument("quest.md", "quest", "Defeat the dragon lord quest objectives");

        var mockLoreResults = new List<KnowledgeSearchResult>
        {
            new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = "lore-chunk-1",
                    DocumentId = loreDoc.Id,
                    Content = loreDoc.Content,
                    Title = loreDoc.Title,
                    Category = "lore",
                    Source = loreDoc.Source
                },
                Score = 0.95f
            }
        };

        _knowledgeBase.SearchAsync("dragon history", 5, "lore", null, Arg.Any<CancellationToken>())
            .Returns(mockLoreResults);

        // Act
        var context = await _promptAugmentation.AugmentQueryAsync(
            query: "dragon history",
            topK: 5,
            category: "lore");

        // Assert
        context.RetrievedDocuments.Should().HaveCount(1);
        context.RetrievedDocuments.Should().AllSatisfy(r =>
            r.Chunk.Category.Should().Be("lore"));
    }

    [Fact]
    public async Task RagPipeline_WithNoMatches_ReturnsEmptyContext()
    {
        // Arrange
        _knowledgeBase.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<KnowledgeSearchResult>());

        // Act
        var context = await _promptAugmentation.AugmentQueryAsync("nonexistent topic", topK: 5);

        // Assert
        context.Should().NotBeNull();
        context.RetrievedDocuments.Should().BeEmpty();
        context.FormattedContext.Should().BeEmpty();
    }

    [Fact]
    public async Task RagPipeline_WithTopKLimit_RespectsLimit()
    {
        // Arrange
        var results = Enumerable.Range(1, 10)
            .Select(i => new KnowledgeSearchResult
            {
                Chunk = new DocumentChunk
                {
                    Id = $"chunk-{i}",
                    DocumentId = $"doc-{i}",
                    Content = $"Content {i}",
                    Title = $"Document {i}",
                    Category = "lore",
                    Source = $"doc-{i}.md"
                },
                Score = 1.0f - (i * 0.05f)
            })
            .Take(3)
            .ToList();

        _knowledgeBase.SearchAsync(Arg.Any<string>(), 3, Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(results);

        // Act
        var context = await _promptAugmentation.AugmentQueryAsync("test query", topK: 3);

        // Assert
        context.RetrievedDocuments.Should().HaveCount(3);
    }

    #endregion

    #region Document Loading Tests

    [Fact]
    public async Task DocumentLoader_WithValidMarkdown_ParsesCorrectly()
    {
        // Arrange
        CreateTestMarkdownFile("test-doc.md", "lore",
            """
            # Test Document

            This is a test document with some content about game lore.
            It has multiple paragraphs and sections.
            """);

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(_testDataDir);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Title.Should().NotBeEmpty(); // Title is extracted from markdown content
        documents[0].Category.Should().Be("lore");
        documents[0].Content.Should().Contain("Test Document");
    }

    [Fact]
    public async Task DocumentLoader_WithMultipleFiles_LoadsAll()
    {
        // Arrange
        CreateTestMarkdownFile("doc1.md", "lore", "Lore content");
        CreateTestMarkdownFile("doc2.md", "quest", "Quest content");
        CreateTestMarkdownFile("doc3.md", "location", "Location content");

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(_testDataDir);

        // Assert
        documents.Should().HaveCount(3);
        documents.Should().Contain(d => d.Category == "lore");
        documents.Should().Contain(d => d.Category == "quest");
        documents.Should().Contain(d => d.Category == "location");
    }

    #endregion

    #region Document Chunking Tests

    [Fact]
    public void DocumentChunker_WithLargeDocument_CreatesMultipleChunks()
    {
        // Arrange
        var largeContent = string.Join("\n\n", Enumerable.Range(1, 50)
            .Select(i => $"Paragraph {i}: " + string.Join(" ", Enumerable.Repeat($"Content for section {i}.", 20))));

        var document = CreateTestDocument("large-doc.md", "lore", largeContent);

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize: 500, overlapSize: 100);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().AllSatisfy(chunk =>
        {
            chunk.Content.Length.Should().BeLessOrEqualTo(600); // Max + overlap tolerance
            chunk.DocumentId.Should().Be(document.Id);
            chunk.Category.Should().Be(document.Category);
        });

        // Verify sequential indices
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].ChunkIndex.Should().Be(i);
            chunks[i].TotalChunks.Should().Be(chunks.Count);
        }
    }

    [Fact]
    public void DocumentChunker_WithSmallDocument_ReturnsSingleChunk()
    {
        // Arrange
        var document = CreateTestDocument("small-doc.md", "lore",
            "This is a small document with minimal content.");

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize: 1000, overlapSize: 200);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Be(document.Content);
        chunks[0].ChunkIndex.Should().Be(0);
        chunks[0].TotalChunks.Should().Be(1);
    }

    #endregion

    #region Prompt Augmentation Tests

    [Fact]
    public void BuildAugmentedPrompt_WithContext_InjectsCorrectly()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";
        var userQuery = "Tell me about dragons";
        var context = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>
            {
                new KnowledgeSearchResult
                {
                    Chunk = new DocumentChunk
                    {
                        Id = "chunk-1",
                        DocumentId = "doc-1",
                        Title = "Dragon History",
                        Content = "Dragons are ancient creatures of immense power.",
                        Category = "lore",
                        Source = "dragons.md"
                    },
                    Score = 0.95f
                }
            }
        };
        context.FormatForPrompt();

        // Act
        var augmentedPrompt = _promptAugmentation.BuildAugmentedPrompt(systemPrompt, userQuery, context);

        // Assert
        augmentedPrompt.Should().Contain("You are a helpful assistant");
        augmentedPrompt.Should().Contain("Tell me about dragons");
        augmentedPrompt.Should().Contain("Dragons are ancient creatures");
        augmentedPrompt.Should().Contain("Dragon History");
    }

    [Fact]
    public void BuildAugmentedPrompt_WithoutContext_ReturnsBasicPrompt()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";
        var userQuery = "Tell me something";
        var emptyContext = new RagContext
        {
            Query = userQuery,
            RetrievedDocuments = new List<KnowledgeSearchResult>()
        };

        // Act
        var augmentedPrompt = _promptAugmentation.BuildAugmentedPrompt(systemPrompt, userQuery, emptyContext);

        // Assert
        augmentedPrompt.Should().Contain(systemPrompt);
        augmentedPrompt.Should().Contain(userQuery);
    }

    #endregion

    #region Multi-Category Search Tests

    [Fact]
    public async Task RagPipeline_WithMultipleCategories_RetrievesAcrossCategories()
    {
        // Arrange
        var mockResults = new List<KnowledgeSearchResult>
        {
            CreateMockSearchResult("lore-doc", "lore", "Ancient dragon history", 0.95f),
            CreateMockSearchResult("quest-doc", "quest", "Defeat the dragon", 0.88f),
            CreateMockSearchResult("location-doc", "location", "Dragon's mountain lair", 0.82f)
        };

        _knowledgeBase.SearchAsync("dragon", 5, null, null, Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var context = await _promptAugmentation.AugmentQueryAsync("dragon", topK: 5, category: null);

        // Assert
        context.RetrievedDocuments.Should().HaveCount(3);
        context.RetrievedDocuments.Select(r => r.Chunk.Category).Should().Contain("lore");
        context.RetrievedDocuments.Select(r => r.Chunk.Category).Should().Contain("quest");
        context.RetrievedDocuments.Select(r => r.Chunk.Category).Should().Contain("location");
    }

    [Fact]
    public async Task RagPipeline_OrdersByRelevanceScore_DescendingOrder()
    {
        // Arrange
        var mockResults = new List<KnowledgeSearchResult>
        {
            CreateMockSearchResult("doc-1", "lore", "High relevance", 0.95f),
            CreateMockSearchResult("doc-2", "lore", "Medium relevance", 0.75f),
            CreateMockSearchResult("doc-3", "lore", "Low relevance", 0.50f)
        };

        _knowledgeBase.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var context = await _promptAugmentation.AugmentQueryAsync("test query", topK: 5);

        // Assert
        context.RetrievedDocuments.Should().BeInDescendingOrder(r => r.Score);
        context.RetrievedDocuments[0].Score.Should().BeGreaterOrEqualTo(context.RetrievedDocuments[^1].Score);
    }

    #endregion

    #region Helper Methods

    private KnowledgeDocument CreateTestDocument(string filename, string category, string content)
    {
        return new KnowledgeDocument
        {
            Id = Guid.NewGuid().ToString(),
            Title = Path.GetFileNameWithoutExtension(filename),
            Content = content,
            Category = category,
            Tags = new List<string> { "test", category },
            Source = filename,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void CreateTestMarkdownFile(string filename, string category, string content)
    {
        var filePath = Path.Combine(_testDataDir, filename);
        var fileContent = $"""
            ---
            category: {category}
            tags: [test, integration]
            ---

            {content}
            """;
        File.WriteAllText(filePath, fileContent);
    }

    private static KnowledgeSearchResult CreateMockSearchResult(string docId, string category, string content, float score)
    {
        return new KnowledgeSearchResult
        {
            Chunk = new DocumentChunk
            {
                Id = $"{docId}-chunk",
                DocumentId = docId,
                Title = $"{category} Document",
                Content = content,
                Category = category,
                Source = $"{docId}.md"
            },
            Score = score
        };
    }

    #endregion
}
