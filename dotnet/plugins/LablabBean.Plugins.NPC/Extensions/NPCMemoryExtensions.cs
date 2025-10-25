using LablabBean.Contracts.AI.Extensions;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Plugins.NPC.Extensions;

/// <summary>
/// Extension methods for registering memory-enhanced NPC services
/// </summary>
public static class NPCMemoryExtensions
{
    /// <summary>
    /// Adds memory-enhanced NPC services to the DI container
    /// </summary>
    public static IServiceCollection AddMemoryEnhancedNPC(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add base memory service
        services.AddKernelMemoryService(configuration);

        // Add memory-enhanced dialogue system
        services.AddSingleton<MemoryEnhancedDialogueSystem>();

        // Add memory-enhanced NPC service
        services.AddSingleton<MemoryEnhancedNPCService>();

        return services;
    }
}
