using LablabBean.DependencyInjection.Diagnostics;

namespace LablabBean.DependencyInjection.Tests.Unit;

// Ensure diagnostics tests do not run in parallel with others using the same collection
[Collection("Diagnostics")]
public class DiagnosticsEventsTests
{
    [Fact]
    public void ContainerCreated_Fires_ForRootAndChild()
    {
        var created = new List<string>();
        EventHandler<ContainerEventArgs> handler = (_, e) => created.Add($"{e.ContainerId}:{e.Name}:{e.Depth}:{e.ParentName}:{e.ParentId}");

        try
        {
            HierarchicalContainerDiagnostics.ContainerCreated += handler;

            var rootServices = new ServiceCollection();
            var root = rootServices.BuildHierarchicalServiceProvider("Global");

            var child = root.CreateChildContainer(_ => { }, "Dungeon");

            // Validate names, depths, and that IDs are present
            created.Any(x => x.Contains(":Global:0:")) .Should().BeTrue();
            created.Any(x => x.Contains(":Dungeon:1:Global:")) .Should().BeTrue();

            // basic sanity on ordering (root created before child)
            int idxRoot = created.FindIndex(s => s.Contains(":Global:0:"));
            int idxChild = created.FindIndex(s => s.Contains(":Dungeon:1:Global:"));
            idxRoot.Should().BeGreaterThan(-1);
            idxChild.Should().BeGreaterThan(-1);
            idxRoot.Should().BeLessThan(idxChild);
        }
        finally
        {
            HierarchicalContainerDiagnostics.ContainerCreated -= handler;
        }
    }

    [Fact]
    public void ContainerDisposed_Fires_ChildBeforeParent()
    {
        var disposed = new List<string>();
        EventHandler<ContainerEventArgs> handler = (_, e) => disposed.Add($"{e.Name}:{e.Depth}");

        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Global");
        var child = root.CreateChildContainer(_ => { }, "Dungeon");

        try
        {
            HierarchicalContainerDiagnostics.ContainerDisposed += handler;

            root.Dispose();

            // Filter only our pair to avoid noise from other tests
            var filtered = disposed.Where(s => s == "Dungeon:1" || s == "Global:0").ToList();
            filtered.Should().HaveCount(2);
            // Child disposed before parent due to cascading order
            filtered[0].Should().Be("Dungeon:1");
            filtered[1].Should().Be("Global:0");
        }
        finally
        {
            HierarchicalContainerDiagnostics.ContainerDisposed -= handler;
        }
    }
}
