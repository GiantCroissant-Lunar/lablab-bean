using System;
using System.Threading.Tasks;
using Arch.Core;
using LablabBean.Plugins.NPC.Agents;
using LablabBean.Plugins.NPC.Components;
using LablabBean.Plugins.NPC.Data;

namespace LablabBean.Plugins.NPC.Factories;

/// <summary>
/// Factory for creating NPC entities with optional LLM-generated dialogue.
/// </summary>
public class NPCFactory
{
    private readonly World _world;
    private readonly DialogueGeneratorAgent? _dialogueGenerator;

    public NPCFactory(World world, DialogueGeneratorAgent? dialogueGenerator = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _dialogueGenerator = dialogueGenerator;
    }

    /// <summary>
    /// Creates an NPC entity with a pre-defined dialogue tree.
    /// </summary>
    public Entity CreateNPC(string name, string role, DialogueTree? dialogueTree = null)
    {
        var npcId = Guid.NewGuid().ToString();
        var dialogueTreeId = dialogueTree?.Id;

        var npc = new Components.NPC(npcId, name, role, dialogueTreeId);

        var entity = _world.Create(npc);
        return entity;
    }

    /// <summary>
    /// Creates an NPC with LLM-generated dialogue.
    /// </summary>
    public async Task<(Entity NpcEntity, DialogueTree DialogueTree)> CreateDynamicNPCAsync(
        string name,
        string role,
        DialogueGenerationContext context)
    {
        if (_dialogueGenerator == null)
        {
            // Fallback: create NPC without dialogue
            var entity = CreateNPC(name, role);
            var fallbackTree = CreateFallbackDialogue(name, role);
            return (entity, fallbackTree);
        }

        try
        {
            var dialogueTree = await _dialogueGenerator.GenerateDialogueTreeAsync(context);
            var npc = new Components.NPC(Guid.NewGuid().ToString(), name, role, dialogueTree.Id);
            var entity = _world.Create(npc);

            return (entity, dialogueTree);
        }
        catch (Exception)
        {
            // Fallback on error
            var entity = CreateNPC(name, role);
            var fallbackTree = CreateFallbackDialogue(name, role);
            return (entity, fallbackTree);
        }
    }

    /// <summary>
    /// Creates a quest-giver NPC with dialogue.
    /// </summary>
    public async Task<(Entity NpcEntity, DialogueTree DialogueTree)> CreateQuestGiverAsync(
        string name,
        int playerLevel = 1,
        string? personality = null)
    {
        var context = new DialogueGenerationContext(
            NPCName: name,
            NPCRole: "Quest Giver",
            PlayerLevel: playerLevel,
            Personality: personality ?? "Wise and experienced, eager to help adventurers",
            Location: "Dungeon entrance",
            SpecialContext: "Has important quests for brave adventurers"
        );

        return await CreateDynamicNPCAsync(name, "QuestGiver", context);
    }

    /// <summary>
    /// Creates a merchant NPC with dialogue.
    /// </summary>
    public async Task<(Entity NpcEntity, DialogueTree DialogueTree)> CreateMerchantAsync(
        string name,
        int playerLevel = 1,
        string? personality = null)
    {
        var context = new DialogueGenerationContext(
            NPCName: name,
            NPCRole: "Merchant",
            PlayerLevel: playerLevel,
            Personality: personality ?? "Shrewd but fair, always looking for profit",
            Location: "Trading post",
            SpecialContext: "Sells useful items and equipment"
        );

        return await CreateDynamicNPCAsync(name, "Merchant", context);
    }

    /// <summary>
    /// Creates a lore NPC with interesting stories.
    /// </summary>
    public async Task<(Entity NpcEntity, DialogueTree DialogueTree)> CreateLoreKeeperAsync(
        string name,
        int playerLevel = 1,
        string? personality = null)
    {
        var context = new DialogueGenerationContext(
            NPCName: name,
            NPCRole: "Lore Keeper",
            PlayerLevel: playerLevel,
            Personality: personality ?? "Ancient and knowledgeable, speaks in riddles",
            Location: "Hidden library",
            SpecialContext: "Knows secrets of the dungeon's history"
        );

        return await CreateDynamicNPCAsync(name, "LoreKeeper", context);
    }

    /// <summary>
    /// Creates a simple template dialogue tree.
    /// </summary>
    private DialogueTree CreateFallbackDialogue(string npcName, string role)
    {
        var roleGreeting = role.ToLower() switch
        {
            "questgiver" => "I have tasks for brave souls like yourself.",
            "merchant" => "Welcome! Care to browse my wares?",
            "lorekeeper" => "Ah, a seeker of knowledge. Ask and you shall receive.",
            _ => "Hello, traveler."
        };

        return new DialogueTree
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{npcName} - {role} Dialogue",
            StartNodeId = "start",
            Nodes = new System.Collections.Generic.Dictionary<string, DialogueNode>
            {
                ["start"] = new DialogueNode
                {
                    Id = "start",
                    Text = $"Greetings! I am {npcName}. {roleGreeting}",
                    Choices = new System.Collections.Generic.List<DialogueChoice>
                    {
                        new() { Text = "Tell me about yourself", NextNodeId = "about" },
                        new() { Text = "What can you do for me?", NextNodeId = "services" },
                        new() { Text = "Goodbye", NextNodeId = "end" }
                    }
                },
                ["about"] = new DialogueNode
                {
                    Id = "about",
                    Text = $"I've been in these halls for many years, helping adventurers like you.",
                    Choices = new System.Collections.Generic.List<DialogueChoice>
                    {
                        new() { Text = "What services do you offer?", NextNodeId = "services" },
                        new() { Text = "Farewell", NextNodeId = "end" }
                    }
                },
                ["services"] = new DialogueNode
                {
                    Id = "services",
                    Text = role.ToLower() switch
                    {
                        "questgiver" => "I can offer you quests and guidance.",
                        "merchant" => "I buy and sell equipment. Show me what you have!",
                        "lorekeeper" => "I share knowledge of this place's secrets.",
                        _ => "I provide assistance to travelers."
                    },
                    Choices = new System.Collections.Generic.List<DialogueChoice>
                    {
                        new() { Text = "Tell me more", NextNodeId = "about" },
                        new() { Text = "That's all", NextNodeId = "end" }
                    }
                },
                ["end"] = new DialogueNode
                {
                    Id = "end",
                    Text = "Safe travels, adventurer. May fortune favor you!",
                    Choices = new System.Collections.Generic.List<DialogueChoice>()
                }
            }
        };
    }

    /// <summary>
    /// Checks if LLM dialogue generation is available.
    /// </summary>
    public bool HasLLMGeneration => _dialogueGenerator != null;
}
