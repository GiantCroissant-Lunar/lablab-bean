using LablabBean.Infrastructure.Extensions;
using LablabBean.Plugins.Core;
using LablabBean.Reactive.Extensions;
using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.SadConsole.Screens;
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
    
    // Register game services required by GameScreen
    services.AddSingleton<GameWorldManager>();
    services.AddSingleton<MovementSystem>();
    services.AddSingleton<CombatSystem>();
    services.AddSingleton<AISystem>();
    services.AddSingleton<ActorSystem>();
    services.AddSingleton<InventorySystem>();
    services.AddSingleton<ItemSpawnSystem>();
    services.AddSingleton<StatusEffectSystem>();
    services.AddSingleton<GameStateManager>();

    var serviceProvider = services.BuildServiceProvider();

    // TODO: Start plugin system manually when needed
    // var pluginService = serviceProvider.GetServices<IHostedService>()
    //     .FirstOrDefault(s => s.GetType().Name.Contains("PluginLoader"));
    // if (pluginService != null) await pluginService.StartAsync(CancellationToken.None);

    // Configure SadConsole
    var width = GameSettings.GAME_WIDTH;
    var height = GameSettings.GAME_HEIGHT;

    var builder = new Builder()
        .SetScreenSize(width, height)
        .IsStartingScreenFocused(true)
        .ConfigureFonts(true);

    // Start SadConsole
    Game.Create(builder);
    // Optionally set window title if supported by current SadConsole/MonoGame version
    // (Removed direct assignment to avoid API mismatch)

    // Create and set the starting screen using DI
    var gameScreen = ActivatorUtilities.CreateInstance<GameScreen>(serviceProvider, width, height);
    gameScreen.Initialize();
    Game.Instance.Screen = gameScreen;

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
