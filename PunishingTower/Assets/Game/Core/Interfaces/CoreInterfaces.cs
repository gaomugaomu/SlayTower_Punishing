using PunishingTower.Core.Events;

namespace PunishingTower.Core
{
    /// <summary>
    /// Any entity participating in combat (constructs, enemies, commander).
    /// </summary>
    public interface ICombatActor
    {
        string Id { get; }
        string DisplayName { get; }
    }

    /// <summary>
    /// A usable skill backed by data-driven effects.
    /// </summary>
    public interface ISkill
    {
        string Id { get; }
        bool CanExecute(CombatContext context);
        void Execute(CombatContext context);
    }

    /// <summary>
    /// A single reusable gameplay effect resolved through the EffectSystem.
    /// </summary>
    public interface IEffect
    {
        void Apply(EffectContext context);
    }

    /// <summary>
    /// Runtime signal orb instance. Data is separate from runtime state.
    /// </summary>
    public interface ISignalOrb
    {
        string Id { get; }
        int Color { get; }
    }

    /// <summary>
    /// A temporary buff/debuff with duration and stack count.
    /// </summary>
    public interface IStatusEffect
    {
        string Id { get; }
        int Duration { get; }
        int Stacks { get; }
    }

    /// <summary>
    /// Objects that react to events (relics, passives, boss rules).
    /// </summary>
    public interface ITriggerListener
    {
        void OnGameEvent(IGameEvent evt);
    }

    /// <summary>
    /// Behaviour contributed by a relic.
    /// </summary>
    public interface IRelicEffect
    {
        string Id { get; }
        void Apply(CombatContext context);
    }

    /// <summary>
    /// Battle lifecycle controller. Must not contain skill logic, damage formulas or UI code.
    /// </summary>
    public interface IBattleManager
    {
        void StartBattle();
        void EndBattle(bool victory);
        BattleState GetBattleState();
    }
}
