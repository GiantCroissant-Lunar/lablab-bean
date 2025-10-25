namespace LablabBean.AI.Agents.Configuration;

/// <summary>
/// Configuration options for Semantic Kernel
/// </summary>
public class SemanticKernelOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model ID for text generation (e.g., "gpt-4o", "gpt-4-turbo")
    /// </summary>
    public string ModelId { get; set; } = "gpt-4o";

    /// <summary>
    /// Model ID for embeddings (e.g., "text-embedding-3-small")
    /// </summary>
    public string EmbeddingModelId { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Organization ID (optional)
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// Max tokens for completion
    /// </summary>
    public int MaxTokens { get; set; } = 500;

    /// <summary>
    /// Temperature for generation (0.0 - 2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable response caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Validate configuration
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(ModelId);
    }
}

/// <summary>
/// Configuration for Akka.NET persistence
/// </summary>
public class AkkaPersistenceOptions
{
    public const string SectionName = "Akka:Persistence";

    public string ConnectionString { get; set; } = "Data Source=avatars.db";
    public string ProviderName { get; set; } = "Microsoft.Data.Sqlite";
    public bool AutoInitialize { get; set; } = true;
}
