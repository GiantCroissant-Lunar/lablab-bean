using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using LablabBean.Contracts.Resilience.Services;

namespace LablabBean.Contracts.AI.Memory;

/// <summary>
/// Minimal RAG service implementation backed by Microsoft Kernel Memory.
/// Provides grounded answers with citations from indexed knowledge base documents.
/// </summary>
public class RagService : IRagService
{
    private readonly IKernelMemory _memory;
    private readonly ILogger<RagService> _logger;
    private readonly IService? _resilience;
    private const string KnowledgeBaseIndex = "knowledge-base";

    public RagService(
        IKernelMemory memory,
        ILogger<RagService> logger,
        IService? resilience = null)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _resilience = resilience;
    }

    public async Task IndexDocumentAsync(
        KnowledgeBaseDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var tags = new TagCollection
        {
            { "document_id", document.DocumentId },
            { "title", document.Title },
            { "category", document.Category },
            { "role", document.Role },
            { "indexed_at", DateTimeOffset.UtcNow.ToString("O") }
        };

        foreach (var kv in document.Tags)
        {
            tags.Add(kv.Key, kv.Value);
        }

        try
        {
            var operation = () => _memory.ImportTextAsync(
                text: document.Content,
                documentId: document.DocumentId,
                tags: tags,
                index: KnowledgeBaseIndex,
                cancellationToken: cancellationToken);

            if (_resilience != null)
                await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken);
            else
                await operation();

            _logger.LogInformation("Indexed knowledge doc {DocId}: {Title}", document.DocumentId, document.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index knowledge doc {DocId}", document.DocumentId);
            throw;
        }
    }

    public async Task<KnowledgeBaseAnswer> QueryKnowledgeBaseAsync(
        string query,
        string? role = null,
        string? category = null,
        int maxCitations = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        if (maxCitations <= 0) maxCitations = 1;

        try
        {
            var filter = new MemoryFilter();
            if (!string.IsNullOrWhiteSpace(role))
            {
                filter.Add("role", role);
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                filter.Add("category", category);
            }

            var operation = () => _memory.SearchAsync(
                query: query,
                index: KnowledgeBaseIndex,
                filter: filter,
                limit: Math.Max(maxCitations, 5),
                cancellationToken: cancellationToken);

            var answer = _resilience != null
                ? await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken)
                : await operation();

            var citations = new List<Citation>();
            var sb = new System.Text.StringBuilder();

            // Simple synthesis: collect top partitions' text as the answer basis
            int added = 0;
            foreach (var c in (answer?.Results ?? new List<Microsoft.KernelMemory.Citation>()))
            {
                foreach (var p in c.Partitions)
                {
                    if (added >= maxCitations) break;

                    var docId = c.DocumentId ?? c.Link ?? string.Empty;
                    var title = c.SourceName ?? (p.Tags.ContainsKey("title") && p.Tags["title"].Count > 0
                        ? p.Tags["title"][0]
                        : docId);

                    citations.Add(new Citation
                    {
                        DocumentId = docId,
                        DocumentTitle = title ?? string.Empty,
                        Text = p.Text ?? string.Empty,
                        RelevanceScore = p.Relevance,
                        PartitionKey = p.PartitionNumber.ToString(),
                        Tags = p.Tags.ToDictionary(kvp => kvp.Key, kvp => string.Join(", ", kvp.Value))
                    });

                    // Add a short bullet from this partition to the synthesized answer
                    var snippet = p.Text?.Trim();
                    if (!string.IsNullOrEmpty(snippet))
                    {
                        sb.AppendLine("- " + (snippet.Length > 300 ? snippet[..300] + "..." : snippet));
                    }
                    added++;
                    if (added >= maxCitations) break;
                }
                if (added >= maxCitations) break;
            }

            var confidence = citations.Count > 0 ? citations.Average(c => c.RelevanceScore) : 0.0;
            var synthesized = citations.Count > 0
                ? sb.ToString().TrimEnd()
                : "No grounded sources found in the knowledge base for this query.";

            return new KnowledgeBaseAnswer
            {
                Answer = synthesized,
                Query = query,
                Citations = citations,
                ConfidenceScore = confidence
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge base query failed: {Query}", query);
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var operation = () => _memory.SearchAsync("health_check", index: KnowledgeBaseIndex, limit: 1, cancellationToken: cancellationToken);
            if (_resilience != null)
                await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken);
            else
                await operation();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAG health check failed");
            return false;
        }
    }

    public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        var operation = () => _memory.DeleteDocumentAsync(documentId, KnowledgeBaseIndex, cancellationToken);
        if (_resilience != null)
            await _resilience.ExecuteWithRetryAsync(operation, retryPolicy: null, cancellationToken);
        else
            await operation();
    }
}
