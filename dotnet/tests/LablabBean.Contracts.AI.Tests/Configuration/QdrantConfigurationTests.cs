using FluentAssertions;
using LablabBean.AI.Agents.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LablabBean.Contracts.AI.Tests.Configuration;

/// <summary>
/// Unit tests for Qdrant configuration validation
/// </summary>
public class QdrantConfigurationTests
{
    [Fact]
    public void QdrantConfig_WithValidSettings_ShouldLoadCorrectly()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "KernelMemory:Storage:Provider", "Qdrant" },
                { "KernelMemory:Storage:ConnectionString", "http://localhost:6333" },
                { "KernelMemory:Storage:CollectionName", "game_memories" }
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KernelMemoryOptions>>();

        // Assert
        options.Value.Should().NotBeNull();
        options.Value.Storage.Provider.Should().Be("Qdrant");
        options.Value.Storage.ConnectionString.Should().Be("http://localhost:6333");
        options.Value.Storage.CollectionName.Should().Be("game_memories");
    }

    [Fact]
    public void QdrantConfig_WithMissingConnectionString_ShouldHaveNullValue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "KernelMemory:Storage:Provider", "Qdrant" },
                { "KernelMemory:Storage:CollectionName", "game_memories" }
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KernelMemoryOptions>>();

        // Assert
        options.Value.Storage.ConnectionString.Should().BeNull();
    }

    [Fact]
    public void QdrantConfig_WithCustomEndpoint_ShouldUseCustomValue()
    {
        // Arrange
        var customEndpoint = "http://qdrant.example.com:6333";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "KernelMemory:Storage:Provider", "Qdrant" },
                { "KernelMemory:Storage:ConnectionString", customEndpoint },
                { "KernelMemory:Storage:CollectionName", "production_memories" }
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KernelMemoryOptions>>();

        // Assert
        options.Value.Storage.ConnectionString.Should().Be(customEndpoint);
        options.Value.Storage.CollectionName.Should().Be("production_memories");
    }

    [Fact]
    public void StorageOptions_DefaultProvider_ShouldBeVolatile()
    {
        // Arrange
        var options = new StorageOptions();

        // Assert
        options.Provider.Should().Be("Volatile");
        options.ConnectionString.Should().BeNull();
        options.CollectionName.Should().BeNull();
    }

    [Theory]
    [InlineData("Qdrant")]
    [InlineData("AzureAISearch")]
    [InlineData("Volatile")]
    public void StorageOptions_ShouldAcceptValidProviders(string provider)
    {
        // Arrange & Act
        var options = new StorageOptions { Provider = provider };

        // Assert
        options.Provider.Should().Be(provider);
    }

    [Fact]
    public void KernelMemoryOptions_ShouldHaveNestedConfigStructure()
    {
        // Arrange
        var options = new KernelMemoryOptions
        {
            Storage = new StorageOptions
            {
                Provider = "Qdrant",
                ConnectionString = "http://localhost:6333",
                CollectionName = "test_collection"
            },
            Embedding = new EmbeddingOptions
            {
                Provider = "OpenAI",
                ModelName = "text-embedding-ada-002"
            },
            TextGeneration = new TextGenerationOptions
            {
                Provider = "OpenAI",
                ModelName = "gpt-4"
            }
        };

        // Assert
        options.Storage.Should().NotBeNull();
        options.Storage.Provider.Should().Be("Qdrant");
        options.Embedding.Should().NotBeNull();
        options.TextGeneration.Should().NotBeNull();
    }

    [Fact]
    public void QdrantConfig_WithAllOptions_ShouldLoadCompletely()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "KernelMemory:Storage:Provider", "Qdrant" },
                { "KernelMemory:Storage:ConnectionString", "http://localhost:6333" },
                { "KernelMemory:Storage:CollectionName", "game_memories" },
                { "KernelMemory:Embedding:Provider", "OpenAI" },
                { "KernelMemory:Embedding:ModelName", "text-embedding-ada-002" },
                { "KernelMemory:Embedding:MaxTokens", "8191" },
                { "KernelMemory:TextGeneration:Provider", "OpenAI" },
                { "KernelMemory:TextGeneration:ModelName", "gpt-4" },
                { "KernelMemory:TextGeneration:MaxTokens", "4096" }
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KernelMemoryOptions>>();

        // Assert
        var value = options.Value;
        value.Storage.Provider.Should().Be("Qdrant");
        value.Storage.ConnectionString.Should().Be("http://localhost:6333");
        value.Storage.CollectionName.Should().Be("game_memories");
        value.Embedding.Provider.Should().Be("OpenAI");
        value.Embedding.ModelName.Should().Be("text-embedding-ada-002");
        value.Embedding.MaxTokens.Should().Be(8191);
        value.TextGeneration.Provider.Should().Be("OpenAI");
        value.TextGeneration.ModelName.Should().Be("gpt-4");
        value.TextGeneration.MaxTokens.Should().Be(4096);
    }

    [Fact]
    public void QdrantConfig_FromJsonFormat_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""KernelMemory"": {
                ""Storage"": {
                    ""Provider"": ""Qdrant"",
                    ""ConnectionString"": ""http://localhost:6333"",
                    ""CollectionName"": ""game_memories""
                },
                ""Embedding"": {
                    ""Provider"": ""OpenAI"",
                    ""ModelName"": ""text-embedding-ada-002""
                },
                ""TextGeneration"": {
                    ""Provider"": ""OpenAI"",
                    ""ModelName"": ""gpt-4""
                }
            }
        }";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        var services = new ServiceCollection();
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KernelMemoryOptions>>();

        // Assert
        options.Value.Storage.Provider.Should().Be("Qdrant");
        options.Value.Storage.ConnectionString.Should().Be("http://localhost:6333");
    }
}
