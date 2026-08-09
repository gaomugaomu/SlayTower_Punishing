namespace PunishingTower.Combat
{
    /// <summary>Turn phases per doc 24. Each phase is independently testable.</summary>
    public enum TurnPhase
    {
        BattleStart,
        EnemyIntentPhase,
        PlayerTurnStart,
        PlayerAction,
        PlayerEnd,
        EnemyAction,
        TurnEnd,
        NextTurn
    }

    /// <summary>
    /// Turn state machine (doc 24):
    /// BattleStart -&gt; EnemyIntentPhase -&gt; PlayerTurnStart -&gt; PlayerAction -&gt; PlayerEnd
    /// -&gt; EnemyAction -&gt; TurnEnd -&gt; NextTurn -&gt; (EnemyIntentPhase of next round)
    /// </summary>
    public class TurnManager
    {
        public TurnPhase Phase { get; private set; }
        public int Round { get; private set; }

        public TurnManager()
        {
            Phase = TurnPhase.BattleStart;
            Round = 1;
        }

        public void BeginBattle()
        {
            Round = 1;
            Phase = TurnPhase.EnemyIntentPhase;
        }

        /// <summary>EnemyIntentPhase -&gt; PlayerTurnStart: intents have been revealed.</summary>
        public void RevealIntents()
        {
            RequirePhase(TurnPhase.EnemyIntentPhase);
            Phase = TurnPhase.PlayerTurnStart;
        }

        /// <summary>PlayerTurnStart -&gt; PlayerAction: player begins acting.</summary>
        public void BeginPlayerTurn()
        {
            RequirePhase(TurnPhase.PlayerTurnStart);
            Phase = TurnPhase.PlayerAction;
        }

        /// <summary>PlayerAction -&gt; PlayerEnd: player finished their actions.</summary>
        public void EndPlayerActions()
        {
            RequirePhase(TurnPhase.PlayerAction);
            Phase = TurnPhase.PlayerEnd;
        }

        /// <summary>PlayerEnd -&gt; EnemyAction: enemies execute.</summary>
        public void BeginEnemyTurn()
        {
            RequirePhase(TurnPhase.PlayerEnd);
            Phase = TurnPhase.EnemyAction;
        }

        /// <summary>EnemyAction -&gt; TurnEnd: enemies finished.</summary>
        public void EndEnemyTurn()
        {
            RequirePhase(TurnPhase.EnemyAction);
            Phase = TurnPhase.TurnEnd;
        }

        /// <summary>TurnEnd -&gt; NextTurn: advances the round counter.</summary>
        public void AdvanceRound()
        {
            RequirePhase(TurnPhase.TurnEnd);
            Round++;
            Phase = TurnPhase.NextTurn;
        }

        /// <summary>NextTurn -&gt; EnemyIntentPhase: a new round begins.</summary>
        public void StartNextRound()
        {
            RequirePhase(TurnPhase.NextTurn);
            Phase = TurnPhase.EnemyIntentPhase;
        }

        private void RequirePhase(TurnPhase expected)
        {
            if (Phase != expected)
            {
                throw new System.InvalidOperationException($"TurnManager: expected phase {expected} but was {Phase}");
            }
        }
    }
}
