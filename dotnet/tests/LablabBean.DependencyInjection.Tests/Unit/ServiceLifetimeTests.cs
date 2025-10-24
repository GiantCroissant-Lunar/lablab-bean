using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Tests.Unit;

public class ServiceLifetimeTests
{
    private interface ISingletonService { }
    private class SingletonService : ISingletonService { }

    private interface IScopedService { }
    private class ScopedService : IScopedService { }

    private interface ITransientService { }
    private class TransientService : ITransientService { }

    [Fact]
    public void SingletonServices_ReturnSameInstanceAcrossHierarchy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();
        var root = services.BuildHierarchicalServiceProvider("Root");

        var child = root.CreateChildContainer(s =>
        {
            s.AddSingleton<ITransientService, TransientService>();
        }, "Child");

        // Act
        var rootInstance = root.GetService<ISingletonService>();
        var childInstance = child.GetService<ISingletonService>();

        // Assert
        rootInstance.Should().NotBeNull();
        childInstance.Should().NotBeNull();
        rootInstance.Should().BeSameAs(childInstance);
    }

    [Fact]
    public void ScopedServices_AreScopedToContainer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        using var scope1 = container.CreateScope();
        using var scope2 = container.CreateScope();

        var instance1a = scope1.ServiceProvider.GetService<IScopedService>();
        var instance1b = scope1.ServiceProvider.GetService<IScopedService>();
        var instance2 = scope2.ServiceProvider.GetService<IScopedService>();

        // Assert
        instance1a.Should().NotBeNull();
        instance1b.Should().NotBeNull();
        instance2.Should().NotBeNull();

        // Same instance within same scope
        instance1a.Should().BeSameAs(instance1b);

        // Different instances across different scopes
        instance1a.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void TransientServices_ReturnNewInstanceEveryTime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITransientService, TransientService>();
        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        var instance1 = container.GetService<ITransientService>();
        var instance2 = container.GetService<ITransientService>();
        var instance3 = container.GetService<ITransientService>();

        // Assert
        instance1.Should().NotBeNull();
        instance2.Should().NotBeNull();
        instance3.Should().NotBeNull();

        instance1.Should().NotBeSameAs(instance2);
        instance2.Should().NotBeSameAs(instance3);
        instance1.Should().NotBeSameAs(instance3);
    }

    [Fact]
    public void CreateScope_ReturnsValidIServiceScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act - Call CreateScope directly on the hierarchical provider
        using var scope = ((IServiceScopeFactory)container).CreateScope();

        // Assert
        scope.Should().NotBeNull();
        scope.ServiceProvider.Should().NotBeNull();
        scope.ServiceProvider.Should().BeAssignableTo<IHierarchicalServiceProvider>();
    }

    [Fact]
    public void ScopedService_DisposedWhenScopeDisposed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<DisposableService>();
        var container = services.BuildHierarchicalServiceProvider("Test");

        DisposableService? service;

        // Act
        using (var scope = container.CreateScope())
        {
            service = scope.ServiceProvider.GetService<DisposableService>();
            service.Should().NotBeNull();
            service!.IsDisposed.Should().BeFalse();
        }

        // Assert
        service.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void IServiceProviderFactory_CreatesValidContainers()
    {
        // Arrange
        var factory = new HierarchicalServiceProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();

        // Act
        var builder = factory.CreateBuilder(services);
        var provider = factory.CreateServiceProvider(builder);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeAssignableTo<IHierarchicalServiceProvider>();

        var service = provider.GetService<ISingletonService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void FactoryBasedRegistration_WorksCorrectly()
    {
        // Arrange
        var factoryCalled = false;
        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService>(sp =>
        {
            factoryCalled = true;
            return new SingletonService();
        });

        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        var service = container.GetService<ISingletonService>();

        // Assert
        service.Should().NotBeNull();
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public void InstanceBasedRegistration_WorksCorrectly()
    {
        // Arrange
        var instance = new SingletonService();
        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService>(instance);

        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        var service = container.GetService<ISingletonService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(instance);
    }

    [Fact]
    public void TypeBasedRegistration_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ISingletonService), typeof(SingletonService));

        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        var service = container.GetService<ISingletonService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<SingletonService>();
    }

    [Fact]
    public void BuildWithOptions_ValidateScopes_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();

        var options = new ServiceProviderOptions
        {
            ValidateScopes = true
        };

        // Act
        var container = services.BuildHierarchicalServiceProvider(options, "Test");

        // Assert - Should not throw on build
        container.Should().NotBeNull();

        // Should be able to create scopes
        using var scope = container.CreateScope();
        var service = scope.ServiceProvider.GetService<IScopedService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void BuildWithValidateScopes_BoolOverload_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();

        // Act
        var container = services.BuildHierarchicalServiceProvider(validateScopes: true, name: "Test");

        // Assert
        container.Should().NotBeNull();

        using var scope = container.CreateScope();
        var service = scope.ServiceProvider.GetService<IScopedService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void MultipleScopes_CanExistSimultaneously()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        using var scope1 = container.CreateScope();
        using var scope2 = container.CreateScope();
        using var scope3 = container.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<IScopedService>();
        var service2 = scope2.ServiceProvider.GetService<IScopedService>();
        var service3 = scope3.ServiceProvider.GetService<IScopedService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service3.Should().NotBeNull();

        service1.Should().NotBeSameAs(service2);
        service2.Should().NotBeSameAs(service3);
        service1.Should().NotBeSameAs(service3);
    }

    private class DisposableService : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
