using System.Reflection;
using System.Text;
using LablabBean.Reporting.Contracts.Contracts;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

namespace LablabBean.Plugins.Reporting.Html;

/// <summary>
/// HTML report renderer using Scriban templates.
/// Supports BuildMetricsData, SessionStatisticsData, and PluginHealthData.
/// </summary>
public class HtmlReportRenderer : IReportRenderer
{
    private readonly ILogger<HtmlReportRenderer> _logger;
    private readonly Dictionary<Type, Template> _templateCache = new();

    public HtmlReportRenderer(ILogger<HtmlReportRenderer> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ReportFormat> SupportedFormats => new[] { ReportFormat.HTML };

    public async Task<ReportResult> RenderAsync(
        ReportRequest request,
        object data,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var dataType = data.GetType();
            _logger.LogInformation("Rendering HTML report for data type: {DataType}", dataType.Name);

            // Get or compile template
            var template = await GetTemplateAsync(dataType, cancellationToken);

            // Prepare template context
            var scriptObject = new ScriptObject();
            scriptObject.Import(data, renamer: member => ToSnakeCase(member.Name));
            scriptObject.Add("report_title", dataType.Name.Replace("Data", ""));
            scriptObject.Add("generated_at", DateTime.UtcNow);
            
            var context = new TemplateContext();
            
            // Add custom functions
            var functions = new ScriptObject();
            functions.Import(typeof(TemplateHelpers));
            context.PushGlobal(functions);
            
            context.PushGlobal(scriptObject);

            // Render template
            var html = await template.RenderAsync(context);
            
            _logger.LogInformation("HTML report rendered successfully, size: {Size} bytes", html.Length);

            // Write to output file if specified
            string? outputPath = null;
            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                outputPath = request.OutputPath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                    ? request.OutputPath
                    : $"{request.OutputPath}.html";

                await File.WriteAllTextAsync(outputPath, html, cancellationToken);
                _logger.LogInformation("Report written to: {OutputPath}", outputPath);
            }

            var duration = DateTime.UtcNow - startTime;

            return ReportResult.Success(
                outputPath ?? string.Empty,
                html.Length,
                duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render HTML report");
            
            return ReportResult.Failure(ex.Message);
        }
    }

    private async Task<Template> GetTemplateAsync(Type dataType, CancellationToken cancellationToken)
    {
        if (_templateCache.TryGetValue(dataType, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var templateName = GetTemplateName(dataType);
        var templateContent = await LoadEmbeddedTemplateAsync(templateName, cancellationToken);
        
        var template = Template.Parse(templateContent);
        
        if (template.HasErrors)
        {
            var errors = string.Join(", ", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Template compilation failed: {errors}");
        }

        _templateCache[dataType] = template;
        return template;
    }

    private string GetTemplateName(Type dataType)
    {
        return dataType.Name switch
        {
            nameof(BuildMetricsData) => "BuildMetrics.sbn",
            nameof(SessionStatisticsData) => "SessionStatistics.sbn",
            nameof(PluginHealthData) => "PluginHealth.sbn",
            _ => throw new NotSupportedException($"No template available for data type: {dataType.Name}")
        };
    }

    private async Task<string> LoadEmbeddedTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"LablabBean.Plugins.Reporting.Html.Templates.{templateName}";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            var availableResources = assembly.GetManifestResourceNames();
            _logger.LogError(
                "Template not found: {TemplateName}. Available resources: {Resources}",
                resourceName,
                string.Join(", ", availableResources));
            
            throw new FileNotFoundException($"Template not found: {templateName}");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));

        for (int i = 1; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Helper functions for Scriban templates.
/// </summary>
public static class TemplateHelpers
{
    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        if (timeSpan.TotalMinutes >= 1)
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        if (timeSpan.TotalSeconds >= 1)
        {
            return $"{timeSpan.TotalSeconds:F2}s";
        }
        
        return $"{timeSpan.TotalMilliseconds:F0}ms";
    }
}
