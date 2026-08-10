using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Core.Events;
using PunishingTower.Data;

namespace PunishingTower.Relic
{
    /// <summary>
    /// Applies relic effects by listening to battle events (doc 12 / 30).
    /// Every construct can equip multiple relics. A relic only takes effect while
    /// its owner construct is ACTIVE (in the squad); leaving the squad disables it.
    /// </summary>
    public class RelicSystem
    {
        private SquadRuntime squad;
        private BattleContext battle;
        private bool battleStarted;

        public void Initialize(SquadRuntime squadRef, BattleContext battleRef)
        {
            squad = squadRef;
            battle = battleRef;
            EventBus.Subscribe<ThreeMatchEvent>(OnThreeMatch);
            EventBus.Subscribe<TurnStartEvent>(OnTurnStart);
            EventBus.Subscribe<BattleStartEvent>(OnBattleStart);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<ThreeMatchEvent>(OnThreeMatch);
            EventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
            EventBus.Unsubscribe<BattleStartEvent>(OnBattleStart);
        }

        /// <summary>Equips a relic onto a construct. A relic can only be on one construct at a time.</summary>
        public bool EquipRelic(ConstructState owner, RelicData relic)
        {
            if (owner == null || relic == null)
            {
                return false;
            }

            // A relic can only be equipped once - unequip from any previous owner first.
            UnequipRelic(relic);

            return owner.EquipRelic(relic);
        }

        /// <summary>Removes a relic from whichever construct currently holds it.</summary>
        public bool UnequipRelic(RelicData relic)
        {
            if (relic == null || squad == null)
            {
                return false;
            }

            foreach (ConstructState member in squad.Members)
            {
                if (member.UnequipRelic(relic))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Moves a relic from its current owner to another construct.</summary>
        public bool SwitchRelicOwner(RelicData relic, ConstructState newOwner)
        {
            if (relic == null || newOwner == null)
            {
                return false;
            }
            return EquipRelic(newOwner, relic);
        }

        /// <summary>Returns the construct currently holding the relic, or null.</summary>
        public ConstructState GetRelicOwner(RelicData relic)
        {
            if (relic == null || squad == null)
            {
                return null;
            }
            foreach (ConstructState member in squad.Members)
            {
                if (member.HasEquippedRelic(relic))
                {
                    return member;
                }
            }
            return null;
        }

        /// <summary>
        /// All relics currently EFFECTIVE: equipped on a construct that is Active in the squad.
        /// Relics of constructs that left the squad (Unavailable/Recovering) do not apply.
        /// </summary>
        public List<RelicData> GetActiveRelics()
        {
            var result = new List<RelicData>();
            if (squad == null)
            {
                return result;
            }
            foreach (ConstructState member in squad.Members)
            {
                if (!member.IsActive)
                {
                    continue;
                }
                result.AddRange(member.EquippedRelics);
            }
            return result;
        }

        private void OnThreeMatch(ThreeMatchEvent evt)
        {
            foreach (RelicData relic in GetActiveRelics())
            {
                if (relic.Trigger != RelicTrigger.ThreeMatch)
                {
                    continue;
                }
                ApplyRelic(relic);
            }
        }

        private void OnTurnStart(TurnStartEvent evt)
        {
            foreach (RelicData relic in GetActiveRelics())
            {
                if (relic.Trigger != RelicTrigger.TurnStart)
                {
                    continue;
                }
                ApplyRelic(relic);
            }
        }

        private void OnBattleStart(BattleStartEvent evt)
        {
            if (battleStarted)
            {
                return;
            }
            battleStarted = true;
            foreach (RelicData relic in GetActiveRelics())
            {
                if (relic.Trigger != RelicTrigger.BattleStart)
                {
                    continue;
                }
                ApplyRelic(relic);
            }
        }

        private void ApplyRelic(RelicData relic)
        {
            switch (relic.EffectType)
            {
                case EffectType.Energy:
                    if (relic.TeamWide)
                    {
                        // Team wide: the currently acting construct gains energy.
                        if (squad != null && squad.Current != null)
                        {
                            squad.Current.AddEnergy(relic.Amount);
                        }
                    }
                    else
                    {
                        // Owner only: the equipped construct gains energy.
                        ConstructState owner = GetRelicOwner(relic);
                        if (owner != null)
                        {
                            owner.AddEnergy(relic.Amount);
                        }
                    }
                    break;
                case EffectType.Shield:
                    if (battle != null)
                    {
                        battle.Commander.AddShield(relic.Amount);
                    }
                    break;
                case EffectType.Heal:
                    if (battle != null)
                    {
                        battle.Commander.Heal(relic.Amount);
                    }
                    break;
            }
        }
    }
}
