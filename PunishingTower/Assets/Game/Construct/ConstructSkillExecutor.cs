using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Data;
using PunishingTower.SignalOrb;

namespace PunishingTower.Construct
{
    /// <summary>
    /// Executes a construct's orb skill effects against the battle context (doc 214).
    /// Effect types are resolved generically - no per-character hard code.
    /// </summary>
    public static class ConstructSkillExecutor
    {
        /// <summary>
        /// Resolves every effect of the given skill tier.
        /// Damage targets the selected enemy; shield/heal/infection hit the commander;
        /// energy goes to the acting construct; draw orb pulls from the orb pool.
        /// Returns a human-readable description of what happened.
        /// </summary>
        public static string Execute(ConstructState actor, SkillData skill, int orbCount,
            BattleContext battle, OrbPool orbPool)
        {
            if (actor == null || skill == null || battle == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            IReadOnlyList<EffectData> effects = skill.GetEffects(orbCount);

            foreach (EffectData effect in effects)
            {
                parts.Add(ResolveEffect(actor, effect, battle, orbPool));
            }

            return string.Join(", ", parts);
        }

        private static string ResolveEffect(ConstructState actor, EffectData effect,
            BattleContext battle, OrbPool orbPool)
        {
            switch (effect.type)
            {
                case EffectType.Damage:
                {
                    EnemyState target = battle.SelectedEnemy;
                    if (target == null || target.IsDefeated)
                    {
                        return $"no target for {effect.amount} damage";
                    }
                    int amount = effect.amount;
                    if (actor.Data.CorePassive == CorePassiveType.ThreeMatchNextRedBonus && actor.ThreeMatchBonusPending)
                    {
                        amount += actor.Data.CorePassiveValue;
                        actor.ThreeMatchBonusPending = false;
                    }
                    int dealt = target.TakeDamage(amount);
                    return $"{target.DisplayName} -{dealt} HP";
                }

                case EffectType.Shield:
                {
                    battle.Commander.AddShield(effect.amount);
                    return $"commander +{effect.amount} shield";
                }

                case EffectType.Heal:
                {
                    battle.Commander.Heal(effect.amount);
                    return $"commander +{effect.amount} HP";
                }

                case EffectType.Energy:
                {
                    actor.AddEnergy(effect.amount);
                    return $"{actor.DisplayName} +{effect.amount} energy";
                }

                case EffectType.DrawOrb:
                {
                    if (orbPool == null)
                    {
                        return $"draw {effect.amount} orbs (no pool)";
                    }
                    int drawn = orbPool.Draw(effect.amount).Count;
                    return $"draw {drawn} orbs";
                }

                case EffectType.Infection:
                {
                    battle.Commander.AddInfection(effect.amount);
                    return $"commander +{effect.amount} infection";
                }

                case EffectType.CoreMark:
                {
                    actor.ThreeMatchBonusPending = true;
                    return "core mark charged";
                }

                case EffectType.Weaken:
                {
                    return "weaken applied";
                }

                default:
                    return $"unknown effect {effect.type}";
            }
        }
    }
}
