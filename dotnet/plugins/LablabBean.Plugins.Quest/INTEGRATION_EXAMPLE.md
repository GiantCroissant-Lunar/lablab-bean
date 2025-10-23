# Quest & Progression Integration Example

This example demonstrates how the Quest and Progression systems work together.

## Flow

### 1. Quest Completion Awards XP

When a quest is completed, the `QuestRewardSystem` automatically awards XP to the player:

```csharp
// In QuestRewardSystem.CompleteQuest()
if (rewards.ExperiencePoints > 0 && _progressionService != null)
{
    bool leveledUp = _progressionService.AwardExperience(playerId, rewards.ExperiencePoints);

    if (leveledUp)
    {
        // Player gained one or more levels!
        // UI can show level-up notification
    }
}
```

### 2. Plugin Integration

The `QuestPlugin` automatically injects the `ProgressionService` during initialization:

```csharp
// In QuestPlugin.InitializeAsync()
var progressionService = context.Registry.Get<ProgressionService>();
if (progressionService != null)
{
    _questRewardSystem.SetProgressionService(progressionService);
}
```

### 3. Sample Quests with XP Rewards

#### Training Grounds (Level 1)

- **XP Reward**: 100 XP
- **Objectives**: Defeat 5 training dummies
- **Progression Impact**: ~29% progress to level 2 (348 XP required)

#### Bandit Trouble (Level 3)

- **XP Reward**: 350 XP
- **Objectives**: Eliminate 10 bandits, defeat bandit leader
- **Progression Impact**: Can gain full level at early levels

#### The Lost Artifact (Level 5)

- **XP Reward**: 500 XP
- **Objectives**: Reach dungeon level 5, collect artifact, return to Eldrin
- **Progression Impact**: ~28% progress to level 6 (1,814 XP required)

## XP Progression Example

Starting at **Level 1**:

| Quest                | XP Gained | Total XP | Level | XP to Next |
|---------------------|-----------|----------|-------|------------|
| -                   | 0         | 0        | 1     | 348        |
| Training Grounds    | +100      | 100      | 1     | 248        |
| (2 more completions)| +200      | 300      | 1     | 48         |
| Training Grounds    | +100      | 400      | 2     | 320 (720-52)|
| Bandit Trouble      | +350      | 750      | 3     | 338        |
| The Lost Artifact   | +500      | 1,250    | 3     | 68 to L4   |

## Cascading Level-Ups

If a player earns enough XP from a single quest, they can level up multiple times:

```csharp
// Example: Level 1 player completes a high-level quest worth 1,000 XP
// Level 1 -> 2 requires 348 XP
// Level 2 -> 3 requires 720 XP
// Total: 1,068 XP

AwardExperience(playerId, 1000);
// Result: Player is now level 2 (not quite level 3)
// 1,000 XP awarded
// - 348 XP for level 2 (632 XP remaining)
// - 632 XP towards level 3 (88 XP short)
```

## Level-Gated Quest Access

The dialogue system can check level requirements:

```json
{
  "id": "expert-quest-choice",
  "text": "I have a dangerous mission for experienced adventurers.",
  "condition": "level >= 10",
  "nextNodeId": "accept-expert-quest"
}
```

## Statistics Tracking

The `ProgressionService` tracks lifetime XP:

```csharp
int totalXP = progressionService.GetTotalXPGained(playerId);
// Shows total XP earned across all level-ups
```

## Integration Benefits

1. **Automatic XP Awards**: Quest completion automatically grants XP
2. **Cascading Level-Ups**: Players can gain multiple levels from one quest
3. **Stat Progression**: Health, Attack, Defense, Speed increase on level-up
4. **Health Restore**: Players heal to full on level-up (optional)
5. **Level Gates**: Quests and dialogue can require minimum levels
6. **Progression Tracking**: Lifetime XP statistics available

## Future Enhancements

- **Event Bus Integration**: Publish `OnPlayerLevelUp` events
- **UI Notifications**: Show level-up popup with stat increases
- **XP Bar**: Display XP progress in HUD
- **Quest XP Scaling**: Scale XP rewards based on player level
- **Bonus XP**: Award bonus XP for completing quests above/below level
