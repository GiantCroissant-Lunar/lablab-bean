namespace LablabBean.Game.Core.Components;

/// <summary>
/// Component for items that can be picked up
/// </summary>
public struct Item
{
    public ItemType Type { get; set; }
    public bool CanPickup { get; set; }

    public Item(ItemType type, bool canPickup = true)
    {
        Type = type;
        CanPickup = canPickup;
    }
}

/// <summary>
/// Types of items in the game
/// </summary>
public enum ItemType
{
    Weapon,
    Armor,
    Potion,
    Food,
    Key,
    Treasure,
    Misc
}

/// <summary>
/// Component for entities with an inventory
/// </summary>
public struct Inventory
{
    public List<int> Items { get; set; } // Entity IDs of items

    public Inventory()
    {
        Items = new List<int>();
    }

    public void AddItem(int itemEntityId)
    {
        Items.Add(itemEntityId);
    }

    public bool RemoveItem(int itemEntityId)
    {
        return Items.Remove(itemEntityId);
    }
}

/// <summary>
/// Component for consumable items (potions, food, etc.)
/// </summary>
public struct Consumable
{
    public int HealthRestore { get; set; }
    public int Uses { get; set; }

    public Consumable(int healthRestore, int uses = 1)
    {
        HealthRestore = healthRestore;
        Uses = uses;
    }

    public bool CanUse => Uses > 0;

    public void Use()
    {
        if (Uses > 0)
            Uses--;
    }
}

/// <summary>
/// Component for equippable items
/// </summary>
public struct Equippable
{
    public EquipSlot Slot { get; set; }
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public bool IsEquipped { get; set; }

    public Equippable(EquipSlot slot, int attackBonus = 0, int defenseBonus = 0)
    {
        Slot = slot;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        IsEquipped = false;
    }
}

/// <summary>
/// Equipment slots
/// </summary>
public enum EquipSlot
{
    Weapon,
    Armor,
    Shield,
    Helmet,
    Boots,
    Ring
}
