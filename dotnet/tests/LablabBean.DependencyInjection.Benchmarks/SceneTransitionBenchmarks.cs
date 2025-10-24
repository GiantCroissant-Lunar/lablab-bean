using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Benchmarks;

[MemoryDiagnoser]
public class SceneTransitionBenchmarks
{
    private SceneContainerManager _manager = default!;

    [GlobalSetup]
    public void Setup()
    {
        _manager = new SceneContainerManager();
        var global = new ServiceCollection();
        global.AddSingleton<object>(_ => new object());
        _manager.InitializeGlobalContainer(global);
    }

    [Benchmark]
    public void CreateAndUnloadScene()
    {
        var sceneServices = new ServiceCollection();
        sceneServices.AddSingleton<object>(_ => new object());
        var scene = _manager.CreateSceneContainer("BenchScene", sceneServices);
        _manager.UnloadScene("BenchScene");
    }
}
