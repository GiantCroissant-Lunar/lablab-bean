using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LablabBean.AI.Agents.Services.KnowledgeBase;

/// <summary>
/// Loads knowledge documents from markdown files with YAML front matter
/// </summary>
public class DocumentLoader : IDocumentLoader
{
    private readonly ILogger<DocumentLoader> _logger;
    private readonly IDeserializer _yamlDeserializer;

    public DocumentLoader(ILogger<DocumentLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<List<KnowledgeDocument>> LoadFromDirectoryAsync(
        string directoryPath,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
            return new List<KnowledgeDocument>();
        }

        var documents = new List<KnowledgeDocument>();
        var markdownFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.AllDirectories);

        _logger.LogInformation("Loading {Count} markdown files from {DirectoryPath}", markdownFiles.Length, directoryPath);

        foreach (var filePath in markdownFiles)
        {
            try
            {
                var document = await LoadFromFileAsync(filePath, category, cancellationToken);
                documents.Add(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load document from {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Successfully loaded {Count} documents", documents.Count);
        return documents;
    }

    public async Task<KnowledgeDocument> LoadFromFileAsync(
        string filePath,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

        return ParseMarkdown(content, relativePath, category);
    }

    public KnowledgeDocument ParseMarkdown(string content, string source, string? defaultCategory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var document = new KnowledgeDocument
        {
            Source = source,
            Category = defaultCategory ?? "general"
        };

        // Check for YAML front matter
        if (content.StartsWith("---"))
        {
            var parts = content.Split(new[] { "---" }, 3, StringSplitOptions.None);

            if (parts.Length >= 3)
            {
                var frontMatter = parts[1].Trim();
                var bodyContent = parts[2].Trim();

                try
                {
                    var metadata = _yamlDeserializer.Deserialize<Dictionary<string, object>>(frontMatter);

                    if (metadata != null)
                    {
                        if (metadata.TryGetValue("title", out var title))
                            document.Title = title.ToString() ?? string.Empty;

                        if (metadata.TryGetValue("category", out var category))
                            document.Category = category.ToString() ?? defaultCategory ?? "general";

                        if (metadata.TryGetValue("tags", out var tags) && tags is List<object> tagList)
                            document.Tags = tagList.Select(t => t.ToString() ?? string.Empty).ToList();

                        if (metadata.TryGetValue("weight", out var weight) && float.TryParse(weight.ToString(), out var weightValue))
                            document.Weight = weightValue;

                        document.Metadata = metadata;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse YAML front matter for {Source}", source);
                }

                document.Content = bodyContent;
            }
            else
            {
                document.Content = content;
            }
        }
        else
        {
            document.Content = content;
        }

        // Extract title from first heading if not set
        if (string.IsNullOrWhiteSpace(document.Title))
        {
            var lines = document.Content.Split('\n');
            var firstHeading = lines.FirstOrDefault(l => l.TrimStart().StartsWith("# "));
            if (firstHeading != null)
            {
                document.Title = firstHeading.TrimStart('#').Trim();
            }
            else
            {
                document.Title = Path.GetFileNameWithoutExtension(source);
            }
        }

        return document;
    }
}
