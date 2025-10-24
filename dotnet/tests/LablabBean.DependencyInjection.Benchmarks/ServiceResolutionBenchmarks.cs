using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Benchmarks;

[MemoryDiagnoser]
public class ServiceResolutionBenchmarks
{
    public interface ISvc {}
    public class Svc : ISvc {}

    private IHierarchicalServiceProvider _local = default!;            // Single container with local service
    private IHierarchicalServiceProvider _parent = default!;          // Parent with service
    private IHierarchicalServiceProvider _childFromParent = default!; // Child resolving from parent
    private IHierarchicalServiceProvider _root = default!;            // Root with service
    private IHierarchicalServiceProvider _mid = default!;             // Middle
    private IHierarchicalServiceProvider _leaf = default!;            // Leaf resolving from grandparent

    [GlobalSetup]
    public void Setup()
    {
        // Local container with local registration
        var localServices = new ServiceCollection();
        localServices.AddSingleton<ISvc, Svc>();
        _local = localServices.BuildHierarchicalServiceProvider("Local");

        // Parent-child: service only in parent
        var parentServices = new ServiceCollection();
        parentServices.AddSingleton<ISvc, Svc>();
        _parent = parentServices.BuildHierarchicalServiceProvider("Parent");
        _childFromParent = _parent.CreateChildContainer(_ => { }, "Child");

        // Grandparent chain: service only in root
        var rootServices = new ServiceCollection();
        rootServices.AddSingleton<ISvc, Svc>();
        _root = rootServices.BuildHierarchicalServiceProvider("Root");
        _mid = _root.CreateChildContainer(_ => { }, "Mid");
        _leaf = _mid.CreateChildContainer(_ => { }, "Leaf");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _local.Dispose();
        _parent.Dispose();
        _root.Dispose();
    }

    [Benchmark]
    public object? Resolve_Local() => _local.GetService<ISvc>();

    [Benchmark]
    public object? Resolve_Parent() => _childFromParent.GetService<ISvc>();

    [Benchmark]
    public object? Resolve_Grandparent() => _leaf.GetService<ISvc>();
}
