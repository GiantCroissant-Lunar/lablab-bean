namespace LablabBean.Contracts.Game.Events;

/// <summary>
/// Published when combat occurs between entities.
/// </summary>
public record CombatEvent(
    Guid AttackerId,
    Guid TargetId,
    int DamageDealt,
    bool IsHit,
    bool IsKill,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public CombatEvent(Guid attackerId, Guid targetId, int damageDealt, bool isHit, bool isKill)
        : this(attackerId, targetId, damageDealt, isHit, isKill, DateTimeOffset.UtcNow)
    {
    }
}
