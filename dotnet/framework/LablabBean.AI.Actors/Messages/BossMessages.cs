namespace LablabBean.AI.Actors.Messages;

/// <summary>
/// Messages specific to Boss actor operations
/// </summary>

// Boss Decision Making
public sealed record MakeBossDecision(
    string Context,
    Dictionary<string, object> Parameters,
    string? EmployeeId = null
);

public sealed record BossDecisionMade(
    string Decision,
    string Reasoning,
    float Confidence,
    Dictionary<string, object> Metadata
);

// Boss Dialogue
public sealed record InitiateBossDialogue(
    string EmployeeId,
    string Topic,
    string? Context = null
);

public sealed record BossDialogueResponse(
    string EmployeeId,
    string Response,
    string Emotion,
    float RelationshipDelta
);

// Boss State Management
public sealed record UpdateBossState(
    float? StressLevel = null,
    float? FatigueLevel = null,
    string? CurrentEmotion = null,
    Dictionary<string, float>? Modifiers = null
);

public sealed record GetBossState;

public sealed record BossStateResponse(
    string CurrentEmotion,
    float StressLevel,
    float FatigueLevel,
    Dictionary<string, float> ActiveModifiers,
    DateTime LastUpdate
);

// Employee Relationship Management
public sealed record UpdateEmployeeRelationship(
    string EmployeeId,
    float TrustDelta,
    float RespectDelta,
    string? Reason = null
);

public sealed record GetEmployeeRelationship(string EmployeeId);

public sealed record EmployeeRelationshipResponse(
    string EmployeeId,
    float TrustLevel,
    float RespectLevel,
    int InteractionCount,
    DateTime LastInteraction
);

// Boss Memory Operations
public sealed record AddBossMemory(
    string Content,
    string Category,
    float EmotionalIntensity,
    Dictionary<string, object>? Metadata = null
);

public sealed record QueryBossMemory(
    string? Category = null,
    DateTime? Since = null,
    int MaxResults = 10
);

public sealed record BossMemoryResponse(
    List<MemoryEntry> Memories,
    int TotalCount
);

public sealed record MemoryEntry(
    string Content,
    string Category,
    float EmotionalIntensity,
    DateTime Timestamp,
    Dictionary<string, object> Metadata
);

// Boss Performance Evaluation
public sealed record EvaluateEmployeePerformance(
    string EmployeeId,
    Dictionary<string, float> Metrics,
    string Period
);

public sealed record PerformanceEvaluationResult(
    string EmployeeId,
    float OverallScore,
    string Feedback,
    List<string> Strengths,
    List<string> ImprovementAreas,
    Dictionary<string, float> DetailedScores
);

// Boss Task Delegation
public sealed record DelegateTask(
    string TaskId,
    string TaskDescription,
    string? PreferredEmployeeId = null,
    int Priority = 1
);

public sealed record TaskDelegated(
    string TaskId,
    string EmployeeId,
    string Reasoning,
    DateTime DueDate
);

// Boss Mood and Personality
public sealed record GetBossPersonality;

public sealed record BossPersonalityResponse(
    string PersonalityName,
    Dictionary<string, float> CurrentTraits,
    Dictionary<string, float> CurrentModifiers,
    string ActivePersonalityProfile
);

public sealed record AdjustBossPersonality(
    Dictionary<string, float>? TraitAdjustments = null,
    Dictionary<string, float>? ModifierAdjustments = null,
    string? Reason = null
);
