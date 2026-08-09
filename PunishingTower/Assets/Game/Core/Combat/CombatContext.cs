namespace PunishingTower.Core
{
    /// <summary>
    /// Snapshot of overall battle state used by IBattleManager.GetBattleState.
    /// </summary>
    public enum BattlePhase
    {
        NotStarted,
        EnemyIntent,
        PlayerTurn,
        EnemyAction,
        Victory,
        Defeat
    }

    public sealed class BattleState
    {
        public BattlePhase Phase { get; set; }
        public int Round { get; set; }
        public bool IsActive { get; set; }
        public bool IsOver { get; set; }

        public BattleState()
        {
            Phase = BattlePhase.NotStarted;
            Round = 0;
            IsActive = false;
            IsOver = false;
        }
    }

    /// <summary>
    /// Context passed to skills and relic effects containing actors and services.
    /// Kept minimal and serializable for testability.
    /// </summary>
    public class CombatContext
    {
        public ICombatActor Source { get; set; }
        public ICombatActor Target { get; set; }
    }

    /// <summary>
    /// Context passed to an IEffect when it is resolved.
    /// </summary>
    public class EffectContext : CombatContext
    {
        public int Amount { get; set; }
        public int Value { get; set; }
    }
}
