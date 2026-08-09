using NUnit.Framework;
using PunishingTower.Combat;

namespace PunishingTower.Tests
{
    public class EnemyTests
    {
        [Test]
        public void Enemy_InitialState_Defaults()
        {
            var enemy = new EnemyState("enemy_1", "Infected Unit", 50);

            Assert.AreEqual(50, enemy.MaxHp);
            Assert.AreEqual(50, enemy.Hp);
            Assert.AreEqual(0, enemy.Shield);
            Assert.IsFalse(enemy.IsDefeated);
        }

        [Test]
        public void Enemy_TakesDamage_ReducesHp()
        {
            var enemy = new EnemyState("enemy_1", "Infected Unit", 50);

            enemy.TakeDamage(20);

            Assert.AreEqual(30, enemy.Hp);
        }

        [Test]
        public void Enemy_Shield_AbsorbsDamage()
        {
            var enemy = new EnemyState("enemy_1", "Infected Unit", 50);
            enemy.AddShield(10);

            enemy.TakeDamage(6);

            Assert.AreEqual(50, enemy.Hp);
            Assert.AreEqual(4, enemy.Shield);
        }

        [Test]
        public void Enemy_DefeatedWhenHpZero()
        {
            var enemy = new EnemyState("enemy_1", "Infected Unit", 50);

            enemy.TakeDamage(50);

            Assert.AreEqual(0, enemy.Hp);
            Assert.IsTrue(enemy.IsDefeated);
        }

        [Test]
        public void Enemy_AddsShield_OnDefense()
        {
            var enemy = new EnemyState("enemy_1", "Infected Unit", 50);

            enemy.AddShield(8);

            Assert.AreEqual(8, enemy.Shield);
        }
    }
}
