# Windows App Session Reporting - User Guide

## Overview

The Windows app now automatically tracks gameplay session metrics and exports detailed reports on exit.

## What's Tracked

### Combat Metrics

- **Total Kills**: Number of enemies defeated
- **Total Deaths**: Number of times the player died
- **K/D Ratio**: Kill/Death ratio (automatically calculated)

### Progression Metrics (Prepared for future)

- **Levels Completed**: Dungeon levels cleared
- **Items Collected**: Items picked up
- **Dungeons Completed**: Full dungeon runs

### Session Info

- **Session ID**: Unique identifier for each play session
- **Start Time**: When the session began
- **End Time**: When the session ended
- **Duration**: Total playtime in minutes

## How It Works

### Automatic Tracking

1. **Session Start**: Metrics collection begins automatically when the app starts
2. **During Gameplay**:
   - Every enemy kill increments `TotalKills`
   - Every player death increments `TotalDeaths`
   - Events are logged to `logs/lablab-bean-windows-{date}.log`
3. **Session End**: Report exports automatically when you exit the app

### Report Location

Reports are saved to version-specific directories:

```
build/_artifacts/{version}/reports/sessions/windows-session-{timestamp}.json
```

Example: `build/_artifacts/0.1.0/reports/sessions/windows-session-20251023-051530.json`

This ensures that:

- Each version's reports are isolated
- Build artifacts include both build-time AND runtime reports
- Easy to compare performance across versions

### Log Output

During gameplay, you'll see log entries like:

```
[INF] Session started: a1b2c3d4-e5f6-... (v0.1.0)
[INF] Enemy killed. Total kills: 1
[INF] Enemy killed. Total kills: 2
[INF] Player died. Total deaths: 1
[INF] Enemy killed. Total kills: 3
[INF] Session ended: a1b2c3d4-e5f6-... | Duration: 15:30 | Kills: 42 | Deaths: 3 | K/D: 14.00
[INF] Session report exported to build/_artifacts/0.1.0/reports/sessions/windows-session-20251023-051530.json
```

## Example Report

```json
{
  "SessionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "Version": "0.1.0",
  "StartTime": "2025-10-23T05:00:00Z",
  "EndTime": "2025-10-23T05:30:00Z",
  "DurationMinutes": 30.5,
  "Stats": {
    "TotalKills": 42,
    "TotalDeaths": 3,
    "LevelsCompleted": 5,
    "ItemsCollected": 18,
    "DungeonsCompleted": 1,
    "KDRatio": 14.0
  }
}
```

## Using the Reports

### View Your Stats

Simply open the JSON file in any text editor or JSON viewer.

### Track Progress

Compare reports from different sessions:

```bash
# View all session reports for current version
ls build/_artifacts/0.1.0/reports/sessions/

# View all versions
ls build/_artifacts/*/reports/sessions/

# Compare your best K/D ratio across all versions
cat build/_artifacts/*/reports/sessions/*.json | grep "KDRatio"
```

### Analyze Performance

Use the session data to:

- Track improvement over time
- Identify difficult levels (high death count)
- Measure playtime
- Set personal records

## Programmatic Access

For developers who want to analyze reports programmatically:

```csharp
using System.Text.Json;

var json = File.ReadAllText("build/_artifacts/0.1.0/reports/sessions/windows-session-20251023-051530.json");
var sessionData = JsonSerializer.Deserialize<SessionData>(json);

Console.WriteLine($"Version: {sessionData.Version}");
Console.WriteLine($"K/D Ratio: {sessionData.Stats.KDRatio:F2}");
Console.WriteLine($"Duration: {sessionData.DurationMinutes:F1} minutes");
```

## Troubleshooting

### Report Not Generated

**Problem**: No report file after exiting the app

**Solutions**:

1. Check logs: `logs/lablab-bean-windows-{date}.log`
2. Look for errors: `[ERR] Failed to export session report`
3. Ensure directory exists: `build/_artifacts/{version}/reports/sessions/`
4. Check file permissions

### Metrics Not Updating

**Problem**: Kills/deaths not incrementing

**Solutions**:

1. Check that events are firing: Look for `[INF] Enemy killed` in logs
2. Verify `SessionMetricsCollector` is registered in DI
3. Ensure combat events are subscribed in `Program.cs`

### Missing Dependencies

**Problem**: Build errors related to reporting

**Solution**:

```bash
dotnet restore dotnet/windows-app/LablabBean.Windows/LablabBean.Windows.csproj
dotnet build dotnet/windows-app/LablabBean.Windows/LablabBean.Windows.csproj
```

## Future Enhancements

Planned features (see implementation doc for details):

- 📊 Real-time stats display in HUD
- 💾 Manual export via in-game menu
- 📈 Session comparison and progress tracking
- 🏆 Achievement tracking
- 📉 Performance metrics (FPS, memory)
- 🔌 Plugin health monitoring

## Configuration

Currently, reporting is always enabled. Future versions may add configuration options:

```json
{
  "Reporting": {
    "Enabled": true,
    "AutoExport": true,
    "ReportFormat": "JSON",
    "OutputDirectory": "build/_artifacts/{version}/reports/sessions"
  }
}
```

---

**Version**: 1.0.0
**Last Updated**: 2025-10-23
**Status**: Production Ready ✅
