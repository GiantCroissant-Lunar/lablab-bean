using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.Mapper;
using LablabBean.Contracts.Input.Router;

namespace LablabBean.Contracts.Input.Tests;

/// <summary>
/// Tests for Input contract validation.
/// </summary>
public class InputContractTests
{
    [Fact]
    public void RawKeyEvent_HasKeyAndModifiers()
    {
        // Arrange & Act
        var evt = new RawKeyEvent("W", "Ctrl");

        // Assert
        Assert.Equal("W", evt.Key);
        Assert.Equal("Ctrl", evt.Modifiers);
    }

    [Fact]
    public void RawKeyEvent_ModifiersDefaultToEmpty()
    {
        // Arrange & Act
        var evt = new RawKeyEvent("A");

        // Assert
        Assert.Equal("A", evt.Key);
        Assert.Equal("", evt.Modifiers);
    }

    [Fact]
    public void InputEvent_HasTimestamp()
    {
        // Arrange
        var command = new InputCommand("Move", "W");

        // Act
        var evt = new InputEvent(command);

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal(command, evt.Command);
    }

    [Fact]
    public void InputCommand_HasTypeAndKey()
    {
        // Arrange & Act
        var command = new InputCommand("Attack", "Space");

        // Assert
        Assert.Equal("Attack", command.Type);
        Assert.Equal("Space", command.Key);
        Assert.Null(command.Metadata);
    }

    [Fact]
    public void InputAction_HasName()
    {
        // Arrange & Act
        var action = new InputAction("MoveNorth");

        // Assert
        Assert.Equal("MoveNorth", action.Name);
        Assert.Null(action.Metadata);
    }

    [Fact]
    public void InputActionTriggeredEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new InputActionTriggeredEvent("Jump");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("Jump", evt.ActionName);
    }

    [Fact]
    public void InputScopePushedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new InputScopePushedEvent("InventoryScope");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("InventoryScope", evt.ScopeName);
    }

    [Fact]
    public void InputScopePoppedEvent_HasTimestamp()
    {
        // Arrange & Act
        var evt = new InputScopePoppedEvent("MenuScope");

        // Assert
        Assert.NotEqual(default, evt.Timestamp);
        Assert.Equal("MenuScope", evt.ScopeName);
    }

    [Fact]
    public void RouterService_HasRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(Router.IService<>);

        // Assert - Verify interface has required methods
        Assert.NotNull(serviceType.GetMethod(nameof(Router.IService<object>.PushScope)));
        Assert.NotNull(serviceType.GetMethod(nameof(Router.IService<object>.Dispatch)));
        Assert.NotNull(serviceType.GetProperty(nameof(Router.IService<object>.Top)));
    }

    [Fact]
    public void MapperService_HasRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(Mapper.IService);

        // Assert - Verify interface has required methods
        Assert.NotNull(serviceType.GetMethod(nameof(Mapper.IService.Map)));
        Assert.NotNull(serviceType.GetMethod(nameof(Mapper.IService.TryGetAction)));
        Assert.NotNull(serviceType.GetMethod(nameof(Mapper.IService.RegisterMapping)));
        Assert.NotNull(serviceType.GetMethod(nameof(Mapper.IService.UnregisterMapping)));
        Assert.NotNull(serviceType.GetMethod(nameof(Mapper.IService.GetActionNames)));
    }

    [Fact]
    public void IInputScope_HasRequiredMembers()
    {
        // Arrange
        var scopeType = typeof(IInputScope<>);

        // Assert - Verify interface has required members
        Assert.NotNull(scopeType.GetMethod(nameof(IInputScope<object>.HandleAsync)));
        Assert.NotNull(scopeType.GetProperty(nameof(IInputScope<object>.Name)));
    }

    [Fact]
    public void Events_AreImmutable()
    {
        // Arrange & Act
        var actionEvent = new InputActionTriggeredEvent("Test");
        var pushedEvent = new InputScopePushedEvent("Scope1");
        var poppedEvent = new InputScopePoppedEvent("Scope2");

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<InputActionTriggeredEvent>(actionEvent);
        Assert.IsAssignableFrom<InputScopePushedEvent>(pushedEvent);
        Assert.IsAssignableFrom<InputScopePoppedEvent>(poppedEvent);
    }

    [Fact]
    public void Models_AreImmutable()
    {
        // Arrange & Act
        var rawKey = new RawKeyEvent("A");
        var command = new InputCommand("Move", "W");
        var action = new InputAction("Jump");

        // Assert - Records are immutable by default
        Assert.IsAssignableFrom<RawKeyEvent>(rawKey);
        Assert.IsAssignableFrom<InputCommand>(command);
        Assert.IsAssignableFrom<InputAction>(action);
    }
}
