using NUnit.Framework;
using PunishingTower.Combat;

namespace PunishingTower.Tests
{
    public class BattleContextTests
    {
        private static BattleContext CreateContext(params EnemyState[] enemies)
        {
            var ctx = new BattleContext { Commander = new CommanderState() };
            foreach (EnemyState enemy in enemies)
            {
                ctx.AddEnemy(enemy);
            }
            return ctx;
        }

        [Test]
        public void BattleContext_SelectedEnemy_DefaultsToFirst()
        {
            var ctx = CreateContext(
                new EnemyState("e1", "Unit A", 50),
                new EnemyState("e2", "Unit B", 60));

            Assert.AreEqual("e1", ctx.SelectedEnemy.Id);
        }

        [Test]
        public void BattleContext_SelectNext_Wraps()
        {
            var ctx = CreateContext(
                new EnemyState("e1", "Unit A", 50),
                new EnemyState("e2", "Unit B", 60),
                new EnemyState("e3", "Unit C", 70));

            ctx.SelectNextEnemy();
            Assert.AreEqual("e2", ctx.SelectedEnemy.Id);
            ctx.SelectNextEnemy();
            Assert.AreEqual("e3", ctx.SelectedEnemy.Id);
            ctx.SelectNextEnemy();
            Assert.AreEqual("e1", ctx.SelectedEnemy.Id);
        }

        [Test]
        public void BattleContext_SelectNext_SkipsDefeated()
        {
            var e2 = new EnemyState("e2", "Unit B", 60);
            e2.TakeDamage(100);
            var ctx = CreateContext(
                new EnemyState("e1", "Unit A", 50),
                e2,
                new EnemyState("e3", "Unit C", 70));

            ctx.SelectNextEnemy();

            Assert.AreEqual("e3", ctx.SelectedEnemy.Id);
        }

        [Test]
        public void BattleContext_AllEnemiesDefeated_OnlyWhenAllDead()
        {
            var e1 = new EnemyState("e1", "Unit A", 50);
            var e2 = new EnemyState("e2", "Unit B", 60);
            var ctx = CreateContext(e1, e2);

            e1.TakeDamage(100);
            Assert.IsFalse(ctx.AllEnemiesDefeated);

            e2.TakeDamage(100);
            Assert.IsTrue(ctx.AllEnemiesDefeated);
        }

        [Test]
        public void BattleContext_AliveEnemyCount()
        {
            var e1 = new EnemyState("e1", "Unit A", 50);
            var e2 = new EnemyState("e2", "Unit B", 60);
            var ctx = CreateContext(e1, e2);

            e1.TakeDamage(100);
            Assert.AreEqual(1, ctx.AliveEnemyCount);
        }

        [Test]
        public void BattleContext_RemoveEnemy_FixesSelection()
        {
            var ctx = CreateContext(
                new EnemyState("e1", "Unit A", 50),
                new EnemyState("e2", "Unit B", 60));

            ctx.SelectNextEnemy();
            ctx.RemoveEnemy(ctx.Enemies[1]);

            Assert.AreEqual(1, ctx.Enemies.Count);
            Assert.AreEqual("e1", ctx.SelectedEnemy.Id);
        }
    }
}
