namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when a spell is cast
/// </summary>
public record SpellCastEvent(int CasterEntityId, string SpellId, int? TargetEntityId = null);
