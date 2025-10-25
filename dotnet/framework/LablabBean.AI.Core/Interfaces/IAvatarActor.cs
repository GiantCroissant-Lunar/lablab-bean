using LablabBean.AI.Core.Models;

namespace LablabBean.AI.Core.Interfaces;

/// <summary>
/// Interface for avatar actors
/// </summary>
public interface IAvatarActor
{
    string EntityId { get; }
    Task HandleDamageAsync(float damage, string sourceId);
    Task HandlePlayerNearbyAsync(string playerId, float distance);
    Task<AvatarState> GetStateAsync();
    Task SaveSnapshotAsync();
}
