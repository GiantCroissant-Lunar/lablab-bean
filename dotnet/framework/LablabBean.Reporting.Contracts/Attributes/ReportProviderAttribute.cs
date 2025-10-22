using System;

namespace LablabBean.Reporting.Contracts.Attributes;

/// <summary>
/// Marks a class as a report provider for compile-time discovery.
/// Class must implement IReportProvider.
/// Source generator will create a static registry of all providers.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ReportProviderAttribute : Attribute
{
    /// <summary>
    /// Unique name of the provider (e.g., "BuildMetrics", "Session", "PluginHealth").
    /// Used to resolve provider from CLI commands.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Category for grouping providers (e.g., "Build", "Analytics", "Diagnostics").
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// Priority for provider selection when multiple providers exist in same category.
    /// Lower values = higher priority. Default is 0.
    /// </summary>
    public int Priority { get; }
    
    public ReportProviderAttribute(string name, string category, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));
        
        Name = name;
        Category = category;
        Priority = priority;
    }
}
