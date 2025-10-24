using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace LablabBean.DependencyInjection.Tests.Integration;

/// <summary>
/// Integration tests for multi-level hierarchy scenarios (Global → Dungeon → Floor).
/// </summary>
public class MultiLevelHierarchyTests
{
    private interface ISaveSystem { }
    private class SaveSystem : ISaveSystem { }

    private interface IAudioManager { }
    private class AudioManager : IAudioManager { }

    private interface ICombatSystem { }
    private class CombatSystem : ICombatSystem
    {
        private readonly IHierarchicalServiceProvider _serviceProvider;
        public ISaveSystem SaveSystem => _serviceProvider.GetService<ISaveSystem>()!;
        public CombatSystem(IHierarchicalServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
    }

    private interface IFloorGenerator { }
    private class FloorGenerator : IFloorGenerator
    {
        private readonly IHierarchicalServiceProvider _serviceProvider;
        public ISaveSystem SaveSystem => _serviceProvider.GetService<ISaveSystem>()!;
        public ICombatSystem CombatSystem => _serviceProvider.GetService<ICombatSystem>()!;

        public FloorGenerator(IHierarchicalServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }

    [Fact]
    public void GlobalDungeonFloorHierarchy_ServiceResolution_WorksCorrectly()
    {
        // Arrange - Global container with shared services
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        globalServices.AddSingleton<IAudioManager, AudioManager>();
        var global = globalServices.BuildHierarchicalServiceProvider("Global");

        // Act - Create dungeon container with combat system
        var dungeon = global.CreateChildContainer(services =>
        {
            services.AddSingleton<ICombatSystem, CombatSystem>();
        }, "Dungeon");

        // Create floor container with floor generator
        var floor = dungeon.CreateChildContainer(services =>
        {
            services.AddSingleton<IFloorGenerator, FloorGenerator>();
        }, "Floor1");

        // Assert - Floor can access all services in hierarchy
        var saveSystem = floor.GetService<ISaveSystem>();
        var audioManager = floor.GetService<IAudioManager>();
        var combatSystem = floor.GetService<ICombatSystem>();
        var floorGenerator = floor.GetService<IFloorGenerator>();

        saveSystem.Should().NotBeNull();
        audioManager.Should().NotBeNull();
        combatSystem.Should().NotBeNull();
        floorGenerator.Should().NotBeNull();

        // Verify same instances across hierarchy
        global.GetService<ISaveSystem>().Should().BeSameAs(saveSystem);
        dungeon.GetService<ISaveSystem>().Should().BeSameAs(saveSystem);

        // Verify hierarchy structure
        floor.GetHierarchyPath().Should().Be("Global → Dungeon → Floor1");
        floor.Depth.Should().Be(2);
        dungeon.Depth.Should().Be(1);
        global.Depth.Should().Be(0);
    }

    [Fact]
    public void CascadingDisposal_InMultiLevelHierarchy_WorksCorrectly()
    {
        // Arrange - Create 3-level hierarchy
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        var global = globalServices.BuildHierarchicalServiceProvider("Global");

        var dungeon = global.CreateChildContainer(services =>
        {
            services.AddSingleton<ICombatSystem, CombatSystem>();
        }, "Dungeon");

        var floor1 = dungeon.CreateChildContainer(_ => { }, "Floor1");
        var floor2 = dungeon.CreateChildContainer(_ => { }, "Floor2");

        // Act - Dispose dungeon (should cascade to floors)
        dungeon.Dispose();

        // Assert
        dungeon.IsDisposed.Should().BeTrue();
        floor1.IsDisposed.Should().BeTrue();
        floor2.IsDisposed.Should().BeTrue();
        global.IsDisposed.Should().BeFalse();

        // Verify global still works
        var saveSystem = global.GetService<ISaveSystem>();
        saveSystem.Should().NotBeNull();
    }

    [Fact]
    public void MultipleDungeons_IsolatedFromEachOther()
    {
        // Arrange
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        var global = globalServices.BuildHierarchicalServiceProvider("Global");

        var dungeon1 = global.CreateChildContainer(services =>
        {
            services.AddSingleton<ICombatSystem, CombatSystem>();
        }, "Dungeon1");

        var dungeon2 = global.CreateChildContainer(services =>
        {
            services.AddSingleton<ICombatSystem, CombatSystem>();
        }, "Dungeon2");

        // Act
        var combat1 = dungeon1.GetService<ICombatSystem>();
        var combat2 = dungeon2.GetService<ICombatSystem>();

        // Assert - Each dungeon has its own combat system instance
        combat1.Should().NotBeNull();
        combat2.Should().NotBeNull();
        combat1.Should().NotBeSameAs(combat2);

        // But they share the same save system from global
        var save1 = dungeon1.GetService<ISaveSystem>();
        var save2 = dungeon2.GetService<ISaveSystem>();
        save1.Should().BeSameAs(save2);
    }
}
