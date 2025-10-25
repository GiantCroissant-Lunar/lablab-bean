namespace LablabBean.AI.Actors.Messages;

public record TakeDamageMessage(float Damage, string SourceId, DateTime Timestamp);
public record PlayerNearbyMessage(string PlayerId, float Distance, DateTime Timestamp);
public record DialogueRequestMessage(string TargetId, string Topic, Dictionary<string, object>? Context = null);
public record AIDecisionMessage(string DecisionType, string Action, Dictionary<string, object> Parameters, string Reasoning, float Confidence);
public record DialogueResponseMessage(string DialogueText, string EmotionalTone, Dictionary<string, object>? Metadata = null);
public record PublishGameEvent(object Event);
public record SaveSnapshotCommand();
public record GetAIDecisionRequest(string EntityId);
public record GetDialogueRequest(string SpeakerId, string ListenerId, string? Topic = null);
