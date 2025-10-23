using LablabBean.Contracts.Scene;
using LablabBean.Contracts.Scene.Services;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.SceneLoader;

/// <summary>
/// Example implementation of scene service.
/// </summary>
public class SceneLoaderService : IService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private Camera _camera = new(new Position(0, 0), 1.0f);
    private readonly Viewport _viewport = new(80, 24);
    private readonly List<EntitySnapshot> _entities = new();
    private string? _currentSceneId;

    public event EventHandler<SceneShutdownEventArgs>? Shutdown;

    public SceneLoaderService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LoadSceneAsync(string sceneId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Loading scene: {SceneId}", sceneId);

            // Simulate scene loading
            await Task.Delay(10, ct); // Simulate I/O

            _currentSceneId = sceneId;
            _entities.Clear();

            // Publish event
            await _eventBus.PublishAsync(new SceneLoadedEvent(sceneId));

            _logger.LogInformation("Scene loaded: {SceneId}", sceneId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load scene: {SceneId}", sceneId);
            await _eventBus.PublishAsync(new SceneLoadFailedEvent(sceneId, ex));
            throw;
        }
    }

    public async Task UnloadSceneAsync(string sceneId, CancellationToken ct = default)
    {
        _logger.LogInformation("Unloading scene: {SceneId}", sceneId);

        _entities.Clear();
        _currentSceneId = null;

        // Publish event
        await _eventBus.PublishAsync(new SceneUnloadedEvent(sceneId));

        _logger.LogInformation("Scene unloaded: {SceneId}", sceneId);
    }

    public Viewport GetViewport()
    {
        return _viewport;
    }

    public CameraViewport GetCameraViewport()
    {
        return new CameraViewport(_camera, _viewport);
    }

    public void SetCamera(Camera camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _logger.LogDebug("Camera set to position ({X}, {Y}) with zoom {Zoom}",
            camera.Position.X, camera.Position.Y, camera.Zoom);
    }

    public void UpdateWorld(IReadOnlyList<EntitySnapshot> snapshots)
    {
        _entities.Clear();
        _entities.AddRange(snapshots);
        _logger.LogDebug("World updated with {Count} entities", snapshots.Count);
    }
}
