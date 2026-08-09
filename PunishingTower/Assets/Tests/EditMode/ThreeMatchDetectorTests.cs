using System.Collections.Generic;
using NUnit.Framework;
using PunishingTower.Core;
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

        private static OrbInstance Locked(OrbData data)
        {
            return new OrbInstance(data) { Locked = true };
        }

        [Test]
        public void ResolvePlay_MiddleOfTriple_ReturnsThree()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            OrbPlayGroup group = ThreeMatchDetector.ResolvePlay(row, 1);

            Assert.IsNotNull(group);
            Assert.AreEqual(3, group.MatchCount);
            Assert.AreEqual((int)OrbColor.Red, group.Color);
        }

        [Test]
        public void ResolvePlay_AnyOrbOfTriple_EliminatesAllThree()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 0).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 1).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 2).MatchCount);
        }

        [Test]
        public void ResolvePlay_IsolatedOrb_IsSingle()
        {
            var row = Orbs(
                CreateOrb("r", OrbColor.Red),
                CreateOrb("y", OrbColor.Yellow),
                CreateOrb("b", OrbColor.Blue));

            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 0).MatchCount);
            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 1).MatchCount);
            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 2).MatchCount);
        }

        [Test]
        public void ResolvePlay_Pair_EliminatesBoth()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("y", OrbColor.Yellow));

            OrbPlayGroup group = ThreeMatchDetector.ResolvePlay(row, 0);

            Assert.AreEqual(2, group.MatchCount);
        }

        [Test]
        public void ResolvePlay_FourSame_RightmostThreeAreTriple_LeftmostIsSingle()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red),
                CreateOrb("r4", OrbColor.Red));

            // row is indexed left(0) to right(3). Groups right-to-left: [1,2,3] then [0].
            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 0).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 1).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 2).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 3).MatchCount);
        }

        [Test]
        public void ResolvePlay_FiveSame_SplitsIntoThreeAndTwo()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red),
                CreateOrb("r4", OrbColor.Red),
                CreateOrb("r5", OrbColor.Red));

            // Groups right-to-left: [2,3,4] and [0,1].
            Assert.AreEqual(2, ThreeMatchDetector.ResolvePlay(row, 0).MatchCount);
            Assert.AreEqual(2, ThreeMatchDetector.ResolvePlay(row, 1).MatchCount);
            Assert.AreEqual(3, ThreeMatchDetector.ResolvePlay(row, 2).MatchCount);
        }

        [Test]
        public void ResolvePlay_NonConsecutiveOrbs_AreIsolated()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("y", OrbColor.Yellow),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("r3", OrbColor.Red));

            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 0).MatchCount);
            Assert.AreEqual(1, ThreeMatchDetector.ResolvePlay(row, 1).MatchCount);
            Assert.AreEqual(2, ThreeMatchDetector.ResolvePlay(row, 2).MatchCount);
            Assert.AreEqual(2, ThreeMatchDetector.ResolvePlay(row, 3).MatchCount);
        }

        [Test]
        public void ResolvePlay_LockedOrb_CannotBePlayed()
        {
            var row = new List<ISignalOrb>
            {
                new OrbInstance(CreateOrb("r1", OrbColor.Red)),
                Locked(CreateOrb("locked", OrbColor.Red)),
                new OrbInstance(CreateOrb("r3", OrbColor.Red))
            };

            Assert.IsNull(ThreeMatchDetector.ResolvePlay(row, 1));
        }

        [Test]
        public void ResolvePlay_SpecialOrb_CannotBePlayed()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("w", OrbColor.White),
                CreateOrb("r2", OrbColor.Red));

            Assert.IsNull(ThreeMatchDetector.ResolvePlay(row, 1));
        }

        [Test]
        public void GetAllGroups_SplitsRowIntoPlayableGroups()
        {
            var row = Orbs(
                CreateOrb("r1", OrbColor.Red),
                CreateOrb("r2", OrbColor.Red),
                CreateOrb("y1", OrbColor.Yellow),
                CreateOrb("y2", OrbColor.Yellow),
                CreateOrb("b1", OrbColor.Blue));

            var groups = ThreeMatchDetector.GetAllGroups(row);

            Assert.AreEqual(3, groups.Count);
            Assert.AreEqual(2, groups[0].MatchCount);
            Assert.AreEqual((int)OrbColor.Red, groups[0].Color);
            Assert.AreEqual(2, groups[1].MatchCount);
            Assert.AreEqual((int)OrbColor.Yellow, groups[1].Color);
            Assert.AreEqual(1, groups[2].MatchCount);
            Assert.AreEqual((int)OrbColor.Blue, groups[2].Color);
        }

        [Test]
        public void GetAllGroups_LockedOrb_IsItsOwnGroup()
        {
            var row = new List<ISignalOrb>
            {
                new OrbInstance(CreateOrb("r1", OrbColor.Red)),
                new OrbInstance(CreateOrb("r2", OrbColor.Red)),
                Locked(CreateOrb("locked", OrbColor.Red)),
                new OrbInstance(CreateOrb("r3", OrbColor.Red))
            };

            var groups = ThreeMatchDetector.GetAllGroups(row);

            Assert.AreEqual(3, groups.Count);
        }
    }
}
