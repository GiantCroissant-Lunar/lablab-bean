---
title: "GOAP Adoption Evaluation for Gameplay AI"
date: "2025-10-22"
category: "findings"
tags: ["goap", "ai", "gameplay", "evaluation", "planning"]
status: "complete"
author: "Kiro AI Assistant"
---

# GOAP Adoption Evaluation for Gameplay AI

**Date:** 2025-10-22
**Status:** ✅ **RECOMMENDATION: ADOPT GOAP FOR ADVANCED AI**
**Priority:** Medium - Significant AI enhancement potential with moderate implementation effort

---

## Executive Summary

After evaluating the goap-ckpor reference project against the current lablab-bean dungeon crawler AI system, **we recommend adopting GOAP (Goal-Oriented Action Planning) for advanced AI behaviors**. While the current simple AI system works well for basic gameplay, GOAP would enable sophisticated, emergent AI behaviors that could significantly enhance gameplay depth and player engagement.

## Evaluation Context

### Reference Project: goap-ckpor

**Location:** `ref-projects/goap-ckpor/`
**Status:** Phase 2 Complete (Source copied, namespaces updated)
**Architecture:** .NET adaptation of CrashKonijn's Unity GOAP library

### Target Project: lablab-bean

**Current AI:** Simple behavior-based system (Wander, Chase, Flee, Patrol, Idle)
**Architecture:** ECS-based with Arch framework, turn-based energy system

---

## GOAP-CKPor Capabilities Analysis

### ✅ Core GOAP Architecture

**Goal-Oriented Planning:**

- **Agents** pursue **Goals** (desired world states)
- **Actions** have **Preconditions** and **Effects**
- **Planner** finds optimal action sequences to achieve goals
- **Sensors** provide world state information

**Technical Implementation:**

```csharp
// GOAP planning process
var resolver = new GraphResolver(actions, keyResolver);
var runData = configure(resolver);
var result = resolver.StartResolve(runData).Complete();
```

### 🎯 Key Advantages Over Simple AI

**Emergent Behavior:**

- AI can discover new behavior combinations
- Complex multi-step plans emerge from simple actions
- Adaptive responses to changing conditions

**Maintainable Architecture:**

- Actions are modular and reusable
- Goals define "what" without specifying "how"
- Easy to add new behaviors without modifying existing code

**Professional-Grade AI:**

- Used in AAA games (F.E.A.R., Tomb Raider, etc.)
- Proven scalable architecture
- Sophisticated decision-making capabilities

### 🔧 Unity → .NET Adaptations

**Successfully Adapted:**

```csharp
// Unity → .NET replacements
NativeArray<T>     → T[] or List<T>
NativeHashMap<K,V> → Dictionary<K,V>
MonoBehaviour      → Interface-based services
Transform          → ITransformable interface
Vector3            → Position3D struct
Coroutines         → async/await
```

**Plugin Integration:**

```json
{
  "interface": "Plate.CrossMilo.Contracts.Goap.IGoapPlannerService",
  "implementation": "Plate.Plugins.GoapCkpor.GoapCkporService",
  "lifecycle": "singleton",
  "priority": 50
}
```

---

## Current Lablab-Bean AI Analysis

### ✅ Existing Strengths

**Functional Basic AI:**

- 5 behavior types: Wander, Chase, Flee, Patrol, Idle
- Pathfinding integration with GoRogue
- Energy-based turn system
- Aggro range optimization (only activates nearby enemies)

**ECS Architecture:**

```csharp
public struct AI
{
    public AIBehavior Behavior { get; set; }
}

public enum AIBehavior
{
    Wander, Chase, Flee, Patrol, Idle
}
```

**Performance Optimized:**

- Only processes AI within 15-tile aggro range
- Efficient ECS queries with Arch framework
- Turn-based system prevents AI spam

### ❌ Current Limitations

**Simple State Machine:**

- Fixed behavior per entity
- No dynamic goal switching
- Limited decision-making capability
- No multi-step planning

**Hardcoded Logic:**

```csharp
// Current AI is procedural, not goal-oriented
switch (behavior)
{
    case AIBehavior.Chase:
        // Hardcoded chase logic
        break;
    case AIBehavior.Flee:
        // Hardcoded flee logic
        break;
}
```

**No Emergent Behavior:**

- AI cannot adapt to new situations
- No complex multi-action sequences
- Limited interaction between different AI entities

---

## GOAP vs Current AI Comparison

### Scenario: Goblin Behavior

**Current Simple AI:**

```csharp
// Goblin has fixed "Chase" behavior
if (playerNearby) Chase();
else Wander();
```

**GOAP AI Potential:**

```csharp
// Goblin has multiple goals and actions
Goals: [AttackPlayer, FindWeapon, CallForHelp, Survive]
Actions: [MoveToTarget, AttackMelee, PickupItem, Shout, Flee]

// AI dynamically plans:
// 1. If weak + no weapon → FindWeapon goal → PickupItem action
// 2. If strong + weapon → AttackPlayer goal → MoveToTarget + AttackMelee
// 3. If injured → Survive goal → CallForHelp + Flee actions
```

### Complexity Comparison

| Feature | Current AI | GOAP AI |
|---------|------------|---------|
| **Behavior Types** | 5 fixed behaviors | Unlimited emergent combinations |
| **Decision Making** | Simple state machine | Multi-step planning |
| **Adaptability** | Static behavior assignment | Dynamic goal prioritization |
| **Extensibility** | Modify switch statements | Add new actions/goals |
| **Emergent Behavior** | None | Complex sequences emerge |
| **Maintenance** | Procedural code | Declarative actions/goals |

---

## Integration Assessment

### 🎯 Architectural Compatibility

**ECS Integration:**

```csharp
// Current ECS components can be enhanced
public struct AI
{
    public AIBehavior Behavior { get; set; }  // Keep for simple AI
    public IGoapAgent? GoapAgent { get; set; } // Add for advanced AI
    public bool UseGoap { get; set; }          // Toggle between systems
}
```

**Hybrid Approach:**

- Keep simple AI for basic enemies (goblins, rats)
- Use GOAP for complex enemies (bosses, intelligent creatures)
- Gradual migration path

### 📊 Implementation Effort: **MEDIUM**

**Phase 1: Basic Integration (1 week)**

1. Copy goap-ckpor to lablab-bean framework
2. Create basic dungeon-specific actions and goals
3. Implement hybrid AI system (simple + GOAP)
4. Test with one enemy type

**Phase 2: Game-Specific Implementation (2 weeks)**

1. Define comprehensive action library
2. Create goal hierarchy for different enemy types
3. Integrate with existing ECS and pathfinding
4. Performance optimization

**Phase 3: Advanced Features (1-2 weeks)**

1. Multi-agent coordination
2. Dynamic goal prioritization
3. Learning and adaptation
4. Complex boss behaviors

---

## Expected Benefits

### 🧠 Enhanced AI Behaviors

**Intelligent Enemy Types:**

```csharp
// Orc Warrior GOAP setup
Goals: [DefendTerritory, AttackIntruders, MaintainWeapon, Survive]
Actions: [Patrol, Investigate, Attack, Block, Retreat, RepairWeapon]

// Emergent behavior examples:
// - Hears noise → Investigate → finds player → Attack
// - Weapon breaks → MaintainWeapon → finds repair kit → RepairWeapon
// - Low health → Survive → Retreat to safe area
```

**Boss AI Complexity:**

```csharp
// Dragon Boss GOAP setup
Goals: [ProtectTreasure, EliminateThreats, MaintainDominance]
Actions: [Breathe Fire, Fly, Land, Summon Minions, Taunt, Heal]

// Complex sequences:
// - Player enters → ProtectTreasure → Summon Minions + Breathe Fire
// - Low health → MaintainDominance → Fly to unreachable area + Heal
// - Multiple players → EliminateThreats → Area attacks + tactical positioning
```

### 🎮 Gameplay Improvements

**Emergent Storytelling:**

- AI creates unexpected situations
- Players develop counter-strategies
- Each playthrough feels different

**Scalable Difficulty:**

- Simple enemies use basic AI
- Advanced enemies use GOAP
- Boss encounters become tactical puzzles

**Player Engagement:**

- AI feels more intelligent and responsive
- Combat becomes more strategic
- Exploration rewards tactical thinking

### 🔧 Development Benefits

**Maintainable AI:**

```csharp
// Adding new behavior is simple
public class CallForHelpAction : ActionBase
{
    public override bool IsValid(WorldState state) => state.Health < 30;
    public override void Execute() => SpawnReinforcements();
}

// No need to modify existing AI code
```

**Designer-Friendly:**

- Non-programmers can create AI behaviors
- Actions and goals are self-documenting
- Easy to balance and tune

---

## Technical Implementation Plan

### Phase 1: Foundation Setup

**Files to Create:**

```
dotnet/framework/
├── LablabBean.Contracts.AI/
│   ├── IGoapAgent.cs
│   ├── IAction.cs
│   ├── IGoal.cs
│   └── WorldState.cs
├── LablabBean.Plugins.GOAP/
│   ├── GoapPlugin.cs
│   ├── DungeonGoapAgent.cs
│   └── plugin.json
└── LablabBean.Game.Core/AI/
    ├── Actions/
    │   ├── MoveToTargetAction.cs
    │   ├── AttackAction.cs
    │   └── FleeAction.cs
    └── Goals/
        ├── AttackPlayerGoal.cs
        ├── SurviveGoal.cs
        └── PatrolGoal.cs
```

**Integration Points:**

```csharp
// Enhanced AISystem.cs
public void ProcessAI(World world, DungeonMap map)
{
    world.Query(in query, (Entity entity, ref AI ai, ref Actor actor, ref Position pos) =>
    {
        if (ai.UseGoap && ai.GoapAgent != null)
        {
            ProcessGoapAI(entity, ai.GoapAgent, world, map);
        }
        else
        {
            ProcessSimpleAI(entity, ai.Behavior, world, map); // Existing logic
        }
    });
}
```

### Phase 2: Action Library

**Core Actions:**

```csharp
// Movement actions
public class MoveToTargetAction : ActionBase
{
    public override Preconditions => new() { ["HasTarget"] = true };
    public override Effects => new() { ["AtTarget"] = true };
}

// Combat actions
public class AttackMeleeAction : ActionBase
{
    public override Preconditions => new() { ["AtTarget"] = true, ["HasWeapon"] = true };
    public override Effects => new() { ["TargetDamaged"] = true };
}

// Utility actions
public class PickupItemAction : ActionBase
{
    public override Preconditions => new() { ["ItemNearby"] = true };
    public override Effects => new() { ["HasItem"] = true };
}
```

### Phase 3: Goal Hierarchy

**Enemy Goals:**

```csharp
public class AttackPlayerGoal : GoalBase
{
    public override int Priority => 100;
    public override Conditions => new() { ["PlayerDefeated"] = true };
}

public class SurviveGoal : GoalBase
{
    public override int Priority => 200; // Higher priority when health low
    public override Conditions => new() { ["InSafeLocation"] = true };
}
```

---

## Risk Assessment

### ⚠️ Potential Challenges

**Medium Risk:**

- **Complexity:** GOAP is more complex than simple state machines
- **Performance:** Planning overhead vs simple behavior switching
- **Learning Curve:** Team needs to understand GOAP concepts

**Mitigation Strategies:**

- Start with hybrid approach (simple + GOAP)
- Use GOAP only for complex enemies initially
- Extensive testing and performance monitoring
- Comprehensive documentation and examples

### 🛡️ Fallback Plan

If GOAP proves too complex:

1. Keep simple AI as primary system
2. Use GOAP only for boss encounters
3. Implement gradual migration over multiple releases
4. Focus on specific high-value scenarios

---

## Success Metrics

### Phase 1 Success Criteria

- [ ] GOAP plugin loads and integrates with ECS
- [ ] At least one enemy type uses GOAP successfully
- [ ] Performance impact < 10% vs simple AI
- [ ] Hybrid system works (simple + GOAP coexist)

### Phase 2 Success Criteria

- [ ] 3+ enemy types with distinct GOAP behaviors
- [ ] Complex multi-step AI sequences working
- [ ] Boss encounters use advanced GOAP features
- [ ] Player feedback indicates more engaging AI

### Phase 3 Success Criteria

- [ ] Multi-agent coordination working
- [ ] Dynamic difficulty scaling via AI complexity
- [ ] Designer tools for creating AI behaviors
- [ ] Performance optimized for production

---

## Comparison with Current AI

### When to Use Simple AI

- **Basic enemies** (rats, weak goblins)
- **Performance-critical scenarios** (large numbers of enemies)
- **Simple, predictable behaviors**
- **Rapid prototyping**

### When to Use GOAP AI

- **Intelligent enemies** (orcs, mages, bosses)
- **Complex multi-step behaviors**
- **Adaptive and emergent gameplay**
- **Memorable encounters**

### Hybrid Approach Benefits

- **Best of both worlds:** Performance + sophistication
- **Gradual adoption:** Low-risk migration path
- **Scalable complexity:** Match AI to enemy importance
- **Development flexibility:** Choose right tool for each scenario

---

## Conclusion

**RECOMMENDATION: ADOPT GOAP FOR ADVANCED AI** ✅

The goap-ckpor reference project provides a mature, well-adapted GOAP implementation that would significantly enhance the AI capabilities of lablab-bean's dungeon crawler. While the current simple AI system works well for basic gameplay, GOAP would enable sophisticated, emergent behaviors that could transform the game experience.

**Key Decision Factors:**

1. **Moderate Implementation Cost:** Hybrid approach reduces risk
2. **High Value for Complex AI:** Boss fights and intelligent enemies
3. **Proven Technology:** GOAP is industry-standard for advanced AI
4. **Scalable Adoption:** Can start small and expand gradually
5. **Enhanced Player Experience:** More engaging and unpredictable AI

**Recommended Approach:**

1. **Hybrid System:** Keep simple AI for basic enemies, add GOAP for complex ones
2. **Gradual Migration:** Start with one boss enemy, expand over time
3. **Performance Focus:** Monitor and optimize throughout development
4. **Player-Centric:** Focus on behaviors that enhance gameplay experience

**Next Steps:**

1. Create GOAP integration specification
2. Implement Phase 1 with hybrid AI system
3. Test with boss encounter scenarios
4. Gather player feedback and iterate

---

**Status:** ✅ **EVALUATION COMPLETE - PROCEED WITH HYBRID ADOPTION**
**Priority:** Medium
**Estimated Timeline:** 4-6 weeks for full implementation
**Risk Level:** Medium (mitigated by hybrid approach)
