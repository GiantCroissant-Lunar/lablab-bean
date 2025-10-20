using LablabBean.Console.Services;
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
            
            // Add application services
            services.AddSingleton<ITerminalGuiService, TerminalGuiService>();
            services.AddSingleton<IMenuService, MenuService>();
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
