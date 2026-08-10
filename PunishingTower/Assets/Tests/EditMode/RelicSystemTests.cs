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
            relics.AddRelic(RelicAssetFactory.GreyRavenBadge());

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
            relics.AddRelic(relic);

            EventBus.Publish(new TurnStartEvent(1));

            Assert.AreEqual(3, battle.Commander.Shield);

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
