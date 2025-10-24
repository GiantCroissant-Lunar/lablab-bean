using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Benchmarks;

[MemoryDiagnoser]
public class ContainerLifecycleBenchmarks
{
    private IHierarchicalServiceProvider _root = default!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        _root = services.BuildHierarchicalServiceProvider("Root");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _root.Dispose();
    }

    [Benchmark]
    public void CreateDispose_Child()
    {
        var child = _root.CreateChildContainer(_ => { });
        child.Dispose();
    }
}
