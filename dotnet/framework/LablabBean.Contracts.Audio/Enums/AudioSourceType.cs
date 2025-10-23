namespace LablabBean.Contracts.Audio;

/// <summary>
/// Audio source types supported by different providers
/// </summary>
public enum AudioSourceType
{
    /// <summary>
    /// .NET native audio
    /// </summary>
    Native,

    /// <summary>
    /// NAudio framework
    /// </summary>
    NAudio,

    /// <summary>
    /// BASS audio library
    /// </summary>
    BASS,

    /// <summary>
    /// OpenAL audio engine
    /// </summary>
    OpenAL,

    /// <summary>
    /// FMOD audio engine
    /// </summary>
    FMOD,

    /// <summary>
    /// Wwise audio engine
    /// </summary>
    Wwise,

    /// <summary>
    /// Custom audio implementation
    /// </summary>
    Custom
}
