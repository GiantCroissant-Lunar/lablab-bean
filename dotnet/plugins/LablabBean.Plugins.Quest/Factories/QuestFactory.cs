using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Plugins.Quest.Agents;
using LablabBean.Plugins.Quest.Components;

namespace LablabBean.Plugins.Quest.Factories;

/// <summary>
/// Factory for creating quest entities with LLM-generated content.
/// </summary>
public class QuestFactory
{
    private readonly World _world;
    private readonly QuestGeneratorAgent? _questGenerator;

    public QuestFactory(World world, QuestGeneratorAgent? questGenerator = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _questGenerator = questGenerator;
    }

    /// <summary>
    /// Creates a quest entity from a template.
    /// </summary>
    public Entity CreateQuest(string name, string description, List<QuestObjective> objectives, QuestRewards rewards, int questGiverId, QuestPrerequisites? prerequisites = null)
    {
        var questId = Guid.NewGuid().ToString();

        var quest = new Components.Quest(
            questId,
            name,
            description,
            questGiverId
        );

        // Create entity with quest and related components
        var entity = _world.Create(quest);

        // Add objectives as separate components
        foreach (var objective in objectives)
        {
            entity.Add(objective);
        }

        // Add rewards and prerequisites
        entity.Add(rewards);
        if (prerequisites.HasValue)
            entity.Add(prerequisites.Value);

        return entity;
    }

    /// <summary>
    /// Creates a quest entity from generated data.
    /// </summary>
    public Entity CreateQuestFromGenerated(GeneratedQuest generatedQuest, int questGiverId)
    {
        var prerequisites = new QuestPrerequisites(generatedQuest.MinimumLevel);

        return CreateQuest(
            generatedQuest.Name,
            generatedQuest.Description,
            generatedQuest.Objectives,
            generatedQuest.Rewards,
            questGiverId,
            prerequisites
        );
    }

    /// <summary>
    /// Generates and creates a dynamic quest using LLM.
    /// </summary>
    public async Task<Entity?> CreateDynamicQuestAsync(QuestGenerationContext context, int questGiverId)
    {
        if (_questGenerator == null)
            return null;

        try
        {
            var generatedQuest = await _questGenerator.GenerateQuestAsync(context);
            return CreateQuestFromGenerated(generatedQuest, questGiverId);
        }
        catch (Exception)
        {
            // Fallback to template quest on error
            return CreateFallbackQuest(questGiverId, context.PlayerLevel);
        }
    }

    /// <summary>
    /// Generates multiple quest options for player to choose from.
    /// </summary>
    public async Task<List<Entity>> CreateQuestOptionsAsync(QuestGenerationContext context, int questGiverId, int count = 3)
    {
        var questEntities = new List<Entity>();

        if (_questGenerator == null)
        {
            // Return template quests if no generator available
            for (int i = 0; i < count; i++)
            {
                questEntities.Add(CreateFallbackQuest(questGiverId, context.PlayerLevel));
            }
            return questEntities;
        }

        try
        {
            var generatedQuests = await _questGenerator.GenerateQuestOptionsAsync(context, count);

            foreach (var quest in generatedQuests)
            {
                questEntities.Add(CreateQuestFromGenerated(quest, questGiverId));
            }
        }
        catch (Exception)
        {
            // Fallback to template quests on error
            for (int i = 0; i < count; i++)
            {
                questEntities.Add(CreateFallbackQuest(questGiverId, context.PlayerLevel));
            }
        }

        return questEntities;
    }

    /// <summary>
    /// Creates a quest chain (series of related quests).
    /// </summary>
    public async Task<List<Entity>> CreateQuestChainAsync(QuestGenerationContext context, int questGiverId, int chainLength = 3)
    {
        var questEntities = new List<Entity>();

        if (_questGenerator == null)
        {
            // Return single quest if no generator available
            questEntities.Add(CreateFallbackQuest(questGiverId, context.PlayerLevel));
            return questEntities;
        }

        try
        {
            var generatedChain = await _questGenerator.GenerateQuestChainAsync(context, chainLength);

            List<string>? previousQuestIds = null;
            foreach (var quest in generatedChain)
            {
                var prerequisites = new QuestPrerequisites(
                    quest.MinimumLevel,
                    requiredQuests: previousQuestIds
                );

                var entity = CreateQuest(
                    quest.Name,
                    quest.Description,
                    quest.Objectives,
                    quest.Rewards,
                    questGiverId,
                    prerequisites
                );

                questEntities.Add(entity);
                previousQuestIds = new List<string> { entity.Get<Components.Quest>().Id };
            }
        }
        catch (Exception)
        {
            // Fallback to single quest on error
            questEntities.Add(CreateFallbackQuest(questGiverId, context.PlayerLevel));
        }

        return questEntities;
    }

    /// <summary>
    /// Creates a simple template quest as fallback.
    /// </summary>
    private Entity CreateFallbackQuest(int questGiverId, int playerLevel)
    {
        var questId = Guid.NewGuid().ToString();
        var objectives = new List<QuestObjective>
        {
            new(Guid.NewGuid().ToString(), questId, ObjectiveType.Kill, "Defeat goblins", "Goblin", 5)
        };

        var rewards = new QuestRewards(
            experiencePoints: 100 * playerLevel,
            gold: 50 * playerLevel
        );

        return CreateQuest(
            "Goblin Extermination",
            "The dungeon is overrun with goblins. Clear them out!",
            objectives,
            rewards,
            questGiverId
        );
    }

    /// <summary>
    /// Creates a simple collection quest.
    /// </summary>
    public Entity CreateCollectionQuest(string itemType, int count, int questGiverId, int playerLevel = 1)
    {
        var questId = Guid.NewGuid().ToString();
        var objectives = new List<QuestObjective>
        {
            new(Guid.NewGuid().ToString(), questId, ObjectiveType.Collect, $"Collect {count} {itemType}", itemType, count)
        };

        var rewards = new QuestRewards(
            experiencePoints: 150 * playerLevel,
            gold: 75 * playerLevel
        );

        return CreateQuest(
            $"Collect {itemType}",
            $"I need you to gather {count} {itemType} for me.",
            objectives,
            rewards,
            questGiverId
        );
    }

    /// <summary>
    /// Creates an exploration quest.
    /// </summary>
    public Entity CreateExplorationQuest(int targetFloor, int questGiverId, int playerLevel = 1)
    {
        var questId = Guid.NewGuid().ToString();
        var objectives = new List<QuestObjective>
        {
            new(Guid.NewGuid().ToString(), questId, ObjectiveType.Reach, $"Reach floor {targetFloor}", $"Floor{targetFloor}", 1)
        };

        var rewards = new QuestRewards(
            experiencePoints: 200 * playerLevel,
            gold: 100 * playerLevel
        );

        return CreateQuest(
            $"Explore Floor {targetFloor}",
            $"Venture deep into the dungeon and reach floor {targetFloor}.",
            objectives,
            rewards,
            questGiverId
        );
    }

    /// <summary>
    /// Checks if LLM quest generation is available.
    /// </summary>
    public bool HasLLMGeneration => _questGenerator != null;
}
