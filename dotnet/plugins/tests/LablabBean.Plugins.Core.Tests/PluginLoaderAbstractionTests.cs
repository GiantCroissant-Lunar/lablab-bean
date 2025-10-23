using FluentAssertions;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LablabBean.Plugins.Core.Tests;

/// <summary>
/// Tests for IPluginLoader abstraction and factory pattern.
/// </summary>
public class PluginLoaderAbstractionTests
{
    [Fact]
    public void IPluginLoader_DefinesRequiredMembers()
    {
        // Arrange & Act
        var loaderType = typeof(IPluginLoader);
        var properties = loaderType.GetProperties();
        var methods = loaderType.GetMethods();

        // Assert
        properties.Should().Contain(p => p.Name == "PluginRegistry");
        properties.Should().Contain(p => p.Name == "ServiceRegistry");

        methods.Should().Contain(m => m.Name == "DiscoverAndLoadAsync");
        methods.Should().Contain(m => m.Name == "UnloadPluginAsync");
        methods.Should().Contain(m => m.Name == "UnloadAllAsync");
    }

    [Fact]
    public void PluginLoader_ImplementsIPluginLoader()
    {
        // Arrange & Act
        var loaderType = typeof(PluginLoader);

        // Assert
        loaderType.Should().Implement<IPluginLoader>();
    }

    [Fact]
    public void PluginLoaderFactory_CreatesValidLoader()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());
        var serviceProvider = services.BuildServiceProvider();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var configuration = new ConfigurationBuilder().Build();
        var pluginRegistry = new PluginRegistry();
        var serviceRegistry = new ServiceRegistry();

        // Act
        var loader = PluginLoaderFactory.Create(
            logger,
            loggerFactory,
            configuration,
            serviceProvider,
            pluginRegistry,
            serviceRegistry);

        // Assert
        loader.Should().NotBeNull();
        loader.Should().BeAssignableTo<IPluginLoader>();
        loader.PluginRegistry.Should().BeSameAs(pluginRegistry);
        loader.ServiceRegistry.Should().BeSameAs(serviceRegistry);
    }

    [Fact]
    public void PluginLoaderFactory_CreatesAlcLoaderForDotNetPlatform()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());
        var serviceProvider = services.BuildServiceProvider();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var configuration = new ConfigurationBuilder().Build();
        var pluginRegistry = new PluginRegistry();
        var serviceRegistry = new ServiceRegistry();

        // Act
        var loader = PluginLoaderFactory.Create(
            logger,
            loggerFactory,
            configuration,
            serviceProvider,
            pluginRegistry,
            serviceRegistry);

        // Assert - Currently returns ALC-based PluginLoader for .NET
        loader.Should().BeOfType<PluginLoader>();
    }
}
