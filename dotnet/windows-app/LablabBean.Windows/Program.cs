using LablabBean.Infrastructure.Extensions;
using LablabBean.Plugins.Core;
using LablabBean.Reactive.Extensions;
using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;
using LablabBean.Game.SadConsole.Screens;
using LablabBean.Game.SadConsole.Services;
using LablabBean.Windows;
using LablabBean.Reporting.Analytics;
using LablabBean.Reporting.Contracts;
using LablabBean.Plugins.Reporting.Html;
using LablabBean.Plugins.Reporting.Csv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Configuration;
using Serilog;
using System.Reflection;

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

    // Add logging services
    services.AddLogging(builder =>
    {
        builder.AddSerilog(dispose: true);
    });

    services.AddLablabBeanInfrastructure(configuration);
    services.AddLablabBeanReactive();

    // Add plugin system (note: requires manual start/stop since not using Generic Host)
    services.AddPluginSystem(configuration);

    // Add reporting services
    services.AddTransient<SessionStatisticsProvider>();
    services.AddTransient<PluginHealthProvider>();
    services.AddSingleton<AdvancedAnalyticsCollector>();
    services.AddSingleton<PersistenceService>();
    services.AddSingleton(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<AchievementSystem>>();
        var sessionId = Guid.NewGuid().ToString();
        return new AchievementSystem(logger, sessionId);
    });
    services.AddSingleton(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<LeaderboardSystem>>();
        var persistence = sp.GetRequiredService<PersistenceService>();
        return new LeaderboardSystem(logger, persistence);
    });
    services.AddSingleton<SessionMetricsCollector>();
    services.AddSingleton<ReportExportService>();

    // Register game services required by GameScreen
    services.AddSingleton<GameWorldManager>();
    services.AddSingleton<MovementSystem>();
    services.AddSingleton<CombatSystem>();
    services.AddSingleton<AISystem>();
    services.AddSingleton<ActorSystem>();
    services.AddSingleton<InventorySystem>();
    services.AddSingleton<ItemSpawnSystem>();
    services.AddSingleton<StatusEffectSystem>();
    services.AddSingleton<LevelManager>();
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

    // Start SadConsole with a callback to set up the screen after initialization
    Game.Create(builder);

    // SadConsole is now initialized, safe to create screens
    Game.Instance.Started += (sender, args) =>
    {
        // Create and set the starting screen using DI
        var gameScreen = ActivatorUtilities.CreateInstance<GameScreen>(serviceProvider, width, height);
        gameScreen.Initialize();
        Game.Instance.Screen = gameScreen;

        // Hook combat events for metrics collection
        var metricsCollector = serviceProvider.GetRequiredService<SessionMetricsCollector>();
        var advancedAnalytics = serviceProvider.GetRequiredService<AdvancedAnalyticsCollector>();
        var gameStateManager = serviceProvider.GetRequiredService<GameStateManager>();
        var combatSystem = gameStateManager.CombatSystem;
        var inventorySystem = gameStateManager.InventorySystem;
        var levelManager = gameStateManager.LevelManager;

        if (combatSystem != null)
        {
            combatSystem.OnEntityDied += entity =>
            {
                var world = gameStateManager.WorldManager.GetWorld(GameMode.Play);

                if (world.Has<Player>(entity))
                {
                    metricsCollector.TotalDeaths++;
                    Log.Information("Player died. Total deaths: {Deaths}", metricsCollector.TotalDeaths);
                }
                else if (world.Has<Enemy>(entity))
                {
                    metricsCollector.TotalKills++;

                    // Track enemy type (randomly assign for now)
                    var enemyTypes = Enum.GetValues<LablabBean.Reporting.Contracts.Models.EnemyType>();
                    var randomType = enemyTypes[Random.Shared.Next(enemyTypes.Length)];
                    advancedAnalytics.RecordEnemyKilled(randomType);

                    Log.Information("Enemy killed. Total kills: {Kills}", metricsCollector.TotalKills);
                }
            };

            // Hook damage tracking
            combatSystem.OnDamageDealt += (attacker, target, damage) =>
            {
                var world = gameStateManager.WorldManager.GetWorld(GameMode.Play);
                if (world.Has<Player>(attacker))
                {
                    bool isCritical = Random.Shared.Next(100) < 15; // 15% crit chance simulation
                    advancedAnalytics.RecordDamageDealt(damage, isCritical);
                }
                else if (world.Has<Player>(target))
                {
                    advancedAnalytics.RecordDamageTaken(damage);
                }
            };

            // Hook healing tracking
            combatSystem.OnHealed += (entity, healAmount) =>
            {
                var world = gameStateManager.WorldManager.GetWorld(GameMode.Play);
                if (world.Has<Player>(entity))
                {
                    advancedAnalytics.RecordHealing(healAmount);
                    Log.Information("Healing received: {Amount}", healAmount);
                }
            };

            // Hook dodge tracking
            combatSystem.OnAttackMissed += (attacker, target) =>
            {
                var world = gameStateManager.WorldManager.GetWorld(GameMode.Play);
                if (world.Has<Player>(target))
                {
                    advancedAnalytics.RecordPerfectDodge();
                    Log.Information("Perfect dodge!");
                }
            };
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnItemPickedUp += (playerEntity, itemEntity) =>
            {
                metricsCollector.ItemsCollected++;

                // Track item type (randomly assign for now)
                var itemTypes = Enum.GetValues<LablabBean.Reporting.Contracts.Models.ItemType>();
                var randomType = itemTypes[Random.Shared.Next(itemTypes.Length)];
                advancedAnalytics.RecordItemCollected(randomType);

                Log.Information("Item collected. Total items: {Items}", metricsCollector.ItemsCollected);
            };
        }

        if (levelManager != null)
        {
            levelManager.OnLevelCompleted += levelNumber =>
            {
                metricsCollector.LevelsCompleted++;
                advancedAnalytics.EndLevel();

                // Start next level tracking
                advancedAnalytics.StartLevel();

                Log.Information("Level {Level} completed. Total levels: {Total}", levelNumber, metricsCollector.LevelsCompleted);
            };

            levelManager.OnNewDepthReached += depth =>
            {
                metricsCollector.MaxDepth = Math.Max(metricsCollector.MaxDepth, depth);
                Log.Information("New depth record: Level {Depth}", depth);
            };

            levelManager.OnDungeonCompleted += () =>
            {
                metricsCollector.DungeonsCompleted++;
                advancedAnalytics.EndDungeon();
                Log.Information("Dungeon completed! Total dungeons: {Count}", metricsCollector.DungeonsCompleted);
            };

            // Start initial level tracking
            advancedAnalytics.StartLevel();
            advancedAnalytics.StartDungeon();
        }

        Log.Information("Game metrics and advanced analytics collection initialized");
    };

    Game.Instance.Run();
    Game.Instance.Dispose();

    // Export session report before exit
    var metricsCollector = serviceProvider.GetService<SessionMetricsCollector>();
    if (metricsCollector != null)
    {
        try
        {
            // Get version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                       ?? assembly.GetName().Version?.ToString()
                       ?? "0.1.0-dev";

            var reportDir = Path.Combine("build", "_artifacts", version, "reports", "sessions");
            var reportPath = Path.Combine(reportDir, $"windows-session-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
            await metricsCollector.ExportSessionReportAsync(reportPath, LablabBean.Reporting.Contracts.Models.ReportFormat.HTML);
            Log.Information("Session report exported to {Path}", reportPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to export session report");
        }
        finally
        {
            metricsCollector.Dispose();
        }
    }

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
