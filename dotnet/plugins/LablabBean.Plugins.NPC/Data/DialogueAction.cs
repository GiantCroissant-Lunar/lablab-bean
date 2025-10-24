using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Data;

/// <summary>
/// Types of actions that can be triggered from dialogue
/// </summary>
public enum DialogueActionType
{
    /// <summary>
    /// Accept a quest (params: { questId: string })
    /// </summary>
    AcceptQuest,

    /// <summary>
    /// Complete a quest (params: { questId: string })
    /// </summary>
    CompleteQuest,

    /// <summary>
    /// Set NPC state variable (params: { key: string, value: object })
    /// </summary>
    SetNPCState,

    /// <summary>
    /// Give item to player (params: { itemId: string, quantity: int })
    /// </summary>
    GiveItem,

    /// <summary>
    /// Take item from player (params: { itemId: string, quantity: int })
    /// </summary>
    TakeItem,

    /// <summary>
    /// Give gold to player (params: { amount: int })
    /// </summary>
    GiveGold,

    /// <summary>
    /// Take gold from player (params: { amount: int })
    /// </summary>
    TakeGold,

    /// <summary>
    /// Open trade interface (params: { merchantId: string })
    /// </summary>
    OpenTrade,

    /// <summary>
    /// Trigger custom game event (params: { eventName: string, data: object })
    /// </summary>
    TriggerEvent,

    /// <summary>
    /// Set player state variable (params: { key: string, value: object })
    /// </summary>
    SetPlayerState,

    /// <summary>
    /// Start combat encounter (params: { encounterId: string })
    /// </summary>
    StartCombat
}

/// <summary>
/// Represents an action that can be executed during dialogue
/// Actions can modify game state, grant items, trigger quests, etc.
/// </summary>
public class DialogueAction
{
    /// <summary>
    /// The type of action to execute
    /// </summary>
    public DialogueActionType Type { get; set; }

    /// <summary>
    /// Parameters for the action (varies by action type)
    /// Stored as key-value pairs for flexibility
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Optional condition that must be met for this action to execute
    /// Format: Same DSL as dialogue choice conditions
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Optional message to display when action is executed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Helper: Get string parameter value
    /// </summary>
    public string? GetString(string key)
    {
        return Parameters.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// Helper: Get int parameter value
    /// </summary>
    public int GetInt(string key, int defaultValue = 0)
    {
        if (Parameters.TryGetValue(key, out var value))
        {
            if (value is int intValue)
                return intValue;
            if (int.TryParse(value?.ToString(), out var parsed))
                return parsed;
        }
        return defaultValue;
    }

    /// <summary>
    /// Helper: Get bool parameter value
    /// </summary>
    public bool GetBool(string key, bool defaultValue = false)
    {
        if (Parameters.TryGetValue(key, out var value))
        {
            if (value is bool boolValue)
                return boolValue;
            if (bool.TryParse(value?.ToString(), out var parsed))
                return parsed;
        }
        return defaultValue;
    }

    /// <summary>
    /// Checks if this action has a condition that needs evaluation
    /// </summary>
    public bool HasCondition => !string.IsNullOrEmpty(Condition);
}
