using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>What triggers a relic effect.</summary>
    public enum RelicTrigger
    {
        /// <summary>Fires whenever a three match occurs.</summary>
        ThreeMatch,

        /// <summary>Fires at the start of every player turn.</summary>
        TurnStart,

        /// <summary>Fires once when the battle begins.</summary>
        BattleStart
    }

    /// <summary>
    /// Static relic definition (doc 12). Effects are data driven; the relic system
    /// listens to battle events and applies the configured effect.
    /// </summary>
    [CreateAssetMenu(fileName = "Relic_", menuName = "PunishingTower/Data/Relic")]
    public class RelicData : GameDataObject
    {
        [SerializeField] private RelicTrigger trigger = RelicTrigger.ThreeMatch;
        [SerializeField] private EffectType effectType = EffectType.Energy;
        [SerializeField] private int amount = 1;
        [SerializeField, TextArea] private string description;

        public RelicTrigger Trigger => trigger;
        public EffectType EffectType => effectType;
        public int Amount => amount;
        public string Description => description;

#if UNITY_EDITOR
        public void AssignRelic(RelicTrigger newTrigger, EffectType newEffect, int newAmount, string newDescription)
        {
            trigger = newTrigger;
            effectType = newEffect;
            amount = newAmount;
            description = newDescription;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
