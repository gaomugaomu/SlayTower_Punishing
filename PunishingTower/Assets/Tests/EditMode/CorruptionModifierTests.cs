using NUnit.Framework;
using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.Enemy;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class CorruptionModifierTests
    {
        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        private static OrbData CreateOrb(OrbColor color)
        {
            var data = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            data.AssignIdentity("orb_" + color, "orb");
            data.AssignColor(color);
#endif
            return data;
        }

        private static OrbPool CreatePool()
        {
            return new OrbPool(new[]
            {
                CreateOrb(OrbColor.Red), CreateOrb(OrbColor.Red), CreateOrb(OrbColor.Red),
                CreateOrb(OrbColor.Yellow), CreateOrb(OrbColor.Yellow), CreateOrb(OrbColor.Yellow),
                CreateOrb(OrbColor.Blue), CreateOrb(OrbColor.Blue), CreateOrb(OrbColor.Blue)
            });
        }

        [Test]
        public void CorruptionModifier_EveryThreeRounds_InjectsCorruptedOrb()
        {
            var pool = CreatePool();
            int before = pool.TotalCount;

            var modifier = new CorruptionModifier(pool, 3);

            // Rounds 1, 2: no corruption yet.
            EventBus.Publish(new TurnEndEvent(1));
            EventBus.Publish(new TurnEndEvent(2));
            Assert.AreEqual(before, pool.TotalCount);
            Assert.AreEqual(0, modifier.CorruptedCount);

            // Round 3: first corrupted orb appears.
            EventBus.Publish(new TurnEndEvent(3));
            Assert.AreEqual(before + 1, pool.TotalCount);
            Assert.AreEqual(1, modifier.CorruptedCount);

            // Round 6: second corrupted orb.
            EventBus.Publish(new TurnEndEvent(4));
            EventBus.Publish(new TurnEndEvent(5));
            EventBus.Publish(new TurnEndEvent(6));
            Assert.AreEqual(before + 2, pool.TotalCount);
            Assert.AreEqual(2, modifier.CorruptedCount);

            modifier.Shutdown();
        }

        [Test]
        public void CorruptionModifier_CorruptedOrb_IsLocked()
        {
            var pool = CreatePool();
            var modifier = new CorruptionModifier(pool, 1);

            modifier.InjectCorruptedOrb();

            bool found = false;
            foreach (OrbInstance orb in pool.DrawPile)
            {
                if (orb.Locked)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "corrupted orb should be locked in the draw pile");

            modifier.Shutdown();
        }

        [Test]
        public void CorruptionModifier_Draw_CorruptedOrbEntersHandAsLocked()
        {
            var pool = CreatePool();
            var modifier = new CorruptionModifier(pool, 1);

            modifier.InjectCorruptedOrb();

            OrbInstance orb = pool.Draw();
            // The injected orb may not be the one drawn; draw until we find it or the pile is empty.
            int guard = 0;
            while (orb != null && !orb.Locked && guard < 20)
            {
                orb = pool.Draw();
                guard++;
            }

            Assert.IsNotNull(orb);
            Assert.IsTrue(orb.Locked, "corrupted orb drawn into hand stays locked");

            modifier.Shutdown();
        }
    }
}
