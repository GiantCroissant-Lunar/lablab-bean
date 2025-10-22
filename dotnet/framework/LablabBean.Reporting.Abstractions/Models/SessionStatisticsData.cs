using System;
using System.Collections.Generic;

namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Data model for game session statistics reports.
/// Maps to FR-026 through FR-032 (session statistics requirements).
/// </summary>
public class SessionStatisticsData
{
    // Session Metadata (FR-026)
    public string SessionId { get; set; } = string.Empty;
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public TimeSpan TotalPlaytime { get; set; }
    
    // Combat Statistics (FR-027, FR-028)
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public decimal KillDeathRatio { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public decimal AverageDamagePerKill { get; set; }
    
    // Progression (FR-029)
    public int ItemsCollected { get; set; }
    public int LevelsCompleted { get; set; }
    public int AchievementsUnlocked { get; set; }
    
    // Performance Metrics (FR-030)
    public int AverageFrameRate { get; set; }
    public TimeSpan TotalLoadTime { get; set; }
    
    // Event Timeline (FR-031)
    public List<SessionEvent> KeyEvents { get; set; } = new();
    
    // Metadata
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Significant event during session.
/// </summary>
public class SessionEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
