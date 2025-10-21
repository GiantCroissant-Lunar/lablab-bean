// Assembly: LablabBean.Contracts.Game
// Namespace: LablabBean.Contracts.Game.Services
// Purpose: Game service contract for dungeon crawler mechanics

using LablabBean.Contracts.Game.Events;
using LablabBean.Contracts.Game.Models;

namespace LablabBean.Contracts.Game.Services;

/// <summary>
/// Game service contract for dungeon crawler mechanics.
/// Platform-independent interface for game loop, entity management, and turn processing.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the core game operations without specifying implementation
/// details. Multiple implementations can coexist (e.g., console-based, Unity-based)
/// and are selected via IRegistry priority.
/// </para>
/// <para>
/// Events published by this service:
/// - EntitySpawnedEvent: When SpawnEntityAsync succeeds
/// - EntityMovedEvent: When MoveEntityAsync succeeds
/// - CombatEvent: When AttackAsync completes
/// - GameStateChangedEvent: When game state transitions occur
/// </para>
/// </remarks>
public interface IService
{
    /// <summary>
    /// Start a new game session.
    /// </summary>
    /// <param name="options">Game start options (difficulty, seed, player name)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when game is initialized</returns>
    /// <remarks>
    /// Publishes GameStateChangedEvent (NotStarted → Running) on success.
    /// </remarks>
    Task StartGameAsync(GameStartOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a single game turn.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when turn processing is done</returns>
    /// <remarks>
    /// This method executes one iteration of the game loop:
    /// 1. Process player action (if any)
    /// 2. Process AI entity actions
    /// 3. Update game state
    /// 4. Publish relevant events
    /// </remarks>
    Task ProcessTurnAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Spawn an entity at the specified position.
    /// </summary>
    /// <param name="entityType">Type of entity to spawn (e.g., "player", "goblin", "chest")</param>
    /// <param name="position">World position for the entity</param>
    /// <returns>Entity ID of the spawned entity</returns>
    /// <remarks>
    /// Publishes EntitySpawnedEvent on success.
    /// Throws InvalidOperationException if position is occupied or invalid.
    /// </remarks>
    Task<Guid> SpawnEntityAsync(string entityType, Position position);

    /// <summary>
    /// Move an entity to a new position.
    /// </summary>
    /// <param name="entityId">Entity to move</param>
    /// <param name="newPosition">Target position</param>
    /// <returns>True if move succeeded, false if blocked (wall, other entity, etc.)</returns>
    /// <remarks>
    /// Publishes EntityMovedEvent if move succeeds.
    /// Returns false (does not throw) if move is blocked.
    /// </remarks>
    Task<bool> MoveEntityAsync(Guid entityId, Position newPosition);

    /// <summary>
    /// Execute an attack from attacker to target.
    /// </summary>
    /// <param name="attackerId">Attacking entity ID</param>
    /// <param name="targetId">Target entity ID</param>
    /// <returns>Combat result (damage dealt, hit/miss, kill status)</returns>
    /// <remarks>
    /// Publishes CombatEvent with results.
    /// Throws InvalidOperationException if either entity doesn't exist or is dead.
    /// </remarks>
    Task<CombatResult> AttackAsync(Guid attackerId, Guid targetId);

    /// <summary>
    /// Get current game state snapshot.
    /// </summary>
    /// <returns>Immutable game state</returns>
    /// <remarks>
    /// Returns a snapshot of the current game state. This is a read-only view
    /// and does not reflect real-time changes.
    /// </remarks>
    GameState GetGameState();

    /// <summary>
    /// Get all entities as immutable snapshots.
    /// </summary>
    /// <returns>Collection of entity snapshots</returns>
    /// <remarks>
    /// Returns snapshots of all entities in the game world. This is a read-only
    /// view and does not reflect real-time changes.
    /// </remarks>
    IReadOnlyCollection<EntitySnapshot> GetEntities();
}
