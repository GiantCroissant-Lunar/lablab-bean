# Phase 4 Memory System - File Structure

## Directory Layout

```
LablabBean.Plugins.NPC/
├── Components/
│   └── NPC.cs
├── Data/
│   └── [dialogue trees]
├── Services/
│   ├── NPCService.cs
│   └── MemoryEnhancedNPCService.cs         ⭐ Enhanced service
├── Systems/
│   ├── NPCSystem.cs
│   ├── DialogueSystem.cs
│   └── MemoryEnhancedDialogueSystem.cs     ⭐ Memory integration
├── Extensions/
│   └── NPCMemoryExtensions.cs              ⭐ DI registration
├── Examples/                                ⭐ NEW! Phase 4 refinements
│   ├── RelationshipBasedMerchant.cs        ⭐ Dynamic pricing
│   ├── ContextAwareGreetingSystem.cs       ⭐ Smart greetings
│   ├── MemoryVisualizationDashboard.cs     ⭐ Debug tools
│   ├── USAGE_EXAMPLES.md                   ⭐ Complete guide
│   ├── QUICK_REFERENCE.md                  ⭐ Developer cheat sheet
│   └── ARCHITECTURE.md                     ⭐ System design
├── PHASE4_MEMORY_INTEGRATION.md
├── PHASE4_REFINEMENT_SUMMARY.md            ⭐ NEW!
└── NPC_SYSTEM_PHASE4_SUMMARY.md
```

## File Purposes

### Core System (Phase 4 Base)

| File | Purpose | Status |
|------|---------|--------|
| MemoryEnhancedNPCService.cs | Main orchestration service | ✅ Complete |
| MemoryEnhancedDialogueSystem.cs | Dialogue + memory dual-write | ✅ Complete |
| NPCMemoryExtensions.cs | DI container setup | ✅ Complete |

### Examples (Phase 4 Refinement)

| File | Purpose | Lines | Status |
|------|---------|-------|--------|
| RelationshipBasedMerchant.cs | Dynamic pricing example | 160 | ✅ Complete |
| ContextAwareGreetingSystem.cs | Smart greeting generation | 220 | ✅ Complete |
| MemoryVisualizationDashboard.cs | Debug/analytics dashboard | 280 | ✅ Complete |

### Documentation (Phase 4 Refinement)

| File | Purpose | Lines | Audience |
|------|---------|-------|----------|
| USAGE_EXAMPLES.md | Complete usage guide | 500+ | All developers |
| QUICK_REFERENCE.md | Developer cheat sheet | 250+ | Quick lookup |
| ARCHITECTURE.md | System design docs | 300+ | Architects |
| PHASE4_REFINEMENT_SUMMARY.md | Executive summary | 250+ | Management |

## Usage Paths

### Minimal Integration

```
Program.cs
    └─> services.AddMemoryEnhancedNPC(configuration)

Game Code
    └─> Inject MemoryEnhancedNPCService
        └─> Use like regular NPCService
```

### Full Integration with Examples

```
Program.cs
    ├─> services.AddMemoryEnhancedNPC(configuration)
    ├─> services.AddSingleton<RelationshipBasedMerchant>()
    ├─> services.AddSingleton<ContextAwareGreetingSystem>()
    └─> services.AddSingleton<MemoryVisualizationDashboard>()

Shop System
    └─> Inject RelationshipBasedMerchant
        ├─> GetPriceAsync() → Dynamic pricing
        ├─> CanAccessExclusiveItemsAsync() → Item unlocks
        └─> GetPersonalizedGreetingAsync() → Custom text

Dialogue Handler
    └─> Inject ContextAwareGreetingSystem
        ├─> GenerateGreetingAsync() → Context-aware intro
        └─> GenerateFarewellAsync() → Smart goodbye

Admin Panel
    └─> Inject MemoryVisualizationDashboard
        ├─> PrintDashboardAsync() → Quick overview
        └─> PrintNpcReportAsync() → Detailed analysis
```

## Dependencies

```
Examples/
    ├── RelationshipBasedMerchant
    │   ├─> MemoryEnhancedNPCService
    │   └─> RelationshipLevel enum
    │
    ├── ContextAwareGreetingSystem
    │   ├─> MemoryEnhancedNPCService
    │   └─> RelationshipLevel enum
    │
    └── MemoryVisualizationDashboard
        ├─> MemoryEnhancedNPCService
        ├─> IMemoryService
        └─> RelationshipLevel enum

MemoryEnhancedNPCService
    ├─> NPCSystem
    ├─> MemoryEnhancedDialogueSystem
    └─> World

MemoryEnhancedDialogueSystem
    ├─> DialogueSystem (base)
    ├─> IMemoryService
    └─> World
```

## Import Map

### For Basic Usage

```csharp
using LablabBean.Plugins.NPC.Extensions;  // AddMemoryEnhancedNPC
using LablabBean.Plugins.NPC.Services;    // MemoryEnhancedNPCService
```

### For Examples

```csharp
using LablabBean.Plugins.NPC.Examples;    // All example classes
using LablabBean.Plugins.NPC.Systems;     // RelationshipLevel enum
```

## Example Code Locations

### Dynamic Pricing

- **Implementation**: `Examples/RelationshipBasedMerchant.cs`
- **Example Usage**: `Examples/USAGE_EXAMPLES.md` (lines 50-120)
- **Quick Ref**: `Examples/QUICK_REFERENCE.md` (lines 30-60)

### Context Greetings

- **Implementation**: `Examples/ContextAwareGreetingSystem.cs`
- **Example Usage**: `Examples/USAGE_EXAMPLES.md` (lines 150-230)
- **Quick Ref**: `Examples/QUICK_REFERENCE.md` (lines 90-120)

### Memory Dashboard

- **Implementation**: `Examples/MemoryVisualizationDashboard.cs`
- **Example Usage**: `Examples/USAGE_EXAMPLES.md` (lines 270-350)
- **Quick Ref**: `Examples/QUICK_REFERENCE.md` (lines 150-180)

## Testing Locations

### Unit Tests (Recommended)

```
tests/LablabBean.Plugins.NPC.Tests/
├── Examples/
│   ├── RelationshipBasedMerchantTests.cs    (to be created)
│   ├── ContextAwareGreetingSystemTests.cs   (to be created)
│   └── MemoryVisualizationDashboardTests.cs (to be created)
└── Integration/
    └── MemoryEnhancedNPCIntegrationTests.cs (exists)
```

## Configuration

### appsettings.json

```json
{
  ""KernelMemory"": {
    ""VectorDb"": ""SimpleVectorDb"",
    ""Embeddings"": {
      ""Type"": ""OpenAI"",
      ""Model"": ""text-embedding-3-small""
    }
  }
}
```

### Environment Variables

```
OPENAI_API_KEY=your_key_here
```

---

**File Structure Guide** | Phase 4 Memory System
**Version**: 1.0 | **Last Updated**: 2025-10-25
