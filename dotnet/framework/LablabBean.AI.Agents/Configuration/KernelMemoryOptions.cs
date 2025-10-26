namespace LablabBean.AI.Agents.Configuration;

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

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Storage.Provider))
        {
            throw new InvalidOperationException("Storage provider must be specified");
        }

        if (Storage.Provider.Equals("Qdrant", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(Storage.ConnectionString))
            {
                throw new InvalidOperationException("Qdrant connection string is required when provider is 'Qdrant'");
            }

            if (!Uri.TryCreate(Storage.ConnectionString, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                throw new InvalidOperationException($"Invalid Qdrant connection string: '{Storage.ConnectionString}'. Must be a valid HTTP/HTTPS URL.");
            }
        }

        if (string.IsNullOrWhiteSpace(Embedding.Provider))
        {
            throw new InvalidOperationException("Embedding provider must be specified");
        }

        if (Embedding.MaxTokens <= 0)
        {
            throw new InvalidOperationException("Embedding MaxTokens must be greater than 0");
        }
    }
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
