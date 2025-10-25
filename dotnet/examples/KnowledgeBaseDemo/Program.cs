using LablabBean.AI.Agents.Extensions;
using LablabBean.AI.Agents.Services.KnowledgeBase;
using LablabBean.AI.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KnowledgeBaseDemo;

/// <summary>
/// Demo program showing Knowledge Base RAG system in action
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Knowledge Base RAG Demo ===\n");

        // Setup DI container
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .Build();

        // Register services
        services.AddKernelMemory(configuration);
        services.AddKnowledgeBase(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Get services
        var loader = serviceProvider.GetRequiredService<IDocumentLoader>();
        var knowledgeBase = serviceProvider.GetRequiredService<IKnowledgeBaseService>();
        var promptAugmentation = serviceProvider.GetRequiredService<IPromptAugmentationService>();

        Console.WriteLine("✓ Services initialized\n");

        // Initialize knowledge base
        await knowledgeBase.InitializeAsync();
        Console.WriteLine("✓ Knowledge base initialized\n");

        // Load documents from docs/knowledge-base
        var docsPath = Path.Combine("..", "..", "..", "..", "..", "docs", "knowledge-base");
        var fullPath = Path.GetFullPath(docsPath);

        Console.WriteLine($"Loading documents from: {fullPath}\n");

        if (!Directory.Exists(fullPath))
        {
            Console.WriteLine("⚠ Knowledge base directory not found. Please create it at:");
            Console.WriteLine($"  {fullPath}");
            return;
        }

        var documents = await loader.LoadFromDirectoryAsync(fullPath);
        Console.WriteLine($"✓ Loaded {documents.Count} documents\n");

        if (documents.Count == 0)
        {
            Console.WriteLine("⚠ No documents found. Please add markdown files to the knowledge-base directory.");
            return;
        }

        // Show loaded documents
        Console.WriteLine("Documents loaded:");
        foreach (var doc in documents)
        {
            Console.WriteLine($"  • {doc.Title} ({doc.Category}) - {doc.Content.Length} chars");
        }
        Console.WriteLine();

        // Add documents to knowledge base
        Console.WriteLine("Adding documents to knowledge base...");
        var documentIds = await knowledgeBase.AddDocumentsAsync(documents);
        Console.WriteLine($"✓ Added {documentIds.Count} documents to knowledge base\n");

        // Demo queries
        var queries = new[]
        {
            "Tell me about Draconus the Eternal",
            "What are the major cities in the realm?",
            "Are there any quests about dragons?",
            "Where can I find the Dragonbane Sword?"
        };

        Console.WriteLine("=== Running Demo Queries ===\n");

        foreach (var query in queries)
        {
            Console.WriteLine($"Query: \"{query}\"");
            Console.WriteLine(new string('-', 60));

            // Search knowledge base
            var searchResults = await knowledgeBase.SearchAsync(query, topK: 3);

            if (searchResults.Any())
            {
                Console.WriteLine($"\nFound {searchResults.Count} relevant results:\n");

                foreach (var result in searchResults)
                {
                    Console.WriteLine($"  📄 {result.Chunk.Title}");
                    Console.WriteLine($"     Category: {result.Chunk.Category}");
                    Console.WriteLine($"     Relevance: {result.Score:P1}");
                    Console.WriteLine($"     Preview: {TruncateText(result.Chunk.Content, 200)}");
                    Console.WriteLine();
                }

                // Show augmented context
                var ragContext = await promptAugmentation.AugmentQueryAsync(query, topK: 2);
                Console.WriteLine("\nAugmented Prompt Context:");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine(TruncateText(ragContext.FormattedContext, 500));
                Console.WriteLine(new string('=', 60));
            }
            else
            {
                Console.WriteLine("\n⚠ No relevant results found.");
            }

            Console.WriteLine("\n\n");
        }

        Console.WriteLine("=== Demo Complete ===");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }
}
