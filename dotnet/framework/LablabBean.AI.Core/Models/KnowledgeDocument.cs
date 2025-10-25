namespace LablabBean.AI.Core.Models;

/// <summary>
/// Represents a document in the knowledge base with metadata
/// </summary>
public class KnowledgeDocument
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the document
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Main content of the document
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category/type of the document (e.g., "lore", "quest", "location", "item")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Tags for filtering and classification
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Source file path or URL
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// When the document was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the document was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Importance/weight of this document (0.0-1.0)
    /// </summary>
    public float Weight { get; set; } = 1.0f;
}

/// <summary>
/// A chunk of a larger document for better retrieval
/// </summary>
public class DocumentChunk
{
    /// <summary>
    /// Unique identifier for the chunk
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Reference to the parent document ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// The chunk content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Position of this chunk in the document (0-based)
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Total number of chunks in the document
    /// </summary>
    public int TotalChunks { get; set; }

    /// <summary>
    /// Inherit document metadata
    /// </summary>
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result from a knowledge base search with relevance score
/// </summary>
public class KnowledgeSearchResult
{
    /// <summary>
    /// The matching document or chunk
    /// </summary>
    public DocumentChunk Chunk { get; set; } = new();

    /// <summary>
    /// Relevance score (0.0-1.0)
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Distance from query (lower is better)
    /// </summary>
    public float Distance { get; set; }
}
