using LablabBean.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LablabBean.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLablabBeanInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings
        services.Configure<AppSettings>(configuration.GetSection("App"));

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    public static IHostBuilder UseLablabBeanInfrastructure(this IHostBuilder builder)
    {
        return builder
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            });
    }
}
