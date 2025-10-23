using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// NPC component - represents a non-player character
/// Enhanced for US3 with state management
/// </summary>
public struct NPC
{
    /// <summary>
    /// Unique identifier for the NPC
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the NPC
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// NPC role/type (e.g., "QuestGiver", "Merchant", "Guard")
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// ID of the dialogue tree associated with this NPC
    /// </summary>
    public string? DialogueTreeId { get; set; }

    /// <summary>
    /// Whether the NPC can be interacted with
    /// </summary>
    public bool IsInteractable { get; set; }

    /// <summary>
    /// Custom state data for the NPC (JSON string)
    /// Stores key-value pairs for remembering player choices
    /// </summary>
    public string? StateData { get; set; }

    public NPC(string id, string name, string role, string? dialogueTreeId = null)
    {
        Id = id;
        Name = name;
        Role = role;
        DialogueTreeId = dialogueTreeId;
        IsInteractable = true;
        StateData = null;
    }

    /// <summary>
    /// Sets a state variable for this NPC
    /// </summary>
    public void SetState(string key, string value)
    {
        var state = GetStateDict();
        state[key] = value;
        StateData = JsonSerializer.Serialize(state);
    }

    /// <summary>
    /// Gets a state variable value
    /// </summary>
    public string? GetState(string key)
    {
        var state = GetStateDict();
        return state.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if a state variable exists
    /// </summary>
    public bool HasState(string key)
    {
        var state = GetStateDict();
        return state.ContainsKey(key);
    }

    /// <summary>
    /// Clears all state variables
    /// </summary>
    public void ClearState()
    {
        StateData = null;
    }

    /// <summary>
    /// Gets all state variables as a dictionary
    /// </summary>
    private Dictionary<string, string> GetStateDict()
    {
        if (string.IsNullOrEmpty(StateData))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(StateData)
                ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
