using FluentAssertions;
using LablabBean.AI.Agents.Services.KnowledgeBase;
using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Agents.Tests.Services;

public class DocumentChunkerTests
{
    private readonly DocumentChunker _chunker;

    public DocumentChunkerTests()
    {
        _chunker = new DocumentChunker();
    }

    [Fact]
    public void ChunkDocument_WithSmallDocument_ReturnsOneChunk()
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-1",
            Title = "Small Document",
            Content = "This is a small document with less than 1000 characters.",
            Category = "lore",
            Tags = new List<string> { "test" },
            Source = "test.md"
        };

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize: 1000, overlapSize: 200);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Be(document.Content);
        chunks[0].DocumentId.Should().Be(document.Id);
        chunks[0].Title.Should().Be(document.Title);
        chunks[0].ChunkIndex.Should().Be(0);
        chunks[0].TotalChunks.Should().Be(1);
    }

    [Fact]
    public void ChunkDocument_WithLargeDocument_ReturnsMultipleChunks()
    {
        // Arrange
        var longContent = string.Join(" ", Enumerable.Repeat("This is a sentence.", 200));
        var document = new KnowledgeDocument
        {
            Id = "test-2",
            Title = "Large Document",
            Content = longContent,
            Category = "lore",
            Tags = new List<string> { "test", "large" },
            Source = "large.md"
        };

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize: 1000, overlapSize: 200);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(c => c.DocumentId == document.Id);
        chunks.Should().OnlyContain(c => c.Title == document.Title);
        chunks.Should().OnlyContain(c => c.Category == document.Category);

        // Verify chunk indices are sequential
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].ChunkIndex.Should().Be(i);
            chunks[i].TotalChunks.Should().Be(chunks.Count);
        }
    }

    [Fact]
    public void ChunkDocument_WithOverlap_HasOverlappingContent()
    {
        // Arrange
        var content = string.Join(" ", Enumerable.Repeat("Word", 500));
        var document = new KnowledgeDocument
        {
            Id = "test-3",
            Title = "Overlap Test",
            Content = content,
            Category = "lore"
        };

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize: 1000, overlapSize: 200);

        // Assert
        if (chunks.Count > 1)
        {
            // Check that consecutive chunks have overlap
            for (int i = 0; i < chunks.Count - 1; i++)
            {
                var chunk1End = chunks[i].Content.Substring(Math.Max(0, chunks[i].Content.Length - 100));
                var chunk2Start = chunks[i + 1].Content.Substring(0, Math.Min(100, chunks[i + 1].Content.Length));

                // Should have some common content
                chunk1End.Split(' ').Intersect(chunk2Start.Split(' ')).Should().NotBeEmpty();
            }
        }
    }

    [Fact]
    public void ChunkDocument_PreservesMetadata()
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-4",
            Title = "Metadata Test",
            Content = string.Join(" ", Enumerable.Repeat("Content", 300)),
            Category = "quest",
            Tags = new List<string> { "important", "urgent" },
            Source = "metadata.md",
            Metadata = new Dictionary<string, object>
            {
                { "author", "Test Author" },
                { "version", "1.0" }
            }
        };

        // Act
        var chunks = _chunker.ChunkDocument(document);

        // Assert
        chunks.Should().OnlyContain(c => c.Category == document.Category);
        chunks.Should().OnlyContain(c => c.Tags.SequenceEqual(document.Tags));
        chunks.Should().OnlyContain(c => c.Source == document.Source);
        chunks.Should().OnlyContain(c => c.Metadata.Count == document.Metadata.Count);
    }

    [Fact]
    public void ChunkText_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var chunks = _chunker.ChunkText("", maxChunkSize: 1000, overlapSize: 200);

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_WithShortText_ReturnsSingleChunk()
    {
        // Arrange
        var text = "Short text";

        // Act
        var chunks = _chunker.ChunkText(text, maxChunkSize: 1000, overlapSize: 200);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Should().Be(text);
    }

    [Fact]
    public void ChunkText_WithLongText_ReturnsMultipleChunks()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Repeat("Word", 500));

        // Act
        var chunks = _chunker.ChunkText(text, maxChunkSize: 100, overlapSize: 20);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(c => c.Length <= 120); // Max + some tolerance for word boundaries
    }

    [Fact]
    public void ChunkText_PreservesWordBoundaries()
    {
        // Arrange
        var text = "This is a test sentence that should be split properly at word boundaries.";

        // Act
        var chunks = _chunker.ChunkText(text, maxChunkSize: 30, overlapSize: 10);

        // Assert
        chunks.Should().OnlyContain(c => !c.StartsWith(" ") && !c.EndsWith(" "));
    }

    [Theory]
    [InlineData(100, 20)]
    [InlineData(500, 100)]
    [InlineData(1000, 200)]
    public void ChunkDocument_WithDifferentSizes_ProducesValidChunks(int maxChunkSize, int overlapSize)
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-sizes",
            Title = "Size Test",
            Content = string.Join(" ", Enumerable.Repeat("Testing different chunk sizes.", 100)),
            Category = "test"
        };

        // Act
        var chunks = _chunker.ChunkDocument(document, maxChunkSize, overlapSize);

        // Assert
        chunks.Should().NotBeEmpty();
        chunks.Should().OnlyContain(c => c.Content.Length <= maxChunkSize + 100); // Tolerance for word boundaries
        chunks.Should().OnlyContain(c => c.ChunkIndex >= 0 && c.ChunkIndex < chunks.Count);
        chunks.Should().OnlyContain(c => c.TotalChunks == chunks.Count);
    }

    [Fact]
    public void ChunkDocument_WithNullContent_ThrowsException()
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-null",
            Title = "Null Test",
            Content = null!,
            Category = "test"
        };

        // Act & Assert
        var act = () => _chunker.ChunkDocument(document);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChunkDocument_WithZeroMaxSize_ThrowsException()
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-zero",
            Title = "Zero Test",
            Content = "Content",
            Category = "test"
        };

        // Act & Assert
        var act = () => _chunker.ChunkDocument(document, maxChunkSize: 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChunkDocument_WithNegativeOverlap_ThrowsException()
    {
        // Arrange
        var document = new KnowledgeDocument
        {
            Id = "test-negative",
            Title = "Negative Test",
            Content = "Content",
            Category = "test"
        };

        // Act & Assert
        var act = () => _chunker.ChunkDocument(document, maxChunkSize: 1000, overlapSize: -1);
        act.Should().Throw<ArgumentException>();
    }
}
