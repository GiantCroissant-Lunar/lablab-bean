using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;
using LablabBean.Game.Core.Systems;
using LablabBean.Game.Core.Worlds;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

namespace LablabBean.Game.Core.Services;

/// <summary>
/// Main game state manager that coordinates all game systems
/// Handles play mode and edit mode switching
/// </summary>
public class GameStateManager : IDisposable
{
    private readonly ILogger<GameStateManager> _logger;
    private readonly GameWorldManager _worldManager;
    private readonly MovementSystem _movementSystem;
    private readonly CombatSystem _combatSystem;
    private readonly AISystem _aiSystem;
    private readonly ActorSystem _actorSystem;

    private DungeonMap? _currentMap;
    private bool _disposed;
    private bool _isInitialized;

    public GameWorldManager WorldManager => _worldManager;
    public DungeonMap? CurrentMap => _currentMap;
    public GameMode CurrentMode => _worldManager.CurrentMode;

    public GameStateManager(
        ILogger<GameStateManager> logger,
        GameWorldManager worldManager,
        MovementSystem movementSystem,
        CombatSystem combatSystem,
        AISystem aiSystem,
        ActorSystem actorSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
        _aiSystem = aiSystem ?? throw new ArgumentNullException(nameof(aiSystem));
        _actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
    }

    /// <summary>
    /// Initializes a new game with a generated map
    /// </summary>
    public void InitializeNewGame(int mapWidth = 80, int mapHeight = 50)
    {
        _logger.LogInformation("Initializing new game with map size {Width}x{Height}", mapWidth, mapHeight);

        // Generate a new map with rooms
        var generator = new RoomDungeonGenerator();
        var (map, rooms) = generator.Generate(mapWidth, mapHeight, maxRooms: 15, minRoomSize: 6, maxRoomSize: 12);
        _currentMap = map;

        _logger.LogInformation("Generated dungeon with {RoomCount} rooms", rooms.Count);

        // Initialize play mode world with room info
        InitializePlayWorld(rooms);

        // Initialize edit mode world (empty for now)
        _worldManager.ClearWorld(GameMode.Edit);

        _isInitialized = true;
        _logger.LogInformation("Game initialized successfully");
    }

    /// <summary>
    /// Initializes the play world with player and enemies
    /// </summary>
    private void InitializePlayWorld(List<RoomDungeonGenerator.Room> rooms)
    {
        var world = _worldManager.GetWorld(GameMode.Play);
        world.Clear();

        if (_currentMap == null || rooms.Count == 0)
            throw new InvalidOperationException("Cannot initialize play world without a map and rooms");

        // Spawn player in the first room
        var playerSpawn = rooms[0].Center;

        // Create the player
        var player = world.Create(
            new Player("Hero"),
            new Position(playerSpawn),
            new Health(100, 100),
            new Combat(10, 5),
            new Actor(100),
            new Renderable('@', Color.Yellow, Color.Black, 100),
            new Visible(true),
            new BlocksMovement(true),
            new Name("Player"),
            new Inventory(maxCapacity: 20),
            new EquipmentSlots()
        );

        _logger.LogInformation("Player created at {Position}", playerSpawn);

        // Create enemies in other rooms (1-3 per room)
        var random = new Random();
        int totalEnemies = 0;
        
        for (int i = 1; i < rooms.Count; i++)
        {
            int enemiesInRoom = random.Next(1, 4);
            for (int j = 0; j < enemiesInRoom; j++)
            {
                var enemyPos = GetRandomPositionInRoom(rooms[i], random);
                CreateEnemy(world, enemyPos, random);
                totalEnemies++;
            }
        }

        _logger.LogInformation("Created {EnemyCount} enemies across {RoomCount} rooms", totalEnemies, rooms.Count - 1);

        // Calculate initial FOV with larger radius to see more of the dungeon
        _currentMap.CalculateFOV(playerSpawn, 20);

        _logger.LogInformation("Play world initialized with player and enemies");
    }

    /// <summary>
    /// Gets a random walkable position within a room
    /// </summary>
    private Point GetRandomPositionInRoom(RoomDungeonGenerator.Room room, Random random)
    {
        int x = random.Next(room.Bounds.X + 1, room.Bounds.X + room.Bounds.Width - 1);
        int y = random.Next(room.Bounds.Y + 1, room.Bounds.Y + room.Bounds.Height - 1);
        return new Point(x, y);
    }

    /// <summary>
    /// Creates a single enemy entity
    /// </summary>
    private void CreateEnemy(World world, Point position, Random random)
    {
        // Random enemy type
        var enemyTypes = new[] { "Goblin", "Orc", "Troll", "Skeleton" };
        var enemyType = enemyTypes[random.Next(enemyTypes.Length)];

        var enemy = world.Create(
            new Enemy(enemyType),
            new Position(position),
            new Health(30, 30),
            new Combat(5, 2),
            new Actor(80 + random.Next(40)),
            new AI(AIBehavior.Chase),
            new Renderable(GetEnemyGlyph(enemyType), GetEnemyColor(enemyType), Color.Black, 50),
            new Visible(true),
            new BlocksMovement(true),
            new Name(enemyType)
        );

        _logger.LogDebug("Created {EnemyType} at {Position}", enemyType, position);
    }

    /// <summary>
    /// Finds a random walkable position on the map
    /// </summary>
    private Point FindWalkablePosition(DungeonMap map)
    {
        var random = new Random();
        Point position;

        do
        {
            position = new Point(
                random.Next(1, map.Width - 1),
                random.Next(1, map.Height - 1)
            );
        } while (!map.IsWalkable(position));

        return position;
    }

    /// <summary>
    /// Gets the glyph for an enemy type
    /// </summary>
    private char GetEnemyGlyph(string enemyType) => enemyType switch
    {
        "Goblin" => 'g',
        "Orc" => 'o',
        "Troll" => 'T',
        "Skeleton" => 's',
        _ => 'e'
    };

    /// <summary>
    /// Gets the color for an enemy type
    /// </summary>
    private Color GetEnemyColor(string enemyType) => enemyType switch
    {
        "Goblin" => Color.Green,
        "Orc" => Color.Red,
        "Troll" => Color.Brown,
        "Skeleton" => Color.White,
        _ => Color.Gray
    };

    /// <summary>
    /// Switches between play and edit modes
    /// </summary>
    public void SwitchMode(GameMode newMode)
    {
        _worldManager.SwitchMode(newMode);
        _logger.LogInformation("Switched to {Mode} mode", newMode);
    }

    /// <summary>
    /// Processes one game update tick
    /// </summary>
    public void Update()
    {
        if (!_isInitialized || _currentMap == null)
            return;

        var world = _worldManager.CurrentWorld;

        // Only process game logic in play mode
        if (CurrentMode == GameMode.Play)
        {
            // Process actor energy accumulation
            _actorSystem.ProcessTick(world);

            // Process AI for entities that can act
            if (!_actorSystem.IsPlayerTurn(world))
            {
                _aiSystem.ProcessAI(world, _currentMap);
            }

            // Update FOV based on player position
            UpdatePlayerFOV(world);
        }
    }

    /// <summary>
    /// Handles player input for movement
    /// </summary>
    public bool HandlePlayerMove(int dx, int dy)
    {
        if (!_isInitialized || _currentMap == null)
            return false;

        var world = _worldManager.GetWorld(GameMode.Play);
        var query = new QueryDescription().WithAll<Player, Position, Actor>();

        bool moved = false;

        world.Query(in query, (Entity entity, ref Player player, ref Position pos, ref Actor actor) =>
        {
            if (!actor.CanAct)
                return;

            var newPosition = new Position(pos.Point + new Point(dx, dy));

            // Check if there's an enemy to attack
            var targetEntity = GetEntityAtPosition(world, newPosition);
            if (targetEntity.HasValue)
            {
                var target = targetEntity.Value;
                if (target.Has<Enemy>())
                {
                    _combatSystem.Attack(world, entity, target);
                    actor.ConsumeEnergy();
                    moved = true;
                    return;
                }
            }

            // Try to move
            if (_movementSystem.MoveEntity(world, entity, newPosition, _currentMap))
            {
                actor.ConsumeEnergy();
                moved = true;
            }
        });

        return moved;
    }

    /// <summary>
    /// Gets entity at a specific position
    /// </summary>
    private Entity? GetEntityAtPosition(World world, Position position)
    {
        var query = new QueryDescription().WithAll<Position>();
        Entity? result = null;

        world.Query(in query, (Entity entity, ref Position pos) =>
        {
            if (pos.Point == position.Point)
            {
                result = entity;
            }
        });

        return result;
    }

    /// <summary>
    /// Updates player FOV
    /// </summary>
    private void UpdatePlayerFOV(World world)
    {
        if (_currentMap == null)
            return;

        var query = new QueryDescription().WithAll<Player, Position>();

        world.Query(in query, (Entity entity, ref Player player, ref Position pos) =>
        {
            _currentMap.CalculateFOV(pos.Point, 20);
        });
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _worldManager?.Dispose();
        _disposed = true;

        _logger.LogInformation("GameStateManager disposed");
    }
}
