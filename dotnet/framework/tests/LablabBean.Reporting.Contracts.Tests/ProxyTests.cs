using LablabBean.Plugins.Contracts;
using LablabBean.Reporting.Contracts.Proxies;

namespace LablabBean.Reporting.Contracts.Tests;

/// <summary>
/// Basic tests to verify proxy classes can be instantiated and require IRegistry.
/// </summary>
public class ProxyTests
{
    [Fact]
    public void ReportingServiceProxy_Constructor_RequiresRegistry()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ReportingServiceProxy(null!));
        Assert.Equal("registry", ex.ParamName);
    }

    [Fact]
    public void ReportProviderProxy_Constructor_RequiresRegistry()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ReportProviderProxy(null!));
        Assert.Equal("registry", ex.ParamName);
    }

    [Fact]
    public void ReportRendererProxy_Constructor_RequiresRegistry()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ReportRendererProxy(null!));
        Assert.Equal("registry", ex.ParamName);
    }

    [Fact]
    public void ReportingServiceProxy_CanBeInstantiated_WithRegistry()
    {
        // Arrange
        var registry = new TestRegistry();

        // Act
        var proxy = new ReportingServiceProxy(registry);

        // Assert
        Assert.NotNull(proxy);
    }

    [Fact]
    public void ReportProviderProxy_CanBeInstantiated_WithRegistry()
    {
        // Arrange
        var registry = new TestRegistry();

        // Act
        var proxy = new ReportProviderProxy(registry);

        // Assert
        Assert.NotNull(proxy);
    }

    [Fact]
    public void ReportRendererProxy_CanBeInstantiated_WithRegistry()
    {
        // Arrange
        var registry = new TestRegistry();

        // Act
        var proxy = new ReportRendererProxy(registry);

        // Assert
        Assert.NotNull(proxy);
    }

    /// <summary>
    /// Minimal test implementation of IRegistry for proxy instantiation tests.
    /// </summary>
    private class TestRegistry : IRegistry
    {
        public TService Get<TService>(string? name = null) where TService : class
            => throw new NotImplementedException();

        public TService Get<TService>(SelectionMode mode) where TService : class
            => throw new NotImplementedException();

        public IEnumerable<TService> GetAll<TService>() where TService : class
            => throw new NotImplementedException();

        public void Register<TService>(TService implementation, ServiceMetadata metadata) where TService : class
            => throw new NotImplementedException();

        public void Register<TService>(TService implementation, int priority = 0) where TService : class
            => throw new NotImplementedException();

        public bool Unregister<TService>(TService implementation) where TService : class
            => throw new NotImplementedException();

        public bool IsRegistered<TService>() where TService : class
            => throw new NotImplementedException();
    }
}
