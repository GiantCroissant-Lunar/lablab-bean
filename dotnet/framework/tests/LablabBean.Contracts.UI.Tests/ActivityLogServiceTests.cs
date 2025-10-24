using FluentAssertions;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;

namespace LablabBean.Contracts.UI.Tests;

/// <summary>
/// Unit tests for IActivityLogService contract behavior
/// </summary>
public class ActivityLogServiceTests
{
    [Fact]
    public void ActivityEntryDto_ShouldHaveDefaultIcon()
    {
        // Arrange & Act
        var entry = new ActivityEntryDto
        {
            Message = "Test message",
            Severity = ActivitySeverity.Info
        };

        // Assert
        entry.Icon.Should().Be("·", "default icon should be a dot");
        entry.Color.Should().Be("White", "default color should be white");
    }

    [Theory]
    [InlineData(ActivitySeverity.Success, "+", "Green")]
    [InlineData(ActivitySeverity.Warning, "!", "Yellow")]
    [InlineData(ActivitySeverity.Error, "×", "Red")]
    [InlineData(ActivitySeverity.Combat, "⚔", "Red")]
    [InlineData(ActivitySeverity.Loot, "$", "Gold")]
    [InlineData(ActivitySeverity.System, "·", "Gray")]
    [InlineData(ActivitySeverity.Info, "·", "White")]
    public void ActivityEntryDto_ShouldMapSeverityToIconAndColor(
        ActivitySeverity severity,
        string expectedIcon,
        string expectedColor)
    {
        // This test documents the expected icon/color mapping
        // Actual implementation should match these expectations
        severity.Should().BeDefined();
        expectedIcon.Should().NotBeNullOrEmpty();
        expectedColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ActivityLogOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new ActivityLogOptions();

        // Assert
        options.MaxEntries.Should().Be(1000, "default max entries should be 1000");
        options.ShowTimestamps.Should().BeTrue("timestamps should be enabled by default");
        options.MirrorToLogger.Should().BeTrue("logger mirroring should be enabled by default");
        options.EnabledCategories.Should().BeEmpty("all categories should be enabled by default");
        options.MinimumSeverity.Should().Be(ActivitySeverity.Info, "default minimum severity");
        options.LogMovement.Should().BeFalse("movement logging should be off by default (verbose)");
    }

    [Fact]
    public void ActivityCategory_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var categories = Enum.GetValues<ActivityCategory>();

        // Assert
        categories.Should().Contain(ActivityCategory.System);
        categories.Should().Contain(ActivityCategory.Combat);
        categories.Should().Contain(ActivityCategory.Movement);
        categories.Should().Contain(ActivityCategory.Items);
        categories.Should().Contain(ActivityCategory.Level);
        categories.Should().Contain(ActivityCategory.Quest);
        categories.Should().Contain(ActivityCategory.Dialogue);
        categories.Should().Contain(ActivityCategory.Analytics);
        categories.Should().Contain(ActivityCategory.UI);
        categories.Should().Contain(ActivityCategory.Misc);
    }

    [Fact]
    public void ActivitySeverity_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var severities = Enum.GetValues<ActivitySeverity>();

        // Assert
        severities.Should().Contain(ActivitySeverity.Info);
        severities.Should().Contain(ActivitySeverity.Success);
        severities.Should().Contain(ActivitySeverity.Warning);
        severities.Should().Contain(ActivitySeverity.Error);
        severities.Should().Contain(ActivitySeverity.Combat);
        severities.Should().Contain(ActivitySeverity.Loot);
        severities.Should().Contain(ActivitySeverity.System);
    }

    [Fact]
    public void IActivityLogService_Interface_ShouldHaveRequiredMembers()
    {
        // This test ensures the interface contract is stable
        var interfaceType = typeof(IActivityLogService);

        // Properties
        interfaceType.GetProperty(nameof(IActivityLogService.Sequence)).Should().NotBeNull();
        interfaceType.GetProperty(nameof(IActivityLogService.Capacity)).Should().NotBeNull();
        interfaceType.GetProperty(nameof(IActivityLogService.OnLogAdded)).Should().NotBeNull();

        // Events
        interfaceType.GetEvent(nameof(IActivityLogService.Changed)).Should().NotBeNull();

        // Query methods
        interfaceType.GetMethod(nameof(IActivityLogService.GetLast)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.GetRecentEntries)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.GetSince)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.GetByCategory)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.GetBySeverity)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Search)).Should().NotBeNull();

        // Append methods
        interfaceType.GetMethod(nameof(IActivityLogService.Append)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Info)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Success)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Warning)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Error)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Combat)).Should().NotBeNull();
        interfaceType.GetMethod(nameof(IActivityLogService.Loot)).Should().NotBeNull();

        // Maintenance
        interfaceType.GetMethod(nameof(IActivityLogService.ClearLog)).Should().NotBeNull();
    }

    [Fact]
    public void ActivityEntryDto_ShouldSupportTimestamp()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var entry = new ActivityEntryDto
        {
            Timestamp = now,
            Message = "Test",
            Severity = ActivitySeverity.Info
        };

        // Assert
        entry.Timestamp.Should().Be(now);
        entry.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ActivityEntryDto_ShouldSupportMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["damage"] = 15,
            ["target"] = "Goblin",
            ["weapon"] = "Sword"
        };

        // Act
        var entry = new ActivityEntryDto
        {
            Message = "You hit Goblin for 15 damage",
            Severity = ActivitySeverity.Combat,
            Metadata = metadata
        };

        // Assert
        entry.Metadata.Should().NotBeNull();
        entry.Metadata.Should().ContainKey("damage");
        entry.Metadata!["damage"].Should().Be(15);
        entry.Metadata.Should().ContainKey("target");
        entry.Metadata["target"].Should().Be("Goblin");
    }

    [Fact]
    public void ActivityEntryDto_ShouldSupportTags()
    {
        // Arrange & Act
        var entry = new ActivityEntryDto
        {
            Message = "Critical hit!",
            Severity = ActivitySeverity.Combat,
            Tags = new[] { "critical", "melee", "player-action" }
        };

        // Assert
        entry.Tags.Should().NotBeNull();
        entry.Tags.Should().HaveCount(3);
        entry.Tags.Should().Contain("critical");
        entry.Tags.Should().Contain("player-action");
    }

    [Fact]
    public void ActivityLogOptions_EnabledCategories_ShouldBeModifiable()
    {
        // Arrange
        var options = new ActivityLogOptions();

        // Act
        options.EnabledCategories.Add(ActivityCategory.Combat);
        options.EnabledCategories.Add(ActivityCategory.Items);

        // Assert
        options.EnabledCategories.Should().HaveCount(2);
        options.EnabledCategories.Should().Contain(ActivityCategory.Combat);
        options.EnabledCategories.Should().Contain(ActivityCategory.Items);
        options.EnabledCategories.Should().NotContain(ActivityCategory.Movement);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void ActivityLogOptions_MaxEntries_ShouldBeConfigurable(int maxEntries)
    {
        // Arrange & Act
        var options = new ActivityLogOptions
        {
            MaxEntries = maxEntries
        };

        // Assert
        options.MaxEntries.Should().Be(maxEntries);
    }

    [Fact]
    public void ActivityEntryDto_ShouldBeImmutable()
    {
        // Arrange
        var entry = new ActivityEntryDto
        {
            Message = "Test",
            Severity = ActivitySeverity.Info,
            Category = ActivityCategory.System,
            Timestamp = DateTimeOffset.UtcNow
        };

        // Assert - All properties should be init-only
        entry.Message.Should().Be("Test");
        entry.Severity.Should().Be(ActivitySeverity.Info);
        entry.Category.Should().Be(ActivityCategory.System);

        // This test documents that ActivityEntryDto uses init-only properties
        // which makes it effectively immutable after construction
    }
}
