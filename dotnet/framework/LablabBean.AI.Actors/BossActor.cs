using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using LablabBean.AI.Actors.Messages;
using LablabBean.AI.Core.Events;
using LablabBean.AI.Core.Interfaces;
using LablabBean.AI.Core.Models;
using Models = LablabBean.AI.Core.Models;

namespace LablabBean.AI.Actors;

/// <summary>
/// Boss actor - manages boss AI state, personality, and decision-making
/// Implements persistence for state recovery
/// </summary>
public sealed class BossActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IActorRef _eventBusAdapter;
    private readonly IIntelligenceAgent? _intelligenceAgent;
    private readonly BossPersonality _personality;

    // Actor State
    private AvatarState _state;
    private AvatarMemory _memory;
    private Dictionary<string, AvatarRelationship> _relationships;
    private string _currentEmotion;
    private float _stressLevel;
    private float _fatigueLevel;
    private Dictionary<string, float> _activeModifiers;
    private int _snapshotInterval;

    public override string PersistenceId { get; }

    public BossActor(
        string entityId,
        BossPersonality personality,
        IActorRef eventBusAdapter,
        IIntelligenceAgent? intelligenceAgent = null,
        int snapshotInterval = 10)
    {
        PersistenceId = $"boss-{entityId}";
        _personality = personality;
        _eventBusAdapter = eventBusAdapter;
        _intelligenceAgent = intelligenceAgent;
        _snapshotInterval = snapshotInterval;

        // Initialize state
        _state = new AvatarState
        {
            EntityId = entityId,
            CurrentBehavior = "Idle",
            EmotionalState = personality.Emotions.Default,
            Health = 100f,
            MaxHealth = 100f,
            LastUpdated = DateTime.UtcNow
        };

        _memory = new AvatarMemory
        {
            EntityId = entityId,
            MaxShortTermMemories = personality.Memory.ShortTermCapacity,
            MaxLongTermMemories = 50
        };

        _relationships = new();
        _currentEmotion = personality.Emotions.Default;
        _stressLevel = 0.0f;
        _fatigueLevel = 0.0f;
        _activeModifiers = new();

        // Commands
        Command<MakeBossDecision>(HandleMakeBossDecision);
        Command<InitiateBossDialogue>(HandleInitiateBossDialogue);
        Command<UpdateBossState>(HandleUpdateBossState);
        Command<GetBossState>(HandleGetBossState);
        Command<UpdateEmployeeRelationship>(HandleUpdateEmployeeRelationship);
        Command<GetEmployeeRelationship>(HandleGetEmployeeRelationship);
        Command<AddBossMemory>(HandleAddBossMemory);
        Command<QueryBossMemory>(HandleQueryBossMemory);
        Command<EvaluateEmployeePerformance>(HandleEvaluateEmployeePerformance);
        Command<DelegateTask>(HandleDelegateTask);
        Command<GetBossPersonality>(HandleGetBossPersonality);
        Command<AdjustBossPersonality>(HandleAdjustBossPersonality);
        Command<SaveSnapshotSuccess>(HandleSaveSnapshotSuccess);
        Command<SaveSnapshotFailure>(HandleSaveSnapshotFailure);

        // Recovery
        Recover<SnapshotOffer>(HandleSnapshotOffer);
        Recover<BossStateUpdated>(HandleBossStateUpdated);
        Recover<EmployeeRelationshipUpdated>(HandleEmployeeRelationshipUpdated);
        Recover<BossMemoryAdded>(HandleBossMemoryAdded);
    }

    protected override void PreStart()
    {
        base.PreStart();
        _log.Info($"BossActor starting: {PersistenceId}");
    }

    protected override void PostStop()
    {
        _log.Info($"BossActor stopped: {PersistenceId}");

        // Publish stop event
        _eventBusAdapter.Tell(new ActorStoppedEvent
        {
            EntityId = _state.EntityId,
            ActorPath = Self.Path.ToString(),
            Reason = "Normal shutdown",
            Timestamp = DateTime.UtcNow
        });

        base.PostStop();
    }

    #region Command Handlers

    private void HandleMakeBossDecision(MakeBossDecision msg)
    {
        _log.Debug($"Making decision for context: {msg.Context}");

        if (_intelligenceAgent == null)
        {
            Sender.Tell(new BossDecisionMade(
                "No decision agent available",
                "Intelligence agent not configured",
                0.0f,
                new()
            ));
            return;
        }

        // Build context for decision
        var context = new AvatarContext
        {
            EntityId = _state.EntityId,
            Name = _personality.Name,
            PersonalityProfile = _personality.AvatarType,
            CurrentState = new Dictionary<string, object>
            {
                ["emotion"] = _currentEmotion,
                ["stress"] = _stressLevel,
                ["fatigue"] = _fatigueLevel,
                ["behavior"] = _state.CurrentBehavior
            },
            EnvironmentFactors = new Dictionary<string, float>(),
            NearbyEntities = msg.EmployeeId != null ? new List<string> { msg.EmployeeId } : new()
        };

        // Copy parameters to environment factors
        foreach (var (key, value) in msg.Parameters)
        {
            if (value is float floatValue)
                context.EnvironmentFactors[key] = floatValue;
            else if (value is int intValue)
                context.EnvironmentFactors[key] = intValue;
            else if (value is double doubleValue)
                context.EnvironmentFactors[key] = (float)doubleValue;
        }

        // TODO: Async decision making via intelligence agent
        // For now, return a placeholder
        var decision = new BossDecisionMade(
            Decision: "Decision pending",
            Reasoning: "Awaiting intelligence agent processing",
            Confidence: 0.5f,
            Metadata: new Dictionary<string, object>
            {
                ["context"] = msg.Context,
                ["emotion"] = _currentEmotion,
                ["stress"] = _stressLevel
            }
        );

        Sender.Tell(decision);

        // Publish thought event
        _eventBusAdapter.Tell(new AIThoughtEvent
        {
            EntityId = _state.EntityId,
            Thought = $"Considering: {msg.Context}",
            Category = "decision_making",
            Metadata = msg.Parameters,
            Timestamp = DateTime.UtcNow
        });
    }

    private void HandleInitiateBossDialogue(InitiateBossDialogue msg)
    {
        _log.Debug($"Initiating dialogue with {msg.EmployeeId} about {msg.Topic}");

        // Get or create relationship
        if (!_relationships.TryGetValue(msg.EmployeeId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.EmployeeId,
                Affinity = 0f,
                RelationshipType = "Neutral",
                LastInteraction = DateTime.UtcNow
            };
            _relationships[msg.EmployeeId] = relationship;
        }

        // Select response based on relationship and personality
        var response = GenerateDialogueResponse(msg.EmployeeId, msg.Topic, msg.Context, relationship);

        Sender.Tell(response);

        // Update relationship
        relationship.RecordInteraction($"Discussed: {msg.Topic}");

        PersistEvent(new EmployeeRelationshipUpdated(msg.EmployeeId, relationship));

        // Publish dialogue event
        _eventBusAdapter.Tell(new NPCDialogueEvent
        {
            EntityId = _state.EntityId,
            TargetEntityId = msg.EmployeeId,
            DialogueText = response.Response,
            EmotionalTone = response.Emotion,
            Metadata = new Dictionary<string, object>
            {
                ["context"] = msg.Context ?? string.Empty,
                ["topic"] = msg.Topic
            },
            Timestamp = DateTime.UtcNow
        });
    }

    private void HandleUpdateBossState(UpdateBossState msg)
    {
        var updated = false;

        if (msg.StressLevel.HasValue)
        {
            _stressLevel = Math.Clamp(msg.StressLevel.Value, 0f, 1f);
            updated = true;
        }

        if (msg.FatigueLevel.HasValue)
        {
            _fatigueLevel = Math.Clamp(msg.FatigueLevel.Value, 0f, 1f);
            updated = true;
        }

        if (msg.CurrentEmotion != null && _personality.Emotions.Available.Contains(msg.CurrentEmotion))
        {
            _currentEmotion = msg.CurrentEmotion;
            updated = true;
        }

        if (msg.Modifiers != null)
        {
            foreach (var (key, value) in msg.Modifiers)
            {
                _activeModifiers[key] = value;
            }
            updated = true;
        }

        if (updated)
        {
            _state.LastUpdated = DateTime.UtcNow;
            PersistEvent(new BossStateUpdated(_stressLevel, _fatigueLevel, _currentEmotion, _activeModifiers));

            // Publish behavior change if significant
            if (msg.CurrentEmotion != null)
            {
                _eventBusAdapter.Tell(new AIBehaviorChangedEvent
                {
                    EntityId = _state.EntityId,
                    PreviousBehavior = _state.CurrentBehavior,
                    NewBehavior = _currentEmotion,
                    Reason = "Emotional state changed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        Sender.Tell(new StatusReply(updated));
    }

    private void HandleGetBossState(GetBossState msg)
    {
        var response = new BossStateResponse(
            _currentEmotion,
            _stressLevel,
            _fatigueLevel,
            new Dictionary<string, float>(_activeModifiers),
            _state.LastUpdated
        );

        Sender.Tell(response);
    }

    private void HandleUpdateEmployeeRelationship(UpdateEmployeeRelationship msg)
    {
        if (!_relationships.TryGetValue(msg.EmployeeId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.EmployeeId,
                Affinity = 0f,
                RelationshipType = "Neutral",
                LastInteraction = DateTime.UtcNow
            };
        }

        // Convert trust/respect deltas to affinity (scale from -1/+1 to -100/+100)
        var affinityDelta = (msg.TrustDelta + msg.RespectDelta) * 50f;
        relationship.AdjustAffinity(affinityDelta);

        if (!string.IsNullOrEmpty(msg.Reason))
        {
            relationship.RecordInteraction(msg.Reason);
        }

        _relationships[msg.EmployeeId] = relationship;

        PersistEvent(new EmployeeRelationshipUpdated(msg.EmployeeId, relationship));

        Sender.Tell(new StatusReply(true));
    }

    private void HandleGetEmployeeRelationship(GetEmployeeRelationship msg)
    {
        if (_relationships.TryGetValue(msg.EmployeeId, out var relationship))
        {
            // Convert affinity back to trust/respect scale (0-1)
            var trustLevel = Math.Clamp((relationship.Affinity + 100f) / 200f, 0f, 1f);
            var respectLevel = Math.Clamp((relationship.Affinity + 100f) / 200f, 0f, 1f);
            var interactionCount = relationship.SharedHistory.Count;

            var response = new EmployeeRelationshipResponse(
                msg.EmployeeId,
                trustLevel,
                respectLevel,
                interactionCount,
                relationship.LastInteraction
            );
            Sender.Tell(response);
        }
        else
        {
            Sender.Tell(new EmployeeRelationshipResponse(
                msg.EmployeeId,
                0.5f,
                0.5f,
                0,
                DateTime.MinValue
            ));
        }
    }

    private void HandleAddBossMemory(AddBossMemory msg)
    {
        var memoryEntry = new Models.MemoryEntry
        {
            EventType = msg.Category,
            Description = msg.Content,
            Timestamp = DateTime.UtcNow,
            Importance = msg.EmotionalIntensity,
            Metadata = msg.Metadata ?? new()
        };

        _memory.AddMemory(memoryEntry);

        PersistEvent(new BossMemoryAdded(msg.Content, msg.Category, msg.EmotionalIntensity, memoryEntry.Timestamp));

        Sender.Tell(new StatusReply(true));
    }

    private void HandleQueryBossMemory(QueryBossMemory msg)
    {
        var allMemories = _memory.LongTermMemory.Concat(_memory.ShortTermMemory);

        if (msg.Category != null)
        {
            allMemories = allMemories.Where(m => m.EventType == msg.Category);
        }

        if (msg.Since != null)
        {
            allMemories = allMemories.Where(m => m.Timestamp >= msg.Since);
        }

        var memories = allMemories
            .OrderByDescending(m => m.Timestamp)
            .Take(msg.MaxResults)
            .Select(m => new Messages.MemoryEntry(
                m.Description,
                m.EventType,
                m.Importance,
                m.Timestamp,
                m.Metadata
            ))
            .ToList();

        Sender.Tell(new BossMemoryResponse(memories, memories.Count));
    }

    private void HandleEvaluateEmployeePerformance(EvaluateEmployeePerformance msg)
    {
        // Calculate weighted score based on personality priorities
        var overallScore = CalculatePerformanceScore(msg.Metrics);

        // Generate feedback based on personality traits
        var (strengths, improvements) = AnalyzePerformance(msg.Metrics);

        var result = new PerformanceEvaluationResult(
            msg.EmployeeId,
            overallScore,
            GeneratePerformanceFeedback(msg.EmployeeId, overallScore, strengths, improvements),
            strengths,
            improvements,
            msg.Metrics
        );

        Sender.Tell(result);

        // Update relationship based on performance
        var relationshipDelta = (overallScore - 0.5f) * 0.1f; // -0.05 to +0.05
        Self.Tell(new UpdateEmployeeRelationship(msg.EmployeeId, relationshipDelta, relationshipDelta, "Performance evaluation"));
    }

    private void HandleDelegateTask(DelegateTask msg)
    {
        // Simple delegation logic - prefer specified employee or best relationship
        var selectedEmployee = msg.PreferredEmployeeId ?? _relationships
            .OrderByDescending(r => r.Value.Affinity)
            .FirstOrDefault().Key ?? "employee-1";

        var reasoning = msg.PreferredEmployeeId != null
            ? "Delegated to preferred employee"
            : "Delegated to employee with highest affinity";

        var dueDate = DateTime.UtcNow.AddHours(msg.Priority switch
        {
            1 => 24,
            2 => 12,
            3 => 6,
            _ => 24
        });

        var result = new TaskDelegated(msg.TaskId, selectedEmployee, reasoning, dueDate);

        Sender.Tell(result);

        // Add memory of delegation
        Self.Tell(new AddBossMemory(
            $"Delegated task {msg.TaskId} to {selectedEmployee}",
            "task_delegation",
            0.4f,
            new Dictionary<string, object>
            {
                ["task_id"] = msg.TaskId,
                ["employee_id"] = selectedEmployee,
                ["priority"] = msg.Priority
            }
        ));
    }

    private void HandleGetBossPersonality(GetBossPersonality msg)
    {
        var currentTraits = new Dictionary<string, float>
        {
            ["leadership"] = _personality.Traits.Leadership,
            ["strictness"] = _personality.Traits.Strictness,
            ["fairness"] = _personality.Traits.Fairness,
            ["empathy"] = _personality.Traits.Empathy,
            ["efficiency"] = _personality.Traits.Efficiency
        };

        var response = new BossPersonalityResponse(
            _personality.Name,
            currentTraits,
            new Dictionary<string, float>(_activeModifiers),
            _personality.AvatarType
        );

        Sender.Tell(response);
    }

    private void HandleAdjustBossPersonality(AdjustBossPersonality msg)
    {
        if (msg.ModifierAdjustments != null)
        {
            foreach (var (key, value) in msg.ModifierAdjustments)
            {
                _activeModifiers[key] = Math.Clamp(value, -1f, 1f);
            }
        }

        _state.LastUpdated = DateTime.UtcNow;
        PersistEvent(new BossStateUpdated(_stressLevel, _fatigueLevel, _currentEmotion, _activeModifiers));

        Sender.Tell(new StatusReply(true));
    }

    #endregion

    #region Recovery Handlers

    private void HandleSnapshotOffer(SnapshotOffer offer)
    {
        if (offer.Snapshot is BossActorSnapshot snapshot)
        {
            _state = snapshot.State;
            _memory = snapshot.Memory;
            _relationships = snapshot.Relationships;
            _currentEmotion = snapshot.CurrentEmotion;
            _stressLevel = snapshot.StressLevel;
            _fatigueLevel = snapshot.FatigueLevel;
            _activeModifiers = snapshot.ActiveModifiers;

            _log.Info($"Recovered from snapshot at sequence {offer.Metadata.SequenceNr}");
        }
    }

    private void HandleBossStateUpdated(BossStateUpdated evt)
    {
        _stressLevel = evt.StressLevel;
        _fatigueLevel = evt.FatigueLevel;
        _currentEmotion = evt.CurrentEmotion;
        _activeModifiers = evt.ActiveModifiers;
        _state.LastUpdated = DateTime.UtcNow;
    }

    private void HandleEmployeeRelationshipUpdated(EmployeeRelationshipUpdated evt)
    {
        _relationships[evt.EmployeeId] = evt.Relationship;
    }

    private void HandleBossMemoryAdded(BossMemoryAdded evt)
    {
        var memoryEntry = new Models.MemoryEntry
        {
            EventType = evt.Category,
            Description = evt.Content,
            Timestamp = evt.Timestamp,
            Importance = evt.EmotionalIntensity,
            Metadata = new Dictionary<string, object> { ["category"] = evt.Category }
        };

        _memory.AddMemory(memoryEntry);
    }

    private void HandleSaveSnapshotSuccess(SaveSnapshotSuccess msg)
    {
        _log.Debug($"Snapshot saved successfully at sequence {msg.Metadata.SequenceNr}");
    }

    private void HandleSaveSnapshotFailure(SaveSnapshotFailure msg)
    {
        _log.Warning($"Snapshot save failed: {msg.Cause.Message}");
    }

    #endregion

    #region Helper Methods

    private void PersistEvent<T>(T evt) where T : class
    {
        Persist(evt, _ =>
        {
            if (LastSequenceNr % _snapshotInterval == 0 && LastSequenceNr != 0)
            {
                SaveSnapshot(new BossActorSnapshot(
                    _state,
                    _memory,
                    _relationships,
                    _currentEmotion,
                    _stressLevel,
                    _fatigueLevel,
                    _activeModifiers
                ));
            }
        });
    }

    private BossDialogueResponse GenerateDialogueResponse(
        string employeeId,
        string topic,
        string? context,
        AvatarRelationship relationship)
    {
        // Simple template-based response for MVP
        // Convert affinity to trust level for template selection
        var trustLevel = Math.Clamp((relationship.Affinity + 100f) / 200f, 0f, 1f);
        var templates = GetResponseTemplates(trustLevel);
        var response = templates.FirstOrDefault() ?? $"Let's discuss {topic}.";

        var relationshipDelta = _personality.Relationships.TrustBuildRate * 0.01f;

        return new BossDialogueResponse(
            employeeId,
            response.Replace("{name}", employeeId).Replace("{task}", topic),
            _currentEmotion,
            relationshipDelta
        );
    }

    private List<string> GetResponseTemplates(float trustLevel)
    {
        var category = trustLevel > 0.7f ? "high_relationship" : "low_relationship";

        if (_personality.ResponseTemplates.TryGetValue("greeting", out var greetings) &&
            greetings.TryGetValue(category, out var templates))
        {
            return templates;
        }

        return new List<string> { "Hello. Let's get to work." };
    }

    private float CalculatePerformanceScore(Dictionary<string, float> metrics)
    {
        if (metrics.Count == 0) return 0.5f;

        var avgScore = metrics.Values.Average();

        // Weight by personality priorities
        var weighted = avgScore * _personality.Priorities.BusinessGoals +
                      avgScore * _personality.Priorities.Efficiency;

        return Math.Clamp(weighted, 0f, 1f);
    }

    private (List<string> strengths, List<string> improvements) AnalyzePerformance(Dictionary<string, float> metrics)
    {
        var strengths = new List<string>();
        var improvements = new List<string>();

        foreach (var (metric, score) in metrics)
        {
            if (score >= 0.7f)
                strengths.Add(metric);
            else if (score < 0.5f)
                improvements.Add(metric);
        }

        return (strengths, improvements);
    }

    private string GeneratePerformanceFeedback(
        string employeeId,
        float score,
        List<string> strengths,
        List<string> improvements)
    {
        var feedbackLevel = score switch
        {
            >= 0.8f => "Excellent work",
            >= 0.6f => "Good performance",
            >= 0.4f => "Acceptable, but needs improvement",
            _ => "Performance below expectations"
        };

        var feedback = $"{feedbackLevel}. ";

        if (strengths.Any())
            feedback += $"Strengths: {string.Join(", ", strengths)}. ";

        if (improvements.Any())
            feedback += $"Areas to improve: {string.Join(", ", improvements)}.";

        return feedback;
    }

    #endregion

    #region Persistence Events

    private sealed record BossStateUpdated(
        float StressLevel,
        float FatigueLevel,
        string CurrentEmotion,
        Dictionary<string, float> ActiveModifiers
    );

    private sealed record EmployeeRelationshipUpdated(
        string EmployeeId,
        AvatarRelationship Relationship
    );

    private sealed record BossMemoryAdded(
        string Content,
        string Category,
        float EmotionalIntensity,
        DateTime Timestamp
    );

    #endregion
}

/// <summary>
/// Snapshot for BossActor state persistence
/// </summary>
public sealed record BossActorSnapshot(
    AvatarState State,
    AvatarMemory Memory,
    Dictionary<string, AvatarRelationship> Relationships,
    string CurrentEmotion,
    float StressLevel,
    float FatigueLevel,
    Dictionary<string, float> ActiveModifiers
);

/// <summary>
/// Simple status reply for commands
/// </summary>
public sealed record StatusReply(bool Success, string? Message = null);
