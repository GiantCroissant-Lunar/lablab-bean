using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Benchmarks;

[MemoryDiagnoser]
public class MultiCycleBenchmarks
{
    private IHierarchicalServiceProvider _root = default!;
    private sealed class Transient { public Guid Id { get; } = Guid.NewGuid(); }

    [Params(1000)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        // Add a few services to make creation non-trivial
        services.AddSingleton<object>(_ => new object());
        services.AddTransient<Transient>(_ => new Transient());
        _root = services.BuildHierarchicalServiceProvider("Root");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _root.Dispose();
    }

    [Benchmark]
    public void CreateDisposeChild_Loop()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var child = _root.CreateChildContainer(_ => { }, name: null);
            child.Dispose();
        }
    }
}
