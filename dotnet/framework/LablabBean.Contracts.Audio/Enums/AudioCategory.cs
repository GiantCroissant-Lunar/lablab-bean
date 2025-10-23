namespace LablabBean.Contracts.Audio;

/// <summary>
/// Audio categories for volume mixing and management
/// </summary>
public enum AudioCategory
{
    /// <summary>
    /// Background music and ambient sounds
    /// </summary>
    Music,

    /// <summary>
    /// Sound effects and gameplay audio
    /// </summary>
    SFX,

    /// <summary>
    /// Voice acting and dialogue
    /// </summary>
    Voice,

    /// <summary>
    /// User interface sounds
    /// </summary>
    UI,

    /// <summary>
    /// Environment and ambient audio
    /// </summary>
    Environment,

    /// <summary>
    /// Master volume (affects all categories)
    /// </summary>
    Master
}
