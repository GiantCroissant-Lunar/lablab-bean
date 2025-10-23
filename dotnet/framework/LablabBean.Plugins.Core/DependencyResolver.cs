namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Resolves plugin dependencies using Kahn's topological sort algorithm.
/// Handles hard/soft dependencies, cycle detection, and version selection.
/// </summary>
public sealed class DependencyResolver
{
    private readonly ILogger _logger;

    public DependencyResolver(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ResolveResult Resolve(IReadOnlyList<PluginManifest> manifests)
    {
        var pluginMap = manifests.ToDictionary(m => m.Id);
        var result = new ResolveResult();

        var availableIds = new HashSet<string>(manifests.Select(m => m.Id));

        foreach (var manifest in manifests)
        {
            var missingHardDeps = new List<string>();
            var missingSoftDeps = new List<string>();

            foreach (var dep in manifest.Dependencies)
            {
                if (!availableIds.Contains(dep.Id))
                {
                    if (dep.Optional)
                    {
                        missingSoftDeps.Add(dep.Id);
                    }
                    else
                    {
                        missingHardDeps.Add(dep.Id);
                    }
                }
            }

            if (missingHardDeps.Count > 0)
            {
                var missing = string.Join(", ", missingHardDeps);
                _logger.LogError("Plugin {PluginId} excluded: missing hard dependencies: {MissingDeps}",
                    manifest.Id, missing);
                result.ExcludedPlugins.Add(manifest.Id);
                result.FailureReasons[manifest.Id] = $"Missing hard dependencies: {missing}";
                continue;
            }

            if (missingSoftDeps.Count > 0)
            {
                var missing = string.Join(", ", missingSoftDeps);
                _logger.LogWarning("Plugin {PluginId} has missing soft dependencies: {MissingDeps}",
                    manifest.Id, missing);
            }
        }

        var loadableManifests = manifests
            .Where(m => !result.ExcludedPlugins.Contains(m.Id))
            .ToList();

        if (loadableManifests.Count == 0)
        {
            return result;
        }

        var sorted = TopologicalSort(loadableManifests, _logger);

        if (sorted.Count != loadableManifests.Count)
        {
            var cycles = DetectCycles(loadableManifests);
            var cycleInfo = string.Join(", ", cycles.Select(c => string.Join(" -> ", c)));
            throw new InvalidOperationException($"Circular dependencies detected: {cycleInfo}");
        }

        result.LoadOrder = sorted;
        return result;
    }

    private static List<string> TopologicalSort(List<PluginManifest> manifests, ILogger logger)
    {
        var pluginMap = manifests.ToDictionary(m => m.Id);
        var inDegree = new Dictionary<string, int>();
        var graph = new Dictionary<string, List<string>>();

        foreach (var manifest in manifests)
        {
            inDegree[manifest.Id] = 0;
            graph[manifest.Id] = new List<string>();
        }

        foreach (var manifest in manifests)
        {
            foreach (var dep in manifest.Dependencies)
            {
                if (!dep.Optional && pluginMap.ContainsKey(dep.Id))
                {
                    graph[dep.Id].Add(manifest.Id);
                    inDegree[manifest.Id]++;
                }
            }
        }

        var queue = new Queue<string>();
        foreach (var kvp in inDegree.Where(kvp => kvp.Value == 0))
        {
            queue.Enqueue(kvp.Key);
        }

        var sorted = new List<string>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sorted.Add(current);

            foreach (var neighbor in graph[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return sorted;
    }

    private static List<List<string>> DetectCycles(List<PluginManifest> manifests)
    {
        var cycles = new List<List<string>>();
        var pluginMap = manifests.ToDictionary(m => m.Id);
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var currentPath = new List<string>();

        void Dfs(string id)
        {
            if (recursionStack.Contains(id))
            {
                var cycleStart = currentPath.IndexOf(id);
                cycles.Add(currentPath.Skip(cycleStart).Concat(new[] { id }).ToList());
                return;
            }

            if (visited.Contains(id)) return;

            visited.Add(id);
            recursionStack.Add(id);
            currentPath.Add(id);

            if (pluginMap.TryGetValue(id, out var manifest))
            {
                foreach (var dep in manifest.Dependencies.Where(d => !d.Optional))
                {
                    if (pluginMap.ContainsKey(dep.Id))
                    {
                        Dfs(dep.Id);
                    }
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            recursionStack.Remove(id);
        }

        foreach (var manifest in manifests)
        {
            if (!visited.Contains(manifest.Id))
            {
                Dfs(manifest.Id);
            }
        }

        return cycles;
    }

    public sealed class ResolveResult
    {
        public List<string> LoadOrder { get; set; } = new();
        public HashSet<string> ExcludedPlugins { get; } = new();
        public Dictionary<string, string> FailureReasons { get; } = new();
    }
}
