namespace LablabBean.AI.Core.Models;

/// <summary>
/// Employee personality configuration loaded from YAML
/// </summary>
public sealed class EmployeePersonality
{
    public string Name { get; init; } = "Employee";
    public string Version { get; init; } = "1.0.0";
    public string AvatarType { get; init; } = "employee";

    public EmployeeTraits Traits { get; init; } = new();
    public EmployeeBehavior Behavior { get; init; } = new();
    public EmployeeSkills Skills { get; init; } = new();
    public MemoryConfiguration Memory { get; init; } = new();
    public DialogueStyle Dialogue { get; init; } = new();
    public WorkPreferences Preferences { get; init; } = new();
    public GrowthParameters Growth { get; init; } = new();
    public EmployeeRelationshipDynamics Relationships { get; init; } = new();
    public PerformanceFactors Performance { get; init; } = new();
    public SystemPrompts Prompts { get; init; } = new();
    public EmotionalStates Emotions { get; init; } = new();
    public Dictionary<string, Dictionary<string, List<string>>> ResponseTemplates { get; init; } = new();
    public Dictionary<string, TaskModifier> TaskModifiers { get; init; } = new();
    public Dictionary<string, LearningCurve> Learning { get; init; } = new();
    public PersonalityMetadata Metadata { get; init; } = new();
}

public sealed class EmployeeTraits
{
    public float Diligence { get; init; } = 0.70f;
    public float Friendliness { get; init; } = 0.75f;
    public float Adaptability { get; init; } = 0.65f;
    public float Creativity { get; init; } = 0.60f;
    public float Teamwork { get; init; } = 0.70f;
    public float AttentionToDetail { get; init; } = 0.65f;
    public float Enthusiasm { get; init; } = 0.70f;
    public float Resilience { get; init; } = 0.60f;
}

public sealed class EmployeeBehavior
{
    public float TaskCompletionSpeed { get; init; } = 0.70f;
    public float InitiativeLevel { get; init; } = 0.60f;
    public float CustomerFocus { get; init; } = 0.75f;
    public float LearningRate { get; init; } = 0.65f;
    public float StressTolerance { get; init; } = 0.60f;
    public float MistakeRecovery { get; init; } = 0.70f;
}

public sealed class EmployeeSkills
{
    public float CoffeeMaking { get; set; } = 0.60f;
    public float CustomerService { get; set; } = 0.70f;
    public float CashHandling { get; set; } = 0.65f;
    public float Cleaning { get; set; } = 0.70f;
    public float Multitasking { get; set; } = 0.60f;
    public float ProblemSolving { get; set; } = 0.55f;
}

public sealed class WorkPreferences
{
    public List<string> PreferredTasks { get; init; } = new();
    public List<string> DislikedTasks { get; init; } = new();
    public string PreferredShift { get; init; } = "morning";
    public float TeamWorkPreference { get; init; } = 0.70f;
}

public sealed class GrowthParameters
{
    public float SkillImprovementRate { get; init; } = 0.02f;
    public float ConfidenceBuildRate { get; init; } = 0.015f;
    public float ConfidenceDecayRate { get; init; } = 0.010f;
    public float BurnoutThreshold { get; init; } = 0.75f;
    public float MotivationBaseline { get; init; } = 0.70f;
}

public sealed class PerformanceFactors
{
    public float BaseEfficiency { get; init; } = 0.70f;
    public float QualityFocus { get; init; } = 0.65f;
    public float Consistency { get; init; } = 0.60f;
    public float PeakHoursBoost { get; init; } = 0.10f;
    public float FatigueImpact { get; init; } = 0.30f;
}

public sealed class TaskModifier
{
    public float BaseTime { get; init; }
    public float QualityBonus { get; init; }
    public float EnergyImpact { get; init; }
    public float BaseSatisfaction { get; init; }
    public float FriendlinessBonus { get; init; }
    public float StressPenalty { get; init; }
    public float DetailBonus { get; init; }
    public float FatiguePenalty { get; init; }
}

public sealed class LearningCurve
{
    public float PlateauLevel { get; init; }
    public int PracticeRequired { get; init; }
}

public sealed class EmployeeRelationshipDynamics
{
    public float BossRespect { get; init; } = 0.65f;
    public float PeerFriendliness { get; init; } = 0.70f;
    public float CustomerWarmth { get; init; } = 0.75f;
    public float ConflictTolerance { get; init; } = 0.50f;
    public float FeedbackReceptivity { get; init; } = 0.70f;
}
