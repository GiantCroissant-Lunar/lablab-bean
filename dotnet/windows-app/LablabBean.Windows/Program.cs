using LablabBean.Infrastructure.Extensions;
using LablabBean.Plugins.Core;
using LablabBean.Reactive.Extensions;
using LablabBean.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SadConsole;
using SadConsole.Configuration;
using Serilog;

try
{
    // Build configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .WriteTo.File("logs/lablab-bean-windows-.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    Log.Information("Starting Lablab Bean Windows application");

    // Build DI container
    var services = new ServiceCollection();
    services.AddLablabBeanInfrastructure(configuration);
    services.AddLablabBeanReactive();
    
    // Add plugin system (note: requires manual start/stop since not using Generic Host)
    services.AddPluginSystem(configuration);
    
    services.AddSingleton<RootScreen>();

    var serviceProvider = services.BuildServiceProvider();

    // TODO: Start plugin system manually when needed
    // var pluginService = serviceProvider.GetServices<IHostedService>()
    //     .FirstOrDefault(s => s.GetType().Name.Contains("PluginLoader"));
    // if (pluginService != null) await pluginService.StartAsync(CancellationToken.None);

    // Configure SadConsole
    var builder = new Builder()
        .SetScreenSize(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
        .SetStartingScreen<RootScreen>()
        .IsStartingScreenFocused(true)
        .ConfigureFonts(true);

    // Start SadConsole
    Game.Create(builder);
    Game.Instance.MonoGameInstance.WindowTitle = "Lablab Bean - SadConsole";
    
    // Set the service provider for dependency injection
    Game.Instance.Services = serviceProvider;

    Game.Instance.Run();
    Game.Instance.Dispose();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
