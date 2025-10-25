using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LablabBean.Plugins.Quest.Components;
using Microsoft.Extensions.AI;

namespace LablabBean.Plugins.Quest.Agents;

/// <summary>
/// LLM-powered quest generator that creates dynamic, contextual quests.
/// </summary>
public class QuestGeneratorAgent
{
    private readonly IChatClient _chatClient;
    private readonly Random _random = new();

    public QuestGeneratorAgent(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    /// <summary>
    /// Generates a quest based on player context and dungeon state.
    /// </summary>
    public async Task<GeneratedQuest> GenerateQuestAsync(QuestGenerationContext context)
    {
        var prompt = BuildQuestGenerationPrompt(context);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, GetSystemPrompt()),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient.CompleteAsync(messages);
        var questData = ParseQuestResponse(response.Message.Text ?? "");

        return questData;
    }

    /// <summary>
    /// Generates multiple quest options for player choice.
    /// </summary>
    public async Task<List<GeneratedQuest>> GenerateQuestOptionsAsync(QuestGenerationContext context, int count = 3)
    {
        var quests = new List<GeneratedQuest>();

        for (int i = 0; i < count; i++)
        {
            var quest = await GenerateQuestAsync(context);
            quests.Add(quest);
            await Task.Delay(100); // Small delay between requests
        }

        return quests;
    }

    /// <summary>
    /// Generates a quest chain (series of related quests).
    /// </summary>
    public async Task<List<GeneratedQuest>> GenerateQuestChainAsync(QuestGenerationContext context, int chainLength = 3)
    {
        var chain = new List<GeneratedQuest>();
        var chainPrompt = $"Generate a {chainLength}-part quest chain where each quest naturally leads to the next.";

        context = context with { AdditionalContext = chainPrompt };

        for (int i = 0; i < chainLength; i++)
        {
            var partContext = context with
            {
                AdditionalContext = $"{chainPrompt} This is part {i + 1} of {chainLength}."
            };

            var quest = await GenerateQuestAsync(partContext);
            chain.Add(quest);

            // Update context for next quest in chain
            context = context with
            {
                CompletedQuests = new List<string>(context.CompletedQuests ?? new()) { quest.Name }
            };
        }

        return chain;
    }

    private string GetSystemPrompt()
    {
        return @"You are a quest generator for a dungeon crawler RPG.

Generate creative, engaging quests that:
- Match the player's current level and abilities
- Fit the dungeon setting and current floor
- Have clear, achievable objectives
- Provide appropriate rewards
- Include interesting narrative context

Return quests in JSON format:
{
  ""name"": ""Quest Name"",
  ""description"": ""Quest description and story"",
  ""objectives"": [
    {
      ""type"": ""KillEnemies"",
      ""target"": ""enemy_type"",
      ""required"": 5,
      ""description"": ""Defeat 5 enemies""
    }
  ],
  ""rewards"": {
    ""experiencePoints"": 100,
    ""gold"": 50
  },
  ""minimumLevel"": 1
}

Objective types: KillEnemies, CollectItems, ReachLocation, TalkToNPC, DeliverItem, Survive";
    }

    private string BuildQuestGenerationPrompt(QuestGenerationContext context)
    {
        return $@"Generate a quest with the following context:

Player Level: {context.PlayerLevel}
Dungeon Floor: {context.DungeonLevel}
Nearby Enemies: {string.Join(", ", context.NearbyEnemyTypes ?? new())}
Available NPCs: {string.Join(", ", context.AvailableNPCs ?? new())}
Completed Quests: {context.CompletedQuests?.Count ?? 0}
Player Class: {context.PlayerClass ?? "Adventurer"}
{(string.IsNullOrEmpty(context.AdditionalContext) ? "" : $"Additional Context: {context.AdditionalContext}")}

Generate a quest appropriate for this context.";
    }

    private GeneratedQuest ParseQuestResponse(string response)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var questData = JsonSerializer.Deserialize<QuestJsonData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (questData != null)
                {
                    return ConvertToGeneratedQuest(questData);
                }
            }
        }
        catch (JsonException)
        {
            // Fallback to template quest if parsing fails
        }

        return CreateFallbackQuest();
    }

    private GeneratedQuest ConvertToGeneratedQuest(QuestJsonData data)
    {
        var questId = Guid.NewGuid().ToString();
        var objectives = new List<QuestObjective>();

        foreach (var obj in data.Objectives ?? new())
        {
            if (Enum.TryParse<ObjectiveType>(obj.Type, true, out var objectiveType))
            {
                objectives.Add(new QuestObjective(
                    Guid.NewGuid().ToString(),
                    questId,
                    objectiveType,
                    obj.Description ?? "Complete objective",
                    obj.Target ?? "",
                    obj.Required
                ));
            }
        }

        return new GeneratedQuest
        {
            Name = data.Name ?? "Unknown Quest",
            Description = data.Description ?? "A mysterious quest",
            Objectives = objectives,
            Rewards = new QuestRewards(
                data.Rewards?.ExperiencePoints ?? 100,
                data.Rewards?.Gold ?? 50
            ),
            MinimumLevel = data.MinimumLevel ?? 1
        };
    }

    private GeneratedQuest CreateFallbackQuest()
    {
        var questTypes = new[]
        {
            ("Goblin Trouble", "Clear out the goblin infestation", ObjectiveType.Kill, "Goblin", 5),
            ("Lost Item", "Find the missing artifact", ObjectiveType.Collect, "Artifact", 1),
            ("Deep Exploration", "Reach the next dungeon level", ObjectiveType.Reach, "NextLevel", 1)
        };

        var selected = questTypes[_random.Next(questTypes.Length)];
        var questId = Guid.NewGuid().ToString();
        var objectiveId = Guid.NewGuid().ToString();

        return new GeneratedQuest
        {
            Name = selected.Item1,
            Description = selected.Item2,
            Objectives = new List<QuestObjective>
            {
                new(objectiveId, questId, selected.Item3, selected.Item1, selected.Item4, selected.Item5)
            },
            Rewards = new QuestRewards(100, 50),
            MinimumLevel = 1
        };
    }
}

/// <summary>
/// Context information for quest generation.
/// </summary>
public record QuestGenerationContext(
    int PlayerLevel,
    int DungeonLevel,
    string? PlayerClass = null,
    List<string>? NearbyEnemyTypes = null,
    List<string>? AvailableNPCs = null,
    List<string>? CompletedQuests = null,
    string? AdditionalContext = null
);

/// <summary>
/// Generated quest data.
/// </summary>
public class GeneratedQuest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<QuestObjective> Objectives { get; set; } = new();
    public QuestRewards Rewards { get; set; }
    public int MinimumLevel { get; set; }
}

// JSON deserialization models
internal class QuestJsonData
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ObjectiveJsonData>? Objectives { get; set; }
    public RewardsJsonData? Rewards { get; set; }
    public int? MinimumLevel { get; set; }
}

internal class ObjectiveJsonData
{
    public string? Type { get; set; }
    public string? Target { get; set; }
    public int Required { get; set; }
    public string? Description { get; set; }
}

internal class RewardsJsonData
{
    public int ExperiencePoints { get; set; }
    public int Gold { get; set; }
}
