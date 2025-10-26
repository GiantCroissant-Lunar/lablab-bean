using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Core.Interfaces;

/// <summary>
/// Service for managing knowledge base documents and retrieval
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Add a document to the knowledge base
    /// </summary>
    Task<string> AddDocumentAsync(KnowledgeDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple documents in batch
    /// </summary>
    Task<List<string>> AddDocumentsAsync(List<KnowledgeDocument> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for relevant documents
    /// </summary>
    Task<List<KnowledgeSearchResult>> SearchAsync(
        string query,
        int topK = 5,
        string? category = null,
        List<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    Task<KnowledgeDocument?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing document
    /// </summary>
    Task<bool> UpdateDocumentAsync(KnowledgeDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document
    /// </summary>
    Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all documents in a category
    /// </summary>
    Task<List<KnowledgeDocument>> ListDocumentsAsync(
        string? category = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the knowledge base is initialized and ready
    /// </summary>
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialize or ensure the knowledge base collection exists
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for loading documents from various sources
/// </summary>
public interface IDocumentLoader
{
    /// <summary>
    /// Load documents from a directory of markdown files
    /// </summary>
    Task<List<KnowledgeDocument>> LoadFromDirectoryAsync(
        string directoryPath,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Load a single document from a file
    /// </summary>
    Task<KnowledgeDocument> LoadFromFileAsync(
        string filePath,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parse markdown with front matter
    /// </summary>
    KnowledgeDocument ParseMarkdown(string content, string source, string? defaultCategory = null);
}

/// <summary>
/// Service for chunking large documents
/// </summary>
public interface IDocumentChunker
{
    /// <summary>
    /// Split a document into smaller chunks
    /// </summary>
    List<DocumentChunk> ChunkDocument(
        KnowledgeDocument document,
        int maxChunkSize = 1000,
        int overlapSize = 200);

    /// <summary>
    /// Split text into chunks with overlap
    /// </summary>
    List<string> ChunkText(
        string text,
        int maxChunkSize = 1000,
        int overlapSize = 200);
}

/// <summary>
/// Service for augmenting prompts with retrieved context
/// </summary>
public interface IPromptAugmentationService
{
    /// <summary>
    /// Augment a user query with relevant knowledge base context
    /// </summary>
    Task<RagContext> AugmentQueryAsync(
        string query,
        int topK = 5,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Build an augmented prompt with context
    /// </summary>
    string BuildAugmentedPrompt(
        string systemPrompt,
        string userQuery,
        RagContext context);
}
