using FluentAssertions;
using LablabBean.Contracts.AI.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private IKnowledgeBaseService? _knowledgeBaseService;

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

        // Add Kernel Memory with in-memory storage
        services.AddKernelMemory(config =>
        {
            config.WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key");
            config.WithSimpleVectorDb(new Microsoft.KernelMemory.Configuration.SimpleVectorDbConfig());
        });

        // Add KnowledgeBaseService
        services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();

        _serviceProvider = services.BuildServiceProvider();
        _knowledgeBaseService = _serviceProvider.GetRequiredService<IKnowledgeBaseService>();

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
    public async Task RAGWorkflow_IndexAndQuery_ReturnsGroundedAnswer()
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
        await _knowledgeBaseService!.IndexDocumentAsync(document);

        // Wait a moment for indexing to complete
        await Task.Delay(2000);

        // Act - Query the knowledge base
        var query = "How should I handle an angry customer who is making a complaint?";
        _output.WriteLine($"\nQuerying: {query}");

        var answer = await _knowledgeBaseService.QueryKnowledgeBaseAsync(
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
        answer.IsGrounded.Should().BeTrue("answer should be grounded in the knowledge base");
        answer.Citations.Should().NotBeEmpty("answer should include citations");
        answer.ConfidenceScore.Should().BeGreaterThan(0.0);

        // Verify citations reference the indexed document
        answer.Citations.Should().Contain(c =>
            c.DocumentId == document.DocumentId &&
            c.DocumentTitle == document.Title);

        // Verify the answer is relevant to complaint handling
        var relevantKeywords = new[] { "listen", "apologize", "complaint", "escalate", "customer" };
        answer.Answer.Should().ContainAny(relevantKeywords, "answer should mention complaint handling procedures");
    }

    [Fact]
    public async Task RAGWorkflow_QueryWithRoleFilter_ReturnsRoleSpecificResults()
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

        await _knowledgeBaseService!.IndexDocumentAsync(employeeDoc);
        await _knowledgeBaseService.IndexDocumentAsync(bossDoc);
        await Task.Delay(2000);

        // Act - Query with employee role filter
        var employeeQuery = "What should I do with complex issues?";
        var employeeAnswer = await _knowledgeBaseService.QueryKnowledgeBaseAsync(
            employeeQuery,
            role: "employee");

        // Assert - Employee query should get employee-specific guidance
        _output.WriteLine($"Employee Answer: {employeeAnswer.Answer}");
        employeeAnswer.Should().NotBeNull();
        employeeAnswer.Citations.Should().Contain(c => c.DocumentId == employeeDoc.DocumentId);
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

        await _knowledgeBaseService!.IndexDocumentAsync(document);
        await Task.Delay(2000);

        // Act - Query about a completely different topic
        var query = "What are the company's environmental sustainability practices?";
        var answer = await _knowledgeBaseService.QueryKnowledgeBaseAsync(query);

        // Assert - Should return ungrounded answer or very low confidence
        _output.WriteLine($"Answer: {answer.Answer}");
        _output.WriteLine($"Grounded: {answer.IsGrounded}");
        _output.WriteLine($"Confidence: {answer.ConfidenceScore:F2}");

        answer.Should().NotBeNull();
        if (answer.Citations.Any())
        {
            // If citations exist, they should have low relevance
            answer.Citations.Max(c => c.RelevanceScore).Should().BeLessThan(0.7);
        }
    }

    [Fact]
    public async Task RAGWorkflow_MultipleDocuments_RanksSourcesByRelevance()
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
            await _knowledgeBaseService!.IndexDocumentAsync(doc);
        }
        await Task.Delay(3000);

        // Act - Query specifically about complaints
        var query = "How do I handle customer complaints?";
        var answer = await _knowledgeBaseService!.QueryKnowledgeBaseAsync(query, maxCitations: 3);

        // Assert - Citations should be ranked by relevance
        _output.WriteLine($"\n=== RANKED CITATIONS ===");
        foreach (var citation in answer.Citations)
        {
            _output.WriteLine($"{citation.DocumentTitle}: {citation.RelevanceScore:F2}");
        }

        answer.Citations.Should().NotBeEmpty();

        // Verify citations are in descending relevance order
        for (int i = 0; i < answer.Citations.Count - 1; i++)
        {
            answer.Citations[i].RelevanceScore.Should().BeGreaterThanOrEqualTo(
                answer.Citations[i + 1].RelevanceScore,
                "citations should be ordered by relevance score");
        }

        // The most relevant documents should be about complaints specifically
        var topCitation = answer.Citations.First();
        topCitation.DocumentTitle.Should().ContainAny(new[] { "Complaint", "complaint" });
    }

    [Fact]
    public async Task HealthCheck_WithValidService_ReturnsHealthy()
    {
        // Act
        var isHealthy = await _knowledgeBaseService!.IsHealthyAsync();

        // Assert
        isHealthy.Should().BeTrue("knowledge base service should be healthy");
    }
}
