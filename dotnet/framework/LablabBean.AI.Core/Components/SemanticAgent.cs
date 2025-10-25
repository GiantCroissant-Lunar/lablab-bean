namespace LablabBean.AI.Core.Components;

/// <summary>
/// Agent types for Semantic Kernel agents
/// </summary>
public enum AgentType
{
    BossIntelligence,
    NpcDialogue,
    EnemyTactics,
    QuestGiver
}

/// <summary>
/// ECS component that holds a reference to a Semantic Kernel agent
/// </summary>
public struct SemanticAgent
{
    public string AgentId { get; set; }
    public AgentType AgentType { get; set; }
    public bool IsInitialized { get; set; }

    public SemanticAgent(string agentId, AgentType agentType)
    {
        AgentId = agentId;
        AgentType = agentType;
        IsInitialized = false;
    }
}
