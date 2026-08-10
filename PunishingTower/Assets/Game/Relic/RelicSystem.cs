using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Core.Events;
using PunishingTower.Data;

namespace PunishingTower.Relic
{
    /// <summary>
    /// Applies relic effects by listening to battle events (doc 12 / 30).
    /// Currently supports the Grey Raven Badge: +1 energy on every three match.
    /// </summary>
    public class RelicSystem
    {
        private readonly List<RelicData> relics = new List<RelicData>();
        private SquadRuntime squad;
        private BattleContext battle;
        private bool battleStarted;

        public IReadOnlyList<RelicData> Relics => relics;

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

        public void AddRelic(RelicData relic)
        {
            if (relic != null)
            {
                relics.Add(relic);
            }
        }

        private void OnThreeMatch(ThreeMatchEvent evt)
        {
            foreach (RelicData relic in relics)
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
            foreach (RelicData relic in relics)
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
            foreach (RelicData relic in relics)
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
                    if (squad != null && squad.Current != null)
                    {
                        squad.Current.AddEnergy(relic.Amount);
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
