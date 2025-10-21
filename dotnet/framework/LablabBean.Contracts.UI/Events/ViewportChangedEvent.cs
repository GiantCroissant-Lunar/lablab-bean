using LablabBean.Contracts.UI.Models;

namespace LablabBean.Contracts.UI.Events;

/// <summary>
/// Published when viewport bounds change (resize, camera move).
/// </summary>
public record ViewportChangedEvent(
    ViewportBounds OldViewport,
    ViewportBounds NewViewport,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Convenience constructor that automatically sets timestamp to current UTC time.
    /// </summary>
    public ViewportChangedEvent(ViewportBounds oldViewport, ViewportBounds newViewport)
        : this(oldViewport, newViewport, DateTimeOffset.UtcNow)
    {
    }
}
