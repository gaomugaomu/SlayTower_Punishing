using PunishingTower.Core;
using PunishingTower.Data;

namespace PunishingTower.Construct
{
    /// <summary>Combat availability of a construct (doc 06). No HP - only the commander has HP.</summary>
    public enum ConstructStateFlag
    {
        Active,
        Unavailable,
        Recovering
    }

    /// <summary>
    /// Runtime construct: holds static data reference plus independent ultimate energy
    /// and combat availability. Constructs never have HP.
    /// </summary>
    public class ConstructState : ICombatActor
    {
        public ConstructData Data { get; }

        public string Id => Data != null ? Data.Id : string.Empty;
        public string DisplayName => Data != null ? Data.DisplayName : string.Empty;

        public int Energy { get; private set; }
        public int EnergyMax => Data != null ? Data.EnergyMax : 100;
        public bool IsEnergyFull => Energy >= EnergyMax;

        public ConstructStateFlag Flag { get; private set; } = ConstructStateFlag.Active;
        public bool IsActive => Flag == ConstructStateFlag.Active;

        public ConstructState(ConstructData data)
        {
            Data = data;
            Energy = data != null ? System.Math.Min(data.InitialEnergy, EnergyMax) : 0;
        }

        public void AddEnergy(int amount)
        {
            if (amount > 0)
            {
                Energy = System.Math.Min(EnergyMax, Energy + amount);
            }
        }

        /// <summary>Resets energy to zero and returns true when it was full (ultimate used).</summary>
        public bool TryConsumeUltimateEnergy()
        {
            if (!IsEnergyFull)
            {
                return false;
            }
            Energy = 0;
            return true;
        }

        public void SetFlag(ConstructStateFlag flag)
        {
            Flag = flag;
        }

        public int BasicAttackDamage => Data != null ? Data.BasicAttackDamage : 0;
        public int BasicAttackEnergyGain => Data != null ? Data.BasicAttackEnergyGain : 1;
        public int UltimateDamage => Data != null ? Data.UltimateDamage : 0;
    }
}
