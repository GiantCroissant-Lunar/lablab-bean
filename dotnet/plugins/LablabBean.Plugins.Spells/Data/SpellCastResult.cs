namespace LablabBean.Plugins.Spells.Data;

/// <summary>
/// Result of a spell cast attempt.
/// </summary>
public record SpellCastResult(
    bool Success,
    string? FailureReason = null,
    int DamageDealt = 0,
    int HealingDone = 0,
    List<Guid>? AffectedEntities = null
);
