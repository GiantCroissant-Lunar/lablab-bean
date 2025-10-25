using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using LablabBean.AI.Agents.Configuration;

#pragma warning disable SKEXP0010

namespace LablabBean.AI.Agents.Extensions;

/// <summary>
/// Extension methods for registering Semantic Kernel services
/// </summary>
public static class ServiceCollectionExtensions
{
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
