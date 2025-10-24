---
id: activity-log-renderer-guide
title: Activity Log Renderer Implementation Guide
version: 1.0.0
status: draft
category: development
tags: [activity-log, rendering, ui, architecture, guide]
created: 2025-10-23
updated: 2025-10-23
author: Claude
---

# Activity Log Renderer Implementation Guide

This guide explains how to create a new renderer for the activity log system, demonstrating the complete separation between logging logic and UI rendering.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Step-by-Step Implementation](#step-by-step-implementation)
- [Reference Implementations](#reference-implementations)
- [Testing Your Renderer](#testing-your-renderer)
- [Best Practices](#best-practices)

## Overview

The activity log system in Lablab-Bean follows a **clean separation of concerns** pattern where:

- **Core logging logic** is UI-agnostic (in `LablabBean.Game.Core`)
- **Service interface** provides a platform-independent contract (`IActivityLogService`)
- **Renderers** consume the service and display logs in their specific UI framework

This design allows multiple UI frameworks (Terminal.Gui, SadConsole, web terminals, Unity, etc.) to display the same activity log without modifying core game code.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                Your Custom Renderer (Framework-Specific)         │
│                                                                   │
│  - Subscribes to IActivityLogService                             │
│  - Formats ActivityEntryDto for your UI                          │
│  - Handles display in your framework                             │
└─────────────────────────────────────────────────────────────────┘
                               ↓
                    Uses IActivityLogService
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│              IActivityLogService (Platform-Agnostic)             │
│                                                                   │
│  - Observable pattern (IObservable<ActivityEntryDto>)            │
│  - Event pattern (Changed event)                                 │
│  - Query methods (GetLast, GetByCategory, Search, etc.)          │
└─────────────────────────────────────────────────────────────────┘
                               ↓
                    Implemented by ActivityLogService
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│                   Core Game Logic (ECS-based)                    │
│                                                                   │
│  - ActivityLogSystem (ECS system)                                │
│  - ActivityLog component (circular buffer)                       │
│  - Game systems (Combat, Inventory, etc.) log events             │
└─────────────────────────────────────────────────────────────────┘
```

## Prerequisites

Before implementing a renderer, ensure you have:

1. **Reference to `LablabBean.Contracts.UI`** - Contains `IActivityLogService` interface and DTOs
2. **Understanding of the notification patterns** - Event-based or Observable-based
3. **Your UI framework set up** - Terminal.Gui, SadConsole, web framework, etc.
4. **Dependency injection configured** - To receive `IActivityLogService` instance

## Step-by-Step Implementation

### Step 1: Create Your Renderer Class

```csharp
using LablabBean.Contracts.UI.Models;
using LablabBean.Contracts.UI.Services;

namespace YourNamespace.UI;

public class YourActivityLogRenderer
{
    private IActivityLogService? _service;

    // Your UI framework-specific components
    // Example: ListView, TextBox, Canvas, etc.

    public YourActivityLogRenderer(/* UI framework parameters */)
    {
        // Initialize your UI components
    }
}
```

### Step 2: Implement the Bind Method

The `Bind` method connects your renderer to the activity log service:

```csharp
public void Bind(IActivityLogService service)
{
    if (service == null)
        throw new ArgumentNullException(nameof(service));

    _service = service;

    // Option 1: Use event-based notification (simpler)
    _service.Changed += OnActivityLogChanged;

    // Option 2: Use observable pattern (reactive)
    _subscription = _service.OnLogAdded.Subscribe(entry =>
    {
        // Handle new entry
        DisplayEntry(entry);
    });

    // Load initial entries
    RefreshDisplay();
}
```

### Step 3: Implement the Refresh/Update Method

This method queries the service and updates your UI:

```csharp
private void RefreshDisplay()
{
    if (_service == null) return;

    // Get recent entries (adjust count based on your display size)
    var entries = _service.GetRecentEntries(maxCount: 50);

    // Clear and rebuild your display
    ClearDisplay();

    foreach (var entry in entries)
    {
        DisplayEntry(entry);
    }

    // Auto-scroll to bottom (optional)
    ScrollToBottom();
}
```

### Step 4: Format Entries for Your UI

Use the data from `ActivityEntryDto` to format the display:

```csharp
private void DisplayEntry(ActivityEntryDto entry)
{
    // Use pre-computed icon and color from DTO
    var icon = entry.Icon;        // "⚔", "+", "!", etc.
    var color = entry.Color;      // "Red", "Green", "Yellow", etc.
    var message = entry.Message;

    // Optional: Include timestamp
    var timestamp = entry.Timestamp.ToLocalTime().ToString("HH:mm:ss");

    // Format based on your UI framework
    var displayText = $"[{timestamp}] {icon} {message}";

    // Render with color (framework-specific)
    RenderTextWithColor(displayText, color);
}
```

### Step 5: Handle Event Notifications

Respond to new log entries:

```csharp
private void OnActivityLogChanged(long sequence)
{
    // Service notifies us that log has changed
    // Re-query and refresh display
    RefreshDisplay();
}
```

### Step 6: Clean Up Resources

Implement disposal if needed:

```csharp
public void Dispose()
{
    if (_service != null)
    {
        _service.Changed -= OnActivityLogChanged;
    }

    _subscription?.Dispose();
}
```

## Reference Implementations

### Example 1: Terminal.Gui Renderer

**Location:** `dotnet/console-app/LablabBean.Game.TerminalUI/Views/ActivityLogView.cs`

```csharp
public class ActivityLogView : FrameView
{
    private readonly ListView _listView;
    private IActivityLogService? _service;

    public ActivityLogView(string title = "Activity") : base(title)
    {
        _listView = new ListView(new List<string>())
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_listView);
    }

    public void Bind(IActivityLogService service)
    {
        _service = service;
        _service.Changed += OnServiceChanged;
        RefreshFromService();
    }

    private void OnServiceChanged(long sequence)
    {
        RefreshFromService();
    }

    private void RefreshFromService()
    {
        if (_service == null) return;
        var entries = _service.GetRecentEntries(100);
        var lines = entries.Select(e => $"{e.Icon} {e.Message}").ToList();
        _listView.SetSource(lines);
        _listView.SelectedItem = lines.Count - 1; // Auto-scroll
    }
}
```

**Key Points:**
- Uses Terminal.Gui `ListView` for display
- Subscribes to `Changed` event
- Auto-scrolls to bottom on updates

### Example 2: SadConsole Renderer

**Location:** `dotnet/windows-app/LablabBean.Game.SadConsole/Renderers/ActivityLogRenderer.cs`

```csharp
public class ActivityLogRenderer
{
    private readonly ControlsConsole _console;
    private readonly ListBox _listBox;
    private IActivityLogService? _service;

    public ControlsConsole Console => _console;

    public ActivityLogRenderer(int width, int height)
    {
        _console = new ControlsConsole(width, height);
        _listBox = new ListBox(width - 2, height - 2)
        {
            Position = new Point(1, 1)
        };
        _console.Controls.Add(_listBox);

        // Draw border
        _console.Surface.DrawBox(
            new Rectangle(0, 0, width, height),
            ShapeParameters.CreateStyledBox(
                ICellSurface.ConnectedLineThin,
                new ColoredGlyph(Color.White, Color.Black)));
    }

    public void Bind(IActivityLogService service)
    {
        _service = service;
        _service.Changed += OnServiceChanged;
        RefreshFromService();
    }

    private void RefreshFromService()
    {
        if (_service == null) return;
        var entries = _service.GetRecentEntries(100);

        _listBox.Items.Clear();
        foreach (var entry in entries)
        {
            _listBox.Items.Add($"{entry.Icon} {entry.Message}");
        }

        if (_listBox.Items.Count > 0)
            _listBox.SelectedIndex = _listBox.Items.Count - 1;
    }
}
```

**Key Points:**
- Uses SadConsole `ListBox` for display
- Draws a styled border
- Same subscription pattern as Terminal.Gui

### Example 3: Web Terminal Renderer (Hypothetical)

```csharp
public class WebTerminalActivityLogRenderer
{
    private IActivityLogService? _service;
    private IDisposable? _subscription;

    public async Task InitializeAsync(IActivityLogService service)
    {
        _service = service;

        // Use reactive pattern for real-time updates
        _subscription = _service.OnLogAdded.Subscribe(async entry =>
        {
            // Send to browser via SignalR/WebSocket
            await _hubContext.Clients.All.SendAsync(
                "ActivityLogEntry",
                new
                {
                    timestamp = entry.Timestamp.ToString("HH:mm:ss"),
                    icon = entry.Icon,
                    color = entry.Color,
                    message = entry.Message
                });
        });

        // Send initial entries
        var entries = _service.GetRecentEntries(50);
        await _hubContext.Clients.All.SendAsync("InitialActivityLog", entries);
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
```

**Key Points:**
- Uses `IObservable<ActivityEntryDto>` for real-time streaming
- Sends to browser via SignalR
- No polling required - reactive push notifications

## Testing Your Renderer

### Unit Testing Without Game Systems

You can test your renderer with a mock service:

```csharp
[Fact]
public void Renderer_ShouldDisplayEntries_WhenBound()
{
    // Arrange
    var mockService = new Mock<IActivityLogService>();
    var testEntries = new List<ActivityEntryDto>
    {
        new()
        {
            Message = "Test message",
            Icon = "⚔",
            Color = "Red",
            Severity = ActivitySeverity.Combat,
            Timestamp = DateTimeOffset.UtcNow
        }
    };

    mockService.Setup(s => s.GetRecentEntries(It.IsAny<int>()))
        .Returns(testEntries);

    var renderer = new YourActivityLogRenderer();

    // Act
    renderer.Bind(mockService.Object);

    // Assert
    // Verify your renderer displays the entry
    renderer.GetDisplayedText().Should().Contain("Test message");
}
```

### Integration Testing

Test with the real service in a test harness:

```csharp
// Create test world and systems
var worldManager = new GameWorldManager();
var logSystem = new ActivityLogSystem();
var logService = new ActivityLogService(logger, worldManager, logSystem, options);

// Create your renderer
var renderer = new YourActivityLogRenderer();
renderer.Bind(logService);

// Log some test messages
logService.Combat("Test combat message");
logService.Info("Test info message");

// Verify renderer updates
// ... assertions
```

## Best Practices

### 1. Always Use the Service Interface

✅ **DO:**
```csharp
public void Bind(IActivityLogService service) { ... }
```

❌ **DON'T:**
```csharp
public void Update(World world) { ... } // Direct ECS access
```

**Why:** Using the interface ensures complete decoupling from the ECS layer.

### 2. Handle Null Services Gracefully

```csharp
private void RefreshDisplay()
{
    if (_service == null)
    {
        // Log warning or return early
        return;
    }

    var entries = _service.GetRecentEntries(100);
    // ...
}
```

### 3. Use Pre-Computed Icons and Colors

The service provides `Icon` and `Color` properties on DTOs - use them directly:

```csharp
var icon = entry.Icon;    // Already computed
var color = entry.Color;  // Already computed
```

Don't re-implement severity-to-icon mapping in your renderer.

### 4. Choose the Right Notification Pattern

**Event-based (simpler):**
```csharp
_service.Changed += sequence => RefreshDisplay();
```

**Observable-based (reactive):**
```csharp
_subscription = _service.OnLogAdded
    .Throttle(TimeSpan.FromMilliseconds(100))
    .ObserveOn(Scheduler.MainThread)
    .Subscribe(entry => DisplayEntry(entry));
```

Use observables when you need:
- Throttling/debouncing
- Thread marshalling
- Complex reactive chains

### 5. Implement Auto-Scrolling

Users expect new messages to appear at the bottom and auto-scroll:

```csharp
private void RefreshDisplay()
{
    // ... render entries ...

    // Auto-scroll to bottom
    if (_listView.Items.Count > 0)
    {
        _listView.SelectedIndex = _listView.Items.Count - 1;
    }
}
```

### 6. Make Display Size Configurable

```csharp
public void SetMaxDisplayLines(int maxLines)
{
    _maxLines = Math.Max(10, maxLines);
}
```

### 7. Support Optional Timestamps

```csharp
public void ShowTimestamps(bool show)
{
    _showTimestamps = show;
    RefreshDisplay(); // Re-render with/without timestamps
}

private string FormatEntry(ActivityEntryDto entry)
{
    var ts = _showTimestamps
        ? $"[{entry.Timestamp:HH:mm}] "
        : string.Empty;

    return $"{ts}{entry.Icon} {entry.Message}";
}
```

### 8. Clean Up Subscriptions

```csharp
public void Dispose()
{
    if (_service != null)
    {
        _service.Changed -= OnActivityLogChanged;
        _service = null;
    }

    _subscription?.Dispose();
    _subscription = null;
}
```

### 9. Use Filtering for Advanced UIs

If your UI supports tabs or filters:

```csharp
public void ShowCombatOnly()
{
    var combatEntries = _service.GetByCategory(ActivityCategory.Combat, 100);
    DisplayEntries(combatEntries);
}

public void Search(string searchTerm)
{
    var matchingEntries = _service.Search(searchTerm, 50);
    DisplayEntries(matchingEntries);
}
```

### 10. Handle Thread Marshalling

If your UI requires updates on a specific thread:

```csharp
private void OnActivityLogChanged(long sequence)
{
    // Marshal to UI thread
    Dispatcher.Invoke(() => RefreshDisplay());

    // Or for async:
    await Dispatcher.InvokeAsync(() => RefreshDisplay());
}
```

## Configuration in DI

Register your renderer in your application's dependency injection:

```csharp
// Program.cs or Startup.cs
services.AddSingleton<IActivityLogService, ActivityLogService>();
services.Configure<ActivityLogOptions>(options =>
{
    options.MaxEntries = 1000;
    options.ShowTimestamps = true;
    options.MirrorToLogger = true;
});

// Register your renderer
services.AddSingleton<YourActivityLogRenderer>();
```

## Advanced Features

### 1. Color Mapping for Your Framework

Map the `Color` string to your framework's color type:

```csharp
private YourFrameworkColor MapColor(string colorName)
{
    return colorName switch
    {
        "Red" => YourFramework.Colors.Red,
        "Green" => YourFramework.Colors.Green,
        "Yellow" => YourFramework.Colors.Yellow,
        "Gold" => YourFramework.Colors.Gold,
        "Gray" => YourFramework.Colors.Gray,
        "White" => YourFramework.Colors.White,
        _ => YourFramework.Colors.White
    };
}
```

### 2. Implement Rich Text Formatting

```csharp
private string FormatWithRichText(ActivityEntryDto entry)
{
    // Example: Markdown-like formatting
    var color = MapColor(entry.Color);
    return $"<color={color}>{entry.Icon} {entry.Message}</color>";
}
```

### 3. Add Sound/Visual Effects

```csharp
private void DisplayEntry(ActivityEntryDto entry)
{
    // Display text
    RenderEntry(entry);

    // Play sound based on severity
    if (entry.Severity == ActivitySeverity.Combat)
    {
        _audioPlayer.Play("combat_hit.wav");
    }
    else if (entry.Severity == ActivitySeverity.Success)
    {
        _audioPlayer.Play("success.wav");
    }

    // Add visual effect
    if (entry.Severity == ActivitySeverity.Error)
    {
        FlashBorder(Color.Red);
    }
}
```

### 4. Implement Log Persistence

```csharp
private void OnActivityLogChanged(long sequence)
{
    RefreshDisplay();

    // Optionally save to file
    if (_saveLogs)
    {
        var entries = _service.GetRecentEntries(1);
        if (entries.Count > 0)
        {
            AppendToLogFile(entries[0]);
        }
    }
}
```

## Troubleshooting

### Issue: Renderer not updating

**Cause:** Not subscribed to events or observable.

**Solution:** Ensure `Bind()` is called and subscriptions are set up:
```csharp
_service.Changed += OnActivityLogChanged; // Event
// OR
_subscription = _service.OnLogAdded.Subscribe(...); // Observable
```

### Issue: Duplicate entries appearing

**Cause:** Appending instead of replacing on refresh.

**Solution:** Clear display before re-rendering:
```csharp
private void RefreshDisplay()
{
    ClearDisplay(); // Clear first
    var entries = _service.GetRecentEntries(100);
    foreach (var entry in entries)
    {
        DisplayEntry(entry);
    }
}
```

### Issue: Thread access violations

**Cause:** UI updates on non-UI thread.

**Solution:** Marshal to UI thread:
```csharp
_service.Changed += sequence =>
{
    Application.MainLoop.Invoke(() => RefreshDisplay()); // Terminal.Gui
    // OR
    Dispatcher.Invoke(() => RefreshDisplay()); // WPF
    // OR
    InvokeOnMainThread(() => RefreshDisplay()); // Unity
};
```

### Issue: Icons not displaying correctly

**Cause:** Font doesn't support Unicode glyphs.

**Solution:** Use fallback ASCII icons or load a font with Unicode support:
```csharp
private string GetIcon(ActivityEntryDto entry)
{
    if (_supportsUnicode)
        return entry.Icon; // "⚔", "$", etc.

    // Fallback to ASCII
    return entry.Severity switch
    {
        ActivitySeverity.Combat => "X",
        ActivitySeverity.Loot => "$",
        ActivitySeverity.Success => "+",
        _ => "*"
    };
}
```

## Summary Checklist

When implementing a new renderer, ensure you:

- [ ] Reference `LablabBean.Contracts.UI` package
- [ ] Create a renderer class for your UI framework
- [ ] Implement `Bind(IActivityLogService)` method
- [ ] Subscribe to either `Changed` event or `OnLogAdded` observable
- [ ] Implement `RefreshDisplay()` to query and render entries
- [ ] Use `entry.Icon` and `entry.Color` from DTOs
- [ ] Handle null service gracefully
- [ ] Implement auto-scrolling to newest entries
- [ ] Clean up subscriptions on dispose
- [ ] Test with mock service first
- [ ] Add to your application's DI container
- [ ] Document any framework-specific quirks

## Further Reading

- **Activity Log Architecture**: See `docs/ACTIVITY_LOG_ARCHITECTURE.md` (if available)
- **Service Contract**: `dotnet/framework/LablabBean.Contracts.UI/Services/IActivityLogService.cs`
- **Example Renderers**:
  - Terminal.Gui: `dotnet/console-app/LablabBean.Game.TerminalUI/Views/ActivityLogView.cs`
  - SadConsole: `dotnet/windows-app/LablabBean.Game.SadConsole/Renderers/ActivityLogRenderer.cs`
- **Unit Tests**: `dotnet/framework/tests/LablabBean.Contracts.UI.Tests/ActivityLogServiceTests.cs`

---

**Version:** 1.0.0
**Last Updated:** 2025-10-23
**Maintainer:** Lablab-Bean Team
