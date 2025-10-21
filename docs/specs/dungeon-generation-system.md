---
doc_id: DOC-2025-00034
title: Dungeon Generation System Specification
doc_type: spec
status: active
canonical: true
created: 2025-10-20
tags: [dungeon-generation, procedural, fov, fog-of-war, rooms]
summary: >
  Procedurally generated dungeons with rooms connected by L-shaped corridors,
  fog of war mechanics, and field of view calculations.
---

# Dungeon Generation System Specification

**Version**: 0.0.2
**Status**: Implemented
**Author**: Development Team
**Date**: 2025-10-20

## Overview

The dungeon generation system creates procedurally generated dungeons with rooms connected by L-shaped corridors. It supports fog of war mechanics and field of view (FOV) calculations for visibility management.

## Requirements

### Functional Requirements

- **REQ-001**: Generate dungeons with 5-15 rooms
- **REQ-002**: Each room must be 6-12 tiles in size
- **REQ-003**: Rooms connected by L-shaped corridors
- **REQ-004**: Support fog of war (explored vs unexplored)
- **REQ-005**: Calculate FOV with configurable radius (currently 20 tiles)
- **REQ-006**: Track three visibility states: visible, explored, unexplored

### Non-Functional Requirements

- **NFREQ-001**: Generation must complete in <100ms
- **NFREQ-002**: Support maps up to 100x100 tiles
- **NFREQ-003**: Deterministic generation with seed support

## Architecture

### Components

1. **RoomDungeonGenerator** (`LablabBean.Game.Core/Maps/RoomDungeonGenerator.cs`)
   - Generates dungeon layout with rooms and corridors
   - Places rooms randomly ensuring no overlaps
   - Creates L-shaped corridors between room centers

2. **FogOfWar** (`LablabBean.Game.Core/Maps/FogOfWar.cs`)
   - Tracks explored tiles
   - Maintains boolean grid of visited locations
   - Provides query methods for exploration state

3. **DungeonMap** (`LablabBean.Game.Core/Maps/DungeonMap.cs`)
   - Integrates FOV calculation
   - Manages tile visibility states
   - Coordinates between generation and rendering

4. **GameStateManager** (`LablabBean.Game.Core/Services/GameStateManager.cs`)
   - Initializes dungeon with room generator
   - Sets FOV radius (20 tiles)
   - Spawns monsters per room (1-3 per room)

### Data Flow

```
RoomDungeonGenerator
    ↓ (generates)
DungeonMap
    ↓ (calculates)
FOV System
    ↓ (updates)
FogOfWar
    ↓ (renders)
MapView
```

## Implementation Details

### Room Generation Algorithm

1. **Initialize**: Create empty map
2. **Place Rooms**: 
   - Generate 5-15 random room positions
   - Size: random 6-12 tiles width/height
   - Ensure no overlaps
3. **Connect Rooms**:
   - For each consecutive room pair
   - Calculate center points
   - Create L-shaped corridor:
     - Horizontal segment from room1.center to room2.x
     - Vertical segment from room2.x to room2.center
4. **Carve Tiles**: Set floor tiles for rooms and corridors

### FOV Calculation

- **Algorithm**: Recursive shadowcasting
- **Radius**: 20 tiles (configurable)
- **Origin**: Player position
- **Updates**: On player movement

### Visibility States

| State | Description | Rendering |
|-------|-------------|-----------|
| Visible | In current FOV | Bright: `.` (floor), `#` (wall) |
| Explored | Previously visible | Dim: `·` (floor), `▓` (wall) |
| Unexplored | Never seen | Blank/empty |

## Configuration

```csharp
// In GameStateManager.cs
const int MinRooms = 5;
const int MaxRooms = 15;
const int MinRoomSize = 6;
const int MaxRoomSize = 12;
const int FOVRadius = 20;  // Visibility range
```

## Files Modified/Created

### Created Files
- `dotnet/framework/LablabBean.Game.Core/Maps/RoomDungeonGenerator.cs`
- `dotnet/framework/LablabBean.Game.Core/Maps/FogOfWar.cs`
- `dotnet/framework/LablabBean.Game.TerminalUI/Views/MapView.cs`

### Modified Files
- `dotnet/framework/LablabBean.Game.Core/Maps/DungeonMap.cs`
- `dotnet/framework/LablabBean.Game.Core/Services/GameStateManager.cs`
- `dotnet/framework/LablabBean.Game.TerminalUI/Services/WorldViewService.cs`

## Testing

### Manual Test Cases

- **TC-001**: Generate dungeon with default settings
  - Expected: 5-15 rooms with corridors
  - Status: ✅ Pass

- **TC-002**: Verify room sizes
  - Expected: All rooms 6-12 tiles
  - Status: ✅ Pass

- **TC-003**: Check corridor connectivity
  - Expected: All rooms reachable
  - Status: ✅ Pass

- **TC-004**: Test FOV calculation
  - Expected: Visible tiles within 20-tile radius
  - Status: ✅ Pass

- **TC-005**: Verify fog of war
  - Expected: Three distinct visibility states
  - Status: ✅ Pass

### Acceptance Criteria

- [x] Dungeon generates successfully
- [x] Rooms are properly sized (6-12 tiles)
- [x] Corridors connect all rooms
- [x] FOV updates on player movement
- [x] Fog of war tracks exploration
- [x] Rendering shows three visibility states

## Performance

- **Generation Time**: ~10-20ms for typical dungeon
- **FOV Calculation**: ~2-5ms per update
- **Memory Usage**: ~50KB per dungeon

## Known Issues

- **Issue #1**: No validation for minimum room spacing
  - **Impact**: Rooms can be very close but not overlapping
  - **Priority**: Low
  - **Workaround**: Current behavior acceptable

- **Issue #2**: Corridor algorithm creates only L-shaped paths
  - **Impact**: Limited variety in corridor shapes
  - **Priority**: Low
  - **Future**: Could add S-shaped or straight corridors

## Future Enhancements

1. **Room Types**: Special room types (treasure, boss, shrine)
2. **Corridor Variety**: Multiple corridor shapes
3. **Biomes**: Different tile sets per region
4. **Doors**: Add door tiles between rooms and corridors
5. **Secret Rooms**: Hidden rooms revealed by FOV
6. **Dynamic Lighting**: Light sources affect visibility
7. **Destructible Walls**: Allow terrain modification

## Integration Points

### With Entity System
- Spawns monsters in rooms (1-3 per room)
- Player spawns in first generated room
- Entities visible only within FOV

### With Rendering System
- MapView renders dungeon from DungeonMap
- Camera centers on player position
- Buffer-based rendering for efficiency

### With Game State
- GameStateManager owns DungeonMap instance
- Updates FOV on player movement
- Tracks fog of war state

## Debug Information

### Logging
```csharp
_logger.LogInformation("Generated dungeon with {RoomCount} rooms", rooms.Count);
_logger.LogDebug("FOV calculated for position ({X}, {Y}), radius {Radius}", x, y, radius);
```

### Debug Panel
- Shows current FOV radius
- Displays room count
- Reports generation time

## References

- **Roguelike Development**: http://www.roguebasin.com/
- **Shadowcasting FOV**: http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting
- **Dungeon Generation**: http://www.roguebasin.com/index.php?title=Dungeon-Building_Algorithm

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.0.1 | 2025-10-15 | Initial implementation |
| 0.0.2 | 2025-10-20 | Increased FOV radius 8→20, improved fog rendering |

---

**Implementation Status**: ✅ Complete
**Next Review**: When adding new room types or corridor algorithms
