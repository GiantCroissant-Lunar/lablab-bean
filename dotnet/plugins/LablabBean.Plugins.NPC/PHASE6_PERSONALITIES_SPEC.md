# Phase 6: Advanced Dialogue Generation & NPC Personalities

**Status**: 🚀 Starting
**Priority**: High
**Started**: 2025-10-25
**Depends On**: Phase 4 (Memory), Phase 5 (Combat Memory)

---

## 📋 Overview

**Goal**: Create a dynamic dialogue generation system that combines memory, combat history, and NPC personalities to generate contextually rich, personality-driven conversations.

**Key Features**:

- Personality-driven dialogue generation
- Context-aware responses using memory + combat history
- Emotional dialogue variations
- Dynamic quest/trading dialogue
- Integration with existing systems

---

## 🎯 User Stories

### US1: Personality-Driven Dialogue

**As a** player
**I want** NPCs to have distinct personalities reflected in their speech
**So that** each NPC feels unique and memorable

**Acceptance Criteria**:

- ✅ NPCs have personality traits (friendly, grumpy, fearful, brave, etc.)
- ✅ Dialogue tone matches personality
- ✅ Word choice reflects personality
- ✅ Different NPCs respond differently to same situation

### US2: Combat-Aware Dialogue

**As a** player
**I want** NPCs to reference our combat history in dialogue
**So that** conversations feel connected to gameplay

**Acceptance Criteria**:

- ✅ NPCs mention previous fights
- ✅ Dialogue changes based on win/loss record
- ✅ Emotional states affect dialogue tone
- ✅ Rivals have unique dialogue

### US3: Memory-Enhanced Conversations

**As a** player
**I want** NPCs to reference past interactions in dialogue
**So that** relationships feel meaningful and persistent

**Acceptance Criteria**:

- ✅ NPCs recall previous topics discussed
- ✅ Relationship level affects dialogue options
- ✅ Recent interactions influence responses
- ✅ Long-term memory creates continuity

---

## 🏗️ Architecture

### New Components

#### 1. NPCPersonality Component

```csharp
public struct NPCPersonality
{
    public PersonalityTraits Traits { get; set; }
    public DialogueStyle Style { get; set; }
    public EmotionalRange EmotionalRange { get; set; }
    public Dictionary<string, float> TraitValues { get; set; } // 0.0 to 1.0
}

public struct PersonalityTraits
{
    public float Friendliness { get; set; }  // 0.0 (hostile) to 1.0 (friendly)
    public float Courage { get; set; }       // 0.0 (coward) to 1.0 (brave)
    public float Formality { get; set; }     // 0.0 (casual) to 1.0 (formal)
    public float Honesty { get; set; }       // 0.0 (deceptive) to 1.0 (honest)
    public float Chattiness { get; set; }    // 0.0 (terse) to 1.0 (verbose)
    public float Greed { get; set; }         // 0.0 (generous) to 1.0 (greedy)
}

public enum DialogueStyle
{
    Formal,      // "Good day, traveler."
    Casual,      // "Hey there!"
    Gruff,       // "What do you want?"
    Flowery,     // "Greetings, noble adventurer!"
    Minimal      // "Yeah?"
}

public enum EmotionalRange
{
    Stable,      // Emotions change slowly
    Volatile,    // Emotions change quickly
    Stoic,       // Shows little emotion
    Expressive   // Shows emotions clearly
}
```

#### 2. DialogueContext Component

```csharp
public struct DialogueContext
{
    public DialogueType CurrentType { get; set; }
    public string? CurrentTopic { get; set; }
    public DateTime LastDialogue { get; set; }
    public int DialogueCount { get; set; }
    public Dictionary<string, int> TopicMentions { get; set; }
}

public enum DialogueType
{
    Greeting,
    Farewell,
    Combat,
    Trading,
    Quest,
    Casual,
    Intimidation,
    Persuasion
}
```

### New Systems

#### 1. PersonalityDialogueSystem

**Responsibilities**:

- Generate dialogue based on personality traits
- Apply dialogue style transformations
- Adjust tone for emotional state
- Select appropriate vocabulary

**Key Methods**:

- `GenerateGreeting(Entity npc, Entity player, DialogueContext context)`
- `GenerateResponse(Entity npc, string playerInput, DialogueContext context)`
- `ApplyPersonalityTone(string baseDialogue, NPCPersonality personality)`
- `SelectVocabulary(DialogueStyle style, EmotionalState emotion)`

#### 2. ContextualDialogueSystem

**Responsibilities**:

- Combine memory, combat history, and personality
- Generate contextually rich responses
- Reference past events appropriately
- Maintain conversation flow

**Key Methods**:

- `GenerateContextualDialogue(Entity npc, Entity player, DialogueType type)`
- `ReferenceMemory(string baseDialogue, List<Interaction> recentMemories)`
- `ReferenceCombat(string baseDialogue, CombatHistory combatHistory)`
- `IntegrateAllContexts(NPCPersonality, MemoryContext, CombatContext)`

### New Services

#### IDialogueGenerationService

```csharp
public interface IDialogueGenerationService
{
    Task<DialogueResponse> GenerateDialogueAsync(
        Guid npcId,
        Guid playerId,
        DialogueType type,
        string? playerInput = null);

    Task<List<DialogueOption>> GenerateDialogueOptionsAsync(
        Guid npcId,
        Guid playerId,
        DialogueType type);

    Task<string> GenerateGreetingAsync(
        Guid npcId,
        Guid playerId);

    Task<string> GenerateResponseAsync(
        Guid npcId,
        Guid playerId,
        string playerStatement);
}

public class DialogueResponse
{
    public string Text { get; set; }
    public DialogueEmotion Emotion { get; set; }
    public List<DialogueOption> Options { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class DialogueOption
{
    public string Id { get; set; }
    public string Text { get; set; }
    public DialogueType Type { get; set; }
    public bool RequiresRelationship { get; set; }
    public int MinRelationshipLevel { get; set; }
}

public enum DialogueEmotion
{
    Neutral,
    Happy,
    Angry,
    Sad,
    Fearful,
    Surprised,
    Contemptuous
}
```

---

## 🔄 Integration Points

### 1. With Phase 4 (Memory System)

```csharp
public async Task<string> GenerateMemoryAwareDialogue(Entity npc, Entity player)
{
    // Get relationship and recent interactions
    var relationship = await memoryService.GetRelationshipAsync(npc.Id, player.Id);
    var recentMemories = await memoryService.GetRecentInteractionsAsync(npc.Id, player.Id, 5);

    // Generate base dialogue
    var baseDialogue = GenerateBaseDialogue(npc, player);

    // Add memory references
    if (relationship.Level == RelationshipLevel.CloseFriend)
    {
        baseDialogue += " It's always good to see a close friend.";
    }

    if (recentMemories.Any(m => m.Topics.Contains("weapons")))
    {
        baseDialogue += " Still looking for quality weapons?";
    }

    return baseDialogue;
}
```

### 2. With Phase 5 (Combat Memory)

```csharp
public async Task<string> GenerateCombatAwareDialogue(Entity npc, Entity player)
{
    var combatHistory = await combatMemoryService.GetCombatHistoryAsync(npc.Id, player.Id);
    var personality = npc.Get<NPCPersonality>();

    if (combatHistory == null)
    {
        return "I haven't crossed blades with you before.";
    }

    // Adjust dialogue based on combat relationship
    switch (combatHistory.Relationship)
    {
        case CombatRelationship.Afraid:
            return personality.Traits.Courage < 0.3
                ? "Please, I don't want any trouble!"
                : "I... I remember you. Let's not fight again.";

        case CombatRelationship.Rival:
            return "Ah, my worthy opponent returns! Ready for another bout?";

        case CombatRelationship.Nemesis:
            return "YOU! This ends today!";

        case CombatRelationship.Hunter:
            return "I've been waiting for you. Time for revenge!";

        default:
            return "Hello there.";
    }
}
```

### 3. With Dialogue System

```csharp
public class EnhancedDialogueHandler
{
    public async Task<string> StartDialogue(Entity npc, Entity player)
    {
        // Get all contexts
        var personality = npc.Get<NPCPersonality>();
        var emotion = npc.Get<CombatMemory>()?.EmotionalState ?? CombatEmotionalState.Neutral;
        var relationship = await memoryService.GetRelationshipAsync(npc.Id, player.Id);
        var combatHistory = await combatMemoryService.GetCombatHistoryAsync(npc.Id, player.Id);

        // Generate contextual greeting
        var greeting = await dialogueGenerationService.GenerateGreetingAsync(npc.Id, player.Id);

        // Apply personality tone
        greeting = ApplyPersonalityTone(greeting, personality);

        // Add emotional inflection
        greeting = ApplyEmotionalTone(greeting, emotion);

        return greeting;
    }
}
```

---

## 📊 Example Scenarios

### Scenario 1: Friendly Merchant (High Friendliness, Casual Style)

**Context**: Player has bought 5 items, no combat history

```
Greeting (First visit):
"Hey there, friend! Welcome to my shop!"

Greeting (After friendship):
"Ah, my favorite customer! Good to see you again!"

Trading Dialogue:
"For you? I can do 15% off. Friends get the best deals!"

Combat Reference (if fought once):
"Look, about that scuffle... water under the bridge, yeah? Let's just do business."
```

### Scenario 2: Grumpy Guard (Low Friendliness, Gruff Style)

**Context**: Player is stranger, no history

```
Greeting:
"What do you want?"

After Building Relationship:
"You again. At least you're not trouble."

Combat Reference (After losing twice):
"Don't get any ideas. I remember what you did."
```

### Scenario 3: Cowardly Bandit (Low Courage, After Multiple Defeats)

**Context**: Lost 4 fights, relationship = Afraid

```
Greeting:
"No! Please, not you again! I yield!"

Dialogue:
"I-I have information! Valuable information! Just don't hurt me!"

If Approached Aggressively:
*Flees immediately* "I'm not stupid enough to fight you!"
```

### Scenario 4: Honorable Rival (High Courage, Evenly Matched)

**Context**: 3 wins, 3 losses each, relationship = Rival

```
Greeting:
"Ah, my worthy opponent! I've been training since our last encounter."

Dialogue:
"You fight with skill and honor. It's rare to find a true challenge."

Challenge Dialogue:
"Shall we test our mettle once more? I believe I've learned your patterns."
```

---

## 🎨 Personality Templates

### Template 1: The Cheerful Merchant

```yaml
personality:
  traits:
    friendliness: 0.9
    courage: 0.5
    formality: 0.3
    honesty: 0.8
    chattiness: 0.9
    greed: 0.4
  style: Casual
  emotional_range: Expressive

dialogue_patterns:
  greeting: ["Hey there!", "Welcome, friend!", "Good to see you!"]
  farewell: ["Come back soon!", "Take care!", "See you around!"]
  trading: ["For you? Great price!", "I can do a deal!", "You'll love this!"]
```

### Template 2: The Stoic Warrior

```yaml
personality:
  traits:
    friendliness: 0.3
    courage: 0.95
    formality: 0.6
    honesty: 0.9
    chattiness: 0.2
    greed: 0.1
  style: Minimal
  emotional_range: Stoic

dialogue_patterns:
  greeting: [".", "Hmm.", "What?"]
  farewell: ["Go.", "Leave.", "Done."]
  combat: ["Fight me.", "Show your skill.", "No mercy."]
```

### Template 3: The Scheming Noble

```yaml
personality:
  traits:
    friendliness: 0.6
    courage: 0.4
    formality: 0.95
    honesty: 0.3
    chattiness: 0.7
    greed: 0.8
  style: Flowery
  emotional_range: Stable

dialogue_patterns:
  greeting: ["Greetings, dear traveler.", "How fortuitous!", "A pleasure!"]
  trading: ["Perhaps we can reach an... arrangement.", "I have what you need."]
  deception: ["Trust me, this is genuine.", "Would I deceive you?"]
```

---

## 📝 Implementation Tasks

### Phase 6.1: Personality System (3-4 hours)

- [ ] **T201**: Create `NPCPersonality` component
- [ ] **T202**: Create `DialogueContext` component
- [ ] **T203**: Implement `PersonalityDialogueSystem`
- [ ] **T204**: Create personality templates (YAML files)
- [ ] **T205**: Implement tone application logic

### Phase 6.2: Contextual Dialogue (3-4 hours)

- [ ] **T206**: Implement `ContextualDialogueSystem`
- [ ] **T207**: Memory integration in dialogue
- [ ] **T208**: Combat history integration in dialogue
- [ ] **T209**: Context merging algorithm
- [ ] **T210**: Response generation pipeline

### Phase 6.3: Dialogue Generation Service (2-3 hours)

- [ ] **T211**: Implement `IDialogueGenerationService` interface
- [ ] **T212**: Create `DialogueGenerationService`
- [ ] **T213**: Implement greeting generation
- [ ] **T214**: Implement response generation
- [ ] **T215**: Implement dialogue options generation

### Phase 6.4: Production Examples (2-3 hours)

- [ ] **T216**: Create `PersonalityShowcase.cs` example
- [ ] **T217**: Create `DynamicMerchantDialogue.cs` example
- [ ] **T218**: Create `RivalDialogueSystem.cs` example

### Phase 6.5: Testing & Documentation (2 hours)

- [ ] **T219**: Write unit tests for PersonalityDialogueSystem
- [ ] **T220**: Write unit tests for ContextualDialogueSystem
- [ ] **T221**: Create USAGE_EXAMPLES.md
- [ ] **T222**: Update ARCHITECTURE.md
- [ ] **T223**: Create PHASE6_SUMMARY.md

**Total Estimated Time**: 12-16 hours

---

## ✅ Success Criteria

### Functional Requirements

- ✅ NPCs have distinct personalities
- ✅ Dialogue reflects personality traits
- ✅ Combat history influences dialogue
- ✅ Memory creates conversation continuity
- ✅ Emotional states affect tone
- ✅ Relationship levels unlock dialogue options

### Code Quality

- ✅ Zero compilation errors
- ✅ XML documentation complete
- ✅ Integration with Phase 4 & 5
- ✅ Production-ready examples
- ✅ Clean architecture

### Dialogue Quality

- ✅ Distinct personality voices
- ✅ Contextually appropriate responses
- ✅ Natural conversation flow
- ✅ Meaningful memory references
- ✅ Believable emotional reactions

---

## 🚀 Getting Started

### Step 1: Review Previous Phases

```bash
# Review memory system
cat PHASE4_REFINEMENT_SUMMARY.md

# Review combat memory
cat PHASE5_IMPLEMENTATION_SUMMARY.md
```

### Step 2: Start Implementation

```bash
# Create personality components
# Start with T201-T205
```

### Step 3: Test & Iterate

```bash
# Build and test
dotnet build
# Test personality variations
```

---

## 📚 References

- **Phase 4**: Memory & Relationships
- **Phase 5**: Combat Memory & Adaptive AI
- **Dialogue System**: `Systems/DialogueSystem.cs`
- **NPC Service**: `Services/NPCService.cs`

---

**Ready to make NPCs truly come alive with personality!** 🎭

**Status**: Ready for implementation
**Next Action**: Start with T201 (NPCPersonality component)
