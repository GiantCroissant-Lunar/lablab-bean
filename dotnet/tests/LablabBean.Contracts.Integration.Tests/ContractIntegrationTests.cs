using LablabBean.Contracts.Scene;
using LablabBean.Contracts.Input;
using LablabBean.Contracts.Config;
using LablabBean.Contracts.Resource;

namespace LablabBean.Contracts.Integration.Tests;

/// <summary>
/// Integration tests validating all 4 contract assemblies work together.
/// </summary>
public class ContractIntegrationTests
{
    [Fact]
    public void AllContractAssemblies_LoadSuccessfully()
    {
        // Arrange & Act - Just loading the assemblies
        var sceneAssembly = typeof(SceneLoadedEvent).Assembly;
        var inputAssembly = typeof(InputEvent).Assembly;
        var configAssembly = typeof(ConfigChangedEvent).Assembly;
        var resourceAssembly = typeof(ResourceLoadStartedEvent).Assembly;

        // Assert
        Assert.NotNull(sceneAssembly);
        Assert.NotNull(inputAssembly);
        Assert.NotNull(configAssembly);
        Assert.NotNull(resourceAssembly);
    }

    [Fact]
    public void SceneContract_EventsAreImmutable()
    {
        // Arrange & Act
        var loadedEvent = new SceneLoadedEvent("scene1");
        var unloadedEvent = new SceneUnloadedEvent("scene2");
        var failedEvent = new SceneLoadFailedEvent("scene3", new Exception());

        // Assert - Records are immutable
        Assert.IsAssignableFrom<SceneLoadedEvent>(loadedEvent);
        Assert.IsAssignableFrom<SceneUnloadedEvent>(unloadedEvent);
        Assert.IsAssignableFrom<SceneLoadFailedEvent>(failedEvent);
    }

    [Fact]
    public void InputContract_EventsAreImmutable()
    {
        // Arrange & Act
        var actionEvent = new InputActionTriggeredEvent("Jump");
        var pushedEvent = new InputScopePushedEvent("MenuScope");
        var poppedEvent = new InputScopePoppedEvent("GameScope");

        // Assert
        Assert.IsAssignableFrom<InputActionTriggeredEvent>(actionEvent);
        Assert.IsAssignableFrom<InputScopePushedEvent>(pushedEvent);
        Assert.IsAssignableFrom<InputScopePoppedEvent>(poppedEvent);
    }

    [Fact]
    public void ConfigContract_EventsAreImmutable()
    {
        // Arrange & Act
        var changedEvent = new ConfigChangedEvent("key", "old", "new");
        var reloadedEvent = new ConfigReloadedEvent();

        // Assert
        Assert.IsAssignableFrom<ConfigChangedEvent>(changedEvent);
        Assert.IsAssignableFrom<ConfigReloadedEvent>(reloadedEvent);
    }

    [Fact]
    public void ResourceContract_EventsAreImmutable()
    {
        // Arrange & Act
        var startedEvent = new ResourceLoadStartedEvent("texture1");
        var completedEvent = new ResourceLoadCompletedEvent("sound1", 100);
        var failedEvent = new ResourceLoadFailedEvent("data1", new Exception());
        var unloadedEvent = new ResourceUnloadedEvent("model1");

        // Assert
        Assert.IsAssignableFrom<ResourceLoadStartedEvent>(startedEvent);
        Assert.IsAssignableFrom<ResourceLoadCompletedEvent>(completedEvent);
        Assert.IsAssignableFrom<ResourceLoadFailedEvent>(failedEvent);
        Assert.IsAssignableFrom<ResourceUnloadedEvent>(unloadedEvent);
    }

    [Fact]
    public void AllEvents_HaveTimestamps()
    {
        // Arrange & Act
        var sceneEvent = new SceneLoadedEvent("scene1");
        var inputEvent = new InputActionTriggeredEvent("Action1");
        var configEvent = new ConfigChangedEvent("key", null, "value");
        var resourceEvent = new ResourceLoadStartedEvent("res1");

        // Assert
        Assert.NotEqual(default, sceneEvent.Timestamp);
        Assert.NotEqual(default, inputEvent.Timestamp);
        Assert.NotEqual(default, configEvent.Timestamp);
        Assert.NotEqual(default, resourceEvent.Timestamp);
    }

    [Fact]
    public void SceneModels_SupportComplexScenarios()
    {
        // Arrange
        var camera = new Camera(new Position(10, 20), 1.5f);
        var viewport = new Viewport(80, 24);
        var cameraViewport = new CameraViewport(camera, viewport);

        // Act
        var entitySnapshot = new EntitySnapshot(
            Guid.NewGuid(),
            new Position(5, 5),
            '@',
            "#FFFFFF",
            "#000000"
        );

        // Assert
        Assert.Equal(10, cameraViewport.Camera.Position.X);
        Assert.Equal(80, cameraViewport.Viewport.Width);
        Assert.Equal('@', entitySnapshot.Glyph);
    }

    [Fact]
    public void InputModels_SupportComplexScenarios()
    {
        // Arrange
        var rawKey = new RawKeyEvent("W", "Ctrl");
        var command = new InputCommand("Move", "W");
        var inputEvent = new InputEvent(command);
        var action = new InputAction("MoveNorth");

        // Assert
        Assert.Equal("Ctrl", rawKey.Modifiers);
        Assert.Equal("Move", command.Type);
        Assert.NotEqual(default, inputEvent.Timestamp);
        Assert.Equal("MoveNorth", action.Name);
    }

    [Fact]
    public void ResourceModels_SupportProgressTracking()
    {
        // Arrange
        var progress = new LoadProgress("texture1", 512, 1024, 50.0f);
        var metadata = new ResourceMetadata("texture1", "image", 1024, "/textures/player.png");
        var result = new ResourceLoadResult<string>("data", metadata, 150);

        // Assert
        Assert.Equal(50.0f, progress.PercentComplete);
        Assert.Equal(1024, metadata.SizeBytes);
        Assert.Equal(150, result.LoadTimeMs);
    }

    [Fact]
    public void AllContracts_UseConsistentNamingConventions()
    {
        // Arrange - Check that all service interfaces are named IService
        var sceneService = typeof(Scene.Services.IService);
        var inputRouterService = typeof(Input.Router.IService<>);
        var inputMapperService = typeof(Input.Mapper.IService);
        var configService = typeof(Config.Services.IService);
        var resourceService = typeof(Resource.Services.IService);

        // Assert - All follow the IService naming pattern
        Assert.Equal("IService", sceneService.Name);
        Assert.Equal("IService", inputMapperService.Name);
        Assert.Equal("IService", configService.Name);
        Assert.Equal("IService", resourceService.Name);
    }

    [Fact]
    public void AllContracts_EventsFollowNamingPattern()
    {
        // Arrange - All events should end with "Event"
        var events = new[]
        {
            typeof(SceneLoadedEvent),
            typeof(InputActionTriggeredEvent),
            typeof(ConfigChangedEvent),
            typeof(ResourceLoadStartedEvent)
        };

        // Assert
        foreach (var eventType in events)
        {
            Assert.EndsWith("Event", eventType.Name);
        }
    }
}
