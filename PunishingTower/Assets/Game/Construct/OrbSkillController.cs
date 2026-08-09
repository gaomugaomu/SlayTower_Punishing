using PunishingTower.Combat;
using PunishingTower.Core.Events;
using PunishingTower.Data;
using PunishingTower.SignalOrb;

namespace PunishingTower.Construct
{
    /// <summary>
    /// Bridges the orb row to construct skills:
    /// resolving a play group (1/2/3 match) spends 1 AP, uses the construct's skill
    /// for the orb color, publishes ThreeMatchEvent on a 3-match and discards the orbs.
    /// </summary>
    public class OrbSkillController
    {
        private readonly OrbPool orbPool;

        public OrbSkillController(OrbPool orbPool)
        {
            this.orbPool = orbPool;
        }

        /// <summary>
        /// Plays the given group with the acting construct. Returns a description,
        /// or null when the play was rejected (no AP / no skill / no target).
        /// </summary>
        public string TryPlayGroup(OrbPlayGroup group, ConstructState actor,
            BattleContext battle, ActionPointSystem ap)
        {
            if (group == null || actor == null || battle == null || ap == null)
            {
                return null;
            }
            if (!ap.CanSpend(1))
            {
                return null;
            }
            if (actor.Data == null || !actor.IsActive)
            {
                return null;
            }

            SkillData skill = actor.Data.GetSkill((OrbColor)group.Color);
            if (skill == null)
            {
                return null;
            }

            ap.TrySpend(1);

            if (group.MatchCount >= 3)
            {
                EventBus.Publish(new ThreeMatchEvent(group.Color, group.MatchCount));
            }

            string effectText = ConstructSkillExecutor.Execute(actor, skill, group.MatchCount, battle, orbPool);

            foreach (var orb in group.Orbs)
            {
                if (orb is OrbInstance instance)
                {
                    orbPool.DiscardOrb(instance);
                }
            }

            return effectText;
        }
    }
}
