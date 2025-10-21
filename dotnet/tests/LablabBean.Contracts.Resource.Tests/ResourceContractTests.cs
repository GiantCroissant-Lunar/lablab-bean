using LablabBean.Contracts.Resource;
using LablabBean.Contracts.Resource.Services;

namespace LablabBean.Contracts.Resource.Tests;

/// <summary>
/// Tests for Resource contract validation.
/// </summary>
public class ResourceContractTests
{
    [Fact]
    public void LoadProgress_HasAllProperties()
    {
        // Arrange & Act
        var progress = new LoadProgress("texture1", 1024, 2048, 50.0f);

        // Assert
        Assert.Equal("texture1", progress.ResourceId);
        Assert.Equal(1024, progress.BytesLoaded);
        Assert.Equal(2048, progress.TotalBytes);
        Assert.Equal(50.0f, progress.PercentComplete);
    }

    [Fact]
    public void LoadProgress_SupportsUnknownTotalBytes()
    {
        // Arrange & Act
        var progress = new LoadProgress("data1", 512, null, 0.0f);

        // Assert
        Assert.Null(progress.TotalBytes);
        Assert.Equal(512, progress.BytesLoaded);
    }

    [Fact]
    public void ResourceMetadata_HasAllProperties()
    {
        // Arrange & Act
        var metadata = new ResourceMetadata("sound1", "audio", 4096, "/sounds/effect.wav");

        // Assert
        Assert.Equal("sound1", metadata.ResourceId);
        Assert.Equal("audio", metadata.ResourceType);
        Assert.Equal(4096, metadata.SizeBytes);
        Assert.Equal("/sounds/effect.wav", metadata.Path);
    }

    [Fact]
    public void ResourceLoadResult_HasAllProperties()
    {
        // Arrange
        var resource = "test resource";
        var metadata = new ResourceMetadata("res1", "text", 100, "/data/test.txt");

        // Act
        var result = new ResourceLoadResult<string>(resource, metadata, 150);

        // Assert
        Assert.Equal(resource, result.Resource);
        Assert.Equal(metadata, result.Metadata);
        Assert.Equal(150, result.LoadTimeMs);
    }

    [Fact]
    public void ResourceLoadStartedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new ResourceLoadStartedEvent("texture1");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("texture1", evt.ResourceId);
    }

    [Fact]
    public void ResourceLoadCompletedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new ResourceLoadCompletedEvent("sound1", 250);

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("sound1", evt.ResourceId);
        Assert.Equal(250, evt.LoadTimeMs);
    }

    [Fact]
    public void ResourceLoadFailedEvent_HasTimestamp()
    {
        // Arrange
        var error = new Exception("Load failed");

        // Act
        var evt = new ResourceLoadFailedEvent("data1", error);

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("data1", evt.ResourceId);
        Assert.Equal(error, evt.Error);
    }

    [Fact]
    public void ResourceUnloadedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new ResourceUnloadedEvent("texture1");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("texture1", evt.ResourceId);
    }

    [Fact]
    public void IService_HasRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(IService);

        // Assert - Verify interface has required methods
        var methods = serviceType.GetMethods();
        Assert.Contains(methods, m => m.Name == nameof(IService.LoadAsync));
        Assert.Contains(methods, m => m.Name == nameof(IService.PreloadAsync));
        Assert.Contains(methods, m => m.Name == nameof(IService.Unload));
        Assert.Contains(methods, m => m.Name == nameof(IService.IsLoaded));
        Assert.Contains(methods, m => m.Name == nameof(IService.GetCacheSize));
        Assert.Contains(methods, m => m.Name == nameof(IService.ClearCache));
    }

    [Fact]
    public void Events_AreImmutable()
    {
        // Arrange & Act
        var startedEvent = new ResourceLoadStartedEvent("res1");
        var completedEvent = new ResourceLoadCompletedEvent("res2", 100);
        var failedEvent = new ResourceLoadFailedEvent("res3", new Exception());
        var unloadedEvent = new ResourceUnloadedEvent("res4");

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<ResourceLoadStartedEvent>(startedEvent);
        Assert.IsAssignableFrom<ResourceLoadCompletedEvent>(completedEvent);
        Assert.IsAssignableFrom<ResourceLoadFailedEvent>(failedEvent);
        Assert.IsAssignableFrom<ResourceUnloadedEvent>(unloadedEvent);
    }

    [Fact]
    public void Models_AreImmutable()
    {
        // Arrange & Act
        var progress = new LoadProgress("res1", 100, 200, 50.0f);
        var metadata = new ResourceMetadata("res2", "type", 1024, "/path");
        var result = new ResourceLoadResult<string>("data", metadata, 50);

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<LoadProgress>(progress);
        Assert.IsAssignableFrom<ResourceMetadata>(metadata);
        Assert.IsAssignableFrom<ResourceLoadResult<string>>(result);
    }
}
