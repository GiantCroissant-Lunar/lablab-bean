using System.Reactive;
using System.Reactive.Linq;
using LablabBean.Contracts.Media;
using LablabBean.Contracts.Media.DTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace LablabBean.Reactive.ViewModels.Media;

/// <summary>
/// ViewModel for media player UI.
/// Exposes reactive properties and commands for playback control.
/// </summary>
public class MediaPlayerViewModel : ReactiveObject
{
    private readonly IMediaService _mediaService;

    [Reactive]
    public PlaybackState? State { get; private set; }

    [Reactive]
    public TimeSpan Position { get; private set; }

    [Reactive]
    public TimeSpan Duration { get; private set; }

    [Reactive]
    public float Volume { get; set; }

    [Reactive]
    public string? CurrentFilePath { get; set; }

    [Reactive]
    public bool IsPlaying { get; private set; }

    [Reactive]
    public bool IsPaused { get; private set; }

    [Reactive]
    public bool IsStopped { get; private set; }

    [Reactive]
    public string? ErrorMessage { get; private set; }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<string, MediaInfo> LoadMediaCommand { get; }

    public MediaPlayerViewModel(IMediaService mediaService)
    {
        _mediaService = mediaService;

        // Initial state
        Volume = 1.0f;
        IsStopped = true;

        // Subscribe to media service observables
        _mediaService.PlaybackState
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(state =>
            {
                State = state;
                IsPlaying = state.Status == PlaybackStatus.Playing;
                IsPaused = state.Status == PlaybackStatus.Paused;
                IsStopped = state.Status == PlaybackStatus.Stopped;
                ErrorMessage = state.ErrorMessage;
            });

        _mediaService.Position
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(pos => Position = pos);

        _mediaService.Duration
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(dur => Duration = dur);

        _mediaService.Volume
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vol => Volume = vol);

        // Create commands with can-execute logic
        var canPlay = this.WhenAnyValue(
            x => x.State,
            x => x.IsPlaying,
            (state, playing) => state != null &&
                               state.CurrentMedia != null &&
                               !playing &&
                               state.Status != PlaybackStatus.Error);

        PlayCommand = ReactiveCommand.CreateFromTask(
            async () => await _mediaService.PlayAsync(),
            canPlay);

        var canPause = this.WhenAnyValue(x => x.IsPlaying)
            .Select(playing => playing);

        PauseCommand = ReactiveCommand.CreateFromTask(
            async () => await _mediaService.PauseAsync(),
            canPause);

        var canStop = this.WhenAnyValue(x => x.IsStopped)
            .Select(stopped => !stopped);

        StopCommand = ReactiveCommand.CreateFromTask(
            async () => await _mediaService.StopAsync(),
            canStop);

        LoadMediaCommand = ReactiveCommand.CreateFromTask<string, MediaInfo>(
            async path =>
            {
                CurrentFilePath = path;
                return await _mediaService.LoadAsync(path);
            });

        // Handle volume changes with throttling
        this.WhenAnyValue(x => x.Volume)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .DistinctUntilChanged()
            .Skip(1) // Skip initial value
            .Subscribe(async vol =>
            {
                try
                {
                    await _mediaService.SetVolumeAsync(vol);
                }
                catch
                {
                    // Ignore errors during volume adjustment
                }
            });

        // Handle command errors
        PlayCommand.ThrownExceptions.Subscribe(ex =>
        {
            ErrorMessage = $"Play error: {ex.Message}";
        });

        PauseCommand.ThrownExceptions.Subscribe(ex =>
        {
            ErrorMessage = $"Pause error: {ex.Message}";
        });

        StopCommand.ThrownExceptions.Subscribe(ex =>
        {
            ErrorMessage = $"Stop error: {ex.Message}";
        });

        LoadMediaCommand.ThrownExceptions.Subscribe(ex =>
        {
            ErrorMessage = $"Load error: {ex.Message}";
        });
    }

    public async Task SeekAsync(TimeSpan position)
    {
        try
        {
            await _mediaService.SeekAsync(position);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Seek error: {ex.Message}";
        }
    }

    public string FormatTimeSpan(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return time.ToString(@"h\:mm\:ss");
        return time.ToString(@"m\:ss");
    }

    public double ProgressPercentage => Duration.TotalSeconds > 0
        ? (Position.TotalSeconds / Duration.TotalSeconds) * 100.0
        : 0.0;
}
