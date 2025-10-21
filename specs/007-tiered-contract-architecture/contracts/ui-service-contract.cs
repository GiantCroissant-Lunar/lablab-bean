// Assembly: LablabBean.Contracts.UI
// Namespace: LablabBean.Contracts.UI.Services
// Purpose: UI service contract for rendering and input handling

using LablabBean.Contracts.Game.Models;
using LablabBean.Contracts.UI.Events;
using LablabBean.Contracts.UI.Models;

namespace LablabBean.Contracts.UI.Services;

/// <summary>
/// UI service contract for rendering and input handling.
/// Platform-independent interface for display updates and user interaction.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines UI operations without specifying the rendering technology
/// (Terminal.Gui, SadConsole, Unity, etc.). Multiple implementations can coexist
/// and are selected via IRegistry priority.
/// </para>
/// <para>
/// Events published by this service:
/// - InputReceivedEvent: When HandleInputAsync processes user input
/// - ViewportChangedEvent: When SetViewportCenter changes camera position
/// </para>
/// <para>
/// Typical usage pattern:
/// 1. InitializeAsync() - Set up UI system
/// 2. Subscribe to game events (EntityMovedEvent, etc.)
/// 3. RenderViewportAsync() in response to game events
/// 4. UpdateDisplayAsync() to refresh screen
/// 5. HandleInputAsync() when user input occurs
/// </para>
/// </remarks>
public interface IService
{
    /// <summary>
    /// Initialize the UI system.
    /// </summary>
    /// <param name="options">UI initialization options (viewport size, theme, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when UI is ready</returns>
    /// <remarks>
    /// This method sets up the rendering system, creates windows/views, and
    /// prepares for display. Must be called before any rendering operations.
    /// </remarks>
    Task InitializeAsync(UIInitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Render the game viewport with current entities and terrain.
    /// </summary>
    /// <param name="viewport">Viewport bounds to render (camera view)</param>
    /// <param name="entities">Entities to display within the viewport</param>
    /// <returns>Task that completes when rendering is done</returns>
    /// <remarks>
    /// This method updates the display buffer with entities and terrain.
    /// Call UpdateDisplayAsync() afterward to flush changes to the screen.
    /// </remarks>
    Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities);

    /// <summary>
    /// Update the UI display (refresh screen, process pending updates).
    /// </summary>
    /// <returns>Task that completes when display is updated</returns>
    /// <remarks>
    /// Flushes pending rendering changes to the screen. Should be called after
    /// RenderViewportAsync() or other display modifications.
    /// </remarks>
    Task UpdateDisplayAsync();

    /// <summary>
    /// Handle user input command.
    /// </summary>
    /// <param name="command">Input command to process</param>
    /// <returns>Task that completes when input is handled</returns>
    /// <remarks>
    /// Publishes InputReceivedEvent for each input.
    /// The UI service typically translates raw input (key presses) into
    /// semantic commands (movement, action, menu) before publishing.
    /// </remarks>
    Task HandleInputAsync(InputCommand command);

    /// <summary>
    /// Get current viewport bounds.
    /// </summary>
    /// <returns>Current viewport bounds (camera view)</returns>
    /// <remarks>
    /// Returns the current visible area of the game world.
    /// </remarks>
    ViewportBounds GetViewport();

    /// <summary>
    /// Set viewport center position (camera follow).
    /// </summary>
    /// <param name="centerPosition">New center position for the camera</param>
    /// <remarks>
    /// Publishes ViewportChangedEvent if the viewport bounds change.
    /// Typically used to follow the player entity or focus on a specific location.
    /// </remarks>
    void SetViewportCenter(Position centerPosition);
}
