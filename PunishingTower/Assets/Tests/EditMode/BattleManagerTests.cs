using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.Core;
using PunishingTower.Core.Events;

namespace PunishingTower.Tests
{
    public class BattleManagerTests
    {
        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        private static BattleContext CreateContext()
        {
            return new BattleContext
            {
                Commander = new CommanderState()
            };
        }

        private static EnemyState AddEnemy(BattleContext ctx, string id, int hp, int attack, int defense)
        {
            var enemy = new EnemyState(id, "Unit " + id, hp, attack, defense);
            ctx.AddEnemy(enemy);
            return enemy;
        }

        [Test]
        public void BattleManager_StartBattle_EntersIntentPhase()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);

            manager.StartBattle();

            Assert.AreEqual(BattlePhase.EnemyIntent, manager.GetBattleState().Phase);
            Assert.IsTrue(manager.GetBattleState().IsActive);
            Assert.IsFalse(manager.IsOver);
        }

        [Test]
        public void BattleManager_RevealIntents_ResetsActionPoints()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);
            manager.StartBattle();

            manager.RevealIntents();
            manager.BeginPlayerTurn();
            manager.ActionPoints.TrySpend(2);
            Assert.AreEqual(1, manager.ActionPoints.ActionPoints);

            manager.EndPlayerTurn();
            Assert.AreEqual(0, manager.ActionPoints.ActionPoints);

            manager.RevealIntents();
            Assert.AreEqual(3, manager.ActionPoints.ActionPoints);
        }

        [Test]
        public void BattleManager_KillAllEnemies_Victory()
        {
            var battle = CreateContext();
            var e1 = AddEnemy(battle, "e1", 50, 10, 8);
            var e2 = AddEnemy(battle, "e2", 60, 8, 10);
            var manager = new BattleManager(battle);
            manager.StartBattle();
            manager.RevealIntents();
            manager.BeginPlayerTurn();

            e1.TakeDamage(100);
            e2.TakeDamage(100);

            manager.EndPlayerTurn();

            Assert.IsTrue(manager.IsOver);
            Assert.AreEqual(BattlePhase.Victory, manager.GetBattleState().Phase);
        }

        [Test]
        public void BattleManager_CommanderDead_Defeat()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);
            manager.StartBattle();
            manager.RevealIntents();
            manager.BeginPlayerTurn();

            battle.Commander.TakeDamage(100);
            manager.EndPlayerTurn();

            Assert.IsTrue(manager.IsOver);
            Assert.AreEqual(BattlePhase.Defeat, manager.GetBattleState().Phase);
        }

        [Test]
        public void BattleManager_EnemyExecutor_RunsOnEndTurn()
        {
            var battle = CreateContext();
            var e1 = AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle, ctx =>
            {
                ctx.Commander.TakeDamage(10);
            });
            manager.StartBattle();
            manager.RevealIntents();
            manager.BeginPlayerTurn();

            manager.EndPlayerTurn();

            Assert.AreEqual(90, battle.Commander.Hp);
        }

        [Test]
        public void BattleManager_EndBattle_PublishesEvent()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);
            manager.StartBattle();

            BattleEndEvent received = null;
            EventBus.Subscribe<BattleEndEvent>(e => received = e);

            manager.EndBattle(true);

            Assert.IsNotNull(received);
            Assert.IsTrue(received.Victory);
            Assert.AreEqual(BattlePhase.Victory, manager.GetBattleState().Phase);
        }

        [Test]
        public void BattleManager_StartBattle_PublishesEvent()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);

            BattleStartEvent received = null;
            EventBus.Subscribe<BattleStartEvent>(e => received = e);

            manager.StartBattle();

            Assert.IsNotNull(received);
            Assert.AreEqual(1, received.Round);
        }

        [Test]
        public void BattleManager_AfterEndPlayerTurn_EntersNextRoundIntent()
        {
            var battle = CreateContext();
            AddEnemy(battle, "e1", 50, 10, 8);
            var manager = new BattleManager(battle);
            manager.StartBattle();
            manager.RevealIntents();
            manager.BeginPlayerTurn();

            manager.EndPlayerTurn();

            Assert.AreEqual(2, manager.Turns.Round);
            Assert.AreEqual(BattlePhase.EnemyIntent, manager.GetBattleState().Phase);
        }
    }
}
