namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for registering plugin services with DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add plugin system services to the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root (optional, will be resolved from DI if null)</param>
    public static IServiceCollection AddPluginSystem(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration != null)
        {
            services.Configure<PluginOptions>(configuration.GetSection(PluginOptions.SectionName));
        }

        services.AddSingleton<PluginRegistry>();
        services.AddSingleton<ServiceRegistry>();
        services.AddSingleton<IPluginRegistry>(sp => sp.GetRequiredService<PluginRegistry>());
        services.AddSingleton<IRegistry>(sp => sp.GetRequiredService<ServiceRegistry>());
        
        services.AddSingleton(sp =>
        {
            var options = sp.GetService<IOptions<PluginOptions>>()?.Value ?? new PluginOptions();
            return new PluginLoader(
                sp.GetRequiredService<ILogger<PluginLoader>>(),
                sp.GetRequiredService<ILoggerFactory>(),
                sp.GetRequiredService<IConfiguration>(),
                sp,
                sp.GetRequiredService<PluginRegistry>(),
                sp.GetRequiredService<ServiceRegistry>(),
                options.HotReload,
                options.Profile
            );
        });

        services.AddHostedService<PluginLoaderHostedService>();

        return services;
    }
}
