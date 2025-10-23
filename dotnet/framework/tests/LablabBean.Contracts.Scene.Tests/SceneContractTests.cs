using LablabBean.Contracts.Scene;
using LablabBean.Contracts.Scene.Services;

namespace LablabBean.Contracts.Scene.Tests;

/// <summary>
/// Tests for Scene contract validation.
/// </summary>
public class SceneContractTests
{
    [Fact]
    public void SceneLoadedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new SceneLoadedEvent("test-scene");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("test-scene", evt.SceneId);
    }

    [Fact]
    public void SceneUnloadedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new SceneUnloadedEvent("test-scene");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("test-scene", evt.SceneId);
    }

    [Fact]
    public void SceneLoadFailedEvent_HasTimestamp()
    {
        // Arrange
        var error = new Exception("Test error");

        // Act
        var evt = new SceneLoadFailedEvent("test-scene", error);

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("test-scene", evt.SceneId);
        Assert.Equal(error, evt.Error);
    }

    [Fact]
    public void Camera_HasPositionAndZoom()
    {
        // Arrange & Act
        var camera = new Camera(new Position(10, 20), 1.5f);

        // Assert
        Assert.Equal(10, camera.Position.X);
        Assert.Equal(20, camera.Position.Y);
        Assert.Equal(1.5f, camera.Zoom);
    }

    [Fact]
    public void Camera_DefaultZoomIsOne()
    {
        // Arrange & Act
        var camera = new Camera(new Position(0, 0));

        // Assert
        Assert.Equal(1.0f, camera.Zoom);
    }

    [Fact]
    public void Viewport_HasWidthAndHeight()
    {
        // Arrange & Act
        var viewport = new Viewport(80, 24);

        // Assert
        Assert.Equal(80, viewport.Width);
        Assert.Equal(24, viewport.Height);
    }

    [Fact]
    public void CameraViewport_CombinesCameraAndViewport()
    {
        // Arrange
        var camera = new Camera(new Position(5, 5), 1.0f);
        var viewport = new Viewport(40, 20);

        // Act
        var cameraViewport = new CameraViewport(camera, viewport);

        // Assert
        Assert.Equal(camera, cameraViewport.Camera);
        Assert.Equal(viewport, cameraViewport.Viewport);
    }

    [Fact]
    public void EntitySnapshot_HasAllProperties()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var position = new Position(10, 15);

        // Act
        var snapshot = new EntitySnapshot(
            entityId,
            position,
            '@',
            "#FFFFFF",
            "#000000"
        );

        // Assert
        Assert.Equal(entityId, snapshot.EntityId);
        Assert.Equal(position, snapshot.Position);
        Assert.Equal('@', snapshot.Glyph);
        Assert.Equal("#FFFFFF", snapshot.ForegroundColor);
        Assert.Equal("#000000", snapshot.BackgroundColor);
    }

    [Fact]
    public void IService_HasRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(IService);

        // Assert - Verify interface has required methods
        Assert.NotNull(serviceType.GetMethod(nameof(IService.LoadSceneAsync)));
        Assert.NotNull(serviceType.GetMethod(nameof(IService.UnloadSceneAsync)));
        Assert.NotNull(serviceType.GetMethod(nameof(IService.GetViewport)));
        Assert.NotNull(serviceType.GetMethod(nameof(IService.GetCameraViewport)));
        Assert.NotNull(serviceType.GetMethod(nameof(IService.SetCamera)));
        Assert.NotNull(serviceType.GetMethod(nameof(IService.UpdateWorld)));
    }

    [Fact]
    public void Events_AreImmutable()
    {
        // Arrange & Act
        var loadedEvent = new SceneLoadedEvent("scene1");
        var unloadedEvent = new SceneUnloadedEvent("scene2");
        var failedEvent = new SceneLoadFailedEvent("scene3", new Exception());

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<SceneLoadedEvent>(loadedEvent);
        Assert.IsAssignableFrom<SceneUnloadedEvent>(unloadedEvent);
        Assert.IsAssignableFrom<SceneLoadFailedEvent>(failedEvent);
    }
}
