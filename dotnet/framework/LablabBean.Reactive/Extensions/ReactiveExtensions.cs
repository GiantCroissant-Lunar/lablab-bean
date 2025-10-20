using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace LablabBean.Reactive.Extensions;

public static class ReactiveExtensions
{
    public static IServiceCollection AddLablabBeanReactive(this IServiceCollection services)
    {
        // Add MessagePipe
        services.AddMessagePipe();

        // Add ObservableCollections support
        // (No explicit registration needed, just ensure package is referenced)

        return services;
    }
}
