using System;
using PunishingTower.Core;

namespace PunishingTower.Combat
{
    /// <summary>Infection danger levels driving turn-start penalties.</summary>
    public enum InfectionLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Commander runtime state: shared HP across the squad, infection value and serum.
    /// Defeat when HP reaches 0 or infection reaches 100.
    /// </summary>
    public class CommanderState : ICombatActor
    {
        public const int DefaultMaxHp = 100;
        public const int DefaultMaxSerum = 3;
        public const int SerumReduction = 25;
        public const int CriticalInfection = 100;

        public string Id => "commander";
        public string DisplayName => "Commander";

        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Shield { get; private set; }
        public int Infection { get; private set; }
        public int SerumCount { get; private set; }
        public int MaxSerum { get; }

        public bool IsDefeated => Hp <= 0 || Infection >= CriticalInfection;
        public bool IsShielded => Shield > 0;

        public CommanderState(int maxHp = DefaultMaxHp, int maxSerum = DefaultMaxSerum, int serumCount = DefaultMaxSerum)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            MaxSerum = maxSerum;
            SerumCount = Math.Min(serumCount, maxSerum);
        }

        public InfectionLevel GetInfectionLevel()
        {
            if (Infection >= CriticalInfection) { return InfectionLevel.Critical; }
            if (Infection >= 67) { return InfectionLevel.High; }
            if (Infection >= 34) { return InfectionLevel.Medium; }
            return InfectionLevel.Low;
        }

        /// <summary>Shield absorbs first; remaining damage hits HP and raises infection by half the raw damage.</summary>
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
                AddInfection(hpDamage / 2);
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

        public void AddInfection(int amount)
        {
            if (amount > 0)
            {
                Infection = Math.Min(CriticalInfection, Infection + amount);
            }
        }

        /// <summary>Consumes one serum and reduces infection. Returns false when no serum is left.</summary>
        public bool UseSerum()
        {
            if (SerumCount <= 0)
            {
                return false;
            }
            SerumCount--;
            Infection = Math.Max(0, Infection - SerumReduction);
            return true;
        }

        /// <summary>
        /// Turn-start infection penalty. Low: none. Medium: 1 HP. High: 3 HP.
        /// Returns the HP lost (0 when no penalty).
        /// </summary>
        public int ApplyTurnStartPenalty()
        {
            int damage = 0;
            switch (GetInfectionLevel())
            {
                case InfectionLevel.Medium: damage = 1; break;
                case InfectionLevel.High: damage = 3; break;
            }

            if (damage > 0)
            {
                Hp = Math.Max(0, Hp - damage);
            }
            return damage;
        }
    }
}
