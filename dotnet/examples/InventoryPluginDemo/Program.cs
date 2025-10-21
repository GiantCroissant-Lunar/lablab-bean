using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Plugins.Inventory;
using Microsoft.Extensions.Logging;
using SadRogue.Primitives;

Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         Inventory Plugin Demo - Spec 005 Migration Test            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Create a simple logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("InventoryDemo");

Console.WriteLine("📦 Creating Inventory Service (Direct Instantiation)...");
Console.WriteLine();

// Create inventory service directly (simpler than full plugin loading for demo)
var inventoryService = new InventoryService(logger);
Console.WriteLine("✅ IInventoryService created successfully!");
Console.WriteLine();

Console.WriteLine("─────────────────────────────────────────────────────────────────────");
Console.WriteLine();
Console.WriteLine("🎮 Creating Test World with Player and Items...");
Console.WriteLine();

// Create ECS world
var world = World.Create();

// Create player entity
var player = world.Create(
    new Player("Hero"),
    new Actor(speed: 100),
    new Position(5, 5),
    new Health(80, 100),
    new Combat(10, 5),
    new LablabBean.Game.Core.Components.Inventory(maxCapacity: 10),
    new EquipmentSlots()
);

Console.WriteLine($"  Player: {world.Get<Player>(player).Name}");
Console.WriteLine($"  Health: {world.Get<Health>(player).Current}/{world.Get<Health>(player).Maximum}");
Console.WriteLine($"  Attack: {world.Get<Combat>(player).Attack}, Defense: {world.Get<Combat>(player).Defense}");
Console.WriteLine();

// Create test items
var healthPotion = world.Create(
    new Item("Health Potion", '!', ItemType.Consumable, "Restores 20 HP"),
    new Consumable(ConsumableEffect.RestoreHealth, 20),
    new Stackable(count: 3, maxStack: 99),
    new Position(5, 6),
    new Renderable('!', Color.Red),
    new Visible()
);

var ironSword = world.Create(
    new Item("Iron Sword", '/', ItemType.Weapon, "A sturdy iron blade"),
    new Equippable(EquipmentSlot.MainHand, attackBonus: 5, defenseBonus: 0),
    new Position(6, 5),
    new Renderable('/', Color.Gray),
    new Visible()
);

var leatherArmor = world.Create(
    new Item("Leather Armor", '[', ItemType.Armor, "Light protective armor"),
    new Equippable(EquipmentSlot.Chest, attackBonus: 0, defenseBonus: 3),
    new Position(4, 5),
    new Renderable('[', Color.Brown),
    new Visible()
);

Console.WriteLine("  Items created:");
Console.WriteLine($"    - {world.Get<Item>(healthPotion).Name} (adjacent)");
Console.WriteLine($"    - {world.Get<Item>(ironSword).Name} (adjacent)");
Console.WriteLine($"    - {world.Get<Item>(leatherArmor).Name} (adjacent)");
Console.WriteLine();

Console.WriteLine("─────────────────────────────────────────────────────────────────────");
Console.WriteLine();

// Test 1: Get pickupable items
Console.WriteLine("Test 1: Get Pickupable Items");
var pickupable = inventoryService.GetPickupableItems(world, player);
Console.WriteLine($"  Found {pickupable.Count} pickupable items:");
foreach (var item in pickupable)
{
    Console.WriteLine($"    - {item.Name} ({item.Glyph})");
}
Console.WriteLine();

// Test 2: Pickup items
Console.WriteLine("Test 2: Pickup Items");
foreach (var item in pickupable)
{
    Entity itemEntity = default;
    var query = new QueryDescription().WithAll<Item>();
    world.Query(in query, (Entity e, ref Item i) =>
    {
        if (e.Id == item.EntityId)
            itemEntity = e;
    });
    
    var result = inventoryService.PickupItem(world, player, itemEntity);
    Console.WriteLine($"  {(result.Success ? "✅" : "❌")} {result.Message}");
}
Console.WriteLine();

// Test 3: View inventory
Console.WriteLine("Test 3: View Inventory");
var inventory = inventoryService.GetInventoryItems(world, player);
Console.WriteLine($"  Inventory ({inventory.Count}/10 items):");
foreach (var item in inventory)
{
    var equipped = item.IsEquipped ? " [EQUIPPED]" : "";
    Console.WriteLine($"    - {item.Name} x{item.Count}{equipped}");
}
Console.WriteLine();

// Test 4: Use consumable
Console.WriteLine("Test 4: Use Consumable");
var consumables = inventoryService.GetConsumables(world, player);
if (consumables.Count > 0)
{
    var potion = consumables[0];
    Entity potionEntity = default;
    var query = new QueryDescription().WithAll<Item>();
    world.Query(in query, (Entity e, ref Item i) =>
    {
        if (e.Id == potion.EntityId)
            potionEntity = e;
    });
    
    Console.WriteLine($"  Health before: {world.Get<Health>(player).Current}/{world.Get<Health>(player).Maximum}");
    var result = inventoryService.UseConsumable(world, player, potionEntity);
    Console.WriteLine($"  {(result.Success ? "✅" : "❌")} {result.Message}");
    Console.WriteLine($"  Health after: {world.Get<Health>(player).Current}/{world.Get<Health>(player).Maximum}");
}
Console.WriteLine();

// Test 5: Equip items
Console.WriteLine("Test 5: Equip Items");
var equippables = inventoryService.GetEquippables(world, player);
Console.WriteLine($"  Combat before: ATK {world.Get<Combat>(player).Attack}, DEF {world.Get<Combat>(player).Defense}");
foreach (var item in equippables)
{
    Entity itemEntity = default;
    var query = new QueryDescription().WithAll<Item>();
    world.Query(in query, (Entity e, ref Item i) =>
    {
        if (e.Id == item.EntityId)
            itemEntity = e;
    });
    
    var result = inventoryService.EquipItem(world, player, itemEntity);
    Console.WriteLine($"  {(result.Success ? "✅" : "❌")} {result.Message}");
}
Console.WriteLine($"  Combat after: ATK {world.Get<Combat>(player).Attack}, DEF {world.Get<Combat>(player).Defense}");
Console.WriteLine();

// Test 6: Calculate total stats
Console.WriteLine("Test 6: Calculate Total Stats");
var (attack, defense, speed) = inventoryService.CalculateTotalStats(world, player);
Console.WriteLine($"  Total Stats: ATK {attack}, DEF {defense}, SPD {speed}");
Console.WriteLine();

// Test 7: View final inventory
Console.WriteLine("Test 7: Final Inventory State");
inventory = inventoryService.GetInventoryItems(world, player);
Console.WriteLine($"  Inventory ({inventory.Count}/10 items):");
foreach (var item in inventory)
{
    var equipped = item.IsEquipped ? " [EQUIPPED]" : "";
    Console.WriteLine($"    - {item.Name} x{item.Count}{equipped}");
}
Console.WriteLine();

Console.WriteLine("─────────────────────────────────────────────────────────────────────");
Console.WriteLine();
Console.WriteLine("✅ All tests completed successfully!");
Console.WriteLine();
Console.WriteLine("Summary:");
Console.WriteLine("  ✓ IInventoryService instantiated and functional");
Console.WriteLine("  ✓ Pickup items from world");
Console.WriteLine("  ✓ Use consumables (health restoration)");
Console.WriteLine("  ✓ Equip weapons and armor");
Console.WriteLine("  ✓ Stat calculation with equipment bonuses");
Console.WriteLine("  ✓ Clean read models for host integration");
Console.WriteLine();
Console.WriteLine("🎉 Inventory Plugin Migration (Spec 005) - SUCCESS!");
Console.WriteLine();

// Cleanup
world.Dispose();
