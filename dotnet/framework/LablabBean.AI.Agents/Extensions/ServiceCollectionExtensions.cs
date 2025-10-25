using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using LablabBean.AI.Agents.Configuration;
using LablabBean.AI.Core.Interfaces;
using LablabBean.Contracts.AI.Memory;
using LablabBean.AI.Agents.Services;
using LablabBean.AI.Agents.Services.KnowledgeBase;

#pragma warning disable SKEXP0010

namespace LablabBean.AI.Agents.Extensions;

/// <summary>
/// Extension methods for registering Semantic Kernel services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Semantic Kernel agents for intelligent avatars (convenience method)
    /// </summary>
    public static IServiceCollection AddSemanticKernelAgents(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try to add Semantic Kernel with OpenAI, but make it optional
        try
        {
            services.AddSemanticKernelWithOpenAI(configuration);
        }
        catch (InvalidOperationException)
        {
            // SK not configured - add mock kernel instead for testing
            services.AddKernel();
        }

        // Register personality loaders
        services.AddSingleton<BossPersonalityLoader>();
        services.AddSingleton<EmployeePersonalityLoader>();

        // NOTE: Intelligence agents are NOT registered as singletons
        // They are created per-entity by the IntelligentAISystem or factories
        // Each agent needs a unique agentId which can't be provided via DI

        services.AddSingleton<TacticsAgent>();

        // Register factories
        services.AddSingleton<BossFactory>();
        services.AddSingleton<EmployeeFactory>();

        return services;
    }

    /// <summary>
    /// Add Kernel Memory services for NPC memory storage and retrieval
    /// </summary>
    public static IServiceCollection AddKernelMemory(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<KernelMemoryOptions>(
            configuration.GetSection(KernelMemoryOptions.SectionName));

        // Register IKernelMemory instance
        services.AddSingleton<Microsoft.KernelMemory.IKernelMemory>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Microsoft.KernelMemory.KernelMemoryBuilder>>();

            logger.LogInformation("Initializing Kernel Memory with volatile (in-memory) storage");

            // Build Kernel Memory with simple in-memory storage
            // Note: This uses SimpleVectorDb for development/testing
            // In production (Phase 4), this will be replaced with Qdrant
            var builder = new Microsoft.KernelMemory.KernelMemoryBuilder();

            return builder.Build<Microsoft.KernelMemory.MemoryServerless>();
        });

        // Register memory service
        services.AddSingleton<IMemoryService, MemoryService>();

        return services;
    }

    /// <summary>
    /// Add Knowledge Base services for RAG (Retrieval Augmented Generation)
    /// </summary>
    public static IServiceCollection AddKnowledgeBase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register document processing services
        services.AddSingleton<IDocumentLoader, DocumentLoader>();
        services.AddSingleton<IDocumentChunker, DocumentChunker>();

        // Register knowledge base service (depends on IKernelMemory)
        services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();

        // Register prompt augmentation service
        services.AddSingleton<IPromptAugmentationService, PromptAugmentationService>();

        return services;
    }

    /// <summary>
    /// Add Semantic Kernel with OpenAI connector
    /// </summary>
    public static IServiceCollection AddSemanticKernelWithOpenAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(SemanticKernelOptions.SectionName)
            .Get<SemanticKernelOptions>() ?? new SemanticKernelOptions();

        // Check if API key is set to placeholder or missing
        if (!options.IsValid() || options.ApiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            throw new InvalidOperationException(
                "Invalid Semantic Kernel configuration. Ensure ApiKey and ModelId are set in appsettings.json");
        }

        services.AddSingleton(options);

        services.AddKernel()
            .AddOpenAIChatCompletion(
                modelId: options.ModelId,
                apiKey: options.ApiKey,
                orgId: options.OrganizationId)
            .AddOpenAITextEmbeddingGeneration(
                modelId: options.EmbeddingModelId,
                apiKey: options.ApiKey,
                orgId: options.OrganizationId);

        return services;
    }

    /// <summary>
    /// Add Semantic Kernel with custom configuration
    /// </summary>
    public static IServiceCollection AddSemanticKernelWithOpenAI(
        this IServiceCollection services,
        Action<SemanticKernelOptions> configure)
    {
        var options = new SemanticKernelOptions();
        configure(options);

        if (!options.IsValid())
        {
            throw new InvalidOperationException(
                "Invalid Semantic Kernel configuration. Ensure ApiKey and ModelId are set.");
        }

        services.AddSingleton(options);

        services.AddKernel()
            .AddOpenAIChatCompletion(
                modelId: options.ModelId,
                apiKey: options.ApiKey,
                orgId: options.OrganizationId)
            .AddOpenAITextEmbeddingGeneration(
                modelId: options.EmbeddingModelId,
                apiKey: options.ApiKey,
                orgId: options.OrganizationId);

        return services;
    }
}
