using PunishingTower.Core;

namespace PunishingTower.Combat
{
    /// <summary>
    /// Context for resolving a battle effect. Holds commander, target enemy and the effect amount.
    /// </summary>
    public class BattleEffectContext : EffectContext
    {
        public CommanderState Commander { get; set; }
        public EnemyState TargetEnemy { get; set; }

        public bool TargetsEnemy => TargetEnemy != null;
    }

    /// <summary>Damage to the target (enemy or commander).</summary>
    public sealed class DamageEffect : IEffect
    {
        public int Amount { get; }

        public DamageEffect(int amount)
        {
            Amount = amount;
        }

        public void Apply(EffectContext context)
        {
            var ctx = context as BattleEffectContext;
            if (ctx == null)
            {
                return;
            }

            if (ctx.TargetsEnemy)
            {
                ctx.TargetEnemy.TakeDamage(Amount);
            }
            else
            {
                ctx.Commander.TakeDamage(Amount);
            }
        }
    }

    /// <summary>Heals the commander (constructs have no HP).</summary>
    public sealed class HealEffect : IEffect
    {
        public int Amount { get; }

        public HealEffect(int amount)
        {
            Amount = amount;
        }

        public void Apply(EffectContext context)
        {
            var ctx = context as BattleEffectContext;
            if (ctx == null)
            {
                return;
            }
            ctx.Commander.Heal(Amount);
        }
    }

    /// <summary>Adds shield to the commander.</summary>
    public sealed class ShieldEffect : IEffect
    {
        public int Amount { get; }

        public ShieldEffect(int amount)
        {
            Amount = amount;
        }

        public void Apply(EffectContext context)
        {
            var ctx = context as BattleEffectContext;
            if (ctx == null)
            {
                return;
            }
            ctx.Commander.AddShield(Amount);
        }
    }

    /// <summary>Raises the commander infection value.</summary>
    public sealed class InfectionEffect : IEffect
    {
        public int Amount { get; }

        public InfectionEffect(int amount)
        {
            Amount = amount;
        }

        public void Apply(EffectContext context)
        {
            var ctx = context as BattleEffectContext;
            if (ctx == null)
            {
                return;
            }
            ctx.Commander.AddInfection(Amount);
        }
    }

    /// <summary>Resolves a list of effects against the battle context.</summary>
    public static class EffectResolver
    {
        public static void Resolve(System.Collections.Generic.IEnumerable<IEffect> effects, BattleEffectContext context)
        {
            if (effects == null || context == null)
            {
                return;
            }

            foreach (IEffect effect in effects)
            {
                effect.Apply(context);
            }
        }
    }
}
