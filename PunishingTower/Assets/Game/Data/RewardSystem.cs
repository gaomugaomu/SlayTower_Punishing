using System.Collections.Generic;
using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>Victory reward categories (doc 218).</summary>
    public enum RewardType
    {
        BlackCard,
        Orb,
        Relic
    }

    /// <summary>A single reward granted after victory.</summary>
    [System.Serializable]
    public class RewardEntry
    {
        public RewardType type;
        public int amount;
        public string label;

        public RewardEntry(RewardType type, int amount, string label)
        {
            this.type = type;
            this.amount = amount;
            this.label = label;
        }

        public override string ToString()
        {
            return $"{label} x{amount}";
        }
    }

    /// <summary>
    /// Generates victory rewards (doc 218). Simplified prototype:
    /// normal battle = black cards + one random reward.
    /// </summary>
    public static class RewardGenerator
    {
        /// <summary>Generates rewards for a normal battle victory.</summary>
        public static List<RewardEntry> GenerateNormalBattle(System.Random rng, int blackCards = 10)
        {
            var rewards = new List<RewardEntry>
            {
                new RewardEntry(RewardType.BlackCard, blackCards, "黑卡")
            };

            int roll = rng.Next(3);
            switch (roll)
            {
                case 0:
                    rewards.Add(new RewardEntry(RewardType.Orb, 1, "信号球(获得1球)"));
                    break;
                case 1:
                    rewards.Add(new RewardEntry(RewardType.Relic, 1, "遗物"));
                    break;
                default:
                    rewards.Add(new RewardEntry(RewardType.BlackCard, 5, "黑卡(额外)"));
                    break;
            }

            return rewards;
        }
    }
}
