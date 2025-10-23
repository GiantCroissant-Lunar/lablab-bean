namespace LablabBean.Game.Core.Components;

/// <summary>
/// Base component for all items in the game.
/// Attached to entities that represent items (on ground or in inventory).
/// </summary>
public struct Item
{
    public string Name { get; set; }
    public char Glyph { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public int Weight { get; set; }

    public Item(string name, char glyph, ItemType type, string description = "", int weight = 1)
    {
        Name = name;
        Glyph = glyph;
        Description = description;
        Type = type;
        Weight = weight;
    }
}

/// <summary>
/// Types of items in the game
/// </summary>
public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Accessory,
    Miscellaneous
}

/// <summary>
/// Component attached to player entity to store inventory.
/// Contains references to item entities.
/// </summary>
public struct Inventory
{
    public List<int> Items { get; set; }
    public int MaxCapacity { get; set; }

    public Inventory(int maxCapacity = 20)
    {
        Items = new List<int>();
        MaxCapacity = maxCapacity;
    }

    public readonly int CurrentCount => Items?.Count ?? 0;
    public readonly bool IsFull => CurrentCount >= MaxCapacity;

    public void AddItem(int itemEntityId)
    {
        if (!IsFull)
        {
            Items.Add(itemEntityId);
        }
    }

    public bool RemoveItem(int itemEntityId)
    {
        return Items.Remove(itemEntityId);
    }
}

/// <summary>
/// Component for items that can be consumed (used once and destroyed).
/// </summary>
public struct Consumable
{
    public ConsumableEffect Effect { get; set; }
    public int EffectValue { get; set; }
    public bool UsableOutOfCombat { get; set; }

    // Status effect properties
    public EffectType? AppliesEffect { get; set; }
    public int? EffectMagnitude { get; set; }
    public int? EffectDuration { get; set; }
    public EffectType? RemovesEffect { get; set; }
    public bool RemovesAllNegativeEffects { get; set; }

    public Consumable(ConsumableEffect effect, int effectValue, bool usableOutOfCombat = true)
    {
        Effect = effect;
        EffectValue = effectValue;
        UsableOutOfCombat = usableOutOfCombat;
        AppliesEffect = null;
        EffectMagnitude = null;
        EffectDuration = null;
        RemovesEffect = null;
        RemovesAllNegativeEffects = false;
    }
}

/// <summary>
/// Types of consumable effects
/// </summary>
public enum ConsumableEffect
{
    RestoreHealth,
    RestoreMana,
    IncreaseSpeed,
    CurePoison,
    ApplyStatusEffect  // New: for potions that apply buffs/debuffs
}

/// <summary>
/// Component for items that can be equipped to modify stats.
/// </summary>
public struct Equippable
{
    public EquipmentSlot Slot { get; set; }
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int SpeedModifier { get; set; }
    public bool TwoHanded { get; set; }

    public Equippable(EquipmentSlot slot, int attackBonus = 0, int defenseBonus = 0, int speedModifier = 0, bool twoHanded = false)
    {
        Slot = slot;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        SpeedModifier = speedModifier;
        TwoHanded = twoHanded;
    }
}

/// <summary>
/// Equipment slot types
/// </summary>
public enum EquipmentSlot
{
    MainHand,
    OffHand,
    Head,
    Chest,
    Legs,
    Feet,
    Hands,
    Accessory1,
    Accessory2
}

/// <summary>
/// Component for items that can stack (multiple instances in one inventory slot).
/// Typically used for consumables.
/// </summary>
public struct Stackable
{
    public int Count { get; set; }
    public int MaxStack { get; set; }

    public Stackable(int count = 1, int maxStack = 99)
    {
        Count = count;
        MaxStack = maxStack;
    }

    public readonly bool IsFull => Count >= MaxStack;
    public readonly bool IsEmpty => Count <= 0;
}

/// <summary>
/// Component attached to player entity to track equipped items.
/// Maps equipment slots to item entity references.
/// </summary>
public struct EquipmentSlots
{
    public Dictionary<EquipmentSlot, int?> Slots { get; set; }

    public EquipmentSlots()
    {
        Slots = CreateEmptySlots();
    }

    public static Dictionary<EquipmentSlot, int?> CreateEmptySlots()
    {
        return new Dictionary<EquipmentSlot, int?>
        {
            { EquipmentSlot.MainHand, null },
            { EquipmentSlot.OffHand, null },
            { EquipmentSlot.Head, null },
            { EquipmentSlot.Chest, null },
            { EquipmentSlot.Legs, null },
            { EquipmentSlot.Feet, null },
            { EquipmentSlot.Hands, null },
            { EquipmentSlot.Accessory1, null },
            { EquipmentSlot.Accessory2, null }
        };
    }
}
