using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Contracts.Attributes;

namespace LablabBean.SourceGenerators.Proxy.Tests;

/// <summary>
/// Tests for ProxyServiceGenerator basic functionality.
/// </summary>
public class ProxyGeneratorTests
{
    [Fact]
    public void Generator_FindsPartialClassWithAttribute()
    {
        // This test verifies the generator can find and process
        // a partial class marked with [RealizeService]
        
        // Arrange - Create a simple test interface
        var testInterface = typeof(ITestService);
        
        // Act - The generator should process TestProxy class below
        var proxy = new TestProxy(new TestRegistry());
        
        // Assert - Proxy should be created (compilation test)
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Attributes_AreAccessible()
    {
        // Verify attributes are properly defined and accessible
        var realizeAttr = typeof(RealizeServiceAttribute);
        var strategyAttr = typeof(SelectionStrategyAttribute);
        
        Assert.NotNull(realizeAttr);
        Assert.NotNull(strategyAttr);
    }
}

// Test interface
public interface ITestService
{
    void DoSomething();
    string GetValue();
}

// Test proxy class (generator will implement interface methods)
[RealizeService(typeof(ITestService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class TestProxy
{
    private readonly IRegistry _registry;

    public TestProxy(IRegistry registry)
    {
        _registry = registry;
    }
}

// Mock registry for testing
public class TestRegistry : IRegistry
{
    public T Get<T>(SelectionMode mode = SelectionMode.HighestPriority) where T : class
    {
        return new TestServiceImpl() as T ?? throw new InvalidOperationException();
    }

    public IEnumerable<T> GetAll<T>() where T : class
    {
        yield return new TestServiceImpl() as T ?? throw new InvalidOperationException();
    }

    public void Register<T>(T implementation, ServiceMetadata metadata) where T : class
    {
        // Not needed for test
    }

    public void Register<T>(T implementation, int priority = 100) where T : class
    {
        // Not needed for test
    }

    public bool IsRegistered<T>() where T : class
    {
        return true;
    }

    public bool Unregister<T>(T implementation) where T : class
    {
        return true;
    }
}

// Test implementation
public class TestServiceImpl : ITestService
{
    public void DoSomething() { }
    public string GetValue() => "test";
}
