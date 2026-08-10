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

        /// <summary>For Orb rewards: the orb color granted (for UI display).</summary>
        public OrbColor orbColor;

        public RewardEntry(RewardType type, int amount, string label, OrbColor orbColor = OrbColor.Red)
        {
            this.type = type;
            this.amount = amount;
            this.label = label;
            this.orbColor = orbColor;
        }

        public override string ToString()
        {
            if (type == RewardType.Orb)
            {
                string colorName;
                switch (orbColor)
                {
                    case OrbColor.Red: colorName = "红"; break;
                    case OrbColor.Yellow: colorName = "黄"; break;
                    case OrbColor.Blue: colorName = "蓝"; break;
                    case OrbColor.White: colorName = "白"; break;
                    default: colorName = "?"; break;
                }
                return $"{label}({colorName}球) x{amount}";
            }
            return $"{label} x{amount}";
        }
    }

    /// <summary>
    /// Generates victory rewards (doc 218). Simplified prototype:
    /// normal battle = black cards + one random reward.
    /// </summary>
    public static class RewardGenerator
    {
        private static readonly OrbColor[] RewardColors = { OrbColor.Red, OrbColor.Yellow, OrbColor.Blue };

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
                {
                    OrbColor color = RewardColors[rng.Next(RewardColors.Length)];
                    rewards.Add(new RewardEntry(RewardType.Orb, 1, "信号球", color));
                    break;
                }
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
