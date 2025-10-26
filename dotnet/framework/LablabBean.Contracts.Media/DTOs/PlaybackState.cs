namespace LablabBean.Contracts.Media.DTOs;

/// <summary>
/// Complete playback state snapshot
/// </summary>
/// <param name="Status">Current playback status</param>
/// <param name="Position">Current playback position</param>
/// <param name="Duration">Total media duration (TimeSpan.Zero if unknown)</param>
/// <param name="Volume">Volume level (0.0 to 1.0)</param>
/// <param name="CurrentMedia">Currently loaded media info (null if none)</param>
/// <param name="ActivePlaylist">Currently active playlist (null if none)</param>
/// <param name="ErrorMessage">Error message if Status is Error (null otherwise)</param>
public record PlaybackState(
    PlaybackStatus Status,
    TimeSpan Position,
    TimeSpan Duration,
    float Volume,
    MediaInfo? CurrentMedia,
    Playlist? ActivePlaylist,
    string? ErrorMessage
);
