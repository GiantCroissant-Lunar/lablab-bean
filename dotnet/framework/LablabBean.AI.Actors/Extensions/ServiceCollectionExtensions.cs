using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.AI.Actors.Extensions;

/// <summary>
/// Extension methods for registering Akka.NET services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Akka.NET actors for intelligent avatars (convenience method)
    /// </summary>
    public static IServiceCollection AddAkkaActors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Akka:Persistence:ConnectionString"] ?? "Data Source=avatars.db";
        return services.AddAkkaWithPersistence(connectionString);
    }

    /// <summary>
    /// Add Akka.NET actor system for AI avatars
    /// </summary>
    public static IServiceCollection AddAkkaActorSystem(
        this IServiceCollection services,
        Action<AkkaConfigurationBuilder>? configure = null)
    {
        services.AddAkka("AvatarActorSystem", (builder, provider) =>
        {
            builder
                .ConfigureLoggers(setup =>
                {
                    setup.ClearLoggers();
                    setup.AddLoggerFactory();
                })
                .WithActors((system, registry) =>
                {
                    var logger = provider.GetRequiredService<ILoggerFactory>();
                    var eventBusAdapter = system.ActorOf(
                        Bridges.EventBusAkkaAdapter.Props(),
                        "event-bus-adapter");

                    registry.Register<Bridges.EventBusAkkaAdapter>(eventBusAdapter);
                });

            configure?.Invoke(builder);
        });

        return services;
    }

    /// <summary>
    /// Add Akka.NET with persistence support
    /// </summary>
    public static IServiceCollection AddAkkaWithPersistence(
        this IServiceCollection services,
        string connectionString,
        Action<AkkaConfigurationBuilder>? configure = null)
    {
        services.AddAkka("AvatarActorSystem", (builder, provider) =>
        {
            builder
                .ConfigureLoggers(setup =>
                {
                    setup.ClearLoggers();
                    setup.AddLoggerFactory();
                })
                .WithActors((system, registry) =>
                {
                    var eventBusAdapter = system.ActorOf(
                        Bridges.EventBusAkkaAdapter.Props(),
                        "event-bus-adapter");

                    registry.Register<Bridges.EventBusAkkaAdapter>(eventBusAdapter);
                });

            configure?.Invoke(builder);
        });

        return services;
    }
}
