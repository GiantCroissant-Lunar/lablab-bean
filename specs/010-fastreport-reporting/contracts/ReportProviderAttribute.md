# ReportProviderAttribute

**Location**: `dotnet/framework/LablabBean.Reporting.Abstractions/Attributes/ReportProviderAttribute.cs`

```csharp
using System;

namespace LablabBean.Reporting.Abstractions.Attributes;

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
```

**Usage**:

```csharp
// BuildMetricsProvider.cs
[ReportProvider("BuildMetrics", "Build")]
public class BuildMetricsProvider : IReportProvider
{
    // Implementation
}

// SessionStatisticsProvider.cs
[ReportProvider("Session", "Analytics")]
public class SessionStatisticsProvider : IReportProvider
{
    // Implementation
}

// PluginHealthProvider.cs
[ReportProvider("PluginHealth", "Diagnostics")]
public class PluginHealthProvider : IReportProvider
{
    // Implementation
}
```

**Categories**:

- `"Build"` - Build-time metrics (tests, coverage, timing)
- `"Analytics"` - Runtime analytics (session stats, player behavior)
- `"Diagnostics"` - System health (plugin status, performance)

**Priority**:
When multiple providers exist in the same category, lower priority values are preferred.

```csharp
[ReportProvider("BuildMetrics", "Build", priority: 0)]  // Default
[ReportProvider("BuildMetricsV2", "Build", priority: 10)]  // Fallback
```

**Source Generator Output**:
The source generator scans all assemblies for classes with this attribute and generates a static registry:

```csharp
// Generated: ReportProviderRegistry.g.cs
public static class ReportProviderRegistry
{
    public static IReadOnlyList<ProviderRegistration> Providers { get; } = new[]
    {
        new ProviderRegistration(typeof(BuildMetricsProvider), "BuildMetrics", "Build", 0),
        new ProviderRegistration(typeof(SessionStatisticsProvider), "Session", "Analytics", 0),
        new ProviderRegistration(typeof(PluginHealthProvider), "PluginHealth", "Diagnostics", 0)
    };
}
```
