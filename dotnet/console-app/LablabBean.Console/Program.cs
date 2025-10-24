using System.CommandLine;
using LablabBean.Console.Commands;
using LablabBean.Console.Services;
using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.TerminalUI.Services;
using LablabBean.Infrastructure.Extensions;
using LablabBean.Plugins.Core;
using LablabBean.Reactive.Extensions;
using LablabBean.Reporting.Contracts.Contracts;
using LablabBean.Plugins.Reporting.Html;
using LablabBean.Plugins.Reporting.Csv;
using LablabBean.Reporting.Providers.Build;
using LablabBean.Reporting.Analytics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

try
{
    // Check if CLI arguments are provided (report commands)
    if (args.Length > 0 && args[0] == "report")
    {
        // Build DI container for reporting
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Register report providers (will use source generator auto-registration when available)
        // For now, register manually until source generator is verified working
        services.AddTransient<BuildMetricsProvider>();
        services.AddTransient<SessionStatisticsProvider>();
        services.AddTransient<PluginHealthProvider>();

        // Register renderers
        services.AddSingleton<IReportRenderer, HtmlReportRenderer>();
        services.AddSingleton<IReportRenderer, CsvReportRenderer>();

        var serviceProvider = services.BuildServiceProvider();

        var rootCommand = new RootCommand("LablabBean Console - Dungeon Crawler Game & Reporting Tool");
        rootCommand.AddCommand(ReportCommand.Create(serviceProvider));

        return await rootCommand.InvokeAsync(args);
    }

    // Plugins CLI (discovery / listing without starting TUI)
    if (args.Length > 0 && args[0] == "plugins")
    {
        var rootCommand = new RootCommand("LablabBean Console - Plugins CLI");
        rootCommand.AddCommand(PluginsCommand.Create());
        return await rootCommand.InvokeAsync(args);
    }

    // Otherwise, run interactive Terminal.Gui mode
    var host = Host.CreateDefaultBuilder(args)
        .UseLablabBeanInfrastructure()
        .ConfigureServices((context, services) =>
        {
            services.AddLablabBeanInfrastructure(context.Configuration);
            services.AddLablabBeanReactive();

            // Add plugin system
            services.AddPluginSystem(context.Configuration);

            // Add game framework services
            services.AddSingleton<GameWorldManager>();
            services.AddSingleton<MovementSystem>();
            services.AddSingleton<CombatSystem>();
            services.AddSingleton<AISystem>();
            services.AddSingleton<ActorSystem>();
            services.AddSingleton<InventorySystem>();
            services.AddSingleton<ItemSpawnSystem>();
            services.AddSingleton<StatusEffectSystem>();
            services.AddSingleton<GameStateManager>();

            // Add Terminal.Gui rendering services
            services.AddSingleton<HudService>();
            services.AddSingleton<WorldViewService>();

            // Add application services
            services.AddSingleton<ITerminalGuiService, TerminalGuiService>();
            services.AddSingleton<IMenuService, MenuService>();
            services.AddSingleton<DungeonCrawlerService>();
            services.AddHostedService<ConsoleHostedService>();
        })
        .Build();

    await host.RunAsync();
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
