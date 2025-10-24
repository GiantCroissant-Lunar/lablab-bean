# Phase 4: User Story 3 - NPC Interactions & Dialogue

**Date**: 2025-10-23
**Status**: 🚀 Starting Implementation
**Goal**: Full NPC dialogue system with branching trees, conditions, and actions

## 📋 Task List (T046-T061)

### Dialogue Data Models (T046-T049) - PARALLEL ✅

- [ ] T046 Create DialogueTree data class
- [ ] T047 Create DialogueNode data class
- [ ] T048 Create DialogueChoice data class
- [ ] T049 Create DialogueAction data class

### Enhanced Dialogue System (T050-T052)

- [ ] T050 Enhance DialogueSystem with condition evaluation
- [ ] T051 Implement dialogue action execution
- [ ] T052 Add NPC state persistence

### Enhanced NPC Service (T053-T055)

- [ ] T053 Enhance NPCService with full dialogue tree navigation
- [ ] T054 Implement NPC state management
- [ ] T055 Add dialogue condition parser (simple DSL)

### Sample Data (T056-T058) - PARALLEL

- [ ] T056 Create complex dialogue tree samples
- [ ] T057 Create lore NPC samples
- [ ] T058 Create merchant dialogue templates

### UI Enhancement (T059-T061)

- [ ] T059 Enhance DialogueScreen with choice filtering
- [ ] T060 Add dialogue history display
- [ ] T061 Add NPC type indicators

---

## 🎯 Success Criteria

**Functional Requirements**:

- ✅ Multi-branch dialogue trees work correctly
- ✅ Conditional dialogue choices (based on level, items, quests)
- ✅ Dialogue actions (AcceptQuest, CompleteQuest, OpenTrade, SetNPCState)
- ✅ NPC state persistence (remembers previous interactions)
- ✅ Dialogue history tracking

**Test Scenario**:

1. Place NPC with multi-branch dialogue tree
2. Player interacts with NPC
3. Dialogue presents conditional choices (some disabled if conditions not met)
4. Player selects choice → triggers action (accept quest, trade, etc.)
5. NPC remembers state in future interactions
6. Different paths available based on previous choices

---

## 📊 Architecture Overview

```
DialogueTree (JSON)
  ├── nodes[]
  │   ├── DialogueNode
  │   │   ├── id: string
  │   │   ├── text: string
  │   │   ├── npcName: string
  │   │   ├── choices[]
  │   │   │   ├── DialogueChoice
  │   │   │   │   ├── text: string
  │   │   │   │   ├── nextNodeId: string
  │   │   │   │   ├── condition: string (DSL)
  │   │   │   │   ├── actions[]
  │   │   │   │   │   └── DialogueAction
  │   │   │   │   │       ├── type: enum
  │   │   │   │   │       └── parameters: Dictionary
  └── startNodeId: string
```

### Condition DSL Examples

```
"level >= 5"
"hasItem('rusty_key')"
"hasQuest('retrieve_artifact')"
"questComplete('retrieve_artifact')"
"npcState('met_before') == true"
"level >= 5 AND hasItem('key')"
"hasQuest('q1') OR questComplete('q2')"
```

### Action Types

```csharp
public enum DialogueActionType
{
    AcceptQuest,      // params: { questId: string }
    CompleteQuest,    // params: { questId: string }
    SetNPCState,      // params: { key: string, value: object }
    GiveItem,         // params: { itemId: string, quantity: int }
    TakeItem,         // params: { itemId: string, quantity: int }
    GiveGold,         // params: { amount: int }
    TakeGold,         // params: { amount: int }
    OpenTrade,        // params: { merchantId: string }
    TriggerEvent      // params: { eventName: string }
}
```

---

## 🔄 Integration Points

### With Quest System (US1)

- Dialogue actions can accept/complete quests
- Dialogue conditions check quest state
- Quest givers use dialogue trees

### With Merchant System (US5 - future)

- Dialogue can trigger trade interface
- Merchant NPCs have trade-specific dialogue

### With Progression System (US2 - future)

- Dialogue conditions check player level
- Level-gated dialogue options

---

## 📁 File Structure

```
dotnet/plugins/LablabBean.Plugins.NPC/
├── Components/
│   ├── NPC.cs                      ✅ EXISTS
│   └── DialogueState.cs            ✅ EXISTS
├── Data/
│   ├── DialogueTree.cs             ⭐ NEW (T046)
│   ├── DialogueNode.cs             ⭐ NEW (T047)
│   ├── DialogueChoice.cs           ⭐ NEW (T048)
│   ├── DialogueAction.cs           ⭐ NEW (T049)
│   ├── NPCs/
│   │   ├── eldrin-questgiver.json  ✅ EXISTS
│   │   ├── lorekeeper.json         ⭐ NEW (T057)
│   │   └── sage.json               ⭐ NEW (T057)
│   └── Dialogue/
│       ├── eldrin-dialogue.json    ✅ EXISTS
│       ├── complex-dialogue.json   ⭐ NEW (T056)
│       └── merchant-template.json  ⭐ NEW (T058)
├── Systems/
│   ├── NPCSystem.cs                ✅ EXISTS
│   └── DialogueSystem.cs           🔧 ENHANCE (T050-T052)
├── Services/
│   └── NPCService.cs               🔧 ENHANCE (T053-T055)
├── NPCPlugin.cs                    ✅ EXISTS
└── plugin.json                     ✅ EXISTS

dotnet/console-app/LablabBean.Game.TerminalUI/Views/
└── DialogueView.cs                 🔧 ENHANCE (T059-T061)
```

---

## 🚀 Implementation Strategy

### Step 1: Data Models (Parallel - 30 min)

Create all 4 data classes (DialogueTree, DialogueNode, DialogueChoice, DialogueAction)

### Step 2: Dialogue System Enhancement (60 min)

- Add condition evaluator
- Add action executor
- Add state persistence

### Step 3: NPC Service Enhancement (45 min)

- Full dialogue navigation
- State management
- Condition parser/DSL

### Step 4: Sample Data (Parallel - 30 min)

Create sample dialogue trees and NPCs

### Step 5: UI Enhancement (30 min - DEFERRED if Terminal.Gui issues)

Update DialogueView (if we fix Terminal.Gui API issues)

**Total Estimated Time**: ~3-4 hours

---

## 📝 Notes

- Building on existing NPC plugin from Phase 3
- DialogueSystem.cs already exists (basic version)
- NPCService.cs already exists (minimal version)
- We'll enhance these with full dialogue tree support
- Terminal.Gui UI is optional (can defer if needed)

---

**Ready to start**: T046-T049 (data models) - can all be done in parallel
