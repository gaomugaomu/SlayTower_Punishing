namespace PunishingTower.Combat
{
    /// <summary>What an enemy plans to do, shown to the player before the enemy acts.</summary>
    public enum IntentType
    {
        Attack,
        Defense,
        Buff,
        Debuff,
        Special
    }

    /// <summary>Enemy intent data: decided by AI, displayed by UI, executed on enemy turn.</summary>
    public class EnemyIntent
    {
        public IntentType Type { get; }
        public int Amount { get; }
        public string Description { get; }

        public EnemyIntent(IntentType type, int amount, string description)
        {
            Type = type;
            Amount = amount;
            Description = description;
        }

        public static EnemyIntent Attack(int damage)
        {
            return new EnemyIntent(IntentType.Attack, damage, $"Attack {damage}");
        }

        public static EnemyIntent Defense(int shield)
        {
            return new EnemyIntent(IntentType.Defense, shield, $"Gain {shield} shield");
        }

        public static EnemyIntent Buff(int amount)
        {
            return new EnemyIntent(IntentType.Buff, amount, $"Buff {amount}");
        }

        public static EnemyIntent Debuff(int amount)
        {
            return new EnemyIntent(IntentType.Debuff, amount, $"Debuff {amount}");
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
