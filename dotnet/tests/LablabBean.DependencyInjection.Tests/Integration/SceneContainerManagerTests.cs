using FluentAssertions;

namespace LablabBean.DependencyInjection.Tests.Integration;

public class SceneContainerManagerTests
{
    // Test services
    private interface ISaveSystem { }
    private class SaveSystem : ISaveSystem { }

    private interface IAudioManager { }
    private class AudioManager : IAudioManager { }

    private interface ICombatSystem { }
    private class CombatSystem : ICombatSystem { }

    private interface ILootSystem { }
    private class LootSystem : ILootSystem { }

    [Fact]
    public void InitializeGlobalContainer_CreatesGlobalContainer()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var services = new ServiceCollection();
        services.AddSingleton<ISaveSystem, SaveSystem>();
        services.AddSingleton<IAudioManager, AudioManager>();

        // Act
        manager.InitializeGlobalContainer(services);

        // Assert
        manager.IsInitialized.Should().BeTrue();
        manager.GlobalContainer.Should().NotBeNull();
        manager.GlobalContainer!.Name.Should().Be("Global");
        manager.GlobalContainer.Parent.Should().BeNull();

        // Verify services are accessible
        var saveSystem = manager.GlobalContainer.GetService<ISaveSystem>();
        saveSystem.Should().NotBeNull();
        saveSystem.Should().BeOfType<SaveSystem>();

        var audioManager = manager.GlobalContainer.GetService<IAudioManager>();
        audioManager.Should().NotBeNull();
        audioManager.Should().BeOfType<AudioManager>();
    }

    [Fact]
    public void InitializeGlobalContainer_WhenAlreadyInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var services = new ServiceCollection();
        manager.InitializeGlobalContainer(services);

        // Act
        var act = () => manager.InitializeGlobalContainer(services);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Global container has already been initialized.");
    }

    [Fact]
    public void CreateSceneContainer_RegistersSceneContainer()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        manager.InitializeGlobalContainer(globalServices);

        var sceneServices = new ServiceCollection();
        sceneServices.AddSingleton<ICombatSystem, CombatSystem>();

        // Act
        var sceneContainer = manager.CreateSceneContainer("Dungeon", sceneServices);

        // Assert
        sceneContainer.Should().NotBeNull();
        sceneContainer.Name.Should().Be("Dungeon");
        sceneContainer.Parent.Should().Be(manager.GlobalContainer);

        // Verify scene is in registry
        var sceneNames = manager.GetSceneNames().ToList();
        sceneNames.Should().Contain("Dungeon");

        // Verify scene services accessible
        var combatSystem = sceneContainer.GetService<ICombatSystem>();
        combatSystem.Should().NotBeNull();
        combatSystem.Should().BeOfType<CombatSystem>();

        // Verify global services accessible from scene
        var saveSystem = sceneContainer.GetService<ISaveSystem>();
        saveSystem.Should().NotBeNull();
        saveSystem.Should().BeOfType<SaveSystem>();
    }

    [Fact]
    public void CreateSceneContainer_WithParentName_CreatesHierarchy()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        manager.InitializeGlobalContainer(globalServices);

        var dungeonServices = new ServiceCollection();
        dungeonServices.AddSingleton<ICombatSystem, CombatSystem>();
        var dungeonContainer = manager.CreateSceneContainer("Dungeon", dungeonServices);

        var floorServices = new ServiceCollection();
        floorServices.AddSingleton<ILootSystem, LootSystem>();

        // Act
        var floorContainer = manager.CreateSceneContainer("Floor1", floorServices, "Dungeon");

        // Assert
        floorContainer.Should().NotBeNull();
        floorContainer.Name.Should().Be("Floor1");
        floorContainer.Parent.Should().Be(dungeonContainer);
        floorContainer.Depth.Should().Be(2); // Global (0) -> Dungeon (1) -> Floor1 (2)

        // Verify floor services accessible
        var lootSystem = floorContainer.GetService<ILootSystem>();
        lootSystem.Should().NotBeNull();

        // Verify dungeon services accessible from floor
        var combatSystem = floorContainer.GetService<ICombatSystem>();
        combatSystem.Should().NotBeNull();

        // Verify global services accessible from floor
        var saveSystem = floorContainer.GetService<ISaveSystem>();
        saveSystem.Should().NotBeNull();
    }

    [Fact]
    public void CreateSceneContainer_BeforeGlobalInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var sceneServices = new ServiceCollection();

        // Act
        var act = () => manager.CreateSceneContainer("Scene1", sceneServices);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Global container must be initialized*");
    }

    [Fact]
    public void CreateSceneContainer_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        var sceneServices1 = new ServiceCollection();
        manager.CreateSceneContainer("Dungeon", sceneServices1);

        var sceneServices2 = new ServiceCollection();

        // Act
        var act = () => manager.CreateSceneContainer("Dungeon", sceneServices2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Scene 'Dungeon' already exists*");
    }

    [Fact]
    public void CreateSceneContainer_WithInvalidParentName_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        var sceneServices = new ServiceCollection();

        // Act
        var act = () => manager.CreateSceneContainer("Floor1", sceneServices, "NonExistentDungeon");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Parent scene 'NonExistentDungeon' not found*");
    }

    [Fact]
    public void GetSceneContainer_RetrievesRegisteredContainer()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        var sceneServices = new ServiceCollection();
        var createdContainer = manager.CreateSceneContainer("Dungeon", sceneServices);

        // Act
        var retrievedContainer = manager.GetSceneContainer("Dungeon");

        // Assert
        retrievedContainer.Should().NotBeNull();
        retrievedContainer.Should().Be(createdContainer);
    }

    [Fact]
    public void GetSceneContainer_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        // Act
        var container = manager.GetSceneContainer("NonExistent");

        // Assert
        container.Should().BeNull();
    }

    [Fact]
    public void UnloadScene_DisposesAndRemovesContainer()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        var sceneServices = new ServiceCollection();
        var sceneContainer = manager.CreateSceneContainer("Dungeon", sceneServices);

        sceneContainer.IsDisposed.Should().BeFalse();

        // Act
        manager.UnloadScene("Dungeon");

        // Assert
        sceneContainer.IsDisposed.Should().BeTrue();

        var retrievedContainer = manager.GetSceneContainer("Dungeon");
        retrievedContainer.Should().BeNull();

        var sceneNames = manager.GetSceneNames().ToList();
        sceneNames.Should().NotContain("Dungeon");
    }

    [Fact]
    public void UnloadScene_WhenSceneNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        // Act
        var act = () => manager.UnloadScene("NonExistent");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Scene 'NonExistent' not found*");
    }

    [Fact]
    public void GetSceneNames_ReturnsAllSceneNames()
    {
        // Arrange
        var manager = new SceneContainerManager();
        var globalServices = new ServiceCollection();
        manager.InitializeGlobalContainer(globalServices);

        var dungeonServices = new ServiceCollection();
        manager.CreateSceneContainer("Dungeon", dungeonServices);

        var townServices = new ServiceCollection();
        manager.CreateSceneContainer("Town", townServices);

        // Act
        var sceneNames = manager.GetSceneNames().ToList();

        // Assert
        sceneNames.Should().HaveCount(2);
        sceneNames.Should().Contain("Dungeon");
        sceneNames.Should().Contain("Town");
    }

    [Fact]
    public void CompleteGameScenario_GlobalToMultipleScenes_WorksCorrectly()
    {
        // Arrange: Create manager
        var manager = new SceneContainerManager();

        // Step 1: Initialize global container with global services
        var globalServices = new ServiceCollection();
        globalServices.AddSingleton<ISaveSystem, SaveSystem>();
        globalServices.AddSingleton<IAudioManager, AudioManager>();
        manager.InitializeGlobalContainer(globalServices);

        manager.IsInitialized.Should().BeTrue();

        // Step 2: Create Dungeon scene
        var dungeonServices = new ServiceCollection();
        dungeonServices.AddSingleton<ICombatSystem, CombatSystem>();
        var dungeonContainer = manager.CreateSceneContainer("Dungeon", dungeonServices);

        dungeonContainer.Name.Should().Be("Dungeon");
        dungeonContainer.Parent.Should().Be(manager.GlobalContainer);

        // Step 3: Create Floor1 as child of Dungeon
        var floor1Services = new ServiceCollection();
        floor1Services.AddSingleton<ILootSystem, LootSystem>();
        var floor1Container = manager.CreateSceneContainer("Floor1", floor1Services, "Dungeon");

        floor1Container.Name.Should().Be("Floor1");
        floor1Container.Parent.Should().Be(dungeonContainer);
        floor1Container.Depth.Should().Be(2);

        // Step 4: Verify Floor1 can access all services (local, parent, global)
        var lootSystem = floor1Container.GetService<ILootSystem>();
        lootSystem.Should().NotBeNull("Floor1 should have its own LootSystem");

        var combatSystem = floor1Container.GetService<ICombatSystem>();
        combatSystem.Should().NotBeNull("Floor1 should access Dungeon's CombatSystem");

        var saveSystem = floor1Container.GetService<ISaveSystem>();
        saveSystem.Should().NotBeNull("Floor1 should access Global's SaveSystem");

        var audioManager = floor1Container.GetService<IAudioManager>();
        audioManager.Should().NotBeNull("Floor1 should access Global's AudioManager");

        // Step 5: Create Floor2 as sibling to Floor1 (also child of Dungeon)
        var floor2Services = new ServiceCollection();
        floor2Services.AddSingleton<ILootSystem, LootSystem>(); // Different instance
        var floor2Container = manager.CreateSceneContainer("Floor2", floor2Services, "Dungeon");

        floor2Container.Parent.Should().Be(dungeonContainer);

        // Verify Floor1 and Floor2 have different LootSystem instances
        var floor1Loot = floor1Container.GetService<ILootSystem>();
        var floor2Loot = floor2Container.GetService<ILootSystem>();
        floor1Loot.Should().NotBeSameAs(floor2Loot, "Sibling scenes should have isolated services");

        // But they share Combat and Global services
        var floor1Combat = floor1Container.GetService<ICombatSystem>();
        var floor2Combat = floor2Container.GetService<ICombatSystem>();
        floor1Combat.Should().BeSameAs(floor2Combat, "Both floors should share Dungeon's CombatSystem");

        // Step 6: Verify scene registry
        var sceneNames = manager.GetSceneNames().ToList();
        sceneNames.Should().Contain("Dungeon");
        sceneNames.Should().Contain("Floor1");
        sceneNames.Should().Contain("Floor2");
        sceneNames.Should().HaveCount(3);

        // Step 7: Unload Floor1 (shouldn't affect Floor2 or Dungeon)
        manager.UnloadScene("Floor1");
        floor1Container.IsDisposed.Should().BeTrue();
        floor2Container.IsDisposed.Should().BeFalse();
        dungeonContainer.IsDisposed.Should().BeFalse();

        manager.GetSceneContainer("Floor1").Should().BeNull();
        manager.GetSceneContainer("Floor2").Should().NotBeNull();

        // Step 8: Unload Dungeon (shouldn't affect Floor2 since it's a sibling)
        manager.UnloadScene("Dungeon");
        dungeonContainer.IsDisposed.Should().BeTrue();

        // Note: Floor2 was a child of Dungeon, but since we removed it from registry
        // and disposed Dungeon, Floor2's parent is now disposed. This is expected behavior.
        // In a real game, you'd unload all children before unloading the parent.

        // Step 9: Verify scene registry updated
        var finalSceneNames = manager.GetSceneNames().ToList();
        finalSceneNames.Should().Contain("Floor2");
        finalSceneNames.Should().HaveCount(1);

        // Step 10: Unload remaining scenes
        manager.UnloadScene("Floor2");
        manager.GetSceneNames().Should().BeEmpty();
    }
}
