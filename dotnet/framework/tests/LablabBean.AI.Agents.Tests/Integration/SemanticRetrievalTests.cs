using FluentAssertions;
using LablabBean.AI.Agents.Configuration;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using NSubstitute;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Integration tests for semantic memory retrieval
/// Tests T020 - Complete end-to-end workflow of storing and retrieving memories using semantic search
/// </summary>
public class SemanticRetrievalTests : IAsyncLifetime
{
    private readonly ILogger<LablabBean.AI.Agents.Services.MemoryService> _logger;
    private readonly IOptions<KernelMemoryOptions> _options;
    private readonly IKernelMemory _kernelMemory;
    private LablabBean.AI.Agents.Services.MemoryService? _memoryService;

    public SemanticRetrievalTests()
    {
        _logger = Substitute.For<ILogger<LablabBean.AI.Agents.Services.MemoryService>>();
        _kernelMemory = Substitute.For<IKernelMemory>();

        // Configure with in-memory storage for integration testing
        var memoryOptions = new KernelMemoryOptions
        {
            Embedding = new EmbeddingOptions
            {
                Provider = "OpenAI",
                ModelName = "text-embedding-3-small",
                MaxTokens = 8191,
                Endpoint = null // Will use OpenAI default
            },
            Storage = new StorageOptions
            {
                Provider = "Volatile", // In-memory for tests
                ConnectionString = "",
                CollectionName = "integration-test-memories"
            },
            TextGeneration = new TextGenerationOptions
            {
                Provider = "OpenAI",
                ModelName = "gpt-4",
                MaxTokens = 4096,
                Endpoint = null
            }
        };

        _options = Options.Create(memoryOptions);
    }

    public async Task InitializeAsync()
    {
        _memoryService = new LablabBean.AI.Agents.Services.MemoryService(_logger, _kernelMemory, _options);

        // Seed test data once T021 is implemented
        // For now, initialization will be minimal
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Cleanup after tests
        await Task.CompletedTask;
    }

    #region T020: Semantic Retrieval Integration Tests

    [Fact]
    public async Task SemanticRetrieval_CustomerServiceScenario_RetrievesRelevantMemories()
    {
        // This test verifies the core user story:
        // "NPCs should retrieve contextually relevant memories instead of just the most recent ones"

        // Arrange - Store 10 memories: 3 about customers, 7 irrelevant
        var customerMemories = new[]
        {
            new MemoryEntry
            {
                Id = "mem-001",
                Content = "Employee successfully handled an angry customer by offering a refund and sincere apology. Customer left satisfied.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.8,
                Tags = new Dictionary<string, string> { { "scenario", "customer_service" } }
            },
            new MemoryEntry
            {
                Id = "mem-002",
                Content = "Learned that upset customers respond better to empathy and active listening rather than defensive explanations.",
                EntityId = "employee_001",
                MemoryType = "lesson_learned",
                Importance = 0.9,
                Tags = new Dictionary<string, string> { { "scenario", "customer_service" } }
            },
            new MemoryEntry
            {
                Id = "mem-003",
                Content = "Customer complained about long wait times. Offered discount on next visit and personally ensured faster service.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.7,
                Tags = new Dictionary<string, string> { { "scenario", "customer_service" } }
            }
        };

        var irrelevantMemories = new[]
        {
            new MemoryEntry
            {
                Id = "mem-004",
                Content = "Organized supplies in the basement storage room. Found old equipment that needs disposal.",
                EntityId = "employee_001",
                MemoryType = "task",
                Importance = 0.3
            },
            new MemoryEntry
            {
                Id = "mem-005",
                Content = "Attended team meeting about new work schedule policies. Shift changes start next month.",
                EntityId = "employee_001",
                MemoryType = "meeting",
                Importance = 0.5
            },
            new MemoryEntry
            {
                Id = "mem-006",
                Content = "Noticed the coffee machine needs repair. Submitted maintenance request.",
                EntityId = "employee_001",
                MemoryType = "observation",
                Importance = 0.2
            },
            new MemoryEntry
            {
                Id = "mem-007",
                Content = "Helped coworker move heavy boxes. They thanked me for the assistance.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.4
            },
            new MemoryEntry
            {
                Id = "mem-008",
                Content = "Read new company policy about overtime compensation. Seems fair.",
                EntityId = "employee_001",
                MemoryType = "knowledge",
                Importance = 0.5
            },
            new MemoryEntry
            {
                Id = "mem-009",
                Content = "Cleaned workspace at end of shift. Everything organized and ready for tomorrow.",
                EntityId = "employee_001",
                MemoryType = "task",
                Importance = 0.2
            },
            new MemoryEntry
            {
                Id = "mem-010",
                Content = "Boss mentioned good performance this week. Feeling appreciated.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.6
            }
        };

        // Act & Assert - Store all memories (should throw NotImplemented until T021)
        foreach (var memory in customerMemories.Concat(irrelevantMemories))
        {
            var storeAction = async () => await _memoryService!.StoreMemoryAsync(memory);
            await storeAction.Should().ThrowAsync<NotImplementedException>();
        }

        // Once T021-T022 are implemented, this test should:
        // 1. Successfully store all memories
        // 2. Query for "How should I handle an angry customer?"
        // 3. Verify top 3 results are customer-related memories (mem-001, mem-002, mem-003)
        // 4. Verify relevance scores > 0.7
        // 5. Verify irrelevant memories have low scores or are filtered out

        /* UNCOMMENT AFTER T021-T022 IMPLEMENTATION:

        // Store all memories successfully
        foreach (var memory in customerMemories.Concat(irrelevantMemories))
        {
            await _memoryService!.StoreMemoryAsync(memory);
        }

        // Query for customer service guidance
        var queryText = "How should I handle an angry customer?";
        var options = new MemoryRetrievalOptions
        {
            EntityId = "employee_001",
            MinRelevanceScore = 0.7,
            Limit = 5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(queryText, options);

        // Verify results
        results.Should().NotBeEmpty("semantic search should find relevant memories");
        results.Should().HaveCountGreaterThanOrEqualTo(3, "should find at least the 3 customer-related memories");

        // Top results should be customer-related
        var topResult = results.First();
        topResult.RelevanceScore.Should().BeGreaterThan(0.7, "top result should be highly relevant");
        topResult.Memory.Id.Should().BeOneOf("mem-001", "mem-002", "mem-003", "top result should be customer-related");

        // All returned results should meet minimum relevance threshold
        results.Should().OnlyContain(r => r.RelevanceScore >= 0.7, "all results should meet minimum relevance score");

        // Most results should be customer-related (at least 60%)
        var customerRelatedCount = results.Count(r => r.Memory.Id.StartsWith("mem-00") && int.Parse(r.Memory.Id.Split('-')[1]) <= 3);
        var ratio = (double)customerRelatedCount / results.Count;
        ratio.Should().BeGreaterThan(0.6, "majority of results should be customer-related");
        */
    }

    [Fact]
    public async Task SemanticRetrieval_TacticalMemory_FindsSimilarCombatScenarios()
    {
        // This test verifies tactical memory retrieval for User Story 4 foundation

        // Arrange - Store tactical observations
        var tacticalMemories = new[]
        {
            new MemoryEntry
            {
                Id = "tactical-001",
                Content = "Player tends to rush forward aggressively when health is high. Vulnerable to flanking attacks.",
                EntityId = "enemy_archer_001",
                MemoryType = "tactical_observation",
                Importance = 0.9,
                Tags = new Dictionary<string, string>
                {
                    { "behavior", "aggressive" },
                    { "counter", "flanking" }
                }
            },
            new MemoryEntry
            {
                Id = "tactical-002",
                Content = "Player uses hit-and-run tactics, striking quickly then retreating. Hard to pin down in melee range.",
                EntityId = "enemy_archer_001",
                MemoryType = "tactical_observation",
                Importance = 0.8,
                Tags = new Dictionary<string, string>
                {
                    { "behavior", "hit_and_run" },
                    { "counter", "ranged" }
                }
            },
            new MemoryEntry
            {
                Id = "tactical-003",
                Content = "Player exhibited aggressive rush tactics in last 3 encounters. Kiting strategy was 70% effective as counter.",
                EntityId = "enemy_archer_001",
                MemoryType = "tactical_observation",
                Importance = 0.85,
                Tags = new Dictionary<string, string>
                {
                    { "behavior", "aggressive" },
                    { "counter", "kiting" }
                }
            }
        };

        // Act & Assert - Should throw NotImplemented until T021
        foreach (var memory in tacticalMemories)
        {
            var storeAction = async () => await _memoryService!.StoreMemoryAsync(memory);
            await storeAction.Should().ThrowAsync<NotImplementedException>();
        }

        /* UNCOMMENT AFTER T021-T022 IMPLEMENTATION:

        // Store tactical memories
        foreach (var memory in tacticalMemories)
        {
            await _memoryService!.StoreMemoryAsync(memory);
        }

        // Query for similar aggressive player behavior
        var queryText = "Player is charging at me aggressively. What counter-tactics work?";
        var options = new MemoryRetrievalOptions
        {
            EntityId = "enemy_archer_001",
            MemoryType = "tactical_observation",
            MinRelevanceScore = 0.7,
            Limit = 3
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(queryText, options);

        // Verify tactical memories are retrieved
        results.Should().NotBeEmpty("should find relevant tactical memories");
        results.First().Memory.Id.Should().BeOneOf("tactical-001", "tactical-003", "should prioritize aggressive behavior counters");
        results.Should().OnlyContain(r => r.Memory.Tags.ContainsKey("behavior"), "results should have tactical tags");
        */
    }

    [Fact]
    public async Task SemanticRetrieval_CrossEntitySearch_FindsRelevantMemoriesAcrossNPCs()
    {
        // This test verifies that memories can be searched across entities when needed
        // Useful for knowledge base scenarios where information is shared

        // Arrange - Store memories for different entities
        var sharedKnowledgeMemories = new[]
        {
            new MemoryEntry
            {
                Id = "kb-001",
                Content = "Company policy: All customer complaints must be documented and escalated to management within 24 hours.",
                EntityId = "knowledge_base",
                MemoryType = "policy",
                Importance = 1.0,
                Tags = new Dictionary<string, string> { { "topic", "customer_complaints" } }
            },
            new MemoryEntry
            {
                Id = "emp-001",
                Content = "I learned from the employee handbook that serious complaints need manager approval for refunds over $100.",
                EntityId = "employee_002",
                MemoryType = "knowledge",
                Importance = 0.8,
                Tags = new Dictionary<string, string> { { "topic", "customer_complaints" } }
            },
            new MemoryEntry
            {
                Id = "boss-001",
                Content = "Reminded team about complaint escalation procedures during meeting. Critical for customer satisfaction.",
                EntityId = "boss_001",
                MemoryType = "instruction",
                Importance = 0.9,
                Tags = new Dictionary<string, string> { { "topic", "customer_complaints" } }
            }
        };

        // Act & Assert - Should throw NotImplemented until T021
        foreach (var memory in sharedKnowledgeMemories)
        {
            var storeAction = async () => await _memoryService!.StoreMemoryAsync(memory);
            await storeAction.Should().ThrowAsync<NotImplementedException>();
        }

        /* UNCOMMENT AFTER T021-T022 IMPLEMENTATION:

        // Store knowledge memories
        foreach (var memory in sharedKnowledgeMemories)
        {
            await _memoryService!.StoreMemoryAsync(memory);
        }

        // Query without entity filter - search across all entities
        var queryText = "What is the company policy on handling customer complaints?";
        var options = new MemoryRetrievalOptions
        {
            EntityId = null, // Search all entities
            MinRelevanceScore = 0.7,
            Limit = 5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(queryText, options);

        // Verify results from multiple entities
        results.Should().NotBeEmpty("should find knowledge across entities");
        var entityIds = results.Select(r => r.Memory.EntityId).Distinct().ToList();
        entityIds.Should().HaveCountGreaterThan(1, "should find memories from multiple entities");

        // Knowledge base memory should be top result (highest importance)
        results.First().Memory.EntityId.Should().Be("knowledge_base", "official policy should rank highest");
        */
    }

    [Fact]
    public async Task SemanticRetrieval_WithTimeFilter_OnlyReturnsRecentMemories()
    {
        // This test verifies time-based filtering works correctly

        // Arrange
        var now = DateTimeOffset.UtcNow;
        var memories = new[]
        {
            new MemoryEntry
            {
                Id = "recent-001",
                Content = "Customer was happy with our service today. They said they'll recommend us to friends.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.7,
                Timestamp = now.AddHours(-2) // 2 hours ago
            },
            new MemoryEntry
            {
                Id = "old-001",
                Content = "Had a great interaction with a customer last month. They seemed very satisfied.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.7,
                Timestamp = now.AddDays(-30) // 30 days ago
            },
            new MemoryEntry
            {
                Id = "recent-002",
                Content = "Today's customer appreciated the quick service. Mentioned our efficiency.",
                EntityId = "employee_001",
                MemoryType = "interaction",
                Importance = 0.6,
                Timestamp = now.AddHours(-5) // 5 hours ago
            }
        };

        // Act & Assert - Should throw NotImplemented until T021
        foreach (var memory in memories)
        {
            var storeAction = async () => await _memoryService!.StoreMemoryAsync(memory);
            await storeAction.Should().ThrowAsync<NotImplementedException>();
        }

        /* UNCOMMENT AFTER T021-T022 IMPLEMENTATION:

        // Store memories with different timestamps
        foreach (var memory in memories)
        {
            await _memoryService!.StoreMemoryAsync(memory);
        }

        // Query with time filter - last 7 days only
        var queryText = "positive customer interactions";
        var options = new MemoryRetrievalOptions
        {
            EntityId = "employee_001",
            FromTimestamp = now.AddDays(-7),
            ToTimestamp = now,
            MinRelevanceScore = 0.6,
            Limit = 10
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(queryText, options);

        // Verify only recent memories are returned
        results.Should().NotBeEmpty("should find recent memories");
        results.Should().OnlyContain(r => r.Memory.Id.StartsWith("recent"), "should only return memories from last 7 days");
        results.Should().NotContain(r => r.Memory.Id == "old-001", "should not return 30-day-old memory");
        */
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task SemanticRetrieval_With100Memories_CompletesWithin200ms()
    {
        // This test will verify the performance requirement: <200ms p95 latency
        // TODO: Implement after T021-T022

        // For now, just verify the service exists
        _memoryService.Should().NotBeNull();
        await Task.CompletedTask;

        /* UNCOMMENT AFTER T021-T022 IMPLEMENTATION:

        // Store 100 memories
        var memories = Enumerable.Range(1, 100).Select(i => new MemoryEntry
        {
            Id = $"perf-{i:D3}",
            Content = $"Memory content number {i} with various details about different topics and scenarios.",
            EntityId = "employee_001",
            MemoryType = i % 3 == 0 ? "interaction" : i % 3 == 1 ? "observation" : "knowledge",
            Importance = 0.5 + (i % 50) / 100.0
        }).ToList();

        foreach (var memory in memories)
        {
            await _memoryService!.StoreMemoryAsync(memory);
        }

        // Query and measure performance
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var queryText = "important interactions and observations";
        var options = new MemoryRetrievalOptions
        {
            EntityId = "employee_001",
            MinRelevanceScore = 0.7,
            Limit = 5
        };

        var results = await _memoryService.RetrieveRelevantMemoriesAsync(queryText, options);

        sw.Stop();

        // Verify performance
        sw.ElapsedMilliseconds.Should().BeLessThan(200, "retrieval should complete within 200ms");
        results.Should().NotBeEmpty("should find relevant memories");
        */
    }

    #endregion
}
