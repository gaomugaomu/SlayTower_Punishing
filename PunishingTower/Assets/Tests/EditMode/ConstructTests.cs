using System.Linq;
using NUnit.Framework;
using PunishingTower.Construct;
using PunishingTower.Data;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class ConstructTests
    {
        private static ConstructData CreateConstruct(string id, int attack, int energyGain, int maxEnergy, int ultDamage)
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, id);
            data.AssignCombatStats(ConstructType.Attack, attack, energyGain, maxEnergy, ultDamage);
#endif
            return data;
        }

        [Test]
        public void Construct_InitialState_UsesConfiguredInitialEnergy()
        {
            var construct = new ConstructState(CreateConstruct("lucia", 6, 1, 100, 40));

            Assert.AreEqual(100, construct.Energy);
            Assert.AreEqual(ConstructStateFlag.Active, construct.Flag);
            Assert.IsTrue(construct.IsActive);
            Assert.IsTrue(construct.IsEnergyFull);
        }

        [Test]
        public void Construct_InitialState_ZeroEnergy_WhenConfigured()
        {
            var data = CreateConstruct("lucia", 6, 1, 100, 40);
#if UNITY_EDITOR
            data.AssignInitialEnergy(0);
#endif
            var construct = new ConstructState(data);

            Assert.AreEqual(0, construct.Energy);
            Assert.IsFalse(construct.IsEnergyFull);
        }

        [Test]
        public void Construct_AddEnergy_IncreasesAndCaps()
        {
            var data = CreateConstruct("lucia", 6, 1, 100, 40);
#if UNITY_EDITOR
            data.AssignInitialEnergy(0);
#endif
            var construct = new ConstructState(data);

            construct.AddEnergy(30);
            construct.AddEnergy(90);

            Assert.AreEqual(100, construct.Energy);
            Assert.IsTrue(construct.IsEnergyFull);
        }

        [Test]
        public void Construct_Ultimate_ConsumesFullEnergy()
        {
            var data = CreateConstruct("lucia", 6, 1, 100, 40);
#if UNITY_EDITOR
            data.AssignInitialEnergy(0);
#endif
            var construct = new ConstructState(data);
            construct.AddEnergy(100);

            bool ok = construct.TryConsumeUltimateEnergy();

            Assert.IsTrue(ok);
            Assert.AreEqual(0, construct.Energy);
        }

        [Test]
        public void Construct_Ultimate_NotFull_ReturnsFalse()
        {
            var data = CreateConstruct("lucia", 6, 1, 100, 40);
#if UNITY_EDITOR
            data.AssignInitialEnergy(0);
#endif
            var construct = new ConstructState(data);
            construct.AddEnergy(50);

            bool ok = construct.TryConsumeUltimateEnergy();

            Assert.IsFalse(ok);
            Assert.AreEqual(50, construct.Energy);
        }

        [Test]
        public void Construct_Stats_FromData()
        {
            var construct = new ConstructState(CreateConstruct("lucia", 6, 1, 100, 40));

            Assert.AreEqual(6, construct.BasicAttackDamage);
            Assert.AreEqual(1, construct.BasicAttackEnergyGain);
            Assert.AreEqual(40, construct.UltimateDamage);
        }

        [Test]
        public void Construct_SetFlag_Unavailable()
        {
            var construct = new ConstructState(CreateConstruct("lucia", 6, 1, 100, 40));

            construct.SetFlag(ConstructStateFlag.Unavailable);

            Assert.IsFalse(construct.IsActive);
        }
    }

    public class SquadRuntimeTests
    {
        private static ConstructData CreateConstruct(string id)
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, id);
            data.AssignCombatStats(ConstructType.Attack, 6, 1, 100, 40);
#endif
            return data;
        }

        private static SquadRuntime CreateGreyRaven()
        {
            return new SquadRuntime(new[]
            {
                CreateConstruct("lucia"),
                CreateConstruct("lee"),
                CreateConstruct("liv")
            });
        }

        [Test]
        public void Squad_InitialState_FirstMemberSelected()
        {
            var squad = CreateGreyRaven();

            Assert.AreEqual(3, squad.Count);
            Assert.AreEqual("lucia", squad.Current.Id);
        }

        [Test]
        public void Squad_SelectNext_Cycles()
        {
            var squad = CreateGreyRaven();

            squad.SelectNext();
            Assert.AreEqual("lee", squad.Current.Id);
            squad.SelectNext();
            Assert.AreEqual("liv", squad.Current.Id);
            squad.SelectNext();
            Assert.AreEqual("lucia", squad.Current.Id);
        }

        [Test]
        public void Squad_SelectPrevious_Cycles()
        {
            var squad = CreateGreyRaven();

            squad.SelectPrevious();
            Assert.AreEqual("liv", squad.Current.Id);
        }

        [Test]
        public void Squad_SelectNext_SkipsUnavailable()
        {
            var squad = CreateGreyRaven();
            squad.Members[1].SetFlag(ConstructStateFlag.Unavailable);

            squad.SelectNext();

            Assert.AreEqual("liv", squad.Current.Id);
        }

        [Test]
        public void Squad_AddMember_DynamicExpansion()
        {
            var squad = CreateGreyRaven();

            var newMember = squad.AddMember(CreateConstruct("kami"));

            Assert.AreEqual(4, squad.Count);
            Assert.IsTrue(squad.Members.Contains(newMember));
        }

        [Test]
        public void Squad_RemoveMember_Shrinks()
        {
            var squad = CreateGreyRaven();

            bool removed = squad.RemoveMember(squad.Members[1]);

            Assert.IsTrue(removed);
            Assert.AreEqual(2, squad.Count);
            Assert.AreEqual("lucia", squad.Current.Id);
        }

        [Test]
        public void Squad_ActiveCount_ExcludesUnavailable()
        {
            var squad = CreateGreyRaven();
            squad.Members[2].SetFlag(ConstructStateFlag.Unavailable);

            Assert.AreEqual(2, squad.ActiveCount);
        }

        [Test]
        public void Squad_SelectAt_SelectsIndex()
        {
            var squad = CreateGreyRaven();

            squad.SelectAt(2);

            Assert.AreEqual("liv", squad.Current.Id);
        }

        [Test]
        public void Squad_SelectAt_UnavailableIgnored()
        {
            var squad = CreateGreyRaven();
            squad.Members[1].SetFlag(ConstructStateFlag.Unavailable);

            squad.SelectAt(1);

            Assert.AreEqual("lucia", squad.Current.Id);
        }
    }
}
