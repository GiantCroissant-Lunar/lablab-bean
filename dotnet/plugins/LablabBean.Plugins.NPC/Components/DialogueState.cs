namespace LablabBean.Plugins.NPC.Components;

/// <summary>
/// Dialogue state component - tracks current dialogue state for an NPC interaction
/// </summary>
public struct DialogueState
{
    /// <summary>
    /// ID of the current dialogue tree
    /// </summary>
    public string DialogueTreeId { get; set; }

    /// <summary>
    /// ID of the current node in the dialogue tree
    /// </summary>
    public string CurrentNodeId { get; set; }

    /// <summary>
    /// Whether dialogue is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Entity ID of the player in dialogue
    /// </summary>
    public int PlayerEntityId { get; set; }

    /// <summary>
    /// Entity ID of the NPC in dialogue
    /// </summary>
    public int NPCEntityId { get; set; }

    public DialogueState(string dialogueTreeId, string currentNodeId, int playerEntityId, int npcEntityId)
    {
        DialogueTreeId = dialogueTreeId;
        CurrentNodeId = currentNodeId;
        IsActive = true;
        PlayerEntityId = playerEntityId;
        NPCEntityId = npcEntityId;
    }

    /// <summary>
    /// Ends the dialogue
    /// </summary>
    public void End()
    {
        IsActive = false;
    }

    /// <summary>
    /// Moves to a new dialogue node
    /// </summary>
    public void MoveToNode(string nodeId)
    {
        CurrentNodeId = nodeId;
    }
}
