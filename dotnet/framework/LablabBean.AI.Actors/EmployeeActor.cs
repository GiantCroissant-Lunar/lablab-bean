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
/// Employee actor - manages employee AI state, skills, and performance
/// Implements persistence for state recovery
/// </summary>
public sealed class EmployeeActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IActorRef _eventBusAdapter;
    private readonly IIntelligenceAgent? _intelligenceAgent;
    private readonly EmployeePersonality _personality;

    // Actor State
    private AvatarState _state;
    private AvatarMemory _memory;
    private EmployeeSkills _skills;
    private Dictionary<string, AvatarRelationship> _relationships;
    private Dictionary<string, int> _taskPracticeCount;
    private string _currentEmotion;
    private float _energy;
    private float _stress;
    private float _motivation;
    private float _confidence;
    private Dictionary<string, float> _statusEffects;
    private bool _onShift;
    private string? _currentTask;
    private DateTime? _shiftStart;
    private int _tasksCompletedToday;
    private int _mistakesToday;
    private int _snapshotInterval;

    public override string PersistenceId { get; }

    public EmployeeActor(
        string entityId,
        EmployeePersonality personality,
        IActorRef eventBusAdapter,
        IIntelligenceAgent? intelligenceAgent = null,
        int snapshotInterval = 10)
    {
        PersistenceId = $"employee-{entityId}";
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

        _skills = new EmployeeSkills
        {
            CoffeeMaking = personality.Skills.CoffeeMaking,
            CustomerService = personality.Skills.CustomerService,
            CashHandling = personality.Skills.CashHandling,
            Cleaning = personality.Skills.Cleaning,
            Multitasking = personality.Skills.Multitasking,
            ProblemSolving = personality.Skills.ProblemSolving
        };

        _relationships = new();
        _taskPracticeCount = new();
        _currentEmotion = personality.Emotions.Default;
        _energy = 1.0f;
        _stress = 0.0f;
        _motivation = personality.Growth.MotivationBaseline;
        _confidence = 0.65f;
        _statusEffects = new();
        _onShift = false;
        _currentTask = null;
        _shiftStart = null;
        _tasksCompletedToday = 0;
        _mistakesToday = 0;

        // Commands - Task Management
        Command<AssignTask>(HandleAssignTask);
        Command<TaskCompleted>(HandleTaskCompleted);
        Command<TaskFailed>(HandleTaskFailed);

        // Customer Service
        Command<ServeCustomer>(HandleServeCustomer);

        // Skill Development
        Command<PracticeSkill>(HandlePracticeSkill);
        Command<ReceiveTraining>(HandleReceiveTraining);

        // State Management
        Command<UpdateEmployeeState>(HandleUpdateEmployeeState);
        Command<GetEmployeeState>(HandleGetEmployeeState);

        // Feedback
        Command<ReceiveFeedback>(HandleReceiveFeedback);

        // Breaks
        Command<TakeBreak>(HandleTakeBreak);

        // Interactions
        Command<InteractWithPeer>(HandleInteractWithPeer);
        Command<InteractWithBoss>(HandleInteractWithBoss);

        // Performance
        Command<GetPerformanceMetrics>(HandleGetPerformanceMetrics);

        // Shift
        Command<StartShift>(HandleStartShift);
        Command<EndShift>(HandleEndShift);

        // Errors
        Command<ReportMistake>(HandleReportMistake);

        // Personality
        Command<GetEmployeePersonality>(HandleGetEmployeePersonality);
        Command<AdjustEmployeeTraits>(HandleAdjustEmployeeTraits);

        Command<SaveSnapshotSuccess>(HandleSaveSnapshotSuccess);
        Command<SaveSnapshotFailure>(HandleSaveSnapshotFailure);

        // Recovery
        Recover<SnapshotOffer>(HandleSnapshotOffer);
        Recover<EmployeeStateUpdated>(HandleEmployeeStateUpdated);
        Recover<SkillUpdated>(HandleSkillUpdated);
        Recover<TaskPracticed>(HandleTaskPracticed);
    }

    protected override void PreStart()
    {
        base.PreStart();
        _log.Info($"EmployeeActor starting: {PersistenceId}");
    }

    protected override void PostStop()
    {
        _log.Info($"EmployeeActor stopped: {PersistenceId}");

        _eventBusAdapter.Tell(new ActorStoppedEvent
        {
            EntityId = _state.EntityId,
            ActorPath = Self.Path.ToString(),
            Reason = "Normal shutdown",
            Timestamp = DateTime.UtcNow
        });

        base.PostStop();
    }

    #region Command Handlers - Task Management

    private void HandleAssignTask(AssignTask msg)
    {
        _log.Debug($"Task assigned: {msg.TaskType} (Priority: {msg.Priority})");

        if (_currentTask != null)
        {
            Sender.Tell(new TaskFailed(
                msg.TaskId,
                _state.EntityId,
                "Already working on another task",
                DateTime.UtcNow
            ));
            return;
        }

        _currentTask = msg.TaskId;
        _state.CurrentBehavior = msg.TaskType;

        // Calculate estimated duration based on skill and personality
        var baseTime = GetTaskBaseTime(msg.TaskType);
        var skillModifier = GetSkillLevel(msg.TaskType);
        var energyModifier = 1.0f + (_personality.Performance.FatigueImpact * (1.0f - _energy));
        var estimatedDuration = baseTime * (2.0f - skillModifier) * energyModifier;

        var started = new TaskStarted(
            msg.TaskId,
            _state.EntityId,
            DateTime.UtcNow,
            estimatedDuration
        );

        Sender.Tell(started);
        PersistEvent(new TaskPracticed(msg.TaskType, DateTime.UtcNow));

        // Add to memory
        AddMemory($"Started task: {msg.TaskType}", "task", 0.4f);
    }

    private void HandleTaskCompleted(TaskCompleted msg)
    {
        _log.Debug($"Task completed: {msg.TaskId} (Quality: {msg.QualityScore})");

        _currentTask = null;
        _state.CurrentBehavior = "Idle";
        _tasksCompletedToday++;

        // Skill improvement
        var taskType = msg.Results.TryGetValue("task_type", out var tt) ? tt.ToString() ?? "" : "";
        if (!string.IsNullOrEmpty(taskType))
        {
            ImproveSkill(taskType, msg.QualityScore);
        }

        // Energy and stress impact
        var energyCost = 0.05f * (2.0f - msg.QualityScore);
        var stressChange = msg.QualityScore > 0.8f ? -0.02f : 0.01f;

        UpdateState(energyCost: -energyCost, stressChange: stressChange);

        // Confidence boost for good work
        if (msg.QualityScore >= 0.8f)
        {
            _confidence = Math.Clamp(_confidence + _personality.Growth.ConfidenceBuildRate, 0f, 1f);
            SetEmotion("proud");
        }

        AddMemory($"Completed: {taskType} (Quality: {msg.QualityScore:F2})", "task_completion", msg.QualityScore * 0.5f);

        PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));

        Sender.Tell(new StatusReply(true, "Task completed successfully"));
    }

    private void HandleTaskFailed(TaskFailed msg)
    {
        _log.Warning($"Task failed: {msg.TaskId} - {msg.Reason}");

        _currentTask = null;
        _state.CurrentBehavior = "Idle";
        _mistakesToday++;

        // Confidence and motivation hit
        _confidence = Math.Clamp(_confidence - _personality.Growth.ConfidenceDecayRate, 0f, 1f);
        _motivation = Math.Clamp(_motivation - 0.05f, 0f, 1f);
        _stress += 0.05f;

        SetEmotion("frustrated");

        AddMemory($"Failed task: {msg.Reason}", "task_failure", 0.6f);

        PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));

        Sender.Tell(new StatusReply(false, $"Task failed: {msg.Reason}"));
    }

    #endregion

    #region Command Handlers - Customer Service

    private void HandleServeCustomer(ServeCustomer msg)
    {
        _log.Debug($"Serving customer: {msg.CustomerId}");

        var serviceSkill = _skills.CustomerService;
        var friendliness = _personality.Traits.Friendliness;
        var stressPenalty = _stress * (_personality.TaskModifiers.TryGetValue("customer_service", out var mod)
            ? mod.StressPenalty
            : 0.1f);

        // Calculate satisfaction
        var baseSatisfaction = 0.70f;
        var satisfactionScore = Math.Clamp(
            baseSatisfaction + (serviceSkill * 0.15f) + (friendliness * 0.15f) - stressPenalty,
            0f,
            1f
        );

        // Service time depends on skill and energy
        var baseServiceTime = 120f; // seconds
        var serviceTime = baseServiceTime * (2.0f - serviceSkill) * (1.0f + (1.0f - _energy) * 0.3f);

        var result = new CustomerServed(
            msg.CustomerId,
            _state.EntityId,
            satisfactionScore,
            serviceTime,
            new Dictionary<string, object>
            {
                ["order_id"] = msg.OrderId,
                ["employee_mood"] = _currentEmotion,
                ["service_quality"] = satisfactionScore >= 0.8f ? "excellent" : satisfactionScore >= 0.6f ? "good" : "average"
            }
        );

        Sender.Tell(result);

        // Practice customer service skill
        ImproveSkill("customer_service", satisfactionScore);

        // Energy and stress impact
        UpdateState(energyCost: -0.03f, stressChange: msg.CustomerMood == "difficult" ? 0.05f : -0.01f);

        AddMemory($"Served customer {msg.CustomerId}", "customer_service", satisfactionScore * 0.4f);

        // Publish dialogue event
        _eventBusAdapter.Tell(new NPCDialogueEvent
        {
            EntityId = _state.EntityId,
            TargetEntityId = msg.CustomerId,
            DialogueText = GenerateCustomerGreeting(msg.CustomerMood),
            EmotionalTone = _currentEmotion,
            Metadata = new Dictionary<string, object>
            {
                ["service_type"] = "order_taking",
                ["satisfaction"] = satisfactionScore
            },
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    #region Command Handlers - Skill Development

    private void HandlePracticeSkill(PracticeSkill msg)
    {
        _log.Debug($"Practicing skill: {msg.SkillName}");

        var currentLevel = GetSkillLevel(msg.SkillName);
        var improvement = _personality.Growth.SkillImprovementRate * _personality.Behavior.LearningRate;

        // Check if at plateau
        if (_personality.Learning.TryGetValue(msg.SkillName, out var learningCurve))
        {
            if (currentLevel >= learningCurve.PlateauLevel)
            {
                improvement *= 0.5f; // Slower improvement after plateau
            }
        }

        var newLevel = Math.Clamp(currentLevel + improvement, 0f, 1f);
        SetSkillLevel(msg.SkillName, newLevel);

        var improved = new SkillImproved(
            _state.EntityId,
            msg.SkillName,
            currentLevel,
            newLevel,
            DateTime.UtcNow
        );

        Sender.Tell(improved);
        PersistEvent(new SkillUpdated(msg.SkillName, newLevel));

        AddMemory($"Practiced {msg.SkillName}: {currentLevel:F2} → {newLevel:F2}", "skill_development", 0.5f);
    }

    private void HandleReceiveTraining(ReceiveTraining msg)
    {
        _log.Info($"Receiving training on {msg.SkillName} from {msg.TrainerId}");

        var currentLevel = GetSkillLevel(msg.SkillName);
        var trainingBonus = 0.05f; // Training is more effective than practice
        var learningRateModifier = _personality.Behavior.LearningRate;

        var improvement = trainingBonus * learningRateModifier * (_energy * 0.5f + 0.5f);
        var newLevel = Math.Clamp(currentLevel + improvement, 0f, 1f);

        SetSkillLevel(msg.SkillName, newLevel);

        // Boost motivation from training
        _motivation = Math.Clamp(_motivation + 0.05f, 0f, 1f);

        // Create/update relationship with trainer
        if (!_relationships.TryGetValue(msg.TrainerId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.TrainerId,
                Affinity = 10f,
                RelationshipType = "Friendly",
                LastInteraction = DateTime.UtcNow
            };
            _relationships[msg.TrainerId] = relationship;
        }

        relationship.AdjustAffinity(5f);
        relationship.RecordInteraction($"Training on {msg.SkillName}");

        var improved = new SkillImproved(
            _state.EntityId,
            msg.SkillName,
            currentLevel,
            newLevel,
            DateTime.UtcNow
        );

        Sender.Tell(improved);
        PersistEvent(new SkillUpdated(msg.SkillName, newLevel));

        AddMemory($"Trained by {msg.TrainerId} on {msg.SkillName}", "training", 0.7f);
    }

    #endregion

    #region Command Handlers - State Management

    private void HandleUpdateEmployeeState(UpdateEmployeeState msg)
    {
        var updated = false;

        if (msg.Energy.HasValue)
        {
            _energy = Math.Clamp(msg.Energy.Value, 0f, 1f);
            updated = true;
        }

        if (msg.Stress.HasValue)
        {
            _stress = Math.Clamp(msg.Stress.Value, 0f, 1f);
            updated = true;
        }

        if (msg.Motivation.HasValue)
        {
            _motivation = Math.Clamp(msg.Motivation.Value, 0f, 1f);
            updated = true;
        }

        if (msg.CurrentEmotion != null && _personality.Emotions.Available.Contains(msg.CurrentEmotion))
        {
            _currentEmotion = msg.CurrentEmotion;
            updated = true;
        }

        if (msg.StatusEffects != null)
        {
            foreach (var (key, value) in msg.StatusEffects)
            {
                _statusEffects[key] = value;
            }
            updated = true;
        }

        if (updated)
        {
            _state.LastUpdated = DateTime.UtcNow;
            _state.EmotionalState = _currentEmotion;
            PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));
        }

        Sender.Tell(new StatusReply(updated));
    }

    private void HandleGetEmployeeState(GetEmployeeState msg)
    {
        var response = new EmployeeStateResponse(
            _state.EntityId,
            _energy,
            _stress,
            _motivation,
            _currentEmotion,
            GetSkillsDictionary(),
            new Dictionary<string, float>(_statusEffects),
            _state.LastUpdated
        );

        Sender.Tell(response);
    }

    #endregion

    #region Command Handlers - Feedback

    private void HandleReceiveFeedback(ReceiveFeedback msg)
    {
        _log.Debug($"Received {msg.FeedbackType} from {msg.FromEntityId}");

        float motivationDelta = 0f;
        float confidenceDelta = 0f;
        string emotionalResponse = _currentEmotion;

        switch (msg.FeedbackType.ToLower())
        {
            case "praise":
                motivationDelta = 0.10f * msg.ImpactIntensity * _personality.Relationships.FeedbackReceptivity;
                confidenceDelta = _personality.Growth.ConfidenceBuildRate * msg.ImpactIntensity;
                emotionalResponse = "happy";
                break;

            case "criticism":
                motivationDelta = -0.05f * msg.ImpactIntensity;
                confidenceDelta = -_personality.Growth.ConfidenceDecayRate * msg.ImpactIntensity;
                emotionalResponse = _personality.Relationships.FeedbackReceptivity > 0.7f ? "focused" : "frustrated";
                break;

            case "suggestion":
                motivationDelta = 0.03f * msg.ImpactIntensity * _personality.Behavior.LearningRate;
                emotionalResponse = "focused";
                break;
        }

        _motivation = Math.Clamp(_motivation + motivationDelta, 0f, 1f);
        _confidence = Math.Clamp(_confidence + confidenceDelta, 0f, 1f);
        SetEmotion(emotionalResponse);

        // Update relationship with feedback giver
        if (!_relationships.TryGetValue(msg.FromEntityId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.FromEntityId,
                Affinity = 0f,
                RelationshipType = "Neutral",
                LastInteraction = DateTime.UtcNow
            };
            _relationships[msg.FromEntityId] = relationship;
        }

        var affinityChange = msg.FeedbackType.ToLower() == "praise" ? 5f : -2f;
        relationship.AdjustAffinity(affinityChange * msg.ImpactIntensity);
        relationship.RecordInteraction($"Received {msg.FeedbackType}");

        var result = new FeedbackProcessed(
            _state.EntityId,
            msg.FeedbackType,
            motivationDelta,
            confidenceDelta,
            emotionalResponse
        );

        Sender.Tell(result);

        AddMemory($"{msg.FeedbackType} from {msg.FromEntityId}: {msg.Message}", "feedback", msg.ImpactIntensity * 0.6f);

        PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));
    }

    #endregion

    #region Command Handlers - Breaks

    private void HandleTakeBreak(TakeBreak msg)
    {
        _log.Debug($"Taking {msg.BreakType} break");

        _currentTask = null;
        _state.CurrentBehavior = "OnBreak";

        // Calculate recovery
        float energyRestored = msg.BreakType switch
        {
            "short" => 0.15f,
            "meal" => 0.35f,
            "emergency" => 0.25f,
            _ => 0.15f
        };

        float stressReduced = msg.BreakType switch
        {
            "short" => 0.10f,
            "meal" => 0.25f,
            "emergency" => 0.30f,
            _ => 0.10f
        };

        _energy = Math.Clamp(_energy + energyRestored, 0f, 1f);
        _stress = Math.Clamp(_stress - stressReduced, 0f, 1f);

        if (_energy > 0.7f && _stress < 0.3f)
        {
            SetEmotion("content");
        }

        var result = new BreakCompleted(
            _state.EntityId,
            energyRestored,
            stressReduced,
            DateTime.UtcNow.AddSeconds(msg.Duration)
        );

        Sender.Tell(result);

        AddMemory($"Took {msg.BreakType} break", "break", 0.3f);

        PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));
    }

    #endregion

    #region Command Handlers - Interactions

    private void HandleInteractWithPeer(InteractWithPeer msg)
    {
        _log.Debug($"Interacting with peer {msg.PeerEmployeeId}: {msg.InteractionType}");

        if (!_relationships.TryGetValue(msg.PeerEmployeeId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.PeerEmployeeId,
                Affinity = 0f,
                RelationshipType = "Neutral",
                LastInteraction = DateTime.UtcNow
            };
            _relationships[msg.PeerEmployeeId] = relationship;
        }

        float affinityChange = msg.InteractionType switch
        {
            "help" => 10f * _personality.Traits.Teamwork,
            "collaborate" => 5f * _personality.Traits.Teamwork,
            "chat" => 3f * _personality.Traits.Friendliness,
            _ => 2f
        };

        relationship.AdjustAffinity(affinityChange);
        relationship.RecordInteraction($"{msg.InteractionType}: {msg.Context}");

        // Small stress reduction from positive social interaction
        if (affinityChange > 0)
        {
            _stress = Math.Clamp(_stress - 0.02f, 0f, 1f);
        }

        var result = new PeerInteractionResult(
            _state.EntityId,
            msg.PeerEmployeeId,
            "positive",
            affinityChange,
            new Dictionary<string, object>
            {
                ["interaction_type"] = msg.InteractionType,
                ["context"] = msg.Context ?? ""
            }
        );

        Sender.Tell(result);

        AddMemory($"{msg.InteractionType} with {msg.PeerEmployeeId}", "social", 0.4f);
    }

    private void HandleInteractWithBoss(InteractWithBoss msg)
    {
        _log.Debug($"Interacting with boss {msg.BossId}: {msg.InteractionType}");

        if (!_relationships.TryGetValue(msg.BossId, out var relationship))
        {
            relationship = new AvatarRelationship
            {
                EntityId = _state.EntityId,
                TargetEntityId = msg.BossId,
                Affinity = _personality.Relationships.BossRespect * 100f - 50f,
                RelationshipType = "Professional",
                LastInteraction = DateTime.UtcNow
            };
            _relationships[msg.BossId] = relationship;
        }

        relationship.RecordInteraction($"{msg.InteractionType}: {msg.Topic}");

        // Slight stress from boss interaction depending on type
        var stressChange = msg.InteractionType switch
        {
            "report" => 0.02f,
            "ask" => 0.01f,
            "discuss" => -0.01f,
            _ => 0f
        };

        _stress = Math.Clamp(_stress + stressChange, 0f, 1f);

        AddMemory($"{msg.InteractionType} with boss about {msg.Topic}", "boss_interaction", 0.5f);

        Sender.Tell(new StatusReply(true));
    }

    #endregion

    #region Command Handlers - Performance & Shift

    private void HandleGetPerformanceMetrics(GetPerformanceMetrics msg)
    {
        var response = new PerformanceMetricsResponse(
            _state.EntityId,
            CalculateOverallEfficiency(),
            CalculateCustomerSatisfaction(),
            CalculateTaskAccuracy(),
            _tasksCompletedToday,
            _mistakesToday,
            GetSkillsDictionary(),
            _shiftStart ?? DateTime.UtcNow.Date,
            DateTime.UtcNow
        );

        Sender.Tell(response);
    }

    private void HandleStartShift(StartShift msg)
    {
        _log.Info($"Starting {msg.ShiftType} shift");

        _onShift = true;
        _shiftStart = msg.StartTime;
        _tasksCompletedToday = 0;
        _mistakesToday = 0;

        // Reset daily energy based on shift type
        _energy = msg.ShiftType switch
        {
            "morning" => 0.9f,
            "afternoon" => 0.8f,
            "evening" => 0.7f,
            _ => 0.8f
        };

        _motivation = _personality.Growth.MotivationBaseline;
        SetEmotion("content");

        AddMemory($"Started {msg.ShiftType} shift", "shift", 0.4f);

        Sender.Tell(new StatusReply(true, $"{msg.ShiftType} shift started"));
    }

    private void HandleEndShift(EndShift msg)
    {
        _log.Info("Ending shift");

        _onShift = false;
        _currentTask = null;
        _state.CurrentBehavior = "OffDuty";

        var timeWorked = _shiftStart.HasValue
            ? (float)(msg.ActualEndTime - _shiftStart.Value).TotalHours
            : 0f;

        var status = new ShiftStatus(
            _state.EntityId,
            false,
            null,
            null,
            timeWorked,
            _tasksCompletedToday
        );

        Sender.Tell(status);

        AddMemory($"Completed shift: {_tasksCompletedToday} tasks, {timeWorked:F1}h", "shift", 0.5f);

        _shiftStart = null;
    }

    #endregion

    #region Command Handlers - Errors & Personality

    private void HandleReportMistake(ReportMistake msg)
    {
        _log.Warning($"Mistake reported: {msg.MistakeType} in task {msg.TaskId}");

        _mistakesToday++;
        _confidence = Math.Clamp(_confidence - _personality.Growth.ConfidenceDecayRate, 0f, 1f);
        _stress += 0.05f;

        SetEmotion("frustrated");

        AddMemory($"Mistake: {msg.MistakeType} - {msg.Description}", "mistake", 0.7f);

        PersistEvent(new EmployeeStateUpdated(_energy, _stress, _motivation, _confidence, _currentEmotion));

        Sender.Tell(new StatusReply(true, "Mistake acknowledged"));
    }

    private void HandleGetEmployeePersonality(GetEmployeePersonality msg)
    {
        var response = new EmployeePersonalityResponse(
            _state.EntityId,
            _personality.Name,
            new Dictionary<string, float>
            {
                ["diligence"] = _personality.Traits.Diligence,
                ["friendliness"] = _personality.Traits.Friendliness,
                ["adaptability"] = _personality.Traits.Adaptability,
                ["teamwork"] = _personality.Traits.Teamwork
            },
            GetSkillsDictionary(),
            _personality.AvatarType
        );

        Sender.Tell(response);
    }

    private void HandleAdjustEmployeeTraits(AdjustEmployeeTraits msg)
    {
        // For MVP, we don't dynamically adjust personality traits
        // But we acknowledge the request

        AddMemory($"Personality adjustment requested: {msg.Reason}", "personality", 0.5f);

        Sender.Tell(new StatusReply(true, "Trait adjustment noted"));
    }

    #endregion

    #region Recovery Handlers

    private void HandleSnapshotOffer(SnapshotOffer offer)
    {
        if (offer.Snapshot is EmployeeActorSnapshot snapshot)
        {
            _state = snapshot.State;
            _memory = snapshot.Memory;
            _skills = snapshot.Skills;
            _relationships = snapshot.Relationships;
            _taskPracticeCount = snapshot.TaskPracticeCount;
            _currentEmotion = snapshot.CurrentEmotion;
            _energy = snapshot.Energy;
            _stress = snapshot.Stress;
            _motivation = snapshot.Motivation;
            _confidence = snapshot.Confidence;
            _statusEffects = snapshot.StatusEffects;
            _onShift = snapshot.OnShift;
            _tasksCompletedToday = snapshot.TasksCompletedToday;
            _mistakesToday = snapshot.MistakesToday;

            _log.Info($"Recovered from snapshot at sequence {offer.Metadata.SequenceNr}");
        }
    }

    private void HandleEmployeeStateUpdated(EmployeeStateUpdated evt)
    {
        _energy = evt.Energy;
        _stress = evt.Stress;
        _motivation = evt.Motivation;
        _confidence = evt.Confidence;
        _currentEmotion = evt.CurrentEmotion;
        _state.LastUpdated = DateTime.UtcNow;
        _state.EmotionalState = _currentEmotion;
    }

    private void HandleSkillUpdated(SkillUpdated evt)
    {
        SetSkillLevel(evt.SkillName, evt.NewLevel);
    }

    private void HandleTaskPracticed(TaskPracticed evt)
    {
        if (!_taskPracticeCount.ContainsKey(evt.TaskType))
        {
            _taskPracticeCount[evt.TaskType] = 0;
        }
        _taskPracticeCount[evt.TaskType]++;
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
                SaveSnapshot(new EmployeeActorSnapshot(
                    _state,
                    _memory,
                    _skills,
                    _relationships,
                    _taskPracticeCount,
                    _currentEmotion,
                    _energy,
                    _stress,
                    _motivation,
                    _confidence,
                    _statusEffects,
                    _onShift,
                    _tasksCompletedToday,
                    _mistakesToday
                ));
            }
        });
    }

    private void AddMemory(string content, string category, float importance)
    {
        var memoryEntry = new Models.MemoryEntry
        {
            EventType = category,
            Description = content,
            Timestamp = DateTime.UtcNow,
            Importance = importance,
            Metadata = new Dictionary<string, object> { ["category"] = category }
        };

        _memory.AddMemory(memoryEntry);
    }

    private void UpdateState(float energyCost = 0f, float stressChange = 0f)
    {
        _energy = Math.Clamp(_energy + energyCost, 0f, 1f);
        _stress = Math.Clamp(_stress + stressChange, 0f, 1f);

        // Check for burnout
        if (_stress > _personality.Growth.BurnoutThreshold)
        {
            _motivation = Math.Clamp(_motivation - 0.05f, 0f, 1f);
            SetEmotion("stressed");
        }
    }

    private void SetEmotion(string emotion)
    {
        if (_personality.Emotions.Available.Contains(emotion))
        {
            var previousEmotion = _currentEmotion;
            _currentEmotion = emotion;
            _state.EmotionalState = emotion;

            if (previousEmotion != emotion)
            {
                _eventBusAdapter.Tell(new AIBehaviorChangedEvent
                {
                    EntityId = _state.EntityId,
                    PreviousBehavior = previousEmotion,
                    NewBehavior = emotion,
                    Reason = "Emotional state changed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    private float GetTaskBaseTime(string taskType)
    {
        if (_personality.TaskModifiers.TryGetValue(taskType.ToLower(), out var modifier))
        {
            return modifier.BaseTime;
        }
        return 90f; // default
    }

    private float GetSkillLevel(string skillName)
    {
        return skillName.ToLower() switch
        {
            "coffee_making" or "coffeemaking" => _skills.CoffeeMaking,
            "customer_service" or "customerservice" => _skills.CustomerService,
            "cash_handling" or "cashhandling" => _skills.CashHandling,
            "cleaning" => _skills.Cleaning,
            "multitasking" => _skills.Multitasking,
            "problem_solving" or "problemsolving" => _skills.ProblemSolving,
            _ => 0.5f
        };
    }

    private void SetSkillLevel(string skillName, float level)
    {
        switch (skillName.ToLower())
        {
            case "coffee_making":
            case "coffeemaking":
                _skills.CoffeeMaking = level;
                break;
            case "customer_service":
            case "customerservice":
                _skills.CustomerService = level;
                break;
            case "cash_handling":
            case "cashhandling":
                _skills.CashHandling = level;
                break;
            case "cleaning":
                _skills.Cleaning = level;
                break;
            case "multitasking":
                _skills.Multitasking = level;
                break;
            case "problem_solving":
            case "problemsolving":
                _skills.ProblemSolving = level;
                break;
        }
    }

    private void ImproveSkill(string taskType, float qualityScore)
    {
        var skillName = taskType.ToLower() switch
        {
            "make_coffee" => "coffee_making",
            "serve_customer" => "customer_service",
            "handle_cash" => "cash_handling",
            "clean" => "cleaning",
            _ => null
        };

        if (skillName != null && qualityScore >= 0.6f)
        {
            var currentLevel = GetSkillLevel(skillName);
            var improvement = _personality.Growth.SkillImprovementRate * qualityScore;

            // Check practice count
            if (_taskPracticeCount.TryGetValue(taskType, out var count))
            {
                if (_personality.Learning.TryGetValue(skillName, out var curve))
                {
                    if (count >= curve.PracticeRequired && currentLevel >= curve.PlateauLevel)
                    {
                        improvement *= 0.3f; // Diminishing returns
                    }
                }
            }

            var newLevel = Math.Clamp(currentLevel + improvement, 0f, 1f);
            if (newLevel > currentLevel)
            {
                SetSkillLevel(skillName, newLevel);
                PersistEvent(new SkillUpdated(skillName, newLevel));

                if (newLevel - currentLevel >= 0.05f)
                {
                    // Significant improvement
                    _confidence = Math.Clamp(_confidence + _personality.Growth.ConfidenceBuildRate, 0f, 1f);
                    _eventBusAdapter.Tell(new AIThoughtEvent
                    {
                        EntityId = _state.EntityId,
                        Thought = $"I'm getting better at {skillName}!",
                        Category = "skill_growth",
                        Metadata = new Dictionary<string, object>
                        {
                            ["skill"] = skillName,
                            ["old_level"] = currentLevel,
                            ["new_level"] = newLevel
                        },
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }
    }

    private Dictionary<string, float> GetSkillsDictionary()
    {
        return new Dictionary<string, float>
        {
            ["coffee_making"] = _skills.CoffeeMaking,
            ["customer_service"] = _skills.CustomerService,
            ["cash_handling"] = _skills.CashHandling,
            ["cleaning"] = _skills.Cleaning,
            ["multitasking"] = _skills.Multitasking,
            ["problem_solving"] = _skills.ProblemSolving
        };
    }

    private float CalculateOverallEfficiency()
    {
        var baseEfficiency = _personality.Performance.BaseEfficiency;
        var energyFactor = _energy * 0.3f;
        var stressFactor = (1.0f - _stress) * 0.2f;
        var motivationFactor = _motivation * 0.2f;
        var skillFactor = GetSkillsDictionary().Values.Average() * 0.3f;

        return Math.Clamp(baseEfficiency + energyFactor + stressFactor + motivationFactor + skillFactor, 0f, 1f);
    }

    private float CalculateCustomerSatisfaction()
    {
        var serviceSkill = _skills.CustomerService;
        var friendliness = _personality.Traits.Friendliness;
        var stressPenalty = _stress * 0.2f;

        return Math.Clamp((serviceSkill + friendliness) / 2f - stressPenalty, 0f, 1f);
    }

    private float CalculateTaskAccuracy()
    {
        if (_tasksCompletedToday == 0) return 1.0f;

        var mistakeRate = (float)_mistakesToday / _tasksCompletedToday;
        return Math.Clamp(1.0f - mistakeRate, 0f, 1f);
    }

    private string GenerateCustomerGreeting(string customerMood)
    {
        var templates = _personality.ResponseTemplates.TryGetValue("greeting_customer", out var greetings)
            ? greetings
            : new Dictionary<string, List<string>>();

        var moodKey = _energy > 0.7f ? "energetic" : "tired";

        if (templates.TryGetValue(moodKey, out var options) && options.Any())
        {
            return options[new Random().Next(options.Count)];
        }

        return "Welcome to Lablab Bean! What can I get for you?";
    }

    #endregion

    #region Persistence Events

    private sealed record EmployeeStateUpdated(
        float Energy,
        float Stress,
        float Motivation,
        float Confidence,
        string CurrentEmotion
    );

    private sealed record SkillUpdated(
        string SkillName,
        float NewLevel
    );

    private sealed record TaskPracticed(
        string TaskType,
        DateTime PracticedAt
    );

    #endregion
}

/// <summary>
/// Snapshot for EmployeeActor state persistence
/// </summary>
public sealed record EmployeeActorSnapshot(
    AvatarState State,
    AvatarMemory Memory,
    EmployeeSkills Skills,
    Dictionary<string, AvatarRelationship> Relationships,
    Dictionary<string, int> TaskPracticeCount,
    string CurrentEmotion,
    float Energy,
    float Stress,
    float Motivation,
    float Confidence,
    Dictionary<string, float> StatusEffects,
    bool OnShift,
    int TasksCompletedToday,
    int MistakesToday
);
