namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Plugin loader with AssemblyLoadContext isolation, lifecycle orchestration, and hot reload support.
/// </summary>
public sealed class PluginLoader : IPluginLoader, IDisposable
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;
    private readonly PluginRegistry _pluginRegistry;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly DependencyResolver _dependencyResolver;
    private readonly CapabilityValidator _capabilityValidator;
    private readonly Dictionary<string, LoadedPlugin> _loadedPlugins = new();
    private readonly bool _enableHotReload;
    private readonly string _profile;
    private readonly PluginSystemMetrics? _metrics;
    private bool _disposed;

    public PluginLoader(
        ILogger<PluginLoader> logger,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IServiceProvider services,
        PluginRegistry pluginRegistry,
        ServiceRegistry serviceRegistry,
        bool enableHotReload = false,
        string profile = "dotnet.console",
        PluginSystemMetrics? metrics = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _pluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _enableHotReload = enableHotReload;
        _profile = profile;
        _metrics = metrics;
        _dependencyResolver = new DependencyResolver(_logger);
        _capabilityValidator = new CapabilityValidator(_logger, _configuration);

        // Ensure core framework services are present in the registry before any plugin InitializeAsync runs.
        try
        {
            var eventBus = services.GetService<IEventBus>();
            if (eventBus != null && !_serviceRegistry.IsRegistered<IEventBus>())
            {
                _serviceRegistry.Register<IEventBus>(eventBus, new ServiceMetadata
                {
                    Priority = 1000,
                    Name = "EventBus",
                    Version = "1.0.0"
                });
            }
        }
        catch
        {
            // Non-fatal: plugins that require EventBus will error if unavailable; leave as is.
        }
    }

    public IPluginRegistry PluginRegistry => _pluginRegistry;
    public IRegistry ServiceRegistry => _serviceRegistry;

    /// <summary>
    /// Discover plugins from the specified directories.
    /// </summary>
    public async Task<int> DiscoverAndLoadAsync(IEnumerable<string> pluginPaths, CancellationToken ct = default)
    {
        // Phase 0: Preload contract assemblies into Default ALC
        PreloadContractAssemblies();

        var manifests = new List<(string path, PluginManifest manifest)>();

        foreach (var pluginPath in pluginPaths)
        {
            var absolutePath = Path.GetFullPath(pluginPath);

            if (!Directory.Exists(absolutePath))
            {
                _logger.LogWarning("Plugin path does not exist: {PluginPath}", absolutePath);
                continue;
            }

            // If the provided path is itself a plugin directory (contains plugin.json), process directly
            var selfManifest = Path.Combine(absolutePath, "plugin.json");
            if (File.Exists(selfManifest))
            {
                try
                {
                    var manifest = ManifestParser.ParseFile(selfManifest);
                    manifests.Add((absolutePath, manifest));
                    _logger.LogInformation("Discovered plugin: {PluginId} v{Version}", manifest.Id, manifest.Version);
                    continue; // don't enumerate children
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse manifest: {ManifestPath}", selfManifest);
                    continue;
                }
            }

            _logger.LogInformation("Scanning for plugins in: {PluginPath}", absolutePath);

            var pluginDirs = Directory.GetDirectories(absolutePath);
            foreach (var pluginDir in pluginDirs)
            {
                var manifestPath = Path.Combine(pluginDir, "plugin.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifest = ManifestParser.ParseFile(manifestPath);
                        manifests.Add((pluginDir, manifest));
                        _logger.LogInformation("Discovered plugin: {PluginId} v{Version}", manifest.Id, manifest.Version);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse manifest: {ManifestPath}", manifestPath);
                    }
                    continue;
                }

                // Defensive: also discover manifests nested under "plugins/<name>/plugin.json"
                var nestedPluginsRoot = Path.Combine(pluginDir, "plugins");
                if (Directory.Exists(nestedPluginsRoot))
                {
                    foreach (var nestedDir in Directory.GetDirectories(nestedPluginsRoot))
                    {
                        var nestedManifest = Path.Combine(nestedDir, "plugin.json");
                        if (!File.Exists(nestedManifest))
                            continue;

                        try
                        {
                            var manifest = ManifestParser.ParseFile(nestedManifest);
                            manifests.Add((nestedDir, manifest));
                            _logger.LogInformation("Discovered plugin (nested): {PluginId} v{Version}", manifest.Id, manifest.Version);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse nested manifest: {ManifestPath}", nestedManifest);
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("No plugin.json found in: {PluginDir}", pluginDir);
                }
            }
        }

        if (manifests.Count == 0)
        {
            _logger.LogWarning("No plugins discovered");
            return 0;
        }

        // Step 1: Validate capabilities (single UI + single renderer)
        var allManifests = manifests.Select(m => m.manifest).ToList();
        var capabilityResult = _capabilityValidator.Validate(allManifests);

        // Register capability-excluded plugins
        foreach (var (excludedId, reason) in capabilityResult.ExcludedPlugins)
        {
            var excluded = manifests.First(m => m.manifest.Id == excludedId);
            var descriptor = new PluginDescriptor
            {
                Id = excluded.manifest.Id,
                Name = excluded.manifest.Name,
                Version = excluded.manifest.Version,
                State = PluginState.Failed,
                Manifest = excluded.manifest,
                FailureReason = reason
            };
            _pluginRegistry.Add(descriptor);
        }

        // Step 2: Resolve dependencies on capability-validated plugins
        var result = _dependencyResolver.Resolve(capabilityResult.ManifestsToLoad);

        foreach (var excludedId in result.ExcludedPlugins)
        {
            var excluded = manifests.First(m => m.manifest.Id == excludedId);
            var descriptor = new PluginDescriptor
            {
                Id = excluded.manifest.Id,
                Name = excluded.manifest.Name,
                Version = excluded.manifest.Version,
                State = PluginState.Failed,
                Manifest = excluded.manifest,
                FailureReason = result.FailureReasons[excludedId]
            };
            _pluginRegistry.Add(descriptor);
        }

        var loadablePlugins = manifests
            .Where(m => !result.ExcludedPlugins.Contains(m.manifest.Id))
            .ToDictionary(m => m.manifest.Id);

        var loadedCount = 0;
        foreach (var pluginId in result.LoadOrder)
        {
            if (!loadablePlugins.TryGetValue(pluginId, out var pluginInfo))
            {
                continue;
            }

            try
            {
                await LoadPluginAsync(pluginInfo.path, pluginInfo.manifest, ct);
                loadedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin: {PluginId}", pluginId);
                _pluginRegistry.UpdateState(pluginId, PluginState.Failed, ex.Message);

                var failedMetrics = _metrics?.Plugins.FirstOrDefault(m => m.PluginName == pluginId);
                if (failedMetrics != null)
                {
                    _metrics?.CompletePluginLoad(failedMetrics, false, ex.Message);
                }
            }
        }

        return loadedCount;
    }

    private async Task LoadPluginAsync(string pluginDir, PluginManifest manifest, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Loading plugin: {PluginId}", manifest.Id);

        var pluginMetrics = _metrics?.StartPluginLoad(manifest.Id, _profile);

        var descriptor = new PluginDescriptor
        {
            Id = manifest.Id,
            Name = manifest.Name,
            Version = manifest.Version,
            State = PluginState.Created,
            Manifest = manifest
        };
        _pluginRegistry.Add(descriptor);

        string? assemblyName = null;
        string? typeName = null;

        if (manifest.EntryPoint.Count > 0)
        {
            var profile = DetermineProfile();
            if (manifest.EntryPoint.TryGetValue(profile, out var entryPoint))
            {
                var parts = entryPoint.Split(',');
                if (parts.Length == 2)
                {
                    assemblyName = parts[0].Trim();
                    typeName = parts[1].Trim();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(typeName))
        {
            assemblyName = manifest.EntryAssembly;
            typeName = manifest.EntryType;
        }

        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(typeName))
        {
            throw new InvalidOperationException($"No valid entry point found for plugin {manifest.Id}");
        }

        var assemblyPath = Path.Combine(pluginDir, assemblyName);
        if (!assemblyPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            assemblyPath += ".dll";
        }

        if (!File.Exists(assemblyPath))
        {
            // Fallback: in published console artifacts, plugin.json is copied under
            // publish/console/plugins/<Plugin>/, but the assembly DLLs are placed in
            // the main publish directory (AppContext.BaseDirectory). Try there.
            var baseDir = AppContext.BaseDirectory;
            var altPath = Path.Combine(baseDir, Path.GetFileName(assemblyPath));
            if (File.Exists(altPath))
            {
                _logger.LogInformation(
                    "Plugin assembly not found beside manifest, using base directory: {AltPath}",
                    altPath);
                assemblyPath = altPath;
            }
            else
            {
                throw new FileNotFoundException($"Plugin assembly not found: {assemblyPath}");
            }
        }

        var loadContext = new PluginLoadContext(assemblyPath, _enableHotReload);
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

        var pluginType = assembly.GetType(typeName);
        if (pluginType == null)
        {
            throw new InvalidOperationException($"Plugin type not found: {typeName}");
        }

        // Check if type implements IPlugin by name (cross-ALC compatibility)
        var implementsIPlugin = pluginType.GetInterfaces()
            .Any(i => i.FullName == typeof(IPlugin).FullName);

        if (!implementsIPlugin)
        {
            throw new InvalidOperationException($"Plugin type does not implement IPlugin: {typeName}");
        }

        var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;

        var pluginLogger = _loggerFactory.CreateLogger(manifest.Id);
        var pluginHost = new PluginHost(_loggerFactory, _services);
        var pluginContext = new PluginContext(_serviceRegistry, _configuration, pluginLogger, pluginHost);

        await plugin.InitializeAsync(pluginContext, ct);
        _pluginRegistry.UpdateState(manifest.Id, PluginState.Initialized);

        await plugin.StartAsync(ct);
        _pluginRegistry.UpdateState(manifest.Id, PluginState.Started);

        var loadedPlugin = new LoadedPlugin
        {
            Plugin = plugin,
            LoadContext = loadContext,
            Manifest = manifest,
            Logger = pluginLogger
        };
        _loadedPlugins[manifest.Id] = loadedPlugin;

        if (pluginMetrics != null)
        {
            pluginMetrics.Version = manifest.Version;
            pluginMetrics.DependencyCount = manifest.Dependencies?.Count ?? 0;
            _metrics?.CompletePluginLoad(pluginMetrics, true);
        }

        sw.Stop();
        _logger.LogInformation("Plugin loaded: {PluginId} in {ElapsedMs}ms", manifest.Id, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Stop and unload all plugins.
    /// </summary>
    public async Task UnloadAllAsync(CancellationToken ct = default)
    {
        var pluginIds = _loadedPlugins.Keys.ToList();
        foreach (var pluginId in pluginIds)
        {
            await UnloadPluginAsync(pluginId, ct);
        }
    }

    /// <summary>
    /// Unload a specific plugin.
    /// </summary>
    public async Task UnloadPluginAsync(string pluginId, CancellationToken ct = default)
    {
        if (!_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
        {
            return;
        }

        _logger.LogInformation("Unloading plugin: {PluginId}", pluginId);

        try
        {
            await loadedPlugin.Plugin.StopAsync(ct);
            _pluginRegistry.UpdateState(pluginId, PluginState.Stopped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping plugin: {PluginId}", pluginId);
        }

        _loadedPlugins.Remove(pluginId);

        if (_enableHotReload && loadedPlugin.LoadContext != null)
        {
            loadedPlugin.LoadContext.Unload();

            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(100, ct);
            }
        }

        _pluginRegistry.UpdateState(pluginId, PluginState.Unloaded);
        _logger.LogInformation("Plugin unloaded: {PluginId}", pluginId);
    }

    /// <summary>
    /// Reload a plugin (unload then load again).
    /// </summary>
    public async Task ReloadPluginAsync(string pluginId, CancellationToken ct = default)
    {
        if (!_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
        {
            throw new InvalidOperationException($"Plugin not loaded: {pluginId}");
        }

        var pluginDir = Path.GetDirectoryName(loadedPlugin.LoadContext?.Name);
        if (string.IsNullOrEmpty(pluginDir))
        {
            throw new InvalidOperationException($"Cannot determine plugin directory for: {pluginId}");
        }

        var manifest = loadedPlugin.Manifest;

        await UnloadPluginAsync(pluginId, ct);
        await LoadPluginAsync(pluginDir, manifest, ct);

        _logger.LogInformation("Plugin reloaded: {PluginId}", pluginId);
    }

    private string DetermineProfile()
    {
        return _profile;
    }

    /// <summary>
    /// Preload contract assemblies into Default ALC before plugin loading.
    /// This ensures all plugins reference the same type definitions.
    /// </summary>
    private void PreloadContractAssemblies()
    {
        var baseDir = AppContext.BaseDirectory;

        if (!Directory.Exists(baseDir))
        {
            _logger.LogWarning("Base directory does not exist: {BaseDir}", baseDir);
            return;
        }

        // Discover all contract DLLs in the base directory
        var contractPattern = "*.Contracts.dll";
        var contractFiles = Directory.GetFiles(baseDir, contractPattern, SearchOption.TopDirectoryOnly);

        // Also preload shared UI libs to ensure type identity across ALCs
        var sharedLibs = new[] { "Terminal.Gui.dll" };
        foreach (var lib in sharedLibs)
        {
            var p = Path.Combine(baseDir, lib);
            if (File.Exists(p))
            {
                contractFiles = contractFiles.Append(p).ToArray();
            }
        }

        _logger.LogInformation("Preloading {Count} contract assemblies from {BaseDir}", contractFiles.Length, baseDir);

        foreach (var contractPath in contractFiles)
        {
            try
            {
                var contractName = Path.GetFileNameWithoutExtension(contractPath);

                // Check if already loaded
                var alreadyLoaded = AssemblyLoadContext.Default.Assemblies
                    .Any(a => string.Equals(a.GetName().Name, contractName, StringComparison.OrdinalIgnoreCase));

                if (alreadyLoaded)
                {
                    _logger.LogDebug("Contract assembly already loaded: {ContractName}", contractName);
                    continue;
                }

                // Load into Default ALC
                AssemblyLoadContext.Default.LoadFromAssemblyPath(contractPath);
                _logger.LogInformation("Preloaded contract assembly: {ContractName}", contractName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to preload contract assembly: {ContractPath}", contractPath);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        var task = UnloadAllAsync(CancellationToken.None);
        task.Wait();

        _disposed = true;
    }

    private sealed class LoadedPlugin
    {
        public required IPlugin Plugin { get; init; }
        public required PluginLoadContext? LoadContext { get; init; }
        public required PluginManifest Manifest { get; init; }
        public required ILogger Logger { get; init; }
    }
}
