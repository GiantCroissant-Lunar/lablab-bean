# Merchant System Quick Start

## 5-Minute Integration Guide

### Step 1: Initialize Player with Gold

```csharp
using LablabBean.Game.Core.Components;

// Create player with starting gold
var player = world.Create(
    new Player("Hero"),
    new Health(100, 100),
    new Gold(100)  // Start with 100 gold
);
```

### Step 2: Create a Merchant

```csharp
using LablabBean.Plugins.Merchant.Components;
using LablabBean.Plugins.NPC.Components;

// Create merchant NPC with inventory
var merchant = world.Create(
    new NPC("Mara"),
    new Name("Mara's General Store"),
    new MerchantInventory
    {
        BuyPriceMultiplier = 0.5f,  // Buys at 50% value
        SellPriceMultiplier = 1.0f,  // Sells at 100% value
        RefreshInterval = 5
    }
);

// Add items to merchant stock
ref var inventory = ref merchant.Get<MerchantInventory>();
inventory.AddStock(
    itemId: healthPotionId,
    basePrice: 50,
    quantity: 10,
    isInfinite: true
);
```

### Step 3: Get the Merchant Service

```csharp
var merchantService = context.Registry.Get<MerchantService>();
```

### Step 4: Your First Purchase

```csharp
// Start trading
merchantService.StartTrade(player.Id, merchant.Id);

// Check gold
int gold = merchantService.GetGold(player.Id);
Console.WriteLine($"💰 Your Gold: {gold}");

// Buy item
bool success = merchantService.BuyItem(
    playerId: player.Id,
    merchantId: merchant.Id,
    itemId: healthPotionId,
    quantity: 2
);

if (success)
{
    Console.WriteLine("✅ Purchased 2 health potions!");
    Console.WriteLine($"💰 Remaining: {merchantService.GetGold(player.Id)}g");
}
else
{
    Console.WriteLine("❌ Purchase failed!");
}

// End trading
merchantService.EndTrade(player.Id);
```

### Step 5: Manage Gold

```csharp
// Add gold (quest reward)
merchantService.AddGold(player.Id, 50);

// Remove gold
merchantService.RemoveGold(player.Id, 25);

// Check affordability
bool canAfford = merchantService.CanAfford(player.Id, 75);
if (canAfford)
{
    Console.WriteLine("You can afford this!");
}
```

## Complete Example: Trading Screen

```csharp
public void ShowTradeScreen(Entity player, Entity merchant)
{
    var merchantService = context.Registry.Get<MerchantService>();

    // Start trade
    merchantService.StartTrade(player.Id, merchant.Id);

    while (true)
    {
        // Show player gold
        var gold = merchantService.GetGold(player.Id);
        Console.WriteLine($"\n💰 Your Gold: {gold}");

        // Show merchant inventory
        Console.WriteLine("\n🏪 Available Items:");
        var inventory = merchantService.GetMerchantInventory(merchant.Id);
        int index = 1;

        foreach (var item in inventory.Where(i => i.InStock))
        {
            var affordSymbol = gold >= item.SellPrice ? "✅" : "❌";
            Console.WriteLine($"{index}. {affordSymbol} {item.ItemName} - {item.SellPrice}g");

            if (item.Quantity == -1)
                Console.WriteLine("   (Unlimited stock)");
            else
                Console.WriteLine($"   (Stock: {item.Quantity})");

            index++;
        }

        // Player choice
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  [1-N] Buy item");
        Console.WriteLine("  [S] Sell items");
        Console.WriteLine("  [L] Leave");
        Console.Write("\nChoice: ");

        var choice = Console.ReadLine()?.ToUpper();

        if (choice == "L")
        {
            break;
        }
        else if (choice == "S")
        {
            // Show sell screen
            ShowSellScreen(player, merchant);
        }
        else if (int.TryParse(choice, out int itemIndex))
        {
            // Buy item
            var selectedItem = inventory.ElementAt(itemIndex - 1);

            if (!merchantService.CanAfford(player.Id, selectedItem.SellPrice))
            {
                Console.WriteLine($"❌ Not enough gold! Need {selectedItem.SellPrice}g");
                continue;
            }

            bool success = merchantService.BuyItem(
                player.Id,
                merchant.Id,
                selectedItem.ItemId,
                quantity: 1
            );

            if (success)
            {
                Console.WriteLine($"✅ Purchased {selectedItem.ItemName}!");
            }
            else
            {
                Console.WriteLine("❌ Purchase failed!");
            }
        }
    }

    // End trade
    merchantService.EndTrade(player.Id);
    Console.WriteLine("\nThanks for visiting!");
}
```

## Merchant Definitions

### All Merchant IDs

```csharp
public static class Merchants
{
    // General Store - Basic supplies
    public static readonly Guid GeneralStore =
        Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d");

    // Blacksmith - Weapons & armor
    public static readonly Guid Blacksmith =
        Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e");

    // Magic Shop - Spells & magical items
    public static readonly Guid MagicShop =
        Guid.Parse("c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f");
}
```

### Sample Item IDs

```csharp
// General Store Items
public static readonly Guid HealthPotion =
    Guid.Parse("10000000-0000-0000-0000-000000000001");

public static readonly Guid ManaPotion =
    Guid.Parse("10000000-0000-0000-0000-000000000002");

// Blacksmith Items
public static readonly Guid IronSword =
    Guid.Parse("20000000-0000-0000-0000-000000000001");

public static readonly Guid IronShield =
    Guid.Parse("20000000-0000-0000-0000-000000000002");

// Magic Shop Items
public static readonly Guid SpellTomeMagicMissile =
    Guid.Parse("30000000-0000-0000-0000-000000000001");

public static readonly Guid SpellTomeFireball =
    Guid.Parse("30000000-0000-0000-0000-000000000002");
```

## Gold Drop on Enemy Death

```csharp
public void OnEnemyKilled(Entity enemy, Entity player)
{
    var merchantService = context.Registry.Get<MerchantService>();

    // Random gold drop based on enemy type
    int goldDrop = enemy.Get<Enemy>().Type switch
    {
        "Goblin" => Random.Shared.Next(5, 15),
        "Orc" => Random.Shared.Next(10, 30),
        "Dragon" => Random.Shared.Next(100, 500),
        _ => Random.Shared.Next(1, 10)
    };

    merchantService.AddGold(player.Id, goldDrop);
    Console.WriteLine($"💰 Found {goldDrop} gold!");
}
```

## Quest Integration

```csharp
public void OnQuestComplete(Entity player, Quest quest)
{
    var merchantService = context.Registry.Get<MerchantService>();

    // Award gold
    if (quest.GoldReward > 0)
    {
        merchantService.AddGold(player.Id, quest.GoldReward);
        Console.WriteLine($"💰 Received {quest.GoldReward} gold!");
    }
}
```

## NPC Dialogue Integration

```csharp
public void OnNPCInteraction(Entity player, Entity npc)
{
    // Check if NPC is a merchant
    if (npc.Has<MerchantInventory>())
    {
        Console.WriteLine("This NPC is a merchant!");
        Console.WriteLine("[T] Trade with merchant");

        var input = Console.ReadKey();
        if (input.KeyChar == 'T' || input.KeyChar == 't')
        {
            ShowTradeScreen(player, npc);
        }
    }
}
```

## Stock Refresh

```csharp
// On level change
public void OnLevelChange(int newLevel)
{
    var merchantPlugin = GetPlugin<MerchantPlugin>();
    merchantPlugin.RefreshAllMerchants(newLevel);

    Console.WriteLine($"📦 Merchant stock refreshed for level {newLevel}");
}
```

## Testing

```csharp
public void TestMerchantSystem()
{
    var world = new World();
    var merchantService = GetMerchantService();

    // Create test player with gold
    var player = world.Create(
        new Player("Test"),
        new Gold(500)
    );

    // Create test merchant
    var merchant = world.Create(
        new NPC("TestMerchant"),
        new Name("Test Shop"),
        new MerchantInventory()
    );

    // Add test items
    ref var inv = ref merchant.Get<MerchantInventory>();
    inv.AddStock(Guid.NewGuid(), 50, 10, false);
    inv.AddStock(Guid.NewGuid(), 100, 5, false);

    Console.WriteLine("=== Testing Buy ===");
    merchantService.StartTrade(player.Id, merchant.Id);

    var items = merchantService.GetMerchantInventory(merchant.Id);
    var firstItem = items.First();

    bool success = merchantService.BuyItem(
        player.Id,
        merchant.Id,
        firstItem.ItemId,
        1
    );

    Console.WriteLine($"Buy Success: {success}");
    Console.WriteLine($"Gold After: {merchantService.GetGold(player.Id)}");

    merchantService.EndTrade(player.Id);

    Console.WriteLine("\n✅ Tests passed!");
}
```

## Common Patterns

### Check Before Purchase

```csharp
if (merchantService.CanAfford(player.Id, itemPrice))
{
    merchantService.BuyItem(player.Id, merchant.Id, itemId);
}
else
{
    Console.WriteLine("Not enough gold!");
}
```

### Sell Multiple Items

```csharp
foreach (var itemId in playerJunk)
{
    merchantService.SellItem(player.Id, merchant.Id, itemId, 1);
}
Console.WriteLine("Sold all junk items!");
```

### Show Merchant Greeting

```csharp
var npcName = merchant.Get<Name>().Value;
var gold = merchantService.GetGold(player.Id);

Console.WriteLine($"Welcome to {npcName}!");
Console.WriteLine($"You have {gold} gold.");
```

## That's It

You now have a fully functional merchant system!

**Next Steps:**

- Add UI for trade screen
- Create more merchant types
- Implement item database
- Add reputation system
- Create merchant quests

See `MERCHANT_SYSTEM.md` for complete documentation! 🚀
