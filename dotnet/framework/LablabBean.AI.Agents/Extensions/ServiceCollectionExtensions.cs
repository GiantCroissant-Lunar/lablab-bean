using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using LablabBean.AI.Agents.Configuration;
using LablabBean.AI.Core.Interfaces;

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
        // Add Semantic Kernel with OpenAI
        services.AddSemanticKernelWithOpenAI(configuration);

        // Register personality loaders
        services.AddSingleton<BossPersonalityLoader>();
        services.AddSingleton<EmployeePersonalityLoader>();

        // Register intelligence agents
        services.AddSingleton<IIntelligenceAgent, BossIntelligenceAgent>();
        services.AddSingleton<IIntelligenceAgent, EmployeeIntelligenceAgent>();
        services.AddSingleton<TacticsAgent>();

        // Register factories
        services.AddSingleton<BossFactory>();
        services.AddSingleton<EmployeeFactory>();

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

        if (!options.IsValid())
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
