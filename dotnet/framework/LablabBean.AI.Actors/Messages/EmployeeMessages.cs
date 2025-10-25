namespace LablabBean.AI.Actors.Messages;

/// <summary>
/// Messages specific to Employee actor operations
/// </summary>

// Task Management
public sealed record AssignTask(
    string TaskId,
    string TaskType,
    int Priority,
    Dictionary<string, object> Parameters,
    DateTime? Deadline = null
);

public sealed record TaskStarted(
    string TaskId,
    string EmployeeId,
    DateTime StartTime,
    float EstimatedDuration
);

public sealed record TaskCompleted(
    string TaskId,
    string EmployeeId,
    DateTime CompletionTime,
    float QualityScore,
    Dictionary<string, object> Results
);

public sealed record TaskFailed(
    string TaskId,
    string EmployeeId,
    string Reason,
    DateTime FailureTime
);

// Customer Interaction
public sealed record ServeCustomer(
    string CustomerId,
    string OrderId,
    List<string> OrderItems,
    string CustomerMood
);

public sealed record CustomerServed(
    string CustomerId,
    string EmployeeId,
    float SatisfactionScore,
    float ServiceTime,
    Dictionary<string, object> Feedback
);

// Skill Development
public sealed record PracticeSkill(
    string SkillName,
    int Duration
);

public sealed record SkillImproved(
    string EmployeeId,
    string SkillName,
    float OldLevel,
    float NewLevel,
    DateTime ImprovedAt
);

public sealed record ReceiveTraining(
    string SkillName,
    string TrainerId,
    int Duration
);

// Employee State Management
public sealed record UpdateEmployeeState(
    float? Energy = null,
    float? Stress = null,
    float? Motivation = null,
    string? CurrentEmotion = null,
    Dictionary<string, float>? StatusEffects = null
);

public sealed record GetEmployeeState;

public sealed record EmployeeStateResponse(
    string EmployeeId,
    float Energy,
    float Stress,
    float Motivation,
    string CurrentEmotion,
    Dictionary<string, float> Skills,
    Dictionary<string, float> StatusEffects,
    DateTime LastUpdate
);

// Feedback and Recognition
public sealed record ReceiveFeedback(
    string FromEntityId,
    string FeedbackType, // "praise", "criticism", "suggestion"
    string Message,
    float ImpactIntensity
);

public sealed record FeedbackProcessed(
    string EmployeeId,
    string FeedbackType,
    float MotivationDelta,
    float ConfidenceDelta,
    string EmotionalResponse
);

// Break Management
public sealed record TakeBreak(
    string BreakType, // "short", "meal", "emergency"
    int Duration
);

public sealed record BreakCompleted(
    string EmployeeId,
    float EnergyRestored,
    float StressReduced,
    DateTime CompletionTime
);

// Interaction with Others
public sealed record InteractWithPeer(
    string PeerEmployeeId,
    string InteractionType, // "collaborate", "chat", "help"
    string? Context = null
);

public sealed record InteractWithBoss(
    string BossId,
    string InteractionType, // "report", "ask", "discuss"
    string Topic
);

public sealed record PeerInteractionResult(
    string EmployeeId,
    string PeerId,
    string Outcome,
    float RelationshipDelta,
    Dictionary<string, object> Details
);

// Performance Queries
public sealed record GetPerformanceMetrics;

public sealed record PerformanceMetricsResponse(
    string EmployeeId,
    float OverallEfficiency,
    float CustomerSatisfaction,
    float TaskAccuracy,
    int TasksCompleted,
    int MistakesMade,
    Dictionary<string, float> SkillLevels,
    DateTime PeriodStart,
    DateTime PeriodEnd
);

// Shift Management
public sealed record StartShift(
    string ShiftType, // "morning", "afternoon", "evening"
    DateTime StartTime,
    DateTime EndTime
);

public sealed record EndShift(
    DateTime ActualEndTime
);

public sealed record ShiftStatus(
    string EmployeeId,
    bool OnShift,
    string? ShiftType,
    DateTime? ShiftStart,
    float TimeWorked,
    int TasksCompleted
);

// Error Handling
public sealed record ReportMistake(
    string TaskId,
    string MistakeType,
    string Description,
    bool RequiresCorrection
);

public sealed record MistakeResolved(
    string EmployeeId,
    string TaskId,
    string Resolution,
    float TimeToResolve,
    bool LessonLearned
);

// Personality Updates
public sealed record GetEmployeePersonality;

public sealed record EmployeePersonalityResponse(
    string EmployeeId,
    string PersonalityName,
    Dictionary<string, float> CurrentTraits,
    Dictionary<string, float> CurrentSkills,
    string ActivePersonalityProfile
);

public sealed record AdjustEmployeeTraits(
    Dictionary<string, float>? TraitAdjustments = null,
    string? Reason = null
);
