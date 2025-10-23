namespace LablabBean.Plugins.Core.Tests;

using FluentAssertions;
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

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
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().BuildServiceProvider();
        var pluginRegistry = new PluginRegistry();
        var serviceRegistry = new ServiceRegistry(new EventBus());

        // Act
        var loader = PluginLoaderFactory.Create(
            logger,
            loggerFactory,
            configuration,
            services,
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
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().BuildServiceProvider();
        var pluginRegistry = new PluginRegistry();
        var serviceRegistry = new ServiceRegistry(new EventBus());

        // Act
        var loader = PluginLoaderFactory.Create(
            logger,
            loggerFactory,
            configuration,
            services,
            pluginRegistry,
            serviceRegistry);

        // Assert - Currently returns ALC-based PluginLoader for .NET
        loader.Should().BeOfType<PluginLoader>();
    }
}
