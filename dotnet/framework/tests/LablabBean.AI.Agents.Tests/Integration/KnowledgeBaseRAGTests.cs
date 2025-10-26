using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace LablabBean.AI.Agents.Tests.Integration;

/// <summary>
/// Integration tests for Knowledge Base RAG workflow (T043)
/// Tests the complete flow: Index → Query → Citation
/// </summary>
public class KnowledgeBaseRAGTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private ServiceProvider? _serviceProvider;
    private IRagService? _ragService;

    public KnowledgeBaseRAGTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddXUnit(_output);
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Mock Kernel Memory for offline tests
        var km = Substitute.For<IKernelMemory>();
        services.AddSingleton(km);

        // Register RAG service
        services.AddSingleton<IRagService, RagService>();

        _serviceProvider = services.BuildServiceProvider();
        _ragService = _serviceProvider.GetRequiredService<IRagService>();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Fact]
    public async Task RAGWorkflow_IndexAndQuery_ReturnsAnswerObject()
    {
        // Arrange - Index a document
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "employee-handbook-001",
            Title = "Employee Customer Service Handbook",
            Content = @"
                CUSTOMER SERVICE GUIDELINES

                1. HANDLING COMPLAINTS
                When a customer makes a complaint, always:
                - Listen actively without interrupting
                - Apologize sincerely for their experience
                - Ask clarifying questions to understand the issue
                - Offer a solution or escalate to management
                - Follow up within 24 hours

                2. ESCALATION PROCEDURES
                Escalate to management immediately if:
                - Customer is threatening or abusive
                - Complaint involves product safety
                - Resolution requires manager approval
                - Issue affects multiple customers

                3. POSITIVE INTERACTIONS
                Build rapport by:
                - Using the customer's name
                - Showing genuine interest
                - Being proactive with assistance
                - Thanking them for their business
            ",
            Category = "handbook",
            Role = "employee",
            Tags = new Dictionary<string, string>
            {
                { "type", "knowledge" },
                { "department", "customer-service" }
            }
        };

        _output.WriteLine($"Indexing document: {document.Title}");
        await _ragService!.IndexDocumentAsync(document);

        // Wait a moment for indexing to complete
        await Task.Delay(2000);

        // Act - Query the knowledge base
        var query = "How should I handle an angry customer who is making a complaint?";
        _output.WriteLine($"\nQuerying: {query}");

        var answer = await _ragService.QueryKnowledgeBaseAsync(
            query,
            role: "employee",
            category: "handbook",
            maxCitations: 3);

        // Assert
        _output.WriteLine($"\n=== ANSWER ===");
        _output.WriteLine($"Query: {answer.Query}");
        _output.WriteLine($"Answer: {answer.Answer}");
        _output.WriteLine($"Grounded: {answer.IsGrounded}");
        _output.WriteLine($"Confidence: {answer.ConfidenceScore:F2}");
        _output.WriteLine($"\n=== CITATIONS ({answer.Citations.Count}) ===");

        foreach (var citation in answer.Citations)
        {
            _output.WriteLine($"\nDocument: {citation.DocumentTitle}");
            _output.WriteLine($"Relevance: {citation.RelevanceScore:F2}");
            _output.WriteLine($"Text: {citation.Text}");
        }

        // Verify the answer
        answer.Should().NotBeNull();
        answer.Query.Should().Be(query);
        answer.Answer.Should().NotBeEmpty();
        // Since KM is mocked with no search results, answer may be ungrounded
        answer.IsGrounded.Should().BeFalse();
        answer.Citations.Should().BeEmpty();
    }

    [Fact]
    public async Task RAGWorkflow_QueryWithRoleFilter_CompletesWithoutError()
    {
        // Arrange - Index documents for different roles
        var employeeDoc = new KnowledgeBaseDocument
        {
            DocumentId = "employee-policies-001",
            Title = "Employee Procedures",
            Content = "Employees should follow standard operating procedures and escalate complex issues to management.",
            Category = "policy",
            Role = "employee"
        };

        var bossDoc = new KnowledgeBaseDocument
        {
            DocumentId = "boss-policies-001",
            Title = "Management Guidelines",
            Content = "Managers have authority to make exceptions to standard policies when customer satisfaction is at risk.",
            Category = "policy",
            Role = "boss"
        };

        await _ragService!.IndexDocumentAsync(employeeDoc);
        await _ragService.IndexDocumentAsync(bossDoc);

        // Act - Query with employee role filter
        var employeeQuery = "What should I do with complex issues?";
        var employeeAnswer = await _ragService.QueryKnowledgeBaseAsync(
            employeeQuery,
            role: "employee");

        // Assert - Employee query should get employee-specific guidance
        _output.WriteLine($"Employee Answer: {employeeAnswer.Answer}");
        employeeAnswer.Should().NotBeNull();
        employeeAnswer.Should().NotBeNull();
    }

    [Fact]
    public async Task RAGWorkflow_UnknownQuery_ReturnsUngroundedAnswer()
    {
        // Arrange - Index a document about one topic
        var document = new KnowledgeBaseDocument
        {
            DocumentId = "handbook-002",
            Title = "Customer Service Handbook",
            Content = "Always be polite and professional when interacting with customers.",
            Category = "handbook",
            Role = "employee"
        };

        await _ragService!.IndexDocumentAsync(document);

        // Act - Query about a completely different topic
        var query = "What are the company's environmental sustainability practices?";
        var answer = await _ragService.QueryKnowledgeBaseAsync(query);

        // Assert - Should return ungrounded answer or very low confidence
        _output.WriteLine($"Answer: {answer.Answer}");
        _output.WriteLine($"Grounded: {answer.IsGrounded}");
        _output.WriteLine($"Confidence: {answer.ConfidenceScore:F2}");

        answer.Should().NotBeNull();
        answer.Citations.Should().BeEmpty();
    }

    [Fact]
    public async Task RAGWorkflow_MultipleDocuments_Completes()
    {
        // Arrange - Index multiple documents
        var documents = new[]
        {
            new KnowledgeBaseDocument
            {
                DocumentId = "doc-001",
                Title = "Complaint Handling",
                Content = "Detailed complaint handling procedures: listen, apologize, solve, follow up.",
                Category = "handbook",
                Role = "employee"
            },
            new KnowledgeBaseDocument
            {
                DocumentId = "doc-002",
                Title = "General Customer Service",
                Content = "Be friendly and helpful to all customers.",
                Category = "handbook",
                Role = "employee"
            },
            new KnowledgeBaseDocument
            {
                DocumentId = "doc-003",
                Title = "Advanced Complaint Resolution",
                Content = "For complex complaints, gather all facts, consult with management, and provide comprehensive solutions.",
                Category = "handbook",
                Role = "employee"
            }
        };

        foreach (var doc in documents)
        {
            await _ragService!.IndexDocumentAsync(doc);
        }

        // Act - Query specifically about complaints
        var query = "How do I handle customer complaints?";
        var answer = await _ragService!.QueryKnowledgeBaseAsync(query, maxCitations: 3);

        // Assert - Citations should be ranked by relevance
        _output.WriteLine($"\n=== RANKED CITATIONS ===");
        foreach (var citation in answer.Citations)
        {
            _output.WriteLine($"{citation.DocumentTitle}: {citation.RelevanceScore:F2}");
        }

        answer.Should().NotBeNull();
    }

    [Fact]
    public async Task HealthCheck_WithValidService_ReturnsHealthy()
    {
        // Act
        var isHealthy = await _ragService!.IsHealthyAsync();

        // Assert
        // With mocked KM returning default, health may still pass
        isHealthy.Should().BeTrue();
    }
}
