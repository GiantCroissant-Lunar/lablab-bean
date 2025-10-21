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
    private readonly InventorySystem _inventorySystem;
    private readonly ItemSpawnSystem _itemSpawnSystem;
    private readonly StatusEffectSystem _statusEffectSystem;
    private LevelManager? _levelManager;
    private DifficultyScalingSystem? _difficultyScaling;

    private DungeonMap? _currentMap;
    private bool _disposed;
    private bool _isInitialized;

    public GameWorldManager WorldManager => _worldManager;
    public DungeonMap? CurrentMap => _currentMap;
    public GameMode CurrentMode => _worldManager.CurrentMode;
    public StatusEffectSystem StatusEffectSystem => _statusEffectSystem;
    public LevelManager? LevelManager => _levelManager;
    public int CurrentDungeonLevel => _levelManager?.CurrentLevel ?? 1;

    public GameStateManager(
        ILogger<GameStateManager> logger,
        GameWorldManager worldManager,
        MovementSystem movementSystem,
        CombatSystem combatSystem,
        AISystem aiSystem,
        ActorSystem actorSystem,
        InventorySystem inventorySystem,
        ItemSpawnSystem itemSpawnSystem,
        StatusEffectSystem statusEffectSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
        _aiSystem = aiSystem ?? throw new ArgumentNullException(nameof(aiSystem));
        _actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
        _inventorySystem = inventorySystem ?? throw new ArgumentNullException(nameof(inventorySystem));
        _itemSpawnSystem = itemSpawnSystem ?? throw new ArgumentNullException(nameof(itemSpawnSystem));
        _statusEffectSystem = statusEffectSystem ?? throw new ArgumentNullException(nameof(statusEffectSystem));
    }

    /// <summary>
    /// Initializes a new game with a generated map
    /// </summary>
    public void InitializeNewGame(int mapWidth = 80, int mapHeight = 50)
    {
        _logger.LogInformation("Initializing new game with map size {Width}x{Height}", mapWidth, mapHeight);

        // Initialize difficulty scaling system
        _difficultyScaling = new DifficultyScalingSystem(_worldManager.GetWorld(GameMode.Play));
        
        // Initialize level manager
        var mapGenerator = new MapGenerator();
        _levelManager = new LevelManager(_worldManager.GetWorld(GameMode.Play), mapGenerator, _difficultyScaling);

        // Generate first level manually (don't use DescendLevel for initialization)
        var map = mapGenerator.GenerateRoomsAndCorridors(80, 50);
        var level = new DungeonLevel(1, map);
        _levelManager.InitializeFirstLevel(level);
        _currentMap = map;

        _logger.LogInformation("Generated dungeon level 1");

        // Initialize play mode world
        InitializePlayWorld();

        // Initialize edit mode world (empty for now)
        _worldManager.ClearWorld(GameMode.Edit);

        _isInitialized = true;
        _logger.LogInformation("Game initialized successfully");
    }

    /// <summary>
    /// Initializes the play world with player and enemies
    /// </summary>
    private void InitializePlayWorld()
    {
        var world = _worldManager.GetWorld(GameMode.Play);
        
        if (_currentMap == null)
            throw new InvalidOperationException("Cannot initialize play world without a map");

        // Find spawn position (center of map or first walkable position)
        var playerSpawn = FindPlayerSpawnPosition(_currentMap);

        // Create the player
        var player = world.Create(
            new Player("Hero"),
            new Position(playerSpawn),
            new Health(50, 100),  // Start at 50/100 HP for testing healing
            new Combat(10, 5),
            new Actor(100),
            new Renderable('@', Color.Yellow, Color.Black, 100),
            new Visible(true),
            new BlocksMovement(true),
            new Name("Player"),
            new Inventory(maxCapacity: 20),
            new EquipmentSlots(),
            StatusEffects.CreateEmpty()
        );

        _logger.LogInformation("Player created at {Position} with 50/100 HP for testing", playerSpawn);

        // Create enemies across the map
        var random = new Random();
        int enemyCount = 10 + (CurrentDungeonLevel * 2); // More enemies on deeper levels
        
        for (int i = 0; i < enemyCount; i++)
        {
            var enemyPos = FindWalkablePosition(_currentMap, random);
            if (enemyPos != playerSpawn) // Don't spawn on player
            {
                CreateEnemy(world, enemyPos, random, CurrentDungeonLevel);
            }
        }

        _logger.LogInformation("Created {EnemyCount} enemies on level {Level}", enemyCount, CurrentDungeonLevel);

        // Spawn items across the map
        SpawnItemsOnLevel(world, random);

        // Calculate initial FOV with larger radius to see more of the dungeon
        _currentMap.CalculateFOV(playerSpawn, 20);

        _logger.LogInformation("Play world initialized with player, enemies, and items");
    }

    /// <summary>
    /// Find a suitable spawn position for the player
    /// </summary>
    private Point FindPlayerSpawnPosition(DungeonMap map)
    {
        // Try center first
        var center = new Point(map.Width / 2, map.Height / 2);
        if (map.IsWalkable(center))
            return center;

        // Find first walkable position
        for (int y = 1; y < map.Height - 1; y++)
        {
            for (int x = 1; x < map.Width - 1; x++)
            {
                var pos = new Point(x, y);
                if (map.IsWalkable(pos))
                    return pos;
            }
        }

        return new Point(1, 1); // Fallback
    }

    /// <summary>
    /// Spawn items on the current level
    /// </summary>
    private void SpawnItemsOnLevel(World world, Random random)
    {
        if (_currentMap == null)
            return;

        int itemCount = 5 + (CurrentDungeonLevel * 2); // More items on deeper levels
        
        for (int i = 0; i < itemCount; i++)
        {
            var itemPos = FindWalkablePosition(_currentMap, random);
            
            // Use difficulty scaling to determine if item should spawn
            if (_difficultyScaling != null && _difficultyScaling.ShouldDropLoot(CurrentDungeonLevel))
            {
                // For now, spawn healing potions
                // TODO: Extend with weighted spawn tables based on level
                _itemSpawnSystem.SpawnHealingPotion(world, itemPos);
            }
        }
    }

    /// <summary>
    /// Creates a single enemy entity with level scaling
    /// </summary>
    private void CreateEnemy(World world, Point position, Random random, int dungeonLevel = 1)
    {
        // Random enemy type with special enemies
        var roll = random.Next(100);
        string enemyType;
        Enemy enemyComponent;
        int baseHealth, baseAttack, baseDefense, baseSpeed;
        
        if (roll < 20) // 20% chance for Toxic Spider
        {
            enemyType = "Toxic Spider";
            baseHealth = 15;
            baseAttack = 3;
            baseDefense = 1;
            baseSpeed = 90;
            
            // Create enemy with poison attack (40% chance to poison for 5 turns)
            enemyComponent = new Enemy(enemyType)
            {
                InflictsEffect = EffectType.Poison,
                EffectProbability = 40,  // 40% chance
                EffectMagnitude = 3,     // 3 damage per turn
                EffectDuration = 5       // 5 turns
            };
        }
        else
        {
            // Normal enemies
            var normalEnemyTypes = new[] { "Goblin", "Orc", "Troll", "Skeleton" };
            enemyType = normalEnemyTypes[random.Next(normalEnemyTypes.Length)];
            baseHealth = 30;
            baseAttack = 5;
            baseDefense = 2;
            baseSpeed = 80 + random.Next(40);
            enemyComponent = new Enemy(enemyType);
        }

        // Apply difficulty scaling
        int scaledHealth = baseHealth;
        int scaledAttack = baseAttack;
        int scaledDefense = baseDefense;
        int scaledSpeed = baseSpeed;

        if (_difficultyScaling != null && dungeonLevel > 1)
        {
            scaledHealth = _difficultyScaling.CalculateScaledStat(baseHealth, dungeonLevel);
            scaledAttack = _difficultyScaling.CalculateScaledStat(baseAttack, dungeonLevel);
            scaledDefense = _difficultyScaling.CalculateScaledStat(baseDefense, dungeonLevel);
            scaledSpeed = _difficultyScaling.CalculateScaledStat(baseSpeed, dungeonLevel);
        }

        var enemy = world.Create(
            enemyComponent,
            new Position(position),
            new Health(scaledHealth, scaledHealth),
            new Combat(scaledAttack, scaledDefense),
            new Actor(scaledSpeed),
            new AI(AIBehavior.Chase),
            new Renderable(GetEnemyGlyph(enemyType), GetEnemyColor(enemyType), Color.Black, 50),
            new Visible(true),
            new BlocksMovement(true),
            new Name(enemyType),
            StatusEffects.CreateEmpty()
        );

        _logger.LogDebug("Created {EnemyType} at {Position} (Level {Level}): {Health} HP, {Attack} ATK", 
            enemyType, position, dungeonLevel, scaledHealth, scaledAttack);
    }

    /// <summary>
    /// Finds a random walkable position on the map
    /// </summary>
    private Point FindWalkablePosition(DungeonMap map, Random? random = null)
    {
        random ??= new Random();
        Point position;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            position = new Point(
                random.Next(1, map.Width - 1),
                random.Next(1, map.Height - 1)
            );
            attempts++;
        } while (!map.IsWalkable(position) && attempts < maxAttempts);

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
        "Toxic Spider" => 'x',
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
        "Toxic Spider" => Color.Purple,
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
    /// Handles player attempting to pick up items
    /// Returns a list of messages describing what happened
    /// </summary>
    public List<string> HandlePlayerPickup()
    {
        var messages = new List<string>();

        if (!_isInitialized || _currentMap == null)
            return messages;

        var world = _worldManager.GetWorld(GameMode.Play);
        var query = new QueryDescription().WithAll<Player, Position, Actor, Inventory>();

        world.Query(in query, (Entity playerEntity, ref Player player, ref Position pos, ref Actor actor, ref Inventory inventory) =>
        {
            if (!actor.CanAct)
            {
                messages.Add("You can't act yet!");
                return;
            }

            // Get all pickupable items
            var items = _inventorySystem.GetPickupableItems(world, playerEntity);

            if (items.Count == 0)
            {
                messages.Add("There's nothing here to pick up.");
                return;
            }

            // If only one item, pick it up directly
            if (items.Count == 1)
            {
                var message = _inventorySystem.PickupItem(world, playerEntity, items[0]);
                messages.Add(message);
                actor.ConsumeEnergy();
                return;
            }

            // Multiple items - add message for each
            messages.Add($"There are {items.Count} items here:");
            for (int i = 0; i < items.Count; i++)
            {
                var item = world.Get<Item>(items[i]);
                var count = world.Has<Stackable>(items[i]) ? world.Get<Stackable>(items[i]).Count : 1;
                var countStr = count > 1 ? $" x{count}" : "";
                messages.Add($"  {i + 1}. {item.Name}{countStr}");
            }
            messages.Add("(Multiple item pickup to be implemented)");
        });

        return messages;
    }

    /// <summary>
    /// Handles player using/consuming an item from inventory
    /// Returns a message describing the result
    /// </summary>
    public string HandlePlayerUseItem()
    {
        var world = _worldManager.CurrentWorld;
        var query = new QueryDescription().WithAll<Player, Actor>();
        string result = "";

        world.Query(in query, (Entity playerEntity, ref Player player, ref Actor actor) =>
        {
            if (!actor.CanAct)
            {
                result = "Cannot act yet!";
                return;
            }

            // Priority 1: Try consumables first
            var consumables = _inventorySystem.GetConsumables(world, playerEntity);

            if (consumables.Count > 0)
            {
                // Use first consumable
                var firstConsumable = consumables[0];
                var message = _inventorySystem.UseConsumable(world, playerEntity, firstConsumable.ItemEntity);
                
                if (!message.StartsWith("Cannot") && !message.StartsWith("Already"))
                {
                    // Item was successfully used - consume energy
                    actor.ConsumeEnergy();
                }

                result = message;
                return;
            }

            // Priority 2: Try equipment if no consumables
            var equippables = _inventorySystem.GetEquippables(world, playerEntity);

            if (equippables.Count > 0)
            {
                // Equip first equippable item
                var firstEquippable = equippables[0];
                var (success, message, atkChange, defChange, spdChange) = _inventorySystem.EquipItem(world, playerEntity, firstEquippable.ItemEntity);
                
                if (success)
                {
                    // Item was successfully equipped - consume energy
                    actor.ConsumeEnergy();
                }

                result = message;
                return;
            }

            // No usable items
            result = "No usable items in inventory!";
        });

        return result;
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
    /// Checks if it's the player's turn (player can act)
    /// </summary>
    public bool IsPlayerTurn()
    {
        if (!_isInitialized || CurrentMode != GameMode.Play)
            return false;

        var world = _worldManager.GetWorld(GameMode.Play);
        return _actorSystem.IsPlayerTurn(world);
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

    /// <summary>
    /// Handles player interaction with staircases
    /// </summary>
    public bool HandleStaircaseInteraction(StaircaseDirection direction)
    {
        if (!_isInitialized || _currentMap == null || _levelManager == null)
        {
            _logger.LogWarning("Cannot handle staircase interaction - game not initialized or no level manager");
            return false;
        }

        var world = _worldManager.GetWorld(GameMode.Play);
        var query = new QueryDescription().WithAll<Player, Position, Actor>();
        bool actionTaken = false;

        world.Query(in query, (Entity playerEntity, ref Player player, ref Position pos, ref Actor actor) =>
        {
            if (!actor.CanAct)
            {
                _logger.LogDebug("Player cannot act yet");
                return;
            }

            // Check if player is on a staircase
            if (!_levelManager.CanTransition(playerEntity, direction))
            {
                var oppositeDir = direction == StaircaseDirection.Down ? "upward" : "downward";
                _logger.LogInformation("Player not on {Direction} staircase (may be on {Opposite} staircase or no staircase)", 
                    direction, oppositeDir);
                return;
            }

            // Perform transition
            LevelTransitionResult result;
            if (direction == StaircaseDirection.Down)
            {
                result = _levelManager.DescendLevel(playerEntity);
            }
            else
            {
                result = _levelManager.AscendLevel(playerEntity);
            }

            if (result.Success)
            {
                _logger.LogInformation("Level transition successful: {Message}", result.Message);
                
                // Update current map reference
                _currentMap = _levelManager.GetCurrentMap();
                
                // Clear and reinitialize the world for the new level
                world.Clear();
                InitializePlayWorld();
                
                // Consume energy for the action
                actor.ConsumeEnergy();
                actionTaken = true;

                if (result.NewRecordDepth)
                {
                    _logger.LogInformation("New record depth achieved: Level {Level}", result.NewLevel);
                }

                if (result.VictoryTriggered)
                {
                    _logger.LogInformation("Victory condition triggered!");
                }
            }
            else
            {
                _logger.LogWarning("Level transition failed: {Message}", result.Message);
            }
        });

        return actionTaken;
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
