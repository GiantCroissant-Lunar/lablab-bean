using System.CommandLine;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Console.Commands;

public static class KnowledgeBaseCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var kbCommand = new Command("kb", "Manage the knowledge base for NPCs (lore, quests, locations)");

        kbCommand.AddCommand(CreateIngestCommand(serviceProvider));
        kbCommand.AddCommand(CreateQueryCommand(serviceProvider));
        kbCommand.AddCommand(CreateDeleteCommand(serviceProvider));
        kbCommand.AddCommand(CreateListCommand(serviceProvider));

        return kbCommand;
    }

    private static Command CreateIngestCommand(IServiceProvider serviceProvider)
    {
        var command = new Command("ingest", "Load documents into the knowledge base");

        var pathOption = new Option<FileInfo>(
            aliases: new[] { "--file", "-f" },
            description: "Path to markdown file to ingest");

        var directoryOption = new Option<DirectoryInfo>(
            aliases: new[] { "--directory", "-d" },
            description: "Path to directory containing markdown files to ingest");

        var categoryOption = new Option<string>(
            aliases: new[] { "--category", "-c" },
            description: "Category for the document(s): lore, quest, location, item");

        command.AddOption(pathOption);
        command.AddOption(directoryOption);
        command.AddOption(categoryOption);

        command.SetHandler(async (file, directory, category) =>
        {
            await HandleIngestAsync(serviceProvider, file, directory, category);
        }, pathOption, directoryOption, categoryOption);

        return command;
    }

    private static Command CreateQueryCommand(IServiceProvider serviceProvider)
    {
        var command = new Command("query", "Query the knowledge base");

        var queryOption = new Option<string>(
            aliases: new[] { "--text", "-t" },
            description: "Query text");
        queryOption.IsRequired = true;

        var limitOption = new Option<int>(
            aliases: new[] { "--limit", "-l" },
            getDefaultValue: () => 3,
            description: "Maximum number of results");

        var categoryOption = new Option<string?>(
            aliases: new[] { "--category", "-c" },
            description: "Filter by category: lore, quest, location, item");

        command.AddOption(queryOption);
        command.AddOption(limitOption);
        command.AddOption(categoryOption);

        command.SetHandler(async (query, limit, category) =>
        {
            await HandleQueryAsync(serviceProvider, query, limit, category);
        }, queryOption, limitOption, categoryOption);

        return command;
    }

    private static Command CreateDeleteCommand(IServiceProvider serviceProvider)
    {
        var command = new Command("delete", "Delete documents from the knowledge base");

        var documentIdOption = new Option<string>(
            aliases: new[] { "--id", "-i" },
            description: "Document ID to delete");

        var categoryOption = new Option<string?>(
            aliases: new[] { "--category", "-c" },
            description: "Delete all documents in category: lore, quest, location, item");

        command.AddOption(documentIdOption);
        command.AddOption(categoryOption);

        command.SetHandler(async (documentId, category) =>
        {
            await HandleDeleteAsync(serviceProvider, documentId, category);
        }, documentIdOption, categoryOption);

        return command;
    }

    private static Command CreateListCommand(IServiceProvider serviceProvider)
    {
        var command = new Command("list", "List documents in the knowledge base");

        var categoryOption = new Option<string?>(
            aliases: new[] { "--category", "-c" },
            description: "Filter by category: lore, quest, location, item");

        command.AddOption(categoryOption);

        command.SetHandler(async (category) =>
        {
            await HandleListAsync(serviceProvider, category);
        }, categoryOption);

        return command;
    }

    // Handlers

    private static async Task HandleIngestAsync(
        IServiceProvider serviceProvider,
        FileInfo? file,
        DirectoryInfo? directory,
        string? category)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IKnowledgeBaseService>>();
        var kbService = serviceProvider.GetRequiredService<IKnowledgeBaseService>();
        var documentLoader = serviceProvider.GetRequiredService<IDocumentLoader>();

        try
        {
            var documents = new List<KnowledgeDocument>();

            if (file != null)
            {
                if (!file.Exists)
                {
                    System.Console.WriteLine($"❌ Error: File not found: {file.FullName}");
                    return;
                }

                System.Console.Write($"📄 Loading {file.Name}... ");
                var doc = await documentLoader.LoadFromFileAsync(file.FullName, category);
                documents.Add(doc);
                System.Console.WriteLine("✅");
            }
            else if (directory != null)
            {
                if (!directory.Exists)
                {
                    System.Console.WriteLine($"❌ Error: Directory not found: {directory.FullName}");
                    return;
                }

                System.Console.Write($"📂 Loading documents from {directory.Name}... ");
                documents = await documentLoader.LoadFromDirectoryAsync(directory.FullName, category);
                System.Console.WriteLine($"✅ Found {documents.Count} document(s)");
            }
            else
            {
                System.Console.WriteLine("❌ Error: Please provide either --file or --directory");
                return;
            }

            if (documents.Count == 0)
            {
                System.Console.WriteLine("⚠️ No markdown files found");
                return;
            }

            System.Console.WriteLine($"\n📚 Ingesting {documents.Count} document(s)...\n");

            var documentIds = await kbService.AddDocumentsAsync(documents);

            for (int i = 0; i < documents.Count; i++)
            {
                System.Console.WriteLine($"  ✅ {documents[i].Title} → ID: {documentIds[i]}");
            }

            System.Console.WriteLine($"\n🎉 Successfully ingested {documents.Count} document(s)!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ingest documents");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task HandleQueryAsync(
        IServiceProvider serviceProvider,
        string query,
        int limit,
        string? category)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IKnowledgeBaseService>>();
        var kbService = serviceProvider.GetRequiredService<IKnowledgeBaseService>();

        try
        {
            System.Console.WriteLine($"🔍 Searching knowledge base for: \"{query}\"\n");

            var results = await kbService.SearchAsync(query, limit, category);

            if (results.Count == 0)
            {
                System.Console.WriteLine("⚠️ No results found");
                return;
            }

            System.Console.WriteLine($"Found {results.Count} result(s):\n");

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var chunk = result.Chunk;

                System.Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                System.Console.WriteLine($"Result #{i + 1} (Score: {result.Score:F3})");
                System.Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                System.Console.WriteLine($"Document ID: {chunk.DocumentId}");
                System.Console.WriteLine($"Title: {chunk.Title}");
                System.Console.WriteLine($"Category: {chunk.Category}");
                System.Console.WriteLine($"Chunk: {chunk.ChunkIndex + 1}/{chunk.TotalChunks}");
                if (chunk.Tags?.Count > 0)
                {
                    System.Console.WriteLine($"Tags: {string.Join(", ", chunk.Tags)}");
                }
                System.Console.WriteLine($"\nContent:\n{chunk.Content}\n");
                if (!string.IsNullOrEmpty(chunk.Source))
                {
                    System.Console.WriteLine($"Source: {chunk.Source}\n");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query knowledge base");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task HandleDeleteAsync(
        IServiceProvider serviceProvider,
        string? documentId,
        string? category)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IKnowledgeBaseService>>();
        var kbService = serviceProvider.GetRequiredService<IKnowledgeBaseService>();

        try
        {
            if (!string.IsNullOrEmpty(documentId))
            {
                System.Console.Write($"🗑️ Deleting document: {documentId}... ");
                var success = await kbService.DeleteDocumentAsync(documentId);

                if (success)
                {
                    System.Console.WriteLine("✅ Deleted successfully");
                }
                else
                {
                    System.Console.WriteLine("❌ Document not found");
                }
            }
            else if (!string.IsNullOrEmpty(category))
            {
                System.Console.Write($"🗑️ Deleting all documents in category '{category}'... ");
                // Note: This would require adding a DeleteByCategoryAsync method to IKnowledgeBaseService
                System.Console.WriteLine("⚠️ Not implemented yet - please specify --id for now");
            }
            else
            {
                System.Console.WriteLine("❌ Error: Please provide either --id or --category");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete documents");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task HandleListAsync(
        IServiceProvider serviceProvider,
        string? category)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IKnowledgeBaseService>>();
        var kbService = serviceProvider.GetRequiredService<IKnowledgeBaseService>();

        try
        {
            if (category != null)
            {
                System.Console.WriteLine($"📋 Listing documents in category: {category}\n");
            }
            else
            {
                System.Console.WriteLine("📋 Listing all documents in knowledge base...\n");
            }

            var documents = await kbService.ListDocumentsAsync(category);

            if (documents.Count == 0)
            {
                System.Console.WriteLine("⚠️ No documents found");
                return;
            }

            System.Console.WriteLine($"Found {documents.Count} document(s):\n");

            foreach (var doc in documents)
            {
                System.Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                System.Console.WriteLine($"ID: {doc.Id}");
                System.Console.WriteLine($"Title: {doc.Title}");
                System.Console.WriteLine($"Category: {doc.Category}");
                if (doc.Tags?.Count > 0)
                {
                    System.Console.WriteLine($"Tags: {string.Join(", ", doc.Tags)}");
                }
                System.Console.WriteLine($"Content Length: {doc.Content.Length} characters");
                if (!string.IsNullOrEmpty(doc.Source))
                {
                    System.Console.WriteLine($"Source: {doc.Source}");
                }
                System.Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list documents");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
