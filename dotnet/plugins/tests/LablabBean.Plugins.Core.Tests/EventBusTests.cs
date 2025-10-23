using FluentAssertions;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Plugins.Core.Tests;

public class EventBusTests
{
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        _eventBus = new EventBus(NullLogger<EventBus>.Instance);
    }

    // T010: Subscribe and publish single event
    [Fact]
    public async Task PublishAsync_WithSingleSubscriber_InvokesHandler()
    {
        // Arrange
        var receivedEvent = false;
        var testEvent = new TestEvent("test-data");

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            receivedEvent = true;
            evt.Data.Should().Be("test-data");
            return Task.CompletedTask;
        });

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        receivedEvent.Should().BeTrue();
    }

    // T011: Multiple subscribers receive same event
    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_InvokesAllHandlers()
    {
        // Arrange
        var subscriber1Called = false;
        var subscriber2Called = false;
        var subscriber3Called = false;
        var testEvent = new TestEvent("shared-data");

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            subscriber1Called = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            subscriber2Called = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            subscriber3Called = true;
            return Task.CompletedTask;
        });

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
        subscriber3Called.Should().BeTrue();
    }

    // T012: Subscriber exception doesn't affect others
    [Fact]
    public async Task PublishAsync_WhenSubscriberThrows_ContinuesToOtherSubscribers()
    {
        // Arrange
        var subscriber1Called = false;
        var subscriber2Called = false;
        var subscriber3Called = false;
        var testEvent = new TestEvent("error-test");

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            subscriber1Called = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            throw new InvalidOperationException("Subscriber 2 failed");
        });

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            subscriber3Called = true;
            return Task.CompletedTask;
        });

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        subscriber1Called.Should().BeTrue("first subscriber should execute");
        subscriber2Called.Should().BeFalse("second subscriber throws exception");
        subscriber3Called.Should().BeTrue("third subscriber should still execute despite second subscriber's exception");
    }

    // T013: No subscribers completes successfully
    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var testEvent = new TestEvent("no-subscribers");

        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // T014: Concurrent publishing from multiple threads
    [Fact]
    public async Task PublishAsync_ConcurrentPublishing_HandlesThreadSafely()
    {
        // Arrange
        var receivedCount = 0;
        var lockObject = new object();
        var expectedCount = 100;

        _eventBus.Subscribe<TestEvent>(evt =>
        {
            lock (lockObject)
            {
                receivedCount++;
            }
            return Task.CompletedTask;
        });

        // Act
        var tasks = Enumerable.Range(0, expectedCount)
            .Select(i => Task.Run(async () =>
            {
                await _eventBus.PublishAsync(new TestEvent($"concurrent-{i}"));
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        receivedCount.Should().Be(expectedCount, "all events should be received");
    }

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _eventBus.Subscribe<TestEvent>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("handler");
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync<TestEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("eventData");
    }

    [Fact]
    public async Task PublishAsync_ExecutesSubscribersSequentially()
    {
        // Arrange
        var executionOrder = new List<int>();
        var lockObject = new object();

        _eventBus.Subscribe<TestEvent>(async evt =>
        {
            await Task.Delay(10); // Simulate work
            lock (lockObject)
            {
                executionOrder.Add(1);
            }
        });

        _eventBus.Subscribe<TestEvent>(async evt =>
        {
            await Task.Delay(10); // Simulate work
            lock (lockObject)
            {
                executionOrder.Add(2);
            }
        });

        _eventBus.Subscribe<TestEvent>(async evt =>
        {
            await Task.Delay(10); // Simulate work
            lock (lockObject)
            {
                executionOrder.Add(3);
            }
        });

        // Act
        await _eventBus.PublishAsync(new TestEvent("sequential-test"));

        // Assert
        executionOrder.Should().ContainInOrder(1, 2, 3)
            .And.HaveCount(3, "subscribers should execute in order");
    }

    // Test event class
    private record TestEvent(string Data);
}
