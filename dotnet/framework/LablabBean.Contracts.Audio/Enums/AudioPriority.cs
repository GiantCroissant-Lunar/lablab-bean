namespace LablabBean.Contracts.Audio;

/// <summary>
/// Audio playback priority levels
/// </summary>
public enum AudioPriority
{
    /// <summary>
    /// Lowest priority - first to be culled when audio sources are limited
    /// </summary>
    Low = 256,

    /// <summary>
    /// Normal priority for most audio
    /// </summary>
    Normal = 128,

    /// <summary>
    /// High priority - important audio that should rarely be culled
    /// </summary>
    High = 64,

    /// <summary>
    /// Critical priority - audio that should never be culled (UI sounds, dialogue, etc.)
    /// </summary>
    Critical = 0
}
