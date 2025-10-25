namespace LablabBean.AI.Core.Models;

/// <summary>
/// Context provided to the LLM for Retrieval Augmented Generation
/// </summary>
public class RagContext
{
    /// <summary>
    /// The original user query
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Retrieved documents/chunks relevant to the query
    /// </summary>
    public List<KnowledgeSearchResult> RetrievedDocuments { get; set; } = new();

    /// <summary>
    /// Formatted context string to inject into the prompt
    /// </summary>
    public string FormattedContext { get; set; } = string.Empty;

    /// <summary>
    /// Citations for the sources used
    /// </summary>
    public List<string> Citations { get; set; } = new();

    /// <summary>
    /// Metadata about the retrieval
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Timestamp when this context was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Format the context for injection into prompts
    /// </summary>
    public string FormatForPrompt()
    {
        if (!RetrievedDocuments.Any())
        {
            return string.Empty;
        }

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine("# Relevant Knowledge Base Context\n");

        for (int i = 0; i < RetrievedDocuments.Count; i++)
        {
            var result = RetrievedDocuments[i];
            contextBuilder.AppendLine($"## Source {i + 1}: {result.Chunk.Title}");
            contextBuilder.AppendLine($"Category: {result.Chunk.Category}");
            contextBuilder.AppendLine($"Relevance: {result.Score:P1}\n");
            contextBuilder.AppendLine(result.Chunk.Content);
            contextBuilder.AppendLine($"\n[Source: {result.Chunk.Source}]\n");
            contextBuilder.AppendLine("---\n");
        }

        FormattedContext = contextBuilder.ToString();
        return FormattedContext;
    }

    /// <summary>
    /// Get formatted citations
    /// </summary>
    public string GetCitations()
    {
        if (!RetrievedDocuments.Any())
        {
            return string.Empty;
        }

        var uniqueSources = RetrievedDocuments
            .Select(r => r.Chunk.Source)
            .Distinct()
            .ToList();

        Citations = uniqueSources;
        return string.Join(", ", uniqueSources.Select((s, i) => $"[{i + 1}] {s}"));
    }
}
