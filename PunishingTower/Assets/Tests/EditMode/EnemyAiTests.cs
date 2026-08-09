using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.Enemy;

namespace PunishingTower.Tests
{
    public class EnemyAiTests
    {
        [Test]
        public void EnemyAI_FirstTurn_DecidesAttack()
        {
            var ai = new EnemyAiController(seed: 1);
            var enemy = new EnemyState("enemy_1", "Unit", 50);

            EnemyIntent intent = ai.DecideIntent(enemy, 10, 8);

            Assert.AreEqual(IntentType.Attack, intent.Type);
            Assert.AreEqual(10, intent.Amount);
        }

        [Test]
        public void EnemyAI_SecondTurn_DecidesDefense()
        {
            var ai = new EnemyAiController(seed: 1);
            var enemy = new EnemyState("enemy_1", "Unit", 50);

            ai.DecideIntent(enemy, 10, 8);
            EnemyIntent intent = ai.DecideIntent(enemy, 10, 8);

            Assert.AreEqual(IntentType.Defense, intent.Type);
            Assert.AreEqual(8, intent.Amount);
        }

        [Test]
        public void EnemyIntent_PreservesData()
        {
            var intent = EnemyIntent.Attack(12);

            Assert.AreEqual(IntentType.Attack, intent.Type);
            Assert.AreEqual(12, intent.Amount);
            Assert.AreEqual("Attack 12", intent.Description);
        }

        [Test]
        public void ExecuteIntent_Attack_DamagesCommander()
        {
            var commander = new CommanderState();
            var enemy = new EnemyState("enemy_1", "Unit", 50);
            var intent = EnemyIntent.Attack(10);

            string result = EnemyAiController.ExecuteIntent(enemy, intent, commander);

            Assert.AreEqual(90, commander.Hp);
            StringAssert.Contains("10 damage", result);
        }

        [Test]
        public void ExecuteIntent_Defense_GrantsEnemyShield()
        {
            var commander = new CommanderState();
            var enemy = new EnemyState("enemy_1", "Unit", 50);
            var intent = EnemyIntent.Defense(8);

            string result = EnemyAiController.ExecuteIntent(enemy, intent, commander);

            Assert.AreEqual(8, enemy.Shield);
            StringAssert.Contains("8 shield", result);
        }
    }
}
