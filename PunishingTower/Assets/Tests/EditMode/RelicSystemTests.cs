using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.Relic;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class RelicSystemTests
    {
        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        private static ConstructData CreateConstruct(int initialEnergy)
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity("lucia", "露西亚");
            data.AssignCombatStats(ConstructType.Attack, 6, 1, 100, 40);
            data.AssignInitialEnergy(initialEnergy);
            data.AssignSkills(SkillAssetFactory.LuciaRed(), SkillAssetFactory.LuciaBlue(), SkillAssetFactory.LuciaYellow());
#endif
            return data;
        }

        [Test]
        public void GreyRavenBadge_ThreeMatch_GrantsOneEnergy()
        {
            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };

            var relics = new RelicSystem();
            relics.Initialize(squad, battle);
            relics.EquipRelic(squad.Current, RelicAssetFactory.GreyRavenBadge());

            // Emitting a three match should grant +1 energy to the current construct.
            EventBus.Publish(new ThreeMatchEvent((int)OrbColor.Red, 3));

            Assert.AreEqual(1, squad.Current.Energy);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_NoRelics_ThreeMatchGrantsNoEnergy()
        {
            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };

            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            EventBus.Publish(new ThreeMatchEvent((int)OrbColor.Red, 3));

            Assert.AreEqual(0, squad.Current.Energy);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_TurnStartRelic_AppliesEffect()
        {
            var relic = ScriptableObject.CreateInstance<RelicData>();
#if UNITY_EDITOR
            relic.AssignIdentity("relic_turn", "回合遗物");
            relic.AssignRelic(RelicTrigger.TurnStart, EffectType.Shield, 3, "每回合开始+3盾");
#endif

            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };

            var relics = new RelicSystem();
            relics.Initialize(squad, battle);
            relics.EquipRelic(squad.Current, relic);

            EventBus.Publish(new TurnStartEvent(1));

            Assert.AreEqual(3, battle.Commander.Shield);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_Equip_SetsOwnerSlot()
        {
            var squad = new SquadRuntime(new[]
            {
                CreateConstruct(0), CreateConstruct(0), CreateConstruct(0)
            });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            bool ok = relics.EquipRelic(squad.Members[1], badge);

            Assert.IsTrue(ok);
            Assert.IsTrue(squad.Members[1].HasEquippedRelic(badge));
            Assert.AreEqual(0, squad.Members[0].EquippedRelics.Count);
            Assert.AreSame(squad.Members[1], relics.GetRelicOwner(badge));

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_SwitchOwner_MovesRelic()
        {
            var squad = new SquadRuntime(new[]
            {
                CreateConstruct(0), CreateConstruct(0), CreateConstruct(0)
            });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            relics.EquipRelic(squad.Members[0], badge);
            bool switched = relics.SwitchRelicOwner(badge, squad.Members[2]);

            Assert.IsTrue(switched);
            Assert.AreEqual(0, squad.Members[0].EquippedRelics.Count);
            Assert.IsTrue(squad.Members[2].HasEquippedRelic(badge));

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_Unequip_RemovesFromOwner()
        {
            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            relics.EquipRelic(squad.Current, badge);
            bool removed = relics.UnequipRelic(badge);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, squad.Current.EquippedRelics.Count);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_MultipleRelics_OnSameConstruct()
        {
            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            var second = ScriptableObject.CreateInstance<RelicData>();
#if UNITY_EDITOR
            second.AssignIdentity("relic_2", "第二遗物");
            second.AssignRelic(RelicTrigger.TurnStart, EffectType.Shield, 3, "回合开始+3盾");
#endif

            relics.EquipRelic(squad.Current, badge);
            relics.EquipRelic(squad.Current, second);

            Assert.AreEqual(2, squad.Current.EquippedRelics.Count);
            Assert.AreEqual(2, relics.GetActiveRelics().Count);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_OwnerLeftSquad_RelicNotActive()
        {
            var squad = new SquadRuntime(new[]
            {
                CreateConstruct(0), CreateConstruct(0)
            });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            relics.EquipRelic(squad.Members[0], badge);
            Assert.AreEqual(1, relics.GetActiveRelics().Count);

            // Construct leaves the squad (Unavailable): its relics stop applying.
            squad.Members[0].SetFlag(ConstructStateFlag.Unavailable);

            Assert.AreEqual(0, relics.GetActiveRelics().Count);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_OwnerReturns_SquadRelicActiveAgain()
        {
            var squad = new SquadRuntime(new[] { CreateConstruct(0) });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            relics.EquipRelic(squad.Current, badge);

            squad.Current.SetFlag(ConstructStateFlag.Unavailable);
            Assert.AreEqual(0, relics.GetActiveRelics().Count);

            squad.Current.SetFlag(ConstructStateFlag.Active);
            Assert.AreEqual(1, relics.GetActiveRelics().Count);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_RelicOnlyOnOneOwner_AtATime()
        {
            var squad = new SquadRuntime(new[]
            {
                CreateConstruct(0), CreateConstruct(0), CreateConstruct(0)
            });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            relics.EquipRelic(squad.Members[0], badge);
            relics.EquipRelic(squad.Members[1], badge);

            Assert.AreSame(squad.Members[1], relics.GetRelicOwner(badge));
            Assert.AreEqual(0, squad.Members[0].EquippedRelics.Count);
            Assert.AreEqual(1, squad.Members[1].EquippedRelics.Count);

            relics.Shutdown();
        }

        [Test]
        public void RelicSystem_SelectedRelic_EquipsToChosenConstruct()
        {
            // End-to-end: select a relic, then equip it to a specific construct.
            var squad = new SquadRuntime(new[]
            {
                CreateConstruct(0), CreateConstruct(0), CreateConstruct(0)
            });
            var battle = new BattleContext { Commander = new CommanderState() };
            var relics = new RelicSystem();
            relics.Initialize(squad, battle);

            RelicData badge = RelicAssetFactory.GreyRavenBadge();
            RelicData core = ScriptableObject.CreateInstance<RelicData>();
#if UNITY_EDITOR
            core.AssignIdentity("core", "防御核心");
            core.AssignRelic(RelicTrigger.TurnStart, EffectType.Shield, 3, "回合盾", false);
#endif

            relics.EquipRelic(squad.Members[0], badge);
            relics.EquipRelic(squad.Members[2], core);

            // Verify owners.
            Assert.AreSame(squad.Members[0], relics.GetRelicOwner(badge));
            Assert.AreSame(squad.Members[2], relics.GetRelicOwner(core));

            relics.Shutdown();
        }
    }

    public class RewardTests
    {
        [Test]
        public void GenerateNormalBattle_AlwaysHasBlackCards()
        {
            var rewards = RewardGenerator.GenerateNormalBattle(new System.Random(42));

            Assert.IsTrue(rewards.Exists(r => r.type == RewardType.BlackCard));
            Assert.AreEqual(10, rewards.Find(r => r.type == RewardType.BlackCard).amount);
        }

        [Test]
        public void GenerateNormalBattle_HasAtLeastOneExtraReward()
        {
            var rewards = RewardGenerator.GenerateNormalBattle(new System.Random(42));

            Assert.GreaterOrEqual(rewards.Count, 2);
        }
    }
}
