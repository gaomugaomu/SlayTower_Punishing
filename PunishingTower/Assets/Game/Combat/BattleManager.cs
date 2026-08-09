using PunishingTower.Core;
using PunishingTower.Core.Events;

namespace PunishingTower.Combat
{
    /// <summary>
    /// Battle lifecycle controller (doc 23): initializes the battle, drives the turn flow,
    /// coordinates the systems and determines victory/defeat.
    /// Must NOT contain character skill logic, damage formulas or UI code.
    /// Enemy turn execution is injected from the enemy layer.
    /// </summary>
    public class BattleManager : IBattleManager
    {
        public BattleContext Battle { get; }
        public TurnManager Turns { get; }
        public ActionPointSystem ActionPoints { get; }

        private readonly System.Action<BattleContext> enemyTurnExecutor;
        private readonly BattleState state = new BattleState();

        public bool IsOver => state.IsOver;

        public BattleManager(BattleContext battle, int actionPointsPerTurn = 3)
            : this(battle, null, actionPointsPerTurn)
        {
        }

        public BattleManager(BattleContext battle, System.Action<BattleContext> enemyTurnExecutor, int actionPointsPerTurn = 3)
        {
            Battle = battle;
            this.enemyTurnExecutor = enemyTurnExecutor;
            Turns = new TurnManager();
            ActionPoints = new ActionPointSystem(actionPointsPerTurn);
        }

        public void StartBattle()
        {
            state.Phase = BattlePhase.EnemyIntent;
            state.Round = 1;
            state.IsActive = true;
            state.IsOver = false;

            Turns.BeginBattle();
            EventBus.Publish(new BattleStartEvent(Turns.Round));
        }

        /// <summary>EnemyIntentPhase -&gt; PlayerTurnStart: reveal intents, reset action points.</summary>
        public void RevealIntents()
        {
            if (state.IsOver)
            {
                return;
            }

            Turns.RevealIntents();
            ActionPoints.BeginTurn();
            EventBus.Publish(new TurnStartEvent(Turns.Round));
        }

        /// <summary>PlayerTurnStart -&gt; PlayerAction.</summary>
        public void BeginPlayerTurn()
        {
            if (state.IsOver)
            {
                return;
            }

            Turns.BeginPlayerTurn();
            state.Phase = BattlePhase.PlayerTurn;
        }

        /// <summary>
        /// Ends the player turn: PlayerAction -&gt; PlayerEnd -&gt; EnemyAction -&gt; TurnEnd.
        /// Executes the injected enemy turn, then advances to the next round
        /// unless the battle is over.
        /// </summary>
        public void EndPlayerTurn()
        {
            if (state.IsOver)
            {
                return;
            }

            Turns.EndPlayerActions();
            Turns.BeginEnemyTurn();
            state.Phase = BattlePhase.EnemyAction;
            ActionPoints.EndTurn();

            if (enemyTurnExecutor != null)
            {
                enemyTurnExecutor.Invoke(Battle);
            }

            Turns.EndEnemyTurn();
            Turns.AdvanceRound();
            Turns.StartNextRound();
            state.Round = Turns.Round;

            EventBus.Publish(new TurnEndEvent(Turns.Round));

            if (Battle.AllEnemiesDefeated)
            {
                EndBattle(true);
                return;
            }
            if (Battle.Commander.IsDefeated)
            {
                EndBattle(false);
                return;
            }

            state.Phase = BattlePhase.EnemyIntent;
        }

        public void EndBattle(bool victory)
        {
            state.IsOver = true;
            state.IsActive = false;
            state.Phase = victory ? BattlePhase.Victory : BattlePhase.Defeat;
            EventBus.Publish(new BattleEndEvent(victory));
        }

        public BattleState GetBattleState()
        {
            return state;
        }
    }
}
