namespace LablabBean.Contracts.AI.Configuration;

/// <summary>
/// Configuration options for Kernel Memory integration
/// </summary>
public class KernelMemoryOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "KernelMemory";

    /// <summary>
    /// Embedding provider configuration
    /// </summary>
    public EmbeddingOptions Embedding { get; set; } = new();

    /// <summary>
    /// Storage provider configuration
    /// </summary>
    public StorageOptions Storage { get; set; } = new();

    /// <summary>
    /// Text generation provider configuration
    /// </summary>
    public TextGenerationOptions TextGeneration { get; set; } = new();
}

/// <summary>
/// Embedding provider options
/// </summary>
public class EmbeddingOptions
{
    /// <summary>
    /// Provider name (e.g., "OpenAI", "AzureOpenAI")
    /// </summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>
    /// Model name for embeddings
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Maximum tokens for embedding
    /// </summary>
    public int MaxTokens { get; set; } = 8191;

    /// <summary>
    /// API endpoint (for custom endpoints)
    /// </summary>
    public string? Endpoint { get; set; }
}

/// <summary>
/// Storage provider options
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Storage provider name (e.g., "Volatile", "Qdrant", "AzureAISearch")
    /// </summary>
    public string Provider { get; set; } = "Volatile";

    /// <summary>
    /// Connection string for the storage provider
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Collection/index name
    /// </summary>
    public string? CollectionName { get; set; }
}

/// <summary>
/// Text generation provider options
/// </summary>
public class TextGenerationOptions
{
    /// <summary>
    /// Provider name (e.g., "OpenAI", "AzureOpenAI")
    /// </summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>
    /// Model name for text generation
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Maximum tokens for generation
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// API endpoint (for custom endpoints)
    /// </summary>
    public string? Endpoint { get; set; }
}
