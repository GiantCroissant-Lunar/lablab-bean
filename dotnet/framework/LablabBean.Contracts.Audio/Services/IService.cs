using System.Threading;
using System.Threading.Tasks;

namespace LablabBean.Contracts.Audio.Services;

/// <summary>
/// Audio service interface for managing audio playback, mixing, and effects
/// </summary>
public interface IService
{
    /// <summary>
    /// Play an audio clip asynchronously
    /// </summary>
    /// <param name="audioPath">Path to the audio asset</param>
    /// <param name="sourceType">Audio source type to use</param>
    /// <param name="category">Audio category for mixing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Handle to control the playing audio</returns>
    Task<AudioHandle> PlayAsync(
        string audioPath,
        AudioSourceType sourceType,
        AudioCategory category,
        CancellationToken cancellationToken);

    /// <summary>
    /// Play audio with full configuration
    /// </summary>
    /// <param name="request">Complete audio request with all parameters</param>
    /// <returns>Handle to control the playing audio</returns>
    Task<AudioHandle> PlayAsync(AudioRequest request);

    /// <summary>
    /// Play audio at a specific 3D position
    /// </summary>
    /// <param name="audioPath">Path to the audio asset</param>
    /// <param name="sourceType">Audio source type to use</param>
    /// <param name="position">3D world position for spatial audio</param>
    /// <param name="category">Audio category for mixing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Handle to control the playing audio</returns>
    Task<AudioHandle> PlayAtPositionAsync(
        string audioPath,
        AudioSourceType sourceType,
        AudioPosition position,
        AudioCategory category,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stop playing audio by handle
    /// </summary>
    /// <param name="handle">Audio handle to stop</param>
    /// <param name="fadeOutDuration">Optional fade out duration in seconds</param>
    void Stop(AudioHandle handle, float fadeOutDuration);

    /// <summary>
    /// Stop all audio in a specific category
    /// </summary>
    /// <param name="category">Audio category to stop</param>
    /// <param name="fadeOutDuration">Optional fade out duration in seconds</param>
    void StopCategory(AudioCategory category, float fadeOutDuration);

    /// <summary>
    /// Stop all playing audio
    /// </summary>
    /// <param name="fadeOutDuration">Optional fade out duration in seconds</param>
    void StopAll(float fadeOutDuration);

    /// <summary>
    /// Pause or resume audio by handle
    /// </summary>
    /// <param name="handle">Audio handle to control</param>
    /// <param name="paused">True to pause, false to resume</param>
    void SetPaused(AudioHandle handle, bool paused);

    /// <summary>
    /// Pause or resume all audio in a category
    /// </summary>
    /// <param name="category">Audio category to control</param>
    /// <param name="paused">True to pause, false to resume</param>
    void SetCategoryPaused(AudioCategory category, bool paused);

    /// <summary>
    /// Set volume for a specific audio category
    /// </summary>
    /// <param name="category">Audio category to adjust</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    void SetCategoryVolume(AudioCategory category, float volume);

    /// <summary>
    /// Get current volume for a specific audio category
    /// </summary>
    /// <param name="category">Audio category to query</param>
    /// <returns>Current volume level (0.0 to 1.0)</returns>
    float GetCategoryVolume(AudioCategory category);

    /// <summary>
    /// Set volume for a specific playing audio instance
    /// </summary>
    /// <param name="handle">Audio handle to control</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    void SetVolume(AudioHandle handle, float volume);

    /// <summary>
    /// Set pitch for a specific playing audio instance
    /// </summary>
    /// <param name="handle">Audio handle to control</param>
    /// <param name="pitch">Pitch multiplier (1.0 = normal)</param>
    void SetPitch(AudioHandle handle, float pitch);

    /// <summary>
    /// Check if an audio handle is currently playing
    /// </summary>
    /// <param name="handle">Audio handle to check</param>
    /// <returns>True if the audio is currently playing</returns>
    bool IsPlaying(AudioHandle handle);

    /// <summary>
    /// Get comprehensive audio system statistics
    /// </summary>
    /// <returns>Audio system performance and usage stats</returns>
    AudioStats GetStats();

    /// <summary>
    /// Preload an audio clip into memory for faster playback
    /// </summary>
    /// <param name="audioPath">Path to the audio asset</param>
    /// <param name="sourceType">Audio source type to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PreloadAsync(string audioPath, AudioSourceType sourceType, CancellationToken cancellationToken);

    /// <summary>
    /// Unload a previously preloaded audio clip from memory
    /// </summary>
    /// <param name="audioPath">Path to the audio asset</param>
    /// <param name="sourceType">Audio source type</param>
    void Unload(string audioPath, AudioSourceType sourceType);

    /// <summary>
    /// Unload all preloaded audio clips to free memory
    /// </summary>
    void UnloadAll();
}
