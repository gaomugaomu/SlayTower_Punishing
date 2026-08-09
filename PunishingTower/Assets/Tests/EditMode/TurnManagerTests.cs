using NUnit.Framework;
using PunishingTower.Combat;

namespace PunishingTower.Tests
{
    public class TurnManagerTests
    {
        private TurnManager turns;

        [SetUp]
        public void SetUp()
        {
            turns = new TurnManager();
        }

        [Test]
        public void TurnManager_InitialState_BattleStartRoundOne()
        {
            Assert.AreEqual(TurnPhase.BattleStart, turns.Phase);
            Assert.AreEqual(1, turns.Round);
        }

        [Test]
        public void TurnManager_BeginBattle_GoesToIntentPhase()
        {
            turns.BeginBattle();

            Assert.AreEqual(TurnPhase.EnemyIntentPhase, turns.Phase);
        }

        [Test]
        public void TurnManager_RevealIntents_GoesToPlayerTurnStart()
        {
            turns.BeginBattle();
            turns.RevealIntents();

            Assert.AreEqual(TurnPhase.PlayerTurnStart, turns.Phase);
        }

        [Test]
        public void TurnManager_BeginPlayerTurn_GoesToPlayerAction()
        {
            turns.BeginBattle();
            turns.RevealIntents();
            turns.BeginPlayerTurn();

            Assert.AreEqual(TurnPhase.PlayerAction, turns.Phase);
        }

        [Test]
        public void TurnManager_FullTurnCycle_IncrementsRound()
        {
            turns.BeginBattle();
            turns.RevealIntents();
            turns.BeginPlayerTurn();
            turns.EndPlayerActions();
            turns.BeginEnemyTurn();
            turns.EndEnemyTurn();
            turns.AdvanceRound();
            turns.StartNextRound();

            Assert.AreEqual(TurnPhase.EnemyIntentPhase, turns.Phase);
            Assert.AreEqual(2, turns.Round);
        }

        [Test]
        public void TurnManager_InvalidTransition_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() => turns.BeginPlayerTurn());
        }
    }
}
