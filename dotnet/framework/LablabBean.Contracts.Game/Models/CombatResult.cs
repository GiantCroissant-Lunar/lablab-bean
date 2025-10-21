namespace LablabBean.Contracts.Game.Models;

/// <summary>
/// Combat result details.
/// </summary>
public record CombatResult(
    int DamageDealt,
    bool IsHit,
    bool IsKill,
    string? SpecialEffect
);
