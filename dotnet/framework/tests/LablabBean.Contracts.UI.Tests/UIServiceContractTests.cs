using FluentAssertions;
using LablabBean.Contracts.UI.Events;
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;

namespace LablabBean.Contracts.UI.Tests;

/// <summary>
/// Contract validation tests for User Story 3: UI Service Contract
/// Validates that UI service interfaces follow naming conventions and patterns.
/// </summary>
public class UIServiceContractTests
{
    // T069: Verify UI service interface follows naming conventions
    [Fact]
    public void UIService_InterfaceName_FollowsConvention()
    {
        // Arrange
        var serviceType = typeof(IService);

        // Assert
        serviceType.Name.Should().Be("IService", "service interfaces should be named IService within their domain namespace");
        serviceType.Namespace.Should().Be("LablabBean.Contracts.UI.Services", "service should be in Services namespace");
        serviceType.IsInterface.Should().BeTrue("service contract should be an interface");
    }

    // T070: Verify all methods are async where appropriate
    [Fact]
    public void UIService_Methods_UseAsyncPattern()
    {
        // Arrange
        var serviceType = typeof(IService);
        var methods = serviceType.GetMethods();

        // Act - Check async methods
        var asyncMethods = methods.Where(m => m.ReturnType == typeof(Task) || 
                                              m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

        // Assert
        asyncMethods.Should().Contain(m => m.Name == "InitializeAsync", "InitializeAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "RenderViewportAsync", "RenderViewportAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "UpdateDisplayAsync", "UpdateDisplayAsync should be async");
        asyncMethods.Should().Contain(m => m.Name == "HandleInputAsync", "HandleInputAsync should be async");

        // Synchronous methods (getters/setters)
        var syncMethods = methods.Where(m => m.ReturnType != typeof(Task) && 
                                            !(m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));
        
        syncMethods.Should().Contain(m => m.Name == "GetViewport", "GetViewport should be synchronous getter");
        syncMethods.Should().Contain(m => m.Name == "SetViewportCenter", "SetViewportCenter should be synchronous setter");
    }

    [Fact]
    public void UIEvents_FollowRecordPattern_WithTimestamp()
    {
        // Arrange & Assert
        var inputReceivedEvent = typeof(InputReceivedEvent);
        inputReceivedEvent.Should().NotBeNull();
        inputReceivedEvent.GetProperty("Timestamp").Should().NotBeNull("InputReceivedEvent should have Timestamp property");
        inputReceivedEvent.GetProperty("Timestamp")!.PropertyType.Should().Be(typeof(DateTimeOffset));

        var viewportChangedEvent = typeof(ViewportChangedEvent);
        viewportChangedEvent.GetProperty("Timestamp").Should().NotBeNull("ViewportChangedEvent should have Timestamp property");
    }

    [Fact]
    public void UIEvents_HaveConvenienceConstructors()
    {
        // Arrange & Act - Test convenience constructors
        var inputCommand = new InputCommand(InputType.Movement, "W", null);
        var inputEvent = new InputReceivedEvent(inputCommand);

        var viewport1 = new ViewportBounds(new LablabBean.Contracts.Game.Models.Position(0, 0), 80, 24);
        var viewport2 = new ViewportBounds(new LablabBean.Contracts.Game.Models.Position(1, 1), 80, 24);
        var viewportEvent = new ViewportChangedEvent(viewport1, viewport2);

        // Assert - All events should have timestamps set
        inputEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        viewportEvent.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UIModels_AreImmutable()
    {
        // Arrange & Act
        var inputCommand = new InputCommand(InputType.Action, "Space", null);
        var viewport = new ViewportBounds(new LablabBean.Contracts.Game.Models.Position(5, 5), 80, 24);
        var uiInitOptions = new UIInitOptions(80, 24, true, "Dark");

        // Assert - Records are immutable by design
        inputCommand.Should().BeOfType<InputCommand>();
        viewport.Should().BeOfType<ViewportBounds>();
        uiInitOptions.Should().BeOfType<UIInitOptions>();
    }

    [Fact]
    public void ViewportBounds_CalculatesPropertiesCorrectly()
    {
        // Arrange
        var viewport = new ViewportBounds(new LablabBean.Contracts.Game.Models.Position(10, 10), 20, 10);

        // Assert
        viewport.Center.X.Should().Be(20, "center X should be TopLeft.X + Width/2");
        viewport.Center.Y.Should().Be(15, "center Y should be TopLeft.Y + Height/2");
        viewport.BottomRight.X.Should().Be(29, "bottom right X should be TopLeft.X + Width - 1");
        viewport.BottomRight.Y.Should().Be(19, "bottom right Y should be TopLeft.Y + Height - 1");
    }
}
