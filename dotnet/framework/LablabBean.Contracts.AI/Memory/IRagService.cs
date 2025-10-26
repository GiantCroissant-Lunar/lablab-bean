namespace LablabBean.Contracts.AI.Memory;

/// <summary>
/// Service for RAG (Retrieval-Augmented Generation) queries against knowledge bases
/// Provides grounded answers with citations from indexed documents
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Index a document into the knowledge base for semantic search
    /// </summary>
    /// <param name="document">Document to index</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when indexing is done</returns>
    Task IndexDocumentAsync(
        KnowledgeBaseDocument document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the knowledge base and get a grounded answer with citations
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="role">Optional role filter (e.g., "employee", "boss")</param>
    /// <param name="category">Optional category filter (e.g., "policy", "handbook")</param>
    /// <param name="maxCitations">Maximum number of citations to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Answer with supporting citations</returns>
    Task<KnowledgeBaseAnswer> QueryKnowledgeBaseAsync(
        string query,
        string? role = null,
        string? category = null,
        int maxCitations = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the knowledge base service is healthy and available
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is healthy</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document from the knowledge base
    /// </summary>
    /// <param name="documentId">Document ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when deletion is done</returns>
    Task DeleteDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);
}
