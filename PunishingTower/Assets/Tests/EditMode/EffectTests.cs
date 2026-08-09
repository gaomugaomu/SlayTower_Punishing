using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.Core;

namespace PunishingTower.Tests
{
    public class EffectTests
    {
        private static BattleEffectContext Ctx(CommanderState commander, EnemyState enemy = null)
        {
            return new BattleEffectContext
            {
                Commander = commander,
                TargetEnemy = enemy,
                Amount = 0
            };
        }

        [Test]
        public void DamageEffect_OnEnemy_ReducesEnemyHp()
        {
            var commander = new CommanderState();
            var enemy = new EnemyState("enemy_1", "Unit", 50);
            var ctx = Ctx(commander, enemy);

            new DamageEffect(20).Apply(ctx);

            Assert.AreEqual(30, enemy.Hp);
            Assert.AreEqual(100, commander.Hp);
        }

        [Test]
        public void DamageEffect_OnCommander_ReducesCommanderHp()
        {
            var commander = new CommanderState();
            var ctx = Ctx(commander, null);

            new DamageEffect(15).Apply(ctx);

            Assert.AreEqual(85, commander.Hp);
        }

        [Test]
        public void HealEffect_IncreasesCommanderHp()
        {
            var commander = new CommanderState();
            commander.TakeDamage(30);
            var ctx = Ctx(commander);

            new HealEffect(10).Apply(ctx);

            Assert.AreEqual(80, commander.Hp);
        }

        [Test]
        public void ShieldEffect_AddsCommanderShield()
        {
            var commander = new CommanderState();
            var ctx = Ctx(commander);

            new ShieldEffect(12).Apply(ctx);

            Assert.AreEqual(12, commander.Shield);
        }

        [Test]
        public void InfectionEffect_IncreasesInfection()
        {
            var commander = new CommanderState();
            var ctx = Ctx(commander);

            new InfectionEffect(25).Apply(ctx);

            Assert.AreEqual(25, commander.Infection);
        }

        [Test]
        public void EffectResolver_AppliesMultipleEffectsInOrder()
        {
            var commander = new CommanderState();
            var enemy = new EnemyState("enemy_1", "Unit", 50);
            var ctx = Ctx(commander, enemy);

            EffectResolver.Resolve(new IEffect[]
            {
                new DamageEffect(10),
                new ShieldEffect(5),
                new HealEffect(3)
            }, ctx);

            Assert.AreEqual(40, enemy.Hp);
            Assert.AreEqual(5, commander.Shield);
            Assert.AreEqual(100, commander.Hp);
        }
    }
}
