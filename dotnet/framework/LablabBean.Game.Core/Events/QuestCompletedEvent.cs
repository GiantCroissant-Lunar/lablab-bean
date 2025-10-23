namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a quest is completed
/// </summary>
public record QuestCompletedEvent(string QuestId, int EntityId);
