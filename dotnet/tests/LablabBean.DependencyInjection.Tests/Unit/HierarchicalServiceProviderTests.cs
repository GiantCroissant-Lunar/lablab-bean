using FluentAssertions;
using LablabBean.DependencyInjection.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Tests.Unit;

public class HierarchicalServiceProviderTests
{
    private interface ITestService { }
    private class TestService : ITestService { }

    [Fact]
    public void CreateRootContainer_WithServices_ReturnsSingletonInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var container = services.BuildHierarchicalServiceProvider("Global");

        // Assert
        var service1 = container.GetService<ITestService>();
        var service2 = container.GetService<ITestService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void MultipleGetService_OnSingleton_ReturnsSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var container = services.BuildHierarchicalServiceProvider();

        // Act
        var service1 = container.GetService(typeof(ITestService));
        var service2 = container.GetService(typeof(ITestService));
        var service3 = container.GetService(typeof(ITestService));

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service3.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
        service2.Should().BeSameAs(service3);
    }

    [Fact]
    public void GetService_OnDisposedContainer_ThrowsContainerDisposedException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var container = services.BuildHierarchicalServiceProvider("TestContainer");
        container.Dispose();

        // Act
        Action act = () => container.GetService(typeof(ITestService));

        // Assert
        act.Should().Throw<ContainerDisposedException>()
            .WithMessage("*TestContainer*");
    }

    [Fact]
    public void GetRequiredService_ServiceNotFound_ThrowsWithClearMessage()
    {
        // Arrange
        var services = new ServiceCollection();
        var container = services.BuildHierarchicalServiceProvider("TestContainer");

        // Act
        Action act = () => ((ISupportRequiredService)container).GetRequiredService(typeof(ITestService));

        // Assert
        act.Should().Throw<ServiceResolutionException>()
            .WithMessage("*ITestService*")
            .WithMessage("*TestContainer*");
    }

    [Fact]
    public void BuildHierarchicalServiceProvider_ExtensionMethod_CreatesValidContainer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var container = services.BuildHierarchicalServiceProvider("MyContainer");

        // Assert
        container.Should().NotBeNull();
        container.Name.Should().Be("MyContainer");
        container.Depth.Should().Be(0);
        container.Parent.Should().BeNull();
        container.IsDisposed.Should().BeFalse();

        var service = container.GetService<ITestService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void RootContainer_DefaultName_IsRoot()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var container = services.BuildHierarchicalServiceProvider();

        // Assert
        container.Name.Should().Be("Root");
    }
}
