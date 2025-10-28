namespace LablabBean.Plugins.Core;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validates plugin capabilities and enforces single-instance policies for UI and renderer plugins.
/// </summary>
public sealed class CapabilityValidator
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public CapabilityValidator(ILogger logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Validates that only one UI and one renderer plugin are selected.
    /// Returns the filtered list of manifests to load and excluded manifests with reasons.
    /// </summary>
    public ValidationResult Validate(IReadOnlyList<PluginManifest> manifests)
    {
        var result = new ValidationResult();

        // Get configuration preferences
        var preferredUi = _configuration["Plugins:PreferredUI"];
        var preferredRenderer = _configuration["Plugins:PreferredRenderer"];
        var strictMode = _configuration.GetValue("Plugins:StrictCapabilityMode", true);
        var skipList = _configuration.GetSection("Plugins:Skip").Get<string[]>() ?? Array.Empty<string>();
        var onlyList = _configuration.GetSection("Plugins:Only").Get<string[]>() ?? Array.Empty<string>();

        // Apply Skip filter first
        var afterSkip = manifests.ToList();
        foreach (var skipId in skipList)
        {
            var skipped = afterSkip.FirstOrDefault(m => m.Id == skipId);
            if (skipped != null)
            {
                result.ExcludedPlugins[skipId] = $"Excluded via Plugins:Skip configuration";
                afterSkip.Remove(skipped);
            }
        }

        // Apply Only filter if specified
        if (onlyList.Length > 0)
        {
            var toExclude = afterSkip.Where(m => !onlyList.Contains(m.Id)).ToList();
            foreach (var excluded in toExclude)
            {
                result.ExcludedPlugins[excluded.Id] = $"Not in Plugins:Only list";
                afterSkip.Remove(excluded);
            }
        }

        // Group plugins by capability type
        var uiPlugins = afterSkip
            .Where(m => m.Capabilities.Any(c => c.StartsWith("ui")))
            .ToList();

        var rendererPlugins = afterSkip
            .Where(m => m.Capabilities.Any(c => c.StartsWith("renderer")))
            .ToList();

        // Validate and select UI plugin
        var selectedUi = SelectSinglePlugin(
            uiPlugins,
            preferredUi,
            "ui",
            strictMode,
            result);

        // Validate and select Renderer plugin
        var selectedRenderer = SelectSinglePlugin(
            rendererPlugins,
            preferredRenderer,
            "renderer",
            strictMode,
            result);

        // Build final list of plugins to load
        var toLoad = afterSkip
            .Where(m => !result.ExcludedPlugins.ContainsKey(m.Id))
            .ToList();

        result.ManifestsToLoad = toLoad;
        result.SelectedUi = selectedUi?.Id;
        result.SelectedRenderer = selectedRenderer?.Id;

        // Log selection summary once
        LogSelectionSummary(result);

        return result;
    }

    private PluginManifest? SelectSinglePlugin(
        List<PluginManifest> candidates,
        string? preferredId,
        string capabilityPrefix,
        bool strictMode,
        ValidationResult result)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            var single = candidates[0];
            _logger.LogInformation(
                "Selected single {CapabilityType} plugin: {PluginId}",
                capabilityPrefix,
                single.Id);
            return single;
        }

        // Multiple candidates found
        PluginManifest? selected = null;

        // First: try preferred plugin from config
        if (!string.IsNullOrWhiteSpace(preferredId))
        {
            selected = candidates.FirstOrDefault(p => p.Id == preferredId);
            if (selected != null)
            {
                _logger.LogInformation(
                    "Selected preferred {CapabilityType} plugin from config: {PluginId}",
                    capabilityPrefix,
                    selected.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Preferred {CapabilityType} plugin '{PreferredId}' not found",
                    capabilityPrefix,
                    preferredId);
            }
        }

        // Second: select by highest priority
        if (selected == null)
        {
            selected = candidates.OrderByDescending(p => p.Priority).First();
            _logger.LogInformation(
                "Selected {CapabilityType} plugin by priority: {PluginId} (priority: {Priority})",
                capabilityPrefix,
                selected.Id,
                selected.Priority);
        }

        // Exclude all other candidates
        foreach (var candidate in candidates.Where(c => c.Id != selected.Id))
        {
            var reason = strictMode
                ? $"Only one {capabilityPrefix} plugin allowed; '{selected.Id}' was selected"
                : $"Multiple {capabilityPrefix} plugins found; '{selected.Id}' was selected by priority";

            result.ExcludedPlugins[candidate.Id] = reason;

            if (strictMode)
            {
                _logger.LogError(
                    "Excluded {CapabilityType} plugin {PluginId}: {Reason}",
                    capabilityPrefix,
                    candidate.Id,
                    reason);
            }
            else
            {
                _logger.LogWarning(
                    "Excluded {CapabilityType} plugin {PluginId}: {Reason}",
                    capabilityPrefix,
                    candidate.Id,
                    reason);
            }
        }

        return selected;
    }

    private void LogSelectionSummary(ValidationResult result)
    {
        var summary = "Plugin capability selection: ";
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(result.SelectedUi))
        {
            parts.Add($"UI={result.SelectedUi}");
        }

        if (!string.IsNullOrEmpty(result.SelectedRenderer))
        {
            parts.Add($"Renderer={result.SelectedRenderer}");
        }

        summary += string.Join(", ", parts);

        if (result.ExcludedPlugins.Count > 0)
        {
            var excluded = string.Join(", ", result.ExcludedPlugins.Keys);
            summary += $"; Excluded: {excluded}";
        }

        _logger.LogInformation(summary);
    }

    public sealed class ValidationResult
    {
        /// <summary>
        /// Plugins that passed validation and should be loaded.
        /// </summary>
        public List<PluginManifest> ManifestsToLoad { get; set; } = new();

        /// <summary>
        /// Plugins excluded due to capability policy violations.
        /// Key: plugin ID, Value: exclusion reason.
        /// </summary>
        public Dictionary<string, string> ExcludedPlugins { get; } = new();

        /// <summary>
        /// ID of the selected UI plugin, if any.
        /// </summary>
        public string? SelectedUi { get; set; }

        /// <summary>
        /// ID of the selected Renderer plugin, if any.
        /// </summary>
        public string? SelectedRenderer { get; set; }
    }
}
