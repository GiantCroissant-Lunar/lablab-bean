# 📊 Leaderboard UI Preview

## Leaderboard Screen Layout

```
╔════════════════════════════════════════════════════════════════════════════════════╗
║                           🏆 LEADERBOARDS 🏆                                       ║
║                                                                                    ║
║  Total Score  Kills  K/D Ratio  Levels  Speed  Items  Depth  Achievements        ║
║  ‾‾‾‾‾‾‾‾‾‾‾                                                                      ║
║                                                                                    ║
║  Rank  Player             Score      Details                                      ║
║  ────────────────────────────────────────────────────────────────────────────     ║
║  🥇 #1  TopPlayer         125,450    K: 45 D: 2 L: 15                            ║
║  🥈 #2  Runner-Up         118,200    K: 42 D: 3 L: 14                            ║
║  🥉 #3  BronzeMedal       112,800    K: 38 D: 4 L: 13                            ║
║    #4  FourthPlace        105,300    K: 35 D: 5 L: 12                            ║
║    #5  Player5             98,700    K: 32 D: 6 L: 11                            ║
║    #6  YourPlayer          92,100    K: 30 D: 7 L: 10                            ║
║    #7  Player7             87,400    K: 28 D: 8 L: 9                             ║
║    #8  Player8             82,500    K: 25 D: 9 L: 8                             ║
║    #9  Player9             78,300    K: 22 D: 10 L: 7                            ║
║   #10  Player10            74,100    K: 20 D: 11 L: 6                            ║
║                                                                                    ║
║  Player: YourPlayer | Sessions: 5 | Total Kills: 150 | Achievements: 12 (350 pts)║
║  Your Rank in TotalScore: #6                                                      ║
║                                                                                    ║
║                ← → Change Category | L Close | ESC Exit                           ║
╚════════════════════════════════════════════════════════════════════════════════════╝
```

---

## Color Scheme

### Rank Colors

- **🥇 Gold (Rank #1)**: Bright golden yellow
- **🥈 Silver (Rank #2)**: Light silver/gray
- **🥉 Bronze (Rank #3)**: Bronze/copper color
- **🟡 Yellow (Ranks #4-10)**: Medium yellow
- **⚪ White (Ranks #11+)**: Standard white text
- **🔵 Cyan (Your entries)**: Bright cyan highlight

### UI Elements

- **Border**: Yellow (`Color.Yellow`)
- **Background**: Black (`Color.Black`)
- **Headers**: Yellow
- **Player Stats**: Cyan
- **Details**: Gray

---

## Category Views

### 1. Total Score

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Champion          125,450    K: 45 D: 2 L: 15
🥈 #2  Runner             118,200    K: 42 D: 3 L: 14
🥉 #3  Bronze             112,800    K: 38 D: 4 L: 13
```

### 2. Highest Kills

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Slayer                 68    D: 5 K/D: 13.60
🥈 #2  Hunter                 62    D: 4 K/D: 15.50
🥉 #3  Warrior                58    D: 6 K/D: 9.67
```

### 3. Best K/D Ratio

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Untouchable        25.00    K: 50 D: 2
🥈 #2  Elite              18.50    K: 37 D: 2
🥉 #3  Pro                15.00    K: 45 D: 3
```

### 4. Most Levels Completed

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Explorer              25    00:45:30
🥈 #2  Adventurer            22    00:38:15
🥉 #3  Wanderer              20    00:41:20
```

### 5. Fastest Completion

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Speedster          9,880    Avg: 01:20 L: 15
🥈 #2  Racer              9,750    Avg: 02:30 L: 12
🥉 #3  Runner             9,650    Avg: 03:50 L: 10
```

### 6. Most Items Collected

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Hoarder              150    00:45:30
🥈 #2  Collector            142    00:38:15
🥉 #3  Gatherer             135    00:41:20
```

### 7. Deepest Dungeon

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Explorer              15    00:45:30
🥈 #2  Delver                12    00:38:15
🥉 #3  Miner                 10    00:41:20
```

### 8. Achievement Points

```
Rank  Player             Score      Details
────────────────────────────────────────────
🥇 #1  Completionist      1,500    00:45:30
🥈 #2  Achiever           1,200    00:38:15
🥉 #3  Hunter               950    00:41:20
```

---

## Navigation Flow

```
┌─────────────┐
│ Game Screen │
└──────┬──────┘
       │
       │ Press 'L'
       ▼
┌──────────────────┐
│ Leaderboard View │◄─────┐
└──────┬───────────┘      │
       │                  │
       │ Press '←' or '→' │
       ├──────────────────┘
       │
       │ Press 'L' or 'ESC'
       ▼
┌─────────────┐
│ Game Screen │
└─────────────┘
```

---

## Player Stats Footer

```
╔════════════════════════════════════════════════════════════════════════╗
║  Player: YourUsername | Sessions: 25 | Total Kills: 1,250 |            ║
║  Achievements: 18/19 (450 pts) | Playtime: 5h 30m                      ║
║  Your Rank in TotalScore: #6                                           ║
╚════════════════════════════════════════════════════════════════════════╝
```

**Shows**:

- Player name
- Total sessions played
- Total kills across all sessions
- Achievement count and points
- Total playtime
- Current rank in selected category

---

## Special Features

### Medals & Icons

- 🥇 **Gold Medal** - Rank 1
- 🥈 **Silver Medal** - Rank 2
- 🥉 **Bronze Medal** - Rank 3
- 🏆 **Trophy** - Header decoration

### Visual Highlights

- **Your entries** appear in cyan
- **Top 3** have special medal icons
- **Borders** are double-line box drawing characters
- **Category tabs** have selection highlighting

### Responsive Layout

- Adjusts to terminal width
- Truncates long names with "..."
- Smart column alignment
- Clean spacing

---

## Example Session Flow

### 1. Play Session

```
[You play the game]
- Kill 30 enemies
- Complete 5 levels
- Collect 25 items
- Unlock 2 achievements
```

### 2. Session Ends

```
[Auto-save triggered]
✓ Calculating scores...
✓ Submitting to leaderboards...
  - Rank #8 in Total Score
  - Rank #12 in Highest Kills
  - Rank #5 in Best K/D Ratio
✓ Profile updated
✓ Data saved with backup
```

### 3. View Leaderboards

```
[Press 'L']
📊 See your rankings
🥇 See top players
📈 View your stats
🏆 Check achievements
```

---

## Data Display Examples

### Entry Details by Category

**Total Score**:

```
K: 45 D: 2 L: 15
(Kills, Deaths, Levels)
```

**Highest Kills**:

```
D: 5 K/D: 13.60
(Deaths, Kill/Death Ratio)
```

**Best K/D Ratio**:

```
K: 50 D: 2
(Kills, Deaths)
```

**Fastest Completion**:

```
Avg: 01:20 L: 15
(Average time per level, Levels)
```

**Others**:

```
00:45:30
(Playtime)
```

---

## Mobile-Friendly Design

While this is a terminal UI, the design principles apply:

- ✅ Clear information hierarchy
- ✅ Minimal scrolling needed
- ✅ Touch-friendly navigation (keyboard)
- ✅ High contrast colors
- ✅ Readable fonts (terminal standard)

---

## Accessibility Features

- **Color coding** for quick scanning
- **Clear text labels** for all elements
- **Rank numbers** alongside medals
- **Detailed stats** for context
- **Large text** in terminal
- **Keyboard navigation** (no mouse required)

---

## Performance

- **Fast rendering** with cached data
- **No network calls** (local only)
- **Instant category switching**
- **Smooth animations** (if implemented)
- **No lag** on leaderboard open

---

🎮 **Ready to Compete!**

Press `L` in-game to view your rankings and climb the leaderboards!
