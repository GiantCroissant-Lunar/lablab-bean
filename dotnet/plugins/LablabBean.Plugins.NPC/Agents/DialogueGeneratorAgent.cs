using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LablabBean.Plugins.NPC.Data;
using Microsoft.Extensions.AI;

namespace LablabBean.Plugins.NPC.Agents;

/// <summary>
/// LLM-powered dialogue generator that creates dynamic, contextual NPC conversations.
/// </summary>
public class DialogueGeneratorAgent
{
    private readonly IChatClient _chatClient;
    private readonly Random _random = new();

    public DialogueGeneratorAgent(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    /// <summary>
    /// Generates a complete dialogue tree for an NPC.
    /// </summary>
    public async Task<DialogueTree> GenerateDialogueTreeAsync(DialogueGenerationContext context)
    {
        var prompt = BuildDialoguePrompt(context);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, GetSystemPrompt()),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient.CompleteAsync(messages);
        var dialogueTree = ParseDialogueResponse(response.Message.Text ?? "", context.NPCName);

        return dialogueTree;
    }

    /// <summary>
    /// Generates a single dynamic response for an NPC in context.
    /// </summary>
    public async Task<string> GenerateResponseAsync(string npcName, string npcRole, string playerMessage, string conversationContext)
    {
        var prompt = $@"You are {npcName}, a {npcRole} in a dungeon crawler RPG.

Previous conversation:
{conversationContext}

Player says: ""{playerMessage}""

Respond in character as {npcName}. Keep response concise (1-2 sentences).";

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are an NPC in a dungeon crawler game. Stay in character and be concise."),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient.CompleteAsync(messages);
        return response.Message.Text ?? "...";
    }

    /// <summary>
    /// Generates dialogue choices for a node.
    /// </summary>
    public async Task<List<DialogueChoice>> GenerateChoicesAsync(string npcName, string npcRole, string npcText, int choiceCount = 3)
    {
        var prompt = $@"Generate {choiceCount} different dialogue response options for a player talking to {npcName} ({npcRole}).

NPC just said: ""{npcText}""

Generate {choiceCount} player response options that are:
1. Distinct in tone (friendly, neutral, aggressive, curious, etc.)
2. Lead to different conversation paths
3. Brief (5-10 words each)

Return as JSON array:
[
  {{""text"": ""Player response 1"", ""tone"": ""friendly""}},
  {{""text"": ""Player response 2"", ""tone"": ""neutral""}},
  {{""text"": ""Player response 3"", ""tone"": ""aggressive""}}
]";

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a dialogue system that generates player response options."),
            new(ChatRole.User, prompt)
        };

        var response = await _chatClient.CompleteAsync(messages);
        return ParseChoicesResponse(response.Message.Text ?? "");
    }

    private string GetSystemPrompt()
    {
        return @"You are a dialogue generator for a dungeon crawler RPG.

Generate dialogue trees in JSON format:
{
  ""id"": ""dialogue-id"",
  ""name"": ""Dialogue Name"",
  ""startNodeId"": ""node-1"",
  ""nodes"": {
    ""node-1"": {
      ""id"": ""node-1"",
      ""text"": ""NPC greeting or speech"",
      ""choices"": [
        {
          ""text"": ""Player response option"",
          ""nextNodeId"": ""node-2""
        }
      ],
      ""IsEndNode"": false
    }
  }
}

Guidelines:
- Create branching conversations with 2-4 choices per node
- Include personality and character in NPC dialogue
- Make player choices meaningful and distinct
- Terminal nodes end the conversation (IsEndNode: true)
- Keep dialogue concise but engaging
- Match NPC role and context";
    }

    private string BuildDialoguePrompt(DialogueGenerationContext context)
    {
        return $@"Generate a dialogue tree for an NPC with the following context:

NPC Name: {context.NPCName}
NPC Role: {context.NPCRole}
Personality: {context.Personality ?? "Professional and helpful"}
Location: {context.Location ?? "Dungeon"}
Player Level: {context.PlayerLevel}
{(string.IsNullOrEmpty(context.SpecialContext) ? "" : $"Special Context: {context.SpecialContext}")}

Create a dialogue tree with:
- Opening greeting based on player level and location
- At least 3 conversation branches
- Options for: getting information, accepting quests (if quest-giver), general chat, ending conversation
- Personality consistent throughout
- 5-8 total nodes

Return the complete dialogue tree in JSON format.";
    }

    private DialogueTree ParseDialogueResponse(string response, string npcName)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var tree = JsonSerializer.Deserialize<DialogueTree>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tree != null && tree.Nodes.Any())
                    return tree;
            }
        }
        catch (JsonException)
        {
            // Fallback to template
        }

        return CreateFallbackDialogue(npcName);
    }

    private List<DialogueChoice> ParseChoicesResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var choicesData = JsonSerializer.Deserialize<List<ChoiceData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (choicesData != null)
                {
                    return choicesData.Select(c => new DialogueChoice
                    {
                        Text = c.Text ?? "Continue",
                        NextNodeId = "" // Will be set by caller
                    }).ToList();
                }
            }
        }
        catch (JsonException)
        {
            // Fallback
        }

        return new List<DialogueChoice>
        {
            new() { Text = "Tell me more", NextNodeId = "" },
            new() { Text = "I have questions", NextNodeId = "" },
            new() { Text = "Goodbye", NextNodeId = "" }
        };
    }

    private DialogueTree CreateFallbackDialogue(string npcName)
    {
        var greetings = new[]
        {
            $"Greetings, traveler. I am {npcName}.",
            $"Well met! {npcName} at your service.",
            $"Ah, another adventurer. I'm {npcName}."
        };

        var greeting = greetings[_random.Next(greetings.Length)];

        return new DialogueTree
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{npcName} Dialogue",
            StartNodeId = "start",
            Nodes = new Dictionary<string, DialogueNode>
            {
                ["start"] = new DialogueNode
                {
                    Id = "start",
                    Text = greeting,
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Who are you?", NextNodeId = "about" },
                        new() { Text = "Do you have any quests?", NextNodeId = "quests" },
                        new() { Text = "Farewell", NextNodeId = "end" }
                    }
                },
                ["about"] = new DialogueNode
                {
                    Id = "about",
                    Text = $"I am {npcName}, a humble dweller of these halls.",
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Any quests available?", NextNodeId = "quests" },
                        new() { Text = "Goodbye", NextNodeId = "end" }
                    }
                },
                ["quests"] = new DialogueNode
                {
                    Id = "quests",
                    Text = "I may have some tasks for you. Check back later!",
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Tell me about yourself", NextNodeId = "about" },
                        new() { Text = "Farewell", NextNodeId = "end" }
                    }
                },
                ["end"] = new DialogueNode
                {
                    Id = "end",
                    Text = "Safe travels, adventurer.",
                    Choices = new List<DialogueChoice>()
                }
            }
        };
    }
}

/// <summary>
/// Context information for dialogue generation.
/// </summary>
public record DialogueGenerationContext(
    string NPCName,
    string NPCRole,
    int PlayerLevel = 1,
    string? Personality = null,
    string? Location = null,
    string? SpecialContext = null
);

internal class ChoiceData
{
    public string? Text { get; set; }
    public string? Tone { get; set; }
}
