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

    /// <summary>
    /// Static definition of a construct. Constructs have NO HP - only the commander has HP.
    /// Each construct owns independent ultimate energy and a damage-based ultimate for now.
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
        [SerializeField] private int ultimateDamage = 40;

        public ConstructType ConstructType => constructType;
        public int BasicAttackDamage => basicAttackDamage;
        public int BasicAttackEnergyGain => basicAttackEnergyGain;
        public int EnergyMax => energyMax;
        public int UltimateDamage => ultimateDamage;

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
#endif
    }
}
