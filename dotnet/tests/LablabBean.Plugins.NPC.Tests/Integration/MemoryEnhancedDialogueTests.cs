using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arch.Core;
using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Components;
using LablabBean.Plugins.NPC.Data;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Plugins.NPC.Tests.Integration;

public class MemoryEnhancedDialogueTests : IDisposable
{
    private readonly World _world;
    private readonly DialogueSystem _baseDialogueSystem;
    private readonly TestMemoryService _memoryService;
    private readonly MemoryEnhancedDialogueSystem _sut;

    public MemoryEnhancedDialogueTests()
    {
        _world = World.Create();
        _baseDialogueSystem = new DialogueSystem(_world);
        _memoryService = new TestMemoryService();
        _sut = new MemoryEnhancedDialogueSystem(
            _baseDialogueSystem,
            _memoryService,
            NullLogger<MemoryEnhancedDialogueSystem>.Instance,
            _world
        );
    }

    [Fact]
    public async Task StartDialogue_StoresMemory()
    {
        // Arrange
        var playerEntity = _world.Create(new Components.NPC { Id = "player", Name = "Hero" });
        var npcEntity = _world.Create(new Components.NPC { Id = "npc-001", Name = "Merchant" });

        var dialogueTree = CreateTestDialogueTree();
        _sut.LoadDialogueTree(dialogueTree);

        // Act
        var dialogueEntity = await _sut.StartDialogueAsync(
            playerEntity,
            npcEntity,
            dialogueTree.Id,
            playerId: "test-player");

        await Task.Delay(100); // Give async operation time to complete

        // Assert
        dialogueEntity.Should().NotBeNull();
        _memoryService.StoredMemories.Should().HaveCountGreaterThan(0);

        var memory = _memoryService.StoredMemories[0];
        memory.EntityId.Should().Be("test-player");
        memory.MemoryType.Should().Be("conversation_start");
        memory.Tags.Should().ContainKey("npc_id");
        memory.Tags["npc_id"].Should().Be("npc-001");
    }

    [Fact]
    public async Task SelectChoice_StoresChoiceMemory()
    {
        // Arrange
        var playerEntity = _world.Create(new Components.NPC { Id = "player", Name = "Hero" });
        var npcEntity = _world.Create(new Components.NPC { Id = "npc-001", Name = "Merchant" });

        var dialogueTree = CreateTestDialogueTree();
        _sut.LoadDialogueTree(dialogueTree);

        var dialogueEntity = await _sut.StartDialogueAsync(
            playerEntity,
            npcEntity,
            dialogueTree.Id,
            playerId: "test-player");

        await Task.Delay(100);
        _memoryService.StoredMemories.Clear(); // Clear start memory

        // Act
        await _sut.SelectChoiceAsync(dialogueEntity!.Value, "choice-1", playerId: "test-player");
        await Task.Delay(100);

        // Assert
        _memoryService.StoredMemories.Should().HaveCount(1);

        var choiceMemory = _memoryService.StoredMemories[0];
        choiceMemory.MemoryType.Should().Be("dialogue_choice");
        choiceMemory.Tags.Should().ContainKey("choice_id");
        choiceMemory.Tags["choice_id"].Should().Be("choice-1");
        choiceMemory.Importance.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public async Task GetNpcMemories_RetrievesRelevantMemories()
    {
        // Arrange
        var playerEntity = _world.Create(new Components.NPC { Id = "player", Name = "Hero" });
        var npcEntity = _world.Create(new Components.NPC { Id = "npc-001", Name = "Merchant" });

        var dialogueTree = CreateTestDialogueTree();
        _sut.LoadDialogueTree(dialogueTree);

        await _sut.StartDialogueAsync(playerEntity, npcEntity, dialogueTree.Id, playerId: "test-player");
        await Task.Delay(100);

        // Act
        var memories = await _sut.GetNpcMemoriesAsync("test-player", "npc-001");

        // Assert
        memories.Should().HaveCountGreaterThan(0);
        memories[0].Memory.Tags.Should().ContainKey("npc_id");
        memories[0].Memory.Tags["npc_id"].Should().Be("npc-001");
    }

    [Fact]
    public async Task AnalyzeRelationship_CalculatesInsights()
    {
        // Arrange
        var playerEntity = _world.Create(new Components.NPC { Id = "player", Name = "Hero" });
        var npcEntity = _world.Create(new Components.NPC { Id = "npc-001", Name = "Merchant" });

        var dialogueTree = CreateTestDialogueTree();
        _sut.LoadDialogueTree(dialogueTree);

        // Simulate multiple interactions
        for (int i = 0; i < 3; i++)
        {
            var de = await _sut.StartDialogueAsync(playerEntity, npcEntity, dialogueTree.Id, playerId: "test-player");
            _sut.EndDialogue(de!.Value);
            await Task.Delay(50);
        }

        // Act
        var insights = await _sut.AnalyzeRelationshipAsync("test-player", "npc-001");

        // Assert
        insights.NpcId.Should().Be("npc-001");
        insights.TotalInteractions.Should().BeGreaterThan(0);
        insights.RelationshipLevel.Should().NotBeNullOrEmpty();
    }

    private DialogueTree CreateTestDialogueTree()
    {
        return new DialogueTree
        {
            Id = "test-tree",
            Name = "Test Dialogue",
            Nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "Hello, traveler!",
                    IsStartNode = true,
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "choice-1",
                            Text = "Hello! How are you?",
                            NextNodeId = "node-2"
                        },
                        new DialogueChoice
                        {
                            Id = "choice-2",
                            Text = "Goodbye.",
                            EndsDialogue = true
                        }
                    }
                },
                new DialogueNode
                {
                    Id = "node-2",
                    Text = "I'm doing well, thanks!",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "choice-3",
                            Text = "That's good to hear.",
                            EndsDialogue = true
                        }
                    }
                }
            }
        };
    }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
    }

    private class TestMemoryService : IMemoryService
    {
        public List<MemoryEntry> StoredMemories { get; } = new();

        public Task<string> StoreMemoryAsync(MemoryEntry memory, System.Threading.CancellationToken cancellationToken = default)
        {
            StoredMemories.Add(memory);
            return Task.FromResult(memory.Id);
        }

        public Task<IReadOnlyList<MemoryResult>> RetrieveRelevantMemoriesAsync(
            string queryText,
            MemoryRetrievalOptions options,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var results = StoredMemories
                .Where(m => m.EntityId == options.EntityId)
                .Where(m => options.Tags.Count == 0 || options.Tags.All(t => m.Tags.ContainsKey(t.Key) && m.Tags[t.Key] == t.Value))
                .Select(m => new MemoryResult
                {
                    Memory = m,
                    RelevanceScore = 0.9,
                    Source = "test"
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<MemoryResult>>(results);
        }

        public Task<MemoryEntry?> GetMemoryByIdAsync(string memoryId, System.Threading.CancellationToken cancellationToken = default)
        {
            var memory = StoredMemories.FirstOrDefault(m => m.Id == memoryId);
            return Task.FromResult(memory);
        }

        public Task UpdateMemoryImportanceAsync(string memoryId, double importance, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> CleanOldMemoriesAsync(string entityId, TimeSpan olderThan, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(true);
        }
    }
}
