using System.Collections.Generic;
using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>
    /// Creates skill assets for the Grey Raven squad with the values from docs 201-203.
    /// Used by tests and scene builders.
    /// </summary>
    public static class SkillAssetFactory
    {
        public static SkillData CreateSkill(string id, OrbColor color,
            IReadOnlyList<EffectData> tier1, IReadOnlyList<EffectData> tier2, IReadOnlyList<EffectData> tier3)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
#if UNITY_EDITOR
            skill.AssignIdentity(id, id + " Skill");
            skill.AssignEffects(1, tier1);
            skill.AssignEffects(2, tier2);
            skill.AssignEffects(3, tier3);
#endif
            return skill;
        }

        public static List<EffectData> Effects(params EffectData[] effects)
        {
            return new List<EffectData>(effects);
        }

        public static EffectData Damage(int amount) => new EffectData(EffectType.Damage, amount);
        public static EffectData Shield(int amount) => new EffectData(EffectType.Shield, amount);
        public static EffectData Heal(int amount) => new EffectData(EffectType.Heal, amount);
        public static EffectData Energy(int amount) => new EffectData(EffectType.Energy, amount);
        public static EffectData DrawOrb(int amount) => new EffectData(EffectType.DrawOrb, amount);
        public static EffectData CoreMark() => new EffectData(EffectType.CoreMark, 1);
        public static EffectData Infection(int amount) => new EffectData(EffectType.Infection, amount);

        // ---- Lucia (doc 201): red damage, blue energy, yellow shield ----
        public static SkillData LuciaRed() =>
            CreateSkill("lucia_red", OrbColor.Red,
                Effects(Damage(4)),
                Effects(Damage(9)),
                Effects(Damage(18), CoreMark()));

        public static SkillData LuciaBlue() =>
            CreateSkill("lucia_blue", OrbColor.Blue,
                Effects(Energy(2)),
                Effects(Energy(4)),
                Effects(Energy(6)));

        public static SkillData LuciaYellow() =>
            CreateSkill("lucia_yellow", OrbColor.Yellow,
                Effects(Shield(3)),
                Effects(Shield(6)),
                Effects(Shield(10)));

        // ---- Lee (doc 202): red damage, blue draw, yellow shield ----
        public static SkillData LeeRed() =>
            CreateSkill("lee_red", OrbColor.Red,
                Effects(Damage(5)),
                Effects(Damage(11)),
                Effects(Damage(22)));

        public static SkillData LeeBlue() =>
            CreateSkill("lee_blue", OrbColor.Blue,
                Effects(DrawOrb(1)),
                Effects(DrawOrb(2)),
                Effects(DrawOrb(3)));

        public static SkillData LeeYellow() =>
            CreateSkill("lee_yellow", OrbColor.Yellow,
                Effects(Shield(4)),
                Effects(Shield(8)),
                Effects(Shield(12)));

        // ---- Liv (doc 203): red damage, blue heal, yellow shield ----
        public static SkillData LivRed() =>
            CreateSkill("liv_red", OrbColor.Red,
                Effects(Damage(5)),
                Effects(Damage(10)),
                Effects(Damage(18)));

        public static SkillData LivBlue() =>
            CreateSkill("liv_blue", OrbColor.Blue,
                Effects(Heal(3)),
                Effects(Heal(6)),
                Effects(Heal(12)));

        public static SkillData LivYellow() =>
            CreateSkill("liv_yellow", OrbColor.Yellow,
                Effects(Shield(5)),
                Effects(Shield(10)),
                Effects(Shield(18)));
    }
}
