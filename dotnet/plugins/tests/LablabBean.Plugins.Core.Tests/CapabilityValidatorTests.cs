namespace LablabBean.Plugins.Core.Tests;

using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CapabilityValidatorTests
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public CapabilityValidatorTests()
    {
        _logger = NullLogger.Instance;

        var configData = new Dictionary<string, string?>
        {
            ["Plugins:StrictCapabilityMode"] = "true"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void Validate_WithSingleUIAndRenderer_LoadsBoth()
    {
        // Arrange
        var manifests = new List<PluginManifest>
        {
            CreateManifest("ui-terminal", new[] { "ui", "ui:terminal" }),
            CreateManifest("rendering-terminal", new[] { "renderer", "renderer:terminal" })
        };

        var validator = new CapabilityValidator(_logger, _configuration);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Equal(2, result.ManifestsToLoad.Count);
        Assert.Empty(result.ExcludedPlugins);
    }

    [Fact]
    public void Validate_WithMultipleUI_ExcludesAllButOne()
    {
        // Arrange
        var manifests = new List<PluginManifest>
        {
            CreateManifest("ui-terminal", new[] { "ui", "ui:terminal" }, priority: 100),
            CreateManifest("ui-sadconsole", new[] { "ui", "ui:windows" }, priority: 90)
        };

        var validator = new CapabilityValidator(_logger, _configuration);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Single(result.ManifestsToLoad);
        Assert.Equal("ui-terminal", result.ManifestsToLoad[0].Id);
        Assert.Single(result.ExcludedPlugins);
        Assert.True(result.ExcludedPlugins.ContainsKey("ui-sadconsole"));
    }

    [Fact]
    public void Validate_WithMultipleRenderers_ExcludesAllButOne()
    {
        // Arrange
        var manifests = new List<PluginManifest>
        {
            CreateManifest("rendering-terminal", new[] { "renderer", "renderer:terminal" }, priority: 100),
            CreateManifest("rendering-sadconsole", new[] { "renderer", "renderer:sadconsole" }, priority: 90)
        };

        var validator = new CapabilityValidator(_logger, _configuration);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Single(result.ManifestsToLoad);
        Assert.Equal("rendering-terminal", result.ManifestsToLoad[0].Id);
        Assert.Single(result.ExcludedPlugins);
        Assert.True(result.ExcludedPlugins.ContainsKey("rendering-sadconsole"));
    }

    [Fact]
    public void Validate_WithPreferredUI_SelectsPreferredOverPriority()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Plugins:PreferredUI"] = "ui-sadconsole",
            ["Plugins:StrictCapabilityMode"] = "true"
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var manifests = new List<PluginManifest>
        {
            CreateManifest("ui-terminal", new[] { "ui", "ui:terminal" }, priority: 100),
            CreateManifest("ui-sadconsole", new[] { "ui", "ui:windows" }, priority: 90)
        };

        var validator = new CapabilityValidator(_logger, config);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Single(result.ManifestsToLoad);
        Assert.Equal("ui-sadconsole", result.ManifestsToLoad[0].Id);
        Assert.Single(result.ExcludedPlugins);
        Assert.True(result.ExcludedPlugins.ContainsKey("ui-terminal"));
    }

    [Fact]
    public void Validate_WithNonUIPlugins_LoadsAll()
    {
        // Arrange
        var manifests = new List<PluginManifest>
        {
            CreateManifest("inventory", new[] { "gameplay", "inventory" }),
            CreateManifest("quest", new[] { "gameplay", "quest" }),
            CreateManifest("npc", new[] { "gameplay", "npc" })
        };

        var validator = new CapabilityValidator(_logger, _configuration);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Equal(3, result.ManifestsToLoad.Count);
        Assert.Empty(result.ExcludedPlugins);
    }

    [Fact]
    public void Validate_MixedUIAndGameplay_OnlyRestrictsUI()
    {
        // Arrange
        var manifests = new List<PluginManifest>
        {
            CreateManifest("ui-terminal", new[] { "ui", "ui:terminal" }),
            CreateManifest("ui-sadconsole", new[] { "ui", "ui:windows" }),
            CreateManifest("inventory", new[] { "gameplay", "inventory" }),
            CreateManifest("quest", new[] { "gameplay", "quest" })
        };

        var validator = new CapabilityValidator(_logger, _configuration);

        // Act
        var result = validator.Validate(manifests);

        // Assert
        Assert.Equal(3, result.ManifestsToLoad.Count); // 1 UI + 2 gameplay
        Assert.Single(result.ExcludedPlugins);
        var loadedIds = result.ManifestsToLoad.Select(m => m.Id).ToList();
        Assert.Contains("inventory", loadedIds);
        Assert.Contains("quest", loadedIds);
    }

    private PluginManifest CreateManifest(
        string id,
        string[] capabilities,
        int priority = 100)
    {
        return new PluginManifest
        {
            Id = id,
            Name = id,
            Version = "1.0.0",
            Capabilities = capabilities.ToList(),
            Priority = priority,
            EntryAssembly = $"{id}.dll",
            EntryType = $"{id}.Plugin"
        };
    }
}
