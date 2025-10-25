using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LablabBean.Contracts.AI.Memory;
using LablabBean.Plugins.NPC.Services;
using LablabBean.Plugins.NPC.Systems;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.NPC.Examples;

/// <summary>
/// Memory visualization dashboard for displaying NPC relationship data
/// Useful for debugging and player analytics
/// </summary>
public class MemoryVisualizationDashboard
{
    private readonly MemoryEnhancedNPCService _npcService;
    private readonly ILogger<MemoryVisualizationDashboard> _logger;

    public MemoryVisualizationDashboard(
        MemoryEnhancedNPCService npcService,
        ILogger<MemoryVisualizationDashboard> logger)
    {
        _npcService = npcService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a text-based dashboard for player-NPC relationships
    /// </summary>
    public async Task<string> GeneratePlayerDashboardAsync(
        string playerId,
        params string[] npcIds)
    {
        var dashboard = new StringBuilder();
        dashboard.AppendLine("╔═══════════════════════════════════════════════════════════════════╗");
        dashboard.AppendLine("║           NPC RELATIONSHIP MEMORY DASHBOARD                       ║");
        dashboard.AppendLine("╚═══════════════════════════════════════════════════════════════════╝");
        dashboard.AppendLine();
        dashboard.AppendLine($"Player ID: {playerId}");
        dashboard.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        dashboard.AppendLine();
        dashboard.AppendLine("═══════════════════════════════════════════════════════════════════");

        foreach (var npcId in npcIds)
        {
            try
            {
                var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
                var recentMemories = await _npcService.GetRecentDialogueHistoryAsync(playerId, limit: 3);

                dashboard.AppendLine();
                dashboard.AppendLine($"NPC: {npcId}");
                dashboard.AppendLine("───────────────────────────────────────────────────────────────────");

                // Relationship bar
                dashboard.AppendLine($"Relationship: {GetRelationshipBar(insights.RelationshipLevel)} {insights.RelationshipLevel}");
                dashboard.AppendLine($"Interactions: {insights.InteractionCount}");
                dashboard.AppendLine($"Total Importance: {insights.TotalImportance:F2}");

                if (insights.LastInteraction.HasValue)
                {
                    var timeSince = DateTime.UtcNow - insights.LastInteraction.Value;
                    dashboard.AppendLine($"Last Seen: {FormatTimeSince(timeSince)}");
                }
                else
                {
                    dashboard.AppendLine("Last Seen: Never");
                }

                // Recent memories
                if (recentMemories.Count > 0)
                {
                    dashboard.AppendLine();
                    dashboard.AppendLine("Recent Interactions:");

                    foreach (var memory in recentMemories.Take(3))
                    {
                        var timeSince = DateTime.UtcNow - memory.Memory.Timestamp;
                        dashboard.AppendLine($"  • {FormatTimeSince(timeSince)}: {TruncateText(memory.Memory.Content, 50)}");
                        dashboard.AppendLine($"    Relevance: {memory.RelevanceScore:F3}");
                    }
                }
                else
                {
                    dashboard.AppendLine();
                    dashboard.AppendLine("Recent Interactions: None");
                }

                dashboard.AppendLine("───────────────────────────────────────────────────────────────────");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate dashboard for NPC {NpcId}", npcId);
                dashboard.AppendLine();
                dashboard.AppendLine($"NPC: {npcId}");
                dashboard.AppendLine($"ERROR: {ex.Message}");
                dashboard.AppendLine("───────────────────────────────────────────────────────────────────");
            }
        }

        dashboard.AppendLine();
        dashboard.AppendLine("═══════════════════════════════════════════════════════════════════");

        return dashboard.ToString();
    }

    /// <summary>
    /// Generates a detailed memory report for a specific NPC interaction
    /// </summary>
    public async Task<string> GenerateNpcMemoryReportAsync(
        string playerId,
        string npcId,
        int memoryLimit = 10)
    {
        var insights = await _npcService.GetRelationshipInsightsAsync(playerId, npcId);
        var recentDialogue = await _npcService.GetRecentDialogueHistoryAsync(playerId, limit: memoryLimit);

        var report = new StringBuilder();
        report.AppendLine("╔═══════════════════════════════════════════════════════════════════╗");
        report.AppendLine("║                    NPC MEMORY REPORT                              ║");
        report.AppendLine("╚═══════════════════════════════════════════════════════════════════╝");
        report.AppendLine();
        report.AppendLine($"Player ID: {playerId}");
        report.AppendLine($"NPC ID: {npcId}");
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine();
        report.AppendLine("═══════════════════════════════════════════════════════════════════");
        report.AppendLine();
        report.AppendLine("RELATIONSHIP STATUS");
        report.AppendLine("───────────────────────────────────────────────────────────────────");
        report.AppendLine($"Level: {GetRelationshipBar(insights.RelationshipLevel)} {insights.RelationshipLevel}");
        report.AppendLine($"Total Interactions: {insights.InteractionCount}");
        report.AppendLine($"Cumulative Importance: {insights.TotalImportance:F2}");

        if (insights.LastInteraction.HasValue)
        {
            report.AppendLine($"Last Interaction: {insights.LastInteraction.Value:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"Time Since Last: {FormatTimeSince(DateTime.UtcNow - insights.LastInteraction.Value)}");
        }

        report.AppendLine();
        report.AppendLine("MEMORY TIMELINE");
        report.AppendLine("───────────────────────────────────────────────────────────────────");

        if (recentDialogue.Count > 0)
        {
            foreach (var memory in recentDialogue)
            {
                report.AppendLine();
                report.AppendLine($"[{memory.Memory.Timestamp:yyyy-MM-dd HH:mm:ss}] Relevance: {memory.RelevanceScore:F3}");
                report.AppendLine($"  {memory.Memory.Content}");

                if (!string.IsNullOrEmpty(memory.Memory.MemoryType))
                {
                    report.AppendLine($"  Type: {memory.Memory.MemoryType}");
                }
            }
        }
        else
        {
            report.AppendLine();
            report.AppendLine("No memories found.");
        }

        report.AppendLine();
        report.AppendLine("═══════════════════════════════════════════════════════════════════");
        report.AppendLine();
        report.AppendLine("ANALYSIS");
        report.AppendLine("───────────────────────────────────────────────────────────────────");
        report.AppendLine(GenerateAnalysis(insights, recentDialogue.Count));
        report.AppendLine("═══════════════════════════════════════════════════════════════════");

        return report.ToString();
    }

    /// <summary>
    /// Prints dashboard to console
    /// </summary>
    public async Task PrintDashboardAsync(string playerId, params string[] npcIds)
    {
        var dashboard = await GeneratePlayerDashboardAsync(playerId, npcIds);
        Console.WriteLine(dashboard);
    }

    /// <summary>
    /// Prints detailed report to console
    /// </summary>
    public async Task PrintNpcReportAsync(string playerId, string npcId, int memoryLimit = 10)
    {
        var report = await GenerateNpcMemoryReportAsync(playerId, npcId, memoryLimit);
        Console.WriteLine(report);
    }

    private string GetRelationshipBar(RelationshipLevel level)
    {
        var filled = (int)level + 1;
        var empty = 6 - filled;
        return $"[{'█'.ToString().PadRight(filled, '█')}{'░'.ToString().PadRight(empty, '░')}]";
    }

    private string FormatTimeSince(TimeSpan timeSince)
    {
        if (timeSince.TotalSeconds < 60)
            return "just now";
        if (timeSince.TotalMinutes < 60)
            return $"{(int)timeSince.TotalMinutes}m ago";
        if (timeSince.TotalHours < 24)
            return $"{(int)timeSince.TotalHours}h ago";
        if (timeSince.TotalDays < 7)
            return $"{(int)timeSince.TotalDays}d ago";
        if (timeSince.TotalDays < 30)
            return $"{(int)(timeSince.TotalDays / 7)}w ago";
        return $"{(int)(timeSince.TotalDays / 30)}mo ago";
    }

    private string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text.Substring(0, maxLength - 3) + "...";
    }

    private string GenerateAnalysis(NpcRelationshipInsights insights, int memoryCount)
    {
        var analysis = new StringBuilder();

        if (insights.InteractionCount == 0)
        {
            analysis.AppendLine("• No prior interactions detected");
            analysis.AppendLine("• NPC will treat player as a stranger");
            return analysis.ToString();
        }

        if (insights.RelationshipLevel >= RelationshipLevel.GoodFriend)
        {
            analysis.AppendLine("• Strong relationship established");
            analysis.AppendLine("• NPC will offer preferential treatment");
            analysis.AppendLine("• Access to exclusive dialogue options likely");
        }
        else if (insights.RelationshipLevel >= RelationshipLevel.Friend)
        {
            analysis.AppendLine("• Friendly relationship developing");
            analysis.AppendLine("• NPC becoming more trusting");
        }
        else
        {
            analysis.AppendLine("• Early relationship stage");
            analysis.AppendLine("• More interactions needed to build trust");
        }

        if (insights.LastInteraction.HasValue)
        {
            var daysSince = (DateTime.UtcNow - insights.LastInteraction.Value).TotalDays;
            if (daysSince > 14)
            {
                analysis.AppendLine($"• Long absence ({(int)daysSince} days) may affect relationship");
            }
        }

        analysis.AppendLine($"• {memoryCount} memories stored for context");

        return analysis.ToString();
    }
}
