using System.Collections.Generic;
using NUnit.Framework;
using PunishingTower.Core;
using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class ThreeMatchDetectorTests
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

        private static List<ISignalOrb> Orbs(params OrbData[] datas)
        {
            var list = new List<ISignalOrb>();
            foreach (OrbData d in datas)
            {
                list.Add(new OrbInstance(d));
            }
            return list;
        }

        [Test]
        public void ConsecutiveTriple_DetectsGroup()
        {
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual((int)OrbColor.Red, groups[0].Color);
            Assert.AreEqual(3, groups[0].Count);
        }

        [Test]
        public void NoMatch_ReturnsEmpty()
        {
            var orbs = Orbs(
                CreateOrb("r", OrbColor.Red),
                CreateOrb("y", OrbColor.Yellow),
                CreateOrb("b", OrbColor.Blue));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void NonConsecutive_NotMatched()
        {
            // R Y R R - two reds at the end are not enough, early red is separated.
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("y", OrbColor.Yellow),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void LongerChain_CountsAll()
        {
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red),
                CreateOrb("r4", OrbColor.Red),
                CreateOrb("r5", OrbColor.Red));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(5, groups[0].Count);
        }

        [Test]
        public void LockedOrb_BreaksChain()
        {
            var locked = new OrbInstance(CreateOrb("locked", OrbColor.Red));
            locked.Locked = true;

            var orbs = new List<ISignalOrb>
            {
                new OrbInstance(CreateOrb("r1", OrbColor.Red)),
                new OrbInstance(CreateOrb("r2", OrbColor.Red)),
                locked,
                new OrbInstance(CreateOrb("r3", OrbColor.Red)),
                new OrbInstance(CreateOrb("r4", OrbColor.Red))
            };

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void SpecialOrb_BreaksChain()
        {
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("w", OrbColor.White),
                CreateOrb("r3", OrbColor.Red));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void TwoMatches_DetectsBoth()
        {
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red),
                CreateOrb("y1", OrbColor.Yellow),
                CreateOrb("y2", OrbColor.Yellow),
                CreateOrb("y3", OrbColor.Yellow));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual((int)OrbColor.Red, groups[0].Color);
            Assert.AreEqual((int)OrbColor.Yellow, groups[1].Color);
        }

        [Test]
        public void DocExample_RRYBRB_TriggersOnlyRed()
        {
            // Input: R R Y Y R B B -> YY is only 2 so no match; the resulting R R R triggers.
            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("y1", OrbColor.Yellow),
                CreateOrb("y2", OrbColor.Yellow),
                CreateOrb("r3", OrbColor.Red),
                CreateOrb("b1", OrbColor.Blue),
                CreateOrb("b2", OrbColor.Blue));

            var groups = ThreeMatchDetector.Detect(orbs);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual((int)OrbColor.Red, groups[0].Color);
            Assert.AreEqual(3, groups[0].Count);
        }

        [Test]
        public void PublishMatches_RaisesThreeMatchEvent()
        {
            EventBus.Clear();
            int events = 0;
            int lastColor = -1;
            int lastCount = 0;
            EventBus.Subscribe<ThreeMatchEvent>(e => { events++; lastColor = e.Color; lastCount = e.Count; });

            var orbs = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            ThreeMatchDetector.PublishMatches(orbs);

            Assert.AreEqual(1, events);
            Assert.AreEqual((int)OrbColor.Red, lastColor);
            Assert.AreEqual(3, lastCount);
            EventBus.Clear();
        }
    }
}
