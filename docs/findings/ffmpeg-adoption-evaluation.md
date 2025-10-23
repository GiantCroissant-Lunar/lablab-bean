---
title: "FFmpeg Adoption Evaluation for Gameplay Recording"
date: "2025-10-22"
category: "findings"
tags: ["ffmpeg", "recording", "gameplay", "evaluation"]
status: "complete"
author: "Kiro AI Assistant"
---

# FFmpeg Adoption Evaluation for Gameplay Recording

**Date:** 2025-10-22
**Status:** ✅ **RECOMMENDATION: ADOPT FFMPEG**
**Priority:** High - Significant development and debugging value

---

## Executive Summary

After evaluating the ffmpeg-calox reference project against the current lablab-bean dungeon crawler, **we recommend adopting FFmpeg for gameplay recording**. The integration would provide substantial benefits for development, testing, and debugging with minimal implementation effort due to architectural alignment.

## Evaluation Context

### Reference Project: ffmpeg-calox

**Location:** `ref-projects/ffmpeg-calox/`
**Status:** Phases 1 & 2 Complete (✅ Production Ready)
**Architecture:** Plugin-based video encoding with URF metadata synchronization

### Target Project: lablab-bean

**Current State:** Fully functional dungeon crawler with SadConsole ASCII graphics
**Architecture:** Event-driven plugin system with ECS game framework

---

## FFmpeg-Calox Capabilities Analysis

### ✅ Core Strengths

**Hybrid Recording System:**

- Synchronized video + URF (Universal Recording Format) metadata
- Professional approach similar to LoL/Dota 2 replay systems
- Complete timeline synchronization for debugging

**Technical Excellence:**

- **Performance:** 60 FPS real-time encoding at 1280x720
- **In-Process:** No external FFmpeg process dependencies
- **Multi-Codec:** H.264, VP9, GIF support
- **Cross-Platform:** Windows, macOS, Linux support

**Plugin Architecture:**

- Loose coupling with graceful degradation
- Optional dependency (works without FFmpeg plugin)
- Environment variable control (`ENABLE_VIDEO=1`)
- Clean integration with existing plugin systems

### 🎯 Key Features

```bash
# Enable hybrid recording
export ENABLE_BOT_MODE=1
export ENABLE_VIDEO=1
./Dungeon.Host.SadConsole --duration=300

# Output structure
recordings/bot-session-20251022-143000/
├── bot-session-20251022-143000.mp4       ← Video (1024x720 @ 60fps)
├── bot-session-20251022-143000.urf.jsonl ← Game events/metadata
├── bot-session-20251022-143000.sync.json ← Synchronization data
└── bot-session-20251022-143000.cast      ← Asciinema export
```

**Automatic Dimension Calculation:**

- Terminal: 128x45 cells → Video: 1024x720 pixels (8x16 per cell)
- Perfect for ASCII-based games like our dungeon crawler

---

## Lablab-Bean Current State Analysis

### ✅ Existing Strengths

**Game Infrastructure:**

- Fully functional dungeon crawler with ECS architecture
- SadConsole ASCII graphics (ideal for recording)
- Turn-based combat, AI systems, procedural generation
- Dual rendering support (Terminal.Gui + SadConsole)

**Plugin Architecture:**

- Event-driven design with 1.1M+ events/sec performance
- Existing plugin system with `IEventBus` and `IRegistry`
- Service contracts and priority-based selection
- Perfect foundation for FFmpeg integration

**Game Features Ready for Recording:**

- Player movement and combat
- AI enemy behaviors (Wander, Chase, Flee, Patrol)
- Procedural dungeon generation
- Turn-based gameplay mechanics

### ❌ Current Gaps

**No Recording Capabilities:**

- No video recording functionality
- No gameplay session recording
- No replay system for debugging
- No analytics data collection from gameplay

---

## Integration Assessment

### 🎯 Perfect Architectural Alignment

**Plugin System Compatibility:**

```csharp
// Existing lablab-bean pattern
services.AddSingleton<IGameService>();
eventBus.Publish(new GameEvent());

// FFmpeg integration would follow same pattern
services.AddSingleton<IVideoEncoder>(); // From FFmpeg plugin
services.AddSingleton<IRecorderService>(); // Hybrid recorder
```

**Event Bus Integration:**

```csharp
// Current game events become URF metadata
eventBus.Publish(new CombatEvent(player, enemy, damage));
eventBus.Publish(new PlayerMovedEvent(oldPos, newPos));
eventBus.Publish(new AIBehaviorChangedEvent(entity, behavior));

// These sync with video timeline for debugging
```

### 📊 Implementation Effort: **LOW**

**Phase 1: Basic Integration (1-2 days)**

1. Copy FFmpeg plugin structure to lablab-bean
2. Add recording contracts to framework
3. Integrate with existing game loop
4. Test basic video recording

**Phase 2: Event Integration (2-3 days)**

1. Map game events to URF format
2. Implement hybrid recorder service
3. Add environment variable controls
4. Test synchronized recording

**Phase 3: Analytics & Debugging (1 week)**

1. Build URF analysis tools
2. Create debugging workflows
3. Add replay capabilities
4. Performance optimization

---

## Expected Benefits

### 🔧 Development Benefits

**AI Behavior Analysis:**

```csharp
// Record AI sessions
export ENABLE_BOT_MODE=1
export ENABLE_VIDEO=1
dotnet run

// Analyze behavior patterns
var chaseEvents = urf.Events.Where(e => e.Type == "AIBehaviorChanged" && e.Data.Contains("Chase"));
// Jump to video timestamps to see visual behavior
```

**Bug Reproduction:**

```csharp
// Find exception in URF
var bug = urf.Events.FirstOrDefault(e => e.Type == "Exception");
// Jump to exact moment in video
video.SeekTo(bug.Timestamp);
// See visual state + internal data
```

### 🧪 Testing Benefits

**Automated Test Verification:**

- Record test runs for visual verification
- Compare expected vs actual gameplay visually
- Create regression test videos

**Performance Analysis:**

- Record frame rate and performance metrics
- Visual verification of smooth gameplay
- Identify performance bottlenecks

### 📚 Documentation Benefits

**Tutorial Creation:**

- Generate gameplay videos automatically
- Create animated GIFs for documentation
- Show feature demonstrations

**Analytics:**

- Player behavior pattern analysis
- Game balance verification
- Feature usage statistics

---

## Technical Implementation Plan

### Phase 1: Core Integration

**Files to Create:**

```
dotnet/framework/
├── LablabBean.Contracts.Recording/
│   ├── IVideoEncoder.cs
│   ├── IRecorderService.cs
│   └── RecordingSyncMetadata.cs
└── LablabBean.Plugins.FFmpeg/
    ├── FFmpegPlugin.cs
    ├── FFmpegVideoEncoder.cs
    └── plugin.json
```

**Integration Points:**

```csharp
// In GameStateManager.cs
private readonly IRecorderService? _recorder;

public async Task StartNewGame()
{
    // Start recording if enabled
    if (_recorder != null && Environment.GetEnvironmentVariable("ENABLE_VIDEO") == "1")
    {
        await _recorder.StartRecordingAsync(sessionId, metadata, cancellationToken);
    }

    // Existing game initialization
    InitializeNewGame();
}
```

### Phase 2: Event Mapping

**URF Event Mapping:**

```csharp
// Map existing events to URF format
public record PlayerMovedEvent(Position From, Position To, DateTimeOffset Timestamp);
public record CombatEvent(EntityId Attacker, EntityId Defender, int Damage, DateTimeOffset Timestamp);
public record AIBehaviorChangedEvent(EntityId Entity, AIBehavior NewBehavior, DateTimeOffset Timestamp);
```

### Phase 3: Environment Controls

**Configuration:**

```bash
# Development mode (URF only)
dotnet run

# Recording mode (URF + Video)
export ENABLE_VIDEO=1
dotnet run

# Bot analysis mode
export ENABLE_BOT_MODE=1
export ENABLE_VIDEO=1
dotnet run --duration=300
```

---

## Risk Assessment

### ⚠️ Potential Challenges

**Low Risk:**

- **Dependencies:** FFmpeg.AutoGen handles native libraries
- **Performance:** Proven 60 FPS capability in reference project
- **Integration:** Plugin architectures are compatible

**Mitigation Strategies:**

- Start with basic video recording only
- Add URF integration incrementally
- Use environment variables for optional features
- Maintain graceful degradation (works without FFmpeg)

### 🛡️ Fallback Plan

If integration proves challenging:

1. Implement URF-only recording first
2. Add video recording as separate phase
3. Use existing event bus for metadata collection
4. Export to external video tools if needed

---

## Success Metrics

### Phase 1 Success Criteria

- [ ] Basic video recording of gameplay sessions
- [ ] No performance impact on game loop
- [ ] Environment variable control working
- [ ] Plugin loads/unloads cleanly

### Phase 2 Success Criteria

- [ ] Synchronized video + URF metadata
- [ ] Game events properly captured in URF
- [ ] Timeline synchronization accurate
- [ ] Debugging workflow functional

### Phase 3 Success Criteria

- [ ] Analytics tools for URF data
- [ ] Replay capabilities working
- [ ] Performance optimized for production
- [ ] Documentation and examples complete

---

## Conclusion

**RECOMMENDATION: ADOPT FFMPEG** ✅

The ffmpeg-calox reference project provides a mature, well-architected solution that aligns perfectly with lablab-bean's plugin architecture and ASCII-based gameplay. The integration effort is minimal due to architectural compatibility, while the benefits for development, testing, and debugging are substantial.

**Key Decision Factors:**

1. **Low Implementation Cost:** Plugin architectures align perfectly
2. **High Value:** Debugging and analytics capabilities
3. **Proven Solution:** ffmpeg-calox is production-ready
4. **Perfect Fit:** ASCII graphics ideal for video recording
5. **Future-Proof:** Extensible for advanced features

**Next Steps:**

1. Create integration specification document
2. Begin Phase 1 implementation
3. Test with existing dungeon crawler
4. Expand to full hybrid recording system

---

**Status:** ✅ **EVALUATION COMPLETE - PROCEED WITH ADOPTION**
**Priority:** High
**Estimated Timeline:** 1-2 weeks for full implementation
**Risk Level:** Low
