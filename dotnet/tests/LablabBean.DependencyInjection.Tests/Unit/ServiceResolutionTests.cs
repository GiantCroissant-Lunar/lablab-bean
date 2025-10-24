using FluentAssertions;
using LablabBean.DependencyInjection.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Tests.Unit;

public class ServiceResolutionTests
{
    private interface IParentService { }
    private class ParentService : IParentService { }

    private interface IChildService { }
    private class ChildService : IChildService { }

    private interface ISharedService { }
    private class ParentSharedService : ISharedService { }
    private class ChildSharedService : ISharedService { }

    [Fact]
    public void ChildContainer_CanResolveServiceFromParent()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        parentServices.AddSingleton<IParentService, ParentService>();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");

        var child = parent.CreateChildContainer(services =>
        {
            services.AddSingleton<IChildService, ChildService>();
        }, "Child");

        // Act
        var parentService = child.GetService<IParentService>();
        var childService = child.GetService<IChildService>();

        // Assert
        parentService.Should().NotBeNull();
        childService.Should().NotBeNull();
    }

    [Fact]
    public void ChildContainer_PrioritizesLocalServiceOverParent()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        parentServices.AddSingleton<ISharedService, ParentSharedService>();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");

        var child = parent.CreateChildContainer(services =>
        {
            services.AddSingleton<ISharedService, ChildSharedService>();
        }, "Child");

        // Act
        var parentService = parent.GetService<ISharedService>();
        var childService = child.GetService<ISharedService>();

        // Assert
        parentService.Should().BeOfType<ParentSharedService>();
        childService.Should().BeOfType<ChildSharedService>();
        childService.Should().NotBeSameAs(parentService);
    }

    [Fact]
    public void ServiceNotFoundInChildOrParent_ReturnsNull()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");
        var child = parent.CreateChildContainer(_ => { }, "Child");

        // Act
        var service = child.GetService<IParentService>();

        // Assert
        service.Should().BeNull();
    }

    [Fact]
    public void TwoSiblingContainers_WithSameInterface_GetDifferentInstances()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");

        var child1 = parent.CreateChildContainer(services =>
        {
            services.AddSingleton<ISharedService, ChildSharedService>();
        }, "Child1");

        var child2 = parent.CreateChildContainer(services =>
        {
            services.AddSingleton<ISharedService, ChildSharedService>();
        }, "Child2");

        // Act
        var service1 = child1.GetService<ISharedService>();
        var service2 = child2.GetService<ISharedService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void CreateChildContainer_IncrementsDepthCorrectly()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Root");

        // Act
        var level1 = root.CreateChildContainer(_ => { }, "Level1");
        var level2 = level1.CreateChildContainer(_ => { }, "Level2");

        // Assert
        root.Depth.Should().Be(0);
        level1.Depth.Should().Be(1);
        level2.Depth.Should().Be(2);
    }

    [Fact]
    public void GetHierarchyPath_ReturnsCorrectFullPath()
    {
        // Arrange
        var rootServices = new ServiceCollection();
        var root = rootServices.BuildHierarchicalServiceProvider("Global");
        var dungeon = root.CreateChildContainer(_ => { }, "Dungeon");
        var floor = dungeon.CreateChildContainer(_ => { }, "Floor1");

        // Act
        var rootPath = root.GetHierarchyPath();
        var dungeonPath = dungeon.GetHierarchyPath();
        var floorPath = floor.GetHierarchyPath();

        // Assert
        rootPath.Should().Be("Global");
        dungeonPath.Should().Be("Global → Dungeon");
        floorPath.Should().Be("Global → Dungeon → Floor1");
    }

    [Fact]
    public void CannotCreateChild_FromDisposedContainer()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");
        parent.Dispose();

        // Act
        Action act = () => parent.CreateChildContainer(_ => { }, "Child");

        // Assert
        act.Should().Throw<ContainerDisposedException>()
            .WithMessage("*Parent*");
    }
}
