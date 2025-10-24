using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Tests.Unit;

public class DisposalTests
{
    private interface ITestService { }
    private class TestService : ITestService { }

    [Fact]
    public void DisposingParent_DisposesAllChildrenRecursively()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        var level1 = root.CreateChildContainer(_ => { }, "Level1");
        var level2a = level1.CreateChildContainer(_ => { }, "Level2A");
        var level2b = level1.CreateChildContainer(_ => { }, "Level2B");
        var level3 = level2a.CreateChildContainer(_ => { }, "Level3");

        // Act
        root.Dispose();

        // Assert
        root.IsDisposed.Should().BeTrue();
        level1.IsDisposed.Should().BeTrue();
        level2a.IsDisposed.Should().BeTrue();
        level2b.IsDisposed.Should().BeTrue();
        level3.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void DisposingChild_DoesNotAffectParentOrSiblings()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        var child1 = root.CreateChildContainer(_ => { }, "Child1");
        var child2 = root.CreateChildContainer(_ => { }, "Child2");
        var grandchild = child1.CreateChildContainer(_ => { }, "Grandchild");

        // Act
        child1.Dispose();

        // Assert
        child1.IsDisposed.Should().BeTrue();
        grandchild.IsDisposed.Should().BeTrue();
        root.IsDisposed.Should().BeFalse();
        child2.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Dispose_IsIdempotent_MultipleCallsSafe()
    {
        // Arrange
        var services = new ServiceCollection();
        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        container.Dispose();
        container.Dispose();
        container.Dispose();

        // Assert
        container.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void DeepHierarchy_ResolvesServicesCorrectly()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        rootServices.AddSingleton<ITestService, TestService>();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        var level1 = root.CreateChildContainer(_ => { }, "L1");
        var level2 = level1.CreateChildContainer(_ => { }, "L2");
        var level3 = level2.CreateChildContainer(_ => { }, "L3");
        var level4 = level3.CreateChildContainer(_ => { }, "L4");

        // Act
        var service = level4.GetService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void DisposedContainer_ClearsChildrenList()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        root.CreateChildContainer(_ => { }, "Child1");
        root.CreateChildContainer(_ => { }, "Child2");

        root.Children.Should().HaveCount(2);

        // Act
        root.Dispose();

        // Assert
        root.Children.Should().BeEmpty();
    }

    [Fact]
    public void CreateChildContainer_ExceedingMaxDepth_ThrowsException()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        IHierarchicalServiceProvider current = root;
        for (int i = 0; i < 10; i++)
        {
            current = current.CreateChildContainer(_ => { }, $"Level{i + 1}");
        }

        // Act
        Action act = () => current.CreateChildContainer(_ => { }, "TooDeep");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*maximum depth*");
    }
}
