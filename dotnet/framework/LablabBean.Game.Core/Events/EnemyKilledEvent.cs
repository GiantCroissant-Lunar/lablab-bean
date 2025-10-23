namespace LablabBean.Game.Core.Events;

/// <summary>
/// Event triggered when an enemy is killed
/// </summary>
public record EnemyKilledEvent(int KillerEntityId, int EnemyEntityId, string EnemyType);
