namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a player interacts with an NPC
/// </summary>
public record NPCInteractionEvent(int PlayerEntityId, int NPCEntityId, string InteractionType);
