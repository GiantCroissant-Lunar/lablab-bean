using FluentAssertions;
using LablabBean.AI.Agents.Services.KnowledgeBase;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

namespace LablabBean.AI.Agents.Tests.Services;

public class DocumentLoaderTests : IDisposable
{
    private readonly DocumentLoader _loader;
    private readonly string _testDirectory;

    public DocumentLoaderTests()
    {
        var mockLogger = Substitute.For<ILogger<DocumentLoader>>();
        _loader = new DocumentLoader(mockLogger);
        _testDirectory = Path.Combine(Path.GetTempPath(), "kb_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task LoadFromFileAsync_WithValidMarkdown_LoadsDocument()
    {
        // Arrange
        var content = @"---
title: ""Test Document""
category: ""lore""
tags:
  - test
  - sample
---

# Test Content

This is a test document.";

        var filePath = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var document = await _loader.LoadFromFileAsync(filePath);

        // Assert
        document.Should().NotBeNull();
        document.Title.Should().Be("Test Document");
        document.Category.Should().Be("lore");
        document.Tags.Should().Contain(new[] { "test", "sample" });
        document.Content.Should().Contain("This is a test document");
        document.Source.Should().Be(filePath);
    }

    [Fact]
    public async Task LoadFromFileAsync_WithCategoryOverride_UsesOverride()
    {
        // Arrange
        var content = @"---
title: ""Override Test""
category: ""lore""
---

Content here.";

        var filePath = Path.Combine(_testDirectory, "override.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var document = await _loader.LoadFromFileAsync(filePath, category: "quest");

        // Assert
        document.Category.Should().Be("quest");
    }

    [Fact]
    public async Task LoadFromFileAsync_WithoutFrontMatter_UsesDefaults()
    {
        // Arrange
        var content = "# Simple Document\n\nNo front matter here.";
        var filePath = Path.Combine(_testDirectory, "simple.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var document = await _loader.LoadFromFileAsync(filePath, category: "location");

        // Assert
        document.Should().NotBeNull();
        document.Title.Should().Be("simple"); // From filename
        document.Category.Should().Be("location");
        document.Content.Should().Contain("No front matter here");
    }

    [Fact]
    public async Task LoadFromFileAsync_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.md");

        // Act & Assert
        var act = async () => await _loader.LoadFromFileAsync(filePath);
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_WithMultipleFiles_LoadsAllFiles()
    {
        // Arrange
        await CreateTestFile("doc1.md", "lore", "Document 1");
        await CreateTestFile("doc2.md", "quest", "Document 2");
        await CreateTestFile("doc3.md", "location", "Document 3");

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(_testDirectory);

        // Assert
        documents.Should().HaveCount(3);
        documents.Should().Contain(d => d.Title.Contains("Document 1"));
        documents.Should().Contain(d => d.Title.Contains("Document 2"));
        documents.Should().Contain(d => d.Title.Contains("Document 3"));
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_WithCategoryFilter_LoadsAllWithCategory()
    {
        // Arrange
        await CreateTestFile("lore1.md", "lore", "Lore 1");
        await CreateTestFile("quest1.md", "quest", "Quest 1");
        await CreateTestFile("lore2.md", "lore", "Lore 2");

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(_testDirectory, category: "lore");

        // Assert
        documents.Should().HaveCount(3);
        documents.Should().OnlyContain(d => d.Category == "lore");
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_WithSubdirectories_LoadsRecursively()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        await CreateTestFile("root.md", "lore", "Root Document");
        await CreateTestFile(Path.Combine("subdir", "sub.md"), "lore", "Sub Document");

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(_testDirectory);

        // Assert
        documents.Should().HaveCount(2);
        documents.Should().Contain(d => d.Title.Contains("Root"));
        documents.Should().Contain(d => d.Title.Contains("Sub"));
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_WithEmptyDirectory_ReturnsEmptyList()
    {
        // Arrange
        var emptyDir = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var documents = await _loader.LoadFromDirectoryAsync(emptyDir);

        // Assert
        documents.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_WithNonExistentDirectory_ThrowsException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "does_not_exist");

        // Act & Assert
        var act = async () => await _loader.LoadFromDirectoryAsync(nonExistentDir);
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public void ParseMarkdown_WithFullFrontMatter_ParsesAllFields()
    {
        // Arrange
        var content = @"---
title: ""Full Test""
category: ""lore""
tags:
  - dragon
  - ancient
  - powerful
metadata:
  author: ""Test Author""
  version: ""1.0""
---

# The Dragon

A powerful ancient dragon.";

        // Act
        var document = _loader.ParseMarkdown(content, "test.md");

        // Assert
        document.Title.Should().Be("Full Test");
        document.Category.Should().Be("lore");
        document.Tags.Should().Contain(new[] { "dragon", "ancient", "powerful" });
        document.Content.Should().Contain("A powerful ancient dragon");
        document.Source.Should().Be("test.md");
    }

    [Fact]
    public void ParseMarkdown_WithInvalidYaml_UsesDefaults()
    {
        // Arrange
        var content = @"---
invalid yaml: [
---

Content here.";

        // Act
        var document = _loader.ParseMarkdown(content, "invalid.md", defaultCategory: "test");

        // Assert
        document.Should().NotBeNull();
        document.Category.Should().Be("test");
        document.Source.Should().Be("invalid.md");
    }

    [Fact]
    public void ParseMarkdown_WithoutFrontMatter_ParsesContentOnly()
    {
        // Arrange
        var content = "# Simple Content\n\nJust plain markdown.";

        // Act
        var document = _loader.ParseMarkdown(content, "plain.md", defaultCategory: "item");

        // Assert
        document.Title.Should().Be("plain");
        document.Category.Should().Be("item");
        document.Content.Should().Be(content);
    }

    [Fact]
    public void ParseMarkdown_ExtractsIdFromFilename()
    {
        // Arrange
        var content = "Test content";

        // Act
        var document = _loader.ParseMarkdown(content, "my-document.md");

        // Assert
        document.Id.Should().NotBeNullOrEmpty();
        document.Title.Should().Be("my-document");
    }

    [Fact]
    public void ParseMarkdown_SetsTimestamps()
    {
        // Arrange
        var content = "Test content";
        var beforeParse = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var document = _loader.ParseMarkdown(content, "test.md");
        var afterParse = DateTime.UtcNow.AddSeconds(1);

        // Assert
        document.CreatedAt.Should().BeAfter(beforeParse);
        document.CreatedAt.Should().BeBefore(afterParse);
        document.UpdatedAt.Should().BeAfter(beforeParse);
        document.UpdatedAt.Should().BeBefore(afterParse);
    }

    [Theory]
    [InlineData("lore")]
    [InlineData("quest")]
    [InlineData("location")]
    [InlineData("item")]
    public async Task LoadFromFileAsync_WithDifferentCategories_LoadsCorrectly(string category)
    {
        // Arrange
        var content = $@"---
title: ""Category Test""
category: ""{category}""
---

Content for {category}.";

        var filePath = Path.Combine(_testDirectory, $"{category}.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var document = await _loader.LoadFromFileAsync(filePath);

        // Assert
        document.Category.Should().Be(category);
    }

    [Fact]
    public async Task LoadFromFileAsync_WithUTF8Content_LoadsCorrectly()
    {
        // Arrange
        var content = @"---
title: ""Unicode Test""
category: ""lore""
---

# 龍 (Dragon)

This document contains unicode: 日本語、中文、한글";

        var filePath = Path.Combine(_testDirectory, "unicode.md");
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

        // Act
        var document = await _loader.LoadFromFileAsync(filePath);

        // Assert
        document.Content.Should().Contain("龍");
        document.Content.Should().Contain("日本語");
        document.Content.Should().Contain("중문");
        document.Content.Should().Contain("한글");
    }

    private async Task CreateTestFile(string relativePath, string category, string title)
    {
        var content = $@"---
title: ""{title}""
category: ""{category}""
tags:
  - test
---

# {title}

Test content for {title}.";

        var fullPath = Path.Combine(_testDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(fullPath, content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
