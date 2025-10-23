namespace LablabBean.Game.Core.Components;

/// <summary>
/// Tag component marking an entity as the player
/// </summary>
public struct Player
{
    public string Name { get; set; }

    public Player(string name = "Player")
    {
        Name = name;
    }
}

/// <summary>
/// Component for entities that can take damage
/// </summary>
public struct Health
{
    public int Current { get; set; }
    public int Maximum { get; set; }

    public Health(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }

    public bool IsAlive => Current > 0;
    public float Percentage => Maximum > 0 ? (float)Current / Maximum : 0f;
}

/// <summary>
/// Component for entities that can deal damage
/// </summary>
public struct Combat
{
    public int Attack { get; set; }
    public int Defense { get; set; }

    public Combat(int attack, int defense)
    {
        Attack = attack;
        Defense = defense;
    }
}

/// <summary>
/// Tag component for enemy entities
/// Can inflict status effects on attack
/// </summary>
public struct Enemy
{
    public string Type { get; set; }

    // Status effect attack properties
    public EffectType? InflictsEffect { get; set; }
    public int? EffectProbability { get; set; }  // 0-100 percentage
    public int? EffectMagnitude { get; set; }
    public int? EffectDuration { get; set; }

    public Enemy(string type)
    {
        Type = type;
        InflictsEffect = null;
        EffectProbability = null;
        EffectMagnitude = null;
        EffectDuration = null;
    }
}

/// <summary>
/// Component for entities that can move and act
/// </summary>
public struct Actor
{
    /// <summary>
    /// Energy accumulated for taking actions
    /// When >= 100, the actor can take an action
    /// </summary>
    public int Energy { get; set; }

    /// <summary>
    /// Speed determines how quickly energy accumulates
    /// </summary>
    public int Speed { get; set; }

    public Actor(int speed = 100)
    {
        Energy = 0;
        Speed = speed;
    }

    public bool CanAct => Energy >= 100;

    public void ConsumeEnergy(int cost = 100)
    {
        Energy -= cost;
    }

    public void AccumulateEnergy()
    {
        Energy += Speed;
    }
}

/// <summary>
/// Component for AI-controlled entities
/// </summary>
public struct AI
{
    public AIBehavior Behavior { get; set; }

    public AI(AIBehavior behavior = AIBehavior.Wander)
    {
        Behavior = behavior;
    }
}

/// <summary>
/// AI behavior types
/// </summary>
public enum AIBehavior
{
    Wander,
    Chase,
    Flee,
    Patrol,
    Idle
}

/// <summary>
/// Component for entities that block movement
/// </summary>
public struct BlocksMovement
{
    public bool Blocks { get; set; }

    public BlocksMovement(bool blocks = true)
    {
        Blocks = blocks;
    }
}

/// <summary>
/// Component for naming entities
/// </summary>
public struct Name
{
    public string Value { get; set; }

    public Name(string value)
    {
        Value = value;
    }
}
