namespace LablabBean.Contracts.UI.Models;

/// <summary>
/// Configuration options for the activity log system
/// </summary>
public sealed class ActivityLogOptions
{
    /// <summary>
    /// Maximum number of entries to keep in the circular buffer. Default: 1000
    /// </summary>
    public int MaxEntries { get; set; } = 1000;

    /// <summary>
    /// Whether to display timestamps in the activity log. Default: true
    /// </summary>
    public bool ShowTimestamps { get; set; } = true;

    /// <summary>
    /// Whether to mirror activity log entries to Microsoft.Extensions.Logging. Default: true
    /// </summary>
    public bool MirrorToLogger { get; set; } = true;

    /// <summary>
    /// Categories to enable logging for. If empty, all categories are enabled. Default: empty (all enabled)
    /// </summary>
    public HashSet<ActivityCategory> EnabledCategories { get; set; } = new();

    /// <summary>
    /// Minimum severity level to log. Entries below this level will be ignored. Default: Info
    /// </summary>
    public ActivitySeverity MinimumSeverity { get; set; } = ActivitySeverity.Info;

    /// <summary>
    /// Whether to log movement events. Default: false (movement can be verbose)
    /// </summary>
    public bool LogMovement { get; set; } = false;
}
