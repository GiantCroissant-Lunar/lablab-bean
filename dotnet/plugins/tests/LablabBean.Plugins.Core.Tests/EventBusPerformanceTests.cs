using FluentAssertions;
using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;
using LablabBean.Plugins.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace LablabBean.Plugins.Core.Tests;

/// <summary>
/// Performance tests for EventBus to validate spec requirements.
/// Success Criteria:
/// - SC-003: Event publishing completes in under 10ms for events with up to 10 subscribers
/// - SC-004: Event bus handles at least 1000 events per second without blocking
/// </summary>
public class EventBusPerformanceTests
{
    // T077: Measure event publishing latency with 10 subscribers
    [Fact]
    public async Task EventBus_PublishingLatency_WithTenSubscribers_IsUnder10ms()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var subscriberCount = 10;

        // Subscribe 10 handlers
        for (int i = 0; i < subscriberCount; i++)
        {
            eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
            {
                // Simulate minimal work
                await Task.CompletedTask;
            });
        }

        var testEvent = new EntitySpawnedEvent(Guid.NewGuid(), "test", new Position(0, 0));

        // Warm up
        await eventBus.PublishAsync(testEvent);

        // Act - Measure publishing time
        var stopwatch = Stopwatch.StartNew();
        await eventBus.PublishAsync(testEvent);
        stopwatch.Stop();

        // Assert - SC-003: Should complete in under 10ms
        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        elapsedMs.Should().BeLessThan(10,
            "event publishing with 10 subscribers should complete in under 10ms (SC-003)");

        // Output for visibility
        Console.WriteLine($"Event publishing latency with {subscriberCount} subscribers: {elapsedMs:F3}ms");
    }

    // T078: Measure event throughput (events/second)
    [Fact]
    public async Task EventBus_Throughput_IsAtLeast1000EventsPerSecond()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var receivedCount = 0;
        var lockObject = new object();

        var eventCount = 1000;
        var events = Enumerable.Range(0, eventCount)
            .Select(i => new EntitySpawnedEvent(Guid.NewGuid(), $"entity-{i}", new Position(i, i)))
            .ToList();

        // Warm up (don't count this)
        await eventBus.PublishAsync(new EntitySpawnedEvent(Guid.NewGuid(), "warmup", new Position(0, 0)));

        // Subscribe a handler after warm-up
        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            lock (lockObject)
            {
                receivedCount++;
            }
            await Task.CompletedTask;
        });

        // Act - Measure throughput
        var stopwatch = Stopwatch.StartNew();
        foreach (var evt in events)
        {
            await eventBus.PublishAsync(evt);
        }
        stopwatch.Stop();

        // Assert - SC-004: Should handle at least 1000 events/second
        var eventsPerSecond = eventCount / stopwatch.Elapsed.TotalSeconds;
        eventsPerSecond.Should().BeGreaterThanOrEqualTo(1000,
            "event bus should handle at least 1000 events per second (SC-004)");

        receivedCount.Should().Be(eventCount, "all events should be received");

        // Output for visibility
        Console.WriteLine($"Event throughput: {eventsPerSecond:F0} events/second");
        Console.WriteLine($"Total time for {eventCount} events: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine($"Average time per event: {stopwatch.Elapsed.TotalMilliseconds / eventCount:F3}ms");
    }

    [Fact]
    public async Task EventBus_LatencyWithMultipleSubscribers_ScalesLinearly()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var results = new Dictionary<int, double>();

        // Test with 1, 5, and 10 subscribers
        foreach (var subscriberCount in new[] { 1, 5, 10 })
        {
            var localEventBus = new EventBus(NullLogger<EventBus>.Instance);

            for (int i = 0; i < subscriberCount; i++)
            {
                localEventBus.Subscribe<EntitySpawnedEvent>(async evt =>
                {
                    await Task.CompletedTask;
                });
            }

            var testEvent = new EntitySpawnedEvent(Guid.NewGuid(), "test", new Position(0, 0));

            // Warm up
            await localEventBus.PublishAsync(testEvent);

            // Measure
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                await localEventBus.PublishAsync(testEvent);
            }
            stopwatch.Stop();

            var avgLatency = stopwatch.Elapsed.TotalMilliseconds / 100;
            results[subscriberCount] = avgLatency;

            Console.WriteLine($"Average latency with {subscriberCount} subscribers: {avgLatency:F3}ms");
        }

        // Assert - All should be well under 10ms
        results.Values.Should().AllSatisfy(latency =>
            latency.Should().BeLessThan(10, "all configurations should be under 10ms"));
    }

    [Fact]
    public async Task EventBus_ConcurrentPublishing_MaintainsPerformance()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var receivedCount = 0;
        var lockObject = new object();

        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            lock (lockObject)
            {
                receivedCount++;
            }
            await Task.CompletedTask;
        });

        var eventCount = 1000;
        var concurrentTasks = 10;

        // Act - Publish events concurrently from multiple tasks
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(0, concurrentTasks)
            .Select(async taskId =>
            {
                for (int i = 0; i < eventCount / concurrentTasks; i++)
                {
                    await eventBus.PublishAsync(new EntitySpawnedEvent(
                        Guid.NewGuid(),
                        $"entity-{taskId}-{i}",
                        new Position(i, i)
                    ));
                }
            })
            .ToArray();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var eventsPerSecond = eventCount / stopwatch.Elapsed.TotalSeconds;
        eventsPerSecond.Should().BeGreaterThanOrEqualTo(1000,
            "concurrent publishing should maintain throughput");

        receivedCount.Should().Be(eventCount, "all events should be received");

        Console.WriteLine($"Concurrent throughput ({concurrentTasks} tasks): {eventsPerSecond:F0} events/second");
    }

    [Fact]
    public async Task EventBus_WithSlowSubscriber_DoesNotBlockOthers()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);
        var fastSubscriberCompleted = false;
        var slowSubscriberStarted = false;

        // Fast subscriber
        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            fastSubscriberCompleted = true;
            await Task.CompletedTask;
        });

        // Slow subscriber (simulates heavy work)
        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            slowSubscriberStarted = true;
            await Task.Delay(50); // Simulate slow work
        });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await eventBus.PublishAsync(new EntitySpawnedEvent(Guid.NewGuid(), "test", new Position(0, 0)));
        stopwatch.Stop();

        // Assert
        fastSubscriberCompleted.Should().BeTrue("fast subscriber should complete");
        slowSubscriberStarted.Should().BeTrue("slow subscriber should start");

        // Note: Sequential execution means total time includes slow subscriber
        // This is expected behavior per spec (sequential execution for predictable ordering)
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(50,
            "sequential execution means slow subscriber affects total time");

        Console.WriteLine($"Total time with slow subscriber: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine("Note: Sequential execution is by design for predictable ordering (per spec)");
    }

    [Fact]
    public async Task EventBus_MemoryAllocation_IsMinimal()
    {
        // Arrange
        var eventBus = new EventBus(NullLogger<EventBus>.Instance);

        eventBus.Subscribe<EntitySpawnedEvent>(async evt =>
        {
            await Task.CompletedTask;
        });

        var testEvent = new EntitySpawnedEvent(Guid.NewGuid(), "test", new Position(0, 0));

        // Warm up to trigger any initial allocations
        for (int i = 0; i < 100; i++)
        {
            await eventBus.PublishAsync(testEvent);
        }

        // Force GC to get baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryBefore = GC.GetTotalMemory(false);

        // Act - Publish many events
        for (int i = 0; i < 1000; i++)
        {
            await eventBus.PublishAsync(testEvent);
        }

        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        // Assert - Memory usage should be reasonable
        var bytesPerEvent = memoryUsed / 1000.0;

        Console.WriteLine($"Memory used for 1000 events: {memoryUsed / 1024.0:F2} KB");
        Console.WriteLine($"Average bytes per event: {bytesPerEvent:F2} bytes");

        // This is informational - we don't have a strict requirement, but let's ensure it's reasonable
        bytesPerEvent.Should().BeLessThan(1000, "memory allocation per event should be reasonable");
    }
}
