using System.Collections.Generic;

namespace LablabBean.Plugins.NPC.Data;

/// <summary>
/// Represents a complete dialogue tree with multiple nodes and branching paths
/// </summary>
public class DialogueTree
{
    /// <summary>
    /// Unique identifier for this dialogue tree
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this dialogue tree (for debugging/editing)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ID of the starting node in the dialogue tree
    /// </summary>
    public string StartNodeId { get; set; } = string.Empty;

    /// <summary>
    /// All nodes in this dialogue tree, indexed by ID
    /// </summary>
    public Dictionary<string, DialogueNode> Nodes { get; set; } = new();

    /// <summary>
    /// Gets a node by its ID
    /// </summary>
    public DialogueNode? GetNode(string nodeId)
    {
        return Nodes.TryGetValue(nodeId, out var node) ? node : null;
    }

    /// <summary>
    /// Gets the starting node of the dialogue tree
    /// </summary>
    public DialogueNode? GetStartNode()
    {
        return GetNode(StartNodeId);
    }

    /// <summary>
    /// Validates that all node references are valid
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrEmpty(StartNodeId))
        {
            errors.Add("StartNodeId is not set");
        }
        else if (!Nodes.ContainsKey(StartNodeId))
        {
            errors.Add($"StartNodeId '{StartNodeId}' does not exist in Nodes");
        }

        foreach (var (nodeId, node) in Nodes)
        {
            if (string.IsNullOrEmpty(node.Id))
            {
                errors.Add($"Node has empty ID");
            }
            else if (node.Id != nodeId)
            {
                errors.Add($"Node ID mismatch: key='{nodeId}', node.Id='{node.Id}'");
            }

            foreach (var choice in node.Choices)
            {
                if (!string.IsNullOrEmpty(choice.NextNodeId) && !Nodes.ContainsKey(choice.NextNodeId))
                {
                    errors.Add($"Node '{nodeId}' has choice pointing to non-existent node '{choice.NextNodeId}'");
                }
            }
        }

        return errors.Count == 0;
    }
}
