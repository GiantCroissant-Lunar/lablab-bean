using LablabBean.AI.Agents.Configuration;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace LablabBean.AI.Agents.Services.KnowledgeBase;

/// <summary>
/// Knowledge base service using Kernel Memory for document storage and retrieval
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ILogger<KnowledgeBaseService> _logger;
    private readonly IKernelMemory _memory;
    private readonly IDocumentChunker _chunker;
    private readonly KernelMemoryOptions _options;
    private const string KnowledgeBaseCollection = "knowledge-base";

    public KnowledgeBaseService(
        ILogger<KnowledgeBaseService> logger,
        IKernelMemory memory,
        IDocumentChunker chunker,
        IOptions<KernelMemoryOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing knowledge base collection: {Collection}", KnowledgeBaseCollection);

        // Kernel Memory handles collection creation automatically
        // Just verify we can connect
        var isReady = await IsInitializedAsync(cancellationToken);

        if (!isReady)
        {
            throw new InvalidOperationException("Failed to initialize knowledge base");
        }

        _logger.LogInformation("Knowledge base initialized successfully");
    }

    public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try a simple search to verify connection
            await _memory.SearchAsync("test", index: KnowledgeBaseCollection, limit: 1, cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Knowledge base not initialized or not accessible");
            return false;
        }
    }

    public async Task<string> AddDocumentAsync(KnowledgeDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        _logger.LogDebug("Adding document {DocumentId}: {Title}", document.Id, document.Title);

        try
        {
            // Chunk the document for better retrieval
            var chunks = _chunker.ChunkDocument(document);

            if (!chunks.Any())
            {
                _logger.LogWarning("Document {DocumentId} produced no chunks, storing as single document", document.Id);
                chunks = new List<DocumentChunk>
                {
                    new DocumentChunk
                    {
                        DocumentId = document.Id,
                        Content = document.Content,
                        ChunkIndex = 0,
                        TotalChunks = 1,
                        Title = document.Title,
                        Category = document.Category,
                        Tags = document.Tags,
                        Source = document.Source,
                        Metadata = document.Metadata
                    }
                };
            }

            _logger.LogDebug("Document {DocumentId} split into {ChunkCount} chunks", document.Id, chunks.Count);

            // Store each chunk
            foreach (var chunk in chunks)
            {
                var tags = BuildTagCollection(document, chunk);

                await _memory.ImportTextAsync(
                    text: chunk.Content,
                    documentId: chunk.Id,
                    tags: tags,
                    index: KnowledgeBaseCollection,
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation(
                "Successfully added document {DocumentId} ({ChunkCount} chunks) to knowledge base",
                document.Id, chunks.Count);

            return document.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add document {DocumentId} to knowledge base", document.Id);
            throw;
        }
    }

    public async Task<List<string>> AddDocumentsAsync(List<KnowledgeDocument> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        _logger.LogInformation("Adding {Count} documents to knowledge base", documents.Count);

        var documentIds = new List<string>();

        foreach (var document in documents)
        {
            try
            {
                var id = await AddDocumentAsync(document, cancellationToken);
                documentIds.Add(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add document {DocumentId}", document.Id);
            }
        }

        _logger.LogInformation("Successfully added {Count}/{Total} documents", documentIds.Count, documents.Count);

        return documentIds;
    }

    public async Task<List<KnowledgeSearchResult>> SearchAsync(
        string query,
        int topK = 5,
        string? category = null,
        List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        _logger.LogDebug("Searching knowledge base: query={Query}, topK={TopK}, category={Category}",
            query, topK, category);

        try
        {
            // Build filter for tags
            var filter = new MemoryFilter();

            if (!string.IsNullOrWhiteSpace(category))
            {
                filter.Add("category", category);
            }

            if (tags != null && tags.Any())
            {
                foreach (var tag in tags)
                {
                    filter.Add("tag", tag);
                }
            }

            // Search using Kernel Memory
            var searchResult = await _memory.SearchAsync(
                query: query,
                index: KnowledgeBaseCollection,
                filter: filter,
                limit: topK,
                cancellationToken: cancellationToken);

            var results = new List<KnowledgeSearchResult>();

            foreach (var result in searchResult.Results)
            {
                foreach (var partition in result.Partitions)
                {
                    results.Add(new KnowledgeSearchResult
                    {
                        Chunk = new DocumentChunk
                        {
                            Id = partition.PartitionNumber.ToString(),
                            Content = partition.Text,
                            Title = result.SourceName,
                            Source = result.SourceName,
                            Metadata = partition.Tags.ToDictionary(
                                kvp => kvp.Key,
                                kvp => (object)string.Join(", ", kvp.Value))
                        },
                        Score = (float)partition.Relevance,
                        Distance = 1.0f - (float)partition.Relevance
                    });
                }
            }

            _logger.LogDebug("Found {Count} results for query: {Query}", results.Count, query);

            return results.OrderByDescending(r => r.Score).Take(topK).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search knowledge base for query: {Query}", query);
            throw;
        }
    }

    public async Task<KnowledgeDocument?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        // Note: Kernel Memory doesn't have direct document retrieval by ID
        // We'd need to implement a separate metadata store for this
        // For now, we can search by document ID in tags
        _logger.LogWarning("GetDocumentAsync not fully implemented - requires metadata store");

        return await Task.FromResult<KnowledgeDocument?>(null);
    }

    public async Task<bool> UpdateDocumentAsync(KnowledgeDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        _logger.LogInformation("Updating document {DocumentId}", document.Id);

        try
        {
            // Delete old version and add new version
            await DeleteDocumentAsync(document.Id, cancellationToken);
            await AddDocumentAsync(document, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document {DocumentId}", document.Id);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        _logger.LogInformation("Deleting document {DocumentId}", documentId);

        try
        {
            await _memory.DeleteDocumentAsync(
                documentId: documentId,
                index: KnowledgeBaseCollection,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<List<KnowledgeDocument>> ListDocumentsAsync(
        string? category = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ListDocumentsAsync not fully implemented - requires metadata store");

        // Would require a separate metadata store to implement properly
        return await Task.FromResult(new List<KnowledgeDocument>());
    }

    private TagCollection BuildTagCollection(KnowledgeDocument document, DocumentChunk chunk)
    {
        var tags = new TagCollection
        {
            { "document_id", document.Id },
            { "category", document.Category },
            { "title", document.Title },
            { "source", document.Source },
            { "chunk_index", chunk.ChunkIndex.ToString() },
            { "total_chunks", chunk.TotalChunks.ToString() },
            { "weight", document.Weight.ToString("F2") }
        };

        foreach (var tag in document.Tags)
        {
            tags.Add("tag", tag);
        }

        return tags;
    }
}
