namespace LablabBean.AI.Core.Models;

/// <summary>
/// Boss personality configuration loaded from YAML
/// </summary>
public sealed class BossPersonality
{
    public string Name { get; init; } = "Boss";
    public string Version { get; init; } = "1.0.0";
    public string AvatarType { get; init; } = "boss";

    public PersonalityTraits Traits { get; init; } = new();
    public BehaviorParameters Behavior { get; init; } = new();
    public MemoryConfiguration Memory { get; init; } = new();
    public DialogueStyle Dialogue { get; init; } = new();
    public RelationshipDynamics Relationships { get; init; } = new();
    public DecisionPriorities Priorities { get; init; } = new();
    public ContextualModifiers Modifiers { get; init; } = new();
    public SystemPrompts Prompts { get; init; } = new();
    public EmotionalStates Emotions { get; init; } = new();
    public Dictionary<string, Dictionary<string, List<string>>> ResponseTemplates { get; init; } = new();
    public PersonalityMetadata Metadata { get; init; } = new();
}

public sealed class PersonalityTraits
{
    public float Leadership { get; init; } = 0.85f;
    public float Strictness { get; init; } = 0.65f;
    public float Fairness { get; init; } = 0.75f;
    public float Empathy { get; init; } = 0.55f;
    public float Efficiency { get; init; } = 0.80f;
    public float Humor { get; init; } = 0.45f;
    public float Patience { get; init; } = 0.60f;
    public float Innovation { get; init; } = 0.70f;
}

public sealed class BehaviorParameters
{
    public float DecisionSpeed { get; init; } = 0.70f;
    public float RiskTolerance { get; init; } = 0.45f;
    public float Delegation { get; init; } = 0.75f;
    public float Micromanagement { get; init; } = 0.35f;
    public float PraiseFrequency { get; init; } = 0.60f;
    public float CriticismDirectness { get; init; } = 0.70f;
}

public sealed class MemoryConfiguration
{
    public int ShortTermCapacity { get; init; } = 10;
    public float LongTermPriority { get; init; } = 0.75f;
    public float EmotionalWeight { get; init; } = 0.65f;
}

public sealed class DialogueStyle
{
    public float Formality { get; init; } = 0.70f;
    public float Verbosity { get; init; } = 0.60f;
    public float Positivity { get; init; } = 0.65f;
    public float Directness { get; init; } = 0.75f;
}

public sealed class RelationshipDynamics
{
    public float TrustBuildRate { get; init; } = 0.50f;
    public float TrustDecayRate { get; init; } = 0.30f;
    public float AuthorityImportance { get; init; } = 0.80f;
    public float TeamBonding { get; init; } = 0.65f;
}

public sealed class DecisionPriorities
{
    public float BusinessGoals { get; init; } = 0.35f;
    public float EmployeeWellbeing { get; init; } = 0.25f;
    public float Efficiency { get; init; } = 0.25f;
    public float Innovation { get; init; } = 0.15f;
}

public sealed class ContextualModifiers
{
    public float StressThreshold { get; init; } = 0.70f;
    public float FatigueImpact { get; init; } = 0.50f;
    public float SuccessBoost { get; init; } = 0.20f;
    public float FailureImpact { get; init; } = 0.30f;
}

public sealed class SystemPrompts
{
    public string SystemPrompt { get; init; } = string.Empty;
    public string DecisionTemplate { get; init; } = string.Empty;
    public string DialogueTemplate { get; init; } = string.Empty;
}

public sealed class EmotionalStates
{
    public string Default { get; init; } = "neutral";
    public List<string> Available { get; init; } = new();
    public Dictionary<string, List<string>> Triggers { get; init; } = new();
}

public sealed class PersonalityMetadata
{
    public string Author { get; init; } = string.Empty;
    public string Created { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = new();
    public string Description { get; init; } = string.Empty;
}
