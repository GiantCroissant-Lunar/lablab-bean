using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace LablabBean.AI.Agents.Services.KnowledgeBase;

/// <summary>
/// Chunks large documents into smaller, overlapping pieces for better retrieval
/// </summary>
public class DocumentChunker : IDocumentChunker
{
    private const int DefaultMaxChunkSize = 1000;
    private const int DefaultOverlapSize = 200;

    public List<DocumentChunk> ChunkDocument(
        KnowledgeDocument document,
        int maxChunkSize = DefaultMaxChunkSize,
        int overlapSize = DefaultOverlapSize)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            return new List<DocumentChunk>();
        }

        var textChunks = ChunkText(document.Content, maxChunkSize, overlapSize);
        var chunks = new List<DocumentChunk>();

        for (int i = 0; i < textChunks.Count; i++)
        {
            chunks.Add(new DocumentChunk
            {
                DocumentId = document.Id,
                Content = textChunks[i],
                ChunkIndex = i,
                TotalChunks = textChunks.Count,
                Title = document.Title,
                Category = document.Category,
                Tags = new List<string>(document.Tags),
                Source = document.Source,
                Metadata = new Dictionary<string, object>(document.Metadata)
                {
                    ["chunk_index"] = i,
                    ["total_chunks"] = textChunks.Count,
                    ["document_id"] = document.Id
                }
            });
        }

        return chunks;
    }

    public List<string> ChunkText(
        string text,
        int maxChunkSize = DefaultMaxChunkSize,
        int overlapSize = DefaultOverlapSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (maxChunkSize <= 0)
            throw new ArgumentException("Max chunk size must be positive", nameof(maxChunkSize));

        if (overlapSize < 0 || overlapSize >= maxChunkSize)
            throw new ArgumentException("Overlap size must be non-negative and less than max chunk size", nameof(overlapSize));

        var chunks = new List<string>();

        // Try to split on paragraph boundaries first
        var paragraphs = SplitIntoParagraphs(text);

        var currentChunk = new StringBuilder();
        var currentSize = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphLength = paragraph.Length;

            // If single paragraph is too large, split it further
            if (paragraphLength > maxChunkSize)
            {
                // Save current chunk if it has content
                if (currentSize > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentSize = 0;
                }

                // Split large paragraph by sentences
                var sentences = SplitIntoSentences(paragraph);
                chunks.AddRange(ChunkSentences(sentences, maxChunkSize, overlapSize));
                continue;
            }

            // Check if adding this paragraph exceeds max size
            if (currentSize + paragraphLength > maxChunkSize && currentSize > 0)
            {
                // Save current chunk
                chunks.Add(currentChunk.ToString().Trim());

                // Start new chunk with overlap
                var overlapText = GetOverlapText(currentChunk.ToString(), overlapSize);
                currentChunk.Clear();
                currentChunk.Append(overlapText);
                currentSize = overlapText.Length;
            }

            currentChunk.Append(paragraph);
            currentSize += paragraphLength;
        }

        // Add the last chunk if it has content
        if (currentSize > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
    }

    private List<string> SplitIntoParagraphs(string text)
    {
        // Split on double newlines or markdown headers
        var paragraphs = Regex.Split(text, @"\n\s*\n|(?=^#{1,6}\s)", RegexOptions.Multiline);
        return paragraphs.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
    }

    private List<string> SplitIntoSentences(string text)
    {
        // Simple sentence splitting (can be improved with NLP)
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
        return sentences.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private List<string> ChunkSentences(List<string> sentences, int maxChunkSize, int overlapSize)
    {
        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        var currentSize = 0;

        foreach (var sentence in sentences)
        {
            if (currentSize + sentence.Length > maxChunkSize && currentSize > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());

                var overlapText = GetOverlapText(currentChunk.ToString(), overlapSize);
                currentChunk.Clear();
                currentChunk.Append(overlapText);
                currentSize = overlapText.Length;
            }

            currentChunk.Append(sentence);
            currentChunk.Append(" ");
            currentSize += sentence.Length + 1;
        }

        if (currentSize > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    private string GetOverlapText(string text, int overlapSize)
    {
        if (text.Length <= overlapSize)
            return text;

        // Try to get overlap at sentence boundary
        var lastPart = text[^overlapSize..];
        var sentenceEnd = lastPart.LastIndexOfAny(new[] { '.', '!', '?' });

        if (sentenceEnd > 0)
        {
            return lastPart[(sentenceEnd + 1)..].TrimStart();
        }

        return lastPart;
    }
}
