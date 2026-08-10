using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Enemy
{
    /// <summary>
    /// Boss rule modifier (doc 155 / 208): Corrupted Prototype corrupts the orb pool.
    /// Every N rounds a corrupted (locked) orb is injected into the draw pile.
    /// Corrupted orbs cannot participate in three match. Boss mechanics must use events.
    /// </summary>
    public class CorruptionModifier
    {
        private readonly OrbPool orbPool;
        private readonly int intervalRounds;
        private int roundCounter;
        private int corruptedCount;

        public int CorruptedCount => corruptedCount;
        public int IntervalRounds => intervalRounds;
        public int RoundCounter => roundCounter;

        public CorruptionModifier(OrbPool orbPool, int intervalRounds = 3)
        {
            this.orbPool = orbPool;
            this.intervalRounds = intervalRounds;
            EventBus.Subscribe<TurnEndEvent>(OnTurnEnd);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<TurnEndEvent>(OnTurnEnd);
        }

        /// <summary>Injects one corrupted orb immediately (used to seed or for tests).</summary>
        public void InjectCorruptedOrb()
        {
            if (orbPool == null)
            {
                return;
            }

            OrbData corrupted = CreateCorruptedOrbData();
            OrbInstance instance = orbPool.AddOrb(corrupted);
            if (instance != null)
            {
                instance.Locked = true;
                corruptedCount++;
            }
        }

        private void OnTurnEnd(TurnEndEvent evt)
        {
            roundCounter++;
            if (roundCounter % intervalRounds == 0)
            {
                InjectCorruptedOrb();
                Debug.Log($"[Boss] 第 {evt.Round} 回合结束: 腐化原型体注入 1 颗腐化球(锁球) -> 球池 {orbPool.TotalCount} 球");
            }
        }

        private static OrbData CreateCorruptedOrbData()
        {
            var data = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            data.AssignIdentity("corrupted_" + System.DateTime.UtcNow.Ticks, "腐化球");
            data.AssignColor(OrbColor.White);
#endif
            return data;
        }
    }
}
