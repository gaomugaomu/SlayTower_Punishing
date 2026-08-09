using NUnit.Framework;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class OrbPoolTests
    {
        private static OrbData CreateOrb(string id, OrbColor color)
        {
            var data = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, id);
            data.AssignColor(color);
#endif
            return data;
        }

        private static OrbPool CreatePool(int red, int yellow, int blue)
        {
            var defs = new System.Collections.Generic.List<OrbData>();
            for (int i = 0; i < red; i++) { defs.Add(CreateOrb("red_" + i, OrbColor.Red)); }
            for (int i = 0; i < yellow; i++) { defs.Add(CreateOrb("yellow_" + i, OrbColor.Yellow)); }
            for (int i = 0; i < blue; i++) { defs.Add(CreateOrb("blue_" + i, OrbColor.Blue)); }
            return new OrbPool(defs);
        }

        [Test]
        public void Draw_ReturnsOrbAndRemovesFromPile()
        {
            var pool = CreatePool(3, 0, 0);

            OrbInstance orb = pool.Draw();

            Assert.IsNotNull(orb);
            Assert.AreEqual(2, pool.DrawCount);
            Assert.AreEqual(1, pool.HandCount);
            Assert.AreEqual(3, pool.TotalCount);
        }

        [Test]
        public void Draw_WhenEverythingEmpty_ReturnsNull()
        {
            var pool = CreatePool(0, 0, 0);

            Assert.IsNull(pool.Draw());
        }

        [Test]
        public void ShuffleDiscardIntoDraw_WhenDrawEmpty()
        {
            var pool = CreatePool(1, 0, 0);
            OrbInstance orb = pool.Draw();

            pool.DiscardFromHand(orb);
            Assert.AreEqual(0, pool.DrawCount);
            Assert.AreEqual(1, pool.DiscardCount);

            OrbInstance redrawn = pool.Draw();

            Assert.IsNotNull(redrawn);
            Assert.AreEqual(0, pool.DrawCount);
            Assert.AreEqual(0, pool.DiscardCount);
            Assert.AreEqual(1, pool.HandCount);
        }

        [Test]
        public void Discard_ReturnsOrbToDiscardPile()
        {
            var pool = CreatePool(2, 0, 0);
            pool.Draw(2);

            OrbInstance first = pool.Hand[0];
            pool.DiscardFromHand(first);

            Assert.AreEqual(1, pool.HandCount);
            Assert.AreEqual(1, pool.DiscardCount);
        }

        [Test]
        public void ExhaustOrb_GoesToExhaustPile()
        {
            var pool = CreatePool(2, 0, 0);
            pool.Draw(2);

            OrbInstance first = pool.Hand[0];
            pool.ExhaustFromHand(first);

            Assert.AreEqual(1, pool.HandCount);
            Assert.AreEqual(1, pool.ExhaustCount);
            Assert.AreEqual(2, pool.TotalCount);
        }

        [Test]
        public void PlayFromHand_WithExhaustFlag_GoesToExhaust()
        {
            var pool = CreatePool(1, 0, 0);
            OrbInstance orb = pool.Draw();
            orb.ExhaustOnUse = true;

            pool.PlayFromHand(orb);

            Assert.AreEqual(0, pool.HandCount);
            Assert.AreEqual(1, pool.ExhaustCount);
            Assert.AreEqual(0, pool.DiscardCount);
        }

        [Test]
        public void PlayFromHand_WithRetainFlag_StaysInHand()
        {
            var pool = CreatePool(1, 0, 0);
            OrbInstance orb = pool.Draw();
            orb.Retained = true;

            pool.PlayFromHand(orb);

            Assert.AreEqual(1, pool.HandCount);
            Assert.AreEqual(0, pool.DiscardCount);
        }

        [Test]
        public void PlayFromHand_NormalOrb_GoesToDiscardAndQueue()
        {
            var pool = CreatePool(1, 0, 0);
            OrbInstance orb = pool.Draw();

            pool.PlayFromHand(orb);

            Assert.AreEqual(0, pool.HandCount);
            Assert.AreEqual(1, pool.DiscardCount);
            Assert.AreEqual(1, pool.Queue.Count);
        }

        [Test]
        public void HandLimit_IsSixteen()
        {
            var pool = CreatePool(20, 0, 0);

            pool.Draw(16);

            Assert.IsTrue(pool.IsHandFull);
            Assert.AreEqual(16, pool.HandCount);
        }

        [Test]
        public void Draw_Multiple_ReturnsRequestedCount()
        {
            var pool = CreatePool(5, 0, 0);

            var drawn = pool.Draw(3);

            Assert.AreEqual(3, drawn.Count);
            Assert.AreEqual(3, pool.HandCount);
        }
    }
}
