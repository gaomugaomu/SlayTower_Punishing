using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class ConstructSkillTests
    {
        private static ConstructData CreateLucia()
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity("lucia", "露西亚");
            data.AssignCombatStats(ConstructType.Attack, 6, 1, 100, 40);
            data.AssignInitialEnergy(0);
            data.AssignSkills(SkillAssetFactory.LuciaRed(), SkillAssetFactory.LuciaBlue(), SkillAssetFactory.LuciaYellow());
            data.AssignCorePassive(CorePassiveType.ThreeMatchNextRedBonus, 6);
#endif
            return data;
        }

        private static ConstructData CreateLee()
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity("lee", "里");
            data.AssignCombatStats(ConstructType.Attack, 5, 1, 100, 35);
            data.AssignInitialEnergy(0);
            data.AssignSkills(SkillAssetFactory.LeeRed(), SkillAssetFactory.LeeBlue(), SkillAssetFactory.LeeYellow());
#endif
            return data;
        }

        private static ConstructData CreateLiv()
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity("liv", "丽芙");
            data.AssignCombatStats(ConstructType.Support, 3, 1, 100, 25);
            data.AssignInitialEnergy(0);
            data.AssignSkills(SkillAssetFactory.LivRed(), SkillAssetFactory.LivBlue(), SkillAssetFactory.LivYellow());
#endif
            return data;
        }

        private static BattleContext CreateBattle(out EnemyState enemy)
        {
            var commander = new CommanderState();
            enemy = new EnemyState("enemy_1", "Unit", 50);
            var battle = new BattleContext { Commander = commander };
            battle.AddEnemy(enemy);
            return battle;
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        [Test]
        public void Lucia_RedOneOrb_DealsFourDamage()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            string result = ConstructSkillExecutor.Execute(actor, actor.Data.RedSkill, 1, battle, null);

            Assert.AreEqual(46, enemy.Hp);
            StringAssert.Contains("-4 HP", result);
        }

        [Test]
        public void Lucia_RedThreeOrb_DealsEighteenAndChargesCoreMark()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            string result = ConstructSkillExecutor.Execute(actor, actor.Data.RedSkill, 3, battle, null);

            Assert.AreEqual(32, enemy.Hp);
            Assert.IsTrue(actor.ThreeMatchBonusPending);
        }

        [Test]
        public void Lucia_BlueThreeOrb_GainsSixEnergy()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            ConstructSkillExecutor.Execute(actor, actor.Data.BlueSkill, 3, battle, null);

            Assert.AreEqual(6, actor.Energy);
        }

        [Test]
        public void Lucia_YellowThreeOrb_GainsTenShield()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            ConstructSkillExecutor.Execute(actor, actor.Data.YellowSkill, 3, battle, null);

            Assert.AreEqual(10, battle.Commander.Shield);
        }

        [Test]
        public void Lee_BlueOneOrb_DrawsOneOrb()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLee());
            var pool = new OrbPool(new[]
            {
                ScriptableObject.CreateInstance<OrbData>(),
                ScriptableObject.CreateInstance<OrbData>()
            });

            string result = ConstructSkillExecutor.Execute(actor, actor.Data.BlueSkill, 1, battle, pool);

            Assert.AreEqual(1, pool.HandCount);
            StringAssert.Contains("draw 1 orbs", result);
        }

        [Test]
        public void Liv_BlueTwoOrb_HealsSix()
        {
            var battle = CreateBattle(out EnemyState enemy);
            battle.Commander.TakeDamage(30);
            var actor = new ConstructState(CreateLiv());

            ConstructSkillExecutor.Execute(actor, actor.Data.BlueSkill, 2, battle, null);

            Assert.AreEqual(76, battle.Commander.Hp);
        }

        [Test]
        public void Lucia_RedThreeOrb_ThenRedOneOrb_CoreMarkBonusDamage()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            // Three match: 18 damage + core mark charged.
            ConstructSkillExecutor.Execute(actor, actor.Data.RedSkill, 3, battle, null);
            Assert.AreEqual(32, enemy.Hp);

            // Next red orb activation receives bonus damage (4 + 6 = 10).
            ConstructSkillExecutor.Execute(actor, actor.Data.RedSkill, 1, battle, null);

            Assert.AreEqual(22, enemy.Hp);
            Assert.IsFalse(actor.ThreeMatchBonusPending);
        }

        [Test]
        public void OrbSkillController_PlayGroup_SpendsApAndDiscardsOrbs()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            var pool = new OrbPool(new[] { ScriptableObject.CreateInstance<OrbData>() });
            var drawn = pool.Draw(1)[0];

            // Build a 1-orb play group by hand.
            var group = new OrbPlayGroup((int)OrbColor.Red,
                new System.Collections.Generic.List<PunishingTower.Core.ISignalOrb> { drawn });

            var ap = new ActionPointSystem();
            ap.BeginTurn();
            var controller = new OrbSkillController(pool);

            string result = controller.TryPlayGroup(group, actor, battle, ap);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, ap.ActionPoints);
            Assert.AreEqual(0, pool.HandCount);
            Assert.AreEqual(1, pool.DiscardCount);
        }

        [Test]
        public void OrbSkillController_ThreeMatch_PublishesEvent()
        {
            var battle = CreateBattle(out EnemyState enemy);
            var actor = new ConstructState(CreateLucia());

            var pool = new OrbPool(new[]
            {
                CreateOrb(OrbColor.Red), CreateOrb(OrbColor.Red), CreateOrb(OrbColor.Red)
            });
            var drawn = pool.Draw(3);
            var group = new OrbPlayGroup((int)OrbColor.Red,
                new System.Collections.Generic.List<PunishingTower.Core.ISignalOrb>(drawn));

            int events = 0;
            EventBus.Subscribe<ThreeMatchEvent>(e => events = e.Count);

            var ap = new ActionPointSystem();
            ap.BeginTurn();
            var controller = new OrbSkillController(pool);
            controller.TryPlayGroup(group, actor, battle, ap);

            Assert.AreEqual(3, events);
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
    }
}
