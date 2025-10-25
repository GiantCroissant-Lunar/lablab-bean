# Merchant System Documentation

## Overview

The Merchant System provides a complete trading economy with buying/selling items for gold currency. Players can interact with merchant NPCs to purchase supplies, equipment, and special items, or sell their loot for gold.

## Features

### 💰 Gold Currency

- **Gold Component**: Track player wealth
- **Transactions**: Buy items (spend gold) or sell items (earn gold)
- **Pricing**: Dynamic pricing based on merchant type and item rarity
- **Balance Checking**: Validate purchases before transaction

### 🏪 Merchant NPCs

- **Multiple Types**: General Store, Blacksmith, Alchemist, Magic Shop
- **Custom Inventory**: Each merchant has unique stock
- **Stock Management**: Limited or infinite quantities
- **Price Multipliers**: Merchants buy cheap, sell high
- **Stock Refresh**: Inventory replenishes over time

### 🛒 Trading System

- **Buy Items**: Purchase from merchant inventory
- **Sell Items**: Sell player items to merchants
- **Trade Sessions**: Start/end trading interactions
- **Price Calculation**: Dynamic pricing based on item value
- **Stock Tracking**: Finite vs infinite stock

## Components

### MerchantInventory

```csharp
public class MerchantInventory
{
    public Dictionary<Guid, MerchantStockItem> Stock { get; set; }
    public float BuyPriceMultiplier { get; set; } = 0.5f;  // Merchants buy at 50%
    public float SellPriceMultiplier { get; set; } = 1.0f; // Merchants sell at 100%
    public int RefreshInterval { get; set; } = 5;
    public int LastRefreshLevel { get; set; } = 0;
}
```

**Usage**:

```csharp
var merchant = world.Create(
    new NPC("Mara"),
    new MerchantInventory
    {
        BuyPriceMultiplier = 0.5f,  // Buys at 50% value
        SellPriceMultiplier = 1.0f,  // Sells at 100% value
        RefreshInterval = 5
    }
);
```

### TradeState

```csharp
public class TradeState
{
    public Guid PlayerId { get; set; }
    public Guid MerchantId { get; set; }
    public bool IsActive { get; set; }
}
```

## Systems

### MerchantSystem

Manages merchant stock and inventory operations.

**Methods**:

- `RefreshMerchantStock(Entity merchant, int currentLevel)`: Replenish stock
- `RefreshAllMerchants(int currentLevel)`: Refresh all merchant inventories
- `CalculateSellPrice(Entity merchant, int basePrice)`: Price for buying from merchant
- `CalculateBuyPrice(Entity merchant, int basePrice)`: Price for selling to merchant

### TradingSystem

Handles buy/sell transactions and gold management.

**Methods**:

- `BuyItem(Entity player, Entity merchant, Guid itemId, int quantity)`: Purchase item
- `SellItem(Entity player, Entity merchant, Guid itemId, int quantity)`: Sell item
- `GetPlayerGold(Entity player)`: Get gold amount
- `AddGold(Entity entity, int amount)`: Give gold
- `RemoveGold(Entity entity, int amount)`: Take gold
- `CanAffordPurchase(Entity player, int cost)`: Check affordability

## Service API

### MerchantService

The main API for trading interactions.

#### Trading

```csharp
// Start trade session
merchantService.StartTrade(playerId, merchantId);

// Buy item from merchant
bool success = merchantService.BuyItem(
    playerId: player.Id,
    merchantId: merchant.Id,
    itemId: healthPotionId,
    quantity: 3
);

if (success)
{
    Console.WriteLine("Purchased 3 health potions!");
}

// Sell item to merchant
bool sold = merchantService.SellItem(
    playerId: player.Id,
    merchantId: merchant.Id,
    itemId: oldSwordId,
    quantity: 1
);

// End trade session
merchantService.EndTrade(playerId);
```

#### Gold Management

```csharp
// Get player's gold
int gold = merchantService.GetGold(player.Id);
Console.WriteLine($"💰 Gold: {gold}");

// Add gold (quest reward, loot)
merchantService.AddGold(player.Id, 100);

// Remove gold (manual deduction)
bool removed = merchantService.RemoveGold(player.Id, 50);

// Check if can afford
bool canAfford = merchantService.CanAfford(player.Id, 75);
if (canAfford)
{
    Console.WriteLine("You can afford this item!");
}
```

#### Merchant Inventory

```csharp
// Get merchant's inventory
var inventory = merchantService.GetMerchantInventory(merchant.Id);

foreach (var item in inventory)
{
    Console.WriteLine($"{item.ItemName}");
    Console.WriteLine($"  Buy Price: {item.SellPrice} gold");
    Console.WriteLine($"  Sell Price: {item.BuyPrice} gold");
    Console.WriteLine($"  Stock: {(item.Quantity == -1 ? "Unlimited" : item.Quantity.ToString())}");
}

// Refresh merchant stock
merchantService.RefreshMerchantStock(merchant.Id, currentLevel: 5);

// Calculate prices
int sellPrice = merchantService.CalculateSellPrice(merchant.Id, itemId);
int buyPrice = merchantService.CalculateBuyPrice(merchant.Id, itemId);

// Check stock
bool inStock = merchantService.HasItemInStock(merchant.Id, itemId, quantity: 2);
```

#### Queries

```csharp
// Get active trade state
var tradeState = merchantService.GetTradeState(player.Id);
if (tradeState != null)
{
    Console.WriteLine($"Trading with: {tradeState.MerchantName}");
    Console.WriteLine($"Your gold: {tradeState.PlayerGold}");
}
```

## Merchant Definitions

Merchants are defined in JSON files in `Data/Merchants/`:

### Merchant JSON Structure

```json
{
  "id": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "name": "Mara's General Store",
  "description": "Everything you need for your adventure",
  "type": "GeneralStore",
  "buyPriceMultiplier": 0.5,
  "sellPriceMultiplier": 1.0,
  "inventory": [
    {
      "itemId": "10000000-0000-0000-0000-000000000001",
      "itemName": "Health Potion",
      "description": "Restores 50 HP instantly",
      "basePrice": 50,
      "initialStock": -1,
      "isInfinite": true,
      "minLevel": 1,
      "rarity": "Common"
    }
  ]
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Unique merchant identifier |
| `name` | string | Merchant display name |
| `description` | string | Flavor text |
| `type` | MerchantType | GeneralStore/Blacksmith/Alchemist/MagicShop |
| `buyPriceMultiplier` | float | Price when merchant buys from player (0.5 = 50%) |
| `sellPriceMultiplier` | float | Price when merchant sells to player (1.0 = 100%) |
| `inventory` | MerchantItem[] | Array of items for sale |

### Item Fields

| Field | Type | Description |
|-------|------|-------------|
| `itemId` | Guid | Unique item identifier |
| `itemName` | string | Item display name |
| `description` | string | Item description |
| `basePrice` | int | Base price before multipliers |
| `initialStock` | int | Starting quantity (-1 = infinite) |
| `isInfinite` | bool | Never runs out of stock |
| `minLevel` | int | Required player level |
| `rarity` | ItemRarity | Common/Uncommon/Rare/Epic/Legendary |

## Included Merchants

### 1. General Store (Mara)

- **Type**: General supplies and consumables
- **Buy**: 50% of value
- **Sell**: 100% of value
- **Stock**:
  - Health Potion: 50g (infinite)
  - Mana Potion: 50g (infinite)
  - Antidote: 30g (10 stock)
  - Torch: 10g (infinite)
  - Rope: 20g (5 stock)

### 2. Blacksmith (Tormund's Forge)

- **Type**: Weapons and armor
- **Buy**: 40% of value (cheaper than general)
- **Sell**: 120% of value (more expensive)
- **Stock**:
  - Iron Sword: 100g (3 stock, +5 Attack)
  - Iron Shield: 100g (3 stock, +5 Defense)
  - Steel Sword: 250g (2 stock, +10 Attack)
  - Steel Armor: 300g (2 stock, +10 Defense)
  - Mithril Sword: 1000g (1 stock, +20 Attack, Rare)

### 3. Magic Shop (Elara's Emporium)

- **Type**: Arcane items and spells
- **Buy**: 60% of value
- **Sell**: 150% of value (premium pricing)
- **Stock**:
  - Spell Tome: Magic Missile: 150g (1 stock)
  - Spell Tome: Fireball: 300g (1 stock)
  - Staff of Power: 500g (1 stock, +25% spell damage)
  - Mana Crystal: 400g (2 stock, +20 max mana)
  - Scroll of Teleportation: 200g (3 stock)

## Integration Examples

### With Quest System

```csharp
// Quest reward: Give gold
public void OnQuestComplete(Entity player, Quest quest)
{
    var merchantService = context.Registry.Get<MerchantService>();

    int goldReward = quest.GoldReward;
    merchantService.AddGold(player.Id, goldReward);

    Console.WriteLine($"💰 Received {goldReward} gold!");
}
```

### With NPC System

```csharp
// NPC dialogue triggers trade
public void OnDialogueAction(DialogueAction action, Entity player, Entity npc)
{
    if (action.Type == DialogueActionType.OpenTrade)
    {
        var merchantService = context.Registry.Get<MerchantService>();
        merchantService.StartTrade(player.Id, npc.Id);

        ShowTradeScreen(player, npc);
    }
}
```

### With Combat/Loot System

```csharp
// Drop gold on enemy death
public void OnEnemyKilled(Entity enemy, Entity player)
{
    var merchantService = context.Registry.Get<MerchantService>();

    // Random gold drop
    int goldDrop = Random.Shared.Next(10, 50);
    merchantService.AddGold(player.Id, goldDrop);

    Console.WriteLine($"💰 Found {goldDrop} gold!");
}
```

### With Spell System

```csharp
// Buy spell tomes
public void OnPurchaseSpellTome(Entity player, Guid spellTomeItemId)
{
    var spellService = context.Registry.Get<SpellService>();
    var spellId = GetSpellIdForTome(spellTomeItemId);

    if (!spellService.KnowsSpell(player.Id, spellId))
    {
        spellService.LearnSpell(player.Id, spellId);
        Console.WriteLine($"✨ Learned new spell!");
    }
    else
    {
        Console.WriteLine("You already know this spell!");
    }
}
```

## Complete Trading Example

```csharp
public void TradeWithMerchant(Entity player, Entity merchant)
{
    var merchantService = context.Registry.Get<MerchantService>();

    // Start trade
    merchantService.StartTrade(player.Id, merchant.Id);

    // Show player's gold
    var gold = merchantService.GetGold(player.Id);
    Console.WriteLine($"\n💰 Your Gold: {gold}");

    // Show merchant inventory
    Console.WriteLine("\n🏪 Available Items:");
    var inventory = merchantService.GetMerchantInventory(merchant.Id);
    int index = 1;

    foreach (var item in inventory.Where(i => i.InStock))
    {
        Console.WriteLine($"{index}. {item.ItemName} - {item.SellPrice}g");
        Console.WriteLine($"   {item.ItemDescription}");
        if (item.Quantity != -1)
            Console.WriteLine($"   Stock: {item.Quantity}");
        index++;
    }

    // Player chooses item
    Console.Write("\nBuy item (1-{0}), or 0 to leave: ", inventory.Count());
    int choice = int.Parse(Console.ReadLine() ?? "0");

    if (choice == 0)
    {
        merchantService.EndTrade(player.Id);
        return;
    }

    var selectedItem = inventory.ElementAt(choice - 1);

    // Check if can afford
    if (!merchantService.CanAfford(player.Id, selectedItem.SellPrice))
    {
        Console.WriteLine($"❌ Not enough gold! Need {selectedItem.SellPrice}g");
        return;
    }

    // Buy item
    bool success = merchantService.BuyItem(
        player.Id,
        merchant.Id,
        selectedItem.ItemId,
        quantity: 1
    );

    if (success)
    {
        Console.WriteLine($"✅ Purchased {selectedItem.ItemName}!");
        Console.WriteLine($"💰 Gold remaining: {merchantService.GetGold(player.Id)}");
    }
    else
    {
        Console.WriteLine("❌ Purchase failed!");
    }

    // End trade
    merchantService.EndTrade(player.Id);
}
```

## Pricing System

### Buy vs Sell Prices

```
Base Item Value: 100g

Merchant Selling to Player (Buy Price):
  100g × 1.0 (sell multiplier) = 100g

Merchant Buying from Player (Sell Price):
  100g × 0.5 (buy multiplier) = 50g

Markup: Merchant buys for 50g, sells for 100g (50% margin)
```

### Merchant Type Multipliers

| Merchant Type | Buy Multiplier | Sell Multiplier | Notes |
|---------------|----------------|-----------------|-------|
| General Store | 0.5 (50%) | 1.0 (100%) | Standard pricing |
| Blacksmith | 0.4 (40%) | 1.2 (120%) | Expensive equipment |
| Alchemist | 0.5 (50%) | 1.1 (110%) | Slightly premium |
| Magic Shop | 0.6 (60%) | 1.5 (150%) | Very expensive |

## Stock Refresh

```csharp
// Refresh merchant stock on level change
public void OnLevelChange(int newLevel)
{
    var merchantPlugin = GetPlugin<MerchantPlugin>();
    merchantPlugin.RefreshAllMerchants(newLevel);
}

// Refresh mechanics:
// - Happens every 5 levels by default
// - Infinite items: Never run out
// - Finite items: Restore 50-100% of base stock
// - Adds variety and prevents "sold out" frustration
```

## Creating Custom Merchants

### 1. Create JSON Definition

Create `tavern.json` in `Data/Merchants/`:

```json
{
  "id": "d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a",
  "name": "The Drunken Dragon Tavern",
  "description": "Food, drink, and information",
  "type": "Tavern",
  "buyPriceMultiplier": 0.3,
  "sellPriceMultiplier": 0.8,
  "inventory": [
    {
      "itemId": "40000000-0000-0000-0000-000000000001",
      "itemName": "Ale",
      "description": "Refreshing beverage. +5 HP",
      "basePrice": 5,
      "initialStock": -1,
      "isInfinite": true,
      "minLevel": 1,
      "rarity": "Common"
    },
    {
      "itemId": "40000000-0000-0000-0000-000000000002",
      "itemName": "Cooked Meal",
      "description": "Hot meal. Restores 30 HP",
      "basePrice": 20,
      "initialStock": 10,
      "isInfinite": false,
      "minLevel": 1,
      "rarity": "Common"
    }
  ]
}
```

### 2. Add Merchant Enum (Optional)

Edit `Data/MerchantItem.cs`:

```csharp
public enum MerchantType
{
    GeneralStore,
    Blacksmith,
    Alchemist,
    MagicShop,
    Tavern  // NEW
}
```

### 3. Reload and Test

Merchants are loaded on plugin initialization. Restart to see new merchant.

## Best Practices

### 💰 Economy Balance

- Start with low gold to create value
- Scale prices with dungeon level
- Make consumables affordable (potions)
- Make equipment expensive (weapons)
- Luxury items very expensive (rare spells)

### 🏪 Merchant Placement

- General Store: Dungeon entrance (always accessible)
- Blacksmith: Safe zones (every 5 levels)
- Magic Shop: Rare (every 10 levels)
- Specialty: Hidden or quest-unlocked

### 📊 Stock Management

- Infinite: Consumables (potions, torches)
- Limited: Equipment (weapons, armor)
- Unique: Rare items (1 stock only)
- Refresh: Every 5 levels

### 🎮 Player Experience

- Show prices before purchase
- Warn when low on gold
- Display stock quantities
- Allow "sell all junk" option
- Confirm expensive purchases

## Troubleshooting

### "Not enough gold"

- Check player has Gold component
- Verify gold amount with `GetGold()`
- Ensure prices are reasonable

### "Item not in stock"

- Check merchant has MerchantInventory
- Verify item exists in stock
- Check if item is sold out

### "No active trade session"

- Call `StartTrade()` before buy/sell
- Don't forget to `EndTrade()` when done
- Trade sessions are per-player

### "Merchant has no inventory"

- Add MerchantInventory component
- Load merchant from JSON definition
- Check merchant definition is valid

## Architecture

```
MerchantPlugin
├── Components
│   ├── MerchantInventory.cs
│   └── TradeState.cs
├── Data
│   ├── MerchantItem.cs
│   └── Merchants/
│       ├── general_store.json
│       ├── blacksmith.json
│       └── magic_shop.json
├── Systems
│   ├── MerchantSystem.cs (stock management)
│   └── TradingSystem.cs (buy/sell transactions)
├── Services
│   ├── MerchantService.cs (public API)
│   └── MerchantDatabase.cs (JSON loader)
└── MerchantPlugin.cs
```

## Version History

### v1.0.0 (Current)

- ✅ Gold currency system
- ✅ Buy/sell transactions
- ✅ Multiple merchant types
- ✅ Dynamic pricing
- ✅ Stock management with refresh
- ✅ 3 sample merchants
- ✅ Trade session tracking
- ✅ NPC dialogue integration ready

### Planned Features

- 🔜 Reputation system (discounts for high rep)
- 🔜 Bartering/haggling mechanics
- 🔜 Merchant quests
- 🔜 Black market merchants
- 🔜 Auction house
- 🔜 Trading cards/collectibles

---

**Ready for Phase 9 or final polish!** 🚀
