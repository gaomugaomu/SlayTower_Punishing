using System;
using PunishingTower.Core;

namespace PunishingTower.Combat
{
    /// <summary>Runtime enemy state. Enemies target the commander; they have no energy or orbs.</summary>
    public class EnemyState : ICombatActor
    {
        public string Id { get; }
        public string DisplayName { get; }

        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Shield { get; private set; }

        /// <summary>Attack intent damage dealt to the commander.</summary>
        public int AttackDamage { get; }

        /// <summary>Shield gained when the enemy takes a defense intent.</summary>
        public int DefenseShield { get; }

        public bool IsDefeated => Hp <= 0;
        public bool IsShielded => Shield > 0;

        public EnemyState(string id, string displayName, int maxHp, int attackDamage = 10, int defenseShield = 8)
        {
            Id = id;
            DisplayName = displayName;
            MaxHp = maxHp;
            Hp = maxHp;
            AttackDamage = attackDamage;
            DefenseShield = defenseShield;
        }

        /// <summary>Shield absorbs first; remaining damage hits HP. Returns the HP damage dealt.</summary>
        public int TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int absorbed = Math.Min(Shield, amount);
            Shield -= absorbed;
            int hpDamage = amount - absorbed;
            if (hpDamage > 0)
            {
                Hp = Math.Max(0, Hp - hpDamage);
            }
            return hpDamage;
        }

        public void Heal(int amount)
        {
            if (amount > 0)
            {
                Hp = Math.Min(MaxHp, Hp + amount);
            }
        }

        public void AddShield(int amount)
        {
            if (amount > 0)
            {
                Shield += amount;
            }
        }
    }
}
