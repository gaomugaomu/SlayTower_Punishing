using System.Collections.Generic;
using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>
    /// A skill tier: effects applied when an orb skill is used with 1, 2 or 3 orbs.
    /// </summary>
    [System.Serializable]
    public class SkillTier
    {
        [Tooltip("Effects executed when this tier is triggered.")]
        public List<EffectData> effects = new List<EffectData>();
    }

    /// <summary>
    /// Orb skill definition (doc 44): color + per-tier effect lists.
    /// Index 0 = 1 orb, 1 = 2 orbs, 2 = 3 orbs (three match).
    /// </summary>
    [CreateAssetMenu(fileName = "Skill_", menuName = "PunishingTower/Data/Skill")]
    public class SkillData : GameDataObject
    {
        [SerializeField] private OrbColor color;
        [SerializeField] private SkillTier tier1 = new SkillTier();
        [SerializeField] private SkillTier tier2 = new SkillTier();
        [SerializeField] private SkillTier tier3 = new SkillTier();

        public OrbColor Color => color;

        public IReadOnlyList<EffectData> GetEffects(int orbCount)
        {
            switch (orbCount)
            {
                case 1: return tier1.effects;
                case 2: return tier2.effects;
                case 3: return tier3.effects;
                default: return tier3.effects;
            }
        }

#if UNITY_EDITOR
        public void AssignSkill(OrbColor skillColor, SkillTier t1, SkillTier t2, SkillTier t3)
        {
            color = skillColor;
            tier1 = t1 ?? new SkillTier();
            tier2 = t2 ?? new SkillTier();
            tier3 = t3 ?? new SkillTier();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void AssignEffects(int orbCount, IEnumerable<EffectData> effects)
        {
            SkillTier tier = orbCount == 1 ? tier1 : (orbCount == 2 ? tier2 : tier3);
            tier.effects.Clear();
            if (effects != null)
            {
                tier.effects.AddRange(effects);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
