using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>Construct combat role.</summary>
    public enum ConstructType
    {
        Attack,
        Tank,
        Support
    }

    /// <summary>Core passive behaviours supported by the skill system (doc 06).</summary>
    public enum CorePassiveType
    {
        None = 0,

        /// <summary>After a three match, the next red orb activation deals bonus damage (Lucia).</summary>
        ThreeMatchNextRedBonus,

        /// <summary>After using a yellow orb, the next attack gains a bonus effect (Lee).</summary>
        YellowNextAttackBonus,

        /// <summary>Improves the first support action each battle (Liv).</summary>
        FirstSupportImproved
    }

    /// <summary>
    /// Static definition of a construct. Constructs have NO HP - only the commander has HP.
    /// Each construct owns independent ultimate energy, orb skills and a core passive.
    /// </summary>
    [CreateAssetMenu(fileName = "Construct_", menuName = "PunishingTower/Data/Construct")]
    public class ConstructData : GameDataObject
    {
        [Header("Identity")]
        [SerializeField] private ConstructType constructType;

        [Header("Basic Attack")]
        [SerializeField] private int basicAttackDamage = 6;
        [SerializeField] private int basicAttackEnergyGain = 1;

        [Header("Ultimate")]
        [SerializeField] private int energyMax = 100;
        [SerializeField] private int initialEnergy = 100;
        [SerializeField] private int ultimateDamage = 40;

        [Header("Orb Skills (doc 44)")]
        [SerializeField] private SkillData redSkill;
        [SerializeField] private SkillData blueSkill;
        [SerializeField] private SkillData yellowSkill;

        [Header("Passive")]
        [SerializeField] private CorePassiveType corePassive = CorePassiveType.None;
        [SerializeField] private int corePassiveValue;

        public ConstructType ConstructType => constructType;
        public int BasicAttackDamage => basicAttackDamage;
        public int BasicAttackEnergyGain => basicAttackEnergyGain;
        public int EnergyMax => energyMax;
        public int InitialEnergy => initialEnergy;
        public int UltimateDamage => ultimateDamage;

        public SkillData RedSkill => redSkill;
        public SkillData BlueSkill => blueSkill;
        public SkillData YellowSkill => yellowSkill;

        public CorePassiveType CorePassive => corePassive;
        public int CorePassiveValue => corePassiveValue;

        public SkillData GetSkill(OrbColor color)
        {
            switch (color)
            {
                case OrbColor.Red: return redSkill;
                case OrbColor.Blue: return blueSkill;
                case OrbColor.Yellow: return yellowSkill;
                default: return null;
            }
        }

#if UNITY_EDITOR
        public void AssignCombatStats(ConstructType type, int attackDamage, int energyGain, int maxEnergy, int ultDamage)
        {
            constructType = type;
            basicAttackDamage = attackDamage;
            basicAttackEnergyGain = energyGain;
            energyMax = maxEnergy;
            ultimateDamage = ultDamage;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void AssignInitialEnergy(int value)
        {
            initialEnergy = value;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void AssignSkills(SkillData red, SkillData blue, SkillData yellow)
        {
            redSkill = red;
            blueSkill = blue;
            yellowSkill = yellow;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void AssignCorePassive(CorePassiveType passive, int value)
        {
            corePassive = passive;
            corePassiveValue = value;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
