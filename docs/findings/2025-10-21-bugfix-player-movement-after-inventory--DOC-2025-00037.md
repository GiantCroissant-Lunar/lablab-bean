---
doc_id: DOC-2025-00037
title: Bug Fix - Player Cannot Move After Adding Item to Inventory
doc_type: finding
status: draft
canonical: false
created: 2025-10-21
tags: [bugfix, turn-system, energy, inventory, gameplay]
summary: >
  Fixed issue where player could not move after picking up items due to energy system not processing NPC turns automatically.
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Bug Fix: Player Cannot Move After Adding Item to Inventory

## Problem

After picking up an item with the 'g' key, the player could not move. The key was received but no action was taken, leaving the player unable to continue playing.

## Root Cause

The game uses a turn-based energy system where:

1. When a player picks up an item, the `HandlePlayerPickup()` method consumes the player's energy (100 points)
2. After consuming energy, the player's `CanAct` property becomes false (energy < 100)
3. The game should process NPC turns until the player accumulates enough energy again
4. However, the game was only calling `Update()` once and then waiting for the next keypress
5. Since the player didn't have enough energy, they couldn't move on the next keypress

## Solution

Implemented a turn processing loop that continues to execute NPC turns after the player takes an action until it's the player's turn again.

### Changes Made

#### 1. Added `ProcessNPCTurns()` method in `DungeonCrawlerService.cs`

This method:

- Continuously processes game updates while the player cannot act
- Renders each NPC action for visibility
- Includes a safety limit to prevent infinite loops
- Adds debug logging to track NPC turn processing

#### 2. Added `IsPlayerTurn()` method in `GameStateManager.cs`

This method checks if the player has accumulated enough energy to act.

#### 3. Updated action handling in `OnKeyDown()` method

- After any successful action (movement, pickup, item use), the game now calls `ProcessNPCTurns()`
- Refactored pickup and item use handlers to use the common `actionTaken` flow
- This ensures NPCs get their turns processed automatically after player actions

### Technical Details

**Energy System:**

- Each actor has an Energy and Speed property
- When Energy >= 100, the actor can take an action
- Taking an action consumes 100 energy
- Each game tick, actors accumulate energy based on their Speed

**Turn Flow (After Fix):**

1. Player presses a key (e.g., 'g' to pick up item)
2. Player action consumes 100 energy
3. `Update()` is called once
4. `ProcessNPCTurns()` is called
5. Game continues processing turns and accumulating energy
6. NPCs with enough energy take their actions
7. Loop continues until player has enough energy (Energy >= 100)
8. Control returns to player

### Files Modified

1. `dotnet/console-app/LablabBean.Console/Services/DungeonCrawlerService.cs`
   - Added `ProcessNPCTurns()` method
   - Updated pickup item handler (Key.G)
   - Updated use item handler (Key.U)
   - Updated movement action completion logic

2. `dotnet/framework/LablabBean.Game.Core/Services/GameStateManager.cs`
   - Added `IsPlayerTurn()` public method

## Testing

Build the project with:

```bash
cd dotnet
dotnet build console-app/LablabBean.Console/LablabBean.Console.csproj
```

Run the game with:

```bash
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj
```

Test scenario:

1. Start the game
2. Move around with arrow keys or WASD
3. Find an item (shown as '%' or similar)
4. Press 'g' to pick up the item
5. Verify you can still move after picking up the item
6. Press 'u' to use an item
7. Verify you can still move after using an item

## Notes

- The 50ms delay in `ProcessNPCTurns()` makes NPC actions visible to the player
- The delay can be reduced or removed if instant processing is preferred
- Debug logging shows turn processing in the debug panel at the bottom of the screen
