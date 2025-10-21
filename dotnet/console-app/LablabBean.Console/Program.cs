using LablabBean.Console.Services;
using LablabBean.Game.Core.Services;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using LablabBean.Game.TerminalUI.Services;
using LablabBean.Infrastructure.Extensions;
using LablabBean.Reactive.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseLablabBeanInfrastructure()
        .ConfigureServices((context, services) =>
        {
            services.AddLablabBeanInfrastructure(context.Configuration);
            services.AddLablabBeanReactive();

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
