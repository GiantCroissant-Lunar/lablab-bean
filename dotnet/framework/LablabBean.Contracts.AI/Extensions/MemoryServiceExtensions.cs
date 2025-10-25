using LablabBean.Contracts.AI.Memory;
using LablabBean.Contracts.AI.Configuration;
using LablabBean.Contracts.AI.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

namespace LablabBean.Contracts.AI.Extensions;

public static class MemoryServiceExtensions
{
    public static IServiceCollection AddKernelMemoryService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KernelMemoryOptions>(
            configuration.GetSection(KernelMemoryOptions.SectionName));

        services.AddHttpClient("QdrantHealthCheck")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddSingleton<IKernelMemory>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KernelMemoryBuilder>>();
            var options = configuration.GetSection(KernelMemoryOptions.SectionName).Get<KernelMemoryOptions>();

            var builder = new KernelMemoryBuilder();

            // Configure storage provider with graceful fallback
            if (options?.Storage != null)
            {
                try
                {
                    switch (options.Storage.Provider?.ToLowerInvariant())
                    {
                        case "qdrant":
                            if (!string.IsNullOrWhiteSpace(options.Storage.ConnectionString))
                            {
                                logger.LogInformation("Configuring Qdrant vector store at {Endpoint}",
                                    options.Storage.ConnectionString);

                                builder.WithQdrantMemoryDb(
                                    endpoint: options.Storage.ConnectionString);

                                logger.LogInformation("Kernel Memory configured with Qdrant persistent storage");
                            }
                            else
                            {
                                logger.LogWarning(
                                    "Qdrant connection string not configured, falling back to volatile memory");
                                builder.WithSimpleVectorDb();
                            }
                            break;

                        case "volatile":
                        case "simple":
                        default:
                            logger.LogInformation("Using volatile (in-memory) vector store");
                            builder.WithSimpleVectorDb();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Failed to configure {Provider} storage, falling back to volatile memory",
                        options.Storage.Provider);
                    builder.WithSimpleVectorDb();
                }
            }
            else
            {
                logger.LogWarning("No storage configuration found, using volatile memory");
                builder.WithSimpleVectorDb();
            }

            return builder.Build<MemoryServerless>();
        });

        services.AddSingleton<IMemoryService, KernelMemoryService>();

        return services;
    }

    public static IServiceCollection AddQdrantHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(KernelMemoryOptions.SectionName).Get<KernelMemoryOptions>();

        if (options?.Storage?.Provider?.Equals("Qdrant", StringComparison.OrdinalIgnoreCase) == true
            && !string.IsNullOrWhiteSpace(options.Storage.ConnectionString))
        {
            services.AddHealthChecks()
                .AddCheck<QdrantHealthCheck>(
                    "qdrant",
                    tags: new[] { "ready", "vector-db" });

            services.AddSingleton(sp =>
                new QdrantHealthCheck(
                    options.Storage.ConnectionString,
                    sp.GetRequiredService<ILogger<QdrantHealthCheck>>(),
                    sp.GetRequiredService<IHttpClientFactory>()));
        }

        return services;
    }
}
