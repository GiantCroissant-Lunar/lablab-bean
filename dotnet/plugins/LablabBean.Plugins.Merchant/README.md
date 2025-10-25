# Merchant Trading Plugin 💰

**Version**: 1.0.0 | **Plugin Type**: Gameplay System

---

## Overview

Complete economy system with merchant NPCs, dynamic pricing, and gold-based transactions.

## Features

- **3 Merchant Types**: General, Weapons, Potions
- **50+ Items**: Weapons, armor, potions, scrolls, materials
- **Dynamic Pricing**: Reputation-based discounts
- **Stock Management**: Limited quantities, restocking
- **Gold Economy**: Currency-based trading

## Quick Start

```csharp
services.AddMerchantSystem();

// Create merchant
var merchant = merchantFactory.CreateMerchant(
    "Greta the Smith",
    MerchantType.Weapons,
    baseGold: 1000
);

// Open trade
merchantService.OpenTrade(playerEntity, merchantEntity);

// Buy item
merchantService.BuyItem(playerEntity, merchantEntity, "iron_sword");
```

## Components

### Merchant

```csharp
string Name;
MerchantType Type;          // General, Weapons, Potions
int Gold;                   // Merchant's gold reserves
List<MerchantItem> Stock;   // Available items
float PriceModifier;        // Base pricing (1.0 = standard)
```

### MerchantItem

```csharp
string ItemId;
int Quantity;               // Stock count
int BasePrice;              // Standard price
int CurrentPrice;           // After modifiers
bool IsInfinite;            // Unlimited stock
```

### Currency

```csharp
int Gold;                   // Player's gold
```

## Merchant Database

### 1. General Merchant (Eldrin's Goods)

- Health Potions (Small, Medium, Large)
- Mana Potions
- Antidotes
- Rations
- Torches
- Rope
- Starting equipment

**Base Prices**: 10g - 100g

### 2. Weapons Merchant (Greta's Forge)

- Swords: Iron (50g), Steel (150g), Mithril (500g)
- Axes: Iron (60g), Steel (180g)
- Bows: Shortbow (75g), Longbow (200g)
- Armor: Leather (40g), Chain (120g), Plate (400g)
- Shields: Wooden (30g), Iron (100g)

**Base Prices**: 30g - 500g

### 3. Potions Merchant (Mora's Mysticals)

- Strength Potions (+5 Attack, 3 turns) - 80g
- Defense Potions (+5 Defense, 3 turns) - 80g
- Speed Potions (+2 Speed, 5 turns) - 60g
- Invisibility Potions (1 turn) - 150g
- Resurrection Scrolls - 300g
- Identify Scrolls - 50g

**Base Prices**: 50g - 300g

## Services

### IMerchantService

```csharp
Entity CreateMerchant(string name, MerchantType type, int gold);
void OpenTrade(Entity player, Entity merchant);
void CloseTrade(Entity player);
bool BuyItem(Entity player, Entity merchant, string itemId, int quantity = 1);
bool SellItem(Entity player, Entity merchant, string itemId, int quantity = 1);
int CalculatePrice(Entity player, Entity merchant, string itemId, bool isBuying);
void RestockMerchant(Entity merchant);
```

## Pricing System

### Base Formula

```csharp
finalPrice = basePrice * merchantModifier * reputationModifier * rarityModifier
```

### Reputation Discounts

- **Hostile** (<-50): +50% price
- **Unfriendly** (-50 to 0): +25% price
- **Neutral** (0): Standard price
- **Friendly** (1-50): -10% price
- **Allied** (>50): -25% price

### Item Rarity Multipliers

- **Common**: 1.0x
- **Uncommon**: 1.5x
- **Rare**: 3.0x
- **Epic**: 7.0x
- **Legendary**: 15.0x

## Stock Management

### Restocking

- **Frequency**: Every 24 in-game hours
- **Quantity**: 50-100% of base stock
- **New Items**: 10% chance of rare items appearing

### Limited Stock

```csharp
// Check availability
if (merchant.GetStockQuantity("health_potion") > 0)
{
    merchantService.BuyItem(player, merchant, "health_potion");
}
```

## Integration

### With NPC System

```csharp
// Dialogue action: Open trade
var action = new DialogueAction
{
    Type = DialogueActionType.OpenTrade
};
```

### With Inventory System

```csharp
// Buy item: add to inventory
if (merchantService.BuyItem(player, merchant, itemId))
{
    inventoryService.AddItem(player, itemId);
}

// Sell item: remove from inventory
if (merchantService.SellItem(player, merchant, itemId))
{
    inventoryService.RemoveItem(player, itemId);
}
```

### With Reputation System

```csharp
// Calculate discount
int reputation = npcService.GetReputation(player, merchant);
float discount = merchantService.CalculateReputationDiscount(reputation);
int finalPrice = basePrice * (1.0f - discount);
```

## Events

- `TradeOpenedEvent(Entity player, Entity merchant)`
- `TradeClosedEvent(Entity player)`
- `ItemPurchasedEvent(Entity player, string itemId, int quantity, int totalCost)`
- `ItemSoldEvent(Entity player, string itemId, int quantity, int totalProfit)`
- `MerchantRestockedEvent(Entity merchant)`

## Economy Balance

### Player Gold Sources

- Quest rewards: 50-500g per quest
- Enemy drops: 5-50g per enemy
- Selling loot: 50% of purchase price

### Gold Sinks

- Equipment upgrades
- Potions and consumables
- Spell scrolls
- Inn stays and repairs

### Progression Curve

- **Level 1-5**: 100-500g (basic equipment)
- **Level 6-10**: 500-2000g (mid-tier gear)
- **Level 11-15**: 2000-10000g (high-end items)
- **Level 16-20**: 10000+ (legendary equipment)

## Performance

- Price calculations: <0.5ms
- Stock updates: <1ms per merchant
- Trade UI updates: <5ms
- Supports 100+ merchants

## Configuration

```json
{
  "defaultMerchantGold": 1000,
  "playerStartingGold": 100,
  "sellPriceRatio": 0.5,
  "restockIntervalHours": 24,
  "maxStockPerItem": 10,
  "merchantStockPath": "Data/Merchants/merchant_stock.json"
}
```

---

**See**: [INTEGRATION_EXAMPLES.md](INTEGRATION_EXAMPLES.md)
