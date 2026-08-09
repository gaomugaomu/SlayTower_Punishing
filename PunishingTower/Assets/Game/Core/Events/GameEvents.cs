namespace PunishingTower.Core.Events
{
    /// <summary>Raised when a battle begins.</summary>
    public sealed class BattleStartEvent : IGameEvent
    {
        public int Round { get; }

        public BattleStartEvent(int round)
        {
            Round = round;
        }
    }

    /// <summary>Raised when a battle ends. Victory when the commander survived.</summary>
    public sealed class BattleEndEvent : IGameEvent
    {
        public bool Victory { get; }

        public BattleEndEvent(bool victory)
        {
            Victory = victory;
        }
    }

    /// <summary>Raised at the beginning of a new player turn.</summary>
    public sealed class TurnStartEvent : IGameEvent
    {
        public int Round { get; }

        public TurnStartEvent(int round)
        {
            Round = round;
        }
    }

    /// <summary>Raised when the player ends their turn.</summary>
    public sealed class TurnEndEvent : IGameEvent
    {
        public int Round { get; }

        public TurnEndEvent(int round)
        {
            Round = round;
        }
    }

    /// <summary>Raised whenever a signal orb is played.</summary>
    public sealed class OrbPlayedEvent : IGameEvent
    {
        public string OrbId { get; }
        public int OrbColor { get; }

        public OrbPlayedEvent(string orbId, int orbColor)
        {
            OrbId = orbId;
            OrbColor = orbColor;
        }
    }

    /// <summary>Raised when three or more consecutive orbs of the same color are matched.</summary>
    public sealed class ThreeMatchEvent : IGameEvent
    {
        public int Color { get; }
        public int Count { get; }

        public ThreeMatchEvent(int color, int count)
        {
            Color = color;
            Count = count;
        }
    }

    /// <summary>Raised when a construct uses an ultimate skill.</summary>
    public sealed class UltimateEvent : IGameEvent
    {
        public string ConstructId { get; }

        public UltimateEvent(string constructId)
        {
            ConstructId = constructId;
        }
    }

    /// <summary>Raised when any combat actor takes damage.</summary>
    public sealed class DamageTakenEvent : IGameEvent
    {
        public string TargetId { get; }
        public int Amount { get; }

        public DamageTakenEvent(string targetId, int amount)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }

    /// <summary>Raised when commander infection value changes.</summary>
    public sealed class InfectionChangedEvent : IGameEvent
    {
        public int NewValue { get; }

        public InfectionChangedEvent(int newValue)
        {
            NewValue = newValue;
        }
    }

    /// <summary>Raised when a normal attack is performed.</summary>
    public sealed class BasicAttackEvent : IGameEvent
    {
        public string ActorId { get; }

        public BasicAttackEvent(string actorId)
        {
            ActorId = actorId;
        }
    }
}
