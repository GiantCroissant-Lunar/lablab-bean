using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;

namespace LablabBean.Game.Core.Audio;

/// <summary>
/// Service for managing game audio using LibVLCSharp
/// Handles background music, sound effects, and volume control
/// </summary>
public class AudioService : IDisposable
{
    private readonly ILogger<AudioService> _logger;
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _musicPlayer;
    private readonly Dictionary<string, MediaPlayer> _soundPlayers;
    private bool _disposed;

    public float MusicVolume { get; private set; } = 1.0f;
    public float SoundVolume { get; private set; } = 1.0f;
    public bool IsMusicEnabled { get; private set; } = true;
    public bool IsSoundEnabled { get; private set; } = true;

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            LibVLCSharp.Shared.Core.Initialize();
            _libVLC = new LibVLC();
            _musicPlayer = new MediaPlayer(_libVLC);
            _soundPlayers = new Dictionary<string, MediaPlayer>();

            _logger.LogInformation("AudioService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AudioService");
            throw;
        }
    }

    /// <summary>
    /// Plays background music from a file path
    /// </summary>
    public void PlayMusic(string filePath, bool loop = true)
    {
        if (!IsMusicEnabled) return;

        try
        {
            var media = new Media(_libVLC, new Uri(filePath));

            if (loop)
            {
                _musicPlayer.EndReached += (sender, args) =>
                {
                    _musicPlayer.Stop();
                    _musicPlayer.Play(media);
                };
            }

            _musicPlayer.Play(media);
            _musicPlayer.Volume = (int)(MusicVolume * 100);

            _logger.LogInformation("Playing music: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play music: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Stops the currently playing music
    /// </summary>
    public void StopMusic()
    {
        try
        {
            if (_musicPlayer.IsPlaying)
            {
                _musicPlayer.Stop();
                _logger.LogDebug("Music stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop music");
        }
    }

    /// <summary>
    /// Pauses the currently playing music
    /// </summary>
    public void PauseMusic()
    {
        try
        {
            if (_musicPlayer.IsPlaying)
            {
                _musicPlayer.Pause();
                _logger.LogDebug("Music paused");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause music");
        }
    }

    /// <summary>
    /// Resumes paused music
    /// </summary>
    public void ResumeMusic()
    {
        try
        {
            if (!_musicPlayer.IsPlaying)
            {
                _musicPlayer.Play();
                _logger.LogDebug("Music resumed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume music");
        }
    }

    /// <summary>
    /// Plays a sound effect from a file path
    /// </summary>
    public void PlaySound(string soundId, string filePath)
    {
        if (!IsSoundEnabled) return;

        try
        {
            // Clean up old player if exists
            if (_soundPlayers.TryGetValue(soundId, out var oldPlayer))
            {
                oldPlayer.Stop();
                oldPlayer.Dispose();
                _soundPlayers.Remove(soundId);
            }

            // Create new player for this sound
            var player = new MediaPlayer(_libVLC);
            var media = new Media(_libVLC, new Uri(filePath));

            // Remove player when done
            player.EndReached += (sender, args) =>
            {
                player.Dispose();
                _soundPlayers.Remove(soundId);
            };

            player.Volume = (int)(SoundVolume * 100);
            player.Play(media);

            _soundPlayers[soundId] = player;

            _logger.LogDebug("Playing sound: {SoundId} from {FilePath}", soundId, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play sound: {SoundId} from {FilePath}", soundId, filePath);
        }
    }

    /// <summary>
    /// Sets the music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Math.Clamp(volume, 0f, 1f);
        _musicPlayer.Volume = (int)(MusicVolume * 100);
        _logger.LogDebug("Music volume set to {Volume}", MusicVolume);
    }

    /// <summary>
    /// Sets the sound effects volume (0.0 to 1.0)
    /// </summary>
    public void SetSoundVolume(float volume)
    {
        SoundVolume = Math.Clamp(volume, 0f, 1f);

        foreach (var player in _soundPlayers.Values)
        {
            player.Volume = (int)(SoundVolume * 100);
        }

        _logger.LogDebug("Sound volume set to {Volume}", SoundVolume);
    }

    /// <summary>
    /// Enables or disables music
    /// </summary>
    public void SetMusicEnabled(bool enabled)
    {
        IsMusicEnabled = enabled;

        if (!enabled)
        {
            StopMusic();
        }

        _logger.LogInformation("Music {Status}", enabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// Enables or disables sound effects
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        IsSoundEnabled = enabled;

        if (!enabled)
        {
            StopAllSounds();
        }

        _logger.LogInformation("Sound effects {Status}", enabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// Stops all currently playing sound effects
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var (soundId, player) in _soundPlayers.ToList())
        {
            try
            {
                player.Stop();
                player.Dispose();
                _soundPlayers.Remove(soundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop sound: {SoundId}", soundId);
            }
        }

        _logger.LogDebug("All sounds stopped");
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            StopMusic();
            StopAllSounds();

            _musicPlayer?.Dispose();

            foreach (var player in _soundPlayers.Values)
            {
                player?.Dispose();
            }

            _soundPlayers.Clear();
            _libVLC?.Dispose();

            _logger.LogInformation("AudioService disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing AudioService");
        }

        _disposed = true;
    }
}
