using PunishingTower.Core;
using PunishingTower.Data;

namespace PunishingTower.SignalOrb
{
    /// <summary>
    /// Runtime signal orb instance. Static definition lives in OrbData.
    /// </summary>
    public class OrbInstance : ISignalOrb
    {
        public OrbData Data { get; }

        /// <summary>Locked orbs cannot participate in three match and cannot be played.</summary>
        public bool Locked { get; set; }

        /// <summary>When true the orb goes to the exhaust pile after use instead of discard.</summary>
        public bool ExhaustOnUse { get; set; }

        /// <summary>When true the orb stays in hand after being played.</summary>
        public bool Retained { get; set; }

        public string Id => Data != null ? Data.Id : string.Empty;
        public int Color => Data != null ? (int)Data.Color : 0;

        public OrbInstance(OrbData data)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $"{Id}[{Data.Color}] locked={Locked} exhaust={ExhaustOnUse} retain={Retained}";
        }
    }
}
