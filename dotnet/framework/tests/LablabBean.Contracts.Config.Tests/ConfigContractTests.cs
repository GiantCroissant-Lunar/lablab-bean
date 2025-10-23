using LablabBean.Contracts.Config;
using LablabBean.Contracts.Config.Services;

namespace LablabBean.Contracts.Config.Tests;

/// <summary>
/// Tests for Config contract validation.
/// </summary>
public class ConfigContractTests
{
    [Fact]
    public void ConfigChangedEvent_HasAllProperties()
    {
        // Arrange & Act
        var evt = new ConfigChangedEvent("game:difficulty", "normal", "hard");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("game:difficulty", evt.Key);
        Assert.Equal("normal", evt.OldValue);
        Assert.Equal("hard", evt.NewValue);
    }

    [Fact]
    public void ConfigChangedEvent_SupportsNullValues()
    {
        // Arrange & Act
        var evt = new ConfigChangedEvent("new:key", null, "value");

        // Assert
        Assert.Null(evt.OldValue);
        Assert.Equal("value", evt.NewValue);
    }

    [Fact]
    public void ConfigReloadedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new ConfigReloadedEvent();

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
    }

    [Fact]
    public void IService_HasRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(IService);

        // Assert - Verify interface has required methods (check by name only due to generic overloads)
        var methods = serviceType.GetMethods();
        Assert.Contains(methods, m => m.Name == nameof(IService.Get));
        Assert.Contains(methods, m => m.Name == nameof(IService.GetSection));
        Assert.Contains(methods, m => m.Name == nameof(IService.Set));
        Assert.Contains(methods, m => m.Name == nameof(IService.Exists));
        Assert.Contains(methods, m => m.Name == nameof(IService.ReloadAsync));
    }

    [Fact]
    public void IConfigSection_HasRequiredMembers()
    {
        // Arrange
        var sectionType = typeof(IConfigSection);

        // Assert - Verify interface has required members (check by name only due to generic overloads)
        var methods = sectionType.GetMethods();
        Assert.Contains(methods, m => m.Name == nameof(IConfigSection.Get));
        Assert.Contains(methods, m => m.Name == nameof(IConfigSection.GetSection));
        Assert.Contains(methods, m => m.Name == nameof(IConfigSection.GetKeys));
        Assert.NotNull(sectionType.GetProperty(nameof(IConfigSection.Path)));
    }

    [Fact]
    public void Events_AreImmutable()
    {
        // Arrange & Act
        var changedEvent = new ConfigChangedEvent("key", "old", "new");
        var reloadedEvent = new ConfigReloadedEvent();

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<ConfigChangedEvent>(changedEvent);
        Assert.IsAssignableFrom<ConfigReloadedEvent>(reloadedEvent);
    }

    [Fact]
    public void ConfigChangedEvent_SupportsHierarchicalKeys()
    {
        // Arrange & Act
        var evt = new ConfigChangedEvent("game:graphics:resolution", "1920x1080", "2560x1440");

        // Assert
        Assert.Equal("game:graphics:resolution", evt.Key);
        Assert.Contains(":", evt.Key);
    }
}
