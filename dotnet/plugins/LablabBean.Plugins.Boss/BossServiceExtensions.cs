using LablabBean.Plugins.Boss.Services;
using LablabBean.Plugins.Boss.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.Plugins.Boss;

/// <summary>
/// Extension methods for registering boss system services.
/// </summary>
public static class BossServiceExtensions
{
    /// <summary>
    /// Registers all boss system services and systems.
    /// </summary>
    public static IServiceCollection AddBossSystem(this IServiceCollection services)
    {
        services.AddSingleton<IBossService, BossService>();
        services.AddTransient<BossSystem>();
        services.AddTransient<BossAISystem>();
        services.AddTransient<BossAbilitySystem>();

        return services;
    }
}
