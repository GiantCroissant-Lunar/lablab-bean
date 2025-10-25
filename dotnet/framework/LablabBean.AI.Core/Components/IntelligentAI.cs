namespace LablabBean.AI.Core.Components;

/// <summary>
/// AI capability flags for avatar behaviors
/// </summary>
[Flags]
public enum AICapability
{
    None = 0,
    Dialogue = 1 << 0,
    Memory = 1 << 1,
    EmotionalState = 1 << 2,
    TacticalAdaptation = 1 << 3,
    QuestGeneration = 1 << 4,
    PersonalityDriven = 1 << 5
}

/// <summary>
/// ECS component that marks an entity as having intelligent AI
/// </summary>
public struct IntelligentAI
{
    public AICapability Capabilities { get; set; }
    public float DecisionCooldown { get; set; }
    public float TimeSinceLastDecision { get; set; }

    public IntelligentAI(AICapability capabilities, float decisionCooldown = 1.0f)
    {
        Capabilities = capabilities;
        DecisionCooldown = decisionCooldown;
        TimeSinceLastDecision = 0f;
    }

    public bool HasCapability(AICapability capability)
    {
        return (Capabilities & capability) == capability;
    }
}
